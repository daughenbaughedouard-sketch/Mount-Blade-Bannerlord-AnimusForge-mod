using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPLobbyRejoinVM : ViewModel
{
	private readonly Action<MPLobbyVM.LobbyPage> _onChangePageRequest;

	public Action OnRejoinRequested;

	private bool _isEnabled;

	private bool _isRejoining;

	private string _titleText;

	private string _descriptionText;

	private string _rejoinText;

	private string _fleeText;

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
	public bool IsRejoining
	{
		get
		{
			return _isRejoining;
		}
		set
		{
			if (value != _isRejoining)
			{
				_isRejoining = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRejoining");
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
	public string DescriptionText
	{
		get
		{
			return _descriptionText;
		}
		set
		{
			if (value != _descriptionText)
			{
				_descriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public string RejoinText
	{
		get
		{
			return _rejoinText;
		}
		set
		{
			if (value != _rejoinText)
			{
				_rejoinText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RejoinText");
			}
		}
	}

	[DataSourceProperty]
	public string FleeText
	{
		get
		{
			return _fleeText;
		}
		set
		{
			if (value != _fleeText)
			{
				_fleeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FleeText");
			}
		}
	}

	public MPLobbyRejoinVM(Action<MPLobbyVM.LobbyPage> onChangePageRequest)
	{
		_onChangePageRequest = onChangePageRequest;
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
		TitleText = ((object)new TextObject("{=6zYeU0VO}Disconnected from a match", (Dictionary<string, object>)null)).ToString();
		DescriptionText = ((object)new TextObject("{=1A1t1naG}You have left a ranked game in progress. Please reconnect to the game.", (Dictionary<string, object>)null)).ToString();
		RejoinText = ((object)new TextObject("{=5gGyaTPL}Reconnect", (Dictionary<string, object>)null)).ToString();
		FleeText = ((object)new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null)).ToString();
	}

	private void ExecuteRejoin()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		NetworkMain.GameClient.RejoinBattle();
		TitleText = ((object)new TextObject("{=N0DXasar}Reconnecting", (Dictionary<string, object>)null)).ToString();
		DescriptionText = ((object)new TextObject("{=BZcFB1My}Please wait while you are reconnecting to the game", (Dictionary<string, object>)null)).ToString();
		IsRejoining = true;
		OnRejoinRequested?.Invoke();
	}

	private void ExecuteFlee()
	{
		NetworkMain.GameClient.FleeBattle();
		_onChangePageRequest?.Invoke(MPLobbyVM.LobbyPage.Home);
	}
}
