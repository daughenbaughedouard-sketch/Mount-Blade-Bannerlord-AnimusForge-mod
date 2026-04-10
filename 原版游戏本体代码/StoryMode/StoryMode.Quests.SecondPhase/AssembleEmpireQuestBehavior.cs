using System;
using System.Collections.Generic;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace StoryMode.Quests.SecondPhase;

public class AssembleEmpireQuestBehavior : CampaignBehaviorBase
{
	public class AssembleEmpireQuestBehaviorTypeDefiner : SaveableTypeDefiner
	{
		public AssembleEmpireQuestBehaviorTypeDefiner()
			: base(1002000)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(AssembleEmpireQuest), 1, (IObjectResolver)null);
		}
	}

	public class AssembleEmpireQuest : StoryModeQuestBase
	{
		private int _imperialCultureTowns;

		private int _ownedByPlayerImperialTowns;

		private bool _assembledEmpire;

		private const float _ratioOfSettlementToTake = 0.66f;

		[SaveableField(1)]
		private JournalLog _numberOfCapturedSettlementsLog;

		public override TextObject Title => new TextObject("{=ya8eMCpj}Unify the Empire", (Dictionary<string, object>)null);

		private TextObject _questCanceledLogText => new TextObject("{=tVlZTOst}You have chosen a different path.", (Dictionary<string, object>)null);

		public AssembleEmpireQuest(Hero questGiver)
			: base("assemble_empire_quest", questGiver, CampaignTime.Never)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Expected O, but got Unknown
			//IL_0061: Expected O, but got Unknown
			_assembledEmpire = false;
			CacheSettlementCounts();
			((QuestBase)this).SetDialogs();
			((QuestBase)this).InitializeQuestOnCreation();
			_numberOfCapturedSettlementsLog = ((QuestBase)this).AddDiscreteLog(new TextObject("{=3deb2lMd}To restore the Empire you should capture two thirds of settlements with imperial culture.", (Dictionary<string, object>)null), new TextObject("{=Dp6newHS}Conquered Settlements", (Dictionary<string, object>)null), _ownedByPlayerImperialTowns, MathF.Ceiling((float)_imperialCultureTowns * 0.66f), (TextObject)null, false);
		}

		protected override void SetDialogs()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			((QuestBase)this).DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("{=mxKhvbn7}You have decided to unify the Empire.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver))
				.CloseDialog();
		}

		protected override void InitializeQuestOnGameLoad()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			//IL_004b: Expected O, but got Unknown
			CacheSettlementCounts();
			((QuestBase)this).SetDialogs();
			if (_numberOfCapturedSettlementsLog == null)
			{
				_numberOfCapturedSettlementsLog = ((QuestBase)this).AddDiscreteLog(new TextObject("{=3deb2lMd}To restore the Empire you should capture two thirds of settlements with imperial culture.", (Dictionary<string, object>)null), new TextObject("{=Dp6newHS}Conquered Settlements", (Dictionary<string, object>)null), _ownedByPlayerImperialTowns, MathF.Ceiling((float)_imperialCultureTowns * 0.66f), (TextObject)null, false);
			}
			_numberOfCapturedSettlementsLog.UpdateCurrentProgress((int)MathF.Clamp((float)_ownedByPlayerImperialTowns, 0f, (float)_imperialCultureTowns));
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
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

		private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
		{
			if (settlement.IsTown && ((MBObjectBase)settlement.Culture).StringId == "empire")
			{
				if (settlement.OwnerClan.Kingdom == Clan.PlayerClan.Kingdom && oldOwner.Clan.Kingdom != Clan.PlayerClan.Kingdom)
				{
					_ownedByPlayerImperialTowns++;
				}
				if (oldOwner.Clan.Kingdom == Clan.PlayerClan.Kingdom && newOwner.Clan.Kingdom != Clan.PlayerClan.Kingdom)
				{
					_ownedByPlayerImperialTowns--;
				}
				_numberOfCapturedSettlementsLog.UpdateCurrentProgress((int)MathF.Clamp((float)_ownedByPlayerImperialTowns, 0f, (float)_imperialCultureTowns));
			}
		}

		protected override void HourlyTick()
		{
			if (QuestConditionsHold())
			{
				SuccessQuest();
			}
		}

		private void OnConspiracyActivated()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			if (!_assembledEmpire)
			{
				((QuestBase)this).CompleteQuestWithFail(new TextObject("{=80NOk1Ee}You could not unify the Empire.", (Dictionary<string, object>)null));
			}
		}

		private void CacheSettlementCounts()
		{
			_imperialCultureTowns = 0;
			_ownedByPlayerImperialTowns = 0;
			foreach (Settlement item in (List<Settlement>)(object)Settlement.All)
			{
				if (item.IsTown && ((MBObjectBase)item.Culture).StringId == "empire")
				{
					_imperialCultureTowns++;
					if (item.OwnerClan.Kingdom == Clan.PlayerClan.Kingdom)
					{
						_ownedByPlayerImperialTowns++;
					}
				}
			}
		}

		private bool QuestConditionsHold()
		{
			return _ownedByPlayerImperialTowns >= MathF.Ceiling((float)_imperialCultureTowns * 0.66f);
		}

		private void SuccessQuest()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			((QuestBase)this).AddLog(new TextObject("{=sJeYHMGG}You have unified the Empire.", (Dictionary<string, object>)null), false);
			((QuestBase)this).CompleteQuestWithSuccess();
			_assembledEmpire = true;
			StoryMode.StoryModePhases.SecondPhase.Instance.ActivateConspiracy();
		}

		internal static void AutoGeneratedStaticCollectObjectsAssembleEmpireQuest(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(AssembleEmpireQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_numberOfCapturedSettlementsLog);
		}

		internal static object AutoGeneratedGetMemberValue_numberOfCapturedSettlementsLog(object o)
		{
			return ((AssembleEmpireQuest)o)._numberOfCapturedSettlementsLog;
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
		if (side == MainStoryLineSide.CreateImperialKingdom || side == MainStoryLineSide.SupportImperialKingdom)
		{
			((QuestBase)new AssembleEmpireQuest(StoryModeHeroes.ImperialMentor)).StartQuest();
		}
	}
}
