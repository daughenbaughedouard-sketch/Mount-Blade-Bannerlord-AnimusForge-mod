using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.Localization;

namespace AnimusForge;

internal static class IssueQuestCompat
{
	private static readonly MethodInfo CheckPreconditionsMethod = typeof(IssueBase).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((MethodInfo method) => method.Name == "CheckPreconditions" && method.GetParameters().Length == 2);

	private static readonly PropertyInfo RewardGoldProperty = AccessTools.Property(typeof(IssueBase), "RewardGold");

	private static readonly FieldInfo DiscussDialogFlowField = AccessTools.Field(typeof(QuestBase), "DiscussDialogFlow");

	private static readonly PropertyInfo IsThereDiscussDialogFlowProperty = AccessTools.Property(typeof(QuestBase), "IsThereDiscussDialogFlow");

	internal static bool TryCheckPreconditions(IssueBase issue, Hero giver, out TextObject failureReason)
	{
		failureReason = null;
		if (issue == null || giver == null)
		{
			return false;
		}
		if (CheckPreconditionsMethod == null)
		{
			return true;
		}
		try
		{
			object[] array = new object[2] { giver, null };
			bool flag = (bool)CheckPreconditionsMethod.Invoke(issue, array);
			failureReason = array[1] as TextObject;
			return flag;
		}
		catch
		{
			return true;
		}
	}

	internal static int GetRewardGoldSafe(IssueBase issue)
	{
		if (issue == null || RewardGoldProperty == null)
		{
			return 0;
		}
		try
		{
			return Convert.ToInt32(RewardGoldProperty.GetValue(issue, null));
		}
		catch
		{
			return 0;
		}
	}

	internal static DialogFlow GetDiscussDialogFlowSafe(QuestBase quest)
	{
		if (quest == null)
		{
			return null;
		}
		try
		{
			return DiscussDialogFlowField?.GetValue(quest) as DialogFlow;
		}
		catch
		{
		}
		try
		{
			bool flag = (bool?)IsThereDiscussDialogFlowProperty?.GetValue(quest, null) ?? false;
			if (!flag)
			{
				return null;
			}
		}
		catch
		{
		}
		try
		{
			return quest.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((FieldInfo field) => typeof(DialogFlow).IsAssignableFrom(field.FieldType) && field.Name.IndexOf("DiscussDialogFlow", StringComparison.OrdinalIgnoreCase) >= 0)?.GetValue(quest) as DialogFlow;
		}
		catch
		{
			return null;
		}
	}

	internal static bool IsQuestOngoingSafe(QuestBase quest)
	{
		try
		{
			return quest?.IsOngoing ?? false;
		}
		catch
		{
			return false;
		}
	}

	internal static int GetJournalEntryCountSafe(QuestBase quest)
	{
		try
		{
			return quest?.JournalEntries?.Count ?? 0;
		}
		catch
		{
			return 0;
		}
	}
}
