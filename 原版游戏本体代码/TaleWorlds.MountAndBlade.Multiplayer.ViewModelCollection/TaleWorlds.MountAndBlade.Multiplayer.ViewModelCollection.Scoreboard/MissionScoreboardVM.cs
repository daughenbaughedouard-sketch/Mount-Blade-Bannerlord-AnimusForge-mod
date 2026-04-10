using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

public class MissionScoreboardVM : ViewModel
{
	private const float AttributeRefreshDuration = 1f;

	private ChatBox _chatBox;

	private const float PermissionCheckDuration = 45f;

	private readonly Dictionary<BattleSideEnum, MissionScoreboardSideVM> _missionSides;

	private readonly MissionScoreboardComponent _missionScoreboardComponent;

	private readonly MultiplayerPollComponent _missionPollComponent;

	private VoiceChatHandler _voiceChatHandler;

	private MultiplayerPermissionHandler _permissionHandler;

	private readonly Mission _mission;

	private float _attributeRefreshTimeElapsed;

	private bool _hasMutedAll;

	private bool _canStartKickPolls;

	private TextObject _muteAllText = new TextObject("{=AZSbwcG5}Mute All", (Dictionary<string, object>)null);

	private TextObject _unmuteAllText = new TextObject("{=SzRVIPeZ}Unmute All", (Dictionary<string, object>)null);

	private bool _isActive;

	private InputKeyItemVM _showMouseKey;

	private MPEndOfBattleVM _endOfBattle;

	private MBBindingList<MissionScoreboardSideVM> _sides;

	private MBBindingList<StringPairItemWithActionVM> _playerActionList;

	private string _spectators;

	private string _missionName;

	private string _gameModeText;

	private string _mapName;

	private string _serverName;

	private bool _isBotsEnabled;

	private bool _isSingleSide;

	private bool _isInitalizationOver;

	private bool _isUpdateOver;

	private bool _isMouseEnabled;

	private bool _isPlayerActionsActive;

	private string _toggleMuteText;

	private BattleSideEnum AllySide
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			BattleSideEnum result = (BattleSideEnum)(-1);
			if (GameNetwork.IsMyPeerReady)
			{
				MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
				if (component != null && component.Team != null)
				{
					result = component.Team.Side;
				}
			}
			return result;
		}
	}

	private BattleSideEnum EnemySide
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			BattleSideEnum allySide = AllySide;
			if ((int)allySide != 0)
			{
				if ((int)allySide != 1)
				{
					Debug.FailedAssert("Ally side must be either Attacker or Defender", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Scoreboard\\MissionScoreboardVM.cs", "EnemySide", 559);
					return (BattleSideEnum)(-1);
				}
				return (BattleSideEnum)0;
			}
			return (BattleSideEnum)1;
		}
	}

	[DataSourceProperty]
	public MPEndOfBattleVM EndOfBattle
	{
		get
		{
			return _endOfBattle;
		}
		set
		{
			if (value != _endOfBattle)
			{
				_endOfBattle = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPEndOfBattleVM>(value, "EndOfBattle");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemWithActionVM> PlayerActionList
	{
		get
		{
			return _playerActionList;
		}
		set
		{
			if (value != _playerActionList)
			{
				_playerActionList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemWithActionVM>>(value, "PlayerActionList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionScoreboardSideVM> Sides
	{
		get
		{
			return _sides;
		}
		set
		{
			if (value != _sides)
			{
				_sides = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionScoreboardSideVM>>(value, "Sides");
			}
		}
	}

	[DataSourceProperty]
	public bool IsUpdateOver
	{
		get
		{
			return _isUpdateOver;
		}
		set
		{
			_isUpdateOver = value;
			((ViewModel)this).OnPropertyChangedWithValue(value, "IsUpdateOver");
		}
	}

	[DataSourceProperty]
	public bool IsInitalizationOver
	{
		get
		{
			return _isInitalizationOver;
		}
		set
		{
			if (value != _isInitalizationOver)
			{
				_isInitalizationOver = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInitalizationOver");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMouseEnabled
	{
		get
		{
			return _isMouseEnabled;
		}
		set
		{
			if (value != _isMouseEnabled)
			{
				_isMouseEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMouseEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			if (value != _isActive)
			{
				_isActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerActionsActive
	{
		get
		{
			return _isPlayerActionsActive;
		}
		set
		{
			if (value != _isPlayerActionsActive)
			{
				_isPlayerActionsActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerActionsActive");
			}
		}
	}

	[DataSourceProperty]
	public string Spectators
	{
		get
		{
			return _spectators;
		}
		set
		{
			if (value != _spectators)
			{
				_spectators = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Spectators");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM ShowMouseKey
	{
		get
		{
			return _showMouseKey;
		}
		set
		{
			if (value != _showMouseKey)
			{
				_showMouseKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "ShowMouseKey");
			}
		}
	}

	[DataSourceProperty]
	public string MissionName
	{
		get
		{
			return _missionName;
		}
		set
		{
			if (value != _missionName)
			{
				_missionName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MissionName");
			}
		}
	}

	[DataSourceProperty]
	public string GameModeText
	{
		get
		{
			return _gameModeText;
		}
		set
		{
			if (value != _gameModeText)
			{
				_gameModeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameModeText");
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
	public bool IsBotsEnabled
	{
		get
		{
			return _isBotsEnabled;
		}
		set
		{
			if (value != _isBotsEnabled)
			{
				_isBotsEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBotsEnabled");
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
	public string ToggleMuteText
	{
		get
		{
			return _toggleMuteText;
		}
		set
		{
			if (value != _toggleMuteText)
			{
				_toggleMuteText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ToggleMuteText");
			}
		}
	}

	public MissionScoreboardVM(bool isSingleTeam, Mission mission)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Invalid comparison between Unknown and I4
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Invalid comparison between Unknown and I4
		_chatBox = Game.Current.GetGameHandler<ChatBox>();
		_chatBox.OnPlayerMuteChanged += new PlayerMutedDelegate(OnPlayerMuteChanged);
		_mission = mission;
		MissionLobbyComponent missionBehavior = mission.GetMissionBehavior<MissionLobbyComponent>();
		_missionScoreboardComponent = mission.GetMissionBehavior<MissionScoreboardComponent>();
		_voiceChatHandler = _mission.GetMissionBehavior<VoiceChatHandler>();
		_permissionHandler = GameNetwork.GetNetworkComponent<MultiplayerPermissionHandler>();
		_canStartKickPolls = MultiplayerOptionsExtensions.GetBoolValue((OptionType)5, (MultiplayerOptionsAccessMode)1);
		if (_canStartKickPolls)
		{
			_missionPollComponent = mission.GetMissionBehavior<MultiplayerPollComponent>();
		}
		EndOfBattle = new MPEndOfBattleVM(mission, _missionScoreboardComponent, isSingleTeam);
		PlayerActionList = new MBBindingList<StringPairItemWithActionVM>();
		Sides = new MBBindingList<MissionScoreboardSideVM>();
		_missionSides = new Dictionary<BattleSideEnum, MissionScoreboardSideVM>();
		IsSingleSide = isSingleTeam;
		InitSides();
		GameKey gameKey = HotKeyManager.GetCategory("ScoreboardHotKeyCategory").GetGameKey(35);
		ShowMouseKey = InputKeyItemVM.CreateFromGameKey(gameKey, false);
		MissionName = "";
		IsBotsEnabled = (int)missionBehavior.MissionType == 4 || (int)missionBehavior.MissionType == 3;
		RegisterEvents();
		((ViewModel)this).RefreshValues();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		UnregisterEvents();
		foreach (MissionScoreboardSideVM item in (Collection<MissionScoreboardSideVM>)(object)Sides)
		{
			((ViewModel)item).OnFinalize();
		}
	}

	private void RegisterEvents()
	{
		if (_voiceChatHandler != null)
		{
			_voiceChatHandler.OnPeerMuteStatusUpdated += OnPeerMuteStatusUpdated;
		}
		if (_permissionHandler != null)
		{
			_permissionHandler.OnPlayerPlatformMuteChanged += OnPlayerPlatformMuteChanged;
		}
		_missionScoreboardComponent.OnPlayerSideChanged += OnPlayerSideChanged;
		_missionScoreboardComponent.OnPlayerPropertiesChanged += OnPlayerPropertiesChanged;
		_missionScoreboardComponent.OnBotPropertiesChanged += OnBotPropertiesChanged;
		_missionScoreboardComponent.OnRoundPropertiesChanged += OnRoundPropertiesChanged;
		_missionScoreboardComponent.OnScoreboardInitialized += OnScoreboardInitialized;
		_missionScoreboardComponent.OnMVPSelected += OnMVPSelected;
	}

	private void UnregisterEvents()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		_missionScoreboardComponent.OnPlayerSideChanged -= OnPlayerSideChanged;
		_missionScoreboardComponent.OnPlayerPropertiesChanged -= OnPlayerPropertiesChanged;
		_missionScoreboardComponent.OnBotPropertiesChanged -= OnBotPropertiesChanged;
		_missionScoreboardComponent.OnRoundPropertiesChanged -= OnRoundPropertiesChanged;
		_missionScoreboardComponent.OnScoreboardInitialized -= OnScoreboardInitialized;
		_missionScoreboardComponent.OnMVPSelected -= OnMVPSelected;
		_chatBox.OnPlayerMuteChanged -= new PlayerMutedDelegate(OnPlayerMuteChanged);
		if (_voiceChatHandler != null)
		{
			_voiceChatHandler.OnPeerMuteStatusUpdated -= OnPeerMuteStatusUpdated;
		}
		if (_permissionHandler != null)
		{
			_permissionHandler.OnPlayerPlatformMuteChanged -= OnPlayerPlatformMuteChanged;
		}
	}

	private void OnPlayerPlatformMuteChanged(PlayerId playerId, bool isPlayerMuted)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		foreach (MissionScoreboardSideVM item in (Collection<MissionScoreboardSideVM>)(object)Sides)
		{
			foreach (MissionScoreboardPlayerVM item2 in (Collection<MissionScoreboardPlayerVM>)(object)item.Players)
			{
				if (!item2.IsBot)
				{
					PlayerId id = ((PeerComponent)item2.Peer).Peer.Id;
					if (((PlayerId)(ref id)).Equals(playerId))
					{
						item2.UpdateIsMuted();
						return;
					}
				}
			}
		}
	}

	private void OnPlayerMuteChanged(PlayerId playerId, bool isMuted)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		foreach (MissionScoreboardSideVM item in (Collection<MissionScoreboardSideVM>)(object)Sides)
		{
			foreach (MissionScoreboardPlayerVM item2 in (Collection<MissionScoreboardPlayerVM>)(object)item.Players)
			{
				if (!item2.IsBot)
				{
					PlayerId id = ((PeerComponent)item2.Peer).Peer.Id;
					if (((PlayerId)(ref id)).Equals(playerId))
					{
						item2.UpdateIsMuted();
						return;
					}
				}
			}
		}
	}

	public override void RefreshValues()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		((ViewModel)this).RefreshValues();
		MissionLobbyComponent missionBehavior = _mission.GetMissionBehavior<MissionLobbyComponent>();
		UpdateToggleMuteText();
		GameModeText = ((object)GameTexts.FindText("str_multiplayer_game_type", ((object)missionBehavior.MissionType/*cast due to .constrained prefix*/).ToString())).ToString().ToLower();
		((ViewModel)EndOfBattle).RefreshValues();
		Sides.ApplyActionOnAllItems((Action<MissionScoreboardSideVM>)delegate(MissionScoreboardSideVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		TextObject val = default(TextObject);
		if (GameTexts.TryGetText("str_multiplayer_scene_name", ref val, ((MissionBehavior)missionBehavior).Mission.SceneName))
		{
			MapName = ((object)val).ToString();
		}
		else
		{
			MapName = ((MissionBehavior)missionBehavior).Mission.SceneName;
		}
		ServerName = MultiplayerOptionsExtensions.GetStrValue((OptionType)0, (MultiplayerOptionsAccessMode)1);
		InputKeyItemVM showMouseKey = ShowMouseKey;
		if (showMouseKey != null)
		{
			((ViewModel)showMouseKey).RefreshValues();
		}
	}

	private void ExecutePopulateActionList(MissionScoreboardPlayerVM player)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Expected O, but got Unknown
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Clear();
		if (player.Peer != null && !player.IsMine && !player.IsBot)
		{
			PlayerId id = ((PeerComponent)player.Peer).Peer.Id;
			bool flag = _chatBox.IsPlayerMutedFromGame(id);
			bool flag2 = PermaMuteList.IsPlayerMuted(id);
			bool num = _chatBox.IsPlayerMutedFromPlatform(id);
			bool isMutedFromPlatform = player.Peer.IsMutedFromPlatform;
			if (!num)
			{
				if (!flag2)
				{
					if (PlatformServices.Instance.IsPermanentMuteAvailable)
					{
						((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecutePermanentlyMute, ((object)new TextObject("{=77jmd4QF}Mute Permanently", (Dictionary<string, object>)null)).ToString(), "PermanentMute", (object)player));
					}
					string text = (flag ? ((object)GameTexts.FindText("str_mp_scoreboard_context_unmute_text", (string)null)).ToString() : ((object)GameTexts.FindText("str_mp_scoreboard_context_mute_text", (string)null)).ToString());
					((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteMute, text, flag ? "UnmuteText" : "MuteText", (object)player));
				}
				else
				{
					((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteLiftPermanentMute, ((object)new TextObject("{=CIVPNf2d}Remove Permanent Mute", (Dictionary<string, object>)null)).ToString(), "UnmuteText", (object)player));
				}
			}
			if (player.IsTeammate)
			{
				if (!isMutedFromPlatform && _voiceChatHandler != null && !flag2)
				{
					string text2 = (player.Peer.IsMuted ? ((object)GameTexts.FindText("str_mp_scoreboard_context_unmute_voice", (string)null)).ToString() : ((object)GameTexts.FindText("str_mp_scoreboard_context_mute_voice", (string)null)).ToString());
					((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteMuteVoice, text2, player.Peer.IsMuted ? "UnmuteVoice" : "MuteVoice", (object)player));
				}
				if (_canStartKickPolls)
				{
					((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteKick, ((object)GameTexts.FindText("str_mp_scoreboard_context_kick", (string)null)).ToString(), "StartKickPoll", (object)player));
				}
			}
			StringPairItemWithActionVM val = new StringPairItemWithActionVM((Action<object>)ExecuteReport, ((object)GameTexts.FindText("str_mp_scoreboard_context_report", (string)null)).ToString(), "Report", (object)player);
			if (MultiplayerReportPlayerManager.IsPlayerReportedOverLimit(id))
			{
				val.IsEnabled = false;
				val.Hint.HintText = new TextObject("{=klkYFik9}You've already reported this player.", (Dictionary<string, object>)null);
			}
			((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(val);
			MultiplayerPlayerContextMenuHelper.AddMissionViewProfileOptions(player, PlayerActionList);
		}
		if (((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Count > 0)
		{
			IsPlayerActionsActive = false;
			IsPlayerActionsActive = true;
		}
	}

	public void SetMouseState(bool isMouseVisible)
	{
		IsMouseEnabled = isMouseVisible;
	}

	private void ExecuteReport(object playerObj)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		MissionScoreboardPlayerVM missionScoreboardPlayerVM = playerObj as MissionScoreboardPlayerVM;
		MultiplayerReportPlayerManager.RequestReportPlayer(NetworkMain.GameClient.CurrentMatchId, ((PeerComponent)missionScoreboardPlayerVM.Peer).Peer.Id, missionScoreboardPlayerVM.Peer.DisplayedName, isRequestedFromMission: true);
	}

	private void ExecuteMute(object playerObj)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		MissionScoreboardPlayerVM missionScoreboardPlayerVM = playerObj as MissionScoreboardPlayerVM;
		bool flag = _chatBox.IsPlayerMutedFromGame(((PeerComponent)missionScoreboardPlayerVM.Peer).Peer.Id);
		_chatBox.SetPlayerMuted(((PeerComponent)missionScoreboardPlayerVM.Peer).Peer.Id, !flag);
		GameTexts.SetVariable("PLAYER_NAME", missionScoreboardPlayerVM.Peer.DisplayedName);
		InformationManager.DisplayMessage(new InformationMessage((!flag) ? ((object)GameTexts.FindText("str_mute_notification", (string)null)).ToString() : ((object)GameTexts.FindText("str_unmute_notification", (string)null)).ToString()));
	}

	private void ExecuteMuteVoice(object playerObj)
	{
		MissionScoreboardPlayerVM missionScoreboardPlayerVM = playerObj as MissionScoreboardPlayerVM;
		missionScoreboardPlayerVM.Peer.SetMuted(!missionScoreboardPlayerVM.Peer.IsMuted);
		missionScoreboardPlayerVM.UpdateIsMuted();
	}

	private void ExecutePermanentlyMute(object playerObj)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		MissionScoreboardPlayerVM missionScoreboardPlayerVM = playerObj as MissionScoreboardPlayerVM;
		PermaMuteList.MutePlayer(((PeerComponent)missionScoreboardPlayerVM.Peer).Peer.Id, ((PeerComponent)missionScoreboardPlayerVM.Peer).Name);
		missionScoreboardPlayerVM.Peer.SetMuted(true);
		missionScoreboardPlayerVM.UpdateIsMuted();
		GameTexts.SetVariable("PLAYER_NAME", missionScoreboardPlayerVM.Peer.DisplayedName);
		InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_permanent_mute_notification", (string)null)).ToString()));
	}

	private void ExecuteLiftPermanentMute(object playerObj)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		MissionScoreboardPlayerVM missionScoreboardPlayerVM = playerObj as MissionScoreboardPlayerVM;
		PermaMuteList.RemoveMutedPlayer(((PeerComponent)missionScoreboardPlayerVM.Peer).Peer.Id);
		missionScoreboardPlayerVM.Peer.SetMuted(false);
		missionScoreboardPlayerVM.UpdateIsMuted();
		GameTexts.SetVariable("PLAYER_NAME", missionScoreboardPlayerVM.Peer.DisplayedName);
		InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_unmute_notification", (string)null)).ToString()));
	}

	private void ExecuteKick(object playerObj)
	{
		MissionScoreboardPlayerVM missionScoreboardPlayerVM = playerObj as MissionScoreboardPlayerVM;
		_missionPollComponent.RequestKickPlayerPoll(PeerExtensions.GetNetworkPeer((PeerComponent)(object)missionScoreboardPlayerVM.Peer), false);
	}

	public void Tick(float dt)
	{
		if (!IsActive)
		{
			return;
		}
		EndOfBattle?.Tick(dt);
		CheckAttributeRefresh(dt);
		foreach (MissionScoreboardSideVM item in (Collection<MissionScoreboardSideVM>)(object)Sides)
		{
			item.Tick(dt);
		}
		foreach (MissionScoreboardSideVM item2 in (Collection<MissionScoreboardSideVM>)(object)Sides)
		{
			foreach (MissionScoreboardPlayerVM item3 in (Collection<MissionScoreboardPlayerVM>)(object)item2.Players)
			{
				item3.RefreshDivision(IsSingleSide);
			}
		}
	}

	private void CheckAttributeRefresh(float dt)
	{
		_attributeRefreshTimeElapsed += dt;
		if (_attributeRefreshTimeElapsed >= 1f)
		{
			UpdateSideAllPlayersAttributes((BattleSideEnum)1);
			UpdateSideAllPlayersAttributes((BattleSideEnum)0);
			_attributeRefreshTimeElapsed = 0f;
		}
	}

	private void UpdateSideAllPlayersAttributes(BattleSideEnum battleSide)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		MissionScoreboardSide val = ((IEnumerable<MissionScoreboardSide>)_missionScoreboardComponent.Sides).FirstOrDefault((Func<MissionScoreboardSide, bool>)((MissionScoreboardSide s) => s != null && s.Side == battleSide));
		if (val == null)
		{
			return;
		}
		foreach (MissionPeer player in val.Players)
		{
			OnPlayerPropertiesChanged(battleSide, player);
		}
	}

	public void OnPlayerSideChanged(Team curTeam, Team nextTeam, MissionPeer client)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (((PeerComponent)client).IsMine && nextTeam != null && IsSideValid(nextTeam.Side))
		{
			InitSides();
			return;
		}
		if (curTeam != null && IsSideValid(curTeam.Side))
		{
			_missionSides[_missionScoreboardComponent.GetSideSafe(curTeam.Side).Side].RemovePlayer(client);
		}
		if (nextTeam != null && IsSideValid(nextTeam.Side))
		{
			_missionSides[_missionScoreboardComponent.GetSideSafe(nextTeam.Side).Side].AddPlayer(client);
		}
	}

	private void OnRoundPropertiesChanged()
	{
		foreach (MissionScoreboardSideVM value in _missionSides.Values)
		{
			value.UpdateRoundAttributes();
		}
	}

	private void OnPlayerPropertiesChanged(BattleSideEnum side, MissionPeer client)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (IsSideValid(side))
		{
			_missionSides[_missionScoreboardComponent.GetSideSafe(side).Side].UpdatePlayerAttributes(client);
		}
	}

	private void OnBotPropertiesChanged(BattleSideEnum side)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		BattleSideEnum side2 = _missionScoreboardComponent.GetSideSafe(side).Side;
		if (IsSideValid(side2))
		{
			_missionSides[side2].UpdateBotAttributes();
		}
	}

	private void OnScoreboardInitialized()
	{
		InitSides();
	}

	private void OnMVPSelected(MissionPeer mvpPeer, int mvpCount)
	{
		foreach (MissionScoreboardSideVM item in (Collection<MissionScoreboardSideVM>)(object)Sides)
		{
			foreach (MissionScoreboardPlayerVM item2 in (Collection<MissionScoreboardPlayerVM>)(object)item.Players)
			{
				if (item2.Peer == mvpPeer)
				{
					item2.SetMVPBadgeCount(mvpCount);
					break;
				}
			}
		}
	}

	private bool IsSideValid(BattleSideEnum side)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		if (IsSingleSide)
		{
			if (_missionScoreboardComponent != null && (int)side != -1)
			{
				return (int)side != 2;
			}
			return false;
		}
		if (_missionScoreboardComponent != null && (int)side != -1 && (int)side != 2)
		{
			return _missionScoreboardComponent.Sides.Any((MissionScoreboardSide s) => s != null && s.Side == side);
		}
		return false;
	}

	private void InitSides()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		((Collection<MissionScoreboardSideVM>)(object)Sides).Clear();
		_missionSides.Clear();
		if (IsSingleSide)
		{
			MissionScoreboardSide sideSafe = _missionScoreboardComponent.GetSideSafe((BattleSideEnum)0);
			MultiplayerBattleColors val = MultiplayerBattleColors.CreateWith(sideSafe.GetCulture(), (BasicCultureObject)null);
			MissionScoreboardSideVM missionScoreboardSideVM = new MissionScoreboardSideVM(sideSafe, ExecutePopulateActionList, IsSingleSide, isSecondSide: false, val.AttackerColors);
			((Collection<MissionScoreboardSideVM>)(object)Sides).Add(missionScoreboardSideVM);
			_missionSides.Add(sideSafe.Side, missionScoreboardSideVM);
			return;
		}
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		MissionPeer val2 = ((myPeer != null) ? PeerExtensions.GetComponent<MissionPeer>(myPeer) : null);
		MissionScoreboardSide val3 = ((IEnumerable<MissionScoreboardSide>)_missionScoreboardComponent.Sides).FirstOrDefault((Func<MissionScoreboardSide, bool>)((MissionScoreboardSide s) => s != null && (int)s.Side == 1));
		MissionScoreboardSide val4 = ((IEnumerable<MissionScoreboardSide>)_missionScoreboardComponent.Sides).FirstOrDefault((Func<MissionScoreboardSide, bool>)((MissionScoreboardSide s) => s != null && (int)s.Side == 0));
		MissionScoreboardSide obj;
		if (val2 != null)
		{
			Team team = val2.Team;
			if (((team != null) ? new BattleSideEnum?(team.Side) : ((BattleSideEnum?)null)) == (BattleSideEnum?)1)
			{
				obj = val3;
				goto IL_012e;
			}
		}
		obj = val4;
		goto IL_012e;
		IL_012e:
		MissionScoreboardSide val5 = obj;
		MissionScoreboardSide obj2;
		if (val2 != null)
		{
			Team team2 = val2.Team;
			if (((team2 != null) ? new BattleSideEnum?(team2.Side) : ((BattleSideEnum?)null)) == (BattleSideEnum?)1)
			{
				obj2 = val4;
				goto IL_0173;
			}
		}
		obj2 = val3;
		goto IL_0173;
		IL_0255:
		MissionScoreboardSide val6;
		if (val6 == null)
		{
			return;
		}
		BasicCultureObject val7;
		BasicCultureObject val8;
		MultiplayerCultureColorInfo val9;
		MultiplayerBattleColors val10;
		if (((MBObjectBase)val7).StringId == ((MBObjectBase)val8).StringId)
		{
			if (val2 != null)
			{
				Team team3 = val2.Team;
				if (((team3 != null) ? new BattleSideEnum?(team3.Side) : ((BattleSideEnum?)null)) == (BattleSideEnum?)1)
				{
					val9 = val10.DefenderColors;
					goto IL_02be;
				}
			}
			val9 = val10.AttackerColors;
			goto IL_02be;
		}
		MultiplayerCultureColorInfo cultureColorInfo = val10.DefenderColors;
		goto IL_02cb;
		IL_0216:
		MultiplayerCultureColorInfo cultureColorInfo2;
		MissionScoreboardSideVM missionScoreboardSideVM2 = new MissionScoreboardSideVM(val5, ExecutePopulateActionList, IsSingleSide, isSecondSide: false, cultureColorInfo2);
		((Collection<MissionScoreboardSideVM>)(object)Sides).Add(missionScoreboardSideVM2);
		_missionSides.Add(val5.Side, missionScoreboardSideVM2);
		goto IL_0255;
		IL_02cb:
		MissionScoreboardSideVM missionScoreboardSideVM3 = new MissionScoreboardSideVM(val6, ExecutePopulateActionList, IsSingleSide, isSecondSide: true, cultureColorInfo);
		((Collection<MissionScoreboardSideVM>)(object)Sides).Add(missionScoreboardSideVM3);
		_missionSides.Add(val6.Side, missionScoreboardSideVM3);
		return;
		IL_02be:
		cultureColorInfo = val9;
		goto IL_02cb;
		IL_0173:
		val6 = obj2;
		val7 = val5?.GetCulture();
		val8 = val6?.GetCulture();
		val10 = MultiplayerBattleColors.CreateWith(val7, val8);
		MultiplayerCultureColorInfo val11;
		if (val5 != null)
		{
			if (((MBObjectBase)val7).StringId == ((MBObjectBase)val8).StringId)
			{
				if (val2 != null)
				{
					Team team4 = val2.Team;
					if (((team4 != null) ? new BattleSideEnum?(team4.Side) : ((BattleSideEnum?)null)) == (BattleSideEnum?)1)
					{
						val11 = val10.AttackerColors;
						goto IL_0209;
					}
				}
				val11 = val10.DefenderColors;
				goto IL_0209;
			}
			cultureColorInfo2 = val10.AttackerColors;
			goto IL_0216;
		}
		goto IL_0255;
		IL_0209:
		cultureColorInfo2 = val11;
		goto IL_0216;
	}

	public void DecreaseSpectatorCount(MissionPeer spectatedPeer)
	{
	}

	public void IncreaseSpectatorCount(MissionPeer spectatedPeer)
	{
	}

	public void ExecuteToggleMute()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		foreach (MissionScoreboardSideVM item in (Collection<MissionScoreboardSideVM>)(object)Sides)
		{
			foreach (MissionScoreboardPlayerVM item2 in (Collection<MissionScoreboardPlayerVM>)(object)item.Players)
			{
				if (!item2.IsMine && item2.Peer != null)
				{
					_chatBox.SetPlayerMuted(((PeerComponent)item2.Peer).Peer.Id, !_hasMutedAll);
					item2.Peer.SetMuted(!_hasMutedAll);
					item2.UpdateIsMuted();
				}
			}
		}
		_hasMutedAll = !_hasMutedAll;
		UpdateToggleMuteText();
	}

	private void UpdateToggleMuteText()
	{
		if (_hasMutedAll)
		{
			ToggleMuteText = ((object)_unmuteAllText).ToString();
		}
		else
		{
			ToggleMuteText = ((object)_muteAllText).ToString();
		}
	}

	private void OnPeerMuteStatusUpdated(MissionPeer peer)
	{
		foreach (MissionScoreboardSideVM item in (Collection<MissionScoreboardSideVM>)(object)Sides)
		{
			foreach (MissionScoreboardPlayerVM item2 in (Collection<MissionScoreboardPlayerVM>)(object)item.Players)
			{
				if (item2.Peer == peer)
				{
					item2.UpdateIsMuted();
					break;
				}
			}
		}
	}
}
