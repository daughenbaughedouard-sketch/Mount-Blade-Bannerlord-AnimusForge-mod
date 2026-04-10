using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Storyline;
using StoryMode;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC;

public static class NavalDLCHelpers
{
	public static ExplainedNumber GetAveragePartySizeLimitFromTemplate(PartyTemplateObject templateObject)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (PartyTemplateStack item in (List<PartyTemplateStack>)(object)templateObject.Stacks)
		{
			num += (item.MaxValue + item.MinValue) / 2;
		}
		return new ExplainedNumber((float)num, false, (TextObject)null);
	}

	public static ExplainedNumber GetMaxPartySizeLimitFromTemplate(PartyTemplateObject templateObject)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (PartyTemplateStack item in (List<PartyTemplateStack>)(object)templateObject.Stacks)
		{
			num += item.MaxValue;
		}
		return new ExplainedNumber((float)num, false, (TextObject)null);
	}

	public static List<Ship> GetSetPieceBattleShips(PartyTemplateObject template, PartyBase party)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		List<Ship> list = ((IEnumerable<Ship>)party.Ships).Where((Ship s) => s.IsUsedByQuest).ToList();
		int num = 0;
		foreach (ShipTemplateStack item in (List<ShipTemplateStack>)(object)template.ShipHulls)
		{
			num += item.MaxValue;
		}
		int num2 = num - list.Count();
		if (num2 > 0)
		{
			foreach (Ship item2 in (from s in (IEnumerable<Ship>)party.Ships
				where !s.IsUsedByQuest
				orderby s.FlagshipScore descending
				select s).ToList())
			{
				if (num2 > 0)
				{
					list.Add(item2);
					num2--;
					continue;
				}
				break;
			}
		}
		return list;
	}

	public static bool IsShipOrdersAvailable()
	{
		if (Mission.Current == null || !Mission.Current.IsNavalBattle)
		{
			return false;
		}
		Team playerTeam = Mission.Current.PlayerTeam;
		if (((playerTeam != null) ? playerTeam.PlayerOrderController : null) == null)
		{
			return false;
		}
		if (Mission.Current.GetMissionBehavior<NavalShipsLogic>() == null)
		{
			return false;
		}
		MBReadOnlyList<Formation> selectedFormations = Mission.Current.PlayerTeam.PlayerOrderController.SelectedFormations;
		if (selectedFormations == null)
		{
			return false;
		}
		for (int i = 0; i < ((List<Formation>)(object)selectedFormations).Count; i++)
		{
			if (IsPlayerCaptainOfFormationShip(((List<Formation>)(object)selectedFormations)[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsPlayerCaptainOfFormationShip(Formation formation)
	{
		return IsAgentCaptainOfFormationShip(Agent.Main, formation);
	}

	public static bool IsAgentCaptainOfFormationShip(Agent agent, Formation formation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Mission current = Mission.Current;
		NavalShipsLogic navalShipsLogic = ((current != null) ? current.GetMissionBehavior<NavalShipsLogic>() : null);
		if (navalShipsLogic == null || !navalShipsLogic.GetShip(formation.Team.TeamSide, formation.FormationIndex, out var ship))
		{
			return false;
		}
		if (agent != null && ship.Captain == agent)
		{
			return true;
		}
		if (agent != null && agent.IsPlayerControlled && ship.Formation.Team.IsPlayerTeam)
		{
			return ship.Formation.Index == 0;
		}
		return false;
	}

	public static void SetCustomSailPatternOfPartyShips(MobileParty party, string sailId)
	{
		foreach (Ship item in (List<Ship>)(object)party.Ships)
		{
			item.CustomSailPatternId = sailId;
		}
		MobileParty.MainParty.SetNavalVisualAsDirty();
	}

	public static void AddUpgradePiecesToPartyShips(MobileParty party, Dictionary<string, string> upgradePiecesBySlot, Figurehead figurehead = null)
	{
		foreach (Ship item in (List<Ship>)(object)party.Ships)
		{
			foreach (KeyValuePair<string, string> item2 in upgradePiecesBySlot)
			{
				if (item.HasSlot(item2.Key))
				{
					item.EquipUpgradePiece(item2.Key, MBObjectManager.Instance.GetObject<ShipUpgradePiece>(item2.Value));
				}
			}
			if (figurehead != null)
			{
				item.ChangeFigurehead(figurehead);
			}
		}
	}

	public static void AddSisterToClan()
	{
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		StoryModeHeroes.LittleSister.Clan = Clan.PlayerClan;
		if (StoryModeHeroes.LittleSister.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
		{
			Town obj = SettlementHelper.FindNearestTownToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement s) => s.OwnerClan.MapFaction == Clan.PlayerClan.MapFaction));
			Settlement val = ((obj != null) ? ((SettlementComponent)obj).Settlement : null);
			if (val == null)
			{
				Town obj2 = SettlementHelper.FindNearestTownToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement s) => !Clan.PlayerClan.MapFaction.IsAtWarWith(s.OwnerClan.MapFaction)));
				val = ((obj2 != null) ? ((SettlementComponent)obj2).Settlement : null);
			}
			if (val == null)
			{
				val = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)((Settlement s) => s.IsTown));
			}
			if (Settlement.CurrentSettlement == val)
			{
				TeleportHeroAction.ApplyImmediateTeleportToSettlement(StoryModeHeroes.LittleSister, val);
			}
			else
			{
				TeleportHeroAction.ApplyDelayedTeleportToSettlement(StoryModeHeroes.LittleSister, val);
			}
			StoryModeHelpers.SetPlayerSiblingsSkillsIfNeeded(StoryModeHeroes.LittleSister);
		}
		else
		{
			StoryModeHeroes.LittleSister.ChangeState((CharacterStates)0);
		}
		StoryModeHeroes.LittleSister.UpdateLastKnownClosestSettlement(NavalStorylineData.HomeSettlement);
		TextObject val2 = new TextObject("{=7XTkTi9B}{PLAYER_LITTLE_SISTER.NAME} is the little sister of {PLAYER.LINK}.", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("PLAYER_LITTLE_SISTER", StoryModeHeroes.LittleSister.CharacterObject, val2, false);
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val2, false);
		StoryModeHeroes.LittleSister.EncyclopediaText = val2;
	}
}
