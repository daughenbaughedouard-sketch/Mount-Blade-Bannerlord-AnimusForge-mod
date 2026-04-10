using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Missions.Interaction.InteractionItems;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionAgentStatusUIHandler))]
public class MissionGauntletAgentStatus : MissionAgentStatusUIHandler
{
	protected GauntletLayer _gauntletLayer;

	protected MissionAgentStatusVM _dataSource;

	protected MissionMainAgentController _missionMainAgentController;

	protected MissionGauntletMainAgentEquipmentControllerView _missionMainAgentEquipmentControllerView;

	protected MissionHintLogic _missionHintLogic;

	protected bool _isInDeployment;

	public MissionAgentStatusVM DataSource => _dataSource;

	public override void AddInteractionMessage(MissionInteractionItemBaseVM message)
	{
		base.AddInteractionMessage(message);
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.InteractionInterface.AddSecondaryMessage(message);
		}
	}

	public override void RemoveInteractionMessage(MissionInteractionItemBaseVM message)
	{
		base.RemoveInteractionMessage(message);
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.InteractionInterface.RemoveSecondaryMessage(message);
		}
	}

	public override bool HasInteractionMessage(MissionInteractionItemBaseVM message)
	{
		if (_dataSource == null)
		{
			return false;
		}
		return _dataSource.InteractionInterface.HasSecondaryInteractionMessage(message);
	}

	public override void OnMissionStateActivated()
	{
		((MissionBehavior)this).OnMissionStateActivated();
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnMainAgentWeaponChange();
		}
	}

	public override void EarlyStart()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		((MissionBehavior)this).EarlyStart();
		_dataSource = new MissionAgentStatusVM(((MissionBehavior)this).Mission, base.MissionScreen.CombatCamera, (Func<float>)base.MissionScreen.GetCameraToggleProgress);
		_gauntletLayer = new GauntletLayer("MainAgentHUD", ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MainAgentHUD", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		_dataSource.TakenDamageController.SetIsEnabled(BannerlordConfig.EnableDamageTakenVisuals);
		RegisterInteractionEvents();
		CombatLogManager.OnGenerateCombatLog += OnGenerateCombatLog;
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	protected override void OnCreateView()
	{
		_dataSource.IsAgentStatusAvailable = true;
	}

	protected override void OnDestroyView()
	{
		_dataSource.IsAgentStatusAvailable = false;
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

	private void OnManagedOptionChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 24)
		{
			MissionAgentStatusVM dataSource = _dataSource;
			if (dataSource != null)
			{
				dataSource.TakenDamageController.SetIsEnabled(BannerlordConfig.EnableDamageTakenVisuals);
			}
		}
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.InitializeMainAgentPropterties();
		}
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		base.OnMissionScreenInitialize();
		_isInDeployment = (int)((MissionBehavior)this).Mission.Mode == 6;
	}

	public override void OnDeploymentFinished()
	{
		((MissionBehavior)this).OnDeploymentFinished();
		_isInDeployment = false;
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		base.OnMissionScreenFinalize();
		UnregisterInteractionEvents();
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		CombatLogManager.OnGenerateCombatLog -= OnGenerateCombatLog;
		((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
		_missionMainAgentController = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		_dataSource.IsInDeployement = _isInDeployment;
		_dataSource.Tick(dt);
		_dataSource.InteractionInterface.DisplayInteractionText = !base.MissionScreen.IsRadialMenuActive && !((MissionBehavior)this).Mission.IsOrderMenuOpen;
	}

	public override void OnFocusGained(Agent mainAgent, IFocusable focusableObject, bool isInteractable)
	{
		((MissionBehavior)this).OnFocusGained(mainAgent, focusableObject, isInteractable);
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnFocusGained(mainAgent, focusableObject, isInteractable);
		}
	}

	public override void OnAgentInteraction(Agent userAgent, Agent agent, sbyte agentBoneIndex)
	{
		((MissionBehavior)this).OnAgentInteraction(userAgent, agent, agentBoneIndex);
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnAgentInteraction(userAgent, agent, agentBoneIndex);
		}
	}

	public override void OnFocusLost(Agent agent, IFocusable focusableObject)
	{
		((MissionBehavior)this).OnFocusLost(agent, focusableObject);
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnFocusLost(agent, focusableObject);
		}
	}

	public override void OnAgentDeleted(Agent affectedAgent)
	{
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnAgentDeleted(affectedAgent);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		MissionAgentStatusVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnAgentRemoved(affectedAgent);
		}
	}

	private void OnGenerateCombatLog(CombatLogData logData)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (logData.IsVictimAgentMine && ((CombatLogData)(ref logData)).TotalDamage > 0)
		{
			MissionAgentStatusVM dataSource = _dataSource;
			if (dataSource != null)
			{
				dataSource.OnMainAgentHit(((CombatLogData)(ref logData)).TotalDamage, (float)(logData.IsRangedAttack ? 1 : 0));
			}
		}
		else if (logData.IsAttackerAgentMine && logData.ReflectedDamage > 0)
		{
			MissionAgentStatusVM dataSource2 = _dataSource;
			if (dataSource2 != null)
			{
				dataSource2.OnMainAgentHit(logData.ReflectedDamage, (float)(logData.IsRangedAttack ? 1 : 0));
			}
		}
	}

	private void RegisterInteractionEvents()
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		_missionMainAgentController = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>();
		if (_missionMainAgentController != null)
		{
			_missionMainAgentController.InteractionComponent.OnFocusGained += _dataSource.OnSecondaryFocusGained;
			_missionMainAgentController.InteractionComponent.OnFocusLost += _dataSource.OnSecondaryFocusLost;
			_missionMainAgentController.InteractionComponent.OnFocusHealthChanged += _dataSource.InteractionInterface.OnFocusedHealthChanged;
		}
		_missionMainAgentEquipmentControllerView = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletMainAgentEquipmentControllerView>();
		if (_missionMainAgentEquipmentControllerView != null)
		{
			_missionMainAgentEquipmentControllerView.OnEquipmentDropInteractionViewToggled += _dataSource.OnEquipmentInteractionViewToggled;
			_missionMainAgentEquipmentControllerView.OnEquipmentEquipInteractionViewToggled += _dataSource.OnEquipmentInteractionViewToggled;
		}
		_missionHintLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionHintLogic>();
		if (_missionHintLogic != null)
		{
			_missionHintLogic.OnActiveHintChanged += new MissionHintChangedDelegate(_dataSource.InteractionInterface.OnActiveMissionHintChanged);
		}
	}

	private void UnregisterInteractionEvents()
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		if (_missionMainAgentController != null)
		{
			_missionMainAgentController.InteractionComponent.OnFocusGained -= _dataSource.OnSecondaryFocusGained;
			_missionMainAgentController.InteractionComponent.OnFocusLost -= _dataSource.OnSecondaryFocusLost;
			_missionMainAgentController.InteractionComponent.OnFocusHealthChanged -= _dataSource.InteractionInterface.OnFocusedHealthChanged;
		}
		if (_missionMainAgentEquipmentControllerView != null)
		{
			_missionMainAgentEquipmentControllerView.OnEquipmentDropInteractionViewToggled -= _dataSource.OnEquipmentInteractionViewToggled;
			_missionMainAgentEquipmentControllerView.OnEquipmentEquipInteractionViewToggled -= _dataSource.OnEquipmentInteractionViewToggled;
		}
		_missionHintLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionHintLogic>();
		if (_missionHintLogic != null)
		{
			_missionHintLogic.OnActiveHintChanged -= new MissionHintChangedDelegate(_dataSource.InteractionInterface.OnActiveMissionHintChanged);
		}
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
		UnregisterInteractionEvents();
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
		RegisterInteractionEvents();
	}
}
