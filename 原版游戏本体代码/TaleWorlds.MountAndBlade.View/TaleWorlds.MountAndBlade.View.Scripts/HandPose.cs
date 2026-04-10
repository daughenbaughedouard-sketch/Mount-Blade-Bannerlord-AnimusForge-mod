using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.Scripts;

public class HandPose : ScriptComponentBehavior
{
	private MBGameManager _editorGameManager;

	private bool _initiliazed;

	private bool _isFinished;

	protected override void OnEditorInit()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		((ScriptComponentBehavior)this).OnEditorInit();
		if (Game.Current == null)
		{
			_editorGameManager = (MBGameManager)new EditorGameManager();
		}
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		if (!_isFinished && _editorGameManager != null)
		{
			_isFinished = !((GameManagerBase)_editorGameManager).DoLoadingForGameManager();
		}
		WeakGameEntity gameEntity;
		if (Game.Current != null && !_initiliazed)
		{
			AnimationSystemData val = MonsterExtensions.FillAnimationSystemData(Game.Current.DefaultMonster, MBActionSet.GetActionSet(Game.Current.DefaultMonster.ActionSetCode), 1f, false);
			GameEntityExtensions.CreateSkeletonWithActionSet(((ScriptComponentBehavior)this).GameEntity, ref val);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).CopyComponentsToSkeleton();
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MBSkeletonExtensions.SetAgentActionChannel(((WeakGameEntity)(ref gameEntity)).Skeleton, 0, ref ActionIndexCache.act_tableau_hand_armor_pose, 0f, -0.2f, true, 0f);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Skeleton skeleton = ((WeakGameEntity)(ref gameEntity)).Skeleton;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			skeleton.TickAnimationsAndForceUpdate(0.01f, ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame(), true);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Skeleton.Freeze(false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Skeleton skeleton2 = ((WeakGameEntity)(ref gameEntity)).Skeleton;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			skeleton2.TickAnimationsAndForceUpdate(0.001f, ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame(), false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Skeleton.SetAnimationParameterAtChannel(0, MBMath.ClampFloat(0f, 0f, 1f));
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Skeleton.SetUptoDate(false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Skeleton.Freeze(true);
			_initiliazed = true;
		}
		if (_initiliazed)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Skeleton.Freeze(false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Skeleton skeleton3 = ((WeakGameEntity)(ref gameEntity)).Skeleton;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			skeleton3.TickAnimationsAndForceUpdate(0.001f, ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame(), false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Skeleton.SetAnimationParameterAtChannel(0, MBMath.ClampFloat(0f, 0f, 1f));
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Skeleton.SetUptoDate(false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Skeleton.Freeze(true);
		}
	}
}
