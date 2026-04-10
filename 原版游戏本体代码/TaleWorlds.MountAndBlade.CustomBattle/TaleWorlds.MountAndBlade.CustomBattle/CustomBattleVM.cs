using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattle.SelectionItem;
using TaleWorlds.MountAndBlade.View.CustomBattle;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.CustomBattle;

public class CustomBattleVM : ViewModel
{
	private readonly ICustomBattleProvider _nextCustomBattleProvider;

	private CustomBattleState _customBattleState;

	private TroopTypeSelectionPopUpVM _troopTypeSelectionPopUp;

	private CustomBattleSideVM _enemySide;

	private CustomBattleSideVM _playerSide;

	private bool _isAttackerCustomMachineSelectionEnabled;

	private bool _isDefenderCustomMachineSelectionEnabled;

	private GameTypeSelectionGroupVM _gameTypeSelectionGroup;

	private MapSelectionGroupVM _mapSelectionGroup;

	private string _randomizeButtonText;

	private string _backButtonText;

	private string _startButtonText;

	private string _titleText;

	private string _switchButtonText;

	private bool _CanSwitchMode;

	private HintViewModel _switchHint;

	private MBBindingList<CustomBattleSiegeMachineVM> _attackerMeleeMachines;

	private MBBindingList<CustomBattleSiegeMachineVM> _attackerRangedMachines;

	private MBBindingList<CustomBattleSiegeMachineVM> _defenderMachines;

	private InputKeyItemVM _startInputKey;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _resetInputKey;

	private InputKeyItemVM _randomizeInputKey;

	[DataSourceProperty]
	public TroopTypeSelectionPopUpVM TroopTypeSelectionPopUp
	{
		get
		{
			return _troopTypeSelectionPopUp;
		}
		set
		{
			if (value != _troopTypeSelectionPopUp)
			{
				_troopTypeSelectionPopUp = value;
				((ViewModel)this).OnPropertyChangedWithValue<TroopTypeSelectionPopUpVM>(value, "TroopTypeSelectionPopUp");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAttackerCustomMachineSelectionEnabled
	{
		get
		{
			return _isAttackerCustomMachineSelectionEnabled;
		}
		set
		{
			if (value != _isAttackerCustomMachineSelectionEnabled)
			{
				_isAttackerCustomMachineSelectionEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAttackerCustomMachineSelectionEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDefenderCustomMachineSelectionEnabled
	{
		get
		{
			return _isDefenderCustomMachineSelectionEnabled;
		}
		set
		{
			if (value != _isDefenderCustomMachineSelectionEnabled)
			{
				_isDefenderCustomMachineSelectionEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDefenderCustomMachineSelectionEnabled");
			}
		}
	}

	[DataSourceProperty]
	public string RandomizeButtonText
	{
		get
		{
			return _randomizeButtonText;
		}
		set
		{
			if (value != _randomizeButtonText)
			{
				_randomizeButtonText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RandomizeButtonText");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string BackButtonText
	{
		get
		{
			return _backButtonText;
		}
		set
		{
			if (value != _backButtonText)
			{
				_backButtonText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BackButtonText");
			}
		}
	}

	[DataSourceProperty]
	public string StartButtonText
	{
		get
		{
			return _startButtonText;
		}
		set
		{
			if (value != _startButtonText)
			{
				_startButtonText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "StartButtonText");
			}
		}
	}

	[DataSourceProperty]
	public string SwitchButtonText
	{
		get
		{
			return _switchButtonText;
		}
		set
		{
			if (value != _switchButtonText)
			{
				_switchButtonText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SwitchButtonText");
			}
		}
	}

	[DataSourceProperty]
	public CustomBattleSideVM EnemySide
	{
		get
		{
			return _enemySide;
		}
		set
		{
			if (value != _enemySide)
			{
				_enemySide = value;
				((ViewModel)this).OnPropertyChangedWithValue<CustomBattleSideVM>(value, "EnemySide");
			}
		}
	}

	[DataSourceProperty]
	public CustomBattleSideVM PlayerSide
	{
		get
		{
			return _playerSide;
		}
		set
		{
			if (value != _playerSide)
			{
				_playerSide = value;
				((ViewModel)this).OnPropertyChangedWithValue<CustomBattleSideVM>(value, "PlayerSide");
			}
		}
	}

	[DataSourceProperty]
	public GameTypeSelectionGroupVM GameTypeSelectionGroup
	{
		get
		{
			return _gameTypeSelectionGroup;
		}
		set
		{
			if (value != _gameTypeSelectionGroup)
			{
				_gameTypeSelectionGroup = value;
				((ViewModel)this).OnPropertyChangedWithValue<GameTypeSelectionGroupVM>(value, "GameTypeSelectionGroup");
			}
		}
	}

	[DataSourceProperty]
	public MapSelectionGroupVM MapSelectionGroup
	{
		get
		{
			return _mapSelectionGroup;
		}
		set
		{
			if (value != _mapSelectionGroup)
			{
				_mapSelectionGroup = value;
				((ViewModel)this).OnPropertyChangedWithValue<MapSelectionGroupVM>(value, "MapSelectionGroup");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CustomBattleSiegeMachineVM> AttackerMeleeMachines
	{
		get
		{
			return _attackerMeleeMachines;
		}
		set
		{
			if (value != _attackerMeleeMachines)
			{
				_attackerMeleeMachines = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CustomBattleSiegeMachineVM>>(value, "AttackerMeleeMachines");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CustomBattleSiegeMachineVM> AttackerRangedMachines
	{
		get
		{
			return _attackerRangedMachines;
		}
		set
		{
			if (value != _attackerRangedMachines)
			{
				_attackerRangedMachines = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CustomBattleSiegeMachineVM>>(value, "AttackerRangedMachines");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CustomBattleSiegeMachineVM> DefenderMachines
	{
		get
		{
			return _defenderMachines;
		}
		set
		{
			if (value != _defenderMachines)
			{
				_defenderMachines = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CustomBattleSiegeMachineVM>>(value, "DefenderMachines");
			}
		}
	}

	[DataSourceProperty]
	public bool CanSwitchMode
	{
		get
		{
			return _CanSwitchMode;
		}
		set
		{
			if (value != _CanSwitchMode)
			{
				_CanSwitchMode = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanSwitchMode");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel SwitchHint
	{
		get
		{
			return _switchHint;
		}
		set
		{
			if (value != _switchHint)
			{
				_switchHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "SwitchHint");
			}
		}
	}

	public InputKeyItemVM StartInputKey
	{
		get
		{
			return _startInputKey;
		}
		set
		{
			if (value != _startInputKey)
			{
				_startInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "StartInputKey");
			}
		}
	}

	public InputKeyItemVM CancelInputKey
	{
		get
		{
			return _cancelInputKey;
		}
		set
		{
			if (value != _cancelInputKey)
			{
				_cancelInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "CancelInputKey");
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

	public InputKeyItemVM RandomizeInputKey
	{
		get
		{
			return _randomizeInputKey;
		}
		set
		{
			if (value != _randomizeInputKey)
			{
				_randomizeInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "RandomizeInputKey");
			}
		}
	}

	private static CustomBattleCompositionData GetBattleCompositionDataFromCompositionGroup(ArmyCompositionGroupVM compositionGroup)
	{
		return new CustomBattleCompositionData((float)compositionGroup.RangedInfantryComposition.CompositionValue / 100f, (float)compositionGroup.MeleeCavalryComposition.CompositionValue / 100f, (float)compositionGroup.RangedCavalryComposition.CompositionValue / 100f);
	}

	private static List<BasicCharacterObject>[] GetTroopSelections(ArmyCompositionGroupVM armyComposition)
	{
		return new List<BasicCharacterObject>[4]
		{
			(from x in (IEnumerable<CustomBattleTroopTypeVM>)armyComposition.MeleeInfantryComposition.TroopTypes
				where x.IsSelected
				select x.Character).ToList(),
			(from x in (IEnumerable<CustomBattleTroopTypeVM>)armyComposition.RangedInfantryComposition.TroopTypes
				where x.IsSelected
				select x.Character).ToList(),
			(from x in (IEnumerable<CustomBattleTroopTypeVM>)armyComposition.MeleeCavalryComposition.TroopTypes
				where x.IsSelected
				select x.Character).ToList(),
			(from x in (IEnumerable<CustomBattleTroopTypeVM>)armyComposition.RangedCavalryComposition.TroopTypes
				where x.IsSelected
				select x.Character).ToList()
		};
	}

	private static void FillSiegeMachines(List<MissionSiegeWeapon> machines, MBBindingList<CustomBattleSiegeMachineVM> vmMachines)
	{
		foreach (CustomBattleSiegeMachineVM item in (Collection<CustomBattleSiegeMachineVM>)(object)vmMachines)
		{
			if (item.SiegeEngineType != null)
			{
				machines.Add(MissionSiegeWeapon.CreateDefaultWeapon(item.SiegeEngineType));
			}
		}
	}

	public CustomBattleVM(CustomBattleState battleState)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Expected O, but got Unknown
		_customBattleState = battleState;
		IsAttackerCustomMachineSelectionEnabled = false;
		TroopTypeSelectionPopUp = new TroopTypeSelectionPopUpVM();
		PlayerSide = new CustomBattleSideVM(new TextObject("{=BC7n6qxk}PLAYER", (Dictionary<string, object>)null), isPlayerSide: true, TroopTypeSelectionPopUp, OnSelectedCharactersChanged);
		EnemySide = new CustomBattleSideVM(new TextObject("{=35IHscBa}ENEMY", (Dictionary<string, object>)null), isPlayerSide: false, TroopTypeSelectionPopUp, OnSelectedCharactersChanged);
		OnSelectedCharactersChanged();
		MapSelectionGroup = new MapSelectionGroupVM();
		GameTypeSelectionGroup = new GameTypeSelectionGroupVM(OnPlayerTypeChange, OnGameTypeChange);
		AttackerMeleeMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
		for (int i = 0; i < 3; i++)
		{
			((Collection<CustomBattleSiegeMachineVM>)(object)AttackerMeleeMachines).Add(new CustomBattleSiegeMachineVM(null, OnMeleeMachineSelection, OnResetMachineSelection));
		}
		AttackerRangedMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
		for (int j = 0; j < 4; j++)
		{
			((Collection<CustomBattleSiegeMachineVM>)(object)AttackerRangedMachines).Add(new CustomBattleSiegeMachineVM(null, OnAttackerRangedMachineSelection, OnResetMachineSelection));
		}
		DefenderMachines = new MBBindingList<CustomBattleSiegeMachineVM>();
		for (int k = 0; k < 4; k++)
		{
			((Collection<CustomBattleSiegeMachineVM>)(object)DefenderMachines).Add(new CustomBattleSiegeMachineVM(null, OnDefenderRangedMachineSelection, OnResetMachineSelection));
		}
		CanSwitchMode = CustomBattleFactory.GetProviderCount() > 1;
		if (CanSwitchMode)
		{
			_nextCustomBattleProvider = CustomBattleFactory.CollectNextProvider(typeof(CustomBattleProvider));
			SwitchHint = new HintViewModel(new TextObject("{=Jfe53wbr}Switch to {PROVIDER_NAME}", (Dictionary<string, object>)null).SetTextVariable("PROVIDER_NAME", _nextCustomBattleProvider.GetName()), (string)null);
		}
		((ViewModel)this).RefreshValues();
		SetDefaultSiegeMachines();
	}

	private void SetDefaultSiegeMachines()
	{
		((Collection<CustomBattleSiegeMachineVM>)(object)AttackerMeleeMachines)[0].SetMachineType(DefaultSiegeEngineTypes.SiegeTower);
		((Collection<CustomBattleSiegeMachineVM>)(object)AttackerMeleeMachines)[1].SetMachineType(DefaultSiegeEngineTypes.Ram);
		((Collection<CustomBattleSiegeMachineVM>)(object)AttackerMeleeMachines)[2].SetMachineType(DefaultSiegeEngineTypes.SiegeTower);
		((Collection<CustomBattleSiegeMachineVM>)(object)AttackerRangedMachines)[0].SetMachineType(DefaultSiegeEngineTypes.Trebuchet);
		((Collection<CustomBattleSiegeMachineVM>)(object)AttackerRangedMachines)[1].SetMachineType(DefaultSiegeEngineTypes.Onager);
		((Collection<CustomBattleSiegeMachineVM>)(object)AttackerRangedMachines)[2].SetMachineType(DefaultSiegeEngineTypes.Onager);
		((Collection<CustomBattleSiegeMachineVM>)(object)AttackerRangedMachines)[3].SetMachineType(DefaultSiegeEngineTypes.FireBallista);
		((Collection<CustomBattleSiegeMachineVM>)(object)DefenderMachines)[0].SetMachineType(DefaultSiegeEngineTypes.FireCatapult);
		((Collection<CustomBattleSiegeMachineVM>)(object)DefenderMachines)[1].SetMachineType(DefaultSiegeEngineTypes.FireCatapult);
		((Collection<CustomBattleSiegeMachineVM>)(object)DefenderMachines)[2].SetMachineType(DefaultSiegeEngineTypes.Catapult);
		((Collection<CustomBattleSiegeMachineVM>)(object)DefenderMachines)[3].SetMachineType(DefaultSiegeEngineTypes.FireBallista);
	}

	public void SetActiveState(bool isActive)
	{
		if (isActive)
		{
			EnemySide.UpdateCharacterVisual();
			PlayerSide.UpdateCharacterVisual();
		}
		else
		{
			EnemySide.CurrentSelectedCharacter = null;
			PlayerSide.CurrentSelectedCharacter = null;
		}
	}

	private void OnSelectedCharactersChanged()
	{
		if (PlayerSide?.CharacterSelectionGroup == null || EnemySide?.CharacterSelectionGroup == null)
		{
			return;
		}
		BasicCharacterObject val = PlayerSide.CharacterSelectionGroup.SelectedItem?.Character;
		BasicCharacterObject val2 = EnemySide.CharacterSelectionGroup.SelectedItem?.Character;
		foreach (CharacterItemVM item in (Collection<CharacterItemVM>)(object)PlayerSide.CharacterSelectionGroup.ItemList)
		{
			((SelectorItemVM)item).CanBeSelected = item.Character != val2;
		}
		foreach (CharacterItemVM item2 in (Collection<CharacterItemVM>)(object)EnemySide.CharacterSelectionGroup.ItemList)
		{
			((SelectorItemVM)item2).CanBeSelected = item2.Character != val;
		}
	}

	private void OnPlayerTypeChange(CustomBattlePlayerType playerType)
	{
		PlayerSide.OnPlayerTypeChange(playerType);
	}

	private void OnGameTypeChange(string gameTypeStringId)
	{
		MapSelectionGroup.OnGameTypeChange(gameTypeStringId);
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		RandomizeButtonText = ((object)GameTexts.FindText("str_randomize", (string)null)).ToString();
		StartButtonText = ((object)GameTexts.FindText("str_start", (string)null)).ToString();
		BackButtonText = ((object)GameTexts.FindText("str_back", (string)null)).ToString();
		SwitchButtonText = ((object)GameTexts.FindText("str_switch", (string)null)).ToString();
		TitleText = ((object)GameTexts.FindText("str_custom_battle", (string)null)).ToString();
		((ViewModel)EnemySide).RefreshValues();
		((ViewModel)PlayerSide).RefreshValues();
		AttackerMeleeMachines.ApplyActionOnAllItems((Action<CustomBattleSiegeMachineVM>)delegate(CustomBattleSiegeMachineVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		AttackerRangedMachines.ApplyActionOnAllItems((Action<CustomBattleSiegeMachineVM>)delegate(CustomBattleSiegeMachineVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		DefenderMachines.ApplyActionOnAllItems((Action<CustomBattleSiegeMachineVM>)delegate(CustomBattleSiegeMachineVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		((ViewModel)MapSelectionGroup).RefreshValues();
		TroopTypeSelectionPopUpVM troopTypeSelectionPopUp = TroopTypeSelectionPopUp;
		if (troopTypeSelectionPopUp != null)
		{
			((ViewModel)troopTypeSelectionPopUp).RefreshValues();
		}
	}

	private void OnResetMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
	{
		selectedSlot.SetMachineType(null);
	}

	private void OnMeleeMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		List<InquiryElement> list = new List<InquiryElement>();
		list.Add(new InquiryElement((object)null, ((object)GameTexts.FindText("str_empty", (string)null)).ToString(), (ImageIdentifier)null));
		foreach (SiegeEngineType allAttackerMeleeMachine in CustomBattleData.GetAllAttackerMeleeMachines())
		{
			list.Add(new InquiryElement((object)allAttackerMeleeMachine, ((object)allAttackerMeleeMachine.Name).ToString(), (ImageIdentifier)null));
		}
		MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(((object)new TextObject("{=MVOWsP48}Select a Melee Machine", (Dictionary<string, object>)null)).ToString(), string.Empty, list, true, 1, 1, ((object)GameTexts.FindText("str_done", (string)null)).ToString(), "", (Action<List<InquiryElement>>)delegate(List<InquiryElement> selectedElements)
		{
			CustomBattleSiegeMachineVM customBattleSiegeMachineVM = selectedSlot;
			object obj = selectedElements.FirstOrDefault()?.Identifier;
			customBattleSiegeMachineVM.SetMachineType((SiegeEngineType)((obj is SiegeEngineType) ? obj : null));
		}, (Action<List<InquiryElement>>)null, "", false), false, false);
	}

	private void OnAttackerRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		List<InquiryElement> list = new List<InquiryElement>();
		list.Add(new InquiryElement((object)null, ((object)GameTexts.FindText("str_empty", (string)null)).ToString(), (ImageIdentifier)null));
		foreach (SiegeEngineType allAttackerRangedMachine in CustomBattleData.GetAllAttackerRangedMachines())
		{
			list.Add(new InquiryElement((object)allAttackerRangedMachine, ((object)allAttackerRangedMachine.Name).ToString(), (ImageIdentifier)null));
		}
		MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(((object)new TextObject("{=SLZzfNPr}Select a Ranged Machine", (Dictionary<string, object>)null)).ToString(), string.Empty, list, true, 1, 1, ((object)GameTexts.FindText("str_done", (string)null)).ToString(), "", (Action<List<InquiryElement>>)delegate(List<InquiryElement> selectedElements)
		{
			CustomBattleSiegeMachineVM customBattleSiegeMachineVM = selectedSlot;
			object obj = selectedElements.FirstOrDefault()?.Identifier;
			customBattleSiegeMachineVM.SetMachineType((SiegeEngineType)((obj is SiegeEngineType) ? obj : null));
		}, (Action<List<InquiryElement>>)null, "", false), false, false);
	}

	private void OnDefenderRangedMachineSelection(CustomBattleSiegeMachineVM selectedSlot)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		List<InquiryElement> list = new List<InquiryElement>();
		list.Add(new InquiryElement((object)null, ((object)GameTexts.FindText("str_empty", (string)null)).ToString(), (ImageIdentifier)null));
		foreach (SiegeEngineType allDefenderRangedMachine in CustomBattleData.GetAllDefenderRangedMachines())
		{
			list.Add(new InquiryElement((object)allDefenderRangedMachine, ((object)allDefenderRangedMachine.Name).ToString(), (ImageIdentifier)null));
		}
		MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(((object)new TextObject("{=SLZzfNPr}Select a Ranged Machine", (Dictionary<string, object>)null)).ToString(), string.Empty, list, true, 1, 1, ((object)GameTexts.FindText("str_done", (string)null)).ToString(), "", (Action<List<InquiryElement>>)delegate(List<InquiryElement> selectedElements)
		{
			CustomBattleSiegeMachineVM customBattleSiegeMachineVM = selectedSlot;
			object obj = selectedElements.FirstOrDefault()?.Identifier;
			customBattleSiegeMachineVM.SetMachineType((SiegeEngineType)((obj is SiegeEngineType) ? obj : null));
		}, (Action<List<InquiryElement>>)null, "", false), false, false);
	}

	private void ExecuteRandomizeAttackerSiegeEngines()
	{
		MBList<SiegeEngineType> val = new MBList<SiegeEngineType>();
		((List<SiegeEngineType>)(object)val).AddRange(CustomBattleData.GetAllAttackerMeleeMachines());
		((List<SiegeEngineType>)(object)val).Add((SiegeEngineType)null);
		foreach (CustomBattleSiegeMachineVM item in (Collection<CustomBattleSiegeMachineVM>)(object)_attackerMeleeMachines)
		{
			item.SetMachineType(Extensions.GetRandomElement<SiegeEngineType>(val));
		}
		((List<SiegeEngineType>)(object)val).Clear();
		((List<SiegeEngineType>)(object)val).AddRange(CustomBattleData.GetAllAttackerRangedMachines());
		((List<SiegeEngineType>)(object)val).Add((SiegeEngineType)null);
		foreach (CustomBattleSiegeMachineVM item2 in (Collection<CustomBattleSiegeMachineVM>)(object)_attackerRangedMachines)
		{
			item2.SetMachineType(Extensions.GetRandomElement<SiegeEngineType>(val));
		}
	}

	private void ExecuteRandomizeDefenderSiegeEngines()
	{
		MBList<SiegeEngineType> val = new MBList<SiegeEngineType>();
		((List<SiegeEngineType>)(object)val).AddRange(CustomBattleData.GetAllDefenderRangedMachines());
		((List<SiegeEngineType>)(object)val).Add((SiegeEngineType)null);
		foreach (CustomBattleSiegeMachineVM item in (Collection<CustomBattleSiegeMachineVM>)(object)_defenderMachines)
		{
			item.SetMachineType(Extensions.GetRandomElement<SiegeEngineType>(val));
		}
	}

	public void ExecuteBack()
	{
		Debug.Print("EXECUTE BACK - PRESSED", 0, (DebugColor)4, 17592186044416uL);
		Game.Current.GameStateManager.PopState(0);
	}

	private CustomBattleData PrepareBattleData()
	{
		BasicCharacterObject selectedCharacter = PlayerSide.SelectedCharacter;
		BasicCharacterObject selectedCharacter2 = EnemySide.SelectedCharacter;
		int num = PlayerSide.CompositionGroup.ArmySize;
		int armySize = EnemySide.CompositionGroup.ArmySize;
		bool isPlayerAttacker = GameTypeSelectionGroup.SelectedPlayerSide == CustomBattlePlayerSide.Attacker;
		bool num2 = GameTypeSelectionGroup.SelectedPlayerType == CustomBattlePlayerType.Commander;
		BasicCharacterObject playerSideGeneralCharacter = null;
		if (!num2)
		{
			MBList<BasicCharacterObject> obj = Extensions.ToMBList<BasicCharacterObject>(CustomBattleData.Characters);
			((List<BasicCharacterObject>)(object)obj).Remove(selectedCharacter);
			((List<BasicCharacterObject>)(object)obj).Remove(selectedCharacter2);
			playerSideGeneralCharacter = Extensions.GetRandomElement<BasicCharacterObject>(obj);
			num--;
		}
		int[] troopCounts = CustomBattleHelper.GetTroopCounts(num, GetBattleCompositionDataFromCompositionGroup(PlayerSide.CompositionGroup));
		int[] troopCounts2 = CustomBattleHelper.GetTroopCounts(armySize, GetBattleCompositionDataFromCompositionGroup(EnemySide.CompositionGroup));
		List<BasicCharacterObject>[] troopSelections = GetTroopSelections(PlayerSide.CompositionGroup);
		List<BasicCharacterObject>[] troopSelections2 = GetTroopSelections(EnemySide.CompositionGroup);
		BasicCultureObject faction = PlayerSide.FactionSelectionGroup.SelectedItem.Faction;
		BasicCultureObject faction2 = EnemySide.FactionSelectionGroup.SelectedItem.Faction;
		CustomBattleCombatant[] customBattleParties = CustomBattleHelper.GetCustomBattleParties(selectedCharacter, playerSideGeneralCharacter, selectedCharacter2, faction, troopCounts, troopSelections, faction2, troopCounts2, troopSelections2, isPlayerAttacker);
		List<MissionSiegeWeapon> list = null;
		List<MissionSiegeWeapon> list2 = null;
		float[] wallHitPointsPercentages = null;
		if (GameTypeSelectionGroup.SelectedGameTypeString == "Siege")
		{
			list = new List<MissionSiegeWeapon>();
			list2 = new List<MissionSiegeWeapon>();
			FillSiegeMachines(list, _attackerMeleeMachines);
			FillSiegeMachines(list, _attackerRangedMachines);
			FillSiegeMachines(list2, _defenderMachines);
			wallHitPointsPercentages = CustomBattleHelper.GetWallHitpointPercentages(MapSelectionGroup.SelectedWallBreachedCount);
		}
		return CustomBattleHelper.PrepareBattleData(selectedCharacter, playerSideGeneralCharacter, customBattleParties[0], customBattleParties[1], GameTypeSelectionGroup.SelectedPlayerSide, GameTypeSelectionGroup.SelectedPlayerType, GameTypeSelectionGroup.SelectedGameTypeString, MapSelectionGroup.SelectedMap?.MapId, MapSelectionGroup.SelectedSeasonId, MapSelectionGroup.SelectedTimeOfDay, list, list2, wallHitPointsPercentages, MapSelectionGroup.SelectedSceneLevel, MapSelectionGroup.IsSallyOutSelected);
	}

	public void ExecuteStart()
	{
		CustomBattleHelper.StartGame(PrepareBattleData());
		Debug.Print("EXECUTE START - PRESSED", 0, (DebugColor)4, 17592186044416uL);
	}

	public void ExecuteRandomize()
	{
		GameTypeSelectionGroup.RandomizeAll();
		MapSelectionGroup.RandomizeAll();
		PlayerSide.Randomize();
		EnemySide.Randomize(PlayerSide);
		ExecuteRandomizeAttackerSiegeEngines();
		ExecuteRandomizeDefenderSiegeEngines();
		Debug.Print("EXECUTE RANDOMIZE - PRESSED", 0, (DebugColor)4, 17592186044416uL);
	}

	private void ExecuteDoneDefenderCustomMachineSelection()
	{
		IsDefenderCustomMachineSelectionEnabled = false;
	}

	private void ExecuteDoneAttackerCustomMachineSelection()
	{
		IsAttackerCustomMachineSelectionEnabled = false;
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		((ViewModel)StartInputKey).OnFinalize();
		((ViewModel)CancelInputKey).OnFinalize();
		((ViewModel)ResetInputKey).OnFinalize();
		((ViewModel)RandomizeInputKey).OnFinalize();
		TroopTypeSelectionPopUpVM troopTypeSelectionPopUp = TroopTypeSelectionPopUp;
		if (troopTypeSelectionPopUp != null)
		{
			((ViewModel)troopTypeSelectionPopUp).OnFinalize();
		}
	}

	public void ExecuteSwitchToNextCustomBattle()
	{
		if (CanSwitchMode)
		{
			ExecuteBack();
			GameStateManager.Current = Module.CurrentModule.GlobalGameStateManager;
			_nextCustomBattleProvider.StartCustomBattle();
		}
	}

	public void SetStartInputKey(HotKey hotkey)
	{
		StartInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}

	public void SetCancelInputKey(HotKey hotkey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
		TroopTypeSelectionPopUp?.SetCancelInputKey(hotkey);
	}

	public void SetResetInputKey(HotKey hotkey)
	{
		ResetInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
		TroopTypeSelectionPopUp?.SetResetInputKey(hotkey);
	}

	public void SetRandomizeInputKey(HotKey hotkey)
	{
		RandomizeInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, true);
	}
}
