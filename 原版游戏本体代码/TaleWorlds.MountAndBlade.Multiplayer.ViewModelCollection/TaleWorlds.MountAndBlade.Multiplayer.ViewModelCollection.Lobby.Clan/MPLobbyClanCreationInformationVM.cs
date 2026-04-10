using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanCreationInformationVM : ViewModel
{
	private Action _openClanCreationPopup;

	private bool _isEnabled;

	private bool _canCreateClan;

	private bool _doesHaveEnoughPlayersToCreateClan;

	private int _currentPlayerCount;

	private int _requiredPlayerCount;

	private string _createClanText;

	private string _createClanDescriptionText;

	private string _dontHaveEnoughPlayersInPartyText;

	private string _partyMemberCountText;

	private string _playerText;

	private string _createYourClanText;

	private string _closeText;

	private MBBindingList<MPLobbyClanMemberItemVM> _partyMembers;

	private HintViewModel _cantCreateHint;

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
	public bool CanCreateClan
	{
		get
		{
			return _canCreateClan;
		}
		set
		{
			if (value != _canCreateClan)
			{
				_canCreateClan = value;
				((ViewModel)this).OnPropertyChanged("CanCreateClan");
			}
		}
	}

	[DataSourceProperty]
	public bool DoesHaveEnoughPlayersToCreateClan
	{
		get
		{
			return _doesHaveEnoughPlayersToCreateClan;
		}
		set
		{
			if (value != _doesHaveEnoughPlayersToCreateClan)
			{
				_doesHaveEnoughPlayersToCreateClan = value;
				((ViewModel)this).OnPropertyChanged("DoesHaveEnoughPlayersToCreateClan");
			}
		}
	}

	[DataSourceProperty]
	public int CurrentPlayerCount
	{
		get
		{
			return _currentPlayerCount;
		}
		set
		{
			if (value != _currentPlayerCount)
			{
				_currentPlayerCount = value;
				((ViewModel)this).OnPropertyChanged("CurrentPlayerCount");
			}
		}
	}

	[DataSourceProperty]
	public int RequiredPlayerCount
	{
		get
		{
			return _requiredPlayerCount;
		}
		set
		{
			if (value != _requiredPlayerCount)
			{
				_requiredPlayerCount = value;
				((ViewModel)this).OnPropertyChanged("RequiredPlayerCount");
			}
		}
	}

	[DataSourceProperty]
	public string CreateClanText
	{
		get
		{
			return _createClanText;
		}
		set
		{
			if (value != _createClanText)
			{
				_createClanText = value;
				((ViewModel)this).OnPropertyChanged("CreateClanText");
			}
		}
	}

	[DataSourceProperty]
	public string CreateClanDescriptionText
	{
		get
		{
			return _createClanDescriptionText;
		}
		set
		{
			if (value != _createClanDescriptionText)
			{
				_createClanDescriptionText = value;
				((ViewModel)this).OnPropertyChanged("CreateClanDescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public string DontHaveEnoughPlayersInPartyText
	{
		get
		{
			return _dontHaveEnoughPlayersInPartyText;
		}
		set
		{
			if (value != _dontHaveEnoughPlayersInPartyText)
			{
				_dontHaveEnoughPlayersInPartyText = value;
				((ViewModel)this).OnPropertyChanged("DontHaveEnoughPlayersInPartyText");
			}
		}
	}

	[DataSourceProperty]
	public string PartyMemberCountText
	{
		get
		{
			return _partyMemberCountText;
		}
		set
		{
			if (value != _partyMemberCountText)
			{
				_partyMemberCountText = value;
				((ViewModel)this).OnPropertyChanged("PartyMemberCountText");
			}
		}
	}

	[DataSourceProperty]
	public string PlayerText
	{
		get
		{
			return _playerText;
		}
		set
		{
			if (value != _playerText)
			{
				_playerText = value;
				((ViewModel)this).OnPropertyChanged("PlayerText");
			}
		}
	}

	[DataSourceProperty]
	public string CreateYourClanText
	{
		get
		{
			return _createYourClanText;
		}
		set
		{
			if (value != _createYourClanText)
			{
				_createYourClanText = value;
				((ViewModel)this).OnPropertyChanged("CreateYourClanText");
			}
		}
	}

	[DataSourceProperty]
	public string CloseText
	{
		get
		{
			return _closeText;
		}
		set
		{
			if (value != _closeText)
			{
				_closeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CloseText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyClanMemberItemVM> PartyMembers
	{
		get
		{
			return _partyMembers;
		}
		set
		{
			if (value != _partyMembers)
			{
				_partyMembers = value;
				((ViewModel)this).OnPropertyChanged("PartyMembers");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CantCreateHint
	{
		get
		{
			return _cantCreateHint;
		}
		set
		{
			if (value != _cantCreateHint)
			{
				_cantCreateHint = value;
				((ViewModel)this).OnPropertyChanged("CantCreateHint");
			}
		}
	}

	public MPLobbyClanCreationInformationVM(Action openClanCreationPopup)
	{
		_openClanCreationPopup = openClanCreationPopup;
		PartyMembers = new MBBindingList<MPLobbyClanMemberItemVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
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
		((ViewModel)this).RefreshValues();
		CloseText = ((object)GameTexts.FindText("str_close", (string)null)).ToString();
		CreateClanText = ((object)new TextObject("{=ECb8IPbA}Create Clan", (Dictionary<string, object>)null)).ToString();
		CreateClanDescriptionText = ((object)new TextObject("{=aWzdkfvn}Currently you are not a member of a clan or you don't own a clan. You need to create a party from non-clan member players to form your own clan.", (Dictionary<string, object>)null)).ToString();
		CreateYourClanText = ((object)new TextObject("{=kF3b8cH1}Create Your Clan", (Dictionary<string, object>)null)).ToString();
		DontHaveEnoughPlayersInPartyText = ((object)new TextObject("{=bynNUfSr}Your party does not have enough members to create a clan.", (Dictionary<string, object>)null)).ToString();
		PlayerText = ((object)new TextObject("{=RN6zHak0}Player", (Dictionary<string, object>)null)).ToString();
	}

	public void RefreshWith(ClanHomeInfo info)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		if (info == null)
		{
			CanCreateClan = false;
			CantCreateHint = new HintViewModel(new TextObject("{=EQAjujjO}Clan creation information can't be retrieved", (Dictionary<string, object>)null), (string)null);
			DoesHaveEnoughPlayersToCreateClan = false;
			PartyMemberCountText = ((object)new TextObject("{=y1AGNqyV}Clan creation is not available", (Dictionary<string, object>)null)).ToString();
			((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembers).Clear();
			((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembers).Add(new MPLobbyClanMemberItemVM(NetworkMain.GameClient.PlayerID));
			return;
		}
		CanCreateClan = info.CanCreateClan && NetworkMain.GameClient.IsPartyLeader;
		CantCreateHint = new HintViewModel();
		if (!NetworkMain.GameClient.IsPartyLeader)
		{
			CantCreateHint = new HintViewModel(new TextObject("{=OiWquyWY}You have to be the leader of the party to create a clan", (Dictionary<string, object>)null), (string)null);
		}
		if (info.NotEnoughPlayersInfo == null)
		{
			DoesHaveEnoughPlayersToCreateClan = true;
		}
		else
		{
			CurrentPlayerCount = info.NotEnoughPlayersInfo.CurrentPlayerCount;
			RequiredPlayerCount = info.NotEnoughPlayersInfo.RequiredPlayerCount;
			DoesHaveEnoughPlayersToCreateClan = CurrentPlayerCount == RequiredPlayerCount;
			GameTexts.SetVariable("LEFT", CurrentPlayerCount);
			GameTexts.SetVariable("RIGHT", RequiredPlayerCount);
			GameTexts.SetVariable("STR1", ((object)GameTexts.FindText("str_LEFT_over_RIGHT", (string)null)).ToString());
			GameTexts.SetVariable("STR2", PlayerText);
			PartyMemberCountText = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		}
		((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembers).Clear();
		if (NetworkMain.GameClient.IsInParty)
		{
			foreach (PartyPlayerInLobbyClient item in NetworkMain.GameClient.PlayersInParty)
			{
				MPLobbyClanMemberItemVM mPLobbyClanMemberItemVM = new MPLobbyClanMemberItemVM(item.PlayerId);
				if (info.PlayerNotEligibleInfos != null)
				{
					PlayerNotEligibleInfo[] playerNotEligibleInfos = info.PlayerNotEligibleInfos;
					foreach (PlayerNotEligibleInfo val in playerNotEligibleInfos)
					{
						if (val.PlayerId == item.PlayerId)
						{
							PlayerNotEligibleError[] errors = val.Errors;
							foreach (PlayerNotEligibleError notEligibleInfo in errors)
							{
								mPLobbyClanMemberItemVM.SetNotEligibleInfo(notEligibleInfo);
							}
						}
					}
				}
				((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembers).Add(mPLobbyClanMemberItemVM);
			}
			return;
		}
		((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembers).Add(new MPLobbyClanMemberItemVM(NetworkMain.GameClient.PlayerID));
	}

	public void OnFriendListUpdated(bool forceUpdate = false)
	{
		foreach (MPLobbyClanMemberItemVM item in (Collection<MPLobbyClanMemberItemVM>)(object)PartyMembers)
		{
			item.UpdateNameAndAvatar(forceUpdate);
		}
	}

	public void OnPlayerNameUpdated()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembers).Count; i++)
		{
			MPLobbyClanMemberItemVM mPLobbyClanMemberItemVM = ((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembers)[i];
			if (mPLobbyClanMemberItemVM.Id == NetworkMain.GameClient.PlayerID)
			{
				mPLobbyClanMemberItemVM.UpdateNameAndAvatar(forceUpdate: true);
			}
		}
	}

	public void ExecuteOpenPopup()
	{
		IsEnabled = true;
	}

	public void ExecuteClosePopup()
	{
		IsEnabled = false;
	}

	private void ExecuteOpenClanCreationPopup()
	{
		ExecuteClosePopup();
		_openClanCreationPopup();
	}
}
