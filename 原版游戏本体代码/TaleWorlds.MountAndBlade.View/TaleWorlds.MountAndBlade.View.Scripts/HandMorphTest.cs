using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.View.Scripts;

public class HandMorphTest : ScriptComponentBehavior
{
	private const bool CreateFaceImmediately = true;

	private readonly ActionIndexCache act_defend_up_fist_active = ActionIndexCache.Create("act_defend_up_fist_active");

	private readonly ActionIndexCache act_visual_test_morph_animation = ActionIndexCache.Create("act_visual_test_morph_animation");

	private MBGameManager _editorGameManager;

	private bool _isFinished;

	private bool _characterSpawned;

	private AgentVisuals _agentVisuals;

	public uint ClothColor1 { get; private set; }

	public uint ClothColor2 { get; private set; }

	protected override void OnInit()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		ClothColor1 = uint.MaxValue;
		ClothColor2 = uint.MaxValue;
		if (_agentVisuals == null && !_characterSpawned)
		{
			SpawnCharacter();
			_characterSpawned = true;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		_agentVisuals.GetVisuals().SetFrame(ref globalFrame);
	}

	protected override void OnEditorInit()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		((ScriptComponentBehavior)this).OnEditorInit();
		if (Game.Current == null)
		{
			_editorGameManager = (MBGameManager)new EditorGameManager();
		}
		ClothColor1 = uint.MaxValue;
		ClothColor2 = uint.MaxValue;
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!_isFinished && _editorGameManager != null)
		{
			_isFinished = !((GameManagerBase)_editorGameManager).DoLoadingForGameManager();
		}
		if (Game.Current != null && _agentVisuals == null && !_characterSpawned)
		{
			SpawnCharacter();
			_characterSpawned = true;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		_agentVisuals.GetVisuals().SetFrame(ref globalFrame);
	}

	public void SpawnCharacter()
	{
		CharacterCode characterCode = CharacterCode.CreateFrom(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_0"));
		InitWithCharacter(characterCode);
	}

	public void Reset()
	{
		_agentVisuals?.Reset();
	}

	public void InitWithCharacter(CharacterCode characterCode)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame frame = ((WeakGameEntity)(ref gameEntity)).GetFrame();
		frame.rotation.s.z = 0f;
		frame.rotation.f.z = 0f;
		((Vec3)(ref frame.rotation.s)).Normalize();
		((Vec3)(ref frame.rotation.f)).Normalize();
		frame.rotation.u = Vec3.CrossProduct(frame.rotation.s, frame.rotation.f);
		characterCode.BodyProperties = new BodyProperties(new DynamicBodyProperties(20f, 0f, 0f), ((BodyProperties)(ref characterCode.BodyProperties)).StaticProperties);
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(characterCode.Race);
		AgentVisualsData obj = new AgentVisualsData().Equipment(characterCode.CalculateEquipment()).BodyProperties(characterCode.BodyProperties).Race(characterCode.Race)
			.SkeletonType((SkeletonType)(characterCode.IsFemale ? 1 : 0))
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, characterCode.IsFemale, "_facegen"))
			.ActionCode(ref act_visual_test_morph_animation);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_agentVisuals = AgentVisuals.Create(obj.Scene(((WeakGameEntity)(ref gameEntity)).Scene).Monster(baseMonsterFromRace).PrepareImmediately(true)
			.UseMorphAnims(true)
			.ClothColor1(ClothColor1)
			.ClothColor2(ClothColor2)
			.Frame(frame), "HandMorphTest", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
		_agentVisuals.SetAction(in act_defend_up_fist_active, 1f);
		MatrixFrame val = frame;
		_agentVisuals.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(1f, val, true);
		_agentVisuals.GetVisuals().SetFrame(ref val);
	}

	protected override void OnRemoved(int removeReason)
	{
		((ScriptComponentBehavior)this).OnRemoved(removeReason);
		_agentVisuals.Reset();
	}
}
