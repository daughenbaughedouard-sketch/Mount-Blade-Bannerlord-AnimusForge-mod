using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.MapSiege;

public class MapSiegeVM : ViewModel
{
	public class SiegePOIDistanceComparer : IComparer<MapSiegePOIVM>
	{
		public int Compare(MapSiegePOIVM x, MapSiegePOIVM y)
		{
			return y.LatestW.CompareTo(x.LatestW);
		}
	}

	private readonly Camera _mapCamera;

	private readonly SiegePOIDistanceComparer _poiDistanceComparer;

	private MBBindingList<MapSiegePOIVM> _pointsOfInterest;

	private MapSiegeProductionVM _productionController;

	private float _preparationProgress;

	private string _preparationTitleText;

	private bool _isPreparationsCompleted;

	private bool IsPlayerLeaderOfSiegeEvent
	{
		get
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			SiegeEvent playerSiegeEvent = PlayerSiege.PlayerSiegeEvent;
			if (playerSiegeEvent != null && playerSiegeEvent.IsPlayerSiegeEvent)
			{
				return Campaign.Current.Models.EncounterModel.GetLeaderOfSiegeEvent(PlayerSiege.PlayerSiegeEvent, PlayerSiege.PlayerSide) == Hero.MainHero;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public float PreparationProgress
	{
		get
		{
			return _preparationProgress;
		}
		set
		{
			if (value != _preparationProgress)
			{
				_preparationProgress = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PreparationProgress");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPreparationsCompleted
	{
		get
		{
			return _isPreparationsCompleted;
		}
		set
		{
			if (value != _isPreparationsCompleted)
			{
				_isPreparationsCompleted = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPreparationsCompleted");
			}
		}
	}

	[DataSourceProperty]
	public string PreparationTitleText
	{
		get
		{
			return _preparationTitleText;
		}
		set
		{
			if (value != _preparationTitleText)
			{
				_preparationTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PreparationTitleText");
			}
		}
	}

	[DataSourceProperty]
	public MapSiegeProductionVM ProductionController
	{
		get
		{
			return _productionController;
		}
		set
		{
			if (value != _productionController)
			{
				_productionController = value;
				((ViewModel)this).OnPropertyChangedWithValue<MapSiegeProductionVM>(value, "ProductionController");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MapSiegePOIVM> PointsOfInterest
	{
		get
		{
			return _pointsOfInterest;
		}
		set
		{
			if (value != _pointsOfInterest)
			{
				_pointsOfInterest = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MapSiegePOIVM>>(value, "PointsOfInterest");
			}
		}
	}

	public MapSiegeVM(Camera mapCamera, MatrixFrame[] batteringRamFrames, MatrixFrame[] rangedSiegeEngineFrames, MatrixFrame[] towerSiegeEngineFrames, MatrixFrame[] defenderSiegeEngineFrames, MatrixFrame[] breachableWallFrames)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		_mapCamera = mapCamera;
		PointsOfInterest = new MBBindingList<MapSiegePOIVM>();
		_poiDistanceComparer = new SiegePOIDistanceComparer();
		for (int i = 0; i < batteringRamFrames.Length; i++)
		{
			((Collection<MapSiegePOIVM>)(object)PointsOfInterest).Add(new MapSiegePOIVM(MapSiegePOIVM.POIType.AttackerRamSiegeMachine, batteringRamFrames[i], _mapCamera, i, OnPOISelection));
		}
		for (int j = 0; j < rangedSiegeEngineFrames.Length; j++)
		{
			((Collection<MapSiegePOIVM>)(object)PointsOfInterest).Add(new MapSiegePOIVM(MapSiegePOIVM.POIType.AttackerRangedSiegeMachine, rangedSiegeEngineFrames[j], _mapCamera, j, OnPOISelection));
		}
		for (int k = 0; k < towerSiegeEngineFrames.Length; k++)
		{
			((Collection<MapSiegePOIVM>)(object)PointsOfInterest).Add(new MapSiegePOIVM(MapSiegePOIVM.POIType.AttackerTowerSiegeMachine, towerSiegeEngineFrames[k], _mapCamera, batteringRamFrames.Length + k, OnPOISelection));
		}
		for (int l = 0; l < defenderSiegeEngineFrames.Length; l++)
		{
			((Collection<MapSiegePOIVM>)(object)PointsOfInterest).Add(new MapSiegePOIVM(MapSiegePOIVM.POIType.DefenderSiegeMachine, defenderSiegeEngineFrames[l], _mapCamera, l, OnPOISelection));
		}
		for (int m = 0; m < breachableWallFrames.Length; m++)
		{
			((Collection<MapSiegePOIVM>)(object)PointsOfInterest).Add(new MapSiegePOIVM(MapSiegePOIVM.POIType.WallSection, breachableWallFrames[m], _mapCamera, m, OnPOISelection));
		}
		ProductionController = new MapSiegeProductionVM();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		((ViewModel)this).RefreshValues();
		PreparationTitleText = ((object)GameTexts.FindText("str_building_siege_camp", (string)null)).ToString();
		SiegeEvent playerSiegeEvent = PlayerSiege.PlayerSiegeEvent;
		IsPreparationsCompleted = (playerSiegeEvent != null && playerSiegeEvent.BesiegerCamp.IsPreparationComplete) || (int)PlayerSiege.PlayerSide == 0;
		((ViewModel)ProductionController).RefreshValues();
		PointsOfInterest.ApplyActionOnAllItems((Action<MapSiegePOIVM>)delegate(MapSiegePOIVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	private void OnPOISelection(MapSiegePOIVM poi)
	{
		if (ProductionController.LatestSelectedPOI != null)
		{
			ProductionController.LatestSelectedPOI.IsSelected = false;
		}
		if (IsPlayerLeaderOfSiegeEvent)
		{
			ProductionController.OnMachineSelection(poi);
		}
	}

	public void OnSelectionFromScene(MatrixFrame frameOfEngine)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		if (PlayerSiege.PlayerSiegeEvent == null)
		{
			return;
		}
		Settlement besiegedSettlement = PlayerSiege.BesiegedSettlement;
		if ((besiegedSettlement == null || (int)besiegedSettlement.CurrentSiegeState != 1) && IsPlayerLeaderOfSiegeEvent)
		{
			((IEnumerable<MapSiegePOIVM>)PointsOfInterest).Where((MapSiegePOIVM poi) => ((MatrixFrame)(ref frameOfEngine)).NearlyEquals(poi.MapSceneLocationFrame, 1E-05f))?.FirstOrDefault()?.ExecuteSelection();
		}
	}

	public void Update(float mapCameraDistanceValue)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		SiegeEvent playerSiegeEvent = PlayerSiege.PlayerSiegeEvent;
		IsPreparationsCompleted = (playerSiegeEvent != null && playerSiegeEvent.BesiegerCamp.IsPreparationComplete) || (int)PlayerSiege.PlayerSide == 0;
		SiegeEvent playerSiegeEvent2 = PlayerSiege.PlayerSiegeEvent;
		float? obj;
		if (playerSiegeEvent2 == null)
		{
			obj = null;
		}
		else
		{
			SiegeEnginesContainer siegeEngines = playerSiegeEvent2.BesiegerCamp.SiegeEngines;
			if (siegeEngines == null)
			{
				obj = null;
			}
			else
			{
				SiegeEngineConstructionProgress siegePreparations = siegeEngines.SiegePreparations;
				obj = ((siegePreparations != null) ? new float?(siegePreparations.Progress) : ((float?)null));
			}
		}
		PreparationProgress = obj ?? 0f;
		TWParallel.For(0, ((Collection<MapSiegePOIVM>)(object)PointsOfInterest).Count, (ParallelForAuxPredicate)delegate(int startInclusive, int endExclusive)
		{
			for (int i = startInclusive; i < endExclusive; i++)
			{
				((Collection<MapSiegePOIVM>)(object)PointsOfInterest)[i].RefreshDistanceValue(mapCameraDistanceValue);
				((Collection<MapSiegePOIVM>)(object)PointsOfInterest)[i].RefreshPosition();
				((Collection<MapSiegePOIVM>)(object)PointsOfInterest)[i].UpdateProperties();
			}
		}, 16);
		foreach (MapSiegePOIVM item in (Collection<MapSiegePOIVM>)(object)PointsOfInterest)
		{
			item.RefreshBinding();
		}
		ProductionController.Update();
		PointsOfInterest.Sort((IComparer<MapSiegePOIVM>)_poiDistanceComparer);
	}
}
