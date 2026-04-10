using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.AI.UsableMachineAIs;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using NavalDLC.Missions.ShipActuators;
using NavalDLC.Missions.ShipControl;
using NavalDLC.Missions.ShipInput;
using NavalDLC.Storyline.Objectives.Quest4;
using NavalDLC.Storyline.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
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

public class FloatingFortressSetPieceBattleMissionController : MissionLogic
{
	private abstract class ConversationLine
	{
		public void TryPlayLine()
		{
			if (CanBePlayed())
			{
				Play();
			}
		}

		protected abstract void Play();

		public abstract void Stop();

		public abstract bool IsPlaying();

		protected abstract bool CanBePlayed();
	}

	private class SimpleConversationLine : ConversationLine
	{
		private readonly TextObject _line;

		private readonly CharacterObject _speaker;

		private readonly float _cooldown;

		private readonly NotificationPriority _priority;

		private DialogNotificationHandle _handle;

		private float _blockedTime;

		public SimpleConversationLine(CharacterObject speaker, string line, float cooldown, NotificationPriority priority)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			_speaker = speaker;
			_cooldown = cooldown;
			_priority = priority;
			_line = new TextObject(line, (Dictionary<string, object>)null);
			_blockedTime = 0f;
		}

		protected override void Play()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			_handle = CampaignInformationManager.AddDialogLine(_line, _speaker, (Equipment)null, 0, _priority);
			_blockedTime = Mission.Current.CurrentTime + _cooldown;
		}

		public override void Stop()
		{
			CampaignInformationManager.ClearDialogNotification(_handle, false);
		}

		public override bool IsPlaying()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			if (_handle != null)
			{
				return (int)CampaignInformationManager.GetStatusOfDialogNotification(_handle) == 1;
			}
			return false;
		}

		protected override bool CanBePlayed()
		{
			return _blockedTime <= Mission.Current.CurrentTime;
		}
	}

	private class VariantConversationLine : ConversationLine
	{
		public enum VariationType
		{
			Ordered,
			Random
		}

		private int _current;

		private ConversationLine _active;

		private float _blockedTime;

		private readonly List<ConversationLine> _lines;

		private readonly float _cooldown;

		private readonly VariationType _variationType;

		private readonly bool _canShowEachLineOnce;

		public VariantConversationLine(ConversationLine[] lines, VariationType variationType, float cooldown, bool canShowEachLineOnce = false)
		{
			_lines = lines.ToList();
			_variationType = variationType;
			_cooldown = cooldown;
			_canShowEachLineOnce = canShowEachLineOnce;
			_current = -1;
			_active = null;
			_blockedTime = 0f;
		}

		protected override void Play()
		{
			switch (_variationType)
			{
			case VariationType.Ordered:
				_current = (_current + 1) % _lines.Count;
				break;
			case VariationType.Random:
				_current = MBRandom.RandomInt(0, _lines.Count);
				break;
			default:
				Debug.FailedAssert("Unknown variation type!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\Storyline\\MissionControllers\\FloatingFortressSetPieceBattleMissionController.cs", "Play", 137);
				throw new ArgumentOutOfRangeException();
			}
			_active = _lines[_current];
			_active.TryPlayLine();
			if (_canShowEachLineOnce)
			{
				_lines.RemoveAt(_current);
			}
			_blockedTime = Mission.Current.CurrentTime + _cooldown;
		}

		public override void Stop()
		{
			_active.Stop();
			_active = null;
		}

		public override bool IsPlaying()
		{
			if (_active != null)
			{
				return _active.IsPlaying();
			}
			return false;
		}

		protected override bool CanBePlayed()
		{
			if (_lines.Count > 0)
			{
				return _blockedTime <= Mission.Current.CurrentTime;
			}
			return false;
		}
	}

	private class SequencedConversationLine : ConversationLine
	{
		private float _blockedTime;

		private readonly float _cooldown;

		private readonly ConversationLine[] _lines;

		public SequencedConversationLine(ConversationLine[] lines, float cooldown)
		{
			_lines = lines;
			_cooldown = cooldown;
			_blockedTime = 0f;
		}

		protected override void Play()
		{
			ConversationLine[] lines = _lines;
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i].TryPlayLine();
			}
			_blockedTime = Mission.Current.CurrentTime + _cooldown;
		}

		public override void Stop()
		{
			ConversationLine[] lines = _lines;
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i].Stop();
			}
		}

		public override bool IsPlaying()
		{
			return _lines.Any((ConversationLine x) => x.IsPlaying());
		}

		protected override bool CanBePlayed()
		{
			return _blockedTime <= Mission.Current.CurrentTime;
		}
	}

	private class CircularBuffer<T>
	{
		private readonly T[] _buffer;

		private int _head;

		private int _tail;

		private readonly int _capacity;

		public int Count { get; private set; }

		public T this[int index]
		{
			get
			{
				int num = (_head + index) % _capacity;
				return _buffer[num];
			}
			set
			{
				int num = (_head + index) % _capacity;
				_buffer[num] = value;
			}
		}

		public CircularBuffer(int capacity)
		{
			_capacity = capacity;
			_buffer = new T[capacity];
			_head = 0;
			_tail = 0;
			Count = 0;
		}

		public void Add(T item)
		{
			_buffer[_tail] = item;
			_tail = (_tail + 1) % _capacity;
			if (Count < _capacity)
			{
				Count++;
			}
			else
			{
				_head = (_head + 1) % _capacity;
			}
		}
	}

	private class TrailController
	{
		private readonly CircularBuffer<Vec3> _positions;

		private readonly CircularBuffer<float> _timestamps;

		private readonly float _trailDelay;

		private readonly float _recordInterval;

		private float _lastRecordTime;

		public TrailController(float trailDelay, float recordInterval)
		{
			_trailDelay = trailDelay;
			_recordInterval = recordInterval;
			_lastRecordTime = 0f;
			int val = (int)Math.Ceiling(trailDelay / recordInterval);
			val = Math.Max(val, 10) + 1;
			_positions = new CircularBuffer<Vec3>(val);
			_timestamps = new CircularBuffer<float>(val);
		}

		public void RecordPosition(Vec3 position, float currentTime)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (currentTime - _lastRecordTime >= _recordInterval)
			{
				_positions.Add(position);
				_timestamps.Add(currentTime);
				_lastRecordTime = currentTime;
			}
		}

		public Vec3 GetTrailEndPosition(float currentTime)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			if (_positions.Count == 0)
			{
				return default(Vec3);
			}
			float num = currentTime - _trailDelay;
			for (int num2 = _timestamps.Count - 1; num2 >= 1; num2--)
			{
				float num3 = _timestamps[num2 - 1];
				float num4 = _timestamps[num2];
				if (num >= num3 && num <= num4)
				{
					float num5 = (num - num3) / (num4 - num3);
					Vec3 val = _positions[num2 - 1];
					Vec3 val2 = _positions[num2];
					return Vec3.Lerp(val, val2, num5);
				}
			}
			if (!(num <= _timestamps[0]))
			{
				return _positions[_positions.Count - 1];
			}
			return _positions[0];
		}
	}

	private const float PlayerShipTargetingWarningDistance = 15f;

	private const float TimeToLoseAfterLastBallistaShot = 5f;

	private const float BallistaRandomAttackRadius = 15f;

	private const float BallistaRandomAttackPointSelectionTime = 1f;

	private const string PlayerPhaseOneSpawnPointTag = "sp_player_ship";

	private const string PlayerPhaseTwoSpawnPointTag = "sp_player_phase_two_start";

	private const float PlayerShipTooCloseThresholdDistanceSquared = 10000f;

	private const float PlayerShipLowHpThresholdRatio = 0.65f;

	private const float PlayerRemainingAmmoThresholdRatio = 3f;

	private const float AllyShipAnchorFrameConnectionDistanceSquared = 900f;

	private const string PlayerStartingShipHull = "naval_storyline_quest_4_player_medit_ship";

	private const float AllyShipDistanceToSelfAnchor = 200f;

	private const int PlayerBallistaStartingAmmo = 30;

	private static readonly List<(string, string)[]> AllyShipUpgrades = new List<(string, string)[]>
	{
		new(string, string)[2]
		{
			("sail", "sails_lvl2"),
			("side", "side_northern_shields_lvl1")
		},
		new(string, string)[2]
		{
			("sail", "sails_lvl3"),
			("side", "side_northern_shields_lvl2")
		},
		new(string, string)[2]
		{
			("sail", "sails_lvl2"),
			("side", "side_northern_shields_lvl2")
		},
		new(string, string)[2]
		{
			("sail", "sails_lvl3"),
			("side", "side_northern_shields_lvl3")
		}
	};

	private const int BridgesBetweenEnemyShips = 1;

	private readonly List<Figurehead> _allyShipFigureheads = new List<Figurehead>
	{
		DefaultFigureheads.Raven,
		DefaultFigureheads.Turtle,
		DefaultFigureheads.Boar,
		DefaultFigureheads.Dragon
	};

	private readonly Dictionary<string, string> _playerShipUpgradePieces = new Dictionary<string, string>
	{
		{ "fore", "fore_heavy_ballista_stone" },
		{ "aft", "" },
		{ "hull", "" },
		{ "deck", "" },
		{ "oars", "" },
		{ "sail", "sails_lvl3" },
		{ "roof", "roof_8" }
	};

	private readonly List<string> _allyShipHulls = new List<string> { "northern_medium_ship", "northern_medium_ship", "northern_light_ship", "northern_medium_ship" };

	private readonly List<(string, int)> _playerShipTroops = new List<(string, int)>
	{
		("skolderbrotva_tier_2", 2),
		("skolderbrotva_tier_3", 28)
	};

	private readonly List<(string, int)[]> _allyShipAgents = new List<(string, int)[]>
	{
		new(string, int)[1] { ("skolderbrotva_tier_3", 40) },
		new(string, int)[1] { ("skolderbrotva_tier_3", 39) },
		new(string, int)[2]
		{
			("skolderbrotva_tier_3", 16),
			("skolderbrotva_tier_2", 3)
		},
		new(string, int)[2]
		{
			("gangradirs_kin_melee", 20),
			("gangradirs_kin_ranged", 20)
		}
	};

	private readonly (string, string)[] _enemyShipHulls = new(string, string)[8]
	{
		("naval_storyline_quest_4_boss_light_ship", "sp_enemy_ship_1"),
		("ship_storyline_quest_4_boss_cog_ship", "sp_enemy_ship_2"),
		("naval_storyline_quest_4_boss_light_ship", "sp_enemy_ship_3"),
		("naval_storyline_quest_4_boss_round_ship", "sp_enemy_ship_4"),
		("naval_storyline_quest_4_boss_light_ship", "sp_enemy_ship_5"),
		("ship_storyline_quest_4_boss_cog_ship", "sp_enemy_ship_7"),
		("naval_storyline_quest_4_boss_light_ship", "sp_enemy_ship_6"),
		("ship_storyline_quest_4_boss_cog_ship", "sp_enemy_ship_8")
	};

	private readonly List<(string, int)[]> _initialEnemyShipAgents = new List<(string, int)[]>
	{
		new(string, int)[2]
		{
			("sea_hounds_marksman", 5),
			("sea_hounds", 10)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 2),
			("sea_hounds_pups", 9)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 6),
			("sea_hounds_pups", 9)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 9),
			("sea_hounds_pups", 14)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 4),
			("sea_hounds", 11)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 2),
			("sea_hounds_pups", 4)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 3),
			("sea_hounds", 9)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 3),
			("sea_hounds_pups", 6)
		}
	};

	private readonly List<(string, int)[]> _reinforcementEnemyShipAgents = new List<(string, int)[]>
	{
		new(string, int)[2]
		{
			("sea_hounds_marksman", 2),
			("sea_hounds", 2)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 5),
			("sea_hounds_pups", 2)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 1),
			("sea_hounds_pups", 3)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 2),
			("sea_hounds_pups", 4)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 6),
			("sea_hounds", 2)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 1),
			("sea_hounds_pups", 2)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 2),
			("sea_hounds", 4)
		},
		new(string, int)[2]
		{
			("sea_hounds_marksman", 2),
			("sea_hounds_pups", 4)
		}
	};

	private readonly Dictionary<int, string> _enemyShipsToAddBallista = new Dictionary<int, string>
	{
		{ 2, "fore_mangonel" },
		{ 4, "fore_mangonel" },
		{ 6, "fore_mangonel" },
		{ 8, "fore_mangonel" }
	};

	private MissionShip _playerShip;

	private GameEntity _trailingTargetObject;

	private ShipTargetMissionObject _playerShipTargetObject;

	private readonly TrailController _playerShipTargetObjectTrailController = new TrailController(6f, 0.25f);

	private MBList<MissionShip> _enemyMissionShipsOrdered;

	private bool _isPhaseOneInitialized;

	private int _currentPhaseOneInitializationTick;

	private float _playerLoseRemainingTime = 5f;

	private float _lastRandomAttackPointPickTime;

	private Vec3 _randomAttackPoint;

	private bool _shouldStartPhaseTwo;

	private bool _isPhaseTwoInitialized;

	private int _currentPhaseTwoInitializationTick;

	private bool _isMissionSuccessful;

	private bool _isMissionFailed;

	private List<GameEntity> _entities = new List<GameEntity>();

	private readonly MBList<MissionShip> _playerAllyMissionShips = new MBList<MissionShip>();

	private readonly MBList<(MissionShip, bool)> _playerAllyShipAnchorState = new MBList<(MissionShip, bool)>();

	private readonly MBList<DestructableComponent> _enemySiegeWeaponDestructables = new MBList<DestructableComponent>();

	private readonly Dictionary<DestructableComponent, DestructableComponent> _enemySiegeWeaponByCover = new Dictionary<DestructableComponent, DestructableComponent>();

	private readonly Dictionary<RangedSiegeWeapon, Agent> _cachedMangonelAgents = new Dictionary<RangedSiegeWeapon, Agent>();

	private NavalAgentsLogic _navalAgentsLogic;

	private NavalShipsLogic _navalShipsLogic;

	private readonly ConversationLine _playerShipTooCloseLine;

	private readonly ConversationLine _playerShipLowHpLine;

	private readonly ConversationLine _playerShipRemainingAmmoLine;

	private readonly ConversationLine _playerShipStandingStillLine;

	private readonly ConversationLine _playerShipHitLine;

	private readonly ConversationLine _playerShipSailDestroyedLine;

	private readonly ConversationLine _playerTookMangonelDownLine;

	private readonly ConversationLine _playerTookAllMangonelsDownLine;

	private MissionObjectiveLogic _missionObjectiveLogic;

	private BoardFloatingFortressObjective _boardFloatingFortressObjective;

	private DefeatTheEnemyCrewObjective _defeatTheEnemyCrewObjective;

	public bool IsPhaseOneCompleted { get; private set; }

	public bool IsStartedFromCheckpoint { get; }

	public MBReadOnlyList<MissionShip> EnemyShipsOrdered => (MBReadOnlyList<MissionShip>)(object)_enemyMissionShipsOrdered;

	public FloatingFortressSetPieceBattleMissionController(bool startFromCheckpoint)
	{
		IsStartedFromCheckpoint = startFromCheckpoint;
		_playerShipStandingStillLine = new VariantConversationLine(new ConversationLine[4]
		{
			new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=PRzT0o1t}Keep rowing! The next hit might punch right through our deck!", 0f, (NotificationPriority)2),
			new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=3067dlpE}Keep moving! That last hit made our timbers groan!", 0f, (NotificationPriority)2),
			new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=jaKW2HIJ}Unless you want to swim, I suggest you keep moving!", 0f, (NotificationPriority)2),
			new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=BV06pwuU}Standing still? You planning to go down with the mast?", 0f, (NotificationPriority)2)
		}, VariantConversationLine.VariationType.Ordered, 10f);
		_playerShipHitLine = new VariantConversationLine(new ConversationLine[2]
		{
			new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=qA4pYH6z}That hit us! We’re still afloat, but the next time we might not be so lucky", 0f, (NotificationPriority)3),
			new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=Yv3BQ7cT}Stamp out those sparks, lads! Let’s not get hit again.", 0f, (NotificationPriority)3)
		}, VariantConversationLine.VariationType.Ordered, 15f);
		_playerTookMangonelDownLine = new VariantConversationLine(new ConversationLine[2]
		{
			new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=bdpsa5CC}One mangonel down!", 0f, (NotificationPriority)4),
			new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=k5NjdC48}You smashed that mangonel! Look at it, like a broken toy!", 0f, (NotificationPriority)4)
		}, VariantConversationLine.VariationType.Ordered, 0f, canShowEachLineOnce: true);
		_playerTookAllMangonelsDownLine = new SequencedConversationLine(new ConversationLine[2]
		{
			new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=75khXDaR}You silenced those mangonels! Now let’s all move in and board them!", 0f, (NotificationPriority)2),
			new SimpleConversationLine(NavalStorylineData.Gunnar.CharacterObject, "{=4r2IhSCi}We’re right behind you! Row, lads, row!", 0f, (NotificationPriority)2)
		}, 10000f);
		_playerShipTooCloseLine = new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=tl473Yje}Let’s keep our distance! Their decks are packed with bowmen!", 15f, (NotificationPriority)2);
		_playerShipLowHpLine = new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=eAabzGkE}Our timbers are groaning like a sick man.", 10000f, (NotificationPriority)2);
		_playerShipSailDestroyedLine = new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=gzvtND1s}Our sail is down!", 10000f, (NotificationPriority)3);
		_playerShipRemainingAmmoLine = new SimpleConversationLine(NavalStorylineData.Bjolgur.CharacterObject, "{=O4oqNTAl}Choose your targets! Take out the mangonels before we run out of bolts!", 10000f, (NotificationPriority)3);
	}

	public override void AfterStart()
	{
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_missionObjectiveLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionObjectiveLogic>();
		((MissionBehavior)this).Mission.Scene.SetAtmosphereWithName("TOD_naval_09_00_Overcast");
		_navalShipsLogic.ShipHitEvent += OnShipHit;
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)1, ((MissionBehavior)this).Mission.PlayerTeam.Color, ((MissionBehavior)this).Mission.PlayerTeam.Color2, ((MissionBehavior)this).Mission.PlayerTeam.Banner, true, false, true);
		_navalAgentsLogic.UpdateTeamAgentsData();
		MBMusicManager.Current.StartTheme((MusicTheme)10243, 0.5f, false);
	}

	public override void OnMissionTick(float dt)
	{
		if (!_isPhaseOneInitialized)
		{
			TickPhaseOneInitialization();
		}
		if (_shouldStartPhaseTwo && !_isPhaseTwoInitialized)
		{
			TickPhaseTwoInitialization();
		}
		if (_isPhaseOneInitialized && !_isPhaseTwoInitialized)
		{
			TickPhaseOneLogic(dt);
		}
		if (_isPhaseTwoInitialized)
		{
			TickPhaseTwoLogic(dt);
		}
		if (_isPhaseOneInitialized && IsStartedFromCheckpoint && !_isPhaseTwoInitialized)
		{
			Agent.Main.Controller = (AgentControllerType)2;
			_shouldStartPhaseTwo = true;
		}
	}

	private void TickPhaseOneInitialization()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		_currentPhaseOneInitializationTick++;
		if (_currentPhaseOneInitializationTick == 1)
		{
			UpdateEntityReferences();
			GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_wind");
			if (val != (GameEntity)null)
			{
				MatrixFrame globalFrame = val.GetGlobalFrame();
				Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				SetWindStrengthAndDirection(((Vec2)(ref asVec)).Normalized(), val.GetGlobalScale().z);
			}
			((MissionBehavior)this).Mission.Scene.SetWaterStrength(2f);
			SpawnPlayerShip();
			SpawnEnemyShips();
			ConnectEnemyShips();
			foreach (MissionShip item in (List<MissionShip>)(object)_enemyMissionShipsOrdered)
			{
				IShipOrigin shipOrigin = item.ShipOrigin;
				Ship val2;
				if ((val2 = (Ship)(object)((shipOrigin is Ship) ? shipOrigin : null)) != null)
				{
					val2.IsInvulnerable = true;
				}
			}
			((MissionBehavior)this).Mission.PlayerTeam.SetPlayerRole(true, true);
			UpdateEntityReferences();
		}
		if (_currentPhaseOneInitializationTick != 2)
		{
			return;
		}
		SpawnPlayerShipAgents();
		SpawnPlayer();
		for (int i = 0; i < ((List<MissionShip>)(object)_enemyMissionShipsOrdered).Count; i++)
		{
			(string, int)[] source = _initialEnemyShipAgents[i];
			SpawnEnemyShipAgents(((List<MissionShip>)(object)_enemyMissionShipsOrdered)[i], source);
		}
		_navalAgentsLogic.AssignCaptainToShipForDeploymentMode(Agent.Main, _playerShip, _playerShip);
		_navalShipsLogic.SetDeploymentMode(value: true);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)0);
		_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)2);
		if (Agent.Main != null && Agent.Main.IsUsingGameObject)
		{
			Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
		_navalShipsLogic.SetDeploymentMode(value: false);
		Mission.Current.OnDeploymentFinished();
		Mission.Current.OnAfterDeploymentFinished();
		foreach (MissionShip item2 in (List<MissionShip>)(object)_enemyMissionShipsOrdered)
		{
			item2.SetAnchor(isAnchored: true, anchorInPlace: true);
			item2.BlockConnection();
			if (item2.ShipSiegeWeapon != null)
			{
				_cachedMangonelAgents[item2.ShipSiegeWeapon] = ((UsableMachine)item2.ShipSiegeWeapon).PilotAgent;
				((UsableMachine)item2.ShipSiegeWeapon).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				((UsableMachine)item2.ShipSiegeWeapon).SetIsDisabledForAI(true);
			}
		}
		_playerShip.OnSetRangedWeaponControlMode(value: true);
		_isPhaseOneInitialized = true;
	}

	private void TickPhaseOneLogic(float dt)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		if (_playerShip.IsSinking)
		{
			OnMissionFailed();
			return;
		}
		if (_playerShip.SailHitPoints <= 0f)
		{
			_playerShipSailDestroyedLine.TryPlayLine();
		}
		if (_playerShip.HitPoints <= _playerShip.MaxHealth * 0.65f)
		{
			_playerShipLowHpLine.TryPlayLine();
		}
		if (((List<DestructableComponent>)(object)_enemySiegeWeaponDestructables).Count == 0)
		{
			return;
		}
		if (((UsableMachine)_playerShip.ShipSiegeWeapon).DestructionComponent.IsDestroyed || _playerShip.ShipSiegeWeapon.AmmoCount == 0)
		{
			_playerLoseRemainingTime -= dt;
			if (_playerLoseRemainingTime <= 0f)
			{
				OnMissionFailed();
				return;
			}
		}
		WeakGameEntity val = ((ScriptComponentBehavior)_playerShip).GameEntity;
		Vec3 val2 = ((WeakGameEntity)(ref val)).GlobalPosition;
		bool flag = ((Vec3)(ref val2)).Distance(_trailingTargetObject.GlobalPosition) < 15f;
		foreach (MissionShip item in (List<MissionShip>)(object)_enemyMissionShipsOrdered)
		{
			if (Agent.Main != null && item.GetIsAgentOnShip(Agent.Main))
			{
				OnMissionFailed();
			}
			val = ((ScriptComponentBehavior)_playerShip).GameEntity;
			val2 = ((WeakGameEntity)(ref val)).GlobalPosition;
			val = ((ScriptComponentBehavior)item).GameEntity;
			if (((Vec3)(ref val2)).DistanceSquared(((WeakGameEntity)(ref val)).GlobalPosition) <= 10000f)
			{
				_playerShipTooCloseLine.TryPlayLine();
			}
			if (item.ShipSiegeWeapon == null || ((UsableMachine)item.ShipSiegeWeapon).IsDisabledForAI)
			{
				continue;
			}
			RangedSiegeWeapon shipSiegeWeapon = item.ShipSiegeWeapon;
			if (!((UsableMachine)shipSiegeWeapon).IsDestroyed)
			{
				val = ((ScriptComponentBehavior)shipSiegeWeapon).GameEntity;
				Color val3 = new Color(1f, 0.68f, 0.44f, (MathF.Sin(((MissionBehavior)this).Mission.CurrentTime * 2f) + 1f) / 2f);
				((WeakGameEntity)(ref val)).SetContourColor((uint?)((Color)(ref val3)).ToUnsignedInteger(), true);
			}
			if (flag && !((MissionObject)((UsableMachine)shipSiegeWeapon).PilotStandingPoint).IsDisabled && ((UsableMachine)shipSiegeWeapon).PilotAgent != null && shipSiegeWeapon.CanShootAtPoint(_trailingTargetObject.GlobalPosition))
			{
				_playerShipStandingStillLine.TryPlayLine();
			}
			if (((UsableMachine)shipSiegeWeapon).IsDestroyed || ((MissionObject)((UsableMachine)shipSiegeWeapon).PilotStandingPoint).IsDisabled || (((UsableMissionObject)((UsableMachine)shipSiegeWeapon).PilotStandingPoint).UserAgent != null && ((UsableMissionObject)((UsableMachine)shipSiegeWeapon).PilotStandingPoint).UserAgent.IsActive()) || ((UsableMissionObject)((UsableMachine)shipSiegeWeapon).PilotStandingPoint).HasAIMovingTo || (int)shipSiegeWeapon.State != 0)
			{
				continue;
			}
			float num = 1000000f;
			Agent val4 = null;
			foreach (Agent item2 in (List<Agent>)(object)_navalAgentsLogic.GetActiveAgentsOfShip(item))
			{
				if (!item2.IsHero && item2.Detachment == null)
				{
					val2 = item2.Position;
					val = ((ScriptComponentBehavior)shipSiegeWeapon).GameEntity;
					float num2 = ((Vec3)(ref val2)).DistanceSquared(((WeakGameEntity)(ref val)).GlobalPosition);
					if (num2 < num)
					{
						num = num2;
						val4 = item2;
					}
				}
			}
			if (val4 != null)
			{
				((UsableMachine)shipSiegeWeapon).AddAgentAtSlotIndex(val4, ((UsableMachine)shipSiegeWeapon).PilotStandingPointSlotIndex);
			}
		}
		RangedSiegeWeapon shipSiegeWeapon2 = _playerShip.ShipSiegeWeapon;
		if (shipSiegeWeapon2 != null)
		{
			if ((float)shipSiegeWeapon2.AmmoCount <= (float)((List<DestructableComponent>)(object)_enemySiegeWeaponDestructables).Count * 3f)
			{
				_playerShipRemainingAmmoLine.TryPlayLine();
			}
			if (shipSiegeWeapon2.AmmoCount == 0)
			{
				OnMissionFailed();
			}
			if (!((UsableMachine)shipSiegeWeapon2).IsDestroyed && (((UsableMissionObject)((UsableMachine)shipSiegeWeapon2).PilotStandingPoint).UserAgent == null || !((UsableMissionObject)((UsableMachine)shipSiegeWeapon2).PilotStandingPoint).UserAgent.IsActive()) && !((UsableMissionObject)((UsableMachine)shipSiegeWeapon2).PilotStandingPoint).HasAIMovingTo && (int)shipSiegeWeapon2.State == 0)
			{
				float num3 = 1000000f;
				Agent val5 = null;
				foreach (Agent item3 in (List<Agent>)(object)_navalAgentsLogic.GetActiveAgentsOfShip(_playerShip))
				{
					if (!item3.IsHero && item3.Detachment == null)
					{
						val2 = item3.Position;
						val = ((ScriptComponentBehavior)shipSiegeWeapon2).GameEntity;
						float num4 = ((Vec3)(ref val2)).DistanceSquared(((WeakGameEntity)(ref val)).GlobalPosition);
						if (num4 < num3)
						{
							num3 = num4;
							val5 = item3;
						}
					}
				}
				if (val5 != null)
				{
					((UsableMachine)shipSiegeWeapon2).AddAgentAtSlotIndex(val5, ((UsableMachine)shipSiegeWeapon2).PilotStandingPointSlotIndex);
				}
			}
		}
		TrailController playerShipTargetObjectTrailController = _playerShipTargetObjectTrailController;
		val = ((ScriptComponentBehavior)_playerShip).GameEntity;
		playerShipTargetObjectTrailController.RecordPosition(((WeakGameEntity)(ref val)).GlobalPosition, ((MissionBehavior)this).Mission.CurrentTime);
		val = _trailingTargetObject.WeakEntity;
		((WeakGameEntity)(ref val)).SetGlobalPosition(_playerShipTargetObjectTrailController.GetTrailEndPosition(((MissionBehavior)this).Mission.CurrentTime));
		if (flag)
		{
			val = ((ScriptComponentBehavior)_playerShipTargetObject).GameEntity;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_playerShip).GameEntity;
			((WeakGameEntity)(ref val)).SetGlobalPosition(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			return;
		}
		if (_lastRandomAttackPointPickTime + 1f < ((MissionBehavior)this).Mission.CurrentTime)
		{
			_randomAttackPoint = GetRandomPointOnCircle(Vec3.Zero, 15f);
			_lastRandomAttackPointPickTime = ((MissionBehavior)this).Mission.CurrentTime;
		}
		val = ((ScriptComponentBehavior)_playerShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref val)).GlobalPosition + _randomAttackPoint;
		val = ((ScriptComponentBehavior)_playerShipTargetObject).GameEntity;
		((WeakGameEntity)(ref val)).SetGlobalPosition(globalPosition);
	}

	private void TickPhaseTwoLogic(float dt)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionObjective)_boardFloatingFortressObjective).IsCompleted && _defeatTheEnemyCrewObjective == null)
		{
			_defeatTheEnemyCrewObjective = new DefeatTheEnemyCrewObjective(((MissionBehavior)this).Mission);
			_missionObjectiveLogic.StartObjective((MissionObjective)(object)_defeatTheEnemyCrewObjective);
		}
		for (int i = 0; i < ((List<(MissionShip, bool)>)(object)_playerAllyShipAnchorState).Count; i++)
		{
			(MissionShip, bool) valueTuple = ((List<(MissionShip, bool)>)(object)_playerAllyShipAnchorState)[i];
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)valueTuple.Item1).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			if (valueTuple.Item2)
			{
				if (valueTuple.Item1.GetIsConnectedToEnemy() && valueTuple.Item1.Physics.IsAnchored)
				{
					valueTuple.Item1.SetAnchor(isAnchored: false);
				}
				continue;
			}
			if (valueTuple.Item1.Physics.IsAnchored)
			{
				MatrixFrame anchorGlobalFrame = valueTuple.Item1.Physics.AnchorGlobalFrame;
				if (((Vec3)(ref anchorGlobalFrame.origin)).DistanceSquared(globalPosition) < 200f)
				{
					valueTuple.Item1.SetAnchor(isAnchored: true, anchorInPlace: true);
					valueTuple.Item2 = true;
					((List<(MissionShip, bool)>)(object)_playerAllyShipAnchorState)[i] = valueTuple;
				}
				continue;
			}
			if (valueTuple.Item1.ShipOrder.TargetShip == null)
			{
				MissionShip missionShip = Extensions.MinBy<MissionShip, float>((IEnumerable<MissionShip>)_enemyMissionShipsOrdered, (Func<MissionShip, float>)delegate(MissionShip x)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_0009: Unknown result type (might be due to invalid IL or missing references)
					//IL_000e: Unknown result type (might be due to invalid IL or missing references)
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					//IL_0024: Unknown result type (might be due to invalid IL or missing references)
					WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)x).GameEntity;
					Vec3 globalPosition3 = ((WeakGameEntity)(ref gameEntity2)).GlobalPosition;
					gameEntity2 = ((ScriptComponentBehavior)valueTuple.Item1).GameEntity;
					return ((Vec3)(ref globalPosition3)).DistanceSquared(((WeakGameEntity)(ref gameEntity2)).GlobalPosition);
				});
				valueTuple.Item1.ShipOrder.SetShipEngageOrder(missionShip);
				valueTuple.Item1.ShipOrder.SetBoardingTargetShip(missionShip);
			}
			gameEntity = ((ScriptComponentBehavior)valueTuple.Item1.ShipOrder.TargetShip).GameEntity;
			Vec3 globalPosition2 = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			if (((Vec3)(ref globalPosition)).DistanceSquared(globalPosition2) < 900f)
			{
				Vec3 val = globalPosition2 - globalPosition;
				Vec3 val2 = ((Vec3)(ref val)).NormalizedCopy();
				valueTuple.Item1.SetAnchor(isAnchored: true);
				valueTuple.Item1.SetAnchorFrame(((Vec3)(ref globalPosition2)).AsVec2, ((Vec3)(ref val2)).AsVec2, 0.2f);
			}
		}
	}

	private void SpawnPlayerShip()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		Formation formation = Mission.GetTeam((TeamSideEnum)0).GetFormation((FormationClass)0);
		Ship val = new Ship(((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>("naval_storyline_quest_4_player_medit_ship"))
		{
			IsTradeable = false,
			IsUsedByQuest = true,
			Owner = PartyBase.MainParty
		};
		foreach (KeyValuePair<string, string> playerShipUpgradePiece in _playerShipUpgradePieces)
		{
			val.EquipUpgradePiece(playerShipUpgradePiece.Key, ((GameType)Campaign.Current).ObjectManager.GetObject<ShipUpgradePiece>(playerShipUpgradePiece.Value));
		}
		_playerShip = CreateMissionShip(val, IsStartedFromCheckpoint ? "sp_player_phase_two_start" : "sp_player_ship", formation);
		_playerShip.SetShipOrderActive(isOrderActive: false);
		_trailingTargetObject = GameEntity.CreateEmpty(((MissionBehavior)this).Mission.Scene, true, true, true);
		_playerShipTargetObject = MBExtensions.GetFirstScriptInFamilyDescending<ShipTargetMissionObject>(((ScriptComponentBehavior)_playerShip).GameEntity);
		((ShipBallistaAI)(object)((UsableMachine)_playerShip.ShipSiegeWeapon).Ai).SetCanAiUpdateAim(canAiUpdateAim: false);
		_playerShip.ShipSiegeWeapon.SetStartAmmo(30);
	}

	private void TickPhaseTwoInitialization()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		_currentPhaseTwoInitializationTick++;
		if (_currentPhaseTwoInitializationTick == 1)
		{
			if (!IsStartedFromCheckpoint)
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=BWSp3Uyj}Checkpoint reached.", (Dictionary<string, object>)null)).ToString(), new Color(0f, 1f, 0f, 1f)));
				GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag("sp_player_phase_two_start")));
				_navalShipsLogic.TeleportShip(_playerShip, val.GetGlobalFrame(), checkFreeArea: false);
			}
			((ShipBallistaAI)(object)((UsableMachine)_playerShip.ShipSiegeWeapon).Ai).SetCanAiUpdateAim(canAiUpdateAim: true);
			foreach (MissionShip item in (List<MissionShip>)(object)_enemyMissionShipsOrdered)
			{
				IShipOrigin shipOrigin = item.ShipOrigin;
				Ship val2;
				if ((val2 = (Ship)(object)((shipOrigin is Ship) ? shipOrigin : null)) != null)
				{
					val2.IsInvulnerable = false;
				}
			}
			SpawnAllyShips();
			if (Agent.Main.CurrentlyUsedGameObject != null)
			{
				Agent.Main.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			}
			_playerShip.SetShipOrderActive(isOrderActive: true);
		}
		if (_currentPhaseTwoInitializationTick == 2)
		{
			for (int num = 0; num < ((List<MissionShip>)(object)_playerAllyMissionShips).Count; num++)
			{
				SpawnAllyShipAgents(((List<MissionShip>)(object)_playerAllyMissionShips)[num], _allyShipAgents[num]);
			}
			for (int num2 = 0; num2 < ((List<MissionShip>)(object)_enemyMissionShipsOrdered).Count; num2++)
			{
				(string, int)[] source = _reinforcementEnemyShipAgents[num2];
				SpawnEnemyShipAgents(((List<MissionShip>)(object)_enemyMissionShipsOrdered)[num2], source);
			}
			foreach (MissionShip item2 in (List<MissionShip>)(object)_enemyMissionShipsOrdered)
			{
				item2.ResetConnectionBlock();
				item2.ShipOrder.SetOrderOarsmenLevel(0);
				item2.ShipOrder.SetCutLoose(enable: false);
			}
			List<MissionShip> list = ((IEnumerable<MissionShip>)_enemyMissionShipsOrdered).ToList();
			foreach (MissionShip playerAllyMissionShip in (List<MissionShip>)(object)_playerAllyMissionShips)
			{
				MissionShip missionShip = Extensions.MinBy<MissionShip, float>((IEnumerable<MissionShip>)list, (Func<MissionShip, float>)delegate(MissionShip x)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_0009: Unknown result type (might be due to invalid IL or missing references)
					//IL_000e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0017: Unknown result type (might be due to invalid IL or missing references)
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					//IL_001f: Unknown result type (might be due to invalid IL or missing references)
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)x).GameEntity;
					Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
					gameEntity = ((ScriptComponentBehavior)playerAllyMissionShip).GameEntity;
					return ((Vec3)(ref globalPosition)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
				});
				list.Remove(missionShip);
				playerAllyMissionShip.ShipOrder.SetShipEngageOrder(missionShip);
				playerAllyMissionShip.ShipOrder.SetBoardingTargetShip(missionShip);
			}
			_navalAgentsLogic.SetDeploymentMode(value: true);
			_navalShipsLogic.SetDeploymentMode(value: true);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)0);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)2);
			_navalAgentsLogic.AssignAndTeleportCrewToShipMachines((TeamSideEnum)1);
			_navalAgentsLogic.SetDeploymentMode(value: false);
			_navalShipsLogic.SetDeploymentMode(value: false);
			CampaignInformationManager.ClearAllDialogNotifications(false);
			_playerTookAllMangonelsDownLine.TryPlayLine();
			_boardFloatingFortressObjective = new BoardFloatingFortressObjective(((MissionBehavior)this).Mission, _playerShip, _enemyMissionShipsOrdered);
			_missionObjectiveLogic.StartObjective((MissionObjective)(object)_boardFloatingFortressObjective);
			_isPhaseTwoInitialized = true;
		}
		Agent.Main.Health = Agent.Main.HealthLimit;
	}

	private void SpawnAllyShips()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		List<Formation> list = ((IEnumerable<Formation>)Mission.GetTeam((TeamSideEnum)1).FormationsIncludingEmpty).Where((Formation x) => x != _playerShip.Formation).ToList();
		for (int num = 0; num < _allyShipHulls.Count; num++)
		{
			ShipHull hull = ((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>(_allyShipHulls[num]);
			Ship val = (Ship)(((object)((IEnumerable<Ship>)PartyBase.MainParty.Ships).FirstOrDefault((Func<Ship, bool>)((Ship x) => x.ShipHull == hull))) ?? ((object)new Ship(hull)
			{
				IsTradeable = false,
				IsUsedByQuest = true,
				Owner = PartyBase.MainParty
			}));
			(string, string)[] array = AllyShipUpgrades[num];
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				var (text, text2) = array[num2];
				if (val.HasSlot(text))
				{
					val.EquipUpgradePiece(text, MBObjectManager.Instance.GetObject<ShipUpgradePiece>(text2));
				}
			}
			val.ChangeFigurehead(_allyShipFigureheads[num]);
			string allySpawnPoint = GetAllySpawnPoint(num);
			MissionShip missionShip = CreateMissionShip(val, allySpawnPoint, list[num]);
			GameEntity val2 = Mission.Current.Scene.FindEntityWithTag(allySpawnPoint);
			_navalShipsLogic.TeleportShip(missionShip, val2.GetGlobalFrame(), checkFreeArea: false);
			((List<MissionShip>)(object)_playerAllyMissionShips).Add(missionShip);
			((List<(MissionShip, bool)>)(object)_playerAllyShipAnchorState).Add((missionShip, false));
		}
		foreach (MissionShip item in (List<MissionShip>)(object)_playerAllyMissionShips)
		{
			((MissionObject)item).OnDeploymentFinished();
		}
	}

	private void OnEnemyShipBallistaDestroyed(DestructableComponent target, Agent attackerAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!IsPhaseOneCompleted)
		{
			((List<DestructableComponent>)(object)_enemySiegeWeaponDestructables).Remove(target);
			_playerTookMangonelDownLine.TryPlayLine();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)target).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetContourColor((uint?)null, true);
			if (((List<DestructableComponent>)(object)_enemySiegeWeaponDestructables).Count == 0)
			{
				IsPhaseOneCompleted = true;
			}
		}
	}

	private void OnEnemyShipBallistaCoverDestroyed(DestructableComponent target, Agent attackerAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		DestructableComponent val = _enemySiegeWeaponByCover[target];
		if (!val.IsDestroyed)
		{
			MBGUID id = ((MBObjectBase)Game.Current.ObjectManager.GetObject<ItemObject>("ballista_projectile")).Id;
			int internalValue = (int)((MBGUID)(ref id)).InternalValue;
			Agent main = Agent.Main;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)val).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			Vec3 one = Vec3.One;
			MissionWeapon val2 = new MissionWeapon(ItemObject.GetItemFromWeaponKind(internalValue), (ItemModifier)null, (Banner)null);
			val.TriggerOnHit(main, 10000, globalPosition, one, ref val2, -1, (ScriptComponentBehavior)null);
		}
	}

	public override void OnBehaviorInitialize()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!SailWindProfile.IsSailWindProfileInitialized)
		{
			SailWindProfile.InitializeProfile();
		}
		Team team = Mission.GetTeam((TeamSideEnum)0);
		((MissionBehavior)this).Mission.Teams.Add(team.Side, team.Color, team.Color2, team.Banner, true, false, true);
	}

	private void UpdateEntityReferences()
	{
		((MissionBehavior)this).Mission.Scene.GetEntities(ref _entities);
	}

	private MissionShip CreateMissionShip(Ship ship, string spawnPointId, Formation formation)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		GameEntity val = ((IEnumerable<GameEntity>)_entities).FirstOrDefault((Func<GameEntity, bool>)((GameEntity t) => t.HasTag(spawnPointId)));
		MatrixFrame shipFrame = val.GetGlobalFrame();
		Scene scene = Mission.Current.Scene;
		Vec3 globalPosition = val.GlobalPosition;
		float waterLevelAtPosition = scene.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, true, false);
		shipFrame.origin = new Vec3(val.GlobalPosition.x, val.GlobalPosition.y, waterLevelAtPosition, -1f);
		MissionShip missionShip = missionBehavior.SpawnShip((IShipOrigin)(object)ship, in shipFrame, formation.Team, formation, spawnAnchored: false, (FormationClass)8);
		missionShip.ShipOrder.FormationJoinShip(formation);
		return missionShip;
	}

	private void SpawnEnemyShips()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Expected O, but got Unknown
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Expected O, but got Unknown
		MBList<Formation> formationsIncludingEmpty = Mission.GetTeam((TeamSideEnum)2).FormationsIncludingEmpty;
		_enemyMissionShipsOrdered = new MBList<MissionShip>();
		for (int i = 0; i < _enemyShipHulls.Length; i++)
		{
			(string, string) tuple = _enemyShipHulls[i];
			string item = tuple.Item1;
			string item2 = tuple.Item2;
			ShipHull shipHullObject = ((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>(item);
			Ship val = (Ship)(((object)((IEnumerable<Ship>)MapEvent.PlayerMapEvent.GetLeaderParty(Mission.Current.PlayerEnemyTeam.Side).Ships).FirstOrDefault((Func<Ship, bool>)((Ship x) => x.ShipHull == shipHullObject))) ?? ((object)new Ship(shipHullObject)
			{
				IsTradeable = false,
				IsUsedByQuest = true,
				Owner = MapEvent.PlayerMapEvent.GetLeaderParty(Mission.Current.PlayerEnemyTeam.Side)
			}));
			if (val.HasSlot("fore"))
			{
				bool flag = !IsStartedFromCheckpoint && _enemyShipsToAddBallista.ContainsKey(i + 1);
				val.EquipUpgradePiece("fore", flag ? ((GameType)Campaign.Current).ObjectManager.GetObject<ShipUpgradePiece>(_enemyShipsToAddBallista[i + 1]) : null);
			}
			MissionShip missionShip = CreateMissionShip(val, item2, ((List<Formation>)(object)formationsIncludingEmpty)[i]);
			GameEntity val2 = Mission.Current.Scene.FindEntityWithTag(item2);
			missionShip.SetShipOrderActive(isOrderActive: false);
			missionShip.ShipOrder.SetOrderOarsmenLevel(0);
			missionShip.SetCustomSailSetting(enableCustomSailSetting: true, SailInput.Raised);
			missionShip.SetController(ShipControllerType.None, autoUpdateController: false);
			((MissionObject)((UsableMachine)missionShip.ShipControllerMachine).PilotStandingPoint).SetDisabled(false);
			missionShip.SetCanBeTakenOver(value: false);
			_navalShipsLogic.TeleportShip(missionShip, val2.GetGlobalFrame(), checkFreeArea: false, anchorShip: true);
			if (missionShip.ShipSiegeWeapon != null)
			{
				((List<DestructableComponent>)(object)_enemySiegeWeaponDestructables).Add(((UsableMachine)missionShip.ShipSiegeWeapon).DestructionComponent);
			}
			((List<MissionShip>)(object)_enemyMissionShipsOrdered).Add(missionShip);
		}
		foreach (DestructableComponent item3 in (List<DestructableComponent>)(object)_enemySiegeWeaponDestructables)
		{
			item3.OnDestroyed += new OnHitTakenAndDestroyedDelegate(OnEnemyShipBallistaDestroyed);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)item3).GameEntity;
			WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("ballista_cover");
			DestructableComponent firstScriptOfType = ((WeakGameEntity)(ref firstChildEntityWithTag)).GetFirstScriptOfType<DestructableComponent>();
			if (firstScriptOfType != null)
			{
				_enemySiegeWeaponByCover.Add(firstScriptOfType, item3);
				firstScriptOfType.OnDestroyed += new OnHitTakenAndDestroyedDelegate(OnEnemyShipBallistaCoverDestroyed);
			}
		}
	}

	private void ConnectEnemyShips()
	{
		for (int i = 0; i < ((List<MissionShip>)(object)_enemyMissionShipsOrdered).Count; i++)
		{
			int index = i + 1;
			if (i == ((List<MissionShip>)(object)_enemyMissionShipsOrdered).Count - 1)
			{
				index = 0;
			}
			TryMaintainConnection(((List<MissionShip>)(object)_enemyMissionShipsOrdered)[i], ((List<MissionShip>)(object)_enemyMissionShipsOrdered)[index]);
		}
	}

	private void TryMaintainConnection(MissionShip ship, MissionShip otherShip)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ship.AttachmentMachines)
		{
			if (item.CurrentAttachment != null && item.CurrentAttachment.AttachmentTarget.OwnerShip == otherShip)
			{
				num++;
			}
		}
		if (num >= 1)
		{
			return;
		}
		Vec3 fortressCenter = Vec3.Zero;
		foreach (MissionShip item2 in (List<MissionShip>)(object)_enemyMissionShipsOrdered)
		{
			Vec3 val = fortressCenter;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
			fortressCenter = val + ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		}
		fortressCenter /= (float)((List<MissionShip>)(object)_enemyMissionShipsOrdered).Count;
		foreach (ShipAttachmentMachine item3 in ((IEnumerable<ShipAttachmentMachine>)ship.AttachmentMachines).OrderBy(delegate(ShipAttachmentMachine x)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)x).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity2)).GlobalPosition;
			return ((Vec3)(ref globalPosition)).DistanceSquared(fortressCenter);
		}))
		{
			if (item3.CurrentAttachment != null)
			{
				continue;
			}
			item3.SetPreferredTargetShip(otherShip);
			if (item3.LinkedAttachmentPointMachine.CurrentAttachment != null)
			{
				continue;
			}
			item3.SetCanConnectToFriends(canConnectToFriends: true);
			ShipAttachmentPointMachine bestEnemyAttachment = item3.GetBestEnemyAttachment(checkAttachmentAlreadyExists: true);
			if (bestEnemyAttachment != null)
			{
				item3.ConnectWithAttachmentPointMachine(bestEnemyAttachment, forceBridge: true, unbreakableBridge: true);
				num++;
				if (num >= 1)
				{
					break;
				}
			}
		}
	}

	private void SpawnPlayer()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new PartyAgentOrigin(PartyBase.MainParty, CharacterObject.PlayerCharacter, -1, default(UniqueTroopDescriptor), false, false), _playerShip);
		_navalAgentsLogic.SpawnNextBatch((TeamSideEnum)0);
		Agent main = Agent.Main;
		_navalAgentsLogic.AssignCaptainToShipForDeploymentMode(main, _playerShip);
		Mission.Current.PlayerTeam.PlayerOrderController.Owner = main;
		((MissionBehavior)this).Mission.PlayerTeam.GetFormation((FormationClass)0).PlayerOwner = main;
		main.OnAgentHealthChanged += new OnAgentHealthChangedDelegate(OnMainAgentHealthChanged);
	}

	private void SpawnPlayerShipAgents()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		List<CharacterObject> list = new List<CharacterObject>();
		foreach (var (text, count) in _playerShipTroops)
		{
			list.AddRange(Enumerable.Repeat<CharacterObject>(((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(text), count));
		}
		NavalAgentsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		missionBehavior.SetDesiredTroopCountOfShip(_playerShip, list.Count + 1);
		int deckFrameCount = _playerShip.DeckFrameCount;
		Extensions.Shuffle<CharacterObject>((IList<CharacterObject>)list);
		for (int i = 0; i < deckFrameCount && i < list.Count; i++)
		{
			MatrixFrame nextOuterInnerSpawnGlobalFrame = _playerShip.GetNextOuterInnerSpawnGlobalFrame();
			CharacterObject val = list.ElementAtOrDefault(i);
			if (val == null)
			{
				break;
			}
			AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerTeam).Formation(_playerShip.Formation)
				.InitialPosition(ref nextOuterInnerSpawnGlobalFrame.origin);
			Vec2 val2 = ((Vec3)(ref nextOuterInnerSpawnGlobalFrame.rotation.f)).AsVec2;
			val2 = ((Vec2)(ref val2)).Normalized();
			AgentBuildData val3 = obj.InitialDirection(ref val2).NoHorses(true).NoWeapons(false);
			Agent val4 = Mission.Current.SpawnAgent(val3, false);
			val4.SetAlarmState((AIStateFlag)3);
			missionBehavior.AddAgentToShip(val4, _playerShip);
		}
	}

	private void SetWindStrengthAndDirection(Vec2 direction, float strength)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = Mission.Current.Scene;
		Vec2 val = strength * direction;
		scene.SetGlobalWindVelocity(ref val);
	}

	private void SpawnEnemyShipAgents(MissionShip ship, (string, int)[] source)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		NavalAgentsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		missionBehavior.SetDesiredTroopCountOfShip(ship, source.Sum(((string, int) x) => x.Item2));
		List<CharacterObject> list = new List<CharacterObject>();
		for (int num = 0; num < source.Length; num++)
		{
			var (text, count) = source[num];
			list.AddRange(Enumerable.Repeat<CharacterObject>(((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(text), count));
		}
		Extensions.Shuffle<CharacterObject>((IList<CharacterObject>)list);
		int deckFrameCount = ship.DeckFrameCount;
		for (int num2 = 0; num2 < deckFrameCount && num2 < list.Count; num2++)
		{
			CharacterObject val = list[num2];
			if (val == null)
			{
				break;
			}
			MatrixFrame nextOuterInnerSpawnGlobalFrame = ship.GetNextOuterInnerSpawnGlobalFrame();
			AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerEnemyTeam).InitialPosition(ref nextOuterInnerSpawnGlobalFrame.origin);
			Vec2 val2 = ((Vec3)(ref nextOuterInnerSpawnGlobalFrame.rotation.f)).AsVec2;
			val2 = ((Vec2)(ref val2)).Normalized();
			AgentBuildData val3 = obj.InitialDirection(ref val2).Formation(ship.Formation).NoHorses(true)
				.NoWeapons(false);
			Agent val4 = Mission.Current.SpawnAgent(val3, false);
			val4.SetAlarmState((AIStateFlag)3);
			missionBehavior.AddAgentToShip(val4, ship);
		}
	}

	private void SpawnAllyShipAgents(MissionShip ship, (string, int)[] source)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		NavalAgentsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		missionBehavior.SetDesiredTroopCountOfShip(ship, source.Sum(((string, int) x) => x.Item2));
		List<CharacterObject> list = new List<CharacterObject>();
		for (int num = 0; num < source.Length; num++)
		{
			var (text, count) = source[num];
			list.AddRange(Enumerable.Repeat<CharacterObject>(((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(text), count));
		}
		Extensions.Shuffle<CharacterObject>((IList<CharacterObject>)list);
		int deckFrameCount = ship.DeckFrameCount;
		for (int num2 = 0; num2 < deckFrameCount && num2 < list.Count; num2++)
		{
			MatrixFrame nextOuterInnerSpawnGlobalFrame = ship.GetNextOuterInnerSpawnGlobalFrame();
			CharacterObject val = list[num2];
			if (val == null)
			{
				break;
			}
			AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.PlayerAllyTeam).InitialPosition(ref nextOuterInnerSpawnGlobalFrame.origin);
			Vec2 val2 = ((Vec3)(ref nextOuterInnerSpawnGlobalFrame.rotation.f)).AsVec2;
			val2 = ((Vec2)(ref val2)).Normalized();
			AgentBuildData val3 = obj.InitialDirection(ref val2).NoHorses(true).NoWeapons(false);
			Agent val4 = Mission.Current.SpawnAgent(val3, false);
			val4.SetAlarmState((AIStateFlag)3);
			missionBehavior.AddAgentToShip(val4, ship);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if ((Extensions.IsEmpty<Agent>((IEnumerable<Agent>)((MissionBehavior)this).Mission.PlayerTeam.ActiveAgents) || (affectedAgent.IsMainAgent && !_shouldStartPhaseTwo)) && !_isMissionSuccessful)
		{
			OnMissionFailed();
		}
		else if (Extensions.IsEmpty<Agent>((IEnumerable<Agent>)((MissionBehavior)this).Mission.PlayerEnemyTeam.ActiveAgents) && !_isMissionFailed && !_isMissionSuccessful)
		{
			OnMissionSucceeded();
		}
	}

	private void OnMainAgentHealthChanged(Agent agent, float oldHealth, float newHealth)
	{
		if (!_shouldStartPhaseTwo && newHealth <= 0f)
		{
			OnMissionFailed();
		}
	}

	private void OnMissionSucceeded()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		if (!_isMissionSuccessful && !_isMissionFailed && (int)Mission.Current.CurrentState != 3)
		{
			_isMissionSuccessful = true;
			PlayerEncounter.CampaignBattleResult = CampaignBattleResult.GetResult((BattleState)2, false);
		}
	}

	private void OnMissionFailed()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		if (!_isMissionFailed && !_isMissionSuccessful && (int)Mission.Current.CurrentState != 3)
		{
			_isMissionFailed = true;
			PlayerEncounter.CampaignBattleResult = CampaignBattleResult.GetResult((BattleState)1, false);
			((MissionBehavior)this).Mission.EndMission();
		}
	}

	public override bool MissionEnded(ref MissionResult missionResult)
	{
		bool result = false;
		if (_isMissionSuccessful)
		{
			missionResult = MissionResult.CreateSuccessful((IMission)(object)((MissionBehavior)this).Mission, true);
			result = true;
		}
		else if (_isMissionFailed)
		{
			missionResult = MissionResult.CreateDefeated((IMission)(object)((MissionBehavior)this).Mission);
			result = true;
		}
		MBMusicManager.Current.ForceStopThemeWithFadeOut();
		return result;
	}

	public void OnViewFadeOut(int reason)
	{
		switch (reason)
		{
		case 1:
		{
			_playerShip.SetShipOrderActive(isOrderActive: true);
			MBList<ShipMangonel> val = new MBList<ShipMangonel>();
			foreach (MissionShip item in (List<MissionShip>)(object)_enemyMissionShipsOrdered)
			{
				if (item.ShipSiegeWeapon != null)
				{
					((List<ShipMangonel>)(object)val).Add(item.ShipSiegeWeapon as ShipMangonel);
					((UsableMachine)item.ShipSiegeWeapon).SetIsDisabledForAI(false);
					if (_cachedMangonelAgents.TryGetValue(item.ShipSiegeWeapon, out var value))
					{
						ModuleExtensions.StartUsingMachine(value.Formation, (UsableMachine)(object)item.ShipSiegeWeapon, false);
						((UsableMachine)item.ShipSiegeWeapon).AddAgentAtSlotIndex(value, ((UsableMachine)item.ShipSiegeWeapon).PilotStandingPointSlotIndex);
					}
				}
			}
			_missionObjectiveLogic.StartObjective((MissionObjective)(object)new DestroyMangonelsObjective(((MissionBehavior)this).Mission, val));
			Agent.Main.Controller = (AgentControllerType)2;
			break;
		}
		case 2:
			(((IEnumerable<QuestBase>)Campaign.Current.QuestManager.Quests).FirstOrDefault((Func<QuestBase, bool>)((QuestBase x) => x is CaptureTheImperialMerchantPrusas)) as CaptureTheImperialMerchantPrusas)?.OnCheckPointReached();
			_shouldStartPhaseTwo = true;
			break;
		case 0:
			break;
		}
	}

	public override void OnRetreatMission()
	{
		_isMissionFailed = true;
		PlayerEncounter.CampaignBattleResult = CampaignBattleResult.GetResult((BattleState)1, false);
	}

	private void OnShipHit(MissionShip ship, Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, MissionWeapon weapon, int affectorWeaponSlotOrMissileIndex)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionWeapon)(ref weapon)).CurrentUsageItem != null && Extensions.HasAnyFlag<WeaponFlags>(((MissionWeapon)(ref weapon)).CurrentUsageItem.WeaponFlags, (WeaponFlags)131072) && ship == _playerShip && !_isPhaseTwoInitialized)
		{
			_playerShipHitLine.TryPlayLine();
		}
	}

	private void DestroyEnemyBallistas()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		MBGUID id = ((MBObjectBase)Game.Current.ObjectManager.GetObject<ItemObject>("ballista_projectile")).Id;
		int internalValue = (int)((MBGUID)(ref id)).InternalValue;
		for (int num = ((List<DestructableComponent>)(object)_enemySiegeWeaponDestructables).Count - 1; num >= 0; num--)
		{
			DestructableComponent obj = ((List<DestructableComponent>)(object)_enemySiegeWeaponDestructables)[num];
			Agent main = Agent.Main;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)((List<DestructableComponent>)(object)_enemySiegeWeaponDestructables)[num]).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			Vec3 one = Vec3.One;
			MissionWeapon val = new MissionWeapon(ItemObject.GetItemFromWeaponKind(internalValue), (ItemModifier)null, (Banner)null);
			obj.TriggerOnHit(main, 1000, globalPosition, one, ref val, -1, (ScriptComponentBehavior)null);
		}
	}

	private static string GetAllySpawnPoint(int i)
	{
		return $"sp_player_reinforcement_{i + 1}";
	}

	private Vec3 GetRandomPointOnCircle(Vec3 center, float radius)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		float num = MBRandom.RandomFloat * MathF.PI * 2f;
		float num2 = center.x + radius * MathF.Cos(num);
		float num3 = center.y + radius * MathF.Sin(num);
		return new Vec3(num2, num3, center.z, -1f);
	}
}
