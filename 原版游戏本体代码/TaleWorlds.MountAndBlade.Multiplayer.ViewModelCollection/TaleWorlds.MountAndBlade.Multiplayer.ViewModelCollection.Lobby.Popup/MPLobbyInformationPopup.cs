using System.Collections.Generic;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Popup;

public class MPLobbyInformationPopup : ViewModel
{
	private TextObject _titleTextObj;

	private TextObject _messageTextObj;

	private InputKeyItemVM _doneInputKey;

	private bool _isEnabled;

	private string _title;

	private string _message;

	private string _closeText;

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
	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (value != _title)
			{
				_title = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Title");
			}
		}
	}

	[DataSourceProperty]
	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			if (value != _message)
			{
				_message = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Message");
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

	public override void RefreshValues()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		if (_titleTextObj != (TextObject)null)
		{
			Title = ((object)_titleTextObj).ToString();
		}
		if (_messageTextObj != (TextObject)null)
		{
			Message = ((object)_messageTextObj).ToString();
		}
		CloseText = ((object)new TextObject("{=yQtzabbe}Close", (Dictionary<string, object>)null)).ToString();
	}

	public void ShowInformation(TextObject title, TextObject message)
	{
		IsEnabled = true;
		_titleTextObj = title;
		_messageTextObj = message;
		((ViewModel)this).RefreshValues();
	}

	public void ShowInformation(string title, string message)
	{
		IsEnabled = true;
		_titleTextObj = null;
		_messageTextObj = null;
		Title = title;
		Message = message;
		((ViewModel)this).RefreshValues();
	}

	public void ExecuteClose()
	{
		IsEnabled = false;
	}

	public void SetDoneInputKey(HotKey hotkey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}
}
