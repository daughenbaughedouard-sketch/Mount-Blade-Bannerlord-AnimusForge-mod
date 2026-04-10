using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.View;

public static class ItemCollectionElementViewExtensions
{
	public static string GetMaterialCacheID(object o)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		ItemRosterElement val = (ItemRosterElement)o;
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref val)).EquipmentElement;
		if (!((EquipmentElement)(ref equipmentElement)).Item.IsCraftedWeapon)
		{
			equipmentElement = ((ItemRosterElement)(ref val)).EquipmentElement;
			return "InventorySlot_" + ((EquipmentElement)(ref equipmentElement)).Item.MultiMeshName;
		}
		return "";
	}

	public static MetaMesh GetMultiMesh(this ItemObject item, bool isFemale, bool useSlimVersion, bool needBatchedVersion)
	{
		MetaMesh val = null;
		if (item != null)
		{
			bool flag = false;
			if (item.HasArmorComponent)
			{
				flag = item.ArmorComponent.MultiMeshHasGenderVariations;
			}
			val = item.GetMultiMeshCopyWithGenderData(flag && isFemale, useSlimVersion, needBatchedVersion);
			if ((NativeObject)(object)val == (NativeObject)null || val.MeshCount == 0)
			{
				val = item.GetMultiMeshCopy();
			}
		}
		return val;
	}

	public static MetaMesh GetMultiMesh(this EquipmentElement equipmentElement, bool isFemale, bool useSlimVersion, bool needBatchedVersion)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (equipmentElement.CosmeticItem == null)
		{
			return ((EquipmentElement)(ref equipmentElement)).Item.GetMultiMesh(isFemale, useSlimVersion, needBatchedVersion);
		}
		return equipmentElement.CosmeticItem.GetMultiMesh(isFemale, useSlimVersion, needBatchedVersion);
	}

	public static MetaMesh GetMultiMesh(this MissionWeapon weapon, bool isFemale, bool useSlimVersion, bool needBatchedVersion)
	{
		return ((MissionWeapon)(ref weapon)).Item.GetMultiMesh(isFemale, useSlimVersion, needBatchedVersion);
	}

	public static MetaMesh GetItemMeshForInventory(this ItemRosterElement rosterElement, bool isFemale = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref rosterElement)).EquipmentElement;
		if ((int)((EquipmentElement)(ref equipmentElement)).Item.ItemType != 5)
		{
			equipmentElement = ((ItemRosterElement)(ref rosterElement)).EquipmentElement;
			if ((int)((EquipmentElement)(ref equipmentElement)).Item.ItemType != 6)
			{
				equipmentElement = ((ItemRosterElement)(ref rosterElement)).EquipmentElement;
				if ((int)((EquipmentElement)(ref equipmentElement)).Item.ItemType != 7)
				{
					return ((ItemRosterElement)(ref rosterElement)).EquipmentElement.GetMultiMesh(isFemale, useSlimVersion: false, needBatchedVersion: false);
				}
			}
		}
		equipmentElement = ((ItemRosterElement)(ref rosterElement)).EquipmentElement;
		return ((EquipmentElement)(ref equipmentElement)).Item.GetHolsterMeshCopy();
	}

	public static MetaMesh GetHolsterMeshCopy(this ItemObject item)
	{
		if (item.WeaponDesign != (WeaponDesign)null)
		{
			MetaMesh holsterMesh = CraftedDataViewManager.GetCraftedDataView(item.WeaponDesign).HolsterMesh;
			if (!((NativeObject)(object)holsterMesh != (NativeObject)null))
			{
				return null;
			}
			return holsterMesh.CreateCopy();
		}
		if (!string.IsNullOrEmpty(item.HolsterMeshName))
		{
			return MetaMesh.GetCopy(item.HolsterMeshName, true, false);
		}
		return null;
	}

	public static MetaMesh GetHolsterMeshIfExists(this ItemObject item)
	{
		if (!(item.WeaponDesign != (WeaponDesign)null))
		{
			return null;
		}
		return CraftedDataViewManager.GetCraftedDataView(item.WeaponDesign).HolsterMesh;
	}

	public static MetaMesh GetHolsterWithWeaponMeshCopy(this ItemObject item, bool needBatchedVersion)
	{
		if (item.WeaponDesign != (WeaponDesign)null)
		{
			CraftedDataView craftedDataView = CraftedDataViewManager.GetCraftedDataView(item.WeaponDesign);
			MetaMesh val = (needBatchedVersion ? craftedDataView.HolsterMeshWithWeapon : craftedDataView.NonBatchedHolsterMeshWithWeapon);
			if (!((NativeObject)(object)val != (NativeObject)null))
			{
				return null;
			}
			return val.CreateCopy();
		}
		if (!string.IsNullOrEmpty(item.HolsterWithWeaponMeshName))
		{
			return MetaMesh.GetCopy(item.HolsterWithWeaponMeshName, true, false);
		}
		return null;
	}

	public static MetaMesh GetHolsterWithWeaponMeshIfExists(this ItemObject item)
	{
		if (!(item.WeaponDesign != (WeaponDesign)null))
		{
			return null;
		}
		return CraftedDataViewManager.GetCraftedDataView(item.WeaponDesign).HolsterMeshWithWeapon;
	}

	public static MetaMesh GetFlyingMeshCopy(this ItemObject item, bool needBatchedVersion)
	{
		WeaponComponent weaponComponent = item.WeaponComponent;
		WeaponComponentData val = ((weaponComponent != null) ? weaponComponent.PrimaryWeapon : null);
		if (item.WeaponDesign != (WeaponDesign)null && val != null)
		{
			if (val.IsRangedWeapon && val.IsConsumable)
			{
				CraftedDataView craftedDataView = CraftedDataViewManager.GetCraftedDataView(item.WeaponDesign);
				MetaMesh val2 = (needBatchedVersion ? craftedDataView.WeaponMesh : craftedDataView.NonBatchedWeaponMesh);
				if (!((NativeObject)(object)val2 != (NativeObject)null))
				{
					return null;
				}
				return val2.CreateCopy();
			}
			return null;
		}
		if (!string.IsNullOrEmpty(item.FlyingMeshName))
		{
			return MetaMesh.GetCopy(item.FlyingMeshName, true, false);
		}
		return null;
	}

	public static MetaMesh GetFlyingMeshIfExists(this ItemObject item)
	{
		if (item == null)
		{
			return null;
		}
		WeaponComponent weaponComponent = item.WeaponComponent;
		if (weaponComponent == null)
		{
			return null;
		}
		return weaponComponent.PrimaryWeapon.GetFlyingMeshIfExists(item);
	}

	internal static Material GetTableauMaterial(this ItemObject item, Banner banner)
	{
		Material tableauMaterial = null;
		if (item != null && item.IsUsingTableau)
		{
			Material val = null;
			MetaMesh multiMeshCopy = item.GetMultiMeshCopy();
			int meshCount = multiMeshCopy.MeshCount;
			for (int i = 0; i < meshCount; i++)
			{
				Mesh meshAtIndex = multiMeshCopy.GetMeshAtIndex(i);
				if (!meshAtIndex.HasTag("dont_use_tableau"))
				{
					val = meshAtIndex.GetMaterial();
					((NativeObject)meshAtIndex).ManualInvalidate();
					break;
				}
				((NativeObject)meshAtIndex).ManualInvalidate();
			}
			((NativeObject)multiMeshCopy).ManualInvalidate();
			if (meshCount == 0 || (NativeObject)(object)val == (NativeObject)null)
			{
				multiMeshCopy = item.GetMultiMeshCopy();
				Mesh meshAtIndex2 = multiMeshCopy.GetMeshAtIndex(0);
				val = meshAtIndex2.GetMaterial();
				((NativeObject)meshAtIndex2).ManualInvalidate();
				((NativeObject)multiMeshCopy).ManualInvalidate();
			}
			if (banner != null)
			{
				BannerDebugInfo debugInfo = BannerDebugInfo.CreateManual("ItemCollectionElementViewExtensions");
				if ((NativeObject)(object)val == (NativeObject)null)
				{
					val = Material.GetDefaultTableauSampleMaterial(true);
				}
				uint flagMask = (uint)val.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
				Dictionary<Tuple<Material, Banner>, Material> dictionary = null;
				if (ViewSubModule.BannerTexturedMaterialCache != null)
				{
					dictionary = ViewSubModule.BannerTexturedMaterialCache;
				}
				if (dictionary != null)
				{
					if (dictionary.ContainsKey(new Tuple<Material, Banner>(val, banner)))
					{
						tableauMaterial = dictionary[new Tuple<Material, Banner>(val, banner)];
					}
					else
					{
						tableauMaterial = val.CreateCopy();
						Action<Texture> setAction = delegate(Texture tex)
						{
							ulong shaderFlags = tableauMaterial.GetShaderFlags();
							tableauMaterial.SetShaderFlags(shaderFlags | flagMask);
							tableauMaterial.SetTexture((MBTextureType)1, tex);
						};
						banner.GetTableauTextureSmall(in debugInfo, setAction);
						dictionary.Add(new Tuple<Material, Banner>(val, banner), tableauMaterial);
					}
				}
				else
				{
					tableauMaterial = val.CreateCopy();
					Action<Texture> setAction2 = delegate(Texture tex)
					{
						ulong shaderFlags = tableauMaterial.GetShaderFlags();
						tableauMaterial.SetShaderFlags(shaderFlags | flagMask);
						tableauMaterial.SetTexture((MBTextureType)1, tex);
					};
					banner.GetTableauTextureSmall(in debugInfo, setAction2);
				}
			}
		}
		return tableauMaterial;
	}

	public static MatrixFrame GetCameraFrameForInventory(this ItemRosterElement itemRosterElement)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return MatrixFrame.Identity;
	}

	public static MatrixFrame GetItemFrameForInventory(this ItemRosterElement itemRosterElement)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected I4, but got Unknown
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame result = MatrixFrame.Identity;
		Mat3 identity = Mat3.Identity;
		float num = 0.95f;
		Vec3 positionShift = default(Vec3);
		((Vec3)(ref positionShift))._002Ector(0f, 0f, 0f, -1f);
		MetaMesh itemMeshForInventory = itemRosterElement.GetItemMeshForInventory();
		if ((NativeObject)(object)itemMeshForInventory != (NativeObject)null)
		{
			EquipmentElement equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
			ItemTypeEnum itemType = ((EquipmentElement)(ref equipmentElement)).Item.ItemType;
			switch (itemType - 1)
			{
			case 13:
			case 14:
			case 23:
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutUp(MathF.PI);
				((Mat3)(ref identity)).RotateAboutSide(MathF.PI * -3f / 50f);
				break;
			case 0:
			case 20:
			case 24:
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutUp(MathF.PI / 2f);
				break;
			case 1:
			case 2:
			case 3:
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutForward(-MathF.PI / 4f);
				break;
			case 16:
				((Mat3)(ref identity)).RotateAboutSide(MathF.PI * 11f / 20f);
				num = 2.1f;
				((Vec3)(ref positionShift))._002Ector(0f, -0.4f, 0f, -1f);
				break;
			case 15:
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutUp(MathF.PI);
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 10f);
				((Mat3)(ref identity)).RotateAboutUp(0.47123894f);
				break;
			case 7:
				((Mat3)(ref identity)).RotateAboutUp(MathF.PI);
				break;
			case 9:
				((Mat3)(ref identity)).RotateAboutForward(MathF.PI * 3f / 4f);
				((Mat3)(ref identity)).RotateAboutSide(MathF.PI * -3f / 4f);
				((Mat3)(ref identity)).RotateAboutUp(-MathF.PI / 2f);
				break;
			case 8:
			case 10:
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutForward(-MathF.PI / 4f);
				break;
			case 4:
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutForward(-MathF.PI / 4f);
				break;
			case 5:
			case 6:
			case 11:
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutForward(-MathF.PI / 4f);
				break;
			case 12:
				((Mat3)(ref identity)).RotateAboutSide(-1.0995574f);
				((Mat3)(ref identity)).RotateAboutUp(MathF.PI / 4f);
				break;
			case 21:
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 5f);
				((Mat3)(ref identity)).RotateAboutUp(-0.47123894f);
				break;
			}
			equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
			if (((EquipmentElement)(ref equipmentElement)).Item.IsCraftedWeapon)
			{
				num *= 0.55f;
			}
			equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
			result = ((EquipmentElement)(ref equipmentElement)).Item.GetScaledFrame(identity, itemMeshForInventory, num, positionShift);
			equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
			if (((EquipmentElement)(ref equipmentElement)).Item.IsCraftedWeapon)
			{
				equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
				((MatrixFrame)(ref result)).Elevate(-0.01f * ((float)((EquipmentElement)(ref equipmentElement)).Item.WeaponComponent.PrimaryWeapon.WeaponLength / 2f));
			}
		}
		return result;
	}

	public static MatrixFrame GetItemFrameForItemTooltip(this ItemRosterElement itemRosterElement)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Invalid comparison between Unknown and I4
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Invalid comparison between Unknown and I4
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Invalid comparison between Unknown and I4
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Invalid comparison between Unknown and I4
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Invalid comparison between Unknown and I4
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Invalid comparison between Unknown and I4
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Invalid comparison between Unknown and I4
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Invalid comparison between Unknown and I4
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Invalid comparison between Unknown and I4
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Invalid comparison between Unknown and I4
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Invalid comparison between Unknown and I4
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Invalid comparison between Unknown and I4
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Invalid comparison between Unknown and I4
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Invalid comparison between Unknown and I4
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Invalid comparison between Unknown and I4
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Invalid comparison between Unknown and I4
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Invalid comparison between Unknown and I4
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Invalid comparison between Unknown and I4
		MatrixFrame result = MatrixFrame.Identity;
		Mat3 identity = Mat3.Identity;
		float num = 0.85f;
		Vec3 positionShift = default(Vec3);
		((Vec3)(ref positionShift))._002Ector(0f, 0f, 0f, -1f);
		MetaMesh itemMeshForInventory = itemRosterElement.GetItemMeshForInventory();
		EquipmentElement equipmentElement;
		if ((NativeObject)(object)itemMeshForInventory != (NativeObject)null)
		{
			equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
			ItemTypeEnum itemType = ((EquipmentElement)(ref equipmentElement)).Item.ItemType;
			if ((int)itemType == 15 || (int)itemType == 24 || (int)itemType == 14)
			{
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutUp(MathF.PI);
			}
			else if ((int)itemType == 5)
			{
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
			}
			else if ((int)itemType == 1 || (int)itemType == 25 || (int)itemType == 21)
			{
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutUp(MathF.PI / 2f);
				num = 0.65f;
			}
			else if ((int)itemType == 2 || (int)itemType == 3 || (int)itemType == 9 || (int)itemType == 12 || (int)itemType == 7 || (int)itemType == 6 || (int)itemType == 4 || (int)itemType == 10 || (int)itemType == 11)
			{
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutForward(-MathF.PI / 4f);
			}
			else if ((int)itemType == 16)
			{
				((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutUp(MathF.PI);
			}
			else if ((int)itemType == 17)
			{
				((Mat3)(ref identity)).RotateAboutSide(MathF.PI / 2f);
				((Mat3)(ref identity)).RotateAboutSide(MathF.PI * -2f / 25f);
				num = 2.1f;
				((Vec3)(ref positionShift))._002Ector(0f, -0.4f, 0f, -1f);
			}
			else if ((int)itemType == 8)
			{
				((Mat3)(ref identity)).RotateAboutUp(2.261947f);
			}
			else if ((int)itemType != 19)
			{
				if ((int)itemType == 13)
				{
					((Mat3)(ref identity)).RotateAboutSide(-1.0995574f);
					((Mat3)(ref identity)).RotateAboutUp(MathF.PI / 4f);
				}
				else if ((int)itemType == 22)
				{
					((Mat3)(ref identity)).RotateAboutSide(-MathF.PI / 5f);
					((Mat3)(ref identity)).RotateAboutUp(-0.47123894f);
				}
			}
			equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
			if (((EquipmentElement)(ref equipmentElement)).Item.IsCraftedWeapon)
			{
				num *= 0.55f;
			}
			equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
			result = ((EquipmentElement)(ref equipmentElement)).Item.GetScaledFrame(identity, itemMeshForInventory, num, positionShift);
			result.origin.z -= 5f;
		}
		equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
		if (((EquipmentElement)(ref equipmentElement)).Item.IsCraftedWeapon)
		{
			equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
			((MatrixFrame)(ref result)).Elevate(-0.01f * ((float)((EquipmentElement)(ref equipmentElement)).Item.WeaponComponent.PrimaryWeapon.WeaponLength / 2f));
		}
		return result;
	}

	public static void OnGetWeaponData(ref WeaponData weaponData, MissionWeapon weapon, bool isFemale, Banner banner, bool needBatchedVersion)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		MetaMesh multiMesh = weapon.GetMultiMesh(isFemale, useSlimVersion: false, needBatchedVersion);
		weaponData.WeaponMesh = multiMesh;
		MetaMesh holsterMeshCopy = ((MissionWeapon)(ref weapon)).Item.GetHolsterMeshCopy();
		weaponData.HolsterMesh = holsterMeshCopy;
		MetaMesh holsterWithWeaponMeshCopy = ((MissionWeapon)(ref weapon)).Item.GetHolsterWithWeaponMeshCopy(needBatchedVersion);
		weaponData.Prefab = ((MissionWeapon)(ref weapon)).Item.PrefabName;
		weaponData.HolsterMeshWithWeapon = holsterWithWeaponMeshCopy;
		MetaMesh flyingMeshCopy = ((MissionWeapon)(ref weapon)).Item.GetFlyingMeshCopy(needBatchedVersion);
		weaponData.FlyingMesh = flyingMeshCopy;
		Material tableauMaterial = ((MissionWeapon)(ref weapon)).Item.GetTableauMaterial(banner);
		weaponData.TableauMaterial = tableauMaterial;
	}
}
