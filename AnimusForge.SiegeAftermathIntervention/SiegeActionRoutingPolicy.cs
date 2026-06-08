using System.Linq;

namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free routing policy for postprocess action tags before AF applies side effects.
/// </summary>
public static class SiegeActionRoutingPolicy
{
    public static SiegeActionRoutingDecision Evaluate(SiegeActionRoutingFacts facts)
    {
        facts ??= new SiegeActionRoutingFacts(string.Empty, false, false, false);

        var kinds = SiegeActionTagCatalog.ExtractKinds(facts.RawActionText);
        bool containsDestructiveAction = kinds.Any(SiegeInterventionActionRules.IsDestructive);
        bool hasMercyTrackAction = kinds.Any(SiegeInterventionActionRules.IsMercyTrack);
        bool canApplyMercyTrack = !containsDestructiveAction && !facts.DestructiveOutcomeLocked;
        bool hasReliefAction = kinds.Contains(SiegeInterventionActionKind.Relief);
        bool hasSoldierPositiveCapCandidate = kinds.Contains(SiegeInterventionActionKind.Inspire)
            || kinds.Contains(SiegeInterventionActionKind.RallyOath);

        return new SiegeActionRoutingDecision(
            containsDestructiveAction,
            hasMercyTrackAction,
            canApplyMercyTrack,
            shouldDowngradeSoldierReliefToMercy: facts.TargetIsAlliedSoldier && hasReliefAction && !facts.HasSharedReliefPool,
            shouldCapSoldierPositiveToRelief: facts.TargetIsAlliedSoldier && canApplyMercyTrack && facts.HasSharedReliefPool && hasSoldierPositiveCapCandidate);
    }
}
