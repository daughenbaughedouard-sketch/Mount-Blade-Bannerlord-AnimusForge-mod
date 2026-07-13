param(
    [string]$BasePath = "",
    [string]$TrainingRoot = "",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($BasePath)) {
    $BasePath = Join-Path $ScriptRoot "cases\material_high_value_allow_cases.jsonl"
}

if ([string]::IsNullOrWhiteSpace($TrainingRoot)) {
    $TrainingRoot = Join-Path $ScriptRoot "dist\训练集\20260705"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $ScriptRoot "cases\material_v38_human_200.jsonl"
}

$SummaryPath = [System.IO.Path]::ChangeExtension($OutputPath, ".summary.json")
$Utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false
$Utf8Lenient = New-Object System.Text.UTF8Encoding -ArgumentList @($false, $false)

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

    return $value.Substring(0, [Math]::Max(0, $MaxLength - 12)).TrimEnd() + "`n...(已截断)"
}

function Get-Section {
    param([string]$Text, [string]$Start, [string]$End)
    $match = [regex]::Match($Text, "(?s)" + [regex]::Escape($Start) + "\s*(.*?)\s*" + [regex]::Escape($End))
    if ($match.Success) {
        return Normalize-Text $match.Groups[1].Value
    }

    return ""
}

function Extract-Tags {
    param([string]$Text)
    $result = New-Object System.Collections.Generic.List[string]
    $seen = @{}
    $source = if ($null -eq $Text) { "" } else { $Text }
    foreach ($match in [regex]::Matches($source, "\[[^\[\]\r\n]+\]")) {
        $tag = $match.Value.Trim()
        if (-not $seen.ContainsKey($tag)) {
            $seen[$tag] = $true
            $result.Add($tag)
        }
    }

    return @($result.ToArray())
}

function Get-MoodTag {
    param([object]$Case)
    foreach ($tag in @($Case.expectedTags)) {
        $text = ([string]$tag).Trim()
        if ($text -match "^\[ACTION:MOOD:[^\]]+\]$") {
            return $text
        }
    }

    return "[ACTION:MOOD:NEUTRAL]"
}

function Infer-RuleId {
    param([string]$Tag)
    $text = if ($null -eq $Tag) { "" } else { $Tag.Trim() }
    if ($text -match "^\[ACTION:MOOD:") { return "mood" }
    if ($text -eq "[ACTION:DUEL]" -or $text -match "^\[ACTION:DUEL_LINE_") { return "duel" }
    if ($text -match "^\[ACTION:GIVE_ASSET:" -or $text -match "^\[(AD|ADP);") { return "reward" }
    if ($text -match "^\[(ATT|ATP):") { return "party_transfer" }
    if ($text -eq "[A:H_J_P_P]" -or $text -eq "[A:C_J_P_K]" -or $text -match "^\[A:C_J_K:" -or $text -match "^\[ACTION:KINGDOM_SERVICE:CLAN_JOIN_(PLAYER_KINGDOM|KINGDOM):") { return "hero_join_party" }
    if ($text -match "^\[A:(P_J_K_[MV]|P_L_K)\]$") { return "kingdom_service" }
    if ($text -match "^\[ACTION:KINGDOM_SERVICE:") { return "kingdom_service" }
    if ($text -match "^\[ACTION:DIPLOMACY:" -or $text -match "^\[ACTION:KINGDOM_ANNEX:") { return "diplomacy" }
    if ($text -match "^\[ACTION:VASSALAGE:") { return "kingdom_vassalage" }
    if ($text -match "^\[ACTION:AGENDA:") { return "kingdom_agenda" }
    if ($text -match "^\[ACTION:WORLDMAP_ORDER:") { return "worldmap_party_command" }
    if ($text -match "^\[ACTION:MARRIAGE_") { return "marriage" }
    if ($text -match "^\[ACTION:ISSUE_" -or $text -match "^\[ACTION:QUEST_") { return "vanilla_issue" }
    if ($text -match "^\[ACTION:SCENE_" -or $text -eq "[END]") { return "scene_mechanism_actions" }
    if ($text -match "^\[RELAY:") { return "scene_auto_group_relay" }
    if ($text -eq "[ACTION:NPC_SURRENDER]") { return "action:wilderness" }
    if ($text -eq "[ACTION:KING_ABDICATE_TO_PLAYER]") { return "action:royal" }
    return ""
}

function Extract-LatestReply {
    param([string]$UserContent)
    $latest = Get-Section $UserContent "<latest_reply>" "</latest_reply>"
    $match = [regex]::Match($latest, "(?s)玩家:\s*(.*?)\n\s*NPC:\s*(.*)$")
    if ($match.Success) {
        return @{
            Player = Collapse-Text $match.Groups[1].Value 1400
            Npc = Collapse-Text $match.Groups[2].Value 2400
        }
    }

    return @{
        Player = ""
        Npc = Collapse-Text $latest 2400
    }
}

function Extract-HistoryLines {
    param([string]$UserContent)
    $history = Get-Section $UserContent "<history>" "</history>"
    if ([string]::IsNullOrWhiteSpace($history) -or $history -eq "（无）") {
        return @()
    }

    $items = New-Object System.Collections.Generic.List[string]
    foreach ($raw in ($history -split "`n")) {
        $line = Normalize-Text $raw
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line -match "\[AFEF\s*(玩家|NPC)行为补充\]") {
            $line = $line.Substring(0, [Math]::Max(0, $line.IndexOf("[AFEF", [StringComparison]::Ordinal))).Trim()
            if ([string]::IsNullOrWhiteSpace($line)) {
                continue
            }
        }

        $items.Add((Collapse-Text $line 700))
    }

    if ($items.Count -eq 0) {
        return @()
    }

    $array = @($items.ToArray())
    return @($array[[Math]::Max(0, $array.Count - 8)..($array.Count - 1)] | Where-Object { $_ })
}

function Extract-AfefFacts {
    param([string]$UserContent)
    $facts = New-Object System.Collections.Generic.List[object]
    $seen = @{}
    $source = Get-Section $UserContent "<history>" "</history>"
    foreach ($match in [regex]::Matches($source, "\[AFEF\s*(玩家|NPC)行为补充\]\s*(.*?)(?=\s*\[AFEF|\r?\n|$)")) {
        $kind = if ($match.Groups[1].Value -eq "NPC") { "npc" } else { "player" }
        $text = Collapse-Text $match.Groups[2].Value 900
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

function Extract-RuntimeContext {
    param([string]$UserContent)

    $headers = @(
        "玩家可见装备：",
        "NPC的物品清单：",
        "玩家家族可婚配未婚成员（事实清单）：",
        "对方家族可婚配未婚成员（事实清单）：",
        "债务提示：",
        "运行时补充事实：",
        "标签表："
    )

    $pieces = New-Object System.Collections.Generic.List[string]
    foreach ($header in $headers[0..5]) {
        $section = Extract-NamedContextSection $UserContent $header @($headers | Where-Object { $_ -ne $header }) 7000
        if (-not [string]::IsNullOrWhiteSpace($section)) {
            $pieces.Add($section)
        }
    }

    if ($pieces.Count -eq 0) {
        return "（无）"
    }

    return Collapse-Text (($pieces.ToArray()) -join "`n`n") 14000
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
    if ([string]::IsNullOrWhiteSpace($value) -or $value -eq "（无）") {
        return ""
    }

    return $Header + "`n" + $value
}

function Extract-PreprocessHits {
    param([string]$UserContent)
    $tagTable = Get-Section $UserContent "标签表：" "<latest_reply>"
    $hits = Extract-Tags $tagTable | ForEach-Object { Infer-RuleId $_ } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and $_ -ne "mood" } | Select-Object -Unique
    return @($hits)
}

function Get-RequestUserContent {
    param([string]$Block)
    $lines = $Block -split "`r?`n"
    $requestIndex = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i].Trim() -eq "REQUEST_BODY:") {
            $requestIndex = $i
            break
        }
    }

    if ($requestIndex -lt 0 -or $requestIndex + 1 -ge $lines.Count) {
        throw "REQUEST_BODY not found."
    }

    $request = $lines[$requestIndex + 1].Trim() | ConvertFrom-Json
    foreach ($message in $request.messages) {
        if ([string]$message.role -eq "user") {
            return [string]$message.content
        }
    }

    throw "User message not found."
}

function Read-BlockAtLine {
    param([string]$FilePath, [int]$StartLine)
    $lineNumber = 0
    $capture = $false
    $builder = $null
    foreach ($line in [System.IO.File]::ReadLines($FilePath, $Utf8Lenient)) {
        $lineNumber++
        if ($lineNumber -eq $StartLine) {
            if ($line -notmatch "mode=action_postprocess_http") {
                throw "Selected line is not an action_postprocess_http block: ${FilePath}:$StartLine"
            }

            $capture = $true
            $builder = New-Object System.Text.StringBuilder
        }

        if (-not $capture) {
            continue
        }

        [void]$builder.AppendLine($line)
        if ($line.Trim() -eq "----") {
            return $builder.ToString()
        }
    }

    if ($capture -and $null -ne $builder) {
        return $builder.ToString()
    }

    throw "Block not found: ${FilePath}:$StartLine"
}

function New-PublicCase {
    param([object]$Case, [object[]]$ExpectedTags, [string]$Notes)
    return [ordered]@{
        caseId = [string]$Case.caseId
        title = [string]$Case.title
        preprocessHits = @($Case.preprocessHits)
        playerText = [string]$Case.playerText
        npcReplyText = [string]$Case.npcReplyText
        historyLines = @($Case.historyLines)
        afefFacts = @($Case.afefFacts)
        runtimeContext = [string]$Case.runtimeContext
        expectedTags = @($ExpectedTags)
        notes = $Notes
    }
}

function Get-SourceRuntimeContextFromNotes {
    param([string]$Notes)
    $match = [regex]::Match($Notes, "来源：(?<file>Token_Stats \(\d+\)\.txt):(?<line>\d+)")
    if (-not $match.Success) {
        return ""
    }

    $filePath = Join-Path $TrainingRoot $match.Groups["file"].Value
    if (-not (Test-Path -LiteralPath $filePath)) {
        return ""
    }

    try {
        $block = Read-BlockAtLine $filePath ([int]$match.Groups["line"].Value)
        $userContent = Get-RequestUserContent $block
        return Extract-RuntimeContext $userContent
    }
    catch {
        return ""
    }
}

$MoodOnlyIds = @(
    "material_duel_002",
    "material_reward_015",
    "material_reward_018",
    "material_reward_020",
    "material_hero_join_party_002",
    "material_party_transfer_004",
    "material_kingdom_service_004",
    "material_diplomacy_009",
    "material_hero_join_party_009",
    "material_party_transfer_006",
    "material_kingdom_service_005",
    "material_kingdom_service_009",
    "material_settlement_transfer_013",
    "material_propose_agenda_005",
    "material_hero_join_party_018",
    "material_settlement_transfer_016"
)

$CustomExpectedTags = @{
    "material_diplomacy_004" = @("[ACTION:MOOD:JOY]", "[ACTION:AGENDA:A1:O1:FULLY_PUSH]")
    "material_diplomacy_005" = @("[ACTION:MOOD:JOY]", "[ACTION:AGENDA:A1:O1:FULLY_PUSH]")
    "material_diplomacy_006" = @("[ACTION:MOOD:JOY]", "[ACTION:AGENDA:A1:O1:FULLY_PUSH]")
    "material_diplomacy_007" = @("[ACTION:MOOD:JOY]", "[ACTION:AGENDA:A1:O1:FULLY_PUSH]")
    "material_kingdom_service_003" = @("[ACTION:MOOD:DELIGHTED]", "[ACTION:KINGDOM_SERVICE:MERCENARY:empire_w]")
    "material_worldmap_party_command_014" = @("[ACTION:MOOD:JOY]", "[ACTION:WORLDMAP_ORDER:ATTACK:settlement:castle_S2:15:AI]")
}

$NegativeSelections = @(
    @{ File = "Token_Stats (100).txt"; Line = 2511; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (100).txt"; Line = 3960; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (100).txt"; Line = 4676; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (100).txt"; Line = 5395; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (100).txt"; Line = 6131; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (100).txt"; Line = 7631; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (100).txt"; Line = 8786; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (111).txt"; Line = 1055; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (111).txt"; Line = 1575; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (111).txt"; Line = 2502; Mood = "[ACTION:MOOD:ANNOYED]" },
    @{ File = "Token_Stats (111).txt"; Line = 4798; Mood = "[ACTION:MOOD:DELIGHTED]" },
    @{ File = "Token_Stats (111).txt"; Line = 5119; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (111).txt"; Line = 5445; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (111).txt"; Line = 6852; Mood = "[ACTION:MOOD:DELIGHTED]" },
    @{ File = "Token_Stats (111).txt"; Line = 10428; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (111).txt"; Line = 13037; Mood = "[ACTION:MOOD:ANNOYED]" },
    @{ File = "Token_Stats (112).txt"; Line = 10920; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (112).txt"; Line = 11521; Mood = "[ACTION:MOOD:BORED]" },
    @{ File = "Token_Stats (113).txt"; Line = 455; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (113).txt"; Line = 1007; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (113).txt"; Line = 1326; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (72).txt"; Line = 16143; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (72).txt"; Line = 16510; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (72).txt"; Line = 16842; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (72).txt"; Line = 17231; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (72).txt"; Line = 17566; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (72).txt"; Line = 18880; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (72).txt"; Line = 19195; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (72).txt"; Line = 19613; Mood = "[ACTION:MOOD:ANNOYED]" },
    @{ File = "Token_Stats (72).txt"; Line = 19951; Mood = "[ACTION:MOOD:ANNOYED]" },
    @{ File = "Token_Stats (72).txt"; Line = 20279; Mood = "[ACTION:MOOD:ANNOYED]" },
    @{ File = "Token_Stats (72).txt"; Line = 20583; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (72).txt"; Line = 22331; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (72).txt"; Line = 23423; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (72).txt"; Line = 25149; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (72).txt"; Line = 25463; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (72).txt"; Line = 25815; Mood = "[ACTION:MOOD:NEUTRAL]" },
    @{ File = "Token_Stats (72).txt"; Line = 26175; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (72).txt"; Line = 26574; Mood = "[ACTION:MOOD:JOY]" },
    @{ File = "Token_Stats (72).txt"; Line = 27618; Mood = "[ACTION:MOOD:JOY]" }
)

if (-not (Test-Path -LiteralPath $BasePath)) {
    throw "Base case file not found: $BasePath"
}

if (-not (Test-Path -LiteralPath $TrainingRoot)) {
    throw "Training root not found: $TrainingRoot"
}

$cases = New-Object System.Collections.Generic.List[object]
$manualChanged = 0
$baseRuntimeContextRepaired = 0
$moodOnlySet = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparer]::OrdinalIgnoreCase)
foreach ($id in $MoodOnlyIds) {
    [void]$moodOnlySet.Add($id)
}

foreach ($line in Get-Content -LiteralPath $BasePath -Encoding UTF8) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    $case = $line | ConvertFrom-Json
    $caseId = [string]$case.caseId
    $expected = @($case.expectedTags)
    if ($CustomExpectedTags.ContainsKey($caseId)) {
        $expected = @($CustomExpectedTags[$caseId])
        $manualChanged++
    }
    elseif ($moodOnlySet.Contains($caseId)) {
        $expected = @(Get-MoodTag $case)
        $manualChanged++
    }

    $notes = ([string]$case.notes).Trim()
    if (-not [string]::IsNullOrWhiteSpace($notes)) {
        $notes += " "
    }

    $notes += "v38人工复核答案：只按latest_reply中已明确成交/同意/执行的动作给标签；报价、反问、条件、等待玩家再同意的内容不算动作。"
    $publicCase = New-PublicCase $case $expected $notes
    $sourceRuntimeContext = Get-SourceRuntimeContextFromNotes ([string]$case.notes)
    if (-not [string]::IsNullOrWhiteSpace($sourceRuntimeContext) -and $sourceRuntimeContext -ne "（无）") {
        $publicCase.runtimeContext = $sourceRuntimeContext
        $baseRuntimeContextRepaired++
    }

    $cases.Add($publicCase)
}

if ($cases.Count -ne 160) {
    throw "Expected 160 base cases, got $($cases.Count)."
}

$negativeIndex = 0
foreach ($selection in $NegativeSelections) {
    $negativeIndex++
    $filePath = Join-Path $TrainingRoot ([string]$selection.File)
    if (-not (Test-Path -LiteralPath $filePath)) {
        throw "Training log not found: $filePath"
    }

    $block = Read-BlockAtLine $filePath ([int]$selection.Line)
    $userContent = Get-RequestUserContent $block
    $latest = Extract-LatestReply $userContent
    $hits = @(Extract-PreprocessHits $userContent)
    if ($hits.Count -eq 0) {
        $hits = @("reward")
    }

    $caseId = "material_negative_no_agreement_{0:D3}" -f $negativeIndex
    $titleText = Collapse-Text $latest.Player 36
    if ([string]::IsNullOrWhiteSpace($titleText)) {
        $titleText = Collapse-Text $latest.Npc 36
    }
    $titleText = ($titleText -replace "\s+", " ").Trim()

    $cases.Add([ordered]@{
        caseId = $caseId
        title = "反例：未成交不触发 - " + $titleText
        preprocessHits = @($hits)
        playerText = [string]$latest.Player
        npcReplyText = [string]$latest.Npc
        historyLines = @(Extract-HistoryLines $userContent)
        afefFacts = @(Extract-AfefFacts $userContent)
        runtimeContext = Extract-RuntimeContext $userContent
        expectedTags = @([string]$selection.Mood)
        notes = "v38人工反例；来源：$($selection.File):$($selection.Line)。本轮仍在询问、报价、要求证明、设置条件、拒绝或等待玩家下一步确认，动作标签期望为空。"
    })
}

if ($cases.Count -ne 200) {
    throw "Expected 200 cases, got $($cases.Count)."
}

$outDir = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outDir)) {
    [System.IO.Directory]::CreateDirectory($outDir) | Out-Null
}

$lines = New-Object System.Collections.Generic.List[string]
foreach ($case in $cases) {
    $lines.Add(($case | ConvertTo-Json -Compress -Depth 30))
}

[System.IO.File]::WriteAllText($OutputPath, (($lines -join "`n") + "`n"), $Utf8NoBom)

$noActionCases = @($cases | Where-Object { @($_.expectedTags | Where-Object { ([string]$_) -notmatch "^\[ACTION:MOOD:" }).Count -eq 0 }).Count
$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    basePath = $BasePath
    trainingRoot = $TrainingRoot
    outputPath = $OutputPath
    total = $cases.Count
    baseCases = 160
    negativeCases = $NegativeSelections.Count
    baseManualChanged = $manualChanged
    baseRuntimeContextRepaired = $baseRuntimeContextRepaired
    noActionExpectedCases = $noActionCases
    baseMoodOnlyCaseIds = @($MoodOnlyIds | Sort-Object)
    baseCustomExpectedCaseIds = @($CustomExpectedTags.Keys | Sort-Object)
    negativeSources = @($NegativeSelections | ForEach-Object { [ordered]@{ file = [string]$_.File; line = [int]$_.Line; expectedMood = [string]$_.Mood } })
    note = "expectedTags are manually curated for v38 over-trigger training; log AI outputs are not used as answers."
}

[System.IO.File]::WriteAllText($SummaryPath, ($summary | ConvertTo-Json -Depth 10), $Utf8NoBom)

Write-Host ("Wrote cases: {0}" -f $OutputPath)
Write-Host ("Total: {0}; no-action expected: {1}; base changed: {2}; base runtime repaired: {3}; negatives: {4}" -f $cases.Count, $noActionCases, $manualChanged, $baseRuntimeContextRepaired, $NegativeSelections.Count)
Write-Host ("Summary: {0}" -f $SummaryPath)
