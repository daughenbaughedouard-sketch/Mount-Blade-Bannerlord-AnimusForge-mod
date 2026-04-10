using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.View;

public class NavalDLCViewCheats
{
	[CommandLineArgumentFunction("focus_player_anchor", "naval")]
	public static string FocusPlayerAnchor(List<string> strings)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		string message = string.Empty;
		if (!NavalDLCCheats.CheckCheatUsage(ref message))
		{
			return message;
		}
		if (CampaignCheats.CheckHelp(strings))
		{
			return "Format is \"naval.focus_player_anchor\".";
		}
		if (!MobileParty.MainParty.Anchor.IsValid)
		{
			return "Anchor is not valid";
		}
		MapScreen.Instance.FastMoveCameraToPosition(MobileParty.MainParty.Anchor.Position);
		return "Success";
	}

	[CommandLineArgumentFunction("focus_ship", "naval")]
	public static string FocusShip(List<string> strings)
	{
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		string text = "Format is \"naval.focus_ship [ShipHullStringId/ShipHullName]\".";
		if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckHelp(strings))
		{
			return text;
		}
		string text2 = CampaignCheats.ConcatenateString(strings);
		ShipHull val = MBObjectManager.Instance.GetObject<ShipHull>(text2);
		if (val == null)
		{
			foreach (ShipHull item in (List<ShipHull>)(object)MBObjectManager.Instance.GetObjectTypeList<ShipHull>())
			{
				if (string.Equals(((object)item.Name).ToString().ToLower(), text2.ToLower(), StringComparison.OrdinalIgnoreCase))
				{
					val = item;
					break;
				}
			}
		}
		if (val != null)
		{
			string shipHullStringId = ((MBObjectBase)val).StringId;
			Town val2 = ((IEnumerable<Town>)Town.AllTowns).FirstOrDefault((Func<Town, bool>)((Town x) => ((List<Ship>)(object)x.AvailableShips).Exists((Predicate<Ship>)((Ship y) => ((MBObjectBase)y.ShipHull).StringId == shipHullStringId))));
			if (val2 != null)
			{
				((IEnumerable<Ship>)val2.AvailableShips).First((Ship x) => ((MBObjectBase)x.ShipHull).StringId == shipHullStringId);
				MapScreen.Instance.MapCameraView.SetCameraMode((CameraFollowMode)1);
				((SettlementComponent)val2).Settlement.Party.SetAsCameraFollowParty();
				return "Success! Found in " + ((object)((SettlementComponent)val2).Name).ToString();
			}
		}
		return "Ship is not found : " + text2 + "\n" + text;
	}
}
