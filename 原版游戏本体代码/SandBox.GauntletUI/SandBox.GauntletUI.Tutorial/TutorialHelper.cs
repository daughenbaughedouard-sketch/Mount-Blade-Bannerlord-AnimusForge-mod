using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Helpers;
using SandBox.Missions.MissionLogics.Hideout;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CraftingSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Decisions;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Tutorial;

public static class TutorialHelper
{
	public static bool PlayerIsInAnySettlement
	{
		get
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (currentSettlement != null)
			{
				if (!currentSettlement.IsFortification)
				{
					return currentSettlement.IsVillage;
				}
				return true;
			}
			return false;
		}
	}

	public static bool PlayerIsInAnyVillage
	{
		get
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (currentSettlement == null)
			{
				return false;
			}
			return currentSettlement.IsVillage;
		}
	}

	public static bool IsOrderingAvailable
	{
		get
		{
			Mission current = Mission.Current;
			if (((current != null) ? current.PlayerTeam : null) != null)
			{
				for (int i = 0; i < ((List<Formation>)(object)Mission.Current.PlayerTeam.FormationsIncludingEmpty).Count; i++)
				{
					Formation val = ((List<Formation>)(object)Mission.Current.PlayerTeam.FormationsIncludingEmpty)[i];
					if (val.PlayerOwner == Agent.Main && val.CountOfUnits > 0)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public static bool IsCharacterPopUpWindowOpen => GauntletTutorialSystem.Current.IsCharacterPortraitPopupOpen;

	public static EncyclopediaPages CurrentEncyclopediaPage => GauntletTutorialSystem.Current.CurrentEncyclopediaPageContext;

	public static TutorialContexts CurrentContext => GauntletTutorialSystem.Current.CurrentContext;

	public static bool PlayerIsInNonEnemyTown
	{
		get
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (currentSettlement != null && currentSettlement.IsTown)
			{
				return !FactionManager.IsAtWarAgainstFaction(currentSettlement.MapFaction, MobileParty.MainParty.MapFaction);
			}
			return false;
		}
	}

	public static string ActiveVillageRaidGameMenuID => "raiding_village";

	public static bool IsActiveVillageRaidGameMenuOpen
	{
		get
		{
			Campaign current = Campaign.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				MenuContext currentMenuContext = current.CurrentMenuContext;
				if (currentMenuContext == null)
				{
					obj = null;
				}
				else
				{
					GameMenu gameMenu = currentMenuContext.GameMenu;
					obj = ((gameMenu != null) ? gameMenu.StringId : null);
				}
			}
			return (string?)obj == ActiveVillageRaidGameMenuID;
		}
	}

	public static bool TownMenuIsOpen
	{
		get
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (currentSettlement != null && currentSettlement.IsTown)
			{
				MenuContext currentMenuContext = Campaign.Current.CurrentMenuContext;
				object obj;
				if (currentMenuContext == null)
				{
					obj = null;
				}
				else
				{
					GameMenu gameMenu = currentMenuContext.GameMenu;
					obj = ((gameMenu != null) ? gameMenu.StringId : null);
				}
				return (string?)obj == "town";
			}
			return false;
		}
	}

	public static bool VillageMenuIsOpen
	{
		get
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (currentSettlement == null)
			{
				return false;
			}
			return currentSettlement.IsVillage;
		}
	}

	public static bool BackStreetMenuIsOpen
	{
		get
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (currentSettlement != null && currentSettlement.IsTown && LocationComplex.Current != null)
			{
				Location locationWithId = LocationComplex.Current.GetLocationWithId("tavern");
				return GetMenuLocations.Contains(locationWithId);
			}
			return false;
		}
	}

	public static bool IsPlayerInABattleMission
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			Mission current = Mission.Current;
			if (current != null)
			{
				return (int)current.Mode == 2;
			}
			return false;
		}
	}

	public static bool IsOrderOfBattleOpenAndReady
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			Mission current = Mission.Current;
			if (current != null && (int)current.Mode == 6)
			{
				return !LoadingWindow.IsLoadingWindowActive;
			}
			return false;
		}
	}

	public static bool IsNavalMission
	{
		get
		{
			Mission current = Mission.Current;
			if (current == null)
			{
				return false;
			}
			return current.IsNavalBattle;
		}
	}

	public static bool CanPlayerAssignHimselfToFormation
	{
		get
		{
			if (!IsOrderOfBattleOpenAndReady)
			{
				return false;
			}
			Mission current = Mission.Current;
			if (current == null)
			{
				return false;
			}
			return ((IEnumerable<Formation>)current.PlayerTeam.FormationsIncludingEmpty).Any((Formation x) => x.CountOfUnits > 0 && x.Captain == null);
		}
	}

	public static bool IsPlayerInAFight
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			Mission current = Mission.Current;
			MissionMode? val = ((current != null) ? new MissionMode?(current.Mode) : ((MissionMode?)null));
			if (val.HasValue)
			{
				if (val != (MissionMode?)2 && val != (MissionMode?)3)
				{
					return val == (MissionMode?)7;
				}
				return true;
			}
			return false;
		}
	}

	public static bool IsPlayerEncounterLeader
	{
		get
		{
			Mission current = Mission.Current;
			if (current == null)
			{
				return false;
			}
			Team playerTeam = current.PlayerTeam;
			return ((playerTeam != null) ? new bool?(playerTeam.IsPlayerGeneral) : ((bool?)null)) == true;
		}
	}

	public static bool IsPlayerInAHideoutBattleMission
	{
		get
		{
			Mission current = Mission.Current;
			if (current != null)
			{
				return current.HasMissionBehavior<HideoutMissionController>();
			}
			return false;
		}
	}

	public static IList<Location> GetMenuLocations => Campaign.Current.GameMenuManager.MenuLocations;

	public static bool PlayerIsSafeOnMap => !IsActiveVillageRaidGameMenuOpen;

	public static bool IsCurrentTownHaveDoableCraftingOrder
	{
		get
		{
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			ICraftingCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();
			CraftingOrderSlots val = ((campaignBehavior != null) ? campaignBehavior.CraftingOrders[Settlement.CurrentSettlement?.Town] : null);
			List<CraftingOrder> list = val?.Slots.Where((CraftingOrder x) => x != null).ToList();
			PartyBase mainParty = PartyBase.MainParty;
			MBList<TroopRosterElement> val2 = ((mainParty != null) ? mainParty.MemberRoster.GetTroopRoster() : null);
			if (campaignBehavior == null || val == null || list == null || val2 == null)
			{
				return false;
			}
			for (int num = 0; num < ((List<TroopRosterElement>)(object)val2).Count; num++)
			{
				TroopRosterElement val3 = ((List<TroopRosterElement>)(object)val2)[num];
				if (!((BasicCharacterObject)val3.Character).IsHero)
				{
					continue;
				}
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					if (list[num2].IsOrderAvailableForHero(val3.Character.HeroObject))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public static bool CurrentInventoryScreenIncludesBannerItem
	{
		get
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			InventoryState activeInventoryState = InventoryScreenHelper.GetActiveInventoryState();
			if (activeInventoryState != null)
			{
				InventoryLogic inventoryLogic = activeInventoryState.InventoryLogic;
				IReadOnlyList<ItemRosterElement> readOnlyList = ((inventoryLogic != null) ? inventoryLogic.GetElementsInRoster((InventorySide)0) : null);
				if (readOnlyList != null)
				{
					foreach (ItemRosterElement item in readOnlyList)
					{
						ItemRosterElement current = item;
						EquipmentElement equipmentElement = ((ItemRosterElement)(ref current)).EquipmentElement;
						if (((EquipmentElement)(ref equipmentElement)).Item.IsBannerItem)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	public static bool PlayerHasUnassignedRolesAndMember
	{
		get
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			bool flag = false;
			PartyBase mainParty = PartyBase.MainParty;
			MBList<TroopRosterElement> val = ((mainParty != null) ? mainParty.MemberRoster.GetTroopRoster() : null);
			for (int i = 0; i < ((List<TroopRosterElement>)(object)val).Count; i++)
			{
				TroopRosterElement val2 = ((List<TroopRosterElement>)(object)val)[i];
				if (((BasicCharacterObject)val2.Character).IsHero && !((BasicCharacterObject)val2.Character).IsPlayerCharacter && MobileParty.MainParty.GetHeroPartyRoles(val2.Character.HeroObject).Count == 0)
				{
					flag = true;
					break;
				}
			}
			bool flag2 = MobileParty.MainParty.GetRoleHolder((PartyRole)7) == null || MobileParty.MainParty.GetRoleHolder((PartyRole)8) == null || MobileParty.MainParty.GetRoleHolder((PartyRole)10) == null || MobileParty.MainParty.GetRoleHolder((PartyRole)9) == null;
			return flag && flag2;
		}
	}

	public static bool PlayerCanRecruit
	{
		get
		{
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			if (PlayerIsInAnySettlement && (TownMenuIsOpen || VillageMenuIsOpen) && !Hero.MainHero.IsPrisoner && MobileParty.MainParty.MemberRoster.TotalManCount < PartyBase.MainParty.PartySizeLimit)
			{
				foreach (Hero item in (List<Hero>)(object)Settlement.CurrentSettlement.Notables)
				{
					int num = 0;
					foreach (CharacterObject item2 in HeroHelper.GetVolunteerTroopsOfHeroForRecruitment(item))
					{
						if (item2 != null && HeroHelper.HeroCanRecruitFromHero(item, Hero.MainHero, num))
						{
							ExplainedNumber troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(item2, Hero.MainHero, false);
							int roundedResultNumber = ((ExplainedNumber)(ref troopRecruitmentCost)).RoundedResultNumber;
							return Hero.MainHero.Gold >= 5 * roundedResultNumber;
						}
						num++;
					}
				}
			}
			return false;
		}
	}

	public static bool IsKingdomDecisionPanelActiveAndHasOptions
	{
		get
		{
			if (ScreenManager.TopScreen is GauntletKingdomScreen gauntletKingdomScreen)
			{
				KingdomManagementVM dataSource = gauntletKingdomScreen.DataSource;
				bool? obj;
				if (dataSource == null)
				{
					obj = null;
				}
				else
				{
					KingdomDecisionsVM decision = dataSource.Decision;
					obj = ((decision != null) ? new bool?(decision.IsCurrentDecisionActive) : ((bool?)null));
				}
				if (obj == true)
				{
					return ((Collection<DecisionOptionVM>)(object)gauntletKingdomScreen.DataSource.Decision.CurrentDecision.DecisionOptionsList).Count > 0;
				}
			}
			return false;
		}
	}

	public static Location CurrentMissionLocation
	{
		get
		{
			ICampaignMission current = CampaignMission.Current;
			if (current == null)
			{
				return null;
			}
			return current.Location;
		}
	}

	public static bool BuyingFoodBaseConditions
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			if ((TownMenuIsOpen || VillageMenuIsOpen || (int)CurrentContext == 2) && Settlement.CurrentSettlement != null)
			{
				ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>("grain");
				if (val != null)
				{
					ItemRoster itemRoster = Settlement.CurrentSettlement.ItemRoster;
					int num = itemRoster.FindIndexOfItem(val);
					if (num >= 0)
					{
						int elementUnitCost = itemRoster.GetElementUnitCost(num);
						return Hero.MainHero.Gold >= 5 * elementUnitCost;
					}
				}
			}
			return false;
		}
	}

	public static bool AreTroopUpgradesDisabled
	{
		get
		{
			if (ScreenManager.TopScreen is GauntletPartyScreen gauntletPartyScreen)
			{
				return gauntletPartyScreen.IsTroopUpgradesDisabled;
			}
			return false;
		}
	}

	public static bool PlayerHasAnyUpgradeableTroop
	{
		get
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)MobileParty.MainParty.MemberRoster.GetTroopRoster())
			{
				TroopRosterElement current = item;
				CharacterObject character = current.Character;
				if (((BasicCharacterObject)character).IsHero || ((TroopRosterElement)(ref current)).Number <= 0)
				{
					continue;
				}
				for (int i = 0; i < character.UpgradeTargets.Length; i++)
				{
					if (character.GetUpgradeXpCost(PartyBase.MainParty, i) > ((TroopRosterElement)(ref current)).Xp)
					{
						continue;
					}
					CharacterObject val = character.UpgradeTargets[i];
					if (val.UpgradeRequiresItemFromCategory == null)
					{
						return true;
					}
					foreach (ItemRosterElement item2 in MobileParty.MainParty.ItemRoster)
					{
						ItemRosterElement current2 = item2;
						EquipmentElement equipmentElement = ((ItemRosterElement)(ref current2)).EquipmentElement;
						if (((EquipmentElement)(ref equipmentElement)).Item.ItemCategory == val.UpgradeRequiresItemFromCategory && ((ItemRosterElement)(ref current2)).Amount > 0)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}

	public static bool PlayerIsInAConversation => !Extensions.IsEmpty<CharacterObject>(CharacterObject.ConversationCharacters);

	public static DateTime CurrentTime => DateTime.Now;

	public static int MinimumGoldForCompanion => 999;

	public static float MaximumSpeedForPartyForSpeedTutorial => 4f;

	public static float MaxCohesionForCohesionTutorial => 30f;

	public static bool? IsThereAvailableCompanionInLocation(Location location)
	{
		if (location == null)
		{
			return null;
		}
		return location.GetCharacterList().Any((LocationCharacter x) => ((BasicCharacterObject)x.Character).IsHero && x.Character.HeroObject.IsWanderer && !x.Character.HeroObject.IsPlayerCompanion);
	}
}
