using System;
using System.Collections.ObjectModel;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.FormationMarker;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace NavalDLC.ViewModelCollection.Order;

public class NavalOrderTroopItemVM : OrderTroopItemVM
{
	private readonly NavalShipsLogic _navalShipsLogic;

	private readonly TextObject _troopCountTextObj;

	private readonly TextObject _healthTextObj;

	private MissionShip _cachedShip;

	private string _troopCountText;

	private string _healthText;

	private int _formationClassInt = 5;

	private string _prefabId;

	private bool _hasShip;

	private bool _isShipActive;

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
	public string HealthText
	{
		get
		{
			return _healthText;
		}
		set
		{
			if (value != _healthText)
			{
				_healthText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "HealthText");
			}
		}
	}

	[DataSourceProperty]
	public int FormationClassInt
	{
		get
		{
			return _formationClassInt;
		}
		set
		{
			if (value != _formationClassInt)
			{
				_formationClassInt = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "FormationClassInt");
			}
		}
	}

	[DataSourceProperty]
	public string PrefabId
	{
		get
		{
			return _prefabId;
		}
		set
		{
			if (value != _prefabId)
			{
				_prefabId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PrefabId");
			}
		}
	}

	[DataSourceProperty]
	public bool HasShip
	{
		get
		{
			return _hasShip;
		}
		set
		{
			if (value != _hasShip)
			{
				_hasShip = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasShip");
			}
		}
	}

	[DataSourceProperty]
	public bool IsShipActive
	{
		get
		{
			return _isShipActive;
		}
		set
		{
			if (value != _isShipActive)
			{
				_isShipActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsShipActive");
			}
		}
	}

	public NavalOrderTroopItemVM(Formation formation, Action<OrderTroopItemVM> setSelected, Func<Formation, int> getMorale)
		: base(formation, setSelected, getMorale)
	{
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_troopCountTextObj = GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null);
		_healthTextObj = GameTexts.FindText("str_NUMBER_percent", (string)null);
		((OrderTroopItemVM)this).UpdateVisuals();
	}

	public override void UpdateVisuals()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((OrderTroopItemVM)this).UpdateVisuals();
		if (base.Formation != null && _navalShipsLogic != null && _navalShipsLogic.GetShip((TeamSideEnum)0, base.Formation.FormationIndex, out var ship))
		{
			if (string.IsNullOrEmpty(PrefabId) || ship != _cachedShip)
			{
				_cachedShip = ship;
				HasShip = _cachedShip != null;
				MissionShip cachedShip = _cachedShip;
				IsShipActive = cachedShip != null && cachedShip.HitPoints > 0f;
				PrefabId = ((_cachedShip != null) ? NavalUIHelper.GetPrefabIdOfShipHull(_cachedShip.ShipOrigin.Hull) : null);
			}
		}
		else
		{
			PrefabId = null;
			HasShip = false;
			_cachedShip = null;
			IsShipActive = false;
		}
	}

	public override void Update()
	{
		((OrderTroopItemVM)this).Update();
		MissionShip cachedShip = _cachedShip;
		IsShipActive = cachedShip != null && cachedShip.HitPoints > 0f;
		if (IsShipActive)
		{
			TroopCountText = ((object)_troopCountTextObj.SetTextVariable("LEFT", base.Formation.CountOfUnits.ToString()).SetTextVariable("RIGHT", _cachedShip.CrewSizeOnMainDeck.ToString())).ToString();
			HealthText = ((object)_healthTextObj.SetTextVariable("NUMBER", ((int)(_cachedShip.HitPoints / _cachedShip.MaxHealth * 100f)).ToString())).ToString();
		}
		else
		{
			TroopCountText = base.Formation.CountOfUnits.ToString();
			HealthText = ((object)_healthTextObj.SetTextVariable("NUMBER", 0)).ToString();
		}
	}

	public override void RefreshTargetedOrderVisual()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (!IsShipActive)
		{
			((OrderTroopItemVM)this).RefreshTargetedOrderVisual();
			return;
		}
		bool flag = false;
		string currentOrderIconId = null;
		string currentTargetFormationType = null;
		if (_cachedShip.ShipOrder.MovementOrderEnum == ShipOrder.ShipMovementOrderEnum.Engage && _cachedShip.ShipOrder.TargetShip != null)
		{
			flag = true;
			currentTargetFormationType = "Ship_" + ((object)_cachedShip.ShipOrder.TargetShip.ShipOrigin.Hull.Type/*cast due to .constrained prefix*/).ToString();
			currentOrderIconId = "order_movement_advance";
		}
		if (!flag)
		{
			for (int i = 0; i < ((Collection<OrderItemVM>)(object)((OrderSubjectVM)this).ActiveOrders).Count; i++)
			{
				OrderItemVM val = ((Collection<OrderItemVM>)(object)((OrderSubjectVM)this).ActiveOrders)[i];
				if (val.Order.IsTargeted())
				{
					Formation targetFormation = base.Formation.TargetFormation;
					if (targetFormation != null)
					{
						_navalShipsLogic.GetShip(targetFormation, out var ship);
						currentTargetFormationType = ((ship == null) ? MissionFormationMarkerTargetVM.GetFormationType(targetFormation.PhysicalClass) : ("Ship_" + ((object)ship.ShipOrigin.Hull.Type/*cast due to .constrained prefix*/).ToString()));
						flag = true;
					}
					currentOrderIconId = ((OrderItemBaseVM)val).OrderIconId;
				}
			}
		}
		((OrderTroopItemVM)this).HasTarget = flag;
		((OrderTroopItemVM)this).CurrentOrderIconId = currentOrderIconId;
		((OrderTroopItemVM)this).CurrentTargetFormationType = currentTargetFormationType;
	}

	public void UpdateClassData(DeploymentFormationClass formationClass)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		FormationClassInt = (int)formationClass;
	}

	public override TextObject GetVisibleNameOfFormationForMessage()
	{
		if (IsShipActive)
		{
			return _cachedShip.ShipOrigin.Name;
		}
		return ((OrderTroopItemVM)this).GetVisibleNameOfFormationForMessage();
	}
}
