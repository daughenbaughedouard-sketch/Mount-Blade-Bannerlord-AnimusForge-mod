using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.GameComponents;
using StoryMode;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace NavalDLC.CampaignBehaviors;

public class PiratesCampaignBehavior : CampaignBehaviorBase, IPiratePatrolBehavior
{
	private class PatrolZone
	{
		[SaveableField(0)]
		public readonly CampaignVec2 Position;

		[SaveableField(1)]
		public readonly float Radius;

		public int Density { get; set; }

		public PatrolZone(CampaignVec2 position, float radius)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			Position = position;
			Radius = radius;
			Density = 0;
		}
	}

	private class PiratesCampaignBehaviorSaveDefiner : SaveableTypeDefiner
	{
		public PiratesCampaignBehaviorSaveDefiner()
			: base(2277221)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(PatrolZone), 1, (IObjectResolver)null);
		}

		protected override void DefineContainerDefinitions()
		{
			((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(Dictionary<MobileParty, PatrolZone>));
		}
	}

	private const float PirateStartGoldPerBandit = 10f;

	private const float PatrollingScore = 5f;

	private const float DefaultPatrolRadius = 20f;

	private const float WeakPirateRemovalStrengthThreshold = 0.7f;

	private Dictionary<Clan, List<PatrolZone>> _patrolZones = new Dictionary<Clan, List<PatrolZone>>();

	private Dictionary<MobileParty, PatrolZone> _assignedZones = new Dictionary<MobileParty, PatrolZone>();

	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnNewGameCreated);
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnGameLoaded);
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter, int>)OnNewGameCreatedPartialFollowUp);
		CampaignEvents.DailyTickClanEvent.AddNonSerializedListener((object)this, (Action<Clan>)DailyTickClan);
		CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)DailyTickParty);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
		CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener((object)this, (Action<MobileParty, PartyThinkParams>)AiHourlyTick);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)MapEventEnded);
		TutorialPhase instance = TutorialPhase.Instance;
		if (instance != null && !instance.IsCompleted)
		{
			StoryModeEvents.OnStoryModeTutorialEndedEvent.AddNonSerializedListener((object)this, (Action)OnTutorialEnded);
		}
	}

	private void MapEventEnded(MapEvent mapEvent)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		foreach (PartyBase involvedParty in mapEvent.InvolvedParties)
		{
			if (involvedParty.IsMobile && mapEvent.WinningSide == involvedParty.Side && IsPirateParty(involvedParty.MobileParty) && ((List<Ship>)(object)involvedParty.MobileParty.Ships).Count > Campaign.Current.Models.PartyShipLimitModel.GetIdealShipNumber(involvedParty.MobileParty))
			{
				DiscardShips(involvedParty.MobileParty);
			}
		}
	}

	private void DiscardShips(MobileParty pirateParty)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		MBList<Ship> val = Extensions.ToMBList<Ship>((IEnumerable<Ship>)((IEnumerable<Ship>)pirateParty.Ships).OrderByDescending((Ship x) => Campaign.Current.Models.PartyShipLimitModel.GetShipPriority(pirateParty, x, false)));
		int num = 0;
		int num2 = 0;
		for (int num3 = 0; num3 < ((List<Ship>)(object)val).Count; num3++)
		{
			Ship val2 = ((List<Ship>)(object)val)[num3];
			ShipType type = val2.ShipHull.Type;
			if (num2 < 2 && ((int)type != 1 || num2 == 0 || num < 2) && num < Campaign.Current.Models.PartyShipLimitModel.GetIdealShipNumber(pirateParty))
			{
				num++;
				if ((int)type == 1)
				{
					num2++;
				}
			}
			else
			{
				DestroyShipAction.ApplyByDiscard(val2);
			}
		}
	}

	private void OnTutorialEnded()
	{
		if (TutorialPhase.Instance.IsSkipped)
		{
			for (int i = 0; i < 3; i++)
			{
				foreach (Clan banditFaction in Clan.BanditFactions)
				{
					DailyTickClan(banditFaction);
				}
			}
		}
		StoryModeEvents.OnStoryModeTutorialEndedEvent.ClearListeners((object)this);
	}

	private void OnMobilePartyDestroyed(MobileParty party, PartyBase destroyerParty)
	{
		if (_assignedZones.TryGetValue(party, out var value))
		{
			UnassignPartyFromZone(value, party);
		}
	}

	private void AiHourlyTick(MobileParty mobileParty, PartyThinkParams p)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (IsPirateParty(mobileParty))
		{
			PatrolZone assignedZone = GetAssignedZone(mobileParty);
			if (assignedZone != null)
			{
				NavigationType val = (NavigationType)2;
				AIBehaviorData item = default(AIBehaviorData);
				((AIBehaviorData)(ref item))._002Ector(assignedZone.Position, (AiBehavior)13, val, false, false, false);
				(AIBehaviorData, float) tuple = (item, 5f);
				p.AddBehaviorScore(ref tuple);
			}
			else
			{
				Debug.FailedAssert("This should only be possible for cheats & mods.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\CampaignBehaviors\\PiratesCampaignBehavior.cs", "AiHourlyTick", 244);
			}
		}
	}

	private void OnNewGameCreated(CampaignGameStarter starter)
	{
		AssignPatrolZones();
	}

	private void AssignPatrolZones()
	{
		foreach (Clan banditFaction in Clan.BanditFactions)
		{
			if (IsPirateClan(banditFaction))
			{
				AssignPatrolZones(banditFaction);
			}
		}
	}

	private void OnGameLoaded(CampaignGameStarter starter)
	{
		AssignPatrolZones();
		AdjustAssignedPatrolZones();
	}

	private void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int index)
	{
		if (index % 33 != 0)
		{
			return;
		}
		foreach (Clan banditFaction in Clan.BanditFactions)
		{
			DailyTickClan(banditFaction);
		}
	}

	private void DailyTickClan(Clan clan)
	{
		if (IsPirateClan(clan))
		{
			TrySpawnPirateParties(clan);
		}
	}

	private void DailyTickParty(MobileParty mobileParty)
	{
		if (IsPirateParty(mobileParty))
		{
			TryRemoveWeakPirate(mobileParty);
		}
	}

	private void TryRemoveWeakPirate(MobileParty pirateParty)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (!pirateParty.HasNavalNavigationCapability || pirateParty.MapEvent != null)
		{
			return;
		}
		bool num = (float)pirateParty.MemberRoster.TotalHealthyCount < (float)pirateParty.ActualClan.DefaultPartyTemplate.GetLowerTroopLimit() * 0.7f || (float)((List<Ship>)(object)pirateParty.Ships).Count < (float)LinQuick.SumQ<ShipTemplateStack>((List<ShipTemplateStack>)(object)pirateParty.ActualClan.DefaultPartyTemplate.ShipHulls, (Func<ShipTemplateStack, int>)((ShipTemplateStack t) => t.MinValue)) * 0.7f;
		float num2 = MobileParty.MainParty.SeeingRange * 2f;
		if (!num)
		{
			return;
		}
		CampaignVec2 position = pirateParty.Position;
		if (((CampaignVec2)(ref position)).DistanceSquared(MobileParty.MainParty.Position) >= num2 * num2)
		{
			DestroyPartyAction.ApplyForDisbanding(pirateParty, Settlement.FindFirst((Func<Settlement, bool>)((Settlement t) => t.IsHideout)));
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<Dictionary<MobileParty, PatrolZone>>("_assignedZones", ref _assignedZones);
	}

	private void AssignPatrolZones(Clan clan)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		_patrolZones[clan] = new List<PatrolZone>();
		foreach (var spawnPoint in NavalDLCManager.Instance.NavalMapSceneWrapper.GetSpawnPoints(((MBObjectBase)clan).StringId))
		{
			if (!FindIdenticalZone(spawnPoint.Item1, spawnPoint.Item2, out var zone))
			{
				zone = new PatrolZone(spawnPoint.Item1, spawnPoint.Item2);
			}
			_patrolZones[clan].Add(zone);
		}
	}

	private bool FindIdenticalZone(CampaignVec2 position, float radius, out PatrolZone zone)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		zone = null;
		foreach (KeyValuePair<MobileParty, PatrolZone> assignedZone in _assignedZones)
		{
			CampaignVec2 position2 = assignedZone.Value.Position;
			if (MBMath.ApproximatelyEqualsTo(((CampaignVec2)(ref position2)).Distance(position), 0f, 1E-05f) && MBMath.ApproximatelyEqualsTo(assignedZone.Value.Radius, radius, 1E-05f))
			{
				zone = assignedZone.Value;
				return true;
			}
		}
		return false;
	}

	private PatrolZone GetClosestPatrolZone(MobileParty mobileParty)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		List<PatrolZone> list = _patrolZones[mobileParty.ActualClan];
		PatrolZone result = null;
		float num = float.MaxValue;
		foreach (PatrolZone item in list)
		{
			CampaignVec2 targetPosition = mobileParty.TargetPosition;
			float num2 = ((CampaignVec2)(ref targetPosition)).Distance(item.Position);
			if (num2 < num && CanSpawnPiratePartyInZone(mobileParty.ActualClan, item))
			{
				num = num2;
				result = item;
			}
		}
		return result;
	}

	private void AdjustAssignedPatrolZones()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		foreach (MobileParty item in (List<MobileParty>)(object)MobileParty.AllBanditParties)
		{
			if (IsPirateParty(item) && GetAssignedZone(item) == null)
			{
				PatrolZone patrolZone = GetClosestPatrolZone(item);
				if (patrolZone == null)
				{
					Debug.FailedAssert("zone != null", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\CampaignBehaviors\\PiratesCampaignBehavior.cs", "AdjustAssignedPatrolZones", 387);
					List<PatrolZone> patrolZones = GetPatrolZones(item.ActualClan);
					patrolZone = ((patrolZones.Count != 0) ? Extensions.GetRandomElement<PatrolZone>((IReadOnlyList<PatrolZone>)patrolZones) : new PatrolZone(item.TargetPosition, 20f));
				}
				AssignPartyToZone(patrolZone, item);
				if (item.MapEvent == null)
				{
					item.SetMovePatrolAroundPoint(patrolZone.Position, (NavigationType)2);
				}
			}
		}
		foreach (KeyValuePair<MobileParty, PatrolZone> assignedZone in _assignedZones)
		{
			assignedZone.Value.Density++;
		}
	}

	private bool IsPirateParty(MobileParty mobileParty)
	{
		if (mobileParty.ActualClan != null && IsPirateClan(mobileParty.ActualClan) && !mobileParty.IsCurrentlyUsedByAQuest && mobileParty.HasNavalNavigationCapability)
		{
			return mobileParty.IsCurrentlyAtSea;
		}
		return false;
	}

	private List<PatrolZone> GetPatrolZones(Clan clan)
	{
		return _patrolZones[clan];
	}

	private List<PatrolZone> GetAllPatrolZones()
	{
		List<PatrolZone> list = new List<PatrolZone>();
		foreach (KeyValuePair<Clan, List<PatrolZone>> patrolZone in _patrolZones)
		{
			list.AddRange(patrolZone.Value);
		}
		return list;
	}

	private PatrolZone GetAssignedZone(MobileParty party)
	{
		if (_assignedZones.TryGetValue(party, out var value))
		{
			return value;
		}
		return null;
	}

	private void AssignPartyToZone(PatrolZone zone, MobileParty mobileParty)
	{
		if (_assignedZones.TryGetValue(mobileParty, out var value))
		{
			UnassignPartyFromZone(value, mobileParty);
		}
		zone.Density++;
		_assignedZones[mobileParty] = zone;
	}

	private void UnassignPartyFromZone(PatrolZone assignedZone, MobileParty mobileParty)
	{
		assignedZone.Density--;
		_assignedZones.Remove(mobileParty);
	}

	private void TrySpawnPirateParties(Clan clan)
	{
		GetPirateData(clan, out var pirateMemberCount, out var maximumPirateCount);
		int num = MathF.Floor(MathF.Pow((float)(maximumPirateCount - pirateMemberCount), 0.66f));
		if (num <= 0)
		{
			return;
		}
		int num2 = 0;
		List<PatrolZone> patrolZones = GetPatrolZones(clan);
		Extensions.Shuffle<PatrolZone>((IList<PatrolZone>)patrolZones);
		int iter = 0;
		int bestScore = 0;
		for (int i = 0; i < num; i++)
		{
			PatrolZone randomSuitableZone = GetRandomSuitableZone(clan, patrolZones, ref iter, ref bestScore);
			if (randomSuitableZone != null)
			{
				SpawnPirateParty(clan, randomSuitableZone);
				num2++;
				continue;
			}
			break;
		}
	}

	private void GetPirateData(Clan clan, out int pirateMemberCount, out int maximumPirateCount)
	{
		pirateMemberCount = ((List<WarPartyComponent>)(object)clan.WarPartyComponents).Count;
		maximumPirateCount = Campaign.Current.Models.BanditDensityModel.GetMaxSupportedNumberOfLootersForClan(clan);
	}

	private void SpawnPirateParty(Clan clan, PatrolZone patrolZone)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Settlement val = SettlementHelper.FindNearestSettlementToPoint(ref patrolZone.Position, (Func<Settlement, bool>)((Settlement x) => x.IsTown && x.HasPort));
		CampaignVec2 spawnPosition = GetSpawnPosition(patrolZone);
		MobileParty val2 = BanditPartyComponent.CreateLooterParty(((MBObjectBase)clan).StringId + "_1", clan, val, false, clan.DefaultPartyTemplate, spawnPosition);
		InitializePirateParty(val2, clan);
		AssignPartyToZone(patrolZone, val2);
		val2.SetMovePatrolAroundPoint(patrolZone.Position, (NavigationType)2);
	}

	private CampaignVec2 GetSpawnPosition(PatrolZone zone)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (Campaign.Current.GameStarted && IsPointVisibleToPlayer(zone))
		{
			float num = float.MaxValue;
			PatrolZone patrolZone = null;
			foreach (PatrolZone allPatrolZone in GetAllPatrolZones())
			{
				float zoneDistance = GetZoneDistance(zone, allPatrolZone);
				if (!IsPointVisibleToPlayer(allPatrolZone) && (zoneDistance < num || patrolZone == null))
				{
					patrolZone = allPatrolZone;
					num = zoneDistance;
				}
			}
			if (patrolZone != null)
			{
				zone = patrolZone;
			}
		}
		int num2 = 0;
		CampaignVec2 val = NavigationHelper.FindPointAroundPosition(zone.Position, (NavigationType)2, zone.Radius, 0f, true, false);
		do
		{
			val = NavigationHelper.FindPointAroundPosition(zone.Position, (NavigationType)2, zone.Radius, 0f, true, false);
			num2++;
		}
		while (num2 < 100 && Campaign.Current.Models.BanditDensityModel.IsPositionInsideNavalSafeZone(val));
		return val;
	}

	private float GetZoneDistance(PatrolZone p1, PatrolZone p2)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		int[] invalidTerrainTypesForNavigationType = Campaign.Current.Models.PartyNavigationModel.GetInvalidTerrainTypesForNavigationType((NavigationType)2);
		IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
		CampaignVec2 position = p1.Position;
		PathFaceRecord face = ((CampaignVec2)(ref position)).Face;
		position = p2.Position;
		PathFaceRecord face2 = ((CampaignVec2)(ref position)).Face;
		position = p1.Position;
		Vec2 val = ((CampaignVec2)(ref position)).ToVec2();
		position = p2.Position;
		float result = default(float);
		mapSceneWrapper.GetPathDistanceBetweenAIFaces(face, face2, val, ((CampaignVec2)(ref position)).ToVec2(), 0.5f, (float)Campaign.PathFindingMaxCostLimit, ref result, invalidTerrainTypesForNavigationType, Campaign.Current.Models.MapDistanceModel.RegionSwitchCostFromLandToSea, Campaign.Current.Models.MapDistanceModel.RegionSwitchCostFromSeaToLand);
		return result;
	}

	private PatrolZone GetRandomSuitableZone(Clan clan, List<PatrolZone> zones, ref int iter, ref int bestScore)
	{
		int num = iter + zones.Count;
		int num2 = -1;
		PatrolZone result = null;
		while (iter < num)
		{
			PatrolZone patrolZone = zones[iter % zones.Count];
			iter++;
			if (CanSpawnPiratePartyInZone(clan, patrolZone))
			{
				if (bestScore == patrolZone.Density)
				{
					return patrolZone;
				}
				if (num2 < patrolZone.Density)
				{
					num2 = patrolZone.Density;
					result = patrolZone;
				}
			}
		}
		iter = 0;
		bestScore = num2;
		return result;
	}

	private bool CanSpawnPiratePartyInZone(Clan clan, PatrolZone zone)
	{
		if (_patrolZones.TryGetValue(clan, out var value))
		{
			return value.Contains(zone);
		}
		return false;
	}

	private bool IsPirateClan(Clan clan)
	{
		if (!((BasicCultureObject)clan.Culture).CanHaveSettlement && clan.HasNavalNavigationCapability)
		{
			return clan.IsBanditFaction;
		}
		return false;
	}

	private void InitializePirateParty(MobileParty pirateParty, Clan faction)
	{
		pirateParty.Party.SetVisualAsDirty();
		pirateParty.ActualClan = faction;
		pirateParty.Aggressiveness = 1f - 0.2f * MBRandom.RandomFloat;
		CreatePartyTrade(pirateParty);
		GiveFoodToBanditParty(pirateParty);
		pirateParty.SetLandNavigationAccess(false);
	}

	private bool IsPointVisibleToPlayer(PatrolZone zone)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 position = MobileParty.MainParty.Position;
		return ((CampaignVec2)(ref position)).DistanceSquared(zone.Position) < (MobileParty.MainParty.SeeingRange + zone.Radius) * (MobileParty.MainParty.SeeingRange + zone.Radius);
	}

	private static void CreatePartyTrade(MobileParty banditParty)
	{
		int num = (int)(10f * (float)banditParty.Party.MemberRoster.TotalManCount * (0.5f + 1f * MBRandom.RandomFloat));
		banditParty.InitializePartyTrade(num);
	}

	private void GiveFoodToBanditParty(MobileParty banditParty)
	{
		foreach (ItemObject item in (List<ItemObject>)(object)Items.All)
		{
			if (item.IsFood)
			{
				int num = MBRandom.RoundRandomized((float)banditParty.MemberRoster.TotalManCount * (1f / (float)item.Value) * 8f * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat * MBRandom.RandomFloat);
				if (num > 0)
				{
					banditParty.ItemRoster.AddToCounts(item, num);
				}
			}
		}
	}

	public float GetPatrolRadius(MobileParty mobileParty)
	{
		if (_assignedZones.TryGetValue(mobileParty, out var value))
		{
			return value.Radius;
		}
		return 25f;
	}
}
