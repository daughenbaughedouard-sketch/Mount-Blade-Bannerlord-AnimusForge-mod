using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.ViewModelCollection.MapSiege;

public class MapSiegePOIVM : ViewModel
{
	public enum POIType
	{
		WallSection,
		DefenderSiegeMachine,
		AttackerRamSiegeMachine,
		AttackerTowerSiegeMachine,
		AttackerRangedSiegeMachine
	}

	public enum MachineTypes
	{
		None = -1,
		Wall,
		BrokenWall,
		Ballista,
		Trebuchet,
		Ladder,
		Ram,
		SiegeTower,
		Mangonel
	}

	private readonly Vec3 _mapSceneLocation;

	private readonly Camera _mapCamera;

	private readonly BattleSideEnum _thisSide;

	private readonly Action<MapSiegePOIVM> _onSelection;

	private float _latestX;

	private float _latestY;

	private float _latestW;

	private float _bindCurrentHitpoints;

	private float _bindMaxHitpoints;

	private float _bindWPos;

	private int _bindWSign;

	private int _bindMachineType = -1;

	private int _bindQueueIndex;

	private bool _bindIsInside;

	private bool _bindHasItem;

	private bool _bindIsConstructing;

	private Vec2 _bindPosition;

	private bool _bindIsInVisibleRange;

	private Color _sidePrimaryColor;

	private Color _sideSecondaryColor;

	private Vec2 _position;

	private float _currentHitpoints;

	private int _machineType = -1;

	private float _maxHitpoints;

	private int _queueIndex;

	private bool _isInside;

	private bool _hasItem;

	private bool _isConstructing;

	private bool _isPlayerSidePOI;

	private bool _isFireVersion;

	private bool _isInVisibleRange;

	private bool _isSelected;

	private SiegeEvent Siege => PlayerSiege.PlayerSiegeEvent;

	private BattleSideEnum PlayerSide => PlayerSiege.PlayerSide;

	private Settlement Settlement => Siege.BesiegedSettlement;

	public POIType Type { get; }

	public int MachineIndex { get; }

	public float LatestW => _latestW;

	public SiegeEngineConstructionProgress Machine { get; private set; }

	public MatrixFrame MapSceneLocationFrame { get; private set; }

	public Vec2 Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _position;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (_position != value)
			{
				_position = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Position");
			}
		}
	}

	public Color SidePrimaryColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _sidePrimaryColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (_sidePrimaryColor != value)
			{
				_sidePrimaryColor = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SidePrimaryColor");
			}
		}
	}

	public Color SideSecondaryColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _sideSecondaryColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (_sideSecondaryColor != value)
			{
				_sideSecondaryColor = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SideSecondaryColor");
			}
		}
	}

	public int QueueIndex
	{
		get
		{
			return _queueIndex;
		}
		set
		{
			if (_queueIndex != value)
			{
				_queueIndex = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "QueueIndex");
			}
		}
	}

	public int MachineType
	{
		get
		{
			return _machineType;
		}
		set
		{
			if (_machineType != value)
			{
				_machineType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MachineType");
			}
		}
	}

	public float CurrentHitpoints
	{
		get
		{
			return _currentHitpoints;
		}
		set
		{
			if (_currentHitpoints != value)
			{
				_currentHitpoints = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CurrentHitpoints");
			}
		}
	}

	public float MaxHitpoints
	{
		get
		{
			return _maxHitpoints;
		}
		set
		{
			if (_maxHitpoints != value)
			{
				_maxHitpoints = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MaxHitpoints");
			}
		}
	}

	public bool IsPlayerSidePOI
	{
		get
		{
			return _isPlayerSidePOI;
		}
		set
		{
			if (_isPlayerSidePOI != value)
			{
				_isPlayerSidePOI = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerSidePOI");
			}
		}
	}

	public bool IsFireVersion
	{
		get
		{
			return _isFireVersion;
		}
		set
		{
			if (_isFireVersion != value)
			{
				_isFireVersion = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFireVersion");
			}
		}
	}

	public bool IsInVisibleRange
	{
		get
		{
			return _isInVisibleRange;
		}
		set
		{
			if (_isInVisibleRange != value)
			{
				_isInVisibleRange = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInVisibleRange");
			}
		}
	}

	public bool IsConstructing
	{
		get
		{
			return _isConstructing;
		}
		set
		{
			if (_isConstructing != value)
			{
				_isConstructing = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsConstructing");
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	public bool HasItem
	{
		get
		{
			return _hasItem;
		}
		set
		{
			if (_hasItem != value)
			{
				_hasItem = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasItem");
			}
		}
	}

	public bool IsInside
	{
		get
		{
			return _isInside;
		}
		set
		{
			if (_isInside != value)
			{
				_isInside = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInside");
			}
		}
	}

	public MapSiegePOIVM(POIType type, MatrixFrame mapSceneLocation, Camera mapCamera, int machineIndex, Action<MapSiegePOIVM> onSelection)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Invalid comparison between Unknown and I4
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Invalid comparison between Unknown and I4
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		Type = type;
		_onSelection = onSelection;
		_thisSide = (BattleSideEnum)((Type == POIType.AttackerRamSiegeMachine || Type == POIType.AttackerTowerSiegeMachine || Type == POIType.AttackerRangedSiegeMachine) ? 1 : 0);
		MapSceneLocationFrame = mapSceneLocation;
		_mapSceneLocation = MapSceneLocationFrame.origin;
		_mapCamera = mapCamera;
		MachineIndex = machineIndex;
		Color sidePrimaryColor;
		if ((int)_thisSide != 1)
		{
			IFaction mapFaction = Siege.BesiegedSettlement.MapFaction;
			sidePrimaryColor = Color.FromUint((mapFaction != null) ? mapFaction.Color : 0u);
		}
		else
		{
			IFaction mapFaction2 = Siege.BesiegerCamp.MapFaction;
			sidePrimaryColor = Color.FromUint((mapFaction2 != null) ? mapFaction2.Color : 0u);
		}
		SidePrimaryColor = sidePrimaryColor;
		Color sideSecondaryColor;
		if ((int)_thisSide != 1)
		{
			IFaction mapFaction3 = Siege.BesiegedSettlement.MapFaction;
			sideSecondaryColor = Color.FromUint((mapFaction3 != null) ? mapFaction3.Color2 : 0u);
		}
		else
		{
			IFaction mapFaction4 = Siege.BesiegerCamp.MapFaction;
			sideSecondaryColor = Color.FromUint((mapFaction4 != null) ? mapFaction4.Color2 : 0u);
		}
		SideSecondaryColor = sideSecondaryColor;
		IsPlayerSidePOI = DetermineIfPOIIsPlayerSide();
	}

	public void ExecuteSelection()
	{
		_onSelection(this);
		IsSelected = true;
	}

	public void UpdateProperties()
	{
		Machine = GetDesiredMachine();
		_bindHasItem = Type == POIType.WallSection || Machine != null;
		SiegeEngineConstructionProgress machine = Machine;
		_bindIsConstructing = machine != null && !machine.IsActive;
		RefreshMachineType();
		RefreshHitpoints();
		RefreshQueueIndex();
	}

	public void RefreshDistanceValue(float newDistance)
	{
		_bindIsInVisibleRange = newDistance <= 20f;
	}

	public void RefreshPosition()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		_latestX = 0f;
		_latestY = 0f;
		_latestW = 0f;
		MBWindowManager.WorldToScreenInsideUsableArea(_mapCamera, _mapSceneLocation, ref _latestX, ref _latestY, ref _latestW);
		if (!MathF.IsValidValue(_latestX) || !MathF.IsValidValue(_latestY) || !MathF.IsValidValue(_latestW))
		{
			_latestX = -10000f;
			_latestY = -10000f;
			_latestW = -1f;
		}
		_bindWPos = _latestW;
		_bindWSign = (int)_bindWPos;
		_bindIsInside = IsInsideWindow();
		_bindPosition = new Vec2(_latestX, _latestY);
	}

	public void RefreshBinding()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Position = _bindPosition;
		IsInside = _bindIsInside;
		CurrentHitpoints = _bindCurrentHitpoints;
		MaxHitpoints = _bindMaxHitpoints;
		HasItem = _bindHasItem;
		IsConstructing = _bindIsConstructing;
		MachineType = _bindMachineType;
		QueueIndex = _bindQueueIndex;
		IsInVisibleRange = _bindIsInVisibleRange;
	}

	private void RefreshHitpoints()
	{
		if (Siege != null)
		{
			switch (Type)
			{
			case POIType.WallSection:
			{
				MBReadOnlyList<float> settlementWallSectionHitPointsRatioList = Settlement.SettlementWallSectionHitPointsRatioList;
				_bindMaxHitpoints = Settlement.MaxWallHitPoints / (float)Settlement.WallSectionCount;
				_bindCurrentHitpoints = ((List<float>)(object)settlementWallSectionHitPointsRatioList)[MachineIndex] * _bindMaxHitpoints;
				_bindMachineType = ((_bindCurrentHitpoints <= 0f) ? 1 : 0);
				break;
			}
			case POIType.DefenderSiegeMachine:
			case POIType.AttackerRamSiegeMachine:
			case POIType.AttackerTowerSiegeMachine:
			case POIType.AttackerRangedSiegeMachine:
				if (Machine != null)
				{
					if (Machine.IsActive)
					{
						_bindCurrentHitpoints = Machine.Hitpoints;
						_bindMaxHitpoints = Machine.MaxHitPoints;
					}
					else if (Machine.IsBeingRedeployed)
					{
						_bindCurrentHitpoints = Machine.RedeploymentProgress;
						_bindMaxHitpoints = 1f;
					}
					else
					{
						_bindCurrentHitpoints = Machine.Progress;
						_bindMaxHitpoints = 1f;
					}
				}
				else
				{
					_bindCurrentHitpoints = 0f;
					_bindMaxHitpoints = 0f;
				}
				break;
			}
		}
		else
		{
			_bindCurrentHitpoints = 0f;
			_bindMaxHitpoints = 0f;
		}
	}

	private void RefreshMachineType()
	{
		if (Siege != null)
		{
			switch (Type)
			{
			case POIType.WallSection:
				_bindMachineType = 0;
				break;
			case POIType.DefenderSiegeMachine:
			case POIType.AttackerRamSiegeMachine:
			case POIType.AttackerTowerSiegeMachine:
			case POIType.AttackerRangedSiegeMachine:
				_bindMachineType = (int)((Machine != null) ? GetMachineTypeFromId(((MBObjectBase)Machine.SiegeEngine).StringId) : MachineTypes.None);
				break;
			}
		}
		else
		{
			_bindMachineType = -1;
		}
	}

	private void RefreshQueueIndex()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_bindQueueIndex = ((Machine != null) ? ((IEnumerable<SiegeEngineConstructionProgress>)Siege.GetSiegeEventSide(PlayerSide).SiegeEngines.DeployedSiegeEngines).Where((SiegeEngineConstructionProgress e) => !e.IsActive).ToList().IndexOf(Machine) : (-1));
	}

	private bool DetermineIfPOIIsPlayerSide()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		switch (Type)
		{
		case POIType.WallSection:
		case POIType.DefenderSiegeMachine:
			return (int)PlayerSide == 0;
		case POIType.AttackerRamSiegeMachine:
		case POIType.AttackerTowerSiegeMachine:
		case POIType.AttackerRangedSiegeMachine:
			return (int)PlayerSide == 1;
		default:
			return false;
		}
	}

	private bool IsInsideWindow()
	{
		if (!(_latestX - 100f > Screen.RealScreenResolutionWidth) && !(_latestY - 100f > Screen.RealScreenResolutionHeight) && !(_latestX + 100f < 0f))
		{
			return !(_latestY + 100f < 0f);
		}
		return false;
	}

	public void ExecuteShowTooltip()
	{
		InformationManager.ShowTooltip(typeof(List<TooltipProperty>), new object[1] { SandBoxUIHelper.GetSiegeEngineInProgressTooltip(Machine) });
	}

	public void ExecuteHideTooltip()
	{
		MBInformationManager.HideInformations();
	}

	private MachineTypes GetMachineTypeFromId(string id)
	{
		switch (id.ToLower())
		{
		case "ballista":
		case "fire_ballista":
			return MachineTypes.Ballista;
		case "ram":
			return MachineTypes.Ram;
		case "siege_tower_level1":
		case "siege_tower_level2":
		case "siege_tower_level3":
			return MachineTypes.SiegeTower;
		case "catapult":
		case "mangonel":
		case "onager":
		case "fire_onager":
		case "fire_mangonel":
		case "fire_catapult":
			return MachineTypes.Mangonel;
		case "trebuchet":
		case "bricole":
			return MachineTypes.Trebuchet;
		case "ladder":
			return MachineTypes.Ladder;
		default:
			return MachineTypes.None;
		}
	}

	private SiegeEngineConstructionProgress GetDesiredMachine()
	{
		if (Siege != null)
		{
			switch (Type)
			{
			case POIType.DefenderSiegeMachine:
				return Siege.GetSiegeEventSide((BattleSideEnum)0).SiegeEngines.DeployedRangedSiegeEngines[MachineIndex];
			case POIType.AttackerRamSiegeMachine:
			case POIType.AttackerTowerSiegeMachine:
				return Siege.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedMeleeSiegeEngines[MachineIndex];
			case POIType.AttackerRangedSiegeMachine:
				return Siege.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedRangedSiegeEngines[MachineIndex];
			default:
				return null;
			}
		}
		return null;
	}
}
