using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI;

public class MultiplayerAdminInformationScreen : GlobalLayer
{
	private MultiplayerAdminInformationVM _dataSource;

	private GauntletMovieIdentifier _movie;

	public static MultiplayerAdminInformationScreen Current { get; private set; }

	public MultiplayerAdminInformationScreen()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		_dataSource = new MultiplayerAdminInformationVM();
		GauntletLayer val = new GauntletLayer("MultiplayerAdminInformation", 15300, false);
		_movie = val.LoadMovie("MultiplayerAdminInformation", (ViewModel)(object)_dataSource);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)val;
		InformationManager.OnAddSystemNotification += OnSystemNotificationReceived;
	}

	public static void OnInitialize()
	{
		if (Current == null)
		{
			Current = new MultiplayerAdminInformationScreen();
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)Current, false);
		}
	}

	public void OnFinalize()
	{
		InformationManager.OnAddSystemNotification -= OnSystemNotificationReceived;
	}

	private void OnSystemNotificationReceived(string obj)
	{
		_dataSource.OnNewMessageReceived(obj);
	}

	public static void OnRemove()
	{
		if (Current != null)
		{
			Current.OnFinalize();
			ScreenManager.RemoveGlobalLayer((GlobalLayer)(object)Current);
			ScreenLayer layer = ((GlobalLayer)Current).Layer;
			((GauntletLayer)((layer is GauntletLayer) ? layer : null)).ReleaseMovie(Current._movie);
			Current._dataSource = null;
			Current = null;
		}
	}
}
