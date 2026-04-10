using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

[ScriptComponentParams("ship_visual_only", "")]
internal class BurningNode : ScriptComponentBehavior
{
	private const string LightEntityTag = "light_entity";

	private const string ParticleEntityTag = "particle_entity";

	[EditableScriptComponentVariable(true, "Node Index")]
	private int _nodeIndex = -1;

	private Light _light;

	private ParticleSystem _particle;

	private ParticleSystem _sparkParticle;

	private bool _lightEnabled;

	private bool _sparksEnabled;

	private float _currentFireProgress;

	public Vec2 SailStripLocation { get; private set; }

	public float ExternalFlameMultiplier { get; private set; }

	public float BurningTimer { get; set; }

	public int NodeIndex => _nodeIndex;

	public float CurrentFireProgress
	{
		get
		{
			return _currentFireProgress;
		}
		set
		{
			_currentFireProgress = MathF.Clamp(value, 0f, 1f);
		}
	}

	public BurningNode()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		SailStripLocation = Vec2.Zero;
		ExternalFlameMultiplier = 1f;
		BurningTimer = 0f;
	}

	public void SetSailStripLocation(Vec2 sailStripLocation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SailStripLocation = sailStripLocation;
	}

	public void SetExternalFlameMultiplier(float externalFlameMultiplier)
	{
		ExternalFlameMultiplier = externalFlameMultiplier;
	}

	protected override void OnEditorInit()
	{
		((ScriptComponentBehavior)this).OnEditorInit();
		FetchEntities();
	}

	protected override void OnEditorTick(float dt)
	{
		((ScriptComponentBehavior)this).OnEditorTick(dt);
		FetchEntities();
		TickAux();
	}

	protected override void OnInit()
	{
		((ScriptComponentBehavior)this).OnInit();
		FetchEntities();
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)4;
	}

	protected override void OnTickParallel(float dt)
	{
		TickAux();
	}

	private void FetchEntities()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		_light = null;
		_particle = null;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("light_entity");
		if (firstChildEntityWithTag != (GameEntity)null)
		{
			((WeakGameEntity)(ref firstChildEntityWithTag)).SetVisibilityExcludeParents(true);
			_light = (Light)((WeakGameEntity)(ref firstChildEntityWithTag)).GetComponentAtIndex(0, (ComponentType)1);
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag2 = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("particle_entity");
		if (firstChildEntityWithTag2 != (GameEntity)null)
		{
			((WeakGameEntity)(ref firstChildEntityWithTag2)).SetVisibilityExcludeParents(true);
			_particle = (ParticleSystem)((WeakGameEntity)(ref firstChildEntityWithTag2)).GetComponentAtIndex(0, (ComponentType)4);
		}
	}

	private void TickAux()
	{
		bool flag = _currentFireProgress > 0f;
		if ((NativeObject)(object)_particle != (NativeObject)null)
		{
			_particle.SetEnable(flag);
		}
		if ((NativeObject)(object)_light != (NativeObject)null)
		{
			_light.SetVisibility(flag && _lightEnabled);
		}
		if ((NativeObject)(object)_sparkParticle != (NativeObject)null)
		{
			_sparkParticle.SetEnable(flag && _sparksEnabled);
		}
		if (flag)
		{
			if ((NativeObject)(object)_particle != (NativeObject)null)
			{
				_particle.SetRuntimeEmissionRateMultiplier(_currentFireProgress * ExternalFlameMultiplier);
			}
			if ((NativeObject)(object)_sparkParticle != (NativeObject)null)
			{
				_sparkParticle.SetRuntimeEmissionRateMultiplier(_currentFireProgress * ExternalFlameMultiplier);
			}
		}
	}

	public void EnableSparks()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		_sparksEnabled = true;
		MatrixFrame identity = MatrixFrame.Identity;
		_sparkParticle = ParticleSystem.CreateParticleSystemAttachedToEntity("psys_dripping_flame", ((ScriptComponentBehavior)this).GameEntity, ref identity);
	}

	public void CheckWater()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		float waterLevelAtPosition = ((WeakGameEntity)(ref gameEntity)).GetWaterLevelAtPosition(((Vec3)(ref globalFrame.origin)).AsVec2, true, false);
		if (globalFrame.origin.z < waterLevelAtPosition)
		{
			CurrentFireProgress = 0f;
		}
	}
}
