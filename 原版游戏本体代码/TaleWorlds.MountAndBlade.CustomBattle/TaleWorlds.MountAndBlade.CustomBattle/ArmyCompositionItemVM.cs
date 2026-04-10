using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.CustomBattle;

public class ArmyCompositionItemVM : ViewModel
{
	public enum CompositionType
	{
		MeleeInfantry,
		RangedInfantry,
		MeleeCavalry,
		RangedCavalry
	}

	private readonly MBReadOnlyList<SkillObject> _allSkills;

	private readonly List<BasicCharacterObject> _allCharacterObjects;

	private readonly Action<int, int> _onCompositionValueChanged;

	private readonly TroopTypeSelectionPopUpVM _troopTypeSelectionPopUp;

	private BasicCultureObject _culture;

	private readonly CompositionType _type;

	private readonly int[] _compositionValues;

	private MBBindingList<CustomBattleTroopTypeVM> _troopTypes;

	private HintViewModel _invalidHint;

	private HintViewModel _addTroopTypeHint;

	private bool _isLocked;

	private bool _isValid;

	private string _compositionValuePercentageText;

	[DataSourceProperty]
	public MBBindingList<CustomBattleTroopTypeVM> TroopTypes
	{
		get
		{
			return _troopTypes;
		}
		set
		{
			if (value != _troopTypes)
			{
				_troopTypes = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CustomBattleTroopTypeVM>>(value, "TroopTypes");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel InvalidHint
	{
		get
		{
			return _invalidHint;
		}
		set
		{
			if (value != _invalidHint)
			{
				_invalidHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "InvalidHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel AddTroopTypeHint
	{
		get
		{
			return _addTroopTypeHint;
		}
		set
		{
			if (value != _addTroopTypeHint)
			{
				_addTroopTypeHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "AddTroopTypeHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsLocked
	{
		get
		{
			return _isLocked;
		}
		set
		{
			if (value != _isLocked)
			{
				_isLocked = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsLocked");
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
			OnValidityChanged(value);
		}
	}

	[DataSourceProperty]
	public int CompositionValue
	{
		get
		{
			return _compositionValues[(int)_type];
		}
		set
		{
			if (value != _compositionValues[(int)_type])
			{
				_onCompositionValueChanged(value, (int)_type);
			}
		}
	}

	[DataSourceProperty]
	public string CompositionValuePercentageText
	{
		get
		{
			return _compositionValuePercentageText;
		}
		set
		{
			if (value != _compositionValuePercentageText)
			{
				_compositionValuePercentageText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CompositionValuePercentageText");
			}
		}
	}

	public ArmyCompositionItemVM(CompositionType type, List<BasicCharacterObject> allCharacterObjects, MBReadOnlyList<SkillObject> allSkills, Action<int, int> onCompositionValueChanged, TroopTypeSelectionPopUpVM troopTypeSelectionPopUp, int[] compositionValues)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		_allCharacterObjects = allCharacterObjects;
		_allSkills = allSkills;
		_onCompositionValueChanged = onCompositionValueChanged;
		_troopTypeSelectionPopUp = troopTypeSelectionPopUp;
		_type = type;
		_compositionValues = compositionValues;
		TroopTypes = new MBBindingList<CustomBattleTroopTypeVM>();
		InvalidHint = new HintViewModel(new TextObject("{=iSQTtNUD}This faction doesn't have this troop type.", (Dictionary<string, object>)null), (string)null);
		AddTroopTypeHint = new HintViewModel(new TextObject("{=eMbuGGus}Select troops to spawn in formation.", (Dictionary<string, object>)null), (string)null);
		UpdatePercentageText(_compositionValues[(int)_type]);
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
	}

	public void SetCurrentSelectedCulture(BasicCultureObject culture)
	{
		IsLocked = false;
		_culture = culture;
		PopulateTroopTypes();
	}

	public void ExecuteRandomize(int compositionValue)
	{
		IsValid = true;
		IsLocked = false;
		CompositionValue = compositionValue;
		IsValid = ((Collection<CustomBattleTroopTypeVM>)(object)TroopTypes).Count > 0;
		TroopTypes.ApplyActionOnAllItems((Action<CustomBattleTroopTypeVM>)delegate(CustomBattleTroopTypeVM x)
		{
			x.ExecuteRandomize();
		});
		if (!((IEnumerable<CustomBattleTroopTypeVM>)TroopTypes).Any((CustomBattleTroopTypeVM x) => x.IsSelected) && IsValid)
		{
			((Collection<CustomBattleTroopTypeVM>)(object)TroopTypes)[0].IsSelected = true;
		}
	}

	public void ExecuteAddTroopTypes()
	{
		string title = ((object)GameTexts.FindText("str_custom_battle_choose_troop", _type.ToString())).ToString();
		_troopTypeSelectionPopUp?.OpenPopUp(title, TroopTypes);
	}

	public void RefreshCompositionValue()
	{
		((ViewModel)this).OnPropertyChanged("CompositionValue");
		UpdatePercentageText(_compositionValues[(int)_type]);
	}

	private void UpdatePercentageText(int percentage)
	{
		int num = (int)MathF.Clamp((float)percentage, 0f, 100f);
		CompositionValuePercentageText = ((object)GameTexts.FindText("str_NUMBER_percent", (string)null).SetTextVariable("NUMBER", num)).ToString();
	}

	private void OnValidityChanged(bool value)
	{
		IsLocked = false;
		if (!value)
		{
			CompositionValue = 0;
		}
		IsLocked = !value;
	}

	private void PopulateTroopTypes()
	{
		((Collection<CustomBattleTroopTypeVM>)(object)TroopTypes).Clear();
		MBReadOnlyList<BasicCharacterObject> defaultCharacters = GetDefaultCharacters();
		foreach (BasicCharacterObject allCharacterObject in _allCharacterObjects)
		{
			if (IsValidUnitItem(allCharacterObject))
			{
				((Collection<CustomBattleTroopTypeVM>)(object)TroopTypes).Add(new CustomBattleTroopTypeVM(allCharacterObject, _troopTypeSelectionPopUp.OnItemSelectionToggled, GetTroopTypeIconData(allCharacterObject, _type), _allSkills, ((List<BasicCharacterObject>)(object)defaultCharacters).Contains(allCharacterObject)));
			}
		}
		IsValid = ((Collection<CustomBattleTroopTypeVM>)(object)TroopTypes).Count > 0;
		if (IsValid && !((IEnumerable<CustomBattleTroopTypeVM>)TroopTypes).Any((CustomBattleTroopTypeVM x) => x.IsDefault))
		{
			((Collection<CustomBattleTroopTypeVM>)(object)TroopTypes)[0].IsDefault = true;
		}
		TroopTypes.ApplyActionOnAllItems((Action<CustomBattleTroopTypeVM>)delegate(CustomBattleTroopTypeVM x)
		{
			x.IsSelected = x.IsDefault;
		});
	}

	private bool IsValidUnitItem(BasicCharacterObject o)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I4
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		if (o != null && _culture == o.Culture)
		{
			switch (_type)
			{
			case CompositionType.MeleeInfantry:
				if ((int)o.DefaultFormationClass != 0)
				{
					return (int)o.DefaultFormationClass == 5;
				}
				return true;
			case CompositionType.RangedInfantry:
				return (int)o.DefaultFormationClass == 1;
			case CompositionType.MeleeCavalry:
				if ((int)o.DefaultFormationClass != 2 && (int)o.DefaultFormationClass != 7)
				{
					return (int)o.DefaultFormationClass == 6;
				}
				return true;
			case CompositionType.RangedCavalry:
				return (int)o.DefaultFormationClass == 3;
			default:
				return false;
			}
		}
		return false;
	}

	private MBReadOnlyList<BasicCharacterObject> GetDefaultCharacters()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		MBList<BasicCharacterObject> val = new MBList<BasicCharacterObject>();
		FormationClass formation = (FormationClass)10;
		switch (_type)
		{
		case CompositionType.MeleeInfantry:
			formation = (FormationClass)0;
			break;
		case CompositionType.RangedInfantry:
			formation = (FormationClass)1;
			break;
		case CompositionType.MeleeCavalry:
			formation = (FormationClass)2;
			break;
		case CompositionType.RangedCavalry:
			formation = (FormationClass)3;
			break;
		}
		((List<BasicCharacterObject>)(object)val).Add(CustomBattleHelper.GetDefaultTroopOfFormationForFaction(_culture, formation));
		return (MBReadOnlyList<BasicCharacterObject>)(object)val;
	}

	public static StringItemWithHintVM GetTroopTypeIconData(BasicCharacterObject basicCharacterObject, CompositionType type, bool isBig = false)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		bool flag = false;
		if (basicCharacterObject != null)
		{
			flag = ((MBObjectBase)basicCharacterObject).StringId.Contains("marine") || ((MBObjectBase)basicCharacterObject.Culture).StringId.Contains("nord");
		}
		TextObject val = new TextObject("{=!}{TYPENAME}{MARINER}{BIG}", (Dictionary<string, object>)null);
		TextObject val2;
		switch (type)
		{
		case CompositionType.RangedCavalry:
			val.SetTextVariable("TYPENAME", "horse_archer");
			val2 = GameTexts.FindText("str_troop_type_name", "HorseArcher");
			break;
		case CompositionType.RangedInfantry:
		{
			val.SetTextVariable("TYPENAME", "bow");
			string text2 = (flag ? "Ranged_Mariner" : "Ranged");
			val2 = GameTexts.FindText("str_troop_type_name", text2);
			break;
		}
		case CompositionType.MeleeCavalry:
			val.SetTextVariable("TYPENAME", "cavalry");
			val2 = GameTexts.FindText("str_troop_type_name", "Cavalry");
			break;
		case CompositionType.MeleeInfantry:
		{
			val.SetTextVariable("TYPENAME", "infantry");
			string text = (flag ? "Infantry_Mariner" : "Infantry");
			val2 = GameTexts.FindText("str_troop_type_name", text);
			break;
		}
		default:
			return new StringItemWithHintVM("", (TextObject)null);
		}
		val.SetTextVariable("MARINER", flag ? "_mariner" : "");
		val.SetTextVariable("BIG", isBig ? "_big" : "");
		return new StringItemWithHintVM("General\\TroopTypeIcons\\icon_troop_type_" + ((object)val).ToString(), new TextObject("{=!}" + ((object)val2).ToString(), (Dictionary<string, object>)null));
	}
}
