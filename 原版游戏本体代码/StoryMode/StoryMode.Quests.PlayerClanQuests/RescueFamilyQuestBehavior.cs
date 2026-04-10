using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Missions.MissionLogics.Hideout;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Definition;
using TaleWorlds.SaveSystem.Load;
using TaleWorlds.SaveSystem.Resolvers;

namespace StoryMode.Quests.PlayerClanQuests;

public class RescueFamilyQuestBehavior : CampaignBehaviorBase
{
	public class RescueFamilyQuest : StoryModeQuestBase
	{
		public class RebuildPlayerClanQuestBehaviorTypeDefiner : SaveableTypeDefiner
		{
			public RebuildPlayerClanQuestBehaviorTypeDefiner()
				: base(4140000)
			{
			}

			protected override void DefineClassTypes()
			{
				((SaveableTypeDefiner)this).AddClassDefinition(typeof(RescueFamilyQuest), 1, (IObjectResolver)null);
			}

			protected override void DefineEnumTypes()
			{
				((SaveableTypeDefiner)this).AddEnumDefinition(typeof(RescueFamilyQuestStateEnum), 11, (IEnumResolver)null);
			}
		}

		private enum RescueFamilyQuestStateEnum
		{
			None,
			ReunionTalkWithRadagosDone,
			HideoutTalkWithRadagosDone,
			HideoutBattleInProgress,
			ExecutionTalkWithGalterDone,
			ReunionTalkWithBrotherDone,
			GoodbyeTalkWithRadagosDone
		}

		private const int RaiderPartySize = 10;

		private const int RaiderPartyCount = 2;

		private const string RescueFamilyRaiderPartyStringId = "rescue_family_quest_raider_party_";

		private Hero _radagos;

		private Hero _hideoutBoss;

		private Settlement _targetSettlementForSiblings;

		[SaveableField(1)]
		private readonly Settlement _hideout;

		[SaveableField(7)]
		private readonly List<MobileParty> _raiderParties;

		[SaveableField(8)]
		private RescueFamilyQuestStateEnum _rescueFamilyQuestState;

		private TextObject _startQuestLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=FyzsAZx8}{RADAGOS.LINK} said that he knows where your siblings are. He offered to attack together. He will wait for you at the hideout that he mentioned about near {SETTLEMENT_LINK}. You can see the hideout marked on the map.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("RADAGOS", _radagos.CharacterObject, val, false);
				Town val2 = SettlementHelper.FindNearestTownToSettlement(_hideout.SettlementComponent.Settlement, (NavigationType)1, (Func<Settlement, bool>)null);
				val.SetTextVariable("SETTLEMENT_LINK", ((SettlementComponent)val2).Settlement.EncyclopediaLinkWithName);
				return val;
			}
		}

		private TextObject _defeatedQuestLogText
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Expected O, but got Unknown
				TextObject val = new TextObject("{=Ga8mDgab}You've been defeated at {HIDEOUT_BOSS.LINK}'s hideout. You can attack again when you are ready.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("HIDEOUT_BOSS", _hideoutBoss.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject _letGoRadagosEndQuestLogText
		{
			get
			{
				TextObject val = GameTexts.FindText("rescue_family_quest_let_go_radagos_quest_log", (string)null);
				StringHelpers.SetCharacterProperties("RADAGOS", _radagos.CharacterObject, val, false);
				return val;
			}
		}

		private TextObject _executeRadagosEndQuestLogText
		{
			get
			{
				TextObject val = GameTexts.FindText("rescue_family_quest_execute_radagos_quest_log", (string)null);
				StringHelpers.SetCharacterProperties("RADAGOS", _radagos.CharacterObject, val, false);
				return val;
			}
		}

		public override TextObject Title => new TextObject("{=HPNuqbSf}Rescue Your Family", (Dictionary<string, object>)null);

		public RescueFamilyQuest()
			: base("rescue_your_family_storymode_quest", null, CampaignTime.Never)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			StoryModeManager.Current.MainStoryLine.FamilyRescued = true;
			_radagos = StoryModeHeroes.Radagos;
			_radagos.CharacterObject.SetTransferableInPartyScreen(false);
			_radagos.CharacterObject.SetTransferableInHideouts(false);
			_hideoutBoss = StoryModeHeroes.RadagosHenchman;
			_targetSettlementForSiblings = null;
			_hideout = ((SettlementComponent)SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement s) => !s.IsSettlementBusy((object)this)))).Settlement;
			_rescueFamilyQuestState = RescueFamilyQuestStateEnum.None;
			_raiderParties = new List<MobileParty>();
			InitializeHideout();
			((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_hideout);
			((QuestBase)this).SetDialogs();
			AddGameMenus();
		}

		[LoadInitializationCallback]
		private void OnLoad(MetaData metaData, ObjectLoadData objectLoadData)
		{
			if (objectLoadData.HasMember(2, (int)((TypeDefinitionBase)objectLoadData.TypeDefinition).TypeLevel))
			{
				bool num = (bool)objectLoadData.GetMemberValueBySaveId(2, (int)((TypeDefinitionBase)objectLoadData.TypeDefinition).TypeLevel);
				bool flag = (bool)objectLoadData.GetMemberValueBySaveId(3, (int)((TypeDefinitionBase)objectLoadData.TypeDefinition).TypeLevel);
				bool flag2 = (bool)objectLoadData.GetMemberValueBySaveId(4, (int)((TypeDefinitionBase)objectLoadData.TypeDefinition).TypeLevel);
				bool flag3 = (bool)objectLoadData.GetMemberValueBySaveId(5, (int)((TypeDefinitionBase)objectLoadData.TypeDefinition).TypeLevel);
				if (num)
				{
					_rescueFamilyQuestState = RescueFamilyQuestStateEnum.ReunionTalkWithRadagosDone;
				}
				if (flag)
				{
					_rescueFamilyQuestState = RescueFamilyQuestStateEnum.HideoutTalkWithRadagosDone;
				}
				if (flag2)
				{
					_rescueFamilyQuestState = RescueFamilyQuestStateEnum.ReunionTalkWithBrotherDone;
				}
				if (flag3)
				{
					_rescueFamilyQuestState = RescueFamilyQuestStateEnum.GoodbyeTalkWithRadagosDone;
				}
			}
		}

		protected override void InitializeQuestOnGameLoad()
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			_radagos = StoryModeHeroes.Radagos;
			_radagos.CharacterObject.SetTransferableInPartyScreen(false);
			_radagos.CharacterObject.SetTransferableInHideouts(false);
			_hideoutBoss = StoryModeHeroes.RadagosHenchman;
			((QuestBase)this).SetDialogs();
			AddGameMenus();
			SelectTargetSettlementForSiblings();
			if (!MBSaveLoad.IsUpdatingGameVersion)
			{
				return;
			}
			ApplicationVersion currentVersion = MBSaveLoad.CurrentVersion;
			if (((ApplicationVersion)(ref currentVersion)).IsOlderThan(ApplicationVersion.FromString("v1.4.0", 0)) && _rescueFamilyQuestState == RescueFamilyQuestStateEnum.HideoutTalkWithRadagosDone)
			{
				MapEvent battle = PlayerEncounter.Battle;
				if (((battle != null) ? battle.MapEventSettlement : null) == _hideout)
				{
					_rescueFamilyQuestState = RescueFamilyQuestStateEnum.HideoutBattleInProgress;
				}
			}
		}

		public override void OnHeroCanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
		{
			if (hero == StoryModeHeroes.Radagos && StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted && !StoryModeManager.Current.MainStoryLine.FamilyRescued)
			{
				result = false;
			}
		}

		protected override void OnCompleteWithSuccess()
		{
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Expected O, but got Unknown
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Expected O, but got Unknown
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Expected O, but got Unknown
			StoryModeHeroes.ElderBrother.Clan = Clan.PlayerClan;
			StoryModeHeroes.LittleBrother.Clan = Clan.PlayerClan;
			StoryModeHeroes.ElderBrother.ChangeState((CharacterStates)1);
			EnterSettlementAction.ApplyForCharacterOnly(StoryModeHeroes.ElderBrother, _targetSettlementForSiblings);
			if (StoryModeHeroes.LittleBrother.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
			{
				StoryModeHeroes.LittleBrother.ChangeState((CharacterStates)1);
				EnterSettlementAction.ApplyForCharacterOnly(StoryModeHeroes.LittleBrother, _targetSettlementForSiblings);
				StoryModeHelpers.SetPlayerSiblingsSkillsIfNeeded(StoryModeHeroes.LittleBrother);
			}
			else
			{
				StoryModeHeroes.LittleBrother.ChangeState((CharacterStates)0);
			}
			StoryModeHeroes.ElderBrother.UpdateLastKnownClosestSettlement(_targetSettlementForSiblings);
			StoryModeHeroes.LittleBrother.UpdateLastKnownClosestSettlement(_targetSettlementForSiblings);
			TextObject val = new TextObject("{=PDlaPVIP}{PLAYER_LITTLE_BROTHER.NAME} is the little brother of {PLAYER.LINK}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER_LITTLE_BROTHER", StoryModeHeroes.LittleBrother.CharacterObject, val, false);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
			StoryModeHeroes.LittleBrother.EncyclopediaText = val;
			TextObject val2 = new TextObject("{=LcxfWLgd}{PLAYER_BROTHER.NAME} is the elder brother of {PLAYER.LINK}.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("PLAYER_BROTHER", StoryModeHeroes.ElderBrother.CharacterObject, val2, false);
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val2, false);
			StoryModeHeroes.ElderBrother.EncyclopediaText = val2;
			ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo("NavalDLC");
			if (moduleInfo == null || !moduleInfo.IsActive)
			{
				StoryModeHeroes.LittleSister.Clan = Clan.PlayerClan;
				if (StoryModeHeroes.LittleSister.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
				{
					StoryModeHeroes.LittleSister.ChangeState((CharacterStates)1);
					EnterSettlementAction.ApplyForCharacterOnly(StoryModeHeroes.LittleSister, _targetSettlementForSiblings);
					StoryModeHelpers.SetPlayerSiblingsSkillsIfNeeded(StoryModeHeroes.LittleSister);
				}
				else
				{
					StoryModeHeroes.LittleSister.ChangeState((CharacterStates)0);
				}
				StoryModeHeroes.LittleSister.UpdateLastKnownClosestSettlement(_targetSettlementForSiblings);
				TextObject val3 = new TextObject("{=7XTkTi9B}{PLAYER_LITTLE_SISTER.NAME} is the little sister of {PLAYER.LINK}.", (Dictionary<string, object>)null);
				StringHelpers.SetCharacterProperties("PLAYER_LITTLE_SISTER", StoryModeHeroes.LittleSister.CharacterObject, val3, false);
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val3, false);
				StoryModeHeroes.LittleSister.EncyclopediaText = val3;
			}
		}

		protected override void OnTimedOut()
		{
			base.OnTimedOut();
			KillCharacterAction.ApplyByRemove(StoryModeHeroes.LittleSister, false, true);
			KillCharacterAction.ApplyByRemove(StoryModeHeroes.LittleBrother, false, true);
			KillCharacterAction.ApplyByRemove(StoryModeHeroes.ElderBrother, false, true);
		}

		private void InitializeHideout()
		{
			CheckIfHideoutIsReady();
		}

		private void CheckIfHideoutIsReady()
		{
			if (!_hideout.Hideout.IsInfested)
			{
				for (int i = 0; i < 2; i++)
				{
					if (!_hideout.Hideout.IsInfested)
					{
						_raiderParties.Add(CreateRaiderParty(i, isBanditBossParty: false));
					}
				}
			}
			_hideout.Hideout.IsSpotted = true;
			_hideout.IsVisible = true;
		}

		private void AddRadagosHenchmanToHideout()
		{
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			if (!((IEnumerable<MobileParty>)_hideout.Parties).Any((MobileParty p) => p.IsBanditBossParty))
			{
				_raiderParties.Add(CreateRaiderParty(3, isBanditBossParty: true));
			}
			foreach (MobileParty item in (List<MobileParty>)(object)_hideout.Parties)
			{
				if (!item.IsBanditBossParty)
				{
					continue;
				}
				if (((IEnumerable<TroopRosterElement>)item.MemberRoster.GetTroopRoster()).Any((TroopRosterElement t) => t.Character == _hideout.Culture.BanditBoss))
				{
					TroopRosterElement val = ((IEnumerable<TroopRosterElement>)item.MemberRoster.GetTroopRoster()).First((TroopRosterElement t) => t.Character == _hideout.Culture.BanditBoss);
					item.MemberRoster.RemoveTroop(val.Character, 1, default(UniqueTroopDescriptor), 0);
				}
				_hideoutBoss.ChangeState((CharacterStates)1);
				if (_hideoutBoss.PartyBelongedTo == null)
				{
					item.MemberRoster.AddToCounts(_hideoutBoss.CharacterObject, 1, true, 0, 0, true, -1);
				}
				break;
			}
		}

		private MobileParty CreateRaiderParty(int number, bool isBanditBossParty)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Expected O, but got Unknown
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			Clan val = _hideout.OwnerClan;
			if (((MBObjectBase)val).StringId.Equals("looters"))
			{
				val = Extensions.GetRandomElementInefficiently<Clan>(((IEnumerable<Clan>)Clan.All).Where((Clan c) => c.IsBanditFaction && c.Culture == _hideout.Culture));
			}
			MobileParty obj = BanditPartyComponent.CreateBanditParty("rescue_family_quest_raider_party_" + number, val, _hideout.Hideout, isBanditBossParty, (PartyTemplateObject)null, _hideout.GatePosition);
			CharacterObject val2 = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(((MBObjectBase)_hideout.Culture).StringId + "_bandit");
			obj.MemberRoster.AddToCounts(val2, 5, false, 0, 0, true, -1);
			obj.Party.SetCustomName(new TextObject("{=u1Pkt4HC}Raiders", (Dictionary<string, object>)null));
			obj.ActualClan = val;
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

		private void SelectTargetSettlementForSiblings()
		{
			Town obj = SettlementHelper.FindNearestTownToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement s) => s.OwnerClan.MapFaction == Clan.PlayerClan.MapFaction));
			_targetSettlementForSiblings = ((obj != null) ? ((SettlementComponent)obj).Settlement : null);
			if (_targetSettlementForSiblings == null)
			{
				Town obj2 = SettlementHelper.FindNearestTownToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)((Settlement s) => !Clan.PlayerClan.MapFaction.IsAtWarWith(s.OwnerClan.MapFaction)));
				_targetSettlementForSiblings = ((obj2 != null) ? ((SettlementComponent)obj2).Settlement : null);
			}
			if (_targetSettlementForSiblings == null)
			{
				_targetSettlementForSiblings = SettlementHelper.FindRandomSettlement((Func<Settlement, bool>)((Settlement s) => s.IsTown));
			}
		}

		protected override void RegisterEvents()
		{
			CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
			CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
			CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
			CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
			CampaignEvents.IsSettlementBusyEvent.AddNonSerializedListener((object)this, (ReferenceAction<Settlement, object, int>)IsSettlementBusy);
			CampaignEvents.MapEventStarted.AddNonSerializedListener((object)this, (Action<MapEvent, PartyBase, PartyBase>)OnMapEventStarted);
			CampaignEvents.OnHideoutBattleCompletedEvent.AddNonSerializedListener((object)this, (Action<BattleSideEnum, HideoutEventComponent, HideoutBattleEndState>)OnHideoutBattleCompleted);
			CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionStarted);
		}

		private void IsSettlementBusy(Settlement settlement, object asker, ref int priority)
		{
			if (asker != this && settlement == _hideout)
			{
				priority = Math.Max(priority, 400);
			}
		}

		private void OnMapEventStarted(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
		{
			if (mapEvent.IsHideoutBattle && mapEvent.MapEventSettlement == _hideout && attackerParty == PartyBase.MainParty)
			{
				_rescueFamilyQuestState = RescueFamilyQuestStateEnum.HideoutBattleInProgress;
			}
		}

		private void OnHideoutBattleCompleted(BattleSideEnum winnerSide, HideoutEventComponent hideoutEventComponent, HideoutBattleEndState battleEndState)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Invalid comparison between Unknown and I4
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Invalid comparison between Unknown and I4
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Expected O, but got Unknown
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Expected O, but got Unknown
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Expected O, but got Unknown
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Expected O, but got Unknown
			Settlement mapEventSettlement = ((MapEventComponent)hideoutEventComponent).MapEvent.MapEventSettlement;
			if (mapEventSettlement != _hideout)
			{
				return;
			}
			MobileParty lastAttackerParty = mapEventSettlement.LastAttackerParty;
			if (lastAttackerParty == null || !lastAttackerParty.IsMainParty || _rescueFamilyQuestState != RescueFamilyQuestStateEnum.HideoutBattleInProgress)
			{
				return;
			}
			if ((int)battleEndState > 2 && battleEndState - 3 <= 1)
			{
				CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, true, false, false, false, false), new ConversationCharacterData(StoryModeHeroes.RadagosHenchman.CharacterObject, (PartyBase)null, true, true, false, false, false, false));
				return;
			}
			if (!_hideoutBoss.IsHealthFull())
			{
				_hideoutBoss.Heal(((BasicCharacterObject)_hideoutBoss.CharacterObject).MaxHitPoints(), false);
			}
			((QuestBase)this).AddLog(_defeatedQuestLogText, false);
			DisableHeroAction.Apply(_radagos);
			if (Hero.MainHero.IsPrisoner && _raiderParties.Contains(Hero.MainHero.PartyBelongedToAsPrisoner.MobileParty))
			{
				EndCaptivityAction.ApplyByPeace(Hero.MainHero, (Hero)null);
				InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=FPhWhjq7}Defeated", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=WN6aHR6m}You were defeated by the bandits in the hideout but you managed to escape. You need to wait a while before attacking again.", (Dictionary<string, object>)null)).ToString(), true, false, ((object)new TextObject("{=yQtzabbe}Close", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
			if (((List<MobileParty>)(object)_hideout.Parties).Count == 0)
			{
				InitializeHideout();
			}
			_hideout.Hideout.SetNextPossibleAttackTime(StoryModeData.StorylineQuestHideoutHiddenDuration);
		}

		private void OnMissionStarted(IMission mission)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			if (Settlement.CurrentSettlement != _hideout || PlayerEncounter.Current == null)
			{
				return;
			}
			Mission val = (Mission)mission;
			HideoutAmbushMissionController missionBehavior = val.GetMissionBehavior<HideoutAmbushMissionController>();
			if (missionBehavior != null)
			{
				missionBehavior.SetOverriddenHideoutBossCharacterObject(_hideoutBoss.CharacterObject);
				return;
			}
			HideoutMissionController missionBehavior2 = val.GetMissionBehavior<HideoutMissionController>();
			if (missionBehavior2 != null)
			{
				missionBehavior2.SetOverriddenHideoutBossCharacterObject(_hideoutBoss.CharacterObject);
			}
			else
			{
				Debug.FailedAssert("Hideout boss can not be set!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Quests\\PlayerClanQuests\\RescueFamilyQuestBehavior.cs", "OnMissionStarted", 542);
			}
		}

		private void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
		{
			if (killer == _radagos && victim == _hideoutBoss)
			{
				if (Campaign.Current.CurrentMenuContext != null)
				{
					Campaign.Current.CurrentMenuContext.SwitchToMenu("radagos_goodbye_menu");
				}
				else
				{
					GameMenu.ActivateGameMenu("radagos_goodbye_menu");
				}
			}
		}

		private void OnSettlementLeft(MobileParty party, Settlement settlement)
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			if (party.IsMainParty)
			{
				if (((QuestBase)this).IsTrackEnabled && _rescueFamilyQuestState > RescueFamilyQuestStateEnum.None && !((QuestBase)this).IsTracked((ITrackableCampaignObject)(object)_hideout))
				{
					((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_hideout);
				}
				if (settlement == _hideout && PartyBase.MainParty.MemberRoster.Contains(_radagos.CharacterObject))
				{
					PartyBase.MainParty.MemberRoster.RemoveTroop(_radagos.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
				}
			}
		}

		private void OnGameMenuOpened(MenuCallbackArgs args)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			GameStateManager current = GameStateManager.Current;
			if (((current != null) ? current.ActiveState : null) is MapState)
			{
				if (_rescueFamilyQuestState < RescueFamilyQuestStateEnum.HideoutTalkWithRadagosDone && Settlement.CurrentSettlement != null && Settlement.CurrentSettlement == _hideout)
				{
					CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, true, false, false, false, false), new ConversationCharacterData(StoryModeHeroes.Radagos.CharacterObject, (PartyBase)null, true, true, false, false, false, false));
				}
				else if (_rescueFamilyQuestState == RescueFamilyQuestStateEnum.GoodbyeTalkWithRadagosDone && args.MenuContext.GameMenu.StringId == "radagos_goodbye_menu")
				{
					GameMenu.ExitToLast();
					((QuestBase)this).CompleteQuestWithSuccess();
				}
			}
		}

		private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Invalid comparison between Unknown and I4
			if (_rescueFamilyQuestState < RescueFamilyQuestStateEnum.HideoutTalkWithRadagosDone || settlement != _hideout || mobileParty == null || !mobileParty.IsMainParty)
			{
				return;
			}
			if (!PartyBase.MainParty.MemberRoster.Contains(_radagos.CharacterObject))
			{
				if ((int)_radagos.HeroState != 1)
				{
					_radagos.ChangeState((CharacterStates)1);
				}
				PartyBase.MainParty.MemberRoster.AddToCounts(_radagos.CharacterObject, 1, false, 0, 0, true, -1);
			}
			AddRadagosHenchmanToHideout();
		}

		protected override void HourlyTick()
		{
			CheckIfHideoutIsReady();
		}

		protected override void SetDialogs()
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected O, but got Unknown
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Expected O, but got Unknown
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Expected O, but got Unknown
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Expected O, but got Unknown
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Expected O, but got Unknown
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Expected O, but got Unknown
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Expected O, but got Unknown
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Expected O, but got Unknown
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Expected O, but got Unknown
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Expected O, but got Unknown
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Expected O, but got Unknown
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Expected O, but got Unknown
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Expected O, but got Unknown
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Expected O, but got Unknown
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Expected O, but got Unknown
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Expected O, but got Unknown
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Expected O, but got Unknown
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0240: Expected O, but got Unknown
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Expected O, but got Unknown
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_028f: Expected O, but got Unknown
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a0: Expected O, but got Unknown
			//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Expected O, but got Unknown
			//IL_02df: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e9: Expected O, but got Unknown
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Expected O, but got Unknown
			//IL_0316: Unknown result type (might be due to invalid IL or missing references)
			//IL_0324: Expected O, but got Unknown
			//IL_0331: Unknown result type (might be due to invalid IL or missing references)
			//IL_033e: Expected O, but got Unknown
			//IL_0345: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Expected O, but got Unknown
			//IL_035a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0367: Expected O, but got Unknown
			//IL_036e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0378: Expected O, but got Unknown
			//IL_037f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0389: Expected O, but got Unknown
			//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cb: Expected O, but got Unknown
			//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03dc: Expected O, but got Unknown
			//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ef: Expected O, but got Unknown
			//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0400: Expected O, but got Unknown
			//IL_043f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0449: Expected O, but got Unknown
			//IL_0456: Unknown result type (might be due to invalid IL or missing references)
			//IL_0464: Expected O, but got Unknown
			//IL_0471: Unknown result type (might be due to invalid IL or missing references)
			//IL_047e: Expected O, but got Unknown
			//IL_0484: Unknown result type (might be due to invalid IL or missing references)
			//IL_0492: Expected O, but got Unknown
			//IL_049f: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ac: Expected O, but got Unknown
			//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04bd: Expected O, but got Unknown
			//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f3: Expected O, but got Unknown
			//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0504: Expected O, but got Unknown
			Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 160).NpcLine(new TextObject("{=1yi00v5w}{PLAYER.NAME}! Good to see you. Believe it or not, I mean that. I've been looking for you...[if:convo_calm_friendly][ib:normal2]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(radagos_reunion_conversation_condition))
				.PlayerLine(new TextObject("{=pCNSEPEP}You escaped? Where's my brother? What happened?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=xknCpvcb}Calm down, now. I'll tell you everything.[ib:closed2][if:convo_grave]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(GameTexts.FindText("rescue_family_quest_radagos_conversation_line_1", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=UpUqL368}What scum, eh? Even in this profession, double-crossing your comrades is frowned upon.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=bJjAqCxk}I escaped - one of his men, a little guiltier than the rest, cut my bonds when the others were sleeping - but I can't let a traitor live. So I decided to find you and offer you a deal.[if:convo_focused_voice][ib:hip]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=PlpNTQqf}I know where {HIDEOUT_BOSS.LINK} is now. If you agree, we can attack together and save your kin.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=mmQRCHUM}But in return, I will have the pleasure of killing that bastard. So what do you say?[if:convo_snide_voice][ib:confident2]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.PlayerLine(new TextObject("{=ypDmy5Rn}Uh, how can we possibly trust each other?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=VbJvL8yB}Oh you can't trust me. But you need me, and I figure you have enough men that you could easily slit my throat pretty quickly if I lead you into a trap. And I don't need to trust you - you're my vehicle of revenge, not my partner.[if:convo_grave]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.PlayerLine(new TextObject("{=ft6zzDrJ}I can live with that. Let's go.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=HT9hW29s}Splendid! But I have a few things to do. There is a hideout near this city. {HIDEOUT_BOSS.LINK} keeps your siblings there. I will join you right where the path leads up, just out of sight of their scouts.[if:convo_snide_voice][ib:hip]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.PlayerLine(new TextObject("{=GicEcLx2}See you there then. But, remember, if this is a trap or something, that will cost you your life.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=8b4Ndfep}Oh of course. I have no doubts on that score.[if:convo_nonchalant]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(radagos_reunion_conversation_consequence))
				.CloseDialog(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 160).NpcLine(new TextObject("{=rDuegB1L}You've finally arrived! I have a few things to say before we attack.[ib:confident2][if:convo_nonchalant]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(radagos_hideout_conversation_condition))
				.NpcLine(new TextObject("{=1T7p0O7B}We have to be clever. {HIDEOUT_BOSS.LINK} is a cunning fellow, in a low and base kind of way.[if:convo_normal]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.PlayerLine(new TextObject("{=a29lmPLd}I defeated you before. I know how your gang operates. Less talking, more raiding. C'mon...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=QbsDYITB}That you did, that you did. Lead on, then.[ib:closed2][if:convo_calm_friendly]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(radagos_hideout_conversation_consequence))
				.CloseDialog(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 160).NpcLine(new TextObject("{=PiKISvfu}{PLAYER.NAME}! I knew you'd come. Great Heaven. Damn, {?PLAYER.GENDER}sister{?}brother{\\?}, nothing can stop you! I love you, {?PLAYER.GENDER}sister{?}brother{\\?}.[if:convo_calm_friendly][ib:aggressive2]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(brother_hideout_conversation_condition))
				.PlayerLine(new TextObject("{=DIKPGwj1}So glad to see you safe. Is everyone okay?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(GameTexts.FindText("rescue_family_quest_brother_conversation_line_1", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(GameTexts.FindText("rescue_family_quest_brother_conversation_line_2", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=IC9Vg5MA}Meet me there later, when you're ready to tell me everything.[if:convo_normal][ib:normal2]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.PlayerLine(new TextObject("{=LrItHItu}Okay brother, be careful. Take care.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += brother_hideout_conversation_consequence;
				})
				.CloseDialog(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000015).NpcLine(new TextObject("{=0I9siaQY}Bastards... You're the kin of my captives, right? I saw {RADAGOS.LINK} with you. You know he can't be trusted?[if:convo_confused_annoyed][ib:aggressive]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(bandit_hideout_boss_fight_start_on_condition))
				.PlayerLine(GameTexts.FindText("rescue_family_quest_galter_conversation_player_line_1", (string)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=heoCaRIr}Nah... There's no more talking. Kill me or I kill you, that's how this ends.[ib:warrior][if:convo_bared_teeth]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=2GeiKTlS}I'll do you the honor of duelling you, and my men will stand down if you win.[if:convo_predatory]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(new TextObject("{=ImLQNYWC}Very well - I'll duel you.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence(new OnConsequenceDelegate(bandit_hideout_start_duel_fight_on_consequence))
				.CloseDialog()
				.PlayerOption(new TextObject("{=MMv3hsmI}I don't duel slavers. Men, attack!", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.ClickableCondition(new OnClickableConditionDelegate(bandit_hideout_continue_battle_on_clickable_condition))
				.Consequence(new OnConsequenceDelegate(bandit_hideout_continue_battle_on_consequence))
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog(), (object)this);
			Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000015).NpcLine(new TextObject("{=G9iXmhGK}Look, we can still talk. I'll give you a pouch of silver.[ib:weary][if:convo_confused_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(hideout_boss_prisoner_talk_condition))
				.PlayerLine(new TextObject("{=fM4eSVps}You said talking was a waste of time. You are {RADAGOS.NAME}'s property, now.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					_rescueFamilyQuestState = RescueFamilyQuestStateEnum.ExecutionTalkWithGalterDone;
					hideout_boss_prisoner_talk_consequence();
				})
				.CloseDialog(), (object)this);
			string text = default(string);
			Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1000015).NpcLine(GameTexts.FindText("rescue_family_quest_radagos_goodbye_conversation_line_1", (string)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition(new OnConditionDelegate(goodbye_conversation_with_radagos_condition))
				.GetOutputToken(ref text)
				.NpcLine(new TextObject("{=C79Xxm1b}Don't let your conscience bother you about letting me go, by the way. I won't get back into slaving. Burned too many bridges with my old colleagues, you might say. I'll find some other way to earn my keep - mercenary work, perhaps. Anyway, maybe our paths will cross again.[if:convo_empathic_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(new TextObject("{=c1Q2irLi}Your men killed my parents. Did you really think you would not be punished?", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.NpcLine(new TextObject("{=W7hi7jS4}Eh, well, I dared to hope, I suppose. All right then, I'm not going to grovel to you, so get it over with.[ib:hip][if:convo_uncomfortable_voice]", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.BeginPlayerOptions((string)null, false)
				.PlayerOption(new TextObject("{=kz5PJbV1}I shall. For your many crimes, {RADAGOS.NAME}, your life is forfeit.", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += execute_radagos_consequence;
				})
				.CloseDialog()
				.PlayerOption(GameTexts.FindText("rescue_family_quest_radagos_goodbye_conversation_player_line_1", (string)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.GotoDialogState(text)
				.EndPlayerOptions()
				.PlayerOption(new TextObject("{=RefpTQpr}Maybe. Goodbye, {RADAGOS.NAME}...", (Dictionary<string, object>)null), (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
				.Consequence((OnConsequenceDelegate)delegate
				{
					Campaign.Current.ConversationManager.ConversationEndOneShot += let_go_radagos_consequence;
				})
				.CloseDialog()
				.EndPlayerOptions()
				.CloseDialog(), (object)this);
		}

		private bool radagos_reunion_conversation_condition()
		{
			StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
			StringHelpers.SetCharacterProperties("HIDEOUT_BOSS", _hideoutBoss.CharacterObject, (TextObject)null, false);
			if (_rescueFamilyQuestState == RescueFamilyQuestStateEnum.None)
			{
				return Hero.OneToOneConversationHero == _radagos;
			}
			return false;
		}

		private void radagos_reunion_conversation_consequence()
		{
			_rescueFamilyQuestState = RescueFamilyQuestStateEnum.ReunionTalkWithRadagosDone;
			((QuestBase)this).AddLog(_startQuestLogText, false);
		}

		private bool radagos_hideout_conversation_condition()
		{
			StringHelpers.SetCharacterProperties("HIDEOUT_BOSS", _hideoutBoss.CharacterObject, (TextObject)null, false);
			if (_rescueFamilyQuestState < RescueFamilyQuestStateEnum.HideoutTalkWithRadagosDone && Settlement.CurrentSettlement == _hideout)
			{
				return Hero.OneToOneConversationHero == _radagos;
			}
			return false;
		}

		private void radagos_hideout_conversation_consequence()
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Invalid comparison between Unknown and I4
			_rescueFamilyQuestState = RescueFamilyQuestStateEnum.HideoutTalkWithRadagosDone;
			if (!PartyBase.MainParty.MemberRoster.Contains(_radagos.CharacterObject))
			{
				if ((int)_radagos.HeroState != 1)
				{
					_radagos.ChangeState((CharacterStates)1);
				}
				PartyBase.MainParty.MemberRoster.AddToCounts(_radagos.CharacterObject, 1, false, 0, 0, true, -1);
			}
			AddRadagosHenchmanToHideout();
		}

		private bool brother_hideout_conversation_condition()
		{
			if (_rescueFamilyQuestState < RescueFamilyQuestStateEnum.ReunionTalkWithBrotherDone && Hero.OneToOneConversationHero == StoryModeHeroes.ElderBrother)
			{
				SelectTargetSettlementForSiblings();
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
				StringHelpers.SetCharacterProperties("LITTLE_SISTER", StoryModeHeroes.LittleSister.CharacterObject, (TextObject)null, false);
				StringHelpers.SetCharacterProperties("LITTLE_BROTHER", StoryModeHeroes.LittleBrother.CharacterObject, (TextObject)null, false);
				MBTextManager.SetTextVariable("SETTLEMENT_LINK", _targetSettlementForSiblings.EncyclopediaLinkWithName, false);
				Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
				{
					if (Campaign.Current.CurrentMenuContext != null)
					{
						Campaign.Current.CurrentMenuContext.SwitchToMenu("radagos_goodbye_menu");
					}
					else
					{
						GameMenu.ActivateGameMenu("radagos_goodbye_menu");
					}
				};
				return true;
			}
			return false;
		}

		private void brother_hideout_conversation_consequence()
		{
			_rescueFamilyQuestState = RescueFamilyQuestStateEnum.ReunionTalkWithBrotherDone;
		}

		private bool bandit_hideout_boss_fight_start_on_condition()
		{
			PartyBase encounteredParty = PlayerEncounter.EncounteredParty;
			if (encounteredParty == null || encounteredParty.IsMobile || encounteredParty.MapFaction == null || !encounteredParty.MapFaction.IsBanditFaction)
			{
				return false;
			}
			StringHelpers.SetCharacterProperties("RADAGOS", _radagos.CharacterObject, (TextObject)null, false);
			if (encounteredParty.IsSettlement && encounteredParty.Settlement.IsHideout && encounteredParty.Settlement == _hideout && Mission.Current != null && Hero.OneToOneConversationHero != null && Hero.OneToOneConversationHero == _hideoutBoss)
			{
				if (Mission.Current.GetMissionBehavior<HideoutAmbushMissionController>() == null)
				{
					return Mission.Current.GetMissionBehavior<HideoutMissionController>() != null;
				}
				return true;
			}
			return false;
		}

		private void bandit_hideout_start_duel_fight_on_consequence()
		{
			if (Mission.Current.GetMissionBehavior<HideoutAmbushMissionController>() != null)
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutAmbushMissionController.StartBossFightDuelMode;
			}
			else
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutMissionController.StartBossFightDuelMode;
			}
		}

		private bool bandit_hideout_continue_battle_on_clickable_condition(out TextObject explanation)
		{
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Expected O, but got Unknown
			bool flag = false;
			foreach (Agent item in (List<Agent>)(object)Mission.Current.PlayerTeam.ActiveAgents)
			{
				if (!item.IsMount && (object)item.Character != CharacterObject.PlayerCharacter)
				{
					flag = true;
					break;
				}
			}
			explanation = TextObject.GetEmpty();
			if (!flag)
			{
				explanation = new TextObject("{=F9HxO1iS}You don't have any men.", (Dictionary<string, object>)null);
			}
			return flag;
		}

		private void bandit_hideout_continue_battle_on_consequence()
		{
			if (Mission.Current.GetMissionBehavior<HideoutAmbushMissionController>() != null)
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutAmbushMissionController.StartBossFightBattleMode;
			}
			else
			{
				Campaign.Current.ConversationManager.ConversationEndOneShot += HideoutMissionController.StartBossFightBattleMode;
			}
		}

		private bool hideout_boss_prisoner_talk_condition()
		{
			StringHelpers.SetCharacterProperties("RADAGOS", _radagos.CharacterObject, (TextObject)null, false);
			return Hero.OneToOneConversationHero == _hideoutBoss;
		}

		private void hideout_boss_prisoner_talk_consequence()
		{
			MBInformationManager.ShowSceneNotification((SceneNotificationData)(object)HeroExecutionSceneNotificationData.CreateForInformingPlayer(_radagos, _hideoutBoss, (RelevantContextType)4, (Action)OnGalterExecutionIsDone));
			Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
			{
				if (Campaign.Current.CurrentMenuContext != null)
				{
					Campaign.Current.CurrentMenuContext.SwitchToMenu("radagos_goodbye_menu");
				}
				else
				{
					GameMenu.ActivateGameMenu("radagos_goodbye_menu");
				}
			};
		}

		private void OnGalterExecutionIsDone()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if (_rescueFamilyQuestState == RescueFamilyQuestStateEnum.ExecutionTalkWithGalterDone && !Campaign.Current.ConversationManager.IsConversationInProgress)
			{
				CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, true, false, false, false, false), new ConversationCharacterData(StoryModeHeroes.ElderBrother.CharacterObject, (PartyBase)null, true, true, false, false, false, false));
			}
		}

		private bool goodbye_conversation_with_radagos_condition()
		{
			if (_rescueFamilyQuestState == RescueFamilyQuestStateEnum.ReunionTalkWithBrotherDone && Hero.OneToOneConversationHero == _radagos)
			{
				StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
				StringHelpers.SetCharacterProperties("RADAGOS", _radagos.CharacterObject, (TextObject)null, false);
				return true;
			}
			return false;
		}

		private void execute_radagos_consequence()
		{
			((QuestBase)this).AddLog(_executeRadagosEndQuestLogText, false);
			MBInformationManager.ShowSceneNotification((SceneNotificationData)(object)HeroExecutionSceneNotificationData.CreateForInformingPlayer(Hero.MainHero, _radagos, (RelevantContextType)4, (Action)null));
			_rescueFamilyQuestState = RescueFamilyQuestStateEnum.GoodbyeTalkWithRadagosDone;
		}

		private void let_go_radagos_consequence()
		{
			((QuestBase)this).AddLog(_letGoRadagosEndQuestLogText, false);
			DisableHeroAction.Apply(_radagos);
			_rescueFamilyQuestState = RescueFamilyQuestStateEnum.GoodbyeTalkWithRadagosDone;
		}

		private void AddGameMenus()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_0073: Expected O, but got Unknown
			//IL_0073: Expected O, but got Unknown
			TextObject val = new TextObject("{=kzgbBrYo}As you leave the hideout, {RADAGOS.LINK} comes to you and asks to talk.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("RADAGOS", _radagos.CharacterObject, val, false);
			((QuestBase)this).AddGameMenu("radagos_goodbye_menu", val, new OnInitDelegate(radagos_goodbye_menu_on_init), (MenuOverlayType)0, (MenuFlags)0);
			((QuestBase)this).AddGameMenuOption("radagos_goodbye_menu", "radagos_goodbye_menu_continue", new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null), new OnConditionDelegate(radagos_goodbye_menu_continue_on_condition), new OnConsequenceDelegate(radagos_goodbye_menu_continue_on_consequence), false, -1);
		}

		private void radagos_goodbye_menu_on_init(MenuCallbackArgs args)
		{
		}

		private bool radagos_goodbye_menu_continue_on_condition(MenuCallbackArgs args)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			args.optionLeaveType = (LeaveType)17;
			return true;
		}

		private void radagos_goodbye_menu_continue_on_consequence(MenuCallbackArgs args)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, true, false, false, false, false), new ConversationCharacterData(_radagos.CharacterObject, (PartyBase)null, true, true, false, false, false, false));
		}

		[GameMenuInitializationHandler("radagos_goodbye_menu")]
		private static void quest_game_menus_on_init_background(MenuCallbackArgs args)
		{
			args.MenuContext.SetBackgroundMeshName(((SettlementComponent)SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)null)).WaitMeshName);
		}

		internal static void AutoGeneratedStaticCollectObjectsRescueFamilyQuest(object o, List<object> collectedObjects)
		{
			((MBObjectBase)(RescueFamilyQuest)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected override void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			((QuestBase)this).AutoGeneratedInstanceCollectObjects(collectedObjects);
			collectedObjects.Add(_hideout);
			collectedObjects.Add(_raiderParties);
		}

		internal static object AutoGeneratedGetMemberValue_hideout(object o)
		{
			return ((RescueFamilyQuest)o)._hideout;
		}

		internal static object AutoGeneratedGetMemberValue_raiderParties(object o)
		{
			return ((RescueFamilyQuest)o)._raiderParties;
		}

		internal static object AutoGeneratedGetMemberValue_rescueFamilyQuestState(object o)
		{
			return ((RescueFamilyQuest)o)._rescueFamilyQuestState;
		}
	}

	private bool _rescueFamilyQuestReadyToStart;

	internal RescueFamilyQuestBehavior()
	{
		_rescueFamilyQuestReadyToStart = false;
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnGameLoadedEvent);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
		CampaignEvents.CanHaveCampaignIssuesEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanHaveCampaignIssuesInfoIsRequested);
		CampaignEvents.CanHeroDieEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, KillCharacterActionDetail, bool>)CanHeroDie);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<bool>("_rescueFamilyQuestReadyToStart", ref _rescueFamilyQuestReadyToStart);
	}

	private static void OnGameLoadedEvent(CampaignGameStarter campaignGameStarter)
	{
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		if (!_rescueFamilyQuestReadyToStart || party != MobileParty.MainParty || !settlement.IsTown || settlement.MapFaction.IsAtWarWith(Hero.MainHero.MapFaction) || !(GameStateManager.Current.ActiveState is MapState) || Campaign.Current.ConversationManager.IsConversationFlowActive)
		{
			return;
		}
		bool flag = false;
		foreach (QuestBase item in (List<QuestBase>)(object)Campaign.Current.QuestManager.Quests)
		{
			Hero questGiver = item.QuestGiver;
			if (((questGiver != null) ? questGiver.CurrentSettlement : null) == settlement)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			((QuestBase)new RescueFamilyQuest()).StartQuest();
			_rescueFamilyQuestReadyToStart = false;
			StoryModeHeroes.Radagos.UpdateLastKnownClosestSettlement(Settlement.CurrentSettlement);
			CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, true, true, false, false, false, false), new ConversationCharacterData(StoryModeHeroes.Radagos.CharacterObject, (PartyBase)null, true, true, false, false, false, false));
		}
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails detail)
	{
		if (quest is RebuildPlayerClanQuest)
		{
			_rescueFamilyQuestReadyToStart = true;
		}
		else if (quest is RescueFamilyQuest)
		{
			_rescueFamilyQuestReadyToStart = false;
			StoryModeHeroes.Radagos.CharacterObject.SetTransferableInPartyScreen(true);
			StoryModeHeroes.Radagos.CharacterObject.SetTransferableInHideouts(true);
		}
	}

	private void CanHaveCampaignIssuesInfoIsRequested(Hero hero, ref bool result)
	{
		if (!StoryModeManager.Current.MainStoryLine.FamilyRescued && (hero == StoryModeHeroes.Radagos || hero == StoryModeHeroes.RadagosHenchman))
		{
			result = false;
		}
	}

	private void CanHeroDie(Hero hero, KillCharacterActionDetail causeOfDeath, ref bool result)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		if (hero == StoryModeHeroes.RadagosHenchman && (!StoryModeManager.Current.MainStoryLine.FamilyRescued || _rescueFamilyQuestReadyToStart || (Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(RescueFamilyQuest)) && (int)causeOfDeath != 6 && (int)causeOfDeath != 7)))
		{
			result = false;
		}
	}
}
