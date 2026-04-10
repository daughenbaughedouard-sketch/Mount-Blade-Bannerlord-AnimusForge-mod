using System.Collections.Generic;
using TaleWorlds.AchievementSystem;
using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade;

public class MultiplayerAchievementComponent : MissionLogic
{
	private struct BoulderKillRecord
	{
		public readonly float Time;

		public BoulderKillRecord(float time)
		{
			Time = time;
		}
	}

	private const float SingleMangonelShotTimeout = 4f;

	private const string MaxMultiKillsWithSingleMangonelShotStatID = "MaxMultiKillsWithSingleMangonelShot";

	private const string KillsWithBoulderStatID = "KillsWithBoulder";

	private const string KillsWithChainAttackStatID = "KillsWithChainAttack";

	private const string KillsWithRangedHeadShotsStatID = "KillsWithRangedHeadshots";

	private const string KillsWithRangedMountedStatID = "KillsWithRangedMounted";

	private const string KillsWithCouchedLanceStatID = "KillsWithCouchedLance";

	private const string KillsWithHorseChargeStatID = "KillsWithHorseCharge";

	private const string KillCountCaptainStatID = "KillCountCaptain";

	private const string KillsWithStolenHorse = "KillsWithStolenHorse";

	private const string SatisfiedJackOfAllTradesStatID = "SatisfiedJackOfAllTrades";

	private const string PushedSomeoneOffLedgeStatID = "PushedSomeoneOffLedge";

	private int _cachedMaxMultiKillsWithSingleMangonelShot;

	private int _cachedKillsWithBoulder;

	private int _cachedKillsWithChainAttack;

	private int _cachedKillsWithRangedHeadShots;

	private int _cachedKillsWithRangedMounted;

	private int _cachedKillsWithCouchedLance;

	private int _cachedKillsWithHorseCharge;

	private int _cachedKillCountCaptain;

	private int _cachedKillsWithStolenHorse;

	private int _singleRoundKillsWithMeleeOnFoot;

	private int _singleRoundKillsWithMeleeMounted;

	private int _singleRoundKillsWithRangedOnFoot;

	private int _singleRoundKillsWithRangedMounted;

	private int _singleRoundKillsWithCouchedLance;

	private int _killsWithAStolenHorse;

	private bool _hasStolenMount;

	private MissionLobbyComponent _missionLobbyComponent;

	private MultiplayerRoundComponent _multiplayerRoundComponent;

	private Queue<BoulderKillRecord> _recentBoulderKills;

	public override void OnBehaviorInitialize()
	{
		_missionLobbyComponent = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();
		_multiplayerRoundComponent = Mission.Current.GetMissionBehavior<MultiplayerRoundComponent>();
		CacheAndInitializeAchievementVariables();
	}

	public override void EarlyStart()
	{
		if (_multiplayerRoundComponent != null)
		{
			_multiplayerRoundComponent.OnRoundStarted += OnRoundStarted;
		}
		if (_recentBoulderKills == null)
		{
			_recentBoulderKills = new Queue<BoulderKillRecord>();
		}
		else
		{
			_recentBoulderKills.Clear();
		}
	}

	protected override void OnEndMission()
	{
		if (_multiplayerRoundComponent != null)
		{
			_multiplayerRoundComponent.OnRoundStarted -= OnRoundStarted;
		}
		_recentBoulderKills?.Clear();
	}

	public override void OnMissionTick(float dt)
	{
		if (_recentBoulderKills == null)
		{
			return;
		}
		while (_recentBoulderKills.Count > 0)
		{
			BoulderKillRecord boulderKillRecord = _recentBoulderKills.Peek();
			if (!(((MissionBehavior)this).Mission.CurrentTime - boulderKillRecord.Time < 4f))
			{
				_recentBoulderKills.Dequeue();
				continue;
			}
			break;
		}
	}

	private void OnRoundStarted()
	{
		_singleRoundKillsWithMeleeOnFoot = 0;
		_singleRoundKillsWithMeleeMounted = 0;
		_singleRoundKillsWithRangedOnFoot = 0;
		_singleRoundKillsWithRangedMounted = 0;
		_singleRoundKillsWithCouchedLance = 0;
		_killsWithAStolenHorse = 0;
		_hasStolenMount = false;
	}

	public override void OnAgentMount(Agent agent)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (agent.IsMine)
		{
			EquipmentElement horse = agent.SpawnEquipment.Horse;
			if (((EquipmentElement)(ref horse)).IsEmpty)
			{
				_hasStolenMount = true;
				_killsWithAStolenHorse = 0;
			}
		}
	}

	public override void OnAgentDismount(Agent agent)
	{
		if (agent.IsMine)
		{
			_hasStolenMount = false;
			_killsWithAStolenHorse = 0;
		}
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if (agent.IsMine)
		{
			_hasStolenMount = false;
			_killsWithAStolenHorse = 0;
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected I4, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Invalid comparison between Unknown and I4
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Invalid comparison between Unknown and I4
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Invalid comparison between Unknown and I4
		if (affectedAgent == null || affectedAgent.IsMount)
		{
			return;
		}
		if ((int)agentState == 4)
		{
			if (affectorAgent != null && affectorAgent.IsMine && affectorAgent != affectedAgent && !affectedAgent.IsFriendOf(affectorAgent))
			{
				WeaponClass val = (WeaponClass)blow.WeaponClass;
				int num = (int)val;
				bool flag = num >= 1 && num <= 11;
				bool isMissile = blow.IsMissile;
				if ((int)val == 20 || (int)val == 26)
				{
					_recentBoulderKills.Enqueue(new BoulderKillRecord(((MissionBehavior)this).Mission.CurrentTime));
					if (_recentBoulderKills.Count > 1 && _recentBoulderKills.Count > _cachedMaxMultiKillsWithSingleMangonelShot)
					{
						_cachedMaxMultiKillsWithSingleMangonelShot = _recentBoulderKills.Count;
						SetStatInternal("MaxMultiKillsWithSingleMangonelShot", _cachedMaxMultiKillsWithSingleMangonelShot);
					}
					_cachedKillsWithBoulder++;
					SetStatInternal("KillsWithBoulder", _cachedKillsWithBoulder);
				}
				if ((int)blow.AttackType == 1 && (int)blow.OverrideKillInfo == 21)
				{
					SetStatInternal("PushedSomeoneOffLedge", 1);
				}
				if (isMissile && ((KillingBlow)(ref blow)).IsHeadShot())
				{
					_cachedKillsWithRangedHeadShots++;
					SetStatInternal("KillsWithRangedHeadshots", _cachedKillsWithRangedHeadShots);
				}
				if (affectorAgent.IsReleasingChainAttackInMultiplayer())
				{
					_cachedKillsWithChainAttack++;
					SetStatInternal("KillsWithChainAttack", _cachedKillsWithChainAttack);
				}
				if (affectorAgent.HasMount)
				{
					if (affectorAgent.IsDoingPassiveAttack)
					{
						_singleRoundKillsWithCouchedLance++;
						_cachedKillsWithCouchedLance++;
						SetStatInternal("KillsWithCouchedLance", _cachedKillsWithCouchedLance);
					}
					if (isMissile)
					{
						_singleRoundKillsWithRangedMounted++;
						_cachedKillsWithRangedMounted++;
						SetStatInternal("KillsWithRangedMounted", _cachedKillsWithRangedMounted);
					}
					if (flag)
					{
						_singleRoundKillsWithMeleeMounted++;
					}
					if (!flag && !isMissile)
					{
						_cachedKillsWithHorseCharge++;
						SetStatInternal("KillsWithHorseCharge", _cachedKillsWithHorseCharge);
					}
					if (_hasStolenMount)
					{
						_killsWithAStolenHorse++;
						if (_killsWithAStolenHorse > _cachedKillsWithStolenHorse)
						{
							_cachedKillsWithStolenHorse = _killsWithAStolenHorse;
							SetStatInternal("KillsWithStolenHorse", _cachedKillsWithStolenHorse);
						}
					}
				}
				else
				{
					if (isMissile)
					{
						_singleRoundKillsWithRangedOnFoot++;
					}
					if (flag)
					{
						_singleRoundKillsWithMeleeOnFoot++;
					}
				}
				if ((int)_missionLobbyComponent.MissionType == 5 && _singleRoundKillsWithMeleeOnFoot > 0 && _singleRoundKillsWithMeleeMounted > 0 && _singleRoundKillsWithRangedOnFoot > 0 && _singleRoundKillsWithRangedMounted > 0 && _singleRoundKillsWithCouchedLance > 0)
				{
					SetStatInternal("SatisfiedJackOfAllTrades", 1);
				}
			}
			NetworkCommunicator myPeer = GameNetwork.MyPeer;
			MissionPeer val2 = ((myPeer != null) ? PeerExtensions.GetComponent<MissionPeer>(myPeer) : null);
			if (val2 != null && (int)_missionLobbyComponent.MissionType == 4 && (((affectorAgent != null) ? affectorAgent.MissionPeer : null) == val2 || ((affectorAgent != null) ? affectorAgent.OwningAgentMissionPeer : null) == val2))
			{
				_cachedKillCountCaptain++;
				SetStatInternal("KillCountCaptain", _cachedKillCountCaptain);
			}
		}
		if (affectedAgent.IsMine)
		{
			_hasStolenMount = false;
			_killsWithAStolenHorse = 0;
		}
	}

	private async void CacheAndInitializeAchievementVariables()
	{
		int[] array = await AchievementManager.GetStats(new string[9] { "MaxMultiKillsWithSingleMangonelShot", "KillsWithBoulder", "KillsWithChainAttack", "KillsWithRangedHeadshots", "KillsWithRangedMounted", "KillsWithCouchedLance", "KillsWithHorseCharge", "KillCountCaptain", "KillsWithStolenHorse" });
		if (array != null)
		{
			int num = 0;
			_cachedMaxMultiKillsWithSingleMangonelShot += array[num++];
			_cachedKillsWithBoulder += array[num++];
			_cachedKillsWithChainAttack += array[num++];
			_cachedKillsWithRangedHeadShots += array[num++];
			_cachedKillsWithRangedMounted += array[num++];
			_cachedKillsWithCouchedLance += array[num++];
			_cachedKillsWithHorseCharge += array[num++];
			_cachedKillCountCaptain += array[num++];
			_cachedKillsWithStolenHorse += array[num];
		}
	}

	private void SetStatInternal(string statId, int value)
	{
		AchievementManager.SetStat(statId, value);
	}
}
