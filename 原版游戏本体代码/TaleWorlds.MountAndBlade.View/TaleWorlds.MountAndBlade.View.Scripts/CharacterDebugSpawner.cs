using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.Scripts;

public class CharacterDebugSpawner : ScriptComponentBehavior
{
	private readonly ActionIndexCache[] _actionIndices = (ActionIndexCache[])(object)new ActionIndexCache[97]
	{
		ActionIndexCache.Create("act_start_conversation"),
		ActionIndexCache.Create("act_stand_conversation"),
		ActionIndexCache.Create("act_start_angry_conversation"),
		ActionIndexCache.Create("act_stand_angry_conversation"),
		ActionIndexCache.Create("act_start_sad_conversation"),
		ActionIndexCache.Create("act_stand_sad_conversation"),
		ActionIndexCache.Create("act_start_happy_conversation"),
		ActionIndexCache.Create("act_stand_happy_conversation"),
		ActionIndexCache.Create("act_start_busy_conversation"),
		ActionIndexCache.Create("act_stand_busy_conversation"),
		ActionIndexCache.Create("act_explaining_conversation"),
		ActionIndexCache.Create("act_introduction_conversation"),
		ActionIndexCache.Create("act_wondering_conversation"),
		ActionIndexCache.Create("act_unknown_conversation"),
		ActionIndexCache.Create("act_friendly_conversation"),
		ActionIndexCache.Create("act_offer_conversation"),
		ActionIndexCache.Create("act_negative_conversation"),
		ActionIndexCache.Create("act_affermative_conversation"),
		ActionIndexCache.Create("act_secret_conversation"),
		ActionIndexCache.Create("act_remember_conversation"),
		ActionIndexCache.Create("act_laugh_conversation"),
		ActionIndexCache.Create("act_threat_conversation"),
		ActionIndexCache.Create("act_scared_conversation"),
		ActionIndexCache.Create("act_flirty_conversation"),
		ActionIndexCache.Create("act_thanks_conversation"),
		ActionIndexCache.Create("act_farewell_conversation"),
		ActionIndexCache.Create("act_troop_cavalry_sword"),
		ActionIndexCache.Create("act_inventory_idle_start"),
		ActionIndexCache.Create("act_inventory_idle"),
		ActionIndexCache.Create("act_character_developer_idle"),
		ActionIndexCache.Create("act_inventory_cloth_equip"),
		ActionIndexCache.Create("act_inventory_glove_equip"),
		ActionIndexCache.Create("act_jump"),
		ActionIndexCache.Create("act_jump_loop"),
		ActionIndexCache.Create("act_jump_end"),
		ActionIndexCache.Create("act_jump_end_hard"),
		ActionIndexCache.Create("act_jump_left_stance"),
		ActionIndexCache.Create("act_jump_loop_left_stance"),
		ActionIndexCache.Create("act_jump_end_left_stance"),
		ActionIndexCache.Create("act_jump_end_hard_left_stance"),
		ActionIndexCache.Create("act_jump_forward"),
		ActionIndexCache.Create("act_jump_forward_loop"),
		ActionIndexCache.Create("act_jump_forward_end"),
		ActionIndexCache.Create("act_jump_forward_end_hard"),
		ActionIndexCache.Create("act_jump_forward_left_stance"),
		ActionIndexCache.Create("act_jump_forward_loop_left_stance"),
		ActionIndexCache.Create("act_jump_forward_end_left_stance"),
		ActionIndexCache.Create("act_jump_forward_end_hard_left_stance"),
		ActionIndexCache.Create("act_jump_backward"),
		ActionIndexCache.Create("act_jump_backward_loop"),
		ActionIndexCache.Create("act_jump_backward_end"),
		ActionIndexCache.Create("act_jump_backward_end_hard"),
		ActionIndexCache.Create("act_jump_backward_left_stance"),
		ActionIndexCache.Create("act_jump_backward_loop_left_stance"),
		ActionIndexCache.Create("act_jump_backward_end_left_stance"),
		ActionIndexCache.Create("act_jump_backward_end_hard_left_stance"),
		ActionIndexCache.Create("act_jump_forward_right"),
		ActionIndexCache.Create("act_jump_forward_right_left_stance"),
		ActionIndexCache.Create("act_jump_forward_left"),
		ActionIndexCache.Create("act_jump_forward_left_left_stance"),
		ActionIndexCache.Create("act_jump_right"),
		ActionIndexCache.Create("act_jump_right_loop"),
		ActionIndexCache.Create("act_jump_right_end"),
		ActionIndexCache.Create("act_jump_right_end_hard"),
		ActionIndexCache.Create("act_jump_left"),
		ActionIndexCache.Create("act_jump_left_loop"),
		ActionIndexCache.Create("act_jump_left_end"),
		ActionIndexCache.Create("act_jump_left_end_hard"),
		ActionIndexCache.Create("act_jump_loop_long"),
		ActionIndexCache.Create("act_jump_loop_long_left_stance"),
		ActionIndexCache.Create("act_throne_sit_down_from_front"),
		ActionIndexCache.Create("act_throne_stand_up_to_front"),
		ActionIndexCache.Create("act_throne_sit_idle"),
		ActionIndexCache.Create("act_sit_down_from_front"),
		ActionIndexCache.Create("act_sit_down_from_right"),
		ActionIndexCache.Create("act_sit_down_from_left"),
		ActionIndexCache.Create("act_sit_down_on_floor_1"),
		ActionIndexCache.Create("act_sit_down_on_floor_2"),
		ActionIndexCache.Create("act_sit_down_on_floor_3"),
		ActionIndexCache.Create("act_stand_up_to_front"),
		ActionIndexCache.Create("act_stand_up_to_right"),
		ActionIndexCache.Create("act_stand_up_to_left"),
		ActionIndexCache.Create("act_stand_up_floor_1"),
		ActionIndexCache.Create("act_stand_up_floor_2"),
		ActionIndexCache.Create("act_stand_up_floor_3"),
		ActionIndexCache.Create("act_sit_1"),
		ActionIndexCache.Create("act_sit_2"),
		ActionIndexCache.Create("act_sit_3"),
		ActionIndexCache.Create("act_sit_4"),
		ActionIndexCache.Create("act_sit_5"),
		ActionIndexCache.Create("act_sit_6"),
		ActionIndexCache.Create("act_sit_7"),
		ActionIndexCache.Create("act_sit_8"),
		ActionIndexCache.Create("act_sit_idle_on_floor_1"),
		ActionIndexCache.Create("act_sit_idle_on_floor_2"),
		ActionIndexCache.Create("act_sit_idle_on_floor_3"),
		ActionIndexCache.Create("act_sit_conversation")
	};

	public readonly ActionIndexCache PoseAction = ActionIndexCache.act_walk_idle_unarmed;

	public string LordName = "main_hero";

	public bool IsWeaponWielded;

	private Vec2 MovementDirection;

	private float MovementSpeed;

	private float PhaseDiff;

	private float Time;

	private float ActionSetTimer;

	private float ActionChangeInterval;

	private float MovementDirectionChange;

	private static MBGameManager _editorGameManager = null;

	private static int _editorGameManagerRefCount = 0;

	private static bool isFinished = false;

	private static int gameTickFrameNo = -1;

	private bool CreateFaceImmediately = true;

	private AgentVisuals _agentVisuals;

	public uint ClothColor1 { get; private set; }

	public uint ClothColor2 { get; private set; }

	protected override void OnInit()
	{
		((ScriptComponentBehavior)this).OnInit();
		ClothColor1 = uint.MaxValue;
		ClothColor2 = uint.MaxValue;
	}

	protected override void OnEditorInit()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ScriptComponentBehavior)this).OnEditorInit();
		if (_editorGameManager == null)
		{
			_editorGameManager = (MBGameManager)new EditorGameManager();
		}
		_editorGameManagerRefCount++;
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		if (!isFinished && gameTickFrameNo != Utilities.EngineFrameNo)
		{
			gameTickFrameNo = Utilities.EngineFrameNo;
			isFinished = !((GameManagerBase)_editorGameManager).DoLoadingForGameManager();
		}
		if (Game.Current != null && _agentVisuals == null)
		{
			MovementDirection.x = MBRandom.RandomFloatNormal;
			MovementDirection.y = MBRandom.RandomFloatNormal;
			((Vec2)(ref MovementDirection)).Normalize();
			MovementSpeed = MBRandom.RandomFloat * 9f + 1f;
			PhaseDiff = MBRandom.RandomFloat;
			MovementDirectionChange = MBRandom.RandomFloatNormal * MathF.PI;
			Time = 0f;
			ActionSetTimer = 0f;
			ActionChangeInterval = MBRandom.RandomFloat * 0.5f + 0.5f;
			SpawnCharacter();
		}
		MatrixFrame globalFrame = _agentVisuals.GetVisuals().GetGlobalFrame();
		_agentVisuals.GetVisuals().SetFrame(ref globalFrame);
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(MovementDirection, 0f, -1f);
		((Vec3)(ref val)).RotateAboutZ(MovementDirectionChange * dt);
		MovementDirection.x = val.x;
		MovementDirection.y = val.y;
		float num = MovementSpeed * (MathF.Sin(PhaseDiff + Time) * 0.5f) + 2f;
		Vec2 agentLocalSpeed = MovementDirection * num;
		_agentVisuals.SetAgentLocalSpeed(agentLocalSpeed);
		Time += dt;
		if (Time - ActionSetTimer > ActionChangeInterval)
		{
			ActionSetTimer = Time;
			_agentVisuals.SetAction(in _actionIndices[MBRandom.RandomInt(_actionIndices.Length)]);
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		((ScriptComponentBehavior)this).OnRemoved(removeReason);
		Reset();
		_editorGameManagerRefCount--;
		if (_editorGameManagerRefCount == 0)
		{
			_editorGameManager = null;
			isFinished = false;
		}
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		switch (variableName)
		{
		case "LordName":
		{
			BasicCharacterObject val2 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
			AgentVisualsData copyAgentVisualsData = _agentVisuals.GetCopyAgentVisualsData();
			copyAgentVisualsData.BodyProperties(val2.GetBodyProperties((Equipment)null, -1)).SkeletonType((SkeletonType)(val2.IsFemale ? 1 : 0)).ActionSet(MBGlobals.GetActionSetWithSuffix(copyAgentVisualsData.MonsterData, val2.IsFemale, "_poses"))
				.Equipment(val2.Equipment)
				.UseMorphAnims(true);
			_agentVisuals.Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData);
			break;
		}
		case "PoseAction":
			_agentVisuals?.SetAction(in PoseAction);
			break;
		case "IsWeaponWielded":
		{
			BasicCharacterObject val = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
			WieldWeapon(CharacterCode.CreateFrom(val));
			break;
		}
		}
	}

	public void SetClothColors(uint color1, uint color2)
	{
		ClothColor1 = color1;
		ClothColor2 = color2;
	}

	public void SpawnCharacter()
	{
		BasicCharacterObject val = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
		if (val != null)
		{
			CharacterCode characterCode = CharacterCode.CreateFrom(val);
			InitWithCharacter(characterCode);
		}
	}

	public void Reset()
	{
		_agentVisuals?.Reset();
	}

	public void InitWithCharacter(CharacterCode characterCode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		GameEntity val = GameEntity.CreateEmpty(((WeakGameEntity)(ref gameEntity)).Scene, false, true, true);
		val.Name = "TableauCharacterAgentVisualsEntity";
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(characterCode.Race);
		AgentVisualsData obj = new AgentVisualsData().Equipment(characterCode.CalculateEquipment()).BodyProperties(characterCode.BodyProperties).Race(characterCode.Race)
			.Frame(val.GetGlobalFrame())
			.SkeletonType((SkeletonType)(characterCode.IsFemale ? 1 : 0))
			.Entity(val)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, characterCode.IsFemale, "_facegen"))
			.ActionCode(ref ActionIndexCache.act_inventory_idle_start);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_agentVisuals = AgentVisuals.Create(obj.Scene(((WeakGameEntity)(ref gameEntity)).Scene).Monster(baseMonsterFromRace).PrepareImmediately(CreateFaceImmediately)
			.Banner(characterCode.Banner)
			.ClothColor1(ClothColor1)
			.ClothColor2(ClothColor2), "CharacterDebugSpawner", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
		_agentVisuals.SetAction(in PoseAction, MBRandom.RandomFloat);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).AddChild(val.WeakEntity, false);
		WieldWeapon(characterCode);
		_agentVisuals.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.1f, _agentVisuals.GetVisuals().GetGlobalFrame(), true);
	}

	public void WieldWeapon(CharacterCode characterCode)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!IsWeaponWielded)
		{
			return;
		}
		int num = -1;
		int num2 = -1;
		Equipment val = characterCode.CalculateEquipment();
		for (int i = 0; i < 4; i++)
		{
			EquipmentElement val2 = val[i];
			ItemObject item = ((EquipmentElement)(ref val2)).Item;
			if (((item != null) ? item.PrimaryWeapon : null) == null)
			{
				continue;
			}
			if (num2 == -1)
			{
				val2 = val[i];
				if (Extensions.HasAnyFlag<ItemFlags>(((EquipmentElement)(ref val2)).Item.ItemFlags, (ItemFlags)524288))
				{
					num2 = i;
				}
			}
			if (num == -1)
			{
				val2 = val[i];
				if (Extensions.HasAnyFlag<WeaponFlags>(((EquipmentElement)(ref val2)).Item.PrimaryWeapon.WeaponFlags, (WeaponFlags)3))
				{
					num = i;
				}
			}
		}
		if (num != -1 || num2 != -1)
		{
			AgentVisualsData copyAgentVisualsData = _agentVisuals.GetCopyAgentVisualsData();
			copyAgentVisualsData.RightWieldedItemIndex(num).LeftWieldedItemIndex(num2).ActionCode(ref PoseAction);
			_agentVisuals.Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData);
		}
	}
}
