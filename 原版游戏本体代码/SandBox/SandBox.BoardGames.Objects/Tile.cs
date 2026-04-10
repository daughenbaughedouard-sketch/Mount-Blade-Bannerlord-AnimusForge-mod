using TaleWorlds.Engine;

namespace SandBox.BoardGames.Objects;

public class Tile : ScriptComponentBehavior
{
	public MetaMesh TileMesh;

	protected override void OnInit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).RemoveMultiMesh(((WeakGameEntity)(ref gameEntity2)).GetMetaMesh(0));
	}

	public void SetVisibility(bool visible)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(visible);
	}

	protected override bool MovesEntity()
	{
		return false;
	}
}
