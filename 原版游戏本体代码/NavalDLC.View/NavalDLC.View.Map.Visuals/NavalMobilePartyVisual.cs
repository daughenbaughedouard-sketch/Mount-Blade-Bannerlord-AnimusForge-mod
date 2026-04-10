using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Helpers;
using NavalDLC.Missions.Objects;
using NavalDLC.View.Map.Managers;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.View.Map.Visuals;

public class NavalMobilePartyVisual : MapEntityVisual<PartyBase>
{
	private struct ShipOar
	{
		internal WeakGameEntity _oarEntity;

		internal float _sideSign;
	}

	public struct BlockadeShipVisual
	{
		public GameEntity ShipEntity;

		public float RockingPhase;
	}

	private class ShipFoamDecal
	{
		internal Decal _splashFoamDecal;

		internal MatrixFrame _currentFrame;

		internal float _cumulativeDtTillStart;

		internal Vec3 _randomScale;

		internal Vec3 _currentSpeed;

		internal Vec3 _sideVectorStart;

		internal Vec3 _sideVectorEnd;

		internal ShipFoamDecal()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			_splashFoamDecal = null;
			_currentFrame = MatrixFrame.Identity;
			_sideVectorStart = Vec3.Zero;
			_sideVectorEnd = Vec3.Zero;
			_cumulativeDtTillStart = 0f;
			_randomScale = new Vec3(1f, 1f, 1f, -1f);
			_currentSpeed = Vec3.Zero;
		}
	}

	private const float DefaultWaterLevelZ = 2.58f;

	private const float SailWindVisualAmplifier = 5f;

	private const float BannerWindVisualAmplifier = 3f;

	private const string LeftOarTag = "oar_gate_left";

	private const string RightOarTag = "oar_gate_right";

	private const string BodyMeshTag = "body_mesh";

	private const int NumberOfSplashDecal = 20;

	private float _entityAlpha;

	private bool _isSailFolded;

	private float _sailAlpha;

	private string _flagShipId;

	private bool _isVisualInRaftState;

	private MatrixFrame _firstOarRotationFrameCached = MatrixFrame.Identity;

	private MatrixFrame _secondOarRotationFrameCached = MatrixFrame.Identity;

	private readonly Dictionary<Ship, BlockadeShipVisual> _shipToBlockadeShipVisualCache = new Dictionary<Ship, BlockadeShipVisual>();

	private readonly List<ShipOar> _oars = new List<ShipOar>();

	private readonly List<SailVisual> _sailVisualCache = new List<SailVisual>();

	private SoundEvent _sailingSoundEvent;

	private float _oarPhase;

	private float _rockingPhase;

	private float _swayingAngle;

	private float _rollingAngle;

	private CampaignVec2 _targetPositionForSwaying;

	private float _lastFrameLerpedAngle;

	private GameEntity _shipEntity;

	private WeakGameEntity _bodyMeshEntity;

	private GameEntity _currentCollidedBridgeEntity;

	private float _bearingRotation;

	private GameEntity _shipMovementParticleEntity;

	private ParticleSystem _shipMovementParticle;

	private GameEntity _shipStillMovementParticleEntity;

	private ParticleSystem _shipStillMovementParticle;

	private BoundingBox _wakeBB;

	private Scene _ownerSceneCached;

	private ShipFoamDecal[] _splashFoamDecals = new ShipFoamDecal[20];

	private Vec3 _lastDecalSpawnPosition = Vec3.Zero;

	private float _nextDecalSpawnMetersSq = 0.09f;

	private int _nextDecalToUse;

	public override float BearingRotation => _bearingRotation;

	public override MapEntityVisual AttachedTo
	{
		get
		{
			MobileParty mobileParty = base.MapEntity.MobileParty;
			if (((mobileParty != null) ? mobileParty.AttachedTo : null) != null)
			{
				return (MapEntityVisual)(object)((EntityVisualManagerBase<PartyBase>)NavalMobilePartyVisualManager.Current).GetVisualOfEntity(base.MapEntity.MobileParty.AttachedTo.Party);
			}
			return null;
		}
	}

	public override CampaignVec2 InteractionPositionForPlayer => ((IInteractablePoint)base.MapEntity).GetInteractionPosition(MobileParty.MainParty);

	public override bool IsMobileEntity => base.MapEntity.IsMobile;

	public override bool IsMainEntity => base.MapEntity == PartyBase.MainParty;

	public GameEntity StrategicEntity { get; private set; }

	public NavalMobilePartyVisual(PartyBase partyBase)
		: base(partyBase)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		((MapEntityVisual)this).CircleLocalFrame = MatrixFrame.Identity;
	}

	public override bool IsEnemyOf(IFaction faction)
	{
		return FactionManager.IsAtWarAgainstFaction(base.MapEntity.MapFaction, faction.MapFaction);
	}

	public override bool IsInSameFaction(IFaction faction)
	{
		return DiplomacyHelper.IsSameFactionAndNotEliminated(base.MapEntity.MapFaction, faction.MapFaction);
	}

	public override bool IsAllyOf(IFaction faction)
	{
		return DiplomacyHelper.HasAllianceWithFaction(base.MapEntity.MapFaction, faction.MapFaction);
	}

	internal void OnPartyRemoved()
	{
		if (StrategicEntity != (GameEntity)null)
		{
			RemoveVisualFromVisualsOfEntities();
			((MapEntityVisual)this).ReleaseResources();
			StrategicEntity.Remove(111);
			_isVisualInRaftState = false;
		}
	}

	public override void OnTrackAction()
	{
		MobileParty mobileParty = base.MapEntity.MobileParty;
		if (mobileParty != null)
		{
			if (Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)mobileParty))
			{
				Campaign.Current.VisualTrackerManager.RemoveTrackedObject((ITrackableBase)(object)mobileParty, false);
			}
			else
			{
				Campaign.Current.VisualTrackerManager.RegisterObject((ITrackableCampaignObject)(object)mobileParty);
			}
		}
	}

	public override bool OnMapClick(bool followModifierUsed)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		NavigationType val = default(NavigationType);
		if (((MapEntityVisual)this).IsMainEntity)
		{
			MobileParty.MainParty.SetMoveModeHold();
		}
		else if (base.MapEntity.MobileParty.IsCurrentlyAtSea == MobileParty.MainParty.IsCurrentlyAtSea && NavigationHelper.CanPlayerNavigateToPosition(base.MapEntity.MobileParty.Position, ref val))
		{
			if (followModifierUsed)
			{
				MobileParty.MainParty.SetMoveEscortParty(base.MapEntity.MobileParty, val, false);
			}
			else
			{
				MobileParty.MainParty.SetMoveEngageParty(base.MapEntity.MobileParty, val);
			}
		}
		return true;
	}

	public override void OnHover()
	{
		if (base.MapEntity.MapEvent != null)
		{
			InformationManager.ShowTooltip(typeof(MapEvent), new object[1] { base.MapEntity.MapEvent });
		}
		else
		{
			if (!base.MapEntity.IsMobile || !base.MapEntity.IsVisible)
			{
				return;
			}
			if (base.MapEntity.MobileParty.Army != null && base.MapEntity.MobileParty.Army.DoesLeaderPartyAndAttachedPartiesContain(base.MapEntity.MobileParty))
			{
				if (base.MapEntity.MobileParty.Army.LeaderParty.SiegeEvent != null)
				{
					InformationManager.ShowTooltip(typeof(SiegeEvent), new object[1] { base.MapEntity.MobileParty.Army.LeaderParty.SiegeEvent });
					return;
				}
				InformationManager.ShowTooltip(typeof(Army), new object[3]
				{
					base.MapEntity.MobileParty.Army,
					false,
					true
				});
			}
			else if (base.MapEntity.MobileParty.SiegeEvent != null)
			{
				InformationManager.ShowTooltip(typeof(SiegeEvent), new object[1] { base.MapEntity.MobileParty.SiegeEvent });
			}
			else
			{
				InformationManager.ShowTooltip(typeof(MobileParty), new object[3]
				{
					base.MapEntity.MobileParty,
					false,
					true
				});
			}
		}
	}

	public override Vec3 GetVisualPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vec2 visualPosition2DWithoutError = base.MapEntity.MobileParty.VisualPosition2DWithoutError;
		CampaignVec2 position = base.MapEntity.Position;
		Vec3 val = ((CampaignVec2)(ref position)).AsVec3();
		return ((Vec2)(ref visualPosition2DWithoutError)).ToVec3(((Vec3)(ref val)).Z);
	}

	public override void ReleaseResources()
	{
		ResetPartyIcon();
	}

	public override bool IsVisibleOrFadingOut()
	{
		return _entityAlpha > 0f;
	}

	public override void OnOpenEncyclopedia()
	{
		if (base.MapEntity.MobileParty.IsLordParty && base.MapEntity.MobileParty.LeaderHero != null)
		{
			Campaign.Current.EncyclopediaManager.GoToLink(base.MapEntity.MobileParty.LeaderHero.EncyclopediaLink);
		}
	}

	internal void Tick(float dt, float realDt, ref int dirtyPartiesCount, ref NavalMobilePartyVisual[] dirtyPartiesList)
	{
		if (StrategicEntity == (GameEntity)null)
		{
			return;
		}
		if (base.MapEntity.MobileParty.IsNavalVisualDirty && (_entityAlpha > 0f || base.MapEntity.IsVisible))
		{
			int num = Interlocked.Increment(ref dirtyPartiesCount);
			dirtyPartiesList[num] = this;
		}
		if (!HasNavalVisual())
		{
			return;
		}
		if (!base.MapEntity.MobileParty.IsTransitionInProgress)
		{
			if (((MapEntityVisual)this).IsVisibleOrFadingOut() && StrategicEntity != (GameEntity)null)
			{
				UpdateEntityPosition(dt, realDt, isVisible: true);
			}
		}
		else if (GetTransitionProgress() <= 1f)
		{
			TickTransitionFadeState(dt);
		}
	}

	internal void UpdateEntityPosition(float dt, float realDt, bool isVisible = false)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Invalid comparison between Unknown and I4
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Invalid comparison between Unknown and I4
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Invalid comparison between Unknown and I4
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		MobileParty mobileParty = base.MapEntity.MobileParty;
		UpdateBearingRotation(realDt);
		MatrixFrame entityFrame = MatrixFrame.Identity;
		entityFrame.origin = ((MapEntityVisual)this).GetVisualPosition();
		MatrixFrame localFrame = StrategicEntity.GetLocalFrame();
		Vec2 val = ((Vec3)(ref entityFrame.origin)).AsVec2 - ((Vec3)(ref localFrame.origin)).AsVec2;
		float length = ((Vec2)(ref val)).Length;
		float num = ((dt > 0f) ? (length / dt) : 0f);
		if (mobileParty.Army != null && mobileParty.AttachedTo == mobileParty.Army.LeaderParty && (base.MapEntity.MapEvent == null || !base.MapEntity.MapEvent.IsFieldBattle))
		{
			Vec2 val2;
			if (num > 20f)
			{
				((Mat3)(ref entityFrame.rotation)).RotateAboutUp(_bearingRotation);
			}
			else if (mobileParty.CurrentSettlement == null)
			{
				val2 = ((Vec3)(ref localFrame.rotation.f)).AsVec2;
				float rotationInRadians = ((Vec2)(ref val2)).RotationInRadians;
				val2 = val + Vec2.FromRotation(_bearingRotation) * 0.01f;
				float num2 = MBMath.LerpRadians(rotationInRadians, ((Vec2)(ref val2)).RotationInRadians, Math.Min(6f * dt, 1f), 0.03f * dt, 10f * dt);
				((Mat3)(ref entityFrame.rotation)).RotateAboutUp(num2);
			}
			else
			{
				val2 = ((Vec3)(ref localFrame.rotation.f)).AsVec2;
				float rotationInRadians2 = ((Vec2)(ref val2)).RotationInRadians;
				((Mat3)(ref entityFrame.rotation)).RotateAboutUp(rotationInRadians2);
			}
		}
		else if (mobileParty.CurrentSettlement == null)
		{
			((Mat3)(ref entityFrame.rotation)).RotateAboutUp(GetVisualRotation());
			Vec3 val3 = Vec3.Zero;
			float num3 = default(float);
			Vec3 up = default(Vec3);
			for (int i = -2; i <= 2; i++)
			{
				for (int j = -2; j <= 2; j++)
				{
					Vec2 val4 = ((Vec3)(ref entityFrame.origin)).AsVec2 + new Vec2((float)i * 0.5f, (float)j * 0.5f);
					Campaign.Current.MapSceneWrapper.GetTerrainHeightAndNormal(val4, ref num3, ref up);
					if (num3 < 2.58f)
					{
						up = Vec3.Up;
					}
					val3 += up;
				}
			}
			val3 /= MathF.Pow(5f, 2f);
			float num4 = Vec3.DotProduct(entityFrame.rotation.u, val3);
			float num5 = Vec3.DotProduct(entityFrame.rotation.f, val3);
			Vec3 val5 = entityFrame.rotation.u * num4;
			Vec3 val6 = entityFrame.rotation.f * num5;
			Vec3 val7 = val5 + val6;
			float num6 = Vec3.AngleBetweenTwoVectors(entityFrame.rotation.u, val7);
			float num7 = ((num5 < 0f) ? 1f : (-1f));
			_lastFrameLerpedAngle = MathF.Lerp(_lastFrameLerpedAngle, num7 * num6, 0.1f, 1E-05f);
			((Mat3)(ref entityFrame.rotation)).RotateAboutSide(_lastFrameLerpedAngle);
		}
		if (base.MapEntity.MobileParty.IsMainParty && MobileParty.MainParty.IsCurrentlyAtSea)
		{
			CheckBridgeFadeState();
		}
		if (_shipEntity != (GameEntity)null && !base.MapEntity.MobileParty.IsInRaftState && isVisible)
		{
			Vec2 windForPosition = Campaign.Current.Models.MapWeatherModel.GetWindForPosition(base.MapEntity.Position);
			ApplyWindEffect(windForPosition, ((Vec3)(ref entityFrame.rotation.f)).AsVec2, realDt, dt);
			TickSailingSound(num);
			TickOars(dt, realDt);
			TickIdleShipAnimation(base.MapEntity.FlagShip, ref _rockingPhase, ref entityFrame);
			TickSwayingAnimation(ref entityFrame);
			float speedUpMultiplier = Campaign.Current.SpeedUpMultiplier;
			float num8 = realDt;
			if ((int)Campaign.Current.TimeControlMode == 4 && !Campaign.Current.IsMainPartyWaiting)
			{
				num8 *= speedUpMultiplier;
			}
			else if ((int)Campaign.Current.TimeControlMode == 5 || (int)Campaign.Current.TimeControlMode == 2)
			{
				num8 *= speedUpMultiplier;
			}
			TickFoamDecals(num8);
		}
		if (!Extensions.IsEmpty<KeyValuePair<Ship, BlockadeShipVisual>>((IEnumerable<KeyValuePair<Ship, BlockadeShipVisual>>)_shipToBlockadeShipVisualCache))
		{
			foreach (KeyValuePair<Ship, BlockadeShipVisual> item in _shipToBlockadeShipVisualCache.ToList())
			{
				BlockadeShipVisual value = item.Value;
				MatrixFrame entityFrame2 = value.ShipEntity.GetLocalFrame();
				TickIdleShipAnimation(item.Key, ref value.RockingPhase, ref entityFrame2, isBlockadeShip: true);
				value.ShipEntity.SetLocalFrame(ref entityFrame2, true);
				_shipToBlockadeShipVisualCache[item.Key] = value;
			}
		}
		MatrixFrame frame = StrategicEntity.GetFrame();
		if (!((MatrixFrame)(ref frame)).NearlyEquals(entityFrame, 1E-05f))
		{
			StrategicEntity.SetFrame(ref entityFrame, true);
		}
	}

	internal void OnStartup()
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (base.MapEntity.IsMobile)
		{
			StrategicEntity = GameEntity.CreateEmpty(((EntityVisualManagerBase)NavalMobilePartyVisualManager.Current).MapScene, true, true, true);
			if (!base.MapEntity.IsVisible)
			{
				GameEntity strategicEntity = StrategicEntity;
				strategicEntity.EntityFlags = (EntityFlags)(strategicEntity.EntityFlags | 0x20000000);
			}
		}
		CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(base.MapEntity);
		if (0 == 0)
		{
			((MapEntityVisual)this).CircleLocalFrame = MatrixFrame.Identity;
			if ((visualPartyLeader != null && ((BasicCharacterObject)visualPartyLeader).HasMount()) || base.MapEntity.MobileParty.IsCaravan)
			{
				MatrixFrame circleLocalFrame = ((MapEntityVisual)this).CircleLocalFrame;
				Mat3 rotation = circleLocalFrame.rotation;
				((Mat3)(ref rotation)).ApplyScaleLocal(0.4625f);
				circleLocalFrame.rotation = rotation;
				((MapEntityVisual)this).CircleLocalFrame = circleLocalFrame;
			}
			else
			{
				MatrixFrame circleLocalFrame2 = ((MapEntityVisual)this).CircleLocalFrame;
				Mat3 rotation2 = circleLocalFrame2.rotation;
				((Mat3)(ref rotation2)).ApplyScaleLocal(0.3725f);
				circleLocalFrame2.rotation = rotation2;
				((MapEntityVisual)this).CircleLocalFrame = circleLocalFrame2;
			}
		}
		Vec2 bearing = base.MapEntity.MobileParty.Bearing;
		_bearingRotation = ((Vec2)(ref bearing)).RotationInRadians;
		StrategicEntity.SetVisibilityExcludeParents(base.MapEntity.IsVisible);
		StrategicEntity.SetReadyToRender(true);
		StrategicEntity.SetEntityEnvMapVisibility(false);
		_entityAlpha = (base.MapEntity.IsVisible ? 1f : 0f);
		_sailAlpha = 1f;
		AddVisualToVisualsOfEntities();
	}

	internal void TickFadingState(float realDt)
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		if ((_entityAlpha < 1f && base.MapEntity.IsVisible) || (_entityAlpha > 0f && !base.MapEntity.IsVisible))
		{
			if (base.MapEntity.IsVisible)
			{
				if (_entityAlpha <= 0f)
				{
					foreach (BlockadeShipVisual value in _shipToBlockadeShipVisualCache.Values)
					{
						value.ShipEntity.SetVisibilityExcludeParents(true);
					}
					StrategicEntity.SetVisibilityExcludeParents(true);
				}
				_entityAlpha = MathF.Min(_entityAlpha + MathF.Max(realDt, 1E-05f), 1f);
				StrategicEntity.SetAlpha(_entityAlpha);
				GameEntity strategicEntity = StrategicEntity;
				strategicEntity.EntityFlags = (EntityFlags)(strategicEntity.EntityFlags & -536870913);
				{
					foreach (BlockadeShipVisual value2 in _shipToBlockadeShipVisualCache.Values)
					{
						value2.ShipEntity.SetAlpha(_entityAlpha);
					}
					return;
				}
			}
			_entityAlpha = MathF.Max(_entityAlpha - MathF.Max(realDt, 1E-05f), 0f);
			StrategicEntity.SetAlpha(_entityAlpha);
			foreach (BlockadeShipVisual value3 in _shipToBlockadeShipVisualCache.Values)
			{
				value3.ShipEntity.SetAlpha(_entityAlpha);
			}
			if (!(_entityAlpha <= 0f))
			{
				return;
			}
			StrategicEntity.SetVisibilityExcludeParents(false);
			foreach (BlockadeShipVisual value4 in _shipToBlockadeShipVisualCache.Values)
			{
				value4.ShipEntity.SetVisibilityExcludeParents(false);
			}
			GameEntity strategicEntity2 = StrategicEntity;
			strategicEntity2.EntityFlags = (EntityFlags)(strategicEntity2.EntityFlags | 0x20000000);
			ShipFoamDecal[] splashFoamDecals = _splashFoamDecals;
			foreach (ShipFoamDecal shipFoamDecal in splashFoamDecals)
			{
				if (shipFoamDecal != null && (NativeObject)(object)shipFoamDecal._splashFoamDecal != (NativeObject)null)
				{
					shipFoamDecal._splashFoamDecal.SetIsVisible(false);
				}
			}
		}
		else
		{
			NavalMobilePartyVisualManager.Current.UnRegisterFadingVisual(this);
		}
	}

	private void TickTransitionFadeState(float dt)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		if (GetTransitionProgress() > 0f && base.MapEntity.MobileParty.IsCurrentlyAtSea && _shipEntity != (GameEntity)null && base.MapEntity.IsVisible)
		{
			CampaignVec2 endPositionForNavigationTransition = base.MapEntity.MobileParty.EndPositionForNavigationTransition;
			CampaignVec2 val = base.MapEntity.MobileParty.Position;
			CampaignVec2 val2 = endPositionForNavigationTransition - ((CampaignVec2)(ref val)).ToVec2();
			MatrixFrame globalFrame = StrategicEntity.GetGlobalFrame();
			val = ((CampaignVec2)(ref val2)).LeftVec();
			float rotationInRadians = ((CampaignVec2)(ref val)).RotationInRadians;
			Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			float smallestDifferenceBetweenTwoAngles = MBMath.GetSmallestDifferenceBetweenTwoAngles(rotationInRadians, ((Vec2)(ref asVec)).RotationInRadians);
			val = ((CampaignVec2)(ref val2)).RightVec();
			float rotationInRadians2 = ((CampaignVec2)(ref val)).RotationInRadians;
			asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			float smallestDifferenceBetweenTwoAngles2 = MBMath.GetSmallestDifferenceBetweenTwoAngles(rotationInRadians2, ((Vec2)(ref asVec)).RotationInRadians);
			float num = ((MathF.Abs(smallestDifferenceBetweenTwoAngles2) > MathF.Abs(smallestDifferenceBetweenTwoAngles)) ? smallestDifferenceBetweenTwoAngles : smallestDifferenceBetweenTwoAngles2);
			float num2 = MathF.Lerp(0f, num, dt * 5f, 1E-05f);
			MatrixFrame localFrame = StrategicEntity.GetLocalFrame();
			((MatrixFrame)(ref localFrame)).Rotate(MathF.Abs(num2), ref Vec3.Up);
			StrategicEntity.SetLocalFrame(ref localFrame, false);
			MatrixFrame globalFrame2 = StrategicEntity.GetGlobalFrame();
			CampaignVec2 val3 = base.MapEntity.MobileParty.Position + base.MapEntity.MobileParty.ArmyPositionAdder * 0.7f;
			float num3 = MathF.Lerp(((Vec3)(ref globalFrame2.origin)).X, ((CampaignVec2)(ref val3)).X, dt * 5f, 1E-05f);
			float num4 = MathF.Lerp(((Vec3)(ref globalFrame2.origin)).Y, ((CampaignVec2)(ref val3)).Y, dt * 5f, 1E-05f);
			globalFrame2.origin = new Vec3(num3, num4, globalFrame2.origin.z, -1f);
			StrategicEntity.SetGlobalFrame(ref globalFrame2, true);
		}
	}

	internal void ClearVisualMemory()
	{
		ResetPartyIcon();
		base.MapEntity.SetVisualAsDirty();
	}

	internal void ValidateIsDirty()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (base.MapEntity.MemberRoster.TotalManCount != 0)
		{
			RefreshPartyIcon();
			if ((!(_entityAlpha < 1f) || !base.MapEntity.IsVisible) && (!(_entityAlpha > 0f) || base.MapEntity.IsVisible))
			{
				return;
			}
			if (base.MapEntity.MobileParty.IsTransitionInProgress)
			{
				Vec3 globalPosition = StrategicEntity.GlobalPosition;
				if (!((Vec3)(ref globalPosition)).IsNonZero)
				{
					UpdateEntityPosition(0.1f, 0.1f);
				}
			}
			NavalMobilePartyVisualManager.Current.RegisterFadingVisual(this);
		}
		else
		{
			ResetPartyIcon();
		}
	}

	private void RefreshPartyIcon()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!base.MapEntity.MobileParty.IsNavalVisualDirty)
		{
			return;
		}
		base.MapEntity.MobileParty.OnNavalVisualsUpdated();
		MatrixFrame circleLocalFrame = ((MapEntityVisual)this).CircleLocalFrame;
		circleLocalFrame.origin = Vec3.Zero;
		((MapEntityVisual)this).CircleLocalFrame = circleLocalFrame;
		if (!HasNavalVisual())
		{
			if (((List<Ship>)(object)base.MapEntity.MobileParty.Ships).Count == 0 || base.MapEntity.MobileParty.IsInRaftState)
			{
				ResetPartyIcon();
			}
			else
			{
				RemoveBlockadeVisuals();
				HideNavalVisual();
			}
			RemoveVisualFromVisualsOfEntities();
		}
		else
		{
			AddVisualToVisualsOfEntities();
			Settlement besiegedSettlement = base.MapEntity.MobileParty.BesiegedSettlement;
			if (((besiegedSettlement != null) ? besiegedSettlement.SiegeEvent : null) != null && base.MapEntity.MobileParty.BesiegedSettlement.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(base.MapEntity, (BattleTypes)5))
			{
				HideNavalVisual();
				if (base.MapEntity.MobileParty.BesiegedSettlement.SiegeEvent.IsBlockadeActive)
				{
					NavalDLCViewHelpers.BlockadeVisualHelper.AddBlockadeVisuals(_shipToBlockadeShipVisualCache, base.MapEntity, StrategicEntity);
				}
				else
				{
					RemoveBlockadeVisuals();
				}
			}
			else if (base.MapEntity.MobileParty != null && (base.MapEntity.MobileParty.IsCurrentlyAtSea || base.MapEntity.MobileParty.IsTransitionInProgress))
			{
				if (base.MapEntity.MobileParty.IsInRaftState)
				{
					ResetPartyIcon();
					AddRaftVisual();
				}
				else if (((List<Ship>)(object)base.MapEntity.Ships).Count > 0)
				{
					AddShipVisual();
				}
				InitializePartyCollider(base.MapEntity);
			}
		}
		StrategicEntity.CheckResources(true, false);
	}

	private void InitializePartyCollider(PartyBase party)
	{
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		if (StrategicEntity != (GameEntity)null && party.IsMobile)
		{
			if (_shipEntity != (GameEntity)null && ((WeakGameEntity)(ref _bodyMeshEntity)).IsValid)
			{
				UpdateEntityPosition(0.1f, 0.1f);
				MatrixFrame globalFrame = StrategicEntity.GetGlobalFrame();
				Vec3 eulerAngles = ((Mat3)(ref globalFrame.rotation)).GetEulerAngles();
				globalFrame = ((WeakGameEntity)(ref _bodyMeshEntity)).GetGlobalFrame();
				Vec3 eulerAngles2 = ((Mat3)(ref globalFrame.rotation)).GetEulerAngles();
				BoundingBox localPhysicsBoundingBox = GameEntityPhysicsExtensions.GetLocalPhysicsBoundingBox(_bodyMeshEntity, false);
				((Vec3)(ref localPhysicsBoundingBox.max)).RotateAboutZ(((Vec3)(ref eulerAngles)).RotationZ - ((Vec3)(ref eulerAngles2)).RotationZ);
				((Vec3)(ref localPhysicsBoundingBox.min)).RotateAboutZ(((Vec3)(ref eulerAngles)).RotationZ - ((Vec3)(ref eulerAngles2)).RotationZ);
				float num = MathF.Abs(localPhysicsBoundingBox.max.x - localPhysicsBoundingBox.min.x) / 40f;
				float num2 = num / 2f;
				float num3 = MathF.Max(localPhysicsBoundingBox.max.y, localPhysicsBoundingBox.min.y);
				float num4 = MathF.Min(localPhysicsBoundingBox.max.y, localPhysicsBoundingBox.min.y);
				Vec3 val = default(Vec3);
				((Vec3)(ref val))._002Ector(0f, num3 / 20f - num2, num2 + 0.01f, -1f);
				Vec3 val2 = default(Vec3);
				((Vec3)(ref val2))._002Ector(0f, num4 / 20f + num2, num2 + 0.01f, -1f);
				GameEntityPhysicsExtensions.AddCapsuleAsBody(StrategicEntity, val, val2, num, (BodyFlags)144, "");
			}
			else
			{
				GameEntityPhysicsExtensions.AddCapsuleAsBody(StrategicEntity, new Vec3(0f, 0.5f, 0f, -1f), new Vec3(0f, -0.5f, 0f, -1f), 0.5f, (BodyFlags)144, "");
			}
		}
	}

	private void ResetPartyIcon()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (StrategicEntity != (GameEntity)null)
		{
			if ((StrategicEntity.EntityFlags & 0x10000000) != 0)
			{
				StrategicEntity.RemoveFromPredisplayEntity();
			}
			StrategicEntity.ClearComponents();
		}
		if (_shipEntity != (GameEntity)null)
		{
			_shipEntity.ClearComponents();
			_sailVisualCache.Clear();
			_oars.Clear();
			_shipEntity = null;
			SoundEvent sailingSoundEvent = _sailingSoundEvent;
			if (sailingSoundEvent != null)
			{
				sailingSoundEvent.Stop();
			}
			_sailingSoundEvent = null;
			_oarPhase = 0f;
		}
		RemoveBlockadeVisuals();
		if (_currentCollidedBridgeEntity != (GameEntity)null)
		{
			_currentCollidedBridgeEntity.SetAlpha(1f);
			_currentCollidedBridgeEntity = null;
		}
		Vec2 bearing = base.MapEntity.MobileParty.Bearing;
		_bearingRotation = ((Vec2)(ref bearing)).RotationInRadians;
		_isVisualInRaftState = false;
		NavalMobilePartyVisualManager.Current.UnRegisterFadingVisual(this);
		ShipFoamDecal[] splashFoamDecals = _splashFoamDecals;
		foreach (ShipFoamDecal shipFoamDecal in splashFoamDecals)
		{
			if (shipFoamDecal != null && (NativeObject)(object)shipFoamDecal._splashFoamDecal != (NativeObject)null)
			{
				_ownerSceneCached.RemoveDecalInstance(shipFoamDecal._splashFoamDecal, "editor_set");
				shipFoamDecal._splashFoamDecal = null;
			}
		}
	}

	private void HideNavalVisual()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		StrategicEntity.SetVisibilityExcludeParents(false);
		Vec2 bearing = base.MapEntity.MobileParty.Bearing;
		_bearingRotation = ((Vec2)(ref bearing)).RotationInRadians;
		if (_currentCollidedBridgeEntity != (GameEntity)null)
		{
			_currentCollidedBridgeEntity.SetAlpha(1f);
			_currentCollidedBridgeEntity = null;
		}
		ShipFoamDecal[] splashFoamDecals = _splashFoamDecals;
		foreach (ShipFoamDecal shipFoamDecal in splashFoamDecals)
		{
			if (shipFoamDecal != null && (NativeObject)(object)shipFoamDecal._splashFoamDecal != (NativeObject)null)
			{
				shipFoamDecal._splashFoamDecal.SetIsVisible(false);
			}
		}
		NavalMobilePartyVisualManager.Current.UnRegisterFadingVisual(this);
	}

	private float GetTransitionProgress()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (((MapEntityVisual)this).IsMobileEntity && base.MapEntity.MobileParty.IsTransitionInProgress && base.MapEntity.MobileParty.NavigationTransitionDuration != CampaignTime.Zero)
		{
			CampaignTime val = base.MapEntity.MobileParty.NavigationTransitionStartTime;
			float elapsedHoursUntilNow = ((CampaignTime)(ref val)).ElapsedHoursUntilNow;
			val = base.MapEntity.MobileParty.NavigationTransitionDuration;
			return MBMath.ClampFloat(elapsedHoursUntilNow / (float)((CampaignTime)(ref val)).ToHours, 0f, 1f);
		}
		return 1f;
	}

	private float GetVisualRotation()
	{
		if (base.MapEntity.IsMobile && base.MapEntity.MapEvent != null && base.MapEntity.MapEvent.IsFieldBattle)
		{
			return GetMapEventVisualRotation();
		}
		return _bearingRotation;
	}

	private float GetMapEventVisualRotation()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (base.MapEntity.MapEventSide.OtherSide.LeaderParty != null && base.MapEntity.MapEventSide.OtherSide.LeaderParty.IsMobile && base.MapEntity.MapEventSide.OtherSide.LeaderParty.IsMobile)
		{
			Vec2 val = base.MapEntity.MapEventSide.OtherSide.LeaderParty.MobileParty.VisualPosition2DWithoutError - base.MapEntity.MobileParty.VisualPosition2DWithoutError;
			Vec2 val2 = ((Vec2)(ref val)).Normalized();
			if (base.MapEntity.MapEvent.IsNavalMapEvent)
			{
				((Vec2)(ref val2)).RotateCCW(0.6f);
			}
			return ((Vec2)(ref val2)).RotationInRadians;
		}
		return _bearingRotation;
	}

	private void CollectOars()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		_oars.Clear();
		WeakGameEntity weakEntity = _shipEntity.WeakEntity;
		foreach (WeakGameEntity item3 in ((WeakGameEntity)(ref weakEntity)).CollectChildrenEntitiesWithTagAsEnumarable("oar_gate_left"))
		{
			WeakGameEntity current = item3;
			WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref current)).GetFirstChildEntityWithTag("upgrade_slot");
			if (firstChildEntityWithTag != (GameEntity)null)
			{
				ShipOar item = new ShipOar
				{
					_oarEntity = firstChildEntityWithTag,
					_sideSign = 1f
				};
				_oars.Add(item);
			}
		}
		weakEntity = _shipEntity.WeakEntity;
		foreach (WeakGameEntity item4 in ((WeakGameEntity)(ref weakEntity)).CollectChildrenEntitiesWithTagAsEnumarable("oar_gate_right"))
		{
			WeakGameEntity current2 = item4;
			WeakGameEntity firstChildEntityWithTag2 = ((WeakGameEntity)(ref current2)).GetFirstChildEntityWithTag("upgrade_slot");
			if (firstChildEntityWithTag2 != (GameEntity)null)
			{
				ShipOar item2 = new ShipOar
				{
					_oarEntity = firstChildEntityWithTag2,
					_sideSign = -1f
				};
				_oars.Add(item2);
			}
		}
		_firstOarRotationFrameCached = MatrixFrame.Identity;
		_secondOarRotationFrameCached = MatrixFrame.Identity;
		((Mat3)(ref _firstOarRotationFrameCached.rotation)).RotateAboutSide(-0.17453292f);
		((Mat3)(ref _secondOarRotationFrameCached.rotation)).RotateAboutSide(-0.14835298f);
	}

	private void UpdateBearingRotation(float realDt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = base.MapEntity.MobileParty.Bearing;
		float num = MBMath.WrapAngle(((Vec2)(ref val)).RotationInRadians - _bearingRotation);
		float num2 = realDt / 2f;
		val = ((CampaignVec2)(ref base.MapEntity.MobileParty.NextTargetPosition)).ToVec2() - base.MapEntity.MobileParty.VisualPosition2DWithoutError;
		float num3 = ((((Vec2)(ref val)).Length < 2f) ? 7.5f : 3f);
		_bearingRotation += num * MathF.Min(num2 * num3, 1f);
		_bearingRotation = MBMath.WrapAngle(_bearingRotation);
	}

	private float GetOarVerticalAngle(float phase, float verticalBaseAngle, float verticalRotationAngle)
	{
		return verticalBaseAngle + MathF.Cos(0f - phase) * verticalRotationAngle;
	}

	private void TickSailingSound(float speed)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_sailingSoundEvent.SetPosition(((MapEntityVisual)this).GetVisualPosition());
		if (!_sailingSoundEvent.IsPlaying())
		{
			_sailingSoundEvent.Play();
		}
		_sailingSoundEvent.SetParameter("ShipSpeed", speed);
	}

	private MatrixFrame ComputeOarFrame(ShipOar oar)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		((Mat3)(ref identity.rotation)).RotateAboutForward(oar._sideSign * _oarPhase);
		ref MatrixFrame secondOarRotationFrameCached = ref _secondOarRotationFrameCached;
		MatrixFrame val = ((MatrixFrame)(ref identity)).TransformToParent(ref _firstOarRotationFrameCached);
		return ((MatrixFrame)(ref secondOarRotationFrameCached)).TransformToParent(ref val);
	}

	private void TickOars(float dt, float realDt)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (IsMoving())
		{
			float num = ((dt > 0f) ? dt : (realDt * 0.25f));
			float num2 = (base.MapEntity.MobileParty.IsActive ? base.MapEntity.MobileParty.LastCalculatedBaseSpeed : 0f);
			_oarPhase += num * num2 * 1.87f;
		}
		foreach (ShipOar oar in _oars)
		{
			MatrixFrame val = ComputeOarFrame(oar);
			WeakGameEntity oarEntity = oar._oarEntity;
			((WeakGameEntity)(ref oarEntity)).SetFrame(ref val, false);
		}
	}

	private void AddShipVisual()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		if (!base.MapEntity.IsActive)
		{
			return;
		}
		_isSailFolded = true;
		Ship flagShip = base.MapEntity.FlagShip;
		if (_flagShipId == ((MBObjectBase)flagShip.ShipHull).StringId && _shipEntity != (GameEntity)null && _isVisualInRaftState == base.MapEntity.MobileParty.IsInRaftState)
		{
			NavalDLCViewHelpers.ShipVisualHelper.RefreshShipVisuals(_shipEntity.WeakEntity, flagShip, _sailVisualCache);
		}
		else
		{
			if (StrategicEntity != (GameEntity)null)
			{
				if ((StrategicEntity.EntityFlags & 0x10000000) != 0)
				{
					StrategicEntity.RemoveFromPredisplayEntity();
				}
				StrategicEntity.ClearComponents();
			}
			if (_shipEntity != (GameEntity)null)
			{
				_shipEntity.ClearComponents();
				_sailVisualCache.Clear();
				_shipEntity = null;
			}
			else
			{
				_sailingSoundEvent = SoundEvent.CreateEventFromString("event:/map/army/sail", ((EntityVisualManagerBase)NavalMobilePartyVisualManager.Current).MapScene);
				_sailingSoundEvent.SetPosition(((MapEntityVisual)this).GetVisualPosition());
			}
			_shipEntity = NavalDLCViewHelpers.ShipVisualHelper.GetShipEntityForCampaign(flagShip, StrategicEntity.Scene, flagShip.GetShipVisualSlotInfos());
			NavalDLCViewHelpers.ShipVisualHelper.CollectSailVisuals(_shipEntity.WeakEntity, _sailVisualCache);
			CollectOars();
			float num = 50f;
			foreach (SailVisual item in _sailVisualCache)
			{
				if (item.Type == SailVisual.SailType.LateenSail)
				{
					MatrixFrame localFrame = item.SailYawRotationEntity.GetLocalFrame();
					localFrame.rotation = Mat3.Identity;
					((Mat3)(ref localFrame.rotation)).RotateAboutUp(num * (MathF.PI / 180f));
					item.SailYawRotationEntity.SetFrame(ref localFrame, false);
				}
			}
			WeakGameEntity weakEntity = _shipEntity.WeakEntity;
			_bodyMeshEntity = ((WeakGameEntity)(ref weakEntity)).GetFirstChildEntityWithTagRecursive("body_mesh");
			StrategicEntity.AddChild(_shipEntity, false);
			_shipEntity.SetVisibilityExcludeParents(true);
			_flagShipId = ((MBObjectBase)flagShip.ShipHull).StringId;
			_ownerSceneCached = _shipEntity.Scene;
			_shipMovementParticleEntity = GameEntity.CreateEmpty(_ownerSceneCached, false, false, false);
			_shipMovementParticleEntity.Name = "movement_particle";
			_shipEntity.AddChild(_shipMovementParticleEntity, false);
			MatrixFrame identity = MatrixFrame.Identity;
			if (((WeakGameEntity)(ref _bodyMeshEntity)).IsValid)
			{
				MetaMesh metaMesh = ((WeakGameEntity)(ref _bodyMeshEntity)).GetMetaMesh(0);
				if ((NativeObject)(object)metaMesh != (NativeObject)null)
				{
					_wakeBB = metaMesh.GetBoundingBox();
					identity.origin.y += _wakeBB.max.y * 0.8f;
					((Mat3)(ref identity.rotation)).ApplyScaleLocal(20f);
					_shipMovementParticleEntity.SetFrame(ref identity, true);
				}
			}
			_shipMovementParticleEntity.SetLocalFrame(ref identity, true);
			_lastDecalSpawnPosition = _shipEntity.GetGlobalFrame().origin;
			for (int i = 0; i < 20; i++)
			{
				_splashFoamDecals[i] = new ShipFoamDecal();
			}
			MatrixFrame identity2 = MatrixFrame.Identity;
			_shipMovementParticle = ParticleSystem.CreateParticleSystemAttachedToEntity("psys_campaign_ship_trail", _shipMovementParticleEntity, ref identity2);
			_shipStillMovementParticleEntity = GameEntity.CreateEmpty(_ownerSceneCached, false, false, false);
			_shipStillMovementParticleEntity.Name = "movement_particle_still";
			_shipEntity.AddChild(_shipStillMovementParticleEntity, false);
			_shipStillMovementParticleEntity.SetFrame(ref identity, true);
			_shipStillMovementParticle = ParticleSystem.CreateParticleSystemAttachedToEntity("psys_campaign_ship_trail_still", _shipStillMovementParticleEntity, ref identity2);
			_shipStillMovementParticleEntity.SetVisibilityExcludeParents(false);
		}
		_shipEntity.SetAlpha(GetTransitionProgress());
		StrategicEntity.SetAlpha(GetTransitionProgress());
		StrategicEntity.SetVisibilityExcludeParents(true);
		_isVisualInRaftState = false;
	}

	private bool IsMoving()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		if (base.MapEntity.MobileParty != null && base.MapEntity.MobileParty.IsMainParty)
		{
			result = !Campaign.Current.IsMainPartyWaiting;
		}
		else
		{
			MobileParty mobileParty = base.MapEntity.MobileParty;
			if (mobileParty != null)
			{
				CampaignVec2 position = mobileParty.Position;
				if (!((CampaignVec2)(ref position)).NearlyEquals(((CampaignVec2)(ref base.MapEntity.MobileParty.NextTargetPosition)).ToVec2(), 1E-05f))
				{
					result = true;
				}
			}
		}
		return result;
	}

	private void TickIdleShipAnimation(Ship ship, ref float rockingPhase, ref MatrixFrame entityFrame, bool isBlockadeShip = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Vec2 bearing = base.MapEntity.MobileParty.Bearing;
		if (MBMath.ApproximatelyEqualsTo(MBMath.WrapAngle(((Vec2)(ref bearing)).RotationInRadians - _bearingRotation), 0f, 0.003f))
		{
			float num = 1f;
			float num2 = MathF.PI / 40f;
			if ((int)ship.ShipHull.Type == 0)
			{
				num = 2f;
			}
			else if ((int)ship.ShipHull.Type == 1)
			{
				num = 1.5f;
			}
			rockingPhase += num * 0.01f;
			if (_swayingAngle != 0f)
			{
				_swayingAngle = 0f;
				rockingPhase = MathF.PI / 2f;
			}
			if (MathF.Abs(_rollingAngle) > num2)
			{
				num2 = MathF.Abs(_rollingAngle);
			}
			rockingPhase = MBMath.WrapAngle(rockingPhase);
			float num3 = MBMath.Map(MathF.Cos(rockingPhase), -1f, 1f, 0f - num2, num2);
			if (isBlockadeShip)
			{
				Vec3 eulerAngles = ((Mat3)(ref entityFrame.rotation)).GetEulerAngles();
				eulerAngles.y = num3 - eulerAngles.y;
				((Mat3)(ref entityFrame.rotation)).RotateAboutForward(((Vec3)(ref eulerAngles)).Y);
			}
			else
			{
				_rollingAngle = MBMath.LerpRadians(_rollingAngle, num3, 0.01f, 0f, num2);
				((Mat3)(ref entityFrame.rotation)).RotateAboutForward(_rollingAngle);
			}
		}
	}

	private void TickFoamDecals(float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0511: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_060b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_066e: Unknown result type (might be due to invalid IL or missing references)
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = _shipEntity.GetGlobalFrame();
		Vec3 val = new Vec3(0.013f, 0.025f, 1f, -1f) * 1.176f * 2f;
		Vec3 val2 = val * 17.5f;
		ShipFoamDecal[] splashFoamDecals = _splashFoamDecals;
		foreach (ShipFoamDecal shipFoamDecal in splashFoamDecals)
		{
			if ((NativeObject)(object)shipFoamDecal._splashFoamDecal != (NativeObject)null && shipFoamDecal._cumulativeDtTillStart < 3.15f)
			{
				shipFoamDecal._cumulativeDtTillStart += dt;
				float num = 1f;
				float num2 = 4f;
				if (shipFoamDecal._cumulativeDtTillStart > 0.45f)
				{
					float num3 = shipFoamDecal._cumulativeDtTillStart - 0.45f;
					num = MathF.Clamp(1f - num3 / 2.7f, 0f, 1f);
				}
				else
				{
					num = MathF.Clamp(shipFoamDecal._cumulativeDtTillStart / 0.45f, 0f, 1f);
				}
				float num4 = 0.475f;
				float alpha = MathF.Pow(num, num2) * _entityAlpha * (0.95f - num4) + num4;
				shipFoamDecal._splashFoamDecal.SetAlpha(alpha);
				ref Vec3 origin = ref shipFoamDecal._currentFrame.origin;
				origin += shipFoamDecal._currentSpeed * dt;
				Vec3 currentSpeed = shipFoamDecal._currentSpeed;
				float num5 = ((Vec3)(ref currentSpeed)).Normalize();
				num5 = MathF.Max(num5 - dt * 2.5f, 0f);
				shipFoamDecal._currentSpeed = num5 * currentSpeed;
				float num6 = MathF.Clamp(shipFoamDecal._cumulativeDtTillStart / 3.15f, 0f, 1f);
				num6 = MathF.Pow(num6, 0.4f);
				Vec3 val3 = Vec3.Lerp(val, val2, num6);
				val3.x *= shipFoamDecal._randomScale.x;
				val3.y *= shipFoamDecal._randomScale.y;
				val3.z *= shipFoamDecal._randomScale.z;
				float num7 = 3.15f;
				float num8 = MathF.Clamp(shipFoamDecal._cumulativeDtTillStart / num7, 0f, 1f);
				Vec3 s = Vec3.Slerp(shipFoamDecal._sideVectorStart, shipFoamDecal._sideVectorEnd, num8);
				((Vec3)(ref s)).Normalize();
				shipFoamDecal._currentFrame.rotation.s = s;
				shipFoamDecal._currentFrame.rotation.u = Vec3.Up;
				shipFoamDecal._currentFrame.rotation.f = -((Vec3)(ref shipFoamDecal._currentFrame.rotation.s)).CrossProductWithUp();
				((Mat3)(ref shipFoamDecal._currentFrame.rotation)).ApplyScaleLocal(ref val3);
				shipFoamDecal._splashFoamDecal.Frame = shipFoamDecal._currentFrame;
			}
			else if ((NativeObject)(object)shipFoamDecal._splashFoamDecal != (NativeObject)null)
			{
				shipFoamDecal._splashFoamDecal.SetIsVisible(false);
			}
		}
		Vec3 origin2 = globalFrame.origin;
		float num9 = ((Vec3)(ref _lastDecalSpawnPosition)).DistanceSquared(origin2);
		if (_nextDecalSpawnMetersSq < num9)
		{
			Vec3 val4 = ((Vec3)(ref globalFrame.rotation.f)).NormalizedCopy() * 0.5f;
			Vec3 s2 = globalFrame.rotation.s;
			s2.z = 0f;
			((Vec3)(ref s2)).Normalize();
			ShipFoamDecal shipFoamDecal2 = _splashFoamDecals[_nextDecalToUse];
			if ((NativeObject)(object)shipFoamDecal2._splashFoamDecal == (NativeObject)null)
			{
				Decal val5 = Decal.CreateDecal((string)null);
				val5.SetMaterial(Material.GetFromResource("decal_water_foam"));
				_ownerSceneCached.AddDecalInstance(val5, "editor_set", true);
				shipFoamDecal2._splashFoamDecal = val5;
			}
			shipFoamDecal2._splashFoamDecal.SetIsVisible(true);
			Vec3 val6 = origin2;
			val6 -= globalFrame.rotation.f * _wakeBB.max.z * 1.85f;
			float num10 = (0.5f + (MBRandom.RandomFloat - 0.5f) * 0.5f) * 0.33f;
			_nextDecalSpawnMetersSq = num10 * num10;
			Vec3 val7 = s2;
			MatrixFrame identity = MatrixFrame.Identity;
			identity.origin = val6;
			identity.rotation.u = Vec3.Up;
			Vec3 val8 = ((Mat3)(ref globalFrame.rotation)).TransformToParent(ref val7);
			val8.z = 0f;
			((Vec3)(ref val8)).Normalize();
			identity.rotation.s = val8;
			identity.rotation.f = -((Vec3)(ref identity.rotation.s)).CrossProductWithUp();
			((Vec3)(ref identity.rotation.f)).Normalize();
			shipFoamDecal2._cumulativeDtTillStart = 0f;
			float num11 = 0.6f;
			shipFoamDecal2._randomScale = Vec3.One * (0.9f + MBRandom.RandomFloat * 0.2f) * num11;
			shipFoamDecal2._randomScale.x *= 1f * MBRandom.RandomFloat + 0.4f;
			((Mat3)(ref identity.rotation)).ApplyScaleLocal(ref val);
			shipFoamDecal2._splashFoamDecal.Frame = identity;
			shipFoamDecal2._splashFoamDecal.SetAlpha(0f);
			shipFoamDecal2._currentFrame = identity;
			int num12 = MBRandom.RandomInt(3);
			float num13 = (float)(num12 % 2) * 0.5f;
			float num14 = (float)(num12 / 2) * 0.5f;
			shipFoamDecal2._splashFoamDecal.SetVectorArgument(num13, num14, -0.5f, -0.5f);
			float num15 = 0.16f * (0.8f + MBRandom.RandomFloat * 0.4f);
			float num16 = 0.45f * (0.8f + MBRandom.RandomFloat * 0.4f);
			shipFoamDecal2._currentSpeed = val4 * num16 + identity.rotation.s * ((Vec3)(ref val4)).Length * num15;
			float num17 = -0.34906584f * (0.8f + MBRandom.RandomFloat * 0.4f);
			shipFoamDecal2._sideVectorStart = val8;
			((Vec3)(ref shipFoamDecal2._sideVectorStart)).RotateAboutZ(MathF.PI / 2f);
			shipFoamDecal2._sideVectorEnd = shipFoamDecal2._sideVectorStart;
			((Vec3)(ref shipFoamDecal2._sideVectorEnd)).RotateAboutZ(num17);
			Vec2 val9 = default(Vec2);
			((Vec2)(ref val9))._002Ector(2.5f, 2.5f);
			shipFoamDecal2._splashFoamDecal.OverrideRoadBoundaryP0(val9);
			Vec2 val10 = default(Vec2);
			((Vec2)(ref val10))._002Ector(MBRandom.RandomFloat, MBRandom.RandomFloat);
			shipFoamDecal2._splashFoamDecal.OverrideRoadBoundaryP1(val10);
			_nextDecalToUse = (_nextDecalToUse + 1) % 20;
			_lastDecalSpawnPosition = origin2;
		}
	}

	private void TickSwayingAnimation(ref MatrixFrame entityFrame)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		Vec2 bearing = base.MapEntity.MobileParty.Bearing;
		float num = MBMath.WrapAngle(((Vec2)(ref bearing)).RotationInRadians - _bearingRotation);
		if (!MBMath.ApproximatelyEqualsTo(num, 0f, 0.003f))
		{
			float num2 = 0.5f;
			float num3 = 0.1f;
			if (base.MapEntity.MobileParty.TargetParty != null)
			{
				num2 = 1.5f;
				num3 = 0.01f * MBMath.Map(num, 0f, MathF.PI, 1f, 10f);
			}
			if (_swayingAngle == 0f || !MBMath.ApproximatelyEqualsTo(((CampaignVec2)(ref _targetPositionForSwaying)).Distance(base.MapEntity.MobileParty.NextTargetPosition), 0f, num2))
			{
				_swayingAngle = num;
				_targetPositionForSwaying = base.MapEntity.MobileParty.NextTargetPosition;
			}
			float num4 = ((!(_swayingAngle >= 0f)) ? MBMath.Map(num, _swayingAngle, 0f, -MathF.PI, 0f) : MBMath.Map(num, 0f, _swayingAngle, 0f, MathF.PI));
			float num5 = MBMath.Map(MathF.Abs(_swayingAngle), 0f, MathF.PI, 0f, MathF.PI / 5f);
			float num6 = MBMath.Map(MathF.Sin(num4), -1f, 1f, 0f - num5, num5);
			_rollingAngle = MBMath.LerpRadians(_rollingAngle, num6, num3, 0f, num5);
			((Mat3)(ref entityFrame.rotation)).RotateAboutForward(_rollingAngle);
		}
	}

	private void CheckBridgeFadeState()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if ((int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(base.MapEntity.MobileParty.CurrentNavigationFace) == 25)
		{
			GameEntity nearbyBridgeToParty = NavalMobilePartyVisualManager.Current.GetNearbyBridgeToParty(base.MapEntity);
			if (nearbyBridgeToParty != null)
			{
				nearbyBridgeToParty.SetAlpha(0.3f);
			}
			if (_currentCollidedBridgeEntity != nearbyBridgeToParty)
			{
				GameEntity currentCollidedBridgeEntity = _currentCollidedBridgeEntity;
				if (currentCollidedBridgeEntity != null)
				{
					currentCollidedBridgeEntity.SetAlpha(1f);
				}
				_currentCollidedBridgeEntity = nearbyBridgeToParty;
			}
		}
		else
		{
			GameEntity currentCollidedBridgeEntity2 = _currentCollidedBridgeEntity;
			if (currentCollidedBridgeEntity2 != null)
			{
				currentCollidedBridgeEntity2.SetAlpha(1f);
			}
			_currentCollidedBridgeEntity = null;
		}
	}

	private void ApplyWindEffect(Vec2 windVector, Vec2 shipDirection, float realDt, float dt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Invalid comparison between Unknown and I4
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		if (MathF.Abs(MBMath.ToDegrees(((Vec2)(ref windVector)).AngleBetween(shipDirection))) > 80f)
		{
			if (!_isSailFolded && _sailVisualCache.Count > 0)
			{
				_isSailFolded = true;
				NavalDLCViewHelpers.ShipVisualHelper.FoldSails(_sailVisualCache);
			}
		}
		else if (_isSailFolded && _sailVisualCache.Count > 0)
		{
			_isSailFolded = false;
			NavalDLCViewHelpers.ShipVisualHelper.UnfoldSails(_sailVisualCache);
		}
		if (!base.MapEntity.MobileParty.IsMainParty)
		{
			if ((int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(base.MapEntity.MobileParty.CurrentNavigationFace) == 25)
			{
				_sailAlpha = MathF.Max(_sailAlpha - MathF.Max(realDt, 1E-05f), 0.01f);
				if (_sailAlpha > 0.00999f)
				{
					foreach (SailVisual item in _sailVisualCache)
					{
						item.SetSailEntityAlpha(_sailAlpha);
					}
				}
			}
			else
			{
				_sailAlpha = MathF.Min(_sailAlpha + MathF.Max(realDt, 1E-05f), 1f);
				if (_sailAlpha < 1.00001f)
				{
					foreach (SailVisual item2 in _sailVisualCache)
					{
						item2.SetSailEntityAlpha(_sailAlpha);
					}
				}
			}
		}
		float length = ((Vec2)(ref windVector)).Length;
		Vec2 val = ((Vec2)(ref windVector)).Normalized();
		Vec3 val2 = ((Vec2)(ref val)).ToVec3(0f);
		if (_sailVisualCache.Any() && !_isSailFolded)
		{
			float num = MathF.Clamp(length * 5f, 0.5f, 2.5f);
			foreach (SailVisual item3 in _sailVisualCache)
			{
				if (item3 != null)
				{
					ClothSimulatorComponent sailClothComponent = item3.SailClothComponent;
					if (sailClothComponent != null)
					{
						sailClothComponent.SetForcedWind(val2 * num, false);
					}
				}
			}
		}
		if (!_sailVisualCache.Any())
		{
			return;
		}
		float num2 = MathF.Clamp(length * 3f, 0.3f, 2.5f);
		foreach (SailVisual item4 in _sailVisualCache)
		{
			if (item4 != null)
			{
				ClothSimulatorComponent sailTopBannerClothComponent = item4.SailTopBannerClothComponent;
				if (sailTopBannerClothComponent != null)
				{
					sailTopBannerClothComponent.SetForcedWind(val2 * num2, false);
				}
			}
		}
	}

	private void AddRaftVisual()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		_shipEntity = GameEntity.Instantiate(StrategicEntity.Scene, "raft", MatrixFrame.Identity, true, "");
		StrategicEntity.AddChild(_shipEntity, false);
		bool isMainParty = base.MapEntity.MobileParty.IsMainParty;
		_shipEntity.SetVisibilityExcludeParents(isMainParty);
		_shipEntity.SetAlpha(isMainParty ? 1f : 0f);
		_sailingSoundEvent = SoundEvent.CreateEventFromString("event:/map/army/sail", ((EntityVisualManagerBase)NavalMobilePartyVisualManager.Current).MapScene);
		_sailingSoundEvent.SetPosition(((MapEntityVisual)this).GetVisualPosition());
		_isVisualInRaftState = true;
		WeakGameEntity weakEntity = _shipEntity.WeakEntity;
		_bodyMeshEntity = ((WeakGameEntity)(ref weakEntity)).GetFirstChildEntityWithTagRecursive("body_mesh");
	}

	private void RemoveBlockadeVisuals()
	{
		if (Extensions.IsEmpty<KeyValuePair<Ship, BlockadeShipVisual>>((IEnumerable<KeyValuePair<Ship, BlockadeShipVisual>>)_shipToBlockadeShipVisualCache))
		{
			return;
		}
		foreach (KeyValuePair<Ship, BlockadeShipVisual> item in _shipToBlockadeShipVisualCache)
		{
			item.Value.ShipEntity.SetVisibilityExcludeParents(false);
			item.Value.ShipEntity.ClearComponents();
		}
		_shipToBlockadeShipVisualCache.Clear();
	}

	private bool HasNavalVisual()
	{
		if ((((List<Ship>)(object)base.MapEntity.MobileParty.Ships).Count <= 0 && !base.MapEntity.MobileParty.IsInRaftState) || !base.MapEntity.MobileParty.IsCurrentlyAtSea)
		{
			if (((List<Ship>)(object)base.MapEntity.MobileParty.Ships).Count > 0 && base.MapEntity.MobileParty.SiegeEvent?.BesiegedSettlement != null)
			{
				SiegeEvent siegeEvent = base.MapEntity.MobileParty.SiegeEvent;
				if (siegeEvent == null)
				{
					return false;
				}
				return siegeEvent.IsBlockadeActive;
			}
			return false;
		}
		return true;
	}

	private void AddVisualToVisualsOfEntities()
	{
		if (!MapScreen.VisualsOfEntities.ContainsKey(((NativeObject)StrategicEntity).Pointer))
		{
			MapScreen.VisualsOfEntities.Add(((NativeObject)StrategicEntity).Pointer, (MapEntityVisual)(object)this);
		}
	}

	private void RemoveVisualFromVisualsOfEntities()
	{
		MapScreen.VisualsOfEntities.Remove(((NativeObject)StrategicEntity).Pointer);
		foreach (GameEntity child in StrategicEntity.GetChildren())
		{
			MapScreen.VisualsOfEntities.Remove(((NativeObject)child).Pointer);
		}
	}
}
