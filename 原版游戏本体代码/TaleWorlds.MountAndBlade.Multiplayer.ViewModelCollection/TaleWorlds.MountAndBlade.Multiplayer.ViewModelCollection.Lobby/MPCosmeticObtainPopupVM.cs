using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics.CosmeticTypes;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPCosmeticObtainPopupVM : ViewModel
{
	public enum CosmeticObtainState
	{
		Initialized,
		Ongoing,
		FinishedSuccessfully,
		FinishedUnsuccessfully
	}

	private readonly Action<string, int> _onItemObtained;

	private readonly Func<string> _getExitText;

	private int _currentTauntUsageIndex;

	private string _activeCosmeticID = string.Empty;

	private readonly Dictionary<BasicCultureObject, string> _cultureShieldItemIDs;

	private TextObject _currentLootTextObject = new TextObject("{=7vbGaapv}Your Loot: {LOOT}", (Dictionary<string, object>)null);

	private const string _purchaseCompleteSound = "event:/ui/multiplayer/shop_purchase_complete";

	private List<EquipmentElement> _characterEquipments;

	private bool _isEnabled;

	private bool _canObtain;

	private bool _isOpenedWithClothingItem;

	private bool _isOpenedWithSigilItem;

	private bool _isOpenedWithTauntItem;

	private bool _isObtainSuccessful;

	private int _obtainState;

	private string _animationVariationText;

	private string _obtainDescriptionText;

	private string _continueText;

	private string _notEnoughLootText;

	private string _obtainResultText;

	private string _previewAsText;

	private string _currentLootText;

	private string _clickToCloseText;

	private CharacterViewModel _characterVisual;

	private MPLobbyCosmeticSigilItemVM _sigilItem;

	private MPArmoryCosmeticClothingItemVM _item;

	private MPArmoryCosmeticTauntItemVM _tauntItem;

	private ItemCollectionElementViewModel _itemVisual;

	private MBBindingList<MPCultureItemVM> _cultures;

	private InputKeyItemVM _doneInputKey;

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
	public bool CanObtain
	{
		get
		{
			return _canObtain;
		}
		set
		{
			if (value != _canObtain)
			{
				_canObtain = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanObtain");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOpenedWithClothingItem
	{
		get
		{
			return _isOpenedWithClothingItem;
		}
		set
		{
			if (value != _isOpenedWithClothingItem)
			{
				_isOpenedWithClothingItem = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsOpenedWithClothingItem");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOpenedWithSigilItem
	{
		get
		{
			return _isOpenedWithSigilItem;
		}
		set
		{
			if (value != _isOpenedWithSigilItem)
			{
				_isOpenedWithSigilItem = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsOpenedWithSigilItem");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOpenedWithTauntItem
	{
		get
		{
			return _isOpenedWithTauntItem;
		}
		set
		{
			if (value != _isOpenedWithTauntItem)
			{
				_isOpenedWithTauntItem = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsOpenedWithTauntItem");
			}
		}
	}

	[DataSourceProperty]
	public bool IsObtainSuccessful
	{
		get
		{
			return _isObtainSuccessful;
		}
		set
		{
			if (value != _isObtainSuccessful)
			{
				_isObtainSuccessful = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsObtainSuccessful");
			}
		}
	}

	[DataSourceProperty]
	public int ObtainState
	{
		get
		{
			return _obtainState;
		}
		set
		{
			if (value != _obtainState)
			{
				_obtainState = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ObtainState");
			}
		}
	}

	[DataSourceProperty]
	public string ObtainDescriptionText
	{
		get
		{
			return _obtainDescriptionText;
		}
		set
		{
			if (value != _obtainDescriptionText)
			{
				_obtainDescriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ObtainDescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public string AnimationVariationText
	{
		get
		{
			return _animationVariationText;
		}
		set
		{
			if (value != _animationVariationText)
			{
				_animationVariationText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AnimationVariationText");
			}
		}
	}

	[DataSourceProperty]
	public string ContinueText
	{
		get
		{
			return _continueText;
		}
		set
		{
			if (value != _continueText)
			{
				_continueText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ContinueText");
			}
		}
	}

	[DataSourceProperty]
	public string NotEnoughLootText
	{
		get
		{
			return _notEnoughLootText;
		}
		set
		{
			if (value != _notEnoughLootText)
			{
				_notEnoughLootText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NotEnoughLootText");
			}
		}
	}

	[DataSourceProperty]
	public string ObtainResultText
	{
		get
		{
			return _obtainResultText;
		}
		set
		{
			if (value != _obtainResultText)
			{
				_obtainResultText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ObtainResultText");
			}
		}
	}

	[DataSourceProperty]
	public string PreviewAsText
	{
		get
		{
			return _previewAsText;
		}
		set
		{
			if (value != _previewAsText)
			{
				_previewAsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PreviewAsText");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentLootText
	{
		get
		{
			return _currentLootText;
		}
		set
		{
			if (value != _currentLootText)
			{
				_currentLootText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentLootText");
			}
		}
	}

	[DataSourceProperty]
	public string ClickToCloseText
	{
		get
		{
			return _clickToCloseText;
		}
		set
		{
			if (value != _clickToCloseText)
			{
				_clickToCloseText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ClickToCloseText");
			}
		}
	}

	[DataSourceProperty]
	public CharacterViewModel CharacterVisual
	{
		get
		{
			return _characterVisual;
		}
		set
		{
			if (value != _characterVisual)
			{
				_characterVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterViewModel>(value, "CharacterVisual");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyCosmeticSigilItemVM SigilItem
	{
		get
		{
			return _sigilItem;
		}
		set
		{
			if (value != _sigilItem)
			{
				_sigilItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPLobbyCosmeticSigilItemVM>(value, "SigilItem");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryCosmeticClothingItemVM Item
	{
		get
		{
			return _item;
		}
		set
		{
			if (value != _item)
			{
				_item = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryCosmeticClothingItemVM>(value, "Item");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryCosmeticTauntItemVM TauntItem
	{
		get
		{
			return _tauntItem;
		}
		set
		{
			if (value != _tauntItem)
			{
				_tauntItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryCosmeticTauntItemVM>(value, "TauntItem");
			}
		}
	}

	[DataSourceProperty]
	public ItemCollectionElementViewModel ItemVisual
	{
		get
		{
			return _itemVisual;
		}
		set
		{
			if (value != _itemVisual)
			{
				_itemVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<ItemCollectionElementViewModel>(value, "ItemVisual");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPCultureItemVM> Cultures
	{
		get
		{
			return _cultures;
		}
		set
		{
			if (value != _cultures)
			{
				_cultures = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPCultureItemVM>>(value, "Cultures");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM DoneInputKey
	{
		get
		{
			return _doneInputKey;
		}
		set
		{
			if (value != _doneInputKey)
			{
				_doneInputKey = value;
				((ViewModel)this).OnPropertyChanged("DoneInputKey");
			}
		}
	}

	public MPCosmeticObtainPopupVM(Action<string, int> onItemObtained, Func<string> getContinueKeyText)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Expected O, but got Unknown
		_onItemObtained = onItemObtained;
		_getExitText = getContinueKeyText;
		_characterEquipments = new List<EquipmentElement>();
		ItemVisual = new ItemCollectionElementViewModel();
		MBBindingList<MPCultureItemVM> obj = new MBBindingList<MPCultureItemVM>();
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("vlandia")).StringId, OnCultureSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("sturgia")).StringId, OnCultureSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("empire")).StringId, OnCultureSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("battania")).StringId, OnCultureSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("khuzait")).StringId, OnCultureSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("aserai")).StringId, OnCultureSelection));
		Cultures = obj;
		_cultureShieldItemIDs = new Dictionary<BasicCultureObject, string>
		{
			[((Collection<MPCultureItemVM>)(object)Cultures)[0].Culture] = "mp_tall_heater_shield_light",
			[((Collection<MPCultureItemVM>)(object)Cultures)[1].Culture] = "mp_worn_kite_shield",
			[((Collection<MPCultureItemVM>)(object)Cultures)[2].Culture] = "mp_leather_bound_kite_shield",
			[((Collection<MPCultureItemVM>)(object)Cultures)[3].Culture] = "mp_highland_riders_shield",
			[((Collection<MPCultureItemVM>)(object)Cultures)[4].Culture] = "mp_eastern_wicker_shield",
			[((Collection<MPCultureItemVM>)(object)Cultures)[5].Culture] = "mp_desert_oval_shield"
		};
		CharacterVisual = new CharacterViewModel();
		MPArmoryCosmeticsVM.OnEquipmentRefreshed += OnCharacterEquipmentRefreshed;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		ContinueText = ((object)GameTexts.FindText("str_continue", (string)null)).ToString();
		NotEnoughLootText = ((object)new TextObject("{=FzFqhHKU}Not enough loot", (Dictionary<string, object>)null)).ToString();
		PreviewAsText = ((object)new TextObject("{=V0bpuzV3}Preview as", (Dictionary<string, object>)null)).ToString();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		MPArmoryCosmeticsVM.OnEquipmentRefreshed -= OnCharacterEquipmentRefreshed;
	}

	private void OnCharacterEquipmentRefreshed(List<EquipmentElement> equipments)
	{
		_characterEquipments.Clear();
		_characterEquipments.AddRange(equipments);
	}

	public void OpenWith(MPArmoryCosmeticClothingItemVM item)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		OnOpened();
		Item = item;
		ItemVisual.FillFrom(item.EquipmentElement, (Banner)null);
		ItemVisual.BannerCode = "";
		ItemVisual.InitialPanRotation = 0f;
		ObtainDescriptionText = ((object)new TextObject("{=7uILxbP5}You will obtain this item", (Dictionary<string, object>)null)).ToString();
		_activeCosmeticID = item.CosmeticID;
		IsOpenedWithClothingItem = true;
		GameTexts.SetVariable("STR1", GameTexts.FindText("str_continue", (string)null));
		GameTexts.SetVariable("STR2", item.Cost);
		ContinueText = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		CanObtain = Item?.Cost <= NetworkMain.GameClient.PlayerData.Gold;
	}

	public void OpenWith(MPArmoryCosmeticTauntItemVM item, CharacterViewModel sourceCharacter)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected I4, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected I4, but got Unknown
		OnOpened();
		TauntCosmeticElement tauntCosmeticElement = item.TauntCosmeticElement;
		TauntItem = item;
		Equipment val = LobbyTauntHelper.PrepareForTaunt(Equipment.CreateFromEquipmentCode(sourceCharacter.EquipmentCode), tauntCosmeticElement);
		EquipmentIndex val2 = default(EquipmentIndex);
		EquipmentIndex val3 = default(EquipmentIndex);
		bool flag = default(bool);
		val.GetInitialWeaponIndicesToEquip(ref val2, ref val3, ref flag, (InitialWeaponEquipPreference)0);
		CharacterVisual.RightHandWieldedEquipmentIndex = (int)val2;
		if (!flag)
		{
			CharacterVisual.LeftHandWieldedEquipmentIndex = (int)val3;
		}
		CharacterVisual.FillFrom(sourceCharacter, -1);
		CharacterVisual.SetEquipment(val);
		string defaultAction = TauntUsageManager.Instance.GetDefaultAction(TauntUsageManager.Instance.GetIndexOfAction(((CosmeticElement)tauntCosmeticElement).Id));
		CharacterVisual.ExecuteStartCustomAnimation(TauntUsageManager.Instance.GetDefaultAction(TauntUsageManager.Instance.GetIndexOfAction(((CosmeticElement)tauntCosmeticElement).Id)), true, 0.35f);
		AnimationVariationText = GetAnimationVariationText(defaultAction);
		ItemVisual.BannerCode = "";
		ItemVisual.InitialPanRotation = 0f;
		_activeCosmeticID = ((CosmeticElement)tauntCosmeticElement).Id;
		IsOpenedWithTauntItem = true;
		ObtainDescriptionText = ((object)new TextObject("{=6mrCNU5U}You will obtain this taunt", (Dictionary<string, object>)null)).ToString();
		GameTexts.SetVariable("STR1", GameTexts.FindText("str_continue", (string)null));
		GameTexts.SetVariable("STR2", item.Cost);
		ContinueText = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		CanObtain = TauntItem?.Cost <= NetworkMain.GameClient.PlayerData.Gold;
	}

	private string GetAnimationVariationText(string animationName)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (animationName.EndsWith("leftstance"))
		{
			return ((object)new TextObject("{=8DSymjRe}Left Stance", (Dictionary<string, object>)null)).ToString();
		}
		if (animationName.EndsWith("bow"))
		{
			return ((object)new TextObject("{=5rj7xQE4}Bow", (Dictionary<string, object>)null)).ToString();
		}
		return ((object)new TextObject("{=fMSYE6Ii}Default", (Dictionary<string, object>)null)).ToString();
	}

	public void ExecuteSelectNextAnimation(int increment)
	{
		if (TauntItem?.TauntCosmeticElement == null)
		{
			Debug.FailedAssert("Invalid taunt cosmetic item", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\MPCosmeticObtainPopupVM.cs", "ExecuteSelectNextAnimation", 177);
			return;
		}
		TauntUsageSet usageSet = TauntUsageManager.Instance.GetUsageSet(((CosmeticElement)_tauntItem.TauntCosmeticElement).Id);
		if (usageSet == null)
		{
			Debug.FailedAssert("No usage set for taunt: " + ((CosmeticElement)TauntItem.TauntCosmeticElement).Id, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\MPCosmeticObtainPopupVM.cs", "ExecuteSelectNextAnimation", 185);
			return;
		}
		MBReadOnlyList<TauntUsage> usages = usageSet.GetUsages();
		if (usages == null || ((List<TauntUsage>)(object)usages).Count == 0)
		{
			Debug.FailedAssert("No usages assigned for taunt usage set: " + ((CosmeticElement)TauntItem.TauntCosmeticElement).Id, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\MPCosmeticObtainPopupVM.cs", "ExecuteSelectNextAnimation", 193);
			return;
		}
		_currentTauntUsageIndex += increment;
		if (_currentTauntUsageIndex >= ((List<TauntUsage>)(object)usages).Count)
		{
			_currentTauntUsageIndex = 0;
		}
		else if (_currentTauntUsageIndex < 0)
		{
			_currentTauntUsageIndex = ((List<TauntUsage>)(object)usages).Count - 1;
		}
		string action = ((List<TauntUsage>)(object)usages)[_currentTauntUsageIndex].GetAction();
		CharacterVisual.ExecuteStartCustomAnimation(((List<TauntUsage>)(object)usages)[_currentTauntUsageIndex].GetAction(), true, 0f);
		AnimationVariationText = GetAnimationVariationText(action);
	}

	public void OpenWith(MPLobbyCosmeticSigilItemVM sigilItem)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		OnOpened();
		SigilItem = sigilItem;
		_activeCosmeticID = sigilItem.CosmeticID;
		IsOpenedWithSigilItem = true;
		ItemVisual.InitialPanRotation = -3.3f;
		ObtainDescriptionText = ((object)new TextObject("{=7uILxbP5}You will obtain this item", (Dictionary<string, object>)null)).ToString();
		MPCultureItemVM mPCultureItemVM = ((IEnumerable<MPCultureItemVM>)Cultures).FirstOrDefault((MPCultureItemVM c) => c.IsSelected);
		if (mPCultureItemVM != null)
		{
			mPCultureItemVM.IsSelected = false;
		}
		((Collection<MPCultureItemVM>)(object)Cultures)[0].IsSelected = true;
		OnCultureSelection(((Collection<MPCultureItemVM>)(object)Cultures)[0]);
		GameTexts.SetVariable("STR1", GameTexts.FindText("str_continue", (string)null));
		GameTexts.SetVariable("STR2", sigilItem.Cost);
		ContinueText = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		CanObtain = SigilItem.Cost <= NetworkMain.GameClient.PlayerData.Gold;
	}

	private void OnOpened()
	{
		Item = null;
		SigilItem = null;
		IsOpenedWithSigilItem = false;
		IsOpenedWithClothingItem = false;
		IsOpenedWithTauntItem = false;
		IsObtainSuccessful = false;
		ObtainState = 0;
		IsEnabled = true;
		_currentLootTextObject.SetTextVariable("LOOT", NetworkMain.GameClient.PlayerData.Gold);
		CurrentLootText = ((object)_currentLootTextObject).ToString();
		ClickToCloseText = _getExitText?.Invoke();
	}

	internal async void ExecuteAction()
	{
		if (ObtainState == 2 || ObtainState == 3)
		{
			ExecuteClosePopup();
		}
		else
		{
			if (ObtainState != 0)
			{
				return;
			}
			ObtainState = 1;
			(bool, int) obj = await NetworkMain.GameClient.BuyCosmetic(_activeCosmeticID);
			bool item = obj.Item1;
			int item2 = obj.Item2;
			ContinueText = ((object)GameTexts.FindText("str_continue", (string)null)).ToString();
			if (item)
			{
				if (Item != null)
				{
					Item.IsUnlocked = true;
				}
				else if (SigilItem != null)
				{
					SigilItem.IsUnlocked = true;
				}
				NetworkMain.GameClient.PlayerData.Gold = item2;
				ObtainResultText = ((object)new TextObject("{=V0k0urbO}Item obtained", (Dictionary<string, object>)null)).ToString();
				string arg = (IsOpenedWithSigilItem ? SigilItem.CosmeticID : (IsOpenedWithClothingItem ? Item.CosmeticID : string.Empty));
				_onItemObtained(arg, item2);
				IsObtainSuccessful = true;
				ObtainState = 2;
				SoundEvent.PlaySound2D("event:/ui/multiplayer/shop_purchase_complete");
			}
			else
			{
				ObtainResultText = ((object)new TextObject("{=XtVZe9cC}Item can not be obtained", (Dictionary<string, object>)null)).ToString();
				IsObtainSuccessful = false;
				ObtainState = 3;
			}
		}
	}

	public void ExecuteClosePopup()
	{
		IsEnabled = false;
	}

	private void OnCultureSelection(MPCultureItemVM cultureItem)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>(_cultureShieldItemIDs[cultureItem.Culture]);
		Banner val2 = Banner.CreateOneColoredBannerWithOneIcon(cultureItem.Culture.BackgroundColor1, cultureItem.Culture.ForegroundColor1, SigilItem.IconID);
		ItemVisual.FillFrom(new EquipmentElement(val, (ItemModifier)null, (ItemObject)null, false), val2);
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}
}
