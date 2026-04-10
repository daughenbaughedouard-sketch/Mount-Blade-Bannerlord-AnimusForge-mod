using System;
using System.Collections.Generic;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace TaleWorlds.MountAndBlade;

public class NavalBattleEndLogic : MissionLogic, IBattleEndLogic
{
	public enum ExitResult
	{
		False,
		NeedsPlayerConfirmation,
		True
	}

	public const float DefaultContestedIslandsCheckDuration = 20f;

	public const float RetreatCheckDuration = 5f;

	public const float MainAgentConsideredDeadDuration = 20f;

	public const int MinTroopCountForOutOfActionCheck = 3;

	private IMissionAgentSpawnLogic _missionSpawnLogic;

	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	private bool _notificationsDisabled;

	private MissionTime _enemySideNotYetRetreatingTime;

	private MissionTime _playerSideNotYetRetreatingTime;

	private MissionTime _contestedIslandCheckTimer;

	private MissionTime _mainAgentIsDeadTimer;

	private float _contestedIslandsCheckDuration = 20f;

	private bool _isInContestedIslandsCheckPhase;

	private BasicMissionTimer _checkDepletionOrRetreatingTimer;

	private bool _isPlayerSideRetreating;

	private bool _isEnemySideDepleted;

	private bool _isPlayerSideDepleted;

	private bool _missionEndedMessageShown;

	private bool _victoryReactionsActivated;

	private bool _victoryReactionsActivatedForRetreating;

	private bool _scoreBoardOpenedOnceOnMissionEnd;

	public bool PlayerVictory
	{
		get
		{
			if (!IsEnemySideRetreating)
			{
				return _isEnemySideDepleted;
			}
			return true;
		}
	}

	public bool EnemyVictory
	{
		get
		{
			if (!_isPlayerSideRetreating)
			{
				return _isPlayerSideDepleted;
			}
			return true;
		}
	}

	public bool IsEnemySideRetreating { get; private set; }

	public bool CanCheckForEndCondition { get; private set; }

	public override void OnBehaviorInitialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		((MissionBehavior)this).OnBehaviorInitialize();
		_checkDepletionOrRetreatingTimer = new BasicMissionTimer();
		_missionSpawnLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<IMissionAgentSpawnLogic>();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_navalShipsLogic.MissionEndEvent += OnMissionEnd;
	}

	public override void OnDeploymentFinished()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_contestedIslandCheckTimer = MissionTime.Now;
		_mainAgentIsDeadTimer = MissionTime.Now;
		CanCheckForEndCondition = true;
	}

	public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionBehavior)this).Mission.IsDeploymentFinished && affectedAgent == Agent.Main)
		{
			_mainAgentIsDeadTimer = MissionTime.Now;
		}
	}

	public override void OnAgentControllerSetToPlayer(Agent agent)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionBehavior)this).Mission.IsDeploymentFinished && agent.IsActive())
		{
			_mainAgentIsDeadTimer = MissionTime.Now;
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		if (!((MissionBehavior)this).Mission.IsDeploymentFinished)
		{
			return;
		}
		if (((MissionBehavior)this).Mission.IsMissionEnding)
		{
			if (_notificationsDisabled)
			{
				_scoreBoardOpenedOnceOnMissionEnd = true;
			}
			if (_missionEndedMessageShown && !_scoreBoardOpenedOnceOnMissionEnd)
			{
				if (_checkDepletionOrRetreatingTimer.ElapsedTime > 7f)
				{
					CheckIsEnemySideRetreatingOrOneSideDepleted(forceCheckContestedIslands: true);
					_checkDepletionOrRetreatingTimer.Reset();
					if (((MissionBehavior)this).Mission.MissionResult != null && ((MissionBehavior)this).Mission.MissionResult.PlayerDefeated)
					{
						GameTexts.SetVariable("leave_key", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 4), 1f));
						MBInformationManager.AddQuickInformation(GameTexts.FindText("str_battle_lost_press_tab_to_view_results", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					}
					else if (((MissionBehavior)this).Mission.MissionResult != null && ((MissionBehavior)this).Mission.MissionResult.PlayerVictory)
					{
						if (_isEnemySideDepleted)
						{
							GameTexts.SetVariable("leave_key", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 4), 1f));
							MBInformationManager.AddQuickInformation(GameTexts.FindText("str_battle_won_press_tab_to_view_results", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
						}
					}
					else
					{
						GameTexts.SetVariable("leave_key", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 4), 1f));
						MBInformationManager.AddQuickInformation(GameTexts.FindText("str_battle_finished_press_tab_to_view_results", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					}
				}
			}
			else if (_checkDepletionOrRetreatingTimer.ElapsedTime > 3f && !_scoreBoardOpenedOnceOnMissionEnd)
			{
				if (((MissionBehavior)this).Mission.MissionResult != null && ((MissionBehavior)this).Mission.MissionResult.PlayerDefeated)
				{
					if (_isPlayerSideDepleted)
					{
						MBInformationManager.AddQuickInformation(GameTexts.FindText("str_battle_lost", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					}
					else if (_isPlayerSideRetreating)
					{
						MBInformationManager.AddQuickInformation(GameTexts.FindText("str_friendlies_are_fleeing_you_lost", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					}
				}
				else if (((MissionBehavior)this).Mission.MissionResult != null && ((MissionBehavior)this).Mission.MissionResult.PlayerVictory)
				{
					if (_isEnemySideDepleted)
					{
						MBInformationManager.AddQuickInformation(GameTexts.FindText("str_battle_won", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					}
					else if (IsEnemySideRetreating)
					{
						MBInformationManager.AddQuickInformation(GameTexts.FindText("str_enemies_are_fleeing_you_won", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					}
				}
				else
				{
					MBInformationManager.AddQuickInformation(GameTexts.FindText("str_battle_finished", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
				}
				_missionEndedMessageShown = true;
				_checkDepletionOrRetreatingTimer.Reset();
			}
			if (_victoryReactionsActivated)
			{
				return;
			}
			AgentVictoryLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<AgentVictoryLogic>();
			if (missionBehavior != null)
			{
				CheckIsEnemySideRetreatingOrOneSideDepleted(forceCheckContestedIslands: true);
				if (_isEnemySideDepleted)
				{
					missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(((MissionBehavior)this).Mission.PlayerTeam.Side);
					_victoryReactionsActivated = true;
				}
				else if (_isPlayerSideDepleted)
				{
					missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(((MissionBehavior)this).Mission.PlayerEnemyTeam.Side);
					_victoryReactionsActivated = true;
				}
				else if (IsEnemySideRetreating && !_victoryReactionsActivatedForRetreating)
				{
					missionBehavior.SetTimersOfVictoryReactionsOnRetreat(((MissionBehavior)this).Mission.PlayerTeam.Side);
					_victoryReactionsActivatedForRetreating = true;
				}
				else if (_isPlayerSideRetreating && !_victoryReactionsActivatedForRetreating)
				{
					missionBehavior.SetTimersOfVictoryReactionsOnRetreat(((MissionBehavior)this).Mission.PlayerEnemyTeam.Side);
					_victoryReactionsActivatedForRetreating = true;
				}
			}
		}
		else if (_checkDepletionOrRetreatingTimer.ElapsedTime > 1f)
		{
			CheckIsEnemySideRetreatingOrOneSideDepleted();
			if (_isInContestedIslandsCheckPhase)
			{
				_contestedIslandsCheckDuration = 5f;
			}
			else
			{
				_contestedIslandsCheckDuration = 20f;
			}
			_checkDepletionOrRetreatingTimer.Reset();
		}
	}

	public override bool MissionEnded(ref MissionResult missionResult)
	{
		bool flag = false;
		if (IsEnemySideRetreating || _isEnemySideDepleted)
		{
			missionResult = MissionResult.CreateSuccessful((IMission)(object)((MissionBehavior)this).Mission, IsEnemySideRetreating);
			flag = true;
		}
		else if (_isPlayerSideRetreating || _isPlayerSideDepleted)
		{
			missionResult = MissionResult.CreateDefeated((IMission)(object)((MissionBehavior)this).Mission);
			flag = true;
		}
		if (flag)
		{
			_missionSpawnLogic.StopSpawner((BattleSideEnum)1);
			_missionSpawnLogic.StopSpawner((BattleSideEnum)0);
		}
		return flag;
	}

	public override void OnMissionStateFinalized()
	{
		_navalShipsLogic.MissionEndEvent -= OnMissionEnd;
	}

	private void OnMissionEnd()
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnemySideRetreating)
		{
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents)
			{
				IAgentOriginBase origin = item.Origin;
				if (origin != null)
				{
					origin.SetRouted(true);
				}
			}
			MBList<MissionShip> val = new MBList<MissionShip>();
			_navalShipsLogic.FillTeamShips((TeamSideEnum)2, val);
			MBList<IAgentOriginBase> val2 = new MBList<IAgentOriginBase>();
			foreach (MissionShip item2 in (List<MissionShip>)(object)val)
			{
				_navalAgentsLogic.FillReservedTroopsOfShip(item2, val2);
			}
			foreach (IAgentOriginBase item3 in (List<IAgentOriginBase>)(object)val2)
			{
				item3.SetRouted(true);
			}
		}
		if (Campaign.Current == null || PlayerEncounter.Current == null)
		{
			return;
		}
		MBReadOnlyList<MapEventParty> val3 = new MBReadOnlyList<MapEventParty>();
		if (IsEnemySideRetreating || _isEnemySideDepleted)
		{
			val3 = PlayerEncounter.Battle.PartiesOnSide(Extensions.GetOppositeSide(PlayerEncounter.Battle.PlayerSide));
		}
		else if (_isPlayerSideRetreating || _isPlayerSideDepleted)
		{
			val3 = PlayerEncounter.Battle.PartiesOnSide(PlayerEncounter.Battle.PlayerSide);
		}
		Ship shipToCapture = default(Ship);
		foreach (MissionShip item4 in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			ref Ship reference = ref shipToCapture;
			IShipOrigin shipOrigin = item4.ShipOrigin;
			if ((reference = (Ship)(object)((shipOrigin is Ship) ? shipOrigin : null)) != null && LinQuick.ContainsQ<MapEventParty>((List<MapEventParty>)(object)val3, (Func<MapEventParty, bool>)((MapEventParty x) => x.Party == shipToCapture.Owner)))
			{
				PlayerEncounter.Current.CapturedShipsInEncounter.Add(shipToCapture);
			}
		}
	}

	public ExitResult TryExit()
	{
		if (GameNetwork.IsClientOrReplay)
		{
			return ExitResult.False;
		}
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		if ((mainAgent != null && mainAgent.IsActive() && ((MissionBehavior)this).Mission.IsPlayerCloseToAnEnemy(5f)) || (!((MissionBehavior)this).Mission.MissionEnded && (PlayerVictory || EnemyVictory)))
		{
			return ExitResult.False;
		}
		if (!((MissionBehavior)this).Mission.MissionEnded && !IsEnemySideRetreating)
		{
			return ExitResult.NeedsPlayerConfirmation;
		}
		((MissionBehavior)this).Mission.EndMission();
		return ExitResult.True;
	}

	public void SetNotificationDisabled(bool value)
	{
		_notificationsDisabled = value;
	}

	private void CheckIsEnemySideRetreatingOrOneSideDepleted(bool forceCheckContestedIslands = false)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		if (!CanCheckForEndCondition)
		{
			return;
		}
		BattleSideEnum side = ((MissionBehavior)this).Mission.PlayerTeam.Side;
		BattleSideEnum oppositeSide = Extensions.GetOppositeSide(side);
		if (_missionSpawnLogic.IsSideDepleted(side))
		{
			_isPlayerSideDepleted = true;
		}
		if (_missionSpawnLogic.IsSideDepleted(oppositeSide))
		{
			_isEnemySideDepleted = true;
		}
		if (_isEnemySideDepleted || _isPlayerSideDepleted)
		{
			return;
		}
		if (AreAnySideShipsOutOfAction(side, oppositeSide, out var playerShipsOutOfAction, out var enemyShipsOutOfAction))
		{
			_isInContestedIslandsCheckPhase = ((MissionTime)(ref _contestedIslandCheckTimer)).ElapsedSeconds > _contestedIslandsCheckDuration;
			if (forceCheckContestedIslands || _isInContestedIslandsCheckPhase)
			{
				if (!HasAnyContestedIslands(side, oppositeSide))
				{
					Agent main = Agent.Main;
					bool flag = (main == null || !main.IsActive()) && ((MissionTime)(ref _mainAgentIsDeadTimer)).ElapsedSeconds > 20f;
					if (playerShipsOutOfAction && flag)
					{
						_isPlayerSideDepleted = true;
					}
					if (enemyShipsOutOfAction)
					{
						_isEnemySideDepleted = true;
					}
				}
				_contestedIslandCheckTimer = MissionTime.Now;
			}
		}
		else
		{
			_isInContestedIslandsCheckPhase = false;
			_contestedIslandCheckTimer = MissionTime.Now;
		}
		if (_isEnemySideDepleted || _isPlayerSideDepleted)
		{
			return;
		}
		if (((MissionBehavior)this).Mission.MainAgent != null && ((MissionBehavior)this).Mission.MainAgent.IsPlayerControlled && ((MissionBehavior)this).Mission.MainAgent.IsActive())
		{
			_playerSideNotYetRetreatingTime = MissionTime.Now;
		}
		else
		{
			bool flag2 = true;
			foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
			{
				if (item.Team != null && item.Team.Side == side && !item.IsRetreating)
				{
					flag2 = false;
					break;
				}
			}
			if (!flag2)
			{
				_playerSideNotYetRetreatingTime = MissionTime.Now;
			}
		}
		if (((MissionTime)(ref _playerSideNotYetRetreatingTime)).ElapsedSeconds > 5f)
		{
			_isPlayerSideRetreating = true;
		}
		bool flag3 = true;
		foreach (MissionShip item2 in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (item2.Team != null && item2.Team.Side == oppositeSide && !item2.IsRetreating)
			{
				flag3 = false;
				break;
			}
		}
		if (!flag3)
		{
			_enemySideNotYetRetreatingTime = MissionTime.Now;
		}
		if (((MissionTime)(ref _enemySideNotYetRetreatingTime)).ElapsedSeconds > 5f)
		{
			IsEnemySideRetreating = true;
		}
	}

	private bool AreAnySideShipsOutOfAction(BattleSideEnum playerSide, BattleSideEnum enemySide, out bool playerShipsOutOfAction, out bool enemyShipsOutOfAction)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		playerShipsOutOfAction = false;
		enemyShipsOutOfAction = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (MissionShip item in (List<MissionShip>)(object)_navalShipsLogic.AllShips)
		{
			if (item.Team == null)
			{
				continue;
			}
			if (item.Team.Side == playerSide)
			{
				num++;
				bool flag = false;
				if (item.IsSunk)
				{
					flag = true;
				}
				else if (_navalAgentsLogic.GetTotalTroopCountOfShip(item, spawnableReservesOnly: true) <= 3)
				{
					flag = true;
				}
				if (flag)
				{
					num3++;
				}
			}
			else if (item.Team.Side == enemySide)
			{
				num2++;
				bool flag2 = false;
				if (item.IsSunk)
				{
					flag2 = true;
				}
				else if (_navalAgentsLogic.GetTotalTroopCountOfShip(item, spawnableReservesOnly: true) <= 3)
				{
					flag2 = true;
				}
				if (flag2)
				{
					num4++;
				}
			}
		}
		if (num > 0)
		{
			playerShipsOutOfAction = num3 == num;
		}
		if (num2 > 0)
		{
			enemyShipsOutOfAction = num4 == num2;
		}
		return playerShipsOutOfAction | enemyShipsOutOfAction;
	}

	private bool HasAnyContestedIslands(BattleSideEnum playerSide, BattleSideEnum enemySide)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		ulong num = 0uL;
		ulong num2 = 0uL;
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.AllAgents)
		{
			if (!item.IsActive() || !item.IsHuman || item.Team == null)
			{
				continue;
			}
			AgentNavalComponent component = item.GetComponent<AgentNavalComponent>();
			if (component == null)
			{
				continue;
			}
			ulong steppedCombinedShipIsland = component.GetSteppedCombinedShipIsland();
			if (steppedCombinedShipIsland != 0L)
			{
				BattleSideEnum side = item.Team.Side;
				if (side == playerSide)
				{
					num |= steppedCombinedShipIsland;
				}
				else if (side == enemySide)
				{
					num2 |= steppedCombinedShipIsland;
				}
				if ((num & num2) != 0L)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void OnMissionResultReady(MissionResult missionResult)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			item.SetAgentFlags((AgentFlag)(item.GetAgentFlags() & -9));
		}
	}
}
