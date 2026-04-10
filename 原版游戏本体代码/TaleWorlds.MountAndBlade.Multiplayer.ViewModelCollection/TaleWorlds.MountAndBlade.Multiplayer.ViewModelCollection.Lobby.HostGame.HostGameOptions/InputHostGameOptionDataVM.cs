using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame.HostGameOptions;

public class InputHostGameOptionDataVM : GenericHostGameOptionDataVM
{
	private string _text;

	[DataSourceProperty]
	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (value != _text)
			{
				_text = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Text");
				MultiplayerOptionsExtensions.SetValue(base.OptionType, value, (MultiplayerOptionsAccessMode)1);
			}
		}
	}

	public InputHostGameOptionDataVM(OptionType optionType, int preferredIndex)
		: base((OptionsDataType)4, optionType, preferredIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		RefreshData();
	}

	public override void RefreshData()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		string strValue = MultiplayerOptionsExtensions.GetStrValue(base.OptionType, (MultiplayerOptionsAccessMode)1);
		Text = strValue;
	}
}
