using System;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.GameOver;

public class GameOverStatCategoryVM : ViewModel
{
	private readonly StatCategory _category;

	private readonly Action<GameOverStatCategoryVM> _onSelect;

	private string _name;

	private string _id;

	private bool _isSelected;

	private MBBindingList<GameOverStatItemVM> _items;

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
	public string ID
	{
		get
		{
			return _id;
		}
		set
		{
			if (value != _id)
			{
				_id = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ID");
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
	public MBBindingList<GameOverStatItemVM> Items
	{
		get
		{
			return _items;
		}
		set
		{
			if (value != _items)
			{
				_items = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<GameOverStatItemVM>>(value, "Items");
			}
		}
	}

	public GameOverStatCategoryVM(StatCategory category, Action<GameOverStatCategoryVM> onSelect)
	{
		_category = category;
		_onSelect = onSelect;
		Items = new MBBindingList<GameOverStatItemVM>();
		ID = category.ID;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		((Collection<GameOverStatItemVM>)(object)Items).Clear();
		Name = ((object)GameTexts.FindText("str_game_over_stat_category", _category.ID)).ToString();
		foreach (StatItem item in _category.Items)
		{
			((Collection<GameOverStatItemVM>)(object)Items).Add(new GameOverStatItemVM(item));
		}
	}

	public void ExecuteSelectCategory()
	{
		Action<GameOverStatCategoryVM> onSelect = _onSelect;
		if (onSelect != null)
		{
			Common.DynamicInvokeWithLog((Delegate)onSelect, new object[1] { this });
		}
	}
}
