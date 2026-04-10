using System;
using System.Collections.Generic;
using System.Xml;
using NavalDLC.ComponentInterfaces;
using NavalDLC.CustomBattle.CustomBattleObjects;
using NavalDLC.GameComponents;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.CustomBattle;

public class NavalCustomGame : GameType
{
	private List<NavalCustomBattleSceneData> _customBattleScenes;

	private const TerrainType DefaultTerrain = (TerrainType)19;

	public IEnumerable<NavalCustomBattleSceneData> CustomBattleScenes => _customBattleScenes;

	public override string GameTypeStringId => "CustomGame";

	public override bool IsCoreOnlyGameMode => true;

	public NavalCustomBattleBannerEffects NavalCustomBattleBannerEffects { get; private set; }

	public static NavalCustomGame Current => Game.Current.GameType as NavalCustomGame;

	public NavalCustomGame()
	{
		_customBattleScenes = new List<NavalCustomBattleSceneData>();
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
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		basicGameStarter.AddModel<AgentStatCalculateModel>((MBGameModel<AgentStatCalculateModel>)new CustomBattleAgentStatCalculateModel());
		basicGameStarter.AddModel<AgentStatCalculateModel>((MBGameModel<AgentStatCalculateModel>)(object)new NavalCustomBattleAgentStatCalculateModel());
		basicGameStarter.AddModel<AgentApplyDamageModel>((MBGameModel<AgentApplyDamageModel>)(object)new NavalDLCCustomAgentApplyDamageModel());
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
		basicGameStarter.AddModel<CampaignShipParametersModel>((MBGameModel<CampaignShipParametersModel>)(object)new NavalDLCCampaignShipParametersModel());
		basicGameStarter.AddModel<ShipPhysicsParametersModel>((MBGameModel<ShipPhysicsParametersModel>)new NavalDLCShipPhysicsParametersModel());
		basicGameStarter.AddModel<ClanShipOwnershipModel>((MBGameModel<ClanShipOwnershipModel>)new NavalDLCClanShipOwnershipModel());
		basicGameStarter.AddModel<ShipDistributionModel>((MBGameModel<ShipDistributionModel>)new NavalDLCShipDistributionModel());
		basicGameStarter.AddModel<ShipDeploymentModel>((MBGameModel<ShipDeploymentModel>)new NavalDLCShipDeploymentModel());
		basicGameStarter.AddModel<MissionShipParametersModel>((MBGameModel<MissionShipParametersModel>)(object)new NavalCustomBattleMissionShipParametersModel());
		basicGameStarter.AddModel<BattleInitializationModel>((MBGameModel<BattleInitializationModel>)(object)new NavalCustomBattleInitializationModel());
	}

	private void InitializeScenes()
	{
		XmlDocument mergedXmlForManaged = MBObjectManager.GetMergedXmlForManaged("CustomBattleScenes", true, true, "");
		LoadCustomBattleScenes(mergedXmlForManaged);
	}

	private void LoadCustomGameXmls()
	{
		NavalCustomBattleBannerEffects = new NavalCustomBattleBannerEffects();
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "Items", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "EquipmentRosters", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "NPCCharacters", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "SPCultures", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "ShipUpgradePieces", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "ShipSlots", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "ShipHulls", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "ShipPhysicsReferences", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "MissionShips", false);
	}

	protected override void BeforeRegisterTypes(MBObjectManager objectManager)
	{
	}

	protected override void OnRegisterTypes(MBObjectManager objectManager)
	{
		objectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "NPCCharacters", 43u, true, false);
		objectManager.RegisterType<BasicCultureObject>("Culture", "SPCultures", 17u, true, false);
		objectManager.RegisterType<ShipUpgradePiece>("ShipUpgradePiece", "ShipUpgradePieces", 60u, true, false);
		objectManager.RegisterType<ShipSlot>("ShipSlot", "ShipSlots", 59u, true, false);
		objectManager.RegisterType<ShipHull>("ShipHull", "ShipHulls", 58u, true, false);
		objectManager.RegisterType<ShipPhysicsReference>("ShipPhysicsReference", "ShipPhysicsReferences", 64u, true, false);
		objectManager.RegisterType<MissionShipObject>("MissionShip", "MissionShips", 57u, true, false);
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
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
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
			bool result = false;
			string sceneID = null;
			TextObject name = null;
			TerrainType result2 = (TerrainType)19;
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
				else if (childNode.Attributes[i].Name == "is_naval_map")
				{
					bool.TryParse(childNode.Attributes[i].InnerText, out result);
				}
				else if (childNode.Attributes[i].Name == "terrain" && !Enum.TryParse<TerrainType>(childNode.Attributes[i].InnerText, out result2))
				{
					result2 = (TerrainType)19;
				}
			}
			if (result)
			{
				_customBattleScenes.Add(new NavalCustomBattleSceneData(sceneID, name, result2));
			}
		}
	}

	public override void OnStateChanged(GameState oldState)
	{
	}
}
