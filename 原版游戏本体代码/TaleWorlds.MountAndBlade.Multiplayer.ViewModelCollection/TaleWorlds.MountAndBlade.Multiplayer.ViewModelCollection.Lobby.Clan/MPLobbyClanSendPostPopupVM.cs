using System.Collections.Generic;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanSendPostPopupVM : ViewModel
{
	public enum PostPopupMode
	{
		Information,
		Announcement
	}

	private PostPopupMode _popupMode;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private bool _isSelected;

	private string _titleText;

	private string _postData;

	private string _sendText;

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
				((ViewModel)this).OnPropertyChanged("IsSelected");
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
				((ViewModel)this).OnPropertyChanged("TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string PostData
	{
		get
		{
			return _postData;
		}
		set
		{
			if (value != _postData)
			{
				_postData = value;
				((ViewModel)this).OnPropertyChanged("PostData");
			}
		}
	}

	[DataSourceProperty]
	public string SendText
	{
		get
		{
			return _sendText;
		}
		set
		{
			if (value != _sendText)
			{
				_sendText = value;
				((ViewModel)this).OnPropertyChanged("SendText");
			}
		}
	}

	public MPLobbyClanSendPostPopupVM(PostPopupMode popupMode)
	{
		_popupMode = popupMode;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		SendText = ((object)new TextObject("{=qTYsYJ9V}Send", (Dictionary<string, object>)null)).ToString();
		if (_popupMode == PostPopupMode.Information)
		{
			TitleText = ((object)new TextObject("{=zravuI1b}Type Clan Information", (Dictionary<string, object>)null)).ToString();
		}
		else if (_popupMode == PostPopupMode.Announcement)
		{
			TitleText = ((object)new TextObject("{=g5W32uf4}Type Your Announcement", (Dictionary<string, object>)null)).ToString();
		}
	}

	public void ExecuteOpenPopup()
	{
		IsSelected = true;
		PostData = "";
	}

	public void ExecuteClosePopup()
	{
		IsSelected = false;
	}

	public void ExecuteSend()
	{
		if (_popupMode == PostPopupMode.Information)
		{
			NetworkMain.GameClient.SetClanInformationText(PostData);
		}
		else if (_popupMode == PostPopupMode.Announcement)
		{
			NetworkMain.GameClient.AddClanAnnouncement(PostData);
		}
		ExecuteClosePopup();
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
