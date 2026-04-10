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

public sealed class BehaviorNavalDefendInLine : NavalBehaviorComponent
{
	private enum ShipDefenseState
	{
		StandInLine,
		BeingBoarded,
		GoingToHelp,
		HelpingFriend,
		HelpingFinishedStuckBoarded
	}

	private const float DistanceToKeepWithAllyShip = 30f;

	private NavalShipsLogic _navalShipsLogic;

	private MissionShip _formationMainShip;

	private MBReadOnlyList<ShipAttachmentMachine> _formationShipAttachmentMachines;

	private TeamAINavalComponent _navalTeamAI;

	private ShipDefenseState _currentState;

	private MissionShip _leftAllyShip;

	private MissionShip _rightAllyShip;

	private MissionShip _helpedAllyShip;

	private int _navalLineOrder;

	private bool _swapSide;

	private bool _actualRightSide;

	private MissionShip _allyShip;

	private bool _tacticallyOnRightSide;

	private bool _isAnchor;

	private NavalBehaviorBoardShipSubtask _boardShipSubtask;

	public BehaviorNavalDefendInLine(Formation formation)
		: base(formation)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).BehaviorCoherence = 0.8f;
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
		List<WeakGameEntity> source = new List<WeakGameEntity>();
		_formationShipAttachmentMachines = (MBReadOnlyList<ShipAttachmentMachine>)(object)Extensions.ToMBList<ShipAttachmentMachine>(from ce in source
			where ((WeakGameEntity)(ref ce)).HasScriptOfType<ShipAttachmentMachine>()
			select ((WeakGameEntity)(ref ce)).GetFirstScriptOfType<ShipAttachmentMachine>());
		Extensions.ToMBList<ShipAttachmentPointMachine>(from ce in source
			where ((WeakGameEntity)(ref ce)).HasScriptOfType<ShipAttachmentPointMachine>()
			select ((WeakGameEntity)(ref ce)).GetFirstScriptOfType<ShipAttachmentPointMachine>());
		_navalTeamAI = ((BehaviorComponent)this).Formation.Team.TeamAI as TeamAINavalComponent;
		((BehaviorComponent)this).CalculateCurrentOrder();
		_boardShipSubtask = new NavalBehaviorBoardShipSubtask(_formationMainShip);
	}

	public override void RefreshShipReferences()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
		_leftAllyShip = null;
		_rightAllyShip = null;
		if (_navalLineOrder >= ((List<Formation>)(object)_navalTeamAI.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count || _navalLineOrder < 0)
		{
			_navalLineOrder = 0;
		}
		SetTargetShipSideAndOrder(_tacticallyOnRightSide, _navalLineOrder, _isAnchor);
		if (_helpedAllyShip != null)
		{
			_helpedAllyShip = (_tacticallyOnRightSide ? _leftAllyShip : _rightAllyShip);
		}
	}

	public void SetTargetShipSideAndOrder(bool rightSide, int navalLineOrder, bool isAnchor)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if (((List<Formation>)(object)_navalTeamAI.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count > 0)
		{
			_isAnchor = isAnchor;
			_tacticallyOnRightSide = rightSide;
			_actualRightSide = rightSide;
			_navalLineOrder = navalLineOrder;
			Formation val = ((navalLineOrder > 0) ? ((IEnumerable<Formation>)_navalTeamAI.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).ElementAt(_navalLineOrder - 1) : null);
			Formation val2 = ((navalLineOrder < ((List<Formation>)(object)_navalTeamAI.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count - 1) ? ((IEnumerable<Formation>)_navalTeamAI.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).ElementAt(_navalLineOrder + 1) : null);
			if (val != null)
			{
				_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, val.FormationIndex, out _leftAllyShip);
			}
			if (val2 != null)
			{
				_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, val2.FormationIndex, out _rightAllyShip);
			}
			if (_tacticallyOnRightSide)
			{
				_allyShip = _leftAllyShip;
			}
			else
			{
				_allyShip = _rightAllyShip;
			}
		}
	}

	protected override void CalculateCurrentOrder()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (_navalShipsLogic == null || ((BehaviorComponent)this).Formation.CachedClosestEnemyFormation == null || _allyShip == null)
		{
			((BehaviorComponent)this).CurrentOrder = MovementOrder.MovementOrderStop;
		}
		else if (_formationMainShip != null && _formationMainShip.SearchShipConnection(null, isDirect: true, findEnemy: true, enforceActive: true, acceptNotBridgedConnections: false))
		{
			if (_currentState == ShipDefenseState.BeingBoarded)
			{
				((BehaviorComponent)this).CurrentOrder = _formationMainShip.GetMovementOrderToRallyPoint();
				((BehaviorComponent)this).CurrentFacingOrder = _formationMainShip.GetFacingOrderToRallyPoint();
			}
			else
			{
				((BehaviorComponent)this).CurrentOrder = MovementOrder.MovementOrderCharge;
				((BehaviorComponent)this).CurrentFacingOrder = FacingOrder.FacingOrderLookAtEnemy;
			}
		}
		else
		{
			((BehaviorComponent)this).CurrentOrder = MovementOrder.MovementOrderStop;
		}
	}

	private void CalculateAndSetShipOrders()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = _formationMainShip.GlobalFrame;
		Vec2 desiredPosition = ((Vec3)(ref globalFrame.origin)).AsVec2;
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Vec2 direction = ((Vec2)(ref val)).Normalized();
		MissionShip boardingTargetShip = null;
		switch (_currentState)
		{
		case ShipDefenseState.StandInLine:
		{
			val = _navalTeamAI.TeamNavalQuerySystem.AverageEnemyShipPosition - _navalTeamAI.TeamNavalQuerySystem.AverageShipPosition;
			Vec2 val2 = ((Vec2)(ref val)).Normalized();
			MatrixFrame globalFrame2;
			if (_isAnchor)
			{
				bool flag = false;
				if (_navalTeamAI.UseSpawnPathApproachPosition)
				{
					_navalTeamAI.GetRiverApproachPosition(out desiredPosition, out direction);
					globalFrame2 = _formationMainShip.GlobalFrame;
					val = ((Vec3)(ref globalFrame2.origin)).AsVec2;
					int num;
					if (((Vec2)(ref val)).DistanceSquared(desiredPosition) > 900f)
					{
						globalFrame2 = _formationMainShip.GlobalFrame;
						val = ((Vec3)(ref globalFrame2.origin)).AsVec2;
						num = ((((Vec2)(ref val)).Distance(_navalTeamAI.TeamNavalQuerySystem.AverageEnemyShipPosition) - ((Vec2)(ref desiredPosition)).Distance(_navalTeamAI.TeamNavalQuerySystem.AverageEnemyShipPosition) >= 50f) ? 1 : 0);
					}
					else
					{
						num = 0;
					}
					flag = (byte)num != 0;
				}
				if (!flag)
				{
					direction = val2;
					desiredPosition = ((Vec3)(ref globalFrame.origin)).AsVec2 + direction * 15f;
				}
				break;
			}
			MatrixFrame globalFrame3 = _allyShip.GlobalFrame;
			Vec3 val3 = globalFrame.origin - globalFrame3.origin;
			((Vec3)(ref val3)).Normalize();
			Vec2 val4;
			if (_actualRightSide)
			{
				if (!_navalTeamAI.UseSpawnPathApproachPosition)
				{
					val4 = ((Vec2)(ref val2)).RightVec();
				}
				else
				{
					globalFrame2 = _allyShip.GlobalFrame;
					val = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
					val4 = ((Vec2)(ref val)).RightVec();
				}
			}
			else if (!_navalTeamAI.UseSpawnPathApproachPosition)
			{
				val4 = ((Vec2)(ref val2)).LeftVec();
			}
			else
			{
				globalFrame2 = _allyShip.GlobalFrame;
				val = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
				val4 = ((Vec2)(ref val)).LeftVec();
			}
			globalFrame2 = _allyShip.GlobalFrame;
			desiredPosition = ((Vec3)(ref globalFrame2.origin)).AsVec2 + val2 * 10f + val4 * 30f;
			Vec2 val5 = desiredPosition;
			globalFrame2 = _formationMainShip.GlobalFrame;
			val = val5 - ((Vec3)(ref globalFrame2.origin)).AsVec2;
			float num2 = ((Vec2)(ref val)).DotProduct(val2);
			if (num2 < 0f)
			{
				desiredPosition += num2 * val2;
			}
			direction = val2;
			break;
		}
		case ShipDefenseState.GoingToHelp:
		case ShipDefenseState.HelpingFriend:
			_boardShipSubtask.CalculateShipOrders(out desiredPosition, out direction, out boardingTargetShip);
			break;
		}
		if (_formationMainShip.IsFormationAndShipAIControlled)
		{
			_formationMainShip.ShipOrder.SetShipMovementOrder(desiredPosition, in direction);
			_formationMainShip.ShipOrder.SetBoardingTargetShip(boardingTargetShip);
		}
	}

	private void CheckAndSwitchState()
	{
		if (((BehaviorComponent)this).Formation.CachedClosestEnemyFormation == null)
		{
			return;
		}
		switch (_currentState)
		{
		case ShipDefenseState.StandInLine:
			if (_formationMainShip.SearchShipConnection(null, isDirect: true, findEnemy: true, enforceActive: true, acceptNotBridgedConnections: true))
			{
				_currentState = ShipDefenseState.BeingBoarded;
			}
			else if (_leftAllyShip != null && _leftAllyShip.SearchShipConnection(null, isDirect: true, findEnemy: true, enforceActive: true, acceptNotBridgedConnections: true))
			{
				_currentState = ShipDefenseState.GoingToHelp;
				_helpedAllyShip = _leftAllyShip;
				_boardShipSubtask.SetTargetShipAndSide(_helpedAllyShip, _tacticallyOnRightSide);
			}
			else if (_rightAllyShip != null && _rightAllyShip.SearchShipConnection(null, isDirect: true, findEnemy: true, enforceActive: true, acceptNotBridgedConnections: true))
			{
				_currentState = ShipDefenseState.GoingToHelp;
				_helpedAllyShip = _rightAllyShip;
				_boardShipSubtask.SetTargetShipAndSide(_helpedAllyShip, _tacticallyOnRightSide);
			}
			else if (_formationMainShip.GetIsConnected())
			{
				_currentState = ShipDefenseState.HelpingFinishedStuckBoarded;
			}
			break;
		case ShipDefenseState.BeingBoarded:
			if (!_formationMainShip.GetIsConnected())
			{
				_currentState = ShipDefenseState.StandInLine;
			}
			else if (!_formationMainShip.SearchShipConnection(null, isDirect: true, findEnemy: true, enforceActive: true, acceptNotBridgedConnections: true))
			{
				_currentState = ShipDefenseState.HelpingFinishedStuckBoarded;
			}
			else if (!_formationMainShip.ShipOrder.IsEnemyOnShip)
			{
				_formationMainShip.SearchShipConnection(null, isDirect: true, findEnemy: false, enforceActive: true, acceptNotBridgedConnections: true);
			}
			break;
		case ShipDefenseState.GoingToHelp:
			if (_formationMainShip.SearchShipConnection(_helpedAllyShip, isDirect: true, findEnemy: false, enforceActive: false, acceptNotBridgedConnections: true))
			{
				_currentState = ShipDefenseState.HelpingFriend;
			}
			else if (_helpedAllyShip == null || !_helpedAllyShip.SearchShipConnection(null, isDirect: true, findEnemy: true, enforceActive: true, acceptNotBridgedConnections: true))
			{
				_currentState = ShipDefenseState.StandInLine;
				_helpedAllyShip = null;
			}
			else if (_formationMainShip.SearchShipConnection(null, isDirect: true, findEnemy: true, enforceActive: true, acceptNotBridgedConnections: true))
			{
				_currentState = ShipDefenseState.BeingBoarded;
				_helpedAllyShip = null;
			}
			else if (_formationMainShip.GetIsConnected())
			{
				_currentState = ShipDefenseState.HelpingFinishedStuckBoarded;
				_helpedAllyShip = null;
			}
			break;
		case ShipDefenseState.HelpingFriend:
			if (!_formationMainShip.SearchShipConnection(_helpedAllyShip, isDirect: true, findEnemy: false, enforceActive: false, acceptNotBridgedConnections: true))
			{
				_currentState = ShipDefenseState.GoingToHelp;
				_boardShipSubtask.SetTargetShipAndSide(_helpedAllyShip, _tacticallyOnRightSide);
			}
			break;
		}
	}

	public override void OnDeploymentFinished()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).OnDeploymentFinished();
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
		_currentState = ShipDefenseState.StandInLine;
	}

	public override void ResetBehavior()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).ResetBehavior();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
		_currentState = ShipDefenseState.StandInLine;
	}

	protected override void OnBehaviorActivatedAux()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
		_boardShipSubtask.SetOwnerShip(_formationMainShip);
		_boardShipSubtask.SetTargetShipAndSide(null, _tacticallyOnRightSide ^ _swapSide);
		_currentState = ShipDefenseState.StandInLine;
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
		CheckAndSwitchState();
		CalculateAndSetShipOrders();
		((BehaviorComponent)this).CalculateCurrentOrder();
		((BehaviorComponent)this).Formation.SetMovementOrder(((BehaviorComponent)this).CurrentOrder);
		((BehaviorComponent)this).Formation.SetFacingOrder(((BehaviorComponent)this).CurrentFacingOrder);
	}

	protected override float GetAiWeight()
	{
		float num = 1f;
		if (((BehaviorComponent)this).Formation.CachedClosestEnemyFormation != null)
		{
			if (((BehaviorComponent)this).Formation.QuerySystem.FormationMeleeFightingPower > 0f)
			{
				float num2 = ((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.FormationMeleeFightingPower / ((BehaviorComponent)this).Formation.QuerySystem.FormationMeleeFightingPower;
				num *= ((num2 >= 1f) ? num2 : 1f);
			}
			else
			{
				num = 2f;
			}
		}
		float num3 = 1f / ((BehaviorComponent)this).Formation.Team.QuerySystem.TotalPowerRatio;
		num *= ((num3 >= 1f) ? num3 : 1f);
		return ((_currentState == ShipDefenseState.HelpingFinishedStuckBoarded) ? 0f : 1f) * num * 2f * ((_currentState != ShipDefenseState.HelpingFinishedStuckBoarded && _currentState != ShipDefenseState.StandInLine) ? 5f : 1f);
	}
}
