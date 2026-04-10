using System.Collections.Generic;
using System.Collections.ObjectModel;
using Messages.FromLobbyServer.ToClient;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanCreationPopupVM : ViewModel
{
	private MPCultureItemVM _selectedFaction;

	private MPLobbySigilItemVM _selectedSigilIcon;

	private InputKeyItemVM _cancelInputKey;

	private bool _isEnabled;

	private bool _hasCreationStarted;

	private bool _isWaiting;

	private string _createClanText;

	private string _nameText;

	private string _nameErrorText;

	private string _tagText;

	private string _tagErrorText;

	private string _factionText;

	private string _factionErrorText;

	private string _sigilText;

	private string _sigilIconErrorText;

	private string _createText;

	private string _cancelText;

	private string _nameInputText;

	private string _tagInputText;

	private string _waitingForConfirmationText;

	private MBBindingList<MPCultureItemVM> _factionsList;

	private MBBindingList<MPLobbySigilItemVM> _iconsList;

	private MBBindingList<MPLobbyClanMemberItemVM> _partyMembersList;

	[DataSourceProperty]
	public InputKeyItemVM CancelInputKey
	{
		get
		{
			return _cancelInputKey;
		}
		set
		{
			if (value != _cancelInputKey)
			{
				_cancelInputKey = value;
				((ViewModel)this).OnPropertyChanged("CancelInputKey");
			}
		}
	}

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
	public bool HasCreationStarted
	{
		get
		{
			return _hasCreationStarted;
		}
		set
		{
			if (value != _hasCreationStarted)
			{
				_hasCreationStarted = value;
				((ViewModel)this).OnPropertyChanged("HasCreationStarted");
			}
		}
	}

	[DataSourceProperty]
	public bool IsWaiting
	{
		get
		{
			return _isWaiting;
		}
		set
		{
			if (value != _isWaiting)
			{
				_isWaiting = value;
				((ViewModel)this).OnPropertyChanged("IsWaiting");
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
	public string NameErrorText
	{
		get
		{
			return _nameErrorText;
		}
		set
		{
			if (value != _nameErrorText)
			{
				_nameErrorText = value;
				((ViewModel)this).OnPropertyChanged("NameErrorText");
			}
		}
	}

	[DataSourceProperty]
	public string TagText
	{
		get
		{
			return _tagText;
		}
		set
		{
			if (value != _tagText)
			{
				_tagText = value;
				((ViewModel)this).OnPropertyChanged("TagText");
			}
		}
	}

	[DataSourceProperty]
	public string TagErrorText
	{
		get
		{
			return _tagErrorText;
		}
		set
		{
			if (value != _tagErrorText)
			{
				_tagErrorText = value;
				((ViewModel)this).OnPropertyChanged("TagErrorText");
			}
		}
	}

	[DataSourceProperty]
	public string FactionText
	{
		get
		{
			return _factionText;
		}
		set
		{
			if (value != _factionText)
			{
				_factionText = value;
				((ViewModel)this).OnPropertyChanged("FactionText");
			}
		}
	}

	[DataSourceProperty]
	public string FactionErrorText
	{
		get
		{
			return _factionErrorText;
		}
		set
		{
			if (value != _factionErrorText)
			{
				_factionErrorText = value;
				((ViewModel)this).OnPropertyChanged("FactionErrorText");
			}
		}
	}

	[DataSourceProperty]
	public string SigilText
	{
		get
		{
			return _sigilText;
		}
		set
		{
			if (value != _sigilText)
			{
				_sigilText = value;
				((ViewModel)this).OnPropertyChanged("SigilText");
			}
		}
	}

	[DataSourceProperty]
	public string SigilIconErrorText
	{
		get
		{
			return _sigilIconErrorText;
		}
		set
		{
			if (value != _sigilIconErrorText)
			{
				_sigilIconErrorText = value;
				((ViewModel)this).OnPropertyChanged("SigilIconErrorText");
			}
		}
	}

	[DataSourceProperty]
	public string CreateText
	{
		get
		{
			return _createText;
		}
		set
		{
			if (value != _createText)
			{
				_createText = value;
				((ViewModel)this).OnPropertyChanged("CreateText");
			}
		}
	}

	[DataSourceProperty]
	public string CancelText
	{
		get
		{
			return _cancelText;
		}
		set
		{
			if (value != _cancelText)
			{
				_cancelText = value;
				((ViewModel)this).OnPropertyChanged("CancelText");
			}
		}
	}

	[DataSourceProperty]
	public string NameInputText
	{
		get
		{
			return _nameInputText;
		}
		set
		{
			if (value != _nameInputText)
			{
				_nameInputText = value;
				((ViewModel)this).OnPropertyChanged("NameInputText");
				NameErrorText = "";
			}
		}
	}

	[DataSourceProperty]
	public string TagInputText
	{
		get
		{
			return _tagInputText;
		}
		set
		{
			if (value != _tagInputText)
			{
				_tagInputText = value;
				((ViewModel)this).OnPropertyChanged("TagInputText");
				TagErrorText = "";
			}
		}
	}

	[DataSourceProperty]
	public string WaitingForConfirmationText
	{
		get
		{
			return _waitingForConfirmationText;
		}
		set
		{
			if (value != _waitingForConfirmationText)
			{
				_waitingForConfirmationText = value;
				((ViewModel)this).OnPropertyChanged("WaitingForConfirmationText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPCultureItemVM> FactionsList
	{
		get
		{
			return _factionsList;
		}
		set
		{
			if (value != _factionsList)
			{
				_factionsList = value;
				((ViewModel)this).OnPropertyChanged("FactionsList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbySigilItemVM> IconsList
	{
		get
		{
			return _iconsList;
		}
		set
		{
			if (value != _iconsList)
			{
				_iconsList = value;
				((ViewModel)this).OnPropertyChanged("IconsList");
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

	public MPLobbyClanCreationPopupVM()
	{
		PartyMembersList = new MBBindingList<MPLobbyClanMemberItemVM>();
		PrepareFactionsList();
		PrepareSigilIconsList();
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
		((ViewModel)this).RefreshValues();
		CreateClanText = ((object)new TextObject("{=ECb8IPbA}Create Clan", (Dictionary<string, object>)null)).ToString();
		NameText = ((object)new TextObject("{=PDdh1sBj}Name", (Dictionary<string, object>)null)).ToString();
		TagText = ((object)new TextObject("{=OUvFT99g}Tag", (Dictionary<string, object>)null)).ToString();
		FactionText = ((object)new TextObject("{=PUjDWe5j}Culture", (Dictionary<string, object>)null)).ToString();
		SigilText = ((object)new TextObject("{=P5Z9owOy}Sigil", (Dictionary<string, object>)null)).ToString();
		CreateText = ((object)new TextObject("{=65oGXBYQ}Create", (Dictionary<string, object>)null)).ToString();
		CancelText = ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString();
		WaitingForConfirmationText = ((object)new TextObject("{=08KLQa3P}Waiting For Party Members", (Dictionary<string, object>)null)).ToString();
		ResetAll();
	}

	private void ResetAll()
	{
		ResetErrorTexts();
		ResetUserInputs();
	}

	private void ResetErrorTexts()
	{
		NameErrorText = "";
		TagErrorText = "";
		FactionErrorText = "";
		SigilIconErrorText = "";
	}

	private void ResetUserInputs()
	{
		NameInputText = "";
		TagInputText = "";
		OnFactionSelection(null);
		OnSigilIconSelection(null);
	}

	public void ExecuteOpenPopup()
	{
		((ViewModel)this).RefreshValues();
		IsEnabled = true;
	}

	public void ExecuteClosePopup()
	{
		IsEnabled = false;
	}

	private void PrepareFactionsList()
	{
		_selectedFaction = null;
		MBBindingList<MPCultureItemVM> obj = new MBBindingList<MPCultureItemVM>();
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("vlandia")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("sturgia")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("empire")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("battania")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("khuzait")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("aserai")).StringId, OnFactionSelection));
		FactionsList = obj;
	}

	private void PrepareSigilIconsList()
	{
		IconsList = new MBBindingList<MPLobbySigilItemVM>();
		_selectedSigilIcon = null;
		foreach (BannerIconGroup item2 in (List<BannerIconGroup>)(object)BannerManager.Instance.BannerIconGroups)
		{
			if (item2.IsPattern)
			{
				continue;
			}
			foreach (KeyValuePair<int, BannerIconData> availableIcon in item2.AvailableIcons)
			{
				MPLobbySigilItemVM item = new MPLobbySigilItemVM(availableIcon.Key, OnSigilIconSelection);
				((Collection<MPLobbySigilItemVM>)(object)IconsList).Add(item);
			}
		}
	}

	private void PreparePartyMembersList()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		((Collection<MPLobbyClanMemberItemVM>)(object)PartyMembersList).Clear();
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

	private void OnFactionSelection(MPCultureItemVM faction)
	{
		if (faction != _selectedFaction)
		{
			if (_selectedFaction != null)
			{
				_selectedFaction.IsSelected = false;
			}
			_selectedFaction = faction;
			if (_selectedFaction != null)
			{
				_selectedFaction.IsSelected = true;
				FactionErrorText = "";
			}
		}
	}

	private void OnSigilIconSelection(MPLobbySigilItemVM sigilIcon)
	{
		if (sigilIcon != _selectedSigilIcon)
		{
			if (_selectedSigilIcon != null)
			{
				_selectedSigilIcon.IsSelected = false;
			}
			_selectedSigilIcon = sigilIcon;
			if (_selectedSigilIcon != null)
			{
				_selectedSigilIcon.IsSelected = true;
				SigilIconErrorText = "";
			}
		}
	}

	private void UpdateNameErrorText(StringValidationError error)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		NameErrorText = "";
		if ((int)error == 0)
		{
			NameErrorText = ((object)new TextObject("{=bExIl1A2}Name Length Is Invalid", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)error == 2)
		{
			NameErrorText = ((object)new TextObject("{=Agtv9l7S}This Name Already Exists", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)error == 1)
		{
			NameErrorText = ((object)new TextObject("{=lO1hok44}Name Has Invalid Characters In It", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)error == 3)
		{
			NameErrorText = ((object)new TextObject("{=cl2DnRYR}Name Should Not Contain Offensive Words", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)error == 4)
		{
			NameErrorText = ((object)new TextObject("{=UEgS8RcB}Name Has Invalid Content", (Dictionary<string, object>)null)).ToString();
		}
	}

	private void UpdateTagErrorText(StringValidationError error)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		TagErrorText = "";
		if ((int)error == 0)
		{
			TagErrorText = ((object)new TextObject("{=MjnlWhih}Tag Length Is Invalid", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)error == 2)
		{
			TagErrorText = ((object)new TextObject("{=ulzyykHO}This Tag Already Exists", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)error == 1)
		{
			TagErrorText = ((object)new TextObject("{=FjmxNxZJ}Tag Has Invalid Characters In It", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)error == 3)
		{
			TagErrorText = ((object)new TextObject("{=jyJXcOLe}Tag Should Not Contain Offensive Words", (Dictionary<string, object>)null)).ToString();
		}
		else if ((int)error == 4)
		{
			TagErrorText = ((object)new TextObject("{=hCNnqVgK}Tag Has Invalid Content", (Dictionary<string, object>)null)).ToString();
		}
	}

	public void UpdateFactionErrorText()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		FactionErrorText = "";
		FactionErrorText = ((object)new TextObject("{=p83IO9ls}You must select a culture", (Dictionary<string, object>)null)).ToString();
	}

	public void UpdateSigilIconErrorText()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		SigilIconErrorText = "";
		SigilIconErrorText = ((object)new TextObject("{=uOrwqeQl}You must select a sigil icon", (Dictionary<string, object>)null)).ToString();
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

	private BasicCultureObject GetSelectedCulture()
	{
		return Game.Current.ObjectManager.GetObject<BasicCultureObject>(_selectedFaction.CultureCode);
	}

	private Banner GetCreatedClanSigil()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		BasicCultureObject selectedCulture = GetSelectedCulture();
		Banner val = new Banner(selectedCulture.Banner, selectedCulture.BackgroundColor1, selectedCulture.ForegroundColor1);
		val.SetIconMeshId(_selectedSigilIcon.IconID);
		return val;
	}

	private async void ExecuteTryCreateClan()
	{
		bool areAllInputsValid = true;
		ResetErrorTexts();
		CheckClanParameterValidResult val = await NetworkMain.GameClient.ClanNameExists(NameInputText);
		if (!val.IsValid)
		{
			areAllInputsValid = false;
			UpdateNameErrorText(val.Error);
		}
		if (!(await PlatformServices.Instance.VerifyString(NameInputText)))
		{
			areAllInputsValid = false;
			UpdateNameErrorText((StringValidationError)4);
		}
		CheckClanParameterValidResult val2 = await NetworkMain.GameClient.ClanTagExists(TagInputText);
		if (!val2.IsValid)
		{
			areAllInputsValid = false;
			UpdateTagErrorText(val2.Error);
		}
		if (!(await PlatformServices.Instance.VerifyString(TagInputText)))
		{
			areAllInputsValid = false;
			UpdateTagErrorText((StringValidationError)4);
		}
		if (_selectedFaction == null)
		{
			areAllInputsValid = false;
			UpdateFactionErrorText();
		}
		if (_selectedSigilIcon == null)
		{
			areAllInputsValid = false;
			UpdateSigilIconErrorText();
		}
		if (areAllInputsValid)
		{
			HasCreationStarted = true;
			NetworkMain.GameClient.SendCreateClanMessage(NameInputText, TagInputText, ((MBObjectBase)GetSelectedCulture()).StringId, GetCreatedClanSigil().Serialize());
		}
	}

	public void ExecuteSwitchToWaiting()
	{
		PreparePartyMembersList();
		IsWaiting = true;
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM cancelInputKey = CancelInputKey;
		if (cancelInputKey != null)
		{
			((ViewModel)cancelInputKey).OnFinalize();
		}
	}

	public void SetCancelInputKey(HotKey hotKey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}
}
