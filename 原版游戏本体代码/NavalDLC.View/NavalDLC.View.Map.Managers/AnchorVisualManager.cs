using System;
using System.Collections.Generic;
using NavalDLC.View.Map.Visuals;
using SandBox.View;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.View.Map.Managers;

public class AnchorVisualManager : EntityVisualManagerBase<AnchorPoint>
{
	private const float DecalEntityHeight = 1f;

	private const uint DecalColor = 4291596077u;

	private AnchorVisual _anchorVisual;

	private DecalEntity _anchorCircleDecal;

	private CampaignVec2 _cachedPosition;

	private (bool, bool) _cachedDisabledValue;

	public static AnchorVisualManager Current => SandBoxViewSubModule.SandBoxViewVisualManager.GetEntityComponent<AnchorVisualManager>();

	public override int Priority => 30;

	public override MapEntityVisual<AnchorPoint> GetVisualOfEntity(AnchorPoint entity)
	{
		return _anchorVisual;
	}

	protected override void OnInitialize()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		((CampaignEntityVisualComponent)this).OnInitialize();
		if (CanPartyHaveAnchor())
		{
			if (_anchorVisual == null)
			{
				CreateNewVisual();
			}
			else
			{
				_anchorVisual.OnVisualUpdate();
			}
		}
		_anchorCircleDecal = DecalEntity.Create(((EntityVisualManagerBase)this).MapScene, "decal_city_circle_a", "TownCircle");
	}

	public override bool OnVisualIntersected(Ray mouseRay, UIntPtr[] intersectedEntityIDs, Intersection[] intersectionInfos, int entityCount, Vec3 worldMouseNear, Vec3 worldMouseFar, Vec3 terrainIntersectionPoint, ref MapEntityVisual hoveredVisual, ref MapEntityVisual selectedVisual)
	{
		for (int num = entityCount - 1; num >= 0; num--)
		{
			UIntPtr uIntPtr = intersectedEntityIDs[num];
			if (uIntPtr != UIntPtr.Zero && MapScreen.VisualsOfEntities.TryGetValue(uIntPtr, out var value) && value is AnchorVisual && value.IsVisibleOrFadingOut())
			{
				hoveredVisual = value;
				selectedVisual = value;
			}
		}
		return selectedVisual != null;
	}

	public override void OnVisualTick(MapScreen screen, float realDt, float dt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		MatrixFrame identity = MatrixFrame.Identity;
		if (_anchorVisual != null && ((MobileParty.MainParty.Ai.AiBehaviorInteractable != null && MobileParty.MainParty.Ai.AiBehaviorInteractable is AnchorPoint) || (MapScreen.Instance.CurrentVisualOfTooltip != null && MapScreen.Instance.CurrentVisualOfTooltip is AnchorVisual)))
		{
			flag = true;
			identity.origin = ((MapEntityVisual)_anchorVisual).GetVisualPosition();
		}
		((DecalEntity)(ref _anchorCircleDecal)).GameEntity.SetVisibilityExcludeParents(flag);
		if (flag)
		{
			((DecalEntity)(ref _anchorCircleDecal)).Decal.SetVectorArgument(1f, 1f, 0f, 0f);
			((DecalEntity)(ref _anchorCircleDecal)).Decal.SetFactor1Linear(4291596077u);
			((DecalEntity)(ref _anchorCircleDecal)).GameEntity.SetGlobalFrame(ref identity, true);
		}
	}

	public override void OnTick(float realDt, float dt)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		((CampaignEntityVisualComponent)this).OnTick(realDt, dt);
		bool flag = _anchorVisual?.Entity != (GameEntity)null && _anchorVisual.Entity.IsVisibleIncludeParents() && (((MapEntityVisual<AnchorPoint>)_anchorVisual).MapEntity != MobileParty.MainParty.Anchor || !MobileParty.MainParty.IsActive || MobileParty.MainParty.Anchor.IsDisabled);
		if (_anchorVisual != null && (flag || ((List<Ship>)(object)PartyBase.MainParty.Ships).Count == 0))
		{
			RemoveAnchorVisual();
		}
		if (_anchorVisual != null && CanPartyHaveAnchor())
		{
			UpdateAnchorVisual();
		}
		if (_cachedPosition != MobileParty.MainParty.Anchor.Position && (((CampaignVec2)(ref _cachedPosition)).IsValid() || MobileParty.MainParty.Anchor.IsValid) && !MobileParty.MainParty.Anchor.IsDisabled)
		{
			OnAnchorPositionUpdated();
			_cachedPosition = MobileParty.MainParty.Anchor.Position;
		}
		if (_cachedDisabledValue.Item1 != MobileParty.MainParty.Anchor.IsDisabled || _cachedDisabledValue.Item2 != MobileParty.MainParty.IsActive)
		{
			OnAnchorPositionUpdated();
			_cachedDisabledValue = (MobileParty.MainParty.Anchor.IsDisabled, MobileParty.MainParty.IsActive);
		}
	}

	internal void OnAnchorPositionUpdated()
	{
		if (_anchorVisual != null)
		{
			if (CanPartyHaveAnchor())
			{
				_anchorVisual.UpdateAnchorVisualPosition();
			}
			else
			{
				RemoveAnchorVisual();
			}
		}
		else if (CanPartyHaveAnchor())
		{
			CreateNewVisual();
		}
	}

	private bool CanPartyHaveAnchor()
	{
		if (MobileParty.MainParty.IsActive && MobileParty.MainParty.Anchor.IsValid && MobileParty.MainParty.HasNavalNavigationCapability)
		{
			return !MobileParty.MainParty.Anchor.IsDisabled;
		}
		return false;
	}

	private void CreateNewVisual()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_anchorVisual = new AnchorVisual(MobileParty.MainParty.Anchor);
		_anchorVisual.OnStartup();
		_cachedPosition = ((MapEntityVisual<AnchorPoint>)_anchorVisual).MapEntity.Position;
		MapScreen.VisualsOfEntities.Add(((NativeObject)_anchorVisual.Entity).Pointer, (MapEntityVisual)(object)_anchorVisual);
	}

	private void RemoveAnchorVisual()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		MapScreen.VisualsOfEntities.Remove(((NativeObject)_anchorVisual.Entity).Pointer);
		_anchorVisual.OnRemoved();
		_cachedPosition = CampaignVec2.Invalid;
		_anchorVisual = null;
	}

	private void UpdateAnchorVisual()
	{
		_anchorVisual.OnVisualUpdate();
		if (_anchorVisual?.Entity != (GameEntity)null && !MapScreen.VisualsOfEntities.ContainsKey(((NativeObject)_anchorVisual.Entity).Pointer))
		{
			MapScreen.VisualsOfEntities.Add(((NativeObject)_anchorVisual.Entity).Pointer, (MapEntityVisual)(object)_anchorVisual);
		}
	}
}
