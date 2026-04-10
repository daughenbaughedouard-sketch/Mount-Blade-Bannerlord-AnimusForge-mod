using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer.NetworkComponents;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Intermission;

public class MPIntermissionVM : ViewModel
{
	private bool _hasBaseNetworkComponentSet;

	private BaseNetworkComponent _baseNetworkComponent;

	private MultiplayerIntermissionState _currentIntermissionState;

	private readonly TextObject _voteLabelText = new TextObject("{=KOVHgkVq}Voting Ends In:", (Dictionary<string, object>)null);

	private readonly TextObject _nextGameLabelText = new TextObject("{=lX9Qx7Wo}Next Game Starts In:", (Dictionary<string, object>)null);

	private readonly TextObject _serverIdleLabelText = new TextObject("{=Rhcberxf}Awaiting Server", (Dictionary<string, object>)null);

	private readonly TextObject _matchFinishedText = new TextObject("{=RbazQjFt}Match is Finished", (Dictionary<string, object>)null);

	private readonly TextObject _returningToLobbyText = new TextObject("{=1UaxKbn6}Returning to the Lobby...", (Dictionary<string, object>)null);

	private MPIntermissionMapItemVM _votedMapItem;

	private MPIntermissionCultureItemVM _votedCultureItem;

	private string _connectedPlayersCountValueText;

	private string _maxNumPlayersValueText;

	private bool _isFactionAValid;

	private bool _isFactionBValid;

	private bool _isMissionTimerEnabled;

	private bool _isEndGameTimerEnabled;

	private bool _isNextMapInfoEnabled;

	private bool _isMapVoteEnabled;

	private bool _isCultureVoteEnabled;

	private bool _isPlayerCountEnabled;

	private string _nextMapId;

	private string _nextFactionACultureId;

	private string _nextFactionBCultureId;

	private string _nextGameStateTimerLabel;

	private string _nextGameStateTimerValue;

	private string _playersLabel;

	private string _mapVoteText;

	private string _cultureVoteText;

	private string _serverName;

	private string _welcomeMessage;

	private string _nextGameType;

	private string _nextMapName;

	private Color _nextFactionACultureColor1;

	private Color _nextFactionACultureColor2;

	private Color _nextFactionBCultureColor1;

	private Color _nextFactionBCultureColor2;

	private string _quitText;

	private MBBindingList<MPIntermissionMapItemVM> _availableMaps;

	private MBBindingList<MPIntermissionCultureItemVM> _availableCultures;

	[DataSourceProperty]
	public string ConnectedPlayersCountValueText
	{
		get
		{
			return _connectedPlayersCountValueText;
		}
		set
		{
			if (value != _connectedPlayersCountValueText)
			{
				_connectedPlayersCountValueText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ConnectedPlayersCountValueText");
			}
		}
	}

	[DataSourceProperty]
	public string MaxNumPlayersValueText
	{
		get
		{
			return _maxNumPlayersValueText;
		}
		set
		{
			if (value != _maxNumPlayersValueText)
			{
				_maxNumPlayersValueText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MaxNumPlayersValueText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFactionAValid
	{
		get
		{
			return _isFactionAValid;
		}
		set
		{
			if (value != _isFactionAValid)
			{
				_isFactionAValid = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFactionAValid");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFactionBValid
	{
		get
		{
			return _isFactionBValid;
		}
		set
		{
			if (value != _isFactionBValid)
			{
				_isFactionBValid = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFactionBValid");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMissionTimerEnabled
	{
		get
		{
			return _isMissionTimerEnabled;
		}
		set
		{
			if (value != _isMissionTimerEnabled)
			{
				_isMissionTimerEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMissionTimerEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEndGameTimerEnabled
	{
		get
		{
			return _isEndGameTimerEnabled;
		}
		set
		{
			if (value != _isEndGameTimerEnabled)
			{
				_isEndGameTimerEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEndGameTimerEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNextMapInfoEnabled
	{
		get
		{
			return _isNextMapInfoEnabled;
		}
		set
		{
			if (value != _isNextMapInfoEnabled)
			{
				_isNextMapInfoEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsNextMapInfoEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMapVoteEnabled
	{
		get
		{
			return _isMapVoteEnabled;
		}
		set
		{
			if (value != _isMapVoteEnabled)
			{
				_isMapVoteEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMapVoteEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCultureVoteEnabled
	{
		get
		{
			return _isCultureVoteEnabled;
		}
		set
		{
			if (value != _isCultureVoteEnabled)
			{
				_isCultureVoteEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCultureVoteEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerCountEnabled
	{
		get
		{
			return _isPlayerCountEnabled;
		}
		set
		{
			if (value != _isPlayerCountEnabled)
			{
				_isPlayerCountEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerCountEnabled");
			}
		}
	}

	[DataSourceProperty]
	public string NextMapID
	{
		get
		{
			return _nextMapId;
		}
		set
		{
			if (value != _nextMapId)
			{
				_nextMapId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextMapID");
			}
		}
	}

	[DataSourceProperty]
	public string NextFactionACultureID
	{
		get
		{
			return _nextFactionACultureId;
		}
		set
		{
			if (value != _nextFactionACultureId)
			{
				_nextFactionACultureId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextFactionACultureID");
			}
		}
	}

	[DataSourceProperty]
	public Color NextFactionACultureColor1
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _nextFactionACultureColor1;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _nextFactionACultureColor1)
			{
				_nextFactionACultureColor1 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NextFactionACultureColor1");
			}
		}
	}

	[DataSourceProperty]
	public Color NextFactionACultureColor2
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _nextFactionACultureColor2;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _nextFactionACultureColor2)
			{
				_nextFactionACultureColor2 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NextFactionACultureColor2");
			}
		}
	}

	[DataSourceProperty]
	public string NextFactionBCultureID
	{
		get
		{
			return _nextFactionBCultureId;
		}
		set
		{
			if (value != _nextFactionBCultureId)
			{
				_nextFactionBCultureId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextFactionBCultureID");
			}
		}
	}

	[DataSourceProperty]
	public Color NextFactionBCultureColor1
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _nextFactionBCultureColor1;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _nextFactionBCultureColor1)
			{
				_nextFactionBCultureColor1 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NextFactionBCultureColor1");
			}
		}
	}

	[DataSourceProperty]
	public Color NextFactionBCultureColor2
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _nextFactionBCultureColor2;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _nextFactionBCultureColor2)
			{
				_nextFactionBCultureColor2 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NextFactionBCultureColor2");
			}
		}
	}

	[DataSourceProperty]
	public string PlayersLabel
	{
		get
		{
			return _playersLabel;
		}
		set
		{
			if (value != _playersLabel)
			{
				_playersLabel = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PlayersLabel");
			}
		}
	}

	[DataSourceProperty]
	public string MapVoteText
	{
		get
		{
			return _mapVoteText;
		}
		set
		{
			if (value != _mapVoteText)
			{
				_mapVoteText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MapVoteText");
			}
		}
	}

	[DataSourceProperty]
	public string CultureVoteText
	{
		get
		{
			return _cultureVoteText;
		}
		set
		{
			if (value != _cultureVoteText)
			{
				_cultureVoteText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureVoteText");
			}
		}
	}

	[DataSourceProperty]
	public string NextGameStateTimerLabel
	{
		get
		{
			return _nextGameStateTimerLabel;
		}
		set
		{
			if (value != _nextGameStateTimerLabel)
			{
				_nextGameStateTimerLabel = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextGameStateTimerLabel");
			}
		}
	}

	[DataSourceProperty]
	public string NextGameStateTimerValue
	{
		get
		{
			return _nextGameStateTimerValue;
		}
		set
		{
			if (value != _nextGameStateTimerValue)
			{
				_nextGameStateTimerValue = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextGameStateTimerValue");
			}
		}
	}

	[DataSourceProperty]
	public string WelcomeMessage
	{
		get
		{
			return _welcomeMessage;
		}
		set
		{
			if (value != _welcomeMessage)
			{
				_welcomeMessage = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "WelcomeMessage");
			}
		}
	}

	[DataSourceProperty]
	public string ServerName
	{
		get
		{
			return _serverName;
		}
		set
		{
			if (value != _serverName)
			{
				_serverName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ServerName");
			}
		}
	}

	[DataSourceProperty]
	public string NextGameType
	{
		get
		{
			return _nextGameType;
		}
		set
		{
			if (value != _nextGameType)
			{
				_nextGameType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextGameType");
			}
		}
	}

	[DataSourceProperty]
	public string NextMapName
	{
		get
		{
			return _nextMapName;
		}
		set
		{
			if (value != _nextMapName)
			{
				_nextMapName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextMapName");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPIntermissionMapItemVM> AvailableMaps
	{
		get
		{
			return _availableMaps;
		}
		set
		{
			if (value != _availableMaps)
			{
				_availableMaps = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPIntermissionMapItemVM>>(value, "AvailableMaps");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPIntermissionCultureItemVM> AvailableCultures
	{
		get
		{
			return _availableCultures;
		}
		set
		{
			if (value != _availableCultures)
			{
				_availableCultures = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPIntermissionCultureItemVM>>(value, "AvailableCultures");
			}
		}
	}

	[DataSourceProperty]
	public string QuitText
	{
		get
		{
			return _quitText;
		}
		set
		{
			if (value != _quitText)
			{
				_quitText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "QuitText");
			}
		}
	}

	public MPIntermissionVM()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		AvailableMaps = new MBBindingList<MPIntermissionMapItemVM>();
		AvailableCultures = new MBBindingList<MPIntermissionCultureItemVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		QuitText = ((object)new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null)).ToString();
		PlayersLabel = ((object)new TextObject("{=RfXJdNye}Players", (Dictionary<string, object>)null)).ToString();
		MapVoteText = ((object)new TextObject("{=DraJ6bxq}Vote for the Next Map", (Dictionary<string, object>)null)).ToString();
		CultureVoteText = ((object)new TextObject("{=oF27vprQ}Vote for the Next Culture", (Dictionary<string, object>)null)).ToString();
	}

	public void Tick()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasBaseNetworkComponentSet)
		{
			_baseNetworkComponent = GameNetwork.GetNetworkComponent<BaseNetworkComponent>();
			if (_baseNetworkComponent != null)
			{
				_hasBaseNetworkComponentSet = true;
				BaseNetworkComponent baseNetworkComponent = _baseNetworkComponent;
				baseNetworkComponent.OnIntermissionStateUpdated = (Action)Delegate.Combine(baseNetworkComponent.OnIntermissionStateUpdated, new Action(OnIntermissionStateUpdated));
			}
		}
		else if ((int)_baseNetworkComponent.ClientIntermissionState == 0)
		{
			NextGameStateTimerLabel = ((object)_serverIdleLabelText).ToString();
			NextGameStateTimerValue = string.Empty;
			IsMissionTimerEnabled = false;
			IsEndGameTimerEnabled = false;
			IsNextMapInfoEnabled = false;
			IsMapVoteEnabled = false;
			IsCultureVoteEnabled = false;
			IsPlayerCountEnabled = false;
		}
	}

	public override void OnFinalize()
	{
		if (_baseNetworkComponent != null)
		{
			BaseNetworkComponent baseNetworkComponent = _baseNetworkComponent;
			baseNetworkComponent.OnIntermissionStateUpdated = (Action)Delegate.Remove(baseNetworkComponent.OnIntermissionStateUpdated, new Action(OnIntermissionStateUpdated));
		}
		MultiplayerIntermissionVotingManager.Instance.ClearItems();
	}

	private void OnIntermissionStateUpdated()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Invalid comparison between Unknown and I4
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Invalid comparison between Unknown and I4
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Invalid comparison between Unknown and I4
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Invalid comparison between Unknown and I4
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0581: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0593: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fb: Invalid comparison between Unknown and I4
		//IL_062d: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		_currentIntermissionState = _baseNetworkComponent.ClientIntermissionState;
		bool flag = true;
		if ((int)_currentIntermissionState == 1)
		{
			int num = (int)_baseNetworkComponent.CurrentIntermissionTimer;
			NextGameStateTimerLabel = ((object)_voteLabelText).ToString();
			NextGameStateTimerValue = num.ToString();
			IsMissionTimerEnabled = true;
			IsEndGameTimerEnabled = false;
			IsNextMapInfoEnabled = false;
			IsCultureVoteEnabled = false;
			IsPlayerCountEnabled = true;
			flag = false;
			List<IntermissionVoteItem> mapVoteItems = MultiplayerIntermissionVotingManager.Instance.MapVoteItems;
			if (mapVoteItems.Count > 0)
			{
				IsMapVoteEnabled = true;
				foreach (IntermissionVoteItem mapItem in mapVoteItems)
				{
					if (((IEnumerable<MPIntermissionMapItemVM>)AvailableMaps).FirstOrDefault((MPIntermissionMapItemVM m) => m.MapID == mapItem.Id) == null)
					{
						((Collection<MPIntermissionMapItemVM>)(object)AvailableMaps).Add(new MPIntermissionMapItemVM(mapItem.Id, OnPlayerVotedForMap));
					}
					int voteCount = mapItem.VoteCount;
					((IEnumerable<MPIntermissionMapItemVM>)AvailableMaps).First((MPIntermissionMapItemVM m) => m.MapID == mapItem.Id).Votes = voteCount;
				}
			}
		}
		if ((int)_baseNetworkComponent.ClientIntermissionState == 2)
		{
			int num2 = (int)_baseNetworkComponent.CurrentIntermissionTimer;
			NextGameStateTimerLabel = ((object)_voteLabelText).ToString();
			NextGameStateTimerValue = num2.ToString();
			IsMissionTimerEnabled = true;
			IsEndGameTimerEnabled = false;
			IsNextMapInfoEnabled = false;
			IsMapVoteEnabled = false;
			IsPlayerCountEnabled = true;
			flag = false;
			List<IntermissionVoteItem> cultureVoteItems = MultiplayerIntermissionVotingManager.Instance.CultureVoteItems;
			if (cultureVoteItems.Count > 0)
			{
				IsCultureVoteEnabled = true;
				foreach (IntermissionVoteItem cultureItem in cultureVoteItems)
				{
					if (((IEnumerable<MPIntermissionCultureItemVM>)AvailableCultures).FirstOrDefault((MPIntermissionCultureItemVM c) => c.CultureCode == cultureItem.Id) == null)
					{
						((Collection<MPIntermissionCultureItemVM>)(object)AvailableCultures).Add(new MPIntermissionCultureItemVM(cultureItem.Id, OnPlayerVotedForCulture));
					}
					int voteCount2 = cultureItem.VoteCount;
					((IEnumerable<MPIntermissionCultureItemVM>)AvailableCultures).FirstOrDefault((MPIntermissionCultureItemVM c) => c.CultureCode == cultureItem.Id).Votes = voteCount2;
				}
			}
			string nextFactionACultureID = default(string);
			MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)14, (MultiplayerOptionsAccessMode)1).GetValue(ref nextFactionACultureID);
			string nextFactionBCultureID = default(string);
			MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)15, (MultiplayerOptionsAccessMode)1).GetValue(ref nextFactionBCultureID);
			NextFactionACultureID = nextFactionACultureID;
			NextFactionBCultureID = nextFactionBCultureID;
			BasicCultureObject obj = (string.IsNullOrEmpty(NextFactionACultureID) ? null : MBObjectManager.Instance.GetObject<BasicCultureObject>(NextFactionACultureID));
			BasicCultureObject val = (string.IsNullOrEmpty(NextFactionACultureID) ? null : MBObjectManager.Instance.GetObject<BasicCultureObject>(NextFactionBCultureID));
			MultiplayerBattleColors val2 = MultiplayerBattleColors.CreateWith(obj, val);
			NextFactionACultureColor1 = val2.AttackerColors.Color1;
			NextFactionACultureColor2 = val2.AttackerColors.Color2;
			NextFactionBCultureColor1 = val2.DefenderColors.Color1;
			NextFactionBCultureColor2 = val2.DefenderColors.Color2;
		}
		if ((int)_currentIntermissionState == 3)
		{
			int num3 = (int)_baseNetworkComponent.CurrentIntermissionTimer;
			NextGameStateTimerLabel = ((object)_nextGameLabelText).ToString();
			NextGameStateTimerValue = num3.ToString();
			IsMissionTimerEnabled = true;
			IsEndGameTimerEnabled = false;
			IsNextMapInfoEnabled = true;
			IsMapVoteEnabled = false;
			IsCultureVoteEnabled = false;
			IsPlayerCountEnabled = true;
			flag = true;
			((Collection<MPIntermissionMapItemVM>)(object)AvailableMaps).Clear();
			((Collection<MPIntermissionCultureItemVM>)(object)AvailableCultures).Clear();
			MultiplayerIntermissionVotingManager.Instance.ClearVotes();
			_votedMapItem = null;
			_votedCultureItem = null;
		}
		if ((int)_currentIntermissionState == 4)
		{
			TextObject val3 = GameTexts.FindText("str_string_newline_string", (string)null);
			val3.SetTextVariable("STR1", ((object)_matchFinishedText).ToString());
			val3.SetTextVariable("STR2", ((object)_returningToLobbyText).ToString());
			NextGameStateTimerLabel = ((object)val3).ToString();
			NextGameStateTimerValue = string.Empty;
			IsMissionTimerEnabled = false;
			IsEndGameTimerEnabled = false;
			IsNextMapInfoEnabled = false;
			IsMapVoteEnabled = false;
			IsCultureVoteEnabled = false;
			IsPlayerCountEnabled = false;
			flag = false;
		}
		string text = default(string);
		MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)13, (MultiplayerOptionsAccessMode)1).GetValue(ref text);
		NextMapID = (IsEndGameTimerEnabled ? string.Empty : text);
		TextObject val4 = default(TextObject);
		string text2 = ((!GameTexts.TryGetText("str_multiplayer_scene_name", ref val4, text)) ? text : ((object)val4).ToString());
		NextMapName = (IsEndGameTimerEnabled ? string.Empty : text2);
		if (flag)
		{
			string text3 = default(string);
			MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)14, (MultiplayerOptionsAccessMode)1).GetValue(ref text3);
			IsFactionAValid = !IsEndGameTimerEnabled && !string.IsNullOrEmpty(text3) && (int)_currentIntermissionState != 1;
			NextFactionACultureID = (IsEndGameTimerEnabled ? string.Empty : text3);
			BasicCultureObject obj2 = (string.IsNullOrEmpty(NextFactionACultureID) ? null : MBObjectManager.Instance.GetObject<BasicCultureObject>(NextFactionACultureID));
			BasicCultureObject val5 = (string.IsNullOrEmpty(NextFactionACultureID) ? null : MBObjectManager.Instance.GetObject<BasicCultureObject>(NextFactionBCultureID));
			MultiplayerBattleColors val6 = MultiplayerBattleColors.CreateWith(obj2, val5);
			NextFactionACultureColor1 = val6.AttackerColors.Color1;
			NextFactionACultureColor2 = val6.AttackerColors.Color2;
			NextFactionBCultureColor1 = val6.DefenderColors.Color1;
			NextFactionBCultureColor2 = val6.DefenderColors.Color2;
			if (!string.IsNullOrEmpty(NextFactionACultureID))
			{
				NextFactionACultureColor1 = val6.AttackerColors.Color1;
				NextFactionACultureColor2 = val6.AttackerColors.Color2;
			}
			string text4 = default(string);
			MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)15, (MultiplayerOptionsAccessMode)1).GetValue(ref text4);
			IsFactionBValid = !IsEndGameTimerEnabled && !string.IsNullOrEmpty(text4) && (int)_currentIntermissionState != 1;
			NextFactionBCultureID = (IsEndGameTimerEnabled ? string.Empty : text4);
			if (!string.IsNullOrEmpty(NextFactionBCultureID))
			{
				NextFactionBCultureColor1 = val6.DefenderColors.Color1;
				NextFactionBCultureColor2 = val6.DefenderColors.Color2;
			}
		}
		else
		{
			IsFactionAValid = false;
			IsFactionBValid = false;
		}
		string serverName = default(string);
		MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)0, (MultiplayerOptionsAccessMode)1).GetValue(ref serverName);
		ServerName = serverName;
		string text5 = default(string);
		MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)11, (MultiplayerOptionsAccessMode)1).GetValue(ref text5);
		NextGameType = (IsEndGameTimerEnabled ? string.Empty : ((object)GameTexts.FindText("str_multiplayer_game_type", text5)).ToString());
		string text6 = default(string);
		MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)1, (MultiplayerOptionsAccessMode)1).GetValue(ref text6);
		WelcomeMessage = (IsEndGameTimerEnabled ? string.Empty : text6);
		int num4 = default(int);
		MultiplayerOptions.Instance.GetOptionFromOptionType((OptionType)16, (MultiplayerOptionsAccessMode)1).GetValue(ref num4);
		MaxNumPlayersValueText = num4.ToString();
		ConnectedPlayersCountValueText = GameNetwork.NetworkPeers.Count.ToString();
	}

	public void ExecuteQuitServer()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		LobbyClient gameClient = NetworkMain.GameClient;
		if ((int)gameClient.CurrentState == 16)
		{
			gameClient.QuitFromCustomGame();
		}
		MultiplayerIntermissionVotingManager.Instance.ClearItems();
	}

	private void OnPlayerVotedForMap(MPIntermissionMapItemVM mapItem)
	{
		if (_votedMapItem != null)
		{
			_baseNetworkComponent.IntermissionCastVote(_votedMapItem.MapID, -1);
			_votedMapItem.IsSelected = false;
			_votedMapItem.Votes--;
		}
		_baseNetworkComponent.IntermissionCastVote(mapItem.MapID, 1);
		_votedMapItem = mapItem;
		_votedMapItem.IsSelected = true;
		_votedMapItem.Votes++;
	}

	private void OnPlayerVotedForCulture(MPIntermissionCultureItemVM cultureItem)
	{
		if (_votedCultureItem != null)
		{
			_baseNetworkComponent.IntermissionCastVote(_votedCultureItem.CultureCode, -1);
			_votedCultureItem.IsSelected = false;
			_votedCultureItem.Votes--;
		}
		_baseNetworkComponent.IntermissionCastVote(cultureItem.CultureCode, 1);
		_votedCultureItem = cultureItem;
		_votedCultureItem.IsSelected = true;
		_votedCultureItem.Votes++;
	}
}
