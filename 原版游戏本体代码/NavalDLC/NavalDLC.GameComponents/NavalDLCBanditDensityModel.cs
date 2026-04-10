using System;
using System.Collections.Generic;
using StoryMode;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.GameComponents;

public class NavalDLCBanditDensityModel : BanditDensityModel
{
	private const float GetNavalSafeZoneRadiusForFortificationPort = 15f;

	private const float GetNavalSafeZoneRadiusForVillagePort = 7f;

	private Clan _deserterClan;

	private Clan DeserterClan
	{
		get
		{
			if (_deserterClan == null)
			{
				_deserterClan = Clan.FindFirst((Predicate<Clan>)((Clan x) => ((MBObjectBase)x).StringId == "deserters"));
			}
			return _deserterClan;
		}
	}

	public override int NumberOfMinimumBanditPartiesInAHideoutToInfestIt => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMinimumBanditPartiesInAHideoutToInfestIt;

	public override int NumberOfMaximumBanditPartiesInEachHideout => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMaximumBanditPartiesInEachHideout;

	public override int NumberOfMaximumBanditPartiesAroundEachHideout => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMaximumBanditPartiesAroundEachHideout;

	public override int NumberOfMinimumBanditTroopsInHideoutMission => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMinimumBanditTroopsInHideoutMission;

	public override int NumberOfInitialHideoutsAtEachBanditFaction
	{
		get
		{
			StoryModeManager current = StoryModeManager.Current;
			if (current != null)
			{
				MainStoryLine mainStoryLine = current.MainStoryLine;
				if (((mainStoryLine != null) ? new bool?(mainStoryLine.IsPlayerInteractionRestricted) : ((bool?)null)) == true)
				{
					return 0;
				}
			}
			return 8;
		}
	}

	public override int NumberOfMaximumHideoutsAtEachBanditFaction
	{
		get
		{
			StoryModeManager current = StoryModeManager.Current;
			if (current != null)
			{
				MainStoryLine mainStoryLine = current.MainStoryLine;
				if (((mainStoryLine != null) ? new bool?(mainStoryLine.IsPlayerInteractionRestricted) : ((bool?)null)) == true)
				{
					return 0;
				}
			}
			return 9;
		}
	}

	public override int NumberOfMaximumTroopCountForFirstFightInHideout => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMaximumTroopCountForFirstFightInHideout;

	public override int NumberOfMaximumTroopCountForBossFightInHideout => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMaximumTroopCountForBossFightInHideout;

	public override float SpawnPercentageForFirstFightInHideoutMission => ((MBGameModel<BanditDensityModel>)this).BaseModel.SpawnPercentageForFirstFightInHideoutMission;

	public override int GetMaximumTroopCountForHideoutMission(MobileParty party, bool isAssault)
	{
		return ((MBGameModel<BanditDensityModel>)this).BaseModel.GetMaximumTroopCountForHideoutMission(party, isAssault);
	}

	public override bool IsPositionInsideNavalSafeZone(CampaignVec2 position)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (((CampaignVec2)(ref position)).IsValid() && !position.IsOnLand)
		{
			Settlement item = Campaign.Current.Models.MapDistanceModel.GetClosestEntranceToFace(((CampaignVec2)(ref position)).Face, (NavigationType)2).Item1;
			float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(item, ref position, true, (NavigationType)2);
			float num = (item.IsVillage ? 7f : 15f);
			if (distance < num)
			{
				return true;
			}
		}
		return false;
	}

	public override int GetMaxSupportedNumberOfLootersForClan(Clan clan)
	{
		StoryModeManager current = StoryModeManager.Current;
		if (current != null)
		{
			MainStoryLine mainStoryLine = current.MainStoryLine;
			if (((mainStoryLine != null) ? new bool?(mainStoryLine.IsPlayerInteractionRestricted) : ((bool?)null)) == true)
			{
				return 0;
			}
		}
		if (clan.HasNavalNavigationCapability)
		{
			return NavalDLCManager.Instance.NavalMapSceneWrapper.GetSpawnPoints(((MBObjectBase)clan).StringId).Count;
		}
		if (((MBObjectBase)clan).StringId == "looters")
		{
			return 300 - ((DeserterClan != null) ? ((List<WarPartyComponent>)(object)DeserterClan.WarPartyComponents).Count : 0);
		}
		return ((MBGameModel<BanditDensityModel>)this).BaseModel.GetMaxSupportedNumberOfLootersForClan(clan);
	}

	public override int GetMinimumTroopCountForHideoutMission(MobileParty party, bool isAssault)
	{
		return ((MBGameModel<BanditDensityModel>)this).BaseModel.GetMinimumTroopCountForHideoutMission(party, isAssault);
	}
}
