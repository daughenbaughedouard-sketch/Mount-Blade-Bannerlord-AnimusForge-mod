using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class MissionCaravanOrVillagerTacticsHandler : MissionLogic
{
	public override void EarlyStart()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		foreach (Team item in (List<Team>)(object)Mission.Current.Teams)
		{
			if (item.HasTeamAi && (((IEnumerable<MapEventParty>)MapEvent.PlayerMapEvent.PartiesOnSide(item.Side)).Any((MapEventParty p) => p.Party.IsMobile && p.Party.MobileParty.IsCaravan) || (MapEvent.PlayerMapEvent.MapEventSettlement == null && ((IEnumerable<MapEventParty>)MapEvent.PlayerMapEvent.PartiesOnSide(item.Side)).Any((MapEventParty p) => p.Party.IsMobile && p.Party.MobileParty.IsVillager))))
			{
				item.AddTacticOption((TacticComponent)new TacticDefensiveLine(item));
			}
		}
	}
}
