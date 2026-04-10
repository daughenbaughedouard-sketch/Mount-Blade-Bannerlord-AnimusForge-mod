using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace SandBox.GameComponents;

public class SandboxBattleInitializationModel : BattleInitializationModel
{
	public override List<FormationClass> GetAllAvailableTroopTypes()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		List<FormationClass> list = new List<FormationClass>();
		MapEventSide mapEventSide = PlayerEncounter.Battle.GetMapEventSide(PlayerEncounter.Battle.PlayerSide);
		bool num = PlayerEncounter.Battle.GetLeaderParty(PlayerEncounter.Battle.PlayerSide) == PartyBase.MainParty;
		bool flag = PartyBase.MainParty.MobileParty.Army != null && PartyBase.MainParty.MobileParty.Army.LeaderParty == PartyBase.MainParty.MobileParty;
		bool flag2 = num && flag;
		for (int i = 0; i < ((List<MapEventParty>)(object)mapEventSide.Parties).Count; i++)
		{
			MapEventParty val = ((List<MapEventParty>)(object)mapEventSide.Parties)[i];
			if (!flag2 && val.Party != PartyBase.MainParty)
			{
				continue;
			}
			for (int j = 0; j < val.Party.MemberRoster.Count; j++)
			{
				CharacterObject characterAtIndex = val.Party.MemberRoster.GetCharacterAtIndex(j);
				TroopRosterElement elementCopyAtIndex = val.Party.MemberRoster.GetElementCopyAtIndex(j);
				if (!((BasicCharacterObject)characterAtIndex).IsHero && ((TroopRosterElement)(ref elementCopyAtIndex)).WoundedNumber < ((TroopRosterElement)(ref elementCopyAtIndex)).Number)
				{
					if (((BasicCharacterObject)characterAtIndex).IsInfantry && !((BasicCharacterObject)characterAtIndex).IsMounted && !list.Contains((FormationClass)0))
					{
						list.Add((FormationClass)0);
					}
					if (((BasicCharacterObject)characterAtIndex).IsRanged && !((BasicCharacterObject)characterAtIndex).IsMounted && !list.Contains((FormationClass)1))
					{
						list.Add((FormationClass)1);
					}
					if (((BasicCharacterObject)characterAtIndex).IsMounted && !((BasicCharacterObject)characterAtIndex).IsRanged && !list.Contains((FormationClass)2))
					{
						list.Add((FormationClass)2);
					}
					if (((BasicCharacterObject)characterAtIndex).IsMounted && ((BasicCharacterObject)characterAtIndex).IsRanged && !list.Contains((FormationClass)3))
					{
						list.Add((FormationClass)3);
					}
					if (list.Count == 4)
					{
						return list;
					}
				}
			}
		}
		return list;
	}

	protected override bool CanPlayerSideDeployWithOrderOfBattleAux()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.Current.IsSallyOutBattle)
		{
			return false;
		}
		MapEvent playerMapEvent = MapEvent.PlayerMapEvent;
		if (MapEvent.PlayerMapEvent == null)
		{
			return false;
		}
		PartyBase leaderParty = playerMapEvent.GetLeaderParty(playerMapEvent.PlayerSide);
		if (leaderParty == PartyBase.MainParty || (leaderParty.IsSettlement && leaderParty.Settlement.OwnerClan.Leader == Hero.MainHero) || playerMapEvent.IsPlayerSergeant())
		{
			return Mission.Current.GetMissionBehavior<IMissionAgentSpawnLogic>().GetNumberOfPlayerControllableTroops() >= 20;
		}
		return false;
	}
}
