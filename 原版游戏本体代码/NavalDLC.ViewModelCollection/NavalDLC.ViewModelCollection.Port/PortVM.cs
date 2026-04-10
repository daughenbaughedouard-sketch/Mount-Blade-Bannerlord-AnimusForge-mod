using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NavalDLC.HotKeyCategories;
using NavalDLC.ViewModelCollection.Port.PortScreenHandlers;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port;

public class PortVM : ViewModel
{
	private readonly PortScreenHandler _portScreenHandler;

	private readonly PortScreenModes _portScreenMode;

	private readonly Action<Ship> _onShipSelected;

	private readonly Action _onRostersRefreshed;

	private readonly Action<ShipItemVM> _refreshShipVisual;

	private readonly Action _onUpgradeSlotSelected;

	private readonly MBList<ShipItemVM> _allShips;

	private List<PortChangeInfo> _cachedChanges;

	private PortActionVM _buyAction;

	private PortActionVM _sellAction;

	private PortActionVM _repairAction;

	private PortActionVM _sendToClanAction;

	private PortActionVM _repairAllAction;

	private bool _isConfirmDisabled;

	private bool _canUseKeyboardInputs;

	private bool _canUseGamepadInputs;

	private bool _isControllingCamera;

	private bool _canToggleCamera = true;

	private bool _isMapBarExtended;

	private bool _isAnyUpgradeSlotSelected;

	private bool _isNight;

	private int _totalGoldCost;

	private string _keyboardMoveCameraText;

	private string _cancelText;

	private string _confirmText;

	private string _totalGoldCostText;

	private string _repairText;

	private string _upgradeText;

	private string _buyText;

	private string _sellText;

	private HintViewModel _canConfirmHint;

	private BasicTooltipViewModel _goldCostHint;

	private ShipRosterVM _leftRoster;

	private ShipRosterVM _rightRoster;

	private ShipItemVM _selectedShip;

	private ShipUpgradePieceBaseVM _inspectedUpgrade;

	private ShipUpgradeSlotBaseVM _selectedUpgradeSlot;

	private InputKeyItemVM _resetInputKey;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private InputKeyItemVM _selectPreviousShipInputKey;

	private InputKeyItemVM _selectNextShipInputKey;

	private InputKeyItemVM _selectLeftRosterInputKey;

	private InputKeyItemVM _selectRightRosterInputKey;

	private InputKeyItemVM _gamepadToggleCameraInputKey;

	private MBBindingList<InputKeyItemVM> _gamepadCameraControlKeys;

	private InputKeyItemVM _keyboardRotateCameraInputKey;

	private MBBindingList<InputKeyItemVM> _keyboardMoveCameraInputKeys;

	public MBReadOnlyList<ShipItemVM> AllShips => (MBReadOnlyList<ShipItemVM>)(object)_allShips;

	[DataSourceProperty]
	public PortActionVM BuyAction
	{
		get
		{
			return _buyAction;
		}
		set
		{
			if (value != _buyAction)
			{
				_buyAction = value;
				((ViewModel)this).OnPropertyChangedWithValue<PortActionVM>(value, "BuyAction");
			}
		}
	}

	[DataSourceProperty]
	public PortActionVM SellAction
	{
		get
		{
			return _sellAction;
		}
		set
		{
			if (value != _sellAction)
			{
				_sellAction = value;
				((ViewModel)this).OnPropertyChangedWithValue<PortActionVM>(value, "SellAction");
			}
		}
	}

	[DataSourceProperty]
	public PortActionVM RepairAction
	{
		get
		{
			return _repairAction;
		}
		set
		{
			if (value != _repairAction)
			{
				_repairAction = value;
				((ViewModel)this).OnPropertyChangedWithValue<PortActionVM>(value, "RepairAction");
			}
		}
	}

	[DataSourceProperty]
	public bool IsConfirmDisabled
	{
		get
		{
			return _isConfirmDisabled;
		}
		set
		{
			if (value != _isConfirmDisabled)
			{
				_isConfirmDisabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsConfirmDisabled");
			}
		}
	}

	[DataSourceProperty]
	public PortActionVM SendToClanAction
	{
		get
		{
			return _sendToClanAction;
		}
		set
		{
			if (value != _sendToClanAction)
			{
				_sendToClanAction = value;
				((ViewModel)this).OnPropertyChangedWithValue<PortActionVM>(value, "SendToClanAction");
			}
		}
	}

	[DataSourceProperty]
	public PortActionVM RepairAllAction
	{
		get
		{
			return _repairAllAction;
		}
		set
		{
			if (value != _repairAllAction)
			{
				_repairAllAction = value;
				((ViewModel)this).OnPropertyChangedWithValue<PortActionVM>(value, "RepairAllAction");
			}
		}
	}

	[DataSourceProperty]
	public bool CanUseKeyboardInputs
	{
		get
		{
			return _canUseKeyboardInputs;
		}
		set
		{
			if (value != _canUseKeyboardInputs)
			{
				_canUseKeyboardInputs = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanUseKeyboardInputs");
			}
		}
	}

	[DataSourceProperty]
	public bool CanUseGamepadInputs
	{
		get
		{
			return _canUseGamepadInputs;
		}
		set
		{
			if (value != _canUseGamepadInputs)
			{
				_canUseGamepadInputs = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanUseGamepadInputs");
			}
		}
	}

	[DataSourceProperty]
	public bool IsControllingCamera
	{
		get
		{
			return _isControllingCamera;
		}
		set
		{
			if (value != _isControllingCamera)
			{
				_isControllingCamera = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsControllingCamera");
				UpdateGamepadCameraControlButtonsVisibility();
			}
		}
	}

	[DataSourceProperty]
	public bool CanToggleCamera
	{
		get
		{
			return _canToggleCamera;
		}
		set
		{
			if (value != _canToggleCamera)
			{
				_canToggleCamera = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanToggleCamera");
				UpdateGamepadCameraControlButtonsVisibility();
			}
		}
	}

	[DataSourceProperty]
	public bool IsMapBarExtended
	{
		get
		{
			return _isMapBarExtended;
		}
		set
		{
			if (value != _isMapBarExtended)
			{
				_isMapBarExtended = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMapBarExtended");
			}
		}
	}

	[DataSourceProperty]
	public string KeyboardMoveCameraText
	{
		get
		{
			return _keyboardMoveCameraText;
		}
		set
		{
			if (value != _keyboardMoveCameraText)
			{
				_keyboardMoveCameraText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "KeyboardMoveCameraText");
			}
		}
	}

	[DataSourceProperty]
	public string CancelText
	{
		get
		{
			return _cancelText;
		}
		set
		{
			if (value != _cancelText)
			{
				_cancelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelText");
			}
		}
	}

	[DataSourceProperty]
	public string ConfirmText
	{
		get
		{
			return _confirmText;
		}
		set
		{
			if (value != _confirmText)
			{
				_confirmText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ConfirmText");
			}
		}
	}

	[DataSourceProperty]
	public int TotalGoldCost
	{
		get
		{
			return _totalGoldCost;
		}
		set
		{
			if (value != _totalGoldCost)
			{
				_totalGoldCost = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TotalGoldCost");
			}
		}
	}

	[DataSourceProperty]
	public string TotalGoldCostText
	{
		get
		{
			return _totalGoldCostText;
		}
		set
		{
			if (value != _totalGoldCostText)
			{
				_totalGoldCostText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TotalGoldCostText");
			}
		}
	}

	[DataSourceProperty]
	public string RepairText
	{
		get
		{
			return _repairText;
		}
		set
		{
			if (value != _repairText)
			{
				_repairText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RepairText");
			}
		}
	}

	[DataSourceProperty]
	public string UpgradeText
	{
		get
		{
			return _upgradeText;
		}
		set
		{
			if (value != _upgradeText)
			{
				_upgradeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "UpgradeText");
			}
		}
	}

	[DataSourceProperty]
	public string BuyText
	{
		get
		{
			return _buyText;
		}
		set
		{
			if (value != _buyText)
			{
				_buyText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BuyText");
			}
		}
	}

	[DataSourceProperty]
	public string SellText
	{
		get
		{
			return _sellText;
		}
		set
		{
			if (value != _sellText)
			{
				_sellText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SellText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAnyUpgradeSlotSelected
	{
		get
		{
			return _isAnyUpgradeSlotSelected;
		}
		set
		{
			if (value != _isAnyUpgradeSlotSelected)
			{
				_isAnyUpgradeSlotSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAnyUpgradeSlotSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNight
	{
		get
		{
			return _isNight;
		}
		set
		{
			if (value == _isNight)
			{
				return;
			}
			_isNight = value;
			((ViewModel)this).OnPropertyChangedWithValue(value, "IsNight");
			foreach (ShipItemVM item in (List<ShipItemVM>)(object)AllShips)
			{
				item.IsNight = value;
			}
		}
	}

	[DataSourceProperty]
	public ShipRosterVM LeftRoster
	{
		get
		{
			return _leftRoster;
		}
		set
		{
			if (value != _leftRoster)
			{
				_leftRoster = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipRosterVM>(value, "LeftRoster");
			}
		}
	}

	[DataSourceProperty]
	public ShipRosterVM RightRoster
	{
		get
		{
			return _rightRoster;
		}
		set
		{
			if (value != _rightRoster)
			{
				_rightRoster = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipRosterVM>(value, "RightRoster");
			}
		}
	}

	[DataSourceProperty]
	public ShipItemVM SelectedShip
	{
		get
		{
			return _selectedShip;
		}
		set
		{
			if (value != _selectedShip)
			{
				if (_selectedShip != null)
				{
					_selectedShip.IsSelected = false;
				}
				_selectedShip = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipItemVM>(value, "SelectedShip");
				if (_selectedShip != null)
				{
					_selectedShip.IsSelected = true;
				}
			}
		}
	}

	[DataSourceProperty]
	public ShipUpgradeSlotBaseVM SelectedUpgradeSlot
	{
		get
		{
			return _selectedUpgradeSlot;
		}
		set
		{
			if (value != _selectedUpgradeSlot)
			{
				_selectedUpgradeSlot = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipUpgradeSlotBaseVM>(value, "SelectedUpgradeSlot");
				IsAnyUpgradeSlotSelected = _selectedUpgradeSlot != null;
			}
		}
	}

	[DataSourceProperty]
	public ShipUpgradePieceBaseVM InspectedUpgrade
	{
		get
		{
			return _inspectedUpgrade;
		}
		set
		{
			if (value != _inspectedUpgrade)
			{
				_inspectedUpgrade = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipUpgradePieceBaseVM>(value, "InspectedUpgrade");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CanConfirmHint
	{
		get
		{
			return _canConfirmHint;
		}
		set
		{
			if (value != _canConfirmHint)
			{
				_canConfirmHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "CanConfirmHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel GoldCostHint
	{
		get
		{
			return _goldCostHint;
		}
		set
		{
			if (value != _goldCostHint)
			{
				_goldCostHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<BasicTooltipViewModel>(value, "GoldCostHint");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM ResetInputKey
	{
		get
		{
			return _resetInputKey;
		}
		set
		{
			if (value != _resetInputKey)
			{
				_resetInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "ResetInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM CancelInputKey
	{
		get
		{
			return _cancelInputKey;
		}
		set
		{
			if (value != _cancelInputKey)
			{
				_cancelInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "CancelInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM DoneInputKey
	{
		get
		{
			return _doneInputKey;
		}
		set
		{
			if (value != _doneInputKey)
			{
				_doneInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "DoneInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM SelectPreviousShipInputKey
	{
		get
		{
			return _selectPreviousShipInputKey;
		}
		set
		{
			if (value != _selectPreviousShipInputKey)
			{
				_selectPreviousShipInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "SelectPreviousShipInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM SelectNextShipInputKey
	{
		get
		{
			return _selectNextShipInputKey;
		}
		set
		{
			if (value != _selectNextShipInputKey)
			{
				_selectNextShipInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "SelectNextShipInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM SelectLeftRosterInputKey
	{
		get
		{
			return _selectLeftRosterInputKey;
		}
		set
		{
			if (value != _selectLeftRosterInputKey)
			{
				_selectLeftRosterInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "SelectLeftRosterInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM SelectRightRosterInputKey
	{
		get
		{
			return _selectRightRosterInputKey;
		}
		set
		{
			if (value != _selectRightRosterInputKey)
			{
				_selectRightRosterInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "SelectRightRosterInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM GamepadToggleCameraInputKey
	{
		get
		{
			return _gamepadToggleCameraInputKey;
		}
		set
		{
			if (value != _gamepadToggleCameraInputKey)
			{
				_gamepadToggleCameraInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "GamepadToggleCameraInputKey");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<InputKeyItemVM> GamepadCameraControlKeys
	{
		get
		{
			return _gamepadCameraControlKeys;
		}
		set
		{
			if (value != _gamepadCameraControlKeys)
			{
				_gamepadCameraControlKeys = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<InputKeyItemVM>>(value, "GamepadCameraControlKeys");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<InputKeyItemVM> KeyboardMoveCameraInputKeys
	{
		get
		{
			return _keyboardMoveCameraInputKeys;
		}
		set
		{
			if (value != _keyboardMoveCameraInputKeys)
			{
				_keyboardMoveCameraInputKeys = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<InputKeyItemVM>>(value, "KeyboardMoveCameraInputKeys");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM KeyboardRotateCameraInputKey
	{
		get
		{
			return _keyboardRotateCameraInputKey;
		}
		set
		{
			if (value != _keyboardRotateCameraInputKey)
			{
				_keyboardRotateCameraInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "KeyboardRotateCameraInputKey");
			}
		}
	}

	public PortVM(PortScreenHandler portScreenHandler, PortScreenModes portScreenMode, Action<Ship> onShipSelected, Action onRostersRefreshed, Action<ShipItemVM> refreshShipVisual, Action onUpgradeSlotSelected)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		_portScreenHandler = portScreenHandler;
		_portScreenMode = portScreenMode;
		_onShipSelected = onShipSelected;
		_onRostersRefreshed = onRostersRefreshed;
		_refreshShipVisual = refreshShipVisual;
		_onUpgradeSlotSelected = onUpgradeSlotSelected;
		ShipItemVM.OnSelected += OnShipSelected;
		ShipItemVM.OnRenamed += OnShipRenamed;
		ShipItemVM.OnNameReset += OnShipNameReset;
		ShipUpgradePieceBaseVM.OnInspected += OnShipPieceInspected;
		ShipUpgradeSlotBaseVM.OnShipPieceSelected += OnShipPieceSelected;
		ShipUpgradeContainerVM.OnSlotSelected = (ShipUpgradeContainerVM.ShipSlotSelectedDelegate)Delegate.Combine(ShipUpgradeContainerVM.OnSlotSelected, new ShipUpgradeContainerVM.ShipSlotSelectedDelegate(OnUpgradeSlotSelected));
		ShipFigureheadSlotVM.GetCurrentFigurehead += GetCurrentFigurehead;
		ShipFigureheadSlotVM.GetShipOfFigurehead += GetShipOfFigurehead;
		ShipFigureheadSlotVM.GetIsRightSide += GetIsShipRightSide;
		ShipUpgradePieceVM.GetUpgradePrice += GetUpgradePrice;
		_allShips = new MBList<ShipItemVM>();
		for (int i = 0; i < ((List<Ship>)(object)_portScreenHandler.LeftShips).Count; i++)
		{
			((List<ShipItemVM>)(object)_allShips).Add(new ShipItemVM(((List<Ship>)(object)_portScreenHandler.LeftShips)[i]));
		}
		for (int j = 0; j < ((List<Ship>)(object)_portScreenHandler.RightShips).Count; j++)
		{
			((List<ShipItemVM>)(object)_allShips).Add(new ShipItemVM(((List<Ship>)(object)_portScreenHandler.RightShips)[j]));
		}
		for (int k = 0; k < ((List<ShipItemVM>)(object)_allShips).Count; k++)
		{
			((List<ShipItemVM>)(object)_allShips)[k].RefreshProperties(_portScreenHandler);
		}
		_cachedChanges = new List<PortChangeInfo>();
		CanConfirmHint = new HintViewModel();
		GoldCostHint = new BasicTooltipViewModel((Func<List<TooltipProperty>>)(() => GetGoldCostTooltip()));
		LeftRoster = new ShipRosterVM(OnLeftRosterSelected);
		RightRoster = new ShipRosterVM(OnRightRosterSelected);
		BuyAction = new PortActionVM(ExecuteBuy);
		SellAction = new PortActionVM(ExecuteSell);
		RepairAction = new PortActionVM(ExecuteRepair);
		RepairAllAction = new PortActionVM(ExecuteRepairAll);
		SendToClanAction = new PortActionVM(ExecuteSendToClan);
		GamepadCameraControlKeys = new MBBindingList<InputKeyItemVM>();
		KeyboardMoveCameraInputKeys = new MBBindingList<InputKeyItemVM>();
		RefreshRosters();
		RefreshActionAvailabilities();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		CancelText = ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString();
		ConfirmText = ((object)new TextObject("{=5Unqsx3N}Confirm", (Dictionary<string, object>)null)).ToString();
		((ViewModel)LeftRoster).RefreshValues();
		((ViewModel)RightRoster).RefreshValues();
		KeyboardMoveCameraText = ((object)GameTexts.FindText("str_key_name", typeof(PortHotKeyCategory).Name + "_MovementAxisX")).ToString();
		InputKeyItemVM doneInputKey = DoneInputKey;
		if (doneInputKey != null)
		{
			((ViewModel)doneInputKey).RefreshValues();
		}
		InputKeyItemVM resetInputKey = ResetInputKey;
		if (resetInputKey != null)
		{
			((ViewModel)resetInputKey).RefreshValues();
		}
		InputKeyItemVM cancelInputKey = CancelInputKey;
		if (cancelInputKey != null)
		{
			((ViewModel)cancelInputKey).RefreshValues();
		}
		foreach (InputKeyItemVM item in (Collection<InputKeyItemVM>)(object)GamepadCameraControlKeys)
		{
			((ViewModel)item).RefreshValues();
		}
		foreach (InputKeyItemVM item2 in (Collection<InputKeyItemVM>)(object)KeyboardMoveCameraInputKeys)
		{
			((ViewModel)item2).RefreshValues();
		}
		InputKeyItemVM keyboardRotateCameraInputKey = KeyboardRotateCameraInputKey;
		if (keyboardRotateCameraInputKey != null)
		{
			((ViewModel)keyboardRotateCameraInputKey).RefreshValues();
		}
		UpdateTotalGoldCost();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		((ViewModel)LeftRoster).OnFinalize();
		((ViewModel)RightRoster).OnFinalize();
		ShipItemVM.OnSelected -= OnShipSelected;
		ShipItemVM.OnRenamed -= OnShipRenamed;
		ShipItemVM.OnNameReset -= OnShipNameReset;
		ShipUpgradePieceBaseVM.OnInspected -= OnShipPieceInspected;
		ShipUpgradeSlotBaseVM.OnShipPieceSelected -= OnShipPieceSelected;
		ShipUpgradeContainerVM.OnSlotSelected = (ShipUpgradeContainerVM.ShipSlotSelectedDelegate)Delegate.Remove(ShipUpgradeContainerVM.OnSlotSelected, new ShipUpgradeContainerVM.ShipSlotSelectedDelegate(OnUpgradeSlotSelected));
		ShipFigureheadSlotVM.GetCurrentFigurehead -= GetCurrentFigurehead;
		ShipFigureheadSlotVM.GetShipOfFigurehead -= GetShipOfFigurehead;
		ShipFigureheadSlotVM.GetIsRightSide -= GetIsShipRightSide;
		ShipUpgradePieceVM.GetUpgradePrice -= GetUpgradePrice;
		InputKeyItemVM doneInputKey = DoneInputKey;
		if (doneInputKey != null)
		{
			((ViewModel)doneInputKey).OnFinalize();
		}
		InputKeyItemVM cancelInputKey = CancelInputKey;
		if (cancelInputKey != null)
		{
			((ViewModel)cancelInputKey).OnFinalize();
		}
		InputKeyItemVM resetInputKey = ResetInputKey;
		if (resetInputKey != null)
		{
			((ViewModel)resetInputKey).OnFinalize();
		}
		foreach (InputKeyItemVM item in (Collection<InputKeyItemVM>)(object)GamepadCameraControlKeys)
		{
			((ViewModel)item).OnFinalize();
		}
		foreach (InputKeyItemVM item2 in (Collection<InputKeyItemVM>)(object)KeyboardMoveCameraInputKeys)
		{
			((ViewModel)item2).OnFinalize();
		}
		InputKeyItemVM keyboardRotateCameraInputKey = KeyboardRotateCameraInputKey;
		if (keyboardRotateCameraInputKey != null)
		{
			((ViewModel)keyboardRotateCameraInputKey).OnFinalize();
		}
	}

	public void OnTick(float dt)
	{
		for (int i = 0; i < ((List<ShipItemVM>)(object)_allShips).Count; i++)
		{
			((List<ShipItemVM>)(object)_allShips)[i].Upgrades.Update();
		}
	}

	public void UpdateGamepadCameraControlButtonsVisibility()
	{
		bool? forcedVisibility = null;
		bool? forcedVisibility2 = null;
		if (!IsControllingCamera)
		{
			forcedVisibility = false;
		}
		if (!CanToggleCamera)
		{
			forcedVisibility2 = false;
		}
		for (int i = 0; i < ((Collection<InputKeyItemVM>)(object)GamepadCameraControlKeys).Count; i++)
		{
			InputKeyItemVM val = ((Collection<InputKeyItemVM>)(object)GamepadCameraControlKeys)[i];
			if (val != GamepadToggleCameraInputKey)
			{
				val.SetForcedVisibility(forcedVisibility);
			}
			else
			{
				val.SetForcedVisibility(forcedVisibility2);
			}
		}
	}

	private void UpdateTotalGoldCost()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		TotalGoldCost = _portScreenHandler.GetTotalGoldCost();
		_cachedChanges = _portScreenHandler.GetChanges();
		if (TotalGoldCost > 0 || (TotalGoldCost == 0 && _cachedChanges.Count > 0))
		{
			TotalGoldCostText = ((object)new TextObject("{=jM8XqvAD}You will pay {GOLD}{GOLD_ICON}", (Dictionary<string, object>)null).SetTextVariable("GOLD", TotalGoldCost).SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">")).ToString();
		}
		else if (TotalGoldCost < 0)
		{
			TotalGoldCostText = ((object)new TextObject("{=6ELEOERd}You will receive {GOLD}{GOLD_ICON}", (Dictionary<string, object>)null).SetTextVariable("GOLD", -TotalGoldCost).SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">")).ToString();
		}
		else
		{
			TotalGoldCostText = string.Empty;
		}
		UpdateCanConfirm();
	}

	private void UpdateCanConfirm()
	{
		if (_portScreenHandler.GetCanConfirm(out var disabledHint))
		{
			IsConfirmDisabled = false;
			return;
		}
		IsConfirmDisabled = true;
		CanConfirmHint.HintText = disabledHint;
	}

	private List<TooltipProperty> GetGoldCostTooltip()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>();
		if (TotalGoldCost >= 0)
		{
			foreach (PortChangeInfo cachedChange in _cachedChanges)
			{
				list.Add(new TooltipProperty(cachedChange.Description, ((int)cachedChange.GoldCost).ToString("+#;-#;0"), 0, false, (TooltipPropertyFlags)0));
			}
		}
		else if (TotalGoldCost < 0)
		{
			foreach (PortChangeInfo cachedChange2 in _cachedChanges)
			{
				list.Add(new TooltipProperty(cachedChange2.Description, (-(int)cachedChange2.GoldCost).ToString("+#;-#;0"), 0, false, (TooltipPropertyFlags)0));
			}
		}
		return list;
	}

	public bool AreThereAnyChanges()
	{
		return _portScreenHandler.AreThereAnyChanges();
	}

	public void SelectFirstAvailableRosterAndShip()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		ShipRosterVM shipRosterVM;
		ShipRosterVM shipRosterVM2;
		if ((int)_portScreenMode == 3)
		{
			shipRosterVM = LeftRoster;
			shipRosterVM2 = RightRoster;
		}
		else
		{
			shipRosterVM = RightRoster;
			shipRosterVM2 = LeftRoster;
		}
		if (shipRosterVM.HasAnyShips)
		{
			shipRosterVM.ExecuteSelectRoster();
			((Collection<ShipItemVM>)(object)shipRosterVM.Ships)[0].ExecuteSelect();
		}
		else if (shipRosterVM2.HasAnyShips)
		{
			shipRosterVM2.ExecuteSelectRoster();
			((Collection<ShipItemVM>)(object)shipRosterVM2.Ships)[0].ExecuteSelect();
		}
		else
		{
			Debug.FailedAssert("There are no ships on either roster!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\Port\\PortVM.cs", "SelectFirstAvailableRosterAndShip", 280);
		}
	}

	private void SelectClosestShipFromActiveRoster(int previousSelectedIndex)
	{
		ShipRosterVM selectedRoster = GetSelectedRoster();
		if (!selectedRoster.HasAnyShips || previousSelectedIndex < 0)
		{
			SelectFirstAvailableRosterAndShip();
			return;
		}
		int index = MathF.Min(((Collection<ShipItemVM>)(object)selectedRoster.Ships).Count - 1, previousSelectedIndex);
		((Collection<ShipItemVM>)(object)selectedRoster.Ships)[index].ExecuteSelect();
	}

	private ShipRosterVM GetSelectedRoster()
	{
		if (!LeftRoster.IsSelected)
		{
			return RightRoster;
		}
		return LeftRoster;
	}

	public void ExecuteCancelWithoutInquiry()
	{
		ExecuteCancel();
	}

	public void ExecuteCancel(bool showCancelInquiry = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		if ((int)_portScreenMode == 3)
		{
			if (AreThereAnyChanges())
			{
				InformationManager.ShowInquiry(new InquiryData("", ((object)GameTexts.FindText("str_cancelling_changes", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)ExecuteCancelInternal, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
			else if (LeftRoster.HasAnyShips)
			{
				InformationManager.ShowInquiry(new InquiryData("", ((object)GameTexts.FindText("str_leaving_ships_behind", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)ExecuteCancelInternal, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
			else
			{
				ExecuteCancelInternal();
			}
		}
		else if (showCancelInquiry && AreThereAnyChanges())
		{
			InformationManager.ShowInquiry(new InquiryData("", ((object)GameTexts.FindText("str_cancelling_changes", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)ExecuteCancelInternal, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
		else
		{
			ExecuteCancelInternal();
		}
	}

	private void ExecuteCancelInternal()
	{
		GameStateManager.Current.PopState(0);
	}

	public void ExecuteConfirm()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		if (!IsConfirmDisabled)
		{
			if ((int)_portScreenMode == 3 && LeftRoster.HasAnyShips)
			{
				InformationManager.ShowInquiry(new InquiryData("", ((object)GameTexts.FindText("str_leaving_ships_behind", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)ExecuteConfirmInternal, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
			else
			{
				ExecuteConfirmInternal();
			}
		}
	}

	private void ExecuteConfirmInternal()
	{
		_portScreenHandler.OnConfirmChanges();
		GameStateManager.Current.PopState(0);
	}

	public void ExecuteReset()
	{
		int previousSelectedIndex = ((Collection<ShipItemVM>)(object)GetSelectedRoster().Ships).IndexOf(SelectedShip);
		_portScreenHandler.ResetChanges();
		for (int i = 0; i < ((List<ShipItemVM>)(object)_allShips).Count; i++)
		{
			((List<ShipItemVM>)(object)_allShips)[i].Upgrades.ResetUpgradePieces();
		}
		RefreshRosters();
		SelectClosestShipFromActiveRoster(previousSelectedIndex);
		UpdateTotalGoldCost();
	}

	public void ExecuteRepair()
	{
		_portScreenHandler.OnRepairShip(SelectedShip.Ship);
		SelectedShip.CurrentHp = SelectedShip.MaxHp;
		SelectedShip.IsRepaired = true;
		UpdateTotalGoldCost();
		RefreshRosters();
	}

	public void ExecuteRepairAll()
	{
		foreach (ShipItemVM item in (Collection<ShipItemVM>)(object)RightRoster.Ships)
		{
			Ship ship = item.Ship;
			PortActionInfo canRepairShip = _portScreenHandler.GetCanRepairShip(ship);
			if (canRepairShip.IsRelevant && canRepairShip.IsEnabled)
			{
				_portScreenHandler.OnRepairShip(ship);
				item.CurrentHp = item.MaxHp;
				item.IsRepaired = true;
			}
		}
		UpdateTotalGoldCost();
		RefreshRosters();
	}

	public void ExecuteSendToClan()
	{
		int previousSelectedIndex = ((Collection<ShipItemVM>)(object)GetSelectedRoster().Ships).IndexOf(SelectedShip);
		_portScreenHandler.OnSendToClan(SelectedShip.Ship);
		UpdateTotalGoldCost();
		RefreshRosters();
		SelectClosestShipFromActiveRoster(previousSelectedIndex);
	}

	public void ExecuteBuy()
	{
		int previousSelectedIndex = ((Collection<ShipItemVM>)(object)GetSelectedRoster().Ships).IndexOf(SelectedShip);
		_portScreenHandler.OnBuyShip(SelectedShip.Ship);
		UpdateTotalGoldCost();
		RefreshRosters();
		SelectClosestShipFromActiveRoster(previousSelectedIndex);
	}

	public void ExecuteSell()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		bool flag = false;
		for (int i = 0; i < ((List<PortScreenHandler.ShipUpgradePieceInfo>)(object)_portScreenHandler.SelectedShipPieces).Count; i++)
		{
			if (((List<PortScreenHandler.ShipUpgradePieceInfo>)(object)_portScreenHandler.SelectedShipPieces)[i].Ship == SelectedShip.Ship)
			{
				flag = true;
			}
		}
		for (int j = 0; j < ((List<PortScreenHandler.ShipFigureheadInfo>)(object)_portScreenHandler.SelectedFigureheads).Count; j++)
		{
			if (((List<PortScreenHandler.ShipFigureheadInfo>)(object)_portScreenHandler.SelectedFigureheads)[j].Ship == SelectedShip.Ship)
			{
				flag = true;
			}
		}
		if (SelectedShip.IsRepaired || SelectedShip.IsRenamed || flag)
		{
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=2H95Y2vK}Sell Ship?", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=baQh2cwb}Selling this ship will revert your previous changes to it. Are you sure?", (Dictionary<string, object>)null)).ToString(), true, true, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), ((object)GameTexts.FindText("str_cancel", (string)null)).ToString(), (Action)ExecuteSellAux, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
		else
		{
			ExecuteSellAux();
		}
	}

	private void ExecuteSellAux()
	{
		int previousSelectedIndex = ((Collection<ShipItemVM>)(object)GetSelectedRoster().Ships).IndexOf(SelectedShip);
		_portScreenHandler.OnSellShip(SelectedShip.Ship);
		SelectedShip.Upgrades.ResetUpgradePieces();
		UpdateTotalGoldCost();
		RefreshRosters();
		SelectClosestShipFromActiveRoster(previousSelectedIndex);
	}

	public void ExecuteDeselectSlot()
	{
		SelectedUpgradeSlot?.ExecuteDeselect();
	}

	public bool ExecuteSelectPreviousShip()
	{
		ShipRosterVM selectedRoster = GetSelectedRoster();
		if (!selectedRoster.HasAnyShips)
		{
			return false;
		}
		int num = ((Collection<ShipItemVM>)(object)selectedRoster.Ships).IndexOf(SelectedShip);
		if (num == -1)
		{
			Debug.FailedAssert("Selected ship not found in selected roster!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\Port\\PortVM.cs", "ExecuteSelectPreviousShip", 552);
			((Collection<ShipItemVM>)(object)selectedRoster.Ships)[0].ExecuteSelect();
		}
		else
		{
			int num2 = num - 1;
			if (num2 < 0)
			{
				num2 = ((Collection<ShipItemVM>)(object)selectedRoster.Ships).Count - 1;
			}
			((Collection<ShipItemVM>)(object)selectedRoster.Ships)[num2].ExecuteSelect();
		}
		return true;
	}

	public bool ExecuteSelectNextShip()
	{
		ShipRosterVM selectedRoster = GetSelectedRoster();
		if (!selectedRoster.HasAnyShips)
		{
			return false;
		}
		int num = ((Collection<ShipItemVM>)(object)selectedRoster.Ships).IndexOf(SelectedShip);
		if (num == -1)
		{
			Debug.FailedAssert("Selected ship not found in selected roster!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\Port\\PortVM.cs", "ExecuteSelectNextShip", 581);
			((Collection<ShipItemVM>)(object)selectedRoster.Ships)[0].ExecuteSelect();
		}
		else
		{
			int num2 = num + 1;
			if (num2 >= ((Collection<ShipItemVM>)(object)selectedRoster.Ships).Count)
			{
				num2 = 0;
			}
			((Collection<ShipItemVM>)(object)selectedRoster.Ships)[num2].ExecuteSelect();
		}
		return true;
	}

	private void OnLeftRosterSelected()
	{
		if (!LeftRoster.IsSelected)
		{
			LeftRoster.IsSelected = true;
			RightRoster.IsSelected = false;
			if (LeftRoster.HasAnyShips)
			{
				((Collection<ShipItemVM>)(object)LeftRoster.Ships)[0].ExecuteSelect();
			}
		}
	}

	private void OnRightRosterSelected()
	{
		if (!RightRoster.IsSelected)
		{
			LeftRoster.IsSelected = false;
			RightRoster.IsSelected = true;
			if (RightRoster.HasAnyShips)
			{
				((Collection<ShipItemVM>)(object)RightRoster.Ships)[0].ExecuteSelect();
			}
		}
	}

	private void OnShipPieceInspected(ShipUpgradePieceBaseVM piece)
	{
		if (InspectedUpgrade != null && InspectedUpgrade != piece)
		{
			InspectedUpgrade.IsInspected = false;
		}
		if (piece != null)
		{
			InspectedUpgrade = piece;
			InspectedUpgrade.IsInspected = true;
		}
	}

	public void OnShipPieceSelected(Ship ship, string shipSlotTag, string slotTypeId, ShipUpgradePieceBaseVM pieceVM)
	{
		if (ship == null || string.IsNullOrEmpty(shipSlotTag))
		{
			Debug.FailedAssert("Ship piece selected in an invalid state!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\Port\\PortVM.cs", "OnShipPieceSelected", 647);
		}
		else if (pieceVM == null || !pieceVM.IsDisabled)
		{
			if (string.Equals(slotTypeId, "figurehead", StringComparison.InvariantCultureIgnoreCase))
			{
				_portScreenHandler.OnFigureheadSelected(ship, (pieceVM as ShipFigureheadVM)?.Figurehead);
				UpdateAvailableFigureheads();
			}
			else
			{
				_portScreenHandler.OnUpgradePieceSelected(ship, shipSlotTag, (pieceVM as ShipUpgradePieceVM)?.Piece);
			}
			RefreshSelectedShipProperties();
			UpdateTotalGoldCost();
			_refreshShipVisual?.Invoke(((IEnumerable<ShipItemVM>)AllShips).FirstOrDefault((ShipItemVM x) => x.Ship == ship));
		}
	}

	public void UpdateAvailableFigureheads()
	{
		for (int i = 0; i < ((List<ShipItemVM>)(object)_allShips).Count; i++)
		{
			GetFigureheadSlot(((List<ShipItemVM>)(object)_allShips)[i])?.UpdateAvailableFigureheads();
		}
	}

	public Figurehead GetCurrentFigurehead(Ship ship)
	{
		foreach (PortScreenHandler.ShipFigureheadInfo item in (List<PortScreenHandler.ShipFigureheadInfo>)(object)_portScreenHandler.SelectedFigureheads)
		{
			if (item.Ship == ship)
			{
				return item.Figurehead;
			}
		}
		return ship.Figurehead;
	}

	public Ship GetShipOfFigurehead(Figurehead figurehead, bool isRightSide)
	{
		MBReadOnlyList<Ship> val = (isRightSide ? _portScreenHandler.RightShips : _portScreenHandler.LeftShips);
		for (int i = 0; i < ((List<Ship>)(object)val).Count; i++)
		{
			Ship val2 = ((List<Ship>)(object)val)[i];
			if (GetCurrentFigurehead(val2) == figurehead)
			{
				return val2;
			}
		}
		return null;
	}

	private ShipFigureheadSlotVM GetFigureheadSlot(ShipItemVM ship)
	{
		return ((IEnumerable<ShipUpgradeSlotBaseVM>)ship.Upgrades.UpgradeSlots).FirstOrDefault((ShipUpgradeSlotBaseVM x) => x is ShipFigureheadSlotVM) as ShipFigureheadSlotVM;
	}

	private bool GetIsShipRightSide(Ship ship)
	{
		return ((List<Ship>)(object)_portScreenHandler.RightShips).Contains(ship);
	}

	public void OnUpgradeSlotSelected(ShipUpgradeSlotBaseVM slot)
	{
		SelectedUpgradeSlot = slot;
		if (SelectedUpgradeSlot == null)
		{
			InformationManager.HideTooltip();
			ShipUpgradePieceBaseVM inspectedUpgrade = InspectedUpgrade;
			if (inspectedUpgrade == null || !inspectedUpgrade.IsInspectedFromSlot)
			{
				OnShipPieceInspected(null);
			}
		}
		_onUpgradeSlotSelected?.Invoke();
	}

	public int GetUpgradePrice(Ship ship, ShipUpgradePiece piece)
	{
		return _portScreenHandler.GetUpgradeCostOfShip(ship, piece, isRightSideUpgrading: true);
	}

	private void OnShipRenamed(ShipItemVM ship, string newName)
	{
		_portScreenHandler.OnRenameShip(ship.Ship, newName);
		ship.RefreshProperties(_portScreenHandler);
		UpdateTotalGoldCost();
	}

	private void OnShipNameReset(ShipItemVM ship)
	{
		_portScreenHandler.OnResetShipName(ship.Ship);
		ship.RefreshProperties(_portScreenHandler);
		UpdateTotalGoldCost();
	}

	private void OnShipSelected(ShipItemVM ship)
	{
		if (SelectedShip != ship)
		{
			SelectedShip?.Upgrades?.SelectedSlot?.ExecuteDeselect();
			InformationManager.HideTooltip();
			OnShipPieceInspected(null);
			SelectedShip = ship;
			RefreshSelectedShipProperties();
			_onShipSelected?.Invoke(SelectedShip?.Ship);
		}
	}

	private void RefreshSelectedShipProperties()
	{
		if (SelectedShip == null)
		{
			return;
		}
		SelectedShip.RefreshProperties(_portScreenHandler);
		MBList<(string, ShipUpgradePiece)> val = new MBList<(string, ShipUpgradePiece)>();
		for (int i = 0; i < ((Collection<ShipUpgradeSlotBaseVM>)(object)SelectedShip.Upgrades.UpgradeSlots).Count; i++)
		{
			ShipUpgradeSlotBaseVM shipUpgradeSlotBaseVM = ((Collection<ShipUpgradeSlotBaseVM>)(object)SelectedShip.Upgrades.UpgradeSlots)[i];
			if (shipUpgradeSlotBaseVM.IsChanged && shipUpgradeSlotBaseVM is ShipUpgradeSlotVM)
			{
				((List<(string, ShipUpgradePiece)>)(object)val).Add((shipUpgradeSlotBaseVM.ShipSlotTag, (shipUpgradeSlotBaseVM.SelectedPiece as ShipUpgradePieceVM)?.Piece));
			}
		}
		SelectedShip.Stats.RefreshStats(SelectedShip.CurrentHp, (MBReadOnlyList<(string, ShipUpgradePiece)>)(object)val);
		RefreshActionAvailabilities();
	}

	private void RefreshRosters()
	{
		LeftRoster.SetRosterName(_portScreenHandler.GetLeftRosterName());
		RightRoster.SetRosterName(_portScreenHandler.GetRightRosterName());
		LeftRoster.SetRosterOwner(_portScreenHandler.GetLeftSideOwnerParty());
		RightRoster.SetRosterOwner(_portScreenHandler.GetRightSideOwnerParty());
		GetRosterDifferences((MBReadOnlyList<ShipItemVM>)(object)_allShips, _portScreenHandler.LeftShips, LeftRoster.Ships, out var removedShips, out var addedShips);
		GetRosterDifferences((MBReadOnlyList<ShipItemVM>)(object)_allShips, _portScreenHandler.RightShips, RightRoster.Ships, out var removedShips2, out var addedShips2);
		LeftRoster.RefreshShips(removedShips, addedShips, _portScreenHandler.LeftShips);
		RightRoster.RefreshShips(removedShips2, addedShips2, _portScreenHandler.RightShips);
		for (int i = 0; i < ((List<ShipItemVM>)(object)_allShips).Count; i++)
		{
			((List<ShipItemVM>)(object)_allShips)[i].RefreshProperties(_portScreenHandler);
		}
		RefreshSelectedShipProperties();
		UpdateAvailableFigureheads();
		_onRostersRefreshed?.Invoke();
	}

	private static void GetRosterDifferences(MBReadOnlyList<ShipItemVM> allShips, MBReadOnlyList<Ship> currentShips, MBBindingList<ShipItemVM> dataSourceShips, out MBReadOnlyList<ShipItemVM> removedShips, out MBReadOnlyList<ShipItemVM> addedShips)
	{
		MBList<ShipItemVM> val = new MBList<ShipItemVM>();
		MBList<ShipItemVM> val2 = new MBList<ShipItemVM>();
		for (int i = 0; i < ((Collection<ShipItemVM>)(object)dataSourceShips).Count; i++)
		{
			ShipItemVM shipItemVM = ((Collection<ShipItemVM>)(object)dataSourceShips)[i];
			Ship ship = shipItemVM.Ship;
			if (!((List<Ship>)(object)currentShips).Contains(ship))
			{
				((List<ShipItemVM>)(object)val).Add(shipItemVM);
			}
		}
		for (int j = 0; j < ((List<Ship>)(object)currentShips).Count; j++)
		{
			Ship val3 = ((List<Ship>)(object)currentShips)[j];
			bool flag = false;
			for (int k = 0; k < ((Collection<ShipItemVM>)(object)dataSourceShips).Count; k++)
			{
				if (((Collection<ShipItemVM>)(object)dataSourceShips)[k].Ship == val3)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			ShipItemVM shipItemVM2 = null;
			for (int l = 0; l < ((List<ShipItemVM>)(object)allShips).Count; l++)
			{
				if (((List<ShipItemVM>)(object)allShips)[l].Ship == val3)
				{
					shipItemVM2 = ((List<ShipItemVM>)(object)allShips)[l];
					break;
				}
			}
			if (shipItemVM2 == null)
			{
				Debug.FailedAssert($"Unable to find vm for ship: {val3}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\Port\\PortVM.cs", "GetRosterDifferences", 870);
			}
			else
			{
				((List<ShipItemVM>)(object)val2).Add(shipItemVM2);
			}
		}
		removedShips = (MBReadOnlyList<ShipItemVM>)(object)val;
		addedShips = (MBReadOnlyList<ShipItemVM>)(object)val2;
	}

	private void RefreshActionAvailabilities()
	{
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Expected O, but got Unknown
		if (SelectedShip != null)
		{
			PortActionInfo canBuyShip = _portScreenHandler.GetCanBuyShip(SelectedShip.Ship);
			BuyAction.RefreshWith(canBuyShip);
			BuyAction.AdditionalInfo = GetGoldCostText(canBuyShip.GoldCost);
			PortActionInfo canSellShip = _portScreenHandler.GetCanSellShip(SelectedShip.Ship);
			SellAction.RefreshWith(canSellShip);
			SellAction.AdditionalInfo = GetGoldCostText(canSellShip.GoldCost);
			PortActionInfo canRepairShip = _portScreenHandler.GetCanRepairShip(SelectedShip.Ship);
			RepairAction.RefreshWith(canRepairShip);
			RepairAction.AdditionalInfo = GetGoldCostText(canRepairShip.GoldCost);
			PortActionInfo canRepairAll = _portScreenHandler.GetCanRepairAll(SelectedShip.Ship);
			RepairAllAction.RefreshWith(canRepairAll);
			RepairAllAction.AdditionalInfo = GetGoldCostText(canRepairAll.GoldCost);
			PortActionInfo actionInfo = _portScreenHandler.GetCanUpgradeShip(SelectedShip.Ship);
			SelectedShip.Upgrades.UpdateEnabledStatus(in actionInfo);
			UpgradeText = ((object)actionInfo.ActionName)?.ToString();
			PortActionInfo canRenameShip = _portScreenHandler.GetCanRenameShip(SelectedShip.Ship);
			SelectedShip.PlayerCanChangeShipName = canRenameShip.IsRelevant && canRenameShip.IsEnabled;
			SelectedShip.ChangeShipNameHint = new HintViewModel(canRenameShip.Tooltip, (string)null);
			PortActionInfo canSendToClan = _portScreenHandler.GetCanSendToClan(SelectedShip.Ship);
			SendToClanAction.RefreshWith(canSendToClan);
			SendToClanAction.AdditionalInfo = string.Empty;
		}
	}

	private static string GetGoldCostText(int cost)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (cost == 0)
		{
			return string.Empty;
		}
		return ((object)new TextObject("{=ePmSvu1s}{AMOUNT}{GOLD_ICON}", (Dictionary<string, object>)null).SetTextVariable("AMOUNT", cost)).ToString();
	}

	public void SetResetInputKey(HotKey hotKey)
	{
		ResetInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetCancelInputKey(HotKey hotKey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetSelectPreviousShipInputKey(HotKey hotKey)
	{
		SelectPreviousShipInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetSelectNextShipInputKey(HotKey hotKey)
	{
		SelectNextShipInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetSelectLeftRosterInputKey(HotKey hotKey)
	{
		SelectLeftRosterInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetSelectRightRosterInputKey(HotKey hotKey)
	{
		SelectRightRosterInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetGamepadToggleCameraInputKey(HotKey hotKey)
	{
		InputKeyItemVM val = InputKeyItemVM.CreateFromHotKey(hotKey, true);
		((Collection<InputKeyItemVM>)(object)GamepadCameraControlKeys).Add(val);
		GamepadToggleCameraInputKey = val;
		UpdateGamepadCameraControlButtonsVisibility();
	}

	public void AddGamepadCameraControlInputKey(HotKey hotKey)
	{
		InputKeyItemVM item = InputKeyItemVM.CreateFromHotKey(hotKey, true);
		((Collection<InputKeyItemVM>)(object)GamepadCameraControlKeys).Add(item);
		UpdateGamepadCameraControlButtonsVisibility();
	}

	public void AddGamepadCameraControlInputKey(GameAxisKey gameAxisKey)
	{
		TextObject val = GameTexts.FindText("str_key_name", typeof(PortHotKeyCategory).Name + "_" + gameAxisKey.Id);
		InputKeyItemVM item = InputKeyItemVM.CreateFromForcedID(((object)gameAxisKey.AxisKey).ToString(), val, true);
		((Collection<InputKeyItemVM>)(object)GamepadCameraControlKeys).Add(item);
		UpdateGamepadCameraControlButtonsVisibility();
	}

	public void AddKeyboardMoveCameraInputKey(GameKey gameKey)
	{
		InputKeyItemVM item = InputKeyItemVM.CreateFromGameKey(gameKey, false);
		((Collection<InputKeyItemVM>)(object)KeyboardMoveCameraInputKeys).Add(item);
	}

	public void SetKeyboardRotateCameraInputKey(HotKey hotKey)
	{
		TextObject val = GameTexts.FindText("str_key_name", typeof(PortHotKeyCategory).Name + "_CameraAxisX");
		InputKeyItemVM keyboardRotateCameraInputKey = InputKeyItemVM.CreateFromForcedID(((object)hotKey).ToString(), val, false);
		KeyboardRotateCameraInputKey = keyboardRotateCameraInputKey;
	}
}
