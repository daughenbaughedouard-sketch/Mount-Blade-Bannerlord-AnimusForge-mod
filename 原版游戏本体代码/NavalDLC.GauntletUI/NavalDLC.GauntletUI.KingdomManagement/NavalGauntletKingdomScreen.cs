using System;
using NavalDLC.ViewModelCollection.Kingdom;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.MountAndBlade.View.Screens;

namespace NavalDLC.GauntletUI.KingdomManagement;

[GameStateScreen(typeof(KingdomState))]
public class NavalGauntletKingdomScreen : GauntletKingdomScreen
{
	public NavalGauntletKingdomScreen(KingdomState kingdomState)
		: base(kingdomState)
	{
	}

	protected override KingdomManagementVM CreateDataSource()
	{
		return (KingdomManagementVM)(object)new NavalKingdomManagementVM((Action)base.CloseKingdomScreen, (Action)base.OpenArmyManagement, (Action<Army>)base.ShowArmyOnMap);
	}
}
