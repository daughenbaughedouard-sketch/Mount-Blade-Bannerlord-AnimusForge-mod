using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;

namespace TaleWorlds.MountAndBlade.CustomBattle;

public class CustomBattleSideVM : ViewModel
{
	private readonly TextObject _sideName;

	private readonly bool _isPlayerSide;

	private readonly Action _onCharacterSelected;

	private ArmyCompositionGroupVM _compositionGroup;

	private CustomBattleFactionSelectionVM _factionSelectionGroup;

	private SelectorVM<CharacterItemVM> _characterSelectionGroup;

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
	public SelectorVM<CharacterItemVM> CharacterSelectionGroup
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
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<CharacterItemVM>>(value, "CharacterSelectionGroup");
			}
		}
	}

	[DataSourceProperty]
	public ArmyCompositionGroupVM CompositionGroup
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
				((ViewModel)this).OnPropertyChangedWithValue<ArmyCompositionGroupVM>(value, "CompositionGroup");
			}
		}
	}

	[DataSourceProperty]
	public CustomBattleFactionSelectionVM FactionSelectionGroup
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
				((ViewModel)this).OnPropertyChangedWithValue<CustomBattleFactionSelectionVM>(value, "FactionSelectionGroup");
			}
		}
	}

	public CustomBattleSideVM(TextObject sideName, bool isPlayerSide, TroopTypeSelectionPopUpVM troopTypeSelectionPopUp, Action onCharacterSelected)
	{
		_sideName = sideName;
		_isPlayerSide = isPlayerSide;
		_onCharacterSelected = onCharacterSelected;
		CompositionGroup = new ArmyCompositionGroupVM(troopTypeSelectionPopUp);
		FactionSelectionGroup = new CustomBattleFactionSelectionVM(OnCultureSelection);
		CharacterSelectionGroup = new SelectorVM<CharacterItemVM>(0, (Action<SelectorVM<CharacterItemVM>>)OnCharacterSelection);
		ArmorsList = new MBBindingList<CharacterEquipmentItemVM>();
		WeaponsList = new MBBindingList<CharacterEquipmentItemVM>();
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
		((Collection<CharacterItemVM>)(object)CharacterSelectionGroup.ItemList).Clear();
		foreach (BasicCharacterObject character in CustomBattleData.Characters)
		{
			CharacterSelectionGroup.AddItem(new CharacterItemVM(character));
		}
		CharacterSelectionGroup.SelectedIndex = ((!_isPlayerSide) ? 1 : 0);
		UpdateCharacterVisual();
		_onCharacterSelected?.Invoke();
		((ViewModel)CompositionGroup).RefreshValues();
		((ViewModel)CharacterSelectionGroup).RefreshValues();
		((ViewModel)FactionSelectionGroup).RefreshValues();
	}

	public void OnPlayerTypeChange(CustomBattlePlayerType playerType)
	{
		CompositionGroup.OnPlayerTypeChange(playerType);
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

	private void OnCharacterSelection(SelectorVM<CharacterItemVM> selector)
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
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Expected O, but got Unknown
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Expected O, but got Unknown
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Expected O, but got Unknown
		CurrentSelectedCharacter = new CharacterViewModel((StanceTypes)1);
		CharacterViewModel currentSelectedCharacter = CurrentSelectedCharacter;
		BasicCharacterObject selectedCharacter = SelectedCharacter;
		CustomBattleFactionSelectionVM factionSelectionGroup = FactionSelectionGroup;
		object obj;
		if (factionSelectionGroup == null)
		{
			obj = null;
		}
		else
		{
			FactionItemVM selectedItem = factionSelectionGroup.SelectedItem;
			obj = ((selectedItem != null) ? selectedItem.Faction.Banner.BannerCode : null);
		}
		currentSelectedCharacter.FillFrom(selectedCharacter, -1, (string)obj);
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

	public void Randomize(CustomBattleSideVM oppositeSide = null)
	{
		CharacterSelectionGroup.ExecuteRandomize();
		FactionSelectionGroup.ExecuteRandomize();
		CompositionGroup.ExecuteRandomize(oppositeSide?.CompositionGroup);
	}
}
