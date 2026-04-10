using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.Quests;

public class DefeatTheCaptorsQuest : NavalStorylineQuestBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConditionDelegate _003C_003E9__22_0;

		public static OnConditionDelegate _003C_003E9__23_0;

		internal bool _003CAddAllyDialog_003Eb__22_0()
		{
			StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, (TextObject)null, false);
			Mission current = Mission.Current;
			NavalStorylineCaptivityMissionController navalStorylineCaptivityMissionController = ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null);
			if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && NavalStorylineData.Gunnar.HasMet && navalStorylineCaptivityMissionController != null)
			{
				return !navalStorylineCaptivityMissionController.WasPlayerKnockedOut;
			}
			return false;
		}

		internal bool _003CAddPlayerUnconsciousAllyDialog_003Eb__23_0()
		{
			StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, (TextObject)null, false);
			Mission current = Mission.Current;
			NavalStorylineCaptivityMissionController navalStorylineCaptivityMissionController = ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null);
			if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && NavalStorylineData.Gunnar.HasMet && navalStorylineCaptivityMissionController != null)
			{
				return navalStorylineCaptivityMissionController.WasPlayerKnockedOut;
			}
			return false;
		}
	}

	private const string EnemyCharacterStringId = "sea_hound_captivity";

	private const string CrewCharacterStringId = "captivity_troops";

	private const float EncounterPositionX = 188f;

	private const float EncounterPositionY = 600f;

	private bool _willProgressStoryline = true;

	public override TextObject Title => new TextObject("{=pyPqiRwR}Break Free of Captivity", (Dictionary<string, object>)null);

	private TextObject _descriptionLogText => new TextObject("{=l315rexF}Defeat your captors, then free Gunnar and the others.", (Dictionary<string, object>)null);

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act1;

	public override bool WillProgressStoryline => _willProgressStoryline;

	protected override string MainPartyTemplateStringId => "storyline_act1_captivity_template";

	public DefeatTheCaptorsQuest(string questId)
		: base(questId, Hero.MainHero, CampaignTime.Never, 0)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		((QuestBase)this).SetDialogs();
		((QuestBase)this).AddLog(_descriptionLogText, false);
	}

	protected override void SetDialogs()
	{
		AddAllyDialog();
		AddPlayerUnconsciousAllyDialog();
	}

	protected override void InitializeQuestOnGameLoadInternal()
	{
		base.InitializeQuestOnGameLoadInternal();
		((QuestBase)this).SetDialogs();
	}

	protected override void OnStartQuestInternal()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		base.OnStartQuestInternal();
		_willProgressStoryline = false;
		TextObject val = new TextObject("{=ATA1PShK}Purig's Party", (Dictionary<string, object>)null);
		Clan randomElementInefficiently = Extensions.GetRandomElementInefficiently<Clan>(Clan.BanditFactions);
		MobileParty obj = CustomPartyComponent.CreateCustomPartyWithTroopRoster(NavalStorylineData.HomeSettlement.GatePosition, 4f, NavalStorylineData.HomeSettlement, val, randomElementInefficiently, TroopRoster.CreateDummyTroopRoster(), TroopRoster.CreateDummyTroopRoster(), (Hero)null, "", "", 0f, false);
		Ship val2 = new Ship(MBObjectManager.Instance.GetObject<ShipHull>("nord_medium_ship"));
		ChangeShipOwnerAction.ApplyByMobilePartyCreation(obj.Party, val2);
		CampaignVec2 sailAtPosition = default(CampaignVec2);
		((CampaignVec2)(ref sailAtPosition))._002Ector(new Vec2(188f, 600f), false);
		obj.SetSailAtPosition(sailAtPosition);
		MobileParty.MainParty.SetSailAtPosition(sailAtPosition);
		PlayerEncounter.RestartPlayerEncounter(obj.Party, PartyBase.MainParty, false);
		PlayerEncounter.StartBattle();
		GameMenu.ActivateGameMenu("defeat_the_captors_after_fight");
		StartMission();
	}

	public void StartMission()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		CharacterObject enemyCharacter = MBObjectManager.Instance.GetObject<CharacterObject>("sea_hound_captivity");
		CharacterObject crewCharacter = MBObjectManager.Instance.GetObject<CharacterObject>("captivity_troops");
		NavalMissions.OpenNavalStorylineCaptivityMission(NavalStorylineData.GetNavalMissionInitializerTemplate("naval_storyline_act_1_phase_03"), NavalStorylineData.Gunnar.CharacterObject, enemyCharacter, crewCharacter);
	}

	protected override void HourlyTick()
	{
	}

	protected override void RegisterEventsInternal()
	{
	}

	private void AddAllyDialog()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=qtQXIguv}Well done, {PLAYER.NAME}! That's twice now you've gotten me out of a bad spot.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__22_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = delegate
			{
				StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, (TextObject)null, false);
				Mission current = Mission.Current;
				NavalStorylineCaptivityMissionController navalStorylineCaptivityMissionController = ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null);
				return Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && NavalStorylineData.Gunnar.HasMet && navalStorylineCaptivityMissionController != null && !navalStorylineCaptivityMissionController.WasPlayerKnockedOut;
			};
			_003C_003Ec._003C_003E9__22_0 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Condition((OnConditionDelegate)obj2).NpcLine("{=utFgkzhx}Well… Normally I'd say we put as much distance between us and Purig as quickly as we can, but those merchants are still out there floundering in the waves. We can't leave them there. I can get the sail up. Take the steering oar. Let's see if we can  get them out of the water.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Consequence((OnConsequenceDelegate)delegate
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += OnDialogueEnded;
		})
			.CloseDialog(), (object)this);
	}

	private void AddPlayerUnconsciousAllyDialog()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=nQJohWdO}Are you all right, {PLAYER.NAME}? Don't worry, the rest of us managed to break free and took care of those bastards.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__23_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = delegate
			{
				StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, (TextObject)null, false);
				Mission current = Mission.Current;
				NavalStorylineCaptivityMissionController navalStorylineCaptivityMissionController = ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null);
				return Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && NavalStorylineData.Gunnar.HasMet && navalStorylineCaptivityMissionController != null && navalStorylineCaptivityMissionController.WasPlayerKnockedOut;
			};
			_003C_003Ec._003C_003E9__23_0 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Condition((OnConditionDelegate)obj2).NpcLine("{=evfMsY6h}Well… Normally I'd say we put as much distance between us and Purig as quickly as we can, but those merchants are still out there floundering in the waves. We can't leave them there. I can get the sail up. Take the steering oar. Let's see if we can't get them out of the water.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Consequence((OnConsequenceDelegate)delegate
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += OnDialogueEnded;
		})
			.CloseDialog(), (object)this);
	}

	private void OnDialogueEnded()
	{
		Mission.Current.GetMissionBehavior<NavalStorylineCaptivityMissionController>().OnShipCaptured();
		((QuestBase)this).CompleteQuestWithSuccess();
		MobileParty.MainParty.MemberRoster.Clear();
		CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>("captivity_troops");
		MobileParty.MainParty.AddElementToMemberRoster(val, 7, false);
		MobileParty.MainParty.AddElementToMemberRoster(Hero.MainHero.CharacterObject, 1, true);
		MobileParty.MainParty.AddElementToMemberRoster(NavalStorylineData.Gunnar.CharacterObject, 1, false);
		MobileParty.MainParty.PartyComponent.ChangePartyLeader(Hero.MainHero);
		MobileParty.MainParty.IgnoreForHours(16f);
		((QuestBase)new SaveTheCrewmenQuest("naval_storyline_save_the_crewmen_quest", NavalStorylineData.Gunnar)).StartQuest();
	}
}
