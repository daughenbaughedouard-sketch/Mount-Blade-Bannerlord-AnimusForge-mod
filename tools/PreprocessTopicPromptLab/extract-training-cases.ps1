[CmdletBinding()]
param(
    [string]$SourceDir = "",
    [string]$OutputPath = "",
    [int]$TargetCount = 200,
    [int]$MaxScanFiles = 0
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($SourceDir)) {
    $SourceDir = Join-Path $scriptRoot "..\ActionPostprocessPromptLab\dist\训练集\20260705"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $scriptRoot "cases\training_20260705_topics_200.jsonl"
}

$regexOptions = [System.Text.RegularExpressions.RegexOptions]::Singleline -bor [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Multiline
$script:caseTopics = @(
    "duel",
    "reward",
    "kingdom_service",
    "lords_hall_access",
    "marriage",
    "scene_mechanism_actions",
    "party_transfer",
    "settlement_transfer",
    "vanilla_issue",
    "encounter_release_player",
    "hero_join_party",
    "vote_deal",
    "propose_agenda",
    "worldmap_party_command",
    "diplomacy",
    "kingdom_vassalage",
    "noble_gathering",
    "scene_auto_group_relay",
    "siege_intervention_aftermath"
)
$script:safeFallbackTopics = @(
    "npc_recent_actions",
    "npc_major_actions",
    "noble_deference",
    "surroundings"
)

function Normalize-OneLine {
    param([string]$Text)
    if ($null -eq $Text) {
        return ""
    }

    return ([regex]::Replace($Text.Replace("`r`n", "`n").Replace("`r", "`n"), "\s+", " ")).Trim()
}

function Shorten-Text {
    param([string]$Text, [int]$MaxChars)
    $value = (Normalize-OneLine $Text)
    if ($value.Length -le $MaxChars) {
        return $value
    }

    return $value.Substring(0, [Math]::Max(0, $MaxChars)).Trim() + "..."
}

function Get-MatchGroup {
    param([string]$Text, [string]$Pattern, [int]$GroupIndex = 1)
    $match = [regex]::Match($Text, $Pattern, $script:regexOptions)
    if ($match.Success -and $match.Groups.Count -gt $GroupIndex) {
        return $match.Groups[$GroupIndex].Value.Trim()
    }

    return ""
}

function Get-LastMatchGroup {
    param([string]$Text, [string]$Pattern, [int]$GroupIndex = 1)
    $matches = [regex]::Matches($Text, $Pattern, $script:regexOptions)
    if ($matches.Count -gt 0) {
        $match = $matches[$matches.Count - 1]
        if ($match.Success -and $match.Groups.Count -gt $GroupIndex) {
            return $match.Groups[$GroupIndex].Value.Trim()
        }
    }

    return ""
}

function Test-AnyPattern {
    param([string]$Text, [string[]]$Patterns)
    foreach ($pattern in $Patterns) {
        if ($Text -match $pattern) {
            return $true
        }
    }

    return $false
}

function Test-HistoryCarry {
    param([string]$Latest)
    return Test-AnyPattern $Latest @(
        "^\\s*(是|是的|好|好吧|可以|行|同意|没问题|成交)\\s*$",
        "就按",
        "照你说",
        "如你所说",
        "我答应",
        "照.*(刚才|之前|约定|计划)",
        "按.*(刚才|之前|约定|计划|说好)",
        "刚才说的",
        "之前说的",
        "说好的",
        "谈好的",
        "照办",
        "就这样",
        "继续.*(刚才|之前|计划)",
        "履行",
        "兑现",
        "按约",
        "依约",
        "承诺",
        "那件事",
        "这件事"
    )
}

function Add-Unique {
    param([System.Collections.Generic.List[string]]$Values, [string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) {
        return
    }

    if (-not $Values.Contains($Value)) {
        [void]$Values.Add($Value)
    }
}

function Has-TopicTag {
    param([string]$TagTable, [string]$Topic)
    switch ($Topic) {
        "duel" { return $TagTable -match "\[ACTION:DUEL\]|DUEL_LINE" }
        "reward" { return $TagTable -match "GIVE_GOLD|GIVE_ITEM|\[AD;|\[ADP;" }
        "kingdom_service" { return $TagTable -match "KINGDOM_SERVICE:(LEAVE|MERCENARY|VASSAL)" }
        "lords_hall_access" { return $TagTable -match "OPEN_LORDS_HALL" }
        "marriage" { return $TagTable -match "MARRIAGE|DIVORCE" }
        "scene_mechanism_actions" { return $TagTable -match "SCENE_FOLLOW|SCENE_STOP|SCENE_SUMMON|SCENE_GUIDE|\[END\]" }
        "party_transfer" { return $TagTable -match "\[ATT:|\[ATP:" }
        "settlement_transfer" { return $TagTable -match "SETTLEMENT_TRANSFER" }
        "vanilla_issue" { return $TagTable -match "ISSUE_ACCEPT|QUEST_TURN_IN" }
        "encounter_release_player" { return $TagTable -match "LET_PLAYER_GO" }
        "hero_join_party" { return $TagTable -match "H_J_P_P|CLAN_JOIN_PLAYER_KINGDOM" }
        "vote_deal" { return $TagTable -match "VOTE_DEAL" }
        "propose_agenda" { return $TagTable -match "PROPOSE:" }
        "worldmap_party_command" { return $TagTable -match "WORLDMAP_ORDER" }
        "diplomacy" { return $TagTable -match "DIPLOMACY|KINGDOM_ANNEX" }
        "kingdom_vassalage" { return $TagTable -match "VASSALAGE:SUBMIT" }
        "noble_gathering" { return $TagTable -match "NOBLE_GATHERING" }
        "scene_auto_group_relay" { return $TagTable -match "\[RELAY:" }
        "siege_intervention_aftermath" { return $TagTable -match "\[ACTION:(宽恕|救济|宣抚|盟誓|安兵|召集|抢钱|搜掠|血洗|殖民)\]" }
        default { return $false }
    }
}

function Get-TagTableTopics {
    param([string]$TagTable)
    $topics = New-Object "System.Collections.Generic.List[string]"
    foreach ($topic in $script:caseTopics) {
        if (Has-TopicTag $TagTable $topic) {
            Add-Unique $topics $topic
        }
    }

    return $topics
}

function Test-WeakTopicSignal {
    param([string]$Topic, [string]$Text, [string]$Runtime)
    switch ($Topic) {
        "duel" { return Test-AnyPattern $Text @("决斗", "单挑", "比武", "打一场", "打一架", "一决胜负", "挑战你") }
        "reward" { return Test-AnyPattern ($Text + "`n" + $Runtime) @("第纳尔", "金币", "钱", "债", "欠", "还钱", "报酬", "赏金", "彩礼", "交易", "给", "支付", "物品", "装备", "买", "卖") }
        "kingdom_service" { return Test-AnyPattern $Text @("雇佣兵", "封臣", "效忠", "加入.*王国", "离开.*王国", "脱离.*王国") }
        "lords_hall_access" { return Test-AnyPattern $Text @("领主大厅", "城堡大厅", "进大厅", "进堡", "通行") }
        "marriage" { return Test-AnyPattern ($Text + "`n" + $Runtime) @("婚", "联姻", "提亲", "嫁", "娶", "求婚", "私奔", "离婚", "彩礼") }
        "scene_mechanism_actions" { return Test-AnyPattern $Text @("带路", "带我去", "跟着我", "跟我来", "停止跟随", "叫.*过来", "召唤", "传唤", "引路") }
        "party_transfer" { return (Test-AnyPattern $Text @("给(我|你).{0,16}(士兵|部队|俘虏|兵员|人手|勇士|新兵)", "(士兵|部队|俘虏|兵员|人手|勇士|新兵).{0,16}(给我|给你|交给|转交|调拨|拨给|移交|带走)", "招募.{0,12}(勇士|士兵|部队)")) -and -not (Test-AnyPattern $Text @("资源.{0,8}重建部队", "重建部队")) }
        "settlement_transfer" { return (Test-AnyPattern $Text @("领地", "城堡", "城市", "村庄", "封地", "定居点", "工坊", "商队")) -and (Test-AnyPattern $Text @("转让", "移交", "割让", "归我", "归你", "所有权")) }
        "vanilla_issue" { return Test-AnyPattern $Text @("任务", "委托", "差事", "交任务", "完成任务") }
        "encounter_release_player" { return Test-AnyPattern $Text @("放我走", "让我离开", "释放我", "放行", "饶我") }
        "hero_join_party" { return Test-AnyPattern $Text @("加入我的队伍", "加入我们", "跟随我", "随我同行", "来我队里", "为我效力", "入队") }
        "vote_deal" { return Test-AnyPattern $Text @("投票", "议案", "议程", "影响力", "支持.*选项", "反对.*选项") }
        "propose_agenda" { return Test-AnyPattern $Text @("提出.*议案", "提交.*议程", "发起.*投票", "王国政策", "法律提案", "提案") }
        "worldmap_party_command" { return Test-AnyPattern $Text @("大地图", "带兵.*前往", "部队.*前往", "前往", "驻守", "驻扎", "巡逻", "攻击", "攻打", "追击", "护送", "合并", "停止行动") }
        "diplomacy" { return Test-AnyPattern $Text @("宣战", "停战", "和平", "议和", "结盟", "同盟", "贸易协定", "断交", "吞并", "战争", "贡金", "赔款") }
        "kingdom_vassalage" { return Test-AnyPattern $Text @("臣服", "附庸", "朝贡", "纳贡", "宗主", "保护国", "卫戍", "称臣") }
        "noble_gathering" { return Test-AnyPattern $Text @("宴会", "聚会", "酒会", "邀请.*贵族", "贵族集会", "设宴") }
        "scene_auto_group_relay" { return Test-AnyPattern $Text @("大家(?!族)", "诸位", "你们", "在场", "其他人", "轮流", "接话", "表态", "各位") }
        "siege_intervention_aftermath" { return Test-AnyPattern $Text @("宽恕", "救济", "宣抚", "盟誓", "安兵", "抢钱", "搜掠", "血洗", "殖民", "攻城后", "城破") }
        default { return $false }
    }
}

function Get-ExpectedTopics {
    param([string]$Player, [string]$Npc, [string]$History, [string]$Runtime, [string]$TagTable)

    $topics = New-Object "System.Collections.Generic.List[string]"
    $latest = $Player
    $text = $latest
    if (Test-HistoryCarry $latest) {
        $text = "$latest`n$History"
    }

    $duel = Test-AnyPattern $text @("决斗", "单挑", "比武", "打一场", "打一架", "一决胜负", "挑战你", "接受挑战", "赌斗", "角斗", "刀剑说话")
    if ($duel) {
        Add-Unique $topics "duel"
    }

    $siege = Test-AnyPattern $text @("宽恕", "救济", "宣抚", "盟誓", "安兵", "召集", "抢钱", "搜掠", "血洗", "殖民", "攻城后", "占领后", "城破", "战后处置", "战后处理")
    if ($siege -and ((Has-TopicTag $TagTable "siege_intervention_aftermath") -or (Test-AnyPattern $latest @("宽恕", "救济", "宣抚", "盟誓", "安兵", "搜掠", "血洗", "殖民")))) {
        Add-Unique $topics "siege_intervention_aftermath"
    }

    $reward = Test-AnyPattern $text @("给你", "给我", "交给", "拿去", "支付", "付给", "付款", "还钱", "欠", "债", "赊", "借", "贷", "抵押", "报酬", "赏金", "酬劳", "第纳尔", "金币", "钱", "买", "卖", "交易", "交换", "赠", "赔偿", "赎金", "请客", "喝一杯", "酒钱", "装备", "物品", "粮食", "马匹", "武器", "盔甲")
    $duelOnlyDebt = $duel -and -not (Test-AnyPattern $text @("GIVE_GOLD", "GIVE_ITEM", "ADP", "立即给", "现在给", "先给", "把.*给你", "把.*给我"))
    if ($reward -and -not $duelOnlyDebt -and -not ($siege -and -not (Has-TopicTag $TagTable "reward"))) {
        Add-Unique $topics "reward"
    }

    if (Test-AnyPattern $text @("家族.*加入.*王国", "家族.*效忠", "家族.*随.*旗帜", "全族.*效忠", "全族.*加入", "全族.*随", "加入我的王国", "加入我们的王国", "加入玩家王国", "加入我方王国", "归顺我的王国", "投靠我的王国", "随你的旗帜", "随我旗帜")) {
        Add-Unique $topics "hero_join_party"
    } elseif (Test-AnyPattern $text @("加入我的队伍", "加入我们", "加入我[^的王国]", "跟随我", "随我同行", "来我队里", "为我效力", "成为我的同伴", "入队", "做我的伙伴")) {
        Add-Unique $topics "hero_join_party"
    }

    if (Test-AnyPattern $text @("成为.*雇佣兵", "雇佣兵", "成为.*封臣", "宣誓效忠", "效忠.*王国", "加入.*王国", "离开.*王国", "脱离.*王国", "不再效忠", "退臣")) {
        if (-not (Test-AnyPattern $text @("家族.*加入.*王国", "家族.*随.*旗帜", "全族.*加入", "加入我的王国", "加入我们的王国", "玩家王国", "随你的旗帜", "随我旗帜", "向我宣誓", "效忠于我", "向我效忠"))) {
            Add-Unique $topics "kingdom_service"
        }
    }

    if (Test-AnyPattern $text @("臣服", "附庸", "朝贡", "纳贡", "宗主", "保护国", "卫戍", "驻军换保护", "向.*称臣")) {
        Add-Unique $topics "kingdom_vassalage"
    }

    $assetMention = Test-AnyPattern $text @("领地", "城堡", "城市", "村庄", "封地", "定居点", "工坊", "商队", "贸易车队")
    $assetTransfer = Test-AnyPattern $text @("转让", "移交", "割让", "归我", "归你", "给我.*(城|堡|领地|封地|工坊|商队)", "给你.*(城|堡|领地|封地|工坊|商队)", "把.*(城|堡|领地|封地|工坊|商队).*给", "所有权", "地契")
    if ($assetMention -and $assetTransfer) {
        Add-Unique $topics "settlement_transfer"
    }

    $partyTransfer = (Test-AnyPattern $text @("给(我|你).{0,16}(士兵|部队|俘虏|兵员|人手|勇士|新兵|亲卫)", "(士兵|部队|俘虏|兵员|人手|勇士|新兵|亲卫).{0,16}(给我|给你|交给|转交|调拨|拨给|移交|带走|收下)", "招募.{0,12}(勇士|士兵|部队)")) -and -not (Test-AnyPattern $text @("资源.{0,8}重建部队", "重建部队"))
    if ($partyTransfer) {
        Add-Unique $topics "party_transfer"
    }

    $sceneMove = Test-AnyPattern $text @("带路", "带我去", "跟着我", "跟我来", "别跟", "停止跟随", "叫.*过来", "喊.*来", "召唤", "传唤", "引路", "领路")
    if ($sceneMove -and ((Has-TopicTag $TagTable "scene_mechanism_actions") -or -not (Test-AnyPattern $text @("大地图", "队伍", "军队", "部队.*前往", "巡逻", "攻打")))) {
        Add-Unique $topics "scene_mechanism_actions"
    }

    $worldMove = Test-AnyPattern $text @("大地图", "带兵.*前往", "军队.*前往", "部队.*前往", "队伍.*前往", "前往", "驻守", "驻扎", "巡逻", "跟随我的队伍", "跟随我方部队", "攻击", "攻打", "追击", "护送", "合并", "并入", "停下行动", "停止行动", "创建部队", "建一支部队")
    if ($worldMove -and -not ($sceneMove -and (Has-TopicTag $TagTable "scene_mechanism_actions"))) {
        Add-Unique $topics "worldmap_party_command"
    }

    if (Test-AnyPattern $text @("宣战", "停战", "和平", "议和", "结盟", "同盟", "贸易协定", "断交", "吞并", "战争", "贡金", "赔款", "王国谈判")) {
        Add-Unique $topics "diplomacy"
    }

    if (Test-AnyPattern $text @("投票", "议案投票", "议程投票", "支持.*选项", "反对.*选项", "影响力", "贵族会议投票")) {
        Add-Unique $topics "vote_deal"
    }

    if (Test-AnyPattern $text @("提出.*议案", "提交.*议程", "发起.*投票", "拿到议会", "王国政策", "法律提案", "提案")) {
        Add-Unique $topics "propose_agenda"
    }

    if (Test-AnyPattern $text @("婚", "联姻", "提亲", "嫁给", "娶", "求婚", "私奔", "离婚", "婚约", "成亲")) {
        Add-Unique $topics "marriage"
    }

    if (Test-AnyPattern $text @("原版任务", "任务", "委托", "差事", "交任务", "完成任务", "接下这个活", "帮你跑一趟")) {
        if ((Has-TopicTag $TagTable "vanilla_issue") -or (Test-AnyPattern $latest @("任务", "委托", "交任务", "完成任务"))) {
            Add-Unique $topics "vanilla_issue"
        }
    }

    if (Test-AnyPattern $text @("放我走", "放我们走", "让我离开", "释放我", "放行", "不杀我", "别抓我", "饶我一命")) {
        Add-Unique $topics "encounter_release_player"
    }

    if (Test-AnyPattern $text @("领主大厅", "城堡大厅", "进大厅", "进堡", "见领主", "让我进去", "放我进去", "请求通行")) {
        Add-Unique $topics "lords_hall_access"
    }

    if (Test-AnyPattern $text @("宴会", "聚会", "酒会", "邀请.*贵族", "召集.*贵族", "贵族集会", "设宴")) {
        Add-Unique $topics "noble_gathering"
    }

    if (Test-AnyPattern $latest @("大家(?!族)", "诸位", "你们", "在场", "其他人", "轮流", "接话", "让.*说", "问问.*看法", "听听.*意见", "谁.*回答", "表态", "众人", "各位")) {
        if ((Has-TopicTag $TagTable "scene_auto_group_relay") -or (Test-AnyPattern $latest @("大家(?!族)", "诸位", "各位", "其他人", "轮流", "接话", "表态"))) {
            Add-Unique $topics "scene_auto_group_relay"
        }
    }

    return $topics
}

function Get-AllowedTopics {
    param([System.Collections.Generic.List[string]]$Expected, [string]$Text, [string]$TagTable)
    $allowed = New-Object "System.Collections.Generic.List[string]"

    if ($Expected.Contains("diplomacy") -and (Test-AnyPattern $Text @("领地", "城堡", "割让", "移交"))) {
        Add-Unique $allowed "settlement_transfer"
    }

    if ($Expected.Contains("settlement_transfer") -and (Test-AnyPattern $Text @("和平", "议和", "停战", "宣战"))) {
        Add-Unique $allowed "diplomacy"
    }

    if ($Expected.Contains("worldmap_party_command") -and (Test-AnyPattern $Text @("加入我的队伍", "跟随我", "随我同行"))) {
        Add-Unique $allowed "hero_join_party"
    }

    if ($Expected.Contains("hero_join_party") -and (Test-AnyPattern $Text @("带兵", "大地图", "队伍跟随"))) {
        Add-Unique $allowed "worldmap_party_command"
    }

    if ($Expected.Contains("scene_mechanism_actions") -and (Test-AnyPattern $Text @("大家(?!族)", "诸位", "在场", "其他人"))) {
        Add-Unique $allowed "scene_auto_group_relay"
    }

    if ($Expected.Contains("duel") -and (Test-AnyPattern $Text @("立即给", "现在给", "先给", "付给", "支付"))) {
        Add-Unique $allowed "reward"
    }

    return $allowed
}

function Ensure-FourTopicAllowance {
    param(
        [System.Collections.Generic.List[string]]$Expected,
        [System.Collections.Generic.List[string]]$Allowed,
        [string]$Text,
        [string]$Runtime,
        [string]$TagTable
    )

    while ($Expected.Count -gt 4) {
        $Expected.RemoveAt($Expected.Count - 1)
    }

    foreach ($topic in @(Get-TagTableTopics $TagTable)) {
        if (($Expected.Count + $Allowed.Count) -ge 4) {
            return
        }

        if ($Expected.Contains($topic) -or $Allowed.Contains($topic)) {
            continue
        }

        if (Test-WeakTopicSignal $topic $Text $Runtime) {
            Add-Unique $Allowed $topic
        }
    }

    foreach ($topic in $script:safeFallbackTopics) {
        if (($Expected.Count + $Allowed.Count) -ge 4) {
            return
        }

        if ($Expected.Contains($topic) -or $Allowed.Contains($topic)) {
            continue
        }

        if ($topic -eq "surroundings" -and -not (Test-AnyPattern ($Text + "`n" + $Runtime) @("附近", "哪里", "地点", "城镇", "村庄", "当前地点", "定居点", "周边"))) {
            continue
        }

        if ($topic -eq "noble_deference" -and -not (Test-AnyPattern $Text @("领主", "贵族", "国王", "女王", "可汗", "陛下", "殿下", "头衔", "身份", "家族", "族长"))) {
            continue
        }

        if ($topic -eq "npc_recent_actions" -and -not (Test-AnyPattern $Text @("刚才", "之前", "已经", "记得", "承诺", "过往", "历史", "上次"))) {
            continue
        }

        Add-Unique $Allowed $topic
    }

    foreach ($topic in $script:safeFallbackTopics) {
        if (($Expected.Count + $Allowed.Count) -ge 4) {
            return
        }

        if (-not $Expected.Contains($topic) -and -not $Allowed.Contains($topic)) {
            Add-Unique $Allowed $topic
        }
    }
}

function Get-ForbiddenTopics {
    param([System.Collections.Generic.List[string]]$Expected, [System.Collections.Generic.List[string]]$Allowed, [string]$Text)
    $forbidden = New-Object "System.Collections.Generic.List[string]"
    foreach ($topic in $script:caseTopics) {
        if ($Expected.Contains($topic) -or $Allowed.Contains($topic)) {
            continue
        }

        if ($topic -eq "reward" -and (Test-AnyPattern $Text @("第纳尔", "金币", "欠", "债", "交易", "给", "支付"))) {
            continue
        }

        if ($topic -eq "scene_auto_group_relay" -and (Test-AnyPattern $Text @("大家(?!族)", "诸位", "各位", "其他人"))) {
            continue
        }

        Add-Unique $forbidden $topic
        if ($forbidden.Count -ge 8) {
            break
        }
    }

    if (-not $Expected.Contains("reward") -and -not $Allowed.Contains("reward") -and (Test-AnyPattern $Text @("欠", "债", "借", "还钱", "第纳尔", "金币"))) {
        Add-Unique $forbidden "reward"
    }

    Add-Unique $forbidden "loan"
    return $forbidden
}

function Get-AfefFacts {
    param([string]$History, [string]$Runtime)
    $facts = New-Object "System.Collections.Generic.List[object]"
    $seen = New-Object "System.Collections.Generic.HashSet[string]" ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($line in (($History + "`n" + $Runtime).Replace("`r`n", "`n").Replace("`r", "`n") -split "`n")) {
        $text = $line.Trim()
        if ($text -match "\[AFEF玩家行为补充\]\s*(.+)$") {
            $value = (Shorten-Text $Matches[1] 260)
            if ($seen.Add("player|" + $value)) {
                [void]$facts.Add([pscustomobject]@{ Kind = "player"; Text = $value })
            }
        } elseif ($text -match "\[AFEF NPC行为补充\]\s*(.+)$") {
            $value = (Shorten-Text $Matches[1] 260)
            if ($seen.Add("npc|" + $value)) {
                [void]$facts.Add([pscustomobject]@{ Kind = "npc"; Text = $value })
            }
        }

        if ($facts.Count -ge 10) {
            break
        }
    }

    return $facts
}

function Get-HistoryLines {
    param([string]$History)
    $lines = New-Object "System.Collections.Generic.List[string]"
    foreach ($line in ($History.Replace("`r`n", "`n").Replace("`r", "`n") -split "`n")) {
        $text = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($text)) {
            continue
        }

        if ($text -match "^\(无\)$|AFEF|^标签表|^运行时补充|^<|^>") {
            continue
        }

        [void]$lines.Add((Shorten-Text $text 260))
    }

    while ($lines.Count -gt 14) {
        $lines.RemoveAt(0)
    }

    return $lines
}

function Get-RuntimeText {
    param([string]$Runtime)
    $selected = New-Object "System.Collections.Generic.List[string]"
    $all = New-Object "System.Collections.Generic.List[string]"
    foreach ($line in ($Runtime.Replace("`r`n", "`n").Replace("`r", "`n") -split "`n")) {
        $text = $line.Trim()
        if ([string]::IsNullOrWhiteSpace($text) -or $text -match "AFEF") {
            continue
        }

        $short = Shorten-Text $text 280
        [void]$all.Add($short)
        if ($text -match "候选|清单|可|当前|地点|人物|目标|债务|定居点|部队|俘虏|家族|王国|领地|工坊|商队|议程|投票|婚|宴会|接力|编号|释放|大厅|召唤|带路|跟随|战争|和平|贡金|附庸") {
            [void]$selected.Add($short)
        }
    }

    $source = if ($selected.Count -gt 0) { $selected } else { $all }
    $out = New-Object "System.Collections.Generic.List[string]"
    foreach ($line in $source) {
        [void]$out.Add($line)
        if ($out.Count -ge 22) {
            break
        }
    }

    $joined = ($out -join "`n").Trim()
    if ($joined.Length -gt 1800) {
        return $joined.Substring(0, 1800).Trim() + "..."
    }

    return $joined
}

function New-CaseFromBlock {
    param([string[]]$Lines, [string]$SourceName, [int]$StartLine)

    $text = $Lines -join "`n"
    if ($text -notmatch "mode=action_postprocess_http" -or $text -notmatch "<latest_reply>") {
        return $null
    }

    $history = Get-LastMatchGroup $text "^\s*<history>\s*(.*?)^\s*</history>"
    $runtime = Get-LastMatchGroup $text "^\s*运行时补充事实[:：]\s*(.*?)^\s*标签表[:：]"
    $tagTable = Get-LastMatchGroup $text "^\s*标签表[:：]\s*(.*?)^\s*<latest_reply>"
    $latest = Get-LastMatchGroup $text "^\s*<latest_reply>\s*(.*?)^\s*</latest_reply>"
    if ([string]::IsNullOrWhiteSpace($latest)) {
        return $null
    }

    $latestMatch = [regex]::Match($latest, "^\s*玩家[:：]\s*(.*?)^\s*NPC[:：]\s*(.*)$", $script:regexOptions)
    if (-not $latestMatch.Success) {
        return $null
    }

    $player = Shorten-Text $latestMatch.Groups[1].Value 900
    $npc = Shorten-Text $latestMatch.Groups[2].Value 1200
    if ([string]::IsNullOrWhiteSpace($player)) {
        return $null
    }

    $expected = New-Object "System.Collections.Generic.List[string]"
    foreach ($topic in @(Get-ExpectedTopics $player $npc $history $runtime $tagTable)) {
        Add-Unique $expected $topic
    }

    if ($expected.Count -eq 0) {
        return $null
    }

    $latestText = $player
    $allText = $latestText
    if (Test-HistoryCarry $latestText) {
        $allText = "$latestText`n$history"
    }
    $allowed = New-Object "System.Collections.Generic.List[string]"
    foreach ($topic in @(Get-AllowedTopics $expected $allText $tagTable)) {
        Add-Unique $allowed $topic
    }
    Ensure-FourTopicAllowance $expected $allowed $allText $runtime $tagTable

    $forbidden = New-Object "System.Collections.Generic.List[string]"
    foreach ($topic in @(Get-ForbiddenTopics $expected $allowed $allText)) {
        Add-Unique $forbidden $topic
    }

    $historyLines = Get-HistoryLines $history
    $afefFacts = Get-AfefFacts $history $runtime
    $runtimeText = Get-RuntimeText $runtime
    $primary = $expected[0]
    $preview = Shorten-Text $player 34

    return [pscustomobject]@{
        CaseId = ""
        Title = "$primary - $preview"
        PlayerText = $player
        NpcReplyText = $npc
        HistoryLines = @($historyLines)
        AfefFacts = @($afefFacts)
        RuntimeContext = $runtimeText
        ExpectedTopics = @($expected.ToArray())
        AllowedExtraTopics = @($allowed.ToArray())
        ForbiddenTopics = @($forbidden.ToArray())
        Notes = "source=${SourceName}:$StartLine；Expected=核心必命中；Allowed=游戏默认4条注入的可接受补位；顺序不计。期望话题按对话语义和后处理标签注入关系生成；未采用日志 rule_codes、mentioned_entities 或 AI 输出 token。债务/借贷统一归 reward。"
    }
}

function Get-CaseScore {
    param($Case)
    $score = 0
    $score += @($Case.ExpectedTopics).Count * 4
    if (@($Case.AfefFacts).Count -gt 0) { $score += 3 }
    if (@($Case.HistoryLines).Count -gt 0) { $score += 2 }
    if (-not [string]::IsNullOrWhiteSpace($Case.RuntimeContext)) { $score += 2 }
    if ($Case.PlayerText.Length -ge 12) { $score += 1 }
    if (@($Case.ForbiddenTopics).Count -gt 0) { $score += 1 }
    return $score
}

function Select-BalancedCases {
    param([object[]]$Candidates, [int]$Target)
    $buckets = @{}
    foreach ($candidate in $Candidates) {
        $primary = @($candidate.ExpectedTopics)[0]
        if (-not $buckets.ContainsKey($primary)) {
            $buckets[$primary] = New-Object "System.Collections.Generic.List[object]"
        }

        [void]$buckets[$primary].Add($candidate)
    }

    foreach ($key in @($buckets.Keys)) {
        $itemsForSort = @($buckets[$key].ToArray())
        $buckets[$key] = @($itemsForSort | Sort-Object @{ Expression = { Get-CaseScore $_ }; Descending = $true }, @{ Expression = { $_.PlayerText.Length }; Descending = $true })
    }

    $selected = New-Object "System.Collections.Generic.List[object]"
    $topicOrder = @($buckets.Keys | Sort-Object)
    $index = 0
    while ($selected.Count -lt $Target) {
        $added = $false
        foreach ($topic in $topicOrder) {
            $items = @($buckets[$topic])
            if ($index -lt $items.Count) {
                [void]$selected.Add($items[$index])
                $added = $true
                if ($selected.Count -ge $Target) {
                    break
                }
            }
        }

        if (-not $added) {
            break
        }

        $index++
    }

    return @($selected.ToArray())
}

if (-not (Test-Path -LiteralPath $SourceDir)) {
    throw "Source directory not found: $SourceDir"
}

$files = @(Get-ChildItem -LiteralPath $SourceDir -File -Filter "*.txt" | Sort-Object Length)
if ($MaxScanFiles -gt 0) {
    $files = @($files | Select-Object -First $MaxScanFiles)
}

$candidates = New-Object "System.Collections.Generic.List[object]"
$seen = New-Object "System.Collections.Generic.HashSet[string]" ([System.StringComparer]::OrdinalIgnoreCase)
$totalBlocks = 0
$fileIndex = 0

foreach ($file in $files) {
    $fileIndex++
    Write-Host ("[{0}/{1}] scan {2:n1} MB {3}" -f $fileIndex, $files.Count, ($file.Length / 1MB), $file.Name)
    $stream = [System.IO.File]::Open($file.FullName, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
    try {
        $reader = New-Object System.IO.StreamReader($stream, (New-Object System.Text.UTF8Encoding($false, $true)), $true)
        try {
            $inBlock = $false
            $block = New-Object "System.Collections.Generic.List[string]"
            $lineNo = 0
            $startLine = 0
            while (-not $reader.EndOfStream) {
                $line = $reader.ReadLine()
                $lineNo++
                if ($line -match "mode=action_postprocess_http") {
                    $inBlock = $true
                    $block.Clear()
                    $startLine = $lineNo
                }

                if ($inBlock) {
                    [void]$block.Add($line)
                    if ($line -eq "----") {
                        $totalBlocks++
                        $case = New-CaseFromBlock -Lines @($block) -SourceName $file.Name -StartLine $startLine
                        if ($null -ne $case) {
                            $key = (Normalize-OneLine ($case.PlayerText + "|" + $case.NpcReplyText + "|" + (@($case.ExpectedTopics) -join ",")))
                            if ($seen.Add($key)) {
                                [void]$candidates.Add($case)
                            }
                        }

                        $inBlock = $false
                        $block.Clear()
                    }
                }
            }

            if ($inBlock -and $block.Count -gt 0) {
                $totalBlocks++
                $case = New-CaseFromBlock -Lines @($block) -SourceName $file.Name -StartLine $startLine
                if ($null -ne $case) {
                    $key = (Normalize-OneLine ($case.PlayerText + "|" + $case.NpcReplyText + "|" + (@($case.ExpectedTopics) -join ",")))
                    if ($seen.Add($key)) {
                        [void]$candidates.Add($case)
                    }
                }
            }
        } finally {
            $reader.Dispose()
        }
    } finally {
        $stream.Dispose()
    }
}

$selected = @(Select-BalancedCases -Candidates $candidates.ToArray() -Target $TargetCount)
if ($selected.Count -lt $TargetCount) {
    Write-Warning "Only generated $($selected.Count) cases from $($candidates.Count) candidates."
}

$lines = New-Object "System.Collections.Generic.List[string]"
for ($i = 0; $i -lt $selected.Count; $i++) {
    $case = $selected[$i]
    $case.CaseId = "training_20260705_{0:000}" -f ($i + 1)
    $json = $case | ConvertTo-Json -Depth 16 -Compress
    [void]$lines.Add($json)
}

$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDir)) {
    [void][System.IO.Directory]::CreateDirectory($outputDir)
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllLines($OutputPath, $lines, $utf8NoBom)

$distribution = @($selected | ForEach-Object { @($_.ExpectedTopics)[0] } | Group-Object | Sort-Object Name | ForEach-Object {
    [pscustomobject]@{ Topic = $_.Name; Count = $_.Count }
})
$summaryPath = [System.IO.Path]::ChangeExtension($OutputPath, ".summary.json")
$summary = [pscustomobject]@{
    SourceDir = (Resolve-Path -LiteralPath $SourceDir).Path
    OutputPath = (Resolve-Path -LiteralPath $OutputPath).Path
    TargetCount = $TargetCount
    TotalBlocks = $totalBlocks
    CandidateCount = $candidates.Count
    OutputCount = $selected.Count
    Distribution = $distribution
}
[System.IO.File]::WriteAllText($summaryPath, ($summary | ConvertTo-Json -Depth 8), $utf8NoBom)

Write-Host "blocks=$totalBlocks candidates=$($candidates.Count) output=$($selected.Count)"
Write-Host "wrote $OutputPath"
Write-Host "summary $summaryPath"
$distribution | Format-Table -AutoSize
