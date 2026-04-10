using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StoryMode.Quests.PlayerClanQuests;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class MainStorylineCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.CanHeroDieEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, KillCharacterActionDetail, bool>)CanHeroDie);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
		CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnHeroComesOfAge);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		if (clan == Clan.PlayerClan && newKingdom != null && ((int)detail == 7 || (int)detail == 1))
		{
			Clan.PlayerClan.IsNoble = true;
		}
	}

	private void CanHeroDie(Hero hero, KillCharacterActionDetail causeOfDeath, ref bool result)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		if ((hero == StoryModeHeroes.Radagos && StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(RescueFamilyQuestBehavior.RescueFamilyQuest)) && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(RebuildPlayerClanQuest)) && (int)causeOfDeath == 6) || (int)causeOfDeath == 7)
		{
			result = true;
		}
		else if (hero.IsSpecial && hero != StoryModeHeroes.RadagosHenchman && !StoryModeManager.Current.MainStoryLine.IsCompleted)
		{
			result = false;
		}
	}

	private void OnHeroComesOfAge(Hero hero)
	{
		if (hero == StoryModeHeroes.LittleBrother || (hero == StoryModeHeroes.LittleSister && !ModuleHelper.IsModuleActive("NavalDLC")))
		{
			StoryModeHelpers.SetPlayerSiblingsSkillsIfNeeded(hero);
		}
	}

	private void OnGameLoadFinished()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		if (!MBSaveLoad.IsUpdatingGameVersion)
		{
			return;
		}
		if (MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.3.13.105456", 0))
		{
			if (Clan.PlayerClan.Kingdom != null && !Clan.PlayerClan.IsUnderMercenaryService && !Clan.PlayerClan.IsNoble)
			{
				Clan.PlayerClan.IsNoble = true;
			}
			bool flag = StoryModeManager.Current.MainStoryLine.FamilyRescued && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(RescueFamilyQuestBehavior.RescueFamilyQuest));
			HandlePlayerSiblingsStatesOnLoad(StoryModeHeroes.LittleSister, flag);
			HandlePlayerSiblingsStatesOnLoad(StoryModeHeroes.LittleBrother, flag);
			if (flag)
			{
				CheckStoryModeHeroStateAndUpdateIfNeeded(StoryModeHeroes.ElderBrother);
				CheckAndUpdateGovernorStatusOfStoryModeHero(StoryModeHeroes.ElderBrother);
			}
		}
		EquipmentElement equipmentElement;
		if (MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.2.0", 0))
		{
			FirstPhase instance = FirstPhase.Instance;
			if (instance != null && instance.AllPiecesCollected)
			{
				ItemObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<ItemObject>("dragon_banner");
				bool flag2 = false;
				foreach (ItemRosterElement item2 in MobileParty.MainParty.ItemRoster)
				{
					ItemRosterElement current = item2;
					equipmentElement = ((ItemRosterElement)(ref current)).EquipmentElement;
					if (((EquipmentElement)(ref equipmentElement)).Item == val)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					StoryModeManager.Current.MainStoryLine.FirstPhase?.MergeDragonBanner();
				}
			}
		}
		ApplicationVersion lastLoadedGameVersion = MBSaveLoad.LastLoadedGameVersion;
		if (!((ApplicationVersion)(ref lastLoadedGameVersion)).IsOlderThan(ApplicationVersion.FromString("v1.2.9.35367", 0)))
		{
			return;
		}
		List<EquipmentElement> list = new List<EquipmentElement>();
		foreach (ItemRosterElement item3 in MobileParty.MainParty.ItemRoster)
		{
			ItemRosterElement current2 = item3;
			equipmentElement = ((ItemRosterElement)(ref current2)).EquipmentElement;
			ItemObject item = ((EquipmentElement)(ref equipmentElement)).Item;
			string text = ((item != null) ? ((MBObjectBase)item).StringId : null);
			equipmentElement = ((ItemRosterElement)(ref current2)).EquipmentElement;
			if (!((EquipmentElement)(ref equipmentElement)).IsQuestItem)
			{
				switch (text)
				{
				case "dragon_banner_center":
				case "dragon_banner_dragonhead":
				case "dragon_banner_handle":
				case "dragon_banner":
					list.Add(((ItemRosterElement)(ref current2)).EquipmentElement);
					break;
				}
			}
		}
		if (!list.Any())
		{
			return;
		}
		foreach (EquipmentElement item4 in list)
		{
			EquipmentElement current3 = item4;
			MobileParty.MainParty.ItemRoster.AddToCounts(current3, -1);
			MobileParty.MainParty.ItemRoster.AddToCounts(new EquipmentElement(((EquipmentElement)(ref current3)).Item, (ItemModifier)null, (ItemObject)null, true), 1);
		}
	}

	private void HandlePlayerSiblingsStatesOnLoad(Hero hero, bool isPlayerFamilyRescued)
	{
		if (!hero.IsAlive || (hero != StoryModeHeroes.LittleBrother && (hero != StoryModeHeroes.LittleSister || ModuleHelper.IsModuleActive("NavalDLC"))))
		{
			return;
		}
		AgingCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<AgingCampaignBehavior>();
		FieldInfo field = typeof(AgingCampaignBehavior).GetField("_heroesYoungerThanHeroComesOfAge", BindingFlags.Instance | BindingFlags.NonPublic);
		Dictionary<Hero, int> dictionary = ((campaignBehavior != null) ? ((Dictionary<Hero, int>)field.GetValue(campaignBehavior)) : null);
		if (hero.Age < (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
		{
			if (!hero.IsDisabled && !hero.IsNotSpawned)
			{
				if (isPlayerFamilyRescued)
				{
					hero.ChangeState((CharacterStates)0);
				}
				else
				{
					DisableHeroAction.Apply(hero);
				}
			}
			if (!hero.IsDisabled && dictionary != null && !dictionary.ContainsKey(hero))
			{
				dictionary.Add(hero, (int)hero.Age);
				field.SetValue(campaignBehavior, dictionary);
			}
		}
		else if (isPlayerFamilyRescued)
		{
			if (dictionary != null && dictionary.ContainsKey(hero))
			{
				dictionary.Remove(hero);
			}
			CheckPlayerSiblingsEducationStages(hero);
			CheckStoryModeHeroStateAndUpdateIfNeeded(hero);
			StoryModeHelpers.SetPlayerSiblingsSkillsIfNeeded(hero);
		}
		else if (!hero.IsDisabled)
		{
			DisableHeroAction.Apply(hero);
			if (hero.GovernorOf != null)
			{
				ChangeGovernorAction.RemoveGovernorOf(hero);
			}
		}
		CheckAndUpdateGovernorStatusOfStoryModeHero(hero);
	}

	private void CheckPlayerSiblingsEducationStages(Hero hero)
	{
		EducationCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<EducationCampaignBehavior>();
		if (campaignBehavior != null)
		{
			Type typeFromHandle = typeof(EducationCampaignBehavior);
			if (((Dictionary<Hero, short>)typeFromHandle.GetField("_previousEducations", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(campaignBehavior)).ContainsKey(hero) || !IsHeroAttributesInitialized(hero))
			{
				typeFromHandle.GetMethod("OnHeroComesOfAge", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(campaignBehavior, new object[1] { hero });
			}
		}
	}

	private void CheckStoryModeHeroStateAndUpdateIfNeeded(Hero hero)
	{
		if (hero.IsNotSpawned || hero.IsDisabled)
		{
			Settlement settlementToSpawnForPlayerRelative = GetSettlementToSpawnForPlayerRelative(hero);
			if (hero.BornSettlement == null)
			{
				hero.BornSettlement = settlementToSpawnForPlayerRelative;
			}
			TeleportHeroAction.ApplyImmediateTeleportToSettlement(hero, settlementToSpawnForPlayerRelative);
			if (!hero.IsActive)
			{
				hero.ChangeState((CharacterStates)1);
			}
		}
		if (hero.Clan == null)
		{
			hero.Clan = Clan.PlayerClan;
			if (!hero.IsFugitive)
			{
				MakeHeroFugitiveAction.Apply(hero, false);
			}
		}
	}

	private static void CheckAndUpdateGovernorStatusOfStoryModeHero(Hero hero)
	{
		if (hero.GovernorOf != null && hero.CurrentSettlement != ((SettlementComponent)hero.GovernorOf).Settlement)
		{
			ChangeGovernorAction.RemoveGovernorOf(hero);
		}
	}

	private bool IsHeroAttributesInitialized(Hero hero)
	{
		foreach (CharacterAttribute item in (List<CharacterAttribute>)(object)Attributes.All)
		{
			if (hero.GetAttributeValue(item) != 0)
			{
				return true;
			}
		}
		return false;
	}

	private Settlement GetSettlementToSpawnForPlayerRelative(Hero hero)
	{
		if (hero.GovernorOf != null)
		{
			return ((SettlementComponent)hero.GovernorOf).Settlement;
		}
		if (!hero.HomeSettlement.OwnerClan.IsAtWarWith(Clan.PlayerClan.MapFaction))
		{
			return hero.HomeSettlement;
		}
		if (!Extensions.IsEmpty<Settlement>((IEnumerable<Settlement>)Clan.PlayerClan.MapFaction.Settlements))
		{
			return Extensions.GetRandomElement<Settlement>(Clan.PlayerClan.MapFaction.Settlements);
		}
		foreach (Settlement item in (List<Settlement>)(object)Settlement.All)
		{
			if (!item.MapFaction.IsAtWarWith(Clan.PlayerClan.MapFaction))
			{
				return item;
			}
		}
		return ((SettlementComponent)Extensions.GetRandomElement<Village>(Village.All)).Settlement;
	}
}
