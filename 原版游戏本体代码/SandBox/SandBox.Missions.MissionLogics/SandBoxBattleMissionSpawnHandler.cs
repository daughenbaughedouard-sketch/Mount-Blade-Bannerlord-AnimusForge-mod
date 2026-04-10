using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class SandBoxBattleMissionSpawnHandler : SandBoxMissionSpawnHandler
{
	public override void AfterStart()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		int numberOfInvolvedMen = _mapEvent.GetNumberOfInvolvedMen((BattleSideEnum)0);
		int numberOfInvolvedMen2 = _mapEvent.GetNumberOfInvolvedMen((BattleSideEnum)1);
		int num = numberOfInvolvedMen;
		int num2 = numberOfInvolvedMen2;
		_missionAgentSpawnLogic.SetSpawnHorses((BattleSideEnum)0, !_mapEvent.IsSiegeAssault);
		_missionAgentSpawnLogic.SetSpawnHorses((BattleSideEnum)1, !_mapEvent.IsSiegeAssault);
		MissionSpawnSettings val = SandBoxMissionSpawnHandler.CreateSandBoxBattleWaveSpawnSettings();
		_missionAgentSpawnLogic.InitWithSinglePhase(numberOfInvolvedMen, numberOfInvolvedMen2, num, num2, true, true, ref val);
	}
}
