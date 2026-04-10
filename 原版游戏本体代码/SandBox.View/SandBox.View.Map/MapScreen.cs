using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Missions.BattleScore;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using SandBox.View.Menu;
using SandBox.ViewModelCollection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Incidents;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Library.EventSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.BattleScore;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.View.Scripts;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Map;

[GameStateScreen(typeof(MapState))]
public class MapScreen : ScreenBase, IMapStateHandler, IGameStateListener, IChatLogHandlerScreen
{
	public enum MapOverlayType
	{
		None,
		Army
	}

	public struct DecalEntity
	{
		public GameEntity GameEntity { get; set; }

		public Decal Decal { get; set; }

		public DecalEntity(GameEntity gameEntity, Decal decal)
		{
			GameEntity = gameEntity;
			Decal = decal;
		}

		public static DecalEntity Create(Scene scene, string material, string entityName = null)
		{
			GameEntity obj = GameEntity.CreateEmpty(scene, true, true, true);
			obj.Name = entityName ?? "Entity";
			Decal val = Decal.CreateDecal((string)null);
			Material fromResource = Material.GetFromResource(material);
			if ((NativeObject)(object)fromResource != (NativeObject)null)
			{
				val.SetMaterial(fromResource);
			}
			scene.AddDecalInstance(val, "editor_set", false);
			obj.AddComponent((GameEntityComponent)(object)val);
			return new DecalEntity(obj, val);
		}
	}

	private struct MouseInputState
	{
		public bool IsLeftMouseDown;

		public bool IsLeftMousePressed;

		public bool IsLeftMouseReleased;

		public bool IsMiddleMouseDown;

		public bool IsMiddleMousePressed;

		public bool IsMiddleMouseReleased;

		public bool IsRightMouseDown;

		public bool IsRightMousePressed;

		public bool IsRightMouseReleased;
	}

	public class MainMapCameraMoveEvent : EventBase
	{
		public bool RotationChanged { get; private set; }

		public bool PositionChanged { get; private set; }

		public MainMapCameraMoveEvent(bool rotationChanged, bool positionChanged)
		{
			RotationChanged = rotationChanged;
			PositionChanged = positionChanged;
		}
	}

	private const float DoubleClickTimeLimit = 0.3f;

	private INavigationHandler _navigationHandler;

	private const int _frameDelayAmountForRenderActivation = 5;

	private MenuViewContext _menuViewContext;

	private MenuContext _latestMenuContext;

	public readonly Dictionary<Tuple<Material, Banner>, Material> CharacterBannerMaterialCache = new Dictionary<Tuple<Material, Banner>, Material>();

	private bool _partyIconNeedsRefreshing;

	private uint _tooltipTargetHash;

	private object _tooltipTargetObject;

	private MapViewsContainer _mapViewsContainer;

	private MapView _encounterOverlay;

	public static bool DisableVisualTicks;

	private MapReadyView _mapReadyView;

	private MapView _armyOverlay;

	public IMapTracksCampaignBehavior MapTracksCampaignBehavior;

	private double _lastReleaseTime;

	private double _lastPressTime;

	private MapView _marriageOfferPopupView;

	private Vec3 _clickedPosition;

	private Vec2 _clickedPositionPixel;

	private double _secondLastPressTime;

	private bool _leftButtonDoubleClickOnSceneWidget;

	private bool _ignoreNextTimeToggle;

	private MapView _heirSelectionPopupView;

	private Ray _mouseRay;

	private float _timeToggleTimer = float.MaxValue;

	private float _waitForDoubleClickUntilTime;

	private MapView _campaignOptionsView;

	private MapView _mapCheatsView;

	private MapView _battleSimulationView;

	private MapView _escapeMenuView;

	private bool _leftButtonDraggingMode;

	private MapConversationView _conversationView;

	private MapEntityVisual _preVisualOfSelectedEntity;

	private Vec2 _oldMousePosition;

	private int _activatedFrameNo = Utilities.EngineFrameNo;

	private bool _exitOnSaveOver;

	private bool _isSceneViewEnabled;

	private bool _isReadyForRender;

	private bool _gpuMemoryCleared;

	private bool _focusLost;

	private bool _isKingdomDecisionsDirty;

	private float _cheatPressTimer;

	private DecalEntity _pointTargetWindDirectionDecal;

	private DecalEntity _pointTargetInnerDecal;

	private DecalEntity _pointTargetOuterDecal;

	private DecalEntity _partyHoverOutlineDecal;

	private DecalEntity _townCircleDecal;

	private DecalEntity _settlementHoverOutlineDecal;

	private float _targetCircleRotationStartTime;

	private float _soundCalculationTime;

	private const float SoundCalculationInterval = 0.2f;

	private Dictionary<Tuple<Material, Banner>, Material> _bannerTexturedMaterialCache;

	public const uint EnemyPartyDecalColor = 4292093218u;

	public const uint SameFactionPartyDecalColor = 4284183827u;

	public const uint NeutralPartyDecalColor = 4291596077u;

	public const uint AllyPartyDecalColor = 4279386828u;

	private bool _mapSceneCursorWanted = true;

	private bool _mapSceneCursorActive;

	private TutorialContexts _currentTutorialContext = (TutorialContexts)4;

	private MapColorGradeManager _colorGradeManager;

	private int _mapScreenTickCount;

	private int _sceneReadyFrameCounter;

	public bool TooltipHandlingDisabled;

	private readonly UIntPtr[] _intersectedEntityIDs = new UIntPtr[128];

	private readonly Intersection[] _intersectionInfos = (Intersection[])(object)new Intersection[128];

	private GameEntity[] _tickedMapEntities;

	private Mesh[] _tickedMapMeshes;

	private readonly List<MBCampaignEvent> _periodicCampaignUIEvents;

	private bool _ignoreLeftMouseRelease;

	public IInputContext Input => (IInputContext)(object)((ScreenLayer)SceneLayer).Input;

	public static MapScreen Instance { get; private set; }

	public bool IsReady => _isReadyForRender;

	public INavigationHandler NavigationHandler
	{
		get
		{
			return _navigationHandler;
		}
		set
		{
			if (_navigationHandler != null && value != null && value != _navigationHandler)
			{
				Debug.FailedAssert("Navigation handler should not be changed after map bar initialization", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\MapScreen.cs", "NavigationHandler", 127);
			}
			else
			{
				_navigationHandler = value;
			}
		}
	}

	public MapEntityVisual CurrentVisualOfTooltip { get; private set; }

	public CampaignMapSiegePrefabEntityCache PrefabEntityCache { get; private set; }

	public MapEncyclopediaView EncyclopediaScreenManager { get; private set; }

	public bool IsEscapeMenuOpened { get; private set; }

	public MapNotificationView MapNotificationView { get; private set; }

	public Dictionary<Tuple<Material, Banner>, Material> BannerTexturedMaterialCache => _bannerTexturedMaterialCache ?? (_bannerTexturedMaterialCache = new Dictionary<Tuple<Material, Banner>, Material>());

	public bool IsInMenu => _menuViewContext != null;

	public SceneLayer SceneLayer { get; private set; }

	public MapCameraView MapCameraView { get; private set; }

	public bool MapSceneCursorActive
	{
		get
		{
			return _mapSceneCursorActive;
		}
		set
		{
			if (_mapSceneCursorActive != value)
			{
				_mapSceneCursorActive = value;
			}
		}
	}

	public GameEntity ContourMaskEntity { get; private set; }

	public MapCursor MapCursor { get; private set; } = new MapCursor();

	public List<Mesh> InactiveLightMeshes { get; private set; }

	public List<Mesh> ActiveLightMeshes { get; private set; }

	public Scene MapScene { get; private set; }

	public MapState MapState { get; private set; }

	public bool IsInBattleSimulation { get; private set; }

	public bool IsInTownManagement { get; private set; }

	public bool IsInHideoutTroopManage { get; private set; }

	public bool IsInArmyManagement { get; private set; }

	public bool IsInRecruitment { get; private set; }

	public bool IsBarExtended { get; private set; }

	public bool IsInCampaignOptions { get; private set; }

	public bool IsMarriageOfferPopupActive { get; private set; }

	public bool IsMapCheatsActive { get; private set; }

	public bool IsMapIncidentActive { get; private set; }

	public bool IsHeirSelectionPopupActive { get; private set; }

	public bool IsOverlayContextMenuEnabled { get; private set; }

	public bool IsSoundOn { get; private set; } = true;

	public static Dictionary<UIntPtr, MapEntityVisual> VisualsOfEntities => SandBoxViewSubModule.VisualsOfEntities;

	internal static Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>> FrameAndVisualOfEngines => SandBoxViewSubModule.FrameAndVisualOfEngines;

	public MapScreen(MapState mapState)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		MapState = mapState;
		mapState.Handler = (IMapStateHandler)(object)this;
		_periodicCampaignUIEvents = new List<MBCampaignEvent>();
		InitializeVisuals();
		CampaignMusicHandler.Create();
		_mapViewsContainer = new MapViewsContainer();
		MapCameraView = (MapCameraView)AddMapView<MapCameraView>(Array.Empty<object>());
		AddMapView<MapBarView>(Array.Empty<object>());
		AddMapView<MapConversationView>(Array.Empty<object>());
		_conversationView = GetMapView<MapConversationView>();
		MapTracksCampaignBehavior = Campaign.Current.GetCampaignBehavior<IMapTracksCampaignBehavior>();
	}

	public void OnHoverMapEntity(MapEntityVisual mapEntityVisual)
	{
		uint hashCode = (uint)mapEntityVisual.GetHashCode();
		if (_tooltipTargetHash != hashCode)
		{
			_tooltipTargetHash = hashCode;
			_tooltipTargetObject = null;
			mapEntityVisual.OnHover();
		}
	}

	public void RemoveMapTooltip()
	{
		if (_tooltipTargetObject != null || _tooltipTargetHash != 0)
		{
			_tooltipTargetObject = null;
			_tooltipTargetHash = 0u;
			MBInformationManager.HideInformations();
			CurrentVisualOfTooltip?.OnHoverEnd();
		}
	}

	private static void PreloadTextures()
	{
		List<string> list = new List<string>();
		list.Add("gui_map_circle_enemy");
		list.Add("gui_map_circle_enemy_selected");
		list.Add("gui_map_circle_neutral");
		list.Add("gui_map_circle_neutral_selected");
		for (int i = 2; i <= 5; i++)
		{
			list.Add("gui_map_circle_enemy_selected_" + i);
			list.Add("gui_map_circle_neutral_selected_" + i);
		}
		for (int j = 0; j < list.Count; j++)
		{
			Texture.GetFromResource(list[j]).PreloadTexture(false);
		}
		list.Clear();
	}

	private void SetCameraOfSceneLayer()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		SceneLayer.SetCamera(MapCameraView.Camera);
		Vec3 origin = MapCameraView.CameraFrame.origin;
		origin.z = 0f;
		SceneLayer.SetFocusedShadowmap(false, ref origin, 0f);
	}

	protected override void OnResume()
	{
		((ScreenBase)this).OnResume();
		PreloadTextures();
		IsSoundOn = true;
		RestartAmbientSounds();
		if (_gpuMemoryCleared)
		{
			_gpuMemoryCleared = false;
		}
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnResume();
		});
		MenuContext menuContext = MapState.MenuContext;
		if (_menuViewContext != null)
		{
			if (menuContext != null && menuContext != _menuViewContext.MenuContext)
			{
				_menuViewContext.UpdateMenuContext(menuContext);
			}
			else if (menuContext == null)
			{
				ExitMenuContext();
			}
		}
		_menuViewContext?.OnResume();
		(Campaign.Current.MapSceneWrapper as MapScene).ValidateAgentVisualsReseted();
	}

	protected override void OnPause()
	{
		((ScreenBase)this).OnPause();
		MBInformationManager.HideInformations();
		PauseAmbientSounds();
		IsSoundOn = false;
		_activatedFrameNo = Utilities.EngineFrameNo;
		HandleIfSceneIsReady();
	}

	void IMapStateHandler.OnGameLoadFinished()
	{
		SandBoxViewVisualManager.OnGameLoadFinished();
	}

	protected override void OnActivate()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		((ScreenBase)this).OnActivate();
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnActivate();
		});
		MapCameraView.OnActivate(_leftButtonDraggingMode, _clickedPosition);
		_activatedFrameNo = Utilities.EngineFrameNo;
		HandleIfSceneIsReady();
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)4));
		SetCameraOfSceneLayer();
		RestartAmbientSounds();
		MenuContext menuContext = MapState.MenuContext;
		if (_menuViewContext != null)
		{
			if (menuContext != null && menuContext != _menuViewContext.MenuContext)
			{
				_menuViewContext.UpdateMenuContext(menuContext);
			}
			else if (menuContext == null)
			{
				ExitMenuContext();
			}
		}
		_menuViewContext?.OnResume();
		PartyBase.MainParty.SetVisualAsDirty();
		for (int num = ((List<ScreenLayer>)(object)((ScreenBase)this).Layers).Count - 1; num >= 0; num--)
		{
			if (((List<ScreenLayer>)(object)((ScreenBase)this).Layers)[num].IsActive && ((List<ScreenLayer>)(object)((ScreenBase)this).Layers)[num].IsFocusLayer)
			{
				ScreenManager.TrySetFocus(((List<ScreenLayer>)(object)((ScreenBase)this).Layers)[num]);
			}
		}
	}

	public void ClearGPUMemory()
	{
		if (true)
		{
			SceneLayer.ClearRuntimeGPUMemory(true);
			SceneLayer.SceneView.GetScene().DeleteWaterWakeRenderer();
		}
		SandBoxViewVisualManager.ClearVisualMemory();
		ThumbnailCacheManager.Current.ForceClearAllCache(true);
		Texture.ReleaseGpuMemories();
		_gpuMemoryCleared = true;
	}

	protected override void OnDeactivate()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		_sceneReadyFrameCounter = 0;
		Game current = Game.Current;
		if (current != null)
		{
			current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
		}
		PauseAmbientSounds();
		_menuViewContext?.OnDeactivate();
		MBInformationManager.HideInformations();
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnDeactivate();
		});
		((ScreenBase)this).OnDeactivate();
	}

	public override void OnFocusChangeOnGameWindow(bool focusGained)
	{
		((ScreenBase)this).OnFocusChangeOnGameWindow(focusGained);
		if (!focusGained && BannerlordConfig.StopGameOnFocusLost && !InformationManager.IsAnyInquiryActive())
		{
			MapEncyclopediaView encyclopediaScreenManager = EncyclopediaScreenManager;
			if ((encyclopediaScreenManager == null || !encyclopediaScreenManager.IsEncyclopediaOpen) && _mapViewsContainer.IsOpeningEscapeMenuOnFocusChangeAllowedForAll() && !ScreenFadeController.IsFadeActive)
			{
				OnEscapeMenuToggled(isOpened: true);
			}
		}
		_focusLost = !focusGained;
	}

	public MapView AddMapView<T>(params object[] parameters) where T : MapView, new()
	{
		T mapViewWithType = _mapViewsContainer.GetMapViewWithType<T>();
		if (mapViewWithType != null)
		{
			Debug.FailedAssert("Map view already added to the list", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\MapScreen.cs", "AddMapView", 549);
			Debug.Print("Map view already added to the list: " + typeof(T).Name + ". Returning existing view instead of creating new one.", 0, (DebugColor)12, 17592186044416uL);
			return mapViewWithType;
		}
		MapView mapView = SandBoxViewCreator.CreateMapView<T>(parameters);
		mapView.MapScreen = this;
		mapView.MapState = MapState;
		_mapViewsContainer.Add(mapView);
		mapView.CreateLayout();
		return mapView;
	}

	public T GetMapView<T>() where T : MapView
	{
		return _mapViewsContainer.GetMapViewWithType<T>();
	}

	public void RemoveMapView(MapView mapView)
	{
		mapView.OnDeactivate();
		mapView.OnFinalize();
		_mapViewsContainer.Remove(mapView);
	}

	public void AddEncounterOverlay(MenuOverlayType type)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (_encounterOverlay == null)
		{
			_encounterOverlay = AddMapView<MapOverlayView>(new object[1] { type });
			_mapViewsContainer.Foreach(delegate(MapView view)
			{
				view.OnOverlayCreated();
			});
		}
	}

	public void AddArmyOverlay(MapOverlayType type)
	{
		if (_armyOverlay == null)
		{
			_armyOverlay = AddMapView<MapOverlayView>(new object[1] { type });
			_mapViewsContainer.ForeachReverse(delegate(MapView view)
			{
				view.OnOverlayCreated();
			});
		}
	}

	public void RemoveEncounterOverlay()
	{
		if (_encounterOverlay != null)
		{
			RemoveMapView(_encounterOverlay);
			_encounterOverlay = null;
			_mapViewsContainer.ForeachReverse(delegate(MapView view)
			{
				view.OnOverlayClosed();
			});
		}
	}

	public void RemoveArmyOverlay()
	{
		if (_armyOverlay != null)
		{
			RemoveMapView(_armyOverlay);
			_armyOverlay = null;
			_mapViewsContainer.ForeachReverse(delegate(MapView view)
			{
				view.OnOverlayClosed();
			});
		}
	}

	protected override void OnInitialize()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		((ScreenBase)this).OnInitialize();
		if (MBDebug.TestModeEnabled)
		{
			CheckValidityOfItems();
		}
		Instance = this;
		ThumbnailCacheManager.Current.ForceClearAllCache(true);
		MapCameraView.Initialize();
		ViewSubModule.BannerTexturedMaterialCache = BannerTexturedMaterialCache;
		SceneLayer = new SceneLayer(true, false);
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("MapHotKeyCategory"));
		((ScreenBase)this).AddLayer((ScreenLayer)(object)SceneLayer);
		MapScene = ((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene;
		Utilities.SetAllocationAlwaysValidScene((Scene)null);
		SceneLayer.SetScene(MapScene);
		((View)SceneLayer.SceneView).SetEnable(false);
		SceneLayer.SetSceneUsesShadows(true);
		SceneLayer.SetRenderWithPostfx(true);
		SceneLayer.SetSceneUsesContour(true);
		SceneLayer.SceneView.SetAcceptGlobalDebugRenderObjects(true);
		SceneLayer.SceneView.SetResolutionScaling(true);
		CollectTickableMapMeshes();
		MapNotificationView = AddMapView<MapNotificationView>(Array.Empty<object>()) as MapNotificationView;
		AddMapView<MapBasicView>(Array.Empty<object>());
		AddMapView<MapPartyNameplateView>(Array.Empty<object>());
		AddMapView<MapSettlementNameplateView>(Array.Empty<object>());
		AddMapView<MapEventVisualsView>(Array.Empty<object>());
		AddMapView<MapTrackersView>(Array.Empty<object>());
		AddMapView<MapSaveView>(Array.Empty<object>());
		AddMapView<MapGamepadEffectsView>(Array.Empty<object>());
		EncyclopediaScreenManager = AddMapView<MapEncyclopediaView>(Array.Empty<object>()) as MapEncyclopediaView;
		_mapReadyView = AddMapView<MapReadyView>(Array.Empty<object>()) as MapReadyView;
		_mapReadyView.SetIsMapSceneReady(isReady: false);
		_mouseRay = new Ray(Vec3.Zero, Vec3.Up, float.MaxValue);
		if (PlayerSiege.PlayerSiegeEvent != null)
		{
			if (this != null)
			{
				((IMapStateHandler)this).OnPlayerSiegeActivated();
			}
		}
		PrefabEntityCache = SceneLayer.SceneView.GetScene().GetFirstEntityWithScriptComponent<CampaignMapSiegePrefabEntityCache>().GetFirstScriptOfType<CampaignMapSiegePrefabEntityCache>();
		CampaignEvents.OnSaveOverEvent.AddNonSerializedListener((object)this, (Action<bool, string>)OnSaveOver);
		CampaignEvents.OnMarriageOfferedToPlayerEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero>)OnMarriageOfferedToPlayer);
		CampaignEvents.OnMarriageOfferCanceledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero>)OnMarriageOfferCanceled);
		CampaignEvents.OnHeirSelectionRequestedEvent.AddNonSerializedListener((object)this, (Action<Dictionary<Hero, int>>)OnHeirSelectionRequested);
		CampaignEvents.OnHeirSelectionOverEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnHeirSelectionOver);
		Game.Current.EventManager.RegisterEvent<TutorialContextChangedEvent>((Action<TutorialContextChangedEvent>)OnTutorialContextChanged);
		GameEntity firstEntityWithScriptComponent = MapScene.GetFirstEntityWithScriptComponent<MapColorGradeManager>();
		if (firstEntityWithScriptComponent != (GameEntity)null)
		{
			_colorGradeManager = firstEntityWithScriptComponent.GetFirstScriptOfType<MapColorGradeManager>();
		}
	}

	private void OnSaveOver(bool isSuccessful, string newSaveGameName)
	{
		if (_exitOnSaveOver)
		{
			if (isSuccessful)
			{
				OnExit();
			}
			_exitOnSaveOver = false;
		}
	}

	private void OnMarriageOfferedToPlayer(Hero suitor, Hero maiden)
	{
		_marriageOfferPopupView = AddMapView<MarriageOfferPopupView>(new object[2] { suitor, maiden });
	}

	public void CloseMarriageOfferPopup()
	{
		if (_marriageOfferPopupView != null)
		{
			RemoveMapView(_marriageOfferPopupView);
			_marriageOfferPopupView = null;
		}
	}

	protected override void OnFinalize()
	{
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnFinalize();
		});
		List<EntityVisualManagerBase> components = SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents<EntityVisualManagerBase>();
		for (int num = components.Count - 1; num >= 0; num--)
		{
			SandBoxViewSubModule.SandBoxViewVisualManager.Finalize(components[num]);
		}
		((ScreenBase)this).OnFinalize();
		if ((NativeObject)(object)MapScene != (NativeObject)null)
		{
			MapScene.ClearAll();
		}
		Common.MemoryCleanupGC(false);
		CharacterBannerMaterialCache.Clear();
		ViewSubModule.BannerTexturedMaterialCache = null;
		MBMusicManager.Current.DeactivateCampaignMode();
		MBMusicManager.Current.OnCampaignMusicHandlerFinalize();
		((IMbEventBase)CampaignEvents.OnSaveOverEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnMarriageOfferedToPlayerEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnMarriageOfferCanceledEvent).ClearListeners((object)this);
		Game.Current.EventManager.UnregisterEvent<TutorialContextChangedEvent>((Action<TutorialContextChangedEvent>)OnTutorialContextChanged);
		BannerPersistentTextureCache current = BannerPersistentTextureCache.Current;
		if (current != null)
		{
			current.FlushCache();
		}
		MapScene = null;
		MapCameraView = null;
		Instance = null;
	}

	public void OnHourlyTick()
	{
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnHourlyTick();
		});
		Kingdom kingdom = Clan.PlayerClan.Kingdom;
		_isKingdomDecisionsDirty = ((kingdom != null) ? ((IEnumerable<KingdomDecision>)kingdom.UnresolvedDecisions).FirstOrDefault((Func<KingdomDecision, bool>)((KingdomDecision d) => d.NotifyPlayer && d.IsEnforced && d.IsPlayerParticipant && !d.ShouldBeCancelled())) : null) != null;
	}

	private void OnMarriageOfferCanceled(Hero suitor, Hero maiden)
	{
		CloseMarriageOfferPopup();
	}

	private void OnHeirSelectionRequested(Dictionary<Hero, int> heirApparents)
	{
		_heirSelectionPopupView = AddMapView<HeirSelectionPopupView>(new object[1] { heirApparents });
	}

	public void BeginParleyWith(PartyBase party)
	{
		if (GetMapView<MapParleyAnimationView>() == null)
		{
			AddMapView<MapParleyAnimationView>(new object[1] { party });
		}
	}

	private void OnHeirSelectionOver(Hero selectedHeir)
	{
		if (_heirSelectionPopupView != null)
		{
			RemoveMapView(_heirSelectionPopupView);
			_heirSelectionPopupView = null;
		}
	}

	private void ShowNextKingdomDecisionPopup()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		Kingdom kingdom = Clan.PlayerClan.Kingdom;
		KingdomDecision val = ((kingdom != null) ? ((IEnumerable<KingdomDecision>)kingdom.UnresolvedDecisions).FirstOrDefault((Func<KingdomDecision, bool>)((KingdomDecision d) => d.NotifyPlayer && d.IsEnforced && d.IsPlayerParticipant && !d.ShouldBeCancelled())) : null);
		if (val != null)
		{
			InquiryData val2 = new InquiryData(((object)new TextObject("{=A7349NHy}Critical Kingdom Decision", (Dictionary<string, object>)null)).ToString(), ((object)val.GetChooseTitle()).ToString(), true, false, ((object)new TextObject("{=bFzZwwjT}Examine", (Dictionary<string, object>)null)).ToString(), "", (Action)delegate
			{
				OpenKingdom();
			}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
			val.NotifyPlayer = false;
			InformationManager.ShowInquiry(val2, true, false);
			_isKingdomDecisionsDirty = false;
		}
		else
		{
			Debug.FailedAssert("There is no dirty decision but still demanded one", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\MapScreen.cs", "ShowNextKingdomDecisionPopup", 827);
		}
	}

	void IMapStateHandler.OnMenuModeTick(float dt)
	{
		UpdateTutorialContext();
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnMenuModeTick(dt);
		});
	}

	private void HandleIfBlockerStatesDisabled()
	{
		_ = _isReadyForRender;
		bool flag = SceneLayer.SceneView.ReadyToRender() && SceneLayer.SceneView.CheckSceneReadyToRender();
		bool flag2 = (_isSceneViewEnabled && flag) || _conversationView.IsConversationActive;
		if (flag2)
		{
			if (LoadingWindow.IsLoadingWindowActive)
			{
				if (_sceneReadyFrameCounter == 3)
				{
					LoadingWindow.DisableGlobalLoadingWindow();
					_sceneReadyFrameCounter = 0;
				}
				else
				{
					_sceneReadyFrameCounter++;
				}
			}
		}
		else if (!flag && !LoadingWindow.IsLoadingWindowActive)
		{
			LoadingWindow.EnableGlobalLoadingWindow();
		}
		if (flag)
		{
			_mapReadyView.SetIsMapSceneReady(flag2);
			_isReadyForRender = flag2;
		}
	}

	private void UpdateTutorialContext()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!((ScreenBase)this).IsActive)
		{
			return;
		}
		TutorialContexts val = (TutorialContexts)4;
		if (IsInMenu)
		{
			for (int num = _menuViewContext.MenuViews.Count - 1; num >= 0; num--)
			{
				TutorialContexts tutorialContext = _menuViewContext.MenuViews[num].GetTutorialContext();
				if ((int)tutorialContext != 4)
				{
					val = tutorialContext;
					break;
				}
			}
		}
		if ((int)val == 4)
		{
			val = _mapViewsContainer.GetContextToChangeTo();
		}
		if (_currentTutorialContext != val)
		{
			Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent(val));
		}
	}

	private void CheckCursorState()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Vec3 clippedMouseNear = Vec3.Zero;
		Vec3 clippedMouseFar = Vec3.Zero;
		SceneLayer.SceneView.TranslateMouse(ref clippedMouseNear, ref clippedMouseFar, -1f);
		PathFaceRecord currentFace = PathFaceRecord.NullFaceRecord;
		GetCursorIntersectionPoint(ref clippedMouseNear, ref clippedMouseFar, out var _, out var intersectionPoint, ref currentFace, out var isOnland, (BodyFlags)79617);
		NavigationType val = default(NavigationType);
		((ScreenLayer)SceneLayer).ActiveCursor = (CursorType)(NavigationHelper.CanPlayerNavigateToPosition(new CampaignVec2(((Vec3)(ref intersectionPoint)).AsVec2, isOnland), ref val) ? 1 : 10);
	}

	private void HandleIfSceneIsReady()
	{
		int num = Utilities.EngineFrameNo - _activatedFrameNo;
		bool isSceneViewEnabled = _isSceneViewEnabled;
		if (num < 5)
		{
			isSceneViewEnabled = false;
			MapColorGradeManager colorGradeManager = _colorGradeManager;
			if (colorGradeManager != null)
			{
				colorGradeManager.ApplyAtmosphere(true);
			}
		}
		else
		{
			bool isConversationActive = _conversationView.IsConversationActive;
			bool flag = (object)ScreenManager.TopScreen == this;
			isSceneViewEnabled = !isConversationActive && flag;
		}
		if (isSceneViewEnabled != _isSceneViewEnabled)
		{
			_isSceneViewEnabled = isSceneViewEnabled;
			((View)SceneLayer.SceneView).SetEnable(_isSceneViewEnabled);
			if (_isSceneViewEnabled)
			{
				MapScene.CheckResources(false);
				if (MapScene.SceneHadWaterWakeRenderer())
				{
					MapScene.EnsureWaterWakeRenderer();
					MapScene.SetWaterWakeWorldSize(128f, 0.994f);
					MapScene.SetWaterWakeCameraOffset(8f);
				}
				_sceneReadyFrameCounter = 0;
				if (_focusLost && !IsEscapeMenuOpened)
				{
					((ScreenBase)this).OnFocusChangeOnGameWindow(false);
				}
			}
		}
		HandleIfBlockerStatesDisabled();
	}

	void IMapStateHandler.StartCameraAnimation(CampaignVec2 targetPosition, float animationStopDuration)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MapCameraView.StartCameraAnimation(targetPosition, animationStopDuration);
	}

	private void OnTutorialContextChanged(TutorialContextChangedEvent evnt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_currentTutorialContext = evnt.NewContext;
	}

	void IMapStateHandler.BeforeTick(float dt)
	{
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ba: Invalid comparison between Unknown and I4
		//IL_0780: Unknown result type (might be due to invalid IL or missing references)
		//IL_0786: Invalid comparison between Unknown and I4
		//IL_06c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c7: Invalid comparison between Unknown and I4
		//IL_078d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0793: Invalid comparison between Unknown and I4
		//IL_06db: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e1: Invalid comparison between Unknown and I4
		//IL_079a: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a0: Invalid comparison between Unknown and I4
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ee: Invalid comparison between Unknown and I4
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c9: Invalid comparison between Unknown and I4
		//IL_07a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ad: Invalid comparison between Unknown and I4
		//IL_0702: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d6: Invalid comparison between Unknown and I4
		//IL_07cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_071b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Invalid comparison between Unknown and I4
		//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07df: Invalid comparison between Unknown and I4
		//IL_07f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f9: Invalid comparison between Unknown and I4
		//IL_0800: Unknown result type (might be due to invalid IL or missing references)
		//IL_0806: Invalid comparison between Unknown and I4
		UpdateTutorialContext();
		HandleIfSceneIsReady();
		bool flag = MobileParty.MainParty != null && PartyBase.MainParty.IsValid;
		if (flag && !MapCameraView.CameraAnimationInProgress)
		{
			if (!IsInMenu && ((ScreenLayer)SceneLayer).Input.IsHotKeyPressed("MapChangeCursorMode"))
			{
				_mapSceneCursorWanted = !_mapSceneCursorWanted;
			}
			if (IsMapClickPressed())
			{
				_secondLastPressTime = _lastPressTime;
				_lastPressTime = Time.ApplicationTime;
			}
			_leftButtonDoubleClickOnSceneWidget = false;
			if (IsMapClickReleased())
			{
				Vec2 mousePositionPixel = ((ScreenLayer)SceneLayer).Input.GetMousePositionPixel();
				float applicationTime = Time.ApplicationTime;
				_leftButtonDoubleClickOnSceneWidget = (double)applicationTime - _lastReleaseTime < 0.30000001192092896 && (double)applicationTime - _secondLastPressTime < 0.44999998807907104 && ((Vec2)(ref mousePositionPixel)).Distance(_oldMousePosition) < 10f;
				if (_leftButtonDoubleClickOnSceneWidget)
				{
					_waitForDoubleClickUntilTime = 0f;
				}
				_oldMousePosition = ((ScreenLayer)SceneLayer).Input.GetMousePositionPixel();
				_lastReleaseTime = applicationTime;
			}
			if (IsReady)
			{
				HandleMouse(dt);
			}
		}
		MapSceneCursorActive = !((ScreenLayer)SceneLayer).Input.GetIsMouseActive() && !IsInMenu && (object)ScreenManager.FocusedLayer == SceneLayer && _mapSceneCursorWanted;
		float deltaMouseScroll = ((ScreenLayer)SceneLayer).Input.GetDeltaMouseScroll();
		Vec3 zero = Vec3.Zero;
		Vec3 zero2 = Vec3.Zero;
		SceneLayer.SceneView.TranslateMouse(ref zero, ref zero2, -1f);
		float gameKeyAxis = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisX");
		float num = default(float);
		Vec3 projectedPosition = default(Vec3);
		bool rayCastForClosestEntityOrTerrainCondition = MapScene.RayCastForClosestEntityOrTerrain(zero, zero2, ref num, ref projectedPosition, 0.01f, (BodyFlags)544323529);
		float rX = 0f;
		float rY = 0f;
		float num2 = 1f;
		bool num3 = !Input.IsGamepadActive && !IsInMenu && (object)ScreenManager.FocusedLayer == SceneLayer;
		bool flag2 = Input.IsGamepadActive && MapSceneCursorActive;
		if (num3 || flag2)
		{
			if (((ScreenLayer)SceneLayer).Input.IsGameKeyDown(55))
			{
				num2 = MapCameraView.CameraFastMoveMultiplier;
			}
			rX = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("MapMovementAxisX") * num2;
			rY = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("MapMovementAxisY") * num2;
		}
		_ignoreLeftMouseRelease = false;
		MouseInputState mouseInputState = GetMouseInputState();
		if (mouseInputState.IsLeftMousePressed)
		{
			_clickedPositionPixel = ((ScreenLayer)SceneLayer).Input.GetMousePositionPixel();
			MapScene.RayCastForClosestEntityOrTerrain(((Ray)(ref _mouseRay)).Origin, ((Ray)(ref _mouseRay)).EndPoint, ref num, ref _clickedPosition, 0.01f, (BodyFlags)544323529);
			if (CurrentVisualOfTooltip != null)
			{
				RemoveMapTooltip();
			}
			_leftButtonDraggingMode = false;
		}
		else
		{
			if (mouseInputState.IsLeftMouseDown && !mouseInputState.IsLeftMouseReleased)
			{
				Vec2 mousePositionPixel2 = ((ScreenLayer)SceneLayer).Input.GetMousePositionPixel();
				if ((((Vec2)(ref mousePositionPixel2)).DistanceSquared(_clickedPositionPixel) > 300f || _leftButtonDraggingMode) && !IsInMenu)
				{
					_leftButtonDraggingMode = true;
					goto IL_0358;
				}
			}
			if (_leftButtonDraggingMode)
			{
				_leftButtonDraggingMode = false;
				_ignoreLeftMouseRelease = true;
			}
		}
		goto IL_0358;
		IL_0358:
		if (mouseInputState.IsMiddleMouseDown)
		{
			MBWindowManager.DontChangeCursorPos();
		}
		if (mouseInputState.IsLeftMouseReleased)
		{
			_clickedPositionPixel = ((ScreenLayer)SceneLayer).Input.GetMousePositionPixel();
		}
		MapCameraView.InputInformation inputInformation = default(MapCameraView.InputInformation);
		inputInformation.IsMainPartyValid = flag;
		inputInformation.IsMapReady = IsReady;
		inputInformation.IsControlDown = ((ScreenLayer)SceneLayer).Input.IsControlDown();
		inputInformation.IsMouseActive = ((ScreenLayer)SceneLayer).Input.GetIsMouseActive();
		inputInformation.CheatModeEnabled = Game.Current.CheatMode;
		inputInformation.DeltaMouseScroll = deltaMouseScroll;
		inputInformation.LeftMouseButtonPressed = mouseInputState.IsLeftMousePressed;
		inputInformation.LeftMouseButtonDown = mouseInputState.IsLeftMouseDown;
		inputInformation.LeftMouseButtonReleased = mouseInputState.IsLeftMouseReleased;
		inputInformation.MiddleMouseButtonDown = mouseInputState.IsMiddleMouseDown;
		inputInformation.RightMouseButtonDown = mouseInputState.IsRightMouseDown;
		inputInformation.RotateLeftKeyDown = ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(58);
		inputInformation.RotateRightKeyDown = ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(59);
		inputInformation.PartyMoveUpKey = ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(50);
		inputInformation.PartyMoveDownKey = ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(51);
		inputInformation.PartyMoveLeftKey = ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(52);
		inputInformation.PartyMoveRightKey = ((ScreenLayer)SceneLayer).Input.IsGameKeyDown(53);
		inputInformation.MapZoomIn = ((ScreenLayer)SceneLayer).Input.GetGameKeyState(56);
		inputInformation.MapZoomOut = ((ScreenLayer)SceneLayer).Input.GetGameKeyState(57);
		inputInformation.CameraFollowModeKeyPressed = ((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(64);
		inputInformation.MousePositionPixel = ((ScreenLayer)SceneLayer).Input.GetMousePositionPixel();
		inputInformation.ClickedPositionPixel = _clickedPositionPixel;
		inputInformation.ClickedPosition = _clickedPosition;
		inputInformation.LeftButtonDraggingMode = _leftButtonDraggingMode;
		inputInformation.IsInMenu = IsInMenu;
		inputInformation.WorldMouseNear = zero;
		inputInformation.WorldMouseFar = zero2;
		inputInformation.MouseSensitivity = ((ScreenLayer)SceneLayer).Input.GetMouseSensitivity();
		inputInformation.MouseMoveX = ((ScreenLayer)SceneLayer).Input.GetMouseMoveX();
		inputInformation.MouseMoveY = ((ScreenLayer)SceneLayer).Input.GetMouseMoveY();
		inputInformation.HorizontalCameraInput = gameKeyAxis;
		inputInformation.RayCastForClosestEntityOrTerrainCondition = rayCastForClosestEntityOrTerrainCondition;
		inputInformation.ProjectedPosition = projectedPosition;
		inputInformation.RX = rX;
		inputInformation.RY = rY;
		inputInformation.RS = num2;
		inputInformation.Dt = dt;
		MapCameraView.OnBeforeTick(in inputInformation);
		MapCursor.SetVisible(MapSceneCursorActive);
		if (flag && !Campaign.Current.TimeControlModeLock)
		{
			if (!MapState.AtMenu)
			{
				goto IL_0655;
			}
			if (Campaign.Current.CurrentMenuContext != null)
			{
				GameMenu gameMenu = Campaign.Current.CurrentMenuContext.GameMenu;
				if (gameMenu != null && gameMenu.IsWaitActive)
				{
					goto IL_0655;
				}
			}
		}
		goto IL_08f0;
		IL_08f0:
		if (!flag && CurrentVisualOfTooltip != null)
		{
			RemoveMapTooltip();
			CurrentVisualOfTooltip = null;
		}
		SetCameraOfSceneLayer();
		if (!((ScreenLayer)SceneLayer).Input.GetIsMouseActive() && Campaign.Current.GameStarted)
		{
			MapCursor.BeforeTick(dt);
		}
		return;
		IL_0655:
		float applicationTime2 = Time.ApplicationTime;
		if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(63) && _timeToggleTimer == float.MaxValue)
		{
			_timeToggleTimer = applicationTime2;
		}
		if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(63) && applicationTime2 - _timeToggleTimer > 0.4f)
		{
			if ((int)Campaign.Current.TimeControlMode == 3 || (int)Campaign.Current.TimeControlMode == 1)
			{
				Campaign.Current.SetTimeSpeed(2);
			}
			else if ((int)Campaign.Current.TimeControlMode == 4 || (int)Campaign.Current.TimeControlMode == 2)
			{
				Campaign.Current.SetTimeSpeed(1);
			}
			else if ((int)Campaign.Current.TimeControlMode == 0)
			{
				Campaign.Current.SetTimeSpeed(1);
			}
			else if ((int)Campaign.Current.TimeControlMode == 6)
			{
				Campaign.Current.SetTimeSpeed(2);
			}
			_timeToggleTimer = float.MaxValue;
			_ignoreNextTimeToggle = true;
		}
		else if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(63))
		{
			if (_ignoreNextTimeToggle)
			{
				_ignoreNextTimeToggle = false;
			}
			else
			{
				_waitForDoubleClickUntilTime = 0f;
				if ((int)Campaign.Current.TimeControlMode == 2 || (int)Campaign.Current.TimeControlMode == 1 || (((int)Campaign.Current.TimeControlMode == 4 || (int)Campaign.Current.TimeControlMode == 3) && !Campaign.Current.IsMainPartyWaiting))
				{
					Campaign.Current.SetTimeSpeed(0);
				}
				else if ((int)Campaign.Current.TimeControlMode == 0 || (int)Campaign.Current.TimeControlMode == 3)
				{
					Campaign.Current.SetTimeSpeed(1);
				}
				else if ((int)Campaign.Current.TimeControlMode == 6 || (int)Campaign.Current.TimeControlMode == 4)
				{
					Campaign.Current.SetTimeSpeed(2);
				}
			}
			_timeToggleTimer = float.MaxValue;
		}
		else if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(60))
		{
			_waitForDoubleClickUntilTime = 0f;
			Campaign.Current.SetTimeSpeed(0);
		}
		else if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(61))
		{
			_waitForDoubleClickUntilTime = 0f;
			Campaign.Current.SetTimeSpeed(1);
		}
		else if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(62))
		{
			_waitForDoubleClickUntilTime = 0f;
			Campaign.Current.SetTimeSpeed(2);
		}
		else if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(65))
		{
			if ((int)Campaign.Current.TimeControlMode == 2 || (int)Campaign.Current.TimeControlMode == 4)
			{
				Campaign.Current.SetTimeSpeed(0);
			}
			else
			{
				Campaign.Current.SetTimeSpeed(2);
			}
		}
		goto IL_08f0;
	}

	void IMapStateHandler.Tick(float dt)
	{
		if (!IsInMenu)
		{
			if (_isKingdomDecisionsDirty)
			{
				ShowNextKingdomDecisionPopup();
			}
			else
			{
				if (ViewModel.UIDebugMode && ((ScreenBase)this).DebugInput.IsHotKeyDown("UIExtendedDebugKey") && ((ScreenBase)this).DebugInput.IsHotKeyPressed("MapScreenHotkeyOpenEncyclopedia"))
				{
					OpenEncyclopedia();
				}
				bool cheatMode = Game.Current.CheatMode;
				if (cheatMode && ((ScreenBase)this).DebugInput.IsHotKeyPressed("MapScreenHotkeySwitchCampaignTrueSight"))
				{
					Campaign.Current.TrueSight = !Campaign.Current.TrueSight;
				}
				if (cheatMode)
				{
					((ScreenBase)this).DebugInput.IsHotKeyPressed("MapScreenPrintMultiLineText");
				}
				_mapViewsContainer.ForeachReverse(delegate(MapView view)
				{
					view.OnFrameTick(dt);
				});
			}
		}
		SandBoxViewVisualManager.OnTick(dt, Campaign.Current.CampaignDt);
	}

	void IMapStateHandler.OnIdleTick(float dt)
	{
		UpdateTutorialContext();
		HandleIfSceneIsReady();
		RemoveMapTooltip();
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnIdleTick(dt);
		});
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		MBDebug.SetErrorReportScene(MapScene);
		UpdateMenuView();
		TextObject val = default(TextObject);
		if (IsInMenu)
		{
			_menuViewContext.OnFrameTick(dt);
			if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(4))
			{
				GameMenuOption leaveMenuOption = Campaign.Current.GameMenuManager.GetLeaveMenuOption(_menuViewContext.MenuContext);
				if (leaveMenuOption != null)
				{
					UISoundsHelper.PlayUISound("event:/ui/default");
					if (_menuViewContext.MenuContext.GameMenu.IsWaitMenu)
					{
						_menuViewContext.MenuContext.GameMenu.EndWait();
					}
					leaveMenuOption.RunConsequence(_menuViewContext.MenuContext);
				}
			}
		}
		else if (Campaign.Current != null && !IsInBattleSimulation && !IsInArmyManagement && !IsMarriageOfferPopupActive && !IsHeirSelectionPopupActive && !IsMapCheatsActive && !IsMapIncidentActive && !IsOverlayContextMenuEnabled && !EncyclopediaScreenManager.IsEncyclopediaOpen && CampaignUIHelper.GetMapScreenActionIsEnabledWithReason(ref val))
		{
			Kingdom kingdom = Clan.PlayerClan.Kingdom;
			if (((kingdom == null) ? null : ((IEnumerable<KingdomDecision>)kingdom.UnresolvedDecisions)?.FirstOrDefault((Func<KingdomDecision, bool>)((KingdomDecision d) => d.NeedsPlayerResolution && !d.ShouldBeCancelled()))) != null)
			{
				OpenKingdom();
			}
		}
		if (_partyIconNeedsRefreshing)
		{
			_partyIconNeedsRefreshing = false;
			PartyBase.MainParty.SetVisualAsDirty();
		}
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnMapScreenUpdate(dt);
		});
		SandBoxViewVisualManager.OnFrameTick(Campaign.Current.CampaignDt);
	}

	protected override void OnPostFrameTick(float dt)
	{
		((ScreenBase)this).OnPostFrameTick(dt);
		if (Campaign.Current.CurrentTickCount != _mapScreenTickCount)
		{
			ITask campaignLateAITickTask = Campaign.Current.CampaignLateAITickTask;
			if (campaignLateAITickTask != null)
			{
				campaignLateAITickTask.Invoke();
			}
			_mapScreenTickCount = Campaign.Current.CurrentTickCount;
		}
	}

	private void UpdateMenuView()
	{
		if (_latestMenuContext == null && IsInMenu)
		{
			ExitMenuContext();
		}
		else if ((!IsInMenu && _latestMenuContext != null) || (IsInMenu && _menuViewContext.MenuContext != _latestMenuContext))
		{
			EnterMenuContext(_latestMenuContext);
		}
	}

	private void EnterMenuContext(MenuContext menuContext)
	{
		if (!Hero.MainHero.IsPrisoner)
		{
			MapCameraView.SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
			Campaign.Current.CameraFollowParty = PartyBase.MainParty;
		}
		if (!IsInMenu)
		{
			_menuViewContext = new MenuViewContext((ScreenBase)(object)this, menuContext);
		}
		else
		{
			_menuViewContext.UpdateMenuContext(menuContext);
		}
		_menuViewContext.OnInitialize();
		_menuViewContext.OnActivate();
		if (_conversationView.IsConversationActive)
		{
			_menuViewContext.OnMapConversationActivated();
		}
	}

	private void ExitMenuContext()
	{
		_menuViewContext.OnGameStateDeactivate();
		_menuViewContext.OnDeactivate();
		_menuViewContext.OnFinalize();
		_menuViewContext = null;
	}

	private void OpenBannerEditorScreen()
	{
		if (Campaign.Current.IsBannerEditorEnabled)
		{
			_partyIconNeedsRefreshing = true;
			Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<BannerEditorState>(), 0);
		}
	}

	private void OpenFaceGeneratorScreen()
	{
		if (Campaign.Current.IsFaceGenEnabled)
		{
			IFaceGeneratorCustomFilter faceGeneratorFilter = CharacterHelper.GetFaceGeneratorFilter();
			BarberState val = Game.Current.GameStateManager.CreateState<BarberState>(new object[2]
			{
				Hero.MainHero.CharacterObject,
				faceGeneratorFilter
			});
			GameStateManager.Current.PushState((GameState)(object)val, 0);
		}
	}

	public void OnExit()
	{
		MapCameraView.OnExit();
		MBGameManager.EndGame();
	}

	private void OnEscapeMenuToggled(bool isOpened = false)
	{
		MapCameraView.OnEscapeMenuToggled(isOpened);
		if (IsEscapeMenuOpened != isOpened)
		{
			IsEscapeMenuOpened = isOpened;
			if (isOpened)
			{
				List<EscapeMenuItemVM> escapeMenuItems = GetEscapeMenuItems();
				Game.Current.GameStateManager.RegisterActiveStateDisableRequest((object)this);
				_escapeMenuView = AddMapView<MapEscapeMenuView>(new object[1] { escapeMenuItems });
			}
			else
			{
				RemoveMapView(_escapeMenuView);
				_escapeMenuView = null;
				Game.Current.GameStateManager.UnregisterActiveStateDisableRequest((object)this);
			}
		}
	}

	private void CheckValidityOfItems()
	{
		foreach (ItemObject item in (List<ItemObject>)(object)MBObjectManager.Instance.GetObjectTypeList<ItemObject>())
		{
			if (!item.IsUsingTeamColor)
			{
				continue;
			}
			MetaMesh copy = MetaMesh.GetCopy(item.MultiMeshName, false, false);
			for (int i = 0; i < copy.MeshCount; i++)
			{
				Material material = copy.GetMeshAtIndex(i).GetMaterial();
				if (material.Name != "vertex_color_lighting_skinned" && material.Name != "vertex_color_lighting" && (NativeObject)(object)material.GetTexture((MBTextureType)1) == (NativeObject)null)
				{
					MBDebug.ShowWarning(string.Concat("Item object(", item.Name, ") has 'Using Team Color' flag but does not have a mask texture in diffuse2 slot. "));
					break;
				}
			}
		}
	}

	public void GetCursorIntersectionPoint(ref Vec3 clippedMouseNear, ref Vec3 clippedMouseFar, out float closestDistanceSquared, out Vec3 intersectionPoint, ref PathFaceRecord currentFace, out bool isOnland, BodyFlags excludedBodyFlags = (BodyFlags)79617u)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = clippedMouseFar - clippedMouseNear;
		((Vec3)(ref val)).Normalize();
		Vec3 val2 = clippedMouseFar - clippedMouseNear;
		float num = ((Vec3)(ref val2)).Normalize();
		((Ray)(ref _mouseRay)).Reset(clippedMouseNear, val2, num);
		intersectionPoint = Vec3.Zero;
		closestDistanceSquared = 1E+12f;
		float num2 = default(float);
		Vec3 val3 = default(Vec3);
		if (SceneLayer.SceneView.RayCastForClosestEntityOrTerrain(clippedMouseNear, clippedMouseFar, ref num2, ref val3, 0.01f, excludedBodyFlags))
		{
			closestDistanceSquared = num2 * num2;
			intersectionPoint = clippedMouseNear + val2 * num2;
		}
		CampaignVec2 val4 = new CampaignVec2(((Vec3)(ref intersectionPoint)).AsVec2, true);
		currentFace = ((CampaignVec2)(ref val4)).Face;
		isOnland = true;
		if (!((PathFaceRecord)(ref currentFace)).IsValid())
		{
			val4 = new CampaignVec2(((Vec3)(ref intersectionPoint)).AsVec2, false);
			currentFace = ((CampaignVec2)(ref val4)).Face;
			isOnland = false;
		}
	}

	public void FastMoveCameraToPosition(CampaignVec2 target)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MapCameraView.FastMoveCameraToPosition(target, IsInMenu);
	}

	private void HandleMouse(float dt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Invalid comparison between Unknown and I4
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		if (!Campaign.Current.GameStarted)
		{
			return;
		}
		Vec3 zero = Vec3.Zero;
		Vec3 zero2 = Vec3.Zero;
		SceneLayer.SceneView.TranslateMouse(ref zero, ref zero2, -1f);
		Vec3 clippedMouseNear = zero;
		Vec3 clippedMouseFar = zero2;
		PathFaceRecord currentFace = PathFaceRecord.NullFaceRecord;
		GetCursorIntersectionPoint(ref clippedMouseNear, ref clippedMouseFar, out var closestDistanceSquared, out var _, ref currentFace, out var isOnland, (BodyFlags)79617);
		GetCursorIntersectionPoint(ref clippedMouseNear, ref clippedMouseFar, out closestDistanceSquared, out var intersectionPoint2, ref currentFace, out var _, (BodyFlags)79633);
		int num = MapScene.SelectEntitiesCollidedWith(ref _mouseRay, _intersectionInfos, _intersectedEntityIDs);
		MapEntityVisual hoveredVisual = null;
		MapEntityVisual selectedVisual = null;
		MBList<CampaignEntityVisualComponent> components = SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents();
		for (int i = 0; i < ((List<CampaignEntityVisualComponent>)(object)components).Count && !((List<CampaignEntityVisualComponent>)(object)components)[i].OnVisualIntersected(_mouseRay, _intersectedEntityIDs, _intersectionInfos, num, zero, zero2, intersectionPoint2, ref hoveredVisual, ref selectedVisual); i++)
		{
		}
		Array.Clear(_intersectedEntityIDs, 0, num);
		Array.Clear(_intersectionInfos, 0, num);
		if (hoveredVisual != null && !hoveredVisual.IsMobileEntity)
		{
			((ScreenLayer)SceneLayer).ActiveCursor = (CursorType)1;
		}
		else
		{
			CheckCursorState();
		}
		float gameKeyAxis = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisY");
		bool flag = ((ScreenLayer)SceneLayer).IsHitThisFrame && ((ScreenLayer)SceneLayer).Input.IsKeyDown((InputKey)225);
		MapCameraView.HandleMouse(flag, gameKeyAxis, ((ScreenLayer)SceneLayer).Input.GetMouseMoveY(), dt);
		if (flag)
		{
			MBWindowManager.DontChangeCursorPos();
		}
		if ((object)ScreenManager.FirstHitLayer == SceneLayer && IsMapClickReleased() && !_leftButtonDraggingMode && !_ignoreLeftMouseRelease)
		{
			CampaignVec2 intersectionPoint3 = default(CampaignVec2);
			((CampaignVec2)(ref intersectionPoint3))._002Ector(((Vec3)(ref intersectionPoint2)).AsVec2, isOnland);
			HandleLeftMouseButtonClick(_leftButtonDoubleClickOnSceneWidget ? _preVisualOfSelectedEntity : selectedVisual, intersectionPoint3, currentFace, _leftButtonDoubleClickOnSceneWidget);
			_preVisualOfSelectedEntity = selectedVisual;
		}
		if (BannerlordConfig.MapDoubleClickBehavior == 0 && (int)Campaign.Current.TimeControlMode == 4 && _waitForDoubleClickUntilTime > 0f && _waitForDoubleClickUntilTime < Time.ApplicationTime)
		{
			Campaign.Current.TimeControlMode = (CampaignTimeControlMode)3;
			_waitForDoubleClickUntilTime = 0f;
		}
		if ((object)ScreenManager.FirstHitLayer == SceneLayer)
		{
			if (hoveredVisual != null)
			{
				if (CurrentVisualOfTooltip != hoveredVisual)
				{
					RemoveMapTooltip();
				}
				if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(67))
				{
					hoveredVisual.OnOpenEncyclopedia();
					MapCursor.SetVisible(value: false);
				}
				if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(66))
				{
					hoveredVisual.OnTrackAction();
				}
				OnHoverMapEntity(hoveredVisual);
				CurrentVisualOfTooltip = hoveredVisual;
			}
			else if (!TooltipHandlingDisabled)
			{
				RemoveMapTooltip();
				CurrentVisualOfTooltip = null;
			}
		}
		else
		{
			RemoveMapTooltip();
			CurrentVisualOfTooltip = null;
		}
	}

	private MouseInputState GetMouseInputState()
	{
		if (!((ScreenLayer)SceneLayer).IsHitThisFrame)
		{
			return default(MouseInputState);
		}
		return new MouseInputState
		{
			IsLeftMousePressed = ((ScreenLayer)SceneLayer).Input.IsKeyPressed((InputKey)224),
			IsLeftMouseDown = ((ScreenLayer)SceneLayer).Input.IsKeyDown((InputKey)224),
			IsLeftMouseReleased = ((ScreenLayer)SceneLayer).Input.IsKeyReleased((InputKey)224),
			IsMiddleMousePressed = ((ScreenLayer)SceneLayer).Input.IsKeyPressed((InputKey)226),
			IsMiddleMouseDown = ((ScreenLayer)SceneLayer).Input.IsKeyDown((InputKey)226),
			IsMiddleMouseReleased = ((ScreenLayer)SceneLayer).Input.IsKeyReleased((InputKey)226),
			IsRightMousePressed = ((ScreenLayer)SceneLayer).Input.IsKeyPressed((InputKey)225),
			IsRightMouseDown = ((ScreenLayer)SceneLayer).Input.IsKeyDown((InputKey)225),
			IsRightMouseReleased = ((ScreenLayer)SceneLayer).Input.IsKeyReleased((InputKey)225)
		};
	}

	private bool IsMapClickPressed()
	{
		if (!((ScreenLayer)SceneLayer).Input.IsHotKeyPressed("MapClick"))
		{
			if (((ScreenLayer)SceneLayer).Input.IsHotKeyPressed("MapTouchpadClick"))
			{
				return NativeOptions.GetConfig((NativeOptionsType)17) != 0f;
			}
			return false;
		}
		return true;
	}

	private bool IsMapClickReleased()
	{
		if (!((ScreenLayer)SceneLayer).Input.IsHotKeyReleased("MapClick"))
		{
			if (((ScreenLayer)SceneLayer).Input.IsHotKeyReleased("MapTouchpadClick"))
			{
				return NativeOptions.GetConfig((NativeOptionsType)17) != 0f;
			}
			return false;
		}
		return true;
	}

	private void HandleLeftMouseButtonClick(MapEntityVisual visualOfSelectedEntity, CampaignVec2 intersectionPoint, PathFaceRecord mouseOverFaceIndex, bool isDoubleClick)
	{
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = Input.IsControlDown() && Game.Current.CheatMode;
		if (!MapState.AtMenu)
		{
			if (visualOfSelectedEntity != null)
			{
				if (visualOfSelectedEntity.IsMainEntity)
				{
					MobileParty.MainParty.SetMoveModeHold();
				}
				else
				{
					CampaignVec2 interactionPositionForPlayer = visualOfSelectedEntity.InteractionPositionForPlayer;
					PathFaceRecord face = ((CampaignVec2)(ref interactionPositionForPlayer)).Face;
					flag2 = MapScene.DoesPathExistBetweenFaces(face.FaceIndex, MobileParty.MainParty.CurrentNavigationFace.FaceIndex, false);
					if (flag2 && MapCameraView.ProcessCameraInput && PartyBase.MainParty.MapEvent == null)
					{
						flag = visualOfSelectedEntity.OnMapClick(((ScreenLayer)SceneLayer).Input.IsHotKeyDown("MapFollowModifier"));
						if (flag)
						{
							HandleClickTimeChange(isDoubleClick);
							if (Input.IsGamepadActive)
							{
								if (visualOfSelectedEntity.IsMobileEntity)
								{
									if (visualOfSelectedEntity.IsInSameFaction(PartyBase.MainParty.MapFaction))
									{
										UISoundsHelper.PlayUISound("event:/ui/campaign/click_party");
									}
									else
									{
										UISoundsHelper.PlayUISound("event:/ui/campaign/click_party_enemy");
									}
								}
								else if (visualOfSelectedEntity.IsInSameFaction(PartyBase.MainParty.MapFaction))
								{
									UISoundsHelper.PlayUISound("event:/ui/campaign/click_settlement");
								}
								else
								{
									UISoundsHelper.PlayUISound("event:/ui/campaign/click_settlement_enemy");
								}
							}
						}
						MobileParty.MainParty.ForceAiNoPathMode = false;
					}
				}
			}
			else if (((PathFaceRecord)(ref mouseOverFaceIndex)).IsValid() || flag4)
			{
				if (!MobileParty.MainParty.IsInRaftState)
				{
					if (flag4)
					{
						MobileParty.MainParty.Position = intersectionPoint;
						MobileParty.MainParty.SetMoveModeHold();
						if (NavigationHelper.IsPositionValidForNavigationType(new CampaignVec2(((CampaignVec2)(ref intersectionPoint)).ToVec2(), true), (NavigationType)(MobileParty.MainParty.IsCurrentlyAtSea ? 1 : 2)) || NavigationHelper.IsPositionValidForNavigationType(new CampaignVec2(((CampaignVec2)(ref intersectionPoint)).ToVec2(), false), (NavigationType)(MobileParty.MainParty.IsCurrentlyAtSea ? 1 : 2)))
						{
							MobileParty.MainParty.ChangeIsCurrentlyAtSeaCheat();
						}
						if (MobileParty.MainParty.Army != null)
						{
							foreach (MobileParty item in (List<MobileParty>)(object)MobileParty.MainParty.Army.LeaderParty.AttachedParties)
							{
								item.Position = intersectionPoint;
							}
						}
						foreach (MobileParty item2 in (List<MobileParty>)(object)MobileParty.All)
						{
							item2.Party.UpdateVisibilityAndInspected(MobileParty.MainParty.Position, 0f);
						}
						foreach (Settlement item3 in (List<Settlement>)(object)Settlement.All)
						{
							item3.Party.UpdateVisibilityAndInspected(MobileParty.MainParty.Position, 0f);
						}
						MBDebug.Print("main party cheat move! - " + ((CampaignVec2)(ref intersectionPoint)).X + " " + ((CampaignVec2)(ref intersectionPoint)).Y, 0, (DebugColor)12, 17592186044416uL);
						flag2 = true;
						flag3 = true;
					}
					else
					{
						NavigationType val = default(NavigationType);
						flag2 = NavigationHelper.CanPlayerNavigateToPosition(intersectionPoint, ref val);
					}
				}
				if (flag2 && MapCameraView.ProcessCameraInput && MobileParty.MainParty.MapEvent == null)
				{
					if (!flag3)
					{
						MapState.ProcessTravel(intersectionPoint);
					}
					HandleClickTimeChange(isDoubleClick);
				}
				OnTerrainClick();
			}
		}
		Vec3 intersectionPoint2 = ((CampaignVec2)(ref intersectionPoint)).AsVec3();
		if (!SandBoxViewVisualManager.OnMouseClick(visualOfSelectedEntity, intersectionPoint2, mouseOverFaceIndex, isDoubleClick) && !flag)
		{
			OnTerrainClick();
		}
		if (flag2)
		{
			MapCameraView.HandleLeftMouseButtonClick(((ScreenLayer)SceneLayer).Input.GetIsMouseActive());
		}
	}

	private void OnTerrainClick()
	{
		_mapViewsContainer.Foreach(delegate(MapView view)
		{
			view.OnMapTerrainClick();
		});
		MapCursor.OnMapTerrainClick();
	}

	public void OnSiegeEngineFrameClick(MatrixFrame siegeFrame)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_mapViewsContainer.Foreach(delegate(MapView view)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			view.OnSiegeEngineClick(siegeFrame);
		});
	}

	private void HandleClickTimeChange(bool isDoubleClick)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Invalid comparison between Unknown and I4
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Invalid comparison between Unknown and I4
		switch (BannerlordConfig.MapDoubleClickBehavior)
		{
		case 0:
			if (!isDoubleClick && (int)Campaign.Current.TimeControlMode == 4)
			{
				_waitForDoubleClickUntilTime = Time.ApplicationTime + 0.3f;
				Campaign.Current.TimeControlMode = (CampaignTimeControlMode)4;
			}
			else
			{
				Campaign.Current.TimeControlMode = (CampaignTimeControlMode)(isDoubleClick ? 4 : 3);
			}
			break;
		case 1:
			if (isDoubleClick)
			{
				Campaign.Current.TimeControlMode = (CampaignTimeControlMode)(((int)Campaign.Current.TimeControlMode == 4) ? 3 : 4);
			}
			else
			{
				Campaign.Current.TimeControlMode = (CampaignTimeControlMode)(((int)Campaign.Current.TimeControlMode == 4) ? 4 : 3);
			}
			break;
		case 2:
			Campaign.Current.TimeControlMode = (CampaignTimeControlMode)(((int)Campaign.Current.TimeControlMode == 4) ? 4 : 3);
			break;
		}
	}

	void IMapStateHandler.AfterTick(float dt)
	{
		if ((object)ScreenManager.TopScreen == this)
		{
			TickVisuals(dt);
			SceneLayer sceneLayer = SceneLayer;
			if (sceneLayer != null && ((ScreenLayer)sceneLayer).Input.IsGameKeyPressed(54))
			{
				Campaign.Current.SaveHandler.QuickSaveCurrentGame();
			}
		}
		((ScreenBase)this).DebugInput.IsHotKeyPressed("MapScreenHotkeyShowPos");
	}

	protected virtual bool TickNavigationInput(float dt)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		if (((ScreenLayer)SceneLayer).Input.IsShiftDown() || ((ScreenLayer)SceneLayer).Input.IsControlDown())
		{
			return false;
		}
		bool flag = false;
		NavigationPermissionItem permission;
		if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(38))
		{
			permission = MapNavigationExtensions.GetPermission(_navigationHandler, (MapNavigationItemType)1);
			if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
			{
				OpenInventory();
				flag = true;
				goto IL_0273;
			}
		}
		if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(43))
		{
			permission = MapNavigationExtensions.GetPermission(_navigationHandler, (MapNavigationItemType)0);
			if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
			{
				OpenParty();
				flag = true;
				goto IL_0273;
			}
		}
		if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(39) && !IsInArmyManagement && !IsMapCheatsActive && !IsMapIncidentActive && !IsOverlayContextMenuEnabled)
		{
			OpenEncyclopedia();
			flag = true;
		}
		else if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(36) && !IsInArmyManagement && !IsMarriageOfferPopupActive && !IsHeirSelectionPopupActive && !IsMapCheatsActive && !IsMapIncidentActive && !EncyclopediaScreenManager.IsEncyclopediaOpen && !IsOverlayContextMenuEnabled)
		{
			OpenBannerEditorScreen();
			flag = true;
		}
		else
		{
			if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(40))
			{
				permission = MapNavigationExtensions.GetPermission(_navigationHandler, (MapNavigationItemType)5);
				if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
				{
					OpenKingdom();
					flag = true;
					goto IL_0273;
				}
			}
			if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(42))
			{
				permission = MapNavigationExtensions.GetPermission(_navigationHandler, (MapNavigationItemType)2);
				if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
				{
					OpenQuestsScreen();
					flag = true;
					goto IL_0273;
				}
			}
			if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(41))
			{
				permission = MapNavigationExtensions.GetPermission(_navigationHandler, (MapNavigationItemType)4);
				if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
				{
					OpenClanScreen();
					flag = true;
					goto IL_0273;
				}
			}
			if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(37))
			{
				permission = MapNavigationExtensions.GetPermission(_navigationHandler, (MapNavigationItemType)3);
				if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
				{
					OpenCharacterDevelopmentScreen();
					flag = true;
					goto IL_0273;
				}
			}
			if (((ScreenLayer)SceneLayer).Input.IsHotKeyReleased("ToggleEscapeMenu"))
			{
				if (!_mapViewsContainer.IsThereAnyViewIsEscaped() && !ScreenFadeController.IsFadeActive)
				{
					OpenEscapeMenu();
					flag = true;
				}
			}
			else if (((ScreenLayer)SceneLayer).Input.IsGameKeyPressed(44))
			{
				OpenFaceGeneratorScreen();
				flag = true;
			}
			else if (Input.IsGamepadActive)
			{
				flag = HandleCheatMenuInput(dt);
			}
		}
		goto IL_0273;
		IL_0273:
		if (flag)
		{
			MapCursor.SetVisible(value: false);
		}
		return flag;
	}

	void IMapStateHandler.AfterWaitTick(float dt)
	{
		TickNavigationInput(dt);
	}

	private bool HandleCheatMenuInput(float dt)
	{
		if (!IsMapCheatsActive && Input.IsKeyDown((InputKey)248) && Input.IsKeyDown((InputKey)255) && Input.IsKeyDown((InputKey)241))
		{
			_cheatPressTimer += dt;
			if (_cheatPressTimer > 0.55f)
			{
				OpenGameplayCheats();
			}
			return true;
		}
		_cheatPressTimer = 0f;
		return false;
	}

	void IMapStateHandler.OnRefreshState()
	{
		if (!(Game.Current.GameStateManager.ActiveState is MapState))
		{
			return;
		}
		if (MobileParty.MainParty.Army != null && _armyOverlay == null)
		{
			AddArmyOverlay(MapOverlayType.Army);
		}
		else if (MobileParty.MainParty.Army == null && _armyOverlay != null)
		{
			_mapViewsContainer.ForeachReverse(delegate(MapView view)
			{
				view.OnArmyLeft();
			});
			_mapViewsContainer.ForeachReverse(delegate(MapView view)
			{
				view.OnDispersePlayerLeadedArmy();
			});
		}
	}

	void IMapStateHandler.OnExitingMenuMode()
	{
		_latestMenuContext = null;
	}

	void IMapStateHandler.OnEnteringMenuMode(MenuContext menuContext)
	{
		_latestMenuContext = menuContext;
	}

	void IMapStateHandler.OnMainPartyEncounter()
	{
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnMainPartyEncounter();
		});
	}

	void IMapStateHandler.OnIncidentStarted(Incident incident)
	{
		if (GetMapView<MapIncidentView>() == null)
		{
			AddMapView<MapIncidentView>(new object[1] { incident });
		}
	}

	void IMapStateHandler.OnSignalPeriodicEvents()
	{
		DeleteMarkedPeriodicEvents();
	}

	void IMapStateHandler.OnBattleSimulationStarted(BattleSimulation battleSimulation)
	{
		IsInBattleSimulation = true;
		_battleSimulationView = AddMapView<BattleSimulationMapView>(new object[1] { CreateSimulationScoreboardDatasource(battleSimulation) });
	}

	protected virtual SPScoreboardVM CreateSimulationScoreboardDatasource(BattleSimulation battleSimulation)
	{
		return new SPScoreboardVM((BattleScoreContext)(object)new SandboxSimulationBattleScoreContext(battleSimulation), battleSimulation);
	}

	void IMapStateHandler.OnBattleSimulationEnded()
	{
		IsInBattleSimulation = false;
		RemoveMapView(_battleSimulationView);
		_battleSimulationView = null;
	}

	void IMapStateHandler.OnSiegeEngineClick(MatrixFrame siegeEngineFrame)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MapCameraView.SiegeEngineClick(siegeEngineFrame);
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IMapStateHandler.OnPlayerSiegeActivated()
	{
	}

	void IMapStateHandler.OnPlayerSiegeDeactivated()
	{
	}

	public void SetIsMapCheatsActive(bool isMapCheatsActive)
	{
		if (IsMapCheatsActive != isMapCheatsActive)
		{
			IsMapCheatsActive = isMapCheatsActive;
			_cheatPressTimer = 0f;
		}
	}

	void IMapStateHandler.OnGameplayCheatsEnabled()
	{
		OpenGameplayCheats();
	}

	void IGameStateListener.OnActivate()
	{
	}

	void IGameStateListener.OnDeactivate()
	{
	}

	void IMapStateHandler.OnMapConversationStarts(ConversationCharacterData playerCharacterData, ConversationCharacterData conversationPartnerData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		HandleMapConversationInit(playerCharacterData, conversationPartnerData);
	}

	private void HandleMapConversationInit(ConversationCharacterData playerCharacterData, ConversationCharacterData conversationPartnerData)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnMapConversationStart();
		});
		_menuViewContext?.OnMapConversationActivated();
		_conversationView.InitializeConversation(playerCharacterData, conversationPartnerData);
		MapCursor.SetVisible(value: false);
		HandleIfSceneIsReady();
	}

	void IMapStateHandler.OnMapConversationOver()
	{
		_mapViewsContainer.ForeachReverse(delegate(MapView view)
		{
			view.OnMapConversationOver();
		});
		_menuViewContext?.OnMapConversationDeactivated();
		_conversationView.FinalizeConversation();
		_activatedFrameNo = Utilities.EngineFrameNo;
		HandleIfSceneIsReady();
	}

	private void InitializeVisuals()
	{
		InactiveLightMeshes = new List<Mesh>();
		ActiveLightMeshes = new List<Mesh>();
		MapScene mapScene = Campaign.Current.MapSceneWrapper as MapScene;
		MapCursor.Initialize(this);
		_pointTargetWindDirectionDecal = DecalEntity.Create(mapScene.Scene, "decal_map_circle_wind", "MainPartyTargetLocationWindIndicatorDecal");
		_pointTargetInnerDecal = DecalEntity.Create(mapScene.Scene, "map_circle_decal", "InnerPointTarget");
		_pointTargetOuterDecal = DecalEntity.Create(mapScene.Scene, "map_circle_decal", "OuterPointTarget");
		_partyHoverOutlineDecal = DecalEntity.Create(mapScene.Scene, "map_circle_decal", "MapOutlineDecal");
		_settlementHoverOutlineDecal = DecalEntity.Create(mapScene.Scene, "decal_city_circle_a", "SettlementOutlineDecal");
		_townCircleDecal = DecalEntity.Create(mapScene.Scene, "decal_city_circle_a", "TownCircle");
		SandBoxViewSubModule.SandBoxViewVisualManager.AddEntityComponent<MapTracksVisualManager>();
		SandBoxViewSubModule.SandBoxViewVisualManager.AddEntityComponent<MapWeatherVisualManager>();
		SandBoxViewSubModule.SandBoxViewVisualManager.AddEntityComponent<MapAudioManager>();
		SandBoxViewSubModule.SandBoxViewVisualManager.AddEntityComponent<MobilePartyVisualManager>();
		SandBoxViewSubModule.SandBoxViewVisualManager.AddEntityComponent<SettlementVisualManager>();
		ContourMaskEntity = GameEntity.CreateEmpty(mapScene.Scene, true, true, true);
		ContourMaskEntity.Name = "aContourMask";
	}

	public void SetIsInTownManagement(bool isInTownManagement)
	{
		if (IsInTownManagement != isInTownManagement)
		{
			IsInTownManagement = isInTownManagement;
		}
	}

	public void SetIsInHideoutTroopManage(bool isInHideoutTroopManage)
	{
		if (IsInHideoutTroopManage != isInHideoutTroopManage)
		{
			IsInHideoutTroopManage = isInHideoutTroopManage;
		}
	}

	public void SetIsInArmyManagement(bool isInArmyManagement)
	{
		if (IsInArmyManagement != isInArmyManagement)
		{
			IsInArmyManagement = isInArmyManagement;
			if (!IsInArmyManagement)
			{
				_menuViewContext?.OnResume();
			}
		}
	}

	public void SetIsOverlayContextMenuActive(bool isOverlayContextMenuEnabled)
	{
		if (IsOverlayContextMenuEnabled != isOverlayContextMenuEnabled)
		{
			IsOverlayContextMenuEnabled = isOverlayContextMenuEnabled;
		}
	}

	public void SetIsInRecruitment(bool isInRecruitment)
	{
		if (IsInRecruitment != isInRecruitment)
		{
			IsInRecruitment = isInRecruitment;
		}
	}

	public void SetIsBarExtended(bool isBarExtended)
	{
		if (IsBarExtended != isBarExtended)
		{
			IsBarExtended = isBarExtended;
		}
	}

	public void SetIsMarriageOfferPopupActive(bool isMarriageOfferPopupActive)
	{
		if (IsMarriageOfferPopupActive != isMarriageOfferPopupActive)
		{
			IsMarriageOfferPopupActive = isMarriageOfferPopupActive;
		}
	}

	public void SetIsInCampaignOptions(bool isInCampaignOptions)
	{
		if (IsInCampaignOptions != isInCampaignOptions)
		{
			IsInCampaignOptions = isInCampaignOptions;
		}
	}

	public void SetIsMapIncidentActive(bool isMapIncidentActive)
	{
		if (IsMapIncidentActive != isMapIncidentActive)
		{
			IsMapIncidentActive = isMapIncidentActive;
		}
	}

	private void TickVisuals(float realDt)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!MapScene.IsLoadingFinished())
		{
			return;
		}
		if (DisableVisualTicks)
		{
			MapScene.ClearCurrentFrameTickEntities();
			return;
		}
		Scene mapScene = MapScene;
		CampaignTime now = CampaignTime.Now;
		mapScene.TimeOfDay = ((CampaignTime)(ref now)).CurrentHourInDay;
		float num = default(float);
		float num2 = default(float);
		Campaign.Current.Models.MapWeatherModel.GetSeasonTimeFactorOfCampaignTime(CampaignTime.Now, ref num, ref num2, false);
		MBMapScene.SetSeasonTimeFactor(MapScene, num);
		MBMapScene.TickVisuals(MapScene, Campaign.CurrentTime % (float)CampaignTime.HoursInDay, _tickedMapMeshes);
		if (IsReady)
		{
			SandBoxViewVisualManager.VisualTick(this, realDt, Campaign.Current.CampaignDt);
			TickStepSounds(realDt);
			TickCircles();
		}
		MBWindowManager.PreDisplay();
	}

	public void SetMouseVisible(bool value)
	{
		((ScreenLayer)SceneLayer).InputRestrictions.SetMouseVisibility(value);
	}

	public void SetIsHeirSelectionPopupActive(bool isHeirSelectionPopupActive)
	{
		if (IsHeirSelectionPopupActive != isHeirSelectionPopupActive)
		{
			IsHeirSelectionPopupActive = isHeirSelectionPopupActive;
		}
	}

	public bool GetMouseVisible()
	{
		return MBMapScene.GetMouseVisible();
	}

	public void RestartAmbientSounds()
	{
		if ((NativeObject)(object)MapScene != (NativeObject)null)
		{
			MapScene.ResumeSceneSounds();
		}
	}

	void IGameStateListener.OnFinalize()
	{
	}

	public void PauseAmbientSounds()
	{
		if ((NativeObject)(object)MapScene != (NativeObject)null)
		{
			MapScene.PauseSceneSounds();
		}
	}

	private void CollectTickableMapMeshes()
	{
		_tickedMapEntities = MapScene.FindEntitiesWithTag("ticked_map_entity").ToArray();
		_tickedMapMeshes = (Mesh[])(object)new Mesh[_tickedMapEntities.Length];
		for (int i = 0; i < _tickedMapEntities.Length; i++)
		{
			_tickedMapMeshes[i] = _tickedMapEntities[i].GetFirstMesh();
		}
	}

	public MBCampaignEvent CreatePeriodicUIEvent(CampaignTime triggerPeriod, CampaignTime initialWait)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		MBCampaignEvent val = new MBCampaignEvent(triggerPeriod, initialWait);
		_periodicCampaignUIEvents.Add(val);
		return val;
	}

	private void DeleteMarkedPeriodicEvents()
	{
		for (int num = _periodicCampaignUIEvents.Count - 1; num >= 0; num--)
		{
			if (_periodicCampaignUIEvents[num].isEventDeleted)
			{
				_periodicCampaignUIEvents.RemoveAt(num);
			}
		}
	}

	public void DeletePeriodicUIEvent(MBCampaignEvent campaignEvent)
	{
		campaignEvent.isEventDeleted = true;
	}

	private static float CalculateCameraElevation(float cameraDistance)
	{
		return cameraDistance * 0.5f * 0.015f + 0.35f;
	}

	public void OpenOptions()
	{
		ScreenManager.PushScreen(ViewCreator.CreateOptionsScreen(false));
	}

	public void OpenEncyclopedia()
	{
		Campaign.Current.EncyclopediaManager.GoToLink("LastPage", "");
	}

	public void OpenSaveLoad(bool isSaving)
	{
		ScreenManager.PushScreen(SandBoxViewCreator.CreateSaveLoadScreen(isSaving));
	}

	public void CloseEscapeMenu()
	{
		OnEscapeMenuToggled();
	}

	public void OpenEscapeMenu()
	{
		OnEscapeMenuToggled(isOpened: true);
	}

	private void OpenGameplayCheats()
	{
		_mapCheatsView = AddMapView<MapCheatsView>(Array.Empty<object>());
		IsMapCheatsActive = true;
	}

	public void CloseGameplayCheats()
	{
		if (_mapCheatsView != null)
		{
			RemoveMapView(_mapCheatsView);
		}
		else
		{
			Debug.FailedAssert("Requested remove map cheats but cheats is not enabled", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\MapScreen.cs", "CloseGameplayCheats", 2571);
		}
	}

	public void CloseCampaignOptions()
	{
		if (_campaignOptionsView == null)
		{
			Debug.FailedAssert("Trying to close campaign options when it's not set", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\MapScreen.cs", "CloseCampaignOptions", 2579);
			_campaignOptionsView = GetMapView<MapCampaignOptionsView>();
			if (_campaignOptionsView == null)
			{
				Debug.FailedAssert("Trying to close campaign options when it's not open", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\MapScreen.cs", "CloseCampaignOptions", 2584);
				IsInCampaignOptions = false;
				_campaignOptionsView = null;
				return;
			}
		}
		if (_campaignOptionsView != null)
		{
			RemoveMapView(_campaignOptionsView);
		}
		_campaignOptionsView = null;
		IsInCampaignOptions = false;
	}

	private List<EscapeMenuItemVM> GetEscapeMenuItems()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Expected O, but got Unknown
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Expected O, but got Unknown
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Expected O, but got Unknown
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Expected O, but got Unknown
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Expected O, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		bool isMapConversationActive = _conversationView.IsConversationActive;
		bool cannotQuickSave = MBSaveLoad.IsMaxNumberOfSavesReached() && !MBSaveLoad.IsSaveGameFileExists(MBSaveLoad.ActiveSaveSlotName);
		if (cannotQuickSave && CampaignOptions.IsIronmanMode)
		{
			string activeSaveSlotName = MBSaveLoad.ActiveSaveSlotName;
			string[] saveFileNames = MBSaveLoad.GetSaveFileNames();
			for (int i = 0; i < saveFileNames.Length; i++)
			{
				if (saveFileNames[i] == activeSaveSlotName)
				{
					cannotQuickSave = false;
					break;
				}
			}
		}
		return new List<EscapeMenuItemVM>
		{
			new EscapeMenuItemVM(new TextObject("{=e139gKZc}Return to the Game", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				OnEscapeMenuToggled();
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), true),
			new EscapeMenuItemVM(new TextObject("{=PXT6aA4J}Campaign Options", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				_campaignOptionsView = AddMapView<MapCampaignOptionsView>(Array.Empty<object>());
				IsInCampaignOptions = true;
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false),
			new EscapeMenuItemVM(new TextObject("{=NqarFr4P}Options", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				OnEscapeMenuToggled();
				OpenOptions();
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false),
			new EscapeMenuItemVM(new TextObject("{=bV75iwKa}Save", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				OnEscapeMenuToggled();
				Campaign.Current.SaveHandler.QuickSaveCurrentGame();
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => GetIsEscapeMenuOptionDisabledReason(isMapConversationActive, isIronmanMode: false, cannotQuickSave)), false),
			new EscapeMenuItemVM(new TextObject("{=e0KdfaNe}Save As", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				OnEscapeMenuToggled();
				OpenSaveLoad(isSaving: true);
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => GetIsEscapeMenuOptionDisabledReason(isMapConversationActive, CampaignOptions.IsIronmanMode, cannotQuickSave: false)), false),
			new EscapeMenuItemVM(new TextObject("{=9NuttOBC}Load", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				OnEscapeMenuToggled();
				OpenSaveLoad(isSaving: false);
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => GetIsEscapeMenuOptionDisabledReason(isMapConversationActive, CampaignOptions.IsIronmanMode, cannotQuickSave: false)), false),
			new EscapeMenuItemVM(new TextObject("{=AbEh2y8o}Save And Exit", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				Campaign.Current.SaveHandler.QuickSaveCurrentGame();
				OnEscapeMenuToggled();
				InformationManager.HideInquiry();
				_exitOnSaveOver = true;
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => GetIsEscapeMenuOptionDisabledReason(isMapConversationActive, isIronmanMode: false, cannotQuickSave)), false),
			new EscapeMenuItemVM(new TextObject("{=RamV6yLM}Exit to Main Menu", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				//IL_007f: Unknown result type (might be due to invalid IL or missing references)
				//IL_008b: Expected O, but got Unknown
				InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_exit", (string)null)).ToString(), ((object)GameTexts.FindText("str_mission_exit_query", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)OnExitToMainMenu, (Action)delegate
				{
					OnEscapeMenuToggled();
				}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => GetIsEscapeMenuOptionDisabledReason(isMapConversationActive: false, CampaignOptions.IsIronmanMode, cannotQuickSave: false)), false)
		};
	}

	private Tuple<bool, TextObject> GetIsEscapeMenuOptionDisabledReason(bool isMapConversationActive, bool isIronmanMode, bool cannotQuickSave)
	{
		if (isIronmanMode)
		{
			return new Tuple<bool, TextObject>(item1: true, GameTexts.FindText("str_pause_menu_disabled_hint", "IronmanMode"));
		}
		if (isMapConversationActive)
		{
			return new Tuple<bool, TextObject>(item1: true, GameTexts.FindText("str_pause_menu_disabled_hint", "OngoingConversation"));
		}
		if (cannotQuickSave)
		{
			return new Tuple<bool, TextObject>(item1: true, GameTexts.FindText("str_pause_menu_disabled_hint", "SaveLimitReached"));
		}
		return new Tuple<bool, TextObject>(item1: false, null);
	}

	private void OpenParty()
	{
		if (Hero.MainHero != null && !Hero.MainHero.IsPrisoner && !Hero.MainHero.IsDead)
		{
			PartyScreenHelper.OpenScreenAsNormal();
		}
	}

	public void OpenInventory()
	{
		if (Hero.MainHero != null)
		{
			Hero mainHero = Hero.MainHero;
			if (mainHero != null && !mainHero.IsDead)
			{
				InventoryScreenHelper.OpenScreenAsInventory((Action)null);
			}
		}
	}

	private void OpenKingdom()
	{
		if (Hero.MainHero != null)
		{
			Hero mainHero = Hero.MainHero;
			if (mainHero != null && !mainHero.IsDead && Hero.MainHero.MapFaction.IsKingdomFaction)
			{
				KingdomState val = Game.Current.GameStateManager.CreateState<KingdomState>();
				Game.Current.GameStateManager.PushState((GameState)(object)val, 0);
			}
		}
	}

	private void OnExitToMainMenu()
	{
		OnEscapeMenuToggled();
		InformationManager.HideInquiry();
		OnExit();
	}

	private void OpenQuestsScreen()
	{
		if (Hero.MainHero != null)
		{
			Hero mainHero = Hero.MainHero;
			if (mainHero != null && !mainHero.IsDead)
			{
				Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<QuestsState>(), 0);
			}
		}
	}

	private void OpenClanScreen()
	{
		if (Hero.MainHero != null)
		{
			Hero mainHero = Hero.MainHero;
			if (mainHero != null && !mainHero.IsDead)
			{
				Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<ClanState>(), 0);
			}
		}
	}

	private void OpenCharacterDevelopmentScreen()
	{
		if (Hero.MainHero != null)
		{
			Hero mainHero = Hero.MainHero;
			if (mainHero != null && !mainHero.IsDead)
			{
				Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<CharacterDeveloperState>(), 0);
			}
		}
	}

	public void OpenFacegenScreenAux()
	{
		OpenFaceGeneratorScreen();
	}

	public bool IsCameraLockedToPlayerParty()
	{
		return MapCameraView.IsCameraLockedToPlayerParty();
	}

	public void FastMoveCameraToMainParty()
	{
		MapCameraView.FastMoveCameraToMainParty();
	}

	public void ResetCamera(bool resetDistance, bool teleportToMainParty)
	{
		MapCameraView.ResetCamera(resetDistance, teleportToMainParty);
	}

	public void TeleportCameraToMainParty()
	{
		MapCameraView.TeleportCameraToMainParty();
	}

	void IChatLogHandlerScreen.TryUpdateChatLogLayerParameters(ref bool isTeamChatAvailable, ref bool inputEnabled, ref bool isToggleChatHintAvailable, ref bool isMouseVisible, ref InputContext inputContext)
	{
		if (SceneLayer != null)
		{
			inputEnabled = true;
			isToggleChatHintAvailable = true;
			inputContext = ((ScreenLayer)SceneLayer).Input;
			isMouseVisible = ((ScreenLayer)SceneLayer).InputRestrictions.MouseVisibility;
		}
	}

	private void TickCircles()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Invalid comparison between Unknown and I4
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0730: Unknown result type (might be due to invalid IL or missing references)
		//IL_0735: Unknown result type (might be due to invalid IL or missing references)
		//IL_073b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0742: Unknown result type (might be due to invalid IL or missing references)
		//IL_0747: Unknown result type (might be due to invalid IL or missing references)
		//IL_074c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0751: Unknown result type (might be due to invalid IL or missing references)
		//IL_075a: Unknown result type (might be due to invalid IL or missing references)
		//IL_075f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0839: Unknown result type (might be due to invalid IL or missing references)
		//IL_083b: Unknown result type (might be due to invalid IL or missing references)
		//IL_082c: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b6: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		float num = 0.5f;
		float num2 = 0.5f;
		int num3 = 0;
		int num4 = 0;
		uint factor1Linear = 4293199122u;
		uint factor1Linear2 = 4293199122u;
		uint factor1Linear3 = 4293199122u;
		bool flag4 = false;
		bool flag5 = false;
		MatrixFrame val = MatrixFrame.Identity;
		PartyBase val2 = null;
		CampaignVec2 val3;
		if ((int)MobileParty.MainParty.PartyMoveMode == 1 && (int)MobileParty.MainParty.DefaultBehavior != 2 && (int)MobileParty.MainParty.DefaultBehavior != 0 && !MobileParty.MainParty.ForceAiNoPathMode && MobileParty.MainParty.Ai.AiBehaviorInteractable == null && MobileParty.MainParty.MapEvent == null)
		{
			val3 = MobileParty.MainParty.TargetPosition;
			if (((CampaignVec2)(ref val3)).DistanceSquared(MobileParty.MainParty.Position) > 0.01f)
			{
				flag3 = true;
				flag = true;
				num = 0.238846f;
				num2 = 0.278584f;
				num3 = 4;
				num4 = 5;
				factor1Linear = 4293993473u;
				factor1Linear2 = 4293993473u;
				val3 = MobileParty.MainParty.TargetPosition;
				val.origin = ((CampaignVec2)(ref val3)).AsVec3();
				flag5 = true;
				goto IL_0306;
			}
		}
		if ((int)MobileParty.MainParty.PartyMoveMode == 2 && MobileParty.MainParty.MoveTargetParty != null && MobileParty.MainParty.MoveTargetParty.IsVisible)
		{
			val2 = ((MobileParty.MainParty.MoveTargetParty.CurrentSettlement != null && !MobileParty.MainParty.MoveTargetParty.CurrentSettlement.IsHideout) ? MobileParty.MainParty.MoveTargetParty.CurrentSettlement.Party : MobileParty.MainParty.MoveTargetParty.Party);
		}
		else if ((int)MobileParty.MainParty.DefaultBehavior == 2 && MobileParty.MainParty.TargetSettlement != null)
		{
			val2 = MobileParty.MainParty.TargetSettlement.Party;
		}
		Vec2 val5;
		if (val2 != null)
		{
			if (val2.IsMobile)
			{
				MapEntityVisual<PartyBase> partyVisual = GetPartyVisual(val2);
				if (partyVisual != null)
				{
					val = partyVisual.CircleLocalFrame;
					flag3 = true;
					num3 = GetCircleIndex();
					float num5 = 1.2f;
					if (val2.MobileParty.IsCurrentlyAtSea)
					{
						num5 = 2.5f;
					}
					factor1Linear = GetDecalColorForParty(val2);
					num = ((Mat3)(ref val.rotation)).GetScaleVector().x * num5;
				}
			}
			else
			{
				val = SettlementVisualManager.Current.GetSettlementVisual(val2.Settlement).CircleLocalFrame;
				if (val2.IsSettlement && val2.Settlement.IsFortification)
				{
					flag4 = true;
					flag2 = true;
					factor1Linear3 = GetDecalColorForParty(val2);
					num = ((Mat3)(ref val.rotation)).GetScaleVector().x * 1.3f;
				}
				else
				{
					flag3 = true;
					num3 = 5;
					factor1Linear = GetDecalColorForParty(val2);
					num = ((Mat3)(ref val.rotation)).GetScaleVector().x * 1.2f;
				}
			}
			if (!flag4)
			{
				val3 = val2.Position;
				val.origin = ((CampaignVec2)(ref val3)).AsVec3();
				if (val2.IsMobile)
				{
					ref Vec3 origin = ref val.origin;
					Vec3 val4 = origin;
					val5 = val2.MobileParty.EventPositionAdder + val2.MobileParty.ArmyPositionAdder;
					origin = val4 + ((Vec2)(ref val5)).ToVec3(0f);
				}
			}
		}
		goto IL_0306;
		IL_084a:
		MatrixFrame val6;
		if (flag4)
		{
			_settlementHoverOutlineDecal.GameEntity.SetGlobalFrame(ref val6, true);
			_settlementHoverOutlineDecal.GameEntity.SetVisibilityExcludeParents(true);
			_partyHoverOutlineDecal.GameEntity.SetVisibilityExcludeParents(false);
			return;
		}
		Vec3 val7;
		if (mapEntityVisual.MapEntity.IsMobile && mapEntityVisual.MapEntity.MobileParty.IsCurrentlyAtSea)
		{
			val7 = Vec3.One * 2.5f;
			((MatrixFrame)(ref val6)).Scale(ref val7);
		}
		_partyHoverOutlineDecal.GameEntity.SetGlobalFrame(ref val6, true);
		_settlementHoverOutlineDecal.GameEntity.SetVisibilityExcludeParents(false);
		_partyHoverOutlineDecal.GameEntity.SetVisibilityExcludeParents(true);
		return;
		IL_0845:
		ref Vec3 origin2;
		float z;
		origin2.z = z;
		goto IL_084a;
		IL_0306:
		if (flag5)
		{
			float num6 = (Instance.MapCameraView.CameraDistance + 80f) * (Instance.MapCameraView.CameraDistance + 80f) / 5000f;
			num6 = MathF.Clamp(num6, 0.2f, 45f);
			num *= num6;
			num2 *= num6;
		}
		if (val2 == null)
		{
			_targetCircleRotationStartTime = 0f;
		}
		else if (_targetCircleRotationStartTime == 0f)
		{
			_targetCircleRotationStartTime = MBCommon.GetApplicationTime();
		}
		Vec3 normalAt = Instance.MapScene.GetNormalAt(((Vec3)(ref val.origin)).AsVec2);
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = val.origin;
		MobileParty mainParty = MobileParty.MainParty;
		bool flag6 = mainParty != null && !mainParty.TargetPosition.IsOnLand;
		bool flag7 = val2 != null;
		MobileParty mainParty2 = MobileParty.MainParty;
		if (mainParty2 != null)
		{
			_ = mainParty2.IsCurrentlyAtSea;
		}
		identity.rotation.u = normalAt;
		MatrixFrame val8 = identity;
		ref Mat3 rotation = ref identity.rotation;
		val7 = new Vec3(num, num, num, -1f);
		((Mat3)(ref rotation)).ApplyScaleLocal(ref val7);
		ref Mat3 rotation2 = ref val8.rotation;
		val7 = new Vec3(num2, num2, num2, -1f);
		((Mat3)(ref rotation2)).ApplyScaleLocal(ref val7);
		_townCircleDecal.GameEntity.SetVisibilityExcludeParents(flag2);
		_pointTargetInnerDecal.GameEntity.SetVisibilityExcludeParents(flag3 && (!flag6 || flag7));
		_pointTargetOuterDecal.GameEntity.SetVisibilityExcludeParents(flag && (!flag6 || flag7));
		_pointTargetWindDirectionDecal.GameEntity.SetVisibilityExcludeParents(flag3 && flag6 && !flag7);
		if (flag3)
		{
			if (flag6 && !flag7)
			{
				float num7 = num + 0.15f;
				MatrixFrame val9 = identity;
				val5 = Campaign.Current.Models.MapWeatherModel.GetWindForPosition(MobileParty.MainParty.TargetPosition);
				val7 = ((Vec2)(ref val5)).ToVec3(0f);
				Vec3 val10 = ((Vec3)(ref val7)).NormalizedCopy();
				val9.rotation = Mat3.CreateMat3WithForward(ref val10);
				ref Mat3 rotation3 = ref val9.rotation;
				val7 = new Vec3(num7, num7, num7, -1f);
				((Mat3)(ref rotation3)).ApplyScaleLocal(ref val7);
				((Mat3)(ref val9.rotation)).RotateAboutUp(MathF.PI / 2f);
				_pointTargetWindDirectionDecal.Decal.SetFactor1Linear(factor1Linear);
				_pointTargetWindDirectionDecal.Decal.SetVectorArgument(1f, 1f, 0f, 0f);
				_pointTargetWindDirectionDecal.GameEntity.SetGlobalFrame(ref val9, true);
			}
			else
			{
				_pointTargetInnerDecal.Decal.SetVectorArgument(0.166f, 1f, 0.166f * (float)num3, 0f);
				_pointTargetInnerDecal.Decal.SetFactor1Linear(factor1Linear);
				_pointTargetInnerDecal.GameEntity.SetGlobalFrame(ref identity, true);
			}
		}
		if (flag)
		{
			_pointTargetOuterDecal.Decal.SetVectorArgument(0.166f, 1f, 0.166f * (float)num4, 0f);
			_pointTargetOuterDecal.Decal.SetFactor1Linear(factor1Linear2);
			_pointTargetOuterDecal.GameEntity.SetGlobalFrame(ref val8, true);
		}
		if (flag2)
		{
			_townCircleDecal.Decal.SetVectorArgument(1f, 1f, 0f, 0f);
			_townCircleDecal.Decal.SetFactor1Linear(factor1Linear3);
			_townCircleDecal.GameEntity.SetGlobalFrame(ref val, true);
		}
		val6 = MatrixFrame.Identity;
		if (Instance.CurrentVisualOfTooltip != null && (val2 == null || Instance.CurrentVisualOfTooltip != GetPartyVisual(val2)) && Instance.CurrentVisualOfTooltip is MapEntityVisual<PartyBase> mapEntityVisual)
		{
			Instance.MapCursor.OnAnotherEntityHighlighted();
			if (mapEntityVisual != null)
			{
				flag4 = mapEntityVisual.MapEntity.IsSettlement && mapEntityVisual.MapEntity.Settlement.IsFortification;
				if (flag4)
				{
					val6 = mapEntityVisual.CircleLocalFrame;
					_settlementHoverOutlineDecal.Decal.SetFactor1Linear(GetDecalColorForParty(mapEntityVisual.MapEntity));
					goto IL_084a;
				}
				Vec3 origin3 = _settlementHoverOutlineDecal.GameEntity.GetGlobalFrame().origin;
				val6.origin = mapEntityVisual.GetVisualPosition() + mapEntityVisual.CircleLocalFrame.origin;
				val6.rotation = mapEntityVisual.CircleLocalFrame.rotation;
				_partyHoverOutlineDecal.Decal.SetFactor1Linear(GetDecalColorForParty(mapEntityVisual.MapEntity));
				_partyHoverOutlineDecal.Decal.SetVectorArgument(0.166f, 1f, 0.83f, 0f);
				origin2 = ref val6.origin;
				if (!(((Vec3)(ref origin3)).AsVec2 != ((Vec3)(ref val6.origin)).AsVec2))
				{
					z = origin3.z;
				}
				else
				{
					PartyBase mapEntity = mapEntityVisual.MapEntity;
					if (mapEntity != null)
					{
						MobileParty mobileParty = mapEntity.MobileParty;
						if (((mobileParty != null) ? new bool?(mobileParty.IsCurrentlyAtSea) : ((bool?)null)) == true)
						{
							z = val6.origin.z;
							goto IL_0845;
						}
					}
					z = Instance.MapScene.GetTerrainHeight(((Vec3)(ref val6.origin)).AsVec2, true);
				}
				goto IL_0845;
			}
			_settlementHoverOutlineDecal.GameEntity.SetVisibilityExcludeParents(false);
			_partyHoverOutlineDecal.GameEntity.SetVisibilityExcludeParents(false);
			return;
		}
		_settlementHoverOutlineDecal.GameEntity.SetVisibilityExcludeParents(false);
		_partyHoverOutlineDecal.GameEntity.SetVisibilityExcludeParents(false);
	}

	private int GetCircleIndex()
	{
		int num = (int)((MBCommon.GetApplicationTime() - _targetCircleRotationStartTime) / 0.1f) % 10;
		if (num >= 5)
		{
			num = 10 - num - 1;
		}
		return num;
	}

	private MapEntityVisual<PartyBase> GetPartyVisual(PartyBase party)
	{
		MapEntityVisual<PartyBase> mapEntityVisual = null;
		foreach (EntityVisualManagerBase<PartyBase> component in SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents<EntityVisualManagerBase<PartyBase>>())
		{
			mapEntityVisual = component.GetVisualOfEntity(party);
			if (mapEntityVisual != null)
			{
				break;
			}
		}
		return mapEntityVisual;
	}

	private void TickStepSounds(float realDt)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (NativeConfig.DisableSound || !(ScreenManager.TopScreen is MapScreen))
		{
			return;
		}
		_soundCalculationTime += realDt;
		if (IsSoundOn && Campaign.Current.CampaignDt > 0f)
		{
			MobileParty mainParty = MobileParty.MainParty;
			float seeingRange = mainParty.SeeingRange;
			CampaignVec2 position = mainParty.Position;
			LocatableSearchData<MobileParty> val = MobileParty.StartFindingLocatablesAroundPosition(((CampaignVec2)(ref position)).ToVec2(), seeingRange + 25f);
			for (MobileParty val2 = MobileParty.FindNextLocatable(ref val); val2 != null; val2 = MobileParty.FindNextLocatable(ref val))
			{
				if (!val2.IsMilitia && !val2.IsGarrison && !val2.IsCurrentlyAtSea)
				{
					StepSounds(val2);
				}
			}
		}
		if (_soundCalculationTime > 0.2f)
		{
			_soundCalculationTime -= 0.2f;
		}
	}

	private void StepSounds(MobileParty party)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected I4, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (!party.IsVisible || party.MemberRoster.TotalManCount <= 0)
		{
			return;
		}
		MobilePartyVisual partyVisual = MobilePartyVisualManager.Current.GetPartyVisual(party.Party);
		if (partyVisual.HumanAgentVisuals == null)
		{
			return;
		}
		TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace);
		AgentVisuals val = null;
		TerrainTypeSoundSlot val2 = (TerrainTypeSoundSlot)0;
		if (partyVisual.CaravanMountAgentVisuals != null)
		{
			val2 = (TerrainTypeSoundSlot)5;
			val = partyVisual.CaravanMountAgentVisuals;
		}
		else if (partyVisual.HumanAgentVisuals != null)
		{
			if (partyVisual.MountAgentVisuals != null)
			{
				val2 = (TerrainTypeSoundSlot)3;
				if (party.Army != null && ((List<MobileParty>)(object)party.AttachedParties).Count > 0)
				{
					val2 = (TerrainTypeSoundSlot)2;
				}
				val = partyVisual.MountAgentVisuals;
			}
			else
			{
				val2 = (TerrainTypeSoundSlot)0;
				if (party.Army != null && ((List<MobileParty>)(object)party.AttachedParties).Count > 0)
				{
					val2 = (TerrainTypeSoundSlot)1;
				}
				val = partyVisual.HumanAgentVisuals;
			}
		}
		if (party.AttachedTo == null)
		{
			MBMapScene.TickStepSound(MapScene, val.GetVisuals(), (int)faceTerrainType, val2, ((List<MobileParty>)(object)party.AttachedParties).Count);
		}
	}

	private uint GetDecalColorForParty(PartyBase targetParty)
	{
		if (FactionManager.IsAtWarAgainstFaction(targetParty.MapFaction, Hero.MainHero.MapFaction))
		{
			return 4292093218u;
		}
		if (DiplomacyHelper.IsSameFactionAndNotEliminated(targetParty.MapFaction, Hero.MainHero.MapFaction))
		{
			return 4284183827u;
		}
		if (DiplomacyHelper.HasAllianceWithFaction(targetParty.MapFaction, Hero.MainHero.MapFaction))
		{
			return 4279386828u;
		}
		return 4291596077u;
	}
}
