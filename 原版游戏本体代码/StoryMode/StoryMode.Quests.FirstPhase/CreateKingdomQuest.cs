using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace StoryMode.Quests.FirstPhase;

public class CreateKingdomQuest : StoryModeQuestBase
{
	[SaveableField(1)]
	private readonly bool _isImperial;

	private const int PartySizeRequirement = 100;

	private const int SettlementCountRequirement = 1;

	[SaveableField(2)]
	private bool _hasPlayerCreatedKingdom;

	[SaveableField(9)]
	private JournalLog _leftKingdomLog;

	[SaveableField(10)]
	private Kingdom _playerCreatedKingdom;

	[SaveableField(4)]
	private readonly JournalLog _clanTierRequirementLog;

	[SaveableField(5)]
	private readonly JournalLog _partySizeRequirementLog;

	[SaveableField(6)]
	private readonly JournalLog _settlementOwnershipRequirementLog;

	[SaveableField(7)]
	private readonly JournalLog _clanIndependenceRequirementLog;

	private TextObject _onQuestStartedImperialLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=N5Qg5ick}You told {MENTOR.LINK} that you will create your own imperial faction. You can do that by speaking to one of your governors once you fulfill the requirements. {?MENTOR.GENDER}She{?}He{\\?} expects to talk to you once you succeed.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _onQuestStartedAntiImperialLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=AxKDQJ4G}You told {MENTOR.LINK} that you will create your own kingdom to defeat the Empire. You can do that by speaking to one of your governors once you fulfill the requirements. {?MENTOR.GENDER}She{?}He{\\?} expects to talk to you once you succeed.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _imperialKingdomCreatedLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=UnjgFmnE}Heeding the advice of {MENTOR.LINK}, you have created an imperial faction. You can tell {?MENTOR.GENDER}her{?}him{\\?} that you will support your own kingdom.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", StoryModeHeroes.ImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _antiImperialKingdomCreatedLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			TextObject val = new TextObject("{=BekWpXmR}Heeding the advice of {MENTOR.LINK}, you have created a kingdom to oppose the Empire. You can tell {?MENTOR.GENDER}her{?}him{\\?} that you will support your own kingdom.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("MENTOR", StoryModeHeroes.AntiImperialMentor.CharacterObject, val, false);
			return val;
		}
	}

	private TextObject _leftKingdomAfterCreatingLogText => new TextObject("{=nNavD2NO}You left the kingdom you have created. You can only support kingdoms that you are a part of.", (Dictionary<string, object>)null);

	private TextObject _clanTierRequirementLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			TextObject val = new TextObject("{=QxeKZ3nE}Reach Clan Tier {CLAN_TIER}", (Dictionary<string, object>)null);
			val.SetTextVariable("CLAN_TIER", Campaign.Current.Models.KingdomCreationModel.MinimumClanTierToCreateKingdom);
			return val;
		}
	}

	private TextObject _partySizeRequirementLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			TextObject val = new TextObject("{=NzQq2qp1}Gather {PARTY_SIZE} Troops", (Dictionary<string, object>)null);
			val.SetTextVariable("PARTY_SIZE", 100);
			return val;
		}
	}

	private TextObject _settlementOwnershipRequirementLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			TextObject val = new TextObject("{=Bo66bfTh}Own {?IS_IMPERIAL}an Imperial Settlement{?}a Settlement{\\?} ", (Dictionary<string, object>)null);
			val.SetTextVariable("IS_IMPERIAL", _isImperial ? 1 : 0);
			return val;
		}
	}

	private TextObject _clanIndependenceRequirementLogText => new TextObject("{=a0ZKBj6P}Be an independent clan", (Dictionary<string, object>)null);

	private TextObject _questFailedLogText => new TextObject("{=tVlZTOst}You have chosen a different path.", (Dictionary<string, object>)null);

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			TextObject val = new TextObject("{=HhFHRs7N}Create {?IS_IMPERIAL}an Imperial Faction{?}a Non-Imperial Kingdom{\\?}", (Dictionary<string, object>)null);
			val.SetTextVariable("IS_IMPERIAL", _isImperial ? 1 : 0);
			return val;
		}
	}

	public override bool IsRemainingTimeHidden => false;

	public CreateKingdomQuest(Hero questGiver)
		: base("main_storyline_create_kingdom_quest_" + ((StoryModeHeroes.ImperialMentor == questGiver) ? "1" : "0"), questGiver, StoryModeManager.Current.MainStoryLine.FirstPhase.FirstPhaseEndTime)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Expected O, but got Unknown
		_isImperial = StoryModeHeroes.ImperialMentor == questGiver;
		((QuestBase)this).SetDialogs();
		if (_isImperial)
		{
			((QuestBase)this).AddLog(_onQuestStartedImperialLogText, false);
		}
		else
		{
			((QuestBase)this).AddLog(_onQuestStartedAntiImperialLogText, false);
		}
		int minimumClanTierToCreateKingdom = Campaign.Current.Models.KingdomCreationModel.MinimumClanTierToCreateKingdom;
		_clanTierRequirementLog = ((QuestBase)this).AddDiscreteLog(_clanTierRequirementLogText, new TextObject("{=tTLvo8sM}Clan Tier", (Dictionary<string, object>)null), (int)MathF.Clamp((float)Clan.PlayerClan.Tier, 0f, (float)minimumClanTierToCreateKingdom), minimumClanTierToCreateKingdom, (TextObject)null, false);
		_partySizeRequirementLog = ((QuestBase)this).AddDiscreteLog(_partySizeRequirementLogText, new TextObject("{=aClquusd}Troop Count", (Dictionary<string, object>)null), (int)MathF.Clamp((float)(MobileParty.MainParty.MemberRoster.TotalManCount - MobileParty.MainParty.MemberRoster.TotalWounded), 0f, 100f), 100, (TextObject)null, false);
		_clanIndependenceRequirementLog = ((QuestBase)this).AddDiscreteLog(_clanIndependenceRequirementLogText, new TextObject("{=qa0o7xaj}Clan Independence", (Dictionary<string, object>)null), (Clan.PlayerClan.Kingdom == null) ? 1 : 0, 1, (TextObject)null, false);
		int num = (int)MathF.Clamp((float)(_isImperial ? ((IEnumerable<Settlement>)Clan.PlayerClan.Settlements).Count((Settlement t) => t.IsFortification && t.Culture == StoryModeData.ImperialCulture) : ((IEnumerable<Settlement>)Clan.PlayerClan.Settlements).Count((Settlement t) => t.IsFortification)), 0f, 1f);
		_settlementOwnershipRequirementLog = ((QuestBase)this).AddDiscreteLog(_settlementOwnershipRequirementLogText, new TextObject("{=gL3WCqM5}Settlement Count", (Dictionary<string, object>)null), num, 1, (TextObject)null, false);
		((QuestBase)this).InitializeQuestOnCreation();
		CheckPlayerClanDiplomaticState(Clan.PlayerClan.Kingdom);
	}

	protected override void SetDialogs()
	{
		((QuestBase)this).DiscussDialogFlow = GetMentorDialogueFlow();
	}

	protected override void InitializeQuestOnGameLoad()
	{
		((QuestBase)this).SetDialogs();
	}

	protected override void HourlyTick()
	{
	}

	private DialogFlow GetMentorDialogueFlow()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		return DialogFlow.CreateDialogFlow("quest_discuss", 300).NpcLine("{=kbyqtszZ}I'm listening..", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)(() => Hero.OneToOneConversationHero == ((QuestBase)this).QuestGiver))
			.PlayerLine("{=wErSpkjy}I'm still working on it.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog();
	}

	private void OnClanTierIncreased(Clan clan, bool showNotification)
	{
		if (!_hasPlayerCreatedKingdom && clan == Clan.PlayerClan)
		{
			((QuestBase)this).UpdateQuestTaskStage(_clanTierRequirementLog, (int)MathF.Clamp((float)Clan.PlayerClan.Tier, 0f, (float)Campaign.Current.Models.KingdomCreationModel.MinimumClanTierToCreateKingdom));
		}
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification)
	{
		if (clan == Clan.PlayerClan)
		{
			CheckPlayerClanDiplomaticState(newKingdom);
		}
	}

	private void CheckPlayerClanDiplomaticState(Kingdom newKingdom)
	{
		if (newKingdom == null)
		{
			if (_hasPlayerCreatedKingdom)
			{
				_leftKingdomLog = ((QuestBase)this).AddLog(_leftKingdomAfterCreatingLogText, false);
				_hasPlayerCreatedKingdom = false;
			}
			((QuestBase)this).UpdateQuestTaskStage(_clanIndependenceRequirementLog, 1);
		}
		else if (newKingdom.RulingClan == Clan.PlayerClan)
		{
			_playerCreatedKingdom = newKingdom;
			if (StoryModeData.IsKingdomImperial(newKingdom))
			{
				if (_isImperial)
				{
					_hasPlayerCreatedKingdom = true;
					if (_leftKingdomLog != null)
					{
						((QuestBase)this).RemoveLog(_leftKingdomLog);
					}
					else
					{
						((QuestBase)this).AddLog(_imperialKingdomCreatedLogText, false);
					}
				}
				else
				{
					((QuestBase)this).UpdateQuestTaskStage(_clanIndependenceRequirementLog, 0);
				}
			}
			else if (!_isImperial)
			{
				_hasPlayerCreatedKingdom = true;
				if (_leftKingdomLog != null)
				{
					((QuestBase)this).RemoveLog(_leftKingdomLog);
				}
				else
				{
					((QuestBase)this).AddLog(_antiImperialKingdomCreatedLogText, false);
				}
			}
			else
			{
				((QuestBase)this).UpdateQuestTaskStage(_clanIndependenceRequirementLog, 0);
			}
		}
		else if (_playerCreatedKingdom == newKingdom && _isImperial == StoryModeData.IsKingdomImperial(newKingdom))
		{
			((QuestBase)this).RemoveLog(_leftKingdomLog);
		}
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
	{
		if (_hasPlayerCreatedKingdom || (newOwner != Hero.MainHero && oldOwner != Hero.MainHero))
		{
			return;
		}
		int num = -1;
		if (_isImperial && settlement.Culture == StoryModeData.ImperialCulture)
		{
			num = ((IEnumerable<Settlement>)Clan.PlayerClan.Settlements).Count((Settlement t) => t.IsFortification && t.Culture == StoryModeData.ImperialCulture);
		}
		else if (!_isImperial)
		{
			num = ((IEnumerable<Settlement>)Clan.PlayerClan.Settlements).Count((Settlement t) => t.IsFortification);
		}
		if (num != -1)
		{
			((QuestBase)this).UpdateQuestTaskStage(_settlementOwnershipRequirementLog, (int)MathF.Clamp((float)num, 0f, 1f));
		}
	}

	private void OnPartySizeChanged(PartyBase party)
	{
		if (!_hasPlayerCreatedKingdom && party == PartyBase.MainParty)
		{
			int num = (int)MathF.Clamp((float)(MobileParty.MainParty.MemberRoster.TotalManCount - MobileParty.MainParty.MemberRoster.TotalWounded), 0f, 100f);
			((QuestBase)this).UpdateQuestTaskStage(_partySizeRequirementLog, num);
		}
	}

	private void MainStoryLineChosen(MainStoryLineSide chosenSide)
	{
		if (_hasPlayerCreatedKingdom && ((chosenSide == MainStoryLineSide.CreateImperialKingdom && _isImperial) || (chosenSide == MainStoryLineSide.CreateAntiImperialKingdom && !_isImperial)))
		{
			((QuestBase)this).CompleteQuestWithSuccess();
		}
		else
		{
			((QuestBase)this).CompleteQuestWithCancel(_questFailedLogText);
		}
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.ClanTierIncrease.AddNonSerializedListener((object)this, (Action<Clan, bool>)OnClanTierIncreased);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
		CampaignEvents.OnPartySizeChangedEvent.AddNonSerializedListener((object)this, (Action<PartyBase>)OnPartySizeChanged);
		StoryModeEvents.OnMainStoryLineSideChosenEvent.AddNonSerializedListener((object)this, (Action<MainStoryLineSide>)MainStoryLineChosen);
	}

	internal static void AutoGeneratedStaticCollectObjectsCreateKingdomQuest(object o, List<object> collectedObjects)
	{
		((MBObjectBase)(CreateKingdomQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
	}

	protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
	{
		((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
		collectedObjects.Add(_leftKingdomLog);
		collectedObjects.Add(_playerCreatedKingdom);
		collectedObjects.Add(_clanTierRequirementLog);
		collectedObjects.Add(_partySizeRequirementLog);
		collectedObjects.Add(_settlementOwnershipRequirementLog);
		collectedObjects.Add(_clanIndependenceRequirementLog);
	}

	internal static object AutoGeneratedGetMemberValue_isImperial(object o)
	{
		return ((CreateKingdomQuest)o)._isImperial;
	}

	internal static object AutoGeneratedGetMemberValue_hasPlayerCreatedKingdom(object o)
	{
		return ((CreateKingdomQuest)o)._hasPlayerCreatedKingdom;
	}

	internal static object AutoGeneratedGetMemberValue_leftKingdomLog(object o)
	{
		return ((CreateKingdomQuest)o)._leftKingdomLog;
	}

	internal static object AutoGeneratedGetMemberValue_playerCreatedKingdom(object o)
	{
		return ((CreateKingdomQuest)o)._playerCreatedKingdom;
	}

	internal static object AutoGeneratedGetMemberValue_clanTierRequirementLog(object o)
	{
		return ((CreateKingdomQuest)o)._clanTierRequirementLog;
	}

	internal static object AutoGeneratedGetMemberValue_partySizeRequirementLog(object o)
	{
		return ((CreateKingdomQuest)o)._partySizeRequirementLog;
	}

	internal static object AutoGeneratedGetMemberValue_settlementOwnershipRequirementLog(object o)
	{
		return ((CreateKingdomQuest)o)._settlementOwnershipRequirementLog;
	}

	internal static object AutoGeneratedGetMemberValue_clanIndependenceRequirementLog(object o)
	{
		return ((CreateKingdomQuest)o)._clanIndependenceRequirementLog;
	}
}
