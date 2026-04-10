using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCMapVisibilityModel : MapVisibilityModel
{
	private const float SeaSpottingRangeBonus = 0.3f;

	private const float StormSpottingRangePenalty = -0.4f;

	public override float MaximumSeeingRange()
	{
		return ((MBGameModel<MapVisibilityModel>)this).BaseModel.MaximumSeeingRange();
	}

	public override float GetPartySeeingRangeBase(MobileParty party)
	{
		float num = ((MBGameModel<MapVisibilityModel>)this).BaseModel.GetPartySeeingRangeBase(party);
		if (party.IsCurrentlyAtSea)
		{
			if (party.IsInRaftState)
			{
				num *= 0.5f;
			}
			if (Campaign.Current.IsNight && party.HasPerk(NavalPerks.Shipmaster.NightRaider, false))
			{
				num += 3f;
			}
		}
		return num;
	}

	public override ExplainedNumber GetPartySpottingRange(MobileParty party, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		ExplainedNumber partySpottingRange = ((MBGameModel<MapVisibilityModel>)this).BaseModel.GetPartySpottingRange(party, includeDescriptions);
		if (party.IsCurrentlyAtSea)
		{
			PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.RavenEye, party, true, ref partySpottingRange, false);
			((ExplainedNumber)(ref partySpottingRange)).AddFactor(0.3f, new TextObject("{=B0aCb3Je}At Sea", (Dictionary<string, object>)null));
			foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
			{
				if (item.IsActive)
				{
					Vec2 currentPosition = item.CurrentPosition;
					CampaignVec2 position = party.Position;
					if (((Vec2)(ref currentPosition)).DistanceSquared(((CampaignVec2)(ref position)).ToVec2()) < item.EffectRadius * item.EffectRadius)
					{
						((ExplainedNumber)(ref partySpottingRange)).AddFactor(-0.4f, new TextObject("{=M6V6eCTg}Storm", (Dictionary<string, object>)null));
						break;
					}
				}
			}
		}
		return partySpottingRange;
	}

	public override float GetPartySpottingRatioForMainPartySeeingRange(MobileParty party)
	{
		return ((MBGameModel<MapVisibilityModel>)this).BaseModel.GetPartySpottingRatioForMainPartySeeingRange(party);
	}

	public override float GetHideoutSpottingDistance()
	{
		return ((MBGameModel<MapVisibilityModel>)this).BaseModel.GetHideoutSpottingDistance();
	}
}
