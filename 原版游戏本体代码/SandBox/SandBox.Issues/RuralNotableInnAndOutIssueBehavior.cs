using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using SandBox.BoardGames.MissionLogics;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace SandBox.Issues;

public class RuralNotableInnAndOutIssueBehavior : CampaignBehaviorBase
{
	public class RuralNotableInnAndOutIssueTypeDefiner : SaveableTypeDefiner
	{
		public RuralNotableInnAndOutIssueTypeDefiner()
			: base(585900)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(RuralNotableInnAndOutIssue), 1, (IObjectResolver)null);
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(RuralNotableInnAndOutIssueQuest), 2, (IObjectResolver)null);
		}
	}

	public class RuralNotableInnAndOutIssue : IssueBase
	{
		private const int CompanionSkillLimit = 120;

		private const int QuestMoneyLimit = 2000;

		private const int AlternativeSolutionGoldCost = 1000;

		private BoardGameType _boardGameType;

		private Settlement _targetSettlement;

		public override AlternativeSolutionScaleFlag AlternativeSolutionScaleFlags => (AlternativeSolutionScaleFlag)8;

		protected override bool IssueQuestCanBeDuplicated => false;

		public override int AlternativeSolutionBaseNeededMenCount => 1 + MathF.Ceiling(3f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int AlternativeSolutionBaseDurationInDaysInternal => 1 + MathF.Ceiling(3f * ((IssueBase)this).IssueDifficultyMultiplier);

		protected override int RewardGold => 1000;

		public override TextObject Title => new TextObject("{=uUhtKnfA}Inn and Out", (Dictionary<string, object>)null);

		public override TextObject Description
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=swamqBRq}{ISSUE_OWNER.NAME} wants you to beat the game host", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_OWNER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject IssueBriefByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Expected O, but got Unknown
				TextObject val = new TextObject("{=T0zupcGB}Ah yes... It is a bit embarrassing to mention, [ib:nervous][if:convo_nervous]but... Well, when I am in town, I often have a drink at the inn and perhaps play a round of {GAME_TYPE} or two. Normally I play for low stakes but let's just say that last time the wine went to my head, and I lost something I couldn't afford to lose.", (Dictionary<string, object>)null);
				val.SetTextVariable("GAME_TYPE", GameTexts.FindText("str_boardgame_name", ((object)Unsafe.As<BoardGameType, BoardGameType>(ref _boardGameType)/*cast due to .constrained prefix*/).ToString()));
				return val;
			}
		}

		public override TextObject IssueAcceptByPlayer => new TextObject("{=h2tMadtI}I've heard that story before. What did you lose?", (Dictionary<string, object>)null);

		public override TextObject IssueQuestSolutionExplanationByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Expected O, but got Unknown
				TextObject val = new TextObject("{=LD4tGYCA}It's a deed to a plot of farmland. Not a big or valuable plot,[ib:normal][if:convo_disbelief] mind you, but I'd rather not have to explain to my men why they won't be sowing it this year. You can find the man who took it from me at the tavern in {TARGET_SETTLEMENT}. They call him the \"Game Host\". Just be straight about what you're doing. He's in no position to work the land. I don't imagine that he'll turn down a chance to make more money off of it. Bring it back and {REWARD}{GOLD_ICON} is yours.", (Dictionary<string, object>)null);
				val.SetTextVariable("REWARD", ((IssueBase)this).RewardGold);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.Name);
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
				//IL_002f: Expected O, but got Unknown
				TextObject val = new TextObject("{=urCXu9Fc}Well, I could try and buy it from him, but I would not really prefer that.[if:convo_innocent_smile] I would be the joke of the tavern for months to come... If you choose to do that, I can only offer {REWARD}{GOLD_ICON} to compensate for your payment. If you have a man with a knack for such games he might do the trick.", (Dictionary<string, object>)null);
				val.SetTextVariable("REWARD", ((IssueBase)this).RewardGold);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				return val;
			}
		}

		public override TextObject IssueQuestSolutionAcceptByPlayer => new TextObject("{=KMThnMbt}I'll go to the tavern and win it back the same way you lost it.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionAcceptByPlayer
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=QdKWaabR}Worry not {ISSUE_OWNER.NAME}, my men will be back with your deed in no time.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("ISSUE_OWNER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject IssueDiscussAlternativeSolution => new TextObject("{=1yEyUHJe}I really hope your men can get my deed back. [if:convo_excited]On my father's name, I will never gamble again.", (Dictionary<string, object>)null);

		public override TextObject IssueAlternativeSolutionResponseByIssueGiver
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=kiaN39yb}Thank you, {PLAYER.NAME}. I'm sure your companion will be persuasive.[if:convo_relaxed_happy]", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
				return val;
			}
		}

		public override bool IsThereAlternativeSolution => true;

		public override bool IsThereLordSolution => false;

		protected override TextObject AlternativeSolutionStartLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=MIxzaqzi}{QUEST_GIVER.LINK} told you that he lost a land deed in a wager in {TARGET_CITY}. He needs to buy it back, and he wants your companions to intimidate the seller into offering a reasonable price. You asked {COMPANION.LINK} to take {TROOP_COUNT} of your men to go and take care of it. They should report back to you in {RETURN_DAYS} days.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((IssueBase)this).IssueOwner.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("COMPANION", ((IssueBase)this).AlternativeSolutionHero.CharacterObject, val, false);
				val.SetTextVariable("TARGET_CITY", _targetSettlement.EncyclopediaLinkWithName);
				val.SetTextVariable("RETURN_DAYS", ((IssueBase)this).GetTotalAlternativeSolutionDurationInDays());
				val.SetTextVariable("TROOP_COUNT", base.AlternativeSolutionSentTroops.TotalManCount - 1);
				return val;
			}
		}

		protected override int CompanionSkillRewardXP => (int)(500f + 1000f * ((IssueBase)this).IssueDifficultyMultiplier);

		public RuralNotableInnAndOutIssue(Hero issueOwner)
			: base(issueOwner, CampaignTime.DaysFromNow(30f))
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			InitializeQuestVariables();
		}

		protected override float GetIssueEffectAmountInternal(IssueEffect issueEffect)
		{
			if (issueEffect == DefaultIssueEffects.SettlementProsperity)
			{
				return -0.1f;
			}
			if (issueEffect == DefaultIssueEffects.IssueOwnerPower)
			{
				return -0.1f;
			}
			return 0f;
		}

		public override (SkillObject, int) GetAlternativeSolutionSkill(Hero hero)
		{
			return ((hero.GetSkillValue(DefaultSkills.Charm) >= hero.GetSkillValue(DefaultSkills.Tactics)) ? DefaultSkills.Charm : DefaultSkills.Tactics, 120);
		}

		public override bool AlternativeSolutionCondition(out TextObject explanation)
		{
			if (QuestHelper.CheckRosterForAlternativeSolution(MobileParty.MainParty.MemberRoster, ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount(), ref explanation, 0, false))
			{
				return QuestHelper.CheckGoldForAlternativeSolution(1000, ref explanation);
			}
			return false;
		}

		protected override void AlternativeSolutionEndWithSuccessConsequence()
		{
			((IssueBase)this).RelationshipChangeWithIssueOwner = 5;
			GainRenownAction.Apply(Hero.MainHero, 5f, false);
			Town town = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
			town.Loyalty += 5f;
		}

		protected override void AlternativeSolutionEndWithFailureConsequence()
		{
			((IssueBase)this).RelationshipChangeWithIssueOwner = ((IssueBase)this).RelationshipChangeWithIssueOwner - 5;
			Town town = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound.Town;
			town.Loyalty -= 5f;
		}

		public override bool DoTroopsSatisfyAlternativeSolution(TroopRoster troopRoster, out TextObject explanation)
		{
			return QuestHelper.CheckRosterForAlternativeSolution(troopRoster, ((IssueBase)this).GetTotalAlternativeSolutionNeededMenCount(), ref explanation, 0, false);
		}

		public override IssueFrequency GetFrequency()
		{
			return (IssueFrequency)1;
		}

		public override bool IssueStayAliveConditions()
		{
			BoardGameCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<BoardGameCampaignBehavior>();
			if (campaignBehavior != null && !campaignBehavior.WonBoardGamesInOneWeekInSettlement.Contains(_targetSettlement) && !((IssueBase)this).IssueOwner.CurrentSettlement.IsRaided)
			{
				return !((IssueBase)this).IssueOwner.CurrentSettlement.IsUnderRaid;
			}
			return false;
		}

		protected override void CompleteIssueWithTimedOutConsequences()
		{
		}

		private void InitializeQuestVariables()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			_targetSettlement = ((IssueBase)this).IssueOwner.CurrentSettlement.Village.Bound;
			_boardGameType = _targetSettlement.Culture.BoardGame;
		}

		protected override void OnGameLoad()
		{
			InitializeQuestVariables();
		}

		protected override void HourlyTick()
		{
		}

		protected override QuestBase GenerateIssueQuest(string questId)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return (QuestBase)(object)new RuralNotableInnAndOutIssueQuest(questId, ((IssueBase)this).IssueOwner, CampaignTime.DaysFromNow(14f), ((IssueBase)this).RewardGold);
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
			if (Hero.MainHero.Gold < 2000)
			{
				requiredGold = 2000;
				flag = (PreconditionFlags)((uint)flag | 4u);
			}
			return (int)flag == 0;
		}

		internal static void AutoGeneratedStaticCollectObjectsRuralNotableInnAndOutIssue(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(RuralNotableInnAndOutIssue)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((IssueBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}
	}

	public class RuralNotableInnAndOutIssueQuest : QuestBase
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static OnConditionDelegate _003C_003E9__41_1;

			public static OnConditionDelegate _003C_003E9__41_2;

			internal bool _003CGetGameHostDialogFlow_003Eb__41_1()
			{
				return Hero.MainHero.Gold >= 1000;
			}

			internal bool _003CGetGameHostDialogFlow_003Eb__41_2()
			{
				return Hero.MainHero.Gold < 1000;
			}
		}

		public const int LesserReward = 800;

		private BoardGameType _boardGameType;

		private Settlement _targetSettlement;

		private bool _checkForBoardGameEnd;

		private bool _playerWonTheGame;

		private bool _applyLesserReward;

		[SaveableField(1)]
		private int _tryCount;

		private TextObject QuestStartLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=tirG1BB2}{QUEST_GIVER.LINK} told you that he lost a land deed while playing games in a tavern in {TARGET_SETTLEMENT}. He wants you to go find the game host and win it back for him. You told him that you will take care of the situation yourself.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject SuccessLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=bvhWLb4C}You defeated the Game Host and got the deed back. {QUEST_GIVER.LINK}.{newline}\"Thank you for resolving this issue so neatly. Please accept these {GOLD}{GOLD_ICON} denars with our gratitude.\"", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("GOLD", base.RewardGold);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				return val;
			}
		}

		private TextObject SuccessWithPayingLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=TIPxWsYW}You have bought the deed from the game host. {QUEST_GIVER.LINK}.{newline}\"I am happy that I got my land back. I'm not so happy that everyone knows I had to pay for it, but... Anyway, please accept these {GOLD}{GOLD_ICON} denars with my gratitude.\"", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("GOLD", 800);
				val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
				return val;
			}
		}

		private TextObject LostLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=ye4oqBFB}You lost the board game and failed to help {QUEST_GIVER.LINK}. \"Thank you for trying, {PLAYER.NAME}, but I guess I chose the wrong person for the job.\"", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
				return val;
			}
		}

		private TextObject QuestCanceledTargetVillageRaided
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=DLesz9jI}{QUEST_GIVER.LINK}’s village is raided. {?QUEST_GIVER.GENDER}She{?}He{\\?} flees to the countryside, and your agreement with {?QUEST_GIVER.GENDER}her{?}him{\\?} is canceled.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject QuestCanceledWarDeclared
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Expected O, but got Unknown
				TextObject val = new TextObject("{=cKz1cyuM}Your clan is now at war with {QUEST_GIVER_SETTLEMENT_FACTION}. Quest is canceled.", (Dictionary<string, object>)null);
				val.SetTextVariable("QUEST_GIVER_SETTLEMENT_FACTION", ((QuestBase)this).QuestGiver.CurrentSettlement.MapFaction.Name);
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

		private TextObject QuestCanceledSettlementIsUnderSiege
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=b5LdBYpF}{SETTLEMENT} is under siege. Your agreement with {QUEST_GIVER.LINK} is canceled.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				val.SetTextVariable("SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject TimeoutLog
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=XLy8anVr}You received a message from {QUEST_GIVER.LINK}. \"This may not have seemed like an important task, but I placed my trust in you. I guess I was wrong to do so.\"", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("QUEST_GIVER", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject Title => new TextObject("{=uUhtKnfA}Inn and Out", (Dictionary<string, object>)null);

		public override bool IsRemainingTimeHidden => false;

		public RuralNotableInnAndOutIssueQuest(string questId, Hero giverHero, CampaignTime duration, int rewardGold)
			: base(questId, giverHero, duration, rewardGold)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			InitializeQuestVariables();
			((QuestBase)this).SetDialogs();
			((QuestBase)this).InitializeQuestOnCreation();
		}

		private void InitializeQuestVariables()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			_targetSettlement = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound;
			_boardGameType = _targetSettlement.Culture.BoardGame;
		}

		private void QuestAcceptedConsequences()
		{
			((QuestBase)this).StartQuest();
			((QuestBase)this).AddLog(QuestStartLog, false);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_targetSettlement);
		}

		protected override void InitializeQuestOnGameLoad()
		{
			InitializeQuestVariables();
			((QuestBase)this).SetDialogs();
			if (Campaign.Current.GetCampaignBehavior<BoardGameCampaignBehavior>() == null)
			{
				((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
			}
		}

		protected override void HourlyTick()
		{
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.OnPlayerBoardGameOverEvent.AddNonSerializedListener((object)this, (Action<Hero, BoardGameState>)OnBoardGameEnd);
			CampaignEvents.WarDeclared.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, DeclareWarDetail>)OnWarDeclared);
			CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
			CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener((object)this, (Action<SiegeEvent>)OnSiegeStarted);
			CampaignEvents.MapEventStarted.AddNonSerializedListener((object)this, (Action<MapEvent, PartyBase, PartyBase>)OnMapEventStarted);
			CampaignEvents.VillageBeingRaided.AddNonSerializedListener((object)this, (Action<Village>)OnVillageBeingRaided);
			CampaignEvents.LocationCharactersSimulatedEvent.AddNonSerializedListener((object)this, (Action)OnLocationCharactersSimulated);
		}

		private void OnLocationCharactersSimulated()
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Invalid comparison between Unknown and I4
			if (Settlement.CurrentSettlement == null || Settlement.CurrentSettlement != _targetSettlement || Campaign.Current.GameMenuManager.MenuLocations.Count <= 0 || !(Campaign.Current.GameMenuManager.MenuLocations[0].StringId == "tavern"))
			{
				return;
			}
			foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
			{
				LocationCharacter locationCharacter = LocationComplex.Current.GetLocationWithId("tavern").GetLocationCharacter(item.Origin);
				if (locationCharacter != null && (int)locationCharacter.Character.Occupation == 14)
				{
					locationCharacter.IsVisualTracked = true;
				}
			}
		}

		private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
		{
			if (QuestHelper.CheckMinorMajorCoercion((QuestBase)(object)this, mapEvent, attackerParty))
			{
				QuestHelper.ApplyGenericMinorMajorCoercionConsequences((QuestBase)(object)this, mapEvent);
			}
		}

		private void OnVillageBeingRaided(Village village)
		{
			if (village == ((QuestBase)this).QuestGiver.CurrentSettlement.Village)
			{
				((QuestBase)this).CompleteQuestWithCancel(QuestCanceledTargetVillageRaided);
			}
		}

		private void OnBoardGameEnd(Hero opposingHero, BoardGameState state)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			if (_checkForBoardGameEnd)
			{
				_playerWonTheGame = (int)state == 1;
			}
		}

		private void OnSiegeStarted(SiegeEvent siegeEvent)
		{
			if (siegeEvent.BesiegedSettlement == _targetSettlement)
			{
				((QuestBase)this).CompleteQuestWithCancel(QuestCanceledSettlementIsUnderSiege);
			}
		}

		protected override void SetDialogs()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected O, but got Unknown
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Expected O, but got Unknown
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Expected O, but got Unknown
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Expected O, but got Unknown
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Expected O, but got Unknown
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Expected O, but got Unknown
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Expected O, but got Unknown
			TextObject val = new TextObject("{=I6amLvVE}Good, good. That's the best way to do these things. [if:convo_normal]Go to {TARGET_SETTLEMENT}, find this game host and wipe the smirk off of his face.", (Dictionary<string, object>)null);
			val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.Name);
			base.OfferDialogFlow = DialogFlow.CreateDialogFlow("issue_classic_quest_start", 100).NpcLine(val, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver))
				.Consequence(new OnConsequenceDelegate(QuestAcceptedConsequences))
				.CloseDialog();
			base.DiscussDialogFlow = DialogFlow.CreateDialogFlow("quest_discuss", 100).NpcLine(new TextObject("{=HGRWs0zE}Have you met the man who took my deed? Did you get it back?[if:convo_astonished]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver))
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(new TextObject("{=uJPAYUU7}I will be on my way soon enough.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=MOmePlJQ}Could you hurry this along? I don't want him to find another buyer.[if:convo_pondering] Thank you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.PlayerOption(new TextObject("{=azVhRGik}I am waiting for the right moment.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=bRMLn0jj}Well, if he wanders off to another town, or gets his throat slit,[if:convo_pondering] or loses the deed, that would be the wrong moment, now wouldn't it?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.EndPlayerOptions();
			Campaign.Current.ConversationManager.AddDialogFlow(GetGameHostDialogFlow(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(GetGameHostDialogueAfterFirstGame(), (object)this);
		}

		private DialogFlow GetGameHostDialogFlow()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Expected O, but got Unknown
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Expected O, but got Unknown
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Expected O, but got Unknown
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Expected O, but got Unknown
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Expected O, but got Unknown
			DialogFlow obj = DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=dzWioKRa}Hello there, are you looking for a friendly match? A wager perhaps?[if:convo_mocking_aristocratic]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => TavernHostDialogCondition(isInitialDialogue: true)))
				.PlayerLine(new TextObject("{=eOle8pYT}You won a deed of land from my associate. I'm here to win it back.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine("{=bEipgE5E}Ah, yes, these are the most interesting kinds of games, aren't they? [if:convo_excited]I won't deny myself the pleasure but clearly that deed is worth more to him than just the value of the land. I'll wager the deed, but you need to put up 1000 denars.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=XvkSbY6N}I see your wager. Let's play.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
			object obj2 = _003C_003Ec._003C_003E9__41_1;
			if (obj2 == null)
			{
				OnConditionDelegate val = () => Hero.MainHero.Gold >= 1000;
				_003C_003Ec._003C_003E9__41_1 = val;
				obj2 = (object)val;
			}
			DialogFlow obj3 = obj.Condition((OnConditionDelegate)obj2).Consequence(new OnConsequenceDelegate(StartBoardGame)).CloseDialog()
				.PlayerOption("{=89b5ao7P}As of now, I do not have 1000 denars to afford on gambling. I may get back to you once I get the required amount.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
			object obj4 = _003C_003Ec._003C_003E9__41_2;
			if (obj4 == null)
			{
				OnConditionDelegate val2 = () => Hero.MainHero.Gold < 1000;
				_003C_003Ec._003C_003E9__41_2 = val2;
				obj4 = (object)val2;
			}
			return obj3.Condition((OnConditionDelegate)obj4).NpcLine(new TextObject("{=ppi6eVos}As you wish.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
				.PlayerOption("{=WrnvRayQ}Let's just save ourselves some trouble, and I'll just pay you that amount.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.ClickableCondition(new OnClickableConditionDelegate(CheckPlayerHasEnoughDenarsClickableCondition))
				.NpcLine("{=pa3RY39w}Sure. I'm happy to turn paper into silver... 1000 denars it is.[if:convo_evil_smile]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(PlayerPaid1000QuestSuccess))
				.CloseDialog()
				.PlayerOption("{=BSeplVwe}That's too much. I will be back later.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog();
		}

		private DialogFlow GetGameHostDialogueAfterFirstGame()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			//IL_0033: Expected O, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Expected O, but got Unknown
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Expected O, but got Unknown
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Expected O, but got Unknown
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Expected O, but got Unknown
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Expected O, but got Unknown
			return DialogFlow.CreateDialogFlow("start", 125).BeginNpcOptions((string)null, false).NpcOption(new TextObject("{=dyhZUHao}Well, I thought you were here to be sheared, [if:convo_shocked]but it looks like the sheep bites back. Very well, nicely played, here's your man's land back.", (Dictionary<string, object>)null), (OnConditionDelegate)(() => _playerWonTheGame && TavernHostDialogCondition()), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(PlayerWonTheBoardGame))
				.CloseDialog()
				.NpcOption("{=TdnD29Ax}Ah! You almost had me! Maybe you just weren't paying attention. [if:convo_mocking_teasing]Care to put another 1000 denars on the table and have another go?", (OnConditionDelegate)(() => !_playerWonTheGame && _tryCount < 2 && TavernHostDialogCondition()), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption("{=fiMZ696A}Yes, I'll play again.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.ClickableCondition(new OnClickableConditionDelegate(CheckPlayerHasEnoughDenarsClickableCondition))
				.Consequence(new OnConsequenceDelegate(StartBoardGame))
				.CloseDialog()
				.PlayerOption("{=zlFSIvD5}No, no. I know a trap when I see one. You win. Good-bye.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=ppi6eVos}As you wish.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(PlayerFailAfterBoardGame))
				.CloseDialog()
				.EndPlayerOptions()
				.NpcOption("{=hkNrC5d3}That was fun, but I've learned not to inflict too great a humiliation on those who carry a sword.[if:convo_merry] I'll take my winnings and enjoy them now. Good-bye to you!", (OnConditionDelegate)(() => _tryCount >= 2 && TavernHostDialogCondition()), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(PlayerFailAfterBoardGame))
				.CloseDialog()
				.EndNpcOptions();
		}

		private bool CheckPlayerHasEnoughDenarsClickableCondition(out TextObject explanation)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			if (Hero.MainHero.Gold >= 1000)
			{
				explanation = null;
				return true;
			}
			explanation = new TextObject("{=AMlaYbJv}You don't have 1000 denars.", (Dictionary<string, object>)null);
			return false;
		}

		private bool TavernHostDialogCondition(bool isInitialDialogue = false)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Invalid comparison between Unknown and I4
			if ((!_checkForBoardGameEnd || !isInitialDialogue) && Settlement.CurrentSettlement == _targetSettlement && (int)CharacterObject.OneToOneConversationCharacter.Occupation == 14)
			{
				LocationComplex current = LocationComplex.Current;
				if (((current != null) ? current.GetLocationWithId("tavern") : null) != null)
				{
					Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().DetectOpposingAgent();
					return Mission.Current.GetMissionBehavior<MissionBoardGameLogic>().CheckIfBothSidesAreSitting();
				}
			}
			return false;
		}

		private void PlayerPaid1000QuestSuccess()
		{
			((QuestBase)this).AddLog(SuccessWithPayingLog, false);
			_applyLesserReward = true;
			GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, 1000, false);
			((QuestBase)this).CompleteQuestWithSuccess();
		}

		protected override void OnFinalize()
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Invalid comparison between Unknown and I4
			if (Mission.Current == null)
			{
				return;
			}
			foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
			{
				Location locationWithId = LocationComplex.Current.GetLocationWithId("tavern");
				if (locationWithId != null)
				{
					LocationCharacter locationCharacter = locationWithId.GetLocationCharacter(item.Origin);
					if (locationCharacter != null && (int)locationCharacter.Character.Occupation == 14)
					{
						locationCharacter.IsVisualTracked = false;
					}
				}
			}
		}

		private void ApplySuccessRewards()
		{
			GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, _applyLesserReward ? 800 : base.RewardGold, false);
			ChangeRelationAction.ApplyPlayerRelation(((QuestBase)this).QuestGiver, 5, true, true);
			GainRenownAction.Apply(Hero.MainHero, 1f, false);
			Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
			town.Loyalty += 5f;
		}

		protected override void OnCompleteWithSuccess()
		{
			ApplySuccessRewards();
		}

		private void StartBoardGame()
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			MissionBoardGameLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
			Campaign.Current.GetCampaignBehavior<BoardGameCampaignBehavior>().SetBetAmount(1000);
			missionBehavior.DetectOpposingAgent();
			missionBehavior.SetCurrentDifficulty((AIDifficulty)1);
			missionBehavior.SetBoardGame(_boardGameType);
			missionBehavior.StartBoardGame();
			_checkForBoardGameEnd = true;
			_tryCount++;
		}

		private void PlayerWonTheBoardGame()
		{
			((QuestBase)this).AddLog(SuccessLog, false);
			((QuestBase)this).CompleteQuestWithSuccess();
		}

		private void PlayerFailAfterBoardGame()
		{
			((QuestBase)this).AddLog(LostLog, false);
			((QuestBase)this).RelationshipChangeWithQuestGiver = -5;
			Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
			town.Loyalty -= 5f;
			((QuestBase)this).CompleteQuestWithFail((TextObject)null);
		}

		private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
		{
			if (((QuestBase)this).QuestGiver.CurrentSettlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction))
			{
				((QuestBase)this).CompleteQuestWithCancel(QuestCanceledWarDeclared);
			}
		}

		private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarDetail detail)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			QuestHelper.CheckWarDeclarationAndFailOrCancelTheQuest((QuestBase)(object)this, faction1, faction2, detail, PlayerDeclaredWarQuestLogText, QuestCanceledWarDeclared, false);
		}

		public override IssueQuestFlags IsLocationTrackedByQuest(Location location)
		{
			if (PlayerEncounter.LocationEncounter.Settlement == _targetSettlement && location.StringId == "tavern")
			{
				return (IssueQuestFlags)2;
			}
			return (IssueQuestFlags)0;
		}

		protected override void OnTimedOut()
		{
			((QuestBase)this).RelationshipChangeWithQuestGiver = -5;
			Town town = ((QuestBase)this).QuestGiver.CurrentSettlement.Village.Bound.Town;
			town.Loyalty -= 5f;
			((QuestBase)this).AddLog(TimeoutLog, false);
		}

		internal static void AutoGeneratedStaticCollectObjectsRuralNotableInnAndOutIssueQuest(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(RuralNotableInnAndOutIssueQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		internal static object AutoGeneratedGetMemberValue_tryCount(object o)
		{
			return ((RuralNotableInnAndOutIssueQuest)o)._tryCount;
		}
	}

	private const IssueFrequency RuralNotableInnAndOutIssueFrequency = (IssueFrequency)1;

	private const float IssueDuration = 30f;

	private const float QuestDuration = 14f;

	public override void RegisterEvents()
	{
		CampaignEvents.OnCheckForIssueEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnCheckForIssue);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private bool ConditionsHold(Hero issueGiver)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Invalid comparison between Unknown and I4
		if ((issueGiver.IsRuralNotable || issueGiver.IsHeadman) && issueGiver.CurrentSettlement.Village != null && issueGiver.CurrentSettlement.Village.Bound.IsTown && issueGiver.GetTraitLevel(DefaultTraits.Mercy) + issueGiver.GetTraitLevel(DefaultTraits.Honor) < 0 && Campaign.Current.GetCampaignBehavior<BoardGameCampaignBehavior>() != null)
		{
			return (int)issueGiver.CurrentSettlement.Village.Bound.Culture.BoardGame != -1;
		}
		return false;
	}

	public void OnCheckForIssue(Hero hero)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (ConditionsHold(hero))
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(new StartIssueDelegate(OnSelected), typeof(RuralNotableInnAndOutIssue), (IssueFrequency)1, (object)null));
		}
		else
		{
			Campaign.Current.IssueManager.AddPotentialIssueData(hero, new PotentialIssueData(typeof(RuralNotableInnAndOutIssue), (IssueFrequency)1));
		}
	}

	private IssueBase OnSelected(in PotentialIssueData pid, Hero issueOwner)
	{
		return (IssueBase)(object)new RuralNotableInnAndOutIssue(issueOwner);
	}
}
