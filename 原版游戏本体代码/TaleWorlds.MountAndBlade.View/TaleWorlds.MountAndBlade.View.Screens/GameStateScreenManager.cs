using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens;

public class GameStateScreenManager : IGameStateManagerListener
{
	private Dictionary<Type, MBList<Type>> _screenTypes;

	private GameStateManager GameStateManager => GameStateManager.Current;

	public GameStateScreenManager()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		_screenTypes = new Dictionary<Type, MBList<Type>>();
		CollectTypes();
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	internal void CollectTypes()
	{
		_screenTypes.Clear();
		Assembly assembly = typeof(GameStateScreen).Assembly;
		Assembly[] referencingAssembliesSafe = Extensions.GetReferencingAssembliesSafe(assembly, (Func<Assembly, bool>)null);
		CheckAssemblyScreens(assembly);
		Assembly[] array = referencingAssembliesSafe;
		foreach (Assembly assembly2 in array)
		{
			CheckAssemblyScreens(assembly2);
		}
	}

	private void OnManagedOptionChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 28)
		{
			if (!BannerlordConfig.ForceVSyncInMenus)
			{
				Utilities.SetForceVsync(false);
			}
			else if (GameStateManager.ActiveState.IsMenuState)
			{
				Utilities.SetForceVsync(true);
			}
		}
	}

	private void CheckAssemblyScreens(Assembly assembly)
	{
		foreach (Type item in Extensions.GetTypesSafe(assembly, (Func<Type, bool>)null))
		{
			object[] customAttributesSafe = Extensions.GetCustomAttributesSafe(item, typeof(GameStateScreen), false);
			if (customAttributesSafe == null || customAttributesSafe.Length == 0)
			{
				continue;
			}
			object[] array = customAttributesSafe;
			for (int i = 0; i < array.Length; i++)
			{
				GameStateScreen gameStateScreen = (GameStateScreen)array[i];
				if (_screenTypes.ContainsKey(gameStateScreen.GameStateType))
				{
					((List<Type>)(object)_screenTypes[gameStateScreen.GameStateType]).Add(item);
					continue;
				}
				Dictionary<Type, MBList<Type>> screenTypes = _screenTypes;
				Type gameStateType = gameStateScreen.GameStateType;
				MBList<Type> obj = new MBList<Type>();
				((List<Type>)(object)obj).Add(item);
				screenTypes.Add(gameStateType, obj);
			}
		}
	}

	public ScreenBase CreateScreen(GameState state)
	{
		Type type = null;
		if (_screenTypes.TryGetValue(((object)state).GetType(), out var value))
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
			if (type != null)
			{
				object? obj = Activator.CreateInstance(type, state);
				return (ScreenBase)((obj is ScreenBase) ? obj : null);
			}
			Debug.FailedAssert($"Failed to create game state screen for state: {((object)state).GetType()}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Screens\\GameStateScreenManager.cs", "CreateScreen", 108);
		}
		return null;
	}

	public void BuildScreens()
	{
		int num = 0;
		foreach (GameState gameState in GameStateManager.GameStates)
		{
			ScreenBase val = CreateScreen(gameState);
			gameState.RegisterListener((IGameStateListener)(object)((val is IGameStateListener) ? val : null));
			if (val != null)
			{
				if (num == 0)
				{
					ScreenManager.CleanAndPushScreen(val);
				}
				else
				{
					ScreenManager.PushScreen(val);
				}
			}
			num++;
		}
	}

	void IGameStateManagerListener.OnCreateState(GameState gameState)
	{
		ScreenBase val = CreateScreen(gameState);
		if (val == null)
		{
			Debug.FailedAssert($"Create screen for {((MBObjectBase)gameState).GetName()} returned null.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Screens\\GameStateScreenManager.cs", "OnCreateState", 145);
		}
		gameState.RegisterListener((IGameStateListener)(object)((val is IGameStateListener) ? val : null));
	}

	void IGameStateManagerListener.OnPushState(GameState gameState, bool isTopGameState)
	{
		if (!gameState.IsMenuState)
		{
			Utilities.ClearOldResourcesAndObjects();
		}
		if (gameState.IsMenuState && BannerlordConfig.ForceVSyncInMenus)
		{
			Utilities.SetForceVsync(true);
		}
		else if (!gameState.IsMenuState)
		{
			Utilities.SetForceVsync(false);
		}
		ScreenBase listenerOfType;
		if ((listenerOfType = gameState.GetListenerOfType<ScreenBase>()) != null)
		{
			if (isTopGameState)
			{
				ScreenManager.CleanAndPushScreen(listenerOfType);
			}
			else
			{
				ScreenManager.PushScreen(listenerOfType);
			}
		}
		ThumbnailCacheManager.Current.ClearUnusedCache();
	}

	void IGameStateManagerListener.OnPopState(GameState gameState)
	{
		if (gameState.IsMenuState && BannerlordConfig.ForceVSyncInMenus)
		{
			Utilities.SetForceVsync(false);
		}
		if (GameStateManager.ActiveState != null && GameStateManager.ActiveState.IsMenuState && BannerlordConfig.ForceVSyncInMenus)
		{
			Utilities.SetForceVsync(true);
		}
		ScreenManager.PopScreen();
		if (!gameState.IsMenuState)
		{
			Utilities.ClearOldResourcesAndObjects();
		}
		ThumbnailCacheManager.Current.ClearUnusedCache();
	}

	void IGameStateManagerListener.OnCleanStates()
	{
		ScreenManager.CleanScreens();
	}

	void IGameStateManagerListener.OnSavedGameLoadFinished()
	{
		BuildScreens();
	}
}
