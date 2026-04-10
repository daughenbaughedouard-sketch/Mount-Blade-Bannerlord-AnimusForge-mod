using System.Collections.Generic;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.ViewModelCollection.Missions.NameMarkers;
using SandBox.ViewModelCollection.Missions.NameMarker;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.View.Missions;

public class NavalMissionNameMarkerProvider : MissionNameMarkerProvider
{
	private MissionShip _lastSteppedShip;

	private MissionShip _lastControlledShip;

	private AgentNavalComponent _mainAgentNavalComponent;

	private NavalShipsLogic _navalShipsLogic;

	protected override void OnInitialize(Mission mission)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		((MissionNameMarkerProvider)this).OnInitialize(mission);
		Agent main = Agent.Main;
		_mainAgentNavalComponent = ((main != null) ? main.GetComponent<AgentNavalComponent>() : null);
		_navalShipsLogic = mission.GetMissionBehavior<NavalShipsLogic>();
		mission.OnMainAgentChanged += new OnMainAgentChangedDelegate(OnMainAgentChanged);
	}

	protected override void OnDestroy(Mission mission)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		((MissionNameMarkerProvider)this).OnDestroy(mission);
		mission.OnMainAgentChanged -= new OnMainAgentChangedDelegate(OnMainAgentChanged);
	}

	private void OnMainAgentChanged(Agent oldAgent)
	{
		Agent main = Agent.Main;
		_mainAgentNavalComponent = ((main != null) ? main.GetComponent<AgentNavalComponent>() : null);
		((MissionNameMarkerProvider)this).SetMarkersDirty();
	}

	protected override void OnTick(float dt)
	{
		((MissionNameMarkerProvider)this).OnTick(dt);
		if (_mainAgentNavalComponent == null)
		{
			Agent main = Agent.Main;
			_mainAgentNavalComponent = ((main != null) ? main.GetComponent<AgentNavalComponent>() : null);
		}
		if (_mainAgentNavalComponent?.SteppedShip != _lastSteppedShip)
		{
			_lastSteppedShip = _mainAgentNavalComponent?.SteppedShip;
			((MissionNameMarkerProvider)this).SetMarkersDirty();
		}
		if (_navalShipsLogic?.PlayerControlledShip != _lastControlledShip)
		{
			_lastControlledShip = _navalShipsLogic?.PlayerControlledShip;
			((MissionNameMarkerProvider)this).SetMarkersDirty();
		}
	}

	public override void CreateMarkers(List<MissionNameMarkerTargetBaseVM> markers)
	{
		if (_lastSteppedShip == null || _lastSteppedShip == _lastControlledShip)
		{
			return;
		}
		bool flag = false;
		ShipControllerMachine shipControllerMachine = _lastSteppedShip.ShipControllerMachine;
		bool flag2 = false;
		for (int i = 0; i < markers.Count; i++)
		{
			if (markers[i] is NavalMissionShipControlPointMarkerTargetVM navalMissionShipControlPointMarkerTargetVM && ((MissionNameMarkerTargetVM<ShipControllerMachine>)navalMissionShipControlPointMarkerTargetVM).Target == shipControllerMachine && ((MissionNameMarkerTargetBaseVM)navalMissionShipControlPointMarkerTargetVM).IsPersistent == flag)
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			NavalMissionShipControlPointMarkerTargetVM navalMissionShipControlPointMarkerTargetVM2 = new NavalMissionShipControlPointMarkerTargetVM(shipControllerMachine);
			((MissionNameMarkerTargetBaseVM)navalMissionShipControlPointMarkerTargetVM2).IsPersistent = flag;
			markers.Add((MissionNameMarkerTargetBaseVM)(object)navalMissionShipControlPointMarkerTargetVM2);
		}
	}
}
