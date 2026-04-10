using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Intermission;

public class MPIntermissionMapItemVM : ViewModel
{
	private readonly Action<MPIntermissionMapItemVM> _onPlayerVoted;

	private bool _isSelected;

	private string _mapID;

	private string _mapName;

	private int _votes;

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
	public string MapID
	{
		get
		{
			return _mapID;
		}
		set
		{
			if (value != _mapID)
			{
				_mapID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MapID");
			}
		}
	}

	[DataSourceProperty]
	public string MapName
	{
		get
		{
			return _mapName;
		}
		set
		{
			if (value != _mapName)
			{
				_mapName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MapName");
			}
		}
	}

	[DataSourceProperty]
	public int Votes
	{
		get
		{
			return _votes;
		}
		set
		{
			if (value != _votes)
			{
				_votes = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Votes");
			}
		}
	}

	public MPIntermissionMapItemVM(string mapID, Action<MPIntermissionMapItemVM> onPlayerVoted)
	{
		MapID = mapID;
		_onPlayerVoted = onPlayerVoted;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		TextObject val = default(TextObject);
		if (GameTexts.TryGetText("str_multiplayer_scene_name", ref val, MapID))
		{
			MapName = ((object)val).ToString();
		}
		else
		{
			MapName = MapID;
		}
	}

	public void ExecuteVote()
	{
		_onPlayerVoted(this);
	}
}
