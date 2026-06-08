namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free UI wording for final GCCZ completion and encounter-exit notifications.
/// AF adapters still own live aftermath labels, loot totals, and Bannerlord display side effects.
/// </summary>
public static class SiegeInterventionCompletionUiProfile
{
    public const uint CompletionMessageColor = 0xFFB6F7A8u;

    public const string MassacreVictoryQuickText = "血洗完成，离场后结算战利品。";

    public const string LeaveEncounterQuickText = "攻城后处置已完成，正在离开攻城遭遇。";

    public static string BuildCompletedEncounterMessage(string actionLabel)
    {
        return "【攻城处置】攻城后" + NormalizeActionLabel(actionLabel) + "已经结算完成，正在结束攻城遭遇。";
    }

    public static string BuildLootSettlementSummaryMessage(int marketItemTotal, int marketStackKinds, int marketGold, int civilianGold)
    {
        return "【战利清点】结算：市场物资 " + ClampNonNegative(marketItemTotal) + " 件 / " + ClampNonNegative(marketStackKinds) + " 类，市场金库 " + ClampNonNegative(marketGold) + "，民众第纳尔 " + ClampNonNegative(civilianGold) + "。";
    }

    private static string NormalizeActionLabel(string actionLabel)
    {
        return string.IsNullOrWhiteSpace(actionLabel) ? "处置" : actionLabel.Trim();
    }

    private static int ClampNonNegative(int value)
    {
        return value < 0 ? 0 : value;
    }
}
