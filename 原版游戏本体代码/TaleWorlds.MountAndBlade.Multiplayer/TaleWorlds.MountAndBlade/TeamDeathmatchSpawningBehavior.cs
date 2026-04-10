using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade;

public class TeamDeathmatchSpawningBehavior : SpawningBehaviorBase
{
	public override void Initialize(SpawnComponent spawnComponent)
	{
		((SpawningBehaviorBase)this).Initialize(spawnComponent);
		((SpawningBehaviorBase)this).OnAllAgentsFromPeerSpawnedFromVisuals += OnAllAgentsFromPeerSpawnedFromVisuals;
		if (base.GameMode.WarmupComponent == null)
		{
			((SpawningBehaviorBase)this).RequestStartSpawnSession();
		}
	}

	public override void Clear()
	{
		((SpawningBehaviorBase)this).Clear();
		((SpawningBehaviorBase)this).OnAllAgentsFromPeerSpawnedFromVisuals -= OnAllAgentsFromPeerSpawnedFromVisuals;
	}

	public override void OnTick(float dt)
	{
		if (base.IsSpawningEnabled && base.SpawnCheckTimer.Check(((SpawningBehaviorBase)this).Mission.CurrentTime))
		{
			((SpawningBehaviorBase)this).SpawnAgents();
		}
		((SpawningBehaviorBase)this).OnTick(dt);
	}

	protected override void SpawnAgents()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		BasicCultureObject val = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1));
		BasicCultureObject val2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
		MultiplayerBattleColors val3 = MultiplayerBattleColors.CreateWith(val, val2);
		foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
		{
			if (!networkPeer.IsSynchronized)
			{
				continue;
			}
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(networkPeer);
			if (component == null || component.ControlledAgent != null || component.HasSpawnedAgentVisuals || component.Team == null || component.Team == ((SpawningBehaviorBase)this).Mission.SpectatorTeam || !component.TeamInitialPerkInfoReady || !component.SpawnTimer.Check(((SpawningBehaviorBase)this).Mission.CurrentTime))
			{
				continue;
			}
			_ = component.Team.Side;
			_ = 1;
			MPHeroClass mPHeroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(component, false);
			if (mPHeroClassForPeer == null || mPHeroClassForPeer.TroopCasualCost > base.GameMode.GetCurrentGoldForPeer(component))
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
			Equipment val4 = heroCharacter.Equipment.Clone(false);
			MPOnSpawnPerkHandler onSpawnPerkHandler = MPPerkObject.GetOnSpawnPerkHandler(component);
			IEnumerable<(EquipmentIndex, EquipmentElement)> enumerable = ((onSpawnPerkHandler != null) ? onSpawnPerkHandler.GetAlternativeEquipments(true) : null);
			if (enumerable != null)
			{
				foreach (var item in enumerable)
				{
					val4[item.Item1] = item.Item2;
				}
			}
			MultiplayerCultureColorInfo peerColors = ((MultiplayerBattleColors)(ref val3)).GetPeerColors(component);
			AgentBuildData val5 = new AgentBuildData(heroCharacter).MissionPeer(component).Equipment(val4).Team(component.Team)
				.TroopOrigin((IAgentOriginBase)new BasicBattleAgentOrigin(heroCharacter))
				.IsFemale(((PeerComponent)component).Peer.IsFemale)
				.BodyProperties(((SpawningBehaviorBase)this).GetBodyProperties(component, (component.Culture == val) ? val : val2))
				.VisualsIndex(0)
				.ClothingColor1(peerColors.Color1Uint)
				.ClothingColor2(peerColors.Color2Uint);
			if (base.GameMode.ShouldSpawnVisualsForServer(networkPeer))
			{
				((SpawningBehaviorBase)this).AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(component, val5, component.SelectedTroopIndex, false, 0);
				if (val5.AgentVisualsIndex == 0)
				{
					component.HasSpawnedAgentVisuals = true;
					component.EquipmentUpdatingExpired = false;
				}
			}
			base.GameMode.HandleAgentVisualSpawning(networkPeer, val5, 0, true);
		}
	}

	public override bool AllowEarlyAgentVisualsDespawning(MissionPeer lobbyPeer)
	{
		return true;
	}

	public override int GetMaximumReSpawnPeriodForPeer(MissionPeer peer)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (base.GameMode.WarmupComponent != null && base.GameMode.WarmupComponent.IsInWarmup)
		{
			return 3;
		}
		if (peer.Team != null)
		{
			if ((int)peer.Team.Side == 1)
			{
				return MultiplayerOptionsExtensions.GetIntValue((OptionType)31, (MultiplayerOptionsAccessMode)1);
			}
			if ((int)peer.Team.Side == 0)
			{
				return MultiplayerOptionsExtensions.GetIntValue((OptionType)32, (MultiplayerOptionsAccessMode)1);
			}
		}
		return -1;
	}

	protected override bool IsRoundInProgress()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		return (int)Mission.Current.CurrentState == 2;
	}

	private void OnAllAgentsFromPeerSpawnedFromVisuals(MissionPeer peer)
	{
		bool flag = peer.Team == ((SpawningBehaviorBase)this).Mission.AttackerTeam;
		_ = ((SpawningBehaviorBase)this).Mission.DefenderTeam;
		MPHeroClass val = MultiplayerClassDivisions.GetMPHeroClasses(MBObjectManager.Instance.GetObject<BasicCultureObject>(flag ? MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1) : MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1))).ElementAt(peer.SelectedTroopIndex);
		base.GameMode.ChangeCurrentGoldForPeer(peer, base.GameMode.GetCurrentGoldForPeer(peer) - val.TroopCasualCost);
	}
}
