using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;
using psai.net;

namespace NavalDLC.View;

internal class MusicNavalBattleMissionView : MissionView, IMusicHandler
{
	private enum BattleState
	{
		Starting,
		Started,
		TurnedOneSide,
		Ending
	}

	private enum NavalBattleThemes
	{
		VikingSeaBattle1 = 10241,
		VikingSeaBattle2,
		MediterraneanSeaBattle1,
		Maintheme,
		MediterraneanSeaBattle2
	}

	private const float ChargeOrderIntensityIncreaseCooldownInSeconds = 60f;

	private const float BattleSizeEffectOnStartIntensity = 0.8f;

	private const string CultureSturgia = "sturgia";

	private const string CultureBattania = "battania";

	private const string CultureNord = "nord";

	private BattleState _battleState;

	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	private float _waterStrengthIntensityMultiplier;

	private float _mainAgentBaseHealth;

	private int[] _startingTroopCounts;

	private MissionTime _nextPossibleTimeToIncreaseIntensityForChargeOrder;

	bool IMusicHandler.IsPausable => false;

	private MatrixFrame _listenerGlobalFrame => SoundManager.GetListenerFrame();

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = Mission.Current.GetMissionBehavior<NavalAgentsLogic>();
		_navalShipsLogic.ShipSunkEvent += OnShipSunk;
		_navalShipsLogic.ShipRammingEvent += OnShipRamming;
		_navalShipsLogic.ShipHookThrowEvent += OnShipHookThrow;
		_waterStrengthIntensityMultiplier = 1f + MathF.Max(0f, (Mission.Current.Scene.GetWaterStrength() - 3f) * 0.07f);
		_mainAgentBaseHealth = 0f;
		MBMusicManager.Current.DeactivateCurrentMode();
		MBMusicManager.Current.ActivateBattleMode();
		MBMusicManager.Current.OnBattleMusicHandlerInit((IMusicHandler)(object)this);
	}

	public override void OnRemoveBehavior()
	{
		((MissionView)this).OnRemoveBehavior();
		_navalShipsLogic.ShipSunkEvent -= OnShipSunk;
		_navalShipsLogic.ShipRammingEvent -= OnShipRamming;
		_navalShipsLogic.ShipHookThrowEvent -= OnShipHookThrow;
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
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (((int)orderType == 4 || (int)orderType == 5) && ((MissionTime)(ref _nextPossibleTimeToIncreaseIntensityForChargeOrder)).IsPast)
		{
			float currentIntensity = PsaiCore.Instance.GetCurrentIntensity();
			float num = currentIntensity * MusicParameters.PlayerChargeEffectMultiplierOnIntensity - currentIntensity;
			MBMusicManager.Current.ChangeCurrentThemeIntensity(num * _waterStrengthIntensityMultiplier);
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
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between I4 and Unknown
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
		if ((affectedAgent.IsHuman && (int)affectedAgent.State != 2) || flag)
		{
			float num2 = (flag2 ? MusicParameters.FriendlyTroopDeadEffectOnIntensity : MusicParameters.EnemyTroopDeadEffectOnIntensity);
			if (flag)
			{
				num2 *= MusicParameters.PlayerTroopDeadEffectMultiplierOnIntensity;
			}
			MBMusicManager.Current.ChangeCurrentThemeIntensity(num2 * _waterStrengthIntensityMultiplier);
		}
	}

	public void OnShipSunk(MissionShip ship)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame listenerGlobalFrame = _listenerGlobalFrame;
		ref Vec3 origin = ref listenerGlobalFrame.origin;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
		float num = ((Vec3)(ref origin)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
		if (num < 62500f)
		{
			float num2 = MathF.Max(0.5f - MathF.Sqrt(num) * 0.002f, 0.1f);
			MBMusicManager.Current.ChangeCurrentThemeIntensity(num2 * _waterStrengthIntensityMultiplier);
		}
	}

	public void OnShipRamming(MissionShip rammingShip, MissionShip rammedShip, float damagePercent, bool isFirstImpact, CapsuleData capsuleData, int ramQuality)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame listenerGlobalFrame = _listenerGlobalFrame;
		ref Vec3 origin = ref listenerGlobalFrame.origin;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)rammingShip).GameEntity;
		float num = ((Vec3)(ref origin)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().origin);
		if (num < 10000f)
		{
			float num2 = (isFirstImpact ? 0.2f : 0f);
			float num3 = MathF.Max(2f * damagePercent * (1f - MathF.Sqrt(num) * 0.01f), num2);
			MBMusicManager.Current.ChangeCurrentThemeIntensity(num3 * _waterStrengthIntensityMultiplier);
		}
	}

	public void OnShipHookThrow(MissionShip hookingShip, MissionShip hookedShip)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame listenerGlobalFrame = _listenerGlobalFrame;
		ref Vec3 origin = ref listenerGlobalFrame.origin;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)hookingShip).GameEntity;
		float num = ((Vec3)(ref origin)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
		if (num < 10000f)
		{
			float num2 = 0.05f - MathF.Sqrt(num) * 0.0005f;
			MBMusicManager.Current.ChangeCurrentThemeIntensity(num2 * _waterStrengthIntensityMultiplier);
		}
	}

	private void CheckForStarting()
	{
		if (_startingTroopCounts == null || _startingTroopCounts.Sum() == 0)
		{
			_startingTroopCounts = new int[2]
			{
				_navalAgentsLogic.GetNumberOfSpawnedAgents((BattleSideEnum)0),
				_navalAgentsLogic.GetNumberOfSpawnedAgents((BattleSideEnum)1)
			};
		}
		float num = (float)_startingTroopCounts.Sum() / 500f;
		float num2 = MathF.Max(MusicParameters.DefaultStartIntensity, num * 0.8f) + (MBRandom.RandomFloat - 0.5f) * (MusicParameters.RandomEffectMultiplierOnStartIntensity * 2f);
		NavalBattleThemes navalBattleTheme = GetNavalBattleTheme(((MissionBehavior)this).Mission.MusicCulture);
		MBMusicManager.Current.StartTheme((MusicTheme)navalBattleTheme, num2, false);
		_battleState = BattleState.Started;
	}

	private NavalBattleThemes GetNavalBattleTheme(BasicCultureObject culture)
	{
		if (((MBObjectBase)culture).StringId == "sturgia" || ((MBObjectBase)culture).StringId == "nord" || ((MBObjectBase)culture).StringId == "battania")
		{
			if (!((double)MBRandom.NondeterministicRandomFloat > 0.5))
			{
				return NavalBattleThemes.VikingSeaBattle2;
			}
			return NavalBattleThemes.VikingSeaBattle1;
		}
		if (!((double)MBRandom.NondeterministicRandomFloat > 0.5))
		{
			return NavalBattleThemes.MediterraneanSeaBattle2;
		}
		return NavalBattleThemes.MediterraneanSeaBattle1;
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
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		if (_battleState == BattleState.Starting)
		{
			if (((MissionBehavior)this).Mission.MusicCulture == null)
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
		if (_battleState == BattleState.Started && Mission.Current.MainAgent != null && Mission.Current.MainAgent.IsActive())
		{
			float num = 0f;
			if (_mainAgentBaseHealth <= 0.01f)
			{
				_mainAgentBaseHealth = Mission.Current.MainAgent.BaseHealthLimit;
			}
			float num2 = 1f - Mission.Current.MainAgent.Health / _mainAgentBaseHealth;
			_mainAgentBaseHealth = Mission.Current.MainAgent.Health;
			num += num2;
			Vec3 val = Mission.Current.MainAgent.GetAverageRealGlobalVelocity() - Mission.Current.MainAgent.AverageVelocity;
			float lengthSquared = ((Vec3)(ref val)).LengthSquared;
			num += ((lengthSquared > 25f) ? (dt * 0.01f) : 0f);
			if (num > 0f)
			{
				MBMusicManager.Current.ChangeCurrentThemeIntensity(num * _waterStrengthIntensityMultiplier);
			}
		}
		if (_battleState == BattleState.Started || _battleState == BattleState.TurnedOneSide)
		{
			CheckForEnding();
		}
		CheckIntensityFall();
	}
}
