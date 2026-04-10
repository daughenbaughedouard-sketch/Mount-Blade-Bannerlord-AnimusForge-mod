using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.Admin;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.AdminPanel;

public class MultiplayerAdminPanelOptionGroupVM : ViewModel
{
	private readonly IAdminPanelOptionGroup _optionGroup;

	private readonly Func<IAdminPanelOption, MultiplayerAdminPanelOptionBaseVM> _onCreateOptionVM;

	private readonly Func<IAdminPanelAction, MultiplayerAdminPanelOptionBaseVM> _onCreateActionVM;

	private bool _requiresRestart;

	private string _groupName;

	private HintViewModel _requiresRestartHint;

	private MBBindingList<MultiplayerAdminPanelOptionBaseVM> _options;

	[DataSourceProperty]
	public bool RequiresRestart
	{
		get
		{
			return _requiresRestart;
		}
		set
		{
			if (value != _requiresRestart)
			{
				_requiresRestart = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "RequiresRestart");
			}
		}
	}

	[DataSourceProperty]
	public string GroupName
	{
		get
		{
			return _groupName;
		}
		set
		{
			if (value != _groupName)
			{
				_groupName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GroupName");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel RequiresRestartHint
	{
		get
		{
			return _requiresRestartHint;
		}
		set
		{
			if (value != _requiresRestartHint)
			{
				_requiresRestartHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "RequiresRestartHint");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MultiplayerAdminPanelOptionBaseVM> Options
	{
		get
		{
			return _options;
		}
		set
		{
			if (value != _options)
			{
				_options = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MultiplayerAdminPanelOptionBaseVM>>(value, "Options");
			}
		}
	}

	public MultiplayerAdminPanelOptionGroupVM(IAdminPanelOptionGroup optionGroup, Func<IAdminPanelOption, MultiplayerAdminPanelOptionBaseVM> onCreateOptionVm, Func<IAdminPanelAction, MultiplayerAdminPanelOptionBaseVM> onCreateActionVm)
	{
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		_optionGroup = optionGroup;
		_onCreateOptionVM = onCreateOptionVm;
		_onCreateActionVM = onCreateActionVm;
		Options = new MBBindingList<MultiplayerAdminPanelOptionBaseVM>();
		for (int i = 0; i < ((List<IAdminPanelOption>)(object)_optionGroup.Options).Count; i++)
		{
			MultiplayerAdminPanelOptionBaseVM multiplayerAdminPanelOptionBaseVM = _onCreateOptionVM?.Invoke(((List<IAdminPanelOption>)(object)optionGroup.Options)[i]);
			if (multiplayerAdminPanelOptionBaseVM != null)
			{
				((Collection<MultiplayerAdminPanelOptionBaseVM>)(object)Options).Add(multiplayerAdminPanelOptionBaseVM);
			}
			else
			{
				Debug.FailedAssert("Failed to create view model for option type: " + ((List<IAdminPanelOption>)(object)optionGroup.Options)[i].GetType().Name, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\AdminPanel\\MultiplayerAdminPanelOptionGroupVM.cs", ".ctor", 34);
			}
		}
		for (int j = 0; j < ((List<IAdminPanelAction>)(object)_optionGroup.Actions).Count; j++)
		{
			MultiplayerAdminPanelOptionBaseVM multiplayerAdminPanelOptionBaseVM2 = _onCreateActionVM?.Invoke(((List<IAdminPanelAction>)(object)optionGroup.Actions)[j]);
			if (multiplayerAdminPanelOptionBaseVM2 != null)
			{
				((Collection<MultiplayerAdminPanelOptionBaseVM>)(object)Options).Add(multiplayerAdminPanelOptionBaseVM2);
			}
			else
			{
				Debug.FailedAssert("Failed to create view model for option type: " + ((List<IAdminPanelOption>)(object)optionGroup.Options)[j].GetType().Name, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\AdminPanel\\MultiplayerAdminPanelOptionGroupVM.cs", ".ctor", 48);
			}
		}
		RequiresRestart = _optionGroup.RequiresRestart;
		RequiresRestartHint = new HintViewModel();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		((ViewModel)this).RefreshValues();
		RequiresRestartHint.HintText = (TextObject)((!RequiresRestart) ? ((object)TextObject.GetEmpty()) : ((object)new TextObject("{=sTVcpXkf}All options under this category requires restart.", (Dictionary<string, object>)null)));
		GroupName = ((object)_optionGroup.Name).ToString();
		Options.ApplyActionOnAllItems((Action<MultiplayerAdminPanelOptionBaseVM>)delegate(MultiplayerAdminPanelOptionBaseVM o)
		{
			((ViewModel)o).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		Options.ApplyActionOnAllItems((Action<MultiplayerAdminPanelOptionBaseVM>)delegate(MultiplayerAdminPanelOptionBaseVM o)
		{
			((ViewModel)o).OnFinalize();
		});
	}
}
