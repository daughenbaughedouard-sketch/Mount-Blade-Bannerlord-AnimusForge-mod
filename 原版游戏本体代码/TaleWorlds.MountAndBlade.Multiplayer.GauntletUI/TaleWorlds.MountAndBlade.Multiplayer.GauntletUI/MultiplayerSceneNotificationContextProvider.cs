using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI;

public class MultiplayerSceneNotificationContextProvider : ISceneNotificationContextProvider
{
	public bool IsContextAllowed(RelevantContextType relevantType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)relevantType == 1)
		{
			return GameStateManager.Current.ActiveState is LobbyState;
		}
		return true;
	}
}
