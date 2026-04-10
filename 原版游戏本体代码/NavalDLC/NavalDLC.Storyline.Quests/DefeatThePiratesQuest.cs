using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class DefeatThePiratesQuest : NavalStorylineQuestBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<PartyTemplateStack, int> _003C_003E9__19_0;

		public static OnConditionDelegate _003C_003E9__25_0;

		public static OnConsequenceDelegate _003C_003E9__25_1;

		public static OnConsequenceDelegate _003C_003E9__25_2;

		public static OnConsequenceDelegate _003C_003E9__25_4;

		public static Func<Clan, bool> _003C_003E9__55_0;

		public static Func<Settlement, bool> _003C_003E9__55_1;

		internal int _003Cget_PirateTroopCount_003Eb__19_0(PartyTemplateStack t)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return t.MaxValue;
		}

		internal bool _003CSetDialogs_003Eb__25_0()
		{
			Mission current = Mission.Current;
			PirateBattleMissionController pirateBattleMissionController = ((current != null) ? current.GetMissionBehavior<PirateBattleMissionController>() : null);
			if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && pirateBattleMissionController != null)
			{
				return pirateBattleMissionController.IsFirstShipCleared;
			}
			return false;
		}

		internal void _003CSetDialogs_003Eb__25_1()
		{
			PirateBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<PirateBattleMissionController>();
			Campaign.Current.ConversationManager.ConversationEndOneShot += missionBehavior.OnPlayerSelectedSecondShipToCommand;
		}

		internal void _003CSetDialogs_003Eb__25_2()
		{
			PirateBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<PirateBattleMissionController>();
			Campaign.Current.ConversationManager.ConversationEndOneShot += missionBehavior.OnPlayerSelectedFirstShipToCommand;
		}

		internal void _003CSetDialogs_003Eb__25_4()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject);
			val.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
			Vec3 globalPosition = Mission.Current.Scene.FindEntityWithName("free_infantry_spawn_point_0").GlobalPosition;
			val.InitialPosition(ref globalPosition);
			Vec3 lookDirection = Agent.Main.LookDirection;
			Vec2 val2 = ((Vec3)(ref lookDirection)).AsVec2;
			val2 = ((Vec2)(ref val2)).Normalized();
			val.InitialDirection(ref val2);
			if (Mission.Current != null)
			{
				Agent item = Mission.Current.SpawnAgent(val, false);
				Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<IAgent> { (IAgent)(object)item }, true);
			}
		}

		internal bool _003CSpawnPirates_003Eb__55_0(Clan t)
		{
			return ((MBObjectBase)t).StringId == "northern_pirates";
		}

		internal bool _003CSpawnPirates_003Eb__55_1(Settlement t)
		{
			return t.IsHideout;
		}
	}

	private const string EncounterMenuId = "quest3_encounter_menu";

	private const string RetryMenuId = "quest3_retry_menu";

	private const string PiratePartyTemplateStringId = "storyline_act_2_sea_hounds_template";

	private const string PirateConversationCharacterId = "sea_hounds";

	public const string PlayerPartySailPatternId = "generated_square__h4_09";

	public const string PiratePartySailPatternId = "generated_square_l1_h4_10";

	private static readonly Dictionary<string, string> PlayerShipUpgradePieces = new Dictionary<string, string> { { "sail", "sails_lvl2" } };

	private static readonly Dictionary<string, string> PirateShipUpgradePieces = new Dictionary<string, string> { { "sail", "sails_lvl2" } };

	[SaveableField(1)]
	private MobileParty _pirateParty;

	[SaveableField(2)]
	private bool _battleWon;

	[SaveableField(3)]
	private bool _battleFinished;

	private PartyTemplateObject _pirateTemplate;

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act2;

	public override bool WillProgressStoryline => true;

	protected override string MainPartyTemplateStringId => "storyline_act_2_main_party_template";

	public int PirateTroopCount => ((IEnumerable<PartyTemplateStack>)_pirateTemplate.Stacks).Sum((PartyTemplateStack t) => t.MaxValue);

	public override TextObject Title => new TextObject("{=wKBtraSp}Defeat the Sea Hounds", (Dictionary<string, object>)null);

	private TextObject _descriptionLogText => new TextObject("{=VWK3jIqG}Defeat the two Sea Hound vessels that are lying in wait outside of Ostican.", (Dictionary<string, object>)null);

	public DefeatThePiratesQuest(string questId, Hero questGiver)
		: base(questId, questGiver, CampaignTime.Never, 0)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		_pirateTemplate = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act_2_sea_hounds_template");
		((QuestBase)this).AddLog(_descriptionLogText, false);
	}

	protected override void SetDialogs()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_012d: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		//IL_0186: Expected O, but got Unknown
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Expected O, but got Unknown
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Expected O, but got Unknown
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Expected O, but got Unknown
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Expected O, but got Unknown
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Expected O, but got Unknown
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Expected O, but got Unknown
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Expected O, but got Unknown
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Expected O, but got Unknown
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=NW5vE1xa}That's one Sea Hound defeated, but the other can't be too far away. We've captured a second ship, though. It's a snekkja - it should be quick and nimble. How about you cross over and take the helm? I'll keep command of our old knarr.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__25_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = delegate
			{
				Mission current = Mission.Current;
				PirateBattleMissionController pirateBattleMissionController = ((current != null) ? current.GetMissionBehavior<PirateBattleMissionController>() : null);
				return Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && pirateBattleMissionController != null && pirateBattleMissionController.IsFirstShipCleared;
			};
			_003C_003Ec._003C_003E9__25_0 = val;
			obj2 = (object)val;
		}
		DialogFlow obj3 = obj.Condition((OnConditionDelegate)obj2).BeginPlayerOptions((string)null, false).PlayerOption("{=alDwmQtB}I'll go do that.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj4 = _003C_003Ec._003C_003E9__25_1;
		if (obj4 == null)
		{
			OnConsequenceDelegate val2 = delegate
			{
				PirateBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<PirateBattleMissionController>();
				Campaign.Current.ConversationManager.ConversationEndOneShot += missionBehavior.OnPlayerSelectedSecondShipToCommand;
			};
			_003C_003Ec._003C_003E9__25_1 = val2;
			obj4 = (object)val2;
		}
		DialogFlow obj5 = obj3.Consequence((OnConsequenceDelegate)obj4).NpcLine("{=qauwgx3r}Splendid. Let's go chase down that second Sea Hound.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
			.PlayerOption("{=cnjTiMmv}Very good. I'll keep command of our old knarr. You captain this agile snekkja.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj6 = _003C_003Ec._003C_003E9__25_2;
		if (obj6 == null)
		{
			OnConsequenceDelegate val3 = delegate
			{
				PirateBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<PirateBattleMissionController>();
				Campaign.Current.ConversationManager.ConversationEndOneShot += missionBehavior.OnPlayerSelectedFirstShipToCommand;
			};
			_003C_003Ec._003C_003E9__25_2 = val3;
			obj6 = (object)val3;
		}
		conversationManager.AddDialogFlow(obj5.Consequence((OnConsequenceDelegate)obj6).NpcLine("{=qauwgx3r}Splendid. Let's go chase down that second Sea Hound.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
		string text = "";
		ConversationManager conversationManager2 = Campaign.Current.ConversationManager;
		DialogFlow obj7 = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=dF7jeK5a}I'm new at this, my {?PLAYER.GENDER}lady{?}lord{\\?}! I'm just a farmer who fell on hard times. I signed on with this ship in Varcheg a month ago. They told me we'd be trading grain and ivory across the Byalic. I didn't know we'd be attacking honest folk like yourselves![ib:weary]", new OnMultipleConversationConsequenceDelegate(IsPirate), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null).Condition((OnConditionDelegate)delegate
		{
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
			CharacterObject oneToOneConversationCharacter = CharacterObject.OneToOneConversationCharacter;
			MobileParty pirateParty = _pirateParty;
			return oneToOneConversationCharacter == ConversationHelper.GetConversationCharacterPartyLeader((pirateParty != null) ? pirateParty.Party : null);
		});
		object obj8 = _003C_003Ec._003C_003E9__25_4;
		if (obj8 == null)
		{
			OnConsequenceDelegate val4 = delegate
			{
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0010: Expected O, but got Unknown
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_002c: Expected O, but got Unknown
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_0056: Unknown result type (might be due to invalid IL or missing references)
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_0067: Unknown result type (might be due to invalid IL or missing references)
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				AgentBuildData val5 = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject);
				val5.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val5.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
				Vec3 globalPosition = Mission.Current.Scene.FindEntityWithName("free_infantry_spawn_point_0").GlobalPosition;
				val5.InitialPosition(ref globalPosition);
				Vec3 lookDirection = Agent.Main.LookDirection;
				Vec2 val6 = ((Vec3)(ref lookDirection)).AsVec2;
				val6 = ((Vec2)(ref val6)).Normalized();
				val5.InitialDirection(ref val6);
				if (Mission.Current != null)
				{
					Agent item = Mission.Current.SpawnAgent(val5, false);
					Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<IAgent> { (IAgent)(object)item }, true);
				}
			};
			_003C_003Ec._003C_003E9__25_4 = val4;
			obj8 = (object)val4;
		}
		conversationManager2.AddDialogFlow(obj7.Consequence((OnConsequenceDelegate)obj8).NpcLine("{=GsPj9ptT}Listen - these Sea Hounds are trolls and demons, not men! I want no part of this any more! Spare me, and I promise I'll go back to my old life.", new OnMultipleConversationConsequenceDelegate(IsPirate), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null).BeginPlayerOptions((string)null, false)
			.PlayerOption("{=LBoq4sXI}Tell me the truth, and I'll let you live.", new OnMultipleConversationConsequenceDelegate(IsPirate), (string)null, text)
			.PlayerOption("{=wTEbf3gc}I am looking for my sister. Let me know how to find her, and we will spare your life.", new OnMultipleConversationConsequenceDelegate(IsPirate), (string)null, text)
			.EndPlayerOptions()
			.GenerateToken(ref text)
			.NpcLine("{=Q3bpobtL}We purchased some slaves from some bandits in Ostican. We were planning on selling them onward to another buyer further south along the coast. Perhaps your sister was one of them? Will you spare me?", new OnMultipleConversationConsequenceDelegate(IsPirate), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=b1saAIdA}Are you really a farmer, now? Callouses such as those on your hands are made by oars, not ploughs. And I see a scar on your sword-arm that doesn't look like it came from the kick of a mule. Indeed, I might even recall your name. Hralgar Eel-Nose, is it not?", new OnMultipleConversationConsequenceDelegate(IsGunnar), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=tiHQafDb}[if:convo_predatory][ib:warrior]Gunnar of Langshofn… Three of your old shipmates have we visited while reeving. One died well. The others… It's said that your people are mean and stingy hosts, but those two gave us some fine entertainment.", new OnMultipleConversationConsequenceDelegate(IsPirate), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=yhEKOBfT}As for you, friend of Gunnar... I told you where to seek your sister. Best rescue her quick, or she may take a liking to one of our brave lads and give you a litter of Sea Puppies. So there you have it… I fulfilled my end of the bargain. Put me ashore.", new OnMultipleConversationConsequenceDelegate(IsPirate), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=00iNZpwG}You lied. The bargain is void. Gunnar, do what you will with him.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += OnOption1Chosen;
			})
			.CloseDialog()
			.PlayerOption("{=RSBjrwHG}We will spare your life, but the sea may have other plans for you. Over the side you go.", new OnMultipleConversationConsequenceDelegate(IsPirate), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += OnOption2Chosen;
			})
			.CloseDialog()
			.PlayerOption("{=KfQHGUID}I keep my bargains, however loathsome they may be. We shall put you ashore.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += OnOption3Chosen;
			})
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
	}

	private void AddGameMenus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0024: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0059: Expected O, but got Unknown
		//IL_0059: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_008e: Expected O, but got Unknown
		//IL_008e: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00b2: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		((QuestBase)this).AddGameMenu("quest3_retry_menu", new TextObject("{=etH1IHNZ}You manage to put some distance between you and your enemies, and you have a moment to consider how to proceed.", (Dictionary<string, object>)null), new OnInitDelegate(retry_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("quest3_retry_menu", "try_again_option", new TextObject("{=YHMDy3lQ}Try again", (Dictionary<string, object>)null), new OnConditionDelegate(retry_menu_try_again_on_condition), new OnConsequenceDelegate(retry_menu_try_again_on_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("quest3_retry_menu", "leave_option", new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null), new OnConditionDelegate(leave_on_condition), new OnConsequenceDelegate(leave_on_consequence), true, -1);
		((QuestBase)this).AddGameMenu("quest3_encounter_menu", new TextObject("{=Mv2qMTmx}As you sail out of Ostican harbor you spot a single ship, anchored just offshore. As soon as it sights you it runs out its oars and steers to intercept your course. It is not waiting for its partner, and is probably not expecting you to put up much of a fight.", (Dictionary<string, object>)null), new OnInitDelegate(encounter_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("quest3_encounter_menu", "fight_option", new TextObject("{=Ky03jg94}Fight", (Dictionary<string, object>)null), new OnConditionDelegate(encounter_menu_attack_on_condition), new OnConsequenceDelegate(encounter_menu_attack_on_consequence), false, -1);
	}

	private bool retry_menu_try_again_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)12;
		if (_battleFinished)
		{
			return !_battleWon;
		}
		return false;
	}

	private void retry_menu_try_again_on_consequence(MenuCallbackArgs args)
	{
		OnRetry();
	}

	private bool leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		args.Tooltip = new TextObject("{=wmTjX28f}This will exit story mode and return you to the Sandbox. You can continue the storyline later by talking to Gunnar in the port again.", (Dictionary<string, object>)null);
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void leave_on_consequence(MenuCallbackArgs args)
	{
		((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
		NavalStorylineData.DeactivateNavalStoryline();
	}

	private void retry_menu_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
		if (_battleFinished && _battleWon)
		{
			OnPlayerWon();
		}
	}

	private void encounter_menu_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act2EncounterMenu);
		MobileParty.MainParty.SetMoveModeHold();
		MobileParty pirateParty = _pirateParty;
		if (pirateParty != null)
		{
			pirateParty.SetMoveModeHold();
		}
	}

	private bool encounter_menu_attack_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)12;
		return true;
	}

	private void encounter_menu_attack_on_consequence(MenuCallbackArgs args)
	{
		StartBattle();
	}

	private bool IsGunnar(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
	}

	private bool IsPirate(IAgent agent)
	{
		return ((MBObjectBase)agent.Character).StringId == "sea_hounds";
	}

	private bool IsMainHero(IAgent agent)
	{
		return (object)agent.Character == CharacterObject.PlayerCharacter;
	}

	private void OnOption1Chosen()
	{
		GainRenownAction.Apply(Hero.MainHero, 10f, false);
		TraitLevelingHelper.OnIssueSolvedThroughQuest(Hero.MainHero, DefaultTraits.Honor, -5);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private void OnOption2Chosen()
	{
		GainRenownAction.Apply(Hero.MainHero, 5f, false);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private void OnOption3Chosen()
	{
		TraitLevelingHelper.OnIssueSolvedThroughQuest(Hero.MainHero, DefaultTraits.Honor, 20);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	protected override void InitializeQuestOnGameLoadInternal()
	{
		_pirateTemplate = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act_2_sea_hounds_template");
		AddGameMenus();
		((QuestBase)this).SetDialogs();
		if (MobileParty.MainParty.IsActive)
		{
			NavalDLCHelpers.SetCustomSailPatternOfPartyShips(MobileParty.MainParty, "generated_square__h4_09");
		}
		if (_pirateParty != null && _pirateParty.IsActive)
		{
			NavalDLCHelpers.SetCustomSailPatternOfPartyShips(_pirateParty, "generated_square_l1_h4_10");
		}
	}

	protected override void OnStartQuestInternal()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		SpawnPirates(NavalStorylineData.HomeSettlement);
		MobileParty.MainParty.IgnoreByOtherPartiesTill(((QuestBase)this).QuestDueTime);
		NavalDLCHelpers.SetCustomSailPatternOfPartyShips(MobileParty.MainParty, "generated_square__h4_09");
		NavalDLCHelpers.AddUpgradePiecesToPartyShips(MobileParty.MainParty, PlayerShipUpgradePieces);
	}

	protected override void HourlyTick()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (_pirateParty != null)
		{
			CampaignVec2 position = MobileParty.MainParty.Position;
			if (((CampaignVec2)(ref position)).DistanceSquared(_pirateParty.Position) <= Campaign.Current.Models.EncounterModel.GetEncounterJoiningRadius * Campaign.Current.Models.EncounterModel.GetEncounterJoiningRadius * 1.5f)
			{
				GameMenu.ActivateGameMenu("quest3_encounter_menu");
			}
		}
	}

	private void StartBattle()
	{
		foreach (Ship item in (List<Ship>)(object)_pirateParty.Ships)
		{
			item.IsInvulnerable = false;
		}
		PlayerEncounter.RestartPlayerEncounter(PartyBase.MainParty, _pirateParty.Party, false);
		PlayerEncounter.StartBattle();
		GameMenu.ActivateGameMenu("quest3_retry_menu");
		OpenPirateBattleMission();
	}

	private void OpenPirateBattleMission()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		MissionInitializerRecord navalMissionInitializerTemplate = NavalStorylineData.GetNavalMissionInitializerTemplate("naval_storyline_act_2_tutorial");
		navalMissionInitializerTemplate.PlayingInCampaignMode = true;
		navalMissionInitializerTemplate.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.Position);
		NavalMissions.OpenNavalStorylinePirateBattleMission(navalMissionInitializerTemplate, _pirateParty, PirateTroopCount);
	}

	protected override void RegisterEventsInternal()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (party == MobileParty.MainParty && _pirateParty != null)
		{
			_pirateParty.Ai.SetDoNotMakeNewDecisions(false);
			_pirateParty.SetMoveEngageParty(MobileParty.MainParty, (NavigationType)2);
			_pirateParty.Ai.SetDoNotMakeNewDecisions(true);
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		if (PlayerEncounter.Current == null)
		{
			return;
		}
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		MobileParty pirateParty = _pirateParty;
		if (encounteredParty == ((pirateParty != null) ? pirateParty.Party : null))
		{
			_battleFinished = true;
			_battleWon = false;
			if (PlayerEncounter.Battle != null && (int)PlayerEncounter.BattleState == 1)
			{
				_battleWon = true;
			}
			Hero.MainHero.Heal(Hero.MainHero.MaxHitPoints, false);
		}
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (party == MobileParty.MainParty && _pirateParty != null)
		{
			_pirateParty.Ai.SetDoNotMakeNewDecisions(false);
			_pirateParty.SetMovePatrolAroundPoint(settlement.PortPosition, (NavigationType)2);
			_pirateParty.Ai.SetDoNotMakeNewDecisions(true);
		}
	}

	protected override void OnFinalizeInternal()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerEncounter.Battle != null && PlayerEncounter.Battle.InvolvedParties.Contains(_pirateParty.Party))
		{
			PlayerEncounter.Finish(true);
		}
		if (_pirateParty != null)
		{
			if (_pirateParty.IsActive)
			{
				_pirateParty.Ai.DisableAi();
				DestroyPartyAction.Apply((PartyBase)null, _pirateParty);
			}
			_pirateParty = null;
		}
		MobileParty.MainParty.IgnoreByOtherPartiesTill(CampaignTime.Now);
	}

	protected override void OnCompleteWithSuccessInternal()
	{
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act2Finalized);
	}

	private void OnPlayerWon()
	{
		StartConversationWithPirate();
	}

	private void StartConversationWithPirate()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("sea_hounds");
		_pirateParty.Party.AddElementToMemberRoster(val, 1, false);
		CharacterObject conversationCharacterPartyLeader = ConversationHelper.GetConversationCharacterPartyLeader(_pirateParty.Party);
		ConversationCharacterData val2 = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, false, false, false, false, true);
		ConversationCharacterData val3 = default(ConversationCharacterData);
		((ConversationCharacterData)(ref val3))._002Ector(conversationCharacterPartyLeader, _pirateParty.Party, true, false, false, false, false, true);
		CampaignMission.OpenConversationMission(val2, val3, "conversation_scene_sea_multi_agent", "", true);
	}

	private void OnRetry()
	{
		RefreshPiratePartyForces();
		_battleFinished = false;
		_battleWon = false;
		OpenPirateBattleMission();
	}

	private void SpawnPirates(Settlement settlement)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		Clan val = ((IEnumerable<Clan>)Clan.All).FirstOrDefault((Func<Clan, bool>)((Clan t) => ((MBObjectBase)t).StringId == "northern_pirates"));
		CampaignVec2 val2 = NavigationHelper.FindReachablePointAroundPosition(settlement.PortPosition, (NavigationType)2, 20f, 10f, false);
		TextObject val3 = new TextObject("{=SKC3FeGR}Sea Hounds", (Dictionary<string, object>)null);
		_pirateParty = CustomPartyComponent.CreateCustomPartyWithPartyTemplate(val2, 0.5f, SettlementHelper.FindRandomHideout((Func<Settlement, bool>)((Settlement t) => t.IsHideout)), val3, val, _pirateTemplate, NavalStorylineData.Purig, "", "", 0f, false);
		_pirateParty.Party.SetCustomName(val3);
		_pirateParty.InitializeMobilePartyAtPosition(val2);
		_pirateParty.SetLandNavigationAccess(false);
		_pirateParty.Party.SetVisualAsDirty();
		_pirateParty.ActualClan = val;
		_pirateParty.SetPartyUsedByQuest(true);
		_pirateParty.Party.SetCustomBanner(NavalStorylineData.CorsairBanner);
		PartyComponent partyComponent = _pirateParty.PartyComponent;
		((CustomPartyComponent)((partyComponent is CustomPartyComponent) ? partyComponent : null)).SetBaseSpeed(2.5f);
		_pirateParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
		_pirateParty.SetMoveEngageParty(MobileParty.MainParty, (NavigationType)2);
		_pirateParty.Ai.SetDoNotMakeNewDecisions(true);
		NavalDLCHelpers.AddUpgradePiecesToPartyShips(_pirateParty, PirateShipUpgradePieces);
		NavalDLCHelpers.SetCustomSailPatternOfPartyShips(_pirateParty, "generated_square_l1_h4_10");
	}

	private void RefreshPiratePartyForces()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		_pirateParty.MemberRoster.Clear();
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("sea_hounds");
		_pirateParty.AddElementToMemberRoster(val, PirateTroopCount * 2, false);
		foreach (Ship item in ((IEnumerable<Ship>)_pirateParty.Ships).ToList())
		{
			item.Owner = null;
		}
		using List<ShipTemplateStack>.Enumerator enumerator2 = ((List<ShipTemplateStack>)(object)_pirateTemplate.ShipHulls).GetEnumerator();
		while (enumerator2.MoveNext())
		{
			new Ship(enumerator2.Current.ShipHull)
			{
				Owner = _pirateParty.Party,
				IsInvulnerable = true
			};
		}
	}

	public bool IsPiratePartyVisible()
	{
		if (_pirateParty != null && _pirateParty.IsActive)
		{
			return _pirateParty.IsVisible;
		}
		return false;
	}
}
