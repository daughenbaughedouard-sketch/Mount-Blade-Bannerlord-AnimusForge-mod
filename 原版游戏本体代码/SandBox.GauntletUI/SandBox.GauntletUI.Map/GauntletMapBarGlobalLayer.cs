using System;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Map;

public class GauntletMapBarGlobalLayer : GlobalLayer
{
	protected MapBarVM _dataSource;

	protected GauntletLayer _gauntletLayer;

	protected GauntletMovieIdentifier _movie;

	protected SpriteCategory _mapBarCategory;

	protected MapScreen _mapScreen;

	protected INavigationHandler _mapNavigationHandler;

	protected MapEncyclopediaView _encyclopediaManager;

	protected float _contextAlphaTarget = 1f;

	protected float _contextAlphaModifider;

	private GauntletLayer _armyManagementLayer;

	private SpriteCategory _armyManagementCategory;

	private ArmyManagementVM _armyManagementVM;

	private GauntletMovieIdentifier _gauntletArmyManagementMovie;

	private CampaignTimeControlMode _timeControlModeBeforeArmyManagementOpened;

	public bool IsInArmyManagement
	{
		get
		{
			if (_armyManagementLayer != null)
			{
				return _armyManagementVM != null;
			}
			return false;
		}
	}

	public GauntletMapBarGlobalLayer(MapScreen mapScreen, INavigationHandler navigationHandler, float contextAlphaModifider)
	{
		_mapScreen = mapScreen;
		_mapNavigationHandler = navigationHandler;
		_contextAlphaModifider = contextAlphaModifider;
		_mapScreen.NavigationHandler = navigationHandler;
	}

	public void Initialize(MapBarVM dataSource)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		_dataSource = dataSource;
		_dataSource.Initialize(_mapNavigationHandler, (IMapStateHandler)(object)_mapScreen, (Func<MapBarShortcuts>)GetMapBarShortcuts, (Action)OpenArmyManagement);
		_gauntletLayer = new GauntletLayer("MapBar", 202, false);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)_gauntletLayer;
		_mapBarCategory = UIResourceManager.LoadSpriteCategory("ui_mapbar");
		_movie = _gauntletLayer.LoadMovie("MapBar", (ViewModel)(object)_dataSource);
		_encyclopediaManager = _mapScreen.EncyclopediaScreenManager;
	}

	public void OnFinalize()
	{
		if (_gauntletLayer != null)
		{
			if (_gauntletArmyManagementMovie != null)
			{
				_gauntletLayer.ReleaseMovie(_gauntletArmyManagementMovie);
			}
			if (_movie != null)
			{
				_gauntletLayer.ReleaseMovie(_movie);
			}
		}
		ArmyManagementVM armyManagementVM = _armyManagementVM;
		if (armyManagementVM != null)
		{
			((ViewModel)armyManagementVM).OnFinalize();
		}
		((ViewModel)_dataSource).OnFinalize();
		_mapBarCategory.Unload();
		_armyManagementVM = null;
		_gauntletLayer = null;
		_dataSource = null;
		_encyclopediaManager = null;
		_mapScreen = null;
	}

	public void OnMapConversationStarted()
	{
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		}
	}

	public void OnMapConversationOver()
	{
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
		}
	}

	public void Refresh()
	{
		MapBarVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnRefresh();
		}
	}

	private MapBarShortcuts GetMapBarShortcuts()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		return new MapBarShortcuts
		{
			FastForwardHotkey = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "MapHotKeyCategory", 62)).ToString(),
			PauseHotkey = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "MapHotKeyCategory", 60)).ToString(),
			PlayHotkey = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "MapHotKeyCategory", 61)).ToString()
		};
	}

	protected override void OnTick(float dt)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		((GlobalLayer)this).OnTick(dt);
		_gauntletLayer.UIContext.ContextAlpha = MathF.Lerp(_gauntletLayer.UIContext.ContextAlpha, _contextAlphaTarget, dt * _contextAlphaModifider, 1E-05f);
		Game current = Game.Current;
		GameState val = ((current != null) ? current.GameStateManager.ActiveState : null);
		ScreenBase topScreen = ScreenManager.TopScreen;
		INavigationHandler mapNavigationHandler = _mapNavigationHandler;
		bool flag = mapNavigationHandler != null && mapNavigationHandler.IsAnyElementActive();
		if (topScreen is MapScreen || flag)
		{
			_dataSource.IsEnabled = true;
			_dataSource.CurrentScreen = ((object)topScreen).GetType().Name;
			bool flag2 = ScreenManager.TopScreen is MapScreen;
			_dataSource.MapTimeControl.IsInMap = flag2;
			((GlobalLayer)this).Layer.InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
			if (!(val is MapState))
			{
				_dataSource.MapTimeControl.IsCenterPanelEnabled = false;
				if (flag)
				{
					HandlePanelSwitching();
				}
				_contextAlphaTarget = 1f;
			}
			else
			{
				_ = (MapState)val;
				if (flag2)
				{
					MapScreen mapScreen = ScreenManager.TopScreen as MapScreen;
					mapScreen.SetIsBarExtended(_dataSource.MapInfo.IsInfoBarExtended);
					_dataSource.MapTimeControl.IsInRecruitment = mapScreen.IsInRecruitment;
					_dataSource.MapTimeControl.IsInBattleSimulation = mapScreen.IsInBattleSimulation;
					_dataSource.MapTimeControl.IsEncyclopediaOpen = _encyclopediaManager?.IsEncyclopediaOpen ?? false;
					_dataSource.MapTimeControl.IsInArmyManagement = mapScreen.IsInArmyManagement;
					_dataSource.MapTimeControl.IsInTownManagement = mapScreen.IsInTownManagement;
					_dataSource.MapTimeControl.IsInHideoutTroopManage = mapScreen.IsInHideoutTroopManage;
					_dataSource.MapTimeControl.IsInCampaignOptions = mapScreen.IsInCampaignOptions;
					_dataSource.MapTimeControl.IsEscapeMenuOpened = mapScreen.IsEscapeMenuOpened;
					_dataSource.MapTimeControl.IsMarriageOfferPopupActive = mapScreen.IsMarriageOfferPopupActive;
					_dataSource.MapTimeControl.IsHeirSelectionPopupActive = mapScreen.IsHeirSelectionPopupActive;
					_dataSource.MapTimeControl.IsMapCheatsActive = mapScreen.IsMapCheatsActive;
					_dataSource.MapTimeControl.IsMapIncidentActive = mapScreen.IsMapIncidentActive;
					_dataSource.MapTimeControl.IsOverlayContextMenuEnabled = mapScreen.IsOverlayContextMenuEnabled;
					if (mapScreen.GetMapView<MapConversationView>()?.ConversationMission != null)
					{
						_contextAlphaTarget = 0f;
					}
					else
					{
						_contextAlphaTarget = 1f;
					}
					if (_armyManagementVM != null)
					{
						HandleArmyManagementInput();
					}
				}
				else
				{
					_dataSource.MapTimeControl.IsCenterPanelEnabled = false;
				}
			}
			_dataSource.Tick(dt);
		}
		else
		{
			_dataSource.IsEnabled = false;
			((GlobalLayer)this).Layer.InputRestrictions.ResetInputRestrictions();
		}
	}

	private void HandleArmyManagementInput()
	{
		if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_armyManagementVM.ExecuteCancel();
		}
		else if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("Confirm"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_armyManagementVM.ExecuteDone();
		}
		else if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("Reset"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_armyManagementVM.ExecuteReset();
		}
		else if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("RemoveParty") && _armyManagementVM.FocusedItem != null)
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_armyManagementVM.FocusedItem.ExecuteAction();
		}
	}

	private void HandlePanelSwitching()
	{
		GauntletLayer val = ScreenManager.TopScreen.FindLayer<GauntletLayer>();
		SceneLayer val2 = ScreenManager.TopScreen.FindLayer<SceneLayer>();
		if (((val != null) ? ((ScreenLayer)val).Input : null) != null && !((ScreenLayer)val).IsFocusedOnInput() && (object)ScreenManager.FocusedLayer == val)
		{
			HandlePanelSwitchingInput(((ScreenLayer)val).Input);
		}
		else if (((val2 != null) ? ((ScreenLayer)val2).Input : null) != null && (object)ScreenManager.FocusedLayer == val2)
		{
			HandlePanelSwitchingInput(((ScreenLayer)val2).Input);
		}
	}

	protected virtual bool HandlePanelSwitchingInput(InputContext inputContext)
	{
		if (inputContext.IsGameKeyReleased(37) && !MapNavigationExtensions.IsActive(_mapNavigationHandler, (MapNavigationItemType)3))
		{
			INavigationHandler mapNavigationHandler = _mapNavigationHandler;
			if (mapNavigationHandler != null)
			{
				MapNavigationExtensions.OpenCharacterDeveloper(mapNavigationHandler);
			}
			return true;
		}
		if (inputContext.IsGameKeyReleased(43) && !MapNavigationExtensions.IsActive(_mapNavigationHandler, (MapNavigationItemType)0))
		{
			INavigationHandler mapNavigationHandler2 = _mapNavigationHandler;
			if (mapNavigationHandler2 != null)
			{
				MapNavigationExtensions.OpenParty(mapNavigationHandler2);
			}
			return true;
		}
		if (inputContext.IsGameKeyReleased(42) && !MapNavigationExtensions.IsActive(_mapNavigationHandler, (MapNavigationItemType)2))
		{
			INavigationHandler mapNavigationHandler3 = _mapNavigationHandler;
			if (mapNavigationHandler3 != null)
			{
				MapNavigationExtensions.OpenQuests(mapNavigationHandler3);
			}
			return true;
		}
		if (inputContext.IsGameKeyReleased(38) && !MapNavigationExtensions.IsActive(_mapNavigationHandler, (MapNavigationItemType)1))
		{
			INavigationHandler mapNavigationHandler4 = _mapNavigationHandler;
			if (mapNavigationHandler4 != null)
			{
				MapNavigationExtensions.OpenInventory(mapNavigationHandler4);
			}
			return true;
		}
		if (inputContext.IsGameKeyReleased(41) && !MapNavigationExtensions.IsActive(_mapNavigationHandler, (MapNavigationItemType)4))
		{
			INavigationHandler mapNavigationHandler5 = _mapNavigationHandler;
			if (mapNavigationHandler5 != null)
			{
				MapNavigationExtensions.OpenClan(mapNavigationHandler5);
			}
			return true;
		}
		if (inputContext.IsGameKeyReleased(40) && !MapNavigationExtensions.IsActive(_mapNavigationHandler, (MapNavigationItemType)5))
		{
			INavigationHandler mapNavigationHandler6 = _mapNavigationHandler;
			if (mapNavigationHandler6 != null)
			{
				MapNavigationExtensions.OpenKingdom(mapNavigationHandler6);
			}
			return true;
		}
		return false;
	}

	private void OpenArmyManagement()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		if (_gauntletLayer != null)
		{
			_armyManagementLayer = new GauntletLayer("MapBar_ArmyManagement", 300, false);
			_armyManagementCategory = UIResourceManager.LoadSpriteCategory("ui_armymanagement");
			_armyManagementVM = new ArmyManagementVM((Action)CloseArmyManagement);
			_gauntletArmyManagementMovie = _armyManagementLayer.LoadMovie("ArmyManagement", (ViewModel)(object)_armyManagementVM);
			((ScreenLayer)_armyManagementLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			((ScreenLayer)_armyManagementLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
			((ScreenLayer)_armyManagementLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
			((ScreenLayer)_armyManagementLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
			((ScreenLayer)_armyManagementLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ArmyManagementHotkeyCategory"));
			((ScreenLayer)_armyManagementLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_armyManagementLayer);
			((ScreenBase)_mapScreen).AddLayer((ScreenLayer)(object)_armyManagementLayer);
			_armyManagementVM.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
			_armyManagementVM.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
			_armyManagementVM.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
			_armyManagementVM.SetRemoveInputKey(HotKeyManager.GetCategory("ArmyManagementHotkeyCategory").GetHotKey("RemoveParty"));
			_timeControlModeBeforeArmyManagementOpened = Campaign.Current.TimeControlMode;
			Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
			Campaign.Current.SetTimeControlModeLock(true);
			if (ScreenManager.TopScreen is MapScreen mapScreen)
			{
				mapScreen.SetIsInArmyManagement(isInArmyManagement: true);
			}
		}
	}

	private void CloseArmyManagement()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		((ViewModel)_armyManagementVM).OnFinalize();
		_armyManagementLayer.ReleaseMovie(_gauntletArmyManagementMovie);
		((ScreenBase)_mapScreen).RemoveLayer((ScreenLayer)(object)_armyManagementLayer);
		_armyManagementCategory.Unload();
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)4));
		_gauntletArmyManagementMovie = null;
		_armyManagementVM = null;
		_armyManagementLayer = null;
		Campaign.Current.SetTimeControlModeLock(false);
		Campaign.Current.TimeControlMode = _timeControlModeBeforeArmyManagementOpened;
		if (ScreenManager.TopScreen is MapScreen mapScreen)
		{
			mapScreen.SetIsInArmyManagement(isInArmyManagement: false);
		}
	}

	public bool IsEscaped()
	{
		if (_armyManagementVM != null)
		{
			_armyManagementVM.ExecuteCancel();
			return true;
		}
		return false;
	}
}
