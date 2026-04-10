using System;
using System.Collections.ObjectModel;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port;

public class ShipUpgradePieceBaseVM : ViewModel
{
	public enum ShipUpgradePieceTier
	{
		Bronze = 1,
		Silver,
		Gold,
		Diamond
	}

	public Action<ShipUpgradePieceBaseVM> _onSelected;

	private ShipUpgradePieceTier _upgradePieceTier = ShipUpgradePieceTier.Bronze;

	protected TextObject _slotHintText;

	private string _identifier;

	private string _name;

	private bool _isSelected;

	private bool _isDisabled;

	private bool _isInspected;

	private bool _isBronzeTier = true;

	private bool _isSilverTier;

	private bool _isGoldTier;

	private bool _isDiamondTier;

	private bool _isUnexamined;

	private bool _isHiddenFromPlayer;

	private int _price;

	private MBBindingList<StringPairItemVM> _properties;

	public ShipUpgradePieceTier UpgradePieceTier
	{
		get
		{
			return _upgradePieceTier;
		}
		set
		{
			if (_upgradePieceTier != value)
			{
				_upgradePieceTier = value;
				IsBronzeTier = _upgradePieceTier == ShipUpgradePieceTier.Bronze;
				IsSilverTier = _upgradePieceTier == ShipUpgradePieceTier.Silver;
				IsGoldTier = _upgradePieceTier == ShipUpgradePieceTier.Gold;
				IsDiamondTier = _upgradePieceTier == ShipUpgradePieceTier.Diamond;
			}
		}
	}

	public bool IsInspectedFromSlot { get; private set; }

	[DataSourceProperty]
	public string Identifier
	{
		get
		{
			return _identifier;
		}
		set
		{
			if (value != _identifier)
			{
				_identifier = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Identifier");
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
	public bool IsDisabled
	{
		get
		{
			return _isDisabled;
		}
		set
		{
			if (value != _isDisabled)
			{
				_isDisabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDisabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInspected
	{
		get
		{
			return _isInspected;
		}
		set
		{
			if (value != _isInspected)
			{
				_isInspected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInspected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDiamondTier
	{
		get
		{
			return _isDiamondTier;
		}
		set
		{
			if (value != _isDiamondTier)
			{
				_isDiamondTier = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDiamondTier");
			}
		}
	}

	[DataSourceProperty]
	public bool IsBronzeTier
	{
		get
		{
			return _isBronzeTier;
		}
		set
		{
			if (value != _isBronzeTier)
			{
				_isBronzeTier = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBronzeTier");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSilverTier
	{
		get
		{
			return _isSilverTier;
		}
		set
		{
			if (value != _isSilverTier)
			{
				_isSilverTier = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSilverTier");
			}
		}
	}

	[DataSourceProperty]
	public bool IsGoldTier
	{
		get
		{
			return _isGoldTier;
		}
		set
		{
			if (value != _isGoldTier)
			{
				_isGoldTier = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsGoldTier");
			}
		}
	}

	[DataSourceProperty]
	public bool IsUnexamined
	{
		get
		{
			return _isUnexamined;
		}
		set
		{
			if (value != _isUnexamined)
			{
				_isUnexamined = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsUnexamined");
			}
		}
	}

	[DataSourceProperty]
	public bool IsHiddenFromPlayer
	{
		get
		{
			return _isHiddenFromPlayer;
		}
		set
		{
			if (value != _isHiddenFromPlayer)
			{
				_isHiddenFromPlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsHiddenFromPlayer");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemVM> Properties
	{
		get
		{
			return _properties;
		}
		set
		{
			if (value != _properties)
			{
				_properties = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemVM>>(value, "Properties");
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

	public static event Action<ShipUpgradePieceBaseVM> OnInspected;

	public ShipUpgradePieceBaseVM(Action<ShipUpgradePieceBaseVM> onSelected)
	{
		_onSelected = onSelected;
		Properties = new MBBindingList<StringPairItemVM>();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		UpdateProperties();
	}

	protected virtual PropertyBasedTooltipVM GetProperties()
	{
		return null;
	}

	private void UpdateProperties()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		((Collection<StringPairItemVM>)(object)Properties).Clear();
		PropertyBasedTooltipVM properties = GetProperties();
		if (properties == null)
		{
			return;
		}
		for (int i = 0; i < ((Collection<TooltipProperty>)(object)properties.TooltipPropertyList).Count; i++)
		{
			TooltipProperty val = ((Collection<TooltipProperty>)(object)properties.TooltipPropertyList)[i];
			if (val.PropertyModifier != 4096)
			{
				((Collection<StringPairItemVM>)(object)Properties).Add(new StringPairItemVM(val.DefinitionLabel, val.ValueLabel, (BasicTooltipViewModel)null));
			}
		}
	}

	public void ExecuteSelect()
	{
		_onSelected?.Invoke(this);
	}

	public void ExecuteInspectBegin()
	{
		InspectPiece();
	}

	public virtual void InspectPiece(bool isInspectedFromSlot = false, TextObject slotHintText = null)
	{
		if (IsInspectedFromSlot != isInspectedFromSlot || _slotHintText != slotHintText)
		{
			IsInspectedFromSlot = isInspectedFromSlot;
			_slotHintText = slotHintText;
			UpdateProperties();
		}
		ShipUpgradePieceBaseVM.OnInspected?.Invoke(this);
	}

	public void ExecuteInspectEnd()
	{
		ShipUpgradePieceBaseVM.OnInspected?.Invoke(null);
	}

	public virtual void Update()
	{
	}
}
