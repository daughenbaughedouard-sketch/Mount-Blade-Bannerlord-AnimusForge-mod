using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Missions.NameMarker;

public class MissionNameMarkerVM : ViewModel
{
	private class MarkerDistanceComparer : IComparer<MissionNameMarkerTargetBaseVM>
	{
		public int Compare(MissionNameMarkerTargetBaseVM x, MissionNameMarkerTargetBaseVM y)
		{
			return y.Distance.CompareTo(x.Distance);
		}
	}

	private readonly Camera _missionCamera;

	private bool _prevEnabledState;

	private bool _fadeOutTimerStarted;

	private float _fadeOutTimer;

	private readonly MarkerDistanceComparer _distanceComparer;

	private readonly List<MissionNameMarkerProvider> _providers;

	private MBBindingList<MissionNameMarkerTargetBaseVM> _targets;

	private bool _isEnabled;

	public bool IsTargetsAdded { get; private set; }

	[DataSourceProperty]
	public MBBindingList<MissionNameMarkerTargetBaseVM> Targets
	{
		get
		{
			return _targets;
		}
		set
		{
			if (value != _targets)
			{
				_targets = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionNameMarkerTargetBaseVM>>(value, "Targets");
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
				UpdateTargetStates(value);
				Game.Current.EventManager.TriggerEvent<MissionNameMarkerToggleEvent>(new MissionNameMarkerToggleEvent(value));
			}
		}
	}

	public MissionNameMarkerVM(List<MissionNameMarkerProvider> providers, Camera missionCamera)
	{
		Targets = new MBBindingList<MissionNameMarkerTargetBaseVM>();
		_providers = providers;
		_distanceComparer = new MarkerDistanceComparer();
		_missionCamera = missionCamera;
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Targets.ApplyActionOnAllItems((Action<MissionNameMarkerTargetBaseVM>)delegate(MissionNameMarkerTargetBaseVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		Targets.ApplyActionOnAllItems((Action<MissionNameMarkerTargetBaseVM>)delegate(MissionNameMarkerTargetBaseVM x)
		{
			((ViewModel)x).OnFinalize();
		});
	}

	public void Tick(float dt)
	{
		if (!IsTargetsAdded)
		{
			List<MissionNameMarkerTargetBaseVM> list = new List<MissionNameMarkerTargetBaseVM>();
			for (int i = 0; i < _providers.Count; i++)
			{
				_providers[i].CreateMarkers(list);
			}
			GetTargetDifferences((IList<MissionNameMarkerTargetBaseVM>)Targets, list, out var removedTargets, out var addedTargets);
			for (int j = 0; j < ((List<MissionNameMarkerTargetBaseVM>)(object)removedTargets).Count; j++)
			{
				((Collection<MissionNameMarkerTargetBaseVM>)(object)Targets).Remove(((List<MissionNameMarkerTargetBaseVM>)(object)removedTargets)[j]);
			}
			for (int k = 0; k < ((List<MissionNameMarkerTargetBaseVM>)(object)addedTargets).Count; k++)
			{
				((Collection<MissionNameMarkerTargetBaseVM>)(object)Targets).Add(((List<MissionNameMarkerTargetBaseVM>)(object)addedTargets)[k]);
			}
			IsTargetsAdded = true;
		}
		if (IsEnabled)
		{
			UpdateTargetScreenPositions(forceUpdate: false);
			_fadeOutTimerStarted = false;
			_fadeOutTimer = 0f;
			_prevEnabledState = IsEnabled;
		}
		else
		{
			if (_prevEnabledState)
			{
				_fadeOutTimerStarted = true;
			}
			if (_fadeOutTimerStarted)
			{
				_fadeOutTimer += dt;
			}
			if (_fadeOutTimer >= 2f)
			{
				_fadeOutTimerStarted = false;
			}
			UpdateTargetScreenPositions(_fadeOutTimer < 2f);
		}
		_prevEnabledState = IsEnabled;
	}

	private static void GetTargetDifferences(IList<MissionNameMarkerTargetBaseVM> currentTargets, IList<MissionNameMarkerTargetBaseVM> newTargets, out MBReadOnlyList<MissionNameMarkerTargetBaseVM> removedTargets, out MBReadOnlyList<MissionNameMarkerTargetBaseVM> addedTargets)
	{
		MBList<MissionNameMarkerTargetBaseVM> val = new MBList<MissionNameMarkerTargetBaseVM>();
		MBList<MissionNameMarkerTargetBaseVM> val2 = new MBList<MissionNameMarkerTargetBaseVM>();
		for (int i = 0; i < currentTargets.Count; i++)
		{
			MissionNameMarkerTargetBaseVM missionNameMarkerTargetBaseVM = currentTargets[i];
			bool flag = true;
			for (int j = 0; j < newTargets.Count; j++)
			{
				MissionNameMarkerTargetBaseVM other = newTargets[j];
				if (missionNameMarkerTargetBaseVM.Equals(other))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				((List<MissionNameMarkerTargetBaseVM>)(object)val).Add(missionNameMarkerTargetBaseVM);
			}
		}
		for (int k = 0; k < newTargets.Count; k++)
		{
			MissionNameMarkerTargetBaseVM missionNameMarkerTargetBaseVM2 = newTargets[k];
			bool flag2 = true;
			for (int l = 0; l < currentTargets.Count; l++)
			{
				if (currentTargets[l].Equals(missionNameMarkerTargetBaseVM2))
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				((List<MissionNameMarkerTargetBaseVM>)(object)val2).Add(missionNameMarkerTargetBaseVM2);
			}
		}
		removedTargets = (MBReadOnlyList<MissionNameMarkerTargetBaseVM>)(object)val;
		addedTargets = (MBReadOnlyList<MissionNameMarkerTargetBaseVM>)(object)val2;
	}

	public void SetTargetsDirty()
	{
		IsTargetsAdded = false;
	}

	private void UpdateTargetScreenPositions(bool forceUpdate)
	{
		for (int i = 0; i < ((Collection<MissionNameMarkerTargetBaseVM>)(object)Targets).Count; i++)
		{
			MissionNameMarkerTargetBaseVM missionNameMarkerTargetBaseVM = ((Collection<MissionNameMarkerTargetBaseVM>)(object)Targets)[i];
			if (missionNameMarkerTargetBaseVM.IsEnabled || forceUpdate)
			{
				missionNameMarkerTargetBaseVM.UpdatePosition(_missionCamera);
			}
		}
		Targets.Sort((IComparer<MissionNameMarkerTargetBaseVM>)_distanceComparer);
	}

	private void UpdateTargetStates(bool state)
	{
		foreach (MissionNameMarkerTargetBaseVM item in (Collection<MissionNameMarkerTargetBaseVM>)(object)Targets)
		{
			item.SetEnabledState(state);
		}
	}
}
