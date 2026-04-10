using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.GameComponents;

public class NavalPartyNavigationModel : PartyNavigationModel
{
	private readonly Dictionary<NavigationType, int[]> _invalidTypesIntegerCache = new Dictionary<NavigationType, int[]>();

	private readonly PartyNavigationModel _baseModel;

	public override float GetEmbarkDisembarkThresholdDistance()
	{
		return 0.5f;
	}

	private static bool IsTerrainTypeValidForNaval(TerrainType t)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		if ((int)t != 8 && (int)t != 10 && (int)t != 11 && (int)t != 18 && (int)t != 19 && (int)t != 23 && (int)t != 24)
		{
			return (int)t == 25;
		}
		return true;
	}

	public NavalPartyNavigationModel(PartyNavigationModel partyNavigationModel)
	{
		_baseModel = partyNavigationModel;
		InitializeInvalidTypesCache();
	}

	private void InitializeInvalidTypesCache()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected I4, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected I4, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected I4, but got Unknown
		_invalidTypesIntegerCache.Clear();
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		foreach (TerrainType value in Enum.GetValues(typeof(TerrainType)))
		{
			if (!((PartyNavigationModel)this).IsTerrainTypeValidForNavigationType(value, (NavigationType)3))
			{
				list.Add((int)value);
			}
			if (!((PartyNavigationModel)this).IsTerrainTypeValidForNavigationType(value, (NavigationType)2))
			{
				list3.Add((int)value);
			}
			if (!((PartyNavigationModel)this).IsTerrainTypeValidForNavigationType(value, (NavigationType)1))
			{
				list2.Add((int)value);
			}
		}
		_invalidTypesIntegerCache.Add((NavigationType)3, list.ToArray());
		_invalidTypesIntegerCache.Add((NavigationType)1, list2.ToArray());
		_invalidTypesIntegerCache.Add((NavigationType)2, list3.ToArray());
	}

	public override bool IsTerrainTypeValidForNavigationType(TerrainType terrainType, NavigationType navigationType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if ((int)navigationType == 2)
		{
			return IsTerrainTypeValidForNaval(terrainType);
		}
		if ((int)navigationType == 3)
		{
			if (!IsTerrainTypeValidForNaval(terrainType))
			{
				return _baseModel.IsTerrainTypeValidForNavigationType(terrainType, navigationType);
			}
			return true;
		}
		return _baseModel.IsTerrainTypeValidForNavigationType(terrainType, navigationType);
	}

	public override int[] GetInvalidTerrainTypesForNavigationType(NavigationType navigationType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (_invalidTypesIntegerCache.ContainsKey(navigationType))
		{
			return _invalidTypesIntegerCache[navigationType];
		}
		return new int[0];
	}

	public override bool HasNavalNavigationCapability(MobileParty mobileParty)
	{
		if (((List<Ship>)(object)mobileParty.Ships).Count <= 0)
		{
			if (!mobileParty.IsMainParty)
			{
				if (mobileParty.AttachedTo == null || !mobileParty.AttachedTo.HasNavalNavigationCapability)
				{
					if (((List<MobileParty>)(object)mobileParty.AttachedParties).Count > 0)
					{
						return ((IEnumerable<MobileParty>)mobileParty.AttachedParties).Any((MobileParty x) => ((List<Ship>)(object)x.Ships).Count > 0);
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public override bool CanPlayerNavigateToPosition(CampaignVec2 vec2, out NavigationType navigationType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected I4, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		navigationType = (NavigationType)0;
		PathFaceRecord face = ((CampaignVec2)(ref vec2)).Face;
		if (!((PathFaceRecord)(ref face)).IsValid())
		{
			return false;
		}
		if (!MobileParty.MainParty.IsCurrentlyAtSea && NavigationHelper.IsPositionValidForNavigationType(vec2, (NavigationType)2))
		{
			return false;
		}
		if (MobileParty.MainParty.IsCurrentlyAtSea)
		{
			if (MobileParty.MainParty.HasNavalNavigationCapability && NavigationHelper.IsPositionValidForNavigationType(vec2, (NavigationType)2))
			{
				navigationType = (NavigationType)2;
			}
			else
			{
				navigationType = (NavigationType)(int)MobileParty.MainParty.NavigationCapability;
			}
		}
		else
		{
			navigationType = (NavigationType)1;
		}
		int[] invalidTerrainTypesForNavigationType = Campaign.Current.Models.PartyNavigationModel.GetInvalidTerrainTypesForNavigationType(navigationType);
		if (invalidTerrainTypesForNavigationType.Contains(((CampaignVec2)(ref vec2)).Face.FaceGroupIndex))
		{
			return false;
		}
		if (!vec2.IsOnLand && MobileParty.MainParty.IsCurrentlyAtSea)
		{
			return true;
		}
		IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
		PathFaceRecord currentNavigationFace = MobileParty.MainParty.CurrentNavigationFace;
		PathFaceRecord face2 = ((CampaignVec2)(ref vec2)).Face;
		CampaignVec2 position = MobileParty.MainParty.Position;
		float num = default(float);
		return mapSceneWrapper.GetPathDistanceBetweenAIFaces(currentNavigationFace, face2, ((CampaignVec2)(ref position)).ToVec2(), ((CampaignVec2)(ref vec2)).ToVec2(), 0.3f, (float)Campaign.PathFindingMaxCostLimit, ref num, invalidTerrainTypesForNavigationType, MobileParty.MainParty.GetRegionSwitchCostFromLandToSea(), MobileParty.MainParty.GetRegionSwitchCostFromSeaToLand());
	}
}
