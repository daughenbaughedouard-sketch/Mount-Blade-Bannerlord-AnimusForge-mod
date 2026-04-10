using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FactionBanVote;

public class MultiplayerFactionBanVoteVM : ViewModel
{
	private readonly Action<MultiplayerFactionBanVoteVM> _onSelect;

	public readonly BasicCultureObject Culture;

	private string _name;

	private bool _isEnabled;

	private bool _isSelected;

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
					_onSelect(this);
				}
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

	public MultiplayerFactionBanVoteVM(BasicCultureObject culture, Action<MultiplayerFactionBanVoteVM> onSelect)
	{
		Culture = culture;
		_onSelect = onSelect;
		_isEnabled = true;
		_name = ((object)culture.Name).ToString();
	}
}
