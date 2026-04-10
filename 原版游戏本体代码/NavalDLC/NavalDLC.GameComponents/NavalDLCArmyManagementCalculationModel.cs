using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCArmyManagementCalculationModel : ArmyManagementCalculationModel
{
	public override float AIMobilePartySizeRatioToCallToArmy => ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.AIMobilePartySizeRatioToCallToArmy;

	public override float PlayerMobilePartySizeRatioToCallToArmy => ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.PlayerMobilePartySizeRatioToCallToArmy;

	public override float MinimumNeededFoodInDaysToCallToArmy => ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.MinimumNeededFoodInDaysToCallToArmy;

	public override float MaximumDistanceToCallToArmy => ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.MaximumDistanceToCallToArmy;

	public override int InfluenceValuePerGold => ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.InfluenceValuePerGold;

	public override int AverageCallToArmyCost => ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.AverageCallToArmyCost;

	public override int CohesionThresholdForDispersion => ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.CohesionThresholdForDispersion;

	public override float MaximumWaitTime => ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.MaximumWaitTime;

	public override ExplainedNumber CalculateDailyCohesionChange(Army army, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.CalculateDailyCohesionChange(army, includeDescriptions);
		if (army.LeaderParty != null && !army.LeaderParty.IsCurrentlyAtSea && PartyBaseHelper.HasFeat(army.LeaderParty.Party, NavalCulturalFeats.NordArmyCohesionFeat))
		{
			((ExplainedNumber)(ref result)).AddFactor(NavalCulturalFeats.NordArmyCohesionFeat.EffectBonus, GameTexts.FindText("str_culture", (string)null));
		}
		return result;
	}

	public override int CalculateNewCohesion(Army army, PartyBase newParty, int calculatedCohesion, int sign)
	{
		return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.CalculateNewCohesion(army, newParty, calculatedCohesion, sign);
	}

	public override int CalculatePartyInfluenceCost(MobileParty armyLeaderParty, MobileParty party)
	{
		return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.CalculatePartyInfluenceCost(armyLeaderParty, party);
	}

	public override bool CanLordCreateArmy(MobileParty leaderParty, out MBList<MobileParty> possibleArmyMembers)
	{
		return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.CanLordCreateArmy(leaderParty, ref possibleArmyMembers);
	}

	public override int CalculateTotalInfluenceCost(Army army, float percentage)
	{
		return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.CalculateTotalInfluenceCost(army, percentage);
	}

	public override bool CanPlayerCreateArmy(out TextObject disabledReason)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (!NavalStorylineData.IsNavalStoryLineActive())
		{
			MenuContext currentMenuContext = Campaign.Current.CurrentMenuContext;
			object obj;
			if (currentMenuContext == null)
			{
				obj = null;
			}
			else
			{
				GameMenu gameMenu = currentMenuContext.GameMenu;
				obj = ((gameMenu != null) ? gameMenu.StringId : null);
			}
			if (!((string?)obj == "naval_storyline_outside_town"))
			{
				return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.CanPlayerCreateArmy(ref disabledReason);
			}
		}
		disabledReason = new TextObject("{=lwbwTg5b}You can't perform this action during this time.", (Dictionary<string, object>)null);
		return false;
	}

	public override bool CheckPartyEligibility(MobileParty party, out TextObject explanation)
	{
		return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.CheckPartyEligibility(party, ref explanation);
	}

	public override float DailyBeingAtArmyInfluenceAward(MobileParty armyMemberParty)
	{
		return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.DailyBeingAtArmyInfluenceAward(armyMemberParty);
	}

	public override int GetCohesionBoostInfluenceCost(Army army, int percentageToBoost = 100)
	{
		return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.GetCohesionBoostInfluenceCost(army, percentageToBoost);
	}

	public override int GetPartyRelation(Hero hero)
	{
		return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.GetPartyRelation(hero);
	}

	public override float GetPartySizeScore(MobileParty party)
	{
		return ((MBGameModel<ArmyManagementCalculationModel>)this).BaseModel.GetPartySizeScore(party);
	}
}
