using SandBox.Conversation.MissionLogics;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class TalkBehavior : AgentBehavior
{
	private bool _doNotMove;

	private bool _startConversation;

	public TalkBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
		_startConversation = true;
		_doNotMove = true;
	}

	public override void Tick(float dt, bool isSimulation)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if (!_startConversation || base.Mission.MainAgent == null || !base.Mission.MainAgent.IsActive() || (int)base.Mission.Mode == 1 || (int)base.Mission.Mode == 2 || (int)base.Mission.Mode == 5)
		{
			return;
		}
		float interactionDistanceToUsable = base.OwnerAgent.GetInteractionDistanceToUsable((IUsable)(object)base.Mission.MainAgent);
		Vec3 position = base.OwnerAgent.Position;
		MatrixFrame frame;
		Vec2 asVec;
		if (((Vec3)(ref position)).DistanceSquared(base.Mission.MainAgent.Position) < (interactionDistanceToUsable + 3f) * (interactionDistanceToUsable + 3f) && base.Navigator.CanSeeAgent(base.Mission.MainAgent))
		{
			AgentNavigator navigator = base.Navigator;
			WorldPosition worldPosition = base.OwnerAgent.GetWorldPosition();
			frame = base.OwnerAgent.Frame;
			asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
			navigator.SetTargetFrame(worldPosition, ((Vec2)(ref asVec)).RotationInRadians, 1f, -10f, (AIScriptedFrameFlags)16);
			MissionConversationLogic missionBehavior = base.Mission.GetMissionBehavior<MissionConversationLogic>();
			if (missionBehavior != null && missionBehavior.IsReadyForConversation)
			{
				((MissionBehavior)missionBehavior).OnAgentInteraction(base.Mission.MainAgent, base.OwnerAgent, (sbyte)(-1));
				_startConversation = false;
			}
		}
		else if (!_doNotMove)
		{
			AgentNavigator navigator2 = base.Navigator;
			WorldPosition worldPosition2 = Agent.Main.GetWorldPosition();
			frame = Agent.Main.Frame;
			asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
			navigator2.SetTargetFrame(worldPosition2, ((Vec2)(ref asVec)).RotationInRadians, 1f, -10f, (AIScriptedFrameFlags)16);
		}
	}

	public override float GetAvailability(bool isSimulation)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Invalid comparison between Unknown and I4
		if (isSimulation)
		{
			return 0f;
		}
		if (_startConversation && base.Mission.MainAgent != null && base.Mission.MainAgent.IsActive())
		{
			float num = base.OwnerAgent.GetInteractionDistanceToUsable((IUsable)(object)base.Mission.MainAgent) + 3f;
			Vec3 position = base.OwnerAgent.Position;
			if (((Vec3)(ref position)).DistanceSquared(base.Mission.MainAgent.Position) < num * num && (int)base.Mission.Mode != 1 && !base.Mission.MainAgent.IsEnemyOf(base.OwnerAgent))
			{
				return 1f;
			}
		}
		return 0f;
	}

	public override string GetDebugInfo()
	{
		return "Talk";
	}

	protected override void OnDeactivate()
	{
		base.Navigator.ClearTarget();
		Disable();
	}

	public void Disable()
	{
		_startConversation = false;
		_doNotMove = true;
	}

	public void Enable(bool doNotMove)
	{
		_startConversation = true;
		_doNotMove = doNotMove;
	}
}
