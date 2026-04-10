using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.ShipActuators;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipUnmannedOar : ScriptComponentBehavior, IShipOarScriptComponent
{
	private GameEntity _oarEntity;

	private MatrixFrame _oarExtractedEntitialFrame;

	private MatrixFrame _oarRetractedEntitialFrame;

	private MissionOar _oar;

	private float _lastIdleTime;

	private DestructableComponent _destructableComponent;

	private BoundingBox _unmannedOarBaseBoundingBox;

	protected override void OnInit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		ShipOarDeck.LoadOarScriptEntity(((ScriptComponentBehavior)this).GameEntity, out var oarEntity, ref _oarExtractedEntitialFrame, ref _oarRetractedEntitialFrame, out var handTargetEntity);
		_oarEntity = (((WeakGameEntity)(ref oarEntity)).IsValid ? GameEntity.CreateFromWeakEntity(oarEntity) : null);
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
		handTargetEntity = ((ScriptComponentBehavior)this).GameEntity;
		_destructableComponent = ((WeakGameEntity)(ref handTargetEntity)).GetFirstScriptOfType<DestructableComponent>();
		handTargetEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref handTargetEntity)).SetHasCustomBoundingBoxValidationSystem(true);
		handTargetEntity = ((ScriptComponentBehavior)this).GameEntity;
		_unmannedOarBaseBoundingBox = ((WeakGameEntity)(ref handTargetEntity)).ComputeBoundingBoxFromLongestHalfDimension(2f);
	}

	public void InitializeOar(MissionOar oar)
	{
		_oar = oar;
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(((ScriptComponentBehavior)this).GetTickRequirement() | 4);
	}

	public void ArrangeOarBoundingBox()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).SetManualLocalBoundingBox(ref _unmannedOarBaseBoundingBox);
		val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Parent;
		((WeakGameEntity)(ref val)).SetBoundingboxDirty();
	}

	protected override void OnBoundingBoxValidate()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		BoundingBox val = ((WeakGameEntity)(ref gameEntity)).ComputeBoundingBoxIncludeChildren();
		((BoundingBox)(ref val)).RelaxWithBoundingBox(_unmannedOarBaseBoundingBox);
		((BoundingBox)(ref val)).RecomputeRadius();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).RelaxLocalBoundingBox(ref val);
	}

	public bool CheckOarMachineFlags(bool editMode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref val)).GetChildren())
		{
			WeakGameEntity current = child;
			if (!Extensions.HasAnyFlag<EntityFlags>(((WeakGameEntity)(ref current)).EntityFlags, (EntityFlags)131072) && !Extensions.HasAnyFlag<EntityFlags>(((WeakGameEntity)(ref current)).EntityFlags, (EntityFlags)4096))
			{
				val = ((ScriptComponentBehavior)this).GameEntity;
				val = ((WeakGameEntity)(ref val)).Root;
				string name = ((WeakGameEntity)(ref val)).Name;
				val = ((ScriptComponentBehavior)this).GameEntity;
				string text = $"Root Entity: {name} {((WeakGameEntity)(ref val)).Name}'s child {((WeakGameEntity)(ref current)).Name} must have Does not Affect Parent's Local Bounding Box flag.";
				if (editMode)
				{
					MBEditor.AddEntityWarning(current, text);
				}
				return false;
			}
		}
		return true;
	}

	public void SetSlowDownPhaseForDuration(float slowDownMultiplier, float slowDownDuration)
	{
		_oar.SetSlowDownPhaseForDuration(slowDownMultiplier, slowDownDuration);
	}

	protected override void OnTickParallel(float dt)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		bool newIsUsed = _oar.OwnerShip.FireHitPoints > 0f && _oar.OwnerShip.Physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Floating && (_destructableComponent == null || !_destructableComponent.IsDestroyed);
		_oar.SetUsed(newIsUsed, -1);
		MissionOar oar = _oar;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame val = oar.ComputeOarEntityFrame(dt, ((WeakGameEntity)(ref gameEntity)).GetLocalFrame(), _oarEntity.GetLocalFrame(), in _oarExtractedEntitialFrame, in _oarRetractedEntitialFrame, _lastIdleTime, forUnmanned: true);
		_oarEntity.SetLocalFrame(ref val, false);
		if (!_oar.IsExtracted)
		{
			_lastIdleTime = Mission.Current.CurrentTime;
		}
	}
}
