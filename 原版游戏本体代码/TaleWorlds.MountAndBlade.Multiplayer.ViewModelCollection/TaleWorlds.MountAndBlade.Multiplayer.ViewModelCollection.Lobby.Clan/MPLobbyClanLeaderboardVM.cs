using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanLeaderboardVM : ViewModel
{
	private ClanLeaderboardEntry[] _clans;

	private const int _clansPerPage = 30;

	private int _currentPageNumber;

	private bool _isEnabled;

	private bool _isDataLoading;

	private string _leaderboardText;

	private string _clansText;

	private string _nameText;

	private string _gamesWonText;

	private string _gamesLostText;

	private string _nextText;

	private string _previousText;

	private string _closeText;

	private MBBindingList<MPLobbyClanItemVM> _clanItems;

	private MPLobbyClanLeaderboardSortControllerVM _sortController;

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
	public bool IsDataLoading
	{
		get
		{
			return _isDataLoading;
		}
		set
		{
			if (value != _isDataLoading)
			{
				_isDataLoading = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDataLoading");
			}
		}
	}

	[DataSourceProperty]
	public string LeaderboardText
	{
		get
		{
			return _leaderboardText;
		}
		set
		{
			if (value != _leaderboardText)
			{
				_leaderboardText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LeaderboardText");
			}
		}
	}

	[DataSourceProperty]
	public string ClansText
	{
		get
		{
			return _clansText;
		}
		set
		{
			if (value != _clansText)
			{
				_clansText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ClansText");
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
	public string GamesWonText
	{
		get
		{
			return _gamesWonText;
		}
		set
		{
			if (value != _gamesWonText)
			{
				_gamesWonText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GamesWonText");
			}
		}
	}

	[DataSourceProperty]
	public string GamesLostText
	{
		get
		{
			return _gamesLostText;
		}
		set
		{
			if (value != _gamesLostText)
			{
				_gamesLostText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GamesLostText");
			}
		}
	}

	[DataSourceProperty]
	public string NextText
	{
		get
		{
			return _nextText;
		}
		set
		{
			if (value != _nextText)
			{
				_nextText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextText");
			}
		}
	}

	[DataSourceProperty]
	public string PreviousText
	{
		get
		{
			return _previousText;
		}
		set
		{
			if (value != _previousText)
			{
				_previousText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PreviousText");
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
	public MBBindingList<MPLobbyClanItemVM> ClanItems
	{
		get
		{
			return _clanItems;
		}
		set
		{
			if (value != _clanItems)
			{
				_clanItems = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyClanItemVM>>(value, "ClanItems");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClanLeaderboardSortControllerVM SortController
	{
		get
		{
			return _sortController;
		}
		set
		{
			if (value != _sortController)
			{
				_sortController = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyClanLeaderboardSortControllerVM>(value, "SortController");
			}
		}
	}

	public MPLobbyClanLeaderboardVM()
	{
		ClanItems = new MBBindingList<MPLobbyClanItemVM>();
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
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		CloseText = ((object)GameTexts.FindText("str_close", (string)null)).ToString();
		LeaderboardText = ((object)new TextObject("{=vGF5S2hE}Leaderboard", (Dictionary<string, object>)null)).ToString();
		ClansText = ((object)new TextObject("{=bfQLwMUp}Clans", (Dictionary<string, object>)null)).ToString();
		NameText = ((object)new TextObject("{=PDdh1sBj}Name", (Dictionary<string, object>)null)).ToString();
		GamesWonText = ((object)new TextObject("{=dxlkHhw5}Games Won", (Dictionary<string, object>)null)).ToString();
		GamesLostText = ((object)new TextObject("{=BrjpmaJH}Games Lost", (Dictionary<string, object>)null)).ToString();
		NextText = ((object)new TextObject("{=Rvr1bcu8}Next", (Dictionary<string, object>)null)).ToString();
		PreviousText = ((object)new TextObject("{=WXAaWZVf}Previous", (Dictionary<string, object>)null)).ToString();
	}

	private async void LoadClanLeaderboard()
	{
		IsDataLoading = true;
		ClanLeaderboardInfo val = await NetworkMain.GameClient.GetClanLeaderboardInfo();
		if (((val != null) ? val.ClanEntries : null) != null)
		{
			_clans = val.ClanEntries;
		}
		else
		{
			_clans = (ClanLeaderboardEntry[])(object)new ClanLeaderboardEntry[0];
		}
		SortController = new MPLobbyClanLeaderboardSortControllerVM(ref _clans, OnClansSorted);
		GoToPage(0);
		IsDataLoading = false;
	}

	private void OnClansSorted()
	{
		GoToPage(0);
	}

	private void GoToPage(int pageNumber)
	{
		int num = pageNumber * 30;
		if (_clans != null && num <= _clans.Length - 1)
		{
			((Collection<MPLobbyClanItemVM>)(object)ClanItems).Clear();
			for (int i = num; i < num + 30 && i != _clans.Length; i++)
			{
				ClanLeaderboardEntry val = _clans[i];
				((Collection<MPLobbyClanItemVM>)(object)ClanItems).Add(new MPLobbyClanItemVM(val.Name, val.Tag, val.Sigil, val.WinCount, val.LossCount, i + 1, val.ClanId.Equals(NetworkMain.GameClient.ClanID)));
			}
			_currentPageNumber = pageNumber;
		}
	}

	private void ExecuteGoToNextPage()
	{
		if (_currentPageNumber + 1 <= _clans.Length / 30)
		{
			GoToPage(_currentPageNumber + 1);
		}
		else
		{
			GoToPage(0);
		}
	}

	private void ExecuteGoToPreviousPage()
	{
		if (_currentPageNumber > 0)
		{
			GoToPage(_currentPageNumber - 1);
		}
		else
		{
			GoToPage(_clans.Length / 30);
		}
	}

	public void ExecuteOpenPopup()
	{
		IsEnabled = true;
		LoadClanLeaderboard();
	}

	public void ExecuteClosePopup()
	{
		IsEnabled = false;
	}
}
