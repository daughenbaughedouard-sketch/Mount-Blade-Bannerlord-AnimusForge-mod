using System;
using System.Collections.Generic;
using System.Diagnostics;
using NavalDLC.ComponentInterfaces;
using NavalDLC.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.CampaignBehaviors;

public class StormCampaignBehavior : CampaignBehaviorBase
{
	private List<Vec2> _allOpenSeaWeatherNodePositions;

	private float _spawnDistanceSquaredThreshold;

	private int[] _weatherNodePositionsShuffledIndices;

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunchedEvent);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, (Action)HourlyTick);
	}

	private void HourlyTick()
	{
		foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
		{
			if (item.IsActive)
			{
				if (MBRandom.RandomFloat < 0.5f)
				{
					item.ChangeMoveDirection();
				}
				float hourlyIntensityChangeForStorm = NavalDLCManager.Instance.GameModels.MapStormModel.GetHourlyIntensityChangeForStorm(item);
				item.Intensity += hourlyIntensityChangeForStorm;
				DamageNearbyParties(item);
			}
		}
		TrySpawningNewStorm();
	}

	private void DamageNearbyParties(Storm spawnedStorm)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		LocatableSearchData<MobileParty> val = MobileParty.StartFindingLocatablesAroundPosition(spawnedStorm.CurrentPosition, spawnedStorm.EffectRadius);
		for (MobileParty val2 = MobileParty.FindNextLocatable(ref val); val2 != null; val2 = MobileParty.FindNextLocatable(ref val))
		{
			if (val2.AttachedTo == null)
			{
				TryDamagingParty(val2, spawnedStorm);
			}
		}
	}

	private void TryDamagingParty(MobileParty mobileParty, Storm affectingStorm)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (NavalDLCManager.Instance.GameModels.MapStormModel.CanPartyGetDamagedByStorm(mobileParty))
		{
			float num2 = default(float);
			for (int num = ((List<Ship>)(object)mobileParty.Ships).Count - 1; num >= 0; num--)
			{
				Ship val = ((List<Ship>)(object)mobileParty.Ships)[num];
				MapStormModel mapStormModel = NavalDLCManager.Instance.GameModels.MapStormModel;
				CampaignVec2 position = mobileParty.Position;
				float positionDamageForStorm = mapStormModel.GetPositionDamageForStorm(affectingStorm, ((CampaignVec2)(ref position)).ToVec2(), val);
				val.OnShipDamaged(positionDamageForStorm, (IShipOrigin)null, ref num2);
				_ = NavalDLCManager.Instance.StormManager.DebugVisualsEnabled;
			}
		}
		foreach (MobileParty item in (List<MobileParty>)(object)mobileParty.AttachedParties)
		{
			TryDamagingParty(item, affectingStorm);
		}
	}

	private void TrySpawningNewStorm()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		int[] weatherNodePositionsShuffledIndices = _weatherNodePositionsShuffledIndices;
		foreach (int index in weatherNodePositionsShuffledIndices)
		{
			Vec2 val = _allOpenSeaWeatherNodePositions[index];
			int count = ((List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms).Count;
			if (!(NavalDLCManager.Instance.GameModels.MapStormModel.GetHourlyStormSpawnChanceForPosition(val) > MBRandom.RandomFloat) || count >= NavalDLCManager.Instance.GameModels.MapStormModel.MaximumNumberOfStorms)
			{
				continue;
			}
			bool flag = false;
			foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
			{
				Vec2 currentPosition = item.CurrentPosition;
				if (((Vec2)(ref currentPosition)).DistanceSquared(val) < _spawnDistanceSquaredThreshold)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				NavalDLCManager.Instance.StormManager.CreateStormAtPosition(val);
			}
		}
	}

	private void CreateAndShuffleWeatherNodeDataIndicesDeterministic()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		int count = _allOpenSeaWeatherNodePositions.Count;
		_weatherNodePositionsShuffledIndices = new int[count];
		for (int i = 0; i < count; i++)
		{
			_weatherNodePositionsShuffledIndices[i] = i;
		}
		MBFastRandom val = new MBFastRandom((uint)Extensions.GetDeterministicHashCode(Campaign.Current.UniqueGameId));
		for (int j = 0; j < 20; j++)
		{
			for (int k = 0; k < count; k++)
			{
				int num = val.Next(count);
				int num2 = _weatherNodePositionsShuffledIndices[k];
				_weatherNodePositionsShuffledIndices[k] = _weatherNodePositionsShuffledIndices[num];
				_weatherNodePositionsShuffledIndices[num] = num2;
			}
		}
	}

	private void OnSessionLaunchedEvent(CampaignGameStarter obj)
	{
		_spawnDistanceSquaredThreshold = NavalDLCManager.Instance.GameModels.MapStormModel.GetStormSpawnDistanceSquaredThresholdWithOtherStorms();
		InitializeStormNodes();
	}

	private void InitializeStormNodes()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Invalid comparison between Unknown and I4
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		_allOpenSeaWeatherNodePositions = new List<Vec2>();
		Vec2 terrainSize = Campaign.Current.MapSceneWrapper.GetTerrainSize();
		int defaultWeatherNodeDimension = Campaign.Current.DefaultWeatherNodeDimension;
		int num = defaultWeatherNodeDimension;
		int num2 = defaultWeatherNodeDimension;
		Vec2 val = default(Vec2);
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num3 = (float)i / (float)defaultWeatherNodeDimension * ((Vec2)(ref terrainSize)).X;
				float num4 = (float)j / (float)defaultWeatherNodeDimension * ((Vec2)(ref terrainSize)).Y;
				((Vec2)(ref val))._002Ector(num3, num4);
				IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
				CampaignVec2 val2 = new CampaignVec2(val, false);
				PathFaceRecord faceIndex = mapSceneWrapper.GetFaceIndex(ref val2);
				if (((PathFaceRecord)(ref faceIndex)).IsValid())
				{
					IMapScene mapSceneWrapper2 = Campaign.Current.MapSceneWrapper;
					val2 = new CampaignVec2(val, false);
					if ((int)mapSceneWrapper2.GetTerrainTypeAtPosition(ref val2) == 19)
					{
						_allOpenSeaWeatherNodePositions.Add(val);
					}
				}
			}
		}
		CreateAndShuffleWeatherNodeDataIndicesDeterministic();
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	[Conditional("DEBUG")]
	private void MainPartyStormDamageDebugVisualTick(Storm storm, MobileParty nearbyParty, Ship ship, float damage)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (nearbyParty.IsMainParty)
		{
			MobileParty mainParty = MobileParty.MainParty;
			float maximumWeatherStrengthAtEye = NavalDLCManager.Instance.GameModels.MapStormModel.GetMaximumWeatherStrengthAtEye(storm);
			Vec2 currentPosition = storm.CurrentPosition;
			CampaignVec2 position = mainParty.Position;
			float num = ((Vec2)(ref currentPosition)).Distance(((CampaignVec2)(ref position)).ToVec2());
			_ = NavalDLCManager.Instance.GameModels.MapStormModel.MinimumWeatherStrengthInsideStorm;
			if (!(num < storm.EyeRadius) && num + storm.EyeRadius < storm.EffectRadius)
			{
				MBMath.Map(num, 0f, storm.EffectRadius, maximumWeatherStrengthAtEye, NavalDLCManager.Instance.GameModels.MapStormModel.MinimumWeatherStrengthInsideStorm);
			}
			Campaign.Current.Models.CampaignShipParametersModel.GetShipSizeWeatherFactor(ship.ShipHull);
		}
	}
}
