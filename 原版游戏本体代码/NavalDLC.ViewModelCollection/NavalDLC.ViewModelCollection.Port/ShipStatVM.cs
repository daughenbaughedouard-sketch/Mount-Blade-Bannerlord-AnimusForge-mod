using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port;

public class ShipStatVM : ViewModel
{
	private readonly TextObject _nameTextObj;

	private bool _isBonusBeneficial;

	private string _statId;

	private string _name;

	private string _valueText;

	private string _bonusValueText;

	private BasicTooltipViewModel _tooltip;

	[DataSourceProperty]
	public bool IsBonusBeneficial
	{
		get
		{
			return _isBonusBeneficial;
		}
		set
		{
			if (value != _isBonusBeneficial)
			{
				_isBonusBeneficial = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBonusBeneficial");
			}
		}
	}

	[DataSourceProperty]
	public string StatId
	{
		get
		{
			return _statId;
		}
		set
		{
			if (value != _statId)
			{
				_statId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "StatId");
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
	public string ValueText
	{
		get
		{
			return _valueText;
		}
		set
		{
			if (value != _valueText)
			{
				_valueText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ValueText");
			}
		}
	}

	[DataSourceProperty]
	public string BonusValueText
	{
		get
		{
			return _bonusValueText;
		}
		set
		{
			if (value != _bonusValueText)
			{
				_bonusValueText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BonusValueText");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel Tooltip
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
				((ViewModel)this).OnPropertyChangedWithValue<BasicTooltipViewModel>(value, "Tooltip");
			}
		}
	}

	public ShipStatVM(string statId, TextObject name, string value, string bonusValue = "", bool isBonusBeneficial = true, Func<List<TooltipProperty>> getTooltipProperties = null)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		_nameTextObj = name;
		ValueText = value;
		BonusValueText = bonusValue;
		IsBonusBeneficial = isBonusBeneficial;
		StatId = statId;
		if (getTooltipProperties != null)
		{
			Tooltip = new BasicTooltipViewModel(getTooltipProperties);
		}
		else
		{
			Tooltip = new BasicTooltipViewModel((Func<string>)(() => ((object)GameTexts.FindText("str_ship_stat_explanation", StatId)).ToString()));
		}
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = ((object)_nameTextObj).ToString();
	}
}
