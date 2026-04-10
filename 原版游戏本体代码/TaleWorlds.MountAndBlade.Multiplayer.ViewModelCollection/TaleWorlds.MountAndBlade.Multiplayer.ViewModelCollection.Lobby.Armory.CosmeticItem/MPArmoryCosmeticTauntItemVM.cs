using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics.CosmeticTypes;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;

public class MPArmoryCosmeticTauntItemVM : MPArmoryCosmeticItemBaseVM
{
	private bool _isSelected;

	private bool _requiresOnFoot;

	private float _previewAnimationRatio;

	private string _selectSlotText;

	private string _cancelEquipText;

	private string _tauntId;

	private HintViewModel _blocksMovementOnUsageHint;

	private MBBindingList<StringItemWithEnabledAndHintVM> _tauntUsages;

	public MPArmoryCosmeticsVM.TauntCategoryFlag TauntCategory { get; }

	public TauntCosmeticElement TauntCosmeticElement { get; }

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
				UpdatePreviewAndActionTexts();
			}
		}
	}

	[DataSourceProperty]
	public bool RequiresOnFoot
	{
		get
		{
			return _requiresOnFoot;
		}
		set
		{
			if (value != _requiresOnFoot)
			{
				_requiresOnFoot = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "RequiresOnFoot");
			}
		}
	}

	[DataSourceProperty]
	public float PreviewAnimationRatio
	{
		get
		{
			return _previewAnimationRatio;
		}
		set
		{
			if (value != _previewAnimationRatio)
			{
				_previewAnimationRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PreviewAnimationRatio");
			}
		}
	}

	[DataSourceProperty]
	public string SelectSlotText
	{
		get
		{
			return _selectSlotText;
		}
		set
		{
			if (value != _selectSlotText)
			{
				_selectSlotText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SelectSlotText");
				UpdatePreviewAndActionTexts();
			}
		}
	}

	[DataSourceProperty]
	public string CancelEquipText
	{
		get
		{
			return _cancelEquipText;
		}
		set
		{
			if (value != _cancelEquipText)
			{
				_cancelEquipText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelEquipText");
				UpdatePreviewAndActionTexts();
			}
		}
	}

	[DataSourceProperty]
	public string TauntID
	{
		get
		{
			return _tauntId;
		}
		set
		{
			if (value != _tauntId)
			{
				_tauntId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TauntID");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringItemWithEnabledAndHintVM> TauntUsages
	{
		get
		{
			return _tauntUsages;
		}
		set
		{
			if (value != _tauntUsages)
			{
				_tauntUsages = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringItemWithEnabledAndHintVM>>(value, "TauntUsages");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel BlocksMovementOnUsageHint
	{
		get
		{
			return _blocksMovementOnUsageHint;
		}
		set
		{
			if (value != _blocksMovementOnUsageHint)
			{
				_blocksMovementOnUsageHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "BlocksMovementOnUsageHint");
			}
		}
	}

	public MPArmoryCosmeticTauntItemVM(string tauntId, CosmeticElement cosmetic, string cosmeticID)
		: base(cosmetic, cosmeticID, (CosmeticType)3)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		TauntID = tauntId;
		TauntCosmeticElement = (TauntCosmeticElement)(object)((cosmetic is TauntCosmeticElement) ? cosmetic : null);
		TauntCategory = GetCategoryOfTaunt();
		TauntUsages = new MBBindingList<StringItemWithEnabledAndHintVM>();
		RefreshTauntUsages();
		BlocksMovementOnUsageHint = new HintViewModel();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		base.RefreshValues();
		if (BlocksMovementOnUsageHint != null)
		{
			BlocksMovementOnUsageHint.HintText = new TextObject("{=BUQsaZMg}Blocks Movement on Usage", (Dictionary<string, object>)null);
		}
		SelectSlotText = ((object)new TextObject("{=4gfAb1ar}Select a Slot", (Dictionary<string, object>)null)).ToString();
		CancelEquipText = ((object)new TextObject("{=avYRbfHA}Cancel Equip", (Dictionary<string, object>)null)).ToString();
		TauntCosmeticElement tauntCosmeticElement = TauntCosmeticElement;
		base.Name = ((tauntCosmeticElement == null) ? null : ((object)tauntCosmeticElement.Name)?.ToString());
	}

	private void RefreshTauntUsages()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected O, but got Unknown
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Invalid comparison between Unknown and I4
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Expected O, but got Unknown
		((Collection<StringItemWithEnabledAndHintVM>)(object)TauntUsages).Clear();
		TauntUsageSet usageSet = TauntUsageManager.Instance.GetUsageSet(((CosmeticElement)TauntCosmeticElement).Id);
		TauntUsageFlag? obj;
		if (usageSet == null)
		{
			obj = null;
		}
		else
		{
			MBReadOnlyList<TauntUsage> usages = usageSet.GetUsages();
			obj = ((usages != null) ? new TauntUsageFlag?(((IEnumerable<TauntUsage>)usages).FirstOrDefault().UsageFlag) : ((TauntUsageFlag?)null));
		}
		TauntUsageFlag val = (TauntUsageFlag)(((int?)obj) ?? 0);
		TextObject val2 = new TextObject("{=aeDp7IEK}Usable with {USAGE}", (Dictionary<string, object>)null);
		if ((val & 0x20) == 0)
		{
			TextObject val3 = new TextObject("{=PiHpR4QL}One Handed", (Dictionary<string, object>)null);
			TextObject val4 = val2.CopyTextObject().SetTextVariable("USAGE", val3);
			((Collection<StringItemWithEnabledAndHintVM>)(object)TauntUsages).Add(new StringItemWithEnabledAndHintVM((Action<object>)null, "UsableWithOneHanded", true, (object)null, val4));
		}
		if ((val & 0x10) == 0)
		{
			TextObject val5 = new TextObject("{=t78atYqH}Two Handed", (Dictionary<string, object>)null);
			TextObject val6 = val2.CopyTextObject().SetTextVariable("USAGE", val5);
			((Collection<StringItemWithEnabledAndHintVM>)(object)TauntUsages).Add(new StringItemWithEnabledAndHintVM((Action<object>)null, "UsableWithTwoHanded", true, (object)null, val6));
		}
		if ((val & 0x80) == 0)
		{
			TextObject val7 = new TextObject("{=5rj7xQE4}Bow", (Dictionary<string, object>)null);
			TextObject val8 = val2.CopyTextObject().SetTextVariable("USAGE", val7);
			((Collection<StringItemWithEnabledAndHintVM>)(object)TauntUsages).Add(new StringItemWithEnabledAndHintVM((Action<object>)null, "UsableWithBow", true, (object)null, val8));
		}
		if ((val & 0x100) == 0)
		{
			TextObject val9 = new TextObject("{=TTWL7RLe}Crossbow", (Dictionary<string, object>)null);
			TextObject val10 = val2.CopyTextObject().SetTextVariable("USAGE", val9);
			((Collection<StringItemWithEnabledAndHintVM>)(object)TauntUsages).Add(new StringItemWithEnabledAndHintVM((Action<object>)null, "UsableWithCrossbow", true, (object)null, val10));
		}
		if ((val & 0x40) == 0)
		{
			TextObject val11 = new TextObject("{=Jd0Kq9lD}Shield", (Dictionary<string, object>)null);
			TextObject val12 = val2.CopyTextObject().SetTextVariable("USAGE", val11);
			((Collection<StringItemWithEnabledAndHintVM>)(object)TauntUsages).Add(new StringItemWithEnabledAndHintVM((Action<object>)null, "UsableWithShield", true, (object)null, val12));
		}
		if ((val & 8) == 0)
		{
			TextObject val13 = new TextObject("{=uGM8DWrm}Mount", (Dictionary<string, object>)null);
			TextObject val14 = val2.CopyTextObject().SetTextVariable("USAGE", val13);
			((Collection<StringItemWithEnabledAndHintVM>)(object)TauntUsages).Add(new StringItemWithEnabledAndHintVM((Action<object>)null, "UsableWithMount", true, (object)null, val14));
		}
		RequiresOnFoot = (val & 8) > 0;
	}

	private MPArmoryCosmeticsVM.TauntCategoryFlag GetCategoryOfTaunt()
	{
		TauntUsageSet usageSet = TauntUsageManager.Instance.GetUsageSet(((CosmeticElement)TauntCosmeticElement).Id);
		MBReadOnlyList<TauntUsage> val = ((usageSet != null) ? usageSet.GetUsages() : null);
		MPArmoryCosmeticsVM.TauntCategoryFlag tauntCategoryFlag = MPArmoryCosmeticsVM.TauntCategoryFlag.None;
		if (val == null || ((List<TauntUsage>)(object)val).Count <= 0)
		{
			return tauntCategoryFlag;
		}
		if (AnyUsageNotHaveFlag(val, (TauntUsageFlag)8))
		{
			tauntCategoryFlag |= MPArmoryCosmeticsVM.TauntCategoryFlag.UsableWithMount;
		}
		if (AnyUsageNotHaveFlag(val, (TauntUsageFlag)64))
		{
			tauntCategoryFlag |= MPArmoryCosmeticsVM.TauntCategoryFlag.UsableWithShield;
		}
		if (AnyUsageNotHaveFlag(val, (TauntUsageFlag)32))
		{
			tauntCategoryFlag |= MPArmoryCosmeticsVM.TauntCategoryFlag.UsableWithOneHanded;
		}
		if (AnyUsageNotHaveFlag(val, (TauntUsageFlag)16))
		{
			tauntCategoryFlag |= MPArmoryCosmeticsVM.TauntCategoryFlag.UsableWithTwoHanded;
		}
		if (AnyUsageNotHaveFlag(val, (TauntUsageFlag)128))
		{
			tauntCategoryFlag |= MPArmoryCosmeticsVM.TauntCategoryFlag.UsableWithBow;
		}
		if (AnyUsageNotHaveFlag(val, (TauntUsageFlag)256))
		{
			tauntCategoryFlag |= MPArmoryCosmeticsVM.TauntCategoryFlag.UsableWithCrossbow;
		}
		return tauntCategoryFlag;
	}

	private bool AllUsagesHaveFlag(MBReadOnlyList<TauntUsage> list, TauntUsageFlag flag)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((IEnumerable<TauntUsage>)list).All((TauntUsage u) => (u.UsageFlag & flag) > 0);
	}

	private bool AnyUsageHaveFlag(MBReadOnlyList<TauntUsage> list, TauntUsageFlag flag)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((IEnumerable<TauntUsage>)list).Any((TauntUsage u) => (u.UsageFlag & flag) > 0);
	}

	private bool AnyUsageNotHaveFlag(MBReadOnlyList<TauntUsage> list, TauntUsageFlag flag)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((IEnumerable<TauntUsage>)list).Any((TauntUsage u) => (u.UsageFlag & flag) == 0);
	}
}
