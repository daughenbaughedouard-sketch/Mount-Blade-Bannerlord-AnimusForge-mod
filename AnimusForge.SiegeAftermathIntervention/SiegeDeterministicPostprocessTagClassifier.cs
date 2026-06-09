using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Conservative local fallback for GCCZ postprocess action tags.
/// This is used only as a safety net when the auxiliary postprocessor is missing,
/// delayed, or returns only a mood tag in the active siege-intervention scene.
/// </summary>
public static class SiegeDeterministicPostprocessTagClassifier
{
    public static string BuildTagBlock(SiegeDeterministicPostprocessTagFacts facts)
    {
        if (facts == null || string.IsNullOrWhiteSpace(facts.PlayerText + facts.ReplyText))
        {
            return string.Empty;
        }

        var tags = new List<string>();
        void Add(SiegeInterventionActionKind kind)
        {
            if (SiegeActionTagCatalog.TryGetCanonicalTag(kind, out string tag) && !tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        string player = Normalize(facts.PlayerText);
        string reply = Normalize(facts.ReplyText);
        bool accepted = IsAcceptedOrExecuted(reply);

        if (IsGatherIntent(player, reply, accepted))
        {
            Add(SiegeInterventionActionKind.GatherCivilians);
        }

        if (IsMercyIntent(player, reply, accepted))
        {
            Add(SiegeInterventionActionKind.Mercy);
        }

        if (IsReliefIntent(player, reply, accepted, facts.TargetIsAlliedSoldier, facts.HasSharedReliefPool))
        {
            Add(SiegeInterventionActionKind.Relief);
        }

        if (IsInspireIntent(player, reply, accepted))
        {
            Add(SiegeInterventionActionKind.Inspire);
        }

        if (IsRallyOathIntent(player, reply, accepted))
        {
            Add(SiegeInterventionActionKind.RallyOath);
        }

        if (IsSoldierAppeasementIntent(player, reply, accepted, facts.TargetIsAlliedSoldier, facts.SoldierAppeasementRequired, facts.SoldierAppeasementApplied))
        {
            Add(SiegeInterventionActionKind.AppeaseSoldiers);
        }

        if (IsPlunderIntent(player, reply, accepted))
        {
            Add(SiegeInterventionActionKind.Plunder);
        }

        if (IsMassacreIntent(player, reply, accepted))
        {
            Add(SiegeInterventionActionKind.Massacre);
        }

        if (IsCulturalRepopulationIntent(player, reply, accepted, facts.TargetIsAlliedSoldier))
        {
            Add(SiegeInterventionActionKind.CulturalRepopulation);
        }

        return string.Join("\n", tags).Trim();
    }

    private static string Normalize(string text)
    {
        return (text ?? string.Empty).Replace("\r", "\n").Trim().ToLowerInvariant();
    }

    private static bool IsAcceptedOrExecuted(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply))
        {
            return true;
        }

        return ContainsAny(reply,
            "遵命", "领命", "照办", "这就", "马上", "立刻", "已经", "愿意", "接受", "遵从", "奉命",
            "听候", "服从", "感激", "感谢", "宽宏", "仁慈", "会去", "去做", "执行", "传达", "通知",
            "喊人", "叫人", "聚来", "带来", "分发", "发放", "交出", "收缴", "搜", "杀", "屠");
    }

    private static bool IsGatherIntent(string player, string reply, bool accepted)
    {
        bool playerIntent = ContainsAny(player, "召集", "集合", "聚集", "喊人", "叫人", "叫来", "叫过来", "带过来", "通知", "传令")
            && ContainsAny(player + " " + reply, "平民", "民众", "百姓", "村民", "镇民", "商民", "居民", "所有人", "大家", "听训", "训话", "演讲", "广场", "街口", "过来");
        bool replyExecuted = ContainsAny(reply, "召集", "集合", "聚集", "喊人", "叫人", "叫过来", "带过来", "通知", "传令")
            && ContainsAny(reply, "平民", "民众", "百姓", "村民", "镇民", "商民", "居民", "所有人", "大家", "听训", "训话", "广场", "过来");
        return playerIntent || (accepted && replyExecuted);
    }

    private static bool IsMercyIntent(string player, string reply, bool accepted)
    {
        bool playerIntent = ContainsAny(player, "宽恕", "饶恕", "饶过", "放过", "赦免", "免死", "不杀", "不抢", "不追究", "留你们性命", "保你们性命");
        bool replyAcknowledgesMercy = ContainsAny(reply, "宽恕", "饶恕", "放过", "仁慈", "开恩", "不杀", "活命", "免死");
        return playerIntent || (accepted && replyAcknowledgesMercy);
    }

    private static bool IsReliefIntent(string player, string reply, bool accepted, bool targetIsAlliedSoldier, bool hasSharedReliefPool)
    {
        bool materialIntent = ContainsAny(player, "救济", "赈济", "发放", "分发", "给他们粮", "给民众粮", "给百姓粮", "给平民粮", "给钱", "给第纳尔", "粮食", "食物", "物资", "安顿");
        bool civilianVerbalIntent = ContainsAny(player, "保护", "安置", "安顿", "约束士兵", "严明军纪", "不许扰民", "不许抢", "维持秩序");
        bool replyRelief = ContainsAny(reply, "救济", "发放", "分发", "粮食", "第纳尔", "物资", "保护", "安顿", "军纪", "秩序", "恩泽");
        if (targetIsAlliedSoldier)
        {
            return (hasSharedReliefPool && materialIntent) || (accepted && hasSharedReliefPool && replyRelief);
        }

        return materialIntent || civilianVerbalIntent || (accepted && replyRelief);
    }

    private static bool IsInspireIntent(string player, string reply, bool accepted)
    {
        bool playerIntent = ContainsAny(player, "宣抚", "安民", "安定民心", "安定城心", "稳定民心", "公开演讲", "发表演讲", "发布告示", "安抚全城");
        bool replyIntent = ContainsAny(reply, "宣抚", "安民", "安定民心", "安定城心", "演讲", "告示", "安抚全城");
        return playerIntent || (accepted && replyIntent);
    }

    private static bool IsRallyOathIntent(string player, string reply, bool accepted)
    {
        bool playerIntent = ContainsAny(player, "盟誓", "宣誓", "效忠", "归附", "归顺", "臣服", "向我效命", "归心", "效力于我");
        bool replyIntent = ContainsAny(reply, "盟誓", "宣誓", "效忠", "归附", "归顺", "臣服", "效命", "归心");
        return playerIntent || (accepted && replyIntent);
    }

    private static bool IsSoldierAppeasementIntent(string player, string reply, bool accepted, bool targetIsAlliedSoldier, bool required, bool applied)
    {
        if (!targetIsAlliedSoldier || !required || applied)
        {
            return false;
        }

        bool playerIntent = ContainsAny(player, "安抚士兵", "安抚军心", "补偿士兵", "补偿你们", "战利补偿", "军心", "士气", "日后战利", "战利安排");
        bool replyIntent = ContainsAny(reply, "军心", "士气", "补偿", "战利", "服从", "不满");
        return playerIntent || (accepted && replyIntent);
    }

    private static bool IsPlunderIntent(string player, string reply, bool accepted)
    {
        bool playerIntent = ContainsAny(player, "搜掠", "掠夺", "洗劫", "劫掠", "抢掠", "抢光", "收缴财物", "收缴货物", "交出财物", "交钱", "交出钱粮", "财产换命", "以财换命", "战利品");
        bool replyIntent = ContainsAny(reply, "搜掠", "掠夺", "洗劫", "劫掠", "抢掠", "收缴", "交出财物", "交出钱粮", "财产换命", "战利品");
        return playerIntent || (accepted && ContainsAny(player, "拿", "取", "收") && replyIntent);
    }

    private static bool IsMassacreIntent(string player, string reply, bool accepted)
    {
        bool playerIntent = ContainsAny(player, "血洗", "屠城", "屠杀", "杀光", "杀尽", "一个不留", "全部杀", "清洗全城", "灭口");
        bool replyIntent = ContainsAny(reply, "血洗", "屠城", "屠杀", "杀光", "杀尽", "一个不留", "清洗全城");
        return playerIntent || (accepted && replyIntent);
    }

    private static bool IsCulturalRepopulationIntent(string player, string reply, bool accepted, bool targetIsAlliedSoldier)
    {
        if (!targetIsAlliedSoldier)
        {
            return false;
        }

        bool playerIntent = ContainsAny(player, "殖民", "迁入", "迁民", "改换文化", "改文化", "迁入我国", "换成本族", "杀尽原住民", "清空原住民");
        bool replyIntent = ContainsAny(reply, "殖民", "迁入", "迁民", "改换文化", "改文化", "原住民");
        return playerIntent || (accepted && replyIntent);
    }

    private static bool ContainsAny(string text, params string[] terms)
    {
        if (string.IsNullOrWhiteSpace(text) || terms == null)
        {
            return false;
        }

        for (int i = 0; i < terms.Length; i++)
        {
            string term = terms[i];
            if (!string.IsNullOrWhiteSpace(term) && text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
