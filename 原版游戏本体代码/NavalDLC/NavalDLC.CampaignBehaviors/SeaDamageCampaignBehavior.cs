using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.CampaignBehaviors;

public class SeaDamageCampaignBehavior : CampaignBehaviorBase
{
	public static bool DebugSeaDamage;

	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)HourlyTickParty);
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)Tick);
	}

	private void Tick(float dt)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugSeaDamage)
		{
			return;
		}
		foreach (MobileParty item in (List<MobileParty>)(object)MobileParty.All)
		{
			if (item.IsVisible && item.CurrentSettlement == null && item.IsCurrentlyAtSea && ((IEnumerable<Ship>)item.Ships).Any())
			{
				Ship val = ((List<Ship>)(object)item.Ships)[0];
				float maxHitPoints = val.MaxHitPoints;
				float hitPoints = val.HitPoints;
				CampaignVec2 position = item.Position;
				Vec3 val2 = ((CampaignVec2)(ref position)).AsVec3() + Vec3.Up * 3.75f;
				val2.x -= 1f;
				int num = 0;
				float num2 = Campaign.Current.Models.CampaignShipDamageModel.GetHourlyShipDamage(item, val);
				if (num2 > 0f)
				{
					num = (int)(val.HitPoints / val.MaxHitPoints / num2);
				}
				string text = ((object)(TerrainType)item.CurrentNavigationFace.FaceGroupIndex/*cast due to .constrained prefix*/).ToString();
				object[] obj = new object[5] { maxHitPoints, hitPoints, val.SeaWorthiness, text, null };
				MapWeatherModel mapWeatherModel = Campaign.Current.Models.MapWeatherModel;
				position = item.Position;
				obj[4] = ((object)mapWeatherModel.GetWeatherEventInPosition(((CampaignVec2)(ref position)).ToVec2())/*cast due to .constrained prefix*/).ToString();
				string text2 = string.Format("Max Hitpoints: {0}\nHitpoints: {1}\nSeaworthiness: {2}\nTerrain: {3}\nEffected by: {4}", obj);
				if (num > 0)
				{
					text2 += $"\nEstimated Hours: {num}";
				}
				else
				{
					text2 += "\nEstimated Hours: N/A";
				}
			}
		}
	}

	private void HourlyTickParty(MobileParty party)
	{
		if (!party.IsActive || !party.IsCurrentlyAtSea || party.IsInRaftState || party.MapEvent != null)
		{
			return;
		}
		float num3 = default(float);
		for (int num = ((List<Ship>)(object)party.Ships).Count - 1; num >= 0; num--)
		{
			float num2 = Campaign.Current.Models.CampaignShipDamageModel.GetHourlyShipDamage(party, ((List<Ship>)(object)party.Ships)[num]);
			if (num2 > 0f)
			{
				((List<Ship>)(object)party.Ships)[num].OnShipDamaged(num2, (IShipOrigin)null, ref num3);
			}
		}
		if (party.HasPerk(NavalPerks.Shipmaster.MasterAndCommander, false))
		{
			AddXpToTroops(party, MathF.Round(NavalPerks.Shipmaster.MasterAndCommander.PrimaryBonus));
		}
	}

	private static void AddXpToTroops(MobileParty party, int amount)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		TroopRoster memberRoster = party.MemberRoster;
		int val = default(int);
		for (int i = 0; i < memberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = memberRoster.GetElementCopyAtIndex(i);
			if (!((BasicCharacterObject)elementCopyAtIndex.Character).IsHero && MobilePartyHelper.CanTroopGainXp(party.Party, elementCopyAtIndex.Character, ref val))
			{
				int num = Math.Min(val, amount);
				memberRoster.AddXpToTroopAtIndex(i, num);
			}
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
