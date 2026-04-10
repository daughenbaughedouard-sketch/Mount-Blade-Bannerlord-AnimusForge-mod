using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Map.DistanceCache;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace SandBox.View.Map;

public class SettlementPositionScript : ScriptComponentBehavior
{
	private sealed class SettlementRecord : ISettlementDataHolder
	{
		public readonly string SettlementId;

		public readonly XmlNode Node;

		public readonly Vec2 Position;

		public readonly Vec2 GatePosition;

		public readonly bool HasGate;

		public readonly Vec2 PortPosition;

		public readonly bool HasPort;

		public readonly bool IsFortification;

		public string StringId => SettlementId;

		CampaignVec2 ISettlementDataHolder.GatePosition => new CampaignVec2(GatePosition, true);

		CampaignVec2 ISettlementDataHolder.PortPosition => new CampaignVec2(PortPosition, false);

		bool ISettlementDataHolder.IsFortification => IsFortification;

		bool ISettlementDataHolder.HasPort => HasPort;

		public SettlementRecord(string settlementId, Vec2 position, Vec2 gatePosition, XmlNode node, bool hasGate, Vec2 portPosition, bool hasPort, bool isFortification)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			SettlementId = settlementId;
			Position = position;
			GatePosition = gatePosition;
			Node = node;
			HasGate = hasGate;
			PortPosition = portPosition;
			HasPort = hasPort;
			IsFortification = isFortification;
		}
	}

	private sealed class SettlementPositionScriptNavigationCache : NavigationCache<SettlementRecord>
	{
		private readonly Scene Scene;

		private readonly List<SettlementRecord> _settlementRecords;

		private readonly int[] _excludedFaceIds;

		private readonly int _regionSwitchCostTo0;

		private readonly int _regionSwitchCostTo1;

		public SettlementPositionScriptNavigationCache(List<SettlementRecord> settlementRecords, Scene scene, MapDistanceModel mapDistanceModel, PartyNavigationModel partyNavigationModel, NavigationType navigationType)
			: base(navigationType)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Scene = scene;
			_settlementRecords = settlementRecords;
			_excludedFaceIds = partyNavigationModel.GetInvalidTerrainTypesForNavigationType(base._navigationType);
			_regionSwitchCostTo0 = mapDistanceModel.RegionSwitchCostFromLandToSea;
			_regionSwitchCostTo1 = mapDistanceModel.RegionSwitchCostFromSeaToLand;
		}

		protected override NavigationCacheElement<SettlementRecord> GetCacheElement(SettlementRecord settlement, bool isPortUsed)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return new NavigationCacheElement<SettlementRecord>(settlement, isPortUsed);
		}

		protected override SettlementRecord GetCacheElement(string settlementId)
		{
			return _settlementRecords.Single((SettlementRecord x) => x.SettlementId == settlementId);
		}

		public override void GetSceneXmlCrcValues(out uint sceneXmlCrc, out uint sceneNavigationMeshCrc)
		{
			sceneXmlCrc = Scene.GetSceneXMLCRC();
			sceneNavigationMeshCrc = Scene.GetNavigationMeshCRC();
		}

		protected override int GetNavMeshFaceCount()
		{
			return Scene.GetNavMeshFaceCount();
		}

		protected override Vec2 GetNavMeshFaceCenterPosition(int faceIndex)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Vec3 zero = Vec3.Zero;
			Scene.GetNavMeshCenterPosition(faceIndex, ref zero);
			return ((Vec3)(ref zero)).AsVec2;
		}

		protected override PathFaceRecord GetFaceRecordAtIndex(int faceIndex)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return Scene.GetNavMeshPathFaceRecord(faceIndex);
		}

		protected override int[] GetExcludedFaceIds()
		{
			return _excludedFaceIds;
		}

		protected override int GetRegionSwitchCostTo0()
		{
			return _regionSwitchCostTo0;
		}

		protected override int GetRegionSwitchCostTo1()
		{
			return _regionSwitchCostTo1;
		}

		protected override IEnumerable<SettlementRecord> GetClosestSettlementsToPositionInCache(Vec2 checkPosition, List<SettlementRecord> settlements)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Invalid comparison between Unknown and I4
			if ((int)base._navigationType == 2)
			{
				return from x in settlements
					where x.HasPort
					orderby ((Vec2)(ref checkPosition)).DistanceSquared(x.PortPosition)
					select x;
			}
			if ((int)base._navigationType == 1)
			{
				return settlements.OrderBy((SettlementRecord x) => ((Vec2)(ref checkPosition)).DistanceSquared(x.GatePosition));
			}
			return settlements.OrderBy((SettlementRecord x) => (!x.HasPort) ? ((Vec2)(ref checkPosition)).DistanceSquared(x.GatePosition) : MathF.Min(((Vec2)(ref checkPosition)).DistanceSquared(x.GatePosition), ((Vec2)(ref checkPosition)).DistanceSquared(x.PortPosition)));
		}

		protected override float GetRealPathDistanceFromPositionToSettlement(Vec2 checkPosition, PathFaceRecord currentFaceRecord, float maxDistanceToLookForPathDetection, SettlementRecord currentSettlementToLook, out bool isPort)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected I4, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			float result = float.MaxValue;
			isPort = false;
			PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
			NavigationType navigationType = base._navigationType;
			switch (navigationType - 1)
			{
			case 0:
			{
				Scene.GetNavMeshFaceIndex(ref nullFaceRecord, currentSettlementToLook.GatePosition, true, false, true);
				float num4 = default(float);
				if (Scene.GetPathDistanceBetweenAIFaces(currentFaceRecord.FaceIndex, nullFaceRecord.FaceIndex, checkPosition, currentSettlementToLook.GatePosition, 0.3f, maxDistanceToLookForPathDetection, ref num4, _excludedFaceIds, _regionSwitchCostTo0, _regionSwitchCostTo1))
				{
					result = num4;
				}
				break;
			}
			case 1:
			{
				Scene.GetNavMeshFaceIndex(ref nullFaceRecord, currentSettlementToLook.PortPosition, false, false, true);
				float num3 = default(float);
				if (Scene.GetPathDistanceBetweenAIFaces(currentFaceRecord.FaceIndex, nullFaceRecord.FaceIndex, checkPosition, currentSettlementToLook.PortPosition, 0.3f, maxDistanceToLookForPathDetection, ref num3, _excludedFaceIds, _regionSwitchCostTo0, _regionSwitchCostTo1))
				{
					result = num3;
					isPort = true;
				}
				break;
			}
			case 2:
			{
				Scene.GetNavMeshFaceIndex(ref nullFaceRecord, currentSettlementToLook.GatePosition, true, false, true);
				float num = default(float);
				if (Scene.GetPathDistanceBetweenAIFaces(currentFaceRecord.FaceIndex, nullFaceRecord.FaceIndex, checkPosition, currentSettlementToLook.GatePosition, 0.3f, maxDistanceToLookForPathDetection, ref num, _excludedFaceIds, _regionSwitchCostTo0, _regionSwitchCostTo1))
				{
					result = num;
				}
				if (currentSettlementToLook.HasPort)
				{
					Scene.GetNavMeshFaceIndex(ref nullFaceRecord, currentSettlementToLook.PortPosition, false, false, true);
					float num2 = default(float);
					if (Scene.GetPathDistanceBetweenAIFaces(currentFaceRecord.FaceIndex, nullFaceRecord.FaceIndex, checkPosition, currentSettlementToLook.PortPosition, 0.3f, maxDistanceToLookForPathDetection, ref num2, _excludedFaceIds, _regionSwitchCostTo0, _regionSwitchCostTo1) && num2 < num)
					{
						result = num2;
						isPort = true;
					}
				}
				break;
			}
			}
			return result;
		}

		protected override float GetRealDistanceAndLandRatioBetweenSettlements(NavigationCacheElement<SettlementRecord> settlement1, NavigationCacheElement<SettlementRecord> settlement2, out float landRatio)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Invalid comparison between Unknown and I4
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Invalid comparison between Unknown and I4
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Expected O, but got Unknown
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			CampaignVec2 val;
			Vec2 val2;
			if (!settlement1.IsPortUsed)
			{
				val = settlement1.GatePosition;
				val2 = ((CampaignVec2)(ref val)).ToVec2();
			}
			else
			{
				val = settlement1.PortPosition;
				val2 = ((CampaignVec2)(ref val)).ToVec2();
			}
			Vec2 val3 = val2;
			Vec2 val4;
			if (!settlement2.IsPortUsed)
			{
				val = settlement2.GatePosition;
				val4 = ((CampaignVec2)(ref val)).ToVec2();
			}
			else
			{
				val = settlement2.PortPosition;
				val4 = ((CampaignVec2)(ref val)).ToVec2();
			}
			Vec2 val5 = val4;
			PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
			Scene.GetNavMeshFaceIndex(ref nullFaceRecord, val3, !settlement1.IsPortUsed, false, true);
			PathFaceRecord nullFaceRecord2 = PathFaceRecord.NullFaceRecord;
			Scene.GetNavMeshFaceIndex(ref nullFaceRecord2, val5, !settlement2.IsPortUsed, false, true);
			landRatio = 1f;
			if ((int)base._navigationType == 2)
			{
				landRatio = 0f;
			}
			else if ((int)base._navigationType == 3)
			{
				NavigationPath val6 = new NavigationPath();
				Scene.GetPathBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, val3, val5, 0.3f, val6, _excludedFaceIds, 1f, _regionSwitchCostTo0, _regionSwitchCostTo1);
				landRatio = base.GetLandRatioOfPath(val6, val3);
			}
			float result = default(float);
			Scene.GetPathDistanceBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, val3, val5, 0.3f, float.PositiveInfinity, ref result, _excludedFaceIds, _regionSwitchCostTo0, _regionSwitchCostTo1);
			return result;
		}

		protected override void GetFaceRecordForPoint(Vec2 position, out bool isOnRegion1)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			isOnRegion1 = true;
			PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
			Scene.GetNavMeshFaceIndex(ref nullFaceRecord, position, isOnRegion1, false, true);
			if (!((PathFaceRecord)(ref nullFaceRecord)).IsValid())
			{
				isOnRegion1 = false;
				Scene.GetNavMeshFaceIndex(ref nullFaceRecord, position, isOnRegion1, false, true);
			}
			if (!((PathFaceRecord)(ref nullFaceRecord)).IsValid())
			{
				Debug.Print($"{position} has no region data.", 0, (DebugColor)3, 17592186044416uL);
			}
		}

		protected override bool CheckBeingNeighbor(List<SettlementRecord> settlementsToConsider, SettlementRecord settlement1, SettlementRecord settlement2, bool useGate1, bool useGate2, out float distance)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Expected O, but got Unknown
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0279: Unknown result type (might be due to invalid IL or missing references)
			//IL_0284: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			Vec2 val = (useGate1 ? settlement1.GatePosition : settlement1.PortPosition);
			Vec2 val2 = (useGate2 ? settlement2.GatePosition : settlement2.PortPosition);
			PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
			Scene.GetNavMeshFaceIndex(ref nullFaceRecord, val, useGate1, false, true);
			PathFaceRecord nullFaceRecord2 = PathFaceRecord.NullFaceRecord;
			Scene.GetNavMeshFaceIndex(ref nullFaceRecord2, val2, useGate2, false, true);
			if (!((PathFaceRecord)(ref nullFaceRecord)).IsValid() || !((PathFaceRecord)(ref nullFaceRecord2)).IsValid())
			{
				Debug.FailedAssert("Settlement navFace index should not be -1, check here", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SettlementPositionScript.cs", "CheckBeingNeighbor", 392);
			}
			NavigationPath val3 = new NavigationPath();
			float num = (((float)(_regionSwitchCostTo0 + _regionSwitchCostTo1) > 0f) ? 2f : 0f);
			if (num > 0f)
			{
				Scene.GetPathBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, val, val2, 0.3f, val3, _excludedFaceIds, num, _regionSwitchCostTo0, _regionSwitchCostTo1);
			}
			else
			{
				Scene.GetPathBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, val, val2, 0.3f, val3, _excludedFaceIds, 0f);
			}
			bool flag = val3.Size > 0 || nullFaceRecord.FaceIndex == nullFaceRecord2.FaceIndex;
			bool flag2 = useGate1;
			if (!Scene.GetPathDistanceBetweenAIFaces(nullFaceRecord.FaceIndex, nullFaceRecord2.FaceIndex, val, val2, 0.3f, 1784684f, ref distance, ((NavigationCache<SettlementRecord>)this).GetExcludedFaceIds(), _regionSwitchCostTo0, _regionSwitchCostTo1))
			{
				distance = 1784684f;
			}
			bool flag3 = default(bool);
			for (int i = 0; i < val3.Size && flag; i++)
			{
				Vec2 val4 = val3[i] - ((i == 0) ? val : val3[i - 1]);
				float num2 = ((Vec2)(ref val4)).Length / 1f;
				((Vec2)(ref val4)).Normalize();
				for (int j = 0; (float)j < num2; j++)
				{
					Vec2 val5 = ((i == 0) ? val : val3[i - 1]) + val4 * 1f * (float)j;
					if (!(val5 != val) || !(val5 != val2))
					{
						continue;
					}
					PathFaceRecord nullFaceRecord3 = PathFaceRecord.NullFaceRecord;
					Scene.GetNavMeshFaceIndex(ref nullFaceRecord3, val5, flag2, false, true);
					if (nullFaceRecord3.FaceIndex == -1)
					{
						flag2 = !flag2;
						Scene.GetNavMeshFaceIndex(ref nullFaceRecord3, val5, flag2, false, true);
					}
					float realPathDistanceFromPositionToSettlement = ((NavigationCache<SettlementRecord>)this).GetRealPathDistanceFromPositionToSettlement(val5, nullFaceRecord3, distance, settlement1, ref flag3);
					float realPathDistanceFromPositionToSettlement2 = ((NavigationCache<SettlementRecord>)this).GetRealPathDistanceFromPositionToSettlement(val5, nullFaceRecord3, distance, settlement2, ref flag3);
					float num3 = ((realPathDistanceFromPositionToSettlement < realPathDistanceFromPositionToSettlement2) ? realPathDistanceFromPositionToSettlement : realPathDistanceFromPositionToSettlement2);
					if (nullFaceRecord3.FaceIndex != -1)
					{
						SettlementRecord closestSettlementToPosition = base.GetClosestSettlementToPosition(val5, nullFaceRecord3, _excludedFaceIds, settlementsToConsider, _regionSwitchCostTo0, _regionSwitchCostTo1, num3 * 0.8f, ref flag3);
						if (closestSettlementToPosition != null && closestSettlementToPosition != settlement1 && closestSettlementToPosition != settlement2)
						{
							flag = false;
							break;
						}
					}
				}
			}
			return flag;
		}

		protected override List<SettlementRecord> GetAllRegisteredSettlements()
		{
			return _settlementRecords;
		}
	}

	private const string SandBoxModuleId = "Sandbox";

	private const string NavalDLCModuleId = "NavalDLC";

	private const string NavalPartyNavigationModelName = "NavalPartyNavigationModel";

	private const string NavalMapDistanceModelName = "NavalDLCMapDistanceModel";

	private bool _mapIsSandBox;

	private bool _mapIsNavalDLC;

	[EditableScriptComponentVariable(true, "")]
	private string _partyNavigationModelOverriddenClassName;

	[EditableScriptComponentVariable(true, "")]
	private string _distanceModelOverridenClassName;

	private PartyNavigationModel _partyNavigationModel;

	private MapDistanceModel _mapDistanceModel;

	public SimpleButton CheckPositions;

	public SimpleButton SavePositions;

	public SimpleButton ComputeAndSaveSettlementDistanceCache;

	private string SettlementsXmlPath
	{
		get
		{
			string text = ((ScriptComponentBehavior)this).Scene.GetModulePath();
			if (text.Contains("$BASE"))
			{
				text = text.Remove(0, 6);
				text = BasePath.Name + text;
			}
			return text + "ModuleData/settlements.xml";
		}
	}

	protected override void OnInit()
	{
		try
		{
			InitializeCachedVariables();
			bool useNavalNavigation = false;
			if (GetMapIsNavalDLC() || (!GetMapIsSandBox() && ModuleHelper.IsModuleActive("NavalDLC")))
			{
				useNavalNavigation = true;
			}
			RegisterNavigationCachesOnGameLoad(useNavalNavigation);
		}
		catch (Exception ex)
		{
			Debug.Print("Error when reading distance cache " + ex.Message, 0, (DebugColor)12, 17592186044416uL);
			Debug.Print("SettlementsDistanceCacheFilePath could not be read!. Campaign starting performance will be affected very badly, cache will be initialized now.", 0, (DebugColor)12, 17592186044416uL);
			Debug.FailedAssert("SettlementsDistanceCacheFilePath could not be read!. Campaign starting performance will be affected very badly, cache will be initialized now.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SettlementPositionScript.cs", "OnInit", 536);
		}
	}

	private void RegisterNavigationCachesOnGameLoad(bool useNavalNavigation)
	{
		SandBoxNavigationCache val = ReadNavigationCacheForNavigationTypeOnGameLoad((NavigationType)1);
		_mapDistanceModel.RegisterDistanceCache((NavigationType)1, (INavigationCache)(object)val);
		if (useNavalNavigation)
		{
			SandBoxNavigationCache val2 = ReadNavigationCacheForNavigationTypeOnGameLoad((NavigationType)2);
			SandBoxNavigationCache val3 = ReadNavigationCacheForNavigationTypeOnGameLoad((NavigationType)3);
			_mapDistanceModel.RegisterDistanceCache((NavigationType)2, (INavigationCache)(object)val2);
			_mapDistanceModel.RegisterDistanceCache((NavigationType)3, (INavigationCache)(object)val3);
		}
	}

	private SandBoxNavigationCache ReadNavigationCacheForNavigationTypeOnGameLoad(NavigationType navigationCapability)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		string text = string.Empty;
		foreach (ModuleInfo activeModule in ModuleHelper.GetActiveModules())
		{
			if (activeModule.IsActive && GetSettlementsDistanceCacheFileForCapability(activeModule.Id, navigationCapability, out var filePath))
			{
				text = filePath;
			}
		}
		SandBoxNavigationCache val;
		if (!string.IsNullOrEmpty(text))
		{
			val = ReadNavigationCacheOnGameLoad(text, navigationCapability);
		}
		else
		{
			Debug.FailedAssert($"Navigation type with id {navigationCapability} file is not found, this should not be happening, will generate cache (this will take some time)", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SettlementPositionScript.cs", "ReadNavigationCacheForNavigationTypeOnGameLoad", 576);
			val = new SandBoxNavigationCache(navigationCapability);
			((NavigationCache<Settlement>)(object)val).GenerateCacheData();
		}
		return val;
	}

	private SandBoxNavigationCache ReadNavigationCacheOnGameLoad(string path, NavigationType navigationCapability)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_000e: Expected O, but got Unknown
		SandBoxNavigationCache val = new SandBoxNavigationCache(navigationCapability);
		((NavigationCache<Settlement>)val).Deserialize(path);
		return val;
	}

	protected override void OnEditorInit()
	{
		((ScriptComponentBehavior)this).OnEditorInit();
		_partyNavigationModelOverriddenClassName = "";
		_distanceModelOverridenClassName = "";
		InitializeCachedVariables();
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		if (variableName == "SavePositions")
		{
			SaveSettlementPositions();
		}
		if (variableName == "ComputeAndSaveSettlementDistanceCache")
		{
			SaveSettlementDistanceCacheEditor();
		}
		if (variableName == "CheckPositions")
		{
			CheckSettlementPositions();
		}
		if (variableName == "_partyNavigationModelOverriddenClassName" || variableName == "_distanceModelOverridenClassName")
		{
			InitializeCachedVariables();
		}
	}

	protected override void OnSceneSave(string saveFolder)
	{
		((ScriptComponentBehavior)this).OnSceneSave(saveFolder);
		SaveSettlementPositions();
	}

	private void CheckSettlementPositions()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		XmlDocument xmlDocument = LoadXmlFile(SettlementsXmlPath);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).RemoveAllChildren();
		PartyNavigationModel partyNavigationModel = GetPartyNavigationModel();
		bool[] regionMapping = SandBoxHelpers.MapSceneHelper.GetRegionMapping(partyNavigationModel);
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).Scene.SetNavMeshRegionMap(regionMapping);
		List<int> list = partyNavigationModel.GetInvalidTerrainTypesForNavigationType((NavigationType)1).ToList();
		list.Add(0);
		List<int> list2 = null;
		foreach (XmlNode item2 in xmlDocument.DocumentElement.SelectNodes("Settlement"))
		{
			string value = item2.Attributes["id"].Value;
			GameEntity campaignEntityWithName = ((ScriptComponentBehavior)this).Scene.GetCampaignEntityWithName(value);
			if (!(campaignEntityWithName != (GameEntity)null))
			{
				continue;
			}
			Vec3 origin = campaignEntityWithName.GetGlobalFrame().origin;
			Vec3 val = default(Vec3);
			Vec3 val2 = default(Vec3);
			List<GameEntity> list3 = new List<GameEntity>();
			campaignEntityWithName.GetChildrenRecursive(ref list3);
			bool flag = false;
			bool flag2 = false;
			foreach (GameEntity item3 in list3)
			{
				if (item3.HasTag("main_map_city_gate"))
				{
					val = item3.GetGlobalFrame().origin;
					flag = true;
				}
				if (item3.HasTag("main_map_city_port"))
				{
					val2 = item3.GetGlobalFrame().origin;
					flag2 = true;
				}
			}
			Vec3 val3 = origin;
			if (flag)
			{
				val3 = val;
			}
			PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.GetNavMeshFaceIndex(ref nullFaceRecord, ((Vec3)(ref val3)).AsVec2, true, true, false);
			int item = 0;
			if (((PathFaceRecord)(ref nullFaceRecord)).IsValid())
			{
				item = nullFaceRecord.FaceGroupIndex;
			}
			if (list.Contains(item))
			{
				Debug.Print($"There is gate position problem with settlement {campaignEntityWithName.Name} at position:  {((Vec3)(ref val3)).AsVec2}", 0, (DebugColor)12, 17592186044416uL);
				MBEditor.ZoomToPosition(val3);
				break;
			}
			if (flag2)
			{
				if (list2 == null)
				{
					list2 = partyNavigationModel.GetInvalidTerrainTypesForNavigationType((NavigationType)2).ToList();
					list2.Add(0);
				}
				nullFaceRecord = PathFaceRecord.NullFaceRecord;
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).Scene.GetNavMeshFaceIndex(ref nullFaceRecord, ((Vec3)(ref val2)).AsVec2, false, true, false);
				item = 0;
				if (((PathFaceRecord)(ref nullFaceRecord)).IsValid())
				{
					item = nullFaceRecord.FaceGroupIndex;
				}
				if (list2.Contains(item))
				{
					Debug.Print($"There is port position problem with settlement {campaignEntityWithName.Name} at position:  {((Vec3)(ref val2)).AsVec2}", 0, (DebugColor)12, 17592186044416uL);
					MBEditor.ZoomToPosition(val2);
					break;
				}
			}
		}
	}

	private void InitializeCachedVariables()
	{
		_mapIsNavalDLC = string.Equals("NavalDLC", GetMapModuleId(), StringComparison.CurrentCultureIgnoreCase);
		_mapIsSandBox = string.Equals("Sandbox", GetMapModuleId(), StringComparison.CurrentCultureIgnoreCase);
		_partyNavigationModel = GetPartyNavigationModel();
		_mapDistanceModel = GetMapDistanceModel();
	}

	protected override bool IsOnlyVisual()
	{
		return true;
	}

	private bool GetMapIsNavalDLC()
	{
		return _mapIsNavalDLC;
	}

	private bool GetMapIsSandBox()
	{
		return _mapIsSandBox;
	}

	private string GetMapModuleId()
	{
		return ((ScriptComponentBehavior)this).Scene.GetModulePath().Trim().TrimEnd(new char[1] { '/' })
			.Split(new char[1] { '/' })
			.Last();
	}

	private PartyNavigationModel GetPartyNavigationModel()
	{
		if (Campaign.Current != null)
		{
			return Campaign.Current.Models.PartyNavigationModel;
		}
		if (string.IsNullOrEmpty(_partyNavigationModelOverriddenClassName))
		{
			if (GetMapIsSandBox())
			{
				_partyNavigationModelOverriddenClassName = "DefaultPartyNavigationModel";
				return CreateBaseNavigationModel(naval: false);
			}
			if (GetMapIsNavalDLC())
			{
				if (!ModuleHelper.IsModuleActive("NavalDLC"))
				{
					throw new ApplicationException("NavalDlc map changes can not be made without NavalDlc module!");
				}
				_partyNavigationModelOverriddenClassName = "NavalPartyNavigationModel";
				return CreateBaseNavigationModel(naval: true);
			}
			if (ModuleHelper.IsModuleActive("NavalDLC"))
			{
				_partyNavigationModelOverriddenClassName = "NavalPartyNavigationModel";
				return CreateBaseNavigationModel(naval: true);
			}
			_partyNavigationModelOverriddenClassName = "DefaultPartyNavigationModel";
			return CreateBaseNavigationModel(naval: false);
		}
		if (FindClass(_partyNavigationModelOverriddenClassName) == null)
		{
			Debug.FailedAssert("Cant find custom navigation model", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SettlementPositionScript.cs", "GetPartyNavigationModel", 826);
			return CreateBaseNavigationModel(GetMapIsNavalDLC());
		}
		return CreateCustomNavigationModel(_partyNavigationModelOverriddenClassName, !GetMapIsSandBox() && ModuleHelper.IsModuleActive("NavalDLC"));
	}

	private MapDistanceModel GetMapDistanceModel()
	{
		if (Campaign.Current != null)
		{
			return Campaign.Current.Models.MapDistanceModel;
		}
		if (string.IsNullOrEmpty(_distanceModelOverridenClassName))
		{
			if (GetMapIsSandBox())
			{
				_distanceModelOverridenClassName = "DefaultMapDistanceModel";
				return CreateBaseDistanceModel(naval: false);
			}
			if (GetMapIsNavalDLC())
			{
				if (!ModuleHelper.IsModuleActive("NavalDLC"))
				{
					throw new ApplicationException("NavalDlc map changes can not be made without NavalDlc module!");
				}
				_distanceModelOverridenClassName = "NavalDLCMapDistanceModel";
				return CreateBaseDistanceModel(naval: true);
			}
			if (ModuleHelper.IsModuleActive("NavalDLC"))
			{
				_distanceModelOverridenClassName = "NavalDLCMapDistanceModel";
				return CreateBaseDistanceModel(naval: true);
			}
			_distanceModelOverridenClassName = "DefaultMapDistanceModel";
			return CreateBaseDistanceModel(naval: false);
		}
		if (FindClass(_distanceModelOverridenClassName) == null)
		{
			Debug.FailedAssert("Cant find custom navigation model", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SettlementPositionScript.cs", "GetMapDistanceModel", 882);
			return CreateBaseDistanceModel(GetMapIsNavalDLC());
		}
		return CreateCustomMapDistanceModel(_distanceModelOverridenClassName, !GetMapIsSandBox() && ModuleHelper.IsModuleActive("NavalDLC"));
	}

	private static PartyNavigationModel CreateCustomNavigationModel(string name, bool naval)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		if (name == "DefaultPartyNavigationModel")
		{
			return CreateBaseNavigationModel(naval: false);
		}
		Type type = FindClass(name);
		if (type == null)
		{
			Debug.FailedAssert("Cant find custom navigation model", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SettlementPositionScript.cs", "CreateCustomNavigationModel", 903);
			return CreateBaseNavigationModel(naval);
		}
		if (!(type.GetConstructor(new Type[1] { typeof(PartyNavigationModel) }) != null))
		{
			return (PartyNavigationModel)Activator.CreateInstance(type);
		}
		return (PartyNavigationModel)Activator.CreateInstance(type, CreateBaseNavigationModel(naval));
	}

	private static MapDistanceModel CreateCustomMapDistanceModel(string name, bool naval)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		if (name == "DefaultMapDistanceModel")
		{
			return CreateBaseDistanceModel(naval: false);
		}
		Type type = FindClass(name);
		if (type == null)
		{
			Debug.FailedAssert("Cant find custom navigation model", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\SettlementPositionScript.cs", "CreateCustomMapDistanceModel", 930);
			return CreateBaseDistanceModel(naval);
		}
		return (MapDistanceModel)Activator.CreateInstance(type);
	}

	private static Type FindClass(string name)
	{
		Type result = null;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			foreach (Type item in Extensions.GetTypesSafe(assemblies[i], (Func<Type, bool>)null))
			{
				if (item.Name == name)
				{
					result = item;
					break;
				}
			}
		}
		return result;
	}

	private static PartyNavigationModel CreateBaseNavigationModel(bool naval)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		if (naval)
		{
			Type type = FindClass("NavalPartyNavigationModel");
			if (type == null)
			{
				throw new ArgumentException("Cant find naval navigation model");
			}
			return (PartyNavigationModel)Activator.CreateInstance(type, CreateBaseNavigationModel(naval: false));
		}
		return (PartyNavigationModel)new DefaultPartyNavigationModel();
	}

	private static MapDistanceModel CreateBaseDistanceModel(bool naval)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (naval)
		{
			Type type = FindClass("NavalDLCMapDistanceModel");
			if (type == null)
			{
				throw new ArgumentException("Cant find naval navigation model");
			}
			return (MapDistanceModel)Activator.CreateInstance(type);
		}
		return (MapDistanceModel)new DefaultMapDistanceModel();
	}

	private static MapDistanceModel CreateBaseDistanceModel()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		return (MapDistanceModel)new DefaultMapDistanceModel();
	}

	private unsafe bool GetSettlementsDistanceCacheFileForCapability(string moduleId, NavigationType navigationType, out string filePath)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		string text = ModuleHelper.GetModuleFullPath(moduleId) + "ModuleData/DistanceCaches";
		string text2 = ((object)(*(NavigationType*)(&navigationType))/*cast due to .constrained prefix*/).ToString();
		filePath = text + "/settlements_distance_cache_" + text2 + ".bin";
		bool num = File.Exists(filePath);
		if (num)
		{
			Debug.Print($"Found distance cache at: {moduleId}, {text}, {navigationType}", 0, (DebugColor)12, 17592186044416uL);
		}
		return num;
	}

	private List<SettlementRecord> LoadSettlementData(XmlDocument settlementDocument)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		List<SettlementRecord> list = new List<SettlementRecord>();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).RemoveAllChildren();
		foreach (XmlNode item in settlementDocument.DocumentElement.SelectNodes("Settlement"))
		{
			_ = item.Attributes["name"].Value;
			string value = item.Attributes["id"].Value;
			GameEntity campaignEntityWithName = ((ScriptComponentBehavior)this).Scene.GetCampaignEntityWithName(value);
			if (campaignEntityWithName == (GameEntity)null)
			{
				continue;
			}
			MatrixFrame globalFrame = campaignEntityWithName.GetGlobalFrame();
			Vec2 asVec = ((Vec3)(ref globalFrame.origin)).AsVec2;
			Vec2 val = default(Vec2);
			List<GameEntity> list2 = new List<GameEntity>();
			campaignEntityWithName.GetChildrenRecursive(ref list2);
			bool flag = false;
			bool hasPort = false;
			Vec2 portPosition = default(Vec2);
			foreach (GameEntity item2 in list2)
			{
				if (item2.HasTag("main_map_city_gate"))
				{
					MatrixFrame globalFrame2 = item2.GetGlobalFrame();
					val = ((Vec3)(ref globalFrame2.origin)).AsVec2;
					flag = true;
				}
				if (item2.HasTag("main_map_city_port"))
				{
					MatrixFrame globalFrame3 = item2.GetGlobalFrame();
					portPosition = ((Vec3)(ref globalFrame3.origin)).AsVec2;
					hasPort = true;
				}
				if (item2.HasTag("main_map_village_dropoff"))
				{
					MatrixFrame globalFrame4 = item2.GetGlobalFrame();
					portPosition = ((Vec3)(ref globalFrame4.origin)).AsVec2;
					hasPort = true;
				}
			}
			bool isFortification = false;
			foreach (XmlNode childNode in item.ChildNodes)
			{
				if (!childNode.Name.Equals("Components"))
				{
					continue;
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name.Equals("Town"))
					{
						if (childNode2.Attributes["is_castle"] != null)
						{
							bool.Parse(childNode2.Attributes["is_castle"].Value);
						}
						else
							_ = 0;
						isFortification = true;
						break;
					}
				}
				break;
			}
			list.Add(new SettlementRecord(value, asVec, flag ? val : asVec, item, flag, portPosition, hasPort, isFortification));
		}
		return list;
	}

	private XmlDocument LoadXmlFile(string path)
	{
		Debug.Print("opening " + path, 0, (DebugColor)12, 17592186044416uL);
		XmlDocument xmlDocument = new XmlDocument();
		StreamReader streamReader = new StreamReader(path);
		string xml = streamReader.ReadToEnd();
		xmlDocument.LoadXml(xml);
		streamReader.Close();
		return xmlDocument;
	}

	private void SaveSettlementPositions()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		XmlDocument xmlDocument = LoadXmlFile(SettlementsXmlPath);
		foreach (SettlementRecord item in LoadSettlementData(xmlDocument))
		{
			_ = item.Node.Attributes["name"].Value;
			if (item.Node.Attributes["posX"] == null)
			{
				XmlAttribute node = xmlDocument.CreateAttribute("posX");
				item.Node.Attributes.Append(node);
			}
			XmlAttribute? xmlAttribute = item.Node.Attributes["posX"];
			Vec2 val = item.Position;
			xmlAttribute.Value = ((Vec2)(ref val)).X.ToString();
			if (item.Node.Attributes["posY"] == null)
			{
				XmlAttribute node2 = xmlDocument.CreateAttribute("posY");
				item.Node.Attributes.Append(node2);
			}
			XmlAttribute? xmlAttribute2 = item.Node.Attributes["posY"];
			val = item.Position;
			xmlAttribute2.Value = ((Vec2)(ref val)).Y.ToString();
			if (item.HasGate)
			{
				if (item.Node.Attributes["gate_posX"] == null)
				{
					XmlAttribute node3 = xmlDocument.CreateAttribute("gate_posX");
					item.Node.Attributes.Append(node3);
				}
				XmlAttribute? xmlAttribute3 = item.Node.Attributes["gate_posX"];
				val = item.GatePosition;
				xmlAttribute3.Value = ((Vec2)(ref val)).X.ToString();
				if (item.Node.Attributes["gate_posY"] == null)
				{
					XmlAttribute node4 = xmlDocument.CreateAttribute("gate_posY");
					item.Node.Attributes.Append(node4);
				}
				XmlAttribute? xmlAttribute4 = item.Node.Attributes["gate_posY"];
				val = item.GatePosition;
				xmlAttribute4.Value = ((Vec2)(ref val)).Y.ToString();
			}
			if (item.HasPort)
			{
				if (item.Node.Attributes["port_posX"] == null)
				{
					XmlAttribute node5 = xmlDocument.CreateAttribute("port_posX");
					item.Node.Attributes.Append(node5);
				}
				XmlAttribute? xmlAttribute5 = item.Node.Attributes["port_posX"];
				val = item.PortPosition;
				xmlAttribute5.Value = ((Vec2)(ref val)).X.ToString();
				if (item.Node.Attributes["port_posY"] == null)
				{
					XmlAttribute node6 = xmlDocument.CreateAttribute("port_posY");
					item.Node.Attributes.Append(node6);
				}
				XmlAttribute? xmlAttribute6 = item.Node.Attributes["port_posY"];
				val = item.PortPosition;
				xmlAttribute6.Value = ((Vec2)(ref val)).Y.ToString();
			}
		}
		xmlDocument.Save(SettlementsXmlPath);
	}

	private void SaveSettlementDistanceCacheEditor()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		bool[] regionMapping = SandBoxHelpers.MapSceneHelper.GetRegionMapping(_partyNavigationModel);
		((ScriptComponentBehavior)this).Scene.SetNavMeshRegionMap(regionMapping);
		List<NavigationType> list = new List<NavigationType> { (NavigationType)1 };
		if (GetMapIsNavalDLC() || (!GetMapIsSandBox() && ModuleHelper.IsModuleActive("NavalDLC")))
		{
			list.Add((NavigationType)2);
			list.Add((NavigationType)3);
		}
		foreach (NavigationType item in list)
		{
			int[] invalidTerrainTypesForNavigationType = _partyNavigationModel.GetInvalidTerrainTypesForNavigationType(item);
			try
			{
				XmlDocument settlementDocument = LoadXmlFile(SettlementsXmlPath);
				List<SettlementRecord> settlementRecords = LoadSettlementData(settlementDocument);
				int[] array = invalidTerrainTypesForNavigationType;
				foreach (int num in array)
				{
					((ScriptComponentBehavior)this).Scene.SetAbilityOfFacesWithId(num, false);
				}
				SettlementPositionScriptNavigationCache settlementPositionScriptNavigationCache = new SettlementPositionScriptNavigationCache(settlementRecords, ((ScriptComponentBehavior)this).Scene, _mapDistanceModel, _partyNavigationModel, item);
				((NavigationCache<SettlementRecord>)settlementPositionScriptNavigationCache).GenerateCacheData();
				GetSettlementsDistanceCacheFileForCapability(GetMapModuleId(), item, out var filePath);
				((NavigationCache<SettlementRecord>)settlementPositionScriptNavigationCache).Serialize(filePath);
			}
			catch
			{
			}
			finally
			{
				int[] array = invalidTerrainTypesForNavigationType;
				foreach (int num2 in array)
				{
					((ScriptComponentBehavior)this).Scene.SetAbilityOfFacesWithId(num2, true);
				}
			}
		}
	}
}
