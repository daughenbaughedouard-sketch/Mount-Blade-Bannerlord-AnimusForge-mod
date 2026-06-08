namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free source codes for mission-end and native-summary aftermath transitions.
/// AF adapters still own mission lifecycle, menu switching, loot-screen timing, and encounter transitions.
/// </summary>
public static class SiegeAftermathTransitionSourceProfile
{
    public const string MissionEndFinalizeSource = "mission_end";

    public const string MissionEndFinalizedSource = "mission_end_finalized";

    public const string MissionEndNoPendingAftermathSource = "mission_end_no_pending_aftermath";

    public const string NativeDevastateSummaryContinueLootSource = "native_devastate_summary_continue_loot";

    public const string NativeDevastateSummaryContinueNoLootSource = "native_devastate_summary_continue_no_loot";
}
