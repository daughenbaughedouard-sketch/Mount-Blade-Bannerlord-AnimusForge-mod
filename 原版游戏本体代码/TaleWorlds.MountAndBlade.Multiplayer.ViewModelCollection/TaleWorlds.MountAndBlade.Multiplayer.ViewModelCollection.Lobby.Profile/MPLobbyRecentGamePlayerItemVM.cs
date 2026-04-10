using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Profile;

public class MPLobbyRecentGamePlayerItemVM : MPLobbyPlayerBaseVM
{
	public readonly MatchHistoryData MatchOfThePlayer;

	private readonly Action<MPLobbyRecentGamePlayerItemVM> _onActivatePlayerActions;

	private int _killCount;

	private int _deathCount;

	private int _assistCount;

	[DataSourceProperty]
	public int KillCount
	{
		get
		{
			return _killCount;
		}
		set
		{
			if (value != _killCount)
			{
				_killCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "KillCount");
			}
		}
	}

	[DataSourceProperty]
	public int DeathCount
	{
		get
		{
			return _deathCount;
		}
		set
		{
			if (value != _deathCount)
			{
				_deathCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "DeathCount");
			}
		}
	}

	[DataSourceProperty]
	public int AssistCount
	{
		get
		{
			return _assistCount;
		}
		set
		{
			if (value != _assistCount)
			{
				_assistCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AssistCount");
			}
		}
	}

	public unsafe MPLobbyRecentGamePlayerItemVM(PlayerId playerId, MatchHistoryData matchOfThePlayer, Action<MPLobbyRecentGamePlayerItemVM> onActivatePlayerActions)
		: base(playerId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		MatchOfThePlayer = matchOfThePlayer;
		_onActivatePlayerActions = onActivatePlayerActions;
		PlayerInfo val = ((IEnumerable<PlayerInfo>)MatchOfThePlayer.Players).FirstOrDefault((Func<PlayerInfo, bool>)((PlayerInfo p) => p.PlayerId == ((object)(*(PlayerId*)(&playerId))/*cast due to .constrained prefix*/).ToString()));
		if (val != null)
		{
			KillCount = val.Kill;
			DeathCount = val.Death;
			AssistCount = val.Assist;
		}
		((ViewModel)this).RefreshValues();
	}

	private void ExecuteActivatePlayerActions()
	{
		_onActivatePlayerActions?.Invoke(this);
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
	}
}
