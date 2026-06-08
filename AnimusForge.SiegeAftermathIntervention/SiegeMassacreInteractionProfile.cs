namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free runtime parameters and source codes for GCCZ massacre interactions.
/// AF adapters still own live mission-agent routing, order timing application, hide-point projection, and combat side effects.
/// </summary>
public static class SiegeMassacreInteractionProfile
{
    public const float CivilianHideDistance = 42f;

    public const float CivilianHideRefreshSeconds = 10.0f;

    public const float SoldierFollowRefreshSeconds = 2.0f;

    public const float SoldierTargetRefreshSeconds = 0.75f;

    public const string OccupationFollowSource = "massacre_occupation_follow";

    public const string CombatPrepareSource = "massacre_combat_prepare";

    public const string AlliedCombatDriveSource = "massacre_drive";

    public const string AllTargetsDownVictorySource = "all_targets_down";
}
