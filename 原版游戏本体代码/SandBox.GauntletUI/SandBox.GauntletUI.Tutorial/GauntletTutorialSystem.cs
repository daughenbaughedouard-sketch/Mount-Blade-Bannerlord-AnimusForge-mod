using System;
using System.Collections.Generic;
using System.Reflection;
using SandBox.View.Map;
using SandBox.ViewModelCollection.MapSiege;
using SandBox.ViewModelCollection.Missions.NameMarker;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ArmyManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper.PerkSelection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Events;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Decisions;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Tutorial;

public class GauntletTutorialSystem : GlobalLayer
{
	public static GauntletTutorialSystem Current;

	private readonly Dictionary<string, TutorialItemBase> _mappedTutorialItems;

	private readonly Dictionary<TutorialItemBase, string> _tutorialItemIdentifiers;

	private CampaignTutorial _currentTutorial;

	private TutorialItemBase _currentTutorialVisualItem;

	private List<TutorialItemBase> _currentlyAvailableTutorialItems;

	private TutorialItemBase[] _currentlyAvailableTutorialItemsCopy;

	private TutorialVM _dataSource;

	private bool _isInitialized;

	private List<CampaignTutorial> _currentCampaignTutorials;

	private GauntletMovieIdentifier _movie;

	public EncyclopediaPages CurrentEncyclopediaPageContext { get; private set; }

	public bool IsCharacterPortraitPopupOpen { get; private set; }

	public TutorialContexts CurrentContext { get; private set; }

	public GauntletTutorialSystem()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		_isInitialized = true;
		_dataSource = new TutorialVM(DisableTutorialStep);
		((GlobalLayer)this).Layer = (ScreenLayer)new GauntletLayer("TutorialScreen", 15300, false);
		GauntletLayer val = (GauntletLayer)((GlobalLayer)this).Layer;
		_movie = val.LoadMovie("TutorialScreen", (ViewModel)(object)_dataSource);
		((ScreenLayer)val).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		ScreenManager.AddGlobalLayer((GlobalLayer)(object)this, true);
		_mappedTutorialItems = new Dictionary<string, TutorialItemBase>();
		_tutorialItemIdentifiers = new Dictionary<TutorialItemBase, string>();
		_currentlyAvailableTutorialItems = new List<TutorialItemBase>();
		_currentlyAvailableTutorialItemsCopy = new TutorialItemBase[0];
		RegisterEvents();
		RegisterTutorialTypes();
		UpdateKeytexts();
		_currentCampaignTutorials = new List<CampaignTutorial>();
	}

	protected override void OnTick(float dt)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		((GlobalLayer)this).OnTick(dt);
		if (!_isInitialized)
		{
			return;
		}
		if (_currentlyAvailableTutorialItemsCopy.Length != _currentlyAvailableTutorialItems.Capacity)
		{
			_currentlyAvailableTutorialItemsCopy = new TutorialItemBase[_currentlyAvailableTutorialItems.Capacity];
		}
		_currentlyAvailableTutorialItems.CopyTo(_currentlyAvailableTutorialItemsCopy);
		int count = _currentlyAvailableTutorialItems.Count;
		if (_currentTutorial == null)
		{
			_currentCampaignTutorials.Clear();
			_currentlyAvailableTutorialItems.Clear();
			if (CampaignEventDispatcher.Instance != null)
			{
				((CampaignEventReceiver)CampaignEventDispatcher.Instance).CollectAvailableTutorials(ref _currentCampaignTutorials);
				foreach (CampaignTutorial currentCampaignTutorial in _currentCampaignTutorials)
				{
					if (_mappedTutorialItems.TryGetValue(currentCampaignTutorial.TutorialTypeId, out var value))
					{
						if (value.GetTutorialsRelevantContext() == CurrentContext)
						{
							_currentlyAvailableTutorialItems.Add(value);
						}
						if (_currentTutorial == null && value.GetTutorialsRelevantContext() == CurrentContext && value.IsConditionsMetForActivation())
						{
							SetCurrentTutorial(currentCampaignTutorial, value);
						}
					}
				}
			}
		}
		for (int i = 0; i < count; i++)
		{
			if (_currentlyAvailableTutorialItems.IndexOf(_currentlyAvailableTutorialItemsCopy[i]) < 0)
			{
				_currentlyAvailableTutorialItemsCopy[i].OnDeactivate();
			}
		}
		if (_currentlyAvailableTutorialItemsCopy.Length != _currentlyAvailableTutorialItems.Capacity)
		{
			_currentlyAvailableTutorialItemsCopy = new TutorialItemBase[_currentlyAvailableTutorialItems.Capacity];
		}
		else
		{
			_currentlyAvailableTutorialItemsCopy.Initialize();
		}
		_currentlyAvailableTutorialItems.CopyTo(_currentlyAvailableTutorialItemsCopy);
		for (int j = 0; j < _currentlyAvailableTutorialItems.Count; j++)
		{
			TutorialItemBase tutorialItemBase = _currentlyAvailableTutorialItemsCopy[j];
			if (!tutorialItemBase.IsConditionsMetForCompletion())
			{
				continue;
			}
			string text = _tutorialItemIdentifiers[tutorialItemBase];
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnTutorialCompleted(text);
			_currentlyAvailableTutorialItems.Remove(tutorialItemBase);
			if (Mission.Current != null)
			{
				List<MissionBehavior> missionBehaviors = Mission.Current.MissionBehaviors;
				if (missionBehaviors != null)
				{
					for (int k = 0; k < missionBehaviors.Count; k++)
					{
						MissionBehavior val = missionBehaviors[k];
						if (val != null)
						{
							val.OnTutorialCompleted(text);
						}
					}
				}
			}
			if (tutorialItemBase == _currentTutorialVisualItem)
			{
				ResetCurrentTutorial();
			}
			else
			{
				Debug.Print("Completed a non-active tutorial: " + text, 0, (DebugColor)12, 17592186044416uL);
			}
		}
		_currentlyAvailableTutorialItemsCopy.Initialize();
		TutorialItemBase currentTutorialVisualItem = _currentTutorialVisualItem;
		if ((currentTutorialVisualItem != null && !currentTutorialVisualItem.IsConditionsMetForActivation()) || _currentTutorialVisualItem?.GetTutorialsRelevantContext() != (TutorialContexts?)CurrentContext)
		{
			ResetCurrentTutorial();
		}
		if (_currentTutorialVisualItem != null && _currentlyAvailableTutorialItems.IndexOf(_currentTutorialVisualItem) < 0)
		{
			ResetCurrentTutorial();
		}
		_dataSource.IsVisible = _currentTutorialVisualItem?.IsConditionsMetForVisibility() ?? false;
		_dataSource.Tick(dt);
	}

	private void SetCurrentTutorial(CampaignTutorial tutorial, TutorialItemBase tutorialItem)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		_currentTutorial = tutorial;
		_currentTutorialVisualItem = tutorialItem;
		Game.Current.EventManager.TriggerEvent<TutorialNotificationElementChangeEvent>(new TutorialNotificationElementChangeEvent(_currentTutorialVisualItem.HighlightedVisualElementID));
		_dataSource.SetCurrentTutorial(tutorialItem.Placement, tutorial.TutorialTypeId, tutorialItem.MouseRequired);
		if (tutorialItem.MouseRequired)
		{
			((GlobalLayer)this).Layer.InputRestrictions.SetInputRestrictions(false, (InputUsageMask)1);
		}
	}

	private void ResetCurrentTutorial()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		_currentTutorial = null;
		_currentTutorialVisualItem = null;
		_dataSource?.CloseTutorialStep();
		Game.Current.EventManager.TriggerEvent<TutorialNotificationElementChangeEvent>(new TutorialNotificationElementChangeEvent(string.Empty));
		ScreenLayer layer = ((GlobalLayer)this).Layer;
		if (layer != null)
		{
			layer.InputRestrictions.ResetInputRestrictions();
		}
	}

	private void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		CurrentContext = obj.NewContext;
		IsCharacterPortraitPopupOpen = false;
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnTutorialContextChanged(obj);
		});
	}

	private void DisableTutorialStep()
	{
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnTutorialCompleted(_currentTutorial.TutorialTypeId);
		ResetCurrentTutorial();
	}

	public static void OnInitialize()
	{
		if (Current == null)
		{
			Current = new GauntletTutorialSystem();
		}
		_ = Current._isInitialized;
	}

	public static void OnUnload()
	{
		if (Current != null)
		{
			if (Current._isInitialized)
			{
				Current.UnregisterEvents();
				Current._isInitialized = false;
				TutorialVM.Instance = null;
				Current._dataSource = null;
				ScreenManager.RemoveGlobalLayer((GlobalLayer)(object)Current);
				ScreenLayer layer = ((GlobalLayer)Current).Layer;
				((GauntletLayer)((layer is GauntletLayer) ? layer : null)).ReleaseMovie(Current._movie);
			}
			Current = null;
		}
	}

	private void OnEncyclopediaPageChanged(EncyclopediaPageChangedEvent obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		CurrentEncyclopediaPageContext = obj.NewPage;
	}

	private void OnPerkSelectionToggle(PerkSelectionToggleEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPerkSelectionToggle(obj);
		});
	}

	private void OnInventoryTransferItem(InventoryTransferItemEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnInventoryTransferItem(obj);
		});
	}

	private void OnInventoryEquipmentTypeChange(InventoryEquipmentTypeChangedEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnInventoryEquipmentTypeChange(obj);
		});
	}

	private void OnFocusAddedByPlayer(FocusAddedByPlayerEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnFocusAddedByPlayer(obj);
		});
	}

	private void OnPerkSelectedByPlayer(PerkSelectedByPlayerEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPerkSelectedByPlayer(obj);
		});
	}

	private void OnPartyAddedToArmyByPlayer(PartyAddedToArmyByPlayerEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPartyAddedToArmyByPlayer(obj);
		});
	}

	private void OnArmyCohesionByPlayerBoosted(ArmyCohesionBoostedByPlayerEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnArmyCohesionByPlayerBoosted(obj);
		});
	}

	private void OnInventoryFilterChanged(InventoryFilterChangedEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnInventoryFilterChanged(obj);
		});
	}

	private void OnPlayerToggleTrackSettlementFromEncyclopedia(PlayerToggleTrackSettlementFromEncyclopediaEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerToggleTrackSettlementFromEncyclopedia(obj);
		});
	}

	private void OnMissionNameMarkerToggled(MissionNameMarkerToggleEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnMissionNameMarkerToggled(obj);
		});
	}

	private void OnPlayerStartEngineConstruction(PlayerStartEngineConstructionEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerStartEngineConstruction(obj);
		});
	}

	private void OnPlayerInspectedPartySpeed(PlayerInspectedPartySpeedEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerInspectedPartySpeed(obj);
		});
	}

	private void OnGameMenuOpened(MenuCallbackArgs obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnGameMenuOpened(obj);
		});
	}

	private void OnCharacterPortraitPopUpOpened(CharacterObject obj)
	{
		IsCharacterPortraitPopupOpen = true;
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnCharacterPortraitPopUpOpened(obj);
		});
	}

	private void OnCharacterPortraitPopUpClosed()
	{
		IsCharacterPortraitPopupOpen = false;
	}

	private void OnPlayerStartTalkFromMenuOverlay(Hero obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerStartTalkFromMenuOverlay(obj);
		});
	}

	private void OnGameMenuOptionSelected(GameMenu gameMenu, GameMenuOption gameMenuOption)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnGameMenuOptionSelected(gameMenuOption);
		});
	}

	private void OnPlayerStartRecruitment(CharacterObject obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerStartRecruitment(obj);
		});
	}

	private void OnNewCompanionAdded(Hero obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnNewCompanionAdded(obj);
		});
	}

	private void OnPlayerRecruitUnit(CharacterObject obj, int count)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerRecruitedUnit(obj, count);
		});
	}

	private void OnPlayerInventoryExchange(List<(ItemRosterElement, int)> purchasedItems, List<(ItemRosterElement, int)> soldItems, bool isTrading)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerInventoryExchange(purchasedItems, soldItems, isTrading);
		});
	}

	private void OnPlayerUpgradeTroop(PlayerRequestUpgradeTroopEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerUpgradeTroop(obj.SourceTroop, obj.TargetTroop, obj.Number);
		});
	}

	private void OnPlayerMoveTroop(PlayerMoveTroopEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerMoveTroop(obj);
		});
	}

	private void OnPlayerToggledUpgradePopup(PlayerToggledUpgradePopupEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerToggledUpgradePopup(obj);
		});
	}

	private void OnOrderOfBattleHeroAssignedToFormation(OrderOfBattleHeroAssignedToFormationEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnOrderOfBattleHeroAssignedToFormation(obj);
		});
	}

	private void OnPlayerMovementFlagsChanged(MissionPlayerMovementFlagsChangeEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerMovementFlagChanged(obj);
		});
	}

	private void OnOrderOfBattleFormationClassChanged(OrderOfBattleFormationClassChangedEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnOrderOfBattleFormationClassChanged(obj);
		});
	}

	private void OnOrderOfBattleFormationWeightChanged(OrderOfBattleFormationWeightChangedEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnOrderOfBattleFormationWeightChanged(obj);
		});
	}

	private void OnCraftingWeaponClassSelectionOpened(CraftingWeaponClassSelectionOpenedEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnCraftingWeaponClassSelectionOpened(obj);
		});
	}

	private void OnCraftingOnWeaponResultPopupOpened(CraftingWeaponResultPopupToggledEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnCraftingOnWeaponResultPopupOpened(obj);
		});
	}

	private void OnCraftingOrderTabOpened(CraftingOrderTabOpenedEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnCraftingOrderTabOpened(obj);
		});
	}

	private void OnCraftingOrderSelectionOpened(CraftingOrderSelectionOpenedEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnCraftingOrderSelectionOpened(obj);
		});
	}

	private void OnInventoryItemInspected(InventoryItemInspectedEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnInventoryItemInspected(obj);
		});
	}

	private void OnCrimeValueInspectedInSettlementOverlay(CrimeValueInspectedInSettlementOverlayEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnCrimeValueInspectedInSettlementOverlay(obj);
		});
	}

	private void OnClanRoleAssignedThroughClanScreen(ClanRoleAssignedThroughClanScreenEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnClanRoleAssignedThroughClanScreen(obj);
		});
	}

	private void OnMainMapCameraMove(MapScreen.MainMapCameraMoveEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnMainMapCameraMove(obj);
		});
	}

	private void OnPlayerSelectedAKingdomDecisionOption(PlayerSelectedAKingdomDecisionOptionEvent obj)
	{
		_currentlyAvailableTutorialItems.ForEach(delegate(TutorialItemBase t)
		{
			t.OnPlayerSelectedAKingdomDecisionOption(obj);
		});
	}

	private void OnResetAllTutorials(ResetAllTutorialsEvent obj)
	{
		_mappedTutorialItems.Clear();
		_tutorialItemIdentifiers.Clear();
		RegisterTutorialTypes();
	}

	private void OnGamepadActiveStateChanged()
	{
		UpdateKeytexts();
	}

	private void OnKeybindsChanged()
	{
		UpdateKeytexts();
	}

	private void RegisterTutorialTypes()
	{
		foreach (Assembly item in (List<Assembly>)(object)ModuleHelper.GetActiveGameAssemblies())
		{
			Type[] types = item.GetTypes();
			foreach (Type type in types)
			{
				if (!typeof(TutorialItemBase).IsAssignableFrom(type) || type.IsAbstract)
				{
					continue;
				}
				TutorialAttribute customAttribute = type.GetCustomAttribute<TutorialAttribute>();
				if (customAttribute == null)
				{
					Debug.FailedAssert("Tutorial: " + type.Name + " does not have a Tutorial attribute", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Tutorial\\GauntletTutorialSystem.cs", "RegisterTutorialTypes", 508);
					continue;
				}
				ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
				if (constructor == null)
				{
					Debug.FailedAssert("Tutorial: " + type.Name + " does not have a parameterless constructor", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Tutorial\\GauntletTutorialSystem.cs", "RegisterTutorialTypes", 516);
					continue;
				}
				TutorialItemBase tutorialItemBase = (TutorialItemBase)constructor.Invoke(new object[0]);
				string tutorialIdentifier = customAttribute.TutorialIdentifier;
				if (string.IsNullOrEmpty(tutorialIdentifier))
				{
					Debug.FailedAssert("Tutorial: " + type.Name + " does not have a valid identifier", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Tutorial\\GauntletTutorialSystem.cs", "RegisterTutorialTypes", 526);
					continue;
				}
				_mappedTutorialItems[tutorialIdentifier] = tutorialItemBase;
				_tutorialItemIdentifiers[tutorialItemBase] = tutorialIdentifier;
			}
		}
	}

	private void RegisterEvents()
	{
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Expected O, but got Unknown
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
		Game.Current.EventManager.RegisterEvent<InventoryTransferItemEvent>((Action<InventoryTransferItemEvent>)OnInventoryTransferItem);
		Game.Current.EventManager.RegisterEvent<InventoryEquipmentTypeChangedEvent>((Action<InventoryEquipmentTypeChangedEvent>)OnInventoryEquipmentTypeChange);
		Game.Current.EventManager.RegisterEvent<FocusAddedByPlayerEvent>((Action<FocusAddedByPlayerEvent>)OnFocusAddedByPlayer);
		Game.Current.EventManager.RegisterEvent<PerkSelectedByPlayerEvent>((Action<PerkSelectedByPlayerEvent>)OnPerkSelectedByPlayer);
		Game.Current.EventManager.RegisterEvent<ArmyCohesionBoostedByPlayerEvent>((Action<ArmyCohesionBoostedByPlayerEvent>)OnArmyCohesionByPlayerBoosted);
		Game.Current.EventManager.RegisterEvent<PartyAddedToArmyByPlayerEvent>((Action<PartyAddedToArmyByPlayerEvent>)OnPartyAddedToArmyByPlayer);
		Game.Current.EventManager.RegisterEvent<InventoryFilterChangedEvent>((Action<InventoryFilterChangedEvent>)OnInventoryFilterChanged);
		Game.Current.EventManager.RegisterEvent<EncyclopediaPageChangedEvent>((Action<EncyclopediaPageChangedEvent>)OnEncyclopediaPageChanged);
		Game.Current.EventManager.RegisterEvent<PerkSelectionToggleEvent>((Action<PerkSelectionToggleEvent>)OnPerkSelectionToggle);
		Game.Current.EventManager.RegisterEvent<PlayerToggleTrackSettlementFromEncyclopediaEvent>((Action<PlayerToggleTrackSettlementFromEncyclopediaEvent>)OnPlayerToggleTrackSettlementFromEncyclopedia);
		Game.Current.EventManager.RegisterEvent<TutorialContextChangedEvent>((Action<TutorialContextChangedEvent>)OnTutorialContextChanged);
		Game.Current.EventManager.RegisterEvent<MissionNameMarkerToggleEvent>((Action<MissionNameMarkerToggleEvent>)OnMissionNameMarkerToggled);
		Game.Current.EventManager.RegisterEvent<PlayerRequestUpgradeTroopEvent>((Action<PlayerRequestUpgradeTroopEvent>)OnPlayerUpgradeTroop);
		Game.Current.EventManager.RegisterEvent<PlayerStartEngineConstructionEvent>((Action<PlayerStartEngineConstructionEvent>)OnPlayerStartEngineConstruction);
		Game.Current.EventManager.RegisterEvent<PlayerInspectedPartySpeedEvent>((Action<PlayerInspectedPartySpeedEvent>)OnPlayerInspectedPartySpeed);
		Game.Current.EventManager.RegisterEvent<MapScreen.MainMapCameraMoveEvent>((Action<MapScreen.MainMapCameraMoveEvent>)OnMainMapCameraMove);
		Game.Current.EventManager.RegisterEvent<PlayerMoveTroopEvent>((Action<PlayerMoveTroopEvent>)OnPlayerMoveTroop);
		Game.Current.EventManager.RegisterEvent<MissionPlayerMovementFlagsChangeEvent>((Action<MissionPlayerMovementFlagsChangeEvent>)OnPlayerMovementFlagsChanged);
		Game.Current.EventManager.RegisterEvent<ResetAllTutorialsEvent>((Action<ResetAllTutorialsEvent>)OnResetAllTutorials);
		Game.Current.EventManager.RegisterEvent<PlayerToggledUpgradePopupEvent>((Action<PlayerToggledUpgradePopupEvent>)OnPlayerToggledUpgradePopup);
		Game.Current.EventManager.RegisterEvent<OrderOfBattleHeroAssignedToFormationEvent>((Action<OrderOfBattleHeroAssignedToFormationEvent>)OnOrderOfBattleHeroAssignedToFormation);
		Game.Current.EventManager.RegisterEvent<OrderOfBattleFormationClassChangedEvent>((Action<OrderOfBattleFormationClassChangedEvent>)OnOrderOfBattleFormationClassChanged);
		Game.Current.EventManager.RegisterEvent<OrderOfBattleFormationWeightChangedEvent>((Action<OrderOfBattleFormationWeightChangedEvent>)OnOrderOfBattleFormationWeightChanged);
		Game.Current.EventManager.RegisterEvent<CraftingWeaponClassSelectionOpenedEvent>((Action<CraftingWeaponClassSelectionOpenedEvent>)OnCraftingWeaponClassSelectionOpened);
		Game.Current.EventManager.RegisterEvent<CraftingOrderTabOpenedEvent>((Action<CraftingOrderTabOpenedEvent>)OnCraftingOrderTabOpened);
		Game.Current.EventManager.RegisterEvent<CraftingOrderSelectionOpenedEvent>((Action<CraftingOrderSelectionOpenedEvent>)OnCraftingOrderSelectionOpened);
		Game.Current.EventManager.RegisterEvent<CraftingWeaponResultPopupToggledEvent>((Action<CraftingWeaponResultPopupToggledEvent>)OnCraftingOnWeaponResultPopupOpened);
		Game.Current.EventManager.RegisterEvent<InventoryItemInspectedEvent>((Action<InventoryItemInspectedEvent>)OnInventoryItemInspected);
		Game.Current.EventManager.RegisterEvent<CrimeValueInspectedInSettlementOverlayEvent>((Action<CrimeValueInspectedInSettlementOverlayEvent>)OnCrimeValueInspectedInSettlementOverlay);
		Game.Current.EventManager.RegisterEvent<ClanRoleAssignedThroughClanScreenEvent>((Action<ClanRoleAssignedThroughClanScreenEvent>)OnClanRoleAssignedThroughClanScreen);
		Game.Current.EventManager.RegisterEvent<PlayerSelectedAKingdomDecisionOptionEvent>((Action<PlayerSelectedAKingdomDecisionOptionEvent>)OnPlayerSelectedAKingdomDecisionOption);
		HotKeyManager.OnKeybindsChanged += new OnKeybindsChangedEvent(OnKeybindsChanged);
		if (Campaign.Current != null && CampaignEventDispatcher.Instance != null)
		{
			CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
			CampaignEvents.CharacterPortraitPopUpOpenedEvent.AddNonSerializedListener((object)this, (Action<CharacterObject>)OnCharacterPortraitPopUpOpened);
			CampaignEvents.CharacterPortraitPopUpClosedEvent.AddNonSerializedListener((object)this, (Action)OnCharacterPortraitPopUpClosed);
			CampaignEvents.PlayerStartTalkFromMenu.AddNonSerializedListener((object)this, (Action<Hero>)OnPlayerStartTalkFromMenuOverlay);
			CampaignEvents.GameMenuOptionSelectedEvent.AddNonSerializedListener((object)this, (Action<GameMenu, GameMenuOption>)OnGameMenuOptionSelected);
			CampaignEvents.PlayerStartRecruitmentEvent.AddNonSerializedListener((object)this, (Action<CharacterObject>)OnPlayerStartRecruitment);
			CampaignEvents.NewCompanionAdded.AddNonSerializedListener((object)this, (Action<Hero>)OnNewCompanionAdded);
			CampaignEvents.OnUnitRecruitedEvent.AddNonSerializedListener((object)this, (Action<CharacterObject, int>)OnPlayerRecruitUnit);
			CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener((object)this, (Action<List<(ItemRosterElement, int)>, List<(ItemRosterElement, int)>, bool>)OnPlayerInventoryExchange);
		}
	}

	private void UnregisterEvents()
	{
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Expected O, but got Unknown
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Remove(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
		Game current = Game.Current;
		if (current != null)
		{
			current.EventManager.UnregisterEvent<InventoryTransferItemEvent>((Action<InventoryTransferItemEvent>)OnInventoryTransferItem);
		}
		Game current2 = Game.Current;
		if (current2 != null)
		{
			current2.EventManager.UnregisterEvent<InventoryEquipmentTypeChangedEvent>((Action<InventoryEquipmentTypeChangedEvent>)OnInventoryEquipmentTypeChange);
		}
		Game current3 = Game.Current;
		if (current3 != null)
		{
			current3.EventManager.UnregisterEvent<FocusAddedByPlayerEvent>((Action<FocusAddedByPlayerEvent>)OnFocusAddedByPlayer);
		}
		Game current4 = Game.Current;
		if (current4 != null)
		{
			current4.EventManager.UnregisterEvent<PerkSelectedByPlayerEvent>((Action<PerkSelectedByPlayerEvent>)OnPerkSelectedByPlayer);
		}
		Game current5 = Game.Current;
		if (current5 != null)
		{
			current5.EventManager.UnregisterEvent<ArmyCohesionBoostedByPlayerEvent>((Action<ArmyCohesionBoostedByPlayerEvent>)OnArmyCohesionByPlayerBoosted);
		}
		Game current6 = Game.Current;
		if (current6 != null)
		{
			current6.EventManager.UnregisterEvent<PartyAddedToArmyByPlayerEvent>((Action<PartyAddedToArmyByPlayerEvent>)OnPartyAddedToArmyByPlayer);
		}
		Game current7 = Game.Current;
		if (current7 != null)
		{
			current7.EventManager.UnregisterEvent<InventoryFilterChangedEvent>((Action<InventoryFilterChangedEvent>)OnInventoryFilterChanged);
		}
		Game current8 = Game.Current;
		if (current8 != null)
		{
			current8.EventManager.UnregisterEvent<EncyclopediaPageChangedEvent>((Action<EncyclopediaPageChangedEvent>)OnEncyclopediaPageChanged);
		}
		Game current9 = Game.Current;
		if (current9 != null)
		{
			current9.EventManager.UnregisterEvent<PerkSelectionToggleEvent>((Action<PerkSelectionToggleEvent>)OnPerkSelectionToggle);
		}
		Game current10 = Game.Current;
		if (current10 != null)
		{
			current10.EventManager.UnregisterEvent<PlayerToggleTrackSettlementFromEncyclopediaEvent>((Action<PlayerToggleTrackSettlementFromEncyclopediaEvent>)OnPlayerToggleTrackSettlementFromEncyclopedia);
		}
		Game current11 = Game.Current;
		if (current11 != null)
		{
			current11.EventManager.UnregisterEvent<TutorialContextChangedEvent>((Action<TutorialContextChangedEvent>)OnTutorialContextChanged);
		}
		Game current12 = Game.Current;
		if (current12 != null)
		{
			current12.EventManager.UnregisterEvent<MissionNameMarkerToggleEvent>((Action<MissionNameMarkerToggleEvent>)OnMissionNameMarkerToggled);
		}
		Game current13 = Game.Current;
		if (current13 != null)
		{
			current13.EventManager.UnregisterEvent<PlayerRequestUpgradeTroopEvent>((Action<PlayerRequestUpgradeTroopEvent>)OnPlayerUpgradeTroop);
		}
		Game current14 = Game.Current;
		if (current14 != null)
		{
			current14.EventManager.UnregisterEvent<PlayerStartEngineConstructionEvent>((Action<PlayerStartEngineConstructionEvent>)OnPlayerStartEngineConstruction);
		}
		Game current15 = Game.Current;
		if (current15 != null)
		{
			current15.EventManager.UnregisterEvent<PlayerInspectedPartySpeedEvent>((Action<PlayerInspectedPartySpeedEvent>)OnPlayerInspectedPartySpeed);
		}
		Game current16 = Game.Current;
		if (current16 != null)
		{
			current16.EventManager.UnregisterEvent<MapScreen.MainMapCameraMoveEvent>((Action<MapScreen.MainMapCameraMoveEvent>)OnMainMapCameraMove);
		}
		Game current17 = Game.Current;
		if (current17 != null)
		{
			current17.EventManager.UnregisterEvent<PlayerMoveTroopEvent>((Action<PlayerMoveTroopEvent>)OnPlayerMoveTroop);
		}
		Game current18 = Game.Current;
		if (current18 != null)
		{
			current18.EventManager.UnregisterEvent<MissionPlayerMovementFlagsChangeEvent>((Action<MissionPlayerMovementFlagsChangeEvent>)OnPlayerMovementFlagsChanged);
		}
		Game current19 = Game.Current;
		if (current19 != null)
		{
			current19.EventManager.UnregisterEvent<ResetAllTutorialsEvent>((Action<ResetAllTutorialsEvent>)OnResetAllTutorials);
		}
		Game current20 = Game.Current;
		if (current20 != null)
		{
			current20.EventManager.UnregisterEvent<PlayerToggledUpgradePopupEvent>((Action<PlayerToggledUpgradePopupEvent>)OnPlayerToggledUpgradePopup);
		}
		Game current21 = Game.Current;
		if (current21 != null)
		{
			current21.EventManager.UnregisterEvent<OrderOfBattleHeroAssignedToFormationEvent>((Action<OrderOfBattleHeroAssignedToFormationEvent>)OnOrderOfBattleHeroAssignedToFormation);
		}
		Game.Current.EventManager.UnregisterEvent<OrderOfBattleFormationClassChangedEvent>((Action<OrderOfBattleFormationClassChangedEvent>)OnOrderOfBattleFormationClassChanged);
		Game.Current.EventManager.UnregisterEvent<OrderOfBattleFormationWeightChangedEvent>((Action<OrderOfBattleFormationWeightChangedEvent>)OnOrderOfBattleFormationWeightChanged);
		Game.Current.EventManager.UnregisterEvent<CraftingWeaponClassSelectionOpenedEvent>((Action<CraftingWeaponClassSelectionOpenedEvent>)OnCraftingWeaponClassSelectionOpened);
		Game.Current.EventManager.UnregisterEvent<CraftingWeaponResultPopupToggledEvent>((Action<CraftingWeaponResultPopupToggledEvent>)OnCraftingOnWeaponResultPopupOpened);
		Game.Current.EventManager.UnregisterEvent<CraftingOrderTabOpenedEvent>((Action<CraftingOrderTabOpenedEvent>)OnCraftingOrderTabOpened);
		Game.Current.EventManager.UnregisterEvent<CraftingOrderSelectionOpenedEvent>((Action<CraftingOrderSelectionOpenedEvent>)OnCraftingOrderSelectionOpened);
		Game.Current.EventManager.UnregisterEvent<InventoryItemInspectedEvent>((Action<InventoryItemInspectedEvent>)OnInventoryItemInspected);
		Game.Current.EventManager.UnregisterEvent<CrimeValueInspectedInSettlementOverlayEvent>((Action<CrimeValueInspectedInSettlementOverlayEvent>)OnCrimeValueInspectedInSettlementOverlay);
		Game.Current.EventManager.UnregisterEvent<ClanRoleAssignedThroughClanScreenEvent>((Action<ClanRoleAssignedThroughClanScreenEvent>)OnClanRoleAssignedThroughClanScreen);
		Game.Current.EventManager.UnregisterEvent<PlayerSelectedAKingdomDecisionOptionEvent>((Action<PlayerSelectedAKingdomDecisionOptionEvent>)OnPlayerSelectedAKingdomDecisionOption);
		HotKeyManager.OnKeybindsChanged -= new OnKeybindsChangedEvent(OnKeybindsChanged);
		if (Campaign.Current != null && CampaignEventDispatcher.Instance != null)
		{
			((IMbEventBase)CampaignEvents.GameMenuOpened).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.CharacterPortraitPopUpOpenedEvent).ClearListeners((object)this);
			CampaignEvents.CharacterPortraitPopUpClosedEvent.ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.PlayerStartTalkFromMenu).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.GameMenuOptionSelectedEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.PlayerStartRecruitmentEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.NewCompanionAdded).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.OnUnitRecruitedEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.PlayerInventoryExchangeEvent).ClearListeners((object)this);
		}
	}

	private void UpdateKeytexts()
	{
		string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 5), 1f);
		GameTexts.SetVariable("MISSION_INDICATORS_KEY", keyHyperlinkText);
		string keyHyperlinkText2 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 4), 1f);
		GameTexts.SetVariable("LEAVE_MISSION_KEY", keyHyperlinkText2);
		string keyHyperlinkText3 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 87), 1f);
		GameTexts.SetVariable("HOLD_OPEN_ORDER_KEY", keyHyperlinkText3);
		string keyHyperlinkText4 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 69), 1f);
		GameTexts.SetVariable("FIRST_ORDER_CATEGORY_KEY", keyHyperlinkText4);
		string keyHyperlinkText5 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 70), 1f);
		GameTexts.SetVariable("SECOND_ORDER_CATEGORY_KEY", keyHyperlinkText5);
		string keyHyperlinkText6 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 71), 1f);
		GameTexts.SetVariable("THIRD_ORDER_CATEGORY_KEY", keyHyperlinkText6);
		string keyHyperlinkText7 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 72), 1f);
		GameTexts.SetVariable("FOURTH_ORDER_CATEGORY_KEY", keyHyperlinkText7);
		string keyHyperlinkText8 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 79), 1f);
		GameTexts.SetVariable("FIRST_GROUP_HEAR_KEY", keyHyperlinkText8);
		string keyHyperlinkText9 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 80), 1f);
		GameTexts.SetVariable("SECOND_GROUP_HEAR_KEY", keyHyperlinkText9);
		string keyHyperlinkText10 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 88), 1f);
		GameTexts.SetVariable("SELECT_LEFT_FORMATION_KEY", keyHyperlinkText10);
		string keyHyperlinkText11 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 89), 1f);
		GameTexts.SetVariable("SELECT_RIGHT_FORMATION_KEY", keyHyperlinkText11);
		string keyHyperlinkText12 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 0), 1f);
		GameTexts.SetVariable("FORWARD_KEY", keyHyperlinkText12);
		string keyHyperlinkText13 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 1), 1f);
		GameTexts.SetVariable("BACKWARDS_KEY", keyHyperlinkText13);
		string keyHyperlinkText14 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 2), 1f);
		GameTexts.SetVariable("LEFT_KEY", keyHyperlinkText14);
		string keyHyperlinkText15 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 3), 1f);
		GameTexts.SetVariable("RIGHT_KEY", keyHyperlinkText15);
		string keyHyperlinkText16 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f);
		GameTexts.SetVariable("INTERACTION_KEY", keyHyperlinkText16);
		string keyHyperlinkText17 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MapHotKeyCategory", 57), 1f);
		GameTexts.SetVariable("MAP_ZOOM_OUT_KEY", keyHyperlinkText17);
		string keyHyperlinkText18 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MapHotKeyCategory", 56), 1f);
		GameTexts.SetVariable("MAP_ZOOM_IN_KEY", keyHyperlinkText18);
		string keyHyperlinkText19 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MapHotKeyCategory", "MapClick"), 1f);
		GameTexts.SetVariable("CONSOLE_ACTION_KEY", keyHyperlinkText19);
		string keyHyperlinkText20 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 30), 1f);
		GameTexts.SetVariable("WALK_MODE_KEY", keyHyperlinkText20);
		string keyHyperlinkText21 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 15), 1f);
		GameTexts.SetVariable("CROUCH_KEY", keyHyperlinkText21);
		string keyHyperlinkText22 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("MissionOrderHotkeyCategory", 90), 1f);
		GameTexts.SetVariable("APPLY_SELECTION_KEY", keyHyperlinkText22);
		GameTexts.SetVariable("CONSOLE_MOVEMENT_KEY", HyperlinkTexts.GetKeyHyperlinkText("ControllerLStick", 1f));
		GameTexts.SetVariable("CONSOLE_CAMERA_KEY", HyperlinkTexts.GetKeyHyperlinkText("ControllerRStick", 1f));
		GameTexts.SetVariable("UPGRADE_ICON", "{=!}<img src=\"PartyScreen\\upgrade_icon\" extend=\"5\">");
	}
}
