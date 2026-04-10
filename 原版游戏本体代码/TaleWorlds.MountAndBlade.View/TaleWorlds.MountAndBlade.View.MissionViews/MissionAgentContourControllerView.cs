using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class MissionAgentContourControllerView : MissionView
{
	private const bool IsEnabled = false;

	private uint _nonFocusedContourColor;

	private uint _focusedContourColor;

	private uint _friendlyContourColor;

	private List<Agent> _contourAgents;

	private Agent _currentFocusedAgent;

	private bool _isContourAppliedToAllAgents;

	private bool _isContourAppliedToFocusedAgent;

	private bool _isMultiplayer;

	private bool _isAllowedByOption
	{
		get
		{
			if (BannerlordConfig.HideBattleUI)
			{
				return GameNetwork.IsMultiplayer;
			}
			return true;
		}
	}

	public MissionAgentContourControllerView()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Color val = new Color(0.85f, 0.85f, 0.85f, 1f);
		_nonFocusedContourColor = ((Color)(ref val)).ToUnsignedInteger();
		val = new Color(1f, 0.84f, 0.35f, 1f);
		_focusedContourColor = ((Color)(ref val)).ToUnsignedInteger();
		val = new Color(0.44f, 0.83f, 0.26f, 1f);
		_friendlyContourColor = ((Color)(ref val)).ToUnsignedInteger();
		base._002Ector();
		_contourAgents = new List<Agent>();
		_isMultiplayer = GameNetwork.IsSessionActive;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (_isAllowedByOption)
		{
			_ = NativeConfig.GetUIDebugMode;
		}
	}

	private void PopulateContourListWithAgents()
	{
		_contourAgents.Clear();
		Mission mission = ((MissionBehavior)this).Mission;
		object obj;
		if (mission == null)
		{
			obj = null;
		}
		else
		{
			Team playerTeam = mission.PlayerTeam;
			obj = ((playerTeam != null) ? playerTeam.PlayerOrderController : null);
		}
		if (obj == null)
		{
			return;
		}
		foreach (Formation item in (List<Formation>)(object)Mission.Current.PlayerTeam.PlayerOrderController.SelectedFormations)
		{
			item.ApplyActionOnEachUnit((Action<Agent>)delegate(Agent agent)
			{
				if (!agent.IsMainAgent)
				{
					_contourAgents.Add(agent);
				}
			}, (Agent)null);
		}
	}

	public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
	{
		((MissionBehavior)this).OnFocusGained(agent, focusableObject, isInteractable);
		_ = _isAllowedByOption;
	}

	public override void OnFocusLost(Agent agent, IFocusable focusableObject)
	{
		((MissionBehavior)this).OnFocusLost(agent, focusableObject);
		if (_isAllowedByOption)
		{
			RemoveContourFromFocusedAgent();
			_currentFocusedAgent = null;
		}
	}

	private void AddContourToFocusedAgent()
	{
		if (_currentFocusedAgent != null && !_isContourAppliedToFocusedAgent)
		{
			MBAgentVisuals agentVisuals = _currentFocusedAgent.AgentVisuals;
			if (agentVisuals != null)
			{
				agentVisuals.SetContourColor((uint?)_focusedContourColor, true);
			}
			_isContourAppliedToFocusedAgent = true;
		}
	}

	private void RemoveContourFromFocusedAgent()
	{
		if (_currentFocusedAgent == null || !_isContourAppliedToFocusedAgent)
		{
			return;
		}
		if (_contourAgents.Contains(_currentFocusedAgent))
		{
			MBAgentVisuals agentVisuals = _currentFocusedAgent.AgentVisuals;
			if (agentVisuals != null)
			{
				agentVisuals.SetContourColor((uint?)_nonFocusedContourColor, true);
			}
		}
		else
		{
			MBAgentVisuals agentVisuals2 = _currentFocusedAgent.AgentVisuals;
			if (agentVisuals2 != null)
			{
				agentVisuals2.SetContourColor((uint?)null, true);
			}
		}
		_isContourAppliedToFocusedAgent = false;
	}

	private void ApplyContourToAllAgents()
	{
		if (_isContourAppliedToAllAgents)
		{
			return;
		}
		foreach (Agent contourAgent in _contourAgents)
		{
			uint value = ((contourAgent == _currentFocusedAgent) ? _focusedContourColor : (_isMultiplayer ? _friendlyContourColor : _nonFocusedContourColor));
			MBAgentVisuals agentVisuals = contourAgent.AgentVisuals;
			if (agentVisuals != null)
			{
				agentVisuals.SetContourColor((uint?)value, true);
			}
		}
		_isContourAppliedToAllAgents = true;
	}

	private void RemoveContourFromAllAgents()
	{
		if (!_isContourAppliedToAllAgents)
		{
			return;
		}
		foreach (Agent contourAgent in _contourAgents)
		{
			if (_currentFocusedAgent == null || contourAgent != _currentFocusedAgent)
			{
				MBAgentVisuals agentVisuals = contourAgent.AgentVisuals;
				if (agentVisuals != null)
				{
					agentVisuals.SetContourColor((uint?)null, true);
				}
			}
		}
		_isContourAppliedToAllAgents = false;
	}
}
