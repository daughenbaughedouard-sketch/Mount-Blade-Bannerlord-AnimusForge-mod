using System.Collections.Generic;

namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Fallback postprocess rules for the active GCCZ intervention scene.
/// These mirror the passive ModuleData rule and keep rule wording out of the AF adapter.
/// </summary>
public static class SiegePostprocessRuleCatalog
{
    private static readonly SiegePostprocessRuleDefinition[] FallbackRules =
    {
        new SiegePostprocessRuleDefinition("[ACTION:宽恕]", "如果NPC在<latest_reply>里明确接受或传达玩家对普通民众的宽恕、不追究、不杀不抢，就输出这个；若只是害怕、讨价还价、模糊求饶或玩家没有明确给出处置，就不要输出"),
        new SiegePostprocessRuleDefinition("[ACTION:救济]", "分两种情况输出：A）当前说话者是玩家己方入城士兵时，必须同时有AF共享第纳尔/粮食/物资，并且<latest_reply>明确接受或传达把这些已交付物资分发给民众；B）当前说话者是战败平民/商人/工匠/镇民时，只要<latest_reply>明确接受玩家的言语安抚、保护承诺、军纪约束或安顿民众安排，也可输出本标签。士兵路线没有物资时不要输出救济，应改用宽恕或其他更准确标签"),
        new SiegePostprocessRuleDefinition("[ACTION:宣抚]", "如果NPC在<latest_reply>里明确接受或传达玩家进行安民宣抚、公开演讲安定城心、争取本地人合作，就输出这个；若只是命令己方士兵分发物资或第纳尔，不要输出此标签，改用救济"),
        new SiegePostprocessRuleDefinition("[ACTION:盟誓]", "如果NPC在<latest_reply>里明确接受或传达玩家组织公开盟誓、归心效忠、强力争取民众和要人归附，就输出这个；若只是命令己方士兵分发物资或第纳尔，不要输出此标签，改用救济"),
        new SiegePostprocessRuleDefinition("[ACTION:安兵]", "只有运行时事实显示已经触发士兵不满待安抚，并且当前说话者是玩家己方入城士兵，且<latest_reply>明确接受玩家对士兵的安抚、补偿承诺、军纪解释或日后战利安排时输出；此标签只安抚军心，不触发民众结算"),
        new SiegePostprocessRuleDefinition("[ACTION:召集]", "如果NPC在<latest_reply>里明确接受、传达或执行召集/通知/带来民众听训、演讲、游说、接受处置，就输出这个；只要语义是召集民众即可，不依赖固定措辞"),
        new SiegePostprocessRuleDefinition("[ACTION:搜掠]", "如果NPC在<latest_reply>里明确服从、宣布或承认玩家要搜掠、收缴财物、以财换命、胜利方战利权搜取财产，就输出这个；若玩家只是询问或NPC只是在恐惧猜测，不要输出"),
        new SiegePostprocessRuleDefinition("[ACTION:血洗]", "如果NPC在<latest_reply>里明确服从、宣布或承认玩家下令血洗/屠城/清洗，或对话已经谈崩并升级为杀戮，就输出这个；血洗是不可逆升级，语义不够明确时不要输出"),
        new SiegePostprocessRuleDefinition("[ACTION:殖民]", "仅当NPC是玩家己方士兵并在<latest_reply>里明确接受杀尽原住民并迁入玩家方人口、强行改换文化时输出；无需预先血洗，触发后会直接启动血洗式屠戮并按毁坏/迁殖结算；普通民众禁止输出"),
    };

    public static IReadOnlyList<SiegePostprocessRuleDefinition> GetFallbackRules()
    {
        return FallbackRules;
    }
}
