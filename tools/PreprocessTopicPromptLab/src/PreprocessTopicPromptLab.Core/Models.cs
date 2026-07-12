namespace PreprocessTopicPromptLab.Core;

public sealed class PostprocessRuleEntry
{
    public string Tag { get; set; } = "";

    public string Description { get; set; } = "";
}

public sealed class PreprocessPromptConfig
{
    public int Version { get; set; } = 1;

    public bool IsEnabled { get; set; } = true;

    public string SystemPrompt { get; set; } = PreprocessTopicLabService.DefaultSystemPrompt;

    public string UserPromptTemplate { get; set; } = PreprocessTopicLabService.DefaultUserPromptTemplate;

    public string RoutingGuidance { get; set; } = "";

    public int RuleInstructionMaxChars { get; set; } = 180;

    public Dictionary<string, TopicRouteOverride> TopicOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class TopicRouteOverride
{
    public string Code { get; set; } = "";

    public string TopicLabel { get; set; } = "";

    public bool? IsEnabled { get; set; }
}

public sealed class TopicRuleInfo
{
    private static readonly Dictionary<string, string> ChineseLabelsById = new(StringComparer.OrdinalIgnoreCase)
    {
        ["duel"] = "决斗",
        ["reward"] = "交易、转让、赠予、借贷与偿还",
        ["loan"] = "借贷",
        ["surroundings"] = "周边定居点信息",
        ["kingdom_service"] = "加入或离开王国：雇佣兵/封臣",
        ["lords_hall_access"] = "请求通行或进入大厅",
        ["marriage"] = "婚姻与联姻",
        ["scene_mechanism_actions"] = "本地 3D 场景移动：带路/跟随",
        ["party_transfer"] = "NPC 部队/俘虏转交给玩家；名人志愿兵招募",
        ["settlement_transfer"] = "固定资产转移：领地、工坊、商队、贸易车队",
        ["vanilla_issue"] = "原版任务",
        ["npc_major_actions"] = "NPC 重大背景/历史",
        ["npc_recent_actions"] = "NPC 近期行动",
        ["encounter_release_player"] = "遭遇时释放玩家",
        ["hero_join_party"] = "招募 NPC 加入玩家队伍",
        ["noble_deference"] = "贵族身份压迫/头衔威慑",
        ["kingdom_agenda"] = "王国议程投票与提案",
        ["worldmap_party_command"] = "大地图移动与攻击命令",
        ["diplomacy"] = "王室外交 / 国王谈判",
        ["kingdom_vassalage"] = "王国附庸与臣服",
        ["noble_gathering"] = "贵族宴会 / 聚会举办与邀请",
        ["scene_auto_group_relay"] = "场景群体接话/转发",
        ["siege_intervention_aftermath"] = "攻城后场景结算"
    };

    public string Id { get; set; } = "";

    public string Source { get; set; } = "";

    public int TopicNumber { get; set; }

    public string TopicLabel { get; set; } = "";

    public string Code { get; set; } = "";

    public bool IsEnabled { get; set; } = true;

    public string Group { get; set; } = "";

    public int Priority { get; set; }

    public string Instruction { get; set; } = "";

    public string NonHeroInstruction { get; set; } = "";

    public List<string> TriggerKeywords { get; set; } = new();

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

    public List<TopicRuleInfo> Rules { get; set; } = new();
}

public sealed class AfefFact
{
    public string Kind { get; set; } = "player";

    public string Text { get; set; } = "";
}

public sealed class PreprocessLabCase
{
    public string CaseId { get; set; } = "";

    public string Title { get; set; } = "";

    public string PlayerText { get; set; } = "";

    public string NpcReplyText { get; set; } = "";

    public List<string> HistoryLines { get; set; } = new();

    public List<AfefFact> AfefFacts { get; set; } = new();

    public string RuntimeContext { get; set; } = "";

    public List<string> ExpectedTopics { get; set; } = new();

    public List<string> AllowedExtraTopics { get; set; } = new();

    public List<string> ForbiddenTopics { get; set; } = new();

    public string Notes { get; set; } = "";

    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? CaseId : CaseId + " - " + Title;
}

public sealed class PreprocessLabSettings
{
    public string ApiProtocol { get; set; } = "auto";

    public string ApiUrl { get; set; } = "https://api.openai.com/v1/chat/completions";

    public string ApiKey { get; set; } = "";

    public string Model { get; set; } = "";

    public float Temperature { get; set; } = 0f;

    public int MaxTokens { get; set; } = 512;

    public bool ThinkingEnabled { get; set; }

    public string ReasoningEffort { get; set; } = "low";

    public string PromptVersionPath { get; set; } = "";
}

public sealed class RenderedPreprocessPrompt
{
    public string SystemPrompt { get; set; } = "";

    public string UserPrompt { get; set; } = "";

    public string TopicRules { get; set; } = "";

    public string HistoryText { get; set; } = "";

    public string AfefText { get; set; } = "";

    public string RequestJson { get; set; } = "";
}

public sealed class ApiCallResult
{
    public bool Success { get; set; }

    public int StatusCode { get; set; }

    public string AssistantText { get; set; } = "";

    public string RawResponse { get; set; } = "";

    public int InputTokens { get; set; }

    public int CacheCreationInputTokens { get; set; }

    public int CacheReadInputTokens { get; set; }

    public int OutputTokens { get; set; }

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public int TotalTokens { get; set; }

    public string Error { get; set; } = "";
}

public sealed class TopicScoreResult
{
    public List<string> ExpectedTopics { get; set; } = new();

    public List<string> AllowedExtraTopics { get; set; } = new();

    public List<string> ForbiddenTopics { get; set; } = new();

    public List<string> ActualTopics { get; set; } = new();

    public List<string> MissingTopics { get; set; } = new();

    public List<string> UnexpectedTopics { get; set; } = new();

    public List<string> ForbiddenHits { get; set; } = new();

    public bool ExactMatch { get; set; }

    public double Recall { get; set; }

    public double Precision { get; set; }

    public int ForbiddenHitCount => ForbiddenHits.Count;
}

public sealed class RunArtifact
{
    public string CaseId { get; set; } = "";

    public string PromptPath { get; set; } = "";

    public string RequestPath { get; set; } = "";

    public string ResponsePath { get; set; } = "";

    public string MetaPath { get; set; } = "";

    public string InjectedRulesPath { get; set; } = "";

    public ApiCallResult Result { get; set; } = new();

    public TopicScoreResult Score { get; set; } = new();
}
