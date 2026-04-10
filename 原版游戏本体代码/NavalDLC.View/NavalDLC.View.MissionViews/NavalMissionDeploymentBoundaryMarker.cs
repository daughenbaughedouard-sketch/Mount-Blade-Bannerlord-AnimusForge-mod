using System;
using System.Collections.Generic;
using NavalDLC.Missions.NavalPhysics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

namespace NavalDLC.View.MissionViews;

public class NavalMissionDeploymentBoundaryMarker : MissionDeploymentBoundaryMarker
{
	private readonly string _largePrefabName;

	private GameEntity _cachedLargeEntity;

	public NavalMissionDeploymentBoundaryMarker(string smallPrefabName, string largePrefabName, float markerInterval = 20f)
		: base(smallPrefabName, markerInterval)
	{
		_largePrefabName = largePrefabName;
	}

	protected override void MarkLine(Vec3 startPoint, Vec3 endPoint, List<GameEntity> boundary, Banner banner = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = endPoint - startPoint;
		float length = ((Vec3)(ref val)).Length;
		Vec3 val2 = val;
		((Vec3)(ref val2)).Normalize();
		val2 *= base.MarkerInterval;
		for (float num = 0f; num < length; num += base.MarkerInterval)
		{
			GameEntity val3 = CreateBoundaryEntity((int)(num / base.MarkerInterval) % 4 == 0);
			NavalPhysics firstScriptOfType = val3.GetFirstScriptOfType<NavalPhysics>();
			MatrixFrame identity = MatrixFrame.Identity;
			((Mat3)(ref identity.rotation)).RotateAboutUp(((Vec3)(ref val)).RotationZ + MathF.PI);
			identity.origin = startPoint;
			identity.origin.z = val3.GetWaterLevelAtPosition(((Vec3)(ref identity.origin)).AsVec2, true, false) - (firstScriptOfType?.StabilitySubmergedHeightOfShip ?? 0f);
			val3.SetFrame(ref identity, true);
			firstScriptOfType?.SetAnchor(isAnchored: true, anchorInPlace: true);
			boundary.Add(val3);
			startPoint += val2;
		}
	}

	private GameEntity CreateBoundaryEntity(bool isLarge)
	{
		Scene scene = Mission.Current.Scene;
		if (isLarge && _cachedLargeEntity == (GameEntity)null)
		{
			_cachedLargeEntity = GameEntity.Instantiate((Scene)null, _largePrefabName, false, true, "");
		}
		else if (!isLarge && base._cachedEntity == (GameEntity)null)
		{
			base._cachedEntity = GameEntity.Instantiate((Scene)null, base._prefabName, false, true, "");
		}
		GameEntity obj = GameEntity.CopyFrom(scene, isLarge ? _cachedLargeEntity : base._cachedEntity, true, true);
		obj.SetMobility((Mobility)1);
		return obj;
	}
}
