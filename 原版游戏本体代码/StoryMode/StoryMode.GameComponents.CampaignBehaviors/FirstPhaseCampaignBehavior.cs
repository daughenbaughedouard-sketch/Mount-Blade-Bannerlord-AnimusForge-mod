using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using StoryMode.Quests.FirstPhase;
using StoryMode.Quests.PlayerClanQuests;
using StoryMode.Quests.TutorialPhase;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class FirstPhaseCampaignBehavior : CampaignBehaviorBase
{
	private Location _imperialMentorHouse;

	private Location _antiImperialMentorHouse;

	private bool _popUpShowed;

	public override void RegisterEvents()
	{
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnGameLoaded);
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnNewGameCreatedPartialFollowUpEnd);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener((object)this, (Action)OnBeforeMissionOpened);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
		StoryModeEvents.OnBannerPieceCollectedEvent.AddNonSerializedListener((object)this, (Action)OnBannerPieceCollected);
		StoryModeEvents.OnStoryModeTutorialEndedEvent.AddNonSerializedListener((object)this, (Action)OnStoryModeTutorialEnded);
		StoryModeEvents.OnMainStoryLineSideChosenEvent.AddNonSerializedListener((object)this, (Action<MainStoryLineSide>)OnMainStoryLineSideChosen);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<Location>("_imperialMentorHouse", ref _imperialMentorHouse);
		dataStore.SyncData<Location>("_antiImperialMentorHouse", ref _antiImperialMentorHouse);
		dataStore.SyncData<bool>("_popUpShowed", ref _popUpShowed);
	}

	private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
	{
		SpawnMentorsIfNeeded();
	}

	private void OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter campaignGameStarter)
	{
		Settlement val = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)((Settlement s) => s.IsTown && !s.IsUnderSiege && ((MBObjectBase)s.Culture).StringId == "empire"));
		_imperialMentorHouse = ReserveHouseForMentor(StoryModeHeroes.ImperialMentor, val);
		Settlement val2 = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)((Settlement s) => s.IsTown && !s.IsUnderSiege && ((MBObjectBase)s.Culture).StringId == "battania"));
		_antiImperialMentorHouse = ReserveHouseForMentor(StoryModeHeroes.AntiImperialMentor, val2);
		StoryModeManager.Current.MainStoryLine.SetMentorSettlements(val, val2);
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)detail == 1)
		{
			if (quest is BannerInvestigationQuest)
			{
				((QuestBase)new MeetWithIstianaQuest(StoryModeManager.Current.MainStoryLine.ImperialMentorSettlement)).StartQuest();
				((QuestBase)new MeetWithArzagosQuest(StoryModeManager.Current.MainStoryLine.AntiImperialMentorSettlement)).StartQuest();
			}
			else if (quest is MeetWithIstianaQuest)
			{
				Hero imperialMentor = StoryModeHeroes.ImperialMentor;
				((QuestBase)new IstianasBannerPieceQuest(imperialMentor, FindSuitableHideout(imperialMentor))).StartQuest();
			}
			else if (quest is MeetWithArzagosQuest)
			{
				Hero antiImperialMentor = StoryModeHeroes.AntiImperialMentor;
				((QuestBase)new ArzagosBannerPieceQuest(antiImperialMentor, FindSuitableHideout(antiImperialMentor))).StartQuest();
			}
		}
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		SpawnMentorsIfNeeded();
	}

	private void OnBeforeMissionOpened()
	{
		SpawnMentorsIfNeeded();
	}

	private void SpawnMentorsIfNeeded()
	{
		if (_imperialMentorHouse != null && _antiImperialMentorHouse != null && Settlement.CurrentSettlement != null && (StoryModeHeroes.ImperialMentor.CurrentSettlement == Settlement.CurrentSettlement || StoryModeHeroes.AntiImperialMentor.CurrentSettlement == Settlement.CurrentSettlement))
		{
			SpawnMentorInHouse(Settlement.CurrentSettlement);
		}
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		if (((MBObjectBase)settlement).StringId == "tutorial_training_field" && party == MobileParty.MainParty && TutorialPhase.Instance.TutorialQuestPhase == TutorialQuestPhase.Finalized && !_popUpShowed && TutorialPhase.Instance.IsSkipped)
		{
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=EWD4Op6d}Notification", (Dictionary<string, object>)null)).ToString(), ((object)GameTexts.FindText("main_storyline_skip_tutorial_notification_text", (string)null)).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)delegate
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Expected O, but got Unknown
				_popUpShowed = true;
				((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)Campaign.Current.GetCampaignBehavior<TutorialPhaseCampaignBehavior>());
				MBInformationManager.ShowSceneNotification((SceneNotificationData)new FindingFirstBannerPieceSceneNotificationItem(Hero.MainHero, (Action)OnPieceFoundAction));
			}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
	}

	private void ShowStealthTutorialInquiry()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001c: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		TextObject val = new TextObject("{=DhMge68x}Stealth Tutorial", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=bU88a6lW}You and your brother part ways. As he rides over the crest of a hill, he lifts his arm in salute, then disappears from view. A few days ago you were a family of six. Now, you are alone, and you realize that despite your courage and determination you and your brother may never see each other again.{newline}However, you are not left long in your solitude. As you make the final preparations to set out, a young boy staggers into your camp. Once he regains his breath, he tells you that a small group of bandits raided his village and seized the headman as a hostage. The villagers saw you riding through the countryside, and thought you might be able to help them.", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)GameTexts.FindText("str_continue", (string)null)).ToString(), string.Empty, (Action)StartStealthTutorial, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void StartStealthTutorial()
	{
		((QuestBase)new VillagersInNeed()).StartQuest();
		StoryModeEvents.Instance.OnStealthTutorialActivated();
	}

	private void OnPieceFoundAction()
	{
		SelectClanName();
	}

	private void OnStoryModeTutorialEnded()
	{
		((QuestBase)new RebuildPlayerClanQuest()).StartQuest();
		((QuestBase)new BannerInvestigationQuest()).StartQuest();
	}

	private void OnBannerPieceCollected()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		TextObject val = new TextObject("{=Pus87ZW2}You've found the {BANNER_PIECE_COUNT} banner piece!", (Dictionary<string, object>)null);
		if (FirstPhase.Instance == null || FirstPhase.Instance.CollectedBannerPieceCount == 1)
		{
			val.SetTextVariable("BANNER_PIECE_COUNT", new TextObject("{=oAoTaAWg}first", (Dictionary<string, object>)null));
		}
		else if (FirstPhase.Instance.CollectedBannerPieceCount == 2)
		{
			val.SetTextVariable("BANNER_PIECE_COUNT", new TextObject("{=9ZyXl25X}second", (Dictionary<string, object>)null));
		}
		else if (FirstPhase.Instance.CollectedBannerPieceCount == 3)
		{
			val.SetTextVariable("BANNER_PIECE_COUNT", new TextObject("{=4cw169Kb}third and the final", (Dictionary<string, object>)null));
		}
		MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
	}

	private void OnMainStoryLineSideChosen(MainStoryLineSide side)
	{
		_imperialMentorHouse.RemoveReservation();
		_imperialMentorHouse = null;
		_antiImperialMentorHouse.RemoveReservation();
		_antiImperialMentorHouse = null;
	}

	private void SelectClanName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		InformationManager.ShowTextInquiry(new TextInquiryData(((object)new TextObject("{=JJiKk4ow}Select your family name: ", (Dictionary<string, object>)null)).ToString(), string.Empty, true, false, ((object)GameTexts.FindText("str_done", (string)null)).ToString(), (string)null, (Action<string>)OnChangeClanNameDone, (Action)null, false, (Func<string, Tuple<bool, string>>)FactionHelper.IsClanNameApplicable, "", ((object)Clan.PlayerClan.Name).ToString()), false, false);
	}

	private void OnChangeClanNameDone(string newClanName)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		TextObject val = GameTexts.FindText("str_generic_clan_name", (string)null);
		val.SetTextVariable("CLAN_NAME", new TextObject(newClanName, (Dictionary<string, object>)null));
		Clan.PlayerClan.ChangeClanName(val, val);
		OpenBannerSelectionScreen(ShowStealthTutorialInquiry);
	}

	private void OpenBannerSelectionScreen(Action endAction)
	{
		Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<BannerEditorState>(new object[1] { endAction }), 0);
	}

	private Settlement FindSuitableHideout(Hero questGiver)
	{
		Settlement result = null;
		float num = float.MaxValue;
		foreach (Hideout item in (List<Hideout>)(object)Hideout.All)
		{
			if (!((SettlementComponent)item).Settlement.IsSettlementBusy((object)this))
			{
				float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(((SettlementComponent)item).Settlement, questGiver.CurrentSettlement, false, false, (NavigationType)1);
				if (distance < num)
				{
					num = distance;
					result = ((SettlementComponent)item).Settlement;
				}
			}
		}
		return result;
	}

	private void SpawnMentorInHouse(Settlement settlement)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		Hero obj = ((StoryModeHeroes.ImperialMentor.CurrentSettlement == settlement) ? StoryModeHeroes.ImperialMentor : StoryModeHeroes.AntiImperialMentor);
		Location val = ((StoryModeHeroes.ImperialMentor.CurrentSettlement == settlement) ? _imperialMentorHouse : _antiImperialMentorHouse);
		CharacterObject characterObject = obj.CharacterObject;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)characterObject).Race, "_settlement");
		AgentData obj2 = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)characterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		LocationCharacter val2 = new LocationCharacter(obj2, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common", true, (CharacterRelations)0, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
		val.AddCharacter(val2);
	}

	private Location ReserveHouseForMentor(Hero mentor, Settlement settlement)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		if (settlement == null)
		{
			Debug.Print("There is null settlement in ReserveHouseForMentor", 0, (DebugColor)12, 17592186044416uL);
		}
		MBList<Location> val = new MBList<Location>();
		((List<Location>)(object)val).Add(settlement.LocationComplex.GetLocationWithId("house_1"));
		((List<Location>)(object)val).Add(settlement.LocationComplex.GetLocationWithId("house_2"));
		((List<Location>)(object)val).Add(settlement.LocationComplex.GetLocationWithId("house_3"));
		Location obj = ((IEnumerable<Location>)val).First((Location h) => !h.IsReserved) ?? Extensions.GetRandomElement<Location>(val);
		TextObject val2 = new TextObject("{=EZ19JOGj}{MENTOR.NAME}'s House", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("MENTOR", mentor.CharacterObject, val2, false);
		obj.ReserveLocation(val2, val2);
		return obj;
	}
}
