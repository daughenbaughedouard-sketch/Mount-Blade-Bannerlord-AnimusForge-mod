using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.KillFeed;

public class MPDuelKillNotificationItemVM : ViewModel
{
	private Action<MPDuelKillNotificationItemVM> _onRemove;

	private bool _isEndOfDuel;

	private int _arenaType;

	private int _firstPlayerScore;

	private int _secondPlayerScore;

	private string _firstPlayerName;

	private string _secondPlayerName;

	private MPTeammateCompassTargetVM _firstPlayerCompassElement;

	private MPTeammateCompassTargetVM _secondPlayerCompassElement;

	[DataSourceProperty]
	public bool IsEndOfDuel
	{
		get
		{
			return _isEndOfDuel;
		}
		set
		{
			if (value != _isEndOfDuel)
			{
				_isEndOfDuel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEndOfDuel");
			}
		}
	}

	[DataSourceProperty]
	public int ArenaType
	{
		get
		{
			return _arenaType;
		}
		set
		{
			if (value != _arenaType)
			{
				_arenaType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ArenaType");
			}
		}
	}

	[DataSourceProperty]
	public int FirstPlayerScore
	{
		get
		{
			return _firstPlayerScore;
		}
		set
		{
			if (value != _firstPlayerScore)
			{
				_firstPlayerScore = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "FirstPlayerScore");
			}
		}
	}

	[DataSourceProperty]
	public int SecondPlayerScore
	{
		get
		{
			return _secondPlayerScore;
		}
		set
		{
			if (value != _secondPlayerScore)
			{
				_secondPlayerScore = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SecondPlayerScore");
			}
		}
	}

	[DataSourceProperty]
	public string FirstPlayerName
	{
		get
		{
			return _firstPlayerName;
		}
		set
		{
			if (value != _firstPlayerName)
			{
				_firstPlayerName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FirstPlayerName");
			}
		}
	}

	[DataSourceProperty]
	public string SecondPlayerName
	{
		get
		{
			return _secondPlayerName;
		}
		set
		{
			if (value != _secondPlayerName)
			{
				_secondPlayerName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SecondPlayerName");
			}
		}
	}

	[DataSourceProperty]
	public MPTeammateCompassTargetVM FirstPlayerCompassElement
	{
		get
		{
			return _firstPlayerCompassElement;
		}
		set
		{
			if (value != _firstPlayerCompassElement)
			{
				_firstPlayerCompassElement = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPTeammateCompassTargetVM>(value, "FirstPlayerCompassElement");
			}
		}
	}

	[DataSourceProperty]
	public MPTeammateCompassTargetVM SecondPlayerCompassElement
	{
		get
		{
			return _secondPlayerCompassElement;
		}
		set
		{
			if (value != _secondPlayerCompassElement)
			{
				_secondPlayerCompassElement = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPTeammateCompassTargetVM>(value, "SecondPlayerCompassElement");
			}
		}
	}

	public MPDuelKillNotificationItemVM(MissionPeer firstPlayerPeer, MissionPeer secondPlayerPeer, int firstPlayerScore, int secondPlayerScore, TroopType arenaTroopType, Action<MPDuelKillNotificationItemVM> onRemove)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		_onRemove = onRemove;
		ArenaType = (int)arenaTroopType;
		FirstPlayerScore = firstPlayerScore;
		SecondPlayerScore = secondPlayerScore;
		int intValue = MultiplayerOptionsExtensions.GetIntValue((OptionType)37, (MultiplayerOptionsAccessMode)1);
		IsEndOfDuel = FirstPlayerScore == intValue || SecondPlayerScore == intValue;
		InitProperties(firstPlayerPeer, secondPlayerPeer);
	}

	public void InitProperties(MissionPeer firstPlayerPeer, MissionPeer secondPlayerPeer)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		TargetIconType peerIconType = GetPeerIconType(firstPlayerPeer);
		FirstPlayerName = firstPlayerPeer.DisplayedName;
		Color white = Color.White;
		uint color = ((Color)(ref white)).ToUnsignedInteger();
		white = Color.White;
		FirstPlayerCompassElement = new MPTeammateCompassTargetVM(peerIconType, color, ((Color)(ref white)).ToUnsignedInteger(), Banner.CreateOneColoredEmptyBanner(0), isAlly: false);
		TargetIconType peerIconType2 = GetPeerIconType(secondPlayerPeer);
		SecondPlayerName = secondPlayerPeer.DisplayedName;
		white = Color.White;
		uint color2 = ((Color)(ref white)).ToUnsignedInteger();
		white = Color.White;
		SecondPlayerCompassElement = new MPTeammateCompassTargetVM(peerIconType2, color2, ((Color)(ref white)).ToUnsignedInteger(), Banner.CreateOneColoredEmptyBanner(0), isAlly: false);
	}

	private TargetIconType GetPeerIconType(MissionPeer peer)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		MPHeroClass mPHeroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(peer, false);
		if (mPHeroClassForPeer != null)
		{
			return mPHeroClassForPeer.IconType;
		}
		return (TargetIconType)(-1);
	}

	public void ExecuteRemove()
	{
		_onRemove(this);
	}
}
