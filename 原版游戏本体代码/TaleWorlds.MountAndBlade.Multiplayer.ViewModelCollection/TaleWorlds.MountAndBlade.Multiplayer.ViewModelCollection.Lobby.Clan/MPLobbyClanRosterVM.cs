using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanRosterVM : ViewModel
{
	private class MemberComparer : IComparer<MPLobbyClanMemberItemVM>
	{
		public int Compare(MPLobbyClanMemberItemVM x, MPLobbyClanMemberItemVM y)
		{
			if (y.Rank != x.Rank)
			{
				return y.Rank.CompareTo(x.Rank);
			}
			return y.IsOnline.CompareTo(x.IsOnline);
		}
	}

	private bool _isClanLeader;

	private bool _isClanOfficer;

	private MemberComparer _memberComparer;

	private bool _isSelected;

	private bool _isMemberActionsActive;

	private bool _isPrivilegedMember;

	private string _rosterText;

	private string _nameText;

	private string _badgeText;

	private string _statusText;

	private MBBindingList<MPLobbyClanMemberItemVM> _membersList;

	private MBBindingList<StringPairItemWithActionVM> _memberActionsList;

	private HintViewModel _promoteToClanOfficerHint;

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
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMemberActionsActive
	{
		get
		{
			return _isMemberActionsActive;
		}
		set
		{
			if (value != _isMemberActionsActive)
			{
				_isMemberActionsActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMemberActionsActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPrivilegedMember
	{
		get
		{
			return _isPrivilegedMember;
		}
		set
		{
			if (value != _isPrivilegedMember)
			{
				_isPrivilegedMember = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPrivilegedMember");
			}
		}
	}

	[DataSourceProperty]
	public string RosterText
	{
		get
		{
			return _rosterText;
		}
		set
		{
			if (value != _rosterText)
			{
				_rosterText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RosterText");
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
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NameText");
			}
		}
	}

	[DataSourceProperty]
	public string BadgeText
	{
		get
		{
			return _badgeText;
		}
		set
		{
			if (value != _badgeText)
			{
				_badgeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BadgeText");
			}
		}
	}

	[DataSourceProperty]
	public string StatusText
	{
		get
		{
			return _statusText;
		}
		set
		{
			if (value != _statusText)
			{
				_statusText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "StatusText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyClanMemberItemVM> MembersList
	{
		get
		{
			return _membersList;
		}
		set
		{
			if (value != _membersList)
			{
				_membersList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyClanMemberItemVM>>(value, "MembersList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemWithActionVM> MemberActionsList
	{
		get
		{
			return _memberActionsList;
		}
		set
		{
			if (value != _memberActionsList)
			{
				_memberActionsList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemWithActionVM>>(value, "MemberActionsList");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel PromoteToClanOfficerHint
	{
		get
		{
			return _promoteToClanOfficerHint;
		}
		set
		{
			if (value != _promoteToClanOfficerHint)
			{
				_promoteToClanOfficerHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "PromoteToClanOfficerHint");
			}
		}
	}

	public MPLobbyClanRosterVM()
	{
		MembersList = new MBBindingList<MPLobbyClanMemberItemVM>();
		MemberActionsList = new MBBindingList<StringPairItemWithActionVM>();
		_memberComparer = new MemberComparer();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		RosterText = ((object)new TextObject("{=oyVeCtlg}Roster", (Dictionary<string, object>)null)).ToString();
		NameText = ((object)new TextObject("{=PDdh1sBj}Name", (Dictionary<string, object>)null)).ToString();
		BadgeText = ((object)new TextObject("{=4PrfimcK}Badge", (Dictionary<string, object>)null)).ToString();
		StatusText = ((object)new TextObject("{=DXczLzml}Status", (Dictionary<string, object>)null)).ToString();
		PromoteToClanOfficerHint = new HintViewModel(new TextObject("{=oeSrXaKt}You need to demote one of the officers", (Dictionary<string, object>)null), (string)null);
	}

	public void RefreshClanInformation(ClanHomeInfo info)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Invalid comparison between Unknown and I4
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Invalid comparison between Unknown and I4
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Invalid comparison between Unknown and I4
		if (info == null || info.ClanInfo == null)
		{
			return;
		}
		_isClanLeader = NetworkMain.GameClient.IsClanLeader;
		_isClanOfficer = NetworkMain.GameClient.IsClanOfficer;
		((Collection<MPLobbyClanMemberItemVM>)(object)MembersList).Clear();
		ClanPlayer[] players = info.ClanInfo.Players;
		foreach (ClanPlayer member in players)
		{
			if (!MultiplayerPlayerHelper.IsBlocked(member.PlayerId))
			{
				ClanPlayerInfo val = info.ClanPlayerInfos.First(delegate(ClanPlayerInfo val2)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000f: Unknown result type (might be due to invalid IL or missing references)
					PlayerId playerId = val2.PlayerId;
					return ((PlayerId)(ref playerId)).Equals(member.PlayerId);
				});
				if (val != null)
				{
					bool isOnline = (int)val.State == 2 || (int)val.State == 4 || (int)val.State == 3;
					((Collection<MPLobbyClanMemberItemVM>)(object)MembersList).Add(new MPLobbyClanMemberItemVM(member, isOnline, val.ActiveBadgeId, val.State, ExecutePopulateActionsList));
				}
			}
		}
		MembersList.Sort((IComparer<MPLobbyClanMemberItemVM>)_memberComparer);
		IsPrivilegedMember = NetworkMain.GameClient.IsClanLeader || NetworkMain.GameClient.IsClanOfficer;
	}

	public void OnPlayerNameUpdated(string playerName)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((Collection<MPLobbyClanMemberItemVM>)(object)MembersList).Count; i++)
		{
			MPLobbyClanMemberItemVM mPLobbyClanMemberItemVM = ((Collection<MPLobbyClanMemberItemVM>)(object)MembersList)[i];
			if (mPLobbyClanMemberItemVM.Id == NetworkMain.GameClient.PlayerID)
			{
				mPLobbyClanMemberItemVM.UpdateNameAndAvatar(forceUpdate: true);
			}
		}
	}

	private void ExecutePopulateActionsList(MPLobbyClanMemberItemVM member)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Expected O, but got Unknown
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Expected O, but got Unknown
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Expected O, but got Unknown
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Expected O, but got Unknown
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Expected O, but got Unknown
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Expected O, but got Unknown
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Expected O, but got Unknown
		((Collection<StringPairItemWithActionVM>)(object)MemberActionsList).Clear();
		if (NetworkMain.GameClient.PlayerID != member.Id)
		{
			if (_isClanLeader)
			{
				((Collection<StringPairItemWithActionVM>)(object)MemberActionsList).Add(new StringPairItemWithActionVM((Action<object>)ExecutePromoteToClanLeader, ((object)new TextObject("{=GRpGNYHW}Promote To Clan Leader", (Dictionary<string, object>)null)).ToString(), "PromoteToClanLeader", (object)member));
				if (NetworkMain.GameClient.IsPlayerClanOfficer(member.Id))
				{
					((Collection<StringPairItemWithActionVM>)(object)MemberActionsList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteDemoteFromClanOfficer, ((object)new TextObject("{=gowlLS2b}Demote From Clan Officer", (Dictionary<string, object>)null)).ToString(), "DemoteFromClanOfficer", (object)member));
				}
				else
				{
					StringPairItemWithActionVM val = new StringPairItemWithActionVM((Action<object>)ExecutePromoteToClanOfficer, ((object)new TextObject("{=BXI1ObU8}Promote To Clan Officer", (Dictionary<string, object>)null)).ToString(), "PromoteToClanOfficer", (object)member);
					if (NetworkMain.GameClient.PlayersInClan.Count((ClanPlayer m) => (int)m.Role == 1) == Parameters.ClanOfficerCount)
					{
						val.IsEnabled = false;
						val.Hint = PromoteToClanOfficerHint;
					}
					((Collection<StringPairItemWithActionVM>)(object)MemberActionsList).Add(val);
				}
			}
			if ((_isClanOfficer || _isClanLeader) && !NetworkMain.GameClient.IsPlayerClanLeader(member.Id) && (!_isClanOfficer || !NetworkMain.GameClient.IsPlayerClanOfficer(member.Id)))
			{
				((Collection<StringPairItemWithActionVM>)(object)MemberActionsList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteKickFromClan, ((object)new TextObject("{=S8pZEPni}Kick From Clan", (Dictionary<string, object>)null)).ToString(), "KickFromClan", (object)member));
			}
			if (NetworkMain.GameClient.FriendInfos.All((FriendInfo f) => f.Id != member.Id))
			{
				((Collection<StringPairItemWithActionVM>)(object)MemberActionsList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteRequestFriendship, ((object)GameTexts.FindText("str_mp_scoreboard_context_request_friendship", (string)null)).ToString(), "RequestFriendship", (object)member));
			}
			else
			{
				((Collection<StringPairItemWithActionVM>)(object)MemberActionsList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteTerminateFriendship, ((object)new TextObject("{=2YIVRuRa}Remove From Friends", (Dictionary<string, object>)null)).ToString(), "TerminateFriendship", (object)member));
			}
			if (NetworkMain.GameClient.SupportedFeatures.SupportsFeatures((Features)4))
			{
				((Collection<StringPairItemWithActionVM>)(object)MemberActionsList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteInviteToParty, ((object)new TextObject("{=RzROgBkv}Invite To Party", (Dictionary<string, object>)null)).ToString(), "InviteToParty", (object)member));
			}
			MultiplayerPlayerContextMenuHelper.AddLobbyViewProfileOptions(member, MemberActionsList);
		}
		if (((Collection<StringPairItemWithActionVM>)(object)MemberActionsList).Count > 0)
		{
			IsMemberActionsActive = false;
			IsMemberActionsActive = true;
		}
	}

	private void ExecuteRequestFriendship(object memberObj)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyClanMemberItemVM mPLobbyClanMemberItemVM = memberObj as MPLobbyClanMemberItemVM;
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(mPLobbyClanMemberItemVM.Id);
		NetworkMain.GameClient.AddFriend(mPLobbyClanMemberItemVM.Id, flag);
	}

	private void ExecuteTerminateFriendship(object memberObj)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyClanMemberItemVM mPLobbyClanMemberItemVM = memberObj as MPLobbyClanMemberItemVM;
		NetworkMain.GameClient.RemoveFriend(mPLobbyClanMemberItemVM.Id);
	}

	private void ExecutePromoteToClanLeader(object memberObj)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		MPLobbyClanMemberItemVM member = memberObj as MPLobbyClanMemberItemVM;
		GameTexts.SetVariable("MEMBER_NAME", member.Name);
		string? text = ((object)new TextObject("{=GRpGNYHW}Promote To Clan Leader", (Dictionary<string, object>)null)).ToString();
		string text2 = ((object)new TextObject("{=Z0TW2cub}Are you sure want to promote {MEMBER_NAME} as clan leader? You will lose your leadership.", (Dictionary<string, object>)null)).ToString();
		InformationManager.ShowInquiry(new InquiryData(text, text2, true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			PromoteToClanLeader(member.Id);
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void ExecutePromoteToClanOfficer(object memberObj)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		MPLobbyClanMemberItemVM member = memberObj as MPLobbyClanMemberItemVM;
		GameTexts.SetVariable("MEMBER_NAME", member.Name);
		string? text = ((object)new TextObject("{=BXI1ObU8}Promote To Clan Officer", (Dictionary<string, object>)null)).ToString();
		string text2 = ((object)new TextObject("{=MS4Ng2iw}Are you sure want to promote {MEMBER_NAME} as clan officer?", (Dictionary<string, object>)null)).ToString();
		InformationManager.ShowInquiry(new InquiryData(text, text2, true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			PromoteToClanOfficer(member.Id);
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void ExecuteDemoteFromClanOfficer(object memberObj)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		MPLobbyClanMemberItemVM member = memberObj as MPLobbyClanMemberItemVM;
		GameTexts.SetVariable("MEMBER_NAME", member.Name);
		string? text = ((object)new TextObject("{=gowlLS2b}Demote From Clan Officer", (Dictionary<string, object>)null)).ToString();
		string text2 = ((object)new TextObject("{=pSb1P6ZA}Are you sure want to demote {MEMBER_NAME} from clan officers?", (Dictionary<string, object>)null)).ToString();
		InformationManager.ShowInquiry(new InquiryData(text, text2, true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			DemoteFromClanOfficer(member.Id);
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void ExecuteKickFromClan(object memberObj)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		MPLobbyClanMemberItemVM member = memberObj as MPLobbyClanMemberItemVM;
		GameTexts.SetVariable("MEMBER_NAME", member.Name);
		string? text = ((object)new TextObject("{=S8pZEPni}Kick From Clan", (Dictionary<string, object>)null)).ToString();
		string text2 = ((object)new TextObject("{=L6eaNe2q}Are you sure want to kick {MEMBER_NAME} from clan?", (Dictionary<string, object>)null)).ToString();
		InformationManager.ShowInquiry(new InquiryData(text, text2, true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			KickFromClan(member.Id);
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void ExecuteInviteToParty(object memberObj)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		MPLobbyClanMemberItemVM mPLobbyClanMemberItemVM = memberObj as MPLobbyClanMemberItemVM;
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(mPLobbyClanMemberItemVM.Id);
		NetworkMain.GameClient.InviteToParty(mPLobbyClanMemberItemVM.Id, flag);
	}

	private void ExecuteViewProfile(object memberObj)
	{
		(memberObj as MPLobbyClanMemberItemVM).ExecuteShowProfile();
	}

	private void PromoteToClanLeader(PlayerId playerId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(playerId);
		NetworkMain.GameClient.PromoteToClanLeader(playerId, flag);
	}

	private void PromoteToClanOfficer(PlayerId playerId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = BannerlordConfig.EnableGenericNames && !NetworkMain.GameClient.IsKnownPlayer(playerId);
		NetworkMain.GameClient.AssignAsClanOfficer(playerId, flag);
	}

	private void DemoteFromClanOfficer(PlayerId playerId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		NetworkMain.GameClient.RemoveClanOfficerRoleForPlayer(playerId);
	}

	private void KickFromClan(PlayerId playerId)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		NetworkMain.GameClient.KickFromClan(playerId);
	}
}
