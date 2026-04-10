using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace SandBox.Issues;

public class NotableWantsDaughterFoundIssueBehavior : CampaignBehaviorBase
{
	public class NotableWantsDaughterFoundIssueTypeDefiner : SaveableTypeDefiner
	{
		public NotableWantsDaughterFoundIssueTypeDefiner()
			: base(1088000)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(NotableWantsDaughterFoundIssue), 1, (IObjectResolver)null);
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(NotableWantsDaughterFoundIssueQuest), 2, (IObjectResolver)null);
		}
	}

	public class NotableWantsDaughterFoundIssue : IssueBase
	{
		private const int TroopTierForAlternativeSolution = 2;

		private const int RequiredSkillLevelForAlternativeSolution = 120;

		public override AlternativeSolutionScaleFlag AlternativeSolutionScaleFlags => (AlternativeSolutionScaleFlag)8;

		public override bool IsThereAlternativeSolution => true;

		public override bool IsThereLordSolution => false;

		protected override int RewardGold => 500 + MathF.Round(1200f * ((IssueBase)this).IssueDifficultyMultiplier);

		public override int AlternativeSolutionBaseNeededMenCount => 2 + MathF.Ceiling(4f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int AlternativeSolutionBaseDurationInDaysInternal => 4 + MathF.Ceiling(5f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int CompanionSkillRewardXP => (int)(500f + 1000f * ((IssueBase)this).IssueDifficultyMultiplier);

		public override TextObject IssueBriefByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=x9VgLEzi}Yes... I've suffered a great misfortune. [ib:demure][if:convo_shocked]My daughter, a headstrong girl, has been bewitched by this never-do-well. I told her to stop seeing him but she wouldn't listen! Now she's missing - I'm sure she's been abducted by him! I'm offering a bounty of {BASE_REWARD_GOLD}{GOLD_ICON} to anyone who brings her back. Please {?PLAYER.GENDER}ma'am{?}sir{\\?}! Don't let a father's heart be broken.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				val.SetTextVariable("BASE_REWARD_GOLD", ((IssueBase)this).RewardGold);
				return val;
			}
		}

		public override TextObject IssueAcceptByPlayer => new TextObject("{=35w6g8gM}Tell me more. What's wrong with the man? ", (Dictionary<string, object>)null);

		public override TextObject IssueQuestSolutionExplanationByIssueGiver => new TextObject("{=IY5b9vZV}Everything is wrong. [if:convo_annoyed]He is from a low family, the kind who is always involved in some land fraud scheme, or seen dealing with known bandits. Every village has a black sheep like that but I never imagined he would get his hooks into my daughter!", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				TextObject val = new TextObject("{=v0XsM7Zz}If you send your best tracker with a few men, I am sure they will find my girl [if:convo_pondering]and be back to you in no more than {ALTERNATIVE_SOLUTION_WAIT_DAYS} days.", (Dictionary<string, object>)null);
				val.SetTextVariable("ALTERNATIVE_SOLUTION_WAIT_DAYS", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				return val;
			}
		}

		public override TextObject IssuePlayerResponseAfterAlternativeExplanation => new TextObject("{=Ldp6ckgj}Don't worry, either I or one of my companions should be able to find her and see what's going on.", (Dictionary<string, object>)null);

		public override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=uYrxCtDa}I should be able to find her and see what's going on.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionAcceptByPlayer
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				TextObject val = new TextObject("{=WSrGHkal}I will have one of my trackers and {REQUIRED_TROOP_AMOUNT} of my men to find your daughter.", (Dictionary<string, object>)null);
				val.SetTextVariable("REQUIRED_TROOP_AMOUNT", ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount());
				return val;
			}
		}

		public override TextObject IssueDiscussAlternativeSolution => new TextObject("{=mBPcZddA}{?PLAYER.GENDER}Madam{?}Sir{\\?}, we are still waiting [ib:demure][if:convo_undecided_open]for your men to bring my daughter back. I pray for their success.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionResponseByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=Hhd3KaKu}Thank you, my {?PLAYER.GENDER}lady{?}lord{\\?}. If your men can find my girl and bring her back to me, I will be so grateful.[if:convo_happy] I will pay you {BASE_REWARD_GOLD}{GOLD_ICON} for your trouble.", (Dictionary<string, object>)null);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				val.SetTextVariable("BASE_REWARD_GOLD", ((IssueBase)this).RewardGold);
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
				return val;
			}
		}

		protected override TextObject AlternativeSolutionStartLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=6OmbzoBs}{ISSUE_GIVER.LINK}, a merchant from {ISSUE_GIVER_SETTLEMENT}, has told you that {?ISSUE_GIVER.GENDER}her{?}his{\\?} daughter has gone missing. You choose {COMPANION.LINK} and {REQUIRED_TROOP_AMOUNT} men to search for her and bring her back. You expect them to complete this task and return in {ALTERNATIVE_SOLUTION_DAYS} days.", (Dictionary<string, object>)null);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				val.SetTextVariable("BASE_REWARD_GOLD", ((IssueBase)this).RewardGold);
				val.SetTextVariable("ISSUE_GIVER_SETTLEMENT", ((IssueBase)this).IssueOwner.CurrentSettlement.Name);
				val.SetTextVariable("REQUIRED_TROOP_AMOUNT", base.AlternativeSolutionSentTroops.TotalManCount - 1);
				val.SetTextVariable("ALTERNATIVE_SOLUTION_DAYS", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("COMPANION", ((IssueBase)this).AlternativeSolutionHero.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject IssueAlternativeSolutionSuccessLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=MaXA5HJi}Your companions report that the {ISSUE_GIVER.LINK}'s daughter returns to {?ISSUE_GIVER.GENDER}her{?}him{\\?} safe and sound. {?ISSUE_GIVER.GENDER}She{?}He{\\?} is happy and sends {?ISSUE_GIVER.GENDER}her{?}his{\\?} regards with a large pouch of {BASE_REWARD_GOLD}{GOLD_ICON}.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				val.SetTextVariable("BASE_REWARD_GOLD", ((IssueBase)this).RewardGold);
				return val;
			}
		}

		public override TextObject Title
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=kr68V5pm}{ISSUE_GIVER.NAME} Wants {?ISSUE_GIVER.GENDER}Her{?}His{\\?} Daughter Found", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject Description
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=SkzM5eSv}{ISSUE_GIVER.LINK}'s daughter is missing. {?ISSUE_GIVER.GENDER}She{?}He{\\?} is offering a substantial reward to find the young woman and bring her back safely.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject IssueAsRumorInSettlement
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=7RyXSkEE}Wouldn't want to be the poor lovesick sap who ran off with {QUEST_GIVER.NAME}'s daughter.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		public NotableWantsDaughterFoundIssue(Hero issueOwner)
			: base(issueOwner, CampaignTime.DaysFromNow(30f))
		{
		}//IL_0007: Unknown result type (might be due to invalid IL or missing references)


		protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
		{
			if (issueEffect == DefaultIssueEffects.IssueOwnerPower)
			{
				return -0.1f;
			}
			return 0f;
		}

		protected override void AlternativeSolutionEndWithSuccessConsequence()
		{
			ApplySuccessRewards();
			float randomFloat = MBRandom.RandomFloat;
			SkillObject val = null;
			val = ((randomFloat <= 0.33f) ? DefaultSkills.OneHanded : ((!(randomFloat <= 0.66f)) ? DefaultSkills.Polearm : DefaultSkills.TwoHanded));
			((IssueBase)this).AlternativeSolutionHero.AddSkillXp(val, (float)(int)(500f + 1000f * ((IssueBase)this).IssueDifficultyMultiplier));
		}

		protected override void AlternativeSolutionEndWithFailureConsequence()
		{
			((IssueBase)this).RelationshipChangeWithIssueOwner = -10;
			if (((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound != null)
			{
				Town town = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
				town.Prosperity -= 5f;
				Town town2 = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
				town2.Security -= 5f;
			}
		}

		private void ApplySuccessRewards()
		{
			GainRenownAction.Apply(Hero.MainHero, 2f, false);
			((IssueBase)this).IssueOwner.AddPower(10f);
			((IssueBase)this).RelationshipChangeWithIssueOwner = 10;
			if (((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound != null)
			{
				Town town = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
				town.Security += 10f;
			}
		}

		protected override void OnGameLoad()
		{
		}

		protected override void HourlyTick()
		{
		}

		protected override QuestBase GenerateIssueQuest(string questId)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return (QuestBase)(object)new NotableWantsDaughterFoundIssueQuest(questId, ((IssueBase)this).IssueOwner, CampaignTime.DaysFromNow(19f), ((IssueBase)this).RewardGold, ((IssueBase)this).IssueDifficultyMultiplier);
		}

		public override IssueFrequency GetFrequency()
		{
			return (IssueFrequency)2;
		}

		public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero)
		{
			return ((hero.GetSkillValue(DefaultSkills.Charm) >= hero.GetSkillValue(DefaultSkills.Scouting)) ? DefaultSkills.Charm : DefaultSkills.Scouting, 120);
		}

		public override bool AlternativeSolutionCondition(out TextObject explanation)
		{
			return QuestHelper.CheckRosterForAlternativeSolution(MobileParty.MainParty.MemberRoster, ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount(), ref explanation, 2, false);
		}

		public override bool DoTroopsSatisfyAlternativeSolution(TroopRoster troopRoster, out TextObject explanation)
		{
			return QuestHelper.CheckRosterForAlternativeSolution(troopRoster, ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount(), ref explanation, 2, false);
		}

		public override bool IsTroopTypeNeededByAlternativeSolution(CharacterObject character)
		{
			return character.Tier >= 2;
		}

		protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill, out int requiredGold)
		{
			bool flag2 = issueGiver.GetRelationWithPlayer() >= -10f && !issueGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction);
			flag = (PreconditionFlags)((!flag2) ? ((!issueGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction)) ? 1 : 64) : 0);
			relationHero = issueGiver;
			skill = null;
			requiredGold = 0;
			return flag2;
		}

		public override bool IssueStayAliveConditions()
		{
			if (!((IssueBase)this).IssueOwner.CurrentSettlement.IsRaided)
			{
				return !((IssueBase)this).IssueOwner.CurrentSettlement.IsUnderRaid;
			}
			return false;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}

		internal static void AutoGeneratedStaticCollectObjectsNotableWantsDaughterFoundIssue(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(NotableWantsDaughterFoundIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((IssueBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}
	}

	public class NotableWantsDaughterFoundIssueQuest : QuestBase
	{
		[SaveableField(10)]
		private readonly Hero _daughterHero;

		[SaveableField(20)]
		private readonly Hero _rogueHero;

		private Agent _daughterAgent;

		private Agent _rogueAgent;

		[SaveableField(50)]
		private bool _isQuestTargetMission;

		[SaveableField(60)]
		private bool _didPlayerBeatRouge;

		[SaveableField(70)]
		private bool _exitedQuestSettlementForTheFirstTime = true;

		[SaveableField(80)]
		private bool _isTrackerLogAdded;

		[SaveableField(90)]
		private bool _isDaughterPersuaded;

		[SaveableField(91)]
		private bool _isDaughterCaptured;

		[SaveableField(100)]
		private bool _acceptedDaughtersEscape;

		[SaveableField(110)]
		private readonly Village _targetVillage;

		[SaveableField(120)]
		private bool _villageIsRaidedTalkWithDaughter;

		[SaveableField(140)]
		private Dictionary<Village, bool> _villagesAndAlreadyVisitedBooleans = new Dictionary<Village, bool>();

		private Dictionary<string, CharacterObject> _rogueCharacterBasedOnCulture = new Dictionary<string, CharacterObject>();

		private bool _playerDefeatedByRogue;

		private PersuasionTask _task;

		private const PersuasionDifficulty Difficulty = (PersuasionDifficulty)5;

		private const int MaxAgeForDaughterAndRogue = 25;

		[SaveableField(130)]
		private readonly float _questDifficultyMultiplier;

		public override TextObject Title
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=kr68V5pm}{ISSUE_GIVER.NAME} Wants {?ISSUE_GIVER.GENDER}Her{?}His{\\?} Daughter Found", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		public override bool IsRemainingTimeHidden => false;

		private bool DoesMainPartyHasEnoughScoutingSkill => (float)MobilePartyHelper.GetMainPartySkillCounsellor(DefaultSkills.Scouting).GetSkillValue(DefaultSkills.Scouting) >= 150f * _questDifficultyMultiplier;

		private TextObject PlayerStartsQuestLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Expected O, but got Unknown
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_0055: Unknown result type (might be due to invalid IL or missing references)
				//IL_0067: Unknown result type (might be due to invalid IL or missing references)
				//IL_0079: Expected O, but got Unknown
				TextObject val = new TextObject("{=1jExD58d}{QUEST_GIVER.LINK}, a merchant from {SETTLEMENT_NAME}, told you that {?QUEST_GIVER.GENDER}her{?}his{\\?} daughter {TARGET_HERO.NAME} has either been abducted or run off with a local rogue. You have agreed to search for her and bring her back to {SETTLEMENT_NAME}. If you cannot find their tracks when you exit settlement, you should visit the nearby villages of {SETTLEMENT_NAME} to look for clues and tracks of the kidnapper.", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val, "QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, false);
				TextObjectExtensions.SetCharacterProperties(val, "TARGET_HERO", _daughterHero.CharacterObject, false);
				val.SetTextVariable("SETTLEMENT_NAME", ((QuestBase)this).QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
				val.SetTextVariable("BASE_REWARD_GOLD", base.RewardGold);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				return val;
			}
		}

		private TextObject SuccessQuestLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Expected O, but got Unknown
				TextObject val = new TextObject("{=asVE53ac}Daughter returns to {QUEST_GIVER.LINK}. {?QUEST_GIVER.GENDER}She{?}He{\\?} is happy. Sends {?QUEST_GIVER.GENDER}her{?}his{\\?} regards with a large pouch of {BASE_REWARD}{GOLD_ICON}.", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val, "QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, false);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				val.SetTextVariable("BASE_REWARD", base.RewardGold);
				return val;
			}
		}

		private TextObject PlayerDefeatedByRogueLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=i1sth9Ls}You were defeated by the rogue. He and {TARGET_HERO.NAME} ran off while you were unconscious. You failed to bring the daughter back to her {?QUEST_GIVER.GENDER}mother{?}father{\\?} as promised to {QUEST_GIVER.LINK}. {?QUEST_GIVER.GENDER}She{?}He{\\?} is furious.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_HERO", _daughterHero.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject FailQuestLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0023: Expected O, but got Unknown
				TextObject val = new TextObject("{=ak2EMWWR}You failed to bring the daughter back to her {?QUEST_GIVER.GENDER}mother{?}father{\\?} as promised to {QUEST_GIVER.LINK}. {QUEST_GIVER.LINK} is furious", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val, "QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, false);
				return val;
			}
		}

		private TextObject QuestCanceledWarDeclaredLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0023: Expected O, but got Unknown
				TextObject val = new TextObject("{=vW6kBki9}Your clan is now at war with {QUEST_GIVER.LINK}'s realm. Your agreement with {QUEST_GIVER.LINK} is canceled.", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val, "QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, false);
				return val;
			}
		}

		private TextObject PlayerDeclaredWarQuestLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0023: Expected O, but got Unknown
				TextObject val = new TextObject("{=bqeWVVEE}Your actions have started a war with {QUEST_GIVER.LINK}'s faction. {?QUEST_GIVER.GENDER}She{?}He{\\?} cancels your agreement and the quest is a failure.", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val, "QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, false);
				return val;
			}
		}

		private TextObject VillageRaidedCancelQuestLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Expected O, but got Unknown
				TextObject val = new TextObject("{=aN85Kfnq}{SETTLEMENT} was raided. Your agreement with {QUEST_GIVER.LINK} is canceled.", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val, "QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, false);
				val.SetTextVariable("SETTLEMENT", ((QuestBase)this).QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		public NotableWantsDaughterFoundIssueQuest(string questId, Hero questGiver, CampaignTime duration, int baseReward, float issueDifficultyMultiplier)
			: base(questId, questGiver, duration, baseReward)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			_questDifficultyMultiplier = issueDifficultyMultiplier;
			_targetVillage = Extensions.GetRandomElementWithPredicate<Village>(questGiver.CurrentSettlement.Village.Bound.BoundVillages, (Func<Village, bool>)((Village x) => x != questGiver.CurrentSettlement.Village));
			Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture = _rogueCharacterBasedOnCulture;
			Clan? obj = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "steppe_bandits"));
			rogueCharacterBasedOnCulture.Add("khuzait", (obj != null) ? obj.Culture.BanditBoss : null);
			Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture2 = _rogueCharacterBasedOnCulture;
			Clan? obj2 = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "mountain_bandits"));
			rogueCharacterBasedOnCulture2.Add("vlandia", (obj2 != null) ? obj2.Culture.BanditBoss : null);
			Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture3 = _rogueCharacterBasedOnCulture;
			Clan? obj3 = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "desert_bandits"));
			rogueCharacterBasedOnCulture3.Add("aserai", (obj3 != null) ? obj3.Culture.BanditBoss : null);
			Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture4 = _rogueCharacterBasedOnCulture;
			Clan? obj4 = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "forest_bandits"));
			rogueCharacterBasedOnCulture4.Add("battania", (obj4 != null) ? obj4.Culture.BanditBoss : null);
			Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture5 = _rogueCharacterBasedOnCulture;
			Clan? obj5 = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "sea_raiders"));
			rogueCharacterBasedOnCulture5.Add("sturgia", (obj5 != null) ? obj5.Culture.BanditBoss : null);
			Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture6 = _rogueCharacterBasedOnCulture;
			Clan? obj6 = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "mountain_bandits"));
			rogueCharacterBasedOnCulture6.Add("empire_w", (obj6 != null) ? obj6.Culture.BanditBoss : null);
			Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture7 = _rogueCharacterBasedOnCulture;
			Clan? obj7 = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "mountain_bandits"));
			rogueCharacterBasedOnCulture7.Add("empire_s", (obj7 != null) ? obj7.Culture.BanditBoss : null);
			Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture8 = _rogueCharacterBasedOnCulture;
			Clan? obj8 = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "mountain_bandits"));
			rogueCharacterBasedOnCulture8.Add("empire", (obj8 != null) ? obj8.Culture.BanditBoss : null);
			Dictionary<string, CharacterObject> rogueCharacterBasedOnCulture9 = _rogueCharacterBasedOnCulture;
			Clan? obj9 = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "sea_raiders"));
			rogueCharacterBasedOnCulture9.Add("nord", (obj9 != null) ? obj9.Culture.BanditBoss : null);
			int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
			int num = MBRandom.RandomInt(heroComesOfAge, 25);
			int num2 = MBRandom.RandomInt(heroComesOfAge, 25);
			CharacterObject randomCompanionTemplateWithPredicate = CharacterHelper.GetRandomCompanionTemplateWithPredicate((Func<CharacterObject, bool>)((CharacterObject x) => ((BasicCharacterObject)x).IsFemale && x.Culture == questGiver.CurrentSettlement.Culture));
			_daughterHero = HeroCreator.CreateSpecialHero(randomCompanionTemplateWithPredicate, questGiver.HomeSettlement, questGiver.Clan, (Clan)null, num);
			_daughterHero.HiddenInEncyclopedia = true;
			_daughterHero.Father = questGiver;
			_rogueHero = HeroCreator.CreateSpecialHero(GetRogueCharacterBasedOnCulture(((MBObjectBase)questGiver.Culture).StringId), questGiver.HomeSettlement, questGiver.Clan, (Clan)null, num2);
			_rogueHero.Culture = questGiver.Culture;
			_rogueHero.HiddenInEncyclopedia = true;
			((QuestBase)this).SetDialogs();
			((QuestBase)this).InitializeQuestOnCreation();
		}

		private CharacterObject GetRogueCharacterBasedOnCulture(string cultureStrId)
		{
			if (_rogueCharacterBasedOnCulture.ContainsKey(cultureStrId))
			{
				return _rogueCharacterBasedOnCulture[cultureStrId];
			}
			return Extensions.GetRandomElementWithPredicate<CharacterObject>(((QuestBase)this).QuestGiver.CurrentSettlement.Culture.NotableTemplates, (Func<CharacterObject, bool>)((CharacterObject x) => (int)x.Occupation == 21 && !((BasicCharacterObject)x).IsFemale));
		}

		protected override void SetDialogs()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected O, but got Unknown
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Expected O, but got Unknown
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Expected O, but got Unknown
			TextObject val = new TextObject("{=PZq1EMcx}Thank you for your help. I am still very worried about my girl {TARGET_HERO.FIRSTNAME}. Please find her and bring her back to me as soon as you can.[if:convo_worried]", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("TARGET_HERO", _daughterHero.CharacterObject, val, false);
			TextObject val2 = new TextObject("{=sglD6abb}Please! Bring my daughter back.", (Dictionary<string, object>)null);
			TextObject val3 = new TextObject("{=ddEu5IFQ}I hope so.", (Dictionary<string, object>)null);
			TextObject val4 = new TextObject("{=IdKG3IaS}Good to hear that.", (Dictionary<string, object>)null);
			TextObject val5 = new TextObject("{=0hXofVLx}Don't worry I'll bring her.", (Dictionary<string, object>)null);
			TextObject val6 = new TextObject("{=zpqP5LsC}I'll go right away.", (Dictionary<string, object>)null);
			base.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver && !_didPlayerBeatRouge))
				.Consequence(new OnConsequenceDelegate(QuestAcceptedConsequences))
				.CloseDialog();
			base.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(val2, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver && !_didPlayerBeatRouge))
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(val5, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(val3, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.PlayerOption(val6, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(val4, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog();
			Campaign.Current.ConversationManager.AddDialogFlow(GetRougeDialogFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDaughterAfterFightDialog(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDaughterAfterAcceptDialog(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDaughterAfterPersuadedDialog(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDaughterDialogWhenVillageRaid(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetRougeAfterAcceptDialog(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetRogueAfterPersuadedDialog(), (object)this);
		}

		protected override void InitializeQuestOnGameLoad()
		{
			((QuestBase)this).SetDialogs();
			if (_daughterHero != null)
			{
				_daughterHero.HiddenInEncyclopedia = true;
			}
			if (_rogueHero != null)
			{
				_rogueHero.HiddenInEncyclopedia = true;
			}
		}

		protected override void HourlyTick()
		{
		}

		private bool IsRougeHero(IAgent agent)
		{
			return (object)agent.Character == _rogueHero.CharacterObject;
		}

		private bool IsDaughterHero(IAgent agent)
		{
			return (object)agent.Character == _daughterHero.CharacterObject;
		}

		private bool IsMainHero(IAgent agent)
		{
			return (object)agent.Character == CharacterObject.PlayerCharacter;
		}

		private bool multi_character_conversation_on_condition()
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			if (!_villageIsRaidedTalkWithDaughter && !_isDaughterPersuaded && !_didPlayerBeatRouge && !_acceptedDaughtersEscape && _isQuestTargetMission && (CharacterObject.OneToOneConversationCharacter == _daughterHero.CharacterObject || CharacterObject.OneToOneConversationCharacter == _rogueHero.CharacterObject))
			{
				MBList<Agent> val = new MBList<Agent>();
				Mission current = Mission.Current;
				Vec3 position = Agent.Main.Position;
				foreach (Agent item in (List<Agent>)(object)current.GetNearbyAgents(((Vec3)(ref position)).AsVec2, 100f, val))
				{
					if ((object)item.Character == _daughterHero.CharacterObject)
					{
						_daughterAgent = item;
						if (Mission.Current.GetMissionBehavior<MissionConversationLogic>() != null && Hero.OneToOneConversationHero != _daughterHero)
						{
							Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<Agent> { _daughterAgent }, true);
						}
					}
					else if ((object)item.Character == _rogueHero.CharacterObject)
					{
						_rogueAgent = item;
						if (Mission.Current.GetMissionBehavior<MissionConversationLogic>() != null && Hero.OneToOneConversationHero != _rogueHero)
						{
							Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<Agent> { _rogueAgent }, true);
						}
					}
				}
				if (_daughterAgent != null && _rogueAgent != null && _daughterAgent.Health > 10f)
				{
					return _rogueAgent.Health > 10f;
				}
				return false;
			}
			return false;
		}

		private bool daughter_conversation_after_fight_on_condition()
		{
			if (CharacterObject.OneToOneConversationCharacter == _daughterHero.CharacterObject)
			{
				return _didPlayerBeatRouge;
			}
			return false;
		}

		private void multi_agent_conversation_on_consequence()
		{
			_task = GetPersuasionTask();
		}

		private DialogFlow GetRougeDialogFlow()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Expected O, but got Unknown
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Expected O, but got Unknown
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Expected O, but got Unknown
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Expected O, but got Unknown
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Expected O, but got Unknown
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Expected O, but got Unknown
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Expected O, but got Unknown
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Expected O, but got Unknown
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Expected O, but got Unknown
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Expected O, but got Unknown
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Expected O, but got Unknown
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Expected O, but got Unknown
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Expected O, but got Unknown
			//IL_01d5: Expected O, but got Unknown
			//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Expected O, but got Unknown
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Expected O, but got Unknown
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Expected O, but got Unknown
			//IL_0218: Expected O, but got Unknown
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Expected O, but got Unknown
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Expected O, but got Unknown
			//IL_0254: Expected O, but got Unknown
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_0274: Expected O, but got Unknown
			//IL_0274: Expected O, but got Unknown
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0289: Unknown result type (might be due to invalid IL or missing references)
			//IL_0295: Expected O, but got Unknown
			//IL_0295: Expected O, but got Unknown
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a6: Expected O, but got Unknown
			//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c0: Expected O, but got Unknown
			//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e1: Expected O, but got Unknown
			//IL_02e1: Expected O, but got Unknown
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Expected O, but got Unknown
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_030b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0317: Expected O, but got Unknown
			//IL_0317: Expected O, but got Unknown
			//IL_034a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0356: Expected O, but got Unknown
			//IL_035e: Unknown result type (might be due to invalid IL or missing references)
			//IL_036a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0376: Expected O, but got Unknown
			//IL_0376: Expected O, but got Unknown
			//IL_0386: Unknown result type (might be due to invalid IL or missing references)
			//IL_0392: Expected O, but got Unknown
			//IL_0398: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b1: Expected O, but got Unknown
			//IL_03b1: Expected O, but got Unknown
			//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c2: Expected O, but got Unknown
			//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03dc: Expected O, but got Unknown
			//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fb: Expected O, but got Unknown
			//IL_03fb: Expected O, but got Unknown
			//IL_0402: Unknown result type (might be due to invalid IL or missing references)
			//IL_040c: Expected O, but got Unknown
			TextObject val = new TextObject("{=ovFbMMTJ}Who are you? Are you one of the bounty hunters sent by {QUEST_GIVER.LINK} to track us? Like we're animals or something? Look friend, we have done nothing wrong. As you may have figured out already, this woman and I, we love each other. I didn't force her to do anything.[ib:closed][if:convo_innocent_smile]", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
			TextObject val2 = new TextObject("{=D25oY3j1}Thank you {?PLAYER.GENDER}lady{?}sir{\\?}. For your kindness and understanding. We won't forget this.[ib:demure][if:convo_happy]", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val2, false);
			TextObject val3 = new TextObject("{=oL3amiu1}Come {DAUGHTER_NAME.NAME}, let's go before other hounds sniff our trail... I mean... No offense {?PLAYER.GENDER}madam{?}sir{\\?}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("DAUGHTER_NAME", _daughterHero.CharacterObject, val3, false);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val3, false);
			TextObject val4 = new TextObject("{=92sbq1YY}I'm no child, {?PLAYER.GENDER}lady{?}sir{\\?}! Draw your weapon! I challenge you to a duel![ib:warrior2][if:convo_excited]", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val4, false);
			TextObject val5 = new TextObject("{=jfzErupx}He is right! I ran away with him willingly. I love my {?QUEST_GIVER.GENDER}mother{?}father{\\?},[ib:closed][if:convo_grave] but {?QUEST_GIVER.GENDER}she{?}he{\\?} can be such a tyrant. Please {?PLAYER.GENDER}lady{?}sir{\\?}, if you believe in freedom and love, please leave us be.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val5, false);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val5, false);
			TextObject val6 = new TextObject("{=5NljlbLA}Thank you kind {?PLAYER.GENDER}lady{?}sir{\\?}, thank you.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val6, false);
			TextObject val7 = new TextObject("{=i5fNZrhh}Please, {?PLAYER.GENDER}lady{?}sir{\\?}. I love him truly and I wish to spend the rest of my life with him.[ib:demure][if:convo_worried] I beg of you, please don't stand in our way.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val7, false);
			TextObject val8 = new TextObject("{=0RCdPKj2}Yes {?QUEST_GIVER.GENDER}she{?}he{\\?} would probably be sad. But not because of what you think. See, {QUEST_GIVER.LINK} promised me to one of {?QUEST_GIVER.GENDER}her{?}his{\\?} allies' sons and this will devastate {?QUEST_GIVER.GENDER}her{?}his{\\?} plans. That is true.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val8, false);
			TextObject val9 = new TextObject("{=5W7Kxfq9}I understand. If that is the case, I will let you go.", (Dictionary<string, object>)null);
			TextObject val10 = new TextObject("{=3XimdHOn}How do I know he's not forcing you to say that?", (Dictionary<string, object>)null);
			TextObject val11 = new TextObject("{=zNqDEuAw}But I've promised to find you and return you to your {?QUEST_GIVER.GENDER}mother{?}father{\\?}. {?QUEST_GIVER.GENDER}She{?}He{\\?} would be devastated.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val11, false);
			TextObject val12 = new TextObject("{=tuaQ5uU3}I guess the only way to free you from this pretty boy's spell is to kill him.", (Dictionary<string, object>)null);
			TextObject val13 = new TextObject("{=HDCmeGhG}I'm sorry but I gave a promise. I don't break my promises.", (Dictionary<string, object>)null);
			TextObject val14 = new TextObject("{=VGrHWxzf}This will be a massacre, not a duel, but I'm fine with that.", (Dictionary<string, object>)null);
			TextObject val15 = new TextObject("{=sytYViXb}I accept your duel.", (Dictionary<string, object>)null);
			DialogFlow val16 = DialogFlow.CreateDialogFlow("start", 125).NpcLine(val, new OnMultipleConversationConsequenceDelegate(IsRougeHero), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null).Condition(new OnConditionDelegate(multi_character_conversation_on_condition))
				.Consequence(new OnConsequenceDelegate(multi_agent_conversation_on_consequence))
				.NpcLine(val5, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(val9, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), (string)null, (string)null)
				.NpcLine(val2, new OnMultipleConversationConsequenceDelegate(IsRougeHero), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.NpcLine(val3, new OnMultipleConversationConsequenceDelegate(IsRougeHero), new OnMultipleConversationConsequenceDelegate(IsDaughterHero), (string)null, (string)null)
				.NpcLine(val6, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += PlayerAcceptedDaughtersEscape;
				})
				.CloseDialog()
				.PlayerOption(val10, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), (string)null, (string)null)
				.NpcLine(val7, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.PlayerLine(val11, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), (string)null, (string)null)
				.NpcLine(val8, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.GotoDialogState("start_daughter_persuade_to_come_persuasion")
				.GoBackToDialogState("daughter_persuade_to_come_persuasion_finished")
				.PlayerLine((Hero.MainHero.GetTraitLevel(DefaultTraits.Mercy) < 0) ? val12 : val13, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), (string)null, (string)null)
				.NpcLine(val4, new OnMultipleConversationConsequenceDelegate(IsRougeHero), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(val14, new OnMultipleConversationConsequenceDelegate(IsRougeHero), (string)null, (string)null)
				.NpcLine(new TextObject("{=XWVW0oTB}You bastard![ib:aggressive][if:convo_furious]", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsRougeHero), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += PlayerRejectsDuelFight;
				})
				.CloseDialog()
				.PlayerOption(val15, new OnMultipleConversationConsequenceDelegate(IsRougeHero), (string)null, (string)null)
				.NpcLine(new TextObject("{=jqahxjWD}Heaven protect me![ib:aggressive][if:convo_astonished]", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsRougeHero), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += PlayerAcceptsDuelFight;
				})
				.CloseDialog()
				.EndPlayerOptions()
				.EndPlayerOptions()
				.CloseDialog();
			AddPersuasionDialogs(val16);
			return val16;
		}

		private DialogFlow GetDaughterAfterFightDialog()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected O, but got Unknown
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Expected O, but got Unknown
			TextObject val = new TextObject("{=MN2v1AZQ}I hate you! You killed him! I can't believe it! I will hate you with all my heart till my dying days.[if:convo_angry]", (Dictionary<string, object>)null);
			TextObject val2 = new TextObject("{=TTkVcObg}What choice do I have, you heartless bastard?![if:convo_furious]", (Dictionary<string, object>)null);
			TextObject val3 = new TextObject("{=XqsrsjiL}I did what I had to do. Pack up, you need to go.", (Dictionary<string, object>)null);
			TextObject val4 = new TextObject("{=KQ3aYvp3}Some day you'll see I did you a favor. Pack up, you need to go.", (Dictionary<string, object>)null);
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(daughter_conversation_after_fight_on_condition))
				.PlayerLine((Hero.MainHero.GetTraitLevel(DefaultTraits.Mercy) < 0) ? val3 : val4, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(val2, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += PlayerWonTheFight;
				})
				.CloseDialog();
		}

		private DialogFlow GetDaughterAfterAcceptDialog()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			TextObject val = new TextObject("{=0Wg00sfN}Thank you, {?PLAYER.GENDER}madam{?}sir{\\?}. We will be moving immediately.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
			TextObject val2 = new TextObject("{=kUReBc04}Good.", (Dictionary<string, object>)null);
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(daughter_conversation_after_accept_on_condition))
				.PlayerLine(val2, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog();
		}

		private bool daughter_conversation_after_accept_on_condition()
		{
			if (CharacterObject.OneToOneConversationCharacter == _daughterHero.CharacterObject)
			{
				return _acceptedDaughtersEscape;
			}
			return false;
		}

		private DialogFlow GetDaughterAfterPersuadedDialog()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			TextObject val = new TextObject("{=B8bHpJRP}You are right, {?PLAYER.GENDER}my lady{?}sir{\\?}. I should be moving immediately.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
			TextObject val2 = new TextObject("{=kUReBc04}Good.", (Dictionary<string, object>)null);
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(daughter_conversation_after_persuaded_on_condition))
				.PlayerLine(val2, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog();
		}

		private DialogFlow GetDaughterDialogWhenVillageRaid()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=w0HPC53e}Who are you? What do you want from me?[ib:nervous][if:convo_bared_teeth]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _villageIsRaidedTalkWithDaughter))
				.PlayerLine(new TextObject("{=iRupMGI0}Calm down! Your father has sent me to find you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=dwNquUNr}My father? Oh, thank god! I saw terrible things. [ib:nervous2][if:convo_shocked]They took my beloved one and slew many innocents without hesitation.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.PlayerLine("{=HtAr22re}Try to forget all about these and return to your father's house.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=FgSIsasF}Yes, you are right. I shall be on my way...", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
					{
						ApplyDeliverySuccessConsequences();
						((QuestBase)this).CompleteQuestWithSuccess();
						((QuestBase)this).AddLog(SuccessQuestLogText, false);
						_villageIsRaidedTalkWithDaughter = false;
					};
				})
				.CloseDialog();
		}

		private bool daughter_conversation_after_persuaded_on_condition()
		{
			if (CharacterObject.OneToOneConversationCharacter == _daughterHero.CharacterObject)
			{
				return _isDaughterPersuaded;
			}
			return false;
		}

		private DialogFlow GetRougeAfterAcceptDialog()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			TextObject val = new TextObject("{=wlKtDR2z}Thank you, {?PLAYER.GENDER}my lady{?}sir{\\?}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(rogue_conversation_after_accept_on_condition))
				.PlayerLine(new TextObject("{=0YJGvJ7o}You should leave now.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=6Q4cPOSG}Yes, we will.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog();
		}

		private bool rogue_conversation_after_accept_on_condition()
		{
			if (CharacterObject.OneToOneConversationCharacter == _rogueHero.CharacterObject)
			{
				return _acceptedDaughtersEscape;
			}
			return false;
		}

		private DialogFlow GetRogueAfterPersuadedDialog()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected O, but got Unknown
			TextObject val = new TextObject("{=GFt9KiHP}You are right. Maybe we need to persuade {QUEST_GIVER.NAME}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
			TextObject val2 = new TextObject("{=btJkBTSF}I am sure you can solve it.", (Dictionary<string, object>)null);
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(rogue_conversation_after_persuaded_on_condition))
				.PlayerLine(val2, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog();
		}

		private bool rogue_conversation_after_persuaded_on_condition()
		{
			if (CharacterObject.OneToOneConversationCharacter == _rogueHero.CharacterObject)
			{
				return _isDaughterPersuaded;
			}
			return false;
		}

		protected override void OnTimedOut()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			ApplyDeliveryRejectedFailConsequences();
			TextObject val = new TextObject("{=KAvwytDK}You didn't bring {DAUGHTER.NAME} to {QUEST_GIVER.LINK}. {?QUEST_GIVER.GENDER}she{?}he{\\?} must be furious.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
			StringHelpers.SetCharacterProperties("DAUGHTER", _daughterHero.CharacterObject, val, false);
			((QuestBase)this).AddLog(val, false);
		}

		private void PlayerAcceptedDaughtersEscape()
		{
			_acceptedDaughtersEscape = true;
		}

		private void PlayerWonTheFight()
		{
			_isDaughterCaptured = true;
			Mission.Current.SetMissionMode((MissionMode)0, false);
		}

		private void ApplyDaughtersEscapeAcceptedFailConsequences()
		{
			((QuestBase)this).RelationshipChangeWithQuestGiver = -10;
			if (((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound != null)
			{
				Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town.Security -= 5f;
				Town town2 = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town2.Prosperity -= 5f;
			}
		}

		private void ApplyDeliveryRejectedFailConsequences()
		{
			((QuestBase)this).RelationshipChangeWithQuestGiver = -10;
			if (((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound != null)
			{
				Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town.Security -= 5f;
				Town town2 = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town2.Prosperity -= 5f;
			}
		}

		private void ApplyDeliverySuccessConsequences()
		{
			GainRenownAction.Apply(Hero.MainHero, 2f, false);
			((QuestBase)this).QuestGiver.AddPower(10f);
			((QuestBase)this).RelationshipChangeWithQuestGiver = 10;
			if (((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound != null)
			{
				Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town.Security += 10f;
			}
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, base.RewardGold, false);
		}

		private void PlayerRejectsDuelFight()
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			_rogueAgent = (Agent)MissionConversationLogic.Current.ConversationManager.ConversationAgents.First((IAgent x) => !x.Character.IsFemale);
			List<Agent> list = new List<Agent> { Agent.Main };
			List<Agent> opponentSideAgents = new List<Agent> { _rogueAgent };
			MBList<Agent> val = new MBList<Agent>();
			Mission current = Mission.Current;
			Vec3 position = Agent.Main.Position;
			foreach (Agent item in (List<Agent>)(object)current.GetNearbyAgents(((Vec3)(ref position)).AsVec2, 30f, val))
			{
				foreach (Hero item2 in Hero.MainHero.CompanionsInParty)
				{
					if ((object)item.Character == item2.CharacterObject)
					{
						list.Add(item);
						break;
					}
				}
			}
			_rogueAgent.Health = 150 + list.Count * 20;
			_rogueAgent.Defensiveness = 1f;
			Mission.Current.GetMissionBehavior<MissionFightHandler>().StartCustomFight(list, opponentSideAgents, dropWeapons: false, isItemUseDisabled: false, StartConversationAfterFight);
		}

		private void PlayerAcceptsDuelFight()
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			_rogueAgent = (Agent)MissionConversationLogic.Current.ConversationManager.ConversationAgents.First((IAgent x) => !x.Character.IsFemale);
			List<Agent> playerSideAgents = new List<Agent> { Agent.Main };
			List<Agent> opponentSideAgents = new List<Agent> { _rogueAgent };
			MBList<Agent> val = new MBList<Agent>();
			Mission current = Mission.Current;
			Vec3 position = Agent.Main.Position;
			foreach (Agent item in (List<Agent>)(object)current.GetNearbyAgents(((Vec3)(ref position)).AsVec2, 30f, val))
			{
				foreach (Hero item2 in Hero.MainHero.CompanionsInParty)
				{
					if ((object)item.Character == item2.CharacterObject)
					{
						item.SetTeam(Mission.Current.SpectatorTeam, false);
						DailyBehaviorGroup behaviorGroup = item.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
						if (behaviorGroup.GetActiveBehavior() is FollowAgentBehavior)
						{
							behaviorGroup.GetBehavior<FollowAgentBehavior>().SetTargetAgent(null);
						}
						break;
					}
				}
			}
			_rogueAgent.Health = 200f;
			Mission.Current.GetMissionBehavior<MissionFightHandler>().StartCustomFight(playerSideAgents, opponentSideAgents, dropWeapons: false, isItemUseDisabled: false, StartConversationAfterFight);
		}

		private void StartConversationAfterFight(bool isPlayerSideWon)
		{
			if (isPlayerSideWon)
			{
				_didPlayerBeatRouge = true;
				Campaign.Current.ConversationManager.SetupAndStartMissionConversation((IAgent)(object)_daughterAgent, (IAgent)(object)Mission.Current.MainAgent, false);
				TraitLevelingHelper.OnHostileAction(-50);
			}
			else
			{
				_playerDefeatedByRogue = true;
			}
		}

		private void AddPersuasionDialogs(DialogFlow dialog)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected O, but got Unknown
			//IL_0086: Expected O, but got Unknown
			//IL_0086: Expected O, but got Unknown
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Expected O, but got Unknown
			//IL_00d5: Expected O, but got Unknown
			//IL_00d5: Expected O, but got Unknown
			//IL_00d5: Expected O, but got Unknown
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Expected O, but got Unknown
			//IL_013d: Expected O, but got Unknown
			//IL_013d: Expected O, but got Unknown
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Expected O, but got Unknown
			//IL_018f: Expected O, but got Unknown
			//IL_018f: Expected O, but got Unknown
			//IL_018f: Expected O, but got Unknown
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Expected O, but got Unknown
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Expected O, but got Unknown
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Expected O, but got Unknown
			//IL_01f9: Expected O, but got Unknown
			//IL_01f9: Expected O, but got Unknown
			//IL_01f9: Expected O, but got Unknown
			//IL_0216: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Expected O, but got Unknown
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Expected O, but got Unknown
			//IL_024d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Expected O, but got Unknown
			//IL_0263: Expected O, but got Unknown
			//IL_0263: Expected O, but got Unknown
			//IL_0263: Expected O, but got Unknown
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0298: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Expected O, but got Unknown
			//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Expected O, but got Unknown
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cd: Expected O, but got Unknown
			//IL_02cd: Expected O, but got Unknown
			//IL_02cd: Expected O, but got Unknown
			//IL_02cd: Expected O, but got Unknown
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Expected O, but got Unknown
			//IL_030f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0315: Expected O, but got Unknown
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0337: Expected O, but got Unknown
			//IL_0337: Expected O, but got Unknown
			//IL_0337: Expected O, but got Unknown
			//IL_0337: Expected O, but got Unknown
			//IL_0354: Unknown result type (might be due to invalid IL or missing references)
			//IL_0360: Unknown result type (might be due to invalid IL or missing references)
			//IL_0370: Expected O, but got Unknown
			//IL_0370: Expected O, but got Unknown
			TextObject val = new TextObject("{=ob5SejgJ}I will not abandon my love, {?PLAYER.GENDER}lady{?}sir{\\?}!", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
			TextObject val2 = new TextObject("{=cqe8FU8M}{?QUEST_GIVER.GENDER}She{?}He{\\?} cares nothing about me! Only about {?QUEST_GIVER.GENDER}her{?}his{\\?} reputation in our district.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val2, false);
			dialog.AddDialogLine("daughter_persuade_to_come_introduction", "start_daughter_persuade_to_come_persuasion", "daughter_persuade_to_come_start_reservation", ((object)val2).ToString(), (OnConditionDelegate)null, new OnConsequenceDelegate(persuasion_start_with_daughter_on_consequence), (object)this, 100, (OnClickableConditionDelegate)null, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), new OnMultipleConversationConsequenceDelegate(IsMainHero));
			dialog.AddDialogLine("daughter_persuade_to_come_rejected", "daughter_persuade_to_come_start_reservation", "daughter_persuade_to_come_persuasion_failed", "{=!}{FAILED_PERSUASION_LINE}", new OnConditionDelegate(daughter_persuade_to_come_persuasion_failed_on_condition), new OnConsequenceDelegate(daughter_persuade_to_come_persuasion_failed_on_consequence), (object)this, 100, (OnClickableConditionDelegate)null, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), new OnMultipleConversationConsequenceDelegate(IsMainHero));
			dialog.AddDialogLine("daughter_persuade_to_come_failed", "daughter_persuade_to_come_persuasion_failed", "daughter_persuade_to_come_persuasion_finished", ((object)val).ToString(), (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			dialog.AddDialogLine("daughter_persuade_to_come_start", "daughter_persuade_to_come_start_reservation", "daughter_persuade_to_come_persuasion_select_option", "{=9b2BETct}I have already decided. Don't expect me to change my mind.", (OnConditionDelegate)(() => !daughter_persuade_to_come_persuasion_failed_on_condition()), (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), new OnMultipleConversationConsequenceDelegate(IsMainHero));
			dialog.AddDialogLine("daughter_persuade_to_come_success", "daughter_persuade_to_come_start_reservation", "close_window", "{=3tmXBpRH}You're right. I cannot do this. I will return to my family. ", new OnConditionDelegate(ConversationManager.GetPersuasionProgressSatisfied), new OnConsequenceDelegate(daughter_persuade_to_come_persuasion_success_on_consequence), (object)this, int.MaxValue, (OnClickableConditionDelegate)null, new OnMultipleConversationConsequenceDelegate(IsDaughterHero), new OnMultipleConversationConsequenceDelegate(IsMainHero));
			OnConditionDelegate val3 = persuasion_select_option_1_on_condition;
			OnConsequenceDelegate val4 = persuasion_select_option_1_on_consequence;
			OnPersuasionOptionDelegate val5 = new OnPersuasionOptionDelegate(persuasion_setup_option_1);
			OnClickableConditionDelegate val6 = new OnClickableConditionDelegate(persuasion_clickable_option_1_on_condition);
			dialog.AddPlayerLine("daughter_persuade_to_come_select_option_1", "daughter_persuade_to_come_persuasion_select_option", "daughter_persuade_to_come_persuasion_selected_option_response", "{=!}{DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_1}", val3, val4, (object)this, 100, val6, val5, new OnMultipleConversationConsequenceDelegate(IsMainHero), new OnMultipleConversationConsequenceDelegate(IsDaughterHero));
			OnConditionDelegate val7 = persuasion_select_option_2_on_condition;
			OnConsequenceDelegate val8 = persuasion_select_option_2_on_consequence;
			val5 = new OnPersuasionOptionDelegate(persuasion_setup_option_2);
			val6 = new OnClickableConditionDelegate(persuasion_clickable_option_2_on_condition);
			dialog.AddPlayerLine("daughter_persuade_to_come_select_option_2", "daughter_persuade_to_come_persuasion_select_option", "daughter_persuade_to_come_persuasion_selected_option_response", "{=!}{DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_2}", val7, val8, (object)this, 100, val6, val5, new OnMultipleConversationConsequenceDelegate(IsMainHero), new OnMultipleConversationConsequenceDelegate(IsDaughterHero));
			OnConditionDelegate val9 = persuasion_select_option_3_on_condition;
			OnConsequenceDelegate val10 = persuasion_select_option_3_on_consequence;
			val5 = new OnPersuasionOptionDelegate(persuasion_setup_option_3);
			val6 = new OnClickableConditionDelegate(persuasion_clickable_option_3_on_condition);
			dialog.AddPlayerLine("daughter_persuade_to_come_select_option_3", "daughter_persuade_to_come_persuasion_select_option", "daughter_persuade_to_come_persuasion_selected_option_response", "{=!}{DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_3}", val9, val10, (object)this, 100, val6, val5, new OnMultipleConversationConsequenceDelegate(IsMainHero), new OnMultipleConversationConsequenceDelegate(IsDaughterHero));
			OnConditionDelegate val11 = persuasion_select_option_4_on_condition;
			OnConsequenceDelegate val12 = persuasion_select_option_4_on_consequence;
			val5 = new OnPersuasionOptionDelegate(persuasion_setup_option_4);
			val6 = new OnClickableConditionDelegate(persuasion_clickable_option_4_on_condition);
			dialog.AddPlayerLine("daughter_persuade_to_come_select_option_4", "daughter_persuade_to_come_persuasion_select_option", "daughter_persuade_to_come_persuasion_selected_option_response", "{=!}{DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_4}", val11, val12, (object)this, 100, val6, val5, new OnMultipleConversationConsequenceDelegate(IsMainHero), new OnMultipleConversationConsequenceDelegate(IsDaughterHero));
			dialog.AddDialogLine("daughter_persuade_to_come_select_option_reaction", "daughter_persuade_to_come_persuasion_selected_option_response", "daughter_persuade_to_come_start_reservation", "{=D0xDRqvm}{PERSUASION_REACTION}", new OnConditionDelegate(persuasion_selected_option_response_on_condition), new OnConsequenceDelegate(persuasion_selected_option_response_on_consequence), (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		}

		private void persuasion_selected_option_response_on_consequence()
		{
			Tuple<PersuasionOptionArgs, PersuasionOptionResult> tuple = ConversationManager.GetPersuasionChosenOptions().Last();
			float difficulty = Campaign.Current.Models.PersuasionModel.GetDifficulty((PersuasionDifficulty)5);
			float num = default(float);
			float num2 = default(float);
			Campaign.Current.Models.PersuasionModel.GetEffectChances(tuple.Item1, ref num, ref num2, difficulty);
			_task.ApplyEffects(num, num2);
		}

		private bool persuasion_selected_option_response_on_condition()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			PersuasionOptionResult item = ConversationManager.GetPersuasionChosenOptions().Last().Item2;
			MBTextManager.SetTextVariable("PERSUASION_REACTION", PersuasionHelper.GetDefaultPersuasionOptionReaction(item), false);
			return true;
		}

		private bool persuasion_select_option_1_on_condition()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 0)
			{
				TextObject val = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", (Dictionary<string, object>)null);
				val.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(0), false));
				val.SetTextVariable("PERSUASION_OPTION_LINE", ((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(0).Line);
				MBTextManager.SetTextVariable("DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_1", val, false);
				return true;
			}
			return false;
		}

		private bool persuasion_select_option_2_on_condition()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 1)
			{
				TextObject val = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", (Dictionary<string, object>)null);
				val.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(1), false));
				val.SetTextVariable("PERSUASION_OPTION_LINE", ((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(1).Line);
				MBTextManager.SetTextVariable("DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_2", val, false);
				return true;
			}
			return false;
		}

		private bool persuasion_select_option_3_on_condition()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 2)
			{
				TextObject val = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", (Dictionary<string, object>)null);
				val.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(2), false));
				val.SetTextVariable("PERSUASION_OPTION_LINE", ((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(2).Line);
				MBTextManager.SetTextVariable("DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_3", val, false);
				return true;
			}
			return false;
		}

		private bool persuasion_select_option_4_on_condition()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 3)
			{
				TextObject val = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", (Dictionary<string, object>)null);
				val.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(3), false));
				val.SetTextVariable("PERSUASION_OPTION_LINE", ((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(3).Line);
				MBTextManager.SetTextVariable("DAUGHTER_PERSUADE_TO_COME_PERSUADE_ATTEMPT_4", val, false);
				return true;
			}
			return false;
		}

		private void persuasion_select_option_1_on_consequence()
		{
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 0)
			{
				((List<PersuasionOptionArgs>)(object)_task.Options)[0].BlockTheOption(true);
			}
		}

		private void persuasion_select_option_2_on_consequence()
		{
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 1)
			{
				((List<PersuasionOptionArgs>)(object)_task.Options)[1].BlockTheOption(true);
			}
		}

		private void persuasion_select_option_3_on_consequence()
		{
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 2)
			{
				((List<PersuasionOptionArgs>)(object)_task.Options)[2].BlockTheOption(true);
			}
		}

		private void persuasion_select_option_4_on_consequence()
		{
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 3)
			{
				((List<PersuasionOptionArgs>)(object)_task.Options)[3].BlockTheOption(true);
			}
		}

		private PersuasionOptionArgs persuasion_setup_option_1()
		{
			return ((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(0);
		}

		private PersuasionOptionArgs persuasion_setup_option_2()
		{
			return ((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(1);
		}

		private PersuasionOptionArgs persuasion_setup_option_3()
		{
			return ((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(2);
		}

		private PersuasionOptionArgs persuasion_setup_option_4()
		{
			return ((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(3);
		}

		private bool persuasion_clickable_option_1_on_condition(out TextObject hintText)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			hintText = new TextObject("{=9ACJsI6S}Blocked", (Dictionary<string, object>)null);
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 0)
			{
				hintText = (((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(0).IsBlocked ? hintText : null);
				return !((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(0).IsBlocked;
			}
			return false;
		}

		private bool persuasion_clickable_option_2_on_condition(out TextObject hintText)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			hintText = new TextObject("{=9ACJsI6S}Blocked", (Dictionary<string, object>)null);
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 1)
			{
				hintText = (((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(1).IsBlocked ? hintText : null);
				return !((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(1).IsBlocked;
			}
			return false;
		}

		private bool persuasion_clickable_option_3_on_condition(out TextObject hintText)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			hintText = new TextObject("{=9ACJsI6S}Blocked", (Dictionary<string, object>)null);
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 2)
			{
				hintText = (((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(2).IsBlocked ? hintText : null);
				return !((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(2).IsBlocked;
			}
			return false;
		}

		private bool persuasion_clickable_option_4_on_condition(out TextObject hintText)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			hintText = new TextObject("{=9ACJsI6S}Blocked", (Dictionary<string, object>)null);
			if (((List<PersuasionOptionArgs>)(object)_task.Options).Count > 3)
			{
				hintText = (((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(3).IsBlocked ? hintText : null);
				return !((IEnumerable<PersuasionOptionArgs>)_task.Options).ElementAt(3).IsBlocked;
			}
			return false;
		}

		private PersuasionTask GetPersuasionTask()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Expected O, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected O, but got Unknown
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Expected O, but got Unknown
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Expected O, but got Unknown
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Expected O, but got Unknown
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Expected O, but got Unknown
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Expected O, but got Unknown
			PersuasionTask val = new PersuasionTask(0)
			{
				FinalFailLine = new TextObject("{=5aDlmdmb}No... No. It does not make sense.", (Dictionary<string, object>)null),
				TryLaterLine = TextObject.GetEmpty(),
				SpokenLine = new TextObject("{=6P1ruzsC}Maybe...", (Dictionary<string, object>)null)
			};
			PersuasionOptionArgs val2 = new PersuasionOptionArgs(DefaultSkills.Leadership, DefaultTraits.Honor, (TraitEffect)0, (PersuasionArgumentStrength)(-1), true, new TextObject("{=Nhfl6tcM}Maybe, but that is your duty to your family.", (Dictionary<string, object>)null), (Tuple<TraitObject, int>[])null, false, false, false);
			val.AddOptionToTask(val2);
			TextObject val3 = new TextObject("{=lustkZ7s}Perhaps {?QUEST_GIVER.GENDER}she{?}he{\\?} made those plans because {?QUEST_GIVER.GENDER}she{?}he{\\?} loves you.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val3, false);
			PersuasionOptionArgs val4 = new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Mercy, (TraitEffect)0, (PersuasionArgumentStrength)2, false, val3, (Tuple<TraitObject, int>[])null, false, false, false);
			val.AddOptionToTask(val4);
			PersuasionOptionArgs val5 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Calculating, (TraitEffect)0, (PersuasionArgumentStrength)(-2), false, new TextObject("{=Ns6Svjsn}Do you think this one will be faithful to you over many years? I know a rogue when I see one.", (Dictionary<string, object>)null), (Tuple<TraitObject, int>[])null, false, false, false);
			val.AddOptionToTask(val5);
			PersuasionOptionArgs val6 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Mercy, (TraitEffect)1, (PersuasionArgumentStrength)(-3), true, new TextObject("{=2dL6j8Hp}You want to marry a corpse? Because I'm going to kill your lover if you don't listen.", (Dictionary<string, object>)null), (Tuple<TraitObject, int>[])null, true, false, false);
			val.AddOptionToTask(val6);
			return val;
		}

		private void persuasion_start_with_daughter_on_consequence()
		{
			ConversationManager.StartPersuasion(2f, 1f, 0f, 2f, 2f, 0f, (PersuasionDifficulty)5);
		}

		private void daughter_persuade_to_come_persuasion_success_on_consequence()
		{
			ConversationManager.EndPersuasion();
			_isDaughterPersuaded = true;
		}

		private bool daughter_persuade_to_come_persuasion_failed_on_condition()
		{
			if (((IEnumerable<PersuasionOptionArgs>)_task.Options).All((PersuasionOptionArgs x) => x.IsBlocked) && !ConversationManager.GetPersuasionProgressSatisfied())
			{
				MBTextManager.SetTextVariable("FAILED_PERSUASION_LINE", _task.FinalFailLine, false);
				return true;
			}
			return false;
		}

		private void daughter_persuade_to_come_persuasion_failed_on_consequence()
		{
			ConversationManager.EndPersuasion();
		}

		private void OnSettlementLeft(MobileParty party, Settlement settlement)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected O, but got Unknown
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Expected O, but got Unknown
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Expected O, but got Unknown
			if (party.IsMainParty && settlement == ((QuestBase)this).QuestGiver.CurrentSettlement && _exitedQuestSettlementForTheFirstTime)
			{
				if (DoesMainPartyHasEnoughScoutingSkill)
				{
					QuestHelper.AddMapArrowFromPointToTarget(new TextObject("{=YdwLnWa1}Direction of daughter and rogue", (Dictionary<string, object>)null), settlement.Position, ((SettlementComponent)_targetVillage).Settlement.Position, 5f, 0.1f);
					MBInformationManager.AddQuickInformation(new TextObject("{=O15PyNUK}With the help of your scouting skill, you were able to trace their tracks.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					MBInformationManager.AddQuickInformation(new TextObject("{=gOWebWiK}Their direction is marked with an arrow in the campaign map.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)((SettlementComponent)_targetVillage).Settlement);
				}
				else
				{
					foreach (Village item in (List<Village>)(object)((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.BoundVillages)
					{
						if (item != ((QuestBase)this).QuestGiver.CurrentSettlement.Village)
						{
							_villagesAndAlreadyVisitedBooleans.Add(item, value: false);
							((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)((SettlementComponent)item).Settlement);
						}
					}
				}
				TextObject val = new TextObject("{=FvtAJE2Q}In order to find {QUEST_GIVER.LINK}'s daughter, you have decided to visit nearby villages.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				((QuestBase)this).AddLog(val, DoesMainPartyHasEnoughScoutingSkill);
				_exitedQuestSettlementForTheFirstTime = false;
			}
			if (party.IsMainParty && settlement == ((SettlementComponent)_targetVillage).Settlement)
			{
				_isQuestTargetMission = false;
			}
		}

		public void OnBeforeMissionOpened()
		{
			if (_isQuestTargetMission)
			{
				Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("village_center");
				if (locationWithId != null)
				{
					HandleRogueEquipment();
					locationWithId.AddCharacter(CreateQuestLocationCharacter(_daughterHero.CharacterObject, (CharacterRelations)0));
					locationWithId.AddCharacter(CreateQuestLocationCharacter(_rogueHero.CharacterObject, (CharacterRelations)0));
				}
			}
		}

		private void HandleRogueEquipment()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Invalid comparison between Unknown and I4
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>("short_sword_t3");
			_rogueHero.CivilianEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)0, new EquipmentElement(val, (ItemModifier)null, (ItemObject)null, false));
			for (EquipmentIndex val2 = (EquipmentIndex)0; (int)val2 < 5; val2 = (EquipmentIndex)(val2 + 1))
			{
				EquipmentElement val3 = _rogueHero.BattleEquipment[val2];
				ItemObject item = ((EquipmentElement)(ref val3)).Item;
				if (item != null && item.WeaponComponent.PrimaryWeapon.IsShield)
				{
					Equipment battleEquipment = _rogueHero.BattleEquipment;
					EquipmentIndex val4 = val2;
					val3 = default(EquipmentElement);
					battleEquipment.AddEquipmentToSlotWithoutAgent(val4, val3);
				}
			}
		}

		private void OnMissionEnded(IMission mission)
		{
			if (_isQuestTargetMission)
			{
				_daughterAgent = null;
				_rogueAgent = null;
				if (_isDaughterPersuaded)
				{
					ApplyDeliverySuccessConsequences();
					((QuestBase)this).CompleteQuestWithSuccess();
					((QuestBase)this).AddLog(SuccessQuestLogText, false);
					RemoveQuestCharacters();
				}
				else if (_acceptedDaughtersEscape)
				{
					ApplyDaughtersEscapeAcceptedFailConsequences();
					((QuestBase)this).CompleteQuestWithFail(FailQuestLogText);
					RemoveQuestCharacters();
				}
				else if (_isDaughterCaptured)
				{
					ApplyDeliverySuccessConsequences();
					((QuestBase)this).CompleteQuestWithSuccess();
					((QuestBase)this).AddLog(SuccessQuestLogText, false);
					RemoveQuestCharacters();
				}
				else if (_playerDefeatedByRogue)
				{
					ApplyDeliveryFailedDueToDuelLostConsequences();
					((QuestBase)this).CompleteQuestWithFail((TextObject)null);
					((QuestBase)this).AddLog(PlayerDefeatedByRogueLogText, false);
					RemoveQuestCharacters();
				}
			}
		}

		private void ApplyDeliveryFailedDueToDuelLostConsequences()
		{
			ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero.MainHero, _daughterHero, -5, true);
			((QuestBase)this).RelationshipChangeWithQuestGiver = -10;
			if (((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound != null)
			{
				Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town.Security -= 5f;
				Town town2 = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town2.Prosperity -= 5f;
			}
		}

		private LocationCharacter CreateQuestLocationCharacter(CharacterObject character, CharacterRelations relation)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected O, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Expected O, but got Unknown
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)character).Race, "_settlement");
			Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, ((BasicCharacterObject)character).IsFemale, "_villager"), monsterWithSuffix);
			AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)character, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2);
			IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
			return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddCompanionBehaviors), "alley_2", true, relation, tuple.Item1, false, false, (ItemObject)null, false, true, true, (AfterAgentCreatedDelegate)null, false);
		}

		private void RemoveQuestCharacters()
		{
			Settlement.CurrentSettlement.LocationComplex.RemoveCharacterIfExists(_daughterHero);
			Settlement.CurrentSettlement.LocationComplex.RemoveCharacterIfExists(_rogueHero);
		}

		private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
		{
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Expected O, but got Unknown
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Expected O, but got Unknown
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Expected O, but got Unknown
			//IL_0247: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Expected O, but got Unknown
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Expected O, but got Unknown
			//IL_0291: Unknown result type (might be due to invalid IL or missing references)
			//IL_029b: Expected O, but got Unknown
			//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bb: Expected O, but got Unknown
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Expected O, but got Unknown
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Expected O, but got Unknown
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected O, but got Unknown
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Expected O, but got Unknown
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Expected O, but got Unknown
			if (party == null || !party.IsMainParty || !settlement.IsVillage)
			{
				return;
			}
			if (_villagesAndAlreadyVisitedBooleans.ContainsKey(settlement.Village) && !_villagesAndAlreadyVisitedBooleans[settlement.Village])
			{
				if (settlement.Village != _targetVillage)
				{
					if (settlement.IsSettlementBusy((object)this))
					{
						TextObject val = (settlement.IsRaided ? new TextObject("{=YTaM6G1E}It seems the village has been raided a short while ago. You found nothing but smoke, fire and crying people.", (Dictionary<string, object>)null) : new TextObject("{=2P3UJ8be}You ask around the village if anyone saw {TARGET_HERO.NAME} or some suspicious characters with a young woman.{newline}{newline}Villagers say that they saw a young man and woman ride in early in the morning. They bought some supplies and trotted off towards {TARGET_VILLAGE}.", (Dictionary<string, object>)null));
						val.SetTextVariable("TARGET_VILLAGE", ((SettlementComponent)_targetVillage).Name);
						StringHelpers.SetCharacterProperties("TARGET_HERO", _daughterHero.CharacterObject, val, false);
						InformationManager.ShowInquiry(new InquiryData(((object)((QuestBase)this).Title).ToString(), ((object)val).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), "", (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
					}
					if (!_isTrackerLogAdded)
					{
						TextObject val2 = new TextObject("{=WGi3Zuv7}You asked the villagers around {CURRENT_SETTLEMENT} if they saw a young woman matching the description of {QUEST_GIVER.LINK}'s daughter, {TARGET_HERO.NAME}.{newline}{newline}They said a young woman and a young man dropped by early in the morning to buy some supplies and then rode off towards {TARGET_VILLAGE}.", (Dictionary<string, object>)null);
						val2.SetTextVariable("CURRENT_SETTLEMENT", Hero.MainHero.CurrentSettlement.Name);
						val2.SetTextVariable("TARGET_VILLAGE", ((SettlementComponent)_targetVillage).Settlement.EncyclopediaLinkWithName);
						StringHelpers.SetCharacterProperties("TARGET_HERO", _daughterHero.CharacterObject, val2, false);
						StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val2, false);
						((QuestBase)this).AddLog(val2, false);
						_isTrackerLogAdded = true;
					}
				}
				else
				{
					InquiryData val3 = null;
					if (settlement.IsRaided)
					{
						TextObject val4 = new TextObject("{=edoXFdmg}You have found {QUEST_GIVER.NAME}'s daughter.", (Dictionary<string, object>)null);
						StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val4, false);
						TextObject val5 = new TextObject("{=aYMW8bWi}Talk to her", (Dictionary<string, object>)null);
						val3 = new InquiryData(((object)((QuestBase)this).Title).ToString(), ((object)val4).ToString(), true, false, ((object)val5).ToString(), (string)null, (Action)TalkWithDaughterAfterRaid, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
					}
					else if (settlement.IsSettlementBusy((object)this))
					{
						TextObject val6 = new TextObject("{=aj4DZYyX}You ask around the village if anyone saw {TARGET_HERO.NAME} or some suspicious characters with a young woman. Villagers say that there was a young man and woman who arrived here exhausted. The villagers allowed them to stay for a while. You should search the village to find her.", (Dictionary<string, object>)null);
						StringHelpers.SetCharacterProperties("TARGET_HERO", _daughterHero.CharacterObject, val6, false);
						((QuestBase)this).AddLog(val6, false);
					}
					else
					{
						TextObject val7 = new TextObject("{=bbwNIIKI}You ask around the village if anyone saw {TARGET_HERO.NAME} or some suspicious characters with a young woman.{newline}{newline}Villagers say that there was a young man and woman who arrived here exhausted. The villagers allowed them to stay for a while.{newline}You can check the area, and see if they are still hiding here.", (Dictionary<string, object>)null);
						StringHelpers.SetCharacterProperties("TARGET_HERO", _daughterHero.CharacterObject, val7, false);
						val3 = new InquiryData(((object)((QuestBase)this).Title).ToString(), ((object)val7).ToString(), true, true, ((object)new TextObject("{=bb6e8DoM}Search the village", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString(), (Action)SearchTheVillage, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
					}
					if (val3 != null)
					{
						InformationManager.ShowInquiry(val3, false, false);
					}
				}
				_villagesAndAlreadyVisitedBooleans[settlement.Village] = true;
			}
			if (settlement == ((SettlementComponent)_targetVillage).Settlement)
			{
				if (!((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)_daughterHero))
				{
					((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_daughterHero);
				}
				if (!((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)_rogueHero))
				{
					((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_rogueHero);
				}
				_isQuestTargetMission = true;
			}
		}

		private void SearchTheVillage()
		{
			LocationEncounter locationEncounter = PlayerEncounter.LocationEncounter;
			LocationEncounter obj = ((locationEncounter is VillageEncounter) ? locationEncounter : null);
			if (obj != null)
			{
				obj.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("village_center"), (Location)null, (CharacterObject)null, (string)null);
			}
		}

		private void TalkWithDaughterAfterRaid()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			_villageIsRaidedTalkWithDaughter = true;
			CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, false, false, false, false, false, false), new ConversationCharacterData(_daughterHero.CharacterObject, (PartyBase)null, false, false, false, false, false, false));
		}

		private void QuestAcceptedConsequences()
		{
			((QuestBase)this).StartQuest();
			((QuestBase)this).AddLog(PlayerStartsQuestLogText, false);
		}

		private void CanHeroDie(Hero victim, KillCharacterActionDetail detail, ref bool result)
		{
			if (victim == Hero.MainHero && Settlement.CurrentSettlement == ((SettlementComponent)_targetVillage).Settlement && Mission.Current != null)
			{
				result = false;
			}
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
			CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
			CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener((object)this, (Action)OnBeforeMissionOpened);
			CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
			CampaignEvents.WarDeclared.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, DeclareWarDetail>)OnWarDeclared);
			CampaignEvents.CanHeroDieEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, KillCharacterActionDetail, bool>)CanHeroDie);
			CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
			CampaignEvents.MapEventStarted.AddNonSerializedListener((object)this, (Action<MapEvent, PartyBase, PartyBase>)OnMapEventStarted);
			CampaignEvents.RaidCompletedEvent.AddNonSerializedListener((object)this, (Action<BattleSideEnum, RaidEventComponent>)OnRaidCompleted);
		}

		private void OnRaidCompleted(BattleSideEnum side, RaidEventComponent raidEventComponent)
		{
			if (raidEventComponent.MapEventSettlement == ((QuestBase)this).QuestGiver.CurrentSettlement)
			{
				((QuestBase)this).CompleteQuestWithCancel(VillageRaidedCancelQuestLogText);
			}
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _rogueHero || hero == _daughterHero)
			{
				result = false;
			}
		}

		public override void OnHeroCanMoveToSettlementInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _rogueHero || hero == _daughterHero)
			{
				result = false;
			}
		}

		private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
		{
			if (QuestHelper.CheckMinorMajorCoercion((QuestBase)(object)this, mapEvent, attackerParty))
			{
				QuestHelper.ApplyGenericMinorMajorCoercionConsequences((QuestBase)(object)this, mapEvent);
			}
		}

		private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
		{
			if (((QuestBase)this).QuestGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				((QuestBase)this).CompleteQuestWithCancel(QuestCanceledWarDeclaredLog);
			}
		}

		private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarDetail detail)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest((QuestBase)(object)this, faction1, faction2, detail, PlayerDeclaredWarQuestLogText, QuestCanceledWarDeclaredLog, false);
		}

		protected override void OnFinalize()
		{
			if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)((SettlementComponent)_targetVillage).Settlement))
			{
				((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)((SettlementComponent)_targetVillage).Settlement);
			}
			if (!Hero.MainHero.IsPrisoner && !DoesMainPartyHasEnoughScoutingSkill)
			{
				foreach (Village item in (List<Village>)(object)((QuestBase)this).QuestGiver.CurrentSettlement.BoundVillages)
				{
					if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)((SettlementComponent)item).Settlement))
					{
						((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)((SettlementComponent)item).Settlement);
					}
				}
			}
			if (_rogueHero != null && _rogueHero.IsAlive)
			{
				KillCharacterAction.ApplyByRemove(_rogueHero, false, true);
			}
			if (_daughterHero != null && _daughterHero.IsAlive)
			{
				KillCharacterAction.ApplyByRemove(_daughterHero, false, true);
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsNotableWantsDaughterFoundIssueQuest(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(NotableWantsDaughterFoundIssueQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_daughterHero);
			collectedObjects.Add(_rogueHero);
			collectedObjects.Add(_targetVillage);
			collectedObjects.Add(_villagesAndAlreadyVisitedBooleans);
		}

		internal static object AutoGeneratedGetMemberValue_daughterHero(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._daughterHero;
		}

		internal static object AutoGeneratedGetMemberValue_rogueHero(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._rogueHero;
		}

		internal static object AutoGeneratedGetMemberValue_isQuestTargetMission(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._isQuestTargetMission;
		}

		internal static object AutoGeneratedGetMemberValue_didPlayerBeatRouge(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._didPlayerBeatRouge;
		}

		internal static object AutoGeneratedGetMemberValue_exitedQuestSettlementForTheFirstTime(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._exitedQuestSettlementForTheFirstTime;
		}

		internal static object AutoGeneratedGetMemberValue_isTrackerLogAdded(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._isTrackerLogAdded;
		}

		internal static object AutoGeneratedGetMemberValue_isDaughterPersuaded(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._isDaughterPersuaded;
		}

		internal static object AutoGeneratedGetMemberValue_isDaughterCaptured(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._isDaughterCaptured;
		}

		internal static object AutoGeneratedGetMemberValue_acceptedDaughtersEscape(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._acceptedDaughtersEscape;
		}

		internal static object AutoGeneratedGetMemberValue_targetVillage(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._targetVillage;
		}

		internal static object AutoGeneratedGetMemberValue_villageIsRaidedTalkWithDaughter(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._villageIsRaidedTalkWithDaughter;
		}

		internal static object AutoGeneratedGetMemberValue_villagesAndAlreadyVisitedBooleans(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._villagesAndAlreadyVisitedBooleans;
		}

		internal static object AutoGeneratedGetMemberValue_questDifficultyMultiplier(object o)
		{
			return ((NotableWantsDaughterFoundIssueQuest)o)._questDifficultyMultiplier;
		}
	}

	private const IssueFrequency NotableWantsDaughterFoundIssueFrequency = (IssueFrequency)2;

	private const int IssueDuration = 30;

	private const int QuestTimeLimit = 19;

	private const int BaseRewardGold = 500;

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnCheckForIssue);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
	}

	private void OnGameLoadFinished()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!MBSaveLoad.IsUpdatingGameVersion)
		{
			return;
		}
		ApplicationVersion lastLoadedGameVersion = MBSaveLoad.LastLoadedGameVersion;
		if (!((ApplicationVersion)(ref lastLoadedGameVersion)).IsOlderThan(ApplicationVersion.FromString("v1.2.8.31599", 0)))
		{
			return;
		}
		foreach (Hero item in (List<Hero>)(object)Hero.DeadOrDisabledHeroes)
		{
			if (item.IsDead && item.CompanionOf == Clan.PlayerClan && item.Father != null && item.Father.IsNotable && item.Father.CurrentSettlement.IsVillage)
			{
				RemoveCompanionAction.ApplyByDeath(Clan.PlayerClan, item);
			}
		}
	}

	public void OnCheckForIssue(Hero hero)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (ConditionsHold(hero))
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new StartIssueDelegate(OnStartIssue), typeof(NotableWantsDaughterFoundIssue), (IssueFrequency)2, (object)null));
		}
		else
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(NotableWantsDaughterFoundIssue), (IssueFrequency)2));
		}
	}

	private bool ConditionsHold(Hero issueGiver)
	{
		if (issueGiver.IsRuralNotable && issueGiver.CurrentSettlement.IsVillage && issueGiver.CurrentSettlement.Village.Bound != null && ((List<Village>)(object)issueGiver.CurrentSettlement.Village.Bound.BoundVillages).Count > 2 && issueGiver.CanHaveCampaignIssues() && issueGiver.Age > (float)(Campaign.Current.Models.AgeModel.HeroComesOfAge * 2) && CharacterHelper.GetRandomCompanionTemplateWithPredicate((Func<CharacterObject, bool>)((CharacterObject x) => ((BasicCharacterObject)x).IsFemale && x.Culture == issueGiver.CurrentSettlement.Culture)) != null && ((IEnumerable<CharacterObject>)issueGiver.CurrentSettlement.Culture.NotableTemplates).Any((CharacterObject x) => (int)x.Occupation == 21 && !((BasicCharacterObject)x).IsFemale) && issueGiver.GetTraitLevel(DefaultTraits.Mercy) <= 0)
		{
			return issueGiver.GetTraitLevel(DefaultTraits.Generosity) <= 0;
		}
		return false;
	}

	private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
	{
		return (IssueBase)(object)new NotableWantsDaughterFoundIssue(issueOwner);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
