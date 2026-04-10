using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

public class MissionScoreboardSideVM : ViewModel
{
	private readonly MissionScoreboardSide _missionScoreboardSide;

	private readonly Dictionary<MissionPeer, MissionScoreboardPlayerVM> _playersMap;

	private MissionScoreboardPlayerVM _bot;

	private Action<MissionScoreboardPlayerVM> _executeActivate;

	private const string _avatarHeaderId = "avatar";

	private readonly int _avatarHeaderIndex;

	private List<string> _irregularHeaderIDs = new List<string> { "name", "avatar", "score", "kill", "assist" };

	private MBBindingList<MissionScoreboardPlayerVM> _players;

	private MBBindingList<MissionScoreboardHeaderItemVM> _entryProperties;

	private MissionScoreboardPlayerSortControllerVM _playerSortController;

	private bool _isSingleSide;

	private bool _isSecondSide;

	private bool _showAttackerOrDefenderIcons;

	private bool _isAttacker;

	private int _roundsWon;

	private string _name;

	private string _cultureId;

	private string _teamColor;

	private string _playersText;

	private Color _cultureColor1;

	private Color _cultureColor2;

	[DataSourceProperty]
	public MBBindingList<MissionScoreboardPlayerVM> Players
	{
		get
		{
			return _players;
		}
		set
		{
			if (_players != value)
			{
				_players = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionScoreboardPlayerVM>>(value, "Players");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionScoreboardHeaderItemVM> EntryProperties
	{
		get
		{
			return _entryProperties;
		}
		set
		{
			if (value != _entryProperties)
			{
				_entryProperties = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionScoreboardHeaderItemVM>>(value, "EntryProperties");
			}
		}
	}

	[DataSourceProperty]
	public MissionScoreboardPlayerSortControllerVM PlayerSortController
	{
		get
		{
			return _playerSortController;
		}
		set
		{
			if (value != _playerSortController)
			{
				_playerSortController = value;
				((ViewModel)this).OnPropertyChangedWithValue<MissionScoreboardPlayerSortControllerVM>(value, "PlayerSortController");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSingleSide
	{
		get
		{
			return _isSingleSide;
		}
		set
		{
			if (value != _isSingleSide)
			{
				_isSingleSide = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSingleSide");
			}
		}
	}

	[DataSourceProperty]
	public Color CultureColor1
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cultureColor1;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _cultureColor1)
			{
				_cultureColor1 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CultureColor1");
			}
		}
	}

	[DataSourceProperty]
	public Color CultureColor2
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cultureColor2;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _cultureColor2)
			{
				_cultureColor2 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CultureColor2");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSecondSide
	{
		get
		{
			return _isSecondSide;
		}
		set
		{
			if (value != _isSecondSide)
			{
				_isSecondSide = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSecondSide");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowAttackerOrDefenderIcons
	{
		get
		{
			return _showAttackerOrDefenderIcons;
		}
		set
		{
			if (value != _showAttackerOrDefenderIcons)
			{
				_showAttackerOrDefenderIcons = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowAttackerOrDefenderIcons");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAttacker
	{
		get
		{
			return _isAttacker;
		}
		set
		{
			if (value != _isAttacker)
			{
				_isAttacker = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAttacker");
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

	[DataSourceProperty]
	public string PlayersText
	{
		get
		{
			return _playersText;
		}
		set
		{
			if (value != _playersText)
			{
				_playersText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PlayersText");
			}
		}
	}

	[DataSourceProperty]
	public string CultureId
	{
		get
		{
			return _cultureId;
		}
		set
		{
			if (value != _cultureId)
			{
				_cultureId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureId");
			}
		}
	}

	[DataSourceProperty]
	public int RoundsWon
	{
		get
		{
			return _roundsWon;
		}
		set
		{
			if (_roundsWon != value)
			{
				_roundsWon = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "RoundsWon");
			}
		}
	}

	[DataSourceProperty]
	public string TeamColor
	{
		get
		{
			return _teamColor;
		}
		set
		{
			if (value != _teamColor)
			{
				_teamColor = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TeamColor");
			}
		}
	}

	public MissionScoreboardSideVM(MissionScoreboardSide missionScoreboardSide, Action<MissionScoreboardPlayerVM> executeActivate, bool isSingleSide, bool isSecondSide, MultiplayerCultureColorInfo cultureColorInfo)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Invalid comparison between Unknown and I4
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Expected O, but got Unknown
		_executeActivate = executeActivate;
		_missionScoreboardSide = missionScoreboardSide;
		_playersMap = new Dictionary<MissionPeer, MissionScoreboardPlayerVM>();
		Players = new MBBindingList<MissionScoreboardPlayerVM>();
		PlayerSortController = new MissionScoreboardPlayerSortControllerVM(ref _players);
		_avatarHeaderIndex = Extensions.IndexOf<string>(missionScoreboardSide.GetHeaderIds(), "avatar");
		int score = missionScoreboardSide.GetScore((MissionPeer)null);
		string[] valuesOf = missionScoreboardSide.GetValuesOf((MissionPeer)null);
		string[] headerIds = missionScoreboardSide.GetHeaderIds();
		_bot = new MissionScoreboardPlayerVM(valuesOf, headerIds, score, _executeActivate);
		foreach (MissionPeer player in missionScoreboardSide.Players)
		{
			AddPlayer(player);
		}
		UpdateBotAttributes();
		UpdateRoundAttributes();
		IsSingleSide = isSingleSide;
		IsSecondSide = isSecondSide;
		BasicCultureObject culture = cultureColorInfo.Culture;
		CultureId = ((culture != null) ? ((MBObjectBase)culture).StringId : null) ?? string.Empty;
		CultureColor1 = cultureColorInfo.Color1;
		CultureColor2 = cultureColorInfo.Color2;
		BasicCultureObject culture2 = cultureColorInfo.Culture;
		TeamColor = "0x" + ((culture2 != null) ? culture2.Color2.ToString("X") : null);
		ShowAttackerOrDefenderIcons = Mission.Current.HasMissionBehavior<MissionMultiplayerSiegeClient>();
		IsAttacker = (int)missionScoreboardSide.Side == 1;
		((ViewModel)this).RefreshValues();
		NetworkCommunicator.OnPeerAveragePingUpdated += OnPeerPingUpdated;
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	public override void RefreshValues()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		((ViewModel)this).RefreshValues();
		BasicCultureObject val = MBObjectManager.Instance.GetObject<BasicCultureObject>(((int)_missionScoreboardSide.Side == 1) ? MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1) : MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
		if (IsSingleSide)
		{
			Name = MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1);
		}
		else
		{
			Name = ((object)val.Name).ToString();
		}
		EntryProperties = new MBBindingList<MissionScoreboardHeaderItemVM>();
		string[] headerIds = _missionScoreboardSide.GetHeaderIds();
		string[] headerNames = _missionScoreboardSide.GetHeaderNames();
		for (int i = 0; i < headerIds.Length; i++)
		{
			((Collection<MissionScoreboardHeaderItemVM>)(object)EntryProperties).Add(new MissionScoreboardHeaderItemVM(this, headerIds[i], headerNames[i], headerIds[i] == "avatar", _irregularHeaderIDs.Contains(headerIds[i])));
		}
		UpdatePlayersText();
		MissionScoreboardPlayerSortControllerVM playerSortController = PlayerSortController;
		if (playerSortController != null)
		{
			((ViewModel)playerSortController).RefreshValues();
		}
	}

	public void Tick(float dt)
	{
		foreach (MissionScoreboardPlayerVM item in (Collection<MissionScoreboardPlayerVM>)(object)Players)
		{
			item.Tick(dt);
		}
	}

	public override void OnFinalize()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		((ViewModel)this).OnFinalize();
		NetworkCommunicator.OnPeerAveragePingUpdated -= OnPeerPingUpdated;
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	public void UpdateRoundAttributes()
	{
		RoundsWon = _missionScoreboardSide.SideScore;
		SortPlayers();
	}

	public void UpdateBotAttributes()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		int num = (((int)_missionScoreboardSide.Side == 1) ? MultiplayerOptionsExtensions.GetIntValue((OptionType)18, (MultiplayerOptionsAccessMode)1) : MultiplayerOptionsExtensions.GetIntValue((OptionType)19, (MultiplayerOptionsAccessMode)1));
		if (num > 0)
		{
			int score = _missionScoreboardSide.GetScore((MissionPeer)null);
			string[] valuesOf = _missionScoreboardSide.GetValuesOf((MissionPeer)null);
			_bot.UpdateAttributes(valuesOf, score);
			if (!((Collection<MissionScoreboardPlayerVM>)(object)Players).Contains(_bot))
			{
				((Collection<MissionScoreboardPlayerVM>)(object)Players).Add(_bot);
			}
		}
		else if (num == 0 && ((Collection<MissionScoreboardPlayerVM>)(object)Players).Contains(_bot))
		{
			((Collection<MissionScoreboardPlayerVM>)(object)Players).Remove(_bot);
		}
		SortPlayers();
	}

	public void UpdatePlayerAttributes(MissionPeer player)
	{
		if (_playersMap.ContainsKey(player))
		{
			int score = _missionScoreboardSide.GetScore(player);
			string[] valuesOf = _missionScoreboardSide.GetValuesOf(player);
			_playersMap[player].UpdateAttributes(valuesOf, score);
		}
		SortPlayers();
	}

	public void RemovePlayer(MissionPeer peer)
	{
		if (_playersMap.TryGetValue(peer, out var value))
		{
			((Collection<MissionScoreboardPlayerVM>)(object)Players).Remove(value);
			_playersMap.Remove(peer);
		}
		else
		{
			Debug.FailedAssert("Trying to remove a player that is not registered", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Scoreboard\\MissionScoreboardSideVM.cs", "RemovePlayer", 167);
		}
		SortPlayers();
		UpdatePlayersText();
	}

	public void AddPlayer(MissionPeer peer)
	{
		if (!_playersMap.ContainsKey(peer))
		{
			int score = _missionScoreboardSide.GetScore(peer);
			string[] valuesOf = _missionScoreboardSide.GetValuesOf(peer);
			string[] headerIds = _missionScoreboardSide.GetHeaderIds();
			MissionScoreboardPlayerVM missionScoreboardPlayerVM = new MissionScoreboardPlayerVM(peer, valuesOf, headerIds, score, _executeActivate);
			_playersMap.Add(peer, missionScoreboardPlayerVM);
			((Collection<MissionScoreboardPlayerVM>)(object)Players).Add(missionScoreboardPlayerVM);
		}
		SortPlayers();
		UpdatePlayersText();
	}

	private void UpdatePlayersText()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		TextObject val = new TextObject("{=R28ac5ij}{NUMBER} Players", (Dictionary<string, object>)null);
		val.SetTextVariable("NUMBER", ((Collection<MissionScoreboardPlayerVM>)(object)Players).Count);
		PlayersText = ((object)val).ToString();
	}

	private void SortPlayers()
	{
		PlayerSortController.SortByCurrentState();
	}

	private void OnPeerPingUpdated(NetworkCommunicator peer)
	{
		MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(peer);
		if (component != null)
		{
			UpdatePlayerAttributes(component);
		}
	}

	private void OnManagedOptionChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType != 33)
		{
			return;
		}
		foreach (MissionScoreboardPlayerVM item in (Collection<MissionScoreboardPlayerVM>)(object)Players)
		{
			if (!item.IsBot)
			{
				item.RefreshAvatar();
			}
		}
	}
}
