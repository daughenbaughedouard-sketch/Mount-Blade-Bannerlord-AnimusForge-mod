using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens;

[GameStateScreen(typeof(MissionState))]
public class MissionScreen : ScreenBase, IMissionSystemHandler, IGameStateListener, IMissionScreen, IMissionListener, IChatLogHandlerScreen
{
	public delegate void OnSpectateAgentDelegate(Agent followedAgent);

	public delegate List<Agent> GatherCustomAgentListToSpectateDelegate(Agent forcedAgentToInclude);

	public const int LoadingScreenFramesLeftInitial = 15;

	private const float LookUpLimit = MathF.PI * 5f / 14f;

	private const float LookDownLimit = -1.3659099f;

	public const float FirstPersonNearClippingDistance = 0.065f;

	public const float ThirdPersonNearClippingDistance = 0.1f;

	public const float FarClippingDistance = 12500f;

	private const float HoldTimeForCameraToggle = 0.5f;

	public const float MinCameraAddedDistance = 0.7f;

	public const float MinCameraDistanceHardLimit = 0.48f;

	public const float DefaultViewAngle = 65f;

	public const float MaxCameraAddedDistance = 2.4f;

	private const int _cheatTimeSpeedRequestId = 1121;

	private const string AttackerCameraEntityTag = "strategyCameraAttacker";

	private const string DefenderCameraEntityTag = "strategyCameraDefender";

	private const string CameraHeightLimiterTag = "camera_height_limiter";

	public Func<BasicCharacterObject> GetSpectatedCharacter;

	private GatherCustomAgentListToSpectateDelegate _gatherCustomAgentListToSpectate;

	private float _cameraRayCastOffset;

	private bool _forceCanZoom;

	public bool AllowInputWithCustomCamera;

	private readonly Vec3[] _cameraNearPlanePoints = (Vec3[])(object)new Vec3[4];

	private readonly Vec3[] _cameraBoxPoints = (Vec3[])(object)new Vec3[8];

	private Vec3 _cameraTarget;

	private float _cameraBearingDelta;

	private float _cameraElevationDelta;

	private float _cameraSpecialTargetAddedBearing;

	private float _cameraSpecialCurrentAddedBearing;

	private float _cameraSpecialTargetAddedElevation;

	private float _cameraSpecialCurrentAddedElevation;

	private Vec3 _cameraSpecialTargetPositionToAdd;

	private Vec3 _cameraSpecialCurrentPositionToAdd;

	private float _cameraSpecialTargetDistanceToAdd;

	private float _cameraSpecialCurrentDistanceToAdd;

	private bool _cameraAddSpecialMovement;

	private bool _cameraAddSpecialPositionalMovement;

	private bool _cameraApplySpecialMovementsInstantly;

	private float _cameraSpecialCurrentFOV;

	private float _cameraSpecialTargetFOV;

	private float _cameraTargetAddedHeight;

	private float _cameraDeploymentHeightToAdd;

	private float _lastCameraAddedDistance;

	private float _cameraAddedElevation;

	private float _cameraHeightLimit;

	private float _cameraShakeStartTime;

	private float _cameraShakeCurrentTimeWithFrequency;

	private float _cameraShakeIntensity;

	private float _currentViewBlockingBodyCoeff;

	private float _targetViewBlockingBodyCoeff;

	private bool _applySmoothTransitionToVirtualEyeCamera;

	private Vec3 _cameraSpeed;

	private float _cameraSpeedMultiplier;

	private bool _cameraSmoothMode;

	private bool _fixCamera;

	private int _shiftSpeedMultiplier = 3;

	private bool _tickEditor;

	private bool _playerDeploymentCancelled;

	private bool _isDeactivated;

	private bool _zoomToggled;

	private float _zoomAmount;

	private float _cameraToggleStartTime = float.MaxValue;

	private bool _displayingDialog;

	private MissionMainAgentController _missionMainAgentController;

	private ICameraModeLogic _missionCameraModeLogic;

	private MissionLobbyComponent _missionLobbyComponent;

	private readonly List<object> _objectsWithActiveRadialMenu;

	private bool _isPlayerAgentAdded = true;

	private bool _isRenderingStarted;

	private bool _onSceneRenderingStartedCalled;

	private int _loadingScreenFramesLeft = 15;

	private bool _resetDraggingMode;

	private bool _rightButtonDraggingMode;

	private Vec2 _clickedPositionPixel = Vec2.Zero;

	private Agent _agentToFollowOverride;

	private Agent _lastFollowedAgent;

	private bool _isGamepadActive;

	private readonly MissionViewsContainer _missionViewsContainer;

	private MissionState _missionState;

	public bool LockCameraMovement { get; private set; }

	public OrderFlag OrderFlag { get; set; }

	public Camera CombatCamera { get; private set; }

	public Camera CustomCamera { get; set; }

	public float CameraBearing { get; set; }

	public float MaxCameraZoom { get; private set; } = 1f;

	public float CameraElevation { get; private set; }

	public float CameraResultDistanceToTarget { get; private set; }

	public float CameraViewAngle { get; private set; }

	public bool IsPhotoModeEnabled { get; private set; }

	public bool IsConversationActive { get; private set; }

	public bool IsDeploymentActive => (int)Mission.Mode == 6;

	public SceneLayer SceneLayer { get; private set; }

	public SceneView SceneView
	{
		get
		{
			SceneLayer sceneLayer = SceneLayer;
			if (sceneLayer == null)
			{
				return null;
			}
			return sceneLayer.SceneView;
		}
	}

	public Mission Mission { get; private set; }

	public bool IsCheatGhostMode { get; set; }

	public bool IsRadialMenuActive => _objectsWithActiveRadialMenu.Count > 0;

	public IInputContext InputManager => Mission.InputManager;

	private bool IsOrderMenuOpen => Mission.IsOrderMenuOpen;

	private bool IsTransferMenuOpen => Mission.IsTransferMenuOpen;

	public Agent LastFollowedAgent
	{
		get
		{
			return _lastFollowedAgent;
		}
		private set
		{
			if (_lastFollowedAgent == value)
			{
				return;
			}
			Agent lastFollowedAgent = _lastFollowedAgent;
			_lastFollowedAgent = value;
			NetworkCommunicator myPeer = GameNetwork.MyPeer;
			MissionPeer val = ((myPeer != null) ? PeerExtensions.GetComponent<MissionPeer>(myPeer) : null);
			if (GameNetwork.IsMyPeerReady)
			{
				if (val != null)
				{
					val.FollowedAgent = _lastFollowedAgent;
				}
				else
				{
					Debug.FailedAssert("MyPeer.IsSynchronized but myMissionPeer == null", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Screens\\MissionScreen.cs", "LastFollowedAgent", 219);
				}
			}
			ResetMaxCameraZoom();
			if (lastFollowedAgent != null)
			{
				this.OnSpectateAgentFocusOut?.Invoke(lastFollowedAgent);
			}
			if (_lastFollowedAgent != null)
			{
				this.OnSpectateAgentFocusIn?.Invoke(_lastFollowedAgent);
			}
			if (_lastFollowedAgent == _agentToFollowOverride)
			{
				_agentToFollowOverride = null;
			}
		}
	}

	public IAgentVisual LastFollowedAgentVisuals { get; set; }

	public override bool MouseVisible => ScreenManager.GetMouseVisibility();

	public bool PhotoModeRequiresMouse { get; private set; }

	public bool IsFocusLost { get; private set; }

	public bool IsMissionTickable
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			if (((ScreenBase)this).IsActive && Mission != null)
			{
				if ((int)Mission.CurrentState != 2)
				{
					return Mission.MissionEnded;
				}
				return true;
			}
			return false;
		}
	}

	public event OnSpectateAgentDelegate OnSpectateAgentFocusIn;

	public event OnSpectateAgentDelegate OnSpectateAgentFocusOut;

	public MissionScreen(MissionState missionState)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		missionState.Handler = (IMissionSystemHandler)(object)this;
		((View)new SceneLayer(true, true).SceneView).SetEnable(false);
		_resetDraggingMode = false;
		_missionState = missionState;
		Mission = missionState.CurrentMission;
		CombatCamera = Camera.CreateCamera();
		_objectsWithActiveRadialMenu = new List<object>();
		_missionViewsContainer = new MissionViewsContainer();
	}

	static MissionScreen()
	{
	}

	protected override void OnInitialize()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		MBDebug.Print("-------MissionScreen-OnInitialize", 0, (DebugColor)12, 17592186044416uL);
		((ScreenBase)this).OnInitialize();
		Module.CurrentModule.SkinsXMLHasChanged += OnSkinsXMLChanged;
		CameraViewAngle = 65f;
		_cameraTarget = new Vec3(0f, 0f, 10f, -1f);
		CameraBearing = 0f;
		CameraElevation = -0.2f;
		_cameraBearingDelta = 0f;
		_cameraElevationDelta = 0f;
		_cameraSpecialTargetAddedBearing = 0f;
		_cameraSpecialCurrentAddedBearing = 0f;
		_cameraSpecialTargetAddedElevation = 0f;
		_cameraSpecialCurrentAddedElevation = 0f;
		_cameraSpecialTargetPositionToAdd = Vec3.Zero;
		_cameraSpecialCurrentPositionToAdd = Vec3.Zero;
		_cameraSpecialTargetDistanceToAdd = 0f;
		_cameraSpecialCurrentDistanceToAdd = 0f;
		_cameraSpecialCurrentFOV = 65f;
		_cameraSpecialTargetFOV = 65f;
		_cameraAddedElevation = 0f;
		_cameraTargetAddedHeight = 0f;
		_cameraDeploymentHeightToAdd = 0f;
		_lastCameraAddedDistance = 0f;
		CameraResultDistanceToTarget = 0f;
		_cameraSpeed = Vec3.Zero;
		_cameraSpeedMultiplier = 1f;
		_cameraHeightLimit = 0f;
		_cameraAddSpecialMovement = false;
		_cameraAddSpecialPositionalMovement = false;
		_cameraApplySpecialMovementsInstantly = false;
		_currentViewBlockingBodyCoeff = 1f;
		_targetViewBlockingBodyCoeff = 1f;
		_cameraSmoothMode = false;
		CustomCamera = null;
		InformationManager.HideAllMessages();
	}

	protected virtual void InitializeMissionView()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		_missionState.Paused = false;
		SceneLayer = new SceneLayer(true, true);
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ScoreboardHotKeyCategory"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("CombatHotKeyCategory"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Cheats"));
		Mission.InputManager = (IInputContext)(object)((ScreenLayer)SceneLayer).Input;
		((ScreenBase)this).AddLayer((ScreenLayer)(object)SceneLayer);
		SceneView.SetScene(Mission.Scene);
		SceneView.SetSceneUsesShadows(true);
		SceneView.SetAcceptGlobalDebugRenderObjects(true);
		SceneView.SetResolutionScaling(true);
		_missionMainAgentController = Mission.GetMissionBehavior<MissionMainAgentController>();
		_missionLobbyComponent = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();
		ref ICameraModeLogic missionCameraModeLogic = ref _missionCameraModeLogic;
		MissionBehavior? obj = ((IEnumerable<MissionBehavior>)Mission.MissionBehaviors).FirstOrDefault((Func<MissionBehavior, bool>)((MissionBehavior b) => b is ICameraModeLogic));
		missionCameraModeLogic = (ICameraModeLogic)(object)((obj is ICameraModeLogic) ? obj : null);
		foreach (MissionBehavior missionBehavior in Mission.MissionBehaviors)
		{
			if (missionBehavior is MissionView missionView)
			{
				missionView.OnMissionScreenInitialize();
			}
		}
		Mission.AgentVisualCreator = (IAgentVisualCreator)(object)new AgentVisualsCreator();
		Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
	}

	protected override void OnActivate()
	{
		((ScreenBase)this).OnActivate();
		ActivateLoadingScreen();
		if (Mission != null && Mission.MissionEnded && ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			ScreenManager.TopScreen.DeactivateAllLayers();
			((View)missionScreen.SceneView).SetEnable(false);
		}
	}

	protected override void OnResume()
	{
		((ScreenBase)this).OnResume();
		if (Mission != null && Mission.MissionEnded && ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			ScreenManager.TopScreen.DeactivateAllLayers();
			((View)missionScreen.SceneView).SetEnable(false);
		}
	}

	public override void OnFocusChangeOnGameWindow(bool focusGained)
	{
		((ScreenBase)this).OnFocusChangeOnGameWindow(focusGained);
		if (!LoadingWindow.IsLoadingWindowActive && !InformationManager.IsAnyInquiryActive())
		{
			Mission mission = Mission;
			List<MissionBehavior> list = ((mission == null) ? null : (from v in mission.MissionBehaviors?.Where((MissionBehavior v) => v is MissionView)
				orderby ((MissionView)(object)v).ViewOrderPriority
				select v).ToList());
			if (list != null)
			{
				for (int num = 0; num < list.Count; num++)
				{
					(list[num] as MissionView).OnFocusChangeOnGameWindow(focusGained);
				}
			}
		}
		IsFocusLost = !focusGained;
	}

	public void SetOrderFlagVisibility(bool value)
	{
		if (OrderFlag != null)
		{
			OrderFlag.IsVisible = value;
		}
	}

	public string GetFollowText()
	{
		if (LastFollowedAgent == null)
		{
			return "";
		}
		return LastFollowedAgent.Name;
	}

	public string GetFollowPartyText()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		if (LastFollowedAgent != null)
		{
			TextObject val = new TextObject("{=xsC8Ierj}({BATTLE_COMBATANT})", (Dictionary<string, object>)null);
			val.SetTextVariable("BATTLE_COMBATANT", LastFollowedAgent.Origin.BattleCombatant.Name);
			return ((object)val).ToString();
		}
		return "";
	}

	public bool SetDisplayDialog(bool value)
	{
		bool result = _displayingDialog != value;
		_displayingDialog = value;
		return result;
	}

	bool IMissionScreen.GetDisplayDialog()
	{
		return _displayingDialog;
	}

	public bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		if (ScreenFadeController.IsFadeActive)
		{
			return false;
		}
		Mission mission = Mission;
		List<MissionBehavior> list = ((mission == null) ? null : (from v in mission.MissionBehaviors?.Where((MissionBehavior v) => v is MissionView)
			orderby ((MissionView)(object)v).ViewOrderPriority
			select v).ToList());
		if (list != null)
		{
			foreach (MissionBehavior item in list)
			{
				if (!(item as MissionView).IsOpeningEscapeMenuOnFocusChangeAllowed())
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsPhotoModeAllowed()
	{
		Mission mission = Mission;
		foreach (MissionBehavior item in (mission == null) ? null : mission.MissionBehaviors?.Where((MissionBehavior v) => v is MissionView).ToList())
		{
			if (!(item as MissionView).IsPhotoModeAllowed())
			{
				return false;
			}
		}
		return true;
	}

	public void SetExtraCameraParameters(bool newForceCanZoom, float newCameraRayCastStartingPointOffset)
	{
		_forceCanZoom = newForceCanZoom;
		_cameraRayCastOffset = newCameraRayCastStartingPointOffset;
	}

	public void SetCustomAgentListToSpectateGatherer(GatherCustomAgentListToSpectateDelegate gatherer)
	{
		_gatherCustomAgentListToSpectate = gatherer;
	}

	public void UpdateFreeCamera(MatrixFrame frame)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		CombatCamera.Frame = frame;
		Vec3 val = -frame.rotation.u;
		CameraBearing = ((Vec3)(ref val)).RotationZ;
		Vec3 val2 = default(Vec3);
		((Vec3)(ref val2))._002Ector(0f, 0f, 1f, -1f);
		CameraElevation = MathF.Acos(Vec3.DotProduct(val2, val)) - MathF.PI / 2f;
	}

	protected override void OnFrameTick(float dt)
	{
		if (SceneLayer != null)
		{
			bool flag = MBDebug.IsErrorReportModeActive();
			if (flag)
			{
				_missionState.Paused = MBDebug.IsErrorReportModePauseMission();
			}
			if (((ScreenBase)this).DebugInput.IsHotKeyPressed("MissionScreenHotkeyFixCamera"))
			{
				_fixCamera = !_fixCamera;
			}
			flag = flag || _fixCamera;
			if (IsPhotoModeEnabled)
			{
				flag = flag || PhotoModeRequiresMouse;
			}
			((ScreenLayer)SceneLayer).InputRestrictions.SetMouseVisibility(flag);
		}
		if (Mission == null)
		{
			return;
		}
		if (IsMissionTickable)
		{
			_missionViewsContainer.ForEach(delegate(MissionView missionView)
			{
				missionView.OnMissionScreenTick(dt);
			});
		}
		HandleInputs();
	}

	private void ActivateMissionView()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		MBDebug.Print("-------MissionScreen-OnActivate", 0, (DebugColor)12, 17592186044416uL);
		Mission.OnMainAgentChanged += new OnMainAgentChangedDelegate(Mission_OnMainAgentChanged);
		Mission.OnBeforeAgentRemoved += new OnBeforeAgentRemovedDelegate(Mission_OnBeforeAgentRemoved);
		Mission.OnCameraShakeTriggered += new OnCameraShakeTriggeredDelegate(MissionOnCameraShakeTriggered);
		_cameraBearingDelta = 0f;
		_cameraElevationDelta = 0f;
		SetCameraFrameToMapView();
		CheckForUpdateCamera(1E-05f);
		Mission.ResetFirstThirdPersonView();
		if (MBEditor.EditModeEnabled && MBEditor.IsEditModeOn)
		{
			MBEditor.EnterEditMissionMode(Mission);
		}
		_missionViewsContainer.ForEach(delegate(MissionView missionView)
		{
			missionView.OnMissionScreenActivate();
		});
	}

	private void Mission_OnMainAgentChanged(Agent oldAgent)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		if (oldAgent != null)
		{
			oldAgent.OnMainAgentWieldedItemChange = (OnMainAgentWieldedItemChangeDelegate)Delegate.Remove((Delegate?)(object)oldAgent.OnMainAgentWieldedItemChange, (Delegate?)new OnMainAgentWieldedItemChangeDelegate(OnMainAgentWeaponChanged));
		}
		if (Mission.MainAgent != null)
		{
			Agent mainAgent = Mission.MainAgent;
			mainAgent.OnMainAgentWieldedItemChange = (OnMainAgentWieldedItemChangeDelegate)Delegate.Combine((Delegate?)(object)mainAgent.OnMainAgentWieldedItemChange, (Delegate?)new OnMainAgentWieldedItemChangeDelegate(OnMainAgentWeaponChanged));
			_isPlayerAgentAdded = true;
		}
		ResetMaxCameraZoom();
	}

	private void Mission_OnBeforeAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		if (affectedAgent == _agentToFollowOverride)
		{
			_agentToFollowOverride = null;
		}
		else if (affectedAgent == Mission.MainAgent)
		{
			_agentToFollowOverride = affectorAgent;
		}
	}

	private void MissionOnCameraShakeTriggered(in Vec3 position, float radius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position2 = CombatCamera.Position;
		if (((Vec3)(ref position2)).DistanceSquared(position) < radius * radius)
		{
			_cameraShakeStartTime = Game.Current.ApplicationTime;
			position2 = CombatCamera.Position;
			_cameraShakeIntensity = Math.Min(1.25f - ((Vec3)(ref position2)).Distance(position) / radius, 1f);
		}
	}

	public void OnMainAgentWeaponChanged()
	{
		ResetMaxCameraZoom();
	}

	private void ResetMaxCameraZoom()
	{
		if (LastFollowedAgent == null || LastFollowedAgent != Mission.MainAgent)
		{
			MaxCameraZoom = 1f;
		}
		else
		{
			MaxCameraZoom = ((Mission.Current != null) ? MathF.Max(1f, Mission.Current.GetMainAgentMaxCameraZoom()) : 1f);
		}
	}

	protected override void OnDeactivate()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		((ScreenBase)this).OnDeactivate();
		MBDebug.Print("-------MissionScreen-OnDeactivate", 0, (DebugColor)12, 17592186044416uL);
		if (Mission != null)
		{
			if (Mission.MainAgent != null)
			{
				Agent mainAgent = Mission.MainAgent;
				mainAgent.OnMainAgentWieldedItemChange = (OnMainAgentWieldedItemChangeDelegate)Delegate.Remove((Delegate?)(object)mainAgent.OnMainAgentWieldedItemChange, (Delegate?)new OnMainAgentWieldedItemChangeDelegate(OnMainAgentWeaponChanged));
			}
			Mission.OnMainAgentChanged -= new OnMainAgentChangedDelegate(Mission_OnMainAgentChanged);
			Mission.OnBeforeAgentRemoved -= new OnBeforeAgentRemovedDelegate(Mission_OnBeforeAgentRemoved);
			Mission.OnCameraShakeTriggered -= new OnCameraShakeTriggeredDelegate(MissionOnCameraShakeTriggered);
			_missionViewsContainer.ForEach(delegate(MissionView missionView)
			{
				missionView.OnMissionScreenDeactivate();
			});
			_isRenderingStarted = false;
			_loadingScreenFramesLeft = 15;
		}
	}

	protected override void OnFinalize()
	{
		MBDebug.Print("-------MissionScreen-OnFinalize", 0, (DebugColor)12, 17592186044416uL);
		Module.CurrentModule.SkinsXMLHasChanged -= OnSkinsXMLChanged;
		LoadingWindow.EnableGlobalLoadingWindow();
		if (Mission != null)
		{
			Mission.InputManager = null;
		}
		Mission = null;
		OrderFlag = null;
		SceneLayer = null;
		_missionMainAgentController = null;
		CombatCamera = null;
		CustomCamera = null;
		_missionState = null;
		((ScreenBase)this).OnFinalize();
	}

	private IEnumerable<MissionBehavior> AddDefaultMissionBehaviorsTo(Mission mission, IEnumerable<MissionBehavior> behaviors)
	{
		List<MissionBehavior> list = new List<MissionBehavior>();
		IEnumerable<MissionBehavior> collection = ViewCreatorManager.CreateDefaultMissionBehaviors(mission);
		list.AddRange(collection);
		return behaviors.Concat(list);
	}

	private void OnSkinsXMLChanged()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected I4, but got Unknown
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			item.EquipItemsFromSpawnEquipment(true, false, false, 0);
			item.UpdateAgentProperties();
			item.AgentVisuals.UpdateSkeletonScale((int)item.SpawnEquipment.BodyDeformType);
		}
	}

	private void OnSceneRenderingStarted()
	{
		LoadingWindow.DisableGlobalLoadingWindow();
		Utilities.SetScreenTextRenderingState(true);
		_missionViewsContainer.ForEach(delegate(MissionView missionView)
		{
			missionView.OnSceneRenderingStarted();
		});
	}

	[CommandLineArgumentFunction("fix_camera_toggle", "mission")]
	public static string ToggleFixedMissionCamera(List<string> strings)
	{
		if (ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			SetFixedMissionCameraActive(!missionScreen._fixCamera);
		}
		return "Done";
	}

	public static void SetFixedMissionCameraActive(bool active)
	{
		if (ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			missionScreen._fixCamera = active;
			((ScreenLayer)missionScreen.SceneLayer).InputRestrictions.SetMouseVisibility(missionScreen._fixCamera);
		}
	}

	[CommandLineArgumentFunction("set_shift_camera_speed", "mission")]
	public static string SetShiftCameraSpeed(List<string> strings)
	{
		if (ScreenManager.TopScreen is MissionScreen missionScreen)
		{
			if (strings.Count > 0 && int.TryParse(strings[0], out var result))
			{
				missionScreen._shiftSpeedMultiplier = result;
				return "Done";
			}
			return "Current multiplier is " + missionScreen._shiftSpeedMultiplier;
		}
		return "No Mission Available";
	}

	[CommandLineArgumentFunction("set_camera_position", "mission")]
	public static string SetCameraPosition(List<string> strings)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!GameNetwork.IsSessionActive)
		{
			if (strings.Count < 3)
			{
				return "You need to enter 3 arguments.";
			}
			List<float> list = new List<float>();
			for (int i = 0; i < strings.Count; i++)
			{
				if (float.TryParse(strings[i], out var result))
				{
					list.Add(result);
					continue;
				}
				return "Argument " + (i + 1) + " is not valid.";
			}
			if (ScreenManager.TopScreen is MissionScreen missionScreen)
			{
				missionScreen.IsCheatGhostMode = true;
				missionScreen.LastFollowedAgent = null;
				missionScreen.CombatCamera.Position = new Vec3(list[0], list[1], list[2], -1f);
				return "Camera position has been set to: " + strings[0] + ", " + strings[1] + ", " + strings[2];
			}
			return "Mission screen not found.";
		}
		return "Does not work on multiplayer.";
	}

	private void CheckForUpdateCamera(float dt)
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if (_fixCamera && !IsPhotoModeEnabled)
		{
			return;
		}
		if ((NativeObject)(object)CustomCamera != (NativeObject)null)
		{
			if (_zoomAmount > 0f)
			{
				_zoomAmount = MBMath.ClampFloat(_zoomAmount, 0f, 1f);
				float num = 37f / MaxCameraZoom;
				CameraViewAngle = MBMath.Lerp(Mission.GetFirstPersonFov(), num, _zoomAmount, 0.005f);
				CustomCamera.SetFovVertical(_cameraSpecialCurrentFOV * (CameraViewAngle / 65f) * (MathF.PI / 180f), Screen.AspectRatio, 0.065f, 12500f);
			}
			CombatCamera.FillParametersFrom(CustomCamera);
			if (CustomCamera.Entity != (GameEntity)null)
			{
				MatrixFrame globalFrame = CustomCamera.Entity.GetGlobalFrame();
				((Mat3)(ref globalFrame.rotation)).MakeUnit();
				CombatCamera.Frame = globalFrame;
			}
			SceneView.SetCamera(CombatCamera);
			SoundManager.SetListenerFrame(CombatCamera.Frame);
			return;
		}
		bool flag = false;
		foreach (MissionBehavior missionBehavior in Mission.MissionBehaviors)
		{
			if (missionBehavior is MissionView missionView)
			{
				flag = flag || missionView.UpdateOverridenCamera(dt);
			}
		}
		if (!flag)
		{
			UpdateCamera(dt);
		}
	}

	private void UpdateDragData()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (_resetDraggingMode)
		{
			_rightButtonDraggingMode = false;
			_resetDraggingMode = false;
		}
		else if (((ScreenLayer)SceneLayer).Input.IsKeyReleased((InputKey)225))
		{
			_resetDraggingMode = true;
		}
		else if (((ScreenLayer)SceneLayer).Input.IsKeyPressed((InputKey)225))
		{
			_clickedPositionPixel = ((ScreenLayer)SceneLayer).Input.GetMousePositionPixel();
		}
		else if (((ScreenLayer)SceneLayer).Input.IsKeyDown((InputKey)225) && !((ScreenLayer)SceneLayer).Input.IsKeyReleased((InputKey)225))
		{
			Vec2 mousePositionPixel = ((ScreenLayer)SceneLayer).Input.GetMousePositionPixel();
			if (((Vec2)(ref mousePositionPixel)).DistanceSquared(_clickedPositionPixel) > 10f && !_rightButtonDraggingMode)
			{
				_rightButtonDraggingMode = true;
			}
		}
	}

	private void UpdateCamera(float dt)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Invalid comparison between Unknown and I4
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_1392: Unknown result type (might be due to invalid IL or missing references)
		//IL_1397: Unknown result type (might be due to invalid IL or missing references)
		//IL_134c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1351: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Invalid comparison between Unknown and I4
		//IL_13cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_13d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c72: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c84: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c88: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c99: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ca9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cb6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cbc: Invalid comparison between Unknown and I4
		//IL_0b1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b20: Invalid comparison between Unknown and I4
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_14b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_14b6: Invalid comparison between Unknown and I4
		//IL_13e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_13e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cca: Invalid comparison between Unknown and I4
		//IL_0b28: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2e: Invalid comparison between Unknown and I4
		//IL_14be: Unknown result type (might be due to invalid IL or missing references)
		//IL_14c4: Invalid comparison between Unknown and I4
		//IL_1435: Unknown result type (might be due to invalid IL or missing references)
		//IL_143a: Unknown result type (might be due to invalid IL or missing references)
		//IL_17fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1802: Invalid comparison between Unknown and I4
		//IL_2bac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d10: Unknown result type (might be due to invalid IL or missing references)
		//IL_180a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1810: Invalid comparison between Unknown and I4
		//IL_1703: Unknown result type (might be due to invalid IL or missing references)
		//IL_1709: Invalid comparison between Unknown and I4
		//IL_16cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c09: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c0f: Invalid comparison between Unknown and I4
		//IL_0cf5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cfb: Invalid comparison between Unknown and I4
		//IL_0d28: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d36: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b65: Invalid comparison between Unknown and I4
		//IL_197f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1984: Unknown result type (might be due to invalid IL or missing references)
		//IL_14f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_14fb: Invalid comparison between Unknown and I4
		//IL_1717: Unknown result type (might be due to invalid IL or missing references)
		//IL_1722: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c17: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c1d: Invalid comparison between Unknown and I4
		//IL_0b7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b86: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1778: Unknown result type (might be due to invalid IL or missing references)
		//IL_177b: Invalid comparison between Unknown and I4
		//IL_10a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_10bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_10c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_10cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_10de: Unknown result type (might be due to invalid IL or missing references)
		//IL_10e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1117: Unknown result type (might be due to invalid IL or missing references)
		//IL_111c: Unknown result type (might be due to invalid IL or missing references)
		//IL_111e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1120: Unknown result type (might be due to invalid IL or missing references)
		//IL_1122: Unknown result type (might be due to invalid IL or missing references)
		//IL_1127: Unknown result type (might be due to invalid IL or missing references)
		//IL_1129: Unknown result type (might be due to invalid IL or missing references)
		//IL_112b: Unknown result type (might be due to invalid IL or missing references)
		//IL_112d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0faa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fd7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fda: Invalid comparison between Unknown and I4
		//IL_0bd1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd7: Invalid comparison between Unknown and I4
		//IL_0bba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		//IL_066e: Invalid comparison between Unknown and I4
		//IL_1841: Unknown result type (might be due to invalid IL or missing references)
		//IL_1847: Invalid comparison between Unknown and I4
		//IL_19bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_19c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_115f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1161: Unknown result type (might be due to invalid IL or missing references)
		//IL_1163: Unknown result type (might be due to invalid IL or missing references)
		//IL_1168: Unknown result type (might be due to invalid IL or missing references)
		//IL_116c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1171: Unknown result type (might be due to invalid IL or missing references)
		//IL_1173: Unknown result type (might be due to invalid IL or missing references)
		//IL_1175: Unknown result type (might be due to invalid IL or missing references)
		//IL_117a: Unknown result type (might be due to invalid IL or missing references)
		//IL_117f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fde: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e62: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e67: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dcc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ddf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0df3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0df5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dfa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dfe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e03: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e23: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e30: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e38: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e3e: Invalid comparison between Unknown and I4
		//IL_084a: Unknown result type (might be due to invalid IL or missing references)
		//IL_085a: Unknown result type (might be due to invalid IL or missing references)
		//IL_085c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0861: Unknown result type (might be due to invalid IL or missing references)
		//IL_0866: Unknown result type (might be due to invalid IL or missing references)
		//IL_086c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0871: Unknown result type (might be due to invalid IL or missing references)
		//IL_0876: Unknown result type (might be due to invalid IL or missing references)
		//IL_0883: Unknown result type (might be due to invalid IL or missing references)
		//IL_0893: Unknown result type (might be due to invalid IL or missing references)
		//IL_0895: Unknown result type (might be due to invalid IL or missing references)
		//IL_089a: Unknown result type (might be due to invalid IL or missing references)
		//IL_089f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_08af: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08de: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1868: Unknown result type (might be due to invalid IL or missing references)
		//IL_186d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1877: Unknown result type (might be due to invalid IL or missing references)
		//IL_187c: Unknown result type (might be due to invalid IL or missing references)
		//IL_187e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1883: Unknown result type (might be due to invalid IL or missing references)
		//IL_1887: Unknown result type (might be due to invalid IL or missing references)
		//IL_188e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1893: Unknown result type (might be due to invalid IL or missing references)
		//IL_1897: Unknown result type (might be due to invalid IL or missing references)
		//IL_189c: Unknown result type (might be due to invalid IL or missing references)
		//IL_18a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_18ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_18b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_18c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_18c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1526: Unknown result type (might be due to invalid IL or missing references)
		//IL_1538: Unknown result type (might be due to invalid IL or missing references)
		//IL_154b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c51: Invalid comparison between Unknown and I4
		//IL_2d4c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d41: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d43: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_11ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_11dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_11f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_11fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_118d: Unknown result type (might be due to invalid IL or missing references)
		//IL_118f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1194: Unknown result type (might be due to invalid IL or missing references)
		//IL_12d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_12dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_12fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1303: Unknown result type (might be due to invalid IL or missing references)
		//IL_1322: Unknown result type (might be due to invalid IL or missing references)
		//IL_1329: Unknown result type (might be due to invalid IL or missing references)
		//IL_100e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d90: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d95: Unknown result type (might be due to invalid IL or missing references)
		//IL_0beb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf1: Invalid comparison between Unknown and I4
		//IL_0686: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0707: Unknown result type (might be due to invalid IL or missing references)
		//IL_070c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0710: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Unknown result type (might be due to invalid IL or missing references)
		//IL_071b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0720: Unknown result type (might be due to invalid IL or missing references)
		//IL_0725: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Invalid comparison between Unknown and I4
		//IL_1571: Unknown result type (might be due to invalid IL or missing references)
		//IL_1577: Invalid comparison between Unknown and I4
		//IL_1bea: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bec: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b55: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b58: Invalid comparison between Unknown and I4
		//IL_2d51: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e8c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e91: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ead: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eaf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eb4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eb6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eb8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ebf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e53: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e58: Unknown result type (might be due to invalid IL or missing references)
		//IL_0904: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_191b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1922: Unknown result type (might be due to invalid IL or missing references)
		//IL_157e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1583: Unknown result type (might be due to invalid IL or missing references)
		//IL_1587: Unknown result type (might be due to invalid IL or missing references)
		//IL_158e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1593: Unknown result type (might be due to invalid IL or missing references)
		//IL_1597: Unknown result type (might be due to invalid IL or missing references)
		//IL_159c: Unknown result type (might be due to invalid IL or missing references)
		//IL_15a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_15ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_15f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1603: Unknown result type (might be due to invalid IL or missing references)
		//IL_1608: Unknown result type (might be due to invalid IL or missing references)
		//IL_163c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1643: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b61: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b6c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b71: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b86: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0edb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0edf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eeb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c01: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c06: Unknown result type (might be due to invalid IL or missing references)
		//IL_0924: Unknown result type (might be due to invalid IL or missing references)
		//IL_0929: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_17c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_17c7: Invalid comparison between Unknown and I4
		//IL_1c2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c30: Invalid comparison between Unknown and I4
		//IL_2c83: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c85: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c92: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c97: Unknown result type (might be due to invalid IL or missing references)
		//IL_2c9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d69: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d70: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d72: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d77: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d79: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d88: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d92: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_09df: Invalid comparison between Unknown and I4
		//IL_0943: Unknown result type (might be due to invalid IL or missing references)
		//IL_0949: Invalid comparison between Unknown and I4
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c3b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c41: Invalid comparison between Unknown and I4
		//IL_09ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0959: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Invalid comparison between Unknown and I4
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Invalid comparison between Unknown and I4
		//IL_0994: Unknown result type (might be due to invalid IL or missing references)
		//IL_0999: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a58: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0637: Unknown result type (might be due to invalid IL or missing references)
		//IL_0648: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_0658: Unknown result type (might be due to invalid IL or missing references)
		//IL_065d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c65: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a91: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c93: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f54: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f5a: Invalid comparison between Unknown and I4
		//IL_1cc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d20: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f84: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f71: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f78: Invalid comparison between Unknown and I4
		//IL_1fbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fc0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e1f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e21: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e32: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e44: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e50: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e54: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e59: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d48: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d4d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d54: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d92: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d97: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1daf: Unknown result type (might be due to invalid IL or missing references)
		//IL_1db1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1db3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1db8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dbf: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dc4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dc9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dcb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dd2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dd4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dd9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1de6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ded: Unknown result type (might be due to invalid IL or missing references)
		//IL_1df2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1df6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dfb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e00: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e0d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fd1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fd6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fc6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fa7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fac: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fa0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fdb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fb1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fb3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ea2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fe8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ffe: Unknown result type (might be due to invalid IL or missing references)
		//IL_2003: Unknown result type (might be due to invalid IL or missing references)
		//IL_2005: Unknown result type (might be due to invalid IL or missing references)
		//IL_200e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2013: Unknown result type (might be due to invalid IL or missing references)
		//IL_2018: Unknown result type (might be due to invalid IL or missing references)
		//IL_20b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_20bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_2025: Unknown result type (might be due to invalid IL or missing references)
		//IL_2027: Unknown result type (might be due to invalid IL or missing references)
		//IL_202c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2033: Unknown result type (might be due to invalid IL or missing references)
		//IL_206a: Unknown result type (might be due to invalid IL or missing references)
		//IL_206f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2077: Unknown result type (might be due to invalid IL or missing references)
		//IL_207d: Invalid comparison between Unknown and I4
		//IL_211f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2124: Unknown result type (might be due to invalid IL or missing references)
		//IL_20c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_20cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_20d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_20d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_208f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2094: Unknown result type (might be due to invalid IL or missing references)
		//IL_2096: Unknown result type (might be due to invalid IL or missing references)
		//IL_209b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2085: Unknown result type (might be due to invalid IL or missing references)
		//IL_208b: Invalid comparison between Unknown and I4
		//IL_234d: Unknown result type (might be due to invalid IL or missing references)
		//IL_234f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2351: Unknown result type (might be due to invalid IL or missing references)
		//IL_2356: Unknown result type (might be due to invalid IL or missing references)
		//IL_2358: Unknown result type (might be due to invalid IL or missing references)
		//IL_235a: Unknown result type (might be due to invalid IL or missing references)
		//IL_235c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2361: Unknown result type (might be due to invalid IL or missing references)
		//IL_237e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2136: Unknown result type (might be due to invalid IL or missing references)
		//IL_213e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2143: Unknown result type (might be due to invalid IL or missing references)
		//IL_2148: Unknown result type (might be due to invalid IL or missing references)
		//IL_2150: Unknown result type (might be due to invalid IL or missing references)
		//IL_2155: Unknown result type (might be due to invalid IL or missing references)
		//IL_20ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_20f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_20a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_20a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_20ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_20b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_2384: Unknown result type (might be due to invalid IL or missing references)
		//IL_2179: Unknown result type (might be due to invalid IL or missing references)
		//IL_217b: Unknown result type (might be due to invalid IL or missing references)
		//IL_217d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2182: Unknown result type (might be due to invalid IL or missing references)
		//IL_218d: Unknown result type (might be due to invalid IL or missing references)
		//IL_219d: Unknown result type (might be due to invalid IL or missing references)
		//IL_21a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_21ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_21b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_21b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_21b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_21b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_21bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_21c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_21cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_21dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_21e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_21ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_21f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_21f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_21f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_21f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_21fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2208: Unknown result type (might be due to invalid IL or missing references)
		//IL_2218: Unknown result type (might be due to invalid IL or missing references)
		//IL_2223: Unknown result type (might be due to invalid IL or missing references)
		//IL_2228: Unknown result type (might be due to invalid IL or missing references)
		//IL_222d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2166: Unknown result type (might be due to invalid IL or missing references)
		//IL_216b: Unknown result type (might be due to invalid IL or missing references)
		//IL_20fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2102: Unknown result type (might be due to invalid IL or missing references)
		//IL_2109: Unknown result type (might be due to invalid IL or missing references)
		//IL_210f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2114: Unknown result type (might be due to invalid IL or missing references)
		//IL_2119: Unknown result type (might be due to invalid IL or missing references)
		//IL_23a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_23b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_23bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_23be: Unknown result type (might be due to invalid IL or missing references)
		//IL_23c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_23c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_23c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_23ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_23d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_23b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_2564: Unknown result type (might be due to invalid IL or missing references)
		//IL_2566: Unknown result type (might be due to invalid IL or missing references)
		//IL_256b: Unknown result type (might be due to invalid IL or missing references)
		//IL_23ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_23f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_2576: Unknown result type (might be due to invalid IL or missing references)
		//IL_2578: Unknown result type (might be due to invalid IL or missing references)
		//IL_2580: Unknown result type (might be due to invalid IL or missing references)
		//IL_2586: Invalid comparison between Unknown and I4
		//IL_2414: Unknown result type (might be due to invalid IL or missing references)
		//IL_2419: Unknown result type (might be due to invalid IL or missing references)
		//IL_241d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2422: Unknown result type (might be due to invalid IL or missing references)
		//IL_2635: Unknown result type (might be due to invalid IL or missing references)
		//IL_2637: Unknown result type (might be due to invalid IL or missing references)
		//IL_263b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2640: Unknown result type (might be due to invalid IL or missing references)
		//IL_2645: Unknown result type (might be due to invalid IL or missing references)
		//IL_2591: Unknown result type (might be due to invalid IL or missing references)
		//IL_2597: Invalid comparison between Unknown and I4
		//IL_24bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_24be: Unknown result type (might be due to invalid IL or missing references)
		//IL_24c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_24c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_24cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_24d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_24ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_24ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_24f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_24f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_24ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_2504: Unknown result type (might be due to invalid IL or missing references)
		//IL_2510: Unknown result type (might be due to invalid IL or missing references)
		//IL_2512: Unknown result type (might be due to invalid IL or missing references)
		//IL_2517: Unknown result type (might be due to invalid IL or missing references)
		//IL_251c: Unknown result type (might be due to invalid IL or missing references)
		//IL_251e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2523: Unknown result type (might be due to invalid IL or missing references)
		//IL_2528: Unknown result type (might be due to invalid IL or missing references)
		//IL_252d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2531: Unknown result type (might be due to invalid IL or missing references)
		//IL_2536: Unknown result type (might be due to invalid IL or missing references)
		//IL_2542: Unknown result type (might be due to invalid IL or missing references)
		//IL_2544: Unknown result type (might be due to invalid IL or missing references)
		//IL_2549: Unknown result type (might be due to invalid IL or missing references)
		//IL_254e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2550: Unknown result type (might be due to invalid IL or missing references)
		//IL_2555: Unknown result type (might be due to invalid IL or missing references)
		//IL_255a: Unknown result type (might be due to invalid IL or missing references)
		//IL_255f: Unknown result type (might be due to invalid IL or missing references)
		//IL_22c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_22c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_22ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_22cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_22ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_22d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_22d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_22e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_22e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_22ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_22ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_22f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_22f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_22f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_22fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_2306: Unknown result type (might be due to invalid IL or missing references)
		//IL_230b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2310: Unknown result type (might be due to invalid IL or missing references)
		//IL_2318: Unknown result type (might be due to invalid IL or missing references)
		//IL_2322: Unknown result type (might be due to invalid IL or missing references)
		//IL_2336: Unknown result type (might be due to invalid IL or missing references)
		//IL_26a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_26a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_25ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_25b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_25bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_25c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_2480: Unknown result type (might be due to invalid IL or missing references)
		//IL_2485: Unknown result type (might be due to invalid IL or missing references)
		//IL_2487: Unknown result type (might be due to invalid IL or missing references)
		//IL_2489: Unknown result type (might be due to invalid IL or missing references)
		//IL_2493: Unknown result type (might be due to invalid IL or missing references)
		//IL_249d: Unknown result type (might be due to invalid IL or missing references)
		//IL_24a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_24a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_266c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2671: Unknown result type (might be due to invalid IL or missing references)
		//IL_2674: Unknown result type (might be due to invalid IL or missing references)
		//IL_2679: Unknown result type (might be due to invalid IL or missing references)
		//IL_2686: Unknown result type (might be due to invalid IL or missing references)
		//IL_268c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2691: Unknown result type (might be due to invalid IL or missing references)
		//IL_2696: Unknown result type (might be due to invalid IL or missing references)
		//IL_25db: Unknown result type (might be due to invalid IL or missing references)
		//IL_25dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_25df: Unknown result type (might be due to invalid IL or missing references)
		//IL_25e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_25eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_25f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_2629: Unknown result type (might be due to invalid IL or missing references)
		//IL_262e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2633: Unknown result type (might be due to invalid IL or missing references)
		//IL_26e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_26e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_26f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_26fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_26ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_2704: Unknown result type (might be due to invalid IL or missing references)
		//IL_2711: Unknown result type (might be due to invalid IL or missing references)
		//IL_2718: Unknown result type (might be due to invalid IL or missing references)
		//IL_271d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2723: Unknown result type (might be due to invalid IL or missing references)
		//IL_2728: Unknown result type (might be due to invalid IL or missing references)
		//IL_272a: Unknown result type (might be due to invalid IL or missing references)
		//IL_272f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2740: Unknown result type (might be due to invalid IL or missing references)
		//IL_2742: Unknown result type (might be due to invalid IL or missing references)
		//IL_2744: Unknown result type (might be due to invalid IL or missing references)
		//IL_2749: Unknown result type (might be due to invalid IL or missing references)
		//IL_275e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2763: Unknown result type (might be due to invalid IL or missing references)
		//IL_2765: Unknown result type (might be due to invalid IL or missing references)
		//IL_276a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2788: Unknown result type (might be due to invalid IL or missing references)
		//IL_278d: Unknown result type (might be due to invalid IL or missing references)
		//IL_278f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2794: Unknown result type (might be due to invalid IL or missing references)
		//IL_279e: Unknown result type (might be due to invalid IL or missing references)
		//IL_27a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_27a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_27bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_27c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_27d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_27d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_27eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_27f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_2803: Unknown result type (might be due to invalid IL or missing references)
		//IL_2808: Unknown result type (might be due to invalid IL or missing references)
		//IL_280a: Unknown result type (might be due to invalid IL or missing references)
		//IL_280f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2819: Unknown result type (might be due to invalid IL or missing references)
		//IL_281e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2823: Unknown result type (might be due to invalid IL or missing references)
		//IL_2836: Unknown result type (might be due to invalid IL or missing references)
		//IL_283b: Unknown result type (might be due to invalid IL or missing references)
		//IL_283d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2842: Unknown result type (might be due to invalid IL or missing references)
		//IL_284c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2851: Unknown result type (might be due to invalid IL or missing references)
		//IL_2856: Unknown result type (might be due to invalid IL or missing references)
		//IL_2869: Unknown result type (might be due to invalid IL or missing references)
		//IL_286e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2881: Unknown result type (might be due to invalid IL or missing references)
		//IL_2886: Unknown result type (might be due to invalid IL or missing references)
		//IL_2888: Unknown result type (might be due to invalid IL or missing references)
		//IL_288d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2897: Unknown result type (might be due to invalid IL or missing references)
		//IL_289c: Unknown result type (might be due to invalid IL or missing references)
		//IL_28a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_28cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_28d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_291a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2913: Unknown result type (might be due to invalid IL or missing references)
		//IL_2934: Unknown result type (might be due to invalid IL or missing references)
		//IL_2936: Unknown result type (might be due to invalid IL or missing references)
		//IL_293b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2940: Unknown result type (might be due to invalid IL or missing references)
		//IL_2942: Unknown result type (might be due to invalid IL or missing references)
		//IL_2944: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a12: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a14: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a1f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a25: Invalid comparison between Unknown and I4
		//IL_2adf: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ae4: Unknown result type (might be due to invalid IL or missing references)
		//IL_2aec: Unknown result type (might be due to invalid IL or missing references)
		//IL_2af1: Unknown result type (might be due to invalid IL or missing references)
		//IL_2af6: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a30: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a36: Invalid comparison between Unknown and I4
		//IL_2a4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a56: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a60: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a81: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a83: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a88: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a8f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a96: Unknown result type (might be due to invalid IL or missing references)
		//IL_2acd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ad2: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ad7: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b28: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b32: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = Mission.Scene;
		bool photoModeOrbit = scene.GetPhotoModeOrbit();
		float num = (IsPhotoModeEnabled ? scene.GetPhotoModeFov() : 0f);
		bool flag = _isGamepadActive && PhotoModeRequiresMouse;
		UpdateDragData();
		MatrixFrame val = MatrixFrame.Identity;
		MissionPeer val2 = ((GameNetwork.MyPeer != null) ? PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer) : null);
		SpectatorData spectatingData = GetSpectatingData(CombatCamera.Frame.origin);
		Agent agentToFollow = ((SpectatorData)(ref spectatingData)).AgentToFollow;
		IAgentVisual agentVisualToFollow = ((SpectatorData)(ref spectatingData)).AgentVisualToFollow;
		SpectatorCameraTypes cameraType = ((SpectatorData)(ref spectatingData)).CameraType;
		bool flag2 = Mission.CameraIsFirstPerson && agentToFollow != null && agentToFollow == Mission.MainAgent;
		float num2 = (flag2 ? Mission.GetFirstPersonFov() : 65f);
		if (IsPhotoModeEnabled)
		{
			CameraViewAngle = num2;
		}
		else
		{
			_zoomAmount = MBMath.ClampFloat(_zoomAmount, 0f, 1f);
			float num3 = 37f / MaxCameraZoom;
			CameraViewAngle = MBMath.Lerp(num2, num3, _zoomAmount, 0.005f);
		}
		if (_missionMainAgentController == null)
		{
			_missionMainAgentController = Mission.GetMissionBehavior<MissionMainAgentController>();
		}
		else
		{
			_missionMainAgentController.IsDisabled = true;
		}
		if (_missionMainAgentController != null && (int)Mission.Mode != 6 && Mission.MainAgent != null && Mission.MainAgent.IsCameraAttachable())
		{
			_missionMainAgentController.IsDisabled = false;
		}
		bool flag3 = _cameraApplySpecialMovementsInstantly;
		Vec3 val6;
		Vec2 val8;
		float num13;
		bool flag4;
		MatrixFrame val14;
		bool flag5;
		bool flag6;
		bool flag7;
		Vec2 val17;
		Vec3 val18;
		MatrixFrame boneEntitialFrame;
		MatrixFrame frame3;
		Vec3 val25;
		bool flag8;
		Vec3 val19;
		float num28;
		float num10;
		Vec3 val22;
		if ((IsPhotoModeEnabled && !photoModeOrbit) || (agentToFollow == null && agentVisualToFollow == null))
		{
			float num4 = 0f - scene.GetPhotoModeRoll();
			((Mat3)(ref val.rotation)).RotateAboutSide(MathF.PI / 2f);
			((Mat3)(ref val.rotation)).RotateAboutForward(CameraBearing);
			((Mat3)(ref val.rotation)).RotateAboutSide(CameraElevation);
			((Mat3)(ref val.rotation)).RotateAboutUp(num4);
			val.origin = CombatCamera.Frame.origin;
			_cameraSpeed *= 1f - 5f * dt;
			_cameraSpeed.x = MBMath.ClampFloat(_cameraSpeed.x, -20f, 20f);
			_cameraSpeed.y = MBMath.ClampFloat(_cameraSpeed.y, -20f, 20f);
			_cameraSpeed.z = MBMath.ClampFloat(_cameraSpeed.z, -20f, 20f);
			if (Game.Current.CheatMode)
			{
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyIncreaseCameraSpeed"))
				{
					_cameraSpeedMultiplier *= 1.5f;
				}
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyDecreaseCameraSpeed"))
				{
					_cameraSpeedMultiplier *= 2f / 3f;
				}
				if (InputManager.IsHotKeyPressed("ResetCameraSpeed"))
				{
					_cameraSpeedMultiplier = 1f;
				}
				if (InputManager.IsControlDown())
				{
					float num5 = ((ScreenLayer)SceneLayer).Input.GetDeltaMouseScroll() * (1f / 120f);
					if (num5 > 0.01f)
					{
						_cameraSpeedMultiplier *= 1.25f;
					}
					else if (num5 < -0.01f)
					{
						_cameraSpeedMultiplier *= 0.8f;
					}
				}
			}
			float waterLevel = scene.GetWaterLevel();
			float num6 = 10f * _cameraSpeedMultiplier * ((!IsPhotoModeEnabled) ? 1f : (flag ? 0f : 0.3f));
			if ((int)Mission.Mode == 6)
			{
				float num7 = MathF.Max(scene.GetGroundHeightAtPosition(val.origin, (BodyFlags)544321929), waterLevel);
				num6 *= MathF.Max(1f, 1f + (val.origin.z - num7 - 5f) / 10f);
			}
			if ((!IsPhotoModeEnabled && ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(24)) || (IsPhotoModeEnabled && !flag && ((ScreenLayer)SceneLayer).Input.IsHotKeyDown("FasterCamera")))
			{
				num6 *= (float)_shiftSpeedMultiplier;
			}
			if (!_cameraSmoothMode)
			{
				_cameraSpeed.x = 0f;
				_cameraSpeed.y = 0f;
				_cameraSpeed.z = 0f;
			}
			if ((!InputManager.IsControlDown() || !InputManager.IsAltDown()) && !LockCameraMovement)
			{
				bool num8 = !_isGamepadActive || (int)Mission.Mode != 6 || Input.IsKeyDown((InputKey)254);
				Vec3 val3 = Vec3.Zero;
				if (num8)
				{
					val3.x = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("MovementAxisX");
					val3.y = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("MovementAxisY");
					if (MathF.Abs(val3.x) < 0.2f)
					{
						val3.x = 0f;
					}
					if (MathF.Abs(val3.y) < 0.2f)
					{
						val3.y = 0f;
					}
				}
				if (!_isGamepadActive || (!IsPhotoModeEnabled && (int)Mission.Mode != 6 && !IsOrderMenuOpen && !IsTransferMenuOpen))
				{
					if (((ScreenLayer)SceneLayer).Input.IsGameKeyDown(14))
					{
						val3.z += 1f;
					}
					if (((ScreenLayer)SceneLayer).Input.IsGameKeyDown(15))
					{
						val3.z -= 1f;
					}
				}
				else if ((int)Mission.Mode == 6 && ((ScreenLayer)SceneLayer).IsHitThisFrame)
				{
					if (((ScreenLayer)SceneLayer).Input.IsKeyDown((InputKey)249))
					{
						val3.z += 1f;
					}
					if (((ScreenLayer)SceneLayer).Input.IsKeyDown((InputKey)248))
					{
						val3.z -= 1f;
					}
				}
				if (((Vec3)(ref val3)).IsNonZero)
				{
					float val4 = ((Vec3)(ref val3)).Normalize();
					val3 *= num6 * Math.Min(1f, val4);
					_cameraSpeed += val3;
				}
			}
			if ((int)Mission.Mode == 6 && !IsRadialMenuActive)
			{
				ref Vec3 origin = ref val.origin;
				Vec3 val5 = origin;
				float x = _cameraSpeed.x;
				val6 = new Vec3(((Vec3)(ref val.rotation.s)).AsVec2, 0f, -1f);
				origin = val5 + x * ((Vec3)(ref val6)).NormalizedCopy() * dt;
				ref Vec3 origin2 = ref val.origin;
				Vec3 val7 = origin2;
				float y = _cameraSpeed.y;
				val6 = new Vec3(((Vec3)(ref val.rotation.u)).AsVec2, 0f, -1f);
				origin2 = val7 - y * ((Vec3)(ref val6)).NormalizedCopy() * dt;
				val.origin.z += _cameraSpeed.z * dt;
				if (!Game.Current.CheatMode || !InputManager.IsControlDown())
				{
					_cameraDeploymentHeightToAdd += 3f * ((ScreenLayer)SceneLayer).Input.GetDeltaMouseScroll() / 120f;
					if (((ScreenLayer)SceneLayer).Input.IsHotKeyDown("DeploymentCameraIsActive"))
					{
						_cameraDeploymentHeightToAdd += 0.05f * Input.MouseMoveY;
					}
				}
				if (MathF.Abs(_cameraDeploymentHeightToAdd) > 0.001f)
				{
					val.origin.z += _cameraDeploymentHeightToAdd * dt * 10f;
					_cameraDeploymentHeightToAdd = MathF.Lerp(_cameraDeploymentHeightToAdd, 0f, 1f - MathF.Pow(0.0005f, dt), 1E-05f);
				}
				else
				{
					val.origin.z += _cameraDeploymentHeightToAdd;
					_cameraDeploymentHeightToAdd = 0f;
				}
			}
			else
			{
				ref Vec3 origin3 = ref val.origin;
				origin3 += _cameraSpeed.x * val.rotation.s * dt;
				ref Vec3 origin4 = ref val.origin;
				origin4 -= _cameraSpeed.y * val.rotation.u * dt;
				ref Vec3 origin5 = ref val.origin;
				origin5 += _cameraSpeed.z * val.rotation.f * dt;
			}
			if (!MBEditor.IsEditModeOn)
			{
				if (!Mission.IsPositionInsideBoundaries(((Vec3)(ref val.origin)).AsVec2))
				{
					((Vec3)(ref val.origin)).AsVec2 = Mission.GetClosestBoundaryPosition(((Vec3)(ref val.origin)).AsVec2);
				}
				if (!GameNetwork.IsMultiplayer && (int)Mission.Mode == 6)
				{
					_ = Mission.PlayerTeam.Side;
					IMissionDeploymentPlan deploymentPlan = Mission.DeploymentPlan;
					if (deploymentPlan.HasDeploymentBoundaries(Mission.PlayerTeam))
					{
						Team playerTeam = Mission.PlayerTeam;
						val8 = ((Vec3)(ref val.origin)).AsVec2;
						if (!deploymentPlan.IsPositionInsideDeploymentBoundaries(playerTeam, ref val8))
						{
							ref Vec3 origin6 = ref val.origin;
							Team playerTeam2 = Mission.PlayerTeam;
							val8 = ((Vec3)(ref val.origin)).AsVec2;
							((Vec3)(ref origin6)).AsVec2 = deploymentPlan.GetClosestDeploymentBoundaryPosition(playerTeam2, ref val8);
						}
					}
				}
				float num9 = MathF.Max(scene.GetGroundHeightAtPosition(((int)Mission.Mode == 6) ? (val.origin + new Vec3(0f, 0f, 100f, -1f)) : val.origin, (BodyFlags)544321929), waterLevel);
				if (!IsCheatGhostMode && num9 < 9999f)
				{
					val.origin.z = MathF.Max(val.origin.z, num9 + 0.5f);
				}
				if (val.origin.z > num9 + 80f)
				{
					val.origin.z = num9 + 80f;
				}
				if (_cameraHeightLimit > 0f && val.origin.z > _cameraHeightLimit)
				{
					val.origin.z = _cameraHeightLimit;
				}
				if (val.origin.z < -100f)
				{
					val.origin.z = -100f;
				}
			}
		}
		else
		{
			if (!flag2 || IsPhotoModeEnabled)
			{
				num10 = 0.6f;
				float num11 = 0f;
				bool num12 = agentVisualToFollow != null;
				num13 = 1f;
				flag4 = false;
				float num15;
				if (num12)
				{
					_cameraSpecialTargetAddedBearing = 0f;
					_cameraSpecialTargetAddedElevation = 0f;
					_cameraSpecialTargetPositionToAdd = Vec3.Zero;
					_cameraSpecialTargetDistanceToAdd = 0f;
					num10 = 1.25f;
					flag3 = flag3 || agentVisualToFollow != LastFollowedAgentVisuals;
					EquipmentElement horse = agentVisualToFollow.GetEquipment().Horse;
					if (((EquipmentElement)(ref horse)).Item != null)
					{
						horse = agentVisualToFollow.GetEquipment().Horse;
						float num14 = (float)((EquipmentElement)(ref horse)).Item.HorseComponent.BodyLength * 0.01f;
						num10 += 2f;
						num15 = 1f * num14 + 0.9f * num13 - 0.2f;
					}
					else
					{
						num15 = 1f * num13;
					}
					MatrixFrame frame = agentVisualToFollow.GetFrame();
					CameraBearing = MBMath.WrapAngle(((Vec3)(ref frame.rotation.f)).RotationZ + MathF.PI);
					CameraElevation = 0.15f;
				}
				else
				{
					flag4 = agentToFollow.HasMount;
					flag3 = flag3 || agentToFollow != LastFollowedAgent;
					if (Mission.CustomCameraFixedDistance == float.MinValue)
					{
						num13 = agentToFollow.AgentScale;
						if (((int)Mission.Mode == 1 || (int)Mission.Mode == 5) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && (int)_missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == 3)
						{
							IFocusable obj = _missionMainAgentController?.InteractionComponent.CurrentFocusedObject;
							Agent val9 = (Agent)(object)((obj is Agent) ? obj : null);
							num15 = (val9.AgentVisuals.GetGlobalStableEyePoint(true).z + agentToFollow.AgentVisuals.GetGlobalStableEyePoint(true).z) * 0.5f - agentToFollow.Position.z;
							if (val9.HasMount)
							{
								num10 += 0.1f;
							}
							if ((int)Mission.Mode == 5)
							{
								val6 = val9.Position;
								Vec2 asVec = ((Vec3)(ref val6)).AsVec2;
								val6 = agentToFollow.Position;
								Vec2 val10 = asVec - ((Vec3)(ref val6)).AsVec2;
								float length = ((Vec2)(ref val10)).Length;
								float num16 = MathF.Max(num10 + Mission.CameraAddedDistance, 0.48f) * num13 + length * 0.5f;
								num15 += -0.004f * num16 * _cameraSpecialCurrentFOV;
								Vec3 globalStableEyePoint = val9.AgentVisuals.GetGlobalStableEyePoint(val9.IsHuman);
								Vec3 globalStableEyePoint2 = agentToFollow.AgentVisuals.GetGlobalStableEyePoint(agentToFollow.IsHuman);
								float num17 = ((Vec2)(ref val10)).RotationInRadians - MathF.Min(0.47123894f, 0.4f / length);
								_cameraSpecialTargetAddedBearing = MBMath.WrapAngle(num17 - CameraBearing);
								Vec2 val11 = default(Vec2);
								((Vec2)(ref val11))._002Ector(globalStableEyePoint.z - globalStableEyePoint2.z, MathF.Max(length, 1f));
								float num18 = (flag4 ? (-0.03f) : 0f) - ((Vec2)(ref val11)).RotationInRadians;
								_cameraSpecialTargetAddedElevation = num18 - CameraElevation + MathF.Asin(-0.2f * (num16 - length * 0.5f) / num16);
							}
						}
						else if (!flag4)
						{
							num15 = (((int)agentToFollow.AgentVisuals.GetCurrentRagdollState() == 3) ? 0.5f : (((agentToFollow.GetCurrentAnimationFlag(0) & 0x40000000) != 0) ? 0.5f : ((!agentToFollow.CrouchMode && !agentToFollow.IsSitting()) ? ((agentToFollow.Monster.StandingEyeHeight + 0.2f) * num13) : ((agentToFollow.Monster.CrouchEyeHeight + 0.2f) * num13))));
						}
						else
						{
							num10 += 0.1f;
							Agent mountAgent = agentToFollow.MountAgent;
							Monster monster = mountAgent.Monster;
							num15 = (monster.RiderCameraHeightAdder + monster.BodyCapsulePoint1.z + monster.BodyCapsuleRadius) * mountAgent.AgentScale + agentToFollow.Monster.CrouchEyeHeight * num13;
						}
						if ((IsViewingCharacter() && ((int)cameraType != 6 || agentToFollow == Mission.MainAgent)) || IsPhotoModeEnabled)
						{
							num15 *= 0.5f;
							num10 += 0.5f;
						}
						else if (agentToFollow.HasMount && agentToFollow.IsDoingPassiveAttack && ((int)cameraType != 6 || agentToFollow == Mission.MainAgent))
						{
							num15 *= 1.1f;
						}
					}
					else
					{
						num15 = 0f;
					}
					if (_cameraAddSpecialMovement)
					{
						if (((int)Mission.Mode == 1 || (int)Mission.Mode == 5) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && (int)_missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == 3)
						{
							IFocusable currentFocusedObject = _missionMainAgentController.InteractionComponent.CurrentFocusedObject;
							IFocusable obj2 = ((currentFocusedObject is Agent) ? currentFocusedObject : null);
							Vec3 globalStableEyePoint3 = ((Agent)obj2).AgentVisuals.GetGlobalStableEyePoint(true);
							Vec3 globalStableEyePoint4 = agentToFollow.AgentVisuals.GetGlobalStableEyePoint(true);
							val6 = ((Agent)obj2).Position;
							Vec2 asVec2 = ((Vec3)(ref val6)).AsVec2;
							val6 = agentToFollow.Position;
							Vec2 val12 = asVec2 - ((Vec3)(ref val6)).AsVec2;
							float length2 = ((Vec2)(ref val12)).Length;
							_cameraSpecialTargetPositionToAdd = new Vec3(val12 * 0.5f, 0f, -1f);
							_cameraSpecialTargetDistanceToAdd = length2 * (flag4 ? 1.3f : 0.8f) - num10;
							float num19 = ((Vec2)(ref val12)).RotationInRadians - MathF.Min(0.47123894f, 0.48f / length2);
							_cameraSpecialTargetAddedBearing = MBMath.WrapAngle(num19 - CameraBearing);
							Vec2 val13 = default(Vec2);
							((Vec2)(ref val13))._002Ector(globalStableEyePoint3.z - globalStableEyePoint4.z, MathF.Max(length2, 1f));
							float num20 = (flag4 ? (-0.03f) : 0f) - ((Vec2)(ref val13)).RotationInRadians;
							_cameraSpecialTargetAddedElevation = num20 - CameraElevation;
							_cameraSpecialTargetFOV = MathF.Min(32.5f, 50f / length2);
						}
						else
						{
							_cameraSpecialTargetPositionToAdd = Vec3.Zero;
							_cameraSpecialTargetDistanceToAdd = 0f;
							_cameraSpecialTargetAddedBearing = 0f;
							_cameraSpecialTargetAddedElevation = 0f;
							_cameraSpecialTargetFOV = 65f;
						}
						if (flag3)
						{
							_cameraSpecialCurrentPositionToAdd = _cameraSpecialTargetPositionToAdd;
							_cameraSpecialCurrentDistanceToAdd = _cameraSpecialTargetDistanceToAdd;
							_cameraSpecialCurrentAddedBearing = _cameraSpecialTargetAddedBearing;
							_cameraSpecialCurrentAddedElevation = _cameraSpecialTargetAddedElevation;
							_cameraSpecialCurrentFOV = _cameraSpecialTargetFOV;
						}
					}
					if (_cameraSpecialCurrentDistanceToAdd != _cameraSpecialTargetDistanceToAdd)
					{
						float num21 = _cameraSpecialTargetDistanceToAdd - _cameraSpecialCurrentDistanceToAdd;
						if (flag3 || MathF.Abs(num21) < 0.0001f)
						{
							_cameraSpecialCurrentDistanceToAdd = _cameraSpecialTargetDistanceToAdd;
						}
						else
						{
							float num22 = num21 * 4f * dt;
							_cameraSpecialCurrentDistanceToAdd += num22;
						}
					}
					num10 += _cameraSpecialCurrentDistanceToAdd;
				}
				if (flag3)
				{
					_cameraTargetAddedHeight = num15;
				}
				else
				{
					_cameraTargetAddedHeight += (num15 - _cameraTargetAddedHeight) * dt * 6f * num13;
				}
				if (_cameraSpecialTargetAddedBearing != _cameraSpecialCurrentAddedBearing)
				{
					float num23 = _cameraSpecialTargetAddedBearing - _cameraSpecialCurrentAddedBearing;
					if (flag3 || MathF.Abs(num23) < 0.0001f)
					{
						_cameraSpecialCurrentAddedBearing = _cameraSpecialTargetAddedBearing;
					}
					else
					{
						float num24 = num23 * 10f * dt;
						_cameraSpecialCurrentAddedBearing += num24;
					}
				}
				if (_cameraSpecialTargetAddedElevation != _cameraSpecialCurrentAddedElevation)
				{
					float num25 = _cameraSpecialTargetAddedElevation - _cameraSpecialCurrentAddedElevation;
					if (flag3 || MathF.Abs(num25) < 0.0001f)
					{
						_cameraSpecialCurrentAddedElevation = _cameraSpecialTargetAddedElevation;
					}
					else
					{
						float num26 = num25 * 8f * dt;
						_cameraSpecialCurrentAddedElevation += num26;
					}
				}
				((Mat3)(ref val.rotation)).RotateAboutSide(MathF.PI / 2f);
				if (agentToFollow != null && !agentToFollow.IsMine && (int)cameraType == 6)
				{
					Vec3 lookDirection = agentToFollow.LookDirection;
					ref Mat3 rotation = ref val.rotation;
					val8 = ((Vec3)(ref lookDirection)).AsVec2;
					((Mat3)(ref rotation)).RotateAboutForward(((Vec2)(ref val8)).RotationInRadians);
					((Mat3)(ref val.rotation)).RotateAboutSide(MathF.Asin(lookDirection.z));
				}
				else
				{
					((Mat3)(ref val.rotation)).RotateAboutForward(CameraBearing + _cameraSpecialCurrentAddedBearing);
					((Mat3)(ref val.rotation)).RotateAboutSide(CameraElevation + _cameraSpecialCurrentAddedElevation);
					if (IsPhotoModeEnabled)
					{
						float num27 = 0f - scene.GetPhotoModeRoll();
						((Mat3)(ref val.rotation)).RotateAboutUp(num27);
					}
				}
				val14 = val;
				num28 = ((Mission.CustomCameraFixedDistance != float.MinValue) ? Mission.CustomCameraFixedDistance : (MathF.Max(num10 + Mission.CameraAddedDistance, 0.48f) * num13));
				if ((int)Mission.Mode != 1 && (int)Mission.Mode != 5 && agentToFollow != null && agentToFollow.IsActive() && BannerlordConfig.EnableVerticalAimCorrection)
				{
					MissionWeapon wieldedWeapon = agentToFollow.WieldedWeapon;
					WeaponComponentData currentUsageItem = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
					if (currentUsageItem != null && currentUsageItem.IsRangedWeapon)
					{
						MatrixFrame frame2 = CombatCamera.Frame;
						((Mat3)(ref frame2.rotation)).RotateAboutSide(0f - _cameraAddedElevation);
						float num29;
						if (flag4)
						{
							Agent mountAgent2 = agentToFollow.MountAgent;
							Monster monster2 = mountAgent2.Monster;
							num29 = (monster2.RiderCameraHeightAdder + monster2.BodyCapsulePoint1.z + monster2.BodyCapsuleRadius) * mountAgent2.AgentScale + agentToFollow.Monster.CrouchEyeHeight * num13;
						}
						else
						{
							num29 = (agentToFollow.CrouchMode ? agentToFollow.Monster.CrouchEyeHeight : agentToFollow.Monster.StandingEyeHeight) * num13;
						}
						if (Extensions.HasAnyFlag<WeaponFlags>(currentUsageItem.WeaponFlags, (WeaponFlags)4294967296L))
						{
							num29 *= 1.25f;
						}
						float num31;
						if (flag3)
						{
							Vec3 val15 = agentToFollow.Position + val.rotation.f * num13 * (0.7f * MathF.Pow(MathF.Cos(1f / ((num28 / num13 - 0.2f) * 30f + 20f)), 3500f));
							val15.z += _cameraTargetAddedHeight;
							Vec3 val16 = val15 + val.rotation.u * num28;
							float z = val16.z;
							float num30 = 0f - val14.rotation.u.z;
							Vec2 asVec3 = ((Vec3)(ref val16)).AsVec2;
							val6 = agentToFollow.Position;
							val8 = asVec3 - ((Vec3)(ref val6)).AsVec2;
							num31 = z + num30 * ((Vec2)(ref val8)).Length - (agentToFollow.Position.z + num29);
						}
						else
						{
							float z2 = frame2.origin.z;
							float num32 = 0f - frame2.rotation.u.z;
							Vec2 asVec4 = ((Vec3)(ref frame2.origin)).AsVec2;
							val6 = agentToFollow.Position;
							val8 = asVec4 - ((Vec3)(ref val6)).AsVec2;
							num31 = z2 + num32 * ((Vec2)(ref val8)).Length - (agentToFollow.Position.z + num29);
						}
						if (num31 > 0f)
						{
							float num33 = MathF.Sqrt(19.6f * num31);
							wieldedWeapon = agentToFollow.WieldedWeapon;
							num11 = MathF.Max(-0.15f, 0f - MathF.Asin(MathF.Min(1f, num33 / (float)((MissionWeapon)(ref wieldedWeapon)).GetModifiedMissileSpeedForCurrentUsage())));
						}
						else
						{
							num11 = 0f;
						}
					}
					else
					{
						num11 = ManagedParameters.Instance.GetManagedParameter((ManagedParametersEnum)2);
					}
				}
				if (flag3 || IsPhotoModeEnabled)
				{
					_cameraAddedElevation = num11;
				}
				else
				{
					_cameraAddedElevation += (num11 - _cameraAddedElevation) * dt * 3f;
				}
				if (!IsPhotoModeEnabled)
				{
					((Mat3)(ref val.rotation)).RotateAboutSide(_cameraAddedElevation);
				}
				flag5 = IsViewingCharacter() && !GameNetwork.IsSessionActive;
				flag6 = agentToFollow != null && (NativeObject)(object)agentToFollow.AgentVisuals != (NativeObject)null && (int)agentToFollow.AgentVisuals.GetCurrentRagdollState() > 0;
				flag7 = agentToFollow != null && agentToFollow.IsActive() && (int)agentToFollow.GetCurrentActionType(0) == 37;
				val17 = Vec2.Zero;
				if (num12)
				{
					MBAgentVisuals visuals = GetPlayerAgentVisuals(val2).GetVisuals();
					val18 = ((visuals != null) ? visuals.GetGlobalFrame().origin : val2.ControlledAgent.Position);
					val19 = val18;
				}
				else
				{
					val18 = agentToFollow.VisualPosition;
					val19 = (flag6 ? agentToFollow.AgentVisuals.GetFrame().origin : val18);
					if (flag4)
					{
						val17 = agentToFollow.MountAgent.GetMovementDirection() * agentToFollow.MountAgent.Monster.RiderBodyCapsuleForwardAdder;
						val19 += ((Vec2)(ref val17)).ToVec3(0f);
					}
				}
				if (_cameraAddSpecialPositionalMovement)
				{
					Vec3 val20 = val14.rotation.f * num13 * (0.7f * MathF.Pow(MathF.Cos(1f / ((num28 / num13 - 0.2f) * 30f + 20f)), 3500f));
					if ((int)Mission.Mode == 1 || (int)Mission.Mode == 5)
					{
						_cameraSpecialCurrentPositionToAdd += val20;
					}
					else
					{
						_cameraSpecialCurrentPositionToAdd -= val20;
					}
				}
				if (_cameraSpecialCurrentPositionToAdd != _cameraSpecialTargetPositionToAdd)
				{
					Vec3 val21 = _cameraSpecialTargetPositionToAdd - _cameraSpecialCurrentPositionToAdd;
					if (flag3 || ((Vec3)(ref val21)).LengthSquared < 1.0000001E-06f)
					{
						_cameraSpecialCurrentPositionToAdd = _cameraSpecialTargetPositionToAdd;
					}
					else
					{
						_cameraSpecialCurrentPositionToAdd += val21 * 4f * dt;
					}
				}
				val22 = _cameraSpecialCurrentPositionToAdd;
				if (!Mission.CameraIsFirstPerson)
				{
					val22 += Mission.CustomCameraTargetLocalOffset;
					val6 = Mission.CustomCameraLocalOffset;
					if (!((Vec3)(ref val6)).IsNonZero)
					{
						val6 = Mission.CustomCameraLocalOffset2;
						if (!((Vec3)(ref val6)).IsNonZero)
						{
							goto IL_222f;
						}
					}
					val22 += val.rotation.s * (Mission.CustomCameraLocalOffset.x + Mission.CustomCameraLocalOffset2.x);
					val22 += -val.rotation.u * (Mission.CustomCameraLocalOffset.y + Mission.CustomCameraLocalOffset2.y);
					val22 += val.rotation.f * (Mission.CustomCameraLocalOffset.z + Mission.CustomCameraLocalOffset2.z);
					goto IL_222f;
				}
				goto IL_234d;
			}
			Agent val23 = agentToFollow;
			if ((NativeObject)(object)agentToFollow.AgentVisuals != (NativeObject)null)
			{
				if (_cameraAddSpecialMovement)
				{
					if (((int)Mission.Mode == 1 || (int)Mission.Mode == 5) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && (int)_missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == 3)
					{
						IFocusable currentFocusedObject2 = _missionMainAgentController.InteractionComponent.CurrentFocusedObject;
						Vec3 val24 = ((Agent)((currentFocusedObject2 is Agent) ? currentFocusedObject2 : null)).Position - agentToFollow.Position;
						float cameraSpecialTargetFOV;
						if (!(65f / CameraViewAngle * MathF.Abs(val24.z) < 2f))
						{
							val8 = ((Vec3)(ref val24)).AsVec2;
							cameraSpecialTargetFOV = 160f / ((Vec2)(ref val8)).Length;
						}
						else
						{
							float num34 = (((int)Mission.Mode == 5) ? 48.75f : 32.5f);
							float num35 = (((int)Mission.Mode == 5) ? 75f : 50f);
							val8 = ((Vec3)(ref val24)).AsVec2;
							cameraSpecialTargetFOV = MathF.Min(num34, num35 / ((Vec2)(ref val8)).Length);
						}
						_cameraSpecialTargetFOV = cameraSpecialTargetFOV;
					}
					else
					{
						_cameraSpecialTargetFOV = 65f;
					}
					if (flag3)
					{
						_cameraSpecialCurrentFOV = _cameraSpecialTargetFOV;
					}
				}
				agentToFollow.AgentVisuals.GetSkeleton().ForceUpdateBoneFrames();
				boneEntitialFrame = agentToFollow.GetBoneEntitialFrame(agentToFollow.Monster.ThoraxLookDirectionBoneIndex, true);
				MatrixFrame boneEntitialFrame2 = agentToFollow.GetBoneEntitialFrame(agentToFollow.Monster.HeadLookDirectionBoneIndex, true);
				val6 = val23.Monster.FirstPersonCameraOffsetWrtHead;
				boneEntitialFrame2.origin = ((MatrixFrame)(ref boneEntitialFrame2)).TransformToParent(ref val6);
				frame3 = agentToFollow.AgentVisuals.GetFrame();
				val25 = ((MatrixFrame)(ref frame3)).TransformToParentDouble(ref boneEntitialFrame2.origin);
				flag8 = ((int)Mission.Mode == 1 || (int)Mission.Mode == 5) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && (int)_missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == 3;
				if ((val23.GetCurrentAnimationFlag(0) & 0x800000) != 0 || (val23.GetCurrentAnimationFlag(1) & 0x800000) != 0)
				{
					MatrixFrame val26 = ((MatrixFrame)(ref frame3)).TransformToParent(ref boneEntitialFrame2);
					((Mat3)(ref val26.rotation)).MakeUnit();
					CameraBearing = ((Vec3)(ref val26.rotation.f)).RotationZ;
					CameraElevation = ((Vec3)(ref val26.rotation.f)).RotationX;
				}
				else
				{
					if (flag8)
					{
						goto IL_0da3;
					}
					if (agentToFollow.IsMainAgent && _missionMainAgentController != null)
					{
						val6 = _missionMainAgentController.CustomLookDir;
						if (((Vec3)(ref val6)).IsNonZero)
						{
							goto IL_0da3;
						}
					}
					float num36 = MBMath.WrapAngle(CameraBearing);
					float num37 = MBMath.WrapAngle(CameraElevation);
					CalculateNewBearingAndElevationForFirstPerson(agentToFollow, num36, num37, 0f, 0f, out var newBearing, out var newElevation);
					CameraBearing = MBMath.LerpRadians(num36, newBearing, Math.Min(dt * 12f, 1f), 1E-05f, 0.5f);
					CameraElevation = MBMath.LerpRadians(num37, newElevation, Math.Min(dt * 12f, 1f), 1E-05f, 0.5f);
				}
				goto IL_0f99;
			}
			val = CombatCamera.Frame;
		}
		goto IL_2b37;
		IL_2b37:
		if (_cameraSpecialCurrentFOV != _cameraSpecialTargetFOV)
		{
			float num38 = _cameraSpecialTargetFOV - _cameraSpecialCurrentFOV;
			if (flag3 || MathF.Abs(num38) < 0.001f)
			{
				_cameraSpecialCurrentFOV = _cameraSpecialTargetFOV;
			}
			else
			{
				_cameraSpecialCurrentFOV += num38 * 3f * dt;
			}
		}
		float num39 = (Mission.CameraIsFirstPerson ? 0.065f : 0.1f);
		CombatCamera.Frame = val;
		if (IsPhotoModeEnabled)
		{
			float depthOfFieldFocus = 0f;
			float num40 = 0f;
			float num41 = 0f;
			float num42 = 0f;
			bool flag9 = false;
			scene.GetPhotoModeFocus(ref num40, ref num41, ref depthOfFieldFocus, ref num42, ref flag9);
			scene.SetDepthOfFieldFocus(depthOfFieldFocus);
			scene.SetDepthOfFieldParameters(num40, num41, flag9);
		}
		else if (((int)Mission.Mode == 1 || (int)Mission.Mode == 5) && _missionMainAgentController?.InteractionComponent.CurrentFocusedObject != null && (int)_missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType == 3)
		{
			IFocusable obj3 = _missionMainAgentController?.InteractionComponent.CurrentFocusedObject;
			Agent val27 = (Agent)(object)((obj3 is Agent) ? obj3 : null);
			scene.SetDepthOfFieldParameters(5f, 5f, false);
			val6 = val.origin - val27.AgentVisuals.GetGlobalStableEyePoint(true);
			scene.SetDepthOfFieldFocus(((Vec3)(ref val6)).Length);
		}
		else if (!MBMath.ApproximatelyEqualsTo(_zoomAmount, 1f, 1E-05f))
		{
			scene.SetDepthOfFieldParameters(0f, 0f, false);
			scene.SetDepthOfFieldFocus(0f);
		}
		CombatCamera.SetFovVertical(IsPhotoModeEnabled ? (num * (MathF.PI / 180f)) : (_cameraSpecialCurrentFOV * Mission.CustomCameraFovMultiplier * (CameraViewAngle / 65f) * (MathF.PI / 180f)), Screen.AspectRatio, num39, 12500f);
		SceneView.SetCamera(CombatCamera);
		Vec3 val28 = ((agentToFollow != null) ? agentToFollow.GetEyeGlobalPosition() : val.origin);
		if (agentToFollow != null && Mission.ListenerAndAttenuationPosBlendFactor > 0f)
		{
			Vec3 val29 = val.origin - val28;
			val28 += val29 * Mission.ListenerAndAttenuationPosBlendFactor;
		}
		Mission.SetCameraFrame(ref val, 65f / CameraViewAngle, ref val28);
		if (LastFollowedAgent != null && LastFollowedAgent != Mission.MainAgent && (agentToFollow == Mission.MainAgent || agentToFollow == null))
		{
			this.OnSpectateAgentFocusOut?.Invoke(LastFollowedAgent);
		}
		LastFollowedAgent = agentToFollow;
		LastFollowedAgentVisuals = agentVisualToFollow;
		_cameraApplySpecialMovementsInstantly = false;
		_cameraAddSpecialMovement = false;
		_cameraAddSpecialPositionalMovement = false;
		return;
		IL_234d:
		val18 += val22;
		val19 += val22;
		val19.z += _cameraTargetAddedHeight;
		int num43 = 0;
		bool flag10 = agentToFollow != null;
		Vec3 val30 = val22 + ((!flag10) ? Vec3.Invalid : ((flag4 && agentToFollow.MountAgent.AgentVisuals.IsValid()) ? agentToFollow.MountAgent.GetChestGlobalPosition() : agentToFollow.GetChestGlobalPosition()));
		Vec3 val31 = val19 + val14.rotation.u * num28;
		if (!Mission.CameraIsFirstPerson)
		{
			val6 = Mission.CustomCameraLocalRotationalOffset;
			if (((Vec3)(ref val6)).IsNonZero || _cameraShakeStartTime > 0f)
			{
				val6 = Mission.CustomCameraLocalRotationalOffset;
				Vec2 val32 = ((Vec3)(ref val6)).AsVec2;
				if (_cameraShakeStartTime > 0f)
				{
					float currentTime = Mission.Current.CurrentTime;
					float num44 = (1f - MathF.Pow((currentTime - _cameraShakeStartTime) / 2f, 0.4f)) * _cameraShakeIntensity * 0.6f * 0.02f;
					if (num44 > 0f)
					{
						Vec3 val33 = MBPerlin.NoiseVec3(_cameraShakeCurrentTimeWithFrequency);
						val32 += new Vec2(val33.x * num44, val33.y * num44);
					}
				}
				val.rotation.u = ((Vec3)(ref val.rotation.u)).RotateAboutAnArbitraryVector(val.rotation.s, val32.x);
				val.rotation.u = ((Vec3)(ref val.rotation.u)).RotateAboutAnArbitraryVector(val.rotation.f, val32.y);
				ref Mat3 rotation2 = ref val.rotation;
				val6 = Vec3.CrossProduct(val.rotation.u, val.rotation.s);
				rotation2.f = ((Vec3)(ref val6)).NormalizedCopy();
				val.rotation.s = Vec3.CrossProduct(val.rotation.f, val.rotation.u);
			}
		}
		Vec3 val34 = val31 - val19;
		num28 = ((Vec3)(ref val34)).Normalize();
		bool flag11;
		Vec3 val39 = default(Vec3);
		float num49 = default(float);
		WeakGameEntity val41 = default(WeakGameEntity);
		do
		{
			Vec3 val35 = val19;
			if ((int)Mission.Mode != 1 && (int)Mission.Mode != 5)
			{
				val6 = Mission.CustomCameraLocalOffset + Mission.CustomCameraLocalOffset2;
				float num45 = Math.Max(0f, 1f - ((Vec3)(ref val6)).Length);
				if (num45 > 0f)
				{
					val35 += val14.rotation.f * num13 * num45 * (0.7f * MathF.Pow(MathF.Cos(1f / ((num28 / num13 - 0.2f) * 30f + 20f)), 3500f));
				}
			}
			Vec3 val36 = val35 + val34 * num28;
			if (flag6 || flag7)
			{
				float num46 = 0f;
				if (flag7)
				{
					float currentActionProgress = agentToFollow.GetCurrentActionProgress(0);
					num46 = currentActionProgress * currentActionProgress * 20f;
				}
				val35 = _cameraTarget + (val35 - _cameraTarget) * (5f + num46) * dt;
			}
			flag11 = false;
			MatrixFrame val37 = new MatrixFrame(ref val.rotation, ref val36);
			Camera.GetNearPlanePointsStatic(ref val37, IsPhotoModeEnabled ? (num * (MathF.PI / 180f)) : (CameraViewAngle * (MathF.PI / 180f)), Screen.AspectRatio, 0.2f, 1f, _cameraNearPlanePoints);
			Vec3 val38 = Vec3.Zero;
			for (int i = 0; i < 4; i++)
			{
				val38 += _cameraNearPlanePoints[i];
			}
			val38 *= 0.25f;
			((Vec3)(ref val39))._002Ector(((Vec3)(ref val18)).AsVec2 + val17, val35.z, -1f);
			Vec3 val40 = val39 - val38;
			for (int j = 0; j < 4; j++)
			{
				ref Vec3 reference = ref _cameraNearPlanePoints[j];
				reference += val40;
			}
			_cameraBoxPoints[0] = _cameraNearPlanePoints[3] + val37.rotation.u * 0.01f;
			_cameraBoxPoints[1] = _cameraNearPlanePoints[0];
			_cameraBoxPoints[2] = _cameraNearPlanePoints[3];
			_cameraBoxPoints[3] = _cameraNearPlanePoints[2];
			_cameraBoxPoints[4] = _cameraNearPlanePoints[1] + val37.rotation.u * 0.01f;
			_cameraBoxPoints[5] = _cameraNearPlanePoints[0] + val37.rotation.u * 0.01f;
			_cameraBoxPoints[6] = _cameraNearPlanePoints[1];
			_cameraBoxPoints[7] = _cameraNearPlanePoints[2] + val37.rotation.u * 0.01f;
			float num47 = ((IsPhotoModeEnabled && !flag && photoModeOrbit) ? _zoomAmount : 0f);
			num28 += num47;
			Vec3 zero = Vec3.Zero;
			if (!Mission.CustomCameraIgnoreCollision)
			{
				Vec3[] cameraBoxPoints = _cameraBoxPoints;
				ref Vec3 u = ref val37.rotation.u;
				float num48 = num28 + 0.5f;
				GameEntity ignoredEntityForCamera = Mission.IgnoredEntityForCamera;
				if (scene.BoxCastOnlyForCamera(cameraBoxPoints, ref val39, flag10, ref val30, ref u, num48, (ignoredEntityForCamera != null) ? ignoredEntityForCamera.WeakEntity : WeakGameEntity.Invalid, ref num49, ref zero, ref val41, (BodyFlags)544585673))
				{
					num49 = MathF.Max(Vec3.DotProduct(val37.rotation.u, zero - val35), 0.48f * num13);
					if (num49 < num28)
					{
						flag11 = true;
						num28 = num49;
					}
				}
			}
			num43++;
		}
		while (!flag5 && num43 < 5 && flag11);
		num10 = num28 - Mission.CameraAddedDistance;
		if (flag3 || (CameraResultDistanceToTarget > num28 && num43 > 1))
		{
			CameraResultDistanceToTarget = num28;
		}
		else
		{
			float num50 = MathF.Max(MathF.Abs(Mission.CameraAddedDistance - _lastCameraAddedDistance) * num13, MathF.Abs((num10 - (CameraResultDistanceToTarget - _lastCameraAddedDistance)) * dt * 3f * num13));
			CameraResultDistanceToTarget += MBMath.ClampFloat(num28 - CameraResultDistanceToTarget, 0f - num50, num50);
		}
		_lastCameraAddedDistance = Mission.CameraAddedDistance;
		_cameraTarget = val19;
		if ((int)Mission.Mode != 1 && (int)Mission.Mode != 5)
		{
			val6 = Mission.CustomCameraLocalOffset + Mission.CustomCameraLocalOffset2;
			float num51 = Math.Max(0f, 1f - ((Vec3)(ref val6)).Length);
			if (num51 > 0f)
			{
				_cameraTarget += val14.rotation.f * num13 * num51 * (0.7f * MathF.Pow(MathF.Cos(1f / ((num28 / num13 - 0.2f) * 30f + 20f)), 3500f));
			}
		}
		val.origin = _cameraTarget + val34 * CameraResultDistanceToTarget;
		if (!Mission.CameraIsFirstPerson && agentToFollow != null && agentToFollow.IsPlayerControlled)
		{
			ref Vec3 origin7 = ref val.origin;
			origin7 += Mission.CustomCameraGlobalOffset;
		}
		goto IL_2b37;
		IL_0da3:
		Vec3 val43;
		if (flag8)
		{
			IFocusable currentFocusedObject3 = _missionMainAgentController.InteractionComponent.CurrentFocusedObject;
			Agent val42 = (Agent)(object)((currentFocusedObject3 is Agent) ? currentFocusedObject3 : null);
			val6 = val42.Position;
			val6 = new Vec3(((Vec3)(ref val6)).AsVec2, val42.AgentVisuals.GetGlobalStableEyePoint(val42.IsHuman).z, -1f) - val25;
			val43 = ((Vec3)(ref val6)).NormalizedCopy();
			val6 = new Vec3(val43.y, 0f - val43.x, 0f, -1f);
			Vec3 val44 = ((Vec3)(ref val6)).NormalizedCopy();
			val43 = ((Vec3)(ref val43)).RotateAboutAnArbitraryVector(val44, (((int)Mission.Mode == 1) ? (-0.003f) : (-0.0045f)) * _cameraSpecialCurrentFOV);
		}
		else
		{
			val43 = _missionMainAgentController.CustomLookDir;
		}
		if (flag3)
		{
			CameraBearing = ((Vec3)(ref val43)).RotationZ;
			CameraElevation = ((Vec3)(ref val43)).RotationX;
		}
		else
		{
			Mat3 identity = Mat3.Identity;
			((Mat3)(ref identity)).RotateAboutUp(CameraBearing);
			((Mat3)(ref identity)).RotateAboutSide(CameraElevation);
			Vec3 f = identity.f;
			Vec3 val45 = Vec3.CrossProduct(f, val43);
			float num52 = ((Vec3)(ref val45)).Normalize();
			Vec3 val46;
			if (num52 < 0.0001f)
			{
				val46 = val43;
			}
			else
			{
				val46 = f;
				val46 = ((Vec3)(ref val46)).RotateAboutAnArbitraryVector(val45, num52 * dt * 5f);
			}
			CameraBearing = ((Vec3)(ref val46)).RotationZ;
			CameraElevation = ((Vec3)(ref val46)).RotationX;
		}
		goto IL_0f99;
		IL_0f99:
		if (agentToFollow.IsInWater())
		{
			AgentMovementMode val47 = (AgentMovementMode)(agentToFollow.MovementMode & 3);
			((Mat3)(ref val.rotation)).RotateAboutSide(MathF.PI / 2f);
			((Mat3)(ref val.rotation)).RotateAboutForward(CameraBearing);
			((Mat3)(ref val.rotation)).RotateAboutSide(((int)val47 == 2 && agentToFollow.GetCurrentVelocity().y < 0f) ? Math.Max(CameraElevation, -0.5f) : CameraElevation);
			val.origin = val25;
		}
		else
		{
			((Mat3)(ref val.rotation)).RotateAboutSide(MathF.PI / 2f);
			((Mat3)(ref val.rotation)).RotateAboutForward(CameraBearing);
			((Mat3)(ref val.rotation)).RotateAboutSide(CameraElevation);
			float actionChannelWeight = agentToFollow.GetActionChannelWeight(1);
			float num53 = MBMath.WrapAngle(CameraBearing - agentToFollow.MovementDirectionAsAngle);
			float num54 = 1f - (1f - actionChannelWeight) * MBMath.ClampFloat((MathF.Abs(num53) - 1f) * 0.66f, 0f, 1f);
			Vec3 val48 = frame3.rotation.u * 0.25f;
			Vec3 val49 = frame3.rotation.u * 0.15f + Vec3.Forward * 0.15f;
			((Vec3)(ref val49)).RotateAboutX(MBMath.ClampFloat(CameraElevation, -0.35f, 0.35f));
			((Vec3)(ref val49)).RotateAboutZ(CameraBearing);
			Vec3 val50 = ((MatrixFrame)(ref frame3)).TransformToParent(ref boneEntitialFrame.origin);
			val50 += val48;
			val50 += val49;
			if (actionChannelWeight > 0f)
			{
				_currentViewBlockingBodyCoeff = (_targetViewBlockingBodyCoeff = 1f);
				_applySmoothTransitionToVirtualEyeCamera = true;
			}
			else
			{
				val6 = val25 - val50;
				Vec3 val51 = ((Vec3)(ref val6)).NormalizedCopy();
				if (Vec3.DotProduct(val.rotation.u, val51) > 0f)
				{
					val51 = -val51;
				}
				float num55 = 0.97499996f;
				float num56 = MathF.Lerp(0.55f, 0.7f, MathF.Abs(val.rotation.u.z), 1E-05f);
				float num57 = default(float);
				if (Mission.Scene.RayCastForClosestEntityOrTerrain(val25 - val51 * (num55 * num56), val25 + val51 * (num55 * (1f - num56)), ref num57, ref val6, ref val41, 0.01f, (BodyFlags)544585673))
				{
					float num58 = (num55 - num57) / 0.065f;
					_targetViewBlockingBodyCoeff = 1f / MathF.Max(1f, num58 * num58 * num58);
				}
				else
				{
					_targetViewBlockingBodyCoeff = 1f;
				}
				if (_currentViewBlockingBodyCoeff < _targetViewBlockingBodyCoeff)
				{
					_currentViewBlockingBodyCoeff = MathF.Min(_currentViewBlockingBodyCoeff + dt * 12f, _targetViewBlockingBodyCoeff);
				}
				else if (_currentViewBlockingBodyCoeff > _targetViewBlockingBodyCoeff)
				{
					_currentViewBlockingBodyCoeff = (_applySmoothTransitionToVirtualEyeCamera ? MathF.Max(_currentViewBlockingBodyCoeff - dt * 6f, _targetViewBlockingBodyCoeff) : _targetViewBlockingBodyCoeff);
				}
				else
				{
					_applySmoothTransitionToVirtualEyeCamera = false;
				}
				num54 *= _currentViewBlockingBodyCoeff;
			}
			val.origin.x = MBMath.Lerp(val50.x, val25.x, num54, 1E-05f);
			val.origin.y = MBMath.Lerp(val50.y, val25.y, num54, 1E-05f);
			val.origin.z = MBMath.Lerp(val50.z, val25.z, actionChannelWeight, 1E-05f);
		}
		goto IL_2b37;
		IL_222f:
		if (_cameraShakeStartTime > 0f)
		{
			float applicationTime = Game.Current.ApplicationTime;
			if (_cameraShakeStartTime > applicationTime - 2f)
			{
				float num59 = 1f - MathF.Pow((applicationTime - _cameraShakeStartTime) / 2f, 0.4f);
				float num60 = num59 * _cameraShakeIntensity * 0.6f;
				float num61 = num60 * 0.02f;
				_cameraShakeCurrentTimeWithFrequency += dt * 15f * num59;
				if (num60 > 0f)
				{
					Vec3 val52 = MBPerlin.NoiseVec3(_cameraShakeCurrentTimeWithFrequency);
					val22 += val.rotation.s * (val52.x * num60);
					val22 += val.rotation.f * (val52.z * num60);
					Mission.SetCustomCameraLocalRotationalOffset(new Vec3(val52.x * num61, val52.y * num61, 0f, -1f));
				}
			}
			else
			{
				_cameraShakeStartTime = 0f;
			}
		}
		goto IL_234d;
	}

	protected virtual bool CanToggleCamera()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		if ((int)Mission.Mode != 6)
		{
			return (int)Mission.Mode != 9;
		}
		return false;
	}

	protected virtual bool CanViewCharacter()
	{
		return true;
	}

	public bool IsViewingCharacter()
	{
		if (!CanViewCharacter())
		{
			return false;
		}
		if (!Mission.CameraIsFirstPerson && !IsOrderMenuOpen)
		{
			return ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(25);
		}
		return false;
	}

	private void SetCameraFrameToMapView()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Invalid comparison between Unknown and I4
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = MatrixFrame.Identity;
		bool flag = false;
		if (GameNetwork.IsMultiplayer)
		{
			GameEntity val2 = Mission.Scene.FindEntityWithTag("mp_camera_start_pos");
			if (val2 != (GameEntity)null)
			{
				val = val2.GetGlobalFrame();
				((Mat3)(ref val.rotation)).Orthonormalize();
				CameraBearing = ((Vec3)(ref val.rotation.f)).RotationZ;
				CameraElevation = ((Vec3)(ref val.rotation.f)).RotationX - MathF.PI / 2f;
			}
			else
			{
				Debug.FailedAssert("Multiplayer scene does not contain a camera frame", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Screens\\MissionScreen.cs", "SetCameraFrameToMapView", 2236);
				flag = true;
			}
		}
		else if ((int)Mission.Mode == 6)
		{
			GameEntity val3 = (((int)Mission.PlayerTeam.Side != 1) ? (Mission.Scene.FindEntityWithTag("strategyCameraDefender") ?? Mission.Scene.FindEntityWithTag("strategyCameraAttacker")) : (Mission.Scene.FindEntityWithTag("strategyCameraAttacker") ?? Mission.Scene.FindEntityWithTag("strategyCameraDefender")));
			if (val3 != (GameEntity)null)
			{
				val = val3.GetGlobalFrame();
				CameraBearing = ((Vec3)(ref val.rotation.f)).RotationZ;
				CameraElevation = ((Vec3)(ref val.rotation.f)).RotationX - MathF.PI / 2f;
			}
			else if (Mission.HasSpawnPath)
			{
				float num = Mission.ComputeSpawnPathDeploymentOffset(100, Mission.GetInitialSpawnPath());
				WorldFrame spawnPathFrame = Mission.GetSpawnPathFrame(Mission.PlayerTeam.Side, num, 0f);
				val = ((WorldFrame)(ref spawnPathFrame)).ToGroundMatrixFrame();
				val.origin.z += 25f;
				val.origin -= 25f * val.rotation.f;
				CameraBearing = ((Vec3)(ref val.rotation.f)).RotationZ;
				CameraElevation = -MathF.PI / 4f;
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			Vec3 val4 = default(Vec3);
			((Vec3)(ref val4))._002Ector(float.MaxValue, float.MaxValue, 0f, -1f);
			Vec3 val5 = default(Vec3);
			((Vec3)(ref val5))._002Ector(float.MinValue, float.MinValue, 0f, -1f);
			if (Mission.Boundaries.ContainsKey("walk_area"))
			{
				foreach (Vec2 item in Mission.Boundaries["walk_area"])
				{
					val4.x = MathF.Min(val4.x, item.x);
					val4.y = MathF.Min(val4.y, item.y);
					val5.x = MathF.Max(val5.x, item.x);
					val5.y = MathF.Max(val5.y, item.y);
				}
			}
			else
			{
				Mission.Scene.GetBoundingBox(ref val4, ref val5);
			}
			Vec3 val6 = (val.origin = (val4 + val5) * 0.5f);
			val.origin.z += 10000f;
			val.origin.z = Mission.Scene.GetGroundHeightAtPosition(val6, (BodyFlags)544321929) + 10f;
		}
		CombatCamera.Frame = val;
	}

	private bool HandleUserInputDebug()
	{
		bool result = false;
		if (((ScreenBase)this).DebugInput.IsHotKeyPressed("MissionScreenHotkeyResetDebugVariables"))
		{
			GameNetwork.ResetDebugVariables();
		}
		if (((ScreenBase)this).DebugInput.IsHotKeyPressed("FixSkeletons"))
		{
			MBCommon.FixSkeletons();
			MessageManager.DisplayMessage("Skeleton models are reloaded...", 4294901760u);
			result = true;
		}
		return result;
	}

	private void HandleUserInput(float dt)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Invalid comparison between Unknown and I4
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Invalid comparison between Unknown and I4
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Invalid comparison between Unknown and I4
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Invalid comparison between Unknown and I4
		//IL_057e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0583: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Invalid comparison between Unknown and I4
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_061f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0624: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0904: Unknown result type (might be due to invalid IL or missing references)
		//IL_090f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0914: Unknown result type (might be due to invalid IL or missing references)
		//IL_091f: Unknown result type (might be due to invalid IL or missing references)
		//IL_092a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0934: Unknown result type (might be due to invalid IL or missing references)
		//IL_0939: Unknown result type (might be due to invalid IL or missing references)
		//IL_0999: Unknown result type (might be due to invalid IL or missing references)
		//IL_099f: Invalid comparison between Unknown and I4
		//IL_0a49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4f: Invalid comparison between Unknown and I4
		//IL_09bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5d: Invalid comparison between Unknown and I4
		//IL_0a8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a91: Invalid comparison between Unknown and I4
		//IL_09e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bfd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c18: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c1e: Invalid comparison between Unknown and I4
		//IL_0c26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c2c: Invalid comparison between Unknown and I4
		//IL_0c70: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c75: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c5d: Invalid comparison between Unknown and I4
		bool flag = false;
		bool flag2 = _isGamepadActive && PhotoModeRequiresMouse;
		if (Mission == null || (int)Mission.CurrentState == 3)
		{
			return;
		}
		if (!flag && Game.Current.CheatMode)
		{
			flag = HandleUserInputCheatMode(dt);
		}
		if (!flag && ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(16) && Mission.CanTakeControlOfAgent(LastFollowedAgent))
		{
			Mission.TakeControlOfAgent(LastFollowedAgent);
			flag = true;
		}
		if (flag)
		{
			return;
		}
		float num = ((ScreenLayer)SceneLayer).Input.GetMouseSensitivity();
		if (!((ScreenBase)this).MouseVisible && Mission.MainAgent != null && (int)Mission.MainAgent.State == 1 && Mission.MainAgent.IsLookRotationInSlowMotion)
		{
			num *= ManagedParameters.Instance.GetManagedParameter((ManagedParametersEnum)1);
		}
		float num2 = dt / 0.0009f;
		float num3 = dt / 0.0009f;
		float num4 = 0f;
		float num5 = 0f;
		if ((!MBCommon.IsPaused || IsPhotoModeEnabled) && !IsRadialMenuActive && ((NativeObject)(object)CustomCamera == (NativeObject)null || AllowInputWithCustomCamera) && _cameraSpecialTargetFOV > 9f && (int)Mission.Mode != 5)
		{
			if (((ScreenBase)this).MouseVisible && !((ScreenLayer)SceneLayer).Input.IsKeyDown((InputKey)225))
			{
				if ((int)Mission.Mode != 1)
				{
					if ((int)Mission.Mode == 6)
					{
						num4 = num2 * ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisX");
						num5 = (0f - num3) * ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisY");
					}
					else
					{
						if (((ScreenLayer)SceneLayer).Input.GetMousePositionRanged().x <= 0.01f)
						{
							num4 = -400f * dt;
						}
						else if (((ScreenLayer)SceneLayer).Input.GetMousePositionRanged().x >= 0.99f)
						{
							num4 = 400f * dt;
						}
						if (((ScreenLayer)SceneLayer).Input.GetMousePositionRanged().y <= 0.01f)
						{
							num5 = -400f * dt;
						}
						else if (((ScreenLayer)SceneLayer).Input.GetMousePositionRanged().y >= 0.99f)
						{
							num5 = 400f * dt;
						}
					}
				}
			}
			else if (!((ScreenLayer)SceneLayer).Input.GetIsMouseActive())
			{
				float gameKeyAxis = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisX");
				float gameKeyAxis2 = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisY");
				if (gameKeyAxis > 0.9f || gameKeyAxis < -0.9f)
				{
					num2 = dt / 0.00045f;
				}
				if (gameKeyAxis2 > 0.9f || gameKeyAxis2 < -0.9f)
				{
					num3 = dt / 0.00045f;
				}
				if (_zoomToggled)
				{
					num2 *= BannerlordConfig.ZoomSensitivityModifier;
					num3 *= BannerlordConfig.ZoomSensitivityModifier;
				}
				num4 = num2 * ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisX") + ((ScreenLayer)SceneLayer).Input.GetMouseMoveX();
				num5 = (0f - num3) * ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisY") + ((ScreenLayer)SceneLayer).Input.GetMouseMoveY();
				if (_missionMainAgentController.IsPlayerAiming && NativeOptions.GetConfig((NativeOptionsType)15) == 1f)
				{
					float config = NativeOptions.GetConfig((NativeOptionsType)16);
					float gyroX = Input.GetGyroX();
					Input.GetGyroY();
					float gyroZ = Input.GetGyroZ();
					num4 += config * gyroZ * 12f * -1f;
					num5 += config * gyroX * 12f * -1f;
				}
			}
			else
			{
				num4 = ((ScreenLayer)SceneLayer).Input.GetMouseMoveX();
				num5 = ((ScreenLayer)SceneLayer).Input.GetMouseMoveY();
				if (_zoomAmount > 0.66f)
				{
					num4 *= BannerlordConfig.ZoomSensitivityModifier * _zoomAmount;
					num5 *= BannerlordConfig.ZoomSensitivityModifier * _zoomAmount;
				}
			}
		}
		if (NativeConfig.EnableEditMode && ((ScreenBase)this).DebugInput.IsHotKeyPressed("MissionScreenHotkeySwitchCameraSmooth"))
		{
			_cameraSmoothMode = !_cameraSmoothMode;
			MessageManager.DisplayMessage(_cameraSmoothMode ? "Camera smooth mode Enabled." : "Camera smooth mode Disabled.", uint.MaxValue);
		}
		float num6 = 0.0035f;
		float num8;
		if (_cameraSmoothMode)
		{
			num6 *= 0.02f;
			float num7 = 0.02f + dt - 8f * (dt * dt);
			num8 = MathF.Max(0f, 1f - 2f * num7);
		}
		else
		{
			num8 = 0f;
		}
		_cameraBearingDelta *= num8;
		_cameraElevationDelta *= num8;
		bool isSessionActive = GameNetwork.IsSessionActive;
		float num9 = num6 * num;
		float num10 = (0f - num4) * num9;
		float num11 = (NativeConfig.InvertMouse ? num5 : (0f - num5)) * num9;
		if (isSessionActive)
		{
			float num12 = 0.3f + 10f * dt;
			num10 = MBMath.ClampFloat(num10, 0f - num12, num12);
			num11 = MBMath.ClampFloat(num11, 0f - num12, num12);
		}
		_cameraBearingDelta += num10;
		_cameraElevationDelta += num11;
		if (isSessionActive)
		{
			float num13 = 0.3f + 10f * dt;
			_cameraBearingDelta = MBMath.ClampFloat(_cameraBearingDelta, 0f - num13, num13);
			_cameraElevationDelta = MBMath.ClampFloat(_cameraElevationDelta, 0f - num13, num13);
		}
		SpectatorData spectatingData = GetSpectatingData(CombatCamera.Frame.origin);
		Agent agentToFollow = ((SpectatorData)(ref spectatingData)).AgentToFollow;
		MissionWeapon wieldedWeapon;
		if (Mission.CameraIsFirstPerson && agentToFollow != null && (int)agentToFollow.Controller == 2 && agentToFollow.HasMount)
		{
			if (ManagedOptions.GetConfig((ManagedOptionsType)7) == 1f)
			{
				wieldedWeapon = agentToFollow.WieldedWeapon;
				if (!((MissionWeapon)(ref wieldedWeapon)).IsEmpty)
				{
					wieldedWeapon = agentToFollow.WieldedWeapon;
					if (((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsRangedWeapon)
					{
						goto IL_0641;
					}
				}
			}
			if (ManagedOptions.GetConfig((ManagedOptionsType)7) != 2f)
			{
				goto IL_0634;
			}
			wieldedWeapon = agentToFollow.WieldedWeapon;
			if (!((MissionWeapon)(ref wieldedWeapon)).IsEmpty)
			{
				wieldedWeapon = agentToFollow.WieldedWeapon;
				if (!((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsMeleeWeapon)
				{
					goto IL_0634;
				}
			}
			goto IL_0641;
		}
		goto IL_065c;
		IL_0634:
		if (ManagedOptions.GetConfig((ManagedOptionsType)7) == 3f)
		{
			goto IL_0641;
		}
		goto IL_065c;
		IL_0d61:
		CameraBearing += _cameraBearingDelta;
		CameraElevation += _cameraElevationDelta;
		CameraElevation = MBMath.ClampFloat(CameraElevation, -1.3659099f, MathF.PI * 5f / 14f);
		goto IL_0da2;
		IL_0da2:
		if (LockCameraMovement)
		{
			_cameraToggleStartTime = float.MaxValue;
		}
		else
		{
			if (!CanToggleCamera())
			{
				return;
			}
			if (!Input.IsMouseActive)
			{
				float applicationTime = Time.ApplicationTime;
				if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(27))
				{
					if (((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("MovementAxisX") <= 0.1f && ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("MovementAxisY") <= 0.1f)
					{
						_cameraToggleStartTime = applicationTime;
					}
				}
				else if (!((ScreenLayer)SceneLayer).Input.IsGameKeyDown(27))
				{
					_cameraToggleStartTime = float.MaxValue;
				}
				if (GetCameraToggleProgress() >= 1f)
				{
					_cameraToggleStartTime = float.MaxValue;
					Mission.CameraIsFirstPerson = !Mission.CameraIsFirstPerson;
					_cameraApplySpecialMovementsInstantly = true;
				}
			}
			else if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(27))
			{
				Mission.CameraIsFirstPerson = !Mission.CameraIsFirstPerson;
				_cameraApplySpecialMovementsInstantly = true;
			}
		}
		return;
		IL_065c:
		if (Mission.CustomCameraFixedDistance == float.MinValue)
		{
			if (InputManager.IsGameKeyDown(28))
			{
				Mission.CameraAddedDistance -= 2.1f * dt;
			}
			if (InputManager.IsGameKeyDown(29))
			{
				Mission.CameraAddedDistance += 2.1f * dt;
			}
		}
		Mission.CameraAddedDistance = MBMath.ClampFloat(Mission.CameraAddedDistance, 0.7f, 2.4f);
		_isGamepadActive = Input.IsGamepadActive;
		if (!_isGamepadActive)
		{
			goto IL_0730;
		}
		Agent mainAgent = Mission.MainAgent;
		if (mainAgent != null)
		{
			wieldedWeapon = mainAgent.WieldedWeapon;
			WeaponComponentData currentUsageItem = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
			if (((currentUsageItem != null) ? new bool?(currentUsageItem.IsRangedWeapon) : ((bool?)null)) == true)
			{
				goto IL_0730;
			}
		}
		goto IL_0750;
		IL_0759:
		int num14;
		bool flag3 = (byte)num14 != 0;
		if (flag3)
		{
			if (!Input.IsGamepadActive)
			{
				_zoomToggled = false;
			}
			else if (((ScreenLayer)SceneLayer).Input.IsHotKeyPressed("ToggleZoom"))
			{
				_zoomToggled = !_zoomToggled;
			}
		}
		else
		{
			_zoomToggled = false;
		}
		bool photoModeOrbit = Mission.Scene.GetPhotoModeOrbit();
		if (IsPhotoModeEnabled)
		{
			if (photoModeOrbit && !flag2)
			{
				_zoomAmount -= ((ScreenLayer)SceneLayer).Input.GetDeltaMouseScroll() * 0.002f;
				_zoomAmount = MBMath.ClampFloat(_zoomAmount, 0f, 50f);
			}
		}
		else
		{
			if (agentToFollow != null && agentToFollow.IsMine && (_zoomToggled || (flag3 && ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(24))))
			{
				_zoomAmount += 5f * dt;
			}
			else
			{
				_zoomAmount -= 5f * dt;
			}
			_zoomAmount = MBMath.ClampFloat(_zoomAmount, 0f, 1f);
		}
		if (!IsPhotoModeEnabled)
		{
			if (MBMath.ApproximatelyEqualsTo(_zoomAmount, 1f, 1E-05f))
			{
				Mission.Scene.SetDepthOfFieldParameters(_zoomAmount * 160f * 110f, _zoomAmount * 1500f * 0.3f, false);
			}
			else
			{
				Mission.Scene.SetDepthOfFieldParameters(0f, 0f, false);
			}
		}
		float depthOfFieldFocus = default(float);
		Mission.Scene.RayCastForClosestEntityOrTerrain(CombatCamera.Position + CombatCamera.Direction * _cameraRayCastOffset, CombatCamera.Position + CombatCamera.Direction * 3000f, ref depthOfFieldFocus, 0.01f, (BodyFlags)79617);
		Mission.Scene.SetDepthOfFieldFocus(depthOfFieldFocus);
		Agent mainAgent2 = Mission.MainAgent;
		if (mainAgent2 != null && !IsPhotoModeEnabled)
		{
			Vec3 val;
			if (_isPlayerAgentAdded)
			{
				_isPlayerAgentAdded = false;
				if ((int)Mission.Mode != 6)
				{
					float cameraBearing;
					if (!Mission.CameraIsFirstPerson)
					{
						cameraBearing = mainAgent2.MovementDirectionAsAngle;
					}
					else
					{
						val = mainAgent2.LookDirection;
						cameraBearing = ((Vec3)(ref val)).RotationZ;
					}
					CameraBearing = cameraBearing;
					float cameraElevation;
					if (!Mission.CameraIsFirstPerson)
					{
						cameraElevation = 0f;
					}
					else
					{
						val = mainAgent2.LookDirection;
						cameraElevation = ((Vec3)(ref val)).RotationX;
					}
					CameraElevation = cameraElevation;
					_cameraSpecialTargetAddedBearing = 0f;
					_cameraSpecialTargetAddedElevation = 0f;
					_cameraSpecialCurrentAddedBearing = 0f;
					_cameraSpecialCurrentAddedElevation = 0f;
				}
			}
			if (!(Mission.ClearSceneTimerElapsedTime >= 0f))
			{
				return;
			}
			if (IsViewingCharacter() || (int)Mission.Mode == 1 || (int)Mission.Mode == 5 || mainAgent2.IsLookDirectionLocked || _missionMainAgentController?.LockedAgent != null)
			{
				if ((int)Mission.Mode != 5)
				{
					if (_missionMainAgentController.LockedAgent != null)
					{
						val = mainAgent2.LookDirection;
						CameraBearing = ((Vec3)(ref val)).RotationZ;
						val = mainAgent2.LookDirection;
						CameraElevation = ((Vec3)(ref val)).RotationX;
					}
					else
					{
						_cameraSpecialTargetAddedBearing = MBMath.WrapAngle(_cameraSpecialTargetAddedBearing + _cameraBearingDelta);
						_cameraSpecialTargetAddedElevation = MBMath.WrapAngle(_cameraSpecialTargetAddedElevation + _cameraElevationDelta);
						_cameraSpecialCurrentAddedBearing = MBMath.WrapAngle(_cameraSpecialCurrentAddedBearing + _cameraBearingDelta);
						_cameraSpecialCurrentAddedElevation = MBMath.WrapAngle(_cameraSpecialCurrentAddedElevation + _cameraElevationDelta);
					}
				}
				float num15 = CameraElevation + _cameraSpecialTargetAddedElevation;
				num15 = MBMath.ClampFloat(num15, -1.3659099f, MathF.PI * 5f / 14f);
				_cameraSpecialTargetAddedElevation = num15 - CameraElevation;
				num15 = CameraElevation + _cameraSpecialCurrentAddedElevation;
				num15 = MBMath.ClampFloat(num15, -1.3659099f, MathF.PI * 5f / 14f);
				_cameraSpecialCurrentAddedElevation = num15 - CameraElevation;
				goto IL_0da2;
			}
			_cameraSpecialTargetAddedBearing = 0f;
			_cameraSpecialTargetAddedElevation = 0f;
			if (Mission.CameraIsFirstPerson && agentToFollow != null && agentToFollow == Mission.MainAgent && !IsPhotoModeEnabled && !Extensions.HasAnyFlag<AnimFlags>(agentToFollow.GetCurrentAnimationFlag(0), (AnimFlags)8388608) && !Extensions.HasAnyFlag<AnimFlags>(agentToFollow.GetCurrentAnimationFlag(1), (AnimFlags)8388608) && (((int)Mission.Mode != 1 && (int)Mission.Mode != 5) || _missionMainAgentController?.InteractionComponent.CurrentFocusedObject == null || (int)_missionMainAgentController.InteractionComponent.CurrentFocusedObject.FocusableObjectType != 3))
			{
				if (_missionMainAgentController != null)
				{
					val = _missionMainAgentController.CustomLookDir;
					if (((Vec3)(ref val)).IsNonZero)
					{
						goto IL_0d61;
					}
				}
				float num16 = MBMath.WrapAngle(CameraBearing + _cameraBearingDelta);
				float num17 = MBMath.WrapAngle(CameraElevation + _cameraElevationDelta);
				CalculateNewBearingAndElevationForFirstPerson(agentToFollow, CameraBearing, CameraElevation, _cameraBearingDelta, _cameraElevationDelta, out var newBearing, out var newElevation);
				if (newBearing != num16)
				{
					float num18 = MBMath.WrapAngle(_cameraBearingDelta);
					ValueTuple<float, float> valueTuple = MathF.MinMax(0f - num18, num18);
					float item = valueTuple.Item1;
					float item2 = valueTuple.Item2;
					_cameraBearingDelta = MBMath.ClampFloat(MBMath.WrapAngle(newBearing - CameraBearing), item, item2);
				}
				if (newElevation != num17)
				{
					float num19 = MBMath.WrapAngle(_cameraElevationDelta);
					ValueTuple<float, float> valueTuple2 = MathF.MinMax(0f - num19, num19);
					float item3 = valueTuple2.Item1;
					float item4 = valueTuple2.Item2;
					_cameraElevationDelta = MBMath.ClampFloat(MBMath.WrapAngle(newElevation - CameraElevation), item3, item4);
				}
			}
			goto IL_0d61;
		}
		if (IsPhotoModeEnabled && Mission.CameraIsFirstPerson)
		{
			Mission.CameraIsFirstPerson = false;
		}
		CameraBearing += _cameraBearingDelta;
		CameraElevation += _cameraElevationDelta;
		CameraElevation = MBMath.ClampFloat(CameraElevation, -1.3659099f, MathF.PI * 5f / 14f);
		return;
		IL_0730:
		if (!((NativeObject)(object)CustomCamera == (NativeObject)null) || IsRadialMenuActive)
		{
			goto IL_0750;
		}
		num14 = 1;
		goto IL_0759;
		IL_0750:
		num14 = (_forceCanZoom ? 1 : 0);
		goto IL_0759;
		IL_0641:
		_cameraBearingDelta += agentToFollow.MountAgent.GetTurnSpeed() * dt;
		goto IL_065c;
	}

	public float GetCameraToggleProgress()
	{
		if (_cameraToggleStartTime != float.MaxValue && ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(27))
		{
			return (Time.ApplicationTime - _cameraToggleStartTime) / 0.5f;
		}
		return 0f;
	}

	private bool HandleUserInputCheatMode(float dt)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Invalid comparison between Unknown and I4
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Invalid comparison between Unknown and I4
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Invalid comparison between Unknown and I4
		bool result = false;
		if (!GameNetwork.IsMultiplayer)
		{
			if (InputManager.IsHotKeyPressed("EnterSlowMotion"))
			{
				float num = default(float);
				if (Mission.GetRequestedTimeSpeed(1121, ref num))
				{
					Mission.RemoveTimeSpeedRequest(1121);
				}
				else
				{
					Mission.AddTimeSpeedRequest(new TimeSpeedRequest(0.1f, 1121));
				}
				result = true;
			}
			float num2 = default(float);
			if (Mission.GetRequestedTimeSpeed(1121, ref num2))
			{
				if (InputManager.IsHotKeyDown("MissionScreenHotkeyIncreaseSlowMotionFactor"))
				{
					Mission.RemoveTimeSpeedRequest(1121);
					num2 = MBMath.ClampFloat(num2 + 0.5f * dt, 0f, 1f);
					Mission.AddTimeSpeedRequest(new TimeSpeedRequest(num2, 1121));
				}
				if (InputManager.IsHotKeyDown("MissionScreenHotkeyDecreaseSlowMotionFactor"))
				{
					Mission.RemoveTimeSpeedRequest(1121);
					num2 = MBMath.ClampFloat(num2 - 0.5f * dt, 0f, 1f);
					Mission.AddTimeSpeedRequest(new TimeSpeedRequest(num2, 1121));
				}
			}
			if (InputManager.IsHotKeyPressed("Pause"))
			{
				_missionState.Paused = !_missionState.Paused;
				result = true;
			}
			if (InputManager.IsHotKeyPressed("MissionScreenHotkeyHealYourSelf") && Mission.MainAgent != null)
			{
				Mission.MainAgent.Health = Mission.MainAgent.HealthLimit;
				result = true;
			}
			if (InputManager.IsHotKeyPressed("MissionScreenHotkeyHealYourHorse"))
			{
				Agent mainAgent = Mission.MainAgent;
				if (((mainAgent != null) ? mainAgent.MountAgent : null) != null)
				{
					Mission.MainAgent.MountAgent.Health = Mission.MainAgent.MountAgent.HealthLimit;
					result = true;
				}
			}
			if (!InputManager.IsShiftDown())
			{
				if (!InputManager.IsAltDown())
				{
					if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillEnemyAgent"))
					{
						return Mission.Current.KillCheats(false, true, false, false);
					}
				}
				else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillAllEnemyAgents"))
				{
					return Mission.Current.KillCheats(true, true, false, false);
				}
			}
			else if (!InputManager.IsAltDown())
			{
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillEnemyHorse"))
				{
					return Mission.Current.KillCheats(false, true, true, false);
				}
			}
			else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillAllEnemyHorses"))
			{
				return Mission.Current.KillCheats(true, true, true, false);
			}
			if (!InputManager.IsShiftDown())
			{
				if (!InputManager.IsAltDown())
				{
					if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillFriendlyAgent"))
					{
						return Mission.Current.KillCheats(false, false, false, false);
					}
				}
				else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillAllFriendlyAgents"))
				{
					return Mission.Current.KillCheats(true, false, false, false);
				}
			}
			else if (!InputManager.IsAltDown())
			{
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillFriendlyHorse"))
				{
					return Mission.Current.KillCheats(false, false, true, false);
				}
			}
			else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillAllFriendlyHorses"))
			{
				return Mission.Current.KillCheats(true, false, true, false);
			}
			if (!InputManager.IsShiftDown())
			{
				if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillYourSelf"))
				{
					return Mission.Current.KillCheats(false, false, false, true);
				}
			}
			else if (InputManager.IsHotKeyPressed("MissionScreenHotkeyKillYourHorse"))
			{
				return Mission.Current.KillCheats(false, false, true, true);
			}
			if ((GameNetwork.IsServerOrRecorder || !GameNetwork.IsMultiplayer) && InputManager.IsHotKeyPressed("MissionScreenHotkeyGhostCam"))
			{
				IsCheatGhostMode = !IsCheatGhostMode;
			}
		}
		if (!GameNetwork.IsSessionActive)
		{
			if (InputManager.IsHotKeyPressed("MissionScreenHotkeySwitchAgentToAi"))
			{
				Debug.Print("Cheat: SwitchAgentToAi", 0, (DebugColor)12, 17592186044416uL);
				if (Mission.MainAgent != null && Mission.MainAgent.IsActive())
				{
					Mission.MainAgent.Controller = (AgentControllerType)(((int)Mission.MainAgent.Controller == 2) ? 1 : 2);
					result = true;
				}
			}
			if (InputManager.IsHotKeyPressed("MissionScreenHotkeyControlFollowedAgent"))
			{
				Debug.Print("Cheat: ControlFollowedAgent", 0, (DebugColor)12, 17592186044416uL);
				if (Mission.MainAgent != null)
				{
					if ((int)Mission.MainAgent.Controller == 2)
					{
						Mission.MainAgent.Controller = (AgentControllerType)1;
						if (LastFollowedAgent != null)
						{
							LastFollowedAgent.Controller = (AgentControllerType)2;
						}
					}
					else
					{
						foreach (Agent item in (List<Agent>)(object)Mission.Agents)
						{
							if ((int)item.Controller == 2)
							{
								item.Controller = (AgentControllerType)1;
							}
						}
						Mission.MainAgent.Controller = (AgentControllerType)2;
					}
					result = true;
				}
				else
				{
					if (LastFollowedAgent != null)
					{
						LastFollowedAgent.Controller = (AgentControllerType)2;
					}
					result = true;
				}
			}
		}
		return result;
	}

	public void AddMissionView(MissionView missionView)
	{
		Mission.AddMissionBehavior((MissionBehavior)(object)missionView);
		RegisterView(missionView);
		missionView.OnMissionScreenInitialize();
		Debug.ReportMemoryBookmark("MissionView Initialized: " + ((object)missionView).GetType().Name);
	}

	public void ScreenPointToWorldRay(Vec2 screenPoint, out Vec3 rayBegin, out Vec3 rayEnd)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		rayBegin = Vec3.Invalid;
		rayEnd = Vec3.Invalid;
		Vec2 val = SceneView.ScreenPointToViewportPoint(screenPoint);
		CombatCamera.ViewportPointToWorldRay(ref rayBegin, ref rayEnd, val);
		float num = -1f;
		foreach (KeyValuePair<string, ICollection<Vec2>> boundary in Mission.Boundaries)
		{
			float boundaryRadius = Mission.Boundaries.GetBoundaryRadius(boundary.Key);
			if (num < boundaryRadius)
			{
				num = boundaryRadius;
			}
		}
		if (num < 0f)
		{
			num = 30f;
		}
		Vec3 val2 = rayEnd - rayBegin;
		float num2 = ((Vec3)(ref val2)).Normalize();
		rayEnd = rayBegin + val2 * MathF.Min(num2, num);
	}

	public bool GetProjectedMousePositionOnGround(out Vec3 groundPosition, out Vec3 groundNormal, BodyFlags excludeBodyOwnerFlags, bool checkOccludedSurface)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return SceneView.ProjectedMousePositionOnGround(ref groundPosition, ref groundNormal, ((ScreenBase)this).MouseVisible, excludeBodyOwnerFlags, checkOccludedSurface);
	}

	public bool GetProjectedMousePositionOnWater(out Vec3 waterPosition)
	{
		return SceneView.ProjectedMousePositionOnWater(ref waterPosition, ((ScreenBase)this).MouseVisible);
	}

	public void CancelQuickPositionOrder()
	{
		if (OrderFlag != null)
		{
			OrderFlag.IsVisible = false;
		}
	}

	public bool MissionStartedRendering()
	{
		if ((NativeObject)(object)SceneView != (NativeObject)null)
		{
			return SceneView.ReadyToRender();
		}
		return false;
	}

	public Vec3 GetOrderFlagPosition()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (OrderFlag != null)
		{
			return OrderFlag.Position;
		}
		return Vec3.Invalid;
	}

	public MatrixFrame GetOrderFlagFrame()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return OrderFlag.Frame;
	}

	private void ActivateLoadingScreen()
	{
		if (SceneLayer != null && (NativeObject)(object)SceneLayer.SceneView != (NativeObject)null)
		{
			Scene scene = SceneLayer.SceneView.GetScene();
			if ((NativeObject)(object)scene != (NativeObject)null)
			{
				scene.PreloadForRendering();
			}
		}
	}

	public void RegisterRadialMenuObject<T>(T radialMenuOwnerObject) where T : class
	{
		if (!_objectsWithActiveRadialMenu.Contains(radialMenuOwnerObject))
		{
			_objectsWithActiveRadialMenu.Add(radialMenuOwnerObject);
		}
	}

	public void UnregisterRadialMenuObject(object radialMenuOwnerObject)
	{
		if (_objectsWithActiveRadialMenu.Contains(radialMenuOwnerObject))
		{
			_objectsWithActiveRadialMenu.Remove(radialMenuOwnerObject);
		}
	}

	public void SetPhotoModeRequiresMouse(bool isRequired)
	{
		PhotoModeRequiresMouse = isRequired;
	}

	public void SetPhotoModeEnabled(bool isEnabled)
	{
		if (IsPhotoModeEnabled == isEnabled || GameNetwork.IsMultiplayer)
		{
			return;
		}
		IsPhotoModeEnabled = isEnabled;
		if (isEnabled)
		{
			MBCommon.PauseGameEngine();
			_missionViewsContainer.ForEach(delegate(MissionView missionView)
			{
				missionView.OnPhotoModeActivated();
			});
		}
		else
		{
			MBCommon.UnPauseGameEngine();
			_missionViewsContainer.ForEach(delegate(MissionView missionView)
			{
				missionView.OnPhotoModeDeactivated();
			});
		}
		Mission.Scene.SetPhotoModeOn(IsPhotoModeEnabled);
	}

	public void SetConversationActive(bool isActive)
	{
		if (IsConversationActive == isActive || GameNetwork.IsMultiplayer)
		{
			return;
		}
		IsConversationActive = isActive;
		_missionViewsContainer.ForEach(delegate(MissionView missionView)
		{
			if (isActive)
			{
				missionView.OnConversationBegin();
			}
			else
			{
				missionView.OnConversationEnd();
			}
		});
	}

	public void SetCameraLockState(bool isLocked)
	{
		LockCameraMovement = isLocked;
	}

	public void RegisterView(MissionView missionView)
	{
		_missionViewsContainer.Add(missionView);
		missionView.MissionScreen = this;
	}

	public void UnregisterView(MissionView missionView)
	{
		_missionViewsContainer.Remove(missionView);
		missionView.MissionScreen = null;
	}

	public virtual void TeleportMainAgentToCameraFocusForCheat()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame lastFinalRenderCameraFrame = Mission.Scene.LastFinalRenderCameraFrame;
		float num = default(float);
		if (Mission.Scene.RayCastForClosestEntityOrTerrain(lastFinalRenderCameraFrame.origin, lastFinalRenderCameraFrame.origin + -lastFinalRenderCameraFrame.rotation.u * 100f, ref num, 0.01f, (BodyFlags)544321929))
		{
			Vec3 origin = lastFinalRenderCameraFrame.origin + -lastFinalRenderCameraFrame.rotation.u * num;
			Vec2 val = -((Vec3)(ref lastFinalRenderCameraFrame.rotation.u)).AsVec2;
			((Vec2)(ref val)).Normalize();
			MatrixFrame val2 = new MatrixFrame
			{
				origin = origin,
				rotation = 
				{
					f = new Vec3(val.x, val.y, 0f, -1f)
				},
				rotation = 
				{
					u = new Vec3(0f, 0f, 1f, -1f)
				}
			};
			((Mat3)(ref val2.rotation)).Orthonormalize();
			Agent.Main.TeleportToPosition(val2.origin);
		}
	}

	public IAgentVisual GetPlayerAgentVisuals(MissionPeer lobbyPeer)
	{
		return lobbyPeer.GetAgentVisualForPeer(0);
	}

	public void SetAgentToFollow(Agent agent)
	{
		_agentToFollowOverride = agent;
	}

	public SpectatorData GetSpectatingData(Vec3 currentCameraPosition)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between Unknown and I4
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Invalid comparison between Unknown and I4
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Invalid comparison between Unknown and I4
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I4
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Invalid comparison between Unknown and I4
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Invalid comparison between Unknown and I4
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Invalid comparison between Unknown and I4
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Invalid comparison between Unknown and I4
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		Agent val = null;
		IAgentVisual val2 = null;
		SpectatorCameraTypes val3 = (SpectatorCameraTypes)(-1);
		bool flag = Mission.MainAgent != null && Mission.MainAgent.IsCameraAttachable() && (int)Mission.Mode != 6;
		bool flag2 = flag || (LastFollowedAgent != null && (int)LastFollowedAgent.Controller == 2 && LastFollowedAgent.IsCameraAttachable());
		MissionPeer val4 = ((GameNetwork.MyPeer != null) ? PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer) : null);
		bool flag3 = val4 != null && val4.HasSpawnedAgentVisuals;
		bool flag4 = (_missionLobbyComponent != null && ((int)_missionLobbyComponent.MissionType == 2 || (int)_missionLobbyComponent.MissionType == 0)) || (int)Mission.Mode == 6;
		SpectatorCameraTypes val5;
		if (!IsCheatGhostMode && !flag2 && flag4 && _agentToFollowOverride != null && _agentToFollowOverride.IsCameraAttachable() && !flag3)
		{
			val = _agentToFollowOverride;
			val5 = (SpectatorCameraTypes)2;
		}
		else
		{
			if (_missionCameraModeLogic != null)
			{
				val3 = _missionCameraModeLogic.GetMissionCameraLockMode(flag2);
			}
			if (IsCheatGhostMode)
			{
				val5 = (SpectatorCameraTypes)0;
			}
			else if ((int)val3 != -1)
			{
				val5 = val3;
			}
			else if ((int)Mission.Mode == 6)
			{
				val5 = (SpectatorCameraTypes)0;
			}
			else if (flag)
			{
				val5 = (SpectatorCameraTypes)1;
				val = Mission.MainAgent;
			}
			else if (flag2)
			{
				val5 = (SpectatorCameraTypes)1;
				val = LastFollowedAgent;
			}
			else if (val4 == null || GetPlayerAgentVisuals(val4) == null || (int)val3 == 0)
			{
				val5 = (GameNetwork.IsMultiplayer ? ((SpectatorCameraTypes)MultiplayerOptionsExtensions.GetIntValue((OptionType)25, (MultiplayerOptionsAccessMode)1)) : ((SpectatorCameraTypes)0));
			}
			else
			{
				val5 = (SpectatorCameraTypes)7;
				val2 = GetPlayerAgentVisuals(val4);
			}
			if (((int)val5 != 1 && (int)val5 != 7 && (int)Mission.Mode != 6) || (IsCheatGhostMode && !IsOrderMenuOpen && !IsTransferMenuOpen))
			{
				if (LastFollowedAgent != null && LastFollowedAgent.IsCameraAttachable())
				{
					val = LastFollowedAgent;
				}
				else if ((int)val5 != 0 || (_gatherCustomAgentListToSpectate != null && LastFollowedAgent != null))
				{
					val = FindNextCameraAttachableAgent(LastFollowedAgent, val5, 1, currentCameraPosition);
				}
				bool flag5 = Game.Current.CheatMode && InputManager.IsControlDown();
				if (InputManager.IsGameKeyReleased(10) || InputManager.IsGameKeyReleased(11))
				{
					if (!flag5)
					{
						val = FindNextCameraAttachableAgent(LastFollowedAgent, val5, -1, currentCameraPosition);
					}
				}
				else if ((InputManager.IsGameKeyReleased(9) || InputManager.IsGameKeyReleased(12)) && !_rightButtonDraggingMode)
				{
					if (!flag5)
					{
						val = FindNextCameraAttachableAgent(LastFollowedAgent, val5, 1, currentCameraPosition);
					}
				}
				else if ((InputManager.IsGameKeyDown(0) || InputManager.IsGameKeyDown(1) || InputManager.IsGameKeyDown(2) || InputManager.IsGameKeyDown(3) || (InputManager.GetIsControllerConnected() && (Input.GetKeyState((InputKey)222).y != 0f || Input.GetKeyState((InputKey)222).x != 0f))) && (int)val5 == 0)
				{
					val = null;
					val2 = null;
				}
			}
		}
		return new SpectatorData(val, val2, val5);
	}

	private Agent FindNextCameraAttachableAgent(Agent currentAgent, SpectatorCameraTypes cameraLockMode, int iterationDirection, Vec3 currentCameraPosition)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected I4, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.AllAgents == null || ((List<Agent>)(object)Mission.AllAgents).Count == 0)
		{
			return null;
		}
		if (MBDebug.IsErrorReportModeActive())
		{
			return null;
		}
		MissionPeer missionPeer = (GameNetwork.IsMyPeerReady ? PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer) : null);
		List<Agent> list;
		if (_gatherCustomAgentListToSpectate != null)
		{
			list = _gatherCustomAgentListToSpectate(currentAgent);
		}
		else
		{
			switch (cameraLockMode - 2)
			{
			case 3:
			case 4:
				list = ((IEnumerable<Agent>)Mission.AllAgents).Where((Agent x) => (x.Team == Mission.PlayerTeam && x.MissionPeer != null && x.IsCameraAttachable()) || x == currentAgent).ToList();
				break;
			case 2:
				list = ((IEnumerable<Agent>)Mission.AllAgents).Where(delegate(Agent x)
				{
					if (x.Formation != null)
					{
						Formation formation = x.Formation;
						MissionPeer obj = missionPeer;
						if (formation == ((obj != null) ? obj.ControlledFormation : null) && x.IsCameraAttachable())
						{
							return true;
						}
					}
					return x == currentAgent;
				}).ToList();
				break;
			case 1:
				list = ((IEnumerable<Agent>)Mission.AllAgents).Where((Agent x) => (x.MissionPeer != null && x.IsCameraAttachable()) || x == currentAgent).ToList();
				break;
			case 0:
				list = ((IEnumerable<Agent>)Mission.AllAgents).Where((Agent x) => x.IsCameraAttachable() || x == currentAgent).ToList();
				break;
			default:
				list = ((IEnumerable<Agent>)Mission.AllAgents).Where((Agent x) => x.IsCameraAttachable() || x == currentAgent).ToList();
				break;
			}
		}
		if (list.Count - ((currentAgent != null && !currentAgent.IsCameraAttachable()) ? 1 : 0) == 0)
		{
			return null;
		}
		if (currentAgent == null)
		{
			Agent result = null;
			float num = float.MaxValue;
			foreach (Agent item in list)
			{
				Vec3 val = currentCameraPosition - item.Position;
				float lengthSquared = ((Vec3)(ref val)).LengthSquared;
				if (num > lengthSquared)
				{
					num = lengthSquared;
					result = item;
				}
			}
			return result;
		}
		int num2 = list.IndexOf(currentAgent);
		if (iterationDirection == 1)
		{
			return list[(num2 + 1) % list.Count];
		}
		return (num2 < 0) ? list[list.Count - 1] : list[(num2 + list.Count - 1) % list.Count];
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}

	void IGameStateListener.OnActivate()
	{
		if (_isDeactivated)
		{
			ActivateMissionView();
		}
		_isDeactivated = false;
	}

	void IGameStateListener.OnDeactivate()
	{
		_isDeactivated = true;
		Mission mission = Mission;
		if (((mission != null) ? mission.MissionBehaviors : null) != null)
		{
			_missionViewsContainer.ForEach(delegate(MissionView missionView)
			{
				missionView.OnMissionScreenDeactivate();
			});
		}
		((ScreenBase)this).OnDeactivate();
	}

	void IMissionSystemHandler.OnMissionAfterStarting(Mission mission)
	{
		Mission = mission;
		Mission.AddListener((IMissionListener)(object)this);
		foreach (MissionBehavior missionBehavior in Mission.MissionBehaviors)
		{
			if (missionBehavior is MissionView missionView)
			{
				RegisterView(missionView);
			}
		}
	}

	void IMissionSystemHandler.OnMissionLoadingFinished(Mission mission)
	{
		Mission = mission;
		InitializeMissionView();
		ActivateMissionView();
	}

	void IMissionSystemHandler.BeforeMissionTick(Mission mission, float realDt)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		if (MBEditor.EditModeEnabled)
		{
			if (((ScreenBase)this).DebugInput.IsHotKeyReleased("EnterEditMode") && mission == null)
			{
				if (MBEditor.IsEditModeOn)
				{
					MBEditor.LeaveEditMode();
					_tickEditor = false;
				}
				else
				{
					MBEditor.EnterEditMode(SceneView, CombatCamera.Frame, CameraElevation, CameraBearing);
					_tickEditor = true;
				}
			}
			if (_tickEditor && MBEditor.IsEditModeOn)
			{
				MBEditor.TickEditMode(realDt);
				return;
			}
		}
		if (mission == null || (NativeObject)(object)mission.Scene == (NativeObject)null)
		{
			return;
		}
		mission.Scene.SetOwnerThread();
		mission.Scene.SetDynamicShadowmapCascadesRadiusMultiplier(1f);
		if (MBEditor.EditModeEnabled)
		{
			MBCommon.CheckResourceModifications();
		}
		HandleUserInput(realDt);
		if (!_isRenderingStarted && MissionStartedRendering())
		{
			Mission.Current.OnRenderingStarted();
			_isRenderingStarted = true;
			Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)8));
		}
		if (_isRenderingStarted && _loadingScreenFramesLeft >= 0 && !_onSceneRenderingStartedCalled)
		{
			if (_loadingScreenFramesLeft > 0)
			{
				_loadingScreenFramesLeft--;
				Mission current = Mission.Current;
				Utilities.SetLoadingScreenPercentage((current == null || !current.HasMissionBehavior<DeploymentMissionController>()) ? (1f - (float)_loadingScreenFramesLeft * 0.02f) : ((_loadingScreenFramesLeft == 0) ? 1f : (0.92f - (float)_loadingScreenFramesLeft * 0.005f)));
			}
			bool flag = AreViewsReady();
			if (_loadingScreenFramesLeft <= 0 && flag && !MBAnimation.IsAnyAnimationLoadingFromDisk())
			{
				OnSceneRenderingStarted();
				_onSceneRenderingStartedCalled = true;
			}
		}
	}

	private bool AreViewsReady()
	{
		bool isReady = true;
		_missionViewsContainer.ForEach(delegate(MissionView missionView)
		{
			bool flag = missionView.IsReady();
			isReady &= flag;
		});
		return isReady;
	}

	private void CameraTick(Mission mission, float realDt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)mission.CurrentState == 2)
		{
			CheckForUpdateCamera(realDt);
		}
	}

	void IMissionSystemHandler.UpdateCamera(Mission mission, float realDt)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		CameraTick(mission, realDt);
		if ((int)mission.CurrentState == 2 && !mission.MissionEnded)
		{
			MBWindowManager.PreDisplay();
		}
	}

	protected virtual void AfterMissionTick(Mission mission, float realDt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		if (((int)mission.CurrentState == 2 || (mission.MissionEnded && (int)mission.CurrentState != 4)) && Game.Current.CheatMode && IsCheatGhostMode && Agent.Main != null && InputManager.IsHotKeyPressed("MissionScreenHotkeyTeleportMainAgent"))
		{
			TeleportMainAgentToCameraFocusForCheat();
		}
		if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(4) && !((ScreenBase)this).DebugInput.IsAltDown() && MBEditor.EditModeEnabled && MBEditor.IsEditModeOn)
		{
			MBEditor.LeaveEditMissionMode();
		}
		if ((NativeObject)(object)mission.Scene == (NativeObject)null)
		{
			MBDebug.Print("Mission is null on MissionScreen::OnFrameTick second phase", 0, (DebugColor)12, 17592186044416uL);
		}
	}

	void IMissionSystemHandler.AfterMissionTick(Mission mission, float realDt)
	{
		AfterMissionTick(mission, realDt);
	}

	IEnumerable<MissionBehavior> IMissionSystemHandler.OnAddBehaviors(IEnumerable<MissionBehavior> behaviors, Mission mission, string missionName, bool addDefaultMissionBehaviors)
	{
		if (addDefaultMissionBehaviors)
		{
			behaviors = AddDefaultMissionBehaviorsTo(mission, behaviors);
		}
		behaviors = ViewCreatorManager.CollectMissionBehaviors(missionName, mission, behaviors);
		return behaviors;
	}

	private void HandleInputs()
	{
		if (!MBEditor.IsEditorMissionOn() && MissionStartedRendering() && ((ScreenLayer)SceneLayer).Input.IsHotKeyReleased("ToggleEscapeMenu") && !LoadingWindow.IsLoadingWindowActive)
		{
			OnEscape();
		}
	}

	public void OnEscape()
	{
		if (!IsMissionTickable || ScreenFadeController.IsFadeActive)
		{
			return;
		}
		foreach (MissionBehavior item in (from v in Mission.MissionBehaviors
			where v is MissionView
			orderby ((MissionView)(object)v).ViewOrderPriority
			select v).ToList())
		{
			MissionView missionView = item as MissionView;
			if (!IsMissionTickable || missionView.OnEscape())
			{
				break;
			}
		}
	}

	bool IMissionSystemHandler.RenderIsReady()
	{
		return MissionStartedRendering();
	}

	void IMissionListener.OnEndMission()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		_agentToFollowOverride = null;
		LastFollowedAgent = null;
		LastFollowedAgentVisuals = null;
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
		_missionViewsContainer.ForEach(delegate(MissionView missionView)
		{
			missionView.OnMissionScreenFinalize();
			UnregisterView(missionView);
		});
		CraftedDataViewManager.Clear();
		Mission.RemoveListener((IMissionListener)(object)this);
	}

	void IMissionListener.OnEquipItemsFromSpawnEquipmentBegin(Agent agent, CreationType creationType)
	{
		agent.ClearEquipment();
		agent.AgentVisuals.ClearVisualComponents(false, false);
	}

	void IMissionListener.OnEquipItemsFromSpawnEquipment(Agent agent, CreationType creationType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected I4, but got Unknown
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Invalid comparison between Unknown and I4
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Invalid comparison between Unknown and I4
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Invalid comparison between Unknown and I4
		EquipmentElement val;
		switch (creationType - 1)
		{
		case 0:
		case 2:
		{
			bool flag = false;
			Random randomGenerator = null;
			bool randomizeColors = agent.RandomizeColors;
			uint color3;
			uint color4;
			if (randomizeColors)
			{
				int bodyPropertiesSeed = agent.BodyPropertiesSeed;
				randomGenerator = new Random(bodyPropertiesSeed);
				AgentVisuals.GetRandomClothingColors(bodyPropertiesSeed, Color.FromUint(agent.ClothingColor1), Color.FromUint(agent.ClothingColor2), out var color, out var color2);
				color3 = ((Color)(ref color)).ToUnsignedInteger();
				color4 = ((Color)(ref color2)).ToUnsignedInteger();
			}
			else
			{
				color3 = agent.ClothingColor1;
				color4 = agent.ClothingColor2;
			}
			for (EquipmentIndex val2 = (EquipmentIndex)5; (int)val2 < 10; val2 = (EquipmentIndex)(val2 + 1))
			{
				val = agent.SpawnEquipment[val2];
				if (((EquipmentElement)(ref val)).IsVisualEmpty)
				{
					continue;
				}
				ItemObject obj = agent.SpawnEquipment[val2].CosmeticItem;
				if (obj == null)
				{
					val = agent.SpawnEquipment[val2];
					obj = ((EquipmentElement)(ref val)).Item;
				}
				ItemObject val3 = obj;
				int num;
				if ((int)val2 == 6)
				{
					val = agent.SpawnEquipment[(EquipmentIndex)8];
					if (((EquipmentElement)(ref val)).Item != null)
					{
						val = agent.SpawnEquipment[(EquipmentIndex)8];
						num = ((!((EquipmentElement)(ref val)).Item.ArmorComponent.IsNoSlim) ? 1 : 0);
						goto IL_0107;
					}
				}
				num = 0;
				goto IL_0107;
				IL_0363:
				if ((int)val2 != 6 || string.IsNullOrEmpty(val3.ArmBandMeshName))
				{
					continue;
				}
				MetaMesh copy = MetaMesh.GetCopy(val3.ArmBandMeshName, true, true);
				if (!((NativeObject)(object)copy != (NativeObject)null))
				{
					continue;
				}
				if (randomizeColors)
				{
					copy.SetGlossMultiplier(AgentVisuals.GetRandomGlossFactor(randomGenerator));
				}
				if (val3.IsUsingTeamColor)
				{
					for (int i = 0; i < copy.MeshCount; i++)
					{
						Mesh meshAtIndex = copy.GetMeshAtIndex(i);
						if (!meshAtIndex.HasTag("no_team_color"))
						{
							meshAtIndex.Color = color3;
							meshAtIndex.Color2 = color4;
							Material val4 = meshAtIndex.GetMaterial().CreateCopy();
							val4.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
							meshAtIndex.SetMaterial(val4);
							flag = true;
						}
						((NativeObject)meshAtIndex).ManualInvalidate();
					}
				}
				agent.AgentVisuals.AddMultiMesh(copy, MBAgentVisuals.GetBodyMeshIndex(val2));
				((NativeObject)copy).ManualInvalidate();
				continue;
				IL_0107:
				bool useSlimVersion = (byte)num != 0;
				bool isFemale = agent.Age >= 14f && agent.IsFemale;
				MetaMesh multiMesh = agent.SpawnEquipment[val2].GetMultiMesh(isFemale, useSlimVersion, needBatchedVersion: true);
				if ((NativeObject)(object)multiMesh != (NativeObject)null)
				{
					if (randomizeColors)
					{
						multiMesh.SetGlossMultiplier(AgentVisuals.GetRandomGlossFactor(randomGenerator));
					}
					if (val3.IsUsingTableau)
					{
						object obj2;
						if (agent == null)
						{
							obj2 = null;
						}
						else
						{
							IAgentOriginBase origin = agent.Origin;
							obj2 = ((origin != null) ? origin.Banner : null);
						}
						if (obj2 != null)
						{
							for (int j = 0; j < multiMesh.MeshCount; j++)
							{
								Mesh currentMesh = multiMesh.GetMeshAtIndex(j);
								Mesh obj3 = currentMesh;
								if (obj3 != null && !obj3.HasTag("dont_use_tableau"))
								{
									Mesh obj4 = currentMesh;
									if (obj4 != null && obj4.HasTag("banner_replacement_mesh"))
									{
										((BannerVisual)(object)agent.Origin.Banner.BannerVisual).GetTableauTextureLarge(BannerDebugInfo.CreateManual(((object)this).GetType().Name), delegate(Texture t)
										{
											ApplyBannerTextureToMesh(currentMesh, t);
										});
										((NativeObject)currentMesh).ManualInvalidate();
										break;
									}
								}
								((NativeObject)currentMesh).ManualInvalidate();
							}
							goto IL_02bc;
						}
					}
					if (val3.IsUsingTeamColor)
					{
						for (int num2 = 0; num2 < multiMesh.MeshCount; num2++)
						{
							Mesh meshAtIndex2 = multiMesh.GetMeshAtIndex(num2);
							if (!meshAtIndex2.HasTag("no_team_color"))
							{
								meshAtIndex2.Color = color3;
								meshAtIndex2.Color2 = color4;
								Material val5 = meshAtIndex2.GetMaterial().CreateCopy();
								val5.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
								meshAtIndex2.SetMaterial(val5);
								flag = true;
							}
							((NativeObject)meshAtIndex2).ManualInvalidate();
						}
					}
					goto IL_02bc;
				}
				goto IL_0363;
				IL_02bc:
				if (val3.UsingFacegenScaling)
				{
					multiMesh.UseHeadBoneFaceGenScaling(agent.AgentVisuals.GetSkeleton(), agent.Monster.HeadLookDirectionBoneIndex, agent.AgentVisuals.GetFacegenScalingMatrix());
				}
				Skeleton skeleton = agent.AgentVisuals.GetSkeleton();
				int num3 = ((skeleton != null) ? skeleton.GetComponentCount((ComponentType)3) : (-1));
				agent.AgentVisuals.AddMultiMesh(multiMesh, MBAgentVisuals.GetBodyMeshIndex(val2));
				((NativeObject)multiMesh).ManualInvalidate();
				int num4 = ((skeleton != null) ? skeleton.GetComponentCount((ComponentType)3) : (-1));
				if ((NativeObject)(object)skeleton != (NativeObject)null && (int)val2 == 9 && num4 > num3)
				{
					GameEntityComponent componentAtIndex = skeleton.GetComponentAtIndex((ComponentType)3, num4 - 1);
					agent.SetCapeClothSimulator(componentAtIndex);
				}
				goto IL_0363;
			}
			val = agent.SpawnEquipment[(EquipmentIndex)6];
			ItemObject item2 = ((EquipmentElement)(ref val)).Item;
			if (item2 != null)
			{
				int lodAtlasIndex = item2.LodAtlasIndex;
				if (lodAtlasIndex != -1)
				{
					agent.AgentVisuals.SetLodAtlasShadingIndex(lodAtlasIndex, flag, agent.ClothingColor1, agent.ClothingColor2);
				}
			}
			break;
		}
		case 1:
		{
			MBAgentVisuals agentVisuals = agent.AgentVisuals;
			val = agent.SpawnEquipment[(EquipmentIndex)10];
			ItemObject item = ((EquipmentElement)(ref val)).Item;
			val = agent.SpawnEquipment[(EquipmentIndex)11];
			MountVisualCreator.AddMountMeshToAgentVisual(agentVisuals, item, ((EquipmentElement)(ref val)).Item, agent.HorseCreationKey, agent);
			break;
		}
		}
		ArmorMaterialTypes bodyArmorMaterialType = (ArmorMaterialTypes)0;
		val = agent.SpawnEquipment[(EquipmentIndex)6];
		ItemObject item3 = ((EquipmentElement)(ref val)).Item;
		if (item3 != null)
		{
			bodyArmorMaterialType = item3.ArmorComponent.MaterialType;
		}
		agent.SetBodyArmorMaterialType(bodyArmorMaterialType);
	}

	void IMissionListener.OnConversationCharacterChanged()
	{
		_cameraAddSpecialMovement = true;
	}

	void IMissionListener.OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Invalid comparison between Unknown and I4
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Invalid comparison between Unknown and I4
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Invalid comparison between Unknown and I4
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I4
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Invalid comparison between Unknown and I4
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Invalid comparison between Unknown and I4
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Invalid comparison between Unknown and I4
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Invalid comparison between Unknown and I4
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Invalid comparison between Unknown and I4
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Mission.Mode == 1 && (int)oldMissionMode != 1)
		{
			_cameraAddSpecialMovement = true;
			_cameraApplySpecialMovementsInstantly = atStart;
		}
		else if ((int)Mission.Mode == 2 && (int)oldMissionMode == 6 && (NativeObject)(object)CombatCamera != (NativeObject)null)
		{
			_cameraAddSpecialMovement = true;
			_cameraApplySpecialMovementsInstantly = atStart || _playerDeploymentCancelled;
			SpectatorData spectatingData = GetSpectatingData(CombatCamera.Position);
			Agent agentToFollow = ((SpectatorData)(ref spectatingData)).AgentToFollow;
			if (!atStart)
			{
				LastFollowedAgent = agentToFollow;
			}
			_cameraSpecialCurrentAddedElevation = CameraElevation;
			if (agentToFollow != null)
			{
				_cameraSpecialCurrentAddedBearing = MBMath.WrapAngle(CameraBearing - agentToFollow.LookDirectionAsAngle);
				_cameraSpecialCurrentPositionToAdd = CombatCamera.Position - agentToFollow.VisualPosition;
				CameraBearing = agentToFollow.LookDirectionAsAngle;
			}
			else
			{
				_cameraSpecialCurrentAddedBearing = 0f;
				_cameraSpecialCurrentPositionToAdd = Vec3.Zero;
				CameraBearing = 0f;
			}
			CameraElevation = 0f;
		}
		if ((((int)Mission.Mode == 1 || (int)Mission.Mode == 5) && (int)oldMissionMode != 1 && (int)oldMissionMode != 5) || (((int)oldMissionMode == 1 || (int)oldMissionMode == 5) && (int)Mission.Mode != 1 && (int)Mission.Mode != 5))
		{
			_cameraAddSpecialMovement = true;
			_cameraAddSpecialPositionalMovement = true;
			_cameraApplySpecialMovementsInstantly = atStart;
		}
		_cameraHeightLimit = 0f;
		if ((int)Mission.Mode == 6)
		{
			GameEntity val = (((int)Mission.PlayerTeam.Side != 1) ? (Mission.Scene.FindEntityWithTag("strategyCameraDefender") ?? Mission.Scene.FindEntityWithTag("strategyCameraAttacker")) : (Mission.Scene.FindEntityWithTag("strategyCameraAttacker") ?? Mission.Scene.FindEntityWithTag("strategyCameraDefender")));
			if (val != (GameEntity)null)
			{
				_cameraHeightLimit = val.GetGlobalFrame().origin.z;
			}
		}
		else
		{
			GameEntity val2 = Mission.Scene.FindEntityWithTag("camera_height_limiter");
			if (val2 != (GameEntity)null)
			{
				_cameraHeightLimit = val2.GetGlobalFrame().origin.z;
			}
		}
	}

	void IMissionListener.OnResetMission()
	{
		_agentToFollowOverride = null;
		LastFollowedAgent = null;
		LastFollowedAgentVisuals = null;
	}

	void IMissionListener.OnDeploymentPlanMade(Team team, bool isFirstPlan)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		if (!GameNetwork.IsMultiplayer && (int)Mission.Mode == 6 && isFirstPlan)
		{
			Team playerTeam = Mission.PlayerTeam;
			if (playerTeam == team)
			{
				DeploymentMissionController missionBehavior = Mission.GetMissionBehavior<DeploymentMissionController>();
				bool flag = missionBehavior != null && MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle();
				GameEntity val = (((int)playerTeam.Side != 1) ? (Mission.Scene.FindEntityWithTag("strategyCameraDefender") ?? Mission.Scene.FindEntityWithTag("strategyCameraAttacker")) : (Mission.Scene.FindEntityWithTag("strategyCameraAttacker") ?? Mission.Scene.FindEntityWithTag("strategyCameraDefender")));
				if (val == (GameEntity)null && flag)
				{
					MatrixFrame zoomFocusFrame = Mission.DeploymentPlan.GetZoomFocusFrame(playerTeam);
					MatrixFrame val2 = zoomFocusFrame;
					float fovHorizontal = CombatCamera.GetFovHorizontal();
					float num = Math.Max(Mission.DeploymentPlan.GetZoomOffset(playerTeam, fovHorizontal), 32f);
					((Mat3)(ref val2.rotation)).RotateAboutSide(-MathF.PI / 6f);
					val2.origin -= num * val2.rotation.f;
					bool flag2 = false;
					if (Mission.IsPositionInsideBoundaries(((Vec3)(ref val2.origin)).AsVec2))
					{
						flag2 = true;
					}
					else
					{
						ICollection<Vec2> value = ((IEnumerable<KeyValuePair<string, ICollection<Vec2>>>)Mission.Boundaries).Where((KeyValuePair<string, ICollection<Vec2>> boundary) => boundary.Key == "walk_area").First().Value;
						MBList<Vec2> val3 = Extensions.ToMBList<Vec2>((IEnumerable<Vec2>)value);
						((List<Vec2>)(object)val3).AddRange((IEnumerable<Vec2>)value);
						Vec2 asVec = ((Vec3)(ref val2.rotation.f)).AsVec2;
						Vec2 val4 = ((Vec2)(ref asVec)).Normalized();
						Vec2 asVec2 = ((Vec3)(ref val2.origin)).AsVec2;
						Vec2 val5 = default(Vec2);
						if (MBMath.IntersectRayWithPolygon(asVec2, val4, val3, ref val5))
						{
							Vec2 asVec3 = ((Vec3)(ref zoomFocusFrame.origin)).AsVec2;
							float num2 = ((Vec2)(ref val5)).Distance(asVec3);
							float val6 = ((Vec2)(ref asVec2)).Distance(asVec3);
							float num3 = num2 / Math.Max(val6, 0.1f) * val2.origin.z;
							Vec3 origin = default(Vec3);
							((Vec3)(ref origin))._002Ector(val5, num3, -1f);
							val2.origin = origin;
							flag2 = true;
						}
					}
					if (!flag2)
					{
						val2 = zoomFocusFrame;
						val2.origin.z += 20f;
					}
					CombatCamera.Frame = val2;
					CameraBearing = ((Vec3)(ref val2.rotation.f)).RotationZ;
					CameraElevation = ((Vec3)(ref val2.rotation.f)).RotationX;
				}
				_playerDeploymentCancelled = missionBehavior != null && !flag;
			}
		}
		_missionViewsContainer.ForEach(delegate(MissionView missionView)
		{
			missionView.OnDeploymentPlanMade(team, isFirstPlan);
		});
	}

	private void CalculateNewBearingAndElevationForFirstPerson(Agent agentToFollow, float oldCameraBearing, float oldCameraElevation, float cameraBearingDelta, float cameraElevationDelta, out float newBearing, out float newElevation)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		newBearing = MBMath.WrapAngle(oldCameraBearing + cameraBearingDelta);
		newElevation = MBMath.WrapAngle(oldCameraElevation + cameraElevationDelta);
		AnimFlags currentAnimationFlag = agentToFollow.GetCurrentAnimationFlag(0);
		AnimFlags currentAnimationFlag2 = agentToFollow.GetCurrentAnimationFlag(1);
		Vec2 bodyRotationConstraint = agentToFollow.GetBodyRotationConstraint(1);
		bool flag = ((Vec2)(ref bodyRotationConstraint)).IsNonZero();
		if (!(agentToFollow.HasMount && flag) && !Extensions.HasAnyFlag<AnimFlags>(currentAnimationFlag, (AnimFlags)285212672) && !Extensions.HasAnyFlag<AnimFlags>(currentAnimationFlag2, (AnimFlags)285212672) && (int)agentToFollow.MovementLockedState != 2)
		{
			return;
		}
		MatrixFrame boneEntitialFrame = agentToFollow.GetBoneEntitialFrame(agentToFollow.Monster.ThoraxLookDirectionBoneIndex, true);
		MatrixFrame frame = agentToFollow.AgentVisuals.GetFrame();
		float num = (flag ? 0f : ((Vec3)(ref boneEntitialFrame.rotation.f)).RotationZ);
		float num2 = num + ((Vec3)(ref frame.rotation.f)).RotationZ;
		if (flag)
		{
			bodyRotationConstraint.x = Math.Max(-3.1414928f, bodyRotationConstraint.x - 0.9f);
			bodyRotationConstraint.y = Math.Min(3.1414928f, bodyRotationConstraint.y + 0.9f);
		}
		else
		{
			bodyRotationConstraint.y = MBMath.ToRadians(50f);
			if (Math.Abs(num) > bodyRotationConstraint.y - 0.0001f)
			{
				float num3 = Math.Abs(num) - (bodyRotationConstraint.y - 0.0001f);
				bodyRotationConstraint.y += num3 * 0.5f;
				num2 += num3 * ((num < 0f) ? 0.25f : (-0.25f));
			}
			bodyRotationConstraint.x = 0f - bodyRotationConstraint.y;
		}
		if (num2 <= -MathF.PI)
		{
			num2 += MathF.PI * 2f;
		}
		else if (num2 > MathF.PI)
		{
			num2 -= MathF.PI * 2f;
		}
		float num4 = MBMath.WrapAngle(oldCameraBearing - num2);
		num4 += cameraBearingDelta;
		if (num4 > bodyRotationConstraint.y)
		{
			num4 = bodyRotationConstraint.y;
		}
		else if (num4 < bodyRotationConstraint.x)
		{
			num4 = bodyRotationConstraint.x;
		}
		newBearing = MBMath.WrapAngle(num4 + num2);
		float num5 = 0f - MBMath.ToRadians(25f);
		if (!flag && MBMath.GetSmallestDifferenceBetweenTwoAngles(((Vec3)(ref frame.rotation.f)).RotationX, newElevation) < num5)
		{
			newElevation = ((Vec3)(ref frame.rotation.f)).RotationX + num5;
		}
	}

	private static void ApplyBannerTextureToMesh(Mesh armorMesh, Texture bannerTexture)
	{
		if ((NativeObject)(object)armorMesh != (NativeObject)null)
		{
			Material val = armorMesh.GetMaterial().CreateCopy();
			val.SetTexture((MBTextureType)1, bannerTexture);
			uint num = (uint)val.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
			ulong shaderFlags = val.GetShaderFlags();
			val.SetShaderFlags(shaderFlags | num);
			armorMesh.SetMaterial(val);
		}
	}

	void IChatLogHandlerScreen.TryUpdateChatLogLayerParameters(ref bool isTeamChatAvailable, ref bool inputEnabled, ref bool isToggleChatHintAvailable, ref bool isMouseVisible, ref InputContext inputContext)
	{
		if (SceneLayer != null)
		{
			inputEnabled = true;
			inputContext = ((ScreenLayer)SceneLayer).Input;
		}
	}
}
