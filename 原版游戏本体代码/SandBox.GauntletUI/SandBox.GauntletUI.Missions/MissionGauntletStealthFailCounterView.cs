using SandBox.Missions;
using SandBox.View.Missions;
using SandBox.ViewModelCollection.Missions.NameMarker.Targets.Hideout;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(MissionStealthFailCounterView))]
public class MissionGauntletStealthFailCounterView : MissionStealthFailCounterView
{
	private GauntletLayer _countdownLayer;

	private MissionStealthFailCounterVM _countdownCounterVM;

	private StealthFailCounterMissionLogic _stealthFailCounterMissionLogic;

	public override void OnMissionScreenInitialize()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_countdownCounterVM = new MissionStealthFailCounterVM();
		_countdownLayer = new GauntletLayer("MissionStealthFailCounter", 10, false);
		_countdownLayer.LoadMovie("MissionStealthFailCounter", (ViewModel)(object)_countdownCounterVM);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_countdownLayer);
	}

	public override void AfterStart()
	{
		_stealthFailCounterMissionLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<StealthFailCounterMissionLogic>();
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		((ViewModel)_countdownCounterVM).OnFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_countdownLayer);
		_countdownLayer = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (_stealthFailCounterMissionLogic != null)
		{
			_countdownCounterVM.UpdateFailCounter(_stealthFailCounterMissionLogic.FailCounterElapsedTime, _stealthFailCounterMissionLogic.FailCounterSeconds, _stealthFailCounterMissionLogic.IsActive);
		}
	}
}
