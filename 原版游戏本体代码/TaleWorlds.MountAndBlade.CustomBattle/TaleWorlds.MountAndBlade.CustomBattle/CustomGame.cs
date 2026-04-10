using System;
using System.Collections.Generic;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.MountAndBlade.CustomBattle.CustomBattleObjects;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.CustomBattle;

public class CustomGame : GameType
{
	private List<CustomBattleSceneData> _customBattleScenes;

	private const TerrainType DefaultTerrain = (TerrainType)1;

	private const ForestDensity DefaultForestDensity = (ForestDensity)0;

	public IEnumerable<CustomBattleSceneData> CustomBattleScenes => _customBattleScenes;

	public override bool IsCoreOnlyGameMode => true;

	public CustomBattleBannerEffects CustomBattleBannerEffects { get; private set; }

	public static CustomGame Current => Game.Current.GameType as CustomGame;

	public CustomGame()
	{
		_customBattleScenes = new List<CustomBattleSceneData>();
	}

	protected override void OnInitialize()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		InitializeScenes();
		Game currentGame = ((GameType)this).CurrentGame;
		IGameStarter val = (IGameStarter)new BasicGameStarter();
		InitializeGameModels(val);
		((GameType)this).GameManager.InitializeGameStarter(currentGame, val);
		((GameType)this).GameManager.OnGameStart(((GameType)this).CurrentGame, val);
		MBObjectManager objectManager = currentGame.ObjectManager;
		currentGame.SetBasicModels(val.Models);
		currentGame.CreateGameManager();
		((GameType)this).GameManager.BeginGameStart(((GameType)this).CurrentGame);
		currentGame.InitializeDefaultGameObjects();
		currentGame.LoadBasicFiles();
		LoadCustomGameXmls();
		objectManager.UnregisterNonReadyObjects();
		currentGame.SetDefaultEquipments((IReadOnlyDictionary<string, Equipment>)new Dictionary<string, Equipment>());
		objectManager.UnregisterNonReadyObjects();
		((GameType)this).GameManager.OnNewCampaignStart(((GameType)this).CurrentGame, (object)null);
		((GameType)this).GameManager.OnAfterCampaignStart(((GameType)this).CurrentGame);
		((GameType)this).GameManager.OnGameInitializationFinished(((GameType)this).CurrentGame);
	}

	private void InitializeGameModels(IGameStarter basicGameStarter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		basicGameStarter.AddModel<AgentStatCalculateModel>((MBGameModel<AgentStatCalculateModel>)new CustomBattleAgentStatCalculateModel());
		basicGameStarter.AddModel<AgentApplyDamageModel>((MBGameModel<AgentApplyDamageModel>)new CustomAgentApplyDamageModel());
		basicGameStarter.AddModel<ApplyWeatherEffectsModel>((MBGameModel<ApplyWeatherEffectsModel>)new CustomBattleApplyWeatherEffectsModel());
		basicGameStarter.AddModel<AutoBlockModel>((MBGameModel<AutoBlockModel>)new CustomBattleAutoBlockModel());
		basicGameStarter.AddModel<BattleMoraleModel>((MBGameModel<BattleMoraleModel>)new CustomBattleMoraleModel());
		basicGameStarter.AddModel<BattleInitializationModel>((MBGameModel<BattleInitializationModel>)new CustomBattleInitializationModel());
		basicGameStarter.AddModel<BattleSpawnModel>((MBGameModel<BattleSpawnModel>)new CustomBattleSpawnModel());
		basicGameStarter.AddModel<AgentDecideKilledOrUnconsciousModel>((MBGameModel<AgentDecideKilledOrUnconsciousModel>)new DefaultAgentDecideKilledOrUnconsciousModel());
		basicGameStarter.AddModel<MissionDifficultyModel>((MBGameModel<MissionDifficultyModel>)new DefaultMissionDifficultyModel());
		basicGameStarter.AddModel<RidingModel>((MBGameModel<RidingModel>)new DefaultRidingModel());
		basicGameStarter.AddModel<StrikeMagnitudeCalculationModel>((MBGameModel<StrikeMagnitudeCalculationModel>)new DefaultStrikeMagnitudeModel());
		basicGameStarter.AddModel<BattleBannerBearersModel>((MBGameModel<BattleBannerBearersModel>)new CustomBattleBannerBearersModel());
		basicGameStarter.AddModel<FormationArrangementModel>((MBGameModel<FormationArrangementModel>)new DefaultFormationArrangementModel());
		basicGameStarter.AddModel<DamageParticleModel>((MBGameModel<DamageParticleModel>)new DefaultDamageParticleModel());
		basicGameStarter.AddModel<ItemPickupModel>((MBGameModel<ItemPickupModel>)new DefaultItemPickupModel());
		basicGameStarter.AddModel<ItemValueModel>((MBGameModel<ItemValueModel>)new DefaultItemValueModel());
		basicGameStarter.AddModel<MissionSiegeEngineCalculationModel>((MBGameModel<MissionSiegeEngineCalculationModel>)new DefaultSiegeEngineCalculationModel());
	}

	private void InitializeScenes()
	{
		XmlDocument mergedXmlForManaged = MBObjectManager.GetMergedXmlForManaged("CustomBattleScenes", true, true, "");
		LoadCustomBattleScenes(mergedXmlForManaged);
	}

	private void LoadCustomGameXmls()
	{
		CustomBattleBannerEffects = new CustomBattleBannerEffects();
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "Items", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "EquipmentRosters", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "NPCCharacters", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "SPCultures", false);
	}

	protected override void BeforeRegisterTypes(MBObjectManager objectManager)
	{
	}

	protected override void OnRegisterTypes(MBObjectManager objectManager)
	{
		objectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "NPCCharacters", 43u, true, false);
		objectManager.RegisterType<BasicCultureObject>("Culture", "SPCultures", 17u, true, false);
	}

	protected override void DoLoadingForGameType(GameTypeLoadingStates gameTypeLoadingState, out GameTypeLoadingStates nextState)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		nextState = (GameTypeLoadingStates)(-1);
		switch ((int)gameTypeLoadingState)
		{
		case 0:
			((GameType)this).CurrentGame.Initialize();
			nextState = (GameTypeLoadingStates)1;
			break;
		case 1:
			nextState = (GameTypeLoadingStates)2;
			break;
		case 2:
			nextState = (GameTypeLoadingStates)3;
			break;
		case 3:
			break;
		}
	}

	public override void OnDestroy()
	{
	}

	private void LoadCustomBattleScenes(XmlDocument doc)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		if (doc.ChildNodes.Count == 0)
		{
			throw new TWXmlLoadException("Incorrect XML document format. XML document has no nodes.");
		}
		bool num = doc.ChildNodes[0].Name.ToLower().Equals("xml");
		if (num && doc.ChildNodes.Count == 1)
		{
			throw new TWXmlLoadException("Incorrect XML document format. XML document must have at least one child node");
		}
		XmlNode xmlNode = (num ? doc.ChildNodes[1] : doc.ChildNodes[0]);
		if (xmlNode.Name != "CustomBattleScenes")
		{
			throw new TWXmlLoadException("Incorrect XML document format. Root node's name must be CustomBattleScenes.");
		}
		if (!(xmlNode.Name == "CustomBattleScenes"))
		{
			return;
		}
		foreach (XmlNode childNode in xmlNode.ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Comment)
			{
				continue;
			}
			string sceneID = null;
			TextObject name = null;
			TerrainType result = (TerrainType)1;
			ForestDensity result2 = (ForestDensity)0;
			bool result3 = false;
			bool result4 = false;
			bool result5 = false;
			bool result6 = false;
			for (int i = 0; i < childNode.Attributes.Count; i++)
			{
				if (childNode.Attributes[i].Name == "id")
				{
					sceneID = childNode.Attributes[i].InnerText;
				}
				else if (childNode.Attributes[i].Name == "name")
				{
					name = new TextObject(childNode.Attributes[i].InnerText, (Dictionary<string, object>)null);
				}
				else if (childNode.Attributes[i].Name == "terrain")
				{
					if (!Enum.TryParse<TerrainType>(childNode.Attributes[i].InnerText, out result))
					{
						result = (TerrainType)1;
					}
				}
				else if (childNode.Attributes[i].Name == "forest_density")
				{
					char[] array = childNode.Attributes[i].InnerText.ToLower().ToCharArray();
					array[0] = char.ToUpper(array[0]);
					if (!Enum.TryParse<ForestDensity>(new string(array), out result2))
					{
						result2 = (ForestDensity)0;
					}
				}
				else if (childNode.Attributes[i].Name == "is_siege_map")
				{
					bool.TryParse(childNode.Attributes[i].InnerText, out result3);
				}
				else if (childNode.Attributes[i].Name == "is_village_map")
				{
					bool.TryParse(childNode.Attributes[i].InnerText, out result4);
				}
				else if (childNode.Attributes[i].Name == "is_lords_hall_map")
				{
					bool.TryParse(childNode.Attributes[i].InnerText, out result5);
				}
				else if (childNode.Attributes[i].Name == "is_naval_map")
				{
					bool.TryParse(childNode.Attributes[i].InnerText, out result6);
				}
			}
			if (result6)
			{
				continue;
			}
			XmlNodeList childNodes = childNode.ChildNodes;
			List<TerrainType> list = new List<TerrainType>();
			foreach (XmlNode item in childNodes)
			{
				if (item.NodeType == XmlNodeType.Comment || !(item.Name == "flags"))
				{
					continue;
				}
				foreach (XmlNode childNode2 in item.ChildNodes)
				{
					if (childNode2.NodeType != XmlNodeType.Comment && childNode2.Attributes["name"].InnerText == "TerrainType" && Enum.TryParse<TerrainType>(childNode2.Attributes["value"].InnerText, out TerrainType result7) && !list.Contains(result7))
					{
						list.Add(result7);
					}
				}
			}
			_customBattleScenes.Add(new CustomBattleSceneData(sceneID, name, result, list, result2, result3, result4, result5));
		}
	}

	public override void OnStateChanged(GameState oldState)
	{
	}
}
