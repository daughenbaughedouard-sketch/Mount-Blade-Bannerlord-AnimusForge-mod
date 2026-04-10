using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Information.RundownTooltip;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Options;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.SceneNotification;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletUISubModule : MBSubModuleBase
{
	private bool _initialized;

	private bool _isMultiplayer;

	private GauntletQueryManager _queryManager;

	private SpriteCategory _fullBackgroundCategory;

	private SpriteCategory _fullscreensCategory;

	private bool _areResourcesDirty;

	private bool _loadingWindowCreated;

	public static GauntletUISubModule Instance { get; private set; }

	protected override void OnSubModuleLoad()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		((MBSubModuleBase)this).OnSubModuleLoad();
		CustomWidgetManager.TouchAssembly();
		BannerlordCustomWidgetManager.TouchAssembly();
		RefreshResources(initialLoad: true);
		ScreenManager.OnControllerDisconnected += new OnControllerDisconnectedEvent(OnControllerDisconnected);
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		NativeOptions.GetConfig((NativeOptionsType)19);
		GauntletGamepadNavigationManager.Initialize();
		GauntletGameVersionView.AddModuleVersionInfo("Bannerlord", ((object)Utilities.GetApplicationVersionWithBuildNumber()/*cast due to .constrained prefix*/).ToString());
		Instance = this;
	}

	private void RefreshResources(bool initialLoad)
	{
		Dictionary<GauntletLayer, List<GauntletMovieIdentifier>> dictionary = new Dictionary<GauntletLayer, List<GauntletMovieIdentifier>>();
		if (!initialLoad)
		{
			List<GauntletMovieIdentifier> value = default(List<GauntletMovieIdentifier>);
			foreach (ScreenLayer sortedLayer in ScreenManager.SortedLayers)
			{
				GauntletLayer val;
				if ((val = (GauntletLayer)(object)((sortedLayer is GauntletLayer) ? sortedLayer : null)) != null)
				{
					val.OnResourceRefreshBegin(ref value);
					dictionary.Add(val, value);
				}
			}
		}
		WidgetInfo.Refresh();
		UIResourceManager.Refresh();
		SpriteCategory fullBackgroundCategory = _fullBackgroundCategory;
		if (fullBackgroundCategory != null)
		{
			fullBackgroundCategory.Unload();
		}
		SpriteCategory fullscreensCategory = _fullscreensCategory;
		if (fullscreensCategory != null)
		{
			fullscreensCategory.Unload();
		}
		_fullBackgroundCategory = UIResourceManager.LoadSpriteCategory("ui_fullbackgrounds");
		_fullscreensCategory = UIResourceManager.LoadSpriteCategory("ui_fullscreens");
		GauntletGameVersionView.Refresh();
		SpriteCategory[] array = UIResourceManager.SpriteData.SpriteCategories.Values.Where((SpriteCategory x) => x.AlwaysLoad).ToArray();
		float num = 0.2f / (float)(array.Length - 1);
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			Extensions.Load(array[num2]);
			if (initialLoad)
			{
				Utilities.SetLoadingScreenPercentage(0.4f + (float)num2 * num);
			}
		}
		if (initialLoad)
		{
			Utilities.SetLoadingScreenPercentage(0.6f);
		}
		if (initialLoad)
		{
			return;
		}
		foreach (ScreenLayer sortedLayer2 in ScreenManager.SortedLayers)
		{
			GauntletLayer val2;
			if ((val2 = (GauntletLayer)(object)((sortedLayer2 is GauntletLayer) ? sortedLayer2 : null)) != null)
			{
				val2.OnResourceRefreshEnd(dictionary[val2]);
			}
		}
	}

	private void OnControllerDisconnected()
	{
	}

	private void OnManagedOptionChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 0)
		{
			UIResourceManager.OnLanguageChange(BannerlordConfig.Language);
			ScreenManager.UpdateLayout();
		}
		else if ((int)changedManagedOptionsType == 31)
		{
			ScreenManager.OnScaleChange(BannerlordConfig.UIScale);
		}
	}

	protected override void OnNewModuleLoad()
	{
		_areResourcesDirty = true;
	}

	protected override void OnSubModuleUnloaded()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		ScreenManager.OnControllerDisconnected -= new OnControllerDisconnectedEvent(OnControllerDisconnected);
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		UIResourceManager.Clear();
		if (GauntletGamepadNavigationManager.Instance != null)
		{
			GauntletGamepadNavigationManager.Instance.OnFinalize();
		}
		SpriteCategory fullBackgroundCategory = _fullBackgroundCategory;
		if (fullBackgroundCategory != null)
		{
			fullBackgroundCategory.Unload();
		}
		SpriteCategory fullscreensCategory = _fullscreensCategory;
		if (fullscreensCategory != null)
		{
			fullscreensCategory.Unload();
		}
		GauntletGameVersionView.RemoveModuleVersionInfo("Bannerlord");
		Instance = null;
		GauntletInformationView.OnFinalize();
		((MBSubModuleBase)this).OnSubModuleUnloaded();
	}

	protected override void OnBeforeInitialModuleScreenSetAsRoot()
	{
		if (!_initialized)
		{
			if (!Utilities.CommandLineArgumentExists("VisualTests"))
			{
				GauntletInformationView.Initialize();
				GauntletSceneNotification.Initialize();
				GauntletSceneNotification.Current.RegisterContextProvider((ISceneNotificationContextProvider)(object)new NativeSceneNotificationContextProvider());
				GauntletChatLogView.Initialize();
				GauntletGamepadCursor.Initialize();
				GauntletGameVersionView.Initialize();
				GauntletCameraFadeView.Initialize();
				InformationManager.RegisterTooltip<List<TooltipProperty>, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)PropertyBasedTooltipVM.RefreshGenericPropertyBasedTooltip, "PropertyBasedTooltip");
				InformationManager.RegisterTooltip<RundownLineVM, RundownTooltipVM>((Action<RundownTooltipVM, object[]>)RundownTooltipVM.RefreshGenericRundownTooltip, "RundownTooltip");
				InformationManager.RegisterTooltip<string, HintVM>((Action<HintVM, object[]>)HintVM.RefreshGenericHintTooltip, "HintTooltip");
				_queryManager = new GauntletQueryManager();
				_queryManager.Initialize();
				_queryManager.InitializeKeyVisuals();
			}
			UIResourceManager.OnLanguageChange(BannerlordConfig.Language);
			ScreenManager.OnScaleChange(BannerlordConfig.UIScale);
			_initialized = true;
		}
	}

	public override void OnMultiplayerGameStart(Game game, object starterObject)
	{
		((MBSubModuleBase)this).OnMultiplayerGameStart(game, starterObject);
		if (!_isMultiplayer)
		{
			ILoadingWindowManager loadingWindowManager = LoadingWindow.LoadingWindowManager;
			if (loadingWindowManager != null)
			{
				loadingWindowManager.SetCurrentModeIsMultiplayer(true);
			}
			_isMultiplayer = true;
		}
	}

	public override void OnGameEnd(Game game)
	{
		((MBSubModuleBase)this).OnGameEnd(game);
		if (_isMultiplayer)
		{
			ILoadingWindowManager loadingWindowManager = LoadingWindow.LoadingWindowManager;
			if (loadingWindowManager != null)
			{
				loadingWindowManager.SetCurrentModeIsMultiplayer(false);
			}
			_isMultiplayer = false;
		}
	}

	protected override void OnApplicationTick(float dt)
	{
		if (_areResourcesDirty)
		{
			RefreshResources(initialLoad: false);
			_areResourcesDirty = false;
		}
		((MBSubModuleBase)this).OnApplicationTick(dt);
		if (!_loadingWindowCreated)
		{
			if (LoadingWindow.LoadingWindowManager == null)
			{
				LoadingWindow.InitializeWith<GauntletDefaultLoadingWindowManager>();
			}
			_loadingWindowCreated = true;
		}
		UIResourceManager.Update();
		if (GauntletGamepadNavigationManager.Instance != null && ScreenManager.GetMouseVisibility())
		{
			GauntletGamepadNavigationManager.Instance.IsTouchpadMouseEnabled = NativeOptions.GetConfig((NativeOptionsType)17) != 0f;
			GauntletGamepadNavigationManager.Instance.Update(dt);
		}
	}

	[CommandLineArgumentFunction("clear", "chatlog")]
	public static string ClearChatLog(List<string> strings)
	{
		InformationManager.ClearAllMessages();
		return "Chatlog cleared!";
	}

	[CommandLineArgumentFunction("can_focus_while_in_mission", "chatlog")]
	public static string SetCanFocusWhileInMission(List<string> strings)
	{
		if (strings[0] == "0" || strings[0] == "1")
		{
			GauntletChatLogView.Current.SetCanFocusWhileInMission(strings[0] == "1");
			return "Chat window will" + ((strings[0] == "1") ? " " : " NOT ") + " be able to gain focus now.";
		}
		return "Wrong input";
	}
}
