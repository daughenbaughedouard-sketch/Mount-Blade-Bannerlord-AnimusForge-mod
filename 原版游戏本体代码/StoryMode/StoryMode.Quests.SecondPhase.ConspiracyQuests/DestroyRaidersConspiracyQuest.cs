using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
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
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.SecondPhase.ConspiracyQuests;

public class DestroyRaidersConspiracyQuest : ConspiracyQuestBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<Settlement, bool> _003C_003E9__55_2;

		public static Func<Hideout, Settlement> _003C_003E9__57_1;

		public static Func<Settlement, bool> _003C_003E9__60_0;

		public static OnConsequenceDelegate _003C_003E9__68_2;

		internal bool _003CDetermineTargetSettlement_003Eb__55_2(Settlement t)
		{
			if (!t.IsTown)
			{
				return t.IsCastle;
			}
			return true;
		}

		internal Settlement _003CDetermineClosestHideouts_003Eb__57_1(Hideout x)
		{
			return ((SettlementComponent)x).Settlement;
		}

		internal bool _003CGetBanditTypeForSettlement_003Eb__60_0(Settlement x)
		{
			return x.IsActive;
		}

		internal void _003CGetConspiracyCaptainDialogue_003Eb__68_2()
		{
			PlayerEncounter.LeaveEncounter = true;
		}
	}

	private const int QuestSuccededRelationBonus = 5;

	private const int QuestSucceededSecurityBonus = 5;

	private const int QuestSuceededProsperityBonus = 5;

	private const int QuestSuceededRenownBonus = 5;

	private const int QuestFailedRelationPenalty = -5;

	private const int NumberOfRegularRaidersToSpawn = 3;

	[SaveableField(1)]
	private readonly Settlement _targetSettlement;

	[SaveableField(2)]
	private readonly List<MobileParty> _regularRaiderParties;

	[SaveableField(3)]
	private MobileParty _specialRaiderParty;

	[SaveableField(4)]
	private JournalLog _regularPartiesProgressTracker;

	[SaveableField(5)]
	private JournalLog _specialPartyProgressTracker;

	[SaveableField(6)]
	private Clan _banditFaction;

	[SaveableField(7)]
	private CharacterObject _conspiracyCaptainCharacter;

	[SaveableField(8)]
	private Settlement _closestHideout;

	[SaveableField(9)]
	private List<MobileParty> _directedRaidersToEngagePlayer;

	private float RaiderPartyPlayerEncounterRadius => Campaign.Current.Models.EncounterModel.GetEncounterJoiningRadius * 3f;

	public override TextObject Title => new TextObject("{=DfiACGay}Destroy Raiders", (Dictionary<string, object>)null);

	public override float ConspiracyStrengthDecreaseAmount => 50f;

	private int RegularRaiderPartyTroopCount => 17 + MathF.Ceiling(23f * Campaign.Current.Models.IssueModel.GetIssueDifficultyMultiplier());

	private int SpecialRaiderPartyTroopCount => 33 + MathF.Ceiling(37f * Campaign.Current.Models.IssueModel.GetIssueDifficultyMultiplier());

	public override TextObject StartLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=Dr63pCHt}{MENTOR.LINK} has sent you a message about bandit attacks near {TARGET_SETTLEMENT}, and advises you to go there and eliminate them all before their actions turn the locals against your movement. ", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
			val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	public override TextObject StartMessageLogFromMentor
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=V5K8RpAa}{MENTOR.LINK}'s message: “Greetings, {PLAYER.NAME}. We have a new problem. I've had reports from my agents of unusual bandit activity near {TARGET_SETTLEMENT}. They appear to be raiding and killing travellers {?IS_EMPIRE}under the protection of the Empire{?}who aren't under the protection of the Empire{\\?}, and leaving the others alone. This seems very much like the work of {NEMESIS_MENTOR.LINK}, to terrorize local merchants so that no one will stand up for our cause. I advise you to wipe these bandits out as quickly as possible. That would send a good message, both to our allies and our enemies.”", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
			StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, val, false);
			bool isOnImperialQuestLine = StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine;
			StringHelpers.SetCharacterProperties("NEMESIS_MENTOR", isOnImperialQuestLine ? StoryModeHeroes.AntiImperialMentor.CharacterObject : StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			val.SetTextVariable("IS_IMPERIAL", isOnImperialQuestLine ? 1 : 0);
			val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	public override TextObject SideNotificationText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=T7OTmJUp}{MENTOR.LINK} has a message for you", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", ((QuestBase)this).QuestGiver.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _destroyRaidersQuestSucceededLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=qg05CSZb}You have defeated all the raiders near {TARGET_SETTLEMENT}. Many people now hope you can bring peace and prosperity back to the region.", (Dictionary<string, object>)null);
			val.SetTextVariable("TARGET_SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _destroyRaidersQuestFailedOnTimedOutLogText => new TextObject("{=DaBN0O7N}You have failed to defeat all raider parties in time. Many of the locals feel that you've brought misfortune upon them, and want nothing to do with you.", (Dictionary<string, object>)null);

	private TextObject _destroyRaidersQuestFailedOnPlayerDefeatedByRaidersLogText => new TextObject("{=mN60B07k}You have lost the battle against raiders and failed to defeat conspiracy forces. Many of the locals feel that you've brought misfortune upon them, and want nothing to do with you.", (Dictionary<string, object>)null);

	private TextObject _destroyRaidersRegularPartiesProgress
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=dbLb3krw}Hunt the gangs of {RAIDER_NAME}", (Dictionary<string, object>)null);
			val.SetTextVariable("RAIDER_NAME", _banditFaction.Name);
			return val;
		}
	}

	private TextObject _destroyRaidersSpecialPartyProgress => new TextObject("{=QVkuaezc}Hunt the conspiracy war party", (Dictionary<string, object>)null);

	private TextObject _destroyRaidersRegularProgressNotification
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=US0VAHiE}You have eliminated a {RAIDER_NAME} party.", (Dictionary<string, object>)null);
			val.SetTextVariable("RAIDER_NAME", _banditFaction.Name);
			return val;
		}
	}

	private TextObject _destroyRaidersRegularProgressCompletedNotification
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=LfH7VXDH}You have eliminated all {RAIDER_NAME} gangs in the vicinity.", (Dictionary<string, object>)null);
			val.SetTextVariable("RAIDER_NAME", _banditFaction.Name);
			return val;
		}
	}

	private TextObject _destroyRaidersSpecialPartyInformationQuestLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			TextObject val = new TextObject("{=agrsO3qQ}Due to your successful skirmishes against {RAIDER_NAME}, a conspiracy war party is now patrolling around {SETTLEMENT}.", (Dictionary<string, object>)null);
			val.SetTextVariable("RAIDER_NAME", _banditFaction.Name);
			val.SetTextVariable("SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _destroyRaidersSpecialPartySpawnNotification
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			TextObject val = new TextObject("{=QOVLkdTp}A conspiracy war party is now patrolling around {SETTLEMENT}.", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT", _targetSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	public DestroyRaidersConspiracyQuest(string questId, Hero questGiver)
		: base(questId, questGiver)
	{
		_regularRaiderParties = new List<MobileParty>(3);
		_directedRaidersToEngagePlayer = new List<MobileParty>(3);
		_targetSettlement = DetermineTargetSettlement();
		_banditFaction = GetBanditTypeForSettlement(_targetSettlement);
	}

	protected override void SetDialogs()
	{
		Campaign.Current.ConversationManager.AddDialogFlow(GetConspiracyCaptainDialogue(), (object)this);
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener((object)this, (Action<PartyBase, Hero>)OnHeroTakenPrisoner);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)MobilePartyDestroyed);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)MapEventEnded);
	}

	private void OnGameMenuOpened(MenuCallbackArgs menuCallbackArgs)
	{
		if (menuCallbackArgs.MenuContext.GameMenu.StringId == "prisoner_wait")
		{
			PartyBase captorParty = PlayerCaptivity.CaptorParty;
			if (captorParty != null && captorParty.IsMobile && (_regularRaiderParties.Contains(PlayerCaptivity.CaptorParty.MobileParty) || _specialRaiderParty == PlayerCaptivity.CaptorParty.MobileParty))
			{
				OnQuestFailedByDefeat();
			}
		}
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
		DetermineClosestHideouts();
		if (_directedRaidersToEngagePlayer == null)
		{
			_directedRaidersToEngagePlayer = new List<MobileParty>(3);
			return;
		}
		if (_directedRaidersToEngagePlayer.Count > _regularRaiderParties.Count)
		{
			_directedRaidersToEngagePlayer = new List<MobileParty>(3);
			{
				foreach (MobileParty regularRaiderParty in _regularRaiderParties)
				{
					SetDefaultRaiderAi(regularRaiderParty);
				}
				return;
			}
		}
		foreach (MobileParty regularRaiderParty2 in _regularRaiderParties)
		{
			CheckRaiderPartyPlayerEncounter(regularRaiderParty2);
		}
	}

	protected override void OnStartQuest()
	{
		base.OnStartQuest();
		string text = (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? "conspiracy_commander_antiempire" : "conspiracy_commander_empire");
		_conspiracyCaptainCharacter = Game.Current.ObjectManager.GetObject<CharacterObject>(text);
		InitializeRaiders();
		_regularPartiesProgressTracker = ((QuestBase)this).AddDiscreteLog(_destroyRaidersRegularPartiesProgress, TextObject.GetEmpty(), 0, 3, (TextObject)null, false);
		((QuestBase)this).SetDialogs();
		((QuestBase)this).InitializeQuestOnCreation();
	}

	private Settlement DetermineTargetSettlement()
	{
		Settlement val = null;
		Settlement centralSettlement = StoryModeHeroes.ImperialMentor.HomeSettlement;
		if (!Extensions.IsEmpty<Settlement>((IEnumerable<Settlement>)Clan.PlayerClan.Settlements))
		{
			val = Extensions.GetRandomElementWithPredicate<Settlement>(Clan.PlayerClan.Settlements, (Func<Settlement, bool>)delegate(Settlement t)
			{
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				if (t.IsTown || t.IsCastle)
				{
					MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
					CampaignVec2 gatePosition = t.GatePosition;
					CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
					return mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1);
				}
				return false;
			});
		}
		else
		{
			MBList<Settlement> val2 = Extensions.ToMBList<Settlement>(((IEnumerable<Settlement>)StoryModeManager.Current.MainStoryLine.PlayerSupportedKingdom.Settlements).Where((Settlement t) => t.IsTown || t.IsCastle));
			if (!Extensions.IsEmpty<Settlement>((IEnumerable<Settlement>)val2))
			{
				val = Extensions.GetRandomElementWithPredicate<Settlement>(val2, (Func<Settlement, bool>)delegate(Settlement t)
				{
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					//IL_0015: Unknown result type (might be due to invalid IL or missing references)
					//IL_001e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0023: Unknown result type (might be due to invalid IL or missing references)
					MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
					CampaignVec2 gatePosition = t.GatePosition;
					CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
					return mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1);
				});
			}
		}
		if (val == null)
		{
			Debug.FailedAssert("Destroy raiders conspiracy quest settlement is null", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Quests\\SecondPhase\\ConspiracyQuests\\DestroyRaidersConspiracyQuest.cs", "DetermineTargetSettlement", 306);
			val = Extensions.GetRandomElementWithPredicate<Settlement>(Settlement.All, (Func<Settlement, bool>)delegate(Settlement t)
			{
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				if (t.IsTown || t.IsCastle)
				{
					MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
					CampaignVec2 gatePosition = t.GatePosition;
					CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
					return mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1);
				}
				return false;
			});
		}
		return val;
	}

	private void InitializeRaiders()
	{
		List<Settlement> source = DetermineClosestHideouts();
		for (int i = 0; i < 3; i++)
		{
			SpawnRaiderPartyAtHideout(source.ElementAt(i));
		}
	}

	private List<Settlement> DetermineClosestHideouts()
	{
		MapDistanceModel model = Campaign.Current.Models.MapDistanceModel;
		Settlement centralSettlement = StoryModeHeroes.ImperialMentor.HomeSettlement;
		List<Settlement> list = (from x in ((IEnumerable<Hideout>)Hideout.All).Where(delegate(Hideout t)
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
				CampaignVec2 gatePosition = ((SettlementComponent)t).Settlement.GatePosition;
				CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
				return mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1);
			})
			select ((SettlementComponent)x).Settlement into t
			orderby model.GetDistance(_targetSettlement, t, false, false, (NavigationType)1)
			select t).Take(3).ToList();
		_closestHideout = list[0];
		return list;
	}

	private void SpawnRaiderPartyAtHideout(Settlement hideout, bool isSpecialParty = false)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		PartyTemplateObject val;
		int num;
		TextObject customName;
		if (isSpecialParty)
		{
			val = (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("conspiracy_anti_imperial_special_raider_party_template") : ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("conspiracy_imperial_special_raider_party_template"));
			num = SpecialRaiderPartyTroopCount;
			customName = new TextObject("{=GW7Zg3IP}Conspiracy War Party", (Dictionary<string, object>)null);
		}
		else
		{
			val = _banditFaction.DefaultPartyTemplate;
			num = RegularRaiderPartyTroopCount;
			customName = _banditFaction.Name;
		}
		object[] obj = new object[4] { "destroy_raiders_conspiracy_quest_", _banditFaction.Name, "_", null };
		CampaignTime now = CampaignTime.Now;
		obj[3] = ((CampaignTime)(ref now)).ElapsedSecondsUntilNow;
		MobileParty val2 = BanditPartyComponent.CreateBanditParty(string.Concat(obj), _banditFaction, hideout.Hideout, false, val, hideout.GatePosition);
		val2.Party.SetCustomName(customName);
		val2.MemberRoster.Clear();
		val2.SetPartyUsedByQuest(true);
		SetDefaultRaiderAi(val2);
		if (isSpecialParty)
		{
			_specialRaiderParty = val2;
			val2.MemberRoster.AddToCounts(_conspiracyCaptainCharacter, 1, true, 0, 0, true, -1);
			val2.ItemRoster.Clear();
			val2.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("vlandia_horse"), num / 2);
			MBInformationManager.AddQuickInformation(_destroyRaidersSpecialPartySpawnNotification, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		else
		{
			_regularRaiderParties.Add(val2);
		}
		DistributeConspiracyRaiderTroopsByLevel(val, val2.Party, num);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)val2);
	}

	private void SetDefaultRaiderAi(MobileParty raiderParty)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		SetPartyAiAction.GetActionForPatrollingAroundSettlement(raiderParty, _targetSettlement, (NavigationType)1, false, false);
		raiderParty.Ai.CheckPartyNeedsUpdate();
		raiderParty.Ai.SetDoNotMakeNewDecisions(true);
		raiderParty.IgnoreByOtherPartiesTill(CampaignTime.Never);
	}

	private Clan GetBanditTypeForSettlement(Settlement settlement)
	{
		Hideout closestHideout = SettlementHelper.FindNearestHideoutToSettlement(settlement, (NavigationType)1, (Func<Settlement, bool>)((Settlement x) => x.IsActive));
		if (closestHideout == null)
		{
			return Extensions.GetRandomElementInefficiently<Clan>(Clan.BanditFactions);
		}
		return Clan.BanditFactions.FirstOrDefault((Func<Clan, bool>)((Clan t) => t.Culture == ((SettlementComponent)closestHideout).Settlement.Culture));
	}

	private void MobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
	{
		if (destroyerParty != null && destroyerParty.MobileParty == MobileParty.MainParty)
		{
			if (_regularRaiderParties.Contains(mobileParty))
			{
				OnBanditPartyClearedByPlayer(mobileParty);
			}
			else if (_specialRaiderParty == mobileParty)
			{
				OnSpecialBanditPartyClearedByPlayer();
			}
		}
	}

	private void MapEventEnded(MapEvent mapEvent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if ((int)mapEvent.WinningSide == -1 || (int)mapEvent.DefeatedSide == -1 || !mapEvent.IsPlayerMapEvent || !mapEvent.InvolvedParties.Any((PartyBase t) => t.IsMobile && (_regularRaiderParties.Contains(t.MobileParty) || t.MobileParty == _specialRaiderParty)))
		{
			return;
		}
		if (PlayerEncounter.Battle.WinningSide == PlayerEncounter.Battle.PlayerSide)
		{
			foreach (MapEventParty item in (List<MapEventParty>)(object)mapEvent.GetMapEventSide(mapEvent.DefeatedSide).Parties)
			{
				MobileParty mobileParty = item.Party.MobileParty;
				if (mobileParty != null && mobileParty.IsActive && mobileParty.Party.NumberOfHealthyMembers > 0 && (_regularRaiderParties.Contains(mobileParty) || _specialRaiderParty == mobileParty))
				{
					DestroyPartyAction.Apply(PartyBase.MainParty, mobileParty);
				}
			}
			return;
		}
		PartyBase captorParty = PlayerCaptivity.CaptorParty;
		if (captorParty == null || !captorParty.IsMobile || (!_regularRaiderParties.Contains(PlayerCaptivity.CaptorParty.MobileParty) && _specialRaiderParty != PlayerCaptivity.CaptorParty.MobileParty))
		{
			OnQuestFailedByDefeat();
		}
	}

	private void OnSpecialBanditPartyClearedByPlayer()
	{
		if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)_specialRaiderParty))
		{
			((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)_specialRaiderParty);
		}
		_specialPartyProgressTracker.UpdateCurrentProgress(1);
		_specialRaiderParty = null;
		OnQuestSucceeded();
	}

	private void OnBanditPartyClearedByPlayer(MobileParty defeatedParty)
	{
		_regularRaiderParties.Remove(defeatedParty);
		_regularPartiesProgressTracker.UpdateCurrentProgress(3 - _regularRaiderParties.Count);
		if (_regularPartiesProgressTracker.HasBeenCompleted())
		{
			MBInformationManager.AddQuickInformation(_destroyRaidersRegularProgressCompletedNotification, 0, (BasicCharacterObject)null, (Equipment)null, "");
			((QuestBase)this).AddLog(_destroyRaidersSpecialPartyInformationQuestLog, false);
			_specialPartyProgressTracker = ((QuestBase)this).AddDiscreteLog(_destroyRaidersSpecialPartyProgress, TextObject.GetEmpty(), 0, 1, (TextObject)null, false);
			SpawnRaiderPartyAtHideout(_closestHideout, isSpecialParty: true);
		}
		else
		{
			if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)defeatedParty))
			{
				((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)defeatedParty);
			}
			MBInformationManager.AddQuickInformation(_destroyRaidersRegularProgressNotification, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void OnHeroTakenPrisoner(PartyBase capturer, Hero prisoner)
	{
		if (prisoner.Clan != Clan.PlayerClan && capturer.IsMobile && (_regularRaiderParties.Contains(capturer.MobileParty) || _specialRaiderParty == capturer.MobileParty))
		{
			Debug.FailedAssert("Hero has been taken prisoner by conspiracy raider party", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Quests\\SecondPhase\\ConspiracyQuests\\DestroyRaidersConspiracyQuest.cs", "OnHeroTakenPrisoner", 533);
			EndCaptivityAction.ApplyByEscape(prisoner, (Hero)null, true);
		}
	}

	protected override void HourlyTick()
	{
		foreach (MobileParty regularRaiderParty in _regularRaiderParties)
		{
			CheckRaiderPartyPlayerEncounter(regularRaiderParty);
		}
	}

	private void CheckRaiderPartyPlayerEncounter(MobileParty raiderParty)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 position = raiderParty.Position;
		if (((CampaignVec2)(ref position)).DistanceSquared(MobileParty.MainParty.Position) <= RaiderPartyPlayerEncounterRadius)
		{
			CampaignTime doNotAttackMainPartyUntil = raiderParty.Ai.DoNotAttackMainPartyUntil;
			if (((CampaignTime)(ref doNotAttackMainPartyUntil)).IsPast && raiderParty.Party.CalculateCurrentStrength() > PartyBase.MainParty.CalculateCurrentStrength() * 1.2f && MobileParty.MainParty.CurrentSettlement == null)
			{
				if (!_directedRaidersToEngagePlayer.Contains(raiderParty))
				{
					SetPartyAiAction.GetActionForEngagingParty(raiderParty, MobileParty.MainParty, (NavigationType)1, false);
					raiderParty.Ai.CheckPartyNeedsUpdate();
					_directedRaidersToEngagePlayer.Add(raiderParty);
				}
				return;
			}
		}
		if (_directedRaidersToEngagePlayer.Contains(raiderParty))
		{
			_directedRaidersToEngagePlayer.Remove(raiderParty);
			SetDefaultRaiderAi(raiderParty);
		}
	}

	private DialogFlow GetConspiracyCaptainDialogue()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 125).NpcLine("{=bzmcPtZ6}We know you. We were told to look out for you. We know what you're planning with {MENTOR.NAME}. You will fail, and you will die.[ib:closed][if:convo_predatory]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
		{
			StringHelpers.SetCharacterProperties("MENTOR", ((QuestBase)this).QuestGiver.CharacterObject, (TextObject)null, false);
			return CharacterObject.OneToOneConversationCharacter == _conspiracyCaptainCharacter && _specialRaiderParty != null && !_specialPartyProgressTracker.HasBeenCompleted();
		})
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=BrHU0NuE}Maybe. But if we do, you won't live to see it.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += OnConspiracyCaptainDialogueEnd;
			})
			.NpcLine("{=EoLcoaHM}We'll see...", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.PlayerOption("{=TLaxmQDF}You'll without a doubt perish by my sword, but today is not the day.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__68_2;
		if (obj2 == null)
		{
			OnConsequenceDelegate val = delegate
			{
				PlayerEncounter.LeaveEncounter = true;
			};
			_003C_003Ec._003C_003E9__68_2 = val;
			obj2 = (object)val;
		}
		return obj.Consequence((OnConsequenceDelegate)obj2).NpcLine("{=9aY0ifwi}We shall meet again...[if:convo_insulted]", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog()
			.EndPlayerOptions()
			.CloseDialog();
	}

	private void OnConspiracyCaptainDialogueEnd()
	{
		PlayerEncounter.RestartPlayerEncounter(_specialRaiderParty.Party, PartyBase.MainParty, true);
		PlayerEncounter.StartBattle();
	}

	private void OnQuestSucceeded()
	{
		if (_targetSettlement.OwnerClan != Clan.PlayerClan && !_targetSettlement.OwnerClan.MapFaction.IsAtWarWith(Clan.PlayerClan.MapFaction))
		{
			ChangeRelationAction.ApplyPlayerRelation(_targetSettlement.OwnerClan.Leader, 5, true, true);
		}
		Clan.PlayerClan.AddRenown(5f, true);
		Town town = _targetSettlement.Town;
		town.Security += 5f;
		Town town2 = _targetSettlement.Town;
		town2.Prosperity += 5f;
		((QuestBase)this).AddLog(_destroyRaidersQuestSucceededLogText, false);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private void OnQuestFailedByDefeat()
	{
		OnQuestFailed();
		((QuestBase)this).AddLog(_destroyRaidersQuestFailedOnPlayerDefeatedByRaidersLogText, false);
		((QuestBase)this).CompleteQuestWithFail((TextObject)null);
	}

	private void OnQuestFailed()
	{
		foreach (MobileParty regularRaiderParty in _regularRaiderParties)
		{
			if (regularRaiderParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, regularRaiderParty);
			}
		}
		if (_specialRaiderParty != null && _specialRaiderParty.IsActive)
		{
			DestroyPartyAction.Apply((PartyBase)null, _specialRaiderParty);
		}
		if (_targetSettlement.OwnerClan != Clan.PlayerClan)
		{
			ChangeRelationAction.ApplyPlayerRelation(_targetSettlement.OwnerClan.Leader, -5, true, true);
		}
	}

	protected override void OnTimedOut()
	{
		OnQuestFailed();
		((QuestBase)this).AddLog(_destroyRaidersQuestFailedOnTimedOutLogText, false);
	}

	internal static void AutoGeneratedStaticCollectObjectsDestroyRaidersConspiracyQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(DestroyRaidersConspiracyQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_targetSettlement);
		collectedObjects.Add(_regularRaiderParties);
		collectedObjects.Add(_specialRaiderParty);
		collectedObjects.Add(_regularPartiesProgressTracker);
		collectedObjects.Add(_specialPartyProgressTracker);
		collectedObjects.Add(_banditFaction);
		collectedObjects.Add(_conspiracyCaptainCharacter);
		collectedObjects.Add(_closestHideout);
		collectedObjects.Add(_directedRaidersToEngagePlayer);
	}

	internal static object AutoGeneratedGetMemberValue_targetSettlement(object o)
	{
		return ((DestroyRaidersConspiracyQuest)o)._targetSettlement;
	}

	internal static object AutoGeneratedGetMemberValue_regularRaiderParties(object o)
	{
		return ((DestroyRaidersConspiracyQuest)o)._regularRaiderParties;
	}

	internal static object AutoGeneratedGetMemberValue_specialRaiderParty(object o)
	{
		return ((DestroyRaidersConspiracyQuest)o)._specialRaiderParty;
	}

	internal static object AutoGeneratedGetMemberValue_regularPartiesProgressTracker(object o)
	{
		return ((DestroyRaidersConspiracyQuest)o)._regularPartiesProgressTracker;
	}

	internal static object AutoGeneratedGetMemberValue_specialPartyProgressTracker(object o)
	{
		return ((DestroyRaidersConspiracyQuest)o)._specialPartyProgressTracker;
	}

	internal static object AutoGeneratedGetMemberValue_banditFaction(object o)
	{
		return ((DestroyRaidersConspiracyQuest)o)._banditFaction;
	}

	internal static object AutoGeneratedGetMemberValue_conspiracyCaptainCharacter(object o)
	{
		return ((DestroyRaidersConspiracyQuest)o)._conspiracyCaptainCharacter;
	}

	internal static object AutoGeneratedGetMemberValue_closestHideout(object o)
	{
		return ((DestroyRaidersConspiracyQuest)o)._closestHideout;
	}

	internal static object AutoGeneratedGetMemberValue_directedRaidersToEngagePlayer(object o)
	{
		return ((DestroyRaidersConspiracyQuest)o)._directedRaidersToEngagePlayer;
	}
}
