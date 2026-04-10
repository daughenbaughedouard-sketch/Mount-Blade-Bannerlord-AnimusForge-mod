using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View;

public class PopupSceneEmissionHandler : ScriptComponentBehavior
{
	public float startTime;

	public float transitionTime;

	private float timeElapsed;

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	protected override void OnEditorInit()
	{
		((ScriptComponentBehavior)this).OnEditorInit();
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnTick(float dt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		timeElapsed += dt;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child;
			Mesh firstMesh = ((WeakGameEntity)(ref current)).GetFirstMesh();
			if ((NativeObject)(object)firstMesh != (NativeObject)null)
			{
				firstMesh.SetVectorArgument(1f, 0.5f, 1f, MBMath.SmoothStep(startTime, startTime + transitionTime, timeElapsed) * 10f);
			}
		}
	}

	protected override void OnEditorTick(float dt)
	{
		((ScriptComponentBehavior)this).OnEditorTick(dt);
		((ScriptComponentBehavior)this).OnTick(dt);
	}
}
