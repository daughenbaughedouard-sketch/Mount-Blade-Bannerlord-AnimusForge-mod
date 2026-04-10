using System;
using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCCampaignShipDamageModel : CampaignShipDamageModel
{
	private const float MaximumDamageToShip = 10000f;

	private const float MinimumDamageToShip = 1f;

	private const float AverageBeingOnOpenSeaRatio = 0.27f;

	public override int GetHourlyShipDamage(MobileParty owner, Ship ship)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		int result = 0;
		if (owner.CurrentSettlement == null && owner.MapEvent == null && (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(owner.CurrentNavigationFace) == 19)
		{
			result = (int)CalculateOpenSeaAttritionDamageForShip(ship);
		}
		return result;
	}

	public override float GetEstimatedSafeSailDuration(MobileParty mobileParty)
	{
		float num = 0f;
		foreach (Ship item in (List<Ship>)(object)mobileParty.Ships)
		{
			float num2 = CalculateOpenSeaAttritionDamageForShip(item) * 0.27f;
			float num3 = item.HitPoints / num2;
			num += num3;
		}
		return num / (float)((List<Ship>)(object)mobileParty.Ships).Count;
	}

	public override float GetShipDamage(Ship ship, Ship rammingShip, float rawDamage)
	{
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(rawDamage, false, (TextObject)null);
		PartyBase owner = ship.Owner;
		if (owner != null && owner.IsMobile)
		{
			SkillHelper.AddSkillBonusForParty(NavalSkillEffects.ShipDamageReduction, ship.Owner.MobileParty, ref val);
		}
		if (rammingShip != null && rammingShip.Figurehead != null && rammingShip.Figurehead == DefaultFigureheads.Ram)
		{
			((ExplainedNumber)(ref val)).AddFactor(rammingShip.Figurehead.EffectAmount, (TextObject)null);
		}
		return Math.Max(0f, ((ExplainedNumber)(ref val)).ResultNumber);
	}

	private float CalculateOpenSeaAttritionDamageForShip(Ship ship)
	{
		int seaWorthiness = ship.SeaWorthiness;
		return MBMath.ClampFloat(Campaign.Current.Models.CampaignShipParametersModel.GetShipSizeWeatherFactor(ship.ShipHull) * (1f - (float)seaWorthiness / 400f) * ((100f - (float)seaWorthiness) / 100f), 1f, 10000f);
	}
}
