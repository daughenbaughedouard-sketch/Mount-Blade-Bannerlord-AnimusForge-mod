using System;
using System.Collections.Generic;
using System.Reflection;
using SandBox.View.Map;
using SandBox.View.Menu;
using SandBox.View.Missions;
using SandBox.View.Missions.NameMarkers;
using SandBox.View.Missions.Tournaments;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.ScreenSystem;

namespace SandBox.View;

public static class SandBoxViewCreator
{
	private static Dictionary<Type, MBList<Type>> _actualViewTypes;

	static SandBoxViewCreator()
	{
		CollectTypes();
	}

	private static void CollectTypes()
	{
		_actualViewTypes = new Dictionary<Type, MBList<Type>>();
		Assembly assembly = typeof(ViewCreatorModule).Assembly;
		Assembly[] referencingAssembliesSafe = Extensions.GetReferencingAssembliesSafe(assembly, (Func<Assembly, bool>)null);
		CheckOverridenViews(assembly);
		Assembly[] array = referencingAssembliesSafe;
		for (int i = 0; i < array.Length; i++)
		{
			CheckOverridenViews(array[i]);
		}
	}

	private static void CheckOverridenViews(Assembly assembly)
	{
		foreach (Type item in Extensions.GetTypesSafe(assembly, (Func<Type, bool>)null))
		{
			if (!typeof(MapView).IsAssignableFrom(item) && !typeof(MenuView).IsAssignableFrom(item) && !typeof(MissionView).IsAssignableFrom(item) && !typeof(ScreenBase).IsAssignableFrom(item))
			{
				continue;
			}
			object[] customAttributesSafe = Extensions.GetCustomAttributesSafe(item, typeof(OverrideView), false);
			if (customAttributesSafe == null || customAttributesSafe.Length != 1)
			{
				continue;
			}
			object obj = customAttributesSafe[0];
			OverrideView val = (OverrideView)((obj is OverrideView) ? obj : null);
			if (val != null)
			{
				if (_actualViewTypes.TryGetValue(val.BaseType, out var value))
				{
					((List<Type>)(object)value).Add(item);
					continue;
				}
				Dictionary<Type, MBList<Type>> actualViewTypes = _actualViewTypes;
				Type baseType = val.BaseType;
				MBList<Type> obj2 = new MBList<Type>();
				((List<Type>)(object)obj2).Add(item);
				actualViewTypes[baseType] = obj2;
			}
		}
	}

	public static ScreenBase CreateSaveLoadScreen(bool isSaving)
	{
		return ViewCreatorManager.CreateScreenView<SaveLoadScreen>(new object[1] { isSaving });
	}

	public static MissionView CreateMissionCraftingView()
	{
		return null;
	}

	public static MissionView CreateMissionNameMarkerUIHandler(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<MissionNameMarkerUIHandler>(mission != null, mission, Array.Empty<object>());
	}

	public static MissionView CreateMissionConversationView(Mission mission)
	{
		return ViewCreatorManager.CreateMissionView<MissionConversationView>(true, mission, Array.Empty<object>());
	}

	public static MissionView CreateMissionBarterView()
	{
		return ViewCreatorManager.CreateMissionView<BarterView>(false, (Mission)null, Array.Empty<object>());
	}

	public static MissionView CreateMissionAgentAlarmStateView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<MissionAgentAlarmStateView>(mission != null, mission, Array.Empty<object>());
	}

	public static MissionView CreateMissionMainAgentDetectionView(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<MissionMainAgentDetectionView>(mission != null, mission, Array.Empty<object>());
	}

	public static MissionView CreateMissionStealthFailCounter(Mission mission = null)
	{
		return ViewCreatorManager.CreateMissionView<MissionStealthFailCounterView>(mission != null, mission, Array.Empty<object>());
	}

	public static MissionView CreateMissionTournamentView()
	{
		return ViewCreatorManager.CreateMissionView<MissionTournamentView>(false, (Mission)null, Array.Empty<object>());
	}

	public static MissionView CreateMissionQuestBarView()
	{
		return ViewCreatorManager.CreateMissionView<MissionQuestBarView>(false, (Mission)null, Array.Empty<object>());
	}

	public static MapView CreateMapView<T>(params object[] parameters) where T : MapView
	{
		Type type = typeof(T);
		if (_actualViewTypes.TryGetValue(typeof(T), out var value))
		{
			MBList<Assembly> activeGameAssemblies = ModuleHelper.GetActiveGameAssemblies();
			for (int num = ((List<Type>)(object)value).Count - 1; num >= 0; num--)
			{
				if (((List<Assembly>)(object)activeGameAssemblies).Contains(((List<Type>)(object)value)[num].Assembly))
				{
					type = ((List<Type>)(object)value)[num];
					break;
				}
			}
		}
		return Activator.CreateInstance(type, parameters) as MapView;
	}

	public static MenuView CreateMenuView<T>(params object[] parameters) where T : MenuView
	{
		Type type = typeof(T);
		if (_actualViewTypes.TryGetValue(typeof(T), out var value))
		{
			MBList<Assembly> activeGameAssemblies = ModuleHelper.GetActiveGameAssemblies();
			for (int num = ((List<Type>)(object)value).Count - 1; num >= 0; num--)
			{
				if (((List<Assembly>)(object)activeGameAssemblies).Contains(((List<Type>)(object)value)[num].Assembly))
				{
					type = ((List<Type>)(object)value)[num];
					break;
				}
			}
		}
		return Activator.CreateInstance(type, parameters) as MenuView;
	}

	public static MissionView CreateBoardGameView()
	{
		return ViewCreatorManager.CreateMissionView<BoardGameView>(false, (Mission)null, Array.Empty<object>());
	}

	public static MissionView CreateMissionArenaPracticeFightView()
	{
		return ViewCreatorManager.CreateMissionView<MissionArenaPracticeFightView>(false, (Mission)null, Array.Empty<object>());
	}
}
