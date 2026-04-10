using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.FormationMarker;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionSiegeEngineMarkerView))]
public class MissionGauntletSiegeEngineMarker : MissionBattleUIBaseView
{
	private List<SiegeWeapon> _siegeEngines;

	private MissionSiegeEngineMarkerVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private MissionGauntletSingleplayerOrderUIHandler _orderHandler;

	protected override void OnCreateView()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		_dataSource = new MissionSiegeEngineMarkerVM(((MissionBehavior)this).Mission, base.MissionScreen.CombatCamera);
		_gauntletLayer = new GauntletLayer("MissionSiegeEngineMarker", ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("SiegeEngineMarker", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		_orderHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletSingleplayerOrderUIHandler>();
	}

	public override void OnDeploymentFinished()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		((MissionBehavior)this).OnDeploymentFinished();
		_siegeEngines = new List<SiegeWeapon>();
		foreach (MissionObject item in (List<MissionObject>)(object)((MissionBehavior)this).Mission.ActiveMissionObjects)
		{
			SiegeWeapon val;
			if ((val = (SiegeWeapon)(object)((item is SiegeWeapon) ? item : null)) != null && ((UsableMachine)val).DestructionComponent != null && (int)val.Side != -1)
			{
				_siegeEngines.Add(val);
			}
		}
	}

	protected override void OnDestroyView()
	{
		((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	protected override void OnSuspendView()
	{
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		}
	}

	protected override void OnResumeView()
	{
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (base.IsViewCreated)
		{
			if (!_dataSource.IsInitialized && _siegeEngines != null)
			{
				_dataSource.InitializeWith(_siegeEngines);
			}
			if (!_orderHandler.IsDeployment)
			{
				_dataSource.IsEnabled = base.Input.IsGameKeyDown(5);
			}
			_dataSource.Tick(dt);
		}
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (base.IsViewCreated)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (base.IsViewCreated)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}
}
