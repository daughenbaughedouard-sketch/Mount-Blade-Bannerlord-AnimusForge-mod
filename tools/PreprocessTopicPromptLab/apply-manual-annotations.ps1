[CmdletBinding()]
param(
    [string]$InputPath = "",
    [string]$AnnotationPath = "",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($InputPath)) {
    $InputPath = Join-Path $scriptRoot "cases\training_20260705_topics_200.jsonl"
}

if ([string]::IsNullOrWhiteSpace($AnnotationPath)) {
    $AnnotationPath = Join-Path $scriptRoot "cases\training_20260705_topics_200.annotations.jsonl"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $scriptRoot "cases\training_20260705_topics_200.manual.jsonl"
}

$allRiskTopics = @(
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
$safeFillers = @("npc_recent_actions", "npc_major_actions", "noble_deference", "surroundings")

function Get-Array {
    param($Value)
    if ($null -eq $Value) {
        return @()
    }

    if ($Value -is [System.Array]) {
        return @($Value | ForEach-Object { "$_".Trim() } | Where-Object { $_ })
    }

    return @("$Value".Trim()) | Where-Object { $_ }
}

function Add-Unique {
    param([System.Collections.ArrayList]$List, [string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) {
        return
    }

    if (-not $List.Contains($Value)) {
        [void]$List.Add($Value)
    }
}

if (-not (Test-Path -LiteralPath $InputPath)) {
    throw "Input not found: $InputPath"
}

if (-not (Test-Path -LiteralPath $AnnotationPath)) {
    throw "Annotation file not found: $AnnotationPath"
}

$annotations = @{}
Get-Content -LiteralPath $AnnotationPath -Encoding UTF8 | ForEach-Object {
    $line = $_.Trim()
    if (-not $line) {
        return
    }

    $item = $line | ConvertFrom-Json
    if ([string]::IsNullOrWhiteSpace($item.caseId)) {
        throw "Annotation is missing caseId: $line"
    }

    $annotations[$item.caseId] = $item
}

$outLines = New-Object "System.Collections.Generic.List[string]"
$applied = 0
$total = 0
Get-Content -LiteralPath $InputPath -Encoding UTF8 | ForEach-Object {
    $line = $_.Trim()
    if (-not $line) {
        return
    }

    $total++
    $case = $line | ConvertFrom-Json
    if ($annotations.ContainsKey($case.caseId)) {
        $ann = $annotations[$case.caseId]
        $expected = New-Object System.Collections.ArrayList
        $allowed = New-Object System.Collections.ArrayList

        foreach ($topic in (Get-Array $ann.expectedTopics)) {
            Add-Unique $expected $topic
        }

        foreach ($topic in (Get-Array $ann.allowedExtraTopics)) {
            if (-not $expected.Contains($topic)) {
                Add-Unique $allowed $topic
            }
        }

        foreach ($topic in $safeFillers) {
            if (($expected.Count + $allowed.Count) -ge 4) {
                break
            }

            if (-not $expected.Contains($topic) -and -not $allowed.Contains($topic)) {
                Add-Unique $allowed $topic
            }
        }

        $case.expectedTopics = @($expected)
        $case.allowedExtraTopics = @($allowed)

        $acceptable = @{}
        foreach ($topic in @($expected + $allowed)) {
            $acceptable[$topic] = $true
        }

        $forbidden = New-Object System.Collections.ArrayList
        foreach ($topic in $allRiskTopics) {
            if (-not $acceptable.ContainsKey($topic)) {
                Add-Unique $forbidden $topic
            }
        }

        Add-Unique $forbidden "loan"
        $case.forbiddenTopics = @($forbidden)
        $manualNotes = ""
        if ($ann.PSObject.Properties.Name -contains "notes" -and $null -ne $ann.notes) {
            $manualNotes = "$($ann.notes)"
        }

        $case.notes = "manual_annotation_v1；Expected=人工核心必命中；Allowed=默认4条注入可接受补位；顺序不计；" + $manualNotes
        $applied++
    }

    [void]$outLines.Add(($case | ConvertTo-Json -Depth 16 -Compress))
}

$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDir)) {
    [void][System.IO.Directory]::CreateDirectory($outputDir)
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllLines($OutputPath, $outLines, $utf8NoBom)
Write-Host "cases=$total annotations=$($annotations.Count) applied=$applied output=$OutputPath"
