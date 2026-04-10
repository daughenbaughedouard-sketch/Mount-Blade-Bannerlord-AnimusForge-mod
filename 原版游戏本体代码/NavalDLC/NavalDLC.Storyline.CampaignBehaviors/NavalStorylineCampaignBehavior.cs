using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Helpers;
using NavalDLC.Storyline.Quests;
using SandBox;
using SandBox.GameComponents;
using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionLogics;
using StoryMode;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.CampaignBehaviors;

public class NavalStorylineCampaignBehavior : CampaignBehaviorBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<Village, bool> _003C_003E9__26_0;

		public static OnConditionDelegate _003C_003E9__45_0;

		public static OnConditionDelegate _003C_003E9__46_0;

		public static OnConditionDelegate _003C_003E9__47_0;

		public static OnConditionDelegate _003C_003E9__48_0;

		public static OnConditionDelegate _003C_003E9__49_0;

		public static Action _003C_003E9__54_0;

		public static Action _003C_003E9__56_0;

		public static OnConditionDelegate _003C_003E9__58_0;

		internal bool _003COnGameLoadFinished_003Eb__26_0(Village x)
		{
			return ((MBObjectBase)((SettlementComponent)x).Settlement).StringId == "village_N1_2";
		}

		internal bool _003CAddGunnarSeaDefaultConversations_003Eb__45_0()
		{
			if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && Settlement.CurrentSettlement == null && MobileParty.MainParty.IsCurrentlyAtSea)
			{
				return Hero.OneToOneConversationHero.PartyBelongedTo == MobileParty.MainParty;
			}
			return false;
		}

		internal bool _003CAddGunnarTownDefaultConversations_003Eb__46_0()
		{
			if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar)
			{
				return Settlement.CurrentSettlement != null;
			}
			return false;
		}

		internal bool _003CAddGunnarStorylineActivationNotPossibleConversation_003Eb__47_0()
		{
			if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar)
			{
				return !NavalStorylineData.IsStorylineActivationPossible();
			}
			return false;
		}

		internal bool _003CAddGunnarRansomConversations_003Eb__48_0()
		{
			MBTextManager.SetTextVariable("GOLD", 10000);
			MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">", false);
			return true;
		}

		internal bool _003CAddLaharDefaultConversations_003Eb__49_0()
		{
			return Hero.OneToOneConversationHero == NavalStorylineData.Lahar;
		}

		internal void _003CEndStorylineByRansom_003Eb__54_0()
		{
			Mission.Current.EndMission();
		}

		internal void _003CRequestRansomSister_003Eb__56_0()
		{
			Mission.Current.EndMission();
		}

		internal bool _003CAddBjolgurDefaultConversations_003Eb__58_0()
		{
			return Hero.OneToOneConversationHero == NavalStorylineData.Bjolgur;
		}
	}

	private const int RansomGoldCost = 10000;

	private bool _isNavalStorylineActive;

	private NavalStorylineData.NavalStorylineSetPieceBattleMissionTypes _activeMissionType = NavalStorylineData.NavalStorylineSetPieceBattleMissionTypes.None;

	private bool _isNavalStorylineCanceled;

	private TroopRoster _troops = TroopRoster.CreateDummyTroopRoster();

	private TroopRoster _prisoners = TroopRoster.CreateDummyTroopRoster();

	private List<Ship> _ships = new List<Ship>();

	private bool _inquiryFired;

	private AnchorPoint _cachedAnchor;

	private NavalStorylineData.NavalStorylineStage _lastCompletedStorylineStage = NavalStorylineData.NavalStorylineStage.None;

	private bool _isFirstReturnToOstican = true;

	private bool _isTutorialSkipped;

	private CampaignTime _sisterReturnTime = CampaignTime.Zero;

	private bool _removeCrimeHandler;

	private int _foodStage = 1;

	private NavalStorylineData.NavalStorylineCheckpoint _lastSavedCheckpoint;

	private void OnNewGameCreated(CampaignGameStarter starter)
	{
		if (NavalStorylineData.Gunnar.IsDisabled || NavalStorylineData.Gunnar.IsNotSpawned)
		{
			NavalStorylineData.Gunnar.ChangeState((CharacterStates)1);
		}
		if (NavalStorylineData.Gunnar.PartyBelongedTo == null && NavalStorylineData.Gunnar.StayingInSettlement == null)
		{
			EnterSettlementAction.ApplyForCharacterOnly(NavalStorylineData.Gunnar, NavalStorylineData.HomeSettlement);
		}
	}

	public override void RegisterEvents()
	{
		if (!_isNavalStorylineCanceled)
		{
			CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)Tick);
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
			CampaignEvents.OnHeirSelectionOverEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnHeirSelectionOver);
			CampaignEvents.CanHeroDieEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, KillCharacterActionDetail, bool>)CanHeroDie);
			CampaignEvents.CanHeroBecomePrisonerEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanHeroBecomePrisoner);
			CampaignEvents.CanHaveCampaignIssuesEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanHaveCampaignIssues);
			CampaignEvents.OnMobilePartyNavigationStateChangedEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnMobilePartyNavigationStateChanged);
			CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
			CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, (Action)HourlyTick);
			CampaignEvents.OnQuestCompletedEvent.AddNonSerializedListener((object)this, (Action<QuestBase, QuestCompleteDetails>)OnQuestCompleted);
			CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnNewGameCreated);
			CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
			NavalDLCEvents.OnNavalStorylineTutorialSkippedEvent.AddNonSerializedListener((object)this, (Action)OnNavalStorylineSkipped);
			NavalDLCEvents.OnNavalStorylineCanceledEvent.AddNonSerializedListener((object)this, (Action<NavalStorylineData.StorylineCancelDetail>)OnNavalStorylineCanceled);
			CampaignEvents.AfterMissionStarted.AddNonSerializedListener((object)this, (Action<IMission>)AfterMissionStarted);
			CampaignEvents.MissionTickEvent.AddNonSerializedListener((object)this, (Action<float>)MissionTickEvent);
			CampaignEvents.OnGameLoadFinishedEvent.AddNonSerializedListener((object)this, (Action)OnGameLoadFinished);
			CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
			CampaignEvents.HeroComesOfAgeEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnHeroComesOfAge);
			CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		if (_isNavalStorylineActive && _activeMissionType != NavalStorylineData.NavalStorylineSetPieceBattleMissionTypes.None)
		{
			_activeMissionType = NavalStorylineData.NavalStorylineSetPieceBattleMissionTypes.None;
		}
	}

	private void HourlyTick()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (MobileParty.MainParty.MapEvent == null && MobileParty.MainParty.SiegeEvent == null && Hero.MainHero.IsActive && !MobileParty.MainParty.IsInRaftState && IsWaitingForSistersReturn() && ((CampaignTime)(ref _sisterReturnTime)).IsPast)
		{
			ShowSisterPopUp();
			_sisterReturnTime = CampaignTime.Zero;
		}
	}

	private void ShowSisterPopUp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0028: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		TextObject val = new TextObject("{=FdXpi6Ql}Word from Gunnar", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=9AjMDDOJ}You receive a message from Gunnar urging you to hurry back to Ostican. He has found and ransomed your sister.", (Dictionary<string, object>)null);
		TextObject val3 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)val3).ToString(), string.Empty, (Action)OnSisterRansomed, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void OnSisterRansomed()
	{
		NavalStorylineData.Gunnar.ChangeState((CharacterStates)1);
		TeleportHeroAction.ApplyImmediateTeleportToSettlement(NavalStorylineData.Gunnar, NavalStorylineData.HomeSettlement);
		_lastCompletedStorylineStage = NavalStorylineData.NavalStorylineStage.Act3SpeakToGunnarAndSister;
		NavalDLCEvents.Instance.OnSisterRansomed();
	}

	private bool IsSister(IAgent agent)
	{
		return (object)agent.Character == StoryModeHeroes.LittleSister.CharacterObject;
	}

	private bool IsGunnar(IAgent agent)
	{
		return (object)agent.Character == NavalStorylineData.Gunnar.CharacterObject;
	}

	private bool IsPlayer(IAgent agent)
	{
		return (object)agent.Character == Hero.MainHero.CharacterObject;
	}

	private void OnHeroComesOfAge(Hero hero)
	{
		if (hero == StoryModeHeroes.LittleSister && NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest5))
		{
			StoryModeHelpers.SetPlayerSiblingsSkillsIfNeeded(hero);
		}
	}

	private void OnGameLoadFinished()
	{
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (StoryModeHeroes.LittleSister.IsAlive && MBSaveLoad.IsUpdatingGameVersion && MBSaveLoad.LastLoadedGameVersion < ApplicationVersion.FromString("v1.3.15.109185", 0))
		{
			AgingCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<AgingCampaignBehavior>();
			FieldInfo field = typeof(AgingCampaignBehavior).GetField("_heroesYoungerThanHeroComesOfAge", BindingFlags.Instance | BindingFlags.NonPublic);
			Dictionary<Hero, int> dictionary = ((campaignBehavior != null) ? ((Dictionary<Hero, int>)field.GetValue(campaignBehavior)) : null);
			bool flag = NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest5);
			if (StoryModeHeroes.LittleSister.Age < (float)Campaign.Current.Models.AgeModel.HeroComesOfAge)
			{
				if (!StoryModeHeroes.LittleSister.IsDisabled && !StoryModeHeroes.LittleSister.IsNotSpawned)
				{
					if (flag)
					{
						StoryModeHeroes.LittleSister.ChangeState((CharacterStates)0);
					}
					else
					{
						DisableHeroAction.Apply(StoryModeHeroes.LittleSister);
					}
				}
				if (!StoryModeHeroes.LittleSister.IsDisabled && dictionary != null && !dictionary.ContainsKey(StoryModeHeroes.LittleSister))
				{
					dictionary.Add(StoryModeHeroes.LittleSister, (int)StoryModeHeroes.LittleSister.Age);
					field.SetValue(campaignBehavior, dictionary);
				}
			}
			else if (flag)
			{
				if (dictionary != null && dictionary.ContainsKey(StoryModeHeroes.LittleSister))
				{
					dictionary.Remove(StoryModeHeroes.LittleSister);
				}
				CheckPlayerSiblingsEducationStages(StoryModeHeroes.LittleSister);
				CheckStoryModeHeroStateAndUpdateIfNeeded(StoryModeHeroes.LittleSister);
				StoryModeHelpers.SetPlayerSiblingsSkillsIfNeeded(StoryModeHeroes.LittleSister);
			}
			else if (!StoryModeHeroes.LittleSister.IsDisabled)
			{
				DisableHeroAction.Apply(StoryModeHeroes.LittleSister);
				if (StoryModeHeroes.LittleSister.GovernorOf != null)
				{
					ChangeGovernorAction.RemoveGovernorOf(StoryModeHeroes.LittleSister);
				}
			}
			CheckAndUpdateGovernorStatusOfStoryModeHero(StoryModeHeroes.LittleSister);
		}
		if (!MBSaveLoad.IsUpdatingGameVersion)
		{
			return;
		}
		ApplicationVersion lastLoadedGameVersion = MBSaveLoad.LastLoadedGameVersion;
		if (!((ApplicationVersion)(ref lastLoadedGameVersion)).IsOlderThan(ApplicationVersion.FromString("v1.3.15", 0)))
		{
			return;
		}
		if (NavalStorylineData.GetStorylineStage() >= NavalStorylineData.NavalStorylineStage.Act3Quest5)
		{
			Settlement currentSettlement = NavalStorylineData.Gunnar.CurrentSettlement;
			Village val = ((IEnumerable<Village>)Village.All).FirstOrDefault((Func<Village, bool>)((Village x) => ((MBObjectBase)((SettlementComponent)x).Settlement).StringId == "village_N1_2"));
			if (val != null && currentSettlement != ((SettlementComponent)val).Settlement)
			{
				TeleportHeroAction.ApplyImmediateTeleportToSettlement(NavalStorylineData.Gunnar, ((SettlementComponent)val).Settlement);
			}
		}
		NavalStorylineData.SetHeroText(NavalStorylineData.Gunnar);
		NavalStorylineData.SetHeroText(NavalStorylineData.Purig);
		NavalStorylineData.SetHeroText(NavalStorylineData.Lahar);
		NavalStorylineData.SetHeroText(NavalStorylineData.EmiraAlFahda);
		NavalStorylineData.SetHeroText(NavalStorylineData.Prusas);
		NavalStorylineData.SetHeroText(NavalStorylineData.Bjolgur);
		if (NavalStorylineData.Purig.IsDead)
		{
			SetOnPurigKilledTexts();
		}
	}

	private void CheckPlayerSiblingsEducationStages(Hero hero)
	{
		EducationCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<EducationCampaignBehavior>();
		if (campaignBehavior != null)
		{
			Type typeFromHandle = typeof(EducationCampaignBehavior);
			if (((Dictionary<Hero, short>)typeFromHandle.GetField("_previousEducations", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(campaignBehavior)).ContainsKey(hero) || !IsHeroAttributesInitialized(hero))
			{
				typeFromHandle.GetMethod("OnHeroComesOfAge", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(campaignBehavior, new object[1] { hero });
			}
		}
	}

	private void CheckStoryModeHeroStateAndUpdateIfNeeded(Hero hero)
	{
		if (hero.IsNotSpawned || hero.IsDisabled)
		{
			Settlement settlementToSpawnForPlayerRelative = GetSettlementToSpawnForPlayerRelative(hero);
			if (hero.BornSettlement == null)
			{
				hero.BornSettlement = settlementToSpawnForPlayerRelative;
			}
			TeleportHeroAction.ApplyImmediateTeleportToSettlement(hero, settlementToSpawnForPlayerRelative);
			if (!hero.IsActive)
			{
				hero.ChangeState((CharacterStates)1);
			}
		}
		if (hero.Clan == null)
		{
			hero.Clan = Clan.PlayerClan;
			if (!hero.IsFugitive)
			{
				MakeHeroFugitiveAction.Apply(hero, false);
			}
		}
	}

	private void CheckAndUpdateGovernorStatusOfStoryModeHero(Hero hero)
	{
		if (hero.GovernorOf != null && hero.CurrentSettlement != ((SettlementComponent)hero.GovernorOf).Settlement)
		{
			Debug.FailedAssert("Last governor check might be unnecessary, check this case", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\CampaignBehaviors\\NavalStorylineCampaignBehavior.cs", "CheckAndUpdateGovernorStatusOfStoryModeHero", 342);
			ChangeGovernorAction.RemoveGovernorOf(StoryModeHeroes.LittleSister);
		}
	}

	private bool IsHeroAttributesInitialized(Hero hero)
	{
		if (hero.CharacterAttributes.GetPropertyValue(DefaultCharacterAttributes.Vigor) == 0 && hero.CharacterAttributes.GetPropertyValue(DefaultCharacterAttributes.Control) == 0 && hero.CharacterAttributes.GetPropertyValue(DefaultCharacterAttributes.Endurance) == 0 && hero.CharacterAttributes.GetPropertyValue(DefaultCharacterAttributes.Cunning) == 0 && hero.CharacterAttributes.GetPropertyValue(DefaultCharacterAttributes.Social) == 0)
		{
			return hero.CharacterAttributes.GetPropertyValue(DefaultCharacterAttributes.Intelligence) != 0;
		}
		return true;
	}

	private Settlement GetSettlementToSpawnForPlayerRelative(Hero hero)
	{
		if (hero.GovernorOf != null)
		{
			return ((SettlementComponent)hero.GovernorOf).Settlement;
		}
		if (!hero.HomeSettlement.OwnerClan.IsAtWarWith(Clan.PlayerClan.MapFaction))
		{
			return hero.HomeSettlement;
		}
		if (!Extensions.IsEmpty<Settlement>((IEnumerable<Settlement>)Clan.PlayerClan.MapFaction.Settlements))
		{
			return Extensions.GetRandomElement<Settlement>(Clan.PlayerClan.MapFaction.Settlements);
		}
		foreach (Settlement item in (List<Settlement>)(object)Settlement.All)
		{
			if (!item.MapFaction.IsAtWarWith(Clan.PlayerClan.MapFaction))
			{
				return item;
			}
		}
		return ((SettlementComponent)Extensions.GetRandomElement<Village>(Village.All)).Settlement;
	}

	private void MissionTickEvent(float dt)
	{
		if (_removeCrimeHandler)
		{
			RemoveCrimeHandler(Mission.Current);
			_removeCrimeHandler = false;
		}
	}

	private void AfterMissionStarted(IMission mission)
	{
		if (_isNavalStorylineActive && LocationComplex.Current != null && Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsTown && Campaign.Current.Models.CrimeModel.IsPlayerCrimeRatingSevere(Settlement.CurrentSettlement.MapFaction))
		{
			_removeCrimeHandler = true;
		}
	}

	private void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
	{
		if (victim == NavalStorylineData.Purig)
		{
			SetOnPurigKilledTexts();
		}
	}

	private void SetOnPurigKilledTexts()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		TextObject val = new TextObject("{=QPn1OTcd}Purig was a Nord warrior who fought in the rebellion against Volbjorn, first ruler of the Nordvyg. Following the king's victory he joined with other defeated rebels to form the Sea Hounds, a pirate confederation that terrorized the northern seas of Calradia, but he was defeated and slain by {PLAYER.NAME}.", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val, false);
		NavalStorylineData.Purig.EncyclopediaText = val;
		TextObject val2 = new TextObject("{=8NxBfsY1}Gunnar of Lagshofn is a Nord warrior from the island of Beinland. He won a reputation for courage fighting in a rebellion against Volbjorn, first king of the Nordvyg, but after the rebels' defeat he made his peace with the victors. He then joined with {PLAYER.NAME} to vanquish the Sea Hounds, a pirate confederation led by Purig that had terrorized the northern seas of Calradia.", (Dictionary<string, object>)null);
		StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, val2, false);
		NavalStorylineData.Gunnar.EncyclopediaText = val2;
	}

	private void OnNavalStorylineCanceled(NavalStorylineData.StorylineCancelDetail detail)
	{
		_isNavalStorylineCanceled = true;
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		if (party == MobileParty.MainParty && NavalStorylineData.Gunnar.StayingInSettlement == settlement && ((MBObjectBase)settlement).StringId.Equals("village_N1_2"))
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)NavalStorylineData.Gunnar.CharacterObject).Race, "_settlement");
			(string, Monster) tuple = (ActionSetCode.GenerateActionSetNameWithSuffix(monsterWithSuffix, ((BasicCharacterObject)NavalStorylineData.Gunnar.CharacterObject).IsFemale, "_lord"), monsterWithSuffix);
			IFaction mapFaction = NavalStorylineData.Gunnar.MapFaction;
			uint num = ((mapFaction != null) ? mapFaction.Color : 4291609515u);
			IFaction mapFaction2 = NavalStorylineData.Gunnar.MapFaction;
			uint num2 = ((mapFaction2 != null) ? mapFaction2.Color : 4291609515u);
			AgentData val = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(tuple.Item2).NoHorses(true).ClothingColor1(num)
				.ClothingColor2(num2);
			Location locationWithId = LocationComplex.Current.GetLocationWithId("village_center");
			IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
			locationWithId.AddCharacter(new LocationCharacter(val, new AddBehaviorsDelegate(agentBehaviorManager.AddFixedCharacterBehaviors), "sp_notable", true, (CharacterRelations)1, tuple.Item1, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false));
		}
	}

	private void OnQuestCompleted(QuestBase quest, QuestCompleteDetails details)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (!(quest is NavalStorylineQuestBase { WillProgressStoryline: not false } navalStorylineQuestBase))
		{
			return;
		}
		if ((int)details == 2)
		{
			ChangeNavalStorylineActivity(activity: false);
			return;
		}
		if (navalStorylineQuestBase.Stage < NavalStorylineData.NavalStorylineStage.Act3Quest5)
		{
			((QuestBase)new ReturnToBaseQuest("naval_storyline_return_to_base", NavalStorylineData.Gunnar)).StartQuest();
		}
		_lastCompletedStorylineStage = navalStorylineQuestBase.Stage;
	}

	private void CanHeroBecomePrisoner(Hero hero, ref bool result)
	{
		if (_isNavalStorylineActive && NavalStorylineData.IsNavalStorylineHero(hero))
		{
			result = false;
		}
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (party == MobileParty.MainParty && _isNavalStorylineActive)
		{
			Campaign.Current.SaveHandler.ForceAutoSave();
			if (!MobileParty.MainParty.IsCurrentlyAtSea)
			{
				ChangeNavalStorylineActivity(activity: false);
			}
		}
	}

	private void OnMobilePartyNavigationStateChanged(MobileParty mobileParty)
	{
		if (_isNavalStorylineActive && mobileParty.IsMainParty && !mobileParty.IsCurrentlyAtSea && PlayerEncounter.EncounterSettlement == null)
		{
			ChangeNavalStorylineActivity(activity: false);
		}
	}

	private void OnHeirSelectionOver(Hero hero)
	{
		if (_isNavalStorylineActive)
		{
			ChangeNavalStorylineActivity(activity: false);
		}
	}

	private void OnSessionLaunched(CampaignGameStarter starter)
	{
		AddGameMenus(starter);
		AddDialogues();
	}

	private void AddDialogues()
	{
		AddGunnarSeaDefaultConversations();
		AddGunnarTownDefaultConversations();
		AddGunnarRansomConversations();
		AddGunnarSisterRansomConversations();
		AddGunnarStorylineActivationNotPossibleConversation();
		AddBjolgurDefaultConversations();
		AddLaharDefaultConversations();
	}

	private void AddGunnarSeaDefaultConversations()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 200).NpcLine("{=0zTShzbi}Keep an eye on the horizon, and look for sails.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__45_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = () => Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && Settlement.CurrentSettlement == null && MobileParty.MainParty.IsCurrentlyAtSea && Hero.OneToOneConversationHero.PartyBelongedTo == MobileParty.MainParty;
			_003C_003Ec._003C_003E9__45_0 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Condition((OnConditionDelegate)obj2).CloseDialog(), (object)null);
	}

	private void AddGunnarTownDefaultConversations()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 200).NpcLine("{=Si6F4bdz}I'm waiting for more news. Soon, I may have more to tell you.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__46_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = () => Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && Settlement.CurrentSettlement != null;
			_003C_003Ec._003C_003E9__46_0 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Condition((OnConditionDelegate)obj2).CloseDialog(), (object)null);
	}

	private void AddGunnarStorylineActivationNotPossibleConversation()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 30000).NpcLine("{=njVdva7h}This isn't the right time to pursue our war against the Sea Hounds, but believe me, I am not about to abandon it.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__47_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = () => Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && !NavalStorylineData.IsStorylineActivationPossible();
			_003C_003Ec._003C_003E9__47_0 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Condition((OnConditionDelegate)obj2).PlayerLine("{=KrsZJv1e}I shall return, hopefully under better circumstances.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).CloseDialog(), (object)null);
	}

	private void AddGunnarRansomConversations()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		string text = default(string);
		DialogFlow obj = DialogFlow.CreateDialogFlow("gunnar_ransom_sister", 1200).NpcLine("{=F94IaWhk}Ah... So be it. I understand why you must put your sister's safety above other considerations. I know people who can pass a message to the Sea Hounds, and I can make inquiries about a ransom, if you like.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).ClickableCondition(new OnClickableConditionDelegate(CanRansomSister))
			.GenerateToken(ref text)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=JIoiP1Is}Do that.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption("{=NvCbw6VY}No. I will not pay money to pirates.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=BpQZOVIp}I am glad that you see things that way.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions()
			.NpcLine("{=R9byg5mp}Very well. But I should warn you... By now, I am sure, the Sea Hounds know your name. You are building a bit of a reputation. I doubt that they’d give up your sister as cheaply as they would some common captive. If you left me {GOLD}{GOLD_ICON} denars, I'm sure that would suffice, but that kind of money may be hard to come by.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, text, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__48_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = delegate
			{
				MBTextManager.SetTextVariable("GOLD", 10000);
				MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">", false);
				return true;
			};
			_003C_003Ec._003C_003E9__48_0 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Condition((OnConditionDelegate)obj2).BeginPlayerOptions((string)null, false).PlayerOption("{=rFaOeL2M}Here - this is {GOLD}{GOLD_ICON} denars. Take it, and make your inquiries.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.ClickableCondition(new OnClickableConditionDelegate(DoesPlayerHaveEnoughGoldToRansomSister))
			.NpcLine("{=nSWqf79K}Right... I will send word when I know more.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.Consequence(new OnConsequenceDelegate(RequestRansomSister))
			.CloseDialog()
			.PlayerOption("{=UgWdVbxn}Somehow, I will raise the money.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=S5cioFLJ}Very well. If you are able to raise the money, and still wish to proceed with the ransom, then let me know. I owe you my life, and I am always ready to help you in whatever course you choose.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.PlayerOption("{=OONSEXb2}I will never pay that kind of money to brigands.", (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.NpcLine("{=BpQZOVIp}I am glad that you see things that way.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null)
			.CloseDialog()
			.EndPlayerOptions(), (object)null);
	}

	private void AddLaharDefaultConversations()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 200).NpcLine("{=TAiPuK1n}When you're at sea, you long to be on shore. When you're on shore, shuffling about and waiting for things to be made ready, you'd give anything to be back at sea, running fast before the wind. That's always how it is.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__49_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = () => Hero.OneToOneConversationHero == NavalStorylineData.Lahar;
			_003C_003Ec._003C_003E9__49_0 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Condition((OnConditionDelegate)obj2).CloseDialog(), (object)null);
	}

	private void AddGunnarSisterRansomConversations()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_006d: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00e7: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_010b: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		string text = default(string);
		string text2 = default(string);
		Campaign.Current.ConversationManager.AddDialogFlow(DialogFlow.CreateDialogFlow("start", 1200).NpcLine("{=Gkmt02Zo}{PLAYER.NAME}... I have someone who is eager to see you again!", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null).Condition((OnConditionDelegate)delegate
		{
			int num;
			if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement)
			{
				num = ((_lastCompletedStorylineStage == NavalStorylineData.NavalStorylineStage.Act3SpeakToGunnarAndSister) ? 1 : 0);
				if (num != 0)
				{
					StringHelpers.SetCharacterProperties("PLAYER", CharacterObject.PlayerCharacter, (TextObject)null, false);
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		})
			.Consequence((OnConsequenceDelegate)delegate
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				SpawnSister(GetSisterTeleportPosition());
			})
			.NpcLine("{=MmGO1qT4}{?PLAYER.GENDER}Sister{?}Brother{\\?}! Is that you? Heaven's mercy! I had all but given up hope, and then they told me that you had arranged for my ransom. Thank you, from the bottom of my heart, thank you!", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.GenerateToken(ref text)
			.GenerateToken(ref text2)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=ZYtba6KE}My sister... Of course I could not leave you in the hands of those cruel men.", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, (string)null)
			.GotoDialogState(text)
			.PlayerOption("{=5Drb4hBh}A small price to pay for your safety. Well, maybe not that small...", new OnMultipleConversationConsequenceDelegate(IsSister), (string)null, (string)null)
			.GotoDialogState(text)
			.EndPlayerOptions()
			.NpcLine("{=QunHWmAo}That terrible night... Father, mother... Those vile slavers, dragging me from port to port. I won’t speak of it now. But Gunnar says that you have risen in the world, that our fortunes have changed. Know that I am ready to do my part, for our family and our future...", new OnMultipleConversationConsequenceDelegate(IsSister), new OnMultipleConversationConsequenceDelegate(IsPlayer), text, (string)null)
			.NpcLine("{=LZSRoGTN}{PLAYER.NAME}... I am glad to have helped you reunite your family, and I hope it repays part of my debt to you. But now I must take my leave. I have unfinished business with the Sea Hounds, and with Purig in particular. I do not think we shall meet again.", new OnMultipleConversationConsequenceDelegate(IsGunnar), new OnMultipleConversationConsequenceDelegate(IsPlayer), (string)null, (string)null)
			.BeginPlayerOptions((string)null, false)
			.PlayerOption("{=I2Ab1kzZ}Good hunting, Gunnar. Give that bastard Purig one from me.", new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null)
			.GotoDialogState(text2)
			.PlayerOption("{=agCqAQuA}It seems a bit of a doomed errand, but good luck anyway.", new OnMultipleConversationConsequenceDelegate(IsGunnar), (string)null, (string)null)
			.GotoDialogState(text2)
			.EndPlayerOptions()
			.NpcLine("{=2g2FhKb5}Farewell.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, text2, (string)null)
			.Consequence(new OnConsequenceDelegate(EndStorylineByRansom))
			.CloseDialog(), (object)null);
	}

	private void SpawnSister(Vec3 spawnPosition)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Agent val = ((IEnumerable<Agent>)Mission.Current.Agents).FirstOrDefault((Func<Agent, bool>)((Agent x) => IsSister((IAgent)(object)x)));
		if (val == null)
		{
			AgentBuildData val2 = new AgentBuildData((BasicCharacterObject)(object)StoryModeHeroes.LittleSister.CharacterObject);
			val2.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val2.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
			val2.InitialPosition(ref spawnPosition);
			Vec3 lookDirection = Agent.Main.LookDirection;
			Vec2 val3 = ((Vec3)(ref lookDirection)).AsVec2;
			val3 = -((Vec2)(ref val3)).Normalized();
			val2.InitialDirection(ref val3);
			val2.NoHorses(true);
			val2.CivilianEquipment(true);
			val = Mission.Current.SpawnAgent(val2, false);
		}
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		MBList<IAgent> obj = new MBList<IAgent>();
		((List<IAgent>)(object)obj).Add((IAgent)(object)val);
		conversationManager.AddConversationAgents((IEnumerable<IAgent>)obj, true);
		RemoveWalkingBehavior(StoryModeHeroes.LittleSister.CharacterObject);
	}

	private void RemoveWalkingBehavior(CharacterObject character)
	{
		Agent? obj = ((IEnumerable<Agent>)Mission.Current.Agents).FirstOrDefault((Func<Agent, bool>)((Agent x) => (object)x.Character == character));
		CampaignAgentComponent component = obj.GetComponent<CampaignAgentComponent>();
		obj.ClearTargetFrame();
		AgentNavigator agentNavigator = component.AgentNavigator;
		if (agentNavigator != null)
		{
			DailyBehaviorGroup behaviorGroup = agentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
			if (behaviorGroup != null)
			{
				((AgentBehaviorGroup)behaviorGroup).RemoveBehavior<WalkingBehavior>();
			}
		}
	}

	private Vec3 GetSisterTeleportPosition()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Vec3 randomPositionAroundPoint = Mission.Current.GetRandomPositionAroundPoint(Agent.Main.Position + Agent.Main.LookRotation.s * 3f, 1f, 1.5f, false);
		int num = 20;
		while (Mission.Current.Scene.GetNavigationMeshForPosition(ref randomPositionAroundPoint) == UIntPtr.Zero && num > 0)
		{
			randomPositionAroundPoint = Mission.Current.GetRandomPositionAroundPoint(Agent.Main.Position + Agent.Main.LookRotation.s * 3f, 1f, 1.5f, false);
			num--;
		}
		return randomPositionAroundPoint;
	}

	private void EndStorylineByRansom()
	{
		NavalDLCEvents.Instance.OnNavalStorylineCanceled(NavalStorylineData.StorylineCancelDetail.ByRansom);
		Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
		{
			Mission.Current.EndMission();
		};
		LocationComplex current = LocationComplex.Current;
		if (current != null)
		{
			current.RemoveCharacterIfExists(NavalStorylineData.Gunnar);
		}
		DisableHeroAction.Apply(NavalStorylineData.Gunnar);
		NavalDLCHelpers.AddSisterToClan();
	}

	private bool DoesPlayerHaveEnoughGoldToRansomSister(out TextObject tooltip)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		bool num = Hero.MainHero.Gold >= 10000;
		if (!num)
		{
			tooltip = new TextObject("{=d0kbtGYn}You don't have enough gold.", (Dictionary<string, object>)null);
		}
		else
		{
			tooltip = TextObject.GetEmpty();
		}
		MBTextManager.SetTextVariable("GOLD", 10000);
		MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">", false);
		return num;
	}

	private void RequestRansomSister()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(ScourgeoftheSeasQuest)))
		{
			((QuestBase)new ScourgeoftheSeasQuest()).StartQuest();
		}
		GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, 10000, false);
		_sisterReturnTime = CampaignTime.WeeksFromNow(3f);
		LocationComplex current = LocationComplex.Current;
		if (current != null)
		{
			current.RemoveCharacterIfExists(NavalStorylineData.Gunnar);
		}
		DisableHeroAction.Apply(NavalStorylineData.Gunnar);
		Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
		{
			Mission.Current.EndMission();
		};
		NavalDLCEvents.Instance.OnSisterRansomRequested();
	}

	private bool CanRansomSister(out TextObject tooltip)
	{
		tooltip = TextObject.GetEmpty();
		if (Hero.OneToOneConversationHero == NavalStorylineData.Gunnar && Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement && !NavalStorylineData.IsNavalStorylineCanceled())
		{
			return !NavalStorylineData.IsNavalStoryLineActive();
		}
		return false;
	}

	private void AddBjolgurDefaultConversations()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		DialogFlow obj = DialogFlow.CreateDialogFlow("start", 200).NpcLine("{=fzhTwmvM}A battle at sea is a fine thing. Cowards have nowhere to run, and the fish do your cleaning-up for you.", (OnMultipleConversationConsequenceDelegate)null, (OnMultipleConversationConsequenceDelegate)null, (string)null, (string)null);
		object obj2 = _003C_003Ec._003C_003E9__58_0;
		if (obj2 == null)
		{
			OnConditionDelegate val = () => Hero.OneToOneConversationHero == NavalStorylineData.Bjolgur;
			_003C_003Ec._003C_003E9__58_0 = val;
			obj2 = (object)val;
		}
		conversationManager.AddDialogFlow(obj.Condition((OnConditionDelegate)obj2).CloseDialog(), (object)null);
	}

	private void home_settlement_encounter_init(MenuCallbackArgs args)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		TextObject val = new TextObject("{=lqy3wHWi}You have returned to Ostican harbor. Gunnar takes his leave to see if any new information about the Sea Hounds has arrived, or any new allies to join you in your fight. He tells you to look for him in the harbor when you are ready to proceed.", (Dictionary<string, object>)null);
		if (_isFirstReturnToOstican)
		{
			val = new TextObject("{=7UmbvMKi}You return to Ostican harbor, and tie your ship up at the pier. Besides the Vlandian traders and fishing vessels lies a small Nordic longship. Gunnar tells you that some of his comrades have responded to his call to hunt the Sea Hounds. He tells you he needs to dictate a letter to some others, and asks you to meet him later in the port.", (Dictionary<string, object>)null);
		}
		MBTextManager.SetTextVariable("MENU_TEXT", val, false);
	}

	private void leave_on_consequence(MenuCallbackArgs args)
	{
		if (_isFirstReturnToOstican)
		{
			_isFirstReturnToOstican = false;
		}
		Settlement val = Settlement.CurrentSettlement ?? PlayerEncounter.EncounterSettlement;
		bool flag = default(bool);
		bool flag2 = default(bool);
		GameMenu.SwitchToMenu((MobileParty.MainParty.HasNavalNavigationCapability && MobileParty.MainParty.Anchor.IsAtSettlement(val)) ? "naval_town_outside" : Campaign.Current.Models.EncounterGameMenuModel.GetEncounterMenu(PartyBase.MainParty, val.Party, ref flag, ref flag2));
	}

	private bool leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		args.Tooltip = new TextObject("{=wmTjX28f}This will exit story mode and return you to the Sandbox. You can continue the storyline later by talking to Gunnar in the port again.", (Dictionary<string, object>)null);
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void AddGameMenus(CampaignGameStarter campaignGameStarter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00a0: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d1: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		//IL_0140: Expected O, but got Unknown
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Expected O, but got Unknown
		//IL_0171: Expected O, but got Unknown
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Expected O, but got Unknown
		//IL_01c1: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Expected O, but got Unknown
		//IL_01f2: Expected O, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Expected O, but got Unknown
		//IL_0223: Expected O, but got Unknown
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Expected O, but got Unknown
		//IL_0254: Expected O, but got Unknown
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Expected O, but got Unknown
		//IL_0285: Expected O, but got Unknown
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Expected O, but got Unknown
		//IL_02ca: Expected O, but got Unknown
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Expected O, but got Unknown
		campaignGameStarter.AddGameMenu("naval_storyline_encounter_blocking", "{=LptlZGpR}The seas are rough, and it is difficult to bring your ship within hailing distance. Gunnar urges you not to waste time here, as you are in some haste.", new OnInitDelegate(virtual_encounter_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_encounter_blocking", "continue", "{=3sRdGQou}Leave", new OnConditionDelegate(leave_on_condition), new OnConsequenceDelegate(virtual_encounter_end_consequence), true, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("naval_storyline_outside_town", "{MENU_TEXT}", new OnInitDelegate(home_settlement_encounter_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_outside_town", "talk_to_gunnar", "{=fJP8DJcB}Talk to Gunnar in port", new OnConditionDelegate(talk_to_gunnar_on_condition), new OnConsequenceDelegate(talk_to_gunnar_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_outside_town", "continue", "{=8nP7PcCQ}Continue the story later", new OnConditionDelegate(leave_on_condition), new OnConsequenceDelegate(leave_on_consequence), true, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("naval_storyline_encounter_meeting", "{=!}.", new OnInitDelegate(game_menu_naval_storyline_encounter_meeting_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenu("naval_storyline_encounter", "{=!}{ENCOUNTER_TEXT}", new OnInitDelegate(game_menu_naval_storyline_encounter_on_init), (MenuOverlayType)4, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_encounter", "attack", "{=zxMOqlhs}Attack", new OnConditionDelegate(game_menu_naval_storyline_encounter_attack_on_condition), new OnConsequenceDelegate(game_menu_naval_storyline_encounter_attack_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_encounter", "leave", "{=2YYRyrOO}Leave...", new OnConditionDelegate(game_menu_naval_storyline_encounter_leave_on_condition), new OnConsequenceDelegate(game_menu_naval_storyline_encounter_leave_on_consequence), true, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("naval_storyline_join_encounter", "{=jKWJpIES}{JOIN_ENCOUNTER_TEXT}. You decide to...", new OnInitDelegate(game_menu_join_naval_storyline_encounter_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_join_encounter", "join_encounter_help_attackers", "{=h3yEHb4U}Help {ATTACKER}.", new OnConditionDelegate(game_menu_join_naval_storyline_encounter_help_attackers_on_condition), new OnConsequenceDelegate(game_menu_join_naval_storyline_encounter_help_attackers_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_join_encounter", "join_encounter_help_defenders", "{=FwIgakj8}Help {DEFENDER}.", new OnConditionDelegate(game_menu_join_naval_storyline_encounter_help_defenders_on_condition), new OnConsequenceDelegate(game_menu_join_naval_storyline_encounter_help_defenders_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_storyline_join_encounter", "join_encounter_leave", "{=!}{LEAVE_TEXT}", new OnConditionDelegate(game_menu_join_naval_storyline_encounter_leave_no_army_on_condition), new OnConsequenceDelegate(game_menu_join_naval_storyline_encounter_leave_on_condition), true, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("town_outside", "contact_gunnar", "{=KStpUvo2}Hail Gunnar's contact for entry", new OnConditionDelegate(talk_to_gunnar_town_outside_on_condition), new OnConsequenceDelegate(contact_gunnar_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("naval_town_outside", "contact_gunnar", "{=KStpUvo2}Hail Gunnar's contact for entry", new OnConditionDelegate(talk_to_gunnar_town_outside_on_condition), new OnConsequenceDelegate(contact_gunnar_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenu("talk_to_gunnar_restricted", "{=sNybnI5O}Gunnar's contact snuck you into the town and lead you to him.", (OnInitDelegate)null, (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameStarter.AddGameMenuOption("talk_to_gunnar_restricted", "talk_to_gunnar_restricted_continue", "{=DM6luo3c}Continue", new OnConditionDelegate(talk_to_gunnar_restricted_continue), new OnConsequenceDelegate(talk_to_gunnar_on_consequence), false, -1, false, (object)null);
		campaignGameStarter.AddGameMenuOption("talk_to_gunnar_restricted", "leave", "{=3sRdGQou}Leave", (OnConditionDelegate)null, new OnConsequenceDelegate(contact_gunnar_leave_on_consequence), false, -1, false, (object)null);
	}

	private bool talk_to_gunnar_town_outside_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		NavalStorylineData.NavalStorylineStage storylineStage = NavalStorylineData.GetStorylineStage();
		if (storylineStage >= NavalStorylineData.NavalStorylineStage.Act1 && storylineStage != NavalStorylineData.NavalStorylineStage.Act3Quest5 && !NavalStorylineData.IsMainPartyAllowed())
		{
			return !Settlement.CurrentSettlement.IsUnderSiege;
		}
		return false;
	}

	private bool talk_to_gunnar_restricted_continue(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)22;
		return true;
	}

	private void contact_gunnar_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("talk_to_gunnar_restricted");
	}

	private void contact_gunnar_leave_on_consequence(MenuCallbackArgs args)
	{
		Settlement val = Settlement.CurrentSettlement ?? PlayerEncounter.EncounterSettlement;
		bool flag = default(bool);
		bool flag2 = default(bool);
		GameMenu.SwitchToMenu((MobileParty.MainParty.HasNavalNavigationCapability && MobileParty.MainParty.Anchor.IsAtSettlement(val)) ? "naval_town_outside" : Campaign.Current.Models.EncounterGameMenuModel.GetEncounterMenu(PartyBase.MainParty, val.Party, ref flag, ref flag2));
	}

	private void game_menu_naval_storyline_encounter_meeting_on_init(MenuCallbackArgs args)
	{
		if (PlayerEncounter.Current != null && ((PlayerEncounter.Battle != null && PlayerEncounter.Battle.AttackerSide.LeaderParty != PartyBase.MainParty && PlayerEncounter.Battle.DefenderSide.LeaderParty != PartyBase.MainParty) || PlayerEncounter.MeetingDone))
		{
			if (PlayerEncounter.LeaveEncounter)
			{
				PlayerEncounter.Finish(true);
				return;
			}
			if (PlayerEncounter.Battle == null)
			{
				PlayerEncounter.StartBattle();
			}
			if (PlayerEncounter.BattleChallenge)
			{
				GameMenu.SwitchToMenu("duel_starter_menu");
				return;
			}
			MBTextManager.SetTextVariable("ENCOUNTER_TEXT", GameTexts.FindText("str_you_have_encountered_PARTY", (string)null), false);
			GameMenu.SwitchToMenu("naval_storyline_encounter");
		}
		else
		{
			PlayerEncounter.DoMeeting();
		}
	}

	private void game_menu_naval_storyline_encounter_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetPanelSound("event:/ui/panels/battle/slide_in");
		if (PlayerEncounter.Battle == null)
		{
			if (MobileParty.MainParty.MapEvent != null)
			{
				PlayerEncounter.Init();
			}
			else
			{
				PlayerEncounter.StartBattle();
			}
		}
		PlayerEncounter.Update();
		if (PlayerEncounter.Current == null)
		{
			Campaign.Current.SaveHandler.SignalAutoSave();
		}
	}

	private bool game_menu_naval_storyline_encounter_attack_on_condition(MenuCallbackArgs args)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		MenuCallbackArgs val = new MenuCallbackArgs(args.MapState, TextObject.GetEmpty());
		CampaignBattleResult campaignBattleResult = PlayerEncounter.CampaignBattleResult;
		if (campaignBattleResult != null && !campaignBattleResult.PlayerVictory && Hero.MainHero.IsWounded && !PlayerEncounter.PlayerSurrender)
		{
			PlayerEncounter.PlayerSurrender = true;
			PlayerEncounter.Update();
			return false;
		}
		if (MenuHelper.EncounterOrderAttackCondition(val) && Hero.MainHero.HitPoints < Hero.MainHero.WoundedHealthLimit + 1)
		{
			Hero.MainHero.HitPoints = Hero.MainHero.WoundedHealthLimit + 1;
		}
		MenuHelper.CheckEnemyAttackableHonorably(args);
		return MenuHelper.EncounterAttackCondition(args);
	}

	private void game_menu_naval_storyline_encounter_attack_on_consequence(MenuCallbackArgs args)
	{
		MenuHelper.EncounterAttackConsequence(args);
	}

	private bool game_menu_naval_storyline_encounter_leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void game_menu_naval_storyline_encounter_leave_on_consequence(MenuCallbackArgs args)
	{
		MenuHelper.EncounterLeaveConsequence();
	}

	private void game_menu_join_naval_storyline_encounter_on_init(MenuCallbackArgs args)
	{
		MapEvent encounteredBattle = PlayerEncounter.EncounteredBattle;
		PartyBase leaderParty = encounteredBattle.GetLeaderParty((BattleSideEnum)1);
		PartyBase leaderParty2 = encounteredBattle.GetLeaderParty((BattleSideEnum)0);
		if (leaderParty.IsMobile && leaderParty.MobileParty.Army != null)
		{
			MBTextManager.SetTextVariable("ATTACKER", leaderParty.MobileParty.ArmyName, false);
		}
		else
		{
			MBTextManager.SetTextVariable("ATTACKER", leaderParty.Name, false);
		}
		if (leaderParty2.IsMobile && leaderParty2.MobileParty.Army != null)
		{
			MBTextManager.SetTextVariable("DEFENDER", leaderParty2.MobileParty.ArmyName, false);
		}
		else
		{
			MBTextManager.SetTextVariable("DEFENDER", leaderParty2.Name, false);
		}
		MBTextManager.SetTextVariable("JOIN_ENCOUNTER_TEXT", GameTexts.FindText("str_come_across_battle", (string)null), false);
	}

	private void game_menu_join_naval_storyline_encounter_leave_on_condition(MenuCallbackArgs args)
	{
		PlayerEncounter.Finish(true);
	}

	private bool game_menu_join_naval_storyline_encounter_help_attackers_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)23;
		return PlayerEncounter.EncounteredBattle.CanPartyJoinBattle(PartyBase.MainParty, (BattleSideEnum)1);
	}

	private void game_menu_join_naval_storyline_encounter_help_attackers_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.JoinBattle((BattleSideEnum)1);
		GameMenu.SwitchToMenu("naval_storyline_encounter");
	}

	private bool game_menu_join_naval_storyline_encounter_help_defenders_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)23;
		return PlayerEncounter.EncounteredBattle.CanPartyJoinBattle(PartyBase.MainParty, (BattleSideEnum)0);
	}

	private void game_menu_join_naval_storyline_encounter_help_defenders_on_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.JoinBattle((BattleSideEnum)0);
		GameMenu.ActivateGameMenu("naval_storyline_encounter");
	}

	private bool game_menu_join_naval_storyline_encounter_leave_no_army_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		MBTextManager.SetTextVariable("LEAVE_TEXT", "{=ebUwP3Q3}Don't get involved.", false);
		return true;
	}

	[GameMenuInitializationHandler("naval_storyline_encounter")]
	private static void game_menu_naval_storyline_encounter_on_init_background(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
	}

	[GameMenuInitializationHandler("naval_storyline_encounter_meeting")]
	private static void game_menu_naval_storyline_encounter_meeting_on_init_background(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("encounter_naval");
	}

	[GameMenuInitializationHandler("naval_storyline_join_encounter")]
	private static void game_menu_naval_storyline_join_encounter_on_init_background(MenuCallbackArgs args)
	{
		string encounterCultureBackgroundMesh = MenuHelper.GetEncounterCultureBackgroundMesh(PlayerEncounter.EncounteredParty.MapFaction.Culture);
		args.MenuContext.SetBackgroundMeshName(encounterCultureBackgroundMesh);
	}

	private void talk_to_gunnar_on_consequence(MenuCallbackArgs args)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		leave_on_consequence(args);
		Mission val = null;
		if (LocationComplex.Current != null && PlayerEncounter.LocationEncounter != null)
		{
			val = (Mission)PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(LocationComplex.Current.GetLocationWithId("port"), (Location)null, NavalStorylineData.Gunnar.CharacterObject, (string)null);
		}
		else
		{
			Location locationWithId = NavalStorylineData.HomeSettlement.LocationComplex.GetLocationWithId("port");
			val = (Mission)CampaignMission.OpenConversationMission(new ConversationCharacterData(CharacterObject.PlayerCharacter, PartyBase.MainParty, true, true, false, false, false, true), new ConversationCharacterData(NavalStorylineData.Gunnar.CharacterObject, PartyBase.MainParty, true, true, false, false, false, true), locationWithId.GetSceneName(NavalStorylineData.HomeSettlement.Town.GetWallLevel()), "", false);
		}
		RemoveCrimeHandler(val);
	}

	[GameMenuInitializationHandler("naval_storyline_encounter_blocking")]
	private static void naval_storyline_encounter_meeting_blocking_on_init_background(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName(((SettlementComponent)SettlementHelper.FindNearestHideoutToMobileParty(MobileParty.MainParty, (NavigationType)3, (Func<Settlement, bool>)null)).WaitMeshName);
	}

	private void RemoveCrimeHandler(Mission mission)
	{
		MissionCrimeHandler missionBehavior = mission.GetMissionBehavior<MissionCrimeHandler>();
		if (missionBehavior != null)
		{
			mission.RemoveMissionBehavior((MissionBehavior)(object)missionBehavior);
		}
	}

	[GameMenuInitializationHandler("naval_storyline_outside_town")]
	private static void naval_storyline_outside_town_on_init_background(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName(NavalStorylineData.HomeSettlement.SettlementComponent.WaitMeshName);
	}

	private bool talk_to_gunnar_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)1;
		return true;
	}

	private void virtual_encounter_end_consequence(MenuCallbackArgs args)
	{
		PlayerEncounter.Finish(true);
	}

	private void virtual_encounter_init(MenuCallbackArgs args)
	{
	}

	private void CanHaveCampaignIssues(Hero hero, ref bool result)
	{
		if (NavalStorylineData.IsNavalStorylineHero(hero))
		{
			result = false;
		}
	}

	private void OnNavalStorylineSkipped()
	{
		_lastCompletedStorylineStage = NavalStorylineData.NavalStorylineStage.Act2;
		_isTutorialSkipped = true;
	}

	private void Tick(float dt)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		if (!_inquiryFired && !MobileParty.MainParty.IsInRaftState && _isNavalStorylineActive && MobileParty.MainParty.IsCurrentlyAtSea && MobileParty.MainParty.IsTransitionInProgress)
		{
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=461jcc87}Leaving Story Mode", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=dV92VE8i}When you leave story mode, you will be returned to Ostican. You can speak to Gunnar in port to try again later. Do you wish to continue?", (Dictionary<string, object>)null)).ToString(), true, true, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), ((object)GameTexts.FindText("str_cancel", (string)null)).ToString(), (Action)OnAcceptDeactivatingNavalStoryline, (Action)OnRejectDeactivatingNavalStoryline, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
		}
	}

	private void OnAcceptDeactivatingNavalStoryline()
	{
		_inquiryFired = true;
	}

	private void OnRejectDeactivatingNavalStoryline()
	{
		MobileParty.MainParty.SetMoveModeHold();
		MobileParty.MainParty.CancelNavigationTransition();
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<bool>("_isActive", ref _isNavalStorylineActive);
		dataStore.SyncData<TroopRoster>("_troops", ref _troops);
		dataStore.SyncData<List<Ship>>("_ships", ref _ships);
		dataStore.SyncData<TroopRoster>("_prisoners", ref _prisoners);
		dataStore.SyncData<bool>("_inquiryFired", ref _inquiryFired);
		dataStore.SyncData<AnchorPoint>("_cachedAnchor", ref _cachedAnchor);
		dataStore.SyncData<NavalStorylineData.NavalStorylineStage>("_storylineStage", ref _lastCompletedStorylineStage);
		dataStore.SyncData<bool>("_isNavalStorylineCanceled", ref _isNavalStorylineCanceled);
		dataStore.SyncData<bool>("_isFirstReturnToOstican", ref _isFirstReturnToOstican);
		dataStore.SyncData<bool>("_isTutorialSkipped", ref _isTutorialSkipped);
		dataStore.SyncData<int>("_foodStage", ref _foodStage);
		dataStore.SyncData<CampaignTime>("_sisterReturnTime", ref _sisterReturnTime);
		dataStore.SyncData<NavalStorylineData.NavalStorylineCheckpoint>("_lastSavedCheckpoint", ref _lastSavedCheckpoint);
	}

	public bool IsNavalStorylineActive()
	{
		return _isNavalStorylineActive;
	}

	private void CanHeroDie(Hero hero, KillCharacterActionDetail causeOfDeath, ref bool result)
	{
		if (!_isNavalStorylineCanceled && NavalStorylineData.IsNavalStorylineHero(hero) && !NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest5))
		{
			result = false;
		}
	}

	public NavalStorylineData.NavalStorylineSetPieceBattleMissionTypes GetNavalStorylineSetPieceBattleMissionType()
	{
		return _activeMissionType;
	}

	public void SetNavalStorylineSetPieceBattleMissionType(NavalStorylineData.NavalStorylineSetPieceBattleMissionTypes missionType)
	{
		_activeMissionType = missionType;
	}

	public NavalStorylineData.NavalStorylineStage GetNavalStorylineStage()
	{
		return _lastCompletedStorylineStage;
	}

	public bool GetIsNavalStorylineCanceled()
	{
		return _isNavalStorylineCanceled;
	}

	public bool IsTutorialSkipped()
	{
		return _isTutorialSkipped;
	}

	public void OnCheckpointReached(NavalStorylineData.NavalStorylineCheckpoint checkpoint)
	{
		if (checkpoint != _lastSavedCheckpoint)
		{
			_lastSavedCheckpoint = checkpoint;
			Campaign.Current.SaveHandler.ForceAutoSave();
		}
	}

	public void ChangeNavalStorylineActivity(bool activity)
	{
		if (_isNavalStorylineActive != activity)
		{
			_isNavalStorylineActive = activity;
			OnActivityChanged(_isNavalStorylineActive);
		}
	}

	private void OnActivityChanged(bool newState)
	{
		_inquiryFired = false;
		if (newState)
		{
			CacheTroopsAndShips();
		}
		else
		{
			ClearRosters();
			GetTroopsAndShipsFromCache();
			NavalStorylineData.TeleportMainHeroAndGunnarBackToBase();
		}
		MobileParty.MainParty.MemberRoster.UpdateVersion();
		NavalDLCEvents.Instance.OnNavalStorylineActivityChanged(newState);
	}

	public bool IsWaitingForSistersReturn()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _sisterReturnTime != CampaignTime.Zero;
	}

	public void GiveProvisionsToPlayer()
	{
		int num = (int)(_lastCompletedStorylineStage + 1);
		if (_foodStage < num)
		{
			GiveProvisionsToPlayerInternal();
			_foodStage = (int)(_lastCompletedStorylineStage + 1);
		}
	}

	private void GiveProvisionsToPlayerInternal()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		float num = ((_lastCompletedStorylineStage == NavalStorylineData.NavalStorylineStage.Act3Quest2) ? 7f : 5.5f);
		float num2 = num * MathF.Abs(MobileParty.MainParty.FoodChange);
		if (num2 > 0f)
		{
			ItemRosterElement val = default(ItemRosterElement);
			((ItemRosterElement)(ref val))._002Ector(DefaultItems.Grain, (int)(num2 / 2f), (ItemModifier)null);
			MobileParty.MainParty.ItemRoster.Add(val);
			ItemObject val2 = MBObjectManager.Instance.GetObject<ItemObject>("fish");
			if (val2 != null)
			{
				ItemRosterElement val3 = default(ItemRosterElement);
				((ItemRosterElement)(ref val3))._002Ector(val2, (int)(num2 / 2f), (ItemModifier)null);
				MobileParty.MainParty.ItemRoster.Add(val3);
			}
		}
		int num3 = (int)((float)MobileParty.MainParty.TotalWage * num);
		num3 = (int)(Math.Round((float)num3 / 100f) * 100.0);
		GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, num3, false);
		InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=wJefidrb}Gunnar has secured some provisions for the journey.", (Dictionary<string, object>)null)).ToString(), new Color(0f, 1f, 0f, 1f)));
	}

	private void GetTroopsAndShipsFromCache()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		MBList<TroopRosterElement> troopRoster = _troops.GetTroopRoster();
		for (int num = ((List<TroopRosterElement>)(object)troopRoster).Count - 1; num >= 0; num--)
		{
			TroopRosterElement val = ((List<TroopRosterElement>)(object)troopRoster)[num];
			if (((BasicCharacterObject)val.Character).IsHero)
			{
				val.Character.HeroObject.ChangeState((CharacterStates)1);
			}
			MobileParty.MainParty.MemberRoster.AddToCounts(val.Character, ((TroopRosterElement)(ref val)).Number, false, ((TroopRosterElement)(ref val)).WoundedNumber, ((TroopRosterElement)(ref val)).Xp, true, -1);
		}
		MBList<TroopRosterElement> troopRoster2 = _prisoners.GetTroopRoster();
		for (int num2 = ((List<TroopRosterElement>)(object)troopRoster2).Count - 1; num2 >= 0; num2--)
		{
			TroopRosterElement val2 = ((List<TroopRosterElement>)(object)troopRoster2)[num2];
			if (((BasicCharacterObject)val2.Character).IsHero)
			{
				val2.Character.HeroObject.ChangeState((CharacterStates)3);
			}
			MobileParty.MainParty.PrisonRoster.AddToCounts(val2.Character, ((TroopRosterElement)(ref val2)).Number, false, ((TroopRosterElement)(ref val2)).WoundedNumber, 0, true, -1);
		}
		_troops.Clear();
		_prisoners.Clear();
		for (int num3 = _ships.Count - 1; num3 >= 0; num3--)
		{
			Ship val3 = _ships[num3];
			ChangeShipOwnerAction.ApplyByTransferring(PartyBase.MainParty, val3);
		}
		if (_cachedAnchor != null)
		{
			MobileParty.MainParty.SetAnchor(_cachedAnchor);
			_cachedAnchor = null;
		}
		else
		{
			MobileParty.MainParty.Anchor.ResetPosition();
		}
		_ships.Clear();
	}

	private void ClearRosters()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		MBList<TroopRosterElement> troopRoster = MobileParty.MainParty.MemberRoster.GetTroopRoster();
		for (int num = ((List<TroopRosterElement>)(object)troopRoster).Count - 1; num >= 0; num--)
		{
			TroopRosterElement val = ((List<TroopRosterElement>)(object)troopRoster)[num];
			if (val.Character != CharacterObject.PlayerCharacter)
			{
				MobileParty.MainParty.MemberRoster.AddToCounts(val.Character, -((TroopRosterElement)(ref val)).Number, false, -((TroopRosterElement)(ref val)).WoundedNumber, 0, true, -1);
			}
			if (((BasicCharacterObject)val.Character).IsHero)
			{
				foreach (IMissionPlayerFollowerHandler behavior in Campaign.Current.CampaignBehaviorManager.GetBehaviors<IMissionPlayerFollowerHandler>())
				{
					behavior.RemoveFollowingHero(val.Character.HeroObject);
				}
			}
		}
		MBList<TroopRosterElement> troopRoster2 = MobileParty.MainParty.PrisonRoster.GetTroopRoster();
		for (int num2 = ((List<TroopRosterElement>)(object)troopRoster2).Count - 1; num2 >= 0; num2--)
		{
			TroopRosterElement val2 = ((List<TroopRosterElement>)(object)troopRoster2)[num2];
			MobileParty.MainParty.PrisonRoster.AddToCounts(val2.Character, -((TroopRosterElement)(ref val2)).Number, false, -((TroopRosterElement)(ref val2)).WoundedNumber, 0, true, -1);
		}
		for (int num3 = ((List<Ship>)(object)PartyBase.MainParty.Ships).Count - 1; num3 >= 0; num3--)
		{
			DestroyShipAction.Apply(((List<Ship>)(object)PartyBase.MainParty.Ships)[num3]);
		}
	}

	private void CacheTroopsAndShips()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		MBList<TroopRosterElement> troopRoster = MobileParty.MainParty.MemberRoster.GetTroopRoster();
		for (int num = ((List<TroopRosterElement>)(object)troopRoster).Count - 1; num >= 0; num--)
		{
			TroopRosterElement val = ((List<TroopRosterElement>)(object)troopRoster)[num];
			if (val.Character != CharacterObject.PlayerCharacter)
			{
				_troops.Add(val);
				if (((BasicCharacterObject)val.Character).IsHero)
				{
					val.Character.HeroObject.ChangeState((CharacterStates)6);
				}
				MobileParty.MainParty.MemberRoster.AddToCountsAtIndex(num, -((TroopRosterElement)(ref val)).Number, -((TroopRosterElement)(ref val)).WoundedNumber, 0, true);
			}
		}
		MBList<TroopRosterElement> troopRoster2 = MobileParty.MainParty.PrisonRoster.GetTroopRoster();
		for (int num2 = ((List<TroopRosterElement>)(object)troopRoster2).Count - 1; num2 >= 0; num2--)
		{
			TroopRosterElement val2 = ((List<TroopRosterElement>)(object)troopRoster2)[num2];
			_prisoners.Add(val2);
			if (((BasicCharacterObject)val2.Character).IsHero)
			{
				val2.Character.HeroObject.ChangeState((CharacterStates)6);
			}
			MobileParty.MainParty.PrisonRoster.AddToCountsAtIndex(num2, -((TroopRosterElement)(ref val2)).Number, -((TroopRosterElement)(ref val2)).WoundedNumber, 0, true);
		}
		_cachedAnchor = ((!MobileParty.MainParty.Anchor.IsValid) ? ((AnchorPoint)null) : new AnchorPoint(MobileParty.MainParty.Anchor));
		for (int num3 = ((List<Ship>)(object)MobileParty.MainParty.Ships).Count - 1; num3 >= 0; num3--)
		{
			Ship val3 = ((List<Ship>)(object)MobileParty.MainParty.Ships)[num3];
			val3.Owner = null;
			_ships.Add(val3);
		}
		MobileParty.MainParty.Anchor.ResetPosition();
	}
}
