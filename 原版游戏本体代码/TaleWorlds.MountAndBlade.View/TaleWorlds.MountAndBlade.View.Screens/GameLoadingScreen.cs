using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens;

[GameStateScreen(typeof(GameLoadingState))]
public class GameLoadingScreen : ScreenBase, IGameStateListener
{
	public GameLoadingScreen(GameLoadingState gameLoadingState)
	{
	}

	protected override void OnActivate()
	{
		((ScreenBase)this).OnActivate();
		LoadingWindow.EnableGlobalLoadingWindow();
		Utilities.SetScreenTextRenderingState(false);
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		Utilities.SetScreenTextRenderingState(true);
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
