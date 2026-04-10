using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MultiplayerReportPlayerVM : ViewModel
{
	private readonly Action<string, PlayerId, string, PlayerReportType, string> _onReportDone;

	private readonly Action _onCancel;

	private string _currentGameId = string.Empty;

	private PlayerId _currentPlayerId;

	private string _currentPlayerName = string.Empty;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private string _reportMessage;

	private string _reportReasonText;

	private string _doneText;

	private string _muteDescriptionText;

	private bool _canSendReport;

	private bool _isRequestedFromMission;

	private HintViewModel _disabledReasonHint;

	private SelectorVM<SelectorItemVM> _reportReasons;

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
				((ViewModel)this).OnPropertyChanged("DoneInputKey");
			}
		}
	}

	[DataSourceProperty]
	public string ReportMessage
	{
		get
		{
			return _reportMessage;
		}
		set
		{
			if (_reportMessage != value)
			{
				_reportMessage = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ReportMessage");
			}
		}
	}

	[DataSourceProperty]
	public string DoneText
	{
		get
		{
			return _doneText;
		}
		set
		{
			if (_doneText != value)
			{
				_doneText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DoneText");
			}
		}
	}

	[DataSourceProperty]
	public string ReportReasonText
	{
		get
		{
			return _reportReasonText;
		}
		set
		{
			if (_reportReasonText != value)
			{
				_reportReasonText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ReportReasonText");
			}
		}
	}

	[DataSourceProperty]
	public bool CanSendReport
	{
		get
		{
			return _canSendReport;
		}
		set
		{
			if (_canSendReport != value)
			{
				_canSendReport = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanSendReport");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRequestedFromMission
	{
		get
		{
			return _isRequestedFromMission;
		}
		set
		{
			if (value != _isRequestedFromMission)
			{
				_isRequestedFromMission = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRequestedFromMission");
			}
		}
	}

	[DataSourceProperty]
	public string MuteDescriptionText
	{
		get
		{
			return _muteDescriptionText;
		}
		set
		{
			if (value != _muteDescriptionText)
			{
				_muteDescriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MuteDescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<SelectorItemVM> ReportReasons
	{
		get
		{
			return _reportReasons;
		}
		set
		{
			if (_reportReasons != value)
			{
				_reportReasons = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<SelectorItemVM>>(value, "ReportReasons");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel DisabledReasonHint
	{
		get
		{
			return _disabledReasonHint;
		}
		set
		{
			if (_disabledReasonHint != value)
			{
				_disabledReasonHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "DisabledReasonHint");
			}
		}
	}

	public MultiplayerReportPlayerVM(Action<string, PlayerId, string, PlayerReportType, string> onReportDone, Action onCancel)
	{
		_onReportDone = onReportDone;
		_onCancel = onCancel;
		ReportReasons = new SelectorVM<SelectorItemVM>(0, (Action<SelectorVM<SelectorItemVM>>)null);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		DisabledReasonHint = new HintViewModel(new TextObject("{=klkYFik9}You've already reported this player.", (Dictionary<string, object>)null), (string)null);
		ReportReasonText = ((object)new TextObject("{=cw5QyeRU}Report Reason", (Dictionary<string, object>)null)).ToString();
		DoneText = ((object)GameTexts.FindText("str_done", (string)null)).ToString();
		MuteDescriptionText = ((object)new TextObject("{=gGa3ZhqN}This player will be muted automatically.", (Dictionary<string, object>)null)).ToString();
		List<string> list = new List<string> { ((object)new TextObject("{=koX9okuG}None", (Dictionary<string, object>)null)).ToString() };
		foreach (object value in Enum.GetValues(typeof(PlayerReportType)))
		{
			list.Add(((object)GameTexts.FindText("str_multiplayer_report_reason", ((int)value).ToString())).ToString());
		}
		ReportReasons.Refresh((IEnumerable<string>)list, 0, (Action<SelectorVM<SelectorItemVM>>)OnReasonSelectionChange);
	}

	private void OnReasonSelectionChange(SelectorVM<SelectorItemVM> obj)
	{
		CanSendReport = obj.SelectedItem != null && obj.SelectedIndex != 0;
	}

	public void OpenNewReportWithGamePlayerId(string gameId, PlayerId playerId, string playerName, bool isRequestedFromMission)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		ReportReasons.SelectedIndex = 0;
		ReportMessage = "";
		_currentGameId = gameId;
		_currentPlayerId = playerId;
		_currentPlayerName = playerName;
		IsRequestedFromMission = isRequestedFromMission;
	}

	public void ExecuteDone()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (CanSendReport && ReportReasons.SelectedIndex > 0 && _currentGameId != string.Empty && _currentPlayerId != PlayerId.Empty)
		{
			if (ReportMessage.Length > 500)
			{
				ReportMessage = ReportMessage.Substring(0, 500);
			}
			Action<string, PlayerId, string, PlayerReportType, string> onReportDone = _onReportDone;
			if (onReportDone != null)
			{
				Common.DynamicInvokeWithLog((Delegate)onReportDone, new object[5]
				{
					_currentGameId,
					_currentPlayerId,
					_currentPlayerName,
					(object)(PlayerReportType)(ReportReasons.SelectedIndex - 1),
					ReportMessage
				});
			}
			_currentGameId = string.Empty;
			_currentPlayerId = PlayerId.Empty;
			_currentPlayerName = string.Empty;
		}
	}

	public void ExecuteCancel()
	{
		Action onCancel = _onCancel;
		if (onCancel != null)
		{
			Common.DynamicInvokeWithLog((Delegate)onCancel, Array.Empty<object>());
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM cancelInputKey = CancelInputKey;
		if (cancelInputKey != null)
		{
			((ViewModel)cancelInputKey).OnFinalize();
		}
		InputKeyItemVM doneInputKey = DoneInputKey;
		if (doneInputKey != null)
		{
			((ViewModel)doneInputKey).OnFinalize();
		}
	}

	public void SetCancelInputKey(HotKey hotKey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}
}
