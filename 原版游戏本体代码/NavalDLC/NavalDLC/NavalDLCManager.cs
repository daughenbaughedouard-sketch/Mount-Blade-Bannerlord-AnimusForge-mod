using System.Collections.Generic;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Map;
using NavalDLC.Settlements;
using NavalDLC.Settlements.Building;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace NavalDLC;

public class NavalDLCManager : GameHandler
{
	public static NavalDLCManager Instance;

	public GameModels GameModels { get; private set; }

	public NavalCulturalFeats NavalCulturalFeats { get; private set; }

	public NavalBuildingTypes NavalBuildingTypes { get; private set; }

	public NavalVillageTypes NavalVillageTypes { get; private set; }

	public NavalSkills NavalSkills { get; private set; }

	public NavalSkillEffects NavalSkillEffects { get; private set; }

	public NavalPerks NavalPerks { get; private set; }

	public NavalPolicies NavalPolicies { get; private set; }

	public NavalStorylineData NavalStorylineData { get; private set; }

	public NavalDLCEvents NavalDLCEvents { get; private set; }

	public NavalItemCategories NavalItemCategories { get; private set; }

	public INavalMapSceneWrapper NavalMapSceneWrapper { get; set; }

	public Dictionary<Village, List<FishingPartyComponent>> FishingParties { get; private set; }

	public StormManager StormManager { get; internal set; }

	public override void OnAfterSave()
	{
	}

	public override void OnBeforeSave()
	{
	}

	protected override void OnInitialize()
	{
	}

	protected override void OnTick(float dt)
	{
		NavalMapSceneWrapper?.Tick(dt);
	}

	public void OnGameStart(Game game, IGameStarter gameStarter)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		GameModels = game.AddGameModelsManager<GameModels>(gameStarter.Models);
		if (game.GameType is Campaign)
		{
			NavalDLCEvents = new NavalDLCEvents();
			Campaign.Current.AddCampaignEventReceiver((CampaignEventReceiver)(object)NavalDLCEvents);
			if ((int)Campaign.Current.CampaignGameLoadingType == 1 || (int)Campaign.Current.CampaignGameLoadingType == 0)
			{
				Campaign.Current.AddCustomManager<StormManager>();
				StormManager = Campaign.Current.GetCustomManager<StormManager>();
			}
			else if ((int)Campaign.Current.CampaignGameLoadingType == 2)
			{
				StormManager = Campaign.Current.GetCustomManager<StormManager>();
				StormManager.OnAfterLoad();
			}
		}
	}

	public void OnGameEnd(Game game)
	{
		NavalMapSceneWrapper = null;
		Instance = null;
	}

	public void InitializeNavalGameObjects(Game game)
	{
		GameType gameType = game.GameType;
		Campaign val = (Campaign)(object)((gameType is Campaign) ? gameType : null);
		if (val != null)
		{
			NavalBuildingTypes = new NavalBuildingTypes();
			NavalCulturalFeats = new NavalCulturalFeats();
			NavalItemCategories = new NavalItemCategories();
			NavalVillageTypes = new NavalVillageTypes();
			NavalStorylineData = new NavalStorylineData();
		}
		NavalSkills = new NavalSkills();
		if (val != null)
		{
			NavalSkillEffects = new NavalSkillEffects();
			NavalPerks = new NavalPerks();
			NavalPolicies = new NavalPolicies();
			val.SkillLevelingManager = (ISkillLevelingManager)(object)new NavalSkillLevellingManager();
			FishingParties = new Dictionary<Village, List<FishingPartyComponent>>();
		}
	}
}
