using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public class EditorSceneMissionManager : MBGameManager
{
	private string _missionName;

	private string _sceneName;

	private string _levels;

	private bool _forReplay;

	private string _replayFileName;

	private bool _isRecord;

	private float _startTime;

	private float _endTime;

	public EditorSceneMissionManager(string missionName, string sceneName, string levels, bool forReplay, string replayFileName, bool isRecord, float startTime, float endTime)
	{
		_missionName = missionName;
		_sceneName = sceneName;
		_levels = levels;
		_forReplay = forReplay;
		_replayFileName = replayFileName;
		_isRecord = isRecord;
		_startTime = startTime;
		_endTime = endTime;
	}

	protected override void DoLoadingForGameManager(GameManagerLoadingSteps gameManagerLoadingSteps, out GameManagerLoadingSteps nextStep)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		nextStep = (GameManagerLoadingSteps)(-1);
		switch ((int)gameManagerLoadingSteps)
		{
		case 0:
		{
			MBGameManager.LoadModuleData(false);
			MBDebug.Print("Game creating...", 0, (DebugColor)12, 17592186044416uL);
			MBGlobals.InitializeReferences();
			Game val;
			if (_forReplay)
			{
				val = Game.CreateGame((GameType)new EditorGame(), (GameManagerBase)(object)this);
			}
			else
			{
				Campaign val2 = new Campaign((CampaignGameMode)2);
				val = Game.CreateGame((GameType)val2, (GameManagerBase)(object)this);
				val2.SetLoadingParameters((GameLoadingType)0);
			}
			val.DoLoading();
			nextStep = (GameManagerLoadingSteps)1;
			break;
		}
		case 1:
		{
			bool flag = true;
			foreach (MBSubModuleBase item in (List<MBSubModuleBase>)(object)Module.CurrentModule.CollectSubModules())
			{
				flag = flag && item.DoLoading(Game.Current);
			}
			Campaign.Current.DefaultWeatherNodeDimension = 32;
			Campaign.Current.Models.MapWeatherModel.InitializeCaches();
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
		MBGlobals.InitializeReferences();
		if (!_forReplay)
		{
			Campaign.Current.InitializeGamePlayReferences();
		}
		Module.CurrentModule.StartMissionForEditorAux(_missionName, _sceneName, _levels, _forReplay, _replayFileName, _isRecord);
		MissionState.Current.MissionReplayStartTime = _startTime;
		MissionState.Current.MissionEndTime = _endTime;
	}
}
