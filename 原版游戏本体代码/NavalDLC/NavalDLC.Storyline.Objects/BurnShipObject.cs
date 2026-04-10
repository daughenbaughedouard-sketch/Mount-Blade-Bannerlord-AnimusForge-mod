using System.Collections.Generic;
using NavalDLC.Storyline.MissionControllers;
using SandBox.AI;
using SandBox.Objects.AnimationPoints;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Storyline.Objects;

public class BurnShipObject : UsableMachine
{
	public float UseTime = 5f;

	private DynamicObjectAnimationPoint _machineUsePoint;

	private BlockedEstuaryMissionController _controller;

	private bool _hasUserCached;

	private bool _stateSet;

	private bool _used;

	private float _timer;

	public bool HasUser => ((UsableMissionObject)_machineUsePoint).HasUser;

	public override bool IsDeactivated => _used;

	protected override void OnInit()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnInit();
		_controller = Mission.Current.GetMissionBehavior<BlockedEstuaryMissionController>();
		_machineUsePoint = (DynamicObjectAnimationPoint)((UsableMachine)this).PilotStandingPoint;
		((UsableMissionObject)_machineUsePoint).IsDeactivated = false;
		((UsableMissionObject)_machineUsePoint).IsDisabledForPlayers = true;
		((UsableMissionObject)_machineUsePoint).LockUserFrames = false;
		((UsableMissionObject)_machineUsePoint).LockUserPositions = false;
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	protected override void OnTick(float dt)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnTick(dt);
		if (_hasUserCached != HasUser)
		{
			_timer = 0f;
			_hasUserCached = HasUser;
		}
		if (!_used)
		{
			if (((UsableMissionObject)_machineUsePoint).HasUser && !_stateSet)
			{
				ActionIndexCache val = ActionIndexCache.Create(_machineUsePoint.LoopStartAction);
				((UsableMissionObject)_machineUsePoint).UserAgent.SetActionChannel(0, ref val, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				_stateSet = true;
			}
			if (((UsableMissionObject)_machineUsePoint).HasUser)
			{
				_timer += dt;
			}
			if (_stateSet && ((UsableMissionObject)_machineUsePoint).HasUser && _timer > UseTime)
			{
				OnUse();
			}
		}
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=eAnAZNib}Barrel of oil", (Dictionary<string, object>)null);
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(2 | ((UsableMachine)this).GetTickRequirement());
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		TextObject obj = GameTexts.FindText("str_key_action", (string)null);
		obj.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		return obj;
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return (UsableMachineAIBase)new UsablePlaceAI((UsableMachine)(object)this);
	}

	private void OnUse()
	{
		((UsableMissionObject)_machineUsePoint).UserAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		((MissionObject)this).SetDisabled(true);
		_used = true;
		_controller.OnBurningMachineUsed(this);
	}
}
