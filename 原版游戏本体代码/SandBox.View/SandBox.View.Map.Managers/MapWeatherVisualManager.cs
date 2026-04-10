using System;
using System.Collections.Generic;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.View.Map.Managers;

public class MapWeatherVisualManager : EntityVisualManagerBase<WeatherNode>
{
	public const int DefaultCloudHeight = 26;

	public const int OpenSeaStormCloudHeight = 20;

	private MapWeatherVisual[] _allWeatherNodeVisuals;

	private const string RainPrefabName = "campaign_rain_prefab";

	private const string BlizzardPrefabName = "campaign_snow_prefab";

	private const string RainSoundPath = "event:/map/ambient/bed/rain";

	private const string SnowSoundPath = "event:/map/ambient/bed/snow";

	private const string WeatherEventParameterName = "Rainfall";

	private const string CameraRainPrefabName = "map_camera_rain_prefab";

	private const string CameraStormPrefabName = "map_camera_storm_prefab";

	private const int DefaultRainObjectPoolCount = 5;

	private const int DefaultBlizzardObjectPoolCount = 5;

	private const int WeatherCheckOriginZDelta = 25;

	private readonly List<GameEntity> _unusedRainPrefabEntityPool;

	private readonly List<GameEntity> _unusedBlizzardPrefabEntityPool;

	private readonly Scene _mapScene;

	private readonly byte[] _rainData = new byte[Campaign.Current.DefaultWeatherNodeDimension * Campaign.Current.DefaultWeatherNodeDimension * 2];

	private readonly byte[] _rainDataTemporal = new byte[Campaign.Current.DefaultWeatherNodeDimension * Campaign.Current.DefaultWeatherNodeDimension * 2];

	private SoundEvent _currentRainSound;

	private SoundEvent _currentBlizzardSound;

	private GameEntity _cameraRainEffect;

	private GameEntity _cameraStormEffect;

	public static MapWeatherVisualManager Current => SandBoxViewSubModule.SandBoxViewVisualManager.GetEntityComponent<MapWeatherVisualManager>();

	public override int Priority => 60;

	private int DimensionSquared => Campaign.Current.DefaultWeatherNodeDimension * Campaign.Current.DefaultWeatherNodeDimension;

	public MapWeatherVisualManager()
	{
		_unusedRainPrefabEntityPool = new List<GameEntity>();
		_unusedBlizzardPrefabEntityPool = new List<GameEntity>();
		for (int i = 0; i < DimensionSquared * 2; i++)
		{
			_rainData[i] = 0;
			_rainDataTemporal[i] = 0;
		}
		_allWeatherNodeVisuals = new MapWeatherVisual[DimensionSquared];
		_mapScene = ((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene;
		WeatherNode[] allWeatherNodes = Campaign.Current.GetCampaignBehavior<MapWeatherCampaignBehavior>().AllWeatherNodes;
		for (int j = 0; j < allWeatherNodes.Length; j++)
		{
			_allWeatherNodeVisuals[j] = new MapWeatherVisual(allWeatherNodes[j]);
		}
	}

	public override void OnVisualTick(MapScreen screen, float realDt, float dt)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		for (int i = 0; i < _allWeatherNodeVisuals.Length; i++)
		{
			_allWeatherNodeVisuals[i].Tick();
		}
		TWParallel.For(0, DimensionSquared, (ParallelForAuxPredicate)delegate(int startInclusive, int endExclusive)
		{
			for (int j = startInclusive; j < endExclusive; j++)
			{
				int num = j * 2;
				_rainDataTemporal[num] = (byte)MBMath.Lerp((float)(int)_rainDataTemporal[num], (float)(int)_rainData[num], 1f - (float)Math.Exp(-1.8f * (realDt + dt)), 1E-05f);
				_rainDataTemporal[num + 1] = (byte)MBMath.Lerp((float)(int)_rainDataTemporal[num + 1], (float)(int)_rainData[num + 1], 1f - (float)Math.Exp(-1.8f * (realDt + dt)), 1E-05f);
			}
		}, 16);
		_mapScene.SetLandscapeRainMaskData(_rainDataTemporal);
		WeatherAudioAndVisualTick();
	}

	public void SetRainData(int dataIndex, byte value)
	{
		_rainData[dataIndex * 2] = value;
	}

	public void SetCloudData(int dataIndex, byte value)
	{
		_rainData[dataIndex * 2 + 1] = value;
	}

	private void WeatherAudioAndVisualTick()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Invalid comparison between Unknown and I4
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Invalid comparison between Unknown and I4
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Invalid comparison between Unknown and I4
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Invalid comparison between Unknown and I4
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Invalid comparison between Unknown and I4
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		SoundManager.SetGlobalParameter("Rainfall", 0.5f);
		float num = 0f;
		int num2 = 26;
		MatrixFrame lastFinalRenderCameraFrame = _mapScene.LastFinalRenderCameraFrame;
		Vec2 asVec = ((Vec3)(ref lastFinalRenderCameraFrame.origin)).AsVec2;
		IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
		CampaignVec2 val = new CampaignVec2(asVec, true);
		mapSceneWrapper.GetHeightAtPoint(ref val, ref num);
		Vec2 terrainSize = Campaign.Current.MapSceneWrapper.GetTerrainSize();
		float num3 = MBMath.ClampFloat(asVec.x, 0f, terrainSize.x);
		float num4 = MBMath.ClampFloat(asVec.y, 0f, terrainSize.y);
		Vec2 val2 = default(Vec2);
		((Vec2)(ref val2))._002Ector(num3, num4);
		WeatherEvent val3 = Campaign.Current.Models.MapWeatherModel.GetWeatherEventInPosition(val2);
		GameEntity val4 = _cameraRainEffect;
		if ((int)val3 == 5)
		{
			val3 = (WeatherEvent)2;
			val4 = _cameraStormEffect;
			num2 = 20;
		}
		if ((int)val3 == 2 || (int)val3 == 4)
		{
			if ((int)val3 == 2)
			{
				if (((Vec3)(ref lastFinalRenderCameraFrame.origin)).Z < (float)num2 * 2.5f)
				{
					val4.SetVisibilityExcludeParents(true);
					MatrixFrame val5 = ((MatrixFrame)(ref lastFinalRenderCameraFrame)).Elevate(-5f);
					val4.SetFrame(ref val5, true);
				}
				else
				{
					val4.SetVisibilityExcludeParents(false);
				}
				DestroyBlizzardSound();
				StartRainSoundIfNeeded();
				MBMapScene.ApplyRainColorGrade = true;
			}
			else if ((int)val3 == 4)
			{
				DestroyRainSound();
				StartBlizzardSoundIfNeeded();
				val4.SetVisibilityExcludeParents(false);
				MBMapScene.ApplyRainColorGrade = false;
			}
		}
		else
		{
			DestroyBlizzardSound();
			DestroyRainSound();
			_cameraRainEffect.SetVisibilityExcludeParents(false);
			_cameraStormEffect.SetVisibilityExcludeParents(false);
			MBMapScene.ApplyRainColorGrade = false;
		}
	}

	private void DestroyRainSound()
	{
		if (_currentRainSound != null)
		{
			_currentRainSound.Stop();
			_currentRainSound = null;
		}
	}

	private void DestroyBlizzardSound()
	{
		if (_currentBlizzardSound != null)
		{
			_currentBlizzardSound.Stop();
			_currentBlizzardSound = null;
		}
	}

	private void StartRainSoundIfNeeded()
	{
		if (_currentRainSound == null)
		{
			_currentRainSound = SoundManager.CreateEvent("event:/map/ambient/bed/rain", _mapScene);
			_currentRainSound.Play();
		}
	}

	private void StartBlizzardSoundIfNeeded()
	{
		if (_currentBlizzardSound == null)
		{
			_currentBlizzardSound = SoundManager.CreateEvent("event:/map/ambient/bed/snow", _mapScene);
			_currentBlizzardSound.Play();
		}
	}

	protected override void OnInitialize()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		base.OnInitialize();
		InitializeObjectPoolWithDefaultCount();
		_cameraRainEffect = GameEntity.Instantiate(_mapScene, "map_camera_rain_prefab", MatrixFrame.Identity, true, "");
		_cameraStormEffect = GameEntity.Instantiate(_mapScene, "map_camera_storm_prefab", MatrixFrame.Identity, true, "");
	}

	public GameEntity GetRainPrefabFromPool()
	{
		if (Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_unusedRainPrefabEntityPool))
		{
			_unusedRainPrefabEntityPool.AddRange(CreateNewWeatherPrefabPoolElements("campaign_rain_prefab", 5));
		}
		GameEntity val = _unusedRainPrefabEntityPool[0];
		_unusedRainPrefabEntityPool.Remove(val);
		return val;
	}

	public GameEntity GetBlizzardPrefabFromPool()
	{
		if (Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_unusedBlizzardPrefabEntityPool))
		{
			_unusedBlizzardPrefabEntityPool.AddRange(CreateNewWeatherPrefabPoolElements("campaign_snow_prefab", 5));
		}
		GameEntity val = _unusedBlizzardPrefabEntityPool[0];
		_unusedBlizzardPrefabEntityPool.Remove(val);
		return val;
	}

	public void ReleaseRainPrefab(GameEntity prefab)
	{
		_unusedRainPrefabEntityPool.Add(prefab);
		prefab.SetVisibilityExcludeParents(false);
	}

	public void ReleaseBlizzardPrefab(GameEntity prefab)
	{
		_unusedBlizzardPrefabEntityPool.Add(prefab);
		prefab.SetVisibilityExcludeParents(false);
	}

	private void InitializeObjectPoolWithDefaultCount()
	{
		_unusedRainPrefabEntityPool.AddRange(CreateNewWeatherPrefabPoolElements("campaign_rain_prefab", 5));
		_unusedBlizzardPrefabEntityPool.AddRange(CreateNewWeatherPrefabPoolElements("campaign_snow_prefab", 5));
	}

	private List<GameEntity> CreateNewWeatherPrefabPoolElements(string prefabName, int delta)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		List<GameEntity> list = new List<GameEntity>();
		for (int i = 0; i < delta; i++)
		{
			GameEntity val = GameEntity.Instantiate(_mapScene, prefabName, MatrixFrame.Identity, true, "");
			val.SetVisibilityExcludeParents(false);
			list.Add(val);
		}
		return list;
	}

	public override MapEntityVisual<WeatherNode> GetVisualOfEntity(WeatherNode entity)
	{
		return null;
	}
}
