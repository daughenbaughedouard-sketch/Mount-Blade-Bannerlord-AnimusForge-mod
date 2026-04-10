using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame.HostGameOptions;

public class NumericHostGameOptionDataVM : GenericHostGameOptionDataVM
{
	private int _value;

	private int _min;

	private int _max;

	[DataSourceProperty]
	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (value != _value)
			{
				_value = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Value");
				((ViewModel)this).OnPropertyChanged("ValueAsString");
				MultiplayerOptionsExtensions.SetValue(base.OptionType, value, (MultiplayerOptionsAccessMode)1);
			}
		}
	}

	[DataSourceProperty]
	public string ValueAsString => _value.ToString();

	[DataSourceProperty]
	public int Min
	{
		get
		{
			return _min;
		}
		set
		{
			if (value != _min)
			{
				_min = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Min");
			}
		}
	}

	[DataSourceProperty]
	public int Max
	{
		get
		{
			return _max;
		}
		set
		{
			if (value != _max)
			{
				_max = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Max");
			}
		}
	}

	public NumericHostGameOptionDataVM(OptionType optionType, int preferredIndex)
		: base((OptionsDataType)1, optionType, preferredIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		RefreshData();
	}

	public override void RefreshData()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerOptionsProperty optionProperty = MultiplayerOptionsExtensions.GetOptionProperty(base.OptionType);
		Min = optionProperty.BoundsMin;
		Max = optionProperty.BoundsMax;
		Value = MultiplayerOptionsExtensions.GetIntValue(base.OptionType, (MultiplayerOptionsAccessMode)1);
	}
}
