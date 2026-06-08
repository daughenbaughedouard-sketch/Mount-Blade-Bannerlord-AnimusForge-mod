namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free runtime parameters for GCCZ soldier cordon positioning and order refresh timing.
/// AF adapters still own live soldier selection, target-slot projection, movement orders, and look-at side effects.
/// </summary>
public static class SiegeSoldierCordonProfile
{
    public const float MinRadius = 7.2f;

    public const float Padding = 2.8f;

    public const float TeleportDistance = 18f;

    public const float MoveTolerance = 0.75f;

    public const float SettleTolerance = 0.45f;

    public const float OrderRefreshSeconds = 1.25f;

    public const float LookRefreshSeconds = 1.1f;
}
