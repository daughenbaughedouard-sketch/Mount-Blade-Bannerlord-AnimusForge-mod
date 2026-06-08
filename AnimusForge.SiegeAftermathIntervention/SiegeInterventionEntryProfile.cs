namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free profile for GCCZ scene-entry UI wording.
/// AF adapters still resolve Bannerlord settlements, locations, menu args, and display side effects.
/// </summary>
public static class SiegeInterventionEntryProfile
{
    public const uint MissingSceneMessageColor = 0xFFFF7777u;

    public const string EnabledTooltip = "{=!}暂不立即处置战后事务；你将披甲带约50名健康士兵进城，普通民众仍散在城内街区，再由现场对话或行动决定安抚、宽恕、搜掠或血洗。";

    public const string MissingSceneTooltip = "{=!}当前没有可进入的攻城胜利定居点场景。";

    public const string MissingSceneMessage = "【攻城处置】当前没有可进入的被攻陷定居点场景。";
}
