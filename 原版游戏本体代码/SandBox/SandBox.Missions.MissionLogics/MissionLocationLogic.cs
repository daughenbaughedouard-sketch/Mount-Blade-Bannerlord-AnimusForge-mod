using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.AgentBehaviors;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Objects;

namespace SandBox.Missions.MissionLogics;

public class MissionLocationLogic : MissionLogic
{
	private readonly Location _previousLocation;

	private readonly Location _currentLocation;

	private MissionAgentHandler _missionAgentHandler;

	private readonly string _playerSpecialSpawnTag;

	private bool _noHorsesforCharactersAccompanyingPlayer;

	public MissionLocationLogic(Location location, string specialPlayerTag = null)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		_currentLocation = location;
		_previousLocation = (((int)Campaign.Current.GameMode == 1) ? Campaign.Current.GameMenuManager.PreviousLocation : null);
		if (_previousLocation != null)
		{
			Location currentLocation = _currentLocation;
			if (currentLocation != null && !currentLocation.LocationsOfPassages.Contains(_previousLocation))
			{
				Debug.FailedAssert(string.Concat("No passage from ", _previousLocation.DoorName, " to ", _currentLocation.DoorName), "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\MissionLocationLogic.cs", ".ctor", 36);
				_previousLocation = null;
			}
		}
		_playerSpecialSpawnTag = specialPlayerTag;
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
		CampaignEvents.BeforePlayerAgentSpawnEvent.AddNonSerializedListener((object)this, (ReferenceAction<MatrixFrame>)OnBeforePlayerAgentSpawn);
		CampaignEvents.PlayerAgentSpawned.AddNonSerializedListener((object)this, (Action)OnPlayerAgentSpawned);
	}

	public override void EarlyStart()
	{
		_missionAgentHandler = Mission.Current.GetMissionBehavior<MissionAgentHandler>();
	}

	private void OnPlayerAgentSpawned()
	{
		SpawnCharactersAccompanyingPlayer(_noHorsesforCharactersAccompanyingPlayer);
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		if (!affectedAgent.IsHuman || ((int)agentState != 4 && (int)agentState != 3))
		{
			return;
		}
		LocationCharacter locationCharacter = CampaignMission.Current.Location.GetLocationCharacter(affectedAgent.Origin);
		if (locationCharacter != null)
		{
			CampaignMission.Current.Location.RemoveLocationCharacter(locationCharacter);
			if (PlayerEncounter.LocationEncounter.GetAccompanyingCharacter(locationCharacter) != null && (int)affectedAgent.State == 4)
			{
				PlayerEncounter.LocationEncounter.RemoveAccompanyingCharacter(locationCharacter);
			}
		}
	}

	public override void OnRemoveBehavior()
	{
		foreach (Location listOfLocation in LocationComplex.Current.GetListOfLocations())
		{
			if (listOfLocation.StringId == "center" || listOfLocation.StringId == "village_center" || listOfLocation.StringId == "lordshall" || listOfLocation.StringId == "prison" || listOfLocation.StringId == "tavern" || listOfLocation.StringId == "alley" || listOfLocation.StringId == "port")
			{
				listOfLocation.RemoveAllCharacters((Predicate<LocationCharacter>)((LocationCharacter x) => !((BasicCharacterObject)x.Character).IsHero));
			}
		}
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
		((MissionBehavior)this).OnRemoveBehavior();
	}

	private void OnBeforePlayerAgentSpawn(ref MatrixFrame spawnPointFrame)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (int)Campaign.Current.GameMode == 1 && PlayerEncounter.IsActive && (Settlement.CurrentSettlement.IsTown || Settlement.CurrentSettlement.IsCastle) && !Campaign.Current.IsNight && CampaignMission.Current.Location.StringId == "center" && !PlayerEncounter.LocationEncounter.IsInsideOfASettlement;
		if (!string.IsNullOrEmpty(_playerSpecialSpawnTag))
		{
			WeakGameEntity val = WeakGameEntity.Invalid;
			UsableMachine val2 = _missionAgentHandler?.GetAllUsablePointsWithTag(_playerSpecialSpawnTag).FirstOrDefault();
			if (val2 != null)
			{
				StandingPoint? obj = ((IEnumerable<StandingPoint>)val2.StandingPoints).FirstOrDefault();
				val = ((obj != null) ? ((ScriptComponentBehavior)obj).GameEntity : WeakGameEntity.Invalid);
			}
			if (!((WeakGameEntity)(ref val)).IsValid)
			{
				GameEntity obj2 = Mission.Current.Scene.FindEntityWithTag(_playerSpecialSpawnTag);
				val = ((obj2 != null) ? obj2.WeakEntity : WeakGameEntity.Invalid);
			}
			if (((WeakGameEntity)(ref val)).IsValid)
			{
				spawnPointFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
				((Mat3)(ref spawnPointFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			}
		}
		else if (CampaignMission.Current.Location.StringId == "arena")
		{
			GameEntity val3 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_player_near_arena_master");
			if (val3 != (GameEntity)null)
			{
				spawnPointFrame = val3.GetGlobalFrame();
				((Mat3)(ref spawnPointFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			}
		}
		else if (_previousLocation != null)
		{
			spawnPointFrame = GetSpawnFrameOfPassage(_previousLocation);
			((Mat3)(ref spawnPointFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			_noHorsesforCharactersAccompanyingPlayer = true;
		}
		else if (flag)
		{
			GameEntity val4 = Mission.Current.Scene.FindEntityWithTag("spawnpoint_player_outside");
			if (val4 != (GameEntity)null)
			{
				spawnPointFrame = val4.GetGlobalFrame();
				((Mat3)(ref spawnPointFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			}
		}
		if (PlayerEncounter.LocationEncounter is TownEncounter)
		{
			PlayerEncounter.LocationEncounter.IsInsideOfASettlement = true;
		}
		if (PlayerEncounter.LocationEncounter.Settlement.IsTown || PlayerEncounter.LocationEncounter.Settlement.IsCastle)
		{
			_noHorsesforCharactersAccompanyingPlayer = true;
		}
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unUsedPoints)
	{
		IEnumerable<LocationCharacter> characterList = CampaignMission.Current.Location.GetCharacterList();
		if (!PlayerEncounter.LocationEncounter.Settlement.IsTown || CampaignMission.Current.Location != LocationComplex.Current.GetLocationWithId("center"))
		{
			return;
		}
		foreach (LocationCharacter character in LocationComplex.Current.GetLocationWithId("alley").GetCharacterList())
		{
			characterList.Append(character);
		}
	}

	public override void OnCreated()
	{
		if (_currentLocation != null)
		{
			CampaignMission.Current.Location = _currentLocation;
		}
	}

	public void SpawnCharactersAccompanyingPlayer(bool noHorse)
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		bool flag = PlayerEncounter.LocationEncounter.CharactersAccompanyingPlayer.Any((AccompanyingCharacter c) => c.IsFollowingPlayerAtMissionStart);
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("navigation_mesh_deactivator");
		foreach (AccompanyingCharacter item in PlayerEncounter.LocationEncounter.CharactersAccompanyingPlayer)
		{
			bool flag2 = ((BasicCharacterObject)item.LocationCharacter.Character).IsHero && item.LocationCharacter.Character.HeroObject.IsWounded;
			if ((!_currentLocation.GetCharacterList().Contains(item.LocationCharacter) && flag2) || !item.CanEnterLocation(_currentLocation))
			{
				continue;
			}
			_currentLocation.AddCharacter(item.LocationCharacter);
			if (item.IsFollowingPlayerAtMissionStart || (!flag && num == 0))
			{
				WorldFrame worldFrame = ((MissionBehavior)this).Mission.MainAgent.GetWorldFrame();
				ref WorldPosition origin = ref worldFrame.Origin;
				Vec3 val2 = ((MissionBehavior)this).Mission.GetRandomPositionAroundPoint(((WorldPosition)(ref worldFrame.Origin)).GetNavMeshVec3(), 0.5f, 2f, false);
				((WorldPosition)(ref origin)).SetVec2(((Vec3)(ref val2)).AsVec2);
				Agent val3 = _missionAgentHandler.SpawnWanderingAgentWithInitialFrame(item.LocationCharacter, ((WorldFrame)(ref worldFrame)).ToGroundMatrixFrame(), WeakGameEntity.Invalid, noHorse);
				if (val != (GameEntity)null)
				{
					int disableFaceWithId = val.GetFirstScriptOfType<NavigationMeshDeactivator>().DisableFaceWithId;
					if (disableFaceWithId != -1)
					{
						val3.SetAgentExcludeStateForFaceGroupId(disableFaceWithId, false);
					}
				}
				int num2 = 0;
				while (true)
				{
					val2 = ((MissionBehavior)this).Mission.MainAgent.Position;
					Vec2 asVec = ((Vec3)(ref val2)).AsVec2;
					if (val3.CanMoveDirectlyToPosition(ref asVec) || num2 >= 50)
					{
						break;
					}
					ref WorldPosition origin2 = ref worldFrame.Origin;
					val2 = ((MissionBehavior)this).Mission.GetRandomPositionAroundPoint(((WorldPosition)(ref worldFrame.Origin)).GetNavMeshVec3(), 0.5f, 4f, false);
					((WorldPosition)(ref origin2)).SetVec2(((Vec3)(ref val2)).AsVec2);
					val3.TeleportToPosition(((WorldFrame)(ref worldFrame)).ToGroundMatrixFrame().origin);
					num2++;
				}
				val3.SetTeam(((MissionBehavior)this).Mission.PlayerTeam, true);
				num++;
			}
			else
			{
				_missionAgentHandler.SpawnWanderingAgent(item.LocationCharacter).SetTeam(((MissionBehavior)this).Mission.PlayerTeam, true);
			}
			foreach (Agent item2 in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				LocationCharacter locationCharacter = CampaignMission.Current.Location.GetLocationCharacter(item2.Origin);
				AccompanyingCharacter accompanyingCharacter = PlayerEncounter.LocationEncounter.GetAccompanyingCharacter(locationCharacter);
				if (item2.GetComponent<CampaignAgentComponent>().AgentNavigator != null && accompanyingCharacter != null)
				{
					DailyBehaviorGroup behaviorGroup = item2.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
					if (item.IsFollowingPlayerAtMissionStart)
					{
						FollowAgentBehavior obj = behaviorGroup.GetBehavior<FollowAgentBehavior>() ?? behaviorGroup.AddBehavior<FollowAgentBehavior>();
						behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
						obj.SetTargetAgent(Agent.Main);
					}
					else
					{
						behaviorGroup.Behaviors.Clear();
					}
				}
			}
		}
	}

	public MatrixFrame GetSpawnFrameOfPassage(Location location)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame result = MatrixFrame.Identity;
		UsableMachine val = ((IEnumerable<UsableMachine>)_missionAgentHandler.TownPassageProps).FirstOrDefault((Func<UsableMachine, bool>)((UsableMachine x) => ((Passage)(object)x).ToLocation == location)) ?? ((IEnumerable<UsableMachine>)_missionAgentHandler.DisabledPassages).FirstOrDefault((Func<UsableMachine, bool>)((UsableMachine x) => ((Passage)(object)x).ToLocation == location));
		if (val != null)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)val.PilotStandingPoint).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			globalFrame.origin.z = ((MissionBehavior)this).Mission.Scene.GetGroundHeightAtPosition(globalFrame.origin, (BodyFlags)544321929);
			((Mat3)(ref globalFrame.rotation)).RotateAboutUp(MathF.PI);
			result = globalFrame;
		}
		return result;
	}
}
