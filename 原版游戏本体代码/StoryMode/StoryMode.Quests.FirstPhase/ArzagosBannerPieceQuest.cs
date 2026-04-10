using System;
using System.Collections.Generic;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
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

namespace StoryMode.Quests.FirstPhase;

public class ArzagosBannerPieceQuest : StoryModeQuestBase
{
	public enum HideoutBattleEndState
	{
		None,
		Retreated,
		Defeated,
		Victory
	}

	private const int MainPartyHealHitPointLimit = 50;

	private const int RaiderPartySize = 10;

	private const int RaiderPartyCount = 2;

	private const string ArzagosRaiderPartyStringId = "arzagos_banner_piece_quest_raider_party_";

	[SaveableField(1)]
	private readonly Settlement _hideout;

	[SaveableField(2)]
	private readonly List<MobileParty> _raiderParties;

	[SaveableField(3)]
	private HideoutBattleEndState _hideoutBattleEndState;

	private TextObject _startQuestLog => new TextObject("{=wvHvnEog}Find the hideout that Arzagos told you about and get the next banner piece.", (Dictionary<string, object>)null);

	public override TextObject Title => new TextObject("{=ay1gPPsP}Find Another Piece of the Banner for Arzagos", (Dictionary<string, object>)null);

	public override bool IsRemainingTimeHidden => false;

	public ArzagosBannerPieceQuest(Hero questGiver, Settlement hideout)
		: base("arzagos_banner_piece_quest", questGiver, StoryModeManager.Current.MainStoryLine.FirstPhase.FirstPhaseEndTime)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		_hideout = hideout;
		_raiderParties = new List<MobileParty>();
		InitializeHideout();
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_hideout);
		((QuestBase)this).SetDialogs();
		((QuestBase)this).InitializeQuestOnCreation();
		((QuestBase)this).AddLog(_startQuestLog, false);
		_hideoutBattleEndState = HideoutBattleEndState.None;
	}

	protected override void OnFinalize()
	{
		((QuestBase)this).OnFinalize();
	}

	protected override void HourlyTick()
	{
		if (!_hideout.Hideout.IsInfested || !_hideout.Hideout.IsSpotted || !_hideout.IsVisible)
		{
			InitializeHideout();
		}
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.IsSettlementBusyEvent.AddNonSerializedListener((object)this, (ReferenceAction<Settlement, object, int>)IsSettlementBusy);
		CampaignEvents.OnHideoutDeactivatedEvent.AddNonSerializedListener((object)this, (Action<Settlement>)OnHideoutCleared);
	}

	private void IsSettlementBusy(Settlement settlement, object asker, ref int priority)
	{
		if (asker != this && settlement == _hideout)
		{
			priority = Math.Max(priority, 400);
		}
	}

	private void OnHideoutCleared(Settlement hideout)
	{
		if (hideout == _hideout)
		{
			MobileParty lastAttackerParty = hideout.LastAttackerParty;
			if (lastAttackerParty != null && lastAttackerParty.IsMainParty && (_hideoutBattleEndState == HideoutBattleEndState.None || PlayerEncounter.Current.ForceHideoutSendTroops))
			{
				_hideoutBattleEndState = HideoutBattleEndState.Victory;
				StoryMode.StoryModePhases.FirstPhase.Instance.CollectBannerPiece();
				((QuestBase)this).CompleteQuestWithSuccess();
				_hideoutBattleEndState = HideoutBattleEndState.None;
			}
		}
	}

	protected override void SetDialogs()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("hero_main_options", 100).PlayerLine(new TextObject("{=dlBFVkDj}About the task you gave me...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(conversation_lord_task_given_on_condition))
			.NpcLine(new TextObject("{=a0JxUMgo}What happened? Did you find the piece of the banner?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver))
			.PlayerLine(new TextObject("{=rY0fdQSb}No, I am still working on it...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog(), (object)this);
	}

	private bool conversation_lord_task_given_on_condition()
	{
		if (Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver)
		{
			return ((QuestBase)this).IsOngoing;
		}
		return false;
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
	}

	private void InitializeHideout()
	{
		_hideout.Hideout.IsSpotted = true;
		_hideout.IsVisible = true;
		_hideoutBattleEndState = HideoutBattleEndState.None;
		if (_hideout.Hideout.IsInfested)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (!_hideout.Hideout.IsInfested)
			{
				_raiderParties.Add(CreateRaiderParty(i));
			}
		}
	}

	private MobileParty CreateRaiderParty(int number)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		Clan hideoutClan = GetHideoutClan(_hideout);
		MobileParty obj = BanditPartyComponent.CreateBanditParty("arzagos_banner_piece_quest_raider_party_" + number, hideoutClan, _hideout.Hideout, false, (PartyTemplateObject)null, _hideout.GatePosition);
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(((MBObjectBase)_hideout.Culture).StringId + "_bandit");
		obj.MemberRoster.AddToCounts(val, 5, false, 0, 0, true, -1);
		obj.Party.SetCustomName(new TextObject("{=u1Pkt4HC}Raiders", (Dictionary<string, object>)null));
		obj.ActualClan = hideoutClan;
		obj.Position = _hideout.Position;
		obj.Party.SetVisualAsDirty();
		float num = obj.Party.CalculateCurrentStrength();
		int num2 = (int)(1f * MBRandom.RandomFloat * 20f * num + 50f);
		obj.InitializePartyTrade(num2);
		obj.SetMoveGoToSettlement(_hideout, (NavigationType)1, false);
		obj.Ai.SetDoNotMakeNewDecisions(true);
		obj.SetPartyUsedByQuest(true);
		EnterSettlementAction.ApplyForParty(obj, _hideout);
		return obj;
	}

	private void OnMapEventEnded(MapEvent mapEvent)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		if (PlayerEncounter.Current == null || !mapEvent.IsPlayerMapEvent || Settlement.CurrentSettlement != _hideout)
		{
			return;
		}
		if (mapEvent.WinningSide == mapEvent.PlayerSide)
		{
			_hideoutBattleEndState = HideoutBattleEndState.Victory;
		}
		else if ((int)mapEvent.WinningSide == -1)
		{
			_hideoutBattleEndState = HideoutBattleEndState.Retreated;
			if (Hero.MainHero.IsPrisoner && _raiderParties.Contains(Hero.MainHero.PartyBelongedToAsPrisoner.MobileParty))
			{
				EndCaptivityAction.ApplyByPeace(Hero.MainHero, (Hero)null);
				if (Hero.MainHero.HitPoints < 50)
				{
					Hero.MainHero.Heal(50 - Hero.MainHero.HitPoints, false);
				}
				InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=FPhWhjq7}Defeated", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=6iytd81P}You are defeated by the bandits in the hideout but you managed to escape. You need to wait a while before attacking again.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yQtzabbe}Close", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
			if (((List<MobileParty>)(object)_hideout.Parties).Count == 0)
			{
				InitializeHideout();
			}
			_hideout.Hideout.SetNextPossibleAttackTime(StoryModeData.StorylineQuestHideoutHiddenDuration);
			_hideoutBattleEndState = HideoutBattleEndState.None;
		}
		else
		{
			_hideout.Hideout.SetNextPossibleAttackTime(StoryModeData.StorylineQuestHideoutHiddenDuration);
			_hideoutBattleEndState = HideoutBattleEndState.Defeated;
		}
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		if (_hideoutBattleEndState != HideoutBattleEndState.Victory && Settlement.CurrentSettlement == _hideout && !_hideout.Hideout.IsInfested)
		{
			InitializeHideout();
		}
		if (_hideoutBattleEndState == HideoutBattleEndState.Victory)
		{
			StoryMode.StoryModePhases.FirstPhase.Instance.CollectBannerPiece();
			((QuestBase)this).CompleteQuestWithSuccess();
			_hideoutBattleEndState = HideoutBattleEndState.None;
		}
		else
		{
			if (_hideoutBattleEndState != HideoutBattleEndState.Retreated && _hideoutBattleEndState != HideoutBattleEndState.Defeated)
			{
				return;
			}
			if (Hero.MainHero.IsPrisoner)
			{
				EndCaptivityAction.ApplyByPeace(Hero.MainHero, (Hero)null);
				if (Hero.MainHero.HitPoints < 50)
				{
					Hero.MainHero.Heal(50 - Hero.MainHero.HitPoints, false);
				}
				InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=FPhWhjq7}Defeated", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=btAV7mmq}You are defeated by the raiders in the hideout but you managed to escape. You need to wait a while before attacking again.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yQtzabbe}Close", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
				if (((List<MobileParty>)(object)_hideout.Parties).Count == 0)
				{
					InitializeHideout();
				}
			}
			_hideoutBattleEndState = HideoutBattleEndState.None;
		}
	}

	private Clan GetHideoutClan(Settlement hideout)
	{
		foreach (Clan item in (List<Clan>)(object)Clan.All)
		{
			if (item.Culture == _hideout.Culture && item.IsBanditFaction && !((MBObjectBase)item).StringId.Equals("looters"))
			{
				return item;
			}
		}
		return null;
	}

	internal static void AutoGeneratedStaticCollectObjectsArzagosBannerPieceQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(ArzagosBannerPieceQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_hideout);
		collectedObjects.Add(_raiderParties);
	}

	internal static object AutoGeneratedGetMemberValue_hideout(object o)
	{
		return ((ArzagosBannerPieceQuest)o)._hideout;
	}

	internal static object AutoGeneratedGetMemberValue_raiderParties(object o)
	{
		return ((ArzagosBannerPieceQuest)o)._raiderParties;
	}

	internal static object AutoGeneratedGetMemberValue_hideoutBattleEndState(object o)
	{
		return ((ArzagosBannerPieceQuest)o)._hideoutBattleEndState;
	}
}
