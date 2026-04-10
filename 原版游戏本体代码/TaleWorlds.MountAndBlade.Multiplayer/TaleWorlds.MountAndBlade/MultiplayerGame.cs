using System.Collections.Generic;
using System.Xml;
using TaleWorlds.Avatar.PlayerServices;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.MountAndBlade.Diamond.MultiplayerBadges;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade;

public class MultiplayerGame : GameType
{
	public override bool IsCoreOnlyGameMode => true;

	public static MultiplayerGame Current => Game.Current.GameType as MultiplayerGame;

	public override bool RequiresTutorial => false;

	protected override void OnInitialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		Game currentGame = ((GameType)this).CurrentGame;
		IGameStarter val = (IGameStarter)new BasicGameStarter();
		AddGameModels(val);
		((GameType)this).GameManager.InitializeGameStarter(currentGame, val);
		((GameType)this).GameManager.OnGameStart(((GameType)this).CurrentGame, val);
		currentGame.SetBasicModels(val.Models);
		currentGame.CreateGameManager();
		((GameType)this).GameManager.BeginGameStart(((GameType)this).CurrentGame);
		currentGame.InitializeDefaultGameObjects();
		if (!GameNetwork.IsDedicatedServer)
		{
			currentGame.GameTextManager.LoadGameTexts();
		}
		currentGame.LoadBasicFiles();
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "Items", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "MPCharacters", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "BasicCultures", false);
		MBObjectManagerExtensions.LoadXML(((GameType)this).ObjectManager, "MPClassDivisions", false);
		((GameType)this).ObjectManager.UnregisterNonReadyObjects();
		MultiplayerClassDivisions.Initialize();
		BadgeManager.InitializeWithXML(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/mpbadges.xml");
		((GameType)this).GameManager.OnNewCampaignStart(((GameType)this).CurrentGame, (object)null);
		((GameType)this).GameManager.OnAfterCampaignStart(((GameType)this).CurrentGame);
		((GameType)this).GameManager.OnGameInitializationFinished(((GameType)this).CurrentGame);
		((GameType)this).CurrentGame.AddGameHandler<ChatBox>();
		if (GameNetwork.IsDedicatedServer)
		{
			((GameType)this).CurrentGame.AddGameHandler<MultiplayerGameLogger>();
		}
	}

	private void AddGameModels(IGameStarter basicGameStarter)
	{
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
		basicGameStarter.AddModel<RidingModel>((MBGameModel<RidingModel>)(object)new MultiplayerRidingModel());
		basicGameStarter.AddModel<StrikeMagnitudeCalculationModel>((MBGameModel<StrikeMagnitudeCalculationModel>)(object)new MultiplayerStrikeMagnitudeModel());
		basicGameStarter.AddModel<AgentStatCalculateModel>((MBGameModel<AgentStatCalculateModel>)new MultiplayerAgentStatCalculateModel());
		basicGameStarter.AddModel<AgentApplyDamageModel>((MBGameModel<AgentApplyDamageModel>)new MultiplayerAgentApplyDamageModel());
		basicGameStarter.AddModel<BattleMoraleModel>((MBGameModel<BattleMoraleModel>)new MultiplayerBattleMoraleModel());
		basicGameStarter.AddModel<BattleInitializationModel>((MBGameModel<BattleInitializationModel>)new MultiplayerBattleInitializationModel());
		basicGameStarter.AddModel<BattleSpawnModel>((MBGameModel<BattleSpawnModel>)new MultiplayerBattleSpawnModel());
		basicGameStarter.AddModel<BattleBannerBearersModel>((MBGameModel<BattleBannerBearersModel>)new MultiplayerBattleBannerBearersModel());
		basicGameStarter.AddModel<FormationArrangementModel>((MBGameModel<FormationArrangementModel>)new DefaultFormationArrangementModel());
		basicGameStarter.AddModel<AgentDecideKilledOrUnconsciousModel>((MBGameModel<AgentDecideKilledOrUnconsciousModel>)new DefaultAgentDecideKilledOrUnconsciousModel());
		basicGameStarter.AddModel<DamageParticleModel>((MBGameModel<DamageParticleModel>)new DefaultDamageParticleModel());
		basicGameStarter.AddModel<ItemPickupModel>((MBGameModel<ItemPickupModel>)new DefaultItemPickupModel());
		basicGameStarter.AddModel<MissionSiegeEngineCalculationModel>((MBGameModel<MissionSiegeEngineCalculationModel>)new DefaultSiegeEngineCalculationModel());
	}

	public static Dictionary<string, Equipment> ReadDefaultEquipments(string defaultEquipmentsPath)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		Dictionary<string, Equipment> dictionary = new Dictionary<string, Equipment>();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.Load(defaultEquipmentsPath);
		foreach (XmlNode childNode in xmlDocument.ChildNodes[0].ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Element)
			{
				string value = childNode.Attributes["name"].Value;
				Equipment val = new Equipment((EquipmentType)0);
				val.Deserialize((MBObjectManager)null, childNode);
				dictionary.Add(value, val);
			}
		}
		return dictionary;
	}

	protected override void BeforeRegisterTypes(MBObjectManager objectManager)
	{
	}

	protected override void OnRegisterTypes(MBObjectManager objectManager)
	{
		objectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "MPCharacters", 43u, true, false);
		objectManager.RegisterType<BasicCultureObject>("Culture", "BasicCultures", 17u, true, false);
		objectManager.RegisterType<MPHeroClass>("MPClassDivision", "MPClassDivisions", 45u, true, false);
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
		BadgeManager.OnFinalize();
		MultiplayerOptions.Release();
		InformationManager.ClearAllMessages();
		MultiplayerClassDivisions.Release();
		AvatarServices.ClearAvatarCaches();
	}

	public override void OnStateChanged(GameState oldState)
	{
	}
}
