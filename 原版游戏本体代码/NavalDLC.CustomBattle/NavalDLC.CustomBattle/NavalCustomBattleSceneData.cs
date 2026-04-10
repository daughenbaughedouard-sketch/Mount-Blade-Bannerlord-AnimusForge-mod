using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.CustomBattle;

public struct NavalCustomBattleSceneData
{
	public string SceneID { get; private set; }

	public TextObject Name { get; private set; }

	public TerrainType Terrain { get; private set; }

	public NavalCustomBattleSceneData(string sceneID, TextObject name, TerrainType terrain)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		SceneID = sceneID;
		Name = name;
		Terrain = terrain;
	}
}
