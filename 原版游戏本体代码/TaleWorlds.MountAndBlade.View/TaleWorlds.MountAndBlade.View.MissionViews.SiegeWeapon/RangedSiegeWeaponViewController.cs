using TaleWorlds.Engine;

namespace TaleWorlds.MountAndBlade.View.MissionViews.SiegeWeapon;

[DefaultView]
public class RangedSiegeWeaponViewController : MissionView
{
	public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		((MissionBehavior)this).OnObjectUsed(userAgent, usedObject);
		if (!userAgent.IsMainAgent || !(usedObject is StandingPoint))
		{
			return;
		}
		UsableMachine usableMachineFromPoint = GetUsableMachineFromPoint((StandingPoint)(object)((usedObject is StandingPoint) ? usedObject : null));
		if (usableMachineFromPoint is RangedSiegeWeapon)
		{
			RangedSiegeWeapon val = (RangedSiegeWeapon)(object)((usableMachineFromPoint is RangedSiegeWeapon) ? usableMachineFromPoint : null);
			if (((UsableMachine)val).GetComponent<RangedSiegeWeaponView>() == null)
			{
				AddRangedSiegeWeaponView(val);
			}
		}
	}

	private UsableMachine GetUsableMachineFromPoint(StandingPoint standingPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)standingPoint).GameEntity;
		while (((WeakGameEntity)(ref val)).IsValid && !((WeakGameEntity)(ref val)).HasScriptOfType<UsableMachine>())
		{
			val = ((WeakGameEntity)(ref val)).Parent;
		}
		if (((WeakGameEntity)(ref val)).IsValid)
		{
			UsableMachine firstScriptOfType = ((WeakGameEntity)(ref val)).GetFirstScriptOfType<UsableMachine>();
			if (firstScriptOfType != null)
			{
				return firstScriptOfType;
			}
		}
		return null;
	}

	private void AddRangedSiegeWeaponView(RangedSiegeWeapon rangedSiegeWeapon)
	{
		RangedSiegeWeaponView rangedSiegeWeaponView = null;
		rangedSiegeWeaponView = ((rangedSiegeWeapon is Trebuchet) ? new TrebuchetView() : ((rangedSiegeWeapon is Mangonel) ? new MangonelView() : ((!(rangedSiegeWeapon is Ballista)) ? new RangedSiegeWeaponView() : new BallistaView())));
		rangedSiegeWeaponView.Initialize(rangedSiegeWeapon, base.MissionScreen);
		((UsableMachine)rangedSiegeWeapon).AddComponent((UsableMissionObjectComponent)(object)rangedSiegeWeaponView);
	}
}
