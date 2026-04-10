using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipControl;
using NavalDLC.Missions.ShipInput;
using NavalDLC.Storyline.Objectives.Captivity;
using SandBox;
using SandBox.Missions.AgentBehaviors;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.MissionLogics;
using TaleWorlds.MountAndBlade.Missions.Objectives;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline;

public class NavalStorylineCaptivityMissionController : MissionLogic
{
	private const int ScatteredCrewCountPerArea = 2;

	private const string PlayerEquipmentId = "item_set_player_captivity";

	private const string GunnarEquipmentId = "item_set_gangradir_captivity";

	private const float InitialOarForceMultiplier = 0.01f;

	private const float FinalOarForceMultiplier = 0.95f;

	private const float CloseSailsDistanceToFinalHighlight = 90f;

	private const float WindStrength = 1.1f;

	private const float FadeInDuration = 0.75f;

	private const float BlackDuration = 1f;

	private const float FadeOutDuration = 0.75f;

	private int _missionInitializationPeriod;

	private MissionObjectiveLogic _missionObjectiveLogic;

	private Agent _gunnarAgent;

	private readonly List<Agent> _crewAgents = new List<Agent>();

	private readonly CharacterObject _allyCharacterObject;

	private readonly BasicCharacterObject _enemyCharacterObject;

	private readonly CharacterObject _crewCharacterObject;

	private ShipOarMachine _oarUsedByPlayer;

	private ShipOarMachine _oarUsedByAlly;

	private List<GameEntity> _entities = new List<GameEntity>();

	private readonly List<(Agent, bool)> _scatteredCrew = new List<(Agent, bool)>();

	private readonly List<Agent> _savedScatteredAgents = new List<Agent>();

	private bool _allScatteredCrewMembersAreSaved;

	private bool _hasTalkedToGunnarOutro;

	private float _outroSpeechDelayTimer;

	private SpawnedItemEntity _weaponEntity;

	private GameEntity _spawnZone1;

	private GameEntity _spawnZone2;

	private bool _isFinalized;

	private bool _hasSavedOarsmen;

	private SoundEvent _spawnZone1HelpSoundEvent;

	private SoundEvent _spawnZone2HelpSoundEvent;

	private int _savedOarsmenCount;

	private bool _hasTalkedToGunnar;

	private bool _isConversationSetupInProgress;

	private int _spawnedOarsmenCount;

	private float _speechDelayTimer;

	private int _saveTargetAgentCount;

	private ActionIndexCache _tinkeringAction;

	private bool _isPlayerTinkeringWithTheBindsMachine;

	private int _previousOarsmenLevel;

	private List<AgentBindsMachine> _agentBindMachines = new List<AgentBindsMachine>();

	private List<ShipOarMachine> _leftOars = new List<ShipOarMachine>();

	private List<ShipOarMachine> _rightOars = new List<ShipOarMachine>();

	private Dictionary<Agent, ShipOarMachine> _oarAssignments = new Dictionary<Agent, ShipOarMachine>();

	private Agent _crewConversationAgent;

	public Action OnMarkedObjectStatusChangedEvent;

	public Action OnPlayerStartedEscapeEvent;

	public Action<Vec3> OnConversationSetupEvent;

	public Action<int> OnOarsmenLevelChanged;

	public Action<float, float, float> OnStartFadeOutEvent;

	public Action OnFirstHighlightClearedEvent;

	public MissionShip MissionShip { get; private set; }

	public bool IsPlayerFree { get; private set; }

	public bool HasTalkedToGunnar => _hasTalkedToGunnar;

	public bool WasPlayerKnockedOut { get; private set; }

	public NavalStorylineCaptivityMissionController(CharacterObject allyCharacter, BasicCharacterObject enemyCharacter, CharacterObject crewCharacter)
	{
		_allyCharacterObject = allyCharacter;
		_enemyCharacterObject = enemyCharacter;
		_crewCharacterObject = crewCharacter;
	}

	public override void OnBehaviorInitialize()
	{
		if (!SailWindProfile.IsSailWindProfileInitialized)
		{
			SailWindProfile.InitializeProfile();
		}
	}

	public bool IsInitialized()
	{
		return _missionInitializationPeriod > 1;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Expected O, but got Unknown
		//IL_02c5: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		if (_missionInitializationPeriod == 0)
		{
			if (!SailWindProfile.IsSailWindProfileInitialized)
			{
				SailWindProfile.InitializeProfile();
			}
			_missionInitializationPeriod++;
			_missionObjectiveLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionObjectiveLogic>();
			UpdateEntityReferences();
			((MissionBehavior)this).Mission.PlayerTeam.DisableDetachmentTicking();
			((MissionBehavior)this).Mission.Scene.SetWaterStrength(0f);
			MissionShip = CreateShip();
			UpdateEntityReferences();
			CategorizeOars();
			((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("spawn_highlight_1"))).SetVisibilityExcludeParents(false);
			((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("spawn_highlight_2"))).SetVisibilityExcludeParents(false);
			MissionShip.SetController(ShipControllerType.AI, autoUpdateController: false);
			MissionShip.SetCustomSailSetting(enableCustomSailSetting: true, SailInput.Raised);
			MatrixFrame globalFrame = MissionShip.GlobalFrame;
			MissionShip.ShipOrder.SetShipMovementOrder(((Vec3)(ref globalFrame.origin)).AsVec2 + ((Vec3)(ref globalFrame.rotation.f)).AsVec2 * 50f);
			GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("sp_wind")));
			if (val != (GameEntity)null)
			{
				Vec3 f = val.GetGlobalFrame().rotation.f;
				Vec2 asVec = ((Vec3)(ref f)).AsVec2;
				Vec2 val2 = ((Vec2)(ref asVec)).Normalized() * 1.1f;
				Mission.Current.Scene.SetGlobalWindStrengthVector(ref val2);
			}
			((MissionBehavior)this).Mission.Scene.SetWaterStrength(1f);
			Mission.Current.OnDeploymentFinished();
			MBMusicManager.Current.StartThemeWithConstantIntensity((MusicTheme)8, false);
			MBMusicManager.Current.ChangeCurrentThemeIntensity(-1f);
			MBMusicManager.Current.ChangeCurrentThemeIntensity(0.5f);
		}
		else if (_missionInitializationPeriod == 1)
		{
			_missionInitializationPeriod++;
			SpawnPlayerAgent();
			SpawnAllyAgent();
			SpawnEnemyAgents();
			SpawnCrewAgents();
			SpawnWeapon();
			InitializeUsableMachines();
			SetOarForceMultipliers(0.01f);
			Formation formation = MissionShip.Formation;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)MissionShip).GameEntity;
			Vec3 val3 = -((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation.f;
			formation.SetFacingOrder(FacingOrder.FacingOrderLookAtDirection(((Vec3)(ref val3)).AsVec2));
			TextObject val4 = new TextObject("{=lRLE9fpA}{PLAYER.NAME}! Your chain is loose. It's now or never! Get up and strike them down!", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val4, "PLAYER", Hero.MainHero.CharacterObject, false);
			CampaignInformationManager.AddDialogLine(val4, _allyCharacterObject, ((BasicCharacterObject)_allyCharacterObject).FirstCivilianEquipment, 1000, (NotificationPriority)2);
			CaptivityEscapeCaptivityObjective captivityEscapeCaptivityObjective = new CaptivityEscapeCaptivityObjective(Mission.Current, this);
			_missionObjectiveLogic.StartObjective((MissionObjective)(object)captivityEscapeCaptivityObjective);
		}
		CheckEnemyAlarmedState();
		CheckIfCrewmenAreNearby();
		if (_isPlayerTinkeringWithTheBindsMachine)
		{
			CheckIfPlayerIsReleasedFromOar();
		}
		if (_hasSavedOarsmen && !_hasTalkedToGunnar)
		{
			_speechDelayTimer += dt;
			if (!_isConversationSetupInProgress && _speechDelayTimer > 0.75f)
			{
				SetupPostFightConversation();
			}
			if (_speechDelayTimer > 1.75f)
			{
				StartPostFightConversation();
				ReenableAllOars();
			}
		}
		if (_allScatteredCrewMembersAreSaved && !_hasTalkedToGunnarOutro)
		{
			_outroSpeechDelayTimer += dt;
			if (!_isConversationSetupInProgress && _outroSpeechDelayTimer > 0.75f)
			{
				SetupSavedCrewConversation();
			}
			if (_outroSpeechDelayTimer > 1.75f)
			{
				StartSavedCrewConversation();
			}
		}
		if (!HasTalkedToGunnar)
		{
			return;
		}
		if (MissionShip.ShipOrder.OarsmenLevel > 0)
		{
			foreach (Agent crewAgent in _crewAgents)
			{
				if (crewAgent.IsActive())
				{
					MakeAgentUseAssignedOarMachine(crewAgent);
				}
			}
			foreach (Agent savedScatteredAgent in _savedScatteredAgents)
			{
				if (savedScatteredAgent.IsActive() && savedScatteredAgent != _crewConversationAgent)
				{
					MakeAgentUseAssignedOarMachine(savedScatteredAgent);
				}
			}
			if (!_allScatteredCrewMembersAreSaved && !Campaign.Current.ConversationManager.IsAgentInConversation((IAgent)(object)_gunnarAgent))
			{
				MakeAgentUseAssignedOarMachine(_gunnarAgent);
			}
		}
		int oarsmenLevel = MissionShip.ShipOrder.OarsmenLevel;
		if (_previousOarsmenLevel != oarsmenLevel)
		{
			OnOarsmenLevelChanged(oarsmenLevel);
			_previousOarsmenLevel = oarsmenLevel;
		}
	}

	private void MakeAgentUseAssignedOarMachine(Agent agent)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (agent.IsDetachedFromFormation)
		{
			return;
		}
		_oarAssignments.TryGetValue(agent, out var value);
		if (value == null)
		{
			value = GetOarMachineToUse();
			if (value != null)
			{
				_oarAssignments.Add(agent, value);
			}
		}
		if (!((UsableMachine)value).IsDisabledForBattleSideAI(agent.Team.Side))
		{
			((UsableMachine)value).AddAgentAtSlotIndex(agent, 0);
		}
	}

	private void CheckIfCrewmenAreNearby()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if (!_hasSavedOarsmen || _allScatteredCrewMembersAreSaved || _scatteredCrew.Count <= 0)
		{
			return;
		}
		Vec3 origin = MissionShip.GlobalFrame.origin;
		Vec3 val2;
		for (int num = _scatteredCrew.Count - 1; num >= 0; num--)
		{
			(Agent, bool) tuple = _scatteredCrew[num];
			var (val, _) = tuple;
			if (MissionShip.GetIsAgentOnShip(val) && val.CurrentlyUsedGameObject == null)
			{
				_scatteredCrew.RemoveAt(num);
				_savedScatteredAgents.Add(val);
				if (_savedScatteredAgents.Count == 2)
				{
					OnFirstHighlightClearedEvent();
					OnFirstHighlightCleared();
				}
				if (_savedScatteredAgents.Count == _saveTargetAgentCount)
				{
					OnAllCrewSaved();
				}
			}
			else if (!tuple.Item2 && ((Vec3)(ref origin)).DistanceSquared(tuple.Item1.Position) <= 900f)
			{
				_scatteredCrew[num] = (tuple.Item1, true);
				tuple.Item1.ClearTargetFrame();
				tuple.Item1.Formation = ((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)0);
				MissionShip.SetShipClimbingOrderStandAloneTickingActive(isShipClimbingMachineStandaloneTickingActive: true);
				NavalAgentsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalAgentsLogic>();
				missionBehavior.AddAgentToShip(tuple.Item1, MissionShip);
				missionBehavior.TransferAgentToShip(tuple.Item1, MissionShip);
				OnPlayerReachedFirstZone();
				val2 = val.Position;
				float num2 = ((Vec3)(ref val2)).DistanceSquared(_spawnZone1.GlobalPosition);
				val2 = val.Position;
				if (num2 > ((Vec3)(ref val2)).DistanceSquared(_spawnZone2.GlobalPosition))
				{
					if (_spawnZone2HelpSoundEvent != null)
					{
						_spawnZone2HelpSoundEvent.Stop();
						_spawnZone2HelpSoundEvent = null;
					}
				}
				else if (_spawnZone1HelpSoundEvent != null)
				{
					_spawnZone1HelpSoundEvent.Stop();
					_spawnZone1HelpSoundEvent = null;
				}
			}
		}
		if (_allScatteredCrewMembersAreSaved)
		{
			return;
		}
		val2 = _spawnZone1.GlobalPosition;
		if (((Vec3)(ref val2)).DistanceSquared(origin) <= 900f)
		{
			_entities.First((GameEntity t) => t.HasTag("spawn_highlight_1")).SetVisibilityExcludeParents(false);
		}
		val2 = _spawnZone2.GlobalPosition;
		if (((Vec3)(ref val2)).DistanceSquared(origin) <= 900f)
		{
			_entities.First((GameEntity t) => t.HasTag("spawn_highlight_2")).SetVisibilityExcludeParents(false);
		}
	}

	private void UpdateEntityReferences()
	{
		_entities.Clear();
		((MissionBehavior)this).Mission.Scene.GetEntities(ref _entities);
	}

	private void CheckEnemyAlarmedState()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents)
		{
			if (item.IsAlarmed())
			{
				continue;
			}
			foreach (Agent item2 in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents)
			{
				bool flag = item.GetComponent<CampaignAgentComponent>().AgentNavigator.CanSeeAgent(item2);
				Vec3 position = item.Position;
				float num = ((Vec3)(ref position)).DistanceSquared(item2.Position);
				if (num <= 5f || (num <= 10f && flag))
				{
					OnAgentEntersFight(item, item2);
				}
			}
		}
	}

	public override InquiryData OnEndMissionRequest(out bool canLeave)
	{
		canLeave = _isFinalized;
		return null;
	}

	private MissionShip CreateShip()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("spawnpoint_ship")));
		MatrixFrame shipFrame = val.GetGlobalFrame();
		Scene scene = Mission.Current.Scene;
		Vec3 globalPosition = val.GlobalPosition;
		float waterLevelAtPosition = scene.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, true, false);
		shipFrame.origin = new Vec3(val.GlobalPosition.x, val.GlobalPosition.y, waterLevelAtPosition, -1f);
		Team team = Mission.GetTeam((TeamSideEnum)0);
		Formation formation = team.GetFormation((FormationClass)0);
		Ship shipOrigin = ((IEnumerable<Ship>)PartyBase.MainParty.Ships).FirstOrDefault((Func<Ship, bool>)((Ship s) => ((MBObjectBase)s.ShipHull).StringId == "ship_knarr_storyline")) ?? ((IEnumerable<Ship>)PartyBase.MainParty.Ships).First();
		MissionShip missionShip = missionBehavior.SpawnShip((IShipOrigin)(object)shipOrigin, in shipFrame, team, formation, spawnAnchored: false, (FormationClass)8);
		missionShip.ShipOrder.SetAIControllableWithoutTroops(value: true);
		missionShip.ShipOrder.SetOrderOarsmenLevel(2);
		missionShip.SetShipOrderActive(isOrderActive: false);
		return missionShip;
	}

	private void SpawnPlayerAgent()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		ShipOarMachine firstScriptOfType = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("target_player"))).Parent.GetFirstScriptOfType<ShipOarMachine>();
		((UsableMissionObject)((UsableMachine)firstScriptOfType).PilotStandingPoint).AddComponent((UsableMissionObjectComponent)new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none, true));
		_oarUsedByPlayer = firstScriptOfType;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)((UsableMachine)_oarUsedByPlayer).PilotStandingPoint).GameEntity;
		Formation formation = ((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)0);
		MBEquipmentRoster val = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("item_set_player_captivity");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)Hero.MainHero.CharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, Hero.MainHero.CharacterObject, -1, default(UniqueTroopDescriptor), false, false)).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val2 = obj2.InitialDirection(ref asVec).NoHorses(true).NoWeapons(true)
			.Formation(formation)
			.Equipment(val.DefaultEquipment);
		Agent obj3 = Mission.Current.SpawnAgent(val2, false);
		obj3.Controller = (AgentControllerType)2;
		obj3.UseGameObject((UsableMissionObject)(object)((UsableMachine)_oarUsedByPlayer).PilotStandingPoint, -1);
		((UsableMachine)_oarUsedByPlayer).OnPilotAssignedDuringSpawn();
	}

	private void SpawnAllyAgent()
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("target_ally")));
		_oarUsedByAlly = val.Parent.GetFirstScriptOfType<ShipOarMachine>();
		AgentBindsMachine firstScriptOfType = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("agentbind_ally"))).GetFirstScriptOfType<AgentBindsMachine>();
		firstScriptOfType.SetOarMachine(_oarUsedByAlly);
		_agentBindMachines.Add(firstScriptOfType);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)((UsableMachine)_oarUsedByAlly).PilotStandingPoint).GameEntity;
		MBEquipmentRoster val2 = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("item_set_gangradir_captivity");
		Formation formation = ((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)0);
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)_allyCharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, _allyCharacterObject, -1, default(UniqueTroopDescriptor), false, false)).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val3 = obj2.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false)
			.Equipment(val2.DefaultEquipment)
			.Formation(formation);
		OnAgentAssignedToOarOnSpawn(_gunnarAgent = Mission.Current.SpawnAgent(val3, false), _oarUsedByAlly);
	}

	private Agent SpawnAllyCrewAgent(Vec3 globalPosition, Vec2 globalDirection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)_crewCharacterObject).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)_crewCharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerTeam).InitialPosition(ref globalPosition)
			.InitialDirection(ref globalDirection)
			.NoHorses(true)
			.NoWeapons(false);
		Agent val2 = Mission.Current.SpawnAgent(val, false);
		val2.GetComponent<AgentNavalComponent>().SetCanDrown(canDrown: false);
		Vec3 position = val2.Position;
		val2.SetTargetPosition(((Vec3)(ref position)).AsVec2);
		VisualTrackerMissionBehavior missionBehavior = Mission.Current.GetMissionBehavior<VisualTrackerMissionBehavior>();
		if (missionBehavior != null)
		{
			missionBehavior.RegisterLocalOnlyObject((ITrackableBase)(object)val2);
		}
		return val2;
	}

	private void SpawnEnemyAgents()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		foreach (GameEntity item in _entities.Where((GameEntity t) => t.HasTag("spawnpoint_guard")).ToList())
		{
			AgentBuildData obj = new AgentBuildData(_enemyCharacterObject).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(_enemyCharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam);
			Vec3 globalPosition = item.GlobalPosition;
			AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
			MatrixFrame globalFrame = item.GetGlobalFrame();
			Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			val = ((Vec2)(ref val)).Normalized();
			AgentBuildData val2 = obj2.InitialDirection(ref val).NoHorses(true).NoWeapons(false);
			Agent obj3 = Mission.Current.SpawnAgent(val2, false);
			CampaignAgentComponent component = obj3.GetComponent<CampaignAgentComponent>();
			if (component.AgentNavigator == null)
			{
				component.CreateAgentNavigator();
			}
			string text = "act_drunk_trio_right";
			if (item.HasTag("guard_1"))
			{
				text = "act_drunk_trio_middle";
			}
			else if (item.HasTag("guard_2"))
			{
				text = "act_drunk_trio_left";
			}
			else if (item.HasTag("guard_3"))
			{
				text = "act_drunk_trio_right";
			}
			MBActionSet actionSet = MBGlobals.GetActionSet("as_human_hideout_bandit");
			AnimationSystemData val3 = MonsterExtensions.FillAnimationSystemData(val2.AgentMonster, actionSet, ((BasicCharacterObject)NavalStorylineData.Gunnar.CharacterObject).GetStepSize(), false);
			obj3.SetActionSet(ref val3);
			ActionIndexCache val4 = ActionIndexCache.Create(text);
			obj3.SetActionChannel(0, ref val4, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		}
	}

	private void SpawnCrewAgents()
	{
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Expected O, but got Unknown
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = _entities.First((GameEntity t) => t.HasTag("spawnpoint_neutral_npc_1"));
		ShipOarMachine firstScriptOfType = val.Parent.GetFirstScriptOfType<ShipOarMachine>();
		AgentBindsMachine firstScriptOfType2 = _entities.First((GameEntity t) => t.HasTag("agentbind_neutral_1")).GetFirstScriptOfType<AgentBindsMachine>();
		firstScriptOfType2.SetOarMachine(firstScriptOfType);
		_agentBindMachines.Add(firstScriptOfType2);
		GameEntity val2 = _entities.First((GameEntity t) => t.HasTag("spawnpoint_neutral_npc_2"));
		ShipOarMachine firstScriptOfType3 = val2.Parent.GetFirstScriptOfType<ShipOarMachine>();
		AgentBindsMachine firstScriptOfType4 = _entities.First((GameEntity t) => t.HasTag("agentbind_neutral_2")).GetFirstScriptOfType<AgentBindsMachine>();
		firstScriptOfType4.SetOarMachine(firstScriptOfType3);
		_agentBindMachines.Add(firstScriptOfType4);
		GameEntity val3 = _entities.First((GameEntity t) => t.HasTag("spawnpoint_neutral_npc_3"));
		ShipOarMachine firstScriptOfType5 = val3.Parent.GetFirstScriptOfType<ShipOarMachine>();
		AgentBindsMachine firstScriptOfType6 = _entities.First((GameEntity t) => t.HasTag("agentbind_neutral_3")).GetFirstScriptOfType<AgentBindsMachine>();
		firstScriptOfType6.SetOarMachine(firstScriptOfType5);
		_agentBindMachines.Add(firstScriptOfType6);
		GameEntity[] array = (GameEntity[])(object)new GameEntity[3] { val, val2, val3 };
		foreach (GameEntity val4 in array)
		{
			PartyBase.MainParty.AddMember(_crewCharacterObject, 1, 0);
			Formation formation = ((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)0);
			AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)_crewCharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, _crewCharacterObject, -1, default(UniqueTroopDescriptor), false, false)).Team(((MissionBehavior)this).Mission.PlayerTeam);
			Vec3 globalPosition = val4.GlobalPosition;
			AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
			MatrixFrame globalFrame = val4.GetGlobalFrame();
			Vec2 val5 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			val5 = ((Vec2)(ref val5)).Normalized();
			AgentBuildData val6 = obj2.InitialDirection(ref val5).NoHorses(true).NoWeapons(false)
				.Formation(formation);
			Agent val7 = Mission.Current.SpawnAgent(val6, false);
			_crewAgents.Add(val7);
			ShipOarMachine firstScriptOfType7 = val4.Parent.GetFirstScriptOfType<ShipOarMachine>();
			OnAgentAssignedToOarOnSpawn(val7, firstScriptOfType7);
			_spawnedOarsmenCount++;
		}
	}

	private void OnAgentAssignedToOarOnSpawn(Agent agent, ShipOarMachine oarMachine)
	{
		Formation formation = agent.Formation;
		if (formation != null)
		{
			formation.DetachUnit(agent, false);
		}
		agent.Detachment = (IDetachment)(object)oarMachine;
		agent.UseGameObject((UsableMissionObject)(object)((UsableMachine)oarMachine).PilotStandingPoint, -1);
		_oarAssignments.Add(agent, oarMachine);
		((UsableMachine)oarMachine).OnPilotAssignedDuringSpawn();
	}

	private void SpawnWeapon()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("pickup_weapon")));
		ItemObject val2 = MBObjectManager.Instance.GetObject<ItemObject>("shackle");
		MissionWeapon val3 = default(MissionWeapon);
		((MissionWeapon)(ref val3))._002Ector(val2, (ItemModifier)null, (Banner)null);
		_weaponEntity = Mission.Current.SpawnWeaponWithNewEntity(ref val3, (WeaponSpawnFlags)8, val.GetGlobalFrame()).GetFirstScriptOfType<SpawnedItemEntity>();
	}

	public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		if (userAgent.IsPlayerControlled)
		{
			OnMarkedObjectStatusChangedEvent();
		}
	}

	public override void OnObjectStoppedBeingUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (_isFinalized)
		{
			return;
		}
		if (userAgent.IsPlayerControlled && (object)usedObject == ((UsableMachine)_oarUsedByPlayer).PilotStandingPoint)
		{
			OnPlayerStartedEscape();
		}
		else if (userAgent == _gunnarAgent || (object)userAgent.Character == _crewCharacterObject)
		{
			if (!HasTalkedToGunnar)
			{
				_savedOarsmenCount++;
				AgentBindsMachine agentBindsMachine = _agentBindMachines.FirstOrDefault((AgentBindsMachine t) => (object)((UsableMachine)t.ShipOarMachine).PilotStandingPoint == usedObject);
				if (agentBindsMachine != null)
				{
					((UsableMissionObject)((UsableMachine)agentBindsMachine).PilotStandingPoint).IsDisabledForPlayers = true;
				}
				if (!_hasSavedOarsmen && _savedOarsmenCount >= _spawnedOarsmenCount + 1)
				{
					_hasSavedOarsmen = true;
					OnStartFadeOutEvent(0.75f, 1f, 0.75f);
				}
			}
			UsableMissionObject obj = usedObject;
			if (obj != null)
			{
				WeakGameEntity val = ((ScriptComponentBehavior)obj).GameEntity;
				val = ((WeakGameEntity)(ref val)).Parent;
				if (((WeakGameEntity)(ref val)).HasScriptOfType<ShipOarMachine>())
				{
					val = ((ScriptComponentBehavior)usedObject).GameEntity;
					Vec3 origin = ((WeakGameEntity)(ref val)).GetGlobalFrame().origin;
					WorldPosition val2 = default(WorldPosition);
					((WorldPosition)(ref val2))._002Ector(((MissionBehavior)this).Mission.Scene, origin);
					userAgent.SetScriptedPosition(ref val2, true, (AIScriptedFrameFlags)0);
					goto IL_013a;
				}
			}
			WorldPosition worldPosition = userAgent.GetWorldPosition();
			userAgent.SetScriptedPosition(ref worldPosition, true, (AIScriptedFrameFlags)0);
		}
		goto IL_013a;
		IL_013a:
		OnMarkedObjectStatusChangedEvent();
	}

	private void HandleChainVisualsAfterDialogue()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		foreach (AgentBindsMachine agentBindMachine in _agentBindMachines)
		{
			((UsableMachine)agentBindMachine).Deactivate();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)agentBindMachine).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
		}
		GameEntity? obj = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("agentbind_ally_broken")));
		if (obj != null)
		{
			obj.SetVisibilityExcludeParents(true);
		}
		GameEntity? obj2 = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("agentbind_neutral_1_broken")));
		if (obj2 != null)
		{
			obj2.SetVisibilityExcludeParents(true);
		}
		GameEntity? obj3 = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("agentbind_neutral_2_broken")));
		if (obj3 != null)
		{
			obj3.SetVisibilityExcludeParents(true);
		}
		GameEntity? obj4 = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("agentbind_neutral_3_broken")));
		if (obj4 != null)
		{
			obj4.SetVisibilityExcludeParents(true);
		}
	}

	private void OnPlayerStartedEscape()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		OnPlayerStartedEscapeEvent();
		_tinkeringAction = ActionIndexCache.Create("act_cutscene_break_chains_1");
		Agent.Main.SetActionChannel(0, ref _tinkeringAction, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		_isPlayerTinkeringWithTheBindsMachine = true;
	}

	private void CheckIfPlayerIsReleasedFromOar()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if (Agent.Main.GetCurrentAction(0) == _tinkeringAction && Agent.Main.GetCurrentActionProgress(0) > 0.95f)
		{
			Agent.Main.ClearHandInverseKinematics();
			CampaignInformationManager.AddDialogLine(new TextObject("{=g1PnXEDa}{PLAYER.NAME}! It's now or never! Go, cut those bastards down!", (Dictionary<string, object>)null), _allyCharacterObject, ((BasicCharacterObject)_allyCharacterObject).FirstCivilianEquipment, 1000, (NotificationPriority)2);
			bool flag = default(bool);
			Agent.Main.OnItemPickup(_weaponEntity, (EquipmentIndex)0, ref flag);
			_isPlayerTinkeringWithTheBindsMachine = false;
			IsPlayerFree = true;
			GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("player_shackle")));
			if (val != (GameEntity)null)
			{
				val.SetVisibilityExcludeParents(false);
			}
			((UsableMissionObject)((UsableMachine)_oarUsedByPlayer).PilotStandingPoint).IsDisabledForPlayers = true;
			CaptivityDefeatCaptorsObjective captivityDefeatCaptorsObjective = new CaptivityDefeatCaptorsObjective(Mission.Current, this);
			_missionObjectiveLogic.StartObjective((MissionObjective)(object)captivityDefeatCaptorsObjective);
			MissionShip.ShipOrder.SetShipStopOrder();
		}
	}

	private void TriggerEnemies()
	{
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents)
		{
			if (item.IsAIControlled && !item.IsUsingGameObject && !item.IsAlarmed())
			{
				OnAgentEntersFight(item);
			}
		}
	}

	private void OnAgentEntersFight(Agent agent, Agent targetAgent = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		AgentFlag agentFlags = agent.GetAgentFlags();
		agent.SetAgentFlags((AgentFlag)(agentFlags | 0x10000));
		agent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		CampaignAgentComponent component = agent.GetComponent<CampaignAgentComponent>();
		AgentNavigator val = component.AgentNavigator ?? component.CreateAgentNavigator();
		AlarmedBehaviorGroup val2 = val.GetBehaviorGroup<AlarmedBehaviorGroup>();
		if (val2 == null)
		{
			val2 = val.AddBehaviorGroup<AlarmedBehaviorGroup>();
			((AgentBehaviorGroup)val2).AddBehavior<FightBehavior>();
		}
		((AgentBehaviorGroup)val2).SetScriptedBehavior<FightBehavior>();
		agent.SetAutomaticTargetSelection(false);
		if (targetAgent == null)
		{
			targetAgent = Agent.Main;
		}
		if (targetAgent != null)
		{
			agent.SetTargetAgent(targetAgent);
			AlarmedBehaviorGroup.AlarmAgent(agent);
		}
	}

	public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		if (_scatteredCrew != null && _scatteredCrew.Any(((Agent, bool) x) => x.Item1 == affectedAgent))
		{
			Debug.FailedAssert("Should crew to save agent be removed", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\MissionControllers\\NavalStorylineCaptivityMissionController.cs", "OnEarlyAgentRemoved", 796);
		}
		if (affectedAgent.Team != ((MissionBehavior)this).Mission.PlayerEnemyTeam)
		{
			return;
		}
		TriggerEnemies();
		if (!Extensions.IsEmpty<Agent>((IEnumerable<Agent>)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents))
		{
			return;
		}
		CampaignInformationManager.AddDialogLine(new TextObject("{=bu8MRgpS}Well done! Now, help us get these chains off.", (Dictionary<string, object>)null), _allyCharacterObject, ((BasicCharacterObject)_allyCharacterObject).FirstCivilianEquipment, 1000, (NotificationPriority)2);
		foreach (AgentBindsMachine agentBindMachine in _agentBindMachines)
		{
			((UsableMissionObject)((UsableMachine)agentBindMachine).PilotStandingPoint).IsDisabledForPlayers = false;
		}
		OnMarkedObjectStatusChangedEvent();
		CaptivityFreePrisonersObjective captivityFreePrisonersObjective = new CaptivityFreePrisonersObjective(Mission.Current, this);
		_missionObjectiveLogic.StartObjective((MissionObjective)(object)captivityFreePrisonersObjective);
		foreach (Agent crewAgent in _crewAgents)
		{
			CombatVoiceNetworkPredictionType val = (CombatVoiceNetworkPredictionType)2;
			crewAgent.MakeVoice(VoiceType.Victory, val);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (affectedAgent == Agent.Main)
		{
			FinalizeMission();
		}
	}

	private void SpawnScatteredCrew()
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		_saveTargetAgentCount = 0;
		_spawnZone1 = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("crewmen_spawn_zone_alt_1")));
		_spawnZone2 = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("crewmen_spawn_zone_alt_2")));
		((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("spawn_highlight_1"))).SetVisibilityExcludeParents(true);
		((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("spawn_highlight_2"))).SetVisibilityExcludeParents(true);
		Vec3 globalPosition = _spawnZone1.GlobalPosition;
		MatrixFrame globalFrame = _spawnZone1.GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		SpawnCrewAroundPosition(globalPosition, ((Vec2)(ref asVec)).Normalized());
		Vec3 globalPosition2 = _spawnZone2.GlobalPosition;
		globalFrame = _spawnZone2.GetGlobalFrame();
		asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		SpawnCrewAroundPosition(globalPosition2, ((Vec2)(ref asVec)).Normalized());
		int eventGlobalIndex = SoundManager.GetEventGlobalIndex("event:/mission/ambient/special/storyline/drowning_save_us");
		_spawnZone1HelpSoundEvent = SoundEvent.CreateEvent(eventGlobalIndex, Mission.Current.Scene);
		_spawnZone1HelpSoundEvent.SetPosition(_spawnZone1.GlobalPosition);
		_spawnZone1HelpSoundEvent.Play();
		_spawnZone2HelpSoundEvent = SoundEvent.CreateEvent(eventGlobalIndex, Mission.Current.Scene);
		_spawnZone2HelpSoundEvent.SetPosition(_spawnZone2.GlobalPosition);
		_spawnZone2HelpSoundEvent.Play();
		CaptivitySaveTheCrewmenObjective captivitySaveTheCrewmenObjective = new CaptivitySaveTheCrewmenObjective(Mission.Current, this);
		_missionObjectiveLogic.StartObjective((MissionObjective)(object)captivitySaveTheCrewmenObjective);
	}

	private void SpawnCrewAroundPosition(Vec3 spawnGlobalPosition, Vec2 spawnGlobalDirection)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		spawnGlobalPosition.z = ((MissionBehavior)this).Mission.Scene.GetWaterLevelAtPosition(((Vec3)(ref spawnGlobalPosition)).AsVec2, false, false) - 3f;
		for (int i = 0; i < 2; i++)
		{
			Agent item = SpawnAllyCrewAgent(spawnGlobalPosition + new Vec3(MBRandom.RandomFloatRanged(1f, 4f), MBRandom.RandomFloatRanged(1f, 4f), 0f, -1f), spawnGlobalDirection);
			_scatteredCrew.Add((item, false));
			_saveTargetAgentCount++;
		}
	}

	private void SetupPostFightConversation()
	{
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		_isConversationSetupInProgress = true;
		MissionShip.SetAnchor(isAnchored: true, anchorInPlace: true);
		MissionShip.ShipOrder.SetOrderOarsmenLevel(0);
		GameEntity val = _entities.First((GameEntity t) => t.HasTag("conversation_player"));
		GameEntity val2 = _entities.First((GameEntity t) => t.HasTag("conversation_ally"));
		if (Agent.Main == null || !Agent.Main.IsActive())
		{
			RespawnMainAgent(val);
		}
		Agent.Main.AgentVisuals.SetVisible(false);
		for (int num = ((List<Agent>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents).Count - 1; num >= 0; num--)
		{
			((List<Agent>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents)[num].FadeOut(true, true);
		}
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents)
		{
			if (!item.IsPlayerControlled && item != _gunnarAgent)
			{
				item.AgentVisuals.SetVisible(false);
			}
		}
		if (_gunnarAgent.IsUsingGameObject)
		{
			_gunnarAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			_gunnarAgent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			_gunnarAgent.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
		}
		_gunnarAgent.TeleportToPosition(val2.GlobalPosition);
		Agent.Main.AgentVisuals.SetVisible(true);
		Agent.Main.TeleportToPosition(val.GlobalPosition);
		Vec3 obj = val2.GlobalPosition - Agent.Main.Position;
		OnConversationSetupEvent(obj);
		WorldPosition val3 = default(WorldPosition);
		((WorldPosition)(ref val3))._002Ector(Mission.Current.Scene, val2.GlobalPosition);
		Agent gunnarAgent = _gunnarAgent;
		Vec2 val4 = ((Vec3)(ref obj)).AsVec2;
		gunnarAgent.SetScriptedPositionAndDirection(ref val3, 0f - ((Vec2)(ref val4)).RotationInRadians, false, (AIScriptedFrameFlags)0);
		Agent gunnarAgent2 = _gunnarAgent;
		val4 = ((Vec3)(ref obj)).AsVec2;
		val4 = -((Vec2)(ref val4)).Normalized();
		gunnarAgent2.SetMovementDirection(ref val4);
		_gunnarAgent.Controller = (AgentControllerType)0;
	}

	private void StartPostFightConversation()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		_hasTalkedToGunnar = true;
		_isConversationSetupInProgress = false;
		Campaign.Current.ConversationManager.SetupAndStartMissionConversation((IAgent)(object)_gunnarAgent, (IAgent)(object)((MissionBehavior)this).Mission.MainAgent, true);
		foreach (AgentBindsMachine agentBindMachine in _agentBindMachines)
		{
			((UsableMachine)agentBindMachine).Deactivate();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)agentBindMachine).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
		}
		SetOarForceMultipliers(0.95f);
		foreach (GameEntity item in _entities.Where((GameEntity t) => t.HasScriptOfType<ShipControllerMachine>()))
		{
			ShipControllerMachine firstScriptOfType = item.GetFirstScriptOfType<ShipControllerMachine>();
			if (firstScriptOfType != null)
			{
				((UsableMissionObject)((UsableMachine)firstScriptOfType).PilotStandingPoint).IsDisabledForPlayers = false;
			}
		}
		OnMarkedObjectStatusChangedEvent();
		Mission.Current.SetMissionMode((MissionMode)1, true);
	}

	private void SetOarForceMultipliers(float forceMultiplier)
	{
		MissionShip.SetOarAppliedForceMultiplierForStoryMission(forceMultiplier);
	}

	private void CategorizeOars()
	{
		foreach (GameEntity child in ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("left_oar"))).GetChildren())
		{
			IEnumerable<ShipOarMachine> scriptComponents = child.GetScriptComponents<ShipOarMachine>();
			if (!Extensions.IsEmpty<ShipOarMachine>(scriptComponents))
			{
				ShipOarMachine item = scriptComponents.FirstOrDefault();
				_leftOars.Add(item);
			}
		}
		foreach (GameEntity child2 in ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("right_oar"))).GetChildren())
		{
			IEnumerable<ShipOarMachine> scriptComponents2 = child2.GetScriptComponents<ShipOarMachine>();
			if (!Extensions.IsEmpty<ShipOarMachine>(scriptComponents2))
			{
				ShipOarMachine item2 = scriptComponents2.FirstOrDefault();
				_rightOars.Add(item2);
			}
		}
	}

	private ShipOarMachine GetOarMachineToUse()
	{
		IEnumerable<ShipOarMachine> source = _leftOars.Where((ShipOarMachine t) => !((UsableMissionObject)((UsableMachine)t).PilotStandingPoint).HasUser && !((UsableMissionObject)((UsableMachine)t).PilotStandingPoint).HasAIMovingTo && _oarAssignments.All((KeyValuePair<Agent, ShipOarMachine> k) => k.Value != t));
		IEnumerable<ShipOarMachine> source2 = _rightOars.Where((ShipOarMachine t) => !((UsableMissionObject)((UsableMachine)t).PilotStandingPoint).HasUser && !((UsableMissionObject)((UsableMachine)t).PilotStandingPoint).HasAIMovingTo && _oarAssignments.All((KeyValuePair<Agent, ShipOarMachine> k) => k.Value != t));
		if (source.Count() <= source2.Count())
		{
			return source2.FirstOrDefault();
		}
		return source.FirstOrDefault();
	}

	private void RespawnMainAgent(GameEntity respawnPositionEntity)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		WasPlayerKnockedOut = true;
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)Hero.MainHero.CharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, Hero.MainHero.CharacterObject, -1, default(UniqueTroopDescriptor), false, false)).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = respawnPositionEntity.GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = respawnPositionEntity.GetGlobalFrame();
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		AgentBuildData val2 = obj2.InitialDirection(ref val).NoHorses(true).NoWeapons(false);
		Mission.Current.SpawnAgent(val2, false).Controller = (AgentControllerType)2;
	}

	private void ReenableAllOars()
	{
		IEnumerable<GameEntity> enumerable = _entities.Where((GameEntity t) => t.HasScriptOfType<UsableMachine>());
		Formation formation = ((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)0);
		foreach (GameEntity item in enumerable)
		{
			UsableMachine firstScriptOfType = item.GetFirstScriptOfType<UsableMachine>();
			if (firstScriptOfType is ShipOarMachine && !((List<IDetachment>)(object)formation.Detachments).Contains((IDetachment)(object)firstScriptOfType))
			{
				ModuleExtensions.StartUsingMachine(formation, firstScriptOfType, false);
			}
		}
	}

	private void InitializeUsableMachines()
	{
		foreach (GameEntity item in _entities.Where((GameEntity t) => t.HasScriptOfType<UsableMachine>()))
		{
			UsableMachine firstScriptOfType = item.GetFirstScriptOfType<UsableMachine>();
			if (firstScriptOfType is ShipOarMachine)
			{
				firstScriptOfType.SetEnemyRangeToStopUsing(-1f);
				if ((object)firstScriptOfType != _oarUsedByPlayer)
				{
					((UsableMissionObject)firstScriptOfType.PilotStandingPoint).IsDisabledForPlayers = true;
				}
			}
			if (firstScriptOfType is ShipControllerMachine)
			{
				((UsableMissionObject)firstScriptOfType.PilotStandingPoint).IsDisabledForPlayers = true;
			}
		}
		foreach (AgentBindsMachine agentBindMachine in _agentBindMachines)
		{
			((UsableMissionObject)((UsableMachine)agentBindMachine).PilotStandingPoint).IsDisabledForPlayers = true;
		}
	}

	public override void OnAgentAlarmedStateChanged(Agent agent, AIStateFlag flag)
	{
		if (agent.Character == _enemyCharacterObject && (agent.IsUsingGameObject || AgentComponentExtensions.AIInterestedInAnyGameObject(agent)))
		{
			agent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
	}

	public void FinalizeMission()
	{
		_isFinalized = true;
		MBMusicManager.Current.ForceStopThemeWithFadeOut();
		((MissionBehavior)this).Mission.EndMission();
	}

	public override bool MissionEnded(ref MissionResult missionResult)
	{
		if (_isFinalized)
		{
			missionResult = MissionResult.CreateSuccessful((IMission)(object)((MissionBehavior)this).Mission, false);
			return true;
		}
		return false;
	}

	public void OnShipCaptured()
	{
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents)
		{
			if (!item.IsPlayerControlled || item != _gunnarAgent)
			{
				item.AgentVisuals.SetVisible(true);
			}
		}
		_gunnarAgent.Controller = (AgentControllerType)1;
		_gunnarAgent.ClearTargetFrame();
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)2, true);
		SpawnScatteredCrew();
		MissionShip.SetController(ShipControllerType.None);
		MissionShip.ShipOrder.SetShipStopOrder();
		MissionShip.SetInputRecord(ShipInputRecord.None());
		MissionShip.SetCustomSailSetting(enableCustomSailSetting: false, SailInput.Raised);
		MissionShip.SetShipOrderActive(isOrderActive: false);
		Formation formation = Mission.GetTeam((TeamSideEnum)0).GetFormation((FormationClass)0);
		MissionShip.ShipOrder.SetFormation(formation);
		if (((UsableMissionObject)((UsableMachine)_oarUsedByAlly).PilotStandingPoint).UserAgent != null && ((UsableMissionObject)((UsableMachine)_oarUsedByAlly).PilotStandingPoint).UserAgent != _gunnarAgent)
		{
			((UsableMissionObject)((UsableMachine)_oarUsedByAlly).PilotStandingPoint).UserAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		HandleChainVisualsAfterDialogue();
		MissionShip.SetAnchor(isAnchored: false);
	}

	public bool IsSavedCrew(IAgent agent)
	{
		return ((IEnumerable<IAgent>)_savedScatteredAgents).Contains(agent);
	}

	private void OnAllCrewSaved()
	{
		_allScatteredCrewMembersAreSaved = true;
		OnStartFadeOutEvent(0.75f, 1f, 0.75f);
		_crewConversationAgent = _savedScatteredAgents[_savedScatteredAgents.Count - 1];
		if (MissionShip.IsPlayerControlled)
		{
			Agent.Main.HandleStopUsingAction();
		}
	}

	private void SetupSavedCrewConversation()
	{
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		_isConversationSetupInProgress = true;
		GameEntity val = _entities.First((GameEntity t) => t.HasTag("conversation_player"));
		GameEntity val2 = _entities.First((GameEntity t) => t.HasTag("conversation_ally"));
		GameEntity val3 = _entities.First((GameEntity t) => t.HasTag("conversation_crew"));
		Agent.Main.AgentVisuals.SetVisible(true);
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents)
		{
			if (!item.IsPlayerControlled && item != _gunnarAgent && item != _crewConversationAgent)
			{
				item.AgentVisuals.SetVisible(false);
			}
		}
		if (_gunnarAgent.IsUsingGameObject)
		{
			_gunnarAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			_gunnarAgent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			_gunnarAgent.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
		}
		if (_crewConversationAgent.IsUsingGameObject)
		{
			_crewConversationAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			_crewConversationAgent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			_crewConversationAgent.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
		}
		if (Agent.Main.IsUsingGameObject)
		{
			Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		if (!MissionShip.HasController)
		{
			MissionShip.SetInputRecord(ShipInputRecord.None());
		}
		else if (MissionShip.IsAIControlled)
		{
			MissionShip.ShipOrder.SetShipStopOrder();
		}
		if (!MissionShip.Physics.IsAnchored)
		{
			MissionShip.Physics.SetAnchor(isAnchored: true);
		}
		_gunnarAgent.ClearTargetFrame();
		_gunnarAgent.TeleportToPosition(val2.GlobalPosition);
		Agent.Main.TeleportToPosition(val.GlobalPosition);
		_crewConversationAgent.TeleportToPosition(val3.GlobalPosition);
		_crewConversationAgent.ClearTargetFrame();
		WorldPosition val4 = default(WorldPosition);
		((WorldPosition)(ref val4))._002Ector(((MissionBehavior)this).Mission.Scene, val3.GlobalPosition);
		_crewConversationAgent.SetScriptedPosition(ref val4, true, (AIScriptedFrameFlags)16);
		Vec3 obj = _crewConversationAgent.Position - Agent.Main.Position;
		OnConversationSetupEvent(obj);
		Agent crewConversationAgent = _crewConversationAgent;
		Vec2 val5 = ((Vec3)(ref obj)).AsVec2;
		val5 = -((Vec2)(ref val5)).Normalized();
		crewConversationAgent.SetMovementDirection(ref val5);
		_crewConversationAgent.Controller = (AgentControllerType)0;
		WorldPosition val6 = default(WorldPosition);
		((WorldPosition)(ref val6))._002Ector(Mission.Current.Scene, val2.GlobalPosition);
		Agent gunnarAgent = _gunnarAgent;
		val5 = ((Vec3)(ref obj)).AsVec2;
		gunnarAgent.SetScriptedPositionAndDirection(ref val6, 0f - ((Vec2)(ref val5)).RotationInRadians, false, (AIScriptedFrameFlags)0);
		Vec3 val7 = Agent.Main.Position - val2.GlobalPosition;
		Agent gunnarAgent2 = _gunnarAgent;
		val5 = ((Vec3)(ref val7)).AsVec2;
		val5 = ((Vec2)(ref val5)).Normalized();
		gunnarAgent2.SetMovementDirection(ref val5);
		_gunnarAgent.Controller = (AgentControllerType)0;
		MissionShip.ShipOrder.SetShipStopOrder();
	}

	private void OnPlayerReachedFirstZone()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		CampaignInformationManager.AddDialogLine(new TextObject("{=wYMz91k4}Right - now let’s slow down so that they can climb aboard.", (Dictionary<string, object>)null), _allyCharacterObject, ((BasicCharacterObject)_allyCharacterObject).FirstCivilianEquipment, 1000, (NotificationPriority)2);
	}

	private void OnFirstHighlightCleared()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		CampaignInformationManager.AddDialogLine(new TextObject("{=HuChgeJp}There’s two more of them over there. Let’s go fish them out.", (Dictionary<string, object>)null), _allyCharacterObject, ((BasicCharacterObject)_allyCharacterObject).FirstCivilianEquipment, 1000, (NotificationPriority)2);
	}

	private void StartSavedCrewConversation()
	{
		_hasTalkedToGunnarOutro = true;
		_isConversationSetupInProgress = false;
		Mission.Current.SetMissionMode((MissionMode)1, true);
		Campaign.Current.ConversationManager.SetupAndStartMissionConversation((IAgent)(object)_crewConversationAgent, (IAgent)(object)((MissionBehavior)this).Mission.MainAgent, true);
		Campaign.Current.ConversationManager.AddConversationAgents((IEnumerable<IAgent>)new List<IAgent> { (IAgent)(object)_gunnarAgent }, true);
	}

	public override void OnTutorialCompleted(string completedTutorialIdentifier)
	{
		if (completedTutorialIdentifier == "ShipCameraTutorial")
		{
			OnCameraTutorialFinished();
		}
	}

	private void OnCameraTutorialFinished()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		CampaignInformationManager.AddDialogLine(new TextObject("{=o8Jj8RJ1}Can you see those poor lads thrashing in the water over there?", (Dictionary<string, object>)null), _allyCharacterObject, ((BasicCharacterObject)_allyCharacterObject).FirstCivilianEquipment, 1000, (NotificationPriority)2);
	}

	public ShipControllerMachine GetMarkedShipControllerMachine()
	{
		if (HasTalkedToGunnar)
		{
			Agent userAgent = ((UsableMissionObject)((UsableMachine)MissionShip.ShipControllerMachine).PilotStandingPoint).UserAgent;
			if (userAgent == null || !userAgent.IsPlayerControlled)
			{
				return MissionShip.ShipControllerMachine;
			}
		}
		return null;
	}

	public List<AgentBindsMachine> GetMarkedAgentBinds()
	{
		return _agentBindMachines.Where((AgentBindsMachine t) => !((UsableMissionObject)((UsableMachine)t).PilotStandingPoint).IsDisabledForPlayers).ToList();
	}

	public List<Agent> GetScatteredCrewmen()
	{
		return _scatteredCrew.Select(((Agent, bool) t) => t.Item1).ToList();
	}

	public List<Agent> GetCaptorAgents()
	{
		return ((IEnumerable<Agent>)Mission.Current.PlayerEnemyTeam.ActiveAgents).ToList();
	}

	public bool IsFirstHighlightCleared()
	{
		return _savedScatteredAgents.Count((Agent t) => t.IsOnLand()) == 2;
	}

	public bool IsReadyToCloseSails()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (IsFirstHighlightCleared() && _scatteredCrew.Count > 0)
		{
			Vec3 val = _scatteredCrew.FirstOrDefault().Item1.Position - MissionShip.GlobalFrame.origin;
			return ((Vec3)(ref val)).LengthSquared <= 8100f;
		}
		return false;
	}

	public float GetStoppedShipSpeedThreshold()
	{
		return 2f;
	}

	public bool IsPlayerInShipControls()
	{
		if (MissionShip != null && Agent.Main != null)
		{
			return ((UsableMissionObject)((UsableMachine)MissionShip.ShipControllerMachine).PilotStandingPoint).UserAgent == Agent.Main;
		}
		return false;
	}
}
