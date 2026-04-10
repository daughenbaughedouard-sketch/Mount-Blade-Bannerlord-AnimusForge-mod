using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics.CosmeticTypes;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;

public class MPArmoryCosmeticClothingItemVM : MPArmoryCosmeticItemBaseVM
{
	public EquipmentElement EquipmentElement { get; }

	public MPArmoryCosmeticsVM.ClothingCategory ClothingCategory { get; }

	public ClothingCosmeticElement ClothingCosmeticElement { get; }

	public MPArmoryCosmeticClothingItemVM(CosmeticElement cosmetic, string cosmeticID)
		: base(cosmetic, cosmeticID, (CosmeticType)0)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected I4, but got Unknown
		ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>(cosmetic.Id);
		EquipmentElement = new EquipmentElement(val, (ItemModifier)null, (ItemObject)null, false);
		base.Icon = new ItemImageIdentifierVM(val, "");
		ClothingCategory = GetCosmeticCategory();
		ClothingCosmeticElement = (ClothingCosmeticElement)(object)((cosmetic is ClothingCosmeticElement) ? cosmetic : null);
		EquipmentElement equipmentElement = EquipmentElement;
		base.ItemType = (int)((EquipmentElement)(ref equipmentElement)).Item.ItemType;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshValues();
		EquipmentElement equipmentElement = EquipmentElement;
		ItemObject item = ((EquipmentElement)(ref equipmentElement)).Item;
		base.Name = ((item != null) ? ((object)item.Name).ToString() : null);
	}

	private MPArmoryCosmeticsVM.ClothingCategory GetCosmeticCategory()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected I4, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		EquipmentElement equipmentElement = EquipmentElement;
		ItemTypeEnum type = ((EquipmentElement)(ref equipmentElement)).Item.Type;
		switch (type - 14)
		{
		default:
			if ((int)type == 24)
			{
				return MPArmoryCosmeticsVM.ClothingCategory.Cape;
			}
			return MPArmoryCosmeticsVM.ClothingCategory.Invalid;
		case 1:
			return MPArmoryCosmeticsVM.ClothingCategory.BodyArmor;
		case 0:
			return MPArmoryCosmeticsVM.ClothingCategory.HeadArmor;
		case 3:
			return MPArmoryCosmeticsVM.ClothingCategory.HandArmor;
		case 2:
			return MPArmoryCosmeticsVM.ClothingCategory.LegArmor;
		}
	}
}
