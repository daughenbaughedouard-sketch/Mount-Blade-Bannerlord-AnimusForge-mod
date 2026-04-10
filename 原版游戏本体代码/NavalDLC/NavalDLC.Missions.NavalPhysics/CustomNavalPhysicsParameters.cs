using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.Missions.NavalPhysics;

public class CustomNavalPhysicsParameters : ScriptComponentBehavior
{
	public bool BehaveLikeShip;

	public float FloatingForceMultiplier = 1f;

	public float LinearFrictionMultiplierRight = 1f;

	public float LinearFrictionMultiplierLeft = 1f;

	public float LinearFrictionMultiplierForward = 1f;

	public float LinearFrictionMultiplierBackward = 1f;

	public float LinearFrictionMultiplierUp = 1f;

	public float LinearFrictionMultiplierDown = 1f;

	public Vec3 AngularFrictionMultiplier = Vec3.One;

	public float ContinuousDriftSpeed;

	protected override void OnInit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<NavalPhysics>().SetContinuousDriftSpeed(ContinuousDriftSpeed);
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorTick(dt);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<NavalPhysics>()?.SetContinuousDriftSpeed(ContinuousDriftSpeed);
	}
}
