using TaleWorlds.CampaignSystem.Map;

namespace SandBox;

public class MapSceneCreator : IMapSceneCreator
{
	IMapScene IMapSceneCreator.CreateMapScene()
	{
		return (IMapScene)(object)new MapScene();
	}
}
