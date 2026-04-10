using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame.HostGameOptions;

public class BooleanHostGameOptionDataVM : GenericHostGameOptionDataVM
{
	private bool _isSelected;

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (value != _isSelected)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
				MultiplayerOptionsExtensions.SetValue(base.OptionType, value, (MultiplayerOptionsAccessMode)1);
			}
		}
	}

	public BooleanHostGameOptionDataVM(OptionType optionType, int preferredIndex)
		: base((OptionsDataType)0, optionType, preferredIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		RefreshData();
	}

	public override void RefreshData()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		IsSelected = MultiplayerOptionsExtensions.GetBoolValue(base.OptionType, (MultiplayerOptionsAccessMode)1);
	}
}
