using System.Collections.Generic;
using SandBox.Conversation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.Quests;

public class SaveTheCrewmenQuest : NavalStorylineQuestBase
{
	private const string QuestFinishMenuId = "save_the_crewmen_placeholder_menu";

	private bool _willProgressStoryline;

	public override string SpecialQuestType => "NavalStoryline";

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act1;

	public override bool WillProgressStoryline => _willProgressStoryline;

	protected override string MainPartyTemplateStringId => string.Empty;

	public override TextObject Title => new TextObject("{=tvGCC1BF}Save the Crewmen", (Dictionary<string, object>)null);

	private TextObject DescriptionLogText => new TextObject("{=PSjYdlCe}Rescue the merchant sailors who jumped overboard to escape the pirates.", (Dictionary<string, object>)null);

	public SaveTheCrewmenQuest(string questId, Hero questGiver)
		: base(questId, questGiver, CampaignTime.Never, 0)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((QuestBase)this).AddLog(DescriptionLogText, false);
		((QuestBase)this).SetDialogs();
	}

	protected override void SetDialogs()
	{
		AddPlayerSavedCrewDialog();
	}

	protected override void InitializeQuestOnGameLoadInternal()
	{
		AddGameMenus();
	}

	protected override void OnStartQuestInternal()
	{
		_willProgressStoryline = true;
		AddGameMenus();
	}

	protected override void HourlyTick()
	{
	}

	protected override void RegisterEventsInternal()
	{
	}

	private void AddPlayerSavedCrewDialog()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_003d: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0072: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_0096: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=kPB4vvTD}Thank you. Heaven be praised. We thought we'd escaped the arrows only to be drowned by the waves. Heaven protect us all.", new OnMultipleConversationConsequenceDelegate(IsSavedCrew), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null).Condition((OnConditionDelegate)(() => IsSavedCrew((IAgent)(object)ConversationMission.OneToOneConversationAgent)))
			.NpcLine("{=GVBtIsvA}Think nothing of it, lads. You'd have done the same for any of us, one sailor for another.", new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsSavedCrew), (string)null, (string)null)
			.NpcLine("{=zQRrXKQH}So look, lads… Purig is still around, but I suspect he's overladen and undermanned. I doubt he can find us before nightfall, which is good, because I don't think we can outfight him. By my reckoning, we're still not far from Ostican. So row, my boys, for Ostican and safety!", new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsMainAgent), (string)null, (string)null)
			.Consequence((OnConsequenceDelegate)delegate
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += OnCrewSaved;
			})
			.CloseDialog(), (object)this);
	}

	private void OnCrewSaved()
	{
		Mission current = Mission.Current;
		((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null).FinalizeMission();
		Campaign.Current.GameMenuManager.SetNextMenu("save_the_crewmen_placeholder_menu");
	}

	private void CompleteQuest()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		PlayerEncounter.Finish(true);
		((QuestBase)this).CompleteQuestWithSuccess();
		for (int num = ((List<Ship>)(object)MobileParty.MainParty.Ships).Count - 1; num >= 0; num--)
		{
			((List<Ship>)(object)MobileParty.MainParty.Ships)[num].Owner = null;
		}
		Ship val = new Ship(MBObjectManager.Instance.GetObject<ShipHull>("northern_trade_ship"));
		ChangeShipOwnerAction.ApplyByTransferring(PartyBase.MainParty, val);
		GameState activeState = GameStateManager.Current.ActiveState;
		MapState val2;
		if ((val2 = (MapState)(object)((activeState is MapState) ? activeState : null)) != null)
		{
			val2.Handler.TeleportCameraToMainParty();
		}
		NavalStorylineData.OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint.Act1CaptivitySucceeded);
	}

	private void AddGameMenus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0024: Expected O, but got Unknown
		((QuestBase)this).AddGameMenu("save_the_crewmen_placeholder_menu", new TextObject("{=!}TEMP", (Dictionary<string, object>)null), new OnInitDelegate(naval_storyline_act_3_quest_1_setpiece_menu_on_init), (MenuOverlayType)4, (MenuFlags)0);
	}

	private void naval_storyline_act_3_quest_1_setpiece_menu_on_init(MenuCallbackArgs args)
	{
		CompleteQuest();
	}

	private bool IsGunnar(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
	}

	private bool IsMainAgent(IAgent agent)
	{
		return (object)agent == Agent.Main;
	}

	private bool IsSavedCrew(IAgent agent)
	{
		Mission current = Mission.Current;
		return ((current != null) ? current.GetMissionBehavior<NavalStorylineCaptivityMissionController>() : null)?.IsSavedCrew(agent) ?? false;
	}
}
