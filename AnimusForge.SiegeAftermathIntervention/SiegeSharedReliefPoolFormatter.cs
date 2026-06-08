using System.Collections.Generic;

namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free shared civilian relief-pool checks and context wording.
/// </summary>
public static class SiegeSharedReliefPoolFormatter
{
    public static bool HasAnyMaterial(SiegeSharedReliefPoolFacts facts)
    {
        return facts != null
            && (facts.Gold > 0
                || facts.FoodUnits > 0
                || facts.ItemTotal > 0
                || facts.ItemValue > 0);
    }

    public static string DescribeForContext(SiegeSharedReliefPoolFacts facts)
    {
        if (facts == null || (facts.Gold <= 0 && facts.FoodUnits <= 0 && facts.ItemTotal <= 0))
        {
            return "尚未通过AF给予功能交付共享物资";
        }

        var parts = new List<string>();
        if (facts.Gold > 0)
        {
            parts.Add(facts.Gold + " 第纳尔");
        }

        if (facts.FoodUnits > 0)
        {
            parts.Add(facts.FoodUnits + " 份食物");
        }

        int nonFoodItems = facts.ItemTotal - facts.FoodUnits;
        if (nonFoodItems > 0)
        {
            parts.Add(nonFoodItems + " 件非食物物资，估值 " + facts.ItemValue);
        }

        return parts.Count > 0
            ? string.Join("，", parts)
            : "尚未通过AF给予功能交付共享物资";
    }
}
