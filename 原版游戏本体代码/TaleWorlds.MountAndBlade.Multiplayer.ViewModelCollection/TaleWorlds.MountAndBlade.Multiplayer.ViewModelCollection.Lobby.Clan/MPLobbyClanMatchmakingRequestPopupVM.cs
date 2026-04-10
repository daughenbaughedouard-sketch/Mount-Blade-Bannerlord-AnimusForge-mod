using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanMatchmakingRequestPopupVM : ViewModel
{
	private Guid _partyId;

	private bool _isEnabled;

	private bool _isClanMatch;

	private bool _isPracticeMatch;

	private string _titleText;

	private string _clanName;

	private string _wantsToJoinText;

	private string _doYouAcceptText;

	private BannerImageIdentifierVM _clanSigil;

	private MPLobbyPlayerBaseVM _challengerPartyLeader;

	private MBBindingList<MPLobbyPlayerBaseVM> _challengerPartyPlayers;

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
	public bool IsClanMatch
	{
		get
		{
			return _isClanMatch;
		}
		set
		{
			if (value != _isClanMatch)
			{
				_isClanMatch = value;
				((ViewModel)this).OnPropertyChanged("IsClanMatch");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPracticeMatch
	{
		get
		{
			return _isPracticeMatch;
		}
		set
		{
			if (value != _isPracticeMatch)
			{
				_isPracticeMatch = value;
				((ViewModel)this).OnPropertyChanged("IsPracticeMatch");
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
	public string ClanName
	{
		get
		{
			return _clanName;
		}
		set
		{
			if (value != _clanName)
			{
				_clanName = value;
				((ViewModel)this).OnPropertyChanged("ClanName");
			}
		}
	}

	[DataSourceProperty]
	public string WantsToJoinText
	{
		get
		{
			return _wantsToJoinText;
		}
		set
		{
			if (value != _wantsToJoinText)
			{
				_wantsToJoinText = value;
				((ViewModel)this).OnPropertyChanged("WantsToJoinText");
			}
		}
	}

	[DataSourceProperty]
	public string DoYouAcceptText
	{
		get
		{
			return _doYouAcceptText;
		}
		set
		{
			if (value != _doYouAcceptText)
			{
				_doYouAcceptText = value;
				((ViewModel)this).OnPropertyChanged("DoYouAcceptText");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM ClanSigil
	{
		get
		{
			return _clanSigil;
		}
		set
		{
			if (value != _clanSigil)
			{
				_clanSigil = value;
				((ViewModel)this).OnPropertyChanged("ClanSigil");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerBaseVM ChallengerPartyLeader
	{
		get
		{
			return _challengerPartyLeader;
		}
		set
		{
			if (value != _challengerPartyLeader)
			{
				_challengerPartyLeader = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPlayerBaseVM>(value, "ChallengerPartyLeader");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyPlayerBaseVM> ChallengerPartyPlayers
	{
		get
		{
			return _challengerPartyPlayers;
		}
		set
		{
			if (value != _challengerPartyPlayers)
			{
				_challengerPartyPlayers = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyPlayerBaseVM>>(value, "ChallengerPartyPlayers");
			}
		}
	}

	public MPLobbyClanMatchmakingRequestPopupVM()
	{
		ChallengerPartyPlayers = new MBBindingList<MPLobbyPlayerBaseVM>();
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
		TitleText = ((object)new TextObject("{=1pwQgr04}Matchmaking Request", (Dictionary<string, object>)null)).ToString();
		WantsToJoinText = ((object)new TextObject("{=WHKG5Rbq}This team wants to join the match you created.", (Dictionary<string, object>)null)).ToString();
		DoYouAcceptText = ((object)new TextObject("{=xkV9g4le}Do you accept them as your opponent?", (Dictionary<string, object>)null)).ToString();
	}

	public void OpenWith(string clanName, string clanSigilCode, Guid partyId, PlayerId[] challengerPlayerIDs, PlayerId challengerPartyLeaderID, PremadeGameType premadeGameType)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		((Collection<MPLobbyPlayerBaseVM>)(object)ChallengerPartyPlayers).Clear();
		IsClanMatch = false;
		IsPracticeMatch = false;
		_partyId = partyId;
		if ((int)premadeGameType == 1)
		{
			IsClanMatch = true;
			ClanName = clanName;
			ClanSigil = new BannerImageIdentifierVM(new Banner(clanSigilCode), true);
		}
		else if ((int)premadeGameType == 0)
		{
			IsPracticeMatch = true;
			ChallengerPartyLeader = new MPLobbyPlayerBaseVM(challengerPartyLeaderID);
			foreach (PlayerId id in challengerPlayerIDs)
			{
				((Collection<MPLobbyPlayerBaseVM>)(object)ChallengerPartyPlayers).Add(new MPLobbyPlayerBaseVM(id));
			}
		}
		IsEnabled = true;
	}

	public void Close()
	{
		IsEnabled = false;
	}

	public void ExecuteAcceptMatchmaking()
	{
		NetworkMain.GameClient.AcceptJoinPremadeGameRequest(_partyId);
		Close();
	}

	public void ExecuteDeclineMatchmaking()
	{
		NetworkMain.GameClient.DeclineJoinPremadeGameRequest(_partyId);
		Close();
	}
}
