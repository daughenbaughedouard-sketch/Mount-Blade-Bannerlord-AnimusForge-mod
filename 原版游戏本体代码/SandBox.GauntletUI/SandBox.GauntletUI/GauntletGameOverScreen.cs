using SandBox.ViewModelCollection.GameOver;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI;

[GameStateScreen(typeof(GameOverState))]
public class GauntletGameOverScreen : ScreenBase, IGameOverStateHandler, IGameStateListener
{
	private SpriteCategory _gameOverCategory;

	private GameOverVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private readonly GameOverState _gameOverState;

	public GauntletGameOverScreen(GameOverState gameOverState)
	{
		_gameOverState = gameOverState;
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			CloseGameOverScreen();
		}
	}

	void IGameStateListener.OnActivate()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected I4, but got Unknown
		((ScreenBase)this).OnActivate();
		_gameOverCategory = UIResourceManager.LoadSpriteCategory("ui_gameover");
		_gauntletLayer = new GauntletLayer("GameOverScreen", 1, true);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		_dataSource = new GameOverVM(_gameOverState.Reason, CloseGameOverScreen);
		_dataSource.SetCloseInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_gauntletLayer.LoadMovie("GameOverScreen", (ViewModel)(object)_dataSource);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)15));
		GameOverReason reason = _gameOverState.Reason;
		switch ((int)reason)
		{
		case 1:
			UISoundsHelper.PlayUISound("event:/ui/endgame/end_clan_destroyed");
			break;
		case 0:
			UISoundsHelper.PlayUISound("event:/ui/endgame/end_retirement");
			break;
		case 2:
			UISoundsHelper.PlayUISound("event:/ui/endgame/end_victory");
			break;
		}
		LoadingWindow.DisableGlobalLoadingWindow();
	}

	void IGameStateListener.OnDeactivate()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		((ScreenBase)this).OnDeactivate();
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
		_gameOverCategory.Unload();
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_gauntletLayer = null;
	}

	private void CloseGameOverScreen()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		bool flag = false;
		if (flag || Game.Current.IsDevelopmentMode || (int)_gameOverState.Reason == 2)
		{
			Game.Current.GameStateManager.PopState(0);
			if ((flag || Game.Current.IsDevelopmentMode) && (int)_gameOverState.Reason == 0)
			{
				PlayerEncounter.Finish(true);
			}
		}
		else
		{
			MBGameManager.EndGame();
		}
	}
}
