using System.Collections.Generic;
using SandBox.Conversation.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Conversation;

public static class ConversationMission
{
	public static Agent OneToOneConversationAgent
	{
		get
		{
			IAgent oneToOneConversationAgent = Campaign.Current.ConversationManager.OneToOneConversationAgent;
			return (Agent)(object)((oneToOneConversationAgent is Agent) ? oneToOneConversationAgent : null);
		}
	}

	public static CharacterObject OneToOneConversationCharacter => Campaign.Current.ConversationManager.OneToOneConversationCharacter;

	public static Agent CurrentSpeakerAgent
	{
		get
		{
			IAgent speakerAgent = Campaign.Current.ConversationManager.SpeakerAgent;
			return (Agent)(object)((speakerAgent is Agent) ? speakerAgent : null);
		}
	}

	public static IEnumerable<Agent> ConversationAgents
	{
		get
		{
			foreach (IAgent conversationAgent in Campaign.Current.ConversationManager.ConversationAgents)
			{
				yield return (Agent)(object)((conversationAgent is Agent) ? conversationAgent : null);
			}
		}
	}

	public static void StartConversationWithAgent(Agent agent)
	{
		Mission.Current.GetMissionBehavior<MissionConversationLogic>()?.StartConversation(agent, setActionsInstantly: true);
	}
}
