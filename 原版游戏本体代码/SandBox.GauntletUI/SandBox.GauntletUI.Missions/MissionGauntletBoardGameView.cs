using System;
using SandBox.BoardGames.MissionLogics;
using SandBox.ViewModelCollection.BoardGame;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(BoardGameView))]
public class MissionGauntletBoardGameView : MissionView, IBoardGameHandler
{
	private BoardGameVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private GauntletMovieIdentifier _gauntletMovie;

	private GameEntity _cameraHolder;

	private SpriteCategory _spriteCategory;

	private bool _missionMouseVisibilityState;

	private InputUsageMask _missionInputRestrictions;

	public MissionBoardGameLogic _missionBoardGameHandler { get; private set; }

	public Camera Camera { get; private set; }

	public MissionGauntletBoardGameView()
	{
		base.ViewOrderPriority = 2;
	}

	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("BoardGameHotkeyCategory"));
	}

	public override void OnMissionScreenActivate()
	{
		((MissionView)this).OnMissionScreenActivate();
		_missionBoardGameHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionBoardGameLogic>();
		if (_missionBoardGameHandler != null)
		{
			_missionBoardGameHandler.Handler = (IBoardGameHandler)(object)this;
		}
	}

	public override bool OnEscape()
	{
		return _dataSource != null;
	}

	void IBoardGameHandler.Activate()
	{
		_dataSource.Activate();
	}

	void IBoardGameHandler.SwitchTurns()
	{
		_dataSource?.SwitchTurns();
	}

	void IBoardGameHandler.DiceRoll(int roll)
	{
		_dataSource?.DiceRoll(roll);
	}

	void IBoardGameHandler.Install()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		_spriteCategory = UIResourceManager.LoadSpriteCategory("ui_boardgame");
		_dataSource = new BoardGameVM();
		_dataSource.SetRollDiceKey(HotKeyManager.GetCategory("BoardGameHotkeyCategory").GetHotKey("BoardGameRollDice"));
		_gauntletLayer = new GauntletLayer("MissionBoardGame", base.ViewOrderPriority, false);
		_gauntletMovie = _gauntletLayer.LoadMovie("BoardGame", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		_cameraHolder = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("camera_holder");
		CreateCamera();
		if (_cameraHolder == (GameEntity)null)
		{
			_cameraHolder = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("camera_holder");
		}
		if ((NativeObject)(object)Camera == (NativeObject)null)
		{
			CreateCamera();
		}
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		_missionMouseVisibilityState = ((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).InputRestrictions.MouseVisibility;
		_missionInputRestrictions = ((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).InputRestrictions.InputUsageMask;
		((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).IsFocusLayer = true;
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesStateAndDeactivateOthers(new string[2] { "SceneLayer", "MissionBoardGame" }, true);
		ScreenManager.TrySetFocus((ScreenLayer)(object)((MissionView)this).MissionScreen.SceneLayer);
		SetStaticCamera();
	}

	void IBoardGameHandler.Uninstall()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (_dataSource != null)
		{
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
		}
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).InputRestrictions.SetInputRestrictions(_missionMouseVisibilityState, _missionInputRestrictions);
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletMovie = null;
		_gauntletLayer = null;
		Camera = null;
		_cameraHolder = null;
		((MissionView)this).MissionScreen.CustomCamera = null;
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesStateAndToggleOthers(new string[1] { "MissionBoardGame" }, false);
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesState(new string[1] { "SceneLayer" }, true);
		_spriteCategory.Unload();
	}

	private bool IsHotkeyPressedInAnyLayer(string hotkeyID)
	{
		SceneLayer sceneLayer = ((MissionView)this).MissionScreen.SceneLayer;
		bool num = sceneLayer != null && ((ScreenLayer)sceneLayer).Input.IsHotKeyPressed(hotkeyID);
		GauntletLayer gauntletLayer = _gauntletLayer;
		bool flag = gauntletLayer != null && ((ScreenLayer)gauntletLayer).Input.IsHotKeyPressed(hotkeyID);
		return num || flag;
	}

	private bool IsHotkeyDownInAnyLayer(string hotkeyID)
	{
		SceneLayer sceneLayer = ((MissionView)this).MissionScreen.SceneLayer;
		bool num = sceneLayer != null && ((ScreenLayer)sceneLayer).Input.IsHotKeyDown(hotkeyID);
		GauntletLayer gauntletLayer = _gauntletLayer;
		bool flag = gauntletLayer != null && ((ScreenLayer)gauntletLayer).Input.IsHotKeyDown(hotkeyID);
		return num || flag;
	}

	private bool IsGameKeyReleasedInAnyLayer(string hotKeyID)
	{
		SceneLayer sceneLayer = ((MissionView)this).MissionScreen.SceneLayer;
		bool num = sceneLayer != null && ((ScreenLayer)sceneLayer).Input.IsHotKeyReleased(hotKeyID);
		GauntletLayer gauntletLayer = _gauntletLayer;
		bool flag = gauntletLayer != null && ((ScreenLayer)gauntletLayer).Input.IsHotKeyReleased(hotKeyID);
		return num || flag;
	}

	private void CreateCamera()
	{
		Camera = Camera.CreateCamera();
		if (_cameraHolder != (GameEntity)null)
		{
			Camera.Entity = _cameraHolder;
		}
		Camera.SetFovVertical(MathF.PI / 4f, 1.7777778f, 0.01f, 3000f);
	}

	private void SetStaticCamera()
	{
		if (_cameraHolder != (GameEntity)null && Camera.Entity != (GameEntity)null)
		{
			((MissionView)this).MissionScreen.CustomCamera = Camera;
		}
		else
		{
			Debug.FailedAssert("[DEBUG]Camera entities are null.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Missions\\MissionGauntletBoardGameView.cs", "SetStaticCamera", 189);
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		MissionBoardGameLogic missionBoardGameHandler = _missionBoardGameHandler;
		if (missionBoardGameHandler == null || !missionBoardGameHandler.IsGameInProgress)
		{
			return;
		}
		MissionScreen missionScreen = ((MissionView)this).MissionScreen;
		if (missionScreen != null && missionScreen.IsPhotoModeEnabled)
		{
			return;
		}
		((MissionView)this).OnMissionScreenTick(dt);
		if (_gauntletLayer != null && _dataSource != null)
		{
			if (IsHotkeyPressedInAnyLayer("Exit"))
			{
				_dataSource.ExecuteForfeit();
			}
			else if (IsHotkeyPressedInAnyLayer("BoardGameRollDice") && _dataSource.IsGameUsingDice)
			{
				_dataSource.ExecuteRoll();
			}
		}
		if (_missionBoardGameHandler.Board != null)
		{
			Vec3 rayBegin = default(Vec3);
			Vec3 rayEnd = default(Vec3);
			((MissionView)this).MissionScreen.ScreenPointToWorldRay(((MissionView)this).Input.GetMousePositionRanged(), ref rayBegin, ref rayEnd);
			_missionBoardGameHandler.Board.SetUserRay(rayBegin, rayEnd);
		}
	}

	public override void OnMissionScreenFinalize()
	{
		if (_dataSource != null)
		{
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
		}
		_gauntletLayer = null;
		_gauntletMovie = null;
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override void OnPhotoModeActivated()
	{
		((MissionView)this).OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		((MissionView)this).OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}
}
