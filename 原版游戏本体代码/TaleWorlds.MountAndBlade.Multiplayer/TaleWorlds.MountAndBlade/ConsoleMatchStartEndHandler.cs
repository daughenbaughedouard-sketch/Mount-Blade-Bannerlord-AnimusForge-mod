using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade;

public class ConsoleMatchStartEndHandler : MissionNetwork
{
	private enum MatchState
	{
		NotPlaying,
		Playing
	}

	private MissionMultiplayerGameModeBaseClient _gameModeClient;

	private MultiplayerMissionAgentVisualSpawnComponent _visualSpawnComponent;

	private MatchState _matchState;

	private bool _inGameCheckActive;

	private float _playingCheckTimer;

	private List<VirtualPlayer> _activeOtherPlayers;

	public override void OnBehaviorInitialize()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		((MissionNetwork)this).OnBehaviorInitialize();
		_activeOtherPlayers = new List<VirtualPlayer>();
		_matchState = MatchState.NotPlaying;
		_gameModeClient = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		_visualSpawnComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>();
		_visualSpawnComponent.OnMyAgentSpawnedFromVisual += AgentVisualSpawnComponentOnOnMyAgentVisualSpawned;
		MissionPeer.OnTeamChanged += new OnTeamChangedDelegate(OnTeamChange);
	}

	public override void OnRemoveBehavior()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((MissionNetwork)this).OnRemoveBehavior();
		MissionPeer.OnTeamChanged -= new OnTeamChangedDelegate(OnTeamChange);
		if (_matchState == MatchState.Playing)
		{
			_matchState = MatchState.NotPlaying;
			PlatformServices.MultiplayerGameStateChanged(false);
		}
	}

	private void AgentVisualSpawnComponentOnOnMyAgentVisualSpawned()
	{
		_visualSpawnComponent.OnMyAgentSpawnedFromVisual -= AgentVisualSpawnComponentOnOnMyAgentVisualSpawned;
		_inGameCheckActive = true;
	}

	private void OnTeamChange(NetworkCommunicator peer, Team previousTeam, Team newTeam)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)newTeam.Side != -1)
		{
			return;
		}
		if (peer.IsMine)
		{
			_visualSpawnComponent.OnMyAgentVisualSpawned += AgentVisualSpawnComponentOnOnMyAgentVisualSpawned;
			_inGameCheckActive = false;
			PlatformServices.MultiplayerGameStateChanged(false);
			return;
		}
		int num = _activeOtherPlayers.IndexOf(peer.VirtualPlayer);
		if (num >= 0)
		{
			_activeOtherPlayers.RemoveAt(num);
		}
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if (agent.MissionPeer != null && !((PeerComponent)agent.MissionPeer).IsMine && !_activeOtherPlayers.Contains(((PeerComponent)agent.MissionPeer).Peer))
		{
			_activeOtherPlayers.Add(((PeerComponent)agent.MissionPeer).Peer);
		}
	}

	public override void OnMissionTick(float dt)
	{
		_playingCheckTimer -= dt;
		if (!(_playingCheckTimer <= 0f))
		{
			return;
		}
		_playingCheckTimer += 1f;
		if (!_inGameCheckActive)
		{
			return;
		}
		if (_activeOtherPlayers.Count > 0)
		{
			if (_matchState == MatchState.NotPlaying)
			{
				_matchState = MatchState.Playing;
				PlatformServices.MultiplayerGameStateChanged(true);
			}
		}
		else if (_matchState == MatchState.Playing)
		{
			_matchState = MatchState.NotPlaying;
			PlatformServices.MultiplayerGameStateChanged(false);
		}
	}
}
