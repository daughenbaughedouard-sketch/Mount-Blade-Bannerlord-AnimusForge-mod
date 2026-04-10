using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class MissionBasicTeamLogic : MissionLogic
{
	public override void EarlyStart()
	{
		((MissionBehavior)this).EarlyStart();
		InitializeTeams();
	}

	private void GetTeamColor(BattleSideEnum side, bool isPlayerAttacker, out uint teamColor1, out uint teamColor2)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		teamColor1 = uint.MaxValue;
		teamColor2 = uint.MaxValue;
		if ((int)Campaign.Current.GameMode != 1)
		{
			return;
		}
		if ((isPlayerAttacker && (int)side == 1) || (!isPlayerAttacker && (int)side == 0))
		{
			teamColor1 = Hero.MainHero.MapFaction.Color;
			teamColor2 = Hero.MainHero.MapFaction.Color2;
		}
		else if (MobileParty.MainParty.MapEvent != null)
		{
			if (MobileParty.MainParty.MapEvent.MapEventSettlement != null)
			{
				teamColor1 = MobileParty.MainParty.MapEvent.MapEventSettlement.MapFaction.Color;
				teamColor2 = MobileParty.MainParty.MapEvent.MapEventSettlement.MapFaction.Color2;
			}
			else
			{
				teamColor1 = MobileParty.MainParty.MapEvent.GetLeaderParty(side).MapFaction.Color;
				teamColor2 = MobileParty.MainParty.MapEvent.GetLeaderParty(side).MapFaction.Color2;
			}
		}
	}

	private void InitializeTeams(bool isPlayerAttacker = true)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (!Extensions.IsEmpty<Team>((IEnumerable<Team>)((MissionBehavior)this).Mission.Teams))
		{
			throw new MBIllegalValueException("Number of teams is not 0.");
		}
		GetTeamColor((BattleSideEnum)0, isPlayerAttacker, out var teamColor, out var teamColor2);
		GetTeamColor((BattleSideEnum)1, isPlayerAttacker, out var teamColor3, out var teamColor4);
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)0, teamColor, teamColor2, (Banner)null, true, false, true);
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)1, teamColor3, teamColor4, (Banner)null, true, false, true);
		if (isPlayerAttacker)
		{
			((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)1, uint.MaxValue, uint.MaxValue, (Banner)null, true, false, true);
			((MissionBehavior)this).Mission.PlayerTeam = ((MissionBehavior)this).Mission.AttackerTeam;
		}
		else
		{
			((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)0, uint.MaxValue, uint.MaxValue, (Banner)null, true, false, true);
			((MissionBehavior)this).Mission.PlayerTeam = ((MissionBehavior)this).Mission.DefenderTeam;
		}
	}
}
