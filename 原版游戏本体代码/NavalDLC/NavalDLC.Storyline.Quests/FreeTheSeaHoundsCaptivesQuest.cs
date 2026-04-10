using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Missions;
using NavalDLC.SceneInformationPopupTypes;
using NavalDLC.Storyline.MissionControllers;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class FreeTheSeaHoundsCaptivesQuest : NavalStorylineQuestBase
{
	public enum FreeTheSeaHoundsCaptivesQuestState
	{
		None,
		RestartMission,
		GoToSeaHoundPartyPosition,
		EncounteredWithSeaHoundsParty,
		TalkedWithGunnarBeforeFight,
		TalkedWithPurigBeforeBossFight,
		PlayerLostBossFight,
		DefeatedPurig,
		HeadBackToOstican
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConditionDelegate _003C_003E9__44_0;

		public static OnConditionDelegate _003C_003E9__44_1;

		public static Action _003C_003E9__44_13;

		public static OnConsequenceDelegate _003C_003E9__44_2;

		public static OnConditionDelegate _003C_003E9__44_3;

		public static Func<Settlement, bool> _003C_003E9__72_0;

		public static Func<Clan, bool> _003C_003E9__72_1;

		public static Func<PartyTemplateStack, int> _003C_003E9__74_0;

		public static Func<PartyTemplateStack, int> _003C_003E9__74_1;

		internal bool _003CSetDialogs_003Eb__44_0()
		{
			if (Mission.Current == null)
			{
				return false;
			}
			Quest5SetPieceBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>();
			if (missionBehavior != null && Hero.OneToOneConversationHero == StoryModeHeroes.LittleSister)
			{
				return missionBehavior.State == Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.Phase1ShipInteriorPhase;
			}
			return false;
		}

		internal bool _003CSetDialogs_003Eb__44_1()
		{
			StringHelpers.SetCharacterProperties("SISTER", StoryModeHeroes.LittleSister.CharacterObject, (TextObject)null, false);
			return true;
		}

		internal void _003CSetDialogs_003Eb__44_2()
		{
			Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
			{
				Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>().SetTalkedWithSister();
			};
		}

		internal void _003CSetDialogs_003Eb__44_13()
		{
			Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>().SetTalkedWithSister();
		}

		internal bool _003CSetDialogs_003Eb__44_3()
		{
			if (Mission.Current == null)
			{
				return false;
			}
			StringHelpers.SetCharacterProperties("QUEST_5_COMPANION", NavalStorylineData.Gunnar.CharacterObject, (TextObject)null, false);
			Quest5SetPieceBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>();
			if (missionBehavior != null && Hero.OneToOneConversationHero == NavalStorylineData.Purig)
			{
				return missionBehavior.State == Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.BossFightConversationInProgress;
			}
			return false;
		}

		internal bool _003CCreateSeaHoundParty_003Eb__72_0(Settlement x)
		{
			return x.IsActive;
		}

		internal bool _003CCreateSeaHoundParty_003Eb__72_1(Clan x)
		{
			return ((MBObjectBase)x).StringId == "northern_pirates";
		}

		internal int _003CFillParty_003Eb__74_0(PartyTemplateStack s)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return s.MinValue;
		}

		internal int _003CFillParty_003Eb__74_1(PartyTemplateStack s)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return s.MaxValue;
		}
	}

	private const int PlayerLostDuelAndLetPurigGoHonorBonus = 50;

	private const int PlayerLostDuelAndKilledPurigHonorPenalty = -50;

	private const int PlayerLostDuelAndKilledPurigRenownBonus = 50;

	private const string SeaHoundSetPieceBattlePartyTemplateString = "storyline_act3_quest_5_sea_hounds_set_piece_battle_template";

	private const string SeaHoundPartyTemplateStringId = "storyline_act3_quest_5_sea_hounds_template";

	private const string EncounterMenuId = "act_3_quest_5_encounter_menu";

	private const string MissionMenuId = "act_3_quest_5_mission_menu";

	private const string SetPieceBattleSceneName = "naval_storyline_act_3_quest_5";

	private const int SeaHoundPartySize = 67;

	private const string NordMediumShipStringId = "nord_medium_ship";

	private const string AseraiHeavyShipStringId = "aserai_heavy_ship";

	[SaveableField(1)]
	private MobileParty _seaHoundsParty;

	private bool _shouldMissionContinueFromCheckpoint;

	[SaveableField(0)]
	private FreeTheSeaHoundsCaptivesQuestState _currentState;

	[SaveableField(7)]
	private float _strengthModifier;

	private bool _isPurigKilledViaConversation;

	private bool _isSisterSavedSceneNotificationTriggered;

	[SaveableField(12)]
	private readonly MapMarker _skatriaIslandsMarker;

	[SaveableField(13)]
	private Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState _lastHitCheckpoint;

	[SaveableField(14)]
	public Quest5SetPieceBattleMissionController.BossFightOutComeEnum BossFightOutCome;

	private readonly List<KeyValuePair<string, string>> _seaHoundPartyShipUpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("side", "side_northern_shields_lvl2")
	};

	private readonly List<KeyValuePair<string, string>> _nordMediumShipyShipUpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("sail", "sails_lvl2"),
		new KeyValuePair<string, string>("side", "side_northern_shields_lvl2")
	};

	private readonly List<KeyValuePair<string, string>> _aseraiHeavyShipUpgradePieceList = new List<KeyValuePair<string, string>>
	{
		new KeyValuePair<string, string>("fore", "fore_ballista"),
		new KeyValuePair<string, string>("aft", "aft_battlement_lvl3_wbarracks"),
		new KeyValuePair<string, string>("deck", "deck_arrow_and_javelin_crates_lvl2"),
		new KeyValuePair<string, string>("sail", "sails_lvl2")
	};

	public override TextObject Title => new TextObject("{=JYCrUhnu}Free the Sea Hounds' captives", (Dictionary<string, object>)null);

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act3Quest5;

	public override bool WillProgressStoryline => true;

	protected override string MainPartyTemplateStringId => "storyline_act3_quest_5_main_party_template";

	private CampaignVec2 _seaHoundsSpawnPosition => new CampaignVec2(new Vec2(260f, 815f), false);

	private TextObject _allyDefeatedText => new TextObject("{=9sfcVI0Q}Your allies were defeated. You will have to try again.", (Dictionary<string, object>)null);

	private TextObject _findSeaHoundsQuestLog => new TextObject("{=mp0EKEI9}Go to Angranfjord and locate the Sea Hounds.", (Dictionary<string, object>)null);

	private TextObject _arrivedAngranfjordQuestLog => new TextObject("{=7Gl82o4g}You have arrived at Angranfjord, Purig's lair.", (Dictionary<string, object>)null);

	public FreeTheSeaHoundsCaptivesQuest(string questId, float strengthModifier)
		: base(questId, NavalStorylineData.Gunnar, CampaignTime.Never, 0)
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		_strengthModifier = strengthModifier;
		MapMarkerManager mapMarkerManager = Campaign.Current.MapMarkerManager;
		Banner corsairBanner = NavalStorylineData.CorsairBanner;
		TextObject val = new TextObject("{=GSksjBCZ}Angranfjord", (Dictionary<string, object>)null);
		CampaignVec2 seaHoundsSpawnPosition = _seaHoundsSpawnPosition;
		_skatriaIslandsMarker = mapMarkerManager.CreateMapMarker(corsairBanner, val, ((CampaignVec2)(ref seaHoundsSpawnPosition)).AsVec3(), true, ((MBObjectBase)this).StringId);
		_currentState = FreeTheSeaHoundsCaptivesQuestState.GoToSeaHoundPartyPosition;
		((QuestBase)this).SetDialogs();
		AddGameMenus();
	}

	protected override void PreAfterLoad()
	{
		if (NavalStorylineData.Purig.IsDead)
		{
			return;
		}
		if (NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest5) || NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3SpeakToGunnarAndSister))
		{
			KillCharacterAction.ApplyByRemove(NavalStorylineData.Purig, false, true);
		}
		else
		{
			if (!NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest4) || NavalStorylineData.Purig.VolunteerTypes != null)
			{
				return;
			}
			MobileParty partyBelongedTo = NavalStorylineData.Purig.PartyBelongedTo;
			if (partyBelongedTo != null)
			{
				MapEvent mapEvent = partyBelongedTo.MapEvent;
				if (((mapEvent != null) ? new bool?(mapEvent.IsPlayerMapEvent) : ((bool?)null)) == true)
				{
					NavalStorylineData.Purig.PartyBelongedTo.MapEvent.FinalizeEvent();
				}
			}
			KillCharacterAction.ApplyByRemove(NavalStorylineData.Purig, false, true);
			_lastHitCheckpoint = Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.End;
			_currentState = FreeTheSeaHoundsCaptivesQuestState.DefeatedPurig;
		}
	}

	protected override void InitializeQuestOnGameLoadInternal()
	{
		base.InitializeQuestOnGameLoadInternal();
		((QuestBase)this).SetDialogs();
		AddGameMenus();
		if (_lastHitCheckpoint == Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.End)
		{
			if (BossFightOutCome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.None)
			{
				BossFightOutCome = Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerRefusedTheDuel;
			}
			ShowNavalSaveSisterSceneNotification();
		}
	}

	protected override void RegisterEventsInternal()
	{
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, (Action)OnHourlyTick);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
		CampaignEvents.CanHeroBecomePrisonerEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanHeroBecomePrisoner);
		CampaignEvents.PartyVisibilityChangedEvent.AddNonSerializedListener((object)this, (Action<PartyBase>)OnPartyVisibilityChanged);
	}

	protected override void SetDialogs()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0041: Expected O, but got Unknown
		//IL_0041: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0084: Expected O, but got Unknown
		//IL_0084: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00b6: Expected O, but got Unknown
		//IL_00b6: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00d8: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		//IL_012a: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Expected O, but got Unknown
		//IL_01ba: Expected O, but got Unknown
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Expected O, but got Unknown
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Expected O, but got Unknown
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Expected O, but got Unknown
		//IL_0242: Expected O, but got Unknown
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Expected O, but got Unknown
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Expected O, but got Unknown
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Expected O, but got Unknown
		//IL_02ca: Expected O, but got Unknown
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Expected O, but got Unknown
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Expected O, but got Unknown
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Expected O, but got Unknown
		//IL_0352: Expected O, but got Unknown
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Expected O, but got Unknown
		//IL_03b7: Expected O, but got Unknown
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Expected O, but got Unknown
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Expected O, but got Unknown
		//IL_0407: Expected O, but got Unknown
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Expected O, but got Unknown
		//IL_0433: Expected O, but got Unknown
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Expected O, but got Unknown
		//IL_045f: Expected O, but got Unknown
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Expected O, but got Unknown
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Expected O, but got Unknown
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Expected O, but got Unknown
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Expected O, but got Unknown
		//IL_050d: Expected O, but got Unknown
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Expected O, but got Unknown
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_0541: Expected O, but got Unknown
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Expected O, but got Unknown
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_0573: Expected O, but got Unknown
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Expected O, but got Unknown
		//IL_05be: Expected O, but got Unknown
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Expected O, but got Unknown
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f2: Expected O, but got Unknown
		//IL_0602: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_061e: Expected O, but got Unknown
		//IL_061e: Expected O, but got Unknown
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Expected O, but got Unknown
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_0650: Expected O, but got Unknown
		//IL_0657: Unknown result type (might be due to invalid IL or missing references)
		//IL_0661: Expected O, but got Unknown
		//IL_0677: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Expected O, but got Unknown
		//IL_0696: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Expected O, but got Unknown
		//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Expected O, but got Unknown
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0704: Unknown result type (might be due to invalid IL or missing references)
		//IL_0710: Expected O, but got Unknown
		//IL_0710: Expected O, but got Unknown
		//IL_0717: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Expected O, but got Unknown
		//IL_0728: Unknown result type (might be due to invalid IL or missing references)
		//IL_0732: Expected O, but got Unknown
		//IL_074d: Unknown result type (might be due to invalid IL or missing references)
		//IL_075a: Expected O, but got Unknown
		//IL_076e: Unknown result type (might be due to invalid IL or missing references)
		//IL_077b: Expected O, but got Unknown
		//IL_0790: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ad: Expected O, but got Unknown
		//IL_07ad: Expected O, but got Unknown
		//IL_07bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Expected O, but got Unknown
		//IL_07d9: Expected O, but got Unknown
		//IL_07e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Expected O, but got Unknown
		//IL_0805: Expected O, but got Unknown
		//IL_0815: Unknown result type (might be due to invalid IL or missing references)
		//IL_0825: Unknown result type (might be due to invalid IL or missing references)
		//IL_0831: Expected O, but got Unknown
		//IL_0831: Expected O, but got Unknown
		//IL_084c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0859: Expected O, but got Unknown
		//IL_086d: Unknown result type (might be due to invalid IL or missing references)
		//IL_087a: Expected O, but got Unknown
		//IL_088c: Unknown result type (might be due to invalid IL or missing references)
		//IL_089c: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a8: Expected O, but got Unknown
		//IL_08a8: Expected O, but got Unknown
		//IL_08af: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b9: Expected O, but got Unknown
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Expected O, but got Unknown
		DialogFlow val = DialogFlow.CreateDialogFlow("start", 1200).NpcLine(new TextObject("{=qn00ppJR}There they are. With your sister as their hostage, a straight-out attack is out of the question. Throughout this voyage, I have been thinking on what we might do to ensure her safety, and I recommend that we try an old corsair's trick.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null).Condition(new OnConditionDelegate(GunnarInitialMeetingDialogCondition))
			.NpcLine(new TextObject("{=axgouPEG}Do you see that big cluster of ships back there? That's got to be where they're holding the prisoners. That smaller vessel out front, though - that's got to be a picket, and it will stop us before we get too close. Let's approach it, pretending to be a buyer, while Bjolgur and Lahar stay out of sight. Crusas can banter with them a bit as a distraction. One of our men shall stand at his side with a dagger, lest he betray us.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.NpcLine(new TextObject("{=HzlWiTns}You and I, meanwhile, shall dive off the side of our ship, swim round to the stern of the prisoner ship, and climb up the side. Then together we can try to find your sister on board. Once we succeed, well, we'll just have to figure it out from there.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.PlayerLine(new TextObject("{=kJaiDDRi}Let's proceed, then.", (Dictionary<string, object>)null), new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(GunnarInitialMeetingDialogConsequence))
			.CloseDialog();
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=Q5B3Uvoa}Who's there? What's going on??[if:convo_dismayed]", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__44_0;
		if (obj2 == null)
		{
			OnConditionDelegate val2 = delegate
			{
				if (Mission.Current == null)
				{
					return false;
				}
				Quest5SetPieceBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>();
				return missionBehavior != null && Hero.OneToOneConversationHero == StoryModeHeroes.LittleSister && missionBehavior.State == Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.Phase1ShipInteriorPhase;
			};
			_003C_003Ec._003C_003E9__44_0 = val2;
			obj2 = (object)val2;
		}
		DialogFlow obj3 = obj.Condition((OnConditionDelegate)obj2).PlayerLine("{=0lTm2sy1}{SISTER.NAME}... Is that you? It's me!", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, (string)null);
		object obj4 = _003C_003Ec._003C_003E9__44_1;
		if (obj4 == null)
		{
			OnConditionDelegate val3 = delegate
			{
				StringHelpers.SetCharacterProperties("SISTER", StoryModeHeroes.LittleSister.CharacterObject, (TextObject)null, false);
				return true;
			};
			_003C_003Ec._003C_003E9__44_1 = val3;
			obj4 = (object)val3;
		}
		DialogFlow obj5 = obj3.Condition((OnConditionDelegate)obj4).NpcLine("{=IC9Fvl54}{?PLAYER.GENDER}Sister{?}Brother{\\?}! Heaven's mercy! What are you doing here?[rf:convo_relaxed_happy]", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null).BeginPlayerOptions((string)null, false)
			.PlayerOption("{=HKx2nxGt}It is. We're here to rescue you! Just... Keep your voice low.", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, (string)null)
			.GotoDialogState("sister_answer_1")
			.PlayerOption("{=gvOJ43Na}{SISTER.NAME}, I just need you to be patient and strong a little longer.", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, (string)null)
			.GotoDialogState("sister_answer_1")
			.EndPlayerOptions()
			.NpcLine("{=OLTofDbM}I'll be silent. What's going on?[ib:wounded]", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), "sister_answer_1", (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=jrloQtMP}I'm going to take this ship, and get you to safety.", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, (string)null)
			.GotoDialogState("sister_answer_2")
			.PlayerOption("{=aLaA3jZ2}I'm going to free you, and kill every last one of those slavers!", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, (string)null)
			.GotoDialogState("sister_answer_2")
			.EndPlayerOptions()
			.NpcLine("{=w83SHIYa}Can you get me out of here?", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), "sister_answer_2", (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=21BSwRCQ}Those timbers on your cell look thick. I don't have time now to chop through them.", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, (string)null)
			.GotoDialogState("sister_answer_3")
			.PlayerOption("{=kfHpv0Jg}I'll finish off the slavers and sail this ship out of here, then we can break you out.", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, (string)null)
			.GotoDialogState("sister_answer_3")
			.EndPlayerOptions()
			.NpcLine("{=jjjS4TLY}I understand. Heaven protect you, {?PLAYER.GENDER}Sister{?}Brother{\\?}![rf:convo_grave]", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), "sister_answer_3", (string)null);
		object obj6 = _003C_003Ec._003C_003E9__44_2;
		if (obj6 == null)
		{
			OnConsequenceDelegate val4 = delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
				{
					Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>().SetTalkedWithSister();
				};
			};
			_003C_003Ec._003C_003E9__44_2 = val4;
			obj6 = (object)val4;
		}
		DialogFlow val5 = obj5.Consequence((OnConsequenceDelegate)obj6).CloseDialog();
		DialogFlow obj7 = DialogFlow.CreateDialogFlow("start", 5200).NpcLine("{=Ja5bHsro}You... You and {QUEST_5_COMPANION.NAME} have been slaughtering my allies all up and down this coast, and now it comes to this.", new OnMultipleConversationConsequenceDelegate(IsPurig), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null);
		object obj8 = _003C_003Ec._003C_003E9__44_3;
		if (obj8 == null)
		{
			OnConditionDelegate val6 = delegate
			{
				if (Mission.Current == null)
				{
					return false;
				}
				StringHelpers.SetCharacterProperties("QUEST_5_COMPANION", NavalStorylineData.Gunnar.CharacterObject, (TextObject)null, false);
				Quest5SetPieceBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>();
				return missionBehavior != null && Hero.OneToOneConversationHero == NavalStorylineData.Purig && missionBehavior.State == Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.BossFightConversationInProgress;
			};
			_003C_003Ec._003C_003E9__44_3 = val6;
			obj8 = (object)val6;
		}
		DialogFlow val7 = obj7.Condition((OnConditionDelegate)obj8).NpcLine("{=naMWdTPV}I was going to forge the Sea Hounds into a weapon of vengeance against the house of Volbjorn.", new OnMultipleConversationConsequenceDelegate(IsPurig), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null).NpcLine("{=MR1tc1Ao}I would have drowned them in their own blood. But to the free warriors of the north, to the men who stood against the tyrant - I would have showered them with gold. I would have given them the fame that they deserved. We would have ruled the northern seas.", new OnMultipleConversationConsequenceDelegate(IsPurig), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.NpcLine("{=7rCvGfgb}But that is all for nothing. Instead, the kings of Nordvyg, the men that {QUEST_5_COMPANION.NAME} and I fought, will have the last laugh. So, do you like what you've wrought?", new OnMultipleConversationConsequenceDelegate(IsPurig), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=fiSglIaN}You'd have been twice the tyrant that Volbjorn was.", new OnMultipleConversationConsequenceDelegate(IsPurig), (string)null, (string)null)
			.GotoDialogState("purig_answer")
			.PlayerOption("{=7pWJKkQx}I don't care about your old wars. You put my sister in a cage.", new OnMultipleConversationConsequenceDelegate(IsPurig), (string)null, (string)null)
			.GotoDialogState("purig_answer")
			.PlayerOption("{=Mkxm5l1N}You are outnumbered. Stop bandying words.", new OnMultipleConversationConsequenceDelegate(IsPurig), (string)null, (string)null)
			.GotoDialogState("purig_answer")
			.EndPlayerOptions()
			.NpcLine("{=U9CfaZTF}Not much honor in having your men just cut me down, is there? Fight me one-to-one. If I win, I go free, and we need never see each other again. If you win, people will remember you as the one who slew the terror of the north.", new OnMultipleConversationConsequenceDelegate(IsPurig), new OnMultipleConversationConsequenceDelegate(IsPlayer), "purig_answer", (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=16CMD4HL}I am willing to duel.", new OnMultipleConversationConsequenceDelegate(IsPurig), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>().StartBossFight(isDuel: true);
				_currentState = FreeTheSeaHoundsCaptivesQuestState.TalkedWithPurigBeforeBossFight;
			})
			.CloseDialog()
			.PlayerOption("{=pspOcQY7}You dare talk to me of honor? Kill him, lads!", new OnMultipleConversationConsequenceDelegate(IsPurig), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Mission.Current.GetMissionBehavior<Quest5SetPieceBattleMissionController>().StartBossFight(isDuel: false);
				_currentState = FreeTheSeaHoundsCaptivesQuestState.TalkedWithPurigBeforeBossFight;
			})
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog();
		DialogFlow val8 = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=bMaepOl8}Had enough, have you? Well, are you going to honor your word and put us ashore?", new OnMultipleConversationConsequenceDelegate(IsPurig), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == NavalStorylineData.Purig && BossFightOutCome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerDefeatedWaitingForConversation))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=da9N56ba}You won fairly, Purig. You and your men shall be put ashore.", new OnMultipleConversationConsequenceDelegate(IsPurig), (string)null, (string)null)
			.NpcLine("{=mnBuBKhI}Good. Perhaps {QUEST_5_COMPANION.NAME} and I will find each other some day and settle things our own way, but you will never see me again.", new OnMultipleConversationConsequenceDelegate(IsPurig), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_isPurigKilledViaConversation = false;
				BossFightOutCome = Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndLetPurigGo;
				StringHelpers.SetCharacterProperties("QUEST_5_COMPANION", NavalStorylineData.Gunnar.CharacterObject, (TextObject)null, false);
				Campaign.Current.ConversationManager.ConversationEndOneShot += BossFightAftermathConversationWithPurigConsequence;
			})
			.CloseDialog()
			.PlayerOption("{=fsumvsjK}I'll repay your treachery in your own coin. Finish him, lads!", new OnMultipleConversationConsequenceDelegate(IsPurig), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				_isPurigKilledViaConversation = true;
				BossFightOutCome = Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndHadPurigKilledAnyway;
				Campaign.Current.ConversationManager.ConversationEndOneShot += BossFightAftermathConversationWithPurigConsequence;
			})
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog();
		TextObject val9 = new TextObject("{=FW5OE4fE}{PLAYER.NAME}... {?PLAYER.GENDER}Sister{?}Brother{\\?}... Heaven's mercy, I had given up hope. I thought I'd die in that dark place, in the power of those cruel men.", (Dictionary<string, object>)null);
		TextObjectExtensions.SetCharacterProperties(val9, "PLAYER", CharacterObject.PlayerCharacter, false);
		TextObject val10 = new TextObject("{=6Bx9b4JH}Heaven bless you, {?PLAYER.GENDER}sister{?}brother{\\?}! I am ready to do my part, for our family and our future! But I can see your men calling you. Get us to safety, and we will speak again.", (Dictionary<string, object>)null);
		TextObjectExtensions.SetCharacterProperties(val10, "PLAYER", CharacterObject.PlayerCharacter, false);
		TextObjectExtensions.SetCharacterProperties(new TextObject("{=V52pdTgC}{PLAYER.NAME}... I hate to interrupt, but we need to move fast. We've got men badly hurt, and our water stocks are low. My lads won't be leaving any loot behind, though, not after they bled for it. We shall see you in Ostican!", (Dictionary<string, object>)null), "PLAYER", CharacterObject.PlayerCharacter, false);
		string text = default(string);
		string text2 = default(string);
		DialogFlow obj9 = DialogFlow.CreateDialogFlow("start", 1200).GenerateToken(ref text).GenerateToken(ref text2)
			.NpcLine(val9, new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => _currentState == FreeTheSeaHoundsCaptivesQuestState.DefeatedPurig))
			.Consequence((OnConsequenceDelegate)delegate
			{
				SpawnBjolgur();
				_currentState = FreeTheSeaHoundsCaptivesQuestState.HeadBackToOstican;
			})
			.BeginPlayerOptions((string)null, false);
		string text3 = text;
		DialogFlow obj10 = obj9.PlayerOption("{=iP0fWuZA}My sister... What you must have gone through...", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, text3);
		string text4 = text;
		DialogFlow obj11 = obj10.PlayerOption("{=0vwGcEoV}You're safe now. Rest. We can speak later.", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, text4).EndPlayerOptions().NpcLine("{=CZ6yprOg}That awful night... I awoke to cries and screaming and smoke. Father and mother... I won't speak of it. Some of those villains grabbed me and threw me over a horse. In the camp I saw our little brother, and my heart sank, but I did not see you, and that gave me hope.", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), text, (string)null)
			.NpcLine("{=O5xn66z4}They separated us and took the younger stronger ones to be marched to the coast. They mocked us, telling us that we would be worked until our deaths on some hot island mine or on a frozen shoreline. I told them that you would come after me with an army of warriors and see them all hanged. I did not believe it, though... I just could not bear to have no answer to their taunts.", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.NpcLine("{=ugyC5nt9}We arrived in Ostican. We were smuggled in by night, as the slave trade was banned by the Vlandian king, though many there clearly profited from it. Eventually Purig came to buy us. He questioned all of us closely, about our families. At first I thought he was trying to find out whether he could get a ransom for us, but no, he was trying to find someone related to you! He feared you, and was keeping me to protect himself from you! That made me proud, despite my misery.", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.NpcLine("{=rTlhgDi8}They threw me in that cell, where you found me, and we sailed from port to port. Sometimes I could press my ear to the door and I could hear Purig discussing his plans to topple the Nord king and build a pirate empire. And I heard your name again and again, as their schemes were foiled and the noose around his neck grew tighter. And then, just a short while ago, I heard your voice at the door of my cell, and I knew Heaven had answered my prayers!", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.BeginPlayerOptions((string)null, false);
		string text5 = text2;
		DialogFlow obj12 = obj11.PlayerOption("{=JUwcYtEY}I would never have given up trying to rescue you, or our little brother or any of us!", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, text5);
		string text6 = text2;
		DialogFlow val11 = obj12.PlayerOption("{=5J3vrPII}Our fortunes have changed. This morning you were a captive, but now you are a lady of rank.", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, text6).EndPlayerOptions().NpcLine(val10, new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(((QuestBase)this).CompleteQuestWithSuccess))
			.CloseDialog();
		Campaign.Current.ConversationManager.AddDialogFlow(val, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val5, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val7, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val8, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val11, (object)null);
		static bool IsGunnar(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
		}
		static bool IsPlayer(IAgent agent)
		{
			return (object)agent.Character == CharacterObject.PlayerCharacter;
		}
		static bool IsPurig(IAgent agent)
		{
			return (object)agent.Character == NavalStorylineData.Purig.CharacterObject;
		}
		static bool IsSister(IAgent agent)
		{
			return (object)agent.Character == StoryModeHeroes.LittleSister.CharacterObject;
		}
	}

	private void SpawnBjolgur()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Bjolgur.CharacterObject);
		val.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
		Vec3 globalPosition = Mission.Current.Scene.FindEntityWithName("free_infantry_spawn_point_0").GlobalPosition;
		val.InitialPosition(ref globalPosition);
		Vec3 lookDirection = Agent.Main.LookDirection;
		Vec2 val2 = ((Vec3)(ref lookDirection)).AsVec2;
		val2 = ((Vec2)(ref val2)).Normalized();
		val.InitialDirection(ref val2);
		val.NoHorses(true);
		Agent val3 = Mission.Current.SpawnAgent(val, false);
		Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)(object)new Agent[1] { val3 }, true);
	}

	private void BossFightAftermathConversationWithPurigConsequence()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		_currentState = FreeTheSeaHoundsCaptivesQuestState.DefeatedPurig;
		TextObject val;
		if (_isPurigKilledViaConversation)
		{
			val = new TextObject("{=T76bsVKF}Your men make quick work of Purig and his crew, assured that few will blame them for giving the Sea Hounds a taste of their own villainy. Meanwhile, you return to the roundship, which your men have already begun to search for loot and captives to free. As hopeful cries well up from the hold, they pry open the hatches, and look below.", (Dictionary<string, object>)null);
			TraitLevelingHelper.OnIssueSolvedThroughQuest(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, 50)
			});
		}
		else
		{
			val = new TextObject("{=bWFRemi6}Purig and his men jump into the waters of the bay and wade to shore. They disappear into the forested cliffs by the fjord. Meanwhile, you return to the Sea Hounds' roundship, which your men have already begun to search for loot and captives to free. As hopeful cries well up from the hold, they pry open the hatches, and look below.", (Dictionary<string, object>)null);
			TraitLevelingHelper.OnIssueSolvedThroughQuest(Hero.MainHero, new Tuple<TraitObject, int>[1]
			{
				new Tuple<TraitObject, int>(DefaultTraits.Honor, -50)
			});
			Clan.PlayerClan.AddRenown(50f, true);
		}
		InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=fNLTX4VS}Sister Saved", (Dictionary<string, object>)null)).ToString(), ((object)val).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), string.Empty, (Action)DuelLostPopUpConsequence, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private bool GunnarInitialMeetingDialogCondition()
	{
		if (_currentState == FreeTheSeaHoundsCaptivesQuestState.EncounteredWithSeaHoundsParty && Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && NavalStorylineData.IsStorylineActivationPossible() && NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest4))
		{
			return Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(FreeTheSeaHoundsCaptivesQuest));
		}
		return false;
	}

	private void GunnarInitialMeetingDialogConsequence()
	{
		_currentState = FreeTheSeaHoundsCaptivesQuestState.TalkedWithGunnarBeforeFight;
	}

	private void DuelLostPopUpConsequence()
	{
		ShowNavalSaveSisterSceneNotification();
	}

	private void AddGameMenus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0024: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0059: Expected O, but got Unknown
		//IL_0059: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_007d: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00b2: Expected O, but got Unknown
		//IL_00b2: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_011c: Expected O, but got Unknown
		//IL_011c: Expected O, but got Unknown
		((QuestBase)this).AddGameMenu("act_3_quest_5_encounter_menu", new TextObject("{=oPap9pvt}You have arrived at your destination, Angranfjord. The entrance to the inlet between forested crags is hard to spot from the open sea, but Crusas points it out to you. You row forward in Crusas' ship while Bjolgur and Lahar hold back, keeping watch for the Shield Brother reinforcements. Soon you see a cluster of vessels, sitting at anchor. This must be Purig's fleet.", (Dictionary<string, object>)null), new OnInitDelegate(game_menu_encounter_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("act_3_quest_5_encounter_menu", "continue", new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null), new OnConditionDelegate(encounter_menu_continue_on_condition), new OnConsequenceDelegate(encounter_menu_continue_on_consequence), false, -1);
		((QuestBase)this).AddGameMenu("act_3_quest_5_mission_menu", new TextObject("{=etH1IHNZ}You manage to put some distance between you and your enemies, and you have a moment to consider how to proceed.", (Dictionary<string, object>)null), new OnInitDelegate(mission_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
		((QuestBase)this).AddGameMenuOption("act_3_quest_5_mission_menu", "checkpoint", new TextObject("{=mBAxWNpo}Try again from last checkpoint", (Dictionary<string, object>)null), new OnConditionDelegate(encounter_menu_checkpoint_on_condition), new OnConsequenceDelegate(encounter_menu_checkpoint_on_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("act_3_quest_5_mission_menu", "start_over", new TextObject("{=lvbqEglM}Start over", (Dictionary<string, object>)null), new OnConditionDelegate(encounter_menu_start_over_on_condition), new OnConsequenceDelegate(encounter_menu_start_over_on_consequence), false, -1);
		((QuestBase)this).AddGameMenuOption("act_3_quest_5_mission_menu", "leave", new TextObject("{=3sRdGQou}Leave", (Dictionary<string, object>)null), new OnConditionDelegate(encounter_menu_leave_on_condition), new OnConsequenceDelegate(encounter_menu_leave_on_consequence), true, -1);
	}

	private void HandleMenuInitState()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (_currentState == FreeTheSeaHoundsCaptivesQuestState.TalkedWithGunnarBeforeFight)
		{
			if (PlayerEncounter.Battle == null)
			{
				PlayerEncounter.StartBattle();
			}
			InitializeSetPieceBattleMission();
		}
		else if (_currentState == FreeTheSeaHoundsCaptivesQuestState.DefeatedPurig)
		{
			PlayerEncounter.LeaveEncounter = true;
			GameMenu.ExitToLast();
			if (BossFightOutCome != Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndHadPurigKilledAnyway && BossFightOutCome != Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndLetPurigGo)
			{
				ShowNavalSaveSisterSceneNotification();
			}
		}
		else if (_currentState == FreeTheSeaHoundsCaptivesQuestState.PlayerLostBossFight && BossFightOutCome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerDefeatedWaitingForConversation)
		{
			CampaignMission.OpenConversationMission(new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, false, false, false, false, false, false), new ConversationCharacterData(NavalStorylineData.Purig.CharacterObject, _seaHoundsParty.Party, false, false, false, false, false, false), "", "", false);
		}
	}

	private void mission_menu_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName(((SettlementComponent)SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)null)).WaitMeshName);
		HandleMenuInitState();
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest5MissionMenu);
	}

	private void game_menu_encounter_on_init(MenuCallbackArgs args)
	{
		if (_lastHitCheckpoint == Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.None || _lastHitCheckpoint == Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.InitializePhase1Part1)
		{
			args.MenuContext.SetBackgroundMeshName("encounter_naval");
			HandleMenuInitState();
		}
		else
		{
			GameMenu.SwitchToMenu("act_3_quest_5_mission_menu");
		}
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest5EncounterMenu);
	}

	[GameMenuInitializationHandler("act_3_quest_5_encounter_menu")]
	private static void quest_game_menus_on_init_background(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName(((SettlementComponent)SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)null)).WaitMeshName);
	}

	private bool encounter_menu_continue_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		return true;
	}

	private void encounter_menu_continue_on_consequence(MenuCallbackArgs args)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		_currentState = FreeTheSeaHoundsCaptivesQuestState.EncounteredWithSeaHoundsParty;
		ConversationCharacterData val = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, false, false, false, false, false, false);
		ConversationCharacterData val2 = default(ConversationCharacterData);
		((ConversationCharacterData)(ref val2))._002Ector(NavalStorylineData.Gunnar.CharacterObject, PartyBase.MainParty, false, false, false, false, false, false);
		CampaignMission.OpenConversationMission(val, val2, "", "", false);
		GameMenu.ActivateGameMenu("act_3_quest_5_mission_menu");
	}

	private bool encounter_menu_checkpoint_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		return CanStartFromCheckPoint();
	}

	private void encounter_menu_checkpoint_on_consequence(MenuCallbackArgs args)
	{
		InitializeSetPieceBattleMission(_lastHitCheckpoint);
	}

	private bool encounter_menu_start_over_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		return !CanStartFromCheckPoint();
	}

	private void encounter_menu_start_over_on_consequence(MenuCallbackArgs args)
	{
		InitializeSetPieceBattleMission();
	}

	private bool encounter_menu_leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void encounter_menu_leave_on_consequence(MenuCallbackArgs args)
	{
		if (MobileParty.MainParty.MapEvent != null)
		{
			MenuHelper.EncounterLeaveConsequence();
		}
		NavalStorylineData.DeactivateNavalStoryline();
		GameMenu.ExitToLast();
	}

	private void InitializeSetPieceBattleMission(Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState checkpoint = Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.InitializePhase1Part1)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		if (NavalStorylineData.Purig.PartyBelongedTo != _seaHoundsParty && !NavalStorylineData.Purig.IsDead)
		{
			if ((int)NavalStorylineData.Purig.HeroState != 1)
			{
				NavalStorylineData.Purig.ChangeState((CharacterStates)1);
			}
			_seaHoundsParty.Party.MemberRoster.AddToCounts(NavalStorylineData.Purig.CharacterObject, 1, false, 0, 0, true, -1);
		}
		NavalMissions.OpenNavalStorylineQuest5SetPieceBattleMission(NavalStorylineData.GetNavalMissionInitializerTemplate("naval_storyline_act_3_quest_5"), _seaHoundsParty, checkpoint);
	}

	protected override void OnStartQuestInternal()
	{
		((QuestBase)this).AddLog(_findSeaHoundsQuestLog, false);
		CreateSeaHoundParty();
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_skatriaIslandsMarker);
		foreach (Ship item in (List<Ship>)(object)MobileParty.MainParty.Ships)
		{
			if (((MBObjectBase)item.ShipHull).StringId == "nord_medium_ship")
			{
				item.ChangeFigurehead(DefaultFigureheads.Raven);
				foreach (KeyValuePair<string, string> nordMediumShipyShipUpgradePiece in _nordMediumShipyShipUpgradePieceList)
				{
					if (!string.IsNullOrEmpty(nordMediumShipyShipUpgradePiece.Value))
					{
						ShipUpgradePiece val = MBObjectManager.Instance.GetObject<ShipUpgradePiece>(nordMediumShipyShipUpgradePiece.Value);
						item.EquipUpgradePiece(nordMediumShipyShipUpgradePiece.Key, val);
					}
				}
			}
			else
			{
				if (!(((MBObjectBase)item.ShipHull).StringId == "aserai_heavy_ship"))
				{
					continue;
				}
				foreach (KeyValuePair<string, string> aseraiHeavyShipUpgradePiece in _aseraiHeavyShipUpgradePieceList)
				{
					if (!string.IsNullOrEmpty(aseraiHeavyShipUpgradePiece.Value))
					{
						ShipUpgradePiece val2 = MBObjectManager.Instance.GetObject<ShipUpgradePiece>(aseraiHeavyShipUpgradePiece.Value);
						item.EquipUpgradePiece(aseraiHeavyShipUpgradePiece.Key, val2);
					}
				}
			}
		}
	}

	private void OnPartyVisibilityChanged(PartyBase party)
	{
		if (_currentState == FreeTheSeaHoundsCaptivesQuestState.GoToSeaHoundPartyPosition && party == _seaHoundsParty.Party && _seaHoundsParty.IsVisible)
		{
			((QuestBase)this).AddLog(_arrivedAngranfjordQuestLog, false);
		}
	}

	private void CanHeroBecomePrisoner(Hero hero, ref bool result)
	{
		if (hero == Hero.MainHero)
		{
			result = false;
		}
	}

	private void OnMapEventEnded(MapEvent mapEvent)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (MobileParty.MainParty.MapEvent == mapEvent && mapEvent.HasWinner)
		{
			_ = mapEvent.WinningSide;
			_ = mapEvent.PlayerSide;
		}
	}

	private void OnHourlyTick()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = _skatriaIslandsMarker.Position;
		CampaignVec2 position2 = MobileParty.MainParty.Position;
		if (((Vec3)(ref position)).Distance(((CampaignVec2)(ref position2)).AsVec3()) > 15f)
		{
			_skatriaIslandsMarker.IsVisibleOnMap = true;
		}
		else
		{
			_skatriaIslandsMarker.IsVisibleOnMap = false;
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		Quest5SetPieceBattleMissionController missionBehavior = ((Mission)mission).GetMissionBehavior<Quest5SetPieceBattleMissionController>();
		if (missionBehavior != null)
		{
			BossFightOutCome = missionBehavior.BossFightOutCome;
			_lastHitCheckpoint = missionBehavior.LastHitCheckpoint;
			_shouldMissionContinueFromCheckpoint = missionBehavior.ShouldMissionContinueFromCheckpoint;
		}
		if (_lastHitCheckpoint != Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.None && _lastHitCheckpoint < Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.End)
		{
			_currentState = FreeTheSeaHoundsCaptivesQuestState.RestartMission;
		}
		if (_currentState > FreeTheSeaHoundsCaptivesQuestState.TalkedWithGunnarBeforeFight)
		{
			if (_currentState == FreeTheSeaHoundsCaptivesQuestState.TalkedWithPurigBeforeBossFight && BossFightOutCome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerDefeatedWaitingForConversation)
			{
				_currentState = FreeTheSeaHoundsCaptivesQuestState.PlayerLostBossFight;
			}
			else if (PlayerEncounter.EncounteredMobileParty == _seaHoundsParty && MapEvent.PlayerMapEvent != null && MapEvent.PlayerMapEvent.HasWinner && MapEvent.PlayerMapEvent.WinningSide == mission.PlayerTeam.Side)
			{
				_currentState = FreeTheSeaHoundsCaptivesQuestState.DefeatedPurig;
			}
		}
	}

	protected override void OnFinalizeInternal()
	{
		DestroySeaHoundParty();
		if (NavalStorylineData.Purig.IsAlive)
		{
			KillCharacterAction.ApplyByRemove(NavalStorylineData.Purig, false, true);
		}
	}

	protected override void OnCompleteWithSuccessInternal()
	{
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act3Quest5Succeeded);
	}

	private void CreateSeaHoundParty()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		Hideout val = SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement x) => x.IsActive));
		Clan val2 = ((IEnumerable<Clan>)Clan.All).FirstOrDefault((Func<Clan, bool>)((Clan x) => ((MBObjectBase)x).StringId == "northern_pirates"));
		PartyTemplateObject val3 = ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("storyline_act3_quest_5_sea_hounds_template") ?? val2.DefaultPartyTemplate;
		_seaHoundsParty = BanditPartyComponent.CreateBanditParty("free_the_sea_hounds_captives_initial_quest_party", val2, ((SettlementComponent)val).Settlement.Hideout, false, val3, _seaHoundsSpawnPosition);
		_seaHoundsParty.Party.SetCustomName(new TextObject("{=SKC3FeGR}Sea Hounds", (Dictionary<string, object>)null));
		_seaHoundsParty.SetPartyUsedByQuest(true);
		_seaHoundsParty.IsInfoHidden = true;
		_seaHoundsParty.IgnoreByOtherPartiesTill(CampaignTime.Years(999f));
		_seaHoundsParty.SetLandNavigationAccess(false);
		_seaHoundsParty.Ai.SetDoNotMakeNewDecisions(true);
		_seaHoundsParty.Party.SetCustomBanner(NavalStorylineData.CorsairBanner);
		MobileParty.UpdateLocator(_seaHoundsParty);
		_seaHoundsParty.MemberRoster.Clear();
		FillParty(_seaHoundsParty, val3, MathF.Round(67f * _strengthModifier));
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_seaHoundsParty);
		foreach (Ship item in (List<Ship>)(object)_seaHoundsParty.Ships)
		{
			item.ChangeFigurehead(DefaultFigureheads.Dragon);
			foreach (KeyValuePair<string, string> seaHoundPartyShipUpgradePiece in _seaHoundPartyShipUpgradePieceList)
			{
				if (!string.IsNullOrEmpty(seaHoundPartyShipUpgradePiece.Value))
				{
					ShipUpgradePiece val4 = MBObjectManager.Instance.GetObject<ShipUpgradePiece>(seaHoundPartyShipUpgradePiece.Value);
					item.EquipUpgradePiece(seaHoundPartyShipUpgradePiece.Key, val4);
				}
			}
		}
	}

	private void DestroySeaHoundParty()
	{
		if (_seaHoundsParty != null && _seaHoundsParty.IsActive)
		{
			DestroyPartyAction.Apply((PartyBase)null, _seaHoundsParty);
		}
	}

	private static void FillParty(MobileParty mobileParty, PartyTemplateObject partyTemplate, int desiredMenCount)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		int num2 = ((IEnumerable<PartyTemplateStack>)partyTemplate.Stacks).Sum((PartyTemplateStack s) => s.MinValue);
		int num3 = ((IEnumerable<PartyTemplateStack>)partyTemplate.Stacks).Sum((PartyTemplateStack s) => s.MaxValue);
		num = ((desiredMenCount < num2) ? ((float)desiredMenCount / (float)num2 - 1f) : ((num2 > desiredMenCount || desiredMenCount > num3) ? ((float)desiredMenCount / (float)num3) : ((float)(desiredMenCount - num2) / (float)(num3 - num2))));
		for (int num4 = 0; num4 < ((List<PartyTemplateStack>)(object)partyTemplate.Stacks).Count; num4++)
		{
			PartyTemplateStack val = ((List<PartyTemplateStack>)(object)partyTemplate.Stacks)[num4];
			int minValue = val.MinValue;
			int maxValue = val.MaxValue;
			int num5 = ((-1f <= num && num < 0f) ? MBRandom.RoundRandomized((float)minValue + (float)minValue * num) : ((!(0f <= num) || !(num <= 1f)) ? MBRandom.RoundRandomized((float)maxValue * num) : MBRandom.RoundRandomized((float)minValue + (float)(maxValue - minValue) * num)));
			if (num5 > 0)
			{
				mobileParty.MemberRoster.AddToCounts(val.Character, num5, false, 0, 0, true, -1);
			}
		}
		while (mobileParty.MemberRoster.TotalManCount > desiredMenCount)
		{
			int index = MBRandom.RoundRandomized(MBRandom.RandomFloatRanged((float)(((List<PartyTemplateStack>)(object)partyTemplate.Stacks).Count - 1)));
			CharacterObject character = ((List<PartyTemplateStack>)(object)partyTemplate.Stacks)[index].Character;
			mobileParty.MemberRoster.AddToCounts(character, -1, false, 0, 0, true, -1);
		}
		while (mobileParty.MemberRoster.TotalManCount < desiredMenCount)
		{
			int index2 = MBRandom.RoundRandomized(MBRandom.RandomFloatRanged((float)(((List<PartyTemplateStack>)(object)partyTemplate.Stacks).Count - 1)));
			CharacterObject character2 = ((List<PartyTemplateStack>)(object)partyTemplate.Stacks)[index2].Character;
			mobileParty.MemberRoster.AddToCounts(character2, 1, false, 0, 0, true, -1);
		}
	}

	private void ShowNavalSaveSisterSceneNotification()
	{
		if (!_isSisterSavedSceneNotificationTriggered)
		{
			MBInformationManager.ShowSceneNotification((SceneNotificationData)(object)new NavalSaveSisterSceneNotificationItem(Hero.MainHero, StoryModeHeroes.LittleSister, OnNavalSaveSisterSceneNotificationClosed));
			_isSisterSavedSceneNotificationTriggered = true;
		}
	}

	private void OnNavalSaveSisterSceneNotificationClosed()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		ConversationCharacterData val = new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, true, false, false, false, true);
		ConversationCharacterData val2 = default(ConversationCharacterData);
		((ConversationCharacterData)(ref val2))._002Ector(StoryModeHeroes.LittleSister.CharacterObject, PartyBase.MainParty, true, true, false, true, false, true);
		CampaignMission.OpenConversationMission(val, val2, "conversation_scene_sea_multi_agent", "", true);
	}

	private void ShowAllyDefeatedPopUp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001c: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		TextObject val = new TextObject("{=cH3Kpkwg}Ally Defeated", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)_allyDefeatedText).ToString(), true, false, ((object)val2).ToString(), (string)null, (Action)OnAllyDefeatedPopUpClosed, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void OnAllyDefeatedPopUpClosed()
	{
		((QuestBase)this).CompleteQuestWithCancel(_allyDefeatedText);
		NavalStorylineData.DeactivateNavalStoryline();
	}

	private bool CanStartFromCheckPoint()
	{
		if (_lastHitCheckpoint != Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.None)
		{
			return _lastHitCheckpoint != Quest5SetPieceBattleMissionController.Quest5SetPieceBattleMissionState.InitializePhase1Part1;
		}
		return false;
	}
}
