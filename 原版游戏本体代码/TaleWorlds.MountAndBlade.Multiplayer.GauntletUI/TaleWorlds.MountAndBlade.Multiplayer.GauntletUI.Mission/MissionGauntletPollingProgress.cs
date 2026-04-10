using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MultiplayerPollProgressUIHandler))]
public class MissionGauntletPollingProgress : MissionView
{
	private MultiplayerPollComponent _multiplayerPollComponent;

	private MultiplayerPollProgressVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private bool _isActive;

	private bool _isVoteOpenForMyPeer;

	private InputContext _input => ((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input;

	public MissionGauntletPollingProgress()
	{
		base.ViewOrderPriority = 24;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_multiplayerPollComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerPollComponent>();
		MultiplayerPollComponent multiplayerPollComponent = _multiplayerPollComponent;
		multiplayerPollComponent.OnKickPollOpened = (Action<MissionPeer, MissionPeer, bool>)Delegate.Combine(multiplayerPollComponent.OnKickPollOpened, new Action<MissionPeer, MissionPeer, bool>(OnKickPollOpened));
		MultiplayerPollComponent multiplayerPollComponent2 = _multiplayerPollComponent;
		multiplayerPollComponent2.OnPollUpdated = (Action<int, int>)Delegate.Combine(multiplayerPollComponent2.OnPollUpdated, new Action<int, int>(OnPollUpdated));
		MultiplayerPollComponent multiplayerPollComponent3 = _multiplayerPollComponent;
		multiplayerPollComponent3.OnPollClosed = (Action)Delegate.Combine(multiplayerPollComponent3.OnPollClosed, new Action(OnPollClosed));
		MultiplayerPollComponent multiplayerPollComponent4 = _multiplayerPollComponent;
		multiplayerPollComponent4.OnPollCancelled = (Action)Delegate.Combine(multiplayerPollComponent4.OnPollCancelled, new Action(OnPollClosed));
		_dataSource = new MultiplayerPollProgressVM();
		_gauntletLayer = new GauntletLayer("MultiplayerPollingProgress", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerPollingProgress", (ViewModel)(object)_dataSource);
		_input.RegisterHotKeyCategory(HotKeyManager.GetCategory("PollHotkeyCategory"));
		_dataSource.AddKey(HotKeyManager.GetCategory("PollHotkeyCategory").GetGameKey(108));
		_dataSource.AddKey(HotKeyManager.GetCategory("PollHotkeyCategory").GetGameKey(109));
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		MultiplayerPollComponent multiplayerPollComponent = _multiplayerPollComponent;
		multiplayerPollComponent.OnKickPollOpened = (Action<MissionPeer, MissionPeer, bool>)Delegate.Remove(multiplayerPollComponent.OnKickPollOpened, new Action<MissionPeer, MissionPeer, bool>(OnKickPollOpened));
		MultiplayerPollComponent multiplayerPollComponent2 = _multiplayerPollComponent;
		multiplayerPollComponent2.OnPollUpdated = (Action<int, int>)Delegate.Remove(multiplayerPollComponent2.OnPollUpdated, new Action<int, int>(OnPollUpdated));
		MultiplayerPollComponent multiplayerPollComponent3 = _multiplayerPollComponent;
		multiplayerPollComponent3.OnPollClosed = (Action)Delegate.Remove(multiplayerPollComponent3.OnPollClosed, new Action(OnPollClosed));
		MultiplayerPollComponent multiplayerPollComponent4 = _multiplayerPollComponent;
		multiplayerPollComponent4.OnPollCancelled = (Action)Delegate.Remove(multiplayerPollComponent4.OnPollCancelled, new Action(OnPollClosed));
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		MultiplayerPollProgressVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
		((MissionView)this).MissionScreen.SetDisplayDialog(false);
		((MissionView)this).OnMissionScreenFinalize();
	}

	private void OnKickPollOpened(MissionPeer initiatorPeer, MissionPeer targetPeer, bool isBanRequested)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_isActive = true;
		_isVoteOpenForMyPeer = NetworkMain.GameClient.PlayerID == ((PeerComponent)targetPeer).Peer.Id;
		_dataSource.OnKickPollOpened(initiatorPeer, targetPeer, isBanRequested);
	}

	private void OnPollUpdated(int votesAccepted, int votesRejected)
	{
		_dataSource.OnPollUpdated(votesAccepted, votesRejected);
	}

	private void OnPollClosed()
	{
		_isActive = false;
		_dataSource.OnPollClosed();
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (_isActive && !_isVoteOpenForMyPeer)
		{
			if (_input.IsGameKeyPressed(108))
			{
				_isActive = false;
				_multiplayerPollComponent.Vote(true);
				_dataSource.OnPollOptionPicked();
			}
			else if (_input.IsGameKeyPressed(109))
			{
				_isActive = false;
				_multiplayerPollComponent.Vote(false);
				_dataSource.OnPollOptionPicked();
			}
		}
	}
}
