using System;
using System.Collections.Generic;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;

public abstract class MPArmoryCosmeticItemBaseVM : ViewModel
{
	public readonly CosmeticElement Cosmetic;

	public readonly string CosmeticID;

	private bool _isUnlocked;

	private bool _isUsed;

	private bool _areActionsEnabled;

	private bool _isSelectable;

	private bool _isUnequippable;

	private int _cost;

	private int _rarity;

	private int _itemType;

	private string _name;

	private string _ownedText;

	private string _actionText;

	private string _previewText;

	private ItemImageIdentifierVM _icon;

	private InputKeyItemVM _actionKey;

	private InputKeyItemVM _previewKey;

	public string UnequipText { get; private set; }

	public CosmeticType CosmeticType { get; }

	[DataSourceProperty]
	public bool IsUnlocked
	{
		get
		{
			return _isUnlocked;
		}
		set
		{
			if (value != _isUnlocked)
			{
				_isUnlocked = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsUnlocked");
				UpdatePreviewAndActionTexts();
			}
		}
	}

	[DataSourceProperty]
	public bool IsUsed
	{
		get
		{
			return _isUsed;
		}
		set
		{
			if (value != _isUsed)
			{
				_isUsed = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsUsed");
				UpdatePreviewAndActionTexts();
			}
		}
	}

	[DataSourceProperty]
	public bool AreActionsEnabled
	{
		get
		{
			return _areActionsEnabled;
		}
		set
		{
			if (value != _areActionsEnabled)
			{
				_areActionsEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AreActionsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelectable
	{
		get
		{
			return _isSelectable;
		}
		set
		{
			if (value != _isSelectable)
			{
				_isSelectable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelectable");
			}
		}
	}

	[DataSourceProperty]
	public bool IsUnequippable
	{
		get
		{
			return _isUnequippable;
		}
		set
		{
			if (value != _isUnequippable)
			{
				_isUnequippable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsUnequippable");
			}
		}
	}

	[DataSourceProperty]
	public int Cost
	{
		get
		{
			return _cost;
		}
		set
		{
			if (value != _cost)
			{
				_cost = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Cost");
			}
		}
	}

	[DataSourceProperty]
	public int Rarity
	{
		get
		{
			return _rarity;
		}
		set
		{
			if (value != _rarity)
			{
				_rarity = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Rarity");
			}
		}
	}

	[DataSourceProperty]
	public int ItemType
	{
		get
		{
			return _itemType;
		}
		set
		{
			if (value != _itemType)
			{
				_itemType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ItemType");
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
	public string OwnedText
	{
		get
		{
			return _ownedText;
		}
		set
		{
			if (value != _ownedText)
			{
				_ownedText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OwnedText");
			}
		}
	}

	[DataSourceProperty]
	public string ActionText
	{
		get
		{
			return _actionText;
		}
		set
		{
			if (value != _actionText)
			{
				_actionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ActionText");
			}
		}
	}

	[DataSourceProperty]
	public string PreviewText
	{
		get
		{
			return _previewText;
		}
		set
		{
			if (value != _previewText)
			{
				_previewText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PreviewText");
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
	public InputKeyItemVM ActionKey
	{
		get
		{
			return _actionKey;
		}
		set
		{
			if (value != _actionKey)
			{
				_actionKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "ActionKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM PreviewKey
	{
		get
		{
			return _previewKey;
		}
		set
		{
			if (value != _previewKey)
			{
				_previewKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "PreviewKey");
			}
		}
	}

	public static event Action<MPArmoryCosmeticItemBaseVM> OnEquipped;

	public static event Action<MPArmoryCosmeticItemBaseVM> OnPurchaseRequested;

	public static event Action<MPArmoryCosmeticItemBaseVM> OnPreviewed;

	public MPArmoryCosmeticItemBaseVM(CosmeticElement cosmetic, string cosmeticID, CosmeticType cosmeticType)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected I4, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Cosmetic = cosmetic;
		CosmeticID = cosmeticID;
		Cost = cosmetic.Cost;
		Rarity = (int)cosmetic.Rarity;
		CosmeticType = cosmeticType;
		IsUnequippable = true;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		OwnedText = ((object)new TextObject("{=B5bcj3pC}Owned", (Dictionary<string, object>)null)).ToString();
		UnequipText = ((object)new TextObject("{=QndVFTbx}Unequip", (Dictionary<string, object>)null)).ToString();
		UpdatePreviewAndActionTexts();
	}

	public override void OnFinalize()
	{
		InputKeyItemVM actionKey = ActionKey;
		if (actionKey != null)
		{
			((ViewModel)actionKey).OnFinalize();
		}
		InputKeyItemVM previewKey = PreviewKey;
		if (previewKey != null)
		{
			((ViewModel)previewKey).OnFinalize();
		}
	}

	public void ExecuteAction()
	{
		if (IsUnlocked)
		{
			MPArmoryCosmeticItemBaseVM.OnEquipped?.Invoke(this);
		}
		else
		{
			MPArmoryCosmeticItemBaseVM.OnPurchaseRequested(this);
		}
	}

	public void ExecutePreview()
	{
		MPArmoryCosmeticItemBaseVM.OnPreviewed(this);
	}

	public void ExecuteEnableActions()
	{
		AreActionsEnabled = true;
	}

	public void ExecuteDisableActions()
	{
		AreActionsEnabled = false;
	}

	protected void UpdatePreviewAndActionTexts()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if (IsUnlocked)
		{
			if (IsUsed)
			{
				ActionText = (IsUnequippable ? UnequipText : string.Empty);
			}
			else
			{
				ActionText = ((object)new TextObject("{=DKqLY1aJ}Equip", (Dictionary<string, object>)null)).ToString();
			}
		}
		else
		{
			ActionText = ((object)new TextObject("{=i2mNBaxE}Obtain", (Dictionary<string, object>)null)).ToString();
		}
		PreviewText = ((object)new TextObject("{=un7poy9x}Preview", (Dictionary<string, object>)null)).ToString();
	}

	public void RefreshKeyBindings(HotKey actionKey, HotKey previewKey)
	{
		if (IsUnlocked && IsUsed && !IsUnequippable)
		{
			ActionKey = InputKeyItemVM.CreateFromHotKey((HotKey)null, false);
		}
		else
		{
			string groupId = actionKey.GroupId;
			InputKeyItemVM actionKey2 = ActionKey;
			if (!(groupId != ((actionKey2 == null) ? null : actionKey2.HotKey?.GroupId)))
			{
				string id = actionKey.Id;
				InputKeyItemVM actionKey3 = ActionKey;
				if (!(id != ((actionKey3 == null) ? null : actionKey3.HotKey?.Id)))
				{
					goto IL_008a;
				}
			}
			ActionKey = InputKeyItemVM.CreateFromHotKey(actionKey, false);
		}
		goto IL_008a;
		IL_008a:
		string groupId2 = previewKey.GroupId;
		InputKeyItemVM previewKey2 = PreviewKey;
		if (!(groupId2 != ((previewKey2 == null) ? null : previewKey2.HotKey?.GroupId)))
		{
			string id2 = previewKey.Id;
			InputKeyItemVM previewKey3 = PreviewKey;
			if (!(id2 != ((previewKey3 == null) ? null : previewKey3.HotKey?.Id)))
			{
				goto IL_00ed;
			}
		}
		PreviewKey = InputKeyItemVM.CreateFromHotKey(previewKey, false);
		goto IL_00ed;
		IL_00ed:
		UpdatePreviewAndActionTexts();
	}
}
