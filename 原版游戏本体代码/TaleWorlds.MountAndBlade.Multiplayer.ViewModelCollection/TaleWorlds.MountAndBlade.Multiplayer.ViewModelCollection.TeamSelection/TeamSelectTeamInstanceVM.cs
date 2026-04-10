using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.TeamSelection;

public class TeamSelectTeamInstanceVM : ViewModel
{
	private const int MaxFriendAvatarCount = 6;

	public readonly Team Team;

	public readonly Action<Team> _onSelect;

	private readonly List<MPPlayerVM> _friends;

	private MissionScoreboardComponent _missionScoreboardComponent;

	private MissionScoreboardSide _missionScoreboardSide;

	private readonly BasicCultureObject _culture;

	private bool _isDisabled;

	private string _displayedPrimary;

	private string _displayedSecondary;

	private string _displayedSecondarySub;

	private string _lockText;

	private string _cultureId;

	private int _score;

	private BannerImageIdentifierVM _banner;

	private MBBindingList<MPPlayerVM> _friendAvatars;

	private bool _hasExtraFriends;

	private bool _isAttacker;

	private bool _isSiege;

	private string _friendsExtraText;

	private HintViewModel _friendsExtraHint;

	private Color _cultureColor1;

	private Color _cultureColor2;

	[DataSourceProperty]
	public string CultureId
	{
		get
		{
			return _cultureId;
		}
		set
		{
			if (_cultureId != value)
			{
				_cultureId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureId");
			}
		}
	}

	[DataSourceProperty]
	public int Score
	{
		get
		{
			return _score;
		}
		set
		{
			if (value != _score)
			{
				_score = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Score");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDisabled
	{
		get
		{
			return _isDisabled;
		}
		set
		{
			if (_isDisabled != value)
			{
				_isDisabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDisabled");
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
			if (_isAttacker != value)
			{
				_isAttacker = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAttacker");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSiege
	{
		get
		{
			return _isSiege;
		}
		set
		{
			if (_isSiege != value)
			{
				_isSiege = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSiege");
			}
		}
	}

	[DataSourceProperty]
	public string DisplayedPrimary
	{
		get
		{
			return _displayedPrimary;
		}
		set
		{
			_displayedPrimary = value;
			((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DisplayedPrimary");
		}
	}

	[DataSourceProperty]
	public string DisplayedSecondary
	{
		get
		{
			return _displayedSecondary;
		}
		set
		{
			_displayedSecondary = value;
			((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DisplayedSecondary");
		}
	}

	[DataSourceProperty]
	public string DisplayedSecondarySub
	{
		get
		{
			return _displayedSecondarySub;
		}
		set
		{
			_displayedSecondarySub = value;
			((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DisplayedSecondarySub");
		}
	}

	[DataSourceProperty]
	public string LockText
	{
		get
		{
			return _lockText;
		}
		set
		{
			_lockText = value;
			((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LockText");
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM Banner
	{
		get
		{
			return _banner;
		}
		set
		{
			if (value != _banner && (value == null || _banner == null || ((ImageIdentifierVM)_banner).Id != ((ImageIdentifierVM)value).Id))
			{
				_banner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "Banner");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPPlayerVM> FriendAvatars
	{
		get
		{
			return _friendAvatars;
		}
		set
		{
			if (_friendAvatars != value)
			{
				_friendAvatars = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPPlayerVM>>(value, "FriendAvatars");
			}
		}
	}

	[DataSourceProperty]
	public bool HasExtraFriends
	{
		get
		{
			return _hasExtraFriends;
		}
		set
		{
			if (_hasExtraFriends != value)
			{
				_hasExtraFriends = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasExtraFriends");
			}
		}
	}

	[DataSourceProperty]
	public string FriendsExtraText
	{
		get
		{
			return _friendsExtraText;
		}
		set
		{
			if (_friendsExtraText != value)
			{
				_friendsExtraText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FriendsExtraText");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel FriendsExtraHint
	{
		get
		{
			return _friendsExtraHint;
		}
		set
		{
			if (_friendsExtraHint != value)
			{
				_friendsExtraHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "FriendsExtraHint");
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

	public TeamSelectTeamInstanceVM(MissionScoreboardComponent missionScoreboardComponent, Team team, BasicCultureObject culture, Banner banner, Action<Team> onSelect, MultiplayerCultureColorInfo cultureColors)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Invalid comparison between Unknown and I4
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		Team = team;
		_onSelect = onSelect;
		_culture = culture;
		Mission current = Mission.Current;
		IsSiege = current != null && current.HasMissionBehavior<MissionMultiplayerSiegeClient>();
		if (Team != null && (int)Team.Side != -1)
		{
			_missionScoreboardComponent = missionScoreboardComponent;
			_missionScoreboardComponent.OnRoundPropertiesChanged += UpdateTeamScores;
			_missionScoreboardSide = ((IEnumerable<MissionScoreboardSide>)_missionScoreboardComponent.Sides).FirstOrDefault((Func<MissionScoreboardSide, bool>)((MissionScoreboardSide s) => s != null && s.Side == Team.Side));
			IsAttacker = (int)Team.Side == 1;
			UpdateTeamScores();
		}
		CultureId = ((culture == null) ? "" : ((MBObjectBase)culture).StringId);
		if (team == null)
		{
			IsDisabled = true;
		}
		Banner = new BannerImageIdentifierVM(banner, true);
		CultureColor1 = cultureColors.Color1;
		CultureColor2 = cultureColors.Color2;
		_friends = new List<MPPlayerVM>();
		FriendAvatars = new MBBindingList<MPPlayerVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		DisplayedPrimary = ((_culture == null) ? ((object)new TextObject("{=pSheKLB4}Spectator", (Dictionary<string, object>)null)).ToString() : ((object)_culture.Name).ToString());
	}

	public override void OnFinalize()
	{
		if (_missionScoreboardComponent != null)
		{
			_missionScoreboardComponent.OnRoundPropertiesChanged -= UpdateTeamScores;
		}
		_missionScoreboardComponent = null;
		_missionScoreboardSide = null;
		((ViewModel)this).OnFinalize();
	}

	private void UpdateTeamScores()
	{
		if (_missionScoreboardSide != null)
		{
			Score = _missionScoreboardSide.SideScore;
		}
	}

	public void RefreshFriends(IEnumerable<MissionPeer> friends)
	{
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Expected O, but got Unknown
		List<MissionPeer> list = friends.ToList();
		List<MPPlayerVM> list2 = new List<MPPlayerVM>();
		foreach (MPPlayerVM friend in _friends)
		{
			if (!list.Contains(friend.Peer))
			{
				list2.Add(friend);
			}
		}
		foreach (MPPlayerVM item in list2)
		{
			_friends.Remove(item);
		}
		List<MissionPeer> list3 = _friends.Select((MPPlayerVM x) => x.Peer).ToList();
		foreach (MissionPeer item2 in list)
		{
			if (!list3.Contains(item2))
			{
				_friends.Add(new MPPlayerVM(item2));
			}
		}
		((Collection<MPPlayerVM>)(object)FriendAvatars).Clear();
		MBStringBuilder val = default(MBStringBuilder);
		((MBStringBuilder)(ref val)).Initialize(16, "RefreshFriends");
		for (int num = 0; num < _friends.Count; num++)
		{
			if (num < 6)
			{
				((Collection<MPPlayerVM>)(object)FriendAvatars).Add(_friends[num]);
			}
			else
			{
				((MBStringBuilder)(ref val)).AppendLine<string>(_friends[num].Peer.DisplayedName);
			}
		}
		int num2 = _friends.Count - 6;
		if (num2 > 0)
		{
			HasExtraFriends = true;
			TextObject val2 = new TextObject("{=hbwp3g3k}+{FRIEND_COUNT} {newline} {?PLURAL}friends{?}friend{\\?}", (Dictionary<string, object>)null);
			val2.SetTextVariable("FRIEND_COUNT", num2);
			val2.SetTextVariable("PLURAL", (num2 != 1) ? 1 : 0);
			FriendsExtraText = ((object)val2).ToString();
			FriendsExtraHint = new HintViewModel(val2, (string)null);
		}
		else
		{
			((MBStringBuilder)(ref val)).Release();
			HasExtraFriends = false;
			FriendsExtraText = "";
		}
	}

	public void SetIsDisabled(bool isCurrentTeam, bool disabledForBalance)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		IsDisabled = isCurrentTeam || disabledForBalance;
		if (isCurrentTeam)
		{
			LockText = ((object)new TextObject("{=SoQcsslF}CURRENT TEAM", (Dictionary<string, object>)null)).ToString();
		}
		else if (disabledForBalance)
		{
			LockText = ((object)new TextObject("{=qe46yXVJ}LOCKED FOR BALANCE", (Dictionary<string, object>)null)).ToString();
		}
	}

	public void ExecuteSelectTeam()
	{
		if (_onSelect != null)
		{
			_onSelect(Team);
		}
	}
}
