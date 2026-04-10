using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;

namespace SandBox.CampaignBehaviors;

public class RecruitmentAgentSpawnBehavior : CampaignBehaviorBase
{
	private RecruitmentCampaignBehavior RecruitmentBehavior => Campaign.Current.CampaignBehaviorManager.GetBehavior<RecruitmentCampaignBehavior>();

	public override void RegisterEvents()
	{
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
		CampaignEvents.MercenaryNumberChangedInTown.AddNonSerializedListener((object)this, (Action<Town, int, int>)OnMercenaryNumberChanged);
		CampaignEvents.MercenaryTroopChangedInTown.AddNonSerializedListener((object)this, (Action<Town, CharacterObject, CharacterObject>)OnMercenaryTroopChanged);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		Location locationWithId = settlement.LocationComplex.GetLocationWithId("tavern");
		if (CampaignMission.Current.Location == locationWithId)
		{
			AddMercenaryCharacterToTavern(settlement);
		}
	}

	private void CheckIfMercenaryCharacterNeedsToRefresh(Settlement settlement, CharacterObject oldTroopType)
	{
		if (!settlement.IsTown || settlement != Settlement.CurrentSettlement || PlayerEncounter.LocationEncounter == null || settlement.LocationComplex == null || (CampaignMission.Current != null && GameStateManager.Current.ActiveState == CampaignMission.Current.State))
		{
			return;
		}
		if (oldTroopType != null)
		{
			Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern").RemoveAllCharacters((Predicate<LocationCharacter>)((LocationCharacter x) => x.Character.Occupation == oldTroopType.Occupation));
		}
		AddMercenaryCharacterToTavern(settlement);
	}

	private void OnMercenaryNumberChanged(Town town, int oldNumber, int newNumber)
	{
		if (RecruitmentBehavior != null)
		{
			CheckIfMercenaryCharacterNeedsToRefresh(((SettlementComponent)town).Owner.Settlement, RecruitmentBehavior.GetMercenaryData(town).TroopType);
		}
	}

	private void OnMercenaryTroopChanged(Town town, CharacterObject oldTroopType, CharacterObject newTroopType)
	{
		CheckIfMercenaryCharacterNeedsToRefresh(((SettlementComponent)town).Owner.Settlement, oldTroopType);
	}

	private void AddMercenaryCharacterToTavern(Settlement settlement)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		if (settlement.LocationComplex != null && settlement.IsTown && RecruitmentBehavior != null && RecruitmentBehavior.GetMercenaryData(settlement.Town).HasAvailableMercenary((Occupation)0))
		{
			Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
			if (locationWithId != null)
			{
				locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateMercenary), settlement.Culture, (CharacterRelations)0, 1);
			}
		}
	}

	private LocationCharacter CreateMercenary(CultureObject culture, CharacterRelations relation)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		CharacterObject troopType = RecruitmentBehavior.GetMercenaryData(PlayerEncounter.EncounterSettlement.Town).TroopType;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)troopType).Race, "_settlement");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)troopType, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).NoHorses(true);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddOutdoorWandererBehaviors), "spawnpoint_mercenary", true, relation, (string)null, false, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}
}
