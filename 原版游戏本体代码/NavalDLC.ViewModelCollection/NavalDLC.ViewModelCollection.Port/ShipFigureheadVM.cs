using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.ViewModelCollection.Port;

public class ShipFigureheadVM : ShipUpgradePieceBaseVM
{
	public Ship EquippedShip;

	public readonly Figurehead Figurehead;

	private readonly IViewDataTracker _viewDataTracker = Campaign.Current.GetCampaignBehavior<IViewDataTracker>();

	public ShipFigureheadVM(Figurehead figurehead, Action<ShipUpgradePieceBaseVM> onSelected)
		: base(onSelected)
	{
		Figurehead = figurehead;
		base.Price = 0;
		base.UpgradePieceTier = ShipUpgradePieceTier.Diamond;
		base.Identifier = ((MBObjectBase)figurehead).StringId;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		base.RefreshValues();
		base.Name = ((object)((PropertyObject)Figurehead).Name).ToString();
	}

	protected override PropertyBasedTooltipVM GetProperties()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		object[] array = new object[1] { Figurehead };
		PropertyBasedTooltipVM val = new PropertyBasedTooltipVM(typeof(Figurehead), array);
		if (base.IsHiddenFromPlayer)
		{
			((Collection<TooltipProperty>)(object)val.TooltipPropertyList).Clear();
			val.AddProperty(((object)new TextObject("{=4RUs8Cfu}Not Unlocked", (Dictionary<string, object>)null)).ToString(), string.Empty, 0, (TooltipPropertyFlags)0);
			return val;
		}
		if (!base.IsInspectedFromSlot)
		{
			val.AddProperty(" ", " ", 0, (TooltipPropertyFlags)0);
			if (base.IsSelected)
			{
				val.AddProperty(((object)new TextObject("{=OSoAVlqc}Equipped", (Dictionary<string, object>)null)).ToString(), string.Empty, 0, (TooltipPropertyFlags)0);
			}
			else if (EquippedShip != null)
			{
				val.AddProperty(((object)new TextObject("{=bQzObjHj}Attached Ship", (Dictionary<string, object>)null)).ToString(), ((object)EquippedShip.Name).ToString(), 0, (TooltipPropertyFlags)0);
			}
			else if (base.IsDisabled)
			{
				val.AddProperty(((object)new TextObject("{=4RUs8Cfu}Not Unlocked", (Dictionary<string, object>)null)).ToString(), string.Empty, 0, (TooltipPropertyFlags)0);
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

	public override void InspectPiece(bool isInspectedFromSlot = false, TextObject slotHintText = null)
	{
		base.InspectPiece(isInspectedFromSlot, slotHintText);
		if (base.IsUnexamined)
		{
			_viewDataTracker.OnFigureheadExamined(Figurehead);
		}
	}

	public override void Update()
	{
		base.Update();
		base.IsUnexamined = !base.IsDisabled && _viewDataTracker.UnexaminedFigureheads.Contains(Figurehead);
	}
}
