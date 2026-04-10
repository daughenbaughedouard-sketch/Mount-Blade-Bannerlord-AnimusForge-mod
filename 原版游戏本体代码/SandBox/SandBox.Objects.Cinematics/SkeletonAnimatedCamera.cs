using System.Collections.Generic;
using SandBox.Objects.AnimationPoints;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.Cinematics;

public class SkeletonAnimatedCamera : ScriptComponentBehavior
{
	public string SkeletonName = "human_skeleton";

	public int BoneIndex;

	public Vec3 AttachmentOffset = new Vec3(0f, 0f, 0f, -1f);

	public string AnimationName = "";

	public SimpleButton Restart;

	private void CreateVisualizer()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (SkeletonName != "" && AnimationName != "")
		{
			GameEntityExtensions.CreateSimpleSkeleton(((ScriptComponentBehavior)this).GameEntity, SkeletonName);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MBSkeletonExtensions.SetAnimationAtChannel(((WeakGameEntity)(ref gameEntity)).Skeleton, AnimationName, 0, 1f, -1f, 0f);
		}
	}

	protected override void OnInit()
	{
		((ScriptComponentBehavior)this).OnInit();
		CreateVisualizer();
	}

	protected override void OnEditorInit()
	{
		((ScriptComponentBehavior)this).OnEditorInit();
		((ScriptComponentBehavior)this).OnInit();
	}

	protected override void OnTick(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		GameEntity val = ((WeakGameEntity)(ref gameEntity)).Scene.FindEntityWithTag("camera_instance");
		if (val != (GameEntity)null)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if ((NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Skeleton != (NativeObject)null)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MatrixFrame boneEntitialFrame = ((WeakGameEntity)(ref gameEntity)).Skeleton.GetBoneEntitialFrame((sbyte)BoneIndex);
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				boneEntitialFrame = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref boneEntitialFrame);
				MatrixFrame listenerFrame = new MatrixFrame
				{
					rotation = boneEntitialFrame.rotation,
					rotation = 
					{
						u = -boneEntitialFrame.rotation.s
					},
					rotation = 
					{
						f = -boneEntitialFrame.rotation.u
					},
					rotation = 
					{
						s = boneEntitialFrame.rotation.f
					},
					origin = boneEntitialFrame.origin + AttachmentOffset
				};
				val.SetGlobalFrame(ref listenerFrame, true);
				SoundManager.SetListenerFrame(listenerFrame);
			}
		}
	}

	protected override void OnEditorTick(float dt)
	{
		((ScriptComponentBehavior)this).OnTick(dt);
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		if (variableName == "SkeletonName" || variableName == "AnimationName")
		{
			CreateVisualizer();
		}
		if (!(variableName == "Restart"))
		{
			return;
		}
		List<GameEntity> list = new List<GameEntity>();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).Scene.GetAllEntitiesWithScriptComponent<AnimationPoint>(ref list);
		foreach (GameEntity item in list)
		{
			item.GetFirstScriptOfType<AnimationPoint>().RequestResync();
		}
		CreateVisualizer();
	}
}
