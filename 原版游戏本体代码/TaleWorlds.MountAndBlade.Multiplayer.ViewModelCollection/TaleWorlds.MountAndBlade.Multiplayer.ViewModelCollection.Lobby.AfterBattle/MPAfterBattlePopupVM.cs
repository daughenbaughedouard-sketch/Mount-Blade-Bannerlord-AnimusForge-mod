using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;
using TaleWorlds.MountAndBlade.Diamond.Ranked;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.AfterBattle;

public class MPAfterBattlePopupVM : ViewModel
{
	private enum AfterBattleState
	{
		None,
		GeneralProgression,
		LevelUp,
		RatingChange,
		RankChange
	}

	private AfterBattleState _currentState;

	private bool _hasLeveledUp;

	private int _oldExperience;

	private int _newExperience;

	private List<string> _earnedBadgeIDs;

	private int _lootGained;

	private bool _hasRatingChanged;

	private bool _hasRankChanged;

	private bool _hasFinishedEvaluation;

	private RankBarInfo _oldRankBarInfo;

	private RankBarInfo _newRankBarInfo;

	private string _battleResultsTitleText;

	private string _levelUpTitleText;

	private string _rankProgressTitleText;

	private string _promotedTitleText;

	private string _demotedTitleText;

	private string _evaluationFinishedTitleText;

	private TextObject _pointsGainedTextObj = new TextObject("{=EFU3uo0y}You've gained {POINTS} points", (Dictionary<string, object>)null);

	private TextObject _pointsLostTextObj = new TextObject("{=oMYz0PvL}You've lost {POINTS} points", (Dictionary<string, object>)null);

	private readonly Func<string> _getExitText;

	private bool _isEnabled;

	private bool _isShowingGeneralProgression;

	private bool _isShowingNewLevel;

	private bool _isShowingRankProgression;

	private bool _isShowingNewRank;

	private bool _hasLostRating;

	private string _titleText;

	private string _levelText;

	private string _experienceText;

	private string _clickToContinueText;

	private string _reachedLevelText;

	private string _pointsText;

	private string _pointChangeText;

	private string _oldRankID;

	private string _newRankID;

	private string _oldRankName;

	private string _newRankName;

	private int _initialRatio;

	private int _finalRatio;

	private int _numOfLevelUps;

	private int _gainedExperience;

	private int _levelsExperienceRequirment;

	private int _currentLevel;

	private int _nextLevel;

	private int _shownRating;

	private MBBindingList<MPAfterBattleRewardItemVM> _rewardsEarned;

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
	public bool IsShowingGeneralProgression
	{
		get
		{
			return _isShowingGeneralProgression;
		}
		set
		{
			if (value != _isShowingGeneralProgression)
			{
				_isShowingGeneralProgression = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsShowingGeneralProgression");
			}
		}
	}

	[DataSourceProperty]
	public bool IsShowingNewLevel
	{
		get
		{
			return _isShowingNewLevel;
		}
		set
		{
			if (value != _isShowingNewLevel)
			{
				_isShowingNewLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsShowingNewLevel");
			}
		}
	}

	[DataSourceProperty]
	public bool IsShowingRankProgression
	{
		get
		{
			return _isShowingRankProgression;
		}
		set
		{
			if (value != _isShowingRankProgression)
			{
				_isShowingRankProgression = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsShowingRankProgression");
			}
		}
	}

	[DataSourceProperty]
	public bool IsShowingNewRank
	{
		get
		{
			return _isShowingNewRank;
		}
		set
		{
			if (value != _isShowingNewRank)
			{
				_isShowingNewRank = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsShowingNewRank");
			}
		}
	}

	[DataSourceProperty]
	public bool HasLostRating
	{
		get
		{
			return _hasLostRating;
		}
		set
		{
			if (value != _hasLostRating)
			{
				_hasLostRating = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasLostRating");
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
	public string LevelText
	{
		get
		{
			return _levelText;
		}
		set
		{
			if (value != _levelText)
			{
				_levelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LevelText");
			}
		}
	}

	[DataSourceProperty]
	public string ExperienceText
	{
		get
		{
			return _experienceText;
		}
		set
		{
			if (value != _experienceText)
			{
				_experienceText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ExperienceText");
			}
		}
	}

	[DataSourceProperty]
	public string ClickToContinueText
	{
		get
		{
			return _clickToContinueText;
		}
		set
		{
			if (value != _clickToContinueText)
			{
				_clickToContinueText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ClickToContinueText");
			}
		}
	}

	[DataSourceProperty]
	public string ReachedLevelText
	{
		get
		{
			return _reachedLevelText;
		}
		set
		{
			if (value != _reachedLevelText)
			{
				_reachedLevelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ReachedLevelText");
			}
		}
	}

	[DataSourceProperty]
	public string PointsText
	{
		get
		{
			return _pointsText;
		}
		set
		{
			if (value != _pointsText)
			{
				_pointsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PointsText");
			}
		}
	}

	[DataSourceProperty]
	public string PointChangedText
	{
		get
		{
			return _pointChangeText;
		}
		set
		{
			if (value != _pointChangeText)
			{
				_pointChangeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PointChangedText");
			}
		}
	}

	[DataSourceProperty]
	public string OldRankID
	{
		get
		{
			return _oldRankID;
		}
		set
		{
			if (value != _oldRankID)
			{
				_oldRankID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OldRankID");
			}
		}
	}

	[DataSourceProperty]
	public string NewRankID
	{
		get
		{
			return _newRankID;
		}
		set
		{
			if (value != _newRankID)
			{
				_newRankID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NewRankID");
			}
		}
	}

	[DataSourceProperty]
	public string OldRankName
	{
		get
		{
			return _oldRankName;
		}
		set
		{
			if (value != _oldRankName)
			{
				_oldRankName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OldRankName");
			}
		}
	}

	[DataSourceProperty]
	public string NewRankName
	{
		get
		{
			return _newRankName;
		}
		set
		{
			if (value != _newRankName)
			{
				_newRankName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NewRankName");
			}
		}
	}

	[DataSourceProperty]
	public int FinalRatio
	{
		get
		{
			return _finalRatio;
		}
		set
		{
			if (value != _finalRatio)
			{
				_finalRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "FinalRatio");
			}
		}
	}

	[DataSourceProperty]
	public int NumOfLevelUps
	{
		get
		{
			return _numOfLevelUps;
		}
		set
		{
			if (value != _numOfLevelUps)
			{
				_numOfLevelUps = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NumOfLevelUps");
			}
		}
	}

	[DataSourceProperty]
	public int InitialRatio
	{
		get
		{
			return _initialRatio;
		}
		set
		{
			if (value != _initialRatio)
			{
				_initialRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "InitialRatio");
			}
		}
	}

	[DataSourceProperty]
	public int GainedExperience
	{
		get
		{
			return _gainedExperience;
		}
		set
		{
			if (value != _gainedExperience)
			{
				_gainedExperience = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "GainedExperience");
			}
		}
	}

	[DataSourceProperty]
	public int LevelsExperienceRequirment
	{
		get
		{
			return _levelsExperienceRequirment;
		}
		set
		{
			if (value != _levelsExperienceRequirment)
			{
				_levelsExperienceRequirment = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "LevelsExperienceRequirment");
			}
		}
	}

	[DataSourceProperty]
	public int NextLevel
	{
		get
		{
			return _nextLevel;
		}
		set
		{
			if (value != _nextLevel)
			{
				_nextLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NextLevel");
			}
		}
	}

	[DataSourceProperty]
	public int CurrentLevel
	{
		get
		{
			return _currentLevel;
		}
		set
		{
			if (value != _currentLevel)
			{
				_currentLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CurrentLevel");
			}
		}
	}

	[DataSourceProperty]
	public int ShownRating
	{
		get
		{
			return _shownRating;
		}
		set
		{
			if (value != _shownRating)
			{
				_shownRating = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShownRating");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPAfterBattleRewardItemVM> RewardsEarned
	{
		get
		{
			return _rewardsEarned;
		}
		set
		{
			if (value != _rewardsEarned)
			{
				_rewardsEarned = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPAfterBattleRewardItemVM>>(value, "RewardsEarned");
			}
		}
	}

	public MPAfterBattlePopupVM(Func<string> getExitText)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		_getExitText = getExitText;
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
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		_battleResultsTitleText = ((object)new TextObject("{=pguhTmXw}Battle Results", (Dictionary<string, object>)null)).ToString();
		_levelUpTitleText = ((object)new TextObject("{=0tUYng4e}Leveled Up!", (Dictionary<string, object>)null)).ToString();
		_rankProgressTitleText = ((object)new TextObject("{=XEGaQB2G}Rank Progression", (Dictionary<string, object>)null)).ToString();
		_promotedTitleText = ((object)new TextObject("{=bn0v5ST0}Promoted!", (Dictionary<string, object>)null)).ToString();
		_demotedTitleText = ((object)new TextObject("{=HUndnpNw}Demoted!", (Dictionary<string, object>)null)).ToString();
		_evaluationFinishedTitleText = ((object)new TextObject("{=2KZLf51A}Evaluation Matches Finished", (Dictionary<string, object>)null)).ToString();
		LevelText = ((object)GameTexts.FindText("str_level", (string)null)).ToString();
		ExperienceText = ((object)new TextObject("{=SwSaXwQg}exp", (Dictionary<string, object>)null)).ToString();
		PointsText = ((object)new TextObject("{=4dRTWSN3}Points", (Dictionary<string, object>)null)).ToString();
	}

	public void OpenWith(int oldExperience, int newExperience, List<string> badgesEarned, int lootGained, RankBarInfo oldRankBarInfo, RankBarInfo newRankBarInfo)
	{
		ClickToContinueText = _getExitText?.Invoke();
		_oldExperience = oldExperience;
		_newExperience = newExperience;
		_earnedBadgeIDs = badgesEarned;
		_lootGained = lootGained;
		_oldRankBarInfo = oldRankBarInfo;
		_newRankBarInfo = newRankBarInfo;
		_hasRatingChanged = oldRankBarInfo != null && newRankBarInfo != null && !oldRankBarInfo.IsEvaluating && !newRankBarInfo.IsEvaluating;
		_hasRankChanged = _hasRatingChanged && oldRankBarInfo.RankId != newRankBarInfo.RankId;
		_hasFinishedEvaluation = _oldRankBarInfo != null && _newRankBarInfo != null && _oldRankBarInfo.IsEvaluating && !_newRankBarInfo.IsEvaluating;
		AdvanceState();
		IsEnabled = true;
	}

	private void AdvanceState()
	{
		HideInfo();
		switch (_currentState)
		{
		case AfterBattleState.None:
			_currentState = AfterBattleState.GeneralProgression;
			ShowGeneralProgression();
			break;
		case AfterBattleState.GeneralProgression:
			if (_hasLeveledUp)
			{
				_currentState = AfterBattleState.LevelUp;
				ShowLevelUp();
			}
			else if (_hasRatingChanged)
			{
				_currentState = AfterBattleState.RatingChange;
				ShowRankProgression();
			}
			else if (_hasFinishedEvaluation)
			{
				_currentState = AfterBattleState.RankChange;
				ShowRankChange();
			}
			else
			{
				Disable();
			}
			break;
		case AfterBattleState.LevelUp:
			if (_hasRatingChanged)
			{
				_currentState = AfterBattleState.RatingChange;
				ShowRankProgression();
			}
			else if (_hasFinishedEvaluation)
			{
				_currentState = AfterBattleState.RankChange;
				ShowRankChange();
			}
			else
			{
				Disable();
			}
			break;
		case AfterBattleState.RatingChange:
			if (_hasRankChanged)
			{
				_currentState = AfterBattleState.RankChange;
				ShowRankChange();
			}
			else
			{
				Disable();
			}
			break;
		case AfterBattleState.RankChange:
			Disable();
			break;
		}
	}

	private void ShowGeneralProgression()
	{
		TitleText = _battleResultsTitleText;
		InitialRatio = 0;
		FinalRatio = 0;
		NumOfLevelUps = 0;
		PlayerDataExperience val = default(PlayerDataExperience);
		((PlayerDataExperience)(ref val))._002Ector(_oldExperience);
		PlayerDataExperience val2 = default(PlayerDataExperience);
		((PlayerDataExperience)(ref val2))._002Ector(_newExperience);
		GainedExperience = _newExperience - _oldExperience;
		CurrentLevel = ((PlayerDataExperience)(ref val)).Level;
		NextLevel = CurrentLevel + 1;
		InitialRatio = (int)((float)((PlayerDataExperience)(ref val)).ExperienceInCurrentLevel / (float)(((PlayerDataExperience)(ref val)).ExperienceToNextLevel + ((PlayerDataExperience)(ref val)).ExperienceInCurrentLevel) * 100f);
		FinalRatio = (int)((float)((PlayerDataExperience)(ref val2)).ExperienceInCurrentLevel / (float)(((PlayerDataExperience)(ref val2)).ExperienceToNextLevel + ((PlayerDataExperience)(ref val2)).ExperienceInCurrentLevel) * 100f);
		NumOfLevelUps = ((PlayerDataExperience)(ref val2)).Level - ((PlayerDataExperience)(ref val)).Level;
		_hasLeveledUp = NumOfLevelUps > 0;
		HasLostRating = GainedExperience < 0;
		float num = (float)NumOfLevelUps + (float)FinalRatio / 100f;
		LevelsExperienceRequirment = (int)((float)_newExperience / num);
		RewardsEarned = new MBBindingList<MPAfterBattleRewardItemVM>();
		foreach (string earnedBadgeID in _earnedBadgeIDs)
		{
			Badge byId = BadgeManager.GetById(earnedBadgeID);
			if (byId != null)
			{
				((Collection<MPAfterBattleRewardItemVM>)(object)RewardsEarned).Add((MPAfterBattleRewardItemVM)new MPAfterBattleBadgeRewardItemVM(byId));
			}
		}
		if (_lootGained > 0)
		{
			int num2 = _lootGained - _earnedBadgeIDs.Count * Parameters.LootRewardPerBadgeEarned;
			int additionalLootFromBadges = _lootGained - num2;
			((Collection<MPAfterBattleRewardItemVM>)(object)RewardsEarned).Add((MPAfterBattleRewardItemVM)new MPAfterBattleLootRewardItemVM(num2, additionalLootFromBadges));
		}
		IsShowingGeneralProgression = true;
	}

	private void ShowLevelUp()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		TitleText = _levelUpTitleText;
		PlayerDataExperience val = new PlayerDataExperience(_newExperience);
		int level = ((PlayerDataExperience)(ref val)).Level;
		GameTexts.SetVariable("STR1", GameTexts.FindText("str_level", (string)null));
		GameTexts.SetVariable("STR2", level);
		ReachedLevelText = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		SoundEvent.PlaySound2D("event:/ui/multiplayer/levelup");
		IsShowingNewLevel = true;
	}

	private void ShowRankProgression()
	{
		TitleText = _rankProgressTitleText;
		OldRankID = _oldRankBarInfo.RankId;
		NewRankID = _newRankBarInfo.RankId;
		OldRankName = MPLobbyVM.GetLocalizedRankName(OldRankID);
		NewRankName = MPLobbyVM.GetLocalizedRankName(NewRankID);
		HasLostRating = _oldRankBarInfo.Rating > _newRankBarInfo.Rating;
		ShownRating = _newRankBarInfo.Rating;
		InitialRatio = (int)_oldRankBarInfo.ProgressPercentage;
		FinalRatio = (int)_newRankBarInfo.ProgressPercentage;
		NumOfLevelUps = Extensions.IndexOf<string>(Ranks.RankIds, NewRankID) - Extensions.IndexOf<string>(Ranks.RankIds, OldRankID);
		if (HasLostRating)
		{
			_pointsLostTextObj.SetTextVariable("POINTS", _oldRankBarInfo.Rating - _newRankBarInfo.Rating);
			PointChangedText = ((object)_pointsLostTextObj).ToString();
		}
		else
		{
			_pointsGainedTextObj.SetTextVariable("POINTS", _newRankBarInfo.Rating - _oldRankBarInfo.Rating);
			PointChangedText = ((object)_pointsGainedTextObj).ToString();
		}
		IsShowingRankProgression = true;
	}

	private void ShowRankChange()
	{
		if (OldRankID != string.Empty && OldRankID != null)
		{
			bool flag = Extensions.IndexOf<string>(Ranks.RankIds, OldRankID) < Extensions.IndexOf<string>(Ranks.RankIds, NewRankID);
			TitleText = (flag ? _promotedTitleText : _demotedTitleText);
			IsShowingNewRank = true;
		}
		else if (_hasFinishedEvaluation)
		{
			OldRankID = string.Empty;
			NewRankID = _newRankBarInfo.RankId;
			OldRankName = MPLobbyVM.GetLocalizedRankName(OldRankID);
			NewRankName = MPLobbyVM.GetLocalizedRankName(NewRankID);
			TitleText = _evaluationFinishedTitleText;
			IsShowingNewRank = true;
		}
	}

	private void HideInfo()
	{
		IsShowingGeneralProgression = false;
		IsShowingNewLevel = false;
		IsShowingRankProgression = false;
		IsShowingNewRank = false;
	}

	private void Disable()
	{
		HideInfo();
		ShownRating = 0;
		_currentState = AfterBattleState.None;
		IsEnabled = false;
	}

	public void ExecuteClose()
	{
		if (IsEnabled)
		{
			AdvanceState();
		}
	}
}
