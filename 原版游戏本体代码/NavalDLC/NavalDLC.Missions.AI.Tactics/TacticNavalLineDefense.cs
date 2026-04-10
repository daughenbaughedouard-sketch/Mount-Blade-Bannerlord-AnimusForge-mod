using System.Collections.Generic;
using NavalDLC.Missions.AI.Behaviors;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.Tactics;

public class TacticNavalLineDefense : NavalTacticComponent
{
	private readonly TeamAINavalComponent _teamAINavalComponent;

	private readonly NavalShipsLogic _navalShipsLogic;

	public TacticNavalLineDefense(Team team)
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

	private void NavalDefensiveEngage()
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		int num = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count / 2;
		int num2 = num - 1;
		bool flag = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count > ((List<MissionShip>)(object)_teamAINavalComponent.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count;
		Formation val = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num];
		val.AI.ResetBehaviorWeights();
		NavalTacticComponent.SetDefaultNavalBehaviorWeights(val);
		val.AI.SetBehaviorWeight<BehaviorNavalEngageCorrespondingEnemy>(1f).SetTargetShipSideAndOrder(!flag, num);
		val.AI.SetBehaviorWeight<BehaviorNavalDefendInLine>(1f).SetTargetShipSideAndOrder(rightSide: true, num, isAnchor: true);
		val.AI.SetBehaviorWeight<BehaviorNavalSkirmish>(1f);
		val.AI.SetBehaviorWeight<BehaviorNavalRamming>(1f);
		for (int i = num + 1; i < ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count; i++)
		{
			Formation obj = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[i];
			obj.AI.ResetBehaviorWeights();
			NavalTacticComponent.SetDefaultNavalBehaviorWeights(obj);
			obj.AI.SetBehaviorWeight<BehaviorNavalEngageCorrespondingEnemy>(1f).SetTargetShipSideAndOrder(!flag, i);
			BehaviorNavalDefendInLine behaviorNavalDefendInLine = obj.AI.SetBehaviorWeight<BehaviorNavalDefendInLine>(1f);
			_navalShipsLogic.GetShip(val.Team.TeamSide, val.FormationIndex, out var _);
			behaviorNavalDefendInLine.SetTargetShipSideAndOrder(rightSide: true, i, isAnchor: false);
			obj.AI.SetBehaviorWeight<BehaviorNavalSkirmish>(1f);
			obj.AI.SetBehaviorWeight<BehaviorNavalRamming>(1f);
			val = obj;
		}
		if (num2 >= 0 && num2 < ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count)
		{
			val = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num2];
			val.AI.ResetBehaviorWeights();
			NavalTacticComponent.SetDefaultNavalBehaviorWeights(val);
			val.AI.SetBehaviorWeight<BehaviorNavalEngageCorrespondingEnemy>(1f).SetTargetShipSideAndOrder(flag, num2);
			val.AI.SetBehaviorWeight<BehaviorNavalDefendInLine>(1f).SetTargetShipSideAndOrder(rightSide: false, num2, isAnchor: false);
			val.AI.SetBehaviorWeight<BehaviorNavalSkirmish>(1f);
			val.AI.SetBehaviorWeight<BehaviorNavalRamming>(1f);
			for (int num3 = num2 - 1; num3 >= 0; num3--)
			{
				Formation obj2 = ((List<Formation>)(object)_teamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num3];
				obj2.AI.ResetBehaviorWeights();
				NavalTacticComponent.SetDefaultNavalBehaviorWeights(obj2);
				obj2.AI.SetBehaviorWeight<BehaviorNavalEngageCorrespondingEnemy>(1f).SetTargetShipSideAndOrder(flag, num3);
				BehaviorNavalDefendInLine behaviorNavalDefendInLine2 = obj2.AI.SetBehaviorWeight<BehaviorNavalDefendInLine>(1f);
				_navalShipsLogic.GetShip(val.Team.TeamSide, val.FormationIndex, out var _);
				behaviorNavalDefendInLine2.SetTargetShipSideAndOrder(rightSide: false, num3, isAnchor: false);
				obj2.AI.SetBehaviorWeight<BehaviorNavalSkirmish>(1f);
				obj2.AI.SetBehaviorWeight<BehaviorNavalRamming>(1f);
				val = obj2;
			}
		}
	}

	private void NavalDefensivePositioning()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		int num = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count / 2;
		int num2 = num - 1;
		_ = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count;
		_ = ((List<MissionShip>)(object)TeamAINavalComponent.TeamNavalQuerySystem.EnemyShipsWithFormationsInLeftToRightOrder).Count;
		Formation val = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num];
		val.AI.ResetBehaviorWeights();
		NavalTacticComponent.SetDefaultNavalBehaviorWeights(val);
		val.AI.SetBehaviorWeight<BehaviorNavalDefendInLine>(1f).SetTargetShipSideAndOrder(rightSide: true, num, isAnchor: true);
		NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		for (int i = num + 1; i < ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count; i++)
		{
			Formation obj = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[i];
			obj.AI.ResetBehaviorWeights();
			NavalTacticComponent.SetDefaultNavalBehaviorWeights(obj);
			BehaviorNavalDefendInLine behaviorNavalDefendInLine = obj.AI.SetBehaviorWeight<BehaviorNavalDefendInLine>(1f);
			missionBehavior.GetShip(val.Team.TeamSide, val.FormationIndex, out var _);
			behaviorNavalDefendInLine.SetTargetShipSideAndOrder(rightSide: true, i, isAnchor: false);
			val = obj;
		}
		if (num2 >= 0 && num2 < ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder).Count)
		{
			val = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num2];
			val.AI.ResetBehaviorWeights();
			NavalTacticComponent.SetDefaultNavalBehaviorWeights(val);
			val.AI.SetBehaviorWeight<BehaviorNavalDefendInLine>(1f).SetTargetShipSideAndOrder(rightSide: false, num2, isAnchor: false);
			for (int num3 = num2 - 1; num3 >= 0; num3--)
			{
				Formation obj2 = ((List<Formation>)(object)TeamAINavalComponent.TeamNavalQuerySystem.FormationsInShipsInLeftToRightOrder)[num3];
				obj2.AI.ResetBehaviorWeights();
				NavalTacticComponent.SetDefaultNavalBehaviorWeights(obj2);
				BehaviorNavalDefendInLine behaviorNavalDefendInLine2 = obj2.AI.SetBehaviorWeight<BehaviorNavalDefendInLine>(1f);
				missionBehavior.GetShip(val.Team.TeamSide, val.FormationIndex, out var _);
				behaviorNavalDefendInLine2.SetTargetShipSideAndOrder(rightSide: false, num3, isAnchor: false);
				val = obj2;
			}
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
						NavalDefensiveEngage();
					}
					else if (!_teamAINavalComponent.UseSpawnPathApproachPosition || flag || ((TacticComponent)this).IsTacticReapplyNeeded)
					{
						NavalDefensivePositioning();
					}
				}
				((TacticComponent)this).IsTacticReapplyNeeded = false;
			}
		}
		((TacticComponent)this).TickOccasionally();
	}

	protected override float GetTacticWeight()
	{
		if (((TacticComponent)this).Team.TeamAI.IsDefenseApplicable)
		{
			return 1.5f;
		}
		return 0f;
	}
}
