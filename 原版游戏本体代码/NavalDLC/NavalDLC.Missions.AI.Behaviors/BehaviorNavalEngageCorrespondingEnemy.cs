using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.Behaviors;

public sealed class BehaviorNavalEngageCorrespondingEnemy : NavalBehaviorComponent
{
	private enum ShipBoardingState
	{
		ApproachFromFarAway,
		GettingClose,
		AdjustingOrientation,
		InPosition,
		Connected
	}

	private const float IdealBoardingDistance = 12f;

	private const float MaximumBoardingDistance = 30f;

	private const float DriftedAwayDistance = 50f;

	private NavalShipsLogic _navalShipsLogic;

	private MissionShip _formationMainShip;

	private MBReadOnlyList<ShipAttachmentMachine> _formationShipAttachmentMachines;

	private MBReadOnlyList<ShipAttachmentPointMachine> _formationShipAttachmentPointMachines;

	private TeamAINavalComponent _navalTeamAI;

	private ShipBoardingState _currentState;

	private bool _tacticallyOnRightSide;

	private MissionShip _targetShip;

	private int _navalLineOrder;

	private bool _perfectMatch = true;

	private bool _actualRightSide;

	private NavalBehaviorBoardShipSubtask _boardShipSubtask;

	public BehaviorNavalEngageCorrespondingEnemy(Formation formation)
		: base(formation)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).BehaviorCoherence = 0.8f;
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		List<WeakGameEntity> source = new List<WeakGameEntity>();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_formationMainShip).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetChildrenRecursive(ref source);
		_formationShipAttachmentMachines = (MBReadOnlyList<ShipAttachmentMachine>)(object)Extensions.ToMBList<ShipAttachmentMachine>(from ce in source
			where ((WeakGameEntity)(ref ce)).HasScriptOfType<ShipAttachmentMachine>()
			select ((WeakGameEntity)(ref ce)).GetFirstScriptOfType<ShipAttachmentMachine>());
		_formationShipAttachmentPointMachines = (MBReadOnlyList<ShipAttachmentPointMachine>)(object)Extensions.ToMBList<ShipAttachmentPointMachine>(from ce in source
			where ((WeakGameEntity)(ref ce)).HasScriptOfType<ShipAttachmentPointMachine>()
			select ((WeakGameEntity)(ref ce)).GetFirstScriptOfType<ShipAttachmentPointMachine>());
		_navalTeamAI = ((BehaviorComponent)this).Formation.Team.TeamAI as TeamAINavalComponent;
		_currentState = ShipBoardingState.ApproachFromFarAway;
		((BehaviorComponent)this).CalculateCurrentOrder();
		_boardShipSubtask = new NavalBehaviorBoardShipSubtask(_formationMainShip);
	}

	public override void RefreshShipReferences()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
		SetTargetShipSideAndOrder(_tacticallyOnRightSide, _navalLineOrder);
	}

	public void SetTargetShipSideAndOrder(bool rightSide, int navalLineOrder)
	{
		_tacticallyOnRightSide = rightSide;
		_actualRightSide = rightSide;
		_navalLineOrder = navalLineOrder;
		_targetShip = FindCorrespondingEnemyShip();
		_boardShipSubtask.SetTargetShipAndSide(_targetShip, _tacticallyOnRightSide);
	}

	private MissionShip FindCorrespondingEnemyShip()
	{
		if (_formationMainShip == null || ((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count <= 0)
		{
			return null;
		}
		if (_formationMainShip.GetIsConnectedToEnemy(out var connectedEnemyShip))
		{
			return connectedEnemyShip;
		}
		float num = ((float)((List<Formation>)(object)_navalTeamAI.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count - 1f) * 0.5f;
		float num2 = ((float)((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count - 1f) * 0.5f;
		bool flag = num > num2;
		if ((int)num == _navalLineOrder && (float)(int)num + 0.1f > num)
		{
			if (num2 >= (float)(int)num2 + 0.1f)
			{
				_actualRightSide = flag;
				num2 += (_actualRightSide ? 0.5f : (-0.5f));
			}
			if (num2 < 0f)
			{
				num2 = 0f;
				_actualRightSide = true;
			}
			else if (num2 >= (float)((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count)
			{
				num2 = ((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count - 1;
				_actualRightSide = false;
			}
			return ((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder)[(int)num2];
		}
		int num3;
		int num4;
		float num5;
		float num6;
		if ((float)(int)num + 0.1f > num)
		{
			num3 = (int)(num - 1f);
			num4 = (int)(num + 1f);
			num5 = num2 + 1f;
			num6 = num2 - 1f;
		}
		else
		{
			num3 = (int)(num - 0.5f);
			num4 = (int)(num + 0.5f);
			num5 = num2 + 0.5f;
			num6 = num2 - 0.5f;
		}
		while (num3 >= 0 || num4 < ((List<Formation>)(object)_navalTeamAI.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count)
		{
			if (num3 == _navalLineOrder)
			{
				if (num5 >= (float)(int)num5 + 0.1f)
				{
					num5 += (flag ? (-0.5f) : 0.5f);
					_actualRightSide = !flag;
				}
				else
				{
					_actualRightSide = flag;
				}
				if ((int)num5 >= ((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count)
				{
					_actualRightSide = false;
					num5 = ((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count - 1;
				}
				return ((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder)[(int)num5];
			}
			if (num4 == _navalLineOrder)
			{
				if (num6 >= (float)(int)num6 + 0.1f)
				{
					num6 += (flag ? 0.5f : (-0.5f));
					_actualRightSide = flag;
				}
				else
				{
					_actualRightSide = !flag;
				}
				if (num6 < 0f)
				{
					_actualRightSide = true;
					num6 = 0f;
				}
				return ((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder)[(int)num6];
			}
			num3--;
			num4++;
			num5 += 1f;
			num6 -= 1f;
		}
		return null;
	}

	private void RefreshTargetShip()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		MissionShip missionShip2;
		MissionShip missionShip = (missionShip2 = _boardShipSubtask.GetCurrentEffectiveTargetShip());
		Formation val = ((BehaviorComponent)this).Formation.CachedClosestEnemyFormation?.Formation;
		MissionShip ship = null;
		if (val != null)
		{
			_navalShipsLogic.GetShip(val.Team.TeamSide, val.FormationIndex, out ship);
		}
		WeakGameEntity gameEntity;
		Vec3 globalPosition;
		if (ship != null)
		{
			gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
			globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			gameEntity = ((ScriptComponentBehavior)_formationMainShip).GameEntity;
			float num = ((Vec3)(ref globalPosition)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			if (num <= 3600f)
			{
				double num2 = Math.Sqrt(num);
				if (_targetShip != null)
				{
					gameEntity = ((ScriptComponentBehavior)_targetShip).GameEntity;
					globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
					gameEntity = ((ScriptComponentBehavior)_formationMainShip).GameEntity;
					if (!((double)((Vec3)(ref globalPosition)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) - num2 > 30.0) || !((double)_boardShipSubtask.GetEffectiveDistanceToObjective() - num2 > 30.0))
					{
						goto IL_00f1;
					}
				}
				missionShip2 = ship;
			}
		}
		goto IL_00f1;
		IL_00f1:
		if (missionShip == missionShip2 || _targetShip == missionShip2)
		{
			return;
		}
		MissionShip targetShip = _targetShip;
		if ((targetShip != null && !targetShip.AnyActiveFormationTroopOnShip) || missionShip2 == null)
		{
			_targetShip = missionShip2;
			return;
		}
		if (!(_boardShipSubtask.GetEffectiveDistanceToObjective() > 60f))
		{
			float effectiveDistanceToObjective = _boardShipSubtask.GetEffectiveDistanceToObjective();
			gameEntity = ((ScriptComponentBehavior)_formationMainShip).GameEntity;
			globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			gameEntity = ((ScriptComponentBehavior)missionShip2).GameEntity;
			if (!(effectiveDistanceToObjective > ((Vec3)(ref globalPosition)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition) * 1.2f))
			{
				return;
			}
		}
		_targetShip = missionShip2;
		_boardShipSubtask.SetTargetShipAndSide(_targetShip, _tacticallyOnRightSide);
	}

	protected override void CalculateCurrentOrder()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (_navalShipsLogic == null || ((BehaviorComponent)this).Formation.CachedClosestEnemyFormation == null || _targetShip == null)
		{
			((BehaviorComponent)this).CurrentOrder = MovementOrder.MovementOrderStop;
		}
		else if (_formationMainShip != null && (_formationMainShip.SearchShipConnection(null, isDirect: true, findEnemy: true, enforceActive: false, acceptNotBridgedConnections: false) || _currentState == ShipBoardingState.Connected))
		{
			((BehaviorComponent)this).CurrentOrder = MovementOrder.MovementOrderCharge;
		}
		else
		{
			((BehaviorComponent)this).CurrentOrder = MovementOrder.MovementOrderStop;
		}
	}

	private void CalculateAndSetShipOrders()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (((BehaviorComponent)this).Formation.CachedClosestEnemyFormation != null && _targetShip != null && _formationMainShip != null && _formationMainShip.IsFormationAndShipAIControlled)
		{
			_boardShipSubtask.CalculateShipOrders(out var desiredPosition, out var desiredDirection, out var boardingTargetShip);
			_formationMainShip.ShipOrder.SetShipMovementOrder(desiredPosition, in desiredDirection);
			_formationMainShip.ShipOrder.SetBoardingTargetShip(boardingTargetShip);
		}
	}

	private void CheckAndRefreshTargetIfNecessary()
	{
		if (_targetShip == null || !_targetShip.AnyActiveFormationTroopOnShip)
		{
			_targetShip = FindCorrespondingEnemyShip();
			_boardShipSubtask.SetTargetShipAndSide(_targetShip, _tacticallyOnRightSide);
			return;
		}
		switch (_boardShipSubtask.State)
		{
		case NavalBehaviorBoardShipSubtask.ShipBoardingState.ApproachFromFarAway:
		case NavalBehaviorBoardShipSubtask.ShipBoardingState.GettingClose:
			RefreshTargetShip();
			break;
		case NavalBehaviorBoardShipSubtask.ShipBoardingState.InactiveStuck:
		{
			MissionShip missionShip = FindCorrespondingEnemyShip();
			if (missionShip != _boardShipSubtask.GetCurrentGivenTarget())
			{
				_targetShip = missionShip;
				_boardShipSubtask.SetTargetShipAndSide(_targetShip, _tacticallyOnRightSide);
			}
			break;
		}
		}
	}

	private void CheckAndSwitchState()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		if (((BehaviorComponent)this).Formation.CachedClosestEnemyFormation != null && _targetShip != null && _targetShip.AnyActiveFormationTroopOnShip && _formationMainShip != null)
		{
			MatrixFrame globalFrame = _targetShip.GlobalFrame;
			MatrixFrame globalFrame2 = _formationMainShip.GlobalFrame;
			Vec2 val;
			Vec2 val2;
			if (!_actualRightSide)
			{
				val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				val = ((Vec2)(ref val)).RightVec();
				val2 = ((Vec2)(ref val)).Normalized();
			}
			else
			{
				val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				val = ((Vec2)(ref val)).LeftVec();
				val2 = ((Vec2)(ref val)).Normalized();
			}
			Vec2 val3 = val2;
			Vec2 asVec;
			switch (_currentState)
			{
			case ShipBoardingState.ApproachFromFarAway:
				val = ((Vec3)(ref globalFrame.origin)).AsVec2 - ((Vec3)(ref globalFrame2.origin)).AsVec2;
				if (!(((Vec2)(ref val)).LengthSquared < 900f))
				{
					val = ((Vec3)(ref globalFrame.origin)).AsVec2 + val3 * 12f - ((Vec3)(ref globalFrame2.origin)).AsVec2;
					if (!(((Vec2)(ref val)).LengthSquared < 2500f))
					{
						break;
					}
				}
				_currentState = ShipBoardingState.GettingClose;
				break;
			case ShipBoardingState.GettingClose:
				val = ((Vec3)(ref globalFrame.origin)).AsVec2 - ((Vec3)(ref globalFrame2.origin)).AsVec2;
				if (!(((Vec2)(ref val)).LengthSquared < 900f))
				{
					val = ((Vec3)(ref globalFrame.origin)).AsVec2 + val3 * 12f - ((Vec3)(ref globalFrame2.origin)).AsVec2;
					if (!(((Vec2)(ref val)).LengthSquared < 900f))
					{
						break;
					}
				}
				_currentState = ShipBoardingState.AdjustingOrientation;
				break;
			case ShipBoardingState.AdjustingOrientation:
				val = ((Vec3)(ref globalFrame.origin)).AsVec2 + val3 * 12f - ((Vec3)(ref globalFrame2.origin)).AsVec2;
				if (((Vec2)(ref val)).LengthSquared > 2500f)
				{
					_currentState = ShipBoardingState.GettingClose;
					break;
				}
				val = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
				val = ((Vec2)(ref val)).Normalized();
				asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				if (Math.Abs(((Vec2)(ref val)).DotProduct(((Vec2)(ref asVec)).Normalized())) > 0.8f)
				{
					_currentState = ShipBoardingState.InPosition;
				}
				break;
			case ShipBoardingState.InPosition:
			{
				bool flag2 = false;
				foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_formationShipAttachmentMachines)
				{
					if (item.CurrentAttachment != null)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)_formationShipAttachmentPointMachines)
					{
						if (item2.CurrentAttachment != null)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (flag2)
				{
					_currentState = ShipBoardingState.Connected;
					break;
				}
				val = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
				val = ((Vec2)(ref val)).Normalized();
				asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				if (Math.Abs(((Vec2)(ref val)).DotProduct(((Vec2)(ref asVec)).Normalized())) < 0.6f)
				{
					_currentState = ShipBoardingState.AdjustingOrientation;
					break;
				}
				val = ((Vec3)(ref globalFrame.origin)).AsVec2 + val3 * 12f - ((Vec3)(ref globalFrame2.origin)).AsVec2;
				if (((Vec2)(ref val)).LengthSquared > 2500f)
				{
					_currentState = ShipBoardingState.GettingClose;
				}
				break;
			}
			case ShipBoardingState.Connected:
			{
				bool flag = false;
				foreach (ShipAttachmentMachine item3 in (List<ShipAttachmentMachine>)(object)_formationShipAttachmentMachines)
				{
					if (item3.CurrentAttachment != null)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					foreach (ShipAttachmentPointMachine item4 in (List<ShipAttachmentPointMachine>)(object)_formationShipAttachmentPointMachines)
					{
						if (item4.CurrentAttachment != null)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					_currentState = ShipBoardingState.GettingClose;
				}
				break;
			}
			}
		}
		else
		{
			RefreshTargetShip();
		}
	}

	public override void OnDeploymentFinished()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).OnDeploymentFinished();
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		_currentState = ShipBoardingState.ApproachFromFarAway;
	}

	public override void ResetBehavior()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).ResetBehavior();
		_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		_currentState = ShipBoardingState.ApproachFromFarAway;
	}

	protected override void OnBehaviorActivatedAux()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		RefreshTargetShip();
		_boardShipSubtask.SetOwnerShip(_formationMainShip);
		_targetShip = FindCorrespondingEnemyShip();
		_boardShipSubtask.SetTargetShipAndSide(_targetShip, _tacticallyOnRightSide);
		_currentState = ShipBoardingState.ApproachFromFarAway;
		_formationMainShip.ShipOrder.SetBoardingTargetShip(null);
		_formationMainShip.ShipOrder.SetCutLoose(enable: false);
		_formationMainShip.ShipOrder.SetOrderOarsmenLevel(2);
		((BehaviorComponent)this).CalculateCurrentOrder();
		((BehaviorComponent)this).Formation.SetMovementOrder(((BehaviorComponent)this).CurrentOrder);
		((BehaviorComponent)this).Formation.SetFacingOrder(((BehaviorComponent)this).CurrentFacingOrder);
		((BehaviorComponent)this).Formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
		((BehaviorComponent)this).Formation.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
		((BehaviorComponent)this).Formation.SetFormOrder(FormOrder.FormOrderWide, true);
	}

	private void CancelPreferredTargetShipForAttachmentMachines()
	{
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)_formationShipAttachmentMachines)
		{
			item.SetPreferredTargetShip(null);
		}
	}

	public override void OnLostAIControl()
	{
		((BehaviorComponent)this).OnLostAIControl();
		CancelPreferredTargetShipForAttachmentMachines();
	}

	public override void OnBehaviorCanceled()
	{
		((BehaviorComponent)this).OnBehaviorCanceled();
		CancelPreferredTargetShipForAttachmentMachines();
	}

	public override void TickOccasionally()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (_navalShipsLogic == null)
		{
			_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
			if (_navalShipsLogic == null)
			{
				return;
			}
		}
		if (_formationMainShip.Formation != ((BehaviorComponent)this).Formation)
		{
			_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		}
		CheckAndRefreshTargetIfNecessary();
		CalculateAndSetShipOrders();
		((BehaviorComponent)this).CalculateCurrentOrder();
		((BehaviorComponent)this).Formation.SetMovementOrder(((BehaviorComponent)this).CurrentOrder);
		((BehaviorComponent)this).Formation.SetFacingOrder(((BehaviorComponent)this).CurrentFacingOrder);
	}

	protected override float GetAiWeight()
	{
		float num = 0f;
		if (((BehaviorComponent)this).Formation.CachedClosestEnemyFormation != null)
		{
			num = ((!(((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.FormationMeleeFightingPower > 0f)) ? 20f : (((BehaviorComponent)this).Formation.QuerySystem.FormationMeleeFightingPower / ((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.FormationMeleeFightingPower));
		}
		return (_perfectMatch ? 1.5f : 1.25f) * MathF.Clamp(num, 0f, 20f) * ((BehaviorComponent)this).Formation.QuerySystem.InfantryUnitRatio;
	}
}
