using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using SandBox.BoardGames;
using SandBox.BoardGames.MissionLogics;
using SandBox.Conversation;
using SandBox.Conversation.MissionLogics;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.CampaignBehaviors;

public class BoardGameCampaignBehavior : CampaignBehaviorBase
{
	private const int NumberOfBoardGamesCanPlayerPlayAgainstHeroPerDay = 3;

	private Dictionary<Hero, List<CampaignTime>> _heroAndBoardGameTimeDictionary = new Dictionary<Hero, List<CampaignTime>>();

	private Dictionary<Settlement, CampaignTime> _wonBoardGamesInOneWeekInSettlement = new Dictionary<Settlement, CampaignTime>();

	private AIDifficulty _difficulty;

	private int _betAmount;

	private bool _influenceGained;

	private bool _renownGained;

	private bool _opposingHeroExtraXPGained;

	private bool _relationGained;

	private bool _gainedNothing;

	private CultureObject _initializedBoardGameCultureInMission;

	public IEnumerable<Settlement> WonBoardGamesInOneWeekInSettlement
	{
		get
		{
			foreach (Settlement key in _wonBoardGamesInOneWeekInSettlement.Keys)
			{
				yield return key;
			}
		}
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionStarted);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnd);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
		CampaignEvents.OnPlayerBoardGameOverEvent.AddNonSerializedListener((object)this, (Action<Hero, BoardGameState>)OnPlayerBoardGameOver);
		CampaignEvents.WeeklyTickEvent.AddNonSerializedListener((object)this, (Action)WeeklyTick);
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
	}

	private void OnMissionEnd(IMission obj)
	{
		_initializedBoardGameCultureInMission = null;
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<Dictionary<Hero, List<CampaignTime>>>("_heroAndBoardGameTimeDictionary", ref _heroAndBoardGameTimeDictionary);
		dataStore.SyncData<Dictionary<Settlement, CampaignTime>>("_wonBoardGamesInOneWeekInSettlement", ref _wonBoardGamesInOneWeekInSettlement);
	}

	public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
	}

	private void WeeklyTick()
	{
		DeleteOldBoardGamesOfChampion();
		foreach (Hero item in _heroAndBoardGameTimeDictionary.Keys.ToList())
		{
			DeleteOldBoardGamesOfHero(item);
		}
	}

	private void OnPlayerBoardGameOver(Hero opposingHero, BoardGameState state)
	{
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Invalid comparison between Unknown and I4
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Invalid comparison between Unknown and I4
		if (opposingHero != null)
		{
			GameEndWithHero(opposingHero);
			if ((int)state == 1)
			{
				_opposingHeroExtraXPGained = (int)_difficulty != 2 && MBRandom.RandomFloat <= 0.5f;
				SkillLevelingManager.OnBoardGameWonAgainstLord(opposingHero, _difficulty, _opposingHeroExtraXPGained);
				float num = 0.1f;
				num += ((opposingHero.IsFemale != Hero.MainHero.IsFemale) ? 0.1f : 0f);
				num += (float)Hero.MainHero.GetSkillValue(DefaultSkills.Charm) / 100f;
				num += ((opposingHero.GetTraitLevel(DefaultTraits.Calculating) == 1) ? 0.2f : 0f);
				bool num2 = MBRandom.RandomFloat <= num;
				bool flag = opposingHero.MapFaction == Hero.MainHero.MapFaction && (int)_difficulty == 2 && MBRandom.RandomFloat <= 0.4f;
				bool flag2 = (int)_difficulty == 2;
				if (num2)
				{
					ChangeRelationAction.ApplyPlayerRelation(opposingHero, 1, true, true);
					_relationGained = true;
				}
				else if (flag)
				{
					GainKingdomInfluenceAction.ApplyForBoardGameWon(opposingHero, 1f);
					_influenceGained = true;
				}
				else if (flag2)
				{
					GainRenownAction.Apply(Hero.MainHero, 1f, false);
					_renownGained = true;
				}
				else
				{
					_gainedNothing = true;
				}
			}
		}
		else if ((int)state == 1)
		{
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, _betAmount, false);
			if (_betAmount > 0)
			{
				PlayerWonAgainstTavernChampion();
			}
		}
		else if ((int)state == 2)
		{
			GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, _betAmount, false);
		}
		SetBetAmount(0);
	}

	public unsafe void InitializeConversationVars()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		ICampaignMission current = CampaignMission.Current;
		object obj;
		if (current == null)
		{
			obj = null;
		}
		else
		{
			Location location = current.Location;
			obj = ((location != null) ? location.StringId : null);
		}
		if (!((string?)obj == "lordshall"))
		{
			ICampaignMission current2 = CampaignMission.Current;
			object obj2;
			if (current2 == null)
			{
				obj2 = null;
			}
			else
			{
				Location location2 = current2.Location;
				obj2 = ((location2 != null) ? location2.StringId : null);
			}
			if (!((string?)obj2 == "tavern"))
			{
				return;
			}
		}
		CultureObject boardGameCulture = GetBoardGameCulture();
		BoardGameType boardGame = boardGameCulture.BoardGame;
		if ((int)boardGame == -1)
		{
			MBDebug.ShowWarning("Boardgame not yet implemented, or not found.");
		}
		if ((int)boardGame != -1)
		{
			MBTextManager.SetTextVariable("GAME_NAME", GameTexts.FindText("str_boardgame_name", ((object)(*(BoardGameType*)(&boardGame))/*cast due to .constrained prefix*/).ToString()), false);
			MBTextManager.SetTextVariable("CULTURE_NAME", ((BasicCultureObject)boardGameCulture).Name, false);
			MBTextManager.SetTextVariable("DIFFICULTY", GameTexts.FindText("str_boardgame_difficulty", ((object)Unsafe.As<AIDifficulty, AIDifficulty>(ref _difficulty)/*cast due to .constrained prefix*/).ToString()), false);
			MBTextManager.SetTextVariable("BET_AMOUNT", _betAmount.ToString(), false);
			MBTextManager.SetTextVariable("IS_BETTING", (_betAmount > 0) ? 1 : 0);
			Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().SetBoardGame(boardGame);
		}
	}

	public void OnMissionStarted(IMission mission)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		Mission val = (Mission)mission;
		if ((NativeObject)(object)Mission.Current.Scene != (NativeObject)null)
		{
			_ = Mission.Current.Scene.FindEntityWithTag("boardgame") != (GameEntity)null;
		}
		if ((NativeObject)(object)Mission.Current.Scene != (NativeObject)null && Mission.Current.Scene.FindEntityWithTag("boardgame_holder") != (GameEntity)null && CampaignMission.Current.Location != null && (CampaignMission.Current.Location.StringId == "lordshall" || CampaignMission.Current.Location.StringId == "tavern"))
		{
			val.AddMissionBehavior((MissionBehavior)(object)new MissionBoardGameLogic());
			InitializeBoardGamePrefabInMission();
		}
	}

	private CultureObject GetBoardGameCulture()
	{
		if (_initializedBoardGameCultureInMission != null)
		{
			return _initializedBoardGameCultureInMission;
		}
		if (CampaignMission.Current.Location.StringId == "lordshall")
		{
			return Settlement.CurrentSettlement.OwnerClan.Culture;
		}
		return Settlement.CurrentSettlement.Culture;
	}

	private unsafe void InitializeBoardGamePrefabInMission()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Campaign.Current.GameMode == 1)
		{
			CultureObject boardGameCulture = GetBoardGameCulture();
			BoardGameType boardGame = boardGameCulture.BoardGame;
			GameEntity val = Mission.Current.Scene.FindEntityWithTag("boardgame_holder");
			MatrixFrame globalFrame = val.GetGlobalFrame();
			Mission.Current.Scene.RemoveEntity(val, 92);
			GameEntity val2 = GameEntity.Instantiate(Mission.Current.Scene, "BoardGame" + ((object)(*(BoardGameType*)(&boardGame))/*cast due to .constrained prefix*/).ToString() + "_FullSetup", true, true, "");
			MatrixFrame frame = val2.GetFrame();
			MatrixFrame val3 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref frame);
			val2.SetGlobalFrame(ref val3, true);
			GameEntity firstChildEntityWithTag = val2.GetFirstChildEntityWithTag("dice_board");
			if (firstChildEntityWithTag != (GameEntity)null && firstChildEntityWithTag.HasScriptOfType<VertexAnimator>())
			{
				firstChildEntityWithTag.GetFirstScriptOfType<VertexAnimator>().StopAndGoToEnd();
			}
			_initializedBoardGameCultureInMission = boardGameCulture;
		}
	}

	public void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
	{
		if (_heroAndBoardGameTimeDictionary.ContainsKey(victim))
		{
			_heroAndBoardGameTimeDictionary.Remove(victim);
		}
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		if (settlement.IsTown && CampaignMission.Current != null)
		{
			Location location = CampaignMission.Current.Location;
			if (location != null && location.StringId == "tavern" && unusedUsablePointCount.TryGetValue("spawnpoint_tavernkeeper", out var value) && value > 0)
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateGameHost), settlement.Culture, (CharacterRelations)0, 1);
			}
		}
	}

	private static LocationCharacter CreateGameHost(CultureObject culture, CharacterRelations relation)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		CharacterObject tavernGamehost = culture.TavernGamehost;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)tavernGamehost).Race, "_settlement");
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(tavernGamehost, ref num, ref num2, "");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)tavernGamehost, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddCompanionBehaviors), "gambler_npc", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_villager"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	protected void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_017c: Expected O, but got Unknown
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		//IL_01b3: Expected O, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Expected O, but got Unknown
		//IL_01ea: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Expected O, but got Unknown
		//IL_0221: Expected O, but got Unknown
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Expected O, but got Unknown
		//IL_0258: Expected O, but got Unknown
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Expected O, but got Unknown
		//IL_028f: Expected O, but got Unknown
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Expected O, but got Unknown
		//IL_02c6: Expected O, but got Unknown
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Expected O, but got Unknown
		//IL_02fd: Expected O, but got Unknown
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Expected O, but got Unknown
		//IL_0334: Expected O, but got Unknown
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Expected O, but got Unknown
		//IL_036b: Expected O, but got Unknown
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Expected O, but got Unknown
		//IL_03a2: Expected O, but got Unknown
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Expected O, but got Unknown
		//IL_03d9: Expected O, but got Unknown
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Expected O, but got Unknown
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Expected O, but got Unknown
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Expected O, but got Unknown
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Expected O, but got Unknown
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Expected O, but got Unknown
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Expected O, but got Unknown
		//IL_052c: Expected O, but got Unknown
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Expected O, but got Unknown
		//IL_0563: Expected O, but got Unknown
		//IL_0580: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Expected O, but got Unknown
		//IL_059a: Expected O, but got Unknown
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Expected O, but got Unknown
		//IL_05d1: Expected O, but got Unknown
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0608: Expected O, but got Unknown
		//IL_0608: Expected O, but got Unknown
		//IL_0626: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Expected O, but got Unknown
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_065f: Expected O, but got Unknown
		//IL_067d: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Expected O, but got Unknown
		//IL_06c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d7: Expected O, but got Unknown
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0702: Expected O, but got Unknown
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		//IL_072d: Expected O, but got Unknown
		//IL_074a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0758: Expected O, but got Unknown
		//IL_0775: Unknown result type (might be due to invalid IL or missing references)
		//IL_0783: Expected O, but got Unknown
		//IL_07a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ae: Expected O, but got Unknown
		//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Expected O, but got Unknown
		//IL_0838: Unknown result type (might be due to invalid IL or missing references)
		//IL_0846: Expected O, but got Unknown
		//IL_0863: Unknown result type (might be due to invalid IL or missing references)
		//IL_0871: Expected O, but got Unknown
		//IL_088f: Unknown result type (might be due to invalid IL or missing references)
		//IL_089c: Expected O, but got Unknown
		//IL_08b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c7: Expected O, but got Unknown
		//IL_08e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fd: Expected O, but got Unknown
		//IL_08fd: Expected O, but got Unknown
		//IL_091b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0928: Expected O, but got Unknown
		//IL_0945: Unknown result type (might be due to invalid IL or missing references)
		//IL_0953: Expected O, but got Unknown
		//IL_0970: Unknown result type (might be due to invalid IL or missing references)
		//IL_097e: Expected O, but got Unknown
		//IL_099b: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b4: Expected O, but got Unknown
		//IL_09b4: Expected O, but got Unknown
		//IL_09d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e0: Expected O, but got Unknown
		//IL_0a1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2c: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("talk_common_to_taverngamehost", "start", "close_window", "{GAME_MASTER_INTRO}", (OnConditionDelegate)(() => conversation_talk_common_to_taverngamehost_on_condition() && !taverngamehost_player_sitting_now_on_condition()), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("talk_common_to_taverngamehost_2", "start", "taverngamehost_talk", "{=LGrzKlET}Let me know how much of a challenge you can stand and we'll get started. I'm ready to offer you a {DIFFICULTY} challenge and {?IS_BETTING}a bet of {BET_AMOUNT}{GOLD_ICON}.{?}friendly game.{\\?}", (OnConditionDelegate)(() => conversation_talk_common_to_taverngamehost_on_condition() && taverngamehost_player_sitting_now_on_condition()), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_game", "taverngamehost_talk", "taverngamehost_think_play", "{=BdpW8gUM}That looks good, let's play!", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_change_difficulty", "taverngamehost_talk", "taverngamehost_change_difficulty", "{=MbwG7Gy8}Can I change the difficulty?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_change_bet", "taverngamehost_talk", "taverngamehost_change_bet", "{=PbDK3PIi}Can I change the amount we're betting?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_game_history", "taverngamehost_talk", "taverngamehost_learn_history", "{=YM7etEzu}What exactly is {GAME_NAME}?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_reject", "taverngamehost_talk", "close_window", "{=N7BFbQmT}I'm not interested.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_start_playing_ask_accept", "taverngamehost_think_play", "taverngamehost_start_play", "{=GrHJYz7O}Very well. Now, what side do you want?", new OnConditionDelegate(taverngame_host_play_game_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_start_playing_ask_decline", "taverngamehost_think_play", "taverngamehost_talk", "{=bTnmpqU4}I'm afraid I don't have time for another game.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_first", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=7tuyySmq}I'll start.", new OnConditionDelegate(conversation_taverngamehost_talk_is_seega_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_one_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_last", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=J9fJlz2Y}You can start.", new OnConditionDelegate(conversation_taverngamehost_talk_is_seega_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_two_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_first_2", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=HdT5YyAb}I'll be white.", new OnConditionDelegate(conversation_taverngamehost_talk_is_puluc_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_one_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_last_2", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=i8HysulS}I'll be black.", new OnConditionDelegate(conversation_taverngamehost_talk_is_puluc_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_two_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_first_3", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=HdT5YyAb}I'll be white.", new OnConditionDelegate(conversation_taverngamehost_talk_is_konane_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_one_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_last_3", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=i8HysulS}I'll be black.", new OnConditionDelegate(conversation_taverngamehost_talk_is_konane_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_two_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_first_4", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=HdT5YyAb}I'll be white.", new OnConditionDelegate(conversation_taverngamehost_talk_is_mutorere_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_one_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_last_4", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=i8HysulS}I'll be black.", new OnConditionDelegate(conversation_taverngamehost_talk_is_mutorere_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_two_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_first_5", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=EnOOqaqf}I'll be sheep.", new OnConditionDelegate(conversation_taverngamehost_talk_is_baghchal_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_one_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_last_5", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=QjtOAyKE}I'll be wolves.", new OnConditionDelegate(conversation_taverngamehost_talk_is_baghchal_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_two_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_first_6", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=qsavxffL}I'll be attackers.", new OnConditionDelegate(conversation_taverngamehost_talk_is_tablut_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_one_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_last_6", "taverngamehost_start_play", "taverngamehost_confirm_play", "{=WD7vOalb}I'll be defenders.", new OnConditionDelegate(conversation_taverngamehost_talk_is_tablut_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_set_player_two_starts_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_back", "taverngamehost_start_play", "start", "{=dUSfRYYH}Just a minute..", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_start_playing_now", "taverngamehost_confirm_play", "close_window", "{=aB1EZssb}Great, let's begin!", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_taverngamehost_play_game_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_ask_difficulty", "taverngamehost_change_difficulty", "taverngamehost_changing_difficulty", "{=9VR0VeNT}Yes, how easy should I make things for you?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_change_difficulty_easy", "taverngamehost_changing_difficulty", "start", "{=j9Weia10}Easy", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_taverngamehost_difficulty_easy_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_change_difficulty_normal", "taverngamehost_changing_difficulty", "start", "{=8UBfIenN}Normal", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_taverngamehost_difficulty_normal_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_change_difficulty_hard", "taverngamehost_changing_difficulty", "start", "{=OnaJowBF}Hard. Don't hold back or you'll regret it.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_taverngamehost_difficulty_hard_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_ask_betting", "taverngamehost_change_bet", "taverngamehost_changing_bet", "{=T5jd4m69}That will only make this more fun. How much were you thinking?", new OnConditionDelegate(conversation_taverngamehost_talk_place_bet_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_100_denars", "taverngamehost_changing_bet", "start", "{=T29epQk3}100{GOLD_ICON}", new OnConditionDelegate(conversation_taverngamehost_can_bet_100_denars_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_bet_100_denars_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_200_denars", "taverngamehost_changing_bet", "start", "{=mHm5SLhb}200{GOLD_ICON}", new OnConditionDelegate(conversation_taverngamehost_can_bet_200_denars_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_bet_200_denars_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_300_denars", "taverngamehost_changing_bet", "start", "{=LnbzQIz6}300{GOLD_ICON}", new OnConditionDelegate(conversation_taverngamehost_can_bet_300_denars_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_bet_300_denars_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_400_denars", "taverngamehost_changing_bet", "start", "{=ck36TZFP}400{GOLD_ICON}", new OnConditionDelegate(conversation_taverngamehost_can_bet_400_denars_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_bet_400_denars_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_500_denars", "taverngamehost_changing_bet", "start", "{=YHTTPKMb}500{GOLD_ICON}", new OnConditionDelegate(conversation_taverngamehost_can_bet_500_denars_on_condition), new OnConsequenceDelegate(conversation_taverngamehost_bet_500_denars_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_0_denars", "taverngamehost_changing_bet", "start", "{=lVx35dWp}On second thought, let's keep this match friendly.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_taverngamehost_bet_0_denars_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_deny_betting", "taverngamehost_change_bet", "taverngamehost_changing_difficulty_for_bet", "{=4xtBNkjN}Unfortunately, I only allow betting when I'm playing at my best. You'll have to up the difficulty.", new OnConditionDelegate(conversation_taverngamehost_talk_not_place_bet_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_changing_difficulty_for_bet_yes", "taverngamehost_changing_difficulty_for_bet", "taverngamehost_change_bet_2", "{=i4xzuOJE}Sure, I'll play at the hardest level.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_taverngamehost_difficulty_hard_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_changing_difficulty_for_bet_no", "taverngamehost_changing_difficulty_for_bet", "start", "{=2ynnnR4c}I'd prefer to keep the difficulty where it's at.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_ask_betting_2", "taverngamehost_change_bet_2", "taverngamehost_changing_bet", "{=GfHssUYV}Now, feel free to place a bet.", new OnConditionDelegate(conversation_taverngamehost_talk_place_bet_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_tell_history_seega", "taverngamehost_learn_history", "taverngamehost_after_history", "{=9PUvbZzD}{GAME_NAME} is a traditional game within the {CULTURE_NAME}. It is a game of calm strategy. You start by placing your pieces on the board, crafting a trap for your enemy to fall into. Then you battle across the board, capturing and eliminating your opponent.", new OnConditionDelegate(conversation_taverngamehost_talk_is_seega_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_tell_history_puluc", "taverngamehost_learn_history", "taverngamehost_after_history", "{=sVcJTu7K}{GAME_NAME} is fast and harsh, as warfare should be. Capture as much as possible to keep your opponent weakened and demoralized. But behind this endless offense, there should always be a strong defense to punish any attempt from your opponent to regain control.", new OnConditionDelegate(conversation_taverngamehost_talk_is_puluc_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_tell_history_mutorere", "taverngamehost_learn_history", "taverngamehost_after_history", "{=SV0IEWD2}{GAME_NAME} is a game of anticipation. With no possibility of capturing, all your effort should be on reading your opponent and planning further ahead than him.", new OnConditionDelegate(conversation_taverngamehost_talk_is_mutorere_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_tell_history_konane", "taverngamehost_learn_history", "taverngamehost_after_history", "{=tVb0nWxm}War is all about sacrifice. In {GAME_NAME} you must make sure that your opponent sacrifices more than you do. Every move can expose you or your opponent and must be carefully considered.", new OnConditionDelegate(conversation_taverngamehost_talk_is_konane_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_tell_history_baghchal", "taverngamehost_learn_history", "taverngamehost_after_history", "{=mo4rbYvm}A couple of powerful wolves against a flock of helpless sheep. {GAME_NAME} is a game of uneven odds and seemingly all-powerful adversaries. But through strategy and sacrifice, even the sheep can dominate the wolves.", new OnConditionDelegate(conversation_taverngamehost_talk_is_baghchal_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("taverngamehost_tell_history_tablut", "taverngamehost_learn_history", "taverngamehost_after_history", "{=nMzfnOFG}{GAME_NAME} is a game of incredibly uneven odds. A weakened and trapped king must try to escape from a horde of attackers who assault from every direction. Ironic how we, the once all-powerful {CULTURE_NAME}, have now fallen in the same position.", new OnConditionDelegate(conversation_taverngamehost_talk_is_tablut_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_history_back", "taverngamehost_after_history", "start", "{=QP7L2YLG}Sounds fun.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("taverngamehost_player_history_leave", "taverngamehost_after_history", "close_window", "{=Ng6Rrlr6}I'd rather do something else", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("lord_player_play_game", "hero_main_options", "lord_answer_to_play_boardgame", "{=3hv4P5OO}Would you care to pass the time with a game of {GAME_NAME}?", new OnConditionDelegate(conversation_lord_talk_game_on_condition), (OnConsequenceDelegate)null, 2, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("lord_player_cancel_boardgame", "hero_main_options", "lord_answer_to_cancel_play_boardgame", "{=ySk7bD8P}Actually, I have other things to do. Maybe later.", new OnConditionDelegate(conversation_lord_talk_cancel_game_on_condition), (OnConsequenceDelegate)null, 2, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("lord_agrees_cancel_play", "lord_answer_to_cancel_play_boardgame", "close_window", "{=dzXaXKaC}Very well.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_lord_talk_cancel_game_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("lord_player_ask_to_play_boardgame_again", "hero_main_options", "lord_answer_to_play_again_boardgame", "{=U342eACh}Would you like to play another round of {GAME_NAME}?", new OnConditionDelegate(conversation_lord_talk_game_again_on_condition), (OnConsequenceDelegate)null, 2, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("lord_answer_to_play_boardgame_again_accept", "lord_answer_to_play_again_boardgame", "close_window", "{=aD1BoB3c}Yes. Let's have another round.", new OnConditionDelegate(conversation_lord_play_game_on_condition), new OnConsequenceDelegate(conversation_lord_play_game_again_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("lord_answer_to_play_boardgame_again_decline", "lord_answer_to_play_again_boardgame", "hero_main_options", "{=fqKVojaV}No, not now.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_lord_dont_play_game_again_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("lord_after_player_win_boardgame", "start", "close_window", "{=!}{PLAYER_GAME_WON_LORD_STRING}", new OnConditionDelegate(lord_after_player_win_boardgame_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("lord_after_lord_win_boardgame", "start", "hero_main_options", "{=dC6YhgPP}Ah. A good match, that.", new OnConditionDelegate(lord_after_lord_win_boardgame_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("lord_agrees_play", "lord_answer_to_play_boardgame", "lord_setup_game", "{=!}{GAME_AGREEMENT_STRING}", new OnConditionDelegate(conversation_lord_play_game_on_condition), new OnConsequenceDelegate(conversation_lord_detect_difficulty_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("lord_player_start_game", "lord_setup_game", "close_window", "{=bAy9PdrF}Let's begin, then.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_lord_play_game_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("lord_player_leave", "lord_setup_game", "close_window", "{=OQgBim7l}Actually, I have other things to do.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("lord_refuses_play", "lord_answer_to_play_boardgame", "close_window", "{=!}{LORD_REJECT_GAME_STRING}", new OnConditionDelegate(conversation_lord_reject_game_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
	}

	private bool conversation_lord_reject_game_condition()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		TextObject val = ((Hero.OneToOneConversationHero.GetRelationWithPlayer() > -20f) ? new TextObject("{=aRDcoLX0}Now is not a good time, {PLAYER.NAME}. ", (Dictionary<string, object>)null) : new TextObject("{=GLRrAj61}I do not wish to play games with the likes of you.", (Dictionary<string, object>)null));
		MBTextManager.SetTextVariable("LORD_REJECT_GAME_STRING", val, false);
		return true;
	}

	private bool conversation_talk_common_to_taverngamehost_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation != 14)
		{
			return false;
		}
		InitializeConversationVars();
		MBTextManager.SetTextVariable("GAME_MASTER_INTRO", "{=HDhLMbt7}Greetings, traveler. Do you play {GAME_NAME}? I am reckoned a master of this game, the traditional pastime of the {CULTURE_NAME}. If you are interested in playing, take a seat and we'll start.", false);
		if (Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan || Settlement.CurrentSettlement.MapFaction.Leader == Hero.MainHero)
		{
			MBTextManager.SetTextVariable("GAME_MASTER_INTRO", "{=yN4imaGo}Your {?PLAYER.GENDER}ladyship{?}lordship{\\?}... This is quite the honor. Do you play {GAME_NAME}? It's the traditional pastime of the {CULTURE_NAME}, and I am reckoned a master. If you wish to play a game, please, take a seat and we'll start.", false);
		}
		return true;
	}

	private void conversation_taverngamehost_bet_0_denars_on_consequence()
	{
		SetBetAmount(0);
	}

	private static bool conversation_taverngamehost_can_bet_100_denars_on_condition()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		CharacterObject oneToOneConversationCharacter = ConversationMission.OneToOneConversationCharacter;
		CharacterObject val = (CharacterObject)Agent.Main.Character;
		bool num = !((BasicCharacterObject)oneToOneConversationCharacter).IsHero || oneToOneConversationCharacter.HeroObject.Gold >= 100;
		bool flag = val.HeroObject.Gold >= 100;
		return num && flag;
	}

	private void conversation_taverngamehost_bet_100_denars_on_consequence()
	{
		SetBetAmount(100);
	}

	private static bool conversation_taverngamehost_can_bet_200_denars_on_condition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		CharacterObject val = (CharacterObject)ConversationMission.OneToOneConversationAgent.Character;
		CharacterObject val2 = (CharacterObject)Agent.Main.Character;
		bool num = !((BasicCharacterObject)val).IsHero || val.HeroObject.Gold >= 200;
		bool flag = val2.HeroObject.Gold >= 200;
		return num && flag;
	}

	private void conversation_taverngamehost_bet_200_denars_on_consequence()
	{
		SetBetAmount(200);
	}

	private static bool conversation_taverngamehost_can_bet_300_denars_on_condition()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		CharacterObject oneToOneConversationCharacter = ConversationMission.OneToOneConversationCharacter;
		CharacterObject val = (CharacterObject)Agent.Main.Character;
		bool num = !((BasicCharacterObject)oneToOneConversationCharacter).IsHero || oneToOneConversationCharacter.HeroObject.Gold >= 300;
		bool flag = val.HeroObject.Gold >= 300;
		return num && flag;
	}

	private void conversation_taverngamehost_bet_300_denars_on_consequence()
	{
		SetBetAmount(300);
	}

	private static bool conversation_taverngamehost_can_bet_400_denars_on_condition()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		CharacterObject oneToOneConversationCharacter = ConversationMission.OneToOneConversationCharacter;
		CharacterObject val = (CharacterObject)Agent.Main.Character;
		bool num = !((BasicCharacterObject)oneToOneConversationCharacter).IsHero || oneToOneConversationCharacter.HeroObject.Gold >= 400;
		bool flag = val.HeroObject.Gold >= 400;
		return num && flag;
	}

	private void conversation_taverngamehost_bet_400_denars_on_consequence()
	{
		SetBetAmount(400);
	}

	private static bool conversation_taverngamehost_can_bet_500_denars_on_condition()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		CharacterObject oneToOneConversationCharacter = ConversationMission.OneToOneConversationCharacter;
		CharacterObject val = (CharacterObject)Agent.Main.Character;
		bool num = !((BasicCharacterObject)oneToOneConversationCharacter).IsHero || oneToOneConversationCharacter.HeroObject.Gold >= 500;
		bool flag = val.HeroObject.Gold >= 500;
		return num && flag;
	}

	private bool taverngame_host_play_game_on_condition()
	{
		if (_betAmount == 0)
		{
			return true;
		}
		DeleteOldBoardGamesOfChampion();
		return !_wonBoardGamesInOneWeekInSettlement.ContainsKey(Settlement.CurrentSettlement);
	}

	private void conversation_taverngamehost_bet_500_denars_on_consequence()
	{
		SetBetAmount(500);
	}

	private void conversation_taverngamehost_difficulty_easy_on_consequence()
	{
		SetDifficulty((AIDifficulty)0);
		SetBetAmount(0);
	}

	private void conversation_taverngamehost_difficulty_normal_on_consequence()
	{
		SetDifficulty((AIDifficulty)1);
		SetBetAmount(0);
	}

	private void conversation_taverngamehost_difficulty_hard_on_consequence()
	{
		SetDifficulty((AIDifficulty)2);
	}

	private static void conversation_lord_play_game_again_on_consequence()
	{
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().DetectOpposingAgent();
		Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
		{
			Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().StartBoardGame();
		};
	}

	private static void conversation_lord_dont_play_game_again_on_consequence()
	{
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().SetGameOver(GameOverEnum.PlayerCanceledTheGame);
	}

	private void conversation_lord_detect_difficulty_consequence()
	{
		int skillValue = ((BasicCharacterObject)ConversationMission.OneToOneConversationCharacter).GetSkillValue(DefaultSkills.Steward);
		if (skillValue >= 0 && skillValue < 50)
		{
			SetDifficulty((AIDifficulty)0);
		}
		else if (skillValue >= 50 && skillValue < 100)
		{
			SetDifficulty((AIDifficulty)1);
		}
		else if (skillValue >= 100)
		{
			SetDifficulty((AIDifficulty)2);
		}
	}

	private static void conversation_taverngamehost_set_player_one_starts_on_consequence()
	{
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().SetStartingPlayer(playerOneStarts: true);
	}

	private static void conversation_taverngamehost_set_player_two_starts_on_consequence()
	{
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().SetStartingPlayer(playerOneStarts: false);
	}

	private static void conversation_taverngamehost_play_game_on_consequence()
	{
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().DetectOpposingAgent();
		Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
		{
			Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().StartBoardGame();
		};
	}

	private bool conversation_taverngamehost_talk_place_bet_on_condition()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		CharacterObject oneToOneConversationCharacter = ConversationMission.OneToOneConversationCharacter;
		bool num = !((BasicCharacterObject)oneToOneConversationCharacter).IsHero || oneToOneConversationCharacter.HeroObject.Gold >= 100;
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		if (num)
		{
			return (int)_difficulty == 2;
		}
		return false;
	}

	private bool conversation_taverngamehost_talk_not_place_bet_on_condition()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		CharacterObject oneToOneConversationCharacter = ConversationMission.OneToOneConversationCharacter;
		bool num = !((BasicCharacterObject)oneToOneConversationCharacter).IsHero || oneToOneConversationCharacter.HeroObject.Gold >= 100;
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		if (num)
		{
			return (int)_difficulty != 2;
		}
		return false;
	}

	private static bool conversation_taverngamehost_talk_is_seega_on_condition()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		MissionBoardGameLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		if (missionBehavior != null)
		{
			return (int)missionBehavior.CurrentBoardGame == 0;
		}
		return false;
	}

	private static bool conversation_taverngamehost_talk_is_puluc_on_condition()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		MissionBoardGameLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		if (missionBehavior != null)
		{
			return (int)missionBehavior.CurrentBoardGame == 1;
		}
		return false;
	}

	private static bool conversation_taverngamehost_talk_is_mutorere_on_condition()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		MissionBoardGameLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		if (missionBehavior != null)
		{
			return (int)missionBehavior.CurrentBoardGame == 3;
		}
		return false;
	}

	private static bool conversation_taverngamehost_talk_is_konane_on_condition()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		MissionBoardGameLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		if (missionBehavior != null)
		{
			return (int)missionBehavior.CurrentBoardGame == 2;
		}
		return false;
	}

	private static bool conversation_taverngamehost_talk_is_baghchal_on_condition()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		MissionBoardGameLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		if (missionBehavior != null)
		{
			return (int)missionBehavior.CurrentBoardGame == 5;
		}
		return false;
	}

	private static bool conversation_taverngamehost_talk_is_tablut_on_condition()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		MissionBoardGameLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		if (missionBehavior != null)
		{
			return (int)missionBehavior.CurrentBoardGame == 4;
		}
		return false;
	}

	public static bool taverngamehost_player_sitting_now_on_condition()
	{
		GameEntity val = Mission.Current.Scene.FindEntityWithTag("gambler_player");
		if (val != (GameEntity)null)
		{
			Chair chair = ((IEnumerable<Chair>)MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<Chair>(val)).FirstOrDefault();
			if (chair != null && Agent.Main != null)
			{
				return chair.IsAgentFullySitting(Agent.Main);
			}
			return false;
		}
		return false;
	}

	private bool conversation_lord_talk_game_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 3)
		{
			ICampaignMission current = CampaignMission.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				Location location = current.Location;
				obj = ((location != null) ? location.StringId : null);
			}
			if ((string?)obj == "lordshall" && MissionBoardGameLogic.IsBoardGameAvailable())
			{
				InitializeConversationVars();
				return true;
			}
		}
		return false;
	}

	private static bool conversation_lord_talk_game_again_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 3 && MissionBoardGameLogic.IsThereActiveBoardGameWithHero(Hero.OneToOneConversationHero))
		{
			return Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().IsGameInProgress;
		}
		return false;
	}

	private static bool conversation_lord_talk_cancel_game_on_condition()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation == 3 && MissionBoardGameLogic.IsThereActiveBoardGameWithHero(Hero.OneToOneConversationHero))
		{
			if (!Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().IsOpposingAgentMovingToPlayingChair)
			{
				return !Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().IsGameInProgress;
			}
			return true;
		}
		return false;
	}

	private static void conversation_lord_talk_cancel_game_on_consequence()
	{
		Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
		{
			Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().SetGameOver(GameOverEnum.PlayerCanceledTheGame);
		};
	}

	private static bool lord_after_lord_win_boardgame_condition()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		Mission current = Mission.Current;
		MissionBoardGameLogic missionBoardGameLogic = ((current != null) ? current.GetMissionBehavior<MissionBoardGameLogic>() : null);
		if (missionBoardGameLogic != null && (int)missionBoardGameLogic.BoardGameFinalState != 0)
		{
			return (int)missionBoardGameLogic.BoardGameFinalState != 1;
		}
		return false;
	}

	private bool lord_after_player_win_boardgame_condition()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		Mission current = Mission.Current;
		MissionBoardGameLogic missionBoardGameLogic = ((current != null) ? current.GetMissionBehavior<MissionBoardGameLogic>() : null);
		if (missionBoardGameLogic != null && (int)missionBoardGameLogic.BoardGameFinalState == 1)
		{
			if (_relationGained)
			{
				MBTextManager.SetTextVariable("PLAYER_GAME_WON_LORD_STRING", "{=QTfliM5b}I enjoyed our game. Let's play again later.", false);
			}
			else if (_influenceGained)
			{
				MBTextManager.SetTextVariable("PLAYER_GAME_WON_LORD_STRING", "{=31oG5njl}You are a sharp thinker. Our kingdom would do well to hear your thoughts on matters of importance.", false);
			}
			else if (_opposingHeroExtraXPGained)
			{
				MBTextManager.SetTextVariable("PLAYER_GAME_WON_LORD_STRING", "{=nxpyHb77}Well, I am still a novice in this game, but I learned a lot from playing with you.", false);
			}
			else if (_renownGained)
			{
				MBTextManager.SetTextVariable("PLAYER_GAME_WON_LORD_STRING", "{=k1b5crrx}You are an accomplished player. I will take note of that.", false);
			}
			else if (_gainedNothing)
			{
				MBTextManager.SetTextVariable("PLAYER_GAME_WON_LORD_STRING", "{=HzabMi4t}That was a fun game. Thank you.", false);
			}
			return true;
		}
		return false;
	}

	private bool conversation_lord_play_game_on_condition()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		if (CanPlayerPlayBoardGameAgainstHero(Hero.OneToOneConversationHero))
		{
			string text = "DrinkingInTavernTag";
			if (MissionConversationLogic.Current.ConversationManager.IsTagApplicable(text, Hero.OneToOneConversationHero.CharacterObject))
			{
				MBTextManager.SetTextVariable("GAME_AGREEMENT_STRING", new TextObject("{=LztDzy8W}Why not? I'm not going anywhere right now, and I could use another drink.", (Dictionary<string, object>)null), false);
			}
			else if (Hero.OneToOneConversationHero.CharacterObject.GetPersona() == DefaultTraits.PersonaCurt)
			{
				MBTextManager.SetTextVariable("GAME_AGREEMENT_STRING", new TextObject("{=2luygc8o}Mm. I suppose. Takes my mind off all these problems I have to deal with.", (Dictionary<string, object>)null), false);
			}
			else if (Hero.OneToOneConversationHero.CharacterObject.GetPersona() == DefaultTraits.PersonaEarnest)
			{
				MBTextManager.SetTextVariable("GAME_AGREEMENT_STRING", new TextObject("{=349mwgWC}Certainly. A good game always keeps the mind active and fresh.", (Dictionary<string, object>)null), false);
			}
			else if (Hero.OneToOneConversationHero.CharacterObject.GetPersona() == DefaultTraits.PersonaIronic)
			{
				MBTextManager.SetTextVariable("GAME_AGREEMENT_STRING", new TextObject("{=rGaaVBBT}Ah. Very well. I don't mind testing your mettle.", (Dictionary<string, object>)null), false);
			}
			else if (Hero.OneToOneConversationHero.CharacterObject.GetPersona() == DefaultTraits.PersonaSoftspoken)
			{
				MBTextManager.SetTextVariable("GAME_AGREEMENT_STRING", new TextObject("{=idPV1Csj}Yes... Why not? I have nothing too urgent right now.", (Dictionary<string, object>)null), false);
			}
			return true;
		}
		return false;
	}

	private static void conversation_lord_play_game_on_consequence()
	{
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().DetectOpposingAgent();
	}

	public void PlayerWonAgainstTavernChampion()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!_wonBoardGamesInOneWeekInSettlement.ContainsKey(Settlement.CurrentSettlement))
		{
			_wonBoardGamesInOneWeekInSettlement.Add(Settlement.CurrentSettlement, CampaignTime.Now);
		}
	}

	private void GameEndWithHero(Hero hero)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (_heroAndBoardGameTimeDictionary.ContainsKey(hero))
		{
			_heroAndBoardGameTimeDictionary[hero].Add(CampaignTime.Now);
			return;
		}
		_heroAndBoardGameTimeDictionary.Add(hero, new List<CampaignTime>());
		_heroAndBoardGameTimeDictionary[hero].Add(CampaignTime.Now);
	}

	private bool CanPlayerPlayBoardGameAgainstHero(Hero hero)
	{
		if (hero.GetRelationWithPlayer() >= 0f)
		{
			DeleteOldBoardGamesOfHero(hero);
			if (_heroAndBoardGameTimeDictionary.ContainsKey(hero))
			{
				List<CampaignTime> list = _heroAndBoardGameTimeDictionary[hero];
				return 3 > list.Count;
			}
			return true;
		}
		return false;
	}

	private void DeleteOldBoardGamesOfChampion()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		foreach (Settlement item in (List<Settlement>)(object)Settlement.All)
		{
			if (_wonBoardGamesInOneWeekInSettlement.ContainsKey(item))
			{
				CampaignTime val = _wonBoardGamesInOneWeekInSettlement[item];
				if (((CampaignTime)(ref val)).ElapsedWeeksUntilNow >= 1f)
				{
					_wonBoardGamesInOneWeekInSettlement.Remove(item);
				}
			}
		}
	}

	private void DeleteOldBoardGamesOfHero(Hero hero)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!_heroAndBoardGameTimeDictionary.ContainsKey(hero))
		{
			return;
		}
		List<CampaignTime> list = _heroAndBoardGameTimeDictionary[hero];
		for (int num = list.Count - 1; num >= 0; num--)
		{
			CampaignTime val = list[num];
			if (((CampaignTime)(ref val)).ElapsedDaysUntilNow > 1f)
			{
				list.RemoveAt(num);
			}
		}
		if (Extensions.IsEmpty<CampaignTime>((IEnumerable<CampaignTime>)list))
		{
			_heroAndBoardGameTimeDictionary.Remove(hero);
		}
	}

	public void SetBetAmount(int bet)
	{
		_betAmount = bet;
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().SetBetAmount(bet);
		MBTextManager.SetTextVariable("BET_AMOUNT", bet.ToString(), false);
		MBTextManager.SetTextVariable("IS_BETTING", (bet > 0) ? 1 : 0);
	}

	private unsafe void SetDifficulty(AIDifficulty difficulty)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_difficulty = difficulty;
		Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().SetCurrentDifficulty(difficulty);
		MBTextManager.SetTextVariable("DIFFICULTY", GameTexts.FindText("str_boardgame_difficulty", ((object)(*(AIDifficulty*)(&difficulty))/*cast due to .constrained prefix*/).ToString()), false);
	}
}
