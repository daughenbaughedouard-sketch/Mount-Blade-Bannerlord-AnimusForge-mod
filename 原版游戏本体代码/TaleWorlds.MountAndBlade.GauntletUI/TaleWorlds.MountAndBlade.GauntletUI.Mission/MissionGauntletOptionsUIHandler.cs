using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions.AuxiliaryKeys;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions.GameKeys;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionOptionsUIHandler))]
public class MissionGauntletOptionsUIHandler : MissionView
{
	private GauntletLayer _gauntletLayer;

	private OptionsVM _dataSource;

	private GauntletMovieIdentifier _movie;

	private KeybindingPopup _keybindingPopup;

	private KeyOptionVM _currentKey;

	private SpriteCategory _optionsSpriteCategory;

	private bool _initialClothSimValue;

	public bool IsEnabled { get; private set; }

	public MissionGauntletOptionsUIHandler()
	{
		ViewOrderPriority = 49;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.OnMissionScreenInitialize();
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionOptionsComponent>().OnOptionsAdded += new OnMissionAddOptionsDelegate(OnShowOptions);
		_keybindingPopup = new KeybindingPopup(SetHotKey, (ScreenBase)(object)base.MissionScreen);
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionOptionsComponent>().OnOptionsAdded -= new OnMissionAddOptionsDelegate(OnShowOptions);
		base.OnMissionScreenFinalize();
		OptionsVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
		_movie = null;
		_keybindingPopup?.OnToggle(isActive: false);
		_keybindingPopup = null;
		_gauntletLayer = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
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
		_keybindingPopup?.Tick();
	}

	public override bool OnEscape()
	{
		if (_dataSource != null)
		{
			_dataSource.ExecuteCloseOptions();
			return true;
		}
		return base.OnEscape();
	}

	private void OnShowOptions()
	{
		IsEnabled = true;
		OnEscapeMenuToggled(isOpened: true);
		_initialClothSimValue = NativeOptions.GetConfig((NativeOptionsType)53) == 0f;
		InformationManager.HideAllMessages();
	}

	private void OnCloseOptions()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		IsEnabled = false;
		OnEscapeMenuToggled(isOpened: false);
		bool flag = NativeOptions.GetConfig((NativeOptionsType)53) == 0f;
		if (_initialClothSimValue != flag)
		{
			InformationManager.ShowInquiry(new InquiryData(((object)Module.CurrentModule.GlobalTextManager.FindText("str_option_wont_take_effect_mission_title", (string)null)).ToString(), ((object)Module.CurrentModule.GlobalTextManager.FindText("str_option_wont_take_effect_mission_desc", (string)null)).ToString(), true, false, ((object)Module.CurrentModule.GlobalTextManager.FindText("str_ok", (string)null)).ToString(), string.Empty, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
		}
	}

	public override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return _gauntletLayer == null;
	}

	private void OnEscapeMenuToggled(bool isOpened)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		if (isOpened)
		{
			if (!GameNetwork.IsMultiplayer)
			{
				MBCommon.PauseGameEngine();
			}
		}
		else
		{
			MBCommon.UnPauseGameEngine();
		}
		if (isOpened)
		{
			OptionsMode val = (OptionsMode)((!GameNetwork.IsMultiplayer) ? 1 : 2);
			_dataSource = new OptionsVM(val, (Action)OnCloseOptions, (Action<KeyOptionVM>)OnKeybindRequest, (Action)null, (Action)null);
			_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
			_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
			_dataSource.SetPreviousTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
			_dataSource.SetNextTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
			_dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
			_gauntletLayer = new GauntletLayer("MissionOptions", ++ViewOrderPriority, false);
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
			_optionsSpriteCategory = UIResourceManager.LoadSpriteCategory("ui_options");
			_movie = _gauntletLayer.LoadMovie("Options", (ViewModel)(object)_dataSource);
			((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
			Game current = Game.Current;
			if (current != null)
			{
				current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)13));
			}
		}
		else
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
			((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			_keybindingPopup?.OnToggle(isActive: false);
			_optionsSpriteCategory.Unload();
			_gauntletLayer = null;
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
			_gauntletLayer = null;
			Game current2 = Game.Current;
			if (current2 != null)
			{
				current2.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)8));
			}
		}
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
				Debug.FailedAssert("Could not find GameKeyGroup during SetHotKey", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\Mission\\MissionGauntletOptionsUIHandler.cs", "SetHotKey", 246);
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
					Debug.FailedAssert("Could not find AuxiliaryKeyGroup during SetHotKey", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\Mission\\MissionGauntletOptionsUIHandler.cs", "SetHotKey", 269);
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
