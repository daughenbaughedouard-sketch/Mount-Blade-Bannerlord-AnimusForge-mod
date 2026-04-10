using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics.Arena;

public class ArenaDuelMissionController : MissionLogic
{
	private CharacterObject _duelCharacter;

	private bool _requireCivilianEquipment;

	private bool _spawnBothSideWithHorses;

	private bool _duelHasEnded;

	private Agent _duelAgent;

	private float _customAgentHealth;

	private BasicMissionTimer _duelEndTimer;

	private MBList<MatrixFrame> _initialSpawnFrames;

	private static Action<CharacterObject> _onDuelEnd;

	public ArenaDuelMissionController(CharacterObject duelCharacter, bool requireCivilianEquipment, bool spawnBothSideWithHorses, Action<CharacterObject> onDuelEnd, float customAgentHealth)
	{
		_duelCharacter = duelCharacter;
		_requireCivilianEquipment = requireCivilianEquipment;
		_spawnBothSideWithHorses = spawnBothSideWithHorses;
		_customAgentHealth = customAgentHealth;
		_onDuelEnd = onDuelEnd;
	}

	public override void AfterStart()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		_duelHasEnded = false;
		_duelEndTimer = new BasicMissionTimer();
		DeactivateOtherTournamentSets();
		InitializeMissionTeams();
		_initialSpawnFrames = Extensions.ToMBList<MatrixFrame>(from e in ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_arena")
			select e.GetGlobalFrame());
		for (int num = 0; num < ((List<MatrixFrame>)(object)_initialSpawnFrames).Count; num++)
		{
			MatrixFrame value = ((List<MatrixFrame>)(object)_initialSpawnFrames)[num];
			((Mat3)(ref value.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			((List<MatrixFrame>)(object)_initialSpawnFrames)[num] = value;
		}
		MatrixFrame randomElement = Extensions.GetRandomElement<MatrixFrame>(_initialSpawnFrames);
		((List<MatrixFrame>)(object)_initialSpawnFrames).Remove(randomElement);
		MatrixFrame randomElement2 = Extensions.GetRandomElement<MatrixFrame>(_initialSpawnFrames);
		SpawnAgent(CharacterObject.PlayerCharacter, randomElement);
		_duelAgent = SpawnAgent(_duelCharacter, randomElement2);
		_duelAgent.Defensiveness = 1f;
	}

	private void InitializeMissionTeams()
	{
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)0, Hero.MainHero.MapFaction.Color, Hero.MainHero.MapFaction.Color2, (Banner)null, true, false, true);
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)1, ((BasicCultureObject)_duelCharacter.Culture).Color, ((BasicCultureObject)_duelCharacter.Culture).Color2, (Banner)null, true, false, true);
		((MissionBehavior)this).Mission.PlayerTeam = ((MissionBehavior)this).Mission.Teams.Defender;
	}

	private void DeactivateOtherTournamentSets()
	{
		TournamentBehavior.DeleteTournamentSetsExcept(((MissionBehavior)this).Mission.Scene.FindEntityWithTag("tournament_fight"));
	}

	private Agent SpawnAgent(CharacterObject character, MatrixFrame spawnFrame)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)character);
		val.BodyProperties(((BasicCharacterObject)character).GetBodyPropertiesMax(false));
		Mission mission = ((MissionBehavior)this).Mission;
		AgentBuildData obj = val.Team((character == CharacterObject.PlayerCharacter) ? ((MissionBehavior)this).Mission.PlayerTeam : ((MissionBehavior)this).Mission.PlayerEnemyTeam).InitialPosition(ref spawnFrame.origin);
		Vec2 val2 = ((Vec3)(ref spawnFrame.rotation.f)).AsVec2;
		val2 = ((Vec2)(ref val2)).Normalized();
		Agent val3 = mission.SpawnAgent(obj.InitialDirection(ref val2).NoHorses(!_spawnBothSideWithHorses).Equipment(_requireCivilianEquipment ? ((BasicCharacterObject)character).FirstCivilianEquipment : ((BasicCharacterObject)character).FirstBattleEquipment)
			.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)character, -1, (Banner)null, default(UniqueTroopDescriptor))), false);
		val3.FadeIn();
		if (character == CharacterObject.PlayerCharacter)
		{
			val3.Controller = (AgentControllerType)2;
		}
		if (val3.IsAIControlled)
		{
			val3.SetWatchState((WatchState)2);
		}
		val3.Health = _customAgentHealth;
		val3.BaseHealthLimit = _customAgentHealth;
		val3.HealthLimit = _customAgentHealth;
		return val3;
	}

	public override void OnMissionTick(float dt)
	{
		if (_duelHasEnded && _duelEndTimer.ElapsedTime > 4f)
		{
			GameTexts.SetVariable("leave_key", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 4), 1f));
			MBInformationManager.AddQuickInformation(GameTexts.FindText("str_duel_has_ended", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
			_duelEndTimer.Reset();
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		if (_onDuelEnd != null)
		{
			_onDuelEnd((affectedAgent == _duelAgent) ? CharacterObject.PlayerCharacter : _duelCharacter);
			_onDuelEnd = null;
			_duelHasEnded = true;
			_duelEndTimer.Reset();
		}
	}

	public override InquiryData OnEndMissionRequest(out bool canPlayerLeave)
	{
		canPlayerLeave = true;
		if (!_duelHasEnded)
		{
			canPlayerLeave = false;
			MBInformationManager.AddQuickInformation(GameTexts.FindText("str_can_not_retreat_duel_ongoing", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		return null;
	}
}
