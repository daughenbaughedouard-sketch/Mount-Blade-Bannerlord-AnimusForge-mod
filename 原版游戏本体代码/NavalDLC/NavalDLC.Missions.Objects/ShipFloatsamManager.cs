using System;
using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Missions.Objects;

public class ShipFloatsamManager : ScriptComponentBehavior
{
	private enum DebrisType
	{
		Generic,
		Scrape,
		Ramming
	}

	private enum DecalType
	{
		Collision,
		Scrape
	}

	private struct ImpulseRecord
	{
		internal Vec3 AveragePosition;

		internal Vec3 AverageNormal;

		internal float TotalImpulse;

		internal Vec3 Speed;

		internal DebrisType DebrisType;

		internal float InitialSpeedMultiplier;

		internal Vec3 ShipLocalPosition;

		internal Vec3 ShipLocalNormal;

		internal DecalType DecalType;
	}

	private struct ShieldBreakRecord
	{
		internal Vec3 LinearVelocity;

		internal Texture BannerTexture;

		internal MatrixFrame ShipLocalSpawnFrame;

		internal string PrefabName;
	}

	private class ScrapeRecord
	{
		internal ParticleSystem Particle;

		internal float AccumulatedDistance;

		internal Vec3 PreviousPosition;
	}

	private static readonly string[] GenericPrefabNames = new string[3] { "floatable_debris_broken_barrel", "floatable_debris_door", "floatable_debris_barrel_a" };

	private static readonly string[] RammingPrefabNames = new string[7] { "floatable_debris_plank_b", "floatable_debris_plank_e", "floatable_debris_plank_f", "floatable_debris_plank_g", "floatable_debris_plank_h", "floatable_debris_plank_j", "floatable_debris_plank_k" };

	private static readonly string[] ScrapeDebrisPrefabNames = new string[7] { "floatable_debris_plank_b", "floatable_debris_plank_e", "floatable_debris_plank_f", "floatable_debris_plank_g", "floatable_debris_plank_h", "floatable_debris_plank_j", "floatable_debris_plank_k" };

	private static readonly string[] CollisionDecalPrefabNames = new string[3] { "decal_ship_damaged_a", "decal_ship_damaged_b", "decal_ship_damaged_c" };

	private static readonly string[] ScrapeDecalPrefabNames = new string[3] { "decal_ship_damage_02", "decal_ship_damage_03", "decal_ship_damage_04" };

	private const string RudderPrefabName = "floatable_debris_rudder";

	private const string ShieldPrefabName = "floatable_debris_";

	private const string OarPrefabName = "floatable_debris_oar_a";

	private const string MastPrefabName = "floatable_debris_mast";

	private const string BodyMeshTag = "body_mesh";

	private const string BannerTag = "banner_with_faction_color";

	private const int MaxNumberOfPendingImpulseRecords = 10;

	private const float DebrisBreakImpulseThreshold = 150000f;

	private const int MaxDecalCount = 30;

	private Dictionary<WeakGameEntity, ScrapeRecord> _scrapeRecords = new Dictionary<WeakGameEntity, ScrapeRecord>();

	private GameEntity _identityFrameParticleParent;

	private int _scrapeParticleIndex = -1;

	private int _collisionHitParticleIndex = -1;

	private int _midCollisionHitParticleIndex = -1;

	private int _bigCollisionHitParticleIndex = -1;

	private readonly MBFastRandom _randomGenerator = new MBFastRandom();

	private ImpulseRecord[] _impulseRecordsToProcess = new ImpulseRecord[10];

	private ShieldBreakRecord[] _shieldBreakRecords = new ShieldBreakRecord[10];

	private uint _shipColor;

	private int _numberOfPendingImpulseRecords;

	private int _numberOfPendingShieldBreakRecords;

	private uint _shipDecalColor;

	private bool _sinkingFloatsamSpawned;

	private List<GameEntity> _collisionDecals;

	private string _shieldName;

	private NavalFloatsamLogic _floatsamMissionLogic;

	private GameEntity _bodyEntity;

	private MissionShip _ownMissionShipCached;

	private bool _floatsamSystemEnabled;

	internal ShipFloatsamManager()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Color white = Colors.White;
		_shipColor = ((Color)(ref white)).ToUnsignedInteger();
		white = Colors.White;
		_shipDecalColor = ((Color)(ref white)).ToUnsignedInteger();
		_collisionDecals = new List<GameEntity>();
		_shieldName = "";
		((ScriptComponentBehavior)this)._002Ector();
	}

	protected override void OnInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_identityFrameParticleParent = GameEntity.CreateEmpty(((WeakGameEntity)(ref gameEntity)).Scene, false, false, false);
		GameEntity identityFrameParticleParent = _identityFrameParticleParent;
		identityFrameParticleParent.EntityFlags = (EntityFlags)(identityFrameParticleParent.EntityFlags | 0x20000);
		_scrapeParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_game_ship_scrape_emit_on_move");
		_collisionHitParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_game_ship_collision");
		_midCollisionHitParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_naval_ship_hit_mid");
		_bigCollisionHitParticleIndex = ParticleSystemManager.GetRuntimeIdByName("psys_naval_ship_hit_large");
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTagRecursive = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTagRecursive("body_mesh");
		if (firstChildEntityWithTagRecursive != (GameEntity)null)
		{
			_bodyEntity = GameEntity.CreateFromWeakEntity(firstChildEntityWithTagRecursive);
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		ColorAssigner firstScriptOfType = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<ColorAssigner>();
		if (firstScriptOfType != null)
		{
			Color val = firstScriptOfType.ShipColor;
			_shipColor = ((Color)(ref val)).ToUnsignedInteger();
			val = firstScriptOfType.RamDebrisColor;
			_shipDecalColor = ((Color)(ref val)).ToUnsignedInteger();
		}
		_floatsamMissionLogic = Mission.Current.GetMissionBehavior<NavalFloatsamLogic>();
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).GetChildrenRecursive(ref list);
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current = item;
			ShipShieldComponent firstScriptOfType2 = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<ShipShieldComponent>();
			if (firstScriptOfType2 != null)
			{
				((DestructableComponent)firstScriptOfType2).OnDestroyed += new OnHitTakenAndDestroyedDelegate(OnShieldDestroyed);
				_shieldName = ((WeakGameEntity)(ref current)).Name;
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_ownMissionShipCached = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<MissionShip>();
		if (_ownMissionShipCached != null)
		{
			NavalShipsLogic missionBehavior = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
			if (missionBehavior != null)
			{
				missionBehavior.ShipHitEvent += OnShipHit;
				missionBehavior.ShipRammingEvent += OnShipRamming;
			}
		}
	}

	protected override void OnTick(float dt)
	{
		if (_floatsamSystemEnabled)
		{
			CheckSinking();
			ProcessImpulseEffects();
			ProcessShieldBreakRecords();
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnPhysicsCollision(ref PhysicsContact contact, WeakGameEntity entity0, WeakGameEntity entity1)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Invalid comparison between Unknown and I4
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_054a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		if (!((WeakGameEntity)(ref entity1)).HasScriptComponent(MissionShip.MissionShipScriptNameHash) || !_floatsamSystemEnabled)
		{
			return;
		}
		MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref entity0)).GetBodyWorldTransform();
		bool flag = true;
		Vec3 val = Vec3.Zero;
		Vec3 val2 = Vec3.Zero;
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < contact.NumberOfContactPairs; i++)
		{
			PhysicsContactPair val3 = ((PhysicsContact)(ref contact))[i];
			for (int j = 0; j < val3.NumberOfContacts; j++)
			{
				PhysicsContactInfo val4 = ((PhysicsContactPair)(ref val3))[j];
				val += val4.Position;
				num += ((Vec3)(ref val4.Impulse)).Length;
				val2 += val4.Normal;
				_ = Colors.White;
				if ((int)val3.ContactEventType == 0)
				{
					flag = false;
				}
				else if ((int)val3.ContactEventType == 1)
				{
					flag = false;
				}
				num2 += 1f;
			}
		}
		if (num2 > 0f)
		{
			val /= num2;
			val2 /= num2;
			((Vec3)(ref val2)).Normalize();
			val2 *= -1f;
		}
		WeakGameEntity gameEntity;
		if (_scrapeRecords.TryGetValue(entity1, out var value))
		{
			if (flag || num2 == 0f)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).RemoveComponent((GameEntityComponent)(object)value.Particle);
				_scrapeRecords.Remove(entity1);
				return;
			}
			MatrixFrame identity = MatrixFrame.Identity;
			identity.rotation.u = Vec3.Up;
			identity.rotation.s = val2;
			identity.rotation.f = -((Vec3)(ref identity.rotation.s)).CrossProductWithUp();
			identity.rotation.s = Vec3.CrossProduct(identity.rotation.f, identity.rotation.u);
			identity.origin = val;
			value.AccumulatedDistance += ((Vec3)(ref value.PreviousPosition)).Distance(val);
			value.PreviousPosition = val;
			value.Particle.SetLocalFrame(ref identity);
			if (!(value.AccumulatedDistance > 2.5f))
			{
				return;
			}
			value.AccumulatedDistance = 0f;
			if (_numberOfPendingImpulseRecords < 10)
			{
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].AveragePosition = val;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].AverageNormal = val2;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].TotalImpulse = 150000f;
				Vec3 speed = Vec3.Zero;
				if (GameEntityPhysicsExtensions.HasDynamicRigidBody(entity0))
				{
					speed = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)this).GameEntity, val);
				}
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].Speed = speed;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].DebrisType = DebrisType.Scrape;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].DecalType = DecalType.Scrape;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].InitialSpeedMultiplier = 0.25f;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].ShipLocalPosition = ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref val);
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].ShipLocalNormal = ((Mat3)(ref bodyWorldTransform.rotation)).TransformToLocal(ref val2);
				_numberOfPendingImpulseRecords++;
			}
		}
		else if (num2 > 0f)
		{
			ScrapeRecord scrapeRecord = new ScrapeRecord();
			MatrixFrame identity2 = MatrixFrame.Identity;
			identity2.rotation.u = Vec3.Up;
			identity2.rotation.s = val2;
			identity2.rotation.f = -((Vec3)(ref identity2.rotation.s)).CrossProductWithUp();
			identity2.rotation.s = Vec3.CrossProduct(identity2.rotation.f, identity2.rotation.u);
			identity2.origin = val;
			scrapeRecord.Particle = ParticleSystem.CreateParticleSystemAttachedToEntity(_scrapeParticleIndex, _identityFrameParticleParent, ref identity2);
			scrapeRecord.PreviousPosition = val;
			_scrapeRecords.Add(entity1, scrapeRecord);
			if (num > 15000f)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).Scene.CreateBurstParticle(_collisionHitParticleIndex, identity2);
			}
			Vec3 val5 = Vec3.Zero;
			if (GameEntityPhysicsExtensions.HasDynamicRigidBody(entity0))
			{
				val5 = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)this).GameEntity, val);
			}
			Vec3 val6 = Vec3.Zero;
			if (GameEntityPhysicsExtensions.HasDynamicRigidBody(entity1))
			{
				val6 = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)this).GameEntity, val);
			}
			if (_numberOfPendingImpulseRecords < 10)
			{
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].AveragePosition = val;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].AverageNormal = val2;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].TotalImpulse = num;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].Speed = val5 - val6;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].DebrisType = DebrisType.Scrape;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].DecalType = DecalType.Collision;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].InitialSpeedMultiplier = 1f;
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].ShipLocalPosition = ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref val);
				_impulseRecordsToProcess[_numberOfPendingImpulseRecords].ShipLocalNormal = ((Mat3)(ref bodyWorldTransform.rotation)).TransformToLocal(ref val2);
				_numberOfPendingImpulseRecords++;
			}
		}
	}

	private void ProcessImpulseEffects()
	{
		while (_numberOfPendingImpulseRecords > 0)
		{
			int num = _numberOfPendingImpulseRecords - 1;
			ProcessImpactEffect(_impulseRecordsToProcess[num]);
			_numberOfPendingImpulseRecords--;
		}
	}

	private void ProcessShieldBreakRecords()
	{
		while (_numberOfPendingShieldBreakRecords > 0)
		{
			int num = _numberOfPendingShieldBreakRecords - 1;
			SpawnBrokenShield(_shieldBreakRecords[num]);
			_numberOfPendingShieldBreakRecords--;
		}
	}

	private void SpawnBrokenShield(ShieldBreakRecord record)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		GameEntity val = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, record.PrefabName, true, true, "");
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		MatrixFrame val2 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref record.ShipLocalSpawnFrame);
		Vec3 val3 = ComputeRandomPositionOffset(in _randomGenerator, 0.75f);
		ref Vec3 origin = ref val2.origin;
		origin += val3;
		val.SetFrame(ref val2, true);
		GameEntityPhysicsExtensions.SetLinearVelocity(val, record.LinearVelocity);
		SetRandomAngularVelocityToEntity(val);
		if ((NativeObject)(object)record.BannerTexture != (NativeObject)null)
		{
			foreach (Mesh item in val.GetFirstChildEntityWithTag("shield_mesh_entity").GetAllMeshesWithTag("banner_with_faction_color"))
			{
				Material val4 = item.GetMaterial().CreateCopy();
				val4.SetTexture((MBTextureType)1, record.BannerTexture);
				uint num = (uint)val4.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
				ulong shaderFlags = val4.GetShaderFlags();
				val4.SetShaderFlags(shaderFlags | num);
				item.SetMaterial(val4);
			}
		}
		if (_floatsamMissionLogic != null)
		{
			_floatsamMissionLogic.RegisterFloatsamInstance(val);
		}
	}

	private static Vec3 ComputeRandomPositionOffset(in MBFastRandom randGenerator, float halfRange)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		return new Vec3
		{
			x = randGenerator.NextFloatRanged(0f - halfRange, halfRange),
			y = randGenerator.NextFloatRanged(0f - halfRange, halfRange),
			z = randGenerator.NextFloatRanged(0f - halfRange, halfRange)
		};
	}

	private void ProcessImpactEffect(ImpulseRecord record)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		int num = ((record.DebrisType == DebrisType.Ramming) ? 10 : 7);
		int num2 = MathF.Min((int)(record.TotalImpulse / 150000f), num);
		WeakGameEntity gameEntity;
		Vec3 val5;
		for (int i = 0; i < num2; i++)
		{
			string randomDebrisPrefab = GetRandomDebrisPrefab(record.DebrisType);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			GameEntity val = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, randomDebrisPrefab, true, true, "");
			MatrixFrame identity = MatrixFrame.Identity;
			((Mat3)(ref identity.rotation)).RotateAboutSide(_randomGenerator.NextFloatRanged(0f, MathF.PI * 2f));
			((Mat3)(ref identity.rotation)).RotateAboutForward(_randomGenerator.NextFloatRanged(0f, MathF.PI * 2f));
			((Mat3)(ref identity.rotation)).RotateAboutUp(_randomGenerator.NextFloatRanged(0f, MathF.PI * 2f));
			((Mat3)(ref identity.rotation)).Orthonormalize();
			Vec3 val2 = ComputeRandomPositionOffset(in _randomGenerator, 0.75f);
			identity.origin = record.AveragePosition + val2;
			val.SetFrame(ref identity, true);
			Vec3 val3 = record.TotalImpulse * record.AverageNormal;
			float num3 = (0.27f + _randomGenerator.NextFloatRanged(0f, 0.3f)) * 0.032f;
			Vec3 val4 = record.Speed + val3 / GameEntityPhysicsExtensions.GetMass(val);
			float num4 = ((Vec3)(ref val4)).Normalize();
			val4 = ((Vec3)(ref val4)).RotateAboutAnArbitraryVector(record.AverageNormal, _randomGenerator.NextFloatRanged(-MathF.PI / 2f, MathF.PI / 2f));
			num4 *= num3;
			num4 = MathF.Min(num4, 30f);
			val5 = val4 + Vec3.Up * 0.75f;
			val4 = ((Vec3)(ref val5)).NormalizedCopy() * num4;
			GameEntityPhysicsExtensions.SetLinearVelocity(val, val4);
			foreach (Mesh item in val.GetAllMeshesWithTag("auto_factor_color"))
			{
				item.Color = _shipColor;
			}
			SetRandomAngularVelocityToEntity(val);
			if (_floatsamMissionLogic != null)
			{
				_floatsamMissionLogic.RegisterFloatsamInstance(val);
			}
		}
		if (_collisionDecals.Count >= 30)
		{
			return;
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 origin = record.ShipLocalPosition;
		Vec3 u = record.ShipLocalNormal;
		if (_bodyEntity != (GameEntity)null)
		{
			float num5 = 2.5f;
			val5 = ((Mat3)(ref globalFrame.rotation)).TransformToParent(ref record.ShipLocalNormal);
			Vec3 val6 = -((Vec3)(ref val5)).NormalizedCopy();
			Vec3 val7 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref record.ShipLocalPosition) - val6 * num5;
			Vec3 zero = Vec3.Zero;
			float num6 = 0f;
			if (_bodyEntity.RayHitEntityWithNormal(val7, val6, num5, ref zero, ref num6))
			{
				val5 = val7 + val6 * num6;
				origin = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref val5);
				val5 = ((Mat3)(ref globalFrame.rotation)).TransformToLocal(ref zero);
				u = ((Vec3)(ref val5)).NormalizedCopy();
			}
		}
		MatrixFrame identity2 = MatrixFrame.Identity;
		identity2.origin = origin;
		identity2.rotation.u = u;
		identity2.rotation.f = Vec3.Up;
		identity2.rotation.s = Vec3.CrossProduct(identity2.rotation.u, identity2.rotation.s);
		((Vec3)(ref identity2.rotation.f)).Normalize();
		identity2.rotation.s = Vec3.CrossProduct(identity2.rotation.f, identity2.rotation.u);
		if (record.DecalType == DecalType.Scrape)
		{
			float num7 = _randomGenerator.NextFloatRanged(1.75f, 2.75f);
			float num8 = _randomGenerator.NextFloatRanged(1.25f, 1.75f);
			ref Mat3 rotation = ref identity2.rotation;
			val5 = new Vec3(num7, num8, 0.2f, -1f);
			((Mat3)(ref rotation)).ApplyScaleLocal(ref val5);
		}
		else if (record.DecalType == DecalType.Collision)
		{
			float num9 = _randomGenerator.NextFloatRanged(1.55f, 2.55f);
			ref Mat3 rotation2 = ref identity2.rotation;
			val5 = new Vec3(num9, 1f, 0.2f, -1f);
			((Mat3)(ref rotation2)).ApplyScaleLocal(ref val5);
		}
		string text = "";
		if (record.DecalType == DecalType.Collision)
		{
			text = GetRandomCollisionDecalPrefab();
		}
		else if (record.DecalType == DecalType.Scrape)
		{
			text = GetRandomScrapeDecalPrefab();
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		GameEntity val8 = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, text, MatrixFrame.Identity, true, "");
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).AddChild(val8.WeakEntity, false);
		val8.SetFrame(ref identity2, true);
		val8.SetFactorColor(_shipDecalColor);
		_collisionDecals.Add(val8);
	}

	private string GetRandomDebrisPrefab(DebrisType type)
	{
		switch (type)
		{
		case DebrisType.Generic:
		{
			int num3 = _randomGenerator.Next(GenericPrefabNames.Length);
			return GenericPrefabNames[num3];
		}
		case DebrisType.Scrape:
		{
			int num2 = _randomGenerator.Next(ScrapeDebrisPrefabNames.Length);
			return ScrapeDebrisPrefabNames[num2];
		}
		case DebrisType.Ramming:
		{
			int num = _randomGenerator.Next(RammingPrefabNames.Length);
			return RammingPrefabNames[num];
		}
		default:
			return "";
		}
	}

	private string GetRandomCollisionDecalPrefab()
	{
		int num = _randomGenerator.Next(CollisionDecalPrefabNames.Length);
		return CollisionDecalPrefabNames[num];
	}

	private string GetRandomScrapeDecalPrefab()
	{
		int num = _randomGenerator.Next(ScrapeDecalPrefabNames.Length);
		return ScrapeDecalPrefabNames[num];
	}

	private void SetRandomAngularVelocityToEntity(GameEntity entity)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.8f;
		GameEntityPhysicsExtensions.SetAngularVelocity(entity, new Vec3(_randomGenerator.NextFloatRanged(0f - num, num), _randomGenerator.NextFloatRanged(0f - num, num), _randomGenerator.NextFloatRanged(0f - num, num), -1f));
	}

	private void CheckSinking()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		if (_sinkingFloatsamSpawned || _ownMissionShipCached.Physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Floating)
		{
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		BoundingBox physicsBoundingBoxWithoutChildren = _ownMissionShipCached.Physics.PhysicsBoundingBoxWithoutChildren;
		float num = (physicsBoundingBoxWithoutChildren.max.z - physicsBoundingBoxWithoutChildren.min.z) * 0.75f;
		float num2 = globalPosition.z + num;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (!(num2 < ((WeakGameEntity)(ref gameEntity)).GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, true, false)))
		{
			return;
		}
		Vec3 min = physicsBoundingBoxWithoutChildren.min;
		Vec3 max = physicsBoundingBoxWithoutChildren.max;
		max.z = min.z;
		Vec3 val = max - min;
		float num3 = MathF.Max(Vec2.DotProduct(((Vec3)(ref val)).AsVec2, ((Vec3)(ref val)).AsVec2) / 1000f, 1f);
		_sinkingFloatsamSpawned = true;
		int num4 = (int)((float)_randomGenerator.Next(7, 10) * num3);
		for (int i = 0; i < num4; i++)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			GameEntity val2 = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, "floatable_debris_oar_a", true, true, "");
			if (val2 != (GameEntity)null)
			{
				Vec3 val3 = min + new Vec3(val.x * _randomGenerator.NextFloat(), val.y * _randomGenerator.NextFloat(), 0f, -1f);
				MatrixFrame identity = MatrixFrame.Identity;
				identity.origin = globalPosition + val3;
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				float waterLevelAtPosition = ((WeakGameEntity)(ref gameEntity)).GetWaterLevelAtPosition(((Vec3)(ref identity.origin)).AsVec2, true, false);
				identity.origin.z = waterLevelAtPosition - 1.5f * _randomGenerator.NextFloatRanged(1f, 4.5f);
				val2.SetFrame(ref identity, true);
				val2.SetFactorColor(_shipColor);
				SetRandomAngularVelocityToEntity(val2);
				if (_floatsamMissionLogic != null)
				{
					_floatsamMissionLogic.RegisterFloatsamInstance(val2);
				}
			}
		}
		Vec3 val4 = min + new Vec3(val.x * _randomGenerator.NextFloat(), val.y * _randomGenerator.NextFloat(), 0f, -1f);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		GameEntity val5 = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, "floatable_debris_rudder", true, true, "");
		MatrixFrame identity2 = MatrixFrame.Identity;
		identity2.origin = globalPosition + val4;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		float waterLevelAtPosition2 = ((WeakGameEntity)(ref gameEntity)).GetWaterLevelAtPosition(((Vec3)(ref identity2.origin)).AsVec2, true, false);
		identity2.origin.z = waterLevelAtPosition2 - 1.5f * _randomGenerator.NextFloatRanged(1f, 4.5f);
		val5.SetFrame(ref identity2, true);
		val5.SetFactorColor(_shipColor);
		SetRandomAngularVelocityToEntity(val5);
		if (_floatsamMissionLogic != null)
		{
			_floatsamMissionLogic.RegisterFloatsamInstance(val5);
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		GameEntity val6 = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, "floatable_debris_mast", true, true, "");
		if (val6 != (GameEntity)null)
		{
			Vec3 val7 = min + new Vec3(val.x * _randomGenerator.NextFloat(), val.y * _randomGenerator.NextFloat(), 0f, -1f);
			MatrixFrame identity3 = MatrixFrame.Identity;
			identity3.origin = globalPosition + val7;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			float waterLevelAtPosition3 = ((WeakGameEntity)(ref gameEntity)).GetWaterLevelAtPosition(((Vec3)(ref identity3.origin)).AsVec2, true, false);
			identity3.origin.z = waterLevelAtPosition3 - 1.5f * _randomGenerator.NextFloatRanged(3.5f, 5.5f);
			val6.SetFrame(ref identity3, true);
			val6.SetFactorColor(_shipColor);
			SetRandomAngularVelocityToEntity(val6);
			if (_floatsamMissionLogic != null)
			{
				_floatsamMissionLogic.RegisterFloatsamInstance(val6);
			}
		}
		int num5 = (int)((float)_randomGenerator.Next(10, 16) * num3);
		for (int j = 0; j < num5; j++)
		{
			Vec3 val8 = min + new Vec3(val.x * _randomGenerator.NextFloat(), val.y * _randomGenerator.NextFloat(), 0f, -1f);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			GameEntity val9 = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, GetRandomDebrisPrefab(DebrisType.Generic), true, true, "");
			MatrixFrame identity4 = MatrixFrame.Identity;
			identity4.origin = globalPosition + val8;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			float waterLevelAtPosition4 = ((WeakGameEntity)(ref gameEntity)).GetWaterLevelAtPosition(((Vec3)(ref identity4.origin)).AsVec2, true, false);
			identity4.origin.z = waterLevelAtPosition4 - 1.5f * _randomGenerator.NextFloatRanged(1f, 4.5f);
			val9.SetFrame(ref identity4, true);
			val9.SetFactorColor(_shipColor);
			SetRandomAngularVelocityToEntity(val9);
			if (_floatsamMissionLogic != null)
			{
				_floatsamMissionLogic.RegisterFloatsamInstance(val9);
			}
		}
	}

	private void OnShieldDestroyed(DestructableComponent target, Agent attackerAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		if (!_floatsamSystemEnabled || _numberOfPendingShieldBreakRecords >= 10)
		{
			return;
		}
		Texture bannerTexture = null;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)target).GameEntity;
		GameEntityComponent componentAtIndex = ((WeakGameEntity)(ref gameEntity)).GetComponentAtIndex(0, (ComponentType)0);
		MetaMesh val = (MetaMesh)(object)((componentAtIndex is MetaMesh) ? componentAtIndex : null);
		if ((NativeObject)(object)val != (NativeObject)null && val.MeshCount > 0)
		{
			bannerTexture = val.GetMeshAtIndex(0).GetMaterial().GetTexture((MBTextureType)1);
		}
		string text = "floatable_debris_";
		text += _shieldName;
		if (_randomGenerator.NextFloat() > 0.15f)
		{
			switch (_randomGenerator.Next(0, 3))
			{
			case 0:
				text += "_broken_a";
				break;
			case 1:
				text += "_broken_b";
				break;
			case 2:
				text += "_broken_c";
				break;
			}
		}
		gameEntity = ((ScriptComponentBehavior)target).GameEntity;
		Vec3 linearVelocity = GameEntityPhysicsExtensions.GetLinearVelocity(((WeakGameEntity)(ref gameEntity)).Root);
		linearVelocity += Vec3.Up * 1.5f;
		ref ShieldBreakRecord reference = ref _shieldBreakRecords[_numberOfPendingShieldBreakRecords];
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		gameEntity = ((ScriptComponentBehavior)target).GameEntity;
		MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		reference.ShipLocalSpawnFrame = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalFrame2);
		_shieldBreakRecords[_numberOfPendingShieldBreakRecords].BannerTexture = bannerTexture;
		_shieldBreakRecords[_numberOfPendingShieldBreakRecords].LinearVelocity = linearVelocity;
		_shieldBreakRecords[_numberOfPendingShieldBreakRecords].PrefabName = text;
		_numberOfPendingShieldBreakRecords++;
	}

	private void OnShipRamming(MissionShip rammingShip, MissionShip rammedShip, float damagePercent, bool isFirstImpact, CapsuleData capsuleData, int ramQuality)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		if (isFirstImpact && rammedShip == _ownMissionShipCached)
		{
			Vec3 linearVelocity = rammingShip.Physics.LinearVelocity;
			Vec3 val = ((Vec3)(ref linearVelocity)).NormalizedCopy();
			_impulseRecordsToProcess[_numberOfPendingImpulseRecords].AveragePosition = ((CapsuleData)(ref capsuleData)).P2 + new Vec3(0f, 0f, 1f, -1f);
			ref ImpulseRecord reference = ref _impulseRecordsToProcess[_numberOfPendingImpulseRecords];
			Vec3 val2 = -val + new Vec3(0f, 0f, 1.75f, -1f);
			reference.AverageNormal = ((Vec3)(ref val2)).NormalizedCopy();
			_impulseRecordsToProcess[_numberOfPendingImpulseRecords].TotalImpulse = (float)(ramQuality + 5) * 150000f;
			_impulseRecordsToProcess[_numberOfPendingImpulseRecords].Speed = linearVelocity * 2f;
			_impulseRecordsToProcess[_numberOfPendingImpulseRecords].DebrisType = DebrisType.Ramming;
			_impulseRecordsToProcess[_numberOfPendingImpulseRecords].DecalType = DecalType.Collision;
			_impulseRecordsToProcess[_numberOfPendingImpulseRecords].InitialSpeedMultiplier = 1f;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)rammedShip).GameEntity;
			MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
			ref ImpulseRecord reference2 = ref _impulseRecordsToProcess[_numberOfPendingImpulseRecords];
			val2 = ((CapsuleData)(ref capsuleData)).P2;
			reference2.ShipLocalPosition = ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref val2);
			_impulseRecordsToProcess[_numberOfPendingImpulseRecords].ShipLocalNormal = ((Mat3)(ref bodyWorldTransform.rotation)).TransformToLocal(ref linearVelocity);
			_numberOfPendingImpulseRecords++;
		}
	}

	private void OnShipHit(MissionShip ship, Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, MissionWeapon weapon, int missileIndex)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		if (!_floatsamSystemEnabled || ship != _ownMissionShipCached || ((MissionWeapon)(ref weapon)).CurrentUsageItem == null)
		{
			return;
		}
		WeaponClass weaponClass = ((MissionWeapon)(ref weapon)).CurrentUsageItem.WeaponClass;
		if (((int)weaponClass != 20 && (int)weaponClass != 19 && (int)weaponClass != 26 && (int)weaponClass != 27) || _numberOfPendingImpulseRecords >= 10)
		{
			return;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec3 val = -impactDirection;
		Vec3 val2 = val;
		if (_bodyEntity != (GameEntity)null)
		{
			Vec3 zero = Vec3.Zero;
			float num = 0f;
			if (_bodyEntity.RayHitEntityWithNormal(impactPosition - impactDirection, ((Vec3)(ref impactDirection)).NormalizedCopy(), 2f, ref zero, ref num))
			{
				val = zero;
				val2 = zero;
				((Vec3)(ref val)).Normalize();
			}
		}
		int num2 = ((!((MBObjectBase)((MissionWeapon)(ref weapon)).Item).StringId.Contains("grape")) ? _midCollisionHitParticleIndex : _collisionHitParticleIndex);
		MatrixFrame identity = MatrixFrame.Identity;
		identity.rotation.u = Vec3.Up;
		identity.rotation.s = val;
		identity.rotation.f = -((Vec3)(ref globalFrame.rotation.s)).CrossProductWithUp();
		identity.rotation.s = Vec3.CrossProduct(globalFrame.rotation.f, globalFrame.rotation.u);
		identity.origin = impactPosition;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).Scene.CreateBurstParticle(num2, identity);
		Vec3 speed = Vec3.Zero;
		if (GameEntityPhysicsExtensions.HasDynamicRigidBody(((ScriptComponentBehavior)this).GameEntity))
		{
			speed = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)this).GameEntity, impactPosition);
		}
		float num3 = (float)damage / 150f;
		_impulseRecordsToProcess[_numberOfPendingImpulseRecords].AveragePosition = impactPosition;
		_impulseRecordsToProcess[_numberOfPendingImpulseRecords].AverageNormal = val;
		_impulseRecordsToProcess[_numberOfPendingImpulseRecords].TotalImpulse = 150000f * num3;
		_impulseRecordsToProcess[_numberOfPendingImpulseRecords].Speed = speed;
		_impulseRecordsToProcess[_numberOfPendingImpulseRecords].DebrisType = DebrisType.Scrape;
		_impulseRecordsToProcess[_numberOfPendingImpulseRecords].DecalType = DecalType.Collision;
		_impulseRecordsToProcess[_numberOfPendingImpulseRecords].InitialSpeedMultiplier = 1f;
		_impulseRecordsToProcess[_numberOfPendingImpulseRecords].ShipLocalPosition = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref impactPosition);
		_impulseRecordsToProcess[_numberOfPendingImpulseRecords].ShipLocalNormal = ((Mat3)(ref globalFrame.rotation)).TransformToLocal(ref val2);
		_numberOfPendingImpulseRecords++;
	}

	public void EnableFloatsamSystem()
	{
		_floatsamSystemEnabled = true;
	}
}
