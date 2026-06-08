using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Low-coupling extraction of the current fused action-tag vocabulary.
/// This intentionally preserves the existing Chinese canonical tags used by the runtime postprocessor.
/// </summary>
public static class SiegeActionTagCatalog
{
    private static readonly Regex ActionTagRegex = new Regex(
        @"\[ACTION:(?<name>SIEGE_[A-Z_]+|宽恕|救济|宣抚|盟誓|安兵|召集|搜掠|血洗|殖民)(?::\d+)?\]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly IReadOnlyDictionary<string, SiegeInterventionActionKind> NameToKind =
        new Dictionary<string, SiegeInterventionActionKind>(StringComparer.OrdinalIgnoreCase)
        {
            ["SIEGE_MERCY"] = SiegeInterventionActionKind.Mercy,
            ["宽恕"] = SiegeInterventionActionKind.Mercy,
            ["SIEGE_RELIEF"] = SiegeInterventionActionKind.Relief,
            ["救济"] = SiegeInterventionActionKind.Relief,
            ["SIEGE_INSPIRE"] = SiegeInterventionActionKind.Inspire,
            ["宣抚"] = SiegeInterventionActionKind.Inspire,
            ["SIEGE_RALLY_OATH"] = SiegeInterventionActionKind.RallyOath,
            ["盟誓"] = SiegeInterventionActionKind.RallyOath,
            ["SIEGE_APPEASE_SOLDIERS"] = SiegeInterventionActionKind.AppeaseSoldiers,
            ["安兵"] = SiegeInterventionActionKind.AppeaseSoldiers,
            ["SIEGE_GATHER_CIVILIANS"] = SiegeInterventionActionKind.GatherCivilians,
            ["召集"] = SiegeInterventionActionKind.GatherCivilians,
            ["SIEGE_PLUNDER"] = SiegeInterventionActionKind.Plunder,
            ["搜掠"] = SiegeInterventionActionKind.Plunder,
            ["SIEGE_MASSACRE"] = SiegeInterventionActionKind.Massacre,
            ["血洗"] = SiegeInterventionActionKind.Massacre,
            ["SIEGE_CULTURAL_REPOPULATION"] = SiegeInterventionActionKind.CulturalRepopulation,
            ["SIEGE_PURGE_REPOPULATION"] = SiegeInterventionActionKind.CulturalRepopulation,
            ["殖民"] = SiegeInterventionActionKind.CulturalRepopulation,
        };

    private static readonly IReadOnlyDictionary<SiegeInterventionActionKind, string> KindToCanonicalTag =
        new Dictionary<SiegeInterventionActionKind, string>
        {
            [SiegeInterventionActionKind.Mercy] = "[ACTION:宽恕]",
            [SiegeInterventionActionKind.Relief] = "[ACTION:救济]",
            [SiegeInterventionActionKind.Inspire] = "[ACTION:宣抚]",
            [SiegeInterventionActionKind.RallyOath] = "[ACTION:盟誓]",
            [SiegeInterventionActionKind.AppeaseSoldiers] = "[ACTION:安兵]",
            [SiegeInterventionActionKind.GatherCivilians] = "[ACTION:召集]",
            [SiegeInterventionActionKind.Plunder] = "[ACTION:搜掠]",
            [SiegeInterventionActionKind.Massacre] = "[ACTION:血洗]",
            [SiegeInterventionActionKind.CulturalRepopulation] = "[ACTION:殖民]",
        };

    public static bool TryParseName(string tagName, out SiegeInterventionActionKind kind)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            kind = SiegeInterventionActionKind.Unknown;
            return false;
        }

        return NameToKind.TryGetValue(tagName.Trim(), out kind);
    }

    public static bool TryGetCanonicalTag(SiegeInterventionActionKind kind, out string canonicalTag)
    {
        return KindToCanonicalTag.TryGetValue(kind, out canonicalTag);
    }

    public static IReadOnlyList<SiegeInterventionActionKind> ExtractKinds(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Array.Empty<SiegeInterventionActionKind>();
        }

        var result = new List<SiegeInterventionActionKind>();
        var seen = new HashSet<SiegeInterventionActionKind>();
        foreach (Match match in ActionTagRegex.Matches(text))
        {
            var name = match.Groups["name"].Value;
            if (TryParseName(name, out var kind) && seen.Add(kind))
            {
                result.Add(kind);
            }
        }

        return result;
    }

    public static IReadOnlyList<string> NormalizeToCanonicalTags(string text)
    {
        var kinds = ExtractKinds(text);
        if (kinds.Count == 0)
        {
            return Array.Empty<string>();
        }

        var result = new List<string>(kinds.Count);
        foreach (var kind in kinds)
        {
            if (TryGetCanonicalTag(kind, out var tag))
            {
                result.Add(tag);
            }
        }

        return result;
    }
}
