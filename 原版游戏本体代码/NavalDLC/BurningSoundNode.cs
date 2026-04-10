using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

internal class BurningSoundNode : ScriptComponentBehavior
{
	private const int MaxNumberOfCachedBurningNodes = 5;

	private const string _soundPath = "event:/mission/ambient/detail/fire/fire_dynamic";

	private const float FireRadius = 5f;

	private const float FireRadiusSq = 25f;

	private List<BurningNode> _burningNodesAttached = new List<BurningNode>();

	private bool _enabled;

	private float _burningSoundEventIntensityParam;

	private SoundEvent _burningSoundEvent;

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)10;
	}

	protected override void OnTick(float dt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (_enabled)
		{
			SoundEvent burningSoundEvent = _burningSoundEvent;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			burningSoundEvent.SetPosition(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			_burningSoundEvent.SetParameter("FireIntensity", _burningSoundEventIntensityParam);
		}
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		if (_enabled)
		{
			SoundEvent burningSoundEvent = _burningSoundEvent;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			burningSoundEvent.SetPosition(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			float num = 0f;
			foreach (BurningNode item in _burningNodesAttached)
			{
				num += item.CurrentFireProgress;
			}
			_burningSoundEventIntensityParam = num;
			SoundEvent burningSoundEvent2 = _burningSoundEvent;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			burningSoundEvent2.SetPosition(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			_burningSoundEvent.SetParameter("FireIntensity", _burningSoundEventIntensityParam);
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).IsSelectedOnEditor();
	}

	protected override void OnTickParallel2(float dt)
	{
		if (!_enabled)
		{
			return;
		}
		float num = 0f;
		foreach (BurningNode item in _burningNodesAttached)
		{
			num += item.CurrentFireProgress;
		}
		_burningSoundEventIntensityParam = num;
	}

	public void AddBurningNode(BurningNode node)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)node).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((Vec3)(ref globalPosition)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GlobalPosition) < 25f)
		{
			_burningNodesAttached.Add(node);
		}
	}

	public void StartFire()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		_enabled = true;
		Mission current = Mission.Current;
		_burningSoundEvent = SoundEvent.CreateEventFromString("event:/mission/ambient/detail/fire/fire_dynamic", (current != null) ? current.Scene : null);
		SoundEvent burningSoundEvent = _burningSoundEvent;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		burningSoundEvent.SetPosition(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
		_burningSoundEvent.Play();
		_burningSoundEvent.SetParameter("FireIntensity", _burningSoundEventIntensityParam);
	}

	public void StopFire()
	{
		_enabled = false;
		_burningSoundEvent.Stop();
		_burningSoundEvent = null;
		_burningNodesAttached.Clear();
	}
}
