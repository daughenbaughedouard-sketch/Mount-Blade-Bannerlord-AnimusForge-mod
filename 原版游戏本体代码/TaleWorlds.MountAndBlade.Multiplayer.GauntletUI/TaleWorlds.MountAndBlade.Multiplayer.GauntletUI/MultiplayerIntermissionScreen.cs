using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Intermission;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI;

[GameStateScreen(typeof(LobbyGameStateCustomGameClient))]
[GameStateScreen(typeof(LobbyGameStateCommunityClient))]
public class MultiplayerIntermissionScreen : ScreenBase, IGameStateListener, IChatLogHandlerScreen
{
	private MPIntermissionVM _dataSource;

	private SpriteCategory _customGameClientCategory;

	public GauntletLayer Layer { get; private set; }

	public MultiplayerIntermissionScreen(LobbyGameStateCustomGameClient gameState)
	{
		Construct();
	}

	public MultiplayerIntermissionScreen(LobbyGameStateCommunityClient gameState)
	{
		Construct();
	}

	private void Construct()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		_customGameClientCategory = UIResourceManager.LoadSpriteCategory("ui_mpintermission");
		_dataSource = new MPIntermissionVM();
		Layer = new GauntletLayer("MultiplayerIntermission", 100, false);
		((ScreenLayer)Layer).IsFocusLayer = true;
		((ScreenBase)this).AddLayer((ScreenLayer)(object)Layer);
		((ScreenLayer)Layer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		Layer.LoadMovie("MultiplayerIntermission", (ViewModel)(object)_dataSource);
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		_dataSource.Tick();
	}

	protected override void OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		_customGameClientCategory.Unload();
		((ScreenLayer)Layer).InputRestrictions.ResetInputRestrictions();
		Layer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	void IGameStateListener.OnActivate()
	{
		((ScreenLayer)Layer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		ScreenManager.TrySetFocus((ScreenLayer)(object)Layer);
		LoadingWindow.EnableGlobalLoadingWindow();
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

	void IChatLogHandlerScreen.TryUpdateChatLogLayerParameters(ref bool isTeamChatAvailable, ref bool inputEnabled, ref bool isToggleChatHintAvailable, ref bool isMouseVisible, ref InputContext inputContext)
	{
		if (Layer != null)
		{
			isTeamChatAvailable = false;
			inputEnabled = true;
			inputContext = ((ScreenLayer)Layer).Input;
		}
	}
}
