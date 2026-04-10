using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerDeathCardUIHandler))]
public class MissionGauntletDeathCard : MissionView
{
	private MPDeathCardVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private MissionPeer _myPeer;

	public override void OnMissionScreenInitialize()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		MissionMultiplayerGameModeBaseClient missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		_dataSource = new MPDeathCardVM(missionBehavior.GameType);
		_gauntletLayer = new GauntletLayer("MultiplayerDeathCard", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerDeathCard", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>().OnMyAgentVisualSpawned += OnMainAgentVisualSpawned;
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (GameNetwork.MyPeer != null && _myPeer == null)
		{
			_myPeer = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
		}
		MissionPeer myPeer = _myPeer;
		if (myPeer != null && myPeer.WantsToSpawnAsBot && _dataSource.IsActive)
		{
			_dataSource.Deactivate();
		}
	}

	private void OnMainAgentVisualSpawned()
	{
		_dataSource.Deactivate();
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
		if (affectedAgent.IsMine && (int)blow.DamageType != -1)
		{
			_dataSource.OnMainAgentRemoved(affectorAgent, blow);
		}
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		((ViewModel)_dataSource).OnFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>().OnMyAgentVisualSpawned -= OnMainAgentVisualSpawned;
		_dataSource = null;
		_gauntletLayer = null;
	}
}
