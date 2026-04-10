using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.Source.Objects;
using TaleWorlds.ObjectSystem;

namespace SandBox;

public static class SandBoxHelpers
{
	public static class MissionHelper
	{
		public static void FollowAgent(Agent agent, Agent target)
		{
			if (agent != null && target != null && agent.IsActive() && target.IsActive())
			{
				AgentBehaviorGroup activeBehaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetActiveBehaviorGroup();
				if (activeBehaviorGroup != null)
				{
					FollowAgentBehavior followAgentBehavior = activeBehaviorGroup.GetBehavior<FollowAgentBehavior>();
					if (followAgentBehavior == null)
					{
						followAgentBehavior = activeBehaviorGroup.AddBehavior<FollowAgentBehavior>();
					}
					activeBehaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
					followAgentBehavior.SetTargetAgent(target);
				}
			}
			else
			{
				Debug.FailedAssert("Cant follow agent", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\SandboxHelpers.cs", "FollowAgent", 45);
			}
		}

		public static void UnfollowAgent(Agent agent)
		{
			if (agent != null && agent.IsActive())
			{
				AgentBehaviorGroup activeBehaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetActiveBehaviorGroup();
				if (activeBehaviorGroup != null && activeBehaviorGroup.GetBehavior<FollowAgentBehavior>() != null)
				{
					activeBehaviorGroup.RemoveBehavior<FollowAgentBehavior>();
				}
			}
			else
			{
				Debug.FailedAssert("Cant unfollow agent", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\SandboxHelpers.cs", "UnfollowAgent", 66);
			}
		}

		public static void FadeOutAgents(IEnumerable<Agent> agents, bool hideInstantly, bool hideMount)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Invalid comparison between Unknown and I4
			if (agents == null)
			{
				return;
			}
			Agent[] array = agents.ToArray();
			Agent[] array2 = array;
			foreach (Agent val in array2)
			{
				if (!val.IsMount)
				{
					val.FadeOut(hideInstantly, hideMount);
				}
			}
			array2 = array;
			foreach (Agent val2 in array2)
			{
				if ((int)val2.State != 2)
				{
					val2.FadeOut(hideInstantly, hideMount);
				}
			}
		}

		public static void DisableGenericMissionEventScript(string triggeringObjectTag, GenericMissionEvent missionEvent)
		{
			foreach (ScriptComponentBehavior scriptComponent in Mission.Current.Scene.FindEntityWithTag(triggeringObjectTag).GetScriptComponents())
			{
				GenericMissionEventScript val;
				if ((val = (GenericMissionEventScript)(object)((scriptComponent is GenericMissionEventScript) ? scriptComponent : null)) != null && val.EventId.Equals(missionEvent.EventId) && val.Parameter.Equals(missionEvent.Parameter))
				{
					val.IsDisabled = true;
				}
			}
		}

		public static void SpawnPlayer(bool civilianEquipment = false, bool noHorses = false, bool noWeapon = false, bool wieldInitialWeapons = false, string spawnTag = "")
		{
			GameEntity val = null;
			val = (string.IsNullOrEmpty(spawnTag) ? Mission.Current.Scene.FindEntityWithTag("spawnpoint_player") : Mission.Current.Scene.FindEntityWithTag(spawnTag));
			SpawnPlayer(val, civilianEquipment, noHorses, noWeapon, wieldInitialWeapons);
		}

		public static void SpawnPlayer(GameEntity spawnPosition, bool civilianEquipment = false, bool noHorses = false, bool noWeapon = false, bool wieldInitialWeapons = false)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Invalid comparison between Unknown and I4
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Expected O, but got Unknown
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Invalid comparison between Unknown and I4
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Expected O, but got Unknown
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			if ((int)Campaign.Current.GameMode != 1)
			{
				civilianEquipment = false;
			}
			MatrixFrame val = MatrixFrame.Identity;
			if (spawnPosition != (GameEntity)null)
			{
				val = spawnPosition.GetGlobalFrame();
				((Mat3)(ref val.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			}
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnBeforePlayerAgentSpawn(ref val);
			CharacterObject playerCharacter = CharacterObject.PlayerCharacter;
			AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)playerCharacter).Team(Mission.Current.PlayerTeam).InitialPosition(ref val.origin);
			Vec2 val2 = ((Vec3)(ref val.rotation.f)).AsVec2;
			val2 = ((Vec2)(ref val2)).Normalized();
			AgentBuildData obj2 = obj.InitialDirection(ref val2).CivilianEquipment(civilianEquipment).NoHorses(noHorses)
				.NoWeapons(noWeapon)
				.ClothingColor1(Mission.Current.PlayerTeam.Color)
				.ClothingColor2(Mission.Current.PlayerTeam.Color2)
				.TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, playerCharacter, -1, default(UniqueTroopDescriptor), false, false));
			EquipmentElement val3 = ((BasicCharacterObject)playerCharacter).Equipment[(EquipmentIndex)10];
			AgentBuildData val4 = obj2.MountKey(MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val3)).Item, ((BasicCharacterObject)playerCharacter).GetMountKeySeed())).Controller((AgentControllerType)2);
			Debug.Print($"Spawn position: {val.origin}", 0, (DebugColor)12, 17592186044416uL);
			Hero heroObject = playerCharacter.HeroObject;
			if (((heroObject != null) ? heroObject.ClanBanner : null) != null)
			{
				val4.Banner(playerCharacter.HeroObject.ClanBanner);
			}
			if ((int)Campaign.Current.GameMode != 1)
			{
				val4.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)CharacterObject.PlayerCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
			}
			if (Campaign.Current.IsMainHeroDisguised)
			{
				MBEquipmentRoster val5 = MBObjectManager.Instance.GetObject<MBEquipmentRoster>("npc_disguised_hero_equipment_template");
				val4.Equipment(val5.DefaultEquipment);
			}
			Agent val6 = Mission.Current.SpawnAgent(val4, false);
			if (wieldInitialWeapons)
			{
				val6.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
			}
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnPlayerAgentSpawned();
			if (spawnPosition != (GameEntity)null)
			{
				string[] tags = spawnPosition.Tags;
				foreach (string text in tags)
				{
					val6.AgentVisuals.GetEntity().AddTag(text);
				}
			}
			for (int j = 0; j < 3; j++)
			{
				Agent.Main.AgentVisuals.GetSkeleton().TickAnimations(0.1f, Agent.Main.AgentVisuals.GetGlobalFrame(), true);
			}
		}

		public static List<Agent> SpawnHorses()
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			List<Agent> list = new List<Agent>();
			ItemRosterElement val2 = default(ItemRosterElement);
			foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("sp_horse"))
			{
				MatrixFrame globalFrame = item.GetGlobalFrame();
				string text = item.Tags[1];
				ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>(text);
				((ItemRosterElement)(ref val2))._002Ector(val, 1, (ItemModifier)null);
				ItemRosterElement val3 = default(ItemRosterElement);
				if (val.HasHorseComponent)
				{
					((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
					Mission current2 = Mission.Current;
					ItemRosterElement val4 = val2;
					ref Vec3 origin = ref globalFrame.origin;
					Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
					Agent val5 = current2.SpawnMonster(val4, val3, ref origin, ref asVec, -1);
					AnimalSpawnSettings.CheckAndSetAnimalAgentFlags(item, val5);
					SimulateAnimalAnimations(val5);
					list.Add(val5);
				}
			}
			return list;
		}

		public static void SpawnSheeps()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			ItemRosterElement val = default(ItemRosterElement);
			foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("sp_sheep"))
			{
				MatrixFrame globalFrame = item.GetGlobalFrame();
				((ItemRosterElement)(ref val))._002Ector(Game.Current.ObjectManager.GetObject<ItemObject>("sheep"), 0, (ItemModifier)null);
				((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
				Mission current2 = Mission.Current;
				ItemRosterElement val2 = val;
				ref Vec3 origin = ref globalFrame.origin;
				Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				Agent val3 = current2.SpawnMonster(val2, default(ItemRosterElement), ref origin, ref asVec, -1);
				GameEntity val4 = Mission.Current.Scene.FindEntityWithTag("navigation_mesh_deactivator");
				if (val4 != (GameEntity)null)
				{
					NavigationMeshDeactivator firstScriptOfType = val4.GetFirstScriptOfType<NavigationMeshDeactivator>();
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithId, true);
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithIdForAnimals, true);
				}
				AnimalSpawnSettings.CheckAndSetAnimalAgentFlags(item, val3);
				SimulateAnimalAnimations(val3);
			}
		}

		public static void SpawnCows()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			ItemRosterElement val = default(ItemRosterElement);
			foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("sp_cow"))
			{
				MatrixFrame globalFrame = item.GetGlobalFrame();
				((ItemRosterElement)(ref val))._002Ector(Game.Current.ObjectManager.GetObject<ItemObject>("cow"), 0, (ItemModifier)null);
				((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
				Mission current2 = Mission.Current;
				ItemRosterElement val2 = val;
				ref Vec3 origin = ref globalFrame.origin;
				Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				Agent val3 = current2.SpawnMonster(val2, default(ItemRosterElement), ref origin, ref asVec, -1);
				GameEntity val4 = Mission.Current.Scene.FindEntityWithTag("navigation_mesh_deactivator");
				if (val4 != (GameEntity)null)
				{
					NavigationMeshDeactivator firstScriptOfType = val4.GetFirstScriptOfType<NavigationMeshDeactivator>();
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithId, true);
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithIdForAnimals, true);
				}
				AnimalSpawnSettings.CheckAndSetAnimalAgentFlags(item, val3);
				SimulateAnimalAnimations(val3);
			}
		}

		public static void SpawnGeese()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			ItemRosterElement val = default(ItemRosterElement);
			foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("sp_goose"))
			{
				MatrixFrame globalFrame = item.GetGlobalFrame();
				((ItemRosterElement)(ref val))._002Ector(Game.Current.ObjectManager.GetObject<ItemObject>("goose"), 0, (ItemModifier)null);
				((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
				Mission current2 = Mission.Current;
				ItemRosterElement val2 = val;
				ref Vec3 origin = ref globalFrame.origin;
				Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				Agent val3 = current2.SpawnMonster(val2, default(ItemRosterElement), ref origin, ref asVec, -1);
				GameEntity val4 = Mission.Current.Scene.FindEntityWithTag("navigation_mesh_deactivator");
				if (val4 != (GameEntity)null)
				{
					NavigationMeshDeactivator firstScriptOfType = val4.GetFirstScriptOfType<NavigationMeshDeactivator>();
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithId, true);
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithIdForAnimals, true);
				}
				AnimalSpawnSettings.CheckAndSetAnimalAgentFlags(item, val3);
				SimulateAnimalAnimations(val3);
			}
		}

		public static void SpawnChicken()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			ItemRosterElement val = default(ItemRosterElement);
			foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("sp_chicken"))
			{
				MatrixFrame globalFrame = item.GetGlobalFrame();
				((ItemRosterElement)(ref val))._002Ector(Game.Current.ObjectManager.GetObject<ItemObject>("chicken"), 0, (ItemModifier)null);
				((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
				Mission current2 = Mission.Current;
				ItemRosterElement val2 = val;
				ref Vec3 origin = ref globalFrame.origin;
				Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				Agent val3 = current2.SpawnMonster(val2, default(ItemRosterElement), ref origin, ref asVec, -1);
				GameEntity val4 = Mission.Current.Scene.FindEntityWithTag("navigation_mesh_deactivator");
				if (val4 != (GameEntity)null)
				{
					NavigationMeshDeactivator firstScriptOfType = val4.GetFirstScriptOfType<NavigationMeshDeactivator>();
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithId, true);
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithIdForAnimals, true);
				}
				AnimalSpawnSettings.CheckAndSetAnimalAgentFlags(item, val3);
				SimulateAnimalAnimations(val3);
			}
		}

		public static void SpawnHogs()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			ItemRosterElement val = default(ItemRosterElement);
			foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("sp_hog"))
			{
				MatrixFrame globalFrame = item.GetGlobalFrame();
				((ItemRosterElement)(ref val))._002Ector(Game.Current.ObjectManager.GetObject<ItemObject>("hog"), 0, (ItemModifier)null);
				((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
				Mission current2 = Mission.Current;
				ItemRosterElement val2 = val;
				ref Vec3 origin = ref globalFrame.origin;
				Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				Agent val3 = current2.SpawnMonster(val2, default(ItemRosterElement), ref origin, ref asVec, -1);
				GameEntity val4 = Mission.Current.Scene.FindEntityWithTag("navigation_mesh_deactivator");
				if (val4 != (GameEntity)null)
				{
					NavigationMeshDeactivator firstScriptOfType = val4.GetFirstScriptOfType<NavigationMeshDeactivator>();
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithId, true);
					val3.SetAgentExcludeStateForFaceGroupId(firstScriptOfType.DisableFaceWithIdForAnimals, true);
				}
				AnimalSpawnSettings.CheckAndSetAnimalAgentFlags(item, val3);
				SimulateAnimalAnimations(val3);
			}
		}

		private static void SimulateAnimalAnimations(Agent agent)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			int num = 10 + MBRandom.RandomInt(90);
			for (int i = 0; i < num; i++)
			{
				agent.TickActionChannels(0.1f);
				Vec3 val = agent.ComputeAnimationDisplacement(0.1f);
				if (((Vec3)(ref val)).LengthSquared > 0f)
				{
					agent.TeleportToPosition(agent.Position + val);
				}
				agent.AgentVisuals.GetSkeleton().TickAnimations(0.1f, agent.AgentVisuals.GetGlobalFrame(), true);
			}
		}
	}

	public static class MapSceneHelper
	{
		public static bool[] GetRegionMapping(PartyNavigationModel model)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			TerrainType[] obj = (TerrainType[])Enum.GetValues(typeof(TerrainType));
			bool[] array = new bool[obj.Max((TerrainType v) => (int)v) + 1];
			TerrainType[] array2 = obj;
			foreach (TerrainType val in array2)
			{
				array[val] = model.IsTerrainTypeValidForNavigationType(val, (NavigationType)1);
			}
			return array;
		}
	}
}
