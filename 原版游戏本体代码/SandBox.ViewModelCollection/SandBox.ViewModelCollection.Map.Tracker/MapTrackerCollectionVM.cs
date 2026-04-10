using System;
using System.Collections.ObjectModel;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.Tracker;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Map.Tracker;

public class MapTrackerCollectionVM : ViewModel
{
	private readonly MapTrackerProvider _mapTrackerProvider;

	private MBBindingList<MapTrackerItemVM> _trackers;

	public MBBindingList<MapTrackerItemVM> Trackers
	{
		get
		{
			return _trackers;
		}
		set
		{
			if (value != _trackers)
			{
				_trackers = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MapTrackerItemVM>>(value, "Trackers");
			}
		}
	}

	public MapTrackerCollectionVM()
	{
		_mapTrackerProvider = new MapTrackerProvider();
		Trackers = new MBBindingList<MapTrackerItemVM>();
		MapTrackerItemVM[] trackers = _mapTrackerProvider.GetTrackers();
		foreach (MapTrackerItemVM item in trackers)
		{
			((Collection<MapTrackerItemVM>)(object)Trackers).Add(item);
		}
		_mapTrackerProvider.OnTrackerAddedOrRemoved += OnTrackerAddedOrRemoved;
	}

	private void OnTrackerAddedOrRemoved(MapTrackerItemVM item, bool added)
	{
		if (added)
		{
			((Collection<MapTrackerItemVM>)(object)Trackers).Add(item);
		}
		else
		{
			((Collection<MapTrackerItemVM>)(object)Trackers).Remove(item);
		}
	}

	public void Tick(float dt)
	{
		for (int i = 0; i < ((Collection<MapTrackerItemVM>)(object)Trackers).Count; i++)
		{
			((Collection<MapTrackerItemVM>)(object)Trackers)[i].RefreshBinding();
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		_mapTrackerProvider.OnTrackerAddedOrRemoved -= OnTrackerAddedOrRemoved;
		Trackers.ApplyActionOnAllItems((Action<MapTrackerItemVM>)delegate(MapTrackerItemVM t)
		{
			((ViewModel)t).OnFinalize();
		});
	}

	public void UpdateProperties()
	{
		Trackers.ApplyActionOnAllItems((Action<MapTrackerItemVM>)delegate(MapTrackerItemVM t)
		{
			t.UpdateProperties();
		});
	}
}
