using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Conversation.MissionLogics;

public class ConversationMissionLogic : MissionLogic
{
	private enum NavalConversationCameraState
	{
		None,
		SameShip,
		Level,
		LookDown,
		LookUp
	}

	private const float MinimumAgentHeightForRopeAnimation = 1.76f;

	private const float MaximumWindStrength = 6f;

	private const float MaximumWaveStrength = 2.5f;

	private const float WindStrengthAmplifier = 2f;

	private readonly List<Agent> _addBloodToAgents;

	private Agent _curConversationPartnerAgent;

	private bool _isRenderingStarted;

	private bool _conversationStarted;

	private bool _isCivilianEquipmentRequiredForLeader;

	private bool _isCivilianEquipmentRequiredForBodyGuards;

	private List<GameEntity> _usedSpawnPoints;

	private GameEntity _agentHangPointShort;

	private GameEntity _agentHangPointSecondShort;

	private GameEntity _agentHangPointTall;

	private GameEntity _agentHangPointSecondTall;

	private GameEntity _conversationSet;

	private bool _realCameraController;

	private readonly bool _isNaval;

	private float _otherPartyHeightMultiplier;

	private NavalConversationCameraState _navalConversationState;

	public GameEntity CustomConversationCameraEntity;

	private bool IsReadyForConversation
	{
		get
		{
			if (_isRenderingStarted && Agent.Main != null)
			{
				return Agent.Main.IsActive();
			}
			return false;
		}
	}

	public ConversationCharacterData OtherSideConversationData { get; private set; }

	public ConversationCharacterData PlayerConversationData { get; private set; }

	public bool IsMultiAgentConversation { get; private set; }

	public ConversationMissionLogic(ConversationCharacterData playerCharacterData, ConversationCharacterData otherCharacterData, bool isMultiAgentConversation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		PlayerConversationData = playerCharacterData;
		OtherSideConversationData = otherCharacterData;
		IsMultiAgentConversation = isMultiAgentConversation;
		int isNaval;
		if (!isMultiAgentConversation)
		{
			PartyBase party = playerCharacterData.Party;
			bool? obj;
			if (party == null)
			{
				obj = null;
			}
			else
			{
				MobileParty mobileParty = party.MobileParty;
				obj = ((mobileParty != null) ? new bool?(mobileParty.IsCurrentlyAtSea) : ((bool?)null));
			}
			bool? flag = obj;
			if (!flag.HasValue)
			{
				PartyBase party2 = otherCharacterData.Party;
				bool? obj2;
				if (party2 == null)
				{
					obj2 = null;
				}
				else
				{
					MobileParty mobileParty2 = party2.MobileParty;
					obj2 = ((mobileParty2 != null) ? new bool?(mobileParty2.IsCurrentlyAtSea) : ((bool?)null));
				}
				isNaval = ((obj2 ?? false) ? 1 : 0);
			}
			else
			{
				isNaval = ((flag == true) ? 1 : 0);
			}
		}
		else
		{
			isNaval = 0;
		}
		_isNaval = (byte)isNaval != 0;
		_isCivilianEquipmentRequiredForLeader = otherCharacterData.IsCivilianEquipmentRequiredForLeader;
		_isCivilianEquipmentRequiredForBodyGuards = otherCharacterData.IsCivilianEquipmentRequiredForBodyGuardCharacters;
		_addBloodToAgents = new List<Agent>();
	}

	public override void AfterStart()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Invalid comparison between Unknown and I4
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).AfterStart();
		_realCameraController = ((MissionBehavior)this).Mission.CameraIsFirstPerson;
		Vec2 val;
		if (_isNaval)
		{
			string navalConversationCameraTag = GetNavalConversationCameraTag(OtherSideConversationData.Party);
			val = Mission.Current.Scene.GetGlobalWindStrengthVector();
			float num = ((Vec2)(ref val)).Length * 2f;
			float waterStrength = Mission.Current.Scene.GetWaterStrength();
			CustomConversationCameraEntity = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag(navalConversationCameraTag);
			Scene scene = Mission.Current.Scene;
			val = MathF.Clamp(num, 1E-05f, 6f) * Vec2.Side;
			scene.SetGlobalWindStrengthVector(ref val);
			Mission.Current.Scene.SetWaterStrength(MathF.Clamp(waterStrength, 1E-05f, 2.5f));
		}
		else if (IsMultiAgentConversation)
		{
			val = Mission.Current.Scene.GetGlobalWindStrengthVector();
			float num2 = ((Vec2)(ref val)).Length * 2f;
			float waterStrength2 = Mission.Current.Scene.GetWaterStrength();
			Scene scene2 = Mission.Current.Scene;
			val = MathF.Clamp(num2, 1E-05f, 6f) * Vec2.Side;
			scene2.SetGlobalWindStrengthVector(ref val);
			Mission.Current.Scene.SetWaterStrength(MathF.Clamp(waterStrength2, 1E-05f, 2.5f));
			((MissionBehavior)this).Mission.CameraIsFirstPerson = true;
		}
		else
		{
			((MissionBehavior)this).Mission.CameraIsFirstPerson = true;
		}
		IEnumerable<GameEntity> enumerable = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("binary_conversation_point");
		if (enumerable.Any())
		{
			_conversationSet = Extensions.GetRandomElement<GameEntity>(Extensions.ToMBList<GameEntity>(enumerable));
		}
		_usedSpawnPoints = new List<GameEntity>();
		BattleSideEnum val2 = (BattleSideEnum)1;
		if (_isNaval)
		{
			val2 = (BattleSideEnum)1;
		}
		else if (PlayerSiege.PlayerSiegeEvent != null)
		{
			val2 = PlayerSiege.PlayerSide;
		}
		else if (PlayerEncounter.Current != null)
		{
			val2 = ((!PlayerEncounter.InsideSettlement || (int)PlayerEncounter.Current.OpponentSide == 0) ? ((BattleSideEnum)1) : ((BattleSideEnum)0));
			if (PlayerEncounter.Current.EncounterSettlementAux != null && PlayerEncounter.Current.EncounterSettlementAux.MapFaction == Hero.MainHero.MapFaction)
			{
				val2 = ((!PlayerEncounter.Current.EncounterSettlementAux.IsUnderSiege) ? ((BattleSideEnum)1) : ((BattleSideEnum)0));
			}
		}
		((MissionBehavior)this).Mission.PlayerTeam = ((MissionBehavior)this).Mission.Teams.Add(val2, Hero.MainHero.MapFaction.Color, Hero.MainHero.MapFaction.Color2, (Banner)null, true, false, true);
		int num3;
		if (!OtherSideConversationData.NoHorse)
		{
			EquipmentElement val3 = ((BasicCharacterObject)OtherSideConversationData.Character).Equipment[10];
			if (((EquipmentElement)(ref val3)).Item != null)
			{
				val3 = ((BasicCharacterObject)OtherSideConversationData.Character).Equipment[10];
				if (((EquipmentElement)(ref val3)).Item.HasHorseComponent)
				{
					num3 = (((int)val2 == 0) ? 1 : 0);
					goto IL_02c5;
				}
			}
		}
		num3 = 0;
		goto IL_02c5;
		IL_02c5:
		bool flag = (byte)num3 != 0;
		MatrixFrame val4;
		MatrixFrame initialFrame;
		if (_conversationSet != (GameEntity)null)
		{
			if (((MissionBehavior)this).Mission.PlayerTeam.IsDefender)
			{
				val4 = GetDefenderSideSpawnFrame();
				initialFrame = GetAttackerSideSpawnFrame(flag);
			}
			else
			{
				val4 = GetAttackerSideSpawnFrame(flag);
				initialFrame = GetDefenderSideSpawnFrame();
			}
		}
		else
		{
			val4 = GetPlayerSideSpawnFrameInSettlement();
			initialFrame = GetOtherSideSpawnFrameInSettlement(val4);
		}
		if (_isNaval)
		{
			if (_navalConversationState != NavalConversationCameraState.SameShip)
			{
				GameEntity firstEntityWithName = ((MissionBehavior)this).Mission.Scene.GetFirstEntityWithName("Ship");
				if (firstEntityWithName != (GameEntity)null)
				{
					WeakGameEntity weakEntity = firstEntityWithName.WeakEntity;
					WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref weakEntity)).GetFirstChildEntityWithTag("tall_rope");
					if (firstChildEntityWithTag != WeakGameEntity.Invalid)
					{
						_agentHangPointTall = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref firstChildEntityWithTag)).GetFirstChildEntityWithTagRecursive("rope_hang_point"));
						_agentHangPointSecondTall = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref firstChildEntityWithTag)).GetFirstChildEntityWithTagRecursive("rope_hang_point2"));
					}
					WeakGameEntity firstChildEntityWithTag2 = ((WeakGameEntity)(ref weakEntity)).GetFirstChildEntityWithTag("short_rope");
					if (firstChildEntityWithTag2 != WeakGameEntity.Invalid)
					{
						_agentHangPointShort = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref firstChildEntityWithTag2)).GetFirstChildEntityWithTagRecursive("rope_hang_point"));
						_agentHangPointSecondShort = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref firstChildEntityWithTag2)).GetFirstChildEntityWithTagRecursive("rope_hang_point2"));
					}
				}
			}
			else
			{
				((MatrixFrame)(ref initialFrame)).Rotate(MathF.PI, ref Vec3.Up);
			}
		}
		SpawnPlayer(PlayerConversationData, val4);
		SpawnOtherSide(OtherSideConversationData, initialFrame, flag, !((MissionBehavior)this).Mission.PlayerTeam.IsDefender);
	}

	private void SpawnPlayer(ConversationCharacterData playerConversationData, MatrixFrame initialFrame)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame initialFrame2 = new MatrixFrame(ref initialFrame.rotation, ref initialFrame.origin);
		((Mat3)(ref initialFrame2.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		SpawnCharacter(CharacterObject.PlayerCharacter, playerConversationData, initialFrame2, in ActionIndexCache.act_conversation_normal_loop);
	}

	private void SpawnOtherSide(ConversationCharacterData characterData, MatrixFrame initialFrame, bool spawnWithHorse, bool isDefenderSide)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Expected O, but got Unknown
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = new MatrixFrame(ref initialFrame.rotation, ref initialFrame.origin);
		if (!_isNaval && Agent.Main != null)
		{
			val.rotation.f = Agent.Main.Position - val.origin;
		}
		((Mat3)(ref val.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)characterData.Character).Race, "_settlement");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)characterData.Character).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)characterData.Character, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerTeam).Monster(monsterWithSuffix)
			.InitialPosition(ref val.origin);
		Vec2 asVec = ((Vec3)(ref val.rotation.f)).AsVec2;
		AgentBuildData val2 = obj.InitialDirection(ref asVec).NoHorses(!spawnWithHorse).CivilianEquipment(_isCivilianEquipmentRequiredForLeader)
			.SetPrepareImmediately();
		Hero heroObject = characterData.Character.HeroObject;
		if (((heroObject != null) ? heroObject.MapFaction : null) != null)
		{
			val2.Banner(characterData.Character.HeroObject.MapFaction.Banner);
			val2.ClothingColor1(characterData.Character.HeroObject.MapFaction.Color).ClothingColor2(characterData.Character.HeroObject.MapFaction.Color2);
		}
		else
		{
			PartyBase party = characterData.Party;
			object obj2;
			if (party == null)
			{
				obj2 = null;
			}
			else
			{
				Hero leaderHero = party.LeaderHero;
				obj2 = ((leaderHero != null) ? leaderHero.ClanBanner : null);
			}
			if (obj2 != null)
			{
				val2.Banner(characterData.Party.LeaderHero.ClanBanner);
				val2.ClothingColor1(characterData.Party.LeaderHero.MapFaction.Color).ClothingColor2(characterData.Party.LeaderHero.MapFaction.Color2);
			}
			else
			{
				PartyBase party2 = characterData.Party;
				if (((party2 != null) ? party2.MapFaction : null) != null)
				{
					PartyBase party3 = characterData.Party;
					object obj3;
					if (party3 == null)
					{
						obj3 = null;
					}
					else
					{
						IFaction mapFaction = party3.MapFaction;
						obj3 = ((mapFaction != null) ? mapFaction.Banner : null);
					}
					val2.Banner((Banner)obj3);
					val2.ClothingColor1(characterData.Party.MapFaction.Color).ClothingColor2(characterData.Party.MapFaction.Color2);
				}
			}
		}
		if (spawnWithHorse)
		{
			EquipmentElement val3 = ((BasicCharacterObject)characterData.Character).Equipment[(EquipmentIndex)10];
			val2.MountKey(MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val3)).Item, ((BasicCharacterObject)characterData.Character).GetMountKeySeed()));
		}
		if (characterData.Party != null)
		{
			val2.TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(characterData.Party, characterData.Character, 0, new UniqueTroopDescriptor(FlattenedTroopRoster.GenerateUniqueNoFromParty(characterData.Party.MobileParty, 0)), false, false));
		}
		Agent val4 = ((MissionBehavior)this).Mission.SpawnAgent(val2, false);
		_otherPartyHeightMultiplier = val4.GetEyeGlobalHeight();
		if (characterData.SpawnedAfterFight)
		{
			_addBloodToAgents.Add(val4);
		}
		if (val4.MountAgent == null)
		{
			val4.SetActionChannel(0, ref ActionIndexCache.act_conversation_normal_loop, false, (AnimFlags)0, 0f, 1f, 0f, 0.4f, MBRandom.RandomFloat, false, -0.2f, 0, true);
		}
		else
		{
			val4.MountAgent.AgentVisuals.SetAgentLodZeroOrMax(true);
		}
		val4.AgentVisuals.SetAgentLodZeroOrMax(true);
		_curConversationPartnerAgent = val4;
		bool flag = characterData.Character.HeroObject != null && characterData.Character.HeroObject.IsPlayerCompanion;
		if (!characterData.NoBodyguards && !flag)
		{
			SpawnBodyguards(isDefenderSide);
		}
	}

	private MatrixFrame GetDefenderSideSpawnFrame()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame result = MatrixFrame.Identity;
		foreach (GameEntity child in _conversationSet.GetChildren())
		{
			if (child.HasTag("opponent_infantry_spawn"))
			{
				result = child.GetGlobalFrame();
				break;
			}
		}
		((Mat3)(ref result.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		return result;
	}

	private MatrixFrame GetAttackerSideSpawnFrame(bool hasHorse)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame result = MatrixFrame.Identity;
		if (_isNaval && CustomConversationCameraEntity != (GameEntity)null)
		{
			result = CustomConversationCameraEntity.GetGlobalFrame();
		}
		else
		{
			foreach (GameEntity child in _conversationSet.GetChildren())
			{
				if (hasHorse && child.HasTag("player_cavalry_spawn"))
				{
					result = child.GetGlobalFrame();
					break;
				}
				if (child.HasTag("player_infantry_spawn"))
				{
					result = child.GetGlobalFrame();
					break;
				}
			}
		}
		((Mat3)(ref result.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		return result;
	}

	private MatrixFrame GetPlayerSideSpawnFrameInSettlement()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		GameEntity obj = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawnpoint_player") ?? ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_player_conversation").FirstOrDefault() ?? ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawnpoint_player_outside");
		MatrixFrame result = ((obj != null) ? obj.GetFrame() : MatrixFrame.Identity);
		((Mat3)(ref result.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		return result;
	}

	private MatrixFrame GetOtherSideSpawnFrameInSettlement(MatrixFrame playerFrame)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame result = playerFrame;
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(playerFrame.rotation.f, -1f);
		((Vec3)(ref val)).Normalize();
		result.origin = playerFrame.origin + 4f * val;
		((Mat3)(ref result.rotation)).RotateAboutUp(MathF.PI);
		return result;
	}

	public override void OnRenderingStarted()
	{
		_isRenderingStarted = true;
		Debug.Print("\n ConversationMissionLogic::OnRenderingStarted\n", 0, (DebugColor)7, 64uL);
	}

	private void InitializeAfterCreation(Agent conversationPartnerAgent, PartyBase conversationPartnerParty)
	{
		Campaign.Current.ConversationManager.SetupAndStartMapConversation((conversationPartnerParty != null) ? conversationPartnerParty.MobileParty : null, (IAgent)(object)conversationPartnerAgent, (IAgent)(object)Mission.Current.MainAgentServer);
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)1, true);
	}

	public override void OnMissionTick(float dt)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		if (_addBloodToAgents.Count > 0)
		{
			foreach (Agent addBloodToAgent in _addBloodToAgents)
			{
				(sbyte, sbyte) randomPairOfRealBloodBurstBoneIndices = addBloodToAgent.GetRandomPairOfRealBloodBurstBoneIndices();
				if (randomPairOfRealBloodBurstBoneIndices.Item1 != -1 && randomPairOfRealBloodBurstBoneIndices.Item2 != -1)
				{
					addBloodToAgent.CreateBloodBurstAtLimb(randomPairOfRealBloodBurstBoneIndices.Item1, 0.1f + MBRandom.RandomFloat * 0.1f);
					addBloodToAgent.CreateBloodBurstAtLimb(randomPairOfRealBloodBurstBoneIndices.Item2, 0.2f + MBRandom.RandomFloat * 0.2f);
				}
			}
			_addBloodToAgents.Clear();
		}
		if (!_conversationStarted)
		{
			if (!IsReadyForConversation)
			{
				return;
			}
			InitializeAfterCreation(_curConversationPartnerAgent, OtherSideConversationData.Party);
			_conversationStarted = true;
		}
		if (((MissionBehavior)this).Mission.InputManager.IsGameKeyPressed(4))
		{
			Campaign.Current.ConversationManager.EndConversation();
		}
		if (_isNaval && _curConversationPartnerAgent != null && _agentHangPointShort != (GameEntity)null && _navalConversationState != NavalConversationCameraState.SameShip)
		{
			if (ActionIndexCache.act_conversation_naval_start == _curConversationPartnerAgent.GetCurrentAction(0) || ActionIndexCache.act_conversation_naval_idle_loop == _curConversationPartnerAgent.GetCurrentAction(0))
			{
				MatrixFrame globalFrame = ((_otherPartyHeightMultiplier >= 1.76f) ? _agentHangPointTall : _agentHangPointShort).GetGlobalFrame();
				Vec3 val = ((_otherPartyHeightMultiplier >= 1.76f) ? _agentHangPointSecondTall : _agentHangPointSecondShort).GetGlobalFrame().origin - globalFrame.origin;
				((Vec3)(ref val)).Normalize();
				Vec3 val2 = globalFrame.rotation.f;
				((Vec3)(ref val2)).Normalize();
				Vec3 val3 = Vec3.CrossProduct(val2, val);
				((Vec3)(ref val3)).Normalize();
				val2 = Vec3.CrossProduct(val, val3);
				((Vec3)(ref val2)).Normalize();
				globalFrame.rotation.f = val2;
				globalFrame.rotation.u = -val;
				globalFrame.rotation.s = -val3;
				Agent curConversationPartnerAgent = _curConversationPartnerAgent;
				MatrixFrame identity = MatrixFrame.Identity;
				curConversationPartnerAgent.SetHandInverseKinematicsFrame(ref globalFrame, ref identity);
			}
			else
			{
				_curConversationPartnerAgent.ClearHandInverseKinematics();
			}
		}
		if (IsMultiAgentConversation && (ActionIndexCache.act_conversation_naval_start == _curConversationPartnerAgent.GetCurrentAction(0) || ActionIndexCache.act_conversation_naval_idle_loop == _curConversationPartnerAgent.GetCurrentAction(0)))
		{
			_curConversationPartnerAgent.SetCurrentActionProgress(0, 1f);
			_curConversationPartnerAgent.SetActionChannel(0, ref ActionIndexCache.act_conversation_normal_loop, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		}
		if (!Campaign.Current.ConversationManager.IsConversationInProgress)
		{
			((MissionBehavior)this).Mission.EndMission();
		}
	}

	private void SpawnBodyguards(bool isDefenderSide)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		int num = 2;
		ConversationCharacterData otherSideConversationData = OtherSideConversationData;
		if (otherSideConversationData.Party == null)
		{
			return;
		}
		TroopRoster memberRoster = otherSideConversationData.Party.MemberRoster;
		int num2 = memberRoster.TotalManCount;
		if (memberRoster.Contains(CharacterObject.PlayerCharacter))
		{
			num2--;
		}
		if (num2 < num + 1)
		{
			return;
		}
		List<CharacterObject> list = new List<CharacterObject>();
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)memberRoster.GetTroopRoster())
		{
			if (((BasicCharacterObject)item.Character).IsHero && otherSideConversationData.Character != item.Character && !list.Contains(item.Character) && item.Character.HeroObject.IsWounded && !((BasicCharacterObject)item.Character).IsPlayerCharacter)
			{
				list.Add(item.Character);
			}
		}
		while (list.Count < num)
		{
			foreach (TroopRosterElement item2 in ((IEnumerable<TroopRosterElement>)memberRoster.GetTroopRoster()).OrderByDescending((TroopRosterElement k) => ((BasicCharacterObject)k.Character).Level))
			{
				if ((!((BasicCharacterObject)otherSideConversationData.Character).IsHero || otherSideConversationData.Character != item2.Character) && !((BasicCharacterObject)item2.Character).IsPlayerCharacter)
				{
					list.Add(item2.Character);
				}
				if (list.Count == num)
				{
					break;
				}
			}
		}
		List<ActionIndexCache> list2 = new List<ActionIndexCache>
		{
			ActionIndexCache.act_stand_1,
			ActionIndexCache.act_inventory_idle_start,
			ActionIndexCache.act_inventory_idle,
			ActionIndexCache.act_conversation_normal_loop,
			ActionIndexCache.act_conversation_warrior_loop,
			ActionIndexCache.act_conversation_hip_loop,
			ActionIndexCache.act_conversation_closed_loop,
			ActionIndexCache.act_conversation_demure_loop
		};
		for (int num3 = 0; num3 < num; num3++)
		{
			int index = new Random().Next(0, list.Count);
			int index2 = MBRandom.RandomInt(0, list2.Count);
			SpawnCharacter(list[index], otherSideConversationData, GetBodyguardSpawnFrame(((BasicCharacterObject)list[index]).HasMount(), isDefenderSide), list2[index2]);
			list2.RemoveAt(index2);
			list.RemoveAt(index);
		}
	}

	private void SpawnCharacter(CharacterObject character, ConversationCharacterData characterData, MatrixFrame initialFrame, in ActionIndexCache conversationAction)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)character).Race, "_settlement");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)character).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)character, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerTeam).Monster(monsterWithSuffix)
			.InitialPosition(ref initialFrame.origin);
		Vec2 val = ((Vec3)(ref initialFrame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		AgentBuildData val2 = obj.InitialDirection(ref val).NoHorses(((BasicCharacterObject)character).HasMount()).NoWeapons(characterData.NoWeapon)
			.CivilianEquipment((character == CharacterObject.PlayerCharacter) ? _isCivilianEquipmentRequiredForLeader : _isCivilianEquipmentRequiredForBodyGuards)
			.SetPrepareImmediately();
		PartyBase party = characterData.Party;
		object obj2;
		if (party == null)
		{
			obj2 = null;
		}
		else
		{
			Hero leaderHero = party.LeaderHero;
			obj2 = ((leaderHero != null) ? leaderHero.ClanBanner : null);
		}
		if (obj2 != null)
		{
			val2.Banner(characterData.Party.LeaderHero.ClanBanner);
		}
		else if (characterData.Party != null)
		{
			PartyBase party2 = characterData.Party;
			if (((party2 != null) ? party2.MapFaction : null) != null)
			{
				val2.Banner(characterData.Party.MapFaction.Banner);
			}
		}
		if (characterData.Party != null)
		{
			val2.ClothingColor1(characterData.Party.MapFaction.Color).ClothingColor2(characterData.Party.MapFaction.Color2);
		}
		if (characterData.Character == CharacterObject.PlayerCharacter)
		{
			val2.Controller((AgentControllerType)2);
		}
		Agent val3 = ((MissionBehavior)this).Mission.SpawnAgent(val2, false);
		val3.AgentVisuals.SetAgentLodZeroOrMax(true);
		val3.SetLookAgent(Agent.Main);
		AnimationSystemData val4 = MonsterExtensions.FillAnimationSystemData(val2.AgentMonster, MBGlobals.GetActionSetWithSuffix(val2.AgentMonster, val2.AgentIsFemale, "_poses"), ((BasicCharacterObject)character).GetStepSize(), false);
		val3.SetActionSet(ref val4);
		if (characterData.Character == CharacterObject.PlayerCharacter)
		{
			val3.AgentVisuals.GetSkeleton().TickAnimationsAndForceUpdate(0.1f, initialFrame, true);
		}
		if (characterData.SpawnedAfterFight)
		{
			_addBloodToAgents.Add(val3);
		}
		else if (val3.MountAgent == null)
		{
			val3.SetActionChannel(0, ref conversationAction, false, (AnimFlags)0, 0f, 1f, 0f, 0.4f, MBRandom.RandomFloat * 0.8f, false, -0.2f, 0, true);
		}
	}

	private MatrixFrame GetBodyguardSpawnFrame(bool spawnWithHorse, bool isDefenderSide)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame result = MatrixFrame.Identity;
		foreach (GameEntity child in _conversationSet.GetChildren())
		{
			if (!isDefenderSide)
			{
				if (spawnWithHorse && child.HasTag("player_bodyguard_cavalry_spawn") && !_usedSpawnPoints.Contains(child))
				{
					_usedSpawnPoints.Add(child);
					result = child.GetGlobalFrame();
					break;
				}
				if (child.HasTag("player_bodyguard_infantry_spawn") && !_usedSpawnPoints.Contains(child))
				{
					_usedSpawnPoints.Add(child);
					result = child.GetGlobalFrame();
					break;
				}
			}
			else
			{
				if (spawnWithHorse && child.HasTag("opponent_bodyguard_cavalry_spawn") && !_usedSpawnPoints.Contains(child))
				{
					_usedSpawnPoints.Add(child);
					result = child.GetGlobalFrame();
					break;
				}
				if (child.HasTag("opponent_bodyguard_infantry_spawn") && !_usedSpawnPoints.Contains(child))
				{
					_usedSpawnPoints.Add(child);
					result = child.GetGlobalFrame();
					break;
				}
			}
		}
		((Mat3)(ref result.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		return result;
	}

	protected override void OnEndMission()
	{
		_conversationSet = null;
		((MissionBehavior)this).Mission.CameraIsFirstPerson = _realCameraController;
	}

	private string GetNavalConversationCameraTag(PartyBase encounteredParty)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		string result;
		if (encounteredParty == null || encounteredParty == PartyBase.MainParty)
		{
			result = "custom_camera_same_ship";
			_navalConversationState = NavalConversationCameraState.SameShip;
		}
		else
		{
			ShipType val = (ShipType)((((List<Ship>)(object)PartyBase.MainParty.Ships).Count <= 0) ? 1 : ((int)PartyBase.MainParty.FlagShip.ShipHull.Type));
			ShipType val2 = (Extensions.IsEmpty<Ship>((IEnumerable<Ship>)encounteredParty.Ships) ? val : encounteredParty.FlagShip.ShipHull.Type);
			if (val < val2)
			{
				result = "custom_camera_lookup";
				_navalConversationState = NavalConversationCameraState.LookUp;
			}
			else if (val > val2)
			{
				result = "custom_camera_lookdown";
				_navalConversationState = NavalConversationCameraState.LookDown;
			}
			else
			{
				result = "custom_camera_level";
				_navalConversationState = NavalConversationCameraState.Level;
			}
		}
		return result;
	}
}
