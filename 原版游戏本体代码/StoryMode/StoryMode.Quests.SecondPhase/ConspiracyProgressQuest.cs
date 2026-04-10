using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.SecondPhase;

public class ConspiracyProgressQuest : StoryModeQuestBase
{
	[SaveableField(2)]
	private JournalLog _startQuestLog;

	private bool _isImperialSide => StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine;

	private TextObject _startQuestLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=oX2aoilb}{MENTOR.NAME} knows of the rise of your {KINGDOM_NAME}. Rumors say {MENTOR.NAME} is planning to undo your progress. Be ready!", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", _isImperialSide ? StoryModeHeroes.AntiImperialMentor.CharacterObject : StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			val.SetTextVariable("KINGDOM_NAME", (Clan.PlayerClan.Kingdom != null) ? Clan.PlayerClan.Kingdom.Name : Clan.PlayerClan.Name);
			return val;
		}
	}

	private TextObject _questCanceledLogText => new TextObject("{=tVlZTOst}You have chosen a different path.", (Dictionary<string, object>)null);

	public override TextObject Title
	{
		get
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected O, but got Unknown
			TextObject val;
			if (_isImperialSide)
			{
				val = new TextObject("{=PJ5C3Dim}{ANTIIMPERIAL_MENTOR.NAME}'s Conspiracy", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ANTIIMPERIAL_MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			}
			else
			{
				val = new TextObject("{=i3SSc0I4}{IMPERIAL_MENTOR.NAME}'s Plan", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("IMPERIAL_MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			}
			return val;
		}
	}

	public ConspiracyProgressQuest()
		: base("conspiracy_quest_campaign_behavior", null, CampaignTime.Never)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		StoryMode.StoryModePhases.SecondPhase.Instance.TriggerConspiracy();
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
	}

	protected override void HourlyTick()
	{
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		StoryModeEvents.OnConspiracyActivatedEvent.AddNonSerializedListener((object)this, (Action)OnConspiracyActivated);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
	{
		if (clan == Clan.PlayerClan && oldKingdom == StoryModeManager.Current.MainStoryLine.PlayerSupportedKingdom)
		{
			((QuestBase)this).CompleteQuestWithCancel(_questCanceledLogText);
			StoryModeManager.Current.MainStoryLine.CancelSecondAndThirdPhase();
		}
	}

	protected override void OnStartQuest()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		_startQuestLog = ((QuestBase)this).AddDiscreteLog(_startQuestLogText, new TextObject("{=1LrHV647}Conspiracy Strength", (Dictionary<string, object>)null), (int)StoryMode.StoryModePhases.SecondPhase.Instance.ConspiracyStrength, 2000, (TextObject)null, false);
	}

	protected override void SetDialogs()
	{
	}

	protected override void OnFinalize()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		((QuestBase)this).OnFinalize();
		foreach (QuestBase item in ((IEnumerable<QuestBase>)Campaign.Current.QuestManager.Quests).ToList())
		{
			if (typeof(ConspiracyQuestBase) == ((object)item).GetType().BaseType && item.IsOngoing)
			{
				item.CompleteQuestWithCancel(new TextObject("{=YJxCbbpd}Conspiracy is activated!", (Dictionary<string, object>)null));
			}
		}
	}

	protected override void DailyTick()
	{
		StoryModeManager.Current.MainStoryLine.SecondPhase.IncreaseConspiracyStrength();
		_startQuestLog.UpdateCurrentProgress((int)StoryModeManager.Current.MainStoryLine.SecondPhase.ConspiracyStrength);
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)detail == 1 && typeof(ConspiracyQuestBase) == ((object)quest).GetType().BaseType)
		{
			_startQuestLog.UpdateCurrentProgress((int)StoryModeManager.Current.MainStoryLine.SecondPhase.ConspiracyStrength);
		}
	}

	private void OnConspiracyActivated()
	{
		((QuestBase)this).CompleteQuestWithTimeOut((TextObject)null);
	}

	internal static void AutoGeneratedStaticCollectObjectsConspiracyProgressQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(ConspiracyProgressQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_startQuestLog);
	}

	internal static object AutoGeneratedGetMemberValue_startQuestLog(object o)
	{
		return ((ConspiracyProgressQuest)o)._startQuestLog;
	}
}
