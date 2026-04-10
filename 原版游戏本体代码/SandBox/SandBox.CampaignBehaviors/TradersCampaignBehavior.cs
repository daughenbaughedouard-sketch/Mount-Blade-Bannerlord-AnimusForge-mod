using System;
using System.Collections.Generic;
using Helpers;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.CampaignBehaviors;

public class TradersCampaignBehavior : CampaignBehaviorBase
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

	protected void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("weaponsmith_talk_start_normal", "start", "weaponsmith_talk_player", "{=!}{TRADER_GREETING}", new OnConditionDelegate(conversation_weaponsmith_talk_start_normal_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("weaponsmith_talk_start_to_player_in_disguise", "start", "close_window", "{=1auLEn9y}Look, my good {?PLAYER.GENDER}woman{?}man{\\?}, these are hard times for sure, but I need you to move along. You'll scare away my customers.", new OnConditionDelegate(conversation_weaponsmith_talk_start_to_player_in_disguise_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("weaponsmith_talk_initial", "weaponsmith_begin", "weaponsmith_talk_player", "{=jxw54Ijt}Okay, is there anything more I can help with?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("weaponsmith_talk_player_1", "weaponsmith_talk_player", "merchant_response_1", "{=ExltvaKo}Let me see what you have for sale...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("weaponsmith_talk_player_request_craft", "weaponsmith_talk_player", "merchant_response_crafting", "{=w1vzpCNi}I need you to craft a weapon for me", new OnConditionDelegate(conversation_open_crafting_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("weaponsmith_talk_player_3", "weaponsmith_talk_player", "merchant_response_3", "{=8hNYr2VX}I was just passing by.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("weaponsmith_talk_merchant_response_1", "merchant_response_1", "player_merchant_talk_close", "{=K5mG9nDv}With pleasure.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("weaponsmith_talk_merchant_response_2", "merchant_response_2", "player_merchant_talk_2", "{=5bRQ0gt7}How many men do you need for it? For each men I want 100{GOLD_ICON}.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("weaponsmith_talk_merchant_response_craft", "merchant_response_crafting", "player_merchant_craft_talk_close", "{=lF5HkBDy}As you wish.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("weaponsmith_talk_merchant_craft_opened", "player_merchant_craft_talk_close", "close_window", "{=TD8Jxn7U}Have a nice day my {?PLAYER.GENDER}lady{?}lord{\\?}.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_weaponsmith_craft_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("weaponsmith_talk_merchant_response_3", "merchant_response_3", "close_window", "{=FpNWdIaT}Yes, of course. Just ask me if there is anything you need.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("weaponsmith_talk_end", "player_merchant_talk_close", "close_window", "{=Yh0danUf}Thank you and good day my {?PLAYER.GENDER}lady{?}lord{\\?}.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_weaponsmith_talk_player_on_consequence), 100, (OnClickableConditionDelegate)null);
	}

	private bool conversation_open_crafting_on_condition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		if (CharacterObject.OneToOneConversationCharacter != null)
		{
			return (int)CharacterObject.OneToOneConversationCharacter.Occupation == 28;
		}
		return false;
	}

	private bool conversation_weaponsmith_talk_start_normal_on_condition()
	{
		if (IsTrader())
		{
			if (Campaign.Current.IsMainHeroDisguised)
			{
				if (Mission.Current.GetMissionBehavior<DisguiseMissionLogic>().ContactAlreadySetCommonCondition() && Hero.MainHero.GetPerkValue(Roguery.SmugglerConnections))
				{
					MBTextManager.SetTextVariable("TRADER_GREETING", "{=bqg2gS7i}Ah, a friend of a friend. How may I help you?", false);
					return true;
				}
				return false;
			}
			MBTextManager.SetTextVariable("TRADER_GREETING", "{=7IxFrati}Greetings my {?PLAYER.GENDER}lady{?}lord{\\?}, how may I help you?", false);
			return true;
		}
		return false;
	}

	private bool conversation_weaponsmith_talk_start_to_player_in_disguise_on_condition()
	{
		if (IsTrader())
		{
			return !conversation_weaponsmith_talk_start_normal_on_condition();
		}
		return false;
	}

	private bool IsTrader()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		if ((int)CharacterObject.OneToOneConversationCharacter.Occupation != 10 && (int)CharacterObject.OneToOneConversationCharacter.Occupation != 11 && (int)CharacterObject.OneToOneConversationCharacter.Occupation != 12)
		{
			return (int)CharacterObject.OneToOneConversationCharacter.Occupation == 4;
		}
		return true;
	}

	private void conversation_weaponsmith_talk_player_on_consequence()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected I4, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		InventoryCategoryType val = (InventoryCategoryType)(-1);
		Occupation occupation = CharacterObject.OneToOneConversationCharacter.Occupation;
		if ((int)occupation != 4)
		{
			switch (occupation - 10)
			{
			default:
				if ((int)occupation == 28)
				{
					val = (InventoryCategoryType)2;
				}
				break;
			case 0:
				val = (InventoryCategoryType)2;
				break;
			case 1:
				val = (InventoryCategoryType)1;
				break;
			case 2:
				val = (InventoryCategoryType)4;
				break;
			}
		}
		else
		{
			val = (InventoryCategoryType)5;
		}
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (Mission.Current != null)
		{
			InventoryScreenHelper.OpenScreenAsTrade(currentSettlement.ItemRoster, (SettlementComponent)(object)currentSettlement.Town, val, (Action)OnInventoryScreenDone);
		}
		else
		{
			InventoryScreenHelper.OpenScreenAsTrade(currentSettlement.ItemRoster, (SettlementComponent)(object)currentSettlement.Town, val, (Action)null);
		}
	}

	private void conversation_weaponsmith_craft_on_consequence()
	{
		CraftingHelper.OpenCrafting(((List<CraftingTemplate>)(object)CraftingTemplate.All)[0], (CraftingState)null);
	}

	private void OnInventoryScreenDone()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			CharacterObject val = (CharacterObject)item.Character;
			if (item.IsHuman && val != null && ((BasicCharacterObject)val).IsHero && val.HeroObject.PartyBelongedTo == MobileParty.MainParty)
			{
				item.UpdateSpawnEquipmentAndRefreshVisuals(Mission.Current.DoesMissionRequireCivilianEquipment ? ((BasicCharacterObject)val).FirstCivilianEquipment : ((BasicCharacterObject)val).FirstBattleEquipment);
			}
		}
	}
}
