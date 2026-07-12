param(
    [string]$MaterialRoot = "",
    [string]$OutputPath = "",
    [int]$MaxCases = 160,
    [int]$MaxPerRule = 24
)

$ErrorActionPreference = "Stop"

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

function Decode-Utf8Base64 {
    param([string]$Value)
    return [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($Value))
}

if ([string]::IsNullOrWhiteSpace($MaterialRoot)) {
    $MaterialRoot = Join-Path (Join-Path $ScriptRoot "dist") (Decode-Utf8Base64 "57Sg5p2Q")
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $ScriptRoot "cases\material_high_value_allow_cases.jsonl"
}

$SummaryPath = [System.IO.Path]::ChangeExtension($OutputPath, ".summary.json")
$Utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false
$Utf8Lenient = New-Object System.Text.UTF8Encoding -ArgumentList @($false, $false)

$RuleLabels = @{
    "duel" = "duel"
    "reward" = "reward/debt"
    "kingdom_service" = "kingdom service"
    "lords_hall_access" = "lords hall access"
    "marriage" = "marriage"
    "scene_mechanism_actions" = "scene movement"
    "party_transfer" = "party transfer"
    "settlement_transfer" = "settlement transfer"
    "vanilla_issue" = "vanilla issue"
    "encounter_release_player" = "release player"
    "hero_join_party" = "hero join party"
    "vote_deal" = "vote deal"
    "propose_agenda" = "propose agenda"
    "worldmap_party_command" = "worldmap command"
    "diplomacy" = "diplomacy"
    "kingdom_vassalage" = "vassalage"
    "noble_gathering" = "noble gathering"
    "scene_auto_group_relay" = "scene relay"
    "siege_intervention_aftermath" = "siege aftermath"
    "action:wilderness" = "wilderness action"
    "action:royal" = "royal action"
}

$ExplicitTextPatterns = @(
    "6Zi06IyO", "6IKJ5qOS", "6b6f5aS0", "6IKJ56m0", "6Zi06YGT",
    "6Zi06JKC", "5rer5rC0", "57K+5ray", "5bCE57K+", "5Lmz5aS0",
    "5Lmz5oi/", "5aW25a2Q", "5oCn5Lqk", "5oCn54ix", "5Y+j5Lqk",
    "6IKb5Lqk", "5by65aW4", "5Lmx5Lym", "5pON5L2g", "5bmy5L2g",
    "6KKr5pON", "6LCD5pWZ", "6Ieq5oWw", "5rer6I2h", "5LiL5L2T",
    "5Lqr55So", "6Lqr5L2T5LiL",
    "56eB5Lq65oiY5Yip5ZOB", "5pqn5pin", "6IiM5bCW", "5Zi05ZSH",
    "56GV5aSn", "6IOv5LiL", "5oyR6YCX", "6aWx5ruh",
    "5oy65ouU", "6IKJ5L2T", "6LWk5p2h5p2h", "5oul5pyJ5oiR"
) | ForEach-Object { Decode-Utf8Base64 $_ }

$PlayerWord = Decode-Utf8Base64 "546p5a62"
$NpcWord = "NPC"
$AfefSuffix = Decode-Utf8Base64 "6KGM5Li66KGl5YWF"
$NoText = Decode-Utf8Base64 "77yI5peg77yJ"
$RuntimeHeader = Decode-Utf8Base64 "6L+Q6KGM5pe26KGl5YWF5LqL5a6e77ya"
$TagTableHeader = Decode-Utf8Base64 "5qCH562+6KGo77ya"
$PlayerVisibleEquipmentHeader = Decode-Utf8Base64 "546p5a625Y+v6KeB6KOF5aSH77ya"
$NpcItemListHeader = Decode-Utf8Base64 "TlBD55qE54mp5ZOB5riF5Y2V77ya"
$MarriagePlayerHeader = Decode-Utf8Base64 "546p5a625a625peP5Y+v5ama6YWN5pyq5ama5oiQ5ZGY77yI5LqL5a6e5riF5Y2V77yJ77ya"
$MarriageTargetHeader = Decode-Utf8Base64 "5a+55pa55a625peP5Y+v5ama6YWN5pyq5ama5oiQ5ZGY77yI5LqL5a6e5riF5Y2V77yJ77ya"
$DebtHintHeader = Decode-Utf8Base64 "5YC65Yqh5o+Q56S677ya"
$TruncatedText = Decode-Utf8Base64 "5bey5oiq5pat77yM5Y6f5pel5b+X5pu06ZW/"
$NotesPrefix = Decode-Utf8Base64 "5LuO57Sg5p2Q5pel5b+X6Ieq5Yqo5o+Q5Y+W77ybZXhwZWN0ZWRUYWdzIOS4uuWOn+aXpeW/lyBhY3Rpb25fcG9zdHByb2Nlc3NfaHR0cCDovpPlh7rvvIzlkI7nu63pnIDkurrlt6XlpI3moLjjgILmnaXmupDvvJo="
$ConsentSignals = @(
    "5ZCM5oSP", "5o6l5Y+X", "5oS/5oSP", "5Y+v5Lul", "5YeG6K64",
    "5YWB6K64", "5oiQ5Lqk", "57uZ5L2g", "5Lqk57uZ5L2g", "5b2S6aG6",
    "6Iej5pyN", "5oqV6ZmN", "6YeK5pS+", "5Yqg5YWl", "6Lef6ZqP",
    "5bim5L2g", "5Y+s6ZuG", "6K6p57uZ5L2g", "5oiQ5Li65L2g55qE",
    "546w5Zyo5bCx"
) | ForEach-Object { Decode-Utf8Base64 $_ }
$RuntimeSignals = @(
    "57yW5Y+3", "5YCZ6YCJ", "5riF5Y2V", "5YC65Yqh", "5bqT5a2Y",
    "54mp5ZOB", "5L+Y6JmP", "6YOo6Zif", "6K6u56iL", "6YCJ6aG5"
) | ForEach-Object { Decode-Utf8Base64 $_ }

function Normalize-Text {
    param([string]$Text)
    if ($null -eq $Text) {
        return ""
    }

    return (($Text -replace "`r", "") -replace "[`t ]+", " ").Trim()
}

function Collapse-Text {
    param([string]$Text, [int]$MaxLength)
    $value = Normalize-Text $Text
    if ($value.Length -le $MaxLength) {
        return $value
    }

    return $value.Substring(0, [Math]::Max(0, $MaxLength - 24)).TrimEnd() + "`n...(" + $TruncatedText + ")"
}

function Has-ExplicitText {
    param([string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $false
    }

    foreach ($pattern in $ExplicitTextPatterns) {
        if ($Text.IndexOf($pattern, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            return $true
        }
    }

    return $false
}

function Test-ContainsAny {
    param([string]$Text, [string[]]$Needles)
    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $false
    }

    foreach ($needle in $Needles) {
        if (-not [string]::IsNullOrWhiteSpace($needle) -and $Text.IndexOf($needle, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            return $true
        }
    }

    return $false
}

function Get-Section {
    param([string]$Text, [string]$StartPattern, [string]$EndPattern)
    $pattern = "(?s)" + $StartPattern + "\s*(.*?)\s*" + $EndPattern
    $match = [regex]::Match($Text, $pattern)
    if ($match.Success) {
        return (Normalize-Text $match.Groups[1].Value)
    }

    return ""
}

function Extract-LatestReply {
    param([string]$UserContent)
    $latest = Get-Section $UserContent "<latest_reply>" "</latest_reply>"
    if ([string]::IsNullOrWhiteSpace($latest)) {
        return @{ Player = ""; Npc = "" }
    }

    $pattern = "(?s)" + [regex]::Escape($PlayerWord) + ":\s*(.*?)\r?\n\s*" + [regex]::Escape($NpcWord) + ":\s*(.*)$"
    $match = [regex]::Match($latest, $pattern)
    if (-not $match.Success) {
        return @{ Player = ""; Npc = $latest }
    }

    return @{
        Player = Collapse-Text $match.Groups[1].Value 900
        Npc = Collapse-Text $match.Groups[2].Value 1600
    }
}

function Extract-HistoryLines {
    param([string]$UserContent)
    $history = Get-Section $UserContent "<history>" "</history>"
    if ([string]::IsNullOrWhiteSpace($history) -or $history -eq $NoText) {
        return @()
    }

    $lines = New-Object System.Collections.Generic.List[string]
    $afefLinePattern = "\[AFEF\s*(" + [regex]::Escape($PlayerWord) + "|" + [regex]::Escape($NpcWord) + ")" + [regex]::Escape($AfefSuffix) + "\]"
    foreach ($raw in ($history -split "`n")) {
        $line = Normalize-Text $raw
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line -match $afefLinePattern) {
            $afefIndex = $line.IndexOf("[AFEF", [StringComparison]::OrdinalIgnoreCase)
            if ($afefIndex -lt 0) {
                continue
            }

            $line = $line.Substring(0, $afefIndex).Trim()
            $line = [regex]::Replace($line, "\s*\u3010[^\u3011]*\u3011\s*$", "").Trim()
            $line = [regex]::Replace($line, "\s*\u3010[^\u3011]*$", "").Trim()
            if ([string]::IsNullOrWhiteSpace($line)) {
                continue
            }
        }

        $lines.Add((Collapse-Text $line 500))
    }

    if ($lines.Count -eq 0) {
        return @()
    }

    $start = [Math]::Max(0, $lines.Count - 8)
    return @($lines.ToArray()[$start..($lines.Count - 1)] | Where-Object { $_ })
}

function Extract-AfefFacts {
    param([string]$UserContent)
    $facts = New-Object System.Collections.Generic.List[object]
    $seen = @{}

    $afefPattern = "\[AFEF\s*(" + [regex]::Escape($PlayerWord) + "|" + [regex]::Escape($NpcWord) + ")" + [regex]::Escape($AfefSuffix) + "\]\s*(.*?)(?=(?:\s*\u3010[^\u3011]*\u3011)?\s*\[AFEF|\r?\n|$)"
    foreach ($match in [regex]::Matches($UserContent, $afefPattern)) {
        $kind = if ($match.Groups[1].Value -eq $NpcWord) { "npc" } else { "player" }
        $text = Collapse-Text $match.Groups[2].Value 700
        $text = [regex]::Replace($text, "\s*\u3010[^\u3011]*\u3011\s*$", "").Trim()
        $text = [regex]::Replace($text, "\s*\u3010[^\u3011]*$", "").Trim()
        if ([string]::IsNullOrWhiteSpace($text)) {
            continue
        }

        $key = $kind + "|" + $text
        if ($seen.ContainsKey($key)) {
            continue
        }

        $seen[$key] = $true
        $facts.Add([ordered]@{
            kind = $kind
            text = $text
        })
    }

    return @($facts.ToArray())
}

function Extract-NamedContextSection {
    param([string]$UserContent, [string]$Header, [string[]]$NextHeaders, [int]$MaxLength)
    if ([string]::IsNullOrWhiteSpace($UserContent) -or [string]::IsNullOrWhiteSpace($Header)) {
        return ""
    }

    $start = $UserContent.IndexOf($Header, [StringComparison]::Ordinal)
    if ($start -lt 0) {
        return ""
    }

    $contentStart = $start + $Header.Length
    $end = $UserContent.Length
    foreach ($next in $NextHeaders) {
        if ([string]::IsNullOrWhiteSpace($next)) {
            continue
        }

        $idx = $UserContent.IndexOf($next, $contentStart, [StringComparison]::Ordinal)
        if ($idx -ge 0 -and $idx -lt $end) {
            $end = $idx
        }
    }

    if ($end -le $contentStart) {
        return ""
    }

    $value = Collapse-Text ($UserContent.Substring($contentStart, $end - $contentStart)) $MaxLength
    if ([string]::IsNullOrWhiteSpace($value) -or $value -eq $NoText) {
        return ""
    }

    return $Header + "`n" + $value
}

function Extract-RuntimeContext {
    param([string]$UserContent)

    $orderedHeaders = @(
        $PlayerVisibleEquipmentHeader,
        $NpcItemListHeader,
        $MarriagePlayerHeader,
        $MarriageTargetHeader,
        $DebtHintHeader,
        $RuntimeHeader,
        $TagTableHeader
    )

    $pieces = New-Object System.Collections.Generic.List[string]
    $sections = @(
        @{ Header = $PlayerVisibleEquipmentHeader; Max = 3000 },
        @{ Header = $NpcItemListHeader; Max = 5000 },
        @{ Header = $MarriagePlayerHeader; Max = 2500 },
        @{ Header = $MarriageTargetHeader; Max = 2500 },
        @{ Header = $DebtHintHeader; Max = 2500 },
        @{ Header = $RuntimeHeader; Max = 7000 }
    )

    foreach ($section in $sections) {
        $header = [string]$section.Header
        $nextHeaders = @($orderedHeaders | Where-Object { $_ -ne $header })
        $text = Extract-NamedContextSection $UserContent $header $nextHeaders ([int]$section.Max)
        if (-not [string]::IsNullOrWhiteSpace($text)) {
            $pieces.Add($text)
        }
    }

    if ($pieces.Count -eq 0) {
        return $NoText
    }

    return Collapse-Text (($pieces.ToArray()) -join "`n`n") 14000
}

function Extract-ResponseTags {
    param([string[]]$Lines)
    $tags = New-Object System.Collections.Generic.List[string]
    $seen = @{}
    $inResponse = $false

    foreach ($line in $Lines) {
        $value = $line.Trim()
        if ($value -eq "ai_response=") {
            $inResponse = $true
            continue
        }

        if ($value.StartsWith("ai_response=", [StringComparison]::Ordinal)) {
            $inResponse = $true
            $value = $value.Substring("ai_response=".Length).Trim()
        }

        if (-not $inResponse) {
            continue
        }

        if ($value.StartsWith("raw_response=", [StringComparison]::Ordinal) -or
            $value.StartsWith("raw_response_sample=", [StringComparison]::Ordinal) -or
            $value -eq "----") {
            break
        }

        foreach ($match in [regex]::Matches($value, "\[[^\[\]\r\n]+\]")) {
            $tag = $match.Value.Trim()
            if (-not $seen.ContainsKey($tag)) {
                $seen[$tag] = $true
                $tags.Add($tag)
            }
        }
    }

    return @($tags.ToArray())
}

function Infer-RuleId {
    param([string]$Tag)
    if ($Tag -match "^\[ACTION:MOOD:") { return "mood" }
    if ($Tag -eq "[ACTION:DUEL]" -or $Tag -match "^\[ACTION:DUEL_LINE_") { return "duel" }
    if ($Tag -match "^\[ACTION:GIVE_(GOLD|ITEM):" -or $Tag -match "^\[(AD|ADP);") { return "reward" }
    if ($Tag -match "^\[ACTION:KINGDOM_SERVICE:(LEAVE|MERCENARY|VASSAL):") { return "kingdom_service" }
    if ($Tag -eq "[ACTION:OPEN_LORDS_HALL]") { return "lords_hall_access" }
    if ($Tag -match "^\[ACTION:(MARRIAGE_FORMAL|MARRIAGE_ELOPE|DIVORCE):") { return "marriage" }
    if ($Tag -match "^\[ACTION:SCENE_(FOLLOW_PLAYER|STOP_FOLLOW|SUMMON|GUIDE):" -or $Tag -match "^\[ACTION:SCENE_(FOLLOW_PLAYER|STOP_FOLLOW)\]$" -or $Tag -eq "[END]") { return "scene_mechanism_actions" }
    if ($Tag -match "^\[(ATT|ATP):") { return "party_transfer" }
    if ($Tag -match "^\[ACTION:SETTLEMENT_TRANSFER:") { return "settlement_transfer" }
    if ($Tag -match "^\[ACTION:(ISSUE_ACCEPT_SELF|ISSUE_ACCEPT_ALT|QUEST_TURN_IN)") { return "vanilla_issue" }
    if ($Tag -eq "[ACTION:LET_PLAYER_GO]") { return "encounter_release_player" }
    if ($Tag -eq "[A:H_J_P_P]" -or $Tag -match "^\[ACTION:KINGDOM_SERVICE:CLAN_JOIN_PLAYER_KINGDOM:") { return "hero_join_party" }
    if ($Tag -match "^\[ACTION:VOTE_DEAL:") { return "vote_deal" }
    if ($Tag -match "^\[ACTION:PROPOSE:") { return "propose_agenda" }
    if ($Tag -match "^\[ACTION:WORLDMAP_ORDER:") { return "worldmap_party_command" }
    if ($Tag -match "^\[ACTION:(DIPLOMACY|KINGDOM_ANNEX):") { return "diplomacy" }
    if ($Tag -match "^\[ACTION:VASSALAGE:") { return "kingdom_vassalage" }
    if ($Tag -match "^\[ACTION:NOBLE_GATHERING:") { return "noble_gathering" }
    if ($Tag -match "^\[RELAY:") { return "scene_auto_group_relay" }
    if ($Tag -match "^\[ACTION:[^\x00-\x7F]+\]$") { return "siege_intervention_aftermath" }
    if ($Tag -eq "[ACTION:NPC_SURRENDER]") { return "action:wilderness" }
    if ($Tag -eq "[ACTION:KING_ABDICATE_TO_PLAYER]") { return "action:royal" }

    return ""
}

function Get-ExpectedTagsAndRules {
    param([string[]]$Tags)
    $expected = New-Object System.Collections.Generic.List[string]
    $rules = New-Object System.Collections.Generic.List[string]
    $seenTags = @{}
    $seenRules = @{}
    $hasDuelTag = @($Tags | Where-Object { $_ -eq "[ACTION:DUEL]" -or $_ -match "^\[ACTION:DUEL_LINE_" }).Count -gt 0

    foreach ($tag in $Tags) {
        $rule = Infer-RuleId $tag
        if ($hasDuelTag -and $tag -match "^\[AD;") {
            $rule = "duel"
        }

        if ([string]::IsNullOrWhiteSpace($rule)) {
            continue
        }

        if (-not $seenTags.ContainsKey($tag)) {
            $seenTags[$tag] = $true
            $expected.Add($tag)
        }

        if ($rule -ne "mood" -and -not $seenRules.ContainsKey($rule)) {
            $seenRules[$rule] = $true
            $rules.Add($rule)
        }
    }

    return @{
        ExpectedTags = @($expected.ToArray())
        RuleIds = @($rules.ToArray())
    }
}

function Score-Case {
    param(
        [string[]]$ExpectedTags,
        [string[]]$RuleIds,
        [string]$PlayerText,
        [string]$NpcText,
        [string]$HistoryText,
        [string]$RuntimeContext,
        [object[]]$AfefFacts
    )

    $score = 0
    $nonMood = @($ExpectedTags | Where-Object { $_ -notmatch "^\[ACTION:MOOD:" })
    $score += 50 * [Math]::Min(4, $nonMood.Count)
    $score += 12 * [Math]::Min(3, $RuleIds.Count)

    $combined = ($PlayerText + "`n" + $NpcText)
    if (Test-ContainsAny $combined $ConsentSignals) {
        $score += 18
    }

    if ($RuntimeContext -match "ID|Id|id|settlement|hero|party|clan|kingdom" -or (Test-ContainsAny $RuntimeContext $RuntimeSignals)) {
        $score += 12
    }

    if ($AfefFacts.Count -gt 0) {
        $score += 10
    }

    if ($HistoryText -match ([regex]::Escape($PlayerWord) + ":|" + [regex]::Escape($NpcWord) + ":")) {
        $score += 4
    }

    if ($ExpectedTags -contains "[END]") {
        $score -= 16
    }

    return $score
}

function Make-CaseId {
    param([string]$RuleId, [int]$Index)
    $safeRule = ($RuleId -replace "[^A-Za-z0-9_:-]", "_") -replace ":", "_"
    return ("material_{0}_{1:D3}" -f $safeRule, $Index)
}

function Make-Title {
    param([string]$RuleId, [string]$PlayerText, [string[]]$Tags)
    $label = if ($RuleLabels.ContainsKey($RuleId)) { $RuleLabels[$RuleId] } else { $RuleId }
    $summary = Normalize-Text $PlayerText
    if ($summary.Length -gt 34) {
        $summary = $summary.Substring(0, 34).TrimEnd() + "..."
    }

    if ([string]::IsNullOrWhiteSpace($summary)) {
        $summary = (($Tags | Where-Object { $_ -notmatch "^\[ACTION:MOOD:" } | Select-Object -First 1) -join "")
    }

    return $label + " - " + $summary
}

function Process-Block {
    param([string]$Text, [string]$SourceFile, [int]$StartLine)
    $script:Stats.Blocks++
    $lines = $Text -split "`r?`n"
    $requestIndex = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i].Trim() -eq "REQUEST_BODY:") {
            $requestIndex = $i
            break
        }
    }

    if ($requestIndex -lt 0 -or $requestIndex + 1 -ge $lines.Count) {
        $script:Stats.SkippedNoRequest++
        return
    }

    $requestJson = $lines[$requestIndex + 1].Trim()
    if (-not $requestJson.StartsWith("{", [StringComparison]::Ordinal)) {
        $script:Stats.SkippedNoRequest++
        return
    }

    try {
        $request = $requestJson | ConvertFrom-Json
    }
    catch {
        $script:Stats.SkippedInvalidRequest++
        return
    }

    $userContent = ""
    if ($null -ne $request.messages) {
        foreach ($message in $request.messages) {
            if ([string]$message.role -eq "user") {
                $userContent = [string]$message.content
                break
            }
        }
    }

    if ([string]::IsNullOrWhiteSpace($userContent)) {
        $script:Stats.SkippedNoRequest++
        return
    }

    $rawTags = Extract-ResponseTags $lines
    if ($rawTags.Count -eq 0) {
        $script:Stats.SkippedNoTags++
        return
    }

    $tagInfo = Get-ExpectedTagsAndRules $rawTags
    $expectedTags = @($tagInfo.ExpectedTags)
    $ruleIds = @($tagInfo.RuleIds)
    if ($expectedTags.Count -eq 0 -or $ruleIds.Count -eq 0) {
        $script:Stats.SkippedUnsupportedTags++
        return
    }

    $latest = Extract-LatestReply $userContent
    $playerText = [string]$latest.Player
    $npcText = [string]$latest.Npc
    if ([string]::IsNullOrWhiteSpace($playerText) -and [string]::IsNullOrWhiteSpace($npcText)) {
        $script:Stats.SkippedNoLatestReply++
        return
    }

    $historyLines = @(Extract-HistoryLines $userContent)
    $afefFacts = @(Extract-AfefFacts $userContent)
    $runtimeContext = Extract-RuntimeContext $userContent
    $afefText = ($afefFacts | ForEach-Object { [string]$_.text }) -join "`n"
    $contentForFilter = $playerText + "`n" + $npcText + "`n" + ($historyLines -join "`n") + "`n" + $afefText + "`n" + $runtimeContext + "`n" + ($expectedTags -join "`n")
    if (Has-ExplicitText $contentForFilter) {
        $script:Stats.SkippedExplicit++
        return
    }

    $primaryRule = [string]$ruleIds[0]
    $dedupeKey = (($primaryRule + "|" + $playerText + "|" + $npcText + "|" + ($expectedTags -join "|")) -replace "\s+", " ").ToLowerInvariant()
    if ($script:Dedupe.ContainsKey($dedupeKey)) {
        $script:Stats.SkippedDuplicate++
        return
    }

    $script:Dedupe[$dedupeKey] = $true
    $historyText = $historyLines -join "`n"
    $score = Score-Case $expectedTags $ruleIds $playerText $npcText $historyText $runtimeContext $afefFacts
    $case = [ordered]@{
        _score = $score
        _primaryRule = $primaryRule
        _sourceFile = (Split-Path -Leaf $SourceFile)
        _sourceLine = $StartLine
        caseId = ""
        title = ""
        preprocessHits = @($ruleIds)
        playerText = $playerText
        npcReplyText = $npcText
        historyLines = @($historyLines)
        afefFacts = @($afefFacts)
        runtimeContext = $runtimeContext
        expectedTags = @($expectedTags)
        notes = $NotesPrefix + "$(Split-Path -Leaf $SourceFile):$StartLine."
    }

    $script:Candidates.Add($case)
    $script:Stats.AcceptedCandidates++
}

if (-not (Test-Path -LiteralPath $MaterialRoot)) {
    throw "Material directory does not exist: $MaterialRoot"
}

$script:Stats = [ordered]@{
    Files = 0
    Blocks = 0
    AcceptedCandidates = 0
    WrittenCases = 0
    SkippedNoRequest = 0
    SkippedInvalidRequest = 0
    SkippedNoTags = 0
    SkippedUnsupportedTags = 0
    SkippedNoLatestReply = 0
    SkippedExplicit = 0
    SkippedDuplicate = 0
}

$script:Candidates = New-Object System.Collections.Generic.List[object]
$script:Dedupe = @{}

$files = Get-ChildItem -LiteralPath $MaterialRoot -File -Filter "*.txt" | Sort-Object Name
foreach ($file in $files) {
    $script:Stats.Files++
    Write-Host ("Scanning {0}/{1}: {2}" -f $script:Stats.Files, $files.Count, $file.Name)

    $builder = $null
    $inBlock = $false
    $startLine = 0
    $lineNumber = 0

    foreach ($line in [System.IO.File]::ReadLines($file.FullName, $Utf8Lenient)) {
        $lineNumber++
        if ($line -match "^\[\d\d:\d\d:\d\d\].*mode=action_postprocess_http") {
            if ($inBlock -and $null -ne $builder) {
                Process-Block $builder.ToString() $file.FullName $startLine
            }

            $builder = New-Object System.Text.StringBuilder
            [void]$builder.AppendLine($line)
            $inBlock = $true
            $startLine = $lineNumber
            continue
        }

        if (-not $inBlock) {
            continue
        }

        [void]$builder.AppendLine($line)
        if ($line.Trim() -eq "----") {
            Process-Block $builder.ToString() $file.FullName $startLine
            $builder = $null
            $inBlock = $false
            $startLine = 0
        }
    }

    if ($inBlock -and $null -ne $builder) {
        Process-Block $builder.ToString() $file.FullName $startLine
    }
}

$selected = New-Object System.Collections.Generic.List[object]
$perRule = @{}
$ranked = $script:Candidates | Sort-Object @{ Expression = { $_._score }; Descending = $true }, @{ Expression = { $_._sourceFile }; Descending = $false }, @{ Expression = { $_._sourceLine }; Descending = $false }
foreach ($candidate in $ranked) {
    if ($selected.Count -ge $MaxCases) {
        break
    }

    $rule = [string]$candidate._primaryRule
    if (-not $perRule.ContainsKey($rule)) {
        $perRule[$rule] = 0
    }

    if ($perRule[$rule] -ge $MaxPerRule) {
        continue
    }

    $perRule[$rule]++
    $candidate.caseId = Make-CaseId $rule $perRule[$rule]
    $candidate.title = Make-Title $rule $candidate.playerText $candidate.expectedTags
    $selected.Add($candidate)
}

$outDir = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outDir)) {
    [System.IO.Directory]::CreateDirectory($outDir) | Out-Null
}

$linesOut = New-Object System.Collections.Generic.List[string]
foreach ($case in $selected) {
    $publicCase = [ordered]@{
        caseId = $case.caseId
        title = $case.title
        preprocessHits = @($case.preprocessHits)
        playerText = $case.playerText
        npcReplyText = $case.npcReplyText
        historyLines = @($case.historyLines)
        afefFacts = @($case.afefFacts)
        runtimeContext = $case.runtimeContext
        expectedTags = @($case.expectedTags)
        notes = $case.notes
    }
    $linesOut.Add(($publicCase | ConvertTo-Json -Compress -Depth 20))
}

[System.IO.File]::WriteAllText($OutputPath, (($linesOut -join "`n") + "`n"), $Utf8NoBom)

$ruleCounts = [ordered]@{}
foreach ($case in $selected) {
    $rule = [string]$case._primaryRule
    if (-not $ruleCounts.Contains($rule)) {
        $ruleCounts[$rule] = 0
    }

    $ruleCounts[$rule]++
}

$summary = [ordered]@{
    materialRoot = $MaterialRoot
    outputPath = $OutputPath
    generatedAt = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    stats = $script:Stats
    maxCases = $MaxCases
    maxPerRule = $MaxPerRule
    writtenCases = $selected.Count
    ruleCounts = $ruleCounts
}

$script:Stats.WrittenCases = $selected.Count
[System.IO.File]::WriteAllText($SummaryPath, ($summary | ConvertTo-Json -Depth 20), $Utf8NoBom)

Write-Host ("Candidate cases: {0}" -f $script:Stats.AcceptedCandidates)
Write-Host ("Written cases: {0}" -f $selected.Count)
Write-Host ("Case file: {0}" -f $OutputPath)
Write-Host ("Summary file: {0}" -f $SummaryPath)
