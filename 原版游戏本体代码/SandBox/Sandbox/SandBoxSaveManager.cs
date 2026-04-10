using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public class SandBoxSaveManager : ISaveManager
{
	public int GetAutoSaveInterval()
	{
		return BannerlordConfig.AutoSaveInterval;
	}

	public bool IsAutoSaveDisabled()
	{
		return BannerlordConfig.AutoSaveInterval == -1;
	}

	public void OnSaveOver(bool isSuccessful, string newSaveGameName)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (isSuccessful)
		{
			BannerlordConfig.LatestSaveGameName = newSaveGameName;
			BannerlordConfig.Save();
		}
	}
}
