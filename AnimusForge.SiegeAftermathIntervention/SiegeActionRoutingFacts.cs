namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free facts for routing postprocess action tags before AF applies Bannerlord side effects.
/// </summary>
public sealed class SiegeActionRoutingFacts
{
    public SiegeActionRoutingFacts(
        string rawActionText,
        bool destructiveOutcomeLocked,
        bool targetIsAlliedSoldier,
        bool hasSharedReliefPool)
    {
        RawActionText = rawActionText ?? string.Empty;
        DestructiveOutcomeLocked = destructiveOutcomeLocked;
        TargetIsAlliedSoldier = targetIsAlliedSoldier;
        HasSharedReliefPool = hasSharedReliefPool;
    }

    public string RawActionText { get; }

    public bool DestructiveOutcomeLocked { get; }

    public bool TargetIsAlliedSoldier { get; }

    public bool HasSharedReliefPool { get; }
}
