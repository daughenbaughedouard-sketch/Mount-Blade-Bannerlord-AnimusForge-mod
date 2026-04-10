using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.View.Map.Managers;

public class MapTracksVisualManager : EntityVisualManagerBase<Track>
{
	private const string TrackPrefabName = "map_track_arrow";

	private const int DefaultObjectPoolCount = 256;

	private Dictionary<Track, (TrackVisual, GameEntity)> _trackVisuals;

	private SphereData _trackSphere;

	private bool _tracksDirty = true;

	private readonly ParallelForAuxPredicate _parallelUpdateTrackColorsPredicate;

	private readonly ParallelForAuxPredicate _parallelUpdateVisibleTracksPredicate;

	private Stack<GameEntity> _entityPool;

	public static MapTracksVisualManager Current => SandBoxViewSubModule.SandBoxViewVisualManager.GetEntityComponent<MapTracksVisualManager>();

	public override int Priority => 50;

	public MapTracksVisualManager()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		_trackVisuals = new Dictionary<Track, (TrackVisual, GameEntity)>();
		_entityPool = new Stack<GameEntity>();
		PopulateEntityPool();
		_parallelUpdateTrackColorsPredicate = new ParallelForAuxPredicate(ParallelUpdateTrackColors);
		_parallelUpdateVisibleTracksPredicate = new ParallelForAuxPredicate(ParallelUpdateVisibleTracks);
	}

	public override void OnVisualTick(MapScreen screen, float realDt, float dt)
	{
		if (_tracksDirty)
		{
			UpdateTrackMesh();
			_tracksDirty = false;
		}
		TWParallel.For(0, ((List<Track>)(object)MapScreen.Instance.MapTracksCampaignBehavior.DetectedTracks).Count, _parallelUpdateTrackColorsPredicate, 16);
	}

	public override bool OnVisualIntersected(Ray mouseRay, UIntPtr[] intersectedEntityIDs, Intersection[] intersectionInfos, int entityCount, Vec3 worldMouseNear, Vec3 worldMouseFar, Vec3 terrainIntersectionPoint, ref MapEntityVisual hoveredVisual, ref MapEntityVisual selectedVisual)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (hoveredVisual == null)
		{
			hoveredVisual = GetVisualOfEntity(GetTrackOnMouse(mouseRay, terrainIntersectionPoint));
		}
		return hoveredVisual != null;
	}

	public override void OnGameLoadFinished()
	{
		base.OnGameLoadFinished();
		foreach (Track item in (List<Track>)(object)MapScreen.Instance.MapTracksCampaignBehavior.DetectedTracks)
		{
			OnTrackDetected(item);
		}
	}

	public override MapEntityVisual<Track> GetVisualOfEntity(Track entity)
	{
		if (entity == null)
		{
			return null;
		}
		return _trackVisuals[entity].Item1;
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		foreach (GameEntity item in _entityPool.ToList())
		{
			item.Remove(111);
		}
		_entityPool.Clear();
		_trackVisuals.Clear();
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		CampaignEvents.TrackDetectedEvent.AddNonSerializedListener((object)this, (Action<Track>)OnTrackDetected);
		CampaignEvents.TrackLostEvent.AddNonSerializedListener((object)this, (Action<Track>)OnTrackLost);
	}

	internal void ReleaseResources(Track track)
	{
		if (_trackVisuals.TryGetValue(track, out var value))
		{
			value.Item2.Remove(111);
		}
	}

	private void OnTrackDetected(Track track)
	{
		_tracksDirty = true;
		GameEntity gameEntity = GetGameEntity();
		gameEntity.SetVisibilityExcludeParents(true);
		_trackVisuals.Add(track, (new TrackVisual(track), gameEntity));
		SandBoxViewSubModule.VisualsOfEntities.Add(((NativeObject)_trackVisuals[track].Item2).Pointer, _trackVisuals[track].Item1);
	}

	private void OnTrackLost(Track track)
	{
		_tracksDirty = true;
		(TrackVisual, GameEntity) tuple = _trackVisuals[track];
		_trackVisuals.Remove(track);
		SandBoxViewSubModule.VisualsOfEntities.Remove(((NativeObject)tuple.Item2).Pointer);
		ReleaseEntity(tuple.Item2);
	}

	private void ParallelUpdateTrackColors(Track track)
	{
		GameEntityComponent componentAtIndex = _trackVisuals[track].Item2.GetComponentAtIndex(0, (ComponentType)7);
		((Decal)((componentAtIndex is Decal) ? componentAtIndex : null)).SetFactor1(Campaign.Current.Models.MapTrackModel.GetTrackColor(track));
	}

	private void ParallelUpdateTrackColors(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			ParallelUpdateTrackColors(((List<Track>)(object)MapScreen.Instance.MapTracksCampaignBehavior.DetectedTracks)[i]);
		}
	}

	private void UpdateTrackMesh()
	{
		TWParallel.For(0, ((List<Track>)(object)MapScreen.Instance.MapTracksCampaignBehavior.DetectedTracks).Count, _parallelUpdateVisibleTracksPredicate, 16);
	}

	private void UpdateTrackPoolPosition(Track track)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = CalculateTrackFrame(track);
		_trackVisuals[track].Item2.SetFrame(ref val, true);
	}

	private void ParallelUpdateVisibleTracks(Track track)
	{
		_trackVisuals[track].Item2.SetVisibilityExcludeParents(true);
		UpdateTrackPoolPosition(track);
	}

	private void ParallelUpdateVisibleTracks(int startInclusive, int endExclusive)
	{
		for (int i = startInclusive; i < endExclusive; i++)
		{
			ParallelUpdateVisibleTracks(((List<Track>)(object)MapScreen.Instance.MapTracksCampaignBehavior.DetectedTracks)[i]);
		}
	}

	private bool RaySphereIntersection(Ray ray, SphereData sphere, ref Vec3 intersectionPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		Vec3 origin = sphere.Origin;
		float radius = sphere.Radius;
		Vec3 val = origin - ((Ray)(ref ray)).Origin;
		float num = Vec3.DotProduct(((Ray)(ref ray)).Direction, val);
		if (num > 0f)
		{
			Vec3 val2 = ((Ray)(ref ray)).Origin + ((Ray)(ref ray)).Direction * num - origin;
			float num2 = radius * radius - ((Vec3)(ref val2)).LengthSquared;
			if (num2 >= 0f)
			{
				float num3 = MathF.Sqrt(num2);
				float num4 = num - num3;
				if (num4 >= 0f && num4 <= ((Ray)(ref ray)).MaxDistance)
				{
					intersectionPoint = ((Ray)(ref ray)).Origin + ((Ray)(ref ray)).Direction * num4;
					return true;
				}
				if (num4 < 0f)
				{
					intersectionPoint = ((Ray)(ref ray)).Origin;
					return true;
				}
			}
		}
		else
		{
			Vec3 val3 = ((Ray)(ref ray)).Origin - origin;
			if (((Vec3)(ref val3)).LengthSquared < radius * radius)
			{
				intersectionPoint = ((Ray)(ref ray)).Origin;
				return true;
			}
		}
		return false;
	}

	private Track GetTrackOnMouse(Ray mouseRay, Vec3 mouseIntersectionPoint)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		Track result = null;
		for (int i = 0; i < ((List<Track>)(object)MapScreen.Instance.MapTracksCampaignBehavior.DetectedTracks).Count; i++)
		{
			Track val = ((List<Track>)(object)MapScreen.Instance.MapTracksCampaignBehavior.DetectedTracks)[i];
			float trackScale = Campaign.Current.Models.MapTrackModel.GetTrackScale(val);
			MatrixFrame val2 = CalculateTrackFrame(val);
			Vec3 val3 = val2.origin - mouseIntersectionPoint;
			float lengthSquared = ((Vec3)(ref val3)).LengthSquared;
			if (lengthSquared < 0.1f)
			{
				float num = MathF.Sqrt(lengthSquared);
				_trackSphere.Origin = val2.origin;
				_trackSphere.Radius = 0.05f + num * 0.01f + trackScale;
				Vec3 intersectionPoint = default(Vec3);
				if (RaySphereIntersection(mouseRay, _trackSphere, ref intersectionPoint))
				{
					result = val;
					break;
				}
			}
		}
		return result;
	}

	private MatrixFrame CalculateTrackFrame(Track track)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		Vec3 origin = ((CampaignVec2)(ref track.Position)).AsVec3();
		float scale = track.Scale;
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = origin;
		float num = default(float);
		Vec3 u = default(Vec3);
		Campaign.Current.MapSceneWrapper.GetTerrainHeightAndNormal(((Vec3)(ref identity.origin)).AsVec2, ref num, ref u);
		identity.rotation.u = u;
		Vec2 asVec = ((Vec3)(ref identity.rotation.f)).AsVec2;
		((Vec2)(ref asVec)).RotateCCW(track.Direction);
		identity.rotation.f = new Vec3(asVec.x, asVec.y, identity.rotation.f.z, -1f);
		identity.rotation.s = Vec3.CrossProduct(identity.rotation.f, identity.rotation.u);
		((Vec3)(ref identity.rotation.s)).Normalize();
		identity.rotation.f = Vec3.CrossProduct(identity.rotation.u, identity.rotation.s);
		((Vec3)(ref identity.rotation.f)).Normalize();
		float num2 = scale;
		ref Vec3 s = ref identity.rotation.s;
		s *= num2;
		ref Vec3 f = ref identity.rotation.f;
		f *= num2;
		ref Vec3 u2 = ref identity.rotation.u;
		u2 *= num2;
		return identity;
	}

	private GameEntity GetGameEntity()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Stack<GameEntity> entityPool = _entityPool;
		if (entityPool.Count != 0)
		{
			return entityPool.Pop();
		}
		GameEntity obj = GameEntity.Instantiate(base.MapScene, "map_track_arrow", MatrixFrame.Identity, true, "");
		obj.SetVisibilityExcludeParents(false);
		return obj;
	}

	private void PopulateEntityPool()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 256; i++)
		{
			GameEntity val = GameEntity.Instantiate(base.MapScene, "map_track_arrow", MatrixFrame.Identity, true, "");
			val.SetVisibilityExcludeParents(false);
			_entityPool.Push(val);
		}
	}

	private void ReleaseEntity(GameEntity e)
	{
		e.SetVisibilityExcludeParents(false);
		if (_entityPool == null)
		{
			_entityPool = new Stack<GameEntity>();
		}
		_entityPool.Push(e);
	}
}
