using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

public class HeroInformationVM : ViewModel
{
	private const int _defaultNumberOfBotsPerFormation = 25;

	private TextObject _armySizeHintWithDefaultValue = new TextObject("{=aalbxe7z}Army Size", (Dictionary<string, object>)null);

	private ShallowItemVM.ItemGroup _latestSelectedItemGroup;

	private HintViewModel _armySizeHint;

	private HintViewModel _movementSpeedHint;

	private HintViewModel _hitPointsHint;

	private HintViewModel _armorHint;

	private ShallowItemVM _item1;

	private ShallowItemVM _item2;

	private ShallowItemVM _item3;

	private ShallowItemVM _item4;

	private ShallowItemVM _itemHorse;

	private ShallowItemVM _itemSelected;

	private string _information;

	private string _nameText;

	private string _equipmentText;

	private int _movementSpeed;

	private int _hitPoints;

	private int _armySize;

	private int _armor;

	private bool _armyAvailable;

	public MPHeroClass HeroClass { get; private set; }

	[DataSourceProperty]
	public HintViewModel ArmySizeHint
	{
		get
		{
			return _armySizeHint;
		}
		set
		{
			if (value != _armySizeHint)
			{
				_armySizeHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ArmySizeHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel MovementSpeedHint
	{
		get
		{
			return _movementSpeedHint;
		}
		set
		{
			if (value != _movementSpeedHint)
			{
				_movementSpeedHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "MovementSpeedHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel HitPointsHint
	{
		get
		{
			return _hitPointsHint;
		}
		set
		{
			if (value != _hitPointsHint)
			{
				_hitPointsHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "HitPointsHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ArmorHint
	{
		get
		{
			return _armorHint;
		}
		set
		{
			if (value != _armorHint)
			{
				_armorHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ArmorHint");
			}
		}
	}

	[DataSourceProperty]
	public ShallowItemVM Item1
	{
		get
		{
			return _item1;
		}
		set
		{
			if (value != _item1)
			{
				_item1 = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShallowItemVM>(value, "Item1");
			}
		}
	}

	[DataSourceProperty]
	public ShallowItemVM Item2
	{
		get
		{
			return _item2;
		}
		set
		{
			if (value != _item2)
			{
				_item2 = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShallowItemVM>(value, "Item2");
			}
		}
	}

	[DataSourceProperty]
	public ShallowItemVM Item3
	{
		get
		{
			return _item3;
		}
		set
		{
			if (value != _item3)
			{
				_item3 = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShallowItemVM>(value, "Item3");
			}
		}
	}

	[DataSourceProperty]
	public ShallowItemVM Item4
	{
		get
		{
			return _item4;
		}
		set
		{
			if (value != _item4)
			{
				_item4 = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShallowItemVM>(value, "Item4");
			}
		}
	}

	[DataSourceProperty]
	public ShallowItemVM ItemHorse
	{
		get
		{
			return _itemHorse;
		}
		set
		{
			if (value != _itemHorse)
			{
				_itemHorse = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShallowItemVM>(value, "ItemHorse");
			}
		}
	}

	[DataSourceProperty]
	public ShallowItemVM ItemSelected
	{
		get
		{
			return _itemSelected;
		}
		set
		{
			if (value != _itemSelected)
			{
				_itemSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShallowItemVM>(value, "ItemSelected");
			}
		}
	}

	[DataSourceProperty]
	public string Information
	{
		get
		{
			return _information;
		}
		set
		{
			if (value != _information)
			{
				_information = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Information");
			}
		}
	}

	[DataSourceProperty]
	public string EquipmentText
	{
		get
		{
			return _equipmentText;
		}
		set
		{
			if (value != _equipmentText)
			{
				_equipmentText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "EquipmentText");
			}
		}
	}

	[DataSourceProperty]
	public string NameText
	{
		get
		{
			return _nameText;
		}
		set
		{
			if (value != _nameText)
			{
				_nameText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NameText");
			}
		}
	}

	[DataSourceProperty]
	public int MovementSpeed
	{
		get
		{
			return _movementSpeed;
		}
		set
		{
			if (value != _movementSpeed)
			{
				_movementSpeed = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MovementSpeed");
			}
		}
	}

	[DataSourceProperty]
	public int ArmySize
	{
		get
		{
			return _armySize;
		}
		set
		{
			if (value != _armySize)
			{
				_armySize = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ArmySize");
			}
		}
	}

	[DataSourceProperty]
	public int HitPoints
	{
		get
		{
			return _hitPoints;
		}
		set
		{
			if (value != _hitPoints)
			{
				_hitPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HitPoints");
			}
		}
	}

	[DataSourceProperty]
	public int Armor
	{
		get
		{
			return _armor;
		}
		set
		{
			if (value != _armor)
			{
				_armor = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Armor");
			}
		}
	}

	[DataSourceProperty]
	public bool IsArmyAvailable
	{
		get
		{
			return _armyAvailable;
		}
		set
		{
			if (value != _armyAvailable)
			{
				_armyAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsArmyAvailable");
			}
		}
	}

	public HeroInformationVM()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		_latestSelectedItemGroup = ShallowItemVM.ItemGroup.None;
		Item1 = new ShallowItemVM(UpdateHighlightedItem);
		Item2 = new ShallowItemVM(UpdateHighlightedItem);
		Item3 = new ShallowItemVM(UpdateHighlightedItem);
		Item4 = new ShallowItemVM(UpdateHighlightedItem);
		ItemHorse = new ShallowItemVM(UpdateHighlightedItem);
		IsArmyAvailable = MultiplayerOptionsExtensions.GetIntValue((OptionType)20, (MultiplayerOptionsAccessMode)1) > 0;
		SetFirstSelectedItem();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		ArmySizeHint = new HintViewModel(GameTexts.FindText("str_army_size", (string)null), (string)null);
		MovementSpeedHint = new HintViewModel(GameTexts.FindText("str_movement_speed", (string)null), (string)null);
		HitPointsHint = new HintViewModel(GameTexts.FindText("str_hitpoints", (string)null), (string)null);
		ArmorHint = new HintViewModel(GameTexts.FindText("str_armor", (string)null), (string)null);
		EquipmentText = ((object)GameTexts.FindText("str_equipment", (string)null)).ToString();
		ShallowItemVM item = _item1;
		if (item != null)
		{
			((ViewModel)item).RefreshValues();
		}
		ShallowItemVM item2 = _item2;
		if (item2 != null)
		{
			((ViewModel)item2).RefreshValues();
		}
		ShallowItemVM item3 = _item3;
		if (item3 != null)
		{
			((ViewModel)item3).RefreshValues();
		}
		ShallowItemVM item4 = _item4;
		if (item4 != null)
		{
			((ViewModel)item4).RefreshValues();
		}
		ShallowItemVM itemHorse = _itemHorse;
		if (itemHorse != null)
		{
			((ViewModel)itemHorse).RefreshValues();
		}
		ShallowItemVM itemSelected = _itemSelected;
		if (itemSelected != null)
		{
			((ViewModel)itemSelected).RefreshValues();
		}
		if (HeroClass != null)
		{
			NameText = ((object)HeroClass.HeroName).ToString();
		}
	}

	public void RefreshWith(MPHeroClass heroClass, List<IReadOnlyPerkObject> perks)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		HeroClass = heroClass;
		Equipment val = heroClass.HeroCharacter.Equipment.Clone(false);
		MPOnSpawnPerkHandler onSpawnPerkHandler = MPPerkObject.GetOnSpawnPerkHandler((IEnumerable<IReadOnlyPerkObject>)perks);
		IEnumerable<(EquipmentIndex, EquipmentElement)> enumerable = ((onSpawnPerkHandler != null) ? onSpawnPerkHandler.GetAlternativeEquipments(true) : null);
		if (enumerable != null)
		{
			foreach (var item in enumerable)
			{
				val[item.Item1] = item.Item2;
			}
		}
		ItemHorse.RefreshWith((EquipmentIndex)10, val);
		Item1.RefreshWith((EquipmentIndex)0, val);
		Item2.RefreshWith((EquipmentIndex)1, val);
		Item3.RefreshWith((EquipmentIndex)2, val);
		Item4.RefreshWith((EquipmentIndex)3, val);
		Information = ((object)heroClass.HeroInformation)?.ToString();
		NameText = ((object)heroClass.HeroName).ToString();
		int num = MultiplayerOptionsExtensions.GetIntValue((OptionType)20, (MultiplayerOptionsAccessMode)1);
		if (num == 0)
		{
			num = 25;
			_armySizeHintWithDefaultValue.SetTextVariable("OPTION_VALUE", 25);
			ArmySizeHint.HintText = _armySizeHintWithDefaultValue;
		}
		else
		{
			ArmySizeHint.HintText = GameTexts.FindText("str_army_size", (string)null);
		}
		ArmySize = MPPerkObject.GetTroopCount(heroClass, num, onSpawnPerkHandler);
		MovementSpeed = (int)(HeroClass.HeroMovementSpeedMultiplier * 100f);
		HitPoints = heroClass.Health;
		Armor = (int)((onSpawnPerkHandler != null) ? onSpawnPerkHandler.GetDrivenPropertyBonusOnSpawn(true, (DrivenProperty)56, (float)HeroClass.ArmorValue) : 0f) + HeroClass.ArmorValue;
		if (!TrySetSelectedItemByType(_latestSelectedItemGroup))
		{
			SetFirstSelectedItem();
		}
	}

	private bool TrySetSelectedItemByType(ShallowItemVM.ItemGroup itemGroup)
	{
		if (Item1.IsValid && Item1.Type == itemGroup)
		{
			UpdateHighlightedItem(Item1);
			return true;
		}
		if (Item2.IsValid && Item2.Type == itemGroup)
		{
			UpdateHighlightedItem(Item2);
			return true;
		}
		if (Item3.IsValid && Item3.Type == itemGroup)
		{
			UpdateHighlightedItem(Item3);
			return true;
		}
		if (Item4.IsValid && Item4.Type == itemGroup)
		{
			UpdateHighlightedItem(Item4);
			return true;
		}
		if (ItemHorse.IsValid && ItemHorse.Type == itemGroup)
		{
			UpdateHighlightedItem(ItemHorse);
			return true;
		}
		return false;
	}

	private void SetFirstSelectedItem()
	{
		ShallowItemVM itemSelected = ItemSelected;
		if (itemSelected == null || !itemSelected.IsValid)
		{
			if (Item1.IsValid)
			{
				UpdateHighlightedItem(Item1);
			}
			else if (Item2.IsValid)
			{
				UpdateHighlightedItem(Item2);
			}
			else if (Item3.IsValid)
			{
				UpdateHighlightedItem(Item3);
			}
			else if (Item4.IsValid)
			{
				UpdateHighlightedItem(Item4);
			}
			else if (ItemHorse.IsValid)
			{
				UpdateHighlightedItem(ItemHorse);
			}
		}
	}

	public void UpdateHighlightedItem(ShallowItemVM item)
	{
		ItemSelected = item;
		Item1.IsSelected = false;
		Item2.IsSelected = false;
		Item3.IsSelected = false;
		Item4.IsSelected = false;
		ItemHorse.IsSelected = false;
		item.IsSelected = true;
		_latestSelectedItemGroup = item.Type;
	}
}
