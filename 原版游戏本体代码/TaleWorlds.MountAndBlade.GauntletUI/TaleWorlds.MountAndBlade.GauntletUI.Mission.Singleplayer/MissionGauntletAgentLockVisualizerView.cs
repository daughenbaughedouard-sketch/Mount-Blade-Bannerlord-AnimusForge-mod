using System.Collections.ObjectModel;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionAgentLockVisualizerView))]
public class MissionGauntletAgentLockVisualizerView : MissionBattleUIBaseView
{
	private GauntletLayer _layer;

	private MissionAgentLockVisualizerVM _dataSource;

	private MissionMainAgentController _missionMainAgentController;

	private Agent _latestLockedAgent;

	private Agent _latestPotentialLockedAgent;

	protected override void OnCreateView()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		_missionMainAgentController = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>();
		_missionMainAgentController.OnLockedAgentChanged += OnLockedAgentChanged;
		_missionMainAgentController.OnPotentialLockedAgentChanged += OnPotentialLockedAgentChanged;
		_dataSource = new MissionAgentLockVisualizerVM();
		_layer = new GauntletLayer("MissionAgentLockVisualizer", 10, false);
		_layer.LoadMovie("AgentLockTargets", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_layer);
	}

	protected override void OnDestroyView()
	{
		((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_layer);
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_layer = null;
		_missionMainAgentController = null;
	}

	protected override void OnSuspendView()
	{
		if (_layer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layer, true);
		}
	}

	protected override void OnResumeView()
	{
		if (_layer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layer, false);
		}
	}

	private void OnPotentialLockedAgentChanged(Agent newPotentialAgent)
	{
		MissionAgentLockVisualizerVM dataSource = _dataSource;
		if (dataSource != null && dataSource.IsEnabled)
		{
			_dataSource.OnPossibleLockAgentChange(_latestPotentialLockedAgent, newPotentialAgent);
			_latestPotentialLockedAgent = newPotentialAgent;
		}
	}

	private void OnLockedAgentChanged(Agent newAgent)
	{
		MissionAgentLockVisualizerVM dataSource = _dataSource;
		if (dataSource != null && dataSource.IsEnabled)
		{
			_dataSource.OnActiveLockAgentChange(_latestLockedAgent, newAgent);
			_latestLockedAgent = newAgent;
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		base.OnMissionScreenTick(dt);
		if (!base.IsViewCreated || _dataSource == null)
		{
			return;
		}
		_dataSource.IsEnabled = IsMainAgentAvailable();
		if (_dataSource.IsEnabled)
		{
			for (int i = 0; i < ((Collection<MissionAgentLockItemVM>)(object)_dataSource.AllTrackedAgents).Count; i++)
			{
				MissionAgentLockItemVM val = ((Collection<MissionAgentLockItemVM>)(object)_dataSource.AllTrackedAgents)[i];
				float num = 0f;
				float num2 = 0f;
				float num3 = 0f;
				MBWindowManager.WorldToScreenInsideUsableArea(base.MissionScreen.CombatCamera, val.TrackedAgent.GetChestGlobalPosition(), ref num, ref num2, ref num3);
				val.Position = new Vec2(num, num2);
			}
		}
	}

	private bool IsMainAgentAvailable()
	{
		Agent main = Agent.Main;
		if (main == null)
		{
			return false;
		}
		return main.IsActive();
	}
}
