using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame.HostGameOptions;

public class MultipleSelectionHostGameOptionDataVM : GenericHostGameOptionDataVM
{
	public Action<MultipleSelectionHostGameOptionDataVM> OnChangedSelection;

	private SelectorVM<SelectorItemVM> _selector;

	[DataSourceProperty]
	public SelectorVM<SelectorItemVM> Selector
	{
		get
		{
			return _selector;
		}
		set
		{
			if (value != _selector)
			{
				_selector = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<SelectorItemVM>>(value, "Selector");
			}
		}
	}

	public MultipleSelectionHostGameOptionDataVM(OptionType optionType, int preferredIndex)
		: base((OptionsDataType)3, optionType, preferredIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		List<string> multiplayerOptionsList = MultiplayerOptions.Instance.GetMultiplayerOptionsList(base.OptionType);
		List<string> multiplayerOptionsTextList = MultiplayerOptions.Instance.GetMultiplayerOptionsTextList(base.OptionType);
		List<string> list = new List<string>();
		foreach (string item in multiplayerOptionsTextList)
		{
			list.Add(item);
		}
		Selector = new SelectorVM<SelectorItemVM>((IEnumerable<string>)list, multiplayerOptionsList.IndexOf(MultiplayerOptions.Instance.GetValueTextForOptionWithMultipleSelection(base.OptionType)), (Action<SelectorVM<SelectorItemVM>>)null);
		Selector.SetOnChangeAction((Action<SelectorVM<SelectorItemVM>>)OnChangeSelected);
	}

	public override void RefreshData()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Selector.SetOnChangeAction((Action<SelectorVM<SelectorItemVM>>)null);
		List<string> multiplayerOptionsList = MultiplayerOptions.Instance.GetMultiplayerOptionsList(base.OptionType);
		List<string> multiplayerOptionsTextList = MultiplayerOptions.Instance.GetMultiplayerOptionsTextList(base.OptionType);
		List<string> list = new List<string>();
		foreach (string item in multiplayerOptionsTextList)
		{
			list.Add(item);
		}
		int num = multiplayerOptionsList.IndexOf(MultiplayerOptions.Instance.GetValueTextForOptionWithMultipleSelection(base.OptionType));
		if (num != Selector.SelectedIndex)
		{
			Selector.SelectedIndex = num;
		}
		Selector.SetOnChangeAction((Action<SelectorVM<SelectorItemVM>>)OnChangeSelected);
	}

	public void RefreshList()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		List<string> multiplayerOptionsList = MultiplayerOptions.Instance.GetMultiplayerOptionsList(base.OptionType);
		List<string> multiplayerOptionsTextList = MultiplayerOptions.Instance.GetMultiplayerOptionsTextList(base.OptionType);
		List<string> list = new List<string>();
		foreach (string item in multiplayerOptionsTextList)
		{
			list.Add(item);
		}
		Selector.Refresh((IEnumerable<string>)list, multiplayerOptionsList.IndexOf(MultiplayerOptions.Instance.GetValueTextForOptionWithMultipleSelection(base.OptionType)), (Action<SelectorVM<SelectorItemVM>>)OnChangeSelected);
	}

	private void OnChangeSelected(SelectorVM<SelectorItemVM> selector)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (selector.SelectedIndex >= 0 && selector.SelectedIndex < ((Collection<SelectorItemVM>)(object)selector.ItemList).Count)
		{
			string text = MultiplayerOptions.Instance.GetMultiplayerOptionsList(base.OptionType)[selector.SelectedIndex];
			MultiplayerOptions.Instance.SetValueForOptionWithMultipleSelectionFromText(base.OptionType, text);
			OnChangedSelection?.Invoke(this);
		}
	}
}
