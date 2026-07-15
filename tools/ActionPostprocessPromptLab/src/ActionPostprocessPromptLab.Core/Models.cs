namespace ActionPostprocessPromptLab.Core;

public sealed class PostprocessRuleEntry
{
    public string Tag { get; set; } = "";

    public string Description { get; set; } = "";
}

public sealed class ActionPostprocessConfigModel
{
    public int Version { get; set; } = 1;

    public bool IsEnabled { get; set; } = true;

    public string SystemPrompt { get; set; } = "";

    public string UserPromptTemplate { get; set; } = "";

    public string FallbackMoodTag { get; set; } = "[ACTION:MOOD:NEUTRAL]";

    public List<PostprocessRuleEntry> WildernessPostprocessRules { get; set; } = new();

    public List<PostprocessRuleEntry> RoyalPostprocessRules { get; set; } = new();

    public List<PostprocessRuleEntry> MoodRules { get; set; } = new();

    public Dictionary<string, Dictionary<string, string>> RulePostprocessDescriptionOverrides { get; set; } = new();
}

public sealed class PromptRuleInfo
{
    private static readonly Dictionary<string, string> ChineseLabelsById = new(StringComparer.OrdinalIgnoreCase)
    {
        ["duel"] = "决斗",
		["reward"] = "资产交易、转让、赠予、借贷与偿还",
        ["kingdom_service"] = "加入或离开王国：雇佣兵/封臣",
        ["lords_hall_access"] = "请求通行或进入大厅",
        ["marriage"] = "婚姻与联姻",
        ["scene_mechanism_actions"] = "本地 3D 场景移动：带路/跟随（不含大地图与攻击）",
        ["party_transfer"] = "NPC 部队/俘虏转交给玩家；名人志愿兵招募",
        ["vanilla_issue"] = "原版任务：NPC 提供、接受、追踪或交还任务",
        ["encounter_release_player"] = "遭遇时释放玩家",
        ["hero_join_party"] = "招募 NPC 加入玩家队伍",
        ["kingdom_agenda"] = "王国议程投票与提案",
        ["worldmap_party_command"] = "大地图移动与攻击命令",
        ["diplomacy"] = "王室外交 / 国王对国王谈判",
        ["kingdom_vassalage"] = "王国附庸与臣服",
        ["noble_gathering"] = "贵族宴会 / 聚会举办与邀请",
        ["scene_auto_group_relay"] = "场景自动群体转发后处理",
        ["siege_intervention_aftermath"] = "AnimusForge 攻城后场景结算",
        ["action:royal"] = "王室后处理通用规则",
        ["action:wilderness"] = "野外后处理通用规则"
    };

    public string Id { get; set; } = "";

    public string Source { get; set; } = "";

    public int TopicNumber { get; set; }

    public string TopicLabel { get; set; } = "";

    public string Code { get; set; } = "";

    public bool IsEnabled { get; set; } = true;

    public List<PostprocessRuleEntry> PostprocessRules { get; set; } = new();

    public string DisplayName
    {
        get
        {
            var prefix = TopicNumber > 0 ? TopicNumber + " " : "";
            var label = ChineseLabelsById.TryGetValue(Id, out var chineseLabel) ? chineseLabel : TopicLabel;
            if (string.IsNullOrWhiteSpace(label))
            {
                label = Id;
            }

            return prefix + label + " [" + Id + "]";
        }
    }
}

public sealed class PromptCatalog
{
    public string RepoRoot { get; set; } = "";

    public string RuleBehaviorPath { get; set; } = "";

    public string ActionPostprocessPath { get; set; } = "";

    public ActionPostprocessConfigModel ActionConfig { get; set; } = new();

    public List<PromptRuleInfo> Rules { get; set; } = new();
}

public sealed class AfefFact
{
    public string Kind { get; set; } = "player";

    public string Text { get; set; } = "";
}

public sealed class PromptLabCase
{
    public string CaseId { get; set; } = "";

    public string Title { get; set; } = "";

    public List<string> PreprocessHits { get; set; } = new();

    public string PlayerText { get; set; } = "";

    public string NpcReplyText { get; set; } = "";

    public List<string> HistoryLines { get; set; } = new();

    public List<AfefFact> AfefFacts { get; set; } = new();

    public string RuntimeContext { get; set; } = "";

    public List<string> ExpectedTags { get; set; } = new();

    public string Notes { get; set; } = "";

    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? CaseId : CaseId + " - " + Title;
}

public sealed class PromptLabSettings
{
    public string ApiProtocol { get; set; } = "auto";

    public string ApiUrl { get; set; } = "https://api.openai.com/v1/chat/completions";

    public string ApiKey { get; set; } = "";

    public string Model { get; set; } = "";

    public float Temperature { get; set; } = 0f;

    public int MaxTokens { get; set; } = 5000;

    public bool ThinkingEnabled { get; set; } = true;

    public string ReasoningEffort { get; set; } = "max";

    public string PromptVersionPath { get; set; } = "";
}

public sealed class RenderedPrompt
{
    public string SystemPrompt { get; set; } = "";

    public string UserPrompt { get; set; } = "";

    public string LatestReplyBlock { get; set; } = "";

    public string HistoryText { get; set; } = "";

    public string TagRules { get; set; } = "";

    public string MoodRules { get; set; } = "";

    public string RequestJson { get; set; } = "";
}

public sealed class ApiCallResult
{
    public bool Success { get; set; }

    public int StatusCode { get; set; }

    public string AssistantText { get; set; } = "";

    public string RawResponse { get; set; } = "";

    public string Error { get; set; } = "";
}

public sealed class RunArtifact
{
    public string CaseId { get; set; } = "";

    public string PromptPath { get; set; } = "";

    public string RequestPath { get; set; } = "";

    public string ResponsePath { get; set; } = "";

    public string MetaPath { get; set; } = "";

    public ApiCallResult Result { get; set; } = new();
}
