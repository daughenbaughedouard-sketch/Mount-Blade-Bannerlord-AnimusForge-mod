using System;
using NavalDLC.ViewModelCollection.ClanManagement;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.MountAndBlade.View.Screens;

namespace NavalDLC.GauntletUI.Clan;

[GameStateScreen(typeof(ClanState))]
public class NavalGauntletClanScreen : GauntletClanScreen
{
	public NavalGauntletClanScreen(ClanState clanState)
		: base(clanState)
	{
	}

	protected override ClanManagementVM CreateDataSource()
	{
		return (ClanManagementVM)(object)new NavalClanManagementVM((Action)base.CloseClanScreen, (Action<Hero>)base.ShowHeroOnMap, (Action<Hero>)base.OpenPartyScreenForNewClanParty, (Action)base.OpenBannerEditorWithPlayerClan);
	}
}
