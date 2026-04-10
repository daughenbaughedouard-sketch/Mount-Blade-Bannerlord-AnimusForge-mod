using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.MissionLogics.Arena;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Issues.IssueQuestTasks;

public class ArenaDuelQuestTask : QuestTaskBase
{
	private Settlement _settlement;

	private CharacterObject _opponentCharacter;

	private Agent _playerAgent;

	private Agent _opponentAgent;

	private bool _duelStarted;

	private BasicMissionTimer _missionEndTimer;

	public ArenaDuelQuestTask(CharacterObject duelOpponentCharacter, Settlement settlement, Action onSucceededAction, Action onFailedAction, DialogFlow dialogFlow = null)
		: base(dialogFlow, onSucceededAction, onFailedAction, (Action)null)
	{
		_opponentCharacter = duelOpponentCharacter;
		_settlement = settlement;
	}

	public void AfterStart(IMission mission)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if (!Mission.Current.HasMissionBehavior<ArenaDuelMissionBehavior>() || PlayerEncounter.LocationEncounter.Settlement != _settlement)
		{
			return;
		}
		InitializeTeams();
		List<MatrixFrame> list = (from e in Mission.Current.Scene.FindEntitiesWithTag("sp_arena_respawn")
			select e.GetGlobalFrame()).ToList();
		MatrixFrame val = list[MBRandom.RandomInt(list.Count)];
		float num = float.MaxValue;
		MatrixFrame frame = val;
		foreach (MatrixFrame item in list)
		{
			MatrixFrame current = item;
			if ((ref val) != (ref current))
			{
				Vec3 origin = current.origin;
				if (((Vec3)(ref origin)).DistanceSquared(val.origin) < num)
				{
					frame = current;
				}
			}
		}
		((Mat3)(ref val.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		((Mat3)(ref frame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		_playerAgent = SpawnArenaAgent(CharacterObject.PlayerCharacter, Mission.Current.PlayerTeam, val);
		_opponentAgent = SpawnArenaAgent(_opponentCharacter, Mission.Current.PlayerEnemyTeam, frame);
	}

	public override void SetReferences()
	{
		CampaignEvents.AfterMissionStarted.AddNonSerializedListener((object)this, (Action<IMission>)AfterStart);
		CampaignEvents.GameMenuOpened.AddNonSerializedListener((object)this, (Action<MenuCallbackArgs>)OnGameMenuOpened);
		CampaignEvents.MissionTickEvent.AddNonSerializedListener((object)this, (Action<float>)MissionTick);
	}

	public void OnGameMenuOpened(MenuCallbackArgs args)
	{
		if (Hero.MainHero.CurrentSettlement != _settlement)
		{
			return;
		}
		if (_duelStarted)
		{
			if (_opponentAgent.IsActive())
			{
				((QuestTaskBase)this).Finish((FinishStates)1);
			}
			else
			{
				((QuestTaskBase)this).Finish((FinishStates)0);
			}
		}
		else
		{
			OpenArenaDuelMission();
		}
	}

	public void MissionTick(float dt)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		if (Mission.Current.HasMissionBehavior<ArenaDuelMissionBehavior>() && PlayerEncounter.LocationEncounter.Settlement == _settlement && ((_playerAgent != null && !_playerAgent.IsActive()) || (_opponentAgent != null && !_opponentAgent.IsActive())))
		{
			if (_missionEndTimer != null && _missionEndTimer.ElapsedTime > 4f)
			{
				Mission.Current.EndMission();
			}
			else if (_missionEndTimer == null && ((_playerAgent != null && !_playerAgent.IsActive()) || (_opponentAgent != null && !_opponentAgent.IsActive())))
			{
				_missionEndTimer = new BasicMissionTimer();
			}
		}
	}

	private void OpenArenaDuelMission()
	{
		Location locationWithId = _settlement.LocationComplex.GetLocationWithId("arena");
		int num = ((!_settlement.IsTown) ? 1 : _settlement.Town.GetWallLevel());
		SandBoxMissions.OpenArenaDuelMission(locationWithId.GetSceneName(num), locationWithId);
		_duelStarted = true;
	}

	private void InitializeTeams()
	{
		Mission.Current.Teams.Add((BattleSideEnum)0, Hero.MainHero.MapFaction.Color, Hero.MainHero.MapFaction.Color2, (Banner)null, true, false, true);
		Mission.Current.Teams.Add((BattleSideEnum)1, Hero.MainHero.MapFaction.Color2, Hero.MainHero.MapFaction.Color, (Banner)null, true, false, true);
		Mission.Current.PlayerTeam = Mission.Current.DefenderTeam;
	}

	private Agent SpawnArenaAgent(CharacterObject character, Team team, MatrixFrame frame)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		if (team == Mission.Current.PlayerTeam)
		{
			character = CharacterObject.PlayerCharacter;
		}
		Equipment randomElement = Extensions.GetRandomElement<Equipment>(_settlement.Culture.DuelPresetEquipmentRoster.AllEquipments);
		Mission current = Mission.Current;
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)character).Team(team).ClothingColor1(team.Color).ClothingColor2(team.Color2)
			.InitialPosition(ref frame.origin);
		Vec2 val = ((Vec3)(ref frame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		Agent val2 = current.SpawnAgent(obj.InitialDirection(ref val).NoHorses(true).Equipment(randomElement)
			.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)character, -1, (Banner)null, default(UniqueTroopDescriptor)))
			.Controller((AgentControllerType)((character != CharacterObject.PlayerCharacter) ? 1 : 2)), false);
		if (val2.IsAIControlled)
		{
			val2.SetWatchState((WatchState)2);
		}
		return val2;
	}
}
