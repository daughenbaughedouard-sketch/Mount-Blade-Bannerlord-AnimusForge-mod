using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View;

public class PopupSceneSkeletonAnimationScript : ScriptComponentBehavior
{
	public string SkeletonName = "";

	public int BoneIndex;

	public Vec3 AttachmentOffset = new Vec3(0f, 0f, 0f, -1f);

	public string InitialAnimationClip = "";

	public string PositiveAnimationClip = "";

	public string NegativeAnimationClip = "";

	public string InitialAnimationContinueClip = "";

	public string PositiveAnimationContinueClip = "";

	public string NegativeAnimationContinueClip = "";

	private int _currentState;

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	public void Initialize()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		if (SkeletonName != "")
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (!((NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Skeleton == (NativeObject)null))
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				if (!(((WeakGameEntity)(ref gameEntity)).Skeleton.GetName() != SkeletonName))
				{
					goto IL_005a;
				}
			}
			GameEntityExtensions.CreateSimpleSkeleton(((ScriptComponentBehavior)this).GameEntity, SkeletonName);
			return;
		}
		goto IL_005a;
		IL_005a:
		if (SkeletonName == "")
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if ((NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Skeleton != (NativeObject)null)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).RemoveSkeleton();
			}
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnTick(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).Skeleton.GetAnimationAtChannel(0) == InitialAnimationClip)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Skeleton.GetAnimationParameterAtChannel(0) >= 1f)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MBSkeletonExtensions.SetAnimationAtChannel(((WeakGameEntity)(ref gameEntity)).Skeleton, InitialAnimationContinueClip, 0, 1f, -1f, 0f);
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).Skeleton.GetAnimationAtChannel(0) == PositiveAnimationClip)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Skeleton.GetAnimationParameterAtChannel(0) >= 1f)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MBSkeletonExtensions.SetAnimationAtChannel(((WeakGameEntity)(ref gameEntity)).Skeleton, PositiveAnimationContinueClip, 0, 1f, -1f, 0f);
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).Skeleton.GetAnimationAtChannel(0) == NegativeAnimationClip)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Skeleton.GetAnimationParameterAtChannel(0) >= 1f)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MBSkeletonExtensions.SetAnimationAtChannel(((WeakGameEntity)(ref gameEntity)).Skeleton, NegativeAnimationContinueClip, 0, 1f, -1f, 0f);
			}
		}
	}

	public void SetState(int state)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		string text = state switch
		{
			0 => InitialAnimationClip, 
			1 => PositiveAnimationClip, 
			_ => NegativeAnimationClip, 
		};
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).IsValid)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if ((NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Skeleton != (NativeObject)null && !string.IsNullOrEmpty(text))
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MBSkeletonExtensions.SetAnimationAtChannel(((WeakGameEntity)(ref gameEntity)).Skeleton, text, 0, 1f, -1f, 0f);
			}
		}
		_currentState = state;
	}
}
