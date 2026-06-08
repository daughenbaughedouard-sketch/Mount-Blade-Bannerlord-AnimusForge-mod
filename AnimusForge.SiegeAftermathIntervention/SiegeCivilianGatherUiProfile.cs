namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free UI and memory wording for the GCCZ civilian-gathering flow.
/// AF adapters still own live mission agents, formation control, and display side effects.
/// </summary>
public static class SiegeCivilianGatherUiProfile
{
    public const uint MessageColor = 0xFFB6F7A8u;

    public const string GatherMemoryTitle = "召集";

    public const string AssemblyMemoryTitle = "聚集";

    public const string PropagationStartedMessage = "【攻城处置】传令已经发出，民众会逐步聚拢，等待你的进一步命令。";

    public const string MessengerAddedMessage = "【攻城处置】新的传令者已加入召集。";

    public const string FormationReadyMessage = "【攻城处置】民众已经聚拢听命，你现在可以像战场上调度队列一样让他们列阵。";

    public const string FormationCompleteMemory = "民众已经完成聚集并编入玩家可调度的民众队列，后续NPC应知道民众已到场听命。";

    public static string BuildCivilianPreparedMessage(int civilianCount)
    {
        int safeCivilianCount = civilianCount < 0 ? 0 : civilianCount;
        return "【攻城处置】城内有 " + safeCivilianCount + " 名普通民众等待处置。士兵已跟随你入城。";
    }

    public static string BuildPropagationStartedMemory(bool seedIsSoldier)
    {
        return "玩家已下令召集民众，" + (seedIsSoldier ? "己方士兵" : "平民") + "开始作为传令者通知城内民众前来听训/接受处置。";
    }

    public static string BuildMessengerAddedMemory(int messengerCount)
    {
        int safeMessengerCount = messengerCount < 0 ? 0 : messengerCount;
        return "玩家追加了传令者继续通知民众；当前传令者约 " + safeMessengerCount + " 人。";
    }

    public static string BuildFormationQueuedMemory(string reason)
    {
        return "民众召集进入收束阶段，系统正把已跟随的平民转入玩家可调度的民众队列；原因=" + NormalizeReason(reason) + "。";
    }

    private static string NormalizeReason(string reason)
    {
        return string.IsNullOrWhiteSpace(reason) ? "N/A" : reason.Trim();
    }
}
