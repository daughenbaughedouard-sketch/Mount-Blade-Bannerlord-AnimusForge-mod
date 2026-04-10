using System;
using System.Collections.Generic;
using System.Linq;
using SandBox;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Storyline;

public class NavalStorylineAlleyFightMissionController : MissionLogic
{
	private const string EnemyTroopStringId = "naval_storyline_alley_fight_enemy";

	private const float SpeechDelayAfterCombatDuration = 1.5f;

	private const float BanterNotificationRepeatDuration = 12f;

	private const string GunnarEquipmentId = "item_set_gangradir_alleyfight";

	private bool _isMissionInitialized;

	private bool _isMissionFailed;

	private List<GameEntity> _entities = new List<GameEntity>();

	private Agent _gunnarAgent;

	private bool _willGunnarBecomeVulnerable;

	private float _gunnarInvulnerabilityTimer;

	private float _gunnarInvulnerabilityDurationAfterCinematic = 10f;

	private bool _shouldShowEndNotification;

	private bool _shouldShowBanterNotifications = true;

	private float _banterNotificationTimer = 12f;

	private int _banterLineIndex;

	private List<TextObject> _banterLines = new List<TextObject>
	{
		new TextObject("{=kDQXVwSJ}Hey old man! We want a word.", (Dictionary<string, object>)null),
		new TextObject("{=J3eXaYJs}Don't worry - we just want to talk to you.", (Dictionary<string, object>)null),
		new TextObject("{=q7cvwXab}We're not going to hurt you.", (Dictionary<string, object>)null),
		new TextObject("{=aneZwbHJ}Easy there, grandpa. Hand off your sword hilt.", (Dictionary<string, object>)null)
	};

	private bool _shoulStartOutroConversation;

	private float _speechDelayTimer;

	private bool _isEnemyAttackToPlayerQueued;

	private float _enemyAttackToPlayerTimer;

	private float _enemyAttackToPlayerDuration = 3f;

	private Agent _directedEnemyAgent;

	public override void EarlyStart()
	{
		((MissionBehavior)this).EarlyStart();
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)0, Clan.PlayerClan.Color, Clan.PlayerClan.Color2, Clan.PlayerClan.Banner, true, false, true);
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)1, NavalStorylineData.CorsairBanner.GetPrimaryColor(), NavalStorylineData.CorsairBanner.GetSecondaryColor(), NavalStorylineData.CorsairBanner, true, false, true);
		((MissionBehavior)this).Mission.PlayerTeam = ((MissionBehavior)this).Mission.DefenderTeam;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		if (!_isMissionInitialized)
		{
			_isMissionInitialized = true;
			UpdateEntityReferences();
			Team team = Mission.GetTeam((TeamSideEnum)0);
			Formation formation = team.GetFormation((FormationClass)0);
			Mission.GetTeam((TeamSideEnum)2);
			SpawnPlayer();
			GameEntity spawnPoint = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("sp_gangradir")));
			SpawnGunnar(spawnPoint);
			SpawnEnemyTroop("sp_thug_1", "act_argue_trio_right");
			SpawnEnemyTroop("sp_thug_2", "act_argue_trio_middle_2");
			SpawnEnemyTroop("sp_thug_3", "act_argue_trio_left");
			team.SetPlayerRole(true, true);
			formation.PlayerOwner = Agent.Main;
			Mission.Current.OnDeploymentFinished();
		}
		if (_willGunnarBecomeVulnerable)
		{
			_gunnarInvulnerabilityTimer += dt;
			if (_gunnarInvulnerabilityTimer >= _gunnarInvulnerabilityDurationAfterCinematic)
			{
				_gunnarAgent.ToggleInvulnerable();
				_willGunnarBecomeVulnerable = false;
			}
		}
		if (_shoulStartOutroConversation)
		{
			_speechDelayTimer += dt;
			if (_speechDelayTimer >= 1.5f)
			{
				_shoulStartOutroConversation = false;
				TriggerCombatEnd();
			}
		}
		if (_shouldShowEndNotification)
		{
			GameTexts.SetVariable("leave_key", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 4), 1f));
			ShowNotification(GameTexts.FindText("str_battle_won_press_tab_to_view_results", (string)null), null);
			_shouldShowEndNotification = false;
		}
		if (_shouldShowBanterNotifications)
		{
			_banterNotificationTimer += dt;
			if (_banterNotificationTimer > 12f)
			{
				_banterNotificationTimer = 0f;
				TextObject text = _banterLines[_banterLineIndex];
				ShowNotification(text, Extensions.GetRandomElement<Agent>(((MissionBehavior)this).Mission.Teams.PlayerEnemy.ActiveAgents).Character);
				_banterLineIndex = (_banterLineIndex + 1) % _banterLines.Count;
			}
		}
		if (_isEnemyAttackToPlayerQueued)
		{
			_enemyAttackToPlayerTimer += dt;
			if (_enemyAttackToPlayerTimer >= _enemyAttackToPlayerDuration)
			{
				_isEnemyAttackToPlayerQueued = false;
				Agent randomElement = Extensions.GetRandomElement<Agent>(((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents);
				if (randomElement != null)
				{
					_directedEnemyAgent = randomElement;
				}
			}
		}
		if (_directedEnemyAgent != null && _directedEnemyAgent.IsActive())
		{
			Vec3 position = _directedEnemyAgent.Position;
			if (((Vec3)(ref position)).DistanceSquared(Agent.Main.Position) <= 1f)
			{
				_directedEnemyAgent.ClearTargetFrame();
				_directedEnemyAgent = null;
			}
			else
			{
				WorldPosition val = default(WorldPosition);
				((WorldPosition)(ref val))._002Ector(((MissionBehavior)this).Mission.Scene, Agent.Main.Position);
				_directedEnemyAgent.SetScriptedPosition(ref val, false, (AIScriptedFrameFlags)8);
			}
		}
	}

	private void UpdateEntityReferences()
	{
		((MissionBehavior)this).Mission.Scene.GetEntities(ref _entities);
	}

	public void OnCinematicStarted()
	{
		_shouldShowBanterNotifications = false;
		Mission.Current.SetMissionMode((MissionMode)9, true);
	}

	public void StartFight()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		Mission.Current.SetMissionMode((MissionMode)2, true);
		_willGunnarBecomeVulnerable = true;
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Teams.PlayerEnemy.ActiveAgents)
		{
			item.ToggleInvulnerable();
		}
		OnTeamAgentsShouldAttack(((MissionBehavior)this).Mission.Teams.Player);
		OnTeamAgentsShouldAttack(((MissionBehavior)this).Mission.Teams.PlayerEnemy);
		((MissionBehavior)this).Mission.PlayerTeam.MasterOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerTeam.MasterOrderController.SetOrder((OrderType)4);
		_isEnemyAttackToPlayerQueued = true;
		ShowNotification(new TextObject("{=6zHJnnil}Hey you! Stranger! Would you like to help an old man drive off a few stray dogs here?", (Dictionary<string, object>)null), (BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject);
	}

	private void SpawnPlayer()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("sp_player")));
		Formation formation = ((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)0);
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)Hero.MainHero.CharacterObject).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)Hero.MainHero.CharacterObject, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = val.GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = val.GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val2 = obj2.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false)
			.Formation(formation);
		Mission.Current.SpawnAgent(val2, false).Controller = (AgentControllerType)2;
	}

	private void SpawnGunnar(GameEntity spawnPoint)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		MBEquipmentRoster val = Game.Current.ObjectManager.GetObject<MBEquipmentRoster>("item_set_gangradir_alleyfight");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)NavalStorylineData.Gunnar.CharacterObject).TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, NavalStorylineData.Gunnar.CharacterObject, -1, default(UniqueTroopDescriptor), false, false)).Team(((MissionBehavior)this).Mission.PlayerTeam);
		Vec3 globalPosition = spawnPoint.GlobalPosition;
		AgentBuildData obj2 = obj.InitialPosition(ref globalPosition);
		MatrixFrame globalFrame = spawnPoint.GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val2 = obj2.InitialDirection(ref asVec).NoHorses(true).NoWeapons(false)
			.Equipment(val.DefaultEquipment);
		_gunnarAgent = Mission.Current.SpawnAgent(val2, false);
		MBActionSet actionSet = MBGlobals.GetActionSet("as_human_hideout_bandit");
		AnimationSystemData val3 = MonsterExtensions.FillAnimationSystemData(val2.AgentMonster, actionSet, ((BasicCharacterObject)NavalStorylineData.Gunnar.CharacterObject).GetStepSize(), false);
		_gunnarAgent.SetActionSet(ref val3);
		_gunnarAgent.SetActionChannel(0, ref ActionIndexCache.act_argue_trio_middle, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		UsableMachine firstScriptOfType = spawnPoint.GetFirstScriptOfType<UsableMachine>();
		if (firstScriptOfType != null)
		{
			StandingPoint val4 = ((IEnumerable<StandingPoint>)firstScriptOfType.StandingPoints).FirstOrDefault();
			_gunnarAgent.UseGameObject((UsableMissionObject)(object)val4, -1);
		}
		_gunnarAgent.ToggleInvulnerable();
	}

	private void SpawnEnemyTroop(string spawnPointId, string animationId)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("naval_storyline_alley_fight_enemy");
		GameEntity? obj = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag(spawnPointId)));
		Vec3 globalPosition = obj.GlobalPosition;
		MatrixFrame globalFrame = obj.GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		AgentBuildData val2 = new AgentBuildData((BasicCharacterObject)(object)val).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam).InitialPosition(ref globalPosition)
			.InitialDirection(ref asVec)
			.NoHorses(true)
			.NoWeapons(false)
			.Banner(NavalStorylineData.CorsairBanner);
		Agent val3 = Mission.Current.SpawnAgent(val2, false);
		MBActionSet actionSet = MBGlobals.GetActionSet("as_human_hideout_bandit");
		AnimationSystemData val4 = MonsterExtensions.FillAnimationSystemData(val2.AgentMonster, actionSet, ((BasicCharacterObject)val).GetStepSize(), false);
		val3.SetActionSet(ref val4);
		ActionIndexCache val5 = ActionIndexCache.Create(animationId);
		val3.SetActionChannel(0, ref val5, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		StandingPoint val6 = ((IEnumerable<StandingPoint>)obj.GetFirstScriptOfType<UsableMachine>().StandingPoints).FirstOrDefault();
		val3.UseGameObject((UsableMissionObject)(object)val6, -1);
		for (int num = 0; num < 50; num++)
		{
			val3.TickActionChannels(0.1f);
		}
		val3.ToggleInvulnerable();
	}

	private void OnTeamAgentsShouldAttack(Team team)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent item in (List<Agent>)(object)team.ActiveAgents)
		{
			AgentFlag agentFlags = item.GetAgentFlags();
			item.SetAgentFlags((AgentFlag)(agentFlags | 0x10000));
			CampaignAgentComponent component = item.GetComponent<CampaignAgentComponent>();
			AgentNavigator val = component.AgentNavigator;
			if (val == null)
			{
				val = component.CreateAgentNavigator();
				((AgentBehaviorGroup)val.AddBehaviorGroup<AlarmedBehaviorGroup>()).AddBehavior<FightBehavior>();
			}
			((AgentBehaviorGroup)val.GetBehaviorGroup<AlarmedBehaviorGroup>()).SetScriptedBehavior<FightBehavior>();
			item.SetAlarmState((AIStateFlag)3);
			if (item.IsUsingGameObject)
			{
				item.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents))
		{
			OnEnemyTeamDefeated();
		}
		else if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents) || affectedAgent.IsMainAgent)
		{
			OnPlayerTeamDefeated();
			if (affectedAgent.IsMainAgent)
			{
				((MissionBehavior)this).Mission.EndMission();
			}
		}
	}

	private void OnEnemyTeamDefeated()
	{
		_shoulStartOutroConversation = true;
	}

	private void TriggerCombatEnd()
	{
		_shouldShowBanterNotifications = false;
		Agent gunnarAgent = _gunnarAgent;
		CampaignAgentComponent obj = ((gunnarAgent != null) ? gunnarAgent.GetComponent<CampaignAgentComponent>() : null);
		AgentNavigator val = ((obj != null) ? obj.AgentNavigator : null);
		if (val != null)
		{
			AlarmedBehaviorGroup behaviorGroup = val.GetBehaviorGroup<AlarmedBehaviorGroup>();
			if (behaviorGroup != null)
			{
				((AgentBehaviorGroup)behaviorGroup).IsActive = false;
			}
		}
		((MissionBehavior)this).Mission.GetMissionBehavior<NavalStorylineAlleyFightCinematicController>().OnFightEnded();
	}

	public void SetupConversation()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		if (Agent.Main == null || !Agent.Main.IsActive())
		{
			SpawnPlayer();
		}
		GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("conversation_ally")));
		GameEntity val2 = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("conversation_player")));
		if (_gunnarAgent == null || !_gunnarAgent.IsActive())
		{
			SpawnGunnar(val);
		}
		if (val != (GameEntity)null && val2 != (GameEntity)null)
		{
			_gunnarAgent.TeleportToPosition(val.GlobalPosition);
			Agent gunnarAgent = _gunnarAgent;
			Vec3 globalPosition = val.GlobalPosition;
			gunnarAgent.SetTargetPosition(((Vec3)(ref globalPosition)).AsVec2);
			Agent.Main.TeleportToPosition(val2.GlobalPosition);
			Agent.Main.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)1);
			Agent.Main.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)1);
			Agent.Main.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			Agent.Main.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			_gunnarAgent.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)1);
			_gunnarAgent.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)1);
			_gunnarAgent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			_gunnarAgent.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, 0f, 0.4f, 0f, false, -0.2f, 0, true);
			Vec3 val3 = Agent.Main.Position - _gunnarAgent.Position;
			((MissionBehavior)this).Mission.GetMissionBehavior<NavalStorylineAlleyFightCinematicController>().OnConversationSetup(-val3);
			Agent gunnarAgent2 = _gunnarAgent;
			Vec2 val4 = ((Vec3)(ref val3)).AsVec2;
			val4 = ((Vec2)(ref val4)).Normalized();
			gunnarAgent2.SetMovementDirection(ref val4);
			_gunnarAgent.Controller = (AgentControllerType)0;
		}
	}

	public void StartPostFightConversation()
	{
		Campaign.Current.ConversationManager.SetupAndStartMissionConversation((IAgent)(object)_gunnarAgent, (IAgent)(object)((MissionBehavior)this).Mission.MainAgent, true);
		Mission.Current.SetMissionMode((MissionMode)1, true);
	}

	private void ShowNotification(TextObject text, BasicCharacterObject speaker)
	{
		MBInformationManager.AddQuickInformation(text, 0, speaker, ((BasicCharacterObject)NavalStorylineData.Gunnar.CharacterObject).FirstCivilianEquipment, "");
	}

	private void OnPlayerTeamDefeated()
	{
		_isMissionFailed = true;
		_shouldShowEndNotification = true;
	}

	public CharacterObject GetEnemyCharacterObject()
	{
		return ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("naval_storyline_alley_fight_enemy");
	}

	public void OnConversationEnded()
	{
		Mission.Current.EndMission();
	}

	public override bool MissionEnded(ref MissionResult missionResult)
	{
		bool result = false;
		if (_isMissionFailed)
		{
			missionResult = MissionResult.CreateDefeated((IMission)(object)((MissionBehavior)this).Mission);
			result = true;
		}
		return result;
	}

	protected override void OnEndMission()
	{
		CampaignInformationManager.ClearAllDialogNotifications(true);
	}
}
