using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCInventoryCapacityModel : InventoryCapacityModel
{
	private static readonly TextObject _textTroopMounts = new TextObject("{=GIlU4NXm}Troops' Mounts", (Dictionary<string, object>)null);

	private static readonly TextObject _textMountsAndPackAnimals = new TextObject("{=Sb1MKbvP}Mounts and Pack Animals", (Dictionary<string, object>)null);

	private static readonly TextObject _textLiveStocksAnimals = new TextObject("{=KxUgSAKi}Live Stock Animals", (Dictionary<string, object>)null);

	private static readonly TextObject _textItems = new TextObject("{=U7er3V9s}Items", (Dictionary<string, object>)null);

	private static readonly TextObject _textBaseNavalCapacity = new TextObject("{=7Q8ufo5X}Ships", (Dictionary<string, object>)null);

	private const float CustomMountWeight = 50f;

	private const float CustomPackAnimalWeight = 30f;

	private const float CustomLiveStockWeight = 20f;

	public override ExplainedNumber CalculateInventoryCapacity(MobileParty mobileParty, bool isCurrentlyAtSea, bool includeDescriptions = false, int additionalManOnFoot = 0, int additionalSpareMounts = 0, int additionalPackAnimals = 0, bool includeFollowers = false)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<InventoryCapacityModel>)this).BaseModel.CalculateInventoryCapacity(mobileParty, isCurrentlyAtSea, includeDescriptions, additionalManOnFoot, additionalSpareMounts, additionalPackAnimals, includeFollowers);
		if (isCurrentlyAtSea)
		{
			float num = 0f;
			foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
			{
				num += item.InventoryCapacity;
			}
			foreach (MobileParty item2 in (List<MobileParty>)(object)mobileParty.AttachedParties)
			{
				foreach (Ship item3 in (List<Ship>)(object)item2.Ships)
				{
					num += item3.InventoryCapacity;
				}
			}
			((ExplainedNumber)(ref result)).Add(num, _textBaseNavalCapacity, (TextObject)null);
		}
		return result;
	}

	public override int GetItemAverageWeight()
	{
		return ((MBGameModel<InventoryCapacityModel>)this).BaseModel.GetItemAverageWeight();
	}

	public override ExplainedNumber CalculateTotalWeightCarried(MobileParty mobileParty, bool isCurrentlyAtSea, bool includeDescriptions = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<InventoryCapacityModel>)this).BaseModel.CalculateTotalWeightCarried(mobileParty, isCurrentlyAtSea, includeDescriptions);
		if (isCurrentlyAtSea)
		{
			foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)mobileParty.MemberRoster.GetTroopRoster())
			{
				TroopRosterElement current = item;
				float num = 0f;
				if (!((BasicCharacterObject)current.Character).IsHero)
				{
					EquipmentElement horse = ((BasicCharacterObject)current.Character).Equipment.Horse;
					if (!((EquipmentElement)(ref horse)).IsEmpty)
					{
						num += 50f * (float)((TroopRosterElement)(ref current)).Number;
					}
				}
				((ExplainedNumber)(ref result)).Add(num, _textTroopMounts, (TextObject)null);
			}
		}
		return result;
	}

	public override float GetItemEffectiveWeight(EquipmentElement equipmentElement, MobileParty mobileParty, bool isCurrentlyAtSea, out TextObject description)
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		if (isCurrentlyAtSea)
		{
			ItemObject item = ((EquipmentElement)(ref equipmentElement)).Item;
			ExplainedNumber val = default(ExplainedNumber);
			if (item.HasHorseComponent)
			{
				if (item.HorseComponent.IsMount)
				{
					((ExplainedNumber)(ref val))._002Ector(50f, false, (TextObject)null);
					description = _textMountsAndPackAnimals;
					PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.NavalHorde, mobileParty, false, ref val, false);
				}
				else if (item.HorseComponent.IsPackAnimal)
				{
					((ExplainedNumber)(ref val))._002Ector(30f, false, (TextObject)null);
					description = _textMountsAndPackAnimals;
					PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.Optimization, mobileParty, false, ref val, false);
				}
				else if (item.HorseComponent.IsLiveStock)
				{
					((ExplainedNumber)(ref val))._002Ector(20f, false, (TextObject)null);
					description = _textLiveStocksAnimals;
					PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.Optimization, mobileParty, false, ref val, false);
				}
				else
				{
					((ExplainedNumber)(ref val))._002Ector(((EquipmentElement)(ref equipmentElement)).GetEquipmentElementWeight(), false, (TextObject)null);
					description = _textItems;
				}
			}
			else
			{
				((ExplainedNumber)(ref val))._002Ector(((EquipmentElement)(ref equipmentElement)).GetEquipmentElementWeight(), false, (TextObject)null);
				description = _textItems;
			}
			if (item.IsTradeGood)
			{
				PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.GildedPurse, mobileParty, false, ref val, false);
			}
			return ((ExplainedNumber)(ref val)).ResultNumber;
		}
		return ((MBGameModel<InventoryCapacityModel>)this).BaseModel.GetItemEffectiveWeight(equipmentElement, mobileParty, isCurrentlyAtSea, ref description);
	}
}
