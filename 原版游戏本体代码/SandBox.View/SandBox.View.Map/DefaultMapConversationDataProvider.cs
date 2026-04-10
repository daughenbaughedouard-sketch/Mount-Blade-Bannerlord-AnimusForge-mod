using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace SandBox.View.Map;

public class DefaultMapConversationDataProvider : IMapConversationDataProvider
{
	string IMapConversationDataProvider.GetAtmosphereNameFromData(MapConversationTableauData data)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected I4, but got Unknown
		string text = ((data.TimeOfDay <= 3f || data.TimeOfDay >= 21f) ? "night" : ((!(data.TimeOfDay > 8f) || !(data.TimeOfDay < 16f)) ? "sunset" : "noon"));
		if (data.Settlement == null || data.Settlement.IsHideout)
		{
			if (data.IsCurrentTerrainUnderSnow)
			{
				return "conv_snow_" + text + "_0";
			}
			TerrainType conversationTerrainType = data.ConversationTerrainType;
			return (conversationTerrainType - 1) switch
			{
				1 => "conv_desert_" + text + "_0", 
				4 => "conv_steppe_" + text + "_0", 
				3 => "conv_forest_" + text + "_0", 
				_ => "conv_plains_" + text + "_0", 
			};
		}
		string stringId = ((MBObjectBase)data.Settlement.Culture).StringId;
		bool isLocationInside;
		string locationNameFromLocationId = GetLocationNameFromLocationId(data.LocationId, out isLocationInside);
		if (locationNameFromLocationId != null)
		{
			if (isLocationInside)
			{
				return "conv_" + stringId + "_" + locationNameFromLocationId + "_0";
			}
			return "conv_" + stringId + "_" + locationNameFromLocationId + "_" + text + "_0";
		}
		return "conv_" + stringId + "_town_" + text + "_0";
	}

	private static string GetLocationNameFromLocationId(string locationId, out bool isLocationInside)
	{
		switch (locationId)
		{
		case "tavern":
			isLocationInside = true;
			return "tavern";
		case "lordshall":
			isLocationInside = true;
			return "lordshall";
		case "port":
			isLocationInside = false;
			return "shipyard";
		default:
			isLocationInside = false;
			return null;
		}
	}
}
