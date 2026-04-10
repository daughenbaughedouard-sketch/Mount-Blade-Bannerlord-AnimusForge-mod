using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace SandBox;

public class SandBoxGameManager : MBGameManager
{
	public delegate Campaign CampaignCreatorDelegate();

	private LoadResult _loadedGameResult;

	private CampaignCreatorDelegate _campaignCreator;

	public bool LoadingSavedGame { get; private set; }

	public MetaData MetaData
	{
		get
		{
			LoadResult loadedGameResult = _loadedGameResult;
			if (loadedGameResult == null)
			{
				return null;
			}
			return loadedGameResult.MetaData;
		}
	}

	public SandBoxGameManager(CampaignCreatorDelegate campaignCreator)
	{
		LoadingSavedGame = false;
		_campaignCreator = campaignCreator;
	}

	public SandBoxGameManager(LoadResult loadedGameResult)
	{
		LoadingSavedGame = true;
		_loadedGameResult = loadedGameResult;
	}

	public override void OnGameEnd(Game game)
	{
		MBDebug.SetErrorReportScene((Scene)null);
		((MBGameManager)this).OnGameEnd(game);
	}

	protected override void DoLoadingForGameManager(GameManagerLoadingSteps gameManagerLoadingStep, out GameManagerLoadingSteps nextStep)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		nextStep = (GameManagerLoadingSteps)(-1);
		switch ((int)gameManagerLoadingStep)
		{
		case 0:
			nextStep = (GameManagerLoadingSteps)1;
			break;
		case 1:
			MBGameManager.LoadModuleData(LoadingSavedGame);
			nextStep = (GameManagerLoadingSteps)2;
			break;
		case 2:
			if (!LoadingSavedGame)
			{
				MBGameManager.StartNewGame();
			}
			nextStep = (GameManagerLoadingSteps)3;
			break;
		case 3:
			MBGlobals.InitializeReferences();
			if (!LoadingSavedGame)
			{
				MBDebug.Print("Initializing new game begin...", 0, (DebugColor)12, 17592186044416uL);
				Campaign obj = _campaignCreator();
				Game.CreateGame((GameType)(object)obj, (GameManagerBase)(object)this);
				obj.SetLoadingParameters((GameLoadingType)1);
				MBDebug.Print("Initializing new game end...", 0, (DebugColor)12, 17592186044416uL);
			}
			else
			{
				MBDebug.Print("Initializing saved game begin...", 0, (DebugColor)12, 17592186044416uL);
				((Campaign)Game.LoadSaveGame(_loadedGameResult, (GameManagerBase)(object)this).GameType).SetLoadingParameters((GameLoadingType)2);
				_loadedGameResult = null;
				Common.MemoryCleanupGC(false);
				MBDebug.Print("Initializing saved game end...", 0, (DebugColor)12, 17592186044416uL);
			}
			Game.Current.DoLoading();
			nextStep = (GameManagerLoadingSteps)4;
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
			nextStep = (GameManagerLoadingSteps)(Game.Current.DoLoading() ? (-1) : 5);
			break;
		}
	}

	public override void OnAfterCampaignStart(Game game)
	{
	}

	public override void OnLoadFinished()
	{
		if (!LoadingSavedGame)
		{
			MBDebug.Print("Switching to menu window...", 0, (DebugColor)12, 17592186044416uL);
			if (!Game.Current.IsDevelopmentMode)
			{
				MBDebug.Print("OnLoadFinished Not DevelopmentMode", 0, (DebugColor)12, 17592186044416uL);
				VideoPlaybackState val = Game.Current.GameStateManager.CreateState<VideoPlaybackState>();
				string text = ModuleHelper.GetModuleFullPath("SandBox") + "Videos/CampaignIntro/";
				string text2 = text + "campaign_intro";
				string text3 = text + "campaign_intro.ivf";
				string text4 = text + "campaign_intro.ogg";
				val.SetStartingParameters(text3, text4, text2, 30f, true);
				val.SetOnVideoFinisedDelegate((Action)LaunchSandboxCharacterCreation);
				Game.Current.GameStateManager.CleanAndPushState((GameState)(object)val, 0);
			}
			else
			{
				MBDebug.Print("OnLoadFinished DevelopmentMode", 0, (DebugColor)12, 17592186044416uL);
				MBDebug.Print("Launching Sandbox Character Creation", 0, (DebugColor)12, 17592186044416uL);
				LaunchSandboxCharacterCreation();
			}
		}
		else
		{
			MBDebug.Print("Loading Save Game", 0, (DebugColor)12, 17592186044416uL);
			Game.Current.GameStateManager.OnSavedGameLoadFinished();
			Game.Current.GameStateManager.CleanAndPushState((GameState)(object)Game.Current.GameStateManager.CreateState<MapState>(), 0);
			GameState activeState = Game.Current.GameStateManager.ActiveState;
			MapState val2 = (MapState)(object)((activeState is MapState) ? activeState : null);
			string text5 = ((val2 != null) ? val2.GameMenuId : null);
			if (!string.IsNullOrEmpty(text5))
			{
				if (Campaign.Current.GameMenuManager.GetGameMenu(text5) != null)
				{
					PlayerEncounter current = PlayerEncounter.Current;
					if (current != null)
					{
						current.OnLoad();
					}
					Campaign.Current.GameMenuManager.SetNextMenu(text5);
				}
				else
				{
					PlayerEncounter.Finish(true);
				}
			}
			PartyBase.MainParty.SetVisualAsDirty();
			Campaign.Current.CampaignInformationManager.OnGameLoaded();
			foreach (Settlement item in (List<Settlement>)(object)Settlement.All)
			{
				item.Party.SetLevelMaskIsDirty();
			}
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnGameLoadFinished();
			if (val2 != null)
			{
				val2.OnLoadingFinished();
			}
		}
		((MBGameManager)this).IsLoaded = true;
	}

	private void LaunchSandboxCharacterCreation()
	{
		CharacterCreationState val = Game.Current.GameStateManager.CreateState<CharacterCreationState>();
		Game.Current.GameStateManager.CleanAndPushState((GameState)(object)val, 0);
	}
}
