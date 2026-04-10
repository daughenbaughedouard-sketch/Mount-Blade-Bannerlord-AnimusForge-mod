using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPLobbyBadgeItemVM : ViewModel
{
	private readonly Func<Badge, bool> _hasPlayerEarnedBadge;

	private readonly Action _onSelectedBadgeChange;

	private readonly Action<MPLobbyBadgeItemVM> _onInspected;

	private MPLobbyAchievementBadgeGroupVM _group;

	private Action<MPLobbyAchievementBadgeGroupVM> _onBadgeProgressInfoRequested;

	private const string PlaytimeConditionID = "Playtime";

	private string _name;

	private string _description;

	private string _badgeConditionsText;

	private string _badgeId;

	private bool _isEarned;

	private bool _isSelected;

	private bool _hasNotification;

	private bool _isBeingChanged;

	private bool _isFocused;

	private MBBindingList<StringPairItemVM> _conditions;

	private InputKeyItemVM _inspectProgressKey;

	public Badge Badge { get; private set; }

	[DataSourceProperty]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != _description)
			{
				_description = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Description");
			}
		}
	}

	[DataSourceProperty]
	public string BadgeConditionsText
	{
		get
		{
			return _badgeConditionsText;
		}
		set
		{
			if (value != _badgeConditionsText)
			{
				_badgeConditionsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BadgeConditionsText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemVM> Conditions
	{
		get
		{
			return _conditions;
		}
		set
		{
			if (value != _conditions)
			{
				_conditions = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemVM>>(value, "Conditions");
			}
		}
	}

	[DataSourceProperty]
	public string BadgeId
	{
		get
		{
			return _badgeId;
		}
		set
		{
			if (value != _badgeId)
			{
				_badgeId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BadgeId");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEarned
	{
		get
		{
			return _isEarned;
		}
		set
		{
			if (value != _isEarned)
			{
				_isEarned = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEarned");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool HasNotification
	{
		get
		{
			return _hasNotification;
		}
		set
		{
			if (value != _hasNotification)
			{
				_hasNotification = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasNotification");
			}
		}
	}

	[DataSourceProperty]
	public bool IsBeingChanged
	{
		get
		{
			return _isBeingChanged;
		}
		set
		{
			if (value != _isBeingChanged)
			{
				_isBeingChanged = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBeingChanged");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFocused
	{
		get
		{
			return _isFocused;
		}
		set
		{
			if (value != _isFocused)
			{
				_isFocused = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFocused");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM InspectProgressKey
	{
		get
		{
			return _inspectProgressKey;
		}
		set
		{
			if (value != _inspectProgressKey)
			{
				_inspectProgressKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "InspectProgressKey");
			}
		}
	}

	public MPLobbyBadgeItemVM(Badge badge, Action onSelectedBadgeChange, Func<Badge, bool> hasPlayerEarnedBadge, Action<MPLobbyBadgeItemVM> onInspected)
	{
		_hasPlayerEarnedBadge = hasPlayerEarnedBadge;
		_onSelectedBadgeChange = onSelectedBadgeChange;
		_onInspected = onInspected;
		Badge = badge;
		Conditions = new MBBindingList<StringPairItemVM>();
		UpdateWith(Badge);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		BadgeConditionsText = ((object)GameTexts.FindText("str_multiplayer_badge_conditions", (string)null)).ToString();
	}

	public void RefreshKeyBindings(HotKey inspectProgressKey)
	{
		InspectProgressKey = InputKeyItemVM.CreateFromHotKey(inspectProgressKey, false);
	}

	public override void OnFinalize()
	{
		InputKeyItemVM inspectProgressKey = InspectProgressKey;
		if (inspectProgressKey != null)
		{
			((ViewModel)inspectProgressKey).OnFinalize();
		}
	}

	public void UpdateWith(Badge badge)
	{
		Badge = badge;
		BadgeId = ((Badge == null) ? "none" : Badge.StringId);
		UpdateIsSelected();
		IsEarned = _hasPlayerEarnedBadge(badge);
		RefreshProperties();
	}

	private void RefreshProperties()
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Invalid comparison between Unknown and I4
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		((Collection<StringPairItemVM>)(object)Conditions).Clear();
		if (Badge != null)
		{
			Name = ((object)Badge.Name).ToString();
			Description = ((object)Badge.Description).ToString();
			Badge badge = Badge;
			ConditionalBadge val;
			if ((val = (ConditionalBadge)(object)((badge is ConditionalBadge) ? badge : null)) == null || val.BadgeConditions.Count <= 0 || Badge.IsTimed)
			{
				return;
			}
			{
				foreach (BadgeCondition badgeCondition in val.BadgeConditions)
				{
					if ((int)badgeCondition.Type == 2)
					{
						int num = BadgeManager.GetBadgeConditionNumericValue(NetworkMain.GameClient.PlayerData, badgeCondition);
						if (badgeCondition.StringId.Equals("Playtime"))
						{
							num /= 3600;
						}
						((Collection<StringPairItemVM>)(object)Conditions).Add(new StringPairItemVM(((object)badgeCondition.Description).ToString(), num.ToString(), (BasicTooltipViewModel)null));
					}
				}
				return;
			}
		}
		Name = ((object)new TextObject("{=koX9okuG}None", (Dictionary<string, object>)null)).ToString();
		Description = ((object)new TextObject("{=gcl2duJH}Reset your badge", (Dictionary<string, object>)null)).ToString();
	}

	private async void ExecuteSetAsActive()
	{
		IsBeingChanged = true;
		if (Badge == null)
		{
			await NetworkMain.GameClient.UpdateShownBadgeId("");
		}
		else if (IsEarned)
		{
			await NetworkMain.GameClient.UpdateShownBadgeId(Badge.StringId);
		}
		else
		{
			InformationManager.ShowInquiry(new InquiryData(string.Empty, ((object)new TextObject("{=B1KQ4i9q}Badge is not earned yet. Please check conditions.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), string.Empty, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
		IsBeingChanged = false;
		_onSelectedBadgeChange();
	}

	private void ExecuteShowProgression()
	{
		if (Badge is ConditionalBadge)
		{
			_onBadgeProgressInfoRequested?.Invoke(_group);
		}
	}

	public void UpdateIsSelected()
	{
		if (Badge == null)
		{
			PlayerData playerData = NetworkMain.GameClient.PlayerData;
			IsSelected = string.IsNullOrEmpty((playerData != null) ? playerData.ShownBadgeId : null);
		}
		else
		{
			string stringId = Badge.StringId;
			PlayerData playerData2 = NetworkMain.GameClient.PlayerData;
			IsSelected = stringId == ((playerData2 != null) ? playerData2.ShownBadgeId : null);
		}
	}

	public void SetGroup(MPLobbyAchievementBadgeGroupVM group, Action<MPLobbyAchievementBadgeGroupVM> onBadgeProgressInfoRequested)
	{
		_group = group;
		_onBadgeProgressInfoRequested = onBadgeProgressInfoRequested;
	}

	private void ExecuteGainFocus()
	{
		IsFocused = true;
		if (HasNotification)
		{
			HasNotification = false;
		}
		_onInspected?.Invoke(this);
	}

	private void ExecuteLoseFocus()
	{
		IsFocused = false;
		_onInspected?.Invoke(null);
	}
}
