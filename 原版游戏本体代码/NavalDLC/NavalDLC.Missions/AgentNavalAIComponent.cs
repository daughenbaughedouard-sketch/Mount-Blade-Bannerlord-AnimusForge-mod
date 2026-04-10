using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions;

public class AgentNavalAIComponent : AgentComponent
{
	public enum AgentNavalTaunts
	{
		Invite,
		Invite2,
		Point
	}

	private const float CheckBridgeAndTargetingAgentCooldown = 3f;

	private const float BarkCooldown = 1.5f;

	private const float MediumMoraleThreshold = 70f;

	private const float JumpOffTimerInSeconds = 2f;

	private float _tauntTimer;

	private float _barkTimer;

	private float _checkBridgesAndTargetingAgentTimer;

	private float _tauntCooldown = 12f + MBRandom.RandomFloat * 2f;

	private float _tauntDelayTimer;

	private float _barkDelayTimer;

	private float _tauntDelay;

	private float _barkDelay;

	private bool _tauntFired;

	private bool _barkFired;

	private AgentNavalComponent _agentNavalComponent;

	private NavalShipsLogic _navalShipsLogic;

	private ActionIndexCache _currentActionIndexCache;

	private SkinVoiceType _currentVoiceType;

	private bool _isConnectedToEnemyWithoutBridges;

	public AgentNavalAIComponent(Agent agent)
		: base(agent)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		_tauntTimer = 0f;
		_barkTimer = 0f;
		_checkBridgesAndTargetingAgentTimer = 0f;
		_tauntDelay = 0f;
		_barkDelay = 0f;
		_tauntDelayTimer = 0f;
		_barkDelayTimer = 0f;
		_tauntFired = false;
		_barkFired = false;
		_isConnectedToEnemyWithoutBridges = false;
		_currentActionIndexCache = ActionIndexCache.act_none;
		_agentNavalComponent = base.Agent.GetComponent<AgentNavalComponent>();
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
	}

	public bool UnderMeleeAttack(float timeLimit = 1f)
	{
		return MBCommon.GetTotalMissionTime() - base.Agent.LastMeleeHitTime < timeLimit;
	}

	public bool UnderRangedAttack(float timeLimit = 1f)
	{
		return MBCommon.GetTotalMissionTime() - base.Agent.LastMeleeHitTime < timeLimit;
	}

	public bool RangeAttacking(float timeLimit = 1f)
	{
		return MBCommon.GetTotalMissionTime() - base.Agent.LastRangedAttackTime < timeLimit;
	}

	public bool MeleeAttacking(float timeLimit = 1f)
	{
		return MBCommon.GetTotalMissionTime() - base.Agent.LastMeleeHitTime < timeLimit;
	}

	private bool DecideBoardingTaunts()
	{
		bool result = false;
		float morale = AgentComponentExtensions.GetMorale(base.Agent);
		if (!base.Agent.IsUsingGameObject && morale > 70f && _agentNavalComponent.SteppedShip != null)
		{
			float randomFloat = MBRandom.RandomFloat;
			if (_isConnectedToEnemyWithoutBridges)
			{
				if (randomFloat < 0.33f)
				{
					TryToTriggerTaunt(AgentNavalTaunts.Invite, 0.1f + MBRandom.RandomFloat * 1.5f, 0.1f);
				}
				else if (randomFloat < 0.66f)
				{
					TryToTriggerTaunt(AgentNavalTaunts.Invite2, 0.1f + MBRandom.RandomFloat * 1.5f, 0.1f);
				}
				else
				{
					TryToTriggerTaunt(AgentNavalTaunts.Point, 0.1f + MBRandom.RandomFloat * 1.5f, 0.1f);
				}
				result = true;
			}
		}
		return result;
	}

	private bool DecideTaunt()
	{
		bool result = false;
		if (base.Agent.IsAIControlled)
		{
			result = DecideBoardingTaunts();
		}
		return result;
	}

	public override void OnTickParallel(float dt)
	{
		_tauntTimer += dt;
		_tauntDelayTimer += dt;
		if (_tauntTimer >= _tauntCooldown)
		{
			DecideTaunt();
			_tauntTimer = 0f;
		}
		ExecuteTaunt();
	}

	public override void OnTick(float dt)
	{
		if (base.Agent.IsAIControlled && Mission.Current.CurrentTime - _agentNavalComponent.LastJumpOffTime >= 2f && _agentNavalComponent.SteppedShip != null && _agentNavalComponent.SteppedShip.FireHitPoints <= 0f && !base.Agent.IsUsingGameObject && base.Agent.HumanAIComponent.GetCurrentlyMovingGameObject() == null && base.Agent.IsOnLand())
		{
			foreach (ShipAttachmentPointMachine item in (List<ShipAttachmentPointMachine>)(object)_agentNavalComponent.SteppedShip.AttachmentPointMachines)
			{
				if (((UsableMissionObject)((UsableMachine)item).PilotStandingPoint).HasAIMovingTo || ((UsableMachine)item).PilotAgent != null || item.CurrentAttachment != null)
				{
					continue;
				}
				if (base.Agent.Formation == null)
				{
					AgentComponentExtensions.AIMoveToGameObjectEnable(base.Agent, (UsableMissionObject)(object)((UsableMachine)item).PilotStandingPoint, (IDetachment)(object)item, (AIScriptedFrameFlags)2);
				}
				else if (base.Agent.Formation == _agentNavalComponent.SteppedShip.Formation)
				{
					if (base.Agent.Detachment != null)
					{
						base.Agent.TryAttachToFormation();
					}
					((UsableMachine)item).AddAgentAtSlotIndex(base.Agent, 0);
				}
				break;
			}
		}
		_barkTimer += dt;
		_barkDelayTimer += dt;
		_checkBridgesAndTargetingAgentTimer += dt;
		ExecuteBark();
		if (_checkBridgesAndTargetingAgentTimer >= 3f)
		{
			_isConnectedToEnemyWithoutBridges = _agentNavalComponent.SteppedShip != null && _agentNavalComponent.SteppedShip.GetIsConnectedToEnemyWithoutBridges();
			_checkBridgesAndTargetingAgentTimer = 0f;
		}
	}

	private void ExecuteTaunt()
	{
		if (_tauntFired && _tauntDelayTimer >= _tauntDelay)
		{
			base.Agent.SetActionChannel(1, ref _currentActionIndexCache, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			_tauntDelayTimer = 0f;
			_tauntFired = false;
		}
	}

	private void ExecuteBark()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (_barkFired && _barkDelayTimer >= _barkDelay)
		{
			base.Agent.MakeVoice(_currentVoiceType, (CombatVoiceNetworkPredictionType)2);
			_barkDelayTimer = 0f;
			_barkFired = false;
		}
	}

	public void TryToTriggerTaunt(AgentNavalTaunts navalTaunt, float delay, float chanceToTrigger = 1f, bool makeTimerZeroIfSuccessful = false)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (chanceToTrigger >= MBRandom.RandomFloat && !base.Agent.IsInBeingStruckAction && base.Agent.IsOnLand() && (makeTimerZeroIfSuccessful || (_tauntTimer >= _tauntCooldown && !_tauntFired)) && !UnderMeleeAttack() && !UnderRangedAttack() && !RangeAttacking() && !MeleeAttacking())
		{
			_currentActionIndexCache = SelectActionForTaunt(navalTaunt);
			_tauntDelay = delay;
			if (makeTimerZeroIfSuccessful)
			{
				base.Agent.SetActionChannel(1, ref _currentActionIndexCache, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				_tauntFired = false;
			}
			else
			{
				_tauntDelayTimer = 0f;
				_tauntFired = true;
			}
		}
	}

	public void TryToTriggerBark(SkinVoiceType voiceType, float delay, float chanceToTrigger = 1f, bool makeTimerZeroIfSuccessful = false)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!(_barkTimer >= 1.5f) || !(chanceToTrigger >= MBRandom.RandomFloat))
		{
			return;
		}
		if (Mission.Current.MainAgent != null)
		{
			Vec3 position = Mission.Current.MainAgent.Position;
			if (!(((Vec3)(ref position)).DistanceSquared(base.Agent.Position) < 625f))
			{
				return;
			}
		}
		_barkTimer = 0f;
		_barkDelay = delay;
		_barkDelayTimer = 0f;
		_currentVoiceType = voiceType;
		if (makeTimerZeroIfSuccessful)
		{
			base.Agent.MakeVoice(_currentVoiceType, (CombatVoiceNetworkPredictionType)2);
			_barkFired = false;
		}
		else
		{
			_barkFired = true;
		}
	}

	private ActionIndexCache SelectActionForTaunt(AgentNavalTaunts navalTaunt)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		ActionIndexCache result = ActionIndexCache.act_none;
		EquipmentIndex primaryWieldedItemIndex = base.Agent.GetPrimaryWieldedItemIndex();
		EquipmentIndex offhandWieldedItemIndex = base.Agent.GetOffhandWieldedItemIndex();
		object obj;
		MissionWeapon val;
		if ((int)primaryWieldedItemIndex == -1)
		{
			obj = null;
		}
		else
		{
			val = base.Agent.Equipment[primaryWieldedItemIndex];
			obj = ((MissionWeapon)(ref val)).CurrentUsageItem;
		}
		WeaponComponentData val2 = (WeaponComponentData)obj;
		object obj2;
		if ((int)offhandWieldedItemIndex == -1)
		{
			obj2 = null;
		}
		else
		{
			val = base.Agent.Equipment[offhandWieldedItemIndex];
			obj2 = ((MissionWeapon)(ref val)).CurrentUsageItem;
		}
		WeaponComponentData val3 = (WeaponComponentData)obj2;
		bool hasMount = base.Agent.HasMount;
		bool isLeftStance = base.Agent.GetIsLeftStance();
		int num = -1;
		switch (navalTaunt)
		{
		case AgentNavalTaunts.Invite:
			num = ((val3 == null || !val3.IsShield) ? TauntUsageManager.Instance.GetIndexOfAction("taunt_10") : TauntUsageManager.Instance.GetIndexOfAction("taunt_13"));
			break;
		case AgentNavalTaunts.Invite2:
			num = TauntUsageManager.Instance.GetIndexOfAction("taunt_11");
			break;
		case AgentNavalTaunts.Point:
			num = TauntUsageManager.Instance.GetIndexOfAction("taunt_17");
			break;
		}
		if (num != -1)
		{
			result = ActionIndexCache.Create(TauntUsageManager.Instance.GetAction(num, isLeftStance, !hasMount, val2, val3));
		}
		return result;
	}
}
