using System.Collections.Generic;
using NavalDLC.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.GameComponents;

public class NavalDLCMapWeatherModel : MapWeatherModel
{
	private const float MaximumWindSpeed = 30f;

	private const float MinWindWithStormOnCampaignMap = 0.1f;

	private const float MaxWindWithStormOnCampaignMap = 1f;

	private const float MinWindWithoutStormOnCampaignMap = 1f / 15f;

	private const float MaxWindWithoutStormOnCampaignMap = 0.46f;

	private const float MinWindSpeedRatioWithStormOnMission = 2f / 3f;

	private const float MaxWindSpeedRatioWithStormOnMission = 1f;

	private const float MinWindSpeedRatioWithoutStormOnMission = 0.4f;

	private const float MaxWindSpeedRatioWithoutStormOnMission = 8f / 15f;

	public override CampaignTime WeatherUpdateFrequency => ((MBGameModel<MapWeatherModel>)this).BaseModel.WeatherUpdateFrequency;

	public override CampaignTime WeatherUpdatePeriod => ((MBGameModel<MapWeatherModel>)this).BaseModel.WeatherUpdatePeriod;

	public override AtmosphereInfo GetAtmosphereModel(CampaignVec2 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		AtmosphereInfo atmosphereModel = ((MBGameModel<MapWeatherModel>)this).BaseModel.GetAtmosphereModel(position);
		if (!position.IsOnLand)
		{
			atmosphereModel.NauticalInfo.UseSceneWindDirection = 0;
			atmosphereModel.NauticalInfo.CanUseLowAltitudeAtmosphere = 1;
			atmosphereModel.NauticalInfo.IsInsideStorm = (IsPositionInsideStormForMission(position) ? 1 : 0);
			float num = 0f;
			num = ((atmosphereModel.NauticalInfo.IsInsideStorm != 1) ? (((Vec2)(ref atmosphereModel.NauticalInfo.WindVector)).Length * 0.13333336f / (59f / 150f) + 0.4f) : ((((Vec2)(ref atmosphereModel.NauticalInfo.WindVector)).Length - 0.1f) * 0.3333333f / 0.9f + 2f / 3f));
			atmosphereModel.NauticalInfo.WindVector = ((Vec2)(ref atmosphereModel.NauticalInfo.WindVector)).Normalized() * num;
		}
		else
		{
			ref NauticalInformation nauticalInfo = ref atmosphereModel.NauticalInfo;
			Vec2 windForPosition = Campaign.Current.Models.MapWeatherModel.GetWindForPosition(position);
			nauticalInfo.WindVector = ((Vec2)(ref windForPosition)).Normalized() * 0.26f;
		}
		return atmosphereModel;
	}

	public override AtmosphereState GetInterpolatedAtmosphereState(CampaignTime timeOfYear, Vec3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<MapWeatherModel>)this).BaseModel.GetInterpolatedAtmosphereState(timeOfYear, pos);
	}

	public override void GetSeasonTimeFactorOfCampaignTime(CampaignTime ct, out float timeFactorForSnow, out float timeFactorForRain, bool snapCampaignTimeToWeatherPeriod = true)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((MBGameModel<MapWeatherModel>)this).BaseModel.GetSeasonTimeFactorOfCampaignTime(ct, ref timeFactorForSnow, ref timeFactorForRain, snapCampaignTimeToWeatherPeriod);
	}

	public override void GetSnowAndRainDataForPosition(Vec2 position, CampaignTime ct, out float snowValue, out float rainValue)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		((MBGameModel<MapWeatherModel>)this).BaseModel.GetSnowAndRainDataForPosition(position, ct, ref snowValue, ref rainValue);
		foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
		{
			Vec2 currentPosition = item.CurrentPosition;
			if (((Vec2)(ref currentPosition)).DistanceSquared(position) < item.EffectRadius * item.EffectRadius)
			{
				rainValue = 1f;
				break;
			}
		}
	}

	public override WeatherEventEffectOnTerrain GetWeatherEffectOnTerrainForPosition(Vec2 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		WeatherEventEffectOnTerrain result = ((MBGameModel<MapWeatherModel>)this).BaseModel.GetWeatherEffectOnTerrainForPosition(pos);
		foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
		{
			if (item.HasWetWeatherEffectAtPosition(pos))
			{
				result = (WeatherEventEffectOnTerrain)1;
				break;
			}
		}
		return result;
	}

	public override void InitializeCaches()
	{
		((MBGameModel<MapWeatherModel>)this).BaseModel.InitializeCaches();
	}

	public override WeatherEvent GetWeatherEventInPosition(Vec2 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		WeatherEvent weatherEventInPosition = ((MBGameModel<MapWeatherModel>)this).BaseModel.GetWeatherEventInPosition(pos);
		if ((int)weatherEventInPosition == 0)
		{
			Storm storm = null;
			foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
			{
				if (item.IsActive)
				{
					Vec2 currentPosition = item.CurrentPosition;
					if (((Vec2)(ref currentPosition)).Distance(pos) < item.EffectRadius * 1.25f)
					{
						storm = item;
						break;
					}
				}
			}
			if (storm != null)
			{
				return (WeatherEvent)5;
			}
		}
		return weatherEventInPosition;
	}

	public override Vec2 GetWindForPosition(CampaignVec2 position)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Vec2 windAtPosition = NavalDLCManager.Instance.NavalMapSceneWrapper.GetWindAtPosition(((CampaignVec2)(ref position)).ToVec2());
		float num = MathF.Clamp(((Vec2)(ref windAtPosition)).Length, 1f / 15f, 0.46f);
		((Vec2)(ref windAtPosition)).Normalize();
		float normalizedWindStrengthOfStormForPosition = NavalDLCManager.Instance.GameModels.MapStormModel.GetNormalizedWindStrengthOfStormForPosition(((CampaignVec2)(ref position)).ToVec2());
		float num2 = 0f;
		if (normalizedWindStrengthOfStormForPosition > 0f)
		{
			num2 = MBMath.Map(normalizedWindStrengthOfStormForPosition, 0f, 1f, 0.1f, 0.6f);
		}
		return windAtPosition * MathF.Clamp(num + num2, 1E-05f, 1f);
	}

	public override WeatherEvent UpdateWeatherForPosition(CampaignVec2 position, CampaignTime ct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!position.IsOnLand)
		{
			return (WeatherEvent)0;
		}
		return ((MBGameModel<MapWeatherModel>)this).BaseModel.UpdateWeatherForPosition(position, ct);
	}

	private bool IsPositionInsideStormForMission(CampaignVec2 position)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		Storm storm = null;
		Vec2 currentPosition;
		foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
		{
			if (item.IsActive)
			{
				currentPosition = item.CurrentPosition;
				if (((Vec2)(ref currentPosition)).DistanceSquared(((CampaignVec2)(ref position)).ToVec2()) < item.EffectRadius * item.EffectRadius)
				{
					storm = item;
					break;
				}
			}
		}
		if (storm != null)
		{
			currentPosition = storm.CurrentPosition;
			float num = ((Vec2)(ref currentPosition)).DistanceSquared(((CampaignVec2)(ref position)).ToVec2());
			float num2 = storm.EffectRadius * storm.EffectRadius;
			float num3 = num / num2;
			float num4 = 0.64000005f;
			return num3 <= num4;
		}
		return false;
	}
}
