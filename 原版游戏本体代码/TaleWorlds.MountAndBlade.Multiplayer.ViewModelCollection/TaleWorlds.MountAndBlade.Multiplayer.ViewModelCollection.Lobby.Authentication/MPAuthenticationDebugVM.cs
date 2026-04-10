using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Authentication;

public class MPAuthenticationDebugVM : ViewModel
{
	private bool _isEnabled;

	private bool _isLoginRequestActive;

	private string _titleText;

	private string _usernameText;

	private string _passwordText;

	private string _username;

	private string _password;

	private string _loginText;

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
	public bool IsLoginRequestActive
	{
		get
		{
			return _isLoginRequestActive;
		}
		set
		{
			if (value != _isLoginRequestActive)
			{
				_isLoginRequestActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsLoginRequestActive");
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
	public string UsernameText
	{
		get
		{
			return _usernameText;
		}
		set
		{
			if (value != _usernameText)
			{
				_usernameText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "UsernameText");
			}
		}
	}

	[DataSourceProperty]
	public string PasswordText
	{
		get
		{
			return _passwordText;
		}
		set
		{
			if (value != _passwordText)
			{
				_passwordText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PasswordText");
			}
		}
	}

	[DataSourceProperty]
	public string Username
	{
		get
		{
			return _username;
		}
		set
		{
			if (value != _username)
			{
				_username = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Username");
			}
		}
	}

	[DataSourceProperty]
	public string Password
	{
		get
		{
			return _password;
		}
		set
		{
			if (value != _password)
			{
				_password = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Password");
			}
		}
	}

	[DataSourceProperty]
	public string LoginText
	{
		get
		{
			return _loginText;
		}
		set
		{
			if (value != _loginText)
			{
				_loginText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LoginText");
			}
		}
	}

	public MPAuthenticationDebugVM()
	{
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
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=!}For Debug Purposes", (Dictionary<string, object>)null)).ToString();
		UsernameText = ((object)new TextObject("{=!}Username:", (Dictionary<string, object>)null)).ToString();
		PasswordText = ((object)new TextObject("{=!}Password:", (Dictionary<string, object>)null)).ToString();
		LoginText = ((object)new TextObject("{=!}Login", (Dictionary<string, object>)null)).ToString();
	}

	private async void ExecuteLogin()
	{
		LobbyState obj = Game.Current.GameStateManager.ActiveState as LobbyState;
		IsLoginRequestActive = true;
		await obj.TryLogin(Username, Password);
		IsLoginRequestActive = false;
	}
}
