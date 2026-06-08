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

    public const string CampaignTickPostMissionFinishSource = "campaign_tick_post_mission";

    public const string DoneMenuContinueFinishSource = "af_done_menu_continue";

    public const string NativeDevastateSummaryContinueLootSource = "native_devastate_summary_continue_loot";

    public const string NativeDevastateSummaryContinueNoLootSource = "native_devastate_summary_continue_no_loot";

    public const string CampaignTickNativeMenuDetectedSourcePrefix = "campaign_tick_native_menu_detected:";

    public const string NativeMenuInitSourcePrefix = "native_menu_init:";

    public const string UnavailableSourceSuffix = "N/A";

    public static string BuildCampaignTickNativeMenuDetectedSource(string menuId)
    {
        return CampaignTickNativeMenuDetectedSourcePrefix + NormalizeSourceSuffix(menuId);
    }

    public static string BuildNativeMenuInitSource(string source)
    {
        return NativeMenuInitSourcePrefix + NormalizeSourceSuffix(source);
    }

    private static string NormalizeSourceSuffix(string value)
    {
        return value ?? UnavailableSourceSuffix;
    }
}
