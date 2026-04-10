using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.SaveLoad;

public class SavedGamePropertyVM : ViewModel
{
	public enum SavedGameProperty
	{
		None = -1,
		Health,
		Gold,
		Influence,
		PartySize,
		Food,
		Fiefs,
		Ships
	}

	private TextObject _valueText;

	private HintViewModel _hint;

	private string _propertyType;

	private string _value;

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
	public string PropertyType
	{
		get
		{
			return _propertyType;
		}
		set
		{
			if (value != _propertyType)
			{
				_propertyType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PropertyType");
			}
		}
	}

	[DataSourceProperty]
	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (value != _value)
			{
				_value = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Value");
			}
		}
	}

	public SavedGamePropertyVM(SavedGameProperty type, TextObject value, TextObject hint)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		PropertyType = type.ToString();
		_valueText = value;
		Hint = new HintViewModel(hint, (string)null);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Value = ((object)_valueText).ToString();
	}
}
