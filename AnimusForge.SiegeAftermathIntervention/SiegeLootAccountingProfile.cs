namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free UI wording for GCCZ loot accounting.
/// AF adapters still own Bannerlord gold/item mutation, target selection, and display side effects.
/// </summary>
public static class SiegeLootAccountingProfile
{
    public const uint LootMessageColor = 0xFFFFC46Bu;

    public static string BuildCivilianExitSettlementMessage(int recordedCivilianCount, int lootedCivilianCount, int gainedGold)
    {
        return "【战利清点】本次入城处置共记录 " + ClampNonNegative(recordedCivilianCount)
            + " 名普通民众；离场时结算剩余 " + ClampNonNegative(lootedCivilianCount)
            + " 名，共新增 " + ClampNonNegative(gainedGold)
            + " 第纳尔。";
    }

    public static string BuildCivilianLootMessage(string actorName, string targetName, int amount)
    {
        return "【战利清点】" + BuildCivilianLootLine(actorName, targetName, amount);
    }

    public static string BuildCivilianLootLine(string actorName, string targetName, int amount)
    {
        string safeTargetName = string.IsNullOrWhiteSpace(targetName) ? "目标" : targetName.Trim();
        int safeAmount = ClampNonNegative(amount);
        if (string.IsNullOrWhiteSpace(actorName))
        {
            return "从 " + safeTargetName + " 取得 " + safeAmount + " 第纳尔。";
        }

        return actorName.Trim() + " 盘问 " + safeTargetName + " 后取得 " + safeAmount + " 第纳尔。";
    }

    private static int ClampNonNegative(int value)
    {
        return value < 0 ? 0 : value;
    }
}
