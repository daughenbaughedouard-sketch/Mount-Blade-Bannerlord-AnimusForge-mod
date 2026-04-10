using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.ClassFilter;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;

public class MPLobbyClassFilterVM : ViewModel
{
	private Action<MPLobbyClassFilterClassItemVM, bool> _onSelectionChange;

	private string _titleText;

	private MBBindingList<MPLobbyClassFilterFactionItemVM> _factions;

	private MBBindingList<MPLobbyClassFilterClassGroupItemVM> _activeClassGroups;

	public MPLobbyClassFilterClassItemVM SelectedClassItem { get; private set; }

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
	public MBBindingList<MPLobbyClassFilterFactionItemVM> Factions
	{
		get
		{
			return _factions;
		}
		set
		{
			if (value != _factions)
			{
				_factions = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyClassFilterFactionItemVM>>(value, "Factions");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyClassFilterClassGroupItemVM> ActiveClassGroups
	{
		get
		{
			return _activeClassGroups;
		}
		set
		{
			if (value != _activeClassGroups)
			{
				_activeClassGroups = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyClassFilterClassGroupItemVM>>(value, "ActiveClassGroups");
			}
		}
	}

	public MPLobbyClassFilterVM(Action<MPLobbyClassFilterClassItemVM, bool> onSelectionChange)
	{
		_onSelectionChange = onSelectionChange;
		Factions = new MBBindingList<MPLobbyClassFilterFactionItemVM>();
		((Collection<MPLobbyClassFilterFactionItemVM>)(object)Factions).Add(new MPLobbyClassFilterFactionItemVM("empire", isEnabled: true, OnFactionFilterChanged, OnSelectionChange));
		((Collection<MPLobbyClassFilterFactionItemVM>)(object)Factions).Add(new MPLobbyClassFilterFactionItemVM("vlandia", isEnabled: true, OnFactionFilterChanged, OnSelectionChange));
		((Collection<MPLobbyClassFilterFactionItemVM>)(object)Factions).Add(new MPLobbyClassFilterFactionItemVM("battania", isEnabled: true, OnFactionFilterChanged, OnSelectionChange));
		((Collection<MPLobbyClassFilterFactionItemVM>)(object)Factions).Add(new MPLobbyClassFilterFactionItemVM("sturgia", isEnabled: true, OnFactionFilterChanged, OnSelectionChange));
		((Collection<MPLobbyClassFilterFactionItemVM>)(object)Factions).Add(new MPLobbyClassFilterFactionItemVM("khuzait", isEnabled: true, OnFactionFilterChanged, OnSelectionChange));
		((Collection<MPLobbyClassFilterFactionItemVM>)(object)Factions).Add(new MPLobbyClassFilterFactionItemVM("aserai", isEnabled: true, OnFactionFilterChanged, OnSelectionChange));
		ActiveClassGroups = new MBBindingList<MPLobbyClassFilterClassGroupItemVM>();
		((Collection<MPLobbyClassFilterFactionItemVM>)(object)Factions)[0].IsActive = true;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=Q50X65NB}Classes", (Dictionary<string, object>)null)).ToString();
		Factions.ApplyActionOnAllItems((Action<MPLobbyClassFilterFactionItemVM>)delegate(MPLobbyClassFilterFactionItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		ActiveClassGroups.ApplyActionOnAllItems((Action<MPLobbyClassFilterClassGroupItemVM>)delegate(MPLobbyClassFilterClassGroupItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	private void OnFactionFilterChanged(MPLobbyClassFilterFactionItemVM factionItemVm)
	{
		ActiveClassGroups = factionItemVm.ClassGroups;
		OnSelectionChange(factionItemVm.SelectedClassItem);
	}

	private void OnSelectionChange(MPLobbyClassFilterClassItemVM selectedItemVm)
	{
		SelectedClassItem = selectedItemVm;
		_onSelectionChange?.Invoke(selectedItemVm, arg2: false);
	}
}
