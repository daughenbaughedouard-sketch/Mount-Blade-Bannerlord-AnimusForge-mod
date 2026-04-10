using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.Conversation.MissionLogics;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class MissionAlleyHandler : MissionLogic
{
	private const float ConstantForInitiatingConversation = 5f;

	private static Vec3 _fightPosition = Vec3.Invalid;

	private Dictionary<Agent, AgentNavigator> _rivalThugAgentsAndAgentNavigators;

	private const int DistanceForEndingAlleyFight = 20;

	private const int GuardAgentSafeZone = 10;

	private static List<Agent> _guardAgents;

	private Dictionary<Alley, bool> _conversationTriggeredAlleys;

	private bool _agentCachesInitialized;

	private MissionFightHandler _missionFightHandler;

	private DisguiseMissionLogic _disguiseMissionLogic;

	public bool CanThugConversationBeTriggered
	{
		get
		{
			if (_disguiseMissionLogic != null)
			{
				return _disguiseMissionLogic.CanCommonAreaFightBeTriggered();
			}
			return true;
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Invalid comparison between Unknown and I4
		if (!_agentCachesInitialized)
		{
			_conversationTriggeredAlleys = new Dictionary<Alley, bool>();
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				if (!item.IsHuman)
				{
					continue;
				}
				CampaignAgentComponent component = item.GetComponent<CampaignAgentComponent>();
				if (component?.AgentNavigator?.MemberOfAlley != null && ((SettlementArea)component.AgentNavigator.MemberOfAlley).Owner != Hero.MainHero)
				{
					if (!_rivalThugAgentsAndAgentNavigators.ContainsKey(item))
					{
						_rivalThugAgentsAndAgentNavigators.Add(item, component.AgentNavigator);
					}
					if (!_conversationTriggeredAlleys.ContainsKey(component.AgentNavigator.MemberOfAlley))
					{
						_conversationTriggeredAlleys.Add(component.AgentNavigator.MemberOfAlley, value: false);
					}
				}
			}
			_agentCachesInitialized = ((List<Agent>)(object)((MissionBehavior)this).Mission.Agents).Count > 0;
		}
		if ((int)Mission.Current.Mode == 2)
		{
			EndFightIfPlayerIsFarAwayOrNearGuard();
		}
		else if (MBRandom.RandomFloat < dt * 10f && CanThugConversationBeTriggered)
		{
			CheckAndTriggerConversationWithRivalThug();
		}
	}

	private void CheckAndTriggerConversationWithRivalThug()
	{
		if (Campaign.Current.ConversationManager.IsConversationFlowActive || Agent.Main == null)
		{
			return;
		}
		foreach (KeyValuePair<Agent, AgentNavigator> rivalThugAgentsAndAgentNavigator in _rivalThugAgentsAndAgentNavigators)
		{
			if (rivalThugAgentsAndAgentNavigator.Key.IsActive() && _conversationTriggeredAlleys.TryGetValue(rivalThugAgentsAndAgentNavigator.Value.MemberOfAlley, out var value) && !value)
			{
				Agent key = rivalThugAgentsAndAgentNavigator.Key;
				if (key.GetDistanceTo(Agent.Main) < 5f && rivalThugAgentsAndAgentNavigator.Value.CanSeeAgent(Agent.Main))
				{
					Mission.Current.GetMissionBehavior<MissionConversationLogic>().StartConversation(key, setActionsInstantly: false);
					_conversationTriggeredAlleys[rivalThugAgentsAndAgentNavigator.Value.MemberOfAlley] = true;
					break;
				}
			}
		}
	}

	public override void AfterStart()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		_disguiseMissionLogic = Mission.Current.GetMissionBehavior<DisguiseMissionLogic>();
		_guardAgents = new List<Agent>();
		_rivalThugAgentsAndAgentNavigators = new Dictionary<Agent, AgentNavigator>();
		_fightPosition = Vec3.Invalid;
		_missionFightHandler = Mission.Current.GetMissionBehavior<MissionFightHandler>();
	}

	private void EndFightIfPlayerIsFarAwayOrNearGuard()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (Agent.Main == null)
		{
			return;
		}
		bool flag = false;
		Vec3 val;
		foreach (Agent guardAgent in _guardAgents)
		{
			val = Agent.Main.Position - guardAgent.Position;
			if (((Vec3)(ref val)).Length <= 10f)
			{
				flag = true;
				break;
			}
		}
		if (_fightPosition != Vec3.Invalid)
		{
			val = Agent.Main.Position - _fightPosition;
			if (((Vec3)(ref val)).Length >= 20f)
			{
				flag = true;
			}
		}
		if (flag)
		{
			EndFight();
		}
	}

	private (bool, string) CanPlayerOccupyTheCurrentAlley()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		if (!Settlement.CurrentSettlement.Alleys.All((Alley x) => ((SettlementArea)x).Owner != Hero.MainHero))
		{
			TextObject val = new TextObject("{=ribkM9dl}You already own another alley in the settlement.", (Dictionary<string, object>)null);
			return (false, ((object)val).ToString());
		}
		if (!Campaign.Current.Models.AlleyModel.GetClanMembersAndAvailabilityDetailsForLeadingAnAlley(CampaignMission.Current.LastVisitedAlley).Any<(Hero, AlleyMemberAvailabilityDetail)>(((Hero, AlleyMemberAvailabilityDetail) x) => (int)x.Item2 == 0 || (int)x.Item2 == 1))
		{
			TextObject val = new TextObject("{=hnhKJYbx}You don't have any suitable clan members to assign this alley. ({ROGUERY_SKILL} skill {NEEDED_SKILL_LEVEL} or higher, {TRAIT_NAME} trait {MAX_TRAIT_AMOUNT} or lower)", (Dictionary<string, object>)null);
			val.SetTextVariable("ROGUERY_SKILL", ((PropertyObject)DefaultSkills.Roguery).Name);
			val.SetTextVariable("NEEDED_SKILL_LEVEL", 30);
			val.SetTextVariable("TRAIT_NAME", ((PropertyObject)DefaultTraits.Mercy).Name);
			val.SetTextVariable("MAX_TRAIT_AMOUNT", 0);
			return (false, ((object)val).ToString());
		}
		if (MobileParty.MainParty.MemberRoster.TotalRegulars < Campaign.Current.Models.AlleyModel.MinimumTroopCountInPlayerOwnedAlley)
		{
			TextObject val = new TextObject("{=zLnqZdIK}You don't have enough troops to assign this alley. (Needed at least {NEEDED_TROOP_NUMBER})", (Dictionary<string, object>)null);
			val.SetTextVariable("NEEDED_TROOP_NUMBER", Campaign.Current.Models.AlleyModel.MinimumTroopCountInPlayerOwnedAlley);
			return (false, ((object)val).ToString());
		}
		return (true, null);
	}

	private void EndFight()
	{
		_missionFightHandler.EndFight();
		foreach (Agent guardAgent in _guardAgents)
		{
			guardAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>().GetBehavior<FightBehavior>().IsActive = false;
		}
		_guardAgents.Clear();
		Mission.Current.SetMissionMode((MissionMode)0, false);
	}

	private void OnTakeOverTheAlley()
	{
		AlleyHelper.CreateMultiSelectionInquiryForSelectingClanMemberToAlley(CampaignMission.Current.LastVisitedAlley, (Action<List<InquiryElement>>)OnCompanionSelectedForNewAlley, (Action<List<InquiryElement>>)OnCompanionSelectionCancel);
	}

	private void OnCompanionSelectionCancel(List<InquiryElement> obj)
	{
		OnLeaveItEmpty();
	}

	private void OnCompanionSelectedForNewAlley(List<InquiryElement> companion)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_004f: Expected O, but got Unknown
		//IL_004f: Expected O, but got Unknown
		object identifier = companion.First().Identifier;
		CharacterObject val = (CharacterObject)((identifier is CharacterObject) ? identifier : null);
		TroopRoster val2 = TroopRoster.CreateDummyTroopRoster();
		val2.AddToCounts(val, 1, false, 0, 0, true, -1);
		AlleyHelper.OpenScreenForManagingAlley(true, val2, new PartyPresentationDoneButtonDelegate(OnPartyScreenDoneClicked), new TextObject("{=s8dsW6m0}New Alley", (Dictionary<string, object>)null), new PartyPresentationCancelButtonDelegate(OnPartyScreenCancel));
	}

	private void OnPartyScreenCancel()
	{
		OnLeaveItEmpty();
	}

	public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		if (affectedAgent.IsHuman && affectorAgent != null && affectorAgent == Agent.Main && affectorAgent.IsHuman && affectedAgent.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
		{
			(affectedAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<InterruptingBehaviorGroup>()?.GetBehavior<TalkBehavior>())?.Disable();
			if (!affectedAgent.IsEnemyOf(affectorAgent) && affectedAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.MemberOfAlley != null)
			{
				StartCommonAreaBattle(affectedAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.MemberOfAlley);
			}
		}
	}

	private bool OnPartyScreenDoneClicked(TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		TeleportHeroAction.ApplyDelayedTeleportToSettlement(((List<TroopRosterElement>)(object)leftMemberRoster.GetTroopRoster()).Find((Predicate<TroopRosterElement>)((TroopRosterElement x) => ((BasicCharacterObject)x.Character).IsHero)).Character.HeroObject, MobileParty.MainParty.CurrentSettlement);
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)leftMemberRoster.GetTroopRoster())
		{
			TroopRosterElement current = item;
			if (!((BasicCharacterObject)current.Character).IsHero)
			{
				MobileParty.MainParty.MemberRoster.RemoveTroop(current.Character, ((TroopRosterElement)(ref current)).Number, default(UniqueTroopDescriptor), 0);
			}
		}
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnAlleyOccupiedByPlayer(CampaignMission.Current.LastVisitedAlley, leftMemberRoster);
		return true;
	}

	public void StartCommonAreaBattle(Alley alley)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Invalid comparison between Unknown and I4
		_guardAgents.Clear();
		_conversationTriggeredAlleys[alley] = true;
		List<Agent> accompanyingAgents = new List<Agent>();
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			LocationCharacter val = LocationComplex.Current.FindCharacter((IAgent)(object)item);
			AccompanyingCharacter accompanyingCharacter = PlayerEncounter.LocationEncounter.GetAccompanyingCharacter(val);
			CharacterObject val2 = (CharacterObject)item.Character;
			if (accompanyingCharacter != null && accompanyingCharacter.IsFollowingPlayerAtMissionStart)
			{
				accompanyingAgents.Add(item);
			}
			else if (val2 != null && ((int)val2.Occupation == 24 || (int)val2.Occupation == 7))
			{
				_guardAgents.Add(item);
			}
		}
		List<Agent> playerSideAgents = ((IEnumerable<Agent>)Mission.Current.Agents).Where((Agent agent) => agent.IsHuman && agent.Character.IsHero && (agent.IsPlayerControlled || accompanyingAgents.Contains(agent))).ToList();
		List<Agent> opponentSideAgents = ((IEnumerable<Agent>)Mission.Current.Agents).Where((Agent agent) => agent.IsHuman && agent.GetComponent<CampaignAgentComponent>().AgentNavigator != null && agent.GetComponent<CampaignAgentComponent>().AgentNavigator.MemberOfAlley == alley).ToList();
		_fightPosition = Agent.Main.Position;
		Mission.Current.GetMissionBehavior<MissionFightHandler>().StartCustomFight(playerSideAgents, opponentSideAgents, dropWeapons: false, isItemUseDisabled: false, OnAlleyFightEnd);
	}

	private void OnLeaveItEmpty()
	{
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnAlleyClearedByPlayer(CampaignMission.Current.LastVisitedAlley);
	}

	private void OnAlleyFightEnd(bool isPlayerSideWon)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_003a: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (isPlayerSideWon)
		{
			TextObject val = new TextObject("{=4QfQBi2k}Alley fight won", (Dictionary<string, object>)null);
			TextObject val2 = new TextObject("{=8SK2BZum}You have cleared an alley which belonged to a gang leader. Now, you can either take it over for your own benefit or leave it empty to help the town. To own an alley, you will need to assign a suitable clan member and some troops to watch over it. This will provide denars to your clan, but also increase your crime rating.", (Dictionary<string, object>)null);
			TextObject val3 = new TextObject("{=qxY2ASqp}Take over the alley", (Dictionary<string, object>)null);
			TextObject val4 = new TextObject("{=jjEzdO0Y}Leave it empty", (Dictionary<string, object>)null);
			InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, true, ((object)val3).ToString(), ((object)val4).ToString(), (Action)OnTakeOverTheAlley, (Action)OnLeaveItEmpty, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)CanPlayerOccupyTheCurrentAlley, (Func<ValueTuple<bool, string>>)null), true, false);
		}
		else if (Agent.Main == null || !Agent.Main.IsActive())
		{
			Mission.Current.NextCheckTimeEndMission = 0f;
			if (!Campaign.Current.IsMainHeroDisguised)
			{
				Campaign.Current.GameMenuManager.SetNextMenu("settlement_player_unconscious");
			}
		}
		_fightPosition = Vec3.Invalid;
	}
}
