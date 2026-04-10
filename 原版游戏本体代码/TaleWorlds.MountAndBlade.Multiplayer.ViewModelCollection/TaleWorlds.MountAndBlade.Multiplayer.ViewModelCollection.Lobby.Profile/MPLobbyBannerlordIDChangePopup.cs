using System.Collections.Generic;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyBannerlordIDChangePopup : ViewModel
{
	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private bool _isSelected;

	private bool _hasRequestSent;

	private string _bannerlordIDInputText;

	private string _changeBannerlordIDText;

	private string _typeYourNameText;

	private string _requestSentText;

	private string _errorText;

	private string _cancelText;

	private string _doneText;

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
	public string BannerlordIDInputText
	{
		get
		{
			return _bannerlordIDInputText;
		}
		set
		{
			if (value != _bannerlordIDInputText)
			{
				_bannerlordIDInputText = value;
				ErrorText = "";
				((ViewModel)this).OnPropertyChanged("BannerlordIDInputText");
			}
		}
	}

	[DataSourceProperty]
	public string ChangeBannerlordIDText
	{
		get
		{
			return _changeBannerlordIDText;
		}
		set
		{
			if (value != _changeBannerlordIDText)
			{
				_changeBannerlordIDText = value;
				((ViewModel)this).OnPropertyChanged("ChangeBannerlordIDText");
			}
		}
	}

	[DataSourceProperty]
	public string TypeYourNameText
	{
		get
		{
			return _typeYourNameText;
		}
		set
		{
			if (value != _typeYourNameText)
			{
				_typeYourNameText = value;
				((ViewModel)this).OnPropertyChanged("TypeYourNameText");
			}
		}
	}

	[DataSourceProperty]
	public string RequestSentText
	{
		get
		{
			return _requestSentText;
		}
		set
		{
			if (value != _requestSentText)
			{
				_requestSentText = value;
				((ViewModel)this).OnPropertyChanged("RequestSentText");
			}
		}
	}

	[DataSourceProperty]
	public bool HasRequestSent
	{
		get
		{
			return _hasRequestSent;
		}
		set
		{
			if (value != _hasRequestSent)
			{
				_hasRequestSent = value;
				((ViewModel)this).OnPropertyChanged("HasRequestSent");
			}
		}
	}

	[DataSourceProperty]
	public string ErrorText
	{
		get
		{
			return _errorText;
		}
		set
		{
			if (value != _errorText)
			{
				_errorText = value;
				((ViewModel)this).OnPropertyChanged("ErrorText");
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
				((ViewModel)this).OnPropertyChanged("CancelText");
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
			if (value != _doneText)
			{
				_doneText = value;
				((ViewModel)this).OnPropertyChanged("DoneText");
			}
		}
	}

	public MPLobbyBannerlordIDChangePopup()
	{
		BannerlordIDInputText = "";
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
		((ViewModel)this).RefreshValues();
		ChangeBannerlordIDText = ((object)new TextObject("{=ozREO8ev}Change Bannerlord ID", (Dictionary<string, object>)null)).ToString();
		TypeYourNameText = ((object)new TextObject("{=clxT9H4T}Type Your Name", (Dictionary<string, object>)null)).ToString();
		RequestSentText = ((object)new TextObject("{=V2lpn6dc}Your Bannerlord ID changing request has been successfully sent.", (Dictionary<string, object>)null)).ToString();
		DoneText = ((object)GameTexts.FindText("str_done", (string)null)).ToString();
		CancelText = ((object)GameTexts.FindText("str_cancel", (string)null)).ToString();
		ErrorText = "";
	}

	public void ExecuteOpenPopup()
	{
		IsSelected = true;
		HasRequestSent = false;
	}

	public void ExecuteClosePopup()
	{
		IsSelected = false;
		BannerlordIDInputText = "";
		ErrorText = "";
	}

	private async Task<bool> IsInputValid()
	{
		if (BannerlordIDInputText.Length < Parameters.UsernameMinLength)
		{
			GameTexts.SetVariable("STR1", new TextObject("{=k7fJ7TF0}Has to be at least", (Dictionary<string, object>)null));
			GameTexts.SetVariable("STR2", Parameters.UsernameMinLength);
			string text = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
			GameTexts.SetVariable("STR1", text);
			GameTexts.SetVariable("STR2", new TextObject("{=nWJGjCgy}characters", (Dictionary<string, object>)null));
			ErrorText = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
			return false;
		}
		if (!Common.IsAllLetters(BannerlordIDInputText))
		{
			ErrorText = ((object)new TextObject("{=Po8jNaXb}Can only contain letters", (Dictionary<string, object>)null)).ToString();
			return false;
		}
		if (!(await PlatformServices.Instance.VerifyString(BannerlordIDInputText)))
		{
			ErrorText = ((object)new TextObject("{=bXAIlBHv}Can not contain offensive language", (Dictionary<string, object>)null)).ToString();
			return false;
		}
		return true;
	}

	public async void ExecuteApply()
	{
		if (!HasRequestSent)
		{
			if (await IsInputValid())
			{
				NetworkMain.GameClient.ChangeUsername(BannerlordIDInputText);
				HasRequestSent = true;
				ErrorText = "";
			}
		}
		else
		{
			ExecuteClosePopup();
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
