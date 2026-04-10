using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.Missions.Objectives;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.Missions.Objective;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(MissionObjectiveView))]
public class MissionGauntletObjectiveView : MissionObjectiveView
{
	private GauntletLayer _gauntletLayer;

	private MissionObjectiveVM _dataSource;

	private MissionObjectiveLogic _objectiveLogic;

	private MissionObjective _latestObjective;

	public override void OnMissionScreenInitialize()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		base.OnMissionScreenInitialize();
		_objectiveLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionObjectiveLogic>();
		if (_objectiveLogic == null)
		{
			Debug.FailedAssert("Mission objective view is enabled but there is no objective logic in mission", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\Mission\\MissionGauntletObjectiveView.cs", "OnMissionScreenInitialize", 34);
			return;
		}
		_dataSource = new MissionObjectiveVM(_objectiveLogic, base.MissionScreen.CombatCamera);
		_gauntletLayer = new GauntletLayer("MissionObjective", 1, false);
		_gauntletLayer.LoadMovie("MissionObjectives", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		base.OnMissionScreenFinalize();
		if (_gauntletLayer != null)
		{
			((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			_gauntletLayer = null;
		}
		if (_dataSource != null)
		{
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (_objectiveLogic != null && _gauntletLayer != null && _dataSource != null)
		{
			UpdateContextAlpha(dt);
			MissionObjective currentObjective = _objectiveLogic.CurrentObjective;
			if (_latestObjective != currentObjective)
			{
				_latestObjective = currentObjective;
				_dataSource.UpdateObjective(_latestObjective);
			}
			_dataSource.Tick(dt);
		}
	}

	private void UpdateContextAlpha(float dt)
	{
		float num = (_dataSource.IsEnabled ? 1f : 0f);
		float num2 = MathF.Clamp(dt * 6f, 0f, 1f);
		float contextAlpha = _gauntletLayer.UIContext.ContextAlpha;
		contextAlpha = MathF.Lerp(contextAlpha, num, num2, 1E-05f);
		_gauntletLayer.UIContext.ContextAlpha = contextAlpha;
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}

	protected override void OnResumeView()
	{
		base.OnResumeView();
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
	}

	protected override void OnSuspendView()
	{
		base.OnSuspendView();
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
	}
}
