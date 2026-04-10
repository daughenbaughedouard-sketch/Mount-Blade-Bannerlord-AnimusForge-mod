using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens;

public class SceneEditorLayer : ScreenLayer
{
	public SceneEditorLayer()
		: base("SceneEditorLayer", -100)
	{
	}

	protected override void OnActivate()
	{
		((ScreenLayer)this).OnActivate();
	}

	protected override void Tick(float dt)
	{
		((ScreenLayer)this).Tick(dt);
	}

	protected override void OnDeactivate()
	{
		((ScreenLayer)this).OnDeactivate();
	}

	protected override void RefreshGlobalOrder(ref int currentOrder)
	{
		SceneView editorSceneView = MBEditor.GetEditorSceneView();
		if ((NativeObject)(object)editorSceneView != (NativeObject)null)
		{
			((View)editorSceneView).SetRenderOrder(currentOrder);
			currentOrder++;
		}
	}
}
