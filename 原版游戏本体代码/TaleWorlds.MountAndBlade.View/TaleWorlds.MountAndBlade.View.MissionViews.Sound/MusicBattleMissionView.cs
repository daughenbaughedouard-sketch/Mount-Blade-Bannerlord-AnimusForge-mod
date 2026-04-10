using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using psai.net;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Sound;

public class MusicBattleMissionView : MissionView, IMusicHandler
{
	private enum BattleState
	{
		Starting,
		Started,
		TurnedOneSide,
		Ending
	}

	private const float ChargeOrderIntensityIncreaseCooldownInSeconds = 60f;

	private BattleState _battleState;

	private MissionAgentSpawnLogic _missionAgentSpawnLogic;

	private int[] _startingTroopCounts;

	private float _startingBattleRatio;

	private bool _isSiegeBattle;

	private bool _isPaganBattle;

	private MissionTime _nextPossibleTimeToIncreaseIntensityForChargeOrder;

	bool IMusicHandler.IsPausable => false;

	private BattleSideEnum PlayerSide
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			Team playerTeam = Mission.Current.PlayerTeam;
			if (playerTeam == null)
			{
				return (BattleSideEnum)(-1);
			}
			return playerTeam.Side;
		}
	}

	public MusicBattleMissionView(bool isSiegeBattle)
	{
		_isSiegeBattle = isSiegeBattle;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_missionAgentSpawnLogic = Mission.Current.GetMissionBehavior<MissionAgentSpawnLogic>();
		MBMusicManager.Current.DeactivateCurrentMode();
		MBMusicManager.Current.ActivateBattleMode();
		MBMusicManager.Current.OnBattleMusicHandlerInit((IMusicHandler)(object)this);
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		MBMusicManager.Current.DeactivateBattleMode();
		MBMusicManager.Current.OnBattleMusicHandlerFinalize();
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.OnOrderIssued -= new OnOrderIssuedDelegate(PlayerOrderControllerOnOrderIssued);
	}

	public override void AfterStart()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		_nextPossibleTimeToIncreaseIntensityForChargeOrder = MissionTime.Now;
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.OnOrderIssued += new OnOrderIssuedDelegate(PlayerOrderControllerOnOrderIssued);
	}

	private void PlayerOrderControllerOnOrderIssued(OrderType orderType, IEnumerable<Formation> appliedFormations, OrderController orderController, object[] parameters)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (((int)orderType == 4 || (int)orderType == 5) && ((MissionTime)(ref _nextPossibleTimeToIncreaseIntensityForChargeOrder)).IsPast)
		{
			float currentIntensity = PsaiCore.Instance.GetCurrentIntensity();
			float num = currentIntensity * MusicParameters.PlayerChargeEffectMultiplierOnIntensity - currentIntensity;
			MBMusicManager.Current.ChangeCurrentThemeIntensity(num);
			_nextPossibleTimeToIncreaseIntensityForChargeOrder = MissionTime.Now + MissionTime.Seconds(60f);
		}
	}

	private void CheckIntensityFall()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
		if (((PsaiInfo)(ref psaiInfo)).effectiveThemeId >= 0)
		{
			if (float.IsNaN(((PsaiInfo)(ref psaiInfo)).currentIntensity))
			{
				MBMusicManager.Current.ChangeCurrentThemeIntensity(MusicParameters.MinIntensity);
			}
			else if (((PsaiInfo)(ref psaiInfo)).currentIntensity < MusicParameters.MinIntensity)
			{
				MBMusicManager.Current.ChangeCurrentThemeIntensity(MusicParameters.MinIntensity - ((PsaiInfo)(ref psaiInfo)).currentIntensity);
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Invalid comparison between Unknown and I4
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between I4 and Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Invalid comparison between Unknown and I4
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Invalid comparison between Unknown and I4
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Invalid comparison between Unknown and I4
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		if (_battleState == BattleState.Starting)
		{
			return;
		}
		bool flag = affectedAgent.IsMine || (affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsMine);
		Team team = affectedAgent.Team;
		BattleSideEnum val = (BattleSideEnum)((team == null) ? (-1) : ((int)team.Side));
		int num;
		if (!flag)
		{
			if ((int)val != -1)
			{
				Team playerTeam = Mission.Current.PlayerTeam;
				num = ((((playerTeam == null) ? (-1) : ((int)playerTeam.Side)) == (int)val) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
		}
		else
		{
			num = 1;
		}
		bool flag2 = (byte)num != 0;
		if (!_isSiegeBattle && affectedAgent.IsHuman && (int)val != -1 && _battleState == BattleState.Started && _startingTroopCounts.Sum() >= MusicParameters.SmallBattleTreshold)
		{
			MissionTime now = MissionTime.Now;
			if (((MissionTime)(ref now)).ToSeconds > (double)MusicParameters.BattleTurnsOneSideCooldown && _missionAgentSpawnLogic.NumberOfRemainingTroops == 0)
			{
				int[] array = new int[2] { _missionAgentSpawnLogic.NumberOfActiveDefenderTroops, _missionAgentSpawnLogic.NumberOfActiveAttackerTroops };
				array[val]--;
				MusicTheme val2 = (MusicTheme)(-1);
				if (array[0] > 0 && array[1] > 0)
				{
					float num2 = (float)array[0] / (float)array[1];
					if (num2 < _startingBattleRatio * MusicParameters.BattleRatioTresholdOnIntensity)
					{
						val2 = MBMusicManager.Current.GetBattleTurnsOneSideTheme(((MissionBehavior)this).Mission.MusicCulture, (int)PlayerSide > 0, _isPaganBattle);
					}
					else if (num2 > _startingBattleRatio / MusicParameters.BattleRatioTresholdOnIntensity)
					{
						val2 = MBMusicManager.Current.GetBattleTurnsOneSideTheme(((MissionBehavior)this).Mission.MusicCulture, (int)PlayerSide == 0, _isPaganBattle);
					}
				}
				if ((int)val2 != -1)
				{
					MBMusicManager.Current.StartTheme(val2, PsaiCore.Instance.GetCurrentIntensity(), false);
					_battleState = BattleState.TurnedOneSide;
				}
			}
		}
		if ((affectedAgent.IsHuman && (int)affectedAgent.State != 2) || flag)
		{
			float num3 = (flag2 ? MusicParameters.FriendlyTroopDeadEffectOnIntensity : MusicParameters.EnemyTroopDeadEffectOnIntensity);
			if (flag)
			{
				num3 *= MusicParameters.PlayerTroopDeadEffectMultiplierOnIntensity;
			}
			MBMusicManager.Current.ChangeCurrentThemeIntensity(num3);
		}
	}

	private void CheckForStarting()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		if (_startingTroopCounts == null)
		{
			_startingTroopCounts = new int[2]
			{
				_missionAgentSpawnLogic.GetTotalNumberOfTroopsForSide((BattleSideEnum)0),
				_missionAgentSpawnLogic.GetTotalNumberOfTroopsForSide((BattleSideEnum)1)
			};
			_startingBattleRatio = (float)_startingTroopCounts[0] / (float)_startingTroopCounts[1];
		}
		Agent main = Agent.Main;
		Vec2 val;
		if (main == null)
		{
			val = Vec2.Invalid;
		}
		else
		{
			Vec3 position = main.Position;
			val = ((Vec3)(ref position)).AsVec2;
		}
		Vec2 val2 = val;
		Team playerTeam = Mission.Current.PlayerTeam;
		bool flag = playerTeam != null && ((IEnumerable<Formation>)playerTeam.FormationsIncludingEmpty).Any((Formation f) => f.CountOfUnits > 0);
		float num = float.MaxValue;
		if (flag || ((Vec2)(ref val2)).IsValid)
		{
			foreach (Formation item in (List<Formation>)(object)Mission.Current.PlayerEnemyTeam.FormationsIncludingEmpty)
			{
				if (item.CountOfUnits <= 0)
				{
					continue;
				}
				float num2 = float.MaxValue;
				if (!flag && ((Vec2)(ref val2)).IsValid)
				{
					num2 = ((Vec2)(ref val2)).DistanceSquared(item.CurrentPosition);
				}
				else if (flag)
				{
					foreach (Formation item2 in (List<Formation>)(object)Mission.Current.PlayerTeam.FormationsIncludingEmpty)
					{
						if (item2.CountOfUnits > 0)
						{
							Vec2 currentPosition = item2.CurrentPosition;
							float num3 = ((Vec2)(ref currentPosition)).DistanceSquared(item.CurrentPosition);
							if (num2 > num3)
							{
								num2 = num3;
							}
						}
					}
				}
				if (num > num2)
				{
					num = num2;
				}
			}
		}
		int num4 = _startingTroopCounts.Sum();
		bool flag2 = false;
		if (num4 < MusicParameters.SmallBattleTreshold)
		{
			if (num < MusicParameters.SmallBattleDistanceTreshold * MusicParameters.SmallBattleDistanceTreshold)
			{
				flag2 = true;
			}
		}
		else if (num4 < MusicParameters.MediumBattleTreshold)
		{
			if (num < MusicParameters.MediumBattleDistanceTreshold * MusicParameters.MediumBattleDistanceTreshold)
			{
				flag2 = true;
			}
		}
		else if (num4 < MusicParameters.LargeBattleTreshold)
		{
			if (num < MusicParameters.LargeBattleDistanceTreshold * MusicParameters.LargeBattleDistanceTreshold)
			{
				flag2 = true;
			}
		}
		else if (num < MusicParameters.MaxBattleDistanceTreshold * MusicParameters.MaxBattleDistanceTreshold)
		{
			flag2 = true;
		}
		if (flag2)
		{
			float num5 = (float)num4 / 1000f;
			float num6 = MusicParameters.DefaultStartIntensity + num5 * MusicParameters.BattleSizeEffectOnStartIntensity + (MBRandom.RandomFloat - 0.5f) * (MusicParameters.RandomEffectMultiplierOnStartIntensity * 2f);
			MusicTheme val3 = (_isSiegeBattle ? MBMusicManager.Current.GetSiegeTheme(((MissionBehavior)this).Mission.MusicCulture) : MBMusicManager.Current.GetBattleTheme(((MissionBehavior)this).Mission.MusicCulture, num4, ref _isPaganBattle));
			MBMusicManager.Current.StartTheme(val3, num6, false);
			_battleState = BattleState.Started;
		}
	}

	private void CheckForEnding()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.Current.IsMissionEnding)
		{
			PsaiInfo psaiInfo;
			if (Mission.Current.MissionResult != null)
			{
				MusicTheme battleEndTheme = MBMusicManager.Current.GetBattleEndTheme(((MissionBehavior)this).Mission.MusicCulture, Mission.Current.MissionResult.PlayerVictory);
				MBMusicManager current = MBMusicManager.Current;
				psaiInfo = PsaiCore.Instance.GetPsaiInfo();
				current.StartTheme(battleEndTheme, ((PsaiInfo)(ref psaiInfo)).currentIntensity, true);
				_battleState = BattleState.Ending;
			}
			else
			{
				MBMusicManager current2 = MBMusicManager.Current;
				psaiInfo = PsaiCore.Instance.GetPsaiInfo();
				current2.StartTheme((MusicTheme)26, ((PsaiInfo)(ref psaiInfo)).currentIntensity, true);
				_battleState = BattleState.Ending;
			}
		}
	}

	void IMusicHandler.OnUpdated(float dt)
	{
		if (_battleState == BattleState.Starting)
		{
			if (((MissionBehavior)this).Mission.MusicCulture == null && Mission.Current.GetMissionBehavior<DeploymentHandler>() == null && _missionAgentSpawnLogic.IsDeploymentOver)
			{
				KeyValuePair<BasicCultureObject, int> keyValuePair = new KeyValuePair<BasicCultureObject, int>(null, -1);
				Dictionary<BasicCultureObject, int> dictionary = new Dictionary<BasicCultureObject, int>();
				foreach (Team item in (List<Team>)(object)((MissionBehavior)this).Mission.Teams)
				{
					foreach (Agent item2 in (List<Agent>)(object)item.ActiveAgents)
					{
						BasicCultureObject culture = item2.Character.Culture;
						if (culture != null && culture.IsMainCulture)
						{
							if (!dictionary.ContainsKey(item2.Character.Culture))
							{
								dictionary.Add(item2.Character.Culture, 0);
							}
							dictionary[item2.Character.Culture]++;
							if (dictionary[item2.Character.Culture] > keyValuePair.Value)
							{
								keyValuePair = new KeyValuePair<BasicCultureObject, int>(item2.Character.Culture, dictionary[item2.Character.Culture]);
							}
						}
					}
				}
				if (keyValuePair.Key != null)
				{
					((MissionBehavior)this).Mission.MusicCulture = keyValuePair.Key;
				}
				else
				{
					((MissionBehavior)this).Mission.MusicCulture = Game.Current.PlayerTroop.Culture;
				}
			}
			if (((MissionBehavior)this).Mission.MusicCulture != null)
			{
				CheckForStarting();
			}
		}
		if (_battleState == BattleState.Started || _battleState == BattleState.TurnedOneSide)
		{
			CheckForEnding();
		}
		CheckIntensityFall();
	}
}
