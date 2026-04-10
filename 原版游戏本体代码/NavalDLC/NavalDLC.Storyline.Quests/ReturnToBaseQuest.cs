using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class ReturnToBaseQuest : QuestBase
{
	private const string QuestFinishInvisibleMenuId = "return_to_base_placeholder";

	[SaveableField(0)]
	private bool _popupShown;

	public override string SpecialQuestType => "NavalStoryline";

	public override TextObject Title
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=B9l3S9qh}Return to {SETTLEMENT_NAME}", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT_NAME", NavalStorylineData.HomeSettlement.Name);
			return val;
		}
	}

	public override bool IsRemainingTimeHidden => true;

	private TextObject _descriptionLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=vmWnfbJb}Sail back to {SETTLEMENT_LINK} and prepare for your next move.", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.HomeSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	private TextObject _successLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=NJcCXXu9}You have returned to {SETTLEMENT_LINK} and agreed to meet with Gunnar in the port after getting some much-needed rest.", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT_LINK", NavalStorylineData.HomeSettlement.EncyclopediaLinkWithName);
			return val;
		}
	}

	public ReturnToBaseQuest(string questId, Hero questGiver)
		: base(questId, questGiver, CampaignTime.Never, 0)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((QuestBase)this).AddLog(_descriptionLogText, false);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)NavalStorylineData.HomeSettlement);
	}

	protected override void SetDialogs()
	{
	}

	protected override void InitializeQuestOnGameLoad()
	{
		AddGameMenus();
	}

	protected override void OnStartQuest()
	{
		AddGameMenus();
		_popupShown = NavalStorylineData.GetStorylineStage() < NavalStorylineData.NavalStorylineStage.Act2 || GetDistanceToOstican() < Campaign.Current.EstimatedAverageLordPartySpeed * 0.8f * (float)CampaignTime.HoursInDay;
		if (!_popupShown)
		{
			Campaign.Current.GameMenuManager.SetNextMenu("return_to_base_placeholder");
		}
	}

	private void ShowReturnPopUp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0034: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		TextObject val = new TextObject("{=VxcduBO7}Return to Ostican", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=g1ZFrb3E}Do you want to go to Ostican right away?", (Dictionary<string, object>)null);
		TextObject val3 = new TextObject("{=7Hj13O18}Yes, take me to Ostican", (Dictionary<string, object>)null);
		TextObject val4 = new TextObject("{=l3eSbQJM}No, I will go there myself", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, true, ((object)val3).ToString(), ((object)val4).ToString(), (Action)FinishQuest, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
		if (Campaign.Current.CurrentMenuContext != null)
		{
			GameMenu.ExitToLast();
		}
		_popupShown = true;
	}

	private void AddGameMenus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0024: Expected O, but got Unknown
		((QuestBase)this).AddGameMenu("return_to_base_placeholder", new TextObject("{=!}TEMP", (Dictionary<string, object>)null), new OnInitDelegate(return_to_ostican_menu_on_init), (MenuOverlayType)4, (MenuFlags)0);
	}

	private void return_to_ostican_menu_on_init(MenuCallbackArgs args)
	{
		ShowReturnPopUp();
	}

	protected override void HourlyTick()
	{
	}

	protected override void RegisterEvents()
	{
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)Tick);
	}

	private void Tick(float dt)
	{
		if (!_popupShown)
		{
			ShowReturnPopUp();
		}
	}

	private void OnGameMenuOpened(MenuCallbackArgs args)
	{
		if (MobileParty.MainParty.CurrentSettlement == NavalStorylineData.HomeSettlement && ((QuestBase)this).IsOngoing)
		{
			FinishQuest();
			return;
		}
		PlayerEncounter current = PlayerEncounter.Current;
		if (((current != null) ? current.EncounterSettlementAux : null) == NavalStorylineData.HomeSettlement && ((QuestBase)this).IsOngoing)
		{
			FinishQuest();
		}
	}

	private void FinishQuest()
	{
		((QuestBase)this).CompleteQuestWithSuccess();
		NavalStorylineData.DeactivateNavalStoryline();
	}

	protected override void OnCompleteWithSuccess()
	{
		((QuestBase)this).AddLog(_successLogText, false);
		if (!Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(ScourgeoftheSeasQuest)))
		{
			((QuestBase)new ScourgeoftheSeasQuest()).StartQuest();
		}
	}

	private float GetDistanceToOstican()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 position = MobileParty.MainParty.Position;
		return ((CampaignVec2)(ref position)).Distance(NavalStorylineData.HomeSettlement.PortPosition);
	}
}
