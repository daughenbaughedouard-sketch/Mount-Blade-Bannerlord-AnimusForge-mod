using NavalDLC.ViewModelCollection.Map;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Library;

namespace NavalDLC.View.Overlay;

public class NavalGameMenuOverlayProvider : IGameMenuOverlayProvider
{
	public unsafe GameMenuOverlay GetOverlay(MenuOverlayType menuOverlayType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if ((int)menuOverlayType == 4)
		{
			return (GameMenuOverlay)new EncounterMenuOverlayVM();
		}
		if ((int)menuOverlayType == 1 || (int)menuOverlayType == 2 || (int)menuOverlayType == 3)
		{
			return (GameMenuOverlay)(object)new NavalSettlementMenuOverlayVM(menuOverlayType);
		}
		Debug.FailedAssert("Game menu overlay: " + ((object)(*(MenuOverlayType*)(&menuOverlayType))/*cast due to .constrained prefix*/).ToString() + " could not be found", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.View\\Overlay\\NavalGameMenuOverlayProvider.cs", "GetOverlay", 23);
		return null;
	}
}
