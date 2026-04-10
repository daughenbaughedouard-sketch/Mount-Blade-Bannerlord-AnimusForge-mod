using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class MissionSettlementPrepareLogic : MissionLogic
{
	public override void AfterStart()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Campaign.Current.GameMode == 1 && Settlement.CurrentSettlement != null && (Settlement.CurrentSettlement.IsTown || Settlement.CurrentSettlement.IsCastle))
		{
			OpenGates();
		}
	}

	private void OpenGates()
	{
		foreach (CastleGate item in MBExtensions.FindAllWithType<CastleGate>((IEnumerable<MissionObject>)Mission.Current.ActiveMissionObjects).ToList())
		{
			item.OpenDoorAndDisableGateForCivilianMission();
		}
	}
}
