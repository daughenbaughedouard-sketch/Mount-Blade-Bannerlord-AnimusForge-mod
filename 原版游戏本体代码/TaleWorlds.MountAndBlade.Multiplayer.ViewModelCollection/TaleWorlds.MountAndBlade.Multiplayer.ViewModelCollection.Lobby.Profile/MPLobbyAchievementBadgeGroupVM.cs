using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyAchievementBadgeGroupVM : ViewModel
{
	public readonly string GroupID;

	private readonly Action<MPLobbyAchievementBadgeGroupVM> _onBadgeProgressInfoRequested;

	private int _unlockedBadgeCount;

	private int _totalBadgeCount;

	private const string PlaytimeConditionID = "Playtime";

	private HotKey _inspectProgressKey;

	private bool _isProgressComplete;

	private string _progressCompletedText;

	private int _currentProgress;

	private int _totalProgress;

	private MPLobbyBadgeItemVM _shownBadgeItem;

	private MBBindingList<MPLobbyBadgeItemVM> _badges;

	[DataSourceProperty]
	public bool IsProgressComplete
	{
		get
		{
			return _isProgressComplete;
		}
		set
		{
			if (value != _isProgressComplete)
			{
				_isProgressComplete = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsProgressComplete");
				if (value)
				{
					SetProgressAsCompleted();
				}
			}
		}
	}

	[DataSourceProperty]
	public string ProgressCompletedText
	{
		get
		{
			return _progressCompletedText;
		}
		set
		{
			if (value != _progressCompletedText)
			{
				_progressCompletedText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ProgressCompletedText");
			}
		}
	}

	[DataSourceProperty]
	public int CurrentProgress
	{
		get
		{
			return _currentProgress;
		}
		set
		{
			if (value != _currentProgress)
			{
				_currentProgress = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CurrentProgress");
			}
		}
	}

	[DataSourceProperty]
	public int TotalProgress
	{
		get
		{
			return _totalProgress;
		}
		set
		{
			if (value != _totalProgress)
			{
				_totalProgress = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TotalProgress");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyBadgeItemVM ShownBadgeItem
	{
		get
		{
			return _shownBadgeItem;
		}
		set
		{
			if (value != _shownBadgeItem)
			{
				_shownBadgeItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyBadgeItemVM>(value, "ShownBadgeItem");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyBadgeItemVM> Badges
	{
		get
		{
			return _badges;
		}
		set
		{
			if (value != _badges)
			{
				_badges = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyBadgeItemVM>>(value, "Badges");
			}
		}
	}

	public MPLobbyAchievementBadgeGroupVM(string groupID, Action<MPLobbyAchievementBadgeGroupVM> onBadgeProgressInfoRequested)
	{
		_onBadgeProgressInfoRequested = onBadgeProgressInfoRequested;
		GroupID = groupID;
		Badges = new MBBindingList<MPLobbyBadgeItemVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		ProgressCompletedText = ((object)new TextObject("{=vlACTion}You've unlocked all badges!", (Dictionary<string, object>)null)).ToString();
	}

	public void RefreshKeyBindings(HotKey inspectProgressKey)
	{
		_inspectProgressKey = inspectProgressKey;
		foreach (MPLobbyBadgeItemVM item in (Collection<MPLobbyBadgeItemVM>)(object)Badges)
		{
			item.RefreshKeyBindings(inspectProgressKey);
		}
	}

	public void OnGroupBadgeAdded(MPLobbyBadgeItemVM badgeItem)
	{
		if (ShownBadgeItem == null)
		{
			ShownBadgeItem = badgeItem;
		}
		else if (badgeItem.IsEarned && badgeItem.Badge.Index > ShownBadgeItem.Badge.Index)
		{
			ShownBadgeItem = badgeItem;
		}
		badgeItem.SetGroup(this, _onBadgeProgressInfoRequested);
		_totalBadgeCount++;
		if (badgeItem.IsEarned)
		{
			_unlockedBadgeCount++;
		}
		IsProgressComplete = _totalBadgeCount == _unlockedBadgeCount;
		Badge badge = badgeItem.Badge;
		ConditionalBadge val;
		if ((val = (ConditionalBadge)(object)((badge is ConditionalBadge) ? badge : null)) != null && val.BadgeConditions.Count > 0 && !((Badge)val).IsTimed)
		{
			BadgeCondition val2 = val.BadgeConditions[0];
			if (val2.Parameters.TryGetValue("min_value", out var value) && int.TryParse(value, out var result))
			{
				int num = BadgeManager.GetBadgeConditionNumericValue(NetworkMain.GameClient.PlayerData, val2);
				if (val2.StringId.Equals("Playtime"))
				{
					result /= 3600;
					num /= 3600;
				}
				TotalProgress = Math.Max(TotalProgress, result);
				CurrentProgress = num;
			}
			else
			{
				SetProgressAsCompleted();
			}
		}
		else
		{
			SetProgressAsCompleted();
		}
		badgeItem.RefreshKeyBindings(_inspectProgressKey);
		((Collection<MPLobbyBadgeItemVM>)(object)Badges).Add(badgeItem);
	}

	private void SetProgressAsCompleted()
	{
		TotalProgress = 1;
		CurrentProgress = 1;
	}

	public void UpdateBadgeSelection()
	{
		foreach (MPLobbyBadgeItemVM item in (Collection<MPLobbyBadgeItemVM>)(object)Badges)
		{
			item.UpdateIsSelected();
		}
	}
}
