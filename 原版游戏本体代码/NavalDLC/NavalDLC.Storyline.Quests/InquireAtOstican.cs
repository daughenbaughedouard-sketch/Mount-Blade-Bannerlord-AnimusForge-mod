using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class InquireAtOstican : QuestBase
{
	[SaveableField(1)]
	private bool _isGunnarSaved;

	private bool _playCutscene;

	private bool _northernerQuestOptions2Answer1Selected;

	private bool _northernerQuestOptions1Selected;

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=GOYpy4gI}Inquire at {SETTLEMENT}", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT", NavalStorylineData.HomeSettlement.Name);
			return val;
		}
	}

	public override bool IsRemainingTimeHidden => true;

	public override string SpecialQuestType => "NavalStoryline";

	private TextObject _questStartLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=JFNtXUF2}You have heard that bandits might be selling captives to pirates on the Vlandian coast, and the port of {SETTLEMENT} might be a good place to start.", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT", NavalStorylineData.HomeSettlement.Name);
			return val;
		}
	}

	private TextObject _isGunnarSavedLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected O, but got Unknown
			TextObject val = new TextObject("{=Rynxrlis}You met {GUNNAR.LINK} after helping him fight off some attackers. He suggested you come on a voyage north with him. Go to the tavern at {SETTLEMENT} and talk to his comrade {NORTHERNER.LINK}.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "GUNNAR", NavalStorylineData.Gunnar.CharacterObject, false);
			TextObjectExtensions.SetCharacterProperties(val, "NORTHERNER", NavalStorylineData.Purig.CharacterObject, false);
			val.SetTextVariable("SETTLEMENT", NavalStorylineData.HomeSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _tutorialSkippedLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			TextObject val = new TextObject("{=3mvfEsqk}You declined to join {GUNNAR.LINK} on his voyage, but may be able to find him later at {SETTLEMENT}.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "GUNNAR", NavalStorylineData.Gunnar.CharacterObject, false);
			val.SetTextVariable("SETTLEMENT", NavalStorylineData.HomeSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _cancelQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			TextObject val = new TextObject("{=nHc1jonU}You decided to stop searching for your sister.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "NORTHERNER", NavalStorylineData.Purig.CharacterObject, false);
			val.SetTextVariable("SETTLEMENT", NavalStorylineData.HomeSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	public InquireAtOstican()
		: base("inquire_at_ostican", (Hero)null, CampaignTime.Never, 0)
	{
	}//IL_0007: Unknown result type (might be due to invalid IL or missing references)


	protected override void OnStartQuest()
	{
		((QuestBase)this).OnStartQuest();
		((QuestBase)this).AddLog(_questStartLog, false);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)NavalStorylineData.HomeSettlement);
	}

	protected override void SetDialogs()
	{
		AddNorthernerDialog();
	}

	protected override void InitializeQuestOnGameLoad()
	{
		if (_isGunnarSaved)
		{
			((QuestBase)this).SetDialogs();
		}
	}

	protected override void RegisterEvents()
	{
		NavalDLCEvents.OnGunnarSavedEvent.AddNonSerializedListener((object)this, (Action)OnGunnarSaved);
		NavalDLCEvents.OnNavalStorylineCanceledEvent.AddNonSerializedListener((object)this, (Action<NavalStorylineData.StorylineCancelDetail>)OnNavalStorylineCanceled);
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		NavalDLCEvents.OnNavalStorylineTutorialSkippedEvent.AddNonSerializedListener((object)this, (Action)OnNavalTutorialSkipped);
	}

	private void OnGunnarSaved()
	{
		_isGunnarSaved = true;
		((QuestBase)this).SetDialogs();
		((QuestBase)this).AddLog(_isGunnarSavedLog, false);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)NavalStorylineData.Purig);
		NavalStorylineData.Gunnar.SetPersonalRelation(Hero.MainHero, 15);
	}

	private void OnNavalTutorialSkipped()
	{
		((QuestBase)this).AddLog(_tutorialSkippedLog, false);
		((QuestBase)this).CompleteQuestWithSuccess();
		NavalStorylineData.Gunnar.SetPersonalRelation(Hero.MainHero, 10);
	}

	private void OnNavalStorylineCanceled(NavalStorylineData.StorylineCancelDetail detail)
	{
		if (NavalStorylineData.Gunnar.IsActive)
		{
			DisableHeroAction.Apply(NavalStorylineData.Gunnar);
			LocationComplex locationComplex = Settlement.CurrentSettlement.LocationComplex;
			Location val = ((locationComplex != null) ? locationComplex.GetLocationOfCharacter(NavalStorylineData.Gunnar) : null);
			if (val != null && val.GetLocationCharacter(NavalStorylineData.Gunnar) != null)
			{
				Settlement.CurrentSettlement.LocationComplex.RemoveCharacterIfExists(NavalStorylineData.Gunnar);
				LocationEncounter locationEncounter = PlayerEncounter.LocationEncounter;
				if (locationEncounter != null)
				{
					locationEncounter.RemoveAccompanyingCharacter(NavalStorylineData.Gunnar);
				}
			}
		}
		((QuestBase)this).CompleteQuestWithFail(_cancelQuestLog);
	}

	protected override void HourlyTick()
	{
	}

	public override IssueQuestFlags IsLocationTrackedByQuest(Location location)
	{
		if (Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement)
		{
			if (_isGunnarSaved)
			{
				if (location.StringId == "tavern" && !location.ContainsCharacter(NavalStorylineData.Purig))
				{
					return (IssueQuestFlags)16;
				}
			}
			else if (location.StringId == "port")
			{
				return (IssueQuestFlags)4;
			}
		}
		return (IssueQuestFlags)0;
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		if (NavalStorylineData.HomeSettlement == settlement && settlement.IsTown && CampaignMission.Current != null)
		{
			Location location = CampaignMission.Current.Location;
			if (location != null && location.StringId == "tavern" && !NavalStorylineData.Purig.IsDead && _isGunnarSaved)
			{
				location.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateNortherner), settlement.Culture, (CharacterRelations)0, 1);
			}
		}
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		if (_playCutscene && GameStateManager.Current.ActiveState is MapState)
		{
			_playCutscene = false;
			VideoPlaybackState val = Game.Current.GameStateManager.CreateState<VideoPlaybackState>();
			string text = ModuleHelper.GetModuleFullPath("NavalDLC") + "Videos/Storyline/";
			string text2 = text + "naval_storyline_intro";
			float num = 24f;
			string text3 = text + "naval_storyline_intro_cinematic.ivf";
			string text4 = text + "naval_storyline_intro_cinematic.ogg";
			val.SetStartingParameters(text3, text4, text2, num, true);
			val.SetOnVideoFinisedDelegate((Action)OnCinematicCompleted);
			Game.Current.GameStateManager.PushState((GameState)(object)val, 0);
		}
	}

	private LocationCharacter CreateNortherner(CultureObject culture, CharacterRelations relation)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		CharacterObject characterObject = NavalStorylineData.Purig.CharacterObject;
		Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)characterObject).Race, "_settlement");
		AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)characterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddCompanionBehaviors), "sp_storyline_npc", true, relation, ActionSetCode.GenerateActionSetNameWithSuffix(val.AgentMonster, val.AgentIsFemale, "_villager"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private void AddNorthernerDialog()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Expected O, but got Unknown
		//IL_0175: Expected O, but got Unknown
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		//IL_01d4: Expected O, but got Unknown
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Expected O, but got Unknown
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Expected O, but got Unknown
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Expected O, but got Unknown
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Expected O, but got Unknown
		DialogFlow val = DialogFlow.CreateDialogFlow("start", 1200);
		val.AddDialogLine("northerner_meet_dialog_start_before_met", "start", "northerner_meet_dialog_player_options", "{=ay0tHozl}Aye? So who're you, then?", new OnConditionDelegate(northerner_meet_dialog_start_before_met_on_condition), (OnConsequenceDelegate)null, (object)this, 1200, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddPlayerLine("northerner_meet_dialog_player_options_1", "northerner_meet_dialog_player_options", "northerner_meet_dialog_continue", "{=HXnni7no}I am {PLAYER.NAME}. Gunnar sent me. We were in a fight.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddPlayerLine("northerner_meet_dialog_player_options_2", "northerner_meet_dialog_player_options", "northerner_meet_dialog_continue", "{=O4kwRlyY}I helped out Gunnar in a fight. He said he planned to sail with you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_meet_dialog_continue_1_line", "northerner_meet_dialog_continue", "northerner_meet_dialogue_continue_2", "{=4K9ycbC8}A fight, you say… I take it that Gunnar and you won?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddPlayerLine("northerner_meet_dialog_continue_2_line", "northerner_meet_dialogue_continue_2", "northerner_meet_dialog_aftermath", "{=uyWWPIxA}Yes, we defeated three Sea Hounds. Now I wish to sail with you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddPlayerLine("northerner_meet_dialog_continue_3_line", "northerner_meet_dialogue_continue_2", "northerner_meet_dialog_aftermath", "{=Ic4e9HVF}We won, and now I wish to join you against our common enemy.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_meet_dialog_aftermath_line_1", "northerner_meet_dialog_aftermath", "northerner_meet_dialog_aftermath_2", "{=Ni7ienXY}Well... Good for you two! Gunnar is a tough old goat and rather hard to kill. I shall have to ask him all about it when I get the chance. So... Yes, I agreed to help him in his little feud with the Sea Hounds, for old time's sake. I've got my ship and men ready to sail.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_meet_dialog_aftermath_line_2", "northerner_meet_dialog_aftermath_2", "northerner_quest_options", "{=0JNfhDrT}If you're indeed of a mind to go with us, I'm happy to take you. But I've got room for only you. So if you've got any traveling companions, you'll need to leave them in this port. I'm sure you'll be back soon to rejoin them, safe and sound.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		OnConsequenceDelegate val2 = northerner_quest_options_1_consequence;
		OnClickableConditionDelegate val3 = new OnClickableConditionDelegate(northerner_quest_options_1_clickable_condition);
		val.AddPlayerLine("northerner_quest_options_1_line", "northerner_quest_options", "northerner_quest_options_1", "{=S1ES8FFM}I'd feel better if my men could come along as well...", (OnConditionDelegate)null, val2, (object)this, 100, val3, (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_quest_options_1_line_continue", "northerner_quest_options_1", "northerner_quest_options", "{=MjIfvPk9}The northern seas aren't for everyone! Even if you had your own ship, it would just slow us down. Don't worry, me and my boys know those waters like the back of our hands. We won't let you slip overboard.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		OnConsequenceDelegate val4 = northerner_quest_options_2_answer_1_consequence;
		val3 = new OnClickableConditionDelegate(northerner_quest_options_2_answer_1_clickable_condition);
		val.AddPlayerLine("northerner_quest_options_2_line", "northerner_quest_options", "northerner_quest_options_2_answer_1", "{=R6CH1xOc}Did you also fight in this rebellion with Gunnar?", (OnConditionDelegate)null, val4, (object)this, 100, val3, (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_quest_options_2_continue_1", "northerner_quest_options_2_answer_1", "northerner_quest_options_2_answer_2", "{=sfuFR9fr}I did, I did. We started out as young men with nothing but our swords, our sweet mistress the sea whispering promises of wealth and glory in our ears... We served no kings and had no lords. Those were fine times!", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_quest_options_2_continue_2", "northerner_quest_options_2_answer_2", "northerner_quest_options_2_answer_3", "{=pGeYLxkL}Then old Volbjorn brought down the full weight of the north on our brotherhood. Against those odds we could not fight. But some of our old comrades weren't quite ready to abandon that life, and they  turned pirate and became the Sea Hounds...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_quest_options_2_continue_3", "northerner_quest_options_2_answer_3", "northerner_quest_options", "{=06hn50KS}Now Gunnar says they are even worse than the king and the jarls we fought, preying upon the farmers and fishermen of the coast. There's no honor in attacking the weak, he told me so many times. And he's right, of course - it's just that it's so much easier to take their wealth!", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddPlayerLine("northerner_quest_options_3_line", "northerner_quest_options", "northerner_quest_options_3_answer", "{=roU1EPwp}Very well. I'll make ready to sail.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, new OnClickableConditionDelegate(CanSetSailWithNortherner), (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_quest_options_3_continue", "northerner_quest_options_3_answer", "close_window", "{=5LbipyXT}Come down to the ship with me, then! Wind and tide are with us, and I won't tarry long.", (OnConditionDelegate)null, new OnConsequenceDelegate(northerner_quest_options_3_continue_on_consequence), (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddPlayerLine("northerner_quest_options_4_line", "northerner_quest_options", "northerner_quest_options_4_answer", "{=18bzzaFH}I'm not ready to sail just yet.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_quest_options_4_continue", "northerner_quest_options_4_answer", "close_window", "{=s9Rz14CU}Are you sure you're cut out for a life at sea? Make haste when wind and tide are with you, friend! Anyway, come back when you're ready.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddDialogLine("northerner_meet_dialog_start_after_met", "start", "northerner_returned_options", "{=b9hRGOhC}All is good? Packed your bag, kissed your mother and your sweetheart good-bye? Of course my lads and I won't mind if you want to tarry here a little longer. Oh no. There's no hurry at all.", new OnConditionDelegate(northerner_meet_dialog_start_after_met_on_condition), (OnConsequenceDelegate)null, (object)this, 1200, (OnClickableConditionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddPlayerLine("northerner_returned_options_1", "northerner_returned_options", "northerner_quest_options_3_answer", "{=nLM7Lu2m}All is good. I am ready to sail.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, new OnClickableConditionDelegate(CanSetSailWithNortherner), (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		val.AddPlayerLine("northerner_returned_options_2", "northerner_returned_options", "northerner_quest_options_4_answer", "{=18bzzaFH}I'm not ready to sail just yet.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, (object)this, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val, (object)null);
	}

	private bool CanSetSailWithNortherner(out TextObject reasonText)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		reasonText = null;
		bool num = NavalStorylineData.IsStorylineActivationPossible();
		if (!num)
		{
			if (MobileParty.MainParty.Army != null)
			{
				reasonText = new TextObject("{=q9fzW0W3}You cannot do this while you are in an army.", (Dictionary<string, object>)null);
				return num;
			}
			if (Campaign.Current.IsMainHeroDisguised)
			{
				reasonText = new TextObject("{=V9Ub68T7}You cannot do this while disguised.", (Dictionary<string, object>)null);
				return num;
			}
			reasonText = new TextObject("{=H6F5BxgB}This isn't the right time.", (Dictionary<string, object>)null);
		}
		return num;
	}

	private bool northerner_meet_dialog_came_back_on_condition()
	{
		if (Hero.OneToOneConversationHero == NavalStorylineData.Purig)
		{
			return Hero.OneToOneConversationHero.HasMet;
		}
		return false;
	}

	private bool northerner_meet_dialog_start_before_met_on_condition()
	{
		StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, (TextObject)null, false);
		if (Hero.OneToOneConversationHero == NavalStorylineData.Purig && !Hero.OneToOneConversationHero.HasMet)
		{
			return NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.None);
		}
		return false;
	}

	private bool northerner_meet_dialog_start_after_met_on_condition()
	{
		StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, (TextObject)null, false);
		if (Hero.OneToOneConversationHero == NavalStorylineData.Purig && Hero.OneToOneConversationHero.HasMet)
		{
			return NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.None);
		}
		return false;
	}

	private void northerner_quest_options_1_consequence()
	{
		_northernerQuestOptions1Selected = true;
	}

	private bool northerner_quest_options_1_clickable_condition(out TextObject explanation)
	{
		explanation = TextObject.GetEmpty();
		return !_northernerQuestOptions1Selected;
	}

	private void northerner_quest_options_2_answer_1_consequence()
	{
		_northernerQuestOptions2Answer1Selected = true;
	}

	private bool northerner_quest_options_2_answer_1_clickable_condition(out TextObject explanation)
	{
		explanation = TextObject.GetEmpty();
		return !_northernerQuestOptions2Answer1Selected;
	}

	private void northerner_quest_options_3_continue_on_consequence()
	{
		Campaign.Current.ConversationManager.ConversationEndOneShot += OnDialogueEnded;
	}

	private void OnDialogueEnded()
	{
		_playCutscene = true;
		Mission current = Mission.Current;
		if (current != null)
		{
			current.EndMission();
		}
	}

	private void OnCinematicCompleted()
	{
		GameStateManager.Current.PopState(0);
		Settlement.CurrentSettlement.LocationComplex.RemoveCharacterIfExists(NavalStorylineData.Purig);
		((QuestBase)this).CompleteQuestWithSuccess();
		((QuestBase)new DefeatTheCaptorsQuest("naval_storyline_defeat_the_captors_quest")).StartQuest();
	}
}
