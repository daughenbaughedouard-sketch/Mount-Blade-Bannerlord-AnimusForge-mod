param(
    [string]$IndexPath = "",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($IndexPath)) {
    $IndexPath = Join-Path $ScriptRoot "runs\material_high_value_allow_low_20260705.index.jsonl"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = [System.IO.Path]::ChangeExtension($IndexPath, ".analysis.md")
}

$JsonOutputPath = [System.IO.Path]::ChangeExtension($OutputPath, ".json")
$Utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false

function Get-TagFamily {
    param([string]$Tag)
    $text = if ($null -eq $Tag) { "" } else { $Tag.Trim() }
    if ($text -match "^\[ACTION:MOOD:([^\]]+)\]$") { return "MOOD:" + $Matches[1] }
    if ($text -eq "[ACTION:DUEL]") { return "DUEL" }
    if ($text -match "^\[AD;") { return "AD" }
    if ($text -match "^\[ADP;") { return "ADP" }
    if ($text -match "^\[ACTION:DUEL_LINE_WIN:") { return "DUEL_LINE_WIN" }
    if ($text -match "^\[ACTION:DUEL_LINE_LOSE:") { return "DUEL_LINE_LOSE" }
    if ($text -match "^\[ACTION:GIVE_GOLD:") { return "GIVE_GOLD" }
    if ($text -match "^\[ACTION:GIVE_ITEM:") { return "GIVE_ITEM" }
    if ($text -match "^\[ATT:") { return "ATT" }
    if ($text -match "^\[ATP:") { return "ATP" }
    if ($text -match "^\[ACTION:SETTLEMENT_TRANSFER:TO_PLAYER:") { return "SETTLEMENT_TRANSFER:TO_PLAYER" }
    if ($text -match "^\[ACTION:DIPLOMACY:([^:]+):") { return "DIPLOMACY:" + $Matches[1] }
    if ($text -match "^\[ACTION:KINGDOM_ANNEX:") { return "KINGDOM_ANNEX" }
    if ($text -match "^\[ACTION:VASSALAGE:SUBMIT:([^:]+):") { return "VASSALAGE:SUBMIT:" + $Matches[1] }
    if ($text -match "^\[ACTION:KINGDOM_SERVICE:([^:]+):") { return "KINGDOM_SERVICE:" + $Matches[1] }
    if ($text -match "^\[ACTION:OPEN_LORDS_HALL\]$") { return "OPEN_LORDS_HALL" }
    if ($text -match "^\[ACTION:MARRIAGE_FORMAL:") { return "MARRIAGE_FORMAL" }
    if ($text -match "^\[ACTION:MARRIAGE_ELOPE:") { return "MARRIAGE_ELOPE" }
    if ($text -match "^\[ACTION:DIVORCE:") { return "DIVORCE" }
    if ($text -match "^\[ACTION:SCENE_FOLLOW_PLAYER\]$") { return "SCENE_FOLLOW_PLAYER" }
    if ($text -match "^\[ACTION:SCENE_STOP_FOLLOW\]$") { return "SCENE_STOP_FOLLOW" }
    if ($text -match "^\[ACTION:SCENE_SUMMON:") { return "SCENE_SUMMON" }
    if ($text -match "^\[ACTION:SCENE_GUIDE:") { return "SCENE_GUIDE" }
    if ($text -eq "[END]") { return "END" }
    if ($text -match "^\[ACTION:ISSUE_ACCEPT_SELF\]$") { return "ISSUE_ACCEPT_SELF" }
    if ($text -match "^\[ACTION:ISSUE_ACCEPT_ALT:") { return "ISSUE_ACCEPT_ALT" }
    if ($text -match "^\[ACTION:QUEST_TURN_IN\]$") { return "QUEST_TURN_IN" }
    if ($text -eq "[ACTION:LET_PLAYER_GO]") { return "LET_PLAYER_GO" }
    if ($text -eq "[A:H_J_P_P]") { return "H_J_P_P" }
    if ($text -match "^\[ACTION:VOTE_DEAL:") { return "VOTE_DEAL" }
    if ($text -match "^\[ACTION:PROPOSE:([^:]+):") { return "PROPOSE:" + $Matches[1] }
    if ($text -match "^\[ACTION:WORLDMAP_ORDER:([^:\]]+)") { return "WORLDMAP_ORDER:" + $Matches[1] }
    if ($text -match "^\[ACTION:NOBLE_GATHERING:") { return "NOBLE_GATHERING" }
    if ($text -match "^\[RELAY:") { return "RELAY" }
    if ($text -eq "[ACTION:NPC_SURRENDER]") { return "NPC_SURRENDER" }
    if ($text -eq "[ACTION:KING_ABDICATE_TO_PLAYER]") { return "KING_ABDICATE_TO_PLAYER" }
    if ($text -match "^\[ACTION:[^\x00-\x7F]+\]$") { return "SIEGE_AFTER_ACTION" }
    return $text
}

function Normalize-ScoredTag {
    param([string]$Tag)

    $text = if ($null -eq $Tag) { "" } else { $Tag.Trim() }
    if ($text -match '^\[ACTION:DUEL_LINE_WIN:') {
        return "[ACTION:DUEL_LINE_WIN:*]"
    }

    if ($text -match '^\[ACTION:DUEL_LINE_LOSE:') {
        return "[ACTION:DUEL_LINE_LOSE:*]"
    }

    if ($text -match '^\[AD;(.+)\]$') {
        $parts = @($Matches[1].Split([char]';') | ForEach-Object { ([string]$_).Trim() })
        if ($parts.Count -ge 4 -and ($parts[2].Equals("N", [System.StringComparison]::OrdinalIgnoreCase) -or $parts[2].Equals("P", [System.StringComparison]::OrdinalIgnoreCase))) {
            return ("[AD;{0};{1};{2};*]" -f $parts[0], $parts[1], $parts[2].ToUpperInvariant())
        }

        if ($parts.Count -ge 3) {
            return ("[AD;{0};{1};*]" -f $parts[0], $parts[1])
        }
    }

    return $text
}

function Normalize-ScoredTags {
    param([object[]]$Tags)
    return @($Tags | ForEach-Object { Normalize-ScoredTag ([string]$_) } | Select-Object -Unique)
}

function Get-PrimaryRule {
    param([object]$Item)
    if ($null -ne $Item.caseId -and ([string]$Item.caseId) -match "^material_(.+)_\d+$") {
        return $Matches[1]
    }

    return "unknown"
}

function Add-Count {
    param([hashtable]$Map, [string]$Key, [int]$Value = 1)
    if (-not $Map.ContainsKey($Key)) {
        $Map[$Key] = 0
    }

    $Map[$Key] += $Value
}

function Is-MoodTag {
    param([string]$Tag)
    $text = if ($null -eq $Tag) { "" } else { $Tag.Trim() }
    return $text -match "^\[ACTION:MOOD:"
}

function Is-MoodFamily {
    param([string]$Family)
    $text = if ($null -eq $Family) { "" } else { $Family.Trim() }
    return $text.StartsWith("MOOD:", [StringComparison]::Ordinal)
}

if (-not (Test-Path -LiteralPath $IndexPath)) {
    throw "Index file not found: $IndexPath"
}

$items = New-Object System.Collections.Generic.List[object]
foreach ($line in Get-Content -LiteralPath $IndexPath -Encoding UTF8) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    $items.Add(($line | ConvertFrom-Json))
}

$itemArray = @($items.ToArray())
$ruleStats = @{}
$hitCountStats = @{}
$hitKeyStats = @{}
$missingFamilyCounts = @{}
$unexpectedFamilyCounts = @{}
$missingActionFamilyCounts = @{}
$unexpectedActionFamilyCounts = @{}
$mismatchExamples = New-Object System.Collections.Generic.List[object]
$familyMatchCount = 0
$actionFamilyMatchCount = 0
$actionExactMatchCount = 0
$expectedActionFamilyTotal = 0
$hitActionFamilyTotal = 0
$actualActionFamilyTotal = 0
$falseActionFamilyTotal = 0
$moodMatchCount = 0
$total = $itemArray.Count
$exactTagMatchCount = 0
$noActionExpectedTotal = 0
$noActionOverTriggeredCount = 0
$unexpectedActionCaseCount = 0

foreach ($item in $itemArray) {
    $rule = Get-PrimaryRule $item
    $preprocessHits = @($item.preprocessHits | ForEach-Object { ([string]$_).Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $hitCount = if ($null -ne $item.hitCount) { [int]$item.hitCount } else { $preprocessHits.Count }
    $hitKey = if (-not [string]::IsNullOrWhiteSpace([string]$item.hitKey)) {
        [string]$item.hitKey
    }
    elseif ($preprocessHits.Count -gt 0) {
        (@($preprocessHits | Sort-Object) -join " + ")
    }
    else {
        "(unknown)"
    }

    if (-not $ruleStats.ContainsKey($rule)) {
        $ruleStats[$rule] = [ordered]@{
            total = 0
            actionFamilyMatch = 0
            actionExactMatch = 0
            familyMatch = 0
            exactMatch = 0
            moodMatch = 0
        }
    }

    $hitCountKey = [string]$hitCount
    if (-not $hitCountStats.ContainsKey($hitCountKey)) {
        $hitCountStats[$hitCountKey] = [ordered]@{
            total = 0
            actionFamilyMatch = 0
            actionExactMatch = 0
            familyMatch = 0
            exactMatch = 0
            moodMatch = 0
        }
    }

    if (-not $hitKeyStats.ContainsKey($hitKey)) {
        $hitKeyStats[$hitKey] = [ordered]@{
            total = 0
            actionFamilyMatch = 0
            actionExactMatch = 0
            familyMatch = 0
            exactMatch = 0
            moodMatch = 0
        }
    }

    $expectedFamilies = @($item.expectedTags | ForEach-Object { Get-TagFamily ([string]$_) })
    $actualFamilies = @($item.actualTags | ForEach-Object { Get-TagFamily ([string]$_) })
    $missingFamilies = @($expectedFamilies | Where-Object { $actualFamilies -notcontains $_ } | Select-Object -Unique)
    $unexpectedFamilies = @($actualFamilies | Where-Object { $expectedFamilies -notcontains $_ } | Select-Object -Unique)
    $familyMatch = ($missingFamilies.Count -eq 0 -and $unexpectedFamilies.Count -eq 0)
    $expectedScoredTags = if ($item.PSObject.Properties.Name -contains "normalizedExpectedTags") {
        @($item.normalizedExpectedTags)
    }
    else {
        @(Normalize-ScoredTags $item.expectedTags)
    }
    $actualScoredTags = if ($item.PSObject.Properties.Name -contains "normalizedActualTags") {
        @($item.normalizedActualTags)
    }
    else {
        @(Normalize-ScoredTags $item.actualTags)
    }
    $missingScoredTags = @($expectedScoredTags | Where-Object { $actualScoredTags -notcontains $_ })
    $unexpectedScoredTags = @($actualScoredTags | Where-Object { $expectedScoredTags -notcontains $_ })
    $scoredExactMatch = ($missingScoredTags.Count -eq 0 -and $unexpectedScoredTags.Count -eq 0)
    $expectedActionTags = @(Normalize-ScoredTags @($item.expectedTags | Where-Object { -not (Is-MoodTag ([string]$_)) }))
    $actualActionTags = @(Normalize-ScoredTags @($item.actualTags | Where-Object { -not (Is-MoodTag ([string]$_)) }))
    $missingActionTags = @($expectedActionTags | Where-Object { $actualActionTags -notcontains $_ })
    $unexpectedActionTags = @($actualActionTags | Where-Object { $expectedActionTags -notcontains $_ })
    $actionExactMatch = ($missingActionTags.Count -eq 0 -and $unexpectedActionTags.Count -eq 0)
    $expectedActionFamilies = @($expectedFamilies | Where-Object { -not (Is-MoodFamily ([string]$_)) } | Select-Object -Unique)
    $actualActionFamilies = @($actualFamilies | Where-Object { -not (Is-MoodFamily ([string]$_)) } | Select-Object -Unique)
    $missingActionFamilies = @($expectedActionFamilies | Where-Object { $actualActionFamilies -notcontains $_ } | Select-Object -Unique)
    $unexpectedActionFamilies = @($actualActionFamilies | Where-Object { $expectedActionFamilies -notcontains $_ } | Select-Object -Unique)
    $actionFamilyMatch = ($missingActionFamilies.Count -eq 0 -and $unexpectedActionFamilies.Count -eq 0)
    $expectedHasAction = ($expectedActionFamilies.Count -gt 0 -or $expectedActionTags.Count -gt 0)
    $actualHasAction = ($actualActionFamilies.Count -gt 0 -or $actualActionTags.Count -gt 0)
    if (-not $expectedHasAction) {
        $noActionExpectedTotal++
        if ($actualHasAction) {
            $noActionOverTriggeredCount++
        }
    }

    if ($unexpectedActionFamilies.Count -gt 0 -or $unexpectedActionTags.Count -gt 0) {
        $unexpectedActionCaseCount++
    }

    $expectedMood = @($expectedFamilies | Where-Object { $_.StartsWith("MOOD:", [StringComparison]::Ordinal) } | Select-Object -First 1)
    $actualMood = @($actualFamilies | Where-Object { $_.StartsWith("MOOD:", [StringComparison]::Ordinal) } | Select-Object -First 1)
    $moodMatch = ($expectedMood.Count -gt 0 -and $actualMood.Count -gt 0 -and $expectedMood[0] -eq $actualMood[0])

    $ruleStats[$rule].total++
    if ($actionFamilyMatch) {
        $ruleStats[$rule].actionFamilyMatch++
        $hitCountStats[$hitCountKey].actionFamilyMatch++
        $hitKeyStats[$hitKey].actionFamilyMatch++
        $actionFamilyMatchCount++
    }

    if ($actionExactMatch) {
        $ruleStats[$rule].actionExactMatch++
        $hitCountStats[$hitCountKey].actionExactMatch++
        $hitKeyStats[$hitKey].actionExactMatch++
        $actionExactMatchCount++
    }

    if ($familyMatch) {
        $ruleStats[$rule].familyMatch++
        $hitCountStats[$hitCountKey].familyMatch++
        $hitKeyStats[$hitKey].familyMatch++
        $familyMatchCount++
    }

    if ($scoredExactMatch) {
        $ruleStats[$rule].exactMatch++
        $hitCountStats[$hitCountKey].exactMatch++
        $hitKeyStats[$hitKey].exactMatch++
        $exactTagMatchCount++
    }

    if ($moodMatch) {
        $ruleStats[$rule].moodMatch++
        $hitCountStats[$hitCountKey].moodMatch++
        $hitKeyStats[$hitKey].moodMatch++
        $moodMatchCount++
    }

    $hitCountStats[$hitCountKey].total++
    $hitKeyStats[$hitKey].total++

    foreach ($family in $missingFamilies) {
        Add-Count $missingFamilyCounts $family
    }

    foreach ($family in $unexpectedFamilies) {
        Add-Count $unexpectedFamilyCounts $family
    }

    foreach ($family in $missingActionFamilies) {
        Add-Count $missingActionFamilyCounts $family
    }

    foreach ($family in $unexpectedActionFamilies) {
        Add-Count $unexpectedActionFamilyCounts $family
    }

    $expectedActionFamilyTotal += $expectedActionFamilies.Count
    $actualActionFamilyTotal += $actualActionFamilies.Count
    $hitActionFamilyTotal += @($expectedActionFamilies | Where-Object { $actualActionFamilies -contains $_ }).Count
    $falseActionFamilyTotal += @($actualActionFamilies | Where-Object { $expectedActionFamilies -notcontains $_ }).Count

    if (-not $familyMatch -and $mismatchExamples.Count -lt 40) {
        $mismatchExamples.Add([ordered]@{
            caseId = [string]$item.caseId
            rule = $rule
            missingFamilies = @($missingFamilies)
            unexpectedFamilies = @($unexpectedFamilies)
            missingActionFamilies = @($missingActionFamilies)
            unexpectedActionFamilies = @($unexpectedActionFamilies)
            responsePath = [string]$item.responsePath
            requestPath = [string]$item.requestPath
            metaPath = [string]$item.metaPath
        })
    }
}

$ruleRows = @(
    foreach ($key in ($ruleStats.Keys | Sort-Object)) {
        $row = $ruleStats[$key]
        [ordered]@{
            rule = $key
            total = $row.total
            actionFamilyMatch = $row.actionFamilyMatch
            actionFamilyMatchRate = if ($row.total -gt 0) { [Math]::Round(($row.actionFamilyMatch * 100.0 / $row.total), 1) } else { 0 }
            actionExactMatch = $row.actionExactMatch
            familyMatch = $row.familyMatch
            familyMatchRate = if ($row.total -gt 0) { [Math]::Round(($row.familyMatch * 100.0 / $row.total), 1) } else { 0 }
            exactMatch = $row.exactMatch
            moodMatch = $row.moodMatch
        }
    }
)

$hitCountRows = @(
    foreach ($key in ($hitCountStats.Keys | Sort-Object { [int]$_ })) {
        $row = $hitCountStats[$key]
        [ordered]@{
            hitCount = [int]$key
            total = $row.total
            actionFamilyMatch = $row.actionFamilyMatch
            actionFamilyMatchRate = if ($row.total -gt 0) { [Math]::Round(($row.actionFamilyMatch * 100.0 / $row.total), 1) } else { 0 }
            actionExactMatch = $row.actionExactMatch
            familyMatch = $row.familyMatch
            familyMatchRate = if ($row.total -gt 0) { [Math]::Round(($row.familyMatch * 100.0 / $row.total), 1) } else { 0 }
            exactMatch = $row.exactMatch
            moodMatch = $row.moodMatch
        }
    }
)

$hitKeyRows = @(
    foreach ($key in ($hitKeyStats.Keys | Sort-Object { -$hitKeyStats[$_].total }, { $_ })) {
        $row = $hitKeyStats[$key]
        [ordered]@{
            hitKey = $key
            total = $row.total
            actionFamilyMatch = $row.actionFamilyMatch
            actionFamilyMatchRate = if ($row.total -gt 0) { [Math]::Round(($row.actionFamilyMatch * 100.0 / $row.total), 1) } else { 0 }
            actionExactMatch = $row.actionExactMatch
            familyMatch = $row.familyMatch
            familyMatchRate = if ($row.total -gt 0) { [Math]::Round(($row.familyMatch * 100.0 / $row.total), 1) } else { 0 }
            exactMatch = $row.exactMatch
            moodMatch = $row.moodMatch
        }
    }
)

$missingRows = @(
    foreach ($key in ($missingFamilyCounts.Keys | Sort-Object { -$missingFamilyCounts[$_] }, { $_ })) {
        [ordered]@{ family = $key; count = $missingFamilyCounts[$key] }
    }
)

$unexpectedRows = @(
    foreach ($key in ($unexpectedFamilyCounts.Keys | Sort-Object { -$unexpectedFamilyCounts[$_] }, { $_ })) {
        [ordered]@{ family = $key; count = $unexpectedFamilyCounts[$key] }
    }
)

$missingActionRows = @(
    foreach ($key in ($missingActionFamilyCounts.Keys | Sort-Object { -$missingActionFamilyCounts[$_] }, { $_ })) {
        [ordered]@{ family = $key; count = $missingActionFamilyCounts[$key] }
    }
)

$unexpectedActionRows = @(
    foreach ($key in ($unexpectedActionFamilyCounts.Keys | Sort-Object { -$unexpectedActionFamilyCounts[$_] }, { $_ })) {
        [ordered]@{ family = $key; count = $unexpectedActionFamilyCounts[$key] }
    }
)

$analysis = [ordered]@{
    indexPath = $IndexPath
    scoringNote = "ACTION:DUEL_LINE_WIN/LOSE free text and AD remark text are ignored for exact/action exact scoring."
    total = $total
    actionExactMatch = $actionExactMatchCount
    actionFamilyMatch = $actionFamilyMatchCount
    actionFamilyRecall = if ($expectedActionFamilyTotal -gt 0) { [Math]::Round(($hitActionFamilyTotal * 100.0 / $expectedActionFamilyTotal), 1) } else { 0 }
    actionFamilyPrecision = if ($actualActionFamilyTotal -gt 0) { [Math]::Round((($actualActionFamilyTotal - $falseActionFamilyTotal) * 100.0 / $actualActionFamilyTotal), 1) } else { 0 }
    expectedActionFamilies = $expectedActionFamilyTotal
    hitActionFamilies = $hitActionFamilyTotal
    falseActionFamilies = $falseActionFamilyTotal
    unexpectedActionCases = $unexpectedActionCaseCount
    noActionExpected = $noActionExpectedTotal
    noActionOverTriggered = $noActionOverTriggeredCount
    noActionOverTriggerRate = if ($noActionExpectedTotal -gt 0) { [Math]::Round(($noActionOverTriggeredCount * 100.0 / $noActionExpectedTotal), 1) } else { 0 }
    exactTagMatch = $exactTagMatchCount
    familyMatch = $familyMatchCount
    moodMatch = $moodMatchCount
    ruleStats = @($ruleRows)
    hitCountStats = @($hitCountRows)
    hitKeyStats = @($hitKeyRows)
    missingActionFamilies = @($missingActionRows)
    unexpectedActionFamilies = @($unexpectedActionRows)
    missingFamilies = @($missingRows)
    unexpectedFamilies = @($unexpectedRows)
    mismatchExamples = @($mismatchExamples.ToArray())
}

$md = New-Object System.Text.StringBuilder
[void]$md.AppendLine("# Prompt Lab Run Analysis")
[void]$md.AppendLine("")
[void]$md.AppendLine("- Index: " + $IndexPath)
[void]$md.AppendLine("- Scoring: ignores ACTION:DUEL_LINE_WIN/LOSE text and AD remark text.")
[void]$md.AppendLine("- Total: " + $total)
[void]$md.AppendLine("- Action exact match: " + $actionExactMatchCount + " / " + $total)
[void]$md.AppendLine("- Action family match: " + $actionFamilyMatchCount + " / " + $total)
[void]$md.AppendLine("- Action family recall: " + $analysis.actionFamilyRecall + "%")
[void]$md.AppendLine("- Action family precision: " + $analysis.actionFamilyPrecision + "%")
[void]$md.AppendLine("- Unexpected action cases: " + $unexpectedActionCaseCount + " / " + $total)
[void]$md.AppendLine("- No-action expected cases: " + $noActionExpectedTotal)
[void]$md.AppendLine("- No-action over-triggered: " + $noActionOverTriggeredCount + " / " + $noActionExpectedTotal + " (" + $analysis.noActionOverTriggerRate + "%)")
[void]$md.AppendLine("- Exact tag match: " + $analysis.exactTagMatch + " / " + $total)
[void]$md.AppendLine("- Family match: " + $familyMatchCount + " / " + $total)
[void]$md.AppendLine("- Mood match: " + $moodMatchCount + " / " + $total)
[void]$md.AppendLine("")
[void]$md.AppendLine("## Rule Stats")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Rule | Total | Action Family | Action Rate | Action Exact | Family Match | Rate | Exact | Mood |")
[void]$md.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |")
foreach ($row in $ruleRows) {
    [void]$md.AppendLine("| $($row.rule) | $($row.total) | $($row.actionFamilyMatch) | $($row.actionFamilyMatchRate)% | $($row.actionExactMatch) | $($row.familyMatch) | $($row.familyMatchRate)% | $($row.exactMatch) | $($row.moodMatch) |")
}

[void]$md.AppendLine("")
[void]$md.AppendLine("## Hit Count Stats")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Hit Count | Total | Action Family | Action Rate | Action Exact | Family Match | Rate | Exact | Mood |")
[void]$md.AppendLine("| ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |")
foreach ($row in $hitCountRows) {
    [void]$md.AppendLine("| $($row.hitCount) | $($row.total) | $($row.actionFamilyMatch) | $($row.actionFamilyMatchRate)% | $($row.actionExactMatch) | $($row.familyMatch) | $($row.familyMatchRate)% | $($row.exactMatch) | $($row.moodMatch) |")
}

[void]$md.AppendLine("")
[void]$md.AppendLine("## Rule Mix Stats")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Rule Mix | Total | Action Family | Action Rate | Action Exact | Family Match | Rate | Exact | Mood |")
[void]$md.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |")
foreach ($row in ($hitKeyRows | Select-Object -First 40)) {
    [void]$md.AppendLine("| $($row.hitKey) | $($row.total) | $($row.actionFamilyMatch) | $($row.actionFamilyMatchRate)% | $($row.actionExactMatch) | $($row.familyMatch) | $($row.familyMatchRate)% | $($row.exactMatch) | $($row.moodMatch) |")
}

[void]$md.AppendLine("")
[void]$md.AppendLine("## Top Missing Action Families")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Family | Count |")
[void]$md.AppendLine("| --- | ---: |")
foreach ($row in ($missingActionRows | Select-Object -First 30)) {
    [void]$md.AppendLine("| $($row.family) | $($row.count) |")
}

[void]$md.AppendLine("")
[void]$md.AppendLine("## Top Unexpected Action Families")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Family | Count |")
[void]$md.AppendLine("| --- | ---: |")
foreach ($row in ($unexpectedActionRows | Select-Object -First 30)) {
    [void]$md.AppendLine("| $($row.family) | $($row.count) |")
}

[void]$md.AppendLine("")
[void]$md.AppendLine("## Top Missing Families")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Family | Count |")
[void]$md.AppendLine("| --- | ---: |")
foreach ($row in ($missingRows | Select-Object -First 30)) {
    [void]$md.AppendLine("| $($row.family) | $($row.count) |")
}

[void]$md.AppendLine("")
[void]$md.AppendLine("## Top Unexpected Families")
[void]$md.AppendLine("")
[void]$md.AppendLine("| Family | Count |")
[void]$md.AppendLine("| --- | ---: |")
foreach ($row in ($unexpectedRows | Select-Object -First 30)) {
    [void]$md.AppendLine("| $($row.family) | $($row.count) |")
}

[void]$md.AppendLine("")
[void]$md.AppendLine("## First Family Mismatches")
[void]$md.AppendLine("")
foreach ($example in $mismatchExamples) {
    [void]$md.AppendLine("- " + $example.caseId + " (" + $example.rule + ")")
    [void]$md.AppendLine("  - Missing actions: " + ((@($example.missingActionFamilies) -join ", ")))
    [void]$md.AppendLine("  - Unexpected actions: " + ((@($example.unexpectedActionFamilies) -join ", ")))
    [void]$md.AppendLine("  - Missing: " + ((@($example.missingFamilies) -join ", ")))
    [void]$md.AppendLine("  - Unexpected: " + ((@($example.unexpectedFamilies) -join ", ")))
    [void]$md.AppendLine("  - Response: " + $example.responsePath)
}

$outDir = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outDir)) {
    [System.IO.Directory]::CreateDirectory($outDir) | Out-Null
}

[System.IO.File]::WriteAllText($OutputPath, $md.ToString(), $Utf8NoBom)
[System.IO.File]::WriteAllText($JsonOutputPath, ($analysis | ConvertTo-Json -Depth 20), $Utf8NoBom)

Write-Host ("Total: {0}" -f $total)
Write-Host ("Action exact match: {0}" -f $actionExactMatchCount)
Write-Host ("Action family match: {0}" -f $actionFamilyMatchCount)
Write-Host ("Action family recall: {0}%" -f $analysis.actionFamilyRecall)
Write-Host ("Action family precision: {0}%" -f $analysis.actionFamilyPrecision)
Write-Host ("Unexpected action cases: {0}" -f $unexpectedActionCaseCount)
Write-Host ("No-action over-triggered: {0}/{1} ({2}%)" -f $noActionOverTriggeredCount, $noActionExpectedTotal, $analysis.noActionOverTriggerRate)
Write-Host ("Exact tag match: {0}" -f $analysis.exactTagMatch)
Write-Host ("Family match: {0}" -f $familyMatchCount)
Write-Host ("Mood match: {0}" -f $moodMatchCount)
Write-Host ("Analysis file: {0}" -f $OutputPath)
Write-Host ("Analysis JSON: {0}" -f $JsonOutputPath)
