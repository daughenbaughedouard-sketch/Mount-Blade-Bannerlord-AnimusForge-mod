using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public static class MultiplayerCultureExtensions
{
	internal static BasicCultureObject GetCulture(this MissionScoreboardSide side)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		if (side == null)
		{
			return null;
		}
		string text = (((int)side.Side == 1) ? MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1) : MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
		if (!string.IsNullOrEmpty(text))
		{
			return MBObjectManager.Instance.GetObject<BasicCultureObject>(text);
		}
		return null;
	}
}
