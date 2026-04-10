using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.ViewModelCollection.Port;

public class ShipUpgradePieceVM : ShipUpgradePieceBaseVM
{
	public readonly ShipUpgradePiece Piece;

	public readonly Ship Ship;

	public static event Func<Ship, ShipUpgradePiece, int> GetUpgradePrice;

	public ShipUpgradePieceVM(ShipUpgradePiece piece, Ship ship, Action<ShipUpgradePieceBaseVM> onSelected)
		: base(onSelected)
	{
		Piece = piece;
		Ship = ship;
		base.UpgradePieceTier = (ShipUpgradePieceTier)MathF.Clamp((float)Piece.RequiredPortLevel, 1f, 4f);
		base.Identifier = ((MBObjectBase)piece).StringId;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		base.Name = ((object)((MBObjectBase)Piece).GetName()).ToString();
		base.Price = ShipUpgradePieceVM.GetUpgradePrice?.Invoke(Ship, Piece) ?? 0;
	}

	protected override PropertyBasedTooltipVM GetProperties()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		object[] array = new object[1] { Piece };
		PropertyBasedTooltipVM val = new PropertyBasedTooltipVM(typeof(ShipUpgradePiece), array);
		if (!TextObject.IsNullOrEmpty(Piece.Description))
		{
			TooltipProperty item = new TooltipProperty(((object)Piece.Description).ToString(), string.Empty, 0, false, (TooltipPropertyFlags)0);
			((Collection<TooltipProperty>)(object)val.TooltipPropertyList).Insert(0, item);
			item = new TooltipProperty(" ", " ", 0, false, (TooltipPropertyFlags)0);
			((Collection<TooltipProperty>)(object)val.TooltipPropertyList).Insert(1, item);
		}
		if (!base.IsInspectedFromSlot)
		{
			val.AddProperty(" ", " ", 0, (TooltipPropertyFlags)0);
			if (base.IsSelected)
			{
				val.AddProperty(((object)new TextObject("{=OSoAVlqc}Equipped", (Dictionary<string, object>)null)).ToString(), string.Empty, 0, (TooltipPropertyFlags)0);
			}
			else if (base.IsDisabled)
			{
				val.AddProperty(((object)new TextObject("{=DovqkMg1}Not Available In Settlement", (Dictionary<string, object>)null)).ToString(), string.Empty, 0, (TooltipPropertyFlags)0);
			}
			else if (base.Price > 0)
			{
				val.AddProperty(((object)new TextObject("{=ebUrBmHK}Price", (Dictionary<string, object>)null)).ToString(), base.Price.ToString(), 0, (TooltipPropertyFlags)0);
			}
			else
			{
				val.AddProperty(((object)new TextObject("{=Ve1E1wXz}Unlocked", (Dictionary<string, object>)null)).ToString(), string.Empty, 0, (TooltipPropertyFlags)0);
			}
		}
		else if (!TextObject.IsNullOrEmpty(_slotHintText))
		{
			val.AddProperty(" ", " ", 0, (TooltipPropertyFlags)0);
			val.AddProperty(((object)_slotHintText).ToString(), string.Empty, 0, (TooltipPropertyFlags)0);
		}
		return val;
	}
}
