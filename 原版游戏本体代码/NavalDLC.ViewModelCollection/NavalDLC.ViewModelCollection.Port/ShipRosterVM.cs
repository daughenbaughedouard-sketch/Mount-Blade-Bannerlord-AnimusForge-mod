using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port;

public class ShipRosterVM : ViewModel
{
	private class PortShipVMComparer : IComparer<ShipItemVM>
	{
		private readonly MBReadOnlyList<Ship> _orderedShipsList;

		public PortShipVMComparer(MBReadOnlyList<Ship> orderedShipsList)
		{
			_orderedShipsList = orderedShipsList;
		}

		public int Compare(ShipItemVM x, ShipItemVM y)
		{
			int num = ((List<Ship>)(object)_orderedShipsList).IndexOf(x.Ship);
			int value = ((List<Ship>)(object)_orderedShipsList).IndexOf(y.Ship);
			return num.CompareTo(value);
		}
	}

	private TextObject _rosterName;

	private readonly Action _onSelected;

	private bool _hasAnyShips;

	private bool _hasMultipleShips;

	private bool _hasOwnerCharacter;

	private bool _isSelected;

	private bool _isTownShipyard;

	private int _townShipyardLevel;

	private string _name;

	private string _hasNoShipsText;

	private string _shipCountText;

	private string _weightText;

	private string _troopCountText;

	private bool _isWeightDangerous;

	private bool _isTroopCountDangerous;

	private MBBindingList<ShipItemVM> _ships;

	private CharacterImageIdentifierVM _ownerVisual;

	private HintViewModel _tooltip;

	public PartyBase Owner { get; private set; }

	[DataSourceProperty]
	public bool HasAnyShips
	{
		get
		{
			return _hasAnyShips;
		}
		set
		{
			if (value != _hasAnyShips)
			{
				_hasAnyShips = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasAnyShips");
			}
		}
	}

	[DataSourceProperty]
	public bool HasMultipleShips
	{
		get
		{
			return _hasMultipleShips;
		}
		set
		{
			if (value != _hasMultipleShips)
			{
				_hasMultipleShips = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasMultipleShips");
			}
		}
	}

	[DataSourceProperty]
	public bool HasOwnerCharacter
	{
		get
		{
			return _hasOwnerCharacter;
		}
		set
		{
			if (value != _hasOwnerCharacter)
			{
				_hasOwnerCharacter = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasOwnerCharacter");
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
	public bool IsTownShipyard
	{
		get
		{
			return _isTownShipyard;
		}
		set
		{
			if (value != _isTownShipyard)
			{
				_isTownShipyard = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTownShipyard");
			}
		}
	}

	[DataSourceProperty]
	public int TownShipyardLevel
	{
		get
		{
			return _townShipyardLevel;
		}
		set
		{
			if (value != _townShipyardLevel)
			{
				_townShipyardLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TownShipyardLevel");
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
	public string HasNoShipsText
	{
		get
		{
			return _hasNoShipsText;
		}
		set
		{
			if (value != _hasNoShipsText)
			{
				_hasNoShipsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "HasNoShipsText");
			}
		}
	}

	[DataSourceProperty]
	public string ShipCountText
	{
		get
		{
			return _shipCountText;
		}
		set
		{
			if (value != _shipCountText)
			{
				_shipCountText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ShipCountText");
			}
		}
	}

	[DataSourceProperty]
	public string WeightText
	{
		get
		{
			return _weightText;
		}
		set
		{
			if (value != _weightText)
			{
				_weightText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "WeightText");
			}
		}
	}

	[DataSourceProperty]
	public string TroopCountText
	{
		get
		{
			return _troopCountText;
		}
		set
		{
			if (value != _troopCountText)
			{
				_troopCountText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TroopCountText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsWeightDangerous
	{
		get
		{
			return _isWeightDangerous;
		}
		set
		{
			if (value != _isWeightDangerous)
			{
				_isWeightDangerous = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsWeightDangerous");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTroopCountDangerous
	{
		get
		{
			return _isTroopCountDangerous;
		}
		set
		{
			if (value != _isTroopCountDangerous)
			{
				_isTroopCountDangerous = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTroopCountDangerous");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<ShipItemVM> Ships
	{
		get
		{
			return _ships;
		}
		set
		{
			if (value != _ships)
			{
				_ships = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<ShipItemVM>>(value, "Ships");
			}
		}
	}

	[DataSourceProperty]
	public CharacterImageIdentifierVM OwnerCharacterVisual
	{
		get
		{
			return _ownerVisual;
		}
		set
		{
			if (value != _ownerVisual)
			{
				_ownerVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterImageIdentifierVM>(value, "OwnerCharacterVisual");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel Tooltip
	{
		get
		{
			return _tooltip;
		}
		set
		{
			if (value != _tooltip)
			{
				_tooltip = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "Tooltip");
			}
		}
	}

	public ShipRosterVM(Action onSelected)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		_onSelected = onSelected;
		Ships = new MBBindingList<ShipItemVM>();
		Tooltip = new HintViewModel();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Expected O, but got Unknown
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Expected O, but got Unknown
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		((ViewModel)this).RefreshValues();
		Name = ((object)_rosterName)?.ToString();
		HasNoShipsText = ((object)new TextObject("{=vfXHD89T}No ships available", (Dictionary<string, object>)null)).ToString();
		ShipCountText = ((object)new TextObject("{=nx9Pk1ca}{AMOUNT} {?AMOUNT==1}ship{?}ships{\\?}", (Dictionary<string, object>)null).SetTextVariable("AMOUNT", ((Collection<ShipItemVM>)(object)Ships).Count)).ToString();
		if (HasOwnerCharacter)
		{
			float num;
			ExplainedNumber val;
			if (!Owner.IsMobile)
			{
				num = 0f;
			}
			else
			{
				val = Campaign.Current.Models.InventoryCapacityModel.CalculateTotalWeightCarried(Owner.MobileParty, true, false);
				num = ((ExplainedNumber)(ref val)).ResultNumber;
			}
			float num2 = num;
			float num3;
			if (!Owner.IsMobile || !HasAnyShips)
			{
				num3 = ((IEnumerable<ShipItemVM>)_ships).Sum((ShipItemVM x) => x.Ship.InventoryCapacity);
			}
			else
			{
				val = Campaign.Current.Models.InventoryCapacityModel.CalculateInventoryCapacity(Owner.MobileParty, true, false, 0, 0, 0, false);
				num3 = ((ExplainedNumber)(ref val)).ResultNumber;
			}
			float num4 = num3;
			WeightText = ((object)GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null).SetTextVariable("LEFT", (int)num2).SetTextVariable("RIGHT", (int)num4)).ToString();
			IsWeightDangerous = num2 > num4;
			int numberOfAllMembers = Owner.NumberOfAllMembers;
			int num5 = ((IEnumerable<ShipItemVM>)_ships).Sum((ShipItemVM x) => x.Ship.TotalCrewCapacity);
			TroopCountText = ((object)GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null).SetTextVariable("LEFT", numberOfAllMembers).SetTextVariable("RIGHT", num5)).ToString();
			IsTroopCountDangerous = numberOfAllMembers > num5;
		}
		else
		{
			WeightText = string.Empty;
			TroopCountText = string.Empty;
			IsWeightDangerous = false;
			IsTroopCountDangerous = false;
		}
		if (!HasAnyShips)
		{
			Tooltip.HintText = new TextObject("{=vfXHD89T}No ships available", (Dictionary<string, object>)null);
		}
		else if (IsTroopCountDangerous)
		{
			Tooltip.HintText = new TextObject("{=LPUWr7J1}Over the troop limit, sailing speed will be negatively affected!", (Dictionary<string, object>)null);
		}
		else if (IsWeightDangerous)
		{
			Tooltip.HintText = new TextObject("{=qSRbt9qc}Over the carrying limit, sailing speed will be negatively affected!", (Dictionary<string, object>)null);
		}
		else
		{
			Tooltip.HintText = null;
		}
		Ships.ApplyActionOnAllItems((Action<ShipItemVM>)delegate(ShipItemVM s)
		{
			((ViewModel)s).RefreshValues();
		});
	}

	public void SetRosterName(TextObject rosterName)
	{
		_rosterName = rosterName;
		((ViewModel)this).RefreshValues();
	}

	public void SetRosterOwner(PartyBase owner)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		Owner = owner;
		HasOwnerCharacter = Owner != null && Owner.LeaderHero != null;
		PartyBase owner2 = Owner;
		IsTownShipyard = owner2 != null && owner2.IsSettlement && Owner.Settlement.HasPort;
		int townShipyardLevel;
		if (!IsTownShipyard)
		{
			townShipyardLevel = 0;
		}
		else
		{
			Town town = Owner.Settlement.Town;
			int? obj;
			if (town == null)
			{
				obj = null;
			}
			else
			{
				Building shipyard = town.GetShipyard();
				obj = ((shipyard != null) ? new int?(shipyard.CurrentLevel) : ((int?)null));
			}
			townShipyardLevel = obj ?? 0;
		}
		TownShipyardLevel = townShipyardLevel;
		CharacterImageIdentifierVM ownerCharacterVisual = OwnerCharacterVisual;
		if (ownerCharacterVisual != null)
		{
			((ViewModel)ownerCharacterVisual).OnFinalize();
		}
		if (HasOwnerCharacter)
		{
			OwnerCharacterVisual = new CharacterImageIdentifierVM(CharacterCode.CreateFrom((BasicCharacterObject)(object)Owner.LeaderHero.CharacterObject));
		}
		else
		{
			OwnerCharacterVisual = null;
		}
		((ViewModel)this).RefreshValues();
	}

	public void RefreshShips(MBReadOnlyList<ShipItemVM> removedShips, MBReadOnlyList<ShipItemVM> addedShips, MBReadOnlyList<Ship> orderedShipsList)
	{
		for (int i = 0; i < ((List<ShipItemVM>)(object)removedShips).Count; i++)
		{
			((Collection<ShipItemVM>)(object)Ships).Remove(((List<ShipItemVM>)(object)removedShips)[i]);
		}
		for (int j = 0; j < ((List<ShipItemVM>)(object)addedShips).Count; j++)
		{
			((Collection<ShipItemVM>)(object)Ships).Add(((List<ShipItemVM>)(object)addedShips)[j]);
		}
		Ships.Sort((IComparer<ShipItemVM>)new PortShipVMComparer(orderedShipsList));
		HasAnyShips = ((Collection<ShipItemVM>)(object)Ships).Count > 0;
		HasMultipleShips = ((Collection<ShipItemVM>)(object)Ships).Count > 1;
		((ViewModel)this).RefreshValues();
	}

	public void ExecuteSelectRoster()
	{
		_onSelected?.Invoke();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		foreach (ShipItemVM item in (Collection<ShipItemVM>)(object)Ships)
		{
			((ViewModel)item).OnFinalize();
		}
		((Collection<ShipItemVM>)(object)Ships).Clear();
		CharacterImageIdentifierVM ownerCharacterVisual = OwnerCharacterVisual;
		if (ownerCharacterVisual != null)
		{
			((ViewModel)ownerCharacterVisual).OnFinalize();
		}
		OwnerCharacterVisual = null;
	}
}
