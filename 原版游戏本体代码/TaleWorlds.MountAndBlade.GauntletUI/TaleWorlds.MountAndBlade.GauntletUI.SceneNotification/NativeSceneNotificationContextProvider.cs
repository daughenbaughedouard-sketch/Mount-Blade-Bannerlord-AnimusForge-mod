using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.GauntletUI.SceneNotification;

public class NativeSceneNotificationContextProvider : ISceneNotificationContextProvider
{
	public bool IsContextAllowed(RelevantContextType relevantType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)relevantType == 3)
		{
			return GameStateManager.Current.ActiveState is MissionState;
		}
		return true;
	}
}
