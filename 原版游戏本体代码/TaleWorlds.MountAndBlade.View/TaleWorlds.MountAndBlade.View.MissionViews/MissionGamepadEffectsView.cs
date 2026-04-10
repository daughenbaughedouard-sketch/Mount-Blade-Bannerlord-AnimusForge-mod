using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

[DefaultView]
public class MissionGamepadEffectsView : MissionView
{
	private enum TriggerState
	{
		Off,
		SoftTriggerFeedbackLeft,
		SoftTriggerFeedbackRight,
		HardTriggerFeedbackLeft,
		HardTriggerFeedbackRight,
		WeaponEffect,
		Vibration
	}

	private TriggerState _triggerState;

	private readonly byte[] _triggerFeedback = new byte[4];

	private bool _isAdaptiveTriggerEnabled;

	private bool _usingAlternativeAiming;

	private RangedSiegeWeapon _currentlyUsedSiegeWeapon;

	private UsableMissionObject _currentlyUsedMissionObject;

	public override void OnMissionStateActivated()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		((MissionBehavior)this).OnMissionStateActivated();
		ResetTriggerFeedback();
		ResetTriggerVibration();
		_isAdaptiveTriggerEnabled = NativeOptions.GetConfig((NativeOptionsType)14) != 0f;
		_usingAlternativeAiming = NativeOptions.GetConfig((NativeOptionsType)18) != 0f;
		NativeOptions.OnNativeOptionChanged = (OnNativeOptionChangedDelegate)Delegate.Combine((Delegate?)(object)NativeOptions.OnNativeOptionChanged, (Delegate?)new OnNativeOptionChangedDelegate(OnNativeOptionChanged));
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
	}

	private void OnGamepadActiveStateChanged()
	{
		if (!Input.IsGamepadActive)
		{
			ResetTriggerFeedback();
			ResetTriggerVibration();
		}
	}

	public override void OnMissionStateDeactivated()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		((MissionBehavior)this).OnMissionStateDeactivated();
		ResetTriggerFeedback();
		ResetTriggerVibration();
		NativeOptions.OnNativeOptionChanged = (OnNativeOptionChangedDelegate)Delegate.Remove((Delegate?)(object)NativeOptions.OnNativeOptionChanged, (Delegate?)new OnNativeOptionChangedDelegate(OnNativeOptionChanged));
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Remove(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
	}

	public override void OnPreMissionTick(float dt)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnPreMissionTick(dt);
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		if (!_isAdaptiveTriggerEnabled)
		{
			return;
		}
		if (mainAgent != null && (int)mainAgent.State == 1 && mainAgent.CombatActionsEnabled && !mainAgent.IsCheering && !((MissionBehavior)this).Mission.IsOrderMenuOpen && IsMissionModeApplicableForAdaptiveTrigger(((MissionBehavior)this).Mission.Mode))
		{
			MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;
			WeaponComponentData currentUsageItem = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
			bool num = currentUsageItem != null && Extensions.HasAllFlags<WeaponFlags>(currentUsageItem.WeaponFlags, (WeaponFlags)3072);
			WeaponComponentData currentUsageItem2 = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
			bool flag = currentUsageItem2 != null && Extensions.HasAllFlags<WeaponFlags>(currentUsageItem2.WeaponFlags, (WeaponFlags)1024) && !Extensions.HasAllFlags<WeaponFlags>(((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.WeaponFlags, (WeaponFlags)3072);
			WeaponComponentData currentUsageItem3 = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
			bool flag2 = currentUsageItem3 != null && currentUsageItem3.IsRangedWeapon && ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsConsumable;
			WeaponComponentData currentUsageItem4 = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
			int num2;
			if (currentUsageItem4 == null || !Extensions.HasAllFlags<WeaponFlags>(currentUsageItem4.WeaponFlags, (WeaponFlags)1))
			{
				MissionWeapon wieldedOffhandWeapon = mainAgent.WieldedOffhandWeapon;
				num2 = (((MissionWeapon)(ref wieldedOffhandWeapon)).IsShield() ? 1 : 0);
			}
			else
			{
				num2 = 1;
			}
			bool flag3 = (byte)num2 != 0;
			if (num)
			{
				HandleBowAdaptiveTriggers();
			}
			else if (flag)
			{
				HandleCrossbowAdaptiveTriggers();
			}
			else if (flag2)
			{
				HandleThrowableAdaptiveTriggers();
			}
			else if (flag3)
			{
				HandleMeleeAdaptiveTriggers();
			}
			else if (mainAgent.CurrentlyUsedGameObject != null)
			{
				if (mainAgent.CurrentlyUsedGameObject != _currentlyUsedMissionObject)
				{
					_currentlyUsedMissionObject = mainAgent.CurrentlyUsedGameObject;
					UsableMachine usableMachineFromUsableMissionObject = GetUsableMachineFromUsableMissionObject(_currentlyUsedMissionObject);
					RangedSiegeWeapon val;
					_currentlyUsedSiegeWeapon = (((val = (RangedSiegeWeapon)(object)((usableMachineFromUsableMissionObject is RangedSiegeWeapon) ? usableMachineFromUsableMissionObject : null)) != null) ? val : null);
				}
				HandleRangedSiegeEngineAdaptiveTriggers(_currentlyUsedSiegeWeapon);
			}
			else
			{
				_currentlyUsedSiegeWeapon = null;
				_currentlyUsedMissionObject = null;
				ResetTriggerFeedback();
				ResetTriggerVibration();
			}
		}
		else
		{
			_currentlyUsedSiegeWeapon = null;
			_currentlyUsedMissionObject = null;
			ResetTriggerFeedback();
			ResetTriggerVibration();
		}
	}

	public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Invalid comparison between Unknown and I4
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentHit(affectedAgent, affectorAgent, ref affectorWeapon, ref blow, ref attackCollisionData);
		AttackCollisionData val;
		if (affectedAgent == Agent.Main)
		{
			val = attackCollisionData;
			if ((int)((AttackCollisionData)(ref val)).CollisionResult != 3)
			{
				val = attackCollisionData;
				if ((int)((AttackCollisionData)(ref val)).CollisionResult != 5)
				{
					val = attackCollisionData;
					if ((int)((AttackCollisionData)(ref val)).CollisionResult != 4)
					{
						goto IL_0092;
					}
				}
			}
			float[] leftTriggerAmplitudes = new float[1] { 0.5f };
			float[] leftTriggerFrequencies = new float[1] { 0.3f };
			float[] array = new float[1] { 0.3f };
			SetTriggerVibration(leftTriggerAmplitudes, leftTriggerFrequencies, array, array.Length, null, null, null, 0);
			SetTriggerState(TriggerState.Off);
			goto IL_0092;
		}
		if (affectorAgent != Agent.Main)
		{
			return;
		}
		val = attackCollisionData;
		if ((int)((AttackCollisionData)(ref val)).CollisionResult != 1)
		{
			val = attackCollisionData;
			if ((int)((AttackCollisionData)(ref val)).CollisionResult != 3)
			{
				return;
			}
		}
		MissionWeapon val2 = affectorWeapon;
		if (!((MissionWeapon)(ref val2)).IsEmpty)
		{
			val2 = affectorWeapon;
			if (((MissionWeapon)(ref val2)).IsShield())
			{
				float[] leftTriggerAmplitudes2 = new float[1] { 1f };
				float[] leftTriggerFrequencies2 = new float[1] { 0.1f };
				float[] array2 = new float[1] { 0.35f };
				SetTriggerVibration(leftTriggerAmplitudes2, leftTriggerFrequencies2, array2, array2.Length, null, null, null, 0);
			}
		}
		return;
		IL_0092:
		val2 = affectedAgent.WieldedOffhandWeapon;
		if (((MissionWeapon)(ref val2)).IsEmpty)
		{
			val = attackCollisionData;
			if (((AttackCollisionData)(ref val)).AttackBlockedWithShield)
			{
				SetTriggerState(TriggerState.Off);
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
		if (affectedAgent.IsMainAgent)
		{
			ResetTriggerFeedback();
			ResetTriggerVibration();
		}
	}

	protected override void OnEndMission()
	{
		((MissionBehavior)this).OnEndMission();
		SetTriggerState(TriggerState.Off);
	}

	private void OnNativeOptionChanged(NativeOptionsType changedNativeOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)changedNativeOptionsType == 14)
		{
			bool isAdaptiveTriggerEnabled = _isAdaptiveTriggerEnabled;
			_isAdaptiveTriggerEnabled = NativeOptions.GetConfig((NativeOptionsType)14) != 0f;
			_usingAlternativeAiming = NativeOptions.GetConfig((NativeOptionsType)18) != 0f;
			if (isAdaptiveTriggerEnabled && !_isAdaptiveTriggerEnabled)
			{
				_currentlyUsedSiegeWeapon = null;
				_currentlyUsedMissionObject = null;
				ResetTriggerFeedback();
				ResetTriggerVibration();
			}
		}
	}

	private bool IsMissionModeApplicableForAdaptiveTrigger(MissionMode mode)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected I4, but got Unknown
		switch ((int)mode)
		{
		case 0:
		case 2:
		case 3:
		case 4:
		case 7:
			return true;
		default:
			return false;
		}
	}

	private void HandleBowAdaptiveTriggers()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Invalid comparison between Unknown and I4
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		ActionStage val = (ActionStage)((mainAgent == null) ? (-1) : ((int)mainAgent.GetCurrentActionStage(1)));
		if ((int)val == -1 || (int)val == 3 || (int)val == 4)
		{
			SetTriggerState(_usingAlternativeAiming ? TriggerState.SoftTriggerFeedbackLeft : TriggerState.SoftTriggerFeedbackRight);
		}
		else if ((int)val == 0)
		{
			float num = mainAgent.GetAimingTimer() - mainAgent.AgentDrivenProperties.WeaponUnsteadyBeginTime;
			if (num > 0f)
			{
				float num2 = mainAgent.AgentDrivenProperties.WeaponUnsteadyEndTime - mainAgent.AgentDrivenProperties.WeaponUnsteadyBeginTime;
				float num3 = MBMath.ClampFloat(num / num2, 0f, 1f);
				float num4 = MBMath.Lerp(0f, 1f, num3, 1E-05f);
				float[] array = new float[1] { num4 };
				float num5 = MBMath.ClampFloat(1f - num4, 0.1f, 1f);
				float[] array2 = new float[1] { num5 };
				float[] array3 = new float[1] { 0.05f };
				if (_usingAlternativeAiming)
				{
					SetTriggerVibration(array, array2, array3, array3.Length, null, null, null, 0);
				}
				else
				{
					SetTriggerVibration(null, null, null, 0, array, array2, array3, array3.Length);
				}
				_triggerState = TriggerState.Vibration;
			}
			else
			{
				SetTriggerState(_usingAlternativeAiming ? TriggerState.SoftTriggerFeedbackLeft : TriggerState.SoftTriggerFeedbackRight);
				float[] array4 = new float[1] { 0.07f };
				float[] array5 = new float[1] { 0.5f };
				float[] array6 = new float[1] { 0.5f };
				if (_usingAlternativeAiming)
				{
					SetTriggerVibration(array4, array5, array6, array6.Length, null, null, null, 0);
				}
				else
				{
					SetTriggerVibration(null, null, null, 0, array4, array5, array6, array6.Length);
				}
			}
			if (_usingAlternativeAiming)
			{
				SetTriggerWeaponEffect(0, 0, 0, 3, 7, 8);
			}
			else
			{
				SetTriggerWeaponEffect(0, 0, 0, 0, 0, 0);
			}
		}
		else if ((int)val == 2)
		{
			SetTriggerState(TriggerState.Off);
		}
		else
		{
			SetTriggerState(TriggerState.Off);
		}
	}

	private void HandleCrossbowAdaptiveTriggers()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		ActionStage val = (ActionStage)((mainAgent == null) ? (-1) : ((int)mainAgent.GetCurrentActionStage(1)));
		if ((int)val == 3)
		{
			SetTriggerState(TriggerState.Off);
		}
		else if ((int)val == 2)
		{
			float[] rightTriggerAmplitudes = new float[1] { 0.01f };
			float[] rightTriggerFrequencies = new float[1] { 0.08f };
			float[] array = new float[1] { 0.05f };
			SetTriggerVibration(null, null, null, 0, rightTriggerAmplitudes, rightTriggerFrequencies, array, array.Length);
			SetTriggerState(TriggerState.Off);
		}
		else if ((int)val == 0)
		{
			if (_usingAlternativeAiming)
			{
				SetTriggerWeaponEffect(0, 0, 0, 3, 7, 8);
			}
		}
		else if (!_usingAlternativeAiming)
		{
			SetTriggerWeaponEffect(0, 0, 0, 3, 7, 8);
		}
	}

	private void HandleThrowableAdaptiveTriggers()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		MissionWeapon wieldedOffhandWeapon = ((MissionBehavior)this).Mission.MainAgent.WieldedOffhandWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref wieldedOffhandWeapon)).CurrentUsageItem;
		bool num = currentUsageItem != null && Extensions.HasAnyFlag<WeaponFlags>(currentUsageItem.WeaponFlags, (WeaponFlags)268435456);
		_triggerFeedback[2] = 0;
		_triggerFeedback[3] = 3;
		if (num)
		{
			_triggerFeedback[0] = 4;
			_triggerFeedback[1] = 2;
		}
		else
		{
			_triggerFeedback[0] = 0;
			_triggerFeedback[1] = 0;
		}
		SetTriggerFeedback(_triggerFeedback[0], _triggerFeedback[1], _triggerFeedback[2], _triggerFeedback[3]);
	}

	private void HandleMeleeAdaptiveTriggers()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;
		WeaponComponentData currentUsageItem = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
		bool flag = currentUsageItem != null && Extensions.HasAllFlags<WeaponFlags>(currentUsageItem.WeaponFlags, (WeaponFlags)16);
		MissionWeapon wieldedOffhandWeapon = mainAgent.WieldedOffhandWeapon;
		WeaponComponentData currentUsageItem2 = ((MissionWeapon)(ref wieldedOffhandWeapon)).CurrentUsageItem;
		bool num = currentUsageItem2 != null && Extensions.HasAnyFlag<WeaponFlags>(currentUsageItem2.WeaponFlags, (WeaponFlags)268435456);
		if (flag)
		{
			_triggerFeedback[2] = 3;
			_triggerFeedback[3] = 0;
		}
		else if (((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem == null)
		{
			_triggerFeedback[2] = 0;
			_triggerFeedback[3] = 0;
		}
		else
		{
			_triggerFeedback[2] = 4;
			_triggerFeedback[3] = 1;
		}
		if (num || flag || ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem != null)
		{
			_triggerFeedback[0] = 4;
			_triggerFeedback[1] = 2;
		}
		else
		{
			_triggerFeedback[0] = 0;
			_triggerFeedback[1] = 0;
		}
		SetTriggerFeedback(_triggerFeedback[0], _triggerFeedback[1], _triggerFeedback[2], _triggerFeedback[3]);
	}

	private void HandleRangedSiegeEngineAdaptiveTriggers(RangedSiegeWeapon rangedSiegeWeapon)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		if (rangedSiegeWeapon is Ballista || rangedSiegeWeapon is FireBallista)
		{
			if ((int)rangedSiegeWeapon.State == 0)
			{
				SetTriggerWeaponEffect(0, 0, 0, 4, 6, 10);
			}
			else if ((int)rangedSiegeWeapon.State == 2 || (int)rangedSiegeWeapon.State == 1)
			{
				SetTriggerWeaponEffect(0, 0, 0, 0, 0, 0);
				float[] rightTriggerAmplitudes = new float[3] { 0.2f, 0.4f, 0.2f };
				float[] rightTriggerFrequencies = new float[3] { 0.2f, 0.4f, 0.2f };
				float[] array = new float[3] { 0.2f, 0.3f, 0.2f };
				SetTriggerVibration(null, null, null, 0, rightTriggerAmplitudes, rightTriggerFrequencies, array, array.Length);
			}
			else
			{
				ResetTriggerFeedback();
				ResetTriggerVibration();
			}
		}
		else
		{
			ResetTriggerFeedback();
			ResetTriggerVibration();
		}
	}

	private UsableMachine GetUsableMachineFromUsableMissionObject(UsableMissionObject usableMissionObject)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		StandingPoint val;
		if ((val = (StandingPoint)(object)((usableMissionObject is StandingPoint) ? usableMissionObject : null)) != null)
		{
			WeakGameEntity val2 = ((ScriptComponentBehavior)val).GameEntity;
			while (((WeakGameEntity)(ref val2)).IsValid && !((WeakGameEntity)(ref val2)).HasScriptOfType<UsableMachine>())
			{
				val2 = ((WeakGameEntity)(ref val2)).Parent;
			}
			if (((WeakGameEntity)(ref val2)).IsValid)
			{
				UsableMachine firstScriptOfType = ((WeakGameEntity)(ref val2)).GetFirstScriptOfType<UsableMachine>();
				if (firstScriptOfType != null)
				{
					return firstScriptOfType;
				}
			}
		}
		return null;
	}

	private void SetTriggerState(TriggerState triggerState)
	{
		if (_triggerState != triggerState)
		{
			switch (triggerState)
			{
			case TriggerState.Off:
				ResetTriggerFeedback();
				ResetTriggerVibration();
				break;
			case TriggerState.SoftTriggerFeedbackRight:
				SetTriggerFeedback(0, 0, 0, 2);
				SetTriggerWeaponEffect(0, 0, 0, 0, 0, 0);
				break;
			case TriggerState.HardTriggerFeedbackRight:
				SetTriggerFeedback(0, 0, 0, 4);
				SetTriggerWeaponEffect(0, 0, 0, 0, 0, 0);
				break;
			case TriggerState.SoftTriggerFeedbackLeft:
				SetTriggerFeedback(0, 2, 0, 0);
				SetTriggerWeaponEffect(0, 0, 0, 0, 0, 0);
				break;
			case TriggerState.HardTriggerFeedbackLeft:
				SetTriggerFeedback(0, 4, 0, 0);
				SetTriggerWeaponEffect(0, 0, 0, 0, 0, 0);
				break;
			case TriggerState.WeaponEffect:
				SetTriggerWeaponEffect(0, 0, 0, 4, 7, 7);
				break;
			default:
				Debug.FailedAssert("Unexpected trigger state:" + triggerState, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\MissionViews\\MissionGamepadEffectsView.cs", "SetTriggerState", 495);
				break;
			}
			_triggerState = triggerState;
		}
	}

	private void ResetTriggerFeedback()
	{
		_triggerFeedback[0] = 0;
		_triggerFeedback[1] = 0;
		_triggerFeedback[2] = 0;
		_triggerFeedback[3] = 0;
		SetTriggerFeedback(0, 0, 0, 0);
		SetTriggerWeaponEffect(0, 0, 0, 0, 0, 0);
		_triggerState = TriggerState.Off;
	}

	private void SetTriggerFeedback(byte leftTriggerPosition, byte leftTriggerStrength, byte rightTriggerPosition, byte rightTriggerStrength)
	{
		Input.SetTriggerFeedback(leftTriggerPosition, leftTriggerStrength, rightTriggerPosition, rightTriggerStrength);
	}

	private void SetTriggerWeaponEffect(byte leftStartPosition, byte leftEnd_position, byte leftStrength, byte rightStartPosition, byte rightEndPosition, byte rightStrength)
	{
		Input.SetTriggerWeaponEffect(leftStartPosition, leftEnd_position, leftStrength, rightStartPosition, rightEndPosition, rightStrength);
	}

	private void ResetTriggerVibration()
	{
		float[] array = new float[1];
		SetTriggerVibration(array, array, array, 0, array, array, array, 0);
	}

	private void SetTriggerVibration(float[] leftTriggerAmplitudes, float[] leftTriggerFrequencies, float[] leftTriggerDurations, int numLeftTriggerElements, float[] rightTriggerAmplitudes, float[] rightTriggerFrequencies, float[] rightTriggerDurations, int numRightTriggerElements)
	{
		Input.SetTriggerVibration(leftTriggerAmplitudes, leftTriggerFrequencies, leftTriggerDurations, numLeftTriggerElements, rightTriggerAmplitudes, rightTriggerFrequencies, rightTriggerDurations, numRightTriggerElements);
	}

	private static void SetLightbarColor(float red, float green, float blue)
	{
		Input.SetLightbarColor(red, green, blue);
	}
}
