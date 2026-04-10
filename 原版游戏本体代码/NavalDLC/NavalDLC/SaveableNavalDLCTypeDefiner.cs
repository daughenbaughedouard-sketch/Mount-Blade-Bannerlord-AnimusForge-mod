using System.Collections.Generic;
using NavalDLC.CampaignBehaviors;
using NavalDLC.Map;
using NavalDLC.Storyline;
using NavalDLC.Storyline.CampaignBehaviors;
using NavalDLC.Storyline.MissionControllers;
using NavalDLC.Storyline.Quests;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace NavalDLC;

public class SaveableNavalDLCTypeDefiner : SaveableTypeDefiner
{
	public SaveableNavalDLCTypeDefiner()
		: base(520000)
	{
	}

	protected override void DefineClassTypes()
	{
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(DefeatTheCaptorsQuest), 2, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(SpeakToTheSailorsQuest), 9, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(SailToTheGulfOfCharasQuest), 10, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(HuntDownTheEmiraAlFahdaAndTheCorsairsQuest), 11, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(ReturnToBaseQuest), 12, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(SetSailAndEscortTheFortuneSeekersQuest), 13, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(SetSailAndMeetTheFortuneSeekersInTargetSettlementQuest), 14, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(GoToSkatriaIslandsQuest), 15, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(CaptureTheImperialMerchantPrusas), 16, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(InquireAtOstican), 17, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(DefeatThePiratesQuest), 18, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(FreeTheSeaHoundsCaptivesQuest), 19, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(NavalOrderOfBattleCampaignBehavior.NavalOrderOfBattleFormationData), 20, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(FishingPartyComponent), 21, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(StormManager), 22, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(Storm), 23, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(SpeakToGunnarAndSisterQuest), 24, (IObjectResolver)null);
		((SaveableTypeDefiner)this).AddClassDefinition(typeof(ScourgeoftheSeasQuest), 25, (IObjectResolver)null);
	}

	protected override void DefineContainerDefinitions()
	{
		((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(List<NavalOrderOfBattleCampaignBehavior.NavalOrderOfBattleFormationData>));
		((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(List<Storm>));
		((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(Storm.PreviousData[]));
		((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(Dictionary<Ship, List<ShipUpgradePiece>>));
	}

	protected override void DefineStructTypes()
	{
		((SaveableTypeDefiner)this).AddStructDefinition(typeof(Storm.PreviousData), 1, (IObjectResolver)null);
	}

	protected override void DefineEnumTypes()
	{
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(NavalStorylineData.NavalStorylineStage), 1000, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(Storm.StormTypes), 1001, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(FreeTheSeaHoundsCaptivesQuest.FreeTheSeaHoundsCaptivesQuestState), 1002, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(Quest5SetPieceBattleMissionController.BossFightOutComeEnum), 1003, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState), 1004, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(NavalStorylineThirdActFifthQuestBehaviour.NavalStorylineFinalQuestState), 1005, (IEnumResolver)null);
		((SaveableTypeDefiner)this).AddEnumDefinition(typeof(NavalStorylineData.NavalStorylineCheckpoint), 1006, (IEnumResolver)null);
	}
}
