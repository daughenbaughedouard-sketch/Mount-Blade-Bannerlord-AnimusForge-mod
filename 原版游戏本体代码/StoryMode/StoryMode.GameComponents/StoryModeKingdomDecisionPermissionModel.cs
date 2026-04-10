using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace StoryMode.GameComponents;

public class StoryModeKingdomDecisionPermissionModel : KingdomDecisionPermissionModel
{
	public override bool IsPolicyDecisionAllowed(PolicyObject policy)
	{
		return ((MBGameModel<KingdomDecisionPermissionModel>)this).BaseModel.IsPolicyDecisionAllowed(policy);
	}

	public override bool IsAnnexationDecisionAllowed(Settlement annexedSettlement)
	{
		return ((MBGameModel<KingdomDecisionPermissionModel>)this).BaseModel.IsAnnexationDecisionAllowed(annexedSettlement);
	}

	public override bool IsExpulsionDecisionAllowed(Clan expelledClan)
	{
		return ((MBGameModel<KingdomDecisionPermissionModel>)this).BaseModel.IsExpulsionDecisionAllowed(expelledClan);
	}

	public override bool IsKingSelectionDecisionAllowed(Kingdom kingdom)
	{
		return ((MBGameModel<KingdomDecisionPermissionModel>)this).BaseModel.IsKingSelectionDecisionAllowed(kingdom);
	}

	public override bool IsWarDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
	{
		if (StoryModeManager.Current.MainStoryLine.ThirdPhase != null)
		{
			MBReadOnlyList<Kingdom> oppositionKingdoms = StoryModeManager.Current.MainStoryLine.ThirdPhase.OppositionKingdoms;
			if (((List<Kingdom>)(object)oppositionKingdoms).IndexOf(kingdom1) >= 0 && ((List<Kingdom>)(object)oppositionKingdoms).IndexOf(kingdom2) >= 0)
			{
				reason = GameTexts.FindText("str_kingdom_diplomacy_war_truce_disabled_reason_story", (string)null);
				return false;
			}
		}
		return ((MBGameModel<KingdomDecisionPermissionModel>)this).BaseModel.IsWarDecisionAllowedBetweenKingdoms(kingdom1, kingdom2, ref reason);
	}

	public override bool IsPeaceDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
	{
		if (StoryModeManager.Current.MainStoryLine.ThirdPhase != null)
		{
			MBReadOnlyList<Kingdom> oppositionKingdoms = StoryModeManager.Current.MainStoryLine.ThirdPhase.OppositionKingdoms;
			MBReadOnlyList<Kingdom> allyKingdoms = StoryModeManager.Current.MainStoryLine.ThirdPhase.AllyKingdoms;
			if ((((List<Kingdom>)(object)oppositionKingdoms).IndexOf(kingdom1) >= 0 && ((List<Kingdom>)(object)allyKingdoms).IndexOf(kingdom2) >= 0) || (((List<Kingdom>)(object)oppositionKingdoms).IndexOf(kingdom2) >= 0 && ((List<Kingdom>)(object)allyKingdoms).IndexOf(kingdom1) >= 0))
			{
				reason = GameTexts.FindText("str_kingdom_diplomacy_war_truce_disabled_reason_story", (string)null);
				return false;
			}
		}
		return ((MBGameModel<KingdomDecisionPermissionModel>)this).BaseModel.IsPeaceDecisionAllowedBetweenKingdoms(kingdom1, kingdom2, ref reason);
	}

	public override bool IsStartAllianceDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
	{
		return ((MBGameModel<KingdomDecisionPermissionModel>)this).BaseModel.IsStartAllianceDecisionAllowedBetweenKingdoms(kingdom1, kingdom2, ref reason);
	}
}
