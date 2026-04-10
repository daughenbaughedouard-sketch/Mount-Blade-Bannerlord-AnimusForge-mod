using SandBox.View.Missions;
using SandBox.ViewModelCollection.Missions;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(MissionAgentAlarmStateView))]
public class MissionGauntletAgentAlarmStateView : MissionAgentAlarmStateView
{
	private GauntletLayer _layer;

	private MissionAgentAlarmStateVM _dataSource;

	public MissionGauntletAgentAlarmStateView()
	{
		_dataSource = new MissionAgentAlarmStateVM();
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_dataSource.Initialize(((MissionBehavior)this).Mission, ((MissionView)this).MissionScreen.CombatCamera);
		_layer = new GauntletLayer("MissionAlarmState", 10, false);
		_layer.LoadMovie("AgentAlarmStateMissionView", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_layer);
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_layer);
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_layer = null;
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		((MissionBehavior)this).OnAgentBuild(agent, banner);
		_dataSource?.OnAgentBuild(agent, banner);
	}

	public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
	{
		((MissionBehavior)this).OnAgentTeamChanged(prevTeam, newTeam, agent);
		_dataSource?.OnAgentTeamChanged(prevTeam, newTeam, agent);
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
		_dataSource?.OnAgentRemoved(affectedAgent);
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		_dataSource?.Update();
	}

	protected override void OnResumeView()
	{
		((MissionView)this).OnResumeView();
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layer, false);
	}

	protected override void OnSuspendView()
	{
		((MissionView)this).OnSuspendView();
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layer, true);
	}
}
