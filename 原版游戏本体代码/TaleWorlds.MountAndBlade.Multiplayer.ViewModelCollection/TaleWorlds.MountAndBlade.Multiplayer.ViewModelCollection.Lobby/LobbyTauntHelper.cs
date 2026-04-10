using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics.CosmeticTypes;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

internal static class LobbyTauntHelper
{
	public static Equipment PrepareForTaunt(Equipment originalEquipment, TauntCosmeticElement taunt, bool doNotAddComplimentaryWeapons = false)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Invalid comparison between Unknown and I4
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		TauntUsageSet usageSet = TauntUsageManager.Instance.GetUsageSet(((CosmeticElement)taunt).Id);
		MBReadOnlyList<TauntUsage> val = ((usageSet != null) ? usageSet.GetUsages() : null);
		if (val == null || ((List<TauntUsage>)(object)val).Count == 0)
		{
			return originalEquipment;
		}
		Equipment val2 = new Equipment(originalEquipment);
		EquipmentIndex val3 = default(EquipmentIndex);
		EquipmentIndex val4 = default(EquipmentIndex);
		bool flag = default(bool);
		val2.GetInitialWeaponIndicesToEquip(ref val3, ref val4, ref flag, (InitialWeaponEquipPreference)0);
		object obj;
		EquipmentElement val5;
		if ((int)val3 == -1)
		{
			obj = null;
		}
		else
		{
			val5 = val2[val3];
			ItemObject item = ((EquipmentElement)(ref val5)).Item;
			obj = ((item != null) ? item.PrimaryWeapon : null);
		}
		WeaponComponentData val6 = (WeaponComponentData)obj;
		WeaponComponentData val7 = null;
		if (!flag && (int)val4 != -1)
		{
			val5 = val2[val4];
			ItemObject item2 = ((EquipmentElement)(ref val5)).Item;
			val7 = ((item2 != null) ? item2.PrimaryWeapon : null);
		}
		foreach (TauntUsage item3 in (List<TauntUsage>)(object)val)
		{
			if (item3.IsSuitable(false, true, val6, val7))
			{
				return val2;
			}
		}
		TauntUsage val8 = ((IEnumerable<TauntUsage>)val).FirstOrDefault((Func<TauntUsage, bool>)((TauntUsage u) => !Extensions.HasAnyFlag<TauntUsageFlag>(u.UsageFlag, (TauntUsageFlag)3))) ?? ((List<TauntUsage>)(object)val)[0];
		for (EquipmentIndex val9 = (EquipmentIndex)0; (int)val9 < 5; val9 = (EquipmentIndex)(val9 + 1))
		{
			val5 = (val2[val9] = default(EquipmentElement));
		}
		List<ItemObject> list = ((IEnumerable<ItemObject>)MBObjectManager.Instance.GetObjectTypeList<ItemObject>()).ToList();
		list.Sort((ItemObject first, ItemObject second) => first.Value.CompareTo(second.Value));
		EquipmentIndex eqIndex = (EquipmentIndex)0;
		if (Extensions.HasAnyFlag<TauntUsageFlag>(val8.UsageFlag, (TauntUsageFlag)1))
		{
			ItemObject randomElementWithPredicate = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return CosmeticsManagerHelper.IsWeaponClassBow((WeaponClass)((primaryWeapon != null) ? ((int)primaryWeapon.WeaponClass) : 0));
			});
			if (!val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate, (ItemModifier)null, (ItemObject)null, false)))
			{
				return val2;
			}
			ItemObject randomElementWithPredicate2 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Invalid comparison between Unknown and I4
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return primaryWeapon != null && (int)primaryWeapon.WeaponClass == 12;
			});
			val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate2, (ItemModifier)null, (ItemObject)null, false));
			return val2;
		}
		if (Extensions.HasAnyFlag<TauntUsageFlag>(val8.UsageFlag, (TauntUsageFlag)2))
		{
			ItemObject randomElementWithPredicate3 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return CosmeticsManagerHelper.IsWeaponClassShield((WeaponClass)((primaryWeapon != null) ? ((int)primaryWeapon.WeaponClass) : 0));
			});
			if (!val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate3, (ItemModifier)null, (ItemObject)null, false)))
			{
				return val2;
			}
			if (!Extensions.HasAnyFlag<TauntUsageFlag>(val8.UsageFlag, (TauntUsageFlag)32))
			{
				ItemObject randomElementWithPredicate4 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
				{
					//IL_000d: Unknown result type (might be due to invalid IL or missing references)
					WeaponComponentData primaryWeapon = i.PrimaryWeapon;
					return CosmeticsManagerHelper.IsWeaponClassOneHanded((WeaponClass)((primaryWeapon != null) ? ((int)primaryWeapon.WeaponClass) : 0));
				});
				val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate4, (ItemModifier)null, (ItemObject)null, false));
				return val2;
			}
		}
		if (!Extensions.HasAnyFlag<TauntUsageFlag>(val8.UsageFlag, (TauntUsageFlag)16))
		{
			ItemObject randomElementWithPredicate5 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return CosmeticsManagerHelper.IsWeaponClassTwoHanded((WeaponClass)((primaryWeapon != null) ? ((int)primaryWeapon.WeaponClass) : 0));
			});
			if (!val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate5, (ItemModifier)null, (ItemObject)null, false)))
			{
				return val2;
			}
			if (val8.IsSuitable(false, true, randomElementWithPredicate5.PrimaryWeapon, (WeaponComponentData)null))
			{
				return val2;
			}
		}
		if (!Extensions.HasAnyFlag<TauntUsageFlag>(val8.UsageFlag, (TauntUsageFlag)32))
		{
			ItemObject randomElementWithPredicate6 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return CosmeticsManagerHelper.IsWeaponClassOneHanded((WeaponClass)((primaryWeapon != null) ? ((int)primaryWeapon.WeaponClass) : 0));
			});
			if (!val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate6, (ItemModifier)null, (ItemObject)null, false)))
			{
				return val2;
			}
			if (val8.IsSuitable(false, true, randomElementWithPredicate6.PrimaryWeapon, (WeaponComponentData)null))
			{
				return val2;
			}
		}
		if (!Extensions.HasAnyFlag<TauntUsageFlag>(val8.UsageFlag, (TauntUsageFlag)64))
		{
			ItemObject randomElementWithPredicate7 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return CosmeticsManagerHelper.IsWeaponClassShield((WeaponClass)((primaryWeapon != null) ? ((int)primaryWeapon.WeaponClass) : 0));
			});
			if (!val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate7, (ItemModifier)null, (ItemObject)null, false)))
			{
				return val2;
			}
		}
		if (!Extensions.HasAnyFlag<TauntUsageFlag>(val8.UsageFlag, (TauntUsageFlag)128))
		{
			ItemObject randomElementWithPredicate8 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return CosmeticsManagerHelper.IsWeaponClassBow((WeaponClass)((primaryWeapon != null) ? ((int)primaryWeapon.WeaponClass) : 0));
			});
			if (!val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate8, (ItemModifier)null, (ItemObject)null, false)))
			{
				return val2;
			}
			ItemObject randomElementWithPredicate9 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Invalid comparison between Unknown and I4
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return primaryWeapon != null && (int)primaryWeapon.WeaponClass == 12;
			});
			val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate9, (ItemModifier)null, (ItemObject)null, false));
			return val2;
		}
		if (!Extensions.HasAnyFlag<TauntUsageFlag>(val8.UsageFlag, (TauntUsageFlag)256))
		{
			ItemObject randomElementWithPredicate10 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return CosmeticsManagerHelper.IsWeaponClassCrossbow((WeaponClass)((primaryWeapon != null) ? ((int)primaryWeapon.WeaponClass) : 0));
			});
			if (!val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate10, (ItemModifier)null, (ItemObject)null, false)))
			{
				return val2;
			}
			ItemObject randomElementWithPredicate11 = Extensions.GetRandomElementWithPredicate<ItemObject>((IReadOnlyList<ItemObject>)list, (Func<ItemObject, bool>)delegate(ItemObject i)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Invalid comparison between Unknown and I4
				WeaponComponentData primaryWeapon = i.PrimaryWeapon;
				return primaryWeapon != null && (int)primaryWeapon.WeaponClass == 13;
			});
			val2.TryAddElement(ref eqIndex, new EquipmentElement(randomElementWithPredicate11, (ItemModifier)null, (ItemObject)null, false));
			return val2;
		}
		return val2;
	}

	private static Tuple<EquipmentIndex, EquipmentElement, WeaponComponentData> GetWeaponInfoOfType(this Equipment equipment, WeaponClass type)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			EquipmentElement val2 = equipment[val];
			ItemObject item = ((EquipmentElement)(ref val2)).Item;
			WeaponComponentData val3 = ((item == null) ? null : ((IEnumerable<WeaponComponentData>)item.Weapons)?.FirstOrDefault((Func<WeaponComponentData, bool>)((WeaponComponentData w) => w.WeaponClass == type)));
			if (val3 != null)
			{
				return new Tuple<EquipmentIndex, EquipmentElement, WeaponComponentData>(val, equipment[val], val3);
			}
		}
		return null;
	}

	private static Tuple<EquipmentIndex, EquipmentElement, WeaponComponentData> GetWeaponInfoOfPredicate(this Equipment equipment, Predicate<WeaponComponentData> predicate)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (predicate == null)
		{
			return null;
		}
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			EquipmentElement val2 = equipment[val];
			ItemObject item = ((EquipmentElement)(ref val2)).Item;
			WeaponComponentData val3 = ((item == null) ? null : ((IEnumerable<WeaponComponentData>)item.Weapons)?.FirstOrDefault((Func<WeaponComponentData, bool>)((WeaponComponentData w) => predicate(w))));
			if (val3 != null)
			{
				return new Tuple<EquipmentIndex, EquipmentElement, WeaponComponentData>(val, equipment[val], val3);
			}
		}
		return null;
	}

	private static Tuple<EquipmentIndex, EquipmentElement, WeaponComponentData> GetTwoHandedWeaponInfo(this Equipment equipment)
	{
		return equipment.GetWeaponInfoOfType((WeaponClass)5) ?? equipment.GetWeaponInfoOfType((WeaponClass)3) ?? equipment.GetWeaponInfoOfType((WeaponClass)8) ?? equipment.GetWeaponInfoOfType((WeaponClass)10);
	}

	private static bool TryAddElement(this Equipment equipment, ref EquipmentIndex eqIndex, EquipmentElement element)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eqIndex < 0 || (int)eqIndex > 1)
		{
			return false;
		}
		if (Equipment.IsItemFitsToSlot(eqIndex, ((EquipmentElement)(ref element)).Item))
		{
			equipment[eqIndex] = element;
			eqIndex = (EquipmentIndex)((int)eqIndex + 1);
		}
		return true;
	}

	private static void SwapItems(this Equipment equipment, EquipmentIndex first, EquipmentIndex second)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		EquipmentElement val = equipment[first];
		equipment[first] = equipment[second];
		equipment[second] = val;
	}
}
