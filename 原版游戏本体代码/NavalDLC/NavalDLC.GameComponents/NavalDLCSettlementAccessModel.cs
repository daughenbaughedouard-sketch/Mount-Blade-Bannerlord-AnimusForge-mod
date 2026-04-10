using System.Collections.Generic;
using NavalDLC.Storyline;
using NavalDLC.Storyline.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GameComponents;

public class NavalDLCSettlementAccessModel : SettlementAccessModel
{
	public override bool CanMainHeroAccessLocation(Settlement settlement, string locationId, out bool disableOption, out TextObject disabledText)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (locationId.Equals("center"))
		{
			if (NavalStorylineData.IsNavalStoryLineActive())
			{
				disableOption = true;
				disabledText = new TextObject("{=ILnr9eCQ}Door is locked!", (Dictionary<string, object>)null);
				return false;
			}
		}
		else if (locationId == "port")
		{
			return CanMainHeroEnterPort(settlement, out disabledText, out disableOption);
		}
		return ((MBGameModel<SettlementAccessModel>)this).BaseModel.CanMainHeroAccessLocation(settlement, locationId, ref disableOption, ref disabledText);
	}

	public override void CanMainHeroEnterSettlement(Settlement settlement, out AccessDetails accessDetails)
	{
		((MBGameModel<SettlementAccessModel>)this).BaseModel.CanMainHeroEnterSettlement(settlement, ref accessDetails);
	}

	public override void CanMainHeroEnterLordsHall(Settlement settlement, out AccessDetails accessDetails)
	{
		((MBGameModel<SettlementAccessModel>)this).BaseModel.CanMainHeroEnterLordsHall(settlement, ref accessDetails);
	}

	public override void CanMainHeroEnterDungeon(Settlement settlement, out AccessDetails accessDetails)
	{
		((MBGameModel<SettlementAccessModel>)this).BaseModel.CanMainHeroEnterDungeon(settlement, ref accessDetails);
	}

	public override bool CanMainHeroDoSettlementAction(Settlement settlement, SettlementAction settlementAction, out bool disableOption, out TextObject disabledText)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<SettlementAccessModel>)this).BaseModel.CanMainHeroDoSettlementAction(settlement, settlementAction, ref disableOption, ref disabledText);
	}

	public override bool IsRequestMeetingOptionAvailable(Settlement settlement, out bool disableOption, out TextObject disabledText)
	{
		return ((MBGameModel<SettlementAccessModel>)this).BaseModel.IsRequestMeetingOptionAvailable(settlement, ref disableOption, ref disabledText);
	}

	private bool CanMainHeroEnterPort(Settlement settlement, out TextObject disabledText, out bool disableOption)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		bool result = true;
		disabledText = TextObject.GetEmpty();
		disableOption = false;
		if (Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement && Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SpeakToGunnarAndSisterQuest)) && Mission.Current != null)
		{
			result = false;
			disableOption = true;
			disabledText = new TextObject("{=UjERCi2F}This feature is disabled.", (Dictionary<string, object>)null);
		}
		else if (Campaign.Current.IsMainHeroDisguised)
		{
			if (Mission.Current == null)
			{
				disabledText = new TextObject("{=*}You cannot enter the port while in disguise.", (Dictionary<string, object>)null);
			}
			else
			{
				disabledText = new TextObject("{=ILnr9eCQ}Door is locked!", (Dictionary<string, object>)null);
			}
			result = false;
			disableOption = true;
		}
		return result;
	}
}
