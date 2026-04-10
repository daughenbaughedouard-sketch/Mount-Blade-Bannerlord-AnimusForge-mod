using TaleWorlds.Core;

namespace NavalDLC;

public class NavalItemCategories
{
	private ItemCategory _itemCategoryWalrusTusk;

	private ItemCategory _itemCategoryWhaleOil;

	private static NavalItemCategories Instance => NavalDLCManager.Instance.NavalItemCategories;

	public static ItemCategory WalrusTusk => Instance._itemCategoryWalrusTusk;

	public static ItemCategory WhaleOil => Instance._itemCategoryWhaleOil;

	public NavalItemCategories()
	{
		RegisterAll();
		InitializeAll();
	}

	private static ItemCategory Create(string stringId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		return Game.Current.ObjectManager.RegisterPresumedObject<ItemCategory>(new ItemCategory(stringId));
	}

	private void RegisterAll()
	{
		_itemCategoryWalrusTusk = Create("walrus_tusk");
		_itemCategoryWhaleOil = Create("whale_oil");
	}

	private void InitializeAll()
	{
		_itemCategoryWalrusTusk.InitializeObject(true, 10, 38, (Property)3, (ItemCategory)null, 0f, false, true);
		_itemCategoryWhaleOil.InitializeObject(true, 10, 38, (Property)3, (ItemCategory)null, 0f, false, true);
	}
}
