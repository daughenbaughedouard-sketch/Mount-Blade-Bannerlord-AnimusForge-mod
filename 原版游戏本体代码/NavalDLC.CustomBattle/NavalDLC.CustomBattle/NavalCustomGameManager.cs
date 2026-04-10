using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.CustomBattle;

public class NavalCustomGameManager : MBGameManager
{
	protected override void DoLoadingForGameManager(GameManagerLoadingSteps gameManagerLoadingStep, out GameManagerLoadingSteps nextStep)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		nextStep = (GameManagerLoadingSteps)(-1);
		switch ((int)gameManagerLoadingStep)
		{
		case 0:
			MBGameManager.LoadModuleData(false);
			MBGlobals.InitializeReferences();
			Game.CreateGame((GameType)(object)new NavalCustomGame(), (GameManagerBase)(object)this).DoLoading();
			nextStep = (GameManagerLoadingSteps)1;
			break;
		case 1:
		{
			bool flag = true;
			foreach (MBSubModuleBase item in (List<MBSubModuleBase>)(object)Module.CurrentModule.CollectSubModules())
			{
				flag = flag && item.DoLoading(Game.Current);
			}
			nextStep = (GameManagerLoadingSteps)((!flag) ? 1 : 2);
			break;
		}
		case 2:
			MBGameManager.StartNewGame();
			nextStep = (GameManagerLoadingSteps)3;
			break;
		case 3:
			nextStep = (GameManagerLoadingSteps)(Game.Current.DoLoading() ? 4 : 3);
			break;
		case 4:
			nextStep = (GameManagerLoadingSteps)5;
			break;
		case 5:
			nextStep = (GameManagerLoadingSteps)(-1);
			break;
		}
	}

	public override void OnAfterCampaignStart(Game game)
	{
	}

	public override void OnLoadFinished()
	{
		((MBGameManager)this).OnLoadFinished();
		Game.Current.GameStateManager.CleanAndPushState((GameState)(object)Game.Current.GameStateManager.CreateState<NavalCustomBattleState>(), 0);
	}
}
