using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Authentication;

public class MPAuthenticationVM : ViewModel
{
	private readonly LobbyState _lobbyState;

	private bool? _hasPrivilege;

	private readonly TextObject _idleTitle = new TextObject("{=g1lgiwn1}Not Logged In", (Dictionary<string, object>)null);

	private readonly TextObject _idleMessage = new TextObject("{=saZ1OvPt}You can press the login button to establish connection", (Dictionary<string, object>)null);

	private readonly TextObject _noAccessMessage = new TextObject("{=9P0VL49j}You don't have access to multiplayer.", (Dictionary<string, object>)null);

	private readonly TextObject _loggingInTitle = new TextObject("{=iNqucBor}Logging In", (Dictionary<string, object>)null);

	private readonly TextObject _loggingInMessage = new TextObject("{=U4dzbzNb}Please wait while you are being connected to the server", (Dictionary<string, object>)null);

	private static readonly TextObject CantLogoutLoggingInTextObject = new TextObject("{=E0q43haK}Please wait until you are logged in.", (Dictionary<string, object>)null);

	private static readonly TextObject CantLogoutSearchingForMatchTextObject = new TextObject("{=DyeaObj5}Please cancel game search request before logging out.", (Dictionary<string, object>)null);

	private bool _isEnabled;

	private InputKeyItemVM _doneInputKey;

	private InputKeyItemVM _cancelInputKey;

	private bool _isLoginRequestActive;

	private bool _canTryLogin;

	private string _titleText;

	private string _messageText;

	private string _exitText;

	private string _loginText;

	private string _communityGamesText;

	private MPAuthenticationDebugVM _authenticationDebug;

	[DataSourceProperty]
	public InputKeyItemVM DoneInputKey
	{
		get
		{
			return _doneInputKey;
		}
		set
		{
			if (value != _doneInputKey)
			{
				_doneInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "DoneInputKey");
			}
		}
	}

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
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "CancelInputKey");
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
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEnabled");
				if (IsEnabled)
				{
					UpdatePrivilegeInformation();
				}
			}
		}
	}

	[DataSourceProperty]
	public bool IsLoginRequestActive
	{
		get
		{
			return _isLoginRequestActive;
		}
		set
		{
			if (value != _isLoginRequestActive)
			{
				_isLoginRequestActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsLoginRequestActive");
				UpdateCanTryLogin();
			}
		}
	}

	[DataSourceProperty]
	public bool CanTryLogin
	{
		get
		{
			return _canTryLogin;
		}
		set
		{
			if (value != _canTryLogin)
			{
				_canTryLogin = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanTryLogin");
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
	public string MessageText
	{
		get
		{
			return _messageText;
		}
		set
		{
			if (value != _messageText)
			{
				_messageText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MessageText");
			}
		}
	}

	[DataSourceProperty]
	public string ExitText
	{
		get
		{
			return _exitText;
		}
		set
		{
			if (value != _exitText)
			{
				_exitText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ExitText");
			}
		}
	}

	[DataSourceProperty]
	public string LoginText
	{
		get
		{
			return _loginText;
		}
		set
		{
			if (value != _loginText)
			{
				_loginText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LoginText");
			}
		}
	}

	[DataSourceProperty]
	public string CommunityGamesText
	{
		get
		{
			return _communityGamesText;
		}
		set
		{
			if (value != _communityGamesText)
			{
				_communityGamesText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CommunityGamesText");
			}
		}
	}

	[DataSourceProperty]
	public MPAuthenticationDebugVM AuthenticationDebug
	{
		get
		{
			return _authenticationDebug;
		}
		set
		{
			if (value != _authenticationDebug)
			{
				_authenticationDebug = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPAuthenticationDebugVM>(value, "AuthenticationDebug");
			}
		}
	}

	public MPAuthenticationVM(LobbyState lobbyState)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		_lobbyState = lobbyState;
		_hasPrivilege = _lobbyState.HasMultiplayerPrivilege;
		LobbyState lobbyState2 = _lobbyState;
		lobbyState2.OnMultiplayerPrivilegeUpdated = (Action<bool>)Delegate.Combine(lobbyState2.OnMultiplayerPrivilegeUpdated, new Action<bool>(OnMultiplayerPrivilegeUpdated));
		InternetAvailabilityChecker.OnInternetConnectionAvailabilityChanged = (Action<bool>)Delegate.Combine(InternetAvailabilityChecker.OnInternetConnectionAvailabilityChanged, new Action<bool>(OnInternetConnectionAvailabilityChanged));
		AuthenticationDebug = new MPAuthenticationDebugVM();
		AuthenticationDebug.IsEnabled = false;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		ExitText = ((object)new TextObject("{=exitMenuOption}Exit", (Dictionary<string, object>)null)).ToString();
		LoginText = ((object)new TextObject("{=lugGPVOb}Login", (Dictionary<string, object>)null)).ToString();
		TitleText = ((object)_idleTitle).ToString();
		MessageText = ((object)_idleMessage).ToString();
		CommunityGamesText = ((object)new TextObject("{=SIIgjILk}Community Games", (Dictionary<string, object>)null)).ToString();
		((ViewModel)AuthenticationDebug).RefreshValues();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM doneInputKey = DoneInputKey;
		if (doneInputKey != null)
		{
			((ViewModel)doneInputKey).OnFinalize();
		}
		InputKeyItemVM cancelInputKey = CancelInputKey;
		if (cancelInputKey != null)
		{
			((ViewModel)cancelInputKey).OnFinalize();
		}
		LobbyState lobbyState = _lobbyState;
		lobbyState.OnMultiplayerPrivilegeUpdated = (Action<bool>)Delegate.Remove(lobbyState.OnMultiplayerPrivilegeUpdated, new Action<bool>(OnMultiplayerPrivilegeUpdated));
		InternetAvailabilityChecker.OnInternetConnectionAvailabilityChanged = (Action<bool>)Delegate.Remove(InternetAvailabilityChecker.OnInternetConnectionAvailabilityChanged, new Action<bool>(OnInternetConnectionAvailabilityChanged));
	}

	public void OnTick(float dt)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		if (IsEnabled && _lobbyState != null)
		{
			State currentState = NetworkMain.GameClient.CurrentState;
			if (_hasPrivilege.HasValue && !_hasPrivilege.Value)
			{
				TitleText = ((object)_idleTitle).ToString();
				MessageText = ((object)_noAccessMessage).ToString();
			}
			else if (_lobbyState.IsLoggingIn)
			{
				IsLoginRequestActive = true;
				TitleText = ((object)_loggingInTitle).ToString();
				MessageText = ((object)_loggingInMessage).ToString();
			}
			else
			{
				IsLoginRequestActive = false;
				TitleText = ((object)_idleTitle).ToString();
				MessageText = ((object)_idleMessage).ToString();
			}
		}
	}

	public void ExecuteExit()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Invalid comparison between Unknown and I4
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Invalid comparison between Unknown and I4
		State currentState = NetworkMain.GameClient.CurrentState;
		if ((int)currentState == 0)
		{
			InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_exit", (string)null)).ToString(), ((object)GameTexts.FindText("str_mp_exit_query", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)delegate
			{
				OnExit();
			}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			return;
		}
		TextObject val = CantLogoutSearchingForMatchTextObject;
		if ((int)currentState == 1 || (int)currentState == 2 || (int)currentState == 3)
		{
			val = CantLogoutLoggingInTextObject;
		}
		InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_exit", (string)null)).ToString(), ((object)val).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void OnExit()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		LobbyState lobbyState = _lobbyState;
		if (lobbyState == null || !lobbyState.IsLoggingIn)
		{
			if ((int)Module.CurrentModule.StartupInfo.StartupType == 4)
			{
				MBInitialScreenBase.DoExitButtonAction();
			}
			else
			{
				Game.Current.GameStateManager.PopState(0);
			}
		}
	}

	public async void ExecuteLogin()
	{
		LobbyState lobbyState = _lobbyState;
		if (lobbyState == null || !lobbyState.IsLoggingIn)
		{
			try
			{
				await _lobbyState.TryLogin();
			}
			catch (Exception ex)
			{
				Debug.Print(ex.StackTrace ?? "", 0, (DebugColor)12, 17592186044416uL);
			}
		}
	}

	private void OnMultiplayerPrivilegeUpdated(bool hasPrivilege)
	{
		_hasPrivilege = hasPrivilege;
		UpdateCanTryLogin();
	}

	private void OnInternetConnectionAvailabilityChanged(bool isInternetAvailable)
	{
		UpdatePrivilegeInformation();
	}

	private void UpdateCanTryLogin()
	{
		CanTryLogin = _hasPrivilege == true && !_lobbyState.IsLoggingIn;
	}

	private void UpdatePrivilegeInformation()
	{
		_lobbyState?.UpdateHasMultiplayerPrivilege();
	}

	public void SetDoneInputKey(HotKey hotkey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}

	public void SetCancelInputKey(HotKey hotkey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}
}
