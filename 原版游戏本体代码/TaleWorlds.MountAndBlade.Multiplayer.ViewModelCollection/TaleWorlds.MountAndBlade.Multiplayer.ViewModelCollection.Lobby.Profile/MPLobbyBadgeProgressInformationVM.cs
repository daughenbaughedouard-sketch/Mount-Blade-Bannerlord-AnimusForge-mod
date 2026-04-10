using System;
using System.Collections.ObjectModel;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyBadgeProgressInformationVM : ViewModel
{
	private int _shownBadgeIndexOffset;

	private const int MaxShownBadgeCount = 5;

	private readonly Func<string> _getExitText;

	private InputKeyItemVM _previousTabInputKey;

	private InputKeyItemVM _nextTabInputKey;

	private int _shownBadgeCount;

	private bool _isEnabled;

	private bool _canIncreaseBadgeIndices;

	private bool _canDecreaseBadgeIndices;

	private string _clickToCloseText;

	private string _titleText;

	private MPLobbyAchievementBadgeGroupVM _badgeGroup;

	private MBBindingList<StringPairItemVM> _availableBadgeIDs;

	[DataSourceProperty]
	public InputKeyItemVM PreviousTabInputKey
	{
		get
		{
			return _previousTabInputKey;
		}
		set
		{
			if (value != _previousTabInputKey)
			{
				_previousTabInputKey = value;
				((ViewModel)this).OnPropertyChanged("PreviousTabInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM NextTabInputKey
	{
		get
		{
			return _nextTabInputKey;
		}
		set
		{
			if (value != _nextTabInputKey)
			{
				_nextTabInputKey = value;
				((ViewModel)this).OnPropertyChanged("NextTabInputKey");
			}
		}
	}

	[DataSourceProperty]
	public int ShownBadgeCount
	{
		get
		{
			return _shownBadgeCount;
		}
		set
		{
			if (value != _shownBadgeCount)
			{
				_shownBadgeCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShownBadgeCount");
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
			}
		}
	}

	[DataSourceProperty]
	public bool CanIncreaseBadgeIndices
	{
		get
		{
			return _canIncreaseBadgeIndices;
		}
		set
		{
			if (value != _canIncreaseBadgeIndices)
			{
				_canIncreaseBadgeIndices = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanIncreaseBadgeIndices");
			}
		}
	}

	[DataSourceProperty]
	public bool CanDecreaseBadgeIndices
	{
		get
		{
			return _canDecreaseBadgeIndices;
		}
		set
		{
			if (value != _canDecreaseBadgeIndices)
			{
				_canDecreaseBadgeIndices = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanDecreaseBadgeIndices");
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
	public MPLobbyAchievementBadgeGroupVM BadgeGroup
	{
		get
		{
			return _badgeGroup;
		}
		set
		{
			if (value != _badgeGroup)
			{
				_badgeGroup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyAchievementBadgeGroupVM>(value, "BadgeGroup");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemVM> AvailableBadgeIDs
	{
		get
		{
			return _availableBadgeIDs;
		}
		set
		{
			if (value != _availableBadgeIDs)
			{
				_availableBadgeIDs = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemVM>>(value, "AvailableBadgeIDs");
			}
		}
	}

	public MPLobbyBadgeProgressInformationVM(Func<string> getExitText)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		_getExitText = getExitText;
		AvailableBadgeIDs = new MBBindingList<StringPairItemVM>();
		ShownBadgeCount = 5;
		for (int i = 0; i < ShownBadgeCount; i++)
		{
			((Collection<StringPairItemVM>)(object)AvailableBadgeIDs).Add(new StringPairItemVM(string.Empty, string.Empty, (BasicTooltipViewModel)null));
		}
	}

	public void OpenWith(MPLobbyAchievementBadgeGroupVM badgeGroup)
	{
		BadgeGroup = badgeGroup;
		Badge badge = BadgeGroup.ShownBadgeItem.Badge;
		TitleText = ((object)((ConditionalBadge)((badge is ConditionalBadge) ? badge : null)).BadgeConditions[0].Description).ToString();
		_shownBadgeIndexOffset = 0;
		RefreshShownBadges();
		ClickToCloseText = _getExitText?.Invoke();
		IsEnabled = true;
	}

	private void RefreshShownBadges()
	{
		int num = ((Collection<MPLobbyBadgeItemVM>)(object)BadgeGroup.Badges).IndexOf(BadgeGroup.ShownBadgeItem) + _shownBadgeIndexOffset;
		int num2 = 0;
		int num3 = ShownBadgeCount / 2;
		for (int i = num - num3; i <= num + num3; i++)
		{
			if (i >= 0 && i < ((Collection<MPLobbyBadgeItemVM>)(object)BadgeGroup.Badges).Count)
			{
				MPLobbyBadgeItemVM mPLobbyBadgeItemVM = ((Collection<MPLobbyBadgeItemVM>)(object)BadgeGroup.Badges)[i];
				((Collection<StringPairItemVM>)(object)AvailableBadgeIDs)[num2].Value = mPLobbyBadgeItemVM.BadgeId;
				((Collection<StringPairItemVM>)(object)AvailableBadgeIDs)[num2].Definition = mPLobbyBadgeItemVM.Name;
			}
			else
			{
				((Collection<StringPairItemVM>)(object)AvailableBadgeIDs)[num2].Value = string.Empty;
				((Collection<StringPairItemVM>)(object)AvailableBadgeIDs)[num2].Definition = string.Empty;
			}
			num2++;
		}
		CanIncreaseBadgeIndices = ((Collection<MPLobbyBadgeItemVM>)(object)BadgeGroup.Badges).IndexOf(((Collection<MPLobbyBadgeItemVM>)(object)BadgeGroup.Badges)[num]) < ((Collection<MPLobbyBadgeItemVM>)(object)BadgeGroup.Badges).Count - 1;
		CanDecreaseBadgeIndices = num > 0;
	}

	public void ExecuteClosePopup()
	{
		BadgeGroup = null;
		IsEnabled = false;
	}

	public void ExecuteIncreaseActiveBadgeIndices()
	{
		if (CanIncreaseBadgeIndices)
		{
			_shownBadgeIndexOffset++;
			RefreshShownBadges();
		}
	}

	public void ExecuteDecreaseActiveBadgeIndices()
	{
		if (CanDecreaseBadgeIndices)
		{
			_shownBadgeIndexOffset--;
			RefreshShownBadges();
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM previousTabInputKey = PreviousTabInputKey;
		if (previousTabInputKey != null)
		{
			((ViewModel)previousTabInputKey).OnFinalize();
		}
		InputKeyItemVM nextTabInputKey = NextTabInputKey;
		if (nextTabInputKey != null)
		{
			((ViewModel)nextTabInputKey).OnFinalize();
		}
	}

	public void SetPreviousTabInputKey(HotKey hotKey)
	{
		PreviousTabInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetNextTabInputKey(HotKey hotKey)
	{
		NextTabInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}
}
