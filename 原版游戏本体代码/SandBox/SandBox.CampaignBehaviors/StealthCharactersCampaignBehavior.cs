using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace SandBox.CampaignBehaviors;

public class StealthCharactersCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedPoints)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		if (!settlement.IsHideout)
		{
			Location val = settlement.LocationComplex.GetListOfLocations().First();
			if (unusedPoints.TryGetValue("stealth_agent", out var value) && value > 0)
			{
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateStealthCharacter), settlement.Culture, (CharacterRelations)2, value);
			}
			if (unusedPoints.TryGetValue("stealth_agent_forced", out value) && value > 0)
			{
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreteForcedStealthCharacter), settlement.Culture, (CharacterRelations)2, value);
			}
			if (unusedPoints.TryGetValue("disguise_default_agent", out value) && value > 0)
			{
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateDisguiseDefaultCharacter), settlement.Culture, (CharacterRelations)2, value);
			}
			if (unusedPoints.TryGetValue("disguise_officer_agent", out value) && value > 0)
			{
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateDisguiseOfficerCharacter), settlement.Culture, (CharacterRelations)2, value);
			}
			if (unusedPoints.TryGetValue("disguise_shadow_agent", out value) && value > 0)
			{
				val.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateDisguiseShadowTargetCharacter), settlement.Culture, (CharacterRelations)2, value);
			}
		}
	}

	private LocationCharacter CreateStealthCharacter(CultureObject culture, CharacterRelations relation)
	{
		return CreateStealthAgentInternal("stealth_agent", "stealth_character");
	}

	private LocationCharacter CreteForcedStealthCharacter(CultureObject culture, CharacterRelations relation)
	{
		LocationCharacter obj = CreateStealthAgentInternal("stealth_agent_forced", "stealth_character");
		obj.ForceSpawnInSpecialTargetTag = true;
		return obj;
	}

	private LocationCharacter CreateDisguiseDefaultCharacter(CultureObject culture, CharacterRelations relation)
	{
		return CreateStealthAgentInternal("disguise_default_agent", "disguise_default_character");
	}

	private LocationCharacter CreateDisguiseOfficerCharacter(CultureObject culture, CharacterRelations relation)
	{
		return CreateStealthAgentInternal("disguise_officer_agent", "disguise_officer_character");
	}

	private LocationCharacter CreateDisguiseShadowTargetCharacter(CultureObject culture, CharacterRelations relation)
	{
		return CreateStealthAgentInternal("disguise_shadow_agent", "disguise_shadow_target");
	}

	private LocationCharacter CreateStealthAgentInternal(string spawnTag, string characterId)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>(characterId);
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(val, ref num, ref num2, "");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)val).Race, "_settlement_slow")).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddStealthAgentBehaviors), spawnTag, true, (CharacterRelations)2, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
