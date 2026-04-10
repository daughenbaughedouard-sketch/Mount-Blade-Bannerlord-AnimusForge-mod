using System;
using System.Collections.Generic;
using System.Threading;
using Helpers;
using SandBox.View.Map.Managers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.ObjectSystem;

namespace SandBox.View.Map.Visuals;

public class MobilePartyVisual : MapEntityVisual<PartyBase>
{
	private const float PartyScale = 0.3f;

	private const float HorseAnimationSpeedFactor = 1.3f;

	private float _speed;

	private float _entityAlpha;

	private float _transitionStartRotation;

	private Vec2 _lastFrameVisualPositionWithoutError;

	private bool _isEntityMovingCache;

	private bool _isInTransitionProgressCached;

	private float _bearingRotation;

	private (string, GameEntityComponent) _cachedBannerComponent;

	private (string, GameEntity) _cachedBannerEntity;

	private Scene _mapScene;

	public override float BearingRotation => _bearingRotation;

	private Scene MapScene
	{
		get
		{
			if ((NativeObject)(object)_mapScene == (NativeObject)null && Campaign.Current != null && Campaign.Current.MapSceneWrapper != null)
			{
				_mapScene = ((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene;
			}
			return _mapScene;
		}
	}

	public override MapEntityVisual AttachedTo
	{
		get
		{
			MobileParty mobileParty = base.MapEntity.MobileParty;
			if (((mobileParty != null) ? mobileParty.AttachedTo : null) != null)
			{
				return MobilePartyVisualManager.Current.GetVisualOfEntity(base.MapEntity.MobileParty.AttachedTo.Party);
			}
			return null;
		}
	}

	public override CampaignVec2 InteractionPositionForPlayer => ((IInteractablePoint)base.MapEntity).GetInteractionPosition(MobileParty.MainParty);

	public override bool IsMobileEntity => base.MapEntity.IsMobile;

	public override bool IsMainEntity => base.MapEntity == PartyBase.MainParty;

	public GameEntity StrategicEntity { get; private set; }

	public AgentVisuals HumanAgentVisuals { get; private set; }

	public AgentVisuals MountAgentVisuals { get; private set; }

	public AgentVisuals CaravanMountAgentVisuals { get; private set; }

	public MobilePartyVisual(PartyBase partyBase)
		: base(partyBase)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		CircleLocalFrame = MatrixFrame.Identity;
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
			ReleaseResources();
			StrategicEntity.Remove(111);
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
		if (IsMainEntity)
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

	internal void Tick(float dt, float realDt, ref int dirtyPartiesCount, ref MobilePartyVisual[] dirtyPartiesList)
	{
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		if (StrategicEntity == (GameEntity)null)
		{
			return;
		}
		if (base.MapEntity.IsVisualDirty && (_entityAlpha > 0f || base.MapEntity.IsVisible))
		{
			int num = Interlocked.Increment(ref dirtyPartiesCount);
			dirtyPartiesList[num] = this;
		}
		if (!IsVisibleOrFadingOut() || !(StrategicEntity != (GameEntity)null) || (base.MapEntity.MobileParty.IsCurrentlyAtSea && !base.MapEntity.MobileParty.IsTransitionInProgress))
		{
			return;
		}
		UpdateBearingRotation(realDt, dt);
		_speed = (base.MapEntity.MobileParty.IsActive ? base.MapEntity.MobileParty.Speed : 0f);
		float num2 = ((MountAgentVisuals != null) ? 1.3f : 1f);
		float num3 = MathF.Min(0.25f * num2 * _speed / 0.3f, 20f);
		bool flag = IsEntityMovingVisually();
		AgentVisuals humanAgentVisuals = HumanAgentVisuals;
		if (humanAgentVisuals != null)
		{
			humanAgentVisuals.Tick(MountAgentVisuals, dt, flag, num3);
		}
		AgentVisuals mountAgentVisuals = MountAgentVisuals;
		if (mountAgentVisuals != null)
		{
			mountAgentVisuals.Tick((AgentVisuals)null, dt, flag, num3);
		}
		AgentVisuals caravanMountAgentVisuals = CaravanMountAgentVisuals;
		if (caravanMountAgentVisuals != null)
		{
			caravanMountAgentVisuals.Tick((AgentVisuals)null, dt, flag, num3);
		}
		MobileParty mobileParty = base.MapEntity.MobileParty;
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = GetVisualPosition();
		if (mobileParty.Army != null && mobileParty.AttachedTo == mobileParty.Army.LeaderParty && (base.MapEntity.MapEvent == null || !base.MapEntity.MapEvent.IsFieldBattle))
		{
			MatrixFrame frame = StrategicEntity.GetFrame();
			Vec2 val = ((Vec3)(ref identity.origin)).AsVec2 - ((Vec3)(ref frame.origin)).AsVec2;
			Vec2 val2;
			if (((Vec2)(ref val)).Length / dt > 20f)
			{
				((Mat3)(ref identity.rotation)).RotateAboutUp(_bearingRotation);
			}
			else if (mobileParty.CurrentSettlement == null)
			{
				val2 = ((Vec3)(ref frame.rotation.f)).AsVec2;
				float rotationInRadians = ((Vec2)(ref val2)).RotationInRadians;
				val2 = val + Vec2.FromRotation(_bearingRotation) * 0.01f;
				float num4 = MBMath.LerpRadians(rotationInRadians, ((Vec2)(ref val2)).RotationInRadians, Math.Min(6f * dt, 1f), 0.03f * dt, 10f * dt);
				((Mat3)(ref identity.rotation)).RotateAboutUp(num4);
			}
			else
			{
				val2 = ((Vec3)(ref frame.rotation.f)).AsVec2;
				float rotationInRadians2 = ((Vec2)(ref val2)).RotationInRadians;
				((Mat3)(ref identity.rotation)).RotateAboutUp(rotationInRadians2);
			}
		}
		else if (mobileParty.CurrentSettlement == null)
		{
			((Mat3)(ref identity.rotation)).RotateAboutUp(GetVisualRotation());
		}
		MatrixFrame frame2 = StrategicEntity.GetFrame();
		if (!((MatrixFrame)(ref frame2)).NearlyEquals(identity, 1E-05f))
		{
			StrategicEntity.SetFrame(ref identity, true);
			WeakGameEntity weakEntity;
			if (HumanAgentVisuals != null)
			{
				MatrixFrame val3 = identity;
				((Mat3)(ref val3.rotation)).ApplyScaleLocal(HumanAgentVisuals.GetScale());
				weakEntity = HumanAgentVisuals.GetWeakEntity();
				((WeakGameEntity)(ref weakEntity)).SetFrame(ref val3, true);
			}
			if (MountAgentVisuals != null)
			{
				MatrixFrame val4 = identity;
				((Mat3)(ref val4.rotation)).ApplyScaleLocal(MountAgentVisuals.GetScale());
				weakEntity = MountAgentVisuals.GetWeakEntity();
				((WeakGameEntity)(ref weakEntity)).SetFrame(ref val4, true);
			}
			if (CaravanMountAgentVisuals != null)
			{
				frame2 = CaravanMountAgentVisuals.GetFrame();
				MatrixFrame val5 = ((MatrixFrame)(ref identity)).TransformToParent(ref frame2);
				((Mat3)(ref val5.rotation)).ApplyScaleLocal(CaravanMountAgentVisuals.GetScale());
				weakEntity = CaravanMountAgentVisuals.GetWeakEntity();
				((WeakGameEntity)(ref weakEntity)).SetFrame(ref val5, true);
			}
		}
		ApplyWindEffect();
	}

	private void ApplyWindEffect()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (HumanAgentVisuals != null)
		{
			EquipmentElement val = HumanAgentVisuals.GetEquipment()[(EquipmentIndex)4];
			if (!((EquipmentElement)(ref val)).IsEmpty)
			{
				HumanAgentVisuals.SetClothWindToWeaponAtIndex(-StrategicEntity.GetGlobalFrame().rotation.f, false, (EquipmentIndex)4);
			}
		}
		ClothSimulatorComponent val2;
		if ((NativeObject)(object)_cachedBannerComponent.Item2 != (NativeObject)null && (val2 = (ClothSimulatorComponent)/*isinst with value type is only supported in some contexts*/) != null)
		{
			val2.SetForcedWind(-StrategicEntity.GetGlobalFrame().rotation.f, false);
		}
	}

	internal void OnStartup()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		if (base.MapEntity.IsMobile)
		{
			StrategicEntity = GameEntity.CreateEmpty(MapScene, true, true, true);
			if (!base.MapEntity.IsVisible)
			{
				GameEntity strategicEntity = StrategicEntity;
				strategicEntity.EntityFlags = (EntityFlags)(strategicEntity.EntityFlags | 0x20000000);
			}
		}
		CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(base.MapEntity);
		if (0 == 0)
		{
			CircleLocalFrame = MatrixFrame.Identity;
			if ((visualPartyLeader != null && ((BasicCharacterObject)visualPartyLeader).HasMount()) || base.MapEntity.MobileParty.IsCaravan)
			{
				MatrixFrame circleLocalFrame = CircleLocalFrame;
				Mat3 rotation = circleLocalFrame.rotation;
				((Mat3)(ref rotation)).ApplyScaleLocal(0.4625f);
				circleLocalFrame.rotation = rotation;
				CircleLocalFrame = circleLocalFrame;
			}
			else
			{
				MatrixFrame circleLocalFrame2 = CircleLocalFrame;
				Mat3 rotation2 = circleLocalFrame2.rotation;
				((Mat3)(ref rotation2)).ApplyScaleLocal(0.3725f);
				circleLocalFrame2.rotation = rotation2;
				CircleLocalFrame = circleLocalFrame2;
			}
		}
		Vec2 bearing = base.MapEntity.MobileParty.Bearing;
		_bearingRotation = ((Vec2)(ref bearing)).RotationInRadians;
		StrategicEntity.SetVisibilityExcludeParents(base.MapEntity.IsVisible);
		if (HumanAgentVisuals != null)
		{
			WeakGameEntity weakEntity = HumanAgentVisuals.GetWeakEntity();
			if (weakEntity != WeakGameEntity.Invalid)
			{
				((WeakGameEntity)(ref weakEntity)).SetVisibilityExcludeParents(base.MapEntity.IsVisible);
			}
		}
		if (MountAgentVisuals != null)
		{
			WeakGameEntity weakEntity2 = MountAgentVisuals.GetWeakEntity();
			if (weakEntity2 != WeakGameEntity.Invalid)
			{
				((WeakGameEntity)(ref weakEntity2)).SetVisibilityExcludeParents(base.MapEntity.IsVisible);
			}
		}
		if (CaravanMountAgentVisuals != null)
		{
			WeakGameEntity weakEntity3 = CaravanMountAgentVisuals.GetWeakEntity();
			if (weakEntity3 != WeakGameEntity.Invalid)
			{
				((WeakGameEntity)(ref weakEntity3)).SetVisibilityExcludeParents(base.MapEntity.IsVisible);
			}
		}
		StrategicEntity.SetReadyToRender(true);
		StrategicEntity.SetEntityEnvMapVisibility(false);
		_entityAlpha = 0f;
		if (base.MapEntity.IsVisible)
		{
			if (base.MapEntity.MobileParty.IsTransitionInProgress)
			{
				TickFadingState(0.1f, 0.1f);
			}
			else
			{
				_entityAlpha = 1f;
			}
		}
		AddVisualToVisualsOfEntities();
	}

	internal void TickFadingState(float realDt, float dt)
	{
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		if ((!base.MapEntity.MobileParty.IsTransitionInProgress || !base.MapEntity.IsVisible) && ((_entityAlpha < 1f && base.MapEntity.IsVisible) || (_entityAlpha > 0f && !base.MapEntity.IsVisible)))
		{
			if (base.MapEntity.IsVisible)
			{
				if (_entityAlpha <= 0f)
				{
					StrategicEntity.SetVisibilityExcludeParents(true);
					if (HumanAgentVisuals != null)
					{
						WeakGameEntity weakEntity = HumanAgentVisuals.GetWeakEntity();
						if (weakEntity != WeakGameEntity.Invalid)
						{
							((WeakGameEntity)(ref weakEntity)).SetVisibilityExcludeParents(true);
						}
					}
					if (MountAgentVisuals != null)
					{
						WeakGameEntity weakEntity2 = MountAgentVisuals.GetWeakEntity();
						if (weakEntity2 != WeakGameEntity.Invalid)
						{
							((WeakGameEntity)(ref weakEntity2)).SetVisibilityExcludeParents(true);
						}
					}
					if (CaravanMountAgentVisuals != null)
					{
						WeakGameEntity weakEntity3 = CaravanMountAgentVisuals.GetWeakEntity();
						if (weakEntity3 != WeakGameEntity.Invalid)
						{
							((WeakGameEntity)(ref weakEntity3)).SetVisibilityExcludeParents(true);
						}
					}
				}
				_entityAlpha = MathF.Min(_entityAlpha + MathF.Max(realDt, 1E-05f), 1f);
				StrategicEntity.SetAlpha(_entityAlpha);
				if (HumanAgentVisuals != null)
				{
					WeakGameEntity weakEntity4 = HumanAgentVisuals.GetWeakEntity();
					if (weakEntity4 != WeakGameEntity.Invalid)
					{
						((WeakGameEntity)(ref weakEntity4)).SetAlpha(_entityAlpha);
					}
				}
				if (MountAgentVisuals != null)
				{
					WeakGameEntity weakEntity5 = MountAgentVisuals.GetWeakEntity();
					if (weakEntity5 != WeakGameEntity.Invalid)
					{
						((WeakGameEntity)(ref weakEntity5)).SetAlpha(_entityAlpha);
					}
				}
				if (CaravanMountAgentVisuals != null)
				{
					WeakGameEntity weakEntity6 = CaravanMountAgentVisuals.GetWeakEntity();
					if (weakEntity6 != WeakGameEntity.Invalid)
					{
						((WeakGameEntity)(ref weakEntity6)).SetAlpha(_entityAlpha);
					}
				}
				GameEntity strategicEntity = StrategicEntity;
				strategicEntity.EntityFlags = (EntityFlags)(strategicEntity.EntityFlags & -536870913);
				return;
			}
			_entityAlpha = MathF.Max(_entityAlpha - MathF.Max(realDt, 1E-05f), 0f);
			StrategicEntity.SetAlpha(_entityAlpha);
			if (HumanAgentVisuals != null)
			{
				WeakGameEntity weakEntity7 = HumanAgentVisuals.GetWeakEntity();
				if (weakEntity7 != WeakGameEntity.Invalid)
				{
					((WeakGameEntity)(ref weakEntity7)).SetAlpha(_entityAlpha);
				}
			}
			if (MountAgentVisuals != null)
			{
				WeakGameEntity weakEntity8 = MountAgentVisuals.GetWeakEntity();
				if (weakEntity8 != WeakGameEntity.Invalid)
				{
					((WeakGameEntity)(ref weakEntity8)).SetAlpha(_entityAlpha);
				}
			}
			if (CaravanMountAgentVisuals != null)
			{
				WeakGameEntity weakEntity9 = CaravanMountAgentVisuals.GetWeakEntity();
				if (weakEntity9 != WeakGameEntity.Invalid)
				{
					((WeakGameEntity)(ref weakEntity9)).SetAlpha(_entityAlpha);
				}
			}
			if (!(_entityAlpha <= 0f))
			{
				return;
			}
			StrategicEntity.SetVisibilityExcludeParents(false);
			if (HumanAgentVisuals != null)
			{
				WeakGameEntity weakEntity10 = HumanAgentVisuals.GetWeakEntity();
				if (weakEntity10 != WeakGameEntity.Invalid)
				{
					((WeakGameEntity)(ref weakEntity10)).SetVisibilityExcludeParents(false);
				}
			}
			if (MountAgentVisuals != null)
			{
				WeakGameEntity weakEntity11 = MountAgentVisuals.GetWeakEntity();
				if (weakEntity11 != WeakGameEntity.Invalid)
				{
					((WeakGameEntity)(ref weakEntity11)).SetVisibilityExcludeParents(false);
				}
			}
			if (CaravanMountAgentVisuals != null)
			{
				WeakGameEntity weakEntity12 = CaravanMountAgentVisuals.GetWeakEntity();
				if (weakEntity12 != WeakGameEntity.Invalid)
				{
					((WeakGameEntity)(ref weakEntity12)).SetVisibilityExcludeParents(false);
				}
			}
			GameEntity strategicEntity2 = StrategicEntity;
			strategicEntity2.EntityFlags = (EntityFlags)(strategicEntity2.EntityFlags | 0x20000000);
		}
		else if (base.MapEntity.MobileParty.IsTransitionInProgress)
		{
			if ((base.MapEntity.MobileParty.Army == null || base.MapEntity.MobileParty.Army.LeaderParty == base.MapEntity.MobileParty || base.MapEntity.MobileParty.AttachedTo == null) && IsMobileEntity && GetTransitionProgress() < 1f)
			{
				TickTransitionFadeState(dt);
			}
		}
		else
		{
			MobilePartyVisualManager.Current.UnRegisterFadingVisual(this);
		}
	}

	private void UpdateBearingRotation(float realDt, float dt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vec2 bearing = base.MapEntity.MobileParty.Bearing;
		float num = MBMath.WrapAngle(((Vec2)(ref bearing)).RotationInRadians - _bearingRotation);
		float num2 = ((base.MapEntity.MapEvent != null) ? realDt : dt);
		_bearingRotation += num * MathF.Min(num2 * 30f, 1f);
		_bearingRotation = MBMath.WrapAngle(_bearingRotation);
	}

	private void TickTransitionFadeState(float dt)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		float transitionProgress = GetTransitionProgress();
		if (base.MapEntity.MobileParty.IsCurrentlyAtSea)
		{
			_entityAlpha = transitionProgress;
			AgentVisuals humanAgentVisuals = HumanAgentVisuals;
			if (humanAgentVisuals != null)
			{
				GameEntity entity = humanAgentVisuals.GetEntity();
				if (entity != null)
				{
					entity.SetAlpha(_entityAlpha);
				}
			}
			AgentVisuals mountAgentVisuals = MountAgentVisuals;
			if (mountAgentVisuals != null)
			{
				GameEntity entity2 = mountAgentVisuals.GetEntity();
				if (entity2 != null)
				{
					entity2.SetAlpha(_entityAlpha);
				}
			}
			AgentVisuals caravanMountAgentVisuals = CaravanMountAgentVisuals;
			if (caravanMountAgentVisuals != null)
			{
				GameEntity entity3 = caravanMountAgentVisuals.GetEntity();
				if (entity3 != null)
				{
					entity3.SetAlpha(_entityAlpha);
				}
			}
			if (HumanAgentVisuals == null)
			{
				return;
			}
			MatrixFrame frame = HumanAgentVisuals.GetEntity().GetFrame();
			CampaignVec2 val = base.MapEntity.MobileParty.EndPositionForNavigationTransition + base.MapEntity.MobileParty.ArmyPositionAdder;
			float num = MathF.Lerp(((Vec3)(ref frame.origin)).X, ((CampaignVec2)(ref val)).X, dt, 1E-05f);
			float num2 = MathF.Lerp(((Vec3)(ref frame.origin)).Y, ((CampaignVec2)(ref val)).Y, dt, 1E-05f);
			float z = frame.origin.z;
			Vec3 val2 = ((CampaignVec2)(ref val)).AsVec3();
			float num3 = MathF.Lerp(z, ((Vec3)(ref val2)).Z, dt, 1E-05f);
			frame.origin = new Vec3(num, num2, num3, -1f);
			GameEntity entity4 = HumanAgentVisuals.GetEntity();
			if (entity4 != null)
			{
				entity4.SetFrame(ref frame, false);
			}
			AgentVisuals mountAgentVisuals2 = MountAgentVisuals;
			if (mountAgentVisuals2 != null)
			{
				GameEntity entity5 = mountAgentVisuals2.GetEntity();
				if (entity5 != null)
				{
					entity5.SetFrame(ref frame, false);
				}
			}
			AgentVisuals caravanMountAgentVisuals2 = CaravanMountAgentVisuals;
			if (caravanMountAgentVisuals2 != null)
			{
				GameEntity entity6 = caravanMountAgentVisuals2.GetEntity();
				if (entity6 != null)
				{
					entity6.SetFrame(ref frame, false);
				}
			}
			return;
		}
		_entityAlpha = 1f - transitionProgress;
		AgentVisuals humanAgentVisuals2 = HumanAgentVisuals;
		if (humanAgentVisuals2 != null)
		{
			GameEntity entity7 = humanAgentVisuals2.GetEntity();
			if (entity7 != null)
			{
				entity7.SetAlpha(_entityAlpha);
			}
		}
		AgentVisuals mountAgentVisuals3 = MountAgentVisuals;
		if (mountAgentVisuals3 != null)
		{
			GameEntity entity8 = mountAgentVisuals3.GetEntity();
			if (entity8 != null)
			{
				entity8.SetAlpha(_entityAlpha);
			}
		}
		AgentVisuals caravanMountAgentVisuals3 = CaravanMountAgentVisuals;
		if (caravanMountAgentVisuals3 != null)
		{
			GameEntity entity9 = caravanMountAgentVisuals3.GetEntity();
			if (entity9 != null)
			{
				entity9.SetAlpha(_entityAlpha);
			}
		}
	}

	internal void ValidateIsDirty()
	{
		if (base.MapEntity.MemberRoster.TotalManCount != 0)
		{
			RefreshPartyIcon();
			if ((_entityAlpha < 1f && base.MapEntity.IsVisible) || (_entityAlpha > 0f && !base.MapEntity.IsVisible))
			{
				MobilePartyVisualManager.Current.RegisterFadingVisual(this);
			}
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
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Expected O, but got Unknown
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		if (!base.MapEntity.IsVisualDirty)
		{
			return;
		}
		base.MapEntity.OnVisualsUpdated();
		bool clearBannerComponentCache = true;
		bool clearBannerEntityCache = true;
		ResetPartyIcon();
		MatrixFrame circleLocalFrame = CircleLocalFrame;
		circleLocalFrame.origin = Vec3.Zero;
		CircleLocalFrame = circleLocalFrame;
		MobileParty mobileParty = base.MapEntity.MobileParty;
		if (((mobileParty != null) ? mobileParty.CurrentSettlement : null) != null)
		{
			AddVisualToVisualsOfEntities();
			if (!base.MapEntity.MobileParty.MapFaction.IsAtWarWith(base.MapEntity.MobileParty.CurrentSettlement.MapFaction))
			{
				Hero leaderHero = base.MapEntity.LeaderHero;
				if (((leaderHero != null) ? leaderHero.ClanBanner : null) != null)
				{
					string bannerCode = base.MapEntity.LeaderHero.ClanBanner.BannerCode;
					if (!string.IsNullOrEmpty(bannerCode))
					{
						MatrixFrame val = MatrixFrame.Identity;
						Vec3 bannerPositionForParty = SettlementVisualManager.Current.GetSettlementVisual(base.MapEntity.MobileParty.CurrentSettlement).GetBannerPositionForParty(base.MapEntity.MobileParty);
						if (((Vec3)(ref bannerPositionForParty)).IsValid)
						{
							val.origin = bannerPositionForParty;
							MatrixFrame globalFrame = StrategicEntity.GetGlobalFrame();
							val.origin = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref val.origin);
							float num = MBMath.Map((float)base.MapEntity.NumberOfAllMembers / 400f * ((base.MapEntity.MobileParty.Army != null && base.MapEntity.MobileParty.Army.LeaderParty == base.MapEntity.MobileParty) ? 1.25f : 1f), 0f, 1f, 0.2f, 0.5f);
							val = ((MatrixFrame)(ref val)).Elevate(0f - num);
							((Mat3)(ref val.rotation)).ApplyScaleLocal(num);
							globalFrame = StrategicEntity.GetGlobalFrame();
							val.rotation = ((Mat3)(ref globalFrame.rotation)).TransformToLocal(ref val.rotation);
							GameEntityPhysicsExtensions.AddSphereAsBody(StrategicEntity, val.origin + Vec3.Up * 0.3f, 0.15f, (BodyFlags)0);
							clearBannerComponentCache = false;
							string text = "campaign_flag";
							if (_cachedBannerComponent.Item1 == bannerCode + text)
							{
								_cachedBannerComponent.Item2.GetFirstMetaMesh().Frame = val;
								StrategicEntity.AddComponent(_cachedBannerComponent.Item2);
							}
							else
							{
								MetaMesh bannerOfCharacter = GetBannerOfCharacter(new Banner(bannerCode), text);
								bannerOfCharacter.Frame = val;
								int componentCount = StrategicEntity.GetComponentCount((ComponentType)3);
								StrategicEntity.AddMultiMesh(bannerOfCharacter, true);
								if (StrategicEntity.GetComponentCount((ComponentType)3) > componentCount)
								{
									_cachedBannerComponent.Item1 = bannerCode + text;
									_cachedBannerComponent.Item2 = StrategicEntity.GetComponentAtIndex(componentCount, (ComponentType)3);
								}
							}
						}
					}
					goto IL_03fb;
				}
			}
			GameEntityPhysicsExtensions.RemovePhysics(StrategicEntity, false);
		}
		else if (base.MapEntity.MobileParty != null && (base.MapEntity.MobileParty.IsCurrentlyAtSea || base.MapEntity.MobileParty.IsTransitionInProgress))
		{
			RemoveVisualFromVisualsOfEntities();
			if (base.MapEntity.MobileParty.IsTransitionInProgress)
			{
				if (base.MapEntity.MobileParty.Army == null || base.MapEntity.MobileParty.Army.LeaderParty == base.MapEntity.MobileParty || base.MapEntity.MobileParty.AttachedTo == null)
				{
					AddMobileIconComponents(base.MapEntity, ref clearBannerComponentCache, ref clearBannerEntityCache);
				}
				if (!_isInTransitionProgressCached)
				{
					AddVisualToVisualsOfEntities();
					OnTransitionStarted();
				}
			}
			if (base.MapEntity.MobileParty.IsTransitionInProgress != _isInTransitionProgressCached)
			{
				if (_isInTransitionProgressCached)
				{
					OnTransitionEnded();
				}
				else
				{
					OnTransitionStarted();
				}
			}
		}
		else
		{
			AddVisualToVisualsOfEntities();
			InitializePartyCollider(base.MapEntity);
			AddMobileIconComponents(base.MapEntity, ref clearBannerComponentCache, ref clearBannerEntityCache);
		}
		goto IL_03fb;
		IL_03fb:
		if (clearBannerComponentCache)
		{
			_cachedBannerComponent = (null, null);
		}
		if (clearBannerEntityCache)
		{
			_cachedBannerEntity = (null, null);
		}
		StrategicEntity.CheckResources(true, false);
		if (IsMobileEntity)
		{
			_isInTransitionProgressCached = base.MapEntity.MobileParty.IsTransitionInProgress;
		}
	}

	private void AddMobileIconComponents(PartyBase party, ref bool clearBannerComponentCache, ref bool clearBannerEntityCache)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Invalid comparison between Unknown and I4
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Invalid comparison between Unknown and I4
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Invalid comparison between Unknown and I4
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Invalid comparison between Unknown and I4
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		uint contourColor = (FactionManager.IsAtWarAgainstFaction(party.MapFaction, Hero.MainHero.MapFaction) ? 4294905856u : 4278206719u);
		if (IsPartOfBesiegerCamp(party))
		{
			AddTentEntityForParty(StrategicEntity, party, ref clearBannerComponentCache);
		}
		else
		{
			if (PartyBaseHelper.GetVisualPartyLeader(party) == null)
			{
				return;
			}
			string bannerKey = null;
			Hero leaderHero = party.LeaderHero;
			if (((leaderHero != null) ? leaderHero.ClanBanner : null) != null)
			{
				bannerKey = party.LeaderHero.ClanBanner.BannerCode;
			}
			ActionIndexCache leaderAction = ActionIndexCache.act_none;
			ActionIndexCache mountAction = ActionIndexCache.act_none;
			MapEvent val = ((party.MobileParty.Army != null && party.MobileParty.Army.DoesLeaderPartyAndAttachedPartiesContain(party.MobileParty)) ? party.MobileParty.Army.LeaderParty.MapEvent : party.MapEvent);
			GetMeleeWeaponToWield(party, out var wieldedItemIndex);
			if (val != null && ((int)val.EventType == 1 || (int)val.EventType == 2 || (int)val.EventType == 8 || (int)val.EventType == 7))
			{
				GetPartyBattleAnimation(party, wieldedItemIndex, out leaderAction, out mountAction);
			}
			IFaction mapFaction = party.MapFaction;
			uint teamColor = ((mapFaction != null) ? mapFaction.Color : 4291609515u);
			IFaction mapFaction2 = party.MapFaction;
			uint teamColor2 = ((mapFaction2 != null) ? mapFaction2.Color2 : 4291609515u);
			AddCharacterToPartyIcon(party, PartyBaseHelper.GetVisualPartyLeader(party), contourColor, bannerKey, wieldedItemIndex, teamColor, teamColor2, in leaderAction, in mountAction, MBRandom.NondeterministicRandomFloat * 0.7f, ref clearBannerEntityCache);
			if (party.IsMobile)
			{
				GetMountAndHarnessVisualIdsForPartyIcon(out var mountStringId, out var harnessStringId);
				if (!string.IsNullOrEmpty(mountStringId))
				{
					AddMountToPartyIcon(new Vec3(0.3f, -0.25f, 0f, -1f), mountStringId, harnessStringId, contourColor, PartyBaseHelper.GetVisualPartyLeader(party));
				}
			}
		}
	}

	private void AddMountToPartyIcon(Vec3 positionOffset, string mountItemId, string harnessItemId, uint contourColor, CharacterObject character)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		ItemObject val = Game.Current.ObjectManager.GetObject<ItemObject>(mountItemId);
		Monster monster = val.HorseComponent.Monster;
		ItemObject val2 = null;
		if (!string.IsNullOrEmpty(harnessItemId))
		{
			val2 = Game.Current.ObjectManager.GetObject<ItemObject>(harnessItemId);
		}
		Equipment val3 = new Equipment();
		val3[(EquipmentIndex)10] = new EquipmentElement(val, (ItemModifier)null, (ItemObject)null, false);
		val3[(EquipmentIndex)11] = new EquipmentElement(val2, (ItemModifier)null, (ItemObject)null, false);
		AgentVisualsData obj = new AgentVisualsData().Equipment(val3).Scale(val.ScaleFactor * 0.3f);
		Mat3 identity = Mat3.Identity;
		AgentVisualsData val4 = obj.Frame(new MatrixFrame(ref identity, ref positionOffset)).ActionSet(MBGlobals.GetActionSet(monster.ActionSetCode + "_map")).Scene(MapScene)
			.Monster(monster)
			.PrepareImmediately(false)
			.UseScaledWeapons(true)
			.HasClippingPlane(true)
			.MountCreationKey(MountCreationKey.GetRandomMountKeyString(val, ((BasicCharacterObject)character).GetMountKeySeed()));
		CaravanMountAgentVisuals = AgentVisuals.Create(val4, "PartyIcon " + mountItemId, false, false, false);
		CaravanMountAgentVisuals.GetEntity().SetContourColor((uint?)contourColor, false);
		MatrixFrame val5 = CaravanMountAgentVisuals.GetFrame();
		((Mat3)(ref val5.rotation)).ApplyScaleLocal(CaravanMountAgentVisuals.GetScale());
		MatrixFrame frame = StrategicEntity.GetFrame();
		val5 = ((MatrixFrame)(ref frame)).TransformToParent(ref val5);
		CaravanMountAgentVisuals.GetEntity().SetFrame(ref val5, true);
		float num = MathF.Min(0.325f * _speed / 0.3f, 20f);
		CaravanMountAgentVisuals.Tick((AgentVisuals)null, 0.0001f, IsEntityMovingVisually(), num);
		CaravanMountAgentVisuals.GetEntity().Skeleton.ForceUpdateBoneFrames();
	}

	private void AddCharacterToPartyIcon(PartyBase party, CharacterObject characterObject, uint contourColor, string bannerKey, int wieldedItemIndex, uint teamColor1, uint teamColor2, in ActionIndexCache leaderAction, in ActionIndexCache mountAction, float animationStartDuration, ref bool clearBannerEntityCache)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		Equipment val = ((BasicCharacterObject)characterObject).Equipment.Clone(false);
		bool flag = !string.IsNullOrEmpty(bannerKey) && (((((BasicCharacterObject)characterObject).IsPlayerCharacter || characterObject.HeroObject.Clan == Clan.PlayerClan) && Clan.PlayerClan.Tier >= Campaign.Current.Models.ClanTierModel.BannerEligibleTier) || (!((BasicCharacterObject)characterObject).IsPlayerCharacter && (!((BasicCharacterObject)characterObject).IsHero || (((BasicCharacterObject)characterObject).IsHero && characterObject.HeroObject.Clan != Clan.PlayerClan))));
		int num = 4;
		if (flag)
		{
			ItemObject val2 = Game.Current.ObjectManager.GetObject<ItemObject>("campaign_banner_small");
			val[(EquipmentIndex)4] = new EquipmentElement(val2, (ItemModifier)null, (ItemObject)null, false);
		}
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(((BasicCharacterObject)characterObject).Race);
		MBActionSet actionSetWithSuffix = MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, ((BasicCharacterObject)characterObject).IsFemale, flag ? "_map_with_banner" : "_map");
		AgentVisualsData val3 = new AgentVisualsData().UseMorphAnims(true).Equipment(val).BodyProperties(((BasicCharacterObject)characterObject).GetBodyProperties(((BasicCharacterObject)characterObject).Equipment, -1))
			.SkeletonType((SkeletonType)(((BasicCharacterObject)characterObject).IsFemale ? 1 : 0))
			.Scale(0.3f)
			.Frame(StrategicEntity.GetFrame())
			.ActionSet(actionSetWithSuffix)
			.Scene(MapScene)
			.Monster(baseMonsterFromRace)
			.PrepareImmediately(false)
			.RightWieldedItemIndex(wieldedItemIndex)
			.HasClippingPlane(true)
			.UseScaledWeapons(true)
			.ClothColor1(teamColor1)
			.ClothColor2(teamColor2)
			.CharacterObjectStringId(((MBObjectBase)characterObject).StringId)
			.AddColorRandomness(!((BasicCharacterObject)characterObject).IsHero)
			.Race(((BasicCharacterObject)characterObject).Race);
		if (flag)
		{
			Banner val4 = new Banner(bannerKey);
			val3.Banner(val4).LeftWieldedItemIndex(num);
			if (_cachedBannerEntity.Item1 == bannerKey + "campaign_banner_small")
			{
				val3.CachedWeaponEntity((EquipmentIndex)4, _cachedBannerEntity.Item2);
			}
		}
		if (!party.MobileParty.IsCurrentlyAtSea || party.MobileParty.IsTransitionInProgress)
		{
			HumanAgentVisuals = AgentVisuals.Create(val3, "PartyIcon " + ((BasicCharacterObject)characterObject).Name, false, false, false);
		}
		if (HumanAgentVisuals != null)
		{
			if (flag)
			{
				GameEntity entity = HumanAgentVisuals.GetEntity();
				GameEntity child = entity.GetChild(entity.ChildCount - 1);
				if (child.GetComponentCount((ComponentType)3) > 0)
				{
					clearBannerEntityCache = false;
					_cachedBannerEntity = (bannerKey + "campaign_banner_small", child);
				}
			}
			if (leaderAction != ActionIndexCache.act_none)
			{
				float actionAnimationDuration = MBActionSet.GetActionAnimationDuration(actionSetWithSuffix, ref leaderAction);
				if (actionAnimationDuration < 1f)
				{
					MBSkeletonExtensions.SetAgentActionChannel(HumanAgentVisuals.GetVisuals().GetSkeleton(), 0, ref leaderAction, animationStartDuration, -0.2f, true, 0f);
				}
				else
				{
					MBSkeletonExtensions.SetAgentActionChannel(HumanAgentVisuals.GetVisuals().GetSkeleton(), 0, ref leaderAction, animationStartDuration / actionAnimationDuration, -0.2f, true, 0f);
				}
			}
		}
		WeakGameEntity weakEntity;
		if (((BasicCharacterObject)characterObject).HasMount() && (!party.MobileParty.IsCurrentlyAtSea || party.MobileParty.IsTransitionInProgress))
		{
			EquipmentElement val5 = ((BasicCharacterObject)characterObject).Equipment[(EquipmentIndex)10];
			Monster monster = ((EquipmentElement)(ref val5)).Item.HorseComponent.Monster;
			MBActionSet actionSet = MBGlobals.GetActionSet(monster.ActionSetCode + "_map");
			AgentVisualsData obj = new AgentVisualsData().Equipment(((BasicCharacterObject)characterObject).Equipment);
			val5 = ((BasicCharacterObject)characterObject).Equipment[(EquipmentIndex)10];
			AgentVisualsData obj2 = obj.Scale(((EquipmentElement)(ref val5)).Item.ScaleFactor * 0.3f).Frame(MatrixFrame.Identity).ActionSet(actionSet)
				.Scene(MapScene)
				.Monster(monster)
				.PrepareImmediately(false)
				.UseScaledWeapons(true)
				.HasClippingPlane(true);
			val5 = ((BasicCharacterObject)characterObject).Equipment[(EquipmentIndex)10];
			AgentVisualsData val6 = obj2.MountCreationKey(MountCreationKey.GetRandomMountKeyString(((EquipmentElement)(ref val5)).Item, ((BasicCharacterObject)characterObject).GetMountKeySeed()));
			MountAgentVisuals = AgentVisuals.Create(val6, string.Concat("PartyIcon ", ((BasicCharacterObject)characterObject).Name, " mount"), false, false, false);
			if (mountAction != ActionIndexCache.act_none)
			{
				float actionAnimationDuration2 = MBActionSet.GetActionAnimationDuration(actionSet, ref mountAction);
				if (actionAnimationDuration2 < 1f)
				{
					weakEntity = MountAgentVisuals.GetWeakEntity();
					MBSkeletonExtensions.SetAgentActionChannel(((WeakGameEntity)(ref weakEntity)).Skeleton, 0, ref mountAction, animationStartDuration, -0.2f, true, 0f);
				}
				else
				{
					weakEntity = MountAgentVisuals.GetWeakEntity();
					MBSkeletonExtensions.SetAgentActionChannel(((WeakGameEntity)(ref weakEntity)).Skeleton, 0, ref mountAction, animationStartDuration / actionAnimationDuration2, -0.2f, true, 0f);
				}
			}
			weakEntity = MountAgentVisuals.GetWeakEntity();
			((WeakGameEntity)(ref weakEntity)).SetContourColor((uint?)contourColor, false);
			MatrixFrame frame = StrategicEntity.GetFrame();
			((Mat3)(ref frame.rotation)).ApplyScaleLocal(val6.ScaleData);
			weakEntity = MountAgentVisuals.GetWeakEntity();
			((WeakGameEntity)(ref weakEntity)).SetFrame(ref frame, true);
		}
		float num2 = ((MountAgentVisuals != null) ? 1.3f : 1f);
		float num3 = MathF.Min(0.25f * num2 * _speed / 0.3f, 20f);
		if (MountAgentVisuals != null)
		{
			MountAgentVisuals.Tick((AgentVisuals)null, 0.0001f, IsEntityMovingVisually(), num3);
			weakEntity = MountAgentVisuals.GetWeakEntity();
			((WeakGameEntity)(ref weakEntity)).Skeleton.ForceUpdateBoneFrames();
		}
		if (HumanAgentVisuals != null)
		{
			WeakGameEntity weakEntity2 = HumanAgentVisuals.GetWeakEntity();
			((WeakGameEntity)(ref weakEntity2)).SetContourColor((uint?)contourColor, false);
			MatrixFrame frame2 = StrategicEntity.GetFrame();
			((Mat3)(ref frame2.rotation)).ApplyScaleLocal(val3.ScaleData);
			((WeakGameEntity)(ref weakEntity2)).SetFrame(ref frame2, true);
			HumanAgentVisuals.Tick(MountAgentVisuals, 0.0001f, IsEntityMovingVisually(), num3);
			((WeakGameEntity)(ref weakEntity2)).Skeleton.ForceUpdateBoneFrames();
		}
	}

	private bool IsEntityMovingVisually()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (base.MapEntity.IsMobile && base.MapEntity.MapEvent != null)
		{
			_isEntityMovingCache = false;
		}
		else
		{
			if (!(Campaign.Current.CampaignDt > 0f))
			{
				MobileParty mobileParty = base.MapEntity.MobileParty;
				if (mobileParty == null || !mobileParty.IsMainParty || !Campaign.Current.IsMainPartyWaiting)
				{
					goto IL_00af;
				}
			}
			_isEntityMovingCache = false;
			MobileParty mobileParty2 = base.MapEntity.MobileParty;
			if (mobileParty2 != null)
			{
				Vec2 visualPosition2DWithoutError = mobileParty2.VisualPosition2DWithoutError;
				if (!((Vec2)(ref visualPosition2DWithoutError)).NearlyEquals(_lastFrameVisualPositionWithoutError, 1E-05f))
				{
					_lastFrameVisualPositionWithoutError = base.MapEntity.MobileParty.VisualPosition2DWithoutError;
					_isEntityMovingCache = true;
				}
			}
		}
		goto IL_00af;
		IL_00af:
		if (_isInTransitionProgressCached)
		{
			_isEntityMovingCache = true;
		}
		return _isEntityMovingCache;
	}

	public static MetaMesh GetBannerOfCharacter(Banner banner, string bannerMeshName)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		MetaMesh copy = MetaMesh.GetCopy(bannerMeshName, true, false);
		for (int i = 0; i < copy.MeshCount; i++)
		{
			Mesh meshAtIndex = copy.GetMeshAtIndex(i);
			if (meshAtIndex.HasTag("dont_use_tableau"))
			{
				continue;
			}
			Material material = meshAtIndex.GetMaterial();
			Material tableauMaterial = null;
			Tuple<Material, Banner> key = new Tuple<Material, Banner>(material, banner);
			if (MapScreen.Instance.CharacterBannerMaterialCache.ContainsKey(key))
			{
				tableauMaterial = MapScreen.Instance.CharacterBannerMaterialCache[key];
			}
			else
			{
				tableauMaterial = material.CreateCopy();
				Action<Texture> action = delegate(Texture tex)
				{
					tableauMaterial.SetTexture((MBTextureType)1, tex);
					uint num = (uint)tableauMaterial.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
					ulong shaderFlags = tableauMaterial.GetShaderFlags();
					tableauMaterial.SetShaderFlags(shaderFlags | num);
				};
				BannerDebugInfo val = BannerDebugInfo.CreateManual("MobilePartyVisual");
				BannerVisualExtensions.GetTableauTextureLarge(banner, ref val, action);
				MapScreen.Instance.CharacterBannerMaterialCache[key] = tableauMaterial;
			}
			meshAtIndex.SetMaterial(tableauMaterial);
		}
		return copy;
	}

	public void AddTentEntityForParty(GameEntity strategicEntity, PartyBase party, ref bool clearBannerComponentCache)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = GameEntity.CreateEmpty(strategicEntity.Scene, true, true, true);
		val.AddMultiMesh(MetaMesh.GetCopy("map_icon_siege_camp_tent", true, false), true);
		MatrixFrame identity = MatrixFrame.Identity;
		((Mat3)(ref identity.rotation)).ApplyScaleLocal(1.2f);
		val.SetFrame(ref identity, true);
		string text = null;
		Hero leaderHero = party.LeaderHero;
		if (((leaderHero != null) ? leaderHero.ClanBanner : null) != null)
		{
			text = party.LeaderHero.ClanBanner.BannerCode;
		}
		bool flag = party.MobileParty.Army != null && party.MobileParty.Army.LeaderParty == party.MobileParty;
		MatrixFrame identity2 = MatrixFrame.Identity;
		identity2.origin.z += (flag ? 0.2f : 0.15f);
		float num = MBMath.Map(party.CalculateCurrentStrength() / 500f * ((party.MobileParty.Army != null && flag) ? 1f : 0.8f), 0f, 1f, 0.15f, 0.5f);
		((Mat3)(ref identity2.rotation)).ApplyScaleLocal(num);
		if (!string.IsNullOrEmpty(text))
		{
			clearBannerComponentCache = false;
			string text2 = "campaign_flag";
			if (_cachedBannerComponent.Item1 == text + text2)
			{
				_cachedBannerComponent.Item2.GetFirstMetaMesh().Frame = identity2;
				val.AddComponent(_cachedBannerComponent.Item2);
			}
			else
			{
				MetaMesh bannerOfCharacter = GetBannerOfCharacter(new Banner(text), text2);
				bannerOfCharacter.Frame = identity2;
				int componentCount = val.GetComponentCount((ComponentType)3);
				val.AddMultiMesh(bannerOfCharacter, true);
				if (val.GetComponentCount((ComponentType)3) > componentCount)
				{
					_cachedBannerComponent.Item1 = text + text2;
					_cachedBannerComponent.Item2 = val.GetComponentAtIndex(componentCount, (ComponentType)3);
				}
			}
		}
		strategicEntity.AddChild(val, false);
		val.SetVisibilityExcludeParents(true);
	}

	internal void ClearVisualMemory()
	{
		ResetPartyIcon();
		base.MapEntity.SetVisualAsDirty();
		_cachedBannerEntity = (null, null);
	}

	private void GetMeleeWeaponToWield(PartyBase party, out int wieldedItemIndex)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		wieldedItemIndex = -1;
		CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(party);
		if (visualPartyLeader == null)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			EquipmentElement val = ((BasicCharacterObject)visualPartyLeader).Equipment[i];
			if (((EquipmentElement)(ref val)).Item != null)
			{
				val = ((BasicCharacterObject)visualPartyLeader).Equipment[i];
				if (((EquipmentElement)(ref val)).Item.PrimaryWeapon.IsMeleeWeapon)
				{
					wieldedItemIndex = i;
					break;
				}
			}
		}
	}

	private static void GetPartyBattleAnimation(PartyBase party, int wieldedItemIndex, out ActionIndexCache leaderAction, out ActionIndexCache mountAction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Invalid comparison between Unknown and I4
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Invalid comparison between Unknown and I4
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Invalid comparison between Unknown and I4
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Invalid comparison between Unknown and I4
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Invalid comparison between Unknown and I4
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Invalid comparison between Unknown and I4
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Invalid comparison between Unknown and I4
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Invalid comparison between Unknown and I4
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Invalid comparison between Unknown and I4
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Invalid comparison between Unknown and I4
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Invalid comparison between Unknown and I4
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Invalid comparison between Unknown and I4
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Invalid comparison between Unknown and I4
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Invalid comparison between Unknown and I4
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Invalid comparison between Unknown and I4
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Invalid comparison between Unknown and I4
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Invalid comparison between Unknown and I4
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Invalid comparison between Unknown and I4
		leaderAction = ActionIndexCache.act_none;
		mountAction = ActionIndexCache.act_none;
		if (party.MobileParty.Army == null || !party.MobileParty.Army.DoesLeaderPartyAndAttachedPartiesContain(party.MobileParty))
		{
			_ = party.MapEvent;
		}
		else
		{
			_ = party.MobileParty.Army.LeaderParty.MapEvent;
		}
		CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(party);
		MapEvent mapEvent = party.MapEvent;
		if (((mapEvent != null) ? mapEvent.MapEventSettlement : null) != null && visualPartyLeader != null && !((BasicCharacterObject)visualPartyLeader).HasMount())
		{
			leaderAction = ActionIndexCache.act_map_raid;
			return;
		}
		EquipmentElement val;
		if (wieldedItemIndex > -1)
		{
			object obj;
			if (visualPartyLeader == null)
			{
				obj = null;
			}
			else
			{
				val = ((BasicCharacterObject)visualPartyLeader).Equipment[wieldedItemIndex];
				obj = ((EquipmentElement)(ref val)).Item;
			}
			if (obj != null)
			{
				val = ((BasicCharacterObject)visualPartyLeader).Equipment[wieldedItemIndex];
				WeaponComponent weaponComponent = ((EquipmentElement)(ref val)).Item.WeaponComponent;
				if (weaponComponent != null && weaponComponent.PrimaryWeapon.IsMeleeWeapon)
				{
					if (((BasicCharacterObject)visualPartyLeader).HasMount())
					{
						val = ((BasicCharacterObject)visualPartyLeader).Equipment[10];
						if (((EquipmentElement)(ref val)).Item.HorseComponent.Monster.MonsterUsage == "camel")
						{
							if ((int)weaponComponent.GetItemType() == 2 || (int)weaponComponent.GetItemType() == 3)
							{
								leaderAction = ActionIndexCache.act_map_rider_camel_attack_1h;
								mountAction = ActionIndexCache.act_map_mount_attack_1h;
							}
							else if ((int)weaponComponent.GetItemType() == 4)
							{
								if ((int)weaponComponent.PrimaryWeapon.SwingDamageType == -1)
								{
									leaderAction = ActionIndexCache.act_map_rider_camel_attack_1h_spear;
									mountAction = ActionIndexCache.act_map_mount_attack_spear;
								}
								else if ((int)weaponComponent.PrimaryWeapon.WeaponClass == 10)
								{
									leaderAction = ActionIndexCache.act_map_rider_camel_attack_1h_swing;
									mountAction = ActionIndexCache.act_map_mount_attack_swing;
								}
								else
								{
									leaderAction = ActionIndexCache.act_map_rider_camel_attack_2h_swing;
									mountAction = ActionIndexCache.act_map_mount_attack_swing;
								}
							}
						}
						else if ((int)weaponComponent.GetItemType() == 2 || (int)weaponComponent.GetItemType() == 3)
						{
							leaderAction = ActionIndexCache.act_map_rider_horse_attack_1h;
							mountAction = ActionIndexCache.act_map_mount_attack_1h;
						}
						else if ((int)weaponComponent.GetItemType() == 4)
						{
							if ((int)weaponComponent.PrimaryWeapon.SwingDamageType == -1)
							{
								leaderAction = ActionIndexCache.act_map_rider_horse_attack_1h_spear;
								mountAction = ActionIndexCache.act_map_mount_attack_spear;
							}
							else if ((int)weaponComponent.PrimaryWeapon.WeaponClass == 10)
							{
								leaderAction = ActionIndexCache.act_map_rider_horse_attack_1h_swing;
								mountAction = ActionIndexCache.act_map_mount_attack_swing;
							}
							else
							{
								leaderAction = ActionIndexCache.act_map_rider_horse_attack_2h_swing;
								mountAction = ActionIndexCache.act_map_mount_attack_swing;
							}
						}
					}
					else if ((int)weaponComponent.PrimaryWeapon.WeaponClass == 4 || (int)weaponComponent.PrimaryWeapon.WeaponClass == 6 || (int)weaponComponent.PrimaryWeapon.WeaponClass == 2)
					{
						leaderAction = ActionIndexCache.act_map_attack_1h;
					}
					else if ((int)weaponComponent.PrimaryWeapon.WeaponClass == 5 || (int)weaponComponent.PrimaryWeapon.WeaponClass == 8 || (int)weaponComponent.PrimaryWeapon.WeaponClass == 3)
					{
						leaderAction = ActionIndexCache.act_map_attack_2h;
					}
					else if ((int)weaponComponent.PrimaryWeapon.WeaponClass == 9 || (int)weaponComponent.PrimaryWeapon.WeaponClass == 10)
					{
						leaderAction = ActionIndexCache.act_map_attack_spear_1h_or_2h;
					}
				}
			}
		}
		if (leaderAction == ActionIndexCache.act_none)
		{
			if (((BasicCharacterObject)visualPartyLeader).HasMount())
			{
				val = ((BasicCharacterObject)visualPartyLeader).Equipment[10];
				HorseComponent horseComponent = ((EquipmentElement)(ref val)).Item.HorseComponent;
				leaderAction = ((horseComponent.Monster.MonsterUsage == "camel") ? ActionIndexCache.act_map_rider_camel_attack_unarmed : ActionIndexCache.act_map_rider_horse_attack_unarmed);
				mountAction = ActionIndexCache.act_map_mount_attack_unarmed;
			}
			else
			{
				leaderAction = ActionIndexCache.act_map_attack_unarmed;
			}
		}
	}

	private void GetMountAndHarnessVisualIdsForPartyIcon(out string mountStringId, out string harnessStringId)
	{
		mountStringId = "";
		harnessStringId = "";
		if (base.MapEntity.IsMobile)
		{
			PartyComponent partyComponent = base.MapEntity.MobileParty.PartyComponent;
			if (partyComponent != null)
			{
				partyComponent.GetMountAndHarnessVisualIdsForPartyIcon(base.MapEntity, ref mountStringId, ref harnessStringId);
			}
		}
	}

	private void InitializePartyCollider(PartyBase party)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (StrategicEntity != (GameEntity)null && party.IsMobile)
		{
			GameEntityPhysicsExtensions.AddSphereAsBody(StrategicEntity, new Vec3(0f, 0f, 0f, -1f), 0.5f, (BodyFlags)144);
		}
	}

	private void ResetPartyIcon()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (HumanAgentVisuals != null)
		{
			HumanAgentVisuals.Reset();
			HumanAgentVisuals = null;
		}
		if (MountAgentVisuals != null)
		{
			MountAgentVisuals.Reset();
			MountAgentVisuals = null;
		}
		if (CaravanMountAgentVisuals != null)
		{
			CaravanMountAgentVisuals.Reset();
			CaravanMountAgentVisuals = null;
		}
		if (StrategicEntity != (GameEntity)null)
		{
			if ((StrategicEntity.EntityFlags & 0x10000000) != 0)
			{
				StrategicEntity.RemoveFromPredisplayEntity();
			}
			StrategicEntity.ClearComponents();
		}
		Vec2 bearing = base.MapEntity.MobileParty.Bearing;
		_bearingRotation = ((Vec2)(ref bearing)).RotationInRadians;
		MobilePartyVisualManager.Current.UnRegisterFadingVisual(this);
	}

	private float GetTransitionProgress()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (IsMobileEntity && base.MapEntity.MobileParty.IsTransitionInProgress && base.MapEntity.MobileParty.NavigationTransitionDuration != CampaignTime.Zero)
		{
			CampaignTime val = base.MapEntity.MobileParty.NavigationTransitionDuration;
			float num = (float)((CampaignTime)(ref val)).ToHours;
			Army army = base.MapEntity.MobileParty.Army;
			if (((army != null) ? army.LeaderParty : null) == base.MapEntity.MobileParty && ((List<MobileParty>)(object)base.MapEntity.MobileParty.AttachedParties).Count > 0)
			{
				float val2 = LinQuick.MaxQ<MobileParty>((List<MobileParty>)(object)base.MapEntity.MobileParty.AttachedParties, (Func<MobileParty, float>)delegate(MobileParty x)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					CampaignTime navigationTransitionDuration = x.NavigationTransitionDuration;
					return (float)((CampaignTime)(ref navigationTransitionDuration)).ToHours;
				});
				num = Math.Max(num, val2);
			}
			val = base.MapEntity.MobileParty.NavigationTransitionStartTime;
			return MBMath.ClampFloat(((CampaignTime)(ref val)).ElapsedHoursUntilNow / num, 0f, 1f);
		}
		return 1f;
	}

	private void OnTransitionStarted()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		MobilePartyVisualManager.Current.RegisterFadingVisual(this);
		CampaignVec2 val = base.MapEntity.MobileParty.EndPositionForNavigationTransition;
		Vec2 val2 = ((CampaignVec2)(ref val)).ToVec2();
		val = base.MapEntity.Position;
		Vec2 val3 = val2 - ((CampaignVec2)(ref val)).ToVec2();
		_transitionStartRotation = ((Vec2)(ref val3)).RotationInRadians;
	}

	private void OnTransitionEnded()
	{
	}

	private float GetVisualRotation()
	{
		if (base.MapEntity.IsMobile && base.MapEntity.MapEvent != null && base.MapEntity.MapEvent.IsFieldBattle)
		{
			return GetMapEventVisualRotation();
		}
		if (base.MapEntity.IsMobile && base.MapEntity.MobileParty.IsTransitionInProgress)
		{
			return _transitionStartRotation;
		}
		return _bearingRotation;
	}

	private float GetMapEventVisualRotation()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (base.MapEntity.MapEventSide.OtherSide.LeaderParty != null && base.MapEntity.MapEventSide.OtherSide.LeaderParty.IsMobile && base.MapEntity.MapEventSide.OtherSide.LeaderParty.IsMobile)
		{
			Vec2 val = base.MapEntity.MapEventSide.OtherSide.LeaderParty.MobileParty.VisualPosition2DWithoutError - base.MapEntity.MobileParty.VisualPosition2DWithoutError;
			Vec2 val2 = ((Vec2)(ref val)).Normalized();
			return ((Vec2)(ref val2)).RotationInRadians;
		}
		return _bearingRotation;
	}

	private void AddVisualToVisualsOfEntities()
	{
		if (!MapScreen.VisualsOfEntities.ContainsKey(((NativeObject)StrategicEntity).Pointer))
		{
			MapScreen.VisualsOfEntities.Add(((NativeObject)StrategicEntity).Pointer, this);
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

	private bool IsPartOfBesiegerCamp(PartyBase party)
	{
		Settlement besiegedSettlement = party.MobileParty.BesiegedSettlement;
		if (((besiegedSettlement != null) ? besiegedSettlement.SiegeEvent : null) != null)
		{
			return party.MobileParty.BesiegedSettlement.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(party, (BattleTypes)5);
		}
		return false;
	}
}
