using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade;

public class MultiplayerGameManager : MBGameManager
{
	public MultiplayerGameManager()
	{
		MBMusicManager current = MBMusicManager.Current;
		if (current != null)
		{
			current.PauseMusicManagerSystem();
		}
	}

	protected override void DoLoadingForGameManager(GameManagerLoadingSteps gameManagerLoadingStep, out GameManagerLoadingSteps nextStep)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		nextStep = (GameManagerLoadingSteps)(-1);
		switch ((int)gameManagerLoadingStep)
		{
		case 0:
			nextStep = (GameManagerLoadingSteps)1;
			break;
		case 1:
			MBGameManager.LoadModuleData(false);
			MBDebug.Print("Game creating...", 0, (DebugColor)12, 17592186044416uL);
			MBGlobals.InitializeReferences();
			Game.CreateGame((GameType)(object)new MultiplayerGame(), (GameManagerBase)(object)this).DoLoading();
			nextStep = (GameManagerLoadingSteps)2;
			break;
		case 2:
			MBGameManager.StartNewGame();
			nextStep = (GameManagerLoadingSteps)3;
			break;
		case 3:
			nextStep = (GameManagerLoadingSteps)(Game.Current.DoLoading() ? 4 : 3);
			break;
		case 4:
		{
			bool flag = true;
			foreach (MBSubModuleBase item in (List<MBSubModuleBase>)(object)Module.CurrentModule.CollectSubModules())
			{
				flag = flag && item.DoLoading(Game.Current);
			}
			nextStep = (GameManagerLoadingSteps)(flag ? 5 : 4);
			break;
		}
		case 5:
			nextStep = (GameManagerLoadingSteps)(-1);
			break;
		}
	}

	public override void OnLoadFinished()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		((MBGameManager)this).OnLoadFinished();
		MBGlobals.InitializeReferences();
		GameState val = null;
		if (GameNetwork.IsDedicatedServer)
		{
			_ = Module.CurrentModule.StartupInfo.DedicatedServerType;
			val = (GameState)(object)Game.Current.GameStateManager.CreateState<UnspecifiedDedicatedServerState>();
			Utilities.SetFrameLimiterWithSleep(true);
		}
		else
		{
			val = (GameState)(object)Game.Current.GameStateManager.CreateState<LobbyState>();
		}
		Game.Current.GameStateManager.CleanAndPushState(val, 0);
	}

	public override void OnAfterCampaignStart(Game game)
	{
		if (GameNetwork.IsDedicatedServer)
		{
			MultiplayerMain.InitializeAsDedicatedServer((IGameNetworkHandler)(object)new GameNetworkHandler());
		}
		else
		{
			MultiplayerMain.Initialize((IGameNetworkHandler)(object)new GameNetworkHandler());
		}
	}

	public override void OnNewCampaignStart(Game game, object starterObject)
	{
		foreach (MBSubModuleBase item in (List<MBSubModuleBase>)(object)Module.CurrentModule.CollectSubModules())
		{
			item.OnMultiplayerGameStart(game, starterObject);
		}
	}

	public override void OnSessionInvitationAccepted(SessionInvitationType sessionInvitationType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)sessionInvitationType != 1)
		{
			((MBGameManager)this).OnSessionInvitationAccepted(sessionInvitationType);
		}
	}

	public override void OnPlatformRequestedMultiplayer()
	{
	}
}
