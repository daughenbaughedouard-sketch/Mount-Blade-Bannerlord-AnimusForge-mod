using System;
using System.Collections.Generic;
using NavalDLC.Storyline.CampaignBehaviors;
using NavalDLC.Storyline.MissionControllers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class ScourgeoftheSeasQuest : QuestBase
{
	[SaveableField(0)]
	private JournalLog _gunnarJournal;

	public override TextObject Title => new TextObject("{=1EJ1kav2}Scourge of the Seas", (Dictionary<string, object>)null);

	public override bool IsRemainingTimeHidden => true;

	public override string SpecialQuestType => "NavalStoryline";

	public ScourgeoftheSeasQuest()
		: base("scourge_of_the_seas", NavalStorylineData.Gunnar, CampaignTime.Never, 0)
	{
	}//IL_000b: Unknown result type (might be due to invalid IL or missing references)


	protected override void OnStartQuest()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (NavalStorylineData.IsTutorialSkipped())
		{
			UpdateProgress(new TextObject("{=PMfKcz6o}You met a Nord warrior named Gunnar, helping him defeat an ambush in a back alley of Ostican. He suggested that you join forces with him to battle the Sea Hounds, a pirate confederacy, and in doing so that you might learn something about your sister. You declined, and he told you that you might find him in Ostican if you ever changed your mind.", (Dictionary<string, object>)null));
		}
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		NavalDLCEvents.OnNavalStorylineCanceledEvent.AddNonSerializedListener((object)this, (Action<NavalStorylineData.StorylineCancelDetail>)OnStorylineCanceled);
		NavalDLCEvents.OnSisterRansomRequestedEvent.AddNonSerializedListener((object)this, (Action)OnSisterRansomRequested);
		NavalDLCEvents.OnSisterRansomedEvent.AddNonSerializedListener((object)this, (Action)OnSisterRansomed);
	}

	private void OnStorylineCanceled(NavalStorylineData.StorylineCancelDetail detail)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (detail == NavalStorylineData.StorylineCancelDetail.ByRansom)
		{
			((QuestBase)this).AddLog(new TextObject("{=lR4a9LMR}You paid the Sea Hounds a ransom for your sister, and were reunited with her. You took your leave of Gunnar. He will continue his war against them, but you are no longer part of it.", (Dictionary<string, object>)null), false);
			((QuestBase)this).CompleteQuestWithSuccess();
		}
	}

	private void OnSisterRansomed()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		((QuestBase)this).AddLog(new TextObject("{=kOZcZcaR}Gunnar has informed you that he has ransomed your sister, and is waiting for you at Ostican.", (Dictionary<string, object>)null), false);
	}

	private void OnSisterRansomRequested()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		RemoveGunnarLog();
		((QuestBase)this).AddLog(new TextObject("{=UwEzZB5K}You have asked Gunnar to contact the Sea Hounds with an offer to ransom your sister. He will send you a message when he has done so.", (Dictionary<string, object>)null), false);
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails details)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		if ((int)details != 1)
		{
			return;
		}
		if (quest is ReturnToBaseQuest)
		{
			switch (NavalStorylineData.GetStorylineStage())
			{
			case NavalStorylineData.NavalStorylineStage.Act1:
				UpdateProgress(new TextObject("{=7HVntRW9}You met a Nord warrior named Gunnar, helping him defeat an ambush in a back alley of Ostican. You decided to join forces with him to battle the Sea Hounds, a pirate confederacy, and hunt for your sister. But no sooner did you set out with him then the two of you were betrayed by his old comrade Purig, who had joined the Sea Hounds. His men were lax, however, and you were able to break out of captivity and make off with one of his ships.", (Dictionary<string, object>)null));
				break;
			case NavalStorylineData.NavalStorylineStage.Act2:
				UpdateProgress(new TextObject("{=vcOPDZ83}Together with Gunnar's kin, you sailed forth in his longship and defeated two Sea Hound vessels lying in wait off of Ostican. You took a prisoner who revealed to you that the Sea Hounds were involved in trading slaves, giving you your first clue in the hunt for your sister.", (Dictionary<string, object>)null));
				break;
			case NavalStorylineData.NavalStorylineStage.Act3Quest1:
				UpdateQuest1Progress();
				break;
			case NavalStorylineData.NavalStorylineStage.Act3Quest2:
				UpdateProgress(new TextObject("{=UrAFO5ve}Gunnar introduced you to an Aserai sea-captain named Lahar. He was pursuing one of the Sea Hounds' allies, the Emira al-Fahda, and together you set out in search of your fleet. You brought Fahda to battle in the stormy seas off Charas and took her prisoner. She told of Purig's plans to become leader of the Sea Hounds and in exchange for her life revealed to you his next target, a Sturgian silver-ship.", (Dictionary<string, object>)null));
				break;
			case NavalStorylineData.NavalStorylineStage.Act3SpeakToSailors:
				UpdateProgress(new TextObject("{=5O51VPFJ}Acting on what you learned from Fahda, you and Gunnar sailed to the Sturgian port of Omor. There, you met another of Gunnar's old comrades, Bjolgur, a member of the Skolderbroda mercenary brotherhood. Bjolgur hoped to run the silver-ship past the Sea Hounds and enlisted you in his dangerous but effective plan to crash a fireship into their blockading fleet. In exchange, Bjolgur gave you information about a key Sea Hound ally, the imperial merchant Crusas, who he suspected would know more about your sister.", (Dictionary<string, object>)null));
				break;
			case NavalStorylineData.NavalStorylineStage.Act3Quest4:
				UpdateProgress(new TextObject("{=Mpf7S1ED}You and Gunnar sailed to the barren Skatria islands, where you found Crusas' fleet. He had lashed his ships together into a floating fortress, but you led your fleet in to storm it and took him prisoner. Crusas revealed to you that Purig was holding your sister as a hostage, and could likely be found at his base in a northern fjord. You made plans to attack Purig, using Crusas to allay his lookouts' suspicions as you sailed in close to rescue your sister.", (Dictionary<string, object>)null));
				break;
			case NavalStorylineData.NavalStorylineStage.Act3Quest5:
				UpdateFinalQuestProgress();
				break;
			default:
				Debug.FailedAssert("None state is wrong.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\Quests\\ScourgeoftheSeasQuest.cs", "OnQuestCompleted", 92);
				break;
			}
		}
		else if (quest is SpeakToGunnarAndSisterQuest)
		{
			UpdateOutro();
		}
		else if (quest is FreeTheSeaHoundsCaptivesQuest freeTheSeaHoundsCaptivesQuest)
		{
			UpdateAlternativeFinalQuestProgress(freeTheSeaHoundsCaptivesQuest.BossFightOutCome);
			UpdateOutro();
		}
	}

	private void UpdateQuest1Progress()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		if (NavalStorylineData.IsTutorialSkipped())
		{
			UpdateProgress(new TextObject("{=pwVjDfo1}You spoke again to Gunnar in Ostican. He told you that he had been betrayed by an old comrade, Purig, but escaped and learned more about the Sea Hounds' activities. You sailed with Gunnar and his kin to escort a Vlandian merchantman on its homeward voyage from Beinland, hoping that its rich cargo would entice the Sea Hounds into battle. You were not disappointed. Together with the merchants you defeated the attackers, dealing the Sea Hounds a heavy blow.", (Dictionary<string, object>)null));
		}
		else
		{
			UpdateProgress(new TextObject("{=HnNsNEtE}You and Gunnar offered to escort a Vlandian merchantman on its homeward voyage from Beinland, hoping that its rich cargo would entice the Sea Hounds into battle. You were not disappointed. Together with the merchants you defeated the attackers, dealing the Sea Hounds a heavy blow.", (Dictionary<string, object>)null));
		}
	}

	private void UpdateOutro()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		RemoveGunnarLog();
		((QuestBase)this).AddLog(new TextObject("{=H5FJWA7W}You bid farewell to Gunnar. He returned to his home in Lagshofn, in Beinland, where hopes that some day you might visit him. Your sister, recovered from her ordeal, stands ready to join you and the rest of your clan as you continue to forge your destiny in Calradia.", (Dictionary<string, object>)null), false);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private void UpdateAlternativeFinalQuestProgress(Quest5SetPieceBattleMissionController.BossFightOutComeEnum outcome)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		Campaign.Current.GetCampaignBehavior<NavalStorylineThirdActFifthQuestBehaviour>();
		TextObject val = TextObject.GetEmpty();
		switch (outcome)
		{
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedAndWonTheDuel:
			val = new TextObject("{=an8yoGf1}You arrived at Angranfjord and put your plan into action. While Crusas bantered with Purig's men, you and Gunnar swum to his prisoner ship and overcame the guards, sailing your sister to safety. Together with Lahar and Bjolgur you then engaged the Sea Hound fleet in a battle in the fjord. In the end, you fought Purig himself on his flagship and defeated him in a duel. You ended the Sea Hounds' reign of terror, and set your sister free.", (Dictionary<string, object>)null);
			break;
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndLetPurigGo:
			val = new TextObject("{=Y0H43ait}You arrived at Angranfjord and put your plan into action. While Crusas bantered with Purig's men, you and Gunnar swum to his prisoner ship and overcame the guards, sailing your sister to safety. Together with Lahar and Bjolgur you then engaged the Sea Hound fleet in a battle in the fjord. In the end, you fought Purig himself on his flagship but spared his life after he bested you in one-to-one combat. You ended the Sea Hounds' reign of terror, and set your sister free.", (Dictionary<string, object>)null);
			break;
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerRefusedTheDuel:
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndHadPurigKilledAnyway:
			val = new TextObject("{=Y8lIANDP}You arrived at Angranfjord and put your plan into action. While Crusas bantered with Purig's men, you and Gunnar swum to his prisoner ship and overcame the guards, sailing your sister to safety. Together with Lahar and Bjolgur you then engaged the Sea Hound fleet in a battle in the fjord. In the end, you fought Purig himself on his flagship and had your men cut him down. You ended the Sea Hounds' reign of terror, and set your sister free.", (Dictionary<string, object>)null);
			break;
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerDefeatedWaitingForConversation:
			Debug.FailedAssert("Invalid case", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\Quests\\ScourgeoftheSeasQuest.cs", "UpdateAlternativeFinalQuestProgress", 146);
			break;
		}
		RemoveGunnarLog();
		((QuestBase)this).AddLog(val, false);
	}

	private void UpdateFinalQuestProgress()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		NavalStorylineThirdActFifthQuestBehaviour campaignBehavior = Campaign.Current.GetCampaignBehavior<NavalStorylineThirdActFifthQuestBehaviour>();
		TextObject val = TextObject.GetEmpty();
		switch (campaignBehavior.GetBossFightOutcome())
		{
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedAndWonTheDuel:
			val = new TextObject("{=8aiE63ie}You arrived at Angranfjord and put your plan into action. While Crusas bantered with Purig's men, you and Gunnar swum to his prisoner ship and overcame the guards, sailing your sister to safety. Together with Lahar and Bjolgur you then engaged the Sea Hound fleet in a battle in the fjord. In the end, you fought Purig himself on his flagship and defeated him in a duel. You ended the Sea Hounds' reign of terror, and set your sister free. Gunnar awaits you in Ostican to say his farewells.", (Dictionary<string, object>)null);
			break;
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndLetPurigGo:
			val = new TextObject("{=sUvAbm1a}You arrived at Angranfjord and put your plan into action. While Crusas bantered with Purig's men, you and Gunnar swum to his prisoner ship and overcame the guards, sailing your sister to safety. Together with Lahar and Bjolgur you then engaged the Sea Hound fleet in a battle in the fjord. In the end, you fought Purig himself on his flagship but spared his life after he bested you in one-to-one combat. You ended the Sea Hounds' reign of terror, and set your sister free. Gunnar awaits you in Ostican to say his farewells.", (Dictionary<string, object>)null);
			break;
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerRefusedTheDuel:
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndHadPurigKilledAnyway:
			val = new TextObject("{=LxfXj7qE}You arrived at Angranfjord and put your plan into action. While Crusas bantered with Purig's men, you and Gunnar swum to his prisoner ship and overcame the guards, sailing your sister to safety. Together with Lahar and Bjolgur you then engaged the Sea Hound fleet in a battle in the fjord. In the end, you fought Purig himself on his flagship and had your men cut him down. You ended the Sea Hounds' reign of terror, and set your sister free. Gunnar awaits you in Ostican to say his farewells.", (Dictionary<string, object>)null);
			break;
		case Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerDefeatedWaitingForConversation:
			Debug.FailedAssert("Invalid case", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\Quests\\ScourgeoftheSeasQuest.cs", "UpdateFinalQuestProgress", 175);
			break;
		}
		RemoveGunnarLog();
		((QuestBase)this).AddLog(val, false);
	}

	protected override void InitializeQuestOnGameLoad()
	{
	}

	protected override void SetDialogs()
	{
	}

	private void UpdateProgress(TextObject log)
	{
		((QuestBase)this).AddLog(log, false);
		UpdateGunnarLog();
	}

	private void RemoveGunnarLog()
	{
		if (_gunnarJournal != null)
		{
			((QuestBase)this).RemoveLog(_gunnarJournal);
		}
		_gunnarJournal = null;
	}

	private void UpdateGunnarLog()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		RemoveGunnarLog();
		_gunnarJournal = ((QuestBase)this).AddLog(new TextObject("{=vT1aPyAo}Gunnar awaits you in Ostican when you are ready to embark again.", (Dictionary<string, object>)null), false);
	}
}
