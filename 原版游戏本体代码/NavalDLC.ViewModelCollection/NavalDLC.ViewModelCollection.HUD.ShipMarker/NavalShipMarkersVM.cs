using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.ViewModelCollection.HUD.ShipMarker;

public class NavalShipMarkersVM : ViewModel
{
	public class ShipMarkerDistanceComparer : IComparer<NavalShipMarkerItemVM>
	{
		public int Compare(NavalShipMarkerItemVM x, NavalShipMarkerItemVM y)
		{
			return y.Distance.CompareTo(x.Distance);
		}
	}

	private readonly Mission _mission;

	private NavalShipsLogic _navalShipsLogic;

	private readonly ShipMarkerDistanceComparer _comparer;

	private bool _isEnabled;

	private bool _isShipTargetingRelevant;

	private bool _showDistanceTexts;

	private MBBindingList<NavalShipMarkerItemVM> _shipMarkers;

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
				for (int i = 0; i < ((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers).Count; i++)
				{
					((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers)[i].IsEnabled = value;
				}
			}
		}
	}

	[DataSourceProperty]
	public bool IsShipTargetingRelevant
	{
		get
		{
			return _isShipTargetingRelevant;
		}
		set
		{
			if (value != _isShipTargetingRelevant)
			{
				_isShipTargetingRelevant = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsShipTargetingRelevant");
				for (int i = 0; i < ((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers).Count; i++)
				{
					((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers)[i].IsShipTargetRelevant = value;
				}
			}
		}
	}

	[DataSourceProperty]
	public bool ShowDistanceTexts
	{
		get
		{
			return _showDistanceTexts;
		}
		set
		{
			if (value != _showDistanceTexts)
			{
				_showDistanceTexts = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowDistanceTexts");
				for (int i = 0; i < ((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers).Count; i++)
				{
					((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers)[i].ShowDistanceTexts = value;
				}
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<NavalShipMarkerItemVM> ShipMarkers
	{
		get
		{
			return _shipMarkers;
		}
		set
		{
			if (value != _shipMarkers)
			{
				_shipMarkers = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<NavalShipMarkerItemVM>>(value, "ShipMarkers");
			}
		}
	}

	public NavalShipMarkersVM(Mission mission)
	{
		_mission = mission;
		_comparer = new ShipMarkerDistanceComparer();
		ShipMarkers = new MBBindingList<NavalShipMarkerItemVM>();
	}

	public void RefreshShipMarkers()
	{
		if (_navalShipsLogic == null)
		{
			_navalShipsLogic = _mission.GetMissionBehavior<NavalShipsLogic>();
		}
		if (_navalShipsLogic == null)
		{
			((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers).Clear();
			return;
		}
		List<Formation> allFormations = ((IEnumerable<Team>)_mission.Teams).SelectMany((Team x) => (IEnumerable<Formation>)x.FormationsIncludingSpecialAndEmpty).ToList();
		GetShipChanges(allFormations, ShipMarkers, out var markersToRemove, out var markersToAdd);
		for (int num = 0; num < ((List<NavalShipMarkerItemVM>)(object)markersToRemove).Count; num++)
		{
			NavalShipMarkerItemVM item = ((List<NavalShipMarkerItemVM>)(object)markersToRemove)[num];
			((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers).Remove(item);
		}
		for (int num2 = 0; num2 < ((List<NavalShipMarkerItemVM>)(object)markersToAdd).Count; num2++)
		{
			NavalShipMarkerItemVM navalShipMarkerItemVM = ((List<NavalShipMarkerItemVM>)(object)markersToAdd)[num2];
			((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers).Add(navalShipMarkerItemVM);
			navalShipMarkerItemVM.IsEnabled = IsEnabled;
			navalShipMarkerItemVM.IsShipTargetRelevant = IsShipTargetingRelevant;
			navalShipMarkerItemVM.ShowDistanceTexts = ShowDistanceTexts;
		}
		ShipMarkers.Sort((IComparer<NavalShipMarkerItemVM>)_comparer);
		for (int num3 = 0; num3 < ((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers).Count; num3++)
		{
			NavalShipMarkerItemVM navalShipMarkerItemVM2 = ((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers)[num3];
			navalShipMarkerItemVM2.Refresh();
			navalShipMarkerItemVM2.IsEnabled = IsEnabled && navalShipMarkerItemVM2.Ship != _navalShipsLogic.PlayerControlledShip;
		}
	}

	private void GetShipChanges(List<Formation> allFormations, MBBindingList<NavalShipMarkerItemVM> activeMarkers, out MBList<NavalShipMarkerItemVM> markersToRemove, out MBList<NavalShipMarkerItemVM> markersToAdd)
	{
		markersToAdd = new MBList<NavalShipMarkerItemVM>();
		markersToRemove = new MBList<NavalShipMarkerItemVM>();
		List<(Formation, MissionShip)> list = new List<(Formation, MissionShip)>();
		for (int i = 0; i < allFormations.Count; i++)
		{
			Formation val = allFormations[i];
			_navalShipsLogic.GetShip(val, out var ship);
			if ((ship != null || val.CountOfUnits > 0) && (ship == null || (!((MissionObject)ship).IsDisabled && !ship.IsRemoved)))
			{
				list.Add((val, ship));
			}
		}
		for (int j = 0; j < ((Collection<NavalShipMarkerItemVM>)(object)activeMarkers).Count; j++)
		{
			NavalShipMarkerItemVM navalShipMarkerItemVM = ((Collection<NavalShipMarkerItemVM>)(object)activeMarkers)[j];
			bool flag = false;
			for (int k = 0; k < list.Count; k++)
			{
				Formation item = list[k].Item1;
				MissionShip item2 = list[k].Item2;
				if (item == navalShipMarkerItemVM.Formation && item2 == navalShipMarkerItemVM.Ship)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				((List<NavalShipMarkerItemVM>)(object)markersToRemove).Add(navalShipMarkerItemVM);
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			Formation item3 = list[l].Item1;
			MissionShip item4 = list[l].Item2;
			bool flag2 = false;
			for (int m = 0; m < ((Collection<NavalShipMarkerItemVM>)(object)activeMarkers).Count; m++)
			{
				NavalShipMarkerItemVM navalShipMarkerItemVM2 = ((Collection<NavalShipMarkerItemVM>)(object)activeMarkers)[m];
				if (navalShipMarkerItemVM2.Formation == item3 && navalShipMarkerItemVM2.Ship == item4)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				((List<NavalShipMarkerItemVM>)(object)markersToAdd).Add(new NavalShipMarkerItemVM(item3, item4));
			}
		}
	}

	public void UpdateCrewCounts()
	{
		for (int i = 0; i < ((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers).Count; i++)
		{
			NavalShipMarkerItemVM navalShipMarkerItemVM = ((Collection<NavalShipMarkerItemVM>)(object)ShipMarkers)[i];
			navalShipMarkerItemVM.CrewCount = navalShipMarkerItemVM.Formation.CountOfUnits;
		}
	}
}
