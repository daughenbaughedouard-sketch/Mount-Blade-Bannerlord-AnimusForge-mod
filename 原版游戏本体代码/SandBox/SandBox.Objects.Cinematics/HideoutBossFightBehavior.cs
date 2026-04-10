using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.Objects.Cinematics;

public class HideoutBossFightBehavior : ScriptComponentBehavior
{
	private readonly struct HideoutBossFightPreviewEntityInfo
	{
		public readonly GameEntity BaseEntity;

		public readonly GameEntity InitialEntity;

		public readonly GameEntity TargetEntity;

		public static HideoutBossFightPreviewEntityInfo Invalid => new HideoutBossFightPreviewEntityInfo(null, null, null);

		public bool IsValid => BaseEntity == (GameEntity)null;

		public HideoutBossFightPreviewEntityInfo(GameEntity baseEntity, GameEntity initialEntity, GameEntity targetEntity)
		{
			BaseEntity = baseEntity;
			InitialEntity = initialEntity;
			TargetEntity = targetEntity;
		}
	}

	private enum HideoutSeedPerturbOffset
	{
		Player,
		Boss,
		Ally,
		Bandit
	}

	private const int PreviewPerturbSeed = 0;

	private const float PreviewPerturbAmount = 0.25f;

	private const int PreviewTroopCount = 10;

	private const float PreviewPlacementAngle = MathF.PI / 20f;

	private const string InitialFrameTag = "initial_frame";

	private const string TargetFrameTag = "target_frame";

	private const string BossPreviewPrefab = "hideout_boss_fight_preview_boss";

	private const string PlayerPreviewPrefab = "hideout_boss_fight_preview_player";

	private const string AllyPreviewPrefab = "hideout_boss_fight_preview_ally";

	private const string BanditPreviewPrefab = "hideout_boss_fight_preview_bandit";

	private const string PreviewCameraPrefab = "hideout_boss_fight_camera_preview";

	public const float MaxCameraHeight = 5f;

	public const float MaxCameraWidth = 10f;

	public float InnerRadius = 2.5f;

	public float OuterRadius = 6f;

	public float WalkDistance = 3f;

	public bool ShowPreview;

	private int _perturbSeed;

	private Random _perturbRng = new Random(0);

	private MatrixFrame _previousEntityFrame = MatrixFrame.Identity;

	private GameEntity _previewEntities;

	private List<HideoutBossFightPreviewEntityInfo> _previewAllies = new List<HideoutBossFightPreviewEntityInfo>();

	private List<HideoutBossFightPreviewEntityInfo> _previewBandits = new List<HideoutBossFightPreviewEntityInfo>();

	private HideoutBossFightPreviewEntityInfo _previewBoss = HideoutBossFightPreviewEntityInfo.Invalid;

	private HideoutBossFightPreviewEntityInfo _previewPlayer = HideoutBossFightPreviewEntityInfo.Invalid;

	private GameEntity _previewCamera;

	public int PerturbSeed
	{
		get
		{
			return _perturbSeed;
		}
		private set
		{
			_perturbSeed = value;
			ReSeedPerturbRng();
		}
	}

	public void GetPlayerFrames(out MatrixFrame initialFrame, out MatrixFrame targetFrame, float perturbAmount = 0f)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		ReSeedPerturbRng();
		ComputePerturbedSpawnOffset(perturbAmount, out var perturbVector);
		ComputeSpawnWorldFrame(MathF.PI, InnerRadius, perturbVector - WalkDistance * Vec3.Forward, out initialFrame);
		ComputeSpawnWorldFrame(MathF.PI, InnerRadius, in perturbVector, out targetFrame);
	}

	public void GetBossFrames(out MatrixFrame initialFrame, out MatrixFrame targetFrame, float perturbAmount = 0f)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		ReSeedPerturbRng(1);
		ComputePerturbedSpawnOffset(perturbAmount, out var perturbVector);
		ComputeSpawnWorldFrame(0f, InnerRadius, perturbVector + WalkDistance * Vec3.Forward, out initialFrame);
		ComputeSpawnWorldFrame(0f, InnerRadius, in perturbVector, out targetFrame);
	}

	public void GetAllyFrames(out List<MatrixFrame> initialFrames, out List<MatrixFrame> targetFrames, int agentCount = 10, float agentOffsetAngle = MathF.PI / 20f, float perturbAmount = 0f)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		ReSeedPerturbRng(2);
		initialFrames = ComputeSpawnWorldFrames(agentCount, OuterRadius, (0f - WalkDistance) * Vec3.Forward, MathF.PI, agentOffsetAngle, perturbAmount).ToList();
		ReSeedPerturbRng(2);
		targetFrames = ComputeSpawnWorldFrames(agentCount, OuterRadius, Vec3.Zero, MathF.PI, agentOffsetAngle, perturbAmount).ToList();
	}

	public void GetBanditFrames(out List<MatrixFrame> initialFrames, out List<MatrixFrame> targetFrames, int agentCount = 10, float agentOffsetAngle = MathF.PI / 20f, float perturbAmount = 0f)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		ReSeedPerturbRng(3);
		initialFrames = ComputeSpawnWorldFrames(agentCount, OuterRadius, WalkDistance * Vec3.Forward, 0f, agentOffsetAngle, perturbAmount).ToList();
		ReSeedPerturbRng(3);
		targetFrames = ComputeSpawnWorldFrames(agentCount, OuterRadius, Vec3.Zero, 0f, agentOffsetAngle, perturbAmount).ToList();
	}

	public void GetAlliesInitialFrame(out MatrixFrame frame)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		ComputeSpawnWorldFrame(MathF.PI, OuterRadius, (0f - WalkDistance) * Vec3.Forward, out frame);
	}

	public void GetBanditsInitialFrame(out MatrixFrame frame)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		ComputeSpawnWorldFrame(0f, OuterRadius, WalkDistance * Vec3.Forward, out frame);
	}

	public bool IsWorldPointInsideCameraVolume(in Vec3 worldPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		return IsLocalPointInsideCameraVolume(((MatrixFrame)(ref globalFrame)).TransformToLocal(ref worldPoint));
	}

	public bool ClampWorldPointToCameraVolume(in Vec3 worldPoint, out Vec3 clampedPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 localPoint = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref worldPoint);
		bool num = IsLocalPointInsideCameraVolume(in localPoint);
		if (num)
		{
			clampedPoint = worldPoint;
			return num;
		}
		float num2 = 5f;
		float num3 = OuterRadius + WalkDistance;
		localPoint.x = MathF.Clamp(localPoint.x, 0f - num2, num2);
		localPoint.y = MathF.Clamp(localPoint.y, 0f - num3, num3);
		localPoint.z = MathF.Clamp(localPoint.z, 0f, 5f);
		clampedPoint = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref localPoint);
		return num;
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		if (variableName == "ShowPreview")
		{
			UpdatePreview();
			TogglePreviewVisibility(ShowPreview);
		}
		else if (ShowPreview && (variableName == "InnerRadius" || variableName == "OuterRadius" || variableName == "WalkDistance"))
		{
			UpdatePreview();
		}
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorTick(dt);
		if (ShowPreview)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame frame = ((WeakGameEntity)(ref gameEntity)).GetFrame();
			if (!((Vec3)(ref _previousEntityFrame.origin)).NearlyEquals(ref frame.origin, 1E-05f) || !((Mat3)(ref _previousEntityFrame.rotation)).NearlyEquals(ref frame.rotation, 1E-05f))
			{
				_previousEntityFrame = frame;
				UpdatePreview();
			}
		}
	}

	protected override void OnRemoved(int removeReason)
	{
		((ScriptComponentBehavior)this).OnRemoved(removeReason);
		RemovePreview();
	}

	private void UpdatePreview()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		if (_previewEntities == (GameEntity)null)
		{
			GeneratePreview();
		}
		GameEntity previewEntities = _previewEntities;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		previewEntities.SetGlobalFrame(ref globalFrame, true);
		MatrixFrame initialFrame = MatrixFrame.Identity;
		MatrixFrame targetFrame = MatrixFrame.Identity;
		GetPlayerFrames(out initialFrame, out targetFrame, 0.25f);
		_previewPlayer.InitialEntity.SetGlobalFrame(ref initialFrame, true);
		_previewPlayer.TargetEntity.SetGlobalFrame(ref targetFrame, true);
		GetAllyFrames(out var initialFrames, out var targetFrames, 10, MathF.PI / 20f, 0.25f);
		int num = 0;
		foreach (HideoutBossFightPreviewEntityInfo previewAlly in _previewAllies)
		{
			GameEntity initialEntity = previewAlly.InitialEntity;
			globalFrame = initialFrames[num];
			initialEntity.SetGlobalFrame(ref globalFrame, true);
			GameEntity targetEntity = previewAlly.TargetEntity;
			globalFrame = targetFrames[num];
			targetEntity.SetGlobalFrame(ref globalFrame, true);
			num++;
		}
		GetBossFrames(out initialFrame, out targetFrame, 0.25f);
		_previewBoss.InitialEntity.SetGlobalFrame(ref initialFrame, true);
		_previewBoss.TargetEntity.SetGlobalFrame(ref targetFrame, true);
		GetBanditFrames(out var initialFrames2, out var targetFrames2, 10, MathF.PI / 20f, 0.25f);
		int num2 = 0;
		foreach (HideoutBossFightPreviewEntityInfo previewBandit in _previewBandits)
		{
			GameEntity initialEntity2 = previewBandit.InitialEntity;
			globalFrame = initialFrames2[num2];
			initialEntity2.SetGlobalFrame(ref globalFrame, true);
			GameEntity targetEntity2 = previewBandit.TargetEntity;
			globalFrame = targetFrames2[num2];
			targetEntity2.SetGlobalFrame(ref globalFrame, true);
			num2++;
		}
		MatrixFrame frame = _previewCamera.GetFrame();
		Vec3 scaleVector = ((Mat3)(ref frame.rotation)).GetScaleVector();
		Vec3 val = Vec3.Forward * (OuterRadius + WalkDistance) + Vec3.Side * 5f + Vec3.Up * 5f;
		Vec3 val2 = default(Vec3);
		((Vec3)(ref val2))._002Ector(val.x / scaleVector.x, val.y / scaleVector.y, val.z / scaleVector.z, -1f);
		((Mat3)(ref frame.rotation)).ApplyScaleLocal(ref val2);
		_previewCamera.SetFrame(ref frame, true);
	}

	private void GeneratePreview()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Scene scene = ((WeakGameEntity)(ref gameEntity)).Scene;
		_previewEntities = GameEntity.CreateEmpty(scene, false, true, true);
		GameEntity previewEntities = _previewEntities;
		previewEntities.EntityFlags = (EntityFlags)(previewEntities.EntityFlags | 0x20000);
		MatrixFrame identity = MatrixFrame.Identity;
		_previewEntities.SetFrame(ref identity, true);
		MatrixFrame globalFrame = _previewEntities.GetGlobalFrame();
		GameEntity val = GameEntity.Instantiate(scene, "hideout_boss_fight_preview_boss", globalFrame, true, "");
		_previewEntities.AddChild(val, false);
		ReadPrefabEntity(val, out var initialEntity, out var targetEntity);
		_previewBoss = new HideoutBossFightPreviewEntityInfo(val, initialEntity, targetEntity);
		GameEntity val2 = GameEntity.Instantiate(scene, "hideout_boss_fight_preview_player", globalFrame, true, "");
		_previewEntities.AddChild(val2, false);
		ReadPrefabEntity(val2, out var initialEntity2, out var targetEntity2);
		_previewPlayer = new HideoutBossFightPreviewEntityInfo(val2, initialEntity2, targetEntity2);
		for (int i = 0; i < 10; i++)
		{
			GameEntity val3 = GameEntity.Instantiate(scene, "hideout_boss_fight_preview_ally", globalFrame, true, "");
			_previewEntities.AddChild(val3, false);
			ReadPrefabEntity(val3, out var initialEntity3, out var targetEntity3);
			_previewAllies.Add(new HideoutBossFightPreviewEntityInfo(val3, initialEntity3, targetEntity3));
		}
		for (int j = 0; j < 10; j++)
		{
			GameEntity val4 = GameEntity.Instantiate(scene, "hideout_boss_fight_preview_bandit", globalFrame, true, "");
			_previewEntities.AddChild(val4, false);
			ReadPrefabEntity(val4, out var initialEntity4, out var targetEntity4);
			_previewBandits.Add(new HideoutBossFightPreviewEntityInfo(val4, initialEntity4, targetEntity4));
		}
		_previewCamera = GameEntity.Instantiate(scene, "hideout_boss_fight_camera_preview", globalFrame, true, "");
		_previewEntities.AddChild(_previewCamera, false);
	}

	private void RemovePreview()
	{
		if (_previewEntities != (GameEntity)null)
		{
			_previewEntities.Remove(90);
		}
	}

	private void TogglePreviewVisibility(bool value)
	{
		if (_previewEntities != (GameEntity)null)
		{
			_previewEntities.SetVisibilityExcludeParents(value);
		}
	}

	private void ReadPrefabEntity(GameEntity entity, out GameEntity initialEntity, out GameEntity targetEntity)
	{
		GameEntity firstChildEntityWithTag = entity.GetFirstChildEntityWithTag("initial_frame");
		if (firstChildEntityWithTag == (GameEntity)null)
		{
			Debug.FailedAssert("Prefab entity " + entity.Name + " is not a spawn prefab with an initial frame entity", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Objects\\Cinematics\\HideoutBossFightBehavior.cs", "ReadPrefabEntity", 389);
		}
		GameEntity firstChildEntityWithTag2 = entity.GetFirstChildEntityWithTag("target_frame");
		if (firstChildEntityWithTag2 == (GameEntity)null)
		{
			Debug.FailedAssert("Prefab entity " + entity.Name + " is not a spawn prefab with an target frame entity", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Objects\\Cinematics\\HideoutBossFightBehavior.cs", "ReadPrefabEntity", 395);
		}
		initialEntity = firstChildEntityWithTag;
		targetEntity = firstChildEntityWithTag2;
	}

	private void FindRadialPlacementFrame(float angle, float radius, out MatrixFrame frame)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		float num2 = default(float);
		MathF.SinCos(angle, ref num, ref num2);
		Vec3 val = num2 * Vec3.Forward + num * Vec3.Side;
		Vec3 val2 = radius * val;
		Vec3 val3 = ((num2 > 0f) ? (-1f) : 1f) * Vec3.Forward;
		Mat3 val4 = Mat3.CreateMat3WithForward(ref val3);
		frame = new MatrixFrame(ref val4, ref val2);
	}

	private void SnapOnClosestCollider(ref MatrixFrame frameWs)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Scene scene = ((WeakGameEntity)(ref gameEntity)).Scene;
		Vec3 origin = frameWs.origin;
		origin.z += 5f;
		Vec3 val = origin;
		float num = 500f;
		val.z -= num;
		float num2 = default(float);
		if (scene.RayCastForClosestEntityOrTerrain(origin, val, ref num2, 0.01f, (BodyFlags)79617))
		{
			frameWs.origin.z = origin.z - num2;
		}
	}

	private void ReSeedPerturbRng(int seedOffset = 0)
	{
		_perturbRng = new Random(_perturbSeed + seedOffset);
	}

	private void ComputeSpawnWorldFrame(float localAngle, float localRadius, in Vec3 localOffset, out MatrixFrame worldFrame)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		FindRadialPlacementFrame(localAngle, localRadius, out var frame);
		ref Vec3 origin = ref frame.origin;
		origin += localOffset;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		worldFrame = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref frame);
		SnapOnClosestCollider(ref worldFrame);
	}

	private IEnumerable<MatrixFrame> ComputeSpawnWorldFrames(int spawnCount, float localRadius, Vec3 localOffset, float localBaseAngle, float localOffsetAngle, float localPerturbAmount = 0f)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		float[] localPlacementAngles = new float[2]
		{
			localBaseAngle + localOffsetAngle / 2f,
			localBaseAngle - localOffsetAngle / 2f
		};
		int angleIndex = 0;
		MatrixFrame worldFrame = MatrixFrame.Identity;
		Vec3 perturbVector = Vec3.Zero;
		for (int i = 0; i < spawnCount; i++)
		{
			ComputePerturbedSpawnOffset(localPerturbAmount, out perturbVector);
			ComputeSpawnWorldFrame(localPlacementAngles[angleIndex], localRadius, perturbVector + localOffset, out worldFrame);
			yield return worldFrame;
			localPlacementAngles[angleIndex] += (float)((angleIndex == 0) ? 1 : (-1)) * localOffsetAngle;
			angleIndex = (angleIndex + 1) % 2;
		}
	}

	private void ComputePerturbedSpawnOffset(float perturbAmount, out Vec3 perturbVector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		perturbVector = Vec3.Zero;
		perturbAmount = MathF.Abs(perturbAmount);
		if (perturbAmount > 1E-05f)
		{
			float num = default(float);
			float num2 = default(float);
			MathF.SinCos(MathF.PI * 2f * Extensions.NextFloat(_perturbRng), ref num, ref num2);
			perturbVector.x = perturbAmount * num2;
			perturbVector.y = perturbAmount * num;
		}
	}

	private bool IsLocalPointInsideCameraVolume(in Vec3 localPoint)
	{
		float num = 5f;
		float num2 = OuterRadius + WalkDistance;
		if (localPoint.x >= 0f - num && localPoint.x <= num && localPoint.y >= 0f - num2 && localPoint.y <= num2 && localPoint.z >= 0f)
		{
			return localPoint.z <= 5f;
		}
		return false;
	}
}
