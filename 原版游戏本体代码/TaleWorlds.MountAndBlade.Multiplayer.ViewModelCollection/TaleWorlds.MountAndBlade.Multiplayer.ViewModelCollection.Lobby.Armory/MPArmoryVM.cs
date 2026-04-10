using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.ClassFilter;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory;

public class MPArmoryVM : ViewModel
{
	private readonly Action<BasicCharacterObject> _onOpenFacegen;

	private bool _canOpenFacegenBeforeTauntState;

	private BasicCharacterObject _character;

	private MPLobbyClassFilterClassItemVM _currentClassItem;

	private Equipment _lastValidEquipment;

	private Func<string> _getExitText;

	private MPArmoryCosmeticTauntItemVM _tauntItemToRefreshNextAnimationWith;

	private MPArmoryCosmeticTauntItemVM _currentTauntPreviewAnimationSource;

	private bool _isEnabled;

	private bool _isManagingTaunts;

	private bool _isTauntAssignmentActive;

	private bool _canOpenFacegen;

	private MPLobbyClassFilterVM _classFilter;

	private MPArmoryHeroPreviewVM _heroPreview;

	private MPArmoryClassStatsVM _classStats;

	private MPArmoryHeroPerkSelectionVM _heroPerkSelection;

	private MPArmoryCosmeticsVM _cosmetics;

	private string _tauntAssignmentClickToCloseText;

	private string _statsText;

	private string _customizationText;

	private string _facegenText;

	private string _manageTauntsText;

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEnabled");
				OnIsEnabledChanged();
			}
		}
	}

	[DataSourceProperty]
	public bool IsManagingTaunts
	{
		get
		{
			return _isManagingTaunts;
		}
		set
		{
			if (value != _isManagingTaunts)
			{
				_isManagingTaunts = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsManagingTaunts");
				Cosmetics.IsManagingTaunts = value;
			}
		}
	}

	[DataSourceProperty]
	public bool IsTauntAssignmentActive
	{
		get
		{
			return _isTauntAssignmentActive;
		}
		set
		{
			if (value != _isTauntAssignmentActive)
			{
				_isTauntAssignmentActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTauntAssignmentActive");
				if (_isTauntAssignmentActive)
				{
					TauntAssignmentClickToCloseText = _getExitText?.Invoke();
				}
			}
		}
	}

	[DataSourceProperty]
	public bool CanOpenFacegen
	{
		get
		{
			return _canOpenFacegen;
		}
		set
		{
			if (value != _canOpenFacegen)
			{
				_canOpenFacegen = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanOpenFacegen");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyClassFilterVM ClassFilter
	{
		get
		{
			return _classFilter;
		}
		set
		{
			if (value != _classFilter)
			{
				_classFilter = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyClassFilterVM>(value, "ClassFilter");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryHeroPreviewVM HeroPreview
	{
		get
		{
			return _heroPreview;
		}
		set
		{
			if (value != _heroPreview)
			{
				_heroPreview = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryHeroPreviewVM>(value, "HeroPreview");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryClassStatsVM ClassStats
	{
		get
		{
			return _classStats;
		}
		set
		{
			if (value != _classStats)
			{
				_classStats = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryClassStatsVM>(value, "ClassStats");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryHeroPerkSelectionVM HeroPerkSelection
	{
		get
		{
			return _heroPerkSelection;
		}
		set
		{
			if (value != _heroPerkSelection)
			{
				_heroPerkSelection = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryHeroPerkSelectionVM>(value, "HeroPerkSelection");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryCosmeticsVM Cosmetics
	{
		get
		{
			return _cosmetics;
		}
		set
		{
			if (value != _cosmetics)
			{
				_cosmetics = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryCosmeticsVM>(value, "Cosmetics");
			}
		}
	}

	[DataSourceProperty]
	public string TauntAssignmentClickToCloseText
	{
		get
		{
			return _tauntAssignmentClickToCloseText;
		}
		set
		{
			if (value != _tauntAssignmentClickToCloseText)
			{
				_tauntAssignmentClickToCloseText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TauntAssignmentClickToCloseText");
			}
		}
	}

	[DataSourceProperty]
	public string StatsText
	{
		get
		{
			return _statsText;
		}
		set
		{
			if (value != _statsText)
			{
				_statsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "StatsText");
			}
		}
	}

	[DataSourceProperty]
	public string CustomizationText
	{
		get
		{
			return _customizationText;
		}
		set
		{
			if (value != _customizationText)
			{
				_customizationText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CustomizationText");
			}
		}
	}

	[DataSourceProperty]
	public string FacegenText
	{
		get
		{
			return _facegenText;
		}
		set
		{
			if (value != _facegenText)
			{
				_facegenText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FacegenText");
			}
		}
	}

	[DataSourceProperty]
	public string ManageTauntsText
	{
		get
		{
			return _manageTauntsText;
		}
		set
		{
			if (value != _manageTauntsText)
			{
				_manageTauntsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ManageTauntsText");
			}
		}
	}

	public MPArmoryVM(Action<BasicCharacterObject> onOpenFacegen, Action<MPArmoryCosmeticItemBaseVM> onItemObtainRequested, Func<string> getExitText)
	{
		_getExitText = getExitText;
		_onOpenFacegen = onOpenFacegen;
		_character = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
		CanOpenFacegen = true;
		ClassFilter = new MPLobbyClassFilterVM(OnSelectedClassChanged);
		HeroPreview = new MPArmoryHeroPreviewVM();
		ClassStats = new MPArmoryClassStatsVM();
		HeroPerkSelection = new MPArmoryHeroPerkSelectionVM(OnSelectPerk, ForceRefreshCharacter);
		Cosmetics = new MPArmoryCosmeticsVM(GetSelectedPerks);
		InitalizeCallbacks();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		StatsText = ((object)new TextObject("{=ffjTMejn}Stats", (Dictionary<string, object>)null)).ToString();
		CustomizationText = ((object)new TextObject("{=sPkRekRL}Customization", (Dictionary<string, object>)null)).ToString();
		FacegenText = ((object)new TextObject("{=RSx1e5Wf}Edit Character", (Dictionary<string, object>)null)).ToString();
		RefreshManageTauntButtonText();
		((ViewModel)ClassFilter).RefreshValues();
		((ViewModel)HeroPreview).RefreshValues();
		((ViewModel)ClassStats).RefreshValues();
		((ViewModel)Cosmetics).RefreshValues();
		((ViewModel)HeroPerkSelection).RefreshValues();
	}

	private void RefreshManageTauntButtonText()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		ManageTauntsText = (IsManagingTaunts ? ((object)new TextObject("{=WiNRdfsm}Done", (Dictionary<string, object>)null)).ToString() : ((object)new TextObject("{=58O7bWrD}Manage Taunts", (Dictionary<string, object>)null)).ToString());
	}

	public override void OnFinalize()
	{
		HeroPreview = null;
		_character = null;
		FinalizeCallbacks();
		((ViewModel)Cosmetics).OnFinalize();
		((ViewModel)this).OnFinalize();
	}

	private void InitalizeCallbacks()
	{
		CharacterViewModel.OnCustomAnimationFinished = (Action<CharacterViewModel>)Delegate.Combine(CharacterViewModel.OnCustomAnimationFinished, new Action<CharacterViewModel>(OnCharacterCustomAnimationFinished));
		MPArmoryCosmeticsVM.OnCosmeticPreview += OnHeroPreviewItemEquipped;
		MPArmoryCosmeticsVM.OnRemoveCosmeticFromPreview += RemoveHeroPreviewItem;
		MPArmoryCosmeticsVM.OnTauntAssignmentRefresh += OnTauntAssignmentRefresh;
	}

	private void FinalizeCallbacks()
	{
		CharacterViewModel.OnCustomAnimationFinished = (Action<CharacterViewModel>)Delegate.Remove(CharacterViewModel.OnCustomAnimationFinished, new Action<CharacterViewModel>(OnCharacterCustomAnimationFinished));
		MPArmoryCosmeticsVM.OnCosmeticPreview -= OnHeroPreviewItemEquipped;
		MPArmoryCosmeticsVM.OnRemoveCosmeticFromPreview -= RemoveHeroPreviewItem;
		MPArmoryCosmeticsVM.OnTauntAssignmentRefresh -= OnTauntAssignmentRefresh;
	}

	public void OnTick(float dt)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected I4, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected I4, but got Unknown
		if (_tauntItemToRefreshNextAnimationWith != null)
		{
			MPArmoryCosmeticTauntItemVM tauntItemToRefreshNextAnimationWith = _tauntItemToRefreshNextAnimationWith;
			Equipment val = LobbyTauntHelper.PrepareForTaunt(Equipment.CreateFromEquipmentCode(HeroPreview.HeroVisual.EquipmentCode), tauntItemToRefreshNextAnimationWith.TauntCosmeticElement);
			EquipmentIndex val2 = default(EquipmentIndex);
			EquipmentIndex val3 = default(EquipmentIndex);
			bool flag = default(bool);
			val.GetInitialWeaponIndicesToEquip(ref val2, ref val3, ref flag, (InitialWeaponEquipPreference)0);
			HeroPreview.HeroVisual.EquipmentCode = val.CalculateEquipmentCode();
			HeroPreview.HeroVisual.RightHandWieldedEquipmentIndex = (int)val2;
			if (!flag)
			{
				HeroPreview.HeroVisual.LeftHandWieldedEquipmentIndex = (int)val3;
			}
			HeroPreview.HeroVisual.ExecuteStartCustomAnimation(CosmeticsManagerHelper.GetSuitableTauntActionForEquipment(val, tauntItemToRefreshNextAnimationWith.TauntCosmeticElement), false, 0.25f);
			_currentTauntPreviewAnimationSource = _tauntItemToRefreshNextAnimationWith;
			_tauntItemToRefreshNextAnimationWith = null;
		}
		if (HeroPreview.HeroVisual.IsPlayingCustomAnimations && _currentTauntPreviewAnimationSource != null)
		{
			float num = MathF.Clamp(HeroPreview.HeroVisual.CustomAnimationProgressRatio, 0f, 1f);
			_currentTauntPreviewAnimationSource.PreviewAnimationRatio = (int)(num * 100f);
		}
	}

	public void RefreshPlayerData(PlayerData playerData)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (_character != null)
		{
			_character.UpdatePlayerCharacterBodyProperties(playerData.BodyProperties, playerData.Race, playerData.IsFemale);
			BasicCharacterObject character = _character;
			BodyProperties bodyProperties = playerData.BodyProperties;
			character.Age = ((BodyProperties)(ref bodyProperties)).Age;
			MPArmoryHeroPreviewVM heroPreview = HeroPreview;
			BasicCharacterObject character2 = _character;
			bodyProperties = playerData.BodyProperties;
			heroPreview.SetCharacter(character2, ((BodyProperties)(ref bodyProperties)).DynamicProperties, playerData.Race, playerData.IsFemale);
			ForceRefreshCharacter();
			Cosmetics.RefreshPlayerData(playerData);
		}
	}

	public void ForceRefreshCharacter()
	{
		OnSelectedClassChanged(ClassFilter.SelectedClassItem, forceUpdate: true);
	}

	private void OnIsEnabledChanged()
	{
		PlayerData playerData = NetworkMain.GameClient.PlayerData;
		if (IsEnabled && playerData != null)
		{
			RefreshPlayerData(playerData);
		}
		if (IsEnabled)
		{
			Cosmetics.RefreshCosmeticInfoFromNetwork();
			if (IsManagingTaunts)
			{
				ExecuteToggleManageTauntsState();
			}
			return;
		}
		foreach (HeroPerkVM item in (Collection<HeroPerkVM>)(object)HeroPerkSelection?.Perks)
		{
			BasicTooltipViewModel hint = item.Hint;
			if (hint != null)
			{
				hint.ExecuteEndHint();
			}
		}
	}

	private void OnSelectedClassChanged(MPLobbyClassFilterClassItemVM selectedClassItem, bool forceUpdate = false)
	{
		if (HeroPreview != null && ClassStats != null && HeroPerkSelection != null && (_currentClassItem != selectedClassItem || forceUpdate))
		{
			_currentClassItem = selectedClassItem;
			HeroPerkSelection.RefreshPerksListWithHero(selectedClassItem.HeroClass);
			HeroPreview.SetCharacterClass(selectedClassItem.HeroClass.HeroCharacter);
			HeroPreview.SetCharacterPerks(HeroPerkSelection.CurrentSelectedPerks);
			ClassStats.RefreshWith(selectedClassItem.HeroClass);
			ClassStats.HeroInformation.RefreshWith(HeroPerkSelection.CurrentHeroClass, ((IEnumerable<HeroPerkVM>)HeroPerkSelection.Perks).Select((HeroPerkVM x) => x.SelectedPerk).ToList());
			Cosmetics.RefreshSelectedClass(selectedClassItem.HeroClass, HeroPerkSelection.CurrentSelectedPerks);
			Cosmetics.RefreshCosmeticInfoFromNetwork();
		}
	}

	public void SetCanOpenFacegen(bool enabled)
	{
		CanOpenFacegen = enabled;
	}

	private void ExecuteOpenFaceGen()
	{
		_onOpenFacegen?.Invoke(_character);
	}

	public void ExecuteClearTauntSelection()
	{
		Cosmetics.ClearTauntSelections();
	}

	public void ExecuteToggleManageTauntsState()
	{
		IsManagingTaunts = !IsManagingTaunts;
		if (IsManagingTaunts)
		{
			_canOpenFacegenBeforeTauntState = CanOpenFacegen;
			Cosmetics.RefreshAvailableCategoriesBy((CosmeticType)3);
			Cosmetics.TauntSlots.ApplyActionOnAllItems((Action<MPArmoryCosmeticTauntSlotVM>)delegate(MPArmoryCosmeticTauntSlotVM s)
			{
				s.IsEnabled = true;
			});
			Cosmetics.TauntSlots.ApplyActionOnAllItems((Action<MPArmoryCosmeticTauntSlotVM>)delegate(MPArmoryCosmeticTauntSlotVM s)
			{
				s.IsFocused = false;
			});
		}
		else
		{
			Cosmetics.RefreshAvailableCategoriesBy((CosmeticType)0);
			Cosmetics.TauntSlots.ApplyActionOnAllItems((Action<MPArmoryCosmeticTauntSlotVM>)delegate(MPArmoryCosmeticTauntSlotVM s)
			{
				s.IsEnabled = false;
			});
			Cosmetics.ClearTauntSelections();
		}
		CanOpenFacegen = !IsManagingTaunts && _canOpenFacegenBeforeTauntState;
		RefreshManageTauntButtonText();
	}

	public void ExecuteSelectFocusedSlot()
	{
		foreach (MPArmoryCosmeticTauntSlotVM item in (Collection<MPArmoryCosmeticTauntSlotVM>)(object)Cosmetics.TauntSlots)
		{
			if (item.IsFocused)
			{
				item.ExecuteSelect();
				break;
			}
		}
	}

	public void ExecuteEmptyFocusedSlot()
	{
		foreach (MPArmoryCosmeticTauntSlotVM item in (Collection<MPArmoryCosmeticTauntSlotVM>)(object)Cosmetics.TauntSlots)
		{
			if (item.IsFocused)
			{
				MPArmoryCosmeticTauntItemVM assignedTauntItem = item.AssignedTauntItem;
				if (assignedTauntItem != null && assignedTauntItem.IsUsed)
				{
					item.AssignedTauntItem.ExecuteAction();
				}
				item.EmptySlotKeyVisual.SetForcedVisibility((bool?)false);
				item.SelectKeyVisual.SetForcedVisibility((bool?)false);
				break;
			}
		}
	}

	private void OnSelectPerk(HeroPerkVM heroPerk, MPPerkVM candidate)
	{
		if (ClassStats.HeroInformation.HeroClass != null && HeroPerkSelection.CurrentHeroClass != null)
		{
			List<IReadOnlyPerkObject> currentSelectedPerks = HeroPerkSelection.CurrentSelectedPerks;
			if (currentSelectedPerks.Count > 0)
			{
				ClassStats.HeroInformation.RefreshWith(HeroPerkSelection.CurrentHeroClass, currentSelectedPerks);
				HeroPreview.SetCharacterPerks(currentSelectedPerks);
				Cosmetics.RefreshSelectedClass(_currentClassItem.HeroClass, currentSelectedPerks);
			}
		}
	}

	private void RemoveHeroPreviewItem(MPArmoryCosmeticItemBaseVM itemVM)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (itemVM is MPArmoryCosmeticClothingItemVM { EquipmentElement: var equipmentElement })
		{
			EquipmentIndex cosmeticEquipmentIndex = ((EquipmentElement)(ref equipmentElement)).Item.GetCosmeticEquipmentIndex();
			MPArmoryHeroPreviewVM heroPreview = HeroPreview;
			if (heroPreview != null)
			{
				heroPreview.HeroVisual.SetEquipment(cosmeticEquipmentIndex, default(EquipmentElement));
			}
			MPArmoryHeroPreviewVM heroPreview2 = HeroPreview;
			string text = ((heroPreview2 != null) ? heroPreview2.HeroVisual.EquipmentCode : null);
			if (!string.IsNullOrEmpty(text))
			{
				_lastValidEquipment = Equipment.CreateFromEquipmentCode(text);
			}
		}
	}

	private void OnHeroPreviewItemEquipped(MPArmoryCosmeticItemBaseVM itemVM)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (itemVM is MPArmoryCosmeticClothingItemVM { EquipmentElement: var equipmentElement })
		{
			EquipmentIndex cosmeticEquipmentIndex = ((EquipmentElement)(ref equipmentElement)).Item.GetCosmeticEquipmentIndex();
			MPArmoryHeroPreviewVM heroPreview = HeroPreview;
			if (heroPreview != null)
			{
				heroPreview.HeroVisual.SetEquipment(cosmeticEquipmentIndex, equipmentElement);
			}
			MPArmoryHeroPreviewVM heroPreview2 = HeroPreview;
			string text = ((heroPreview2 != null) ? heroPreview2.HeroVisual.EquipmentCode : null);
			if (!string.IsNullOrEmpty(text))
			{
				_lastValidEquipment = Equipment.CreateFromEquipmentCode(text);
			}
		}
		else
		{
			if (!(itemVM is MPArmoryCosmeticTauntItemVM tauntItemToRefreshNextAnimationWith))
			{
				return;
			}
			if (HeroPreview?.HeroVisual != null)
			{
				HeroPreview.HeroVisual.ExecuteStopCustomAnimation();
				if (_lastValidEquipment != null)
				{
					HeroPreview.HeroVisual.SetEquipment(_lastValidEquipment);
				}
			}
			_tauntItemToRefreshNextAnimationWith = tauntItemToRefreshNextAnimationWith;
		}
	}

	private void OnCharacterCustomAnimationFinished(CharacterViewModel character)
	{
		if (character == HeroPreview.HeroVisual && _lastValidEquipment != null && HeroPreview?.HeroVisual != null)
		{
			HeroPreview.HeroVisual.SetEquipment(_lastValidEquipment);
			HeroPreview.HeroVisual.LeftHandWieldedEquipmentIndex = -1;
			HeroPreview.HeroVisual.RightHandWieldedEquipmentIndex = -1;
			_currentTauntPreviewAnimationSource.PreviewAnimationRatio = 0f;
			_currentTauntPreviewAnimationSource = null;
		}
	}

	private void OnTauntAssignmentRefresh()
	{
		IsTauntAssignmentActive = Cosmetics.SelectedTauntItem != null || Cosmetics.SelectedTauntSlot != null;
	}

	private void ResetHeroEquipment()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		MPArmoryHeroPreviewVM heroPreview = HeroPreview;
		if (heroPreview != null)
		{
			heroPreview.HeroVisual.SetEquipment(new Equipment());
		}
	}

	public static void ApplyPerkEffectsToEquipment(ref Equipment equipment, List<IReadOnlyPerkObject> selectedPerks)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		MPOnSpawnPerkHandler onSpawnPerkHandler = MPPerkObject.GetOnSpawnPerkHandler((IEnumerable<IReadOnlyPerkObject>)selectedPerks);
		IEnumerable<(EquipmentIndex, EquipmentElement)> enumerable = ((onSpawnPerkHandler != null) ? onSpawnPerkHandler.GetAlternativeEquipments(true) : null);
		if (enumerable == null)
		{
			return;
		}
		foreach (var item in enumerable)
		{
			equipment[item.Item1] = item.Item2;
		}
	}

	private List<IReadOnlyPerkObject> GetSelectedPerks()
	{
		return HeroPerkSelection.CurrentSelectedPerks;
	}
}
