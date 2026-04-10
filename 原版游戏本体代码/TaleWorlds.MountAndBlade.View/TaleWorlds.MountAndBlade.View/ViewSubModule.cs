using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.InputSystem;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GameKeyCategory;
using TaleWorlds.MountAndBlade.View.CustomBattle;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.MountAndBlade.View.VisualOrders;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View;

public class ViewSubModule : MBSubModuleBase
{
	private Dictionary<Tuple<Material, Banner>, Material> _bannerTexturedMaterialCache;

	private GameStateScreenManager _gameStateScreenManager;

	private bool _newGameInitialization;

	private VisualOrderProvider _visualOrderProvider;

	private static ViewSubModule _instance;

	private bool _initialized;

	private DLCInstallationQueryView _dlcInstallationQueryView;

	public static Dictionary<Tuple<Material, Banner>, Material> BannerTexturedMaterialCache
	{
		get
		{
			return _instance._bannerTexturedMaterialCache;
		}
		set
		{
			_instance._bannerTexturedMaterialCache = value;
		}
	}

	public static GameStateScreenManager GameStateScreenManager => _instance._gameStateScreenManager;

	private void InitializeHotKeyManager()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		string text = "BannerlordGameKeys.xml";
		HotKeyManager.Initialize(new PlatformFilePath(EngineFilePaths.ConfigsPath, text), !ScreenManager.IsEnterButtonRDown);
		HotKeyManager.RegisterInitialContexts((IEnumerable<GameKeyContext>)new List<GameKeyContext>
		{
			(GameKeyContext)new GenericGameKeyContext(),
			(GameKeyContext)new GenericCampaignPanelsGameKeyCategory("GenericCampaignPanelsGameKeyCategory"),
			(GameKeyContext)new GenericPanelGameKeyCategory("GenericPanelGameKeyCategory"),
			(GameKeyContext)new ArmyManagementHotkeyCategory(),
			(GameKeyContext)new BoardGameHotkeyCategory(),
			(GameKeyContext)new ChatLogHotKeyCategory(),
			(GameKeyContext)new CombatHotKeyCategory(),
			(GameKeyContext)new CraftingHotkeyCategory(),
			(GameKeyContext)new FaceGenHotkeyCategory(),
			(GameKeyContext)new InventoryHotKeyCategory(),
			(GameKeyContext)new PartyHotKeyCategory(),
			(GameKeyContext)new MapHotKeyCategory(),
			(GameKeyContext)new MapNotificationHotKeyCategory(),
			(GameKeyContext)new MissionOrderHotkeyCategory(),
			(GameKeyContext)new OrderOfBattleHotKeyCategory(),
			(GameKeyContext)new MultiplayerHotkeyCategory(),
			(GameKeyContext)new ScoreboardHotKeyCategory(),
			(GameKeyContext)new ConversationHotKeyCategory(),
			(GameKeyContext)new CheatsHotKeyCategory(),
			(GameKeyContext)new PhotoModeHotKeyCategory(),
			(GameKeyContext)new PollHotkeyCategory()
		});
	}

	private void InitializeBannerVisualManager()
	{
		if (BannerManager.Instance == null)
		{
			BannerManager.Initialize();
			BannerManager.Instance.LoadBannerIcons();
		}
	}

	protected override void OnSubModuleLoad()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_012e: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected O, but got Unknown
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Expected O, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Expected O, but got Unknown
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Expected O, but got Unknown
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Expected O, but got Unknown
		((MBSubModuleBase)this).OnSubModuleLoad();
		_instance = this;
		InitializeHotKeyManager();
		InitializeBannerVisualManager();
		CraftedDataViewManager.Initialize();
		_visualOrderProvider = (VisualOrderProvider)(object)new DefaultVisualOrderProvider();
		VisualOrderFactory.RegisterProvider(_visualOrderProvider);
		_gameStateScreenManager = new GameStateScreenManager();
		Module.CurrentModule.GlobalGameStateManager.RegisterListener((IGameStateManagerListener)(object)_gameStateScreenManager);
		MBMusicManager.Create();
		TextObject coreContentDisabledReason = new TextObject("{=V8BXjyYq}Disabled during installation.", (Dictionary<string, object>)null);
		if (Utilities.EditModeEnabled)
		{
			Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Editor", new TextObject("{=bUh0x6rA}Editor", (Dictionary<string, object>)null), -1, (Action)delegate
			{
				MBInitialScreenBase.OnEditModeEnterPress();
			}, (Func<ValueTuple<bool, TextObject>>)(() => (Module.CurrentModule.IsOnlyCoreContentEnabled, coreContentDisabledReason)), (TextObject)null, (Func<bool>)null));
		}
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("CustomBattle", new TextObject("{=4gOGGbeQ}Custom Battle", (Dictionary<string, object>)null), 5000, (Action)CustomBattleFactory.StartCustomBattle, (Func<ValueTuple<bool, TextObject>>)(() => ((bool, TextObject))(false, null)), (TextObject)null, (Func<bool>)(() => CustomBattleFactory.GetProviderCount() == 0)));
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Options", new TextObject("{=NqarFr4P}Options", (Dictionary<string, object>)null), 9998, (Action)delegate
		{
			ScreenManager.PushScreen(ViewCreator.CreateOptionsScreen(fromMainMenu: true));
		}, (Func<ValueTuple<bool, TextObject>>)(() => ((bool, TextObject))(false, null)), (TextObject)null, (Func<bool>)null));
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Credits", new TextObject("{=ODQmOrIw}Credits", (Dictionary<string, object>)null), 9999, (Action)delegate
		{
			ScreenManager.PushScreen(ViewCreator.CreateCreditsScreen());
		}, (Func<ValueTuple<bool, TextObject>>)(() => ((bool, TextObject))(false, null)), (TextObject)null, (Func<bool>)null));
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Exit", new TextObject("{=YbpzLHzk}Exit Game", (Dictionary<string, object>)null), 10000, (Action)delegate
		{
			MBInitialScreenBase.DoExitButtonAction();
		}, (Func<ValueTuple<bool, TextObject>>)(() => (Module.CurrentModule.IsOnlyCoreContentEnabled, coreContentDisabledReason)), (TextObject)null, (Func<bool>)null));
		ViewModel.RefreshPropertyAndMethodInfos();
		Module.CurrentModule.ImguiProfilerTick += OnImguiProfilerTick;
		ScreenManager.OnPushScreen += new OnPushScreenEvent(OnScreenManagerPushScreen);
		EngineController.OnConstrainedStateChanged += OnConstrainedStateChange;
		HyperlinkTexts.IsPlayStationGamepadActive = GetIsPlaystationGamepadActive;
		_dlcInstallationQueryView = new DLCInstallationQueryView();
		_dlcInstallationQueryView.Initialize();
	}

	private void OnModuleStructureChanged()
	{
		ViewModel.RefreshPropertyAndMethodInfos();
	}

	private void OnConstrainedStateChange(bool isConstrained)
	{
		ScreenManager.OnConstrainStateChanged(isConstrained);
	}

	private bool GetIsPlaystationGamepadActive()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Input.IsPlaystation(Input.ControllerType);
		return Input.IsGamepadActive && flag;
	}

	protected override void OnSubModuleUnloaded()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		_dlcInstallationQueryView?.OnFinalize();
		_dlcInstallationQueryView = null;
		VisualOrderFactory.UnregisterProvider(_visualOrderProvider);
		ThumbnailCacheManager.ClearManager();
		BannerlordTableauManager.ClearManager();
		CraftedDataViewManager.Clear();
		Module.CurrentModule.ImguiProfilerTick -= OnImguiProfilerTick;
		ScreenManager.OnPushScreen -= new OnPushScreenEvent(OnScreenManagerPushScreen);
		EngineController.OnConstrainedStateChanged -= OnConstrainedStateChange;
		_instance = null;
		((MBSubModuleBase)this).OnSubModuleUnloaded();
	}

	protected override void OnBeforeInitialModuleScreenSetAsRoot()
	{
		if (_initialized)
		{
			BannerPersistentTextureCache.Current?.FlushCache();
		}
		if (!_initialized)
		{
			BannerlordTableauManager.InitializeCharacterTableauRenderSystem();
			ThumbnailCacheManager.InitializeManager();
			ThumbnailCacheManager.Current.RegisterThumbnailCache(new AvatarThumbnailCache(75));
			ThumbnailCacheManager.Current.RegisterThumbnailCache(new BannerThumbnailCache(100));
			ThumbnailCacheManager.Current.RegisterThumbnailCache(new BannerPersistentTextureCache());
			ThumbnailCacheManager.Current.RegisterThumbnailCache(new BannerEditorTextureCache(5));
			ThumbnailCacheManager.Current.RegisterThumbnailCache(new CharacterThumbnailCache(75));
			ThumbnailCacheManager.Current.RegisterThumbnailCache(new CraftingPieceThumbnailCache(75));
			ThumbnailCacheManager.Current.RegisterThumbnailCache(new ItemThumbnailCache(75));
			_initialized = true;
		}
	}

	protected override void OnNewModuleLoad()
	{
		ViewCreatorManager.CollectTypes();
		ViewModel.RefreshPropertyAndMethodInfos();
		_gameStateScreenManager.CollectTypes();
	}

	protected override void OnApplicationTick(float dt)
	{
		((MBSubModuleBase)this).OnApplicationTick(dt);
		if (Input.DebugInput.IsHotKeyPressed("ToggleUI"))
		{
			MBDebug.DisableUI(new List<string>());
		}
		HotKeyManager.Tick(dt);
		MBMusicManager current = MBMusicManager.Current;
		if (current != null)
		{
			current.Update(dt);
		}
		ThumbnailCacheManager.Current?.Tick(dt);
	}

	protected override void AfterAsyncTickTick(float dt)
	{
		MBMusicManager current = MBMusicManager.Current;
		if (current != null)
		{
			current.Update(dt);
		}
	}

	protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		MissionWeapon.OnGetWeaponDataHandler = new OnGetWeaponDataDelegate(ItemCollectionElementViewExtensions.OnGetWeaponData);
	}

	public override void OnCampaignStart(Game game, object starterObject)
	{
		Game.Current.GameStateManager.RegisterListener((IGameStateManagerListener)(object)_gameStateScreenManager);
		_newGameInitialization = false;
	}

	public override void OnMultiplayerGameStart(Game game, object starterObject)
	{
		Game.Current.GameStateManager.RegisterListener((IGameStateManagerListener)(object)_gameStateScreenManager);
	}

	public override void OnGameLoaded(Game game, object initializerObject)
	{
		Game.Current.GameStateManager.RegisterListener((IGameStateManagerListener)(object)_gameStateScreenManager);
	}

	public override void OnGameInitializationFinished(Game game)
	{
		((MBSubModuleBase)this).OnGameInitializationFinished(game);
		foreach (ItemObject item in (List<ItemObject>)(object)Game.Current.ObjectManager.GetObjectTypeList<ItemObject>())
		{
			if (item.MultiMeshName != "")
			{
				MBUnusedResourceManager.SetMeshUsed(item.MultiMeshName);
			}
			HorseComponent horseComponent = item.HorseComponent;
			if (horseComponent != null)
			{
				foreach (KeyValuePair<string, bool> additionalMeshesName in horseComponent.AdditionalMeshesNameList)
				{
					MBUnusedResourceManager.SetMeshUsed(additionalMeshesName.Key);
				}
			}
			if (item.PrimaryWeapon != null)
			{
				MBUnusedResourceManager.SetMeshUsed(item.HolsterMeshName);
				MBUnusedResourceManager.SetMeshUsed(item.HolsterWithWeaponMeshName);
				MBUnusedResourceManager.SetMeshUsed(item.FlyingMeshName);
				MBUnusedResourceManager.SetBodyUsed(item.BodyName);
				MBUnusedResourceManager.SetBodyUsed(item.HolsterBodyName);
				MBUnusedResourceManager.SetBodyUsed(item.CollisionBodyName);
			}
		}
	}

	public override void BeginGameStart(Game game)
	{
		((MBSubModuleBase)this).BeginGameStart(game);
		Game.Current.BannerVisualCreator = (IBannerVisualCreator)(object)new BannerVisualCreator();
	}

	public override bool DoLoading(Game game)
	{
		if (_newGameInitialization)
		{
			return true;
		}
		_newGameInitialization = true;
		return _newGameInitialization;
	}

	public override void OnGameEnd(Game game)
	{
		MissionWeapon.OnGetWeaponDataHandler = null;
		CraftedDataViewManager.Clear();
	}

	private void OnImguiProfilerTick()
	{
	}

	private void OnScreenManagerPushScreen(ScreenBase pushedScreen)
	{
	}
}
