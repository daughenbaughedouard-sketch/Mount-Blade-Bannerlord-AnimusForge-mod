using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace SandBox.CampaignBehaviors;

public class DefaultCutscenesCampaignBehavior : CampaignBehaviorBase
{
	private bool _heroWonLastMapEVent;

	private CultureObject _lastEnemyCulture;

	public override void RegisterEvents()
	{
		CampaignEvents.BeforeHeroesMarried.AddNonSerializedListener((object)this, (Action<Hero, Hero, bool>)OnHeroesMarried);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnd);
		CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnHeroComesOfAge);
		CampaignEvents.KingdomCreatedEvent.AddNonSerializedListener((object)this, (Action<Kingdom>)OnKingdomCreated);
		CampaignEvents.KingdomDestroyedEvent.AddNonSerializedListener((object)this, (Action<Kingdom>)OnKingdomDestroyed);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
		CampaignEvents.KingdomDecisionConcluded.AddNonSerializedListener((object)this, (Action<KingdomDecision, DecisionOutcome, bool>)OnKingdomDecisionConcluded);
		CampaignEvents.OnBeforeMainCharacterDiedEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnBeforeMainCharacterDied);
		CampaignEvents.OnMercenaryServiceEndedEvent.AddNonSerializedListener((object)this, (Action<Clan, EndMercenaryServiceActionDetails>)OnMercenaryServiceEnded);
	}

	private void OnBeforeMainCharacterDied(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Invalid comparison between Unknown and I4
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00b9: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		SceneNotificationData val = null;
		if (victim == Hero.MainHero)
		{
			MobileParty partyBelongedTo = victim.PartyBelongedTo;
			if (partyBelongedTo != null && partyBelongedTo.IsCurrentlyAtSea)
			{
				val = (SceneNotificationData)new NavalDeathSceneNotificationItem(victim, CampaignTime.Now, detail);
			}
			else if ((int)detail == 3)
			{
				val = (SceneNotificationData)new DeathOldAgeSceneNotificationItem(victim);
			}
			else if ((int)detail == 4)
			{
				if (_heroWonLastMapEVent)
				{
					bool noCompanions = !victim.CompanionsInParty.Any();
					List<CharacterObject> allyCharacters = new List<CharacterObject>();
					FillAllyCharacters(noCompanions, ref allyCharacters);
					val = (SceneNotificationData)new MainHeroBattleVictoryDeathNotificationItem(victim, allyCharacters);
				}
				else
				{
					val = (SceneNotificationData)new MainHeroBattleDeathNotificationItem(victim, _lastEnemyCulture);
				}
			}
			else if ((int)detail == 6 || (int)detail == 7)
			{
				TextObject val2 = new TextObject("{=uYjEknNX}{VICTIM.NAME}'s execution by {EXECUTER.NAME}", (Dictionary<string, object>)null);
				TextObjectExtensions.SetCharacterProperties(val2, "VICTIM", victim.CharacterObject, false);
				TextObjectExtensions.SetCharacterProperties(val2, "EXECUTER", killer.CharacterObject, false);
				val = (SceneNotificationData)(object)HeroExecutionSceneNotificationData.CreateForInformingPlayer(killer, victim, (RelevantContextType)4, (Action)null);
			}
		}
		if (val != null)
		{
			MBInformationManager.ShowSceneNotification(val);
		}
	}

	private void OnKingdomDecisionConcluded(KingdomDecision decision, DecisionOutcome chosenOutcome, bool isPlayerInvolved)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		KingSelectionDecisionOutcome val;
		if ((val = (KingSelectionDecisionOutcome)(object)((chosenOutcome is KingSelectionDecisionOutcome) ? chosenOutcome : null)) != null && isPlayerInvolved && val.King == Hero.MainHero)
		{
			MBInformationManager.ShowSceneNotification((SceneNotificationData)new BecomeKingSceneNotificationItem(val.King));
		}
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		if (showNotification)
		{
			SceneNotificationData val = null;
			if (clan == Clan.PlayerClan && (int)detail == 1)
			{
				val = (SceneNotificationData)new JoinKingdomSceneNotificationItem(clan, newKingdom);
			}
			else if (Clan.PlayerClan.Kingdom == newKingdom && (int)detail == 2)
			{
				val = (SceneNotificationData)new JoinKingdomSceneNotificationItem(clan, newKingdom);
			}
			if (val != null)
			{
				MBInformationManager.ShowSceneNotification(val);
			}
		}
	}

	private void OnMercenaryServiceEnded(Clan clan, EndMercenaryServiceActionDetails detail)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		if (clan.Kingdom != null && clan.Kingdom == Clan.PlayerClan.Kingdom && (int)detail == 2)
		{
			MBInformationManager.ShowSceneNotification((SceneNotificationData)new JoinKingdomSceneNotificationItem(clan, clan.Kingdom));
		}
	}

	private void OnKingdomDestroyed(Kingdom kingdom)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		if (!kingdom.IsRebelClan)
		{
			if (kingdom.Leader == Hero.MainHero)
			{
				MBInformationManager.ShowSceneNotification(Campaign.Current.Models.CutsceneSelectionModel.GetKingdomDestroyedSceneNotification(kingdom));
			}
			else
			{
				Campaign.Current.CampaignInformationManager.NewMapNoticeAdded((InformationData)new KingdomDestroyedMapNotification(kingdom, CampaignTime.Now));
			}
		}
	}

	private void OnKingdomCreated(Kingdom kingdom)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (Hero.MainHero.Clan.Kingdom == kingdom)
		{
			MBInformationManager.ShowSceneNotification((SceneNotificationData)new KingdomCreatedSceneNotificationItem(kingdom));
		}
	}

	private void OnHeroComesOfAge(Hero hero)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		Hero mother = hero.Mother;
		if (((mother != null) ? mother.Clan : null) != Clan.PlayerClan)
		{
			Hero father = hero.Father;
			if (((father != null) ? father.Clan : null) != Clan.PlayerClan)
			{
				return;
			}
		}
		Hero mentorHeroForComeOfAge = GetMentorHeroForComeOfAge(hero);
		TextObject val = new TextObject("{=t4KwQOB7}{HERO.NAME} is now of age.", (Dictionary<string, object>)null);
		TextObjectExtensions.SetCharacterProperties(val, "HERO", hero.CharacterObject, false);
		Campaign.Current.CampaignInformationManager.NewMapNoticeAdded((InformationData)new HeirComeOfAgeMapNotification(hero, mentorHeroForComeOfAge, val, CampaignTime.Now));
	}

	private void OnMapEventEnd(MapEvent mapEvent)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		if (mapEvent.IsPlayerMapEvent && mapEvent.HasWinner)
		{
			_heroWonLastMapEVent = (int)mapEvent.WinningSide != -1 && mapEvent.WinningSide == mapEvent.PlayerSide;
			_lastEnemyCulture = (((int)mapEvent.PlayerSide == 1) ? mapEvent.DefenderSide.MapFaction.Culture : mapEvent.AttackerSide.MapFaction.Culture);
		}
	}

	private static void OnHeroesMarried(Hero firstHero, Hero secondHero, bool showNotification)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		if (firstHero == Hero.MainHero || secondHero == Hero.MainHero)
		{
			Hero obj = (firstHero.IsFemale ? secondHero : firstHero);
			MBInformationManager.ShowSceneNotification((SceneNotificationData)new MarriageSceneNotificationItem(obj, obj.Spouse, CampaignTime.Now, (RelevantContextType)0));
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private static void FillAllyCharacters(bool noCompanions, ref List<CharacterObject> allyCharacters)
	{
		if (noCompanions)
		{
			allyCharacters.Add(Hero.MainHero.MapFaction.Culture.RangedEliteMilitiaTroop);
			return;
		}
		List<CharacterObject> source = (from t in (IEnumerable<TroopRosterElement>)MobileParty.MainParty.MemberRoster.GetTroopRoster()
			where t.Character != CharacterObject.PlayerCharacter && ((BasicCharacterObject)t.Character).IsHero
			select t.Character).ToList();
		allyCharacters.AddRange(source.Take(3));
		int count = allyCharacters.Count;
		for (int num = 0; num < 3 - count; num++)
		{
			allyCharacters.Add(Extensions.GetRandomElement<Hero>(Hero.AllAliveHeroes).CharacterObject);
		}
	}

	private Hero GetMentorHeroForComeOfAge(Hero hero)
	{
		Hero result = Hero.MainHero;
		if (hero.IsFemale)
		{
			if (hero.Mother != null && hero.Mother.IsAlive)
			{
				result = hero.Mother;
			}
			else if (hero.Father != null && hero.Father.IsAlive)
			{
				result = hero.Father;
			}
		}
		else if (hero.Father != null && hero.Father.IsAlive)
		{
			result = hero.Father;
		}
		else if (hero.Mother != null && hero.Mother.IsAlive)
		{
			result = hero.Mother;
		}
		if (hero.Mother == Hero.MainHero || hero.Father == Hero.MainHero)
		{
			result = Hero.MainHero;
		}
		return result;
	}
}
