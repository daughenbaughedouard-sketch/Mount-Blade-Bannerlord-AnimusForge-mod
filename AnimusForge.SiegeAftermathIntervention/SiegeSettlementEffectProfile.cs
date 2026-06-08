namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free reason codes for GCCZ settlement-effect mutations.
/// AF adapters still own Bannerlord settlement, town, and reward-system side effects.
/// </summary>
public static class SiegeSettlementEffectProfile
{
    public const string PositivePublicTrustReason = "siege_ai_relief";
}
