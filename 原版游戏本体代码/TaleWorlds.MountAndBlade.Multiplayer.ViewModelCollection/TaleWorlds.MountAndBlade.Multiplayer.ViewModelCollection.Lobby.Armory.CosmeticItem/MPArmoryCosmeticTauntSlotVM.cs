using System;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;

public class MPArmoryCosmeticTauntSlotVM : ViewModel
{
	public readonly int SlotIndex;

	private InputKeyItemVM _selectKeyVisual;

	private InputKeyItemVM _emptySlotKeyVisual;

	private bool _isAcceptingTaunts;

	private bool _isSelected;

	private bool _isEnabled;

	private bool _isEmpty;

	private bool _isFocused;

	private MPArmoryCosmeticTauntItemVM _assignedTauntItem;

	[DataSourceProperty]
	public InputKeyItemVM SelectKeyVisual
	{
		get
		{
			return _selectKeyVisual;
		}
		set
		{
			if (value != _selectKeyVisual)
			{
				_selectKeyVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "SelectKeyVisual");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM EmptySlotKeyVisual
	{
		get
		{
			return _emptySlotKeyVisual;
		}
		set
		{
			if (value != _emptySlotKeyVisual)
			{
				_emptySlotKeyVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "EmptySlotKeyVisual");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAcceptingTaunts
	{
		get
		{
			return _isAcceptingTaunts;
		}
		set
		{
			if (value != _isAcceptingTaunts)
			{
				_isAcceptingTaunts = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAcceptingTaunts");
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
			}
		}
	}

	[DataSourceProperty]
	public bool IsEmpty
	{
		get
		{
			return _isEmpty;
		}
		set
		{
			if (value != _isEmpty)
			{
				_isEmpty = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEmpty");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFocused
	{
		get
		{
			return _isFocused;
		}
		set
		{
			if (value != _isFocused)
			{
				_isFocused = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFocused");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryCosmeticTauntItemVM AssignedTauntItem
	{
		get
		{
			return _assignedTauntItem;
		}
		set
		{
			if (value != _assignedTauntItem)
			{
				_assignedTauntItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryCosmeticTauntItemVM>(value, "AssignedTauntItem");
			}
		}
	}

	public static event Action<MPArmoryCosmeticTauntSlotVM, bool> OnFocusChanged;

	public static event Action<MPArmoryCosmeticTauntSlotVM> OnSelected;

	public static event Action<MPArmoryCosmeticTauntSlotVM> OnPreview;

	public static event Action<MPArmoryCosmeticTauntSlotVM, MPArmoryCosmeticTauntItemVM, bool> OnTauntEquipped;

	public MPArmoryCosmeticTauntSlotVM(int slotIndex)
	{
		IsEmpty = true;
		SlotIndex = slotIndex;
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM selectKeyVisual = SelectKeyVisual;
		if (selectKeyVisual != null)
		{
			((ViewModel)selectKeyVisual).OnFinalize();
		}
	}

	public void AssignTauntItem(MPArmoryCosmeticTauntItemVM tauntItem, bool isSwapping = false)
	{
		MPArmoryCosmeticTauntItemVM assignedTauntItem = AssignedTauntItem;
		AssignedTauntItem = tauntItem;
		IsEmpty = tauntItem == null;
		if (!isSwapping && assignedTauntItem != null)
		{
			assignedTauntItem.IsUsed = false;
		}
		if (AssignedTauntItem != null)
		{
			((CosmeticElement)AssignedTauntItem.TauntCosmeticElement).UsageIndex = SlotIndex;
			AssignedTauntItem.IsUsed = true;
		}
		MPArmoryCosmeticTauntSlotVM.OnTauntEquipped?.Invoke(this, assignedTauntItem, isSwapping);
	}

	public void ExecuteSelect()
	{
		MPArmoryCosmeticTauntSlotVM.OnSelected?.Invoke(this);
	}

	public void ExecutePreview()
	{
		MPArmoryCosmeticTauntSlotVM.OnPreview?.Invoke(this);
	}

	public void ExecuteFocus()
	{
		MPArmoryCosmeticTauntSlotVM.OnFocusChanged?.Invoke(this, arg2: true);
	}

	public void ExecuteUnfocus()
	{
		MPArmoryCosmeticTauntSlotVM.OnFocusChanged?.Invoke(this, arg2: false);
	}

	public void SetSelectKeyVisual(HotKey hotKey)
	{
		SelectKeyVisual = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetEmptySlotKeyVisual(HotKey hotKey)
	{
		EmptySlotKeyVisual = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}
}
