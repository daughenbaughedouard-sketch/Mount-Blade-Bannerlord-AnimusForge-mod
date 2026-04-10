using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Objects;

internal class BasicWaterFloater : ScriptComponentBehavior
{
	protected override void OnInit()
	{
	}

	protected override void OnTick(float dt)
	{
		Float();
	}

	protected override void OnEditorInit()
	{
	}

	protected override void OnEditorTick(float dt)
	{
		Float();
	}

	private void Float()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		globalFrame.origin.z = ((ScriptComponentBehavior)this).Scene.GetWaterLevelAtPosition(((Vec3)(ref globalFrame.origin)).AsVec2, true, false);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetGlobalFrame(ref globalFrame, true);
	}
}
