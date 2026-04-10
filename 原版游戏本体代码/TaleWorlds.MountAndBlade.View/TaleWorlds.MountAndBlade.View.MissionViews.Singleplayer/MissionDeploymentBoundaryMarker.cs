using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

public class MissionDeploymentBoundaryMarker : MissionView
{
	public const string AttackerStaticDeploymentBoundaryName = "walk_area";

	public const string DefenderStaticDeploymentBoundaryName = "deployment_castle_boundary";

	public readonly float MarkerInterval;

	protected readonly Dictionary<string, List<GameEntity>>[] _boundaryMarkersPerSide = new Dictionary<string, List<GameEntity>>[2];

	protected readonly string _prefabName;

	protected GameEntity _cachedEntity;

	protected bool _boundaryMarkersRemoved = true;

	public MissionDeploymentBoundaryMarker(string prefabName, float markerInterval = 2f)
	{
		_prefabName = prefabName;
		MarkerInterval = Math.Max(markerInterval, 0.0001f);
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		for (int i = 0; i < 2; i++)
		{
			_boundaryMarkersPerSide[i] = new Dictionary<string, List<GameEntity>>();
		}
		_boundaryMarkersRemoved = false;
	}

	protected override void OnEndMission()
	{
		((MissionBehavior)this).OnEndMission();
		TryRemoveBoundaryMarkers();
	}

	public override void OnDeploymentPlanMade(Team team, bool isFirstPlan)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!team.IsPlayerTeam && team != ((MissionBehavior)this).Mission.PlayerEnemyTeam)
		{
			return;
		}
		bool flag = ((MissionBehavior)this).Mission.DeploymentPlan.HasDeploymentBoundaries(team);
		if (!(isFirstPlan && flag))
		{
			return;
		}
		foreach (var item in (List<(string, MBList<Vec2>)>)(object)((MissionBehavior)this).Mission.DeploymentPlan.GetDeploymentBoundaries(team))
		{
			AddBoundaryMarkerForSide(team.Side, new KeyValuePair<string, ICollection<Vec2>>(item.Item1, (ICollection<Vec2>)item.Item2));
		}
	}

	public override void OnRemoveBehavior()
	{
		TryRemoveBoundaryMarkers();
		base.OnRemoveBehavior();
	}

	private void AddBoundaryMarkerForSide(BattleSideEnum side, KeyValuePair<string, ICollection<Vec2>> boundary)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected I4, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		string key = boundary.Key;
		int num = (int)side;
		if (!_boundaryMarkersPerSide[num].ContainsKey(key))
		{
			Banner banner = (((int)side == 1) ? ((MissionBehavior)this).Mission.AttackerTeam.Banner : (((int)side == 0) ? ((MissionBehavior)this).Mission.DefenderTeam.Banner : null));
			List<GameEntity> list = new List<GameEntity>();
			List<Vec2> list2 = boundary.Value.ToList();
			for (int i = 0; i < list2.Count; i++)
			{
				MarkLine(new Vec3(list2[i], 0f, -1f), new Vec3(list2[(i + 1) % list2.Count], 0f, -1f), list, banner);
			}
			_boundaryMarkersPerSide[num][key] = list;
		}
	}

	private void TryRemoveBoundaryMarkers()
	{
		if (_boundaryMarkersRemoved)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			foreach (string item in _boundaryMarkersPerSide[i].Keys.ToList())
			{
				RemoveBoundaryMarker(item, (BattleSideEnum)i);
			}
		}
		_boundaryMarkersRemoved = true;
	}

	private void RemoveBoundaryMarker(string boundaryName, BattleSideEnum side)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Expected I4, but got Unknown
		int num = (int)side;
		if (!_boundaryMarkersPerSide[num].TryGetValue(boundaryName, out var value))
		{
			return;
		}
		foreach (GameEntity item in value)
		{
			item.Remove(103);
		}
		_boundaryMarkersPerSide[num].Remove(boundaryName);
	}

	protected virtual void MarkLine(Vec3 startPoint, Vec3 endPoint, List<GameEntity> boundary, Banner banner = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = ((MissionBehavior)this).Mission.Scene;
		Vec3 val = endPoint - startPoint;
		float length = ((Vec3)(ref val)).Length;
		Vec3 val2 = val;
		((Vec3)(ref val2)).Normalize();
		val2 *= MarkerInterval;
		for (float num = 0f; num < length; num += MarkerInterval)
		{
			MatrixFrame identity = MatrixFrame.Identity;
			((Mat3)(ref identity.rotation)).RotateAboutUp(((Vec3)(ref val)).RotationZ + MathF.PI / 2f);
			identity.origin = startPoint;
			if (!scene.GetHeightAtPoint(((Vec3)(ref identity.origin)).AsVec2, (BodyFlags)540127625, ref identity.origin.z))
			{
				identity.origin.z = 0f;
			}
			identity.origin.z -= 0.5f;
			Vec3 val3 = Vec3.One * 0.4f;
			((MatrixFrame)(ref identity)).Scale(ref val3);
			GameEntity val4 = MakeEntity(banner);
			val4.SetFrame(ref identity, true);
			boundary.Add(val4);
			startPoint += val2;
		}
	}

	private GameEntity MakeEntity(Banner banner = null)
	{
		Scene scene = ((MissionBehavior)this).Mission.Scene;
		if (_cachedEntity == (GameEntity)null)
		{
			_cachedEntity = GameEntity.Instantiate((Scene)null, _prefabName, false, true, "");
		}
		GameEntity val = GameEntity.CopyFrom(scene, _cachedEntity, true, true);
		val.SetMobility((Mobility)1);
		if (banner != null)
		{
			Mesh firstMesh = val.GetFirstMesh();
			Material material = firstMesh.GetMaterial();
			Material tableauMaterial = material.CreateCopy();
			banner.GetTableauTextureSmall(BannerDebugInfo.CreateManual(((object)this).GetType().Name), delegate(Texture tex)
			{
				tableauMaterial.SetTexture((MBTextureType)1, tex);
			});
			firstMesh.SetMaterial(tableauMaterial);
		}
		return val;
	}
}
