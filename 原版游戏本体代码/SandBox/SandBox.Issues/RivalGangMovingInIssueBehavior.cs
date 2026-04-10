using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace SandBox.Issues;

public class RivalGangMovingInIssueBehavior : CampaignBehaviorBase
{
	public class RivalGangMovingInIssueTypeDefiner : SaveableTypeDefiner
	{
		public RivalGangMovingInIssueTypeDefiner()
			: base(310000)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(RivalGangMovingInIssue), 1, (IObjectResolver)null);
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(RivalGangMovingInIssueQuest), 2, (IObjectResolver)null);
		}
	}

	public class RivalGangMovingInIssue : IssueBase
	{
		private const int AlternativeSolutionRelationChange = 5;

		private const int AlternativeSolutionFailRelationChange = -5;

		private const int AlternativeSolutionQuestGiverPowerChange = 10;

		private const int AlternativeSolutionRivalGangLeaderPowerChange = -10;

		private const int AlternativeSolutionFailQuestGiverPowerChange = -10;

		private const int AlternativeSolutionFailSecurityChange = -10;

		private const int AlternativeSolutionRivalGangLeaderRelationChange = -5;

		private const int AlternativeSolutionMinimumTroopTier = 2;

		private const int IssueDuration = 15;

		private const int MinimumRequiredMenCount = 5;

		private const int IssueQuestDuration = 8;

		private const int MeleeSkillValueThreshold = 150;

		private const int RoguerySkillValueThreshold = 120;

		private const int PreparationDurationInDays = 2;

		public override AlternativeSolutionScaleFlag AlternativeSolutionScaleFlags => (AlternativeSolutionScaleFlag)12;

		public override TextObject IssueAlternativeSolutionSuccessLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Expected O, but got Unknown
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Expected O, but got Unknown
				TextObject val = new TextObject("{=pzvQ1DkE}Your companion has defeated the rival gang and protected the interests of {QUEST_GIVER.LINK} in {SETTLEMENT}.", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val, "QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, false);
				val.SetTextVariable("SETTLEMENT", ((IssueBase)this).IssueOwner.CurrentSettlement.Name);
				return val;
			}
		}

		[SaveableProperty(207)]
		public Hero RivalGangLeader { get; private set; }

		public override int AlternativeSolutionBaseNeededMenCount => 4 + MathF.Ceiling(6f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int AlternativeSolutionBaseDurationInDaysInternal => 3 + MathF.Ceiling(5f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int RewardGold => (int)(600f + 1700f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int CompanionSkillRewardXP => (int)(750f + 1000f * ((IssueBase)this).IssueDifficultyMultiplier);

		public override TextObject IssueBriefByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Expected O, but got Unknown
				TextObject val = new TextObject("{=GXk6f9ah}I've got a problem... [ib:confident][if:convo_undecided_closed]And {?TARGET_NOTABLE.GENDER}her{?}his{\\?} name is {TARGET_NOTABLE.LINK}. {?TARGET_NOTABLE.GENDER}Her{?}His{\\?} people have been coming around outside the walls, robbing the dice-players and the drinkers enjoying themselves under our protection. Me and my boys are eager to teach them a lesson but I figure some extra muscle wouldn't hurt.", (Dictionary<string, object>)null);
				if (RandomOwnerExtensions.RandomInt((IRandomOwner)(object)((IssueBase)this).IssueOwner, 2) == 0)
				{
					val = new TextObject("{=rgTGzfzI}Yeah. I have a problem all right. [ib:confident][if:convo_undecided_closed]{?TARGET_NOTABLE.GENDER}Her{?}His{\\?} name is {TARGET_NOTABLE.LINK}. {?TARGET_NOTABLE.GENDER}Her{?}His{\\?} people have been bothering shop owners under our protection, demanding money and making threats. Let me tell you something - those shop owners are my cows, and no one else gets to milk them. We're ready to teach these interlopers a lesson, but I could use some help.", (Dictionary<string, object>)null);
				}
				if (RivalGangLeader != null)
				{
					StringHelpers.SetCharacterProperties("TARGET_NOTABLE", RivalGangLeader.CharacterObject, val, false);
				}
				return val;
			}
		}

		public override TextObject IssueAcceptByPlayer => new TextObject("{=kc6vCycY}What exactly do you want me to do?", (Dictionary<string, object>)null);

		public override TextObject IssueQuestSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Expected O, but got Unknown
				TextObject val = new TextObject("{=tyyAfWRR}We already had a small scuffle with them recently. [if:convo_mocking_revenge]They'll be waiting for us to come down hard. Instead, we'll hold off for {NUMBER} days. Let them think that we're backing off… Then, after {NUMBER} days, your men and mine will hit them in the middle of the night when they least expect it. I'll send you a messenger when the time comes and we'll strike them down together.", (Dictionary<string, object>)null);
				val.SetTextVariable("NUMBER", 2);
				return val;
			}
		}

		public override TextObject IssueAlternativeSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Expected O, but got Unknown
				TextObject val = new TextObject("{=sSIjPCPO}If you'd rather not go into the fray yourself, [if:convo_mocking_aristocratic]you can leave me one of your companions together with {TROOP_COUNT} or so good men. If they stuck around for {RETURN_DAYS} days or so, I'd count it a very big favor.", (Dictionary<string, object>)null);
				val.SetTextVariable("TROOP_COUNT", ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount());
				val.SetTextVariable("RETURN_DAYS", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				return val;
			}
		}

		protected override TextObject AlternativeSolutionStartLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=ymbVPod1}{ISSUE_GIVER.LINK}, a gang leader from {SETTLEMENT}, has told you about a new gang that is trying to get a hold on the town. You asked {COMPANION.LINK} to take {TROOP_COUNT} of your best men to stay with {ISSUE_GIVER.LINK} and help {?ISSUE_GIVER.GENDER}her{?}him{\\?} in the coming gang war. They should return to you in {RETURN_DAYS} days.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("COMPANION", ((IssueBase)this).AlternativeSolutionHero.CharacterObject, val, false);
				val.SetTextVariable("SETTLEMENT", ((IssueBase)this).IssueOwner.CurrentSettlement.EncyclopediaLinkWithName);
				val.SetTextVariable("TROOP_COUNT", base.AlternativeSolutionSentTroops.TotalManCount - 1);
				val.SetTextVariable("RETURN_DAYS", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				return val;
			}
		}

		public override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=LdCte9H0}I'll fight the other gang with you myself.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionAcceptByPlayer => new TextObject("{=AdbiUqtT}I'm busy, but I will leave a companion and some men.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionResponseByIssueGiver => new TextObject("{=0enbhess}Thank you. [ib:normal][if:convo_approving]I'm sure your guys are worth their salt..", (Dictionary<string, object>)null);

		public override TextObject IssueDiscussAlternativeSolution => new TextObject("{=QR0V8Ae5}Our lads are well hidden nearby,[ib:normal][if:convo_excited] waiting for the signal to go get those bastards. I won't forget this little favor you're doing me.", (Dictionary<string, object>)null);

		public override bool IsThereAlternativeSolution => true;

		public override bool IsThereLordSolution => false;

		public override TextObject Title
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Expected O, but got Unknown
				TextObject val = new TextObject("{=vAjgn7yx}Rival Gang Moving in at {SETTLEMENT}", (Dictionary<string, object>)null);
				Settlement issueSettlement = ((IssueBase)this).IssueSettlement;
				val.SetTextVariable("SETTLEMENT", ((issueSettlement != null) ? issueSettlement.Name : null) ?? ((IssueBase)this).IssueOwner.HomeSettlement.Name);
				return val;
			}
		}

		public override TextObject Description => new TextObject("{=H4EVfKAh}Gang leader needs help to beat the rival gang.", (Dictionary<string, object>)null);

		public override TextObject IssueAsRumorInSettlement
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=C9feTaca}I hear {QUEST_GIVER.LINK} is going to sort it out with {RIVAL_GANG_LEADER.LINK} once and for all.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("RIVAL_GANG_LEADER", RivalGangLeader.CharacterObject, val, false);
				return val;
			}
		}

		protected override bool IssueQuestCanBeDuplicated => false;

		public RivalGangMovingInIssue(Hero issueOwner, Hero rivalGangLeader)
			: base(issueOwner, CampaignTime.DaysFromNow(15f))
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			RivalGangLeader = rivalGangLeader;
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == RivalGangLeader)
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
			if (issueEffect == DefaultIssueEffects.SettlementSecurity)
			{
				return -0.5f;
			}
			return 0f;
		}

		protected override void AlternativeSolutionEndWithSuccessConsequence()
		{
			((IssueBase)this).RelationshipChangeWithIssueOwner = 5;
			ChangeRelationAction.ApplyPlayerRelation(RivalGangLeader, -5, true, true);
			((IssueBase)this).IssueOwner.AddPower(10f);
			RivalGangLeader.AddPower(-10f);
		}

		protected override void AlternativeSolutionEndWithFailureConsequence()
		{
			((IssueBase)this).RelationshipChangeWithIssueOwner = -5;
			Town town = ((IssueBase)this).IssueSettlement.Town;
			town.Security += -10f;
			((IssueBase)this).IssueOwner.AddPower(-10f);
		}

		public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero)
		{
			int skillValue = hero.GetSkillValue(DefaultSkills.OneHanded);
			int skillValue2 = hero.GetSkillValue(DefaultSkills.TwoHanded);
			int skillValue3 = hero.GetSkillValue(DefaultSkills.Polearm);
			int skillValue4 = hero.GetSkillValue(DefaultSkills.Roguery);
			if (skillValue >= skillValue2 && skillValue >= skillValue3 && skillValue >= skillValue4)
			{
				return (DefaultSkills.OneHanded, 150);
			}
			if (skillValue2 >= skillValue3 && skillValue2 >= skillValue4)
			{
				return (DefaultSkills.TwoHanded, 150);
			}
			if (skillValue3 < skillValue4)
			{
				return (DefaultSkills.Roguery, 120);
			}
			return (DefaultSkills.Polearm, 150);
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

		protected override void OnGameLoad()
		{
		}

		protected override void HourlyTick()
		{
		}

		protected override QuestBase GenerateIssueQuest(string questId)
		{
			return (QuestBase)(object)new RivalGangMovingInIssueQuest(questId, ((IssueBase)this).IssueOwner, RivalGangLeader, 8, ((IssueBase)this).RewardGold, ((IssueBase)this).IssueDifficultyMultiplier);
		}

		public override IssueFrequency GetFrequency()
		{
			return (IssueFrequency)1;
		}

		protected override bool CanPlayerTakeQuestConditions(Hero issueGiver, out PreconditionFlags flag, out Hero relationHero, out SkillObject skill, out int requiredGold)
		{
			flag = (PreconditionFlags)0;
			relationHero = null;
			requiredGold = 0;
			skill = null;
			if (Hero.MainHero.IsWounded)
			{
				flag = (PreconditionFlags)((uint)flag | 0x20u);
			}
			if (issueGiver.GetRelationWithPlayer() < -10f)
			{
				flag = (PreconditionFlags)((uint)flag | 1u);
				relationHero = issueGiver;
			}
			if (MobileParty.MainParty.MemberRoster.TotalHealthyCount < 5)
			{
				flag = (PreconditionFlags)((uint)flag | 0x100u);
			}
			if (((IssueBase)this).IssueOwner.CurrentSettlement.OwnerClan == Clan.PlayerClan)
			{
				flag = (PreconditionFlags)((uint)flag | 0x8000u);
			}
			return (int)flag == 0;
		}

		public override bool IssueStayAliveConditions()
		{
			if (RivalGangLeader.IsAlive && ((IssueBase)this).IssueOwner.CurrentSettlement.OwnerClan != Clan.PlayerClan)
			{
				return ((IssueBase)this).IssueOwner.CurrentSettlement.Town.Security <= 80f;
			}
			return false;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}

		internal static void AutoGeneratedStaticCollectObjectsRivalGangMovingInIssue(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(RivalGangMovingInIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((IssueBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(RivalGangLeader);
		}

		internal static object AutoGeneratedGetMemberValueRivalGangLeader(object o)
		{
			return ((RivalGangMovingInIssue)o).RivalGangLeader;
		}
	}

	public class RivalGangMovingInIssueQuest : QuestBase
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static Action _003C_003E9__78_4;

			public static OnConsequenceDelegate _003C_003E9__78_1;

			public static OnConditionDelegate _003C_003E9__79_2;

			public static OnConditionDelegate _003C_003E9__79_4;

			public static OnConditionDelegate _003C_003E9__79_6;

			public static Func<CharacterObject, bool> _003C_003E9__84_0;

			public static Func<CharacterObject, bool> _003C_003E9__84_1;

			public static Func<CharacterObject, bool> _003C_003E9__84_3;

			public static Func<Settlement, bool> _003C_003E9__86_0;

			public static Func<Settlement, bool> _003C_003E9__87_0;

			public static Predicate<TroopRosterElement> _003C_003E9__88_0;

			public static Func<TroopRosterElement, int> _003C_003E9__88_1;

			public static Predicate<TroopRosterElement> _003C_003E9__89_0;

			internal void _003CGetRivalGangLeaderDialogFlow_003Eb__78_1()
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
				{
					Mission.Current.GetMissionBehavior<CombatMissionWithDialogueController>()?.StartFight(hasPlayerChangedSide: false);
				};
			}

			internal void _003CGetRivalGangLeaderDialogFlow_003Eb__78_4()
			{
				Mission.Current.GetMissionBehavior<CombatMissionWithDialogueController>()?.StartFight(hasPlayerChangedSide: false);
			}

			internal bool _003CGetQuestGiverPreparationCompletedDialogFlow_003Eb__79_2()
			{
				return !Hero.MainHero.IsWounded;
			}

			internal bool _003CGetQuestGiverPreparationCompletedDialogFlow_003Eb__79_4()
			{
				return !Hero.MainHero.IsWounded;
			}

			internal bool _003CGetQuestGiverPreparationCompletedDialogFlow_003Eb__79_6()
			{
				return Hero.MainHero.IsWounded;
			}

			internal bool _003CGetTroopTypeTemplateForDifficulty_003Eb__84_0(CharacterObject t)
			{
				return ((MBObjectBase)t).StringId == "looter";
			}

			internal bool _003CGetTroopTypeTemplateForDifficulty_003Eb__84_1(CharacterObject t)
			{
				return ((MBObjectBase)t).StringId == "mercenary_8";
			}

			internal bool _003CGetTroopTypeTemplateForDifficulty_003Eb__84_3(CharacterObject t)
			{
				if (t.IsBasicTroop)
				{
					return ((BasicCharacterObject)t).IsSoldier;
				}
				return false;
			}

			internal bool _003CCreateRivalGangLeaderParty_003Eb__86_0(Settlement x)
			{
				return x.IsActive;
			}

			internal bool _003CCreateAllyGangLeaderParty_003Eb__87_0(Settlement x)
			{
				return x.IsActive;
			}

			internal bool _003CPreparePlayerParty_003Eb__88_0(TroopRosterElement t)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				return !((BasicCharacterObject)t.Character).IsPlayerCharacter;
			}

			internal int _003CPreparePlayerParty_003Eb__88_1(TroopRosterElement t)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				return ((BasicCharacterObject)t.Character).Level;
			}

			internal bool _003CHandlePlayerEncounterResult_003Eb__89_0(TroopRosterElement t)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				return !((BasicCharacterObject)t.Character).IsPlayerCharacter;
			}
		}

		private const int QuestGiverRelationChangeOnSuccess = 5;

		private const int RivalGangLeaderRelationChangeOnSuccess = -5;

		private const int QuestGiverNotablePowerChangeOnSuccess = 10;

		private const int RivalGangLeaderPowerChangeOnSuccess = -10;

		private const int RenownChangeOnSuccess = 1;

		private const int QuestGiverRelationChangeOnFail = -5;

		private const int QuestGiverRelationChangeOnTimedOut = -5;

		private const int NotablePowerChangeOnFail = -10;

		private const int TownSecurityChangeOnFail = -10;

		private const int RivalGangLeaderRelationChangeOnSuccessfulBetrayal = 5;

		private const int QuestGiverRelationChangeOnSuccessfulBetrayal = -15;

		private const int RivalGangLeaderPowerChangeOnSuccessfulBetrayal = 10;

		private const int QuestGiverRelationChangeOnFailedBetrayal = -10;

		private const int PlayerAttackedQuestGiverHonorChange = -150;

		private const int PlayerAttackedQuestGiverPowerChange = -10;

		private const int NumberOfRegularEnemyTroops = 15;

		private const int PlayerAttackedQuestGiverRelationChange = -8;

		private const int PlayerAttackedQuestGiverSecurityChange = -10;

		private const int NumberOfRegularAllyTroops = 20;

		private const int MaxNumberOfPlayerOwnedTroops = 5;

		private const string AllyGangLeaderHenchmanStringId = "gangster_2";

		private const string RivalGangLeaderHenchmanStringId = "gangster_3";

		private const int PreparationDurationInDays = 2;

		[SaveableField(10)]
		internal readonly Hero _rivalGangLeader;

		[SaveableField(20)]
		private MobileParty _rivalGangLeaderParty;

		private Hero _rivalGangLeaderHenchmanHero;

		[SaveableField(30)]
		private readonly CampaignTime _preparationCompletionTime;

		private Hero _allyGangLeaderHenchmanHero;

		private MobileParty _allyGangLeaderParty;

		[SaveableField(40)]
		private readonly CampaignTime _questTimeoutTime;

		[SaveableField(60)]
		internal readonly float _timeoutDurationInDays;

		[SaveableField(70)]
		internal bool _isFinalStage;

		[SaveableField(80)]
		internal bool _isReadyToBeFinalized;

		[SaveableField(90)]
		internal bool _hasBetrayedQuestGiver;

		private List<TroopRosterElement> _allPlayerTroops;

		private List<CharacterObject> _sentTroops;

		private Hero _partyEngineer;

		private Hero _partyScout;

		private Hero _partyQuartermaster;

		private Hero _partySurgeon;

		[SaveableField(110)]
		private bool _preparationsComplete;

		[SaveableField(120)]
		private int _rewardGold;

		[SaveableField(130)]
		private float _issueDifficulty;

		private Settlement _questSettlement;

		private JournalLog _onQuestStartedLog;

		private JournalLog _onQuestSucceededLog;

		private TextObject OnQuestStartedLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=dav5rmDd}{QUEST_GIVER.LINK}, a gang leader from {SETTLEMENT} has told you about a rival that is trying to get a foothold in {?QUEST_GIVER.GENDER}her{?}his{\\?} town. {?QUEST_GIVER.GENDER}She{?}He{\\?} asked you to wait {DAY_COUNT} days so that the other gang lets its guard down.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("SETTLEMENT", _questSettlement.EncyclopediaLinkWithName);
				val.SetTextVariable("DAY_COUNT", 2);
				return val;
			}
		}

		private TextObject OnQuestFailedWithRejectionLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=aXMg9M7t}You didn't respond to the messenger {QUEST_GIVER.LINK} sent you. {?QUEST_GIVER.GENDER}She{?}He{\\?} will certainly lose to the rival gang without your help.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject OnQuestFailedWithBetrayalLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=Rf0QqRIX}You have chosen to side with the rival gang leader, {RIVAL_GANG_LEADER.LINK}. {QUEST_GIVER.LINK} must be furious.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("RIVAL_GANG_LEADER", _rivalGangLeader.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject OnQuestFailedWithDefeatLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=du3dpMaV}You were unable to defeat {RIVAL_GANG_LEADER.LINK}'s gang, and thus failed to fulfill your commitment to {QUEST_GIVER.LINK}.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("RIVAL_GANG_LEADER", _rivalGangLeader.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject OnQuestSucceededLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=vpUl7xcy}You have defeated the rival gang and protected the interests of {QUEST_GIVER.LINK} in {SETTLEMENT}.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("SETTLEMENT", _questSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject OnQuestPreperationsCompletedLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=OIBiRTRP}{QUEST_GIVER.LINK} is waiting for you at {SETTLEMENT}.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("SETTLEMENT", _questSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject OnQuestCancelledDueToWarLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=vaUlAZba}Your clan is now at war with {QUEST_GIVER.LINK}. Your agreement with {QUEST_GIVER.LINK} was canceled.", (Dictionary<string, object>)null);
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

		private TextObject OnQuestCancelledDueToSiegeLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=s1GWSE9Y}{QUEST_GIVER.LINK} cancels your plans due to the siege of {SETTLEMENT}. {?QUEST_GIVER.GENDER}She{?}He{\\?} has worse troubles than {?QUEST_GIVER.GENDER}her{?}his{\\?} quarrel with the rival gang.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("SETTLEMENT", _questSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject PlayerStartedAlleyFightWithRivalGangLeader
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=OeKgpuAv}After your attack on the rival gang's alley, {QUEST_GIVER.LINK} decided to change {?QUEST_GIVER.GENDER}her{?}his{\\?} plans, and doesn't need your assistance anymore. Quest is canceled.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject PlayerStartedAlleyFightWithQuestgiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=VPGkIqlh}Your attack on {QUEST_GIVER.LINK}'s gang has angered {?QUEST_GIVER.GENDER}her{?}him{\\?} and {?QUEST_GIVER.GENDER}she{?}he{\\?} broke off the agreement that you had.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject OwnerOfQuestSettlementIsPlayerClanLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=KxEnNEoD}Your clan is now owner of the settlement. As the {?PLAYER.GENDER}lady{?}lord{\\?} of the settlement you cannot get involved in gang wars anymore. Your agreement with the {QUEST_GIVER.LINK} has canceled.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
				return val;
			}
		}

		public override TextObject Title
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Expected O, but got Unknown
				TextObject val = new TextObject("{=vAjgn7yx}Rival Gang Moving in at {SETTLEMENT}", (Dictionary<string, object>)null);
				val.SetTextVariable("SETTLEMENT", _questSettlement.Name);
				return val;
			}
		}

		public override bool IsRemainingTimeHidden => false;

		public RivalGangMovingInIssueQuest(string questId, Hero questGiver, Hero rivalGangLeader, int duration, int rewardGold, float issueDifficulty)
			: base(questId, questGiver, CampaignTime.DaysFromNow((float)duration), rewardGold)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			_rivalGangLeader = rivalGangLeader;
			_rewardGold = rewardGold;
			_issueDifficulty = issueDifficulty;
			_timeoutDurationInDays = duration;
			_preparationCompletionTime = CampaignTime.DaysFromNow(2f);
			_questTimeoutTime = CampaignTime.DaysFromNow(_timeoutDurationInDays);
			_sentTroops = new List<CharacterObject>();
			_allPlayerTroops = new List<TroopRosterElement>();
			InitializeQuestSettlement();
			((QuestBase)this).SetDialogs();
			((QuestBase)this).InitializeQuestOnCreation();
		}

		protected override void InitializeQuestOnGameLoad()
		{
			InitializeQuestSettlement();
			((QuestBase)this).SetDialogs();
			Campaign.Current.ConversationManager.AddDialogFlow(GetRivalGangLeaderDialogFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetQuestGiverPreparationCompletedDialogFlow(), (object)this);
			MobileParty rivalGangLeaderParty = _rivalGangLeaderParty;
			if (rivalGangLeaderParty != null)
			{
				rivalGangLeaderParty.SetPartyUsedByQuest(true);
			}
			_sentTroops = new List<CharacterObject>();
			_allPlayerTroops = new List<TroopRosterElement>();
		}

		private void InitializeQuestSettlement()
		{
			_questSettlement = ((QuestBase)this).QuestGiver.CurrentSettlement;
		}

		protected override void SetDialogs()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			base.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine("{=Fwm0PwVb}Great. As I said we need minimum of {NUMBER} days,[ib:normal][if:convo_mocking_revenge] so they'll let their guard down. I will let you know when it's time. Remember, we wait for the dark of the night to strike.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
			{
				MBTextManager.SetTextVariable("SETTLEMENT", _questSettlement.EncyclopediaLinkWithName, false);
				MBTextManager.SetTextVariable("NUMBER", 2);
				return Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver;
			})
				.Consequence(new OnConsequenceDelegate(OnQuestAccepted))
				.CloseDialog();
			base.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine("{=z43j3Tzq}I'm still gathering my men for the fight. I'll send a runner for you when the time comes.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
			{
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, (TextObject)null, false);
				return Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver && !_isFinalStage && !_preparationsComplete;
			})
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=4IHRAmnA}All right. I am waiting for your runner.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=xEs830bT}You'll know right away once the preparations are complete.[ib:closed][if:convo_mocking_teasing] Just don't leave town.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.PlayerOption("{=6g8qvD2M}I can't just hang on here forever. Be quick about it.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=lM7AscLo}I'm getting this together as quickly as I can.[ib:closed][if:convo_nervous]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private DialogFlow GetRivalGangLeaderDialogFlow()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Expected O, but got Unknown
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Expected O, but got Unknown
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Expected O, but got Unknown
			DialogFlow obj = DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=IfeN8lYd}Coming to fight us, eh? Did {QUEST_GIVER.LINK} put you up to this?[ib:aggressive2][if:convo_confused_annoyed] Look, there's no need for bloodshed. This town is big enough for all of us. But... if bloodshed is what you want, we will be happy to provide.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
			{
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, (TextObject)null, false);
				return Hero.OneToOneConversationHero == _rivalGangLeaderHenchmanHero && _isReadyToBeFinalized;
			})
				.NpcLine("{=WSJxl2Hu}What I want to say is... [if:convo_mocking_teasing]You don't need to be a part of this. My boss will double whatever {?QUEST_GIVER.GENDER}she{?}he{\\?} is paying you if you join us.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=GPBja02V}I gave my word to {QUEST_GIVER.LINK}, and I won't be bought.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
			object obj2 = _003C_003Ec._003C_003E9__78_1;
			if (obj2 == null)
			{
				OnConsequenceDelegate val = delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
					{
						Mission.Current.GetMissionBehavior<CombatMissionWithDialogueController>()?.StartFight(hasPlayerChangedSide: false);
					};
				};
				_003C_003Ec._003C_003E9__78_1 = val;
				obj2 = (object)val;
			}
			return obj.Consequence((OnConsequenceDelegate)obj2).NpcLine("{=OSgBicif}You will regret this![ib:warrior][if:convo_furious]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
				.PlayerOption("{=RB4uQpPV}You're going to pay me a lot then, {REWARD}{GOLD_ICON} to be exact. But at that price, I agree.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Condition((OnConditionDelegate)delegate
				{
					MBTextManager.SetTextVariable("REWARD", _rewardGold * 2);
					return true;
				})
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
					{
						_hasBetrayedQuestGiver = true;
						Mission.Current.GetMissionBehavior<CombatMissionWithDialogueController>()?.StartFight(hasPlayerChangedSide: true);
					};
				})
				.NpcLine("{=5jW4FVDc}Welcome to our ranks then. [ib:warrior][if:convo_evil_smile]Let's kill those bastards!", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private DialogFlow GetQuestGiverPreparationCompletedDialogFlow()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			//IL_0033: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Expected O, but got Unknown
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Expected O, but got Unknown
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Expected O, but got Unknown
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Expected O, but got Unknown
			DialogFlow obj = DialogFlow.CreateDialogFlow("start", 125).BeginNpcOptions((string)null, false).NpcOption(new TextObject("{=hM7LSuB1}Good to see you. But we still need to wait until after dusk. {HERO.LINK}'s men may be watching, so let's keep our distance from each other until night falls.", (Dictionary<string, object>)null), (OnConditionDelegate)delegate
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				StringHelpers.SetCharacterProperties("HERO", _rivalGangLeader.CharacterObject, (TextObject)null, false);
				if (Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver && !_isFinalStage)
				{
					CampaignTime val4 = _preparationCompletionTime;
					if (((CampaignTime)(ref val4)).IsPast)
					{
						if (_preparationsComplete)
						{
							val4 = CampaignTime.Now;
							return !((CampaignTime)(ref val4)).IsNightTime;
						}
						return true;
					}
				}
				return false;
			}, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.NpcOption("{=JxNlB547}Are you ready for the fight?[ib:normal][if:convo_undecided_open]", (OnConditionDelegate)delegate
				{
					//IL_001d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					if (Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver && _preparationsComplete && !_isFinalStage)
					{
						CampaignTime now = CampaignTime.Now;
						return ((CampaignTime)(ref now)).IsNightTime;
					}
					return false;
				}, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.EndNpcOptions()
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=NzMX0s21}I am ready.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
			object obj2 = _003C_003Ec._003C_003E9__79_2;
			if (obj2 == null)
			{
				OnConditionDelegate val = () => !Hero.MainHero.IsWounded;
				_003C_003Ec._003C_003E9__79_2 = val;
				obj2 = (object)val;
			}
			DialogFlow obj3 = obj.Condition((OnConditionDelegate)obj2).NpcLine("{=dNjepcKu}Let's finish this![ib:hip][if:convo_mocking_revenge]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += rival_gang_start_fight_on_consequence;
			})
				.CloseDialog()
				.PlayerOption("{=B2Donbwz}I need more time.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
			object obj4 = _003C_003Ec._003C_003E9__79_4;
			if (obj4 == null)
			{
				OnConditionDelegate val2 = () => !Hero.MainHero.IsWounded;
				_003C_003Ec._003C_003E9__79_4 = val2;
				obj4 = (object)val2;
			}
			DialogFlow obj5 = obj3.Condition((OnConditionDelegate)obj4).NpcLine("{=advPT3WY}You'd better hurry up![ib:closed][if:convo_astonished]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += rival_gang_need_more_time_on_consequence;
			})
				.CloseDialog()
				.PlayerOption("{=QaN26CZ5}My wounds are still fresh. I need some time to recover.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
			object obj6 = _003C_003Ec._003C_003E9__79_6;
			if (obj6 == null)
			{
				OnConditionDelegate val3 = () => Hero.MainHero.IsWounded;
				_003C_003Ec._003C_003E9__79_6 = val3;
				obj6 = (object)val3;
			}
			return obj5.Condition((OnConditionDelegate)obj6).NpcLine("{=s0jKaYo0}We must attack before the rival gang hears about our plan. You'd better hurry up![if:convo_astonished]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		public override void OnHeroCanDieInfoIsRequested(Hero hero, KillCharacterActionDetail causeOfDeath, ref bool result)
		{
			if (hero == ((QuestBase)this).QuestGiver || hero == _rivalGangLeader)
			{
				result = false;
			}
		}

		private void rival_gang_start_fight_on_consequence()
		{
			_isFinalStage = true;
			if (Mission.Current != null)
			{
				Mission.Current.EndMission();
			}
			Campaign.Current.GameMenuManager.SetNextMenu("rival_gang_quest_before_fight");
		}

		private void rival_gang_need_more_time_on_consequence()
		{
			if (Campaign.Current.CurrentMenuContext.GameMenu.StringId == "rival_gang_quest_wait_duration_is_over")
			{
				Campaign.Current.GameMenuManager.SetNextMenu("town_wait_menus");
			}
		}

		private void AddQuestGiverGangLeaderOnSuccessDialogFlow()
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=zNPzh5jO}Ah! Now that was as good a fight as any I've had. Here, take this purse, It is all yours as {QUEST_GIVER.LINK} has promised.[ib:hip2][if:convo_huge_smile]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
			{
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, (TextObject)null, false);
				return ((QuestBase)this).IsOngoing && Hero.OneToOneConversationHero == _allyGangLeaderHenchmanHero;
			})
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnQuestSucceeded;
				})
				.CloseDialog(), (object)null);
		}

		private CharacterObject GetTroopTypeTemplateForDifficulty()
		{
			int difficultyRange = MBMath.ClampInt(MathF.Ceiling(_issueDifficulty / 0.1f), 1, 10);
			CharacterObject val = ((difficultyRange == 1) ? ((IEnumerable<CharacterObject>)CharacterObject.All).FirstOrDefault((Func<CharacterObject, bool>)((CharacterObject t) => ((MBObjectBase)t).StringId == "looter")) : ((difficultyRange != 10) ? ((IEnumerable<CharacterObject>)CharacterObject.All).FirstOrDefault((Func<CharacterObject, bool>)((CharacterObject t) => ((MBObjectBase)t).StringId == "mercenary_" + (difficultyRange - 1))) : ((IEnumerable<CharacterObject>)CharacterObject.All).FirstOrDefault((Func<CharacterObject, bool>)((CharacterObject t) => ((MBObjectBase)t).StringId == "mercenary_8"))));
			if (val == null)
			{
				Debug.FailedAssert("Can't find troop in rival gang leader quest", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Issues\\RivalGangMovingInIssueBehavior.cs", "GetTroopTypeTemplateForDifficulty", 806);
				val = ((IEnumerable<CharacterObject>)CharacterObject.All).First((CharacterObject t) => t.IsBasicTroop && ((BasicCharacterObject)t).IsSoldier);
			}
			return val;
		}

		internal void StartAlleyBattle()
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			CreateRivalGangLeaderParty();
			CreateAllyGangLeaderParty();
			PreparePlayerParty();
			PlayerEncounter.RestartPlayerEncounter(_rivalGangLeaderParty.Party, PartyBase.MainParty, false);
			PlayerEncounter.StartBattle();
			_allyGangLeaderParty.MapEventSide = PlayerEncounter.Battle.GetMapEventSide(PlayerEncounter.Battle.PlayerSide);
			GameMenu.ActivateGameMenu("rival_gang_quest_after_fight");
			_isReadyToBeFinalized = true;
			PlayerEncounter.StartCombatMissionWithDialogueInTownCenter(_rivalGangLeaderHenchmanHero.CharacterObject);
		}

		private void CreateRivalGangLeaderParty()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Expected O, but got Unknown
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			TextObject val = new TextObject("{=u4jhIFwG}{GANG_LEADER}'s Party", (Dictionary<string, object>)null);
			val.SetTextVariable("RIVAL_GANG_LEADER", _rivalGangLeader.Name);
			val.SetTextVariable("GANG_LEADER", _rivalGangLeader.Name);
			Hideout closestHideout = SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement x) => x.IsActive));
			Clan val2 = LinQuick.FirstOrDefaultQ<Clan>(Clan.BanditFactions, (Func<Clan, bool>)((Clan t) => t.Culture == ((SettlementComponent)closestHideout).Settlement.Culture));
			_rivalGangLeaderParty = CustomPartyComponent.CreateCustomPartyWithTroopRoster(_questSettlement.GatePosition, 1f, _questSettlement, val, val2, TroopRoster.CreateDummyTroopRoster(), TroopRoster.CreateDummyTroopRoster(), (Hero)null, "", "", 0f, false);
			_rivalGangLeaderParty.SetPartyUsedByQuest(true);
			CharacterObject troopTypeTemplateForDifficulty = GetTroopTypeTemplateForDifficulty();
			_rivalGangLeaderParty.MemberRoster.AddToCounts(troopTypeTemplateForDifficulty, 15, false, 0, 0, true, -1);
			CharacterObject val3 = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_3");
			_rivalGangLeaderHenchmanHero = HeroCreator.CreateSpecialHero(val3, (Settlement)null, (Clan)null, (Clan)null, -1);
			TextObject val4 = new TextObject("{=zJqEdDiq}Henchman of {GANG_LEADER}", (Dictionary<string, object>)null);
			val4.SetTextVariable("GANG_LEADER", _rivalGangLeader.Name);
			_rivalGangLeaderHenchmanHero.SetName(val4, val4);
			_rivalGangLeaderHenchmanHero.HiddenInEncyclopedia = true;
			_rivalGangLeaderHenchmanHero.Culture = _rivalGangLeader.Culture;
			_rivalGangLeaderHenchmanHero.SetNewOccupation((Occupation)31);
			_rivalGangLeaderHenchmanHero.ChangeState((CharacterStates)1);
			_rivalGangLeaderParty.MemberRoster.AddToCounts(_rivalGangLeaderHenchmanHero.CharacterObject, 1, false, 0, 0, true, -1);
			_rivalGangLeaderParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
			EnterSettlementAction.ApplyForParty(_rivalGangLeaderParty, _questSettlement);
		}

		private void CreateAllyGangLeaderParty()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Expected O, but got Unknown
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			TextObject val = new TextObject("{=u4jhIFwG}{GANG_LEADER}'s Party", (Dictionary<string, object>)null);
			val.SetTextVariable("GANG_LEADER", ((QuestBase)this).QuestGiver.Name);
			Hideout closestHideout = SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement x) => x.IsActive));
			Clan val2 = LinQuick.FirstOrDefaultQ<Clan>(Clan.BanditFactions, (Func<Clan, bool>)((Clan t) => t.Culture == ((SettlementComponent)closestHideout).Settlement.Culture));
			_allyGangLeaderParty = CustomPartyComponent.CreateCustomPartyWithTroopRoster(_questSettlement.GatePosition, 1f, _questSettlement, val, val2, TroopRoster.CreateDummyTroopRoster(), TroopRoster.CreateDummyTroopRoster(), (Hero)null, "", "", 0f, false);
			_allyGangLeaderParty.SetPartyUsedByQuest(true);
			CharacterObject troopTypeTemplateForDifficulty = GetTroopTypeTemplateForDifficulty();
			_allyGangLeaderParty.MemberRoster.AddToCounts(troopTypeTemplateForDifficulty, 20, false, 0, 0, true, -1);
			CharacterObject val3 = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_2");
			_allyGangLeaderHenchmanHero = HeroCreator.CreateSpecialHero(val3, (Settlement)null, (Clan)null, (Clan)null, -1);
			TextObject val4 = new TextObject("{=zJqEdDiq}Henchman of {GANG_LEADER}", (Dictionary<string, object>)null);
			val4.SetTextVariable("GANG_LEADER", ((QuestBase)this).QuestGiver.Name);
			_allyGangLeaderHenchmanHero.SetName(val4, val4);
			_allyGangLeaderHenchmanHero.HiddenInEncyclopedia = true;
			_allyGangLeaderHenchmanHero.Culture = ((QuestBase)this).QuestGiver.Culture;
			_allyGangLeaderHenchmanHero.ChangeState((CharacterStates)1);
			_allyGangLeaderParty.MemberRoster.AddToCounts(_allyGangLeaderHenchmanHero.CharacterObject, 1, false, 0, 0, true, -1);
			_allyGangLeaderParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
			EnterSettlementAction.ApplyForParty(_allyGangLeaderParty, _questSettlement);
		}

		private void PreparePlayerParty()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			_allPlayerTroops.Clear();
			foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)PartyBase.MainParty.MemberRoster.GetTroopRoster())
			{
				if (!((BasicCharacterObject)item.Character).IsPlayerCharacter)
				{
					_allPlayerTroops.Add(item);
				}
			}
			_partyEngineer = MobileParty.MainParty.GetRoleHolder((PartyRole)8);
			_partyScout = MobileParty.MainParty.GetRoleHolder((PartyRole)9);
			_partyQuartermaster = MobileParty.MainParty.GetRoleHolder((PartyRole)10);
			_partySurgeon = MobileParty.MainParty.GetRoleHolder((PartyRole)7);
			PartyBase.MainParty.MemberRoster.RemoveIf((Predicate<TroopRosterElement>)((TroopRosterElement t) => !((BasicCharacterObject)t.Character).IsPlayerCharacter));
			if (Extensions.IsEmpty<TroopRosterElement>((IEnumerable<TroopRosterElement>)_allPlayerTroops))
			{
				return;
			}
			_sentTroops.Clear();
			int num = 5;
			foreach (TroopRosterElement item2 in _allPlayerTroops.OrderByDescending((TroopRosterElement t) => ((BasicCharacterObject)t.Character).Level))
			{
				TroopRosterElement current2 = item2;
				if (num <= 0)
				{
					break;
				}
				for (int num2 = 0; num2 < ((TroopRosterElement)(ref current2)).Number - ((TroopRosterElement)(ref current2)).WoundedNumber; num2++)
				{
					if (num <= 0)
					{
						break;
					}
					_sentTroops.Add(current2.Character);
					num--;
				}
			}
			foreach (CharacterObject sentTroop in _sentTroops)
			{
				PartyBase.MainParty.MemberRoster.AddToCounts(sentTroop, 1, false, 0, 0, true, -1);
			}
		}

		internal void HandlePlayerEncounterResult(bool hasPlayerWon)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			PlayerEncounter.Finish(false);
			EncounterManager.StartSettlementEncounter(MobileParty.MainParty, _questSettlement);
			TroopRoster val = PartyBase.MainParty.MemberRoster.CloneRosterData();
			PartyBase.MainParty.MemberRoster.RemoveIf((Predicate<TroopRosterElement>)((TroopRosterElement t) => !((BasicCharacterObject)t.Character).IsPlayerCharacter));
			foreach (TroopRosterElement allPlayerTroop in _allPlayerTroops)
			{
				TroopRosterElement playerTroop = allPlayerTroop;
				int num = val.FindIndexOfTroop(playerTroop.Character);
				int num2 = ((TroopRosterElement)(ref playerTroop)).Number;
				int num3 = ((TroopRosterElement)(ref playerTroop)).WoundedNumber;
				int num4 = ((TroopRosterElement)(ref playerTroop)).Xp;
				if (num >= 0)
				{
					TroopRosterElement elementCopyAtIndex = val.GetElementCopyAtIndex(num);
					num2 -= _sentTroops.Count((CharacterObject t) => t == playerTroop.Character) - ((TroopRosterElement)(ref elementCopyAtIndex)).Number;
					num3 += ((TroopRosterElement)(ref elementCopyAtIndex)).WoundedNumber;
					num4 += ((TroopRosterElement)(ref elementCopyAtIndex)).Xp;
				}
				else if (_sentTroops.Contains(playerTroop.Character))
				{
					num2 -= _sentTroops.Count((CharacterObject t) => t == playerTroop.Character);
				}
				PartyBase.MainParty.MemberRoster.AddToCounts(playerTroop.Character, num2, false, num3, num4, true, -1);
			}
			MobileParty.MainParty.SetPartyEngineer(_partyEngineer);
			MobileParty.MainParty.SetPartyScout(_partyScout);
			MobileParty.MainParty.SetPartyQuartermaster(_partyQuartermaster);
			MobileParty.MainParty.SetPartySurgeon(_partySurgeon);
			if (_rivalGangLeader.PartyBelongedTo == _rivalGangLeaderParty)
			{
				_rivalGangLeaderParty.MemberRoster.AddToCounts(_rivalGangLeader.CharacterObject, -1, false, 0, 0, true, -1);
			}
			if (hasPlayerWon)
			{
				if (!_hasBetrayedQuestGiver)
				{
					AddQuestGiverGangLeaderOnSuccessDialogFlow();
					SpawnAllyHenchmanAfterMissionSuccess();
					PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationOfCharacter(_allyGangLeaderHenchmanHero), (Location)null, _allyGangLeaderHenchmanHero.CharacterObject, (string)null);
				}
				else
				{
					OnBattleWonWithBetrayal();
				}
			}
			else if (!_hasBetrayedQuestGiver)
			{
				OnQuestFailedWithDefeat();
			}
			else
			{
				OnBattleLostWithBetrayal();
			}
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
			CampaignEvents.AlleyClearedByPlayer.AddNonSerializedListener((object)this, (Action<Alley>)OnAlleyClearedByPlayer);
			CampaignEvents.AlleyOccupiedByPlayer.AddNonSerializedListener((object)this, (Action<Alley, TroopRoster>)OnAlleyOccupiedByPlayer);
			CampaignEvents.WarDeclared.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, DeclareWarDetail>)OnWarDeclared);
			CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener((object)this, (Action<SiegeEvent>)OnSiegeEventStarted);
			CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
			CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
		}

		private void SpawnAllyHenchmanAfterMissionSuccess()
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
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)_allyGangLeaderHenchmanHero.CharacterObject).Race, "_settlement");
			AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)_allyGangLeaderHenchmanHero.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix);
			IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
			LocationCharacter val = new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "npc_common", true, (CharacterRelations)0, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
			LocationComplex.Current.GetLocationWithId("center").AddCharacter(val);
		}

		private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
		{
			if (settlement == ((QuestBase)this).QuestGiver.CurrentSettlement && newOwner == Hero.MainHero)
			{
				((QuestBase)this).AddLog(OwnerOfQuestSettlementIsPlayerClanLogText, false);
				((QuestBase)this).QuestGiver.AddPower(-10f);
				ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -5, true, true);
				((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
			}
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == _rivalGangLeader)
			{
				result = false;
			}
		}

		private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
		{
			if (((QuestBase)this).QuestGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				((QuestBase)this).CompleteQuestWithCancel(OnQuestCancelledDueToWarLogText);
			}
		}

		private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarDetail detail)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest((QuestBase)(object)this, faction1, faction2, detail, PlayerDeclaredWarQuestLogText, OnQuestCancelledDueToWarLogText, false);
		}

		private void OnSiegeEventStarted(SiegeEvent siegeEvent)
		{
			if (siegeEvent.BesiegedSettlement == _questSettlement)
			{
				((QuestBase)this).AddLog(OnQuestCancelledDueToSiegeLogText, false);
				((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
			}
		}

		protected override void HourlyTick()
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			if (Instance == null || !((QuestBase)Instance).IsOngoing)
			{
				return;
			}
			CampaignTime val = Instance._preparationCompletionTime;
			if ((2f - ((CampaignTime)(ref val)).RemainingDaysFromNow) / 2f >= 1f && !_preparationsComplete)
			{
				val = CampaignTime.Now;
				if (((CampaignTime)(ref val)).IsNightTime)
				{
					OnGuestGiverPreparationsCompleted();
				}
			}
		}

		private void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if (victim == _rivalGangLeader)
			{
				TextObject val = (((int)detail == 8) ? ((QuestBase)this).TargetHeroDisappearedLogText : ((QuestBase)this).TargetHeroDiedLogText);
				StringHelpers.SetCharacterProperties("QUEST_TARGET", _rivalGangLeader.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				((QuestBase)this).AddLog(val, false);
				((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
			}
		}

		private void OnPlayerAlleyFightEnd(Alley alley)
		{
			if (!_isReadyToBeFinalized)
			{
				if (((SettlementArea)alley).Owner == _rivalGangLeader)
				{
					OnPlayerAttackedRivalGangAlley();
				}
				else if (((SettlementArea)alley).Owner == ((QuestBase)this).QuestGiver)
				{
					OnPlayerAttackedQuestGiverAlley();
				}
			}
		}

		private void OnAlleyClearedByPlayer(Alley alley)
		{
			OnPlayerAlleyFightEnd(alley);
		}

		private void OnAlleyOccupiedByPlayer(Alley alley, TroopRoster troops)
		{
			OnPlayerAlleyFightEnd(alley);
		}

		private void OnPlayerAttackedRivalGangAlley()
		{
			((QuestBase)this).AddLog(PlayerStartedAlleyFightWithRivalGangLeader, false);
			((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
		}

		private void OnPlayerAttackedQuestGiverAlley()
		{
			TraitLevelingHelper.OnIssueSolvedThroughQuest(((QuestBase)this).QuestGiver, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -150)
			});
			((QuestBase)this).QuestGiver.AddPower(-10f);
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, -8, true, true);
			Town town = _questSettlement.Town;
			town.Security += -10f;
			((QuestBase)this).AddLog(PlayerStartedAlleyFightWithQuestgiver, false);
			((QuestBase)this).CompleteQuestWithFail((TextObject)null);
		}

		protected override void OnTimedOut()
		{
			OnQuestFailedWithRejectionOrTimeout();
		}

		private void OnGuestGiverPreparationsCompleted()
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			_preparationsComplete = true;
			if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement == _questSettlement && Campaign.Current.CurrentMenuContext != null && Campaign.Current.CurrentMenuContext.GameMenu.StringId == "town_wait_menus")
			{
				Campaign.Current.CurrentMenuContext.SwitchToMenu("rival_gang_quest_wait_duration_is_over");
			}
			TextObject val = new TextObject("{=DUKbtlNb}{QUEST_GIVER.LINK} has finally sent a messenger telling you it's time to meet {?QUEST_GIVER.GENDER}her{?}him{\\?} and join the fight.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
			((QuestBase)this).AddLog(OnQuestPreperationsCompletedLogText, false);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}

		private void OnQuestAccepted()
		{
			((QuestBase)this).StartQuest();
			_onQuestStartedLog = ((QuestBase)this).AddLog(OnQuestStartedLogText, false);
			Campaign.Current.ConversationManager.AddDialogFlow(GetRivalGangLeaderDialogFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetQuestGiverPreparationCompletedDialogFlow(), (object)this);
		}

		private void OnQuestSucceeded()
		{
			_onQuestSucceededLog = ((QuestBase)this).AddLog(OnQuestSucceededLogText, false);
			GainRenownAction.Apply(Hero.MainHero, 1f, false);
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, _rewardGold, false);
			TraitLevelingHelper.OnIssueSolvedThroughQuest(((QuestBase)this).QuestGiver, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, 50)
			});
			((QuestBase)this).QuestGiver.AddPower(10f);
			_rivalGangLeader.AddPower(-10f);
			((QuestBase)this).RelationshipChangeWithQuestGiver = 5;
			ChangeRelationAction.ApplyPlayerRelation(_rivalGangLeader, -5, true, true);
			GameMenu.ExitToLast();
			GameMenu.ActivateGameMenu("town");
			((QuestBase)this).CompleteQuestWithSuccess();
		}

		private void OnQuestFailedWithRejectionOrTimeout()
		{
			((QuestBase)this).AddLog(OnQuestFailedWithRejectionLogText, false);
			TraitLevelingHelper.OnIssueFailed(((QuestBase)this).QuestGiver, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -20)
			});
			((QuestBase)this).RelationshipChangeWithQuestGiver = -5;
			ApplyQuestFailConsequences();
		}

		private void OnBattleWonWithBetrayal()
		{
			((QuestBase)this).AddLog(OnQuestFailedWithBetrayalLogText, false);
			((QuestBase)this).RelationshipChangeWithQuestGiver = -15;
			if (!_rivalGangLeader.IsDead)
			{
				ChangeRelationAction.ApplyPlayerRelation(_rivalGangLeader, 5, true, true);
			}
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, _rewardGold * 2, false);
			TraitLevelingHelper.OnIssueSolvedThroughBetrayal(((QuestBase)this).QuestGiver, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -100)
			});
			_rivalGangLeader.AddPower(10f);
			GameMenu.SwitchToMenu("town");
			ApplyQuestFailConsequences();
			((QuestBase)this).CompleteQuestWithBetrayal((TextObject)null);
		}

		private void OnBattleLostWithBetrayal()
		{
			((QuestBase)this).AddLog(OnQuestFailedWithBetrayalLogText, false);
			((QuestBase)this).RelationshipChangeWithQuestGiver = -10;
			if (!_rivalGangLeader.IsDead)
			{
				ChangeRelationAction.ApplyPlayerRelation(_rivalGangLeader, -5, true, true);
			}
			_rivalGangLeader.AddPower(-10f);
			TraitLevelingHelper.OnIssueSolvedThroughBetrayal(((QuestBase)this).QuestGiver, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -100)
			});
			GameMenu.SwitchToMenu("town");
			ApplyQuestFailConsequences();
			((QuestBase)this).CompleteQuestWithBetrayal((TextObject)null);
		}

		private void OnQuestFailedWithDefeat()
		{
			((QuestBase)this).RelationshipChangeWithQuestGiver = -5;
			GameMenu.SwitchToMenu("town");
			((QuestBase)this).AddLog(OnQuestFailedWithDefeatLogText, false);
			ApplyQuestFailConsequences();
			((QuestBase)this).CompleteQuestWithFail((TextObject)null);
		}

		private void ApplyQuestFailConsequences()
		{
			((QuestBase)this).QuestGiver.AddPower(-10f);
			Town town = _questSettlement.Town;
			town.Security += -10f;
			if (_rivalGangLeaderParty != null && _rivalGangLeaderParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, _rivalGangLeaderParty);
			}
		}

		protected override void OnFinalize()
		{
			if (_rivalGangLeaderParty != null && _rivalGangLeaderParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, _rivalGangLeaderParty);
			}
			if (_allyGangLeaderParty != null && _allyGangLeaderParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, _allyGangLeaderParty);
			}
			if (_allyGangLeaderHenchmanHero != null && _allyGangLeaderHenchmanHero.IsAlive)
			{
				_allyGangLeaderHenchmanHero.SetNewOccupation((Occupation)31);
				KillCharacterAction.ApplyByRemove(_allyGangLeaderHenchmanHero, false, true);
			}
			if (_rivalGangLeaderHenchmanHero != null && _rivalGangLeaderHenchmanHero.IsAlive)
			{
				_rivalGangLeaderHenchmanHero.SetNewOccupation((Occupation)0);
				KillCharacterAction.ApplyByRemove(_rivalGangLeaderHenchmanHero, false, true);
			}
		}

		internal static void AutoGeneratedStaticCollectObjectsRivalGangMovingInIssueQuest(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(RivalGangMovingInIssueQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_rivalGangLeader);
			collectedObjects.Add(_rivalGangLeaderParty);
			CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime((object)_preparationCompletionTime, collectedObjects);
			CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime((object)_questTimeoutTime, collectedObjects);
		}

		internal static object AutoGeneratedGetMemberValue_rivalGangLeader(object o)
		{
			return ((RivalGangMovingInIssueQuest)o)._rivalGangLeader;
		}

		internal static object AutoGeneratedGetMemberValue_timeoutDurationInDays(object o)
		{
			return ((RivalGangMovingInIssueQuest)o)._timeoutDurationInDays;
		}

		internal static object AutoGeneratedGetMemberValue_isFinalStage(object o)
		{
			return ((RivalGangMovingInIssueQuest)o)._isFinalStage;
		}

		internal static object AutoGeneratedGetMemberValue_isReadyToBeFinalized(object o)
		{
			return ((RivalGangMovingInIssueQuest)o)._isReadyToBeFinalized;
		}

		internal static object AutoGeneratedGetMemberValue_hasBetrayedQuestGiver(object o)
		{
			return ((RivalGangMovingInIssueQuest)o)._hasBetrayedQuestGiver;
		}

		internal static object AutoGeneratedGetMemberValue_rivalGangLeaderParty(object o)
		{
			return ((RivalGangMovingInIssueQuest)o)._rivalGangLeaderParty;
		}

		internal static object AutoGeneratedGetMemberValue_preparationCompletionTime(object o)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((RivalGangMovingInIssueQuest)o)._preparationCompletionTime;
		}

		internal static object AutoGeneratedGetMemberValue_questTimeoutTime(object o)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((RivalGangMovingInIssueQuest)o)._questTimeoutTime;
		}

		internal static object AutoGeneratedGetMemberValue_preparationsComplete(object o)
		{
			return ((RivalGangMovingInIssueQuest)o)._preparationsComplete;
		}

		internal static object AutoGeneratedGetMemberValue_rewardGold(object o)
		{
			return ((RivalGangMovingInIssueQuest)o)._rewardGold;
		}

		internal static object AutoGeneratedGetMemberValue_issueDifficulty(object o)
		{
			return ((RivalGangMovingInIssueQuest)o)._issueDifficulty;
		}
	}

	private const IssueFrequency RivalGangLeaderIssueFrequency = (IssueFrequency)1;

	private RivalGangMovingInIssueQuest _cachedQuest;

	private static RivalGangMovingInIssueQuest Instance
	{
		get
		{
			RivalGangMovingInIssueBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<RivalGangMovingInIssueBehavior>();
			if (campaignBehavior._cachedQuest != null && ((QuestBase)campaignBehavior._cachedQuest).IsOngoing)
			{
				return campaignBehavior._cachedQuest;
			}
			foreach (QuestBase item in (List<QuestBase>)(object)Campaign.Current.QuestManager.Quests)
			{
				if (item is RivalGangMovingInIssueQuest cachedQuest)
				{
					campaignBehavior._cachedQuest = cachedQuest;
					return campaignBehavior._cachedQuest;
				}
			}
			return null;
		}
	}

	private void OnCheckForIssue(Hero hero)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (ConditionsHold(hero))
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new StartIssueDelegate(OnStartIssue), typeof(RivalGangMovingInIssue), (IssueFrequency)1, (object)null));
		}
		else
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(RivalGangMovingInIssue), (IssueFrequency)1));
		}
	}

	private IssueBase OnStartIssue(in PotentialIssueData pid, Hero issueOwner)
	{
		Hero rivalGangLeader = GetRivalGangLeader(issueOwner);
		return (IssueBase)(object)new RivalGangMovingInIssue(issueOwner, rivalGangLeader);
	}

	private static void rival_gang_wait_duration_is_over_menu_on_init(MenuCallbackArgs args)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
		TextObject val = new TextObject("{=9Kr9pjGs}{QUEST_GIVER.LINK} has prepared {?QUEST_GIVER.GENDER}her{?}his{\\?} men and is waiting for you.", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)Instance).QuestGiver.CharacterObject, (TextObject)null, false);
		MBTextManager.SetTextVariable("MENU_TEXT", val, false);
	}

	private bool ConditionsHold(Hero issueGiver)
	{
		if (issueGiver.IsGangLeader && issueGiver.CurrentSettlement != null && issueGiver.CurrentSettlement.IsTown && issueGiver.CurrentSettlement.Town.Security <= 60f)
		{
			return GetRivalGangLeader(issueGiver) != null;
		}
		return false;
	}

	private void rival_gang_quest_wait_duration_is_over_yes_consequence(MenuCallbackArgs args)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, true, false, false, false, false), new ConversationCharacterData(((QuestBase)Instance).QuestGiver.CharacterObject, (PartyBase)null, true, true, false, false, false, false));
	}

	private Hero GetRivalGangLeader(Hero issueOwner)
	{
		Hero result = null;
		foreach (Hero item in (List<Hero>)(object)issueOwner.CurrentSettlement.Notables)
		{
			if (item != issueOwner && item.IsGangLeader && item.CanHaveCampaignIssues())
			{
				result = item;
				break;
			}
		}
		return result;
	}

	private bool rival_gang_quest_wait_duration_is_over_yes_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		return true;
	}

	private bool rival_gang_quest_wait_duration_is_over_no_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnCheckForIssue);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
	}

	private void OnSessionLaunched(CampaignGameStarter gameStarter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_008e: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00bf: Expected O, but got Unknown
		gameStarter.AddGameMenu("rival_gang_quest_before_fight", "", new OnInitDelegate(rival_gang_quest_before_fight_init), (MenuOverlayType)3, (MenuFlags)0, (object)null);
		gameStarter.AddGameMenu("rival_gang_quest_after_fight", "", new OnInitDelegate(rival_gang_quest_after_fight_init), (MenuOverlayType)3, (MenuFlags)0, (object)null);
		gameStarter.AddGameMenu("rival_gang_quest_wait_duration_is_over", "{MENU_TEXT}", new OnInitDelegate(rival_gang_wait_duration_is_over_menu_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		gameStarter.AddGameMenuOption("rival_gang_quest_wait_duration_is_over", "rival_gang_quest_wait_duration_is_over_yes", "{=aka03VdU}Meet {?QUEST_GIVER.GENDER}her{?}him{\\?} now", new OnConditionDelegate(rival_gang_quest_wait_duration_is_over_yes_condition), new OnConsequenceDelegate(rival_gang_quest_wait_duration_is_over_yes_consequence), false, -1, false, (object)null);
		gameStarter.AddGameMenuOption("rival_gang_quest_wait_duration_is_over", "rival_gang_quest_wait_duration_is_over_no", "{=NIzQb6nT}Leave and meet {?QUEST_GIVER.GENDER}her{?}him{\\?} later", new OnConditionDelegate(rival_gang_quest_wait_duration_is_over_no_condition), new OnConsequenceDelegate(rival_gang_quest_wait_duration_is_over_no_consequence), true, -1, false, (object)null);
	}

	private void rival_gang_quest_wait_duration_is_over_no_consequence(MenuCallbackArgs args)
	{
		Campaign.Current.CurrentMenuContext.SwitchToMenu("town_wait_menus");
	}

	private static void rival_gang_quest_before_fight_init(MenuCallbackArgs args)
	{
		if (Instance != null && Instance._isFinalStage)
		{
			Instance.StartAlleyBattle();
		}
	}

	private static void rival_gang_quest_after_fight_init(MenuCallbackArgs args)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (Instance != null && Instance._isReadyToBeFinalized)
		{
			bool hasPlayerWon = PlayerEncounter.Battle.WinningSide == PlayerEncounter.Battle.PlayerSide;
			PlayerEncounter.Current.FinalizeBattle();
			Instance.HandlePlayerEncounterResult(hasPlayerWon);
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	[GameMenuInitializationHandler("rival_gang_quest_after_fight")]
	[GameMenuInitializationHandler("rival_gang_quest_wait_duration_is_over")]
	private static void game_menu_rival_gang_quest_end_on_init(MenuCallbackArgs args)
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement != null)
		{
			args.MenuContext.SetBackgroundMeshName(currentSettlement.SettlementComponent.WaitMeshName);
		}
	}
}
