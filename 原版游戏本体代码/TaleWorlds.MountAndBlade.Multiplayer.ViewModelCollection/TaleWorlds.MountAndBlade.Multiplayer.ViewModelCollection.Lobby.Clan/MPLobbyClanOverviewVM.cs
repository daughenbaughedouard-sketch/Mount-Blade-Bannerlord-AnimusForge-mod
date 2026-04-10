using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanOverviewVM : ViewModel
{
	private readonly Action _openInviteClanMemberPopup;

	private bool _isSelected;

	private bool _isLeader;

	private bool _isPrivilegedMember;

	private bool _areActionButtonsEnabled;

	private bool _doesHaveDescription;

	private bool _doesHaveAnnouncements;

	private string _nameText;

	private string _membersText;

	private string _changeSigilText;

	private string _changeFactionText;

	private string _leaveText;

	private string _disbandText;

	private string _factionCultureID;

	private string _informationText;

	private string _announcementsText;

	private string _clanDescriptionText;

	private string _noDescriptionText;

	private string _noAnnouncementsText;

	private string _titleText;

	private Color _cultureColor1;

	private Color _cultureColor2;

	private BannerImageIdentifierVM _sigilImage;

	private BannerImageIdentifierVM _factionBanner;

	private MPLobbyClanChangeSigilPopupVM _changeSigilPopup;

	private MPLobbyClanChangeFactionPopupVM _changeFactionPopup;

	private MBBindingList<MPLobbyClanAnnouncementVM> _announcementsList;

	private MPLobbyClanSendPostPopupVM _sendAnnouncementPopup;

	private MPLobbyClanSendPostPopupVM _setClanInformationPopup;

	private HintViewModel _cantLeaveHint;

	private HintViewModel _inviteMembersHint;

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
	public bool IsLeader
	{
		get
		{
			return _isLeader;
		}
		set
		{
			if (value != _isLeader)
			{
				_isLeader = value;
				((ViewModel)this).OnPropertyChanged("IsLeader");
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
				((ViewModel)this).OnPropertyChanged("IsPrivilegedMember");
			}
		}
	}

	[DataSourceProperty]
	public bool AreActionButtonsEnabled
	{
		get
		{
			return _areActionButtonsEnabled;
		}
		set
		{
			if (value != _areActionButtonsEnabled)
			{
				_areActionButtonsEnabled = value;
				((ViewModel)this).OnPropertyChanged("AreActionButtonsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool DoesHaveDescription
	{
		get
		{
			return _doesHaveDescription;
		}
		set
		{
			if (value != _doesHaveDescription)
			{
				_doesHaveDescription = value;
				((ViewModel)this).OnPropertyChanged("DoesHaveDescription");
			}
		}
	}

	[DataSourceProperty]
	public bool DoesHaveAnnouncements
	{
		get
		{
			return _doesHaveAnnouncements;
		}
		set
		{
			if (value != _doesHaveAnnouncements)
			{
				_doesHaveAnnouncements = value;
				((ViewModel)this).OnPropertyChanged("DoesHaveAnnouncements");
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
	public string MembersText
	{
		get
		{
			return _membersText;
		}
		set
		{
			if (value != _membersText)
			{
				_membersText = value;
				((ViewModel)this).OnPropertyChanged("MembersText");
			}
		}
	}

	[DataSourceProperty]
	public string ChangeSigilText
	{
		get
		{
			return _changeSigilText;
		}
		set
		{
			if (value != _changeSigilText)
			{
				_changeSigilText = value;
				((ViewModel)this).OnPropertyChanged("ChangeSigilText");
			}
		}
	}

	[DataSourceProperty]
	public string ChangeFactionText
	{
		get
		{
			return _changeFactionText;
		}
		set
		{
			if (value != _changeFactionText)
			{
				_changeFactionText = value;
				((ViewModel)this).OnPropertyChanged("ChangeFactionText");
			}
		}
	}

	[DataSourceProperty]
	public string LeaveText
	{
		get
		{
			return _leaveText;
		}
		set
		{
			if (value != _leaveText)
			{
				_leaveText = value;
				((ViewModel)this).OnPropertyChanged("LeaveText");
			}
		}
	}

	[DataSourceProperty]
	public string DisbandText
	{
		get
		{
			return _disbandText;
		}
		set
		{
			if (value != _disbandText)
			{
				_disbandText = value;
				((ViewModel)this).OnPropertyChanged("DisbandText");
			}
		}
	}

	[DataSourceProperty]
	public string FactionCultureID
	{
		get
		{
			return _factionCultureID;
		}
		set
		{
			if (value != _factionCultureID)
			{
				_factionCultureID = value;
				((ViewModel)this).OnPropertyChanged("FactionCultureID");
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
	public string InformationText
	{
		get
		{
			return _informationText;
		}
		set
		{
			if (value != _informationText)
			{
				_informationText = value;
				((ViewModel)this).OnPropertyChanged("InformationText");
			}
		}
	}

	[DataSourceProperty]
	public string AnnouncementsText
	{
		get
		{
			return _announcementsText;
		}
		set
		{
			if (value != _announcementsText)
			{
				_announcementsText = value;
				((ViewModel)this).OnPropertyChanged("AnnouncementsText");
			}
		}
	}

	[DataSourceProperty]
	public string ClanDescriptionText
	{
		get
		{
			return _clanDescriptionText;
		}
		set
		{
			if (value != _clanDescriptionText)
			{
				_clanDescriptionText = value;
				((ViewModel)this).OnPropertyChanged("ClanDescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public string NoDescriptionText
	{
		get
		{
			return _noDescriptionText;
		}
		set
		{
			if (value != _noDescriptionText)
			{
				_noDescriptionText = value;
				((ViewModel)this).OnPropertyChanged("NoDescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public string NoAnnouncementsText
	{
		get
		{
			return _noAnnouncementsText;
		}
		set
		{
			if (value != _noAnnouncementsText)
			{
				_noAnnouncementsText = value;
				((ViewModel)this).OnPropertyChanged("NoAnnouncementsText");
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
				((ViewModel)this).OnPropertyChanged("TitleText");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM SigilImage
	{
		get
		{
			return _sigilImage;
		}
		set
		{
			if (value != _sigilImage)
			{
				_sigilImage = value;
				((ViewModel)this).OnPropertyChanged("SigilImage");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM FactionBanner
	{
		get
		{
			return _factionBanner;
		}
		set
		{
			if (value != _factionBanner)
			{
				_factionBanner = value;
				((ViewModel)this).OnPropertyChanged("FactionBanner");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanChangeSigilPopupVM ChangeSigilPopup
	{
		get
		{
			return _changeSigilPopup;
		}
		set
		{
			if (value != _changeSigilPopup)
			{
				_changeSigilPopup = value;
				((ViewModel)this).OnPropertyChanged("ChangeSigilPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanChangeFactionPopupVM ChangeFactionPopup
	{
		get
		{
			return _changeFactionPopup;
		}
		set
		{
			if (value != _changeFactionPopup)
			{
				_changeFactionPopup = value;
				((ViewModel)this).OnPropertyChanged("ChangeFactionPopup");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyClanAnnouncementVM> AnnouncementsList
	{
		get
		{
			return _announcementsList;
		}
		set
		{
			if (value != _announcementsList)
			{
				_announcementsList = value;
				((ViewModel)this).OnPropertyChanged("AnnouncementsList");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanSendPostPopupVM SendAnnouncementPopup
	{
		get
		{
			return _sendAnnouncementPopup;
		}
		set
		{
			if (value != _sendAnnouncementPopup)
			{
				_sendAnnouncementPopup = value;
				((ViewModel)this).OnPropertyChanged("SendAnnouncementPopup");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanSendPostPopupVM SetClanInformationPopup
	{
		get
		{
			return _setClanInformationPopup;
		}
		set
		{
			if (value != _setClanInformationPopup)
			{
				_setClanInformationPopup = value;
				((ViewModel)this).OnPropertyChanged("SetClanInformationPopup");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CantLeaveHint
	{
		get
		{
			return _cantLeaveHint;
		}
		set
		{
			if (value != _cantLeaveHint)
			{
				_cantLeaveHint = value;
				((ViewModel)this).OnPropertyChanged("CantLeaveHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel InviteMembersHint
	{
		get
		{
			return _inviteMembersHint;
		}
		set
		{
			if (value != _inviteMembersHint)
			{
				_inviteMembersHint = value;
				((ViewModel)this).OnPropertyChanged("InviteMembersHint");
			}
		}
	}

	public MPLobbyClanOverviewVM(Action openInviteClanMemberPopup)
	{
		_openInviteClanMemberPopup = openInviteClanMemberPopup;
		AnnouncementsList = new MBBindingList<MPLobbyClanAnnouncementVM>();
		ChangeSigilPopup = new MPLobbyClanChangeSigilPopupVM();
		ChangeFactionPopup = new MPLobbyClanChangeFactionPopupVM();
		SendAnnouncementPopup = new MPLobbyClanSendPostPopupVM(MPLobbyClanSendPostPopupVM.PostPopupMode.Announcement);
		SetClanInformationPopup = new MPLobbyClanSendPostPopupVM(MPLobbyClanSendPostPopupVM.PostPopupMode.Information);
		AreActionButtonsEnabled = true;
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
		//IL_006f: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		ChangeSigilText = ((object)new TextObject("{=7R0i82Nw}Change Sigil", (Dictionary<string, object>)null)).ToString();
		ChangeFactionText = ((object)new TextObject("{=aGGq9lJT}Change Culture", (Dictionary<string, object>)null)).ToString();
		LeaveText = ((object)new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null)).ToString();
		DisbandText = ((object)new TextObject("{=xXSFaGW8}Disband", (Dictionary<string, object>)null)).ToString();
		InformationText = ((object)new TextObject("{=SyklU5aP}Information", (Dictionary<string, object>)null)).ToString();
		AnnouncementsText = ((object)new TextObject("{=JY2pBVHQ}Announcements", (Dictionary<string, object>)null)).ToString();
		NoAnnouncementsText = ((object)new TextObject("{=0af2iQvw}Clan doesn't have any announcements", (Dictionary<string, object>)null)).ToString();
		NoDescriptionText = ((object)new TextObject("{=NwiYsUwm}Clan doesn't have a description", (Dictionary<string, object>)null)).ToString();
		TitleText = ((object)new TextObject("{=r223yChR}Overview", (Dictionary<string, object>)null)).ToString();
		CantLeaveHint = new HintViewModel(new TextObject("{=76HlhP7r}You have to give leadership to another member to leave", (Dictionary<string, object>)null), (string)null);
		InviteMembersHint = new HintViewModel(new TextObject("{=tSMckUw3}Invite Members", (Dictionary<string, object>)null), (string)null);
	}

	public async Task RefreshClanInformation(ClanHomeInfo info)
	{
		if (info == null || info.ClanInfo == null)
		{
			CloseAllPopups();
			return;
		}
		ClanInfo clanInfo = info.ClanInfo;
		GameTexts.SetVariable("STR", clanInfo.Tag);
		string clanTagInBrackets = ((object)new TextObject("{=uTXYEAOg}[{STR}]", (Dictionary<string, object>)null)).ToString();
		GameTexts.SetVariable("STR1", await PlatformServices.FilterString(clanInfo.Name, ((object)new TextObject("{=wNUcqcJP}Clan Name", (Dictionary<string, object>)null)).ToString()));
		GameTexts.SetVariable("STR2", clanTagInBrackets);
		NameText = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		GameTexts.SetVariable("LEFT", ((object)new TextObject("{=lBn2pSBL}Members", (Dictionary<string, object>)null)).ToString());
		GameTexts.SetVariable("RIGHT", clanInfo.Players.Length);
		MembersText = ((object)GameTexts.FindText("str_LEFT_colon_RIGHT", (string)null)).ToString();
		SigilImage = new BannerImageIdentifierVM(new Banner(clanInfo.Sigil), true);
		FactionCultureID = clanInfo.Faction;
		BasicCultureObject val = MBObjectManager.Instance.GetObject<BasicCultureObject>(FactionCultureID);
		CultureColor1 = Color.FromUint((val != null) ? val.Color : 0u);
		CultureColor2 = Color.FromUint((val != null) ? val.Color2 : 0u);
		if (NetworkMain.GameClient != null)
		{
			IsLeader = NetworkMain.GameClient.IsClanLeader;
			IsPrivilegedMember = IsLeader || NetworkMain.GameClient.IsClanOfficer;
		}
		else
		{
			Debug.FailedAssert("Game client is destroyed while updating clan home info", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Clan\\MPLobbyClanOverviewVM.cs", "RefreshClanInformation", 89);
			Debug.Print("Game client is destroyed while updating clan home info", 0, (DebugColor)12, 17592186044416uL);
			IsLeader = false;
			IsPrivilegedMember = false;
		}
		FactionBanner = new BannerImageIdentifierVM(val.Banner, true);
		ClanDescriptionText = clanInfo.InformationText;
		DoesHaveDescription = true;
		if (string.IsNullOrEmpty(clanInfo.InformationText))
		{
			DoesHaveDescription = false;
		}
		((Collection<MPLobbyClanAnnouncementVM>)(object)AnnouncementsList).Clear();
		ClanAnnouncement[] announcements = clanInfo.Announcements;
		ClanAnnouncement[] array = announcements;
		foreach (ClanAnnouncement val2 in array)
		{
			((Collection<MPLobbyClanAnnouncementVM>)(object)AnnouncementsList).Add(new MPLobbyClanAnnouncementVM(val2.AuthorId, val2.Announcement, val2.CreationTime, val2.Id, IsPrivilegedMember));
		}
		DoesHaveAnnouncements = true;
		if (Extensions.IsEmpty<ClanAnnouncement>((IEnumerable<ClanAnnouncement>)announcements))
		{
			DoesHaveAnnouncements = false;
		}
	}

	private void ExecuteDisbandClan()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		string? text = ((object)new TextObject("{=oFWcihyW}Disband Clan", (Dictionary<string, object>)null)).ToString();
		string text2 = ((object)new TextObject("{=vW1VgmaP}Are you sure want to disband your clan?", (Dictionary<string, object>)null)).ToString();
		InformationManager.ShowInquiry(new InquiryData(text, text2, true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)DisbandClan, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void DisbandClan()
	{
		NetworkMain.GameClient.DestroyClan();
	}

	private void ExecuteLeaveClan()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		string? text = ((object)new TextObject("{=4ZE6i9nW}Leave Clan", (Dictionary<string, object>)null)).ToString();
		string text2 = ((object)new TextObject("{=67hsZZor}Are you sure want to leave your clan?", (Dictionary<string, object>)null)).ToString();
		InformationManager.ShowInquiry(new InquiryData(text, text2, true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)LeaveClan, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void LeaveClan()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		NetworkMain.GameClient.KickFromClan(NetworkMain.GameClient.PlayerID);
	}

	private void ExecuteOpenChangeSigilPopup()
	{
		ChangeSigilPopup.ExecuteOpenPopup();
	}

	private void ExecuteCloseChangeSigilPopup()
	{
		ChangeSigilPopup.ExecuteClosePopup();
	}

	private void ExecuteOpenChangeFactionPopup()
	{
		ChangeFactionPopup.ExecuteOpenPopup();
	}

	private void ExecuteCloseChangeFactionPopup()
	{
		ChangeFactionPopup.ExecuteClosePopup();
	}

	private void ExecuteOpenSendAnnouncementPopup()
	{
		SendAnnouncementPopup.ExecuteOpenPopup();
	}

	private void ExecuteCloseSendAnnouncementPopup()
	{
		SendAnnouncementPopup.ExecuteClosePopup();
	}

	private void ExecuteOpenSetClanInformationPopup()
	{
		SetClanInformationPopup.ExecuteOpenPopup();
	}

	private void ExecuteCloseSetClanInformationPopup()
	{
		SetClanInformationPopup.ExecuteClosePopup();
	}

	private void ExecuteOpenInviteClanMemberPopup()
	{
		_openInviteClanMemberPopup?.Invoke();
	}

	private void CloseAllPopups()
	{
		ChangeSigilPopup.ExecuteClosePopup();
		ChangeFactionPopup.ExecuteClosePopup();
		SendAnnouncementPopup.ExecuteClosePopup();
		SetClanInformationPopup.ExecuteClosePopup();
	}
}
