param(
    [string]$RunRoot = "",
    [string[]]$RunDirs = @(),
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($RunRoot)) {
    $RunRoot = Join-Path $ScriptRoot "runs"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $RunRoot ("run_index_" + (Get-Date).ToString("yyyyMMdd_HHmmss") + ".jsonl")
}

$SummaryPath = [System.IO.Path]::ChangeExtension($OutputPath, ".summary.json")
$Utf8NoBom = New-Object System.Text.UTF8Encoding -ArgumentList $false

function Extract-Tags {
    param([string]$Text)
    $tags = New-Object System.Collections.Generic.List[string]
    $seen = @{}
    $source = if ($null -eq $Text) { "" } else { $Text }
    foreach ($match in [regex]::Matches($source, "\[[^\[\]\r\n]+\]")) {
        $tag = $match.Value.Trim()
        if (-not $seen.ContainsKey($tag)) {
            $seen[$tag] = $true
            $tags.Add($tag)
        }
    }

    return @($tags.ToArray())
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

    if ($text -match '^\[AD:(.+)\]$') {
        $parts = @($Matches[1].Split([char]':') | ForEach-Object { ([string]$_).Trim() })
        if ($parts.Count -ge 4 -and ($parts[2].Equals("N", [System.StringComparison]::OrdinalIgnoreCase) -or $parts[2].Equals("P", [System.StringComparison]::OrdinalIgnoreCase))) {
            return ("[AD:{0}:{1}:{2}:*]" -f $parts[0], $parts[1], $parts[2].ToUpperInvariant())
        }

        if ($parts.Count -ge 3) {
            return ("[AD:{0}:{1}:*]" -f $parts[0], $parts[1])
        }
    }

    return $text
}

function Normalize-ScoredTags {
    param([object[]]$Tags)
    return @($Tags | ForEach-Object { Normalize-ScoredTag ([string]$_) } | Select-Object -Unique)
}

function Get-RelativePath {
    param([string]$PathValue)
    if ([string]::IsNullOrWhiteSpace($PathValue)) {
        return ""
    }

    try {
        $root = [System.IO.Path]::GetFullPath($ScriptRoot).TrimEnd('\', '/')
        $full = [System.IO.Path]::GetFullPath($PathValue)
        if ($full.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
            return $full.Substring($root.Length).TrimStart('\', '/')
        }
    }
    catch {
    }

    return $PathValue
}

if ($RunDirs.Count -eq 0) {
    $RunDirs = @(Get-ChildItem -LiteralPath $RunRoot -Directory | Sort-Object Name | ForEach-Object { $_.FullName })
}
elseif ($RunDirs.Count -eq 1 -and $RunDirs[0].Contains(",")) {
    $RunDirs = @($RunDirs[0].Split(",") | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

$items = New-Object System.Collections.Generic.List[object]
$runStats = [ordered]@{}

foreach ($runDirInput in $RunDirs) {
    $runDir = $runDirInput
    if (-not [System.IO.Path]::IsPathRooted($runDir)) {
        $runDir = Join-Path $RunRoot $runDir
    }

    if (-not (Test-Path -LiteralPath $runDir)) {
        Write-Warning ("Missing run dir: " + $runDir)
        continue
    }

    $runName = Split-Path -Leaf $runDir
    $metaFiles = Get-ChildItem -LiteralPath $runDir -File -Filter "*.meta.json" | Sort-Object Name
    $runStats[$runName] = [ordered]@{
        total = 0
        success = 0
        failed = 0
    }

    foreach ($metaFile in $metaFiles) {
        $meta = Get-Content -LiteralPath $metaFile.FullName -Raw -Encoding UTF8 | ConvertFrom-Json
        $assistantText = [string]$meta.assistantText
        if ([string]::IsNullOrWhiteSpace($assistantText) -and -not [string]::IsNullOrWhiteSpace([string]$meta.responsePath) -and (Test-Path -LiteralPath ([string]$meta.responsePath))) {
            $assistantText = Get-Content -LiteralPath ([string]$meta.responsePath) -Raw -Encoding UTF8
        }

        $expectedTags = @($meta.expectedTags)
        $actualTags = @(Extract-Tags $assistantText)
        $normalizedExpectedTags = @(Normalize-ScoredTags $expectedTags)
        $normalizedActualTags = @(Normalize-ScoredTags $actualTags)
        $missingTags = @($normalizedExpectedTags | Where-Object { $normalizedActualTags -notcontains $_ })
        $unexpectedTags = @($normalizedActualTags | Where-Object { $normalizedExpectedTags -notcontains $_ })
        $preprocessHits = @($meta.preprocessHits | ForEach-Object { ([string]$_).Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        $hitKey = if ($preprocessHits.Count -gt 0) {
            (@($preprocessHits | Sort-Object) -join " + ")
        }
        else {
            ""
        }

        $runStats[$runName].total++
        if ($meta.success) {
            $runStats[$runName].success++
        }
        else {
            $runStats[$runName].failed++
        }

        $items.Add([ordered]@{
            runDir = $runName
            metaFile = $metaFile.Name
            caseId = [string]$meta.caseId
            title = [string]$meta.title
            success = [bool]$meta.success
            statusCode = [int]$meta.statusCode
            error = [string]$meta.error
            model = [string]$meta.model
            apiUrl = [string]$meta.apiUrl
            thinkingEnabled = [bool]$meta.thinkingEnabled
            reasoningEffortSent = [string]$meta.reasoningEffortSent
            promptVersionPath = [string]$meta.promptVersionPath
            preprocessHits = @($preprocessHits)
            hitCount = $preprocessHits.Count
            hitKey = $hitKey
            expectedTags = @($expectedTags)
            actualTags = @($actualTags)
            normalizedExpectedTags = @($normalizedExpectedTags)
            normalizedActualTags = @($normalizedActualTags)
            missingExpectedTags = @($missingTags)
            unexpectedTags = @($unexpectedTags)
            exactTagMatch = ($missingTags.Count -eq 0 -and $unexpectedTags.Count -eq 0)
            promptPath = Get-RelativePath ([string]$meta.promptPath)
            requestPath = Get-RelativePath ([string]$meta.requestPath)
            responsePath = Get-RelativePath ([string]$meta.responsePath)
            metaPath = Get-RelativePath $metaFile.FullName
        })
    }
}

$outDir = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outDir)) {
    [System.IO.Directory]::CreateDirectory($outDir) | Out-Null
}

$lines = New-Object System.Collections.Generic.List[string]
foreach ($item in $items) {
    $lines.Add(($item | ConvertTo-Json -Compress -Depth 20))
}

[System.IO.File]::WriteAllText($OutputPath, (($lines -join "`n") + "`n"), $Utf8NoBom)

$total = $items.Count
$success = @($items | Where-Object { $_.success }).Count
$failed = $total - $success
$exact = @($items | Where-Object { $_.exactTagMatch }).Count
$summary = [ordered]@{
    generatedAt = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    outputPath = $OutputPath
    total = $total
    success = $success
    failed = $failed
    exactTagMatch = $exact
    exactTagMismatch = $total - $exact
    runStats = $runStats
    note = "exactTagMatch uses normalized scoring: ACTION:DUEL_LINE_WIN/LOSE text and AD remark text are ignored; it is not final quality scoring."
}

[System.IO.File]::WriteAllText($SummaryPath, ($summary | ConvertTo-Json -Depth 20), $Utf8NoBom)

Write-Host ("Indexed items: {0}" -f $total)
Write-Host ("Success: {0}; failed: {1}" -f $success, $failed)
Write-Host ("Exact tag match: {0}; mismatch: {1}" -f $exact, ($total - $exact))
Write-Host ("Index file: {0}" -f $OutputPath)
Write-Host ("Summary file: {0}" -f $SummaryPath)
