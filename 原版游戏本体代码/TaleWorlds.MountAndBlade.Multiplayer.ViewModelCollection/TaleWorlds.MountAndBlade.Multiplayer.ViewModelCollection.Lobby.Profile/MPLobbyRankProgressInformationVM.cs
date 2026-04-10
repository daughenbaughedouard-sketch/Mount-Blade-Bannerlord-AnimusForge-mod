using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.Ranked;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyRankProgressInformationVM : ViewModel
{
	private MPLobbyPlayerBaseVM _basePlayer;

	private readonly Func<string> _getExitText;

	private TextObject _ratingRemainingTitleTextObject = new TextObject("{=7gQkFJqA}{RATING} points remaining to next rank", (Dictionary<string, object>)null);

	private TextObject _finalRankTextObject = new TextObject("{=6mZymVS8}You are at the final rank", (Dictionary<string, object>)null);

	private TextObject _evaluationTextObject = new TextObject("{=Ise5gWw3}{PLAYED_GAMES} / {TOTAL_GAMES} Evaluation matches played", (Dictionary<string, object>)null);

	private bool _isEnabled;

	private bool _isAtFinalRank;

	private bool _isEvaluating;

	private string _titleText;

	private string _clickToCloseText;

	private string _currentRankTitleText;

	private string _ratingRemainingTitleText;

	private string _currentRankID;

	private string _previousRankID;

	private string _nextRankID;

	private int _currentRating;

	private int _nextRankRating;

	private int _ratingRatio;

	private MPLobbyPlayerBaseVM _player;

	private MBBindingList<StringPairItemVM> _allRanks;

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
	public bool IsAtFinalRank
	{
		get
		{
			return _isAtFinalRank;
		}
		set
		{
			if (value != _isAtFinalRank)
			{
				_isAtFinalRank = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAtFinalRank");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEvaluating
	{
		get
		{
			return _isEvaluating;
		}
		set
		{
			if (value != _isEvaluating)
			{
				_isEvaluating = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEvaluating");
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
	public string ClickToCloseText
	{
		get
		{
			return _clickToCloseText;
		}
		set
		{
			if (value != _clickToCloseText)
			{
				_clickToCloseText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ClickToCloseText");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentRankTitleText
	{
		get
		{
			return _currentRankTitleText;
		}
		set
		{
			if (value != _currentRankTitleText)
			{
				_currentRankTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentRankTitleText");
			}
		}
	}

	[DataSourceProperty]
	public string RatingRemainingTitleText
	{
		get
		{
			return _ratingRemainingTitleText;
		}
		set
		{
			if (value != _ratingRemainingTitleText)
			{
				_ratingRemainingTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RatingRemainingTitleText");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentRankID
	{
		get
		{
			return _currentRankID;
		}
		set
		{
			if (value != _currentRankID)
			{
				_currentRankID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentRankID");
			}
		}
	}

	[DataSourceProperty]
	public string PreviousRankID
	{
		get
		{
			return _previousRankID;
		}
		set
		{
			if (value != _previousRankID)
			{
				_previousRankID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PreviousRankID");
			}
		}
	}

	[DataSourceProperty]
	public string NextRankID
	{
		get
		{
			return _nextRankID;
		}
		set
		{
			if (value != _nextRankID)
			{
				_nextRankID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextRankID");
			}
		}
	}

	[DataSourceProperty]
	public int CurrentRating
	{
		get
		{
			return _currentRating;
		}
		set
		{
			if (value != _currentRating)
			{
				_currentRating = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CurrentRating");
			}
		}
	}

	[DataSourceProperty]
	public int NextRankRating
	{
		get
		{
			return _nextRankRating;
		}
		set
		{
			if (value != _nextRankRating)
			{
				_nextRankRating = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NextRankRating");
			}
		}
	}

	[DataSourceProperty]
	public int RatingRatio
	{
		get
		{
			return _ratingRatio;
		}
		set
		{
			if (value != _ratingRatio)
			{
				_ratingRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "RatingRatio");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerBaseVM Player
	{
		get
		{
			return _player;
		}
		set
		{
			if (value != _player)
			{
				_player = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyPlayerBaseVM>(value, "Player");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemVM> AllRanks
	{
		get
		{
			return _allRanks;
		}
		set
		{
			if (value != _allRanks)
			{
				_allRanks = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemVM>>(value, "AllRanks");
			}
		}
	}

	public MPLobbyRankProgressInformationVM(Func<string> getExitText)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		_getExitText = getExitText;
		AllRanks = new MBBindingList<StringPairItemVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=XEGaQB2G}Rank Progression", (Dictionary<string, object>)null)).ToString();
		((Collection<StringPairItemVM>)(object)AllRanks).Clear();
		string[] rankIds = Ranks.RankIds;
		foreach (string rank in rankIds)
		{
			((Collection<StringPairItemVM>)(object)AllRanks).Add(new StringPairItemVM(rank, string.Empty, new BasicTooltipViewModel((Func<string>)(() => MPLobbyVM.GetLocalizedRankName(rank)))));
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		ExecuteClosePopup();
	}

	public void OpenWith(MPLobbyPlayerBaseVM player)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		IsEnabled = true;
		ClickToCloseText = _getExitText?.Invoke();
		if (player.RankInfo == null)
		{
			Debug.FailedAssert("Can't request rank progression information of another player.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Profile\\MPLobbyRankProgressInformationVM.cs", "OpenWith", 54);
			return;
		}
		_basePlayer = player;
		Player = new MPLobbyPlayerBaseVM(player.ProvidedID);
		Player.UpdateRating(OnRatingReceived);
	}

	private void OnRatingReceived()
	{
		Player?.RefreshSelectableGameTypes(isRankedOnly: true, RefreshRankInfo, ((IEnumerable<MPLobbyGameTypeVM>)_basePlayer.GameTypes).FirstOrDefault((MPLobbyGameTypeVM gt) => gt.IsSelected)?.GameTypeID);
	}

	private void RefreshRankInfo(string gameType)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		GameTypeRankInfo val = Player.RankInfo?.FirstOrDefault((Func<GameTypeRankInfo, bool>)((GameTypeRankInfo r) => r.GameType == gameType));
		if (val != null && val.RankBarInfo != null)
		{
			RankBarInfo rankBarInfo = val.RankBarInfo;
			CurrentRankID = rankBarInfo.RankId;
			CurrentRankTitleText = MPLobbyVM.GetLocalizedRankName(CurrentRankID);
			CurrentRating = rankBarInfo.Rating;
			NextRankRating = CurrentRating + rankBarInfo.RatingToNextRank;
			AllRanks.ApplyActionOnAllItems((Action<StringPairItemVM>)delegate(StringPairItemVM r)
			{
				r.Value = " ";
			});
			StringPairItemVM val2 = ((IEnumerable<StringPairItemVM>)AllRanks).FirstOrDefault((Func<StringPairItemVM, bool>)((StringPairItemVM r) => r.Definition == CurrentRankID));
			if (val2 != null)
			{
				val2.Value = ((object)new TextObject("{=sWnQva5O}Current Rank", (Dictionary<string, object>)null)).ToString();
			}
			IsAtFinalRank = string.IsNullOrEmpty(rankBarInfo.NextRankId);
			IsEvaluating = rankBarInfo.IsEvaluating;
			if (rankBarInfo.IsEvaluating)
			{
				RatingRatio = MathF.Floor((float)rankBarInfo.EvaluationMatchesPlayed / (float)rankBarInfo.TotalEvaluationMatchesRequired * 100f);
				NextRankID = string.Empty;
				PreviousRankID = string.Empty;
				CurrentRating = rankBarInfo.EvaluationMatchesPlayed;
				NextRankRating = rankBarInfo.TotalEvaluationMatchesRequired;
				_evaluationTextObject.SetTextVariable("PLAYED_GAMES", rankBarInfo.EvaluationMatchesPlayed);
				_evaluationTextObject.SetTextVariable("TOTAL_GAMES", rankBarInfo.TotalEvaluationMatchesRequired);
				RatingRemainingTitleText = ((object)_evaluationTextObject).ToString();
			}
			else if (IsAtFinalRank)
			{
				RatingRatio = 100;
				NextRankID = string.Empty;
				PreviousRankID = string.Empty;
				RatingRemainingTitleText = ((object)_finalRankTextObject).ToString();
			}
			else
			{
				RatingRatio = MathF.Floor(rankBarInfo.ProgressPercentage);
				NextRankID = rankBarInfo.NextRankId;
				PreviousRankID = rankBarInfo.PreviousRankId;
				_ratingRemainingTitleTextObject.SetTextVariable("RATING", rankBarInfo.RatingToNextRank);
				RatingRemainingTitleText = ((object)_ratingRemainingTitleTextObject).ToString();
			}
		}
		else
		{
			IsEnabled = false;
		}
	}

	public void ExecuteClosePopup()
	{
		Player = null;
		IsEnabled = false;
	}
}
