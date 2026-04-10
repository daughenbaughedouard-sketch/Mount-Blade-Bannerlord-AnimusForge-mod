using System;
using System.Collections.Generic;
using System.Linq;
using SandBox;
using SandBox.Conversation.MissionLogics;
using StoryMode.StoryModeObjects;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;

namespace StoryMode.Missions;

public class TrainingFieldMissionController : MissionLogic
{
	public class TutorialObjective
	{
		private readonly TextObject _name;

		public string Id { get; }

		public bool IsFinished { get; private set; }

		public bool HasBackground { get; }

		public bool IsActive { get; private set; }

		public List<TutorialObjective> SubTasks { get; }

		public float Score { get; private set; }

		public TutorialObjective(string id, bool isFinished = false, bool isActive = false, bool hasBackground = false)
		{
			_name = GameTexts.FindText("str_tutorial_" + id, (string)null);
			Id = id;
			IsFinished = isFinished;
			IsActive = isActive;
			SubTasks = new List<TutorialObjective>();
			Score = 0f;
			HasBackground = hasBackground;
		}

		public void SetTextVariableOfName(string tag, int variable)
		{
			string? text = ((object)_name).ToString();
			_name.SetTextVariable(tag, variable);
			if (text != ((object)_name).ToString())
			{
				_updateObjectivesWillBeCalled = true;
			}
		}

		public string GetNameString()
		{
			if (!(_name == (TextObject)null))
			{
				return ((object)_name).ToString();
			}
			return "";
		}

		public bool SetActive(bool isActive)
		{
			if (IsActive == isActive)
			{
				return false;
			}
			IsActive = isActive;
			_updateObjectivesWillBeCalled = true;
			return true;
		}

		public bool FinishTask()
		{
			if (IsFinished)
			{
				return false;
			}
			IsFinished = true;
			_updateObjectivesWillBeCalled = true;
			return true;
		}

		public void FinishSubTask(string subTaskName, float score)
		{
			TutorialObjective tutorialObjective = SubTasks.Find((TutorialObjective x) => x.Id == subTaskName);
			tutorialObjective.FinishTask();
			if (score != 0f && (tutorialObjective.Score > score || tutorialObjective.Score == 0f))
			{
				tutorialObjective.Score = score;
			}
			if (!SubTasks.Exists((TutorialObjective x) => !x.IsFinished))
			{
				FinishTask();
			}
			_updateObjectivesWillBeCalled = true;
		}

		public bool SetAllSubTasksInactive()
		{
			bool flag = false;
			foreach (TutorialObjective subTask in SubTasks)
			{
				bool flag2 = subTask.SetActive(isActive: false);
				flag = flag || flag2;
				if (subTask.SubTasks.Count > 0)
				{
					bool flag3 = subTask.SetAllSubTasksInactive();
					flag = flag || flag3;
				}
			}
			if (flag)
			{
				_updateObjectivesWillBeCalled = true;
			}
			return flag;
		}

		public void AddSubTask(TutorialObjective newSubTask)
		{
			SubTasks.Add(newSubTask);
			_updateObjectivesWillBeCalled = true;
		}

		public void RestoreScoreFromSave(float score)
		{
			Score = score;
			_updateObjectivesWillBeCalled = true;
		}
	}

	public readonly struct DelayedAction
	{
		private readonly float _orderGivenTime;

		private readonly float _delayTime;

		private readonly Action _order;

		public DelayedAction(Action order, float delayTime)
		{
			_orderGivenTime = Mission.Current.CurrentTime;
			_delayTime = delayTime;
			_order = order;
		}

		public bool Update()
		{
			if (Mission.Current.CurrentTime - _orderGivenTime > _delayTime)
			{
				_order();
				return true;
			}
			return false;
		}
	}

	public enum MouseObjectives
	{
		None,
		AttackLeft,
		AttackRight,
		AttackUp,
		AttackDown,
		DefendLeft,
		DefendRight,
		DefendUp,
		DefendDown
	}

	public enum ObjectivePerformingType
	{
		None,
		ByLookDirection,
		ByMovement,
		AutoBlock
	}

	private enum HorseReturningSituation
	{
		NotInPosition,
		BeginReturn,
		Returning,
		ReturnCompleted,
		Following
	}

	private const string SoundBasicMeleeBlockLeft = "event:/mission/tutorial/vo/parrying/block_left";

	private const string SoundBasicMeleeBlockRight = "event:/mission/tutorial/vo/parrying/block_right";

	private const string SoundBasicMeleeBlockUp = "event:/mission/tutorial/vo/parrying/block_up";

	private const string SoundBasicMeleeBlockDown = "event:/mission/tutorial/vo/parrying/block_down";

	private const string SoundBasicMeleeAttackLeft = "event:/mission/tutorial/vo/parrying/attack_left";

	private const string SoundBasicMeleeAttackRight = "event:/mission/tutorial/vo/parrying/attack_right";

	private const string SoundBasicMeleeAttackUp = "event:/mission/tutorial/vo/parrying/attack_up";

	private const string SoundBasicMeleeAttackDown = "event:/mission/tutorial/vo/parrying/attack_down";

	private const string SoundBasicMeleeRemark = "event:/mission/tutorial/vo/parrying/remark";

	private const string SoundBasicMeleePraise = "event:/mission/tutorial/vo/parrying/praise";

	private const string SoundAdvancedMeleeGreet = "event:/mission/tutorial/vo/fighting/greet";

	private const string SoundAdvancedMeleeWarning = "event:/mission/tutorial/vo/fighting/warning";

	private const string SoundAdvancedMeleePlayerLose = "event:/mission/tutorial/vo/fighting/player_lose";

	private const string SoundAdvancedMeleePlayerWin = "event:/mission/tutorial/vo/fighting/player_win";

	private const string SoundRangedPickPrefix = "event:/mission/tutorial/vo/archery/pick_";

	private const string SoundRangedStartTraining = "event:/mission/tutorial/vo/archery/start_training";

	private const string SoundRangedHitTarget = "event:/mission/tutorial/vo/archery/hit_target";

	private const string SoundRangedFinish = "event:/mission/tutorial/vo/archery/finish";

	private const string SoundMountedPickPrefix = "event:/mission/tutorial/vo/riding/pick_";

	private const string SoundMountedStartCourse = "event:/mission/tutorial/vo/riding/start_course";

	private const string SoundMountedCourseFinish = "event:/mission/tutorial/vo/riding/course_finish";

	private const string SoundMountedCoursePerfect = "event:/mission/tutorial/vo/riding/course_perfect";

	private const string FinishCourseSound = "event:/mission/tutorial/finish_course";

	private const string FinishTaskSound = "event:/mission/tutorial/finish_task";

	private const string HitTargetSound = "event:/mission/tutorial/hit_target";

	private const string RangedNpcCharacter = "tutorial_npc_ranged";

	private const string BowTrainingShootingPositionTag = "bow_training_shooting_position";

	private const string SpawnerRangedNpcTag = "spawner_ranged_npc_tag";

	private const string RangedNpcTargetTag = "_ranged_npc_target";

	private const float ShootingPositionActivationDistance = 2f;

	private const string BasicMeleeNpcSpawnPointTag = "spawner_melee_npc";

	private const string BasicMeleeNpcCharacter = "tutorial_npc_basic_melee";

	private const string AdvancedMeleeNpcSpawnPointTagEasy = "spawner_adv_melee_npc_easy";

	private const string AdvancedMeleeNpcSpawnPointTagNormal = "spawner_adv_melee_npc_normal";

	private const string AdvancedMeleeNpcEasySecondPositionTag = "adv_melee_npc_easy_second_pos";

	private const string AdvancedMeleeNpcNormalSecondPositionTag = "adv_melee_npc_normal_second_pos";

	private const string AdvancedMeleeEasyNpcCharacter = "tutorial_npc_advanced_melee_easy";

	private const string AdvancedMeleeNormalNpcCharacter = "tutorial_npc_advanced_melee_normal";

	private const string AdvancedMeleeBattleAreaTag = "battle_area";

	private const string MountedAISpawnPositionTag = "_mounted_ai_spawn_position";

	private const string MountedAICharacter = "tutorial_npc_mounted_ai";

	private const string MountedAITargetTag = "_mounted_ai_target";

	private const string MountedAIWaitingPositionTag = "_mounted_ai_waiting_position";

	private const string CheckpointTag = "mounted_checkpoint";

	private const string HorseSpawnPositionTag = "spawner_horse";

	private const string FinishGateClosedTag = "finish_gate_closed";

	private const string FinishGateOpenTag = "finish_gate_open";

	private const string NameOfTheHorse = "old_horse";

	private readonly TextObject _trainingFinishedText = new TextObject("{=cRvSuYC8}Choose another weapon or go to another training area.", (Dictionary<string, object>)null);

	private readonly List<DelayedAction> _delayedActions = new List<DelayedAction>();

	private MissionConversationLogic _missionConversationHandler;

	private readonly List<TutorialArea> _trainingAreas = new List<TutorialArea>();

	private TutorialArea _activeTutorialArea;

	private bool _courseFinished;

	private int _trainingProgress;

	private int _trainingSubTypeIndex = -1;

	private string _activeTrainingSubTypeTag = "";

	private float _beginningTime;

	private float _timeScore;

	private bool _showTutorialObjectivesAnyway;

	private Dictionary<string, float> _tutorialScores;

	private GameEntity _shootingPosition;

	private Agent _bowNpc;

	private WorldPosition _rangedNpcSpawnPosition;

	private WorldPosition _rangedTargetPosition;

	private Vec3 _rangedTargetRotation;

	private GameEntity _rangedNpcSpawnPoint;

	private int _rangedLastBrokenTargetCount;

	private readonly List<DestructableComponent> _targetsForRangedNpc = new List<DestructableComponent>();

	private DestructableComponent _lastTargetGiven;

	private bool _atShootingPosition;

	private bool _targetPositionSet;

	private readonly List<TutorialObjective> _rangedObjectives = new List<TutorialObjective>
	{
		new TutorialObjective("ranged_go_to_shooting_position"),
		new TutorialObjective("ranged_shoot_targets")
	};

	private readonly TextObject _remainingTargetText = new TextObject("{=gBbm9beO}Hit all of the targets. {REMAINING_TARGET} {?REMAINING_TARGET>1}targets{?}target{\\?} left.", (Dictionary<string, object>)null);

	private Agent _meleeTrainer;

	private WorldPosition _meleeTrainerDefaultPosition;

	private float _timer;

	private readonly List<TutorialObjective> _meleeObjectives = new List<TutorialObjective>
	{
		new TutorialObjective("melee_go_to_trainer"),
		new TutorialObjective("melee_defense", isFinished: false, isActive: false, hasBackground: true),
		new TutorialObjective("melee_attack", isFinished: false, isActive: false, hasBackground: true)
	};

	private Agent _advancedMeleeTrainerEasy;

	private Agent _advancedMeleeTrainerNormal;

	private float _playerCampaignHealth;

	private float _playerHealth = 100f;

	private float _advancedMeleeTrainerEasyHealth = 100f;

	private float _advancedMeleeTrainerNormalHealth = 100f;

	private MatrixFrame _advancedMeleeTrainerEasyInitialPosition;

	private MatrixFrame _advancedMeleeTrainerEasySecondPosition;

	private MatrixFrame _advancedMeleeTrainerNormalInitialPosition;

	private MatrixFrame _advancedMeleeTrainerNormalSecondPosition;

	private readonly TextObject _fightStartsIn = new TextObject("{=TNxWBS07}Fight will start in {REMAINING_TIME} {?REMAINING_TIME>1}seconds{?}second{\\?}...", (Dictionary<string, object>)null);

	private readonly List<TutorialObjective> _advMeleeObjectives = new List<TutorialObjective>
	{
		new TutorialObjective("adv_melee_go_to_trainer"),
		new TutorialObjective("adv_melee_beat_easy_trainer"),
		new TutorialObjective("adv_melee_beat_normal_trainer")
	};

	private bool _playerLeftBattleArea;

	private GameEntity _finishGateClosed;

	private GameEntity _finishGateOpen;

	private int _finishGateStatus;

	private readonly List<(VolumeBox, bool)> _checkpoints = new List<(VolumeBox, bool)>();

	private int _currentCheckpointIndex = -1;

	private int _mountedLastBrokenTargetCount;

	private float _enteringDotProduct;

	private Agent _horse;

	private WorldPosition _horseBeginningPosition;

	private HorseReturningSituation _horseBehaviorMode = HorseReturningSituation.ReturnCompleted;

	private readonly List<TutorialObjective> _mountedObjectives = new List<TutorialObjective>
	{
		new TutorialObjective("mounted_mount_the_horse"),
		new TutorialObjective("mounted_hit_targets")
	};

	private Agent _mountedAI;

	private MatrixFrame _mountedAISpawnPosition;

	private MatrixFrame _mountedAIWaitingPosition;

	private int _mountedAICurrentCheckpointTarget = -1;

	private int _mountedAICurrentHitTarget;

	private bool _enteredRadiusOfTarget;

	private bool _allTargetsDestroyed;

	private readonly List<DestructableComponent> _mountedAITargets = new List<DestructableComponent>();

	private bool _continueLoop = true;

	private List<TutorialObjective> _detailedObjectives = new List<TutorialObjective>();

	private readonly List<TutorialObjective> _tutorialObjectives = new List<TutorialObjective>();

	public Action UIStartTimer;

	public Func<float> UIEndTimer;

	public Action<string> TimerTick;

	public Action<TextObject> CurrentObjectiveTick;

	public Action<MouseObjectives, ObjectivePerformingType> CurrentMouseObjectiveTick;

	public Action<List<TutorialObjective>> AllObjectivesTick;

	private static bool _updateObjectivesWillBeCalled;

	private Agent _brotherConversationAgent;

	public TextObject InitialCurrentObjective { get; private set; }

	public override void OnCreated()
	{
		((MissionBehavior)this).OnCreated();
		((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment = false;
	}

	public override void AfterStart()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		((MissionBehavior)this).AfterStart();
		((MissionBehavior)this).Mission.IsInventoryAccessible = false;
		((MissionBehavior)this).Mission.IsQuestScreenAccessible = false;
		((MissionBehavior)this).Mission.IsCharacterWindowAccessible = false;
		((MissionBehavior)this).Mission.IsPartyWindowAccessible = false;
		((MissionBehavior)this).Mission.IsKingdomWindowAccessible = false;
		((MissionBehavior)this).Mission.IsClanWindowAccessible = false;
		((MissionBehavior)this).Mission.IsEncyclopediaWindowAccessible = false;
		((MissionBehavior)this).Mission.IsBannerWindowAccessible = false;
		_missionConversationHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>();
		MissionHelper.SpawnPlayer(((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment, true, false, false, "");
		LoadTutorialScores();
		SpawnConversationBrother();
		CollectWeaponsAndObjectives();
		InitializeMeleeTraining();
		InitializeMountedTraining();
		InitializeAdvancedMeleeTraining();
		InitializeBowTraining();
		MakeAllAgentsImmortal();
		SetHorseMountable(mountable: false);
		InitialCurrentObjective = new TextObject("{=BTY2aZCt}Enter a training area.", (Dictionary<string, object>)null);
		_playerCampaignHealth = Agent.Main.Health;
	}

	private void LoadTutorialScores()
	{
		_tutorialScores = StoryModeManager.Current.MainStoryLine.GetTutorialScores();
	}

	protected override void OnEndMission()
	{
		((MissionBehavior)this).OnEndMission();
		Agent.Main.Health = _playerCampaignHealth;
		StoryModeManager.Current.MainStoryLine.SetTutorialScores(_tutorialScores);
	}

	public override void OnRenderingStarted()
	{
		((MissionBehavior)this).OnRenderingStarted();
		if (_brotherConversationAgent != null)
		{
			((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>().StartConversation(_brotherConversationAgent, false, true);
		}
	}

	public override void OnMissionTick(float dt)
	{
		TrainingAreaUpdate();
		UpdateHorseBehavior();
		UpdateBowTraining();
		UpdateMountedAIBehavior();
		if (_updateObjectivesWillBeCalled)
		{
			UpdateObjectives();
		}
		for (int num = _delayedActions.Count - 1; num >= 0; num--)
		{
			if (_delayedActions[num].Update())
			{
				_delayedActions.RemoveAt(num);
			}
		}
	}

	private void UpdateObjectives()
	{
		if (_trainingSubTypeIndex == -1 || _showTutorialObjectivesAnyway)
		{
			AllObjectivesTick?.Invoke(_tutorialObjectives);
		}
		else
		{
			AllObjectivesTick?.Invoke(_detailedObjectives);
		}
		_updateObjectivesWillBeCalled = false;
	}

	private int GetSelectedTrainingSubTypeIndex()
	{
		TrainingIcon activeTrainingIcon = _activeTutorialArea.GetActiveTrainingIcon();
		if (activeTrainingIcon != null)
		{
			EnableAllTrainingIcons();
			activeTrainingIcon.DisableIcon();
			_activeTrainingSubTypeTag = activeTrainingIcon.GetTrainingSubTypeTag();
			return _activeTutorialArea.GetIndexFromTag(activeTrainingIcon.GetTrainingSubTypeTag());
		}
		return -1;
	}

	private string GetHighlightedWeaponRack()
	{
		foreach (TrainingIcon item in (List<TrainingIcon>)(object)_activeTutorialArea.TrainingIconsReadOnly)
		{
			if (item.Focused)
			{
				return item.GetTrainingSubTypeTag();
			}
		}
		return "";
	}

	private void EnableAllTrainingIcons()
	{
		foreach (TrainingIcon item in (List<TrainingIcon>)(object)_activeTutorialArea.TrainingIconsReadOnly)
		{
			item.EnableIcon();
		}
	}

	private void TrainingAreaUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		CheckMainAgentEquipment();
		string[] array = default(string[]);
		if (_activeTutorialArea != null)
		{
			if (_activeTutorialArea.IsPositionInsideTutorialArea(Agent.Main.Position, ref array))
			{
				InTrainingArea();
				if (_trainingSubTypeIndex != -1)
				{
					_activeTutorialArea.CheckWeapons(_trainingSubTypeIndex);
				}
			}
			else
			{
				OnTrainingAreaExit(enableTrainingIcons: true);
				_activeTutorialArea = null;
			}
		}
		else
		{
			foreach (TutorialArea trainingArea in _trainingAreas)
			{
				if (trainingArea.IsPositionInsideTutorialArea(Agent.Main.Position, ref array))
				{
					_activeTutorialArea = trainingArea;
					OnTrainingAreaEnter();
					break;
				}
			}
		}
		UpdateConversationPermission();
	}

	private void UpdateConversationPermission()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (_brotherConversationAgent != null && Mission.Current.MainAgent != null)
		{
			Vec3 val = _brotherConversationAgent.Position - Mission.Current.MainAgent.Position;
			if (!(((Vec3)(ref val)).LengthSquared > 4f))
			{
				_missionConversationHandler.DisableStartConversation(false);
				return;
			}
		}
		_missionConversationHandler.DisableStartConversation(true);
	}

	private void ResetTrainingArea()
	{
		OnTrainingAreaExit(enableTrainingIcons: true);
		OnTrainingAreaEnter();
	}

	private void OnTrainingAreaExit(bool enableTrainingIcons)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		_activeTutorialArea.MarkTrainingIcons(false);
		TutorialObjective? tutorialObjective = _tutorialObjectives.Find((TutorialObjective x) => x.Id == ((object)_activeTutorialArea.TypeOfTraining/*cast due to .constrained prefix*/).ToString());
		tutorialObjective.SetActive(isActive: false);
		tutorialObjective.SetAllSubTasksInactive();
		DropAllWeaponsOfMainAgent();
		SpecialTrainingAreaExit(_activeTutorialArea.TypeOfTraining);
		_activeTutorialArea.DeactivateAllWeapons(true);
		_trainingProgress = 0;
		_trainingSubTypeIndex = -1;
		EnableAllTrainingIcons();
		if (CheckAllObjectivesFinished())
		{
			CurrentObjectiveTick(new TextObject("{=77TavbOY}You have completed all tutorials. You can always come back to improve your score.", (Dictionary<string, object>)null));
			if (!_courseFinished)
			{
				_courseFinished = true;
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/finish_course"), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
			}
		}
		else
		{
			CurrentObjectiveTick(new TextObject("{=BTY2aZCt}Enter a training area.", (Dictionary<string, object>)null));
		}
		TickMouseObjective(MouseObjectives.None);
		UIEndTimer();
	}

	private bool CheckAllObjectivesFinished()
	{
		foreach (TutorialObjective tutorialObjective in _tutorialObjectives)
		{
			if (!tutorialObjective.IsFinished)
			{
				return false;
			}
		}
		return true;
	}

	private void OnTrainingAreaEnter()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		_tutorialObjectives.Find((TutorialObjective x) => x.Id == ((object)_activeTutorialArea.TypeOfTraining/*cast due to .constrained prefix*/).ToString()).SetActive(isActive: true);
		DropAllWeaponsOfMainAgent();
		_trainingProgress = 0;
		_trainingSubTypeIndex = -1;
		SpecialTrainingAreaEnter(_activeTutorialArea.TypeOfTraining);
		CurrentObjectiveTick(new TextObject("{=WIUbM9Hc}Choose a weapon to begin training.", (Dictionary<string, object>)null));
		_activeTutorialArea.MarkTrainingIcons(true);
	}

	private void InTrainingArea()
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		int selectedTrainingSubTypeIndex = GetSelectedTrainingSubTypeIndex();
		if (selectedTrainingSubTypeIndex >= 0)
		{
			OnStartTraining(selectedTrainingSubTypeIndex);
		}
		else
		{
			string highlightedWeaponRack = GetHighlightedWeaponRack();
			if (highlightedWeaponRack != "")
			{
				foreach (TutorialObjective tutorialObjective in _tutorialObjectives)
				{
					if (!(tutorialObjective.Id == ((object)_activeTutorialArea.TypeOfTraining/*cast due to .constrained prefix*/).ToString()))
					{
						continue;
					}
					foreach (TutorialObjective subTask in tutorialObjective.SubTasks)
					{
						if (subTask.Id == highlightedWeaponRack)
						{
							subTask.SetActive(isActive: true);
						}
						else
						{
							subTask.SetActive(isActive: false);
						}
					}
					break;
				}
			}
			else
			{
				_tutorialObjectives.Find((TutorialObjective x) => x.Id == ((object)_activeTutorialArea.TypeOfTraining/*cast due to .constrained prefix*/).ToString()).SetAllSubTasksInactive();
			}
		}
		SpecialInTrainingAreaUpdate(_activeTutorialArea.TypeOfTraining);
	}

	private void OnStartTraining(int index)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_showTutorialObjectivesAnyway = false;
		_activeTutorialArea.MarkTrainingIcons(false);
		SpecialTrainingStart(_activeTutorialArea.TypeOfTraining);
		TickMouseObjective(MouseObjectives.None);
		UIEndTimer();
		DropAllWeaponsOfMainAgent();
		_activeTutorialArea.DeactivateAllWeapons(true);
		_activeTutorialArea.ActivateTaggedWeapons(index);
		_activeTutorialArea.EquipWeaponsToPlayer(index);
		_trainingProgress = 1;
		_trainingSubTypeIndex = index;
		UpdateObjectives();
	}

	private void SuccessfullyFinishTraining(float score)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		_tutorialObjectives.Find((TutorialObjective x) => x.Id == ((object)_activeTutorialArea.TypeOfTraining/*cast due to .constrained prefix*/).ToString()).FinishSubTask(_activeTrainingSubTypeTag, score);
		_tutorialScores[_activeTrainingSubTypeTag] = score;
		_activeTutorialArea.MarkTrainingIcons(true);
		Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/finish_task"), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
		_showTutorialObjectivesAnyway = true;
		UpdateObjectives();
	}

	private static void RefillAmmoOfAgent(Agent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			MissionWeapon val2 = agent.Equipment[val];
			if (((MissionWeapon)(ref val2)).IsAnyConsumable())
			{
				val2 = agent.Equipment[val];
				if (((MissionWeapon)(ref val2)).Amount <= 1)
				{
					EquipmentIndex val3 = val;
					val2 = agent.Equipment[val];
					agent.SetWeaponAmountInSlot(val3, ((MissionWeapon)(ref val2)).ModifiedMaxAmount, true);
				}
			}
		}
	}

	private void SpecialTrainingAreaExit(TrainingType trainingType)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected I4, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (_trainingSubTypeIndex != -1)
		{
			_activeTutorialArea.ResetBreakables(_trainingSubTypeIndex, true);
		}
		switch ((int)trainingType)
		{
		case 0:
			OnBowTrainingExit();
			break;
		case 2:
			OnMountedTrainingExit();
			break;
		case 3:
			OnAdvancedTrainingExit();
			break;
		default:
			throw new ArgumentOutOfRangeException("trainingType", trainingType, null);
		case 1:
			break;
		}
	}

	private void SpecialTrainingAreaEnter(TrainingType trainingType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((int)trainingType > 2)
		{
			if ((int)trainingType != 3)
			{
				throw new ArgumentOutOfRangeException("trainingType", trainingType, null);
			}
			OnAdvancedTrainingAreaEnter();
		}
	}

	private void SpecialTrainingStart(TrainingType trainingType)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected I4, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (_trainingSubTypeIndex != -1)
		{
			_activeTutorialArea.ResetBreakables(_trainingSubTypeIndex, true);
		}
		switch ((int)trainingType)
		{
		case 0:
			OnBowTrainingStart();
			break;
		case 2:
			OnMountedTrainingStart();
			break;
		case 3:
			OnAdvancedTrainingStart();
			break;
		default:
			throw new ArgumentOutOfRangeException("trainingType", trainingType, null);
		case 1:
			break;
		}
	}

	private void SpecialInTrainingAreaUpdate(TrainingType trainingType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected I4, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		switch ((int)trainingType)
		{
		case 0:
			BowInTrainingAreaUpdate();
			break;
		case 1:
			MeleeTrainingUpdate();
			break;
		case 2:
			MountedTrainingUpdate();
			break;
		case 3:
			AdvancedMeleeTrainingUpdate();
			break;
		default:
			throw new ArgumentOutOfRangeException("trainingType", trainingType, null);
		}
	}

	private static void DropAllWeaponsOfMainAgent()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		Mission.Current.MainAgent.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val <= 3; val = (EquipmentIndex)(val + 1))
		{
			MissionWeapon val2 = Mission.Current.MainAgent.Equipment[val];
			if (!((MissionWeapon)(ref val2)).IsEmpty)
			{
				Mission.Current.MainAgent.DropItem(val, (WeaponClass)0);
			}
		}
	}

	private static void RemoveAllWeaponsFromMainAgent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val <= 3; val = (EquipmentIndex)(val + 1))
		{
			MissionWeapon val2 = Mission.Current.MainAgent.Equipment[val];
			if (!((MissionWeapon)(ref val2)).IsEmpty)
			{
				Mission.Current.MainAgent.RemoveEquippedWeapon(val);
			}
		}
	}

	private void CollectWeaponsAndObjectives()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		List<GameEntity> list = new List<GameEntity>();
		Mission.Current.Scene.GetEntities(ref list);
		foreach (GameEntity item in list)
		{
			if (item.HasTag("bow_training_shooting_position"))
			{
				_shootingPosition = item;
			}
			if (item.GetFirstScriptOfType<TutorialArea>() != null)
			{
				_trainingAreas.Add(item.GetFirstScriptOfType<TutorialArea>());
				_tutorialObjectives.Add(new TutorialObjective(((object)_trainingAreas[_trainingAreas.Count - 1].TypeOfTraining/*cast due to .constrained prefix*/).ToString()));
				foreach (string subTrainingTag in _trainingAreas[_trainingAreas.Count - 1].GetSubTrainingTags())
				{
					_tutorialObjectives[_tutorialObjectives.Count - 1].AddSubTask(new TutorialObjective(subTrainingTag));
					if (_tutorialScores.TryGetValue(subTrainingTag, out var value))
					{
						_tutorialObjectives[_tutorialObjectives.Count - 1].SubTasks.Last().RestoreScoreFromSave(value);
					}
				}
			}
			if (item.HasTag("mounted_checkpoint") && item.GetFirstScriptOfType<VolumeBox>() != null)
			{
				bool flag = false;
				for (int i = 0; i < _checkpoints.Count; i++)
				{
					int num = int.Parse(item.Tags[1]);
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)_checkpoints[i].Item1).GameEntity;
					if (num < int.Parse(((WeakGameEntity)(ref gameEntity)).Tags[1]))
					{
						_checkpoints.Insert(i, ValueTuple.Create<VolumeBox, bool>(item.GetFirstScriptOfType<VolumeBox>(), item2: false));
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_checkpoints.Add(ValueTuple.Create<VolumeBox, bool>(item.GetFirstScriptOfType<VolumeBox>(), item2: false));
				}
			}
			if (!item.HasScriptOfType<DestructableComponent>())
			{
				continue;
			}
			if (item.HasTag("_ranged_npc_target"))
			{
				_targetsForRangedNpc.Add(item.GetFirstScriptOfType<DestructableComponent>());
			}
			else if (item.HasTag("_mounted_ai_target"))
			{
				int num2 = int.Parse(item.Tags[1]);
				while (num2 > _mountedAITargets.Count - 1)
				{
					_mountedAITargets.Add(null);
				}
				_mountedAITargets[num2] = item.GetFirstScriptOfType<DestructableComponent>();
			}
		}
	}

	private static void MakeAllAgentsImmortal()
	{
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			item.SetMortalityState((MortalityState)2);
			if (!item.IsMount)
			{
				item.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
			}
		}
	}

	private bool HasAllWeaponsPicked()
	{
		return _activeTutorialArea.HasMainAgentPickedAll(_trainingSubTypeIndex);
	}

	private void CheckMainAgentEquipment()
	{
		if (_trainingSubTypeIndex == -1)
		{
			RemoveAllWeaponsFromMainAgent();
		}
		else
		{
			_activeTutorialArea.CheckMainAgentEquipment(_trainingSubTypeIndex);
		}
	}

	private void StartTimer()
	{
		_beginningTime = ((MissionBehavior)this).Mission.CurrentTime;
	}

	private void EndTimer()
	{
		_timeScore = ((MissionBehavior)this).Mission.CurrentTime - _beginningTime;
	}

	private void SpawnConversationBrother()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		if (!TutorialPhase.Instance.TalkedWithBrotherForTheFirstTime)
		{
			WorldFrame val = default(WorldFrame);
			((WorldFrame)(ref val))._002Ector(Agent.Main.Frame.rotation, new WorldPosition(((MissionBehavior)this).Mission.Scene, Agent.Main.Position));
			ref WorldPosition origin = ref val.Origin;
			WorldFrame worldFrame = Agent.Main.GetWorldFrame();
			((WorldPosition)(ref origin)).SetVec2(((WorldPosition)(ref worldFrame.Origin)).AsVec2 + Vec2.Forward * 3f);
			((Mat3)(ref val.Rotation)).RotateAboutUp(MathF.PI);
			MatrixFrame val2 = ((WorldFrame)(ref val)).ToGroundMatrixFrame();
			CharacterObject characterObject = StoryModeHeroes.ElderBrother.CharacterObject;
			AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)characterObject).Team(((MissionBehavior)this).Mission.SpectatorTeam).InitialPosition(ref val2.origin);
			Vec2 val3 = ((Vec3)(ref val2.rotation.f)).AsVec2;
			val3 = ((Vec2)(ref val3)).Normalized();
			AgentBuildData obj2 = obj.InitialDirection(ref val3).CivilianEquipment(false).NoHorses(true)
				.NoWeapons(true)
				.ClothingColor1(((MissionBehavior)this).Mission.PlayerTeam.Color)
				.ClothingColor2(((MissionBehavior)this).Mission.PlayerTeam.Color2)
				.TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, characterObject, -1, default(UniqueTroopDescriptor), false, false));
			EquipmentElement val4 = ((BasicCharacterObject)characterObject).Equipment[(EquipmentIndex)10];
			AgentBuildData val5 = obj2.MountKey(MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val4)).Item, ((BasicCharacterObject)characterObject).GetMountKeySeed()));
			_brotherConversationAgent = ((MissionBehavior)this).Mission.SpawnAgent(val5, false);
		}
	}

	private void InitializeBowTraining()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		_shootingPosition.SetVisibilityExcludeParents(false);
		_bowNpc = SpawnBowNPC();
		_rangedNpcSpawnPosition = _bowNpc.GetWorldPosition();
		AgentComponentExtensions.SetAIBehaviorValues(_bowNpc, (AISimpleBehaviorKind)2, 0f, 6f, 0f, 66f, 0f);
		AgentComponentExtensions.SetAIBehaviorValues(_bowNpc, (AISimpleBehaviorKind)0, 0f, 6f, 0f, 66f, 0f);
		AgentComponentExtensions.SetAIBehaviorValues(_bowNpc, (AISimpleBehaviorKind)6, 66666f, 6f, 66666f, 120f, 66666f);
		WorldPosition worldPosition = ModuleExtensions.ToWorldPosition(_shootingPosition.GlobalPosition);
		MatrixFrame globalFrame = _shootingPosition.GetGlobalFrame();
		GiveMoveOrderToRangedAgent(worldPosition, ((Vec3)(ref globalFrame.rotation.f)).NormalizedCopy());
	}

	private void GiveMoveOrderToRangedAgent(WorldPosition worldPosition, Vec3 rotation)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Vec2 asVec = ((WorldPosition)(ref worldPosition)).AsVec2;
		if (((Vec2)(ref asVec)).NearlyEquals(((WorldPosition)(ref _rangedTargetPosition)).AsVec2, 0.001f))
		{
			Vec3 groundVec = ((WorldPosition)(ref worldPosition)).GetGroundVec3();
			Vec3 groundVec2 = ((WorldPosition)(ref _rangedTargetPosition)).GetGroundVec3();
			if (((Vec3)(ref groundVec)).NearlyEquals(ref groundVec2, 0.001f) && ((Vec3)(ref rotation)).NearlyEquals(ref _rangedTargetRotation, 1E-05f))
			{
				return;
			}
		}
		_rangedTargetPosition = worldPosition;
		_rangedTargetRotation = rotation;
		_bowNpc.SetWatchState((WatchState)0);
		_targetPositionSet = false;
		_delayedActions.Add(new DelayedAction(delegate
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			_bowNpc.ClearTargetFrame();
			Agent bowNpc = _bowNpc;
			ref WorldPosition reference = ref worldPosition;
			Vec2 asVec2 = ((Vec3)(ref _rangedTargetRotation)).AsVec2;
			bowNpc.SetScriptedPositionAndDirection(ref reference, ((Vec2)(ref asVec2)).RotationInRadians, true, (AIScriptedFrameFlags)0);
		}, 2f));
	}

	private WeakGameEntity GetValidTarget()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		foreach (DestructableComponent item in _targetsForRangedNpc)
		{
			if (!item.IsDestroyed)
			{
				_lastTargetGiven = item;
				return ((ScriptComponentBehavior)_lastTargetGiven).GameEntity;
			}
		}
		foreach (DestructableComponent item2 in _targetsForRangedNpc)
		{
			item2.Reset();
		}
		_lastTargetGiven = _targetsForRangedNpc[0];
		return ((ScriptComponentBehavior)_lastTargetGiven).GameEntity;
	}

	private void UpdateBowTraining()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		if ((_bowNpc.MovementFlags & 0x3F) != 0)
		{
			return;
		}
		Vec3 val = ((WorldPosition)(ref _rangedTargetPosition)).GetGroundVec3() - _bowNpc.Position;
		if (!(((Vec3)(ref val)).LengthSquared < 0.16000001f))
		{
			return;
		}
		if (!_targetPositionSet)
		{
			_bowNpc.DisableScriptedMovement();
			Agent bowNpc = _bowNpc;
			val = _bowNpc.Position;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			bowNpc.SetTargetPositionAndDirection(ref asVec, ref _rangedTargetRotation);
			_targetPositionSet = true;
			val = _bowNpc.Position - _shootingPosition.GlobalPosition;
			float lengthSquared = ((Vec3)(ref val)).LengthSquared;
			val = _bowNpc.Position - ((WorldPosition)(ref _rangedNpcSpawnPosition)).GetGroundVec3();
			if (lengthSquared > ((Vec3)(ref val)).LengthSquared)
			{
				_atShootingPosition = false;
				return;
			}
			_bowNpc.SetWatchState((WatchState)2);
			_bowNpc.SetScriptedTargetEntity(GetValidTarget(), (AISpecialCombatModeFlags)0, false);
			_atShootingPosition = true;
		}
		else if (_atShootingPosition && _lastTargetGiven.IsDestroyed)
		{
			_bowNpc.SetScriptedTargetEntity(GetValidTarget(), (AISpecialCombatModeFlags)0, false);
		}
	}

	private Agent SpawnBowNPC()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = MatrixFrame.Identity;
		_rangedNpcSpawnPoint = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawner_ranged_npc_tag");
		if (_rangedNpcSpawnPoint != (GameEntity)null)
		{
			val = _rangedNpcSpawnPoint.GetGlobalFrame();
			((Mat3)(ref val.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		}
		else
		{
			Debug.FailedAssert("There are no spawn points for bow npc.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Missions\\TrainingFieldMissionController.cs", "SpawnBowNPC", 1091);
		}
		Location locationWithId = LocationComplex.Current.GetLocationWithId("training_field");
		CharacterObject val2 = Game.Current.ObjectManager.GetObject<CharacterObject>("tutorial_npc_ranged");
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(((BasicCharacterObject)val2).Race);
		AgentData val3 = new AgentData((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val2, -1, default(UniqueTroopDescriptor), false, false)).Monster(baseMonsterFromRace).NoHorses(true);
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		locationWithId.AddCharacter(new LocationCharacter(val3, new AddBehaviorsDelegate(agentBehaviorManager.AddFixedCharacterBehaviors), (string)null, true, (CharacterRelations)1, (string)null, true, false, (ItemObject)null, false, true, true, (AfterAgentCreatedDelegate)null, false));
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val2).Team(((MissionBehavior)this).Mission.PlayerTeam).InitialPosition(ref val.origin);
		Vec2 asVec = ((Vec3)(ref val.rotation.f)).AsVec2;
		AgentBuildData obj2 = obj.InitialDirection(ref asVec).CivilianEquipment(false).NoHorses(true)
			.NoWeapons(false)
			.ClothingColor1(((MissionBehavior)this).Mission.PlayerTeam.Color)
			.ClothingColor2(((MissionBehavior)this).Mission.PlayerTeam.Color2)
			.TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val2, -1, default(UniqueTroopDescriptor), false, false));
		EquipmentElement val4 = ((BasicCharacterObject)val2).Equipment[(EquipmentIndex)10];
		AgentBuildData val5 = obj2.MountKey(MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val4)).Item, ((BasicCharacterObject)val2).GetMountKeySeed())).Controller((AgentControllerType)1);
		Agent obj3 = ((MissionBehavior)this).Mission.SpawnAgent(val5, false);
		obj3.SetTeam(Mission.Current.PlayerAllyTeam, false);
		return obj3;
	}

	private void BowInTrainingAreaUpdate()
	{
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		switch (_trainingProgress)
		{
		case 1:
			if (HasAllWeaponsPicked())
			{
				_rangedLastBrokenTargetCount = 0;
				LoadCrossbowForStarting();
				_trainingProgress++;
				CurrentObjectiveTick(new TextObject("{=kwW6v202}Go to shooting position", (Dictionary<string, object>)null));
				_shootingPosition.SetVisibilityExcludeParents(true);
				_detailedObjectives = _rangedObjectives.ConvertAll((TutorialObjective x) => new TutorialObjective(x.Id, x.IsFinished, x.IsActive, x.HasBackground));
				_detailedObjectives[1].SetTextVariableOfName("HIT", _activeTutorialArea.GetBrokenBreakableCount(_trainingSubTypeIndex));
				_detailedObjectives[1].SetTextVariableOfName("ALL", _activeTutorialArea.GetBreakablesCount(_trainingSubTypeIndex));
				_detailedObjectives[0].SetActive(isActive: true);
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/archery/pick_" + _trainingSubTypeIndex), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
			}
			break;
		case 2:
		{
			Vec3 val = _shootingPosition.GetGlobalFrame().origin - Agent.Main.Position;
			if (((Vec3)(ref val)).LengthSquared < 4f)
			{
				_trainingProgress++;
				_shootingPosition.SetVisibilityExcludeParents(false);
				_activeTutorialArea.MarkAllTargets(_trainingSubTypeIndex, true);
				_remainingTargetText.SetTextVariable("REMAINING_TARGET", _activeTutorialArea.GetUnbrokenBreakableCount(_trainingSubTypeIndex));
				CurrentObjectiveTick(_remainingTargetText);
				_detailedObjectives[0].FinishTask();
				_detailedObjectives[1].SetActive(isActive: true);
			}
			break;
		}
		case 4:
		{
			int brokenBreakableCount = _activeTutorialArea.GetBrokenBreakableCount(_trainingSubTypeIndex);
			_remainingTargetText.SetTextVariable("REMAINING_TARGET", _activeTutorialArea.GetUnbrokenBreakableCount(_trainingSubTypeIndex));
			CurrentObjectiveTick(_remainingTargetText);
			_detailedObjectives[1].SetTextVariableOfName("HIT", brokenBreakableCount);
			if (brokenBreakableCount != _rangedLastBrokenTargetCount)
			{
				_rangedLastBrokenTargetCount = brokenBreakableCount;
				_activeTutorialArea.ResetMarkingTargetTimers(_trainingSubTypeIndex);
			}
			if (MBRandom.NondeterministicRandomInt % 4 == 3)
			{
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/archery/hit_target"), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
			}
			if (_activeTutorialArea.AllBreakablesAreBroken(_trainingSubTypeIndex))
			{
				_detailedObjectives[1].FinishTask();
				_trainingProgress++;
				BowTrainingEndedSuccessfully();
			}
			break;
		}
		case 3:
			break;
		}
	}

	public void LoadCrossbowForStarting()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		int num = default(int);
		EquipmentIndex val3 = default(EquipmentIndex);
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			MissionWeapon val2 = Agent.Main.Equipment[val];
			if (!((MissionWeapon)(ref val2)).IsEmpty && (int)((MissionWeapon)(ref val2)).Item.PrimaryWeapon.WeaponClass == 17 && ((MissionWeapon)(ref val2)).Ammo == 0)
			{
				Agent.Main.Equipment.GetAmmoCountAndIndexOfType(((MissionWeapon)(ref val2)).Item.Type, ref num, ref val3, (EquipmentIndex)(-1));
				Agent.Main.SetReloadAmmoInSlot(val, val3, (short)1);
				Agent.Main.SetWeaponReloadPhaseAsClient(val, ((MissionWeapon)(ref val2)).ReloadPhaseCount);
			}
		}
	}

	public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentShootMissile(shooterAgent, weaponIndex, position, velocity, orientation, hasRigidBody, forcedMissileIndex);
		TutorialArea activeTutorialArea = _activeTutorialArea;
		if (activeTutorialArea != null && (int)activeTutorialArea.TypeOfTraining == 0 && _trainingProgress == 3)
		{
			_trainingProgress++;
			_activeTutorialArea.MakeDestructible(_trainingSubTypeIndex);
			UIStartTimer();
			CurrentObjectiveTick(new TextObject("{=9kGnzjrU}Timer Started.", (Dictionary<string, object>)null));
			StartTimer();
			Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/archery/start_training"), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
		}
		RefillAmmoOfAgent(shooterAgent);
	}

	private void BowTrainingEndedSuccessfully()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0064: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		EndTimer();
		_activeTutorialArea.HideBoundaries();
		CurrentObjectiveTick(_trainingFinishedText);
		TextObject val = new TextObject("{=xVFupnFu}You've successfully hit all of the targets in ({TIME_SCORE}) seconds.", (Dictionary<string, object>)null);
		float score = UIEndTimer();
		val.SetTextVariable("TIME_SCORE", new TextObject(score.ToString("0.0"), (Dictionary<string, object>)null));
		MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
		SuccessfullyFinishTraining(score);
		_shootingPosition.SetVisibilityExcludeParents(false);
		Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/archery/finish"), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
	}

	private void OnBowTrainingStart()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		_shootingPosition.SetVisibilityExcludeParents(false);
		WorldPosition worldPosition = ModuleExtensions.ToWorldPosition(_rangedNpcSpawnPoint.GlobalPosition);
		MatrixFrame globalFrame = _rangedNpcSpawnPoint.GetGlobalFrame();
		GiveMoveOrderToRangedAgent(worldPosition, ((Vec3)(ref globalFrame.rotation.f)).NormalizedCopy());
		foreach (DestructableComponent item in _targetsForRangedNpc)
		{
			item.Reset();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
		}
	}

	private void OnBowTrainingExit()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		_shootingPosition.SetVisibilityExcludeParents(false);
		WorldPosition worldPosition = ModuleExtensions.ToWorldPosition(_shootingPosition.GlobalPosition);
		MatrixFrame globalFrame = _shootingPosition.GetGlobalFrame();
		GiveMoveOrderToRangedAgent(worldPosition, ((Vec3)(ref globalFrame.rotation.f)).NormalizedCopy());
		foreach (DestructableComponent item in _targetsForRangedNpc)
		{
			item.Reset();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(true);
		}
	}

	private void InitializeAdvancedMeleeTraining()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		_advancedMeleeTrainerEasy = SpawnAdvancedMeleeTrainerEasy();
		_advancedMeleeTrainerEasy.SetAgentFlags((AgentFlag)(_advancedMeleeTrainerEasy.GetAgentFlags() & -65537));
		_advancedMeleeTrainerEasyInitialPosition = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawner_adv_melee_npc_easy").GetGlobalFrame();
		_advancedMeleeTrainerEasySecondPosition = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("adv_melee_npc_easy_second_pos").GetGlobalFrame();
		_advancedMeleeTrainerNormal = SpawnAdvancedMeleeTrainerNormal();
		_advancedMeleeTrainerNormal.SetAgentFlags((AgentFlag)(_advancedMeleeTrainerNormal.GetAgentFlags() & -65537));
		_advancedMeleeTrainerNormalInitialPosition = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawner_adv_melee_npc_normal").GetGlobalFrame();
		_advancedMeleeTrainerNormalSecondPosition = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("adv_melee_npc_normal_second_pos").GetGlobalFrame();
		BeginNPCFight();
	}

	private Agent SpawnAdvancedMeleeTrainerEasy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		_advancedMeleeTrainerEasyInitialPosition = MatrixFrame.Identity;
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawner_adv_melee_npc_easy");
		if (val != (GameEntity)null)
		{
			_advancedMeleeTrainerEasyInitialPosition = val.GetGlobalFrame();
			((Mat3)(ref _advancedMeleeTrainerEasyInitialPosition.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		}
		else
		{
			Debug.FailedAssert("There are no spawn points for advanced melee trainer.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Missions\\TrainingFieldMissionController.cs", "SpawnAdvancedMeleeTrainerEasy", 1319);
		}
		CharacterObject val2 = Game.Current.ObjectManager.GetObject<CharacterObject>("tutorial_npc_advanced_melee_easy");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val2).Team(((MissionBehavior)this).Mission.PlayerTeam).InitialPosition(ref _advancedMeleeTrainerEasyInitialPosition.origin);
		Vec2 asVec = ((Vec3)(ref _advancedMeleeTrainerEasyInitialPosition.rotation.f)).AsVec2;
		AgentBuildData obj2 = obj.InitialDirection(ref asVec).CivilianEquipment(false).NoHorses(true)
			.NoWeapons(false)
			.ClothingColor1(((MissionBehavior)this).Mission.PlayerTeam.Color)
			.ClothingColor2(((MissionBehavior)this).Mission.PlayerTeam.Color2)
			.TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val2, -1, default(UniqueTroopDescriptor), false, false));
		EquipmentElement val3 = ((BasicCharacterObject)val2).Equipment[(EquipmentIndex)10];
		AgentBuildData val4 = obj2.MountKey(MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val3)).Item, ((BasicCharacterObject)val2).GetMountKeySeed())).Controller((AgentControllerType)1);
		Agent obj3 = ((MissionBehavior)this).Mission.SpawnAgent(val4, false);
		obj3.SetTeam(Mission.Current.DefenderTeam, false);
		return obj3;
	}

	private Agent SpawnAdvancedMeleeTrainerNormal()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		_advancedMeleeTrainerNormalInitialPosition = MatrixFrame.Identity;
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawner_adv_melee_npc_normal");
		if (val != (GameEntity)null)
		{
			_advancedMeleeTrainerNormalInitialPosition = val.GetGlobalFrame();
			((Mat3)(ref _advancedMeleeTrainerNormalInitialPosition.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		}
		else
		{
			Debug.FailedAssert("There are no spawn points for advanced melee trainer.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Missions\\TrainingFieldMissionController.cs", "SpawnAdvancedMeleeTrainerNormal", 1351);
		}
		CharacterObject val2 = Game.Current.ObjectManager.GetObject<CharacterObject>("tutorial_npc_advanced_melee_normal");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val2).Team(((MissionBehavior)this).Mission.PlayerTeam).InitialPosition(ref _advancedMeleeTrainerNormalInitialPosition.origin);
		Vec2 asVec = ((Vec3)(ref _advancedMeleeTrainerNormalInitialPosition.rotation.f)).AsVec2;
		AgentBuildData obj2 = obj.InitialDirection(ref asVec).CivilianEquipment(false).NoHorses(true)
			.NoWeapons(false)
			.ClothingColor1(((MissionBehavior)this).Mission.PlayerTeam.Color)
			.ClothingColor2(((MissionBehavior)this).Mission.PlayerTeam.Color2)
			.TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val2, -1, default(UniqueTroopDescriptor), false, false));
		EquipmentElement val3 = ((BasicCharacterObject)val2).Equipment[(EquipmentIndex)10];
		AgentBuildData val4 = obj2.MountKey(MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val3)).Item, ((BasicCharacterObject)val2).GetMountKeySeed())).Controller((AgentControllerType)1);
		Agent obj3 = ((MissionBehavior)this).Mission.SpawnAgent(val4, false);
		obj3.SetTeam(Mission.Current.DefenderTeam, false);
		return obj3;
	}

	private void AdvancedMeleeTrainingUpdate()
	{
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Expected O, but got Unknown
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Expected O, but got Unknown
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a8: Expected O, but got Unknown
		//IL_0648: Unknown result type (might be due to invalid IL or missing references)
		//IL_0652: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Expected O, but got Unknown
		//IL_06f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Expected O, but got Unknown
		//IL_076f: Unknown result type (might be due to invalid IL or missing references)
		if (_trainingSubTypeIndex == -1)
		{
			return;
		}
		Vec3 val2;
		switch (_trainingProgress)
		{
		case 1:
			if (HasAllWeaponsPicked())
			{
				_playerLeftBattleArea = false;
				_detailedObjectives = _advMeleeObjectives.ConvertAll((TutorialObjective x) => new TutorialObjective(x.Id, x.IsFinished, x.IsActive, x.HasBackground));
				_detailedObjectives[0].SetActive(isActive: true);
				_trainingProgress++;
				CurrentObjectiveTick(new TextObject("{=HhuBPfJn}Go to the trainer.", (Dictionary<string, object>)null));
				WorldPosition val = ModuleExtensions.ToWorldPosition(_advancedMeleeTrainerNormalSecondPosition.origin);
				Agent advancedMeleeTrainerNormal = _advancedMeleeTrainerNormal;
				Vec2 asVec = ((Vec3)(ref _advancedMeleeTrainerNormalSecondPosition.rotation.f)).AsVec2;
				advancedMeleeTrainerNormal.SetScriptedPositionAndDirection(ref val, ((Vec2)(ref asVec)).RotationInRadians, true, (AIScriptedFrameFlags)0);
				_advancedMeleeTrainerNormal.SetTeam(Mission.Current.PlayerAllyTeam, false);
				_advancedMeleeTrainerEasy.SetTeam(Mission.Current.PlayerAllyTeam, false);
			}
			break;
		case 2:
			val2 = _advancedMeleeTrainerEasy.Position - Agent.Main.Position;
			if (((Vec3)(ref val2)).LengthSquared < 6f)
			{
				_detailedObjectives[0].FinishTask();
				_detailedObjectives[1].SetActive(isActive: true);
				_timer = ((MissionBehavior)this).Mission.CurrentTime;
				_trainingProgress++;
				_fightStartsIn.SetTextVariable("REMAINING_TIME", 3);
				CurrentObjectiveTick(_fightStartsIn);
			}
			break;
		case 3:
			if (((MissionBehavior)this).Mission.CurrentTime - _timer > 3f)
			{
				_playerHealth = Agent.Main.HealthLimit;
				_advancedMeleeTrainerEasyHealth = _advancedMeleeTrainerEasy.HealthLimit;
				_advancedMeleeTrainerNormal.SetTeam(Mission.Current.PlayerEnemyTeam, false);
				_advancedMeleeTrainerEasy.SetTeam(Mission.Current.PlayerEnemyTeam, false);
				_advancedMeleeTrainerEasy.SetWatchState((WatchState)2);
				_advancedMeleeTrainerEasy.DisableScriptedMovement();
				_trainingProgress++;
				CurrentObjectiveTick(new TextObject("{=4hdp6SK0}Defeat the trainer!", (Dictionary<string, object>)null));
			}
			else if (((MissionBehavior)this).Mission.CurrentTime - _timer > 2f)
			{
				_fightStartsIn.SetTextVariable("REMAINING_TIME", 1);
				CurrentObjectiveTick(_fightStartsIn);
			}
			else if (((MissionBehavior)this).Mission.CurrentTime - _timer > 1f)
			{
				_fightStartsIn.SetTextVariable("REMAINING_TIME", 2);
				CurrentObjectiveTick(_fightStartsIn);
			}
			break;
		case 4:
			if (_playerHealth <= 1f)
			{
				_trainingProgress = 9;
				CurrentObjectiveTick(new TextObject("{=SvYCz6z6}You've lost. You can restart the training by interacting weapon rack.", (Dictionary<string, object>)null));
				_timer = ((MissionBehavior)this).Mission.CurrentTime;
				Agent.Main.SetActionChannel(0, ref ActionIndexCache.act_strike_fall_back_back_rise, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				Agent.Main.Health = 1.1f;
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/fighting/player_lose"), _advancedMeleeTrainerNormal.GetEyeGlobalPosition(), true, false, -1, -1);
				OnLost();
			}
			else if (_advancedMeleeTrainerEasyHealth <= 1f)
			{
				_detailedObjectives[1].FinishTask();
				_detailedObjectives[2].SetActive(isActive: true);
				CurrentObjectiveTick(new TextObject("{=ikhWkw7T}You've successfully defeated rookie trainer. Go to veteran trainer.", (Dictionary<string, object>)null));
				_timer = ((MissionBehavior)this).Mission.CurrentTime;
				_trainingProgress++;
				OnEasyTrainerBeaten();
				_advancedMeleeTrainerNormal.SetTeam(Mission.Current.PlayerAllyTeam, false);
				_advancedMeleeTrainerEasy.SetTeam(Mission.Current.PlayerAllyTeam, false);
				_advancedMeleeTrainerEasy.SetActionChannel(0, ref ActionIndexCache.act_strike_fall_back_back_rise, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			}
			else
			{
				Agent.Main.Health = _playerHealth;
				CheckAndHandlePlayerInsideBattleArea();
			}
			break;
		case 5:
			val2 = _advancedMeleeTrainerNormal.Position - Agent.Main.Position;
			if (((Vec3)(ref val2)).LengthSquared < 6f)
			{
				val2 = _advancedMeleeTrainerNormal.Position - _advancedMeleeTrainerNormalInitialPosition.origin;
				if (((Vec3)(ref val2)).LengthSquared < 6f)
				{
					_timer = ((MissionBehavior)this).Mission.CurrentTime;
					_trainingProgress++;
					_fightStartsIn.SetTextVariable("REMAINING_TIME", 3);
					CurrentObjectiveTick(_fightStartsIn);
				}
			}
			break;
		case 6:
			if (((MissionBehavior)this).Mission.CurrentTime - _timer > 3f)
			{
				_playerHealth = Agent.Main.HealthLimit;
				_advancedMeleeTrainerNormalHealth = _advancedMeleeTrainerNormal.HealthLimit;
				_advancedMeleeTrainerNormal.SetTeam(Mission.Current.PlayerEnemyTeam, false);
				_advancedMeleeTrainerEasy.SetTeam(Mission.Current.PlayerEnemyTeam, false);
				_advancedMeleeTrainerNormal.SetWatchState((WatchState)2);
				_advancedMeleeTrainerNormal.DisableScriptedMovement();
				_trainingProgress++;
				CurrentObjectiveTick(new TextObject("{=4hdp6SK0}Defeat the trainer!", (Dictionary<string, object>)null));
			}
			else if (((MissionBehavior)this).Mission.CurrentTime - _timer > 2f)
			{
				_fightStartsIn.SetTextVariable("REMAINING_TIME", 1);
				CurrentObjectiveTick(_fightStartsIn);
			}
			else if (((MissionBehavior)this).Mission.CurrentTime - _timer > 1f)
			{
				_fightStartsIn.SetTextVariable("REMAINING_TIME", 2);
				CurrentObjectiveTick(_fightStartsIn);
			}
			break;
		case 7:
			if (_playerHealth <= 1f)
			{
				ResetTrainingArea();
				CurrentObjectiveTick(new TextObject("{=SvYCz6z6}You've lost. You can restart the training by interacting weapon rack.", (Dictionary<string, object>)null));
				_timer = ((MissionBehavior)this).Mission.CurrentTime;
				_trainingProgress++;
				Agent.Main.SetActionChannel(0, ref ActionIndexCache.act_strike_fall_back_back_rise, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				Agent.Main.Health = 1.1f;
				OnLost();
			}
			else if (_advancedMeleeTrainerNormalHealth <= 1f)
			{
				_detailedObjectives[2].FinishTask();
				SuccessfullyFinishTraining(0f);
				CurrentObjectiveTick(new TextObject("{=1RaUauBS}You've successfully finished the training.", (Dictionary<string, object>)null));
				_timer = ((MissionBehavior)this).Mission.CurrentTime;
				_trainingProgress++;
				MakeTrainersPatrolling();
				_advancedMeleeTrainerNormal.SetActionChannel(0, ref ActionIndexCache.act_strike_fall_back_back_rise, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/fighting/player_win"), _advancedMeleeTrainerNormal.GetEyeGlobalPosition(), true, false, -1, -1);
			}
			else
			{
				Agent.Main.Health = _playerHealth;
				CheckAndHandlePlayerInsideBattleArea();
			}
			break;
		}
	}

	private void CheckAndHandlePlayerInsideBattleArea()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		string[] source = default(string[]);
		if (!_activeTutorialArea.IsPositionInsideTutorialArea(Agent.Main.Position, ref source))
		{
			return;
		}
		if (string.IsNullOrEmpty(source.FirstOrDefault((string x) => x == "battle_area")))
		{
			if (!_playerLeftBattleArea)
			{
				_playerLeftBattleArea = true;
				OnPlayerLeftBattleArea();
			}
		}
		else if (_playerLeftBattleArea)
		{
			_playerLeftBattleArea = false;
			OnPlayerReEnteredBattleArea();
		}
	}

	private void OnPlayerLeftBattleArea()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		Vec2 asVec;
		switch (_trainingProgress)
		{
		case 4:
		{
			_advancedMeleeTrainerEasy.SetWatchState((WatchState)0);
			WorldPosition val2 = ModuleExtensions.ToWorldPosition(_advancedMeleeTrainerEasyInitialPosition.origin);
			Agent advancedMeleeTrainerEasy = _advancedMeleeTrainerEasy;
			asVec = ((Vec3)(ref _advancedMeleeTrainerEasySecondPosition.rotation.f)).AsVec2;
			advancedMeleeTrainerEasy.SetScriptedPositionAndDirection(ref val2, ((Vec2)(ref asVec)).RotationInRadians, true, (AIScriptedFrameFlags)0);
			break;
		}
		case 7:
		{
			_advancedMeleeTrainerNormal.SetWatchState((WatchState)0);
			WorldPosition val = ModuleExtensions.ToWorldPosition(_advancedMeleeTrainerNormalInitialPosition.origin);
			Agent advancedMeleeTrainerNormal = _advancedMeleeTrainerNormal;
			asVec = ((Vec3)(ref _advancedMeleeTrainerNormalInitialPosition.rotation.f)).AsVec2;
			advancedMeleeTrainerNormal.SetScriptedPositionAndDirection(ref val, ((Vec2)(ref asVec)).RotationInRadians, true, (AIScriptedFrameFlags)0);
			break;
		}
		}
	}

	private void OnPlayerReEnteredBattleArea()
	{
		switch (_trainingProgress)
		{
		case 4:
			_advancedMeleeTrainerEasy.DisableScriptedMovement();
			_advancedMeleeTrainerEasy.SetWatchState((WatchState)2);
			break;
		case 7:
			_advancedMeleeTrainerNormal.DisableScriptedMovement();
			_advancedMeleeTrainerNormal.SetWatchState((WatchState)2);
			break;
		}
	}

	private void OnEasyTrainerBeaten()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		_advancedMeleeTrainerEasy.SetWatchState((WatchState)0);
		WorldPosition val = ModuleExtensions.ToWorldPosition(_advancedMeleeTrainerEasySecondPosition.origin);
		Agent advancedMeleeTrainerEasy = _advancedMeleeTrainerEasy;
		Vec2 asVec = ((Vec3)(ref _advancedMeleeTrainerEasySecondPosition.rotation.f)).AsVec2;
		advancedMeleeTrainerEasy.SetScriptedPositionAndDirection(ref val, ((Vec2)(ref asVec)).RotationInRadians, true, (AIScriptedFrameFlags)0);
		_advancedMeleeTrainerNormal.SetWatchState((WatchState)0);
		WorldPosition val2 = ModuleExtensions.ToWorldPosition(_advancedMeleeTrainerNormalInitialPosition.origin);
		Agent advancedMeleeTrainerNormal = _advancedMeleeTrainerNormal;
		asVec = ((Vec3)(ref _advancedMeleeTrainerNormalInitialPosition.rotation.f)).AsVec2;
		advancedMeleeTrainerNormal.SetScriptedPositionAndDirection(ref val2, ((Vec2)(ref asVec)).RotationInRadians, true, (AIScriptedFrameFlags)0);
		Agent.Main.Health = Agent.Main.HealthLimit;
	}

	private void MakeTrainersPatrolling()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		WorldPosition val = ModuleExtensions.ToWorldPosition(_advancedMeleeTrainerEasyInitialPosition.origin);
		_advancedMeleeTrainerEasy.SetWatchState((WatchState)0);
		_advancedMeleeTrainerEasy.SetTeam(Mission.Current.PlayerAllyTeam, false);
		Agent advancedMeleeTrainerEasy = _advancedMeleeTrainerEasy;
		Vec2 asVec = ((Vec3)(ref _advancedMeleeTrainerEasyInitialPosition.rotation.f)).AsVec2;
		advancedMeleeTrainerEasy.SetScriptedPositionAndDirection(ref val, ((Vec2)(ref asVec)).RotationInRadians, true, (AIScriptedFrameFlags)0);
		SetAgentDefensiveness(_advancedMeleeTrainerNormal, 0f);
		WorldPosition val2 = ModuleExtensions.ToWorldPosition(_advancedMeleeTrainerNormalInitialPosition.origin);
		_advancedMeleeTrainerNormal.SetWatchState((WatchState)0);
		_advancedMeleeTrainerNormal.SetTeam(Mission.Current.PlayerAllyTeam, false);
		Agent advancedMeleeTrainerNormal = _advancedMeleeTrainerNormal;
		asVec = ((Vec3)(ref _advancedMeleeTrainerNormalInitialPosition.rotation.f)).AsVec2;
		advancedMeleeTrainerNormal.SetScriptedPositionAndDirection(ref val2, ((Vec2)(ref asVec)).RotationInRadians, true, (AIScriptedFrameFlags)0);
		SetAgentDefensiveness(_advancedMeleeTrainerNormal, 0f);
		_delayedActions.Add(new DelayedAction(delegate
		{
			Agent.Main.Health = Agent.Main.HealthLimit;
		}, 1.5f));
	}

	private void OnLost()
	{
		MakeTrainersPatrolling();
	}

	private void BeginNPCFight()
	{
		_advancedMeleeTrainerEasy.DisableScriptedMovement();
		_advancedMeleeTrainerEasy.SetWatchState((WatchState)2);
		_advancedMeleeTrainerEasy.SetTeam(Mission.Current.DefenderTeam, false);
		SetAgentDefensiveness(_advancedMeleeTrainerEasy, 4f);
		_advancedMeleeTrainerNormal.DisableScriptedMovement();
		_advancedMeleeTrainerNormal.SetWatchState((WatchState)2);
		_advancedMeleeTrainerNormal.SetTeam(Mission.Current.AttackerTeam, false);
		SetAgentDefensiveness(_advancedMeleeTrainerNormal, 4f);
	}

	private void OnAdvancedTrainingStart()
	{
		MakeTrainersPatrolling();
		Agent.Main.Health = Agent.Main.HealthLimit;
	}

	private void OnAdvancedTrainingExit()
	{
		Agent.Main.Health = Agent.Main.HealthLimit;
		BeginNPCFight();
	}

	private void OnAdvancedTrainingAreaEnter()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		MakeTrainersPatrolling();
		Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/fighting/greet"), _advancedMeleeTrainerNormal.GetEyeGlobalPosition(), true, false, -1, -1);
	}

	private static void SetAgentDefensiveness(Agent agent, float formationOrderDefensivenessFactor)
	{
		agent.Defensiveness = formationOrderDefensivenessFactor;
	}

	private void InitializeMeleeTraining()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = MatrixFrame.Identity;
		GameEntity val2 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawner_melee_npc");
		if (val2 != (GameEntity)null)
		{
			val = val2.GetGlobalFrame();
			((Mat3)(ref val.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		}
		else
		{
			Debug.FailedAssert("There are no spawn points for basic melee trainer.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Missions\\TrainingFieldMissionController.cs", "InitializeMeleeTraining", 1729);
		}
		CharacterObject val3 = Game.Current.ObjectManager.GetObject<CharacterObject>("tutorial_npc_basic_melee");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val3).Team(((MissionBehavior)this).Mission.PlayerTeam).InitialPosition(ref val.origin);
		Vec2 asVec = ((Vec3)(ref val.rotation.f)).AsVec2;
		AgentBuildData obj2 = obj.InitialDirection(ref asVec).CivilianEquipment(false).NoHorses(true)
			.NoWeapons(false)
			.ClothingColor1(((MissionBehavior)this).Mission.PlayerTeam.Color)
			.ClothingColor2(((MissionBehavior)this).Mission.PlayerTeam.Color2)
			.TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val3, -1, default(UniqueTroopDescriptor), false, false));
		EquipmentElement val4 = ((BasicCharacterObject)val3).Equipment[(EquipmentIndex)10];
		AgentBuildData val5 = obj2.MountKey(MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val4)).Item, ((BasicCharacterObject)val3).GetMountKeySeed())).Controller((AgentControllerType)0);
		Agent val6 = ((MissionBehavior)this).Mission.SpawnAgent(val5, false);
		val6.SetTeam(Mission.Current.DefenderTeam, false);
		_meleeTrainer = val6;
		_meleeTrainerDefaultPosition = _meleeTrainer.GetWorldPosition();
	}

	private void MeleeTrainingUpdate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = _meleeTrainer.Position - ((WorldPosition)(ref _meleeTrainerDefaultPosition)).GetGroundVec3();
		float lengthSquared = ((Vec3)(ref val)).LengthSquared;
		if (lengthSquared > 1f)
		{
			if ((int)_meleeTrainer.MovementFlags == 8192)
			{
				Agent meleeTrainer = _meleeTrainer;
				meleeTrainer.MovementFlags = (MovementControlFlag)(meleeTrainer.MovementFlags & -8193);
			}
			else if ((_meleeTrainer.MovementFlags & 0x3C0) > 0)
			{
				Agent meleeTrainer2 = _meleeTrainer;
				meleeTrainer2.MovementFlags = (MovementControlFlag)(meleeTrainer2.MovementFlags & -961);
				Agent meleeTrainer3 = _meleeTrainer;
				meleeTrainer3.MovementFlags = (MovementControlFlag)(meleeTrainer3.MovementFlags | 0x2000);
			}
			else
			{
				_meleeTrainer.SetTargetPosition(((WorldPosition)(ref _meleeTrainerDefaultPosition)).AsVec2);
			}
			TickMouseObjective(MouseObjectives.None);
		}
		else if (lengthSquared < 0.1f)
		{
			SwordTraining();
		}
	}

	private void SwordTraining()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Invalid comparison between Unknown and I4
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0561: Invalid comparison between Unknown and I4
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Expected O, but got Unknown
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_0581: Unknown result type (might be due to invalid IL or missing references)
		//IL_058b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Invalid comparison between Unknown and I4
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Invalid comparison between Unknown and I4
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Invalid comparison between Unknown and I4
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Invalid comparison between Unknown and I4
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Invalid comparison between Unknown and I4
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Invalid comparison between Unknown and I4
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Invalid comparison between Unknown and I4
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Invalid comparison between Unknown and I4
		if (_trainingProgress == 1)
		{
			if (HasAllWeaponsPicked())
			{
				_detailedObjectives = _meleeObjectives.ConvertAll((TutorialObjective x) => new TutorialObjective(x.Id, x.IsFinished, x.IsActive, x.HasBackground));
				_detailedObjectives[1].SetTextVariableOfName("HIT", 0);
				_detailedObjectives[1].SetTextVariableOfName("ALL", 4);
				_detailedObjectives[2].SetTextVariableOfName("HIT", 0);
				_detailedObjectives[2].SetTextVariableOfName("ALL", 4);
				_detailedObjectives[0].SetActive(isActive: true);
				_trainingProgress++;
				CurrentObjectiveTick(new TextObject("{=Zb1uFhsY}Go to trainer.", (Dictionary<string, object>)null));
			}
			TickMouseObjective(MouseObjectives.None);
			return;
		}
		Vec3 val = _meleeTrainer.Position - Agent.Main.Position;
		if (((Vec3)(ref val)).LengthSquared < 4f)
		{
			Agent meleeTrainer = _meleeTrainer;
			val = _meleeTrainer.Position;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			val = Agent.Main.GetEyeGlobalPosition() - _meleeTrainer.GetWorldFrame().Rotation.s * 0.1f - _meleeTrainer.GetEyeGlobalPosition();
			meleeTrainer.SetTargetPositionAndDirection(ref asVec, ref val);
			switch (_trainingProgress)
			{
			case 2:
				_detailedObjectives[0].FinishTask();
				_detailedObjectives[1].SetActive(isActive: true);
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/block_left"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
				CurrentObjectiveTick(new TextObject("{=Db98U6fF}Defend from left.", (Dictionary<string, object>)null));
				_trainingProgress++;
				break;
			case 3:
				if (((MissionBehavior)this).Mission.CurrentTime - _timer > 2f && (int)Agent.Main.GetCurrentActionDirection(1) == 6 && Agent.Main.GetCurrentActionProgress(1) > 0.1f && (int)Agent.Main.GetCurrentActionType(1) != 36)
				{
					_meleeTrainer.MovementFlags = (MovementControlFlag)0;
					_timer = ((MissionBehavior)this).Mission.CurrentTime;
				}
				else
				{
					_meleeTrainer.MovementFlags = (MovementControlFlag)128;
				}
				TickMouseObjective(MouseObjectives.DefendLeft);
				break;
			case 4:
				if (((MissionBehavior)this).Mission.CurrentTime - _timer > 1.5f && (int)Agent.Main.GetCurrentActionDirection(1) == 7 && Agent.Main.GetCurrentActionProgress(1) > 0.1f && (int)Agent.Main.GetCurrentActionType(1) != 36)
				{
					_meleeTrainer.MovementFlags = (MovementControlFlag)0;
					_timer = ((MissionBehavior)this).Mission.CurrentTime;
				}
				else
				{
					_meleeTrainer.MovementFlags = (MovementControlFlag)64;
				}
				TickMouseObjective(MouseObjectives.DefendRight);
				break;
			case 5:
				if (((MissionBehavior)this).Mission.CurrentTime - _timer > 1.5f && (int)Agent.Main.GetCurrentActionDirection(1) == 4 && Agent.Main.GetCurrentActionProgress(1) > 0.1f && (int)Agent.Main.GetCurrentActionType(1) != 36)
				{
					_meleeTrainer.MovementFlags = (MovementControlFlag)0;
					_timer = ((MissionBehavior)this).Mission.CurrentTime;
				}
				else
				{
					_meleeTrainer.MovementFlags = (MovementControlFlag)256;
				}
				TickMouseObjective(MouseObjectives.DefendUp);
				break;
			case 6:
				if (((MissionBehavior)this).Mission.CurrentTime - _timer > 1.5f && (int)Agent.Main.GetCurrentActionDirection(1) == 5 && Agent.Main.GetCurrentActionProgress(1) > 0.1f && (int)Agent.Main.GetCurrentActionType(1) != 36)
				{
					_meleeTrainer.MovementFlags = (MovementControlFlag)0;
					_timer = ((MissionBehavior)this).Mission.CurrentTime;
				}
				else
				{
					_meleeTrainer.MovementFlags = (MovementControlFlag)512;
				}
				TickMouseObjective(MouseObjectives.DefendDown);
				break;
			case 7:
			{
				Agent meleeTrainer3 = _meleeTrainer;
				meleeTrainer3.MovementFlags = (MovementControlFlag)(meleeTrainer3.MovementFlags | 0x800);
				TickMouseObjective(MouseObjectives.AttackLeft);
				break;
			}
			case 8:
				if (((MissionBehavior)this).Mission.CurrentTime - _timer > 1f)
				{
					Agent meleeTrainer5 = _meleeTrainer;
					meleeTrainer5.MovementFlags = (MovementControlFlag)(meleeTrainer5.MovementFlags | 0x400);
				}
				TickMouseObjective(MouseObjectives.AttackRight);
				break;
			case 9:
				if (((MissionBehavior)this).Mission.CurrentTime - _timer > 1f)
				{
					Agent meleeTrainer4 = _meleeTrainer;
					meleeTrainer4.MovementFlags = (MovementControlFlag)(meleeTrainer4.MovementFlags | 0x1000);
				}
				TickMouseObjective(MouseObjectives.AttackUp);
				break;
			case 10:
				if (((MissionBehavior)this).Mission.CurrentTime - _timer > 1f)
				{
					Agent meleeTrainer2 = _meleeTrainer;
					meleeTrainer2.MovementFlags = (MovementControlFlag)(meleeTrainer2.MovementFlags | 0x2000);
				}
				TickMouseObjective(MouseObjectives.AttackDown);
				break;
			case 11:
				_meleeTrainer.MovementFlags = (MovementControlFlag)0;
				_trainingProgress++;
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/praise"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
				SuccessfullyFinishTraining(0f);
				break;
			}
		}
		else
		{
			TickMouseObjective(MouseObjectives.None);
			if ((int)_meleeTrainer.MovementFlags == 8192)
			{
				Agent meleeTrainer6 = _meleeTrainer;
				meleeTrainer6.MovementFlags = (MovementControlFlag)(meleeTrainer6.MovementFlags & -8193);
			}
			else if ((_meleeTrainer.MovementFlags & 0x3C0) > 0)
			{
				Agent meleeTrainer7 = _meleeTrainer;
				meleeTrainer7.MovementFlags = (MovementControlFlag)(meleeTrainer7.MovementFlags & -961);
				Agent meleeTrainer8 = _meleeTrainer;
				meleeTrainer8.MovementFlags = (MovementControlFlag)(meleeTrainer8.MovementFlags | 0x2000);
			}
		}
	}

	public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_056d: Invalid comparison between Unknown and I4
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Invalid comparison between Unknown and I4
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Invalid comparison between Unknown and I4
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Invalid comparison between Unknown and I4
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Invalid comparison between Unknown and I4
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Invalid comparison between Unknown and I4
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Invalid comparison between Unknown and I4
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Expected O, but got Unknown
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Invalid comparison between Unknown and I4
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Expected O, but got Unknown
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Expected O, but got Unknown
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Expected O, but got Unknown
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Expected O, but got Unknown
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Invalid comparison between Unknown and I4
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Expected O, but got Unknown
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Expected O, but got Unknown
		((MissionBehavior)this).OnScoreHit(affectedAgent, affectorAgent, attackerWeapon, isBlocked, isSiegeEngineHit, ref blow, ref collisionData, damagedHp, hitDistance, shotDifficulty);
		if (isBlocked)
		{
			for (EquipmentIndex val = (EquipmentIndex)0; (int)val <= 3; val = (EquipmentIndex)(val + 1))
			{
				MissionWeapon val2 = affectedAgent.Equipment[val];
				if (!((MissionWeapon)(ref val2)).IsEmpty)
				{
					val2 = affectedAgent.Equipment[val];
					if (((MissionWeapon)(ref val2)).IsShield())
					{
						EquipmentIndex val3 = val;
						val2 = affectedAgent.Equipment[val];
						affectedAgent.ChangeWeaponHitPoints(val3, ((MissionWeapon)(ref val2)).ModifiedMaxHitPoints);
					}
				}
			}
		}
		TutorialArea activeTutorialArea = _activeTutorialArea;
		if (activeTutorialArea != null && (int)activeTutorialArea.TypeOfTraining == 1)
		{
			if ((int)affectedAgent.Controller == 2)
			{
				if (_trainingProgress >= 3 && _trainingProgress <= 6 && isBlocked)
				{
					_timer = ((MissionBehavior)this).Mission.CurrentTime;
					if (_trainingProgress == 3 && (int)affectedAgent.GetCurrentActionDirection(1) == 6)
					{
						_detailedObjectives[1].SetTextVariableOfName("HIT", 1);
						Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/block_right"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
						CurrentObjectiveTick(new TextObject("{=7wmkPNbI}Defend from right.", (Dictionary<string, object>)null));
						_trainingProgress++;
					}
					else if (_trainingProgress == 4 && (int)affectedAgent.GetCurrentActionDirection(1) == 7)
					{
						_detailedObjectives[1].SetTextVariableOfName("HIT", 2);
						Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/block_up"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
						CurrentObjectiveTick(new TextObject("{=CEqKkY3m}Defend from up.", (Dictionary<string, object>)null));
						_trainingProgress++;
					}
					else if (_trainingProgress == 5 && (int)affectedAgent.GetCurrentActionDirection(1) == 4)
					{
						_detailedObjectives[1].SetTextVariableOfName("HIT", 3);
						Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/block_down"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
						CurrentObjectiveTick(new TextObject("{=Qdz5Hely}Defend from down.", (Dictionary<string, object>)null));
						_trainingProgress++;
					}
					else if (_trainingProgress == 6 && (int)affectedAgent.GetCurrentActionDirection(1) == 5)
					{
						_detailedObjectives[1].SetTextVariableOfName("HIT", 4);
						_detailedObjectives[1].FinishTask();
						_detailedObjectives[2].SetActive(isActive: true);
						Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/attack_left"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
						CurrentObjectiveTick(new TextObject("{=8QX1QHAJ}Attack from left.", (Dictionary<string, object>)null));
						_trainingProgress++;
					}
				}
			}
			else if (affectedAgent == _meleeTrainer && affectorAgent != null && (int)affectorAgent.Controller == 2 && _trainingProgress >= 7 && _trainingProgress <= 10 && isBlocked)
			{
				_meleeTrainer.MovementFlags = (MovementControlFlag)0;
				_timer = ((MissionBehavior)this).Mission.CurrentTime;
				if (_trainingProgress == 7 && (int)affectorAgent.GetCurrentActionDirection(1) == 2)
				{
					_detailedObjectives[2].SetTextVariableOfName("HIT", 1);
					Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/attack_right"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
					CurrentObjectiveTick(new TextObject("{=fC60rYwy}Attack from right.", (Dictionary<string, object>)null));
					_trainingProgress++;
				}
				else if (_trainingProgress == 8 && (int)affectorAgent.GetCurrentActionDirection(1) == 3)
				{
					_detailedObjectives[2].SetTextVariableOfName("HIT", 2);
					Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/attack_up"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
					CurrentObjectiveTick(new TextObject("{=j2dW9fZt}Attack from up.", (Dictionary<string, object>)null));
					_trainingProgress++;
				}
				else if (_trainingProgress == 9 && (int)affectorAgent.GetCurrentActionDirection(1) == 0)
				{
					_detailedObjectives[2].SetTextVariableOfName("HIT", 3);
					Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/attack_down"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
					CurrentObjectiveTick(new TextObject("{=X9Vmjipn}Attack from down.", (Dictionary<string, object>)null));
					_trainingProgress++;
				}
				else if (_trainingProgress == 10 && (int)affectorAgent.GetCurrentActionDirection(1) == 1)
				{
					_detailedObjectives[2].SetTextVariableOfName("HIT", 4);
					_detailedObjectives[2].FinishTask();
					CurrentObjectiveTick(_trainingFinishedText);
					TickMouseObjective(MouseObjectives.None);
					MBInformationManager.AddQuickInformation(Agent.Main.Equipment.HasShield() ? new TextObject("{=PiOiQ3u5}You've successfully finished the sword and shield tutorial.", (Dictionary<string, object>)null) : new TextObject("{=GZaYmg95}You've successfully finished the sword tutorial.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					_trainingProgress++;
				}
				else
				{
					MBInformationManager.AddQuickInformation(new TextObject("{=fBJRdxh2}Try again.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/parrying/remark"), _meleeTrainer.GetEyeGlobalPosition(), true, false, -1, -1);
				}
			}
		}
		if (!isBlocked)
		{
			if ((int)affectedAgent.Controller == 2)
			{
				_playerHealth -= blow.InflictedDamage;
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/fighting/warning"), _advancedMeleeTrainerNormal.GetEyeGlobalPosition(), true, false, -1, -1);
			}
			else if (affectedAgent == _advancedMeleeTrainerEasy)
			{
				_advancedMeleeTrainerEasyHealth -= blow.InflictedDamage;
			}
			else if (affectedAgent == _advancedMeleeTrainerNormal)
			{
				_advancedMeleeTrainerNormalHealth -= blow.InflictedDamage;
			}
		}
	}

	private void TickMouseObjective(MouseObjectives objective)
	{
		CurrentMouseObjectiveTick?.Invoke(GetAdjustedMouseObjective(objective), GetObjectivePerformingType(objective));
	}

	private static bool IsAttackDirection(MouseObjectives objective)
	{
		if ((uint)(objective - 1) <= 3u)
		{
			return true;
		}
		return false;
	}

	private static MouseObjectives GetAdjustedMouseObjective(MouseObjectives baseObjective)
	{
		if (IsAttackDirection(baseObjective))
		{
			if (BannerlordConfig.AttackDirectionControl != 0)
			{
				return baseObjective;
			}
			return GetInverseDirection(baseObjective);
		}
		return baseObjective;
	}

	private static ObjectivePerformingType GetObjectivePerformingType(MouseObjectives baseObjective)
	{
		if (IsAttackDirection(baseObjective))
		{
			return BannerlordConfig.AttackDirectionControl switch
			{
				0 => ObjectivePerformingType.ByLookDirection, 
				1 => ObjectivePerformingType.ByLookDirection, 
				_ => ObjectivePerformingType.ByMovement, 
			};
		}
		return BannerlordConfig.DefendDirectionControl switch
		{
			0 => ObjectivePerformingType.ByLookDirection, 
			1 => ObjectivePerformingType.ByMovement, 
			_ => ObjectivePerformingType.AutoBlock, 
		};
	}

	private static MouseObjectives GetInverseDirection(MouseObjectives objective)
	{
		switch (objective)
		{
		case MouseObjectives.None:
			return MouseObjectives.None;
		case MouseObjectives.AttackLeft:
			return MouseObjectives.AttackRight;
		case MouseObjectives.AttackRight:
			return MouseObjectives.AttackLeft;
		case MouseObjectives.AttackUp:
			return MouseObjectives.AttackDown;
		case MouseObjectives.AttackDown:
			return MouseObjectives.AttackUp;
		case MouseObjectives.DefendLeft:
			return MouseObjectives.DefendRight;
		case MouseObjectives.DefendRight:
			return MouseObjectives.DefendLeft;
		case MouseObjectives.DefendUp:
			return MouseObjectives.DefendDown;
		case MouseObjectives.DefendDown:
			return MouseObjectives.DefendUp;
		default:
			Debug.FailedAssert($"Inverse direction is not defined for: {objective}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Missions\\TrainingFieldMissionController.cs", "GetInverseDirection", 2208);
			return MouseObjectives.None;
		}
	}

	private void InitializeMountedTraining()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		_horse = SpawnHorse();
		_horse.Controller = (AgentControllerType)0;
		_horseBeginningPosition = _horse.GetWorldPosition();
		_finishGateClosed = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("finish_gate_closed");
		_finishGateOpen = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("finish_gate_open");
		_mountedAIWaitingPosition = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("_mounted_ai_waiting_position").GetGlobalFrame();
		_mountedAI = SpawnMountedAI();
		_mountedAI.SetWatchState((WatchState)2);
	}

	private Agent SpawnMountedAI()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		_mountedAISpawnPosition = MatrixFrame.Identity;
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("_mounted_ai_spawn_position");
		if (val != (GameEntity)null)
		{
			_mountedAISpawnPosition = val.GetGlobalFrame();
			((Mat3)(ref _mountedAISpawnPosition.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		}
		else
		{
			Debug.FailedAssert("There are no spawn points for mounted ai.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\StoryMode\\Missions\\TrainingFieldMissionController.cs", "SpawnMountedAI", 2253);
		}
		CharacterObject val2 = Game.Current.ObjectManager.GetObject<CharacterObject>("tutorial_npc_mounted_ai");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val2).Team(((MissionBehavior)this).Mission.PlayerTeam).InitialPosition(ref _mountedAISpawnPosition.origin);
		Vec2 asVec = ((Vec3)(ref _mountedAISpawnPosition.rotation.f)).AsVec2;
		AgentBuildData obj2 = obj.InitialDirection(ref asVec).CivilianEquipment(false).NoHorses(false)
			.NoWeapons(false)
			.ClothingColor1(((MissionBehavior)this).Mission.PlayerTeam.Color)
			.ClothingColor2(((MissionBehavior)this).Mission.PlayerTeam.Color2)
			.TroopOrigin((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, val2, -1, default(UniqueTroopDescriptor), false, false));
		EquipmentElement val3 = ((BasicCharacterObject)val2).Equipment[(EquipmentIndex)10];
		AgentBuildData val4 = obj2.MountKey(MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val3)).Item, ((BasicCharacterObject)val2).GetMountKeySeed())).Controller((AgentControllerType)1);
		Agent obj3 = ((MissionBehavior)this).Mission.SpawnAgent(val4, false);
		obj3.SetTeam(Mission.Current.PlayerTeam, false);
		return obj3;
	}

	private void UpdateMountedAIBehavior()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		Vec2 val3;
		if (_mountedAICurrentCheckpointTarget == -1)
		{
			if (_continueLoop)
			{
				Vec3 val = _mountedAISpawnPosition.origin - _mountedAI.Position;
				if (((Vec3)(ref val)).LengthSquared < 6.25f)
				{
					_mountedAICurrentCheckpointTarget++;
					gameEntity = ((ScriptComponentBehavior)_checkpoints[_mountedAICurrentCheckpointTarget].Item1).GameEntity;
					MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
					WorldPosition val2 = ModuleExtensions.ToWorldPosition(globalFrame.origin);
					Agent mountedAI = _mountedAI;
					val3 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
					mountedAI.SetScriptedPositionAndDirection(ref val2, ((Vec2)(ref val3)).RotationInRadians, true, (AIScriptedFrameFlags)0);
					SetFinishGateStatus(open: false);
					_mountedAI.SetWatchState((WatchState)2);
				}
			}
			return;
		}
		bool flag = false;
		gameEntity = ((ScriptComponentBehavior)_checkpoints[_mountedAICurrentCheckpointTarget].Item1).GameEntity;
		WorldPosition val4 = ModuleExtensions.ToWorldPosition(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin);
		Vec2 asVec = ((WorldPosition)(ref val4)).AsVec2;
		val4 = ModuleExtensions.ToWorldPosition(_mountedAI.Position);
		val3 = asVec - ((WorldPosition)(ref val4)).AsVec2;
		if (((Vec2)(ref val3)).LengthSquared < 25f)
		{
			flag = true;
			_mountedAICurrentCheckpointTarget++;
			if (_mountedAICurrentCheckpointTarget > _checkpoints.Count - 1)
			{
				_mountedAICurrentCheckpointTarget = -1;
				if (_continueLoop)
				{
					GoToStartingPosition();
				}
				else
				{
					WorldPosition val5 = ModuleExtensions.ToWorldPosition(_mountedAIWaitingPosition.origin);
					Agent mountedAI2 = _mountedAI;
					val3 = ((Vec3)(ref _mountedAISpawnPosition.rotation.f)).AsVec2;
					mountedAI2.SetScriptedPositionAndDirection(ref val5, ((Vec2)(ref val3)).RotationInRadians, true, (AIScriptedFrameFlags)0);
				}
			}
			else if (_mountedAICurrentCheckpointTarget == _checkpoints.Count - 1)
			{
				SetFinishGateStatus(open: true);
				_mountedAI.SetWatchState((WatchState)0);
			}
		}
		else
		{
			gameEntity = ((ScriptComponentBehavior)_mountedAITargets[_mountedAICurrentHitTarget]).GameEntity;
			val4 = ModuleExtensions.ToWorldPosition(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin);
			Vec2 asVec2 = ((WorldPosition)(ref val4)).AsVec2;
			val4 = ModuleExtensions.ToWorldPosition(_mountedAI.Position);
			val3 = asVec2 - ((WorldPosition)(ref val4)).AsVec2;
			if (((Vec2)(ref val3)).LengthSquared < 169f)
			{
				_enteredRadiusOfTarget = true;
			}
			else
			{
				if (!_allTargetsDestroyed && _mountedAITargets[_mountedAICurrentHitTarget].IsDestroyed)
				{
					goto IL_02d2;
				}
				if (_enteredRadiusOfTarget)
				{
					gameEntity = ((ScriptComponentBehavior)_mountedAITargets[_mountedAICurrentHitTarget]).GameEntity;
					val4 = ModuleExtensions.ToWorldPosition(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin);
					Vec2 asVec3 = ((WorldPosition)(ref val4)).AsVec2;
					val4 = ModuleExtensions.ToWorldPosition(_mountedAI.Position);
					val3 = asVec3 - ((WorldPosition)(ref val4)).AsVec2;
					if (((Vec2)(ref val3)).LengthSquared > 169f)
					{
						goto IL_02d2;
					}
				}
			}
		}
		goto IL_030d;
		IL_02d2:
		_enteredRadiusOfTarget = false;
		flag = true;
		_mountedAICurrentHitTarget++;
		if (_mountedAICurrentHitTarget > _mountedAITargets.Count - 1)
		{
			_mountedAICurrentHitTarget = 0;
			_allTargetsDestroyed = true;
		}
		goto IL_030d;
		IL_030d:
		if (flag && _mountedAICurrentCheckpointTarget != -1)
		{
			gameEntity = ((ScriptComponentBehavior)_checkpoints[_mountedAICurrentCheckpointTarget].Item1).GameEntity;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			WorldPosition val6 = ModuleExtensions.ToWorldPosition(globalFrame2.origin);
			Agent mountedAI3 = _mountedAI;
			val3 = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
			mountedAI3.SetScriptedPositionAndDirection(ref val6, ((Vec2)(ref val3)).RotationInRadians, true, (AIScriptedFrameFlags)0);
			if (!_allTargetsDestroyed)
			{
				_mountedAI.SetScriptedTargetEntity(((ScriptComponentBehavior)_mountedAITargets[_mountedAICurrentHitTarget]).GameEntity, (AISpecialCombatModeFlags)0, false);
			}
		}
	}

	private void GoToStartingPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		WorldPosition val = ModuleExtensions.ToWorldPosition(_mountedAISpawnPosition.origin);
		Agent mountedAI = _mountedAI;
		Vec2 asVec = ((Vec3)(ref _mountedAISpawnPosition.rotation.f)).AsVec2;
		mountedAI.SetScriptedPositionAndDirection(ref val, ((Vec2)(ref asVec)).RotationInRadians, true, (AIScriptedFrameFlags)0);
		RestoreAndShowAllMountedAITargets();
	}

	private void RestoreAndShowAllMountedAITargets()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_allTargetsDestroyed = false;
		foreach (DestructableComponent mountedAITarget in _mountedAITargets)
		{
			mountedAITarget.Reset();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)mountedAITarget).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(true);
		}
	}

	private void HideAllMountedAITargets()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_allTargetsDestroyed = true;
		foreach (DestructableComponent mountedAITarget in _mountedAITargets)
		{
			mountedAITarget.Reset();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)mountedAITarget).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
		}
	}

	private void UpdateHorseBehavior()
	{
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Invalid comparison between Unknown and I4
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Invalid comparison between Unknown and I4
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Invalid comparison between Unknown and I4
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Invalid comparison between Unknown and I4
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		if (_horse != null && _horse.RiderAgent == null)
		{
			if (_horse.IsAIControlled && _horse.CommonAIComponent.IsPanicked)
			{
				_horse.CommonAIComponent.StopRetreating();
			}
			string[] array = default(string[]);
			if (_horseBehaviorMode != HorseReturningSituation.BeginReturn && !_trainingAreas.Find((TutorialArea x) => (int)x.TypeOfTraining == 2).IsPositionInsideTutorialArea(_horse.Position, ref array))
			{
				_horseBehaviorMode = HorseReturningSituation.BeginReturn;
				TutorialArea activeTutorialArea = _activeTutorialArea;
				if (activeTutorialArea != null && (int)activeTutorialArea.TypeOfTraining == 2 && _trainingProgress > 1)
				{
					ResetTrainingArea();
				}
			}
			else
			{
				TutorialArea activeTutorialArea2 = _activeTutorialArea;
				if ((activeTutorialArea2 == null || (int)activeTutorialArea2.TypeOfTraining != 2) && (_horseBehaviorMode == HorseReturningSituation.NotInPosition || _horseBehaviorMode == HorseReturningSituation.Following))
				{
					_horseBehaviorMode = HorseReturningSituation.BeginReturn;
				}
				else
				{
					TutorialArea activeTutorialArea3 = _activeTutorialArea;
					if (activeTutorialArea3 != null && (int)activeTutorialArea3.TypeOfTraining == 2 && !Agent.Main.HasMount && _trainingProgress > 2)
					{
						_horseBehaviorMode = HorseReturningSituation.Following;
					}
				}
			}
			Vec3 val;
			switch (_horseBehaviorMode)
			{
			case HorseReturningSituation.BeginReturn:
				val = _horse.Position - ((WorldPosition)(ref _horseBeginningPosition)).GetGroundVec3();
				if (((Vec3)(ref val)).Length > 1f)
				{
					_horse.Controller = (AgentControllerType)1;
					_horse.SetScriptedPosition(ref _horseBeginningPosition, false, (AIScriptedFrameFlags)0);
					_horseBehaviorMode = HorseReturningSituation.Returning;
				}
				else
				{
					_horseBehaviorMode = HorseReturningSituation.ReturnCompleted;
				}
				break;
			case HorseReturningSituation.Returning:
				val = _horse.Position - ((WorldPosition)(ref _horseBeginningPosition)).GetGroundVec3();
				if (((Vec3)(ref val)).Length < 0.5f)
				{
					Vec2 currentVelocity = _horse.GetCurrentVelocity();
					if (((Vec2)(ref currentVelocity)).LengthSquared <= 0f)
					{
						_horseBehaviorMode = HorseReturningSituation.ReturnCompleted;
					}
					else if ((int)_horse.Controller == 1)
					{
						_horse.Controller = (AgentControllerType)0;
						Agent horse = _horse;
						horse.MovementFlags = (MovementControlFlag)(horse.MovementFlags & -64);
						_horse.MovementInputVector = Vec2.Zero;
					}
				}
				else if ((int)_horse.Controller == 0)
				{
					_horseBehaviorMode = HorseReturningSituation.BeginReturn;
				}
				break;
			case HorseReturningSituation.ReturnCompleted:
				val = _horse.Position - ((WorldPosition)(ref _horseBeginningPosition)).GetGroundVec3();
				if (((Vec3)(ref val)).Length > 1f)
				{
					TutorialArea activeTutorialArea4 = _activeTutorialArea;
					if (activeTutorialArea4 != null && (int)activeTutorialArea4.TypeOfTraining == 2)
					{
						_horseBehaviorMode = HorseReturningSituation.NotInPosition;
						_horse.Controller = (AgentControllerType)0;
						Agent horse2 = _horse;
						horse2.MovementFlags = (MovementControlFlag)(horse2.MovementFlags & -64);
						_horse.MovementInputVector = Vec2.Zero;
					}
				}
				break;
			case HorseReturningSituation.Following:
				val = _horse.Position - Agent.Main.Position;
				if (((Vec3)(ref val)).Length > 3f)
				{
					_horse.Controller = (AgentControllerType)1;
					Vec3 position = Agent.Main.Position;
					val = _horse.Position - Agent.Main.Position;
					Vec3 val2 = position + ((Vec3)(ref val)).NormalizedCopy() * 3f;
					WorldPosition val3 = default(WorldPosition);
					((WorldPosition)(ref val3))._002Ector(Agent.Main.Mission.Scene, val2);
					_horse.SetScriptedPosition(ref val3, false, (AIScriptedFrameFlags)0);
				}
				break;
			}
		}
		else if (_horse.RiderAgent != null && _horseBehaviorMode != HorseReturningSituation.NotInPosition)
		{
			_horseBehaviorMode = HorseReturningSituation.NotInPosition;
			_horse.Controller = (AgentControllerType)0;
			Agent horse3 = _horse;
			horse3.MovementFlags = (MovementControlFlag)(horse3.MovementFlags & -64);
			_horse.MovementInputVector = Vec2.Zero;
		}
	}

	private Agent SpawnHorse()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawner_horse").GetGlobalFrame();
		ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>("old_horse");
		ItemRosterElement val2 = default(ItemRosterElement);
		((ItemRosterElement)(ref val2))._002Ector(val, 1, (ItemModifier)null);
		ItemObject val3 = MBObjectManager.Instance.GetObject<ItemObject>("light_harness");
		ItemRosterElement val4 = default(ItemRosterElement);
		((ItemRosterElement)(ref val4))._002Ector(val3, 0, (ItemModifier)null);
		Agent val5 = null;
		if (val.HasHorseComponent)
		{
			Mission current = Mission.Current;
			ItemRosterElement val6 = val2;
			ItemRosterElement val7 = val4;
			ref Vec3 origin = ref globalFrame.origin;
			Vec2 val8 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			val8 = ((Vec2)(ref val8)).Normalized();
			val5 = current.SpawnMonster(val6, val7, ref origin, ref val8, -1);
			AnimalSpawnSettings.CheckAndSetAnimalAgentFlags(((MissionBehavior)this).Mission.Scene.FindEntityWithTag("spawner_melee_npc"), val5);
		}
		return val5;
	}

	private void MountedTrainingUpdate()
	{
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Expected O, but got Unknown
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		bool flag = false;
		if (_trainingProgress > 2 && _trainingProgress < 5)
		{
			flag = CheckpointUpdate();
		}
		if (Agent.Main.HasMount)
		{
			_activeTutorialArea.ActivateBoundaries();
		}
		else
		{
			_activeTutorialArea.HideBoundaries();
		}
		switch (_trainingProgress)
		{
		case 1:
			if (HasAllWeaponsPicked())
			{
				_detailedObjectives = _mountedObjectives.ConvertAll((TutorialObjective x) => new TutorialObjective(x.Id, x.IsFinished, x.IsActive, x.HasBackground));
				_detailedObjectives[1].SetTextVariableOfName("HIT", _activeTutorialArea.GetBrokenBreakableCount(_trainingSubTypeIndex));
				_detailedObjectives[1].SetTextVariableOfName("ALL", _activeTutorialArea.GetBreakablesCount(_trainingSubTypeIndex));
				_detailedObjectives[0].SetActive(isActive: true);
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/riding/pick_" + _trainingSubTypeIndex), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
				SetHorseMountable(mountable: true);
				_mountedLastBrokenTargetCount = 0;
				_trainingProgress++;
				CurrentObjectiveTick(new TextObject("{=h31YaM4b}Mount the horse.", (Dictionary<string, object>)null));
			}
			break;
		case 2:
			if (Agent.Main.HasMount)
			{
				_detailedObjectives[0].FinishTask();
				_detailedObjectives[1].SetActive(isActive: true);
				_activeTutorialArea.ActivateBoundaries();
				_trainingProgress++;
				CurrentObjectiveTick(new TextObject("{=gJBNUAJd}Finish the track and hit as many targets as you can.", (Dictionary<string, object>)null));
			}
			break;
		case 3:
			if (_checkpoints[0].Item2)
			{
				_activeTutorialArea.MakeDestructible(_trainingSubTypeIndex);
				_activeTutorialArea.ResetBreakables(_trainingSubTypeIndex, false);
				ResetCheckpoints();
				(VolumeBox, bool) value = _checkpoints[0];
				value.Item2 = true;
				_checkpoints[0] = value;
				StartTimer();
				UIStartTimer();
				MBInformationManager.AddQuickInformation(new TextObject("{=HvGW2DvS}Track started.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/riding/start_course"), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
				_trainingProgress++;
			}
			else if (!Agent.Main.HasMount)
			{
				_trainingProgress = 1;
			}
			break;
		case 4:
		{
			int brokenBreakableCount = _activeTutorialArea.GetBrokenBreakableCount(_trainingSubTypeIndex);
			_detailedObjectives[1].SetTextVariableOfName("HIT", brokenBreakableCount);
			if (brokenBreakableCount != _mountedLastBrokenTargetCount)
			{
				Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/hit_target"), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
				_mountedLastBrokenTargetCount = brokenBreakableCount;
			}
			if (flag)
			{
				_detailedObjectives[1].FinishTask();
				_trainingProgress++;
				MountedTrainingEndedSuccessfully();
			}
			break;
		}
		case 5:
			if (!Agent.Main.HasMount)
			{
				_trainingProgress++;
				SetHorseMountable(mountable: false);
				CurrentObjectiveTick(_trainingFinishedText);
			}
			break;
		}
	}

	private void ResetCheckpoints()
	{
		for (int i = 0; i < _checkpoints.Count; i++)
		{
			_checkpoints[i] = ValueTuple.Create<VolumeBox, bool>(_checkpoints[i].Item1, item2: false);
		}
		_currentCheckpointIndex = -1;
	}

	private bool CheckpointUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		WeakGameEntity gameEntity;
		for (int i = 0; i < _checkpoints.Count; i++)
		{
			if (_checkpoints[i].Item1.IsPointIn(Agent.Main.Position))
			{
				if (_currentCheckpointIndex == -1)
				{
					Vec3 velocity = Agent.Main.Velocity;
					gameEntity = ((ScriptComponentBehavior)_checkpoints[i].Item1).GameEntity;
					_enteringDotProduct = Vec3.DotProduct(velocity, ((WeakGameEntity)(ref gameEntity)).GetFrame().rotation.f);
					_currentCheckpointIndex = i;
				}
				return false;
			}
		}
		bool result = false;
		if (_currentCheckpointIndex != -1)
		{
			gameEntity = ((ScriptComponentBehavior)_checkpoints[_currentCheckpointIndex].Item1).GameEntity;
			float num = Vec3.DotProduct(((WeakGameEntity)(ref gameEntity)).GetFrame().rotation.f, Agent.Main.Velocity);
			if (num > 0f == _enteringDotProduct > 0f)
			{
				if ((_currentCheckpointIndex == 0 || _checkpoints[_currentCheckpointIndex - 1].Item2) && num > 0f)
				{
					_checkpoints[_currentCheckpointIndex] = ValueTuple.Create<VolumeBox, bool>(_checkpoints[_currentCheckpointIndex].Item1, item2: true);
					int num2 = 0;
					for (int j = 0; j < _checkpoints.Count; j++)
					{
						if (_checkpoints[j].Item2)
						{
							num2++;
						}
					}
					if (_currentCheckpointIndex == _checkpoints.Count - 1)
					{
						result = true;
					}
					if (_currentCheckpointIndex == _checkpoints.Count - 2)
					{
						SetFinishGateStatus(open: true);
					}
				}
				else if (num < 0f)
				{
					MBInformationManager.AddQuickInformation(new TextObject("{=kvTEeUWO}Wrong way!", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
				}
			}
		}
		_currentCheckpointIndex = -1;
		return result;
	}

	private static void SetHorseMountable(bool mountable)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (mountable)
		{
			Agent.Main.SetAgentFlags((AgentFlag)(Agent.Main.GetAgentFlags() | 0x2000));
		}
		else
		{
			Agent.Main.SetAgentFlags((AgentFlag)(Agent.Main.GetAgentFlags() & -8193));
		}
	}

	private void OnMountedTrainingStart()
	{
		ResetCheckpoints();
		_continueLoop = false;
		HideAllMountedAITargets();
	}

	private void OnMountedTrainingExit()
	{
		SetHorseMountable(mountable: false);
		ResetCheckpoints();
		_continueLoop = true;
		GoToStartingPosition();
	}

	private void SetFinishGateStatus(bool open)
	{
		if (open)
		{
			_finishGateStatus++;
			if (_finishGateStatus == 1)
			{
				_finishGateClosed.SetVisibilityExcludeParents(false);
				_finishGateOpen.SetVisibilityExcludeParents(true);
			}
		}
		else
		{
			_finishGateStatus = MathF.Max(0, _finishGateStatus - 1);
			if (_finishGateStatus == 0)
			{
				_finishGateClosed.SetVisibilityExcludeParents(true);
				_finishGateOpen.SetVisibilityExcludeParents(false);
			}
		}
	}

	private void MountedTrainingEndedSuccessfully()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_011b: Expected O, but got Unknown
		UIEndTimer();
		EndTimer();
		int brokenBreakableCount = _activeTutorialArea.GetBrokenBreakableCount(_trainingSubTypeIndex);
		int breakablesCount = _activeTutorialArea.GetBreakablesCount(_trainingSubTypeIndex);
		float num = _timeScore + (float)(_activeTutorialArea.GetBreakablesCount(_trainingSubTypeIndex) - _activeTutorialArea.GetBrokenBreakableCount(_trainingSubTypeIndex));
		TextObject val = new TextObject("{=W49eUmpT}You can dismount from horse with {CROUCH_KEY}, or {ACTION_KEY} while looking at the horse.", (Dictionary<string, object>)null);
		val.SetTextVariable("CROUCH_KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 15), 1f));
		val.SetTextVariable("ACTION_KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		CurrentObjectiveTick(val);
		if (breakablesCount - brokenBreakableCount == 0)
		{
			Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/riding/course_perfect"), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
			TextObject val2 = new TextObject("{=veHe94Ec}You've successfully finished the track in ({TIME_SCORE}) seconds without missing any targets!", (Dictionary<string, object>)null);
			val2.SetTextVariable("TIME_SCORE", new TextObject(num.ToString("0.0"), (Dictionary<string, object>)null));
			MBInformationManager.AddQuickInformation(val2, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		else
		{
			Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/tutorial/vo/riding/course_finish"), Agent.Main.GetEyeGlobalPosition(), true, false, -1, -1);
			TextObject val3 = new TextObject("{=QLgkR3qN}You've successfully finished the track in ({TIME_SCORE}) seconds. You've received ({PENALTY_SECONDS}) seconds penalty from ({MISSED_TARGETS}) missed targets.", (Dictionary<string, object>)null);
			val3.SetTextVariable("TIME_SCORE", new TextObject(num.ToString("0.0"), (Dictionary<string, object>)null));
			val3.SetTextVariable("PENALTY_SECONDS", new TextObject((num - _timeScore).ToString("0.0"), (Dictionary<string, object>)null));
			val3.SetTextVariable("MISSED_TARGETS", breakablesCount - brokenBreakableCount);
			MBInformationManager.AddQuickInformation(val3, 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
		SetFinishGateStatus(open: false);
		SuccessfullyFinishTraining(num);
	}
}
