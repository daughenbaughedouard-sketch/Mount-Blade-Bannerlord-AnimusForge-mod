using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.CampaignBehaviors;

public class ShipProductionCampaignBehavior : CampaignBehaviorBase
{
	private const float ShipGenerationChance = 0.5f;

	private const float ShipGenerationUpgradePieceAddingChance = 0.5f;

	private const int ShipGenerationDailyCount = 10;

	public static bool DebugShipyards;

	public override void RegisterEvents()
	{
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter, int>)OnNewGameCreatedPartialFollowUp);
		CampaignEvents.DailyTickTownEvent.AddNonSerializedListener((object)this, (Action<Town>)DailyTickTown);
		CampaignEvents.OnShipCreatedEvent.AddNonSerializedListener((object)this, (Action<Ship, Settlement>)OnShipCreated);
		CampaignEvents.OnShipDestroyedEvent.AddNonSerializedListener((object)this, (Action<PartyBase, Ship, ShipDestroyDetail>)OnShipDestroyed);
		CampaignEvents.OnShipOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Ship, PartyBase, ShipOwnerChangeDetail>)OnShipOwnerChanged);
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)Tick);
	}

	private void Tick(float obj)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugShipyards)
		{
			return;
		}
		foreach (Town item in (List<Town>)(object)Town.AllTowns)
		{
			if (!((SettlementComponent)item).Settlement.HasPort)
			{
				continue;
			}
			CampaignVec2 position = ((SettlementComponent)item).Settlement.Position;
			Vec3 val = ((CampaignVec2)(ref position)).AsVec3() + Vec3.Up * 3.75f;
			val.x -= 1f;
			Building shipyard = item.GetShipyard();
			string text = $"Ship Count: {((List<Ship>)(object)item.AvailableShips).Count}\nShipyard level: {shipyard.CurrentLevel}";
			foreach (Ship item2 in (List<Ship>)(object)item.AvailableShips)
			{
				text = text + "\n" + ((object)item2.ShipHull.Name).ToString();
				val = new Vec3(val.x, val.y + 0.25f, val.z, -1f);
			}
		}
		MBList<MobileParty> obj2 = Extensions.ToMBList<MobileParty>(((IEnumerable<MobileParty>)MobileParty.AllLordParties).OrderByDescending((MobileParty x) => ((List<Ship>)(object)x.Ships).Count).Take(5));
		int num = 140;
		foreach (MobileParty item3 in (List<MobileParty>)(object)obj2)
		{
			_ = item3;
			num += 20;
		}
	}

	private void DailyTickTown(Town town)
	{
		if (!((SettlementComponent)town).IsTown || town.IsUnderSiege || ((SettlementComponent)town).Settlement.Party.MapEvent != null || !((SettlementComponent)town).Settlement.HasPort)
		{
			return;
		}
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(0.5f, false, (TextObject)null);
		PerkHelper.AddPerkBonusForTown(NavalPerks.Boatswain.StreamlinedOperations, town, ref val);
		int maxShipCountForTown = GetMaxShipCountForTown(town);
		if (((List<Ship>)(object)town.AvailableShips).Count < maxShipCountForTown && MBRandom.RandomFloat < ((ExplainedNumber)(ref val)).ResultNumber)
		{
			for (int i = 0; i < 10; i++)
			{
				if (((List<Ship>)(object)town.AvailableShips).Count >= maxShipCountForTown)
				{
					break;
				}
				CreateShip(town);
			}
		}
		int idealShipCountForTown = GetIdealShipCountForTown(town);
		if (((List<Ship>)(object)town.AvailableShips).Count >= idealShipCountForTown)
		{
			TryRemoveExcessShipsFromTown(town);
		}
	}

	private static void TryRemoveExcessShipsFromTown(Town town)
	{
		int idealShipCountForTown = GetIdealShipCountForTown(town);
		int num = ((List<Ship>)(object)town.AvailableShips).Count - idealShipCountForTown;
		if (num <= 0)
		{
			return;
		}
		List<Ship> shipsOfOtherCulture = ((IEnumerable<Ship>)town.AvailableShips).Where((Ship x) => !((List<ShipHull>)(object)town.Culture.AvailableShipHulls).Contains(x.ShipHull)).ToList();
		foreach (Ship item in shipsOfOtherCulture)
		{
			if (MBRandom.RandomFloat < 0.7f)
			{
				DestroyShipAction.Apply(item);
				num--;
				if (num < 0)
				{
					break;
				}
			}
		}
		if (num <= 0)
		{
			return;
		}
		foreach (Ship item2 in ((IEnumerable<Ship>)town.AvailableShips).Where((Ship x) => !shipsOfOtherCulture.Contains(x)).ToList())
		{
			if (MBRandom.RandomFloat < 0.3f)
			{
				DestroyShipAction.Apply(item2);
				num--;
				if (num < 0)
				{
					break;
				}
			}
		}
	}

	private void CreateShip(Town town)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		ShipHull randomShipHull = GetRandomShipHull(town);
		if (randomShipHull == null)
		{
			return;
		}
		Ship val = new Ship(randomShipHull);
		List<ShipUpgradePiece> availableShipUpgradePieces = town.GetAvailableShipUpgradePieces();
		Extensions.Shuffle<ShipUpgradePiece>((IList<ShipUpgradePiece>)availableShipUpgradePieces);
		foreach (KeyValuePair<string, ShipSlot> availableSlot in val.ShipHull.AvailableSlots)
		{
			if (!(MBRandom.RandomFloat > 0.5f))
			{
				continue;
			}
			int num = MBRandom.RandomInt(availableShipUpgradePieces.Count);
			for (int i = 0; i < availableShipUpgradePieces.Count; i++)
			{
				ShipUpgradePiece val2 = availableShipUpgradePieces[(i + num) % availableShipUpgradePieces.Count];
				if (val2.DoesPieceMatchSlot(availableSlot.Value))
				{
					val.EquipUpgradePiece(availableSlot.Key, val2);
					break;
				}
			}
		}
		ChangeShipOwnerAction.ApplyByProduction(((SettlementComponent)town).Settlement.Party, val);
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnShipCreated(val, ((SettlementComponent)town).Settlement);
	}

	private void OnShipOwnerChanged(Ship ship, PartyBase oldOwner, ShipOwnerChangeDetail changeDetail)
	{
		if (ship.Owner.IsSettlement)
		{
			RepairShipAction.ApplyForFree(ship);
		}
	}

	private void OnNewGameCreatedPartialFollowUp(CampaignGameStarter starter, int index)
	{
		foreach (Town item in (List<Town>)(object)Town.AllTowns)
		{
			DailyTickTown(item);
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnShipCreated(Ship ship, Settlement settlement)
	{
		if (settlement.IsFortification && settlement.Town.Governor != null && settlement.Town.Governor.GetPerkValue(NavalPerks.Boatswain.MerchantFleet))
		{
			float secondaryBonus = NavalPerks.Boatswain.MerchantFleet.SecondaryBonus;
			if (secondaryBonus > 0f)
			{
				GainKingdomInfluenceAction.ApplyForDefault(settlement.Owner, secondaryBonus);
			}
		}
	}

	private void OnShipDestroyed(PartyBase party, Ship ship, ShipDestroyDetail detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)detail == 1 && party.IsMobile && party.MobileParty.HasPerk(NavalPerks.Boatswain.Salvage, false))
		{
			float num = ship.HitPoints * 0.01f;
			if (num > 0f)
			{
				GainKingdomInfluenceAction.ApplyForDefault(party.LeaderHero, num);
			}
		}
	}

	private ShipHull GetRandomShipHull(Town town)
	{
		MBList<(ShipHull, float)> availableShipHullsForTown = GetAvailableShipHullsForTown(town);
		if (((List<(ShipHull, float)>)(object)availableShipHullsForTown).Count == 0)
		{
			Debug.FailedAssert("Could not find ships to create.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\CampaignBehaviors\\ShipProductionCampaignBehavior.cs", "GetRandomShipHull", 231);
		}
		return MBRandom.ChooseWeighted<ShipHull>((IReadOnlyList<ValueTuple<ShipHull, float>>)availableShipHullsForTown);
	}

	private MBList<(ShipHull, float)> GetAvailableShipHullsForTown(Town town)
	{
		MBList<(ShipHull, float)> val = new MBList<(ShipHull, float)>();
		foreach (ShipHull item in (List<ShipHull>)(object)town.Culture.AvailableShipHulls)
		{
			if (CanTownCreateShipFromHull(town, item))
			{
				((List<(ShipHull, float)>)(object)val).Add((item, item.ProductionBuildWeight));
			}
		}
		return val;
	}

	private bool CanTownCreateShipFromHull(Town town, ShipHull shipHull)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		ShipType type = shipHull.Type;
		return (int)type switch
		{
			0 => town.GetShipyard().CurrentLevel > 0, 
			1 => town.GetShipyard().CurrentLevel > 1, 
			2 => town.GetShipyard().CurrentLevel == 3, 
			_ => false, 
		};
	}

	private static int GetMaxShipCountForTown(Town town)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber val = default(ExplainedNumber);
		town.AddEffectOfBuildings((BuildingEffectEnum)29, ref val);
		return (int)((ExplainedNumber)(ref val)).ResultNumber;
	}

	private static int GetIdealShipCountForTown(Town town)
	{
		return MathF.Max(GetMaxShipCountForTown(town) - 2, 0);
	}
}
