namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free facts used by the local GCCZ fallback tag classifier.
/// AF adapters provide live mission state; the classifier only reads text and coarse speaker state.
/// </summary>
public sealed class SiegeDeterministicPostprocessTagFacts
{
    public SiegeDeterministicPostprocessTagFacts(
        string playerText,
        string replyText,
        bool targetIsAlliedSoldier,
        bool targetIsCivilian,
        bool hasSharedReliefPool,
        bool soldierAppeasementRequired,
        bool soldierAppeasementApplied)
    {
        PlayerText = playerText ?? string.Empty;
        ReplyText = replyText ?? string.Empty;
        TargetIsAlliedSoldier = targetIsAlliedSoldier;
        TargetIsCivilian = targetIsCivilian;
        HasSharedReliefPool = hasSharedReliefPool;
        SoldierAppeasementRequired = soldierAppeasementRequired;
        SoldierAppeasementApplied = soldierAppeasementApplied;
    }

    public string PlayerText { get; }

    public string ReplyText { get; }

    public bool TargetIsAlliedSoldier { get; }

    public bool TargetIsCivilian { get; }

    public bool HasSharedReliefPool { get; }

    public bool SoldierAppeasementRequired { get; }

    public bool SoldierAppeasementApplied { get; }
}
