using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Missions.MissionLogics.Hideout;
using StoryMode.GameComponents.CampaignBehaviors;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.TutorialPhase;

public class FindHideoutTutorialQuest : StoryModeQuestBase
{
	public enum HideoutBattleEndState
	{
		None,
		Retreated,
		Defeated,
		Victory
	}

	private const string RaiderPartyStringId = "radagos_raider_party_";

	private const string TutorialHideoutSceneName = "forest_hideout_003";

	private const int RaiderPartyCount = 2;

	private const int RaiderPartySize = 4;

	private const int MainPartyHealHitPointLimit = 50;

	private const int MaximumHealth = 100;

	private const int PlayerPartySizeMinLimitToAttack = 4;

	[SaveableField(1)]
	private readonly Settlement _hideout;

	[SaveableField(2)]
	private List<MobileParty> _raiderParties;

	private bool _foughtWithRadagos;

	private bool _dueledRadagos;

	[SaveableField(4)]
	private bool _talkedWithRadagos;

	[SaveableField(5)]
	private bool _talkedWithBrother;

	[SaveableField(6)]
	private HideoutBattleEndState _hideoutBattleEndState;

	private List<CharacterObject> _mainPartyTroopBackup;

	private static string _activeHideoutStringId;

	private TextObject _startQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=gSBGpUBm}Find {RADAGOS.LINK}' hideout.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("RADAGOS", StoryModeHeroes.Radagos.CharacterObject, val, false);
			return val;
		}
	}

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=NvkWtb8f}Find the Hideout of {RADAGOS.NAME}' Gang and Defeat Them", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("RADAGOS", StoryModeHeroes.Radagos.CharacterObject, val, false);
			return val;
		}
	}

	public FindHideoutTutorialQuest(Settlement hideout)
		: base("find_hideout_tutorial_quest", null, CampaignTime.Never)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		_hideout = hideout;
		_activeHideoutStringId = ((MBObjectBase)_hideout).StringId;
		_hideout.Party.SetCustomName(new TextObject("{=9xaEPyNV}{RADAGOS.NAME}' Hideout", (Dictionary<string, object>)null));
		StringHelpers.SetCharacterProperties("RADAGOS", StoryModeHeroes.Radagos.CharacterObject, _hideout.Name, false);
		_raiderParties = new List<MobileParty>();
		_foughtWithRadagos = false;
		_talkedWithRadagos = false;
		_talkedWithBrother = false;
		_hideoutBattleEndState = HideoutBattleEndState.None;
		InitializeHideout();
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_hideout);
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		((QuestBase)this).InitializeQuestOnCreation();
		((QuestBase)this).AddLog(_startQuestLog, false);
		StoryMode.StoryModePhases.TutorialPhase.Instance.SetTutorialFocusSettlement(_hideout);
	}

	protected override void HourlyTick()
	{
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
		CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
	}

	public override void OnHeroCanDieInfoIsRequested(Hero hero, KillCharacterActionDetail causeOfDeath, ref bool result)
	{
		if (hero == StoryModeHeroes.Radagos)
		{
			result = false;
		}
	}

	public override void OnHeroCanBeSelectedInInventoryInfoIsRequested(Hero hero, ref bool result)
	{
		if (hero == StoryModeHeroes.Radagos)
		{
			result = false;
		}
	}

	public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
	{
		if (hero == StoryModeHeroes.Radagos)
		{
			result = false;
		}
	}

	protected override void InitializeQuestOnGameLoad()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		_activeHideoutStringId = ((MBObjectBase)_hideout).StringId;
		_hideout.Party.SetCustomName(new TextObject("{=9xaEPyNV}{RADAGOS.NAME}' Hideout", (Dictionary<string, object>)null));
		StringHelpers.SetCharacterProperties("RADAGOS", StoryModeHeroes.Radagos.CharacterObject, _hideout.Name, false);
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		if (_raiderParties.Count <= 2)
		{
			return;
		}
		for (int num = _raiderParties.Count - 1; num >= 0; num--)
		{
			if (_raiderParties[num].MapEvent == null && !_raiderParties[num].IsActive)
			{
				_raiderParties.Remove(_raiderParties[num]);
			}
		}
		int num2 = _raiderParties.Count - 1;
		while (num2 >= 0)
		{
			if (!_raiderParties[num2].IsBanditBossParty && _raiderParties[num2].MapEvent == null)
			{
				if (!_raiderParties[num2].IsActive)
				{
					_raiderParties.Remove(_raiderParties[num2]);
				}
				else
				{
					DestroyPartyAction.Apply((PartyBase)null, _raiderParties[num2]);
				}
			}
			if (_raiderParties.Count > 2)
			{
				num2--;
				continue;
			}
			break;
		}
	}

	protected override void OnStartQuest()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		GameState activeState = GameStateManager.Current.ActiveState;
		MapState val;
		if ((val = (MapState)(object)((activeState is MapState) ? activeState : null)) != null)
		{
			val.Handler.StartCameraAnimation(_hideout.GatePosition, 1f);
		}
	}

	private void InitializeHideout()
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Invalid comparison between Unknown and I4
		_hideout.Hideout.IsSpotted = true;
		_hideout.IsVisible = true;
		if (!_hideout.Hideout.IsInfested)
		{
			for (int i = 0; i < 2; i++)
			{
				if (_hideout.Hideout.IsInfested)
				{
					break;
				}
				_raiderParties.Add(CreateRaiderParty(_raiderParties.Count + 1, isBanditBossParty: false));
			}
		}
		if (!((IEnumerable<MobileParty>)_hideout.Parties).Any((MobileParty p) => p.IsBanditBossParty))
		{
			_raiderParties.Add(CreateRaiderParty(_raiderParties.Count + 1, isBanditBossParty: true));
		}
		foreach (MobileParty item in (List<MobileParty>)(object)_hideout.Parties)
		{
			if (item.IsBanditBossParty)
			{
				int totalRegulars = item.MemberRoster.TotalRegulars;
				item.MemberRoster.Clear();
				if ((int)StoryModeHeroes.Radagos.HeroState != 1)
				{
					StoryModeHeroes.Radagos.ChangeState((CharacterStates)1);
				}
				item.MemberRoster.AddToCounts(StoryModeHeroes.Radagos.CharacterObject, 1, false, 0, 0, true, -1);
				CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("storymode_quest_raider");
				item.MemberRoster.AddToCounts(val, totalRegulars, false, 0, 0, true, -1);
				StoryModeHeroes.Radagos.Heal(100, false);
				break;
			}
		}
	}

	private MobileParty CreateRaiderParty(int number, bool isBanditBossParty)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		MobileParty obj = BanditPartyComponent.CreateBanditParty("radagos_raider_party_" + number, _hideout.OwnerClan, _hideout.Hideout, isBanditBossParty, (PartyTemplateObject)null, _hideout.GatePosition);
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("storymode_quest_raider");
		obj.MemberRoster.AddToCounts(val, 4, false, 0, 0, true, -1);
		obj.Party.SetCustomName(new TextObject("{=u1Pkt4HC}Raiders", (Dictionary<string, object>)null));
		obj.ActualClan = _hideout.OwnerClan;
		obj.Position = _hideout.Position;
		obj.Party.SetCustomOwner(StoryModeHeroes.Radagos);
		obj.Party.SetVisualAsDirty();
		EnterSettlementAction.ApplyForParty(obj, _hideout);
		float num = obj.Party.CalculateCurrentStrength();
		int num2 = (int)(1f * MBRandom.RandomFloat * 20f * num + 50f);
		obj.InitializePartyTrade(num2);
		obj.SetMoveGoToSettlement(_hideout, (NavigationType)1, false);
		EnterSettlementAction.ApplyForParty(obj, _hideout);
		obj.SetPartyUsedByQuest(true);
		return obj;
	}

	protected override void SetDialogs()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected O, but got Unknown
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Expected O, but got Unknown
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Expected O, but got Unknown
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Expected O, but got Unknown
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Expected O, but got Unknown
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Expected O, but got Unknown
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Expected O, but got Unknown
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Expected O, but got Unknown
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Expected O, but got Unknown
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Expected O, but got Unknown
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Expected O, but got Unknown
		StringHelpers.SetCharacterProperties("TACTEOS", StoryModeHeroes.Tacitus.CharacterObject, (TextObject)null, false);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000015).NpcLine(new TextObject("{=R3CnF55p}So... Who's this that comes through my place of business, killing my employees?[if:convo_confused_voice][ib:warrior2]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(bandit_hideout_boss_fight_start_on_condition))
			.PlayerLine(new TextObject("{=itRoeaJf}We heard you took our little brother and sister. Where are they?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(GameTexts.FindText("find_hideout_quest_radagos_conversation_line_1", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=wWLnZ6G4}Since your hunt for your kin is fruitless, how about you clear off and save your own lives? Either that or I force you to lick up all the blood you've spilled here with your tongues. Or... You and I could settle this, one on one.[if:convo_angry_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=ImLQNYWC}Very well - I'll duel you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(bandit_hideout_start_duel_fight_on_consequence))
			.CloseDialog()
			.PlayerOption(new TextObject("{=MMv3hsmI}I don't duel slavers. Men, attack!", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(bandit_hideout_continue_battle_on_clickable_condition))
			.Consequence(new OnConsequenceDelegate(bandit_hideout_continue_battle_on_consequence))
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000010).NpcLine(new TextObject("{=ZhZ7MCeh}Well. I recognize defeat when I see it. If I'm going to be your captive, let me introduce myself. I'm Radagos.[ib:weary2][if:convo_uncomfortable_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(radagos_meeting_conversation_condition))
			.NpcLine(new TextObject("{=w0CUaEU7}You haven't cut my throat yet, which was a wise move. I'm sure I can find a way to be worth more to you alive than dead.[if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=vDRRsed8}You'd better help us get our brother and sister back, or you'll swing from a tree.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(GameTexts.FindText("find_hideout_quest_radagos_conversation_line_2", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(GameTexts.FindText("find_hideout_quest_radagos_conversation_line_3", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=FWSwngVX}Shall we get on the road? Remember - if I drop dead of exhaustion, or drown in some river, that's it for your little dears. I don't expect a cozy palanquin, now, but you'd best not make it too hard a trip for me.[if:convo_uncomfortable_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(radagos_meeting_conversation_consequence))
			.CloseDialog(), (object)this);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000020).NpcLine(new TextObject("{=qp2zYfua}I was hoping to find more treasure here, but I think business wasn't going too well for {RADAGOS.NAME} and his gang.[ib:closed2][if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(brother_farewell_conversation_condition))
			.NpcLine(new TextObject("{=J4qetbZb}I found this strange looking metal piece though. It doesn't look too valuable, but it could be the artifact {TACTEOS.NAME} was talking about. Maybe we can sell it to one of the noble clans for a hefty price.[if:convo_astonished]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=OffNcRby}All right then. Let's get on the road.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(GameTexts.FindText("find_hideout_quest_brother_conversation_line_1", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(GameTexts.FindText("find_hideout_quest_brother_conversation_line_2", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=fp6QBO7l}I'll need to take these men with us. {RADAGOS.NAME} is a slippery one. I don't want him getting away.[if:convo_confused_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=RJ9NbuYr}So you want me to raise the money to ransom the little ones?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=4OUnPjZc}Indeed. You'll have to find a way to do that. Maybe this bronze thing can help.[if:convo_empathic_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=5soUEFEJ}{TACTEOS.NAME} said it could be worth a fortune to the right person, if you manage not to get killed. If he's telling the truth, you must be careful. Never reveal that you have it. Try to understand its value, and how it can be sold.[if:convo_pondering]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=jPKIN2r4}One more thing. When you are talking to nobles and other people of importance, make sure you present yourself as someone from a distant but distinguished family.[if:convo_thinking]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=GVMGXfxS}You can use our family name if you like or make up a new one. You will have a better chance of obtaining an audience with nobles and it'll be easier for me to find you by asking around.[if:convo_normal]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(SelectClanName))
			.NpcLine(GameTexts.FindText("find_hideout_quest_brother_conversation_line_3", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog(), (object)this);
	}

	private bool bandit_hideout_boss_fight_start_on_condition()
	{
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		if (encounteredParty == null || encounteredParty.IsMobile || encounteredParty.MapFaction == null || !encounteredParty.MapFaction.IsBanditFaction)
		{
			return false;
		}
		if (!_foughtWithRadagos && encounteredParty.IsSettlement && encounteredParty.Settlement.IsHideout && encounteredParty.Settlement == _hideout && Mission.Current != null && Mission.Current.GetMissionBehavior<HideoutMissionController>() != null && Hero.OneToOneConversationHero != null)
		{
			return Hero.OneToOneConversationHero == StoryModeHeroes.Radagos;
		}
		return false;
	}

	private void bandit_hideout_start_duel_fight_on_consequence()
	{
		Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutMissionController.StartBossFightDuelMode;
		_dueledRadagos = true;
		_foughtWithRadagos = true;
	}

	private bool bandit_hideout_continue_battle_on_clickable_condition(out TextObject explanation)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		bool flag = false;
		foreach (Agent item in (List<Agent>)(object)Mission.Current.PlayerTeam.ActiveAgents)
		{
			if (!item.IsMount && (object)item.Character != CharacterObject.PlayerCharacter)
			{
				flag = true;
				break;
			}
		}
		explanation = TextObject.GetEmpty();
		if (!flag)
		{
			explanation = new TextObject("{=F9HxO1iS}You don't have any men.", (Dictionary<string, object>)null);
		}
		return flag;
	}

	private void bandit_hideout_continue_battle_on_consequence()
	{
		Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutMissionController.StartBossFightBattleMode;
		_foughtWithRadagos = true;
	}

	private bool radagos_meeting_conversation_condition()
	{
		if (_foughtWithRadagos)
		{
			return Hero.OneToOneConversationHero == StoryModeHeroes.Radagos;
		}
		return false;
	}

	private void radagos_meeting_conversation_consequence()
	{
		StoryModeHeroes.Radagos.SetHasMet();
		_ = StoryModeHeroes.Radagos.PartyBelongedTo;
		DisableHeroAction.Apply(StoryModeHeroes.Radagos);
		_talkedWithRadagos = true;
		Campaign.Current.ConversationManager.ConversationEndOneShot += OpenBrotherConversationMenu;
	}

	private void OpenBrotherConversationMenu()
	{
		GameMenu.ActivateGameMenu("brother_chest_menu");
	}

	private bool brother_farewell_conversation_condition()
	{
		StringHelpers.SetCharacterProperties("RADAGOS", StoryModeHeroes.Radagos.CharacterObject, (TextObject)null, false);
		StringHelpers.SetCharacterProperties("TACTEOS", StoryModeHeroes.Tacitus.CharacterObject, (TextObject)null, false);
		StringHelpers.SetCharacterProperties("LITTLE_BROTHER", StoryModeHeroes.LittleBrother.CharacterObject, (TextObject)null, false);
		StringHelpers.SetCharacterProperties("LITTLE_SISTER", StoryModeHeroes.LittleSister.CharacterObject, (TextObject)null, false);
		if (Hero.OneToOneConversationHero == StoryModeHeroes.ElderBrother)
		{
			return _talkedWithRadagos;
		}
		return false;
	}

	private void SelectClanName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		InformationManager.ShowTextInquiry(new TextInquiryData(((object)new TextObject("{=JJiKk4ow}Select your family name: ", (Dictionary<string, object>)null)).ToString(), string.Empty, true, false, ((object)GameTexts.FindText("str_done", (string)null)).ToString(), (string)null, (Action<string>)OnChangeClanNameDone, (Action)null, false, (Func<string, Tuple<bool, string>>)FactionHelper.IsClanNameApplicable, "", ((object)Clan.PlayerClan.Name).ToString()), false, false);
	}

	private void OnChangeClanNameDone(string newClanName)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		TextObject val = GameTexts.FindText("str_generic_clan_name", (string)null);
		val.SetTextVariable("CLAN_NAME", new TextObject(newClanName, (Dictionary<string, object>)null));
		Clan.PlayerClan.ChangeClanName(val, val);
		Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<BannerEditorState>(), 0);
	}

	private bool OpenBannerSelectionScreen()
	{
		return true;
	}

	private void OnGameMenuOpened(MenuCallbackArgs menuCallbackArgs)
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Expected O, but got Unknown
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Expected O, but got Unknown
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Expected O, but got Unknown
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Invalid comparison between Unknown and I4
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		StoryModeHeroes.Radagos.Heal(StoryModeHeroes.Radagos.MaxHitPoints, false);
		if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement == _hideout && _hideoutBattleEndState == HideoutBattleEndState.None && menuCallbackArgs.MenuContext.GameMenu.StringId != "radagos_hideout" && menuCallbackArgs.MenuContext.GameMenu.StringId != "brother_chest_menu")
		{
			GameMenu.SwitchToMenu("radagos_hideout");
		}
		if (_hideoutBattleEndState == HideoutBattleEndState.Victory && _talkedWithRadagos && menuCallbackArgs.MenuContext.GameMenu.StringId != "brother_chest_menu")
		{
			Campaign.Current.GameMenuManager.SetNextMenu("brother_chest_menu");
		}
		else
		{
			if (_hideoutBattleEndState != HideoutBattleEndState.Defeated && _hideoutBattleEndState != HideoutBattleEndState.Retreated)
			{
				return;
			}
			foreach (MobileParty item in (List<MobileParty>)(object)_hideout.Parties)
			{
				foreach (TroopRosterElement item2 in (List<TroopRosterElement>)(object)item.MemberRoster.GetTroopRoster())
				{
					if (((BasicCharacterObject)item2.Character).IsHero)
					{
						item2.Character.HeroObject.Heal(50 - item2.Character.HeroObject.HitPoints, false);
						continue;
					}
					int elementWoundedNumber = item.MemberRoster.GetElementWoundedNumber(item.MemberRoster.FindIndexOfTroop(item2.Character));
					if (elementWoundedNumber > 0)
					{
						item.MemberRoster.AddToCounts(item2.Character, 0, false, -elementWoundedNumber, 0, true, -1);
					}
				}
				if (!item.IsBanditBossParty && item.MemberRoster.TotalManCount < 4)
				{
					int totalManCount = item.MemberRoster.TotalManCount;
					CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("storymode_quest_raider");
					item.MemberRoster.AddToCounts(val, 4 - totalManCount, false, 0, 0, true, -1);
				}
				if (MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.1.1", 0) && item.IsBanditBossParty && item.MemberRoster.GetTroopCount(StoryModeHeroes.Radagos.CharacterObject) <= 0)
				{
					if ((int)StoryModeHeroes.Radagos.HeroState != 1)
					{
						StoryModeHeroes.Radagos.ChangeState((CharacterStates)1);
					}
					item.MemberRoster.AddToCounts(StoryModeHeroes.Radagos.CharacterObject, 1, false, 0, 0, true, -1);
				}
			}
			if (Hero.MainHero.IsPrisoner)
			{
				EndCaptivityAction.ApplyByPeace(Hero.MainHero, (Hero)null);
				Hero elderBrother = StoryModeHeroes.ElderBrother;
				if (elderBrother.PartyBelongedToAsPrisoner != null)
				{
					EndCaptivityAction.ApplyByPeace(elderBrother, (Hero)null);
				}
				if (!elderBrother.IsActive)
				{
					elderBrother.ChangeState((CharacterStates)1);
				}
				if (elderBrother.PartyBelongedTo == null)
				{
					AddHeroToPartyAction.Apply(elderBrother, MobileParty.MainParty, false);
				}
			}
			if (_hideoutBattleEndState == HideoutBattleEndState.Defeated || _hideoutBattleEndState == HideoutBattleEndState.Retreated)
			{
				TextObject val2 = new TextObject("{=Zq9qXcCk}You are defeated by the {RADAGOS.NAME}' Party, but your brother saved you. It doesn't look like they're going anywhere, though, so you should attack again once you're ready. You must have at least {NUMBER} members in your party. If you don't, go back to {QUEST_VILLAGE} and recruit some more troops.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("RADAGOS", StoryModeHeroes.Radagos.CharacterObject, val2, false);
				val2.SetTextVariable("NUMBER", 4);
				val2.SetTextVariable("QUEST_VILLAGE", Settlement.Find("village_ES3_2").Name);
				InformationManager.ShowInquiry(new InquiryData(((object)((_hideoutBattleEndState == HideoutBattleEndState.Defeated) ? new TextObject("{=FPhWhjq7}Defeated", (Dictionary<string, object>)null) : new TextObject("{=w6Wa3lSL}Retreated", (Dictionary<string, object>)null))).ToString(), ((object)val2).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)delegate
				{
					_hideout.Hideout.IsSpotted = true;
					_hideout.IsVisible = true;
				}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
			if (menuCallbackArgs.MenuContext.GameMenu.StringId == "radagos_hideout" && _hideoutBattleEndState == HideoutBattleEndState.Retreated)
			{
				PlayerEncounter.Finish(true);
			}
			if (_hideoutBattleEndState == HideoutBattleEndState.Defeated || _hideoutBattleEndState == HideoutBattleEndState.Retreated)
			{
				if (Hero.MainHero.HitPoints < 50)
				{
					Hero.MainHero.Heal(50 - Hero.MainHero.HitPoints, false);
				}
				Hero elderBrother2 = StoryModeHeroes.ElderBrother;
				if (elderBrother2.HitPoints < 50)
				{
					elderBrother2.Heal(50 - elderBrother2.HitPoints, false);
				}
				if (elderBrother2.PartyBelongedToAsPrisoner != null)
				{
					EndCaptivityAction.ApplyByPeace(elderBrother2, (Hero)null);
				}
				if (elderBrother2.PartyBelongedTo == null)
				{
					PartyBase.MainParty.MemberRoster.AddToCounts(elderBrother2.CharacterObject, 1, false, 0, 0, true, -1);
				}
			}
			_hideoutBattleEndState = HideoutBattleEndState.None;
			_foughtWithRadagos = false;
			foreach (MobileParty item3 in (List<MobileParty>)(object)_hideout.Parties)
			{
				foreach (TroopRosterElement item4 in (List<TroopRosterElement>)(object)item3.PrisonRoster.GetTroopRoster())
				{
					if (_mainPartyTroopBackup.Contains(item4.Character))
					{
						int num = item3.PrisonRoster.FindIndexOfTroop(item4.Character);
						int elementWoundedNumber2 = item3.PrisonRoster.GetElementWoundedNumber(num);
						int num2 = item3.PrisonRoster.GetTroopCount(item4.Character) - elementWoundedNumber2;
						if (num2 > 0)
						{
							item3.PrisonRoster.AddToCounts(item4.Character, -num2, false, 0, 0, true, -1);
							PartyBase.MainParty.MemberRoster.AddToCounts(item4.Character, num2, false, 0, 0, true, -1);
						}
					}
				}
			}
			_mainPartyTroopBackup?.Clear();
		}
	}

	private void OnMapEventEnded(MapEvent mapEvent)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (_hideoutBattleEndState == HideoutBattleEndState.Victory && !_talkedWithRadagos)
		{
			CampaignMission.OpenConversationMission(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, false, false, false, false, false), new ConversationCharacterData(StoryModeHeroes.Radagos.CharacterObject, (PartyBase)null, true, true, false, false, false, false), "", "", false);
		}
	}

	private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
	{
		if (_raiderParties.Contains(mobileParty))
		{
			_raiderParties.Remove(mobileParty);
		}
	}

	private void OnGameLoadFinished()
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		for (int num = ((List<MobileParty>)(object)_hideout.Parties).Count - 1; num >= 0; num--)
		{
			MobileParty val = ((List<MobileParty>)(object)_hideout.Parties)[num];
			if (val.IsBandit && val.MapEvent == null)
			{
				while (val.MemberRoster.TotalManCount > 4)
				{
					foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)val.MemberRoster.GetTroopRoster())
					{
						if (!((BasicCharacterObject)item.Character).IsHero)
						{
							val.MemberRoster.RemoveTroop(item.Character, 1, default(UniqueTroopDescriptor), 0);
						}
						if (val.MemberRoster.TotalManCount <= 4)
						{
							break;
						}
					}
				}
			}
		}
		while (_hideout.Party.MemberRoster.TotalManCount > 4 && _hideout.Party.MapEvent == null)
		{
			foreach (TroopRosterElement item2 in (List<TroopRosterElement>)(object)_hideout.Party.MemberRoster.GetTroopRoster())
			{
				if (!((BasicCharacterObject)item2.Character).IsHero)
				{
					_hideout.Party.MemberRoster.RemoveTroop(item2.Character, 1, default(UniqueTroopDescriptor), 0);
				}
				if (_hideout.Party.MemberRoster.TotalManCount <= 4)
				{
					break;
				}
			}
		}
	}

	private void AddGameMenus()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0052: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00bc: Expected O, but got Unknown
		//IL_00bc: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00e0: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_0115: Expected O, but got Unknown
		//IL_0115: Expected O, but got Unknown
		StringHelpers.SetCharacterProperties("TACTEOS", StoryModeHeroes.Tacitus.CharacterObject, (TextObject)null, false);
		StringHelpers.SetCharacterProperties("RADAGOS", StoryModeHeroes.Radagos.CharacterObject, (TextObject)null, false);
		((QuestBase)this).AddGameMenu("radagos_hideout", new TextObject("{=z8LQn2Uh}You have arrived at the hideout.", (Dictionary<string, object>)null), new OnInitDelegate(radagos_hideout_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("radagos_hideout", "enter_hideout", new TextObject("{=zxMOqlhs}Attack", (Dictionary<string, object>)null), new OnConditionDelegate(enter_radagos_hideout_condition), new OnConsequenceDelegate(enter_radagos_hideout_on_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("radagos_hideout", "leave_hideout", new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null), new OnConditionDelegate(leave_radagos_hideout_condition), new OnConsequenceDelegate(leave_radagos_hideout_on_consequence), true, -1);
		((QuestBase)this).AddGameMenu("brother_chest_menu", new TextObject("{=bhQ6Jbom}You come across a chest with an old piece of bronze in it. It's so battered and corroded that it could have been anything from a cup to a crown. This must be the chest {TACTEOS.NAME} mentioned to you, that had something to do with 'Neretzes' Folly'.", (Dictionary<string, object>)null), new OnInitDelegate(brother_chest_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("brother_chest_menu", "brother_chest_menu_continue", new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null), new OnConditionDelegate(brother_chest_menu_on_condition), new OnConsequenceDelegate(brother_chest_menu_on_consequence), false, -1);
	}

	private void brother_chest_menu_on_init(MenuCallbackArgs menuCallbackArgs)
	{
		if (_talkedWithBrother)
		{
			_hideoutBattleEndState = HideoutBattleEndState.None;
			PlayerEncounter.Finish(true);
			((QuestBase)this).CompleteQuestWithSuccess();
		}
	}

	private bool brother_chest_menu_on_condition(MenuCallbackArgs menuCallbackArgs)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		menuCallbackArgs.optionLeaveType = (LeaveType)17;
		return ((QuestBase)this).IsOngoing;
	}

	private void brother_chest_menu_on_consequence(MenuCallbackArgs menuCallbackArgs)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_talkedWithBrother = true;
		CampaignMission.OpenConversationMission(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, false, false, false, false, false), new ConversationCharacterData(StoryModeHeroes.ElderBrother.CharacterObject, (PartyBase)null, true, true, false, false, false, false), "", "", false);
	}

	private void radagos_hideout_menu_on_init(MenuCallbackArgs menuCallbackArgs)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		menuCallbackArgs.MenuTitle = new TextObject("{=8OIwHZF1}Hideout", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("RADAGOS", StoryModeHeroes.Radagos.CharacterObject, (TextObject)null, false);
		if (PlayerEncounter.Current == null)
		{
			return;
		}
		MapEvent playerMapEvent = MapEvent.PlayerMapEvent;
		if (playerMapEvent != null)
		{
			if (playerMapEvent.WinningSide == playerMapEvent.PlayerSide)
			{
				if (_dueledRadagos)
				{
					Campaign.Current.CampaignBehaviorManager.GetBehavior<AchievementsCampaignBehavior>()?.OnRadagosDuelWon();
				}
				_hideoutBattleEndState = HideoutBattleEndState.Victory;
			}
			else if ((int)playerMapEvent.WinningSide == -1)
			{
				_hideoutBattleEndState = HideoutBattleEndState.Retreated;
			}
			else
			{
				_hideoutBattleEndState = HideoutBattleEndState.Defeated;
			}
			_dueledRadagos = false;
		}
		if (_hideoutBattleEndState != HideoutBattleEndState.None)
		{
			PlayerEncounter.Update();
		}
	}

	private bool enter_radagos_hideout_condition(MenuCallbackArgs menuCallbackArgs)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		menuCallbackArgs.optionLeaveType = (LeaveType)1;
		if (MobileParty.MainParty.MemberRoster.TotalManCount < 4)
		{
			menuCallbackArgs.IsEnabled = false;
			menuCallbackArgs.Tooltip = new TextObject("{=kaZ1XtDX}You are not strong enough to attack. Recruit more troops from the village.", (Dictionary<string, object>)null);
		}
		if (((QuestBase)this).IsOngoing)
		{
			return _hideoutBattleEndState == HideoutBattleEndState.None;
		}
		return false;
	}

	private void enter_radagos_hideout_on_consequence(MenuCallbackArgs menuCallbackArgs)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		_hideoutBattleEndState = HideoutBattleEndState.None;
		_mainPartyTroopBackup = new List<CharacterObject>();
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)PartyBase.MainParty.MemberRoster.GetTroopRoster())
		{
			if (!((BasicCharacterObject)item.Character).IsHero)
			{
				_mainPartyTroopBackup.Add(item.Character);
			}
		}
		if (!_hideout.Hideout.IsInfested || ((List<MobileParty>)(object)_hideout.Parties).Count < 3)
		{
			InitializeHideout();
		}
		foreach (MobileParty item2 in (List<MobileParty>)(object)_hideout.Parties)
		{
			if (item2.IsBanditBossParty && item2.MemberRoster.Contains(item2.Party.Culture.BanditBoss))
			{
				item2.MemberRoster.RemoveTroop(item2.Party.Culture.BanditBoss, 1, default(UniqueTroopDescriptor), 0);
			}
		}
		if (PlayerEncounter.Battle == null)
		{
			PlayerEncounter.StartBattle();
			PlayerEncounter.Update();
		}
		CampaignMission.OpenHideoutBattleMission("forest_hideout_003", (FlattenedTroopRoster)null, true);
	}

	private bool leave_radagos_hideout_condition(MenuCallbackArgs menuCallbackArgs)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		menuCallbackArgs.optionLeaveType = (LeaveType)16;
		return ((QuestBase)this).IsOngoing;
	}

	private void leave_radagos_hideout_on_consequence(MenuCallbackArgs menuCallbackArgs)
	{
		_hideoutBattleEndState = HideoutBattleEndState.None;
		PlayerEncounter.Finish(true);
	}

	[GameMenuInitializationHandler("radagos_hideout")]
	[GameMenuInitializationHandler("brother_chest_menu")]
	private static void quest_game_menus_on_init_background(MenuCallbackArgs args)
	{
		Settlement val = Settlement.Find(_activeHideoutStringId);
		args.MenuContext.SetBackgroundMeshName(((SettlementComponent)val.Hideout).WaitMeshName);
	}

	protected override void OnCompleteWithSuccess()
	{
		_hideout.Party.SetCustomName((TextObject)null);
		_hideout.Party.SetVisualAsDirty();
		StoryModeHeroes.Radagos.Heal(100, false);
		StoryModeManager.Current.MainStoryLine.CompleteTutorialPhase(isSkipped: false);
	}

	internal static void AutoGeneratedStaticCollectObjectsFindHideoutTutorialQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(FindHideoutTutorialQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_hideout);
		collectedObjects.Add(_raiderParties);
	}

	internal static object AutoGeneratedGetMemberValue_hideout(object o)
	{
		return ((FindHideoutTutorialQuest)o)._hideout;
	}

	internal static object AutoGeneratedGetMemberValue_raiderParties(object o)
	{
		return ((FindHideoutTutorialQuest)o)._raiderParties;
	}

	internal static object AutoGeneratedGetMemberValue_talkedWithRadagos(object o)
	{
		return ((FindHideoutTutorialQuest)o)._talkedWithRadagos;
	}

	internal static object AutoGeneratedGetMemberValue_talkedWithBrother(object o)
	{
		return ((FindHideoutTutorialQuest)o)._talkedWithBrother;
	}

	internal static object AutoGeneratedGetMemberValue_hideoutBattleEndState(object o)
	{
		return ((FindHideoutTutorialQuest)o)._hideoutBattleEndState;
	}
}
