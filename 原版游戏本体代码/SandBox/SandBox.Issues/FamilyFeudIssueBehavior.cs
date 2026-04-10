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
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace SandBox.Issues;

public class FamilyFeudIssueBehavior : CampaignBehaviorBase
{
	public class FamilyFeudIssueTypeDefiner : SaveableTypeDefiner
	{
		public FamilyFeudIssueTypeDefiner()
			: base(1087000)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(FamilyFeudIssue), 1, (IObjectResolver)null);
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(FamilyFeudIssueQuest), 2, (IObjectResolver)null);
		}
	}

	public class FamilyFeudIssueMissionBehavior : MissionLogic
	{
		private Action<Agent, Agent, int> OnAgentHitAction;

		public FamilyFeudIssueMissionBehavior(Action<Agent, Agent, int> agentHitAction)
		{
			OnAgentHitAction = agentHitAction;
		}

		public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
		{
			OnAgentHitAction?.Invoke(affectedAgent, affectorAgent, blow.InflictedDamage);
		}
	}

	public class FamilyFeudIssue : IssueBase
	{
		private const int CompanionRequiredSkillLevel = 120;

		private const int QuestTimeLimit = 20;

		private const int IssueDuration = 30;

		private const int TroopTierForAlternativeSolution = 2;

		[SaveableField(10)]
		private Settlement _targetVillage;

		[SaveableField(20)]
		private Hero _targetNotable;

		public override AlternativeSolutionScaleFlag AlternativeSolutionScaleFlags => (AlternativeSolutionScaleFlag)8;

		public override int AlternativeSolutionBaseNeededMenCount => 3 + MathF.Ceiling(5f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int AlternativeSolutionBaseDurationInDaysInternal => 3 + MathF.Ceiling(7f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int RewardGold => (int)(350f + 1500f * ((IssueBase)this).IssueDifficultyMultiplier);

		[SaveableProperty(30)]
		public override Hero CounterOfferHero { get; protected set; }

		public override int NeededInfluenceForLordSolution => 20;

		protected override int CompanionSkillRewardXP => (int)(500f + 700f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override TextObject AlternativeSolutionStartLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=zRJ1bQFO}{ISSUE_GIVER.LINK}, a landowner from {ISSUE_GIVER_SETTLEMENT}, told you about an incident that is about to turn into an ugly feud. One of the youngsters killed another in an accident and the victim's family refused blood money as compensation and wants blood. You decided to leave {COMPANION.LINK} with some men for {RETURN_DAYS} days to let things cool down. They should return with the reward of {REWARD_GOLD}{GOLD_ICON} denars as promised by {ISSUE_GIVER.LINK} after {RETURN_DAYS} days.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("COMPANION", ((IssueBase)this).AlternativeSolutionHero.CharacterObject, val, false);
				val.SetTextVariable("ISSUE_GIVER_SETTLEMENT", ((IssueBase)this).IssueOwner.CurrentSettlement.EncyclopediaLinkWithName);
				val.SetTextVariable("RETURN_DAYS", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				val.SetTextVariable("REWARD_GOLD", ((IssueBase)this).RewardGold);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				return val;
			}
		}

		public override bool IsThereAlternativeSolution => true;

		public override bool IsThereLordSolution => true;

		public override TextObject IssueBriefByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Expected O, but got Unknown
				TextObject val = new TextObject("{=7qPda0SA}Yes... We do have a problem. One of my relatives fell victim to his temper during a quarrel and killed a man from {TARGET_VILLAGE}.[ib:normal2][if:convo_dismayed] We offered to pay blood money but the family of the deceased have stubbornly refused it. As it turns out, the deceased is kin to {TARGET_NOTABLE}, an elder of this region and now the men of {TARGET_VILLAGE} have sworn to kill my relative.", (Dictionary<string, object>)null);
				val.SetTextVariable("TARGET_VILLAGE", _targetVillage.Name);
				val.SetTextVariable("TARGET_NOTABLE", _targetNotable.Name);
				return val;
			}
		}

		public override TextObject IssueAcceptByPlayer => new TextObject("{=XX3sWsVX}This sounds pretty serious. Go on.", (Dictionary<string, object>)null);

		public override TextObject IssueQuestSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=mgUoXwZt}My family is concerned for the boy's life. He has gone hiding around the village commons. We need someone who can protect him until [ib:normal][if:convo_normal]{TARGET_NOTABLE.LINK} sees reason, accepts the blood money and ends the feud. We would be eternally grateful, if you can help my relative and take him with you for a while maybe.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				val.SetTextVariable("TARGET_VILLAGE", _targetVillage.Name);
				return val;
			}
		}

		public override TextObject IssueAlternativeSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				TextObject val = new TextObject("{=cDYz49kZ}You can keep my relative under your protection for a time until the calls for vengeance die down.[ib:closed][if:convo_pondering] Maybe you can leave one of your warrior companions and {ALTERNATIVE_TROOP_COUNT} men with him to protect him.", (Dictionary<string, object>)null);
				val.SetTextVariable("ALTERNATIVE_TROOP_COUNT", ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount());
				return val;
			}
		}

		protected override TextObject LordSolutionStartLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=oJt4bemH}{QUEST_GIVER.LINK}, a landowner from {QUEST_SETTLEMENT}, told you about an incident that is about to turn into an ugly feud. One young man killed another in an quarrel and the victim's family refused blood money compensation, demanding vengeance instead.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				val.SetTextVariable("QUEST_SETTLEMENT", ((IssueBase)this).IssueOwner.CurrentSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		protected override TextObject LordSolutionCounterOfferRefuseLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=JqN5BSjN}As the dispenser of justice in the district, you decided to allow {TARGET_NOTABLE.LINK} to take vengeance for {?TARGET_NOTABLE.GENDER}her{?}his{\\?} kinsman. You failed to protect the culprit as you promised. {QUEST_GIVER.LINK} is furious.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		protected override TextObject LordSolutionCounterOfferAcceptLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=UxrXNSW7}As the ruler, you have let {TARGET_NOTABLE.LINK} to take {?TARGET_NOTABLE.GENDER}her{?}him{\\?} kinsman's vengeance and failed to protect the boy as you have promised to {QUEST_GIVER.LINK}. {QUEST_GIVER.LINK} is furious.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
				return val;
			}
		}

		public override TextObject IssueLordSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=tsjwrZCZ}I am sure that, as {?PLAYER.GENDER}lady{?}lord{\\?} of this district, you will not let these unlawful threats go unpunished. As the lord of the region, you can talk to {TARGET_NOTABLE.LINK} and force him to accept the blood money.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject IssuePlayerResponseAfterLordExplanation => new TextObject("{=A3GfCPUb}I'm not sure about using my authority in this way. Is there any other way to solve this?", (Dictionary<string, object>)null);

		public override TextObject IssuePlayerResponseAfterAlternativeExplanation => new TextObject("{=8EaCJ2uw}What else can I do?", (Dictionary<string, object>)null);

		public override TextObject IssueLordSolutionAcceptByPlayer
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=Du31GKSb}As the magistrate of this district, I hereby order that blood money shall be accepted. This is a crime of passion, not malice. Tell {TARGET_NOTABLE.LINK} to take the silver or face my wrath!", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject IssueLordSolutionResponseByIssueGiver => new TextObject("{=xNyLPMnx}Thank you, my {?PLAYER.GENDER}lady{?}lord{\\?}, thank you.", (Dictionary<string, object>)null);

		public override TextObject IssueLordSolutionCounterOfferExplanationByOtherNpc
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=vjk2q3OT}{?PLAYER.GENDER}Madam{?}Sir{\\?}, {TARGET_NOTABLE.LINK}'s nephew murdered one of my kinsman, [ib:aggressive][if:convo_bared_teeth]and it is our right to take vengeance on the murderer. Custom gives us the right of vengeance. Everyone must know that we are willing to avenge our sons, or others will think little of killing them. Does it do us good to be a clan of old men and women, drowning in silver, if all our sons are slain? Please sir, allow us to take vengeance. We promise we won't let this turn into a senseless blood feud.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
				return val;
			}
		}

		public override TextObject IssueLordSolutionCounterOfferBriefByOtherNpc => new TextObject("{=JhbbB2dp}My {?PLAYER.GENDER}lady{?}lord{\\?}, may I have a word please?", (Dictionary<string, object>)null);

		public override TextObject IssueLordSolutionCounterOfferAcceptByPlayer => new TextObject("{=TIVHLAjy}You may have a point. I hereby revoke my previous decision.", (Dictionary<string, object>)null);

		public override TextObject IssueLordSolutionCounterOfferAcceptResponseByOtherNpc => new TextObject("{=A9uSikTY}Thank you my {?PLAYER.GENDER}lady{?}lord{\\?}.", (Dictionary<string, object>)null);

		public override TextObject IssueLordSolutionCounterOfferDeclineByPlayer => new TextObject("{=Vs9DfZmJ}No. My word is final. You will have to take the blood money.", (Dictionary<string, object>)null);

		public override TextObject IssueLordSolutionCounterOfferDeclineResponseByOtherNpc => new TextObject("{=3oaVUNdr}I hope you won't be [if:convo_disbelief]regret with your decision, my {?PLAYER.GENDER}lady{?}lord{\\?}.", (Dictionary<string, object>)null);

		public override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=VcfZdKcp}Don't worry, I will protect your relative.", (Dictionary<string, object>)null);

		public override TextObject Title => new TextObject("{=ZpDQxmzJ}Family Feud", (Dictionary<string, object>)null);

		public override TextObject Description
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=aSZvZRYC}A relative of {QUEST_GIVER.NAME} kills a relative of {TARGET_NOTABLE.NAME}. {QUEST_GIVER.NAME} offers to pay blood money for the crime but {TARGET_NOTABLE.NAME} wants revenge.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject IssueAlternativeSolutionAcceptByPlayer
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Expected O, but got Unknown
				TextObject val = new TextObject("{=9ZngZ6W7}I will have one of my companions and {REQUIRED_TROOP_AMOUNT} of my men protect your kinsman for {RETURN_DAYS} days. ", (Dictionary<string, object>)null);
				val.SetTextVariable("REQUIRED_TROOP_AMOUNT", ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount());
				val.SetTextVariable("RETURN_DAYS", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				return val;
			}
		}

		public override TextObject IssueDiscussAlternativeSolution
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0023: Expected O, but got Unknown
				TextObject val = new TextObject("{=n9QRnxbC}I have no doubt that {TARGET_NOTABLE.LINK} will have to accept[ib:closed][if:convo_grateful] the offer after seeing the boy with that many armed men behind him. Thank you, {?PLAYER.GENDER}madam{?}sir{\\?}, for helping to ending this without more blood.", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val, "TARGET_NOTABLE", _targetNotable.CharacterObject, false);
				return val;
			}
		}

		public override TextObject IssueAlternativeSolutionResponseByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=MaGPKGHA}Thank you my {?PLAYER.GENDER}lady{?}lord{\\?}. [if:convo_pondering]I am sure your men will protect the boy and {TARGET_NOTABLE.LINK} will have nothing to do but to accept the blood money. I have to add, I'm ready to pay you {REWARD_GOLD}{GOLD_ICON} denars for your trouble.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				val.SetTextVariable("REWARD_GOLD", ((IssueBase)this).RewardGold);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				return val;
			}
		}

		public override TextObject IssueAsRumorInSettlement
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=lmVCRD4Q}I hope {QUEST_GIVER.LINK} [if:convo_disbelief]can work out that trouble with {?QUEST_GIVER.GENDER}her{?}his{\\?} kinsman.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject IssueAlternativeSolutionSuccessLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=vS6oZJPA}Your companion {COMPANION.LINK} and your men returns with the news of their success. Apparently {TARGET_NOTABLE.LINK} and {?TARGET_NOTABLE.GENDER}her{?}his{\\?} thugs finds the culprit and tries to murder him but your men manages to drive them away. {COMPANION.LINK} tells you that they bloodied their noses so badly that they wouldn’t dare to try again. {QUEST_GIVER.LINK} is grateful and sends {?QUEST_GIVER.GENDER}her{?}his{\\?} regards with a purse full of {REWARD}{GOLD_ICON} denars.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("COMPANION", ((IssueBase)this).AlternativeSolutionHero.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				val.SetTextVariable("REWARD", ((IssueBase)this).RewardGold);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				return val;
			}
		}

		public FamilyFeudIssue(Hero issueOwner, Hero targetNotable, Settlement targetVillage)
			: base(issueOwner, CampaignTime.DaysFromNow(30f))
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_targetNotable = targetNotable;
			_targetVillage = targetVillage;
		}

		public override void OnHeroCanBeSelectedInInventoryInfoIsRequested(Hero hero, ref bool result)
		{
			CommonResrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanHavePartyRoleOrBeGovernorInfoIsRequested(Hero hero, ref bool result)
		{
			CommonResrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanLeadPartyInfoIsRequested(Hero hero, ref bool result)
		{
			CommonResrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			CommonResrictionInfoIsRequested(hero, ref result);
		}

		private void CommonResrictionInfoIsRequested(Hero hero, ref bool result)
		{
			if (_targetNotable == hero)
			{
				result = false;
			}
		}

		protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
		{
			if (issueEffect == DefaultIssueEffects.SettlementSecurity)
			{
				return -1f;
			}
			if (issueEffect == DefaultIssueEffects.IssueOwnerPower)
			{
				return -0.1f;
			}
			return 0f;
		}

		public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero)
		{
			return ((hero.GetSkillValue(DefaultSkills.Athletics) >= hero.GetSkillValue(DefaultSkills.Charm)) ? DefaultSkills.Athletics : DefaultSkills.Charm, 120);
		}

		protected override void LordSolutionConsequenceWithAcceptCounterOffer()
		{
			TraitLevelingHelper.OnIssueSolvedThroughBetrayal(((IssueBase)this).IssueOwner, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -50)
			});
			((IssueBase)this).RelationshipChangeWithIssueOwner = -10;
			ChangeRelationAction.ApplyPlayerRelation(_targetNotable, 5, true, true);
			Town town = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
			town.Prosperity -= 5f;
			Town town2 = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
			town2.Security -= 5f;
		}

		protected override void LordSolutionConsequenceWithRefuseCounterOffer()
		{
			ApplySuccessRewards();
		}

		public override bool LordSolutionCondition(out TextObject explanation)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			if (((IssueBase)this).IssueOwner.CurrentSettlement.OwnerClan == Clan.PlayerClan)
			{
				explanation = null;
				return true;
			}
			explanation = new TextObject("{=9y0zpKUF}You need to be the owner of this settlement!", (Dictionary<string, object>)null);
			return false;
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

		protected override void AlternativeSolutionEndWithSuccessConsequence()
		{
			ApplySuccessRewards();
			float randomFloat = MBRandom.RandomFloat;
			SkillObject val = null;
			val = ((randomFloat <= 0.33f) ? DefaultSkills.OneHanded : ((!(randomFloat <= 0.66f)) ? DefaultSkills.Polearm : DefaultSkills.TwoHanded));
			((IssueBase)this).AlternativeSolutionHero.AddSkillXp(val, (float)(int)(500f + 700f * ((IssueBase)this).IssueDifficultyMultiplier));
		}

		protected override void AlternativeSolutionEndWithFailureConsequence()
		{
			((IssueBase)this).RelationshipChangeWithIssueOwner = -10;
			ChangeRelationAction.ApplyPlayerRelation(_targetNotable, 5, true, true);
			Town town = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
			town.Security -= 5f;
			Town town2 = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
			town2.Prosperity -= 5f;
		}

		private void ApplySuccessRewards()
		{
			GainRenownAction.Apply(Hero.MainHero, 1f, false);
			((IssueBase)this).RelationshipChangeWithIssueOwner = 10;
			ChangeRelationAction.ApplyPlayerRelation(_targetNotable, -5, true, true);
			Town town = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
			town.Security += 10f;
		}

		protected override void AfterIssueCreation()
		{
			((IssueBase)this).CounterOfferHero = ((IEnumerable<Hero>)((IssueBase)this).IssueOwner.CurrentSettlement.Notables).FirstOrDefault((Func<Hero, bool>)((Hero x) => ((BasicCharacterObject)x.CharacterObject).IsHero && x.CharacterObject.HeroObject != ((IssueBase)this).IssueOwner));
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
			return (QuestBase)(object)new FamilyFeudIssueQuest(questId, ((IssueBase)this).IssueOwner, CampaignTime.DaysFromNow(20f), _targetVillage, _targetNotable, ((IssueBase)this).RewardGold);
		}

		public override IssueFrequency GetFrequency()
		{
			return (IssueFrequency)2;
		}

		protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill, out int requiredGold)
		{
			skill = null;
			relationHero = null;
			requiredGold = 0;
			flag = (PreconditionFlags)0;
			if (issueGiver.GetRelationWithPlayer() < -10f)
			{
				flag = (PreconditionFlags)((uint)flag | 1u);
				relationHero = issueGiver;
			}
			if (FactionManager.IsAtWarAgainstFaction(issueGiver.CurrentSettlement.MapFaction, Hero.MainHero.MapFaction))
			{
				flag = (PreconditionFlags)((uint)flag | 0x40u);
			}
			if (((List<Hero>)(object)Clan.PlayerClan.Companions).Count >= Clan.PlayerClan.CompanionLimit)
			{
				flag = (PreconditionFlags)((uint)flag | 0x10000u);
			}
			return (int)flag == 0;
		}

		public override bool IssueStayAliveConditions()
		{
			if (_targetNotable != null && _targetNotable.IsActive)
			{
				if (((IssueBase)this).CounterOfferHero != null)
				{
					if (((IssueBase)this).CounterOfferHero.IsActive)
					{
						return ((IssueBase)this).CounterOfferHero.CurrentSettlement == ((IssueBase)this).IssueSettlement;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}

		internal static void AutoGeneratedStaticCollectObjectsFamilyFeudIssue(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(FamilyFeudIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((IssueBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_targetVillage);
			collectedObjects.Add(_targetNotable);
			collectedObjects.Add(((IssueBase)this).CounterOfferHero);
		}

		internal static object AutoGeneratedGetMemberValueCounterOfferHero(object o)
		{
			return ((IssueBase)(FamilyFeudIssue)o).CounterOfferHero;
		}

		internal static object AutoGeneratedGetMemberValue_targetVillage(object o)
		{
			return ((FamilyFeudIssue)o)._targetVillage;
		}

		internal static object AutoGeneratedGetMemberValue_targetNotable(object o)
		{
			return ((FamilyFeudIssue)o)._targetNotable;
		}
	}

	public class FamilyFeudIssueQuest : QuestBase
	{
		private const int CustomCulpritAgentHealth = 350;

		private const int CustomTargetNotableAgentHealth = 350;

		public const string CommonAreaTag = "alley_2";

		[SaveableField(10)]
		private readonly Settlement _targetSettlement;

		[SaveableField(20)]
		private Hero _targetNotable;

		[SaveableField(30)]
		private Hero _culprit;

		[SaveableField(40)]
		private bool _culpritJoinedPlayerParty;

		[SaveableField(50)]
		private bool _checkForMissionEvents;

		[SaveableField(70)]
		private int _rewardGold;

		private bool _isCulpritDiedInMissionFight;

		private bool _isPlayerKnockedOutMissionFight;

		private bool _isNotableKnockedDownInMissionFight;

		private bool _conversationAfterFightIsDone;

		private bool _persuationInDoneAndSuccessfull;

		private bool _playerBetrayedCulprit;

		private Agent _notableAgent;

		private Agent _culpritAgent;

		private CharacterObject _notableGangsterCharacterObject;

		private List<LocationCharacter> _notableThugs;

		private PersuasionTask _task;

		private const PersuasionDifficulty Difficulty = (PersuasionDifficulty)4;

		public override bool IsRemainingTimeHidden => false;

		private bool FightEnded
		{
			get
			{
				if (!_isCulpritDiedInMissionFight && !_isNotableKnockedDownInMissionFight)
				{
					return _persuationInDoneAndSuccessfull;
				}
				return true;
			}
		}

		public override TextObject Title => new TextObject("{=ZpDQxmzJ}Family Feud", (Dictionary<string, object>)null);

		private TextObject PlayerStartsQuestLogText1
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=rjHQpVDZ}{QUEST_GIVER.LINK} a landowner from {QUEST_GIVER_SETTLEMENT}, told you about an incident that is about to turn into an ugly feud. One of the youngsters killed another during a quarrel and the victim's family refuses the blood money as compensation and wants blood.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("QUEST_GIVER_SETTLEMENT", ((QuestBase)this).QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject PlayerStartsQuestLogText2
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=fgRq7kF2}You agreed to talk to {CULPRIT.LINK} in {QUEST_GIVER_SETTLEMENT} first and convince him to go to {TARGET_NOTABLE.LINK} with you in {TARGET_SETTLEMENT} and mediate the issue between them peacefully and end unnecessary bloodshed. {QUEST_GIVER.LINK} said {?QUEST_GIVER.GENDER}she{?}he{\\?} will pay you {REWARD_GOLD} once the boy is safe again.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("CULPRIT", _culprit.CharacterObject, val, false);
				val.SetTextVariable("QUEST_GIVER_SETTLEMENT", ((QuestBase)this).QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
				val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				val.SetTextVariable("REWARD_GOLD", _rewardGold);
				return val;
			}
		}

		private TextObject SuccessQuestSolutionLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=KJ61SXEU}You have successfully protected {CULPRIT.LINK} from harm as you have promised. {QUEST_GIVER.LINK} is grateful for your service and sends his regards with a purse full of {REWARD_GOLD}{GOLD_ICON} denars for your trouble.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("CULPRIT", _culprit.CharacterObject, val, false);
				val.SetTextVariable("REWARD_GOLD", _rewardGold);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				return val;
			}
		}

		private TextObject CulpritJoinedPlayerPartyLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=s5fXZf2f}You have convinced {CULPRIT.LINK} to go to {TARGET_SETTLEMENT} to face {TARGET_NOTABLE.LINK} to try to solve this issue peacefully. He agreed on the condition that you protect him from his victim's angry relatives.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("CULPRIT", _culprit.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject QuestGiverVillageRaidedBeforeTalkingToCulpritCancel
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=gJG0xmAq}{QUEST_GIVER.LINK}'s village {QUEST_SETTLEMENT} was raided. Your agreement with {QUEST_GIVER.LINK} is canceled.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("QUEST_SETTLEMENT", ((QuestBase)this).QuestGiver.CurrentSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject TargetVillageRaidedBeforeTalkingToCulpritCancel
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=WqY4nvHc}{TARGET_NOTABLE.LINK}'s village {TARGET_SETTLEMENT} was raided. Your agreement with {QUEST_GIVER.LINK} is canceled.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject CulpritDiedQuestFail
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=6zcG8eng}You tried to defend {CULPRIT.LINK} but you were overcome. {NOTABLE.LINK} took {?NOTABLE.GENDER}her{?}his{\\?} revenge. You failed to protect {CULPRIT.LINK} as promised to {QUEST_GIVER.LINK}. {?QUEST_GIVER.GENDER}she{?}he{\\?} is furious.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("CULPRIT", _culprit.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("NOTABLE", _targetNotable.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject PlayerDiedInNotableBattle
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=kG92fjCY}You fell unconscious while defending {CULPRIT.LINK}. {TARGET_NOTABLE.LINK} has taken revenge. You failed to protect {CULPRIT.LINK} as you promised {QUEST_GIVER.LINK}. {?QUEST_GIVER.GENDER}She{?}He{\\?} is furious.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("CULPRIT", _culprit.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject FailQuestLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=LWjIbTBi}You failed to protect {CULPRIT.LINK} as you promised {QUEST_GIVER.LINK}. {QUEST_GIVER.LINK} is furious.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("CULPRIT", _culprit.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject CulpritNoLongerAClanMember
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=wWrEvkuj}{CULPRIT.LINK} is no longer a member of your clan. Your agreement with {QUEST_GIVER.LINK} was terminated.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("CULPRIT", _culprit.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject CompanionLimitReachedQuestLogText => new TextObject("{=rkQ7D36f}The quest was canceled because your party had more companions than you could manage.", (Dictionary<string, object>)null);

		public FamilyFeudIssueQuest(string questId, Hero questGiver, CampaignTime duration, Settlement targetSettlement, Hero targetHero, int rewardGold)
			: base(questId, questGiver, duration, rewardGold)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			_targetSettlement = targetSettlement;
			_targetNotable = targetHero;
			_culpritJoinedPlayerParty = false;
			_checkForMissionEvents = false;
			_culprit = HeroCreator.CreateSpecialHero(MBObjectManager.Instance.GetObject<CharacterObject>("townsman_" + ((MBObjectBase)targetSettlement.Culture).StringId), targetSettlement, (Clan)null, (Clan)null, -1);
			_culprit.SetNewOccupation((Occupation)16);
			ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>("pugio");
			_culprit.CivilianEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)0, new EquipmentElement(val, (ItemModifier)null, (ItemObject)null, false));
			_culprit.BattleEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)0, new EquipmentElement(val, (ItemModifier)null, (ItemObject)null, false));
			_notableGangsterCharacterObject = questGiver.CurrentSettlement.MapFaction.Culture.GangleaderBodyguard;
			_rewardGold = rewardGold;
			InitializeQuestDialogs();
			((QuestBase)this).SetDialogs();
			((QuestBase)this).InitializeQuestOnCreation();
		}

		private void InitializeQuestDialogs()
		{
			Campaign.Current.ConversationManager.AddDialogFlow(GetCulpritDialogFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetNotableThugDialogFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlowBeforeTalkingToCulprit(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlowAfterTalkingToCulprit(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlowAfterKillingCulprit(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlowAfterPlayerBetrayCulprit(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetCulpritDialogFlowAfterCulpritJoin(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlowAfterNotableKnowdown(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetNotableDialogFlowAfterQuestEnd(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetCulpritDialogFlowAfterQuestEnd(), (object)this);
		}

		protected override void InitializeQuestOnGameLoad()
		{
			((QuestBase)this).SetDialogs();
			InitializeQuestDialogs();
			_notableGangsterCharacterObject = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_1");
		}

		protected override void HourlyTick()
		{
		}

		private DialogFlow GetNotableDialogFlowBeforeTalkingToCulprit()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=dpTHWqwv}Are you the {?PLAYER.GENDER}woman{?}man{\\?} who thinks our blood is cheap, that we will accept silver for the life of one of our own?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(notable_culprit_is_not_near_on_condition))
				.NpcLine(new TextObject("{=Vd22iVGE}Well {?PLAYER.GENDER}lady{?}sir{\\?}, sorry to disappoint you, but our people have some self-respect.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.PlayerLine(new TextObject("{=a3AFjfsU}We will see. ", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=AeJqCMJc}Yes, you will see. Good day to you. ", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog();
		}

		private DialogFlow GetNotableDialogFlowAfterKillingCulprit()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=108Dchvt}Stop! We don't need to fight any longer. We have no quarrel with you as justice has been served.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _isCulpritDiedInMissionFight && Hero.OneToOneConversationHero == _targetNotable && !_playerBetrayedCulprit))
				.NpcLine(new TextObject("{=NMrzr7Me}Now, leave peacefully...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += CulpritDiedInNotableFightFail;
				})
				.CloseDialog();
		}

		private DialogFlow GetNotableDialogFlowAfterPlayerBetrayCulprit()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=4aiabOd4}I knew you are a reasonable {?PLAYER.GENDER}woman{?}man{\\?}.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _isCulpritDiedInMissionFight && _playerBetrayedCulprit && Hero.OneToOneConversationHero == _targetNotable))
				.NpcLine(new TextObject("{=NMrzr7Me}Now, leave peacefully...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += CulpritDiedInNotableFightFail;
				})
				.CloseDialog();
		}

		private DialogFlow GetCulpritDialogFlowAfterCulpritJoin()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Expected O, but got Unknown
			TextObject val = new TextObject("{=56ynu2bW}Yes, {?PLAYER.GENDER}milady{?}sir{\\?}.", (Dictionary<string, object>)null);
			TextObject val2 = new TextObject("{=c452Kevh}Well I'm anxious, but I am in your hands now. I trust you will protect me {?PLAYER.GENDER}milady{?}sir{\\?}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val2, false);
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => !FightEnded && _culpritJoinedPlayerParty && Hero.OneToOneConversationHero == _culprit))
				.PlayerLine(new TextObject("{=p1ETQbzg}Just checking on you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(val2, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog();
		}

		private DialogFlow GetNotableDialogFlowAfterQuestEnd()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=UBFS1JLj}I have no problem with the boy anymore,[ib:closed][if:convo_annoyed] okay? Just leave me alone.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => FightEnded && !_persuationInDoneAndSuccessfull && Hero.OneToOneConversationHero == _targetNotable && !_playerBetrayedCulprit))
				.CloseDialog()
				.NpcLine(new TextObject("{=adbQR9j0}I got my gold, you got your boy.[if:convo_bored2] Now leave me alone...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)(() => FightEnded && _persuationInDoneAndSuccessfull && Hero.OneToOneConversationHero == _targetNotable && !_playerBetrayedCulprit))
				.CloseDialog();
		}

		private DialogFlow GetCulpritDialogFlowAfterQuestEnd()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=OybG76Kf}Thank you for saving me, sir.[ib:normal][if:convo_astonished] I won't forget what you did here today.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => FightEnded && Hero.OneToOneConversationHero == _culprit))
				.CloseDialog();
		}

		private DialogFlow GetNotableDialogFlowAfterNotableKnowdown()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_0036: Expected O, but got Unknown
			//IL_0036: Expected O, but got Unknown
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected O, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Expected O, but got Unknown
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Expected O, but got Unknown
			//IL_0082: Expected O, but got Unknown
			//IL_0082: Expected O, but got Unknown
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected O, but got Unknown
			//IL_00ac: Expected O, but got Unknown
			//IL_00ac: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			//IL_00d0: Expected O, but got Unknown
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Expected O, but got Unknown
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Expected O, but got Unknown
			//IL_00ff: Expected O, but got Unknown
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=c6GbRQlg}Stop. We don’t need to fight any longer. [ib:closed][if:convo_insulted]You have made your point. We will accept the blood money.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsTargetNotable(agent)), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsMainAgent(agent)), (string)null, (string)null).Condition(new OnConditionDelegate(multi_character_conversation_condition_after_fight))
				.Consequence(new OnConsequenceDelegate(multi_character_conversation_consequence_after_fight))
				.NpcLine(new TextObject("{=pS0bBRjt}You! Go to your family and tell [if:convo_angry]them to send us the blood money.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsTargetNotable(agent)), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsCulprit(agent)), (string)null, (string)null)
				.NpcLine(new TextObject("{=nxs2U0Yk}Leave now and never come back! [if:convo_furious]If we ever see you here we will kill you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsTargetNotable(agent)), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsCulprit(agent)), (string)null, (string)null)
				.NpcLine("{=udD7Y7mO}Thank you, my {?PLAYER.GENDER}lady{?}sir{\\?}, for protecting me. I will go and tell {ISSUE_GIVER.LINK} of your success here.", (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsCulprit(agent)), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsMainAgent(agent)), (string)null, (string)null)
				.Condition(new OnConditionDelegate(AfterNotableKnowdownEndingCondition))
				.PlayerLine(new TextObject("{=g8qb3Ame}Thank you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsCulprit(agent)), (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += PlayerAndCulpritKnockedDownNotableQuestSuccess;
				})
				.CloseDialog();
		}

		private bool AfterNotableKnowdownEndingCondition()
		{
			StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, (TextObject)null, false);
			return true;
		}

		private void PlayerAndCulpritKnockedDownNotableQuestSuccess()
		{
			_conversationAfterFightIsDone = true;
			HandleAgentBehaviorAfterQuestConversations();
		}

		private void HandleAgentBehaviorAfterQuestConversations()
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			foreach (AccompanyingCharacter item in PlayerEncounter.LocationEncounter.CharactersAccompanyingPlayer)
			{
				if (item.LocationCharacter.Character == _culprit.CharacterObject && _culpritAgent.IsActive())
				{
					item.LocationCharacter.SpecialTargetTag = "npc_common";
					item.LocationCharacter.CharacterRelation = (CharacterRelations)0;
					_culpritAgent.SetMortalityState((MortalityState)1);
					_culpritAgent.SetTeam(Team.Invalid, false);
					DailyBehaviorGroup behaviorGroup = _culpritAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
					behaviorGroup.AddBehavior<WalkingBehavior>();
					behaviorGroup.RemoveBehavior<FollowAgentBehavior>();
					_culpritAgent.ResetEnemyCaches();
					_culpritAgent.InvalidateTargetAgent();
					_culpritAgent.InvalidateAIWeaponSelections();
					_culpritAgent.SetWatchState((WatchState)0);
					if (_notableAgent != null)
					{
						_notableAgent.ResetEnemyCaches();
						_notableAgent.InvalidateTargetAgent();
						_notableAgent.InvalidateAIWeaponSelections();
						_notableAgent.SetWatchState((WatchState)0);
					}
					_culpritAgent.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)3);
					_culpritAgent.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)3);
				}
			}
			Mission.Current.SetMissionMode((MissionMode)0, false);
		}

		private void ApplySuccessConsequences()
		{
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, _rewardGold, false);
			GainRenownAction.Apply(Hero.MainHero, 1f, false);
			((QuestBase)this).RelationshipChangeWithQuestGiver = 10;
			ChangeRelationAction.ApplyPlayerRelation(_targetNotable, -5, true, true);
			Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
			town.Security += 10f;
			((QuestBase)this).CompleteQuestWithSuccess();
		}

		private bool multi_character_conversation_condition_after_fight()
		{
			if (!_conversationAfterFightIsDone && Hero.OneToOneConversationHero == _targetNotable)
			{
				return _isNotableKnockedDownInMissionFight;
			}
			return false;
		}

		private void multi_character_conversation_consequence_after_fight()
		{
			if (Mission.Current.GetMissionBehavior<MissionConversationLogic>() != null)
			{
				Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<Agent> { _culpritAgent }, true);
			}
			_conversationAfterFightIsDone = true;
		}

		private DialogFlow GetNotableDialogFlowAfterTalkingToCulprit()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_0036: Expected O, but got Unknown
			//IL_0036: Expected O, but got Unknown
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Expected O, but got Unknown
			//IL_0071: Expected O, but got Unknown
			//IL_0071: Expected O, but got Unknown
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Expected O, but got Unknown
			//IL_0095: Expected O, but got Unknown
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Expected O, but got Unknown
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Expected O, but got Unknown
			//IL_00d1: Expected O, but got Unknown
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Expected O, but got Unknown
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Expected O, but got Unknown
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Expected O, but got Unknown
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Expected O, but got Unknown
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Expected O, but got Unknown
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Expected O, but got Unknown
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Expected O, but got Unknown
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Expected O, but got Unknown
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Expected O, but got Unknown
			//IL_01a8: Expected O, but got Unknown
			//IL_01a8: Expected O, but got Unknown
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Expected O, but got Unknown
			DialogFlow val = DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=nh7a3Nog}Well well. Who did you bring to see us? [ib:confident][if:convo_irritable]Did he bring his funeral shroud with him? I hope so. He's not leaving here alive.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsTargetNotable(agent)), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsCulprit(agent)), (string)null, (string)null).Condition(new OnConditionDelegate(multi_character_conversation_on_condition))
				.NpcLine(new TextObject("{=RsOmvdmU}We have come to talk! Just listen to us please![if:convo_shocked]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsCulprit(agent)), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsTargetNotable(agent)), (string)null, (string)null)
				.NpcLine("{=JUjvu4XL}I knew we'd find you eventually. Now you will face justice![if:convo_evil_smile]", (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsTargetNotable(agent)), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsCulprit(agent)), (string)null, (string)null)
				.PlayerLine("{=UQyCoQCY}Wait! This lad is now under my protection. We have come to talk in peace..", (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsTargetNotable(agent)), (string)null, (string)null)
				.NpcLine("{=7AiP4BwY}What there is to talk about? [if:convo_confused_annoyed]This bastard murdered one of my kinsman, and it is our right to take vengeance on him!", (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsTargetNotable(agent)), (OnMultipleConversationConsequenceDelegate)((IAgent agent) => IsMainAgent(agent)), (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(new TextObject("{=2iVytG2y}I am not convinced. I will protect the accused until you see reason.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=4HokUcma}You will regret pushing [if:convo_very_stern]your nose into issues that do not concern you!", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=vjOkDM6C}If you defend a murderer [ib:warrior][if:convo_furious]then you die like a murderer. Boys, kill them all!", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
					{
						StartFightWithNotableGang(playerBetrayedCulprit: false);
					};
				})
				.CloseDialog()
				.PlayerOption(new TextObject("{=boAcQxVV}You're breaking the law.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)(() => _task == null || !((IEnumerable<PersuasionOptionArgs>)_task.Options).All((PersuasionOptionArgs x) => x.IsBlocked)))
				.GotoDialogState("start_notable_family_feud_persuasion")
				.PlayerOption(new TextObject("{=J5cQPqGQ}You are right. You are free to deliver justice as you see fit.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=aRPLW15x}Thank you. I knew you are a reasonable[ib:aggressive][if:convo_evil_smile] {?PLAYER.GENDER}woman{?}man{\\?}.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=k5R4qGtL}What? Are you just going [ib:nervous][if:convo_nervous2]to leave me here to be killed? My kin will never forget this!", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsCulprit), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
					{
						_playerBetrayedCulprit = true;
						StartFightWithNotableGang(_playerBetrayedCulprit);
					};
				})
				.CloseDialog();
			AddPersuasionDialogs(val);
			return val;
		}

		private bool IsMainAgent(IAgent agent)
		{
			return (object)agent == Mission.Current.MainAgent;
		}

		private bool IsTargetNotable(IAgent agent)
		{
			return (object)agent.Character == _targetNotable.CharacterObject;
		}

		private bool IsCulprit(IAgent agent)
		{
			return (object)agent.Character == _culprit.CharacterObject;
		}

		private bool notable_culprit_is_not_near_on_condition()
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (Hero.OneToOneConversationHero != _targetNotable || Mission.Current == null || FightEnded)
			{
				return false;
			}
			Mission current = Mission.Current;
			Vec3 position = Agent.Main.Position;
			return ((IEnumerable<Agent>)current.GetNearbyAgents(((Vec3)(ref position)).AsVec2, 10f, new MBList<Agent>())).All((Agent a) => (object)a.Character != _culprit.CharacterObject);
		}

		private bool multi_character_conversation_on_condition()
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (Hero.OneToOneConversationHero != _targetNotable || Mission.Current == null || FightEnded)
			{
				return false;
			}
			Mission current = Mission.Current;
			Vec3 position = Agent.Main.Position;
			MBList<Agent> nearbyAgents = current.GetNearbyAgents(((Vec3)(ref position)).AsVec2, 10f, new MBList<Agent>());
			if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)nearbyAgents) || ((IEnumerable<Agent>)nearbyAgents).All((Agent a) => (object)a.Character != _culprit.CharacterObject))
			{
				return false;
			}
			foreach (Agent item in (List<Agent>)(object)nearbyAgents)
			{
				if ((object)item.Character == _culprit.CharacterObject)
				{
					_culpritAgent = item;
					if (Mission.Current.GetMissionBehavior<MissionConversationLogic>() != null)
					{
						Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<Agent> { _culpritAgent }, true);
					}
					break;
				}
			}
			return true;
		}

		private void AddPersuasionDialogs(DialogFlow dialog)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			//IL_0066: Expected O, but got Unknown
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Expected O, but got Unknown
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Expected O, but got Unknown
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Expected O, but got Unknown
			//IL_00fe: Expected O, but got Unknown
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Expected O, but got Unknown
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Expected O, but got Unknown
			//IL_0152: Expected O, but got Unknown
			//IL_0152: Expected O, but got Unknown
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Expected O, but got Unknown
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Expected O, but got Unknown
			//IL_01a6: Expected O, but got Unknown
			//IL_01a6: Expected O, but got Unknown
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Expected O, but got Unknown
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Expected O, but got Unknown
			//IL_01fa: Expected O, but got Unknown
			//IL_01fa: Expected O, but got Unknown
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Expected O, but got Unknown
			//IL_0233: Expected O, but got Unknown
			dialog.AddDialogLine("family_feud_notable_persuasion_check_accepted", "start_notable_family_feud_persuasion", "family_feud_notable_persuasion_start_reservation", "{=6P1ruzsC}Maybe...", (OnConditionDelegate)null, new OnConsequenceDelegate(persuasion_start_with_notable_on_consequence), (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			dialog.AddDialogLine("family_feud_notable_persuasion_failed", "family_feud_notable_persuasion_start_reservation", "persuation_failed", "{=!}{FAILED_PERSUASION_LINE}", new OnConditionDelegate(persuasion_failed_with_family_feud_notable_on_condition), new OnConsequenceDelegate(persuasion_failed_with_notable_on_consequence), (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			dialog.AddDialogLine("family_feud_notable_persuasion_rejected", "persuation_failed", "close_window", "{=vjOkDM6C}If you defend a murderer [ib:warrior][if:convo_furious]then you die like a murderer. Boys, kill them all!", (OnConditionDelegate)null, new OnConsequenceDelegate(persuasion_failed_with_notable_start_fight_on_consequence), (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			dialog.AddDialogLine("family_feud_notable_persuasion_attempt", "family_feud_notable_persuasion_start_reservation", "family_feud_notable_persuasion_select_option", "{CONTINUE_PERSUASION_LINE}", (OnConditionDelegate)(() => !persuasion_failed_with_family_feud_notable_on_condition()), (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			dialog.AddDialogLine("family_feud_notable_persuasion_success", "family_feud_notable_persuasion_start_reservation", "close_window", "{=qIQbIjVS}All right! I spare the boy's life. Now get out of my sight[ib:closed][if:convo_nonchalant]", new OnConditionDelegate(ConversationManager.GetPersuasionProgressSatisfied), new OnConsequenceDelegate(persuasion_complete_with_notable_on_consequence), (object)this, int.MaxValue, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			OnConditionDelegate val = persuasion_select_option_1_on_condition;
			OnConsequenceDelegate val2 = persuasion_select_option_1_on_consequence;
			OnPersuasionOptionDelegate val3 = new OnPersuasionOptionDelegate(persuasion_setup_option_1);
			OnClickableConditionDelegate val4 = new OnClickableConditionDelegate(persuasion_clickable_option_1_on_condition);
			dialog.AddPlayerLine("family_feud_notable_persuasion_select_option_1", "family_feud_notable_persuasion_select_option", "family_feud_notable_persuasion_selected_option_response", "{=!}{FAMILY_FEUD_PERSUADE_ATTEMPT_1}", val, val2, (object)this, 100, val4, val3, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			OnConditionDelegate val5 = persuasion_select_option_2_on_condition;
			OnConsequenceDelegate val6 = persuasion_select_option_2_on_consequence;
			val3 = new OnPersuasionOptionDelegate(persuasion_setup_option_2);
			val4 = new OnClickableConditionDelegate(persuasion_clickable_option_2_on_condition);
			dialog.AddPlayerLine("family_feud_notable_persuasion_select_option_2", "family_feud_notable_persuasion_select_option", "family_feud_notable_persuasion_selected_option_response", "{=!}{FAMILY_FEUD_PERSUADE_ATTEMPT_2}", val5, val6, (object)this, 100, val4, val3, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			OnConditionDelegate val7 = persuasion_select_option_3_on_condition;
			OnConsequenceDelegate val8 = persuasion_select_option_3_on_consequence;
			val3 = new OnPersuasionOptionDelegate(persuasion_setup_option_3);
			val4 = new OnClickableConditionDelegate(persuasion_clickable_option_3_on_condition);
			dialog.AddPlayerLine("family_feud_notable_persuasion_select_option_3", "family_feud_notable_persuasion_select_option", "family_feud_notable_persuasion_selected_option_response", "{=!}{FAMILY_FEUD_PERSUADE_ATTEMPT_3}", val7, val8, (object)this, 100, val4, val3, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
			dialog.AddDialogLine("family_feud_notable_persuasion_select_option_reaction", "family_feud_notable_persuasion_selected_option_response", "family_feud_notable_persuasion_start_reservation", "{=D0xDRqvm}{PERSUASION_REACTION}", new OnConditionDelegate(persuasion_selected_option_response_on_condition), new OnConsequenceDelegate(persuasion_selected_option_response_on_consequence), (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		}

		private void persuasion_complete_with_notable_on_consequence()
		{
			ConversationManager.EndPersuasion();
			_persuationInDoneAndSuccessfull = true;
			HandleAgentBehaviorAfterQuestConversations();
		}

		private void persuasion_failed_with_notable_on_consequence()
		{
			ConversationManager.EndPersuasion();
		}

		private void persuasion_failed_with_notable_start_fight_on_consequence()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
			{
				StartFightWithNotableGang(playerBetrayedCulprit: false);
			};
		}

		private bool persuasion_failed_with_family_feud_notable_on_condition()
		{
			MBTextManager.SetTextVariable("CONTINUE_PERSUASION_LINE", "{=7B7BhVhV}Let's see what you will come up with...[if:convo_confused_annoyed]", false);
			if (((IEnumerable<PersuasionOptionArgs>)_task.Options).Any((PersuasionOptionArgs x) => x.IsBlocked))
			{
				MBTextManager.SetTextVariable("CONTINUE_PERSUASION_LINE", "{=wvbiyZfp}What else do you have to say?[if:convo_confused_annoyed]", false);
			}
			if (((IEnumerable<PersuasionOptionArgs>)_task.Options).All((PersuasionOptionArgs x) => x.IsBlocked) && !ConversationManager.GetPersuasionProgressSatisfied())
			{
				MBTextManager.SetTextVariable("FAILED_PERSUASION_LINE", _task.FinalFailLine, false);
				return true;
			}
			return false;
		}

		private void persuasion_selected_option_response_on_consequence()
		{
			Tuple<PersuasionOptionArgs, PersuasionOptionResult> tuple = ConversationManager.GetPersuasionChosenOptions().Last();
			float difficulty = Campaign.Current.Models.PersuasionModel.GetDifficulty((PersuasionDifficulty)4);
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

		private void persuasion_start_with_notable_on_consequence()
		{
			_task = GetPersuasionTask();
			ConversationManager.StartPersuasion(2f, 1f, 0f, 2f, 2f, 0f, (PersuasionDifficulty)4);
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
				MBTextManager.SetTextVariable("FAMILY_FEUD_PERSUADE_ATTEMPT_1", val, false);
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
				MBTextManager.SetTextVariable("FAMILY_FEUD_PERSUADE_ATTEMPT_2", val, false);
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
				MBTextManager.SetTextVariable("FAMILY_FEUD_PERSUADE_ATTEMPT_3", val, false);
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
			if (((IEnumerable<PersuasionOptionArgs>)_task.Options).Any())
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

		private PersuasionTask GetPersuasionTask()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected O, but got Unknown
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Expected O, but got Unknown
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Expected O, but got Unknown
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Expected O, but got Unknown
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Expected O, but got Unknown
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Expected O, but got Unknown
			PersuasionTask val = new PersuasionTask(0)
			{
				FinalFailLine = new TextObject("{=rzGqa5oD}Revenge will be taken. Save your breath for the fight...", (Dictionary<string, object>)null),
				TryLaterLine = new TextObject("{=!}IF YOU SEE THIS. CALL CAMPAIGN TEAM.", (Dictionary<string, object>)null),
				SpokenLine = new TextObject("{=6P1ruzsC}Maybe...", (Dictionary<string, object>)null)
			};
			PersuasionOptionArgs val2 = new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Calculating, (TraitEffect)0, (PersuasionArgumentStrength)1, false, new TextObject("{=K9i5SaDc}Blood money is appropriate for a crime of passion. But you kill this boy in cold blood, you will be a real murderer in the eyes of the law, and will no doubt die.", (Dictionary<string, object>)null), (Tuple<TraitObject, int>[])null, false, false, false);
			val.AddOptionToTask(val2);
			PersuasionOptionArgs val3 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Valor, (TraitEffect)0, (PersuasionArgumentStrength)(-3), true, new TextObject("{=FUL8TcYa}I promised to protect the boy at the cost of my life. If you try to harm him, you will bleed for it.", (Dictionary<string, object>)null), (Tuple<TraitObject, int>[])null, true, false, false);
			val.AddOptionToTask(val3);
			PersuasionOptionArgs val4 = new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Mercy, (TraitEffect)0, (PersuasionArgumentStrength)0, false, new TextObject("{=Ytws5O9S}Some day you may wish to save the life of one of your sons through blood money. If you refuse mercy, mercy may be refused you.", (Dictionary<string, object>)null), (Tuple<TraitObject, int>[])null, false, false, false);
			val.AddOptionToTask(val4);
			return val;
		}

		private void StartFightWithNotableGang(bool playerBetrayedCulprit)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Invalid comparison between Unknown and O
			_notableAgent = (Agent)MissionConversationLogic.Current.ConversationManager.ConversationAgents[0];
			List<Agent> list = new List<Agent> { _culpritAgent };
			List<Agent> list2 = new List<Agent> { _notableAgent };
			MBList<Agent> val = new MBList<Agent>();
			Mission current = Mission.Current;
			Vec3 position = Agent.Main.Position;
			foreach (Agent item in (List<Agent>)(object)current.GetNearbyAgents(((Vec3)(ref position)).AsVec2, 30f, val))
			{
				if ((object)(CharacterObject)item.Character == _notableGangsterCharacterObject)
				{
					list2.Add(item);
				}
			}
			if (playerBetrayedCulprit)
			{
				Agent.Main.SetTeam(Mission.Current.SpectatorTeam, false);
			}
			else
			{
				list.Add(Agent.Main);
				foreach (Agent item2 in list2)
				{
					item2.Defensiveness = 2f;
				}
				_culpritAgent.Health = 350f;
				_culpritAgent.BaseHealthLimit = 350f;
				_culpritAgent.HealthLimit = 350f;
			}
			_notableAgent.Health = 350f;
			_notableAgent.BaseHealthLimit = 350f;
			_notableAgent.HealthLimit = 350f;
			Mission.Current.GetMissionBehavior<MissionFightHandler>().StartCustomFight(list, list2, dropWeapons: false, isItemUseDisabled: false, delegate
			{
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0067: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				Vec3 position2;
				if (_isNotableKnockedDownInMissionFight)
				{
					if (Agent.Main != null)
					{
						position2 = _notableAgent.Position;
						if (((Vec3)(ref position2)).DistanceSquared(Agent.Main.Position) < 49f)
						{
							MissionConversationLogic.Current.StartConversation(_notableAgent, setActionsInstantly: false);
							return;
						}
					}
					PlayerAndCulpritKnockedDownNotableQuestSuccess();
				}
				else
				{
					if (Agent.Main != null)
					{
						position2 = _notableAgent.Position;
						if (((Vec3)(ref position2)).DistanceSquared(Agent.Main.Position) < 49f)
						{
							MissionConversationLogic.Current.StartConversation(_notableAgent, setActionsInstantly: false);
							return;
						}
					}
					CulpritDiedInNotableFightFail();
				}
			});
		}

		private void OnAgentHit(Agent affectedAgent, Agent affectorAgent, int damage)
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			if (((QuestBase)this).IsOngoing && !_persuationInDoneAndSuccessfull && affectedAgent.Health <= (float)damage && Agent.Main != null)
			{
				if (affectedAgent == _notableAgent && !_isNotableKnockedDownInMissionFight)
				{
					affectedAgent.Health = 50f;
					_isNotableKnockedDownInMissionFight = true;
					Mission.Current.GetMissionBehavior<MissionFightHandler>().EndFight();
				}
				if (affectedAgent == _culpritAgent && !_isCulpritDiedInMissionFight)
				{
					Blow val = new Blow
					{
						DamageCalculated = true,
						BaseMagnitude = damage,
						InflictedDamage = damage,
						DamagedPercentage = 1f,
						OwnerId = ((affectorAgent != null) ? affectorAgent.Index : (-1))
					};
					affectedAgent.Die(val, (KillInfo)(-1));
					_isCulpritDiedInMissionFight = true;
				}
			}
		}

		protected override void SetDialogs()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected O, but got Unknown
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Expected O, but got Unknown
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Expected O, but got Unknown
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Expected O, but got Unknown
			base.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(new TextObject("{=JjXETjYb}Thank you.[ib:demure][if:convo_thinking] I have to add, I'm ready to pay you {REWARD_GOLD}{GOLD_ICON} denars for your trouble. He is hiding somewhere nearby. Go talk to him, and tell him that you're here to sort things out.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
			{
				MBTextManager.SetTextVariable("REWARD_GOLD", _rewardGold);
				MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">", false);
				return Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver;
			})
				.Consequence(new OnConsequenceDelegate(QuestAcceptedConsequences))
				.CloseDialog();
			base.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("{=ndDpjT8s}Have you been able to talk with my boy yet?[if:convo_innocent_smile]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver))
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(new TextObject("{=ETiAbgHa}I will talk with them right away", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=qmqTLZ9R}Thank you {?PLAYER.GENDER}madam{?}sir{\\?}. You are a savior.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.PlayerOption(new TextObject("{=18NtjryL}Not yet, but I will soon.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=HeIIW3EH}We are waiting for your good news {?PLAYER.GENDER}milady{?}sir{\\?}.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private void QuestAcceptedConsequences()
		{
			((QuestBase)this).StartQuest();
			((QuestBase)this).AddLog(PlayerStartsQuestLogText1, false);
			((QuestBase)this).AddLog(PlayerStartsQuestLogText2, false);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_targetNotable);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_culprit);
			Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("village_center");
			Settlement.CurrentSettlement.LocationComplex.ChangeLocation(CreateCulpritLocationCharacter(Settlement.CurrentSettlement.Culture, (CharacterRelations)0), (Location)null, locationWithId);
		}

		private DialogFlow GetCulpritDialogFlow()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Expected O, but got Unknown
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Expected O, but got Unknown
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Expected O, but got Unknown
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Expected O, but got Unknown
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Expected O, but got Unknown
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(new TextObject("{=w0HPC53e}Who are you? What do you want from me?[ib:nervous][if:convo_bared_teeth]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => !_culpritJoinedPlayerParty && Hero.OneToOneConversationHero == _culprit))
				.PlayerLine(new TextObject("{=UGTCe2qP}Relax. I've talked with your relative, {QUEST_GIVER.NAME}. I know all about your situation. I'm here to help.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)delegate
				{
					StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, (TextObject)null, false);
					return Hero.OneToOneConversationHero == _culprit;
				})
				.Consequence((OnConsequenceDelegate)delegate
				{
					_culprit.SetHasMet();
				})
				.NpcLine(new TextObject("{=45llLiYG}How will you help? Will you protect me?[ib:normal][if:convo_astonished]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.PlayerLine(new TextObject("{=4mwSvCgG}Yes I will. Come now, I will take you with me to {TARGET_NOTABLE.NAME} to resolve this issue peacefully.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)delegate
				{
					StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, (TextObject)null, false);
					return Hero.OneToOneConversationHero == _culprit;
				})
				.NpcLine(new TextObject("{=bHRZhYzd}No! I won't go anywhere near them! They'll kill me![ib:closed2][if:convo_stern]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.PlayerLine(new TextObject("{=sakSp6H8}You can't hide in the shadows forever. I pledge on my honor to protect you if things turn ugly.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=4CFOH0kB}I'm still not sure about all this, but I suppose you're right that I don't have much choice. Let's go get this over.[ib:closed][if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += CulpritJoinedPlayersArmy;
				})
				.CloseDialog();
		}

		private DialogFlow GetNotableThugDialogFlow()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected O, but got Unknown
			TextObject val = new TextObject("{=QMaYa25R}If you dare to even breathe a word against {TARGET_NOTABLE.LINK},[ib:aggressive2][if:convo_furious] it will be your last. You got it scum?", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val, false);
			TextObject val2 = new TextObject("{=vGnY4KBO}I care very little for your threats. My business is with {TARGET_NOTABLE.LINK}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("TARGET_NOTABLE", _targetNotable.CharacterObject, val2, false);
			return DialogFlow.CreateDialogFlow("start", 125).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _notableThugs != null && _notableThugs.Exists((LocationCharacter x) => x.AgentOrigin == Campaign.Current.ConversationManager.ConversationAgents[0].Origin)))
				.PlayerLine(val2, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog();
		}

		private void CulpritJoinedPlayersArmy()
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			_culprit.ChangeState((CharacterStates)1);
			AddCompanionAction.Apply(Clan.PlayerClan, _culprit);
			AddHeroToPartyAction.Apply(_culprit, MobileParty.MainParty, true);
			((QuestBase)this).AddLog(CulpritJoinedPlayerPartyLogText, false);
			if (Mission.Current != null)
			{
				DailyBehaviorGroup behaviorGroup = ((Agent)MissionConversationLogic.Current.ConversationManager.ConversationAgents[0]).GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
				FollowAgentBehavior followAgentBehavior = behaviorGroup.AddBehavior<FollowAgentBehavior>();
				behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
				followAgentBehavior.SetTargetAgent(Agent.Main);
			}
			_culpritJoinedPlayerParty = true;
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
			CampaignEvents.VillageBeingRaided.AddNonSerializedListener((object)this, (Action<Village>)OnVillageRaid);
			CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener((object)this, (Action)OnBeforeMissionOpened);
			CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
			CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnd);
			CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionStarted);
			CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
			CampaignEvents.CompanionRemoved.AddNonSerializedListener((object)this, (Action<Hero, RemoveCompanionDetail>)OnCompanionRemoved);
			CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener((object)this, (Action<PartyBase, Hero>)OnPrisonerTaken);
			CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
			CampaignEvents.MapEventStarted.AddNonSerializedListener((object)this, (Action<MapEvent, PartyBase, PartyBase>)OnMapEventStarted);
			CampaignEvents.CanMoveToSettlementEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanMoveToSettlement);
			CampaignEvents.CanHeroDieEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, KillCharacterActionDetail, bool>)CanHeroDie);
			CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
			CampaignEvents.PerkResetEvent.AddNonSerializedListener((object)this, (Action<Hero, PerkObject>)OnPerksReset);
			CampaignEvents.NewCompanionAdded.AddNonSerializedListener((object)this, (Action<Hero>)OnNewCompanionAdded);
			CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
		}

		private void OnGameLoadFinished()
		{
			CheckCompanionLimit();
		}

		private void OnNewCompanionAdded(Hero hero)
		{
			CheckCompanionLimit();
		}

		private void OnPerksReset(Hero hero, PerkObject perk)
		{
			if (hero == Hero.MainHero)
			{
				CheckCompanionLimit();
			}
		}

		private void CheckCompanionLimit()
		{
			if (((List<Hero>)(object)Clan.PlayerClan.Companions).Count > Clan.PlayerClan.CompanionLimit)
			{
				((QuestBase)this).AddLog(CompanionLimitReachedQuestLogText, false);
				((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
			}
		}

		private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			if (!_culpritJoinedPlayerParty && Settlement.CurrentSettlement == ((QuestBase)this).QuestGiver.CurrentSettlement)
			{
				Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("village_center").AddLocationCharacters(new CreateLocationCharacterDelegate(CreateCulpritLocationCharacter), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, 1);
			}
		}

		private void CanMoveToSettlement(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanBeSelectedInInventoryInfoIsRequested(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanHavePartyRoleOrBeGovernorInfoIsRequested(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanLeadPartyInfoIsRequested(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			CommonRestrictionInfoIsRequested(hero, ref result);
		}

		private void CommonRestrictionInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _culprit || _targetNotable == hero)
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

		private void CanHeroDie(Hero hero, KillCharacterActionDetail causeOfDeath, ref bool result)
		{
			if (hero == _targetNotable)
			{
				result = false;
			}
			else if (hero == Hero.MainHero && Settlement.CurrentSettlement == _targetSettlement && Mission.Current != null)
			{
				result = false;
			}
		}

		private void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if (victim == _targetNotable)
			{
				TextObject val = (((int)detail == 8) ? ((QuestBase)this).TargetHeroDisappearedLogText : ((QuestBase)this).TargetHeroDiedLogText);
				StringHelpers.SetCharacterProperties("QUEST_TARGET", _targetNotable.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				((QuestBase)this).AddLog(val, false);
				((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
			}
		}

		private void OnPrisonerTaken(PartyBase capturer, Hero prisoner)
		{
			if (prisoner == _culprit)
			{
				((QuestBase)this).AddLog(FailQuestLogText, false);
				TiemoutFailConsequences();
				((QuestBase)this).CompleteQuestWithFail((TextObject)null);
			}
		}

		private void OnVillageRaid(Village village)
		{
			if (village == _targetSettlement.Village)
			{
				((QuestBase)this).AddLog(TargetVillageRaidedBeforeTalkingToCulpritCancel, false);
				((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
			}
			else if (village == ((QuestBase)this).QuestGiver.CurrentSettlement.Village && !_culpritJoinedPlayerParty)
			{
				((QuestBase)this).AddLog(QuestGiverVillageRaidedBeforeTalkingToCulpritCancel, false);
				((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
			}
		}

		private void OnCompanionRemoved(Hero companion, RemoveCompanionDetail detail)
		{
			if (((QuestBase)this).IsOngoing && !_isCulpritDiedInMissionFight && !_isPlayerKnockedOutMissionFight && companion == _culprit)
			{
				((QuestBase)this).AddLog(CulpritNoLongerAClanMember, false);
				TiemoutFailConsequences();
				((QuestBase)this).CompleteQuestWithFail((TextObject)null);
			}
		}

		public void OnMissionStarted(IMission iMission)
		{
			if (!_checkForMissionEvents)
			{
				return;
			}
			if (PlayerEncounter.LocationEncounter.CharactersAccompanyingPlayer.All((AccompanyingCharacter x) => x.LocationCharacter.Character != _culprit.CharacterObject))
			{
				LocationCharacter locationCharacterOfHero = LocationComplex.Current.GetLocationCharacterOfHero(_culprit);
				if (locationCharacterOfHero != null)
				{
					PlayerEncounter.LocationEncounter.AddAccompanyingCharacter(locationCharacterOfHero, true);
				}
			}
			FamilyFeudIssueMissionBehavior familyFeudIssueMissionBehavior = new FamilyFeudIssueMissionBehavior(OnAgentHit);
			Mission.Current.AddMissionBehavior((MissionBehavior)(object)familyFeudIssueMissionBehavior);
			Mission.Current.GetMissionBehavior<MissionConversationLogic>().SetSpawnArea("alley_2");
		}

		private void OnMissionEnd(IMission mission)
		{
			if (!_checkForMissionEvents)
			{
				return;
			}
			_notableAgent = null;
			_culpritAgent = null;
			if (Agent.Main == null)
			{
				((QuestBase)this).AddLog(PlayerDiedInNotableBattle, false);
				((QuestBase)this).RelationshipChangeWithQuestGiver = -10;
				Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town.Prosperity -= 5f;
				Town town2 = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town2.Security -= 5f;
				_isPlayerKnockedOutMissionFight = true;
				((QuestBase)this).CompleteQuestWithFail((TextObject)null);
			}
			else if (_isCulpritDiedInMissionFight)
			{
				if (_playerBetrayedCulprit)
				{
					((QuestBase)this).AddLog(FailQuestLogText, false);
					TraitLevelingHelper.OnIssueSolvedThroughBetrayal(Hero.MainHero, new Tuple<TraitObject, int>[1]
					{
						new Tuple<TraitObject, int>(DefaultTraits.Honor, -50)
					});
					ChangeRelationAction.ApplyPlayerRelation(_targetNotable, 5, true, true);
				}
				else
				{
					((QuestBase)this).AddLog(CulpritDiedQuestFail, false);
				}
				((QuestBase)this).RelationshipChangeWithQuestGiver = -10;
				Town town3 = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town3.Prosperity -= 5f;
				Town town4 = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
				town4.Security -= 5f;
				((QuestBase)this).CompleteQuestWithFail((TextObject)null);
			}
			else if (_persuationInDoneAndSuccessfull)
			{
				((QuestBase)this).AddLog(SuccessQuestSolutionLogText, false);
				ApplySuccessConsequences();
			}
			else if (_isNotableKnockedDownInMissionFight)
			{
				((QuestBase)this).AddLog(SuccessQuestSolutionLogText, false);
				ApplySuccessConsequences();
			}
		}

		private void OnGameMenuOpened(MenuCallbackArgs args)
		{
			if (_culpritJoinedPlayerParty && Hero.MainHero.CurrentSettlement == _targetSettlement)
			{
				_checkForMissionEvents = args.MenuContext.GameMenu.StringId == "village";
			}
		}

		public void OnSettlementLeft(MobileParty party, Settlement settlement)
		{
			if (party == MobileParty.MainParty)
			{
				if (settlement == _targetSettlement)
				{
					_checkForMissionEvents = false;
				}
				if (settlement == ((QuestBase)this).QuestGiver.CurrentSettlement && _culpritJoinedPlayerParty && !((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)_targetSettlement))
				{
					((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_targetSettlement);
				}
			}
		}

		public void OnBeforeMissionOpened()
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Expected O, but got Unknown
			if (!_checkForMissionEvents)
			{
				return;
			}
			Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("village_center");
			if (locationWithId != null)
			{
				locationWithId.GetLocationCharacter(_targetNotable).SpecialTargetTag = "alley_2";
				if (_notableThugs == null)
				{
					_notableThugs = new List<LocationCharacter>();
				}
				else
				{
					_notableThugs.Clear();
				}
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateNotablesThugs), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, MathF.Ceiling(Campaign.Current.Models.IssueModel.GetIssueDifficultyMultiplier() * 3f));
			}
		}

		private LocationCharacter CreateCulpritLocationCharacter(CultureObject culture, CharacterRelations relation)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)_culprit.CharacterObject).Race, "_settlement");
			Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, ((BasicCharacterObject)_culprit.CharacterObject).IsFemale, "_villager"), monsterWithSuffix);
			AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)_culprit.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2);
			IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
			return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddFirstCompanionBehavior), "alley_2", true, relation, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
		}

		private LocationCharacter CreateNotablesThugs(CultureObject culture, CharacterRelations relation)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Expected O, but got Unknown
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)_notableGangsterCharacterObject).Race, "_settlement");
			Tuple<string, Monster> tuple = new Tuple<string, Monster>(ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, ((BasicCharacterObject)_notableGangsterCharacterObject).IsFemale, "_villain"), monsterWithSuffix);
			AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)_notableGangsterCharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2);
			IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
			LocationCharacter val = new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddFixedCharacterBehaviors), "alley_2", true, relation, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
			_notableThugs.Add(val);
			return val;
		}

		private void OnMapEventEnded(MapEvent mapEvent)
		{
			if (mapEvent.IsPlayerMapEvent && _culpritJoinedPlayerParty && !((List<TroopRosterElement>)(object)MobileParty.MainParty.MemberRoster.GetTroopRoster()).Exists((Predicate<TroopRosterElement>)((TroopRosterElement x) => x.Character == _culprit.CharacterObject)))
			{
				((QuestBase)this).AddLog(FailQuestLogText, false);
				TiemoutFailConsequences();
				((QuestBase)this).CompleteQuestWithFail((TextObject)null);
			}
		}

		private void CulpritDiedInNotableFightFail()
		{
			_conversationAfterFightIsDone = true;
			HandleAgentBehaviorAfterQuestConversations();
		}

		protected override void OnFinalize()
		{
			if (_culprit.IsPlayerCompanion)
			{
				if (_culprit.IsPrisoner)
				{
					EndCaptivityAction.ApplyByEscape(_culprit, (Hero)null, false);
				}
				RemoveCompanionAction.ApplyAfterQuest(Clan.PlayerClan, _culprit);
			}
			if (_culprit.IsAlive)
			{
				_culprit.Clan = null;
				KillCharacterAction.ApplyByRemove(_culprit, false, true);
			}
		}

		protected override void OnTimedOut()
		{
			((QuestBase)this).AddLog(FailQuestLogText, false);
			TiemoutFailConsequences();
		}

		private void TiemoutFailConsequences()
		{
			TraitLevelingHelper.OnIssueSolvedThroughBetrayal(((QuestBase)this).QuestGiver, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -50)
			});
			((QuestBase)this).RelationshipChangeWithQuestGiver = -10;
			Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
			town.Prosperity -= 5f;
			Town town2 = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
			town2.Security -= 5f;
		}

		internal static void AutoGeneratedStaticCollectObjectsFamilyFeudIssueQuest(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(FamilyFeudIssueQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_targetSettlement);
			collectedObjects.Add(_targetNotable);
			collectedObjects.Add(_culprit);
		}

		internal static object AutoGeneratedGetMemberValue_targetSettlement(object o)
		{
			return ((FamilyFeudIssueQuest)o)._targetSettlement;
		}

		internal static object AutoGeneratedGetMemberValue_targetNotable(object o)
		{
			return ((FamilyFeudIssueQuest)o)._targetNotable;
		}

		internal static object AutoGeneratedGetMemberValue_culprit(object o)
		{
			return ((FamilyFeudIssueQuest)o)._culprit;
		}

		internal static object AutoGeneratedGetMemberValue_culpritJoinedPlayerParty(object o)
		{
			return ((FamilyFeudIssueQuest)o)._culpritJoinedPlayerParty;
		}

		internal static object AutoGeneratedGetMemberValue_checkForMissionEvents(object o)
		{
			return ((FamilyFeudIssueQuest)o)._checkForMissionEvents;
		}

		internal static object AutoGeneratedGetMemberValue_rewardGold(object o)
		{
			return ((FamilyFeudIssueQuest)o)._rewardGold;
		}
	}

	private const IssueFrequency FamilyFeudIssueFrequency = (IssueFrequency)2;

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnCheckForIssue);
	}

	public void OnCheckForIssue(Hero hero)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (ConditionsHold(hero, out var otherVillage, out var otherNotable))
		{
			KeyValuePair<Hero, Settlement> keyValuePair = new KeyValuePair<Hero, Settlement>(otherNotable, otherVillage);
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new StartIssueDelegate(OnStartIssue), typeof(FamilyFeudIssue), (IssueFrequency)2, (object)keyValuePair));
		}
		else
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(FamilyFeudIssue), (IssueFrequency)2));
		}
	}

	private bool ConditionsHold(Hero issueGiver, out Settlement otherVillage, out Hero otherNotable)
	{
		otherVillage = null;
		otherNotable = null;
		if (!issueGiver.IsNotable)
		{
			return false;
		}
		if (issueGiver.IsRuralNotable && issueGiver.CurrentSettlement.IsVillage)
		{
			Settlement bound = issueGiver.CurrentSettlement.Village.Bound;
			if (bound.IsTown)
			{
				foreach (Village item in LinQuick.WhereQ<Village>((List<Village>)(object)bound.BoundVillages, (Func<Village, bool>)((Village x) => x != issueGiver.CurrentSettlement.Village)))
				{
					Hero val = LinQuick.FirstOrDefaultQ<Hero>((List<Hero>)(object)((SettlementComponent)item).Settlement.Notables, (Func<Hero, bool>)((Hero y) => y.IsRuralNotable && y.CanHaveCampaignIssues() && y.GetTraitLevel(DefaultTraits.Mercy) <= 0));
					if (val != null)
					{
						otherVillage = ((SettlementComponent)item).Settlement;
						otherNotable = val;
					}
				}
				return otherVillage != null;
			}
		}
		return false;
	}

	private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		PotentialIssueData val = pid;
		KeyValuePair<Hero, Settlement> keyValuePair = (KeyValuePair<Hero, Settlement>)((PotentialIssueData)(ref val)).RelatedObject;
		return (IssueBase)(object)new FamilyFeudIssue(issueOwner, keyValuePair.Key, keyValuePair.Value);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
