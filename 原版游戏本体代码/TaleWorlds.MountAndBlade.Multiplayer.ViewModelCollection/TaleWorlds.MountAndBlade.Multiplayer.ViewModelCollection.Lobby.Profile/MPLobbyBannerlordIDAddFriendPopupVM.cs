using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyBannerlordIDAddFriendPopupVM : ViewModel
{
	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private bool _isSelected;

	private string _titleText;

	private string _addText;

	private string _errorText;

	private string _bannerlordIDInputText;

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
	public string AddText
	{
		get
		{
			return _addText;
		}
		set
		{
			if (value != _addText)
			{
				_addText = value;
				((ViewModel)this).OnPropertyChanged("AddText");
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
				((ViewModel)this).OnPropertyChanged("BannerlordIDInputText");
				ErrorText = "";
			}
		}
	}

	public MPLobbyBannerlordIDAddFriendPopupVM()
	{
		BannerlordIDInputText = "";
		ErrorText = "";
		((ViewModel)this).RefreshValues();
	}

	public void ExecuteOpenPopup()
	{
		IsSelected = true;
	}

	public void ExecuteClosePopup()
	{
		IsSelected = false;
		BannerlordIDInputText = "";
		ErrorText = "";
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=L3DJHTdY}Enter Bannerlord ID", (Dictionary<string, object>)null)).ToString();
		AddText = ((object)new TextObject("{=tC9C8TLi}Add Friend", (Dictionary<string, object>)null)).ToString();
	}

	public async void ExecuteTryAddFriend()
	{
		string[] array = BannerlordIDInputText.Split(new char[1] { '#' });
		if (array.Length == 2 && !Extensions.IsEmpty<char>((IEnumerable<char>)array[1]))
		{
			string username = array[0];
			int id = 0;
			bool flag = Common.IsAllLetters(array[0]) && array[0].Length >= Parameters.UsernameMinLength;
			if (int.TryParse(array[1], out id) && flag)
			{
				if (await NetworkMain.GameClient.DoesPlayerWithUsernameAndIdExist(username, id))
				{
					NetworkMain.GameClient.AddFriendByUsernameAndId(username, id, BannerlordConfig.EnableGenericNames);
					ExecuteClosePopup();
				}
				else
				{
					ErrorText = ((object)new TextObject("{=tTwQsP6j}Player does not exist", (Dictionary<string, object>)null)).ToString();
				}
			}
			else
			{
				ErrorText = ((object)new TextObject("{=rWm5udCd}You must enter a valid Bannerlord ID", (Dictionary<string, object>)null)).ToString();
			}
		}
		else
		{
			ErrorText = ((object)new TextObject("{=rWm5udCd}You must enter a valid Bannerlord ID", (Dictionary<string, object>)null)).ToString();
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
