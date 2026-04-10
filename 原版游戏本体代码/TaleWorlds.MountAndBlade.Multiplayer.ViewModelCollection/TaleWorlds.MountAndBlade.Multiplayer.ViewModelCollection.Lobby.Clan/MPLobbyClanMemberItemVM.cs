using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanMemberItemVM : MPLobbyPlayerBaseVM
{
	private ClanPlayer _member;

	private Action<MPLobbyClanMemberItemVM> _executeActivate;

	private bool _isOnline;

	private bool _isClanLeader;

	private string _notEligibleInfo;

	private string _inviteAcceptInfo;

	private int _rank;

	private MBBindingList<StringPairItemWithActionVM> _userActionsList;

	private HintViewModel _rankHint;

	public PlayerId Id { get; private set; }

	[DataSourceProperty]
	public bool IsOnline
	{
		get
		{
			return _isOnline;
		}
		set
		{
			if (value != _isOnline)
			{
				_isOnline = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsOnline");
			}
		}
	}

	[DataSourceProperty]
	public bool IsClanLeader
	{
		get
		{
			return _isClanLeader;
		}
		set
		{
			if (value != _isClanLeader)
			{
				_isClanLeader = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsClanLeader");
			}
		}
	}

	[DataSourceProperty]
	public string NotEligibleInfo
	{
		get
		{
			return _notEligibleInfo;
		}
		set
		{
			if (value != _notEligibleInfo)
			{
				_notEligibleInfo = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NotEligibleInfo");
			}
		}
	}

	[DataSourceProperty]
	public string InviteAcceptInfo
	{
		get
		{
			return _inviteAcceptInfo;
		}
		set
		{
			if (value != _inviteAcceptInfo)
			{
				_inviteAcceptInfo = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "InviteAcceptInfo");
			}
		}
	}

	[DataSourceProperty]
	public int Rank
	{
		get
		{
			return _rank;
		}
		set
		{
			if (value != _rank)
			{
				_rank = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Rank");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemWithActionVM> UserActionsList
	{
		get
		{
			return _userActionsList;
		}
		set
		{
			if (value != _userActionsList)
			{
				_userActionsList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemWithActionVM>>(value, "UserActionsList");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel RankHint
	{
		get
		{
			return _rankHint;
		}
		set
		{
			if (value != _rankHint)
			{
				_rankHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "RankHint");
			}
		}
	}

	public MPLobbyClanMemberItemVM(PlayerId playerId)
		: base(playerId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Id = playerId;
		((ViewModel)this).RefreshValues();
	}

	public unsafe MPLobbyClanMemberItemVM(ClanPlayer member, bool isOnline, string selectedBadgeID, AnotherPlayerState state, Action<MPLobbyClanMemberItemVM> executeActivate = null)
		: base(member.PlayerId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected I4, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		_member = member;
		Id = _member.PlayerId;
		IsOnline = isOnline;
		_executeActivate = executeActivate;
		base.SelectedBadgeID = selectedBadgeID;
		if (isOnline)
		{
			base.StateText = ((object)GameTexts.FindText("str_multiplayer_lobby_state", ((object)(*(AnotherPlayerState*)(&state))/*cast due to .constrained prefix*/).ToString())).ToString();
		}
		IsClanLeader = (int)_member.Role == 2;
		Rank = (int)_member.Role;
		RankHint = new HintViewModel();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		base.RefreshValues();
		NotEligibleInfo = "";
		ClanPlayer member = _member;
		if (member != null && (int)member.Role == 2)
		{
			RankHint.HintText = new TextObject("{=SrfYbg3x}Leader", (Dictionary<string, object>)null);
			return;
		}
		ClanPlayer member2 = _member;
		if (member2 != null && (int)member2.Role == 1)
		{
			RankHint.HintText = new TextObject("{=ZYF2t1VI}Officer", (Dictionary<string, object>)null);
		}
	}

	public void SetNotEligibleInfo(PlayerNotEligibleError notEligibleError)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		string text = "";
		if ((int)notEligibleError == 1)
		{
			text = ((object)new TextObject("{=zEMWM4h3}Already In a Clan", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)notEligibleError == 0)
		{
			text = ((object)new TextObject("{=hPbppi6E}Not At The Lobby", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)notEligibleError == 2)
		{
			text = ((object)new TextObject("{=MsokbMx2}Does not support Clan feature", (Dictionary<string, object>)null)).ToString();
		}
		if (string.IsNullOrEmpty(NotEligibleInfo))
		{
			NotEligibleInfo = text;
			return;
		}
		GameTexts.SetVariable("LEFT", NotEligibleInfo);
		GameTexts.SetVariable("RIGHT", text);
		NotEligibleInfo = ((object)GameTexts.FindText("str_LEFT_comma_RIGHT", (string)null)).ToString();
	}

	private void ExecuteSelection()
	{
		_executeActivate?.Invoke(this);
	}
}
