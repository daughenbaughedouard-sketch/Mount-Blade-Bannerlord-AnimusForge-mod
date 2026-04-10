using System.Collections.Generic;
using System.Linq;
using SandBox.AI;
using SandBox.Objects.AnimationPoints;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.Usables;

public class SmithingMachine : UsableMachine
{
	private enum State
	{
		Stable,
		Preparation,
		Working,
		Paused,
		UseAnvilPoint
	}

	private const string MachineIdleAnimationName = "anim_merchant_smithing_machine_idle";

	private const string MachineIdleWithBlendInAnimationName = "anim_merchant_smithing_machine_idle_with_blend_in";

	private const string MachineUseAnimationName = "anim_merchant_smithing_machine_loop";

	private static readonly ActionIndexCache[] _actionsWithoutLeftHandItem = (ActionIndexCache[])(object)new ActionIndexCache[4]
	{
		ActionIndexCache.act_smithing_machine_anvil_start,
		ActionIndexCache.act_smithing_machine_anvil_part_2,
		ActionIndexCache.act_smithing_machine_anvil_part_4,
		ActionIndexCache.act_smithing_machine_anvil_part_5
	};

	private AnimationPoint _anvilUsePoint;

	private AnimationPoint _machineUsePoint;

	private State _state;

	private Timer _disableTimer;

	private float _remainingTimeToReset;

	private bool _leftItemIsVisible;

	protected override void OnInit()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnInit();
		_machineUsePoint = (AnimationPoint)(object)((UsableMachine)this).PilotStandingPoint;
		if (_machineUsePoint == null)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			_ = "Entity(" + ((WeakGameEntity)(ref gameEntity)).Name + ") with script(SmithingMachine) does not have a valid 'PilotStandingPoint'.";
		}
		((UsableMissionObject)_machineUsePoint).IsDeactivated = false;
		((UsableMissionObject)_machineUsePoint).IsDisabledForPlayers = true;
		_machineUsePoint.KeepOldVisibility = true;
		_anvilUsePoint = (AnimationPoint)(object)((IEnumerable<StandingPoint>)((UsableMachine)this).StandingPoints).First((StandingPoint x) => (object)x != _machineUsePoint);
		((UsableMissionObject)_anvilUsePoint).IsDeactivated = true;
		_anvilUsePoint.KeepOldVisibility = true;
		foreach (StandingPoint item in (List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)
		{
			((UsableMissionObject)item).IsDisabledForPlayers = true;
		}
		((SynchedMissionObject)this).SetAnimationAtChannelSynched("anim_merchant_smithing_machine_idle", 0, 1f);
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=OCRafO5h}Bellows", (Dictionary<string, object>)null);
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		TextObject val = new TextObject("{=fEQAPJ2e}{KEY} Use", (Dictionary<string, object>)null);
		val.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		return val;
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(2 | ((UsableMachine)this).GetTickRequirement());
	}

	protected override void OnTick(float dt)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Expected O, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Expected O, but got Unknown
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnTick(dt);
		switch (_state)
		{
		case State.Stable:
			if (((UsableMissionObject)_machineUsePoint).HasUser)
			{
				Vec2 currentVelocity = ((UsableMissionObject)_machineUsePoint).UserAgent.GetCurrentVelocity();
				if (((Vec2)(ref currentVelocity)).LengthSquared < 0.0001f)
				{
					((UsableMissionObject)_machineUsePoint).UserAgent.SetActionChannel(0, ref ActionIndexCache.act_use_smithing_machine_ready, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
					_state = State.Preparation;
				}
			}
			if (((UsableMissionObject)_anvilUsePoint).HasUser)
			{
				_state = State.UseAnvilPoint;
			}
			break;
		case State.Preparation:
			if (!((UsableMissionObject)_machineUsePoint).HasUser)
			{
				((SynchedMissionObject)this).SetAnimationAtChannelSynched("anim_merchant_smithing_machine_idle_with_blend_in", 0, 1f);
				_state = State.Stable;
			}
			else if (((UsableMissionObject)_machineUsePoint).UserAgent.GetCurrentAction(0) == ActionIndexCache.act_use_smithing_machine_ready && ((UsableMissionObject)_machineUsePoint).UserAgent.GetCurrentActionProgress(0) > 0.99f)
			{
				((SynchedMissionObject)this).SetAnimationAtChannelSynched("anim_merchant_smithing_machine_loop", 0, 1f);
				((UsableMissionObject)_machineUsePoint).UserAgent.SetActionChannel(0, ref ActionIndexCache.act_use_smithing_machine_loop, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				_state = State.Working;
			}
			break;
		case State.Working:
			if (!((UsableMissionObject)_machineUsePoint).HasUser)
			{
				((SynchedMissionObject)this).SetAnimationAtChannelSynched("anim_merchant_smithing_machine_idle_with_blend_in", 0, 1f);
				_state = State.Stable;
				_disableTimer = null;
				((UsableMissionObject)_anvilUsePoint).IsDeactivated = false;
			}
			else if (((UsableMissionObject)_machineUsePoint).UserAgent.GetCurrentAction(0) != ActionIndexCache.act_use_smithing_machine_loop)
			{
				((SynchedMissionObject)this).SetAnimationAtChannelSynched("anim_merchant_smithing_machine_idle_with_blend_in", 0, 1f);
				_state = State.Paused;
				_remainingTimeToReset = _disableTimer.Duration - _disableTimer.ElapsedTime();
			}
			else if (_disableTimer == null)
			{
				_disableTimer = new Timer(Mission.Current.CurrentTime, 9.8f, true);
			}
			else if (_disableTimer.Check(Mission.Current.CurrentTime))
			{
				((SynchedMissionObject)this).SetAnimationAtChannelSynched("anim_merchant_smithing_machine_idle_with_blend_in", 0, 1f);
				_disableTimer = null;
				((UsableMissionObject)_machineUsePoint).IsDeactivated = true;
				((UsableMissionObject)_anvilUsePoint).IsDeactivated = false;
				_state = State.Stable;
			}
			break;
		case State.Paused:
			if (_machineUsePoint.IsRotationCorrectDuringUsage())
			{
				((UsableMissionObject)_machineUsePoint).UserAgent.SetActionChannel(0, ref ActionIndexCache.act_use_smithing_machine_ready, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			}
			if (((UsableMissionObject)_machineUsePoint).UserAgent.GetCurrentAction(0) == ActionIndexCache.act_use_smithing_machine_ready)
			{
				_state = State.Preparation;
				_disableTimer.Reset(Mission.Current.CurrentTime, _remainingTimeToReset);
				_remainingTimeToReset = 0f;
			}
			break;
		case State.UseAnvilPoint:
		{
			if (!((UsableMissionObject)_anvilUsePoint).HasUser)
			{
				_state = State.Stable;
				_disableTimer = null;
				((UsableMissionObject)_machineUsePoint).IsDeactivated = false;
				break;
			}
			if (_disableTimer == null)
			{
				_disableTimer = new Timer(Mission.Current.CurrentTime, 96f, true);
				_leftItemIsVisible = true;
				break;
			}
			if (_disableTimer.Check(Mission.Current.CurrentTime))
			{
				_disableTimer = null;
				((UsableMissionObject)_anvilUsePoint).IsDeactivated = true;
				((UsableMissionObject)_machineUsePoint).IsDeactivated = false;
				_state = State.Stable;
				break;
			}
			ActionIndexCache currentAction = ((UsableMissionObject)_anvilUsePoint).UserAgent.GetCurrentAction(0);
			if (_leftItemIsVisible && _actionsWithoutLeftHandItem.Contains(currentAction))
			{
				((UsableMissionObject)_anvilUsePoint).UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SetPrefabVisibility(((UsableMissionObject)_anvilUsePoint).UserAgent.Monster.OffHandItemBoneIndex, _anvilUsePoint.LeftHandItem, isVisible: false);
				_leftItemIsVisible = false;
			}
			else if (!_leftItemIsVisible && !_actionsWithoutLeftHandItem.Contains(currentAction))
			{
				((UsableMissionObject)_anvilUsePoint).UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SetPrefabVisibility(((UsableMissionObject)_anvilUsePoint).UserAgent.Monster.OffHandItemBoneIndex, _anvilUsePoint.LeftHandItem, isVisible: true);
				_leftItemIsVisible = true;
			}
			break;
		}
		}
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new UsablePlaceAI((UsableMachine)(object)this);
	}
}
