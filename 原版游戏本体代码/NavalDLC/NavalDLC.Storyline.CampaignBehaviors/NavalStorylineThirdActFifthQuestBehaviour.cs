using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Storyline.MissionControllers;
using NavalDLC.Storyline.Quests;
using StoryMode;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.CampaignBehaviors;

public class NavalStorylineThirdActFifthQuestBehaviour : CampaignBehaviorBase
{
	public enum NavalStorylineFinalQuestState
	{
		TalkWithGunnarAtPort,
		GunnarWaitsForAnAnswer,
		Quest5IsInProgress,
		TalkWithGunnarAfterFight,
		SpeakToGunnarAndSister,
		End
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConsequenceDelegate _003C_003E9__16_5;

		public static Func<Village, bool> _003C_003E9__18_0;

		internal void _003CAddDialogs_003Eb__16_5()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
		}

		internal bool _003CMakeGunnarNotable_003Eb__18_0(Village x)
		{
			return ((MBObjectBase)((SettlementComponent)x).Settlement).StringId == "village_N1_2";
		}
	}

	private const string QuestConversationMenuId = "naval_storyline_act_3_quest_5_conversation_menu";

	private const string GunnarsLongshipStringId = "northern_medium_ship";

	private const string Tier3NordInfantryStringId = "nord_spear_warrior";

	private const string Tier4NordInfantryStringId = "nord_vargr";

	private const int Tier3NordInfantryCount = 10;

	private const int Tier4NordInfantryCount = 10;

	private NavalStorylineFinalQuestState _navalStorylineFinalQuestState;

	private Quest5SetPieceBattleMissionController.BossFightOutComeEnum _bossFightOutCome;

	private bool _isQuestAcceptedThroughMission;

	private readonly float _strengthModifier = 1f;

	public override void RegisterEvents()
	{
		if (!NavalStorylineData.IsNavalStorylineCanceled())
		{
			CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnAfterSessionLaunched);
			CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
			CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
			CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
		}
	}

	private void OnGameLoadFinished()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		NavalStorylineData.NavalStorylineStage storylineStage = NavalStorylineData.GetStorylineStage();
		if (storylineStage == NavalStorylineData.NavalStorylineStage.Act3Quest4 && Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(FreeTheSeaHoundsCaptivesQuest)))
		{
			_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.Quest5IsInProgress;
		}
		else if (storylineStage == NavalStorylineData.NavalStorylineStage.Act3Quest5 && Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(ReturnToBaseQuest)))
		{
			_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.SpeakToGunnarAndSister;
		}
		else if (storylineStage >= NavalStorylineData.NavalStorylineStage.Act3Quest5)
		{
			_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.End;
		}
		if (MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.3.14", 0) && _navalStorylineFinalQuestState == NavalStorylineFinalQuestState.Quest5IsInProgress && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(FreeTheSeaHoundsCaptivesQuest)))
		{
			_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.TalkWithGunnarAtPort;
		}
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		if (args.MenuContext.GameMenu.StringId == "naval_storyline_outside_town" && _navalStorylineFinalQuestState > NavalStorylineFinalQuestState.Quest5IsInProgress)
		{
			GameMenu.SwitchToMenu("naval_storyline_finalize_menu");
		}
		if (_navalStorylineFinalQuestState <= NavalStorylineFinalQuestState.Quest5IsInProgress && NavalStorylineData.IsStorylineActivationPossible() && NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest4) && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(FreeTheSeaHoundsCaptivesQuest)) && Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement && !Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)NavalStorylineData.Gunnar))
		{
			Campaign.Current.VisualTrackerManager.RegisterObject((ITrackableCampaignObject)(object)NavalStorylineData.Gunnar);
		}
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		if ((int)detail == 1 && quest is CaptureTheImperialMerchantPrusas)
		{
			_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.TalkWithGunnarAtPort;
		}
		else if (quest is FreeTheSeaHoundsCaptivesQuest)
		{
			if ((int)detail == 1)
			{
				NavalStorylineData.DeactivateNavalStoryline();
				_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.TalkWithGunnarAfterFight;
				_bossFightOutCome = ((FreeTheSeaHoundsCaptivesQuest)(object)quest).BossFightOutCome;
			}
			else
			{
				_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.TalkWithGunnarAtPort;
			}
		}
		else if ((int)detail == 1 && quest is SpeakToGunnarAndSisterQuest)
		{
			_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.End;
		}
	}

	private void OnAfterSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		if (StoryModeManager.Current != null)
		{
			AddDialogs();
			AddGameMenus(campaignGameStarter);
		}
	}

	private void AddDialogs()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Expected O, but got Unknown
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Expected O, but got Unknown
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		DialogFlow val = DialogFlow.CreateDialogFlow("start", 1200).NpcLine(new TextObject("{=jWDBinsb}Well... Here we are. Ready to set sail for Angranfjord and settle accounts with our enemies, once and for all. Lahar will sail with us, and Bjolgur, and more of his brothers may join us at our destination. We have Crusas' ship – and Crusas too of course, much as he might not like it – and hopefully the element of surprise. We just need to consider how to turn this best to our advantage.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _navalStorylineFinalQuestState == NavalStorylineFinalQuestState.TalkWithGunnarAtPort && Quest5ConversationStartCondition()))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=el44RZG4}Let us set out, then.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				if (Mission.Current == null)
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += ActivateQuest5;
				}
				else
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
				}
				_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.Quest5IsInProgress;
			})
			.CloseDialog()
			.PlayerOption(new TextObject("{=a0j86F9C}I need a bit more time.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.GunnarWaitsForAnAnswer;
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			})
			.CloseDialog()
			.PlayerOption("{=aEKNUI45}This war on the Sea Hounds is too risky. There must be another way to get my sister back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_ransom_sister")
			.EndPlayerOptions();
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200).NpcLine(new TextObject("{=0Y3S817q}Are you ready to sail to the Angranfjord to carry out our plan? Purig may not be waiting there for much longer.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => _navalStorylineFinalQuestState == NavalStorylineFinalQuestState.GunnarWaitsForAnAnswer && Quest5ConversationStartCondition()))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=qcYkbX2a}Let us sail.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				if (Mission.Current == null)
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += ActivateQuest5;
				}
				else
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += OnPlayerAcceptsQuestThroughMission;
				}
				_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.Quest5IsInProgress;
			})
			.CloseDialog()
			.PlayerOption(new TextObject("{=4LhjHfSY}I am still not ready.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__16_5;
		if (obj2 == null)
		{
			OnConsequenceDelegate val2 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += NavalStorylineData.OnPlayerPostponedQuestStart;
			};
			_003C_003Ec._003C_003E9__16_5 = val2;
			obj2 = (object)val2;
		}
		DialogFlow val3 = obj.Consequence((OnConsequenceDelegate)obj2).CloseDialog().PlayerOption("{=aEKNUI45}This war on the Sea Hounds is too risky. There must be another way to get my sister back.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_ransom_sister")
			.EndPlayerOptions();
		TextObject val4 = new TextObject("{=7SzwQ5NK}{PLAYER.NAME}, welcome! I've been entertaining the village with tales of our adventurers. If you're looking for recruits, then I doubt you'll find a more promising batch than the lads of Lagsholfn. You always have a place by my hearth, old friend.", (Dictionary<string, object>)null);
		TextObject val5 = new TextObject("{=dV5ai0PF}Well, {PLAYER.NAME}... Alas, you appear to have made some enemies here. I do not know if what they say is true, and at any rate, I will never raise a hand against you. But I do not think it is good for you to stay here just now.", (Dictionary<string, object>)null);
		DialogFlow val6 = DialogFlow.CreateDialogFlow("start", 1200).BeginNpcOptions((string)null, false).NpcOption(val4, (OnConditionDelegate)delegate
		{
			if (GunnarNotableConditions())
			{
				Settlement currentSettlement = NavalStorylineData.Gunnar.CurrentSettlement;
				if (currentSettlement == null)
				{
					return false;
				}
				Hero owner = currentSettlement.Owner;
				return ((owner != null) ? new float?(owner.GetRelationWithPlayer()) : ((float?)null)) >= 0f;
			}
			return false;
		}, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("lord_start")
			.NpcOption(val5, (OnConditionDelegate)delegate
			{
				if (GunnarNotableConditions())
				{
					Settlement currentSettlement = NavalStorylineData.Gunnar.CurrentSettlement;
					if (currentSettlement == null)
					{
						return false;
					}
					Hero owner = currentSettlement.Owner;
					return ((owner != null) ? new float?(owner.GetRelationWithPlayer()) : ((float?)null)) < 0f;
				}
				return false;
			}, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("lord_start")
			.EndNpcOptions();
		DialogFlow val7 = DialogFlow.CreateDialogFlow("start", 1500).NpcLine("{=!}{GUNNAR_FINAL_DIALOG_LINE_1}", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
		{
			int num;
			if (_navalStorylineFinalQuestState == NavalStorylineFinalQuestState.TalkWithGunnarAfterFight)
			{
				num = ((Hero.OneToOneConversationHero == NavalStorylineData.Gunnar) ? 1 : 0);
				if (num != 0)
				{
					DecideGunnarDialogue();
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		})
			.NpcLine("{=!}{GUNNAR_FINAL_DIALOG_LINE_2}", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=xxxjoDxM}My men, though... I've had a word with them, and some of them have been quite impressed by your leadership. They want to follow you, if you'll have them. And as I mentioned, they prefer to sail on our ship here, the Wave-Steed, so I guess that's yours too, if you'll have it. She'll carry you well, especially in the rough seas of the north.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=qatVcvrX}I welcome your ship and crew.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(OnPlayerWelcomedGunnarsCrew))
			.GotoDialogState("gunnar_final_dialog_token_1")
			.PlayerOption("{=FaZ1dSuh}I am honored, but I cannot take on your companions.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_final_dialog_token_1")
			.EndPlayerOptions()
			.NpcLine("{=!}{GUNNAR_FINAL_DIALOG_LINE_3}", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, "gunnar_final_dialog_token_1", (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=uh2W7Jh3}Farewell. Perhaps I will take you up on your reputation.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_final_dialog_token_2")
			.PlayerOption("{=C94hXQp3}Farewell, and good hunting.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_final_dialog_token_2")
			.EndPlayerOptions()
			.NpcLine("{=Vcr7BYxJ}Farewell, {PLAYER.NAME}.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, "gunnar_final_dialog_token_2", (string)null)
			.Consequence(new OnConsequenceDelegate(GunnarConversationOnConsequence))
			.CloseDialog();
		Campaign.Current.ConversationManager.AddDialogFlow(val, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val3, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val6, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val7, (object)null);
	}

	private void GunnarConversationOnConsequence()
	{
		NavalDLCHelpers.AddSisterToClan();
		MakeGunnarNotable();
		_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.End;
	}

	private void MakeGunnarNotable()
	{
		Village val = ((IEnumerable<Village>)Village.All).FirstOrDefault((Func<Village, bool>)((Village x) => ((MBObjectBase)((SettlementComponent)x).Settlement).StringId == "village_N1_2"));
		if (val != null)
		{
			TeleportHeroAction.ApplyImmediateTeleportToSettlement(NavalStorylineData.Gunnar, ((SettlementComponent)val).Settlement);
		}
	}

	private void OnPlayerAcceptsQuestThroughMission()
	{
		_isQuestAcceptedThroughMission = true;
		OpenQuestMenu();
		Mission.Current.EndMission();
	}

	private void OpenQuestMenu()
	{
		GameMenu.ActivateGameMenu("naval_storyline_act_3_quest_5_conversation_menu");
	}

	private void AddGameMenus(CampaignGameStarter starter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_006f: Expected O, but got Unknown
		starter.AddGameMenu("naval_storyline_act_3_quest_5_conversation_menu", string.Empty, new OnInitDelegate(naval_storyline_act_3_quest_5_conversation_menu_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		starter.AddGameMenu("naval_storyline_finalize_menu", "{=l1VpTx3x}You have returned to Ostican harbor. Word spreads fast among seafolk, and a trading ship leaving the harbor dips its oars in salute to your victory. As the crews of your ships come ashore, they are clapped on the back by the local fishermen and dock workers and taken to the taverns to drink to the demise of the Sea Hounds.", new OnInitDelegate(naval_storyline_finalize_menu_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		starter.AddGameMenuOption("naval_storyline_finalize_menu", "naval_storyline_finalize_menu_continue_option", "{=DM6luo3c}Continue", new OnConditionDelegate(naval_storyline_finalize_menu_continue_option_on_condition), new OnConsequenceDelegate(naval_storyline_finalize_menu_continue_option_on_consequence), false, -1, false, (object)null);
	}

	private void naval_storyline_act_3_quest_5_conversation_menu_on_init(MenuCallbackArgs args)
	{
		if (_isQuestAcceptedThroughMission && Mission.Current == null)
		{
			ActivateQuest5();
			_isQuestAcceptedThroughMission = false;
		}
	}

	private void naval_storyline_finalize_menu_on_init(MenuCallbackArgs args)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (_navalStorylineFinalQuestState == NavalStorylineFinalQuestState.TalkWithGunnarAfterFight)
		{
			ConversationCharacterData val = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, true, false, false, false, true);
			ConversationCharacterData val2 = default(ConversationCharacterData);
			((ConversationCharacterData)(ref val2))._002Ector(NavalStorylineData.Gunnar.CharacterObject, PartyBase.MainParty, true, true, false, true, false, true);
			CampaignMission.OpenConversationMission(val, val2, "conversation_scene_sea_multi_agent", "", true);
		}
		GameState activeState = Game.Current.GameStateManager.ActiveState;
		MapState val3 = (MapState)(object)((activeState is MapState) ? activeState : null);
		if (val3 != null)
		{
			val3.Handler.TeleportCameraToMainParty();
		}
		string backgroundMeshName = ((MBObjectBase)Settlement.CurrentSettlement.Culture).StringId + "_port";
		args.MenuContext.SetBackgroundMeshName(backgroundMeshName);
		args.MenuContext.SetAmbientSound("event:/map/ambient/node/settlements/2d/port");
	}

	private bool naval_storyline_finalize_menu_continue_option_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)41;
		return true;
	}

	private void naval_storyline_finalize_menu_continue_option_on_consequence(MenuCallbackArgs args)
	{
		if (_navalStorylineFinalQuestState == NavalStorylineFinalQuestState.SpeakToGunnarAndSister && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SpeakToGunnarAndSisterQuest)))
		{
			((QuestBase)new SpeakToGunnarAndSisterQuest(_bossFightOutCome)).StartQuest();
		}
		Settlement val = Settlement.CurrentSettlement ?? PlayerEncounter.EncounterSettlement;
		bool flag = default(bool);
		bool flag2 = default(bool);
		GameMenu.SwitchToMenu(MobileParty.MainParty.HasNavalNavigationCapability ? "naval_town_outside" : Campaign.Current.Models.EncounterGameMenuModel.GetEncounterMenu(PartyBase.MainParty, val.Party, ref flag, ref flag2));
	}

	private void ActivateQuest5()
	{
		if (!Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(FreeTheSeaHoundsCaptivesQuest)))
		{
			Campaign.Current.VisualTrackerManager.RemoveTrackedObject((ITrackableBase)(object)NavalStorylineData.Gunnar, false);
			((QuestBase)new FreeTheSeaHoundsCaptivesQuest("naval_storyline_act3_quest5_1", _strengthModifier)).StartQuest();
			_navalStorylineFinalQuestState = NavalStorylineFinalQuestState.Quest5IsInProgress;
		}
	}

	private bool Quest5ConversationStartCondition()
	{
		if (NavalStorylineData.IsStorylineActivationPossible() && NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest4) && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(FreeTheSeaHoundsCaptivesQuest)) && Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement)
		{
			return Hero.OneToOneConversationHero == NavalStorylineData.Gunnar;
		}
		return false;
	}

	private bool GunnarNotableConditions()
	{
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
		if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && !NavalStorylineData.IsNavalStoryLineActive())
		{
			return NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest5);
		}
		return false;
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<NavalStorylineFinalQuestState>("_navalStorylineFinalQuestState", ref _navalStorylineFinalQuestState);
		dataStore.SyncData<Quest5SetPieceBattleMissionController.BossFightOutComeEnum>("_bossFightOutCome", ref _bossFightOutCome);
	}

	public Quest5SetPieceBattleMissionController.BossFightOutComeEnum GetBossFightOutcome()
	{
		return _bossFightOutCome;
	}

	private void OnPlayerWelcomedGunnarsCrew()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		Ship val = new Ship(MBObjectManager.Instance.GetObject<ShipHull>("northern_medium_ship"));
		val.SetName(new TextObject("{=EUAsSTeT}Wave-Steed", (Dictionary<string, object>)null));
		ChangeShipOwnerAction.ApplyByLooting(PartyBase.MainParty, val);
		CharacterObject val2 = MBObjectManager.Instance.GetObject<CharacterObject>("nord_spear_warrior");
		MobileParty.MainParty.MemberRoster.AddToCounts(val2, 10, false, 0, 0, true, -1);
		CharacterObject val3 = MBObjectManager.Instance.GetObject<CharacterObject>("nord_vargr");
		MobileParty.MainParty.MemberRoster.AddToCounts(val3, 10, false, 0, 0, true, -1);
		if (!MobileParty.MainParty.Anchor.IsValid && Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.HasPort)
		{
			MobileParty.MainParty.Anchor.SetSettlement(Settlement.CurrentSettlement);
		}
		TextObject val4 = new TextObject("{=06sIBlHR}{NUMBER} troops and {SHIP_NAME} were added to your party.", (Dictionary<string, object>)null);
		val4.SetTextVariable("NUMBER", 20);
		val4.SetTextVariable("SHIP_NAME", val.Name);
		InformationManager.DisplayMessage(new InformationMessage(((object)val4).ToString(), new Color(0f, 1f, 0f, 1f)));
	}

	private void DecideGunnarDialogue()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		TextObject val;
		TextObject val2;
		if (_bossFightOutCome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerRefusedTheDuel)
		{
			val = new TextObject("{=dI8a424b}Well then... Your sister is free, thank the gods. You gave Purig the death he deserved. None will mourn him. And the Sea Hounds... Well, I doubt they'll recover from the thrashing we gave them today. The north will thank you.", (Dictionary<string, object>)null);
			val2 = new TextObject("{=UAq8cW8O}Now, I think, I will go ashore, and make my way home. Lagshofn is not far from here. I've settled what I wish to settle, and all this rowing and ramming and climbing and jostling and fighting is hard on my old bones.", (Dictionary<string, object>)null);
		}
		else if (_bossFightOutCome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedAndWonTheDuel)
		{
			val = new TextObject("{=0TP1KQLE}Well then... Your sister is free, thank the gods. You put an end to the Sea Hounds, and gave Purig a far more honorable death than he deserved. Men will speak well of you.", (Dictionary<string, object>)null);
			val2 = new TextObject("{=UAq8cW8O}Now, I think, I will go ashore, and make my way home. Lagshofn is not far from here. I've settled what I wish to settle, and all this rowing and ramming and climbing and jostling and fighting is hard on my old bones.", (Dictionary<string, object>)null);
		}
		else if (_bossFightOutCome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndLetPurigGo)
		{
			val = new TextObject("{=XDzsJmMP}Well then...  Your sister is free, thank the gods. Purig may have gotten away, but I doubt the Sea Hounds will be troubling us much more.", (Dictionary<string, object>)null);
			val2 = new TextObject("{=dPaN65B1}It was an honorable thing, to duel him, and I am glad you kept your word to him, though he did not deserve it. For my part, though, I owe him nothing. I continue to hunt him, here in Beinland, and as it is much easier for him to evade a large group than a single hunter, I will do so alone.", (Dictionary<string, object>)null);
		}
		else
		{
			val = new TextObject("{=8j3z1dBZ}Well then... Your sister is free, thank the gods. Purig is dead, and none will mourn him. I might wish that his death could have come some other way, but I will not dwell on it.", (Dictionary<string, object>)null);
			val2 = new TextObject("{=UAq8cW8O}Now, I think, I will go ashore, and make my way home. Lagshofn is not far from here. I've settled what I wish to settle, and all this rowing and ramming and climbing and jostling and fighting is hard on my old bones.", (Dictionary<string, object>)null);
		}
		TextObject val3 = ((_bossFightOutCome != Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndLetPurigGo) ? new TextObject("{=IGnbxJHn}You should come see me in my village, Lagshofn, in Beinland. It's not much, not for a {?PLAYER.GENDER}warrior{?}man{\\?} like you, who's no doubt seen all the wonders of the Empire and the lands beyond, but we can pass a summer's night on the beach and drink to our deeds.", (Dictionary<string, object>)null) : new TextObject("{=1PPiv2ns}I suspect Purig will try to travel as far from these parts as possible. Perhaps deep into the south, or to the east... Perhaps I will take years to find him, or perhaps my old age will finally catch up to me on the road or on the seas. I do not know if we will meet again.", (Dictionary<string, object>)null));
		MBTextManager.SetTextVariable("GUNNAR_FINAL_DIALOG_LINE_1", val, false);
		MBTextManager.SetTextVariable("GUNNAR_FINAL_DIALOG_LINE_2", val2, false);
		MBTextManager.SetTextVariable("GUNNAR_FINAL_DIALOG_LINE_3", val3, false);
	}
}
