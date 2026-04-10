using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanInvitationPopupVM : ViewModel
{
	public enum InvitationMode
	{
		Creation,
		Invitation
	}

	private InvitationMode _invitationMode;

	private bool _isEnabled;

	private bool _isCreation;

	private string _titleText;

	private string _clanNameAndTag;

	private string _inviteReceivedText;

	private string _withPlayersText;

	private string _wantToJoinText;

	private MBBindingList<MPLobbyClanMemberItemVM> _partyMembersList;

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
				((ViewModel)this).OnPropertyChanged("IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCreation
	{
		get
		{
			return _isCreation;
		}
		set
		{
			if (value != _isCreation)
			{
				_isCreation = value;
				((ViewModel)this).OnPropertyChanged("IsCreation");
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
	public string ClanNameAndTag
	{
		get
		{
			return _clanNameAndTag;
		}
		set
		{
			if (value != _clanNameAndTag)
			{
				_clanNameAndTag = value;
				((ViewModel)this).OnPropertyChanged("ClanNameAndTag");
			}
		}
	}

	[DataSourceProperty]
	public string InviteReceivedText
	{
		get
		{
			return _inviteReceivedText;
		}
		set
		{
			if (value != _inviteReceivedText)
			{
				_inviteReceivedText = value;
				((ViewModel)this).OnPropertyChanged("InviteReceivedText");
			}
		}
	}

	[DataSourceProperty]
	public string WithPlayersText
	{
		get
		{
			return _withPlayersText;
		}
		set
		{
			if (value != _withPlayersText)
			{
				_withPlayersText = value;
				((ViewModel)this).OnPropertyChanged("WithPlayersText");
			}
		}
	}

	[DataSourceProperty]
	public string WantToJoinText
	{
		get
		{
			return _wantToJoinText;
		}
		set
		{
			if (value != _wantToJoinText)
			{
				_wantToJoinText = value;
				((ViewModel)this).OnPropertyChanged("WantToJoinText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyClanMemberItemVM> PartyMembersList
	{
		get
		{
			return _partyMembersList;
		}
		set
		{
			if (value != _partyMembersList)
			{
				_partyMembersList = value;
				((ViewModel)this).OnPropertyChanged("PartyMembersList");
			}
		}
	}

	public MPLobbyClanInvitationPopupVM()
	{
		PartyMembersList = new MBBindingList<MPLobbyClanMemberItemVM>();
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
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=D9zIAw9y}Clan Invite", (Dictionary<string, object>)null)).ToString();
		InviteReceivedText = ((object)new TextObject("{=wNAl9o4A}You received an invite from", (Dictionary<string, object>)null)).ToString();
		WantToJoinText = ((object)new TextObject("{=qa9aOxLm}Do you want to join this clan?", (Dictionary<string, object>)null)).ToString();
	}

	public void Open(string clanName, string clanTag, bool isCreation)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		GameTexts.SetVariable("STR", clanTag);
		string text = ((object)new TextObject("{=uTXYEAOg}[{STR}]", (Dictionary<string, object>)null)).ToString();
		GameTexts.SetVariable("STR1", clanName);
		GameTexts.SetVariable("STR2", text);
		ClanNameAndTag = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembersList).Clear();
		IsCreation = isCreation;
		if (isCreation)
		{
			_invitationMode = InvitationMode.Creation;
			foreach (PartyPlayerInLobbyClient item in NetworkMain.GameClient.PlayersInParty)
			{
				if (item.PlayerId != NetworkMain.GameClient.PlayerID)
				{
					MPLobbyClanMemberItemVM mPLobbyClanMemberItemVM = new MPLobbyClanMemberItemVM(item.PlayerId);
					mPLobbyClanMemberItemVM.InviteAcceptInfo = ((object)new TextObject("{=c0ZdKSkn}Waiting", (Dictionary<string, object>)null)).ToString();
					((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembersList).Add(mPLobbyClanMemberItemVM);
				}
			}
		}
		else
		{
			_invitationMode = InvitationMode.Invitation;
		}
		WithPlayersText = ((((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembersList).Count > 1) ? ((object)new TextObject("{=iCaRFZpG}along with these players", (Dictionary<string, object>)null)).ToString() : string.Empty);
		IsEnabled = true;
	}

	public void Close()
	{
		IsEnabled = false;
	}

	public void UpdateConfirmation(PlayerId playerId, ClanCreationAnswer answer)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		foreach (MPLobbyClanMemberItemVM item in (Collection<MPLobbyClanMemberItemVM>)(object)PartyMembersList)
		{
			if (item.ProvidedID == playerId)
			{
				if ((int)answer == 1)
				{
					item.InviteAcceptInfo = ((object)new TextObject("{=JTMegIk4}Accepted", (Dictionary<string, object>)null)).ToString();
				}
				else if ((int)answer == 2)
				{
					item.InviteAcceptInfo = ((object)new TextObject("{=FgaORzy5}Declined", (Dictionary<string, object>)null)).ToString();
				}
			}
		}
	}

	private void ExecuteAcceptInvitation()
	{
		IsEnabled = false;
		if (_invitationMode == InvitationMode.Creation)
		{
			NetworkMain.GameClient.AcceptClanCreationRequest();
		}
		else
		{
			NetworkMain.GameClient.AcceptClanInvitation();
		}
	}

	private void ExecuteDeclineInvitation()
	{
		IsEnabled = false;
		if (_invitationMode == InvitationMode.Creation)
		{
			NetworkMain.GameClient.DeclineClanCreationRequest();
		}
		else
		{
			NetworkMain.GameClient.DeclineClanInvitation();
		}
	}
}
