using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.CampaignBehaviors;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics.Towns;

public class PrisonBreakMissionController : MissionLogic
{
	private const int PrisonerNearThreshold = 5;

	private const int PrisonerSwitchToAlarmedDistance = 3;

	private bool _isFirstPhase;

	private List<CharacterObject> _killedGuardsInTheFirstPhase;

	private readonly CharacterObject _prisonerCharacter;

	private Agent _prisonerAgent;

	private List<Agent> _aliveGuardAgents;

	private PrisonBreakCampaignBehavior _prisonBreakCampaignBehavior;

	private StealthFailCounterMissionLogic _failCounterMissionLogic;

	private bool _isPrisonerFollowing;

	private bool _isPrisonerNear;

	private bool _missionFailedByStealthCounter;

	public PrisonBreakMissionController(CharacterObject prisonerCharacter)
	{
		_prisonerCharacter = prisonerCharacter;
		_isFirstPhase = true;
		_isPrisonerFollowing = false;
		_aliveGuardAgents = new List<Agent>();
		_killedGuardsInTheFirstPhase = new List<CharacterObject>();
		_prisonBreakCampaignBehavior = Campaign.Current.GetCampaignBehavior<PrisonBreakCampaignBehavior>();
	}

	public override void OnCreated()
	{
		((MissionBehavior)this).OnCreated();
		((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment = false;
	}

	public override void OnBehaviorInitialize()
	{
		Game.Current.EventManager.RegisterEvent<OnStealthMissionCounterFailedEvent>((Action<OnStealthMissionCounterFailedEvent>)OnStealthMissionCounterFailed);
		Game.Current.EventManager.RegisterEvent<LocationCharacterAgentSpawnedMissionEvent>((Action<LocationCharacterAgentSpawnedMissionEvent>)OnLocationCharacterAgentSpawned);
		((MissionBehavior)this).Mission.IsAgentInteractionAllowed_AdditionalCondition += IsAgentInteractionAllowed_AdditionalCondition;
	}

	private void OnLocationCharacterAgentSpawned(LocationCharacterAgentSpawnedMissionEvent missionEvent)
	{
		if (missionEvent.LocationCharacter.Character == _prisonerCharacter)
		{
			_prisonerAgent = missionEvent.Agent;
			_prisonerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>().RemoveBehavior<WalkingBehavior>();
		}
	}

	public override void AfterStart()
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)4, true);
		((MissionBehavior)this).Mission.IsInventoryAccessible = false;
		((MissionBehavior)this).Mission.IsQuestScreenAccessible = false;
		((MissionBehavior)this).Mission.IsKingdomWindowAccessible = false;
		foreach (UsableMachine townPassageProp in ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>().TownPassageProps)
		{
			townPassageProp.Deactivate();
		}
		_failCounterMissionLogic = Mission.Current.GetMissionBehavior<StealthFailCounterMissionLogic>();
		_failCounterMissionLogic.FailCounterSeconds = 15f;
		((MissionBehavior)this).Mission.AllowAiTicking = false;
		SandBoxHelpers.MissionHelper.SpawnPlayer(civilianEquipment: false, noHorses: true);
		((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>().SpawnLocationCharacters();
		((MissionBehavior)this).Mission.AllowAiTicking = true;
		Agent.Main.SetClothingColor1(4279111698u);
		Agent.Main.SetClothingColor2(4279111698u);
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(Hero.MainHero.StealthEquipment);
		PreparePrisonAgent();
		Agent.Main.Formation = new Formation(Mission.Current.Teams.Player, 0);
		((MissionBehavior)this).Mission.FocusableObjectInformationProvider.AddInfoCallback(new GetFocusableObjectInteractionTextsDelegate(GetFocusableObjectInteractionInfoTexts));
		TextObject val = new TextObject("{=QYFuj7H7}Find and talk to {PRISONER_NAME}, Do not alert the guards!", (Dictionary<string, object>)null);
		val.SetTextVariable("PRISONER_NAME", ((BasicCharacterObject)_prisonerCharacter).Name);
		MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		_aliveGuardAgents = ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).Where(delegate(Agent x)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Invalid comparison between Unknown and I4
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I4
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Invalid comparison between Unknown and I4
			BasicCharacterObject character = x.Character;
			CharacterObject val2;
			return (val2 = (CharacterObject)(object)((character is CharacterObject) ? character : null)) != null && ((int)val2.Occupation == 7 || (int)val2.Occupation == 24 || (int)val2.Occupation == 23);
		}).ToList();
	}

	private void SwitchPrisonerFollowingState(bool forceFollow = false)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		_isPrisonerFollowing = forceFollow || !_isPrisonerFollowing;
		MBTextManager.SetTextVariable("IS_PRISONER_FOLLOWING", _isPrisonerFollowing ? 1 : 0);
		FollowAgentBehavior behavior = _prisonerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>().GetBehavior<FollowAgentBehavior>();
		if (_isPrisonerFollowing)
		{
			_prisonerAgent.SetCrouchMode(false);
			behavior.SetTargetAgent(Agent.Main);
			AgentFlag agentFlags = _prisonerAgent.GetAgentFlags();
			_prisonerAgent.SetAgentFlags((AgentFlag)(agentFlags & -65537));
		}
		else
		{
			behavior.SetTargetAgent(null);
			_prisonerAgent.SetCrouchMode(true);
		}
		_prisonerAgent.SetAlarmState((AIStateFlag)0);
	}

	private void CheckPrisonerSwitchToAlarmState()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent aliveGuardAgent in _aliveGuardAgents)
		{
			Vec3 position = _prisonerAgent.Position;
			if (((Vec3)(ref position)).DistanceSquared(aliveGuardAgent.Position) < 3f && aliveGuardAgent.IsAlarmed())
			{
				AgentFlag agentFlags = _prisonerAgent.GetAgentFlags();
				_prisonerAgent.SetAgentFlags((AgentFlag)(agentFlags | 0x10000));
				_prisonerAgent.SetAlarmState((AIStateFlag)3);
			}
		}
	}

	public override void OnAgentInteraction(Agent userAgent, Agent agent, sbyte agentBoneIndex)
	{
		if (userAgent == Agent.Main && agent == _prisonerAgent && _aliveGuardAgents.All((Agent x) => !x.IsAlarmed()))
		{
			if (_isFirstPhase)
			{
				SpawnPhase2Guards();
				SwitchToPhase2();
				SwitchPrisonerFollowingState();
			}
			else
			{
				SwitchPrisonerFollowingState();
			}
		}
	}

	private void SpawnPhase2Guards()
	{
		Location locationWithId = LocationComplex.Current.GetLocationWithId("prison");
		foreach (CharacterObject item in _killedGuardsInTheFirstPhase)
		{
			_ = item;
			LocationCharacter val = _prisonBreakCampaignBehavior.CreatePrisonBreakGuard();
			val.SpecialTargetTag = "prison_break_reinforcement_point";
			LocationComplex.Current.ChangeLocation(val, (Location)null, locationWithId);
			_aliveGuardAgents.Add(((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).Last());
		}
	}

	private void SwitchToPhase2()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		_isFirstPhase = false;
		MBInformationManager.AddQuickInformation(new TextObject("{=ap5pYDR7}Let's get out of here!", (Dictionary<string, object>)null), 0, (BasicCharacterObject)(object)_prisonerCharacter, (Equipment)null, "");
		MBInformationManager.AddQuickInformation(new TextObject("{=S3MaaRQH}Guards know that something is up, be ready to fight!", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		_prisonerAgent.SetTeam(Mission.Current.PlayerTeam, true);
		DailyBehaviorGroup behaviorGroup = _prisonerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
		FollowAgentBehavior followAgentBehavior = behaviorGroup.AddBehavior<FollowAgentBehavior>();
		behaviorGroup.SetScriptedBehavior<FollowAgentBehavior>();
		followAgentBehavior.SetTargetAgent(Agent.Main);
		AgentFlag agentFlags = _prisonerAgent.GetAgentFlags();
		_prisonerAgent.SetAgentFlags((AgentFlag)(agentFlags & -65537));
		_prisonerAgent.WieldNextWeapon((HandIndex)0, (WeaponWieldActionType)0);
		foreach (Agent aliveGuardAgent in _aliveGuardAgents)
		{
			aliveGuardAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>().AddAlarmFactor(2f, aliveGuardAgent.GetWorldPosition());
			aliveGuardAgent.SetAlarmState((AIStateFlag)2);
		}
		UpdateDoorPermission();
	}

	public override bool IsThereAgentAction(Agent userAgent, Agent otherAgent)
	{
		if (userAgent == Agent.Main)
		{
			return otherAgent == _prisonerAgent;
		}
		return false;
	}

	private void GetFocusableObjectInteractionInfoTexts(Agent requesterAgent, IFocusable focusableObject, bool isInteractable, out FocusableObjectInformation focusableObjectInformation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		focusableObjectInformation = default(FocusableObjectInformation);
		Agent val;
		if (requesterAgent.IsMainAgent && (val = (Agent)(object)((focusableObject is Agent) ? focusableObject : null)) != null && val == _prisonerAgent)
		{
			focusableObjectInformation.PrimaryInteractionText = val.Character.Name;
			MBTextManager.SetTextVariable("USE_KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f), false);
			focusableObjectInformation.SecondaryInteractionText = GameTexts.FindText("str_key_action", (string)null);
			focusableObjectInformation.SecondaryInteractionText.SetTextVariable("KEY", GameTexts.FindText("str_ui_agent_interaction_use", (string)null));
			focusableObjectInformation.SecondaryInteractionText.SetTextVariable("ACTION", (!_isFirstPhase) ? GameTexts.FindText("str_ui_prison_break", (string)null) : GameTexts.FindText("str_ui_prison_break_prisoner_greeting", (string)null));
			focusableObjectInformation.IsActive = true;
		}
		else
		{
			focusableObjectInformation.IsActive = false;
		}
	}

	private void PreparePrisonAgent()
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		_prisonerAgent.Health = _prisonerAgent.HealthLimit;
		_prisonerAgent.Defensiveness = 2f;
		AgentNavigator agentNavigator = _prisonerAgent.GetComponent<CampaignAgentComponent>().AgentNavigator;
		agentNavigator.RemoveBehaviorGroup<AlarmedBehaviorGroup>();
		agentNavigator.SpecialTargetTag = "sp_prison_break_prisoner";
		ItemObject val = Extensions.MinBy<ItemObject, int>(((IEnumerable<ItemObject>)Items.All).Where((ItemObject x) => x.IsCraftedWeapon && (int)x.Type == 2 && (int)x.WeaponComponent.GetItemType() == 2 && x.IsCivilian), (Func<ItemObject, int>)((ItemObject x) => x.Value));
		MissionWeapon val2 = default(MissionWeapon);
		((MissionWeapon)(ref val2))._002Ector(val, (ItemModifier)null, _prisonerCharacter.HeroObject.ClanBanner);
		_prisonerAgent.EquipWeaponWithNewEntity((EquipmentIndex)0, ref val2);
		_prisonerAgent.SpawnEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)0, new EquipmentElement(((MissionWeapon)(ref val2)).Item, (ItemModifier)null, (ItemObject)null, false));
		_prisonerAgent.SetCrouchMode(true);
		_prisonerAgent.SetTeam((Team)null, false);
	}

	public override void OnAgentAlarmedStateChanged(Agent agent, AIStateFlag flag)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		UpdateDoorPermission();
		if (agent == _prisonerAgent && !_prisonerAgent.IsAlarmed())
		{
			AgentFlag agentFlags = _prisonerAgent.GetAgentFlags();
			_prisonerAgent.SetAgentFlags((AgentFlag)(agentFlags & -65537));
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		if (_prisonerAgent == affectedAgent)
		{
			_prisonerAgent = null;
		}
		if (_aliveGuardAgents.Contains(affectedAgent))
		{
			if (_isFirstPhase)
			{
				_killedGuardsInTheFirstPhase.Add((CharacterObject)affectedAgent.Character);
			}
			_aliveGuardAgents.Remove(affectedAgent);
		}
		UpdateDoorPermission();
	}

	public override InquiryData OnEndMissionRequest(out bool canLeave)
	{
		canLeave = Agent.Main == null || !Agent.Main.IsActive();
		if (!canLeave)
		{
			MBInformationManager.AddQuickInformation(GameTexts.FindText("str_can_not_retreat", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		return null;
	}

	public void OnStealthMissionCounterFailed(OnStealthMissionCounterFailedEvent obj)
	{
		_missionFailedByStealthCounter = true;
	}

	protected override void OnEndMission()
	{
		Game.Current.EventManager.UnregisterEvent<OnStealthMissionCounterFailedEvent>((Action<OnStealthMissionCounterFailedEvent>)OnStealthMissionCounterFailed);
		Game.Current.EventManager.UnregisterEvent<LocationCharacterAgentSpawnedMissionEvent>((Action<LocationCharacterAgentSpawnedMissionEvent>)OnLocationCharacterAgentSpawned);
		if (PlayerEncounter.LocationEncounter.CharactersAccompanyingPlayer.Any((AccompanyingCharacter x) => x.LocationCharacter.Character == _prisonerCharacter))
		{
			PlayerEncounter.LocationEncounter.RemoveAccompanyingCharacter(_prisonerCharacter.HeroObject);
		}
		if (_missionFailedByStealthCounter)
		{
			GameMenu.SwitchToMenu("settlement_prison_break_fail_player_unconscious");
		}
		else if (Agent.Main == null || !Agent.Main.IsActive())
		{
			GameMenu.SwitchToMenu("settlement_prison_break_fail_player_unconscious");
		}
		else if (_prisonerAgent == null || !_prisonerAgent.IsActive())
		{
			GameMenu.SwitchToMenu("settlement_prison_break_fail_prisoner_unconscious");
		}
		else
		{
			GameMenu.SwitchToMenu("settlement_prison_break_success");
		}
		Campaign.Current.GameMenuManager.NextLocation = null;
		Campaign.Current.GameMenuManager.PreviousLocation = null;
		((MissionBehavior)this).Mission.IsAgentInteractionAllowed_AdditionalCondition -= IsAgentInteractionAllowed_AdditionalCondition;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (Agent.Main != null && _prisonerAgent != null)
		{
			bool isPrisonerNear = _isPrisonerNear;
			Vec3 visualPosition = Agent.Main.VisualPosition;
			_isPrisonerNear = ((Vec3)(ref visualPosition)).DistanceSquared(_prisonerAgent.VisualPosition) < 25f;
			if (isPrisonerNear != _isPrisonerNear)
			{
				UpdateDoorPermission();
			}
		}
		if (_failCounterMissionLogic != null && !_isFirstPhase)
		{
			Mission.Current.RemoveMissionBehavior((MissionBehavior)(object)_failCounterMissionLogic);
			_failCounterMissionLogic = null;
		}
		if (_prisonerAgent == null && _aliveGuardAgents.All((Agent x) => x.IsAlarmStateNormal()))
		{
			ShowMissionFailedPopup();
		}
		if (_prisonerAgent != null)
		{
			CheckPrisonerSwitchToAlarmState();
		}
	}

	private void ShowMissionFailedPopup()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_003a: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		//IL_008e: Expected O, but got Unknown
		TextObject val = new TextObject("{=wQbfWNZO}Mission Failed!", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=KfrybSrr}You made your way out but {PRISONER.NAME} was badly wounded during the escape. You had no choice but to leave {?PRISONER.GENDER}her{?}him{\\?} behind.", (Dictionary<string, object>)null);
		TextObjectExtensions.SetCharacterProperties(val2, "PRISONER", _prisonerCharacter, false);
		TextObject val3 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)val3).ToString(), (string)null, (Action)delegate
		{
			Mission.Current.EndMission();
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), (int)Campaign.Current.GameMode == 1, false);
	}

	private void UpdateDoorPermission()
	{
		bool flag = !_isFirstPhase && (_isPrisonerNear || _aliveGuardAgents.Count == 0) && _aliveGuardAgents.All((Agent x) => x.IsAlarmStateNormal());
		foreach (UsableMachine townPassageProp in ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>().TownPassageProps)
		{
			if (flag)
			{
				townPassageProp.Activate();
			}
			else
			{
				townPassageProp.Deactivate();
			}
		}
	}

	private bool IsAgentInteractionAllowed_AdditionalCondition()
	{
		return true;
	}
}
