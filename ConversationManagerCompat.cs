using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Conversation;

namespace AnimusForge;

internal static class ConversationManagerCompat
{
	internal static readonly FieldInfo SentencesField = AccessTools.Field(typeof(ConversationManager), "_sentences");

	internal static readonly FieldInfo StateMapField = AccessTools.Field(typeof(ConversationManager), "stateMap");

	internal static readonly FieldInfo NumberOfStateIndicesField = AccessTools.Field(typeof(ConversationManager), "_numberOfStateIndices");

	internal static readonly FieldInfo AutoIdField = AccessTools.Field(typeof(ConversationManager), "_autoId");

	internal static readonly FieldInfo AutoTokenField = AccessTools.Field(typeof(ConversationManager), "_autoToken");

	internal static readonly FieldInfo UsedIndicesField = AccessTools.Field(typeof(ConversationManager), "_usedIndices");

	internal static readonly FieldInfo CurrentSentenceField = AccessTools.Field(typeof(ConversationManager), "_currentSentence");

	internal static readonly FieldInfo CurrentSentenceTextField = AccessTools.Field(typeof(ConversationManager), "_currentSentenceText");

	internal static readonly FieldInfo LastSelectedDialogObjectField = AccessTools.Field(typeof(ConversationManager), "_lastSelectedDialogObject");

	internal static readonly FieldInfo CurrentRepeatedDialogSetIndexField = AccessTools.Field(typeof(ConversationManager), "_currentRepeatedDialogSetIndex");

	internal static readonly FieldInfo CurrentRepeatIndexField = AccessTools.Field(typeof(ConversationManager), "_currentRepeatIndex");

	internal static readonly FieldInfo DialogRepeatObjectsField = AccessTools.Field(typeof(ConversationManager), "_dialogRepeatObjects");

	internal static readonly FieldInfo DialogRepeatLinesField = AccessTools.Field(typeof(ConversationManager), "_dialogRepeatLines");

	internal static readonly FieldInfo IsActiveField = AccessTools.Field(typeof(ConversationManager), "_isActive");

	internal static readonly FieldInfo MainAgentField = AccessTools.Field(typeof(ConversationManager), "_mainAgent");

	internal static readonly FieldInfo SpeakerAgentField = AccessTools.Field(typeof(ConversationManager), "_speakerAgent");

	internal static readonly FieldInfo ListenerAgentField = AccessTools.Field(typeof(ConversationManager), "_listenerAgent");

	internal static readonly FieldInfo ConversationAgentsField = AccessTools.Field(typeof(ConversationManager), "_conversationAgents");

	internal static readonly FieldInfo ConversationPartyField = AccessTools.Field(typeof(ConversationManager), "_conversationParty");

	internal static readonly PropertyInfo CurOptionsProperty = AccessTools.Property(typeof(ConversationManager), "CurOptions");

	internal static readonly MethodInfo ProcessPartnerSentenceMethod = AccessTools.Method(typeof(ConversationManager), "ProcessPartnerSentence");

	internal static readonly MethodInfo ProcessSentenceMethod = AccessTools.Method(typeof(ConversationManager), "ProcessSentence");

	internal static readonly MethodInfo ResetRepeatedDialogSystemMethod = AccessTools.Method(typeof(ConversationManager), "ResetRepeatedDialogSystem");

	internal static bool TryInvokeProcessSentence(ConversationManager conversationManager, ConversationSentenceOption option)
	{
		if (conversationManager == null || ProcessSentenceMethod == null)
		{
			return false;
		}
		try
		{
			ProcessSentenceMethod.Invoke(conversationManager, new object[1] { option });
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static bool TryInvokeProcessPartnerSentence(ConversationManager conversationManager)
	{
		if (conversationManager == null || ProcessPartnerSentenceMethod == null)
		{
			return false;
		}
		try
		{
			ProcessPartnerSentenceMethod.Invoke(conversationManager, null);
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static bool TryResetRepeatedDialogSystem(ConversationManager conversationManager)
	{
		if (conversationManager == null || ResetRepeatedDialogSystemMethod == null)
		{
			return false;
		}
		try
		{
			ResetRepeatedDialogSystemMethod.Invoke(conversationManager, null);
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static bool TrySetCurOptions(ConversationManager conversationManager, List<ConversationSentenceOption> options)
	{
		if (conversationManager == null || CurOptionsProperty == null)
		{
			return false;
		}
		try
		{
			CurOptionsProperty.SetValue(conversationManager, options, null);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
