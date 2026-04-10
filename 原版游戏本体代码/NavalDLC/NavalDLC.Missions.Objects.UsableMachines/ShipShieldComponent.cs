using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipShieldComponent : DestructableComponent
{
	private List<GameEntity> _disablingConnectionEntities = new List<GameEntity>();

	public override bool IsFocusable => false;

	private ShipShieldComponent()
	{
	}

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((DestructableComponent)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	public void RegisterRampEntityDisablingShield(GameEntity connectionEntity)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (_disablingConnectionEntities.Count == 0)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
		}
		_disablingConnectionEntities.Add(connectionEntity);
	}

	public void DeregisterRampEntityDisablingShield(GameEntity connectionEntity)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (_disablingConnectionEntities.Remove(connectionEntity) && _disablingConnectionEntities.Count == 0)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(true);
		}
	}
}
