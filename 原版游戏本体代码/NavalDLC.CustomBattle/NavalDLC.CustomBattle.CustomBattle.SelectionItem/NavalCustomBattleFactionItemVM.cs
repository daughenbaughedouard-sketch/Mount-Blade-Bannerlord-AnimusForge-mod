using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.CustomBattle.CustomBattle.SelectionItem;

public class NavalCustomBattleFactionItemVM : ViewModel
{
	private Action<NavalCustomBattleFactionItemVM> _onSelected;

	private HintViewModel _hint;

	private string _cultureCode;

	private bool _isSelected;

	public BasicCultureObject Faction { get; private set; }

	[DataSourceProperty]
	public HintViewModel Hint
	{
		get
		{
			return _hint;
		}
		set
		{
			if (value != _hint)
			{
				_hint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "Hint");
			}
		}
	}

	[DataSourceProperty]
	public string CultureCode
	{
		get
		{
			return _cultureCode;
		}
		set
		{
			if (value != _cultureCode)
			{
				_cultureCode = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureCode");
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
				if (value)
				{
					_onSelected(this);
				}
			}
		}
	}

	public NavalCustomBattleFactionItemVM(BasicCultureObject faction, Action<NavalCustomBattleFactionItemVM> onSelected)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		Faction = faction;
		_onSelected = onSelected;
		CultureCode = ((MBObjectBase)faction).StringId.ToLower();
		Hint = new HintViewModel(faction.Name, (string)null);
	}
}
