using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPLobbyMenuVM : ViewModel
{
	private LobbyState _lobbyState;

	private readonly Action<bool> _setNavigationRestriction;

	private readonly Func<Task> _onQuit;

	private bool _isEnabled;

	private bool _hasProfileNotification;

	private bool _isClanSupported;

	private bool _isMatchmakingSupported;

	private int _pageIndex;

	private string _homeText;

	private string _matchmakingText;

	private string _profileText;

	private string _armoryText;

	private InputKeyItemVM _previousPageInputKey;

	private InputKeyItemVM _nextPageInputKey;

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
	public bool HasProfileNotification
	{
		get
		{
			return _hasProfileNotification;
		}
		set
		{
			if (value != _hasProfileNotification)
			{
				_hasProfileNotification = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasProfileNotification");
			}
		}
	}

	[DataSourceProperty]
	public bool IsClanSupported
	{
		get
		{
			return _isClanSupported;
		}
		set
		{
			if (value != _isClanSupported)
			{
				_isClanSupported = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsClanSupported");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMatchmakingSupported
	{
		get
		{
			return _isMatchmakingSupported;
		}
		set
		{
			if (value != _isMatchmakingSupported)
			{
				_isMatchmakingSupported = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMatchmakingSupported");
			}
		}
	}

	[DataSourceProperty]
	public int PageIndex
	{
		get
		{
			return _pageIndex;
		}
		set
		{
			if (value != _pageIndex)
			{
				_pageIndex = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PageIndex");
			}
		}
	}

	[DataSourceProperty]
	public string HomeText
	{
		get
		{
			return _homeText;
		}
		set
		{
			if (value != _homeText)
			{
				_homeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "HomeText");
			}
		}
	}

	[DataSourceProperty]
	public string MatchmakingText
	{
		get
		{
			return _matchmakingText;
		}
		set
		{
			if (value != _matchmakingText)
			{
				_matchmakingText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MatchmakingText");
			}
		}
	}

	[DataSourceProperty]
	public string ProfileText
	{
		get
		{
			return _profileText;
		}
		set
		{
			if (value != _profileText)
			{
				_profileText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ProfileText");
			}
		}
	}

	[DataSourceProperty]
	public string ArmoryText
	{
		get
		{
			return _armoryText;
		}
		set
		{
			if (value != _armoryText)
			{
				_armoryText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ArmoryText");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM PreviousPageInputKey
	{
		get
		{
			return _previousPageInputKey;
		}
		set
		{
			if (value != _previousPageInputKey)
			{
				_previousPageInputKey = value;
				((ViewModel)this).OnPropertyChanged("PreviousPageInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM NextPageInputKey
	{
		get
		{
			return _nextPageInputKey;
		}
		set
		{
			if (value != _nextPageInputKey)
			{
				_nextPageInputKey = value;
				((ViewModel)this).OnPropertyChanged("NextPageInputKey");
			}
		}
	}

	public MPLobbyMenuVM(LobbyState lobbyState, Action<bool> setNavigationRestriction, Func<Task> onQuit)
	{
		_lobbyState = lobbyState;
		_setNavigationRestriction = setNavigationRestriction;
		_onQuit = onQuit;
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
		((ViewModel)this).RefreshValues();
		HomeText = ((object)new TextObject("{=hometab}Home", (Dictionary<string, object>)null)).ToString();
		MatchmakingText = ((object)new TextObject("{=playgame}Play", (Dictionary<string, object>)null)).ToString();
		ProfileText = ((object)new TextObject("{=0647tsif}Profile", (Dictionary<string, object>)null)).ToString();
		ArmoryText = ((object)new TextObject("{=kG0xuyfE}Armory", (Dictionary<string, object>)null)).ToString();
		PreviousPageInputKey = InputKeyItemVM.CreateFromHotKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"), true);
		NextPageInputKey = InputKeyItemVM.CreateFromHotKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"), true);
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		((ViewModel)PreviousPageInputKey).OnFinalize();
		((ViewModel)NextPageInputKey).OnFinalize();
	}

	public void SetPage(MPLobbyVM.LobbyPage lobbyPage)
	{
		PageIndex = (int)lobbyPage;
	}

	private void ExecuteHome()
	{
		_lobbyState.OnActivateHome();
	}

	private void ExecuteMatchmaking()
	{
		_lobbyState.OnActivateMatchmaking();
	}

	private void ExecuteCustomServer()
	{
		_lobbyState.OnActivateCustomServer();
	}

	private void ExecuteArmory()
	{
		_lobbyState.OnActivateArmory();
	}

	private void ExecuteOptions()
	{
		_lobbyState.OnActivateOptions();
	}

	private void ExecuteProfile()
	{
		_lobbyState.OnActivateProfile();
	}

	public async void ExecuteExit()
	{
		await (_onQuit?.Invoke());
	}

	public void OnSupportedFeaturesRefreshed(SupportedFeatures supportedFeatures)
	{
		IsMatchmakingSupported = supportedFeatures.SupportsFeatures((Features)1);
	}
}
