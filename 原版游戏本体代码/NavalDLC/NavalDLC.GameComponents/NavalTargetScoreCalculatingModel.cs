using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace NavalDLC.GameComponents;

public class NavalTargetScoreCalculatingModel : TargetScoreCalculatingModel
{
	public override float TravelingToAssignmentFactor => ((MBGameModel<TargetScoreCalculatingModel>)this).BaseModel.TravelingToAssignmentFactor;

	public override float BesiegingFactor => ((MBGameModel<TargetScoreCalculatingModel>)this).BaseModel.BesiegingFactor;

	public override float AssaultingTownFactor => ((MBGameModel<TargetScoreCalculatingModel>)this).BaseModel.AssaultingTownFactor;

	public override float RaidingFactor => ((MBGameModel<TargetScoreCalculatingModel>)this).BaseModel.RaidingFactor;

	public override float DefendingFactor => ((MBGameModel<TargetScoreCalculatingModel>)this).BaseModel.DefendingFactor;

	public override float GetDefensivePatrollingFactor(bool isNavalPatrolling)
	{
		float num = ((MBGameModel<TargetScoreCalculatingModel>)this).BaseModel.GetDefensivePatrollingFactor(isNavalPatrolling);
		if (isNavalPatrolling)
		{
			num *= 0.66f;
		}
		return num;
	}

	public override float GetOffensivePatrollingFactor(bool isNavalPatrolling)
	{
		return Campaign.Current.Models.TargetScoreCalculatingModel.GetDefensivePatrollingFactor(isNavalPatrolling) * 2f;
	}

	public override float GetTargetScoreForFaction(Settlement targetSettlement, ArmyTypes missionType, MobileParty mobileParty, float ourStrength)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<TargetScoreCalculatingModel>)this).BaseModel.GetTargetScoreForFaction(targetSettlement, missionType, mobileParty, ourStrength);
	}

	public override float CalculateDefensivePatrollingScoreForSettlement(Settlement settlement, bool isTargetingPort, MobileParty mobileParty)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Invalid comparison between Unknown and I4
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Invalid comparison between Unknown and I4
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		if (isTargetingPort)
		{
			if (!mobileParty.HasNavalNavigationCapability || !settlement.HasPort || settlement.MapFaction != mobileParty.MapFaction)
			{
				return 0f;
			}
			float num = ((mobileParty.Food / (0f - mobileParty.FoodChange) > 5f) ? 1f : 0.2f);
			Clan ownerClan = settlement.OwnerClan;
			Hero leaderHero = mobileParty.LeaderHero;
			float num2 = ((ownerClan == ((leaderHero != null) ? leaderHero.Clan : null)) ? 1f : 0.5f);
			bool flag = (int)mobileParty.DefaultBehavior == 13 && !mobileParty.TargetPosition.IsOnLand && mobileParty.TargetSettlement != null && !mobileParty.TargetSettlement.MapFaction.IsAtWarWith(mobileParty.MapFaction);
			bool num3 = (int)mobileParty.DefaultBehavior == 13 && mobileParty.TargetPosition.IsOnLand;
			float num4 = (flag ? 1.35f : 1f);
			float num5 = (3f + settlement.NearbyNavalThreatIntensity - settlement.NearbyNavalAllyIntensity * 1.5f) * (flag ? 1.5f : 1f);
			float num6 = LinQuick.SumQ<Ship>((List<Ship>)(object)mobileParty.Ships, (Func<Ship, float>)((Ship x) => x.HitPoints / x.MaxHitPoints)) / (float)((List<Ship>)(object)mobileParty.Ships).Count;
			float num7 = (num3 ? 0.5f : 1f);
			return num4 * num2 * num5 * num6 * num7 * num * Campaign.Current.Models.TargetScoreCalculatingModel.GetDefensivePatrollingFactor(true);
		}
		return ((MBGameModel<TargetScoreCalculatingModel>)this).BaseModel.CalculateDefensivePatrollingScoreForSettlement(settlement, false, mobileParty);
	}

	public override float CurrentObjectiveValue(MobileParty mobileParty)
	{
		return ((MBGameModel<TargetScoreCalculatingModel>)this).BaseModel.CurrentObjectiveValue(mobileParty);
	}

	public override float CalculateOffensivePatrollingScoreForSettlement(Settlement settlement, bool isTargetingPort, MobileParty mobileParty)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Invalid comparison between Unknown and I4
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Invalid comparison between Unknown and I4
		float num = ((mobileParty.Food / (0f - mobileParty.FoodChange) > 6f) ? 1f : 0.2f);
		bool num2 = (int)mobileParty.DefaultBehavior == 13 && !mobileParty.TargetPosition.IsOnLand && mobileParty.TargetSettlement != null && mobileParty.TargetSettlement == settlement && mobileParty.TargetSettlement.MapFaction.IsAtWarWith(mobileParty.MapFaction);
		bool flag = (int)mobileParty.DefaultBehavior == 13 && mobileParty.TargetPosition.IsOnLand;
		float num3 = (num2 ? 1.2f : 1f);
		float num4 = LinQuick.SumQ<Ship>((List<Ship>)(object)mobileParty.Ships, (Func<Ship, float>)((Ship x) => x.HitPoints / x.MaxHitPoints)) / (float)((List<Ship>)(object)mobileParty.Ships).Count;
		float num5 = (flag ? 0.5f : 1f);
		float num6 = (settlement.IsVillage ? 1.2f : 1f);
		int num7 = 0;
		foreach (WarPartyComponent item in (List<WarPartyComponent>)(object)mobileParty.MapFaction.WarPartyComponents)
		{
			if (((PartyComponent)item).MobileParty != mobileParty && (int)((PartyComponent)item).MobileParty.DefaultBehavior == 13 && ((PartyComponent)item).MobileParty.TargetSettlement == settlement && ((PartyComponent)item).MobileParty.IsTargetingPort)
			{
				num7++;
			}
		}
		float num8 = MathF.Pow(0.5f, (float)num7);
		return num3 * num4 * num * num5 * num8 * num6 * Campaign.Current.Models.TargetScoreCalculatingModel.GetOffensivePatrollingFactor(true);
	}
}
