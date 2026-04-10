using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.PlayerClanQuests;

public class RebuildPlayerClanQuest : StoryModeQuestBase
{
	private const int GoldGoal = 2000;

	private const int ClanTierRenownGoal = 50;

	private const int RenownReward = 25;

	private const int HiredCompanionGoal = 1;

	private bool _finishQuest;

	[SaveableField(1)]
	private JournalLog _goldGoalLog;

	[SaveableField(2)]
	private JournalLog _partySizeGoalLog;

	[SaveableField(3)]
	private JournalLog _clanTierGoalLog;

	[SaveableField(4)]
	private JournalLog _hireCompanionGoalLog;

	private static int _partySizeGoal => Campaign.Current.Models.BanditDensityModel.GetMinimumTroopCountForHideoutMission(MobileParty.MainParty, false);

	private TextObject _startQuestLogText => new TextObject("{=IITkXnnU}Calradia is a land full of peril - but also opportunities. To face the challenges that await, you will need to build up your clan.{newline}Your brother told you that there are many ways to go about this but that none forego coin. Trade would be one means to this end, fighting and selling off captured bandits in town another. Whatever path you choose to pursue, travelling alone would make you easy pickings for whomever came across your trail.{newline}You know that you can recruit men to follow you from the notables of villages and towns, though they may ask you for a favor or two of their own before they allow you access to their more valued fighters.{newline}Naturally, you may also find more unique characters in the taverns of Calradia. However, these tend to favor more established clans.", (Dictionary<string, object>)null);

	private TextObject _goldGoalLogText => new TextObject("{=bXYFXLgg}Increase your denars by 1000", (Dictionary<string, object>)null);

	private TextObject _partySizeGoalLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			TextObject val = new TextObject("{=b6hQWKHe}Grow your party to {PARTY_SIZE} men", (Dictionary<string, object>)null);
			val.SetTextVariable("PARTY_SIZE", _partySizeGoal);
			return val;
		}
	}

	private TextObject _clanTierGoalLogText => new TextObject("{=RbXiEdXk}Reach Clan Tier 1", (Dictionary<string, object>)null);

	private TextObject _hireCompanionGoalLogText => new TextObject("{=e8Tjf8Ph}Hire 1 Companion", (Dictionary<string, object>)null);

	private TextObject _successLogText => new TextObject("{=eJX7rhch}You have successfully rebuilt your clan.", (Dictionary<string, object>)null);

	public override TextObject Title => new TextObject("{=bESRdcRo}Establish Your Clan", (Dictionary<string, object>)null);

	public RebuildPlayerClanQuest()
		: base("rebuild_player_clan_storymode_quest", null, CampaignTime.Never)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_finishQuest = false;
		((QuestBase)this).SetDialogs();
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.HeroOrPartyTradedGold.AddNonSerializedListener((object)this, (Action<(Hero, PartyBase), (Hero, PartyBase), (int, string), bool>)HeroOrPartyTradedGold);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
		CampaignEvents.OnTroopRecruitedEvent.AddNonSerializedListener((object)this, (Action<Hero, Settlement, Hero, CharacterObject, int>)OnTroopRecruited);
		CampaignEvents.RenownGained.AddNonSerializedListener((object)this, (Action<Hero, int, bool>)OnRenownGained);
		CampaignEvents.NewCompanionAdded.AddNonSerializedListener((object)this, (Action<Hero>)OnNewCompanionAdded);
	}

	protected override void OnStartQuest()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		((QuestBase)this).AddLog(_startQuestLogText, true);
		_goldGoalLog = ((QuestBase)this).AddDiscreteLog(_goldGoalLogText, new TextObject("{=hYgmzZJX}Denars", (Dictionary<string, object>)null), Hero.MainHero.Gold, 2000, (TextObject)null, true);
		_partySizeGoalLog = ((QuestBase)this).AddDiscreteLog(_partySizeGoalLogText, new TextObject("{=DO4PE3Oo}Current Party Size", (Dictionary<string, object>)null), 1, _partySizeGoal, (TextObject)null, true);
		_clanTierGoalLog = ((QuestBase)this).AddDiscreteLog(_clanTierGoalLogText, new TextObject("{=aZxHIra4}Renown", (Dictionary<string, object>)null), (int)Clan.PlayerClan.Renown, 50, (TextObject)null, true);
		_hireCompanionGoalLog = ((QuestBase)this).AddDiscreteLog(_hireCompanionGoalLogText, new TextObject("{=VLD5416o}Companion Hired", (Dictionary<string, object>)null), 0, 1, (TextObject)null, true);
	}

	protected override void OnCompleteWithSuccess()
	{
		GainRenownAction.Apply(Hero.MainHero, 25f, false);
		((QuestBase)this).AddLog(_successLogText, false);
	}

	protected override void SetDialogs()
	{
	}

	private void HeroOrPartyTradedGold((Hero, PartyBase) giver, (Hero, PartyBase) recipient, (int, string) goldAmount, bool showNotification)
	{
		UpdateProgresses();
	}

	protected override void HourlyTick()
	{
		UpdateProgresses();
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		UpdateProgresses();
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		UpdateProgresses();
	}

	private void OnMapEventEnded(MapEvent mapEvent)
	{
		UpdateProgresses();
	}

	private void OnTroopRecruited(Hero recruiterHero, Settlement recruitmentSettlement, Hero recruitmentSource, CharacterObject troop, int amount)
	{
		UpdateProgresses();
	}

	private void OnRenownGained(Hero hero, int gainedRenown, bool doNotNotify)
	{
		UpdateProgresses();
	}

	private void OnNewCompanionAdded(Hero newCompanion)
	{
		UpdateProgresses();
	}

	private void UpdateProgresses()
	{
		_goldGoalLog.UpdateCurrentProgress((Hero.MainHero.Gold > 2000) ? 2000 : Hero.MainHero.Gold);
		_partySizeGoalLog.UpdateCurrentProgress((PartyBase.MainParty.MemberRoster.TotalManCount > _partySizeGoal) ? _partySizeGoal : PartyBase.MainParty.MemberRoster.TotalManCount);
		_clanTierGoalLog.UpdateCurrentProgress((Clan.PlayerClan.Renown > 50f) ? 50 : ((int)Clan.PlayerClan.Renown));
		_hireCompanionGoalLog.UpdateCurrentProgress((((List<Hero>)(object)Clan.PlayerClan.Companions).Count > 1) ? 1 : ((List<Hero>)(object)Clan.PlayerClan.Companions).Count);
		if (_goldGoalLog.CurrentProgress >= 2000 && _partySizeGoalLog.CurrentProgress >= _partySizeGoal && _clanTierGoalLog.CurrentProgress >= 50 && _hireCompanionGoalLog.CurrentProgress >= 1 && !_finishQuest)
		{
			_finishQuest = true;
			((QuestBase)this).CompleteQuestWithSuccess();
		}
	}

	internal static void AutoGeneratedStaticCollectObjectsRebuildPlayerClanQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(RebuildPlayerClanQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_goldGoalLog);
		collectedObjects.Add(_partySizeGoalLog);
		collectedObjects.Add(_clanTierGoalLog);
		collectedObjects.Add(_hireCompanionGoalLog);
	}

	internal static object AutoGeneratedGetMemberValue_goldGoalLog(object o)
	{
		return ((RebuildPlayerClanQuest)o)._goldGoalLog;
	}

	internal static object AutoGeneratedGetMemberValue_partySizeGoalLog(object o)
	{
		return ((RebuildPlayerClanQuest)o)._partySizeGoalLog;
	}

	internal static object AutoGeneratedGetMemberValue_clanTierGoalLog(object o)
	{
		return ((RebuildPlayerClanQuest)o)._clanTierGoalLog;
	}

	internal static object AutoGeneratedGetMemberValue_hireCompanionGoalLog(object o)
	{
		return ((RebuildPlayerClanQuest)o)._hireCompanionGoalLog;
	}
}
