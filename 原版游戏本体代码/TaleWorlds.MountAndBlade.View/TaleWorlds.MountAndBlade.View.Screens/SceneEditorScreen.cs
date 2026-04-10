using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens;

[GameStateScreen(typeof(EditorState))]
public class SceneEditorScreen : ScreenBase, IGameStateListener
{
	private SceneEditorLayer _editorLayer;

	public SceneEditorScreen(EditorState editorState)
	{
	}

	protected override void OnInitialize()
	{
		((ScreenBase)this).OnInitialize();
		_editorLayer = new SceneEditorLayer();
		((ScreenLayer)_editorLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)0);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_editorLayer);
		ManagedParameters.Instance.Initialize(ModuleHelper.GetXmlPath("Native", "managed_core_parameters"));
	}

	protected override void OnActivate()
	{
		((ScreenBase)this).OnActivate();
		MouseManager.ActivateMouseCursor((CursorType)0);
		MBEditor.ActivateSceneEditorPresentation();
	}

	protected override void OnDeactivate()
	{
		MBEditor.DeactivateSceneEditorPresentation();
		MouseManager.ActivateMouseCursor((CursorType)1);
		((ScreenBase)this).OnDeactivate();
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		if (_editorLayer != null)
		{
			bool mouseVisible = Screen.GetMouseVisible();
			((ScreenLayer)_editorLayer).InputRestrictions.SetMouseVisibility(mouseVisible);
		}
		MBEditor.TickSceneEditorPresentation(dt);
	}

	void IGameStateListener.OnActivate()
	{
	}

	void IGameStateListener.OnDeactivate()
	{
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}
}
