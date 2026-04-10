using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCRaidModel : RaidModel
{
	public override int GoldRewardForEachLostHearth => ((MBGameModel<RaidModel>)this).BaseModel.GoldRewardForEachLostHearth;

	public override ExplainedNumber CalculateHitDamage(MapEventSide attackerSide, float settlementHitPoints)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<RaidModel>)this).BaseModel.CalculateHitDamage(attackerSide, settlementHitPoints);
		if (PartyBaseHelper.HasFeat(attackerSide.LeaderParty, NavalCulturalFeats.NordHostileActionSpeedFeat))
		{
			((ExplainedNumber)(ref result)).AddFactor(NavalCulturalFeats.NordHostileActionSpeedFeat.EffectBonus, (TextObject)null);
		}
		return result;
	}

	public override MBReadOnlyList<(ItemObject, float)> GetCommonLootItemScores()
	{
		return ((MBGameModel<RaidModel>)this).BaseModel.GetCommonLootItemScores();
	}
}
