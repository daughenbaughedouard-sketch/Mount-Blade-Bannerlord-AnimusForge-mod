using System;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

public class MissionScoreboardPlayerVM : MPPlayerVM
{
	private const string BadgeHeaderID = "badge";

	private readonly MissionPeer _lobbyPeer;

	private readonly Action<MissionScoreboardPlayerVM> _executeActivate;

	private readonly ChatBox _chatBox;

	private int _ping;

	private bool _isPlayer;

	private bool _isVoiceMuted;

	private bool _isTextMuted;

	private MBBindingList<MissionScoreboardStatItemVM> _stats;

	private MBBindingList<MissionScoreboardMVPItemVM> _mvpBadges;

	public int Score { get; private set; }

	public bool IsBot { get; private set; }

	public bool IsMine
	{
		get
		{
			if (_lobbyPeer != null)
			{
				return ((PeerComponent)_lobbyPeer).IsMine;
			}
			return false;
		}
	}

	public bool IsTeammate
	{
		get
		{
			if (_lobbyPeer != null)
			{
				return _lobbyPeer.Team.IsPlayerTeam;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public int Ping
	{
		get
		{
			return _ping;
		}
		set
		{
			if (value != _ping)
			{
				_ping = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Ping");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayer
	{
		get
		{
			return _isPlayer;
		}
		set
		{
			if (value != _isPlayer)
			{
				_isPlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayer");
			}
		}
	}

	[DataSourceProperty]
	public bool IsVoiceMuted
	{
		get
		{
			return _isVoiceMuted;
		}
		set
		{
			if (value != _isVoiceMuted)
			{
				_isVoiceMuted = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsVoiceMuted");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTextMuted
	{
		get
		{
			return _isTextMuted;
		}
		set
		{
			if (value != _isTextMuted)
			{
				_isTextMuted = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTextMuted");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionScoreboardStatItemVM> Stats
	{
		get
		{
			return _stats;
		}
		set
		{
			if (value != _stats)
			{
				_stats = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionScoreboardStatItemVM>>(value, "Stats");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionScoreboardMVPItemVM> MVPBadges
	{
		get
		{
			return _mvpBadges;
		}
		set
		{
			if (value != _mvpBadges)
			{
				_mvpBadges = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionScoreboardMVPItemVM>>(value, "MVPBadges");
			}
		}
	}

	public MissionScoreboardPlayerVM(MissionPeer peer, string[] attributes, string[] headerIDs, int score, Action<MissionScoreboardPlayerVM> executeActivate)
		: base(peer)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		_chatBox = Game.Current.GetGameHandler<ChatBox>();
		_executeActivate = executeActivate;
		_lobbyPeer = peer;
		Stats = new MBBindingList<MissionScoreboardStatItemVM>();
		for (int i = 0; i < attributes.Length; i++)
		{
			((Collection<MissionScoreboardStatItemVM>)(object)Stats).Add(new MissionScoreboardStatItemVM(this, headerIDs[i], ""));
		}
		UpdateAttributes(attributes, score);
		IsPlayer = IsMine;
		MVPBadges = new MBBindingList<MissionScoreboardMVPItemVM>();
		base.Peer.SetMuted(PermaMuteList.IsPlayerMuted(((PeerComponent)peer).Peer.Id));
		UpdateIsMuted();
	}

	public MissionScoreboardPlayerVM(string[] attributes, string[] headerIDs, int score, Action<MissionScoreboardPlayerVM> executeActivate)
		: base((Agent)null)
	{
		_executeActivate = executeActivate;
		Stats = new MBBindingList<MissionScoreboardStatItemVM>();
		for (int i = 0; i < attributes.Length; i++)
		{
			((Collection<MissionScoreboardStatItemVM>)(object)Stats).Add(new MissionScoreboardStatItemVM(this, headerIDs[i], ""));
		}
		UpdateAttributes(attributes, score);
		IsBot = true;
		IsPlayer = false;
		base.IsDead = false;
	}

	public void Tick(float dt)
	{
		if (!IsBot)
		{
			base.IsDead = _lobbyPeer == null || !_lobbyPeer.IsControlledAgentActive;
		}
	}

	public void UpdateAttributes(string[] attributes, int score)
	{
		if (((Collection<MissionScoreboardStatItemVM>)(object)Stats).Count == attributes.Length)
		{
			for (int i = 0; i < attributes.Length; i++)
			{
				((Collection<MissionScoreboardStatItemVM>)(object)Stats)[i].Item = attributes[i] ?? string.Empty;
			}
		}
		Score = score;
	}

	public void ExecuteSelection()
	{
		_executeActivate?.Invoke(this);
	}

	public void UpdateIsMuted()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = PermaMuteList.IsPlayerMuted(((PeerComponent)_lobbyPeer).Peer.Id);
		IsTextMuted = flag || _chatBox.IsPlayerMuted(((PeerComponent)_lobbyPeer).Peer.Id);
		IsVoiceMuted = flag || base.Peer.IsMutedFromGameOrPlatform;
	}

	public void SetMVPBadgeCount(int badgeCount)
	{
		((Collection<MissionScoreboardMVPItemVM>)(object)MVPBadges).Clear();
		for (int i = 0; i < badgeCount; i++)
		{
			((Collection<MissionScoreboardMVPItemVM>)(object)MVPBadges).Add(new MissionScoreboardMVPItemVM());
		}
	}
}
