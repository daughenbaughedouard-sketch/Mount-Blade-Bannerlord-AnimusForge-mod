using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MPCultureItemVM : ViewModel
{
	private Action<MPCultureItemVM> _onSelection;

	private bool _isSelected;

	private string _cultureCode;

	private HintViewModel _hint;

	public BasicCultureObject Culture { get; private set; }

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChanged("IsSelected");
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
				((ViewModel)this).OnPropertyChanged("CultureCode");
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
				((ViewModel)this).OnPropertyChanged("Hint");
			}
		}
	}

	public MPCultureItemVM(string cultureCode, Action<MPCultureItemVM> onSelection)
	{
		_onSelection = onSelection;
		CultureCode = cultureCode;
		Culture = MBObjectManager.Instance.GetObject<BasicCultureObject>(cultureCode);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		Hint = new HintViewModel(Culture.Name, (string)null);
	}

	private void ExecuteSelection()
	{
		_onSelection(this);
	}
}
