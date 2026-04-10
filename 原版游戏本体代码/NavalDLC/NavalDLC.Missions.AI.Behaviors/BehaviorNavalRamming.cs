using System;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.Behaviors;

public sealed class BehaviorNavalRamming : NavalBehaviorComponent
{
	private NavalShipsLogic _navalShipsLogic;

	private MissionShip _formationMainShip;

	private MissionShip _ignoredShip;

	private bool _isRammingActive;

	public BehaviorNavalRamming(Formation formation)
		: base(formation)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).BehaviorCoherence = 0.8f;
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
	}

	private void CalculateAndSetShipOrders()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (((BehaviorComponent)this).Formation.CachedClosestEnemyFormation == null || !_formationMainShip.IsFormationAndShipAIControlled)
		{
			return;
		}
		MissionShip missionShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.Team.Team.TeamSide, ((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.Formation.FormationIndex).MissionShip;
		Vec3 origin = missionShip.GlobalFrame.origin;
		ShipOrder shipOrder = _formationMainShip.ShipOrder;
		Vec3 val = origin + (origin - _formationMainShip.GlobalFrame.origin) * 2f;
		shipOrder.SetShipMovementOrder(((Vec3)(ref val)).AsVec2);
		if (_ignoredShip != missionShip)
		{
			if (_ignoredShip != null)
			{
				_formationMainShip.AIController.RemoveShipFromCollisionIgnoreListOnAccountOfRamming(_ignoredShip);
			}
			_formationMainShip.AIController.AddShipToCollisionIgnoreListOnAccountOfRamming(missionShip);
			_ignoredShip = missionShip;
		}
	}

	public override void OnDeploymentFinished()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).OnDeploymentFinished();
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
	}

	public override void RefreshShipReferences()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
	}

	public override void ResetBehavior()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((BehaviorComponent)this).ResetBehavior();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
	}

	protected override void OnBehaviorActivatedAux()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_formationMainShip = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex).MissionShip;
		_formationMainShip.ShipOrder.SetBoardingTargetShip(null);
		_formationMainShip.ShipOrder.SetCutLoose(enable: false);
		_formationMainShip.ShipOrder.SetOrderOarsmenLevel(2);
		((BehaviorComponent)this).Formation.SetMovementOrder(((BehaviorComponent)this).CurrentOrder);
		((BehaviorComponent)this).Formation.SetFacingOrder(((BehaviorComponent)this).CurrentFacingOrder);
		((BehaviorComponent)this).Formation.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
		((BehaviorComponent)this).Formation.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
		((BehaviorComponent)this).Formation.SetFormOrder(FormOrder.FormOrderWide, true);
		_isRammingActive = true;
		_ignoredShip = null;
	}

	public override void OnBehaviorCanceled()
	{
		_isRammingActive = false;
		if (_ignoredShip != null)
		{
			_formationMainShip.AIController.RemoveShipFromCollisionIgnoreListOnAccountOfRamming(_ignoredShip);
		}
	}

	public override void TickOccasionally()
	{
		if (_navalShipsLogic == null)
		{
			_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
			if (_navalShipsLogic == null)
			{
				return;
			}
		}
		CalculateAndSetShipOrders();
	}

	protected override float GetAiWeight()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		if (_formationMainShip.Formation != ((BehaviorComponent)this).Formation)
		{
			_navalShipsLogic.GetShip(((BehaviorComponent)this).Formation.Team.TeamSide, ((BehaviorComponent)this).Formation.FormationIndex, out _formationMainShip);
		}
		if (((BehaviorComponent)this).Formation.CachedClosestEnemyFormation != null)
		{
			MatrixFrame globalFrame = _navalShipsLogic.GetShipAssignment(((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.Team.Team.TeamSide, ((BehaviorComponent)this).Formation.CachedClosestEnemyFormation.Formation.FormationIndex).MissionShip.GlobalFrame;
			Vec3 val = globalFrame.origin - _formationMainShip.GlobalFrame.origin;
			Vec2 val2 = ((Vec3)(ref val)).AsVec2;
			val2 = ((Vec2)(ref val2)).Normalized();
			Vec3 linearVelocity = _formationMainShip.Physics.LinearVelocity;
			Vec2 asVec = ((Vec3)(ref linearVelocity)).AsVec2;
			float num = ((Vec2)(ref val2)).DotProduct(((Vec2)(ref asVec)).Normalized());
			if (num > 0.9f * (_isRammingActive ? 0.5f : 1f))
			{
				float num2 = num * 1.5f;
				linearVelocity = _formationMainShip.Physics.LinearVelocity;
				val2 = ((Vec3)(ref linearVelocity)).AsVec2;
				val2 = ((Vec2)(ref val2)).Normalized();
				asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				num = Math.Abs(((Vec2)(ref val2)).DotProduct(((Vec2)(ref asVec)).Normalized()));
				if (num <= 0.1f * (_isRammingActive ? 2f : 1f))
				{
					linearVelocity = _formationMainShip.Physics.LinearVelocity;
					float length = ((Vec3)(ref linearVelocity)).Length;
					if (length > 3f * (_isRammingActive ? 0.5f : 1f))
					{
						val2 = ((Vec3)(ref val)).AsVec2;
						float num3 = ((Vec2)(ref val2)).Length / length;
						if (num3 < 30f * (_isRammingActive ? 2f : 1f))
						{
							if (num3 <= 10f)
							{
								num3 = 10f;
							}
							float num4 = 1.5f - num;
							float num5 = length / 3f;
							float num6 = 30f / num3;
							return num2 * num4 * num5 * num6;
						}
					}
				}
			}
		}
		return 0f;
	}
}
