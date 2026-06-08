using System.Collections.Generic;

namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free formatter for GCCZ per-scene memory context.
/// AF adapters still own event collection, de-duplication, and log side effects.
/// </summary>
public static class SiegeInterventionMemoryContextBuilder
{
    public static string Build(IReadOnlyList<string> memoryEvents)
    {
        if (memoryEvents == null || memoryEvents.Count == 0)
        {
            return string.Empty;
        }

        return "【攻城处置记忆】" + string.Join("；", memoryEvents)
            + "。这些是本次入城处置内已经发生的事实，后续NPC必须承认大概情况，不能表现得像玩家没有下过这些命令或民众没有被聚集过。";
    }
}
