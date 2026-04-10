using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Storyline;
using NavalDLC.View.MissionViews;
using SandBox.Objects.AreaMarkers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Hints;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(FloatingFortressView))]
public class MissionGauntletFloatingFortressView : FloatingFortressView
{
	private abstract class Keyframe<T>
	{
		public float Time { get; set; }

		public T Value { get; set; }

		public Keyframe(float time, T value)
		{
			Time = time;
			Value = value;
		}
	}

	private abstract class Track<TKeyframe, TValue> where TKeyframe : Keyframe<TValue>
	{
		protected readonly List<TKeyframe> Keyframes = new List<TKeyframe>();

		private int _lastKeyframeIndex;

		public void AddKeyframe(TKeyframe keyframe)
		{
			Keyframes.Add(keyframe);
			Keyframes.Sort((TKeyframe a, TKeyframe b) => a.Time.CompareTo(b.Time));
		}

		public void RemoveKeyframe(TKeyframe keyframe)
		{
			Keyframes.Remove(keyframe);
		}

		public void ClearKeyframes()
		{
			Keyframes.Clear();
			_lastKeyframeIndex = 0;
		}

		public bool IsCompleted(float time)
		{
			if (Keyframes.Count != 0)
			{
				return Keyframes.Last().Time <= time;
			}
			return true;
		}

		public abstract TValue Evaluate(float time);

		protected (TKeyframe prev, TKeyframe next, float t) GetKeyframesAtTime(float time)
		{
			if (Keyframes.Count == 0)
			{
				return (prev: null, next: null, t: 0f);
			}
			if (time <= Keyframes[0].Time)
			{
				return (prev: Keyframes[0], next: Keyframes[0], t: 0f);
			}
			if (time >= Keyframes[Keyframes.Count - 1].Time)
			{
				return (prev: Keyframes[Keyframes.Count - 1], next: Keyframes[Keyframes.Count - 1], t: 1f);
			}
			int num = Math.Max(0, Math.Min(_lastKeyframeIndex, Keyframes.Count - 2));
			if (Keyframes[num].Time > time)
			{
				for (int num2 = num; num2 >= 0; num2--)
				{
					if (Keyframes[num2].Time <= time && Keyframes[num2 + 1].Time > time)
					{
						_lastKeyframeIndex = num2;
						float item = (time - Keyframes[num2].Time) / (Keyframes[num2 + 1].Time - Keyframes[num2].Time);
						return (prev: Keyframes[num2], next: Keyframes[num2 + 1], t: item);
					}
				}
			}
			else
			{
				for (int i = num; i < Keyframes.Count - 1; i++)
				{
					if (Keyframes[i].Time <= time && Keyframes[i + 1].Time > time)
					{
						_lastKeyframeIndex = i;
						float item2 = (time - Keyframes[i].Time) / (Keyframes[i + 1].Time - Keyframes[i].Time);
						return (prev: Keyframes[i], next: Keyframes[i + 1], t: item2);
					}
				}
			}
			return (prev: Keyframes[0], next: Keyframes[0], t: 0f);
		}
	}

	private class MatrixFrameKeyFrame : Keyframe<MatrixFrame>
	{
		public MatrixFrameKeyFrame(float time, MatrixFrame value)
			: base(time, value)
		{
		}//IL_0002: Unknown result type (might be due to invalid IL or missing references)

	}

	private class MatrixFrameTrack : Track<MatrixFrameKeyFrame, MatrixFrame>
	{
		public override MatrixFrame Evaluate(float time)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			var (matrixFrameKeyFrame, matrixFrameKeyFrame2, num) = GetKeyframesAtTime(time);
			if (matrixFrameKeyFrame == null || matrixFrameKeyFrame2 == null)
			{
				return MatrixFrame.Zero;
			}
			if (matrixFrameKeyFrame == matrixFrameKeyFrame2)
			{
				return matrixFrameKeyFrame.Value;
			}
			MatrixFrame value = matrixFrameKeyFrame.Value;
			MatrixFrame value2 = matrixFrameKeyFrame2.Value;
			return MatrixFrame.Lerp(ref value, ref value2, num * num * (3f - 2f * num));
		}
	}

	private class EventKeyframe : Keyframe<Action>
	{
		public EventKeyframe(float time, Action value)
			: base(time, value)
		{
		}
	}

	private class EventTrack : Track<EventKeyframe, Action>
	{
		private readonly HashSet<EventKeyframe> _triggeredEvents = new HashSet<EventKeyframe>();

		private float _lastEvaluatedTime = -0f;

		public override Action Evaluate(float time)
		{
			if (time < _lastEvaluatedTime)
			{
				_triggeredEvents.RemoveWhere((EventKeyframe e) => e.Time > time);
			}
			_lastEvaluatedTime = time;
			foreach (EventKeyframe keyframe in Keyframes)
			{
				if (keyframe.Time <= time && _triggeredEvents.Add(keyframe))
				{
					keyframe.Value?.Invoke();
				}
			}
			return null;
		}
	}

	private enum FadeOutReason
	{
		Initialize,
		BallistaCinematicEnded,
		PhaseOneCompleted
	}

	private const float EarliestSkipTime = 2.5f;

	private const float FadeOutTransitionTime = 1.5f;

	private readonly Dictionary<DestructableComponent, AnimatedBasicAreaIndicator> _markerByBallista = new Dictionary<DestructableComponent, AnimatedBasicAreaIndicator>();

	private bool _canInvokeFadeOutEvent = true;

	private FadeOutReason _fadeOutReason;

	private float _initialFadeOutWaitTime = 2f;

	private bool _isInitialized;

	private bool _isPhaseOneCompleted;

	private bool _isShowingBallistaHint;

	private bool _hasUsedBallista;

	private bool _willFadeOutForPhaseOneCompletion;

	private float _remainingTimeForPhaseOneFadeOut = 1.5f;

	private Camera _cinematicCamera;

	private bool _shouldTickCinematic;

	private float _cinematicElapsedTime;

	private MatrixFrameTrack _cinematicCameraTrack;

	private EventTrack _cinematicEventTrack;

	private FloatingFortressSetPieceBattleMissionController _controller;

	private MissionHintLogic _hintLogic;

	private NavalShipsLogic _navalShipsLogic;

	private MissionMainAgentController _missionMainAgentController;

	private MissionGauntletShipControlView _shipControlView;

	private MissionGauntletShipControlView.ShipControlFeatureFlags _suspendedFeatures;

	public bool AreMarkersDirty { get; private set; }

	public override void OnMissionScreenTick(float dt)
	{
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Expected O, but got Unknown
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Expected O, but got Unknown
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Expected O, but got Unknown
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenTick(dt);
		if (!_isInitialized)
		{
			InitializeView();
			_fadeOutReason = FadeOutReason.Initialize;
			_canInvokeFadeOutEvent = true;
			ScreenFadeController.BeginFadeOut(0f);
			_isInitialized = true;
		}
		if (!Mission.Current.Scene.IsLoadingFinished())
		{
			return;
		}
		Agent main = Agent.Main;
		MissionShip missionShip = ((main != null) ? main.GetComponent<AgentNavalComponent>().FormationShip : null);
		int hasUsedBallista;
		if (!_hasUsedBallista)
		{
			if (missionShip == null)
			{
				hasUsedBallista = 0;
			}
			else
			{
				RangedSiegeWeapon shipSiegeWeapon = missionShip.ShipSiegeWeapon;
				hasUsedBallista = ((((shipSiegeWeapon != null) ? new bool?(shipSiegeWeapon.PlayerForceUse) : ((bool?)null)) == true) ? 1 : 0);
			}
		}
		else
		{
			hasUsedBallista = 1;
		}
		_hasUsedBallista = (byte)hasUsedBallista != 0;
		if (_isShowingBallistaHint && _hintLogic.ActiveHint != null && (_hasUsedBallista || _navalShipsLogic.PlayerControlledShip == null))
		{
			_isShowingBallistaHint = false;
			_hintLogic.Clear();
		}
		if (_initialFadeOutWaitTime > 0f)
		{
			_initialFadeOutWaitTime -= dt;
			return;
		}
		if (_controller.IsPhaseOneCompleted && !_isPhaseOneCompleted)
		{
			_isPhaseOneCompleted = true;
			OnPhaseOneCompleted();
		}
		if (_willFadeOutForPhaseOneCompletion)
		{
			_remainingTimeForPhaseOneFadeOut -= dt;
			if (_remainingTimeForPhaseOneFadeOut <= 0f)
			{
				_fadeOutReason = FadeOutReason.PhaseOneCompleted;
				ScreenFadeController.BeginFadeOutAndIn(0.1f, 0.75f, 0.75f);
				_canInvokeFadeOutEvent = true;
				_willFadeOutForPhaseOneCompletion = false;
			}
		}
		WeakGameEntity val;
		WeakGameEntity gameEntity;
		foreach (MissionShip item in (List<MissionShip>)(object)_controller.EnemyShipsOrdered)
		{
			if (item.ShipSiegeWeapon != null)
			{
				RangedSiegeWeapon shipSiegeWeapon2 = item.ShipSiegeWeapon;
				if (!((UsableMachine)shipSiegeWeapon2).IsDestroyed && !_markerByBallista.ContainsKey(((UsableMachine)shipSiegeWeapon2).DestructionComponent))
				{
					((UsableMachine)shipSiegeWeapon2).DestructionComponent.OnDestroyed += new OnHitTakenAndDestroyedDelegate(OnBallistaDestroyed);
					GameEntity obj = GameEntity.CreateEmpty(((MissionBehavior)this).Mission.Scene, true, true, true);
					val = obj.WeakEntity;
					gameEntity = ((ScriptComponentBehavior)shipSiegeWeapon2).GameEntity;
					((WeakGameEntity)(ref val)).SetGlobalPosition(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
					AnimatedBasicAreaIndicator value = AddMarker(obj.WeakEntity, new TextObject("{=cn28TEkM}Target", (Dictionary<string, object>)null), "quest", 1.5f);
					_markerByBallista.Add(((UsableMachine)shipSiegeWeapon2).DestructionComponent, value);
					AreMarkersDirty = true;
				}
			}
		}
		foreach (KeyValuePair<DestructableComponent, AnimatedBasicAreaIndicator> markerByBallistum in _markerByBallista)
		{
			val = ((ScriptComponentBehavior)markerByBallistum.Value).GameEntity;
			gameEntity = ((ScriptComponentBehavior)markerByBallistum.Key).GameEntity;
			((WeakGameEntity)(ref val)).SetGlobalPosition(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
		}
		if (ScreenFadeController.IsFadedOut && _canInvokeFadeOutEvent)
		{
			if (_controller.IsStartedFromCheckpoint)
			{
				_fadeOutReason = FadeOutReason.PhaseOneCompleted;
				ScreenFadeController.BeginFadeIn(1f);
			}
			if (_fadeOutReason == FadeOutReason.Initialize)
			{
				_cinematicCamera = Camera.CreateCamera();
				_cinematicCamera.SetFovHorizontal(((MissionView)this).MissionScreen.CombatCamera.HorizontalFov, ((MissionView)this).MissionScreen.CombatCamera.GetAspectRatio(), ((MissionView)this).MissionScreen.CombatCamera.Near, ((MissionView)this).MissionScreen.CombatCamera.Far);
				_cinematicCamera.Frame = ((MissionView)this).MissionScreen.CombatCamera.Frame;
				((MissionView)this).MissionScreen.CustomCamera = _cinematicCamera;
				ScreenFadeController.BeginFadeIn(1f);
				_shouldTickCinematic = true;
				MissionHint activeHint = MissionHint.CreateWithKeyAndAction(new TextObject("{=FiSENWMB}Skip Cinematic", (Dictionary<string, object>)null), HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 14));
				_hintLogic.SetActiveHint(activeHint);
				_missionMainAgentController.Disable();
				_suspendedFeatures = _shipControlView.SuspendedFeatures;
				_shipControlView.SuspendFeature(~_suspendedFeatures);
			}
			else if (_fadeOutReason == FadeOutReason.BallistaCinematicEnded)
			{
				((MissionView)this).MissionScreen.CustomCamera = null;
				Camera cinematicCamera = _cinematicCamera;
				if (cinematicCamera != null)
				{
					cinematicCamera.ReleaseCamera();
				}
				_cinematicCamera = null;
				_shouldTickCinematic = false;
				if (!_controller.IsPhaseOneCompleted && !_controller.IsStartedFromCheckpoint && !_isShowingBallistaHint && !_hasUsedBallista && missionShip != null)
				{
					if (Agent.Main != null)
					{
						ShipControllerMachine shipControllerMachine = missionShip.ShipControllerMachine;
						Agent.Main.UseGameObject((UsableMissionObject)(object)((UsableMachine)shipControllerMachine).PilotStandingPoint, -1);
					}
					_missionMainAgentController.Enable();
					_shipControlView.ResumeFeature(~_suspendedFeatures);
					_hintLogic.Clear();
					_isShowingBallistaHint = true;
					MissionHint activeHint2 = MissionHint.CreateWithKeyAndAction(new TextObject("{=aTEkCItM}Control Ballista", (Dictionary<string, object>)null), HotKeyManager.GetHotKeyId("NavalShipControlsHotKeyCategory", 115));
					_hintLogic.SetActiveHint(activeHint2);
				}
			}
			_controller.OnViewFadeOut((int)_fadeOutReason);
			_canInvokeFadeOutEvent = false;
		}
		if (!ScreenFadeController.IsFadeActive && !_canInvokeFadeOutEvent)
		{
			_canInvokeFadeOutEvent = true;
		}
	}

	private void OnBallistaDestroyed(DestructableComponent target, Agent attackerAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
	{
		if (_markerByBallista.TryGetValue(target, out var value))
		{
			value.SetIsActive(false);
			AreMarkersDirty = true;
		}
	}

	public override void OnFixedMissionTick(float fixedDt)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (_shouldTickCinematic && !Game.Current.GameStateManager.ActiveStateDisabledByUser)
		{
			_cinematicElapsedTime += fixedDt;
			_cinematicCamera.Frame = _cinematicCameraTrack.Evaluate(_cinematicElapsedTime);
			_cinematicEventTrack.Evaluate(_cinematicElapsedTime);
			if ((Mission.Current.InputManager.IsGameKeyDown(14) && _cinematicElapsedTime >= 2.5f) || (_cinematicCameraTrack.IsCompleted(_cinematicElapsedTime) && _cinematicEventTrack.IsCompleted(_cinematicElapsedTime)))
			{
				_shouldTickCinematic = false;
				_hintLogic.Clear();
				CampaignInformationManager.ClearAllDialogNotifications(true);
				_fadeOutReason = FadeOutReason.BallistaCinematicEnded;
				ScreenFadeController.BeginFadeOutAndIn(0.5f, 0.5f, 0.5f);
				_canInvokeFadeOutEvent = true;
			}
		}
	}

	private void InitializeView()
	{
		_controller = ((MissionBehavior)this).Mission.GetMissionBehavior<FloatingFortressSetPieceBattleMissionController>();
		_hintLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionHintLogic>();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_missionMainAgentController = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>();
		_shipControlView = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletShipControlView>();
		InitializeCinematicKeyframes();
	}

	private void InitializeCinematicKeyframes()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Expected O, but got Unknown
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Expected O, but got Unknown
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_initial").GetGlobalFrame();
		MatrixFrame globalFrame2 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_1").GetGlobalFrame();
		MatrixFrame globalFrame3 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_1a").GetGlobalFrame();
		MatrixFrame globalFrame4 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_2").GetGlobalFrame();
		MatrixFrame globalFrame5 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_2a").GetGlobalFrame();
		MatrixFrame globalFrame6 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_3").GetGlobalFrame();
		MatrixFrame globalFrame7 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_3a").GetGlobalFrame();
		MatrixFrame globalFrame8 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_4").GetGlobalFrame();
		MatrixFrame globalFrame9 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_4a").GetGlobalFrame();
		MatrixFrame globalFrame10 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_5").GetGlobalFrame();
		MatrixFrame globalFrame11 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_camera_5a").GetGlobalFrame();
		TextObject dialogueText1 = new TextObject("{=VUWTon9z}Have a good look at Crusas's floating fortress before we attack. It's formidable, but it's not going anywhere.", (Dictionary<string, object>)null);
		TextObject dialogueText2 = new TextObject("{=0JjVa9p9}He has no less than eight ships lashed together. They mount four heavy mangonels - big ones. Most ships would tip over from the recoil if they weren't chained to each other.", (Dictionary<string, object>)null);
		TextObject dialogueText3 = new TextObject("{=4Bhb39KH}One is on the roundship, which is the fortress's keep, as it were.", (Dictionary<string, object>)null);
		TextObject dialogueText4 = new TextObject("{=MTJMs4A7}Another three are on cogs - one is to the northwest.", (Dictionary<string, object>)null);
		TextObject dialogueText5 = new TextObject("{=ObjIiR2M}The others are to the northeast and southeast.", (Dictionary<string, object>)null);
		TextObject dialogueText6 = new TextObject("{=mVa3D9xf}You must steer the Wasp to take out those mangonels. You need direct hits - but don’t get too close, as their decks are packed with archers. ", (Dictionary<string, object>)null);
		TextObject dialogueText7 = new TextObject("{=afb9bd35}Also, keep moving. One or two hits could shatter our timbers or set us alight and make an end of us.", (Dictionary<string, object>)null);
		TextObject dialogueText8 = new TextObject("{=NIlRAHPb}We're right behind you, brother. Let's take this vile toad of a merchant down!", (Dictionary<string, object>)null);
		_cinematicCameraTrack = new MatrixFrameTrack();
		_cinematicEventTrack = new EventTrack();
		float num = 0f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame));
		_cinematicEventTrack.AddKeyframe(new EventKeyframe(num, delegate
		{
			CampaignInformationManager.ClearAllDialogNotifications(true);
			CampaignInformationManager.AddDialogLine(dialogueText1, NavalStorylineData.Bjolgur.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}));
		num += 10f;
		_cinematicEventTrack.AddKeyframe(new EventKeyframe(num, delegate
		{
			CampaignInformationManager.ClearAllDialogNotifications(true);
			CampaignInformationManager.AddDialogLine(dialogueText2, NavalStorylineData.Bjolgur.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}));
		num += 15f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame2));
		_cinematicEventTrack.AddKeyframe(new EventKeyframe(num, delegate
		{
			CampaignInformationManager.ClearAllDialogNotifications(true);
			CampaignInformationManager.AddDialogLine(dialogueText3, NavalStorylineData.Bjolgur.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}));
		num += 6f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame3));
		_cinematicEventTrack.AddKeyframe(new EventKeyframe(num, delegate
		{
			CampaignInformationManager.ClearAllDialogNotifications(true);
			CampaignInformationManager.AddDialogLine(dialogueText4, NavalStorylineData.Bjolgur.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}));
		num += 0.5f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame4));
		num += 5.5f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame5));
		_cinematicEventTrack.AddKeyframe(new EventKeyframe(num, delegate
		{
			CampaignInformationManager.ClearAllDialogNotifications(true);
			CampaignInformationManager.AddDialogLine(dialogueText5, NavalStorylineData.Bjolgur.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}));
		num += 0.5f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame6));
		num += 1.5f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame7));
		num += 0.5f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame8));
		num += 1.5f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame9));
		_cinematicEventTrack.AddKeyframe(new EventKeyframe(num, delegate
		{
			CampaignInformationManager.ClearAllDialogNotifications(true);
			CampaignInformationManager.AddDialogLine(dialogueText6, NavalStorylineData.Bjolgur.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}));
		num += 6f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame10));
		num += 6f;
		_cinematicEventTrack.AddKeyframe(new EventKeyframe(num, delegate
		{
			CampaignInformationManager.ClearAllDialogNotifications(true);
			CampaignInformationManager.AddDialogLine(dialogueText7, NavalStorylineData.Bjolgur.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}));
		num += 8f;
		_cinematicEventTrack.AddKeyframe(new EventKeyframe(num, delegate
		{
			CampaignInformationManager.ClearAllDialogNotifications(true);
			CampaignInformationManager.AddDialogLine(dialogueText8, NavalStorylineData.Bjolgur.CharacterObject, (Equipment)null, 0, (NotificationPriority)2);
		}));
		num += 7f;
		_cinematicCameraTrack.AddKeyframe(new MatrixFrameKeyFrame(num, globalFrame11));
	}

	private void OnPhaseOneCompleted()
	{
		if (_controller.IsStartedFromCheckpoint)
		{
			ScreenFadeController.BeginFadeIn(0.75f);
		}
		else
		{
			_willFadeOutForPhaseOneCompletion = true;
		}
	}

	private static AnimatedBasicAreaIndicator AddMarker(WeakGameEntity gameEntity, TextObject name, string type, float radius = 5f)
	{
		((WeakGameEntity)(ref gameEntity)).CreateAndAddScriptComponent("AnimatedBasicAreaIndicator", true);
		AnimatedBasicAreaIndicator firstScriptOfType = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<AnimatedBasicAreaIndicator>();
		((AreaMarker)firstScriptOfType).AreaRadius = radius;
		firstScriptOfType.Type = type;
		firstScriptOfType.SetOverriddenName(name);
		return firstScriptOfType;
	}
}
