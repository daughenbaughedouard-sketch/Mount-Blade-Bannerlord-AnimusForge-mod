using SandBox.GauntletUI.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.GauntletUI.SceneNotification;

namespace SandBox.GauntletUI;

public class SandBoxGauntletUISubModule : MBSubModuleBase
{
	private class SandBoxGameStateManagerListener : IGameStateManagerListener
	{
		void IGameStateManagerListener.OnCleanStates()
		{
			UpdateCampaignMission();
		}

		void IGameStateManagerListener.OnCreateState(GameState gameState)
		{
		}

		void IGameStateManagerListener.OnPopState(GameState gameState)
		{
			UpdateCampaignMission();
			if (gameState is MissionState || gameState is MapState)
			{
				CampaignInformationManager.ClearAllDialogNotifications(false);
			}
		}

		void IGameStateManagerListener.OnPushState(GameState gameState, bool isTopGameState)
		{
			UpdateCampaignMission();
			if (gameState is MissionState || gameState is MapState)
			{
				CampaignInformationManager.ClearAllDialogNotifications(false);
			}
		}

		void IGameStateManagerListener.OnSavedGameLoadFinished()
		{
		}

		private void UpdateCampaignMission()
		{
			ICampaignMission current = CampaignMission.Current;
			if (current != null)
			{
				current.OnGameStateChanged();
			}
		}
	}

	private bool _gameStarted;

	private bool _initialized;

	private GameStateManager _registeredGameStateManager;

	private bool _initializedConversationHandler;

	private SandBoxGameStateManagerListener _conversationListener;

	public SandBoxGauntletUISubModule()
	{
		_conversationListener = new SandBoxGameStateManagerListener();
	}

	public override void OnCampaignStart(Game game, object starterObject)
	{
		((MBSubModuleBase)this).OnCampaignStart(game, starterObject);
		if (!_gameStarted && game.GameType is Campaign)
		{
			_gameStarted = true;
		}
	}

	protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
	{
		((MBSubModuleBase)this).OnGameStart(game, gameStarterObject);
		if (!_gameStarted && game.GameType is Campaign)
		{
			_gameStarted = true;
			SandBoxGauntletGameNotification.Initialize();
		}
	}

	public override void OnGameEnd(Game game)
	{
		((MBSubModuleBase)this).OnGameEnd(game);
		if (_gameStarted && game.GameType is Campaign)
		{
			_gameStarted = false;
			GauntletGameNotification.Initialize();
		}
	}

	public override void BeginGameStart(Game game)
	{
		((MBSubModuleBase)this).BeginGameStart(game);
		if (Campaign.Current != null)
		{
			Campaign.Current.VisualCreator.MapEventVisualCreator = (IMapEventVisualCreator)(object)new GauntletMapEventVisualCreator();
		}
	}

	protected override void OnApplicationTick(float dt)
	{
		((MBSubModuleBase)this).OnApplicationTick(dt);
		if (!_initializedConversationHandler)
		{
			Game current = Game.Current;
			if (((current != null) ? current.GameStateManager : null) != null)
			{
				Game.Current.GameStateManager.RegisterListener((IGameStateManagerListener)(object)_conversationListener);
				_registeredGameStateManager = Game.Current.GameStateManager;
				_initializedConversationHandler = true;
				goto IL_008c;
			}
		}
		if (_initializedConversationHandler)
		{
			Game current2 = Game.Current;
			if (((current2 != null) ? current2.GameStateManager : null) == null)
			{
				_registeredGameStateManager.UnregisterListener((IGameStateManagerListener)(object)_conversationListener);
				_initializedConversationHandler = false;
				_registeredGameStateManager = null;
			}
		}
		goto IL_008c;
		IL_008c:
		if (!_initialized && GauntletSceneNotification.Current != null)
		{
			if (!Utilities.CommandLineArgumentExists("VisualTests"))
			{
				GauntletSceneNotification.Current.RegisterContextProvider((ISceneNotificationContextProvider)(object)new SandboxSceneNotificationContextProvider());
			}
			_initialized = true;
		}
	}
}
