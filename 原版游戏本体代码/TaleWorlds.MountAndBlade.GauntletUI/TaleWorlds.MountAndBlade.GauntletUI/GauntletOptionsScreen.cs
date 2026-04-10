using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions.AuxiliaryKeys;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions.GameKeys;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

[OverrideView(typeof(OptionsScreen))]
public class GauntletOptionsScreen : ScreenBase
{
	private GauntletLayer _gauntletLayer;

	private OptionsVM _dataSource;

	private GauntletMovieIdentifier _gauntletMovie;

	private KeybindingPopup _keybindingPopup;

	private KeyOptionVM _currentKey;

	private SpriteCategory _optionsSpriteCategory;

	private bool _isFromMainMenu;

	public GauntletOptionsScreen(bool isFromMainMenu)
	{
		_isFromMainMenu = isFromMainMenu;
	}

	protected override void OnInitialize()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Expected O, but got Unknown
		((ScreenBase)this).OnInitialize();
		_optionsSpriteCategory = UIResourceManager.LoadSpriteCategory("ui_options");
		OptionsMode val = (OptionsMode)(!_isFromMainMenu);
		_dataSource = new OptionsVM(true, val, (Action<KeyOptionVM>)OnKeybindRequest, (Action)null, (Action)null);
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetPreviousTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		_dataSource.SetNextTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_dataSource.ExposurePopUp.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.ExposurePopUp.SetConfirmInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.BrightnessPopUp.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.BrightnessPopUp.SetConfirmInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_gauntletLayer = new GauntletLayer("OptionsScreen", 4000, false);
		_gauntletMovie = _gauntletLayer.LoadMovie("Options", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		_keybindingPopup = new KeybindingPopup(SetHotKey, (ScreenBase)(object)this);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		if (BannerlordConfig.ForceVSyncInMenus)
		{
			Utilities.SetForceVsync(true);
		}
		Game current = Game.Current;
		if (current != null)
		{
			current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)13));
		}
		InformationManager.HideAllMessages();
	}

	protected override void OnFinalize()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		((ScreenBase)this).OnFinalize();
		((ViewModel)_dataSource).OnFinalize();
		_optionsSpriteCategory.Unload();
		Utilities.SetForceVsync(false);
		Game current = Game.Current;
		if (current != null)
		{
			current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
		}
	}

	protected override void OnDeactivate()
	{
		LoadingWindow.EnableGlobalLoadingWindow();
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		if (_gauntletLayer != null && !_keybindingPopup.IsActive)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				if (_dataSource.ExposurePopUp.Visible)
				{
					_dataSource.ExposurePopUp.ExecuteCancel();
				}
				else if (_dataSource.BrightnessPopUp.Visible)
				{
					_dataSource.BrightnessPopUp.ExecuteCancel();
				}
				else
				{
					_dataSource.ExecuteCancel();
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				if (_dataSource.ExposurePopUp.Visible)
				{
					_dataSource.ExposurePopUp.ExecuteConfirm();
				}
				else if (_dataSource.BrightnessPopUp.Visible)
				{
					_dataSource.BrightnessPopUp.ExecuteConfirm();
				}
				else
				{
					_dataSource.ExecuteDone();
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("SwitchToPreviousTab"))
			{
				UISoundsHelper.PlayUISound("event:/ui/tab");
				_dataSource.SelectPreviousCategory();
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("SwitchToNextTab"))
			{
				UISoundsHelper.PlayUISound("event:/ui/tab");
				_dataSource.SelectNextCategory();
			}
		}
		_keybindingPopup.Tick();
	}

	private void OnKeybindRequest(KeyOptionVM requestedHotKeyToChange)
	{
		_currentKey = requestedHotKeyToChange;
		_keybindingPopup.OnToggle(isActive: true);
	}

	private void SetHotKey(Key key)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		if (key.IsControllerInput)
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=B41vvGuo}Invalid key", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
			_keybindingPopup.OnToggle(isActive: false);
			return;
		}
		GameKeyOptionVM gameKey = default(GameKeyOptionVM);
		ref GameKeyOptionVM reference = ref gameKey;
		KeyOptionVM currentKey = _currentKey;
		if ((reference = (GameKeyOptionVM)(object)((currentKey is GameKeyOptionVM) ? currentKey : null)) != null)
		{
			if (((IEnumerable<GameKeyGroupVM>)_dataSource.GameKeyOptionGroups.GameKeyGroups).FirstOrDefault((Func<GameKeyGroupVM, bool>)((GameKeyGroupVM g) => ((Collection<GameKeyOptionVM>)(object)g.GameKeys).Contains(gameKey))) == null)
			{
				Debug.FailedAssert("Could not find GameKeyGroup during SetHotKey", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletOptionsScreen.cs", "SetHotKey", 169);
				MBInformationManager.AddQuickInformation(new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
				_keybindingPopup.OnToggle(isActive: false);
				return;
			}
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit") || key.InputKey == ((KeyOptionVM)gameKey).CurrentKey.InputKey)
			{
				_keybindingPopup.OnToggle(isActive: false);
			}
			else
			{
				GameKeyOptionVM obj = gameKey;
				if (obj != null)
				{
					((KeyOptionVM)obj).Set(key.InputKey);
				}
				gameKey = null;
				_keybindingPopup.OnToggle(isActive: false);
			}
		}
		else
		{
			AuxiliaryKeyOptionVM auxiliaryKey = default(AuxiliaryKeyOptionVM);
			ref AuxiliaryKeyOptionVM reference2 = ref auxiliaryKey;
			KeyOptionVM currentKey2 = _currentKey;
			if ((reference2 = (AuxiliaryKeyOptionVM)(object)((currentKey2 is AuxiliaryKeyOptionVM) ? currentKey2 : null)) != null)
			{
				if (((IEnumerable<AuxiliaryKeyGroupVM>)_dataSource.GameKeyOptionGroups.AuxiliaryKeyGroups).FirstOrDefault((Func<AuxiliaryKeyGroupVM, bool>)((AuxiliaryKeyGroupVM g) => ((Collection<AuxiliaryKeyOptionVM>)(object)g.HotKeys).Contains(auxiliaryKey))) == null)
				{
					Debug.FailedAssert("Could not find AuxiliaryKeyGroup during SetHotKey", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletOptionsScreen.cs", "SetHotKey", 192);
					MBInformationManager.AddQuickInformation(new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					_keybindingPopup.OnToggle(isActive: false);
					return;
				}
				if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit") || key.InputKey == ((KeyOptionVM)auxiliaryKey).CurrentKey.InputKey)
				{
					_keybindingPopup.OnToggle(isActive: false);
				}
				else
				{
					AuxiliaryKeyOptionVM obj2 = auxiliaryKey;
					if (obj2 != null)
					{
						((KeyOptionVM)obj2).Set(key.InputKey);
					}
					auxiliaryKey = null;
					_keybindingPopup.OnToggle(isActive: false);
				}
			}
		}
		OptionsVM dataSource = _dataSource;
		if (dataSource != null)
		{
			GameKeyOptionCategoryVM gameKeyOptionGroups = dataSource.GameKeyOptionGroups;
			if (gameKeyOptionGroups != null)
			{
				((ViewModel)gameKeyOptionGroups).RefreshValues();
			}
		}
	}
}
