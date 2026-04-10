using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace AnimusForge;

internal static class AgentWeaponCompat
{
	private static readonly HashSet<string> _loggedFailureKeys = new HashSet<string>();

	private static readonly object _logLock = new object();

	internal static bool TryWieldInitialWeapons(Agent agent, Agent.WeaponWieldActionType wieldActionType, Equipment.InitialWeaponEquipPreference initialWeaponEquipPreference, string source)
	{
		if (!CanSafelyWieldInitialWeapons(agent))
		{
			return false;
		}
		try
		{
			agent.WieldInitialWeapons(wieldActionType, initialWeaponEquipPreference);
			return true;
		}
		catch (System.Exception ex)
		{
			TryLogFailureOnce(source, ex);
			return false;
		}
	}

	private static bool CanSafelyWieldInitialWeapons(Agent agent)
	{
		if (agent == null)
		{
			return false;
		}
		try
		{
			if (!agent.IsActive() || !agent.IsHuman)
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
		try
		{
			foreach (EquipmentIndex equipmentIndex in MissionEquipmentCompat.EnumerateWeaponSlots())
			{
				if (MissionEquipmentCompat.TryGetMissionWeapon(agent, equipmentIndex, out var missionWeapon) && !missionWeapon.IsEmpty && missionWeapon.Item != null)
				{
					return true;
				}
			}
		}
		catch
		{
			return false;
		}
		return false;
	}

	private static void TryLogFailureOnce(string source, System.Exception ex)
	{
		string text = (string.IsNullOrWhiteSpace(source) ? "unknown" : source) + "|" + ex.GetType().FullName;
		lock (_logLock)
		{
			if (!_loggedFailureKeys.Add(text))
			{
				return;
			}
		}
		Logger.LogTrace("AgentWeaponCompat", $"WieldInitialWeapons skipped at {source}: {ex.GetType().Name}: {ex.Message}");
	}
}
