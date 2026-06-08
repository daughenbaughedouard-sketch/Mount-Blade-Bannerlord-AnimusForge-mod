using System;

namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free profile for non-destructive civic choices after a siege.
/// AF adapters apply Bannerlord settlement, notable, gather, UI, and memory side effects.
/// </summary>
public sealed class SiegeCivicChoiceProfile
{
    private const uint PositiveMessageColor = 0xFFB6F7A8u;

    private const float InspirationLoyaltyBonus = 12f;
    private const float InspirationSecurityBonus = 4f;
    private const int InspirationPublicTrustBonus = 12;
    private const int InspirationNotableRelationBonus = 4;
    private const float InspirationNotablePowerBonus = 2f;
    private const float RallyOathLoyaltyBonus = 28f;
    private const float RallyOathSecurityBonus = 10f;
    private const int RallyOathPublicTrustBonus = 32;
    private const int RallyOathNotableRelationBonus = 12;
    private const float RallyOathNotablePowerBonus = 8f;

    private SiegeCivicChoiceProfile(
        string soldierAppeasementReason,
        int publicTrustDelta,
        float loyaltyDelta,
        float securityDelta,
        int notableRelationDelta,
        float notablePowerDelta,
        int resultingInspirationLevel,
        string sharedPoolEffectReason,
        string gatherSource,
        string messageKey,
        string messageText,
        string memoryTitle,
        string memoryText,
        string repeatSharedPoolEffectReason,
        string repeatMemoryTitle,
        string repeatMemoryText)
    {
        SoldierAppeasementReason = soldierAppeasementReason;
        PublicTrustDelta = publicTrustDelta;
        LoyaltyDelta = loyaltyDelta;
        SecurityDelta = securityDelta;
        NotableRelationDelta = notableRelationDelta;
        NotablePowerDelta = notablePowerDelta;
        ResultingInspirationLevel = resultingInspirationLevel;
        SharedPoolEffectReason = sharedPoolEffectReason;
        GatherSource = gatherSource;
        MessageKey = messageKey;
        MessageText = messageText;
        MessageColor = PositiveMessageColor;
        MemoryTitle = memoryTitle;
        MemoryText = memoryText;
        RepeatSharedPoolEffectReason = repeatSharedPoolEffectReason;
        RepeatMemoryTitle = repeatMemoryTitle;
        RepeatMemoryText = repeatMemoryText;
    }

    public string SoldierAppeasementReason { get; }

    public int PublicTrustDelta { get; }

    public float LoyaltyDelta { get; }

    public float SecurityDelta { get; }

    public int NotableRelationDelta { get; }

    public float NotablePowerDelta { get; }

    public int ResultingInspirationLevel { get; }

    public string SharedPoolEffectReason { get; }

    public string GatherSource { get; }

    public string MessageKey { get; }

    public string MessageText { get; }

    public uint MessageColor { get; }

    public string MemoryTitle { get; }

    public string MemoryText { get; }

    public string RepeatSharedPoolEffectReason { get; }

    public string RepeatMemoryTitle { get; }

    public string RepeatMemoryText { get; }

    public static SiegeCivicChoiceProfile BuildInspiration()
    {
        return new SiegeCivicChoiceProfile(
            soldierAppeasementReason: "宣抚",
            publicTrustDelta: InspirationPublicTrustBonus,
            loyaltyDelta: InspirationLoyaltyBonus,
            securityDelta: InspirationSecurityBonus,
            notableRelationDelta: InspirationNotableRelationBonus,
            notablePowerDelta: InspirationNotablePowerBonus,
            resultingInspirationLevel: 1,
            sharedPoolEffectReason: "inspiration",
            gatherSource: "inspiration",
            messageKey: "inspiration",
            messageText: "【攻城处置】安民宣抚完成：忠诚度有所提升，本地要人对你更愿意合作。",
            memoryTitle: "宣抚",
            memoryText: "玩家已进行安民宣抚，召集民众并宣示新秩序，要求后续NPC承认这条宽恕/安抚路线。",
            repeatSharedPoolEffectReason: "inspiration_repeat",
            repeatMemoryTitle: "宣抚",
            repeatMemoryText: "玩家继续维持安民宣抚路线，NPC应承认民众已被安抚和宣示新秩序。");
    }

    public static SiegeCivicChoiceProfile BuildRallyOath(int currentInspirationLevel)
    {
        bool alreadyInspired = currentInspirationLevel >= 1;
        return new SiegeCivicChoiceProfile(
            soldierAppeasementReason: "盟誓",
            publicTrustDelta: alreadyInspired ? Math.Max(0, RallyOathPublicTrustBonus - InspirationPublicTrustBonus) : RallyOathPublicTrustBonus,
            loyaltyDelta: alreadyInspired ? Math.Max(0f, RallyOathLoyaltyBonus - InspirationLoyaltyBonus) : RallyOathLoyaltyBonus,
            securityDelta: alreadyInspired ? Math.Max(0f, RallyOathSecurityBonus - InspirationSecurityBonus) : RallyOathSecurityBonus,
            notableRelationDelta: alreadyInspired ? Math.Max(0, RallyOathNotableRelationBonus - InspirationNotableRelationBonus) : RallyOathNotableRelationBonus,
            notablePowerDelta: alreadyInspired ? Math.Max(0f, RallyOathNotablePowerBonus - InspirationNotablePowerBonus) : RallyOathNotablePowerBonus,
            resultingInspirationLevel: 2,
            sharedPoolEffectReason: "rally_oath",
            gatherSource: "rally_oath",
            messageKey: "rally_oath",
            messageText: "【攻城处置】归心盟誓完成：忠诚度与治安显著提升，本地要人更倾向协助招兵和治理。",
            memoryTitle: "盟誓",
            memoryText: "玩家已组织公开归心盟誓，强力争取民众和本地要人归附。",
            repeatSharedPoolEffectReason: "rally_oath_repeat",
            repeatMemoryTitle: "盟誓",
            repeatMemoryText: "玩家继续维持归心盟誓路线，本地民众和要人应被视为已被公开争取归附。");
    }
}
