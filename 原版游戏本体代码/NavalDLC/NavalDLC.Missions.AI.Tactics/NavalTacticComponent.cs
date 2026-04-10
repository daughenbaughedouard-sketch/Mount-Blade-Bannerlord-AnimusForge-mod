using System.Collections.Generic;
using NavalDLC.Missions.AI.Behaviors;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.Tactics;

public abstract class NavalTacticComponent : TacticComponent
{
	private const float EngagementDistanceSquared = 40000f;

	protected readonly TeamAINavalComponent TeamAINavalComponent;

	protected bool HasBattleBeenJoined;

	protected MBReadOnlyList<Formation> _shipOrderCached;

	public NavalTacticComponent(Team team)
		: base(team)
	{
		TeamAINavalComponent = team.TeamAI as TeamAINavalComponent;
		_shipOrderCached = new MBReadOnlyList<Formation>();
	}

	public static void SetDefaultNavalBehaviorWeights(Formation f)
	{
		f.AI.SetBehaviorWeight<BehaviorNavalRemoveConnection>(1f);
	}

	protected void NavalApproach()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		int num = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count / 2;
		int num2 = num - 1;
		_ = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count;
		_ = ((List<MissionShip>)(object)TeamAINavalComponent.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count;
		Formation val = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num];
		val.AI.ResetBehaviorWeights();
		SetDefaultNavalBehaviorWeights(val);
		val.AI.SetBehaviorWeight<BehaviorNavalApproachInLine>(1f).SetTargetShipSideAndOrder(rightSide: true, num, isAnchor: true);
		NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		for (int i = num + 1; i < ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count; i++)
		{
			Formation obj = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[i];
			obj.AI.ResetBehaviorWeights();
			SetDefaultNavalBehaviorWeights(obj);
			BehaviorNavalApproachInLine behaviorNavalApproachInLine = obj.AI.SetBehaviorWeight<BehaviorNavalApproachInLine>(1f);
			missionBehavior.GetShip(val.Team.TeamSide, val.FormationIndex, out var _);
			behaviorNavalApproachInLine.SetTargetShipSideAndOrder(rightSide: true, i, isAnchor: false);
			val = obj;
		}
		if (num2 >= 0 && num2 < ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count)
		{
			val = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num2];
			val.AI.ResetBehaviorWeights();
			SetDefaultNavalBehaviorWeights(val);
			val.AI.SetBehaviorWeight<BehaviorNavalApproachInLine>(1f).SetTargetShipSideAndOrder(rightSide: false, num2, isAnchor: false);
			for (int num3 = num2 - 1; num3 >= 0; num3--)
			{
				Formation obj2 = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num3];
				obj2.AI.ResetBehaviorWeights();
				SetDefaultNavalBehaviorWeights(obj2);
				BehaviorNavalApproachInLine behaviorNavalApproachInLine2 = obj2.AI.SetBehaviorWeight<BehaviorNavalApproachInLine>(1f);
				missionBehavior.GetShip(val.Team.TeamSide, val.FormationIndex, out var _);
				behaviorNavalApproachInLine2.SetTargetShipSideAndOrder(rightSide: false, num3, isAnchor: false);
				val = obj2;
			}
		}
		if (TeamAINavalComponent.UseSpawnPathApproachPosition || !((TacticComponent)this).Team.IsAttacker)
		{
			return;
		}
		Vec2 globalWindVelocity = Mission.Current.Scene.GetGlobalWindVelocity();
		Vec2 val2 = TeamAINavalComponent.TeamNavalQuerySystem.AverageEnemyShipPosition - TeamAINavalComponent.TeamNavalQuerySystem.AverageShipPosition;
		Vec2 val3 = ((Vec2)(ref val2)).Normalized();
		val2 = ((Vec2)(ref globalWindVelocity)).Normalized();
		if (!(((Vec2)(ref val2)).DotProduct(val3) > 0.5f))
		{
			return;
		}
		foreach (MissionShip item in (List<MissionShip>)(object)TeamAINavalComponent.TeamNavalQuerySystem.TeamShipsWithFormationsInLeftToRightOrder)
		{
			item.ShipOrder.SetEnforcedSailUsage(enforce: true);
		}
	}

	protected void CheckAndSetHasBattleBeenJoined()
	{
		if (TeamAINavalComponent.TeamNavalQuerySystem.ClosestDistanceSquaredToEnemyShip <= 40000f || ((TacticComponent)this).Team.QuerySystem.DeathByRangedCount > 10 || (float)((TacticComponent)this).Team.QuerySystem.DeathByRangedCount > (float)((TacticComponent)this).Team.QuerySystem.AllyUnitCount * 0.1f)
		{
			HasBattleBeenJoined = true;
			return;
		}
		foreach (MissionShip item in (List<MissionShip>)(object)TeamAINavalComponent.TeamNavalQuerySystem.TeamShipsWithFormationsInLeftToRightOrder)
		{
			if (item.GetIsConnectedToEnemy())
			{
				HasBattleBeenJoined = true;
				break;
			}
		}
	}

	protected bool HasShipOrderChanged()
	{
		for (int i = 0; i < ((List<Formation>)(object)_shipOrderCached).Count && i < ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count; i++)
		{
			if (((List<Formation>)(object)_shipOrderCached)[i] != ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[i])
			{
				return true;
			}
		}
		return false;
	}

	protected override void ManageFormationCounts()
	{
		((TacticComponent)this).ManageFormationCounts();
		TeamAINavalComponent.TeamNavalQuerySystem.ForceExpireSameSideShipLists();
	}
}
