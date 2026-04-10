using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;

namespace NavalDLC.ViewModelCollection.Map.MapBar;

public class NavalMapBarVM : MapBarVM
{
	protected override MapInfoVM CreateInfoVM()
	{
		return (MapInfoVM)(object)new NavalMapInfoVM();
	}
}
