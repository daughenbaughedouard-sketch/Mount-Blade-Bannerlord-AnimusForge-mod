using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions;

public class StealthFailCounterMissionLogic : MissionLogic
{
	private readonly List<Agent> _alarmedAgents = new List<Agent>();

	private Timer _failCounter;

	public float FailCounterSeconds = 5f;

	private TextObject _popupTitle;

	private TextObject _popupDescription;

	public bool IsActive { get; private set; } = true;

	public float FailCounterElapsedTime
	{
		get
		{
			if (!IsActive || _failCounter == null)
			{
				return -1f;
			}
			return _failCounter.ElapsedTime();
		}
	}

	public override void OnAgentAlarmedStateChanged(Agent agent, AIStateFlag flag)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentAlarmedStateChanged(agent, flag);
		if (agent.Team != null && !agent.Team.IsPlayerAlly)
		{
			if (agent.IsAlarmed() && !_alarmedAgents.Contains(agent))
			{
				_alarmedAgents.Add(agent);
			}
			else if (!agent.IsAlarmed() && _alarmedAgents.Contains(agent))
			{
				_alarmedAgents.Remove(agent);
			}
			if (_alarmedAgents.Count == 0)
			{
				IsActive = false;
			}
			else
			{
				IsActive = true;
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
		_alarmedAgents.Remove(affectedAgent);
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		((MissionBehavior)this).OnMissionTick(dt);
		if (!IsActive)
		{
			return;
		}
		if ((int)((MissionBehavior)this).Mission.Mode == 4)
		{
			if (_failCounter == null && !Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_alarmedAgents))
			{
				_failCounter = new Timer(((MissionBehavior)this).Mission.CurrentTime, FailCounterSeconds, true);
			}
			if (_failCounter != null)
			{
				if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)_alarmedAgents))
				{
					_failCounter = null;
				}
				else if (_failCounter.Check(((MissionBehavior)this).Mission.CurrentTime))
				{
					ShowMissionFailedPopup();
				}
			}
		}
		else
		{
			IsActive = false;
		}
	}

	public void SetFailTexts(TextObject title, TextObject description)
	{
		_popupTitle = title;
		_popupDescription = description;
	}

	private void ShowMissionFailedPopup()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Invalid comparison between Unknown and I4
		//IL_00ad: Expected O, but got Unknown
		IsActive = false;
		object obj = ((!TextObject.IsNullOrEmpty(_popupTitle)) ? ((object)_popupTitle) : ((object)new TextObject("{=wQbfWNZO}Mission Failed!", (Dictionary<string, object>)null)));
		TextObject val = (TextObject)((!TextObject.IsNullOrEmpty(_popupDescription)) ? ((object)_popupDescription) : ((object)new TextObject("{=5R0TauYV}You have been compromised.", (Dictionary<string, object>)null)));
		TextObject val2 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(obj.ToString(), ((object)val).ToString(), true, false, ((object)val2).ToString(), (string)null, (Action)delegate
		{
			Game.Current.EventManager.TriggerEvent<OnStealthMissionCounterFailedEvent>(new OnStealthMissionCounterFailedEvent());
			Mission.Current.EndMission();
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), (int)Campaign.Current.GameMode == 1, false);
	}
}
