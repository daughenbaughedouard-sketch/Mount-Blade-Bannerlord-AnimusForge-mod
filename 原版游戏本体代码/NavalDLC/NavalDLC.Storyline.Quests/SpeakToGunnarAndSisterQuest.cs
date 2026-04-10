using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Storyline.MissionControllers;
using SandBox.Conversation.MissionLogics;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class SpeakToGunnarAndSisterQuest : QuestBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConditionDelegate _003C_003E9__19_0;

		public static OnConsequenceDelegate _003C_003E9__19_1;

		public static OnConditionDelegate _003C_003E9__19_2;

		public static Func<Village, bool> _003C_003E9__21_0;

		public static Action _003C_003E9__23_0;

		internal bool _003CInitializeDialogues_003Eb__19_0()
		{
			if (Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SpeakToGunnarAndSisterQuest)) && Hero.OneToOneConversationHero == NavalStorylineData.Gunnar)
			{
				return Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement;
			}
			return false;
		}

		internal void _003CInitializeDialogues_003Eb__19_1()
		{
			MissionConversationLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionConversationLogic>();
			if (missionBehavior != null)
			{
				missionBehavior.DisableStartConversation(true);
			}
		}

		internal bool _003CInitializeDialogues_003Eb__19_2()
		{
			int num;
			if (Hero.OneToOneConversationHero == StoryModeHeroes.LittleSister)
			{
				num = (Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SpeakToGunnarAndSisterQuest)) ? 1 : 0);
				if (num != 0)
				{
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
					StringHelpers.SetCharacterProperties("BROTHER", StoryModeHeroes.ElderBrother.CharacterObject, (TextObject)null, false);
					StringHelpers.SetCharacterProperties("SISTER", StoryModeHeroes.LittleSister.CharacterObject, (TextObject)null, false);
					MBTextManager.SetTextVariable("CLAN_NAME", Clan.PlayerClan.Name, false);
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}

		internal bool _003CMakeGunnarNotable_003Eb__21_0(Village x)
		{
			return ((MBObjectBase)((SettlementComponent)x).Settlement).StringId == "village_N1_2";
		}

		internal void _003CSisterFinalConversationConsequence_003Eb__23_0()
		{
			CampaignMission.Current.EndMission();
		}
	}

	private const string GunnarsLongshipStringId = "northern_medium_ship";

	private const string Tier3NordInfantryStringId = "nord_spear_warrior";

	private const string Tier4NordInfantryStringId = "nord_vargr";

	private const int Tier3NordInfantryCount = 10;

	private const int Tier4NordInfantryCount = 10;

	[SaveableField(1)]
	private Quest5SetPieceBattleMissionController.BossFightOutComeEnum _bossFightOutcome;

	private TextObject _startLog
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=vhqRTs5p}Look for {GUNNAR.NAME} and your sister in Ostican harbor.", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "GUNNAR", NavalStorylineData.Gunnar.CharacterObject, false);
			return val;
		}
	}

	public override TextObject Title => new TextObject("{=9VzikXB0}Speak to Gunnar and Your Sister", (Dictionary<string, object>)null);

	public override bool IsRemainingTimeHidden => true;

	public override string SpecialQuestType => "NavalStoryline";

	public SpeakToGunnarAndSisterQuest(Quest5SetPieceBattleMissionController.BossFightOutComeEnum bossFightOutcome)
		: base("naval_storyline_act3_quest5_end", NavalStorylineData.Gunnar, CampaignTime.Never, 0)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_bossFightOutcome = bossFightOutcome;
	}

	protected override void OnStartQuest()
	{
		InitializeDialogues();
		((QuestBase)this).AddLog(_startLog, false);
		StoryModeHeroes.LittleSister.HitPoints = StoryModeHeroes.LittleSister.MaxHitPoints;
	}

	protected override void SetDialogs()
	{
	}

	protected override void InitializeQuestOnGameLoad()
	{
		if (_bossFightOutcome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.None || _bossFightOutcome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerDefeatedWaitingForConversation)
		{
			_bossFightOutcome = Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerRefusedTheDuel;
		}
		InitializeDialogues();
	}

	protected override void OnCompleteWithSuccess()
	{
		MakeGunnarNotable();
		NavalDLCHelpers.AddSisterToClan();
	}

	private void InitializeDialogues()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
		DecideGunnarDialogue();
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 1500).NpcLine("{=!}{GUNNAR_FINAL_DIALOG_LINE_1}", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__19_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = () => Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SpeakToGunnarAndSisterQuest)) && Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement;
			_003C_003Ec._003C_003E9__19_0 = val;
			obj2 = (object)val;
		}
		DialogFlow obj3 = obj.Condition((OnConditionDelegate)obj2);
		object obj4 = _003C_003Ec._003C_003E9__19_1;
		if (obj4 == null)
		{
			OnConsequenceDelegate val2 = delegate
			{
				MissionConversationLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionConversationLogic>();
				if (missionBehavior != null)
				{
					missionBehavior.DisableStartConversation(true);
				}
			};
			_003C_003Ec._003C_003E9__19_1 = val2;
			obj4 = (object)val2;
		}
		DialogFlow val3 = obj3.Consequence((OnConsequenceDelegate)obj4).NpcLine("{=!}{GUNNAR_FINAL_DIALOG_LINE_2}", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).NpcLine("{=xxxjoDxM}My men, though... I've had a word with them, and some of them have been quite impressed by your leadership. They want to follow you, if you'll have them. And as I mentioned, they prefer to sail on our ship here, the Wave-Steed, so I guess that's yours too, if you'll have it. She'll carry you well, especially in the rough seas of the north.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=qatVcvrX}I welcome your ship and crew.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(OnPlayerWelcomedGunnarsCrew))
			.GotoDialogState("gunnar_final_dialog_token_1")
			.PlayerOption("{=FaZ1dSuh}I am honored, but I cannot take on your companions.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_final_dialog_token_1")
			.EndPlayerOptions()
			.NpcLine("{=!}{GUNNAR_FINAL_DIALOG_LINE_3}", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, "gunnar_final_dialog_token_1", (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=uh2W7Jh3}Farewell. Perhaps I will take you up on your reputation.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_final_dialog_token_2")
			.PlayerOption("{=C94hXQp3}Farewell, and good hunting.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("gunnar_final_dialog_token_2")
			.EndPlayerOptions()
			.NpcLine("{=Vcr7BYxJ}Farewell, {PLAYER.NAME}.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, "gunnar_final_dialog_token_2", (string)null)
			.CloseDialog();
		DialogFlow obj5 = DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=L3NhSRHr}{PLAYER.NAME}... It's good to be free, and back on land. Things have changed so much though. Men follow you, and jump to their feet to obey your orders, and speak of your deeds...", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj6 = _003C_003Ec._003C_003E9__19_2;
		if (obj6 == null)
		{
			OnConditionDelegate val4 = delegate
			{
				int num;
				if (Hero.OneToOneConversationHero == StoryModeHeroes.LittleSister)
				{
					num = (Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SpeakToGunnarAndSisterQuest)) ? 1 : 0);
					if (num != 0)
					{
						StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
						StringHelpers.SetCharacterProperties("BROTHER", StoryModeHeroes.ElderBrother.CharacterObject, (TextObject)null, false);
						StringHelpers.SetCharacterProperties("SISTER", StoryModeHeroes.LittleSister.CharacterObject, (TextObject)null, false);
						MBTextManager.SetTextVariable("CLAN_NAME", Clan.PlayerClan.Name, false);
					}
				}
				else
				{
					num = 0;
				}
				return (byte)num != 0;
			};
			_003C_003Ec._003C_003E9__19_2 = val4;
			obj6 = (object)val4;
		}
		DialogFlow val5 = obj5.Condition((OnConditionDelegate)obj6).NpcLine("{=bqNHSlsb}One moment I am a slave and the next I seem to be some sort of noble lady... I need some time to rest. I will seek out our brother {BROTHER.NAME}.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).BeginPlayerOptions((string)null, false)
			.PlayerOption("{=VNEiqDzI}Of course, {SISTER.NAME}. Join {BROTHER.NAME}, and take all the time you need.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("sister_end_conversation_token")
			.PlayerOption("{=cESGiaPI}Things have indeed changed. Rest now, but remember that you are of the {CLAN_NAME}, and you must learn to command respect.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState("sister_end_conversation_token")
			.EndPlayerOptions()
			.NpcLine("{=WFFv3fyb}Thank you again, {PLAYER.NAME}. I will pray nightly to Heaven for your safety.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, "sister_end_conversation_token", (string)null)
			.Consequence(new OnConsequenceDelegate(SisterFinalConversationConsequence))
			.CloseDialog();
		Campaign.Current.ConversationManager.AddDialogFlow(val3, (object)null);
		Campaign.Current.ConversationManager.AddDialogFlow(val5, (object)null);
	}

	private void DecideGunnarDialogue()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		TextObject val;
		TextObject val2;
		if (_bossFightOutcome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerRefusedTheDuel)
		{
			val = new TextObject("{=JoBwweim}Well, {PLAYER.NAME}... Your sister is free, thank the gods. You gave Purig the death he deserved. None will mourn him.", (Dictionary<string, object>)null);
			val2 = new TextObject("{=bTCuEZW9}As for the Sea Hounds, I hear, they've mostly scattered. It's time for me to return to my home in Beinland. I've settled what I wish to settle, and all this rowing and ramming and climbing and jostling and fighting is hard on my old bones.", (Dictionary<string, object>)null);
		}
		else if (_bossFightOutcome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedAndWonTheDuel)
		{
			val = new TextObject("{=AmwwLMvJ}Well, {PLAYER.NAME}... Your sister is free, thank the gods. You gave Purig a far more honorable death than he deserved. Men will speak well of you.", (Dictionary<string, object>)null);
			val2 = new TextObject("{=bTCuEZW9}As for the Sea Hounds, I hear, they've mostly scattered. It's time for me to return to my home in Beinland. I've settled what I wish to settle, and all this rowing and ramming and climbing and jostling and fighting is hard on my old bones.", (Dictionary<string, object>)null);
		}
		else if (_bossFightOutcome == Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndLetPurigGo)
		{
			val = new TextObject("{=4rXR7jR9}Well, {PLAYER.NAME}... Your sister is free, thank the gods. Purig may have gotten away, but I doubt the Sea Hounds will be troubling us much more.", (Dictionary<string, object>)null);
			val2 = new TextObject("{=GqHo4JE2}It was an honorable thing, to duel him, and I am glad you kept your word to him, though he did not deserve it. For my part, though, I owe him nothing. I will continue to hunt him, and as it is much easier for him to evade a large group than a single hunter, I will do so alone.", (Dictionary<string, object>)null);
		}
		else
		{
			val = new TextObject("{=qGZZRhKj}Well, {PLAYER.NAME}... Your sister is free, thank the gods.  Purig is dead, and none will mourn him. I might that wish his death could have come some other way, but I will not dwell on it.", (Dictionary<string, object>)null);
			val2 = new TextObject("{=aJ8bK4oo}The Sea Hounds, I hear, they've mostly scattered. It's time for me to return to my home in Beinland. I've settled what I wish to settle, and all this rowing and ramming and climbing and jostling and fighting is hard on my old bones.", (Dictionary<string, object>)null);
		}
		TextObject val3 = ((_bossFightOutcome != Quest5SetPieceBattleMissionController.BossFightOutComeEnum.PlayerAcceptedTheDuelLostItAndLetPurigGo) ? new TextObject("{=IGnbxJHn}You should come see me in my village, Lagshofn, in Beinland. It's not much, not for a {?PLAYER.GENDER}warrior{?}man{\\?} like you, who's no doubt seen all the wonders of the Empire and the lands beyond, but we can pass a summer's night on the beach and drink to our deeds.", (Dictionary<string, object>)null) : new TextObject("{=1PPiv2ns}I suspect Purig will try to travel as far from these parts as possible. Perhaps deep into the south, or to the east... Perhaps I will take years to find him, or perhaps my old age will finally catch up to me on the road or on the seas. I do not know if we will meet again.", (Dictionary<string, object>)null));
		MBTextManager.SetTextVariable("GUNNAR_FINAL_DIALOG_LINE_1", val, false);
		MBTextManager.SetTextVariable("GUNNAR_FINAL_DIALOG_LINE_2", val2, false);
		MBTextManager.SetTextVariable("GUNNAR_FINAL_DIALOG_LINE_3", val3, false);
	}

	private void MakeGunnarNotable()
	{
		Village val = ((IEnumerable<Village>)Village.All).FirstOrDefault((Func<Village, bool>)((Village x) => ((MBObjectBase)((SettlementComponent)x).Settlement).StringId == "village_N1_2"));
		if (val != null)
		{
			TeleportHeroAction.ApplyImmediateTeleportToSettlement(NavalStorylineData.Gunnar, ((SettlementComponent)val).Settlement);
		}
	}

	private void OnPlayerWelcomedGunnarsCrew()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		Ship val = new Ship(MBObjectManager.Instance.GetObject<ShipHull>("northern_medium_ship"));
		val.SetName(new TextObject("{=EUAsSTeT}Wave-Steed", (Dictionary<string, object>)null));
		ChangeShipOwnerAction.ApplyByLooting(PartyBase.MainParty, val);
		CharacterObject val2 = MBObjectManager.Instance.GetObject<CharacterObject>("nord_spear_warrior");
		MobileParty.MainParty.MemberRoster.AddToCounts(val2, 10, false, 0, 0, true, -1);
		CharacterObject val3 = MBObjectManager.Instance.GetObject<CharacterObject>("nord_vargr");
		MobileParty.MainParty.MemberRoster.AddToCounts(val3, 10, false, 0, 0, true, -1);
		if (!MobileParty.MainParty.Anchor.IsValid && Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.HasPort)
		{
			MobileParty.MainParty.Anchor.SetSettlement(Settlement.CurrentSettlement);
		}
		TextObject val4 = new TextObject("{=06sIBlHR}{NUMBER} troops and {SHIP_NAME} were added to your party.", (Dictionary<string, object>)null);
		val4.SetTextVariable("NUMBER", 20);
		val4.SetTextVariable("SHIP_NAME", val.Name);
		InformationManager.DisplayMessage(new InformationMessage(((object)val4).ToString(), new Color(0f, 1f, 0f, 1f)));
	}

	private void SisterFinalConversationConsequence()
	{
		((QuestBase)this).CompleteQuestWithSuccess();
		MissionConversationLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionConversationLogic>();
		if (missionBehavior != null)
		{
			missionBehavior.DisableStartConversation(false);
		}
		Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
		{
			CampaignMission.Current.EndMission();
		};
	}
}
