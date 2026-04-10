using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace StoryMode.GameComponents;

public class StoryModeBanditDensityModel : BanditDensityModel
{
	public override int NumberOfMaximumBanditPartiesAroundEachHideout
	{
		get
		{
			if (StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted)
			{
				return 0;
			}
			return ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMaximumBanditPartiesAroundEachHideout;
		}
	}

	public override int NumberOfMaximumBanditPartiesInEachHideout
	{
		get
		{
			if (StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted)
			{
				return 0;
			}
			return ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMaximumBanditPartiesInEachHideout;
		}
	}

	public override int NumberOfMaximumHideoutsAtEachBanditFaction
	{
		get
		{
			if (StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted)
			{
				return 0;
			}
			return ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMaximumHideoutsAtEachBanditFaction;
		}
	}

	public override int NumberOfInitialHideoutsAtEachBanditFaction
	{
		get
		{
			if (StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted)
			{
				return 0;
			}
			return ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfInitialHideoutsAtEachBanditFaction;
		}
	}

	public override int NumberOfMinimumBanditPartiesInAHideoutToInfestIt => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMinimumBanditPartiesInAHideoutToInfestIt;

	public override int NumberOfMinimumBanditTroopsInHideoutMission => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMinimumBanditTroopsInHideoutMission;

	public override int NumberOfMaximumTroopCountForFirstFightInHideout => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMaximumTroopCountForFirstFightInHideout;

	public override int NumberOfMaximumTroopCountForBossFightInHideout => ((MBGameModel<BanditDensityModel>)this).BaseModel.NumberOfMaximumTroopCountForBossFightInHideout;

	public override float SpawnPercentageForFirstFightInHideoutMission => ((MBGameModel<BanditDensityModel>)this).BaseModel.SpawnPercentageForFirstFightInHideoutMission;

	public override int GetMaximumTroopCountForHideoutMission(MobileParty party, bool isAssault)
	{
		return ((MBGameModel<BanditDensityModel>)this).BaseModel.GetMaximumTroopCountForHideoutMission(party, isAssault);
	}

	public override bool IsPositionInsideNavalSafeZone(CampaignVec2 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<BanditDensityModel>)this).BaseModel.IsPositionInsideNavalSafeZone(position);
	}

	public override int GetMaxSupportedNumberOfLootersForClan(Clan clan)
	{
		if (StoryModeManager.Current.MainStoryLine.IsPlayerInteractionRestricted)
		{
			return 0;
		}
		return ((MBGameModel<BanditDensityModel>)this).BaseModel.GetMaxSupportedNumberOfLootersForClan(clan);
	}

	public override int GetMinimumTroopCountForHideoutMission(MobileParty party, bool isAssault)
	{
		return ((MBGameModel<BanditDensityModel>)this).BaseModel.GetMinimumTroopCountForHideoutMission(party, isAssault);
	}
}
