using System;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;
using TaleWorlds.MountAndBlade.ViewModelCollection.InitialMenu;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

[GameStateScreen(typeof(InitialState))]
public class GauntletInitialScreen : MBInitialScreenBase, IChatLogHandlerScreen
{
	private GauntletLayer _gauntletLayer;

	private GauntletLayer _gauntletBrightnessLayer;

	private GauntletLayer _gauntletExposureLayer;

	private InitialMenuVM _dataSource;

	private BrightnessOptionVM _brightnessOptionDataSource;

	private ExposureOptionVM _exposureOptionDataSource;

	private GauntletMovieIdentifier _brightnessOptionMovie;

	private GauntletMovieIdentifier _exposureOptionMovie;

	public GauntletInitialScreen(InitialState initialState)
		: base(initialState)
	{
	}

	protected override void OnInitialize()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		((MBInitialScreenBase)this).OnInitialize();
		_dataSource = new InitialMenuVM(((MBInitialScreenBase)this)._state);
		_gauntletLayer = new GauntletLayer("MainMenu", 1, false);
		_gauntletLayer.LoadMovie("InitialScreen", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)3);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		if (NativeOptions.GetConfig((NativeOptionsType)69) < 4f)
		{
			_brightnessOptionDataSource = new BrightnessOptionVM((Action<bool>)OnCloseBrightness)
			{
				Visible = true
			};
			_gauntletBrightnessLayer = new GauntletLayer("MainMenuBrightness", 2, false);
			((ScreenLayer)_gauntletBrightnessLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)3);
			_brightnessOptionMovie = _gauntletBrightnessLayer.LoadMovie("BrightnessOption", (ViewModel)(object)_brightnessOptionDataSource);
			((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletBrightnessLayer);
		}
		GauntletFullScreenNoticeView.Initialize();
		GauntletGameNotification.Initialize();
		GauntletChatLogView.Current?.LoadMovie(forMultiplayer: false);
		InformationManager.ClearAllMessages();
		((MBInitialScreenBase)this)._state.OnGameContentUpdated += new OnGameContentUpdatedDelegate(OnGameContentUpdated);
		SetGainNavigationAfterFrames(3);
	}

	protected override void OnInitialScreenTick(float dt)
	{
		((MBInitialScreenBase)this).OnInitialScreenTick(dt);
		if (ScreenManager.IsMouseCursorHidden())
		{
			MouseManager.ShowCursor(false);
			MouseManager.ShowCursor(true);
		}
		InitialMenuVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.Tick();
		}
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			BrightnessOptionVM brightnessOptionDataSource = _brightnessOptionDataSource;
			if (brightnessOptionDataSource != null && brightnessOptionDataSource.Visible)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_brightnessOptionDataSource.ExecuteCancel();
				return;
			}
			ExposureOptionVM exposureOptionDataSource = _exposureOptionDataSource;
			if (exposureOptionDataSource != null && exposureOptionDataSource.Visible)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_exposureOptionDataSource.ExecuteCancel();
			}
		}
		else
		{
			if (!((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
			{
				return;
			}
			BrightnessOptionVM brightnessOptionDataSource2 = _brightnessOptionDataSource;
			if (brightnessOptionDataSource2 != null && brightnessOptionDataSource2.Visible)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_brightnessOptionDataSource.ExecuteConfirm();
				return;
			}
			ExposureOptionVM exposureOptionDataSource2 = _exposureOptionDataSource;
			if (exposureOptionDataSource2 != null && exposureOptionDataSource2.Visible)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_exposureOptionDataSource.ExecuteConfirm();
			}
		}
	}

	protected override void OnActivate()
	{
		((MBInitialScreenBase)this).OnActivate();
		InitialMenuVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.RefreshMenuOptions();
		}
		SetGainNavigationAfterFrames(3);
	}

	private void SetGainNavigationAfterFrames(int frameCount)
	{
		_gauntletLayer.UIContext.GamepadNavigation.GainNavigationAfterFrames(frameCount, (Func<bool>)delegate
		{
			BrightnessOptionVM brightnessOptionDataSource = _brightnessOptionDataSource;
			if (brightnessOptionDataSource == null || !brightnessOptionDataSource.Visible)
			{
				ExposureOptionVM exposureOptionDataSource = _exposureOptionDataSource;
				if (exposureOptionDataSource == null)
				{
					return true;
				}
				return !exposureOptionDataSource.Visible;
			}
			return false;
		});
	}

	private void OnGameContentUpdated()
	{
		InitialMenuVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.RefreshMenuOptions();
		}
	}

	private void OnCloseBrightness(bool isConfirm)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		_gauntletBrightnessLayer.ReleaseMovie(_brightnessOptionMovie);
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletBrightnessLayer);
		_brightnessOptionDataSource = null;
		_gauntletBrightnessLayer = null;
		NativeOptions.SaveConfig();
		OpenExposureControl();
	}

	private void OpenExposureControl()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		_exposureOptionDataSource = new ExposureOptionVM((Action<bool>)OnCloseExposureControl)
		{
			Visible = true
		};
		_gauntletExposureLayer = new GauntletLayer("MainMenuExposure", 2, false);
		((ScreenLayer)_gauntletExposureLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)3);
		_exposureOptionMovie = _gauntletExposureLayer.LoadMovie("ExposureOption", (ViewModel)(object)_exposureOptionDataSource);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletExposureLayer);
	}

	private void OnCloseExposureControl(bool isConfirm)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		_gauntletExposureLayer.ReleaseMovie(_exposureOptionMovie);
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletExposureLayer);
		_exposureOptionDataSource = null;
		_gauntletExposureLayer = null;
		NativeOptions.SaveConfig();
	}

	protected override void OnFinalize()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		((MBInitialScreenBase)this).OnFinalize();
		if (((MBInitialScreenBase)this)._state != null)
		{
			((MBInitialScreenBase)this)._state.OnGameContentUpdated -= new OnGameContentUpdatedDelegate(OnGameContentUpdated);
		}
		if (_gauntletLayer != null)
		{
			((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		}
		InitialMenuVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
		_gauntletLayer = null;
	}

	public void TryUpdateChatLogLayerParameters(ref bool isTeamChatAvailable, ref bool inputEnabled, ref bool isToggleChatHintAvailable, ref bool isMouseVisible, ref InputContext inputContext)
	{
		inputEnabled = false;
		inputContext = null;
	}
}
