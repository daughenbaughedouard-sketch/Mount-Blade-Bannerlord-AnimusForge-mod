using System.Collections.Generic;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace TaleWorlds.MountAndBlade;

public class DuelSpawningBehavior : SpawningBehaviorBase
{
	public override void Initialize(SpawnComponent spawnComponent)
	{
		((SpawningBehaviorBase)this).Initialize(spawnComponent);
		((SpawningBehaviorBase)this).OnPeerSpawnedFromVisuals += OnPeerSpawned;
		if (base.GameMode.WarmupComponent == null)
		{
			((SpawningBehaviorBase)this).RequestStartSpawnSession();
		}
	}

	public override void Clear()
	{
		((SpawningBehaviorBase)this).Clear();
		((SpawningBehaviorBase)this).OnPeerSpawnedFromVisuals -= OnPeerSpawned;
	}

	public override void OnTick(float dt)
	{
		if (base.IsSpawningEnabled && base.SpawnCheckTimer.Check(Mission.Current.CurrentTime))
		{
			((SpawningBehaviorBase)this).SpawnAgents();
		}
		((SpawningBehaviorBase)this).OnTick(dt);
	}

	protected override void SpawnAgents()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
		{
			if (!networkPeer.IsSynchronized)
			{
				continue;
			}
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(networkPeer);
			if (!(component.Representative is DuelMissionRepresentative) || !networkPeer.IsSynchronized || component.ControlledAgent != null || component.HasSpawnedAgentVisuals || component.Team == null || component.Team == ((SpawningBehaviorBase)this).Mission.SpectatorTeam || !component.TeamInitialPerkInfoReady || component.Culture == null || !component.SpawnTimer.Check(Mission.Current.CurrentTime))
			{
				continue;
			}
			MPHeroClass mPHeroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(component, false);
			if (mPHeroClassForPeer == null)
			{
				if (component.SelectedTroopIndex != 0)
				{
					component.SelectedTroopIndex = 0;
					GameNetwork.BeginBroadcastModuleEvent();
					GameNetwork.WriteMessage((GameNetworkMessage)new UpdateSelectedTroopIndex(networkPeer, 0));
					GameNetwork.EndBroadcastModuleEvent((EventBroadcastFlags)64, networkPeer);
				}
				continue;
			}
			BasicCharacterObject heroCharacter = mPHeroClassForPeer.HeroCharacter;
			Equipment val = heroCharacter.Equipment.Clone(false);
			MPOnSpawnPerkHandler onSpawnPerkHandler = MPPerkObject.GetOnSpawnPerkHandler(component);
			IEnumerable<(EquipmentIndex, EquipmentElement)> enumerable = ((onSpawnPerkHandler != null) ? onSpawnPerkHandler.GetAlternativeEquipments(true) : null);
			if (enumerable != null)
			{
				foreach (var item in enumerable)
				{
					val[item.Item1] = item.Item2;
				}
			}
			AgentBuildData val2 = new AgentBuildData(heroCharacter).MissionPeer(component).Equipment(val).Team(component.Team)
				.TroopOrigin((IAgentOriginBase)new BasicBattleAgentOrigin(heroCharacter))
				.IsFemale(((PeerComponent)component).Peer.IsFemale)
				.BodyProperties(((SpawningBehaviorBase)this).GetBodyProperties(component, component.Culture))
				.VisualsIndex(0)
				.ClothingColor1(component.Culture.Color)
				.ClothingColor2(component.Culture.Color2);
			if (base.GameMode.ShouldSpawnVisualsForServer(networkPeer))
			{
				((SpawningBehaviorBase)this).AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(component, val2, component.SelectedTroopIndex, false, 0);
				if (val2.AgentVisualsIndex == 0)
				{
					component.HasSpawnedAgentVisuals = true;
					component.EquipmentUpdatingExpired = false;
				}
			}
			base.GameMode.HandleAgentVisualSpawning(networkPeer, val2, 0, true);
		}
	}

	public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
	{
		return true;
	}

	protected override bool IsRoundInProgress()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		return (int)Mission.Current.CurrentState == 2;
	}

	private void OnPeerSpawned(MissionPeer peer)
	{
		_ = peer.Representative;
	}
}
