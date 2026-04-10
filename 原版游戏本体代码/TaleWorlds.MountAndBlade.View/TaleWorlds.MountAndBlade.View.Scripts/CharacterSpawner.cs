using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.View.Scripts;

public class CharacterSpawner : ScriptComponentBehavior
{
	public bool Enabled;

	public string PoseAction = "act_walk_idle_unarmed";

	public string LordName = "main_hero_for_perf";

	public string ActionSetSuffix = "_facegen";

	public string PoseActionForHorse = "horse_stand_3";

	public string BodyPropertiesString = "<BodyProperties version=\"4\" age=\"23.16\" weight=\"0.3333\" build=\"0\" key=\"00000C07000000010011111211151111000701000010000000111011000101000000500202111110000000000000000000000000000000000000000000A00000\" />";

	public bool IsWeaponWielded;

	public bool HasMount;

	public bool WieldOffHand = true;

	public float AnimationProgress;

	public float HorseAnimationProgress;

	private MBGameManager _editorGameManager;

	private bool isFinished;

	private bool CreateFaceImmediately = true;

	private AgentVisuals _agentVisuals;

	private GameEntity _agentEntity;

	private GameEntity _horseEntity;

	public bool Active;

	private MatrixFrame _spawnFrame;

	public uint ClothColor1 { get; private set; }

	public uint ClothColor2 { get; private set; }

	protected override void OnInit()
	{
		((ScriptComponentBehavior)this).OnInit();
		ClothColor1 = uint.MaxValue;
		ClothColor2 = uint.MaxValue;
	}

	protected void Init()
	{
		Active = false;
	}

	protected override void OnEditorInit()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		((ScriptComponentBehavior)this).OnEditorInit();
		if (Game.Current == null)
		{
			_editorGameManager = (MBGameManager)new EditorGameManager();
			isFinished = !((GameManagerBase)_editorGameManager).DoLoadingForGameManager();
		}
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (!Enabled)
		{
			return;
		}
		if (!isFinished && _editorGameManager != null)
		{
			isFinished = !((GameManagerBase)_editorGameManager).DoLoadingForGameManager();
		}
		if (Game.Current != null && _agentVisuals == null)
		{
			SpawnCharacter();
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
			if (_agentEntity != (GameEntity)null)
			{
				_agentEntity.SetVisibilityExcludeParents(false);
			}
			if (_horseEntity != (GameEntity)null)
			{
				_horseEntity.SetVisibilityExcludeParents(false);
			}
		}
		if (_agentVisuals != null)
		{
			Skeleton skeleton = _agentVisuals.GetVisuals().GetSkeleton();
			if ((NativeObject)(object)skeleton != (NativeObject)null)
			{
				skeleton.Freeze(false);
				skeleton.TickAnimationsAndForceUpdate(0.001f, _agentVisuals.GetVisuals().GetGlobalFrame(), false);
			}
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		((ScriptComponentBehavior)this).OnRemoved(removeReason);
		if (_agentVisuals != null)
		{
			_agentVisuals.Reset();
			((NativeObject)_agentVisuals.GetVisuals()).ManualInvalidate();
			_agentVisuals = null;
		}
	}

	public void SetCreateFaceImmediately(bool value)
	{
		CreateFaceImmediately = value;
	}

	private void Disable()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (_agentEntity != (GameEntity)null && _agentEntity.Parent == ((ScriptComponentBehavior)this).GameEntity)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).RemoveChild(_agentEntity.WeakEntity, false, false, true, 34);
		}
		if (_agentVisuals != null)
		{
			_agentVisuals.Reset();
			((NativeObject)_agentVisuals.GetVisuals()).ManualInvalidate();
			_agentVisuals = null;
		}
		if (_horseEntity != (GameEntity)null && _horseEntity.Parent == ((ScriptComponentBehavior)this).GameEntity)
		{
			_horseEntity.Scene.RemoveEntity(_horseEntity, 96);
		}
		Active = false;
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		if (variableName == "Enabled")
		{
			if (Enabled)
			{
				Init();
			}
			else
			{
				Disable();
			}
		}
		if (!Enabled)
		{
			return;
		}
		switch (variableName)
		{
		case "LordName":
		case "ActionSetSuffix":
			if (_agentVisuals != null)
			{
				BasicCharacterObject val5 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
				if (val5 != null)
				{
					InitWithCharacter(CharacterCode.CreateFrom(val5), useBodyProperties: true);
				}
			}
			break;
		case "PoseActionForHorse":
		{
			BasicCharacterObject val4 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
			if (val4 != null)
			{
				InitWithCharacter(CharacterCode.CreateFrom(val4), useBodyProperties: true);
			}
			break;
		}
		case "PoseAction":
			if (_agentVisuals != null)
			{
				BasicCharacterObject val2 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
				if (val2 != null)
				{
					InitWithCharacter(CharacterCode.CreateFrom(val2), useBodyProperties: true);
				}
			}
			break;
		case "IsWeaponWielded":
		{
			BasicCharacterObject val7 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
			WieldWeapon(CharacterCode.CreateFrom(val7));
			break;
		}
		case "AnimationProgress":
		{
			Skeleton skeleton = _agentVisuals.GetVisuals().GetSkeleton();
			skeleton.Freeze(false);
			skeleton.TickAnimationsAndForceUpdate(0.001f, _agentVisuals.GetVisuals().GetGlobalFrame(), false);
			skeleton.SetAnimationParameterAtChannel(0, MBMath.ClampFloat(AnimationProgress, 0f, 1f));
			skeleton.SetUptoDate(false);
			skeleton.Freeze(true);
			break;
		}
		case "HorseAnimationProgress":
			if (_horseEntity != (GameEntity)null)
			{
				_horseEntity.Skeleton.Freeze(false);
				_horseEntity.Skeleton.TickAnimationsAndForceUpdate(0.001f, _horseEntity.GetGlobalFrame(), false);
				_horseEntity.Skeleton.SetAnimationParameterAtChannel(0, MBMath.ClampFloat(HorseAnimationProgress, 0f, 1f));
				_horseEntity.Skeleton.SetUptoDate(false);
				_horseEntity.Skeleton.Freeze(true);
			}
			break;
		case "HasMount":
			if (HasMount)
			{
				BasicCharacterObject val6 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
				SpawnMount(CharacterCode.CreateFrom(val6));
			}
			else if (_horseEntity != (GameEntity)null)
			{
				_horseEntity.Scene.RemoveEntity(_horseEntity, 97);
			}
			break;
		case "Active":
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(Active);
			if (_agentEntity != (GameEntity)null)
			{
				_agentEntity.SetVisibilityExcludeParents(Active);
			}
			if (_horseEntity != (GameEntity)null)
			{
				_horseEntity.SetVisibilityExcludeParents(Active);
			}
			break;
		}
		case "FaceKeyString":
		{
			BasicCharacterObject val3 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
			if (val3 != null)
			{
				InitWithCharacter(CharacterCode.CreateFrom(val3), useBodyProperties: true);
			}
			break;
		}
		case "WieldOffHand":
		{
			BasicCharacterObject val = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(LordName);
			if (val != null)
			{
				InitWithCharacter(CharacterCode.CreateFrom(val), useBodyProperties: true);
			}
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
			InitWithCharacter(characterCode, useBodyProperties: true);
		}
	}

	public void InitWithCharacter(CharacterCode characterCode, bool useBodyProperties = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Invalid comparison between Unknown and I4
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0621: Unknown result type (might be due to invalid IL or missing references)
		//IL_0626: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).BreakPrefab();
		if (_agentEntity != (GameEntity)null && _agentEntity.Parent == ((ScriptComponentBehavior)this).GameEntity)
		{
			val = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref val)).RemoveChild(_agentEntity.WeakEntity, false, false, true, 35);
		}
		_agentVisuals?.Reset();
		AgentVisuals agentVisuals = _agentVisuals;
		if (agentVisuals != null)
		{
			MBAgentVisuals visuals = agentVisuals.GetVisuals();
			if (visuals != null)
			{
				((NativeObject)visuals).ManualInvalidate();
			}
		}
		if (_horseEntity != (GameEntity)null && _horseEntity.Parent == ((ScriptComponentBehavior)this).GameEntity)
		{
			_horseEntity.Scene.RemoveEntity(_horseEntity, 98);
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		_agentEntity = GameEntity.CreateEmpty(((WeakGameEntity)(ref val)).Scene, false, true, true);
		_agentEntity.Name = "TableauCharacterAgentVisualsEntity";
		_spawnFrame = _agentEntity.GetFrame();
		BodyProperties bodyProperties = characterCode.BodyProperties;
		if (useBodyProperties)
		{
			BodyProperties.FromString(BodyPropertiesString, ref bodyProperties);
		}
		if (characterCode.Color1 != uint.MaxValue)
		{
			ClothColor1 = characterCode.Color1;
		}
		if (characterCode.Color2 != uint.MaxValue)
		{
			ClothColor2 = characterCode.Color2;
		}
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(characterCode.Race);
		AgentVisualsData obj = new AgentVisualsData().Equipment(characterCode.CalculateEquipment()).BodyProperties(bodyProperties).Race(characterCode.Race)
			.Frame(_spawnFrame)
			.Scale(1f)
			.SkeletonType((SkeletonType)(characterCode.IsFemale ? 1 : 0))
			.Entity(_agentEntity)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, characterCode.IsFemale, ActionSetSuffix))
			.ActionCode(ref ActionIndexCache.act_inventory_idle_start);
		val = ((ScriptComponentBehavior)this).GameEntity;
		_agentVisuals = AgentVisuals.Create(obj.Scene(((WeakGameEntity)(ref val)).Scene).Monster(baseMonsterFromRace).PrepareImmediately(CreateFaceImmediately)
			.Banner(characterCode.Banner)
			.ClothColor1(ClothColor1)
			.ClothColor2(ClothColor2)
			.UseMorphAnims(true), "TableauCharacterAgentVisuals", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
		_agentVisuals.SetAction(ActionIndexCache.Create(PoseAction), MBMath.ClampFloat(AnimationProgress, 0f, 1f));
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).AddChild(_agentEntity.WeakEntity, false);
		WieldWeapon(characterCode);
		if ((int)characterCode.FormationClass == 1)
		{
			Equipment equipment = _agentVisuals.GetEquipment();
			for (int i = 0; i < 4; i++)
			{
				EquipmentElement val2 = equipment[i];
				ItemObject item = ((EquipmentElement)(ref val2)).Item;
				if (((item != null) ? item.PrimaryWeapon : null) != null)
				{
					val2 = equipment[i];
					ItemObject item2 = ((EquipmentElement)(ref val2)).Item;
					MBDebug.Print("Ranged primary weapon: " + ((item2 != null) ? ((MBObjectBase)item2).StringId : null) + "\n", 0, (DebugColor)12, 17592186044416uL);
				}
			}
		}
		MatrixFrame identity = MatrixFrame.Identity;
		_agentVisuals.GetVisuals().SetFrame(ref identity);
		if (HasMount)
		{
			SpawnMount(characterCode);
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).SetVisibilityExcludeParents(true);
		_agentEntity.SetVisibilityExcludeParents(true);
		if (_horseEntity != (GameEntity)null)
		{
			_horseEntity.SetVisibilityExcludeParents(true);
		}
		_agentEntity.CheckResources(true, true);
		Skeleton skeleton = _agentVisuals.GetVisuals().GetSkeleton();
		skeleton.Freeze(false);
		skeleton.TickAnimationsAndForceUpdate(0.001f, _agentVisuals.GetVisuals().GetGlobalFrame(), false);
		skeleton.SetUptoDate(false);
		skeleton.Freeze(true);
		val = _agentEntity.WeakEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref val)).GetChildren())
		{
			WeakGameEntity current = child;
			((WeakGameEntity)(ref current)).SetBoundingboxDirty();
		}
		skeleton.Freeze(false);
		skeleton.TickAnimationsAndForceUpdate(0.001f, _agentVisuals.GetVisuals().GetGlobalFrame(), false);
		skeleton.SetAnimationParameterAtChannel(0, MBMath.ClampFloat(AnimationProgress, 0f, 1f));
		skeleton.SetUptoDate(false);
		skeleton.Freeze(true);
		_agentEntity.SetBoundingboxDirty();
		val = _agentEntity.WeakEntity;
		foreach (WeakGameEntity child2 in ((WeakGameEntity)(ref val)).GetChildren())
		{
			WeakGameEntity current2 = child2;
			((WeakGameEntity)(ref current2)).SetBoundingboxDirty();
		}
		((NativeObject)skeleton).ManualInvalidate();
		if (_horseEntity != (GameEntity)null)
		{
			_horseEntity.Skeleton.Freeze(false);
			_horseEntity.Skeleton.TickAnimationsAndForceUpdate(0.001f, _horseEntity.GetGlobalFrame(), false);
			_horseEntity.Skeleton.SetUptoDate(false);
			_horseEntity.Skeleton.Freeze(true);
			_horseEntity.SetBoundingboxDirty();
			_horseEntity.SetBoundingboxDirty();
			val = _horseEntity.WeakEntity;
			foreach (WeakGameEntity child3 in ((WeakGameEntity)(ref val)).GetChildren())
			{
				WeakGameEntity current3 = child3;
				((WeakGameEntity)(ref current3)).SetBoundingboxDirty();
			}
		}
		if (_horseEntity != (GameEntity)null)
		{
			_horseEntity.Skeleton.Freeze(false);
			_horseEntity.Skeleton.TickAnimationsAndForceUpdate(0.001f, _horseEntity.GetGlobalFrame(), false);
			_horseEntity.Skeleton.SetAnimationParameterAtChannel(0, MBMath.ClampFloat(HorseAnimationProgress, 0f, 1f));
			_horseEntity.Skeleton.SetUptoDate(false);
			_horseEntity.Skeleton.Freeze(true);
			_horseEntity.SetBoundingboxDirty();
			val = _horseEntity.WeakEntity;
			foreach (WeakGameEntity child4 in ((WeakGameEntity)(ref val)).GetChildren())
			{
				WeakGameEntity current4 = child4;
				((WeakGameEntity)(ref current4)).SetBoundingboxDirty();
			}
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).SetBoundingboxDirty();
		val = ((ScriptComponentBehavior)this).GameEntity;
		if (!((WeakGameEntity)(ref val)).Scene.IsEditorScene())
		{
			if (_agentEntity != (GameEntity)null)
			{
				((NativeObject)_agentEntity).ManualInvalidate();
			}
			if (_horseEntity != (GameEntity)null)
			{
				((NativeObject)_horseEntity).ManualInvalidate();
			}
		}
	}

	private void WieldWeapon(CharacterCode characterCode)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected I4, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Invalid comparison between Unknown and I8
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		if (!IsWeaponWielded)
		{
			return;
		}
		WeaponFlags val = (WeaponFlags)0;
		FormationClass formationClass = characterCode.FormationClass;
		switch ((int)formationClass)
		{
		case 0:
		case 2:
		case 4:
		case 5:
		case 6:
		case 8:
		case 9:
			val = (WeaponFlags)1;
			break;
		case 1:
		case 3:
			val = (WeaponFlags)2;
			break;
		}
		int num = -1;
		int num2 = -1;
		WeaponComponentData val2 = null;
		Equipment val3 = characterCode.CalculateEquipment();
		EquipmentElement val4;
		for (int i = 0; i < 4; i++)
		{
			val4 = val3[i];
			ItemObject item = ((EquipmentElement)(ref val4)).Item;
			if (((item != null) ? item.PrimaryWeapon : null) == null)
			{
				continue;
			}
			if (num2 == -1)
			{
				val4 = val3[i];
				if (Extensions.HasAnyFlag<ItemFlags>(((EquipmentElement)(ref val4)).Item.ItemFlags, (ItemFlags)524288) && WieldOffHand)
				{
					num2 = i;
				}
			}
			if (num == -1)
			{
				val4 = val3[i];
				if (Extensions.HasAnyFlag<WeaponFlags>(((EquipmentElement)(ref val4)).Item.PrimaryWeapon.WeaponFlags, val))
				{
					num = i;
					val4 = val3[i];
					val2 = ((EquipmentElement)(ref val4)).Item.PrimaryWeapon;
				}
			}
		}
		if (WieldOffHand && (long)val == 2 && val2 != null)
		{
			for (int j = 0; j < 4; j++)
			{
				val4 = val3[j];
				ItemObject item2 = ((EquipmentElement)(ref val4)).Item;
				WeaponComponentData val5 = ((item2 != null) ? item2.PrimaryWeapon : null);
				if (val5 != null && val5.IsAmmo && val5.WeaponClass == val2.AmmoClass)
				{
					num2 = j;
				}
			}
		}
		if (num != -1 || num2 != -1)
		{
			AgentVisualsData copyAgentVisualsData = _agentVisuals.GetCopyAgentVisualsData();
			AgentVisualsData obj = copyAgentVisualsData.RightWieldedItemIndex(num).LeftWieldedItemIndex(num2);
			ActionIndexCache val6 = ActionIndexCache.Create(PoseAction);
			obj.ActionCode(ref val6).Frame(_spawnFrame);
			_agentVisuals.Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData);
		}
	}

	private void SpawnMount(CharacterCode characterCode)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		Equipment val = characterCode.CalculateEquipment();
		EquipmentElement val2 = val[(EquipmentIndex)10];
		ItemObject item = ((EquipmentElement)(ref val2)).Item;
		if (item == null)
		{
			HasMount = false;
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_horseEntity = GameEntity.CreateEmpty(((WeakGameEntity)(ref gameEntity)).Scene, false, true, true);
		_horseEntity.Name = "MountEntity";
		Monster monster = item.HorseComponent.Monster;
		MBActionSet actionSet = MBActionSet.GetActionSet(monster.ActionSetCode);
		GameEntityExtensions.CreateAgentSkeleton(_horseEntity, ((MBActionSet)(ref actionSet)).GetSkeletonName(), false, actionSet, monster.MonsterUsage, monster);
		_horseEntity.CopyComponentsToSkeleton();
		Skeleton skeleton = _horseEntity.Skeleton;
		ActionIndexCache val3 = ActionIndexCache.Create(PoseActionForHorse);
		MBSkeletonExtensions.SetAgentActionChannel(skeleton, 0, ref val3, MBMath.ClampFloat(HorseAnimationProgress, 0f, 1f), -0.2f, true, 0f);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).AddChild(_horseEntity.WeakEntity, false);
		GameEntity horseEntity = _horseEntity;
		val2 = val[10];
		ItemObject item2 = ((EquipmentElement)(ref val2)).Item;
		val2 = val[11];
		ItemObject item3 = ((EquipmentElement)(ref val2)).Item;
		val2 = val[10];
		MountVisualCreator.AddMountMeshToEntity(horseEntity, item2, item3, MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val2)).Item, MBRandom.RandomInt()));
		_horseEntity.SetVisibilityExcludeParents(true);
		_horseEntity.Skeleton.TickAnimations(0.01f, _agentVisuals.GetVisuals().GetGlobalFrame(), true);
	}
}
