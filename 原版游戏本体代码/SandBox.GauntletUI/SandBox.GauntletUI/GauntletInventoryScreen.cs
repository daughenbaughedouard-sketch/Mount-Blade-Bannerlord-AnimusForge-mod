using System;
using System.Collections.ObjectModel;
using Helpers;
using SandBox.View;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI;

[GameStateScreen(typeof(InventoryState))]
public class GauntletInventoryScreen : ScreenBase, IInventoryStateHandler, IGameStateListener, IChangeableScreen
{
	private GauntletMovieIdentifier _gauntletMovie;

	private SPInventoryVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private bool _closed;

	private bool _openedFromMission;

	private SpriteCategory _inventoryCategory;

	public InventoryState InventoryState { get; private set; }

	protected override void OnFrameTick(float dt)
	{
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Invalid comparison between Unknown and I4
		((ScreenBase)this).OnFrameTick(dt);
		if (!_closed)
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		_dataSource.IsFiveStackModifierActive = ((ScreenLayer)_gauntletLayer).Input.IsHotKeyDown("FiveStackModifier");
		_dataSource.IsEntireStackModifierActive = ((ScreenLayer)_gauntletLayer).Input.IsHotKeyDown("EntireStackModifier");
		if (_dataSource.IsSearchAvailable && ((ScreenLayer)_gauntletLayer).IsFocusedOnInput())
		{
			return;
		}
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("SwitchAlternative") && _dataSource != null)
		{
			_dataSource.CompareNextItem();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit") || ((ScreenLayer)_gauntletLayer).Input.IsGameKeyReleased(38))
		{
			ExecuteCancel();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
		{
			ExecuteConfirm();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Reset"))
		{
			HandleResetInput();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("SwitchToPreviousTab"))
		{
			if (_dataSource.IsFocusedOnItemList && Input.IsGamepadActive)
			{
				if (_dataSource.CurrentFocusedItem != null && _dataSource.CurrentFocusedItem.IsTransferable && (int)_dataSource.CurrentFocusedItem.InventorySide == 0)
				{
					ExecuteBuySingle();
				}
			}
			else
			{
				ExecuteSwitchToPreviousTab();
			}
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("SwitchToNextTab"))
		{
			if (_dataSource.IsFocusedOnItemList && Input.IsGamepadActive)
			{
				if (_dataSource.CurrentFocusedItem != null && _dataSource.CurrentFocusedItem.IsTransferable && (int)_dataSource.CurrentFocusedItem.InventorySide == 1)
				{
					ExecuteSellSingle();
				}
			}
			else
			{
				ExecuteSwitchToNextTab();
			}
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("TakeAll"))
		{
			ExecuteTakeAll();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("GiveAll"))
		{
			ExecuteGiveAll();
		}
	}

	public GauntletInventoryScreen(InventoryState inventoryState)
	{
		InventoryState = inventoryState;
		InventoryState.Handler = (IInventoryStateHandler)(object)this;
	}

	protected unsafe override void OnInitialize()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		((ScreenBase)this).OnInitialize();
		_inventoryCategory = UIResourceManager.LoadSpriteCategory("ui_inventory");
		InventoryLogic inventoryLogic = InventoryState.InventoryLogic;
		Mission current = Mission.Current;
		_dataSource = new SPInventoryVM(inventoryLogic, current != null && current.DoesMissionRequireCivilianEquipment, (Func<WeaponComponentData, ItemUsageSetFlags>)GetItemUsageSetFlag);
		_dataSource.SetGetKeyTextFromKeyIDFunc(new Func<string, TextObject>(Game.Current.GameTextManager, (nint)(delegate*<GameTextManager, string, TextObject>)(&GameKeyTextExtensions.GetHotKeyGameTextFromKeyID)));
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetPreviousCharacterInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		_dataSource.SetNextCharacterInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		_dataSource.SetBuyAllInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("TakeAll"));
		_dataSource.SetSellAllInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("GiveAll"));
		_gauntletLayer = new GauntletLayer("InventoryScreen", 15, true)
		{
			IsFocusLayer = true
		};
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("InventoryHotKeyCategory"));
		_gauntletMovie = _gauntletLayer.LoadMovie("Inventory", (ViewModel)(object)_dataSource);
		_openedFromMission = ((GameState)InventoryState).Predecessor is MissionState;
		UISoundsHelper.PlayUISound("event:/ui/panels/panel_inventory_open");
		_gauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(2, (Func<bool>)null);
		InformationManager.HideAllMessages();
	}

	protected override void OnDeactivate()
	{
		((ScreenBase)this).OnDeactivate();
		_closed = true;
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		}
		MBInformationManager.HideInformations();
	}

	protected override void OnActivate()
	{
		((ScreenBase)this).OnActivate();
		SPInventoryVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.RefreshCallbacks();
		}
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
	}

	protected override void OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		_gauntletMovie = null;
		_inventoryCategory.Unload();
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_gauntletLayer = null;
	}

	void IGameStateListener.OnActivate()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)2));
	}

	void IGameStateListener.OnDeactivate()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}

	public void ExecuteLootingScript()
	{
		_dataSource.ExecuteBuyAllItems();
	}

	public void ExecuteSellAllLoot()
	{
		_dataSource.ExecuteSellAllItems();
	}

	private void HandleResetInput()
	{
		if (!_dataSource.ItemPreview.IsSelected)
		{
			_dataSource.ExecuteResetTranstactions();
			UISoundsHelper.PlayUISound("event:/ui/default");
		}
	}

	public void ExecuteCancel()
	{
		if (_dataSource.ItemPreview.IsSelected)
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ClosePreview();
		}
		else if (_dataSource.IsAnyEquippedItemSelected())
		{
			_dataSource.ExecuteClearSelectedItem();
		}
		else
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteResetAndCompleteTranstactions(true);
		}
	}

	public void ExecuteConfirm()
	{
		if (!_dataSource.ItemPreview.IsSelected && !_dataSource.IsDoneDisabled)
		{
			_dataSource.ExecuteCompleteTranstactions();
			UISoundsHelper.PlayUISound("event:/ui/default");
		}
	}

	public void ExecuteSwitchToPreviousTab()
	{
		if (!_dataSource.ItemPreview.IsSelected)
		{
			MBBindingList<InventoryCharacterSelectorItemVM> itemList = _dataSource.CharacterList.ItemList;
			if (itemList != null && ((Collection<InventoryCharacterSelectorItemVM>)(object)itemList).Count > 1)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
			_dataSource.CharacterList.ExecuteSelectPreviousItem();
		}
	}

	public void ExecuteSwitchToNextTab()
	{
		if (!_dataSource.ItemPreview.IsSelected)
		{
			MBBindingList<InventoryCharacterSelectorItemVM> itemList = _dataSource.CharacterList.ItemList;
			if (itemList != null && ((Collection<InventoryCharacterSelectorItemVM>)(object)itemList).Count > 1)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
			_dataSource.CharacterList.ExecuteSelectNextItem();
		}
	}

	public void ExecuteBuySingle()
	{
		_dataSource.CurrentFocusedItem.ExecuteBuySingle();
		UISoundsHelper.PlayUISound("event:/ui/transfer");
	}

	public void ExecuteSellSingle()
	{
		_dataSource.CurrentFocusedItem.ExecuteSellSingle();
		UISoundsHelper.PlayUISound("event:/ui/transfer");
	}

	public void ExecuteTakeAll()
	{
		if (!_dataSource.ItemPreview.IsSelected)
		{
			_dataSource.ExecuteBuyAllItems();
			UISoundsHelper.PlayUISound("event:/ui/inventory/take_all");
		}
	}

	public void ExecuteGiveAll()
	{
		if (!_dataSource.ItemPreview.IsSelected)
		{
			_dataSource.ExecuteSellAllItems();
			UISoundsHelper.PlayUISound("event:/ui/inventory/take_all");
		}
	}

	public void ExecuteBuyConsumableItem()
	{
		_dataSource.ExecuteBuyItemTest();
	}

	private ItemUsageSetFlags GetItemUsageSetFlag(WeaponComponentData item)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(item.ItemUsage))
		{
			return MBItem.GetItemUsageSetFlags(item.ItemUsage);
		}
		return (ItemUsageSetFlags)0;
	}

	private void CloseInventoryScreen()
	{
		InventoryScreenHelper.CloseScreen(false);
	}

	bool IChangeableScreen.AnyUnsavedChanges()
	{
		return InventoryState.InventoryLogic.IsThereAnyChanges();
	}

	bool IChangeableScreen.CanChangesBeApplied()
	{
		return InventoryState.InventoryLogic.CanPlayerCompleteTransaction();
	}

	void IChangeableScreen.ApplyChanges()
	{
		_dataSource.ItemPreview.Close();
		InventoryState.InventoryLogic.DoneLogic();
	}

	void IChangeableScreen.ResetChanges()
	{
		InventoryState.InventoryLogic.Reset(true);
	}
}
