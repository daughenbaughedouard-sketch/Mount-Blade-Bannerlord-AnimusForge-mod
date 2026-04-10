using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory;

public static class LobbyCosmeticExtensions
{
	public static ItemTypeEnum ToItemTypeEnum(this MPArmoryCosmeticsVM.ClothingCategory cosmeticCategory)
	{
		return (ItemTypeEnum)(cosmeticCategory switch
		{
			MPArmoryCosmeticsVM.ClothingCategory.ClothingCategoriesBegin => 0, 
			MPArmoryCosmeticsVM.ClothingCategory.BodyArmor => 15, 
			MPArmoryCosmeticsVM.ClothingCategory.HeadArmor => 14, 
			MPArmoryCosmeticsVM.ClothingCategory.Cape => 24, 
			MPArmoryCosmeticsVM.ClothingCategory.HandArmor => 17, 
			MPArmoryCosmeticsVM.ClothingCategory.LegArmor => 16, 
			_ => 0, 
		});
	}

	public static EquipmentIndex GetCosmeticEquipmentIndex(this ItemObject itemObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected I4, but got Unknown
		if (itemObject != null)
		{
			ItemTypeEnum type = itemObject.Type;
			if ((int)type <= 17)
			{
				if ((int)type == 1)
				{
					return (EquipmentIndex)10;
				}
				switch (type - 14)
				{
				case 1:
					return (EquipmentIndex)6;
				case 2:
					return (EquipmentIndex)7;
				case 3:
					return (EquipmentIndex)8;
				case 0:
					return (EquipmentIndex)5;
				}
			}
			else
			{
				if ((int)type == 24)
				{
					return (EquipmentIndex)9;
				}
				if ((int)type == 25)
				{
					return (EquipmentIndex)11;
				}
			}
			return (EquipmentIndex)(-1);
		}
		return (EquipmentIndex)(-1);
	}
}
