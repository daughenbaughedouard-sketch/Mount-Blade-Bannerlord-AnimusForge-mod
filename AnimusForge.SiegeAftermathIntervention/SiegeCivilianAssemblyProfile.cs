namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free runtime parameters for GCCZ civilian assembly counts and layout.
/// AF adapters still own scene capacity checks, spawn gating, formation slot projection, and mission side effects.
/// </summary>
public static class SiegeCivilianAssemblyProfile
{
    public const int MinDesiredCivilianCount = 180;

    public const int MaxDesiredCivilianCount = 220;

    public const int TownSceneCap = 140;

    public const int CastleSceneCap = 90;

    public const int SmallSceneExtraCap = 70;

    public const int SceneTotalAgentSoftCap = 220;

    public const int MinimumSceneCap = 60;

    public const bool EnableExtraSpawns = false;

    public const float ForwardDistance = 4.2f;

    public const float ColumnSpacing = 0.9f;

    public const float RowSpacing = 0.78f;

    public const int Columns = 14;
}
