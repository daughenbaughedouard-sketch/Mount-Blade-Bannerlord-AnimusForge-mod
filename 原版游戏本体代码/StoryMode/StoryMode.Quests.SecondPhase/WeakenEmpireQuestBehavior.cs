using System;
using System.Collections.Generic;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace StoryMode.Quests.SecondPhase;

public class WeakenEmpireQuestBehavior : CampaignBehaviorBase
{
	public class WeakenEmpireQuestBehaviorTypeDefiner : SaveableTypeDefiner
	{
		public WeakenEmpireQuestBehaviorTypeDefiner()
			: base(1005000)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(WeakenEmpireQuest), 1, (IObjectResolver)null);
		}
	}

	public class WeakenEmpireQuest : StoryModeQuestBase
	{
		private const int EmpireDefeatSettlementCount = 4;

		private bool _weakenedEmpire;

		private TextObject _startQuestLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Expected O, but got Unknown
				TextObject val = new TextObject("{=0wQlpbtL}In order for the Empire to go into its final decline, there should be fewer than {NUMBER} imperial-owned settlements. If this happens, another kingdom can become the dominant power in Calradia.", (Dictionary<string, object>)null);
				val.SetTextVariable("NUMBER", 4);
				return val;
			}
		}

		public override TextObject Title => new TextObject("{=iR4QCTxv}Weaken Empire", (Dictionary<string, object>)null);

		private TextObject _questCanceledLogText => new TextObject("{=tVlZTOst}You have chosen a different path.", (Dictionary<string, object>)null);

		public WeakenEmpireQuest(Hero questGiver)
			: base("weaken_empire_quest", questGiver, CampaignTime.Never)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_weakenedEmpire = false;
			((QuestBase)this).SetDialogs();
			((QuestBase)this).InitializeQuestOnCreation();
			((QuestBase)this).AddLog(_startQuestLog, false);
		}

		protected override void SetDialogs()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			((QuestBase)this).DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("{=VeY3PQFL}You chose to defeat the Empire.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver))
				.CloseDialog();
		}

		protected override void InitializeQuestOnGameLoad()
		{
			((QuestBase)this).SetDialogs();
		}

		protected override void RegisterEvents()
		{
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

		protected override void HourlyTick()
		{
			if (QuestConditionsHold())
			{
				SuccessComplete();
			}
		}

		private void OnConspiracyActivated()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			if (!_weakenedEmpire)
			{
				((QuestBase)this).CompleteQuestWithFail(new TextObject("{=JVkPkbdg}You could not weaken the Empire.", (Dictionary<string, object>)null));
			}
		}

		private bool QuestConditionsHold()
		{
			return ((List<Town>)(object)StoryModeData.NorthernEmpireKingdom.Towns).Count + ((List<Town>)(object)StoryModeData.WesternEmpireKingdom.Towns).Count + ((List<Town>)(object)StoryModeData.SouthernEmpireKingdom.Towns).Count < 4;
		}

		private void SuccessComplete()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			((QuestBase)this).AddLog(new TextObject("{=wO19nK2y}You have weakened the Empire.", (Dictionary<string, object>)null), false);
			((QuestBase)this).CompleteQuestWithSuccess();
			_weakenedEmpire = true;
			StoryMode.StoryModePhases.SecondPhase.Instance.ActivateConspiracy();
		}

		internal static void AutoGeneratedStaticCollectObjectsWeakenEmpireQuest(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(WeakenEmpireQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}
	}

	public override void RegisterEvents()
	{
		StoryModeEvents.OnMainStoryLineSideChosenEvent.AddNonSerializedListener((object)this, (Action<MainStoryLineSide>)OnMainStoryLineSideChosen);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnMainStoryLineSideChosen(MainStoryLineSide side)
	{
		if (side == MainStoryLineSide.CreateAntiImperialKingdom || side == MainStoryLineSide.SupportAntiImperialKingdom)
		{
			((QuestBase)new WeakenEmpireQuest(StoryModeHeroes.AntiImperialMentor)).StartQuest();
		}
	}
}
