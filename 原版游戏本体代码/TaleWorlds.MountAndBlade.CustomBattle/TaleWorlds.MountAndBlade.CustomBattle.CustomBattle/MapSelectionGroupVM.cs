using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;

namespace TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;

public class MapSelectionGroupVM : ViewModel
{
	private bool _isCurrentMapSiege;

	private bool _isSallyOutSelected;

	private SelectorVM<MapItemVM> _mapSelection;

	private SelectorVM<SceneLevelItemVM> _sceneLevelSelection;

	private SelectorVM<WallHitpointItemVM> _wallHitpointSelection;

	private SelectorVM<SeasonItemVM> _seasonSelection;

	private SelectorVM<TimeOfDayItemVM> _timeOfDaySelection;

	private string _titleText;

	private string _mapText;

	private string _seasonText;

	private string _timeOfDayText;

	private string _sceneLevelText;

	private string _wallHitpointsText;

	private string _attackerSiegeMachinesText;

	private string _defenderSiegeMachinesText;

	private string _salloutText;

	public int SelectedWallBreachedCount { get; private set; }

	public int SelectedSceneLevel { get; private set; }

	public int SelectedTimeOfDay { get; private set; }

	public string SelectedSeasonId { get; private set; }

	public MapItemVM SelectedMap { get; private set; }

	private List<MapItemVM> _battleMaps { get; set; }

	private List<MapItemVM> _villageMaps { get; set; }

	private List<MapItemVM> _siegeMaps { get; set; }

	private List<MapItemVM> _availableMaps { get; set; }

	[DataSourceProperty]
	public SelectorVM<MapItemVM> MapSelection
	{
		get
		{
			return _mapSelection;
		}
		set
		{
			if (value != _mapSelection)
			{
				_mapSelection = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<MapItemVM>>(value, "MapSelection");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<SceneLevelItemVM> SceneLevelSelection
	{
		get
		{
			return _sceneLevelSelection;
		}
		set
		{
			if (value != _sceneLevelSelection)
			{
				_sceneLevelSelection = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<SceneLevelItemVM>>(value, "SceneLevelSelection");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<WallHitpointItemVM> WallHitpointSelection
	{
		get
		{
			return _wallHitpointSelection;
		}
		set
		{
			if (value != _wallHitpointSelection)
			{
				_wallHitpointSelection = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<WallHitpointItemVM>>(value, "WallHitpointSelection");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<SeasonItemVM> SeasonSelection
	{
		get
		{
			return _seasonSelection;
		}
		set
		{
			if (value != _seasonSelection)
			{
				_seasonSelection = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<SeasonItemVM>>(value, "SeasonSelection");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<TimeOfDayItemVM> TimeOfDaySelection
	{
		get
		{
			return _timeOfDaySelection;
		}
		set
		{
			if (value != _timeOfDaySelection)
			{
				_timeOfDaySelection = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<TimeOfDayItemVM>>(value, "TimeOfDaySelection");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCurrentMapSiege
	{
		get
		{
			return _isCurrentMapSiege;
		}
		set
		{
			if (value != _isCurrentMapSiege)
			{
				_isCurrentMapSiege = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCurrentMapSiege");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSallyOutSelected
	{
		get
		{
			return _isSallyOutSelected;
		}
		set
		{
			if (value != _isSallyOutSelected)
			{
				_isSallyOutSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSallyOutSelected");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string MapText
	{
		get
		{
			return _mapText;
		}
		set
		{
			if (value != _mapText)
			{
				_mapText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MapText");
			}
		}
	}

	[DataSourceProperty]
	public string SeasonText
	{
		get
		{
			return _seasonText;
		}
		set
		{
			if (value != _seasonText)
			{
				_seasonText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SeasonText");
			}
		}
	}

	[DataSourceProperty]
	public string TimeOfDayText
	{
		get
		{
			return _timeOfDayText;
		}
		set
		{
			if (value != _timeOfDayText)
			{
				_timeOfDayText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TimeOfDayText");
			}
		}
	}

	[DataSourceProperty]
	public string SceneLevelText
	{
		get
		{
			return _sceneLevelText;
		}
		set
		{
			if (value != _sceneLevelText)
			{
				_sceneLevelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SceneLevelText");
			}
		}
	}

	[DataSourceProperty]
	public string WallHitpointsText
	{
		get
		{
			return _wallHitpointsText;
		}
		set
		{
			if (value != _wallHitpointsText)
			{
				_wallHitpointsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "WallHitpointsText");
			}
		}
	}

	[DataSourceProperty]
	public string AttackerSiegeMachinesText
	{
		get
		{
			return _attackerSiegeMachinesText;
		}
		set
		{
			if (value != _attackerSiegeMachinesText)
			{
				_attackerSiegeMachinesText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AttackerSiegeMachinesText");
			}
		}
	}

	[DataSourceProperty]
	public string DefenderSiegeMachinesText
	{
		get
		{
			return _defenderSiegeMachinesText;
		}
		set
		{
			if (value != _defenderSiegeMachinesText)
			{
				_defenderSiegeMachinesText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DefenderSiegeMachinesText");
			}
		}
	}

	[DataSourceProperty]
	public string SalloutText
	{
		get
		{
			return _salloutText;
		}
		set
		{
			if (value != _salloutText)
			{
				_salloutText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SalloutText");
			}
		}
	}

	public MapSelectionGroupVM()
	{
		_battleMaps = new List<MapItemVM>();
		_villageMaps = new List<MapItemVM>();
		_siegeMaps = new List<MapItemVM>();
		_availableMaps = _battleMaps;
		MapSelection = new SelectorVM<MapItemVM>(0, (Action<SelectorVM<MapItemVM>>)OnMapSelection);
		WallHitpointSelection = new SelectorVM<WallHitpointItemVM>(0, (Action<SelectorVM<WallHitpointItemVM>>)OnWallHitpointSelection);
		SceneLevelSelection = new SelectorVM<SceneLevelItemVM>(0, (Action<SelectorVM<SceneLevelItemVM>>)OnSceneLevelSelection);
		SeasonSelection = new SelectorVM<SeasonItemVM>(0, (Action<SelectorVM<SeasonItemVM>>)OnSeasonSelection);
		TimeOfDaySelection = new SelectorVM<TimeOfDayItemVM>(0, (Action<SelectorVM<TimeOfDayItemVM>>)OnTimeOfDaySelection);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		PrepareMapLists();
		TitleText = ((object)new TextObject("{=customgametitle}Map", (Dictionary<string, object>)null)).ToString();
		MapText = ((object)new TextObject("{=customgamemapname}Map", (Dictionary<string, object>)null)).ToString();
		SeasonText = ((object)new TextObject("{=xTzDM5XE}Season", (Dictionary<string, object>)null)).ToString();
		TimeOfDayText = ((object)new TextObject("{=DszSWnc3}Time of Day", (Dictionary<string, object>)null)).ToString();
		SceneLevelText = ((object)new TextObject("{=0s52GQJt}Scene Level", (Dictionary<string, object>)null)).ToString();
		WallHitpointsText = ((object)new TextObject("{=4IuXGSdc}Wall Hitpoints", (Dictionary<string, object>)null)).ToString();
		AttackerSiegeMachinesText = ((object)new TextObject("{=AmfIfeIc}Choose Attacker Siege Machines", (Dictionary<string, object>)null)).ToString();
		DefenderSiegeMachinesText = ((object)new TextObject("{=UoiSWe87}Choose Defender Siege Machines", (Dictionary<string, object>)null)).ToString();
		SalloutText = ((object)new TextObject("{=EcKMGoFv}Sallyout", (Dictionary<string, object>)null)).ToString();
		((Collection<MapItemVM>)(object)MapSelection.ItemList).Clear();
		((Collection<WallHitpointItemVM>)(object)WallHitpointSelection.ItemList).Clear();
		((Collection<SceneLevelItemVM>)(object)SceneLevelSelection.ItemList).Clear();
		((Collection<SeasonItemVM>)(object)SeasonSelection.ItemList).Clear();
		((Collection<TimeOfDayItemVM>)(object)TimeOfDaySelection.ItemList).Clear();
		foreach (MapItemVM availableMap in _availableMaps)
		{
			MapSelection.AddItem(new MapItemVM(availableMap.MapName, availableMap.MapId));
		}
		foreach (Tuple<string, int> wallHitpoint in CustomBattleData.WallHitpoints)
		{
			WallHitpointSelection.AddItem(new WallHitpointItemVM(wallHitpoint.Item1, wallHitpoint.Item2));
		}
		foreach (int sceneLevel in CustomBattleData.SceneLevels)
		{
			SceneLevelSelection.AddItem(new SceneLevelItemVM(sceneLevel));
		}
		foreach (Tuple<string, string> season in CustomBattleData.Seasons)
		{
			SeasonSelection.AddItem(new SeasonItemVM(season.Item1, season.Item2));
		}
		foreach (Tuple<string, CustomBattleTimeOfDay> item in CustomBattleData.TimesOfDay)
		{
			TimeOfDaySelection.AddItem(new TimeOfDayItemVM(item.Item1, (int)item.Item2));
		}
		MapSelection.SelectedIndex = 0;
		WallHitpointSelection.SelectedIndex = 0;
		SceneLevelSelection.SelectedIndex = 0;
		SeasonSelection.SelectedIndex = 0;
		TimeOfDaySelection.SelectedIndex = 0;
	}

	public void ExecuteSallyOutChange()
	{
		IsSallyOutSelected = !IsSallyOutSelected;
	}

	private void PrepareMapLists()
	{
		_battleMaps.Clear();
		_villageMaps.Clear();
		_siegeMaps.Clear();
		bool isOnlyCoreContentEnabled = Module.CurrentModule.IsOnlyCoreContentEnabled;
		if (CustomGame.Current != null)
		{
			IEnumerable<CustomBattleSceneData> enumerable;
			if (isOnlyCoreContentEnabled)
			{
				enumerable = CustomGame.Current.CustomBattleScenes.Where((CustomBattleSceneData s) => s.SceneID == "battle_terrain_029");
			}
			else
			{
				IEnumerable<CustomBattleSceneData> enumerable2 = CustomGame.Current.CustomBattleScenes.ToList();
				enumerable = enumerable2;
			}
			foreach (CustomBattleSceneData item2 in enumerable)
			{
				MapItemVM item = new MapItemVM(((object)item2.Name).ToString(), item2.SceneID);
				if (item2.IsVillageMap)
				{
					_villageMaps.Add(item);
				}
				else if (item2.IsSiegeMap)
				{
					_siegeMaps.Add(item);
				}
				else if (!item2.IsLordsHallMap)
				{
					_battleMaps.Add(item);
				}
			}
		}
		Comparer<MapItemVM> comparer = Comparer<MapItemVM>.Create((MapItemVM x, MapItemVM y) => x.MapName.CompareTo(y.MapName));
		_battleMaps.Sort(comparer);
		_villageMaps.Sort(comparer);
		_siegeMaps.Sort(comparer);
	}

	private void OnMapSelection(SelectorVM<MapItemVM> selector)
	{
		SelectedMap = selector.SelectedItem;
	}

	private void OnWallHitpointSelection(SelectorVM<WallHitpointItemVM> selector)
	{
		SelectedWallBreachedCount = selector.SelectedItem.BreachedWallCount;
	}

	private void OnSceneLevelSelection(SelectorVM<SceneLevelItemVM> selector)
	{
		SelectedSceneLevel = selector.SelectedItem.Level;
	}

	private void OnSeasonSelection(SelectorVM<SeasonItemVM> selector)
	{
		SelectedSeasonId = selector.SelectedItem.SeasonId;
	}

	private void OnTimeOfDaySelection(SelectorVM<TimeOfDayItemVM> selector)
	{
		SelectedTimeOfDay = selector.SelectedItem.TimeOfDay;
	}

	public void OnGameTypeChange(string gameTypeStringId)
	{
		((Collection<MapItemVM>)(object)MapSelection.ItemList).Clear();
		switch (gameTypeStringId)
		{
		case "Battle":
			IsCurrentMapSiege = false;
			_availableMaps = _battleMaps;
			break;
		case "Village":
			IsCurrentMapSiege = false;
			_availableMaps = _villageMaps;
			break;
		case "Siege":
			IsCurrentMapSiege = true;
			_availableMaps = _siegeMaps;
			break;
		}
		foreach (MapItemVM availableMap in _availableMaps)
		{
			MapSelection.AddItem(availableMap);
		}
		MapSelection.SelectedIndex = 0;
	}

	public void RandomizeAll()
	{
		MapSelection.ExecuteRandomize();
		SceneLevelSelection.ExecuteRandomize();
		SeasonSelection.ExecuteRandomize();
		WallHitpointSelection.ExecuteRandomize();
		TimeOfDaySelection.ExecuteRandomize();
	}
}
