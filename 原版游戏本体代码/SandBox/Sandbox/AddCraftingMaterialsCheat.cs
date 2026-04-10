using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace SandBox;

public class AddCraftingMaterialsCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		for (CraftingMaterials val = (CraftingMaterials)0; (int)val < 9; val = (CraftingMaterials)(val + 1))
		{
			ItemObject craftingMaterialItem = Campaign.Current.Models.SmithingModel.GetCraftingMaterialItem(val);
			PartyBase.MainParty.ItemRoster.AddToCounts(craftingMaterialItem, 10);
		}
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=63jJ3GGY}Add 10 Crafting Materials Each", (Dictionary<string, object>)null);
	}
}
