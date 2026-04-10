using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.CustomBattle;

public struct CustomBattleSceneData
{
	public string SceneID { get; private set; }

	public TextObject Name { get; private set; }

	public TerrainType Terrain { get; private set; }

	public List<TerrainType> TerrainTypes { get; private set; }

	public ForestDensity ForestDensity { get; private set; }

	public bool IsSiegeMap { get; private set; }

	public bool IsVillageMap { get; private set; }

	public bool IsLordsHallMap { get; private set; }

	public CustomBattleSceneData(string sceneID, TextObject name, TerrainType terrain, List<TerrainType> terrainTypes, ForestDensity forestDensity, bool isSiegeMap, bool isVillageMap, bool isLordsHallMap)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		SceneID = sceneID;
		Name = name;
		Terrain = terrain;
		TerrainTypes = terrainTypes;
		ForestDensity = forestDensity;
		IsSiegeMap = isSiegeMap;
		IsVillageMap = isVillageMap;
		IsLordsHallMap = isLordsHallMap;
	}
}
