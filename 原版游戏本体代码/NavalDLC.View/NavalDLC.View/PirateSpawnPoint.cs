using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.View;

public class PirateSpawnPoint : ScriptComponentBehavior
{
	public string ClanStringId;

	public bool ToggleDebugRadius;

	public float Radius = 10f;

	public Vec2 GetPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		return ((Vec3)(ref globalPosition)).AsVec2;
	}

	protected override void OnInit()
	{
	}

	protected override void OnEditorInit()
	{
	}

	protected override void OnSceneSave(string saveFolder)
	{
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (ToggleDebugRadius || MBEditor.IsEntitySelected(((ScriptComponentBehavior)this).GameEntity))
		{
			Scene scene = ((ScriptComponentBehavior)this).Scene;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			float radius = Radius;
			Color red = Colors.Red;
			DebugExtensions.RenderDebugCircleOnTerrain(scene, globalFrame, radius, ((Color)(ref red)).ToUnsignedInteger(), true, false);
		}
	}
}
