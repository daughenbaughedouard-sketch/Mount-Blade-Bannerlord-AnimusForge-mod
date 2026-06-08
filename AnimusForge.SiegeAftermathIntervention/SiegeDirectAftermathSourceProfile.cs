namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free source codes for direct AF aftermath campaign scripts.
/// AF adapters still own Bannerlord campaign ticks, loot-screen timing, and encounter transitions.
/// </summary>
public static class SiegeDirectAftermathSourceProfile
{
    public const string CampaignTickDirectMassacreScriptSource = "campaign_tick_direct_massacre_script";

    public const string CampaignTickDirectPlunderScriptSource = "campaign_tick_direct_plunder_script";

    public const string DirectMassacrePendingAftermathSource = "direct_massacre_script_pending_aftermath";

    public const string DirectMassacreFallbackPumpSource = "direct_massacre_script";

    public const string DirectMassacreAfterLootSource = "direct_massacre_script_after_loot";

    public const string DirectMassacreNoLootSource = "direct_massacre_script_no_loot";

    public const string DirectPlunderPendingAftermathSource = "direct_plunder_script_pending_aftermath";

    public const string DirectPlunderFallbackPumpSource = "direct_plunder_script";

    public const string DirectPlunderAfterLootSource = "direct_plunder_script_after_loot";

    public const string DirectPlunderNoLootSource = "direct_plunder_script_no_loot";
}
