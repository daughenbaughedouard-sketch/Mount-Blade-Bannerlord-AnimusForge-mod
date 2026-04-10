using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace SandBox.ViewModelCollection.MapSiege;

public class MapSiegeProductionMachineVM : ViewModel
{
	private Action<MapSiegeProductionMachineVM> _onSelection;

	private bool _isCancel;

	private int _machineType;

	private int _numberOfMachines;

	private string _machineID;

	private bool _isReserveOption;

	private string _actionText;

	public SiegeEngineType Engine { get; }

	[DataSourceProperty]
	public int MachineType
	{
		get
		{
			return _machineType;
		}
		set
		{
			if (value != _machineType)
			{
				_machineType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MachineType");
			}
		}
	}

	[DataSourceProperty]
	public string MachineID
	{
		get
		{
			return _machineID;
		}
		set
		{
			if (value != _machineID)
			{
				_machineID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MachineID");
			}
		}
	}

	[DataSourceProperty]
	public int NumberOfMachines
	{
		get
		{
			return _numberOfMachines;
		}
		set
		{
			if (value != _numberOfMachines)
			{
				_numberOfMachines = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NumberOfMachines");
			}
		}
	}

	[DataSourceProperty]
	public string ActionText
	{
		get
		{
			return _actionText;
		}
		set
		{
			if (value != _actionText)
			{
				_actionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ActionText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsReserveOption
	{
		get
		{
			return _isReserveOption;
		}
		set
		{
			if (value != _isReserveOption)
			{
				_isReserveOption = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsReserveOption");
			}
		}
	}

	public MapSiegeProductionMachineVM(SiegeEngineType engineType, int number, Action<MapSiegeProductionMachineVM> onSelection)
	{
		_onSelection = onSelection;
		Engine = engineType;
		NumberOfMachines = number;
		MachineID = ((MBObjectBase)engineType).StringId;
		IsReserveOption = false;
	}

	public MapSiegeProductionMachineVM(Action<MapSiegeProductionMachineVM> onSelection, bool isCancel)
	{
		_onSelection = onSelection;
		Engine = null;
		NumberOfMachines = 0;
		MachineID = "reserve";
		IsReserveOption = true;
		_isCancel = isCancel;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		ActionText = (_isCancel ? ((object)GameTexts.FindText("str_cancel", (string)null)).ToString() : ((object)GameTexts.FindText("str_siege_move_to_reserve", (string)null)).ToString());
	}

	public void OnSelection()
	{
		_onSelection(this);
	}

	public void ExecuteShowTooltip()
	{
		if (Engine != null)
		{
			InformationManager.ShowTooltip(typeof(List<TooltipProperty>), new object[1] { SandBoxUIHelper.GetSiegeEngineTooltip(Engine) });
		}
	}

	public void ExecuteHideTooltip()
	{
		MBInformationManager.HideInformations();
	}
}
