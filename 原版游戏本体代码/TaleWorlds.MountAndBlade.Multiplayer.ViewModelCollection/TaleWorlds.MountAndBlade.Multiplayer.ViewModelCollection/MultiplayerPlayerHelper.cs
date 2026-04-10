using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public static class MultiplayerPlayerHelper
{
	private static IReadOnlyCollection<PlayerId> PlatformBlocks => PlatformServices.Instance.BlockedUsers;

	public static bool IsBlocked(PlayerId playerID)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!PermaMuteList.IsPlayerMuted(playerID))
		{
			if (PlatformBlocks != null)
			{
				return PlatformBlocks.Contains(playerID);
			}
			return false;
		}
		return true;
	}
}
