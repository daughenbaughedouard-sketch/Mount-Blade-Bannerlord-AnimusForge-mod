using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NavalDLC.CampaignBehaviors;
using NavalDLC.Missions.Deployment;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;

namespace NavalDLC.ViewModelCollection.OrderOfBattle;

public class NavalOrderOfBattleVM : ViewModel
{
	private readonly MBList<NavalOrderOfBattleFormationItemVM> _allFormations;

	private readonly List<NavalOrderOfBattleHeroItemVM> _allHeroes;

	private readonly List<NavalOrderOfBattleShipItemVM> _allShips;

	private NavalShipsLogic _navalShipsLogic;

	private NavalDeploymentMissionController _navalDeploymentController;

	private OrderController _orderController;

	private NavalOrderOfBattleCampaignBehavior _navalOrderOfBattleCampaignBehavior;

	private AssignPlayerRoleInTeamMissionController _assignPlayerRoleInTeamMissioncontroller;

	private readonly Action<NavalOrderOfBattleFormationItemVM> _onFormationSelected;

	private readonly Action _clearFormationSelection;

	private readonly Action _onAutoDeploy;

	private readonly Action _onBeginMission;

	private readonly Mission _mission;

	private readonly TextObject _formationsDisabledHintGeneral = new TextObject("{=ZixS1b4u}You're not leading this battle.", (Dictionary<string, object>)null);

	private readonly TextObject _formationsDisabledHintAllies = new TextObject("{=O4n4SAqo}Formation is reserved for allied parties.", (Dictionary<string, object>)null);

	private readonly TextObject _formationsDisabledHintSkills = new TextObject("{=Vs5NavCd}You do not have enough skills/perks for this formation.", (Dictionary<string, object>)null);

	private readonly TextObject _formationsDisabledHintShips = new TextObject("{=bID6axoH}You do not have enough ships for this formation.", (Dictionary<string, object>)null);

	private bool _finalizeInitializationOnNextUpdate;

	private bool _isLoadingConfigurationAgents;

	private bool _isEnabled;

	private bool _isAssignmentDirty;

	private bool _canStartMission;

	private bool _isPlayerGeneral;

	private bool _areCameraControlsEnabled;

	private bool _hasSelectedHero;

	private bool _hasSelectedShip;

	private string _beginMissionText;

	private string _autoDeployText;

	private NavalOrderOfBattleShipItemVM _selectedShip;

	private NavalOrderOfBattleHeroItemVM _selectedHero;

	private MBBindingList<NavalOrderOfBattleFormationItemVM> _leftFormations;

	private MBBindingList<NavalOrderOfBattleFormationItemVM> _rightFormations;

	private MBBindingList<NavalOrderOfBattleHeroItemVM> _unassignedHeroes;

	private MBBindingList<NavalOrderOfBattleShipItemVM> _unassignedShips;

	private bool _areHotkeysEnabled;

	private bool _isPoolAcceptingHero;

	private bool _isPoolAcceptingShip;

	private HintViewModel _canStartHint;

	private bool _canToggleHeroOrShipSelection;

	private InputKeyItemVM _doneInputKey;

	private InputKeyItemVM _resetInputKey;

	public MBReadOnlyList<NavalOrderOfBattleFormationItemVM> AllFormations => (MBReadOnlyList<NavalOrderOfBattleFormationItemVM>)(object)_allFormations;

	public List<FormationConfiguration> CurrentFilterConfiguration { get; private set; }

	public List<ClassConfiguration> CurrentClassConfiguration { get; private set; }

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
	public bool IsAssignmentDirty
	{
		get
		{
			return _isAssignmentDirty;
		}
		set
		{
			if (value != _isAssignmentDirty)
			{
				_isAssignmentDirty = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAssignmentDirty");
			}
		}
	}

	[DataSourceProperty]
	public bool CanStartMission
	{
		get
		{
			return _canStartMission;
		}
		set
		{
			if (value != _canStartMission)
			{
				_canStartMission = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanStartMission");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerGeneral
	{
		get
		{
			return _isPlayerGeneral;
		}
		set
		{
			if (value != _isPlayerGeneral)
			{
				_isPlayerGeneral = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerGeneral");
			}
		}
	}

	[DataSourceProperty]
	public bool AreCameraControlsEnabled
	{
		get
		{
			return _areCameraControlsEnabled;
		}
		set
		{
			if (value != _areCameraControlsEnabled)
			{
				_areCameraControlsEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AreCameraControlsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool HasSelectedHero
	{
		get
		{
			return _hasSelectedHero;
		}
		set
		{
			if (value != _hasSelectedHero)
			{
				_hasSelectedHero = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasSelectedHero");
			}
		}
	}

	[DataSourceProperty]
	public bool HasSelectedShip
	{
		get
		{
			return _hasSelectedShip;
		}
		set
		{
			if (value != _hasSelectedShip)
			{
				_hasSelectedShip = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasSelectedShip");
			}
		}
	}

	[DataSourceProperty]
	public string BeginMissionText
	{
		get
		{
			return _beginMissionText;
		}
		set
		{
			if (value != _beginMissionText)
			{
				_beginMissionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BeginMissionText");
			}
		}
	}

	[DataSourceProperty]
	public string AutoDeployText
	{
		get
		{
			return _autoDeployText;
		}
		set
		{
			if (value != _autoDeployText)
			{
				_autoDeployText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AutoDeployText");
			}
		}
	}

	[DataSourceProperty]
	public NavalOrderOfBattleShipItemVM SelectedShip
	{
		get
		{
			return _selectedShip;
		}
		set
		{
			if (value != _selectedShip)
			{
				if (_selectedShip != null)
				{
					_selectedShip.IsSelected = false;
				}
				_selectedShip = value;
				((ViewModel)this).OnPropertyChangedWithValue<NavalOrderOfBattleShipItemVM>(value, "SelectedShip");
				HasSelectedShip = _selectedShip != null;
				if (_selectedShip != null)
				{
					_selectedShip.IsSelected = true;
				}
				OnSelectionUpdated();
			}
		}
	}

	[DataSourceProperty]
	public NavalOrderOfBattleHeroItemVM SelectedHero
	{
		get
		{
			return _selectedHero;
		}
		set
		{
			if (value != _selectedHero)
			{
				if (_selectedHero != null)
				{
					_selectedHero.IsSelected = false;
				}
				_selectedHero = value;
				((ViewModel)this).OnPropertyChangedWithValue<NavalOrderOfBattleHeroItemVM>(value, "SelectedHero");
				HasSelectedHero = _selectedHero != null;
				if (_selectedHero != null)
				{
					_selectedHero.IsSelected = true;
				}
				OnSelectionUpdated();
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<NavalOrderOfBattleFormationItemVM> LeftFormations
	{
		get
		{
			return _leftFormations;
		}
		set
		{
			if (value != _leftFormations)
			{
				_leftFormations = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<NavalOrderOfBattleFormationItemVM>>(value, "LeftFormations");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<NavalOrderOfBattleFormationItemVM> RightFormations
	{
		get
		{
			return _rightFormations;
		}
		set
		{
			if (value != _rightFormations)
			{
				_rightFormations = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<NavalOrderOfBattleFormationItemVM>>(value, "RightFormations");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<NavalOrderOfBattleHeroItemVM> UnassignedHeroes
	{
		get
		{
			return _unassignedHeroes;
		}
		set
		{
			if (value != _unassignedHeroes)
			{
				_unassignedHeroes = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<NavalOrderOfBattleHeroItemVM>>(value, "UnassignedHeroes");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<NavalOrderOfBattleShipItemVM> UnassignedShips
	{
		get
		{
			return _unassignedShips;
		}
		set
		{
			if (value != _unassignedShips)
			{
				_unassignedShips = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<NavalOrderOfBattleShipItemVM>>(value, "UnassignedShips");
			}
		}
	}

	[DataSourceProperty]
	public bool AreHotkeysEnabled
	{
		get
		{
			return _areHotkeysEnabled;
		}
		set
		{
			if (value != _areHotkeysEnabled)
			{
				_areHotkeysEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AreHotkeysEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPoolAcceptingHero
	{
		get
		{
			return _isPoolAcceptingHero;
		}
		set
		{
			if (value != _isPoolAcceptingHero)
			{
				_isPoolAcceptingHero = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPoolAcceptingHero");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPoolAcceptingShip
	{
		get
		{
			return _isPoolAcceptingShip;
		}
		set
		{
			if (value != _isPoolAcceptingShip)
			{
				_isPoolAcceptingShip = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPoolAcceptingShip");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CanStartHint
	{
		get
		{
			return _canStartHint;
		}
		set
		{
			if (value != _canStartHint)
			{
				_canStartHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "CanStartHint");
			}
		}
	}

	[DataSourceProperty]
	public bool CanToggleHeroOrShipSelection
	{
		get
		{
			return _canToggleHeroOrShipSelection;
		}
		set
		{
			if (value != _canToggleHeroOrShipSelection)
			{
				_canToggleHeroOrShipSelection = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanToggleHeroOrShipSelection");
			}
		}
	}

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
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "DoneInputKey");
			}
		}
	}

	public InputKeyItemVM ResetInputKey
	{
		get
		{
			return _resetInputKey;
		}
		set
		{
			if (value != _resetInputKey)
			{
				_resetInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "ResetInputKey");
			}
		}
	}

	public NavalOrderOfBattleVM(Mission mission, Action<NavalOrderOfBattleFormationItemVM> onFormationSelected, Action clearFormationSelection, Action onAutoDeploy, Action onBeginMission)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		_mission = mission;
		_onFormationSelected = onFormationSelected;
		_clearFormationSelection = clearFormationSelection;
		_onAutoDeploy = onAutoDeploy;
		_onBeginMission = onBeginMission;
		_allFormations = new MBList<NavalOrderOfBattleFormationItemVM>();
		LeftFormations = new MBBindingList<NavalOrderOfBattleFormationItemVM>();
		RightFormations = new MBBindingList<NavalOrderOfBattleFormationItemVM>();
		_allHeroes = new List<NavalOrderOfBattleHeroItemVM>();
		_allShips = new List<NavalOrderOfBattleShipItemVM>();
		UnassignedHeroes = new MBBindingList<NavalOrderOfBattleHeroItemVM>();
		UnassignedShips = new MBBindingList<NavalOrderOfBattleShipItemVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		BeginMissionText = ((object)new TextObject("{=SYYOSOoa}Ready", (Dictionary<string, object>)null)).ToString();
		AutoDeployText = ((object)GameTexts.FindText("str_auto_deploy", (string)null)).ToString();
		_allHeroes.ForEach(delegate(NavalOrderOfBattleHeroItemVM h)
		{
			((ViewModel)h).RefreshValues();
		});
		_allShips.ForEach(delegate(NavalOrderOfBattleShipItemVM s)
		{
			((ViewModel)s).RefreshValues();
		});
		LeftFormations.ApplyActionOnAllItems((Action<NavalOrderOfBattleFormationItemVM>)delegate(NavalOrderOfBattleFormationItemVM f)
		{
			((ViewModel)f).RefreshValues();
		});
		RightFormations.ApplyActionOnAllItems((Action<NavalOrderOfBattleFormationItemVM>)delegate(NavalOrderOfBattleFormationItemVM f)
		{
			((ViewModel)f).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		if (IsEnabled)
		{
			SaveConfiguration();
		}
		if (_navalDeploymentController != null)
		{
			_navalDeploymentController.PlayerShipsUpdated -= OnPlayerShipsUpdated;
			_navalDeploymentController = null;
		}
		if (_orderController != null)
		{
			_orderController.OnSelectedFormationsChanged -= OnSelectedFormationsChanged;
			_orderController = null;
		}
		NavalOrderOfBattleFormationItemVM.OnAcceptCaptain = (Action<NavalOrderOfBattleFormationItemVM>)Delegate.Remove(NavalOrderOfBattleFormationItemVM.OnAcceptCaptain, new Action<NavalOrderOfBattleFormationItemVM>(OnFormationAcceptCaptain));
		NavalOrderOfBattleFormationItemVM.OnAcceptShip = (Action<NavalOrderOfBattleFormationItemVM>)Delegate.Remove(NavalOrderOfBattleFormationItemVM.OnAcceptShip, new Action<NavalOrderOfBattleFormationItemVM>(OnFormationAcceptShip));
		NavalOrderOfBattleFormationItemVM.GetTotalTroopCountWithFilter = (Func<DeploymentFormationClass, FormationFilterType, int>)Delegate.Remove(NavalOrderOfBattleFormationItemVM.GetTotalTroopCountWithFilter, new Func<DeploymentFormationClass, FormationFilterType, int>(GetTroopCountWithFilter));
		IsEnabled = false;
		InputKeyItemVM doneInputKey = DoneInputKey;
		if (doneInputKey != null)
		{
			((ViewModel)doneInputKey).OnFinalize();
		}
		DoneInputKey = null;
		InputKeyItemVM resetInputKey = ResetInputKey;
		if (resetInputKey != null)
		{
			((ViewModel)resetInputKey).OnFinalize();
		}
		ResetInputKey = null;
		LeftFormations.ApplyActionOnAllItems((Action<NavalOrderOfBattleFormationItemVM>)delegate(NavalOrderOfBattleFormationItemVM f)
		{
			((ViewModel)f).OnFinalize();
		});
		((Collection<NavalOrderOfBattleFormationItemVM>)(object)LeftFormations).Clear();
		RightFormations.ApplyActionOnAllItems((Action<NavalOrderOfBattleFormationItemVM>)delegate(NavalOrderOfBattleFormationItemVM f)
		{
			((ViewModel)f).OnFinalize();
		});
		((Collection<NavalOrderOfBattleFormationItemVM>)(object)RightFormations).Clear();
		((List<NavalOrderOfBattleFormationItemVM>)(object)_allFormations).Clear();
		_allHeroes.ForEach(delegate(NavalOrderOfBattleHeroItemVM h)
		{
			((ViewModel)h).OnFinalize();
		});
		_allShips.ForEach(delegate(NavalOrderOfBattleShipItemVM s)
		{
			((ViewModel)s).OnFinalize();
		});
		_allHeroes.Clear();
		_allShips.Clear();
		((Collection<NavalOrderOfBattleHeroItemVM>)(object)UnassignedHeroes).Clear();
		((Collection<NavalOrderOfBattleShipItemVM>)(object)UnassignedShips).Clear();
	}

	public void Initialize()
	{
		_navalShipsLogic = _mission.GetMissionBehavior<NavalShipsLogic>();
		_navalDeploymentController = _mission.GetMissionBehavior<NavalDeploymentMissionController>();
		_assignPlayerRoleInTeamMissioncontroller = _mission.GetMissionBehavior<AssignPlayerRoleInTeamMissionController>();
		_navalDeploymentController.PlayerShipsUpdated += OnPlayerShipsUpdated;
		_orderController = _mission.PlayerTeam.PlayerOrderController;
		_orderController.OnSelectedFormationsChanged += OnSelectedFormationsChanged;
		NavalOrderOfBattleFormationItemVM.OnAcceptCaptain = (Action<NavalOrderOfBattleFormationItemVM>)Delegate.Combine(NavalOrderOfBattleFormationItemVM.OnAcceptCaptain, new Action<NavalOrderOfBattleFormationItemVM>(OnFormationAcceptCaptain));
		NavalOrderOfBattleFormationItemVM.OnAcceptShip = (Action<NavalOrderOfBattleFormationItemVM>)Delegate.Combine(NavalOrderOfBattleFormationItemVM.OnAcceptShip, new Action<NavalOrderOfBattleFormationItemVM>(OnFormationAcceptShip));
		NavalOrderOfBattleFormationItemVM.GetTotalTroopCountWithFilter = (Func<DeploymentFormationClass, FormationFilterType, int>)Delegate.Combine(NavalOrderOfBattleFormationItemVM.GetTotalTroopCountWithFilter, new Func<DeploymentFormationClass, FormationFilterType, int>(GetTroopCountWithFilter));
		IsPlayerGeneral = _mission.PlayerTeam.IsPlayerGeneral;
		CurrentFilterConfiguration = new List<FormationConfiguration>();
		CurrentClassConfiguration = new List<ClassConfiguration>();
		RefreshAll();
		Campaign current = Campaign.Current;
		_navalOrderOfBattleCampaignBehavior = ((current != null) ? current.GetCampaignBehavior<NavalOrderOfBattleCampaignBehavior>() : null);
		LoadConfigurationShips();
		if (IsAssignmentDirty)
		{
			_finalizeInitializationOnNextUpdate = true;
		}
		else
		{
			FinalizeInitialization();
		}
		IsEnabled = true;
	}

	public void ExecuteAutoDeploy()
	{
		if (!IsAssignmentDirty)
		{
			IsAssignmentDirty = true;
			_onAutoDeploy?.Invoke();
		}
	}

	public void ExecuteBeginMission()
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if (IsAssignmentDirty || !CanStartMission)
		{
			return;
		}
		CurrentFilterConfiguration?.Clear();
		CurrentClassConfiguration?.Clear();
		foreach (NavalOrderOfBattleFormationItemVM item in (List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)
		{
			if (item.Formation.CountOfUnits > 0)
			{
				CurrentFilterConfiguration?.Add(new FormationConfiguration(item.Formation.Index, (from f in (IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)item.FilterItems
					where f.IsActive
					select f.FilterType).ToList()));
				CurrentClassConfiguration?.Add(new ClassConfiguration(item.Formation.Index, item.SelectedClass));
			}
			else
			{
				CurrentFilterConfiguration?.Add(new FormationConfiguration(item.Formation.Index, new List<FormationFilterType>()));
				CurrentClassConfiguration?.Add(new ClassConfiguration(item.Formation.Index, (DeploymentFormationClass)1));
			}
		}
		_onBeginMission?.Invoke();
		MBInformationManager.HideInformations();
	}

	public void ExecuteClearHeroAndShipSelection()
	{
		SelectedHero = null;
		SelectedShip = null;
	}

	public bool OnEscape()
	{
		bool result = false;
		if (((IEnumerable<NavalOrderOfBattleFormationItemVM>)AllFormations).Any((NavalOrderOfBattleFormationItemVM x) => x.IsSelected))
		{
			_clearFormationSelection?.Invoke();
			result = true;
		}
		return result;
	}

	private void RefreshFormations()
	{
		if (((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count != 0)
		{
			return;
		}
		MBReadOnlyList<Formation> usableFormations = _navalDeploymentController.GetUsableFormations();
		for (int i = 0; i < ((List<Formation>)(object)usableFormations).Count; i++)
		{
			NavalOrderOfBattleFormationItemVM item = new NavalOrderOfBattleFormationItemVM(((List<Formation>)(object)usableFormations)[i], OnFormationSelected, OnClassChanged, OnFilterUseToggled);
			if (i < ((List<Formation>)(object)usableFormations).Count / 2)
			{
				((Collection<NavalOrderOfBattleFormationItemVM>)(object)LeftFormations).Add(item);
			}
			else
			{
				((Collection<NavalOrderOfBattleFormationItemVM>)(object)RightFormations).Add(item);
			}
			((List<NavalOrderOfBattleFormationItemVM>)(object)_allFormations).Add(item);
		}
	}

	private void RefreshShips()
	{
		if (_allShips.Count == 0)
		{
			foreach (IShipOrigin item in (List<IShipOrigin>)(object)_navalDeploymentController.GetAllPlayerShips())
			{
				_allShips.Add(new NavalOrderOfBattleShipItemVM(item, OnShipSelected, FindFormationOfShip));
			}
		}
		for (int i = 0; i < _allShips.Count; i++)
		{
			NavalOrderOfBattleShipItemVM navalOrderOfBattleShipItemVM = _allShips[i];
			ShipAssignment shipAssignment;
			bool flag = _navalShipsLogic.FindAssignmentOfShipOrigin(navalOrderOfBattleShipItemVM.ShipOrigin, out shipAssignment);
			int isDisabled;
			if (!IsPlayerGeneral)
			{
				if (((IEnumerable<IShipOrigin>)PartyBase.MainParty.Ships).Contains(navalOrderOfBattleShipItemVM.ShipOrigin))
				{
					if (flag)
					{
						Agent captain = shipAssignment.Formation.Captain;
						isDisabled = ((captain == null || !captain.IsMainAgent) ? 1 : 0);
					}
					else
					{
						isDisabled = 0;
					}
				}
				else
				{
					isDisabled = 1;
				}
			}
			else
			{
				isDisabled = 0;
			}
			navalOrderOfBattleShipItemVM.IsDisabled = (byte)isDisabled != 0;
			if (flag)
			{
				navalOrderOfBattleShipItemVM.MissionShip = shipAssignment.MissionShip;
				if (((Collection<NavalOrderOfBattleShipItemVM>)(object)UnassignedShips).Contains(navalOrderOfBattleShipItemVM))
				{
					((Collection<NavalOrderOfBattleShipItemVM>)(object)UnassignedShips).Remove(navalOrderOfBattleShipItemVM);
				}
				for (int j = 0; j < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; j++)
				{
					if (((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[j].Formation == shipAssignment.Formation && ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[j].Ship != navalOrderOfBattleShipItemVM)
					{
						((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[j].Ship = navalOrderOfBattleShipItemVM;
					}
					else if (((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[j].Formation != shipAssignment.Formation && ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[j].Ship == navalOrderOfBattleShipItemVM)
					{
						((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[j].Ship = null;
					}
				}
				continue;
			}
			navalOrderOfBattleShipItemVM.MissionShip = null;
			for (int k = 0; k < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; k++)
			{
				if (((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[k].Ship == navalOrderOfBattleShipItemVM)
				{
					((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[k].Ship = null;
				}
			}
			if (!navalOrderOfBattleShipItemVM.IsDisabled && !((Collection<NavalOrderOfBattleShipItemVM>)(object)UnassignedShips).Contains(navalOrderOfBattleShipItemVM))
			{
				((Collection<NavalOrderOfBattleShipItemVM>)(object)UnassignedShips).Add(navalOrderOfBattleShipItemVM);
			}
			else if (navalOrderOfBattleShipItemVM.IsDisabled && ((Collection<NavalOrderOfBattleShipItemVM>)(object)UnassignedShips).Contains(navalOrderOfBattleShipItemVM))
			{
				((Collection<NavalOrderOfBattleShipItemVM>)(object)UnassignedShips).Remove(navalOrderOfBattleShipItemVM);
			}
		}
	}

	private void RefreshHeroes()
	{
		if (_allHeroes.Count == 0)
		{
			foreach (IAgentOriginBase allPlayerTeamHero in _navalDeploymentController.GetAllPlayerTeamHeroes())
			{
				_allHeroes.Add(new NavalOrderOfBattleHeroItemVM(allPlayerTeamHero, OnHeroSelected));
			}
		}
		for (int i = 0; i < _allHeroes.Count; i++)
		{
			NavalOrderOfBattleHeroItemVM heroVM = _allHeroes[i];
			NavalOrderOfBattleFormationItemVM navalOrderOfBattleFormationItemVM = ((IEnumerable<NavalOrderOfBattleFormationItemVM>)AllFormations).FirstOrDefault(delegate(NavalOrderOfBattleFormationItemVM x)
			{
				Agent captain = x.Formation.Captain;
				return ((captain != null) ? captain.Origin : null) == heroVM.AgentOrigin;
			});
			heroVM.IsDisabled = !IsPlayerGeneral && !heroVM.IsMainHero;
			if (navalOrderOfBattleFormationItemVM != null)
			{
				if (((Collection<NavalOrderOfBattleHeroItemVM>)(object)UnassignedHeroes).Contains(heroVM))
				{
					((Collection<NavalOrderOfBattleHeroItemVM>)(object)UnassignedHeroes).Remove(heroVM);
				}
				for (int num = 0; num < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; num++)
				{
					NavalOrderOfBattleFormationItemVM navalOrderOfBattleFormationItemVM2 = ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[num];
					if (navalOrderOfBattleFormationItemVM2 == navalOrderOfBattleFormationItemVM && navalOrderOfBattleFormationItemVM2.Captain != heroVM)
					{
						navalOrderOfBattleFormationItemVM2.Captain = heroVM;
					}
					else if (navalOrderOfBattleFormationItemVM2 != navalOrderOfBattleFormationItemVM && navalOrderOfBattleFormationItemVM2.Captain == heroVM)
					{
						navalOrderOfBattleFormationItemVM2.Captain = null;
					}
				}
				continue;
			}
			for (int num2 = 0; num2 < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; num2++)
			{
				if (((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[num2].Captain == heroVM)
				{
					((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[num2].Captain = null;
				}
			}
			if (!heroVM.IsDisabled && !((Collection<NavalOrderOfBattleHeroItemVM>)(object)UnassignedHeroes).Contains(heroVM))
			{
				((Collection<NavalOrderOfBattleHeroItemVM>)(object)UnassignedHeroes).Add(heroVM);
			}
			else if (heroVM.IsDisabled && ((Collection<NavalOrderOfBattleHeroItemVM>)(object)UnassignedHeroes).Contains(heroVM))
			{
				((Collection<NavalOrderOfBattleHeroItemVM>)(object)UnassignedHeroes).Remove(heroVM);
			}
		}
	}

	private void RefreshFormationsDisabledAndReason()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		_navalShipsLogic.GetShipDeploymentLimit((TeamSideEnum)0, out var deploymentLimit);
		NavalShipDeploymentLimit deploymentLimit2;
		int shipDeploymentLimit = _navalShipsLogic.GetShipDeploymentLimit((TeamSideEnum)1, out deploymentLimit2);
		int num = _allShips.Count((NavalOrderOfBattleShipItemVM x) => !x.IsDisabled);
		for (int num2 = 0; num2 < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; num2++)
		{
			NavalOrderOfBattleFormationItemVM navalOrderOfBattleFormationItemVM = ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[num2];
			int num3 = num2 + 1;
			if (navalOrderOfBattleFormationItemVM.Formation.PlayerOwner != Agent.Main)
			{
				navalOrderOfBattleFormationItemVM.IsEnabled = false;
				navalOrderOfBattleFormationItemVM.DisabledHint = new HintViewModel(_formationsDisabledHintGeneral, (string)null);
			}
			else if (num3 > 8 - shipDeploymentLimit)
			{
				navalOrderOfBattleFormationItemVM.IsEnabled = false;
				navalOrderOfBattleFormationItemVM.DisabledHint = new HintViewModel(_formationsDisabledHintAllies, (string)null);
			}
			else if (num3 > deploymentLimit.PartiesLimit)
			{
				navalOrderOfBattleFormationItemVM.IsEnabled = false;
				navalOrderOfBattleFormationItemVM.DisabledHint = new HintViewModel(_formationsDisabledHintSkills, (string)null);
			}
			else if (num3 > num)
			{
				navalOrderOfBattleFormationItemVM.IsEnabled = false;
				navalOrderOfBattleFormationItemVM.DisabledHint = new HintViewModel(_formationsDisabledHintShips, (string)null);
			}
			else
			{
				navalOrderOfBattleFormationItemVM.IsEnabled = true;
				navalOrderOfBattleFormationItemVM.DisabledHint = null;
			}
		}
	}

	private void RefreshCanStartMission()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		if (!IsPlayerGeneral)
		{
			CanStartMission = true;
			CanStartHint = null;
		}
		if (((IEnumerable<NavalOrderOfBattleFormationItemVM>)AllFormations).Any((NavalOrderOfBattleFormationItemVM x) => x.HasShip && x.TroopCount == 0))
		{
			CanStartMission = false;
			CanStartHint = new HintViewModel(new TextObject("{=UL3x9GoP}There is a ship without any troops!", (Dictionary<string, object>)null), (string)null);
		}
		else
		{
			CanStartMission = true;
			CanStartHint = null;
		}
	}

	private void FinalizeInitialization()
	{
		LoadConfigurationAgents();
		if (!IsPlayerGeneral)
		{
			_assignPlayerRoleInTeamMissioncontroller.OnPlayerChoiceFinalized();
			RefreshAll();
		}
	}

	private void RefreshAll()
	{
		ExecuteClearHeroAndShipSelection();
		_clearFormationSelection?.Invoke();
		RefreshFormations();
		RefreshShips();
		RefreshHeroes();
		RefreshFormationsDisabledAndReason();
		((ViewModel)this).RefreshValues();
		RefreshCanStartMission();
		IsAssignmentDirty = false;
	}

	private void LoadConfigurationShips()
	{
		if (_navalOrderOfBattleCampaignBehavior == null || !IsPlayerGeneral)
		{
			return;
		}
		for (int i = 0; i < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; i++)
		{
			NavalOrderOfBattleFormationItemVM navalOrderOfBattleFormationItemVM = ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[i];
			NavalOrderOfBattleCampaignBehavior.NavalOrderOfBattleFormationData formationInfo = _navalOrderOfBattleCampaignBehavior.GetFormationDataAtIndex(i, MobileParty.MainParty.Army != null);
			if (formationInfo == null || !navalOrderOfBattleFormationItemVM.IsEnabled)
			{
				continue;
			}
			if (formationInfo.Ship != null)
			{
				NavalOrderOfBattleShipItemVM navalOrderOfBattleShipItemVM = _allShips.FirstOrDefault((NavalOrderOfBattleShipItemVM x) => (object)x.ShipOrigin == formationInfo.Ship);
				if (navalOrderOfBattleShipItemVM != null && navalOrderOfBattleShipItemVM.GetCanBeUnassignedOrMoved() && navalOrderOfBattleFormationItemVM.GetCanAcceptShip())
				{
					AssignShipToFormation(navalOrderOfBattleShipItemVM, navalOrderOfBattleFormationItemVM);
					continue;
				}
				NavalOrderOfBattleShipItemVM ship = navalOrderOfBattleFormationItemVM.Ship;
				if (ship == null || ship.GetCanBeUnassignedOrMoved())
				{
					AssignShipToFormation(null, navalOrderOfBattleFormationItemVM);
				}
			}
			else
			{
				NavalOrderOfBattleShipItemVM ship2 = navalOrderOfBattleFormationItemVM.Ship;
				if (ship2 == null || ship2.GetCanBeUnassignedOrMoved())
				{
					AssignShipToFormation(null, navalOrderOfBattleFormationItemVM);
				}
			}
		}
	}

	private void LoadConfigurationAgents()
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Invalid comparison between Unknown and I4
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Invalid comparison between Unknown and I4
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Invalid comparison between Unknown and I4
		if (_navalOrderOfBattleCampaignBehavior == null || !IsPlayerGeneral)
		{
			return;
		}
		_isLoadingConfigurationAgents = true;
		for (int i = 0; i < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; i++)
		{
			NavalOrderOfBattleFormationItemVM navalOrderOfBattleFormationItemVM = ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[i];
			NavalOrderOfBattleCampaignBehavior.NavalOrderOfBattleFormationData formationInfo = _navalOrderOfBattleCampaignBehavior.GetFormationDataAtIndex(i, MobileParty.MainParty.Army != null);
			if (formationInfo == null || !navalOrderOfBattleFormationItemVM.IsEnabled)
			{
				continue;
			}
			if (formationInfo.Captain != null)
			{
				NavalOrderOfBattleHeroItemVM navalOrderOfBattleHeroItemVM = _allHeroes.FirstOrDefault((NavalOrderOfBattleHeroItemVM x) => (object)x.AgentOrigin.Troop == formationInfo.Captain.CharacterObject);
				if (navalOrderOfBattleHeroItemVM != null && navalOrderOfBattleHeroItemVM.GetCanBeUnassignedOrMoved() && navalOrderOfBattleFormationItemVM.GetCanAcceptCaptain())
				{
					AssignCaptainToFormation(navalOrderOfBattleHeroItemVM, navalOrderOfBattleFormationItemVM);
				}
				else
				{
					NavalOrderOfBattleHeroItemVM captain = navalOrderOfBattleFormationItemVM.Captain;
					if (captain == null || captain.GetCanBeUnassignedOrMoved())
					{
						AssignCaptainToFormation(null, navalOrderOfBattleFormationItemVM);
					}
				}
			}
			else
			{
				NavalOrderOfBattleHeroItemVM captain2 = navalOrderOfBattleFormationItemVM.Captain;
				if (captain2 == null || captain2.GetCanBeUnassignedOrMoved())
				{
					AssignCaptainToFormation(null, navalOrderOfBattleFormationItemVM);
				}
			}
			if ((int)formationInfo.FormationClass != 0 && navalOrderOfBattleFormationItemVM.IsSelectable)
			{
				if ((int)formationInfo.FormationClass == 1)
				{
					navalOrderOfBattleFormationItemVM.ExecuteSelectInfantry();
				}
				else if ((int)formationInfo.FormationClass == 2)
				{
					navalOrderOfBattleFormationItemVM.ExecuteSelectRanged();
				}
				else if ((int)formationInfo.FormationClass == 5)
				{
					navalOrderOfBattleFormationItemVM.ExecuteSelectInfantryAndRanged();
				}
				formationInfo.Filters.TryGetValue((FormationFilterType)1, out var value);
				formationInfo.Filters.TryGetValue((FormationFilterType)4, out var value2);
				formationInfo.Filters.TryGetValue((FormationFilterType)3, out var value3);
				formationInfo.Filters.TryGetValue((FormationFilterType)5, out var value4);
				formationInfo.Filters.TryGetValue((FormationFilterType)6, out var value5);
				((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)navalOrderOfBattleFormationItemVM.FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 1)).IsActive = value;
				((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)navalOrderOfBattleFormationItemVM.FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 3)).IsActive = value3;
				((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)navalOrderOfBattleFormationItemVM.FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 4)).IsActive = value2;
				((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)navalOrderOfBattleFormationItemVM.FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 5)).IsActive = value4;
				((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)navalOrderOfBattleFormationItemVM.FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 6)).IsActive = value5;
			}
		}
		_navalDeploymentController.UpdateShips((TeamSideEnum)0);
		IsAssignmentDirty = true;
		_isLoadingConfigurationAgents = false;
	}

	private void SaveConfiguration()
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		if (_navalOrderOfBattleCampaignBehavior == null || !IsPlayerGeneral || !MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle())
		{
			return;
		}
		List<NavalOrderOfBattleCampaignBehavior.NavalOrderOfBattleFormationData> list = new List<NavalOrderOfBattleCampaignBehavior.NavalOrderOfBattleFormationData>();
		for (int i = 0; i < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; i++)
		{
			NavalOrderOfBattleFormationItemVM formationItemVM = ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[i];
			IShipOrigin val = null;
			Hero captain = null;
			bool isSelectable = formationItemVM.IsSelectable;
			if (isSelectable)
			{
				if (formationItemVM.Ship?.ShipOrigin != null && !formationItemVM.Ship.IsDisabled)
				{
					val = formationItemVM.Ship.ShipOrigin;
				}
				if (formationItemVM.Captain?.AgentOrigin != null && !formationItemVM.Captain.IsDisabled)
				{
					captain = Hero.FindFirst((Func<Hero, bool>)((Hero h) => (object)h.CharacterObject == formationItemVM.Captain.AgentOrigin.Troop));
				}
			}
			DeploymentFormationClass formationClass = (DeploymentFormationClass)(isSelectable ? formationItemVM.FormationClassInt : 0);
			Dictionary<FormationFilterType, bool> filters = new Dictionary<FormationFilterType, bool>
			{
				[(FormationFilterType)1] = isSelectable && formationItemVM.HasFilter((FormationFilterType)1),
				[(FormationFilterType)3] = isSelectable && formationItemVM.HasFilter((FormationFilterType)3),
				[(FormationFilterType)4] = isSelectable && formationItemVM.HasFilter((FormationFilterType)4),
				[(FormationFilterType)5] = isSelectable && formationItemVM.HasFilter((FormationFilterType)5),
				[(FormationFilterType)6] = isSelectable && formationItemVM.HasFilter((FormationFilterType)6)
			};
			list.Add(new NavalOrderOfBattleCampaignBehavior.NavalOrderOfBattleFormationData(captain, (Ship)(object)((val is Ship) ? val : null), formationClass, filters));
		}
		_navalOrderOfBattleCampaignBehavior.SetFormationInfos(list, MobileParty.MainParty.Army != null);
	}

	private void OnClassChanged(NavalOrderOfBattleFormationItemVM formationItem)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!IsAssignmentDirty)
		{
			TroopTraitsMask filter = TroopFilteringUtilities.GetFilter(OrderOfBattleFormationExtensions.GetFormationClasses(formationItem.SelectedClass).ToArray());
			if (_navalDeploymentController.SetTroopClassFilter(filter, formationItem.Formation, !_isLoadingConfigurationAgents))
			{
				IsAssignmentDirty = true;
			}
		}
	}

	private void OnFilterUseToggled(NavalOrderOfBattleFormationItemVM formationItem)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!IsAssignmentDirty)
		{
			TroopTraitsMask filter = TroopFilteringUtilities.GetFilter((from f in (IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)formationItem.FilterItems
				where f.IsActive
				select f.FilterType).ToArray());
			if (_navalDeploymentController.SetTroopTraitsFilter(filter, formationItem.Formation, !_isLoadingConfigurationAgents))
			{
				IsAssignmentDirty = true;
			}
		}
	}

	private void OnFormationSelected(NavalOrderOfBattleFormationItemVM formation)
	{
		if (!IsAssignmentDirty)
		{
			_onFormationSelected?.Invoke(formation);
			ExecuteClearHeroAndShipSelection();
		}
	}

	private void OnSelectedFormationsChanged()
	{
		for (int i = 0; i < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; i++)
		{
			NavalOrderOfBattleFormationItemVM navalOrderOfBattleFormationItemVM = ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[i];
			navalOrderOfBattleFormationItemVM.IsSelected = _orderController.IsFormationListening(navalOrderOfBattleFormationItemVM.Formation);
		}
	}

	private void OnShipSelected(NavalOrderOfBattleShipItemVM ship, bool isSelected)
	{
		if (!IsAssignmentDirty)
		{
			if (isSelected)
			{
				SelectedShip = ship;
				SelectedHero = null;
				_clearFormationSelection?.Invoke();
			}
			else if (SelectedShip == ship)
			{
				SelectedShip = null;
			}
			else
			{
				Debug.FailedAssert("Trying to deselect ship that isn't SelectedShip!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\OrderOfBattle\\NavalOrderOfBattleVM.cs", "OnShipSelected", 793);
			}
		}
	}

	private void OnHeroSelected(NavalOrderOfBattleHeroItemVM hero, bool isSelected)
	{
		if (!IsAssignmentDirty)
		{
			if (isSelected)
			{
				SelectedHero = hero;
				SelectedShip = null;
				_clearFormationSelection?.Invoke();
			}
			else if (SelectedHero == hero)
			{
				SelectedHero = null;
			}
			else
			{
				Debug.FailedAssert("Trying to deselect hero that isn't SelectedHero!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\OrderOfBattle\\NavalOrderOfBattleVM.cs", "OnHeroSelected", 818);
			}
		}
	}

	private void OnFormationAcceptCaptain(NavalOrderOfBattleFormationItemVM formation)
	{
		if (!IsAssignmentDirty)
		{
			if (SelectedHero != null)
			{
				AssignCaptainToFormation(SelectedHero, formation);
				SelectedHero = null;
			}
			else
			{
				Debug.FailedAssert("OnFormationAcceptCaptain called without a selected hero!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\OrderOfBattle\\NavalOrderOfBattleVM.cs", "OnFormationAcceptCaptain", 836);
			}
		}
	}

	private void OnFormationAcceptShip(NavalOrderOfBattleFormationItemVM formation)
	{
		if (!IsAssignmentDirty)
		{
			if (SelectedShip != null)
			{
				AssignShipToFormation(SelectedShip, formation);
				SelectedShip = null;
			}
			else
			{
				Debug.FailedAssert("OnFormationAcceptShip called without a selected ship!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\OrderOfBattle\\NavalOrderOfBattleVM.cs", "OnFormationAcceptShip", 854);
			}
		}
	}

	public void ExecuteReturnHeroToPool()
	{
		if (IsAssignmentDirty)
		{
			return;
		}
		if (SelectedHero != null)
		{
			NavalOrderOfBattleFormationItemVM navalOrderOfBattleFormationItemVM = FindFormationOfCaptain(SelectedHero);
			if (navalOrderOfBattleFormationItemVM != null)
			{
				AssignCaptainToFormation(null, navalOrderOfBattleFormationItemVM);
			}
			SelectedHero = null;
		}
		else
		{
			Debug.FailedAssert("ExecuteReturnHeroToPool called without a selected hero!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\OrderOfBattle\\NavalOrderOfBattleVM.cs", "ExecuteReturnHeroToPool", 877);
		}
	}

	public void ExecuteReturnShipToPool()
	{
		if (IsAssignmentDirty)
		{
			return;
		}
		if (SelectedShip != null)
		{
			NavalOrderOfBattleFormationItemVM navalOrderOfBattleFormationItemVM = FindFormationOfShip(SelectedShip);
			if (navalOrderOfBattleFormationItemVM != null)
			{
				AssignShipToFormation(null, navalOrderOfBattleFormationItemVM);
			}
			SelectedShip = null;
		}
		else
		{
			Debug.FailedAssert("ExecuteReturnShipToPool called without a selected ship!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\OrderOfBattle\\NavalOrderOfBattleVM.cs", "ExecuteReturnShipToPool", 900);
		}
	}

	private void AssignCaptainToFormation(NavalOrderOfBattleHeroItemVM hero, NavalOrderOfBattleFormationItemVM formation)
	{
		if (formation == null)
		{
			Debug.FailedAssert("Trying to assign hero to null formation!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\OrderOfBattle\\NavalOrderOfBattleVM.cs", "AssignCaptainToFormation", 908);
			return;
		}
		bool flag = false;
		if (_navalDeploymentController.IsShipAssignedToFormation(formation.Formation))
		{
			flag = _navalDeploymentController.TryAssignCaptainToFormation(hero?.AgentOrigin, formation.Formation);
		}
		if (flag)
		{
			RefreshAll();
		}
	}

	private bool AssignShipToFormation(NavalOrderOfBattleShipItemVM ship, NavalOrderOfBattleFormationItemVM formation, bool isBatch = false)
	{
		if (formation == null)
		{
			Debug.FailedAssert("Trying to assign ship to null formation!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\OrderOfBattle\\NavalOrderOfBattleVM.cs", "AssignShipToFormation", 934);
			return false;
		}
		bool num = _navalDeploymentController.TryAssignShipToFormation(ship?.ShipOrigin, formation.Formation, !isBatch);
		if (num)
		{
			IsAssignmentDirty = true;
		}
		return num;
	}

	private void OnSelectionUpdated()
	{
		for (int i = 0; i < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; i++)
		{
			NavalOrderOfBattleFormationItemVM navalOrderOfBattleFormationItemVM = ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[i];
			navalOrderOfBattleFormationItemVM.IsAcceptingCaptain = HasSelectedHero && SelectedHero != navalOrderOfBattleFormationItemVM.Captain && navalOrderOfBattleFormationItemVM.GetCanAcceptCaptain() && SelectedHero.GetCanBeUnassignedOrMoved();
			navalOrderOfBattleFormationItemVM.IsAcceptingShip = HasSelectedShip && SelectedShip != navalOrderOfBattleFormationItemVM.Ship && navalOrderOfBattleFormationItemVM.GetCanAcceptShip() && SelectedShip.GetCanBeUnassignedOrMoved();
		}
		IsPoolAcceptingHero = HasSelectedHero && !((Collection<NavalOrderOfBattleHeroItemVM>)(object)UnassignedHeroes).Contains(SelectedHero) && SelectedHero.GetCanBeUnassignedOrMoved();
		IsPoolAcceptingShip = HasSelectedShip && !((Collection<NavalOrderOfBattleShipItemVM>)(object)UnassignedShips).Contains(SelectedShip) && SelectedShip.GetCanBeUnassignedOrMoved();
	}

	private void OnPlayerShipsUpdated()
	{
		RefreshAll();
		if (_finalizeInitializationOnNextUpdate)
		{
			FinalizeInitialization();
			_finalizeInitializationOnNextUpdate = false;
		}
	}

	private NavalOrderOfBattleFormationItemVM FindFormationOfCaptain(NavalOrderOfBattleHeroItemVM hero)
	{
		for (int i = 0; i < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; i++)
		{
			if (((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[i].Captain == hero)
			{
				return ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[i];
			}
		}
		return null;
	}

	private NavalOrderOfBattleFormationItemVM FindFormationOfShip(NavalOrderOfBattleShipItemVM ship)
	{
		for (int i = 0; i < ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations).Count; i++)
		{
			if (((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[i].Ship == ship)
			{
				return ((List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)[i];
			}
		}
		return null;
	}

	private int GetTroopCountWithFilter(DeploymentFormationClass orderOfBattleFormationClass, FormationFilterType filterType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected I4, but got Unknown
		int num = 0;
		List<FormationClass> formationClasses = OrderOfBattleFormationExtensions.GetFormationClasses(orderOfBattleFormationClass);
		foreach (NavalOrderOfBattleFormationItemVM item in (List<NavalOrderOfBattleFormationItemVM>)(object)AllFormations)
		{
			List<FormationClass> formationClasses2 = OrderOfBattleFormationExtensions.GetFormationClasses(item.SelectedClass);
			if (!formationClasses.Intersect(formationClasses2).Any())
			{
				continue;
			}
			switch (filterType - 1)
			{
			case 0:
				num += item.Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent a) => a.HasShieldCached));
				break;
			case 2:
				num += item.Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent a) => a.HasThrownCached));
				break;
			case 3:
				num += item.Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent a) => MissionGameModels.Current.AgentStatCalculateModel.HasHeavyArmor(a)));
				break;
			case 4:
				num += item.Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent a) => a.Character.GetBattleTier() >= 4));
				break;
			case 5:
				num += item.Formation.GetCountOfUnitsWithCondition((Func<Agent, bool>)((Agent a) => a.Character.GetBattleTier() <= 3));
				break;
			}
		}
		return num;
	}

	public void SetDoneInputKey(HotKey hotkey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}

	public void SetResetInputKey(HotKey hotkey)
	{
		ResetInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}
}
