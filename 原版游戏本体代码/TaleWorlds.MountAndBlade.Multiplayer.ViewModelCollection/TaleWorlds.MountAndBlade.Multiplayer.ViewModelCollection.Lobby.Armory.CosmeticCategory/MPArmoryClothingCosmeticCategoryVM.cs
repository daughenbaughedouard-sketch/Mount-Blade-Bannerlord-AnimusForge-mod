using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics.CosmeticTypes;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticCategory;

public class MPArmoryClothingCosmeticCategoryVM : MPArmoryCosmeticCategoryBaseVM
{
	public readonly MPArmoryCosmeticsVM.ClothingCategory ClothingCategory;

	private List<string> _defaultCosmeticIDs;

	public static event Action<MPArmoryClothingCosmeticCategoryVM> OnSelected;

	public MPArmoryClothingCosmeticCategoryVM(MPArmoryCosmeticsVM.ClothingCategory clothingCategory)
		: base((CosmeticType)0)
	{
		_defaultCosmeticIDs = new List<string>();
		ClothingCategory = clothingCategory;
		base.CosmeticCategoryName = clothingCategory.ToString();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		base.AvailableCosmetics.ApplyActionOnAllItems((Action<MPArmoryCosmeticItemBaseVM>)delegate(MPArmoryCosmeticItemBaseVM c)
		{
			((ViewModel)c).RefreshValues();
		});
	}

	protected override void ExecuteSelectCategory()
	{
		MPArmoryClothingCosmeticCategoryVM.OnSelected?.Invoke(this);
	}

	private void AddDefaultItem(ItemObject item)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		MPArmoryCosmeticClothingItemVM item2 = new MPArmoryCosmeticClothingItemVM((CosmeticElement)new ClothingCosmeticElement(((MBObjectBase)item).StringId, (CosmeticRarity)0, 0, new List<string>(), new List<Tuple<string, string>>()), string.Empty)
		{
			IsUnlocked = true,
			IsUnequippable = false
		};
		ItemTypeEnum val = ClothingCategory.ToItemTypeEnum();
		if ((int)val == 0 || val == item.ItemType)
		{
			((Collection<MPArmoryCosmeticItemBaseVM>)(object)base.AvailableCosmetics).Add((MPArmoryCosmeticItemBaseVM)item2);
		}
	}

	public void SetDefaultEquipments(Equipment equipment)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		((Collection<MPArmoryCosmeticItemBaseVM>)(object)base.AvailableCosmetics).Clear();
		_defaultCosmeticIDs.Clear();
		if (equipment == null)
		{
			return;
		}
		for (EquipmentIndex val = (EquipmentIndex)5; (int)val < 10; val = (EquipmentIndex)(val + 1))
		{
			EquipmentElement val2 = equipment[val];
			ItemObject item = ((EquipmentElement)(ref val2)).Item;
			if (item != null)
			{
				List<string> defaultCosmeticIDs = _defaultCosmeticIDs;
				val2 = equipment[val];
				defaultCosmeticIDs.Add(((MBObjectBase)((EquipmentElement)(ref val2)).Item).StringId);
				AddDefaultItem(item);
			}
		}
	}

	public void ReplaceCosmeticWithDefaultItem(MPArmoryCosmeticClothingItemVM cosmetic, MPArmoryCosmeticsVM.ClothingCategory clothingCategory, MPHeroClass selectedClass, List<string> ownedCosmetics)
	{
		bool num = cosmetic.ClothingCategory == clothingCategory || clothingCategory == MPArmoryCosmeticsVM.ClothingCategory.ClothingCategoriesBegin;
		CosmeticElement cosmetic2 = cosmetic.Cosmetic;
		ClothingCosmeticElement val;
		bool flag = (val = (ClothingCosmeticElement)(object)((cosmetic2 is ClothingCosmeticElement) ? cosmetic2 : null)) != null && (val.ReplaceItemsId.Any((string c) => _defaultCosmeticIDs.Contains(c)) || val.ReplaceItemless.Any((Tuple<string, string> r) => r.Item1 == ((MBObjectBase)selectedClass).StringId)) && !((Collection<MPArmoryCosmeticItemBaseVM>)(object)base.AvailableCosmetics).Contains((MPArmoryCosmeticItemBaseVM)cosmetic);
		if (num && flag)
		{
			((Collection<MPArmoryCosmeticItemBaseVM>)(object)base.AvailableCosmetics).Add((MPArmoryCosmeticItemBaseVM)cosmetic);
			cosmetic.IsUnlocked = (ownedCosmetics != null && ownedCosmetics.Contains(cosmetic.CosmeticID)) || cosmetic.Cosmetic.IsFree;
		}
	}

	public void OnEquipmentRefreshed(EquipmentIndex equipmentIndex)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		foreach (MPArmoryCosmeticItemBaseVM item in (Collection<MPArmoryCosmeticItemBaseVM>)(object)base.AvailableCosmetics)
		{
			if (item is MPArmoryCosmeticClothingItemVM { EquipmentElement: var equipmentElement } && ((EquipmentElement)(ref equipmentElement)).Item.GetCosmeticEquipmentIndex() == equipmentIndex)
			{
				item.IsUsed = false;
			}
		}
	}
}
