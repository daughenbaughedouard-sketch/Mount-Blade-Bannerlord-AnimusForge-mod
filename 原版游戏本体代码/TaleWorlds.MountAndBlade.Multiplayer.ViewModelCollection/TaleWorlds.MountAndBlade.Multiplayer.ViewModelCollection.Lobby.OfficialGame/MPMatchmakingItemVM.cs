using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.OfficialGame;

public class MPMatchmakingItemVM : ViewModel
{
	private string _name;

	private string _type;

	private bool _isSelected;

	private bool _isAvailable;

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
	public string Type
	{
		get
		{
			return _type;
		}
		set
		{
			if (value != _type)
			{
				_type = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Type");
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
				this.OnSelectionChanged?.Invoke(this, _isSelected);
			}
		}
	}

	[DataSourceProperty]
	public bool IsAvailable
	{
		get
		{
			return _isAvailable;
		}
		set
		{
			if (value != _isAvailable)
			{
				_isAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAvailable");
			}
		}
	}

	public event Action<MPMatchmakingItemVM, bool> OnSelectionChanged;

	public event Action<MPMatchmakingItemVM> OnSetFocusItem;

	public event Action OnRemoveFocus;

	public unsafe MPMatchmakingItemVM(MultiplayerGameType type)
	{
		Type = ((object)(*(MultiplayerGameType*)(&type))/*cast due to .constrained prefix*/).ToString();
		IsAvailable = true;
		IsSelected = IsAvailable;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", Type)).ToString();
	}

	private void ExecuteSetFocusItem()
	{
		this.OnSetFocusItem?.Invoke(this);
	}

	private void ExecuteRemoveFocus()
	{
		this.OnRemoveFocus?.Invoke();
	}
}
