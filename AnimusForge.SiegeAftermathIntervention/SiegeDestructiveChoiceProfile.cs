using System;

namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free profile for destructive GCCZ choices.
/// AF adapters apply Bannerlord aftermath, troops, mission, UI, settlement, and memory side effects.
/// </summary>
public sealed class SiegeDestructiveChoiceProfile
{
    public const uint ValidationMessageColor = 0xFFFFD27Fu;

    public const uint DirectMassacreTriggerMessageColor = 0xFFFF7777u;

    public const string PlunderPolicyBlockedMessage = "【攻城处置】该定居点与你当前阵营文化相同，军纪禁止掠夺。";

    public const string MassacrePolicyBlockedMessage = "【攻城处置】该定居点与你当前阵营文化相同，军纪禁止毁坏或血洗。";

    public const string SameCultureEntryBlockedTooltip = "{=!}该定居点与你当前阵营文化相同，军纪禁止掠夺或毁坏，只能宽恕或安抚，因此无法亲自进城处置。";

    public const string SameCultureEntryBlockedMessage = "【攻城处置】该定居点与你当前阵营文化相同，军纪禁止掠夺或毁坏；本次只能宽恕或安抚。";

    public const string SameCultureActionBatchBlockedMessage = "【攻城处置】该定居点与你当前阵营文化相同，军纪禁止掠夺或毁坏，本次只能宽恕或安抚。";

    public const string PlayerWeaponAttackTriggerSource = "玩家直接攻击平民触发血洗";

    public const string PlayerHitTriggerSource = "玩家主动攻击NPC触发血洗";

    public const string PlayerAttackReleaseDamageSource = "player_attack_release_massacre_start";

    public const string PlayerAgentHitBridgeSource = "intervention_agent_hit";

    public const string PlayerScoreHitBridgeSource = "intervention_score_hit";

    public const string NonEnemyDamagePrefixSource = "non_enemy_damage_prefix";

    private SiegeDestructiveChoiceProfile(
        SiegeAftermathResolutionKind aftermathKind,
        string assemblySource,
        string messageKey,
        string messageText,
        uint messageColor,
        string memoryTitle,
        string firstMemoryText,
        string repeatMemoryText,
        int publicTrustDelta,
        string publicTrustReason,
        int finalizedPublicTrustDelta,
        string finalizedPublicTrustReason)
    {
        AftermathKind = aftermathKind;
        AssemblySource = assemblySource;
        MessageKey = messageKey;
        MessageText = messageText;
        MessageColor = messageColor;
        MemoryTitle = memoryTitle;
        FirstMemoryText = firstMemoryText;
        RepeatMemoryText = repeatMemoryText;
        PublicTrustDelta = publicTrustDelta;
        PublicTrustReason = publicTrustReason;
        FinalizedPublicTrustDelta = finalizedPublicTrustDelta;
        FinalizedPublicTrustReason = finalizedPublicTrustReason;
    }

    public SiegeAftermathResolutionKind AftermathKind { get; }

    public string AssemblySource { get; }

    public string MessageKey { get; }

    public string MessageText { get; }

    public uint MessageColor { get; }

    public string MemoryTitle { get; }

    public string FirstMemoryText { get; }

    public string RepeatMemoryText { get; }

    public int PublicTrustDelta { get; }

    public string PublicTrustReason { get; }

    public int FinalizedPublicTrustDelta { get; }

    public string FinalizedPublicTrustReason { get; }

    public static SiegeDestructiveChoiceProfile BuildPlunder()
    {
        return new SiegeDestructiveChoiceProfile(
            aftermathKind: SiegeAftermathResolutionKind.Pillage,
            assemblySource: "plunder_started",
            messageKey: "plunder",
            messageText: "【士兵搜掠】搜掠开始：一部分士兵会分散盘问民众并索取财物。若你后续明确宽恕或宣抚，可回退为正向处置；也可升级为血洗。市场金库和物资会在离场后结算。",
            messageColor: 0xFFFFC46Bu,
            memoryTitle: "搜掠",
            firstMemoryText: "玩家已下令搜掠或收缴战利品，士兵开始向民众盘问并索取财物；该处置尚可被后续宽恕/救济/宣抚覆盖。",
            repeatMemoryText: "玩家继续维持搜掠/收缴财物处置，后续NPC应承认士兵正在执行搜掠。",
            publicTrustDelta: 0,
            publicTrustReason: string.Empty,
            finalizedPublicTrustDelta: -10,
            finalizedPublicTrustReason: "siege_ai_plunder_finalized");
    }

    public static SiegeDestructiveChoiceProfile BuildMassacre()
    {
        return new SiegeDestructiveChoiceProfile(
            aftermathKind: SiegeAftermathResolutionKind.Devastate,
            assemblySource: string.Empty,
            messageKey: "massacre",
            messageText: "【攻城处置】血洗已触发：士兵将主动追击城内民众；民众不再四散逃跑，都会转为敌对并反抗。本次处置已锁定为毁坏/血洗，可随时按 TAB 离场，市场战利品在离场后结算。",
            messageColor: 0xFFFF7777u,
            memoryTitle: "血洗",
            firstMemoryText: string.Empty,
            repeatMemoryText: string.Empty,
            publicTrustDelta: -25,
            publicTrustReason: "siege_ai_massacre",
            finalizedPublicTrustDelta: 0,
            finalizedPublicTrustReason: string.Empty);
    }

    public static string DescribeMassacreMemorySource(string triggerSource)
    {
        string source = triggerSource ?? string.Empty;
        if (source.IndexOf("直接攻击", StringComparison.OrdinalIgnoreCase) >= 0
            || source.IndexOf("主动攻击", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "由玩家亲自攻击NPC或平民触发";
        }

        if (source.IndexOf("屠民迁殖", StringComparison.OrdinalIgnoreCase) >= 0
            || source.IndexOf("迁殖", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "由玩家通过己方士兵下达屠民迁殖命令触发";
        }

        return "由玩家通过场景对话命令或谈崩触发";
    }

    public string BuildMassacreMemoryText(string triggerSource)
    {
        return DescribeMassacreMemorySource(triggerSource) + "，本次处置已不可逆升级为毁坏/血洗。";
    }

    public static string BuildPlayerWeaponAttackMessage(string targetName)
    {
        return "【攻城处置】你挥武器攻击 " + NormalizeTargetName(targetName, "一名平民") + "，本次入城处置转为血洗。";
    }

    public static string BuildPlayerWeaponAttackTriggerDetail(string targetName)
    {
        return "玩家在攻城后亲自进城时直接挥武器攻击" + NormalizeTargetName(targetName, "一名平民") + "，本次处置按血洗处理。";
    }

    public static string BuildPlayerHitMessage(string targetName)
    {
        return "【攻城处置】你击中了 " + NormalizeTargetName(targetName, "一名NPC") + "，本次入城处置进入血洗。";
    }

    public static string BuildPlayerHitTriggerDetail(string targetName)
    {
        return "玩家在攻城后亲自进城期间主动攻击了" + NormalizeTargetName(targetName, "一名NPC") + "，本次处置按血洗处理。";
    }

    private static string NormalizeTargetName(string targetName, string fallback)
    {
        return string.IsNullOrWhiteSpace(targetName) ? fallback : targetName.Trim();
    }
}
