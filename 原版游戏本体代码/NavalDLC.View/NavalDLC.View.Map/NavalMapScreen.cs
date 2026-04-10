using Helpers;
using NavalDLC.ViewModelCollection;
using SandBox.View.Map;
using SandBox.ViewModelCollection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace NavalDLC.View.Map;

[GameStateScreen(typeof(MapState))]
public class NavalMapScreen : MapScreen
{
	public NavalMapScreen(MapState mapState)
		: base(mapState)
	{
	}

	protected override bool TickNavigationInput(float dt)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (((MapScreen)this).TickNavigationInput(dt))
		{
			return true;
		}
		if (((ScreenLayer)((MapScreen)this).SceneLayer).Input.IsGameKeyPressed(45))
		{
			NavigationPermissionItem permission = ((MapScreen)this).NavigationHandler.GetElement("manage_fleet").Permission;
			if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
			{
				OpenManageFleet();
				return true;
			}
		}
		return false;
	}

	private void OpenManageFleet()
	{
		if (Hero.MainHero != null && !Hero.MainHero.IsPrisoner && !Hero.MainHero.IsDead)
		{
			PortStateHelper.OpenAsManageFleet(new MBReadOnlyList<Ship>());
		}
	}

	protected override SPScoreboardVM CreateSimulationScoreboardDatasource(BattleSimulation battleSimulation)
	{
		MapEvent mapEvent = battleSimulation.MapEvent;
		if (mapEvent != null && mapEvent.IsNavalMapEvent)
		{
			return (SPScoreboardVM)(object)NavalScoreboardVM.CreateSimulation(battleSimulation);
		}
		return ((MapScreen)this).CreateSimulationScoreboardDatasource(battleSimulation);
	}
}
