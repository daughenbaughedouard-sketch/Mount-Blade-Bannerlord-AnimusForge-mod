using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.GameComponents;

public class NavalDLCShipStatModel : ShipStatModel
{
	public override float GetShipFlagshipScore(Ship ship)
	{
		return GetShipTierf(ship) * MathF.Max(0.1f, ship.HitPoints / ship.MaxHitPoints);
	}

	private float GetShipTierf(Ship ship)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		int num = ship.ShipHull.Value;
		foreach (KeyValuePair<string, ShipSlot> availableSlot in ship.ShipHull.AvailableSlots)
		{
			ShipUpgradePiece pieceAtSlot = ship.GetPieceAtSlot(availableSlot.Key);
			if (pieceAtSlot != null)
			{
				num = (((int)ship.ShipHull.Type != 0) ? (((int)ship.ShipHull.Type != 1) ? (num + pieceAtSlot.HeavyValue) : (num + pieceAtSlot.MediumValue)) : (num + pieceAtSlot.LightValue));
			}
		}
		if (ship.Figurehead != null)
		{
			num += 15000;
		}
		return num;
	}
}
