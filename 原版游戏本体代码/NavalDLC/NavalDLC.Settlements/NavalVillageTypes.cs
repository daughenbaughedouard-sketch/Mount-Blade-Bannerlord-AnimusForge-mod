using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.Settlements;

public class NavalVillageTypes
{
	private static NavalVillageTypes Instance => NavalDLCManager.Instance.NavalVillageTypes;

	public static VillageType WalrusHunter => Instance.VillageTypeWalrusHunter;

	public static VillageType Whaler => Instance.VillageTypeWhaler;

	internal VillageType VillageTypeWalrusHunter { get; private set; }

	internal VillageType VillageTypeWhaler { get; private set; }

	public NavalVillageTypes()
	{
		RegisterAll();
		InitializeAll();
		AddProductions();
	}

	private VillageType Create(string stringId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		return Game.Current.ObjectManager.RegisterPresumedObject<VillageType>(new VillageType(stringId));
	}

	private void RegisterAll()
	{
		VillageTypeWalrusHunter = Create("walrus_hunter");
		VillageTypeWhaler = Create("whaler");
	}

	private ItemObject GetItemObject(string objectId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		return Game.Current.ObjectManager.RegisterPresumedObject<ItemObject>(new ItemObject(objectId));
	}

	private void InitializeAll()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		VillageTypeWalrusHunter.Initialize(new TextObject("{=Eg7KEtGg}Walrus Tusk Hunters", (Dictionary<string, object>)null), "kitchen_horn", "fisherman_ucon", "fisherman_burned", new(ItemObject, float)[1] { (GetItemObject("fish"), 5f) });
		VillageTypeWhaler.Initialize(new TextObject("{=QdCFs5tT}Whalers", (Dictionary<string, object>)null), "bd_barrel_a", "fisherman_ucon", "fisherman_burned", new(ItemObject, float)[1] { (GetItemObject("fish"), 5f) });
	}

	private void AddProductions()
	{
		VillageTypeWalrusHunter.AddProductions((IEnumerable<ValueTuple<ItemObject, float>>)new(ItemObject, float)[1] { (GetItemObject("walrus_tusk"), 1.4f) });
		VillageTypeWhaler.AddProductions((IEnumerable<ValueTuple<ItemObject, float>>)new(ItemObject, float)[1] { (GetItemObject("whale_oil"), 1.8f) });
	}
}
