using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.CustomBattle;

public class CPUBenchmarkMissionLogic : MissionLogic
{
	private delegate void MainThreadJobDelegate();

	private enum BattlePhase
	{
		Start,
		ArrowShower,
		MeleePosition,
		Cav1Pos,
		Cav1PosDef,
		CavalryPosition,
		MeleeAttack,
		RangedAdvance,
		CavalryAdvance,
		CavalryCharge,
		CavalryCharge2,
		RangedAdvance2,
		FullCharge
	}

	private enum BenchmarkStatus
	{
		Inactive,
		Active,
		Result,
		SetDefinition
	}

	private const float FormationDistDiff = 20f;

	private const float PressTimeForExit = 0.05f;

	private const float ResultTime = 9f;

	private readonly int _attackerInfCount;

	private readonly int _attackerRangedCount;

	private readonly int _attackerCavCount;

	private readonly int _defenderInfCount;

	private readonly int _defenderCavCount;

	private int _curPath;

	private float _benchmarkExit;

	private bool _benchmarkFinished;

	private static bool _isSiege;

	private float _showResultTime = 92f;

	private Path[] _paths;

	private Path[] _targets;

	private float _cameraSpeed;

	private float _curPathSpeed;

	private float _curPathLenght;

	private float _nextPathSpeed;

	private float _prevPathSpeed;

	private float _cameraPassedDistanceOnPath;

	private MissionAgentSpawnLogic _missionAgentSpawnLogic;

	private bool _formationsSetUp;

	private Formation _defLeftInf;

	private Formation _defMidCav;

	private Formation _defRightInf;

	private Formation _defLeftBInf;

	private Formation _defMidBInf;

	private Formation _defRightBInf;

	private Formation _attLeftInf;

	private Formation _attRightInf;

	private Formation _attLeftRanged;

	private Formation _attRightRanged;

	private Formation _attLeftCav;

	private Formation _attRightCav;

	private Camera _benchmarkCamera;

	private BattlePhase _battlePhase;

	private bool _isCurPhaseInPlay;

	private float _totalTime;

	private bool _benchmarkStarted;

	public CPUBenchmarkMissionLogic(int attackerInfCount, int attackerRangedCount, int attackerCavCount, int defenderInfCount, int defenderCavCount)
	{
		_attackerInfCount = attackerInfCount;
		_attackerRangedCount = attackerRangedCount;
		_attackerCavCount = attackerCavCount;
		_defenderInfCount = defenderInfCount;
		_defenderCavCount = defenderCavCount;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		Utilities.EnableSingleGPUQueryPerFrame();
		_missionAgentSpawnLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentSpawnLogic>();
		_paths = ((MissionBehavior)this).Mission.Scene.GetPathsWithNamePrefix("CameraPath");
		_targets = ((MissionBehavior)this).Mission.Scene.GetPathsWithNamePrefix("CameraTarget");
		Array.Sort(_paths, (Path x, Path y) => x.GetName().CompareTo(y.GetName()));
		Array.Sort(_targets, (Path x, Path y) => x.GetName().CompareTo(y.GetName()));
		if (_paths.Length != 0)
		{
			_curPath = 0;
			_cameraPassedDistanceOnPath = 0f;
			string name = _paths[_curPath].GetName();
			int num = name.LastIndexOf('_');
			_curPathSpeed = (_cameraSpeed = float.Parse(name.Substring(num + 1)));
			_curPathLenght = _paths[_curPath].GetTotalLength();
			if (_paths.Length > _curPath + 1)
			{
				string name2 = _paths[_curPath + 1].GetName();
				int num2 = name2.LastIndexOf('_');
				_nextPathSpeed = float.Parse(name2.Substring(num2 + 1));
			}
		}
	}

	public override void AfterStart()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		((MissionBehavior)this).AfterStart();
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)10, true);
		if (!_isSiege)
		{
			((MissionBehavior)this).Mission.DefenderTeam.ClearTacticOptions();
			((MissionBehavior)this).Mission.AttackerTeam.ClearTacticOptions();
			((MissionBehavior)this).Mission.DefenderTeam.AddTacticOption((TacticComponent)new TacticStop(((MissionBehavior)this).Mission.Teams.Defender));
			((MissionBehavior)this).Mission.AttackerTeam.AddTacticOption((TacticComponent)new TacticStop(((MissionBehavior)this).Mission.Teams.Attacker));
		}
	}

	private void SetupFormations()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_054a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0622: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0645: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		//IL_064b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0650: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_065a: Unknown result type (might be due to invalid IL or missing references)
		//IL_068e: Unknown result type (might be due to invalid IL or missing references)
		//IL_068f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_0695: Unknown result type (might be due to invalid IL or missing references)
		//IL_069a: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0702: Unknown result type (might be due to invalid IL or missing references)
		//IL_0707: Unknown result type (might be due to invalid IL or missing references)
		//IL_0711: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Unknown result type (might be due to invalid IL or missing references)
		//IL_0717: Unknown result type (might be due to invalid IL or missing references)
		//IL_071c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_075a: Unknown result type (might be due to invalid IL or missing references)
		//IL_075b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0760: Unknown result type (might be due to invalid IL or missing references)
		//IL_0761: Unknown result type (might be due to invalid IL or missing references)
		//IL_0766: Unknown result type (might be due to invalid IL or missing references)
		//IL_0770: Unknown result type (might be due to invalid IL or missing references)
		//IL_0775: Unknown result type (might be due to invalid IL or missing references)
		//IL_077a: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_07af: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Unknown result type (might be due to invalid IL or missing references)
		//IL_0808: Unknown result type (might be due to invalid IL or missing references)
		//IL_082b: Unknown result type (might be due to invalid IL or missing references)
		//IL_082d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0832: Unknown result type (might be due to invalid IL or missing references)
		//IL_0834: Unknown result type (might be due to invalid IL or missing references)
		//IL_0839: Unknown result type (might be due to invalid IL or missing references)
		//IL_083e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0840: Unknown result type (might be due to invalid IL or missing references)
		//IL_0842: Unknown result type (might be due to invalid IL or missing references)
		//IL_0847: Unknown result type (might be due to invalid IL or missing references)
		//IL_0848: Unknown result type (might be due to invalid IL or missing references)
		//IL_084d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0852: Unknown result type (might be due to invalid IL or missing references)
		//IL_0865: Unknown result type (might be due to invalid IL or missing references)
		//IL_0867: Unknown result type (might be due to invalid IL or missing references)
		//IL_0871: Unknown result type (might be due to invalid IL or missing references)
		//IL_0873: Unknown result type (might be due to invalid IL or missing references)
		//IL_0878: Unknown result type (might be due to invalid IL or missing references)
		//IL_087d: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08be: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0903: Unknown result type (might be due to invalid IL or missing references)
		//IL_0905: Unknown result type (might be due to invalid IL or missing references)
		//IL_090a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0914: Unknown result type (might be due to invalid IL or missing references)
		//IL_0919: Unknown result type (might be due to invalid IL or missing references)
		//IL_0923: Unknown result type (might be due to invalid IL or missing references)
		//IL_0925: Unknown result type (might be due to invalid IL or missing references)
		//IL_092a: Unknown result type (might be due to invalid IL or missing references)
		//IL_092f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0963: Unknown result type (might be due to invalid IL or missing references)
		//IL_0964: Unknown result type (might be due to invalid IL or missing references)
		//IL_0969: Unknown result type (might be due to invalid IL or missing references)
		//IL_096a: Unknown result type (might be due to invalid IL or missing references)
		//IL_096f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0979: Unknown result type (might be due to invalid IL or missing references)
		//IL_097e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0988: Unknown result type (might be due to invalid IL or missing references)
		//IL_098a: Unknown result type (might be due to invalid IL or missing references)
		//IL_098f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0994: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_09cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a43: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a59: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a63: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a68: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b10: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b27: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b43: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b48: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b80: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b97: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bcf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0beb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Invalid comparison between Unknown and I4
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Expected I4, but got Unknown
		//IL_0c31: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8b: Unknown result type (might be due to invalid IL or missing references)
		if (_isSiege)
		{
			_showResultTime = 295f;
			Mission.Current.MainAgent = ((List<Agent>)(object)Mission.Current.AttackerTeam.ActiveAgents)[0];
			Utilities.ConstructMainThreadJob((Delegate)new MainThreadJobDelegate(((DeploymentHandler)Mission.Current.GetMissionBehavior<SiegeDeploymentHandler>()).FinishDeployment), Array.Empty<object>());
		}
		else
		{
			MatrixFrame globalFrame = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("defend_right").GetGlobalFrame();
			MatrixFrame globalFrame2 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("defend_mid").GetGlobalFrame();
			MatrixFrame globalFrame3 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("defend_left").GetGlobalFrame();
			MatrixFrame globalFrame4 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("attacker_right").GetGlobalFrame();
			MatrixFrame globalFrame5 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("attacker_mid").GetGlobalFrame();
			MatrixFrame globalFrame6 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("attacker_left").GetGlobalFrame();
			_defLeftInf = ((MissionBehavior)this).Mission.DefenderTeam.GetFormation((FormationClass)0);
			_defMidCav = ((MissionBehavior)this).Mission.DefenderTeam.GetFormation((FormationClass)1);
			_defRightInf = ((MissionBehavior)this).Mission.DefenderTeam.GetFormation((FormationClass)2);
			_defLeftBInf = ((MissionBehavior)this).Mission.DefenderTeam.GetFormation((FormationClass)3);
			_defMidBInf = ((MissionBehavior)this).Mission.DefenderTeam.GetFormation((FormationClass)4);
			_defRightBInf = ((MissionBehavior)this).Mission.DefenderTeam.GetFormation((FormationClass)5);
			_attLeftInf = ((MissionBehavior)this).Mission.AttackerTeam.GetFormation((FormationClass)0);
			_attRightInf = ((MissionBehavior)this).Mission.AttackerTeam.GetFormation((FormationClass)1);
			_attLeftRanged = ((MissionBehavior)this).Mission.AttackerTeam.GetFormation((FormationClass)2);
			_attRightRanged = ((MissionBehavior)this).Mission.AttackerTeam.GetFormation((FormationClass)3);
			_attLeftCav = ((MissionBehavior)this).Mission.AttackerTeam.GetFormation((FormationClass)4);
			_attRightCav = ((MissionBehavior)this).Mission.AttackerTeam.GetFormation((FormationClass)6);
			int num = _defenderInfCount / 6;
			float num2 = (float)_defenderInfCount / 3.8f;
			int num3 = 0;
			int num4 = _attackerInfCount / 2;
			int num5 = 0;
			int num6 = _attackerRangedCount / 2;
			int num7 = 0;
			int num8 = _attackerCavCount / 2;
			int num9 = 0;
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				if (item.Team == null || item.Character == null)
				{
					continue;
				}
				if (item.Team.IsDefender)
				{
					if ((int)item.Character.DefaultFormationClass == 2)
					{
						item.Formation = _defMidCav;
					}
					else if ((float)num3 < num2)
					{
						num3++;
						item.Formation = _defLeftInf;
					}
					else if ((float)num3 < num2 * 2f)
					{
						num3++;
						item.Formation = _defRightInf;
					}
					else if ((float)num3 < num2 * 2f + (float)num)
					{
						num3++;
						item.Formation = _defLeftBInf;
					}
					else if ((float)num3 < num2 * 2f + (float)(num * 2))
					{
						num3++;
						item.Formation = _defMidBInf;
					}
					else
					{
						item.Formation = _defRightBInf;
					}
				}
				else
				{
					if (!item.Team.IsAttacker)
					{
						continue;
					}
					FormationClass defaultFormationClass = item.Character.DefaultFormationClass;
					switch ((int)defaultFormationClass)
					{
					case 0:
						if (num5 < num4)
						{
							num5++;
							item.Formation = _attLeftInf;
						}
						else
						{
							item.Formation = _attRightInf;
						}
						break;
					case 1:
						if (num7 < num6)
						{
							num7++;
							item.Formation = _attLeftRanged;
						}
						else
						{
							item.Formation = _attRightRanged;
						}
						break;
					case 2:
						if (num9 < num8)
						{
							num9++;
							item.Formation = _attLeftCav;
						}
						else
						{
							item.Formation = _attRightCav;
						}
						break;
					}
				}
			}
			((MissionBehavior)this).Mission.IsTeleportingAgents = true;
			_defLeftInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_defMidCav.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_defRightInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_defLeftBInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_defMidBInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_defRightBInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_attLeftInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_attRightInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_attLeftRanged.SetArrangementOrder(ArrangementOrder.ArrangementOrderLoose);
			_attRightRanged.SetArrangementOrder(ArrangementOrder.ArrangementOrderLoose);
			_attLeftCav.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_attRightCav.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
			_defLeftInf.SetFormOrder(FormOrder.FormOrderCustom(35f), true);
			_defMidCav.SetFormOrder(FormOrder.FormOrderCustom(30f), true);
			_defRightInf.SetFormOrder(FormOrder.FormOrderCustom(35f), true);
			_defLeftBInf.SetFormOrder(FormOrder.FormOrderCustom(25f), true);
			_defMidBInf.SetFormOrder(FormOrder.FormOrderCustom(25f), true);
			_defRightBInf.SetFormOrder(FormOrder.FormOrderCustom(25f), true);
			_attLeftInf.SetFormOrder(FormOrder.FormOrderCustom(25f), true);
			_attRightInf.SetFormOrder(FormOrder.FormOrderCustom(25f), true);
			_attLeftRanged.SetFormOrder(FormOrder.FormOrderCustom(50f), true);
			_attRightRanged.SetFormOrder(FormOrder.FormOrderCustom(50f), true);
			_attLeftCav.SetFormOrder(FormOrder.FormOrderCustom(30f), true);
			_attRightCav.SetFormOrder(FormOrder.FormOrderCustom(30f), true);
			_defLeftInf.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame3.origin + globalFrame3.rotation.f * 20f * 1.125f + 8f * globalFrame3.rotation.s), (Vec2?)null, (int?)null);
			_defMidCav.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame2.origin - globalFrame2.rotation.f * 20f), (Vec2?)null, (int?)null);
			_defRightInf.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame.origin + globalFrame.rotation.f * 20f * 1.125f - 8f * globalFrame.rotation.s), (Vec2?)null, (int?)null);
			_defLeftBInf.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame3.origin - globalFrame3.rotation.s * 10f), (Vec2?)null, (int?)null);
			_defMidBInf.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame2.origin), (Vec2?)null, (int?)null);
			_defRightBInf.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame.origin + globalFrame.rotation.s * 10f), (Vec2?)null, (int?)null);
			Vec3 val = globalFrame5.origin - globalFrame6.origin;
			Vec3 val2 = globalFrame5.origin - globalFrame4.origin;
			_attLeftInf.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame6.origin + 0.65f * val), (Vec2?)null, (int?)null);
			_attRightInf.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame4.origin + 0.65f * val2), (Vec2?)null, (int?)null);
			_attLeftRanged.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame6.origin + globalFrame6.rotation.f * 20f - 0.3f * val), (Vec2?)null, (int?)null);
			_attRightRanged.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame4.origin + globalFrame4.rotation.f * 20f - 0.3f * val2), (Vec2?)null, (int?)null);
			_attLeftCav.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame6.origin - globalFrame6.rotation.f * 20f * 0.1f - globalFrame6.rotation.s * 25f), (Vec2?)null, (int?)null);
			_attRightCav.SetPositioning((WorldPosition?)new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame4.origin - globalFrame4.rotation.f * 20f * 0.1f + globalFrame4.rotation.s * 25f), (Vec2?)null, (int?)null);
			_defLeftInf.SetMovementOrder(MovementOrder.MovementOrderMove(_defLeftInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_defMidCav.SetMovementOrder(MovementOrder.MovementOrderMove(_defMidCav.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_defRightInf.SetMovementOrder(MovementOrder.MovementOrderMove(_defRightInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_defLeftBInf.SetMovementOrder(MovementOrder.MovementOrderMove(_defLeftBInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_defMidBInf.SetMovementOrder(MovementOrder.MovementOrderMove(_defMidBInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_defRightBInf.SetMovementOrder(MovementOrder.MovementOrderMove(_defRightBInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_attLeftInf.SetMovementOrder(MovementOrder.MovementOrderMove(_attLeftInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_attRightInf.SetMovementOrder(MovementOrder.MovementOrderMove(_attRightInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_attLeftRanged.SetMovementOrder(MovementOrder.MovementOrderMove(_attLeftRanged.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_attRightRanged.SetMovementOrder(MovementOrder.MovementOrderMove(_attRightRanged.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_attLeftCav.SetMovementOrder(MovementOrder.MovementOrderMove(_attLeftCav.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			_attRightCav.SetMovementOrder(MovementOrder.MovementOrderMove(_attRightCav.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
			foreach (Formation item2 in (List<Formation>)(object)((MissionBehavior)this).Mission.AttackerTeam.FormationsIncludingEmpty)
			{
				if (item2.CountOfUnits > 0)
				{
					item2.SetControlledByAI(false, false);
					item2.SetFiringOrder(FiringOrder.FiringOrderHoldYourFire);
				}
			}
			foreach (Formation item3 in (List<Formation>)(object)((MissionBehavior)this).Mission.DefenderTeam.FormationsIncludingEmpty)
			{
				if (item3.CountOfUnits > 0)
				{
					item3.SetControlledByAI(false, false);
					item3.SetFiringOrder(FiringOrder.FiringOrderHoldYourFire);
				}
			}
			foreach (Agent item4 in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				item4.SetIsAIPaused(true);
			}
		}
		_formationsSetUp = true;
	}

	public override void OnMissionTick(float dt)
	{
		_benchmarkStarted = true;
	}

	protected override void OnEndMission()
	{
		Utilities.SetBenchmarkStatus(0, "");
	}

	public override void OnPreMissionTick(float dt)
	{
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnMissionTick(dt);
		if (!_benchmarkStarted)
		{
			return;
		}
		if (!_formationsSetUp && (_isSiege || _missionAgentSpawnLogic.IsDeploymentOver))
		{
			SetupFormations();
			Utilities.SetBenchmarkStatus(1, _isSiege ? "#" : "");
		}
		if (_formationsSetUp && !_isSiege)
		{
			Check();
		}
		_totalTime += dt;
		Utilities.SetBenchmarkStatus(3, "Battle Size: " + (_attackerCavCount + _attackerInfCount + _attackerRangedCount) + " (" + ((List<Agent>)(object)((MissionBehavior)this).Mission.AttackerTeam.ActiveAgents).Count + ") vs (" + ((List<Agent>)(object)((MissionBehavior)this).Mission.DefenderTeam.ActiveAgents).Count + ") " + (_defenderCavCount + _defenderInfCount));
		if (_benchmarkExit != 0f && !_benchmarkFinished && _totalTime - _benchmarkExit >= 0.05f)
		{
			Utilities.SetBenchmarkStatus(2, "");
			MouseManager.ShowCursor(true);
			_benchmarkFinished = true;
		}
		if (Input.IsKeyPressed((InputKey)1) && _benchmarkExit == 0f)
		{
			_benchmarkExit = _totalTime;
		}
		if (Input.IsKeyReleased((InputKey)1) && _benchmarkExit != 0f && _totalTime - _benchmarkExit < 0.05f)
		{
			_benchmarkExit = 0f;
		}
		if (!_benchmarkFinished && _totalTime > _showResultTime)
		{
			Utilities.SetBenchmarkStatus(2, "");
			MouseManager.ShowCursor(true);
			_benchmarkFinished = true;
			_benchmarkExit = _totalTime;
		}
		if (_benchmarkExit != 0f && _totalTime - _benchmarkExit > 9f)
		{
			Utilities.SetBenchmarkStatus(0, "Battle Size: " + (_attackerCavCount + _attackerInfCount + _attackerRangedCount) + " vs " + (_defenderCavCount + _defenderInfCount));
			Mission.Current.EndMission();
		}
		ScreenBase topScreen = ScreenManager.TopScreen;
		MissionScreen val;
		if ((val = (MissionScreen)(object)((topScreen is MissionScreen) ? topScreen : null)) == null)
		{
			return;
		}
		Camera combatCamera = val.CombatCamera;
		if ((NativeObject)(object)combatCamera != (NativeObject)null && _curPath < _paths.Length)
		{
			if ((NativeObject)(object)_benchmarkCamera == (NativeObject)null)
			{
				_benchmarkCamera = Camera.CreateCamera();
				_benchmarkCamera.SetFovHorizontal(combatCamera.HorizontalFov, combatCamera.GetAspectRatio(), combatCamera.Near, combatCamera.Far);
			}
			if (_cameraPassedDistanceOnPath < _curPathLenght && _cameraPassedDistanceOnPath > _curPathLenght / 6f * 5f)
			{
				_cameraSpeed = MathF.Lerp(_curPathSpeed, (_curPath != _paths.Length - 1) ? ((_nextPathSpeed + _curPathSpeed) / 2f) : 5f, (_cameraPassedDistanceOnPath - _curPathLenght / 6f * 5f) / (_curPathLenght / 6f), 1E-05f);
			}
			if (_cameraPassedDistanceOnPath < _curPathLenght / 6f)
			{
				_cameraSpeed = MathF.Lerp((_curPath != 0) ? ((_curPathSpeed + _prevPathSpeed) / 2f) : 5f, _curPathSpeed, _cameraPassedDistanceOnPath / (_curPathLenght / 6f), 1E-05f);
			}
			_cameraPassedDistanceOnPath += _cameraSpeed * dt;
			if (_cameraPassedDistanceOnPath >= _paths[_curPath].GetTotalLength() && _curPath != _paths.Length - 1)
			{
				_curPath++;
				_curPathLenght = _paths[_curPath].GetTotalLength();
				_prevPathSpeed = _curPathSpeed;
				_curPathSpeed = _nextPathSpeed;
				_cameraPassedDistanceOnPath = _cameraSpeed * dt;
				if (_paths.Length > _curPath + 1)
				{
					string name = _paths[_curPath + 1].GetName();
					int num = name.LastIndexOf('_');
					_nextPathSpeed = float.Parse(name.Substring(num + 1));
				}
			}
			MatrixFrame frameForDistance = _paths[_curPath].GetFrameForDistance(MathF.Min(_paths[_curPath].GetTotalLength(), _cameraPassedDistanceOnPath));
			MatrixFrame frameForDistance2 = _targets[_curPath].GetFrameForDistance(MathF.Min(1f, _cameraPassedDistanceOnPath / _paths[_curPath].GetTotalLength()) * _targets[_curPath].GetTotalLength());
			_benchmarkCamera.LookAt(frameForDistance.origin, frameForDistance2.origin, Vec3.Up);
			val.UpdateFreeCamera(_benchmarkCamera.Frame);
			val.CustomCamera = val.CombatCamera;
		}
		if (Utilities.IsBenchmarkQuited())
		{
			Utilities.SetBenchmarkStatus(0, "Battle Size: " + (_attackerCavCount + _attackerInfCount + _attackerRangedCount) + " vs " + (_defenderCavCount + _defenderInfCount));
			Mission.Current.EndMission();
		}
	}

	private void Check()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_052a: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0591: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_062d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_063c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_064b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0650: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_0657: Unknown result type (might be due to invalid IL or missing references)
		//IL_0664: Unknown result type (might be due to invalid IL or missing references)
		//IL_0669: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		//IL_0677: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0681: Unknown result type (might be due to invalid IL or missing references)
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_0696: Unknown result type (might be due to invalid IL or missing references)
		//IL_069b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0704: Unknown result type (might be due to invalid IL or missing references)
		//IL_0709: Unknown result type (might be due to invalid IL or missing references)
		//IL_070d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Unknown result type (might be due to invalid IL or missing references)
		//IL_072d: Unknown result type (might be due to invalid IL or missing references)
		//IL_072f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0734: Unknown result type (might be due to invalid IL or missing references)
		//IL_0736: Unknown result type (might be due to invalid IL or missing references)
		//IL_073b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0745: Unknown result type (might be due to invalid IL or missing references)
		//IL_074a: Unknown result type (might be due to invalid IL or missing references)
		//IL_074f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0754: Unknown result type (might be due to invalid IL or missing references)
		//IL_076a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0780: Unknown result type (might be due to invalid IL or missing references)
		//IL_0796: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_080e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0813: Unknown result type (might be due to invalid IL or missing references)
		//IL_081c: Unknown result type (might be due to invalid IL or missing references)
		//IL_082e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0833: Unknown result type (might be due to invalid IL or missing references)
		//IL_083c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_084e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0857: Unknown result type (might be due to invalid IL or missing references)
		//IL_085c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0861: Unknown result type (might be due to invalid IL or missing references)
		//IL_0866: Unknown result type (might be due to invalid IL or missing references)
		//IL_086b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0875: Unknown result type (might be due to invalid IL or missing references)
		//IL_087a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0883: Unknown result type (might be due to invalid IL or missing references)
		//IL_0895: Unknown result type (might be due to invalid IL or missing references)
		//IL_089a: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08be: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0907: Unknown result type (might be due to invalid IL or missing references)
		//IL_0909: Unknown result type (might be due to invalid IL or missing references)
		//IL_090e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0966: Unknown result type (might be due to invalid IL or missing references)
		float currentTime = ((MissionBehavior)this).Mission.CurrentTime;
		if (_battlePhase == BattlePhase.Start && currentTime >= 5f)
		{
			((MissionBehavior)this).Mission.IsTeleportingAgents = false;
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				item.SetIsAIPaused(false);
			}
			_battlePhase = BattlePhase.ArrowShower;
		}
		else
		{
			if (_battlePhase == BattlePhase.Start)
			{
				return;
			}
			if (!_isCurPhaseInPlay)
			{
				Debug.Print("State: " + _battlePhase, 0, (DebugColor)7, 64uL);
				Vec2 val;
				switch (_battlePhase)
				{
				case BattlePhase.ArrowShower:
					_attLeftRanged.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_attRightRanged.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_defLeftBInf.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_defRightBInf.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_defMidBInf.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_defLeftInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderShieldWall);
					_defRightInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderShieldWall);
					_defLeftInf.SetFormOrder(FormOrder.FormOrderCustom(35f), true);
					_defRightInf.SetFormOrder(FormOrder.FormOrderCustom(35f), true);
					_attLeftInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderShieldWall);
					_attRightInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderShieldWall);
					break;
				case BattlePhase.MeleePosition:
				{
					Vec2 val19 = -(_attLeftInf.OrderPosition - _defRightInf.OrderPosition);
					Vec2 val20 = -(_attRightInf.OrderPosition - _defLeftInf.OrderPosition);
					((Vec2)(ref val19)).RotateCCW(0.08726646f);
					((Vec2)(ref val20)).RotateCCW(-0.08726646f);
					WorldPosition val21 = _attLeftInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0);
					((WorldPosition)(ref val21)).SetVec2(((WorldPosition)(ref val21)).AsVec2 + val19);
					_attLeftInf.SetMovementOrder(MovementOrder.MovementOrderMove(val21));
					WorldPosition val22 = _attRightInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0);
					((WorldPosition)(ref val22)).SetVec2(((WorldPosition)(ref val22)).AsVec2 + val20);
					_attRightInf.SetMovementOrder(MovementOrder.MovementOrderMove(val22));
					break;
				}
				case BattlePhase.Cav1Pos:
				{
					Vec2 orderPosition2 = _attLeftRanged.OrderPosition;
					Vec2 direction2 = _attLeftRanged.Direction;
					orderPosition2 -= 15f * direction2;
					((Vec2)(ref direction2)).RotateCCW(MathF.PI / 2f);
					orderPosition2 += 60f * direction2;
					WorldPosition val18 = _attLeftRanged.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0);
					((WorldPosition)(ref val18)).SetVec2(orderPosition2);
					_attLeftCav.SetMovementOrder(MovementOrder.MovementOrderMove(val18));
					break;
				}
				case BattlePhase.Cav1PosDef:
				{
					MatrixFrame globalFrame3 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("defend_right").GetGlobalFrame();
					Vec3 val17 = globalFrame3.origin + 40f * globalFrame3.rotation.s;
					_defMidCav.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(((MissionBehavior)this).Mission.Scene, val17)));
					break;
				}
				case BattlePhase.CavalryPosition:
				{
					Vec2 orderPosition = _attRightRanged.OrderPosition;
					Vec2 direction = _attRightRanged.Direction;
					orderPosition += 20f * direction;
					((Vec2)(ref direction)).RotateCCW(-MathF.PI / 2f);
					orderPosition += 80f * direction;
					WorldPosition val16 = _attRightRanged.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0);
					((WorldPosition)(ref val16)).SetVec2(orderPosition);
					_attRightCav.SetMovementOrder(MovementOrder.MovementOrderMove(val16));
					_attLeftInf.SetMovementOrder(MovementOrder.MovementOrderCharge);
					_attRightInf.SetMovementOrder(MovementOrder.MovementOrderCharge);
					_defLeftBInf.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					break;
				}
				case BattlePhase.MeleeAttack:
				{
					_defLeftInf.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_defMidBInf.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_defRightBInf.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_attLeftInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
					_attRightInf.SetArrangementOrder(ArrangementOrder.ArrangementOrderLine);
					Formation attLeftInf = _attLeftInf;
					Scene scene = ((MissionBehavior)this).Mission.Scene;
					val = _defRightInf.GetAveragePositionOfUnits(true, false);
					attLeftInf.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(scene, ((Vec2)(ref val)).ToVec3(0f))));
					Formation attRightInf = _attRightInf;
					Scene scene2 = ((MissionBehavior)this).Mission.Scene;
					val = _defLeftInf.GetAveragePositionOfUnits(true, false);
					attRightInf.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(scene2, ((Vec2)(ref val)).ToVec3(0f))));
					break;
				}
				case BattlePhase.RangedAdvance:
				{
					val = _attLeftRanged.GetAveragePositionOfUnits(true, false);
					Vec3 val10 = ((Vec2)(ref val)).ToVec3(0f);
					val = _attLeftRanged.GetAveragePositionOfUnits(true, false);
					Vec3 val11 = ((Vec2)(ref val)).ToVec3(0f);
					val = _defRightInf.GetAveragePositionOfUnits(true, false);
					Vec3 val12 = val10 - 0.15f * (val11 - ((Vec2)(ref val)).ToVec3(0f));
					val = _attRightRanged.GetAveragePositionOfUnits(true, false);
					Vec3 val13 = ((Vec2)(ref val)).ToVec3(0f);
					val = _attRightRanged.GetAveragePositionOfUnits(true, false);
					Vec3 val14 = ((Vec2)(ref val)).ToVec3(0f);
					val = _defLeftInf.GetAveragePositionOfUnits(true, false);
					Vec3 val15 = val13 - 0.15f * (val14 - ((Vec2)(ref val)).ToVec3(0f));
					_attLeftRanged.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(((MissionBehavior)this).Mission.Scene, val12)));
					_attRightRanged.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(((MissionBehavior)this).Mission.Scene, val15)));
					break;
				}
				case BattlePhase.CavalryAdvance:
				{
					((MissionBehavior)this).Mission.Scene.FindEntityWithTag("attacker_mid").GetGlobalFrame();
					MatrixFrame globalFrame2 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("defend_right").GetGlobalFrame();
					((MissionBehavior)this).Mission.Scene.FindEntityWithTag("defend_left").GetGlobalFrame();
					Vec3 val8 = globalFrame2.origin + globalFrame2.rotation.s * 68f;
					Vec3 val9 = val8;
					val = _attLeftRanged.Direction;
					val8 = val9 + 10f * ((Vec2)(ref val)).ToVec3(0f);
					_attLeftCav.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(((MissionBehavior)this).Mission.Scene, val8)));
					_defMidCav.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(((MissionBehavior)this).Mission.Scene, val8)));
					break;
				}
				case BattlePhase.CavalryCharge:
				{
					MatrixFrame globalFrame = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("defend_left").GetGlobalFrame();
					Formation defLeftBInf = _defLeftBInf;
					val = _attRightCav.CurrentPosition - _defLeftBInf.CurrentPosition;
					defLeftBInf.SetFacingOrder(FacingOrder.FacingOrderLookAtDirection(((Vec2)(ref val)).Normalized()));
					_defLeftBInf.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(((MissionBehavior)this).Mission.Scene, globalFrame.origin - globalFrame.rotation.s * 10f)));
					_attRightCav.SetMovementOrder(MovementOrder.MovementOrderChargeToTarget(_defLeftBInf));
					_attLeftCav.SetMovementOrder(MovementOrder.MovementOrderChargeToTarget(_attLeftInf));
					_defMidCav.SetMovementOrder(MovementOrder.MovementOrderChargeToTarget(_attRightInf));
					break;
				}
				case BattlePhase.CavalryCharge2:
					_attRightCav.SetMovementOrder(MovementOrder.MovementOrderMove(_defLeftBInf.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)0)));
					_attLeftRanged.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_attLeftRanged.SetMovementOrder(MovementOrder.MovementOrderAdvance);
					_attRightRanged.SetFiringOrder(FiringOrder.FiringOrderFireAtWill);
					_attRightRanged.SetMovementOrder(MovementOrder.MovementOrderAdvance);
					break;
				case BattlePhase.RangedAdvance2:
				{
					val = _attLeftRanged.GetAveragePositionOfUnits(true, false);
					Vec3 val2 = ((Vec2)(ref val)).ToVec3(0f);
					val = _attLeftRanged.GetAveragePositionOfUnits(true, false);
					Vec3 val3 = ((Vec2)(ref val)).ToVec3(0f);
					val = _defRightInf.GetAveragePositionOfUnits(true, false);
					Vec3 val4 = val2 - 0.15f * (val3 - ((Vec2)(ref val)).ToVec3(0f));
					val = _attRightRanged.GetAveragePositionOfUnits(true, false);
					Vec3 val5 = ((Vec2)(ref val)).ToVec3(0f);
					val = _attRightRanged.GetAveragePositionOfUnits(true, false);
					Vec3 val6 = ((Vec2)(ref val)).ToVec3(0f);
					val = _defLeftInf.GetAveragePositionOfUnits(true, false);
					Vec3 val7 = val5 - 0.15f * (val6 - ((Vec2)(ref val)).ToVec3(0f));
					_attLeftRanged.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(((MissionBehavior)this).Mission.Scene, val4)));
					_attRightRanged.SetMovementOrder(MovementOrder.MovementOrderMove(new WorldPosition(((MissionBehavior)this).Mission.Scene, val7)));
					break;
				}
				case BattlePhase.FullCharge:
					foreach (Formation item2 in (List<Formation>)(object)((MissionBehavior)this).Mission.AttackerTeam.FormationsIncludingEmpty)
					{
						if (item2.CountOfUnits > 0 && item2 != _attLeftRanged && item2 != _attRightRanged && item2 != _attRightCav)
						{
							item2.SetMovementOrder(MovementOrder.MovementOrderCharge);
						}
					}
					break;
				}
				_isCurPhaseInPlay = true;
				return;
			}
			switch (_battlePhase)
			{
			case BattlePhase.ArrowShower:
				if (currentTime > 14f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.MeleePosition;
				}
				break;
			case BattlePhase.MeleePosition:
				if (currentTime > 19f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.MeleeAttack;
				}
				break;
			case BattlePhase.MeleeAttack:
				if (currentTime > 19f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.Cav1Pos;
				}
				break;
			case BattlePhase.Cav1Pos:
				if (currentTime > 19f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.Cav1PosDef;
				}
				break;
			case BattlePhase.Cav1PosDef:
				if (currentTime > 24f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.CavalryAdvance;
				}
				break;
			case BattlePhase.CavalryAdvance:
				if (currentTime > 30f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.RangedAdvance;
				}
				break;
			case BattlePhase.RangedAdvance:
				if (currentTime > 60f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.CavalryPosition;
				}
				break;
			case BattlePhase.CavalryPosition:
				if (currentTime > 74.5f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.CavalryCharge;
				}
				break;
			case BattlePhase.CavalryCharge:
				if (currentTime > 92f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.CavalryCharge2;
				}
				break;
			case BattlePhase.CavalryCharge2:
				if (currentTime > 93f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.RangedAdvance2;
				}
				break;
			case BattlePhase.RangedAdvance2:
				if (currentTime > 94f)
				{
					_isCurPhaseInPlay = false;
					_battlePhase = BattlePhase.FullCharge;
				}
				break;
			case BattlePhase.FullCharge:
				break;
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
	}

	[CommandLineArgumentFunction("cpu_benchmark_mission", "benchmark")]
	public static string CPUBenchmarkMission(List<string> strings)
	{
		OpenCPUBenchmarkMission("benchmark_battle_11");
		return "Success";
	}

	[CommandLineArgumentFunction("cpu_benchmark", "benchmark")]
	public static string CPUBenchmark(List<string> strings)
	{
		foreach (string @string in strings)
		{
			if (@string == "siege")
			{
				_isSiege = true;
			}
		}
		MBGameManager.StartNewGame((MBGameManager)(object)new CustomGameManager());
		return "";
	}

	[CommandLineArgumentFunction("benchmark_start", "state_string")]
	public static string BenchmarkStateStart(List<string> strings)
	{
		GameState activeState = GameStateManager.Current.ActiveState;
		if (activeState is InitialState)
		{
			MBGameManager.StartNewGame((MBGameManager)(object)new CustomGameManager());
		}
		else if (activeState is CustomBattleState)
		{
			GameStateManager.StateActivateCommand = "state_string.benchmark_end";
			if (!_isSiege)
			{
				OpenCPUBenchmarkMission("benchmark_battle_11");
			}
			else
			{
				OpenCPUBenchmarkMission("benchmark_siege");
			}
		}
		return "";
	}

	[CommandLineArgumentFunction("benchmark_end", "state_string")]
	public static string BenchmarkStateEnd(List<string> strings)
	{
		if (GameStateManager.Current.ActiveState is CustomBattleState)
		{
			GameStateManager.StateActivateCommand = null;
			Game.Current.GameStateManager.PopState(0);
		}
		return "";
	}

	public static Mission OpenCPUBenchmarkMission(string scene)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Expected O, but got Unknown
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Expected O, but got Unknown
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Expected O, but got Unknown
		int realBattleSize = BannerlordConfig.GetRealBattleSize();
		IMissionTroopSupplier[] troopSuppliers = (IMissionTroopSupplier[])(object)new IMissionTroopSupplier[2];
		BasicCultureObject val = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");
		Banner val2 = new Banner("11.4.124.4345.4345.768.768.1.0.0.163.0.5.512.512.769.764.1.0.0");
		Banner val3 = new Banner("11.45.126.4345.4345.768.768.1.0.0.462.0.13.512.512.769.764.1.0.0");
		CustomBattleCombatant playerParty = new CustomBattleCombatant(new TextObject("{=!}Player Party", (Dictionary<string, object>)null), val, val2);
		CustomBattleCombatant enemyParty = new CustomBattleCombatant(new TextObject("{=!}Enemy Party", (Dictionary<string, object>)null), val, val3);
		if (!_isSiege)
		{
			int attackerInfCount = realBattleSize / 100 * 18;
			int attackerRangedCount = realBattleSize / 100 * 10;
			int attackerCavCount = realBattleSize / 100 * 8;
			int defenderInfCount = realBattleSize / 100 * 59;
			int defenderCavCount = realBattleSize / 100 * 5;
			playerParty.Side = (BattleSideEnum)1;
			playerParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("imperial_legionary"), attackerInfCount);
			playerParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("imperial_palatine_guard"), attackerRangedCount);
			playerParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("imperial_cataphract"), attackerCavCount);
			enemyParty.Side = (BattleSideEnum)0;
			enemyParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("battanian_wildling"), defenderInfCount);
			enemyParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("battanian_horseman"), defenderCavCount);
			CustomBattleTroopSupplier val4 = new CustomBattleTroopSupplier(playerParty, true, false, false, (Func<BasicCharacterObject, bool>)null);
			troopSuppliers[playerParty.Side] = (IMissionTroopSupplier)(object)val4;
			CustomBattleTroopSupplier val5 = new CustomBattleTroopSupplier(enemyParty, false, false, false, (Func<BasicCharacterObject, bool>)null);
			troopSuppliers[enemyParty.Side] = (IMissionTroopSupplier)(object)val5;
			MissionInitializerRecord val6 = default(MissionInitializerRecord);
			((MissionInitializerRecord)(ref val6))._002Ector(scene);
			val6.DoNotUseLoadingScreen = false;
			val6.PlayingInCampaignMode = false;
			val6.DecalAtlasGroup = 2;
			return MissionState.OpenNew("CPUBenchmarkMission", val6, (InitializeMissionBehaviorsDelegate)((Mission missionController) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[10]
			{
				(MissionBehavior)new MissionCombatantsLogic((IEnumerable<IBattleCombatant>)null, (IBattleCombatant)(object)playerParty, (IBattleCombatant)(object)enemyParty, (IBattleCombatant)(object)playerParty, (MissionTeamAITypeEnum)1, false),
				(MissionBehavior)new MissionAgentSpawnLogic(troopSuppliers, (BattleSideEnum)1, (BattleSizeType)0),
				(MissionBehavior)new BattlePowerCalculationLogic(),
				(MissionBehavior)new CPUBenchmarkMissionSpawnHandler(enemyParty, playerParty),
				(MissionBehavior)new CPUBenchmarkMissionLogic(attackerInfCount, attackerRangedCount, attackerCavCount, defenderInfCount, defenderCavCount),
				(MissionBehavior)new AgentHumanAILogic(),
				(MissionBehavior)new AgentVictoryLogic(),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f)
			}), true, true);
		}
		int num = realBattleSize / 100 * 30;
		int num2 = realBattleSize / 100 * 25;
		int num3 = realBattleSize / 100 * 20;
		int num4 = realBattleSize / 100 * 25;
		playerParty.Side = (BattleSideEnum)1;
		playerParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("commander_1"), 1);
		playerParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("imperial_legionary"), num);
		playerParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("imperial_palatine_guard"), num2);
		enemyParty.Side = (BattleSideEnum)0;
		enemyParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("commander_2"), 1);
		enemyParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("battanian_wildling"), num3);
		enemyParty.AddCharacter(MBObjectManager.Instance.GetObject<BasicCharacterObject>("battanian_militia_archer"), num4);
		CustomBattleTroopSupplier val7 = new CustomBattleTroopSupplier(playerParty, true, false, false, (Func<BasicCharacterObject, bool>)null);
		troopSuppliers[playerParty.Side] = (IMissionTroopSupplier)(object)val7;
		CustomBattleTroopSupplier val8 = new CustomBattleTroopSupplier(enemyParty, false, false, false, (Func<BasicCharacterObject, bool>)null);
		troopSuppliers[enemyParty.Side] = (IMissionTroopSupplier)(object)val8;
		SiegeEngineType val9 = MBObjectManager.Instance.GetObject<SiegeEngineType>("fire_ballista");
		MBObjectManager.Instance.GetObject<SiegeEngineType>("fire_onager");
		MBObjectManager.Instance.GetObject<SiegeEngineType>("fire_catapult");
		SiegeEngineType val10 = MBObjectManager.Instance.GetObject<SiegeEngineType>("trebuchet");
		SiegeEngineType val11 = MBObjectManager.Instance.GetObject<SiegeEngineType>("ram");
		SiegeEngineType val12 = MBObjectManager.Instance.GetObject<SiegeEngineType>("siege_tower_level2");
		List<MissionSiegeWeapon> list = new List<MissionSiegeWeapon>();
		list.Add(MissionSiegeWeapon.CreateDefaultWeapon(val9));
		list.Add(MissionSiegeWeapon.CreateDefaultWeapon(val9));
		list.Add(MissionSiegeWeapon.CreateDefaultWeapon(val10));
		list.Add(MissionSiegeWeapon.CreateDefaultWeapon(val10));
		list.Add(MissionSiegeWeapon.CreateDefaultWeapon(val12));
		list.Add(MissionSiegeWeapon.CreateDefaultWeapon(val11));
		List<MissionSiegeWeapon> list2 = new List<MissionSiegeWeapon>();
		list2.Add(MissionSiegeWeapon.CreateDefaultWeapon(val9));
		list2.Add(MissionSiegeWeapon.CreateDefaultWeapon(val9));
		list2.Add(MissionSiegeWeapon.CreateDefaultWeapon(val9));
		list2.Add(MissionSiegeWeapon.CreateDefaultWeapon(val9));
		float[] array = new float[2] { 1f, 1f };
		Mission obj = BannerlordMissions.OpenSiegeMissionWithDeployment(scene, MBObjectManager.Instance.GetObject<BasicCharacterObject>("commander_1"), playerParty, enemyParty, true, array, true, list, list2, true, 3, "", false, false, 6f);
		obj.AddMissionBehavior((MissionBehavior)(object)new CPUBenchmarkMissionLogic(num, num2, 0, num3, 0));
		return obj;
	}
}
