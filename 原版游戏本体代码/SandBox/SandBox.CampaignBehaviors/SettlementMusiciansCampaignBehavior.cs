using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Objects;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.CampaignBehaviors;

public class SettlementMusiciansCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionStarted);
	}

	private void OnMissionStarted(IMission mission)
	{
		Mission val;
		if ((val = (Mission)(object)((mission is Mission) ? mission : null)) == null || CampaignMission.Current == null || PlayerEncounter.LocationEncounter == null || PlayerEncounter.LocationEncounter.Settlement == null || CampaignMission.Current.Location == null || (Campaign.Current.IsMainHeroDisguised && !(CampaignMission.Current.Location.StringId != "center")))
		{
			return;
		}
		IEnumerable<MusicianGroup> enumerable = MBExtensions.FindAllWithType<MusicianGroup>((IEnumerable<MissionObject>)val.MissionObjects);
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		foreach (MusicianGroup item in enumerable)
		{
			List<SettlementMusicData> playList = CreateRandomPlayList(settlement);
			item.SetPlayList(playList);
		}
	}

	private List<SettlementMusicData> CreateRandomPlayList(Settlement settlement)
	{
		List<string> listOfLocationTags = new List<string>();
		string stringId = CampaignMission.Current.Location.StringId;
		if (stringId == "center")
		{
			listOfLocationTags.Add("lordshall");
			listOfLocationTags.Add("tavern");
		}
		else
		{
			listOfLocationTags.Add(stringId);
			if (stringId == "port")
			{
				listOfLocationTags.Add("tavern");
			}
		}
		Dictionary<CultureObject, float> dictionary = new Dictionary<CultureObject, float>();
		MBReadOnlyList<CultureObject> objectTypeList = MBObjectManager.Instance.GetObjectTypeList<CultureObject>();
		Town town = settlement.Town;
		float num;
		if (town == null)
		{
			Village village = settlement.Village;
			num = ((village != null) ? village.Bound.Town.Loyalty : 100f);
		}
		else
		{
			num = town.Loyalty;
		}
		float num2 = num * 0.01f;
		float num3 = 0f;
		foreach (CultureObject c in (List<CultureObject>)(object)objectTypeList)
		{
			dictionary.Add(c, 0f);
			float num4 = ((IEnumerable<Kingdom>)Kingdom.All).Sum((Kingdom k) => (c != k.Culture) ? 0f : k.CurrentTotalStrength);
			if (num4 > num3)
			{
				num3 = num4;
			}
		}
		foreach (Kingdom item in (List<Kingdom>)(object)Kingdom.All)
		{
			float num5 = (Campaign.MapDiagonal - Campaign.Current.Models.MapDistanceModel.GetDistance(item.FactionMidSettlement, settlement.MapFaction.FactionMidSettlement, false, false, (NavigationType)3)) / Campaign.Current.Models.MapDistanceModel.GetMaximumDistanceBetweenTwoConnectedSettlements((NavigationType)3);
			float num6 = num5 * num5 * num5 * 2f;
			num6 += (settlement.MapFaction.IsAtWarWith((IFaction)(object)item) ? 1f : 2f) * num2;
			dictionary[item.Culture] = MathF.Max(dictionary[item.Culture], num6);
		}
		foreach (Kingdom item2 in (List<Kingdom>)(object)Kingdom.All)
		{
			dictionary[item2.Culture] += item2.CurrentTotalStrength / num3 * 0.5f;
		}
		foreach (Town item3 in (List<Town>)(object)Town.AllTowns)
		{
			float num7 = (Campaign.MapDiagonal - Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, ((SettlementComponent)item3).Settlement, false, false, (NavigationType)((!settlement.HasPort || !((SettlementComponent)item3).Settlement.HasPort) ? 1 : 3))) / Campaign.MapDiagonal;
			float num8 = num7 * num7 * num7;
			num8 *= MathF.Min(item3.Prosperity, 5000f) * 0.0002f;
			dictionary[item3.Culture] += num8;
		}
		dictionary[settlement.Culture] += 10f;
		dictionary[settlement.MapFaction.Culture] += num2 * 5f;
		List<SettlementMusicData> list = ((IEnumerable<SettlementMusicData>)MBObjectManager.Instance.GetObjectTypeList<SettlementMusicData>()).Where((SettlementMusicData x) => listOfLocationTags.Contains(x.LocationId)).ToList();
		KeyValuePair<CultureObject, float> maxWeightedCulture = Extensions.MaxBy<KeyValuePair<CultureObject, float>, float>((IEnumerable<KeyValuePair<CultureObject, float>>)dictionary, (Func<KeyValuePair<CultureObject, float>, float>)((KeyValuePair<CultureObject, float> x) => x.Value));
		float num9 = (float)list.Count((SettlementMusicData x) => x.Culture == maxWeightedCulture.Key) / maxWeightedCulture.Value;
		List<SettlementMusicData> list2 = new List<SettlementMusicData>();
		foreach (KeyValuePair<CultureObject, float> item4 in dictionary)
		{
			int num10 = MBRandom.RoundRandomized(num9 * item4.Value);
			if (num10 > 0)
			{
				PopulatePlayList(list2, list, item4.Key, num10);
			}
		}
		if (Extensions.IsEmpty<SettlementMusicData>((IEnumerable<SettlementMusicData>)list2))
		{
			list2 = list;
		}
		Extensions.Shuffle<SettlementMusicData>((IList<SettlementMusicData>)list2);
		return list2;
	}

	private void PopulatePlayList(List<SettlementMusicData> playList, List<SettlementMusicData> settlementMusicDatas, CultureObject culture, int count)
	{
		List<SettlementMusicData> list = settlementMusicDatas.Where((SettlementMusicData x) => x.Culture == culture).ToList();
		Extensions.Shuffle<SettlementMusicData>((IList<SettlementMusicData>)list);
		for (int num = 0; num < count && num < list.Count; num++)
		{
			playList.Add(list[num]);
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}
}
