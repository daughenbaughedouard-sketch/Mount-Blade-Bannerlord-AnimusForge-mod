using SandBox.GauntletUI.Tutorial;
using SandBox.View.Map;
using StoryMode.ViewModelCollection.Map;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace StoryMode.GauntletUI;

public class StoryModeGauntletUISubModule : MBSubModuleBase
{
	private bool _registered;

	public override void OnGameInitializationFinished(Game game)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		((MBSubModuleBase)this).OnGameInitializationFinished(game);
		if (game.GameType.RequiresTutorial)
		{
			GauntletTutorialSystem.OnInitialize();
			ScreenManager.OnPushScreen += new OnPushScreenEvent(OnScreenManagerPushScreen);
		}
	}

	private void OnScreenManagerPushScreen(ScreenBase pushedScreen)
	{
		MapScreen val;
		if (!_registered && (val = (MapScreen)(object)((pushedScreen is MapScreen) ? pushedScreen : null)) != null)
		{
			val.MapNotificationView.RegisterMapNotificationType(typeof(ConspiracyQuestMapNotification), typeof(ConspiracyQuestMapNotificationItemVM));
			_registered = true;
		}
	}

	public override void OnGameEnd(Game game)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		((MBSubModuleBase)this).OnGameEnd(game);
		if (game.GameType.RequiresTutorial)
		{
			GauntletTutorialSystem.OnUnload();
			ScreenManager.OnPushScreen -= new OnPushScreenEvent(OnScreenManagerPushScreen);
		}
		_registered = false;
	}
}
