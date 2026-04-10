using System;
using System.Collections.Generic;
using Helpers;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.CampaignBehaviors;

public class NavalCompanionRolesCampaignBehavior : CampaignBehaviorBase
{
	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
	}

	private void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0036: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_006d: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Expected O, but got Unknown
		//IL_00af: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		//IL_00f1: Expected O, but got Unknown
		//IL_00f1: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_0128: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_015f: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Expected O, but got Unknown
		//IL_0196: Expected O, but got Unknown
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		//IL_01cd: Expected O, but got Unknown
		campaignGameStarter.AddPlayerLine("companion_becomes_first_mate", "companion_roles", "companion_okay", "{=FRTvNn9Q}I no longer need you as First Mate.", new OnConditionDelegate(companion_fire_first_mate_on_condition), new OnConsequenceDelegate(remove_first_mate_role_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("companion_becomes_navigator", "companion_roles", "companion_okay", "{=1dO4mgZI}I no longer need you as Navigator.", new OnConditionDelegate(companion_fire_navigator_on_condition), new OnConsequenceDelegate(remove_navigator_role_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("companion_becomes_first_mate_2", "companion_roles", "give_companion_roles", "{=fqva0OdY}First Mate {CURRENTLY_HELD_FIRST_MATE}", new OnConditionDelegate(companion_becomes_first_mate_on_condition), new OnConsequenceDelegate(companion_becomes_first_mate_on_consequence), 100, new OnClickableConditionDelegate(companion_becomes_first_mate_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("companion_becomes_navigator_2", "companion_roles", "give_companion_roles", "{=jjISJIcf}Navigator {CURRENTLY_HELD_NAVIGATOR}", new OnConditionDelegate(companion_becomes_navigator_on_condition), new OnConsequenceDelegate(companion_becomes_navigator_on_consequence), 100, new OnClickableConditionDelegate(companion_becomes_navigator_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("companion_becomes_first_mate_3", "too_many_roles_responses", "companion_okay_to_role_selection", "{=FRTvNn9Q}I no longer need you as First Mate.", new OnConditionDelegate(companion_fire_first_mate_on_condition), new OnConsequenceDelegate(remove_first_mate_role_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("companion_becomes_navigator_3", "too_many_roles_responses", "companion_okay_to_role_selection", "{=1dO4mgZI}I no longer need you as Navigator.", new OnConditionDelegate(companion_fire_navigator_on_condition), new OnConsequenceDelegate(remove_navigator_role_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("tavernkeeper_companion_info_player_select_first_mate", "tavernkeeper_list_companion_types", "player_selected_companion_type", "{=bdMwsaY6}I need a first mate who can enforce discipline and keep the ship battle-ready.", (OnConditionDelegate)null, new OnConsequenceDelegate(tavernkeeper_companion_info_player_select_first_mate_on_consequence), 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("tavernkeeper_companion_info_player_select_navigator", "tavernkeeper_list_companion_types", "player_selected_companion_type", "{=bzoUl6DI}I need a navigator who knows winds, currents and coasts, and can help me sail swiftly.", (OnConditionDelegate)null, new OnConsequenceDelegate(tavernkeeper_companion_info_player_select_navigator_on_consequence), 100, new OnClickableConditionDelegate(companion_type_select_clickable_condition), (OnPersuasionOptionDelegate)null);
	}

	private bool companion_becomes_first_mate_clickable_condition(out TextObject explanation)
	{
		return party_role_assignment_clickable_condition((PartyRole)14, out explanation);
	}

	private bool companion_becomes_first_mate_on_condition()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
		Hero roleHolder = oneToOneConversationHero.PartyBelongedTo.GetRoleHolder((PartyRole)14);
		if (roleHolder != null)
		{
			TextObject val = new TextObject("{=QEp8t8u0}(Currently held by {COMPANION.LINK})", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("COMPANION", roleHolder.CharacterObject, val, false);
			MBTextManager.SetTextVariable("CURRENTLY_HELD_FIRST_MATE", val, false);
		}
		else
		{
			MBTextManager.SetTextVariable("CURRENTLY_HELD_FIRST_MATE", "{=kNQMkh3j}(Currently unassigned)", false);
		}
		return roleHolder != oneToOneConversationHero;
	}

	private void companion_becomes_first_mate_on_consequence()
	{
		Hero.OneToOneConversationHero.PartyBelongedTo.SetPartyFirstMate(Hero.OneToOneConversationHero);
	}

	private bool companion_becomes_navigator_clickable_condition(out TextObject explanation)
	{
		return party_role_assignment_clickable_condition((PartyRole)15, out explanation);
	}

	private bool companion_becomes_navigator_on_condition()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
		Hero roleHolder = oneToOneConversationHero.PartyBelongedTo.GetRoleHolder((PartyRole)15);
		if (roleHolder != null)
		{
			TextObject val = new TextObject("{=QEp8t8u0}(Currently held by {COMPANION.LINK})", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("COMPANION", roleHolder.CharacterObject, val, false);
			MBTextManager.SetTextVariable("CURRENTLY_HELD_NAVIGATOR", val, false);
		}
		else
		{
			MBTextManager.SetTextVariable("CURRENTLY_HELD_NAVIGATOR", "{=kNQMkh3j}(Currently unassigned)", false);
		}
		return roleHolder != oneToOneConversationHero;
	}

	private void companion_becomes_navigator_on_consequence()
	{
		Hero.OneToOneConversationHero.PartyBelongedTo.SetPartyNavigator(Hero.OneToOneConversationHero);
	}

	private bool companion_fire_first_mate_on_condition()
	{
		return CanFireHeroFromRole((PartyRole)14, Hero.OneToOneConversationHero);
	}

	private bool companion_fire_navigator_on_condition()
	{
		return CanFireHeroFromRole((PartyRole)15, Hero.OneToOneConversationHero);
	}

	private void remove_first_mate_role_on_consequence()
	{
		Hero.OneToOneConversationHero.PartyBelongedTo.RemovePartyRoleOfHero(Hero.OneToOneConversationHero, (PartyRole)14);
	}

	private void remove_navigator_role_on_consequence()
	{
		Hero.OneToOneConversationHero.PartyBelongedTo.RemovePartyRoleOfHero(Hero.OneToOneConversationHero, (PartyRole)15);
	}

	private bool party_role_assignment_clickable_condition(PartyRole role, out TextObject explanation)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		bool num = Campaign.Current.Models.ClanMemberPartyRoleModel.IsHeroAssignableForPartyRoleInParty(role, Hero.OneToOneConversationHero, Hero.OneToOneConversationHero.PartyBelongedTo);
		if (!num)
		{
			explanation = new TextObject("{=zcTOL3gI}Not eligible for the role.", (Dictionary<string, object>)null);
			return num;
		}
		explanation = TextObject.GetEmpty();
		return num;
	}

	private bool CanFireHeroFromRole(PartyRole role, Hero hero)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (hero.PartyBelongedTo.GetRoleHolder(role) == hero)
		{
			return hero != hero.PartyBelongedTo.LeaderHero;
		}
		return false;
	}

	private void tavernkeeper_companion_info_player_select_first_mate_on_consequence()
	{
		TavernEmployeesCampaignBehavior behavior = Campaign.Current.CampaignBehaviorManager.GetBehavior<TavernEmployeesCampaignBehavior>();
		if (behavior != null)
		{
			behavior.FindCompanionWithType((PartyRole)14);
		}
		else
		{
			Debug.FailedAssert("TavernEmployeesCampaignBehavior does not exist!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\CampaignBehaviors\\NavalCompanionRolesCampaignBehavior.cs", "tavernkeeper_companion_info_player_select_first_mate_on_consequence", 159);
		}
	}

	private void tavernkeeper_companion_info_player_select_navigator_on_consequence()
	{
		TavernEmployeesCampaignBehavior behavior = Campaign.Current.CampaignBehaviorManager.GetBehavior<TavernEmployeesCampaignBehavior>();
		if (behavior != null)
		{
			behavior.FindCompanionWithType((PartyRole)15);
		}
		else
		{
			Debug.FailedAssert("TavernEmployeesCampaignBehavior does not exist!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\CampaignBehaviors\\NavalCompanionRolesCampaignBehavior.cs", "tavernkeeper_companion_info_player_select_navigator_on_consequence", 172);
		}
	}

	private static bool companion_type_select_clickable_condition(out TextObject explanation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		explanation = new TextObject("{=!}{COMPANION_INQUIRY_COST}{GOLD_ICON}.", (Dictionary<string, object>)null);
		MBTextManager.SetTextVariable("COMPANION_INQUIRY_COST", 2);
		if (Hero.MainHero.Gold < 2)
		{
			explanation = new TextObject("{=xVZVYNan}You don't have enough{GOLD_ICON}.", (Dictionary<string, object>)null);
			return false;
		}
		return true;
	}
}
