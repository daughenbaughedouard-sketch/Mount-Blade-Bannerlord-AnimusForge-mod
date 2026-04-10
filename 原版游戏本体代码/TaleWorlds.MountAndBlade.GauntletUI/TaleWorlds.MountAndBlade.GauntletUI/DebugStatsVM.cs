using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.GauntletUI;

internal class DebugStatsVM : ViewModel
{
	private string _gameVersion;

	[DataSourceProperty]
	public string GameVersion
	{
		get
		{
			return _gameVersion;
		}
		set
		{
			if (value != _gameVersion)
			{
				_gameVersion = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameVersion");
			}
		}
	}

	public DebugStatsVM()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		GameVersion = ((object)ApplicationVersion.FromParametersFile((string)null)/*cast due to .constrained prefix*/).ToString();
	}
}
