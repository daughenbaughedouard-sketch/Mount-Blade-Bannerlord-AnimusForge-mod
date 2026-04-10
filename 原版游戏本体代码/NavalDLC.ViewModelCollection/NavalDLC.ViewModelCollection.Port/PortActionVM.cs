using System;
using NavalDLC.ViewModelCollection.Port.PortScreenHandlers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace NavalDLC.ViewModelCollection.Port;

public class PortActionVM : ViewModel
{
	private readonly Action _action;

	private bool _isVisible;

	private bool _isEnabled;

	private string _name;

	private string _additionalInfo;

	private HintViewModel _tooltip;

	[DataSourceProperty]
	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			if (value != _isVisible)
			{
				_isVisible = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsVisible");
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
	public string AdditionalInfo
	{
		get
		{
			return _additionalInfo;
		}
		set
		{
			if (value != _additionalInfo)
			{
				_additionalInfo = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AdditionalInfo");
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

	public PortActionVM(Action action)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		_action = action;
		Tooltip = new HintViewModel();
	}

	public void RefreshWith(PortActionInfo actionInfo)
	{
		IsVisible = actionInfo.IsRelevant;
		IsEnabled = actionInfo.IsEnabled;
		Name = ((object)actionInfo.ActionName)?.ToString();
		Tooltip.HintText = actionInfo.Tooltip;
	}

	public void ExecuteAction()
	{
		_action?.Invoke();
	}
}
