using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;

namespace SandBox.GauntletUI;

public class SandboxSceneNotificationContextProvider : ISceneNotificationContextProvider
{
	public bool IsContextAllowed(RelevantContextType relevantType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)relevantType == 4)
		{
			return GameStateManager.Current.ActiveState is MapState;
		}
		return true;
	}
}
