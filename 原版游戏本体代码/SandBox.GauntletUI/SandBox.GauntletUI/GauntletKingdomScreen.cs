using System;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Decisions.ItemTypes;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI;

[GameStateScreen(typeof(KingdomState))]
public class GauntletKingdomScreen : ScreenBase, IGameStateListener
{
	private GauntletLayer _gauntletLayer;

	private readonly KingdomState _kingdomState;

	private GauntletLayer _armyManagementLayer;

	private ArmyManagementVM _armyManagementDatasource;

	private SpriteCategory _kingdomCategory;

	private SpriteCategory _armyManagementCategory;

	private SpriteCategory _clanCategory;

	public KingdomManagementVM DataSource { get; private set; }

	public bool IsMakingDecision => DataSource.Decision.IsActive;

	public GauntletKingdomScreen(KingdomState kingdomState)
	{
		_kingdomState = kingdomState;
	}

	protected override void OnInitialize()
	{
		((ScreenBase)this).OnInitialize();
		InformationManager.HideAllMessages();
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		LoadingWindow.DisableGlobalLoadingWindow();
		DataSource.CanSwitchTabs = !InformationManager.GetIsAnyTooltipActiveAndExtended();
		if (MapScreen.Instance != null)
		{
			MapScreen.Instance.NavigationHandler.IsNavigationLocked = DataSource.Decision.IsActive;
		}
		if (DataSource.Decision.IsActive)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
			{
				DecisionItemBaseVM currentDecision = DataSource.Decision.CurrentDecision;
				if (currentDecision != null && currentDecision.CanEndDecision)
				{
					DataSource.Decision.CurrentDecision.ExecuteFinalSelection();
					UISoundsHelper.PlayUISound("event:/ui/reign/decision");
				}
			}
		}
		else if (DataSource.GiftFief.IsOpen)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
			{
				if (DataSource.GiftFief.IsAnyClanSelected)
				{
					DataSource.GiftFief.ExecuteGiftSettlement();
					UISoundsHelper.PlayUISound("event:/ui/default");
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
			{
				DataSource.GiftFief.ExecuteClose();
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
		}
		else if (_armyManagementDatasource != null)
		{
			if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("Exit"))
			{
				_armyManagementDatasource.ExecuteCancel();
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
			else if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("Confirm"))
			{
				_armyManagementDatasource.ExecuteDone();
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
			else if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("Reset"))
			{
				_armyManagementDatasource.ExecuteReset();
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
			else if (((ScreenLayer)_armyManagementLayer).Input.IsHotKeyReleased("RemoveParty") && _armyManagementDatasource.FocusedItem != null)
			{
				_armyManagementDatasource.FocusedItem.ExecuteAction();
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit") || ((ScreenLayer)_gauntletLayer).Input.IsGameKeyPressed(40) || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
		{
			CloseKingdomScreen();
		}
		else if (DataSource.CanSwitchTabs)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("SwitchToPreviousTab"))
			{
				DataSource.SelectPreviousCategory();
				UISoundsHelper.PlayUISound("event:/ui/tab");
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("SwitchToNextTab"))
			{
				DataSource.SelectNextCategory();
				UISoundsHelper.PlayUISound("event:/ui/tab");
			}
		}
		KingdomManagementVM dataSource = DataSource;
		if (dataSource != null)
		{
			dataSource.OnFrameTick();
		}
	}

	protected virtual KingdomManagementVM CreateDataSource()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		return new KingdomManagementVM((Action)CloseKingdomScreen, (Action)OpenArmyManagement, (Action<Army>)ShowArmyOnMap);
	}

	void IGameStateListener.OnActivate()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected O, but got Unknown
		((ScreenBase)this).OnActivate();
		_kingdomCategory = UIResourceManager.LoadSpriteCategory("ui_kingdom");
		_clanCategory = UIResourceManager.LoadSpriteCategory("ui_clan");
		_gauntletLayer = new GauntletLayer("KingdomScreen", 1, true);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		DataSource = CreateDataSource();
		DataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		DataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		DataSource.SetPreviousTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		DataSource.SetNextTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		if (_kingdomState.InitialSelectedDecision != null)
		{
			DataSource.Decision.HandleDecision(_kingdomState.InitialSelectedDecision);
		}
		else if (_kingdomState.InitialSelectedArmy != null)
		{
			DataSource.SelectArmy(_kingdomState.InitialSelectedArmy);
		}
		else if (_kingdomState.InitialSelectedSettlement != null)
		{
			DataSource.SelectSettlement(_kingdomState.InitialSelectedSettlement);
		}
		else if (_kingdomState.InitialSelectedClan != null)
		{
			DataSource.SelectClan(_kingdomState.InitialSelectedClan);
		}
		else if (_kingdomState.InitialSelectedPolicy != null)
		{
			DataSource.SelectPolicy(_kingdomState.InitialSelectedPolicy);
		}
		else if (_kingdomState.InitialSelectedKingdom != null)
		{
			DataSource.SelectKingdom(_kingdomState.InitialSelectedKingdom);
		}
		_gauntletLayer.LoadMovie("KingdomManagement", (ViewModel)(object)DataSource);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)7));
		UISoundsHelper.PlayUISound("event:/ui/panels/panel_kingdom_open");
		_gauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(2, (Func<bool>)null);
	}

	void IGameStateListener.OnDeactivate()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		((ScreenBase)this).OnDeactivate();
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
		if (MapScreen.Instance != null)
		{
			MapScreen.Instance.NavigationHandler.IsNavigationLocked = false;
		}
		_kingdomCategory.Unload();
		_clanCategory.Unload();
		((ViewModel)DataSource).OnFinalize();
		DataSource = null;
		_gauntletLayer = null;
	}

	protected void ShowArmyOnMap(Army army)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		CloseKingdomScreen();
		MapScreen.Instance.FastMoveCameraToPosition(army.LeaderParty.Position);
	}

	protected void OpenArmyManagement()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		if (_gauntletLayer != null)
		{
			_armyManagementDatasource = new ArmyManagementVM((Action)CloseArmyManagement);
			_armyManagementDatasource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
			_armyManagementDatasource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
			_armyManagementDatasource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
			_armyManagementDatasource.SetRemoveInputKey(HotKeyManager.GetCategory("ArmyManagementHotkeyCategory").GetHotKey("RemoveParty"));
			_armyManagementCategory = UIResourceManager.LoadSpriteCategory("ui_armymanagement");
			_armyManagementLayer = new GauntletLayer("Kingdom_ArmManagement", 2, false);
			_armyManagementLayer.LoadMovie("ArmyManagement", (ViewModel)(object)_armyManagementDatasource);
			((ScreenLayer)_armyManagementLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
			((ScreenLayer)_armyManagementLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
			((ScreenLayer)_armyManagementLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ArmyManagementHotkeyCategory"));
			((ScreenLayer)_armyManagementLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			((ScreenLayer)_armyManagementLayer).IsFocusLayer = true;
			((ScreenBase)this).AddLayer((ScreenLayer)(object)_armyManagementLayer);
			ScreenManager.TrySetFocus((ScreenLayer)(object)_armyManagementLayer);
		}
	}

	protected void CloseArmyManagement()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		if (_armyManagementLayer != null)
		{
			((ScreenLayer)_armyManagementLayer).InputRestrictions.ResetInputRestrictions();
			((ScreenLayer)_armyManagementLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_armyManagementLayer);
			((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_armyManagementLayer);
			_armyManagementLayer = null;
		}
		if (_armyManagementDatasource != null)
		{
			((ViewModel)_armyManagementDatasource).OnFinalize();
			_armyManagementDatasource = null;
		}
		if (_armyManagementCategory != null)
		{
			_armyManagementCategory.Unload();
			_armyManagementCategory = null;
		}
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)7));
		DataSource.OnRefresh();
	}

	protected void CloseKingdomScreen()
	{
		Game.Current.GameStateManager.PopState(0);
		UISoundsHelper.PlayUISound("event:/ui/default");
	}
}
