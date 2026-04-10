using System.Collections.Generic;
using System.Collections.ObjectModel;
using SandBox.Missions.MissionLogics;
using SandBox.View.Missions;
using SandBox.ViewModelCollection;
using SandBox.ViewModelCollection.Missions.MainAgentDetection;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace Sandobx.GauntletUI.Missions;

[OverrideView(typeof(MissionMainAgentDetectionView))]
public class GauntletMainAgentDetectionView : MissionMainAgentDetectionView
{
	private GauntletLayer _markersGauntletLayer;

	private GauntletLayer _losingTargetGauntletLayer;

	private GauntletLayer _detectionBarGauntletLayer;

	private MainAgentDetectionVM _detectionDataSource;

	private MissionDisguiseMarkersVM _markersDataSource;

	private MissionLosingTargetVM _losingTargetDataSource;

	private DisguiseMissionLogic _disguiseMissionLogic;

	private float _lastSuspicousLevel;

	public override void OnMissionScreenInitialize()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_detectionDataSource = new MainAgentDetectionVM();
		_losingTargetDataSource = new MissionLosingTargetVM();
		_markersDataSource = new MissionDisguiseMarkersVM();
		_detectionBarGauntletLayer = new GauntletLayer("MissionMainAgentDetection", 10, false);
		_detectionBarGauntletLayer.LoadMovie("MissionMainAgentDetection", (ViewModel)(object)_detectionDataSource);
		_losingTargetGauntletLayer = new GauntletLayer("MissionLosingTarget", 11, false);
		_losingTargetGauntletLayer.LoadMovie("MissionLosingTarget", (ViewModel)(object)_losingTargetDataSource);
		_markersGauntletLayer = new GauntletLayer("MissionDetectionMarkers", 12, false);
		_markersGauntletLayer.LoadMovie("MissionDetectionMarkers", (ViewModel)(object)_markersDataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_detectionBarGauntletLayer);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_losingTargetGauntletLayer);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_markersGauntletLayer);
	}

	public override void AfterStart()
	{
		_disguiseMissionLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<DisguiseMissionLogic>();
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		((ViewModel)_detectionDataSource).OnFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_detectionBarGauntletLayer);
		_detectionBarGauntletLayer = null;
		_detectionDataSource = null;
		_disguiseMissionLogic = null;
	}

	private void UpdateSuspicion(float dt)
	{
		_detectionDataSource.UpdateDetectionValues(0f, 1f, _disguiseMissionLogic.PlayerSuspiciousLevel);
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (_disguiseMissionLogic != null)
		{
			if (_losingTargetDataSource != null)
			{
				UpdateLosingTarget(dt);
			}
			if (_detectionDataSource != null)
			{
				UpdateSuspicion(dt);
			}
			if (_markersDataSource != null)
			{
				UpdateMarkers(dt);
			}
			_lastSuspicousLevel = _disguiseMissionLogic.PlayerSuspiciousLevel;
		}
	}

	private void UpdateLosingTarget(float dt)
	{
		_losingTargetDataSource.UpdateLosingTargetValues(isLosingTarget: false, 0f, 1f);
	}

	private void UpdateMarkers(float dt)
	{
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		bool isInStealthMode = _disguiseMissionLogic.IsInStealthMode;
		bool isSuspicious = isInStealthMode || _lastSuspicousLevel < _disguiseMissionLogic.PlayerSuspiciousLevel;
		List<MissionDisguiseMarkerItemVM> list = new List<MissionDisguiseMarkerItemVM>();
		foreach (MissionDisguiseMarkerItemVM item in (Collection<MissionDisguiseMarkerItemVM>)(object)_markersDataSource.HostileAgents)
		{
			if (_disguiseMissionLogic.GetAgentOffenseInfo(item?.OffenseInfo.Agent) == null)
			{
				list.Add(item);
			}
		}
		foreach (KeyValuePair<Agent, DisguiseMissionLogic.ShadowingAgentOffenseInfo> threatAgentInfo in _disguiseMissionLogic.ThreatAgentInfos)
		{
			bool flag = true;
			foreach (MissionDisguiseMarkerItemVM item2 in (Collection<MissionDisguiseMarkerItemVM>)(object)_markersDataSource.HostileAgents)
			{
				if (threatAgentInfo.Key != null && item2.OffenseInfo?.Agent == threatAgentInfo.Key)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				((Collection<MissionDisguiseMarkerItemVM>)(object)_markersDataSource.HostileAgents).Add(new MissionDisguiseMarkerItemVM(((MissionView)this).MissionScreen.CombatCamera, threatAgentInfo.Value));
			}
		}
		foreach (MissionDisguiseMarkerItemVM item3 in list)
		{
			((Collection<MissionDisguiseMarkerItemVM>)(object)_markersDataSource.HostileAgents).Remove(item3);
		}
		float num = default(float);
		foreach (MissionDisguiseMarkerItemVM item4 in (Collection<MissionDisguiseMarkerItemVM>)(object)_markersDataSource.HostileAgents)
		{
			if (item4.OffenseInfo.Agent.IsActive())
			{
				Vec3 origin = ((MissionView)this).MissionScreen.CombatCamera.Frame.origin;
				Vec3 eyeGlobalPosition = item4.OffenseInfo.Agent.GetEyeGlobalPosition();
				bool flag2 = isInStealthMode || !((MissionBehavior)this).Mission.Scene.RayCastForClosestEntityOrTerrain(origin, eyeGlobalPosition, ref num, 0.035f, (BodyFlags)79617);
				item4.OffenseInfo.SetCanPlayerCameraSeeTheAgent(flag2);
				item4.IsInVision = flag2;
				item4.IsInVisibilityRange = SandBoxUIHelper.IsAgentInVisibilityRangeApproximate(Agent.Main, item4.OffenseInfo.Agent);
				item4.IsStealthModeEnabled = isInStealthMode;
				item4.IsSuspicious = isSuspicious;
			}
			else
			{
				item4.IsInVision = false;
				item4.IsInVisibilityRange = false;
			}
			item4.UpdatePosition();
			item4.RefreshVisuals();
		}
	}
}
