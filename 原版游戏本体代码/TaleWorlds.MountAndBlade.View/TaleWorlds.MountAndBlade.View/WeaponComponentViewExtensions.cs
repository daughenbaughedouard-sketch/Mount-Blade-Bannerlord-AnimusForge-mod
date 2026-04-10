using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;

namespace TaleWorlds.MountAndBlade.View;

public static class WeaponComponentViewExtensions
{
	public static MetaMesh GetFlyingMeshCopy(this WeaponComponentData weaponComponentData, ItemObject item)
	{
		if (item.WeaponDesign != (WeaponDesign)null)
		{
			if (weaponComponentData.IsRangedWeapon && weaponComponentData.IsConsumable)
			{
				MetaMesh weaponMesh = CraftedDataViewManager.GetCraftedDataView(item.WeaponDesign).WeaponMesh;
				if (!((NativeObject)(object)weaponMesh != (NativeObject)null))
				{
					return null;
				}
				return weaponMesh.CreateCopy();
			}
			return null;
		}
		if (!string.IsNullOrEmpty(item.FlyingMeshName))
		{
			return MetaMesh.GetCopy(item.FlyingMeshName, true, false);
		}
		return null;
	}

	public static MetaMesh GetFlyingMeshIfExists(this WeaponComponentData weaponComponentData, ItemObject item)
	{
		if (item.WeaponDesign != (WeaponDesign)null && weaponComponentData.IsRangedWeapon && weaponComponentData.IsConsumable)
		{
			return CraftedDataViewManager.GetCraftedDataView(item.WeaponDesign).WeaponMesh;
		}
		return null;
	}
}
