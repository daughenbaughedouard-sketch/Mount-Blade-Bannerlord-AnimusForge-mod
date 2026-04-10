using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;

namespace NavalDLC.Storyline.CampaignBehaviors;

public class NavalStorylineHeroAgentSpawnBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		if (!NavalStorylineData.IsNavalStorylineCanceled())
		{
			CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
			CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
			CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnMissionEnded(IMission mission)
	{
		if (Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner && LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null && !Settlement.CurrentSettlement.IsUnderSiege && Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.HasPort && NavalStorylineData.IsNavalStoryLineActive())
		{
			AddNavalStorylineHeroesInsideMainPartyToPort(Settlement.CurrentSettlement);
		}
	}

	private void OnGameLoadFinished()
	{
		if (Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner && LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null && !Settlement.CurrentSettlement.IsUnderSiege && Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.HasPort && NavalStorylineData.IsNavalStoryLineActive())
		{
			AddNavalStorylineHeroesInsideMainPartyToPort(Settlement.CurrentSettlement);
		}
	}

	private void AddNavalStorylineHeroesInsideMainPartyToPort(Settlement settlement)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)MobileParty.MainParty.MemberRoster.GetTroopRoster())
		{
			CharacterObject character = item.Character;
			if (((BasicCharacterObject)character).IsHero && NavalStorylineData.IsNavalStorylineHero(character.HeroObject))
			{
				Hero heroObject = character.HeroObject;
				AddNavalStorylineHeroToPortAsLocationCharacter(heroObject);
			}
		}
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		if (Settlement.CurrentSettlement != null && !Hero.MainHero.IsPrisoner && LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null && !Settlement.CurrentSettlement.IsUnderSiege && Settlement.CurrentSettlement.IsTown && Settlement.CurrentSettlement.HasPort && NavalStorylineData.IsNavalStoryLineActive())
		{
			AddNavalStorylineHeroesInsideMainPartyToPort(Settlement.CurrentSettlement);
		}
	}

	private void AddNavalStorylineHeroToPortAsLocationCharacter(Hero storylineHero)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)storylineHero.CharacterObject).Race, "_settlement");
		IFaction mapFaction = storylineHero.MapFaction;
		uint num = ((mapFaction != null) ? mapFaction.Color : 4291609515u);
		IFaction mapFaction2 = storylineHero.MapFaction;
		uint num2 = ((mapFaction2 != null) ? mapFaction2.Color : 4291609515u);
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)storylineHero.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).ClothingColor1(num).ClothingColor2(num2).Monster(monsterWithSuffix)
			.NoHorses(true);
		string text = ActionSetCode.GenerateActionSetNameWithSuffix(obj.AgentMonster, storylineHero.IsFemale, "_lord");
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		LocationCharacter val = new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddFixedCharacterBehaviors), "sp_notable", true, (CharacterRelations)0, text, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
		LocationComplex.Current.GetLocationWithId("port").AddCharacter(val);
	}
}
