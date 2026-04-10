using System;
using System.Collections.Generic;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.KillFeed;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerKillNotificationUIHandler))]
public class MissionGauntletKillNotificationUIHandler : MissionView
{
	private MPKillFeedVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private MissionMultiplayerTeamDeathmatchClient _tdmClient;

	private MissionMultiplayerSiegeClient _siegeClient;

	private MissionMultiplayerGameModeDuelClient _duelClient;

	private MissionMultiplayerGameModeFlagDominationClient _flagDominationClient;

	private bool _isGeneralFeedEnabled;

	private bool _doesGameModeAllowGeneralFeed = true;

	private bool _isPersonalFeedEnabled;

	public override void OnMissionScreenInitialize()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		base.ViewOrderPriority = 2;
		_isGeneralFeedEnabled = _doesGameModeAllowGeneralFeed && BannerlordConfig.KillFeedVisualType < 2;
		_isPersonalFeedEnabled = BannerlordConfig.ReportPersonalDamage;
		_dataSource = new MPKillFeedVM();
		_gauntletLayer = new GauntletLayer("MultiplayerKillFeed", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerKillFeed", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		CombatLogManager.OnGenerateCombatLog += OnCombatLogManagerOnPrintCombatLog;
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnOptionChange));
	}

	private void OnOptionChange(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 19)
		{
			_isGeneralFeedEnabled = _doesGameModeAllowGeneralFeed && BannerlordConfig.KillFeedVisualType < 2;
		}
		else if ((int)changedManagedOptionsType == 21)
		{
			_isPersonalFeedEnabled = BannerlordConfig.ReportPersonalDamage;
		}
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_tdmClient = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerTeamDeathmatchClient>();
		if (_tdmClient != null)
		{
			_tdmClient.OnGoldGainEvent += OnGoldGain;
		}
		_siegeClient = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerSiegeClient>();
		if (_siegeClient != null)
		{
			_siegeClient.OnGoldGainEvent += OnGoldGain;
		}
		_flagDominationClient = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeFlagDominationClient>();
		if (_flagDominationClient != null)
		{
			_flagDominationClient.OnGoldGainEvent += OnGoldGain;
		}
		_duelClient = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeDuelClient>();
		if (_duelClient != null)
		{
			_doesGameModeAllowGeneralFeed = false;
		}
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenFinalize();
		CombatLogManager.OnGenerateCombatLog -= OnCombatLogManagerOnPrintCombatLog;
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnOptionChange));
		if (_tdmClient != null)
		{
			_tdmClient.OnGoldGainEvent -= OnGoldGain;
		}
		if (_siegeClient != null)
		{
			_siegeClient.OnGoldGainEvent -= OnGoldGain;
		}
		if (_flagDominationClient != null)
		{
			_flagDominationClient.OnGoldGainEvent -= OnGoldGain;
		}
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	private void OnGoldGain(GoldGain goldGainMessage)
	{
		if (!_isPersonalFeedEnabled)
		{
			return;
		}
		foreach (KeyValuePair<ushort, int> goldChangeEvent in goldGainMessage.GoldChangeEventList)
		{
			_dataSource.PersonalCasualty.OnGoldChange(goldChangeEvent.Value, (GoldGainFlags)goldChangeEvent.Key);
		}
	}

	private void OnCombatLogManagerOnPrintCombatLog(CombatLogData logData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (_isPersonalFeedEnabled && (logData.IsAttackerAgentMine || logData.IsAttackerAgentRiderAgentMine) && ((CombatLogData)(ref logData)).TotalDamage > 0 && !logData.IsVictimAgentSameAsAttackerAgent)
		{
			_dataSource.OnPersonalDamage(((CombatLogData)(ref logData)).TotalDamage, logData.IsFatalDamage, logData.IsVictimAgentMount, logData.IsFriendlyFire, (int)logData.BodyPartHit == 0, logData.VictimAgentName);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		if (_isGeneralFeedEnabled && !GameNetwork.IsDedicatedServer && affectorAgent != null && affectedAgent.IsHuman && ((int)agentState == 4 || (int)agentState == 3))
		{
			_dataSource.OnAgentRemoved(affectedAgent, affectorAgent, _isPersonalFeedEnabled);
		}
	}
}
