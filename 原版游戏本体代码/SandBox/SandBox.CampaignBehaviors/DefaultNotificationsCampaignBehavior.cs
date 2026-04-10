using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.CampaignBehaviors;

public class DefaultNotificationsCampaignBehavior : CampaignBehaviorBase
{
	private List<Tuple<bool, float>> _foodNotificationList = new List<Tuple<bool, float>>();

	private bool _notificationCheatEnabled;

	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, (Action)OnHourlyTick);
		CampaignEvents.HeroRelationChanged.AddNonSerializedListener((object)this, (Action<Hero, Hero, int, bool, ChangeRelationDetail, Hero, Hero>)OnRelationChanged);
		CampaignEvents.HeroLevelledUp.AddNonSerializedListener((object)this, (Action<Hero, bool>)OnHeroLevelledUp);
		CampaignEvents.HeroGainedSkill.AddNonSerializedListener((object)this, (Action<Hero, SkillObject, int, bool>)OnHeroGainedSkill);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedFaction);
		CampaignEvents.ArmyCreated.AddNonSerializedListener((object)this, (Action<Army>)OnArmyCreated);
		CampaignEvents.OnPlayerArmyLeaderChangedBehaviorEvent.AddNonSerializedListener((object)this, (Action)OnPlayerArmyLeaderChangedBehaviorEvent);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.RenownGained.AddNonSerializedListener((object)this, (Action<Hero, int, bool>)OnRenownGained);
		CampaignEvents.BeforeHeroesMarried.AddNonSerializedListener((object)this, (Action<Hero, Hero, bool>)OnHeroesMarried);
		CampaignEvents.PartyRemovedFromArmyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnPartyRemovedFromArmy);
		CampaignEvents.ArmyDispersed.AddNonSerializedListener((object)this, (Action<Army, ArmyDispersionReason, bool>)OnArmyDispersed);
		CampaignEvents.OnPartyJoinedArmyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnPartyJoinedArmy);
		CampaignEvents.OnChildConceivedEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnChildConceived);
		CampaignEvents.OnGivenBirthEvent.AddNonSerializedListener((object)this, (Action<Hero, List<Hero>, int>)OnGivenBirth);
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
		CampaignEvents.HeroOrPartyTradedGold.AddNonSerializedListener((object)this, (Action<(Hero, PartyBase), (Hero, PartyBase), (int, string), bool>)OnHeroOrPartyTradedGold);
		CampaignEvents.OnTroopsDesertedEvent.AddNonSerializedListener((object)this, (Action<MobileParty, TroopRoster>)OnTroopsDeserted);
		CampaignEvents.ClanTierIncrease.AddNonSerializedListener((object)this, (Action<Clan, bool>)OnClanTierIncreased);
		CampaignEvents.OnSiegeBombardmentHitEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, BattleSideEnum, SiegeEngineType, SiegeBombardTargets>)OnSiegeBombardmentHit);
		CampaignEvents.OnSiegeBombardmentWallHitEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, BattleSideEnum, SiegeEngineType, bool>)OnSiegeBombardmentWallHit);
		CampaignEvents.OnSiegeEngineDestroyedEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, BattleSideEnum, SiegeEngineType>)OnSiegeEngineDestroyed);
		CampaignEvents.BattleStarted.AddNonSerializedListener((object)this, (Action<PartyBase, PartyBase, object, bool>)OnBattleStarted);
		CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener((object)this, (Action<SiegeEvent>)OnSiegeEventStarted);
		CampaignEvents.ItemsLooted.AddNonSerializedListener((object)this, (Action<MobileParty, ItemRoster>)OnItemsLooted);
		CampaignEvents.OnHideoutSpottedEvent.AddNonSerializedListener((object)this, (Action<PartyBase, PartyBase>)OnHideoutSpotted);
		CampaignEvents.OnHeroSharedFoodWithAnotherHeroEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, float>)OnHeroSharedFoodWithAnotherHero);
		CampaignEvents.OnQuestStartedEvent.AddNonSerializedListener((object)this, (Action<QuestBase>)OnQuestStarted);
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		CampaignEvents.QuestLogAddedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, bool>)OnQuestLogAdded);
		CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener((object)this, (Action<PartyBase, Hero>)OnPrisonerTaken);
		CampaignEvents.CharacterBecameFugitiveEvent.AddNonSerializedListener((object)this, (Action<Hero, bool>)OnHeroBecameFugitive);
		CampaignEvents.HeroPrisonerReleased.AddNonSerializedListener((object)this, (Action<Hero, PartyBase, IFaction, EndCaptivityDetail, bool>)OnHeroPrisonerReleased);
		CampaignEvents.OnClanDestroyedEvent.AddNonSerializedListener((object)this, (Action<Clan>)OnClanDestroyed);
		CampaignEvents.OnIssueUpdatedEvent.AddNonSerializedListener((object)this, (Action<IssueBase, IssueUpdateDetails, Hero>)OnIssueUpdated);
		CampaignEvents.HeroOrPartyGaveItem.AddNonSerializedListener((object)this, (Action<(Hero, PartyBase), (Hero, PartyBase), ItemRosterElement, bool>)OnHeroOrPartyGaveItem);
		CampaignEvents.RebellionFinished.AddNonSerializedListener((object)this, (Action<Settlement, Clan>)OnRebellionFinished);
		CampaignEvents.TournamentFinished.AddNonSerializedListener((object)this, (Action<CharacterObject, MBReadOnlyList<CharacterObject>, Town, ItemObject>)OnTournamentFinished);
		CampaignEvents.OnBuildingLevelChangedEvent.AddNonSerializedListener((object)this, (Action<Town, Building, int>)OnBuildingLevelChanged);
		CampaignEvents.CompanionRemoved.AddNonSerializedListener((object)this, (Action<Hero, RemoveCompanionDetail>)OnCompanionRemoved);
		CampaignEvents.OnHeroTeleportationRequestedEvent.AddNonSerializedListener((object)this, (Action<Hero, Settlement, MobileParty, TeleportationDetail>)OnHeroTeleportationRequested);
		CampaignEvents.OnPartyAddedToMapEventEvent.AddNonSerializedListener((object)this, (Action<PartyBase>)OnPartyAddedToMapEvent);
		CampaignEvents.OnCallToWarAgreementStartedEvent.AddNonSerializedListener((object)this, (Action<Kingdom, Kingdom, Kingdom>)OnCallToWarAgreementStarted);
		CampaignEvents.OnCallToWarAgreementEndedEvent.AddNonSerializedListener((object)this, (Action<Kingdom, Kingdom, Kingdom>)OnCallToWarAgreementEnded);
		CampaignEvents.OnAllianceStartedEvent.AddNonSerializedListener((object)this, (Action<Kingdom, Kingdom>)OnAllianceStarted);
		CampaignEvents.OnAllianceEndedEvent.AddNonSerializedListener((object)this, (Action<Kingdom, Kingdom>)OnAllianceEnded);
	}

	private void OnAllianceStarted(Kingdom kingdom1, Kingdom kingdom2)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		if (Clan.PlayerClan.IsUnderMercenaryService && (((List<Clan>)(object)kingdom1.Clans).Contains(Clan.PlayerClan) || ((List<Clan>)(object)kingdom2.Clans).Contains(Clan.PlayerClan)))
		{
			TextObject val = new TextObject("{=hymvJALX}The {KINGDOM_1} and the {KINGDOM_2} have formed an alliance.", (Dictionary<string, object>)null);
			val.SetTextVariable("KINGDOM_1", kingdom1.Name);
			val.SetTextVariable("KINGDOM_2", kingdom2.Name);
			MBInformationManager.AddQuickInformation(val, 5000, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnAllianceEnded(Kingdom kingdom1, Kingdom kingdom2)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		if (((List<Clan>)(object)kingdom1.Clans).Contains(Clan.PlayerClan) || ((List<Clan>)(object)kingdom2.Clans).Contains(Clan.PlayerClan))
		{
			TextObject val = new TextObject("{=KC8CqquX}The alliance between the {KINGDOM_1} and the {KINGDOM_2} is dissolved.", (Dictionary<string, object>)null);
			val.SetTextVariable("KINGDOM_1", kingdom1.Name);
			val.SetTextVariable("KINGDOM_2", kingdom2.Name);
			MBInformationManager.AddQuickInformation(val, 5000, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnCallToWarAgreementStarted(Kingdom callingKingdom, Kingdom calledKingdom, Kingdom kingdomToCallToWarAgainst)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		if (Clan.PlayerClan.IsUnderMercenaryService && ((List<Clan>)(object)callingKingdom.Clans).Contains(Clan.PlayerClan))
		{
			TextObject val = new TextObject("{=zVmDmLCW}Your realm called the {CALLED_KINGDOM} to war against the {KINGDOM_TO_CALL_TO_WAR_AGAINST}.", (Dictionary<string, object>)null);
			val.SetTextVariable("CALLED_KINGDOM", calledKingdom.Name);
			val.SetTextVariable("KINGDOM_TO_CALL_TO_WAR_AGAINST", kingdomToCallToWarAgainst.Name);
			MBInformationManager.AddQuickInformation(val, 5000, (BasicCharacterObject)null, (Equipment)null, "");
		}
		if (Clan.PlayerClan.IsUnderMercenaryService && ((List<Clan>)(object)calledKingdom.Clans).Contains(Clan.PlayerClan))
		{
			TextObject val2 = new TextObject("{=2ihQeId5}The {CALLING_KINGDOM} has called your realm to war against the {KINGDOM_TO_CALL_TO_WAR_AGAINST}.", (Dictionary<string, object>)null);
			val2.SetTextVariable("CALLING_KINGDOM", callingKingdom.Name);
			val2.SetTextVariable("KINGDOM_TO_CALL_TO_WAR_AGAINST", kingdomToCallToWarAgainst.Name);
			MBInformationManager.AddQuickInformation(val2, 5000, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnCallToWarAgreementEnded(Kingdom callingKingdom, Kingdom calledKingdom, Kingdom kingdomToCallToWarAgainst)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		if (((List<Clan>)(object)callingKingdom.Clans).Contains(Clan.PlayerClan))
		{
			TextObject val = new TextObject("{=ocNWAQsu}The call that your realm issued to the {CALLED_KINGDOM}, to go to war against the {KINGDOM_TO_CALL_TO_WAR_AGAINST}, has ended.", (Dictionary<string, object>)null);
			val.SetTextVariable("CALLED_KINGDOM", calledKingdom.Name);
			val.SetTextVariable("KINGDOM_TO_CALL_TO_WAR_AGAINST", kingdomToCallToWarAgainst.Name);
			MBInformationManager.AddQuickInformation(val, 5000, (BasicCharacterObject)null, (Equipment)null, "");
		}
		if (((List<Clan>)(object)calledKingdom.Clans).Contains(Clan.PlayerClan))
		{
			TextObject val2 = new TextObject("{=6eHPy57Z}The call that the {CALLING_KINGDOM} issued to your realm, to go to war against the {KINGDOM_TO_CALL_TO_WAR_AGAINST}, has ended.", (Dictionary<string, object>)null);
			val2.SetTextVariable("CALLING_KINGDOM", callingKingdom.Name);
			val2.SetTextVariable("KINGDOM_TO_CALL_TO_WAR_AGAINST", kingdomToCallToWarAgainst.Name);
			MBInformationManager.AddQuickInformation(val2, 5000, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		if (mobileParty != null && mobileParty.IsTargetingPort && settlement.SiegeEvent != null && !settlement.SiegeEvent.IsBlockadeActive && settlement.SiegeEvent.BesiegerCamp.IsBesiegerSideParty(MobileParty.MainParty))
		{
			TextObject val = new TextObject("{=dPE4A7fO}{ENEMY_PARTY_NAME} has entered the town with {MAN_COUNT} men from the port.", (Dictionary<string, object>)null);
			val.SetTextVariable("ENEMY_PARTY_NAME", mobileParty.Name);
			val.SetTextVariable("MAN_COUNT", mobileParty.MemberRoster.TotalManCount);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnPartyAddedToMapEvent(PartyBase involvedParty)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (involvedParty.LeaderHero == null || involvedParty.LeaderHero.Clan != Clan.PlayerClan || involvedParty == PartyBase.MainParty || (int)involvedParty.Side != 0 || involvedParty.MapEvent.AttackerSide.LeaderParty == null)
		{
			return;
		}
		bool flag = false;
		foreach (PartyBase involvedParty2 in involvedParty.MapEvent.InvolvedParties)
		{
			if (involvedParty2 == PartyBase.MainParty)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		Settlement val = Hero.MainHero.HomeSettlement;
		float num = Campaign.MapDiagonalSquared;
		float num3 = default(float);
		foreach (Settlement item in (List<Settlement>)(object)Settlement.All)
		{
			if (item.IsVillage || item.IsFortification)
			{
				float num2 = (involvedParty.IsMobile ? Campaign.Current.Models.MapDistanceModel.GetDistance(involvedParty.MobileParty, item, false, (NavigationType)3, ref num3) : Campaign.Current.Models.MapDistanceModel.GetDistance(involvedParty.Settlement, item, false, false, (NavigationType)3));
				if (num2 < num)
				{
					num = num2;
					val = item;
				}
			}
		}
		if (val != null)
		{
			TextObject obj = GameTexts.FindText("str_party_attacked", (string)null);
			obj.SetTextVariable("CLAN_PARTY_NAME", involvedParty.Name);
			obj.SetTextVariable("ENEMY_PARTY_NAME", involvedParty.MapEvent.AttackerSide.LeaderParty.Name);
			obj.SetTextVariable("SETTLEMENT_NAME", val.Name);
			MBInformationManager.AddQuickInformation(obj, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnCompanionRemoved(Hero hero, RemoveCompanionDetail detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0076: Expected O, but got Unknown
		if ((int)detail == 3)
		{
			TextObject val = new TextObject("{=2Lj0WkSF}{COMPANION.NAME} is now a {?COMPANION.GENDER}noblewoman{?}lord{\\?} of the {KINGDOM}.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "COMPANION", hero.CharacterObject, false);
			val.SetTextVariable("KINGDOM", Clan.PlayerClan.Kingdom.Name);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/relation");
		}
		if ((int)detail == 0)
		{
			TextObject val2 = new TextObject("{=4zdyeTGn}{COMPANION.NAME} left your clan.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val2, "COMPANION", hero.CharacterObject, false);
			MBInformationManager.AddQuickInformation(val2, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/relation");
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<List<Tuple<bool, float>>>("_foodNotificationList", ref _foodNotificationList);
	}

	private void OnHourlyTick()
	{
		if (MobileParty.MainParty.Army != null)
		{
			CheckFoodNotifications();
		}
	}

	private unsafe void OnIssueUpdated(IssueBase issue, IssueUpdateDetails details, Hero issueSolver)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		if ((int)details == 2)
		{
			TextObject obj = GameTexts.FindText("str_issue_updated", ((object)(*(IssueUpdateDetails*)(&details))/*cast due to .constrained prefix*/).ToString());
			obj.SetTextVariable("ISSUE_NAME", issue.Title);
			MBInformationManager.AddQuickInformation(obj, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_start");
		}
		else if ((int)details == 3)
		{
			TextObject obj2 = GameTexts.FindText("str_issue_updated", ((object)(*(IssueUpdateDetails*)(&details))/*cast due to .constrained prefix*/).ToString());
			obj2.SetTextVariable("ISSUE_NAME", issue.Title);
			MBInformationManager.AddQuickInformation(obj2, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_finished");
		}
		else if ((int)details == 4)
		{
			TextObject obj3 = GameTexts.FindText("str_issue_updated", ((object)(*(IssueUpdateDetails*)(&details))/*cast due to .constrained prefix*/).ToString());
			obj3.SetTextVariable("ISSUE_NAME", issue.Title);
			MBInformationManager.AddQuickInformation(obj3, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_fail");
		}
	}

	private void OnQuestLogAdded(QuestBase quest, bool hideInformation)
	{
		if (!hideInformation)
		{
			TextObject obj = GameTexts.FindText("str_quest_log_added", (string)null);
			obj.SetTextVariable("TITLE", quest.Title);
			MBInformationManager.AddQuickInformation(obj, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_update");
		}
	}

	private unsafe void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)detail != 1)
		{
			if (detail - 2 <= 3)
			{
				TextObject obj = GameTexts.FindText("str_quest_completed", ((object)(*(QuestCompleteDetails*)(&detail))/*cast due to .constrained prefix*/).ToString());
				obj.SetTextVariable("QUEST_TITLE", quest.Title);
				MBInformationManager.AddQuickInformation(obj, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_fail");
			}
		}
		else
		{
			TextObject obj2 = GameTexts.FindText("str_quest_completed", ((object)(*(QuestCompleteDetails*)(&detail))/*cast due to .constrained prefix*/).ToString());
			obj2.SetTextVariable("QUEST_TITLE", quest.Title);
			MBInformationManager.AddQuickInformation(obj2, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_finished");
		}
	}

	private void OnQuestStarted(QuestBase quest)
	{
		TextObject obj = GameTexts.FindText("str_quest_started", (string)null);
		obj.SetTextVariable("QUEST_TITLE", quest.Title);
		MBInformationManager.AddQuickInformation(obj, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_start");
	}

	private void OnRenownGained(Hero hero, int gainedRenown, bool doNotNotifyPlayer)
	{
		if (hero.Clan == Clan.PlayerClan && !doNotNotifyPlayer)
		{
			TextObject val;
			if (hero.PartyBelongedTo != null)
			{
				val = GameTexts.FindText("str_party_gained_renown", (string)null);
				val.SetTextVariable("PARTY", hero.PartyBelongedTo.Name);
			}
			else
			{
				val = GameTexts.FindText("str_clan_gained_renown", (string)null);
			}
			val.SetTextVariable("NEW_RENOWN", $"{hero.Clan.Renown:0.#}");
			val.SetTextVariable("AMOUNT_TO_ADD", gainedRenown);
			val.SetTextVariable("CLAN", hero.Clan.Name);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnHideoutSpotted(PartyBase party, PartyBase hideoutParty)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (party == PartyBase.MainParty)
		{
			InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_hideout_spotted", (string)null)).ToString(), new Color(1f, 0f, 0f, 1f)));
		}
	}

	private void OnHeroBecameFugitive(Hero hero, bool showNotification)
	{
		if (showNotification && hero.Clan == Clan.PlayerClan)
		{
			TextObject obj = GameTexts.FindText("str_fugitive_news", (string)null);
			TextObjectExtensions.SetCharacterProperties(obj, "HERO", hero.CharacterObject, false);
			MBInformationManager.AddQuickInformation(obj, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnPrisonerTaken(PartyBase capturer, Hero prisoner)
	{
		if (prisoner.Clan == Clan.PlayerClan)
		{
			TextObject val = GameTexts.FindText("str_on_prisoner_taken", (string)null);
			if (capturer.IsSettlement && capturer.Settlement.IsTown)
			{
				TextObject val2 = GameTexts.FindText("str_garrison_party_name", (string)null);
				val2.SetTextVariable("MAJOR_PARTY_LEADER", capturer.Settlement.Name);
				val.SetTextVariable("CAPTOR_NAME", val2);
			}
			else
			{
				val.SetTextVariable("CAPTOR_NAME", capturer.Name);
			}
			StringHelpers.SetCharacterProperties("PRISONER", prisoner.CharacterObject, val, false);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private unsafe void OnHeroPrisonerReleased(Hero hero, PartyBase party, IFaction capturerFaction, EndCaptivityDetail detail, bool showNotification)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Invalid comparison between Unknown and I4
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		TextObject val = null;
		if (!showNotification)
		{
			return;
		}
		if (hero.Clan == Clan.PlayerClan)
		{
			val = (((int)detail > 3) ? GameTexts.FindText("str_on_prisoner_released_main_clan_default", (string)null) : GameTexts.FindText("str_on_prisoner_released_main_clan", ((object)(*(EndCaptivityDetail*)(&detail))/*cast due to .constrained prefix*/).ToString()));
		}
		else if (party != null && party.IsSettlement && party.Settlement.IsFortification && party.Settlement.OwnerClan == Clan.PlayerClan)
		{
			if ((int)detail == 3)
			{
				val = GameTexts.FindText("str_on_prisoner_released_escaped_from_settlement", (string)null);
				val.SetTextVariable("SETTLEMENT", party.Settlement.Name);
			}
		}
		else if (party != null && party.IsMobile && party.MobileParty == MobileParty.MainParty && (int)detail == 3)
		{
			val = GameTexts.FindText("str_on_prisoner_released_escaped_from_party", (string)null);
		}
		if (val != (TextObject)null)
		{
			StringHelpers.SetCharacterProperties("PRISONER", hero.CharacterObject, val, false);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnBattleStarted(PartyBase attackerParty, PartyBase defenderParty, object subject, bool showNotification)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		Settlement settlement;
		if (showNotification && (settlement = (Settlement)((subject is Settlement) ? subject : null)) != null && settlement.OwnerClan == Clan.PlayerClan && defenderParty.MapEvent != null && LinQuick.FindIndexQ<MapEventParty>((List<MapEventParty>)(object)defenderParty.MapEvent.DefenderSide.Parties, (Func<MapEventParty, bool>)((MapEventParty p) => p.Party == settlement.Party)) >= 0)
		{
			MBTextManager.SetTextVariable("PARTY", (attackerParty.MobileParty.Army != null) ? attackerParty.MobileParty.ArmyName : attackerParty.Name, false);
			MBTextManager.SetTextVariable("FACTION", attackerParty.MapFaction.Name, false);
			MBTextManager.SetTextVariable("SETTLEMENT", settlement.EncyclopediaLinkWithName, false);
			MBInformationManager.AddQuickInformation(new TextObject("{=ASOW1MuQ}Your settlement {SETTLEMENT} is under attack by {PARTY} of {FACTION}!", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnSiegeEventStarted(SiegeEvent siegeEvent)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		if (siegeEvent.BesiegedSettlement != null && siegeEvent.BesiegedSettlement.OwnerClan == Clan.PlayerClan && siegeEvent.BesiegerCamp.LeaderParty != null)
		{
			MBTextManager.SetTextVariable("PARTY", (siegeEvent.BesiegerCamp.LeaderParty.Army != null) ? siegeEvent.BesiegerCamp.LeaderParty.ArmyName : siegeEvent.BesiegerCamp.LeaderParty.Name, false);
			MBTextManager.SetTextVariable("FACTION", siegeEvent.BesiegerCamp.MapFaction.Name, false);
			MBTextManager.SetTextVariable("SETTLEMENT", siegeEvent.BesiegedSettlement.EncyclopediaLinkWithName, false);
			MBInformationManager.AddQuickInformation(new TextObject("{=3FvGk8k6}Your settlement {SETTLEMENT} is besieged by {PARTY} of {FACTION}!", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnClanTierIncreased(Clan clan, bool shouldNotify = true)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		if (shouldNotify && clan == Clan.PlayerClan)
		{
			MBTextManager.SetTextVariable("CLAN", clan.Name, false);
			MBTextManager.SetTextVariable("TIER_LEVEL", clan.Tier);
			MBInformationManager.AddQuickInformation(new TextObject("{=No04urXt}{CLAN} tier is increased to {TIER_LEVEL}", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnItemsLooted(MobileParty mobileParty, ItemRoster items)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		if (mobileParty != MobileParty.MainParty)
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < items.Count; i++)
		{
			ItemRosterElement elementCopyAtIndex = items.GetElementCopyAtIndex(i);
			int elementNumber = items.GetElementNumber(i);
			MBTextManager.SetTextVariable("NUMBER_OF", elementNumber);
			EquipmentElement equipmentElement = ((ItemRosterElement)(ref elementCopyAtIndex)).EquipmentElement;
			MBTextManager.SetTextVariable("ITEM", ((EquipmentElement)(ref equipmentElement)).Item.Name, false);
			if (flag)
			{
				MBTextManager.SetTextVariable("LEFT", ((object)GameTexts.FindText("str_number_of_item", (string)null)).ToString(), false);
				flag = false;
			}
			else
			{
				MBTextManager.SetTextVariable("RIGHT", ((object)GameTexts.FindText("str_number_of_item", (string)null)).ToString(), false);
				MBTextManager.SetTextVariable("LEFT", ((object)GameTexts.FindText("str_LEFT_comma_RIGHT", (string)null)).ToString(), false);
			}
		}
		MBTextManager.SetTextVariable("PRODUCTS", ((object)GameTexts.FindText("str_LEFT_ONLY", (string)null)).ToString(), false);
		InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=GW8ITTMb}You plundered {PRODUCTS}.", (Dictionary<string, object>)null)).ToString()));
	}

	private void OnRelationChanged(Hero effectiveHero, Hero effectiveHeroGainedRelationWith, int relationChange, bool showNotification, ChangeRelationDetail detail, Hero originalHero, Hero originalGainedRelationWith)
	{
		if (showNotification && relationChange != 0 && (effectiveHero == Hero.MainHero || effectiveHeroGainedRelationWith == Hero.MainHero))
		{
			Hero val = (effectiveHero.IsHumanPlayerCharacter ? effectiveHeroGainedRelationWith : effectiveHero);
			TextObject val2;
			if (val.Clan == null || val.Clan == Clan.PlayerClan)
			{
				val2 = ((relationChange > 0) ? GameTexts.FindText("str_your_relation_increased_with_notable", (string)null) : GameTexts.FindText("str_your_relation_decreased_with_notable", (string)null));
				StringHelpers.SetCharacterProperties("HERO", val.CharacterObject, val2, false);
			}
			else
			{
				val2 = ((relationChange > 0) ? GameTexts.FindText("str_your_relation_increased_with_clan", (string)null) : GameTexts.FindText("str_your_relation_decreased_with_clan", (string)null));
				val2.SetTextVariable("CLAN_LEADER", val.Clan.Name);
			}
			val2.SetTextVariable("VALUE", val.GetRelation(Hero.MainHero));
			val2.SetTextVariable("MAGNITUDE", MathF.Abs(relationChange));
			MBInformationManager.AddQuickInformation(val2, 0, (BasicCharacterObject)(object)(val.IsNotable ? val.CharacterObject : null), (Equipment)null, "event:/ui/notification/relation");
		}
	}

	private void OnHeroLevelledUp(Hero hero, bool shouldNotify)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (shouldNotify && (hero == Hero.MainHero || hero.Clan == Clan.PlayerClan))
		{
			TextObject val = new TextObject("{=3wzCrzEq}{HERO.NAME} gained a level.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("HERO", hero.CharacterObject, val, false);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/levelup");
		}
	}

	private void OnHeroGainedSkill(Hero hero, SkillObject skill, int change = 1, bool shouldNotify = true)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		if (shouldNotify && BannerlordConfig.ReportExperience && (hero == Hero.MainHero || hero.Clan == Clan.PlayerClan || hero.PartyBelongedTo == MobileParty.MainParty || (hero.CompanionOf != null && hero.CompanionOf == Clan.PlayerClan)))
		{
			TextObject val = GameTexts.FindText("str_skill_gained_notification", (string)null);
			StringHelpers.SetCharacterProperties("HERO", hero.CharacterObject, val, false);
			val.SetTextVariable("PLURAL", (change > 1) ? 1 : 0);
			val.SetTextVariable("GAINED_POINTS", change);
			val.SetTextVariable("SKILL_NAME", ((PropertyObject)skill).Name);
			val.SetTextVariable("UPDATED_SKILL_LEVEL", hero.GetSkillValue(skill));
			InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
		}
	}

	private void OnTroopsDeserted(MobileParty mobileParty, TroopRoster desertedTroops)
	{
		if (mobileParty == MobileParty.MainParty || mobileParty.Party.Owner == Hero.MainHero)
		{
			TextObject obj = GameTexts.FindText("str_troops_deserting", (string)null);
			obj.SetTextVariable("PARTY", mobileParty.Name);
			obj.SetTextVariable("DESERTER_COUNT", desertedTroops.TotalManCount);
			obj.SetTextVariable("PLURAL", (desertedTroops.TotalManCount != 1) ? 1 : 0);
			MBInformationManager.AddQuickInformation(obj, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnClanChangedFaction(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		if (clan == Clan.PlayerClan || (object)Hero.MainHero.MapFaction == oldKingdom || (object)Hero.MainHero.MapFaction == newKingdom)
		{
			if ((int)detail == 0 || (int)detail == 5)
			{
				OnMercenaryClanChangedKingdom(clan, oldKingdom, newKingdom);
			}
			else if (showNotification)
			{
				OnRegularClanChangedKingdom(clan, oldKingdom, newKingdom);
			}
		}
	}

	private void OnRegularClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		TextObject val = ((oldKingdom != null && newKingdom == null) ? new TextObject("{=WNKkdpN3}The {CLAN_NAME} left the {OLD_FACTION_NAME}.", (Dictionary<string, object>)null) : ((oldKingdom == null && newKingdom != null) ? new TextObject("{=qeiVFn9s}The {CLAN_NAME} joined the {NEW_FACTION_NAME}", (Dictionary<string, object>)null) : ((oldKingdom != null && newKingdom != null && oldKingdom != newKingdom) ? new TextObject("{=HlrGpPkV}The {CLAN_NAME} changed from the {OLD_FACTION_NAME} to the {NEW_FACTION_NAME}.", (Dictionary<string, object>)null) : ((oldKingdom == null || oldKingdom != newKingdom || clan.IsUnderMercenaryService) ? ((TextObject)null) : new TextObject("{=6f9Hs5zp}The {CLAN_NAME} ended its mercenary contract and became a vassal of the {NEW_FACTION_NAME}", (Dictionary<string, object>)null)))));
		if (!TextObject.IsNullOrEmpty(val))
		{
			val.SetTextVariable("CLAN_NAME", (((List<Hero>)(object)clan.AliveLords).Count == 1) ? ((List<Hero>)(object)clan.AliveLords)[0].Name : clan.Name);
			if (oldKingdom != null)
			{
				val.SetTextVariable("OLD_FACTION_NAME", oldKingdom.InformalName);
			}
			if (newKingdom != null)
			{
				val.SetTextVariable("NEW_FACTION_NAME", newKingdom.InformalName);
			}
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnMercenaryClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		if (clan == Clan.PlayerClan || (object)Hero.MainHero.MapFaction == oldKingdom || (object)Hero.MainHero.MapFaction == newKingdom)
		{
			if (oldKingdom != null && (clan == Hero.MainHero.Clan || (object)oldKingdom == Hero.MainHero.MapFaction))
			{
				TextObject val = (clan.IsUnderMercenaryService ? new TextObject("{=a2AO5T1Q}The {CLAN_NAME} and the {KINGDOM_NAME} have ended their mercenary contract.", (Dictionary<string, object>)null) : new TextObject("{=g7qhnsnJ}The {CLAN_NAME} clan has left the {KINGDOM_NAME}.", (Dictionary<string, object>)null));
				val.SetTextVariable("CLAN_NAME", clan.Name);
				val.SetTextVariable("KINGDOM_NAME", oldKingdom.InformalName);
				MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
			}
			if (newKingdom != null && (clan == Hero.MainHero.Clan || (object)newKingdom == Hero.MainHero.MapFaction) && clan.IsUnderMercenaryService)
			{
				TextObject val2 = new TextObject("{=AozaGCru}The {CLAN_NAME} and the {KINGDOM_NAME} have signed a mercenary contract.", (Dictionary<string, object>)null);
				val2.SetTextVariable("CLAN_NAME", clan.Name);
				val2.SetTextVariable("KINGDOM_NAME", newKingdom.InformalName);
				MBInformationManager.AddQuickInformation(val2, 0, (BasicCharacterObject)null, (Equipment)null, "");
			}
		}
	}

	private void OnArmyCreated(Army army)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		if (((object)army.Kingdom == MobileParty.MainParty.MapFaction && MobileParty.MainParty.Army == null) || _notificationCheatEnabled)
		{
			TextObject val = new TextObject("{=VEHPTzhO}{LEADER.NAME} is gathering an army near {SETTLEMENT}.", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT", army.AiBehaviorObject.Name);
			StringHelpers.SetCharacterProperties("LEADER", army.LeaderParty.LeaderHero.CharacterObject, val, false);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)(object)army.LeaderParty.LeaderHero.CharacterObject, (Equipment)null, "");
		}
	}

	private void OnPlayerArmyLeaderChangedBehaviorEvent()
	{
		MBInformationManager.AddQuickInformation(GameTexts.FindText("str_army_leader_think", "Unknown"), 0, (BasicCharacterObject)(object)MobileParty.MainParty.Army.LeaderParty.LeaderHero.CharacterObject, (Equipment)null, "");
	}

	private void OnSiegeBombardmentHit(MobileParty besiegerParty, Settlement besiegedSettlement, BattleSideEnum side, SiegeEngineType weapon, SiegeBombardTargets target)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		if ((besiegerParty.Army == null || !((List<MobileParty>)(object)besiegerParty.Army.Parties).Contains(MobileParty.MainParty)) && besiegerParty != MobileParty.MainParty && (MobileParty.MainParty.CurrentSettlement == null || MobileParty.MainParty.CurrentSettlement != besiegedSettlement))
		{
			return;
		}
		TextObject val;
		if ((int)target != 2)
		{
			if ((int)target == 3)
			{
				val = (((int)side == 0) ? new TextObject("{=7WlQ0Twr}{WEAPON} of {SETTLEMENT} hit some soldiers of {BESIEGER}!", (Dictionary<string, object>)null) : new TextObject("{=ZrMeSyPu}The {WEAPON} of {BESIEGER} hit some soldiers in {SETTLEMENT}!", (Dictionary<string, object>)null));
			}
			else
			{
				Debug.FailedAssert("invalid bombardment type", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\CampaignBehaviors\\DefaultNotificationsCampaignBehavior.cs", "OnSiegeBombardmentHit", 697);
				val = null;
			}
		}
		else
		{
			val = (((int)side == 0) ? new TextObject("{=gqdsXVNi}{WEAPON} of {SETTLEMENT} hit ranged engines of {BESIEGER}!", (Dictionary<string, object>)null) : new TextObject("{=FnkYfyGa}the {WEAPON} of {BESIEGER} hit ranged engines of {SETTLEMENT}!", (Dictionary<string, object>)null));
		}
		if (!TextObject.IsNullOrEmpty(val))
		{
			val.SetTextVariable("WEAPON", weapon.Name);
			val.SetTextVariable("BESIEGER", (besiegerParty.Army != null) ? besiegerParty.Army.Name : besiegerParty.Name);
			val.SetTextVariable("SETTLEMENT", besiegedSettlement.Name);
			InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
		}
	}

	private void OnSiegeBombardmentWallHit(MobileParty besiegerParty, Settlement besiegedSettlement, BattleSideEnum side, SiegeEngineType weapon, bool isWallCracked)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		if ((besiegerParty.Army != null && ((List<MobileParty>)(object)besiegerParty.Army.Parties).Contains(MobileParty.MainParty)) || besiegerParty == MobileParty.MainParty || (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement == besiegedSettlement))
		{
			TextObject val = new TextObject("{=8Wy1OCsr}The {WEAPON} of {BESIEGER} hit wall of {SETTLEMENT}!", (Dictionary<string, object>)null);
			val.SetTextVariable("WEAPON", weapon.Name);
			val.SetTextVariable("BESIEGER", (besiegerParty.Army != null) ? besiegerParty.Army.Name : besiegerParty.Name);
			val.SetTextVariable("SETTLEMENT", besiegedSettlement.Name);
			InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
			if (isWallCracked)
			{
				TextObject val2 = new TextObject("{=uJNvbag5}The walls of {SETTLEMENT} has been cracked.", (Dictionary<string, object>)null);
				val2.SetTextVariable("SETTLEMENT", besiegedSettlement.Name);
				MBInformationManager.AddQuickInformation(val2, 0, (BasicCharacterObject)null, (Equipment)null, "");
			}
		}
	}

	private void OnSiegeEngineDestroyed(MobileParty besiegerParty, Settlement besiegedSettlement, BattleSideEnum side, SiegeEngineType destroyedEngine)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		if ((besiegerParty.Army != null && ((List<MobileParty>)(object)besiegerParty.Army.Parties).Contains(MobileParty.MainParty)) || besiegerParty == MobileParty.MainParty || (MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement == besiegedSettlement))
		{
			TextObject val = (((int)side == 1) ? new TextObject("{=fa8sla4i}The {SIEGE_ENGINE} of {BESIEGER_PARTY} has been destroyed.", (Dictionary<string, object>)null) : new TextObject("{=U9zFz8Et}The {SIEGE_ENGINE} of {SIEGED_SETTLEMENT_NAME} has been cracked.", (Dictionary<string, object>)null));
			val.SetTextVariable("SIEGED_SETTLEMENT_NAME", besiegedSettlement.Name);
			val.SetTextVariable("BESIEGER_PARTY", besiegerParty.Name);
			val.SetTextVariable("SIEGE_ENGINE", destroyedEngine.Name);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnHeroOrPartyTradedGold((Hero, PartyBase) giverSide, (Hero, PartyBase) recipientSide, (int, string) transactionAmountAndId, bool displayNotification)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		if (displayNotification)
		{
			var (num, _) = transactionAmountAndId;
			MBTextManager.SetTextVariable("GOLD_AMOUNT", MathF.Abs(num));
			bool flag = giverSide.Item1 == Hero.MainHero || giverSide.Item2 == PartyBase.MainParty;
			bool flag2 = recipientSide.Item1 == Hero.MainHero || recipientSide.Item2 == PartyBase.MainParty;
			if ((flag && num > 0) || (flag2 && num < 0))
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_gold_removed_with_icon", (string)null)).ToString(), "event:/ui/notification/coins_negative"));
			}
			else if ((flag && num < 0) || (flag2 && num > 0))
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_you_received_gold_with_icon", (string)null)).ToString(), "event:/ui/notification/coins_positive"));
			}
		}
	}

	private void OnPartyJoinedArmy(MobileParty party)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		if (party.Army == MobileParty.MainParty.Army && party.LeaderHero != party.Army.LeaderParty.LeaderHero)
		{
			TextObject val = new TextObject("{=wD1YDmmg}{PARTY_NAME} has enlisted in {ARMY_NAME}.", (Dictionary<string, object>)null);
			val.SetTextVariable("PARTY_NAME", party.Name);
			val.SetTextVariable("ARMY_NAME", party.Army.Name);
			InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
		}
	}

	private void OnPartyAttachedAnotherParty(MobileParty party)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		if (party.Army == MobileParty.MainParty.Army && party.LeaderHero != party.Army.LeaderParty.LeaderHero)
		{
			TextObject val = new TextObject("{=0aGYre5B}{LEADER.LINK} has arrived at {ARMY_NAME}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("LEADER", party.LeaderHero.CharacterObject, val, false);
			val.SetTextVariable("ARMY_NAME", party.Army.Name);
			InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
		}
	}

	private void OnPartyRemovedFromArmy(MobileParty party)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		if (party.Army == MobileParty.MainParty.Army)
		{
			TextObject val = new TextObject("{=ApG1xg7O}{PARTY_NAME} has left {ARMY_NAME}.", (Dictionary<string, object>)null);
			val.SetTextVariable("PARTY_NAME", party.Name);
			val.SetTextVariable("ARMY_NAME", party.Army.Name);
			InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
		}
		if (party == MobileParty.MainParty)
		{
			CheckFoodNotifications();
		}
	}

	private void OnArmyDispersed(Army army, ArmyDispersionReason reason, bool isPlayersArmy)
	{
		if (isPlayersArmy)
		{
			CheckFoodNotifications();
		}
	}

	private void OnHeroesMarried(Hero firstHero, Hero secondHero, bool showNotification)
	{
		if (showNotification && (firstHero.Clan == Clan.PlayerClan || secondHero.Clan == Clan.PlayerClan))
		{
			StringHelpers.SetCharacterProperties("MARRIED_TO", firstHero.CharacterObject, (TextObject)null, false);
			StringHelpers.SetCharacterProperties("MARRIED_HERO", secondHero.CharacterObject, (TextObject)null, false);
			MBInformationManager.AddQuickInformation(GameTexts.FindText("str_hero_married_hero", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero previousOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		if ((int)detail == 1 && settlement.MapFaction == Hero.MainHero.MapFaction && settlement.IsFortification)
		{
			TextObject val = (Hero.MainHero.MapFaction.IsKingdomFaction ? new TextObject("{=OiCCfAeC}{SETTLEMENT} is taken. Election is started.", (Dictionary<string, object>)null) : new TextObject("{=2VRTPyZY}{SETTLEMENT} is yours.", (Dictionary<string, object>)null));
			val.SetTextVariable("SETTLEMENT", settlement.Name);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnChildConceived(Hero mother)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		if (mother == Hero.MainHero)
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=ZhpT2qVh}You have just learned that you are with child.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		else if (mother == Hero.MainHero.Spouse)
		{
			TextObject val = new TextObject("{=7v2dMsW5}Your spouse {MOTHER} has just learned that she is with child.", (Dictionary<string, object>)null);
			val.SetTextVariable("MOTHER", mother.Name);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		else if (mother.Clan == Clan.PlayerClan)
		{
			TextObject val2 = new TextObject("{=2AGIxoUN}Your clan member {MOTHER} has just learned that she is with child.", (Dictionary<string, object>)null);
			val2.SetTextVariable("MOTHER", mother.Name);
			MBInformationManager.AddQuickInformation(val2, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnGivenBirth(Hero mother, List<Hero> aliveOffsprings, int stillbornCount)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		if (mother == Hero.MainHero || mother == Hero.MainHero.Spouse || mother.Clan == Clan.PlayerClan)
		{
			TextObject val = ((mother == Hero.MainHero) ? new TextObject("{=oIA9lkpc}You have given birth to {DELIVERED_CHILDREN}.", (Dictionary<string, object>)null) : ((mother != Hero.MainHero.Spouse) ? new TextObject("{=LsDRCPp0}Your clan member {MOTHER.NAME} has given birth to {DELIVERED_CHILDREN}.", (Dictionary<string, object>)null) : new TextObject("{=TsbjAsxs}Your wife {MOTHER.NAME} has given birth to {DELIVERED_CHILDREN}.", (Dictionary<string, object>)null)));
			if (stillbornCount == 2)
			{
				val.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=Sn9a1Aba}two stillborn babies", (Dictionary<string, object>)null));
			}
			else if (stillbornCount == 1 && aliveOffsprings.Count == 0)
			{
				val.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=qWLq2y84}a stillborn baby", (Dictionary<string, object>)null));
			}
			else if (stillbornCount == 1 && aliveOffsprings.Count == 1)
			{
				val.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=vn13OyFV}one healthy and one stillborn baby", (Dictionary<string, object>)null));
			}
			else if (stillbornCount == 0 && aliveOffsprings.Count == 1)
			{
				val.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=lbRMmZym}a healthy baby", (Dictionary<string, object>)null));
			}
			else if (stillbornCount == 0 && aliveOffsprings.Count == 2)
			{
				val.SetTextVariable("DELIVERED_CHILDREN", new TextObject("{=EPbHr2DX}two healthy babies", (Dictionary<string, object>)null));
			}
			StringHelpers.SetCharacterProperties("MOTHER", mother.CharacterObject, val, false);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnHeroKilled(Hero victimHero, Hero killer, KillCharacterActionDetail detail, bool showNotification)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (showNotification && victimHero != null && victimHero.Clan == Clan.PlayerClan)
		{
			MBInformationManager.AddQuickInformation(CharacterHelper.GetDeathNotification(victimHero, killer, detail), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnHeroSharedFoodWithAnotherHero(Hero supporterHero, Hero supportedHero, float influence)
	{
		if (supporterHero == Hero.MainHero)
		{
			_foodNotificationList.Add(new Tuple<bool, float>(item1: true, influence));
		}
		else if (supportedHero == Hero.MainHero)
		{
			_foodNotificationList.Add(new Tuple<bool, float>(item1: false, influence));
		}
	}

	private void CheckFoodNotifications()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		bool flag2 = false;
		foreach (Tuple<bool, float> foodNotification in _foodNotificationList)
		{
			if (foodNotification.Item1)
			{
				num += foodNotification.Item2;
				flag = true;
			}
			else
			{
				num2 += foodNotification.Item2;
				flag2 = true;
			}
		}
		if (flag)
		{
			TextObject val = new TextObject("{=B0eBWPoO} You shared your food with starving soldiers of your army. You gained {INFLUENCE}{INFLUENCE_ICON}.", (Dictionary<string, object>)null);
			val.SetTextVariable("INFLUENCE", num.ToString("0.00"));
			val.SetTextVariable("INFLUENCE_ICON", "{=!}<img src=\"General\\Icons\\Influence@2x\" extend=\"5\">");
			InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
		}
		if (flag2)
		{
			TextObject val2 = new TextObject("{=qQ71Ux7D} Your army shared their food with your starving soldiers. You spent {INFLUENCE}{INFLUENCE_ICON}.", (Dictionary<string, object>)null);
			val2.SetTextVariable("INFLUENCE", num2.ToString("0.00"));
			val2.SetTextVariable("INFLUENCE_ICON", "{=!}<img src=\"General\\Icons\\Influence@2x\" extend=\"5\">");
			InformationManager.DisplayMessage(new InformationMessage(((object)val2).ToString()));
		}
		_foodNotificationList.Clear();
	}

	private void OnClanDestroyed(Clan destroyedClan)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		TextObject val = new TextObject("{=PBq1FyrJ}{CLAN_NAME} clan was destroyed.", (Dictionary<string, object>)null);
		val.SetTextVariable("CLAN_NAME", destroyedClan.Name);
		MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
	}

	private void OnHeroOrPartyGaveItem((Hero, PartyBase) giver, (Hero, PartyBase) receiver, ItemRosterElement itemRosterElement, bool showNotification)
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Expected O, but got Unknown
		if (!showNotification || ((ItemRosterElement)(ref itemRosterElement)).Amount <= 0)
		{
			return;
		}
		TextObject val = null;
		if (giver.Item1 == Hero.MainHero || giver.Item2 == PartyBase.MainParty)
		{
			if (receiver.Item1 != null)
			{
				val = GameTexts.FindText("str_hero_gave_item_to_hero", (string)null);
				StringHelpers.SetCharacterProperties("HERO", receiver.Item1.CharacterObject, val, false);
			}
			else
			{
				val = GameTexts.FindText("str_hero_gave_item_to_party", (string)null);
				val.SetTextVariable("PARTY_NAME", receiver.Item2.Name);
			}
		}
		else if (receiver.Item1 == Hero.MainHero || receiver.Item2 == PartyBase.MainParty)
		{
			if (giver.Item1 != null)
			{
				val = GameTexts.FindText("str_hero_received_item_from_hero", (string)null);
				StringHelpers.SetCharacterProperties("HERO", giver.Item1.CharacterObject, val, false);
			}
			else
			{
				val = GameTexts.FindText("str_hero_received_item_from_party", (string)null);
				val.SetTextVariable("PARTY_NAME", giver.Item2.Name);
			}
		}
		if (val != (TextObject)null)
		{
			TextObject obj = val;
			EquipmentElement equipmentElement = ((ItemRosterElement)(ref itemRosterElement)).EquipmentElement;
			obj.SetTextVariable("ITEM", ((EquipmentElement)(ref equipmentElement)).Item.Name);
			val.SetTextVariable("COUNT", ((ItemRosterElement)(ref itemRosterElement)).Amount);
			InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
		}
	}

	private void OnRebellionFinished(Settlement settlement, Clan oldOwnerClan)
	{
		TextObject obj = GameTexts.FindText("str_rebellion_finished", (string)null);
		obj.SetTextVariable("SETTLEMENT", settlement.Name);
		obj.SetTextVariable("RULER", oldOwnerClan.Leader.Name);
		MBInformationManager.AddQuickInformation(obj, 0, (BasicCharacterObject)null, (Equipment)null, "");
	}

	private void OnTournamentFinished(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town, ItemObject prize)
	{
		if (((BasicCharacterObject)winner).IsHero && winner.HeroObject.Clan == Clan.PlayerClan && winner.HeroObject.PartyBelongedTo == MobileParty.MainParty)
		{
			TextObject obj = GameTexts.FindText("str_tournament_companion_won_prize", (string)null);
			obj.SetTextVariable("ITEM_NAME", prize.Name);
			TextObjectExtensions.SetCharacterProperties(obj, "COMPANION", winner, false);
			MBInformationManager.AddQuickInformation(obj, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnBuildingLevelChanged(Town town, Building building, int levelChange)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		if (levelChange > 0 && town.OwnerClan == Clan.PlayerClan)
		{
			TextObject obj = ((building.CurrentLevel == 1) ? GameTexts.FindText("str_building_completed", (string)null) : GameTexts.FindText("str_building_level_gained", (string)null));
			obj.SetTextVariable("SETTLEMENT_NAME", ((SettlementComponent)town).Name);
			obj.SetTextVariable("BUILDING_NAME", building.Name);
			InformationManager.DisplayMessage(new InformationMessage(((object)obj).ToString(), new Color(0f, 1f, 0f, 1f)));
		}
	}

	private void OnHeroTeleportationRequested(Hero hero, Settlement targetSettlement, MobileParty targetParty, TeleportationDetail detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0043: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0087: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Invalid comparison between Unknown and I4
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		if ((int)detail == 1 && targetParty == MobileParty.MainParty && MobileParty.MainParty.IsActive)
		{
			TextObject val = new TextObject("{=abux36nq}{HERO.NAME} joined your party.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", hero.CharacterObject, false);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		if ((int)detail == 2 && targetParty.ActualClan == Clan.PlayerClan && targetParty != MobileParty.MainParty)
		{
			TextObject val2 = new TextObject("{=xxSlIDCW}{HERO.NAME} has joined {?HERO.GENDER}her{?}his{\\?} party and assumed command.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val2, "HERO", hero.CharacterObject, false);
			MBInformationManager.AddQuickInformation(val2, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		if ((int)detail == 0 && hero.Clan == Clan.PlayerClan && targetSettlement.IsFortification && targetSettlement.Town.Governor == hero && (int)hero.HeroState == 7)
		{
			TextObject val3 = new TextObject("{=btynhBAn}The new governor of {SETTLEMENT}, {HERO.NAME}, has arrived and taken up the reins of office.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val3, "HERO", hero.CharacterObject, false);
			val3.SetTextVariable("SETTLEMENT", targetSettlement.Name);
			MBInformationManager.AddQuickInformation(val3, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}
}
