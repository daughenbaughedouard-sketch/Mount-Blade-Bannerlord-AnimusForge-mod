using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Conversation;
using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace SandBox.CampaignBehaviors;

public class AlleyCampaignBehavior : CampaignBehaviorBase, IAlleyCampaignBehavior, ICampaignBehavior
{
	public class AlleyCampaignBehaviorTypeDefiner : SaveableTypeDefiner
	{
		public AlleyCampaignBehaviorTypeDefiner()
			: base(515253)
		{
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(PlayerAlleyData), 1, (IObjectResolver)null);
		}

		protected override void DefineContainerDefinitions()
		{
			((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(List<PlayerAlleyData>));
		}
	}

	internal class PlayerAlleyData
	{
		[SaveableField(1)]
		internal readonly Alley Alley;

		[SaveableField(2)]
		internal Hero AssignedClanMember;

		[SaveableField(3)]
		internal Alley UnderAttackBy;

		[SaveableField(4)]
		internal TroopRoster TroopRoster;

		[SaveableField(5)]
		internal CampaignTime LastRecruitTime;

		[SaveableField(6)]
		internal CampaignTime AttackResponseDueDate;

		internal float RandomFloatWeekly
		{
			get
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				if (!(((CampaignTime)(ref LastRecruitTime)).ElapsedDaysUntilNow > (float)CampaignTime.DaysInWeek))
				{
					return 2f;
				}
				CampaignTime now = CampaignTime.Now;
				return MBRandom.RandomFloatWithSeed((uint)((CampaignTime)(ref now)).ToWeeks, (uint)((SettlementArea)Alley).Tag.GetHashCode());
			}
		}

		internal bool IsUnderAttack => UnderAttackBy != null;

		internal bool IsAssignedClanMemberDead => AssignedClanMember.IsDead;

		internal PlayerAlleyData(Alley alley, TroopRoster roster)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			Alley = alley;
			TroopRoster = roster;
			AssignedClanMember = ((IEnumerable<TroopRosterElement>)roster.GetTroopRoster()).First((TroopRosterElement c) => ((BasicCharacterObject)c.Character).IsHero).Character.HeroObject;
			UnderAttackBy = null;
		}

		internal void AlleyFightWon()
		{
			((SettlementArea)UnderAttackBy).Owner.AddPower(0f - ((SettlementArea)UnderAttackBy).Owner.Power * 0.2f);
			UnderAttackBy.SetOwner((Hero)null);
			UnderAttackBy = null;
			if (!TroopRoster.Contains(AssignedClanMember.CharacterObject))
			{
				TroopRoster.AddToCounts(AssignedClanMember.CharacterObject, 1, true, 0, 0, true, -1);
			}
			Hero.MainHero.AddSkillXp(DefaultSkills.Roguery, Campaign.Current.Models.AlleyModel.GetXpGainAfterSuccessfulAlleyDefenseForMainHero());
			GameMenu.SwitchToMenu("alley_fight_won");
		}

		internal void AlleyFightLost()
		{
			DestroyAlley();
			Hero.MainHero.HitPoints = 1;
			GameMenu.SwitchToMenu("alley_fight_lost");
		}

		internal void AbandonTheAlley(bool fromClanScreen = false)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (!fromClanScreen)
			{
				foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)TroopRoster.GetTroopRoster())
				{
					TroopRosterElement current = item;
					if (!((BasicCharacterObject)current.Character).IsHero)
					{
						MobileParty.MainParty.MemberRoster.AddToCounts(current.Character, ((TroopRosterElement)(ref current)).Number, false, 0, 0, true, -1);
					}
				}
			}
			DestroyAlley(fromAbandoning: true);
		}

		internal void DestroyAlley(bool fromAbandoning = false)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!fromAbandoning && AssignedClanMember.IsAlive && (int)AssignedClanMember.DeathMark == 0)
			{
				MakeHeroFugitiveAction.Apply(AssignedClanMember, false);
			}
			if (UnderAttackBy != null)
			{
				Alley.SetOwner(((SettlementArea)UnderAttackBy).Owner);
			}
			else
			{
				Alley.SetOwner((Hero)null);
			}
			TroopRoster.Clear();
			UnderAttackBy = null;
		}

		internal static void AutoGeneratedStaticCollectObjectsPlayerAlleyData(object o, List<object> collectedObjects)
		{
			((PlayerAlleyData)o).AutoGeneratedInstanceCollectObjects(collectedObjects);
		}

		protected virtual void AutoGeneratedInstanceCollectObjects(List<object> collectedObjects)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			collectedObjects.Add(Alley);
			collectedObjects.Add(AssignedClanMember);
			collectedObjects.Add(UnderAttackBy);
			collectedObjects.Add(TroopRoster);
			CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime((object)LastRecruitTime, collectedObjects);
			CampaignTime.AutoGeneratedStaticCollectObjectsCampaignTime((object)AttackResponseDueDate, collectedObjects);
		}

		internal static object AutoGeneratedGetMemberValueAlley(object o)
		{
			return ((PlayerAlleyData)o).Alley;
		}

		internal static object AutoGeneratedGetMemberValueAssignedClanMember(object o)
		{
			return ((PlayerAlleyData)o).AssignedClanMember;
		}

		internal static object AutoGeneratedGetMemberValueUnderAttackBy(object o)
		{
			return ((PlayerAlleyData)o).UnderAttackBy;
		}

		internal static object AutoGeneratedGetMemberValueTroopRoster(object o)
		{
			return ((PlayerAlleyData)o).TroopRoster;
		}

		internal static object AutoGeneratedGetMemberValueLastRecruitTime(object o)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((PlayerAlleyData)o).LastRecruitTime;
		}

		internal static object AutoGeneratedGetMemberValueAttackResponseDueDate(object o)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((PlayerAlleyData)o).AttackResponseDueDate;
		}
	}

	private const int DesiredOccupiedAlleyPerTownFrequency = 2;

	private const int RelationLossWithSettlementOwnerAfterOccupyingAnAlley = -2;

	private const int RelationLossWithOtherNotablesUponOccupyingAnAlley = -1;

	private const int RelationLossWithOldOwnerUponClearingAlley = -5;

	private const int RelationGainWithOtherNotablesUponClearingAlley = 1;

	private const float SpawningNewAlleyFightDailyPercentage = 0.015f;

	private const float ConvertTroopsToThugsDailyPercentage = 0.01f;

	private const float GainOrLoseAlleyDailyBasePercentage = 0.02f;

	private CharacterObject _thug;

	private CharacterObject _expertThug;

	private CharacterObject _masterThug;

	private List<PlayerAlleyData> _playerOwnedCommonAreaData = new List<PlayerAlleyData>();

	private bool _battleWillStartInCurrentSettlement;

	private bool _waitForBattleResults;

	private bool _playerRetreatedFromMission;

	private bool _playerDiedInMission;

	private bool _playerIsInAlleyFightMission;

	private bool _playerAbandonedAlleyFromDialogRecently;

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.HeroKilledEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnHeroKilled);
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
		CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnNewGameCreated);
		CampaignEvents.AlleyOccupiedByPlayer.AddNonSerializedListener((object)this, (Action<Alley, TroopRoster>)OnAlleyOccupiedByPlayer);
		CampaignEvents.AlleyClearedByPlayer.AddNonSerializedListener((object)this, (Action<Alley>)OnAlleyClearedByPlayer);
		CampaignEvents.AlleyOwnerChanged.AddNonSerializedListener((object)this, (Action<Alley, Hero, Hero>)OnAlleyOwnerChanged);
		CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener((object)this, (Action<Settlement>)DailyTickSettlement);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener((object)this, (Action)DailyTick);
		CampaignEvents.CanHeroLeadPartyEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CommonAlleyLeaderRestriction);
		CampaignEvents.CanBeGovernorOrHavePartyRoleEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CommonAlleyLeaderRestriction);
		CampaignEvents.AfterMissionStarted.AddNonSerializedListener((object)this, (Action<IMission>)OnAfterMissionStarted);
		CampaignEvents.CanHeroDieEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, KillCharacterActionDetail, bool>)CanHeroDie);
	}

	private void CanHeroDie(Hero hero, KillCharacterActionDetail detail, ref bool result)
	{
		if (hero == Hero.MainHero && Mission.Current != null && _playerIsInAlleyFightMission)
		{
			result = false;
		}
	}

	private void OnAfterMissionStarted(IMission mission)
	{
		_playerIsInAlleyFightMission = false;
	}

	private void OnAlleyOwnerChanged(Alley alley, Hero newOwner, Hero oldOwner)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		if (oldOwner == Hero.MainHero)
		{
			TextObject val = new TextObject("{=wAgfOHio}You have lost the ownership of the alley at {SETTLEMENT}.", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT", ((SettlementArea)alley).Settlement.Name);
			MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	private void CommonAlleyLeaderRestriction(Hero hero, ref bool result)
	{
		if (_playerOwnedCommonAreaData.Any((PlayerAlleyData x) => x.AssignedClanMember == hero))
		{
			result = false;
		}
	}

	private void DailyTick()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		for (int num = _playerOwnedCommonAreaData.Count - 1; num >= 0; num--)
		{
			PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData[num];
			CheckConvertTroopsToBandits(playerAlleyData);
			SkillLevelingManager.OnDailyAlleyTick(playerAlleyData.Alley, playerAlleyData.AssignedClanMember);
			if (playerAlleyData.AssignedClanMember.IsDead && playerAlleyData.AssignedClanMember.DeathDay + Campaign.Current.Models.AlleyModel.DestroyAlleyAfterDaysWhenLeaderIsDeath < CampaignTime.Now)
			{
				_playerOwnedCommonAreaData.Remove(playerAlleyData);
				playerAlleyData.DestroyAlley();
			}
			else if (!playerAlleyData.IsUnderAttack && !playerAlleyData.AssignedClanMember.IsDead)
			{
				CheckSpawningNewAlleyFight(playerAlleyData);
			}
			else if (playerAlleyData.IsUnderAttack && ((CampaignTime)(ref playerAlleyData.AttackResponseDueDate)).IsPast)
			{
				_playerOwnedCommonAreaData.Remove(playerAlleyData);
				playerAlleyData.DestroyAlley();
			}
		}
	}

	private void CheckSpawningNewAlleyFight(PlayerAlleyData playerOwnedArea)
	{
		if (MBRandom.RandomFloat < 0.015f && ((SettlementArea)playerOwnedArea.Alley).Settlement.Alleys.Any((Alley x) => (int)x.State == 1))
		{
			StartNewAlleyAttack(playerOwnedArea);
		}
	}

	private void StartNewAlleyAttack(PlayerAlleyData playerOwnedArea)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		playerOwnedArea.UnderAttackBy = Extensions.GetRandomElementInefficiently<Alley>(((SettlementArea)playerOwnedArea.Alley).Settlement.Alleys.Where((Alley x) => (int)x.State == 1));
		((SettlementArea)playerOwnedArea.UnderAttackBy).Owner.SetHasMet();
		Debug.Print($"Player alley with id: {((SettlementArea)playerOwnedArea.Alley).Tag}  in settlement: {((SettlementArea)playerOwnedArea.Alley).Settlement} under attack by hero: {((SettlementArea)playerOwnedArea.UnderAttackBy).Owner}  with id: {((MBObjectBase)((SettlementArea)playerOwnedArea.UnderAttackBy).Owner).StringId} time: {CampaignTime.Now}", 0, (DebugColor)12, 17592186044416uL);
		float alleyAttackResponseTimeInDays = Campaign.Current.Models.AlleyModel.GetAlleyAttackResponseTimeInDays(playerOwnedArea.TroopRoster);
		playerOwnedArea.AttackResponseDueDate = CampaignTime.DaysFromNow(alleyAttackResponseTimeInDays);
		TextObject val = new TextObject("{=5bIpeW9X}Your alley in {SETTLEMENT} is under attack from neighboring gangs. Unless you go to their help, the alley will be lost in {RESPONSE_TIME} days.", (Dictionary<string, object>)null);
		val.SetTextVariable("SETTLEMENT", ((SettlementArea)playerOwnedArea.Alley).Settlement.Name);
		val.SetTextVariable("RESPONSE_TIME", alleyAttackResponseTimeInDays, 2);
		ChangeRelationAction.ApplyPlayerRelation(((SettlementArea)playerOwnedArea.UnderAttackBy).Owner, -5, true, true);
		Campaign.Current.CampaignInformationManager.NewMapNoticeAdded((InformationData)new AlleyUnderAttackMapNotification(playerOwnedArea.Alley, val));
	}

	private void CheckConvertTroopsToBandits(PlayerAlleyData playerOwnedArea)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		foreach (FlattenedTroopRosterElement item in playerOwnedArea.TroopRoster.ToFlattenedRoster())
		{
			FlattenedTroopRosterElement current = item;
			if (MBRandom.RandomFloat < 0.01f && !((BasicCharacterObject)((FlattenedTroopRosterElement)(ref current)).Troop).IsHero && (int)((FlattenedTroopRosterElement)(ref current)).Troop.Occupation != 27)
			{
				playerOwnedArea.TroopRoster.RemoveTroop(((FlattenedTroopRosterElement)(ref current)).Troop, 1, default(UniqueTroopDescriptor), 0);
				CharacterObject val = _thug;
				if (val.Tier < ((FlattenedTroopRosterElement)(ref current)).Troop.Tier)
				{
					val = _expertThug;
				}
				if (val.Tier < ((FlattenedTroopRosterElement)(ref current)).Troop.Tier)
				{
					val = _masterThug;
				}
				playerOwnedArea.TroopRoster.AddToCounts(val, 1, false, 0, 0, true, -1);
			}
		}
	}

	private void OnNewGameCreated(CampaignGameStarter gameStarter)
	{
		foreach (Town item in (List<Town>)(object)Town.AllTowns)
		{
			int num = MBRandom.RandomInt(0, ((SettlementComponent)item).Settlement.Alleys.Count);
			IEnumerable<Hero> source = ((IEnumerable<Hero>)((SettlementComponent)item).Settlement.Notables).Where((Hero x) => x.IsGangLeader);
			for (int num2 = num; num2 < num + 2; num2++)
			{
				((SettlementComponent)item).Settlement.Alleys[num2 % ((SettlementComponent)item).Settlement.Alleys.Count].SetOwner(source.ElementAt(num2 % source.Count()));
			}
		}
	}

	private void DailyTickSettlement(Settlement settlement)
	{
		if (settlement.IsTown)
		{
			TickAlleyOwnerships(settlement);
		}
	}

	private void TickAlleyOwnerships(Settlement settlement)
	{
		foreach (Hero notable in (List<Hero>)(object)settlement.Notables)
		{
			if (!notable.IsGangLeader)
			{
				continue;
			}
			int count = notable.OwnedAlleys.Count;
			float num = 0.02f - (float)count * 0.005f;
			float num2 = (float)count * 0.005f;
			if (MBRandom.RandomFloat < num)
			{
				Alley? obj = ((IEnumerable<Alley>)settlement.Alleys).FirstOrDefault((Func<Alley, bool>)((Alley x) => (int)x.State == 0));
				if (obj != null)
				{
					obj.SetOwner(notable);
				}
			}
			if (MBRandom.RandomFloat < num2 && !_playerOwnedCommonAreaData.Any(delegate(PlayerAlleyData x)
			{
				if (((SettlementArea)x.Alley).Settlement == settlement)
				{
					Alley underAttackBy = x.UnderAttackBy;
					return ((underAttackBy != null) ? ((SettlementArea)underAttackBy).Owner : null) == notable;
				}
				return false;
			}))
			{
				Alley randomElement = Extensions.GetRandomElement<Alley>((IReadOnlyList<Alley>)notable.OwnedAlleys);
				if (randomElement != null)
				{
					randomElement.SetOwner((Hero)null);
				}
			}
			if (!notable.IsHealthFull())
			{
				notable.Heal(10, false);
			}
		}
	}

	private void OnAlleyOccupiedByPlayer(Alley alley, TroopRoster troopRoster)
	{
		Hero owner = ((SettlementArea)alley).Owner;
		alley.SetOwner(Hero.MainHero);
		PlayerAlleyData item = new PlayerAlleyData(alley, troopRoster);
		_playerOwnedCommonAreaData.Add(item);
		ChangeRelationAction.ApplyPlayerRelation(owner, -5, true, true);
		if (((SettlementArea)alley).Settlement.OwnerClan != Clan.PlayerClan)
		{
			ChangeRelationAction.ApplyPlayerRelation(((SettlementArea)alley).Settlement.Owner, -2, true, true);
			foreach (Hero item2 in (List<Hero>)(object)((SettlementArea)alley).Settlement.Notables)
			{
				if (!item2.IsGangLeader)
				{
					ChangeRelationAction.ApplyPlayerRelation(item2, -1, true, true);
				}
			}
		}
		SkillLevelingManager.OnAlleyCleared(alley);
		AddPlayerAlleyCharacters(alley);
		Mission.Current.ClearCorpses(false);
	}

	private void OnAlleyClearedByPlayer(Alley alley)
	{
		ChangeRelationAction.ApplyPlayerRelation(((SettlementArea)alley).Owner, -5, true, true);
		foreach (Hero item in (List<Hero>)(object)((SettlementArea)alley).Settlement.Notables)
		{
			if (!item.IsGangLeader)
			{
				ChangeRelationAction.ApplyPlayerRelation(item, 1, true, true);
			}
		}
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		if (playerAlleyData?.UnderAttackBy == alley)
		{
			playerAlleyData.UnderAttackBy = null;
		}
		alley.SetOwner((Hero)null);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<List<PlayerAlleyData>>("_playerOwnedCommonAreaData", ref _playerOwnedCommonAreaData);
	}

	public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		_thug = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_1");
		_expertThug = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_2");
		_masterThug = MBObjectManager.Instance.GetObject<CharacterObject>("gangster_3");
		AddGameMenus(campaignGameStarter);
		AddDialogs(campaignGameStarter);
		if (!MBSaveLoad.IsUpdatingGameVersion)
		{
			return;
		}
		ApplicationVersion lastLoadedGameVersion = MBSaveLoad.LastLoadedGameVersion;
		if (!((ApplicationVersion)(ref lastLoadedGameVersion)).IsOlderThan(ApplicationVersion.FromString("v1.3.14.107126", 0)))
		{
			return;
		}
		foreach (PlayerAlleyData playerOwnedCommonAreaDatum in _playerOwnedCommonAreaData)
		{
			if (playerOwnedCommonAreaDatum.IsUnderAttack && ((SettlementArea)playerOwnedCommonAreaDatum.UnderAttackBy).Owner == null)
			{
				playerOwnedCommonAreaDatum.UnderAttackBy = null;
			}
		}
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Invalid comparison between Unknown and I4
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (!currentSettlement.IsTown)
		{
			return;
		}
		foreach (Alley alley in currentSettlement.Alleys)
		{
			if ((int)alley.State == 1)
			{
				foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)Campaign.Current.Models.AlleyModel.GetTroopsOfAIOwnedAlley(alley).GetTroopRoster())
				{
					TroopRosterElement current2 = item;
					for (int i = 0; i < ((TroopRosterElement)(ref current2)).Number; i++)
					{
						AddCharacterToAlley(current2.Character, alley);
					}
				}
			}
			else if ((int)alley.State == 2)
			{
				AddPlayerAlleyCharacters(alley);
			}
		}
	}

	private void AddPlayerAlleyCharacters(Alley alley)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.Current != null)
		{
			for (int num = ((List<Agent>)(object)Mission.Current.Agents).Count - 1; num >= 0; num--)
			{
				Agent val = ((List<Agent>)(object)Mission.Current.Agents)[num];
				if (val.IsHuman && !val.Character.IsHero)
				{
					CampaignAgentComponent component = val.GetComponent<CampaignAgentComponent>();
					object obj;
					if (component == null)
					{
						obj = null;
					}
					else
					{
						AgentNavigator agentNavigator = component.AgentNavigator;
						if (agentNavigator == null)
						{
							obj = null;
						}
						else
						{
							Alley memberOfAlley = agentNavigator.MemberOfAlley;
							obj = ((memberOfAlley != null) ? ((SettlementArea)memberOfAlley).Owner : null);
						}
					}
					if (obj == Hero.MainHero)
					{
						val.FadeOut(false, true);
					}
				}
			}
		}
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)_playerOwnedCommonAreaData.First((PlayerAlleyData x) => x.Alley == alley).TroopRoster.GetTroopRoster())
		{
			TroopRosterElement current = item;
			if (!((BasicCharacterObject)current.Character).IsHero || !current.Character.HeroObject.IsTraveling)
			{
				for (int num2 = 0; num2 < ((TroopRosterElement)(ref current)).Number; num2++)
				{
					AddCharacterToAlley(current.Character, alley);
				}
			}
		}
	}

	private void AddCharacterToAlley(CharacterObject character, Alley alley)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
		LocationCharacter val = null;
		if (((BasicCharacterObject)character).IsHero)
		{
			Location locationOfCharacter = Settlement.CurrentSettlement.LocationComplex.GetLocationOfCharacter(character.HeroObject);
			if (locationOfCharacter != null && locationOfCharacter == locationWithId)
			{
				return;
			}
			val = Settlement.CurrentSettlement.LocationComplex.GetLocationCharacterOfHero(character.HeroObject);
		}
		if (val == null)
		{
			Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)character).Race, "_settlement");
			int num = default(int);
			int num2 = default(int);
			Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(character, ref num, ref num2, "AlleyGangMember");
			AgentData val2 = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)character, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(monsterWithSuffix).NoHorses(true).Age(MBRandom.RandomInt(num, num2));
			val = new LocationCharacter(val2, new AddBehaviorsDelegate(BehaviorSets.AddFixedCharacterBehaviors), ((SettlementArea)alley).Tag, true, (CharacterRelations)0, ActionSetCode.GenerateActionSetNameWithSuffix(val2.AgentMonster, val2.AgentIsFemale, "_villain"), true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
		}
		val.SpecialTargetTag = ((SettlementArea)alley).Tag;
		val.SetAlleyOfCharacter(alley);
		Settlement.CurrentSettlement.LocationComplex.ChangeLocation(val, Settlement.CurrentSettlement.LocationComplex.GetLocationOfCharacter(val), locationWithId);
		if (((BasicCharacterObject)character).IsHero)
		{
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnHeroGetsBusy(character.HeroObject, (HeroGetsBusyReasons)3);
		}
	}

	protected void AddGameMenus(CampaignGameStarter campaignGameSystemStarter)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0031: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0081: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00b2: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00e3: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0114: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_0164: Expected O, but got Unknown
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Expected O, but got Unknown
		//IL_0195: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Expected O, but got Unknown
		//IL_01c6: Expected O, but got Unknown
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Expected O, but got Unknown
		//IL_020b: Expected O, but got Unknown
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Expected O, but got Unknown
		//IL_0250: Expected O, but got Unknown
		campaignGameSystemStarter.AddGameMenuOption("town", "manage_alley", "{=VkOtMe5a}Go to alley", new OnConditionDelegate(go_to_alley_on_condition), new OnConsequenceDelegate(go_to_alley_on_consequence), false, 4, false, (object)null);
		campaignGameSystemStarter.AddGameMenu("manage_alley", "{=dWf6ztYu}You are in your alley by the {ALLEY_TYPE}, {FURTHER_INFO}", new OnInitDelegate(manage_alley_menu_on_init), (MenuOverlayType)3, (MenuFlags)0, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("manage_alley", "confront_hostile_alley_leader", "{=grhRXqen}Confront {HOSTILE_GANG_LEADER.NAME} about {?HOSTILE_GANG_LEADER.GENDER}her{?}his{\\?} attack on your alley.", new OnConditionDelegate(alley_under_attack_on_condition), new OnConsequenceDelegate(alley_under_attack_response_on_consequence), false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("manage_alley", "change_leader_of_alley", "{=ClyaDhGU}Change the leader of the alley", new OnConditionDelegate(change_leader_of_alley_on_condition), new OnConsequenceDelegate(change_leader_of_the_alley_on_consequence), false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("manage_alley", "manage_alley_troops", "{=QrBCe41Z}Manage alley troops", new OnConditionDelegate(manage_alley_on_condition), new OnConsequenceDelegate(manage_troops_of_alley), false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("manage_alley", "abandon_alley", "{=ELfguvYD}Abandon the alley", new OnConditionDelegate(abandon_alley_on_condition), new OnConsequenceDelegate(abandon_alley_are_you_sure_consequence), false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenu("manage_alley_abandon_are_you_sure", "{=awjomtnJ}Are you sure?", new OnInitDelegate(abandon_alley_yes_no_on_init), (MenuOverlayType)3, (MenuFlags)0, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("manage_alley_abandon_are_you_sure", "abandon_alley_yes", "{=aeouhelq}Yes", new OnConditionDelegate(alley_continue_on_condition), new OnConsequenceDelegate(abandon_alley_consequence), false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("manage_alley_abandon_are_you_sure", "abandon_alley_no", "{=8OkPHu4f}No", new OnConditionDelegate(alley_go_back_on_condition), new OnConsequenceDelegate(go_to_alley_on_consequence), true, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("manage_alley", "back", "{=4QNycK7T}Go back", new OnConditionDelegate(alley_go_back_on_condition), new OnConsequenceDelegate(leave_alley_menu_consequence), true, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenu("alley_fight_lost", "{=po79q14T}You have failed to defend your alley against the attack, you have lost the ownership of it.", (OnInitDelegate)null, (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("alley_fight_lost", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(alley_continue_on_condition), new OnConsequenceDelegate(alley_fight_continue_on_consequence), false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenu("alley_fight_won", "{=i1sgAm0F}You have succeeded in defending your alley against the attack. You might want to leave some troops in order to compensate for your losses in the fight.", (OnInitDelegate)null, (MenuOverlayType)0, (MenuFlags)0, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("alley_fight_won", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(alley_continue_on_condition), new OnConsequenceDelegate(alley_fight_continue_on_consequence), false, -1, false, (object)null);
	}

	private void abandon_alley_yes_no_on_init(MenuCallbackArgs args)
	{
		if (_playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement) == null)
		{
			GameMenu.SwitchToMenu("town");
		}
	}

	private void abandon_alley_are_you_sure_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("manage_alley_abandon_are_you_sure");
	}

	private bool alley_continue_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		return true;
	}

	private void alley_fight_continue_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("town");
	}

	private bool alley_go_back_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private bool abandon_alley_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)21;
		return true;
	}

	private void alley_under_attack_response_on_consequence(MenuCallbackArgs args)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, false, false, false, false, false, false), new ConversationCharacterData(((SettlementArea)playerAlleyData.UnderAttackBy).Owner.CharacterObject, (PartyBase)null, false, false, false, false, false, false));
	}

	private bool alley_under_attack_on_condition(MenuCallbackArgs args)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => ((SettlementArea)x.Alley).Owner == Hero.MainHero && ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement && x.IsUnderAttack);
		if (playerAlleyData != null)
		{
			args.optionLeaveType = (LeaveType)23;
			StringHelpers.SetCharacterProperties("HOSTILE_GANG_LEADER", ((SettlementArea)playerAlleyData.UnderAttackBy).Owner.CharacterObject, (TextObject)null, false);
			TextObject val = new TextObject("{=9t1LGNz6}{RESPONSE_TIME} {?RESPONSE_TIME>1}days{?}day{\\?} left.", (Dictionary<string, object>)null);
			val.SetTextVariable("RESPONSE_TIME", GetResponseTimeLeftForAttackInDays(playerAlleyData.Alley));
			args.Tooltip = val;
			return true;
		}
		return false;
	}

	private bool manage_alley_on_condition(MenuCallbackArgs args)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (alley_under_attack_on_condition(args))
		{
			args.IsEnabled = false;
			args.Tooltip = new TextObject("{=pdqi2qz1}You can not do this action while your alley is under attack.", (Dictionary<string, object>)null);
		}
		args.optionLeaveType = (LeaveType)29;
		return true;
	}

	private bool change_leader_of_alley_on_condition(MenuCallbackArgs args)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (alley_under_attack_on_condition(args))
		{
			args.IsEnabled = false;
			args.Tooltip = new TextObject("{=pdqi2qz1}You can not do this action while your alley is under attack.", (Dictionary<string, object>)null);
		}
		args.optionLeaveType = (LeaveType)17;
		return true;
	}

	private bool go_to_alley_on_condition(MenuCallbackArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)2;
		return _playerOwnedCommonAreaData.Any((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
	}

	private void go_to_alley_on_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("manage_alley");
	}

	private void leave_alley_menu_consequence(MenuCallbackArgs args)
	{
		GameMenu.SwitchToMenu("town_outside");
	}

	private void abandon_alley_consequence(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)9;
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		_playerOwnedCommonAreaData.Remove(playerAlleyData);
		playerAlleyData.AbandonTheAlley();
		GameMenu.SwitchToMenu("town_outside");
	}

	private void manage_troops_of_alley(MenuCallbackArgs args)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_004f: Expected O, but got Unknown
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		AlleyHelper.OpenScreenForManagingAlley(false, playerAlleyData.TroopRoster, new PartyPresentationDoneButtonDelegate(OnPartyScreenClosed), new TextObject("{=dQBArrqh}Manage Alley", (Dictionary<string, object>)null), (PartyPresentationCancelButtonDelegate)null);
	}

	private bool OnPartyScreenClosed(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
	{
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		playerAlleyData.TroopRoster = leftMemberRoster;
		if (Mission.Current != null)
		{
			AddPlayerAlleyCharacters(playerAlleyData.Alley);
		}
		return true;
	}

	private void change_leader_of_the_alley_on_consequence(MenuCallbackArgs args)
	{
		AlleyHelper.CreateMultiSelectionInquiryForSelectingClanMemberToAlley(_playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement).Alley, (Action<List<InquiryElement>>)ChangeAssignedClanMemberOfAlley, (Action<List<InquiryElement>>)null);
	}

	private void ChangeAssignedClanMemberOfAlley(List<InquiryElement> newClanMemberInquiryElement)
	{
		PlayerAlleyData alleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		object identifier = newClanMemberInquiryElement.First().Identifier;
		Hero heroObject = ((CharacterObject)((identifier is CharacterObject) ? identifier : null)).HeroObject;
		ChangeTheLeaderOfAlleyInternal(alleyData, heroObject);
	}

	private void manage_alley_menu_on_init(MenuCallbackArgs args)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		Campaign.Current.GameMenuManager.MenuLocations.Clear();
		Campaign.Current.GameMenuManager.MenuLocations.Add(Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("alley"));
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		if (playerAlleyData == null)
		{
			GameMenu.SwitchToMenu(_playerAbandonedAlleyFromDialogRecently ? "town" : "alley_fight_lost");
			_playerAbandonedAlleyFromDialogRecently = false;
			return;
		}
		MBTextManager.SetTextVariable("ALLEY_TYPE", ((SettlementArea)playerAlleyData.Alley).Name, false);
		if (playerAlleyData.AssignedClanMember.IsTraveling)
		{
			TextObject val = new TextObject("{=AjBYneFr}{CLAN_MEMBER.NAME} is in charge of the alley. {?CLAN_MEMBER.GENDER}She{?}He{\\?} is currently traveling to the alley and will arrive after {HOURS} {?HOURS > 1}hours{?}hour{\\?}.", (Dictionary<string, object>)null);
			int num = MathF.Ceiling(TeleportationHelper.GetHoursLeftForTeleportingHeroToReachItsDestination(playerAlleyData.AssignedClanMember));
			val.SetTextVariable("HOURS", num);
			MBTextManager.SetTextVariable("FURTHER_INFO", val, false);
		}
		else if (playerAlleyData.AssignedClanMember.IsDead)
		{
			TextObject val = new TextObject("{=P5UbgK4c}{CLAN_MEMBER.NAME} was in charge of the alley. {?CLAN_MEMBER.GENDER}She{?}He{\\?} is dead. Alley will be abandoned after {REMAINING_DAYS} {?REMAINING_DAYS>1}days{?}day{\\?} unless a new overseer is assigned.", (Dictionary<string, object>)null);
			TextObject obj = val;
			CampaignTime val2 = playerAlleyData.AssignedClanMember.DeathDay + Campaign.Current.Models.AlleyModel.DestroyAlleyAfterDaysWhenLeaderIsDeath - CampaignTime.Now;
			obj.SetTextVariable("REMAINING_DAYS", (int)Math.Ceiling(((CampaignTime)(ref val2)).ToDays));
			MBTextManager.SetTextVariable("FURTHER_INFO", val, false);
		}
		else
		{
			TextObject val3 = new TextObject("{=fcqdfY09}{CLAN_MEMBER.NAME} is in charge of the alley.", (Dictionary<string, object>)null);
			MBTextManager.SetTextVariable("FURTHER_INFO", val3, false);
		}
		StringHelpers.SetCharacterProperties("CLAN_MEMBER", playerAlleyData.AssignedClanMember.CharacterObject, (TextObject)null, false);
		if (_waitForBattleResults)
		{
			_waitForBattleResults = false;
			if (playerAlleyData.TroopRoster.Contains(CharacterObject.PlayerCharacter))
			{
				playerAlleyData.TroopRoster.AddToCounts(CharacterObject.PlayerCharacter, -1, true, 0, 0, true, -1);
			}
			if ((playerAlleyData.TroopRoster.TotalManCount == 0 && _playerDiedInMission) || _playerRetreatedFromMission)
			{
				_playerOwnedCommonAreaData.Remove(playerAlleyData);
				playerAlleyData.AlleyFightLost();
			}
			else
			{
				playerAlleyData.AlleyFightWon();
			}
			_playerRetreatedFromMission = false;
			_playerDiedInMission = false;
		}
		if (_battleWillStartInCurrentSettlement)
		{
			StartAlleyFightWithOtherAlley();
		}
	}

	private void StartAlleyFightWithOtherAlley()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		TroopRoster troopRoster = playerAlleyData.TroopRoster;
		if (playerAlleyData.AssignedClanMember.IsTraveling)
		{
			troopRoster.RemoveTroop(playerAlleyData.AssignedClanMember.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
		}
		troopRoster.AddToCounts(CharacterObject.PlayerCharacter, 1, true, 0, 0, true, -1);
		TroopRoster troopsOfAlleyForBattleMission = Campaign.Current.Models.AlleyModel.GetTroopsOfAlleyForBattleMission(playerAlleyData.UnderAttackBy);
		int wallLevel = Settlement.CurrentSettlement.Town.GetWallLevel();
		string scene = Settlement.CurrentSettlement.LocationComplex.GetScene("center", wallLevel);
		Location locationWithId = LocationComplex.Current.GetLocationWithId("center");
		CampaignMission.OpenAlleyFightMission(scene, wallLevel, locationWithId, troopRoster, troopsOfAlleyForBattleMission);
		_battleWillStartInCurrentSettlement = false;
		_waitForBattleResults = true;
	}

	protected void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Expected O, but got Unknown
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Expected O, but got Unknown
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Expected O, but got Unknown
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Expected O, but got Unknown
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Expected O, but got Unknown
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Expected O, but got Unknown
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Expected O, but got Unknown
		//IL_0365: Expected O, but got Unknown
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Expected O, but got Unknown
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Expected O, but got Unknown
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Expected O, but got Unknown
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Expected O, but got Unknown
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("alley_talk_start_player_owned_thug", "start", "alley_player_owned_start_thug", "{=!}{FURTHER_DETAIL}", new OnConditionDelegate(alley_talk_player_owned_thug_on_condition), (OnConsequenceDelegate)null, 120, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_thug_answer", "alley_player_owned_start_thug", "close_window", "{=GvpvZEba}Very well, take care.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 120, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_talk_start_player_owned_alley_manager_not_under_attack", "start", "alley_player_owned_start", "{=cwqR0pp1}Greetings my {?PLAYER.GENDER}lady{?}lord{\\?}. It's good to see you here. How can I help you?", new OnConditionDelegate(alley_talk_player_owned_alley_managed_not_under_attack_on_condition), (OnConsequenceDelegate)null, 120, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_talk_start_player_owned_alley_manager_under_attack", "start", "close_window", "{=jaFWM6sN}Good to have you here, my {?PLAYER.GENDER}lady{?}lord{\\?}. We shall raze them down now.", new OnConditionDelegate(alley_talk_player_owned_alley_managed_common_condition), (OnConsequenceDelegate)null, 120, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_alley_manager_answer_1", "alley_player_owned_start", "alley_manager_general_answer", "{=xJJeXW6j}Let me inspect your troops.", (OnConditionDelegate)null, new OnConsequenceDelegate(manage_troops_of_alley_from_dialog), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_alley_manager_answer_2", "alley_player_owned_start", "player_asked_for_volunteers", "{=ah3WKIc8}I could use some more troops in my party. Have you found any volunteers?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_alley_manager_answer_3", "alley_player_owned_start", "alley_manager_general_answer", "{=ut26sd6p}I want somebody else to take charge of this place.", (OnConditionDelegate)null, new OnConsequenceDelegate(change_leader_of_alley_from_dialog), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_alley_manager_answer_4", "alley_player_owned_start", "abandon_alley_are_you_sure", "{=I8o7oarw}I want to abandon this area.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_talk_start_player_owned_alley_manager_answer_4_1", "abandon_alley_are_you_sure", "abandon_alley_are_you_sure_player", "{=6dDXb4iI}Are you sure? If you are, we can pack up and join you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_alley_manager_answer_4_2", "abandon_alley_are_you_sure_player", "start", "{=ALWqXMiP}Yes, I am sure.", (OnConditionDelegate)null, new OnConsequenceDelegate(abandon_alley_from_dialog_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_alley_manager_answer_4_3", "abandon_alley_are_you_sure_player", "start", "{=YJkiQ6nM}No, I have changed my mind. We will stay here.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_alley_manager_answer_5", "alley_player_owned_start", "close_window", "{=D33fIGQe}Never mind.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_talk_start_player_owned_alley_manager_volunteers_1", "player_asked_for_volunteers", "alley_player_owned_start", "{=nRVrXSbv}Not yet my {?PLAYER.GENDER}lady{?}lord{\\?}, but I am working on it. Better check back next week.", new OnConditionDelegate(alley_has_no_troops_to_recruit), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_talk_start_player_owned_alley_manager_volunteers_2", "player_asked_for_volunteers", "alley_player_ask_for_troops", "{=aLrK7Si7}Yes. I have {TROOPS_TO_RECRUIT} ready to join you.", new OnConditionDelegate(get_troops_to_recruit_from_alley), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_alley_manager_volunteers_3", "alley_player_ask_for_troops", "give_troops_to_player", "{=BNz4ZA6S}Very well. Have them join me now.", (OnConditionDelegate)null, new OnConsequenceDelegate(player_recruited_troops_from_alley), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_talk_start_player_owned_alley_manager_volunteers_4", "give_troops_to_player", "start", "{=PlIYRSIz}All right my {?PLAYER.GENDER}lady{?}lord{\\?}, they will be ready.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_player_owned_alley_manager_volunteers_5", "alley_player_ask_for_troops", "start", "{=n1qrbQVa}I don't need them right now.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_talk_start_player_owned_alley_manager_answer_2_di", "alley_manager_general_answer", "start", "{=lF5HkBDy}As you wish.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_talk_start_normal", "start", "alley_talk_start", "{=qT4nbaAY}Oi, you, what are you doing here?", new OnConditionDelegate(alley_talk_start_normal_on_condition), (OnConsequenceDelegate)null, 120, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_talk_start_normal_2", "start", "alley_talk_start_confront", "{=MzHbdTYe}Well well well, I wasn't expecting to see you there. There must be some little birds informing you about my plans. That won't change anything, though. I'll still crush you.", new OnConditionDelegate(alley_confront_dialog_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_normal_3", "alley_talk_start_confront", "close_window", "{=GMsZZQzI}Bring it on.", (OnConditionDelegate)null, new OnConsequenceDelegate(start_alley_fight_after_conversation), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_talk_start_normal_4", "alley_talk_start_confront", "close_window", "{=QNpuyzc4}Take it easy. I have no interest in the place any more. Take it.", (OnConditionDelegate)null, new OnConsequenceDelegate(abandon_alley_from_dialog_consequence), 100, new OnClickableConditionDelegate(alley_abandon_while_under_attack_clickable_condition), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_start_1", "alley_talk_start", "alley_activity", "{=1NSRPYZt}Just passing through. What goes on here?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_start_2", "alley_talk_start", "first_entry_to_alley_2", "{=HCmQmZbe}I'm just having a look. Do you mind?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_start_3", "alley_talk_start", "close_window", "{=iW9iKbb8}Nothing.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_entry_start_1", "alley_first_talk_start", "first_entry_to_alley_2", "{=X18yfvX7}Just passing through.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_entry_start_2", "alley_first_talk_start", "first_entry_to_alley_2", "{=Y1O5bPpJ}Having a look. Do you mind?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_entry_start_3", "alley_first_talk_start", "first_entry_to_alley_2", "{=eQfL2UmE}None of your business.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("first_entry_to_alley", "first_entry_to_alley_2", "alley_options", "{=Ll2wN2Gm}This is how it goes, friend. This is our turf. We answer to {ALLEY_BOSS.NAME}, and {?ALLEY_BOSS.GENDER}she's{?}he's{\\?} like the {?ALLEY_BOSS.GENDER}queen{?}king{\\?} here. So if you haven't got a good reason to be here, clear out.", new OnConditionDelegate(enter_alley_rude_on_occasion), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("first_entry_to_alley_friendly", "first_entry_to_alley_2", "alley_options", "{=Fo47BuSY}Fine, you know {ALLEY_BOSS.NAME}, you can be here. Just no trouble, you understand?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("ally_entry_start_fight", "alley_options", "alley_fight_start", "{=2Fxva3RY}I don't take orders from the likes of you.", (OnConditionDelegate)null, new OnConsequenceDelegate(start_alley_fight_on_consequence), 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_entry_question_activity", "alley_options", "alley_activity", "{=aNZKqAAS}So what goes on here?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("alley_entry_end_conversation", "alley_options", "close_window", "{=Mk3Qfb4D}I don't want any trouble. Later.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_fight_start", "alley_fight_start", "close_window", "{=EN3Zqyx5}A mouthy one, eh? At him, lads![ib:aggressive][if:convo_angry]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_activity", "alley_activity", "alley_activity_2", "{=bZ5rXBY5}{ALLEY_ACTIVITY_STRING}", new OnConditionDelegate(alley_activity_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_activity_2", "alley_activity_2", "alley_options_player", "{=eZq11NVD}And by the way, we take orders from {ALLEY_BOSS.NAME}, and no one else.", new OnConditionDelegate(alley_activity_2_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_activity_back", "alley_options_decline", "alley_options", "{=pf4EIcBQ}Anything else? Because unless you've got business here, maybe you'd best move along.[if:convo_confused_annoyed][ib:closed]", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_activity_end", "alley_options_player", "close_window", "{=xb1Ps6ZC}Now get lost...", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("alley_meet_boss", "alley_meet_boss", "close_window", "{=NoxFbtEa}Wait here. We'll see if {?ALLEY_BOSS.GENDER}she{?}he{\\?} wants to talk to you. (NOT IMPLEMENTED)", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("gang_leader_bodyguard_start", "start", "close_window", "{=NVvfxdIc}You best talk to the boss.", new OnConditionDelegate(gang_leader_bodyguard_on_condition), (OnConsequenceDelegate)null, 200, (OnClickableConditionDelegate)null);
	}

	private bool alley_abandon_while_under_attack_clickable_condition(out TextObject explanation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		explanation = new TextObject("{=3E1XVyGM}You will lose the ownership of the alley.", (Dictionary<string, object>)null);
		return true;
	}

	private bool alley_confront_dialog_on_condition()
	{
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		if (playerAlleyData != null && playerAlleyData.IsUnderAttack)
		{
			return ((SettlementArea)playerAlleyData.UnderAttackBy).Owner == Hero.OneToOneConversationHero;
		}
		return false;
	}

	private void start_alley_fight_after_conversation()
	{
		_battleWillStartInCurrentSettlement = true;
		Campaign.Current.GameMenuManager.SetNextMenu("manage_alley");
		if (Mission.Current != null)
		{
			Mission.Current.EndMission();
		}
	}

	private void player_recruited_troops_from_alley()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)Campaign.Current.Models.AlleyModel.GetTroopsToRecruitFromAlleyDependingOnAlleyRandom(playerAlleyData.Alley, playerAlleyData.RandomFloatWeekly).GetTroopRoster())
		{
			TroopRosterElement current = item;
			MobileParty.MainParty.MemberRoster.AddToCounts(current.Character, ((TroopRosterElement)(ref current)).Number, false, 0, 0, true, -1);
		}
		MBInformationManager.AddQuickInformation(new TextObject("{=8CW2y0p3}Troops have been joined to your party", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		playerAlleyData.LastRecruitTime = CampaignTime.Now;
	}

	private bool get_troops_to_recruit_from_alley()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		TroopRoster troopsToRecruitFromAlleyDependingOnAlleyRandom = Campaign.Current.Models.AlleyModel.GetTroopsToRecruitFromAlleyDependingOnAlleyRandom(playerAlleyData.Alley, playerAlleyData.RandomFloatWeekly);
		List<TextObject> list = new List<TextObject>();
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)troopsToRecruitFromAlleyDependingOnAlleyRandom.GetTroopRoster())
		{
			TroopRosterElement current = item;
			TextObject val = new TextObject("{=!}{TROOP_COUNT} {?TROOP_COUNT > 1}{TROOP_NAME}{.s}{?}{TROOP_NAME}{\\?}", (Dictionary<string, object>)null);
			val.SetTextVariable("TROOP_COUNT", ((TroopRosterElement)(ref current)).Number);
			val.SetTextVariable("TROOP_NAME", ((BasicCharacterObject)current.Character).Name);
			list.Add(val);
		}
		TextObject val2 = GameTextHelper.MergeTextObjectsWithComma(list, true);
		MBTextManager.SetTextVariable("TROOPS_TO_RECRUIT", val2, false);
		return true;
	}

	private bool alley_has_no_troops_to_recruit()
	{
		return _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement).RandomFloatWeekly > 0.5f;
	}

	private void change_leader_of_alley_from_dialog()
	{
		AlleyHelper.CreateMultiSelectionInquiryForSelectingClanMemberToAlley(_playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement).Alley, (Action<List<InquiryElement>>)ChangeAssignedClanMemberOfAlley, (Action<List<InquiryElement>>)null);
	}

	private void manage_troops_of_alley_from_dialog()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_004f: Expected O, but got Unknown
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		AlleyHelper.OpenScreenForManagingAlley(false, playerAlleyData.TroopRoster, new PartyPresentationDoneButtonDelegate(OnPartyScreenClosed), new TextObject("{=dQBArrqh}Manage Alley", (Dictionary<string, object>)null), (PartyPresentationCancelButtonDelegate)null);
	}

	private void abandon_alley_from_dialog_consequence()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.First((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		if (Mission.Current != null)
		{
			for (int num = ((List<Agent>)(object)Mission.Current.Agents).Count - 1; num >= 0; num--)
			{
				Agent val = ((List<Agent>)(object)Mission.Current.Agents)[num];
				if (val.IsHuman)
				{
					CampaignAgentComponent component = val.GetComponent<CampaignAgentComponent>();
					object obj;
					if (component == null)
					{
						obj = null;
					}
					else
					{
						AgentNavigator agentNavigator = component.AgentNavigator;
						if (agentNavigator == null)
						{
							obj = null;
						}
						else
						{
							Alley memberOfAlley = agentNavigator.MemberOfAlley;
							obj = ((memberOfAlley != null) ? ((SettlementArea)memberOfAlley).Owner : null);
						}
					}
					if (obj == Hero.MainHero && (object)Hero.OneToOneConversationHero.CharacterObject != val.Character)
					{
						val.FadeOut(false, true);
					}
				}
			}
		}
		_playerOwnedCommonAreaData.Remove(playerAlleyData);
		playerAlleyData.AbandonTheAlley();
		if (Mission.Current != null && ((SettlementArea)playerAlleyData.Alley).Owner != null)
		{
			foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)Campaign.Current.Models.AlleyModel.GetTroopsOfAIOwnedAlley(playerAlleyData.Alley).GetTroopRoster())
			{
				TroopRosterElement current = item;
				for (int num2 = 0; num2 < ((TroopRosterElement)(ref current)).Number; num2++)
				{
					AddCharacterToAlley(current.Character, playerAlleyData.Alley);
				}
			}
		}
		_playerAbandonedAlleyFromDialogRecently = true;
	}

	private bool alley_talk_player_owned_alley_managed_not_under_attack_on_condition()
	{
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		if (playerAlleyData != null && !playerAlleyData.IsUnderAttack)
		{
			return alley_talk_player_owned_alley_managed_common_condition();
		}
		return false;
	}

	private bool alley_talk_player_owned_alley_managed_common_condition()
	{
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
		if (playerAlleyData != null)
		{
			return playerAlleyData.AssignedClanMember == Hero.OneToOneConversationHero;
		}
		return false;
	}

	private bool alley_talk_player_owned_thug_on_condition()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		if (!((BasicCharacterObject)CharacterObject.OneToOneConversationCharacter).IsHero)
		{
			PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => ((SettlementArea)x.Alley).Settlement == Settlement.CurrentSettlement);
			if (playerAlleyData != null)
			{
				CampaignAgentComponent component = ((Agent)Campaign.Current.ConversationManager.OneToOneConversationAgent).GetComponent<CampaignAgentComponent>();
				if (component != null && component.AgentNavigator.MemberOfAlley == playerAlleyData.Alley)
				{
					if (playerAlleyData.IsAssignedClanMemberDead)
					{
						TextObject val = new TextObject("{=SdKTUIVJ}Oi, my {?PLAYER.GENDER}lady{?}lord{\\?}. Sorry for your loss, {DEAD_ALLEY_LEADER.NAME} will be missed in these streets. We are waiting for you to appoint a new boss, whenever you’re ready.", (Dictionary<string, object>)null);
						StringHelpers.SetCharacterProperties("DEAD_ALLEY_LEADER", playerAlleyData.AssignedClanMember.CharacterObject, (TextObject)null, false);
						MBTextManager.SetTextVariable("FURTHER_DETAIL", val, false);
					}
					else if (playerAlleyData.AssignedClanMember.IsTraveling)
					{
						TextObject val2 = new TextObject("{=KKvOQAVa}We are waiting for {TRAVELING_ALLEY_LEADER.NAME} to come. Until {?TRAVELING_ALLEY_LEADER.GENDER}she{?}he{\\?} arrives, we'll be extra watchful.", (Dictionary<string, object>)null);
						StringHelpers.SetCharacterProperties("TRAVELING_ALLEY_LEADER", playerAlleyData.AssignedClanMember.CharacterObject, (TextObject)null, false);
						MBTextManager.SetTextVariable("FURTHER_DETAIL", val2, false);
					}
					else
					{
						TextObject val3 = new TextObject("{=OPwO5RXC}Welcome, boss. We're honored to have you here. You can be sure we're keeping an eye on everything going on.", (Dictionary<string, object>)null);
						MBTextManager.SetTextVariable("FURTHER_DETAIL", val3, false);
					}
					return true;
				}
			}
		}
		return false;
	}

	private bool alley_activity_on_condition()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		List<TextObject> list = new List<TextObject>();
		Alley lastVisitedAlley = CampaignMission.Current.LastVisitedAlley;
		if (((SettlementArea)lastVisitedAlley).Owner.GetTraitLevel(DefaultTraits.Thug) > 0)
		{
			list.Add(new TextObject("{=prJBRboS}we look after the honest folk here. Make sure no one smashes up their shops. And if they want to give us a coin or two as a way of saying thanks, well, who'd mind?", (Dictionary<string, object>)null));
		}
		if (((SettlementArea)lastVisitedAlley).Owner.GetTraitLevel(DefaultTraits.Smuggler) > 0)
		{
			list.Add(new TextObject("{=CqnAGehj}suppose someone wanted to buy some goods and didn't want to pay the customs tax. We might be able to help that person out.", (Dictionary<string, object>)null));
		}
		if (((SettlementArea)lastVisitedAlley).Owner.Gold > 100)
		{
			list.Add(new TextObject("{=U8iyCXmF}we help out those who are down on their luck. Give 'em a purse of silver to tide them by. With a bit of speculative interest, naturally.", (Dictionary<string, object>)null));
		}
		MBTextManager.SetTextVariable("ALLEY_ACTIVITY_STRING", "{=1rCk6xRa}Now then... If you're asking,[if:convo_normal][ib:normal]", false);
		for (int i = 0; i < list.Count; i++)
		{
			MBTextManager.SetTextVariable("ALLEY_ACTIVITY_ADDITION", ((object)list[i]).ToString(), false);
			MBTextManager.SetTextVariable("ALLEY_ACTIVITY_STRING", ((object)new TextObject("{=jVjIkODa}{ALLEY_ACTIVITY_STRING} {ALLEY_ACTIVITY_ADDITION}", (Dictionary<string, object>)null)).ToString(), false);
			if (i + 1 != list.Count)
			{
				MBTextManager.SetTextVariable("ALLEY_ACTIVITY_ADDITION", "{=lbNl0a8t}Also,", false);
				MBTextManager.SetTextVariable("ALLEY_ACTIVITY_STRING", ((object)new TextObject("{=jVjIkODa}{ALLEY_ACTIVITY_STRING} {ALLEY_ACTIVITY_ADDITION}", (Dictionary<string, object>)null)).ToString(), false);
			}
		}
		return true;
	}

	private bool alley_activity_2_on_condition()
	{
		StringHelpers.SetCharacterProperties("ALLEY_BOSS", ((SettlementArea)CampaignMission.Current.LastVisitedAlley).Owner.CharacterObject, (TextObject)null, false);
		return true;
	}

	private bool alley_talk_start_normal_on_condition()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		Agent oneToOneConversationAgent = ConversationMission.OneToOneConversationAgent;
		AgentNavigator agentNavigator = ((oneToOneConversationAgent != null) ? oneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator : null);
		if (agentNavigator?.MemberOfAlley != null && (int)agentNavigator.MemberOfAlley.State == 1 && ((SettlementArea)agentNavigator.MemberOfAlley).Owner != Hero.MainHero && Mission.Current.GetMissionBehavior<MissionAlleyHandler>() != null && Mission.Current.GetMissionBehavior<MissionAlleyHandler>().CanThugConversationBeTriggered)
		{
			CampaignMission.Current.LastVisitedAlley = agentNavigator.MemberOfAlley;
			return true;
		}
		return false;
	}

	private bool enter_alley_rude_on_occasion()
	{
		Agent oneToOneConversationAgent = ConversationMission.OneToOneConversationAgent;
		Hero owner = ((SettlementArea)((oneToOneConversationAgent != null) ? oneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.MemberOfAlley : null)).Owner;
		float relationWithPlayer = owner.GetRelationWithPlayer();
		StringHelpers.SetCharacterProperties("ALLEY_BOSS", owner.CharacterObject, (TextObject)null, false);
		if (owner.HasMet)
		{
			return relationWithPlayer < -5f;
		}
		return true;
	}

	private void start_alley_fight_on_consequence()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		_playerIsInAlleyFightMission = true;
		Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
		{
			Mission.Current.GetMissionBehavior<MissionAlleyHandler>().StartCommonAreaBattle(CampaignMission.Current.LastVisitedAlley);
		};
		LogEntry.AddLogEntry((LogEntry)new PlayerAttackAlleyLogEntry(((SettlementArea)CampaignMission.Current.LastVisitedAlley).Owner, Hero.MainHero.CurrentSettlement));
	}

	private bool gang_leader_bodyguard_on_condition()
	{
		if (Settlement.CurrentSettlement != null)
		{
			return CharacterObject.OneToOneConversationCharacter == Settlement.CurrentSettlement.Culture.GangleaderBodyguard;
		}
		return false;
	}

	private void OnHeroKilled(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => x.AssignedClanMember == victim);
		if (playerAlleyData != null)
		{
			playerAlleyData.TroopRoster.RemoveTroop(victim.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
			Campaign.Current.CampaignInformationManager.NewMapNoticeAdded((InformationData)new AlleyLeaderDiedMapNotification(playerAlleyData.Alley, new TextObject("{=EAPYyktd}One of your alleys has lost its leader or is lacking troops", (Dictionary<string, object>)null)));
		}
		foreach (PlayerAlleyData playerOwnedCommonAreaDatum in _playerOwnedCommonAreaData)
		{
			if (playerOwnedCommonAreaDatum.IsUnderAttack && ((SettlementArea)playerOwnedCommonAreaDatum.UnderAttackBy).Owner == victim)
			{
				playerOwnedCommonAreaDatum.UnderAttackBy = null;
			}
		}
		if (victim.Clan == Clan.PlayerClan)
		{
			return;
		}
		foreach (Alley item in victim.OwnedAlleys.ToList())
		{
			item.SetOwner((Hero)null);
		}
	}

	public bool GetIsPlayerAlleyUnderAttack(Alley alley)
	{
		return _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => x.Alley == alley)?.IsUnderAttack ?? false;
	}

	public int GetPlayerOwnedAlleyTroopCount(Alley alley)
	{
		return _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => x.Alley == alley).TroopRoster.TotalRegulars;
	}

	public int GetResponseTimeLeftForAttackInDays(Alley alley)
	{
		return (int)((CampaignTime)(ref _playerOwnedCommonAreaData.First((PlayerAlleyData x) => x.Alley == alley).AttackResponseDueDate)).RemainingDaysFromNow;
	}

	public void AbandonAlleyFromClanMenu(Alley alley)
	{
		PlayerAlleyData playerAlleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => x.Alley == alley);
		_playerOwnedCommonAreaData.Remove(playerAlleyData);
		playerAlleyData?.AbandonTheAlley(fromClanScreen: true);
	}

	public bool IsHeroAlleyLeaderOfAnyPlayerAlley(Hero hero)
	{
		return _playerOwnedCommonAreaData.Any((PlayerAlleyData x) => x.AssignedClanMember == hero);
	}

	public List<Hero> GetAllAssignedClanMembersForOwnedAlleys()
	{
		return _playerOwnedCommonAreaData.Select((PlayerAlleyData x) => x.AssignedClanMember).ToList();
	}

	public void ChangeAlleyMember(Alley alley, Hero newAlleyLead)
	{
		PlayerAlleyData alleyData = _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => x.Alley == alley);
		ChangeTheLeaderOfAlleyInternal(alleyData, newAlleyLead);
	}

	public void OnPlayerRetreatedFromMission()
	{
		_playerRetreatedFromMission = true;
	}

	public void OnPlayerDiedInMission()
	{
		_playerDiedInMission = true;
	}

	public Hero GetAssignedClanMemberOfAlley(Alley alley)
	{
		return _playerOwnedCommonAreaData.FirstOrDefault((PlayerAlleyData x) => x.Alley == alley)?.AssignedClanMember;
	}

	private void ChangeTheLeaderOfAlleyInternal(PlayerAlleyData alleyData, Hero newLeader)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Hero assignedClanMember = alleyData.AssignedClanMember;
		alleyData.AssignedClanMember = newLeader;
		if (!assignedClanMember.IsDead)
		{
			alleyData.TroopRoster.RemoveTroop(assignedClanMember.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
		}
		alleyData.TroopRoster.AddToCounts(newLeader.CharacterObject, 1, true, 0, 0, true, -1);
		TeleportHeroAction.ApplyDelayedTeleportToSettlement(newLeader, ((SettlementArea)alleyData.Alley).Settlement);
		if (Settlement.CurrentSettlement == assignedClanMember.CurrentSettlement)
		{
			LocationCharacter locationCharacterOfHero = Settlement.CurrentSettlement.LocationComplex.GetLocationCharacterOfHero(assignedClanMember);
			Settlement.CurrentSettlement.LocationComplex.GetLocationOfCharacter(locationCharacterOfHero);
			Settlement.CurrentSettlement.LocationComplex.ChangeLocation(locationCharacterOfHero, Settlement.CurrentSettlement.LocationComplex.GetLocationOfCharacter(locationCharacterOfHero), Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern"));
			if (Campaign.Current.CurrentMenuContext != null)
			{
				Campaign.Current.CurrentMenuContext.Refresh();
			}
		}
	}

	[GameMenuInitializationHandler("manage_alley")]
	[GameMenuInitializationHandler("alley_fight_lost")]
	[GameMenuInitializationHandler("alley_fight_won")]
	[GameMenuInitializationHandler("manage_alley_abandon_are_you_sure")]
	public static void alley_related_menu_on_init(MenuCallbackArgs args)
	{
		string backgroundMeshName = ((MBObjectBase)Settlement.CurrentSettlement.Culture).StringId + "_alley";
		args.MenuContext.SetBackgroundMeshName(backgroundMeshName);
	}
}
