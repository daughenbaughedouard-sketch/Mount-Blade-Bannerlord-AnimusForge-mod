using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCPartySpeedCalculationModel : PartySpeedModel
{
	private const float RiverBonus = 0.5f;

	private const float OpenSeaBonus = 0.448f;

	private const int PartyFleetSizeThreshold = 3;

	private const int RaftStateSpeed = 4;

	private const float DisorganizedEffect = -0.4f;

	private const float WindDeadZoneThresholdInDegrees = 60f;

	private const float OverburdenedEffect = -1f;

	private const float MaximumNavalSpeed = 10f;

	private static readonly TextObject _textOverburdened = new TextObject("{=xgO3cCgR}Overburdened", (Dictionary<string, object>)null);

	private static readonly TextObject _textOverFleetSize = new TextObject("{=D3OvWCpp}Over fleet size", (Dictionary<string, object>)null);

	private static readonly TextObject _textDisorganized = new TextObject("{=JuwBb2Yg}Disorganized", (Dictionary<string, object>)null);

	private static readonly TextObject _textShallowDraftPenalty = new TextObject("{=RU7pNBts}Shallow Draft", (Dictionary<string, object>)null);

	private static readonly TextObject _openSeaEffect = new TextObject("{=KzEFMlfZ}Open Sea", (Dictionary<string, object>)null);

	private static readonly TextObject _riverEffect = new TextObject("{=UvIsHvrt}River", (Dictionary<string, object>)null);

	private static readonly TextObject _windEffect = new TextObject("{=lJDeXyt1}Wind", (Dictionary<string, object>)null);

	private static readonly TextObject _gunnarEffect = new TextObject("{=LSVGrpMr}Gunnar's Skill", (Dictionary<string, object>)null);

	private readonly TextObject _cultureEffect = GameTexts.FindText("str_culture", (string)null);

	public override float BaseSpeed => ((MBGameModel<PartySpeedModel>)this).BaseModel.BaseSpeed;

	public override float MinimumSpeed => ((MBGameModel<PartySpeedModel>)this).BaseModel.MinimumSpeed;

	public override ExplainedNumber CalculateBaseSpeed(MobileParty party, bool includeDescriptions = false, int additionalTroopOnFootCount = 0, int additionalTroopOnHorseCount = 0)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (party.IsCurrentlyAtSea)
		{
			return CalculateNavalBaseSpeed(party, includeDescriptions);
		}
		return ((MBGameModel<PartySpeedModel>)this).BaseModel.CalculateBaseSpeed(party, includeDescriptions, additionalTroopOnFootCount, additionalTroopOnHorseCount);
	}

	private ExplainedNumber CalculateNavalBaseSpeed(MobileParty mobileParty, bool includeDescriptions = false)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Expected O, but got Unknown
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Expected O, but got Unknown
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		if (!((IEnumerable<Ship>)mobileParty.Ships).Any())
		{
			return new ExplainedNumber(4f, includeDescriptions, (TextObject)null);
		}
		float totalShipSpeed = 0f;
		float minimumShipSpeed = float.MaxValue;
		int neededSkeletalCrew = 0;
		int num = mobileParty.MemberRoster.TotalManCount;
		float num2 = mobileParty.TotalWeightCarried;
		int num3 = ((List<Ship>)(object)mobileParty.Ships).Count;
		int maximumCrewLimit = 0;
		GetMobilePartyShipSpeedData(mobileParty, ref neededSkeletalCrew, ref maximumCrewLimit, ref totalShipSpeed, ref minimumShipSpeed);
		if (((List<MobileParty>)(object)mobileParty.AttachedParties).Count != 0)
		{
			foreach (MobileParty item in (List<MobileParty>)(object)mobileParty.AttachedParties)
			{
				num3 += ((List<Ship>)(object)item.Ships).Count;
				num += item.MemberRoster.TotalManCount;
				num2 += item.TotalWeightCarried;
				GetMobilePartyShipSpeedData(item, ref neededSkeletalCrew, ref maximumCrewLimit, ref totalShipSpeed, ref minimumShipSpeed);
			}
		}
		float num4 = (totalShipSpeed / (float)num3 + minimumShipSpeed) * 0.5f;
		ExplainedNumber result = default(ExplainedNumber);
		((ExplainedNumber)(ref result))._002Ector(num4, includeDescriptions, (TextObject)null);
		if (mobileParty.IsFishingParty())
		{
			Settlement bound = mobileParty.VillagerPartyComponent.Village.Bound;
			PerkHelper.AddPerkBonusForTown(NavalPerks.Shipmaster.GhostShip, bound.Town, ref result);
		}
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector((float)neededSkeletalCrew, false, (TextObject)null);
		PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.FleetCommander, mobileParty, false, ref val, false);
		neededSkeletalCrew = ((ExplainedNumber)(ref val)).RoundedResultNumber;
		if (mobileParty.HasPerk(NavalPerks.Shipmaster.ChainToOars, true))
		{
			num += mobileParty.PrisonRoster.TotalManCount;
		}
		foreach (MobileParty item2 in (List<MobileParty>)(object)mobileParty.AttachedParties)
		{
			if (item2.HasPerk(NavalPerks.Shipmaster.ChainToOars, true))
			{
				num += item2.PrisonRoster.TotalManCount;
			}
		}
		if (num < neededSkeletalCrew)
		{
			float underSkeletalCrewEffect = GetUnderSkeletalCrewEffect(num, neededSkeletalCrew);
			TextObject val2 = null;
			if (includeDescriptions)
			{
				val2 = new TextObject("{=4LlzFaUa}Undermanned ({AVAILABLE_CREW}/{NEEDED_CREW})", (Dictionary<string, object>)null);
				val2.SetTextVariable("AVAILABLE_CREW", num);
				val2.SetTextVariable("NEEDED_CREW", neededSkeletalCrew);
			}
			((ExplainedNumber)(ref result)).AddFactor(underSkeletalCrewEffect, val2);
		}
		if (num > maximumCrewLimit)
		{
			float overCrewSizeEffect = GetOverCrewSizeEffect(num, maximumCrewLimit);
			TextObject val3 = null;
			if (includeDescriptions)
			{
				val3 = new TextObject("{=X8V6b6mC}Overmanned ({AVAILABLE_CREW}/{NEEDED_CREW})", (Dictionary<string, object>)null);
				val3.SetTextVariable("AVAILABLE_CREW", num);
				val3.SetTextVariable("NEEDED_CREW", maximumCrewLimit);
			}
			((ExplainedNumber)(ref result)).AddFactor(overCrewSizeEffect, val3);
		}
		ExplainedNumber val4 = Campaign.Current.Models.InventoryCapacityModel.CalculateInventoryCapacity(mobileParty, mobileParty.IsCurrentlyAtSea, false, 0, 0, 0, false);
		int num5 = (int)((ExplainedNumber)(ref val4)).ResultNumber;
		if (num2 > (float)num5)
		{
			ExplainedNumber overburdenedEffect = GetOverburdenedEffect(mobileParty, num2 - (float)num5, num5, includeDescriptions);
			((ExplainedNumber)(ref result)).AddFromExplainedNumber(overburdenedEffect, _textOverburdened);
		}
		if (num3 > 3)
		{
			int num6 = num3 - 3;
			float num7 = 0.5f;
			float num8 = 0.2f / (1f + (float)Math.Exp((0f - num7) * ((float)num6 - 3f)));
			if (mobileParty.HasPerk(NavalPerks.Shipmaster.ShoreMaster, true))
			{
				num8 *= 1f + NavalPerks.Shipmaster.ShoreMaster.SecondaryBonus;
			}
			if (mobileParty.HasPerk(NavalPerks.Shipmaster.FleetCommander, false))
			{
				num8 *= 1f + NavalPerks.Shipmaster.FleetCommander.PrimaryBonus;
			}
			((ExplainedNumber)(ref result)).AddFactor(0f - num8, _textOverFleetSize);
		}
		if (mobileParty.IsDisorganized)
		{
			((ExplainedNumber)(ref result)).AddFactor(-0.4f, _textDisorganized);
		}
		((ExplainedNumber)(ref result)).LimitMin(((PartySpeedModel)this).MinimumSpeed);
		return result;
	}

	public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Invalid comparison between Unknown and I4
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Invalid comparison between Unknown and I4
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Invalid comparison between Unknown and I4
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Invalid comparison between Unknown and I4
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Invalid comparison between Unknown and I4
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Invalid comparison between Unknown and I4
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Invalid comparison between Unknown and I4
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Invalid comparison between Unknown and I4
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Invalid comparison between Unknown and I4
		ExplainedNumber result = ((MBGameModel<PartySpeedModel>)this).BaseModel.CalculateFinalSpeed(mobileParty, finalSpeed);
		TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(mobileParty.CurrentNavigationFace);
		if (mobileParty.IsCurrentlyAtSea)
		{
			if ((int)faceTerrainType == 19)
			{
				((ExplainedNumber)(ref result)).AddFactor(0.448f, _openSeaEffect);
			}
			else if ((int)faceTerrainType == 11)
			{
				((ExplainedNumber)(ref result)).AddFactor(0.5f, _riverEffect);
			}
			if (((List<Ship>)(object)mobileParty.Ships).Count > 0)
			{
				float num = 0f;
				foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
				{
					if (!item.ShipHull.HasHold)
					{
						num = (((int)faceTerrainType != 18 && (int)faceTerrainType != 11 && (int)faceTerrainType != 25) ? (num - item.GetCampaignSpeed() * 0.066f) : (num + item.GetCampaignSpeed() * 0.066f));
					}
				}
				((ExplainedNumber)(ref result)).Add(num / (float)((List<Ship>)(object)mobileParty.Ships).Count, _textShallowDraftPenalty, (TextObject)null);
			}
			if (((int)faceTerrainType == 11 || (int)faceTerrainType == 18 || (int)faceTerrainType == 25) && mobileParty.HasPerk(NavalPerks.Shipmaster.RiverRaider, false))
			{
				((ExplainedNumber)(ref result)).AddFactor(-0.448f * NavalPerks.Shipmaster.RiverRaider.PrimaryBonus, ((PropertyObject)NavalPerks.Shipmaster.RiverRaider).Name);
			}
			if (((int)faceTerrainType == 11 || (int)faceTerrainType == 18 || (int)faceTerrainType == 25) && PartyBaseHelper.HasFeat(mobileParty.Party, NavalCulturalFeats.NordShipMovementFeat))
			{
				((ExplainedNumber)(ref result)).AddFactor(NavalCulturalFeats.NordShipMovementFeat.EffectBonus, _cultureEffect);
			}
			SkillHelper.AddSkillBonusForParty(NavalSkillEffects.WindBonus, mobileParty, ref result);
			PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.OldSaltsTouch, mobileParty, true, ref result, false);
			PerkHelper.AddPerkBonusForParty(NavalPerks.Shipmaster.FavorableTide, mobileParty, true, ref result, false);
			float num2 = CalculateWindBoostForParty(mobileParty);
			((ExplainedNumber)(ref result)).AddFactor(num2 * (1f + ((ExplainedNumber)(ref result)).SumOfFactors), _windEffect);
			if (mobileParty.IsMainParty && NavalStorylineData.IsNavalStoryLineActive())
			{
				((ExplainedNumber)(ref result)).Add(1f, _gunnarEffect, (TextObject)null);
			}
			((ExplainedNumber)(ref result)).LimitMax(10f, (TextObject)null);
		}
		return result;
	}

	private float CalculateWindBoostForParty(MobileParty mobileParty)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Vec2 windForPosition = Campaign.Current.Models.MapWeatherModel.GetWindForPosition(mobileParty.Position);
		Vec2 bearing = mobileParty.Bearing;
		float num = MathF.Abs(((Vec2)(ref bearing)).RotationInRadians - ((Vec2)(ref windForPosition)).RotationInRadians) * 57.29578f;
		if (((Vec2)(ref windForPosition)).Length > 0f)
		{
			if (num < 120f)
			{
				float num2 = MBMath.ClampFloat(MBMath.Map(num, 0f, 120f, ((Vec2)(ref windForPosition)).Length, 0f) * 1.5f, 0f, 1.5f);
				if (mobileParty.HasPerk(NavalPerks.Shipmaster.FairWinds, false))
				{
					num2 += NavalPerks.Shipmaster.FairWinds.PrimaryBonus;
				}
				return num2;
			}
			float result = 0f;
			if (mobileParty.HasPerk(NavalPerks.Shipmaster.ShockAndAwe, true))
			{
				result = NavalPerks.Shipmaster.ShockAndAwe.SecondaryBonus;
			}
			return result;
		}
		return 0f;
	}

	private ExplainedNumber GetOverburdenedEffect(MobileParty party, float extraWeightCarried, int partyCapacity, bool includeDescriptions)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = default(ExplainedNumber);
		((ExplainedNumber)(ref result))._002Ector(-1f * (extraWeightCarried / (float)partyCapacity), includeDescriptions, (TextObject)null);
		PerkHelper.AddPerkBonusForParty(NavalPerks.Boatswain.VeteransWisdom, party, false, ref result, false);
		return result;
	}

	private void GetMobilePartyShipSpeedData(MobileParty mobileParty, ref int neededSkeletalCrew, ref int maximumCrewLimit, ref float totalShipSpeed, ref float minimumShipSpeed)
	{
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			neededSkeletalCrew += item.SkeletalCrewCapacity;
			maximumCrewLimit += item.TotalCrewCapacity;
			float campaignSpeed = item.GetCampaignSpeed();
			totalShipSpeed += campaignSpeed;
			if (campaignSpeed < minimumShipSpeed)
			{
				minimumShipSpeed = campaignSpeed;
			}
		}
	}

	private float GetOverCrewSizeEffect(int totalMenCount, int maxCrewSize)
	{
		return 1f / ((float)totalMenCount / (float)maxCrewSize) - 1f;
	}

	private float GetUnderSkeletalCrewEffect(float totalManCount, float neededSkeletalCrew)
	{
		float num = totalManCount / neededSkeletalCrew;
		return (0f - (1f - num)) * 0.4f;
	}
}
