using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SandBox.Tournaments.MissionLogics;
using SandBox.ViewModelCollection.Input;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Tournament;

public class TournamentVM : ViewModel
{
	private readonly List<TournamentRoundVM> _rounds;

	private int _thisRoundBettedAmount;

	private bool _isPlayerParticipating;

	private InputKeyItemVM _doneInputKey;

	private InputKeyItemVM _cancelInputKey;

	private TournamentRoundVM _round1;

	private TournamentRoundVM _round2;

	private TournamentRoundVM _round3;

	private TournamentRoundVM _round4;

	private int _activeRoundIndex = -1;

	private string _joinTournamentText;

	private string _skipRoundText;

	private string _watchRoundText;

	private string _leaveText;

	private bool _canPlayerJoin;

	private TournamentMatchVM _currentMatch;

	private bool _isCurrentMatchActive;

	private string _betTitleText;

	private string _betDescriptionText;

	private string _betOddsText;

	private string _bettedDenarsText;

	private string _overallExpectedDenarsText;

	private string _currentExpectedDenarsText;

	private string _totalDenarsText;

	private string _acceptText;

	private string _cancelText;

	private string _prizeItemName;

	private string _tournamentPrizeText;

	private string _currentWagerText;

	private int _wageredDenars = -1;

	private int _expectedBetDenars = -1;

	private string _betText;

	private int _maximumBetValue;

	private string _tournamentWinnerTitle;

	private TournamentParticipantVM _tournamentWinner;

	private string _tournamentTitle;

	private bool _isOver;

	private bool _hasPrizeItem;

	private bool _isWinnerHero;

	private bool _isBetWindowEnabled;

	private string _winnerIntro;

	private ItemImageIdentifierVM _prizeVisual;

	private BannerImageIdentifierVM _winnerBanner;

	private MBBindingList<TournamentRewardVM> _battleRewards;

	private HintViewModel _skipAllRoundsHint;

	public Action DisableUI { get; }

	public TournamentBehavior Tournament { get; }

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
	public string TournamentWinnerTitle
	{
		get
		{
			return _tournamentWinnerTitle;
		}
		set
		{
			if (value != _tournamentWinnerTitle)
			{
				_tournamentWinnerTitle = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TournamentWinnerTitle");
			}
		}
	}

	[DataSourceProperty]
	public TournamentParticipantVM TournamentWinner
	{
		get
		{
			return _tournamentWinner;
		}
		set
		{
			if (value != _tournamentWinner)
			{
				_tournamentWinner = value;
				((ViewModel)this).OnPropertyChangedWithValue<TournamentParticipantVM>(value, "TournamentWinner");
			}
		}
	}

	[DataSourceProperty]
	public int MaximumBetValue
	{
		get
		{
			return _maximumBetValue;
		}
		set
		{
			if (value != _maximumBetValue)
			{
				_maximumBetValue = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MaximumBetValue");
				_wageredDenars = -1;
				WageredDenars = 0;
			}
		}
	}

	[DataSourceProperty]
	public bool IsBetButtonEnabled
	{
		get
		{
			if (PlayerCanJoinMatch() && Tournament.GetMaximumBet() > _thisRoundBettedAmount)
			{
				return Hero.MainHero.Gold > 0;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public string BetText
	{
		get
		{
			return _betText;
		}
		set
		{
			if (value != _betText)
			{
				_betText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BetText");
			}
		}
	}

	[DataSourceProperty]
	public string BetTitleText
	{
		get
		{
			return _betTitleText;
		}
		set
		{
			if (value != _betTitleText)
			{
				_betTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BetTitleText");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentWagerText
	{
		get
		{
			return _currentWagerText;
		}
		set
		{
			if (value != _currentWagerText)
			{
				_currentWagerText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentWagerText");
			}
		}
	}

	[DataSourceProperty]
	public string BetDescriptionText
	{
		get
		{
			return _betDescriptionText;
		}
		set
		{
			if (value != _betDescriptionText)
			{
				_betDescriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BetDescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public ItemImageIdentifierVM PrizeVisual
	{
		get
		{
			return _prizeVisual;
		}
		set
		{
			if (value != _prizeVisual)
			{
				_prizeVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<ItemImageIdentifierVM>(value, "PrizeVisual");
			}
		}
	}

	[DataSourceProperty]
	public string PrizeItemName
	{
		get
		{
			return _prizeItemName;
		}
		set
		{
			if (value != _prizeItemName)
			{
				_prizeItemName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PrizeItemName");
			}
		}
	}

	[DataSourceProperty]
	public string TournamentPrizeText
	{
		get
		{
			return _tournamentPrizeText;
		}
		set
		{
			if (value != _tournamentPrizeText)
			{
				_tournamentPrizeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TournamentPrizeText");
			}
		}
	}

	[DataSourceProperty]
	public int WageredDenars
	{
		get
		{
			return _wageredDenars;
		}
		set
		{
			if (value != _wageredDenars)
			{
				_wageredDenars = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "WageredDenars");
				ExpectedBetDenars = ((_wageredDenars != 0) ? Tournament.GetExpectedDenarsForBet(_wageredDenars) : 0);
			}
		}
	}

	[DataSourceProperty]
	public int ExpectedBetDenars
	{
		get
		{
			return _expectedBetDenars;
		}
		set
		{
			if (value != _expectedBetDenars)
			{
				_expectedBetDenars = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ExpectedBetDenars");
			}
		}
	}

	[DataSourceProperty]
	public string BetOddsText
	{
		get
		{
			return _betOddsText;
		}
		set
		{
			if (value != _betOddsText)
			{
				_betOddsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BetOddsText");
			}
		}
	}

	[DataSourceProperty]
	public string BettedDenarsText
	{
		get
		{
			return _bettedDenarsText;
		}
		set
		{
			if (value != _bettedDenarsText)
			{
				_bettedDenarsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BettedDenarsText");
			}
		}
	}

	[DataSourceProperty]
	public string OverallExpectedDenarsText
	{
		get
		{
			return _overallExpectedDenarsText;
		}
		set
		{
			if (value != _overallExpectedDenarsText)
			{
				_overallExpectedDenarsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OverallExpectedDenarsText");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentExpectedDenarsText
	{
		get
		{
			return _currentExpectedDenarsText;
		}
		set
		{
			if (value != _currentExpectedDenarsText)
			{
				_currentExpectedDenarsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentExpectedDenarsText");
			}
		}
	}

	[DataSourceProperty]
	public string TotalDenarsText
	{
		get
		{
			return _totalDenarsText;
		}
		set
		{
			if (value != _totalDenarsText)
			{
				_totalDenarsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TotalDenarsText");
			}
		}
	}

	[DataSourceProperty]
	public string AcceptText
	{
		get
		{
			return _acceptText;
		}
		set
		{
			if (value != _acceptText)
			{
				_acceptText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AcceptText");
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
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCurrentMatchActive
	{
		get
		{
			return _isCurrentMatchActive;
		}
		set
		{
			_isCurrentMatchActive = value;
			((ViewModel)this).OnPropertyChangedWithValue(value, "IsCurrentMatchActive");
		}
	}

	[DataSourceProperty]
	public TournamentMatchVM CurrentMatch
	{
		get
		{
			return _currentMatch;
		}
		set
		{
			if (value == _currentMatch)
			{
				return;
			}
			TournamentMatchVM currentMatch = _currentMatch;
			if (currentMatch != null && currentMatch.IsValid)
			{
				_currentMatch.State = 2;
				_currentMatch.Refresh(forceRefresh: false);
				int num = _rounds.FindIndex((TournamentRoundVM r) => r.Matches.Any((TournamentMatchVM m) => m.Match == Tournament.LastMatch));
				if (num < Tournament.Rounds.Length - 1)
				{
					_rounds[num + 1].Initialize();
				}
			}
			_currentMatch = value;
			((ViewModel)this).OnPropertyChangedWithValue<TournamentMatchVM>(value, "CurrentMatch");
			if (_currentMatch != null)
			{
				_currentMatch.State = 1;
			}
		}
	}

	[DataSourceProperty]
	public bool IsTournamentIncomplete
	{
		get
		{
			if (Tournament != null)
			{
				return Tournament.CurrentMatch != null;
			}
			return true;
		}
		set
		{
		}
	}

	[DataSourceProperty]
	public int ActiveRoundIndex
	{
		get
		{
			return _activeRoundIndex;
		}
		set
		{
			if (value != _activeRoundIndex)
			{
				OnNewRoundStarted(_activeRoundIndex, value);
				_activeRoundIndex = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ActiveRoundIndex");
				RefreshBetProperties();
			}
		}
	}

	[DataSourceProperty]
	public bool CanPlayerJoin
	{
		get
		{
			return _canPlayerJoin;
		}
		set
		{
			if (value != _canPlayerJoin)
			{
				_canPlayerJoin = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanPlayerJoin");
			}
		}
	}

	[DataSourceProperty]
	public bool HasPrizeItem
	{
		get
		{
			return _hasPrizeItem;
		}
		set
		{
			if (value != _hasPrizeItem)
			{
				_hasPrizeItem = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasPrizeItem");
			}
		}
	}

	[DataSourceProperty]
	public string JoinTournamentText
	{
		get
		{
			return _joinTournamentText;
		}
		set
		{
			if (value != _joinTournamentText)
			{
				_joinTournamentText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "JoinTournamentText");
			}
		}
	}

	[DataSourceProperty]
	public string SkipRoundText
	{
		get
		{
			return _skipRoundText;
		}
		set
		{
			if (value != _skipRoundText)
			{
				_skipRoundText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SkipRoundText");
			}
		}
	}

	[DataSourceProperty]
	public string WatchRoundText
	{
		get
		{
			return _watchRoundText;
		}
		set
		{
			if (value != _watchRoundText)
			{
				_watchRoundText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "WatchRoundText");
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
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LeaveText");
			}
		}
	}

	[DataSourceProperty]
	public TournamentRoundVM Round1
	{
		get
		{
			return _round1;
		}
		set
		{
			if (value != _round1)
			{
				_round1 = value;
				((ViewModel)this).OnPropertyChangedWithValue<TournamentRoundVM>(value, "Round1");
			}
		}
	}

	[DataSourceProperty]
	public TournamentRoundVM Round2
	{
		get
		{
			return _round2;
		}
		set
		{
			if (value != _round2)
			{
				_round2 = value;
				((ViewModel)this).OnPropertyChangedWithValue<TournamentRoundVM>(value, "Round2");
			}
		}
	}

	[DataSourceProperty]
	public TournamentRoundVM Round3
	{
		get
		{
			return _round3;
		}
		set
		{
			if (value != _round3)
			{
				_round3 = value;
				((ViewModel)this).OnPropertyChangedWithValue<TournamentRoundVM>(value, "Round3");
			}
		}
	}

	[DataSourceProperty]
	public TournamentRoundVM Round4
	{
		get
		{
			return _round4;
		}
		set
		{
			if (value != _round4)
			{
				_round4 = value;
				((ViewModel)this).OnPropertyChangedWithValue<TournamentRoundVM>(value, "Round4");
			}
		}
	}

	[DataSourceProperty]
	public bool InitializationOver => true;

	[DataSourceProperty]
	public string TournamentTitle
	{
		get
		{
			return _tournamentTitle;
		}
		set
		{
			if (value != _tournamentTitle)
			{
				_tournamentTitle = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TournamentTitle");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOver
	{
		get
		{
			return _isOver;
		}
		set
		{
			if (_isOver != value)
			{
				_isOver = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsOver");
			}
		}
	}

	[DataSourceProperty]
	public string WinnerIntro
	{
		get
		{
			return _winnerIntro;
		}
		set
		{
			if (value != _winnerIntro)
			{
				_winnerIntro = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "WinnerIntro");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<TournamentRewardVM> BattleRewards
	{
		get
		{
			return _battleRewards;
		}
		set
		{
			if (value != _battleRewards)
			{
				_battleRewards = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<TournamentRewardVM>>(value, "BattleRewards");
			}
		}
	}

	[DataSourceProperty]
	public bool IsWinnerHero
	{
		get
		{
			return _isWinnerHero;
		}
		set
		{
			if (value != _isWinnerHero)
			{
				_isWinnerHero = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsWinnerHero");
			}
		}
	}

	[DataSourceProperty]
	public bool IsBetWindowEnabled
	{
		get
		{
			return _isBetWindowEnabled;
		}
		set
		{
			if (value != _isBetWindowEnabled)
			{
				_isBetWindowEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBetWindowEnabled");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM WinnerBanner
	{
		get
		{
			return _winnerBanner;
		}
		set
		{
			if (value != _winnerBanner)
			{
				_winnerBanner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "WinnerBanner");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel SkipAllRoundsHint
	{
		get
		{
			return _skipAllRoundsHint;
		}
		set
		{
			if (value != _skipAllRoundsHint)
			{
				_skipAllRoundsHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "SkipAllRoundsHint");
			}
		}
	}

	public TournamentVM(Action disableUI, TournamentBehavior tournamentBehavior)
	{
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		DisableUI = disableUI;
		CurrentMatch = new TournamentMatchVM();
		Round1 = new TournamentRoundVM();
		Round2 = new TournamentRoundVM();
		Round3 = new TournamentRoundVM();
		Round4 = new TournamentRoundVM();
		_rounds = new List<TournamentRoundVM> { Round1, Round2, Round3, Round4 };
		_tournamentWinner = new TournamentParticipantVM();
		Tournament = tournamentBehavior;
		WinnerIntro = ((object)GameTexts.FindText("str_tournament_winner_intro", (string)null)).ToString();
		BattleRewards = new MBBindingList<TournamentRewardVM>();
		for (int i = 0; i < _rounds.Count; i++)
		{
			_rounds[i].Initialize(Tournament.Rounds[i], GameTexts.FindText("str_tournament_round", i.ToString()));
		}
		Refresh();
		Tournament.TournamentEnd += OnTournamentEnd;
		PrizeVisual = (HasPrizeItem ? new ItemImageIdentifierVM(Tournament.TournamentGame.Prize, "") : new ItemImageIdentifierVM((ItemObject)null, ""));
		SkipAllRoundsHint = new HintViewModel();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		LeaveText = ((object)GameTexts.FindText("str_tournament_leave", (string)null)).ToString();
		SkipRoundText = ((object)GameTexts.FindText("str_tournament_skip_round", (string)null)).ToString();
		WatchRoundText = ((object)GameTexts.FindText("str_tournament_watch_round", (string)null)).ToString();
		JoinTournamentText = ((object)GameTexts.FindText("str_tournament_join_tournament", (string)null)).ToString();
		BetText = ((object)GameTexts.FindText("str_bet", (string)null)).ToString();
		AcceptText = ((object)GameTexts.FindText("str_accept", (string)null)).ToString();
		CancelText = ((object)GameTexts.FindText("str_cancel", (string)null)).ToString();
		TournamentWinnerTitle = ((object)GameTexts.FindText("str_tournament_winner_title", (string)null)).ToString();
		BetTitleText = ((object)GameTexts.FindText("str_wager", (string)null)).ToString();
		GameTexts.SetVariable("MAX_AMOUNT", Tournament.GetMaximumBet());
		GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
		BetDescriptionText = ((object)GameTexts.FindText("str_tournament_bet_description", (string)null)).ToString();
		TournamentPrizeText = ((object)GameTexts.FindText("str_tournament_prize", (string)null)).ToString();
		PrizeItemName = ((object)Tournament.TournamentGame.Prize.Name).ToString();
		MBTextManager.SetTextVariable("SETTLEMENT_NAME", Tournament.Settlement.Name, false);
		TournamentTitle = ((object)GameTexts.FindText("str_tournament", (string)null)).ToString();
		CurrentWagerText = ((object)GameTexts.FindText("str_tournament_current_wager", (string)null)).ToString();
		SkipAllRoundsHint.HintText = new TextObject("{=GaOE4bdd}Skip All Rounds", (Dictionary<string, object>)null);
		TournamentRoundVM round = _round1;
		if (round != null)
		{
			((ViewModel)round).RefreshValues();
		}
		TournamentRoundVM round2 = _round2;
		if (round2 != null)
		{
			((ViewModel)round2).RefreshValues();
		}
		TournamentRoundVM round3 = _round3;
		if (round3 != null)
		{
			((ViewModel)round3).RefreshValues();
		}
		TournamentRoundVM round4 = _round4;
		if (round4 != null)
		{
			((ViewModel)round4).RefreshValues();
		}
		TournamentMatchVM currentMatch = _currentMatch;
		if (currentMatch != null)
		{
			((ViewModel)currentMatch).RefreshValues();
		}
		TournamentParticipantVM tournamentWinner = _tournamentWinner;
		if (tournamentWinner != null)
		{
			((ViewModel)tournamentWinner).RefreshValues();
		}
	}

	public void ExecuteBet()
	{
		_thisRoundBettedAmount += WageredDenars;
		Tournament.PlaceABet(WageredDenars);
		RefreshBetProperties();
	}

	public void ExecuteJoinTournament()
	{
		if (PlayerCanJoinMatch())
		{
			Tournament.StartMatch();
			IsCurrentMatchActive = true;
			CurrentMatch.Refresh(forceRefresh: true);
			CurrentMatch.State = 3;
			DisableUI();
			IsCurrentMatchActive = true;
		}
	}

	public void ExecuteSkipRound()
	{
		if (IsTournamentIncomplete)
		{
			Tournament.SkipMatch();
		}
		Refresh();
	}

	public void ExecuteSkipAllRounds()
	{
		int num = 0;
		int num2 = Tournament.Rounds.Sum((TournamentRound r) => r.Matches.Length);
		while (!CanPlayerJoin)
		{
			TournamentRound currentRound = Tournament.CurrentRound;
			if (((currentRound != null) ? currentRound.CurrentMatch : null) != null && num < num2)
			{
				ExecuteSkipRound();
				num++;
				continue;
			}
			break;
		}
	}

	public void ExecuteWatchRound()
	{
		if (!PlayerCanJoinMatch())
		{
			Tournament.StartMatch();
			IsCurrentMatchActive = true;
			CurrentMatch.Refresh(forceRefresh: true);
			CurrentMatch.State = 3;
			DisableUI();
			IsCurrentMatchActive = true;
		}
	}

	public void ExecuteLeave()
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		if (CurrentMatch != null)
		{
			List<TournamentMatch> list = new List<TournamentMatch>();
			for (int i = Tournament.CurrentRoundIndex; i < Tournament.Rounds.Length; i++)
			{
				list.AddRange(Tournament.Rounds[i].Matches.Where((TournamentMatch x) => (int)x.State != 2));
			}
			if (list.Any((TournamentMatch x) => x.Participants.Any((TournamentParticipant y) => y.Character == CharacterObject.PlayerCharacter)))
			{
				InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_forfeit", (string)null)).ToString(), ((object)GameTexts.FindText("str_tournament_forfeit_game", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)EndTournamentMission, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
				return;
			}
		}
		EndTournamentMission();
	}

	private void EndTournamentMission()
	{
		Tournament.EndTournamentViaLeave();
		Mission.Current.EndMission();
	}

	private void RefreshBetProperties()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		TextObject val = new TextObject("{=L9GnQvsq}Stake: {BETTED_DENARS}", (Dictionary<string, object>)null);
		val.SetTextVariable("BETTED_DENARS", Tournament.BettedDenars);
		BettedDenarsText = ((object)val).ToString();
		TextObject val2 = new TextObject("{=xzzSaN4b}Expected: {OVERALL_EXPECTED_DENARS}", (Dictionary<string, object>)null);
		val2.SetTextVariable("OVERALL_EXPECTED_DENARS", Tournament.OverallExpectedDenars);
		OverallExpectedDenarsText = ((object)val2).ToString();
		TextObject val3 = new TextObject("{=yF5fpwNE}Total: {TOTAL}", (Dictionary<string, object>)null);
		val3.SetTextVariable("TOTAL", Tournament.PlayerDenars);
		TotalDenarsText = ((object)val3).ToString();
		((ViewModel)this).OnPropertyChanged("IsBetButtonEnabled");
		MaximumBetValue = MathF.Min(Tournament.GetMaximumBet() - _thisRoundBettedAmount, Hero.MainHero.Gold);
		GameTexts.SetVariable("NORMALIZED_EXPECTED_GOLD", (int)(Tournament.BetOdd * 100f));
		GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
		BetOddsText = ((object)GameTexts.FindText("str_tournament_bet_odd", (string)null)).ToString();
	}

	private void OnNewRoundStarted(int prevRoundIndex, int currentRoundIndex)
	{
		_isPlayerParticipating = Tournament.IsPlayerParticipating;
		_thisRoundBettedAmount = 0;
	}

	public void Refresh()
	{
		IsCurrentMatchActive = false;
		CurrentMatch = _rounds[Tournament.CurrentRoundIndex].Matches.Find((TournamentMatchVM m) => m.IsValid && m.Match == Tournament.CurrentMatch);
		ActiveRoundIndex = Tournament.CurrentRoundIndex;
		CanPlayerJoin = PlayerCanJoinMatch();
		((ViewModel)this).OnPropertyChanged("IsTournamentIncomplete");
		((ViewModel)this).OnPropertyChanged("InitializationOver");
		((ViewModel)this).OnPropertyChanged("IsBetButtonEnabled");
		HasPrizeItem = Tournament.TournamentGame.Prize != null && !IsOver;
	}

	private void OnTournamentEnd()
	{
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Expected O, but got Unknown
		TournamentParticipantVM[] array = Round4.Matches.Last((TournamentMatchVM m) => m.IsValid).GetParticipants().ToArray();
		TournamentParticipantVM tournamentParticipantVM = array[0];
		TournamentParticipantVM tournamentParticipantVM2 = array[1];
		TournamentWinner = Round4.Matches.Last((TournamentMatchVM m) => m.IsValid).GetParticipants().First((TournamentParticipantVM p) => p.Participant == Tournament.Winner);
		TournamentWinner.Refresh();
		if (((BasicCharacterObject)TournamentWinner.Participant.Character).IsHero)
		{
			Hero heroObject = TournamentWinner.Participant.Character.HeroObject;
			TournamentWinner.Character.ArmorColor1 = heroObject.MapFaction.Color;
			TournamentWinner.Character.ArmorColor2 = heroObject.MapFaction.Color2;
		}
		else
		{
			CultureObject culture = TournamentWinner.Participant.Character.Culture;
			TournamentWinner.Character.ArmorColor1 = ((BasicCultureObject)culture).Color;
			TournamentWinner.Character.ArmorColor2 = ((BasicCultureObject)culture).Color2;
		}
		IsWinnerHero = ((BasicCharacterObject)Tournament.Winner.Character).IsHero;
		if (IsWinnerHero)
		{
			WinnerBanner = new BannerImageIdentifierVM(Tournament.Winner.Character.HeroObject.ClanBanner, true);
		}
		if (((BasicCharacterObject)TournamentWinner.Participant.Character).IsPlayerCharacter)
		{
			TournamentParticipantVM tournamentParticipantVM3 = ((tournamentParticipantVM == TournamentWinner) ? tournamentParticipantVM2 : tournamentParticipantVM);
			GameTexts.SetVariable("TOURNAMENT_FINAL_OPPONENT", tournamentParticipantVM3.Name);
			WinnerIntro = ((object)GameTexts.FindText("str_tournament_result_won", (string)null)).ToString();
			if (Tournament.TournamentGame.TournamentWinRenown > 0f)
			{
				GameTexts.SetVariable("RENOWN", Tournament.TournamentGame.TournamentWinRenown.ToString("F1"));
				((Collection<TournamentRewardVM>)(object)BattleRewards).Add(new TournamentRewardVM(((object)GameTexts.FindText("str_tournament_renown", (string)null)).ToString()));
			}
			if (Tournament.TournamentGame.TournamentWinInfluence > 0f)
			{
				float tournamentWinInfluence = Tournament.TournamentGame.TournamentWinInfluence;
				TextObject val = GameTexts.FindText("str_tournament_influence", (string)null);
				val.SetTextVariable("INFLUENCE", tournamentWinInfluence.ToString("F1"));
				val.SetTextVariable("INFLUENCE_ICON", "{=!}<img src=\"General\\Icons\\Influence@2x\" extend=\"5\">");
				((Collection<TournamentRewardVM>)(object)BattleRewards).Add(new TournamentRewardVM(((object)val).ToString()));
			}
			if (Tournament.TournamentGame.Prize != null)
			{
				string text = ((object)Tournament.TournamentGame.Prize.Name).ToString();
				GameTexts.SetVariable("REWARD", text);
				((Collection<TournamentRewardVM>)(object)BattleRewards).Add(new TournamentRewardVM(((object)GameTexts.FindText("str_tournament_reward", (string)null)).ToString(), new ItemImageIdentifierVM(Tournament.TournamentGame.Prize, "")));
			}
			if (Tournament.OverallExpectedDenars > 0)
			{
				int overallExpectedDenars = Tournament.OverallExpectedDenars;
				TextObject val2 = GameTexts.FindText("str_tournament_bet", (string)null);
				val2.SetTextVariable("BET", overallExpectedDenars);
				val2.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				((Collection<TournamentRewardVM>)(object)BattleRewards).Add(new TournamentRewardVM(((object)val2).ToString()));
			}
		}
		else if (((BasicCharacterObject)tournamentParticipantVM.Participant.Character).IsPlayerCharacter || ((BasicCharacterObject)tournamentParticipantVM2.Participant.Character).IsPlayerCharacter)
		{
			TournamentParticipantVM tournamentParticipantVM4 = ((tournamentParticipantVM == TournamentWinner) ? tournamentParticipantVM : tournamentParticipantVM2);
			GameTexts.SetVariable("TOURNAMENT_FINAL_OPPONENT", tournamentParticipantVM4.Name);
			WinnerIntro = ((object)GameTexts.FindText("str_tournament_result_eliminated_at_final", (string)null)).ToString();
		}
		else
		{
			int num = 3;
			bool num2 = Round3.GetParticipants().Any((TournamentParticipantVM p) => ((BasicCharacterObject)p.Participant.Character).IsPlayerCharacter);
			bool flag = Round2.GetParticipants().Any((TournamentParticipantVM p) => ((BasicCharacterObject)p.Participant.Character).IsPlayerCharacter);
			bool flag2 = Round1.GetParticipants().Any((TournamentParticipantVM p) => ((BasicCharacterObject)p.Participant.Character).IsPlayerCharacter);
			if (num2)
			{
				num = 3;
			}
			else if (flag)
			{
				num = 2;
			}
			else if (flag2)
			{
				num = 1;
			}
			bool flag3 = tournamentParticipantVM == TournamentWinner;
			GameTexts.SetVariable("TOURNAMENT_FINAL_PARTICIPANT_A", flag3 ? tournamentParticipantVM.Name : tournamentParticipantVM2.Name);
			GameTexts.SetVariable("TOURNAMENT_FINAL_PARTICIPANT_B", flag3 ? tournamentParticipantVM2.Name : tournamentParticipantVM.Name);
			if (_isPlayerParticipating)
			{
				GameTexts.SetVariable("TOURNAMENT_ELIMINATED_ROUND", num.ToString());
				WinnerIntro = ((object)GameTexts.FindText("str_tournament_result_eliminated", (string)null)).ToString();
			}
			else
			{
				WinnerIntro = ((object)GameTexts.FindText("str_tournament_result_spectator", (string)null)).ToString();
			}
		}
		IsOver = true;
	}

	private bool PlayerCanJoinMatch()
	{
		if (IsTournamentIncomplete)
		{
			return Tournament.CurrentMatch.Participants.Any((TournamentParticipant x) => x.Character == CharacterObject.PlayerCharacter);
		}
		return false;
	}

	public void OnAgentRemoved(Agent agent)
	{
		if (!IsCurrentMatchActive || !agent.IsHuman)
		{
			return;
		}
		TournamentParticipant participant = CurrentMatch.Match.GetParticipant(agent.Origin.UniqueSeed);
		if (participant != null)
		{
			CurrentMatch.GetParticipants().First((TournamentParticipantVM p) => p.Participant == participant).IsDead = true;
		}
	}

	public void ExecuteShowPrizeItemTooltip()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (HasPrizeItem)
		{
			InformationManager.ShowTooltip(typeof(ItemObject), new object[1] { (object)new EquipmentElement(Tournament.TournamentGame.Prize, (ItemModifier)null, (ItemObject)null, false) });
		}
	}

	public void ExecuteHidePrizeItemTooltip()
	{
		MBInformationManager.HideInformations();
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
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}

	public void SetCancelInputKey(HotKey hotKey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}
}
