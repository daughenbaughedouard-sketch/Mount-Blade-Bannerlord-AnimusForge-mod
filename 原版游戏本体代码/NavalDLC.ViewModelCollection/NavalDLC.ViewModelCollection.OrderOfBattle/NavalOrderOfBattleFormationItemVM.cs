using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;

namespace NavalDLC.ViewModelCollection.OrderOfBattle;

public class NavalOrderOfBattleFormationItemVM : ViewModel
{
	public readonly Formation Formation;

	private readonly Action<NavalOrderOfBattleFormationItemVM> _onSelected;

	private readonly Action<NavalOrderOfBattleFormationItemVM> _onClassChanged;

	private readonly Action<NavalOrderOfBattleFormationItemVM> _onFilterToggled;

	public static Action<NavalOrderOfBattleFormationItemVM> OnAcceptCaptain;

	public static Action<NavalOrderOfBattleFormationItemVM> OnAcceptShip;

	public static Func<DeploymentFormationClass, FormationFilterType, int> GetTotalTroopCountWithFilter;

	private readonly TextObject _captainSlotHintText = new TextObject("{=shipcaptain}Captain", (Dictionary<string, object>)null);

	private readonly TextObject _shipSlotHintText = new TextObject("{=1nbU1tV5}Ship", (Dictionary<string, object>)null);

	private readonly TextObject _assignCaptainHintText = new TextObject("{=rHEi6aVz}Assign as Captain", (Dictionary<string, object>)null);

	private readonly TextObject _assignShipHintText = new TextObject("{=6o2JKNbt}Assign as Ship", (Dictionary<string, object>)null);

	private readonly TextObject _infantryHintText = new TextObject("{=IxI1HecC}Give preference to infantry troops", (Dictionary<string, object>)null);

	private readonly TextObject _rangedHintText = new TextObject("{=I9X4VvhG}Give preference to ranged troops", (Dictionary<string, object>)null);

	private readonly TextObject _infantryAndRangedHintText = new TextObject("{=e9nO59x4}Give equal preference to infantry and ranged troops", (Dictionary<string, object>)null);

	private readonly TextObject _filteredTroopCountInfoText = new TextObject("{=yRIPADWl}{TROOP_COUNT}/{TOTAL_TROOP_COUNT}", (Dictionary<string, object>)null);

	private bool _isSelected;

	private bool _isEnabled;

	private bool _isSelectable;

	private bool _hasCaptain;

	private bool _hasShip;

	private bool _isAcceptingCaptain;

	private bool _isAcceptingShip;

	private bool _isInfantrySelected;

	private bool _isRangedSelected;

	private bool _isInfantryAndRangedSelected;

	private string _formationName;

	private string _formationIsEmptyText;

	private int _troopCount;

	private int _formationClassInt;

	private bool _isSkeletalCrewCountWarningActive;

	private string _skeletalCrewCountWarning;

	private HintViewModel _captainSlotHint;

	private HintViewModel _shipSlotHint;

	private HintViewModel _assignCaptainHint;

	private HintViewModel _assignShipHint;

	private NavalOrderOfBattleHeroItemVM _captain;

	private NavalOrderOfBattleShipItemVM _ship;

	private int _wSign;

	private Vec2 _screenPosition;

	private MBBindingList<OrderOfBattleFormationFilterSelectorItemVM> _filterItems;

	private HintViewModel _infantryHint;

	private HintViewModel _rangedHint;

	private HintViewModel _infantryAndRangedHint;

	private HintViewModel _disabledHint;

	private BasicTooltipViewModel _tooltip;

	public DeploymentFormationClass SelectedClass { get; private set; }

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEnabled");
				IsSelectable = HasShip && IsEnabled;
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelectable
	{
		get
		{
			return _isSelectable;
		}
		set
		{
			if (value != _isSelectable)
			{
				_isSelectable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelectable");
				FilterItems.ApplyActionOnAllItems((Action<OrderOfBattleFormationFilterSelectorItemVM>)delegate(OrderOfBattleFormationFilterSelectorItemVM x)
				{
					x.IsEnabled = IsSelectable;
				});
			}
		}
	}

	[DataSourceProperty]
	public bool HasCaptain
	{
		get
		{
			return _hasCaptain;
		}
		set
		{
			if (value != _hasCaptain)
			{
				_hasCaptain = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasCaptain");
			}
		}
	}

	[DataSourceProperty]
	public bool HasShip
	{
		get
		{
			return _hasShip;
		}
		set
		{
			if (value != _hasShip)
			{
				_hasShip = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasShip");
				IsSelectable = HasShip && IsEnabled;
				IsSkeletalCrewCountWarningActive = HasShip && TroopCount < Ship.ShipOrigin.SkeletalCrewCapacity;
			}
		}
	}

	[DataSourceProperty]
	public bool IsAcceptingCaptain
	{
		get
		{
			return _isAcceptingCaptain;
		}
		set
		{
			if (value != _isAcceptingCaptain)
			{
				_isAcceptingCaptain = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAcceptingCaptain");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAcceptingShip
	{
		get
		{
			return _isAcceptingShip;
		}
		set
		{
			if (value != _isAcceptingShip)
			{
				_isAcceptingShip = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAcceptingShip");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInfantrySelected
	{
		get
		{
			return _isInfantrySelected;
		}
		set
		{
			if (value != _isInfantrySelected)
			{
				_isInfantrySelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInfantrySelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRangedSelected
	{
		get
		{
			return _isRangedSelected;
		}
		set
		{
			if (value != _isRangedSelected)
			{
				_isRangedSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRangedSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInfantryAndRangedSelected
	{
		get
		{
			return _isInfantryAndRangedSelected;
		}
		set
		{
			if (value != _isInfantryAndRangedSelected)
			{
				_isInfantryAndRangedSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInfantryAndRangedSelected");
			}
		}
	}

	[DataSourceProperty]
	public string FormationName
	{
		get
		{
			return _formationName;
		}
		set
		{
			if (value != _formationName)
			{
				_formationName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FormationName");
			}
		}
	}

	[DataSourceProperty]
	public string FormationIsEmptyText
	{
		get
		{
			return _formationIsEmptyText;
		}
		set
		{
			if (value != _formationIsEmptyText)
			{
				_formationIsEmptyText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FormationIsEmptyText");
			}
		}
	}

	[DataSourceProperty]
	public int TroopCount
	{
		get
		{
			return _troopCount;
		}
		set
		{
			if (value != _troopCount)
			{
				_troopCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TroopCount");
				IsSkeletalCrewCountWarningActive = HasShip && TroopCount < Ship.ShipOrigin.SkeletalCrewCapacity;
			}
		}
	}

	[DataSourceProperty]
	public int FormationClassInt
	{
		get
		{
			return _formationClassInt;
		}
		set
		{
			if (value != _formationClassInt)
			{
				_formationClassInt = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "FormationClassInt");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSkeletalCrewCountWarningActive
	{
		get
		{
			return _isSkeletalCrewCountWarningActive;
		}
		set
		{
			if (value != _isSkeletalCrewCountWarningActive)
			{
				_isSkeletalCrewCountWarningActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSkeletalCrewCountWarningActive");
			}
		}
	}

	[DataSourceProperty]
	public string SkeletalCrewCountWarning
	{
		get
		{
			return _skeletalCrewCountWarning;
		}
		set
		{
			if (value != _skeletalCrewCountWarning)
			{
				_skeletalCrewCountWarning = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SkeletalCrewCountWarning");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CaptainSlotHint
	{
		get
		{
			return _captainSlotHint;
		}
		set
		{
			if (value != _captainSlotHint)
			{
				_captainSlotHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "CaptainSlotHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ShipSlotHint
	{
		get
		{
			return _shipSlotHint;
		}
		set
		{
			if (value != _shipSlotHint)
			{
				_shipSlotHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ShipSlotHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel AssignCaptainHint
	{
		get
		{
			return _assignCaptainHint;
		}
		set
		{
			if (value != _assignCaptainHint)
			{
				_assignCaptainHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "AssignCaptainHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel AssignShipHint
	{
		get
		{
			return _assignShipHint;
		}
		set
		{
			if (value != _assignShipHint)
			{
				_assignShipHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "AssignShipHint");
			}
		}
	}

	[DataSourceProperty]
	public NavalOrderOfBattleHeroItemVM Captain
	{
		get
		{
			return _captain;
		}
		set
		{
			if (value != _captain)
			{
				_captain = value;
				((ViewModel)this).OnPropertyChangedWithValue<NavalOrderOfBattleHeroItemVM>(value, "Captain");
				HasCaptain = Captain != null;
			}
		}
	}

	[DataSourceProperty]
	public NavalOrderOfBattleShipItemVM Ship
	{
		get
		{
			return _ship;
		}
		set
		{
			if (value == _ship)
			{
				return;
			}
			_ship = value;
			((ViewModel)this).OnPropertyChangedWithValue<NavalOrderOfBattleShipItemVM>(value, "Ship");
			HasShip = Ship != null;
			if (!HasShip)
			{
				foreach (OrderOfBattleFormationFilterSelectorItemVM item in (Collection<OrderOfBattleFormationFilterSelectorItemVM>)(object)FilterItems)
				{
					item.IsActive = false;
				}
			}
			IsSkeletalCrewCountWarningActive = HasShip && TroopCount < Ship.ShipOrigin.SkeletalCrewCapacity;
		}
	}

	[DataSourceProperty]
	public int WSign
	{
		get
		{
			return _wSign;
		}
		set
		{
			if (value != _wSign)
			{
				_wSign = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "WSign");
			}
		}
	}

	[DataSourceProperty]
	public Vec2 ScreenPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _screenPosition;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _screenPosition)
			{
				_screenPosition = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ScreenPosition");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<OrderOfBattleFormationFilterSelectorItemVM> FilterItems
	{
		get
		{
			return _filterItems;
		}
		set
		{
			if (value != _filterItems)
			{
				_filterItems = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<OrderOfBattleFormationFilterSelectorItemVM>>(value, "FilterItems");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel InfantryHint
	{
		get
		{
			return _infantryHint;
		}
		set
		{
			if (value != _infantryHint)
			{
				_infantryHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "InfantryHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel RangedHint
	{
		get
		{
			return _rangedHint;
		}
		set
		{
			if (value != _rangedHint)
			{
				_rangedHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "RangedHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel InfantryAndRangedHint
	{
		get
		{
			return _infantryAndRangedHint;
		}
		set
		{
			if (value != _infantryAndRangedHint)
			{
				_infantryAndRangedHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "InfantryAndRangedHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel DisabledHint
	{
		get
		{
			return _disabledHint;
		}
		set
		{
			if (value != _disabledHint)
			{
				_disabledHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "DisabledHint");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel Tooltip
	{
		get
		{
			return _tooltip;
		}
		set
		{
			if (value != _tooltip)
			{
				_tooltip = value;
				((ViewModel)this).OnPropertyChangedWithValue<BasicTooltipViewModel>(value, "Tooltip");
			}
		}
	}

	public NavalOrderOfBattleFormationItemVM(Formation formation, Action<NavalOrderOfBattleFormationItemVM> onSelected, Action<NavalOrderOfBattleFormationItemVM> onClassChanged, Action<NavalOrderOfBattleFormationItemVM> onFilterToggled)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Invalid comparison between Unknown and I4
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Invalid comparison between Unknown and I4
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		Formation = formation;
		_onSelected = onSelected;
		_onClassChanged = onClassChanged;
		_onFilterToggled = onFilterToggled;
		FilterItems = new MBBindingList<OrderOfBattleFormationFilterSelectorItemVM>();
		for (FormationFilterType val = (FormationFilterType)1; (int)val < 7; val = (FormationFilterType)(val + 1))
		{
			if ((int)val != 2)
			{
				((Collection<OrderOfBattleFormationFilterSelectorItemVM>)(object)FilterItems).Add(new OrderOfBattleFormationFilterSelectorItemVM(val, (Action<OrderOfBattleFormationFilterSelectorItemVM>)OnFilterToggled));
			}
		}
		FilterItems.ApplyActionOnAllItems((Action<OrderOfBattleFormationFilterSelectorItemVM>)delegate(OrderOfBattleFormationFilterSelectorItemVM x)
		{
			x.IsEnabled = IsSelectable;
		});
		Tooltip = new BasicTooltipViewModel((Func<List<TooltipProperty>>)(() => GetTooltip()));
		ExecuteSelectInfantryAndRanged();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		FormationName = (Formation.Index + 1).ToString();
		FormationIsEmptyText = ((object)new TextObject("{=P3IWytsr}Formation is currently empty", (Dictionary<string, object>)null)).ToString();
		CaptainSlotHint = new HintViewModel(_captainSlotHintText, (string)null);
		ShipSlotHint = new HintViewModel(_shipSlotHintText, (string)null);
		AssignCaptainHint = new HintViewModel(_assignCaptainHintText, (string)null);
		AssignShipHint = new HintViewModel(_assignShipHintText, (string)null);
		InfantryHint = new HintViewModel(_infantryHintText, (string)null);
		RangedHint = new HintViewModel(_rangedHintText, (string)null);
		InfantryAndRangedHint = new HintViewModel(_infantryAndRangedHintText, (string)null);
		TroopCount = Formation.CountOfUnits;
		SkeletalCrewCountWarning = ((object)new TextObject("{=JEwakKND}Ship is undercrewed!", (Dictionary<string, object>)null)).ToString();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		foreach (OrderOfBattleFormationFilterSelectorItemVM item in (Collection<OrderOfBattleFormationFilterSelectorItemVM>)(object)FilterItems)
		{
			((ViewModel)item).OnFinalize();
		}
		((Collection<OrderOfBattleFormationFilterSelectorItemVM>)(object)FilterItems).Clear();
	}

	public void ExecuteSelect()
	{
		_onSelected?.Invoke(this);
	}

	public void ExecuteAcceptShip()
	{
		if (GetCanAcceptShip())
		{
			OnAcceptShip?.Invoke(this);
		}
	}

	public void ExecuteAcceptCaptain()
	{
		if (GetCanAcceptCaptain())
		{
			OnAcceptCaptain?.Invoke(this);
		}
	}

	private void OnFilterToggled(OrderOfBattleFormationFilterSelectorItemVM filterItem)
	{
		if (IsSelectable)
		{
			_onFilterToggled?.Invoke(this);
		}
	}

	private bool HasAnyActiveFilter()
	{
		return ((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)FilterItems).Any((OrderOfBattleFormationFilterSelectorItemVM f) => f.IsActive);
	}

	public bool HasFilter(FormationFilterType filter)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)FilterItems).Any((OrderOfBattleFormationFilterSelectorItemVM f) => f.IsActive && f.FilterType == filter);
	}

	public void ExecuteSelectInfantry()
	{
		SelectedClass = (DeploymentFormationClass)1;
		OnClassSelectionUpdated();
	}

	public void ExecuteSelectRanged()
	{
		SelectedClass = (DeploymentFormationClass)2;
		OnClassSelectionUpdated();
	}

	public void ExecuteSelectInfantryAndRanged()
	{
		SelectedClass = (DeploymentFormationClass)5;
		OnClassSelectionUpdated();
	}

	private void OnClassSelectionUpdated()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected I4, but got Unknown
		IsInfantrySelected = (int)SelectedClass == 1;
		IsRangedSelected = (int)SelectedClass == 2;
		IsInfantryAndRangedSelected = (int)SelectedClass == 5;
		FormationClassInt = (int)SelectedClass;
		if (IsSelectable)
		{
			_onClassChanged?.Invoke(this);
		}
	}

	public bool GetCanAcceptShip()
	{
		if (!IsEnabled)
		{
			return Captain?.IsMainHero ?? false;
		}
		return true;
	}

	public bool GetCanAcceptCaptain()
	{
		if (IsEnabled && HasShip)
		{
			NavalOrderOfBattleHeroItemVM captain = Captain;
			if (captain == null)
			{
				return true;
			}
			return !captain.IsMainHero;
		}
		return false;
	}

	private List<TooltipProperty> GetTooltip()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Invalid comparison between Unknown and I4
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Invalid comparison between Unknown and I4
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Invalid comparison between Unknown and I4
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Invalid comparison between Unknown and I4
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Invalid comparison between Unknown and I4
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Expected O, but got Unknown
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Expected O, but got Unknown
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Expected O, but got Unknown
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Expected I4, but got Unknown
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Expected O, but got Unknown
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Expected O, but got Unknown
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Expected O, but got Unknown
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Expected O, but got Unknown
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Expected O, but got Unknown
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Expected O, but got Unknown
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Expected O, but got Unknown
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Expected O, but got Unknown
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>
		{
			new TooltipProperty(((object)new TextObject("{=cZNA5Z6l}Formation {NUMBER}", (Dictionary<string, object>)null).SetTextVariable("NUMBER", FormationName)).ToString(), string.Empty, 0, false, (TooltipPropertyFlags)4096)
		};
		if (!HasShip)
		{
			return list;
		}
		List<Agent> list2 = new List<Agent>();
		int[] array = new int[4];
		foreach (IFormationUnit item in (List<IFormationUnit>)(object)Formation.Arrangement.GetAllUnits())
		{
			Agent val;
			if ((val = (Agent)(object)((item is Agent) ? item : null)) != null)
			{
				if (val.IsHero)
				{
					list2.Add(val);
				}
				FormationClass actualTroopType = GetActualTroopType(val);
				if ((int)actualTroopType >= 0 && (int)actualTroopType < 4)
				{
					array[actualTroopType]++;
				}
			}
		}
		foreach (Agent item2 in (List<Agent>)(object)Formation.DetachedUnits)
		{
			if (item2.IsHero)
			{
				list2.Add(item2);
			}
			FormationClass actualTroopType2 = GetActualTroopType(item2);
			if ((int)actualTroopType2 >= 0 && (int)actualTroopType2 < 4)
			{
				array[actualTroopType2]++;
			}
		}
		bool flag = false;
		for (FormationClass val2 = (FormationClass)0; (int)val2 < 4; val2 = (FormationClass)(val2 + 1))
		{
			int num = array[val2];
			List<Agent> list3 = new List<Agent>();
			for (int i = 0; i < list2.Count; i++)
			{
				Agent val3 = list2[i];
				if (val2 == GetActualTroopType(val3))
				{
					list3.Add(val3);
				}
			}
			if (num > 0)
			{
				if (flag)
				{
					list.Add(new TooltipProperty(string.Empty, string.Empty, -1, false, (TooltipPropertyFlags)0));
				}
				else
				{
					flag = true;
				}
				list.Add(new TooltipProperty(((object)GameTexts.FindText("str_troop_group_name", ((int)val2).ToString())).ToString(), num.ToString(), 0, false, (TooltipPropertyFlags)0));
				if (list3.Count > 0)
				{
					list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)512));
				}
				for (int j = 0; j < list3.Count; j++)
				{
					list.Add(new TooltipProperty(list3[j].Name, " ", 0, false, (TooltipPropertyFlags)0));
				}
			}
		}
		if (HasAnyActiveFilter())
		{
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)1024));
		}
		if (HasFilter((FormationFilterType)1))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent agent) => agent.HasShieldCached)));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(SelectedClass, (FormationFilterType)1));
			list.Add(new TooltipProperty(((object)OrderOfBattleFormationExtensions.GetFilterName((FormationFilterType)1)).ToString(), ((object)_filteredTroopCountInfoText).ToString(), 0, false, (TooltipPropertyFlags)0));
		}
		if (HasFilter((FormationFilterType)3))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent agent) => agent.HasThrownCached)));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(SelectedClass, (FormationFilterType)3));
			list.Add(new TooltipProperty(((object)OrderOfBattleFormationExtensions.GetFilterName((FormationFilterType)3)).ToString(), ((object)_filteredTroopCountInfoText).ToString(), 0, false, (TooltipPropertyFlags)0));
		}
		if (HasFilter((FormationFilterType)4))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent agent) => MissionGameModels.Current.AgentStatCalculateModel.HasHeavyArmor(agent))));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(SelectedClass, (FormationFilterType)4));
			list.Add(new TooltipProperty(((object)OrderOfBattleFormationExtensions.GetFilterName((FormationFilterType)4)).ToString(), ((object)_filteredTroopCountInfoText).ToString(), 0, false, (TooltipPropertyFlags)0));
		}
		if (HasFilter((FormationFilterType)5))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent agent) => agent.Character.GetBattleTier() >= 4)));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(SelectedClass, (FormationFilterType)5));
			list.Add(new TooltipProperty(((object)OrderOfBattleFormationExtensions.GetFilterName((FormationFilterType)5)).ToString(), ((object)_filteredTroopCountInfoText).ToString(), 0, false, (TooltipPropertyFlags)0));
		}
		if (HasFilter((FormationFilterType)6))
		{
			GameTexts.SetVariable("TROOP_COUNT", Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent agent) => agent.Character.GetBattleTier() <= 3)));
			GameTexts.SetVariable("TOTAL_TROOP_COUNT", GetTotalTroopCountWithFilter(SelectedClass, (FormationFilterType)6));
			list.Add(new TooltipProperty(((object)OrderOfBattleFormationExtensions.GetFilterName((FormationFilterType)6)).ToString(), ((object)_filteredTroopCountInfoText).ToString(), 0, false, (TooltipPropertyFlags)0));
		}
		if (Ship?.MissionShip != null)
		{
			int reservedTroopsCountOfShip = Mission.Current.GetMissionBehavior<NavalAgentsLogic>().GetReservedTroopsCountOfShip(Ship.MissionShip);
			if (reservedTroopsCountOfShip > 0)
			{
				list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)1024));
				list.Add(new TooltipProperty(((object)new TextObject("{=25fleLuY}Troops In Reserve", (Dictionary<string, object>)null)).ToString(), reservedTroopsCountOfShip.ToString(), 0, false, (TooltipPropertyFlags)0));
			}
		}
		return list;
	}

	private FormationClass GetActualTroopType(Agent agent)
	{
		if (!QueryLibrary.IsInfantry(agent))
		{
			if (!QueryLibrary.IsRanged(agent))
			{
				if (!QueryLibrary.IsCavalry(agent))
				{
					if (!QueryLibrary.IsRangedCavalry(agent))
					{
						return (FormationClass)10;
					}
					return (FormationClass)3;
				}
				return (FormationClass)2;
			}
			return (FormationClass)1;
		}
		return (FormationClass)0;
	}
}
