using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics.CosmeticTypes;
using TaleWorlds.MountAndBlade.Diamond.Lobby;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticCategory;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory;

public class MPArmoryCosmeticsVM : ViewModel
{
	public enum ClothingCategory
	{
		Invalid = -1,
		ClothingCategoriesBegin = 0,
		All = 0,
		HeadArmor = 1,
		Cape = 2,
		BodyArmor = 3,
		HandArmor = 4,
		LegArmor = 5,
		ClothingCategoriesEnd = 6
	}

	[Flags]
	public enum TauntCategoryFlag
	{
		None = 0,
		UsableWithMount = 1,
		UsableWithOneHanded = 2,
		UsableWithTwoHanded = 4,
		UsableWithBow = 8,
		UsableWithCrossbow = 0x10,
		UsableWithShield = 0x20,
		All = 0x3F
	}

	public abstract class CosmeticItemComparer : IComparer<MPArmoryCosmeticItemBaseVM>
	{
		private bool _isAscending;

		protected int _sortMultiplier
		{
			get
			{
				if (!_isAscending)
				{
					return -1;
				}
				return 1;
			}
		}

		public void SetSortMode(bool isAscending)
		{
			_isAscending = isAscending;
		}

		public abstract int Compare(MPArmoryCosmeticItemBaseVM x, MPArmoryCosmeticItemBaseVM y);
	}

	private class CosmeticItemNameComparer : CosmeticItemComparer
	{
		public override int Compare(MPArmoryCosmeticItemBaseVM x, MPArmoryCosmeticItemBaseVM y)
		{
			return x.Name.CompareTo(y.Name) * base._sortMultiplier;
		}
	}

	private class CosmeticItemCostComparer : CosmeticItemComparer
	{
		public unsafe override int Compare(MPArmoryCosmeticItemBaseVM x, MPArmoryCosmeticItemBaseVM y)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			int num = x.Cost.CompareTo(y.Cost);
			if (num == 0)
			{
				if (x is MPArmoryCosmeticClothingItemVM mPArmoryCosmeticClothingItemVM && y is MPArmoryCosmeticClothingItemVM mPArmoryCosmeticClothingItemVM2)
				{
					EquipmentElement equipmentElement = mPArmoryCosmeticClothingItemVM.EquipmentElement;
					ItemTypeEnum itemType = ((EquipmentElement)(ref equipmentElement)).Item.ItemType;
					equipmentElement = mPArmoryCosmeticClothingItemVM2.EquipmentElement;
					num = ((Enum)(*(ItemTypeEnum*)(&itemType))/*cast due to .constrained prefix*/).CompareTo((object?)((EquipmentElement)(ref equipmentElement)).Item.ItemType);
				}
				else if (x is MPArmoryCosmeticTauntItemVM mPArmoryCosmeticTauntItemVM && y is MPArmoryCosmeticTauntItemVM mPArmoryCosmeticTauntItemVM2)
				{
					num = mPArmoryCosmeticTauntItemVM.Name.CompareTo(mPArmoryCosmeticTauntItemVM2.Name);
				}
			}
			return num * base._sortMultiplier;
		}
	}

	private class CosmeticItemRarityComparer : CosmeticItemComparer
	{
		public unsafe override int Compare(MPArmoryCosmeticItemBaseVM x, MPArmoryCosmeticItemBaseVM y)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			ref CosmeticRarity rarity = ref x.Cosmetic.Rarity;
			object target = y.Cosmetic.Rarity;
			int num = ((Enum)Unsafe.As<CosmeticRarity, CosmeticRarity>(ref rarity)/*cast due to .constrained prefix*/).CompareTo(target);
			if (num == 0)
			{
				if (x is MPArmoryCosmeticClothingItemVM mPArmoryCosmeticClothingItemVM && y is MPArmoryCosmeticClothingItemVM mPArmoryCosmeticClothingItemVM2)
				{
					EquipmentElement equipmentElement = mPArmoryCosmeticClothingItemVM.EquipmentElement;
					ItemTypeEnum itemType = ((EquipmentElement)(ref equipmentElement)).Item.ItemType;
					equipmentElement = mPArmoryCosmeticClothingItemVM2.EquipmentElement;
					num = ((Enum)(*(ItemTypeEnum*)(&itemType))/*cast due to .constrained prefix*/).CompareTo((object?)((EquipmentElement)(ref equipmentElement)).Item.ItemType);
				}
				else if (x is MPArmoryCosmeticTauntItemVM mPArmoryCosmeticTauntItemVM && y is MPArmoryCosmeticTauntItemVM mPArmoryCosmeticTauntItemVM2)
				{
					num = mPArmoryCosmeticTauntItemVM.Name.CompareTo(mPArmoryCosmeticTauntItemVM2.Name);
				}
			}
			return num * base._sortMultiplier;
		}
	}

	private class CosmeticItemCategoryComparer : CosmeticItemComparer
	{
		public unsafe override int Compare(MPArmoryCosmeticItemBaseVM x, MPArmoryCosmeticItemBaseVM y)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if (x is MPArmoryCosmeticClothingItemVM mPArmoryCosmeticClothingItemVM && y is MPArmoryCosmeticClothingItemVM mPArmoryCosmeticClothingItemVM2)
			{
				EquipmentElement equipmentElement = mPArmoryCosmeticClothingItemVM.EquipmentElement;
				ItemTypeEnum itemType = ((EquipmentElement)(ref equipmentElement)).Item.ItemType;
				equipmentElement = mPArmoryCosmeticClothingItemVM2.EquipmentElement;
				return ((Enum)(*(ItemTypeEnum*)(&itemType))/*cast due to .constrained prefix*/).CompareTo((object?)((EquipmentElement)(ref equipmentElement)).Item.ItemType) * base._sortMultiplier;
			}
			return 0;
		}
	}

	private readonly Func<List<IReadOnlyPerkObject>> _getSelectedPerks;

	private List<MPArmoryCosmeticItemBaseVM> _allCosmetics;

	private List<string> _ownedCosmetics;

	private Dictionary<string, List<string>> _usedCosmetics;

	private Equipment _selectedClassDefaultEquipment;

	private CosmeticItemComparer _currentItemComparer;

	private List<CosmeticItemComparer> _itemComparers;

	private Dictionary<ClothingCategory, MPArmoryClothingCosmeticCategoryVM> _clothingCategoriesLookup;

	private Dictionary<TauntCategoryFlag, MPArmoryTauntCosmeticCategoryVM> _tauntCategoriesLookup;

	private Dictionary<string, MPArmoryCosmeticItemBaseVM> _cosmeticItemsLookup;

	private MPHeroClass _selectedClass;

	private string _selectedTroopID;

	private bool _isLocalCosmeticsDirty;

	private bool _isNetworkCosmeticsDirty;

	private bool _isSendingCosmeticData;

	private bool _isRetrievingCosmeticData;

	private CosmeticType _currentCosmeticType;

	private ClothingCategory _currentClothingCategory;

	private TauntCategoryFlag _currentTauntCategory;

	private InputKeyItemVM _actionInputKey;

	private InputKeyItemVM _previewInputKey;

	private int _loot;

	private bool _isLoading;

	private bool _hasCosmeticInfoReceived;

	private bool _isManagingTaunts;

	private bool _isTauntAssignmentActive;

	private string _cosmeticInfoErrorText;

	private HintViewModel _allCategoriesHint;

	private HintViewModel _bodyCategoryHint;

	private HintViewModel _headCategoryHint;

	private HintViewModel _shoulderCategoryHint;

	private HintViewModel _handCategoryHint;

	private HintViewModel _legCategoryHint;

	private HintViewModel _resetPreviewHint;

	private MPArmoryCosmeticCategoryBaseVM _activeCategory;

	private MPArmoryCosmeticTauntSlotVM _selectedTauntSlot;

	private MPArmoryCosmeticTauntItemVM _selectedTauntItem;

	private SelectorVM<SelectorItemVM> _sortCategories;

	private SelectorVM<SelectorItemVM> _sortOrders;

	private MBBindingList<MPArmoryCosmeticTauntSlotVM> _tauntSlots;

	private MBBindingList<MPArmoryCosmeticCategoryBaseVM> _availableCategories;

	[DataSourceProperty]
	public InputKeyItemVM ActionInputKey
	{
		get
		{
			return _actionInputKey;
		}
		set
		{
			if (value != _actionInputKey)
			{
				_actionInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "ActionInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM PreviewInputKey
	{
		get
		{
			return _previewInputKey;
		}
		set
		{
			if (value != _previewInputKey)
			{
				_previewInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "PreviewInputKey");
			}
		}
	}

	[DataSourceProperty]
	public int Loot
	{
		get
		{
			return _loot;
		}
		set
		{
			if (value != _loot)
			{
				_loot = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Loot");
			}
		}
	}

	[DataSourceProperty]
	public bool IsLoading
	{
		get
		{
			return _isLoading;
		}
		set
		{
			if (value != _isLoading)
			{
				_isLoading = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsLoading");
			}
		}
	}

	[DataSourceProperty]
	public bool HasCosmeticInfoReceived
	{
		get
		{
			return _hasCosmeticInfoReceived;
		}
		set
		{
			if (value != _hasCosmeticInfoReceived)
			{
				_hasCosmeticInfoReceived = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasCosmeticInfoReceived");
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
			}
		}
	}

	[DataSourceProperty]
	public string CosmeticInfoErrorText
	{
		get
		{
			return _cosmeticInfoErrorText;
		}
		set
		{
			if (value != _cosmeticInfoErrorText)
			{
				_cosmeticInfoErrorText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CosmeticInfoErrorText");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel AllCategoriesHint
	{
		get
		{
			return _allCategoriesHint;
		}
		set
		{
			if (value != _allCategoriesHint)
			{
				_allCategoriesHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "AllCategoriesHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel BodyCategoryHint
	{
		get
		{
			return _bodyCategoryHint;
		}
		set
		{
			if (value != _bodyCategoryHint)
			{
				_bodyCategoryHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "BodyCategoryHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel HeadCategoryHint
	{
		get
		{
			return _headCategoryHint;
		}
		set
		{
			if (value != _headCategoryHint)
			{
				_headCategoryHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "HeadCategoryHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ShoulderCategoryHint
	{
		get
		{
			return _shoulderCategoryHint;
		}
		set
		{
			if (value != _shoulderCategoryHint)
			{
				_shoulderCategoryHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ShoulderCategoryHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel HandCategoryHint
	{
		get
		{
			return _handCategoryHint;
		}
		set
		{
			if (value != _handCategoryHint)
			{
				_handCategoryHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "HandCategoryHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel LegCategoryHint
	{
		get
		{
			return _legCategoryHint;
		}
		set
		{
			if (value != _legCategoryHint)
			{
				_legCategoryHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "LegCategoryHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel ResetPreviewHint
	{
		get
		{
			return _resetPreviewHint;
		}
		set
		{
			if (value != _resetPreviewHint)
			{
				_resetPreviewHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "ResetPreviewHint");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryCosmeticCategoryBaseVM ActiveCategory
	{
		get
		{
			return _activeCategory;
		}
		set
		{
			if (value != _activeCategory)
			{
				_activeCategory = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryCosmeticCategoryBaseVM>(value, "ActiveCategory");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryCosmeticTauntSlotVM SelectedTauntSlot
	{
		get
		{
			return _selectedTauntSlot;
		}
		set
		{
			if (value != _selectedTauntSlot)
			{
				_selectedTauntSlot = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryCosmeticTauntSlotVM>(value, "SelectedTauntSlot");
				UpdateTauntAssignmentState();
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryCosmeticTauntItemVM SelectedTauntItem
	{
		get
		{
			return _selectedTauntItem;
		}
		set
		{
			if (value != _selectedTauntItem)
			{
				_selectedTauntItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryCosmeticTauntItemVM>(value, "SelectedTauntItem");
				UpdateTauntAssignmentState();
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<SelectorItemVM> SortCategories
	{
		get
		{
			return _sortCategories;
		}
		set
		{
			if (value != _sortCategories)
			{
				_sortCategories = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<SelectorItemVM>>(value, "SortCategories");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<SelectorItemVM> SortOrders
	{
		get
		{
			return _sortOrders;
		}
		set
		{
			if (value != _sortOrders)
			{
				_sortOrders = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<SelectorItemVM>>(value, "SortOrders");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPArmoryCosmeticTauntSlotVM> TauntSlots
	{
		get
		{
			return _tauntSlots;
		}
		set
		{
			if (value != _tauntSlots)
			{
				_tauntSlots = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPArmoryCosmeticTauntSlotVM>>(value, "TauntSlots");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPArmoryCosmeticCategoryBaseVM> AvailableCategories
	{
		get
		{
			return _availableCategories;
		}
		set
		{
			if (value != _availableCategories)
			{
				_availableCategories = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPArmoryCosmeticCategoryBaseVM>>(value, "AvailableCategories");
			}
		}
	}

	public static event Action<MPArmoryCosmeticItemBaseVM> OnCosmeticPreview;

	public static event Action<MPArmoryCosmeticItemBaseVM> OnRemoveCosmeticFromPreview;

	public static event Action<List<EquipmentElement>> OnEquipmentRefreshed;

	public static event Action OnTauntAssignmentRefresh;

	public MPArmoryCosmeticsVM(Func<List<IReadOnlyPerkObject>> getSelectedPerks)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Expected O, but got Unknown
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected O, but got Unknown
		_getSelectedPerks = getSelectedPerks;
		_usedCosmetics = new Dictionary<string, List<string>>();
		_ownedCosmetics = new List<string>();
		_clothingCategoriesLookup = new Dictionary<ClothingCategory, MPArmoryClothingCosmeticCategoryVM>();
		_tauntCategoriesLookup = new Dictionary<TauntCategoryFlag, MPArmoryTauntCosmeticCategoryVM>();
		_cosmeticItemsLookup = new Dictionary<string, MPArmoryCosmeticItemBaseVM>();
		AvailableCategories = new MBBindingList<MPArmoryCosmeticCategoryBaseVM>();
		SortCategories = new SelectorVM<SelectorItemVM>(0, (Action<SelectorVM<SelectorItemVM>>)OnSortCategoryUpdated);
		SortOrders = new SelectorVM<SelectorItemVM>(0, (Action<SelectorVM<SelectorItemVM>>)OnSortOrderUpdated);
		TauntSlots = new MBBindingList<MPArmoryCosmeticTauntSlotVM>();
		InitializeCosmeticItemComparers();
		InitializeAllCosmetics();
		InitializeCallbacks();
		IsLoading = true;
		SortCategories.AddItem(new SelectorItemVM(new TextObject("{=J2wEawTl}Category", (Dictionary<string, object>)null)));
		SortCategories.AddItem(new SelectorItemVM(new TextObject("{=ebUrBmHK}Price", (Dictionary<string, object>)null)));
		SortCategories.AddItem(new SelectorItemVM(new TextObject("{=bD8nTS86}Rarity", (Dictionary<string, object>)null)));
		SortCategories.AddItem(new SelectorItemVM(new TextObject("{=PDdh1sBj}Name", (Dictionary<string, object>)null)));
		SortCategories.SelectedIndex = 0;
		SortOrders.AddItem(new SelectorItemVM(new TextObject("{=mOmFzU78}Ascending", (Dictionary<string, object>)null)));
		SortOrders.AddItem(new SelectorItemVM(new TextObject("{=FgFUsncP}Descending", (Dictionary<string, object>)null)));
		SortOrders.SelectedIndex = 0;
		RefreshAvailableCategoriesBy((CosmeticType)0);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		((ViewModel)SortCategories).RefreshValues();
		((ViewModel)SortOrders).RefreshValues();
		CosmeticInfoErrorText = ((object)new TextObject("{=ehkVpzpa}Unable to get cosmetic information", (Dictionary<string, object>)null)).ToString();
		AllCategoriesHint = new HintViewModel(new TextObject("{=yfa7tpbK}All", (Dictionary<string, object>)null), (string)null);
		BodyCategoryHint = new HintViewModel(GameTexts.FindText("str_inventory_type_13", (string)null), (string)null);
		HeadCategoryHint = new HintViewModel(GameTexts.FindText("str_inventory_type_12", (string)null), (string)null);
		ShoulderCategoryHint = new HintViewModel(GameTexts.FindText("str_inventory_type_22", (string)null), (string)null);
		HandCategoryHint = new HintViewModel(GameTexts.FindText("str_inventory_type_15", (string)null), (string)null);
		LegCategoryHint = new HintViewModel(GameTexts.FindText("str_inventory_type_14", (string)null), (string)null);
		ResetPreviewHint = new HintViewModel(new TextObject("{=imUnCFgZ}Reset preview", (Dictionary<string, object>)null), (string)null);
		_allCosmetics.ForEach(delegate(MPArmoryCosmeticItemBaseVM c)
		{
			((ViewModel)c).RefreshValues();
		});
		AvailableCategories.ApplyActionOnAllItems((Action<MPArmoryCosmeticCategoryBaseVM>)delegate(MPArmoryCosmeticCategoryBaseVM c)
		{
			((ViewModel)c).RefreshValues();
		});
	}

	private void InitializeCallbacks()
	{
		MPArmoryClothingCosmeticCategoryVM.OnSelected += OnClothingCosmeticCategorySelected;
		MPArmoryTauntCosmeticCategoryVM.OnSelected += OnTauntCosmeticCategorySelected;
		MPArmoryCosmeticItemBaseVM.OnPreviewed += EquipItemOnHeroPreview;
		MPArmoryCosmeticItemBaseVM.OnEquipped += OnCosmeticEquipRequested;
		MPArmoryCosmeticTauntSlotVM.OnFocusChanged += OnTauntSlotFocusChanged;
		MPArmoryCosmeticTauntSlotVM.OnSelected += OnTauntSlotSelected;
		MPArmoryCosmeticTauntSlotVM.OnPreview += OnTauntSlotPreview;
		MPArmoryCosmeticTauntSlotVM.OnTauntEquipped += OnTauntItemEquipped;
	}

	private void FinalizeCallbacks()
	{
		MPArmoryClothingCosmeticCategoryVM.OnSelected -= OnClothingCosmeticCategorySelected;
		MPArmoryTauntCosmeticCategoryVM.OnSelected -= OnTauntCosmeticCategorySelected;
		MPArmoryCosmeticItemBaseVM.OnPreviewed -= EquipItemOnHeroPreview;
		MPArmoryCosmeticItemBaseVM.OnEquipped -= OnCosmeticEquipRequested;
		MPArmoryCosmeticTauntSlotVM.OnFocusChanged -= OnTauntSlotFocusChanged;
		MPArmoryCosmeticTauntSlotVM.OnSelected -= OnTauntSlotSelected;
		MPArmoryCosmeticTauntSlotVM.OnPreview -= OnTauntSlotPreview;
		MPArmoryCosmeticTauntSlotVM.OnTauntEquipped -= OnTauntItemEquipped;
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		FinalizeCallbacks();
		AvailableCategories.ApplyActionOnAllItems((Action<MPArmoryCosmeticCategoryBaseVM>)delegate(MPArmoryCosmeticCategoryBaseVM c)
		{
			((ViewModel)c).OnFinalize();
		});
	}

	public async void OnTick(float dt)
	{
		if (NetworkMain.GameClient == null)
		{
			_isNetworkCosmeticsDirty = false;
			_isLocalCosmeticsDirty = false;
		}
		if (!_isSendingCosmeticData && !_isRetrievingCosmeticData)
		{
			if (_isNetworkCosmeticsDirty)
			{
				RefreshCosmeticInfoFromNetworkAux();
				_isNetworkCosmeticsDirty = false;
			}
			if (_isLocalCosmeticsDirty)
			{
				await UpdateUsedCosmeticsAux();
				_isLocalCosmeticsDirty = false;
			}
		}
	}

	private void InitializeCosmeticItemComparers()
	{
		_itemComparers = new List<CosmeticItemComparer>
		{
			new CosmeticItemCategoryComparer(),
			new CosmeticItemCostComparer(),
			new CosmeticItemRarityComparer(),
			new CosmeticItemNameComparer()
		};
		_currentItemComparer = _itemComparers[0];
	}

	private void InitializeAllCosmetics()
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Invalid comparison between Unknown and I4
		_tauntCategoriesLookup.Clear();
		_tauntCategoriesLookup.Add(TauntCategoryFlag.All, new MPArmoryTauntCosmeticCategoryVM(TauntCategoryFlag.All));
		foreach (TauntCategoryFlag value in Enum.GetValues(typeof(TauntCategoryFlag)))
		{
			if (value > TauntCategoryFlag.None && value < TauntCategoryFlag.All)
			{
				_tauntCategoriesLookup.Add(value, new MPArmoryTauntCosmeticCategoryVM(value));
			}
		}
		_clothingCategoriesLookup.Clear();
		for (ClothingCategory clothingCategory = ClothingCategory.ClothingCategoriesBegin; clothingCategory < ClothingCategory.ClothingCategoriesEnd; clothingCategory++)
		{
			_clothingCategoriesLookup.Add(clothingCategory, new MPArmoryClothingCosmeticCategoryVM(clothingCategory));
		}
		_allCosmetics = new List<MPArmoryCosmeticItemBaseVM>();
		List<CosmeticElement> list = ((IEnumerable<CosmeticElement>)CosmeticsManager.CosmeticElementsList).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			ClothingCosmeticElement val;
			if ((int)list[i].Type == 0 && (val = (ClothingCosmeticElement)/*isinst with value type is only supported in some contexts*/) != null)
			{
				MPArmoryCosmeticClothingItemVM mPArmoryCosmeticClothingItemVM = new MPArmoryCosmeticClothingItemVM((CosmeticElement)(object)val, ((CosmeticElement)val).Id);
				mPArmoryCosmeticClothingItemVM.IsUnlocked = ((CosmeticElement)val).IsFree;
				mPArmoryCosmeticClothingItemVM.IsSelectable = true;
				_allCosmetics.Add(mPArmoryCosmeticClothingItemVM);
				_cosmeticItemsLookup.Add(((CosmeticElement)val).Id, mPArmoryCosmeticClothingItemVM);
				((Collection<MPArmoryCosmeticItemBaseVM>)(object)_clothingCategoriesLookup[mPArmoryCosmeticClothingItemVM.ClothingCategory].AvailableCosmetics).Add((MPArmoryCosmeticItemBaseVM)mPArmoryCosmeticClothingItemVM);
				((Collection<MPArmoryCosmeticItemBaseVM>)(object)_clothingCategoriesLookup[ClothingCategory.ClothingCategoriesBegin].AvailableCosmetics).Add((MPArmoryCosmeticItemBaseVM)mPArmoryCosmeticClothingItemVM);
			}
			else
			{
				TauntCosmeticElement val2;
				if ((int)list[i].Type != 3 || (val2 = (TauntCosmeticElement)/*isinst with value type is only supported in some contexts*/) == null)
				{
					continue;
				}
				MPArmoryCosmeticTauntItemVM mPArmoryCosmeticTauntItemVM = new MPArmoryCosmeticTauntItemVM(((CosmeticElement)val2).Id, (CosmeticElement)(object)val2, ((CosmeticElement)val2).Id);
				mPArmoryCosmeticTauntItemVM.IsUnlocked = ((CosmeticElement)val2).IsFree;
				mPArmoryCosmeticTauntItemVM.IsSelectable = true;
				_allCosmetics.Add(mPArmoryCosmeticTauntItemVM);
				_cosmeticItemsLookup.Add(((CosmeticElement)val2).Id, mPArmoryCosmeticTauntItemVM);
				foreach (TauntCategoryFlag value2 in Enum.GetValues(typeof(TauntCategoryFlag)))
				{
					if (value2 > TauntCategoryFlag.None && value2 <= TauntCategoryFlag.All && (mPArmoryCosmeticTauntItemVM.TauntCategory & value2) != TauntCategoryFlag.None)
					{
						((Collection<MPArmoryCosmeticItemBaseVM>)(object)_tauntCategoriesLookup[value2].AvailableCosmetics).Add((MPArmoryCosmeticItemBaseVM)mPArmoryCosmeticTauntItemVM);
					}
				}
			}
		}
		for (int j = 0; j < TauntCosmeticElement.MaxNumberOfTaunts; j++)
		{
			((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots).Add(new MPArmoryCosmeticTauntSlotVM(j));
		}
	}

	private void OnClothingCosmeticCategorySelected(MPArmoryClothingCosmeticCategoryVM selectedCosmetic)
	{
		FilterClothingsByCategory(selectedCosmetic);
	}

	private void OnTauntCosmeticCategorySelected(MPArmoryTauntCosmeticCategoryVM selectedCosmetic)
	{
		FilterTauntsByCategory(selectedCosmetic);
	}

	public void RefreshAvailableCategoriesBy(CosmeticType type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Invalid comparison between Unknown and I4
		_currentCosmeticType = type;
		((Collection<MPArmoryCosmeticCategoryBaseVM>)(object)AvailableCategories).Clear();
		if ((int)type == 0)
		{
			foreach (KeyValuePair<ClothingCategory, MPArmoryClothingCosmeticCategoryVM> item in _clothingCategoriesLookup)
			{
				((Collection<MPArmoryCosmeticCategoryBaseVM>)(object)AvailableCategories).Add((MPArmoryCosmeticCategoryBaseVM)item.Value);
			}
		}
		else if ((int)type == 3)
		{
			foreach (KeyValuePair<TauntCategoryFlag, MPArmoryTauntCosmeticCategoryVM> item2 in _tauntCategoriesLookup)
			{
				((Collection<MPArmoryCosmeticCategoryBaseVM>)(object)AvailableCategories).Add((MPArmoryCosmeticCategoryBaseVM)item2.Value);
			}
		}
		if (((Collection<MPArmoryCosmeticCategoryBaseVM>)(object)AvailableCategories).Count > 0)
		{
			if ((int)type == 0 && _currentClothingCategory != ClothingCategory.Invalid)
			{
				FilterClothingsByCategory(_clothingCategoriesLookup[_currentClothingCategory]);
			}
			else if ((int)type == 3)
			{
				TauntCategoryFlag key = ((_currentTauntCategory != TauntCategoryFlag.None) ? _currentTauntCategory : TauntCategoryFlag.All);
				FilterTauntsByCategory(_tauntCategoriesLookup[key]);
			}
		}
	}

	public void RefreshPlayerData(PlayerData playerData)
	{
		Loot = playerData.Gold;
	}

	public void RefreshCosmeticInfoFromNetwork()
	{
		_isNetworkCosmeticsDirty = true;
	}

	private void RefreshCosmeticInfoFromNetworkAux()
	{
		_isRetrievingCosmeticData = true;
		if (NetworkMain.GameClient.PlayerData == null)
		{
			_isRetrievingCosmeticData = false;
			return;
		}
		IsLoading = true;
		HasCosmeticInfoReceived = true;
		IsLoading = false;
		LobbyClient gameClient = NetworkMain.GameClient;
		object obj;
		if (gameClient == null)
		{
			obj = null;
		}
		else
		{
			PlayerData playerData = gameClient.PlayerData;
			obj = ((playerData != null) ? playerData.UserId.ToString() : null);
		}
		string text = (string)obj;
		LobbyClient gameClient2 = NetworkMain.GameClient;
		IReadOnlyDictionary<string, List<string>> readOnlyDictionary = ((gameClient2 != null) ? gameClient2.UsedCosmetics : null);
		LobbyClient gameClient3 = NetworkMain.GameClient;
		List<string> list = ((gameClient3 == null) ? null : gameClient3.OwnedCosmetics?.ToList());
		if (text == null || readOnlyDictionary == null || list == null)
		{
			_isRetrievingCosmeticData = false;
			return;
		}
		_ownedCosmetics = list;
		MBReadOnlyList<TauntIndexData> tauntIndicesForPlayer = MultiplayerLocalDataManager.Instance.TauntSlotData.GetTauntIndicesForPlayer(text);
		RefreshTaunts(text, tauntIndicesForPlayer);
		_usedCosmetics = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, List<string>> item in readOnlyDictionary)
		{
			_usedCosmetics.Add(item.Key, new List<string>());
			foreach (string item2 in readOnlyDictionary[item.Key])
			{
				_usedCosmetics[item.Key].Add(item2);
			}
		}
		RefreshSelectedClass(_selectedClass, _getSelectedPerks());
		_isRetrievingCosmeticData = false;
	}

	private async Task<bool> UpdateUsedCosmeticsAux()
	{
		_isSendingCosmeticData = true;
		IReadOnlyDictionary<string, List<string>> usedCosmetics = NetworkMain.GameClient.UsedCosmetics;
		Dictionary<string, List<(string, bool)>> dictionary = new Dictionary<string, List<(string, bool)>>();
		foreach (string key2 in _usedCosmetics.Keys)
		{
			dictionary.Add(key2, new List<(string, bool)>());
		}
		foreach (KeyValuePair<string, List<string>> item2 in usedCosmetics)
		{
			foreach (string item3 in item2.Value)
			{
				if (!_usedCosmetics[item2.Key].Contains(item3))
				{
					dictionary[item2.Key].Add((item3, false));
				}
			}
		}
		foreach (KeyValuePair<string, List<string>> usedCosmetic in _usedCosmetics)
		{
			if (!usedCosmetics.ContainsKey(usedCosmetic.Key))
			{
				foreach (string item4 in usedCosmetic.Value)
				{
					dictionary[usedCosmetic.Key].Add((item4, true));
				}
				continue;
			}
			foreach (string item5 in usedCosmetic.Value)
			{
				if (!usedCosmetics[usedCosmetic.Key].Contains(item5))
				{
					dictionary[usedCosmetic.Key].Add((item5, true));
				}
			}
		}
		foreach (KeyValuePair<string, List<(string, bool)>> item6 in dictionary)
		{
			List<ItemTypeEnum> list = new List<ItemTypeEnum>();
			foreach (var item7 in item6.Value)
			{
				var (key, _) = item7;
				if (item7.Item2 && _cosmeticItemsLookup.TryGetValue(key, out var value) && value is MPArmoryCosmeticClothingItemVM { EquipmentElement: var equipmentElement })
				{
					ItemTypeEnum itemType = ((EquipmentElement)(ref equipmentElement)).Item.ItemType;
					list.Add(itemType);
				}
			}
		}
		List<TauntIndexData> list2 = new List<TauntIndexData>();
		TauntIndexData item = default(TauntIndexData);
		for (int i = 0; i < ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots).Count; i++)
		{
			MPArmoryCosmeticTauntItemVM assignedTauntItem = ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)[i].AssignedTauntItem;
			if (assignedTauntItem != null)
			{
				((TauntIndexData)(ref item))._002Ector(assignedTauntItem.TauntID, i);
				list2.Add(item);
			}
		}
		bool result = false;
		LobbyClient gameClient = NetworkMain.GameClient;
		object obj;
		if (gameClient == null)
		{
			obj = null;
		}
		else
		{
			PlayerData playerData = gameClient.PlayerData;
			obj = ((playerData != null) ? playerData.UserId.ToString() : null);
		}
		string text = (string)obj;
		if (text != null)
		{
			MultiplayerLocalDataManager.Instance.TauntSlotData.SetTauntIndicesForPlayer(text, list2);
			result = await NetworkMain.GameClient.UpdateUsedCosmeticItems(dictionary);
		}
		_isSendingCosmeticData = false;
		return result;
	}

	public void RefreshSelectedClass(MPHeroClass selectedClass, List<IReadOnlyPerkObject> selectedPerks)
	{
		_selectedClass = selectedClass;
		if (_selectedClass == null)
		{
			return;
		}
		_selectedClassDefaultEquipment = _selectedClass.HeroCharacter.Equipment.Clone(false);
		if (selectedPerks != null)
		{
			MPArmoryVM.ApplyPerkEffectsToEquipment(ref _selectedClassDefaultEquipment, selectedPerks);
		}
		_selectedTroopID = ((MBObjectBase)_selectedClass).StringId;
		ActiveCategory?.Sort(_currentItemComparer);
		if (_ownedCosmetics != null)
		{
			foreach (string ownedCosmeticID in _ownedCosmetics)
			{
				MPArmoryCosmeticItemBaseVM mPArmoryCosmeticItemBaseVM = ((IEnumerable<MPArmoryCosmeticItemBaseVM>)ActiveCategory?.AvailableCosmetics).FirstOrDefault((MPArmoryCosmeticItemBaseVM c) => c.CosmeticID == ownedCosmeticID);
				if (mPArmoryCosmeticItemBaseVM != null)
				{
					mPArmoryCosmeticItemBaseVM.IsUnlocked = true;
				}
			}
		}
		RefreshFilters();
	}

	private void EquipItemOnHeroPreview(MPArmoryCosmeticItemBaseVM itemVM)
	{
		if (itemVM != null)
		{
			MPArmoryCosmeticsVM.OnCosmeticPreview?.Invoke(itemVM);
		}
		else
		{
			Debug.FailedAssert("Previewing null item", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Armory\\MPArmoryCosmeticsVM.cs", "EquipItemOnHeroPreview", 529);
		}
	}

	private void OnCosmeticEquipRequested(MPArmoryCosmeticItemBaseVM cosmeticItemVM)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if ((int)cosmeticItemVM.CosmeticType == 0)
		{
			OnItemEquipRequested((MPArmoryCosmeticClothingItemVM)cosmeticItemVM);
		}
		else if ((int)cosmeticItemVM.CosmeticType == 3)
		{
			OnTauntEquipRequested((MPArmoryCosmeticTauntItemVM)cosmeticItemVM);
		}
	}

	private void OnItemEquipRequested(MPArmoryCosmeticClothingItemVM itemVM)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Invalid comparison between Unknown and I4
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		if (itemVM.IsUsed && !itemVM.Cosmetic.IsFree && ActiveCategory != null && (int)ActiveCategory.CosmeticType == 0 && itemVM.ClothingCosmeticElement.ReplaceItemsId.Count > 0 && _selectedClassDefaultEquipment != null)
		{
			for (int i = 0; i < itemVM.ClothingCosmeticElement.ReplaceItemsId.Count; i++)
			{
				string replacedItemId = itemVM.ClothingCosmeticElement.ReplaceItemsId[i];
				for (EquipmentIndex val = (EquipmentIndex)5; (int)val < 10; val = (EquipmentIndex)(val + 1))
				{
					EquipmentElement val2 = _selectedClassDefaultEquipment[val];
					ItemObject item = ((EquipmentElement)(ref val2)).Item;
					if (((item != null) ? ((MBObjectBase)item).StringId : null) == replacedItemId && ((IEnumerable<MPArmoryCosmeticItemBaseVM>)ActiveCategory.AvailableCosmetics).FirstOrDefault((MPArmoryCosmeticItemBaseVM c) => c.Cosmetic.Id == replacedItemId) is MPArmoryCosmeticClothingItemVM itemVM2)
					{
						OnClothingItemEquipped(itemVM2);
						_isLocalCosmeticsDirty = true;
						return;
					}
				}
			}
		}
		if (itemVM.ClothingCosmeticElement.ReplaceItemless.Any((Tuple<string, string> r) => r.Item1 == ((MBObjectBase)_selectedClass).StringId))
		{
			if (itemVM.IsUsed)
			{
				itemVM.IsUsed = false;
				_usedCosmetics?[_selectedTroopID].Remove(itemVM.CosmeticID);
				MPArmoryCosmeticsVM.OnRemoveCosmeticFromPreview?.Invoke(itemVM);
			}
			else
			{
				itemVM.ActionText = itemVM.UnequipText;
				OnClothingItemEquipped(itemVM);
			}
		}
		else
		{
			OnClothingItemEquipped(itemVM);
		}
		_isLocalCosmeticsDirty = true;
	}

	private void OnClothingItemEquipped(MPArmoryCosmeticClothingItemVM itemVM, bool forceRemove = true)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		EquipItemOnHeroPreview(itemVM);
		if (!_usedCosmetics.ContainsKey(_selectedTroopID))
		{
			_usedCosmetics.Add(_selectedTroopID, new List<string>());
		}
		if (itemVM.CosmeticID != string.Empty && !_usedCosmetics[_selectedTroopID].Contains(itemVM.CosmeticID))
		{
			_usedCosmetics[_selectedTroopID].Add(itemVM.CosmeticID);
		}
		foreach (MPArmoryCosmeticItemBaseVM item in (Collection<MPArmoryCosmeticItemBaseVM>)(object)ActiveCategory.AvailableCosmetics)
		{
			EquipmentElement equipmentElement = ((MPArmoryCosmeticClothingItemVM)item).EquipmentElement;
			ItemTypeEnum itemType = ((EquipmentElement)(ref equipmentElement)).Item.ItemType;
			equipmentElement = itemVM.EquipmentElement;
			if (itemType == ((EquipmentElement)(ref equipmentElement)).Item.ItemType)
			{
				item.IsUsed = false;
				if (itemVM.Cosmetic.Id != item.Cosmetic.Id && forceRemove)
				{
					_usedCosmetics[_selectedTroopID]?.Remove(item.CosmeticID);
				}
			}
		}
		itemVM.IsUsed = true;
		if (ActiveCategory != null)
		{
			UpdateKeyBindingsForCategory(ActiveCategory);
		}
	}

	public void ClearTauntSelections()
	{
		if (SelectedTauntItem == null && SelectedTauntSlot == null)
		{
			return;
		}
		OnTauntEquipRequested(null);
		OnTauntSlotSelected(null);
		foreach (MPArmoryCosmeticTauntSlotVM item in (Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)
		{
			item.IsAcceptingTaunts = false;
			item.IsFocused = false;
		}
	}

	private void OnTauntEquipRequested(MPArmoryCosmeticTauntItemVM tauntItem)
	{
		if (SelectedTauntItem != null)
		{
			if (SelectedTauntItem == tauntItem)
			{
				ClearTauntSelections();
				return;
			}
			SelectedTauntItem.IsSelected = false;
		}
		SelectedTauntItem = tauntItem;
		if (SelectedTauntItem != null)
		{
			MPArmoryCosmeticTauntSlotVM mPArmoryCosmeticTauntSlotVM = null;
			for (int i = 0; i < ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots).Count; i++)
			{
				if (((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)[i].AssignedTauntItem?.CosmeticID == tauntItem.CosmeticID)
				{
					mPArmoryCosmeticTauntSlotVM = ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)[i];
					break;
				}
			}
			if (mPArmoryCosmeticTauntSlotVM != null)
			{
				SelectedTauntItem = null;
				mPArmoryCosmeticTauntSlotVM.AssignTauntItem(null);
				ClearTauntSelections();
				_isLocalCosmeticsDirty = true;
				return;
			}
			SelectedTauntItem.IsSelected = true;
			SelectedTauntItem.ActionText = SelectedTauntItem.CancelEquipText;
			foreach (MPArmoryCosmeticItemBaseVM item in (Collection<MPArmoryCosmeticItemBaseVM>)(object)ActiveCategory.AvailableCosmetics)
			{
				item.IsSelectable = item == SelectedTauntItem;
			}
		}
		else
		{
			foreach (MPArmoryCosmeticItemBaseVM item2 in (Collection<MPArmoryCosmeticItemBaseVM>)(object)ActiveCategory.AvailableCosmetics)
			{
				item2.IsSelectable = true;
			}
		}
		foreach (MPArmoryCosmeticTauntSlotVM item3 in (Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)
		{
			item3.IsAcceptingTaunts = item3.AssignedTauntItem != tauntItem;
		}
		MPArmoryCosmeticsVM.OnTauntAssignmentRefresh?.Invoke();
	}

	private void OnTauntSlotFocusChanged(MPArmoryCosmeticTauntSlotVM changedSlot, bool isFocused)
	{
		foreach (MPArmoryCosmeticTauntSlotVM item in (Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)
		{
			item.IsFocused = isFocused && changedSlot == item;
			if (item.IsAcceptingTaunts)
			{
				item.EmptySlotKeyVisual.SetForcedVisibility((bool?)false);
				item.SelectKeyVisual.SetForcedVisibility((bool?)null);
			}
			else if (item.AssignedTauntItem != null)
			{
				item.EmptySlotKeyVisual.SetForcedVisibility((bool?)null);
				item.SelectKeyVisual.SetForcedVisibility((bool?)false);
			}
			else
			{
				item.EmptySlotKeyVisual.SetForcedVisibility((bool?)false);
			}
			bool? forcedVisibility = ((!item.IsAcceptingTaunts && item.AssignedTauntItem != null) ? ((bool?)null) : new bool?(false));
			bool? forcedVisibility2 = ((item.AssignedTauntItem != null || item.IsAcceptingTaunts) ? ((bool?)null) : new bool?(false));
			InputKeyItemVM emptySlotKeyVisual = item.EmptySlotKeyVisual;
			if (emptySlotKeyVisual != null)
			{
				emptySlotKeyVisual.SetForcedVisibility(forcedVisibility);
			}
			InputKeyItemVM selectKeyVisual = item.SelectKeyVisual;
			if (selectKeyVisual != null)
			{
				selectKeyVisual.SetForcedVisibility(forcedVisibility2);
			}
		}
	}

	private void OnTauntSlotPreview(MPArmoryCosmeticTauntSlotVM previewSlot)
	{
		previewSlot?.AssignedTauntItem?.ExecutePreview();
	}

	private void OnTauntSlotSelected(MPArmoryCosmeticTauntSlotVM selectedSlot)
	{
		if (SelectedTauntSlot == null && SelectedTauntItem == null && selectedSlot != null && selectedSlot.IsEmpty)
		{
			return;
		}
		MPArmoryCosmeticTauntSlotVM selectedTauntSlot = SelectedTauntSlot;
		SelectedTauntSlot = selectedSlot;
		if (selectedTauntSlot != null)
		{
			selectedTauntSlot.IsSelected = false;
		}
		if (SelectedTauntSlot != null)
		{
			SelectedTauntSlot.IsSelected = true;
		}
		if (selectedSlot?.AssignedTauntItem != null)
		{
			bool flag = false;
			for (int i = 0; i < ((Collection<MPArmoryCosmeticItemBaseVM>)(object)ActiveCategory.AvailableCosmetics).Count; i++)
			{
				if (((Collection<MPArmoryCosmeticItemBaseVM>)(object)ActiveCategory.AvailableCosmetics)[i] == selectedSlot.AssignedTauntItem)
				{
					flag = true;
					break;
				}
			}
			if (!flag && _tauntCategoriesLookup.TryGetValue(TauntCategoryFlag.All, out var value))
			{
				FilterTauntsByCategory(value);
			}
		}
		foreach (MPArmoryCosmeticItemBaseVM item in (Collection<MPArmoryCosmeticItemBaseVM>)(object)ActiveCategory.AvailableCosmetics)
		{
			item.IsSelectable = selectedSlot == null || item == selectedSlot?.AssignedTauntItem;
		}
		if (SelectedTauntItem == null)
		{
			MPArmoryCosmeticTauntSlotVM selectedTauntSlot2 = SelectedTauntSlot;
			if (selectedTauntSlot2 != null && !selectedTauntSlot2.IsEmpty)
			{
				foreach (MPArmoryCosmeticTauntSlotVM item2 in (Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)
				{
					item2.IsAcceptingTaunts = item2 != selectedSlot;
				}
			}
		}
		if (SelectedTauntSlot != null)
		{
			bool flag2 = false;
			if (SelectedTauntItem != null && SelectedTauntSlot.AssignedTauntItem != SelectedTauntItem)
			{
				MPArmoryCosmeticTauntSlotVM mPArmoryCosmeticTauntSlotVM = null;
				for (int j = 0; j < ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots).Count; j++)
				{
					if (((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)[j].AssignedTauntItem == SelectedTauntItem)
					{
						mPArmoryCosmeticTauntSlotVM = ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)[j];
						break;
					}
				}
				if (mPArmoryCosmeticTauntSlotVM != null)
				{
					MPArmoryCosmeticTauntItemVM assignedTauntItem = SelectedTauntSlot.AssignedTauntItem;
					MPArmoryCosmeticTauntItemVM assignedTauntItem2 = mPArmoryCosmeticTauntSlotVM.AssignedTauntItem;
					SelectedTauntSlot.AssignTauntItem(assignedTauntItem2, isSwapping: true);
					mPArmoryCosmeticTauntSlotVM.AssignTauntItem(assignedTauntItem, isSwapping: true);
				}
				else
				{
					SelectedTauntSlot.AssignTauntItem(SelectedTauntItem);
				}
				flag2 = true;
				ClearTauntSelections();
			}
			else if (selectedTauntSlot != null && !selectedTauntSlot.IsEmpty && SelectedTauntSlot != selectedTauntSlot)
			{
				MPArmoryCosmeticTauntItemVM assignedTauntItem3 = selectedTauntSlot.AssignedTauntItem;
				MPArmoryCosmeticTauntItemVM assignedTauntItem4 = SelectedTauntSlot.AssignedTauntItem;
				SelectedTauntSlot.AssignTauntItem(assignedTauntItem3, isSwapping: true);
				selectedTauntSlot.AssignTauntItem(assignedTauntItem4, isSwapping: true);
				flag2 = true;
				ClearTauntSelections();
			}
			if (flag2)
			{
				_isLocalCosmeticsDirty = true;
			}
		}
		MPArmoryCosmeticsVM.OnTauntAssignmentRefresh?.Invoke();
	}

	private void OnTauntItemEquipped(MPArmoryCosmeticTauntSlotVM equippedSlot, MPArmoryCosmeticTauntItemVM previousTauntItem, bool isSwapping)
	{
		for (int i = 0; i < ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots).Count; i++)
		{
			MPArmoryCosmeticTauntItemVM assignedTauntItem = ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)[i].AssignedTauntItem;
			if (assignedTauntItem != null && !assignedTauntItem.IsUnlocked)
			{
				Debug.FailedAssert("Assigned a taunt without ownership: " + assignedTauntItem.TauntID, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Armory\\MPArmoryCosmeticsVM.cs", "OnTauntItemEquipped", 872);
			}
		}
		_isLocalCosmeticsDirty = true;
	}

	public void OnItemObtained(string cosmeticID, int finalLoot)
	{
		_ownedCosmetics.Add(cosmeticID);
		RefreshCosmeticInfoFromNetwork();
		Loot = finalLoot;
	}

	private void OnSortCategoryUpdated(SelectorVM<SelectorItemVM> selector)
	{
		if (SortCategories.SelectedIndex == -1)
		{
			SortCategories.SelectedIndex = 0;
		}
		_currentItemComparer = _itemComparers[selector.SelectedIndex];
		ActiveCategory?.Sort(_currentItemComparer);
	}

	private void OnSortOrderUpdated(SelectorVM<SelectorItemVM> selector)
	{
		if (SortOrders.SelectedIndex == -1)
		{
			SortOrders.SelectedIndex = 0;
		}
		foreach (CosmeticItemComparer itemComparer in _itemComparers)
		{
			itemComparer.SetSortMode(selector.SelectedIndex == 0);
		}
		ActiveCategory?.Sort(_currentItemComparer);
	}

	private void RefreshFilters()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		MPArmoryTauntCosmeticCategoryVM value2;
		if ((int)_currentCosmeticType == 0 && _clothingCategoriesLookup.TryGetValue(_currentClothingCategory, out var value))
		{
			FilterClothingsByCategory(value);
		}
		else if ((int)_currentCosmeticType == 3 && _tauntCategoriesLookup.TryGetValue(_currentTauntCategory, out value2))
		{
			FilterTauntsByCategory(value2);
		}
	}

	private void FilterClothingsByCategory(MPArmoryClothingCosmeticCategoryVM clothingCategory)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if ((int)_currentCosmeticType != 0)
		{
			RefreshAvailableCategoriesBy((CosmeticType)0);
			return;
		}
		if (clothingCategory == null)
		{
			Debug.FailedAssert("Trying to filter by null clothing category", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Armory\\MPArmoryCosmeticsVM.cs", "FilterClothingsByCategory", 935);
			return;
		}
		_currentClothingCategory = clothingCategory.ClothingCategory;
		foreach (KeyValuePair<ClothingCategory, MPArmoryClothingCosmeticCategoryVM> item in _clothingCategoriesLookup)
		{
			item.Value.IsSelected = false;
		}
		clothingCategory.SetDefaultEquipments(_selectedClassDefaultEquipment);
		ActiveCategory = clothingCategory;
		if (_selectedClass != null)
		{
			foreach (MPArmoryCosmeticItemBaseVM allCosmetic in _allCosmetics)
			{
				if ((int)allCosmetic.CosmeticType == 0)
				{
					clothingCategory.ReplaceCosmeticWithDefaultItem((MPArmoryCosmeticClothingItemVM)allCosmetic, clothingCategory.ClothingCategory, _selectedClass, _ownedCosmetics);
				}
			}
		}
		ActiveCategory.Sort(_currentItemComparer);
		RefreshEquipment();
		if (ActiveCategory != null)
		{
			ActiveCategory.IsSelected = true;
			UpdateKeyBindingsForCategory(ActiveCategory);
		}
	}

	private void FilterTauntsByCategory(MPArmoryTauntCosmeticCategoryVM tauntCategory)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)_currentCosmeticType != 3)
		{
			RefreshAvailableCategoriesBy((CosmeticType)3);
		}
		_currentTauntCategory = tauntCategory.TauntCategory;
		foreach (KeyValuePair<TauntCategoryFlag, MPArmoryTauntCosmeticCategoryVM> item in _tauntCategoriesLookup)
		{
			item.Value.IsSelected = false;
		}
		ActiveCategory = tauntCategory;
		if (ActiveCategory != null)
		{
			ActiveCategory.IsSelected = true;
			UpdateKeyBindingsForCategory(ActiveCategory);
		}
		ActiveCategory.Sort(_currentItemComparer);
	}

	private void RefreshEquipment()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<EquipmentIndex, bool> dictionary = new Dictionary<EquipmentIndex, bool>();
		for (EquipmentIndex val = (EquipmentIndex)5; (int)val < 10; val = (EquipmentIndex)(val + 1))
		{
			dictionary.Add(val, value: false);
		}
		List<EquipmentElement> list = new List<EquipmentElement>();
		EquipmentElement val2;
		foreach (MPArmoryCosmeticItemBaseVM item2 in ((IEnumerable<MPArmoryCosmeticItemBaseVM>)ActiveCategory.AvailableCosmetics).Where((MPArmoryCosmeticItemBaseVM c) => (int)c.Cosmetic.Rarity == 0))
		{
			if (item2 is MPArmoryCosmeticClothingItemVM mPArmoryCosmeticClothingItemVM)
			{
				OnClothingItemEquipped(mPArmoryCosmeticClothingItemVM, forceRemove: false);
				val2 = mPArmoryCosmeticClothingItemVM.EquipmentElement;
				dictionary[((EquipmentElement)(ref val2)).Item.GetCosmeticEquipmentIndex()] = true;
				list.Add(mPArmoryCosmeticClothingItemVM.EquipmentElement);
			}
		}
		if (!string.IsNullOrEmpty(_selectedTroopID))
		{
			Dictionary<string, List<string>> usedCosmetics = _usedCosmetics;
			if (usedCosmetics != null && usedCosmetics.ContainsKey(_selectedTroopID))
			{
				Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
				foreach (string key in _usedCosmetics.Keys)
				{
					List<string> list2 = new List<string>();
					foreach (string item3 in _usedCosmetics[key])
					{
						list2.Add(item3);
					}
					dictionary2.Add(key, list2);
				}
				foreach (string cosmeticID in dictionary2[_selectedTroopID])
				{
					MPArmoryCosmeticClothingItemVM cosmeticItem = (MPArmoryCosmeticClothingItemVM)_allCosmetics.First((MPArmoryCosmeticItemBaseVM c) => c.CosmeticID == cosmeticID);
					if (cosmeticItem == null)
					{
						continue;
					}
					val2 = cosmeticItem.EquipmentElement;
					EquipmentIndex cosmeticEquipmentIndex = ((EquipmentElement)(ref val2)).Item.GetCosmeticEquipmentIndex();
					CosmeticElement cosmetic = cosmeticItem.Cosmetic;
					if (Extensions.IsEmpty<Tuple<string, string>>((IEnumerable<Tuple<string, string>>)((ClothingCosmeticElement)((cosmetic is ClothingCosmeticElement) ? cosmetic : null)).ReplaceItemless))
					{
						val2 = _selectedClassDefaultEquipment[cosmeticEquipmentIndex];
						if (((EquipmentElement)(ref val2)).IsEmpty)
						{
							continue;
						}
					}
					EquipmentElement item = ((IEnumerable<EquipmentElement>)list).FirstOrDefault((Func<EquipmentElement, bool>)delegate(EquipmentElement i)
					{
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						//IL_0012: Unknown result type (might be due to invalid IL or missing references)
						//IL_0017: Unknown result type (might be due to invalid IL or missing references)
						//IL_001f: Unknown result type (might be due to invalid IL or missing references)
						EquipmentIndex cosmeticEquipmentIndex2 = ((EquipmentElement)(ref i)).Item.GetCosmeticEquipmentIndex();
						EquipmentElement equipmentElement = cosmeticItem.EquipmentElement;
						return cosmeticEquipmentIndex2 == ((EquipmentElement)(ref equipmentElement)).Item.GetCosmeticEquipmentIndex();
					});
					if (!((EquipmentElement)(ref item)).IsEmpty)
					{
						list.Remove(item);
						list.Add(cosmeticItem.EquipmentElement);
					}
					OnClothingItemEquipped(cosmeticItem);
					dictionary[cosmeticEquipmentIndex] = true;
				}
			}
		}
		foreach (EquipmentIndex key2 in dictionary.Keys)
		{
			if (!dictionary[key2])
			{
				((MPArmoryClothingCosmeticCategoryVM)ActiveCategory)?.OnEquipmentRefreshed(key2);
			}
		}
		MPArmoryCosmeticsVM.OnEquipmentRefreshed?.Invoke(list);
	}

	private void RefreshTaunts(string playerId, MBReadOnlyList<TauntIndexData> registeredTaunts)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		List<TauntIndexData> list = ((IEnumerable<TauntIndexData>)registeredTaunts)?.ToList();
		if (list == null)
		{
			list = new List<TauntIndexData>();
			TauntIndexData item = default(TauntIndexData);
			foreach (MPArmoryCosmeticItemBaseVM item2 in (from c in _tauntCategoriesLookup.SelectMany((KeyValuePair<TauntCategoryFlag, MPArmoryTauntCosmeticCategoryVM> c) => (IEnumerable<MPArmoryCosmeticItemBaseVM>)c.Value.AvailableCosmetics)
				where c.Cosmetic.IsFree
				select c).Distinct())
			{
				((TauntIndexData)(ref item))._002Ector(item2.CosmeticID, list.Count);
				list.Add(item);
			}
		}
		foreach (KeyValuePair<TauntCategoryFlag, MPArmoryTauntCosmeticCategoryVM> item3 in _tauntCategoriesLookup)
		{
			foreach (MPArmoryCosmeticItemBaseVM item4 in (Collection<MPArmoryCosmeticItemBaseVM>)(object)item3.Value.AvailableCosmetics)
			{
				item4.IsUnlocked = item4.Cosmetic.IsFree || _ownedCosmetics.Contains(item4.CosmeticID);
			}
		}
		for (int num = 0; num < ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots).Count; num++)
		{
			((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)[num].AssignTauntItem(null);
		}
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			TauntIndexData val = list[num2];
			string tauntId = ((TauntIndexData)(ref val)).TauntId;
			val = list[num2];
			int tauntIndex = ((TauntIndexData)(ref val)).TauntIndex;
			if (_cosmeticItemsLookup.TryGetValue(tauntId, out var value) && value is MPArmoryCosmeticTauntItemVM mPArmoryCosmeticTauntItemVM)
			{
				if (!mPArmoryCosmeticTauntItemVM.IsUnlocked)
				{
					Debug.FailedAssert("Trying to add non-owned cosmetic to taunt slot: " + mPArmoryCosmeticTauntItemVM.TauntID, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Armory\\MPArmoryCosmeticsVM.cs", "RefreshTaunts", 1113);
				}
				else if (tauntIndex >= 0 && tauntIndex < ((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots).Count)
				{
					((Collection<MPArmoryCosmeticTauntSlotVM>)(object)TauntSlots)[tauntIndex].AssignTauntItem(mPArmoryCosmeticTauntItemVM);
				}
			}
		}
	}

	private void UpdateTauntAssignmentState()
	{
		IsTauntAssignmentActive = SelectedTauntItem != null || SelectedTauntSlot != null;
	}

	private void ExecuteRefreshCosmeticInfo()
	{
		RefreshCosmeticInfoFromNetwork();
	}

	private void ExecuteResetPreview()
	{
		RefreshSelectedClass(_selectedClass, _getSelectedPerks());
	}

	public void RefreshKeyBindings(HotKey actionKey, HotKey previewKey)
	{
		ActionInputKey = InputKeyItemVM.CreateFromHotKey(actionKey, false);
		PreviewInputKey = InputKeyItemVM.CreateFromHotKey(previewKey, false);
		for (int i = 0; i < ((Collection<MPArmoryCosmeticCategoryBaseVM>)(object)AvailableCategories).Count; i++)
		{
			UpdateKeyBindingsForCategory(((Collection<MPArmoryCosmeticCategoryBaseVM>)(object)AvailableCategories)[i]);
		}
	}

	private void UpdateKeyBindingsForCategory(MPArmoryCosmeticCategoryBaseVM categoryVM)
	{
		if (ActionInputKey != null && PreviewInputKey != null)
		{
			for (int i = 0; i < ((Collection<MPArmoryCosmeticItemBaseVM>)(object)categoryVM.AvailableCosmetics).Count; i++)
			{
				((Collection<MPArmoryCosmeticItemBaseVM>)(object)categoryVM.AvailableCosmetics)[i].RefreshKeyBindings(ActionInputKey.HotKey, PreviewInputKey.HotKey);
			}
		}
	}
}
