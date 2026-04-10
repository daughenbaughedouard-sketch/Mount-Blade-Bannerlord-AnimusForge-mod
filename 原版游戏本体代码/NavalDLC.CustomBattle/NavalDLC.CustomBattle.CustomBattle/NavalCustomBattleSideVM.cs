using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NavalDLC.CustomBattle.CustomBattle.SelectionItem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.CustomBattle.CustomBattle;

public class NavalCustomBattleSideVM : ViewModel
{
	private readonly TextObject _sideName;

	private readonly bool _isPlayerSide;

	private readonly Action _onCharacterSelected;

	private NavalCustomBattleArmyCompositionGroupVM _compositionGroup;

	private NavalCustomBattleFactionSelectionVM _factionSelectionGroup;

	private SelectorVM<NavalCustomBattleCharacterItemVM> _characterSelectionGroup;

	private NavalCustomBattleShipSelectionGroupVM _shipSelectionGroup;

	private CharacterViewModel _currentSelectedCharacter;

	private MBBindingList<CharacterEquipmentItemVM> _armorsList;

	private MBBindingList<CharacterEquipmentItemVM> _weaponsList;

	private string _name;

	private string _factionText;

	private string _titleText;

	public BasicCharacterObject SelectedCharacter { get; private set; }

	[DataSourceProperty]
	public CharacterViewModel CurrentSelectedCharacter
	{
		get
		{
			return _currentSelectedCharacter;
		}
		set
		{
			if (value != _currentSelectedCharacter)
			{
				_currentSelectedCharacter = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterViewModel>(value, "CurrentSelectedCharacter");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CharacterEquipmentItemVM> ArmorsList
	{
		get
		{
			return _armorsList;
		}
		set
		{
			if (value != _armorsList)
			{
				_armorsList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CharacterEquipmentItemVM>>(value, "ArmorsList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CharacterEquipmentItemVM> WeaponsList
	{
		get
		{
			return _weaponsList;
		}
		set
		{
			if (value != _weaponsList)
			{
				_weaponsList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CharacterEquipmentItemVM>>(value, "WeaponsList");
			}
		}
	}

	[DataSourceProperty]
	public string FactionText
	{
		get
		{
			return _factionText;
		}
		set
		{
			if (value != _factionText)
			{
				_factionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FactionText");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<NavalCustomBattleCharacterItemVM> CharacterSelectionGroup
	{
		get
		{
			return _characterSelectionGroup;
		}
		set
		{
			if (value != _characterSelectionGroup)
			{
				_characterSelectionGroup = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<NavalCustomBattleCharacterItemVM>>(value, "CharacterSelectionGroup");
			}
		}
	}

	[DataSourceProperty]
	public NavalCustomBattleArmyCompositionGroupVM CompositionGroup
	{
		get
		{
			return _compositionGroup;
		}
		set
		{
			if (value != _compositionGroup)
			{
				_compositionGroup = value;
				((ViewModel)this).OnPropertyChangedWithValue<NavalCustomBattleArmyCompositionGroupVM>(value, "CompositionGroup");
			}
		}
	}

	[DataSourceProperty]
	public NavalCustomBattleFactionSelectionVM FactionSelectionGroup
	{
		get
		{
			return _factionSelectionGroup;
		}
		set
		{
			if (value != _factionSelectionGroup)
			{
				_factionSelectionGroup = value;
				((ViewModel)this).OnPropertyChangedWithValue<NavalCustomBattleFactionSelectionVM>(value, "FactionSelectionGroup");
			}
		}
	}

	[DataSourceProperty]
	public NavalCustomBattleShipSelectionGroupVM ShipSelectionGroup
	{
		get
		{
			return _shipSelectionGroup;
		}
		set
		{
			if (value != _shipSelectionGroup)
			{
				_shipSelectionGroup = value;
				((ViewModel)this).OnPropertyChangedWithValue<NavalCustomBattleShipSelectionGroupVM>(value, "ShipSelectionGroup");
			}
		}
	}

	public NavalCustomBattleSideVM(TextObject sideName, bool isPlayerSide, NavalCustomBattleTroopTypeSelectionPopUpVM troopTypeSelectionPopUp, NavalCustomBattleShipSelectionPopUpVM shipSelectionPopUp, Action<NavalCustomBattleShipItemVM> onShipFocused, Action onCharacterSelected)
	{
		_sideName = sideName;
		_isPlayerSide = isPlayerSide;
		_onCharacterSelected = onCharacterSelected;
		CompositionGroup = new NavalCustomBattleArmyCompositionGroupVM(troopTypeSelectionPopUp);
		FactionSelectionGroup = new NavalCustomBattleFactionSelectionVM(OnCultureSelection);
		CharacterSelectionGroup = new SelectorVM<NavalCustomBattleCharacterItemVM>(0, (Action<SelectorVM<NavalCustomBattleCharacterItemVM>>)OnCharacterSelection);
		ShipSelectionGroup = new NavalCustomBattleShipSelectionGroupVM(_isPlayerSide, shipSelectionPopUp, UpdateTroopCountLimits, onShipFocused);
		ArmorsList = new MBBindingList<CharacterEquipmentItemVM>();
		WeaponsList = new MBBindingList<CharacterEquipmentItemVM>();
		UpdateTroopCountLimits();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		Name = ((object)_sideName).ToString();
		FactionText = ((object)GameTexts.FindText("str_faction", (string)null)).ToString();
		if (_isPlayerSide)
		{
			TitleText = ((object)new TextObject("{=bLXleed8}Player Character", (Dictionary<string, object>)null)).ToString();
		}
		else
		{
			TitleText = ((object)new TextObject("{=QAYngoNQ}Enemy Character", (Dictionary<string, object>)null)).ToString();
		}
		((Collection<NavalCustomBattleCharacterItemVM>)(object)CharacterSelectionGroup.ItemList).Clear();
		foreach (BasicCharacterObject character in NavalCustomBattleData.Characters)
		{
			CharacterSelectionGroup.AddItem(new NavalCustomBattleCharacterItemVM(character));
		}
		CharacterSelectionGroup.SelectedIndex = ((!_isPlayerSide) ? 1 : 0);
		UpdateCharacterVisual();
		_onCharacterSelected?.Invoke();
		((ViewModel)CompositionGroup).RefreshValues();
		((ViewModel)CharacterSelectionGroup).RefreshValues();
		((ViewModel)FactionSelectionGroup).RefreshValues();
		((ViewModel)ShipSelectionGroup).RefreshValues();
	}

	public void UpdateTroopCountLimits()
	{
		if (ShipSelectionGroup != null && CompositionGroup != null)
		{
			List<IShipOrigin> selectedShips = ShipSelectionGroup.GetSelectedShips();
			int minTroopCount = selectedShips.Count() * 4;
			int maxTroopCount = selectedShips.Sum((IShipOrigin x) => x.TotalCrewCapacity);
			int skeletalSize = selectedShips.Sum((IShipOrigin x) => x.SkeletalCrewCapacity);
			int deckSize = selectedShips.Sum((IShipOrigin x) => x.MainDeckCrewCapacity);
			CompositionGroup.UpdateTroopCountLimits(minTroopCount, maxTroopCount, skeletalSize, deckSize);
		}
	}

	private void OnCultureSelection(BasicCultureObject selectedCulture)
	{
		CompositionGroup.SetCurrentSelectedCulture(selectedCulture);
		if (CurrentSelectedCharacter != null)
		{
			CurrentSelectedCharacter.ArmorColor1 = selectedCulture.Color;
			CurrentSelectedCharacter.ArmorColor2 = selectedCulture.Color2;
			CurrentSelectedCharacter.BannerCodeText = selectedCulture.Banner.BannerCode;
		}
	}

	private void OnCharacterSelection(SelectorVM<NavalCustomBattleCharacterItemVM> selector)
	{
		BasicCharacterObject character = selector.SelectedItem.Character;
		SelectedCharacter = character;
		UpdateCharacterVisual();
		_onCharacterSelected?.Invoke();
	}

	public void UpdateCharacterVisual()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Expected O, but got Unknown
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Expected O, but got Unknown
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Expected O, but got Unknown
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Expected O, but got Unknown
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Expected O, but got Unknown
		CurrentSelectedCharacter = new CharacterViewModel((StanceTypes)1);
		CharacterViewModel currentSelectedCharacter = CurrentSelectedCharacter;
		BasicCharacterObject selectedCharacter = SelectedCharacter;
		NavalCustomBattleFactionSelectionVM factionSelectionGroup = FactionSelectionGroup;
		object obj;
		if (factionSelectionGroup == null)
		{
			obj = null;
		}
		else
		{
			NavalCustomBattleFactionItemVM selectedItem = factionSelectionGroup.SelectedItem;
			obj = ((selectedItem != null) ? selectedItem.Faction.Banner.BannerCode : null);
		}
		currentSelectedCharacter.FillFrom(selectedCharacter, -1, (string)obj);
		CurrentSelectedCharacter.SetEquipment((EquipmentIndex)10, EquipmentElement.Invalid);
		if (FactionSelectionGroup?.SelectedItem != null)
		{
			CurrentSelectedCharacter.ArmorColor1 = FactionSelectionGroup.SelectedItem.Faction.Color;
			CurrentSelectedCharacter.ArmorColor2 = FactionSelectionGroup.SelectedItem.Faction.Color2;
		}
		((Collection<CharacterEquipmentItemVM>)(object)ArmorsList).Clear();
		MBBindingList<CharacterEquipmentItemVM> armorsList = ArmorsList;
		EquipmentElement val = SelectedCharacter.Equipment[(EquipmentIndex)5];
		((Collection<CharacterEquipmentItemVM>)(object)armorsList).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
		MBBindingList<CharacterEquipmentItemVM> armorsList2 = ArmorsList;
		val = SelectedCharacter.Equipment[(EquipmentIndex)9];
		((Collection<CharacterEquipmentItemVM>)(object)armorsList2).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
		MBBindingList<CharacterEquipmentItemVM> armorsList3 = ArmorsList;
		val = SelectedCharacter.Equipment[(EquipmentIndex)6];
		((Collection<CharacterEquipmentItemVM>)(object)armorsList3).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
		MBBindingList<CharacterEquipmentItemVM> armorsList4 = ArmorsList;
		val = SelectedCharacter.Equipment[(EquipmentIndex)8];
		((Collection<CharacterEquipmentItemVM>)(object)armorsList4).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
		MBBindingList<CharacterEquipmentItemVM> armorsList5 = ArmorsList;
		val = SelectedCharacter.Equipment[(EquipmentIndex)7];
		((Collection<CharacterEquipmentItemVM>)(object)armorsList5).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
		((Collection<CharacterEquipmentItemVM>)(object)WeaponsList).Clear();
		MBBindingList<CharacterEquipmentItemVM> weaponsList = WeaponsList;
		val = SelectedCharacter.Equipment[(EquipmentIndex)0];
		((Collection<CharacterEquipmentItemVM>)(object)weaponsList).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
		MBBindingList<CharacterEquipmentItemVM> weaponsList2 = WeaponsList;
		val = SelectedCharacter.Equipment[(EquipmentIndex)1];
		((Collection<CharacterEquipmentItemVM>)(object)weaponsList2).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
		MBBindingList<CharacterEquipmentItemVM> weaponsList3 = WeaponsList;
		val = SelectedCharacter.Equipment[(EquipmentIndex)2];
		((Collection<CharacterEquipmentItemVM>)(object)weaponsList3).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
		MBBindingList<CharacterEquipmentItemVM> weaponsList4 = WeaponsList;
		val = SelectedCharacter.Equipment[(EquipmentIndex)3];
		((Collection<CharacterEquipmentItemVM>)(object)weaponsList4).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
		MBBindingList<CharacterEquipmentItemVM> weaponsList5 = WeaponsList;
		val = SelectedCharacter.Equipment[(EquipmentIndex)4];
		((Collection<CharacterEquipmentItemVM>)(object)weaponsList5).Add(new CharacterEquipmentItemVM(((EquipmentElement)(ref val)).Item));
	}

	public void Randomize(int targetDeckSize)
	{
		CharacterSelectionGroup.ExecuteRandomize();
		FactionSelectionGroup.ExecuteRandomize();
		ShipSelectionGroup.ExecuteRandomize(targetDeckSize);
		CompositionGroup.ExecuteRandomize();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		SelectorVM<NavalCustomBattleCharacterItemVM> characterSelectionGroup = CharacterSelectionGroup;
		if (characterSelectionGroup != null)
		{
			((ViewModel)characterSelectionGroup).OnFinalize();
		}
		NavalCustomBattleFactionSelectionVM factionSelectionGroup = FactionSelectionGroup;
		if (factionSelectionGroup != null)
		{
			((ViewModel)factionSelectionGroup).OnFinalize();
		}
		NavalCustomBattleShipSelectionGroupVM shipSelectionGroup = ShipSelectionGroup;
		if (shipSelectionGroup != null)
		{
			((ViewModel)shipSelectionGroup).OnFinalize();
		}
		NavalCustomBattleArmyCompositionGroupVM compositionGroup = CompositionGroup;
		if (compositionGroup != null)
		{
			((ViewModel)compositionGroup).OnFinalize();
		}
	}

	public void SetCycleTierInputKey(HotKey hotkey)
	{
		ShipSelectionGroup.SetCycleTierInputKey(hotkey);
	}
}
