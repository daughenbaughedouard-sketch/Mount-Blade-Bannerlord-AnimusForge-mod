using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Scripts;

namespace TaleWorlds.MountAndBlade.View;

public class PopupSceneSpawnPoint : ScriptComponentBehavior
{
	public string InitialAction = "";

	public string NegativeAction = "";

	public string InitialFaceAnimCode = "";

	public string PositiveFaceAnimCode = "";

	public string NegativeFaceAnimCode = "";

	public string PositiveAction = "";

	public string LeftHandWieldedItem = "";

	public string RightHandWieldedItem = "";

	public string BannerTagToUseForAddedPrefab = "";

	public bool StartWithRandomProgress;

	public Vec3 AttachedPrefabOffset = Vec3.Zero;

	public string PrefabItem = "";

	public HumanBone PrefabBone = (HumanBone)27;

	private AgentVisuals _mountAgentVisuals;

	private AgentVisuals _humanAgentVisuals;

	private ActionIndexCache _initialStateActionCode = ActionIndexCache.act_none;

	private ActionIndexCache _positiveStateActionCode = ActionIndexCache.act_none;

	private ActionIndexCache _negativeStateActionCode = ActionIndexCache.act_none;

	public CompositeComponent AddedPrefabComponent { get; private set; }

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	public void InitializeWithAgentVisuals(AgentVisuals humanVisuals, AgentVisuals mountVisuals = null)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		_humanAgentVisuals = humanVisuals;
		_mountAgentVisuals = mountVisuals;
		_initialStateActionCode = ActionIndexCache.Create(InitialAction);
		_positiveStateActionCode = ((PositiveAction == "") ? _initialStateActionCode : ActionIndexCache.Create(PositiveAction));
		_negativeStateActionCode = ((NegativeAction == "") ? _initialStateActionCode : ActionIndexCache.Create(NegativeAction));
		bool flag = !string.IsNullOrEmpty(RightHandWieldedItem);
		bool flag2 = !string.IsNullOrEmpty(LeftHandWieldedItem);
		if (flag2 || flag)
		{
			AgentVisualsData copyAgentVisualsData = _humanAgentVisuals.GetCopyAgentVisualsData();
			Equipment val = _humanAgentVisuals.GetEquipment().Clone(false);
			if (flag)
			{
				val[(EquipmentIndex)0] = new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>(RightHandWieldedItem), (ItemModifier)null, (ItemObject)null, false);
			}
			if (flag2)
			{
				val[(EquipmentIndex)1] = new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>(LeftHandWieldedItem), (ItemModifier)null, (ItemObject)null, false);
			}
			int num = ((!flag) ? (-1) : 0);
			int num2 = (flag2 ? 1 : (-1));
			copyAgentVisualsData.RightWieldedItemIndex(num).LeftWieldedItemIndex(num2).Equipment(val);
			_humanAgentVisuals.Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData);
		}
		else
		{
			AgentVisualsData copyAgentVisualsData2 = _humanAgentVisuals.GetCopyAgentVisualsData();
			Equipment val2 = _humanAgentVisuals.GetEquipment().Clone(false);
			copyAgentVisualsData2.RightWieldedItemIndex(-1).LeftWieldedItemIndex(-1).Equipment(val2);
			_humanAgentVisuals.Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData2);
		}
		WeakGameEntity gameEntity;
		if (PrefabItem != "")
		{
			if (!GameEntity.PrefabExists(PrefabItem))
			{
				string[] obj = new string[5] { "Cannot find prefab '", PrefabItem, "' for popup agent '", null, null };
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				obj[3] = ((WeakGameEntity)(ref gameEntity)).Name;
				obj[4] = "'";
				MBDebug.ShowWarning(string.Concat(obj));
			}
			else
			{
				AddedPrefabComponent = _humanAgentVisuals.AddPrefabToAgentVisualBoneByBoneType(PrefabItem, PrefabBone);
				if ((NativeObject)(object)AddedPrefabComponent != (NativeObject)null)
				{
					MatrixFrame frame = AddedPrefabComponent.Frame;
					MatrixFrame identity = MatrixFrame.Identity;
					identity.origin = AttachedPrefabOffset;
					AddedPrefabComponent.Frame = (ref identity) * (ref frame);
				}
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Scene scene = ((WeakGameEntity)(ref gameEntity)).Scene;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (GameEntity item in scene.FindEntitiesWithTag(((WeakGameEntity)(ref gameEntity)).Name))
		{
			item.GetFirstScriptOfType<PopupSceneSequence>()?.InitializeWithAgentVisuals(humanVisuals);
		}
		AgentVisuals humanAgentVisuals = _humanAgentVisuals;
		if (humanAgentVisuals != null)
		{
			MBAgentVisuals visuals = humanAgentVisuals.GetVisuals();
			if (visuals != null)
			{
				visuals.CheckResources(true);
			}
		}
		AgentVisuals mountAgentVisuals = _mountAgentVisuals;
		if (mountAgentVisuals != null)
		{
			MBAgentVisuals visuals2 = mountAgentVisuals.GetVisuals();
			if (visuals2 != null)
			{
				visuals2.CheckResources(true);
			}
		}
		_mountAgentVisuals?.Tick(null, 0.0001f);
		AgentVisuals mountAgentVisuals2 = _mountAgentVisuals;
		if (mountAgentVisuals2 != null)
		{
			GameEntity entity = mountAgentVisuals2.GetEntity();
			if (entity != null)
			{
				Skeleton skeleton = entity.Skeleton;
				if (skeleton != null)
				{
					skeleton.ForceUpdateBoneFrames();
				}
			}
		}
		_humanAgentVisuals?.Tick(_mountAgentVisuals, 0.0001f);
		AgentVisuals humanAgentVisuals2 = _humanAgentVisuals;
		if (humanAgentVisuals2 == null)
		{
			return;
		}
		GameEntity entity2 = humanAgentVisuals2.GetEntity();
		if (entity2 != null)
		{
			Skeleton skeleton2 = entity2.Skeleton;
			if (skeleton2 != null)
			{
				skeleton2.ForceUpdateBoneFrames();
			}
		}
	}

	public void SetInitialState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (_initialStateActionCode != ActionIndexCache.act_none)
		{
			float startProgress = (StartWithRandomProgress ? MBRandom.RandomFloatRanged(0.5f) : 0f);
			_humanAgentVisuals?.SetAction(in _initialStateActionCode, startProgress);
			_mountAgentVisuals?.SetAction(in _initialStateActionCode, startProgress);
		}
		if (!string.IsNullOrEmpty(InitialFaceAnimCode))
		{
			MBSkeletonExtensions.SetFacialAnimation(_humanAgentVisuals.GetVisuals().GetSkeleton(), (FacialAnimChannel)1, InitialFaceAnimCode, false, true);
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Scene scene = ((WeakGameEntity)(ref gameEntity)).Scene;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (GameEntity item in scene.FindEntitiesWithTag(((WeakGameEntity)(ref gameEntity)).Name))
		{
			item.GetFirstScriptOfType<PopupSceneSequence>()?.SetInitialState();
		}
	}

	public void SetPositiveState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (_positiveStateActionCode != ActionIndexCache.act_none)
		{
			_humanAgentVisuals?.SetAction(in _positiveStateActionCode);
			_mountAgentVisuals?.SetAction(in _positiveStateActionCode);
		}
		if (!string.IsNullOrEmpty(PositiveFaceAnimCode))
		{
			MBSkeletonExtensions.SetFacialAnimation(_humanAgentVisuals.GetVisuals().GetSkeleton(), (FacialAnimChannel)1, PositiveFaceAnimCode, false, true);
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Scene scene = ((WeakGameEntity)(ref gameEntity)).Scene;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (GameEntity item in scene.FindEntitiesWithTag(((WeakGameEntity)(ref gameEntity)).Name))
		{
			item.GetFirstScriptOfType<PopupSceneSequence>()?.SetPositiveState();
		}
	}

	public void SetNegativeState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (_negativeStateActionCode != ActionIndexCache.act_none)
		{
			_humanAgentVisuals?.SetAction(in _negativeStateActionCode);
			_mountAgentVisuals?.SetAction(in _negativeStateActionCode);
		}
		if (!string.IsNullOrEmpty(NegativeFaceAnimCode))
		{
			MBSkeletonExtensions.SetFacialAnimation(_humanAgentVisuals.GetVisuals().GetSkeleton(), (FacialAnimChannel)1, NegativeFaceAnimCode, false, true);
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Scene scene = ((WeakGameEntity)(ref gameEntity)).Scene;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (GameEntity item in scene.FindEntitiesWithTag(((WeakGameEntity)(ref gameEntity)).Name))
		{
			item.GetFirstScriptOfType<PopupSceneSequence>()?.SetNegativeState();
		}
	}

	public void Destroy()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		_humanAgentVisuals?.Reset();
		_humanAgentVisuals = null;
		_mountAgentVisuals?.Reset();
		_mountAgentVisuals = null;
		_initialStateActionCode = ActionIndexCache.act_none;
		_positiveStateActionCode = ActionIndexCache.act_none;
		_negativeStateActionCode = ActionIndexCache.act_none;
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(2 | ((ScriptComponentBehavior)this).GetTickRequirement());
	}

	protected override void OnTick(float dt)
	{
		_mountAgentVisuals?.Tick(null, dt);
		AgentVisuals mountAgentVisuals = _mountAgentVisuals;
		if (mountAgentVisuals != null)
		{
			GameEntity entity = mountAgentVisuals.GetEntity();
			if (entity != null)
			{
				Skeleton skeleton = entity.Skeleton;
				if (skeleton != null)
				{
					skeleton.ForceUpdateBoneFrames();
				}
			}
		}
		_humanAgentVisuals?.Tick(_mountAgentVisuals, dt);
		AgentVisuals humanAgentVisuals = _humanAgentVisuals;
		if (humanAgentVisuals == null)
		{
			return;
		}
		GameEntity entity2 = humanAgentVisuals.GetEntity();
		if (entity2 != null)
		{
			Skeleton skeleton2 = entity2.Skeleton;
			if (skeleton2 != null)
			{
				skeleton2.ForceUpdateBoneFrames();
			}
		}
	}
}
