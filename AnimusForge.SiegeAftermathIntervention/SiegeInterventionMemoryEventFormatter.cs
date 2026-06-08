using System.Text.RegularExpressions;

namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free formatter for a single GCCZ memory event.
/// AF adapters still own sequencing, duplicate checks, max event count, and logging.
/// </summary>
public static class SiegeInterventionMemoryEventFormatter
{
    private static readonly Regex AnySiegeTagRegex = new Regex(
        "\\[ACTION:(?:SIEGE_[A-Z_]+|宽恕|救济|宣抚|盟誓|安兵|召集|搜掠|血洗|殖民)(?::\\d+)?\\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex WhitespaceRegex = new Regex("\\s+", RegexOptions.Compiled);

    public static string FormatEntry(string kind, string detail)
    {
        string normalizedKind = string.IsNullOrWhiteSpace(kind) ? "处置" : kind.Trim();
        string normalizedDetail = string.IsNullOrWhiteSpace(detail) ? normalizedKind : detail.Trim();
        normalizedDetail = AnySiegeTagRegex.Replace(normalizedDetail, string.Empty);
        normalizedDetail = WhitespaceRegex.Replace(normalizedDetail.Replace("\r", " ").Replace("\n", " "), " ").Trim();
        return normalizedKind + "：" + normalizedDetail;
    }
}
