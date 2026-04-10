using TaleWorlds.Engine;

namespace SandBox.BoardGames.Objects;

public class BoardGameDecal : ScriptComponentBehavior
{
	protected override void OnInit()
	{
		((ScriptComponentBehavior)this).OnInit();
		SetAlpha(0f);
	}

	public void SetAlpha(float alpha)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetAlpha(alpha);
	}

	protected override bool MovesEntity()
	{
		return false;
	}
}
