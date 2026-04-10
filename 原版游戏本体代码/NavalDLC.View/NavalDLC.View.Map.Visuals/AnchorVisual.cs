using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using SandBox;
using SandBox.View.Map;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.View.Map.Visuals;

public class AnchorVisual : MapEntityVisual<AnchorPoint>
{
	private ShipHull _flagshipHull;

	private uint _cachedVersion;

	private List<SailVisual> _sailVisuals = new List<SailVisual>();

	private Scene _mapScene;

	public override CampaignVec2 InteractionPositionForPlayer => base.MapEntity.GetInteractionPosition(MobileParty.MainParty);

	public override MapEntityVisual AttachedTo => null;

	public GameEntity Entity { get; private set; }

	private Scene MapScene
	{
		get
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if ((NativeObject)(object)_mapScene == (NativeObject)null && Campaign.Current != null && Campaign.Current.MapSceneWrapper != null)
			{
				_mapScene = ((MapScene)Campaign.Current.MapSceneWrapper).Scene;
			}
			return _mapScene;
		}
	}

	public AnchorVisual(AnchorPoint mapEntity)
		: base(mapEntity)
	{
	}

	public override Vec3 GetVisualPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 position = base.MapEntity.Position;
		return ((CampaignVec2)(ref position)).AsVec3();
	}

	public override bool IsVisibleOrFadingOut()
	{
		return !base.MapEntity.Owner.IsTransitionInProgress;
	}

	public override void OnHover()
	{
		InformationManager.ShowTooltip(typeof(AnchorPoint), new object[1] { base.MapEntity });
	}

	public override bool OnMapClick(bool followModifierUsed)
	{
		MobileParty.MainParty.SetMoveGoToInteractablePoint((IInteractablePoint)(object)base.MapEntity, (NavigationType)3);
		return true;
	}

	public override void ReleaseResources()
	{
	}

	public override void OnOpenEncyclopedia()
	{
	}

	public void OnStartup()
	{
		if (Entity != (GameEntity)null)
		{
			OnVisualUpdate();
		}
		else
		{
			RefreshGameEntity();
		}
	}

	public void OnRemoved()
	{
		if (((List<Ship>)(object)PartyBase.MainParty.Ships).Count > 0)
		{
			Entity.SetVisibilityExcludeParents(false);
			return;
		}
		base.MapEntity.ResetPosition();
		GameEntity entity = Entity;
		if (entity != null)
		{
			entity.Remove(111);
		}
		Entity = null;
		ResetVersionCache();
		_sailVisuals.Clear();
	}

	public void OnVisualUpdate()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Ship flagShip = PartyBase.MainParty.FlagShip;
		if (_flagshipHull == null || _flagshipHull != flagShip.ShipHull)
		{
			if (Entity != (GameEntity)null)
			{
				MapScreen.VisualsOfEntities.Remove(((NativeObject)Entity).Pointer);
				GameEntity entity = Entity;
				if (entity != null)
				{
					entity.Remove(111);
				}
			}
			RefreshGameEntity();
		}
		else if (flagShip.VersionNo != _cachedVersion)
		{
			UpdateVersionCache();
			NavalDLCViewHelpers.ShipVisualHelper.RefreshShipVisuals(Entity.WeakEntity, flagShip, _sailVisuals);
		}
	}

	private void UpdateVersionCache()
	{
		Ship flagShip = PartyBase.MainParty.FlagShip;
		_cachedVersion = flagShip.VersionNo;
		_flagshipHull = flagShip.ShipHull;
	}

	private void ResetVersionCache()
	{
		_flagshipHull = null;
		_cachedVersion = 0u;
	}

	private void RefreshGameEntity()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		UpdateVersionCache();
		Entity = NavalDLCViewHelpers.ShipVisualHelper.GetFlagshipEntity(PartyBase.MainParty, MapScene);
		NavalDLCViewHelpers.ShipVisualHelper.CollectSailVisuals(Entity.WeakEntity, _sailVisuals);
		Entity.SetVisibilityExcludeParents(false);
		GameEntityPhysicsExtensions.AddSphereAsBody(Entity, new Vec3(0f, 0f, 0f, -1f), 3f, (BodyFlags)144);
		UpdateAnchorVisualPosition();
	}

	internal void UpdateAnchorVisualPosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = CalculateAnchorFrame(base.MapEntity);
		Entity.SetFrame(ref val, true);
		Entity.SetVisibilityExcludeParents(true);
	}

	private MatrixFrame CalculateAnchorFrame(AnchorPoint anchor)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 interactionPosition = anchor.GetInteractionPosition(anchor.Owner);
		Vec2 val = ((CampaignVec2)(ref interactionPosition)).ToVec2();
		CampaignVec2 position = anchor.Position;
		Vec2 val2 = val - ((CampaignVec2)(ref position)).ToVec2();
		Vec2 val3 = ((Vec2)(ref val2)).Normalized();
		Vec3 localScale = Entity.GetLocalScale();
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = ((MapEntityVisual)this).GetVisualPosition();
		((Vec3)(ref identity.rotation.f)).AsVec2 = ((Vec2)(ref val3)).RightVec();
		((Vec3)(ref identity.rotation.f)).NormalizeWithoutChangingZ();
		((Mat3)(ref identity.rotation)).Orthonormalize();
		((Mat3)(ref identity.rotation)).ApplyScaleLocal(ref localScale);
		return identity;
	}

	private bool CanHaveAnchor()
	{
		if (base.MapEntity.Owner.HasNavalNavigationCapability && base.MapEntity.IsValid)
		{
			return !base.MapEntity.IsDisabled;
		}
		return false;
	}
}
