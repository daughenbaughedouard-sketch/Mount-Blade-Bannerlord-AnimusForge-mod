using System;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.KillFeed;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.KillFeed.General;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.KillFeed.Personal;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionSingleplayerKillNotificationUIHandler))]
public class MissionGauntletKillNotificationSingleplayerUIHandler : MissionBattleUIBaseView
{
	protected SPKillFeedVM _dataSource;

	private GauntletLayer _gauntletLayer;

	protected bool _isGeneralFeedEnabled = true;

	protected bool _isPersonalFeedEnabled = true;

	public override void OnMissionScreenInitialize()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		base.OnMissionScreenInitialize();
		ViewOrderPriority = 17;
		_isGeneralFeedEnabled = BannerlordConfig.KillFeedVisualType < 2;
		_isPersonalFeedEnabled = BannerlordConfig.ReportPersonalDamage;
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnOptionChange));
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.OnMissionScreenFinalize();
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnOptionChange));
	}

	protected override void OnCreateView()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		_dataSource = new SPKillFeedVM();
		_gauntletLayer = new GauntletLayer("MissionSPKillFeed", ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("SingleplayerKillfeed", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		CombatLogManager.OnGenerateCombatLog += OnCombatLogManagerOnPrintCombatLog;
	}

	protected override void OnDestroyView()
	{
		CombatLogManager.OnGenerateCombatLog -= OnCombatLogManagerOnPrintCombatLog;
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
		if (_dataSource != null)
		{
			bool isPaused = MBCommon.IsPaused;
			for (int i = 0; i < ((Collection<SPGeneralKillNotificationItemVM>)(object)_dataSource.GeneralCasualty.NotificationList).Count; i++)
			{
				((Collection<SPGeneralKillNotificationItemVM>)(object)_dataSource.GeneralCasualty.NotificationList)[i].IsPaused = isPaused;
			}
			for (int j = 0; j < ((Collection<SPPersonalKillNotificationItemVM>)(object)_dataSource.PersonalFeed.NotificationList).Count; j++)
			{
				((Collection<SPPersonalKillNotificationItemVM>)(object)_dataSource.PersonalFeed.NotificationList)[j].IsPaused = isPaused;
			}
		}
	}

	private void OnOptionChange(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 19)
		{
			_isGeneralFeedEnabled = BannerlordConfig.KillFeedVisualType < 2;
		}
		else if ((int)changedManagedOptionsType == 21)
		{
			_isPersonalFeedEnabled = BannerlordConfig.ReportPersonalDamage;
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Invalid comparison between Unknown and I4
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		if (base.IsViewCreated && affectorAgent != null && ((int)agentState == 4 || (int)agentState == 3))
		{
			bool flag = ((KillingBlow)(ref killingBlow)).IsHeadShot();
			if (_isPersonalFeedEnabled && affectorAgent == Agent.Main && (affectedAgent.IsHuman || affectedAgent.IsMount))
			{
				bool flag2 = affectedAgent.Team == affectorAgent.Team || affectedAgent.IsFriendOf(affectorAgent);
				SPKillFeedVM dataSource = _dataSource;
				int inflictedDamage = killingBlow.InflictedDamage;
				bool isMount = affectedAgent.IsMount;
				BasicCharacterObject character = affectedAgent.Character;
				dataSource.OnPersonalKill(inflictedDamage, isMount, flag2, flag, (character != null) ? ((object)character.Name).ToString() : null, (int)agentState == 3);
			}
			if (_isGeneralFeedEnabled && affectedAgent.IsHuman)
			{
				_dataSource.OnAgentRemoved(affectedAgent, affectorAgent, flag, affectedAgent == affectorAgent, affectedAgent == affectorAgent && affectedAgent.IsInWater());
			}
		}
	}

	private void OnCombatLogManagerOnPrintCombatLog(CombatLogData logData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (_isPersonalFeedEnabled && (logData.IsAttackerAgentMine || logData.IsAttackerAgentRiderAgentMine) && ((CombatLogData)(ref logData)).TotalDamage > 0 && (!logData.IsFatalDamage || (logData.IsEntityToEntityCollisionDamage && logData.IsSpecialDamage)))
		{
			_dataSource.OnPersonalDamage(((CombatLogData)(ref logData)).TotalDamage, logData.IsVictimAgentMount, logData.IsFriendlyFire || logData.IsVictimAgentMine, logData.VictimAgentName);
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
