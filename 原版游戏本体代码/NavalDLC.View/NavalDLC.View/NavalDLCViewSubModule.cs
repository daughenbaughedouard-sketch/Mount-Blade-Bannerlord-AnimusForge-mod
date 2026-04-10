using System;
using NavalDLC.HotKeyCategories;
using NavalDLC.View.Map;
using NavalDLC.View.Map.Managers;
using NavalDLC.View.Missions;
using NavalDLC.View.Overlay;
using NavalDLC.View.Permissions;
using NavalDLC.View.VisualOrders;
using SandBox;
using SandBox.View;
using SandBox.View.Map;
using SandBox.ViewModelCollection.Missions.NameMarker;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order.Visual;
using TaleWorlds.ScreenSystem;

namespace NavalDLC.View;

public class NavalDLCViewSubModule : MBSubModuleBase
{
	private NavalShipVisualOrderProvider _shipVisualOrderProvider;

	private NavalTroopVisualOrderProvider _troopVisualOrderProvider;

	private NavalGameMenuOverlayProvider _gameMenuOverlayProvider;

	protected override void OnSubModuleLoad()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		((MBSubModuleBase)this).OnSubModuleLoad();
		RegisterHotKeyContexts();
		RegisterTooltipTypes();
		_shipVisualOrderProvider = new NavalShipVisualOrderProvider();
		_troopVisualOrderProvider = new NavalTroopVisualOrderProvider();
		VisualOrderFactory.RegisterProvider((VisualOrderProvider)(object)_shipVisualOrderProvider);
		VisualOrderFactory.RegisterProvider((VisualOrderProvider)(object)_troopVisualOrderProvider);
		_gameMenuOverlayProvider = new NavalGameMenuOverlayProvider();
		GameMenuOverlayFactory.RegisterProvider((IGameMenuOverlayProvider)(object)_gameMenuOverlayProvider);
		MissionNameMarkerFactory.DefaultContext.AddProvider<NavalMissionNameMarkerProvider>();
		ScreenManager.OnPushScreen += new OnPushScreenEvent(OnScreenPushed);
	}

	public override void OnNewGameCreated(Game game, object initializerObject)
	{
		if (game.GameType is Campaign)
		{
			NavalDLCManager.Instance.NavalMapSceneWrapper = new NavalMapSceneWrapper();
		}
	}

	public override void OnAfterGameLoaded(Game game)
	{
		if (game.GameType is Campaign)
		{
			NavalDLCManager.Instance.NavalMapSceneWrapper = new NavalMapSceneWrapper();
		}
	}

	public override void OnGameInitializationFinished(Game game)
	{
		((MBSubModuleBase)this).OnGameInitializationFinished(game);
		NavalPermissionsSystem.OnInitialize();
	}

	public override void OnGameEnd(Game game)
	{
		((MBSubModuleBase)this).OnGameEnd(game);
		NavalPermissionsSystem.OnUnload();
		VisualShipFactory.DeregisterVisualShipCache();
	}

	protected override void OnSubModuleUnloaded()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		((MBSubModuleBase)this).OnSubModuleUnloaded();
		UnregisterTooltipTypes();
		VisualOrderFactory.UnregisterProvider((VisualOrderProvider)(object)_shipVisualOrderProvider);
		VisualOrderFactory.UnregisterProvider((VisualOrderProvider)(object)_troopVisualOrderProvider);
		GameMenuOverlayFactory.UnregisterProvider((IGameMenuOverlayProvider)(object)_gameMenuOverlayProvider);
		ScreenManager.OnPushScreen -= new OnPushScreenEvent(OnScreenPushed);
	}

	public override void OnSubModuleDeactivated()
	{
	}

	public override void OnSubModuleActivated()
	{
	}

	private void RegisterTooltipTypes()
	{
		InformationManager.RegisterTooltip<ShipHull, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)NavalTooltipRefresherCollection.RefreshShipHullTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<ShipUpgradePiece, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)NavalTooltipRefresherCollection.RefreshShipPieceTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Figurehead, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)NavalTooltipRefresherCollection.RefreshFigureheadTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<AnchorPoint, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)NavalTooltipRefresherCollection.RefreshAnchorPointTooltip, "PropertyBasedTooltip");
		InformationManager.RegisterTooltip<Settlement, PropertyBasedTooltipVM>((Action<PropertyBasedTooltipVM, object[]>)NavalTooltipRefresherCollection.RefreshSettlementTooltip, "PropertyBasedTooltip");
	}

	private void UnregisterTooltipTypes()
	{
		InformationManager.UnregisterTooltip<ShipHull>();
		InformationManager.UnregisterTooltip<ShipUpgradePiece>();
		InformationManager.UnregisterTooltip<Figurehead>();
		InformationManager.UnregisterTooltip<AnchorPoint>();
		InformationManager.UnregisterTooltip<Settlement>();
	}

	private void RegisterHotKeyContexts()
	{
		HotKeyManager.RegisterContext((GameKeyContext)(object)new NavalShipControlsHotKeyCategory(), false);
		HotKeyManager.RegisterContext((GameKeyContext)(object)new PortHotKeyCategory(), false);
		HotKeyManager.RegisterContext((GameKeyContext)(object)new NavalCheatsHotKeyCategory(), true);
	}

	private void OnScreenPushed(ScreenBase pushedScreen)
	{
		MapScreen val;
		if ((val = (MapScreen)(object)((pushedScreen is MapScreen) ? pushedScreen : null)) != null)
		{
			val.AddMapView<NavalMapAnchorTrackerView>(Array.Empty<object>());
		}
	}

	public override void OnAfterGameInitializationFinished(Game game, object starterObject)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		((MBSubModuleBase)this).OnAfterGameInitializationFinished(game, starterObject);
		if (Campaign.Current != null && Campaign.Current.MapSceneWrapper != null)
		{
			VisualShipFactory.InitializeShipEntityCache(((MapScene)Campaign.Current.MapSceneWrapper).Scene);
			SandBoxViewSubModule.SandBoxViewVisualManager.AddEntityComponent<NavalMobilePartyVisualManager>();
			SandBoxViewSubModule.SandBoxViewVisualManager.AddEntityComponent<AnchorVisualManager>();
			SandBoxViewSubModule.SandBoxViewVisualManager.AddEntityComponent<StormVisualManager>();
		}
	}
}
