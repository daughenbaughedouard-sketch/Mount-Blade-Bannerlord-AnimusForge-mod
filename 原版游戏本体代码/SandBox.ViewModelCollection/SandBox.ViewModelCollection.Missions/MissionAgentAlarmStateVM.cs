using System.Collections.Generic;
using System.Collections.ObjectModel;
using SandBox.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.ViewModelCollection.Missions;

public class MissionAgentAlarmStateVM : ViewModel
{
	private bool _isInitialized;

	private Mission _mission;

	private Camera _camera;

	private DisguiseMissionLogic _disguiseMissionLogic;

	private bool _areStealthBoxesDirty;

	private List<StealthBox> _stealthBoxes;

	private bool _isMainAgentInSafeArea;

	private MBBindingList<MissionAgentAlarmTargetVM> _targets;

	[DataSourceProperty]
	public MBBindingList<MissionAgentAlarmTargetVM> Targets
	{
		get
		{
			return _targets;
		}
		set
		{
			if (value != _targets)
			{
				_targets = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionAgentAlarmTargetVM>>(value, "Targets");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMainAgentInSafeArea
	{
		get
		{
			return _isMainAgentInSafeArea;
		}
		set
		{
			if (value != _isMainAgentInSafeArea)
			{
				_isMainAgentInSafeArea = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMainAgentInSafeArea");
			}
		}
	}

	public MissionAgentAlarmStateVM()
	{
		Targets = new MBBindingList<MissionAgentAlarmTargetVM>();
		_stealthBoxes = new List<StealthBox>();
	}

	public void Initialize(Mission mission, Camera camera)
	{
		_mission = mission;
		_camera = camera;
		_isInitialized = true;
		_areStealthBoxesDirty = true;
		RefreshTargets();
		StealthBox.OnBoxInitialized += OnStealthBoxInitialized;
		StealthBox.OnBoxRemoved += OnStealthBoxRemoved;
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		StealthBox.OnBoxInitialized -= OnStealthBoxInitialized;
		StealthBox.OnBoxRemoved -= OnStealthBoxRemoved;
	}

	private void OnStealthBoxInitialized(StealthBox stealthBox)
	{
		_areStealthBoxesDirty = true;
	}

	private void OnStealthBoxRemoved(StealthBox stealthBox)
	{
		_areStealthBoxesDirty = true;
	}

	private void RefreshStealthBoxEntities()
	{
		_stealthBoxes.Clear();
		Mission current = Mission.Current;
		if ((NativeObject)(object)((current != null) ? current.Scene : null) == (NativeObject)null)
		{
			return;
		}
		List<GameEntity> list = new List<GameEntity>();
		Mission.Current.Scene.GetAllEntitiesWithScriptComponent<StealthBox>(ref list);
		for (int i = 0; i < list.Count; i++)
		{
			StealthBox firstScriptOfTypeRecursive = list[i].GetFirstScriptOfTypeRecursive<StealthBox>();
			if (firstScriptOfTypeRecursive != null)
			{
				_stealthBoxes.Add(firstScriptOfTypeRecursive);
			}
		}
	}

	public void Update()
	{
		if (!_isInitialized)
		{
			return;
		}
		if (_disguiseMissionLogic == null)
		{
			Mission mission = _mission;
			_disguiseMissionLogic = ((mission != null) ? mission.GetMissionBehavior<DisguiseMissionLogic>() : null);
		}
		bool isStealthModeEnabled = _disguiseMissionLogic?.IsInStealthMode ?? false;
		IsMainAgentInSafeArea = IsMainAgentInStealthArea();
		for (int i = 0; i < ((Collection<MissionAgentAlarmTargetVM>)(object)Targets).Count; i++)
		{
			MissionAgentAlarmTargetVM missionAgentAlarmTargetVM = ((Collection<MissionAgentAlarmTargetVM>)(object)Targets)[i];
			if (_disguiseMissionLogic == null)
			{
				missionAgentAlarmTargetVM.IsStealthModeEnabled = true;
				missionAgentAlarmTargetVM.IsMainAgentInVisibilityRange = SandBoxUIHelper.IsAgentInVisibilityRangeApproximate(missionAgentAlarmTargetVM.TargetAgent, Agent.Main);
				missionAgentAlarmTargetVM.IsInVision = true;
				missionAgentAlarmTargetVM.IsSuspected = missionAgentAlarmTargetVM.AlarmProgress > 0;
				missionAgentAlarmTargetVM.UpdateScreenPosition(_camera);
				missionAgentAlarmTargetVM.UpdateValues();
				continue;
			}
			missionAgentAlarmTargetVM.IsStealthModeEnabled = isStealthModeEnabled;
			DisguiseMissionLogic.ShadowingAgentOffenseInfo agentOffenseInfo = _disguiseMissionLogic.GetAgentOffenseInfo(missionAgentAlarmTargetVM.TargetAgent);
			if (agentOffenseInfo != null)
			{
				missionAgentAlarmTargetVM.IsMainAgentInVisibilityRange = SandBoxUIHelper.IsAgentInVisibilityRangeApproximate(missionAgentAlarmTargetVM.TargetAgent, Agent.Main);
				missionAgentAlarmTargetVM.IsInVision = agentOffenseInfo.CanPlayerCameraSeeTheAgent;
				missionAgentAlarmTargetVM.IsSuspected = missionAgentAlarmTargetVM.AlarmProgress > 0;
			}
			missionAgentAlarmTargetVM.UpdateScreenPosition(_camera);
			missionAgentAlarmTargetVM.UpdateValues();
		}
	}

	private bool IsMainAgentInStealthArea()
	{
		Agent main = Agent.Main;
		if (main == null)
		{
			return false;
		}
		Mission current = Mission.Current;
		if ((NativeObject)(object)((current != null) ? current.Scene : null) == (NativeObject)null)
		{
			return false;
		}
		if (_areStealthBoxesDirty)
		{
			RefreshStealthBoxEntities();
			_areStealthBoxesDirty = false;
		}
		for (int i = 0; i < _stealthBoxes.Count; i++)
		{
			if (_stealthBoxes[i].IsAgentInside(main))
			{
				return true;
			}
		}
		return false;
	}

	public void OnAgentRemoved(Agent agent)
	{
		MissionAgentAlarmTargetVM agentTargetFromAgent = GetAgentTargetFromAgent(agent);
		if (agentTargetFromAgent != null)
		{
			((Collection<MissionAgentAlarmTargetVM>)(object)Targets).Remove(agentTargetFromAgent);
		}
	}

	private void RefreshTargets()
	{
		((Collection<MissionAgentAlarmTargetVM>)(object)Targets).Clear();
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (item != null && SandBoxUIHelper.CanAgentBeAlarmed(item))
			{
				((Collection<MissionAgentAlarmTargetVM>)(object)Targets).Add(new MissionAgentAlarmTargetVM(item, OnRemoveTarget));
			}
		}
	}

	public void OnAgentBuild(Agent agent, Banner banner)
	{
		RefreshTargets();
	}

	public void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
	{
		if (agent != null && agent == Agent.Main)
		{
			RefreshTargets();
			return;
		}
		MissionAgentAlarmTargetVM agentTargetFromAgent = GetAgentTargetFromAgent(agent);
		if (agentTargetFromAgent == null && SandBoxUIHelper.CanAgentBeAlarmed(agent))
		{
			((Collection<MissionAgentAlarmTargetVM>)(object)Targets).Add(new MissionAgentAlarmTargetVM(agent, OnRemoveTarget));
		}
		else if (agentTargetFromAgent != null && (newTeam == Team.Invalid || newTeam == null || newTeam.IsPlayerAlly))
		{
			((Collection<MissionAgentAlarmTargetVM>)(object)Targets).Remove(agentTargetFromAgent);
		}
	}

	private void OnRemoveTarget(MissionAgentAlarmTargetVM targetToRemove)
	{
		((Collection<MissionAgentAlarmTargetVM>)(object)Targets).Remove(targetToRemove);
	}

	private MissionAgentAlarmTargetVM GetAgentTargetFromAgent(Agent agent)
	{
		for (int i = 0; i < ((Collection<MissionAgentAlarmTargetVM>)(object)Targets).Count; i++)
		{
			MissionAgentAlarmTargetVM missionAgentAlarmTargetVM = ((Collection<MissionAgentAlarmTargetVM>)(object)Targets)[i];
			if (missionAgentAlarmTargetVM.TargetAgent == agent)
			{
				return missionAgentAlarmTargetVM;
			}
		}
		return null;
	}
}
