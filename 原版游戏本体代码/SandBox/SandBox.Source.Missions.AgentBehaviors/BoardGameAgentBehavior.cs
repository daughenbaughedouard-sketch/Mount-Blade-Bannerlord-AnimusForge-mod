using SandBox.Missions.AgentBehaviors;
using SandBox.Objects.Usables;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Source.Missions.AgentBehaviors;

public class BoardGameAgentBehavior : AgentBehavior
{
	private enum State
	{
		Idle,
		MovingToChair,
		Finish
	}

	private const int FinishDelayAsSeconds = 3;

	private Chair _chair;

	private State _state;

	private Timer _waitTimer;

	public BoardGameAgentBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
	}

	public override void Tick(float dt, bool isSimulation)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		switch (_state)
		{
		case State.Idle:
			if ((object)base.Navigator.TargetUsableMachine != _chair && !_chair.IsAgentFullySitting(base.OwnerAgent))
			{
				base.Navigator.SetTarget((UsableMachine)(object)_chair, isInitialTarget: false, (AIScriptedFrameFlags)0);
				_state = State.MovingToChair;
			}
			break;
		case State.MovingToChair:
			if (_chair.IsAgentFullySitting(base.OwnerAgent))
			{
				_state = State.Idle;
			}
			break;
		case State.Finish:
			if (base.OwnerAgent.IsUsingGameObject && _waitTimer == null)
			{
				base.Navigator.ClearTarget();
				_waitTimer = new Timer(base.Mission.CurrentTime, 3f, true);
			}
			else if (_waitTimer != null)
			{
				if (_waitTimer.Check(base.Mission.CurrentTime))
				{
					RemoveBoardGameBehaviorInternal();
				}
			}
			else
			{
				RemoveBoardGameBehaviorInternal();
			}
			break;
		}
	}

	public override void ConversationTick()
	{
		base.Navigator.ClearTarget();
		_state = State.Idle;
	}

	protected override void OnDeactivate()
	{
		base.Navigator.ClearTarget();
		_chair = null;
		_state = State.Idle;
		_waitTimer = null;
	}

	public override string GetDebugInfo()
	{
		return "BoardGameAgentBehavior";
	}

	public override float GetAvailability(bool isSimulation)
	{
		return 1f;
	}

	private void RemoveBoardGameBehaviorInternal()
	{
		InterruptingBehaviorGroup behaviorGroup = base.OwnerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<InterruptingBehaviorGroup>();
		if (behaviorGroup.GetBehavior<BoardGameAgentBehavior>() != null)
		{
			behaviorGroup.RemoveBehavior<BoardGameAgentBehavior>();
		}
	}

	public static void AddTargetChair(Agent ownerAgent, Chair chair)
	{
		InterruptingBehaviorGroup behaviorGroup = ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<InterruptingBehaviorGroup>();
		bool num = behaviorGroup.GetBehavior<BoardGameAgentBehavior>() == null;
		BoardGameAgentBehavior obj = behaviorGroup.GetBehavior<BoardGameAgentBehavior>() ?? behaviorGroup.AddBehavior<BoardGameAgentBehavior>();
		obj._chair = chair;
		obj._state = State.Idle;
		obj._waitTimer = null;
		if (num)
		{
			behaviorGroup.SetScriptedBehavior<BoardGameAgentBehavior>();
		}
	}

	public static void RemoveBoardGameBehaviorOfAgent(Agent ownerAgent)
	{
		BoardGameAgentBehavior behavior = ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<InterruptingBehaviorGroup>().GetBehavior<BoardGameAgentBehavior>();
		if (behavior != null)
		{
			behavior._chair = null;
			behavior._state = State.Finish;
		}
	}

	public static bool IsAgentMovingToChair(Agent ownerAgent)
	{
		if (ownerAgent == null)
		{
			return false;
		}
		BoardGameAgentBehavior boardGameAgentBehavior = ownerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<InterruptingBehaviorGroup>()?.GetBehavior<BoardGameAgentBehavior>();
		if (boardGameAgentBehavior == null)
		{
			return false;
		}
		return boardGameAgentBehavior._state == State.MovingToChair;
	}
}
