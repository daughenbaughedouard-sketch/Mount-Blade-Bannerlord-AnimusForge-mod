using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.CustomBattle;

public class CPUBenchmarkMissionSpawnHandler : MissionLogic
{
	private MissionAgentSpawnLogic _missionAgentSpawnLogic;

	private CustomBattleCombatant _defenderParty;

	private CustomBattleCombatant _attackerParty;

	public CPUBenchmarkMissionSpawnHandler()
	{
	}

	public CPUBenchmarkMissionSpawnHandler(CustomBattleCombatant defenderParty, CustomBattleCombatant attackerParty)
	{
		_defenderParty = defenderParty;
		_attackerParty = attackerParty;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_missionAgentSpawnLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentSpawnLogic>();
	}

	public override void AfterStart()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		int numberOfHealthyMembers = _defenderParty.NumberOfHealthyMembers;
		int numberOfHealthyMembers2 = _attackerParty.NumberOfHealthyMembers;
		((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)2).SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
		((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)0).SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.GetFormation((FormationClass)2).SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.GetFormation((FormationClass)0).SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
		_missionAgentSpawnLogic.SetSpawnHorses((BattleSideEnum)0, true);
		_missionAgentSpawnLogic.SetSpawnHorses((BattleSideEnum)1, true);
		MissionSpawnSettings val = MissionSpawnSettings.CreateDefaultSpawnSettings();
		_missionAgentSpawnLogic.InitWithSinglePhase(numberOfHealthyMembers, numberOfHealthyMembers2, numberOfHealthyMembers, numberOfHealthyMembers2, true, true, ref val);
	}
}
