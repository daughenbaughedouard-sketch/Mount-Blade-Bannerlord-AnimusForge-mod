using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame.HostGameOptions;

public abstract class GenericHostGameOptionDataVM : ViewModel
{
	private int _index;

	private int _category;

	private string _name;

	private bool _isEnabled;

	public OptionType OptionType { get; }

	public int PreferredIndex { get; }

	[DataSourceProperty]
	public int Index
	{
		get
		{
			return _index;
		}
		set
		{
			if (value != _index)
			{
				_index = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Index");
			}
		}
	}

	[DataSourceProperty]
	public int Category
	{
		get
		{
			return _category;
		}
		set
		{
			if (value != _category)
			{
				_category = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Category");
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

	internal GenericHostGameOptionDataVM(OptionsDataType type, OptionType optionType, int preferredIndex)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected I4, but got Unknown
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Category = (int)type;
		OptionType = optionType;
		PreferredIndex = preferredIndex;
		Index = preferredIndex;
		IsEnabled = true;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((ViewModel)this).RefreshValues();
		Name = ((object)GameTexts.FindText("str_multiplayer_option", ((object)OptionType/*cast due to .constrained prefix*/).ToString())).ToString();
	}

	public abstract void RefreshData();
}
