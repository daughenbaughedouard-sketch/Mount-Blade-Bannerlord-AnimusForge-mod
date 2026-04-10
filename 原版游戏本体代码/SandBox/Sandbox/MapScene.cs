using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public class MapScene : IMapScene
{
	private int _snowAndRainDataTextureWidth;

	private int _snowAndRainDataTextureHeight;

	public const int FlowMapTextureDimension = 512;

	private const string MapCampArea1Tag = "map_camp_area_1";

	private const string MapCampArea2Tag = "map_camp_area_2";

	private Scene _scene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private byte[] _snowAndRainData;

	private float[] _windFlowMapData;

	private Vec2 _minimumPositionCache;

	private Vec2 _maximumPositionCache;

	private float _maximumHeightCache;

	private Dictionary<string, uint> _sceneLevels;

	private int _battleTerrainIndexMapWidth;

	private int _battleTerrainIndexMapHeight;

	private byte[] _battleTerrainIndexMap;

	private Vec2 _terrainSize;

	private ReaderWriterLockSlim _sharedLock;

	public Scene Scene => _scene;

	public MapScene()
	{
		_sharedLock = new ReaderWriterLockSlim();
		_sceneLevels = new Dictionary<string, uint>();
	}

	public Vec2 GetTerrainSize()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _terrainSize;
	}

	public uint GetSceneLevel(string name)
	{
		_sharedLock.EnterReadLock();
		uint value;
		bool num = _sceneLevels.TryGetValue(name, out value) && value != int.MaxValue;
		_sharedLock.ExitReadLock();
		if (num)
		{
			return value;
		}
		uint upgradeLevelMaskOfLevelName = _scene.GetUpgradeLevelMaskOfLevelName(name);
		_sharedLock.EnterWriteLock();
		_sceneLevels[name] = upgradeLevelMaskOfLevelName;
		_sharedLock.ExitWriteLock();
		return upgradeLevelMaskOfLevelName;
	}

	public void SetSceneLevels(List<string> levels)
	{
		foreach (string level in levels)
		{
			_sceneLevels.Add(level, 2147483647u);
		}
	}

	public List<AtmosphereState> GetAtmosphereStates()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		List<AtmosphereState> list = new List<AtmosphereState>();
		foreach (GameEntity item2 in Scene.FindEntitiesWithTag("atmosphere_probe"))
		{
			MapAtmosphereProbe firstScriptOfType = item2.GetFirstScriptOfType<MapAtmosphereProbe>();
			Vec3 globalPosition = item2.GlobalPosition;
			AtmosphereState item = new AtmosphereState
			{
				Position = globalPosition,
				HumidityAverage = firstScriptOfType.rainDensity,
				HumidityVariance = 5f,
				TemperatureAverage = firstScriptOfType.temperature,
				TemperatureVariance = 5f,
				distanceForMaxWeight = firstScriptOfType.minRadius,
				distanceForMinWeight = firstScriptOfType.maxRadius,
				ColorGradeTexture = firstScriptOfType.colorGrade
			};
			list.Add(item);
		}
		return list;
	}

	public void ValidateAgentVisualsReseted()
	{
		if ((NativeObject)(object)_scene != (NativeObject)null && _agentRendererSceneController != null)
		{
			MBAgentRendererSceneController.ValidateAgentVisualsReseted(_scene, _agentRendererSceneController);
		}
	}

	public void SetAtmosphereColorgrade(TerrainType terrainType)
	{
	}

	public void AddNewEntityToMapScene(string entityId, in CampaignVec2 position)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = GameEntity.Instantiate(_scene, entityId, true, true, "");
		if (val != (GameEntity)null)
		{
			CampaignVec2 val2 = position;
			val.SetLocalPosition(((CampaignVec2)(ref val2)).AsVec3());
		}
	}

	public void GetMapBorders(out Vec2 minimumPosition, out Vec2 maximumPosition, out float maximumHeight)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		minimumPosition = _minimumPositionCache;
		maximumPosition = _maximumPositionCache;
		maximumHeight = _maximumHeightCache;
	}

	public void Load()
	{
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Expected O, but got Unknown
		//IL_02ad: Expected O, but got Unknown
		Debug.Print("Creating map scene", 0, (DebugColor)12, 17592186044416uL);
		_scene = Scene.CreateNewScene(false, true, (DecalAtlasGroup)1, "MapScene");
		_scene.SetClothSimulationState(true);
		_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_scene);
		_agentRendererSceneController.SetDoTimerBasedForcedSkeletonUpdates(false);
		_scene.SetOcclusionMode(true);
		SceneInitializationData val = default(SceneInitializationData);
		((SceneInitializationData)(ref val))._002Ector(true);
		val.UsePhysicsMaterials = false;
		val.EnableFloraPhysics = false;
		val.UseTerrainMeshBlending = false;
		val.CreateOros = false;
		_scene.SetFetchCrcInfoOfScene(true);
		DisableUnwalkableNavigationMeshes();
		bool[] regionMapping = SandBoxHelpers.MapSceneHelper.GetRegionMapping(Campaign.Current.Models.PartyNavigationModel);
		_scene.SetNavMeshRegionMap(regionMapping);
		ModuleInfo mainMapModule = GetMainMapModule();
		if (mainMapModule != null)
		{
			_scene.Read("Main_map", mainMapModule.Id, ref val, "");
		}
		else
		{
			_scene.Read("Main_map", ref val, "");
		}
		Utilities.SetAllocationAlwaysValidScene(_scene);
		GameEntity firstEntityWithName = _scene.GetFirstEntityWithName("border_min");
		GameEntity firstEntityWithName2 = _scene.GetFirstEntityWithName("border_max");
		MatrixFrame globalFrame = firstEntityWithName.GetGlobalFrame();
		_minimumPositionCache = ((Vec3)(ref globalFrame.origin)).AsVec2;
		globalFrame = firstEntityWithName2.GetGlobalFrame();
		_maximumPositionCache = ((Vec3)(ref globalFrame.origin)).AsVec2;
		_maximumHeightCache = firstEntityWithName2.GetGlobalFrame().origin.z;
		_scene.DisableStaticShadows(true);
		_scene.InvalidateTerrainPhysicsMaterials();
		_scene.SetDontLoadInvisibleEntities(true);
		GameEntity firstEntityWithName3 = _scene.GetFirstEntityWithName("medit_water_plane");
		if (firstEntityWithName3 != (GameEntity)null)
		{
			firstEntityWithName3.SetDoNotCheckVisibility(true);
		}
		if (_scene.GetFirstEntityWithScriptComponent("Town Scene Manager") == (GameEntity)null)
		{
			MBDebug.ShowWarning("Main map scene must have an entity with 'Town Scene Manager' script for mesh memory optimization.");
		}
		LoadAtmosphereData(_scene);
		Campaign.Current.Models.MapWeatherModel.InitializeCaches();
		MBMapScene.ValidateTerrainSoundIds();
		_scene.OptimizeScene(true, false);
		Vec2i val2 = default(Vec2i);
		float num = default(float);
		int num2 = default(int);
		int num3 = default(int);
		_scene.GetTerrainData(ref val2, ref num, ref num2, ref num3);
		_terrainSize.x = (float)val2.X * num;
		_terrainSize.y = (float)val2.Y * num;
		MBMapScene.GetBattleSceneIndexMap(_scene, ref _battleTerrainIndexMap, ref _battleTerrainIndexMapWidth, ref _battleTerrainIndexMapHeight);
		Debug.Print("Ticking map scene for first initialization", 0, (DebugColor)12, 17592186044416uL);
		_scene.Tick(0.1f);
		AsyncTask campaignLateAITickTask = AsyncTask.CreateWithDelegate(new ManagedDelegate
		{
			Instance = new DelegateDefinition(Campaign.LateAITick)
		}, false);
		Campaign.Current.CampaignLateAITickTask = (ITask)(object)campaignLateAITickTask;
	}

	public void SetSnowAndRainDataWithDimension(Texture snowRainTexture, int weatherNodeGridWidthAndHeight)
	{
		_scene.CreateDynamicRainTexture(weatherNodeGridWidthAndHeight, weatherNodeGridWidthAndHeight);
		_snowAndRainDataTextureWidth = snowRainTexture.Width;
		_snowAndRainDataTextureHeight = snowRainTexture.Height;
		_snowAndRainData = new byte[_snowAndRainDataTextureWidth * _snowAndRainDataTextureHeight * 2];
		snowRainTexture.GetPixelData(_snowAndRainData);
		_scene.SetDynamicSnowTexture(snowRainTexture);
		Campaign.Current.DefaultWeatherNodeDimension = weatherNodeGridWidthAndHeight;
		_windFlowMapData = new float[524288];
		_scene.GetWindFlowMapData(_windFlowMapData);
	}

	public void AfterLoad()
	{
	}

	private ModuleInfo GetMainMapModule()
	{
		ModuleInfo result = null;
		foreach (ModuleInfo activeModule in ModuleHelper.GetActiveModules())
		{
			if (activeModule.IsActive && File.Exists(Path.Combine(activeModule.FolderPath, "SceneObj", "Main_map", "scene.xscene")))
			{
				result = activeModule;
			}
		}
		return result;
	}

	public void Destroy()
	{
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_scene, _agentRendererSceneController, false);
		_agentRendererSceneController = null;
	}

	public void DisableUnwalkableNavigationMeshes()
	{
		int[] invalidTerrainTypesForNavigationType = Campaign.Current.Models.PartyNavigationModel.GetInvalidTerrainTypesForNavigationType((NavigationType)3);
		foreach (int num in invalidTerrainTypesForNavigationType)
		{
			Scene.SetAbilityOfFacesWithId(num, false);
		}
	}

	public PathFaceRecord GetFaceIndex(in CampaignVec2 vec2)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		PathFaceRecord result = default(PathFaceRecord);
		((PathFaceRecord)(ref result))._002Ector(-1, -1, -1);
		Scene scene = _scene;
		CampaignVec2 val = vec2;
		scene.GetNavMeshFaceIndex(ref result, ((CampaignVec2)(ref val)).ToVec2(), vec2.IsOnLand, false, true);
		return result;
	}

	private void LoadAtmosphereData(Scene mapScene)
	{
		MBMapScene.LoadAtmosphereData(mapScene);
	}

	public TerrainType GetTerrainTypeAtPosition(in CampaignVec2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 val = position;
		PathFaceRecord face = ((CampaignVec2)(ref val)).Face;
		return GetFaceTerrainType(face);
	}

	public TerrainType GetFaceTerrainType(PathFaceRecord navMeshFace)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!((PathFaceRecord)(ref navMeshFace)).IsValid())
		{
			Debug.FailedAssert("Null nav mesh face tried to get terrain type.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\MapScene.cs", "GetFaceTerrainType", 338);
			return (TerrainType)1;
		}
		return (TerrainType)navMeshFace.FaceGroupIndex;
	}

	public CampaignVec2 GetNearestFaceCenterForPosition(in CampaignVec2 position, int[] excludedFaceIds)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = _scene;
		CampaignVec2 val = position;
		return new CampaignVec2(MBMapScene.GetNearestFaceCenterForPosition(scene, ((CampaignVec2)(ref val)).ToVec2(), position.IsOnLand, excludedFaceIds), position.IsOnLand);
	}

	public CampaignVec2 GetNearestFaceCenterForPositionWithPath(PathFaceRecord pathFaceRecord, bool targetIsLand, float maxDist, int[] excludedFaceIds)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return new CampaignVec2(MBMapScene.GetNearestFaceCenterForPositionWithPath(_scene, pathFaceRecord, targetIsLand, maxDist, excludedFaceIds), targetIsLand);
	}

	public List<TerrainType> GetEnvironmentTerrainTypes(in CampaignVec2 originPosition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		List<TerrainType> list = new List<TerrainType>();
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(1f, 0f);
		CampaignVec2 position = originPosition;
		list.Add(GetTerrainTypeAtPosition(in position));
		for (int i = 0; i < 8; i++)
		{
			((Vec2)(ref val)).RotateCCW(MathF.PI / 4f * (float)i);
			for (int j = 1; j < 7; j++)
			{
				position += (float)j * val;
				TerrainType terrainTypeAtPosition = GetTerrainTypeAtPosition(in position);
				if (!list.Contains(terrainTypeAtPosition))
				{
					list.Add(terrainTypeAtPosition);
				}
			}
		}
		return list;
	}

	public List<TerrainType> GetEnvironmentTerrainTypesCount(in CampaignVec2 originPosition, out TerrainType currentPositionTerrainType)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected I4, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		List<TerrainType> list = new List<TerrainType>();
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(1f, 0f);
		CampaignVec2 position = originPosition;
		currentPositionTerrainType = (TerrainType)(int)GetTerrainTypeAtPosition(in position);
		list.Add(currentPositionTerrainType);
		for (int i = 0; i < 8; i++)
		{
			((Vec2)(ref val)).RotateCCW(MathF.PI / 4f * (float)i);
			for (int j = 1; j < 7; j++)
			{
				position += (float)j * val;
				PathFaceRecord face = ((CampaignVec2)(ref position)).Face;
				if (((PathFaceRecord)(ref face)).IsValid())
				{
					TerrainType faceTerrainType = GetFaceTerrainType(((CampaignVec2)(ref position)).Face);
					list.Add(faceTerrainType);
				}
			}
		}
		return list;
	}

	public MapPatchData GetMapPatchAtPosition(in CampaignVec2 position)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if (_battleTerrainIndexMap != null)
		{
			CampaignVec2 val = position;
			int num = MathF.Floor(((CampaignVec2)(ref val)).X / ((Vec2)(ref _terrainSize)).X * (float)_battleTerrainIndexMapWidth);
			val = position;
			int num2 = MathF.Floor(((CampaignVec2)(ref val)).Y / ((Vec2)(ref _terrainSize)).Y * (float)_battleTerrainIndexMapHeight);
			num = MBMath.ClampIndex(num, 0, _battleTerrainIndexMapWidth);
			int num3 = (MBMath.ClampIndex(num2, 0, _battleTerrainIndexMapHeight) * _battleTerrainIndexMapWidth + num) * 2;
			byte sceneIndex = _battleTerrainIndexMap[num3];
			byte b = _battleTerrainIndexMap[num3 + 1];
			Vec2 normalizedCoordinates = default(Vec2);
			((Vec2)(ref normalizedCoordinates))._002Ector((float)(b & 0xF) / 15f, (float)((b >> 4) & 0xF) / 15f);
			return new MapPatchData
			{
				sceneIndex = sceneIndex,
				normalizedCoordinates = normalizedCoordinates
			};
		}
		return default(MapPatchData);
	}

	public CampaignVec2 GetAccessiblePointNearPosition(in CampaignVec2 pos, float radius)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = _scene;
		CampaignVec2 val = pos;
		return new CampaignVec2(MBMapScene.GetAccessiblePointNearPosition(scene, ((CampaignVec2)(ref val)).ToVec2(), pos.IsOnLand, radius), pos.IsOnLand);
	}

	public bool GetPathBetweenAIFaces(PathFaceRecord startingFace, PathFaceRecord endingFace, Vec2 startingPosition, Vec2 endingPosition, float agentRadius, NavigationPath path, int[] excludedFaceIds, float extraCostMultiplier, int regionSwitchCostFromLandToSea, int regionSwitchCostFromSeaToLand)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (regionSwitchCostFromLandToSea == 0 && regionSwitchCostFromSeaToLand == 0)
		{
			return _scene.GetPathBetweenAIFaces(startingFace.FaceIndex, endingFace.FaceIndex, startingPosition, endingPosition, agentRadius, path, excludedFaceIds, extraCostMultiplier);
		}
		return _scene.GetPathBetweenAIFaces(startingFace.FaceIndex, endingFace.FaceIndex, startingPosition, endingPosition, agentRadius, path, excludedFaceIds, extraCostMultiplier, regionSwitchCostFromLandToSea, regionSwitchCostFromSeaToLand);
	}

	public bool GetPathDistanceBetweenAIFaces(PathFaceRecord startingAiFace, PathFaceRecord endingAiFace, Vec2 startingPosition, Vec2 endingPosition, float agentRadius, float distanceLimit, out float distance, int[] excludedFaceIds, int regionSwitchCostFromLandToSea, int regionSwitchCostFromSeaToLand)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return _scene.GetPathDistanceBetweenAIFaces(startingAiFace.FaceIndex, endingAiFace.FaceIndex, startingPosition, endingPosition, agentRadius, distanceLimit, ref distance, excludedFaceIds, regionSwitchCostFromLandToSea, regionSwitchCostFromSeaToLand);
	}

	public bool IsLineToPointClear(PathFaceRecord startingFace, Vec2 position, Vec2 destination, float agentRadius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return _scene.IsLineToPointClear(startingFace.FaceIndex, position, destination, agentRadius);
	}

	public Vec2 GetLastPointOnNavigationMeshFromPositionToDestination(PathFaceRecord startingFace, Vec2 position, Vec2 destination, int[] excludedFaceIds = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return _scene.GetLastPointOnNavigationMeshFromPositionToDestination(startingFace.FaceIndex, position, destination, excludedFaceIds);
	}

	public Vec2 GetLastPositionOnNavMeshFaceForPointAndDirection(PathFaceRecord startingFace, Vec2 position, Vec2 destination)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return _scene.GetLastPositionOnNavMeshFaceForPointAndDirection(startingFace, position, destination);
	}

	public Vec2 GetNavigationMeshCenterPosition(PathFaceRecord face)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vec3 zero = Vec3.Zero;
		_scene.GetNavMeshCenterPosition(face.FaceIndex, ref zero);
		return ((Vec3)(ref zero)).AsVec2;
	}

	public Vec2 GetNavigationMeshCenterPosition(int faceIndex)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vec3 zero = Vec3.Zero;
		_scene.GetNavMeshCenterPosition(faceIndex, ref zero);
		return ((Vec3)(ref zero)).AsVec2;
	}

	public int GetNumberOfNavigationMeshFaces()
	{
		return _scene.GetNavMeshFaceCount();
	}

	public PathFaceRecord GetFaceAtIndex(int faceIndex)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return _scene.GetNavMeshPathFaceRecord(faceIndex);
	}

	public bool GetHeightAtPoint(in CampaignVec2 point, ref float height)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		BodyFlags val = (BodyFlags)16;
		val = (BodyFlags)(val | (point.IsOnLand ? 128 : 544321929));
		Scene scene = _scene;
		CampaignVec2 val2 = point;
		return scene.GetHeightAtPoint(((CampaignVec2)(ref val2)).ToVec2(), val, ref height);
	}

	public float GetWinterTimeFactor()
	{
		return _scene.GetWinterTimeFactor();
	}

	public float GetFaceVertexZ(PathFaceRecord navMeshFace)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _scene.GetNavMeshFaceFirstVertexZ(navMeshFace.FaceIndex);
	}

	public Vec3 GetGroundNormal(Vec2 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return _scene.GetNormalAt(position);
	}

	public void GetSiegeCampFrames(Settlement settlement, out List<MatrixFrame> siegeCamp1GlobalFrames, out List<MatrixFrame> siegeCamp2GlobalFrames)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		siegeCamp1GlobalFrames = new List<MatrixFrame>();
		siegeCamp2GlobalFrames = new List<MatrixFrame>();
		GameEntity campaignEntityWithName = _scene.GetCampaignEntityWithName(settlement.Party.Id);
		if (!(campaignEntityWithName != (GameEntity)null) || !settlement.IsFortification)
		{
			return;
		}
		List<GameEntity> list = new List<GameEntity>();
		campaignEntityWithName.GetChildrenRecursive(ref list);
		foreach (GameEntity item in list)
		{
			if (item.HasTag("map_camp_area_1"))
			{
				siegeCamp1GlobalFrames.Add(item.GetGlobalFrame());
			}
			else if (item.HasTag("map_camp_area_2"))
			{
				siegeCamp2GlobalFrames.Add(item.GetGlobalFrame());
			}
		}
	}

	public void GetTerrainHeightAndNormal(Vec2 position, out float height, out Vec3 normal)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_scene.GetTerrainHeightAndNormal(position, ref height, ref normal);
	}

	public string GetTerrainTypeName(TerrainType type)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected I4, but got Unknown
		string result = "Invalid";
		switch (type - 1)
		{
		case 9:
			result = "Water";
			break;
		case 6:
			result = "Mountain";
			break;
		case 2:
			result = "Snow";
			break;
		case 4:
			result = "Steppe";
			break;
		case 0:
			result = "Plain";
			break;
		case 1:
			result = "Desert";
			break;
		case 14:
			result = "Swamp";
			break;
		case 15:
			result = "Dune";
			break;
		case 16:
			result = "Bridge";
			break;
		case 10:
			result = "River";
			break;
		case 3:
			result = "Forest";
			break;
		case 5:
			result = "Fording";
			break;
		case 7:
			result = "Lake";
			break;
		case 12:
			result = "Canyon";
			break;
		}
		return result;
	}

	public uint GetSceneXmlCrc()
	{
		return _scene.GetSceneXMLCRC();
	}

	public uint GetSceneNavigationMeshCrc()
	{
		return _scene.GetNavigationMeshCRC();
	}

	public Vec2 GetWindAtPosition(Vec2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		int textureDataIndexForPosition = GetTextureDataIndexForPosition(position, 512, 512);
		return new Vec2(_windFlowMapData[textureDataIndexForPosition * 2], _windFlowMapData[textureDataIndexForPosition * 2 + 1]);
	}

	public float GetSnowAmountAtPosition(Vec2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		int textureDataIndexForPosition = GetTextureDataIndexForPosition(position, _snowAndRainDataTextureWidth, _snowAndRainDataTextureHeight);
		return (int)_snowAndRainData[textureDataIndexForPosition * 2];
	}

	public float GetRainAmountAtPosition(Vec2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		int textureDataIndexForPosition = GetTextureDataIndexForPosition(position, _snowAndRainDataTextureWidth, _snowAndRainDataTextureHeight);
		return (int)_snowAndRainData[textureDataIndexForPosition * 2 + 1];
	}

	private int GetTextureDataIndexForPosition(Vec2 position, int dimensionX, int dimensionY)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vec2 terrainSize = GetTerrainSize();
		int num = MathF.Floor(position.x / ((Vec2)(ref terrainSize)).X * (float)dimensionX);
		int num2 = MathF.Floor(position.y / ((Vec2)(ref terrainSize)).Y * (float)dimensionY);
		num = MBMath.ClampIndex(num, 0, dimensionX);
		return MBMath.ClampIndex(num2, 0, dimensionY) * dimensionX + num;
	}

	public void SetupWaterWake(float wakeWorldSize, float wakeCameraOffset)
	{
		_scene.EnsureWaterWakeRenderer();
		_scene.SetWaterWakeWorldSize(wakeWorldSize, 0.994f);
		_scene.SetWaterWakeCameraOffset(wakeCameraOffset);
	}

	PathFaceRecord IMapScene.GetFaceIndex(in CampaignVec2 vec2)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetFaceIndex(in vec2);
	}

	TerrainType IMapScene.GetTerrainTypeAtPosition(in CampaignVec2 vec2)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetTerrainTypeAtPosition(in vec2);
	}

	List<TerrainType> IMapScene.GetEnvironmentTerrainTypes(in CampaignVec2 vec2)
	{
		return GetEnvironmentTerrainTypes(in vec2);
	}

	List<TerrainType> IMapScene.GetEnvironmentTerrainTypesCount(in CampaignVec2 vec2, out TerrainType currentPositionTerrainType)
	{
		return GetEnvironmentTerrainTypesCount(in vec2, out currentPositionTerrainType);
	}

	MapPatchData IMapScene.GetMapPatchAtPosition(in CampaignVec2 position)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetMapPatchAtPosition(in position);
	}

	CampaignVec2 IMapScene.GetNearestFaceCenterForPosition(in CampaignVec2 vec2, int[] excludedFaceIds)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return GetNearestFaceCenterForPosition(in vec2, excludedFaceIds);
	}

	CampaignVec2 IMapScene.GetAccessiblePointNearPosition(in CampaignVec2 vec2, float radius)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return GetAccessiblePointNearPosition(in vec2, radius);
	}

	bool IMapScene.GetHeightAtPoint(in CampaignVec2 point, ref float height)
	{
		return GetHeightAtPoint(in point, ref height);
	}

	void IMapScene.AddNewEntityToMapScene(string entityId, in CampaignVec2 position)
	{
		AddNewEntityToMapScene(entityId, in position);
	}
}
