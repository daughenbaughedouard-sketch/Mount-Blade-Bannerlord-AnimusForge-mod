using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace AnimusForge;

internal static class MissionEquipmentCompat
{
	internal static readonly EquipmentIndex[] WeaponSlots = new EquipmentIndex[5]
	{
		EquipmentIndex.WeaponItemBeginSlot,
		EquipmentIndex.Weapon1,
		EquipmentIndex.Weapon2,
		EquipmentIndex.Weapon3,
		EquipmentIndex.ExtraWeaponSlot
	};

	internal static bool IsValidEquipmentSlot(EquipmentIndex equipmentIndex)
	{
		return equipmentIndex >= EquipmentIndex.WeaponItemBeginSlot && equipmentIndex < EquipmentIndex.NumEquipmentSetSlots;
	}

	internal static bool IsValidWeaponSlot(EquipmentIndex equipmentIndex)
	{
		for (int i = 0; i < WeaponSlots.Length; i++)
		{
			if (WeaponSlots[i] == equipmentIndex)
			{
				return true;
			}
		}
		return false;
	}

	internal static IEnumerable<EquipmentIndex> EnumerateWeaponSlots()
	{
		return WeaponSlots;
	}

	internal static bool TryGetMissionWeapon(Agent agent, EquipmentIndex equipmentIndex, out MissionWeapon missionWeapon)
	{
		missionWeapon = default(MissionWeapon);
		if (agent == null)
		{
			return false;
		}
		try
		{
			return TryGetMissionWeapon(agent.Equipment, equipmentIndex, out missionWeapon);
		}
		catch
		{
			missionWeapon = default(MissionWeapon);
			return false;
		}
	}

	internal static bool TryGetMissionWeapon(MissionEquipment equipment, EquipmentIndex equipmentIndex, out MissionWeapon missionWeapon)
	{
		missionWeapon = default(MissionWeapon);
		if (equipment == null || !IsValidWeaponSlot(equipmentIndex))
		{
			return false;
		}
		try
		{
			missionWeapon = equipment[equipmentIndex];
			return true;
		}
		catch
		{
			missionWeapon = default(MissionWeapon);
			return false;
		}
	}

	internal static bool TryGetSpawnEquipmentElement(Agent agent, EquipmentIndex equipmentIndex, out EquipmentElement equipmentElement)
	{
		equipmentElement = default(EquipmentElement);
		if (agent == null || !IsValidEquipmentSlot(equipmentIndex))
		{
			return false;
		}
		try
		{
			equipmentElement = agent.SpawnEquipment[equipmentIndex];
			return true;
		}
		catch
		{
			equipmentElement = default(EquipmentElement);
			return false;
		}
	}

	internal static bool TryGetSpawnWeapon(Agent agent, EquipmentIndex equipmentIndex, out EquipmentElement equipmentElement)
	{
		if (!IsValidWeaponSlot(equipmentIndex))
		{
			equipmentElement = default(EquipmentElement);
			return false;
		}
		return TryGetSpawnEquipmentElement(agent, equipmentIndex, out equipmentElement);
	}

	internal static bool TryGetSpawnItem(Agent agent, EquipmentIndex equipmentIndex, out ItemObject itemObject)
	{
		itemObject = null;
		if (!TryGetSpawnEquipmentElement(agent, equipmentIndex, out var equipmentElement))
		{
			return false;
		}
		try
		{
			itemObject = equipmentElement.Item;
			return itemObject != null;
		}
		catch
		{
			itemObject = null;
			return false;
		}
	}

	internal static bool TryGetItem(Agent agent, EquipmentIndex equipmentIndex, out ItemObject itemObject)
	{
		itemObject = null;
		if (!TryGetMissionWeapon(agent, equipmentIndex, out var missionWeapon))
		{
			return false;
		}
		try
		{
			itemObject = missionWeapon.Item;
			return itemObject != null;
		}
		catch
		{
			itemObject = null;
			return false;
		}
	}

	internal static bool TryGetAgentItem(Agent agent, EquipmentIndex equipmentIndex, out ItemObject itemObject)
	{
		itemObject = null;
		if (TryGetSpawnItem(agent, equipmentIndex, out itemObject))
		{
			return true;
		}
		if (!IsValidWeaponSlot(equipmentIndex))
		{
			itemObject = null;
			return false;
		}
		return TryGetItem(agent, equipmentIndex, out itemObject);
	}

	internal static bool TryGetItem(MissionEquipment equipment, EquipmentIndex equipmentIndex, out ItemObject itemObject)
	{
		itemObject = null;
		if (!TryGetMissionWeapon(equipment, equipmentIndex, out var missionWeapon))
		{
			return false;
		}
		try
		{
			itemObject = missionWeapon.Item;
			return itemObject != null;
		}
		catch
		{
			itemObject = null;
			return false;
		}
	}
}
