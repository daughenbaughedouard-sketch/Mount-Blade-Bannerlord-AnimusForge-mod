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

public sealed class BehaviorNavalApproachInLine : NavalBehaviorComponent
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

	private MBReadOnlyList<ShipAttachmentPointMachine> _formationShipAttachmentPointMachines;

	private TeamAINavalComponent _navalTeamAI;

	private ShipDefenseState _currentState;

	private MissionShip _leftAllyShip;

	private MissionShip _rightAllyShip;

	private MissionShip _helpedAllyShip;

	private int _navalLineOrder;

	private bool _actualRightSide;

	private MissionShip _allyShip;

	private bool _tacticallyOnRightSide;

	private bool _isAnchor;

	private bool _hasPulledAhead;

	private NavalBehaviorBoardShipSubtask _boardShipSubtask;

	public BehaviorNavalApproachInLine(Formation formation)
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
		_formationShipAttachmentPointMachines = (MBReadOnlyList<ShipAttachmentPointMachine>)(object)Extensions.ToMBList<ShipAttachmentPointMachine>(from ce in source
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
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = _formationMainShip.GlobalFrame;
		Vec2 desiredPosition = ((Vec3)(ref globalFrame.origin)).AsVec2;
		globalFrame = _formationMainShip.GlobalFrame;
		Vec2 direction = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		MissionShip boardingTargetShip = null;
		switch (_currentState)
		{
		case ShipDefenseState.StandInLine:
		{
			Vec2 val = _navalTeamAI.TeamNavalQuerySystem.AverageEnemyShipPosition - _navalTeamAI.TeamNavalQuerySystem.AverageShipPosition;
			Vec2 val2 = ((Vec2)(ref val)).Normalized();
			if (_isAnchor)
			{
				Vec2 val3 = _navalTeamAI.TeamNavalQuerySystem.AverageShipPosition * (float)((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.TeamShipsWithFormationsInLeftToRightOrder).Count;
				Vec2 val4 = val3;
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)_formationMainShip).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				val3 = val4 - ((Vec3)(ref globalPosition)).AsVec2;
				val3 /= (float)(((List<MissionShip>)(object)_navalTeamAI.TeamNavalQuerySystem.TeamShipsWithFormationsInLeftToRightOrder).Count - 1);
				gameEntity = ((ScriptComponentBehavior)_formationMainShip).GameEntity;
				globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				Vec2 val5 = ((Vec3)(ref globalPosition)).AsVec2 - val3;
				float num = ((Vec2)(ref val2)).DotProduct(val5);
				bool flag = false;
				if (_navalTeamAI.UseSpawnPathApproachPosition)
				{
					_navalTeamAI.GetRiverApproachPosition(out desiredPosition, out direction);
					globalFrame = _formationMainShip.GlobalFrame;
					val = ((Vec3)(ref globalFrame.origin)).AsVec2;
					int num2;
					if (((Vec2)(ref val)).DistanceSquared(desiredPosition) > 900f)
					{
						globalFrame = _formationMainShip.GlobalFrame;
						val = ((Vec3)(ref globalFrame.origin)).AsVec2;
						num2 = ((((Vec2)(ref val)).Distance(_navalTeamAI.TeamNavalQuerySystem.AverageEnemyShipPosition) - ((Vec2)(ref desiredPosition)).Distance(_navalTeamAI.TeamNavalQuerySystem.AverageEnemyShipPosition) >= 50f) ? 1 : 0);
					}
					else
					{
						num2 = 0;
					}
					flag = (byte)num2 != 0;
				}
				if (flag)
				{
					break;
				}
				if (_hasPulledAhead)
				{
					if (num <= 10f)
					{
						_hasPulledAhead = false;
					}
				}
				else if (num >= 20f)
				{
					_hasPulledAhead = true;
				}
				direction = val2;
				if (_hasPulledAhead)
				{
					globalFrame = _formationMainShip.GlobalFrame;
					desiredPosition = ((Vec3)(ref globalFrame.origin)).AsVec2 + direction * 15f;
				}
				else
				{
					globalFrame = _formationMainShip.GlobalFrame;
					desiredPosition = ((Vec3)(ref globalFrame.origin)).AsVec2 + direction * 450f;
				}
				break;
			}
			Vec3 val6 = _formationMainShip.GlobalFrame.origin - _allyShip.GlobalFrame.origin;
			((Vec3)(ref val6)).Normalize();
			Vec2 val7;
			if (_actualRightSide)
			{
				if (!_navalTeamAI.UseSpawnPathApproachPosition)
				{
					val7 = ((Vec2)(ref val2)).RightVec();
				}
				else
				{
					globalFrame = _allyShip.GlobalFrame;
					val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
					val7 = ((Vec2)(ref val)).RightVec();
				}
			}
			else if (!_navalTeamAI.UseSpawnPathApproachPosition)
			{
				val7 = ((Vec2)(ref val2)).LeftVec();
			}
			else
			{
				globalFrame = _allyShip.GlobalFrame;
				val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				val7 = ((Vec2)(ref val)).LeftVec();
			}
			globalFrame = _allyShip.GlobalFrame;
			desiredPosition = ((Vec3)(ref globalFrame.origin)).AsVec2 + val2 * 30f + val7 * 30f;
			Vec2 val8 = desiredPosition;
			globalFrame = _formationMainShip.GlobalFrame;
			val = val8 - ((Vec3)(ref globalFrame.origin)).AsVec2;
			float num3 = ((Vec2)(ref val)).DotProduct(val2);
			if (num3 < 0f)
			{
				desiredPosition += num3 * val2;
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

	protected override void OnBehaviorActivatedAux()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
		_boardShipSubtask.SetOwnerShip(_formationMainShip);
		_boardShipSubtask.SetTargetShipAndSide(null, _tacticallyOnRightSide);
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
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
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
		if (_formationMainShip.Formation != ((BehaviorComponent)this).Formation && _navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out var ship))
		{
			_formationMainShip = ship;
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
