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
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace SandBox.Issues;

public class ProdigalSonIssueBehavior : CampaignBehaviorBase
{
	public class ProdigalSonIssueTypeDefiner : SaveableTypeDefiner
	{
		public ProdigalSonIssueTypeDefiner()
			: base(345000)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(ProdigalSonIssue), 1, (IObjectResolver)null);
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(ProdigalSonIssueQuest), 2, (IObjectResolver)null);
		}
	}

	public class ProdigalSonIssue : IssueBase
	{
		private const int IssueDurationInDays = 50;

		private const int QuestDurationInDays = 24;

		private const int TroopTierForAlternativeSolution = 2;

		private const int RequiredSkillValueForAlternativeSolution = 120;

		[SaveableField(10)]
		private readonly Hero _prodigalSon;

		[SaveableField(20)]
		private readonly Hero _targetHero;

		[SaveableField(30)]
		private readonly Location _targetHouse;

		private Settlement _targetSettlement;

		public override AlternativeSolutionScaleFlag AlternativeSolutionScaleFlags => (AlternativeSolutionScaleFlag)8;

		private Clan Clan => ((IssueBase)this).IssueOwner.Clan;

		protected override int RewardGold => 1200 + (int)(3000f * ((IssueBase)this).IssueDifficultyMultiplier);

		public override TextObject IssueBriefByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=5a6KlSXt}I have a problem. [ib:normal2][if:convo_pondering]My young kinsman {PRODIGAL_SON.LINK} has gone to town to have fun, drinking, wenching and gambling. Many young men do that, but it seems he was a bit reckless. Now he sends news that he owes a large sum of money to {TARGET_HERO.LINK}, one of the local gang bosses in the city of {SETTLEMENT_LINK}. These ruffians are holding him as a “guest” in their house until someone pays his debt.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("PRODIGAL_SON", _prodigalSon.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_HERO", _targetHero.CharacterObject, val, false);
				val.SetTextVariable("SETTLEMENT_LINK", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		public override TextObject IssueAcceptByPlayer => new TextObject("{=YtS3cgto}What are you planning to do?", (Dictionary<string, object>)null);

		public override TextObject IssueQuestSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Expected O, but got Unknown
				TextObject val = new TextObject("{=ZC1slXw1}I'm not inclined to pay the debt. [ib:closed][if:convo_worried]I'm not going to reward this kind of lawlessness, when even the best families aren't safe. I've sent word to the lord of {SETTLEMENT_NAME} but I can't say I expect to hear back, what with the wars and all. I want someone to go there and free the lad. You could pay, I suppose, but I'd prefer it if you taught those bastards a lesson. I'll pay you either way but obviously you get to keep more if you use force.", (Dictionary<string, object>)null);
				val.SetTextVariable("SETTLEMENT_NAME", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		public override TextObject IssuePlayerResponseAfterAlternativeExplanation => new TextObject("{=4zf1lg6L}I could go myself, or send a companion.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Expected O, but got Unknown
				TextObject val = new TextObject("{=CWbAoGRu}Yes, I don't care how you solve it. [if:convo_normal]Just solve it any way you like. I reckon {NEEDED_MEN_COUNT} led by someone who knows how to handle thugs could solve this in about {ALTERNATIVE_SOLUTION_DURATION} days. I'd send my own men but it could cause complications for us to go marching in wearing our clan colors in another lord's territory.", (Dictionary<string, object>)null);
				val.SetTextVariable("NEEDED_MEN_COUNT", ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount());
				val.SetTextVariable("ALTERNATIVE_SOLUTION_DURATION", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				return val;
			}
		}

		public override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=aKbyJsho}I will free your kinsman myself.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionAcceptByPlayer
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Expected O, but got Unknown
				TextObject val = new TextObject("{=PuuVGOyM}I will send {NEEDED_MEN_COUNT} of my men with one of my lieutenants for {ALTERNATIVE_SOLUTION_DURATION} days to help you.", (Dictionary<string, object>)null);
				val.SetTextVariable("NEEDED_MEN_COUNT", ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount());
				val.SetTextVariable("ALTERNATIVE_SOLUTION_DURATION", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				return val;
			}
		}

		public override TextObject IssueDiscussAlternativeSolution => new TextObject("{=qxhMagyZ}I'm glad someone's on it.[if:convo_relaxed_happy] Just see that they do it quickly.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionResponseByIssueGiver => new TextObject("{=mDXzDXKY}Very good. [if:convo_relaxed_happy]I'm sure you'll chose competent men to bring our boy back.", (Dictionary<string, object>)null);

		protected override TextObject AlternativeSolutionStartLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=Z9sp21rl}{QUEST_GIVER.LINK}, a lord from the {QUEST_GIVER_CLAN} clan, asked you to free {?QUEST_GIVER.GENDER}her{?}his{\\?} relative. The young man is currently held by {TARGET_HERO.LINK} a local gang leader because of his debts. {?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} {QUEST_GIVER.LINK} has given you enough gold to settle {?QUEST_GIVER.GENDER}her{?}his{\\?} debts but {?QUEST_GIVER.GENDER}she{?}he{\\?} encourages you to keep the money to yourself and make an example of these criminals so no one would dare to hold a nobleman again. You have sent {COMPANION.LINK} and {NEEDED_MEN_COUNT} men to take care of the situation for you. They should be back in {ALTERNATIVE_SOLUTION_DURATION} days.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_HERO", _targetHero.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("COMPANION", ((IssueBase)this).AlternativeSolutionHero.CharacterObject, val, false);
				val.SetTextVariable("QUEST_GIVER_CLAN", ((IssueBase)this).IssueOwner.Clan.EncyclopediaLinkWithName);
				val.SetTextVariable("SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				val.SetTextVariable("NEEDED_MEN_COUNT", ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount());
				val.SetTextVariable("ALTERNATIVE_SOLUTION_DURATION", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				return val;
			}
		}

		public override TextObject IssueAlternativeSolutionSuccessLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=IXnvQ8kG}{COMPANION.LINK} and the men you sent with {?COMPANION.GENDER}her{?}him{\\?} safely return with the news of success. {QUEST_GIVER.LINK} is happy and sends you {?QUEST_GIVER.GENDER}her{?}his{\\?} regards with {REWARD}{GOLD_ICON} the money he promised.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("COMPANION", ((IssueBase)this).AlternativeSolutionHero.CharacterObject, val, false);
				val.SetTextVariable("REWARD", ((IssueBase)this).RewardGold);
				return val;
			}
		}

		public override bool IsThereAlternativeSolution => true;

		public override int AlternativeSolutionBaseNeededMenCount => 1 + MathF.Ceiling(3f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int AlternativeSolutionBaseDurationInDaysInternal => 7 + MathF.Ceiling(7f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int CompanionSkillRewardXP => (int)(700f + 900f * ((IssueBase)this).IssueDifficultyMultiplier);

		public override bool IsThereLordSolution => false;

		public override TextObject Title
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Expected O, but got Unknown
				TextObject val = new TextObject("{=Mr2rt8g8}Prodigal Son of {CLAN_NAME}", (Dictionary<string, object>)null);
				val.SetTextVariable("CLAN_NAME", Clan.Name);
				return val;
			}
		}

		public override TextObject Description
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=5puy0Jle}{ISSUE_OWNER.NAME} asks the player to aid a young clan member. He is supposed to have huge gambling debts so the gang leaders holds him as a hostage. You are asked to retrieve him any way possible.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_OWNER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		public ProdigalSonIssue(Hero issueOwner, Hero prodigalSon, Hero targetGangHero)
			: base(issueOwner, CampaignTime.DaysFromNow(50f))
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected O, but got Unknown
			_prodigalSon = prodigalSon;
			_targetHero = targetGangHero;
			_targetSettlement = _targetHero.CurrentSettlement;
			_targetHouse = _targetSettlement.LocationComplex.GetListOfLocations().FirstOrDefault((Func<Location, bool>)((Location x) => x.CanBeReserved && !x.IsReserved));
			TextObject val = new TextObject("{=EZ19JOGj}{MENTOR.NAME}'s House", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", _targetHero.CharacterObject, val, false);
			_targetHouse.ReserveLocation(val, val);
			DisableHeroAction.Apply(_prodigalSon);
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _targetHero || hero == _prodigalSon)
			{
				result = false;
			}
		}

		protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
		{
			if (issueEffect == DefaultIssueEffects.IssueOwnerPower)
			{
				return -0.2f;
			}
			return 0f;
		}

		public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero)
		{
			return ((hero.GetSkillValue(DefaultSkills.Charm) >= hero.GetSkillValue(DefaultSkills.Roguery)) ? DefaultSkills.Charm : DefaultSkills.Roguery, 120);
		}

		protected override void OnGameLoad()
		{
			Town val = ((IEnumerable<Town>)Town.AllTowns).FirstOrDefault((Func<Town, bool>)((Town x) => ((SettlementComponent)x).Settlement.LocationComplex.GetListOfLocations().Contains(_targetHouse)));
			if (val != null)
			{
				_targetSettlement = ((SettlementComponent)val).Settlement;
			}
		}

		protected override void HourlyTick()
		{
		}

		protected override QuestBase GenerateIssueQuest(string questId)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			return (QuestBase)(object)new ProdigalSonIssueQuest(questId, ((IssueBase)this).IssueOwner, _targetHero, _prodigalSon, _targetHouse, ((IssueBase)this).IssueDifficultyMultiplier, CampaignTime.DaysFromNow(24f), ((IssueBase)this).RewardGold);
		}

		public override IssueFrequency GetFrequency()
		{
			return (IssueFrequency)2;
		}

		protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill, out int requiredGold)
		{
			bool flag2 = issueGiver.GetRelationWithPlayer() >= -10f && !issueGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction) && Clan.PlayerClan.Tier >= 1;
			flag = (PreconditionFlags)((!flag2) ? (issueGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction) ? 64 : ((Clan.PlayerClan.Tier >= 1) ? 1 : 128)) : 0);
			relationHero = issueGiver;
			skill = null;
			requiredGold = 0;
			return flag2;
		}

		public override bool IssueStayAliveConditions()
		{
			return _targetHero.IsActive;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}

		public override bool DoTroopsSatisfyAlternativeSolution(TroopRoster troopRoster, out TextObject explanation)
		{
			return QuestHelper.CheckRosterForAlternativeSolution(troopRoster, ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount(), ref explanation, 2, false);
		}

		public override bool IsTroopTypeNeededByAlternativeSolution(CharacterObject character)
		{
			return character.Tier >= 2;
		}

		public override bool AlternativeSolutionCondition(out TextObject explanation)
		{
			return QuestHelper.CheckRosterForAlternativeSolution(MobileParty.MainParty.MemberRoster, ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount(), ref explanation, 2, false);
		}

		protected override void AlternativeSolutionEndWithSuccessConsequence()
		{
			((IssueBase)this).AlternativeSolutionHero.AddSkillXp(DefaultSkills.Charm, (float)(int)(700f + 900f * ((IssueBase)this).IssueDifficultyMultiplier));
			((IssueBase)this).RelationshipChangeWithIssueOwner = 5;
			GainRenownAction.Apply(Hero.MainHero, 3f, false);
		}

		protected override void AlternativeSolutionEndWithFailureConsequence()
		{
			((IssueBase)this).RelationshipChangeWithIssueOwner = -5;
		}

		protected override void OnIssueFinalized()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			if ((int)_prodigalSon.HeroState == 6)
			{
				_prodigalSon.ChangeState((CharacterStates)4);
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsProdigalSonIssue(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(ProdigalSonIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((IssueBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_prodigalSon);
			collectedObjects.Add(_targetHero);
			collectedObjects.Add(_targetHouse);
		}

		internal static object AutoGeneratedGetMemberValue_prodigalSon(object o)
		{
			return ((ProdigalSonIssue)o)._prodigalSon;
		}

		internal static object AutoGeneratedGetMemberValue_targetHero(object o)
		{
			return ((ProdigalSonIssue)o)._targetHero;
		}

		internal static object AutoGeneratedGetMemberValue_targetHouse(object o)
		{
			return ((ProdigalSonIssue)o)._targetHouse;
		}
	}

	public class ProdigalSonIssueQuest : QuestBase
	{
		private const PersuasionDifficulty Difficulty = (PersuasionDifficulty)5;

		private const int DistanceSquaredToStartConversation = 4;

		private const int CrimeRatingCancelRelationshipPenalty = -5;

		private const int CrimeRatingCancelHonorXpPenalty = -50;

		[SaveableField(10)]
		private readonly Hero _targetHero;

		[SaveableField(20)]
		private readonly Hero _prodigalSon;

		[SaveableField(30)]
		private bool _playerTalkedToTargetHero;

		[SaveableField(40)]
		private readonly Location _targetHouse;

		[SaveableField(50)]
		private readonly float _questDifficulty;

		[SaveableField(60)]
		private bool _isHouseFightFinished;

		[SaveableField(70)]
		private bool _playerTriedToPersuade;

		private PersuasionTask _task;

		private bool _isMissionFightInitialized;

		private bool _isFirstMissionTick;

		public override TextObject Title
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Expected O, but got Unknown
				TextObject val = new TextObject("{=Mr2rt8g8}Prodigal Son of {CLAN_NAME}", (Dictionary<string, object>)null);
				val.SetTextVariable("CLAN_NAME", ((QuestBase)this).QuestGiver.Clan.Name);
				return val;
			}
		}

		public override bool IsRemainingTimeHidden => false;

		private Settlement Settlement => _targetHero.CurrentSettlement;

		private int DebtWithInterest => (int)((float)base.RewardGold * 1.1f);

		private TextObject QuestStartedLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=CXw9a1i5}{QUEST_GIVER.LINK}, a {?QUEST_GIVER.GENDER}lady{?}lord{\\?} from the {QUEST_GIVER_CLAN} clan, asked you to go to {SETTLEMENT} to free {?QUEST_GIVER.GENDER}her{?}his{\\?} relative. The young man is currently held by {TARGET_HERO.LINK}, a local gang leader, because of his debts. {QUEST_GIVER.LINK} has suggested that you make an example of the gang so no one would dare to hold a nobleman again. {?QUEST_GIVER.GENDER}She{?}He{\\?} said you can easily find the house in which the young nobleman is held in the town square.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_HERO", _targetHero.CharacterObject, val, false);
				val.SetTextVariable("QUEST_GIVER_CLAN", ((QuestBase)this).QuestGiver.Clan.EncyclopediaLinkWithName);
				val.SetTextVariable("SETTLEMENT", Settlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject PlayerDefeatsThugsQuestSuccessLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=axLR9bQo}You have defeated the thugs that held {PRODIGAL_SON.LINK} as {QUEST_GIVER.LINK} has asked you to. {?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} {QUEST_GIVER.LINK} soon sends {?QUEST_GIVER.GENDER}her{?}his{\\?} best regards and a sum of {REWARD}{GOLD_ICON} as a reward.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("PRODIGAL_SON", _prodigalSon.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("REWARD", base.RewardGold);
				return val;
			}
		}

		private TextObject PlayerPaysTheDebtQuestSuccessLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=skMoB7c6}You have paid the debt that {PRODIGAL_SON.LINK} owes. True to {?TARGET_HERO.GENDER}her{?}his{\\?} word {TARGET_HERO.LINK} releases the boy immediately. Soon after, {?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} {QUEST_GIVER.LINK} sends {?QUEST_GIVER.GENDER}her{?}his{\\?} best regards and a sum of {REWARD}{GOLD_ICON} as a reward.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("PRODIGAL_SON", _prodigalSon.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_HERO", _targetHero.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("REWARD", base.RewardGold);
				return val;
			}
		}

		private TextObject QuestTimeOutFailLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=dmijPqWn}You have failed to extract {QUEST_GIVER.LINK}'s relative captive in time. They have moved the boy to a more secure place. Its impossible to find him now. {QUEST_GIVER.LINK} will have to deal with {TARGET_HERO.LINK} himself now. {?QUEST_GIVER.GENDER}She{?}He{\\?} won't be happy to hear this.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_HERO", _targetHero.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject PlayerHasDefeatedQuestFailLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=d5a8xQos}You have failed to defeat the thugs that keep {QUEST_GIVER.LINK}'s relative captive. After your assault you learn that they move the boy to a more secure place. Now its impossible to find him. {QUEST_GIVER.LINK} will have to deal with {TARGET_HERO.LINK} himself now. {?QUEST_GIVER.GENDER}She{?}He{\\?} won't be happy to hear this.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_HERO", _targetHero.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject PlayerConvincesGangLeaderQuestSuccessLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=Rb7g1U2s}You have convinced {TARGET_HERO.LINK} to release {PRODIGAL_SON.LINK}. Soon after, {?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} {QUEST_GIVER.LINK} sends {?QUEST_GIVER.GENDER}her{?}his{\\?} best regards and a sum of {REWARD}{GOLD_ICON} as a reward.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("PRODIGAL_SON", _prodigalSon.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_HERO", _targetHero.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("REWARD", base.RewardGold);
				return val;
			}
		}

		private TextObject WarDeclaredQuestCancelLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=VuqZuSe2}Your clan is now at war with the {QUEST_GIVER.LINK}'s faction. Your agreement has been canceled.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject PlayerDeclaredWarQuestLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=bqeWVVEE}Your actions have started a war with {QUEST_GIVER.LINK}'s faction. {?QUEST_GIVER.GENDER}She{?}He{\\?} cancels your agreement and the quest is a failure.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject CrimeRatingCancelLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Expected O, but got Unknown
				TextObject val = new TextObject("{=oulvvl52}You are accused in {SETTLEMENT} of a crime, and {QUEST_GIVER.LINK} no longer trusts you in this matter.", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val, "QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, false);
				val.SetTextVariable("SETTLEMENT", Settlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		public ProdigalSonIssueQuest(string questId, Hero questGiver, Hero targetHero, Hero prodigalSon, Location targetHouse, float questDifficulty, CampaignTime duration, int rewardGold)
			: base(questId, questGiver, duration, rewardGold)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			_targetHero = targetHero;
			_prodigalSon = prodigalSon;
			_targetHouse = targetHouse;
			_questDifficulty = questDifficulty;
			((QuestBase)this).SetDialogs();
			((QuestBase)this).InitializeQuestOnCreation();
		}

		protected override void SetDialogs()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Expected O, but got Unknown
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Expected O, but got Unknown
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Expected O, but got Unknown
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Expected O, but got Unknown
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Expected O, but got Unknown
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Expected O, but got Unknown
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Expected O, but got Unknown
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Expected O, but got Unknown
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Expected O, but got Unknown
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Expected O, but got Unknown
			TextObject val = new TextObject("{=bQnVtegC}Good, even better. [ib:confident][if:convo_astonished]You can find the house easily when you go to {SETTLEMENT} and walk around the town square. Or you could just speak to this gang leader, {TARGET_HERO.LINK}, and make {?TARGET_HERO.GENDER}her{?}him{\\?} understand and get my boy released. Good luck. I await good news.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("TARGET_HERO", _targetHero.CharacterObject, val, false);
			Settlement val2 = ((_targetHero.CurrentSettlement != null) ? _targetHero.CurrentSettlement : _targetHero.PartyBelongedTo.HomeSettlement);
			val.SetTextVariable("SETTLEMENT", val2.EncyclopediaLinkWithName);
			base.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(is_talking_to_quest_giver))
				.Consequence(new OnConsequenceDelegate(QuestAcceptedConsequences))
				.CloseDialog();
			base.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("{=TkYk5yxn}Yes? Go already. Get our boy back.[if:convo_excited]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(is_talking_to_quest_giver))
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(new TextObject("{=kqXxvtwQ}Don't worry I'll free him.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=ddEu5IFQ}I hope so.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(MapEventHelper.OnConversationEnd))
				.CloseDialog()
				.PlayerOption(new TextObject("{=Jss9UqZC}I'll go right away", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=IdKG3IaS}Good to hear that.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(MapEventHelper.OnConversationEnd))
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
			Campaign.Current.ConversationManager.AddDialogFlow(GetTargetHeroDialogFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetProdigalSonDialogFlow(), (object)this);
		}

		protected override void InitializeQuestOnGameLoad()
		{
			((QuestBase)this).SetDialogs();
		}

		protected override void HourlyTick()
		{
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener((object)this, (Action)BeforeMissionOpened);
			CampaignEvents.MissionTickEvent.AddNonSerializedListener((object)this, (Action<float>)OnMissionTick);
			CampaignEvents.WarDeclared.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, DeclareWarDetail>)OnWarDeclared);
			CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
			CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
			CampaignEvents.MapEventStarted.AddNonSerializedListener((object)this, (Action<MapEvent, PartyBase, PartyBase>)OnMapEventStarted);
			CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionStarted);
		}

		private void OnMissionStarted(IMission mission)
		{
			ICampaignMission current = CampaignMission.Current;
			if (((current != null) ? current.Location : null) == _targetHouse)
			{
				_isFirstMissionTick = true;
			}
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _prodigalSon || hero == _targetHero)
			{
				result = false;
			}
		}

		public override void OnHeroCanMoveToSettlementInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _prodigalSon)
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

		private void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			if (victim == _targetHero || victim == _prodigalSon)
			{
				TextObject val = (((int)detail == 8) ? ((QuestBase)this).TargetHeroDisappearedLogText : ((QuestBase)this).TargetHeroDiedLogText);
				StringHelpers.SetCharacterProperties("QUEST_TARGET", victim.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				((QuestBase)this).AddLog(val, false);
				((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
			}
		}

		protected override void OnTimedOut()
		{
			FinishQuestFail1();
		}

		protected override void OnFinalize()
		{
			_targetHouse.RemoveReservation();
		}

		private void BeforeMissionOpened()
		{
			if (Settlement.CurrentSettlement != Settlement || LocationComplex.Current == null)
			{
				return;
			}
			if (LocationComplex.Current.GetLocationOfCharacter(_prodigalSon) == null)
			{
				SpawnProdigalSonInHouse();
				if (!_isHouseFightFinished)
				{
					SpawnThugsInHouse();
					_isMissionFightInitialized = false;
				}
			}
			foreach (AccompanyingCharacter character in PlayerEncounter.LocationEncounter.CharactersAccompanyingPlayer)
			{
				if (!character.CanEnterLocation(_targetHouse))
				{
					character.AllowEntranceToLocations((Func<Location, bool>)((Location x) => character.CanEnterLocation(x) || x == _targetHouse));
				}
			}
		}

		private void OnMissionTick(float dt)
		{
			if (CampaignMission.Current.Location != _targetHouse)
			{
				return;
			}
			Mission current = Mission.Current;
			if (_isFirstMissionTick)
			{
				((IEnumerable<Agent>)Mission.Current.Agents).First((Agent x) => (object)x.Character == _prodigalSon.CharacterObject).GetComponent<CampaignAgentComponent>().AgentNavigator.RemoveBehaviorGroup<AlarmedBehaviorGroup>();
				_isFirstMissionTick = false;
			}
			if (_isMissionFightInitialized || _isHouseFightFinished || ((List<Agent>)(object)current.Agents).Count <= 0)
			{
				return;
			}
			_isMissionFightInitialized = true;
			MissionFightHandler missionBehavior = current.GetMissionBehavior<MissionFightHandler>();
			List<Agent> list = new List<Agent>();
			List<Agent> list2 = new List<Agent>();
			foreach (Agent item in (List<Agent>)(object)current.Agents)
			{
				if (item.IsEnemyOf(Agent.Main))
				{
					list.Add(item);
				}
				else if (item.Team == Agent.Main.Team)
				{
					list2.Add(item);
				}
			}
			missionBehavior.StartCustomFight(list2, list, dropWeapons: false, isItemUseDisabled: false, HouseFightFinished);
			foreach (Agent item2 in list)
			{
				item2.Defensiveness = 2f;
			}
		}

		private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarDetail detail)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Invalid comparison between Unknown and I4
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Invalid comparison between Unknown and I4
			if (((QuestBase)this).QuestGiver.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				if ((int)detail == 4)
				{
					((QuestBase)this).RelationshipChangeWithQuestGiver = -5;
					Tuple<TraitObject, int>[] array = new Tuple<TraitObject, int>[1]
					{
						new Tuple<TraitObject, int>(DefaultTraits.Honor, -50)
					};
					TraitLevelingHelper.OnIssueSolvedThroughQuest(Hero.MainHero, array);
				}
				if (DiplomacyHelper.IsWarCausedByPlayer(faction1, faction2, detail))
				{
					((QuestBase)this).CompleteQuestWithFail(PlayerDeclaredWarQuestLogText);
				}
				else
				{
					((QuestBase)this).CompleteQuestWithCancel(((int)detail == 4) ? CrimeRatingCancelLog : WarDeclaredQuestCancelLog);
				}
			}
		}

		private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
		{
			if (clan == Clan.PlayerClan && ((newKingdom != null && newKingdom.IsAtWarWith(((QuestBase)this).QuestGiver.MapFaction)) || (newKingdom == null && clan.IsAtWarWith(((QuestBase)this).QuestGiver.MapFaction))))
			{
				((QuestBase)this).CompleteQuestWithCancel(WarDeclaredQuestCancelLog);
			}
		}

		private void HouseFightFinished(bool isPlayerSideWon)
		{
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			if (isPlayerSideWon)
			{
				Agent val = LinQuick.FirstOrDefaultQ<Agent>((List<Agent>)(object)Mission.Current.Agents, (Func<Agent, bool>)((Agent x) => (object)x.Character == _prodigalSon.CharacterObject));
				if (val == null)
				{
					Debug.Print("Prodigal son id: " + ((MBObjectBase)_prodigalSon.CharacterObject).StringId, 0, (DebugColor)12, 17592186044416uL);
					Debug.Print("Mission agent count: " + ((List<Agent>)(object)Mission.Current.Agents).Count, 0, (DebugColor)12, 17592186044416uL);
					foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
					{
						Debug.Print(string.Concat("Agent: ", item.Character.Name, ", id: ", ((MBObjectBase)item.Character).StringId, ", team: ", item.Team), 0, (DebugColor)12, 17592186044416uL);
					}
				}
				Vec3 position = val.Position;
				if (((Vec3)(ref position)).Distance(Agent.Main.Position) > val.GetInteractionDistanceToUsable((IUsable)(object)Agent.Main))
				{
					ScriptBehavior.AddTargetWithDelegate(val, SelectPlayerAsTarget, null, OnTargetReached);
				}
				else
				{
					Agent targetAgent = null;
					UsableMachine targetUsableMachine = null;
					WorldFrame targetFrame = WorldFrame.Invalid;
					OnTargetReached(val, ref targetAgent, ref targetUsableMachine, ref targetFrame);
				}
			}
			else
			{
				FinishQuestFail2();
			}
			_isHouseFightFinished = true;
		}

		private bool OnTargetReached(Agent agent, ref Agent targetAgent, ref UsableMachine targetUsableMachine, ref WorldFrame targetFrame)
		{
			Mission.Current.GetMissionBehavior<MissionConversationLogic>().StartConversation(agent, setActionsInstantly: false);
			targetAgent = null;
			return false;
		}

		private bool SelectPlayerAsTarget(Agent agent, ref Agent targetAgent, ref UsableMachine targetUsableMachine, ref WorldFrame targetFrame, ref float customTargetReachedRangeThreshold, ref float customTargetReachedRotationThreshold)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			targetAgent = null;
			Vec3 position = agent.Position;
			if (((Vec3)(ref position)).Distance(Agent.Main.Position) > agent.GetInteractionDistanceToUsable((IUsable)(object)Agent.Main))
			{
				targetAgent = Agent.Main;
			}
			return targetAgent != null;
		}

		private void SpawnProdigalSonInHouse()
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)_prodigalSon.CharacterObject).Race, "_settlement");
			AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)_prodigalSon.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix);
			IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
			LocationCharacter val = new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common", true, (CharacterRelations)0, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
			_targetHouse.AddCharacter(val);
		}

		private void SpawnThugsInHouse()
		{
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Expected O, but got Unknown
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Expected O, but got Unknown
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Expected O, but got Unknown
			CharacterObject item = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_1");
			CharacterObject item2 = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_2");
			CharacterObject item3 = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_3");
			List<CharacterObject> list = new List<CharacterObject>();
			if (_questDifficulty < 0.4f)
			{
				list.Add(item);
				list.Add(item);
				if (_questDifficulty >= 0.2f)
				{
					list.Add(item2);
				}
			}
			else if (_questDifficulty < 0.6f)
			{
				list.Add(item);
				list.Add(item2);
				list.Add(item2);
			}
			else
			{
				list.Add(item2);
				list.Add(item3);
				list.Add(item3);
			}
			foreach (CharacterObject item4 in list)
			{
				Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)item4).Race, "_settlement");
				AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)item4, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix);
				IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
				LocationCharacter val = new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common", true, (CharacterRelations)2, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
				_targetHouse.AddCharacter(val);
			}
		}

		private void QuestAcceptedConsequences()
		{
			((QuestBase)this).StartQuest();
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)Settlement);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_targetHero);
			((QuestBase)this).AddLog(QuestStartedLog, false);
		}

		private DialogFlow GetProdigalSonDialogFlow()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=DYq30shK}Thank you, {?PLAYER.GENDER}milady{?}sir{\\?}.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == _prodigalSon))
				.NpcLine("{=K8TSoRSD}Did {?QUEST_GIVER.GENDER}Lady{?}Lord{\\?} {QUEST_GIVER.LINK} send you to rescue me?", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)delegate
				{
					StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, (TextObject)null, false);
					return true;
				})
				.PlayerLine("{=ln3bGyIO}Yes, I'm here to take you back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=evIohG6b}Thank you, but there's no need. Once we are out of here I can manage to return on my own.[if:convo_happy] I appreciate your efforts. I'll tell everyone in my clan of your heroism.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=qsJxhNGZ}Safe travels {?PLAYER.GENDER}milady{?}sir{\\?}.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					((IEnumerable<Agent>)Mission.Current.Agents).First((Agent x) => (object)x.Character == _prodigalSon.CharacterObject).GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>().DisableScriptedBehavior();
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnEndHouseMissionDialog;
				})
				.CloseDialog();
		}

		private DialogFlow GetTargetHeroDialogFlow()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			//IL_0033: Expected O, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			//IL_007b: Expected O, but got Unknown
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected O, but got Unknown
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Expected O, but got Unknown
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Expected O, but got Unknown
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Expected O, but got Unknown
			DialogFlow val = DialogFlow.CreateDialogFlow("start", 125).BeginNpcOptions((string)null, false).NpcOption(new TextObject("{=M0vxXQGB}Yes? Do you have something to say?[ib:closed][if:convo_nonchalant]", (Dictionary<string, object>)null), (OnConditionDelegate)(() => Hero.OneToOneConversationHero == _targetHero && !_playerTalkedToTargetHero), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					StringHelpers.SetCharacterProperties("PRODIGAL_SON", _prodigalSon.CharacterObject, (TextObject)null, false);
					_playerTalkedToTargetHero = true;
				})
				.PlayerLine("{=K5DgDU2a}I am here for the boy. {PRODIGAL_SON.LINK}. You know who I mean.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.GotoDialogState("start")
				.NpcOption(new TextObject("{=I979VDEn}Yes, did you bring {GOLD_AMOUNT}{GOLD_ICON}? [ib:hip][if:convo_stern]That's what he owes... With an interest of course.", (Dictionary<string, object>)null), (OnConditionDelegate)delegate
				{
					int num;
					if (Hero.OneToOneConversationHero == _targetHero)
					{
						num = (_playerTalkedToTargetHero ? 1 : 0);
						if (num != 0)
						{
							MBTextManager.SetTextVariable("GOLD_AMOUNT", DebtWithInterest);
						}
					}
					else
					{
						num = 0;
					}
					return (byte)num != 0;
				}, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=IboStvbL}Here is the money, now release him!", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.ClickableCondition((OnClickableConditionDelegate)delegate(out TextObject explanation)
				{
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Expected O, but got Unknown
					bool result = false;
					if (Hero.MainHero.Gold >= DebtWithInterest)
					{
						explanation = null;
						result = true;
					}
					else
					{
						explanation = new TextObject("{=YuLLsAUb}You don't have {GOLD_AMOUNT}{GOLD_ICON}.", (Dictionary<string, object>)null);
						explanation.SetTextVariable("GOLD_AMOUNT", DebtWithInterest);
					}
					return result;
				})
				.NpcLine("{=7k03GxZ1}It's great doing business with you. I'll order my men to release him immediately.[if:convo_mocking_teasing]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(FinishQuestSuccess4))
				.CloseDialog()
				.PlayerOption("{=9pTkQ5o2}It would be in your interest to let this young nobleman go...", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)(() => !_playerTriedToPersuade))
				.Consequence((OnConsequenceDelegate)delegate
				{
					_playerTriedToPersuade = true;
					_task = GetPersuasionTask();
					persuasion_start_on_consequence();
				})
				.GotoDialogState("persuade_gang_start_reservation")
				.PlayerOption("{=AwZhx2tT}I will be back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=0fp67gxl}Have a good day.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.EndPlayerOptions()
				.EndNpcOptions();
			AddPersuasionDialogs(val);
			return val;
		}

		private void AddPersuasionDialogs(DialogFlow dialog)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected O, but got Unknown
			//IL_0069: Expected O, but got Unknown
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Expected O, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Expected O, but got Unknown
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Expected O, but got Unknown
			//IL_00eb: Expected O, but got Unknown
			//IL_00eb: Expected O, but got Unknown
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Expected O, but got Unknown
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Expected O, but got Unknown
			//IL_013f: Expected O, but got Unknown
			//IL_013f: Expected O, but got Unknown
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Expected O, but got Unknown
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Expected O, but got Unknown
			//IL_0193: Expected O, but got Unknown
			//IL_0193: Expected O, but got Unknown
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Expected O, but got Unknown
			//IL_01cc: Expected O, but got Unknown
			dialog.AddDialogLine("persuade_gang_introduction", "persuade_gang_start_reservation", "persuade_gang_player_option", "{=EIsQnfLP}Tell me how it's in my interest...[ib:closed][if:convo_nonchalant]", new OnConditionDelegate(persuasion_start_on_condition), (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			dialog.AddDialogLine("persuade_gang_success", "persuade_gang_start_reservation", "close_window", "{=alruamIW}Hmm... You may be right. It's not worth it. I'll release the boy immediately.[ib:hip][if:convo_pondering]", new OnConditionDelegate(ConversationManager.GetPersuasionProgressSatisfied), new OnConsequenceDelegate(persuasion_success_on_consequence), (object)this, int.MaxValue, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			dialog.AddDialogLine("persuade_gang_failed", "persuade_gang_start_reservation", "start", "{=1YGgXOB7}Meh... Do you think ruling the streets of a city is easy? You underestimate us. Now, about the money.[ib:closed2][if:convo_nonchalant]", (OnConditionDelegate)null, new OnConsequenceDelegate(ConversationManager.EndPersuasion), (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			OnConditionDelegate val = persuasion_select_option_1_on_condition;
			OnConsequenceDelegate val2 = persuasion_select_option_1_on_consequence;
			OnPersuasionOptionDelegate val3 = new OnPersuasionOptionDelegate(persuasion_setup_option_1);
			OnClickableConditionDelegate val4 = new OnClickableConditionDelegate(persuasion_clickable_option_1_on_condition);
			dialog.AddPlayerLine("persuade_gang_player_option_1", "persuade_gang_player_option", "persuade_gang_player_option_response", "{=!}{PERSUADE_GANG_ATTEMPT_1}", val, val2, (object)this, 100, val4, val3, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			OnConditionDelegate val5 = persuasion_select_option_2_on_condition;
			OnConsequenceDelegate val6 = persuasion_select_option_2_on_consequence;
			val3 = new OnPersuasionOptionDelegate(persuasion_setup_option_2);
			val4 = new OnClickableConditionDelegate(persuasion_clickable_option_2_on_condition);
			dialog.AddPlayerLine("persuade_gang_player_option_2", "persuade_gang_player_option", "persuade_gang_player_option_response", "{=!}{PERSUADE_GANG_ATTEMPT_2}", val5, val6, (object)this, 100, val4, val3, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			OnConditionDelegate val7 = persuasion_select_option_3_on_condition;
			OnConsequenceDelegate val8 = persuasion_select_option_3_on_consequence;
			val3 = new OnPersuasionOptionDelegate(persuasion_setup_option_3);
			val4 = new OnClickableConditionDelegate(persuasion_clickable_option_3_on_condition);
			dialog.AddPlayerLine("persuade_gang_player_option_3", "persuade_gang_player_option", "persuade_gang_player_option_response", "{=!}{PERSUADE_GANG_ATTEMPT_3}", val7, val8, (object)this, 100, val4, val3, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			dialog.AddDialogLine("persuade_gang_option_reaction", "persuade_gang_player_option_response", "persuade_gang_start_reservation", "{=!}{PERSUASION_REACTION}", new OnConditionDelegate(persuasion_selected_option_response_on_condition), new OnConsequenceDelegate(persuasion_selected_option_response_on_consequence), (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		}

		private bool is_talking_to_quest_giver()
		{
			return Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver;
		}

		private bool persuasion_start_on_condition()
		{
			if (Hero.OneToOneConversationHero == _targetHero && !ConversationManager.GetPersuasionIsFailure())
			{
				return ((IEnumerable<PersuasionOptionArgs>)_task.Options).Any((PersuasionOptionArgs x) => !x.IsBlocked);
			}
			return false;
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
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			PersuasionOptionResult item = ConversationManager.GetPersuasionChosenOptions().Last().Item2;
			MBTextManager.SetTextVariable("PERSUASION_REACTION", PersuasionHelper.GetDefaultPersuasionOptionReaction(item), false);
			if ((int)item == 0)
			{
				_task.BlockAllOptions();
			}
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
				MBTextManager.SetTextVariable("PERSUADE_GANG_ATTEMPT_1", val, false);
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
				MBTextManager.SetTextVariable("PERSUADE_GANG_ATTEMPT_2", val, false);
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
				MBTextManager.SetTextVariable("PERSUADE_GANG_ATTEMPT_3", val, false);
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

		private void persuasion_success_on_consequence()
		{
			ConversationManager.EndPersuasion();
			FinishQuestSuccess3();
		}

		private void OnEndHouseMissionDialog()
		{
			Campaign.Current.GameMenuManager.NextLocation = LocationComplex.Current.GetLocationWithId("center");
			Campaign.Current.GameMenuManager.PreviousLocation = CampaignMission.Current.Location;
			Mission.Current.EndMission();
			FinishQuestSuccess1();
		}

		private PersuasionTask GetPersuasionTask()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Expected O, but got Unknown
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Expected O, but got Unknown
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			PersuasionTask val = new PersuasionTask(0)
			{
				FinalFailLine = TextObject.GetEmpty(),
				TryLaterLine = TextObject.GetEmpty(),
				SpokenLine = new TextObject("{=6P1ruzsC}Maybe...", (Dictionary<string, object>)null)
			};
			PersuasionOptionArgs val2 = new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Calculating, (TraitEffect)0, (PersuasionArgumentStrength)(-3), true, new TextObject("{=Lol4clzR}Look, it was a good try, but they're not going to pay. Releasing the kid is the only move that makes sense.", (Dictionary<string, object>)null), (Tuple<TraitObject, int>[])null, false, false, false);
			val.AddOptionToTask(val2);
			PersuasionOptionArgs val3 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Mercy, (TraitEffect)1, (PersuasionArgumentStrength)(-1), false, new TextObject("{=wJCVlVF7}These nobles aren't like you and me. They've kept their wealth by crushing people like you for generations. Don't mess with them.", (Dictionary<string, object>)null), (Tuple<TraitObject, int>[])null, false, false, false);
			val.AddOptionToTask(val3);
			PersuasionOptionArgs val4 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Generosity, (TraitEffect)0, (PersuasionArgumentStrength)0, false, new TextObject("{=o1KOn4WZ}If you let this boy go, his family will remember you did them a favor. That's a better deal for you than a fight you can't hope to win.", (Dictionary<string, object>)null), (Tuple<TraitObject, int>[])null, false, false, false);
			val.AddOptionToTask(val4);
			return val;
		}

		private void persuasion_start_on_consequence()
		{
			ConversationManager.StartPersuasion(2f, 1f, 1f, 2f, 2f, 0f, (PersuasionDifficulty)5);
		}

		private void FinishQuestSuccess1()
		{
			((QuestBase)this).CompleteQuestWithSuccess();
			((QuestBase)this).AddLog(PlayerDefeatsThugsQuestSuccessLog, false);
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, 5, true, true);
			GainRenownAction.Apply(Hero.MainHero, 3f, false);
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, base.RewardGold, false);
		}

		private void FinishQuestSuccess3()
		{
			((QuestBase)this).CompleteQuestWithSuccess();
			((QuestBase)this).AddLog(PlayerConvincesGangLeaderQuestSuccessLog, false);
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, 5, true, true);
			GainRenownAction.Apply(Hero.MainHero, 1f, false);
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, base.RewardGold, false);
		}

		private void FinishQuestSuccess4()
		{
			GainRenownAction.Apply(Hero.MainHero, 1f, false);
			GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, _targetHero, DebtWithInterest, false);
			((QuestBase)this).CompleteQuestWithSuccess();
			((QuestBase)this).AddLog(PlayerPaysTheDebtQuestSuccessLog, false);
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, 5, true, true);
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, base.RewardGold, false);
		}

		private void FinishQuestFail1()
		{
			((QuestBase)this).AddLog(QuestTimeOutFailLog, false);
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -5, true, true);
		}

		private void FinishQuestFail2()
		{
			((QuestBase)this).CompleteQuestWithFail((TextObject)null);
			((QuestBase)this).AddLog(PlayerHasDefeatedQuestFailLog, false);
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -5, true, true);
		}

		internal static void AutoGeneratedStaticCollectObjectsProdigalSonIssueQuest(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(ProdigalSonIssueQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_targetHero);
			collectedObjects.Add(_prodigalSon);
			collectedObjects.Add(_targetHouse);
		}

		internal static object AutoGeneratedGetMemberValue_targetHero(object o)
		{
			return ((ProdigalSonIssueQuest)o)._targetHero;
		}

		internal static object AutoGeneratedGetMemberValue_prodigalSon(object o)
		{
			return ((ProdigalSonIssueQuest)o)._prodigalSon;
		}

		internal static object AutoGeneratedGetMemberValue_playerTalkedToTargetHero(object o)
		{
			return ((ProdigalSonIssueQuest)o)._playerTalkedToTargetHero;
		}

		internal static object AutoGeneratedGetMemberValue_targetHouse(object o)
		{
			return ((ProdigalSonIssueQuest)o)._targetHouse;
		}

		internal static object AutoGeneratedGetMemberValue_questDifficulty(object o)
		{
			return ((ProdigalSonIssueQuest)o)._questDifficulty;
		}

		internal static object AutoGeneratedGetMemberValue_isHouseFightFinished(object o)
		{
			return ((ProdigalSonIssueQuest)o)._isHouseFightFinished;
		}

		internal static object AutoGeneratedGetMemberValue_playerTriedToPersuade(object o)
		{
			return ((ProdigalSonIssueQuest)o)._playerTriedToPersuade;
		}
	}

	private const IssueFrequency ProdigalSonIssueFrequency = (IssueFrequency)2;

	private const int AgeLimitForSon = 35;

	private const int AgeLimitForIssueOwner = 30;

	private const int MinimumAgeDifference = 10;

	private float MaxDistanceForSettlementSelection => Campaign.Current.GetAverageDistanceBetweenClosestTwoTownsWithNavigationType((NavigationType)1) * 2.18f;

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener((object)this, (Action<Hero>)CheckForIssue);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	public void CheckForIssue(Hero hero)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (ConditionsHold(hero, out var selectedHero, out var targetHero))
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new StartIssueDelegate(OnStartIssue), typeof(ProdigalSonIssue), (IssueFrequency)2, (object)new Tuple<Hero, Hero>(selectedHero, targetHero)));
		}
		else
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(ProdigalSonIssue), (IssueFrequency)2));
		}
	}

	private bool ConditionsHoldForSettlement(Settlement settlement, Hero issueGiver)
	{
		if (settlement.IsTown && settlement.MapFaction == issueGiver.MapFaction && settlement != issueGiver.CurrentSettlement && settlement.OwnerClan != issueGiver.Clan && settlement.OwnerClan != Clan.PlayerClan && ((IEnumerable<Hero>)settlement.HeroesWithoutParty).FirstOrDefault((Func<Hero, bool>)((Hero x) => x.CanHaveCampaignIssues() && x.IsGangLeader)) != null)
		{
			return LinQuick.AnyQ<Location>(settlement.LocationComplex.GetListOfLocations(), (Func<Location, bool>)((Location x) => x.CanBeReserved && !x.IsReserved));
		}
		return false;
	}

	private bool ConditionsHold(Hero issueGiver, out Hero selectedHero, out Hero targetHero)
	{
		selectedHero = null;
		targetHero = null;
		if (issueGiver.IsLord && !issueGiver.IsPrisoner && issueGiver.Clan != Clan.PlayerClan && issueGiver.Age > 30f && issueGiver.GetTraitLevel(DefaultTraits.Mercy) <= 0 && (issueGiver.CurrentSettlement != null || issueGiver.PartyBelongedTo != null))
		{
			selectedHero = Extensions.GetRandomElementWithPredicate<Hero>(issueGiver.Clan.AliveLords, (Func<Hero, bool>)((Hero x) => x.IsActive && !x.IsFemale && x.Age < 35f && (int)x.Age + 10 <= (int)issueGiver.Age && !x.IsPrisoner && x.CanHaveCampaignIssues() && x.PartyBelongedTo == null && x.CurrentSettlement != null && x.GovernorOf == null && x.GetTraitLevel(DefaultTraits.Honor) + x.GetTraitLevel(DefaultTraits.Calculating) < 0));
			if (selectedHero != null)
			{
				Settlement val = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)((Settlement x) => ConditionsHoldForSettlement(x, issueGiver) && ((IEnumerable<Hero>)x.HeroesWithoutParty).FirstOrDefault((Func<Hero, bool>)((Hero y) => y.CanHaveCampaignIssues() && y.IsGangLeader && Campaign.Current.Models.MapDistanceModel.GetDistance(issueGiver.CurrentSettlement, x, false, false, (NavigationType)1) < MaxDistanceForSettlementSelection)) != null));
				targetHero = ((val != null) ? ((IEnumerable<Hero>)val.HeroesWithoutParty).FirstOrDefault((Func<Hero, bool>)((Hero y) => y.CanHaveCampaignIssues() && y.IsGangLeader)) : null);
			}
		}
		if (selectedHero != null)
		{
			return targetHero != null;
		}
		return false;
	}

	private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		PotentialIssueData val = pid;
		Tuple<Hero, Hero> tuple = ((PotentialIssueData)(ref val)).RelatedObject as Tuple<Hero, Hero>;
		return (IssueBase)(object)new ProdigalSonIssue(issueOwner, tuple.Item1, tuple.Item2);
	}
}
