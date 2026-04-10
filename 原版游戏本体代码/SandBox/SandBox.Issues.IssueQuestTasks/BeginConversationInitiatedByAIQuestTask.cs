using System;
using SandBox.Conversation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace SandBox.Issues.IssueQuestTasks;

public class BeginConversationInitiatedByAIQuestTask : QuestTaskBase
{
	private bool _conversationOpened;

	private Agent _conversationAgent;

	public BeginConversationInitiatedByAIQuestTask(Agent agent, Action onSucceededAction, Action onFailedAction, Action onCanceledAction, DialogFlow dialogFlow = null)
		: base(dialogFlow, onSucceededAction, onFailedAction, onCanceledAction)
	{
		_conversationAgent = agent;
		((QuestTaskBase)this).IsLogged = false;
	}

	public void MissionTick(float dt)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		if (Mission.Current.MainAgent != null && _conversationAgent != null && !_conversationOpened && (int)Mission.Current.Mode != 1)
		{
			OpenConversation(_conversationAgent);
		}
	}

	private void OpenConversation(Agent agent)
	{
		ConversationMission.StartConversationWithAgent(agent);
	}

	protected override void OnFinished()
	{
		_conversationAgent = null;
	}

	public override void SetReferences()
	{
		CampaignEvents.MissionTickEvent.AddNonSerializedListener((object)this, (Action<float>)MissionTick);
	}
}
