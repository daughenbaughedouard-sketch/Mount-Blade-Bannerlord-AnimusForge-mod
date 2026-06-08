namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free routing decision for the current AI/player action-tag batch.
/// </summary>
public sealed class SiegeActionRoutingDecision
{
    public SiegeActionRoutingDecision(
        bool containsDestructiveAction,
        bool hasMercyTrackAction,
        bool canApplyMercyTrack,
        bool shouldDowngradeSoldierReliefToMercy,
        bool shouldCapSoldierPositiveToRelief)
    {
        ContainsDestructiveAction = containsDestructiveAction;
        HasMercyTrackAction = hasMercyTrackAction;
        CanApplyMercyTrack = canApplyMercyTrack;
        ShouldDowngradeSoldierReliefToMercy = shouldDowngradeSoldierReliefToMercy;
        ShouldCapSoldierPositiveToRelief = shouldCapSoldierPositiveToRelief;
    }

    public bool ContainsDestructiveAction { get; }

    public bool HasMercyTrackAction { get; }

    public bool CanApplyMercyTrack { get; }

    public bool ShouldDowngradeSoldierReliefToMercy { get; }

    public bool ShouldCapSoldierPositiveToRelief { get; }
}
