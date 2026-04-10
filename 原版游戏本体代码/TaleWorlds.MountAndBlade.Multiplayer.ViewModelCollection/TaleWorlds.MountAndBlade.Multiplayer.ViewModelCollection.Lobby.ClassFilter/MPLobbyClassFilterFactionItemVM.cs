using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.ClassFilter;

public class MPLobbyClassFilterFactionItemVM : ViewModel
{
	private Action<MPLobbyClassFilterFactionItemVM> _onActiveChanged;

	private Action<MPLobbyClassFilterClassItemVM> _onClassSelect;

	private Dictionary<string, MPLobbyClassFilterClassGroupItemVM> _classGroupDictionary;

	private bool _isActive;

	private bool _isEnabled;

	private string _cultureCode;

	private HintViewModel _hint;

	private MBBindingList<MPLobbyClassFilterClassGroupItemVM> _classGroups;

	public BasicCultureObject Culture { get; private set; }

	public MPLobbyClassFilterClassItemVM SelectedClassItem { get; private set; }

	[DataSourceProperty]
	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			if (value != _isActive)
			{
				_isActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsActive");
				IsActiveChanged();
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
	public string CultureCode
	{
		get
		{
			return _cultureCode;
		}
		set
		{
			if (value != _cultureCode)
			{
				_cultureCode = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureCode");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel Hint
	{
		get
		{
			return _hint;
		}
		set
		{
			if (value != _hint)
			{
				_hint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "Hint");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyClassFilterClassGroupItemVM> ClassGroups
	{
		get
		{
			return _classGroups;
		}
		set
		{
			if (value != _classGroups)
			{
				_classGroups = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyClassFilterClassGroupItemVM>>(value, "ClassGroups");
			}
		}
	}

	public MPLobbyClassFilterFactionItemVM(string cultureCode, bool isEnabled, Action<MPLobbyClassFilterFactionItemVM> onActiveChanged, Action<MPLobbyClassFilterClassItemVM> onClassSelect)
	{
		_onActiveChanged = onActiveChanged;
		_onClassSelect = onClassSelect;
		CultureCode = cultureCode;
		IsEnabled = isEnabled;
		Culture = MBObjectManager.Instance.GetObject<BasicCultureObject>(cultureCode);
		CreateClassGroupAndClasses(Culture);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		Hint = new HintViewModel(Culture.Name, (string)null);
		ClassGroups.ApplyActionOnAllItems((Action<MPLobbyClassFilterClassGroupItemVM>)delegate(MPLobbyClassFilterClassGroupItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		Culture = null;
		_classGroupDictionary.Clear();
	}

	private void CreateClassGroupAndClasses(BasicCultureObject culture)
	{
		_classGroupDictionary = new Dictionary<string, MPLobbyClassFilterClassGroupItemVM>();
		ClassGroups = new MBBindingList<MPLobbyClassFilterClassGroupItemVM>();
		foreach (MPHeroClassGroup multiplayerHeroClassGroup in MultiplayerClassDivisions.MultiplayerHeroClassGroups)
		{
			MPLobbyClassFilterClassGroupItemVM mPLobbyClassFilterClassGroupItemVM = new MPLobbyClassFilterClassGroupItemVM(multiplayerHeroClassGroup);
			((Collection<MPLobbyClassFilterClassGroupItemVM>)(object)ClassGroups).Add(mPLobbyClassFilterClassGroupItemVM);
			_classGroupDictionary.Add(multiplayerHeroClassGroup.StringId, mPLobbyClassFilterClassGroupItemVM);
		}
		foreach (MPHeroClass mPHeroClass in MultiplayerClassDivisions.GetMPHeroClasses(Culture))
		{
			_classGroupDictionary[mPHeroClass.ClassGroup.StringId].AddClass(culture, mPHeroClass, OnClassItemSelect);
		}
		for (int num = ((Collection<MPLobbyClassFilterClassGroupItemVM>)(object)ClassGroups).Count - 1; num >= 0; num--)
		{
			if (((Collection<MPLobbyClassFilterClassItemVM>)(object)((Collection<MPLobbyClassFilterClassGroupItemVM>)(object)ClassGroups)[num].Classes).Count == 0)
			{
				((Collection<MPLobbyClassFilterClassGroupItemVM>)(object)ClassGroups).RemoveAt(num);
			}
		}
		MPLobbyClassFilterClassItemVM mPLobbyClassFilterClassItemVM = ((Collection<MPLobbyClassFilterClassItemVM>)(object)((Collection<MPLobbyClassFilterClassGroupItemVM>)(object)ClassGroups)[0].Classes)[0];
		mPLobbyClassFilterClassItemVM.IsSelected = true;
		SelectedClassItem = mPLobbyClassFilterClassItemVM;
	}

	private void OnClassItemSelect(MPLobbyClassFilterClassItemVM selectedClassItem)
	{
		foreach (MPLobbyClassFilterClassGroupItemVM item in (Collection<MPLobbyClassFilterClassGroupItemVM>)(object)ClassGroups)
		{
			foreach (MPLobbyClassFilterClassItemVM item2 in (Collection<MPLobbyClassFilterClassItemVM>)(object)item.Classes)
			{
				if (item2 != selectedClassItem)
				{
					item2.IsSelected = false;
				}
			}
		}
		SelectedClassItem = selectedClassItem;
		if (_onClassSelect != null)
		{
			_onClassSelect(selectedClassItem);
		}
	}

	private void IsActiveChanged()
	{
		if (IsActive && _onActiveChanged != null)
		{
			_onActiveChanged(this);
		}
	}
}
