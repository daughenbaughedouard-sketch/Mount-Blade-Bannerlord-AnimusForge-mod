using System.Collections.Generic;
using StoryMode.Quests.FirstPhase;
using StoryMode.Quests.PlayerClanQuests;
using StoryMode.Quests.QuestTasks;
using StoryMode.Quests.SecondPhase;
using StoryMode.Quests.SecondPhase.ConspiracyQuests;
using StoryMode.Quests.TutorialPhase;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace StoryMode;

public class SaveableStoryModeTypeDefiner : SaveableTypeDefiner
{
	public SaveableStoryModeTypeDefiner()
		: base(320000)
	{
	}

	protected override void DefineClassTypes()
	{
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(CampaignStoryMode), 1, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(StoryModeManager), 2, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(MainStoryLine), 3, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(TrainingField), 4, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(TrainingFieldEncounter), 5, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(PurchaseItemTutorialQuestTask), 6, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(RecruitTroopTutorialQuestTask), 7, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(TutorialPhase), 8, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(FirstPhase), 9, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(SecondPhase), 10, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(ThirdPhase), 11, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(ConspiracyQuestBase), 12, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(ConspiracyQuestMapNotification), 13, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(ConspiracyBaseOfOperationsDiscoveredConspiracyQuest), 14, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(DestroyRaidersConspiracyQuest), 15, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(DisruptSupplyLinesConspiracyQuest), 17, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(TravelToVillageTutorialQuest), 694001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(TalkToTheHeadmanTutorialQuest), 693001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(PurchaseGrainTutorialQuest), 691001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(RecruitTroopsTutorialQuest), 692001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(LocateAndRescueTravellerTutorialQuest), 688001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(FindHideoutTutorialQuest), 686001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(BannerInvestigationQuest), 684001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(AssembleTheBannerQuest), 683001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(MeetWithIstianaQuest), 690001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(MeetWithArzagosQuest), 689001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(IstianasBannerPieceQuest), 687001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(ArzagosBannerPieceQuest), 681001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(SupportKingdomQuest), 680001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(CreateKingdomQuest), 580001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(ConspiracyProgressQuest), 695001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(RebuildPlayerClanQuest), 3780001, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(VillagersInNeed), 18, (IObjectResolver)null);
	}

	protected override void DefineStructTypes()
	{
	}

	protected override void DefineEnumTypes()
	{
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(MainStoryLineSide), 2001, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(TutorialQuestPhase), 2002, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(FindHideoutTutorialQuest.HideoutBattleEndState), 686010, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(IstianasBannerPieceQuest.HideoutBattleEndState), 687010, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(ArzagosBannerPieceQuest.HideoutBattleEndState), 681010, (IEnumResolver)null);
	}

	protected override void DefineInterfaceTypes()
	{
	}

	protected override void DefineRootClassTypes()
	{
	}

	protected override void DefineGenericClassDefinitions()
	{
	}

	protected override void DefineGenericStructDefinitions()
	{
	}

	protected override void DefineContainerDefinitions()
	{
		((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(List<TrainingField>));
		((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(Dictionary<string, TrainingField>));
		((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(Dictionary<MBGUID, TrainingField>));
		((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(Dictionary<int, CampaignTime>));
	}
}
