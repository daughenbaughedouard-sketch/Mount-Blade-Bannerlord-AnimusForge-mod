using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Missions.MissionLogics.Arena;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.CampaignBehaviors;

public class ArenaMasterCampaignBehavior : CampaignBehaviorBase
{
	private List<Settlement> _arenaMasterHasMetInSettlements = new List<Settlement>();

	private bool _knowTournaments;

	private bool _enteredPracticeFightFromMenu;

	public override void RegisterEvents()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
		CampaignEvents.AfterMissionStarted.AddNonSerializedListener((object)this, (Action<IMission>)AfterMissionStarted);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<List<Settlement>>("_arenaMasterHasMetInSettlements", ref _arenaMasterHasMetInSettlements);
		dataStore.SyncData<bool>("_knowTournaments", ref _knowTournaments);
	}

	public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
		AddGameMenus(campaignGameStarter);
	}

	private void AddGameMenus(CampaignGameStarter campaignGameStarter)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0031: Expected O, but got Unknown
		campaignGameStarter.AddGameMenuOption("town_arena", "mno_enter_practice_fight", "{=9pg3qc6N}Practice fight", new OnConditionDelegate(game_menu_enter_practice_fight_on_condition), new OnConsequenceDelegate(game_menu_enter_practice_fight_on_consequence), false, 1, false, (object)null);
	}

	public void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		if (mobileParty == MobileParty.MainParty && settlement.IsTown)
		{
			AddArenaMaster(settlement);
		}
	}

	private void OnGameLoadFinished()
	{
		if (Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner && LocationComplex.Current != null && Settlement.CurrentSettlement.IsTown && PlayerEncounter.LocationEncounter != null && !Settlement.CurrentSettlement.IsUnderSiege)
		{
			AddArenaMaster(Settlement.CurrentSettlement);
		}
	}

	private void AddArenaMaster(Settlement settlement)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		settlement.LocationComplex.GetLocationWithId("arena").AddLocationCharacters(new CreateLocationCharacterDelegate(CreateTournamentMaster), settlement.Culture, (CharacterRelations)0, 1);
	}

	protected void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Expected O, but got Unknown
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Expected O, but got Unknown
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Expected O, but got Unknown
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Expected O, but got Unknown
		//IL_04b8: Expected O, but got Unknown
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Expected O, but got Unknown
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Expected O, but got Unknown
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Expected O, but got Unknown
		//IL_061a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0628: Expected O, but got Unknown
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
		//IL_0675: Expected O, but got Unknown
		//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c2: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("arena_master_tournament_meet", "start", "arena_intro_1", "{=GAsVO8cZ}Good day, friend. I'll bet you came here for the games, or as they say nowadays, the tournament!", new OnConditionDelegate(conversation_arena_master_tournament_meet_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_tournament_meet_2", "start", "arena_intro_1a", "{=rqFKxm24}Greetings, friend. If you came for the games, the big fights, I'm afraid you're out of luck. There won't be games, or a 'tournament' as they say nowadays, any time soon.", new OnConditionDelegate(conversation_arena_master_no_tournament_meet_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_meet_no_intro", "start", "arena_master_talk", "{=ZvzxcRbc}Good day, friend. You look like you know your way around an arena. How can I help you?", new OnConditionDelegate(conversation_arena_master_player_knows_arenas_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_meet_start", "start", "arena_master_talk", "{=dgNCuuUL}Hello, {PLAYER.NAME}. Good to see you again.", new OnConditionDelegate(conversation_arena_master_meet_start_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_meet_start_2", "start", "arena_master_post_practice_fight_talk", "{=nmPaCLHp}{FIGHT_DEBRIEF} Do you want to give it another go?", new OnConditionDelegate(conversation_arena_master_post_fight_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_intro_1", "arena_intro_1", "arena_intro_tournament", "{=j9RrkCvM}There's a tournament going on?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_intro_2", "arena_intro_1a", "arena_intro_no_tournament", "{=W1wVPNpy}I've heard of these games...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_intro_3", "arena_intro_tournament", "arena_intro_tournament_2", "{=GAq7KAf0}You bet! Say, you look like a fighter. You should join. Back in the old days it was all condemned criminals and fights to the death, but nowadays they use blunted weapons.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_intro_3a", "arena_intro_tournament_2", "arena_intro_practice_fights", "{=VH27tpkT}It's quite the opportunity to make your name. You risk no more than your teeth, and didn't the Heavens give us thirty of those, just to have a few spare for grand opportunities like this?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_intro_4", "arena_intro_no_tournament", "arena_intro_no_tournament_2", "{=EA2JcVcb}As well you might! They're a grand old imperial custom that's now spread all over Calradia. Back in the old days, they'd give a hundred condemned criminals swords and have them slash at each other until the sands were drenched in blood![if:convo_merry]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_intro_4a", "arena_intro_no_tournament_2", "arena_intro_no_tournament_3", "{=EFKxbLaO}Nowadays things are a little different, of course. The emperors got worried about the people's morals and steered them toward more virtuous kinds of killing, like wars. But the games still go on, just with blunted weapons.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_intro_5", "arena_intro_no_tournament_3", "arena_intro_no_tournament_4", "{=LqkxF5Op}During the games, all the best fighters from the area form teams and pummel each other. Not quite as much fun for the crowd as watching gladiators spill their guts out, of course, but healthier for the participants.[if:convo_approving]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_intro_5a", "arena_intro_no_tournament_4", "arena_intro_practice_fights", "{=jy1o5cNT}You're a warrior, are you not? The games are a fine way to make your name. The local merchants put together a nice fat purse for the winner to attract the talent.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_intro_6", "arena_intro_practice_fights", "arena_intro_perk_reset", "{=iLuezAbk}When there's no tournament, it's still worth coming by. A lot of fighters spend their time here practicing to keep in trim, and we'll award the winners a few coins for their troubles.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_tournament_rules", "arena_intro_4", "arena_tournament_rules", "{=aHGbTpLp}Tell me how tournaments work.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_practice_fight_rules", "arena_intro_4", "arena_practice_fight_rules", "{=H2aaMAe5}Tell me how the practice fights work.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_prizes", "arena_intro_4", "arena_prizes", "{=7pH9MzS1}So you pay us to fight? What's in it for you?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_prizes_2", "arena_intro_4", "arena_master_pre_talk", "{=R2HP4EiX}I don't have any more questions.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_reminder", "arena_master_reminder", "arena_intro_4", "{=k7ebznzr}Yes?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_prizes_answer", "arena_prizes", "arena_prizes_amounts", "{=bUmacxw7}Well, even the practice fights draw those who like to bet on the outcome. But the tournaments! Those pull in crowds from miles around. The merchants love a tournament, and that's why they pony up the silver we need to pay the good souls like you who take and receive the hard knocks.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_training_practice_fight_intro", "arena_tournament_rules", "arena_intro_3a", "{=o0H8Qs0D}The rules of the tournament are standard across Calradia, even outside the Empire. We match the fighters up by drawing lots. Sometimes you're part of a team, and sometimes you fight by yourself.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_training_practice_fight_intro_1c", "arena_intro_3a", "arena_intro_4", "{=Jgkz4uo6}The lots also determine what weapons you get. The winners of each match proceed to the next round. When only two are left, they battle each other to be declared champion.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_training_practice_fight_intro_1a", "arena_practice_fight_rules", "arena_intro_4", "{=cPmV8S4e}We leave the arena open to anyone who wants to practice. There are no rules, no teams. Everyone beats at each other until there is only one fighter left standing. Sounds like fun, eh?[ib:confident2]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_training_practice_fight_intro_2", "arena_prizes_amounts", "arena_tournament_reward", "{=WwbDoZXg}How much are the prizes in the tournaments?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_training_practice_fight_intro_3", "arena_prizes_amounts", "arena_practice_fight_reward", "{=Z4MreMZz}How much are the prizes in the practice fights?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_training_practice_fight_intro_4", "arena_prizes_amounts", "arena_master_pre_talk", "{=4vAbAIqi}Okay. I think I get it.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_training_practice_fight_intro_reward", "arena_practice_fight_reward", "arena_joining_ask", "{=!}{ARENA_REWARD}", new OnConditionDelegate(conversation_arena_practice_fight_explain_reward_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_training_practice_fight_intro_reward_2", "arena_tournament_reward", "arena_joining_ask", "{=!}{TOURNAMENT_REWARD}", new OnConditionDelegate(conversation_arena_tournament_explain_reward_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_training_practice_fight_intro_5", "arena_joining_ask", "arena_joining_answer", "{=Te4pxfWF}So can I join?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_training_practice_fight_intro_6", "arena_joining_answer", "arena_master_talk", "{=bBVLVT7L}Certainly! Looks like a few of our lads are warming up now for the tournament. You can go and hop in if you want to. Or come back later if you just want to practice.[ib:warrior]", new OnConditionDelegate(conversation_town_has_tournament_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_training_practice_fight_intro_7", "arena_joining_answer", "arena_master_talk", "{=KtrZs3yA}Certainly! The arena is open to anyone who doesn't mind hard knocks. Looks like a few of our lads are warming up now. You can go and hop in if you want to. Or come back later when there's a tournament.[ib:warrior]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_pre_talk_explain", "arena_master_explain", "arena_prizes_amounts", "{=ke0IvBXb}Anything else I can explain?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_ask_what_to_do", "arena_master_pre_talk", "arena_master_talk", "{=arena_master_24}So, what would you like to do?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_master_sign_up_tournament", "arena_master_talk", "arena_master_enter_tournament", "{=arena_master_25}Sign me up for the tournament.", new OnConditionDelegate(conversation_town_has_tournament_on_condition), (OnConsequenceDelegate)null, 100, new OnClickableConditionDelegate(conversation_town_arena_fight_join_check_on_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_master_ask_for_practice_fight_fight", "arena_master_talk", "arena_master_enter_practice_fight", "{=arena_master_26}I'd like to participate in a practice fight...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, new OnClickableConditionDelegate(conversation_town_arena_fight_join_check_on_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_master_ask_tournaments", "arena_master_talk", "arena_master_ask_tournaments", "{=arena_master_27}Are there any tournaments going on in nearby towns?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_master_remind_something", "arena_master_talk", "arena_master_reminder", "{=iSNrQKEN}I want to go back to something you'd mentioned earlier...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_master_leave", "arena_master_talk", "close_window", "{=arena_master_30}I need to leave now. Good bye.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 80, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_tournament_location", "arena_master_ask_tournaments", "arena_master_talk", "{=arena_master_31}{NEARBY_TOURNAMENT_STRING}", new OnConditionDelegate(conversation_tournament_soon_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_ask_tournaments_2", "arena_master_ask_tournaments", "arena_master_talk", "{=arena_master_32}There won't be any tournaments any time soon.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 1, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_enter_practice_fight_master_confirm", "arena_master_enter_practice_fight", "arena_master_enter_practice_fight_confirm", "{=arena_master_33}Go to it! Grab a practice weapon on your way down.[if:convo_approving]", new OnConditionDelegate(conversation_arena_join_practice_fight_confirm_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_enter_practice_fight_master_decline", "arena_master_enter_practice_fight", "close_window", "{=FguHzavX}You can't practice in the arena because there is a tournament going on right now.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("arena_master_enter_tournament", "arena_master_enter_tournament", "arena_master_enter_tournament_confirm", "{=arena_master_34}Very well - we'll enter your name in the lots, and when your turn comes up, be ready to go out there and start swinging![if:convo_merry]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_master_enter_practice_fight_confirm", "arena_master_enter_practice_fight_confirm", "close_window", "{=arena_master_35}I'll do that.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_arena_join_fight_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_master_enter_practice_fight_decline", "arena_master_enter_practice_fight_confirm", "arena_master_pre_talk", "{=arena_master_36}On second thought, I'll hold off.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_master_enter_tournament_confirm", "arena_master_enter_tournament_confirm", "close_window", "{=arena_master_37}I'll be ready.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_arena_join_tournament_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_master_enter_tournament_decline", "arena_master_enter_tournament_confirm", "arena_master_pre_talk", "{=arena_master_38}Actually, never mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("arena_join_fight", "arena_master_post_practice_fight_talk", "close_window", "{=GmIluR4H}Sure. Why not?", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_arena_join_fight_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("2593", "arena_master_post_practice_fight_talk", "arena_master_practice_fight_reject", "{=qsg7pZOs}Thanks. But I will give my bruises some time to heal.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("2594", "arena_master_practice_fight_reject", "close_window", "{=Q7B68CVK}{?PLAYER.GENDER}Splendid{?}Good man{\\?}! That's clever of you.[ib:normal]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
	}

	private static LocationCharacter CreateTournamentMaster(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		CharacterObject tournamentMaster = culture.TournamentMaster;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)tournamentMaster).Race, "_settlement");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(tournamentMaster, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)tournamentMaster, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddFixedCharacterBehaviors), "spawnpoint_tournamentmaster", true, relation, (string)null, true, true, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private bool conversation_arena_master_practice_fights_meet_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 5 && !_knowTournaments)
		{
			MBTextManager.SetTextVariable("TOWN_NAME", Settlement.CurrentSettlement.Name, false);
			_knowTournaments = true;
			_arenaMasterHasMetInSettlements.Add(Settlement.CurrentSettlement);
			return true;
		}
		return false;
	}

	private bool conversation_town_has_tournament_on_condition()
	{
		if (Settlement.CurrentSettlement.IsTown)
		{
			return Campaign.Current.TournamentManager.GetTournamentGame(Settlement.CurrentSettlement.Town) != null;
		}
		return false;
	}

	public static bool conversation_tournament_soon_on_condition()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		List<Town> source = ((IEnumerable<Town>)Town.AllTowns).Where((Town x) => Campaign.Current.TournamentManager.GetTournamentGame(x) != null && x != Settlement.CurrentSettlement.Town).ToList();
		source = source.OrderBy((Town x) => DistanceHelper.FindClosestDistanceFromSettlementToSettlement(Settlement.CurrentSettlement, ((SettlementComponent)x).Settlement, (NavigationType)3)).ToList();
		TextObject val = null;
		if (source.Count > 1)
		{
			val = new TextObject("{=pinSMuMe}Well, there's one starting up at {CLOSEST_TOURNAMENT}, then another at {NEXT_CLOSEST_TOURNAMENT}. You should probably be able to get to either of those, if you move quickly.[ib:hip]", (Dictionary<string, object>)null);
			MBTextManager.SetTextVariable("CLOSEST_TOURNAMENT", ((SettlementComponent)source[0]).Settlement.EncyclopediaLinkWithName, false);
			MBTextManager.SetTextVariable("NEXT_CLOSEST_TOURNAMENT", ((SettlementComponent)source[1]).Settlement.EncyclopediaLinkWithName, false);
		}
		else if (source.Count == 1)
		{
			MBTextManager.SetTextVariable("CLOSEST_TOURNAMENT", ((SettlementComponent)source[0]).Settlement.EncyclopediaLinkWithName, false);
			val = new TextObject("{=2WnruiBw}I know of one starting up at {CLOSEST_TOURNAMENT}. You should be able to get there if you move quickly enough.", (Dictionary<string, object>)null);
		}
		else
		{
			val = new TextObject("{=tGI135jv}Ah - I don't know of any right now. That's a bit unusual though. Must be the wars.[ib:closed]", (Dictionary<string, object>)null);
		}
		MBTextManager.SetTextVariable("NEARBY_TOURNAMENT_STRING", val, false);
		return true;
	}

	private bool conversation_arena_join_practice_fight_confirm_on_condition()
	{
		return !Settlement.CurrentSettlement.Town.HasTournament;
	}

	private bool conversation_arena_join_practice_fight_decline_on_condition()
	{
		return Settlement.CurrentSettlement.Town.HasTournament;
	}

	private bool conversation_town_arena_fight_join_check_on_condition(out TextObject explanation)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		if (Hero.MainHero.IsWounded && Campaign.Current.IsMainHeroDisguised)
		{
			explanation = new TextObject("{=DqZtRBXR}You are wounded and in disguise.", (Dictionary<string, object>)null);
			return false;
		}
		if (Hero.MainHero.IsWounded)
		{
			explanation = new TextObject("{=yNMrF2QF}You are wounded", (Dictionary<string, object>)null);
			return false;
		}
		if (Campaign.Current.IsMainHeroDisguised)
		{
			explanation = new TextObject("{=jcEoUPCB}You are in disguise.", (Dictionary<string, object>)null);
			return false;
		}
		explanation = null;
		return true;
	}

	private bool conversation_arena_master_tournament_meet_on_condition()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		if (Settlement.CurrentSettlement == null)
		{
			return false;
		}
		TournamentGame tournamentGame = Campaign.Current.TournamentManager.GetTournamentGame(Settlement.CurrentSettlement.Town);
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 5 && !_knowTournaments && tournamentGame != null)
		{
			MBTextManager.SetTextVariable("TOWN_NAME", Settlement.CurrentSettlement.Name, false);
			_knowTournaments = true;
			_arenaMasterHasMetInSettlements.Add(Settlement.CurrentSettlement);
			return true;
		}
		return false;
	}

	private bool conversation_arena_master_no_tournament_meet_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 5 && !_knowTournaments)
		{
			MBTextManager.SetTextVariable("TOWN_NAME", Settlement.CurrentSettlement.Name, false);
			_knowTournaments = true;
			_arenaMasterHasMetInSettlements.Add(Settlement.CurrentSettlement);
			return true;
		}
		return false;
	}

	private static bool conversation_arena_practice_fight_explain_reward_on_condition()
	{
		MBTextManager.SetTextVariable("OPPONENT_COUNT_1", "3", false);
		MBTextManager.SetTextVariable("PRIZE_1", "5", false);
		MBTextManager.SetTextVariable("OPPONENT_COUNT_2", "6", false);
		MBTextManager.SetTextVariable("PRIZE_2", "10", false);
		MBTextManager.SetTextVariable("OPPONENT_COUNT_3", "10", false);
		MBTextManager.SetTextVariable("PRIZE_3", "25", false);
		MBTextManager.SetTextVariable("OPPONENT_COUNT_4", "20", false);
		MBTextManager.SetTextVariable("PRIZE_4", "60", false);
		MBTextManager.SetTextVariable("PRIZE_5", "250", false);
		MBTextManager.SetTextVariable("ARENA_REWARD", GameTexts.FindText("str_arena_reward", (string)null), false);
		return true;
	}

	private static bool conversation_arena_tournament_explain_reward_on_condition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		MBTextManager.SetTextVariable("TOURNAMENT_REWARD", new TextObject("{=1esi62Zb}Well - we like tournaments to be memorable. So the sponsors pitch together and buy a prize that they'll be talking about in the markets for weeks. A jeweled blade, say, or a fine-bred warhorse. Something a champion would be proud to own.", (Dictionary<string, object>)null), false);
		return true;
	}

	private bool conversation_arena_master_meet_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 5 && _knowTournaments && Settlement.CurrentSettlement.IsTown && !_arenaMasterHasMetInSettlements.Contains(Settlement.CurrentSettlement))
		{
			MBTextManager.SetTextVariable("TOWN_NAME", Settlement.CurrentSettlement.Name, false);
			_arenaMasterHasMetInSettlements.Add(Settlement.CurrentSettlement);
			return true;
		}
		return false;
	}

	private bool conversation_arena_master_meet_start_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 5 && _knowTournaments && Settlement.CurrentSettlement.IsTown && _arenaMasterHasMetInSettlements.Contains(Settlement.CurrentSettlement))
		{
			return !Mission.Current.GetMissionBehavior<ArenaPracticeFightMissionController>().AfterPractice;
		}
		return false;
	}

	private bool conversation_arena_master_player_knows_arenas_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 5 && _knowTournaments && Settlement.CurrentSettlement.IsTown && !_arenaMasterHasMetInSettlements.Contains(Settlement.CurrentSettlement))
		{
			return !Mission.Current.GetMissionBehavior<ArenaPracticeFightMissionController>().AfterPractice;
		}
		return false;
	}

	public static void conversation_arena_join_tournament_on_consequence()
	{
		Mission.Current.EndMission();
		Campaign.Current.GameMenuManager.SetNextMenu("menu_town_tournament_join");
	}

	public static void conversation_arena_join_fight_on_consequence()
	{
		Campaign.Current.ConversationManager.ConversationEndOneShot += StartPlayerPracticeAfterConversationEnd;
	}

	private static void StartPlayerPracticeAfterConversationEnd()
	{
		Mission.Current.SetMissionMode((MissionMode)2, false);
		Mission.Current.GetMissionBehavior<ArenaPracticeFightMissionController>().StartPlayerPractice();
	}

	private bool conversation_arena_master_post_fight_on_condition()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		Mission current = Mission.Current;
		ArenaPracticeFightMissionController arenaPracticeFightMissionController = ((current != null) ? current.GetMissionBehavior<ArenaPracticeFightMissionController>() : null);
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 5 && Settlement.CurrentSettlement.IsTown && arenaPracticeFightMissionController != null && arenaPracticeFightMissionController.AfterPractice)
		{
			arenaPracticeFightMissionController.AfterPractice = false;
			int opponentCountBeatenByPlayer = arenaPracticeFightMissionController.OpponentCountBeatenByPlayer;
			int remainingOpponentCountFromLastPractice = arenaPracticeFightMissionController.RemainingOpponentCountFromLastPractice;
			int num = 0;
			int num2;
			if (remainingOpponentCountFromLastPractice == 0)
			{
				num2 = 6;
				num = 250;
			}
			else if (opponentCountBeatenByPlayer == 0)
			{
				num2 = 0;
			}
			else if (opponentCountBeatenByPlayer < 3)
			{
				num2 = 1;
			}
			else if (opponentCountBeatenByPlayer < 6)
			{
				num2 = 2;
				num = 5;
			}
			else if (opponentCountBeatenByPlayer < 10)
			{
				num2 = 3;
				num = 10;
			}
			else if (opponentCountBeatenByPlayer < 20)
			{
				num2 = 4;
				num = 25;
			}
			else
			{
				num2 = 5;
				num = 60;
			}
			MBTextManager.SetTextVariable("PRIZE", num);
			MBTextManager.SetTextVariable("OPPONENT_COUNT", opponentCountBeatenByPlayer);
			TextObject val = GameTexts.FindText("str_arena_take_down", num2.ToString());
			MBTextManager.SetTextVariable("FIGHT_DEBRIEF", val, false);
			if (num > 0)
			{
				GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, num, true);
				MBTextManager.SetTextVariable("GOLD_AMOUNT", num);
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_quest_gold_reward_msg", (string)null)).ToString(), "event:/ui/notification/coins_positive"));
			}
			Mission.Current.SetMissionMode((MissionMode)1, false);
			return true;
		}
		return false;
	}

	private void AfterMissionStarted(IMission obj)
	{
		if (_enteredPracticeFightFromMenu)
		{
			Mission.Current.SetMissionMode((MissionMode)2, true);
			Mission.Current.GetMissionBehavior<ArenaPracticeFightMissionController>().StartPlayerPractice();
			_enteredPracticeFightFromMenu = false;
		}
	}

	private void game_menu_enter_practice_fight_on_consequence(MenuCallbackArgs args)
	{
		if (!_arenaMasterHasMetInSettlements.Contains(Settlement.CurrentSettlement))
		{
			_arenaMasterHasMetInSettlements.Add(Settlement.CurrentSettlement);
		}
		PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("arena"), (Location)null, (CharacterObject)null, (string)null);
		_enteredPracticeFightFromMenu = true;
	}

	private bool game_menu_enter_practice_fight_on_condition(MenuCallbackArgs args)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		Settlement currentSettlement = Settlement.CurrentSettlement;
		args.optionLeaveType = (LeaveType)33;
		if (!_knowTournaments)
		{
			args.Tooltip = new TextObject("{=Sph9Nliz}You need to learn more about the arena by talking with the arena master.", (Dictionary<string, object>)null);
			args.IsEnabled = false;
			return true;
		}
		if (Hero.MainHero.IsWounded && Campaign.Current.IsMainHeroDisguised)
		{
			args.Tooltip = new TextObject("{=DqZtRBXR}You are wounded and in disguise.", (Dictionary<string, object>)null);
			args.IsEnabled = false;
			return true;
		}
		if (Hero.MainHero.IsWounded)
		{
			args.Tooltip = new TextObject("{=yNMrF2QF}You are wounded", (Dictionary<string, object>)null);
			args.IsEnabled = false;
			return true;
		}
		if (Campaign.Current.IsMainHeroDisguised)
		{
			args.Tooltip = new TextObject("{=jcEoUPCB}You are in disguise.", (Dictionary<string, object>)null);
			args.IsEnabled = false;
			return true;
		}
		if (currentSettlement.Town.HasTournament)
		{
			args.Tooltip = new TextObject("{=NESB0CVc}There is no practice fight because of the Tournament.", (Dictionary<string, object>)null);
			args.IsEnabled = false;
			return true;
		}
		return true;
	}
}
