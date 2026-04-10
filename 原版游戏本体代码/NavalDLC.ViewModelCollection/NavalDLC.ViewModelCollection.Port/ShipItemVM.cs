using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.ViewModelCollection.Port.PortScreenHandlers;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port;

public class ShipItemVM : ViewModel
{
	public readonly Ship Ship;

	private string _changedName;

	private bool _isSelected;

	private bool _playerCanChangeShipName;

	private bool _isRepaired;

	private float _initialHp;

	private float _currentHp;

	private bool _hasChanges;

	private bool _isRenamed;

	private float _maxHp;

	private bool _isSold;

	private string _currentHpText;

	private bool _isBought;

	private string _maxHpText;

	private string _separatorText;

	private string _name;

	private string _hullName;

	private string _prefabId;

	private bool _isNight;

	private int _price;

	public bool _isHealthRelevant;

	private HintViewModel _changeShipNameHint;

	private ShipStatsVM _stats;

	private ShipUpgradeContainerVM _upgrades;

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
	public bool IsRepaired
	{
		get
		{
			return _isRepaired;
		}
		set
		{
			if (value != _isRepaired)
			{
				_isRepaired = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRepaired");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRenamed
	{
		get
		{
			return _isRenamed;
		}
		set
		{
			if (value != _isRenamed)
			{
				_isRenamed = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRenamed");
			}
		}
	}

	[DataSourceProperty]
	public bool PlayerCanChangeShipName
	{
		get
		{
			return _playerCanChangeShipName;
		}
		set
		{
			if (value != _playerCanChangeShipName)
			{
				_playerCanChangeShipName = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PlayerCanChangeShipName");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSold
	{
		get
		{
			return _isSold;
		}
		set
		{
			if (value != _isSold)
			{
				_isSold = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSold");
			}
		}
	}

	[DataSourceProperty]
	public float InitialHp
	{
		get
		{
			return _initialHp;
		}
		set
		{
			if (value != _initialHp)
			{
				_initialHp = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "InitialHp");
			}
		}
	}

	[DataSourceProperty]
	public bool HasChanges
	{
		get
		{
			return _hasChanges;
		}
		set
		{
			if (value != _hasChanges)
			{
				_hasChanges = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasChanges");
			}
		}
	}

	[DataSourceProperty]
	public bool IsBought
	{
		get
		{
			return _isBought;
		}
		set
		{
			if (value != _isBought)
			{
				_isBought = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBought");
			}
		}
	}

	[DataSourceProperty]
	public float CurrentHp
	{
		get
		{
			return _currentHp;
		}
		set
		{
			if (value != _currentHp)
			{
				_currentHp = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CurrentHp");
				RefreshHpStrings();
			}
		}
	}

	[DataSourceProperty]
	public float MaxHp
	{
		get
		{
			return _maxHp;
		}
		set
		{
			if (value != _maxHp)
			{
				_maxHp = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MaxHp");
				RefreshHpStrings();
			}
		}
	}

	[DataSourceProperty]
	public string CurrentHpText
	{
		get
		{
			return _currentHpText;
		}
		set
		{
			if (value != _currentHpText)
			{
				_currentHpText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentHpText");
			}
		}
	}

	[DataSourceProperty]
	public string MaxHpText
	{
		get
		{
			return _maxHpText;
		}
		set
		{
			if (value != _maxHpText)
			{
				_maxHpText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MaxHpText");
			}
		}
	}

	[DataSourceProperty]
	public string SeparatorText
	{
		get
		{
			return _separatorText;
		}
		set
		{
			if (value != _separatorText)
			{
				_separatorText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SeparatorText");
			}
		}
	}

	[DataSourceProperty]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public string HullName
	{
		get
		{
			return _hullName;
		}
		set
		{
			if (value != _hullName)
			{
				_hullName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "HullName");
			}
		}
	}

	[DataSourceProperty]
	public string PrefabId
	{
		get
		{
			return _prefabId;
		}
		set
		{
			if (value != _prefabId)
			{
				_prefabId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PrefabId");
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
			if (value != _isNight)
			{
				_isNight = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsNight");
			}
		}
	}

	[DataSourceProperty]
	public int Price
	{
		get
		{
			return _price;
		}
		set
		{
			if (value != _price)
			{
				_price = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Price");
			}
		}
	}

	[DataSourceProperty]
	public bool IsHealthRelevant
	{
		get
		{
			return _isHealthRelevant;
		}
		set
		{
			if (value != _isHealthRelevant)
			{
				_isHealthRelevant = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsHealthRelevant");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ChangeShipNameHint
	{
		get
		{
			return _changeShipNameHint;
		}
		set
		{
			if (value != _changeShipNameHint)
			{
				_changeShipNameHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ChangeShipNameHint");
			}
		}
	}

	[DataSourceProperty]
	public ShipStatsVM Stats
	{
		get
		{
			return _stats;
		}
		set
		{
			if (value != _stats)
			{
				_stats = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipStatsVM>(value, "Stats");
			}
		}
	}

	[DataSourceProperty]
	public ShipUpgradeContainerVM Upgrades
	{
		get
		{
			return _upgrades;
		}
		set
		{
			if (value != _upgrades)
			{
				_upgrades = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipUpgradeContainerVM>(value, "Upgrades");
			}
		}
	}

	public static event Action<ShipItemVM> OnSelected;

	public static event Action<ShipItemVM, string> OnRenamed;

	public static event Action<ShipItemVM> OnNameReset;

	public ShipItemVM(Ship ship)
	{
		Ship = ship;
		PrefabId = NavalUIHelper.GetPrefabIdOfShipHull(Ship.ShipHull);
		Stats = new ShipStatsVM(Ship);
		Upgrades = new ShipUpgradeContainerVM(this);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = (IsRenamed ? _changedName : ((object)Ship.Name).ToString());
		HullName = ((object)Ship.ShipHull.Name).ToString();
		((ViewModel)Upgrades).RefreshValues();
		((ViewModel)Stats).RefreshValues();
		RefreshHpStrings();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		((ViewModel)Upgrades).OnFinalize();
		((ViewModel)Stats).OnFinalize();
	}

	public void RefreshProperties(PortScreenHandler handler)
	{
		IsBought = ((IEnumerable<PortScreenHandler.ShipTradeInfo>)handler.ShipsToBuy).Any((PortScreenHandler.ShipTradeInfo x) => x.Ship == Ship);
		IsSold = ((IEnumerable<PortScreenHandler.ShipTradeInfo>)handler.ShipsToSell).Any((PortScreenHandler.ShipTradeInfo x) => x.Ship == Ship);
		IsRepaired = ((List<Ship>)(object)handler.ShipsToRepair).Contains(Ship);
		IsRenamed = ((IEnumerable<PortScreenHandler.ShipRenameInfo>)handler.ShipsToRename).Any((PortScreenHandler.ShipRenameInfo s) => s.Ship == Ship);
		InitialHp = Ship.HitPoints;
		MaxHp = Ship.MaxHitPoints;
		CurrentHp = (IsRepaired ? Ship.MaxHitPoints : Ship.HitPoints);
		IsHealthRelevant = InitialHp < MaxHp;
		HasChanges = IsBought || IsSold || IsRepaired || IsRenamed || ((IEnumerable<ShipUpgradeSlotBaseVM>)Upgrades.UpgradeSlots).Any((ShipUpgradeSlotBaseVM s) => s.IsChanged);
		if (((List<Ship>)(object)handler.LeftShips).Contains(Ship))
		{
			PortActionInfo canBuyShip = handler.GetCanBuyShip(Ship);
			Price = ((canBuyShip.IsRelevant && canBuyShip.IsEnabled) ? canBuyShip.GoldCost : 0);
		}
		else
		{
			PortActionInfo canSellShip = handler.GetCanSellShip(Ship);
			Price = ((canSellShip.IsRelevant && canSellShip.IsEnabled) ? canSellShip.GoldCost : 0);
		}
		((ViewModel)this).RefreshValues();
	}

	public void ExecuteChangeShipName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		InformationManager.ShowTextInquiry(new TextInquiryData(((object)new TextObject("{=rO84r0W1}Change Ship Name", (Dictionary<string, object>)null)).ToString(), string.Empty, true, true, ((object)GameTexts.FindText("str_done", (string)null)).ToString(), ((object)GameTexts.FindText("str_cancel", (string)null)).ToString(), (Action<string>)OnChangeShipNameDone, (Action)null, false, (Func<string, Tuple<bool, string>>)NavalUIHelper.IsStringApplicableForShipName, "", ""), false, false);
	}

	public void ExecuteSelect()
	{
		ShipItemVM.OnSelected?.Invoke(this);
	}

	public void ExecuteResetShipName()
	{
		if (IsRenamed)
		{
			_changedName = string.Empty;
			ShipItemVM.OnNameReset?.Invoke(this);
		}
	}

	private void OnChangeShipNameDone(string newName)
	{
		if (newName != Name)
		{
			_changedName = newName;
			ShipItemVM.OnRenamed?.Invoke(this, newName);
		}
	}

	private void RefreshHpStrings()
	{
		CurrentHpText = ((int)CurrentHp).ToString();
		MaxHpText = ((int)MaxHp).ToString();
		SeparatorText = " / ";
	}
}
