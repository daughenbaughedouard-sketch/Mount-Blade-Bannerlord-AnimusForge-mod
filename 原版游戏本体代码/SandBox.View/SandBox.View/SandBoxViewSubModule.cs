using System;
using System.Collections.Generic;
using SandBox.View.Conversation;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using SandBox.View.Missions.NameMarkers;
using SandBox.View.OrderProviders;
using SandBox.View.Overlay;
using SandBox.ViewModelCollection.Missions.NameMarker;
using Sandbox.View.GameStates;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Information.RundownTooltip;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;
using TaleWorlds.ScreenSystem;

namespace SandBox.View;

public class SandBoxViewSubModule : MBSubModuleBase
{
	private bool _latestSaveLoaded;

	private TextObject _sandBoxAchievementsHint = new TextObject("{=j09m7S2E}Achievements are disabled in SandBox mode!", (Dictionary<string, object>)null);

	private bool _isInitialized;

	private HideoutVisualOrderProvider _hideoutVisualOrderProvider;

	private ConversationViewManager _conversationViewManager;

	private SandBoxViewVisualManager _sandBoxViewVisualManager;

	private IMapConversationDataProvider _mapConversationDataProvider;

	private IGameMenuOverlayProvider _gameMenuOverlayProvider;

	private Dictionary<UIntPtr, MapEntityVisual> _visualsOfEntities;

	private Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>> _frameAndVisualOfEngines;

	private static SandBoxViewSubModule _instance;

	public static SandBoxViewVisualManager SandBoxViewVisualManager => _instance._sandBoxViewVisualManager;

	public static ConversationViewManager ConversationViewManager => _instance._conversationViewManager;

	public static IMapConversationDataProvider MapConversationDataProvider => _instance._mapConversationDataProvider;

	internal static Dictionary<UIntPtr, MapEntityVisual> VisualsOfEntities => _instance._visualsOfEntities;

	internal static Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>> FrameAndVisualOfEngines => _instance._frameAndVisualOfEngines;

	protected override void OnSubModuleLoad()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		((MBSubModuleBase)this).OnSubModuleLoad();
		_instance = this;
		RegisterTooltipTypes();
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("CampaignResumeGame", new TextObject("{=6mN03uTP}Saved Games", (Dictionary<string, object>)null), 0, (Action)delegate
		{
			ScreenManager.PushScreen(SandBoxViewCreator.CreateSaveLoadScreen(isSaving: false));
		}, (Func<ValueTuple<bool, TextObject>>)(() => IsSavedGamesDisabled()), (TextObject)null, (Func<bool>)null));
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("ContinueCampaign", new TextObject("{=0tJ1oarX}Continue Campaign", (Dictionary<string, object>)null), 1, (Action)delegate
		{
			PreloadState val = GameStateManager.Current.CreateState<PreloadState>(new object[1] { BannerlordConfig.LatestSaveGameName });
			GameStateManager.Current.PushState((GameState)(object)val, 0);
		}, (Func<ValueTuple<bool, TextObject>>)(() => IsContinueCampaignDisabled(BannerlordConfig.LatestSaveGameName)), (TextObject)null, (Func<bool>)null));
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("SandBoxNewGame", new TextObject("{=171fTtIN}SandBox", (Dictionary<string, object>)null), 3, (Action)delegate
		{
			StartGame();
		}, (Func<ValueTuple<bool, TextObject>>)(() => IsSandboxDisabled()), _sandBoxAchievementsHint, (Func<bool>)null));
		SandBoxSaveHelper.OnStateChange += OnSaveHelperStateChange;
		Module.CurrentModule.ImguiProfilerTick += OnImguiProfilerTick;
		_gameMenuOverlayProvider = (IGameMenuOverlayProvider)(object)new DefaultGameMenuOverlayProvider();
		GameMenuOverlayFactory.RegisterProvider(_gameMenuOverlayProvider);
		MissionNameMarkerFactory.DefaultContext.AddProvider<DefaultMissionNameMarkerHandler>();
		MissionNameMarkerFactory.DefaultContext.AddProvider<StealthNameMarkerProvider>();
		_mapConversationDataProvider = new DefaultMapConversationDataProvider();
		_hideoutVisualOrderProvider = new HideoutVisualOrderProvider();
		VisualOrderFactory.RegisterProvider((VisualOrderProvider)(object)_hideoutVisualOrderProvider);
	}

	protected override void OnSubModuleUnloaded()
	{
		Module.CurrentModule.ImguiProfilerTick -= OnImguiProfilerTick;
		SandBoxSaveHelper.OnStateChange -= OnSaveHelperStateChange;
		GameMenuOverlayFactory.UnregisterProvider(_gameMenuOverlayProvider);
		VisualOrderFactory.UnregisterProvider((VisualOrderProvider)(object)_hideoutVisualOrderProvider);
		UnregisterTooltipTypes();
		_instance = null;
		((MBSubModuleBase)this).OnSubModuleUnloaded();
	}

	protected override void OnApplicationTick(float dt)
	{
		((MBSubModuleBase)this).OnApplicationTick(dt);
		if (!_isInitialized)
		{
			CampaignOptionsManager.Initialize();
			_isInitialized = true;
		}
	}

	public override void OnCampaignStart(Game game, object starterObject)
	{
		((MBSubModuleBase)this).OnCampaignStart(game, starterObject);
		if (Campaign.Current != null)
		{
			_conversationViewManager = new ConversationViewManager();
			_sandBoxViewVisualManager = new SandBoxViewVisualManager();
		}
	}

	public override void OnGameLoaded(Game game, object initializerObject)
	{
		_conversationViewManager = new ConversationViewManager();
		_sandBoxViewVisualManager = new SandBoxViewVisualManager();
	}

	public override void OnAfterGameInitializationFinished(Game game, object starterObject)
	{
		((MBSubModuleBase)this).OnAfterGameInitializationFinished(game, starterObject);
	}

	public override void BeginGameStart(Game game)
	{
		((MBSubModuleBase)this).BeginGameStart(game);
		if (Campaign.Current != null)
		{
			_visualsOfEntities = new Dictionary<UIntPtr, MapEntityVisual>();
			_frameAndVisualOfEngines = new Dictionary<UIntPtr, Tuple<MatrixFrame, SettlementVisual>>();
			Campaign.Current.SaveHandler.MainHeroVisualSupplier = (IMainHeroVisualSupplier)(object)new MainHeroSaveVisualSupplier();
			ThumbnailCacheManager.InitializeSandboxValues();
		}
	}

	public override void OnGameEnd(Game game)
	{
		if (_visualsOfEntities != null)
		{
			foreach (MapEntityVisual value in _visualsOfEntities.Values)
			{
				value.ReleaseResources();
			}
		}
		_visualsOfEntities = null;
		_frameAndVisualOfEngines = null;
		_conversationViewManager = null;
		_sandBoxViewVisualManager = null;
		if (Campaign.Current != null)
		{
			Campaign.Current.SaveHandler.MainHeroVisualSupplier = null;
			ThumbnailCacheManager.ReleaseSandboxValues();
		}
	}

	private (bool, TextObject) IsSavedGamesDisabled()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if (Module.CurrentModule.IsOnlyCoreContentEnabled)
		{
			return (true, new TextObject("{=V8BXjyYq}Disabled during installation.", (Dictionary<string, object>)null));
		}
		if (MBSaveLoad.NumberOfCurrentSaves == 0)
		{
			return (true, new TextObject("{=XcVVE1mp}No saved games found.", (Dictionary<string, object>)null));
		}
		return (false, null);
	}

	private (bool, TextObject) IsContinueCampaignDisabled(string saveName)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		if (Module.CurrentModule.IsOnlyCoreContentEnabled)
		{
			return (true, new TextObject("{=V8BXjyYq}Disabled during installation.", (Dictionary<string, object>)null));
		}
		if (string.IsNullOrEmpty(saveName))
		{
			return (true, new TextObject("{=aWMZQKXZ}Save the game at least once to continue", (Dictionary<string, object>)null));
		}
		SaveGameFileInfo saveFileWithName = MBSaveLoad.GetSaveFileWithName(saveName);
		if (saveFileWithName == null)
		{
			return (true, new TextObject("{=60LTq0tQ}Can't find the save file for the latest save game.", (Dictionary<string, object>)null));
		}
		TextObject reason;
		return (SandBoxSaveHelper.GetIsDisabledWithReason(saveFileWithName, out reason), reason);
	}

	private (bool, TextObject) IsSandboxDisabled()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (Module.CurrentModule.IsOnlyCoreContentEnabled)
		{
			return (true, new TextObject("{=V8BXjyYq}Disabled during installation.", (Dictionary<string, object>)null));
		}
		return (false, null);
	}

	private void ContinueCampaign(string saveName)
	{
		SandBoxSaveHelper.TryLoadSave(MBSaveLoad.GetSaveFileWithName(saveName), StartGame);
	}

	public override void OnInitialState()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		((MBSubModuleBase)this).OnInitialState();
		if (!Module.CurrentModule.StartupInfo.IsContinueGame || _latestSaveLoaded)
		{
			return;
		}
		_latestSaveLoaded = true;
		SaveGameFileInfo[] saveFiles = MBSaveLoad.GetSaveFiles((Func<SaveGameFileInfo, bool>)null);
		if (!Extensions.IsEmpty<SaveGameFileInfo>((IEnumerable<SaveGameFileInfo>)saveFiles))
		{
			string name = Extensions.MaxBy<SaveGameFileInfo, DateTime>((IEnumerable<SaveGameFileInfo>)saveFiles, (Func<SaveGameFileInfo, DateTime>)((SaveGameFileInfo s) => MetaDataExtensions.GetCreationTime(s.MetaData))).Name;
			(bool, TextObject) tuple = IsContinueCampaignDisabled(name);
			if (!tuple.Item1)
			{
				ContinueCampaign(name);
			}
			else
			{
				InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null)).ToString(), ((object)tuple.Item2).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), string.Empty, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
		}
	}

	private void StartGame(LoadResult loadResult)
	{
		MBGameManager.StartNewGame((MBGameManager)(object)new SandBoxGameManager(loadResult));
	}

	private void StartGame()
	{
		MBGameManager.StartNewGame((MBGameManager)(object)new SandBoxGameManager((SandBoxGameManager.CampaignCreatorDelegate)(() => new Campaign((CampaignGameMode)1))));
	}

	private void OnImguiProfilerTick()
	{
		if (Campaign.Current == null)
		{
			return;
		}
		MBReadOnlyList<MobileParty> all = MobileParty.All;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<EntityVisualManagerBase<PartyBase>> components = SandBoxViewVisualManager.GetComponents<EntityVisualManagerBase<PartyBase>>();
		foreach (MobileParty item in (List<MobileParty>)(object)all)
		{
			if (item.IsMilitia || item.IsGarrison)
			{
				continue;
			}
			if (item.IsVisible)
			{
				num++;
			}
			MapEntityVisual<PartyBase> mapEntityVisual = null;
			foreach (EntityVisualManagerBase<PartyBase> item2 in components)
			{
				MapEntityVisual<PartyBase> visualOfEntity = item2.GetVisualOfEntity(PartyBase.MainParty);
				if (visualOfEntity != null)
				{
					mapEntityVisual = visualOfEntity;
				}
			}
			if (mapEntityVisual == null)
			{
				continue;
			}
			if (mapEntityVisual is MobilePartyVisual mobilePartyVisual)
			{
				if (mobilePartyVisual.HumanAgentVisuals != null)
				{
					num2++;
				}
				if (mobilePartyVisual.MountAgentVisuals != null)
				{
					num2++;
				}
				if (mobilePartyVisual.CaravanMountAgentVisuals != null)
				{
					num2++;
				}
			}
			num3++;
		}
		Imgui.BeginMainThreadScope();
		Imgui.Begin("Bannerlord Campaign Statistics");
		Imgui.Columns(2, "", true);
		Imgui.Text("Name");
		Imgui.NextColumn();
		Imgui.Text("Count");
		Imgui.NextColumn();
		Imgui.Separator();
		Imgui.Text("Total Mobile Party");
		Imgui.NextColumn();
		Imgui.Text(num3.ToString());
		Imgui.NextColumn();
		Imgui.Text("Visible Mobile Party");
		Imgui.NextColumn();
		Imgui.Text(num.ToString());
		Imgui.NextColumn();
		Imgui.Text("Total Agent Visuals");
		Imgui.NextColumn();
		Imgui.Text(num2.ToString());
		Imgui.NextColumn();
		Imgui.End();
		Imgui.EndMainThreadScope();
	}

	private void RegisterTooltipTypes()
	{
		InformationManager.RegisterTooltip<List<MobileParty>, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshEncounterTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Track, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshTrackTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<MapEvent, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshMapEventTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<SiegeEvent, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshSiegeEventTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Army, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshArmyTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<MobileParty, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshMobilePartyTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Hero, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshHeroTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Settlement, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshSettlementTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<CharacterObject, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshCharacterTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<WeaponDesignElement, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshCraftingPartTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<InventoryLogic, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshInventoryTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<ItemObject, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshItemTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Building, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshBuildingTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Workshop, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshWorkshopTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Clan, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshClanTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Kingdom, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshKingdomTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<MapMarker, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)TooltipRefresherCollection.RefreshMapMarkerTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<ExplainedNumber, RundownTooltipVM>((Action<RundownTooltipVM, object[]>)TooltipRefresherCollection.RefreshExplainedNumberTooltip, "RundownTooltip");
	}

	private void UnregisterTooltipTypes()
	{
		InformationManager.UnregisterTooltip<List<MobileParty>>();
		InformationManager.UnregisterTooltip<Track>();
		InformationManager.UnregisterTooltip<MapEvent>();
		InformationManager.UnregisterTooltip<Army>();
		InformationManager.UnregisterTooltip<MobileParty>();
		InformationManager.UnregisterTooltip<Hero>();
		InformationManager.UnregisterTooltip<Settlement>();
		InformationManager.UnregisterTooltip<CharacterObject>();
		InformationManager.UnregisterTooltip<WeaponDesignElement>();
		InformationManager.UnregisterTooltip<InventoryLogic>();
		InformationManager.UnregisterTooltip<ItemObject>();
		InformationManager.UnregisterTooltip<Building>();
		InformationManager.UnregisterTooltip<Workshop>();
		InformationManager.UnregisterTooltip<Clan>();
		InformationManager.UnregisterTooltip<Kingdom>();
		InformationManager.UnregisterTooltip<ExplainedNumber>();
	}

	public static void SetMapConversationDataProvider(IMapConversationDataProvider mapConversationDataProvider)
	{
		_instance._mapConversationDataProvider = mapConversationDataProvider;
	}

	private static void OnSaveHelperStateChange(SandBoxSaveHelper.SaveHelperState currentState)
	{
		switch (currentState)
		{
		case SandBoxSaveHelper.SaveHelperState.Start:
		case SandBoxSaveHelper.SaveHelperState.LoadGame:
			LoadingWindow.EnableGlobalLoadingWindow();
			break;
		case SandBoxSaveHelper.SaveHelperState.Inquiry:
			LoadingWindow.DisableGlobalLoadingWindow();
			break;
		default:
			Debug.FailedAssert("Undefined save state for listener!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\SandBoxViewSubModule.cs", "OnSaveHelperStateChange", 683);
			break;
		}
	}
}
