using System;
using System.Collections.Generic;
using System.IO;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.DedicatedCustomServer.ClientHelper;

public class DCSHelperMapItemVM : ViewModel
{
	private readonly Action<DCSHelperMapItemVM> _onSelection;

	private readonly UniqueSceneId _identifiers;

	private bool _isSelected;

	private bool _existsLocally;

	private bool _isCautionSpriteVisible;

	private bool _currentlyPlaying;

	private string _currentlyPlayingText;

	private string _mapName;

	private string _mapPathClean;

	private BasicTooltipViewModel _localMapHint;

	[DataSourceProperty]
	public string ExclamationMark => "!";

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
	public bool ExistsLocally
	{
		get
		{
			return _existsLocally;
		}
		set
		{
			if (value != _existsLocally)
			{
				_existsLocally = value;
				BasicTooltipViewModel localMapHint = LocalMapHint;
				if (localMapHint != null)
				{
					localMapHint.SetToolipCallback(_existsLocally ? new Func<List<TooltipProperty>>(GetTooltipProperties) : null);
				}
				((ViewModel)this).OnPropertyChangedWithValue(value, "ExistsLocally");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCautionSpriteVisible
	{
		get
		{
			return _isCautionSpriteVisible;
		}
		set
		{
			if (value != _isCautionSpriteVisible)
			{
				_isCautionSpriteVisible = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCautionSpriteVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool CurrentlyPlaying
	{
		get
		{
			return _currentlyPlaying;
		}
		set
		{
			if (value != _currentlyPlaying)
			{
				_currentlyPlaying = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CurrentlyPlaying");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentlyPlayingText
	{
		get
		{
			return _currentlyPlayingText;
		}
		set
		{
			if (value != _currentlyPlayingText)
			{
				_currentlyPlayingText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentlyPlayingText");
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
	public string MapPathClean
	{
		get
		{
			return _mapPathClean;
		}
		set
		{
			if (value != _mapPathClean)
			{
				_mapPathClean = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MapPathClean");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel LocalMapHint
	{
		get
		{
			return _localMapHint;
		}
		set
		{
			if (value != _localMapHint)
			{
				_localMapHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<BasicTooltipViewModel>(value, "LocalMapHint");
			}
		}
	}

	public DCSHelperMapItemVM(string mapName, Action<DCSHelperMapItemVM> onSelection, bool currentlyPlaying, UniqueSceneId identifiers)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		_mapName = mapName;
		_onSelection = onSelection;
		_currentlyPlaying = currentlyPlaying;
		_currentlyPlayingText = ((object)new TextObject("{=fy9RJLYf}(Currently playing)", (Dictionary<string, object>)null)).ToString();
		_identifiers = identifiers;
		LocalMapHint = new BasicTooltipViewModel();
	}

	public void ExecuteToggleSelection()
	{
		_onSelection?.Invoke(this);
	}

	public void RefreshLocalMapData()
	{
		string text = default(string);
		if (Utilities.TryGetFullFilePathOfScene(MapName, ref text))
		{
			IsSelected = false;
			MapPathClean = Path.GetDirectoryName(Path.GetFullPath(text));
			ExistsLocally = true;
			IsCautionSpriteVisible = GetIsCautionSpriteVisible(text);
		}
		else
		{
			MapPathClean = null;
			ExistsLocally = false;
		}
	}

	private bool GetIsCautionSpriteVisible(string existingLocalMapPath)
	{
		UniqueSceneId val = default(UniqueSceneId);
		if (_identifiers != null && Utilities.TryGetUniqueIdentifiersForSceneFile(existingLocalMapPath, ref val))
		{
			if (!(_identifiers.Revision != val.Revision))
			{
				return _identifiers.UniqueToken != val.UniqueToken;
			}
			return true;
		}
		return true;
	}

	private List<TooltipProperty> GetTooltipProperties()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>();
		if (IsCautionSpriteVisible)
		{
			list.Add(new TooltipProperty("", ((object)new TextObject("{=maLeU9XO}The map played on the server may not be identical to the local version.", (Dictionary<string, object>)null)).ToString(), 0, false, (TooltipPropertyFlags)0));
			list.Add(new TooltipProperty("", "", 0, false, (TooltipPropertyFlags)1024));
		}
		if (ExistsLocally)
		{
			list.Add(new TooltipProperty("", ((object)new TextObject("{=E8bDYaJq}This map already exists at {MAP_PATH}", (Dictionary<string, object>)null).SetTextVariable("MAP_PATH", MapPathClean)).ToString(), 0, false, (TooltipPropertyFlags)0));
		}
		return list;
	}
}
