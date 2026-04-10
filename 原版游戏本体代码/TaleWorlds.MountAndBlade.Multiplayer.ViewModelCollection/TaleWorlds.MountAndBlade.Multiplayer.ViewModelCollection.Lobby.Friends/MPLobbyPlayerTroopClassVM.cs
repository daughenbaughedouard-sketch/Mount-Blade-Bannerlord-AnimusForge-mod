using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;

public class MPLobbyPlayerTroopClassVM : ViewModel
{
	private string _name;

	private CharacterImageIdentifierVM _preview;

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
	public CharacterImageIdentifierVM Preview
	{
		get
		{
			return _preview;
		}
		set
		{
			if (value != _preview)
			{
				_preview = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterImageIdentifierVM>(value, "Preview");
			}
		}
	}

	public MPLobbyPlayerTroopClassVM()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		Name = "Varangian Guard";
		Preview = new CharacterImageIdentifierVM((CharacterCode)null);
	}
}
