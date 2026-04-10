using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MultiplayerEndOfBattleVM : ViewModel
{
	private readonly MissionMultiplayerGameModeBaseClient _gameMode;

	private readonly float _activeDelay;

	private bool _isBattleEnded;

	private float _activateTimeElapsed;

	private bool _isEnabled;

	private bool _hasFirstPlace;

	private bool _hasSecondPlace;

	private bool _hasThirdPlace;

	private string _titleText;

	private string _descriptionText;

	private MPEndOfBattlePlayerVM _firstPlacePlayer;

	private MPEndOfBattlePlayerVM _secondPlacePlayer;

	private MPEndOfBattlePlayerVM _thirdPlacePlayer;

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
	public bool HasFirstPlace
	{
		get
		{
			return _hasFirstPlace;
		}
		set
		{
			if (value != _hasFirstPlace)
			{
				_hasFirstPlace = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasFirstPlace");
			}
		}
	}

	[DataSourceProperty]
	public bool HasSecondPlace
	{
		get
		{
			return _hasSecondPlace;
		}
		set
		{
			if (value != _hasSecondPlace)
			{
				_hasSecondPlace = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasSecondPlace");
			}
		}
	}

	[DataSourceProperty]
	public bool HasThirdPlace
	{
		get
		{
			return _hasThirdPlace;
		}
		set
		{
			if (value != _hasThirdPlace)
			{
				_hasThirdPlace = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasThirdPlace");
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
	public string DescriptionText
	{
		get
		{
			return _descriptionText;
		}
		set
		{
			if (value != _descriptionText)
			{
				_descriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public MPEndOfBattlePlayerVM FirstPlacePlayer
	{
		get
		{
			return _firstPlacePlayer;
		}
		set
		{
			if (value != _firstPlacePlayer)
			{
				_firstPlacePlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPEndOfBattlePlayerVM>(value, "FirstPlacePlayer");
			}
		}
	}

	[DataSourceProperty]
	public MPEndOfBattlePlayerVM SecondPlacePlayer
	{
		get
		{
			return _secondPlacePlayer;
		}
		set
		{
			if (value != _secondPlacePlayer)
			{
				_secondPlacePlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPEndOfBattlePlayerVM>(value, "SecondPlacePlayer");
			}
		}
	}

	[DataSourceProperty]
	public MPEndOfBattlePlayerVM ThirdPlacePlayer
	{
		get
		{
			return _thirdPlacePlayer;
		}
		set
		{
			if (value != _thirdPlacePlayer)
			{
				_thirdPlacePlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPEndOfBattlePlayerVM>(value, "ThirdPlacePlayer");
			}
		}
	}

	public MultiplayerEndOfBattleVM()
	{
		_activeDelay = MissionLobbyComponent.PostMatchWaitDuration / 2f;
		_gameMode = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=GPfkMajw}Battle Ended", (Dictionary<string, object>)null)).ToString();
		DescriptionText = ((object)new TextObject("{=ADPaaX8R}Best Players of This Battle", (Dictionary<string, object>)null)).ToString();
	}

	public void OnTick(float dt)
	{
		if (_isBattleEnded)
		{
			_activateTimeElapsed += dt;
			if (_activateTimeElapsed >= _activeDelay)
			{
				_isBattleEnded = false;
				OnEnabled();
			}
		}
	}

	private void OnEnabled()
	{
		MissionScoreboardComponent missionBehavior = Mission.Current.GetMissionBehavior<MissionScoreboardComponent>();
		List<MissionPeer> list = new List<MissionPeer>();
		foreach (MissionScoreboardSide item in missionBehavior.Sides.Where((MissionScoreboardSide s) => s != null && (int)s.Side != -1))
		{
			foreach (MissionPeer player in item.Players)
			{
				list.Add(player);
			}
		}
		list.Sort((MissionPeer p1, MissionPeer p2) => GetPeerScore(p2).CompareTo(GetPeerScore(p1)));
		if (list.Count > 0)
		{
			HasFirstPlace = true;
			MissionPeer peer = list[0];
			FirstPlacePlayer = new MPEndOfBattlePlayerVM(peer, GetPeerScore(peer), 1);
		}
		if (list.Count > 1)
		{
			HasSecondPlace = true;
			MissionPeer peer2 = list[1];
			SecondPlacePlayer = new MPEndOfBattlePlayerVM(peer2, GetPeerScore(peer2), 2);
		}
		if (list.Count > 2)
		{
			HasThirdPlace = true;
			MissionPeer peer3 = list[2];
			ThirdPlacePlayer = new MPEndOfBattlePlayerVM(peer3, GetPeerScore(peer3), 3);
		}
		IsEnabled = true;
	}

	public void OnBattleEnded()
	{
		_isBattleEnded = true;
	}

	private int GetPeerScore(MissionPeer peer)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if (peer == null)
		{
			return 0;
		}
		if ((int)_gameMode.GameType == 1)
		{
			DuelMissionRepresentative component = ((PeerComponent)peer).GetComponent<DuelMissionRepresentative>();
			if (component == null)
			{
				return 0;
			}
			return component.Score;
		}
		return peer.Score;
	}
}
