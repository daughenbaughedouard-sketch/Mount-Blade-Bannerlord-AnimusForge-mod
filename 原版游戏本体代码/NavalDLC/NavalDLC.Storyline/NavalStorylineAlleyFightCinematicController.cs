using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Hints;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;

namespace NavalDLC.Storyline;

public class NavalStorylineAlleyFightCinematicController : MissionLogic
{
	public enum NavalAlleyFightCinematicState
	{
		Ready,
		InitialFadeOut,
		BlackScreen,
		InitialFadeIn,
		FirstCamera,
		FinalCamera,
		Completed
	}

	private class ConversationLine
	{
		public TextObject Line;

		public CharacterObject Speaker;

		public DialogNotificationHandle Handle;

		public ConversationLine(TextObject line, CharacterObject speaker)
		{
			Line = line;
			Speaker = speaker;
		}
	}

	private const float CinematicTriggerRadius = 3f;

	private const float FadeDuration = 0.75f;

	private const float BlackScreenDuration = 0.25f;

	private const float FirstCameraDuration = 10f;

	private const int SkipHotKey = 14;

	private bool _isMissionInitialized;

	private List<GameEntity> _entities = new List<GameEntity>();

	private GameEntity _currentCameraEntity;

	private GameEntity _cameraEntity;

	private GameEntity _cameraEntity2;

	private GameEntity _cinematicTriggerZone;

	private NavalAlleyFightCinematicState _currentCinematicState;

	private float _cinematicTimer;

	private NavalStorylineAlleyFightMissionController _missionController;

	private MissionHintLogic _missionHintLogic;

	private List<ConversationLine> _allLines;

	private CharacterObject _enemyCharacterObject;

	private bool _isPostFightConversationQueued;

	private float _postFightDialogueFadeTimer;

	private bool _isConversationSetup;

	private const float PostFightDialogueFadeOutDuration = 0.75f;

	private const float PostFightDialogueBlackDuration = 1f;

	private const float PostFightDialogueFadeInDuration = 0.75f;

	private TextObject SkipHintText => new TextObject("{=FiSENWMB}Skip Cinematic", (Dictionary<string, object>)null);

	public event Action<NavalAlleyFightCinematicState> OnCinematicStateChanged;

	public event Action<float, float, float> OnFightEndedEvent;

	public event Action<Vec3> OnConversationSetupEvent;

	public override void OnMissionTick(float dt)
	{
		if (!_isMissionInitialized)
		{
			Initialize();
		}
		TickCinematic(dt);
		if (_isPostFightConversationQueued)
		{
			_postFightDialogueFadeTimer += dt;
			if (!_isConversationSetup && _postFightDialogueFadeTimer >= 0.75f)
			{
				_isConversationSetup = true;
				_missionController.SetupConversation();
			}
			if (_postFightDialogueFadeTimer >= 1.75f)
			{
				_isPostFightConversationQueued = false;
				_missionController.StartPostFightConversation();
			}
		}
	}

	private void Initialize()
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		_isMissionInitialized = true;
		UpdateEntityReferences();
		_missionController = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalStorylineAlleyFightMissionController>();
		_missionHintLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionHintLogic>();
		_cinematicTriggerZone = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("trigger_cutscene")));
		_cameraEntity = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("sp_camera")));
		_cameraEntity2 = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("sp_camera_2")));
		_currentCameraEntity = _cameraEntity;
		SoundManager.SetListenerFrame(_currentCameraEntity.GetGlobalFrame(), _currentCameraEntity.GlobalPosition);
		_enemyCharacterObject = _missionController.GetEnemyCharacterObject();
		_allLines = new List<ConversationLine>
		{
			new ConversationLine(new TextObject("{=4nAQl8Vx}Listen, you lot, I'm in a bit of a hurry. If I give you a penny each will you stop pestering me?", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject),
			new ConversationLine(new TextObject("{=p7Gxhb6O}You're Gunnar of Lagshofn, aren't you? We've got a message from the Sea Hounds for you.", (Dictionary<string, object>)null), _enemyCharacterObject),
			new ConversationLine(new TextObject("{=G6NrtQuF}You’ve got a message from those curs? Out with it, then. What’s your message?", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject),
			new ConversationLine(new TextObject("{=OMpfszRu}The message... the message is that you will die, you damn fool.", (Dictionary<string, object>)null), _enemyCharacterObject),
			new ConversationLine(new TextObject("{=qtz4B25N}And how should I die, then? Of old age, while you three work up the courage to attack a wizened graybeard? Go on, you've delivered your message, now scamper off.", (Dictionary<string, object>)null), NavalStorylineData.Gunnar.CharacterObject),
			new ConversationLine(new TextObject("{=Nmv85ZfP}We’ll send you down to the Pale One right now. Kill him, boys!", (Dictionary<string, object>)null), _enemyCharacterObject)
		};
	}

	private void UpdateEntityReferences()
	{
		((MissionBehavior)this).Mission.Scene.GetEntities(ref _entities);
	}

	public void GetCameraFrame(out Vec3 position, out Vec3 forward)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!_isMissionInitialized)
		{
			Initialize();
		}
		position = _currentCameraEntity.GlobalPosition;
		forward = _currentCameraEntity.GetGlobalFrame().rotation.f;
	}

	public float GetFadeDuration()
	{
		return 0.75f;
	}

	public float GetBlackScreenDuration()
	{
		return 0.25f;
	}

	private void SetCinematicState(NavalAlleyFightCinematicState newState)
	{
		_cinematicTimer = 0f;
		_currentCinematicState = newState;
		this.OnCinematicStateChanged(_currentCinematicState);
		if (newState == NavalAlleyFightCinematicState.FirstCamera)
		{
			ShowSkipCinematicHintText();
		}
	}

	private void TickCinematic(float dt)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		if (_currentCinematicState == NavalAlleyFightCinematicState.Completed)
		{
			return;
		}
		if (_currentCinematicState == NavalAlleyFightCinematicState.Ready && Agent.Main != null)
		{
			Vec3 globalPosition = _cinematicTriggerZone.GlobalPosition;
			if (((Vec3)(ref globalPosition)).DistanceSquared(Agent.Main.Position) <= 9f)
			{
				if (Mission.Current.CameraIsFirstPerson)
				{
					Mission.Current.CameraIsFirstPerson = false;
				}
				_missionController.OnCinematicStarted();
				SetCinematicState(NavalAlleyFightCinematicState.InitialFadeOut);
			}
		}
		_cinematicTimer += dt;
		if (_currentCinematicState == NavalAlleyFightCinematicState.InitialFadeOut && _cinematicTimer >= 0.75f)
		{
			SetCinematicState(NavalAlleyFightCinematicState.BlackScreen);
		}
		if (_currentCinematicState == NavalAlleyFightCinematicState.BlackScreen)
		{
			if (_cinematicTimer >= 0.25f)
			{
				ActivatePlayerEavesdropAnimation();
				SetCinematicState(NavalAlleyFightCinematicState.InitialFadeIn);
			}
		}
		else if (_currentCinematicState == NavalAlleyFightCinematicState.InitialFadeIn)
		{
			if (_cinematicTimer >= 0.75f)
			{
				foreach (ConversationLine allLine in _allLines)
				{
					DialogNotificationHandle handle = CampaignInformationManager.AddDialogLine(allLine.Line, allLine.Speaker, ((BasicCharacterObject)allLine.Speaker).FirstCivilianEquipment, 0, (NotificationPriority)4);
					allLine.Handle = handle;
				}
				SetCinematicState(NavalAlleyFightCinematicState.FirstCamera);
			}
		}
		else if (_currentCinematicState == NavalAlleyFightCinematicState.FirstCamera)
		{
			if (_cinematicTimer >= 10f)
			{
				_currentCameraEntity = _cameraEntity2;
				SetCinematicState(NavalAlleyFightCinematicState.FinalCamera);
				SoundManager.SetListenerFrame(_currentCameraEntity.GetGlobalFrame(), _currentCameraEntity.GlobalPosition);
			}
		}
		else if (_currentCinematicState == NavalAlleyFightCinematicState.FinalCamera && _allLines.TrueForAll((ConversationLine x) => (int)CampaignInformationManager.GetStatusOfDialogNotification(x.Handle) == 0))
		{
			FinishCinematic();
		}
		HandleSkipCinematic();
	}

	private void ActivatePlayerEavesdropAnimation()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		if (Agent.Main.GetCurrentAction(0) != ActionIndexCache.act_cutscene_npc_argue_player_1)
		{
			Agent.Main.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)1);
			Agent.Main.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)1);
			Agent.Main.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			Agent.Main.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("sp_player_wait")));
			Agent.Main.TeleportToPosition(val.GlobalPosition);
			Vec3 f = val.GetGlobalFrame().rotation.f;
			Agent.Main.LookDirection = f;
			this.OnConversationSetupEvent(f);
			Agent.Main.SetActionChannel(0, ref ActionIndexCache.act_cutscene_npc_argue_player_1, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
		}
	}

	private void FinishCinematic()
	{
		SetCinematicState(NavalAlleyFightCinematicState.Completed);
		_missionController.StartFight();
		Agent.Main.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		_missionHintLogic.Clear();
	}

	private void HandleSkipCinematic()
	{
		if ((_currentCinematicState != NavalAlleyFightCinematicState.FirstCamera && _currentCinematicState != NavalAlleyFightCinematicState.FinalCamera) || !Mission.Current.InputManager.IsGameKeyDown(14) || !_allLines.Any((ConversationLine x) => (int)CampaignInformationManager.GetStatusOfDialogNotification(x.Handle) > 0))
		{
			return;
		}
		foreach (ConversationLine allLine in _allLines)
		{
			CampaignInformationManager.ClearDialogNotification(allLine.Handle, false);
		}
		FinishCinematic();
	}

	public void OnFightEnded()
	{
		_isPostFightConversationQueued = true;
		this.OnFightEndedEvent(0.75f, 1f, 0.75f);
	}

	private void ShowSkipCinematicHintText()
	{
		if (_missionHintLogic.ActiveHint != null)
		{
			_missionHintLogic.Clear();
		}
		MissionHint activeHint = MissionHint.CreateWithKeyAndAction(SkipHintText, HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 14));
		_missionHintLogic.SetActiveHint(activeHint);
	}

	public void OnConversationSetup(Vec3 direction)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		this.OnConversationSetupEvent(direction);
	}
}
