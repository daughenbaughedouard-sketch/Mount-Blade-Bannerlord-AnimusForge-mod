using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.CustomBattle.CustomBattle.SelectionItem;

public class NavalCustomBattleShipHullItemVM : ViewModel
{
	public readonly ShipHull ShipHull;

	private readonly TextObject _nameText;

	private readonly Action<NavalCustomBattleShipHullItemVM> _onSelected;

	private BasicTooltipViewModel _tooltip;

	private HintViewModel _disabledHint;

	private string _name;

	private bool _isSelected;

	private bool _isDisabled;

	private bool _isEmpty;

	private string _prefabId;

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
	public bool IsEmpty
	{
		get
		{
			return _isEmpty;
		}
		set
		{
			if (value != _isEmpty)
			{
				_isEmpty = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEmpty");
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

	public NavalCustomBattleShipHullItemVM(ShipHull shipHull, Action<NavalCustomBattleShipHullItemVM> onSelected)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		ShipHull = shipHull;
		PrefabId = NavalUIHelper.GetPrefabIdOfShipHull(ShipHull);
		_nameText = ShipHull.Name;
		Tooltip = new BasicTooltipViewModel((Func<List<TooltipProperty>>)(() => GetTooltip()));
		_onSelected = onSelected;
		IsEmpty = false;
		((ViewModel)this).RefreshValues();
	}

	public NavalCustomBattleShipHullItemVM(TextObject nameText, TextObject disabledHintText, Action<NavalCustomBattleShipHullItemVM> onSelected)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		_nameText = nameText;
		_onSelected = onSelected;
		DisabledHint = new HintViewModel(disabledHintText, (string)null);
		IsEmpty = true;
		((ViewModel)this).RefreshValues();
	}

	protected virtual List<TooltipProperty> GetTooltip()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		object[] array = new object[1] { ShipHull };
		return ((IEnumerable<TooltipProperty>)new PropertyBasedTooltipVM(typeof(ShipHull), array).TooltipPropertyList).ToList();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = ((object)_nameText).ToString();
	}

	public void ExecuteSelect()
	{
		_onSelected?.Invoke(this);
	}
}
