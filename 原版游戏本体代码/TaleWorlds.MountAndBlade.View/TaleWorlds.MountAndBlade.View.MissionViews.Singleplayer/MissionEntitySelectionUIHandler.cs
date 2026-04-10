using System;
using System.Diagnostics;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

public class MissionEntitySelectionUIHandler : MissionView
{
	private Action<WeakGameEntity> onSelect;

	private Action<WeakGameEntity> onHover;

	public MissionEntitySelectionUIHandler(Action<WeakGameEntity> onSelect = null, Action<WeakGameEntity> onHover = null)
	{
		this.onSelect = onSelect;
		this.onHover = onHover;
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		base.OnMissionScreenTick(dt);
		WeakGameEntity value = new Lazy<WeakGameEntity>((Func<WeakGameEntity>)GetCollidedEntity).Value;
		onHover?.Invoke(value);
		if (base.Input.IsKeyReleased((InputKey)224))
		{
			onSelect?.Invoke(value);
		}
	}

	private unsafe WeakGameEntity GetCollidedEntity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vec2 mousePositionRanged = base.Input.GetMousePositionRanged();
		base.MissionScreen.ScreenPointToWorldRay(mousePositionRanged, out var rayBegin, out var rayEnd);
		TWSharedMutexReadLock val = default(TWSharedMutexReadLock);
		((TWSharedMutexReadLock)(ref val))._002Ector(Scene.PhysicsAndRayCastLock);
		WeakGameEntity result;
		try
		{
			if (Mission.Current != null)
			{
				float num = default(float);
				WeakGameEntity parent = default(WeakGameEntity);
				Mission.Current.Scene.RayCastForClosestEntityOrTerrain(rayBegin, rayEnd, ref num, ref parent, 0.3f, (BodyFlags)79617);
				while (((WeakGameEntity)(ref parent)).IsValid)
				{
					result = ((WeakGameEntity)(ref parent)).Parent;
					if (!((WeakGameEntity)(ref result)).IsValid)
					{
						break;
					}
					parent = ((WeakGameEntity)(ref parent)).Parent;
				}
				result = parent;
			}
			else
			{
				result = WeakGameEntity.Invalid;
			}
		}
		finally
		{
			((IDisposable)(*(TWSharedMutexReadLock*)(&val))/*cast due to .constrained prefix*/).Dispose();
		}
		return result;
	}

	public override void OnRemoveBehavior()
	{
		onSelect = null;
		onHover = null;
		base.OnRemoveBehavior();
	}

	[Conditional("DEBUG")]
	public void TickDebug()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity collidedEntity = GetCollidedEntity();
		if (((WeakGameEntity)(ref collidedEntity)).IsValid)
		{
			_ = ((WeakGameEntity)(ref collidedEntity)).Name;
		}
	}
}
