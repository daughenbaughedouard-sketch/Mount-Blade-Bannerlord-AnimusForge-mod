using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.OfficialGame;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPMatchmakingVM : ViewModel
{
	public enum MatchmakingSubPages
	{
		QuickPlay,
		CustomGame,
		CustomGameList,
		PremadeMatchList,
		Default
	}

	private LobbyState _lobbyState;

	private bool _isTestRegionAvailable;

	private bool _regionsRequireRefresh;

	private MatchmakingSubPages _currentSubPage;

	private IEnumerable<MPHeroClass> _heroClasses;

	private TextObject _selectionInfoTextObject;

	private string[] _defaultSelectedGameTypes;

	private bool _isServerStatusReceived;

	private bool _isEligibleForPremadeMatches;

	private readonly Action<MPLobbyVM.LobbyPage> _onChangePageRequest;

	private readonly Action<string, bool> _onMatchSelectionChanged;

	private readonly Action<bool> _onGameFindRequestStateChanged;

	private bool _isEnabled;

	private bool _isRanked;

	private bool _isCustomGameStageFindEnabled;

	private bool _hasUnofficialModulesLoaded;

	private int _selectedSubPageIndex;

	private MBBindingList<MPMatchmakingItemVM> _quickplayGameTypes;

	private MBBindingList<MPMatchmakingItemVM> _rankedGameTypes;

	private MBBindingList<MPMatchmakingItemVM> _customGameTypes;

	private SelectorVM<MPMatchmakingRegionSelectorItemVM> _regions;

	private MPMatchmakingSelectionInfoVM _selectionInfo;

	private MPCustomGameVM _customServer;

	private MPCustomGameVM _premadeMatches;

	private bool _isMatchFindPossible;

	private bool _isFindingMatch;

	private bool _isRankedGamesEnabled;

	private bool _isCustomGamesEnabled;

	private bool _isQuickplayGamesEnabled;

	private bool _isCustomServerListEnabled;

	private bool _isCustomServerFeatureSupported;

	private bool _isPremadeGamesEnabled;

	private bool _isClansFeatureSupported;

	private string _playText;

	private string _autoFindText;

	private string _matchFindNotPossibleText;

	private string _rankedText;

	private string _casualText;

	private string _selectionInfoText;

	private string _quickPlayText;

	private string _customGameText;

	private string _customServerListText;

	private string _communityGameText;

	private string _teamMatchesText;

	private HintViewModel _regionsHint;

	public MatchmakingSubPages CurrentSubPage => _currentSubPage;

	private bool IsServerQuickPlayAvailable => NetworkMain.GameClient.IsAbleToSearchForGame;

	private bool IsServerCustomGameListAvailable
	{
		get
		{
			if (NetworkMain.GameClient.IsCustomBattleAvailable)
			{
				return !IsFindingMatch;
			}
			return false;
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
	public bool HasUnofficialModulesLoaded
	{
		get
		{
			return _hasUnofficialModulesLoaded;
		}
		set
		{
			if (value != _hasUnofficialModulesLoaded)
			{
				_hasUnofficialModulesLoaded = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasUnofficialModulesLoaded");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCustomGameStageFindEnabled
	{
		get
		{
			return _isCustomGameStageFindEnabled;
		}
		set
		{
			if (value != _isCustomGameStageFindEnabled)
			{
				_isCustomGameStageFindEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCustomGameStageFindEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRankedGamesEnabled
	{
		get
		{
			return _isRankedGamesEnabled;
		}
		set
		{
			if (value != _isRankedGamesEnabled)
			{
				_isRankedGamesEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRankedGamesEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCustomGamesEnabled
	{
		get
		{
			return _isCustomGamesEnabled;
		}
		set
		{
			if (value != _isCustomGamesEnabled)
			{
				_isCustomGamesEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCustomGamesEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsQuickplayGamesEnabled
	{
		get
		{
			return _isQuickplayGamesEnabled;
		}
		set
		{
			if (value != _isQuickplayGamesEnabled)
			{
				_isQuickplayGamesEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsQuickplayGamesEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCustomServerListEnabled
	{
		get
		{
			return _isCustomServerListEnabled;
		}
		set
		{
			if (value != _isCustomServerListEnabled)
			{
				_isCustomServerListEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCustomServerListEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPremadeGamesEnabled
	{
		get
		{
			return _isPremadeGamesEnabled;
		}
		set
		{
			if (value != _isPremadeGamesEnabled)
			{
				_isPremadeGamesEnabled = value;
				((ViewModel)this).OnPropertyChanged("IsPremadeGamesEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCustomServerFeatureSupported
	{
		get
		{
			return _isCustomServerFeatureSupported;
		}
		set
		{
			if (value != _isCustomServerFeatureSupported)
			{
				_isCustomServerFeatureSupported = value;
				((ViewModel)this).OnPropertyChanged("IsCustomServerFeatureSupported");
			}
		}
	}

	[DataSourceProperty]
	public bool IsClansFeatureSupported
	{
		get
		{
			return _isClansFeatureSupported;
		}
		set
		{
			if (value != _isClansFeatureSupported)
			{
				_isClansFeatureSupported = value;
				((ViewModel)this).OnPropertyChanged("IsClansFeatureSupported");
			}
		}
	}

	[DataSourceProperty]
	public MPCustomGameVM CustomServer
	{
		get
		{
			return _customServer;
		}
		set
		{
			if (value != _customServer)
			{
				_customServer = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPCustomGameVM>(value, "CustomServer");
			}
		}
	}

	[DataSourceProperty]
	public MPCustomGameVM PremadeMatches
	{
		get
		{
			return _premadeMatches;
		}
		set
		{
			if (value != _premadeMatches)
			{
				_premadeMatches = value;
				((ViewModel)this).OnPropertyChanged("PremadeMatches");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRanked
	{
		get
		{
			return _isRanked;
		}
		set
		{
			if (value != _isRanked)
			{
				_isRanked = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRanked");
				OnSelectionChanged();
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPMatchmakingItemVM> RankedGameTypes
	{
		get
		{
			return _rankedGameTypes;
		}
		set
		{
			if (value != _rankedGameTypes)
			{
				_rankedGameTypes = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPMatchmakingItemVM>>(value, "RankedGameTypes");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPMatchmakingItemVM> QuickplayGameTypes
	{
		get
		{
			return _quickplayGameTypes;
		}
		set
		{
			if (value != _quickplayGameTypes)
			{
				_quickplayGameTypes = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPMatchmakingItemVM>>(value, "QuickplayGameTypes");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPMatchmakingItemVM> CustomGameTypes
	{
		get
		{
			return _customGameTypes;
		}
		set
		{
			if (value != _customGameTypes)
			{
				_customGameTypes = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPMatchmakingItemVM>>(value, "CustomGameTypes");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<MPMatchmakingRegionSelectorItemVM> Regions
	{
		get
		{
			return _regions;
		}
		set
		{
			if (value != _regions)
			{
				_regions = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<MPMatchmakingRegionSelectorItemVM>>(value, "Regions");
			}
		}
	}

	[DataSourceProperty]
	public MPMatchmakingSelectionInfoVM SelectionInfo
	{
		get
		{
			return _selectionInfo;
		}
		set
		{
			if (value != _selectionInfo)
			{
				_selectionInfo = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPMatchmakingSelectionInfoVM>(value, "SelectionInfo");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMatchFindPossible
	{
		get
		{
			return _isMatchFindPossible;
		}
		set
		{
			if (value != _isMatchFindPossible)
			{
				_isMatchFindPossible = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMatchFindPossible");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFindingMatch
	{
		get
		{
			return _isFindingMatch;
		}
		set
		{
			if (value != _isFindingMatch)
			{
				_isFindingMatch = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFindingMatch");
				IsFindingMatchUpdated();
			}
		}
	}

	[DataSourceProperty]
	public string PlayText
	{
		get
		{
			return _playText;
		}
		set
		{
			if (value != _playText)
			{
				_playText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PlayText");
			}
		}
	}

	[DataSourceProperty]
	public string QuickPlayText
	{
		get
		{
			return _quickPlayText;
		}
		set
		{
			if (value != _quickPlayText)
			{
				_quickPlayText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "QuickPlayText");
			}
		}
	}

	[DataSourceProperty]
	public string CustomGameText
	{
		get
		{
			return _customGameText;
		}
		set
		{
			if (value != _customGameText)
			{
				_customGameText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CustomGameText");
			}
		}
	}

	[DataSourceProperty]
	public string CommunityGameText
	{
		get
		{
			return _communityGameText;
		}
		set
		{
			if (value != _communityGameText)
			{
				_communityGameText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CommunityGameText");
			}
		}
	}

	[DataSourceProperty]
	public string CustomServerListText
	{
		get
		{
			return _customServerListText;
		}
		set
		{
			if (value != _customServerListText)
			{
				_customServerListText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CustomServerListText");
			}
		}
	}

	[DataSourceProperty]
	public string AutoFindText
	{
		get
		{
			return _autoFindText;
		}
		set
		{
			if (value != _autoFindText)
			{
				_autoFindText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AutoFindText");
			}
		}
	}

	[DataSourceProperty]
	public string MatchFindNotPossibleText
	{
		get
		{
			return _matchFindNotPossibleText;
		}
		set
		{
			if (value != _matchFindNotPossibleText)
			{
				_matchFindNotPossibleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MatchFindNotPossibleText");
			}
		}
	}

	[DataSourceProperty]
	public string RankedText
	{
		get
		{
			return _rankedText;
		}
		set
		{
			if (value != _rankedText)
			{
				_rankedText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RankedText");
			}
		}
	}

	[DataSourceProperty]
	public string CasualText
	{
		get
		{
			return _casualText;
		}
		set
		{
			if (value != _casualText)
			{
				_casualText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CasualText");
			}
		}
	}

	[DataSourceProperty]
	public string SelectionInfoText
	{
		get
		{
			return _selectionInfoText;
		}
		set
		{
			if (value != _selectionInfoText)
			{
				_selectionInfoText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SelectionInfoText");
			}
		}
	}

	[DataSourceProperty]
	public int SelectedSubPageIndex
	{
		get
		{
			return _selectedSubPageIndex;
		}
		set
		{
			if (value != _selectedSubPageIndex)
			{
				_selectedSubPageIndex = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SelectedSubPageIndex");
			}
		}
	}

	[DataSourceProperty]
	public string TeamMatchesText
	{
		get
		{
			return _teamMatchesText;
		}
		set
		{
			if (value != _teamMatchesText)
			{
				_teamMatchesText = value;
				((ViewModel)this).OnPropertyChanged("TeamMatchesText");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel RegionsHint
	{
		get
		{
			return _regionsHint;
		}
		set
		{
			if (value != _regionsHint)
			{
				_regionsHint = value;
				((ViewModel)this).OnPropertyChanged("RegionsHint");
			}
		}
	}

	public MPMatchmakingVM(LobbyState lobbyState, Action<MPLobbyVM.LobbyPage> onChangePageRequest, Action<string, bool> onMatchSelectionChanged, Action<bool> onGameFindRequestStateChanged)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		_lobbyState = lobbyState;
		_onChangePageRequest = onChangePageRequest;
		_onMatchSelectionChanged = onMatchSelectionChanged;
		_onGameFindRequestStateChanged = onGameFindRequestStateChanged;
		HasUnofficialModulesLoaded = NetworkMain.GameClient.HasUnofficialModulesLoaded;
		RankedGameTypes = new MBBindingList<MPMatchmakingItemVM>();
		CustomGameTypes = new MBBindingList<MPMatchmakingItemVM>();
		QuickplayGameTypes = new MBBindingList<MPMatchmakingItemVM>();
		CustomServer = new MPCustomGameVM(lobbyState, MPCustomGameVM.CustomGameMode.CustomServer);
		PremadeMatches = new MPCustomGameVM(lobbyState, MPCustomGameVM.CustomGameMode.PremadeGame);
		RefreshSubPageStates();
		_selectionInfoTextObject = new TextObject("{=wuKqRvc3}Game: {GAME_TYPES}  |  Region: {REGIONS}", (Dictionary<string, object>)null);
		InformationManager.OnHideInquiry += OnHideInquiry;
		_defaultSelectedGameTypes = MultiplayerMain.GetUserSelectedGameTypes();
		SelectionInfo = new MPMatchmakingSelectionInfoVM();
		UpdateQuickPlayGameTypeList();
		UpdateCustomGameTypeList();
		_heroClasses = (IEnumerable<MPHeroClass>)MultiplayerClassDivisions.GetMPHeroClasses();
		IsRanked = true;
		Regions = new SelectorVM<MPMatchmakingRegionSelectorItemVM>(0, (Action<SelectorVM<MPMatchmakingRegionSelectorItemVM>>)null);
		((ViewModel)this).RefreshValues();
		RefreshWaitingTime();
		OnSelectionChanged(updatedRegion: true, updatedGameTypes: true);
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		PlayText = ((object)new TextObject("{=wTtyFa89}PLAY", (Dictionary<string, object>)null)).ToString();
		MatchFindNotPossibleText = ((object)new TextObject("{=BrYUHFsg}CHOOSE GAME", (Dictionary<string, object>)null)).ToString();
		AutoFindText = ((object)new TextObject("{=S2bKbhTc}AUTO FIND GAME", (Dictionary<string, object>)null)).ToString();
		RankedText = ((object)GameTexts.FindText("str_multiplayer_ranked", (string)null)).ToString();
		CasualText = ((object)new TextObject("{=GXosklej}Casual", (Dictionary<string, object>)null)).ToString();
		RankedText = ((object)GameTexts.FindText("str_multiplayer_ranked", (string)null)).ToString();
		QuickPlayText = ((object)GameTexts.FindText("str_multiplayer_quick_play", (string)null)).ToString();
		CustomGameText = ((object)GameTexts.FindText("str_multiplayer_custom_game", (string)null)).ToString();
		CustomServerListText = ((object)GameTexts.FindText("str_multiplayer_custom_server_list", (string)null)).ToString();
		TeamMatchesText = ((object)new TextObject("{=PE5LqC9O}Team Matches", (Dictionary<string, object>)null)).ToString();
		CommunityGameText = ((object)new TextObject("{=SIIgjILk}Community Games", (Dictionary<string, object>)null)).ToString();
		RegionsHint = new HintViewModel(new TextObject("{=LzdUwRJo}Select a region for Quick Play and Custom Game", (Dictionary<string, object>)null), (string)null);
		QuickplayGameTypes.ApplyActionOnAllItems((Action<MPMatchmakingItemVM>)delegate(MPMatchmakingItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		RankedGameTypes.ApplyActionOnAllItems((Action<MPMatchmakingItemVM>)delegate(MPMatchmakingItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		CustomGameTypes.ApplyActionOnAllItems((Action<MPMatchmakingItemVM>)delegate(MPMatchmakingItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		((ViewModel)CustomServer).RefreshValues();
		((ViewModel)PremadeMatches).RefreshValues();
		_regionsRequireRefresh = true;
		((ViewModel)Regions).RefreshValues();
		((ViewModel)SelectionInfo).RefreshValues();
	}

	private void RefreshRegionsList()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		string currentRegion = MultiplayerMain.GetUserCurrentRegion();
		string[] availableMatchmakerRegions = MultiplayerMain.GetAvailableMatchmakerRegions();
		List<MPMatchmakingRegionSelectorItemVM> list = new List<MPMatchmakingRegionSelectorItemVM>();
		if (_isTestRegionAvailable)
		{
			MPMatchmakingRegionSelectorItemVM item = new MPMatchmakingRegionSelectorItemVM("Test", new TextObject("{=!}Test", (Dictionary<string, object>)null));
			list.Add(item);
		}
		foreach (string text in availableMatchmakerRegions)
		{
			TextObject regionName = GameTexts.FindText("str_multiplayer_region_name", text);
			list.Add(new MPMatchmakingRegionSelectorItemVM(text, regionName));
		}
		list.Add(new MPMatchmakingRegionSelectorItemVM("None", GameTexts.FindText("str_multiplayer_region_name_none", (string)null)));
		int num = list.FindIndex((MPMatchmakingRegionSelectorItemVM r) => r.RegionCode == currentRegion);
		int num2 = ((num != -1) ? num : list.FindIndex((MPMatchmakingRegionSelectorItemVM r) => r.IsRegionNone));
		Regions.Refresh((IEnumerable<MPMatchmakingRegionSelectorItemVM>)list, num2, (Action<SelectorVM<MPMatchmakingRegionSelectorItemVM>>)OnRegionSelectionChanged);
	}

	internal void OnTick(float dt)
	{
		CustomServer.OnTick(dt);
		PremadeMatches.OnTick(dt);
		if (_regionsRequireRefresh)
		{
			RefreshRegionsList();
			_regionsRequireRefresh = false;
			OnSelectionChanged(updatedRegion: true, updatedGameTypes: true);
		}
	}

	public void TrySetMatchmakingSubPage(MatchmakingSubPages newPage)
	{
		if (_currentSubPage == newPage || ((newPage == MatchmakingSubPages.CustomGameList || newPage == MatchmakingSubPages.CustomGame) && !IsServerCustomGameListAvailable) || (newPage == MatchmakingSubPages.QuickPlay && !IsServerQuickPlayAvailable))
		{
			return;
		}
		if (newPage == MatchmakingSubPages.Default)
		{
			if (IsServerQuickPlayAvailable && !HasUnofficialModulesLoaded)
			{
				newPage = MatchmakingSubPages.QuickPlay;
			}
			else
			{
				if (!IsServerCustomGameListAvailable)
				{
					Debug.FailedAssert("Trying to open matchmaking when nothing is available", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\OfficialGame\\MPMatchmakingVM.cs", "TrySetMatchmakingSubPage", 182);
					return;
				}
				newPage = MatchmakingSubPages.CustomGameList;
			}
		}
		_currentSubPage = newPage;
		SelectedSubPageIndex = (int)newPage;
		CustomServer.IsEnabled = newPage == MatchmakingSubPages.CustomGameList;
		PremadeMatches.IsEnabled = newPage == MatchmakingSubPages.PremadeMatchList;
		IsCustomGameStageFindEnabled = ((IEnumerable<MPMatchmakingItemVM>)CustomGameTypes).Any((MPMatchmakingItemVM g) => g.IsSelected);
		OnSelectionChanged();
	}

	public void RefreshPlayerData(PlayerData playerData)
	{
		if (!string.IsNullOrEmpty(playerData.LastRegion))
		{
			for (int i = 0; i < ((Collection<MPMatchmakingRegionSelectorItemVM>)(object)Regions.ItemList).Count; i++)
			{
				if (playerData.LastRegion == ((Collection<MPMatchmakingRegionSelectorItemVM>)(object)Regions.ItemList)[i].RegionCode)
				{
					Regions.SelectedIndex = i;
					break;
				}
			}
		}
		else
		{
			Regions.SelectedIndex = Extensions.FindIndex<MPMatchmakingRegionSelectorItemVM>((IReadOnlyList<MPMatchmakingRegionSelectorItemVM>)Regions.ItemList, (Func<MPMatchmakingRegionSelectorItemVM, bool>)((MPMatchmakingRegionSelectorItemVM r) => r.IsRegionNone));
		}
		string[] lastGameTypes = playerData.LastGameTypes;
		if (lastGameTypes != null)
		{
			foreach (MPMatchmakingItemVM item in (Collection<MPMatchmakingItemVM>)(object)QuickplayGameTypes)
			{
				item.IsSelected = lastGameTypes.Contains(item.Type);
			}
			return;
		}
		foreach (MPMatchmakingItemVM item2 in (Collection<MPMatchmakingItemVM>)(object)QuickplayGameTypes)
		{
			item2.IsSelected = false;
		}
	}

	private void GameModeOnSetFocusItem(MPMatchmakingItemVM sender)
	{
		SelectionInfo.UpdateForGameType(sender.Type);
		SelectionInfo.SetEnabled(isEnabled: true);
	}

	private void GameModeOnRemoveFocus()
	{
		SelectionInfo.SetEnabled(isEnabled: false);
	}

	private void GameModeOnSelectionChanged(MPMatchmakingItemVM sender, bool isSelected)
	{
		OnSelectionChanged(updatedRegion: false, updatedGameTypes: true);
	}

	private void OnRegionSelectionChanged(SelectorVM<MPMatchmakingRegionSelectorItemVM> selectorVM)
	{
		if (selectorVM.SelectedItem != null)
		{
			OnSelectionChanged(updatedRegion: true);
		}
		else
		{
			_regionsRequireRefresh = true;
		}
	}

	private void OnSelectionChanged(bool updatedRegion = false, bool updatedGameTypes = false)
	{
		string[] gameTypes;
		bool selectedGameTypesInfo = GetSelectedGameTypesInfo(out gameTypes);
		_selectionInfoTextObject.SetTextVariable("GAME_TYPES", MPLobbyVM.GetLocalizedGameTypesString(gameTypes));
		IsMatchFindPossible = SelectedSubPageIndex == 0 && selectedGameTypesInfo && NetworkMain.GameClient.IsAbleToSearchForGame;
		IsCustomGameStageFindEnabled = SelectedSubPageIndex == 1 && selectedGameTypesInfo;
		if (!_regionsRequireRefresh && updatedRegion)
		{
			MPMatchmakingRegionSelectorItemVM selectedItem = Regions.SelectedItem;
			if (selectedItem != null && !selectedItem.IsRegionNone)
			{
				MPMatchmakingRegionSelectorItemVM selectedItem2 = Regions.SelectedItem;
				string text = ((selectedItem2 != null) ? ((SelectorItemVM)selectedItem2).StringItem : null);
				_selectionInfoTextObject.SetTextVariable("REGIONS", text);
				string regionCode = Regions.SelectedItem.RegionCode;
				NetworkMain.GameClient.ChangeRegion(regionCode);
			}
		}
		if (updatedGameTypes && IsMatchFindPossible)
		{
			NetworkMain.GameClient.ChangeGameTypes(gameTypes.ToArray());
		}
		SelectionInfoText = ((object)_selectionInfoTextObject).ToString();
		_onMatchSelectionChanged?.Invoke(SelectionInfoText, IsMatchFindPossible);
	}

	internal void OnServerStatusReceived(ServerStatus serverStatus)
	{
		_isServerStatusReceived = true;
		CustomServer.IsPlayerBasedCustomBattleEnabled = serverStatus.IsPlayerBasedCustomBattleEnabled;
		PremadeMatches.IsPremadeGameEnabled = serverStatus.IsPremadeGameEnabled;
		if (_isTestRegionAvailable != serverStatus.IsTestRegionEnabled)
		{
			_isTestRegionAvailable = serverStatus.IsTestRegionEnabled;
			_regionsRequireRefresh = true;
		}
		RefreshSubPageStates();
	}

	public void OnFindingGame()
	{
		IsFindingMatch = true;
		RefreshSubPageStates();
	}

	public void OnCancelFindingGame()
	{
		IsFindingMatch = false;
		RefreshSubPageStates();
	}

	public override void OnFinalize()
	{
		InformationManager.OnHideInquiry -= OnHideInquiry;
		((ViewModel)CustomServer).OnFinalize();
		foreach (MPMatchmakingItemVM item in (Collection<MPMatchmakingItemVM>)(object)RankedGameTypes)
		{
			item.OnSelectionChanged -= GameModeOnSelectionChanged;
			item.OnSetFocusItem -= GameModeOnSetFocusItem;
			item.OnRemoveFocus -= GameModeOnRemoveFocus;
		}
		foreach (MPMatchmakingItemVM item2 in (Collection<MPMatchmakingItemVM>)(object)QuickplayGameTypes)
		{
			item2.OnSelectionChanged -= GameModeOnSelectionChanged;
			item2.OnSetFocusItem -= GameModeOnSetFocusItem;
			item2.OnRemoveFocus -= GameModeOnRemoveFocus;
		}
		foreach (MPMatchmakingItemVM item3 in (Collection<MPMatchmakingItemVM>)(object)CustomGameTypes)
		{
			item3.OnSelectionChanged -= GameModeOnSelectionChanged;
			item3.OnSetFocusItem -= GameModeOnSetFocusItem;
			item3.OnRemoveFocus -= GameModeOnRemoveFocus;
		}
		((ViewModel)this).OnFinalize();
	}

	public bool GetSelectedGameTypesInfo(out string[] gameTypes)
	{
		List<string> list = new List<string>();
		bool result = false;
		MBBindingList<MPMatchmakingItemVM> currentSubPageList = GetCurrentSubPageList();
		for (int i = 0; i < ((Collection<MPMatchmakingItemVM>)(object)currentSubPageList).Count; i++)
		{
			MPMatchmakingItemVM mPMatchmakingItemVM = ((Collection<MPMatchmakingItemVM>)(object)currentSubPageList)[i];
			if (mPMatchmakingItemVM.IsSelected)
			{
				list.Add(mPMatchmakingItemVM.Type);
				result = true;
			}
		}
		gameTypes = list.ToArray();
		return result;
	}

	private MBBindingList<MPMatchmakingItemVM> GetCurrentSubPageList()
	{
		return (MBBindingList<MPMatchmakingItemVM>)((MatchmakingSubPages)SelectedSubPageIndex switch
		{
			MatchmakingSubPages.CustomGame => CustomGameTypes, 
			_ => QuickplayGameTypes, 
		});
	}

	private void OnHideInquiry()
	{
		if (IsFindingMatch)
		{
			ExecuteCancelFindingGame();
		}
	}

	public void RefreshWaitingTime()
	{
		MBTextManager.SetTextVariable("WAIT_TIME", 10);
	}

	public void ExecuteAutoFindGame()
	{
		if (!((IEnumerable<MPMatchmakingItemVM>)QuickplayGameTypes).Where((MPMatchmakingItemVM q) => q.IsSelected).Any())
		{
			for (int num = 0; num < ((Collection<MPMatchmakingItemVM>)(object)QuickplayGameTypes).Count; num++)
			{
				((Collection<MPMatchmakingItemVM>)(object)QuickplayGameTypes)[num].IsSelected = true;
			}
		}
		ExecuteFindGame();
	}

	private async void ExecuteFindGame()
	{
		if (IsFindingMatch)
		{
			return;
		}
		if (Regions.SelectedItem.IsRegionNone)
		{
			InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_multiplayer_no_region_query_title", (string)null)).ToString(), ((object)GameTexts.FindText("str_multiplayer_no_region_query_description", (string)null)).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), "", (Action)delegate
			{
				_onChangePageRequest?.Invoke(MPLobbyVM.LobbyPage.Matchmaking);
			}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			return;
		}
		string[] array = (from q in (IEnumerable<MPMatchmakingItemVM>)GetCurrentSubPageList()
			where q.IsSelected
			select q.Type).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		if (SelectedSubPageIndex == 1)
		{
			if (!(await NetworkMain.GameClient.FindCustomGame(array, _lobbyState.HasCrossplayPrivilege, MultiplayerMain.GetUserCurrentRegion())))
			{
				InformationManager.ShowInquiry(new InquiryData("", ((object)new TextObject("{=NaZ6xg33}Couldn't find an applicable server to join.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), "", (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
			return;
		}
		_onGameFindRequestStateChanged?.Invoke(obj: true);
		PlatformServices.Instance.CheckPrivilege((Privilege)2, true, (PrivilegeResult)delegate(bool result)
		{
			_onGameFindRequestStateChanged?.Invoke(obj: false);
			if (result)
			{
				NetworkMain.GameClient.FindGame();
			}
		});
	}

	public void RefreshSubPageStates()
	{
		if (_isServerStatusReceived)
		{
			IsCustomServerListEnabled = IsServerCustomGameListAvailable;
			IsQuickplayGamesEnabled = IsServerQuickPlayAvailable && !HasUnofficialModulesLoaded;
			IsCustomGamesEnabled = IsServerCustomGameListAvailable && !HasUnofficialModulesLoaded;
			IsRankedGamesEnabled = false;
			_isEligibleForPremadeMatches = NetworkMain.GameClient.IsEligibleToCreatePremadeGame;
			IsPremadeGamesEnabled = _isEligibleForPremadeMatches && !IsFindingMatch && !HasUnofficialModulesLoaded;
			if ((CurrentSubPage == MatchmakingSubPages.CustomGame || CurrentSubPage == MatchmakingSubPages.CustomGameList) && !IsCustomServerListEnabled)
			{
				TrySetMatchmakingSubPage(MatchmakingSubPages.Default);
			}
			else if (CurrentSubPage == MatchmakingSubPages.QuickPlay && !IsServerQuickPlayAvailable)
			{
				TrySetMatchmakingSubPage(MatchmakingSubPages.Default);
			}
		}
		else
		{
			IsCustomServerListEnabled = false;
			IsQuickplayGamesEnabled = false;
			IsRankedGamesEnabled = false;
			IsCustomGamesEnabled = false;
			IsPremadeGamesEnabled = false;
		}
	}

	private void ExecuteCancelFindingGame()
	{
		if (IsFindingMatch)
		{
			OnCancelFindingGame();
			NetworkMain.GameClient.CancelFindGame();
		}
	}

	private void IsFindingMatchUpdated()
	{
		foreach (MPMatchmakingItemVM item in (Collection<MPMatchmakingItemVM>)(object)RankedGameTypes)
		{
			item.IsAvailable = !IsFindingMatch;
		}
	}

	private void ExecuteChangeEnabledSubPage(int subpageIndex)
	{
		TrySetMatchmakingSubPage((MatchmakingSubPages)subpageIndex);
	}

	private void UpdateRankedGameTypesList()
	{
		MultiplayerGameType[] availableRankedGameModes = MultiplayerMain.GetAvailableRankedGameModes();
		for (int i = 0; i < availableRankedGameModes.Length; i++)
		{
			MPMatchmakingItemVM mPMatchmakingItemVM = new MPMatchmakingItemVM(availableRankedGameModes[i]);
			mPMatchmakingItemVM.IsSelected = _defaultSelectedGameTypes.Contains(mPMatchmakingItemVM.Type);
			mPMatchmakingItemVM.OnSelectionChanged += GameModeOnSelectionChanged;
			mPMatchmakingItemVM.OnSetFocusItem += GameModeOnSetFocusItem;
			mPMatchmakingItemVM.OnRemoveFocus += GameModeOnRemoveFocus;
			((Collection<MPMatchmakingItemVM>)(object)RankedGameTypes).Add(mPMatchmakingItemVM);
		}
	}

	private void UpdateQuickPlayGameTypeList()
	{
		MultiplayerGameType[] availableQuickPlayGameModes = MultiplayerMain.GetAvailableQuickPlayGameModes();
		for (int i = 0; i < availableQuickPlayGameModes.Length; i++)
		{
			MPMatchmakingItemVM mPMatchmakingItemVM = new MPMatchmakingItemVM(availableQuickPlayGameModes[i]);
			mPMatchmakingItemVM.IsSelected = _defaultSelectedGameTypes.Contains(mPMatchmakingItemVM.Type);
			mPMatchmakingItemVM.OnSelectionChanged += GameModeOnSelectionChanged;
			mPMatchmakingItemVM.OnSetFocusItem += GameModeOnSetFocusItem;
			mPMatchmakingItemVM.OnRemoveFocus += GameModeOnRemoveFocus;
			((Collection<MPMatchmakingItemVM>)(object)QuickplayGameTypes).Add(mPMatchmakingItemVM);
		}
	}

	private void UpdateCustomGameTypeList()
	{
		MultiplayerGameType[] availableCustomGameModes = MultiplayerMain.GetAvailableCustomGameModes();
		for (int i = 0; i < availableCustomGameModes.Length; i++)
		{
			MPMatchmakingItemVM mPMatchmakingItemVM = new MPMatchmakingItemVM(availableCustomGameModes[i]);
			mPMatchmakingItemVM.IsSelected = _defaultSelectedGameTypes.Contains(mPMatchmakingItemVM.Type);
			mPMatchmakingItemVM.OnSelectionChanged += GameModeOnSelectionChanged;
			mPMatchmakingItemVM.OnSetFocusItem += GameModeOnSetFocusItem;
			mPMatchmakingItemVM.OnRemoveFocus += GameModeOnRemoveFocus;
			((Collection<MPMatchmakingItemVM>)(object)CustomGameTypes).Add(mPMatchmakingItemVM);
		}
	}

	public void OnPremadeGameEligibilityStatusReceived(bool isEligible)
	{
		_isEligibleForPremadeMatches = isEligible;
		IsPremadeGamesEnabled = _isEligibleForPremadeMatches && !IsFindingMatch && !HasUnofficialModulesLoaded;
	}

	public void OnSupportedFeaturesRefreshed(SupportedFeatures supportedFeatures)
	{
		IsCustomServerFeatureSupported = supportedFeatures.SupportsFeatures((Features)2);
		IsClansFeatureSupported = supportedFeatures.SupportsFeatures((Features)8);
	}
}
