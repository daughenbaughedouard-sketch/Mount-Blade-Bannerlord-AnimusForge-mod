using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace SandBox.Issues;

public class SnareTheWealthyIssueBehavior : CampaignBehaviorBase
{
	public class SnareTheWealthyIssueTypeDefiner : SaveableTypeDefiner
	{
		public SnareTheWealthyIssueTypeDefiner()
			: base(340000)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(SnareTheWealthyIssue), 1, (IObjectResolver)null);
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(SnareTheWealthyIssueQuest), 2, (IObjectResolver)null);
		}

		protected override void DefineEnumTypes()
		{
			((SaveableTypeDefiner)this).AddEnumDefinition(typeof(SnareTheWealthyIssueQuest.SnareTheWealthyQuestChoice), 3, (IEnumResolver)null);
		}
	}

	public class SnareTheWealthyIssue : IssueBase
	{
		private const int IssueDuration = 30;

		private const int IssueQuestDuration = 10;

		private const int MinimumRequiredMenCount = 20;

		private const int MinimumRequiredRelationWithIssueGiver = -10;

		private const int AlternativeSolutionMinimumTroopTier = 2;

		private const int CompanionRoguerySkillValueThreshold = 120;

		[SaveableField(1)]
		private readonly CharacterObject _targetMerchantCharacter;

		private int AlternativeSolutionReward => MathF.Floor(1000f + 3000f * ((IssueBase)this).IssueDifficultyMultiplier);

		public override TextObject IssueBriefByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=bLigh8Sd}Well, let's just say there's an idea I've been mulling over.[ib:confident2][if:convo_bemused] You may be able to help. Have you met {TARGET_MERCHANT.NAME}? {?TARGET_MERCHANT.GENDER}She{?}He{\\?} is a very rich merchant. Very rich indeed. But not very honest… It's not right that someone without morals should have so much wealth, is it? I have a plan to redistribute it a bit.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
				return val;
			}
		}

		public override TextObject IssueAcceptByPlayer => new TextObject("{=keKEFagm}So what's the plan?", (Dictionary<string, object>)null);

		public override TextObject IssueQuestSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=SliFGAX4}{TARGET_MERCHANT.NAME} is always looking for extra swords to protect[if:convo_evil_smile] {?TARGET_MERCHANT.GENDER}her{?}his{\\?} caravans. The wicked are the ones who fear wickedness the most, you might say. What if those guards turned out to be robbers? {TARGET_MERCHANT.NAME} wouldn't trust just anyone but I think {?TARGET_MERCHANT.GENDER}she{?}he{\\?} might hire a renowned warrior like yourself. And if that warrior were to lead the caravan into an ambush… Oh I suppose it's all a bit dishonorable, but I wouldn't worry too much about your reputation. {TARGET_MERCHANT.NAME} is known to defraud {?TARGET_MERCHANT.GENDER}her{?}his{\\?} partners. If something happened to one of {?TARGET_MERCHANT.GENDER}her{?}his{\\?} caravans - well, most people won't know who to believe, and won't really care either.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
				return val;
			}
		}

		public override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=4upBpsnb}All right. I am in.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				TextObject val = new TextObject("{=ivNVRP69}I prefer if you do this yourself, but one of your trusted companions with a strong[if:convo_evil_smile] sword-arm and enough brains to set an ambush can do the job with {TROOP_COUNT} fighters. We'll split the loot, and I'll throw in a little bonus on top of that for you..", (Dictionary<string, object>)null);
				val.SetTextVariable("TROOP_COUNT", ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount());
				return val;
			}
		}

		public override TextObject IssueAlternativeSolutionAcceptByPlayer => new TextObject("{=biqYiCnr}My companion can handle it. Do not worry.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionResponseByIssueGiver => new TextObject("{=UURamhdC}Thank you. This should make both of us a pretty penny.[if:convo_delighted]", (Dictionary<string, object>)null);

		public override TextObject IssueDiscussAlternativeSolution => new TextObject("{=pmuEeFV8}We are still arranging with your men how we'll spring this ambush. Do not worry. Everything will go smoothly.", (Dictionary<string, object>)null);

		protected override TextObject AlternativeSolutionStartLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=28lLrXOe}{ISSUE_GIVER.LINK} shared their plan for robbing {TARGET_MERCHANT.LINK} with you. You agreed to send your companion along with {TROOP_COUNT} men to lead the ambush for them. They will return after {RETURN_DAYS} days.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
				val.SetTextVariable("TROOP_COUNT", base.AlternativeSolutionSentTroops.TotalManCount - 1);
				val.SetTextVariable("RETURN_DAYS", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				return val;
			}
		}

		public override bool IsThereAlternativeSolution => true;

		public override bool IsThereLordSolution => false;

		public override TextObject Title => new TextObject("{=IeihUvCD}Snare the Wealthy", (Dictionary<string, object>)null);

		public override TextObject Description
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=8LghFfQO}Help {ISSUE_GIVER.NAME} to rob {TARGET_MERCHANT.NAME} by acting as their guard.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		protected override bool IssueQuestCanBeDuplicated => false;

		public override AlternativeSolutionScaleFlag AlternativeSolutionScaleFlags => (AlternativeSolutionScaleFlag)(4 | 8);

		public override int AlternativeSolutionBaseNeededMenCount => 10 + MathF.Ceiling(16f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int AlternativeSolutionBaseDurationInDaysInternal => 2 + MathF.Ceiling(4f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int CompanionSkillRewardXP => (int)(800f + 1000f * ((IssueBase)this).IssueDifficultyMultiplier);

		public SnareTheWealthyIssue(Hero issueOwner, CharacterObject targetMerchant)
			: base(issueOwner, CampaignTime.DaysFromNow(30f))
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_targetMerchantCharacter = targetMerchant;
		}

		protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
		{
			if (issueEffect == DefaultIssueEffects.SettlementLoyalty)
			{
				return -0.1f;
			}
			if (issueEffect == DefaultIssueEffects.SettlementSecurity)
			{
				return -0.5f;
			}
			return 0f;
		}

		public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero)
		{
			return ((hero.GetSkillValue(DefaultSkills.Roguery) >= hero.GetSkillValue(DefaultSkills.Tactics)) ? DefaultSkills.Roguery : DefaultSkills.Tactics, 120);
		}

		public override bool AlternativeSolutionCondition(out TextObject explanation)
		{
			return QuestHelper.CheckRosterForAlternativeSolution(MobileParty.MainParty.MemberRoster, ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount(), ref explanation, 2, false);
		}

		public override bool IsTroopTypeNeededByAlternativeSolution(CharacterObject character)
		{
			return character.Tier >= 2;
		}

		public override bool DoTroopsSatisfyAlternativeSolution(TroopRoster troopRoster, out TextObject explanation)
		{
			explanation = null;
			return QuestHelper.CheckRosterForAlternativeSolution(troopRoster, ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount(), ref explanation, 2, false);
		}

		protected override void AlternativeSolutionEndWithSuccessConsequence()
		{
			TraitLevelingHelper.OnIssueSolvedThroughAlternativeSolution(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -50)
			});
			TraitLevelingHelper.OnIssueSolvedThroughAlternativeSolution(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Calculating, 50)
			});
			ChangeRelationAction.ApplyPlayerRelation(((IssueBase)this).IssueOwner, 5, true, true);
			ChangeRelationAction.ApplyPlayerRelation(_targetMerchantCharacter.HeroObject, -10, true, true);
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, AlternativeSolutionReward, false);
		}

		protected override void AlternativeSolutionEndWithFailureConsequence()
		{
			TraitLevelingHelper.OnIssueSolvedThroughAlternativeSolution(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -100)
			});
			TraitLevelingHelper.OnIssueSolvedThroughAlternativeSolution(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Calculating, 100)
			});
			ChangeRelationAction.ApplyPlayerRelation(((IssueBase)this).IssueOwner, -10, true, true);
			ChangeRelationAction.ApplyPlayerRelation(_targetMerchantCharacter.HeroObject, -10, true, true);
		}

		protected override void OnGameLoad()
		{
		}

		protected override void HourlyTick()
		{
		}

		protected override QuestBase GenerateIssueQuest(string questId)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			return (QuestBase)(object)new SnareTheWealthyIssueQuest(questId, ((IssueBase)this).IssueOwner, _targetMerchantCharacter, ((IssueBase)this).IssueDifficultyMultiplier, CampaignTime.DaysFromNow(10f));
		}

		protected override void OnIssueFinalized()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			if (((IssueBase)this).IsSolvingWithQuest)
			{
				Campaign.Current.IssueManager.AddIssueCoolDownData(((object)this).GetType(), (IssueCoolDownData)new HeroRelatedIssueCoolDownData(_targetMerchantCharacter.HeroObject, CampaignTime.DaysFromNow((float)Campaign.Current.Models.IssueModel.IssueOwnerCoolDownInDays)));
				Campaign.Current.IssueManager.AddIssueCoolDownData(typeof(EscortMerchantCaravanIssueQuest), (IssueCoolDownData)new HeroRelatedIssueCoolDownData(_targetMerchantCharacter.HeroObject, CampaignTime.DaysFromNow((float)Campaign.Current.Models.IssueModel.IssueOwnerCoolDownInDays)));
				Campaign.Current.IssueManager.AddIssueCoolDownData(typeof(CaravanAmbushIssueQuest), (IssueCoolDownData)new HeroRelatedIssueCoolDownData(_targetMerchantCharacter.HeroObject, CampaignTime.DaysFromNow((float)Campaign.Current.Models.IssueModel.IssueOwnerCoolDownInDays)));
			}
		}

		public override IssueFrequency GetFrequency()
		{
			return (IssueFrequency)2;
		}

		protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill, out int requiredGold)
		{
			flag = (PreconditionFlags)0;
			relationHero = null;
			requiredGold = 0;
			skill = null;
			if (MobileParty.MainParty.MemberRoster.TotalHealthyCount < 20)
			{
				flag = (PreconditionFlags)((uint)flag | 0x100u);
			}
			if (issueGiver.GetRelationWithPlayer() < -10f)
			{
				flag = (PreconditionFlags)((uint)flag | 1u);
				relationHero = issueGiver;
			}
			if (issueGiver.CurrentSettlement.OwnerClan == Clan.PlayerClan)
			{
				flag = (PreconditionFlags)((uint)flag | 0x8000u);
			}
			return (int)flag == 0;
		}

		public override bool IssueStayAliveConditions()
		{
			if (((IssueBase)this).IssueOwner.IsAlive && ((IssueBase)this).IssueOwner.CurrentSettlement.Town.Security <= 80f)
			{
				return _targetMerchantCharacter.HeroObject.IsAlive;
			}
			return false;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _targetMerchantCharacter.HeroObject)
			{
				result = false;
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsSnareTheWealthyIssue(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(SnareTheWealthyIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((IssueBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_targetMerchantCharacter);
		}

		internal static object AutoGeneratedGetMemberValue_targetMerchantCharacter(object o)
		{
			return ((SnareTheWealthyIssue)o)._targetMerchantCharacter;
		}
	}

	public class SnareTheWealthyIssueQuest : QuestBase
	{
		internal enum SnareTheWealthyQuestChoice
		{
			None,
			SidedWithCaravan,
			SidedWithGang,
			BetrayedBoth
		}

		private delegate void QuestEndDelegate();

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static OnConsequenceDelegate _003C_003E9__64_1;

			public static Func<Settlement, bool> _003C_003E9__69_0;

			public static Func<Settlement, bool> _003C_003E9__70_0;

			internal void _003CGetDialogueWithCaravan_003Eb__64_1()
			{
				PlayerEncounter.LeaveEncounter = true;
			}

			internal bool _003CSpawnQuestParties_003Eb__69_0(Settlement x)
			{
				return x.IsActive;
			}

			internal bool _003CStartBattle_003Eb__70_0(Settlement x)
			{
				return x.IsActive;
			}
		}

		private QuestEndDelegate _startConversationDelegate;

		[SaveableField(1)]
		private CharacterObject _targetMerchantCharacter;

		[SaveableField(2)]
		private Settlement _targetSettlement;

		[SaveableField(3)]
		private MobileParty _caravanParty;

		[SaveableField(4)]
		private MobileParty _gangParty;

		[SaveableField(5)]
		private readonly float _questDifficulty;

		[SaveableField(6)]
		private SnareTheWealthyQuestChoice _playerChoice;

		[SaveableField(7)]
		private bool _canEncounterConversationStart;

		[SaveableField(8)]
		private bool _isCaravanFollowing = true;

		private float CaravanEncounterStartDistance => Campaign.Current.Models.EncounterModel.GetEncounterJoiningRadius * 7f;

		private int CaravanPartyTroopCount => 20 + MathF.Ceiling(40f * _questDifficulty);

		private int GangPartyTroopCount => 10 + MathF.Ceiling(25f * _questDifficulty);

		private int Reward1 => MathF.Floor(1000f + 3000f * _questDifficulty);

		private int Reward2 => MathF.Floor((float)Reward1 * 0.4f);

		public override TextObject Title => new TextObject("{=IeihUvCD}Snare the Wealthy", (Dictionary<string, object>)null);

		public override bool IsRemainingTimeHidden => false;

		private TextObject QuestStartedLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=Ba9nsfHc}{QUEST_GIVER.LINK} shared their plan for robbing {TARGET_MERCHANT.LINK} with you. You agreed to talk with {TARGET_MERCHANT.LINK} to convince {?TARGET_MERCHANT.GENDER}her{?}him{\\?} to guard {?TARGET_MERCHANT.GENDER}her{?}his{\\?} caravan and lead the caravan to ambush around {TARGET_SETTLEMENT}.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject Success1LogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=bblwaDi1}You have successfully robbed {TARGET_MERCHANT.LINK}'s caravan with {QUEST_GIVER.LINK}.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject SidedWithGangLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=lZjj3MZg}When {QUEST_GIVER.LINK} arrived, you kept your side of the bargain and attacked the caravan", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject TimedOutWithoutTalkingToMerchantText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=OMKgidoP}You have failed to convince the merchant to guard {?TARGET_MERCHANT.GENDER}her{?}his{\\?} caravan in time. {QUEST_GIVER.LINK} must be furious.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
				return val;
			}
		}

		private TextObject Fail1LogText => new TextObject("{=DRpcqEMI}The caravan leader said your decisions were wasting their time and decided to go on his way. You have failed to uphold your part in the plan.", (Dictionary<string, object>)null);

		private TextObject Fail2LogText => new TextObject("{=EFjas6hI}At the last moment, you decided to side with the caravan guard and defend them.", (Dictionary<string, object>)null);

		private TextObject Fail2OutcomeLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=JgrG0uoO}Having the {TARGET_MERCHANT.LINK} by your side, you were successful in protecting the caravan.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
				return val;
			}
		}

		private TextObject Fail3LogText => new TextObject("{=0NxiTi8b}You didn't feel like splitting the loot, so you betrayed both the merchant and the gang leader.", (Dictionary<string, object>)null);

		private TextObject Fail3OutcomeLogText => new TextObject("{=KbMew14D}Although the gang leader and the caravaneer joined their forces, you have successfully defeated them and kept the loot for yourself.", (Dictionary<string, object>)null);

		private TextObject Fail4LogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=22nahm29}You have lost the battle against the merchant's caravan and failed to help {QUEST_GIVER.LINK}.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject Fail5LogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=QEgzLRnC}You have lost the battle against {QUEST_GIVER.LINK} and failed to help the merchant as you promised.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject Fail6LogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=pGu2mcar}You have lost the battle against the combined forces of the {QUEST_GIVER.LINK} and the caravan.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject PlayerCapturedQuestSettlementLogText => new TextObject("{=gPFfHluf}Your clan is now owner of the settlement. As the lord of the settlement you cannot be part of the criminal activities anymore. Your agreement with the questgiver has canceled.", (Dictionary<string, object>)null);

		private TextObject QuestSettlementWasCapturedLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=uVigJ3LP}{QUEST_GIVER.LINK} has lost the control of {SETTLEMENT} and the deal is now invalid.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("SETTLEMENT", ((QuestBase)this).QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject WarDeclaredBetweenPlayerAndQuestGiverLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=ojpW4WRD}Your clan is now at war with the {QUEST_GIVER.LINK}'s lord. Your agreement with {QUEST_GIVER.LINK} was canceled.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject TargetSettlementRaidedLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=QkbkesNJ}{QUEST_GIVER.LINK} called off the ambush after {TARGET_SETTLEMENT} was raided.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject TalkedToMerchantLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=N1ZiaLRL}You talked to {TARGET_MERCHANT.LINK} as {QUEST_GIVER.LINK} asked. The caravan is waiting for you outside the gates to be escorted to {TARGET_SETTLEMENT}.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
				val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		public SnareTheWealthyIssueQuest(string questId, Hero questGiver, CharacterObject targetMerchantCharacter, float questDifficulty, CampaignTime duration)
			: base(questId, questGiver, duration, 0)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			_targetMerchantCharacter = targetMerchantCharacter;
			_targetSettlement = GetTargetSettlement();
			_questDifficulty = questDifficulty;
			((QuestBase)this).SetDialogs();
			((QuestBase)this).InitializeQuestOnCreation();
		}

		protected override void InitializeQuestOnGameLoad()
		{
			((QuestBase)this).SetDialogs();
			Campaign.Current.ConversationManager.AddDialogFlow(GetEncounterDialogue(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDialogueWithMerchant(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDialogueWithCaravan(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDialogueWithGangWithoutCaravan(), (object)this);
		}

		private Settlement GetTargetSettlement()
		{
			MapDistanceModel model = Campaign.Current.Models.MapDistanceModel;
			return ((SettlementComponent)Extensions.GetRandomElement<Village>(Extensions.MinBy<Settlement, float>(((IEnumerable<Settlement>)Settlement.All).Where((Settlement t) => t != ((QuestBase)this).QuestGiver.CurrentSettlement && t.IsTown), (Func<Settlement, float>)((Settlement t) => model.GetDistance(t, ((QuestBase)this).QuestGiver.CurrentSettlement, false, false, (NavigationType)1))).BoundVillages)).Settlement;
		}

		protected override void SetDialogs()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected O, but got Unknown
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected O, but got Unknown
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Expected O, but got Unknown
			TextObject discussIntroDialogue = new TextObject("{=lOFR5sq6}Have you talked with {TARGET_MERCHANT.NAME}? It would be a damned waste if we waited too long and word of our plans leaked out.", (Dictionary<string, object>)null);
			TextObject val = new TextObject("{=cc4EEDMg}Splendid. Go have a word with {TARGET_MERCHANT.LINK}. [if:convo_focused_happy]If you can convince {?TARGET_MERCHANT.GENDER}her{?}him{\\?} to guide the caravan, we will wait in ambush along their route.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, val, false);
			base.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver))
				.Consequence(new OnConsequenceDelegate(OnQuestAccepted))
				.CloseDialog();
			base.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(discussIntroDialogue, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
			{
				StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, discussIntroDialogue, false);
				return Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver;
			})
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=YuabHAbV}I'll take care of it shortly..", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=CDXUehf0}Good, good.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.PlayerOption("{=2haJj9mp}I have but I need to deal with some other problems before leading the caravan.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=bSDIHQzO}Please do so. Hate to have word leak out.[if:convo_nervous]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private DialogFlow GetDialogueWithMerchant()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected O, but got Unknown
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			TextObject val = new TextObject("{=OJtUNAbN}Very well. You'll find the caravan [if:convo_calm_friendly]getting ready outside the gates. You will get your payment after the job. Good luck, friend.", (Dictionary<string, object>)null);
			return DialogFlow.CreateDialogFlow("hero_main_options", 125).BeginPlayerOptions((string)null, false).PlayerOption(new TextObject("{=K1ICRis9}I have heard you are looking for extra swords to protect your caravan. I am here to offer my services.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == _targetMerchantCharacter.HeroObject && _caravanParty == null))
				.NpcLine("{=ltbu3S63}Yes, you have heard correctly. I am looking for a capable [if:convo_astonished]leader with a good number of followers. You only need to escort the caravan until they reach {TARGET_SETTLEMENT}. A simple job, but the cargo is very important. I'm willing to pay {MERCHANT_REWARD} denars. And of course, if you betrayed me...", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)delegate
				{
					MBTextManager.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName, false);
					MBTextManager.SetTextVariable("MERCHANT_REWARD", Reward2);
					return true;
				})
				.Consequence(new OnConsequenceDelegate(SpawnQuestParties))
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=AGnd7nDb}Worry not. The outlaws in these parts know my name well, and fear it.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.PlayerOption("{=RCsbpizl}If you have the denars we'll do the job.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.PlayerOption("{=TfDomerj}I think my men and I are more than enough to protect the caravan, good {?TARGET_MERCHANT.GENDER}madam{?}sir{\\?}.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)delegate
				{
					StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, (TextObject)null, false);
					return true;
				})
				.NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.EndPlayerOptions()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private DialogFlow GetDialogueWithCaravan()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			DialogFlow obj = DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=Xs7Qweuw}Lead the way, {PLAYER.NAME}.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => MobileParty.ConversationParty == _caravanParty && _caravanParty != null && !_canEncounterConversationStart));
			object obj2 = _003C_003Ec._003C_003E9__64_1;
			if (obj2 == null)
			{
				OnConsequenceDelegate val = delegate
				{
					PlayerEncounter.LeaveEncounter = true;
				};
				_003C_003Ec._003C_003E9__64_1 = val;
				obj2 = (object)val;
			}
			return obj.Consequence((OnConsequenceDelegate)obj2).CloseDialog();
		}

		private DialogFlow GetDialogueWithGangWithoutCaravan()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=F44s8kPB}Where is the caravan? My men can't wait here for too long.[if:convo_undecided_open]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => MobileParty.ConversationParty == _gangParty && _gangParty != null && !_canEncounterConversationStart))
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=Yqv1jk7D}Don't worry, they are coming towards our trap.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=fHc6fwrb}Good, let's finish this.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private DialogFlow GetEncounterDialogue()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			//IL_0060: Expected O, but got Unknown
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Expected O, but got Unknown
			//IL_0084: Expected O, but got Unknown
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Expected O, but got Unknown
			//IL_00a8: Expected O, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Expected O, but got Unknown
			//IL_00cc: Expected O, but got Unknown
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Expected O, but got Unknown
			//IL_00f0: Expected O, but got Unknown
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Expected O, but got Unknown
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Expected O, but got Unknown
			//IL_0133: Expected O, but got Unknown
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Expected O, but got Unknown
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Expected O, but got Unknown
			//IL_0168: Expected O, but got Unknown
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Expected O, but got Unknown
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Expected O, but got Unknown
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Expected O, but got Unknown
			//IL_01ba: Expected O, but got Unknown
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Expected O, but got Unknown
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Expected O, but got Unknown
			//IL_01ef: Expected O, but got Unknown
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Expected O, but got Unknown
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Expected O, but got Unknown
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Expected O, but got Unknown
			//IL_0241: Expected O, but got Unknown
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Expected O, but got Unknown
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Expected O, but got Unknown
			//IL_0276: Expected O, but got Unknown
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=vVH7wT07}Who are these men? Be on your guard {PLAYER.NAME}, I smell trouble![if:convo_confused_annoyed]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => MobileParty.ConversationParty == _caravanParty && _caravanParty != null && _canEncounterConversationStart))
				.Consequence((OnConsequenceDelegate)delegate
				{
					//IL_0023: Unknown result type (might be due to invalid IL or missing references)
					//IL_0029: Expected O, but got Unknown
					//IL_0034: Unknown result type (might be due to invalid IL or missing references)
					//IL_003a: Unknown result type (might be due to invalid IL or missing references)
					//IL_003b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0045: Expected O, but got Unknown
					//IL_004b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0055: Unknown result type (might be due to invalid IL or missing references)
					//IL_005a: Unknown result type (might be due to invalid IL or missing references)
					//IL_006d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0072: Unknown result type (might be due to invalid IL or missing references)
					//IL_0073: Unknown result type (might be due to invalid IL or missing references)
					//IL_0078: Unknown result type (might be due to invalid IL or missing references)
					//IL_0088: Unknown result type (might be due to invalid IL or missing references)
					//IL_008d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0091: Unknown result type (might be due to invalid IL or missing references)
					//IL_0096: Unknown result type (might be due to invalid IL or missing references)
					//IL_009a: Unknown result type (might be due to invalid IL or missing references)
					//IL_009f: Unknown result type (might be due to invalid IL or missing references)
					//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
					StringHelpers.SetCharacterProperties("TARGET_MERCHANT", _targetMerchantCharacter, (TextObject)null, false);
					AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)ConversationHelper.GetConversationCharacterPartyLeader(_gangParty.Party));
					val.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
					Vec3 val2 = Agent.Main.LookDirection * 10f;
					((Vec3)(ref val2)).RotateAboutZ(1.3962634f);
					Vec3 val3 = Agent.Main.Position + val2;
					val.InitialPosition(ref val3);
					val3 = Agent.Main.LookDirection;
					Vec2 val4 = ((Vec3)(ref val3)).AsVec2;
					val4 = -((Vec2)(ref val4)).Normalized();
					val.InitialDirection(ref val4);
					Agent item = Mission.Current.SpawnAgent(val, false);
					Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<IAgent> { (IAgent)(object)item }, true);
				})
				.NpcLine("{=LJ2AoQyS}Well, well. What do we have here? Must be one of our lucky days, [if:convo_huge_smile]huh? Release all the valuables you carry and nobody gets hurt.", new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), new OnMultipleConversationConsequenceDelegate(IsCaravanMaster), (string)null, (string)null)
				.NpcLine("{=SdgDF4OZ}Hah! You're making a big mistake. See that group of men over there, [if:convo_excited]led by the warrior {PLAYER.NAME}? They're with us, and they'll cut you open.", new OnMultipleConversationConsequenceDelegate(IsCaravanMaster), new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), (string)null, (string)null)
				.NpcLine("{=LaHWB3r0}Oh… I'm afraid there's been a misunderstanding. {PLAYER.NAME} is with us, you see.[if:convo_evil_smile] Did {TARGET_MERCHANT.LINK} stuff you with lies and then send you out to your doom? Oh, shameful, shameful. {?TARGET_MERCHANT.GENDER}She{?}He{\\?} does that fairly often, unfortunately.", new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), new OnMultipleConversationConsequenceDelegate(IsCaravanMaster), (string)null, (string)null)
				.NpcLine("{=EGC4BA4h}{PLAYER.NAME}! Is this true? Look, you're a smart {?PLAYER.GENDER}woman{?}man{\\?}. [if:convo_shocked]You know that {TARGET_MERCHANT.LINK} can pay more than these scum. Take the money and keep your reputation.", new OnMultipleConversationConsequenceDelegate(IsCaravanMaster), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.NpcLine("{=zUKqWeUa}Come on, {PLAYER.NAME}. All this back-and-forth  is making me anxious. Let's finish this.[if:convo_nervous]", new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=UEY5aQ2l}I'm here to rob {TARGET_MERCHANT.NAME}, not be {?TARGET_MERCHANT.GENDER}her{?}his{\\?} lackey. Now, cough up the goods or fight.", new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), (string)null, (string)null)
				.NpcLine("{=tHUHfe6C}You're with them? This is the basest treachery I have ever witnessed![if:convo_furious]", new OnMultipleConversationConsequenceDelegate(IsCaravanMaster), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					((QuestBase)this).AddLog(SidedWithGangLogText, false);
				})
				.NpcLine("{=IKeZLbIK}No offense, captain, but if that's the case you need to get out more. [if:convo_mocking_teasing]Anyway, shall we go to it?", new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					StartBattle(SnareTheWealthyQuestChoice.SidedWithGang);
				})
				.CloseDialog()
				.PlayerOption("{=W7TD4yTc}You know, {TARGET_MERCHANT.NAME}'s man makes a good point. I'm guarding this caravan.", new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), (string)null, (string)null)
				.NpcLine("{=VXp0R7da}Heaven protect you! I knew you'd never be tempted by such a perfidious offer.[if:convo_huge_smile]", new OnMultipleConversationConsequenceDelegate(IsCaravanMaster), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					((QuestBase)this).AddLog(Fail2LogText, false);
				})
				.NpcLine("{=XJOqws2b}Hmf. A funny sense of honor you have… Anyway, I'm not going home empty handed, so let's do this.[if:convo_furious]", new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					StartBattle(SnareTheWealthyQuestChoice.SidedWithCaravan);
				})
				.CloseDialog()
				.PlayerOption("{=ILrYPvTV}You know, I think I'd prefer to take all the loot for myself.", new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), (string)null, (string)null)
				.NpcLine("{=cpTMttNb}Is that so? Hey, caravan captain, whatever your name is… [if:convo_contemptuous]As long as we're all switching sides here, how about I join with you to defeat this miscreant who just betrayed both of us? Whichever of us comes out of this with the most men standing keeps your goods.", new OnMultipleConversationConsequenceDelegate(IsGangPartyLeader), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					((QuestBase)this).AddLog(Fail3LogText, false);
				})
				.NpcLine("{=15UCTrNA}I have no choice, do I? Well, better an honest robber than a traitor![if:convo_aggressive] Let's take {?PLAYER.GENDER}her{?}him{\\?} down.", new OnMultipleConversationConsequenceDelegate(IsCaravanMaster), new OnMultipleConversationConsequenceDelegate(IsMainHero), (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					StartBattle(SnareTheWealthyQuestChoice.BetrayedBoth);
				})
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private void OnQuestAccepted()
		{
			((QuestBase)this).StartQuest();
			((QuestBase)this).AddLog(QuestStartedLogText, false);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_targetMerchantCharacter.HeroObject);
			Campaign.Current.ConversationManager.AddDialogFlow(GetEncounterDialogue(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDialogueWithMerchant(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDialogueWithCaravan(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetDialogueWithGangWithoutCaravan(), (object)this);
		}

		public void GetMountAndHarnessVisualIdsForQuestCaravan(CultureObject culture, out string mountStringId, out string harnessStringId)
		{
			if (((MBObjectBase)culture).StringId == "khuzait" || ((MBObjectBase)culture).StringId == "aserai")
			{
				mountStringId = "camel";
				harnessStringId = "camel_saddle_b";
			}
			else
			{
				mountStringId = "mule";
				harnessStringId = "mule_load_c";
			}
		}

		private void SpawnQuestParties()
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Expected O, but got Unknown
			//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
			TextObject val = GameTexts.FindText("str_caravan_party_name", (string)null);
			TextObjectExtensions.SetCharacterProperties(val, "OWNER", _targetMerchantCharacter, false);
			GetMountAndHarnessVisualIdsForQuestCaravan(_targetMerchantCharacter.Culture, out var mountStringId, out var harnessStringId);
			PartyTemplateObject randomCaravanTemplate = CaravanHelper.GetRandomCaravanTemplate(_targetMerchantCharacter.Culture, false, true);
			_caravanParty = CustomPartyComponent.CreateCustomPartyWithTroopRoster(_targetMerchantCharacter.HeroObject.CurrentSettlement.GatePosition, 0.1f, _targetMerchantCharacter.HeroObject.CurrentSettlement, val, _targetMerchantCharacter.HeroObject.Clan, TroopRoster.CreateDummyTroopRoster(), TroopRoster.CreateDummyTroopRoster(), _targetMerchantCharacter.HeroObject, mountStringId, harnessStringId, MobileParty.MainParty.Speed, false);
			MobilePartyHelper.FillPartyManuallyAfterCreation(_caravanParty, randomCaravanTemplate, CaravanPartyTroopCount);
			_caravanParty.MemberRoster.AddToCounts(_targetMerchantCharacter.Culture.CaravanMaster, 1, false, 0, 0, true, -1);
			_caravanParty.ItemRoster.AddToCounts(Game.Current.ObjectManager.GetObject<ItemObject>("grain"), 40);
			_caravanParty.IgnoreByOtherPartiesTill(((QuestBase)this).QuestDueTime);
			SetPartyAiAction.GetActionForEscortingParty(_caravanParty, MobileParty.MainParty, (NavigationType)1, false, false);
			_caravanParty.Ai.SetDoNotMakeNewDecisions(true);
			_caravanParty.SetPartyUsedByQuest(true);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_caravanParty);
			MobilePartyHelper.TryMatchPartySpeedWithItemWeight(_caravanParty, MobileParty.MainParty.Speed * 1.5f, (ItemObject)null);
			Hideout closestHideout = SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)1, (Func<Settlement, bool>)((Settlement x) => x.IsActive));
			Clan val2 = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan t) => t.Culture == ((SettlementComponent)closestHideout).Settlement.Culture));
			CampaignVec2 gatePosition = _targetSettlement.GatePosition;
			PartyTemplateObject val3 = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("kingdom_hero_party_caravan_ambushers") ?? ((QuestBase)this).QuestGiver.Culture.BanditBossPartyTemplate;
			_gangParty = CustomPartyComponent.CreateCustomPartyWithTroopRoster(gatePosition, 0.1f, _targetSettlement, new TextObject("{=gJNdkwHV}Gang Party", (Dictionary<string, object>)null), (Clan)null, TroopRoster.CreateDummyTroopRoster(), TroopRoster.CreateDummyTroopRoster(), ((QuestBase)this).QuestGiver, "", "", 0f, false);
			MobilePartyHelper.FillPartyManuallyAfterCreation(_gangParty, val3, GangPartyTroopCount);
			_gangParty.MemberRoster.AddToCounts(val2.Culture.BanditBoss, 1, true, 0, 0, true, -1);
			_gangParty.ItemRoster.AddToCounts(Game.Current.ObjectManager.GetObject<ItemObject>("grain"), 40);
			_gangParty.SetPartyUsedByQuest(true);
			_gangParty.IgnoreByOtherPartiesTill(((QuestBase)this).QuestDueTime);
			_gangParty.Ai.SetDoNotMakeNewDecisions(true);
			_gangParty.Ai.DisableAi();
			MobilePartyHelper.TryMatchPartySpeedWithItemWeight(_gangParty, 0.2f, (ItemObject)null);
			_gangParty.SetMoveGoToSettlement(_targetSettlement, (NavigationType)1, false);
			EnterSettlementAction.ApplyForParty(_gangParty, _targetSettlement);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_targetSettlement);
			((QuestBase)this).AddLog(TalkedToMerchantLogText, false);
		}

		private void StartBattle(SnareTheWealthyQuestChoice playerChoice)
		{
			_playerChoice = playerChoice;
			if (_caravanParty.MapEvent != null)
			{
				_caravanParty.MapEvent.FinalizeEvent();
			}
			Hideout closestHideout = SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)1, (Func<Settlement, bool>)((Settlement x) => x.IsActive));
			Clan val = Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan t) => t.Culture == ((SettlementComponent)closestHideout).Settlement.Culture));
			Clan actualClan = ((playerChoice != SnareTheWealthyQuestChoice.SidedWithCaravan) ? val : _caravanParty.Owner.SupporterOf);
			_caravanParty.ActualClan = actualClan;
			Clan actualClan2 = ((playerChoice == SnareTheWealthyQuestChoice.SidedWithGang) ? ((QuestBase)this).QuestGiver.SupporterOf : val);
			_gangParty.ActualClan = actualClan2;
			PartyBase val2 = ((playerChoice != SnareTheWealthyQuestChoice.SidedWithGang) ? _gangParty.Party : _caravanParty.Party);
			PlayerEncounter.Start();
			PlayerEncounter.Current.SetupFields(val2, PartyBase.MainParty);
			PlayerEncounter.StartBattle();
			switch (playerChoice)
			{
			case SnareTheWealthyQuestChoice.BetrayedBoth:
				_caravanParty.MapEventSide = _gangParty.MapEventSide;
				break;
			case SnareTheWealthyQuestChoice.SidedWithCaravan:
				_caravanParty.MapEventSide = PartyBase.MainParty.MapEventSide;
				break;
			default:
				_gangParty.MapEventSide = PartyBase.MainParty.MapEventSide;
				break;
			}
		}

		private void StartEncounterDialogue()
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			if (_gangParty.CurrentSettlement != null)
			{
				LeaveSettlementAction.ApplyForParty(_gangParty);
			}
			PlayerEncounter.Finish(true);
			_canEncounterConversationStart = true;
			ConversationCharacterData val = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, false, false, false, false, false);
			ConversationCharacterData val2 = default(ConversationCharacterData);
			((ConversationCharacterData)(ref val2))._002Ector(ConversationHelper.GetConversationCharacterPartyLeader(_caravanParty.Party), _caravanParty.Party, true, false, false, false, false, true);
			CampaignMission.OpenConversationMission(val, val2, "", "", false);
		}

		private void StartDialogueWithoutCaravan()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			PlayerEncounter.Finish(true);
			ConversationCharacterData val = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, false, false, false, false, false);
			ConversationCharacterData val2 = default(ConversationCharacterData);
			((ConversationCharacterData)(ref val2))._002Ector(ConversationHelper.GetConversationCharacterPartyLeader(_gangParty.Party), _gangParty.Party, true, false, false, false, false, false);
			CampaignMission.OpenConversationMission(val, val2, "", "", false);
		}

		protected override void HourlyTick()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I4
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Invalid comparison between Unknown and I4
			if (_caravanParty != null)
			{
				if ((int)_caravanParty.DefaultBehavior != 14 || (int)_caravanParty.ShortTermBehavior != 14)
				{
					SetPartyAiAction.GetActionForEscortingParty(_caravanParty, MobileParty.MainParty, (NavigationType)1, false, false);
				}
				PartyComponent partyComponent = _caravanParty.PartyComponent;
				((CustomPartyComponent)((partyComponent is CustomPartyComponent) ? partyComponent : null)).CustomPartyBaseSpeed = MobileParty.MainParty.Speed;
				if (MobileParty.MainParty.TargetParty == _caravanParty)
				{
					_caravanParty.SetMoveModeHold();
					_isCaravanFollowing = false;
				}
				else if (!_isCaravanFollowing)
				{
					SetPartyAiAction.GetActionForEscortingParty(_caravanParty, MobileParty.MainParty, (NavigationType)1, false, false);
					_isCaravanFollowing = true;
				}
			}
		}

		private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
		{
			if (settlement == ((QuestBase)this).QuestGiver.CurrentSettlement)
			{
				if (newOwner.Clan == Clan.PlayerClan)
				{
					OnCancel4();
				}
				else
				{
					OnCancel2();
				}
			}
		}

		public void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarDetail reason)
		{
			if ((faction1 == ((QuestBase)this).QuestGiver.MapFaction && faction2 == Hero.MainHero.MapFaction) || (faction2 == ((QuestBase)this).QuestGiver.MapFaction && faction1 == Hero.MainHero.MapFaction))
			{
				OnCancel1();
			}
		}

		public void OnVillageStateChanged(Village village, VillageStates oldState, VillageStates newState, MobileParty raiderParty)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (village == _targetSettlement.Village && (int)newState != 0)
			{
				OnCancel3();
			}
		}

		public void OnMapEventEnded(MapEvent mapEvent)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			if (!mapEvent.IsPlayerMapEvent || _caravanParty == null)
			{
				return;
			}
			if (mapEvent.InvolvedParties.Contains(_caravanParty.Party))
			{
				if (!mapEvent.InvolvedParties.Contains(_gangParty.Party))
				{
					OnFail1();
				}
				else if (mapEvent.WinningSide == mapEvent.PlayerSide)
				{
					if (_playerChoice == SnareTheWealthyQuestChoice.SidedWithGang)
					{
						OnSuccess1();
					}
					else if (_playerChoice == SnareTheWealthyQuestChoice.SidedWithCaravan)
					{
						OnFail2();
					}
					else
					{
						OnFail3();
					}
				}
				else if (_playerChoice == SnareTheWealthyQuestChoice.SidedWithGang)
				{
					OnFail4();
				}
				else if (_playerChoice == SnareTheWealthyQuestChoice.SidedWithCaravan)
				{
					OnFail5();
				}
				else
				{
					OnFail6();
				}
			}
			else
			{
				OnFail1();
			}
		}

		private void OnPartyJoinedArmy(MobileParty mobileParty)
		{
			if (mobileParty == MobileParty.MainParty && _caravanParty != null)
			{
				OnFail1();
			}
		}

		private void OnGameMenuOpened(MenuCallbackArgs args)
		{
			if (_startConversationDelegate != null && MobileParty.MainParty.CurrentSettlement == _targetSettlement && _caravanParty != null)
			{
				_startConversationDelegate();
				_startConversationDelegate = null;
			}
		}

		public void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (party == MobileParty.MainParty && settlement == _targetSettlement && _caravanParty != null)
			{
				CampaignVec2 position = _caravanParty.Position;
				if (((CampaignVec2)(ref position)).DistanceSquared(_targetSettlement.Position) <= CaravanEncounterStartDistance)
				{
					_startConversationDelegate = StartEncounterDialogue;
				}
				else
				{
					_startConversationDelegate = StartDialogueWithoutCaravan;
				}
			}
		}

		public void OnSettlementLeft(MobileParty party, Settlement settlement)
		{
			if (party == MobileParty.MainParty && _caravanParty != null)
			{
				SetPartyAiAction.GetActionForEscortingParty(_caravanParty, MobileParty.MainParty, (NavigationType)1, false, false);
			}
		}

		private void CanHeroBecomePrisoner(Hero hero, ref bool result)
		{
			if (hero == Hero.MainHero && _playerChoice != SnareTheWealthyQuestChoice.None)
			{
				result = false;
			}
		}

		protected override void OnFinalize()
		{
			if (_caravanParty != null && _caravanParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, _caravanParty);
			}
			if (_gangParty != null && _gangParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, _gangParty);
			}
		}

		private void OnSuccess1()
		{
			((QuestBase)this).AddLog(Success1LogText, false);
			TraitLevelingHelper.OnIssueSolvedThroughQuest(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -100)
			});
			TraitLevelingHelper.OnIssueSolvedThroughQuest(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Calculating, 50)
			});
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, 5, true, true);
			ChangeRelationAction.ApplyPlayerRelation(_targetMerchantCharacter.HeroObject, -10, true, true);
			((QuestBase)this).QuestGiver.AddPower(30f);
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, Reward1, false);
			((QuestBase)this).CompleteQuestWithSuccess();
		}

		private void OnTimedOutWithoutTalkingToMerchant()
		{
			((QuestBase)this).AddLog(TimedOutWithoutTalkingToMerchantText, false);
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -50)
			});
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -5, true, true);
		}

		private void OnFail1()
		{
			ApplyFail1Consequences();
			((QuestBase)this).CompleteQuestWithFail((TextObject)null);
		}

		private void ApplyFail1Consequences()
		{
			((QuestBase)this).AddLog(Fail1LogText, false);
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -50)
			});
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -5, true, true);
			ChangeRelationAction.ApplyPlayerRelation(_targetMerchantCharacter.HeroObject, -5, true, true);
		}

		private void OnFail2()
		{
			((QuestBase)this).AddLog(Fail2OutcomeLogText, false);
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, 100)
			});
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -10, true, true);
			ChangeRelationAction.ApplyPlayerRelation(_targetMerchantCharacter.HeroObject, 5, true, true);
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, Reward2, false);
			((QuestBase)this).CompleteQuestWithBetrayal((TextObject)null);
		}

		private void OnFail3()
		{
			((QuestBase)this).AddLog(Fail3OutcomeLogText, false);
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -200)
			});
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Calculating, 100)
			});
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -15, true, true);
			ChangeRelationAction.ApplyPlayerRelation(_targetMerchantCharacter.HeroObject, -20, true, true);
			((QuestBase)this).CompleteQuestWithBetrayal((TextObject)null);
		}

		private void OnFail4()
		{
			((QuestBase)this).AddLog(Fail4LogText, false);
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -100)
			});
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Calculating, 100)
			});
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -10, true, true);
			ChangeRelationAction.ApplyPlayerRelation(_targetMerchantCharacter.HeroObject, -10, true, true);
			((QuestBase)this).CompleteQuestWithFail((TextObject)null);
		}

		private void OnFail5()
		{
			((QuestBase)this).AddLog(Fail5LogText, false);
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -100)
			});
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Calculating, 100)
			});
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -10, true, true);
			ChangeRelationAction.ApplyPlayerRelation(_targetMerchantCharacter.HeroObject, -10, true, true);
			((QuestBase)this).CompleteQuestWithBetrayal((TextObject)null);
		}

		private void OnFail6()
		{
			((QuestBase)this).AddLog(Fail6LogText, false);
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -200)
			});
			TraitLevelingHelper.OnIssueFailed(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Calculating, 100)
			});
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -15, true, true);
			ChangeRelationAction.ApplyPlayerRelation(_targetMerchantCharacter.HeroObject, -20, true, true);
			((QuestBase)this).CompleteQuestWithBetrayal((TextObject)null);
		}

		protected override void OnTimedOut()
		{
			if (_caravanParty == null)
			{
				OnTimedOutWithoutTalkingToMerchant();
			}
			else
			{
				ApplyFail1Consequences();
			}
		}

		private void OnCancel1()
		{
			((QuestBase)this).AddLog(WarDeclaredBetweenPlayerAndQuestGiverLogText, false);
			((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
		}

		private void OnCancel2()
		{
			((QuestBase)this).AddLog(QuestSettlementWasCapturedLogText, false);
			((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
		}

		private void OnCancel3()
		{
			((QuestBase)this).AddLog(TargetSettlementRaidedLogText, false);
			((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
		}

		private void OnCancel4()
		{
			((QuestBase)this).AddLog(PlayerCapturedQuestSettlementLogText, false);
			((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
		}

		private bool IsGangPartyLeader(IAgent agent)
		{
			return (object)agent.Character == ConversationHelper.GetConversationCharacterPartyLeader(_gangParty.Party);
		}

		private bool IsCaravanMaster(IAgent agent)
		{
			return (object)agent.Character == ConversationHelper.GetConversationCharacterPartyLeader(_caravanParty.Party);
		}

		private bool IsMainHero(IAgent agent)
		{
			return (object)agent.Character == CharacterObject.PlayerCharacter;
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _targetMerchantCharacter.HeroObject)
			{
				result = false;
			}
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
			CampaignEvents.WarDeclared.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, DeclareWarDetail>)OnWarDeclared);
			CampaignEvents.VillageStateChanged.AddNonSerializedListener((object)this, (Action<Village, VillageStates, VillageStates, MobileParty>)OnVillageStateChanged);
			CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
			CampaignEvents.OnPartyJoinedArmyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnPartyJoinedArmy);
			CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
			CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
			CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
			CampaignEvents.CanHeroBecomePrisonerEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanHeroBecomePrisoner);
			CampaignEvents.CanHaveCampaignIssuesEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)((QuestBase)this).OnHeroCanHaveCampaignIssuesInfoIsRequested);
		}

		internal static void AutoGeneratedStaticCollectObjectsSnareTheWealthyIssueQuest(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(SnareTheWealthyIssueQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_targetMerchantCharacter);
			collectedObjects.Add(_targetSettlement);
			collectedObjects.Add(_caravanParty);
			collectedObjects.Add(_gangParty);
		}

		internal static object AutoGeneratedGetMemberValue_targetMerchantCharacter(object o)
		{
			return ((SnareTheWealthyIssueQuest)o)._targetMerchantCharacter;
		}

		internal static object AutoGeneratedGetMemberValue_targetSettlement(object o)
		{
			return ((SnareTheWealthyIssueQuest)o)._targetSettlement;
		}

		internal static object AutoGeneratedGetMemberValue_caravanParty(object o)
		{
			return ((SnareTheWealthyIssueQuest)o)._caravanParty;
		}

		internal static object AutoGeneratedGetMemberValue_gangParty(object o)
		{
			return ((SnareTheWealthyIssueQuest)o)._gangParty;
		}

		internal static object AutoGeneratedGetMemberValue_questDifficulty(object o)
		{
			return ((SnareTheWealthyIssueQuest)o)._questDifficulty;
		}

		internal static object AutoGeneratedGetMemberValue_playerChoice(object o)
		{
			return ((SnareTheWealthyIssueQuest)o)._playerChoice;
		}

		internal static object AutoGeneratedGetMemberValue_canEncounterConversationStart(object o)
		{
			return ((SnareTheWealthyIssueQuest)o)._canEncounterConversationStart;
		}

		internal static object AutoGeneratedGetMemberValue_isCaravanFollowing(object o)
		{
			return ((SnareTheWealthyIssueQuest)o)._isCaravanFollowing;
		}
	}

	private const IssueFrequency SnareTheWealthyIssueFrequency = (IssueFrequency)2;

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnCheckForIssue);
	}

	private void OnCheckForIssue(Hero hero)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (ConditionsHold(hero))
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new StartIssueDelegate(OnStartIssue), typeof(SnareTheWealthyIssue), (IssueFrequency)2, (object)null));
		}
		else
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(SnareTheWealthyIssue), (IssueFrequency)2));
		}
	}

	private bool ConditionsHold(Hero issueGiver)
	{
		if (issueGiver.IsGangLeader && issueGiver.CurrentSettlement != null && issueGiver.CurrentSettlement.IsTown && !issueGiver.CurrentSettlement.HasPort && issueGiver.CurrentSettlement.Town.Security <= 50f)
		{
			return GetTargetMerchant(issueGiver) != null;
		}
		return false;
	}

	private Hero GetTargetMerchant(Hero issueOwner)
	{
		Hero result = null;
		foreach (Hero item in (List<Hero>)(object)issueOwner.CurrentSettlement.Notables)
		{
			if (item != issueOwner && item.IsMerchant && item.Power >= 150f && item.GetTraitLevel(DefaultTraits.Mercy) + item.GetTraitLevel(DefaultTraits.Honor) < 0 && item.CanHaveCampaignIssues() && !Campaign.Current.IssueManager.HasIssueCoolDown(typeof(SnareTheWealthyIssue), item) && !Campaign.Current.IssueManager.HasIssueCoolDown(typeof(EscortMerchantCaravanIssueBehavior), item) && !Campaign.Current.IssueManager.HasIssueCoolDown(typeof(CaravanAmbushIssueBehavior), item))
			{
				result = item;
				break;
			}
		}
		return result;
	}

	private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
	{
		Hero targetMerchant = GetTargetMerchant(issueOwner);
		return (IssueBase)(object)new SnareTheWealthyIssue(issueOwner, targetMerchant.CharacterObject);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
