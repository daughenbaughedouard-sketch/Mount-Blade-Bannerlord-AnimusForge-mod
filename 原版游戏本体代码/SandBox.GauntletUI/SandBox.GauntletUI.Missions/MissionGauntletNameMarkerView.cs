using System;
using System.Collections.Generic;
using SandBox.View.Missions.NameMarkers;
using SandBox.ViewModelCollection.Missions.NameMarker;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(MissionNameMarkerUIHandler))]
public class MissionGauntletNameMarkerView : MissionNameMarkerUIHandler
{
	private GauntletLayer _gauntletLayer;

	private MissionNameMarkerVM _dataSource;

	private List<MissionNameMarkerProvider> _nameMarkerProviders;

	private int _lastVisualTrackerVersion;

	public override void OnMissionScreenInitialize()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		((MissionBattleUIBaseView)this).OnMissionScreenInitialize();
		_nameMarkerProviders = MissionNameMarkerFactory.CollectProviders();
		for (int i = 0; i < _nameMarkerProviders.Count; i++)
		{
			_nameMarkerProviders[i].Initialize(((MissionBehavior)this).Mission, SetMarkersDirty);
		}
		_dataSource = new MissionNameMarkerVM(_nameMarkerProviders, ((MissionView)this).MissionScreen.CombatCamera);
		_gauntletLayer = new GauntletLayer("MissionNameMarker", 1, false);
		_gauntletLayer.LoadMovie("NameMarker", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		if (Campaign.Current != null)
		{
			_lastVisualTrackerVersion = Campaign.Current.VisualTrackerManager.TrackedObjectsVersion;
			CampaignEvents.ConversationEnded.AddNonSerializedListener((object)this, (Action<IEnumerable<CharacterObject>>)OnConversationEnd);
		}
		MissionNameMarkerFactory.OnProvidersChanged += OnMarkersChanged;
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionBattleUIBaseView)this).OnMissionScreenFinalize();
		for (int i = 0; i < _nameMarkerProviders.Count; i++)
		{
			_nameMarkerProviders[i].Destroy(((MissionBehavior)this).Mission);
		}
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		if (Campaign.Current != null)
		{
			((IMbEventBase)CampaignEvents.ConversationEnded).ClearListeners((object)this);
		}
		InformationManager.HideAllMessages();
		MissionNameMarkerFactory.OnProvidersChanged -= OnMarkersChanged;
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionBattleUIBaseView)this).OnMissionScreenTick(dt);
		if (((MissionBattleUIBaseView)this).IsViewCreated)
		{
			if (((MissionView)this).IsViewSuspended != ((ScreenLayer)_gauntletLayer).IsActive)
			{
				ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, ((MissionView)this).IsViewSuspended);
			}
			for (int i = 0; i < _nameMarkerProviders.Count; i++)
			{
				_nameMarkerProviders[i].Tick(dt);
			}
			if (((MissionView)this).Input.IsGameKeyDown(5))
			{
				_dataSource.IsEnabled = true;
			}
			else
			{
				_dataSource.IsEnabled = false;
			}
			if (Campaign.Current != null && _lastVisualTrackerVersion != Campaign.Current.VisualTrackerManager.TrackedObjectsVersion)
			{
				SetMarkersDirty();
				_lastVisualTrackerVersion = Campaign.Current.VisualTrackerManager.TrackedObjectsVersion;
			}
			_dataSource.Tick(dt);
		}
	}

	private void OnMarkersChanged()
	{
		MissionNameMarkerFactory.UpdateProviders(_nameMarkerProviders.ToArray(), out var addedProviders, out var removedProviders);
		for (int i = 0; i < removedProviders.Count; i++)
		{
			_nameMarkerProviders.Remove(removedProviders[i]);
		}
		for (int j = 0; j < addedProviders.Count; j++)
		{
			_nameMarkerProviders.Add(addedProviders[j]);
		}
		SetMarkersDirty();
	}

	public override void SetMarkersDirty()
	{
		_dataSource?.SetTargetsDirty();
	}

	public override void OnAgentBuild(Agent affectedAgent, Banner banner)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		((MissionBehavior)this).OnAgentBuild(affectedAgent, banner);
		if ((int)((MissionBehavior)this).Mission.Mode != 2)
		{
			SetMarkersDirty();
		}
	}

	public override void OnAgentDeleted(Agent affectedAgent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)((MissionBehavior)this).Mission.Mode != 2)
		{
			SetMarkersDirty();
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)((MissionBehavior)this).Mission.Mode != 2)
		{
			SetMarkersDirty();
		}
	}

	private void OnConversationEnd(IEnumerable<CharacterObject> conversationCharacters)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)((MissionBehavior)this).Mission.Mode != 2)
		{
			SetMarkersDirty();
		}
	}

	public override void OnPhotoModeActivated()
	{
		((MissionView)this).OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		((MissionView)this).OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}

	protected override void OnResumeView()
	{
		base.OnResumeView();
	}

	protected override void OnSuspendView()
	{
		base.OnSuspendView();
	}
}
