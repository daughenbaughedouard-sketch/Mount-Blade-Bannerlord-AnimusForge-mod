using System.Collections.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.Admin;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.AdminPanel;

public class MultiplayerAdminPanelActionOptionVM : MultiplayerAdminPanelOptionBaseVM
{
	private readonly IAdminPanelAction _action;

	private bool _isActionOption;

	[DataSourceProperty]
	public bool IsActionOption
	{
		get
		{
			return _isActionOption;
		}
		set
		{
			if (value != _isActionOption)
			{
				_isActionOption = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsActionOption");
			}
		}
	}

	public MultiplayerAdminPanelActionOptionVM(IAdminPanelAction option)
		: base(null)
	{
		_action = option;
		IsActionOption = true;
	}

	public override void RefreshValues()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		base.RefreshValues();
		base.OptionTitle = _action?.Name ?? string.Empty;
		base.OptionDescription = _action?.Description ?? string.Empty;
		if (!string.IsNullOrEmpty(_action?.Description))
		{
			base.DescriptionHint = new HintViewModel(new TextObject("{=!}" + _action.Description, (Dictionary<string, object>)null), (string)null);
		}
		else
		{
			base.DescriptionHint = null;
		}
	}

	public override void UpdateValues()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		base.UpdateValues();
		IAdminPanelAction action = _action;
		base.IsFilteredOut = action != null && !action.GetIsAvailable();
		string reason = string.Empty;
		base.IsDisabled = _action?.GetIsDisabled(out reason) ?? false;
		if (!string.IsNullOrEmpty(reason))
		{
			base.DisabledHint = new HintViewModel(new TextObject("{=!}" + reason, (Dictionary<string, object>)null), (string)null);
		}
		else
		{
			base.DisabledHint = null;
		}
	}

	public void ExecuteAction()
	{
		_action.OnActionExecuted();
	}
}
