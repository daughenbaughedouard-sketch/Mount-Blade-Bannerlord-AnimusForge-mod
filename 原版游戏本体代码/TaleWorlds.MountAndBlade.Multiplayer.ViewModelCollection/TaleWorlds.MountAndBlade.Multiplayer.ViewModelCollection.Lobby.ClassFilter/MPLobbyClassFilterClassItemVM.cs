using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.ClassFilter;

public class MPLobbyClassFilterClassItemVM : ViewModel
{
	private Action<MPLobbyClassFilterClassItemVM> _onSelect;

	private bool _isEnabled;

	private bool _isSelected;

	private Color _cultureColor;

	private string _name;

	private string _iconType;

	public MPHeroClass HeroClass { get; private set; }

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
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public Color CultureColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cultureColor;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _cultureColor)
			{
				_cultureColor = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CultureColor");
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
	public string IconType
	{
		get
		{
			return _iconType;
		}
		set
		{
			if (value != _iconType)
			{
				_iconType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "IconType");
			}
		}
	}

	public MPLobbyClassFilterClassItemVM(BasicCultureObject culture, MPHeroClass heroClass, Action<MPLobbyClassFilterClassItemVM> onSelect)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		HeroClass = heroClass;
		_onSelect = onSelect;
		CultureColor = Color.FromUint(culture.Color);
		IconType = ((object)HeroClass.IconType/*cast due to .constrained prefix*/).ToString();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = ((object)HeroClass.HeroName).ToString();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		HeroClass = null;
	}

	private void ExecuteSelect()
	{
		if (_onSelect != null)
		{
			_onSelect(this);
		}
	}
}
