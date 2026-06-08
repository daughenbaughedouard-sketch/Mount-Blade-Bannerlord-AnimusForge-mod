namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free runtime parameters for GCCZ massacre interactions.
/// AF adapters still own live mission-agent routing, order timing application, hide-point projection, and combat side effects.
/// </summary>
public static class SiegeMassacreInteractionProfile
{
    public const float CivilianHideDistance = 42f;

    public const float CivilianHideRefreshSeconds = 10.0f;

    public const float SoldierFollowRefreshSeconds = 2.0f;

    public const float SoldierTargetRefreshSeconds = 0.75f;
}
