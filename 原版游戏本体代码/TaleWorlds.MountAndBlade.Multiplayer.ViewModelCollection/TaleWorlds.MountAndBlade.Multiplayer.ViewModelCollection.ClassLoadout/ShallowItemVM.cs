using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

public class ShallowItemVM : ViewModel
{
	public enum ItemGroup
	{
		None,
		Spear,
		Javelin,
		Bow,
		Crossbow,
		Sword,
		Axe,
		Mace,
		ThrowingAxe,
		ThrowingKnife,
		Ammo,
		Shield,
		Mount,
		Stone
	}

	public class ArmoryItemFlagVM : ViewModel
	{
		private string _icon;

		private HintViewModel _hint;

		[DataSourceProperty]
		public string Icon
		{
			get
			{
				return _icon;
			}
			set
			{
				if (value != _icon)
				{
					_icon = value;
					((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Icon");
				}
			}
		}

		[DataSourceProperty]
		public HintViewModel Hint
		{
			get
			{
				return _hint;
			}
			set
			{
				if (value != _hint)
				{
					_hint = value;
					((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "Hint");
				}
			}
		}

		public ArmoryItemFlagVM(string icon, TextObject hintText)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			Icon = "SPGeneral\\" + icon;
			Hint = new HintViewModel(hintText, (string)null);
		}
	}

	private readonly Action<ShallowItemVM> _onSelect;

	private AlternativeUsageItemOptionVM _latestUsageOption;

	private Equipment _equipment;

	private EquipmentIndex _equipmentIndex;

	private bool _isInitialized;

	private ItemImageIdentifierVM _icon;

	private string _name;

	private string _typeAsString;

	private bool _isValid;

	private bool _isSelected;

	private bool _hasAnyAlternativeUsage;

	private MBBindingList<ArmoryItemFlagVM> _itemInformationList;

	private MBBindingList<ShallowItemPropertyVM> _propertyList;

	private SelectorVM<AlternativeUsageItemOptionVM> _alternativeUsageSelector;

	public ItemGroup Type { get; private set; }

	[DataSourceProperty]
	public MBBindingList<ArmoryItemFlagVM> ItemInformationList
	{
		get
		{
			return _itemInformationList;
		}
		set
		{
			if (value != _itemInformationList)
			{
				_itemInformationList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<ArmoryItemFlagVM>>(value, "ItemInformationList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ShallowItemPropertyVM> PropertyList
	{
		get
		{
			return _propertyList;
		}
		set
		{
			if (value != _propertyList)
			{
				_propertyList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<ShallowItemPropertyVM>>(value, "PropertyList");
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
	public ItemImageIdentifierVM Icon
	{
		get
		{
			return _icon;
		}
		set
		{
			if (value != _icon)
			{
				_icon = value;
				((ViewModel)this).OnPropertyChangedWithValue<ItemImageIdentifierVM>(value, "Icon");
			}
		}
	}

	[DataSourceProperty]
	public string TypeAsString
	{
		get
		{
			return _typeAsString;
		}
		set
		{
			if (value != _typeAsString)
			{
				_typeAsString = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TypeAsString");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool HasAnyAlternativeUsage
	{
		get
		{
			return _hasAnyAlternativeUsage;
		}
		set
		{
			if (value != _hasAnyAlternativeUsage)
			{
				_hasAnyAlternativeUsage = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasAnyAlternativeUsage");
			}
		}
	}

	[DataSourceProperty]
	public bool IsValid
	{
		get
		{
			return _isValid;
		}
		set
		{
			if (value != _isValid)
			{
				_isValid = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsValid");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<AlternativeUsageItemOptionVM> AlternativeUsageSelector
	{
		get
		{
			return _alternativeUsageSelector;
		}
		set
		{
			if (value != _alternativeUsageSelector)
			{
				_alternativeUsageSelector = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<AlternativeUsageItemOptionVM>>(value, "AlternativeUsageSelector");
			}
		}
	}

	public ShallowItemVM(Action<ShallowItemVM> onSelect)
	{
		ItemInformationList = new MBBindingList<ArmoryItemFlagVM>();
		PropertyList = new MBBindingList<ShallowItemPropertyVM>();
		AlternativeUsageSelector = new SelectorVM<AlternativeUsageItemOptionVM>((IEnumerable<string>)new List<string>(), 0, (Action<SelectorVM<AlternativeUsageItemOptionVM>>)OnAlternativeUsageChanged);
		_onSelect = onSelect;
	}

	public override void RefreshValues()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((ViewModel)this).RefreshValues();
		RefreshWith(_equipmentIndex, _equipment);
		PropertyList.ApplyActionOnAllItems((Action<ShallowItemPropertyVM>)delegate(ShallowItemPropertyVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		_equipment = null;
	}

	public void RefreshWith(EquipmentIndex equipmentIndex, Equipment equipment)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		_equipment = equipment;
		_equipmentIndex = equipmentIndex;
		object obj;
		if (equipment == null)
		{
			obj = null;
		}
		else
		{
			EquipmentElement val = equipment[equipmentIndex];
			obj = ((EquipmentElement)(ref val)).Item;
		}
		ItemObject val2 = (ItemObject)obj;
		if (val2 == null || ((int)equipmentIndex == 10 && !val2.HasHorseComponent) || ((int)equipmentIndex != 10 && (val2.PrimaryWeapon == null || val2.PrimaryWeapon.IsAmmo)))
		{
			IsValid = false;
			Icon = new ItemImageIdentifierVM((ItemObject)null, "");
			return;
		}
		IsValid = true;
		Name = ((object)val2.Name).ToString();
		Icon = new ItemImageIdentifierVM(val2, "");
		Type = GetItemGroupType(val2);
		TypeAsString = ((Type == ItemGroup.None) ? "" : Type.ToString());
		HasAnyAlternativeUsage = false;
		((Collection<AlternativeUsageItemOptionVM>)(object)AlternativeUsageSelector.ItemList).Clear();
		if (val2.PrimaryWeapon != null)
		{
			for (int i = 0; i < ((List<WeaponComponentData>)(object)val2.Weapons).Count; i++)
			{
				WeaponComponentData val3 = ((List<WeaponComponentData>)(object)val2.Weapons)[i];
				if (IsItemUsageApplicable(val3))
				{
					TextObject val4 = GameTexts.FindText("str_weapon_usage", val3.WeaponDescriptionId);
					AlternativeUsageSelector.AddItem(new AlternativeUsageItemOptionVM(val3.WeaponDescriptionId, val4, val4, AlternativeUsageSelector, i));
					HasAnyAlternativeUsage = true;
				}
			}
		}
		AlternativeUsageSelector.SelectedIndex = -1;
		AlternativeUsageSelector.SelectedIndex = 0;
		_latestUsageOption = ((IEnumerable<AlternativeUsageItemOptionVM>)AlternativeUsageSelector.ItemList).FirstOrDefault();
		if (_latestUsageOption != null)
		{
			((SelectorItemVM)_latestUsageOption).IsSelected = true;
		}
		AlternativeUsageSelector.SetOnChangeAction((Action<SelectorVM<AlternativeUsageItemOptionVM>>)OnAlternativeUsageChanged);
		RefreshItemPropertyList(_equipmentIndex, _equipment, AlternativeUsageSelector.SelectedIndex);
		_isInitialized = true;
	}

	private void OnAlternativeUsageChanged(SelectorVM<AlternativeUsageItemOptionVM> selector)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (_isInitialized && selector.SelectedIndex >= 0)
		{
			if (_latestUsageOption != null)
			{
				((SelectorItemVM)_latestUsageOption).IsSelected = false;
			}
			RefreshItemPropertyList(_equipmentIndex, _equipment, selector.SelectedIndex);
			if (selector.SelectedItem != null)
			{
				((SelectorItemVM)selector.SelectedItem).IsSelected = true;
			}
		}
	}

	private void RefreshItemPropertyList(EquipmentIndex equipmentIndex, Equipment equipment, int alternativeIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Expected O, but got Unknown
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0502: Expected O, but got Unknown
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Expected O, but got Unknown
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Expected O, but got Unknown
		//IL_0541: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Invalid comparison between Unknown and I4
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Invalid comparison between Unknown and I4
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Invalid comparison between Unknown and I4
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Invalid comparison between Unknown and I4
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected O, but got Unknown
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected O, but got Unknown
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected O, but got Unknown
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Expected O, but got Unknown
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Expected O, but got Unknown
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Invalid comparison between Unknown and I4
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Expected O, but got Unknown
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Expected O, but got Unknown
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Expected O, but got Unknown
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Invalid comparison between Unknown and I4
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Invalid comparison between Unknown and I4
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Invalid comparison between Unknown and I4
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Expected O, but got Unknown
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Expected O, but got Unknown
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Expected O, but got Unknown
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Expected O, but got Unknown
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Expected O, but got Unknown
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		EquipmentElement val = equipment[equipmentIndex];
		ItemObject item = ((EquipmentElement)(ref val)).Item;
		val = equipment[equipmentIndex];
		ItemModifier itemModifier = ((EquipmentElement)(ref val)).ItemModifier;
		((Collection<ShallowItemPropertyVM>)(object)PropertyList).Clear();
		if (item.PrimaryWeapon != null)
		{
			WeaponComponentData val2 = ((List<WeaponComponentData>)(object)item.Weapons)[alternativeIndex];
			ItemTypeEnum itemTypeFromWeaponClass = WeaponComponentData.GetItemTypeFromWeaponClass(val2.WeaponClass);
			if ((int)itemTypeFromWeaponClass == 2 || (int)itemTypeFromWeaponClass == 3 || (int)itemTypeFromWeaponClass == 4)
			{
				if ((int)val2.SwingDamageType != -1)
				{
					AddProperty(new TextObject("{=yJsE4Ayo}Swing Spd.", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedSwingSpeed(val2, itemModifier) / 145f, WeaponComponentDataExtensions.GetModifiedSwingSpeed(val2, itemModifier));
					AddProperty(new TextObject("{=RNgWFLIO}Swing Dmg.", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedSwingDamage(val2, itemModifier) / 143f, WeaponComponentDataExtensions.GetModifiedSwingDamage(val2, itemModifier));
				}
				if ((int)val2.ThrustDamageType != -1)
				{
					AddProperty(new TextObject("{=J0vjDOFO}Thrust Spd.", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedThrustSpeed(val2, itemModifier) / 114f, WeaponComponentDataExtensions.GetModifiedThrustSpeed(val2, itemModifier));
					AddProperty(new TextObject("{=Ie9I2Bha}Thrust Dmg.", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedThrustDamage(val2, itemModifier) / 86f, WeaponComponentDataExtensions.GetModifiedThrustDamage(val2, itemModifier));
				}
				AddProperty(new TextObject("{=ftoSCQ0x}Length", (Dictionary<string, object>)null), (float)val2.WeaponLength / 315f, val2.WeaponLength);
				AddProperty(new TextObject("{=oibdTnXP}Handling", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedHandling(val2, itemModifier) / 120f, WeaponComponentDataExtensions.GetModifiedHandling(val2, itemModifier));
			}
			if ((int)itemTypeFromWeaponClass == 12)
			{
				AddProperty(new TextObject("{=ftoSCQ0x}Length", (Dictionary<string, object>)null), (float)val2.WeaponLength / 147f, val2.WeaponLength);
				AddProperty(new TextObject("{=s31DnnAf}Damage", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedThrustDamage(val2, itemModifier) / 94f, WeaponComponentDataExtensions.GetModifiedThrustDamage(val2, itemModifier));
				AddProperty(new TextObject("{=QfTt7YRB}Fire Rate", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedMissileSpeed(val2, itemModifier) / 115f, WeaponComponentDataExtensions.GetModifiedMissileSpeed(val2, itemModifier));
				AddProperty(new TextObject("{=TAnabTdy}Accuracy", (Dictionary<string, object>)null), (float)val2.Accuracy / 300f, val2.Accuracy);
				AddProperty(new TextObject("{=b31ITmm0}Stack Amnt.", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedStackCount(val2, itemModifier) / 40f, WeaponComponentDataExtensions.GetModifiedStackCount(val2, itemModifier));
			}
			if ((int)itemTypeFromWeaponClass == 8)
			{
				AddProperty(new TextObject("{=6GSXsdeX}Speed", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedThrustSpeed(val2, itemModifier) / 120f, WeaponComponentDataExtensions.GetModifiedThrustSpeed(val2, itemModifier));
				AddProperty(new TextObject("{=GGseMDd3}Durability", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedMaximumHitPoints(val2, itemModifier) / 500f, WeaponComponentDataExtensions.GetModifiedMaximumHitPoints(val2, itemModifier));
				AddProperty(new TextObject("{=ahiBhAqU}Armor", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedArmor(val2, itemModifier) / 40f, WeaponComponentDataExtensions.GetModifiedArmor(val2, itemModifier));
				AddProperty(new TextObject("{=4Dd2xgPm}Weight", (Dictionary<string, object>)null), item.Weight / 40f, (int)item.Weight);
			}
			if ((int)itemTypeFromWeaponClass == 9 || (int)itemTypeFromWeaponClass == 10 || (int)itemTypeFromWeaponClass == 11)
			{
				int num = 0;
				float num2 = 0f;
				int num3 = 0;
				for (EquipmentIndex val3 = (EquipmentIndex)0; (int)val3 < 4; val3 = (EquipmentIndex)(val3 + 1))
				{
					val = equipment[val3];
					ItemObject item2 = ((EquipmentElement)(ref val)).Item;
					val = equipment[val3];
					ItemModifier itemModifier2 = ((EquipmentElement)(ref val)).ItemModifier;
					if (item2 != null && item2.PrimaryWeapon.IsAmmo)
					{
						num += WeaponComponentDataExtensions.GetModifiedStackCount(item2.PrimaryWeapon, itemModifier2);
						num3 += WeaponComponentDataExtensions.GetModifiedThrustDamage(item2.PrimaryWeapon, itemModifier2);
						num2 += 1f;
					}
				}
				num3 = MathF.Round((float)num3 / num2);
				AddProperty(new TextObject("{=ftoSCQ0x}Length", (Dictionary<string, object>)null), (float)val2.WeaponLength / 123f, val2.WeaponLength);
				AddProperty(new TextObject("{=s31DnnAf}Damage", (Dictionary<string, object>)null), (float)(WeaponComponentDataExtensions.GetModifiedThrustDamage(val2, itemModifier) + num3) / 70f, WeaponComponentDataExtensions.GetModifiedThrustDamage(val2, itemModifier) + num3);
				AddProperty(new TextObject("{=QfTt7YRB}Fire Rate", (Dictionary<string, object>)null), (float)WeaponComponentDataExtensions.GetModifiedSwingSpeed(val2, itemModifier) / 120f, WeaponComponentDataExtensions.GetModifiedSwingSpeed(val2, itemModifier));
				AddProperty(new TextObject("{=TAnabTdy}Accuracy", (Dictionary<string, object>)null), (float)val2.Accuracy / 105f, val2.Accuracy);
				AddProperty(new TextObject("{=yUpH2mQ4}Ammo", (Dictionary<string, object>)null), (float)num / 90f, num);
			}
			((Collection<ArmoryItemFlagVM>)(object)ItemInformationList).Clear();
			List<(string, TextObject)> weaponFlagDetails = GetWeaponFlagDetails(val2.WeaponFlags);
			for (int i = 0; i < weaponFlagDetails.Count; i++)
			{
				ArmoryItemFlagVM item3 = new ArmoryItemFlagVM(weaponFlagDetails[i].Item1, weaponFlagDetails[i].Item2);
				((Collection<ArmoryItemFlagVM>)(object)ItemInformationList).Add(item3);
			}
		}
		if (item.HorseComponent != null)
		{
			EquipmentElement val4 = equipment[(EquipmentIndex)10];
			EquipmentElement val5 = equipment[(EquipmentIndex)11];
			int modifiedMountCharge = ((EquipmentElement)(ref val4)).GetModifiedMountCharge(ref val5);
			int num4 = (int)(4.33f * (float)((EquipmentElement)(ref val4)).GetModifiedMountSpeed(ref val5));
			int modifiedMountManeuver = ((EquipmentElement)(ref val4)).GetModifiedMountManeuver(ref val5);
			int modifiedMountHitPoints = ((EquipmentElement)(ref val4)).GetModifiedMountHitPoints();
			int modifiedMountBodyArmor = ((EquipmentElement)(ref val5)).GetModifiedMountBodyArmor();
			AddProperty(new TextObject("{=DAVb2Pzg}Charge Dmg.", (Dictionary<string, object>)null), (float)modifiedMountCharge / 35f, modifiedMountCharge);
			AddProperty(new TextObject("{=6GSXsdeX}Speed", (Dictionary<string, object>)null), (float)num4 / 303.1f, num4);
			AddProperty(new TextObject("{=rg7OuWS2}Maneuver", (Dictionary<string, object>)null), (float)modifiedMountManeuver / 70f, modifiedMountManeuver);
			AddProperty(new TextObject("{=oBbiVeKE}Hit Points", (Dictionary<string, object>)null), (float)modifiedMountHitPoints / 300f, modifiedMountHitPoints);
			AddProperty(new TextObject("{=kftE5nvv}Horse Armor", (Dictionary<string, object>)null), (float)modifiedMountBodyArmor / 100f, modifiedMountBodyArmor);
		}
	}

	private static List<(string, TextObject)> GetWeaponFlagDetails(WeaponFlags weaponFlags)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		List<(string, TextObject)> list = new List<(string, TextObject)>();
		if (Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)65536))
		{
			string item = "WeaponFlagIcons\\bonus_against_shield";
			TextObject item2 = GameTexts.FindText("str_inventory_flag_bonus_against_shield", (string)null);
			list.Add((item, item2));
		}
		if (Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)67108864))
		{
			string item = "WeaponFlagIcons\\can_knock_down";
			TextObject item2 = GameTexts.FindText("str_inventory_flag_can_knockdown", (string)null);
			list.Add((item, item2));
		}
		if (Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)16777216) && !Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)33554432))
		{
			string item = "WeaponFlagIcons\\can_dismount";
			TextObject item2 = GameTexts.FindText("str_inventory_flag_can_dismount", (string)null);
			list.Add((item, item2));
		}
		if (Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)33554432) && !Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)16777216))
		{
			string item = "WeaponFlagIcons\\can_dismount";
			TextObject item2 = GameTexts.FindText("str_inventory_flag_can_hook", (string)null);
			list.Add((item, item2));
		}
		if (Extensions.HasAllFlags<WeaponFlags>(weaponFlags, (WeaponFlags)50331648))
		{
			string item = "WeaponFlagIcons\\can_dismount";
			TextObject item2 = new TextObject("{=7HA99oUg}Both swing and thrust attacks can dismount riders", (Dictionary<string, object>)null);
			list.Add((item, item2));
		}
		if (Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)134217728))
		{
			string item = "WeaponFlagIcons\\can_crush_through";
			TextObject item2 = GameTexts.FindText("str_inventory_flag_can_crush_through", (string)null);
			list.Add((item, item2));
		}
		if (Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)32))
		{
			string item = "WeaponFlagIcons\\not_usable_with_two_hand";
			TextObject item2 = GameTexts.FindText("str_inventory_flag_not_usable_two_hand", (string)null);
			list.Add((item, item2));
		}
		if (Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)16))
		{
			string item = "WeaponFlagIcons\\not_usable_with_one_hand";
			TextObject item2 = GameTexts.FindText("str_inventory_flag_not_usable_one_hand", (string)null);
			list.Add((item, item2));
		}
		if (Extensions.HasAnyFlag<WeaponFlags>(weaponFlags, (WeaponFlags)262144))
		{
			string item = "WeaponFlagIcons\\cant_reload_on_horseback";
			TextObject item2 = GameTexts.FindText("str_inventory_flag_cant_reload_on_horseback", (string)null);
			list.Add((item, item2));
		}
		return list;
	}

	private void AddProperty(TextObject name, float fraction, int value)
	{
		((Collection<ShallowItemPropertyVM>)(object)PropertyList).Add(new ShallowItemPropertyVM(name, MathF.Round(fraction * 1000f), value));
	}

	private static ItemGroup GetItemGroupType(ItemObject item)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected I4, but got Unknown
		if (item.WeaponComponent != null)
		{
			WeaponClass weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
			switch (weaponClass - 2)
			{
			case 14:
				return ItemGroup.Bow;
			case 15:
				return ItemGroup.Crossbow;
			case 7:
			case 8:
			case 9:
				return ItemGroup.Spear;
			case 0:
			case 1:
				return ItemGroup.Sword;
			case 2:
			case 3:
				return ItemGroup.Axe;
			case 4:
			case 6:
				return ItemGroup.Mace;
			case 21:
				return ItemGroup.Javelin;
			case 19:
				return ItemGroup.ThrowingAxe;
			case 20:
				return ItemGroup.ThrowingKnife;
			case 26:
			case 27:
				return ItemGroup.Shield;
			case 10:
			case 11:
			case 12:
			case 13:
			case 23:
				return ItemGroup.Ammo;
			case 16:
			case 17:
			case 25:
				return ItemGroup.Stone;
			default:
				return ItemGroup.None;
			}
		}
		if (item.HasHorseComponent)
		{
			return ItemGroup.Mount;
		}
		return ItemGroup.None;
	}

	[UsedImplicitly]
	public void OnSelect()
	{
		_onSelect(this);
	}

	public static bool IsItemUsageApplicable(WeaponComponentData weapon)
	{
		WeaponDescription obj = ((weapon != null && weapon.WeaponDescriptionId != null) ? MBObjectManager.Instance.GetObject<WeaponDescription>(weapon.WeaponDescriptionId) : null);
		if (obj == null)
		{
			return false;
		}
		return !obj.IsHiddenFromUI;
	}
}
