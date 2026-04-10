using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace SandBox.ViewModelCollection.Nameplate;

public class SettlementNameplateEventsVM : ViewModel
{
	private List<QuestBase> _relatedQuests;

	private Settlement _settlement;

	private bool _areQuestsDirty;

	private MBBindingList<QuestMarkerVM> _trackQuests;

	private MBBindingList<SettlementNameplateEventItemVM> _eventsList;

	public bool IsEventsRegistered { get; private set; }

	[DataSourceProperty]
	public MBBindingList<QuestMarkerVM> TrackQuests
	{
		get
		{
			return _trackQuests;
		}
		set
		{
			if (value != _trackQuests)
			{
				_trackQuests = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<QuestMarkerVM>>(value, "TrackQuests");
			}
		}
	}

	public MBBindingList<SettlementNameplateEventItemVM> EventsList
	{
		get
		{
			return _eventsList;
		}
		set
		{
			if (value != _eventsList)
			{
				_eventsList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<SettlementNameplateEventItemVM>>(value, "EventsList");
			}
		}
	}

	public SettlementNameplateEventsVM(Settlement settlement)
	{
		_settlement = settlement;
		EventsList = new MBBindingList<SettlementNameplateEventItemVM>();
		TrackQuests = new MBBindingList<QuestMarkerVM>();
		_relatedQuests = new List<QuestBase>();
		if (settlement.IsVillage)
		{
			AddPrimaryProductionIcon();
		}
	}

	public void Tick()
	{
		if (_areQuestsDirty)
		{
			RefreshQuestCounts();
			_areQuestsDirty = false;
		}
	}

	private void PopulateEventList()
	{
		if (Campaign.Current.TournamentManager.GetTournamentGame(_settlement.Town) != null)
		{
			((Collection<SettlementNameplateEventItemVM>)(object)EventsList).Add(new SettlementNameplateEventItemVM(SettlementNameplateEventItemVM.SettlementEventType.Tournament));
		}
	}

	public void RegisterEvents()
	{
		if (!IsEventsRegistered)
		{
			PopulateEventList();
			CampaignEvents.TournamentStarted.AddNonSerializedListener((object)this, (Action<Town>)OnTournamentStarted);
			CampaignEvents.TournamentFinished.AddNonSerializedListener((object)this, (Action<CharacterObject, MBReadOnlyList<CharacterObject>, Town, ItemObject>)OnTournamentFinished);
			CampaignEvents.TournamentCancelled.AddNonSerializedListener((object)this, (Action<Town>)OnTournamentCancelled);
			CampaignEvents.OnNewIssueCreatedEvent.AddNonSerializedListener((object)this, (Action<IssueBase>)OnNewIssueCreated);
			CampaignEvents.OnIssueUpdatedEvent.AddNonSerializedListener((object)this, (Action<IssueBase, IssueUpdateDetails, Hero>)OnIssueUpdated);
			CampaignEvents.OnQuestStartedEvent.AddNonSerializedListener((object)this, (Action<QuestBase>)OnQuestStarted);
			CampaignEvents.QuestLogAddedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, bool>)OnQuestLogAdded);
			CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
			CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
			CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
			CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener((object)this, (Action<PartyBase, Hero>)OnHeroTakenPrisoner);
			IsEventsRegistered = true;
			RefreshQuestCounts();
		}
	}

	public void UnloadEvents()
	{
		if (!IsEventsRegistered)
		{
			return;
		}
		((IMbEventBase)CampaignEvents.TournamentStarted).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.TournamentFinished).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.TournamentCancelled).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnNewIssueCreatedEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnIssueUpdatedEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnQuestStartedEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.QuestLogAddedEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnQuestCompletedEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.SettlementEntered).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnSettlementLeftEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.HeroPrisonerTaken).ClearListeners((object)this);
		int num = ((Collection<SettlementNameplateEventItemVM>)(object)EventsList).Count;
		for (int i = 0; i < num; i++)
		{
			if (((Collection<SettlementNameplateEventItemVM>)(object)EventsList)[i].EventType != SettlementNameplateEventItemVM.SettlementEventType.Production)
			{
				((Collection<SettlementNameplateEventItemVM>)(object)EventsList).RemoveAt(i);
				num--;
				i--;
			}
		}
		IsEventsRegistered = false;
	}

	private void OnTournamentStarted(Town town)
	{
		if (_settlement.Town == null || town != _settlement.Town)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < ((Collection<SettlementNameplateEventItemVM>)(object)EventsList).Count; i++)
		{
			if (((Collection<SettlementNameplateEventItemVM>)(object)EventsList)[i].EventType == SettlementNameplateEventItemVM.SettlementEventType.Tournament)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			((Collection<SettlementNameplateEventItemVM>)(object)EventsList).Add(new SettlementNameplateEventItemVM(SettlementNameplateEventItemVM.SettlementEventType.Tournament));
		}
	}

	private void OnTournamentFinished(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town, ItemObject prize)
	{
		RemoveTournament(town);
	}

	private void OnTournamentCancelled(Town town)
	{
		RemoveTournament(town);
	}

	private void RemoveTournament(Town town)
	{
		if (_settlement.Town == null || town != _settlement.Town || ((IEnumerable<SettlementNameplateEventItemVM>)EventsList).Count((SettlementNameplateEventItemVM e) => e.EventType == SettlementNameplateEventItemVM.SettlementEventType.Tournament) <= 0)
		{
			return;
		}
		int num = -1;
		for (int num2 = 0; num2 < ((Collection<SettlementNameplateEventItemVM>)(object)EventsList).Count; num2++)
		{
			if (((Collection<SettlementNameplateEventItemVM>)(object)EventsList)[num2].EventType == SettlementNameplateEventItemVM.SettlementEventType.Tournament)
			{
				num = num2;
				break;
			}
		}
		if (num != -1)
		{
			((Collection<SettlementNameplateEventItemVM>)(object)EventsList).RemoveAt(num);
		}
		else
		{
			Debug.FailedAssert("There should be a tournament item to remove", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Nameplate\\SettlementNameplateEventsVM.cs", "RemoveTournament", 164);
		}
	}

	private void RefreshQuestCounts()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		_relatedQuests.Clear();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = Campaign.Current.IssueManager.GetNumOfActiveIssuesInSettlement(_settlement, false);
		int numOfAvailableIssuesInSettlement = Campaign.Current.IssueManager.GetNumOfAvailableIssuesInSettlement(_settlement);
		((Collection<QuestMarkerVM>)(object)TrackQuests).Clear();
		List<QuestBase> list = default(List<QuestBase>);
		if (Campaign.Current.QuestManager.TrackedObjects.TryGetValue((ITrackableCampaignObject)(object)_settlement, ref list))
		{
			foreach (QuestBase item in list)
			{
				if (item.IsSpecialQuest && !((IEnumerable<QuestMarkerVM>)TrackQuests).Any((QuestMarkerVM x) => (int)x.IssueQuestFlag == 16))
				{
					((Collection<QuestMarkerVM>)(object)TrackQuests).Add(new QuestMarkerVM((IssueQuestFlags)16, (TextObject)null, (TextObject)null));
					_relatedQuests.Add(item);
				}
				else if (!((IEnumerable<QuestMarkerVM>)TrackQuests).Any((QuestMarkerVM x) => (int)x.IssueQuestFlag == 8))
				{
					((Collection<QuestMarkerVM>)(object)TrackQuests).Add(new QuestMarkerVM((IssueQuestFlags)8, (TextObject)null, (TextObject)null));
					_relatedQuests.Add(item);
				}
			}
		}
		List<(bool, QuestBase)> questsRelatedToSettlement = CampaignUIHelper.GetQuestsRelatedToSettlement(_settlement);
		for (int num5 = 0; num5 < questsRelatedToSettlement.Count; num5++)
		{
			if (questsRelatedToSettlement[num5].Item1)
			{
				if (questsRelatedToSettlement[num5].Item2.IsSpecialQuest)
				{
					num++;
				}
				else
				{
					num4++;
				}
			}
			else if (questsRelatedToSettlement[num5].Item2.IsSpecialQuest)
			{
				num3++;
			}
			else
			{
				num2++;
			}
			_relatedQuests.Add(questsRelatedToSettlement[num5].Item2);
		}
		HandleIssueCount(numOfAvailableIssuesInSettlement, SettlementNameplateEventItemVM.SettlementEventType.AvailableIssue);
		HandleIssueCount(num4, SettlementNameplateEventItemVM.SettlementEventType.ActiveQuest);
		HandleIssueCount(num, SettlementNameplateEventItemVM.SettlementEventType.ActiveStoryQuest);
		HandleIssueCount(num2, SettlementNameplateEventItemVM.SettlementEventType.TrackedIssue);
		HandleIssueCount(num3, SettlementNameplateEventItemVM.SettlementEventType.TrackedStoryQuest);
	}

	private void OnNewIssueCreated(IssueBase issue)
	{
		if (issue.IssueSettlement != _settlement)
		{
			Hero issueOwner = issue.IssueOwner;
			if (((issueOwner != null) ? issueOwner.CurrentSettlement : null) != _settlement)
			{
				return;
			}
		}
		_areQuestsDirty = true;
	}

	private void OnIssueUpdated(IssueBase issue, IssueUpdateDetails details, Hero hero)
	{
		if (issue.IssueSettlement == _settlement && issue.IssueQuest == null)
		{
			_areQuestsDirty = true;
		}
	}

	private void OnQuestStarted(QuestBase quest)
	{
		if (IsQuestRelated(quest))
		{
			_areQuestsDirty = true;
		}
	}

	private void OnQuestLogAdded(QuestBase quest, bool hideInformation)
	{
		if (IsQuestRelated(quest))
		{
			_areQuestsDirty = true;
		}
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails details)
	{
		if (IsQuestRelated(quest))
		{
			_areQuestsDirty = true;
		}
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		if (settlement == _settlement)
		{
			_areQuestsDirty = true;
		}
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (settlement == _settlement)
		{
			_areQuestsDirty = true;
		}
	}

	private void OnHeroTakenPrisoner(PartyBase capturer, Hero prisoner)
	{
		if (prisoner.CurrentSettlement == _settlement)
		{
			_areQuestsDirty = true;
		}
	}

	private void AddPrimaryProductionIcon()
	{
		string stringId = ((MBObjectBase)_settlement.Village.VillageType.PrimaryProduction).StringId;
		string productionIconId = (stringId.Contains("camel") ? "camel" : ((stringId.Contains("horse") || stringId.Contains("mule")) ? "horse" : stringId));
		((Collection<SettlementNameplateEventItemVM>)(object)EventsList).Add(new SettlementNameplateEventItemVM(productionIconId));
	}

	private void HandleIssueCount(int count, SettlementNameplateEventItemVM.SettlementEventType eventType)
	{
		SettlementNameplateEventItemVM settlementNameplateEventItemVM = ((IEnumerable<SettlementNameplateEventItemVM>)EventsList).FirstOrDefault((SettlementNameplateEventItemVM e) => e.EventType == eventType);
		if (count > 0 && settlementNameplateEventItemVM == null)
		{
			((Collection<SettlementNameplateEventItemVM>)(object)EventsList).Add(new SettlementNameplateEventItemVM(eventType));
		}
		else if (count == 0 && settlementNameplateEventItemVM != null)
		{
			((Collection<SettlementNameplateEventItemVM>)(object)EventsList).Remove(settlementNameplateEventItemVM);
		}
	}

	private bool IsQuestRelated(QuestBase quest)
	{
		IssueBase issueOfQuest = IssueManager.GetIssueOfQuest(quest);
		if ((issueOfQuest == null || issueOfQuest.IssueSettlement != _settlement) && !_relatedQuests.Contains(quest))
		{
			return CampaignUIHelper.IsQuestRelatedToSettlement(quest, _settlement);
		}
		return true;
	}
}
