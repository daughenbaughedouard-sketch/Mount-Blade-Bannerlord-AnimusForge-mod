using System;
using System.Collections.Generic;
using SandBox.Conversation;
using SandBox.GameComponents;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.CampaignBehaviors;

public class ClanMemberRolesCampaignBehavior : CampaignBehaviorBase, IMissionPlayerFollowerHandler
{
	private List<Hero> _isFollowingPlayer = new List<Hero>();

	private Agent _gatherOrderedAgent;

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)AddDialogs);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
		CampaignEvents.NewCompanionAdded.AddNonSerializedListener((object)this, (Action<Hero>)OnNewCompanionAdded);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.BeforeMissionOpenedEvent.AddNonSerializedListener((object)this, (Action)BeforeMissionOpened);
		CampaignEvents.OnHeroJoinedPartyEvent.AddNonSerializedListener((object)this, (Action<Hero, MobileParty>)OnHeroJoinedParty);
		CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener((object)this, (Action<IMission>)OnMissionEnded);
		CampaignEvents.OnHeroGetsBusyEvent.AddNonSerializedListener((object)this, (Action<Hero, HeroGetsBusyReasons>)OnHeroGetsBusy);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<List<Hero>>("_isFollowingPlayer", ref _isFollowingPlayer);
	}

	private static void FollowMainAgent()
	{
		DailyBehaviorGroup behaviorGroup = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		FollowAgentBehavior followAgentBehavior = behaviorGroup.AddBehavior<FollowAgentBehavior>();
		behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
		followAgentBehavior.SetTargetAgent(Agent.Main);
	}

	public bool IsFollowingPlayer(Hero hero)
	{
		return _isFollowingPlayer.Contains(hero);
	}

	private void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected O, but got Unknown
		campaignGameStarter.AddPlayerLine("clan_member_follow", "hero_main_options", "clan_member_follow_me", "{=blqTMwQT}Follow me.", new OnConditionDelegate(clan_member_follow_me_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("clan_member_dont_follow", "hero_main_options", "clan_member_dont_follow_me", "{=LPtWLajd}You can stop following me now. Thanks.", new OnConditionDelegate(clan_member_dont_follow_me_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("clan_members_follow", "hero_main_options", "clan_member_gather", "{=PUtbpIFI}Gather all my companions in the settlement and find me.", new OnConditionDelegate(clan_members_gather_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("clan_members_dont_follow", "hero_main_options", "clan_members_dont_follow_me", "{=FdwZlCCM}All of you can stop following me and return to what you were doing.", new OnConditionDelegate(clan_members_gather_end_on_condition), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("clan_member_gather_clan_members_accept", "clan_member_gather", "close_window", "{=KL8tVq8P}I shall do that.", (OnConditionDelegate)null, new OnConsequenceDelegate(clan_member_gather_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("clan_member_follow_accept", "clan_member_follow_me", "close_window", "{=gm3wqjvi}Lead the way.", (OnConditionDelegate)null, new OnConsequenceDelegate(clan_member_follow_me_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("clan_member_dont_follow_accept", "clan_member_dont_follow_me", "close_window", "{=ppi6eVos}As you wish.", (OnConditionDelegate)null, new OnConsequenceDelegate(clan_member_dont_follow_me_on_consequence), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("clan_members_dont_follow_accept", "clan_members_dont_follow_me", "close_window", "{=ppi6eVos}As you wish.", (OnConditionDelegate)null, new OnConsequenceDelegate(clan_members_dont_follow_me_on_consequence), 100, (OnClickableConditionDelegate)null);
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (party == MobileParty.MainParty && PlayerEncounter.LocationEncounter != null)
		{
			PlayerEncounter.LocationEncounter.RemoveAllAccompanyingCharacters();
			_isFollowingPlayer.Clear();
		}
	}

	private void BeforeMissionOpened()
	{
		if (PlayerEncounter.LocationEncounter == null)
		{
			return;
		}
		foreach (Hero item in _isFollowingPlayer)
		{
			if (PlayerEncounter.LocationEncounter.GetAccompanyingCharacter(item.CharacterObject) == null)
			{
				AddClanMembersAsAccompanyingCharacter(item);
			}
		}
	}

	private void OnHeroJoinedParty(Hero hero, MobileParty mobileParty)
	{
		if (hero.Clan == Clan.PlayerClan && mobileParty.IsMainParty && mobileParty.CurrentSettlement != null && PlayerEncounter.LocationEncounter != null && MobileParty.MainParty.IsActive && (mobileParty.CurrentSettlement.IsFortification || mobileParty.CurrentSettlement.IsVillage) && _isFollowingPlayer.Count == 0)
		{
			UpdateAccompanyingCharacters();
		}
	}

	public void RemoveFollowingHero(Hero hero)
	{
		if (_isFollowingPlayer.Contains(hero))
		{
			RemoveAccompanyingHero(hero);
		}
	}

	private void OnMissionEnded(IMission mission)
	{
		_gatherOrderedAgent = null;
	}

	private void OnHeroGetsBusy(Hero hero, HeroGetsBusyReasons heroGetsBusyReason)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Invalid comparison between Unknown and I4
		if ((int)heroGetsBusyReason != 2 && (int)heroGetsBusyReason != 3 && (int)heroGetsBusyReason != 4 && (int)heroGetsBusyReason != 5)
		{
			return;
		}
		if (Mission.Current != null)
		{
			for (int i = 0; i < ((List<Agent>)(object)Mission.Current.Agents).Count; i++)
			{
				Agent val = ((List<Agent>)(object)Mission.Current.Agents)[i];
				if (val.IsHuman && val.Character.IsHero && ((CharacterObject)val.Character).HeroObject == hero)
				{
					ClearGatherOrderedAgentIfExists(val);
					if ((int)heroGetsBusyReason == 3)
					{
						AdjustTheBehaviorsOfTheAgent(val);
					}
					break;
				}
			}
		}
		if (PlayerEncounter.LocationEncounter != null)
		{
			RemoveAccompanyingHero(hero);
			if (_isFollowingPlayer.Count == 0)
			{
				UpdateAccompanyingCharacters();
			}
		}
	}

	private void ClearGatherOrderedAgentIfExists(Agent agent)
	{
		if (_gatherOrderedAgent == agent)
		{
			_gatherOrderedAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>().RemoveBehavior<ScriptBehavior>();
			_gatherOrderedAgent = null;
		}
	}

	private void OnNewCompanionAdded(Hero newCompanion)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		Location val = null;
		LocationComplex current = LocationComplex.Current;
		if (current != null)
		{
			foreach (Location listOfLocation in current.GetListOfLocations())
			{
				foreach (LocationCharacter character in listOfLocation.GetCharacterList())
				{
					if (character.Character == newCompanion.CharacterObject)
					{
						val = LocationComplex.Current.GetLocationOfCharacter(character);
						break;
					}
				}
			}
		}
		if (((current != null) ? current.GetLocationWithId("center") : null) != null && val == null)
		{
			AgentData val2 = new AgentData((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, newCompanion.CharacterObject, -1, default(UniqueTroopDescriptor), false, false)).Monster(FaceGen.GetBaseMonsterFromRace(((BasicCharacterObject)newCompanion.CharacterObject).Race)).NoHorses(true);
			Location locationWithId = current.GetLocationWithId("center");
			IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
			locationWithId.AddCharacter(new LocationCharacter(val2, new AddBehaviorsDelegate(agentBehaviorManager.AddFixedCharacterBehaviors), (string)null, true, (CharacterRelations)1, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false));
		}
	}

	private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Campaign.Current.GameMode != 1 || MobileParty.MainParty.CurrentSettlement == null || LocationComplex.Current == null || (!settlement.IsTown && !settlement.IsCastle && !settlement.IsVillage))
		{
			return;
		}
		if (mobileParty == null && settlement == MobileParty.MainParty.CurrentSettlement && hero.Clan == Clan.PlayerClan)
		{
			if (_isFollowingPlayer.Contains(hero) && hero.PartyBelongedTo == null)
			{
				RemoveAccompanyingHero(hero);
				if (_isFollowingPlayer.Count == 0)
				{
					UpdateAccompanyingCharacters();
				}
			}
		}
		else if (mobileParty == MobileParty.MainParty && MobileParty.MainParty.IsActive)
		{
			UpdateAccompanyingCharacters();
		}
	}

	private bool clan_member_follow_me_on_condition()
	{
		if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.LocationComplex != null && !Settlement.CurrentSettlement.IsHideout)
		{
			Location val = (Settlement.CurrentSettlement.IsVillage ? Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("village_center") : Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center"));
			if (Hero.OneToOneConversationHero != null && ConversationMission.OneToOneConversationAgent != null && Hero.OneToOneConversationHero.Clan == Clan.PlayerClan && Hero.OneToOneConversationHero.PartyBelongedTo == MobileParty.MainParty)
			{
				ICampaignMission current = CampaignMission.Current;
				if (((current != null) ? current.Location : null) == val && ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
				{
					return !(ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetActiveBehavior() is FollowAgentBehavior);
				}
			}
			return false;
		}
		return false;
	}

	private bool clan_member_dont_follow_me_on_condition()
	{
		if (ConversationMission.OneToOneConversationAgent != null && Hero.OneToOneConversationHero.Clan == Clan.PlayerClan && Hero.OneToOneConversationHero.PartyBelongedTo == MobileParty.MainParty && ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
		{
			return ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetActiveBehavior() is FollowAgentBehavior;
		}
		return false;
	}

	private bool clan_members_gather_on_condition()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		if (GameStateManager.Current.ActiveState is MissionState)
		{
			if (_gatherOrderedAgent != null || Settlement.CurrentSettlement == null)
			{
				return false;
			}
			InterruptingBehaviorGroup interruptingBehaviorGroup = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator?.GetBehaviorGroup<InterruptingBehaviorGroup>();
			if (interruptingBehaviorGroup != null && interruptingBehaviorGroup.IsActive)
			{
				return false;
			}
			Agent oneToOneConversationAgent = ConversationMission.OneToOneConversationAgent;
			CharacterObject oneToOneConversationCharacter = ConversationMission.OneToOneConversationCharacter;
			if (!((BasicCharacterObject)oneToOneConversationCharacter).IsHero || oneToOneConversationCharacter.HeroObject.Clan != Hero.MainHero.Clan)
			{
				return false;
			}
			foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
			{
				CharacterObject val = (CharacterObject)item.Character;
				if (item.IsHuman && item != oneToOneConversationAgent && item != Agent.Main && ((BasicCharacterObject)val).IsHero && val.HeroObject.Clan == Clan.PlayerClan && val.HeroObject.PartyBelongedTo == MobileParty.MainParty)
				{
					AgentNavigator agentNavigator = item.GetComponent<CampaignAgentComponent>().AgentNavigator;
					if (agentNavigator != null && !(agentNavigator.GetActiveBehavior() is FollowAgentBehavior))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool clan_members_gather_end_on_condition()
	{
		if (ConversationMission.OneToOneConversationAgent != null && _gatherOrderedAgent == ConversationMission.OneToOneConversationAgent)
		{
			if (ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<InterruptingBehaviorGroup>().IsActive)
			{
				return false;
			}
			return true;
		}
		if (!IsAgentFollowingPlayerAsCompanion(ConversationMission.OneToOneConversationAgent))
		{
			return false;
		}
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (item != ConversationMission.OneToOneConversationAgent && IsAgentFollowingPlayerAsCompanion(item))
			{
				return true;
			}
		}
		return false;
	}

	private void clan_member_gather_on_consequence()
	{
		_gatherOrderedAgent = ConversationMission.OneToOneConversationAgent;
		_gatherOrderedAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>().AddBehavior<ScriptBehavior>().IsActive = true;
		ScriptBehavior.AddTargetWithDelegate(_gatherOrderedAgent, SelectTarget, null, OnTargetReached);
		_gatherOrderedAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>().AddBehavior<FollowAgentBehavior>().IsActive = false;
	}

	private void clan_member_dont_follow_me_on_consequence()
	{
		RemoveFollowBehavior(ConversationMission.OneToOneConversationAgent);
	}

	private void clan_members_dont_follow_me_on_consequence()
	{
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			RemoveFollowBehavior(item);
		}
	}

	private void RemoveFollowBehavior(Agent agent)
	{
		ClearGatherOrderedAgentIfExists(agent);
		if (IsAgentFollowingPlayerAsCompanion(agent))
		{
			AdjustTheBehaviorsOfTheAgent(agent);
			LocationCharacter val = LocationComplex.Current.FindCharacter((IAgent)(object)agent);
			RemoveAccompanyingHero(val.Character.HeroObject);
		}
	}

	private void AdjustTheBehaviorsOfTheAgent(Agent agent)
	{
		DailyBehaviorGroup behaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		behaviorGroup.RemoveBehavior<FollowAgentBehavior>();
		ScriptBehavior behavior = behaviorGroup.GetBehavior<ScriptBehavior>();
		if (behavior != null)
		{
			behavior.IsActive = true;
		}
		WalkingBehavior walkingBehavior = behaviorGroup.GetBehavior<WalkingBehavior>();
		if (walkingBehavior == null)
		{
			walkingBehavior = behaviorGroup.AddBehavior<WalkingBehavior>();
		}
		walkingBehavior.IsActive = true;
	}

	private void clan_member_follow_me_on_consequence()
	{
		LocationCharacter locationCharacterOfHero = LocationComplex.Current.GetLocationCharacterOfHero(Hero.OneToOneConversationHero);
		if (!IsFollowingPlayer(locationCharacterOfHero.Character.HeroObject))
		{
			_isFollowingPlayer.Add(locationCharacterOfHero.Character.HeroObject);
		}
		AddClanMembersAsAccompanyingCharacter(locationCharacterOfHero.Character.HeroObject, locationCharacterOfHero);
		Campaign.Current.ConversationManager.ConversationEndOneShot += FollowMainAgent;
	}

	private bool SelectTarget(Agent agent, ref Agent targetAgent, ref UsableMachine targetEntity, ref WorldFrame targetFrame, ref float customTargetReachedRangeThreshold, ref float customTargetReachedRotationThreshold)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (Agent.Main == null)
		{
			return false;
		}
		Agent val = null;
		float num = float.MaxValue;
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			CharacterObject val2 = (CharacterObject)item.Character;
			CampaignAgentComponent component = item.GetComponent<CampaignAgentComponent>();
			if (item == agent || !item.IsHuman || !((BasicCharacterObject)val2).IsHero || val2.HeroObject.Clan != Clan.PlayerClan || val2.HeroObject.PartyBelongedTo != MobileParty.MainParty || component.AgentNavigator == null)
			{
				continue;
			}
			AgentBehavior behavior = item.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehavior<FollowAgentBehavior>();
			if (behavior == null || !behavior.IsActive)
			{
				Vec3 position = agent.Position;
				float num2 = ((Vec3)(ref position)).DistanceSquared(item.Position);
				if (num2 < num)
				{
					num = num2;
					val = item;
				}
			}
		}
		if (val != null)
		{
			targetAgent = val;
			return true;
		}
		DailyBehaviorGroup behaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		FollowAgentBehavior behavior2 = behaviorGroup.GetBehavior<FollowAgentBehavior>();
		behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
		behavior2.IsActive = true;
		behavior2.SetTargetAgent(Agent.Main);
		ScriptBehavior behavior3 = behaviorGroup.GetBehavior<ScriptBehavior>();
		if (behavior3 != null)
		{
			behavior3.IsActive = false;
		}
		WalkingBehavior behavior4 = behaviorGroup.GetBehavior<WalkingBehavior>();
		if (behavior4 != null)
		{
			behavior4.IsActive = false;
		}
		LocationCharacter val3 = LocationComplex.Current.FindCharacter((IAgent)(object)agent);
		if (!IsFollowingPlayer(val3.Character.HeroObject))
		{
			_isFollowingPlayer.Add(val3.Character.HeroObject);
		}
		AddClanMembersAsAccompanyingCharacter(val3.Character.HeroObject, val3);
		_gatherOrderedAgent = null;
		return false;
	}

	private bool OnTargetReached(Agent agent, ref Agent targetAgent, ref UsableMachine targetEntity, ref WorldFrame targetFrame)
	{
		if (Agent.Main == null)
		{
			return false;
		}
		if (targetAgent == null)
		{
			return true;
		}
		LocationCharacter val = LocationComplex.Current.FindCharacter((IAgent)(object)targetAgent);
		if (val == null)
		{
			return true;
		}
		DailyBehaviorGroup behaviorGroup = targetAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		FollowAgentBehavior followAgentBehavior = behaviorGroup.AddBehavior<FollowAgentBehavior>();
		behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
		followAgentBehavior.SetTargetAgent(Agent.Main);
		if (!IsFollowingPlayer(val.Character.HeroObject))
		{
			_isFollowingPlayer.Add(val.Character.HeroObject);
			AddClanMembersAsAccompanyingCharacter(val.Character.HeroObject, val);
		}
		targetAgent = null;
		return true;
	}

	private void UpdateAccompanyingCharacters()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		_isFollowingPlayer.Clear();
		PlayerEncounter.LocationEncounter.RemoveAllAccompanyingCharacters();
		bool flag = false;
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)MobileParty.MainParty.MemberRoster.GetTroopRoster())
		{
			if (((BasicCharacterObject)item.Character).IsHero)
			{
				Hero heroObject = item.Character.HeroObject;
				if (heroObject != Hero.MainHero && !heroObject.IsPrisoner && !heroObject.IsWounded && heroObject.Age >= (float)Campaign.Current.Models.AgeModel.HeroComesOfAge && !flag)
				{
					_isFollowingPlayer.Add(heroObject);
					flag = true;
				}
			}
		}
	}

	private void RemoveAccompanyingHero(Hero hero)
	{
		_isFollowingPlayer.Remove(hero);
		LocationEncounter locationEncounter = PlayerEncounter.LocationEncounter;
		if (locationEncounter != null)
		{
			locationEncounter.RemoveAccompanyingCharacter(hero);
		}
	}

	private bool IsAgentFollowingPlayerAsCompanion(Agent agent)
	{
		BasicCharacterObject obj = ((agent != null) ? agent.Character : null);
		CharacterObject val = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		CampaignAgentComponent campaignAgentComponent = ((agent != null) ? agent.GetComponent<CampaignAgentComponent>() : null);
		if (agent != null && agent.IsHuman && val != null && ((BasicCharacterObject)val).IsHero && val.HeroObject.Clan == Clan.PlayerClan && val.HeroObject.PartyBelongedTo == MobileParty.MainParty)
		{
			return campaignAgentComponent.AgentNavigator?.GetActiveBehavior() is FollowAgentBehavior;
		}
		return false;
	}

	private void AddClanMembersAsAccompanyingCharacter(Hero member, LocationCharacter locationCharacter = null)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		CharacterObject characterObject = member.CharacterObject;
		if (((BasicCharacterObject)characterObject).IsHero && !characterObject.HeroObject.IsWounded && IsFollowingPlayer(member))
		{
			LocationCharacter obj = locationCharacter;
			if (obj == null)
			{
				Hero heroObject = characterObject.HeroObject;
				MobileParty mainParty = MobileParty.MainParty;
				IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
				obj = LocationCharacter.CreateBodyguardHero(heroObject, mainParty, new AddBehaviorsDelegate(agentBehaviorManager.AddFirstCompanionBehavior));
			}
			LocationCharacter val = obj;
			PlayerEncounter.LocationEncounter.AddAccompanyingCharacter(val, true);
			AccompanyingCharacter accompanyingCharacter = PlayerEncounter.LocationEncounter.GetAccompanyingCharacter(characterObject);
			accompanyingCharacter.DisallowEntranceToAllLocations();
			accompanyingCharacter.AllowEntranceToLocations((Func<Location, bool>)((Location x) => x == LocationComplex.Current.GetLocationWithId("center") || x == LocationComplex.Current.GetLocationWithId("village_center") || x == LocationComplex.Current.GetLocationWithId("tavern")));
		}
	}
}
