using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Lobby;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;

public class MPCustomGameItemVM : ViewModel
{
	public const string PingTimeoutText = "-";

	private readonly Action<MPCustomGameItemVM> _onSelect;

	private readonly Action<MPCustomGameItemVM> _onJoin;

	private readonly Action<MPCustomGameItemVM> _onRequestActions;

	private readonly Action<MPCustomGameItemVM> _onToggleFavorite;

	private string _randomString;

	public static readonly string OfficialServerHostName = "TaleWorlds";

	private bool _isPasswordProtected;

	private bool _isFavorite;

	private bool _isClanMatchItem;

	private bool _isOfficialServer;

	private bool _isByOfficialServerProvider;

	private bool _isCommunityServer;

	private bool _isPingInfoAvailable;

	private bool _isSelected;

	private int _playerCount;

	private int _maxPlayerCount;

	private string _hostText;

	private string _nameText;

	private string _gameTypeText;

	private string _playerCountText;

	private string _pingText;

	private string _firstFactionName;

	private string _secondFactionName;

	private string _regionName;

	private string _premadeMatchTypeText;

	private BasicTooltipViewModel _loadedModulesHint;

	public GameServerEntry GameServerInfo { get; }

	public PremadeGameEntry PremadeGameInfo { get; }

	[DataSourceProperty]
	public bool IsPasswordProtected
	{
		get
		{
			return _isPasswordProtected;
		}
		set
		{
			if (value != _isPasswordProtected)
			{
				_isPasswordProtected = value;
				((ViewModel)this).OnPropertyChanged("IsPasswordProtected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFavorite
	{
		get
		{
			return _isFavorite;
		}
		set
		{
			if (value != _isFavorite)
			{
				_isFavorite = value;
				((ViewModel)this).OnPropertyChanged("IsFavorite");
			}
		}
	}

	[DataSourceProperty]
	public bool IsClanMatchItem
	{
		get
		{
			return _isClanMatchItem;
		}
		set
		{
			if (value != _isClanMatchItem)
			{
				_isClanMatchItem = value;
				((ViewModel)this).OnPropertyChanged("IsClanMatchItem");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOfficialServer
	{
		get
		{
			return _isOfficialServer;
		}
		set
		{
			if (value != _isOfficialServer)
			{
				_isOfficialServer = value;
				((ViewModel)this).OnPropertyChanged("IsOfficialServer");
			}
		}
	}

	[DataSourceProperty]
	public bool IsByOfficialServerProvider
	{
		get
		{
			return _isByOfficialServerProvider;
		}
		set
		{
			if (value != _isByOfficialServerProvider)
			{
				_isByOfficialServerProvider = value;
				((ViewModel)this).OnPropertyChanged("IsByOfficialServerProvider");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCommunityServer
	{
		get
		{
			return _isCommunityServer;
		}
		set
		{
			if (value != _isCommunityServer)
			{
				_isCommunityServer = value;
				((ViewModel)this).OnPropertyChanged("IsCommunityServer");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPingInfoAvailable
	{
		get
		{
			return _isPingInfoAvailable;
		}
		set
		{
			if (value != _isPingInfoAvailable)
			{
				_isPingInfoAvailable = value;
				((ViewModel)this).OnPropertyChanged("IsPingInfoAvailable");
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
				((ViewModel)this).OnPropertyChanged("IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public int PlayerCount
	{
		get
		{
			return _playerCount;
		}
		set
		{
			if (value != _playerCount)
			{
				_playerCount = value;
				((ViewModel)this).OnPropertyChanged("PlayerCount");
			}
		}
	}

	[DataSourceProperty]
	public int MaxPlayerCount
	{
		get
		{
			return _maxPlayerCount;
		}
		set
		{
			if (value != _maxPlayerCount)
			{
				_maxPlayerCount = value;
				((ViewModel)this).OnPropertyChanged("MaxPlayerCount");
			}
		}
	}

	[DataSourceProperty]
	public string HostText
	{
		get
		{
			return _hostText;
		}
		set
		{
			if (value != _hostText)
			{
				_hostText = value;
				((ViewModel)this).OnPropertyChanged("HostText");
			}
		}
	}

	[DataSourceProperty]
	public string NameText
	{
		get
		{
			return _nameText;
		}
		set
		{
			if (value != _nameText)
			{
				_nameText = value;
				((ViewModel)this).OnPropertyChanged("NameText");
			}
		}
	}

	[DataSourceProperty]
	public string GameTypeText
	{
		get
		{
			return _gameTypeText;
		}
		set
		{
			if (value != _gameTypeText)
			{
				_gameTypeText = value;
				((ViewModel)this).OnPropertyChanged("GameTypeText");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerCountText
	{
		get
		{
			return _playerCountText;
		}
		set
		{
			if (value != _playerCountText)
			{
				_playerCountText = value;
				((ViewModel)this).OnPropertyChanged("PlayerCountText");
			}
		}
	}

	[DataSourceProperty]
	public string PingText
	{
		get
		{
			return _pingText;
		}
		set
		{
			if (value != _pingText)
			{
				_pingText = value;
				((ViewModel)this).OnPropertyChanged("PingText");
			}
		}
	}

	[DataSourceProperty]
	public string FirstFactionName
	{
		get
		{
			return _firstFactionName;
		}
		set
		{
			if (value != _firstFactionName)
			{
				_firstFactionName = value;
				((ViewModel)this).OnPropertyChanged("FirstFactionName");
			}
		}
	}

	[DataSourceProperty]
	public string SecondFactionName
	{
		get
		{
			return _secondFactionName;
		}
		set
		{
			if (value != _secondFactionName)
			{
				_secondFactionName = value;
				((ViewModel)this).OnPropertyChanged("SecondFactionName");
			}
		}
	}

	[DataSourceProperty]
	public string RegionName
	{
		get
		{
			return _regionName;
		}
		set
		{
			if (value != _regionName)
			{
				_regionName = value;
				((ViewModel)this).OnPropertyChanged("RegionName");
			}
		}
	}

	[DataSourceProperty]
	public string PremadeMatchTypeText
	{
		get
		{
			return _premadeMatchTypeText;
		}
		set
		{
			if (value != _premadeMatchTypeText)
			{
				_premadeMatchTypeText = value;
				((ViewModel)this).OnPropertyChanged("PremadeMatchTypeText");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel LoadedModulesHint
	{
		get
		{
			return _loadedModulesHint;
		}
		set
		{
			if (value != _loadedModulesHint)
			{
				_loadedModulesHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<BasicTooltipViewModel>(value, "LoadedModulesHint");
			}
		}
	}

	public MPCustomGameItemVM(GameServerEntry gameServerInfo, Action<MPCustomGameItemVM> onSelect, Action<MPCustomGameItemVM> onJoin, Action<MPCustomGameItemVM> onRequestActions, Action<MPCustomGameItemVM> onToggleFavorite)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		_onSelect = onSelect;
		_onJoin = onJoin;
		_onRequestActions = onRequestActions;
		_onToggleFavorite = onToggleFavorite;
		GameServerInfo = gameServerInfo;
		string text = ((object)new TextObject("{=vBkrw5VV}Random", (Dictionary<string, object>)null)).ToString();
		_randomString = "-- " + text + " --";
		LoadedModulesHint = new BasicTooltipViewModel((Func<List<TooltipProperty>>)(() => GetLoadedModulesTooltipProperties()));
		UpdateGameServerInfo();
		UpdateIsFavorite();
	}

	public MPCustomGameItemVM(PremadeGameEntry premadeGameInfo, Action<MPCustomGameItemVM> onJoin)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		_onJoin = onJoin;
		PremadeGameInfo = premadeGameInfo;
		IsClanMatchItem = true;
		IsPingInfoAvailable = false;
		string text = ((object)new TextObject("{=vBkrw5VV}Random", (Dictionary<string, object>)null)).ToString();
		_randomString = "-- " + text + " --";
		UpdatePremadeGameInfo();
		UpdateIsFavorite();
	}

	private async void UpdateGameServerInfo()
	{
		IsPasswordProtected = GameServerInfo.PasswordProtected;
		PlayerCount = GameServerInfo.PlayerCount;
		MaxPlayerCount = GameServerInfo.MaxPlayerCount;
		NameText = GameServerInfo.ServerName;
		TextObject val = GameTexts.FindText("str_multiplayer_official_game_type_name", GameServerInfo.GameType);
		GameTypeText = (((object)val).ToString().StartsWith("ERROR: ") ? ((object)new TextObject("{=MT4b8H9h}Unknown", (Dictionary<string, object>)null)).ToString() : ((object)val).ToString());
		GameTexts.SetVariable("LEFT", PlayerCount);
		GameTexts.SetVariable("RIGHT", MaxPlayerCount);
		PlayerCountText = ((object)GameTexts.FindText("str_LEFT_over_RIGHT", (string)null)).ToString();
		IsOfficialServer = GameServerInfo.IsOfficial;
		IsByOfficialServerProvider = GameServerInfo.ByOfficialProvider;
		IsCommunityServer = !IsOfficialServer && !IsByOfficialServerProvider;
		HostText = GameServerInfo.HostName;
		IsPingInfoAvailable = MPCustomGameVM.IsPingInfoAvailable;
		await UpdatePingText();
	}

	private async Task UpdatePingText()
	{
		if (IsPingInfoAvailable)
		{
			long num = await NetworkMain.GameClient.GetPingToServer(GameServerInfo.Address);
			PingText = ((num < 0) ? "-" : num.ToString());
		}
		else
		{
			PingText = "-";
		}
	}

	private void UpdatePremadeGameInfo()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Invalid comparison between Unknown and I4
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		IsPasswordProtected = PremadeGameInfo.IsPasswordProtected;
		NameText = PremadeGameInfo.Name;
		string gameTypeText = (GameTypeText = ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", PremadeGameInfo.GameType)).ToString());
		GameTypeText = gameTypeText;
		RegionName = PremadeGameInfo.Region;
		FirstFactionName = ((PremadeGameInfo.FactionA == Parameters.RandomSelectionString) ? _randomString : PremadeGameInfo.FactionA);
		SecondFactionName = ((PremadeGameInfo.FactionB == Parameters.RandomSelectionString) ? _randomString : PremadeGameInfo.FactionB);
		HostText = OfficialServerHostName;
		IsOfficialServer = true;
		if ((int)PremadeGameInfo.PremadeGameType == 1)
		{
			PremadeMatchTypeText = ((object)new TextObject("{=YNkPy4ta}Clan Match", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)PremadeGameInfo.PremadeGameType == 0)
		{
			PremadeMatchTypeText = ((object)new TextObject("{=H5tiRTya}Practice", (Dictionary<string, object>)null)).ToString();
		}
	}

	private List<TooltipProperty> GetLoadedModulesTooltipProperties()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>();
		if (GameServerInfo != null)
		{
			if (GameServerInfo.LoadedModules.Count > 0)
			{
				list.Add(new TooltipProperty(string.Empty, ((object)new TextObject("{=JXyxj1J5}Modules", (Dictionary<string, object>)null)).ToString(), 1, false, (TooltipPropertyFlags)4096));
				string text = " " + ((object)new TextObject("{=oYS9sabI}(optional)", (Dictionary<string, object>)null)).ToString();
				foreach (ModuleInfoModel loadedModule in GameServerInfo.LoadedModules)
				{
					string text2 = loadedModule.Version;
					if (loadedModule.IsOptional)
					{
						text2 += text;
					}
					list.Add(new TooltipProperty(loadedModule.Name, text2, 0, false, (TooltipPropertyFlags)0));
				}
			}
			TextObject val = (GameServerInfo.AllowsOptionalModules ? new TextObject("{=BBmEESTT}This server allows optional modules.", (Dictionary<string, object>)null) : new TextObject("{=sEbeLmZP}This server does not allow optional modules.", (Dictionary<string, object>)null));
			list.Add(new TooltipProperty("", ((object)val).ToString(), -1, false, (TooltipPropertyFlags)0));
			if (IsCommunityServer)
			{
				list.Add(new TooltipProperty("", string.Empty, 0, false, (TooltipPropertyFlags)1024));
				TextObject val2 = new TextObject("{=W51HSyXy}Press {VIEW_OPTIONS_KEY} to view options", (Dictionary<string, object>)null);
				string text3 = ((object)HotKeyManager.GetCategory("MultiplayerHotkeyCategory").GetHotKey("PreviewCosmeticItem")).ToString();
				val2.SetTextVariable("VIEW_OPTIONS_KEY", GameKeyTextExtensions.GetHotKeyGameTextFromKeyID(Module.CurrentModule.GlobalTextManager, text3.ToLower()));
				list.Add(new TooltipProperty(string.Empty, ((object)val2).ToString(), -1, false, (TooltipPropertyFlags)0)
				{
					OnlyShowWhenNotExtended = true
				});
			}
		}
		return list;
	}

	public void UpdateIsFavorite()
	{
		bool isFavorite = false;
		if (GameServerInfo != null)
		{
			FavoriteServerData val = default(FavoriteServerData);
			isFavorite = MultiplayerLocalDataManager.Instance.FavoriteServers.TryGetServerData(GameServerInfo, ref val);
		}
		IsFavorite = isFavorite;
	}

	public void ExecuteSelect()
	{
		_onSelect?.Invoke(this);
	}

	public void ExecuteFavorite()
	{
		_onToggleFavorite?.Invoke(this);
	}

	public void ExecuteJoin()
	{
		_onJoin?.Invoke(this);
	}

	public void ExecuteViewHostOptions()
	{
		if (_onRequestActions != null)
		{
			_onRequestActions(this);
			MBInformationManager.HideInformations();
		}
	}
}
