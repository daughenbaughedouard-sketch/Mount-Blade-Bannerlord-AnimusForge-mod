using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MultiplayerPollProgressVM : ViewModel
{
	private static readonly TextObject _kickText = new TextObject("{=gk5dCG1j}kick", (Dictionary<string, object>)null);

	private static readonly TextObject _banText = new TextObject("{=sFDrUfNR}ban", (Dictionary<string, object>)null);

	private bool _hasOngoingPoll;

	private bool _areKeysEnabled;

	private int _votesAccepted;

	private int _votesRejected;

	private string _pollInitiatorName;

	private string _pollDescription;

	private MPPlayerVM _targetPlayer;

	private MBBindingList<InputKeyItemVM> _keys;

	[DataSourceProperty]
	public bool HasOngoingPoll
	{
		get
		{
			return _hasOngoingPoll;
		}
		set
		{
			if (value != _hasOngoingPoll)
			{
				_hasOngoingPoll = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasOngoingPoll");
			}
		}
	}

	[DataSourceProperty]
	public bool AreKeysEnabled
	{
		get
		{
			return _areKeysEnabled;
		}
		set
		{
			if (value != _areKeysEnabled)
			{
				_areKeysEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AreKeysEnabled");
			}
		}
	}

	[DataSourceProperty]
	public int VotesAccepted
	{
		get
		{
			return _votesAccepted;
		}
		set
		{
			if (_votesAccepted != value)
			{
				_votesAccepted = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "VotesAccepted");
			}
		}
	}

	[DataSourceProperty]
	public int VotesRejected
	{
		get
		{
			return _votesRejected;
		}
		set
		{
			if (_votesRejected != value)
			{
				_votesRejected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "VotesRejected");
			}
		}
	}

	[DataSourceProperty]
	public string PollInitiatorName
	{
		get
		{
			return _pollInitiatorName;
		}
		set
		{
			if (_pollInitiatorName != value)
			{
				_pollInitiatorName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PollInitiatorName");
			}
		}
	}

	[DataSourceProperty]
	public string PollDescription
	{
		get
		{
			return _pollDescription;
		}
		set
		{
			if (_pollDescription != value)
			{
				_pollDescription = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PollDescription");
			}
		}
	}

	[DataSourceProperty]
	public MPPlayerVM TargetPlayer
	{
		get
		{
			return _targetPlayer;
		}
		set
		{
			if (value != _targetPlayer)
			{
				_targetPlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPPlayerVM>(value, "TargetPlayer");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<InputKeyItemVM> Keys
	{
		get
		{
			return _keys;
		}
		set
		{
			if (_keys != value)
			{
				_keys = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<InputKeyItemVM>>(value, "Keys");
			}
		}
	}

	public MultiplayerPollProgressVM()
	{
		Keys = new MBBindingList<InputKeyItemVM>();
	}

	public void OnKickPollOpened(MissionPeer initiatorPeer, MissionPeer targetPeer, bool isBanRequested)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		TargetPlayer = new MPPlayerVM(targetPeer);
		PollInitiatorName = initiatorPeer.DisplayedName;
		GameTexts.SetVariable("ACTION", isBanRequested ? _banText : _kickText);
		PollDescription = ((object)new TextObject("{=qyuhC21P}wants to {ACTION}", (Dictionary<string, object>)null)).ToString();
		VotesAccepted = 0;
		VotesRejected = 0;
		AreKeysEnabled = NetworkMain.GameClient.PlayerID != ((PeerComponent)targetPeer).Peer.Id;
		HasOngoingPoll = true;
	}

	public void OnPollUpdated(int votesAccepted, int votesRejected)
	{
		VotesAccepted = votesAccepted;
		VotesRejected = votesRejected;
	}

	public void OnPollClosed()
	{
		HasOngoingPoll = false;
	}

	public void OnPollOptionPicked()
	{
		AreKeysEnabled = false;
	}

	public void AddKey(GameKey key)
	{
		((Collection<InputKeyItemVM>)(object)Keys).Add(InputKeyItemVM.CreateFromGameKey(key, false));
	}
}
