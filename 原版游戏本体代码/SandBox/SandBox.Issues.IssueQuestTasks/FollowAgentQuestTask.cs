using System;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Issues.IssueQuestTasks;

public class FollowAgentQuestTask : QuestTaskBase
{
	private Agent _followedAgent;

	private CharacterObject _followedAgentChar;

	private GameEntity _targetEntity;

	private Agent _targetAgent;

	public FollowAgentQuestTask(Agent followedAgent, GameEntity targetEntity, Action onSucceededAction, Action onCanceledAction, DialogFlow dialogFlow = null)
		: base(dialogFlow, onSucceededAction, (Action)null, onCanceledAction)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		_followedAgent = followedAgent;
		_followedAgentChar = (CharacterObject)_followedAgent.Character;
		_targetEntity = targetEntity;
		StartAgentMovement();
	}

	public FollowAgentQuestTask(Agent followedAgent, Agent targetAgent, Action onSucceededAction, Action onCanceledAction, DialogFlow dialogFlow = null)
		: base(dialogFlow, onSucceededAction, (Action)null, onCanceledAction)
	{
		_followedAgent = followedAgent;
		_targetAgent = targetAgent;
		StartAgentMovement();
	}

	private void StartAgentMovement()
	{
		if (_targetEntity != (GameEntity)null)
		{
			UsableMachine firstScriptOfType = _targetEntity.GetFirstScriptOfType<UsableMachine>();
			ScriptBehavior.AddUsableMachineTarget(_followedAgent, firstScriptOfType);
		}
		else if (_targetAgent != null)
		{
			ScriptBehavior.AddAgentTarget(_followedAgent, _targetAgent);
		}
	}

	public void MissionTick(float dt)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		ScriptBehavior scriptBehavior = (ScriptBehavior)_followedAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehavior<ScriptBehavior>();
		if (scriptBehavior == null || !scriptBehavior.IsNearTarget(_targetAgent))
		{
			return;
		}
		Vec2 currentVelocity = _followedAgent.GetCurrentVelocity();
		if (((Vec2)(ref currentVelocity)).LengthSquared < 0.0001f)
		{
			Vec3 position = _followedAgent.Position;
			if (((Vec3)(ref position)).DistanceSquared(Mission.Current.MainAgent.Position) < 16f)
			{
				((QuestTaskBase)this).Finish((FinishStates)0);
			}
		}
	}

	protected override void OnFinished()
	{
		_followedAgent = null;
		_followedAgentChar = null;
		_targetEntity = null;
		_targetAgent = null;
	}

	public override void SetReferences()
	{
		CampaignEvents.MissionTickEvent.AddNonSerializedListener((object)this, (Action<float>)MissionTick);
	}
}
