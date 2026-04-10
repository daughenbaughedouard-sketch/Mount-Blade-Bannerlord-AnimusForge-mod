using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
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

public class DisruptSupplyLinesConspiracyQuest : ConspiracyQuestBase
{
	private const int NumberOfSettlementsToVisit = 6;

	private const int SpawnCaravanWaitDaysAfterQuestStarted = 5;

	[SaveableField(1)]
	private readonly Settlement[] _caravanTargetSettlements;

	[SaveableField(2)]
	private MobileParty _questCaravanMobileParty;

	[SaveableField(3)]
	private readonly CampaignTime _questStartTime;

	public override TextObject Title => new TextObject("{=y150haHv}Disrupt Supply Lines", (Dictionary<string, object>)null);

	public override TextObject SideNotificationText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=IPP6MKfy}{MENTOR.LINK} notified you about a weapons caravan that will supply conspirators with weapons and armour.", (Dictionary<string, object>)null);
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
			TextObject val = new TextObject("{=01Y1DAqA}{MENTOR.LINK} has sent you a message: As you may know, I receive reports from my spies in marketplaces around here. There is a merchant who I have been following - I know he is connected with {OTHER_MENTOR.LINK}. Now, I hear he has bought up a large supply of weapons and armor in {QUEST_FROM_SETTLEMENT_NAME}, and plans to travel to {QUEST_TO_SETTLEMENT_NAME}. From there it will move onward. I expect that {OTHER_MENTOR.LINK} is arming {?OTHER_MENTOR.GENDER}her{?}his{\\?} allies in the gangs in that area. If the caravan delivers its load, then I expect we will soon find some of our friends stabbed to death in the streets by hired thugs, and the rest of our friends too frightened to acknowledge us. I need you to track it down and destroy it. Try to intercept it on the first leg of its journey, before it gets to {QUEST_TO_SETTLEMENT_NAME}. If you fail, find out the next town to which it is going. It may take some time to find it, and when you do, it will be well guarded. But I trust in your perseverance, your skill and your understanding of how important this is. Good hunting.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("OTHER_MENTOR", StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? StoryModeHeroes.AntiImperialMentor.CharacterObject : StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			StringHelpers.SetCharacterProperties("MENTOR", base.Mentor.CharacterObject, val, false);
			val.SetTextVariable("QUEST_FROM_SETTLEMENT_NAME", QuestFromSettlement.EncyclopediaLinkWithName);
			val.SetTextVariable("QUEST_TO_SETTLEMENT_NAME", QuestToSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	public override TextObject StartLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=ZKdBlAmp}An arms caravan to resupply the conspirators will be soon on its way.{newline}{MENTOR.LINK}'s message:{newline}\"Our spies have learned about an arms caravan that is attempting to bring the conspirators high quality weapons and armor. We know that it will set out on its route from {QUEST_FROM_SETTLEMENT_NAME} to {QUEST_TO_SETTLEMENT_NAME} after {SPAWN_DAYS} days. We will find out and notify you about the new routes that it takes as it progresses.\"", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", base.Mentor.CharacterObject, val, false);
			val.SetTextVariable("QUEST_FROM_SETTLEMENT_NAME", QuestFromSettlement.EncyclopediaLinkWithName);
			val.SetTextVariable("QUEST_TO_SETTLEMENT_NAME", QuestToSettlement.EncyclopediaLinkWithName);
			val.SetTextVariable("SPAWN_DAYS", 5);
			return val;
		}
	}

	public override float ConspiracyStrengthDecreaseAmount => 75f;

	private TextObject PlayerDefeatedCaravanLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=Db63Pe03}You have defeated the caravan and acquired its supplies. {OTHER_MENTOR.LINK}'s allies will not have their weapons. This will give us time and resources to prepare.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", base.Mentor.CharacterObject, val, false);
			StringHelpers.SetCharacterProperties("OTHER_MENTOR", StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? StoryModeHeroes.AntiImperialMentor.CharacterObject : StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject MainHeroFailedToDisrupt => new TextObject("{=9aRqqx3U}The caravan has delivered its supplies to the conspirators. A stronger adversary awaits us...", (Dictionary<string, object>)null);

	private TextObject MainHeroLostCombat => new TextObject("{=bT9yspaQ}You have lost the battle against the conspiracy's caravan. A stronger adversary awaits us...", (Dictionary<string, object>)null);

	private Settlement QuestFromSettlement => _caravanTargetSettlements[0];

	private Settlement QuestToSettlement => _caravanTargetSettlements[_caravanTargetSettlements.Length - 1];

	public MobileParty ConspiracyCaravan => _questCaravanMobileParty;

	public int CaravanPartySize => 70 + 70 * (int)GetQuestDifficultyMultiplier();

	public DisruptSupplyLinesConspiracyQuest(string questId, Hero questGiver)
		: base(questId, questGiver)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		_questStartTime = CampaignTime.Now;
		List<Settlement> list = new List<Settlement> { GetQuestFromSettlement() };
		for (int i = 1; i <= 6; i++)
		{
			list.Add(GetNextSettlement(list));
		}
		_caravanTargetSettlements = (Settlement[])(object)new Settlement[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			_caravanTargetSettlements[j] = list[j];
		}
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)QuestFromSettlement);
	}

	private Settlement GetQuestFromSettlement()
	{
		Settlement centralSettlement = StoryModeHeroes.ImperialMentor.HomeSettlement;
		Settlement val = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)delegate(Settlement s)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			if (s.IsTown && s.MapFaction != Clan.PlayerClan.MapFaction)
			{
				MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
				CampaignVec2 gatePosition = s.GatePosition;
				CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
				if (mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1))
				{
					if (!StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine)
					{
						return !StoryModeData.IsKingdomImperial(s.OwnerClan.Kingdom);
					}
					return StoryModeData.IsKingdomImperial(s.OwnerClan.Kingdom);
				}
			}
			return false;
		});
		if (val == null)
		{
			val = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)delegate(Settlement s)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				if (s.IsTown)
				{
					MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
					CampaignVec2 gatePosition = s.GatePosition;
					CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
					if (mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1))
					{
						if (!StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine)
						{
							return !StoryModeData.IsKingdomImperial(s.OwnerClan.Kingdom);
						}
						return StoryModeData.IsKingdomImperial(s.OwnerClan.Kingdom);
					}
				}
				return false;
			});
		}
		if (val == null)
		{
			val = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)delegate(Settlement s)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				if (s.IsTown)
				{
					MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
					CampaignVec2 gatePosition = s.GatePosition;
					CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
					return mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1);
				}
				return false;
			});
		}
		return val;
	}

	private Settlement GetNextSettlement(List<Settlement> caravanTargetSettlements)
	{
		Settlement centralSettlement = StoryModeHeroes.ImperialMentor.HomeSettlement;
		Settlement settlement = caravanTargetSettlements.Last();
		Town obj = SettlementHelper.FindNearestTownToSettlement(settlement, (NavigationType)1, (Func<Settlement, bool>)delegate(Settlement s)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (!caravanTargetSettlements.Contains(s) && s.MapFaction != Clan.PlayerClan.MapFaction)
			{
				MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
				CampaignVec2 gatePosition = s.GatePosition;
				CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
				if (mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1) && (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? StoryModeData.IsKingdomImperial(s.OwnerClan.Kingdom) : (!StoryModeData.IsKingdomImperial(s.OwnerClan.Kingdom))) && Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, s, false, false, (NavigationType)1) > 100f)
				{
					return Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, s, false, false, (NavigationType)1) < 500f;
				}
			}
			return false;
		});
		Settlement val = ((obj != null) ? ((SettlementComponent)obj).Settlement : null);
		if (val == null)
		{
			val = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)delegate(Settlement s)
			{
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0051: Unknown result type (might be due to invalid IL or missing references)
				if (!caravanTargetSettlements.Contains(s) && s.IsTown && s.MapFaction != Clan.PlayerClan.MapFaction)
				{
					MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
					CampaignVec2 gatePosition = s.GatePosition;
					CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
					if (mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1) && Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, s, false, false, (NavigationType)1) > 100f)
					{
						return Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, s, false, false, (NavigationType)1) < 500f;
					}
				}
				return false;
			});
		}
		if (val == null)
		{
			val = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)delegate(Settlement s)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				if (!caravanTargetSettlements.Contains(s) && s.IsTown)
				{
					MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
					CampaignVec2 gatePosition = s.GatePosition;
					CampaignVec2 gatePosition2 = centralSettlement.GatePosition;
					if (mapDistanceModel.PathExistBetweenPoints(ref gatePosition, ref gatePosition2, (NavigationType)1) && Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, s, false, false, (NavigationType)1) > 100f)
					{
						return Campaign.Current.Models.MapDistanceModel.GetDistance(settlement, s, false, false, (NavigationType)1) < 500f;
					}
				}
				return false;
			});
		}
		return val;
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
	}

	protected override void HourlyTick()
	{
	}

	protected override void OnTimedOut()
	{
		MobileParty questCaravanMobileParty = _questCaravanMobileParty;
		if (questCaravanMobileParty != null && questCaravanMobileParty.IsActive)
		{
			DestroyPartyAction.Apply((PartyBase)null, _questCaravanMobileParty);
		}
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		if (_questCaravanMobileParty == null || _questCaravanMobileParty != party)
		{
			return;
		}
		if (settlement == QuestToSettlement)
		{
			DestroyPartyAction.Apply((PartyBase)null, _questCaravanMobileParty);
			FailedToDisrupt();
			return;
		}
		int num = Array.IndexOf(_caravanTargetSettlements, settlement) + 1;
		SetPartyAiAction.GetActionForVisitingSettlement(_questCaravanMobileParty, _caravanTargetSettlements[num], (NavigationType)1, false, false);
		if (((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)settlement))
		{
			((QuestBase)this).RemoveTrackedObject((ITrackableCampaignObject)(object)settlement);
		}
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_caravanTargetSettlements[num]);
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (_questCaravanMobileParty != null && _questCaravanMobileParty == party)
		{
			AddLogForSettlementVisit(settlement);
		}
	}

	private void OnMapEventEnded(MapEvent mapEvent)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		if (!mapEvent.IsPlayerMapEvent || _questCaravanMobileParty == null || !mapEvent.InvolvedParties.Contains(_questCaravanMobileParty.Party))
		{
			return;
		}
		if (mapEvent.WinningSide == mapEvent.PlayerSide)
		{
			if (_questCaravanMobileParty.Party.NumberOfHealthyMembers > 0 && _questCaravanMobileParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, _questCaravanMobileParty);
			}
			BattleWon();
		}
		else if ((int)mapEvent.WinningSide != -1)
		{
			if (_questCaravanMobileParty.IsActive)
			{
				DestroyPartyAction.Apply((PartyBase)null, _questCaravanMobileParty);
			}
			BattleLost();
		}
	}

	protected override void DailyTick()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (_questCaravanMobileParty == null)
		{
			CampaignTime questStartTime = _questStartTime;
			if (((CampaignTime)(ref questStartTime)).ElapsedDaysUntilNow >= 5f)
			{
				CreateQuestCaravanParty();
				((QuestBase)this).SetDialogs();
			}
		}
	}

	private void AddLogForSettlementVisit(Settlement settlement)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		TextObject val = new TextObject("{=SVcr0EJM}Caravan is moving on to {TO_SETTLEMENT_LINK} from {FROM_SETTLEMENT_LINK}.", (Dictionary<string, object>)null);
		int num = Array.IndexOf(_caravanTargetSettlements, settlement) + 1;
		val.SetTextVariable("FROM_SETTLEMENT_LINK", settlement.EncyclopediaLinkWithName);
		val.SetTextVariable("TO_SETTLEMENT_LINK", _caravanTargetSettlements[num].EncyclopediaLinkWithName);
		Campaign.Current.CampaignInformationManager.NewMapNoticeAdded((InformationData)(object)new ConspiracyQuestMapNotification((QuestBase)(object)this, val));
		((QuestBase)this).AddLog(val, false);
	}

	private void CreateQuestCaravanParty()
	{
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		PartyTemplateObject val = (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("conspiracy_anti_imperial_special_raider_party_template") : ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>("conspiracy_imperial_special_raider_party_template"));
		Hero val2 = (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine ? StoryModeHeroes.AntiImperialMentor : StoryModeHeroes.ImperialMentor);
		GetAdditionalVisualsForParty(QuestFromSettlement.Culture, out var mountStringId, out var harnessStringId);
		string[] source = new string[5] { "aserai", "battania", "khuzait", "sturgia", "vlandia" };
		Clan val3 = null;
		foreach (Clan item in (List<Clan>)(object)Clan.All)
		{
			if (!item.IsEliminated && !item.IsBanditFaction && !item.IsMinorFaction && ((StoryModeManager.Current.MainStoryLine.IsOnAntiImperialQuestLine && ((MBObjectBase)item.Culture).StringId == "empire") || (StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine && source.Contains(((MBObjectBase)item.Culture).StringId))))
			{
				val3 = item;
				break;
			}
		}
		_questCaravanMobileParty = CustomPartyComponent.CreateCustomPartyWithPartyTemplate(QuestFromSettlement.GatePosition, 0f, QuestFromSettlement, new TextObject("{=eVzg5Mtl}Conspiracy Caravan", (Dictionary<string, object>)null), val3, val, val2, mountStringId, harnessStringId, 4f, true);
		_questCaravanMobileParty.Aggressiveness = 0f;
		_questCaravanMobileParty.MemberRoster.Clear();
		_questCaravanMobileParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("fish"), 20);
		_questCaravanMobileParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("grain"), 40);
		_questCaravanMobileParty.ItemRoster.AddToCounts(MBObjectManager.Instance.GetObject<ItemObject>("butter"), 20);
		DistributeConspiracyRaiderTroopsByLevel(val, _questCaravanMobileParty.Party, CaravanPartySize);
		_questCaravanMobileParty.IgnoreByOtherPartiesTill(((QuestBase)this).QuestDueTime);
		_questCaravanMobileParty.SetPartyUsedByQuest(true);
		SetPartyAiAction.GetActionForVisitingSettlement(_questCaravanMobileParty, _caravanTargetSettlements[1], (NavigationType)1, false, false);
		_questCaravanMobileParty.Ai.SetDoNotMakeNewDecisions(true);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_questCaravanMobileParty);
		_questCaravanMobileParty.IgnoreByOtherPartiesTill(CampaignTime.DaysFromNow(21f));
		AddLogForSettlementVisit(QuestFromSettlement);
	}

	private void GetAdditionalVisualsForParty(CultureObject culture, out string mountStringId, out string harnessStringId)
	{
		if (((MBObjectBase)culture).StringId == "aserai" || ((MBObjectBase)culture).StringId == "khuzait")
		{
			mountStringId = "camel";
			harnessStringId = ((MBRandom.RandomFloat > 0.5f) ? "camel_saddle_a" : "camel_saddle_b");
		}
		else
		{
			mountStringId = "mule";
			harnessStringId = ((MBRandom.RandomFloat > 0.5f) ? "mule_load_a" : ((MBRandom.RandomFloat > 0.5f) ? "mule_load_b" : "mule_load_c"));
		}
	}

	private float GetQuestDifficultyMultiplier()
	{
		return MBMath.ClampFloat((0f + (float)((List<Town>)(object)Clan.PlayerClan.Fiefs).Count * 0.1f + Clan.PlayerClan.CurrentTotalStrength * 0.0008f + Clan.PlayerClan.Renown * 1.5E-05f + (float)((List<Hero>)(object)Clan.PlayerClan.AliveLords).Count * 0.002f + (float)((List<Hero>)(object)Clan.PlayerClan.Companions).Count * 0.01f + (float)((List<Hero>)(object)Clan.PlayerClan.SupporterNotables).Count * 0.001f + (float)Hero.MainHero.OwnedCaravans.Count * 0.01f + (float)PartyBase.MainParty.NumberOfAllMembers * 0.002f + (float)((BasicCharacterObject)CharacterObject.PlayerCharacter).Level * 0.002f) * 0.975f + MBRandom.RandomFloat * 0.025f, 0.1f, 1f);
	}

	private void BattleWon()
	{
		((QuestBase)this).AddLog(PlayerDefeatedCaravanLog, false);
		((QuestBase)this).CompleteQuestWithSuccess();
	}

	private void BattleLost()
	{
		((QuestBase)this).AddLog(MainHeroLostCombat, false);
		((QuestBase)this).CompleteQuestWithFail((TextObject)null);
	}

	private void FailedToDisrupt()
	{
		((QuestBase)this).AddLog(MainHeroFailedToDisrupt, false);
		((QuestBase)this).CompleteQuestWithFail((TextObject)null);
	}

	protected override void SetDialogs()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000015).NpcLine(new TextObject("{=ch9f3A1e}Greetings, {?PLAYER.GENDER}madam{?}sir{\\?}. Why did you stop our caravan? I trust you are not robbing us.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(conversation_with_caravan_master_condition))
			.BeginPlayerOptions((string)null, false)
			.PlayerOption(new TextObject("{=Xx94UrYe}I might be. What are you carrying? Honest goods, or weapons? How about you let us have a look.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine(new TextObject("{=LXGXxKqw}Ah... Well, I suppose we can drop the charade. [ib:hip2][if:convo_nonchalant]I know who sent you, and I suppose you know who sent me. Certainly, you can see my wares, and then you can feel their sharp end in your belly.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.PlayerOption(new TextObject("{=cEaXehHy}I was just checking on something. You can move along.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(cancel_encounter_consequence))
			.CloseDialog()
			.EndPlayerOptions()
			.CloseDialog(), (object)this);
	}

	private bool conversation_with_caravan_master_condition()
	{
		if (_questCaravanMobileParty != null)
		{
			return ConversationHelper.GetConversationCharacterPartyLeader(_questCaravanMobileParty.Party) == CharacterObject.OneToOneConversationCharacter;
		}
		return false;
	}

	private void cancel_encounter_consequence()
	{
		if (PlayerEncounter.Current != null)
		{
			PlayerEncounter.LeaveEncounter = true;
		}
	}

	internal static void AutoGeneratedStaticCollectObjectsDisruptSupplyLinesConspiracyQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(DisruptSupplyLinesConspiracyQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		base.AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_caravanTargetSettlements);
		collectedObjects.Add(_questCaravanMobileParty);
		CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime((object)_questStartTime, collectedObjects);
	}

	internal static object AutoGeneratedGetMemberValue_caravanTargetSettlements(object o)
	{
		return ((DisruptSupplyLinesConspiracyQuest)o)._caravanTargetSettlements;
	}

	internal static object AutoGeneratedGetMemberValue_questCaravanMobileParty(object o)
	{
		return ((DisruptSupplyLinesConspiracyQuest)o)._questCaravanMobileParty;
	}

	internal static object AutoGeneratedGetMemberValue_questStartTime(object o)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((DisruptSupplyLinesConspiracyQuest)o)._questStartTime;
	}
}
