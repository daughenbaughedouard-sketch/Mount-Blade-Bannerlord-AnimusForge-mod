using System.Collections.Generic;
using NavalDLC.Missions.AI.Behaviors;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.Tactics;

public class TacticNavalBalancedOffense : NavalTacticComponent
{
	private readonly TeamAINavalComponent _teamAINavalComponent;

	private readonly NavalShipsLogic _navalShipsLogic;

	public TacticNavalBalancedOffense(Team team)
		: base(team)
	{
		_teamAINavalComponent = team.TeamAI as TeamAINavalComponent;
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
	}

	protected override bool CheckAndSetAvailableFormationsChanged()
	{
		int aIControlledFormationCount = ((TacticComponent)this).Team.GetAIControlledFormationCount();
		bool num = aIControlledFormationCount != ((TacticComponent)this)._AIControlledFormationCount;
		if (num)
		{
			((TacticComponent)this)._AIControlledFormationCount = aIControlledFormationCount;
			((TacticComponent)this).IsTacticReapplyNeeded = true;
		}
		return num;
	}

	private void NavalEngage()
	{
		int num = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count / 2;
		int num2 = num - 1;
		bool flag = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count > ((List<MissionShip>)(object)_teamAINavalComponent.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count;
		Formation obj = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num];
		obj.AI.ResetBehaviorWeights();
		NavalTacticComponent.SetDefaultNavalBehaviorWeights(obj);
		obj.AI.SetBehaviorWeight<BehaviorNavalEngageCorrespondingEnemy>(1f).SetTargetShipSideAndOrder(!flag, num);
		obj.AI.SetBehaviorWeight<BehaviorNavalSkirmish>(1f);
		obj.AI.SetBehaviorWeight<BehaviorNavalRamming>(1f);
		for (int i = num + 1; i < ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count; i++)
		{
			Formation obj2 = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[i];
			obj2.AI.ResetBehaviorWeights();
			NavalTacticComponent.SetDefaultNavalBehaviorWeights(obj2);
			obj2.AI.SetBehaviorWeight<BehaviorNavalEngageCorrespondingEnemy>(1f).SetTargetShipSideAndOrder(!flag, i);
			obj2.AI.SetBehaviorWeight<BehaviorNavalSkirmish>(1f);
			obj2.AI.SetBehaviorWeight<BehaviorNavalRamming>(1f);
		}
		if (num2 >= 0 && num2 < ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count)
		{
			Formation obj3 = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num2];
			obj3.AI.ResetBehaviorWeights();
			NavalTacticComponent.SetDefaultNavalBehaviorWeights(obj3);
			obj3.AI.SetBehaviorWeight<BehaviorNavalEngageCorrespondingEnemy>(1f).SetTargetShipSideAndOrder(flag, num2);
			obj3.AI.SetBehaviorWeight<BehaviorNavalSkirmish>(1f);
			obj3.AI.SetBehaviorWeight<BehaviorNavalRamming>(1f);
			for (int num3 = num2 - 1; num3 >= 0; num3--)
			{
				Formation obj4 = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num3];
				obj4.AI.ResetBehaviorWeights();
				NavalTacticComponent.SetDefaultNavalBehaviorWeights(obj4);
				obj4.AI.SetBehaviorWeight<BehaviorNavalEngageCorrespondingEnemy>(1f).SetTargetShipSideAndOrder(flag, num3);
				obj4.AI.SetBehaviorWeight<BehaviorNavalSkirmish>(1f);
				obj4.AI.SetBehaviorWeight<BehaviorNavalRamming>(1f);
			}
		}
		foreach (MissionShip item in (List<MissionShip>)(object)TeamAINavalComponent.TeamNavalQuerySystem.TeamShipsWithFormationsInLeftToRightOrder)
		{
			item.ShipOrder.SetEnforcedSailUsage(enforce: false);
		}
	}

	public override void TickOccasionally()
	{
		if (((TacticComponent)this).AreFormationsCreated && ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count > 0 && ((List<MissionShip>)(object)_teamAINavalComponent.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count > 0)
		{
			bool flag = ((TacticComponent)this).CheckAndSetAvailableFormationsChanged();
			bool flag2 = flag || HasShipOrderChanged();
			if (!HasBattleBeenJoined)
			{
				CheckAndSetHasBattleBeenJoined();
				((TacticComponent)this).IsTacticReapplyNeeded = ((TacticComponent)this).IsTacticReapplyNeeded | HasBattleBeenJoined;
			}
			if (flag || flag2 || ((TacticComponent)this).IsTacticReapplyNeeded)
			{
				if (flag)
				{
					((TacticComponent)this).ManageFormationCounts();
				}
				if (flag2)
				{
					_shipOrderCached = (MBReadOnlyList<Formation>)(object)Extensions.ToMBList<Formation>((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder);
				}
				if (((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count > 0)
				{
					if (HasBattleBeenJoined)
					{
						NavalEngage();
					}
					else if (!_teamAINavalComponent.UseSpawnPathApproachPosition || flag || ((TacticComponent)this).IsTacticReapplyNeeded)
					{
						NavalApproach();
					}
				}
				((TacticComponent)this).IsTacticReapplyNeeded = false;
			}
		}
		((TacticComponent)this).TickOccasionally();
	}

	protected override float GetTacticWeight()
	{
		return MathF.Max(((TacticComponent)this).Team.QuerySystem.TotalPowerRatio, 0.1f);
	}
}
