using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.AgentBehaviors;
using SandBox.Objects;
using SandBox.Objects.AnimationPoints;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Objects;

namespace SandBox.Missions.MissionLogics;

public class MissionPathGenerationLogic : MissionLogic
{
	public enum PointOfInterests
	{
		VisitPoint,
		CrossRoadPoint,
		GuardSpawnPoint,
		LookBackPoint
	}

	public class UsableMachineData
	{
		public SynchedMissionObject MissionObject;

		public Vec2 ClosestPointToPath;

		public float PathDistanceRatio;

		public bool IsAlreadyAddedToPath;

		public UsableMachineData(SynchedMissionObject missionObject, Vec2 closestPointToPath, float pathDistanceRatio)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			MissionObject = missionObject;
			ClosestPointToPath = closestPointToPath;
			PathDistanceRatio = pathDistanceRatio;
			IsAlreadyAddedToPath = false;
		}
	}

	public class NavigationPathData
	{
		public GameEntity StartingGameEntity;

		public GameEntity EndingGameEntity;

		public NavigationPath Path;

		public Dictionary<Vec2, float> PathNodeAndDistances;

		public List<UsableMachineData> ValidUsableMachinesData;

		public float TotalDistance;

		public NavigationPathData(List<UsableMachine> allUsablePoints, GameEntity startingEntity, GameEntity endingEntity, int disabledFaceId)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			ValidUsableMachinesData = new List<UsableMachineData>();
			StartingGameEntity = startingEntity;
			EndingGameEntity = endingEntity;
			Path = new NavigationPath();
			PathFaceRecord val = default(PathFaceRecord);
			((PathFaceRecord)(ref val))._002Ector(-1, -1, -1);
			Mission.Current.Scene.GetNavMeshFaceIndex(ref val, startingEntity.GlobalPosition, true);
			PathFaceRecord val2 = default(PathFaceRecord);
			((PathFaceRecord)(ref val2))._002Ector(-1, -1, -1);
			Mission.Current.Scene.GetNavMeshFaceIndex(ref val2, endingEntity.GlobalPosition, true);
			Scene scene = Mission.Current.Scene;
			int faceIndex = val.FaceIndex;
			int faceIndex2 = val2.FaceIndex;
			Vec3 globalPosition = startingEntity.GlobalPosition;
			Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
			globalPosition = endingEntity.GlobalPosition;
			scene.GetPathBetweenAIFaces(faceIndex, faceIndex2, asVec, ((Vec3)(ref globalPosition)).AsVec2, 0f, Path, new int[1] { disabledFaceId }, 1f);
			PathNodeAndDistances = new Dictionary<Vec2, float>();
			PathNodeAndDistances.Add(Path[0], 0f);
			float num = 0f;
			for (int i = 0; i < Path.Size - 1; i++)
			{
				Vec2 val3 = Path[i];
				Vec2 val4 = Path[i + 1];
				num += ((Vec2)(ref val3)).Distance(val4);
				PathNodeAndDistances.Add(val4, num);
			}
			TotalDistance = num;
			InitializeUsablePoints(allUsablePoints);
		}

		private NavigationPathData(NavigationPathData navigationPathData)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			Path = new NavigationPath();
			Path.Size = navigationPathData.Path.Size;
			for (int i = 0; i < navigationPathData.Path.Size; i++)
			{
				Path.PathPoints[i] = navigationPathData.Path.PathPoints[Path.Size - 1 - i];
			}
			TotalDistance = navigationPathData.TotalDistance;
			PathNodeAndDistances = new Dictionary<Vec2, float>();
			foreach (KeyValuePair<Vec2, float> pathNodeAndDistance in navigationPathData.PathNodeAndDistances)
			{
				PathNodeAndDistances.Add(pathNodeAndDistance.Key, TotalDistance - pathNodeAndDistance.Value);
			}
			ValidUsableMachinesData = new List<UsableMachineData>();
			foreach (UsableMachineData validUsableMachinesDatum in navigationPathData.ValidUsableMachinesData)
			{
				ValidUsableMachinesData.Add(new UsableMachineData(validUsableMachinesDatum.MissionObject, validUsableMachinesDatum.ClosestPointToPath, 1f - validUsableMachinesDatum.PathDistanceRatio));
			}
			StartingGameEntity = navigationPathData.EndingGameEntity;
			EndingGameEntity = navigationPathData.StartingGameEntity;
		}

		public NavigationPathData ReverseClone()
		{
			return new NavigationPathData(this);
		}

		private bool GetPositionData(Vec2 position, out Vec2 closestPointToPath, out float pathDistanceRatio)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			bool result = false;
			closestPointToPath = Vec2.Invalid;
			pathDistanceRatio = 0f;
			float num = float.MaxValue;
			for (int i = 0; i < Path.Size - 1; i++)
			{
				Vec2 key = Path[i];
				Vec2 val = Path[i + 1];
				Vec2 closestPointOnLineSegmentToPoint = MBMath.GetClosestPointOnLineSegmentToPoint(ref key, ref val, ref position);
				float num2 = ((Vec2)(ref position)).DistanceSquared(closestPointOnLineSegmentToPoint);
				if (num2 < 2f)
				{
					result = false;
					break;
				}
				if (num2 < 400f)
				{
					result = true;
					if (num2 < num)
					{
						closestPointToPath = closestPointOnLineSegmentToPoint;
						num = num2;
						pathDistanceRatio = (PathNodeAndDistances[key] + ((Vec2)(ref key)).Distance(closestPointOnLineSegmentToPoint)) / TotalDistance;
					}
				}
			}
			return result;
		}

		public void InitializeUsablePoints(List<UsableMachine> allUsableMachines)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			float num3 = float.MinValue;
			float num4 = float.MinValue;
			for (int i = 0; i < Path.Size; i++)
			{
				Vec2 val = Path[i];
				if (((Vec2)(ref val)).X > num3)
				{
					num3 = ((Vec2)(ref val)).X;
				}
				if (((Vec2)(ref val)).X < num)
				{
					num = ((Vec2)(ref val)).X;
				}
				if (((Vec2)(ref val)).Y > num4)
				{
					num4 = ((Vec2)(ref val)).Y;
				}
				if (((Vec2)(ref val)).Y < num2)
				{
					num2 = ((Vec2)(ref val)).Y;
				}
			}
			num3 += 20f;
			num4 += 20f;
			num -= 20f;
			num2 -= 20f;
			foreach (UsableMachine allUsableMachine in allUsableMachines)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)allUsableMachine).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				if (((Vec3)(ref globalPosition)).X > num3)
				{
					continue;
				}
				gameEntity = ((ScriptComponentBehavior)allUsableMachine).GameEntity;
				globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				if (((Vec3)(ref globalPosition)).X < num)
				{
					continue;
				}
				gameEntity = ((ScriptComponentBehavior)allUsableMachine).GameEntity;
				globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				if (((Vec3)(ref globalPosition)).Y > num4)
				{
					continue;
				}
				gameEntity = ((ScriptComponentBehavior)allUsableMachine).GameEntity;
				globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				if (!(((Vec3)(ref globalPosition)).Y < num2) && !(allUsableMachine is Chair))
				{
					gameEntity = ((ScriptComponentBehavior)allUsableMachine).GameEntity;
					globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
					if (GetPositionData(((Vec3)(ref globalPosition)).AsVec2, out var closestPointToPath, out var pathDistanceRatio))
					{
						ValidUsableMachinesData.Add(new UsableMachineData((SynchedMissionObject)(object)allUsableMachine, closestPointToPath, pathDistanceRatio));
					}
				}
			}
		}
	}

	public abstract class PointOfInterestBaseData
	{
		public float Score;

		public abstract PointOfInterests GetPointOfInterestType();

		public abstract List<(Vec2, float)> GetPositionAndRadiusPairs();

		public abstract bool IsInRadius(PointOfInterestBaseData otherPointOfInterest);

		public abstract float GetLocationRatio();
	}

	public class LookBackPointData : PointOfInterestBaseData
	{
		public WorldPosition WorldPosition;

		public WorldPosition DirectionWorldPosition;

		public float PathDistanceRatio;

		public LookBackPointData(WorldPosition position, WorldPosition direction, float pathDistanceRatio)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			WorldPosition = position;
			PathDistanceRatio = pathDistanceRatio;
			DirectionWorldPosition = direction;
		}

		public override PointOfInterests GetPointOfInterestType()
		{
			return PointOfInterests.LookBackPoint;
		}

		public override List<(Vec2, float)> GetPositionAndRadiusPairs()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			List<(Vec2, float)> list = new List<(Vec2, float)>();
			Vec3 navMeshVec = ((WorldPosition)(ref WorldPosition)).GetNavMeshVec3();
			list.Add((((Vec3)(ref navMeshVec)).AsVec2, 10f));
			return list;
		}

		public override bool IsInRadius(PointOfInterestBaseData otherPointOfInterest)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			if (otherPointOfInterest is LookBackPointData)
			{
				foreach (var positionAndRadiusPair in GetPositionAndRadiusPairs())
				{
					foreach (var positionAndRadiusPair2 in otherPointOfInterest.GetPositionAndRadiusPairs())
					{
						var (val, _) = positionAndRadiusPair;
						if (((Vec2)(ref val)).Distance(positionAndRadiusPair2.Item1) < 25f)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public override float GetLocationRatio()
		{
			return PathDistanceRatio;
		}
	}

	public class VisitPointNodeScoreData : PointOfInterestBaseData
	{
		public UsableMachineData VisitPointData;

		public bool UsingAsInteractablePoint;

		public WorldPosition PossibleBlendPointPosition;

		public List<(Vec2, float)> PositionAndRadiusPairs;

		public WorldPosition VisitPointPathStartPoint;

		public float VisitPointPathStartPointPathRatio;

		public WorldPosition ClosestPointToBlendPoint;

		public WorldPosition FWP;

		public WorldPosition SWP;

		public float StartingAngle;

		public Vec2 PathToVisitPoint;

		public VisitPointNodeScoreData(UsableMachineData visitPointData, WorldPosition possibleBlendPointPosition, WorldPosition visitPointPathStartPoint, float visitPointPathStartPointPathRatio, float score, float startingAngle, WorldPosition fWP, WorldPosition sWP, Vec2 pathToVisitPoint, WorldPosition closestPointToBlendPoint)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			VisitPointData = visitPointData;
			PossibleBlendPointPosition = possibleBlendPointPosition;
			VisitPointPathStartPoint = visitPointPathStartPoint;
			Score = score;
			PathToVisitPoint = pathToVisitPoint;
			SWP = sWP;
			FWP = fWP;
			ClosestPointToBlendPoint = closestPointToBlendPoint;
			VisitPointPathStartPointPathRatio = visitPointPathStartPointPathRatio;
			StartingAngle = startingAngle;
			PositionAndRadiusPairs = new List<(Vec2, float)>();
			List<(Vec2, float)> positionAndRadiusPairs = PositionAndRadiusPairs;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)visitPointData.MissionObject).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			positionAndRadiusPairs.Add((((Vec3)(ref globalPosition)).AsVec2, 7f));
			PositionAndRadiusPairs.Add((((WorldPosition)(ref PossibleBlendPointPosition)).AsVec2, 3f));
			PositionAndRadiusPairs.Add((((WorldPosition)(ref VisitPointPathStartPoint)).AsVec2, 3f));
			UsingAsInteractablePoint = false;
		}

		public override PointOfInterests GetPointOfInterestType()
		{
			return PointOfInterests.VisitPoint;
		}

		public override List<(Vec2, float)> GetPositionAndRadiusPairs()
		{
			return PositionAndRadiusPairs;
		}

		public override bool IsInRadius(PointOfInterestBaseData otherPointOfInterest)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			float num = 1f;
			if (otherPointOfInterest is VisitPointNodeScoreData)
			{
				num = 2f;
			}
			else if (otherPointOfInterest is CrossRoadScoreData)
			{
				num = 0.5f;
			}
			foreach (var positionAndRadiusPair in PositionAndRadiusPairs)
			{
				var (val, num2) = positionAndRadiusPair;
				foreach (var (val2, num3) in otherPointOfInterest.GetPositionAndRadiusPairs())
				{
					if (((Vec2)(ref val)).Distance(val2) < (num2 + num3) * num)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override float GetLocationRatio()
		{
			return VisitPointData.PathDistanceRatio;
		}
	}

	public class CrossRoadScoreData : PointOfInterestBaseData
	{
		public UsableMachineData LeftNode;

		public UsableMachineData RightNode;

		public List<(Vec2, float)> PositionAndRadiusPairs;

		public CrossRoadScoreData(UsableMachineData leftNode, UsableMachineData rightNode, float score)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			LeftNode = leftNode;
			RightNode = rightNode;
			Score = score;
			PositionAndRadiusPairs = new List<(Vec2, float)>();
			List<(Vec2, float)> positionAndRadiusPairs = PositionAndRadiusPairs;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)LeftNode.MissionObject).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			positionAndRadiusPairs.Add((((Vec3)(ref globalPosition)).AsVec2, 1f));
			List<(Vec2, float)> positionAndRadiusPairs2 = PositionAndRadiusPairs;
			gameEntity = ((ScriptComponentBehavior)RightNode.MissionObject).GameEntity;
			globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			positionAndRadiusPairs2.Add((((Vec3)(ref globalPosition)).AsVec2, 1f));
			PositionAndRadiusPairs.Add((RightNode.ClosestPointToPath, 1f));
			PositionAndRadiusPairs.Add((LeftNode.ClosestPointToPath, 1f));
		}

		public override PointOfInterests GetPointOfInterestType()
		{
			return PointOfInterests.CrossRoadPoint;
		}

		public override List<(Vec2, float)> GetPositionAndRadiusPairs()
		{
			return PositionAndRadiusPairs;
		}

		public override bool IsInRadius(PointOfInterestBaseData otherPointOfInterest)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			foreach (var positionAndRadiusPair in PositionAndRadiusPairs)
			{
				var (val, num) = positionAndRadiusPair;
				foreach (var (val2, num2) in otherPointOfInterest.GetPositionAndRadiusPairs())
				{
					if (((Vec2)(ref val)).Distance(val2) < num + num2)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override float GetLocationRatio()
		{
			return (LeftNode.PathDistanceRatio + RightNode.PathDistanceRatio) * 0.5f;
		}
	}

	public class StandingGuardSpawnData : PointOfInterestBaseData
	{
		public UsableMachineData GuardPointData;

		public Vec2 SpawnDirection;

		public List<(Vec2, float)> PositionAndRadiusPairs;

		public StandingGuardSpawnData(UsableMachineData guardPointData, Vec2 spawnDirection, float score)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			GuardPointData = guardPointData;
			SpawnDirection = spawnDirection;
			Score = score;
			PositionAndRadiusPairs = new List<(Vec2, float)>();
			List<(Vec2, float)> positionAndRadiusPairs = PositionAndRadiusPairs;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)GuardPointData.MissionObject).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			positionAndRadiusPairs.Add((((Vec3)(ref globalPosition)).AsVec2, 2f));
		}

		public override PointOfInterests GetPointOfInterestType()
		{
			return PointOfInterests.GuardSpawnPoint;
		}

		public override List<(Vec2, float)> GetPositionAndRadiusPairs()
		{
			return PositionAndRadiusPairs;
		}

		public override bool IsInRadius(PointOfInterestBaseData otherPointOfInterest)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			foreach (var positionAndRadiusPair in PositionAndRadiusPairs)
			{
				var (val, num) = positionAndRadiusPair;
				foreach (var (val2, num2) in otherPointOfInterest.GetPositionAndRadiusPairs())
				{
					if (((Vec2)(ref val)).Distance(val2) < num + num2)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override float GetLocationRatio()
		{
			return GuardPointData.PathDistanceRatio;
		}
	}

	public class PointOfInterestScorePair
	{
		public NavigationPathData PathData;

		private List<PointOfInterestBaseData> _data;

		public Dictionary<PointOfInterests, int> PointOfInterestCount;

		public float Score;

		public List<PointOfInterestBaseData> Data => _data;

		public PointOfInterestScorePair(NavigationPathData pathData, List<PointOfInterestBaseData> data, float score)
		{
			PathData = pathData;
			_data = data;
			Score = score;
			PointOfInterestCount = new Dictionary<PointOfInterests, int>();
			PointOfInterests[] array = (PointOfInterests[])Enum.GetValues(typeof(PointOfInterests));
			foreach (PointOfInterests key in array)
			{
				PointOfInterestCount.Add(key, 0);
			}
			foreach (PointOfInterestBaseData datum in _data)
			{
				PointOfInterestCount[datum.GetPointOfInterestType()]++;
			}
		}

		private PointOfInterestScorePair(PointOfInterestScorePair otherPair)
		{
			PathData = otherPair.PathData;
			_data = otherPair._data.ToList();
			Score = otherPair.Score;
			PointOfInterestCount = otherPair.PointOfInterestCount.ToDictionary((KeyValuePair<PointOfInterests, int> x) => x.Key, (KeyValuePair<PointOfInterests, int> x) => x.Value);
		}

		public PointOfInterestScorePair Clone()
		{
			return new PointOfInterestScorePair(this);
		}

		public void AddToData(PointOfInterestBaseData pointOfInterestToAdd)
		{
			PointOfInterestCount[pointOfInterestToAdd.GetPointOfInterestType()]++;
			_data.Add(pointOfInterestToAdd);
			Score += pointOfInterestToAdd.Score;
		}

		public bool IsDataEqualTo(PointOfInterestScorePair other, PointOfInterestBaseData newDataToAdd)
		{
			if (PathData != other.PathData || other.Data.Count + 1 != Data.Count || !MBMath.ApproximatelyEqualsTo(Score, other.Score + newDataToAdd.Score, 1E-05f) || Data[Data.Count - 1] != newDataToAdd)
			{
				return false;
			}
			for (int num = other.Data.Count - 1; num >= 0; num--)
			{
				if (other.Data[num] != Data[num])
				{
					return false;
				}
			}
			return true;
		}

		public bool IsBetterThan(PointOfInterestScorePair other)
		{
			float num = (float)(MaximumVisitPointCountInPath + MinimumVisitPointCountInPath) * 0.5f;
			float num2 = Math.Abs((float)PointOfInterestCount[PointOfInterests.VisitPoint] - num);
			float num3 = Math.Abs((float)other.PointOfInterestCount[PointOfInterests.VisitPoint] - num);
			float num4 = 0.5f;
			float num5 = ((Score >= other.Score) ? 0.2f : (-0.2f));
			float num6 = ((num3 >= num2) ? 0.2f : (-0.2f));
			return num4 + num5 + num6 > 0.5f;
		}

		public bool IsSufficient()
		{
			int num = PointOfInterestCount[PointOfInterests.VisitPoint];
			int num2 = PointOfInterestCount[PointOfInterests.CrossRoadPoint];
			if (Score >= (float)ScoreToAchieve && PathData.TotalDistance >= (float)MinimumPathDistance && PathData.TotalDistance <= (float)MaximumPathDistance && num >= MinimumVisitPointCountInPath && num <= MaximumVisitPointCountInPath && num2 >= MinimumCrossRoadCountInPath)
			{
				return num2 <= MaximumCrossRoadCountInPath;
			}
			return false;
		}

		public void ReOrderDataAccordingToPathRatios()
		{
			_data = _data.OrderBy((PointOfInterestBaseData x) => x.GetLocationRatio()).ToList();
		}
	}

	private const float MaximumPathNodeDistanceSquaredToCheckForCrossRoads = 100f;

	private const float MinimumPathNodeDistanceSquaredToCheckForCrossRoads = 25f;

	private const float StandingGuardCountPerXMeter = 10f;

	private const float HumanMonsterCapsuleRadius = 0.37f;

	private const float MinimumStandingGuardSpawnDistance = 3f;

	private const float OptimumStandingGuardSpawnDistance = 5f;

	private const float MaximumStandingGuardSpawnDistance = 30f;

	private const float DoNotSpawnVisitPointPathRatioMin = 0.2f;

	private const float DoNotSpawnVisitPointPathRatioMax = 0.9f;

	private const float OptimumPathIndexRatioForVisitPoint = 0.75f;

	private const float FilterPadding = 20f;

	private const string VisitBarrelPrefabName = "disguise_mission_interactable_barrel";

	private const bool PlayerCompromised = false;

	private readonly CharacterObject _defaultDisguiseCharacter;

	private int _disabledFaceId;

	public static int MinimumPathDistance = 200;

	public static int MaximumPathDistance = 600;

	public float MinimumDistanceToBlendPointToVisitPoint = 5f;

	private PointOfInterestScorePair _selectedPath;

	public static int MinimumVisitPointCountInPath = 2;

	public static int MaximumVisitPointCountInPath = 10;

	public static int MinimumCrossRoadCountInPath = 2;

	public static int MaximumCrossRoadCountInPath = 10;

	public static int MinimumStandingGuardCountInPath = 5;

	public static int MaximumStandingGuardCountInPath = 50;

	public static float MinimumGuardSpawnPathRatio = 0.15f;

	public static int MaximumLookBackPointCountInPath;

	public static int ScoreToAchieve;

	private Dictionary<Agent, bool> _crossRoadAgentData;

	private DisguiseMissionLogic _disguiseMissionLogic;

	private readonly List<GameEntity> _visitBarrelEntities;

	public List<GameEntity> _startAndFinishPointPool;

	private GameEntity _currentStarting;

	private GameEntity _currentEnding;

	public int CrossRoadMaximumDistance = 30;

	public int CrossRoadMinimumDistance = 10;

	public int MinimumVisitPointDistance = 10;

	public int MaximumVisitPointDistance = 40;

	private List<UsableMachineData> _nearbyLeftSideUsableMachinesCache;

	private List<UsableMachineData> _nearbyRightSideUsableMachinesCache;

	private List<PointOfInterestBaseData> _allTargetAgentPointOfInterest;

	private WorldPosition _tempWorldPosition;

	public MissionPathGenerationLogic(CharacterObject defaultDisguiseCharacter)
	{
		_defaultDisguiseCharacter = defaultDisguiseCharacter;
		_selectedPath = null;
		_nearbyLeftSideUsableMachinesCache = new List<UsableMachineData>();
		_nearbyRightSideUsableMachinesCache = new List<UsableMachineData>();
		_allTargetAgentPointOfInterest = new List<PointOfInterestBaseData>();
		_crossRoadAgentData = new Dictionary<Agent, bool>();
		_visitBarrelEntities = new List<GameEntity>();
		_startAndFinishPointPool = new List<GameEntity>();
	}

	public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (userAgent.IsMainAgent)
		{
			GameEntity item = GameEntity.CreateFromWeakEntity(((ScriptComponentBehavior)usedObject).GameEntity);
			if (_visitBarrelEntities.Contains(item))
			{
				userAgent.SetActionChannel(0, ref ActionIndexCache.act_smithing_machine_anvil_start, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				_visitBarrelEntities.Remove(item);
			}
		}
	}

	private void SpawnDisguiseAgents()
	{
		foreach (PointOfInterestBaseData datum in _selectedPath.Data)
		{
			if (datum is CrossRoadScoreData selectedCrossRoad)
			{
				SpawnCrossRoadAgents(selectedCrossRoad);
			}
			else if (datum is StandingGuardSpawnData standingGuardSpawnPoint)
			{
				SpawnStandingGuards(standingGuardSpawnPoint);
			}
			else if (datum is VisitPointNodeScoreData visitPointNodeScoreData)
			{
				SpawnVisitPointGuardsAndBlendPoints(visitPointNodeScoreData, useAsBarrelPoint: true);
				_allTargetAgentPointOfInterest.Add(visitPointNodeScoreData);
			}
			else if (datum is LookBackPointData item)
			{
				_allTargetAgentPointOfInterest.Add(item);
			}
		}
		_allTargetAgentPointOfInterest = _allTargetAgentPointOfInterest.OrderBy((PointOfInterestBaseData x) => x.GetLocationRatio()).ToList();
	}

	private void SpawnVisitPointGuardsAndBlendPoints(VisitPointNodeScoreData visitPointData, bool useAsBarrelPoint)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		SynchedMissionObject missionObject = visitPointData.VisitPointData.MissionObject;
		FadeOutUserAgentsInUsableMachine((UsableMachine)(object)((missionObject is UsableMachine) ? missionObject : null));
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)visitPointData.VisitPointData.MissionObject).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Mat3 rotation = globalFrame.rotation;
		gameEntity = ((ScriptComponentBehavior)visitPointData.VisitPointData.MissionObject).GameEntity;
		WorldFrame val = default(WorldFrame);
		((WorldFrame)(ref val))._002Ector(rotation, new WorldPosition(((WeakGameEntity)(ref gameEntity)).Scene, globalFrame.origin));
		Vec2 asVec;
		if (useAsBarrelPoint)
		{
			Vec3 groundVec = ((WorldPosition)(ref val.Origin)).GetGroundVec3();
			float num = float.MaxValue;
			Vec3 val2 = Vec3.Zero;
			for (int i = 0; (float)i < 360f; i++)
			{
				((Mat3)(ref val.Rotation)).RotateAboutUp(MathF.PI / 180f);
				Vec3 lastPointOnNavigationMeshFromWorldPositionToDestination = Mission.Current.Scene.GetLastPointOnNavigationMeshFromWorldPositionToDestination(ref val.Origin, ((WorldPosition)(ref val.Origin)).AsVec2 + ((Vec3)(ref val.Rotation.f)).AsVec2 * 30f);
				asVec = ((WorldPosition)(ref val.Origin)).AsVec2;
				float num2 = ((Vec2)(ref asVec)).Distance(((Vec3)(ref lastPointOnNavigationMeshFromWorldPositionToDestination)).AsVec2);
				if (num2 < num)
				{
					num = num2;
					val2 = lastPointOnNavigationMeshFromWorldPositionToDestination;
				}
			}
			PathFaceRecord val3 = default(PathFaceRecord);
			((PathFaceRecord)(ref val3))._002Ector(-1, -1, -1);
			Mission.Current.Scene.GetNavMeshFaceIndex(ref val3, val2, true);
			Vec3 zero = Vec3.Zero;
			Mission.Current.Scene.GetNavMeshCenterPosition(val3.FaceIndex, ref zero);
			((WorldPosition)(ref val.Origin)).SetVec2(((Vec3)(ref val2)).AsVec2 + (((Vec3)(ref zero)).AsVec2 - ((Vec3)(ref val2)).AsVec2) * 0.25f);
			float num3 = Vec3.AngleBetweenTwoVectors(groundVec - val2, val.Rotation.f);
			((Mat3)(ref val.Rotation)).RotateAboutUp(MBMath.ToRadians(num3));
			GameEntity val4 = GameEntity.Instantiate(Mission.Current.Scene, "disguise_mission_interactable_barrel", ((WorldFrame)(ref val)).ToGroundMatrixFrame(), true, "");
			_visitBarrelEntities.Add(val4);
			visitPointData.UsingAsInteractablePoint = true;
			visitPointData.VisitPointData.MissionObject = (SynchedMissionObject)(object)val4.GetFirstScriptOfType<UsableMissionObject>();
		}
		else
		{
			Vec3 initialPosition = ((WorldPosition)(ref val.Origin)).GetGroundVec3() - val.Rotation.f;
			DisguiseMissionLogic disguiseMissionLogic = _disguiseMissionLogic;
			CharacterObject defaultDisguiseCharacter = _defaultDisguiseCharacter;
			asVec = ((Vec3)(ref val.Rotation.f)).AsVec2;
			Agent val5 = disguiseMissionLogic.SpawnDisguiseMissionAgentInternal(defaultDisguiseCharacter, initialPosition, ((Vec2)(ref asVec)).Normalized(), "_hideout_bandit");
			SynchedMissionObject missionObject2 = visitPointData.VisitPointData.MissionObject;
			UsableMachine val6 = (UsableMachine)(object)((missionObject2 is UsableMachine) ? missionObject2 : null);
			if (((IEnumerable<StandingPoint>)val6.StandingPoints).Any() && ((List<StandingPoint>)(object)val6.StandingPoints)[0] is AnimationPoint animationPoint)
			{
				ActionIndexCache val7 = ActionIndexCache.Create(animationPoint.LoopStartAction);
				val5.SetActionChannel(0, ref val7, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloat, false, -0.2f, 0, true);
			}
		}
		Vec2 val8 = ((WorldPosition)(ref visitPointData.ClosestPointToBlendPoint)).AsVec2 - ((WorldPosition)(ref visitPointData.PossibleBlendPointPosition)).AsVec2;
		_disguiseMissionLogic.SpawnDisguiseMissionAgentInternal(Settlement.CurrentSettlement.Culture.Beggar, ((WorldPosition)(ref visitPointData.PossibleBlendPointPosition)).GetNavMeshVec3(), ((Vec2)(ref val8)).Normalized(), "_hideout_bandit", isEnemy: false).SetActionChannel(0, ref ActionIndexCache.act_beggar_idle, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
	}

	private void SpawnStandingGuards(StandingGuardSpawnData standingGuardSpawnPoint)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		SynchedMissionObject missionObject = standingGuardSpawnPoint.GuardPointData.MissionObject;
		FadeOutUserAgentsInUsableMachine((UsableMachine)(object)((missionObject is UsableMachine) ? missionObject : null));
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)standingGuardSpawnPoint.GuardPointData.MissionObject).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		_disguiseMissionLogic.SpawnDisguiseMissionAgentInternal(_defaultDisguiseCharacter, globalFrame.origin, ((Vec2)(ref standingGuardSpawnPoint.SpawnDirection)).Normalized(), "_hideout_bandit");
	}

	private void SpawnCrossRoadAgents(CrossRoadScoreData selectedCrossRoad)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		SynchedMissionObject missionObject = selectedCrossRoad.LeftNode.MissionObject;
		FadeOutUserAgentsInUsableMachine((UsableMachine)(object)((missionObject is UsableMachine) ? missionObject : null));
		SynchedMissionObject missionObject2 = selectedCrossRoad.RightNode.MissionObject;
		FadeOutUserAgentsInUsableMachine((UsableMachine)(object)((missionObject2 is UsableMachine) ? missionObject2 : null));
		WeakGameEntity gameEntity;
		MatrixFrame globalFrame;
		if (!(MBRandom.RandomFloat < 0.5f))
		{
			gameEntity = ((ScriptComponentBehavior)selectedCrossRoad.RightNode.MissionObject).GameEntity;
			globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		}
		else
		{
			gameEntity = ((ScriptComponentBehavior)selectedCrossRoad.LeftNode.MissionObject).GameEntity;
			globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		}
		MatrixFrame val = globalFrame;
		DisguiseMissionLogic disguiseMissionLogic = _disguiseMissionLogic;
		CharacterObject defaultDisguiseCharacter = _defaultDisguiseCharacter;
		Vec3 origin = val.origin;
		Vec2 asVec = ((Vec3)(ref val.rotation.f)).AsVec2;
		Agent val2 = disguiseMissionLogic.SpawnDisguiseMissionAgentInternal(defaultDisguiseCharacter, origin, ((Vec2)(ref asVec)).Normalized(), "_hideout_bandit");
		_crossRoadAgentData.Add(val2, value: false);
		ScriptBehavior.AddTargetWithDelegate(val2, CrossRoadAgentSelectTargetDelegate(selectedCrossRoad), CrossRoadAgentWaitDelegate, CrossRoadAgentOnTargetReachDelegate);
	}

	private void CrossRoadAgentWaitDelegate(Agent agent, ref float waitTimeInSeconds)
	{
		waitTimeInSeconds = MBRandom.RandomInt(6, 30);
	}

	private bool CrossRoadAgentOnTargetReachDelegate(Agent agent1, ref Agent targetAgent, ref UsableMachine machine, ref WorldFrame frame)
	{
		_crossRoadAgentData[agent1] = !_crossRoadAgentData[agent1];
		return true;
	}

	private ScriptBehavior.SelectTargetDelegate CrossRoadAgentSelectTargetDelegate(CrossRoadScoreData selectedCrossRoad)
	{
		return delegate(Agent agent1, ref Agent targetAgent, ref UsableMachine machine, ref WorldFrame frame, ref float customTargetReachedRangeThreshold, ref float customTargetReachedRotationThreshold)
		{
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			customTargetReachedRangeThreshold = 2.5f;
			customTargetReachedRotationThreshold = 0.8f;
			WeakGameEntity gameEntity;
			if (_crossRoadAgentData[agent1])
			{
				Scene scene = Mission.Current.Scene;
				gameEntity = ((ScriptComponentBehavior)selectedCrossRoad.LeftNode.MissionObject).GameEntity;
				WorldPosition val = default(WorldPosition);
				((WorldPosition)(ref val))._002Ector(scene, ((WeakGameEntity)(ref gameEntity)).GlobalPosition);
				gameEntity = ((ScriptComponentBehavior)selectedCrossRoad.LeftNode.MissionObject).GameEntity;
				frame = new WorldFrame(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation, val);
			}
			else
			{
				Scene scene2 = Mission.Current.Scene;
				gameEntity = ((ScriptComponentBehavior)selectedCrossRoad.RightNode.MissionObject).GameEntity;
				WorldPosition val2 = default(WorldPosition);
				((WorldPosition)(ref val2))._002Ector(scene2, ((WeakGameEntity)(ref gameEntity)).GlobalPosition);
				gameEntity = ((ScriptComponentBehavior)selectedCrossRoad.RightNode.MissionObject).GameEntity;
				frame = new WorldFrame(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation, val2);
			}
			return true;
		};
	}

	private float CalculateCrossRoadScoreForUsableMachines(UsableMachineData leftSideUsableMachineData, UsableMachineData rightSideUsableMachineData, NavigationPath originalPath, WorldPosition pathNodeStartPosition, WorldPosition pathNodeEndPosition)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		if (leftSideUsableMachineData.PathDistanceRatio < 0.1f || rightSideUsableMachineData.PathDistanceRatio < 0.1f)
		{
			return 0f;
		}
		float num = ((Vec2)(ref leftSideUsableMachineData.ClosestPointToPath)).Distance(rightSideUsableMachineData.ClosestPointToPath);
		Vec3 val = ((WorldPosition)(ref pathNodeStartPosition)).GetNavMeshVec3();
		if (num > ((Vec3)(ref val)).Distance(((WorldPosition)(ref pathNodeEndPosition)).GetNavMeshVec3()))
		{
			return 0f;
		}
		ref WorldPosition tempWorldPosition = ref _tempWorldPosition;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)leftSideUsableMachineData.MissionObject).GameEntity;
		val = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		((WorldPosition)(ref tempWorldPosition)).SetVec2(((Vec3)(ref val)).AsVec2);
		((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
		WorldPosition tempWorldPosition2 = _tempWorldPosition;
		ref WorldPosition tempWorldPosition3 = ref _tempWorldPosition;
		gameEntity = ((ScriptComponentBehavior)rightSideUsableMachineData.MissionObject).GameEntity;
		val = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		((WorldPosition)(ref tempWorldPosition3)).SetVec2(((Vec3)(ref val)).AsVec2);
		((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
		WorldPosition tempWorldPosition4 = _tempWorldPosition;
		float num2 = default(float);
		Mission.Current.Scene.GetPathDistanceBetweenPositions(ref tempWorldPosition2, ref tempWorldPosition4, 0.37f, ref num2);
		if (num2 > (float)CrossRoadMaximumDistance || num2 < (float)CrossRoadMinimumDistance)
		{
			return 0f;
		}
		((WorldPosition)(ref _tempWorldPosition)).SetVec2(leftSideUsableMachineData.ClosestPointToPath);
		((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
		WorldPosition tempWorldPosition5 = _tempWorldPosition;
		((WorldPosition)(ref _tempWorldPosition)).SetVec2(rightSideUsableMachineData.ClosestPointToPath);
		((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
		WorldPosition tempWorldPosition6 = _tempWorldPosition;
		float num3 = default(float);
		((MissionBehavior)this).Mission.Scene.GetPathDistanceBetweenPositions(ref tempWorldPosition2, ref tempWorldPosition4, 0.37f, ref num3);
		Vec2 val2 = ((WorldPosition)(ref pathNodeStartPosition)).AsVec2;
		((Vec2)(ref val2)).Distance(((WorldPosition)(ref pathNodeEndPosition)).AsVec2);
		float num4 = default(float);
		Mission.Current.Scene.GetPathDistanceBetweenPositions(ref tempWorldPosition2, ref tempWorldPosition5, 0.37f, ref num4);
		float num5 = default(float);
		Mission.Current.Scene.GetPathDistanceBetweenPositions(ref tempWorldPosition6, ref tempWorldPosition4, 0.37f, ref num5);
		if (num4 > num5 && num5 / num4 < 0.2f)
		{
			return 0f;
		}
		if (num5 > num4 && num4 / num5 < 0.2f)
		{
			return 0f;
		}
		val2 = ((WorldPosition)(ref pathNodeEndPosition)).AsVec2 - ((WorldPosition)(ref pathNodeStartPosition)).AsVec2;
		float value = MBMath.ToDegrees(((Vec2)(ref val2)).AngleBetween(((WorldPosition)(ref tempWorldPosition2)).AsVec2 - ((WorldPosition)(ref tempWorldPosition4)).AsVec2));
		if (Math.Abs(value) > 150f || Math.Abs(value) < 30f)
		{
			return 0f;
		}
		float num6 = 0f;
		num6 = ((!(Math.Abs(value) > 90f)) ? MBMath.Map(Math.Abs(value), 30f, 90f, 0f, 1f) : MBMath.Map(Math.Abs(value), 90f, 150f, 1f, 0f));
		float num7 = MBMath.Map(num4 + num5, 0f, 20f, 0f, 1f);
		return num6 + num7;
	}

	private float CalculateVisitPointScore(UsableMachineData usableMachineData, NavigationPath originalPath, WorldPosition pathNodeStart, WorldPosition pathNodeEnd, out Vec3 possibleBlendPointPosition, out float startingAngle, out Vec2 pathToVisitPointZero, out Vec2 closestPointToPath)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_044f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		possibleBlendPointPosition = Vec3.Invalid;
		startingAngle = 0f;
		pathToVisitPointZero = Vec2.Zero;
		closestPointToPath = Vec2.Invalid;
		if (usableMachineData.PathDistanceRatio < 0.2f || usableMachineData.PathDistanceRatio > 0.9f)
		{
			return 0f;
		}
		Scene scene = Mission.Current.Scene;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)usableMachineData.MissionObject).GameEntity;
		WorldPosition val = default(WorldPosition);
		((WorldPosition)(ref val))._002Ector(scene, ((WeakGameEntity)(ref gameEntity)).GlobalPosition);
		((WorldPosition)(ref _tempWorldPosition)).SetVec2(usableMachineData.ClosestPointToPath);
		((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
		WorldPosition tempWorldPosition = _tempWorldPosition;
		NavigationPath val2 = new NavigationPath();
		((MissionBehavior)this).Mission.Scene.GetPathBetweenAIFaces(((WorldPosition)(ref pathNodeStart)).GetNearestNavMesh(), ((WorldPosition)(ref val)).GetNearestNavMesh(), ((WorldPosition)(ref pathNodeStart)).AsVec2, ((WorldPosition)(ref val)).AsVec2, 0f, val2, new int[1] { _disabledFaceId });
		Vec2 val3 = ((WorldPosition)(ref pathNodeStart)).AsVec2 + (((WorldPosition)(ref pathNodeEnd)).AsVec2 - ((WorldPosition)(ref pathNodeStart)).AsVec2) * 0.5f;
		((WorldPosition)(ref _tempWorldPosition)).SetVec2(val3);
		((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
		WorldPosition tempWorldPosition2 = _tempWorldPosition;
		Vec2 val4 = val2[0];
		float num = ((Vec2)(ref val4)).Distance(val3);
		for (int i = 0; i < val2.Size - 1; i++)
		{
			Vec2 val5 = val2[i];
			Vec2 val6 = val2[i + 1];
			num += ((Vec2)(ref val5)).Distance(val6);
		}
		if (num < (float)MinimumVisitPointDistance || num > (float)MaximumVisitPointDistance)
		{
			return 0f;
		}
		float num2 = 0f;
		Vec3 navMeshVec = ((WorldPosition)(ref pathNodeEnd)).GetNavMeshVec3();
		Vec2 asVec = ((Vec3)(ref navMeshVec)).AsVec2;
		navMeshVec = ((WorldPosition)(ref pathNodeStart)).GetNavMeshVec3();
		val4 = asVec - ((Vec3)(ref navMeshVec)).AsVec2;
		Vec2 val7 = val2[0];
		navMeshVec = ((WorldPosition)(ref tempWorldPosition2)).GetNavMeshVec3();
		startingAngle = MBMath.ToDegrees(((Vec2)(ref val4)).AngleBetween(val7 - ((Vec3)(ref navMeshVec)).AsVec2));
		pathToVisitPointZero = val2[0];
		if (Math.Abs(startingAngle) < 90f && Math.Abs(startingAngle) > 30f)
		{
			for (int j = 0; j < val2.Size - 1; j++)
			{
				Vec2 val8 = ((j == 0) ? ((WorldPosition)(ref tempWorldPosition)).AsVec2 : val2[j - 1]);
				Vec2 val9 = val2[j];
				Vec2 val10 = val2[j + 1];
				val4 = val9 - val8;
				float value = MBMath.ToDegrees(((Vec2)(ref val4)).AngleBetween(val10 - val9));
				num2 += MBMath.Map(Math.Abs(value), 0f, 90f, 1f, 0f);
				if (!((float)j > (float)(val2.Size - 1) * 0.25f) || ((Vec3)(ref possibleBlendPointPosition)).IsValid)
				{
					continue;
				}
				((WorldPosition)(ref _tempWorldPosition)).SetVec2(val9);
				((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
				WorldPosition tempWorldPosition3 = _tempWorldPosition;
				Vec3 navMeshVec2 = ((WorldPosition)(ref tempWorldPosition3)).GetNavMeshVec3();
				Vec3 val11 = Vec3.Invalid;
				PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
				int num3 = 0;
				float num4 = float.MaxValue;
				do
				{
					num3++;
					if (num3 > 150)
					{
						break;
					}
					val11 = ((MissionBehavior)this).Mission.GetRandomPositionAroundPoint(navMeshVec2, 2f, 6f, true);
					((MissionBehavior)this).Mission.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, val11, true);
					if (nullFaceRecord.FaceGroupIndex != _disabledFaceId)
					{
						continue;
					}
					for (int k = 0; k < val2.Size - 1; k++)
					{
						Vec2 val12 = val2[k];
						Vec2 val13 = val2[k + 1];
						val4 = ((Vec3)(ref val11)).AsVec2;
						Vec2 closestPointOnLineSegmentToPoint = MBMath.GetClosestPointOnLineSegmentToPoint(ref val12, ref val13, ref val4);
						val4 = ((Vec3)(ref val11)).AsVec2;
						float num5 = ((Vec2)(ref val4)).Distance(closestPointOnLineSegmentToPoint);
						if (num5 < num4)
						{
							closestPointToPath = closestPointOnLineSegmentToPoint;
							num4 = num5;
						}
					}
				}
				while (nullFaceRecord.FaceGroupIndex != _disabledFaceId || num4 < 1.5f);
				if (num3 < 150)
				{
					possibleBlendPointPosition = val11;
				}
			}
			num2 /= (float)val2.Size;
			if (((Vec3)(ref possibleBlendPointPosition)).IsValid)
			{
				val4 = ((Vec3)(ref possibleBlendPointPosition)).AsVec2;
				if (!(((Vec2)(ref val4)).Distance(((WorldPosition)(ref val)).AsVec2) < MinimumDistanceToBlendPointToVisitPoint))
				{
					for (int l = 0; l < originalPath.Size - 1; l++)
					{
						Vec2 val14 = originalPath[l];
						for (int m = 0; m < val2.Size - 1; m++)
						{
							val4 = val2[m];
							if (((Vec2)(ref val4)).Distance(val14) < 2f)
							{
								return 0f;
							}
						}
					}
					float num6 = 0f;
					float num7 = (float)(MaximumVisitPointDistance + MinimumVisitPointDistance) * 0.5f;
					num6 = ((!(num > num7)) ? MBMath.Map(num, (float)MinimumVisitPointDistance, num7, 0f, 0.5f) : MBMath.Map(num, num7, (float)MaximumVisitPointDistance, 0.5f, 1f));
					float num8 = 0f;
					SynchedMissionObject missionObject = usableMachineData.MissionObject;
					UsableMachine val15 = (UsableMachine)(object)((missionObject is UsableMachine) ? missionObject : null);
					if (((List<StandingPoint>)(object)val15.StandingPoints).Count > 0 && ((List<StandingPoint>)(object)val15.StandingPoints).Count != ((IEnumerable<StandingPoint>)val15.StandingPoints).Count((StandingPoint x) => x.HasAlternative()) && ((IEnumerable<StandingPoint>)val15.StandingPoints).Count((StandingPoint x) => x is AnimationPoint animationPoint && animationPoint.PairEntity != (GameEntity)null) == 2)
					{
						num8 = 2f;
					}
					float num9 = ((usableMachineData.PathDistanceRatio > 0.75f) ? MBMath.Map(usableMachineData.PathDistanceRatio, 0.75f, 1f, 1f, 0f) : MBMath.Map(usableMachineData.PathDistanceRatio, 0f, 0.75f, 0f, 1f));
					return 5f + num2 + num6 + num9 + num8;
				}
			}
			return 0f;
		}
		return 0f;
	}

	private float CalculateSpawnGuardScore(UsableMachineData guardSpawnPointData, out Vec2 spawnRotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		spawnRotation = Vec2.Zero;
		SynchedMissionObject missionObject = guardSpawnPointData.MissionObject;
		UsableMachine val = (UsableMachine)(object)((missionObject is UsableMachine) ? missionObject : null);
		if (val.PilotAgent != null)
		{
			return 0f;
		}
		foreach (StandingPoint item in (List<StandingPoint>)(object)val.StandingPoints)
		{
			if (((UsableMissionObject)item).UserAgent != null)
			{
				return 0f;
			}
		}
		if (guardSpawnPointData.PathDistanceRatio < MinimumGuardSpawnPathRatio)
		{
			return 0f;
		}
		ref Vec2 closestPointToPath = ref guardSpawnPointData.ClosestPointToPath;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)guardSpawnPointData.MissionObject).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		float num = ((Vec2)(ref closestPointToPath)).Distance(((Vec3)(ref globalPosition)).AsVec2);
		if (num < 3f)
		{
			return 0f;
		}
		float num2 = 0f;
		num2 = ((!(num > 5f)) ? MBMath.Map(num, 3f, 5f, 0f, 1f) : MBMath.Map(num, 5f, 30f, 1f, 0f));
		Vec2 closestPointToPath2 = guardSpawnPointData.ClosestPointToPath;
		gameEntity = ((ScriptComponentBehavior)guardSpawnPointData.MissionObject).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		spawnRotation = closestPointToPath2 - ((Vec3)(ref globalPosition)).AsVec2;
		return num2;
	}

	protected override void OnEndMission()
	{
		_nearbyLeftSideUsableMachinesCache = null;
		_nearbyRightSideUsableMachinesCache = null;
		_allTargetAgentPointOfInterest = null;
		_crossRoadAgentData = null;
		_startAndFinishPointPool = null;
	}

	public void InitializeBehavior()
	{
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("navigation_mesh_deactivator");
		if (val != (GameEntity)null)
		{
			NavigationMeshDeactivator firstScriptOfType = val.GetFirstScriptOfType<NavigationMeshDeactivator>();
			_disabledFaceId = firstScriptOfType.DisableFaceWithId;
		}
		_disguiseMissionLogic = Mission.Current.GetMissionBehavior<DisguiseMissionLogic>();
		Mission.Current.Scene.GetAllEntitiesWithScriptComponent<PassageUsePoint>(ref _startAndFinishPointPool);
		for (int num = _startAndFinishPointPool.Count - 1; num >= 0; num--)
		{
			Location toLocation = _startAndFinishPointPool[num].GetFirstScriptOfType<PassageUsePoint>().ToLocation;
			switch ((toLocation != null) ? toLocation.StringId : null)
			{
			case null:
			case "lordshall":
			case "prison":
				_startAndFinishPointPool.RemoveAt(num);
				break;
			}
		}
		Mission.Current.Scene.GetAllEntitiesWithScriptComponent<CastleGate>(ref _startAndFinishPointPool);
		foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("sp_player_conversation"))
		{
			_startAndFinishPointPool.Add(item);
		}
	}

	public override void OnMissionTick(float dt)
	{
	}

	private void FadeOutUserAgentsInUsableMachine(UsableMachine usableMachine)
	{
		if (usableMachine.PilotAgent != null)
		{
			usableMachine.PilotAgent.FadeOut(true, true);
		}
		foreach (StandingPoint item in (List<StandingPoint>)(object)usableMachine.StandingPoints)
		{
			if (((UsableMissionObject)item).UserAgent != null)
			{
				((UsableMissionObject)item).UserAgent.FadeOut(true, true);
			}
		}
		((MissionObject)usableMachine).SetDisabled(true);
	}

	private PointOfInterestScorePair CreatePathScorePair(NavigationPathData pathData)
	{
		List<VisitPointNodeScoreData> list = GetVisitPoints(pathData);
		List<CrossRoadScoreData> list2 = GetCrossRoadPoints(pathData);
		if (list.Count == 0 && list2.Count == 0)
		{
			return null;
		}
		List<PointOfInterestBaseData> list3 = new List<PointOfInterestBaseData>();
		Extensions.Shuffle<VisitPointNodeScoreData>((IList<VisitPointNodeScoreData>)list);
		Extensions.Shuffle<CrossRoadScoreData>((IList<CrossRoadScoreData>)list2);
		if (list2.Count > 20)
		{
			list2 = list2.OrderByDescending((CrossRoadScoreData x) => x.Score).Take(20).ToList();
			Extensions.Shuffle<CrossRoadScoreData>((IList<CrossRoadScoreData>)list2);
		}
		if (list.Count > 10)
		{
			list = list.OrderByDescending((VisitPointNodeScoreData x) => x.Score).Take(10).ToList();
			Extensions.Shuffle<VisitPointNodeScoreData>((IList<VisitPointNodeScoreData>)list);
		}
		list3.AddRange(list);
		list3.AddRange(list2);
		Extensions.Shuffle<PointOfInterestBaseData>((IList<PointOfInterestBaseData>)list3);
		Stack<(PointOfInterestScorePair, int)> stack = new Stack<(PointOfInterestScorePair, int)>();
		stack.Push((new PointOfInterestScorePair(pathData, new List<PointOfInterestBaseData>(), 0f), 0));
		return CreatePathDataWith(stack, list3);
	}

	private PointOfInterestScorePair CreatePathDataWith(Stack<(PointOfInterestScorePair, int)> stack, List<PointOfInterestBaseData> pointOfInterestData)
	{
		PointOfInterestScorePair pointOfInterestScorePair = null;
		while (stack.Count > 0)
		{
			(PointOfInterestScorePair, int) tuple = stack.Pop();
			int i;
			for (i = tuple.Item2; i < pointOfInterestData.Count; i++)
			{
				PointOfInterestBaseData data = pointOfInterestData[i];
				if (tuple.Item1.Data.All((PointOfInterestBaseData x) => !x.IsInRadius(data)))
				{
					PointOfInterestScorePair pointOfInterestScorePair2 = tuple.Item1.Clone();
					pointOfInterestScorePair2.AddToData(data);
					if (i + 1 < pointOfInterestData.Count)
					{
						stack.Push((tuple.Item1, i + 1));
						stack.Push((pointOfInterestScorePair2, i + 1));
					}
					if (pointOfInterestScorePair == null || pointOfInterestScorePair2.IsBetterThan(pointOfInterestScorePair))
					{
						pointOfInterestScorePair = pointOfInterestScorePair2;
					}
					if (pointOfInterestScorePair2.IsSufficient())
					{
						return pointOfInterestScorePair2;
					}
					i++;
					tuple = (pointOfInterestScorePair2, i);
					break;
				}
			}
			if (i == pointOfInterestData.Count && tuple.Item1.IsSufficient())
			{
				return tuple.Item1;
			}
		}
		return pointOfInterestScorePair;
	}

	private PointOfInterestScorePair GetRandomPath()
	{
		PointOfInterestScorePair pathInternal = GetPathInternal();
		if (pathInternal != null)
		{
			if (MaximumStandingGuardCountInPath > 0)
			{
				AddStandingGuardsToThePath(pathInternal);
			}
			if (MaximumLookBackPointCountInPath > 0)
			{
				AddLookBackPointsToThePath(pathInternal);
			}
		}
		return pathInternal;
	}

	private void AddLookBackPointsToThePath(PointOfInterestScorePair path)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<int, float> dictionary = new Dictionary<int, float>();
		for (int i = (int)((float)path.PathData.Path.Size * 0.25f); (float)i < (float)path.PathData.Path.Size * 0.9f; i++)
		{
			Vec2 key = path.PathData.Path[i];
			Vec2 key2 = path.PathData.Path[i + 1];
			if (!((Vec2)(ref key)).IsNonZero() || !((Vec2)(ref key2)).IsNonZero())
			{
				continue;
			}
			float num = path.PathData.PathNodeAndDistances[key] / path.PathData.TotalDistance;
			float num2 = path.PathData.PathNodeAndDistances[key2] / path.PathData.TotalDistance;
			float num3 = (num + num2) * 0.5f;
			float num4 = 0f;
			int num5 = 0;
			foreach (PointOfInterestBaseData datum in path.Data)
			{
				float locationRatio = datum.GetLocationRatio();
				if (locationRatio > num3 - 0.1f && locationRatio < num3 + 0.1f)
				{
					num4 += Math.Abs(num3 - locationRatio);
					num5++;
				}
			}
			if (num5 > 0)
			{
				num4 /= (float)num5;
			}
			dictionary.Add(i, num4);
		}
		if (!dictionary.Any())
		{
			return;
		}
		List<KeyValuePair<int, float>> list = dictionary.OrderByDescending((KeyValuePair<int, float> x) => x.Value).ToList();
		int num6 = 0;
		int num7 = ((MaximumLookBackPointCountInPath > 0) ? MBRandom.RandomInt((int)((float)MaximumLookBackPointCountInPath * 0.5f), MaximumLookBackPointCountInPath) : 0);
		if (num7 <= 0)
		{
			return;
		}
		_tempWorldPosition = new WorldPosition(Mission.Current.Scene, path.PathData.StartingGameEntity.GlobalPosition);
		((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
		for (int num8 = 0; num8 < path.PathData.Path.Size - 1; num8++)
		{
			Vec2 val = path.PathData.Path[num8];
			Vec2 val2 = path.PathData.Path[num8 + 1];
			((WorldPosition)(ref _tempWorldPosition)).SetVec2(val);
			((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
			((WorldPosition)(ref _tempWorldPosition)).SetVec2(val2);
			((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
			if (num6 == num7)
			{
				continue;
			}
			foreach (KeyValuePair<int, float> item in list)
			{
				int key3 = item.Key;
				if (num8 == key3)
				{
					Vec2 val3 = (val + val2) * 0.5f;
					float pathDistanceRatio = (path.PathData.PathNodeAndDistances[val] / path.PathData.TotalDistance + path.PathData.PathNodeAndDistances[val2] / path.PathData.TotalDistance) * 0.5f;
					Vec2 val4 = val2 - val;
					Vec2 vec = val3 + ((Vec2)(ref val4)).Normalized();
					((WorldPosition)(ref _tempWorldPosition)).SetVec2(val3);
					((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
					WorldPosition tempWorldPosition = _tempWorldPosition;
					((WorldPosition)(ref _tempWorldPosition)).SetVec2(vec);
					((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
					WorldPosition tempWorldPosition2 = _tempWorldPosition;
					LookBackPointData newData = new LookBackPointData(tempWorldPosition, tempWorldPosition2, pathDistanceRatio);
					if (path.Data.All((PointOfInterestBaseData x) => !x.IsInRadius(newData)))
					{
						path.AddToData(newData);
						num6++;
					}
					if (num6 == num7)
					{
						break;
					}
				}
			}
		}
	}

	private void AddStandingGuardsToThePath(PointOfInterestScorePair path)
	{
		int num = (int)(path.PathData.TotalDistance / 10f);
		num = MBMath.ClampInt(num, MinimumStandingGuardCountInPath, MaximumStandingGuardCountInPath);
		List<StandingGuardSpawnData> guardSpawnPoints = GetGuardSpawnPoints(path.PathData);
		int num2 = 0;
		for (int i = 0; i < guardSpawnPoints.Count; i++)
		{
			StandingGuardSpawnData randomElementWithPredicate = Extensions.GetRandomElementWithPredicate<StandingGuardSpawnData>((IReadOnlyList<StandingGuardSpawnData>)guardSpawnPoints, (Func<StandingGuardSpawnData, bool>)((StandingGuardSpawnData x) => path.Data.All((PointOfInterestBaseData y) => !y.IsInRadius(x))));
			if (randomElementWithPredicate != null)
			{
				path.AddToData(randomElementWithPredicate);
				num2++;
				if (num2 >= num)
				{
					break;
				}
			}
		}
	}

	public List<PointOfInterestScorePair> GetAllPossiblePaths()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		List<PointOfInterestScorePair> list = new List<PointOfInterestScorePair>();
		List<UsableMachine> usablePoints = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>().UsablePoints;
		for (int i = 0; i < _startAndFinishPointPool.Count - 1; i++)
		{
			for (int j = i + 1; j < _startAndFinishPointPool.Count; j++)
			{
				GameEntity val = _startAndFinishPointPool[i];
				GameEntity val2 = _startAndFinishPointPool[j];
				NavigationPathData navigationPathData = new NavigationPathData(usablePoints, val, val2, _disabledFaceId);
				_tempWorldPosition = new WorldPosition(Mission.Current.Scene, val.GlobalPosition);
				((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
				if (navigationPathData.TotalDistance < (float)MaximumPathDistance && navigationPathData.TotalDistance > (float)MinimumPathDistance)
				{
					PointOfInterestScorePair pointOfInterestScorePair = CreatePathScorePair(navigationPathData);
					if (pointOfInterestScorePair != null && pointOfInterestScorePair.Score > (float)ScoreToAchieve)
					{
						if (MaximumStandingGuardCountInPath > 0)
						{
							AddStandingGuardsToThePath(pointOfInterestScorePair);
						}
						if (MaximumLookBackPointCountInPath > 0)
						{
							AddLookBackPointsToThePath(pointOfInterestScorePair);
						}
						list.Add(pointOfInterestScorePair);
					}
				}
				NavigationPathData navigationPathData2 = navigationPathData.ReverseClone();
				_tempWorldPosition = new WorldPosition(Mission.Current.Scene, val2.GlobalPosition);
				((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
				if (!(navigationPathData2.TotalDistance < (float)MaximumPathDistance) || !(navigationPathData2.TotalDistance > (float)MinimumPathDistance))
				{
					continue;
				}
				PointOfInterestScorePair pointOfInterestScorePair2 = CreatePathScorePair(navigationPathData2);
				if (pointOfInterestScorePair2 != null && pointOfInterestScorePair2.Score > (float)ScoreToAchieve)
				{
					if (MaximumStandingGuardCountInPath > 0)
					{
						AddStandingGuardsToThePath(pointOfInterestScorePair2);
					}
					if (MaximumLookBackPointCountInPath > 0)
					{
						AddLookBackPointsToThePath(pointOfInterestScorePair2);
					}
					list.Add(pointOfInterestScorePair2);
				}
			}
		}
		return list;
	}

	public bool IsOnLeftSide(Vec2 lineA, Vec2 lineB, Vec2 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (lineB.x - lineA.x) * (point.y - lineA.y) - (lineB.y - lineA.y) * (point.x - lineA.x) > 0f;
	}

	private PointOfInterestScorePair GetPathInternal()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		List<UsableMachine> usablePoints = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>().UsablePoints;
		PointOfInterestScorePair pointOfInterestScorePair = null;
		for (int i = 0; i < _startAndFinishPointPool.Count - 1; i++)
		{
			for (int j = i + 1; j < _startAndFinishPointPool.Count; j++)
			{
				GameEntity val = _startAndFinishPointPool[i];
				GameEntity val2 = _startAndFinishPointPool[j];
				NavigationPathData navigationPathData = new NavigationPathData(usablePoints, val, val2, _disabledFaceId);
				_tempWorldPosition = new WorldPosition(Mission.Current.Scene, val.GlobalPosition);
				((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
				if (navigationPathData.TotalDistance < (float)MaximumPathDistance && navigationPathData.TotalDistance > (float)MinimumPathDistance)
				{
					PointOfInterestScorePair pointOfInterestScorePair2 = CreatePathScorePair(navigationPathData);
					if (pointOfInterestScorePair2 != null)
					{
						if (pointOfInterestScorePair2.IsSufficient())
						{
							_currentStarting = val;
							_currentEnding = val2;
							return pointOfInterestScorePair2;
						}
						if (pointOfInterestScorePair == null || pointOfInterestScorePair2.IsBetterThan(pointOfInterestScorePair))
						{
							pointOfInterestScorePair = pointOfInterestScorePair2;
						}
					}
				}
				NavigationPathData navigationPathData2 = navigationPathData.ReverseClone();
				_tempWorldPosition = new WorldPosition(Mission.Current.Scene, val2.GlobalPosition);
				((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
				if (!(navigationPathData2.TotalDistance < (float)MaximumPathDistance) || !(navigationPathData2.TotalDistance > (float)MinimumPathDistance))
				{
					continue;
				}
				PointOfInterestScorePair pointOfInterestScorePair3 = CreatePathScorePair(navigationPathData2);
				if (pointOfInterestScorePair3 != null)
				{
					if (pointOfInterestScorePair3.IsSufficient())
					{
						_currentStarting = val2;
						_currentEnding = val;
						return pointOfInterestScorePair3;
					}
					if (pointOfInterestScorePair == null || pointOfInterestScorePair3.IsBetterThan(pointOfInterestScorePair))
					{
						pointOfInterestScorePair = pointOfInterestScorePair3;
					}
				}
			}
		}
		if (pointOfInterestScorePair != null)
		{
			_currentStarting = pointOfInterestScorePair.PathData.StartingGameEntity;
			_currentEnding = pointOfInterestScorePair.PathData.EndingGameEntity;
		}
		return pointOfInterestScorePair;
	}

	private List<StandingGuardSpawnData> GetGuardSpawnPoints(NavigationPathData pathData)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		List<StandingGuardSpawnData> list = new List<StandingGuardSpawnData>();
		foreach (UsableMachineData validUsableMachinesDatum in pathData.ValidUsableMachinesData)
		{
			Vec2 spawnRotation;
			float num = CalculateSpawnGuardScore(validUsableMachinesDatum, out spawnRotation);
			if (num > 0f)
			{
				list.Add(new StandingGuardSpawnData(validUsableMachinesDatum, spawnRotation, num));
			}
		}
		return list;
	}

	private List<VisitPointNodeScoreData> GetVisitPoints(NavigationPathData pathData)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		List<VisitPointNodeScoreData> list = new List<VisitPointNodeScoreData>();
		NavigationPath path = pathData.Path;
		for (int i = 0; i < path.Size - 1; i++)
		{
			Vec2 val = path[i];
			Vec2 val2 = path[i + 1];
			((WorldPosition)(ref _tempWorldPosition)).SetVec2(val);
			((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
			WorldPosition tempWorldPosition = _tempWorldPosition;
			((WorldPosition)(ref _tempWorldPosition)).SetVec2(val2);
			((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
			WorldPosition tempWorldPosition2 = _tempWorldPosition;
			foreach (UsableMachineData validUsableMachinesDatum in pathData.ValidUsableMachinesData)
			{
				if (!validUsableMachinesDatum.IsAlreadyAddedToPath)
				{
					Vec3 possibleBlendPointPosition;
					float startingAngle;
					Vec2 pathToVisitPointZero;
					Vec2 closestPointToPath;
					float num = CalculateVisitPointScore(validUsableMachinesDatum, path, tempWorldPosition, tempWorldPosition2, out possibleBlendPointPosition, out startingAngle, out pathToVisitPointZero, out closestPointToPath);
					if (num > 0f)
					{
						Vec2 vec = val + (val2 - val) * 0.5f;
						((WorldPosition)(ref _tempWorldPosition)).SetVec2(val);
						((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
						((WorldPosition)(ref _tempWorldPosition)).SetVec2(vec);
						((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
						WorldPosition tempWorldPosition3 = _tempWorldPosition;
						((WorldPosition)(ref _tempWorldPosition)).SetVec2(val2);
						((WorldPosition)(ref _tempWorldPosition)).GetNavMeshVec3();
						((WorldPosition)(ref _tempWorldPosition)).SetVec2(((Vec3)(ref possibleBlendPointPosition)).AsVec2);
						((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
						WorldPosition tempWorldPosition4 = _tempWorldPosition;
						((WorldPosition)(ref _tempWorldPosition)).SetVec2(closestPointToPath);
						((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
						WorldPosition tempWorldPosition5 = _tempWorldPosition;
						float visitPointPathStartPointPathRatio = (pathData.PathNodeAndDistances[val] / pathData.TotalDistance + pathData.PathNodeAndDistances[val2] / pathData.TotalDistance) * 0.5f;
						list.Add(new VisitPointNodeScoreData(validUsableMachinesDatum, tempWorldPosition4, tempWorldPosition3, visitPointPathStartPointPathRatio, num, startingAngle, tempWorldPosition, tempWorldPosition2, pathToVisitPointZero, tempWorldPosition5));
						validUsableMachinesDatum.IsAlreadyAddedToPath = true;
					}
				}
			}
		}
		return list;
	}

	private List<CrossRoadScoreData> GetCrossRoadPoints(NavigationPathData pathData)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		List<CrossRoadScoreData> list = new List<CrossRoadScoreData>();
		for (int i = 0; i < pathData.Path.Size - 1; i++)
		{
			_nearbyLeftSideUsableMachinesCache.Clear();
			_nearbyRightSideUsableMachinesCache.Clear();
			Vec2 val = pathData.Path[i];
			Vec2 val2 = pathData.Path[i + 1];
			((WorldPosition)(ref _tempWorldPosition)).SetVec2(val);
			((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
			WorldPosition tempWorldPosition = _tempWorldPosition;
			((WorldPosition)(ref _tempWorldPosition)).SetVec2(val2);
			((WorldPosition)(ref _tempWorldPosition)).GetNavMeshZ();
			WorldPosition tempWorldPosition2 = _tempWorldPosition;
			float num = ((Vec2)(ref val2)).DistanceSquared(val);
			if (!(num > 25f) || !(num < 100f))
			{
				continue;
			}
			WeakGameEntity gameEntity;
			Vec3 globalPosition;
			foreach (UsableMachineData validUsableMachinesDatum in pathData.ValidUsableMachinesData)
			{
				if (!validUsableMachinesDatum.IsAlreadyAddedToPath)
				{
					Vec2 lineB = val2;
					gameEntity = ((ScriptComponentBehavior)validUsableMachinesDatum.MissionObject).GameEntity;
					globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
					if (IsOnLeftSide(val, lineB, ((Vec3)(ref globalPosition)).AsVec2))
					{
						_nearbyLeftSideUsableMachinesCache.Add(validUsableMachinesDatum);
					}
					else
					{
						_nearbyRightSideUsableMachinesCache.Add(validUsableMachinesDatum);
					}
				}
			}
			foreach (UsableMachineData item in _nearbyLeftSideUsableMachinesCache)
			{
				foreach (UsableMachineData item2 in _nearbyRightSideUsableMachinesCache)
				{
					gameEntity = ((ScriptComponentBehavior)item.MissionObject).GameEntity;
					globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
					gameEntity = ((ScriptComponentBehavior)item2.MissionObject).GameEntity;
					((Vec3)(ref globalPosition)).Distance(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
					if (!item.IsAlreadyAddedToPath && !item2.IsAlreadyAddedToPath)
					{
						float num2 = CalculateCrossRoadScoreForUsableMachines(item, item2, pathData.Path, tempWorldPosition, tempWorldPosition2);
						if (num2 > 0f)
						{
							list.Add(new CrossRoadScoreData(item, item2, num2));
							item.IsAlreadyAddedToPath = true;
							item2.IsAlreadyAddedToPath = true;
						}
					}
				}
			}
		}
		return list;
	}

	private void ShowMissionFailedPopup()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0028: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_007c: Expected O, but got Unknown
		TextObject val = new TextObject("{=CMu4B9fZ}Mission Failed", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=RcY8uZA1}You have lost the target.", (Dictionary<string, object>)null);
		TextObject val3 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)val3).ToString(), (string)null, (Action)delegate
		{
			Mission.Current.EndMission();
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), (int)Campaign.Current.GameMode == 1, false);
	}
}
