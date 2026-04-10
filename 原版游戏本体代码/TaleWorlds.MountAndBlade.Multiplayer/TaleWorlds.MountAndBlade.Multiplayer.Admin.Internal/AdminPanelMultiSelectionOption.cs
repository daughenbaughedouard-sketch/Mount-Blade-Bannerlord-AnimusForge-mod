using System.Collections.Generic;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.Admin.Internal;

internal class AdminPanelMultiSelectionOption : AdminPanelOption<IAdminPanelMultiSelectionItem>, IAdminPanelMultiSelectionOption, IAdminPanelOption<IAdminPanelMultiSelectionItem>, IAdminPanelOption
{
	protected IAdminPanelMultiSelectionItem _selectedOption;

	protected MBList<IAdminPanelMultiSelectionItem> _availableOptions;

	public AdminPanelMultiSelectionOption(string uniqueId)
		: base(uniqueId)
	{
		_availableOptions = new MBList<IAdminPanelMultiSelectionItem>();
	}

	protected override bool AreEqualValues(IAdminPanelMultiSelectionItem first, IAdminPanelMultiSelectionItem second)
	{
		return first == second;
	}

	protected override IAdminPanelMultiSelectionItem GetOptionValue(OptionType optionType, MultiplayerOptionsAccessMode accessMode = (MultiplayerOptionsAccessMode)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		string strValue = MultiplayerOptionsExtensions.GetStrValue(optionType, accessMode);
		for (int i = 0; i < ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Count; i++)
		{
			if (((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[i].Value == strValue)
			{
				return ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[i];
			}
		}
		return null;
	}

	protected override void OnValueChanged(IAdminPanelMultiSelectionItem previousValue, IAdminPanelMultiSelectionItem newValue)
	{
		_selectedOption = newValue;
		base.OnValueChanged(previousValue, newValue);
	}

	protected override bool OnGetCanRevertToDefaultValue()
	{
		return ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Contains(base.DefaultValue);
	}

	public virtual AdminPanelMultiSelectionOption BuildAvailableOptions(MBReadOnlyList<IAdminPanelMultiSelectionItem> options)
	{
		((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Clear();
		if (options != null && ((List<IAdminPanelMultiSelectionItem>)(object)options).Count > 0)
		{
			for (int i = 0; i < ((List<IAdminPanelMultiSelectionItem>)(object)options).Count; i++)
			{
				((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Add(((List<IAdminPanelMultiSelectionItem>)(object)options)[i]);
			}
		}
		OnRefresh();
		return this;
	}

	public virtual AdminPanelMultiSelectionOption BuildAvailableOptions(OptionType optionType, bool buildDefaultValue = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Clear();
		string strValue = MultiplayerOptionsExtensions.GetStrValue(optionType, (MultiplayerOptionsAccessMode)0);
		List<string> multiplayerOptionsList = MultiplayerOptions.Instance.GetMultiplayerOptionsList(optionType);
		if (multiplayerOptionsList != null && multiplayerOptionsList.Count > 0)
		{
			for (int i = 0; i < multiplayerOptionsList.Count; i++)
			{
				AdminPanelMultiSelectionItem adminPanelMultiSelectionItem = new AdminPanelMultiSelectionItem(multiplayerOptionsList[i], null);
				((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Add((IAdminPanelMultiSelectionItem)adminPanelMultiSelectionItem);
				if (buildDefaultValue && adminPanelMultiSelectionItem.Value == strValue)
				{
					BuildDefaultValue(adminPanelMultiSelectionItem);
					BuildInitialValue(adminPanelMultiSelectionItem);
				}
			}
		}
		OnRefresh();
		return this;
	}

	public MBReadOnlyList<IAdminPanelMultiSelectionItem> GetAvailableOptions()
	{
		return (MBReadOnlyList<IAdminPanelMultiSelectionItem>)(object)_availableOptions;
	}
}
