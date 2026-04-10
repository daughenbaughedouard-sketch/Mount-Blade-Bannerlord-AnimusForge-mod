using System.Collections.Generic;
using NavalDLC.Settlements.Building;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace NavalDLC;

public static class NavalDLCExtensions
{
	public static bool IsFishingParty(this MobileParty party)
	{
		return party.PartyComponent is FishingPartyComponent;
	}

	public static MBReadOnlyList<FishingPartyComponent> FishingParties(this Village village)
	{
		if (!NavalDLCManager.Instance.FishingParties.TryGetValue(village, out var value))
		{
			value = new List<FishingPartyComponent>();
		}
		return new MBReadOnlyList<FishingPartyComponent>((IEnumerable<FishingPartyComponent>)value);
	}

	public static bool IsPirate(this CharacterObject characterObject)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if (characterObject.IsMariner && !((BasicCharacterObject)characterObject).IsHero)
		{
			return (int)characterObject.Occupation == 15;
		}
		return false;
	}

	public static Building GetShipyard(this Town town)
	{
		foreach (Building item in (List<Building>)(object)town.Buildings)
		{
			if (item.BuildingType == NavalBuildingTypes.SettlementShipyard)
			{
				return item;
			}
		}
		return null;
	}

	public static List<ShipUpgradePiece> GetAvailableShipUpgradePieces(this Town town)
	{
		List<ShipUpgradePiece> list = new List<ShipUpgradePiece>();
		MBReadOnlyList<ShipUpgradePiece> objectTypeList = MBObjectManager.Instance.GetObjectTypeList<ShipUpgradePiece>();
		CultureObject culture = town.Culture;
		Building shipyard = town.GetShipyard();
		int num = ((shipyard != null) ? shipyard.CurrentLevel : 0);
		foreach (ShipUpgradePiece item in (List<ShipUpgradePiece>)(object)objectTypeList)
		{
			if (!item.NotMerchandise && item.RequiredPortLevel <= num && ((item.RequiredCulture1 == null && item.RequiredCulture2 == null) || (object)culture == item.RequiredCulture1 || (object)culture == item.RequiredCulture2))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static bool IsNavalStorylineQuestParty(this PartyBase party, out NavalStorylinePartyData partyData)
	{
		partyData = new NavalStorylinePartyData();
		NavalDLCEvents.Instance.IsNavalQuestParty(party, partyData);
		return partyData.IsQuestParty;
	}

	public static bool IsNavalStorylineQuestParty(this PartyBase party)
	{
		NavalStorylinePartyData partyData;
		return party.IsNavalStorylineQuestParty(out partyData);
	}

	public static bool IsNavalStorylineQuestParty(this MobileParty mobileParty, out NavalStorylinePartyData partyData)
	{
		return mobileParty.Party.IsNavalStorylineQuestParty(out partyData);
	}

	public static bool IsNavalStorylineQuestParty(this MobileParty mobileParty)
	{
		NavalStorylinePartyData partyData;
		return mobileParty.IsNavalStorylineQuestParty(out partyData);
	}
}
