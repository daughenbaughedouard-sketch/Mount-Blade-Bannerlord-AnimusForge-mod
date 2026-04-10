using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;

namespace SandBox.View.Map.Managers;

public abstract class EntityVisualManagerBase : CampaignEntityVisualComponent
{
	private Scene _mapScene;

	public Scene MapScene
	{
		get
		{
			if ((NativeObject)(object)_mapScene == (NativeObject)null && Campaign.Current != null && Campaign.Current.MapSceneWrapper != null)
			{
				_mapScene = ((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene;
			}
			return _mapScene;
		}
	}
}
public abstract class EntityVisualManagerBase<TEntity> : EntityVisualManagerBase
{
	public abstract MapEntityVisual<TEntity> GetVisualOfEntity(TEntity entity);

	public static EntityVisualManagerBase<TEntity> GetEntityVisualManagerBase()
	{
		return SandBoxViewSubModule.SandBoxViewVisualManager.GetEntityComponent<EntityVisualManagerBase<TEntity>>();
	}
}
