using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI;

public class MultiplayerGauntletGameNotification : GauntletGameNotification
{
	protected override string MovieName => "MultiplayerGameNotificationUI";

	public static void Initialize()
	{
		GauntletGameNotification current = GauntletGameNotification.Current;
		if (current != null)
		{
			current.OnFinalize();
		}
		GauntletGameNotification.Current = (GauntletGameNotification)(object)new MultiplayerGauntletGameNotification();
		ScreenManager.AddGlobalLayer((GlobalLayer)(object)GauntletGameNotification.Current, false);
		GauntletGameNotification.Current.RegisterEvents();
	}
}
