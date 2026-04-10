using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Missions.MissionLogics.Hideout;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
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

namespace StoryMode.Quests.SecondPhase.ConspiracyQuests;

public class ConspiracyBaseOfOperationsDiscoveredConspiracyQuest : ConspiracyQuestBase
{
	private const string AntiImperialHideoutBossStringId = "anti_imperial_conspiracy_boss";

	private const string ImperialHideoutBossStringId = "imperial_conspiracy_boss";

	private const int RaiderPartySize = 6;

	private const int RaiderPartyCount = 2;

	private const float DefaultConspiracyReductionAmount = 50f;

	[SaveableField(1)]
	private readonly Settlement _hideout;

	private Settlement _baseLocation;

	private bool _dueledWithHideoutBoss;

	private bool _isSuccess;

	private float _conspiracyStrengthDecreaseAmount;

	[SaveableField(2)]
	private readonly List<MobileParty> _raiderParties;

	public override TextObject Title => new TextObject("{=3Pq58i2u}Conspiracy Base of Operations Discovered", (Dictionary<string, object>)null);

	public override TextObject SideNotificationText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=aY4zWYpg}You have have received an important message from {MENTOR.LINK}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", base.Mentor.CharacterObject, val, false);
			return val;
		}
	}

	public override TextObject StartMessageLogFromMentor
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=XQrmVPKL}{PLAYER.LINK} I hope this letter finds you well. I have learned from a spy in {LOCATION_LINK} that our adversaries have set up a camp in its environs. She could not tell me what they plan to do, but if you raided the camp, stole some of their supplies, and brought it back to me, we could get some idea of their wicked intentions. Search around {LOCATION_LINK} to find the hideout.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
			val.SetTextVariable("LOCATION_LINK", _baseLocation.EncyclopediaLinkWithName);
			return val;
		}
	}

	public override TextObject StartLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=rTYNL1LB}{MENTOR.LINK} told you about a group of conspirators operating in a hideout in the vicinity of {LOCATION_LINK}. You should go there and raid the hideout with a small group of fighters and take the bandits by surprise.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", base.Mentor.CharacterObject, val, false);
			val.SetTextVariable("LOCATION_LINK", _baseLocation.EncyclopediaLinkWithName);
			return val;
		}
	}

	public override float ConspiracyStrengthDecreaseAmount => _conspiracyStrengthDecreaseAmount;

	private TextObject HideoutBossName
	{
		get
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			MobileParty val = ((IEnumerable<MobileParty>)_hideout.Parties).FirstOrDefault((Func<MobileParty, bool>)((MobileParty p) => p.IsBanditBossParty));
			if (val == null || val.MemberRoster.TotalManCount <= 0)
			{
				return new TextObject("{=izCbZEZg}Conspiracy Commander{%Commander is male.}", (Dictionary<string, object>)null);
			}
			return ((BasicCharacterObject)val.MemberRoster.GetCharacterAtIndex(0)).Name;
		}
	}

	private TextObject HideoutSpottedLog => new TextObject("{=nrdl5QaF}My spy spotted some conspirators at the camp, and some local bandits have joined them. My spy does not know if they are expecting an attack, so I implore you to be cautious and to be ready for anything. Needless to say, I'm sure you will send any documents you can find to me so I can study them. Go quickly and return safely.", (Dictionary<string, object>)null);

	private TextObject HideoutRemovedLog => new TextObject("{=cLZWjrZP}They have moved to another hiding place.", (Dictionary<string, object>)null);

	private TextObject NotDueledWithHideoutBossAndDefeatLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=nOLFHL3x}You and your men have defeated {BOSS_NAME} and the rest of the conspirators as {MENTOR.LINK} asked you to do.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", base.Mentor.CharacterObject, val, false);
			val.SetTextVariable("BOSS_NAME", HideoutBossName);
			return val;
		}
	}

	private TextObject NotDueledWithHideoutBossAndDefeatedLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			TextObject val = new TextObject("{=EV5ykPuT}You and your men were defeated by {BOSS_NAME} and his conspirators. Rest of your men finds your broken body among the bloodied pile of corpses. Yet you live to fight another day.", (Dictionary<string, object>)null);
			val.SetTextVariable("BOSS_NAME", HideoutBossName);
			return val;
		}
	}

	private TextObject DueledWithHideoutBossAndDefeatLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			TextObject val = new TextObject("{=LKiREaFZ}You have defeated {BOSS_NAME} in a fair duel his men the conspirators scatters and runs away in shame.", (Dictionary<string, object>)null);
			val.SetTextVariable("BOSS_NAME", HideoutBossName);
			return val;
		}
	}

	private TextObject DueledWithHideoutBossAndDefeatedLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			TextObject val = new TextObject("{=Uk7F483P}You were defeated by the {BOSS_NAME} in the duel. Your men takes your wounded body to the safety. As agreed, conspirators quickly leave and disappear without a trace.", (Dictionary<string, object>)null);
			val.SetTextVariable("BOSS_NAME", HideoutBossName);
			return val;
		}
	}

	public ConspiracyBaseOfOperationsDiscoveredConspiracyQuest(string questId, Hero questGiver)
		: base(questId, questGiver)
	{
		_raiderParties = new List<MobileParty>();
		_hideout = SelectHideout();
		if (_hideout.Hideout.IsSpotted)
		{
			((QuestBase)this).AddLog(HideoutSpottedLog, false);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_hideout);
		}
		_baseLocation = SettlementHelper.FindNearestSettlementToSettlement(_hideout, (NavigationType)1, (Func<Settlement, bool>)((Settlement p) => p.IsFortification));
		_conspiracyStrengthDecreaseAmount = 50f;
		InitializeHideout();
	}

	private Settlement SelectHideout()
	{
		Settlement centralSettlement = StoryModeHeroes.ImperialMentor.HomeSettlement;
		Settlement val = SettlementHelper.FindRandomHideout((Func<Settlement, bool>)delegate(Settlement s)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (s.Hideout.IsInfested)
			{
				MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
				CampaignVec2 gatePosition = s.GatePosition;
				CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
				if (mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1))
				{
					if (!StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine)
					{
						return !StoryModeData.IsKingdomImperial(SettlementHelper.FindNearestFortificationToSettlement(s, (NavigationType)1, (Func<Settlement, bool>)null).OwnerClan.Kingdom);
					}
					return StoryModeData.IsKingdomImperial(SettlementHelper.FindNearestFortificationToSettlement(s, (NavigationType)1, (Func<Settlement, bool>)null).OwnerClan.Kingdom);
				}
			}
			return false;
		});
		if (val == null)
		{
			val = SettlementHelper.FindRandomHideout((Func<Settlement, bool>)delegate(Settlement s)
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
				CampaignVec2 gatePosition = s.GatePosition;
				CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
				return mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1) && ((!StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine) ? (!StoryModeData.IsKingdomImperial(SettlementHelper.FindNearestFortificationToSettlement(s, (NavigationType)1, (Func<Settlement, bool>)null).OwnerClan.Kingdom)) : StoryModeData.IsKingdomImperial(SettlementHelper.FindNearestFortificationToSettlement(s, (NavigationType)1, (Func<Settlement, bool>)null).OwnerClan.Kingdom));
			});
			if (val == null)
			{
				val = SettlementHelper.FindRandomHideout((Func<Settlement, bool>)delegate(Settlement s)
				{
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					//IL_0015: Unknown result type (might be due to invalid IL or missing references)
					//IL_001e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0023: Unknown result type (might be due to invalid IL or missing references)
					MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
					CampaignVec2 gatePosition = s.GatePosition;
					CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
					return mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1) && s.Hideout.IsInfested;
				});
				if (val == null)
				{
					val = SettlementHelper.FindRandomHideout((Func<Settlement, bool>)delegate(Settlement s)
					{
						//IL_0010: Unknown result type (might be due to invalid IL or missing references)
						//IL_0015: Unknown result type (might be due to invalid IL or missing references)
						//IL_001e: Unknown result type (might be due to invalid IL or missing references)
						//IL_0023: Unknown result type (might be due to invalid IL or missing references)
						MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
						CampaignVec2 gatePosition = s.GatePosition;
						CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
						return mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1);
					});
				}
			}
		}
		if (!val.Hideout.IsInfested)
		{
			for (int num = 0; num < 2; num++)
			{
				if (!val.Hideout.IsInfested)
				{
					_raiderParties.Add(CreateRaiderParty(val, isBanditBossParty: false, num));
				}
			}
		}
		return val;
	}

	private MobileParty CreateRaiderParty(Settlement hideout, bool isBanditBossParty, int partyIndex)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		MobileParty obj = BanditPartyComponent.CreateBanditParty("conspiracy_discovered_quest_raider_party_" + partyIndex, hideout.OwnerClan, hideout.Hideout, isBanditBossParty, (PartyTemplateObject)null, hideout.GatePosition);
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(((MBObjectBase)hideout.Culture).StringId + "_bandit");
		obj.MemberRoster.AddToCounts(val, 6, false, 0, 0, true, -1);
		obj.Party.SetCustomName(new TextObject("{=u1Pkt4HC}Raiders", (Dictionary<string, object>)null));
		obj.ActualClan = hideout.OwnerClan;
		obj.Position = hideout.Position;
		obj.Party.SetVisualAsDirty();
		EnterSettlementAction.ApplyForParty(obj, hideout);
		float num = obj.Party.CalculateCurrentStrength();
		int num2 = (int)(1f * MBRandom.RandomFloat * 20f * num + 50f);
		obj.InitializePartyTrade(num2);
		obj.SetMoveGoToSettlement(hideout, (NavigationType)1, false);
		EnterSettlementAction.ApplyForParty(obj, hideout);
		obj.SetPartyUsedByQuest(true);
		return obj;
	}

	protected override void InitializeQuestOnGameLoad()
	{
		_conspiracyStrengthDecreaseAmount = 50f;
		_baseLocation = SettlementHelper.FindNearestFortificationToSettlement(_hideout, (NavigationType)1, (Func<Settlement, bool>)null);
		((QuestBase)this).SetDialogs();
	}

	protected override void HourlyTick()
	{
	}

	private void InitializeHideout()
	{
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_baseLocation);
	}

	private void ChangeHideoutParties()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		PartyTemplateObject raiderTemplate = (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("conspiracy_anti_imperial_special_raider_party_template") : ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("conspiracy_imperial_special_raider_party_template"));
		foreach (MobileParty item in (List<MobileParty>)(object)_hideout.Parties)
		{
			if (item.IsBandit)
			{
				item.Party.SetCustomName(new TextObject("{=FRSas4xT}Conspiracy Troops", (Dictionary<string, object>)null));
				item.SetPartyUsedByQuest(true);
				if (item.IsBanditBossParty)
				{
					int troopCountLimit = item.MemberRoster.TotalManCount - 1;
					item.MemberRoster.Clear();
					DistributeConspiracyRaiderTroopsByLevel(raiderTemplate, item.Party, troopCountLimit);
					CharacterObject val = (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("anti_imperial_conspiracy_boss") : ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("imperial_conspiracy_boss"));
					val.SetTransferableInPartyScreen(false);
					item.MemberRoster.AddToCounts(val, 1, true, 0, 0, true, -1);
				}
				else
				{
					int totalManCount = item.MemberRoster.TotalManCount;
					item.MemberRoster.Clear();
					DistributeConspiracyRaiderTroopsByLevel(raiderTemplate, item.Party, totalManCount);
				}
			}
		}
	}

	protected override void RegisterEvents()
	{
		base.RegisterEvents();
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionStarted);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		CampaignEvents.OnHideoutSpottedEvent.AddNonSerializedListener((object)this, (Action<PartyBase, PartyBase>)OnHideoutSpotted);
		CampaignEvents.OnHideoutDeactivatedEvent.AddNonSerializedListener((object)this, (Action<Settlement>)OnHideoutCleared);
		CampaignEvents.OnHideoutBattleCompletedEvent.AddNonSerializedListener((object)this, (Action<BattleSideEnum, HideoutEventComponent, HideoutBattleEndState>)OnHideoutBattleCompleted);
	}

	private void OnHideoutBattleCompleted(BattleSideEnum winnerSide, HideoutEventComponent hideoutEventComponent, HideoutBattleEndState battleEndState)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		_isSuccess = ((MapEventComponent)hideoutEventComponent).MapEvent.InvolvedParties.Contains(PartyBase.MainParty) && winnerSide == ((MapEventComponent)hideoutEventComponent).MapEvent.PlayerSide;
		HandleHideoutBattleEnd();
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (Settlement.CurrentSettlement != _hideout || !((QuestBase)this).IsOngoing)
		{
			return;
		}
		MobileParty val = ((IEnumerable<MobileParty>)_hideout.Parties).FirstOrDefault((Func<MobileParty, bool>)((MobileParty p) => p.IsBanditBossParty));
		if (val == null || !val.IsActive)
		{
			return;
		}
		if (val.MemberRoster.TotalManCount <= 0)
		{
			ChangeHideoutParties();
			return;
		}
		string text = (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? "anti_imperial_conspiracy_boss" : "imperial_conspiracy_boss");
		bool flag = false;
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)val.MemberRoster.GetTroopRoster())
		{
			if (((MBObjectBase)item.Character).StringId == text)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ChangeHideoutParties();
		}
	}

	private void HandleHideoutBattleEnd()
	{
		if (!((QuestBase)this).IsOngoing)
		{
			return;
		}
		if (Hero.MainHero.IsPrisoner)
		{
			EndCaptivityAction.ApplyByPeace(Hero.MainHero, (Hero)null);
		}
		foreach (MobileParty item in ((IEnumerable<MobileParty>)_hideout.Parties).ToList())
		{
			if (item.IsBandit)
			{
				DestroyPartyAction.Apply((PartyBase)null, item);
			}
		}
		if (_isSuccess)
		{
			((QuestBase)this).CompleteQuestWithSuccess();
			return;
		}
		((QuestBase)this).AddLog(HideoutRemovedLog, false);
		((QuestBase)this).CompleteQuestWithFail((TextObject)null);
	}

	private void OnMissionStarted(IMission mission)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		if (Settlement.CurrentSettlement != _hideout || PlayerEncounter.Current == null)
		{
			return;
		}
		CharacterObject overriddenHideoutBossCharacterObject = (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("anti_imperial_conspiracy_boss") : ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("imperial_conspiracy_boss"));
		Mission val = (Mission)mission;
		HideoutAmbushMissionController missionBehavior = val.GetMissionBehavior<HideoutAmbushMissionController>();
		if (missionBehavior != null)
		{
			missionBehavior.SetOverriddenHideoutBossCharacterObject(overriddenHideoutBossCharacterObject);
			return;
		}
		HideoutMissionController missionBehavior2 = val.GetMissionBehavior<HideoutMissionController>();
		if (missionBehavior2 != null)
		{
			missionBehavior2.SetOverriddenHideoutBossCharacterObject(overriddenHideoutBossCharacterObject);
		}
		else
		{
			Debug.FailedAssert("Hideout boss can not be set!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Quests\\SecondPhase\\ConspiracyQuests\\ConspiracyBaseOfOperationsDiscoveredConspiracyQuest.cs", "OnMissionStarted", 415);
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between Unknown and I4
		if (Settlement.CurrentSettlement != _hideout || PlayerEncounter.Current == null)
		{
			return;
		}
		MapEvent playerMapEvent = MapEvent.PlayerMapEvent;
		if (playerMapEvent == null)
		{
			return;
		}
		if (playerMapEvent.WinningSide == playerMapEvent.PlayerSide)
		{
			if (_dueledWithHideoutBoss)
			{
				DueledWithHideoutBossAndDefeatedCaravan();
			}
			else
			{
				NotDueledWithHideoutBossAndDefeatedCaravan();
			}
			_isSuccess = true;
		}
		else
		{
			if ((int)playerMapEvent.WinningSide != -1)
			{
				if (_dueledWithHideoutBoss)
				{
					DueledWithHideoutBossAndDefeatedByCaravan();
				}
				else
				{
					NotDueledWithHideoutBossAndDefeatedByCaravan();
				}
			}
			_isSuccess = false;
		}
		HandleHideoutBattleEnd();
	}

	private void OnHideoutSpotted(PartyBase party, PartyBase hideoutParty)
	{
		if (party == PartyBase.MainParty && hideoutParty.Settlement == _hideout)
		{
			((QuestBase)this).AddLog(HideoutSpottedLog, false);
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_hideout);
		}
	}

	private void OnHideoutCleared(Settlement hideout)
	{
		if (hideout == _hideout)
		{
			MobileParty lastAttackerParty = hideout.LastAttackerParty;
			if (lastAttackerParty != null && lastAttackerParty.IsMainParty)
			{
				NotDueledWithHideoutBossAndDefeatedCaravan();
				_isSuccess = true;
				HandleHideoutBattleEnd();
			}
		}
	}

	private void NotDueledWithHideoutBossAndDefeatedCaravan()
	{
		((QuestBase)this).AddLog(NotDueledWithHideoutBossAndDefeatLog, false);
		_conspiracyStrengthDecreaseAmount = 50f;
	}

	private void NotDueledWithHideoutBossAndDefeatedByCaravan()
	{
		((QuestBase)this).AddLog(NotDueledWithHideoutBossAndDefeatedLog, false);
	}

	private void DueledWithHideoutBossAndDefeatedCaravan()
	{
		((QuestBase)this).AddLog(DueledWithHideoutBossAndDefeatLog, false);
		_conspiracyStrengthDecreaseAmount = 75f;
	}

	private void DueledWithHideoutBossAndDefeatedByCaravan()
	{
		((QuestBase)this).AddLog(DueledWithHideoutBossAndDefeatedLog, false);
	}

	protected override void SetDialogs()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000015).NpcLine(new TextObject("{=UdHL9YZC}Well well, isn't this the famous {PLAYER.LINK}! You have been a thorn at our side for a while now. It's good that you are here now. It spares us from searching for you.[if:convo_confused_annoyed][ib:hip]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(bandit_hideout_boss_fight_start_on_condition))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=bZI82WMt}Let's get this over with! Men Attack!", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(bandit_hideout_continue_battle_on_clickable_condition))
			.NpcLine(new TextObject("{=H2FMIJmw}My wolves! Kill them![ib:aggressive][if:convo_furious]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(bandit_hideout_continue_battle_on_consequence))
			.CloseDialog()
			.PlayerOption(new TextObject("{=5PGokzW1}Talk is cheap. If you really want me that bad, I challenge you to a duel.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=karjORwI}To hell with that! Why would I want to duel with you?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=MU2O1SaZ}There is an army waiting for you outside.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=tF6VeYaA}If you win, I promise my army won't crush you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.PlayerLine(new TextObject("{=fUcwKbW8}If I win I will just kill you and let these poor excuses you call conspirators run away.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=C0xbbPqE}I will duel you for your insolence! Die dog![ib:warrior][if:convo_furious]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(bandit_hideout_start_duel_fight_on_consequence))
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
	}

	private bool bandit_hideout_boss_fight_start_on_condition()
	{
		PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
		if (encounteredParty == null || encounteredParty.IsMobile || encounteredParty.MapFaction == null || !encounteredParty.MapFaction.IsBanditFaction)
		{
			return false;
		}
		if (encounteredParty.IsSettlement && encounteredParty.Settlement.IsHideout && encounteredParty.Settlement == _hideout && Mission.Current != null && CharacterObject.OneToOneConversationCharacter != null && ((MBObjectBase)CharacterObject.OneToOneConversationCharacter).StringId == (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? "anti_imperial_conspiracy_boss" : "imperial_conspiracy_boss"))
		{
			if (Mission.Current.GetMissionBehavior<HideoutAmbushMissionController>() == null)
			{
				return Mission.Current.GetMissionBehavior<HideoutMissionController>() != null;
			}
			return true;
		}
		return false;
	}

	private void bandit_hideout_start_duel_fight_on_consequence()
	{
		_dueledWithHideoutBoss = true;
		if (Mission.Current.GetMissionBehavior<HideoutAmbushMissionController>() != null)
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutAmbushMissionController.StartBossFightDuelMode;
		}
		else
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutMissionController.StartBossFightDuelMode;
		}
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
		_dueledWithHideoutBoss = false;
		if (Mission.Current.GetMissionBehavior<HideoutAmbushMissionController>() != null)
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutAmbushMissionController.StartBossFightBattleMode;
		}
		else
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutMissionController.StartBossFightBattleMode;
		}
	}

	protected override void OnStartQuest()
	{
		base.OnStartQuest();
		((QuestBase)this).SetDialogs();
	}

	protected override void OnCompleteWithSuccess()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		base.OnCompleteWithSuccess();
		((QuestBase)this).AddLog(new TextObject("{=6Dd3Pa07}You managed to thwart the conspiracy.", (Dictionary<string, object>)null), false);
		foreach (MobileParty raiderParty in _raiderParties)
		{
			if (raiderParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, raiderParty);
			}
		}
		_raiderParties.Clear();
	}

	protected override void OnTimedOut()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		((QuestBase)this).OnTimedOut();
		((QuestBase)this).AddLog(new TextObject("{=S5Dn2K3m}You couldn't stop the conspiracy.", (Dictionary<string, object>)null), false);
	}

	internal static void AutoGeneratedStaticCollectObjectsConspiracyBaseOfOperationsDiscoveredConspiracyQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(ConspiracyBaseOfOperationsDiscoveredConspiracyQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_hideout);
		collectedObjects.Add(_raiderParties);
	}

	internal static object AutoGeneratedGetMemberValue_hideout(object o)
	{
		return ((ConspiracyBaseOfOperationsDiscoveredConspiracyQuest)o)._hideout;
	}

	internal static object AutoGeneratedGetMemberValue_raiderParties(object o)
	{
		return ((ConspiracyBaseOfOperationsDiscoveredConspiracyQuest)o)._raiderParties;
	}
}
