using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Helpers;
using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.Objects;
using NavalDLC.View;
using NavalDLC.ViewModelCollection.Port;
using NavalDLC.ViewModelCollection.Port.PortScreenHandlers;
using SandBox.View;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI.Screens;

[GameStateScreen(typeof(PortState))]
public class GauntletPortScreen : ScreenBase, IGameStateListener, IChangeableScreen
{
	private struct CameraParameters
	{
		public float Azimuth;

		public float Inclination;

		public float Distance;

		public float Deviation;

		public CameraParameters(float azimuth, float inclination, float distance, float deviation)
		{
			Azimuth = azimuth;
			Inclination = inclination;
			Distance = distance;
			Deviation = deviation;
		}
	}

	private struct StaticCameraParameters
	{
		public float HorizontalRotationSensitivity;

		public float VerticalRotationSensitivity;

		public float ZoomSensitivity;

		public float SensitivityMappingMultiplier;

		public float DeviationSensitivityAtMinDistance;

		public float DeviationSensitivityAtMaxDistance;

		public float MinCameraInclination;

		public float MaxCameraInclinationAtMinDistance;

		public float MaxCameraInclinationAtMaxDistance;

		public float MinCameraDistance;

		public float MaxCameraDistance;

		public float MinCameraDistanceWhileInspectingPiece;

		public float CameraDeviationLimit;

		public float FocusDistanceAtMinDistance;

		public float FocusDistanceAtMaxDistance;

		public float ExtraHeightAtMinDistance;

		public float ExtraHeightAtMaxDistance;

		public StaticCameraParameters(float horizontalRotationSensitivity, float verticalRotationSensitivity, float zoomSensitivity, float sensitivityMappingMultiplier, float deviationSensitivityAtMinDistance, float deviationSensitivityAtMaxDistance, float minCameraInclination, float maxCameraInclinationAtMinDistance, float maxCameraInclinationAtMaxDistance, float minCameraDistance, float maxCameraDistance, float minCameraDistanceWhileInspectingPiece, float cameraDeviationLimit, float focusDistanceAtMinDistance, float focusDistanceAtMaxDistance, float extraHeightAtMinDistance, float extraHeightAtMaxDistance)
		{
			HorizontalRotationSensitivity = horizontalRotationSensitivity;
			VerticalRotationSensitivity = verticalRotationSensitivity;
			ZoomSensitivity = zoomSensitivity;
			SensitivityMappingMultiplier = sensitivityMappingMultiplier;
			DeviationSensitivityAtMinDistance = deviationSensitivityAtMinDistance;
			DeviationSensitivityAtMaxDistance = deviationSensitivityAtMaxDistance;
			MinCameraInclination = minCameraInclination;
			MaxCameraInclinationAtMinDistance = maxCameraInclinationAtMinDistance;
			MaxCameraInclinationAtMaxDistance = maxCameraInclinationAtMaxDistance;
			MinCameraDistance = minCameraDistance;
			MaxCameraDistance = maxCameraDistance;
			MinCameraDistanceWhileInspectingPiece = minCameraDistanceWhileInspectingPiece;
			CameraDeviationLimit = cameraDeviationLimit;
			FocusDistanceAtMinDistance = focusDistanceAtMinDistance;
			FocusDistanceAtMaxDistance = focusDistanceAtMaxDistance;
			ExtraHeightAtMinDistance = extraHeightAtMinDistance;
			ExtraHeightAtMaxDistance = extraHeightAtMaxDistance;
		}
	}

	private struct PortShipVisualInfo
	{
		public GameEntity VisualEntity;

		public Vec3 InitialPosition;

		public Vec3 VisualCenterPosition;

		public bool IsHidden;

		public PortShipVisualInfo(GameEntity visualEntity, Vec3 initialPosition, Vec3 visualCenterPosition, bool isHidden = false)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			VisualEntity = visualEntity;
			InitialPosition = initialPosition;
			VisualCenterPosition = visualCenterPosition;
			IsHidden = isHidden;
		}
	}

	private SceneLayer _sceneLayer;

	private Scene _scene;

	private readonly PortState _portState;

	private GauntletLayer _gauntletLayer;

	private PortVM _dataSource;

	private GameEntity _shipSpawnPositionEntity;

	private readonly Dictionary<Ship, PortShipVisualInfo> _shipVisualInfos;

	private PortShipVisualInfo _currentShipVisualInfo;

	private SpriteCategory _portCategory;

	private SpriteCategory _shipPiecesCategory;

	private SpriteCategory _clanCategory;

	private SpriteCategory _characterdeveloperCategory;

	private Camera _sceneCamera;

	private SoundEvent _underwaterSoundEvent;

	private IViewDataTracker _viewDataTracker;

	private readonly bool _isInSettlementPort;

	private bool _isInitialized;

	private bool _isControllingCamera;

	private int _framesToWaitAfterInit;

	private CameraParameters _targetCameraValues;

	private CameraParameters _currentCameraValues;

	private CameraParameters _previousCameraValues;

	private readonly CameraParameters _initialCameraValues;

	private readonly StaticCameraParameters _staticCameraValues;

	private Vec3 _currentCameraTargetPosition;

	private GameEntity _currentSelectedSlotCameraEntity;

	private Vec3 _shipForwardDirection = Vec3.Forward;

	private Vec3 _shipSideDirection = Vec3.Side;

	public GauntletPortScreen(PortState portState)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_portState = portState;
		_initialCameraValues = new CameraParameters(2.2f, 1.45f, 40f, 0f);
		_staticCameraValues = new StaticCameraParameters(0.2f, 0.1f, 0.015f, 1920f, 15f, 25f, MathF.PI / 4f, MathF.PI * 2f / 3f, MathF.PI * 19f / 36f, 15f, 50f, 5f, 15f, 50f, 3000f, 0f, 6f);
		_shipVisualInfos = new Dictionary<Ship, PortShipVisualInfo>();
		Settlement currentSettlement = Settlement.CurrentSettlement;
		_isInSettlementPort = currentSettlement != null && currentSettlement.HasPort;
	}

	protected override void OnInitialize()
	{
		((ScreenBase)this).OnInitialize();
		InformationManager.HideAllMessages();
	}

	protected override void OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		if (_sceneLayer.SceneView.ReadyToRender() && _sceneLayer.SceneView.CheckSceneReadyToRender())
		{
			if (!_isInitialized)
			{
				_scene.WaitWaterRendererCPUSimulation();
				InitializeView();
				_isInitialized = true;
				_framesToWaitAfterInit = 10;
			}
			_dataSource.OnTick(dt);
			_scene.Tick(dt);
			if (_framesToWaitAfterInit > 0)
			{
				_framesToWaitAfterInit--;
				return;
			}
			if (LoadingWindow.IsLoadingWindowActive)
			{
				LoadingWindow.DisableGlobalLoadingWindow();
				return;
			}
			TickSceneInput(dt);
			TickDataSourceInput();
		}
	}

	void IGameStateListener.OnActivate()
	{
	}

	void IGameStateListener.OnDeactivate()
	{
	}

	private void InitializeView()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected I4, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Expected O, but got Unknown
		_shipPiecesCategory = UIResourceManager.LoadSpriteCategory("ui_naval_ship_pieces");
		_portCategory = UIResourceManager.LoadSpriteCategory("ui_port");
		_clanCategory = UIResourceManager.LoadSpriteCategory("ui_clan");
		_characterdeveloperCategory = UIResourceManager.LoadSpriteCategory("ui_characterdeveloper");
		Campaign current = Campaign.Current;
		_viewDataTracker = ((current != null) ? current.GetCampaignBehavior<IViewDataTracker>() : null);
		PortScreenHandler portScreenHandler = null;
		PortScreenModes portScreenMode = _portState.PortScreenMode;
		switch ((int)portScreenMode)
		{
		case 1:
			portScreenHandler = new PortScreenRestrictedModeHandler(_portState.LeftOwner, _portState.RightOwner);
			break;
		case 2:
			portScreenHandler = new PortScreenTradeModeHandler(_portState.LeftOwner, _portState.RightOwner);
			break;
		case 3:
			portScreenHandler = new PortScreenLootModeHandler(GameTexts.FindText("str_loot", (string)null), _portState.RightOwner, _portState.LeftShips, _portState.RightShips);
			break;
		case 0:
			portScreenHandler = new PortScreenStoryModeHandler(_portState.LeftOwner, _portState.RightOwner);
			break;
		case 4:
			portScreenHandler = new PortScreenManageFleetModeHandler(GameTexts.FindText("str_port_discard_ship", (string)null), _portState.RightOwner, _portState.LeftShips, _portState.RightShips);
			break;
		case 5:
			portScreenHandler = new PortScreenManageOtherFleetModeHandler(_portState.LeftOwner);
			break;
		default:
			Debug.FailedAssert("Trying to initialize Port Screen with invalid PortScreenMode. Falling back to manage mode", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.GauntletUI\\Screens\\GauntletPortScreen.cs", "InitializeView", 194);
			portScreenHandler = new PortScreenManageFleetModeHandler(GameTexts.FindText("str_port_discard_ship", (string)null), _portState.RightOwner, _portState.LeftShips, _portState.RightShips);
			break;
		}
		_dataSource = new PortVM(portScreenHandler, _portState.PortScreenMode, OnShipSelected, OnRostersRefreshed, RefreshShipVisual, OnUpgradeSlotSelected);
		InitializeShipVisuals();
		_dataSource.SelectFirstAvailableRosterAndShip();
		_dataSource.IsNight = _scene.TimeOfDay <= 4f || _scene.TimeOfDay >= 20f;
		_gauntletLayer = new GauntletLayer("PortScreen", 10, false);
		_gauntletLayer.LoadMovie("PortScreen", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("PortHotKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_dataSource.SetSelectPreviousShipInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		_dataSource.SetSelectNextShipInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		_dataSource.SetSelectLeftRosterInputKey(HotKeyManager.GetCategory("PortHotKeyCategory").GetHotKey("SelectLeftRoster"));
		_dataSource.SetSelectRightRosterInputKey(HotKeyManager.GetCategory("PortHotKeyCategory").GetHotKey("SelectRightRoster"));
		_dataSource.AddGamepadCameraControlInputKey(((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("PortHotKeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "MovementAxisX")));
		_dataSource.AddGamepadCameraControlInputKey(((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("PortHotKeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "CameraAxisX")));
		_dataSource.AddGamepadCameraControlInputKey(HotKeyManager.GetCategory("PortHotKeyCategory").GetHotKey("ResetCamera"));
		_dataSource.SetGamepadToggleCameraInputKey(HotKeyManager.GetCategory("PortHotKeyCategory").GetHotKey("ToggleCameraMovement"));
		_dataSource.AddKeyboardMoveCameraInputKey(((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("PortHotKeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "MovementAxisY")).PositiveKey);
		_dataSource.AddKeyboardMoveCameraInputKey(((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("PortHotKeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "MovementAxisX")).NegativeKey);
		_dataSource.AddKeyboardMoveCameraInputKey(((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("PortHotKeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "MovementAxisY")).NegativeKey);
		_dataSource.AddKeyboardMoveCameraInputKey(((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("PortHotKeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "MovementAxisX")).PositiveKey);
		_dataSource.SetKeyboardRotateCameraInputKey(HotKeyManager.GetCategory("PortHotKeyCategory").GetHotKey("ToggleCameraMovement"));
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		ResetCamera(isInstant: true);
	}

	void IGameStateListener.OnInitialize()
	{
		LoadingWindow.EnableGlobalLoadingWindow();
		CreateScene();
		_isInitialized = false;
	}

	void IGameStateListener.OnFinalize()
	{
		if (_isInitialized)
		{
			_shipPiecesCategory.Unload();
			_portCategory.Unload();
			_clanCategory.Unload();
			_characterdeveloperCategory.Unload();
			((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			((ViewModel)_dataSource).OnFinalize();
			_gauntletLayer = null;
			_dataSource = null;
			if (_underwaterSoundEvent != null)
			{
				_underwaterSoundEvent.Release();
				_underwaterSoundEvent = null;
				SoundManager.SetGlobalParameter("isUnderwater", 0f);
			}
		}
		if (_sceneLayer != null && (NativeObject)(object)_scene != (NativeObject)null)
		{
			DestroyScene();
		}
	}

	private void CreateScene()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected O, but got Unknown
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		_scene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
		_scene.SetUseAdvancedWaterRendering(true);
		SceneInitializationData val = new SceneInitializationData
		{
			InitPhysicsWorld = true,
			InitFloraNodes = true
		};
		_scene.Read(_isInSettlementPort ? "prototype_port_scene_wide" : "scn_port", ref val, "");
		CampaignVec2 val2 = (_isInSettlementPort ? Settlement.CurrentSettlement.PortPosition : Campaign.Current.MainParty.Position);
		AtmosphereInfo atmosphereModel = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(val2);
		float num = MathF.Max(4f, ((Vec2)(ref atmosphereModel.NauticalInfo.WindVector)).Length);
		float waterStrength = MathF.Max(2f, num / 4f);
		_scene.EnableFixedTick();
		_scene.SetClothSimulationState(true);
		_scene.EnableInclusiveAsyncPhysx();
		_scene.SetWaterStrength(waterStrength);
		Scene scene = _scene;
		Vec2 val3 = num * (_isInSettlementPort ? (-Vec2.Side) : Vec2.Forward);
		scene.SetGlobalWindVelocity(ref val3);
		_scene.SetPhotoAtmosphereViaTod(atmosphereModel.TimeInfo.TimeOfDay, num > 20f);
		_sceneLayer = new SceneLayer(true, true);
		((ScreenLayer)_sceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("PortHotKeyCategory"));
		((ScreenLayer)_sceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_sceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		((ScreenLayer)_sceneLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		_sceneLayer.SceneView.SetScene(_scene);
		_sceneLayer.SceneView.SetSceneUsesShadows(true);
		_sceneLayer.SceneView.SetAcceptGlobalDebugRenderObjects(true);
		_sceneLayer.SceneView.SetRenderWithPostfx(true);
		_sceneLayer.SceneView.SetResolutionScaling(true);
		_shipSpawnPositionEntity = _scene.FindEntityWithName("ship_spawn_point");
		GameEntity shipSpawnPositionEntity = _shipSpawnPositionEntity;
		if (shipSpawnPositionEntity != null)
		{
			GameEntityPhysicsExtensions.SetPhysicsState(shipSpawnPositionEntity, false, true);
		}
		GameEntity shipSpawnPositionEntity2 = _shipSpawnPositionEntity;
		Vec3 shipForwardDirection;
		if (shipSpawnPositionEntity2 == null)
		{
			shipForwardDirection = Vec3.Forward;
		}
		else
		{
			MatrixFrame frame = shipSpawnPositionEntity2.GetFrame();
			val3 = ((Vec3)(ref frame.rotation.f)).AsVec2;
			shipForwardDirection = ((Vec2)(ref val3)).ToVec3(0f);
		}
		_shipForwardDirection = shipForwardDirection;
		_shipSideDirection = Vec3.CrossProduct(Vec3.Up, _shipForwardDirection);
		InitializeCamera();
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_sceneLayer);
	}

	private void InitializeCamera()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = _scene.FindEntityWithName("camera_position");
		GameEntityPhysicsExtensions.SetPhysicsState(val, false, true);
		_sceneCamera = Camera.CreateCamera();
		_sceneCamera.Frame = val.GetFrame();
		_sceneCamera.SetFovHorizontal(MathF.PI / 2f, Screen.AspectRatio, 0.1f, 2000f);
		ResetCamera(isInstant: true);
		UpdateCamera(1f);
		_sceneLayer.SetCamera(_sceneCamera);
	}

	private void DestroyScene()
	{
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_sceneLayer);
		_sceneLayer.ClearAll();
		_scene.WaitWaterRendererCPUSimulation();
		_scene.ClearAll();
		((NativeObject)_scene).ManualInvalidate();
		_scene = null;
		_shipSpawnPositionEntity = null;
		_shipVisualInfos.Clear();
		_sceneCamera = null;
	}

	private void InitializeShipVisuals()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		Vec3 origin = _shipSpawnPositionEntity.GetFrame().origin;
		int num = ((!_isInSettlementPort) ? (-((Collection<ShipItemVM>)(object)_dataSource.RightRoster.Ships).Count / 2) : 0);
		foreach (ShipItemVM item in (Collection<ShipItemVM>)(object)_dataSource.RightRoster.Ships)
		{
			SpawnShipVisual(item.Ship, origin + GetPositionOffsetForIndex(num, isOppositeSide: false), GetExtraRotationInRadiansForIndex(num, isOppositeSide: false));
			num++;
		}
		origin = ((!_isInSettlementPort) ? (origin + Vec3.Forward * 100f) : (origin - Vec3.Forward * 75f));
		num = ((!_isInSettlementPort) ? (-((Collection<ShipItemVM>)(object)_dataSource.LeftRoster.Ships).Count / 2) : 0);
		foreach (ShipItemVM item2 in (Collection<ShipItemVM>)(object)_dataSource.LeftRoster.Ships)
		{
			SpawnShipVisual(item2.Ship, origin + GetPositionOffsetForIndex(num, isOppositeSide: true), GetExtraRotationInRadiansForIndex(num, isOppositeSide: true));
			num++;
		}
	}

	private void SpawnShipVisual(Ship ship, Vec3 position, float rotation)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		List<ShipVisualSlotInfo> shipVisualSlotInfos = ship.GetShipVisualSlotInfos();
		GameEntity shipEntity = NavalDLCViewHelpers.ShipVisualHelper.GetShipEntity(ship, _scene, shipVisualSlotInfos, createPhysics: true);
		MatrixFrame frame = _shipSpawnPositionEntity.GetFrame();
		frame.origin = position;
		frame.origin.z = _scene.GetWaterLevelAtPosition(((Vec3)(ref frame.origin)).AsVec2, true, false) - shipEntity.GetFirstScriptOfType<NavalPhysics>().StabilitySubmergedHeightOfShip;
		((Mat3)(ref frame.rotation)).RotateAboutUp(rotation);
		GameEntityPhysicsExtensions.SetPhysicsState(shipEntity, true, false);
		shipEntity.SetFrame(ref frame, true);
		shipEntity.GetFirstScriptOfType<NavalPhysics>().SetAnchor(isAnchored: true, anchorInPlace: true);
		RotateOars(shipEntity);
		RotateSails(shipEntity);
		shipEntity.GetFirstScriptOfTypeRecursive<ShipWaterEffects>()?.EnableWakeAndParticles();
		_shipVisualInfos.Add(ship, new PortShipVisualInfo(shipEntity, frame.origin, frame.origin + GetVisualCenterOffsetForShip(shipEntity)));
	}

	private void RotateOars(GameEntity visualShip)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (GameEntity item in MBExtensions.CollectChildrenEntitiesWithTag(visualShip, "oar"))
		{
			MatrixFrame frame = item.GetFrame();
			((MatrixFrame)(ref frame)).Rotate(-MathF.PI / 3f, ref Vec3.Side);
			item.SetFrame(ref frame, true);
		}
	}

	private void RotateSails(GameEntity visualShip)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		ShipVisual firstScriptOfType = visualShip.GetFirstScriptOfType<ShipVisual>();
		if (firstScriptOfType == null)
		{
			return;
		}
		foreach (ScriptComponentBehavior sailVisual2 in firstScriptOfType.SailVisuals)
		{
			SailVisual sailVisual = sailVisual2 as SailVisual;
			if (sailVisual.Type == SailVisual.SailType.LateenSail)
			{
				MatrixFrame localFrame = sailVisual.SailYawRotationEntity.GetLocalFrame();
				localFrame.rotation = Mat3.Identity;
				((Mat3)(ref localFrame.rotation)).RotateAboutUp(0.87266463f);
				sailVisual.SailYawRotationEntity.SetLocalFrame(ref localFrame, false);
			}
		}
	}

	private Vec3 GetPositionOffsetForIndex(int i, bool isOppositeSide)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val;
		Vec3 val2;
		if (_isInSettlementPort)
		{
			val = Vec3.Forward * 45f * (float)(i % 4);
			val2 = Vec3.Side * -60f * (float)(i / 4);
		}
		else
		{
			val2 = Vec3.Side * -45f * (float)i;
			val = Vec3.Forward * -20f * (float)MathF.Abs(i);
		}
		if (isOppositeSide)
		{
			val *= -1f;
		}
		Vec3 val3 = (MBRandom.RandomFloatWithSeed((uint)i, (uint)(i + (isOppositeSide ? 1 : 0))) - 0.5f) * 8f * Vec3.Side + (MBRandom.RandomFloatWithSeed((uint)i, (uint)(i + (isOppositeSide ? 3 : 2))) - 0.5f) * 8f * Vec3.Forward;
		return val2 + val + val3;
	}

	private float GetExtraRotationInRadiansForIndex(int i, bool isOppositeSide)
	{
		return (MBRandom.RandomFloatWithSeed((uint)i, (uint)(i + (isOppositeSide ? 1 : 0))) - 0.5f) * 20f * (MathF.PI / 180f);
	}

	private Vec3 GetVisualCenterOffsetForShip(GameEntity shipEntity)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		GameEntity firstChildEntityWithTagRecursive = shipEntity.GetFirstChildEntityWithTagRecursive("body_mesh");
		MetaMesh val = ((firstChildEntityWithTagRecursive != null) ? firstChildEntityWithTagRecursive.GetMetaMesh(0) : null);
		if ((NativeObject)(object)val != (NativeObject)null)
		{
			BoundingBox boundingBox = val.GetBoundingBox();
			return new Vec3(((Vec3)(ref boundingBox.center)).AsVec2, MathF.Lerp(((Vec3)(ref boundingBox.center)).Z, ((Vec3)(ref boundingBox.max)).Z, 0.4f, 1E-05f), -1f);
		}
		return new Vec3(0f, 0f, 2.5f, -1f);
	}

	private void RecalculateShipVisibilities()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Ship, PortShipVisualInfo> item in _shipVisualInfos.ToList())
		{
			Ship key = item.Key;
			bool flag = ShouldShipBeHidden(key);
			if (item.Value.IsHidden != flag)
			{
				_shipVisualInfos[key] = new PortShipVisualInfo(item.Value.VisualEntity, item.Value.InitialPosition, item.Value.VisualCenterPosition, flag);
			}
			item.Value.VisualEntity.SetVisibilityExcludeParents(!flag);
		}
	}

	private bool ShouldShipBeHidden(Ship ship)
	{
		if (!((IEnumerable<ShipItemVM>)_dataSource.LeftRoster.Ships).Any((ShipItemVM x) => x.Ship == ship))
		{
			return !((IEnumerable<ShipItemVM>)_dataSource.RightRoster.Ships).Any((ShipItemVM x) => x.Ship == ship);
		}
		return false;
	}

	private void RecalculateShipPositions()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		Vec3 origin = _shipSpawnPositionEntity.GetFrame().origin;
		int num = ((!_isInSettlementPort) ? (-((Collection<ShipItemVM>)(object)_dataSource.RightRoster.Ships).Count / 2) : 0);
		foreach (ShipItemVM item in (Collection<ShipItemVM>)(object)_dataSource.RightRoster.Ships)
		{
			RecalculateShipPosition(item.Ship, origin + GetPositionOffsetForIndex(num, isOppositeSide: false), GetExtraRotationInRadiansForIndex(num, isOppositeSide: false));
			num++;
		}
		origin = ((!_isInSettlementPort) ? (origin + Vec3.Forward * 100f) : (origin - Vec3.Forward * 75f));
		num = ((!_isInSettlementPort) ? (-((Collection<ShipItemVM>)(object)_dataSource.LeftRoster.Ships).Count / 2) : 0);
		foreach (ShipItemVM item2 in (Collection<ShipItemVM>)(object)_dataSource.LeftRoster.Ships)
		{
			RecalculateShipPosition(item2.Ship, origin + GetPositionOffsetForIndex(num, isOppositeSide: true), GetExtraRotationInRadiansForIndex(num, isOppositeSide: true));
			num++;
		}
	}

	private void RecalculateShipPosition(Ship ship, Vec3 position, float rotation)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		PortShipVisualInfo portShipVisualInfo = _shipVisualInfos[ship];
		if (((Vec3)(ref portShipVisualInfo.InitialPosition)).AsVec2 != ((Vec3)(ref position)).AsVec2)
		{
			GameEntity visualEntity = portShipVisualInfo.VisualEntity;
			MatrixFrame frame = _shipSpawnPositionEntity.GetFrame();
			frame.origin = position;
			frame.origin.z = _scene.GetWaterLevelAtPosition(((Vec3)(ref frame.origin)).AsVec2, true, false) - visualEntity.GetFirstScriptOfType<NavalPhysics>().StabilitySubmergedHeightOfShip;
			((Mat3)(ref frame.rotation)).RotateAboutUp(rotation);
			visualEntity.GetFirstScriptOfType<NavalPhysics>().SetAnchor(isAnchored: false);
			visualEntity.SetFrame(ref frame, true);
			visualEntity.GetFirstScriptOfType<NavalPhysics>().SetAnchor(isAnchored: true, anchorInPlace: true);
			_shipVisualInfos[ship] = new PortShipVisualInfo(visualEntity, frame.origin, frame.origin + GetVisualCenterOffsetForShip(visualEntity), portShipVisualInfo.IsHidden);
			if (_currentShipVisualInfo.VisualEntity == visualEntity)
			{
				_currentShipVisualInfo = _shipVisualInfos[ship];
			}
		}
	}

	private void RefreshShipVisuals()
	{
		foreach (ShipItemVM item in (List<ShipItemVM>)(object)_dataSource.AllShips)
		{
			RefreshShipVisual(item);
		}
	}

	private void RefreshShipVisual(ShipItemVM shipItem)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		Ship ship = shipItem.Ship;
		List<ShipVisualSlotInfo> list = new List<ShipVisualSlotInfo>();
		foreach (ShipUpgradeSlotBaseVM item3 in (Collection<ShipUpgradeSlotBaseVM>)(object)shipItem.Upgrades.UpgradeSlots)
		{
			if (item3 is ShipUpgradeSlotVM)
			{
				string shipSlotTag = item3.ShipSlotTag;
				ShipUpgradePieceVM obj = item3.SelectedPiece as ShipUpgradePieceVM;
				list.Add(new ShipVisualSlotInfo(shipSlotTag, ((obj != null) ? obj.Piece.SlotPrefabChildTagId : null) ?? string.Empty));
			}
			else if (item3 is ShipFigureheadSlotVM)
			{
				string shipSlotTag2 = item3.ShipSlotTag;
				ShipFigureheadVM obj2 = item3.SelectedPiece as ShipFigureheadVM;
				list.Add(new ShipVisualSlotInfo(shipSlotTag2, ((obj2 != null) ? ((MBObjectBase)obj2.Figurehead).StringId : null) ?? string.Empty));
			}
		}
		uint item;
		uint item2;
		Banner shipBanner;
		if (((Collection<ShipItemVM>)(object)_dataSource.LeftRoster.Ships).Contains(shipItem))
		{
			ValueTuple<uint, uint> sailColors = ShipHelper.GetSailColors(_dataSource.LeftRoster.Owner);
			item = sailColors.Item1;
			item2 = sailColors.Item2;
			shipBanner = ShipHelper.GetShipBanner(_dataSource.LeftRoster.Owner);
		}
		else
		{
			ValueTuple<uint, uint> sailColors2 = ShipHelper.GetSailColors(_dataSource.RightRoster.Owner);
			item = sailColors2.Item1;
			item2 = sailColors2.Item2;
			shipBanner = ShipHelper.GetShipBanner(_dataSource.RightRoster.Owner);
		}
		NavalDLCViewHelpers.ShipVisualHelper.RefreshShipVisuals(_shipVisualInfos[ship].VisualEntity, list, item, item2, shipBanner, shipItem.CurrentHp / shipItem.MaxHp);
	}

	private void OnShipSelected(Ship shipItem)
	{
		if (shipItem == null)
		{
			return;
		}
		if (_shipVisualInfos.ContainsKey(shipItem))
		{
			_currentShipVisualInfo = _shipVisualInfos[shipItem];
			foreach (KeyValuePair<Ship, PortShipVisualInfo> shipVisualInfo in _shipVisualInfos)
			{
				if (shipVisualInfo.Value.VisualEntity != _currentShipVisualInfo.VisualEntity)
				{
					shipVisualInfo.Value.VisualEntity.AddBodyFlags((BodyFlags)65536, true);
				}
				else
				{
					shipVisualInfo.Value.VisualEntity.RemoveBodyFlags((BodyFlags)65536, true);
				}
			}
		}
		else
		{
			Debug.FailedAssert("Selected ship item's visual has not been spawned!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.GauntletUI\\Screens\\GauntletPortScreen.cs", "OnShipSelected", 636);
		}
		_targetCameraValues.Deviation = _initialCameraValues.Deviation;
	}

	private void OnRostersRefreshed()
	{
		if (_dataSource != null)
		{
			RecalculateShipVisibilities();
			RecalculateShipPositions();
			RefreshShipVisuals();
		}
	}

	private void OnUpgradeSlotSelected()
	{
		if (_dataSource.IsAnyUpgradeSlotSelected)
		{
			string shipSlotTag = _dataSource.SelectedUpgradeSlot.ShipSlotTag;
			string slotTypeId = _dataSource.SelectedUpgradeSlot.SlotTypeId;
			if (_currentSelectedSlotCameraEntity == (GameEntity)null)
			{
				_previousCameraValues = _currentCameraValues;
			}
			_currentSelectedSlotCameraEntity = _currentShipVisualInfo.VisualEntity.GetFirstChildEntityWithTagRecursive(shipSlotTag + "_point");
			if (_currentSelectedSlotCameraEntity == (GameEntity)null)
			{
				Debug.FailedAssert("Slot camera point entity not found!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.GauntletUI\\Screens\\GauntletPortScreen.cs", "OnUpgradeSlotSelected", 671);
				return;
			}
			_targetCameraValues.Azimuth = GetCameraAzimuthForSlot();
			_targetCameraValues.Inclination = GetCameraInclinationForSlotType(slotTypeId);
			_targetCameraValues.Distance = GetCameraDistanceForSlotType(slotTypeId);
			_targetCameraValues.Deviation = 0f;
		}
		else
		{
			FreeCameraFromUpgradeSlot();
		}
	}

	private void FreeCameraFromUpgradeSlot()
	{
		if (_currentSelectedSlotCameraEntity != (GameEntity)null)
		{
			_currentSelectedSlotCameraEntity = null;
			_targetCameraValues = _previousCameraValues;
		}
	}

	private float GetCameraAzimuthForSlot()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = GetStableSlotPosition() - _shipSideDirection;
		Vec3 val2 = _currentShipVisualInfo.VisualCenterPosition - _shipForwardDirection * _staticCameraValues.CameraDeviationLimit;
		Vec3 val3 = _currentShipVisualInfo.VisualCenterPosition + _shipForwardDirection * _staticCameraValues.CameraDeviationLimit;
		Vec3 closestPointOnLineSegmentToPoint = MBMath.GetClosestPointOnLineSegmentToPoint(ref val2, ref val3, ref val);
		Vec3 val4 = val - closestPointOnLineSegmentToPoint;
		if (MBMath.ApproximatelyEqualsTo(MathF.Abs(Vec3.DotProduct(((Vec3)(ref val4)).NormalizedCopy(), Vec3.Up)), 1f, 1E-05f))
		{
			return _initialCameraValues.Azimuth;
		}
		return MathF.Atan2(val4.y, val4.x);
	}

	private float GetCameraInclinationForSlotType(string slotType)
	{
		return 1.3962634f;
	}

	private float GetCameraDistanceForSlotType(string slotType)
	{
		if (slotType == "hull" || slotType == "sail")
		{
			return _initialCameraValues.Distance;
		}
		return _staticCameraValues.MinCameraDistance;
	}

	private void TickDataSourceInput()
	{
		if (IsHotKeyReleasedInAnyLayer("Confirm"))
		{
			if (!_dataSource.IsConfirmDisabled)
			{
				UISoundsHelper.PlayUISound("event:/ui/port/confirm_ship");
				_dataSource.ExecuteConfirm();
			}
		}
		else if (IsHotKeyReleasedInAnyLayer("Exit"))
		{
			if (_dataSource.IsAnyUpgradeSlotSelected)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.SelectedUpgradeSlot.ExecuteDeselect();
			}
			else
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ExecuteCancel(showCancelInquiry: true);
			}
		}
		else if (IsGameKeyPressedInAnyLayer(45))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteCancel(showCancelInquiry: true);
		}
		else if (IsHotKeyReleasedInAnyLayer("Reset"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteReset();
		}
		else if (IsHotKeyReleasedInAnyLayer("SwitchToPreviousTab"))
		{
			if (!_isControllingCamera && _dataSource.ExecuteSelectPreviousShip())
			{
				UISoundsHelper.PlayUISound("event:/ui/port/choose_ship");
			}
		}
		else if (IsHotKeyReleasedInAnyLayer("SwitchToNextTab"))
		{
			if (!_isControllingCamera && _dataSource.ExecuteSelectNextShip())
			{
				UISoundsHelper.PlayUISound("event:/ui/port/choose_ship");
			}
		}
		else if (IsHotKeyReleasedInAnyLayer("SelectLeftRoster"))
		{
			if (!_isControllingCamera && !_dataSource.LeftRoster.IsSelected && _dataSource.LeftRoster.HasAnyShips)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.LeftRoster.ExecuteSelectRoster();
			}
		}
		else if (IsHotKeyReleasedInAnyLayer("SelectRightRoster") && !_isControllingCamera && !_dataSource.RightRoster.IsSelected && _dataSource.RightRoster.HasAnyShips)
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.RightRoster.ExecuteSelectRoster();
		}
	}

	private bool IsHotKeyPressedInAnyLayer(string hotkey)
	{
		if (!((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed(hotkey))
		{
			return ((ScreenLayer)_sceneLayer).Input.IsHotKeyPressed(hotkey);
		}
		return true;
	}

	private bool IsHotKeyReleasedInAnyLayer(string hotkey)
	{
		if (!((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased(hotkey))
		{
			return ((ScreenLayer)_sceneLayer).Input.IsHotKeyReleased(hotkey);
		}
		return true;
	}

	private bool IsGameKeyPressedInAnyLayer(int gameKey)
	{
		if (!((ScreenLayer)_gauntletLayer).Input.IsGameKeyPressed(gameKey))
		{
			return ((ScreenLayer)_sceneLayer).Input.IsGameKeyPressed(gameKey);
		}
		return true;
	}

	private void TickSceneInput(float dt)
	{
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_061f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		if (((ScreenLayer)_sceneLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == _gauntletLayer)
		{
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
			((ScreenLayer)_sceneLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_sceneLayer);
		}
		else if (!((ScreenLayer)_sceneLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == _sceneLayer)
		{
			((ScreenLayer)_sceneLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_sceneLayer);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
		bool flag = ((ScreenLayer)_sceneLayer).IsHitThisFrame || ((ScreenLayer)_gauntletLayer).IsHitThisFrame;
		if (Input.IsGamepadActive)
		{
			if (flag && IsHotKeyPressedInAnyLayer("ToggleCameraMovement"))
			{
				_isControllingCamera = !_isControllingCamera;
			}
		}
		else if (((ScreenLayer)_sceneLayer).Input.IsHotKeyPressed("ToggleCameraMovement"))
		{
			_isControllingCamera = true;
		}
		else if (((ScreenLayer)_sceneLayer).Input.IsHotKeyReleased("ToggleCameraMovement"))
		{
			_isControllingCamera = false;
		}
		_dataSource.IsControllingCamera = _isControllingCamera;
		_dataSource.CanToggleCamera = flag;
		PortVM dataSource = _dataSource;
		IViewDataTracker viewDataTracker = _viewDataTracker;
		dataSource.IsMapBarExtended = viewDataTracker != null && viewDataTracker.GetMapBarExtendedState();
		_dataSource.CanUseGamepadInputs = Input.IsGamepadActive;
		_dataSource.CanUseKeyboardInputs = !Input.IsGamepadActive && ((ScreenLayer)_sceneLayer).IsHitThisFrame;
		if (_isControllingCamera)
		{
			MBWindowManager.DontChangeCursorPos();
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		}
		else
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		}
		if (((ScreenLayer)_sceneLayer).Input.IsHotKeyPressed("ResetCamera"))
		{
			ResetCamera(isInstant: false);
		}
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(((ScreenLayer)_sceneLayer).Input.GetNormalizedMouseMoveX() * 1920f, ((ScreenLayer)_sceneLayer).Input.GetNormalizedMouseMoveY() * 1080f);
		float num = 0f;
		if (Input.IsGamepadActive)
		{
			if (_isControllingCamera)
			{
				float inputValue = ((ScreenLayer)_sceneLayer).Input.GetGameKeyAxis("MovementAxisY") * -1f;
				NormalizeControllerInputForDeadZone(ref inputValue, 0.1f);
				if (((ScreenLayer)_sceneLayer).Input.IsHotKeyDown("ControllerZoomOut"))
				{
					inputValue += 1f;
				}
				if (((ScreenLayer)_sceneLayer).Input.IsHotKeyDown("ControllerZoomIn"))
				{
					inputValue -= 1f;
				}
				inputValue = MathF.Clamp(inputValue, -1f, 1f);
				num = inputValue * _staticCameraValues.ZoomSensitivity * _staticCameraValues.SensitivityMappingMultiplier * dt;
			}
		}
		else
		{
			float num2 = ((ScreenLayer)_sceneLayer).Input.GetDeltaMouseScroll() * -1f;
			float num3 = ((ScreenLayer)_sceneLayer).Input.GetGameKeyAxis("MovementAxisY") * -1f;
			num = num2 * _staticCameraValues.ZoomSensitivity + num3 * _staticCameraValues.ZoomSensitivity * _staticCameraValues.SensitivityMappingMultiplier * dt;
		}
		_targetCameraValues.Distance = MathF.Clamp(_targetCameraValues.Distance + num, GetTargetMinDistance(), _staticCameraValues.MaxCameraDistance);
		float num4;
		if (Input.IsGamepadActive)
		{
			float inputValue2 = (_isControllingCamera ? (((ScreenLayer)_sceneLayer).Input.GetGameKeyAxis("CameraAxisX") * -1f) : 0f);
			NormalizeControllerInputForDeadZone(ref inputValue2, 0.1f);
			num4 = inputValue2 * _staticCameraValues.HorizontalRotationSensitivity * ((ScreenLayer)_sceneLayer).Input.GetMouseSensitivity() * _staticCameraValues.SensitivityMappingMultiplier * dt;
		}
		else
		{
			num4 = (_isControllingCamera ? (val.x * -1f) : 0f) * _staticCameraValues.HorizontalRotationSensitivity * ((ScreenLayer)_sceneLayer).Input.GetMouseSensitivity();
		}
		_targetCameraValues.Azimuth = MBMath.WrapAngle(_targetCameraValues.Azimuth + num4 * (MathF.PI / 180f));
		float num5;
		if (Input.IsGamepadActive)
		{
			float inputValue3 = (_isControllingCamera ? ((ScreenLayer)_sceneLayer).Input.GetGameKeyAxis("CameraAxisY") : 0f);
			NormalizeControllerInputForDeadZone(ref inputValue3, 0.1f);
			num5 = inputValue3 * _staticCameraValues.VerticalRotationSensitivity * ((ScreenLayer)_sceneLayer).Input.GetMouseSensitivity() * _staticCameraValues.SensitivityMappingMultiplier * dt;
		}
		else
		{
			num5 = (_isControllingCamera ? (val.y * -1f) : 0f) * _staticCameraValues.VerticalRotationSensitivity * ((ScreenLayer)_sceneLayer).Input.GetMouseSensitivity();
		}
		if (NativeConfig.InvertMouse)
		{
			num5 *= -1f;
		}
		float num6 = (_targetCameraValues.Distance - GetTargetMinDistance()) / (_staticCameraValues.MaxCameraDistance - GetTargetMinDistance());
		float num7 = MathF.Lerp(_staticCameraValues.MaxCameraInclinationAtMinDistance, _staticCameraValues.MaxCameraInclinationAtMaxDistance, num6, 1E-05f);
		_targetCameraValues.Inclination = MathF.Clamp(_targetCameraValues.Inclination + num5 * (MathF.PI / 180f), _staticCameraValues.MinCameraInclination, num7);
		float num8 = 0f;
		if (Input.IsGamepadActive)
		{
			if (_isControllingCamera)
			{
				num8 = ((ScreenLayer)_sceneLayer).Input.GetGameKeyAxis("MovementAxisX");
				NormalizeControllerInputForDeadZone(ref num8, 0.1f);
				if (((ScreenLayer)_sceneLayer).Input.IsHotKeyDown("ControllerDeviateRight"))
				{
					num8 += 1f;
				}
				if (((ScreenLayer)_sceneLayer).Input.IsHotKeyDown("ControllerDeviateLeft"))
				{
					num8 -= 1f;
				}
				num8 = MathF.Clamp(num8, -1f, 1f);
			}
		}
		else
		{
			num8 = ((ScreenLayer)_sceneLayer).Input.GetGameKeyAxis("MovementAxisX");
		}
		float num9 = MathF.Lerp(_staticCameraValues.DeviationSensitivityAtMinDistance, _staticCameraValues.DeviationSensitivityAtMaxDistance, num6, 1E-05f);
		float num10 = MathF.Clamp(MathF.Pow(MathF.Cos(_currentCameraValues.Azimuth - Vec3.AngleBetweenTwoVectors(Vec3.Forward, _shipForwardDirection)), 3f) * 2f, -1f, 1f);
		float num11 = num8 * num9 * dt * num10;
		_targetCameraValues.Deviation = MathF.Clamp(_targetCameraValues.Deviation + num11, 0f - _staticCameraValues.CameraDeviationLimit, _staticCameraValues.CameraDeviationLimit);
		if (num11 != 0f)
		{
			FreeCameraFromUpgradeSlot();
		}
		UpdateCamera(dt);
	}

	bool IChangeableScreen.AnyUnsavedChanges()
	{
		if (_isInitialized)
		{
			return _dataSource.AreThereAnyChanges();
		}
		return false;
	}

	bool IChangeableScreen.CanChangesBeApplied()
	{
		return !_dataSource.IsConfirmDisabled;
	}

	void IChangeableScreen.ApplyChanges()
	{
		_dataSource.ExecuteConfirm();
	}

	void IChangeableScreen.ResetChanges()
	{
		_dataSource.ExecuteReset();
	}

	private void UpdateCamera(float dt)
	{
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		float num = MathF.Min(1f, 10f * dt);
		float amount = MathF.Min(1f, 5f * dt);
		float maxAmount = ((_currentSelectedSlotCameraEntity != (GameEntity)null) ? (MathF.PI * 2f * dt) : (100f * dt));
		_currentCameraValues.Azimuth = LerpAngleWithMax(_currentCameraValues.Azimuth, _targetCameraValues.Azimuth, num, maxAmount);
		_currentCameraValues.Inclination = LerpAngleWithMax(_currentCameraValues.Inclination, _targetCameraValues.Inclination, num, maxAmount);
		_currentCameraValues.Deviation = MathF.Lerp(_currentCameraValues.Deviation, _targetCameraValues.Deviation, num, 1E-05f);
		_currentCameraValues.Distance = MathF.Lerp(_currentCameraValues.Distance, _targetCameraValues.Distance, num, 1E-05f);
		float num2 = (_currentCameraValues.Distance - GetTargetMinDistance()) / (_staticCameraValues.MaxCameraDistance - GetTargetMinDistance());
		num2 = MathF.Clamp(num2, 0f, 1f);
		_currentCameraTargetPosition = LerpVec3WithMax(_currentCameraTargetPosition, GetCameraTargetPosition(), amount, 500f * dt);
		Vec3 currentCameraTargetPosition = _currentCameraTargetPosition;
		currentCameraTargetPosition += _currentCameraValues.Deviation * _shipForwardDirection;
		float num3 = AnimationInterpolation.Ease((Type)2, (Function)0, num2);
		currentCameraTargetPosition.z += MathF.Lerp(_staticCameraValues.ExtraHeightAtMinDistance, _staticCameraValues.ExtraHeightAtMaxDistance, num3, 1E-05f);
		HandleCameraCollision(currentCameraTargetPosition);
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = currentCameraTargetPosition;
		identity.origin.x += _currentCameraValues.Distance * MathF.Sin(_currentCameraValues.Inclination) * MathF.Cos(_currentCameraValues.Azimuth);
		identity.origin.y += _currentCameraValues.Distance * MathF.Sin(_currentCameraValues.Inclination) * MathF.Sin(_currentCameraValues.Azimuth);
		identity.origin.z += _currentCameraValues.Distance * MathF.Cos(_currentCameraValues.Inclination);
		_sceneCamera.LookAt(identity.origin, currentCameraTargetPosition, Vec3.Up);
		_sceneCamera.SetFovHorizontal(MathF.PI / 2f, Screen.AspectRatio, 0.1f, 2000f);
		_scene.SetDepthOfFieldFocus(_currentCameraValues.Distance);
		float num4 = AnimationInterpolation.Ease((Type)1, (Function)2, num2);
		float num5 = MathF.Lerp(_staticCameraValues.FocusDistanceAtMinDistance, _staticCameraValues.FocusDistanceAtMaxDistance, num4, 1E-05f);
		_scene.SetDepthOfFieldParameters(num5, num5, true);
		_sceneLayer.SetCamera(_sceneCamera);
		SoundManager.SetListenerFrame(_sceneCamera.Frame);
		HandleIsCameraUnderwater();
		HandleShipEntityVisibilities();
	}

	private float LerpAngleWithMax(float current, float target, float amount, float maxAmount)
	{
		float num = MathF.AngleLerp(current, target, amount, 1E-05f);
		float num2 = (num - current) % (MathF.PI * 2f);
		float num3 = 2f * num2 % (MathF.PI * 2f) - num2;
		if (MathF.Abs(num3) > maxAmount)
		{
			num = MathF.AngleClamp(current + (float)MathF.Sign(num3) * maxAmount);
		}
		return num;
	}

	private Vec3 LerpVec3WithMax(Vec3 current, Vec3 target, float amount, float maxAmount)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.Lerp(current, target, amount);
		if (((Vec3)(ref val)).Distance(current) > maxAmount)
		{
			Vec3 val2 = val - current;
			val = current + ((Vec3)(ref val2)).NormalizedCopy() * maxAmount;
		}
		return val;
	}

	private Vec3 GetCameraTargetPosition()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (_currentShipVisualInfo.VisualEntity != (GameEntity)null)
		{
			if (_currentSelectedSlotCameraEntity != (GameEntity)null)
			{
				return GetStableSlotPosition();
			}
			return _currentShipVisualInfo.VisualCenterPosition;
		}
		return _shipSpawnPositionEntity.GetFrame().origin + new Vec3(0f, 0f, 2.5f, -1f);
	}

	private Vec3 GetStableSlotPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		return _currentSelectedSlotCameraEntity.GlobalPosition - _currentShipVisualInfo.VisualEntity.GlobalPosition + _currentShipVisualInfo.InitialPosition;
	}

	private void NormalizeControllerInputForDeadZone(ref float inputValue, float controllerDeadZone)
	{
		if (MathF.Abs(inputValue) < controllerDeadZone)
		{
			inputValue = 0f;
		}
		else
		{
			inputValue = (inputValue - (float)MathF.Sign(inputValue) * controllerDeadZone) / (1f - controllerDeadZone);
		}
	}

	private void HandleCameraCollision(Vec3 cameraTargetPos)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		if (_scene.RayCastForClosestEntityOrTerrain(_sceneCamera.Position, cameraTargetPos, ref num, 0.01f, (BodyFlags)79617))
		{
			float num2 = _currentCameraValues.Distance - num + 1f;
			if (_currentCameraValues.Distance < num2)
			{
				_currentCameraValues.Distance = num2;
				_targetCameraValues.Distance = num2;
			}
		}
	}

	private void HandleIsCameraUnderwater()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = _sceneCamera.Position;
		float waterLevelAtPosition = _scene.GetWaterLevelAtPosition(((Vec3)(ref position)).AsVec2, true, false);
		if (((Vec3)(ref position)).Z < waterLevelAtPosition)
		{
			if (_underwaterSoundEvent == null)
			{
				_underwaterSoundEvent = SoundManager.CreateEvent("snapshot:/Underwater", _scene);
				_underwaterSoundEvent.Play();
				SoundManager.SetGlobalParameter("isUnderwater", 1f);
			}
		}
		else if (_underwaterSoundEvent != null)
		{
			_underwaterSoundEvent.Release();
			_underwaterSoundEvent = null;
			SoundManager.SetGlobalParameter("isUnderwater", 0f);
		}
	}

	private void ResetCamera(bool isInstant)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (isInstant)
		{
			_currentCameraTargetPosition = GetCameraTargetPosition();
			_currentCameraValues = _initialCameraValues;
		}
		_targetCameraValues = _initialCameraValues;
	}

	private void HandleShipEntityVisibilities()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Ship, PortShipVisualInfo> shipVisualInfo in _shipVisualInfos)
		{
			GameEntity visualEntity = shipVisualInfo.Value.VisualEntity;
			bool isHidden = shipVisualInfo.Value.IsHidden;
			if (visualEntity == _currentShipVisualInfo.VisualEntity)
			{
				visualEntity.SetVisibilityExcludeParents(!isHidden);
				continue;
			}
			float num = 6f;
			ValueTuple<Vec3, Vec3> valueTuple = visualEntity.ComputeGlobalPhysicsBoundingBoxMinMax();
			Vec3 item = valueTuple.Item1;
			Vec3 item2 = valueTuple.Item2;
			float num2 = MathF.Min(((Vec3)(ref item)).X, ((Vec3)(ref item2)).X) - num;
			float num3 = MathF.Max(((Vec3)(ref item)).X, ((Vec3)(ref item2)).X) + num;
			float num4 = MathF.Min(((Vec3)(ref item)).Y, ((Vec3)(ref item2)).Y) - num;
			float num5 = MathF.Max(((Vec3)(ref item)).Y, ((Vec3)(ref item2)).Y) + num;
			float num6 = MathF.Min(((Vec3)(ref item)).Z, ((Vec3)(ref item2)).Z) - num;
			float num7 = MathF.Max(((Vec3)(ref item)).Z, ((Vec3)(ref item2)).Z) + num;
			Vec3 position = _sceneCamera.Position;
			int num8;
			if (((Vec3)(ref position)).X > num2)
			{
				position = _sceneCamera.Position;
				if (((Vec3)(ref position)).X < num3)
				{
					position = _sceneCamera.Position;
					if (((Vec3)(ref position)).Y > num4)
					{
						position = _sceneCamera.Position;
						if (((Vec3)(ref position)).Y < num5)
						{
							position = _sceneCamera.Position;
							if (((Vec3)(ref position)).Z > num6)
							{
								position = _sceneCamera.Position;
								num8 = ((((Vec3)(ref position)).Z < num7) ? 1 : 0);
								goto IL_0194;
							}
						}
					}
				}
			}
			num8 = 0;
			goto IL_0194;
			IL_0194:
			bool flag = (byte)num8 != 0;
			visualEntity.SetVisibilityExcludeParents(!isHidden && !flag);
		}
	}

	private float GetTargetMinDistance()
	{
		if (!(_currentSelectedSlotCameraEntity != (GameEntity)null))
		{
			return _staticCameraValues.MinCameraDistance;
		}
		return _staticCameraValues.MinCameraDistanceWhileInspectingPiece;
	}
}
