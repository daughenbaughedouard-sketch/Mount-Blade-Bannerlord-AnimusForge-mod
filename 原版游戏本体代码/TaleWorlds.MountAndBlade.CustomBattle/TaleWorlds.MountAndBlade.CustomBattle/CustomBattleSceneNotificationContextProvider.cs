using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.CustomBattle;

public class CustomBattleSceneNotificationContextProvider : ISceneNotificationContextProvider
{
	public bool IsContextAllowed(RelevantContextType relevantType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)relevantType == 2)
		{
			return GameStateManager.Current.ActiveState is CustomBattleState;
		}
		return true;
	}
}
