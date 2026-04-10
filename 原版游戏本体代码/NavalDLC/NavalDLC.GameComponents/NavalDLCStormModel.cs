using System.Collections.Generic;
using NavalDLC.ComponentInterfaces;
using NavalDLC.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace NavalDLC.GameComponents;

public class NavalDLCStormModel : MapStormModel
{
	public override float MinimumWeatherStrengthInsideStorm => 1f;

	public override int MaximumNumberOfStorms => 6;

	public override float GetPositionDamageForStorm(Storm storm, Vec2 shipPosition, Ship ship)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		float maximumWeatherStrengthAtEye = NavalDLCManager.Instance.GameModels.MapStormModel.GetMaximumWeatherStrengthAtEye(storm);
		Vec2 currentPosition = storm.CurrentPosition;
		float num = ((Vec2)(ref currentPosition)).Distance(shipPosition);
		float minimumWeatherStrengthInsideStorm = MinimumWeatherStrengthInsideStorm;
		minimumWeatherStrengthInsideStorm = ((!(num < storm.EyeRadius)) ? ((num + storm.EyeRadius < storm.EffectRadius) ? MBMath.Map(num, 0f, storm.EffectRadius, maximumWeatherStrengthAtEye, MinimumWeatherStrengthInsideStorm) : 0f) : maximumWeatherStrengthAtEye);
		IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
		CampaignVec2 val = new CampaignVec2(shipPosition, false);
		PathFaceRecord faceIndex = mapSceneWrapper.GetFaceIndex(ref val);
		Campaign.Current.MapSceneWrapper.GetFaceTerrainType(faceIndex);
		float result = 0f;
		int seaWorthiness = ship.SeaWorthiness;
		if (!((float)seaWorthiness / 2f > minimumWeatherStrengthInsideStorm * 10f))
		{
			result = Campaign.Current.Models.CampaignShipParametersModel.GetShipSizeWeatherFactor(ship.ShipHull) * (minimumWeatherStrengthInsideStorm - (float)seaWorthiness / 400f) * ((100f - (float)seaWorthiness) / 100f);
			float num2 = 10000f;
			float num3 = 1f;
			result = MBMath.ClampFloat(result, num3, num2);
		}
		return result;
	}

	public override float GetHourlyIntensityChangeForStorm(Storm storm)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 val = default(CampaignVec2);
		((CampaignVec2)(ref val))._002Ector(storm.CurrentPosition, false);
		PathFaceRecord faceIndex = Campaign.Current.MapSceneWrapper.GetFaceIndex(ref val);
		if (((PathFaceRecord)(ref faceIndex)).IsValid())
		{
			Vec2 windAtPosition = NavalDLCManager.Instance.NavalMapSceneWrapper.GetWindAtPosition(((CampaignVec2)(ref val)).ToVec2());
			if (!(((Vec2)(ref windAtPosition)).Length < 0.15f))
			{
				return 0.01f;
			}
			return -0.01f;
		}
		return -0.05f;
	}

	public override float GetMaximumWeatherStrengthAtEye(Storm storm)
	{
		return storm.StormType switch
		{
			Storm.StormTypes.Storm => 3f, 
			Storm.StormTypes.ThunderStorm => 6f, 
			Storm.StormTypes.Hurricane => 10f, 
			_ => 0f, 
		};
	}

	public override void GetStormLifeSpan(out CampaignTime minimumDuration, out CampaignTime maximumDuration)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		minimumDuration = CampaignTime.Days(10f);
		maximumDuration = minimumDuration + CampaignTime.Days(11f);
	}

	public override float GetHourlyStormSpawnChanceForPosition(Vec2 position)
	{
		return 0.0025f;
	}

	public override Storm.StormTypes GetSpawnedStormTypeForPosition(Vec2 position)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		float num2 = default(float);
		Campaign.Current.Models.MapWeatherModel.GetSnowAndRainDataForPosition(position, CampaignTime.Now, ref num, ref num2);
		if (num2 <= 0.2f)
		{
			return Storm.StormTypes.Storm;
		}
		if (num2 <= 0.6f)
		{
			return Storm.StormTypes.ThunderStorm;
		}
		return Storm.StormTypes.Hurricane;
	}

	public override bool CanPartyGetDamagedByStorm(MobileParty mobileParty)
	{
		if (mobileParty.CurrentSettlement == null && mobileParty.IsCurrentlyAtSea)
		{
			return mobileParty.MapEvent == null;
		}
		return false;
	}

	public override float GetEffectRadiusOfStorm(Storm storm)
	{
		float num = 0f;
		switch (storm.StormType)
		{
		case Storm.StormTypes.Storm:
			num = 20f;
			break;
		case Storm.StormTypes.ThunderStorm:
			num = 30f;
			break;
		case Storm.StormTypes.Hurricane:
			num = 40f;
			break;
		}
		float num2 = MBMath.Map(storm.Intensity, 0f, 1f, -0.2f, 0.2f);
		return num + num * num2;
	}

	public override float GetEyeRadiusOfStorm(Storm storm)
	{
		return storm.StormType switch
		{
			Storm.StormTypes.Storm => 0f, 
			Storm.StormTypes.ThunderStorm => 0f, 
			Storm.StormTypes.Hurricane => storm.EffectRadius * 0.2f, 
			_ => 0f, 
		};
	}

	public override float GetSpeedOfStorm(Storm storm)
	{
		return storm.StormType switch
		{
			Storm.StormTypes.Storm => 3f, 
			Storm.StormTypes.ThunderStorm => 2f, 
			Storm.StormTypes.Hurricane => 1f, 
			_ => 0f, 
		};
	}

	public override CampaignTime GetDevelopingStateDurationOfStorm(Storm storm)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return CampaignTime.Hours(8f);
	}

	public override CampaignTime GetFinalizingStateDurationOfStorm(Storm storm)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return CampaignTime.Hours(8f);
	}

	public override float GetStormSpawnDistanceSquaredThresholdWithOtherStorms()
	{
		return 40000f;
	}

	public override float GetNormalizedWindStrengthOfStormForPosition(Vec2 position)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		Storm storm = null;
		Vec2 currentPosition;
		foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
		{
			if (item.IsActive)
			{
				currentPosition = item.CurrentPosition;
				if (((Vec2)(ref currentPosition)).Distance(position) < item.EffectRadius)
				{
					storm = item;
					break;
				}
			}
		}
		if (storm != null)
		{
			float effectRadius = storm.EffectRadius;
			currentPosition = storm.CurrentPosition;
			float num = effectRadius - ((Vec2)(ref currentPosition)).Distance(position);
			float effectRadius2 = storm.EffectRadius;
			return num / effectRadius2 * storm.Intensity;
		}
		return 0f;
	}
}
