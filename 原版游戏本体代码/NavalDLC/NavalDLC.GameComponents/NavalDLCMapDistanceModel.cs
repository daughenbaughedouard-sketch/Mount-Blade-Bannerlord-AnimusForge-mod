using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace NavalDLC.GameComponents;

public class NavalDLCMapDistanceModel : MapDistanceModel
{
	private Dictionary<NavigationType, INavigationCache> _navigationCaches = new Dictionary<NavigationType, INavigationCache>();

	public override int RegionSwitchCostFromLandToSea => 50;

	public override int RegionSwitchCostFromSeaToLand => 50;

	public override float MaximumSpawnDistanceForCompanionsAfterDisband => 150f;

	public override void RegisterDistanceCache(NavigationType navigationCapability, INavigationCache cacheToRegister)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_navigationCaches[navigationCapability] = cacheToRegister;
		cacheToRegister.FinalizeInitialization();
	}

	public override float GetMaximumDistanceBetweenTwoConnectedSettlements(NavigationType navigationCapabilities)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (_navigationCaches.TryGetValue(navigationCapabilities, out var value))
		{
			return value.MaximumDistanceBetweenTwoConnectedSettlements;
		}
		return 0f;
	}

	public override float GetLandRatioOfPathBetweenSettlements(Settlement fromSettlement, Settlement toSettlement, bool isFromPort, bool isTargetingPort)
	{
		if (_navigationCaches.TryGetValue((NavigationType)3, out var value))
		{
			float result = default(float);
			value.GetSettlementToSettlementDistanceWithLandRatio(fromSettlement, isFromPort, toSettlement, isTargetingPort, ref result);
			return result;
		}
		return 1f;
	}

	public override float GetDistance(Settlement fromSettlement, Settlement toSettlement, bool isFromPort = false, bool isTargetingPort = false, NavigationType navigationCapability = (NavigationType)3)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		return ((MapDistanceModel)this).GetDistance(fromSettlement, toSettlement, isFromPort, isTargetingPort, navigationCapability, ref num);
	}

	public override float GetDistance(Settlement fromSettlement, Settlement toSettlement, bool isFromPort, bool isTargetingPort, NavigationType navigationCapability, out float landRatio)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		float result = float.MaxValue;
		landRatio = -1f;
		if (fromSettlement != null && toSettlement != null)
		{
			if (fromSettlement != toSettlement)
			{
				return _navigationCaches[navigationCapability].GetSettlementToSettlementDistanceWithLandRatio(fromSettlement, isFromPort, toSettlement, isTargetingPort, ref landRatio);
			}
			result = ((isFromPort == isTargetingPort) ? 0f : ((float)(isFromPort ? ((MapDistanceModel)this).RegionSwitchCostFromSeaToLand : ((MapDistanceModel)this).RegionSwitchCostFromLandToSea)));
			landRatio = (((int)navigationCapability == 3) ? 0.5f : (((int)navigationCapability == 1) ? 1f : (((int)navigationCapability == 2) ? 0f : (-1f))));
		}
		return result;
	}

	public override float GetPortToGateDistanceForSettlement(Settlement settlement)
	{
		float num = default(float);
		return _navigationCaches[(NavigationType)3].GetSettlementToSettlementDistanceWithLandRatio(settlement, true, settlement, false, ref num);
	}

	public override float GetDistance(MobileParty fromMobileParty, Settlement toSettlement, bool isTargetingPort, NavigationType customCapability, out float estimatedLandRatio)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Invalid comparison between Unknown and I4
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Invalid comparison between Unknown and I4
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Invalid comparison between Unknown and I4
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Invalid comparison between Unknown and I4
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Invalid comparison between Unknown and I4
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Invalid comparison between Unknown and I4
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Invalid comparison between Unknown and I4
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Invalid comparison between Unknown and I4
		float num = 100000000f;
		estimatedLandRatio = -1f;
		int faceIndex = fromMobileParty.CurrentNavigationFace.FaceIndex;
		CampaignVec2 val;
		int faceIndex2;
		if (!isTargetingPort)
		{
			val = toSettlement.GatePosition;
			faceIndex2 = ((CampaignVec2)(ref val)).Face.FaceIndex;
		}
		else
		{
			val = toSettlement.PortPosition;
			faceIndex2 = ((CampaignVec2)(ref val)).Face.FaceIndex;
		}
		if (faceIndex == faceIndex2)
		{
			PartyNavigationModel partyNavigationModel = Campaign.Current.Models.PartyNavigationModel;
			IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
			val = fromMobileParty.Position;
			if (partyNavigationModel.IsTerrainTypeValidForNavigationType(mapSceneWrapper.GetFaceTerrainType(((CampaignVec2)(ref val)).Face), customCapability))
			{
				val = fromMobileParty.Position;
				num = ((CampaignVec2)(ref val)).Distance(isTargetingPort ? toSettlement.PortPosition : toSettlement.GatePosition);
				estimatedLandRatio = (((int)customCapability == 1) ? 1f : (((int)customCapability == 2) ? 0f : 0.5f));
			}
		}
		else if ((int)customCapability == 1 && (fromMobileParty.IsCurrentlyAtSea || isTargetingPort))
		{
			num = 100000000f;
		}
		else if ((int)customCapability == 2 && (!fromMobileParty.IsCurrentlyAtSea || !isTargetingPort))
		{
			num = 100000000f;
		}
		else
		{
			(Settlement, bool) closestEntranceToFace = Campaign.Current.Models.MapDistanceModel.GetClosestEntranceToFace(fromMobileParty.CurrentNavigationFace, customCapability);
			var (val2, _) = closestEntranceToFace;
			if (val2 != null)
			{
				bool item = closestEntranceToFace.Item2;
				CampaignVec2 val3 = (item ? val2.PortPosition : val2.GatePosition);
				CampaignVec2 val4 = (isTargetingPort ? toSettlement.PortPosition : toSettlement.GatePosition);
				val = fromMobileParty.Position;
				num = ((CampaignVec2)(ref val)).Distance(val4) - ((CampaignVec2)(ref val3)).Distance(val4) + Campaign.Current.Models.MapDistanceModel.GetDistance(val2, toSettlement, item, isTargetingPort, customCapability);
				if (val2 != toSettlement && (int)customCapability == 3)
				{
					bool flag = val2.HasPort && fromMobileParty.HasNavalNavigationCapability;
					bool flag2 = toSettlement.HasPort && fromMobileParty.HasNavalNavigationCapability;
					estimatedLandRatio = ((MapDistanceModel)this).GetLandRatioOfPathBetweenSettlements(val2, toSettlement, flag, flag2);
				}
				else
				{
					estimatedLandRatio = (((int)customCapability == 3) ? 0.5f : (((int)customCapability == 1) ? 1f : (((int)customCapability == 2) ? 0f : (-1f))));
				}
				if ((int)customCapability == 3)
				{
					num += Campaign.Current.Models.MapDistanceModel.GetTransitionCostAdjustment(val2, item, toSettlement, isTargetingPort, fromMobileParty.IsCurrentlyAtSea, isTargetingPort);
					if (fromMobileParty.IsCurrentlyAtSea == isTargetingPort)
					{
						float distance = Campaign.Current.Models.MapDistanceModel.GetDistance(fromMobileParty, toSettlement, isTargetingPort, (NavigationType)((!fromMobileParty.IsCurrentlyAtSea) ? 1 : 2), ref estimatedLandRatio);
						num = MathF.Min(num, distance);
					}
				}
			}
		}
		return MBMath.ClampFloat(num, 0f, float.MaxValue);
	}

	public override float GetDistance(MobileParty fromMobileParty, MobileParty toMobileParty, NavigationType customCapability, out float landRatio)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		float result = default(float);
		Campaign.Current.Models.MapDistanceModel.GetDistance(fromMobileParty, toMobileParty, customCapability, 100000000f, ref result, ref landRatio);
		return result;
	}

	public override bool GetDistance(MobileParty fromMobileParty, MobileParty toMobileParty, NavigationType customCapability, float maxDistance, out float distance, out float landRatio)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Invalid comparison between Unknown and I4
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		landRatio = (((int)customCapability == 1) ? 1f : (((int)customCapability == 2) ? 0f : (-0.5f)));
		distance = float.MaxValue;
		CampaignVec2 position;
		if (fromMobileParty.CurrentNavigationFace.FaceIndex == toMobileParty.CurrentNavigationFace.FaceIndex)
		{
			PartyNavigationModel partyNavigationModel = Campaign.Current.Models.PartyNavigationModel;
			IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
			position = fromMobileParty.Position;
			if (partyNavigationModel.IsTerrainTypeValidForNavigationType(mapSceneWrapper.GetFaceTerrainType(((CampaignVec2)(ref position)).Face), customCapability))
			{
				position = fromMobileParty.Position;
				distance = ((CampaignVec2)(ref position)).Distance(toMobileParty.Position);
			}
		}
		else if ((int)customCapability == 1 && (fromMobileParty.IsCurrentlyAtSea || toMobileParty.IsCurrentlyAtSea))
		{
			distance = float.MaxValue;
		}
		else if ((int)customCapability == 2 && (!fromMobileParty.IsCurrentlyAtSea || !toMobileParty.IsCurrentlyAtSea))
		{
			distance = float.MaxValue;
		}
		else
		{
			position = fromMobileParty.Position;
			distance = ((CampaignVec2)(ref position)).Distance(toMobileParty.Position);
		}
		distance = MBMath.ClampFloat(distance, 0f, float.MaxValue);
		return distance <= maxDistance;
	}

	public override float GetDistance(MobileParty fromMobileParty, in CampaignVec2 toPoint, NavigationType customCapability, out float landRatio)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Invalid comparison between Unknown and I4
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Invalid comparison between Unknown and I4
		float num = float.MaxValue;
		landRatio = -1f;
		CampaignVec2 val = toPoint;
		PathFaceRecord face = ((CampaignVec2)(ref val)).Face;
		if (fromMobileParty.CurrentNavigationFace.FaceIndex == face.FaceIndex)
		{
			PartyNavigationModel partyNavigationModel = Campaign.Current.Models.PartyNavigationModel;
			IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
			val = fromMobileParty.Position;
			if (partyNavigationModel.IsTerrainTypeValidForNavigationType(mapSceneWrapper.GetFaceTerrainType(((CampaignVec2)(ref val)).Face), customCapability))
			{
				val = fromMobileParty.Position;
				num = ((CampaignVec2)(ref val)).Distance(toPoint);
				landRatio = (((int)customCapability == 1) ? 1f : (((int)customCapability == 2) ? 0f : (-0.5f)));
			}
		}
		else
		{
			MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
			(Settlement, bool) closestEntranceToFace = mapDistanceModel.GetClosestEntranceToFace(fromMobileParty.CurrentNavigationFace, customCapability);
			(Settlement, bool) closestEntranceToFace2 = mapDistanceModel.GetClosestEntranceToFace(face, customCapability);
			var (val2, _) = closestEntranceToFace;
			var (val3, _) = closestEntranceToFace2;
			if (val2 != null && val3 != null)
			{
				bool flag = NavigationHelper.IsPositionValidForNavigationType(toPoint, (NavigationType)2);
				bool item = closestEntranceToFace.Item2;
				bool item2 = closestEntranceToFace2.Item2;
				CampaignVec2 val4 = (item ? val2.PortPosition : val2.GatePosition);
				CampaignVec2 val5 = (item2 ? val3.PortPosition : val3.GatePosition);
				val = fromMobileParty.Position;
				num = ((CampaignVec2)(ref val)).Distance(toPoint) - ((CampaignVec2)(ref val4)).Distance(val5) + ((MapDistanceModel)this).GetDistance(val2, val3, item, item2, customCapability);
				if ((int)customCapability == 3)
				{
					num += mapDistanceModel.GetTransitionCostAdjustment(val2, item, val3, item2, fromMobileParty.IsCurrentlyAtSea, flag);
					if (fromMobileParty.IsCurrentlyAtSea == flag)
					{
						float distance = mapDistanceModel.GetDistance(fromMobileParty, ref toPoint, (NavigationType)((!fromMobileParty.IsCurrentlyAtSea) ? 1 : 2), ref landRatio);
						num = MathF.Min(num, distance);
					}
				}
			}
		}
		return MBMath.ClampFloat(num, 0f, float.MaxValue);
	}

	public override float GetDistance(Settlement fromSettlement, in CampaignVec2 toPoint, bool isFromPort, NavigationType customCapability)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Invalid comparison between Unknown and I4
		float num = float.MaxValue;
		CampaignVec2 val = (isFromPort ? fromSettlement.PortPosition : fromSettlement.GatePosition);
		CampaignVec2 val2 = toPoint;
		PathFaceRecord face = ((CampaignVec2)(ref val2)).Face;
		PathFaceRecord face2 = ((CampaignVec2)(ref val)).Face;
		if (face2.FaceIndex == face.FaceIndex)
		{
			if (Campaign.Current.Models.PartyNavigationModel.IsTerrainTypeValidForNavigationType(Campaign.Current.MapSceneWrapper.GetFaceTerrainType(face2), customCapability))
			{
				num = ((CampaignVec2)(ref val)).Distance(toPoint);
			}
		}
		else
		{
			MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
			(Settlement, bool) closestEntranceToFace = mapDistanceModel.GetClosestEntranceToFace(face, customCapability);
			var (val3, _) = closestEntranceToFace;
			if (val3 != null)
			{
				bool flag = NavigationHelper.IsPositionValidForNavigationType(toPoint, (NavigationType)2);
				bool item = closestEntranceToFace.Item2;
				CampaignVec2 val4 = (isFromPort ? fromSettlement.PortPosition : fromSettlement.GatePosition);
				CampaignVec2 val5 = (item ? val3.PortPosition : val3.GatePosition);
				num = ((CampaignVec2)(ref val4)).Distance(toPoint) - ((CampaignVec2)(ref val4)).Distance(val5) + mapDistanceModel.GetDistance(fromSettlement, val3, isFromPort, item, customCapability);
				if ((int)customCapability == 3)
				{
					num += mapDistanceModel.GetTransitionCostAdjustment(fromSettlement, isFromPort, val3, item, isFromPort, flag);
					if (isFromPort == flag)
					{
						float distance = mapDistanceModel.GetDistance(fromSettlement, ref toPoint, isFromPort, (NavigationType)((!isFromPort) ? 1 : 2));
						num = MathF.Min(num, distance);
					}
				}
			}
		}
		return MBMath.ClampFloat(num, 0f, 100000000f);
	}

	public override bool PathExistBetweenPoints(in CampaignVec2 fromPoint, in CampaignVec2 toPoint, NavigationType navigationType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
		CampaignVec2 val = fromPoint;
		(Settlement, bool) closestEntranceToFace = mapDistanceModel.GetClosestEntranceToFace(((CampaignVec2)(ref val)).Face, navigationType);
		MapDistanceModel mapDistanceModel2 = Campaign.Current.Models.MapDistanceModel;
		val = toPoint;
		(Settlement, bool) closestEntranceToFace2 = mapDistanceModel2.GetClosestEntranceToFace(((CampaignVec2)(ref val)).Face, navigationType);
		if (closestEntranceToFace.Item1 != null && closestEntranceToFace2.Item1 != null)
		{
			return Campaign.Current.Models.MapDistanceModel.GetDistance(closestEntranceToFace.Item1, closestEntranceToFace2.Item1, closestEntranceToFace.Item2, closestEntranceToFace2.Item2, navigationType) < Campaign.MapDiagonal * 10f;
		}
		return false;
	}

	public override (Settlement, bool) GetClosestEntranceToFace(PathFaceRecord face, NavigationType navigationCapabilities)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		bool item = default(bool);
		return (_navigationCaches[navigationCapabilities].GetClosestSettlementToFaceIndex(face.FaceIndex, ref item), item);
	}

	public override MBReadOnlyList<Settlement> GetNeighborsOfFortification(Town town, NavigationType navigationCapabilities)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!_navigationCaches.TryGetValue(navigationCapabilities, out var value))
		{
			Debug.FailedAssert("cache not found", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\GameComponents\\NavalDLCMapDistanceModel.cs", "GetNeighborsOfFortification", 376);
			return new MBReadOnlyList<Settlement>();
		}
		return value.GetNeighbors(((SettlementComponent)town).Settlement);
	}

	public override float GetTransitionCostAdjustment(Settlement settlement1, bool isFromPort, Settlement settlement2, bool isTargetingPort, bool fromIsCurrentlyAtSea, bool toIsCurrentlyAtSea)
	{
		float num = 0f;
		if (isFromPort != fromIsCurrentlyAtSea)
		{
			num -= (float)(isFromPort ? ((MapDistanceModel)this).RegionSwitchCostFromSeaToLand : ((MapDistanceModel)this).RegionSwitchCostFromLandToSea);
		}
		if (isTargetingPort != toIsCurrentlyAtSea)
		{
			num -= (float)(isTargetingPort ? ((MapDistanceModel)this).RegionSwitchCostFromLandToSea : ((MapDistanceModel)this).RegionSwitchCostFromSeaToLand);
		}
		if (isFromPort == isTargetingPort && Campaign.Current.Models.MapDistanceModel.GetDistance(settlement1, settlement2, isFromPort, isTargetingPort, (NavigationType)((!isFromPort) ? 1 : 2)) < Campaign.MapDiagonalSquared)
		{
			num *= -1f;
		}
		return num;
	}
}
