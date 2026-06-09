param(
    [string]$ProjectRoot = "",
    [string]$BuildDll = "",
    [string]$BannerlordRoot = "",
    [switch]$Client13Output,
    [switch]$DualClientOutput,
    [string]$BuildDll13 = "",
    [string]$BuildDll14 = ""
)

$ErrorActionPreference = "Stop"

function Get-FullPathSafe {
    param([Parameter(Mandatory = $true)][string]$Path)

    return [System.IO.Path]::GetFullPath($Path)
}

function Test-SourceModuleDir {
    param([Parameter(Mandatory = $true)][string]$Path)

    $requiredEntries = @("SubModule.xml", "ModuleData", "GUI", "ONNX")
    $missing = New-Object System.Collections.Generic.List[string]

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "Source module directory not found: $Path"
    }

    foreach ($entry in $requiredEntries) {
        if (-not (Test-Path -LiteralPath (Join-Path $Path $entry))) {
            $missing.Add($entry)
        }
    }

    if ($missing.Count -gt 0) {
        throw ("Source module directory is incomplete: {0}`nMissing: {1}" -f $Path, ($missing -join ", "))
    }
}

function Get-Sha256Hash {
    param([Parameter(Mandatory = $true)][string]$Path)

    $fullPath = Get-FullPathSafe -Path $Path
    if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
        throw "File not found for hash: $fullPath"
    }

    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    $stream = [System.IO.File]::OpenRead($fullPath)
    try {
        $hashBytes = $sha256.ComputeHash($stream)
        return ([System.BitConverter]::ToString($hashBytes) -replace "-", "")
    } finally {
        $stream.Dispose()
        $sha256.Dispose()
    }
}

function Get-TargetModuleDirs {
    param([string]$BannerlordRootPath)

    throw "Single unversioned deploy is disabled. Use -DualClientOutput so output goes only to AnimusForge_1_3_x and AnimusForge_1_4_5."
}

function Get-BannerlordModulesDir {
    param([string]$BannerlordRootPath)

    if ([string]::IsNullOrWhiteSpace($BannerlordRootPath)) {
        $targetDirs = @(Get-TargetModuleDirs -BannerlordRootPath $BannerlordRootPath)
        if ($targetDirs.Count -eq 0) {
            throw "No Bannerlord module target found."
        }
        return (Split-Path -Path $targetDirs[0] -Parent)
    }

    $bannerlordRootFull = Get-FullPathSafe -Path $BannerlordRootPath
    if (-not (Test-Path -LiteralPath $bannerlordRootFull -PathType Container)) {
        throw "Bannerlord root not found: $bannerlordRootFull"
    }

    $modulesDir = Join-Path $bannerlordRootFull "Modules"
    if (-not (Test-Path -LiteralPath $modulesDir -PathType Container)) {
        throw "Bannerlord Modules directory not found: $modulesDir"
    }

    return (Get-FullPathSafe -Path $modulesDir)
}

function Get-DualClientTargetModuleDirs {
    param([string]$BannerlordRootPath)

    $modulesDir = Get-BannerlordModulesDir -BannerlordRootPath $BannerlordRootPath
    return @(
        (Join-Path $modulesDir "AnimusForge_1_3_x"),
        (Join-Path $modulesDir "AnimusForge_1_4_5")
    )
}

function Get-ExistingModuleXmlTargets {
    param([string[]]$TargetModuleDirs)

    $existingTargets = New-Object System.Collections.Generic.List[string]
    foreach ($targetDir in $TargetModuleDirs) {
        if (-not [string]::IsNullOrWhiteSpace($targetDir)) {
            $xmlPath = Join-Path $targetDir "SubModule.xml"
            if (Test-Path -LiteralPath $xmlPath -PathType Leaf) {
                $existingTargets.Add((Get-FullPathSafe -Path $targetDir))
            }
        }
    }

    return @($existingTargets | Sort-Object -Unique)
}

function Sync-SubModuleXmlBackToSource {
    param(
        [Parameter(Mandatory = $true)][string]$SourceModuleDir,
        [Parameter(Mandatory = $true)][string[]]$TargetModuleDirs
    )

    $sourceXmlPath = Join-Path $SourceModuleDir "SubModule.xml"
    $existingTargets = @(Get-ExistingModuleXmlTargets -TargetModuleDirs $TargetModuleDirs)

    if ($existingTargets.Count -eq 0) {
        Write-Host "SubModule XML: no existing target XML found, keeping source copy"
        return
    }

    $selectedTarget = @($existingTargets)[0]
    $selectedXmlPath = Join-Path $selectedTarget "SubModule.xml"
    $selectedHash = Get-Sha256Hash -Path $selectedXmlPath

    if ($existingTargets.Count -gt 1) {
        $mismatchedTargets = New-Object System.Collections.Generic.List[string]
        foreach ($targetDir in $existingTargets | Select-Object -Skip 1) {
            $targetXmlPath = Join-Path $targetDir "SubModule.xml"
            $targetHash = Get-Sha256Hash -Path $targetXmlPath
            if ($targetHash -ne $selectedHash) {
                $mismatchedTargets.Add($targetDir)
            }
        }

        if ($mismatchedTargets.Count -gt 0) {
            $allTargets = ($existingTargets | ForEach-Object { " - $_" }) -join "`r`n"
            throw "Multiple target SubModule.xml files differ. Refusing to choose one source of truth.`r`n$allTargets"
        }
    }

    Copy-Item -LiteralPath $selectedXmlPath -Destination $sourceXmlPath -Force
    Write-Host "Synced XML   : $selectedXmlPath -> $sourceXmlPath"
}

function Sync-PlayerExportsBackToSource {
    param(
        [Parameter(Mandatory = $true)][string]$SourceModuleDir,
        [Parameter(Mandatory = $true)][string[]]$TargetModuleDirs
    )

    $existingTargets = @(Get-ExistingModuleXmlTargets -TargetModuleDirs $TargetModuleDirs)
    if ($existingTargets.Count -eq 0) {
        Write-Host "PlayerExports: no existing target module found, keeping source copy"
        return
    }

    $selectedTarget = @($existingTargets)[0]
    $targetPlayerExports = Join-Path $selectedTarget "PlayerExports"
    $sourcePlayerExports = Join-Path $SourceModuleDir "PlayerExports"

    if (-not (Test-Path -LiteralPath $targetPlayerExports -PathType Container)) {
        Write-Host "PlayerExports: target folder not found, keeping source copy"
        return
    }

    New-Item -ItemType Directory -Path $sourcePlayerExports -Force | Out-Null

    $arguments = @(
        $targetPlayerExports,
        $sourcePlayerExports,
        "/MIR",
        "/R:1",
        "/W:1",
        "/NP",
        "/NFL",
        "/NDL",
        "/NJH",
        "/NJS"
    )

    & robocopy @arguments | Out-Null
    $exitCode = $LASTEXITCODE
    if ($exitCode -ge 8) {
        throw "robocopy failed while syncing PlayerExports back to source with exit code $exitCode"
    }

    Write-Host "Synced Data  : $targetPlayerExports -> $sourcePlayerExports"
}

function Sync-BuildOutputIntoSourceModule {
    param(
        [Parameter(Mandatory = $true)][string]$SourceModuleDir,
        [string]$BuildDllPath
    )

    if ([string]::IsNullOrWhiteSpace($BuildDllPath)) {
        return
    }

    $buildDllFull = Get-FullPathSafe -Path $BuildDllPath
    if (-not (Test-Path -LiteralPath $buildDllFull -PathType Leaf)) {
        throw "Build DLL not found: $buildDllFull"
    }

    $moduleBinDir = Join-Path $SourceModuleDir "bin\Win64_Shipping_Client"
    New-Item -ItemType Directory -Path $moduleBinDir -Force | Out-Null

    $targetDllPath = Join-Path $moduleBinDir "AnimusForge.dll"
    Copy-Item -LiteralPath $buildDllFull -Destination $targetDllPath -Force
    Write-Host "Updated     : $targetDllPath"

    $buildPdbPath = [System.IO.Path]::ChangeExtension($buildDllFull, ".pdb")
    if (Test-Path -LiteralPath $buildPdbPath -PathType Leaf) {
        $targetPdbPath = Join-Path $moduleBinDir "AnimusForge.pdb"
        Copy-Item -LiteralPath $buildPdbPath -Destination $targetPdbPath -Force
        Write-Host "Updated     : $targetPdbPath"
    }
}

function Invoke-RobocopySync {
    param(
        [Parameter(Mandatory = $true)][string]$SourceDir,
        [Parameter(Mandatory = $true)][string]$TargetDir
    )

    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null

    $arguments = @(
        $SourceDir,
        $TargetDir,
        "/E",
        "/R:1",
        "/W:1",
        "/NP",
        "/NFL",
        "/NDL",
        "/NJH",
        "/NJS",
        "/XD",
        "Logs"
    )

    & robocopy @arguments | Out-Null
    $exitCode = $LASTEXITCODE
    if ($exitCode -ge 8) {
        throw "robocopy failed for target '$TargetDir' with exit code $exitCode"
    }
}

function Invoke-RobocopyModuleSync {
    param(
        [Parameter(Mandatory = $true)][string]$SourceDir,
        [Parameter(Mandatory = $true)][string]$TargetDir
    )

    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null

    $arguments = @(
        $SourceDir,
        $TargetDir,
        "/MIR",
        "/R:1",
        "/W:1",
        "/NP",
        "/NFL",
        "/NDL",
        "/NJH",
        "/NJS",
        "/XD",
        "Logs",
        "Clients",
        "/XF",
        "AnimusForge.1.3.x.dll",
        "AnimusForge.1.3.x.pdb",
        "AnimusForge.1.4.5.dll",
        "AnimusForge.1.4.5.pdb"
    )

    & robocopy @arguments | Out-Null
    $exitCode = $LASTEXITCODE
    if ($exitCode -ge 8) {
        throw "robocopy failed for target '$TargetDir' with exit code $exitCode"
    }
}

function Copy-BuildOutputIntoModule {
    param(
        [Parameter(Mandatory = $true)][string]$TargetModuleDir,
        [Parameter(Mandatory = $true)][string]$BuildDllPath
    )

    $buildDllFull = Get-FullPathSafe -Path $BuildDllPath
    if (-not (Test-Path -LiteralPath $buildDllFull -PathType Leaf)) {
        throw "Build DLL not found: $buildDllFull"
    }

    $moduleBinDir = Join-Path $TargetModuleDir "bin\Win64_Shipping_Client"
    New-Item -ItemType Directory -Path $moduleBinDir -Force | Out-Null

    $targetDllPath = Join-Path $moduleBinDir "AnimusForge.dll"
    Copy-Item -LiteralPath $buildDllFull -Destination $targetDllPath -Force
    Write-Host "Updated DLL : $targetDllPath"

    $buildPdbPath = [System.IO.Path]::ChangeExtension($buildDllFull, ".pdb")
    if (Test-Path -LiteralPath $buildPdbPath -PathType Leaf) {
        $targetPdbPath = Join-Path $moduleBinDir "AnimusForge.pdb"
        Copy-Item -LiteralPath $buildPdbPath -Destination $targetPdbPath -Force
        Write-Host "Updated PDB : $targetPdbPath"
    }

    $dependencyNames = @(
        "0Harmony.dll",
        "Microsoft.ML.OnnxRuntime.dll",
        "System.Memory.dll",
        "System.Buffers.dll",
        "System.Runtime.CompilerServices.Unsafe.dll"
    )
    $candidateDependencyDirs = New-Object System.Collections.Generic.List[string]
    $candidateDependencyDirs.Add((Split-Path -Path $buildDllFull -Parent))
    if (-not [string]::IsNullOrWhiteSpace($script:projectRootFull)) {
        $candidateDependencyDirs.Add((Join-Path $script:projectRootFull "bin\Debug\net472"))
        $candidateDependencyDirs.Add((Join-Path $script:projectRootFull "bin\Release\net472"))
        $candidateDependencyDirs.Add((Join-Path $script:projectRootFull "AnimusForge\bin\Win64_Shipping_Client"))
    }

    foreach ($dependencyName in $dependencyNames) {
        $dependencySource = $null
        foreach ($candidateDir in $candidateDependencyDirs) {
            if ([string]::IsNullOrWhiteSpace($candidateDir)) {
                continue
            }
            $candidatePath = Join-Path $candidateDir $dependencyName
            if (Test-Path -LiteralPath $candidatePath -PathType Leaf) {
                $dependencySource = $candidatePath
                break
            }
        }

        if ([string]::IsNullOrWhiteSpace($dependencySource)) {
            throw "Required module-local dependency not found in build output: $dependencyName"
        }

        Copy-Item -LiteralPath $dependencySource -Destination (Join-Path $moduleBinDir $dependencyName) -Force
        Write-Host "Updated Dep : $(Join-Path $moduleBinDir $dependencyName)"
    }
}

function Set-SubModuleIdentity {
    param(
        [Parameter(Mandatory = $true)][string]$TargetModuleDir,
        [Parameter(Mandatory = $true)][string]$ModuleId,
        [Parameter(Mandatory = $true)][string]$DisplayName
    )

    $subModulePath = Join-Path $TargetModuleDir "SubModule.xml"
    if (-not (Test-Path -LiteralPath $subModulePath -PathType Leaf)) {
        throw "SubModule.xml not found after module sync: $subModulePath"
    }

    [xml]$xml = Get-Content -LiteralPath $subModulePath
    $xml.Module.Id.value = $ModuleId
    $xml.Module.Name.value = $DisplayName
    if ($xml.Module.SubModules -and $xml.Module.SubModules.SubModule) {
        foreach ($subModule in @($xml.Module.SubModules.SubModule)) {
            if ($subModule.Name) {
                $subModule.Name.value = $DisplayName
            }
            if ($subModule.DLLName) {
                $subModule.DLLName.value = "AnimusForge.dll"
            }
        }
    }
    if ($xml.Module.Assemblies -and $xml.Module.Assemblies.Assembly) {
        foreach ($assembly in @($xml.Module.Assemblies.Assembly)) {
            $assembly.value = "AnimusForge.dll"
        }
    }

    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Indent = $true
    $settings.IndentChars = "    "
    $settings.NewLineChars = "`r`n"
    $settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace
    $settings.OmitXmlDeclaration = $true

    $writer = [System.Xml.XmlWriter]::Create($subModulePath, $settings)
    try {
        $xml.Save($writer)
    } finally {
        $writer.Dispose()
    }

    Write-Host "Updated XML : $subModulePath ($ModuleId)"
}

function Deploy-DualClientModule {
    param(
        [Parameter(Mandatory = $true)][string]$SourceModuleDir,
        [Parameter(Mandatory = $true)][string]$BannerlordRootPath,
        [Parameter(Mandatory = $true)][string]$BuildDll13Path,
        [Parameter(Mandatory = $true)][string]$BuildDll14Path
    )

    $modulesDir = Get-BannerlordModulesDir -BannerlordRootPath $BannerlordRootPath
    $target13 = Join-Path $modulesDir "AnimusForge_1_3_x"
    $target14 = Join-Path $modulesDir "AnimusForge_1_4_5"

    $clientSpecs = @(
        [PSCustomObject]@{ Label = "1.3.x"; Target = $target13; BuildDll = $BuildDll13Path; ModuleId = "AnimusForge_1_3_x"; DisplayName = "AnimusForge 1.3.x" },
        [PSCustomObject]@{ Label = "1.4.5"; Target = $target14; BuildDll = $BuildDll14Path; ModuleId = "AnimusForge_1_4_5"; DisplayName = "AnimusForge 1.4.5" }
    )

    foreach ($client in $clientSpecs) {
        $targetFull = Get-FullPathSafe -Path $client.Target
        Write-Host "Deploying    : $targetFull ($($client.Label))"
        Invoke-RobocopyModuleSync -SourceDir $SourceModuleDir -TargetDir $targetFull
        Copy-BuildOutputIntoModule -TargetModuleDir $targetFull -BuildDllPath $client.BuildDll
        Set-SubModuleIdentity -TargetModuleDir $targetFull -ModuleId $client.ModuleId -DisplayName $client.DisplayName
        Assert-SameHash -SourceRoot $SourceModuleDir -TargetRoot $targetFull -RelativePath "ModuleData\RuleBehaviorPrompts.json"
        Assert-SameHash -SourceRoot $targetFull -TargetRoot $targetFull -RelativePath "bin\Win64_Shipping_Client\AnimusForge.dll"
    }

    Write-Host "Deploy Mode  : dual version module output"
    Write-Host "Deploy Result: success"
}

function Deploy-Client13Module {
    param(
        [Parameter(Mandatory = $true)][string]$SourceModuleDir,
        [Parameter(Mandatory = $true)][string]$BannerlordRootPath,
        [Parameter(Mandatory = $true)][string]$BuildDll13Path
    )

    $modulesDir = Get-BannerlordModulesDir -BannerlordRootPath $BannerlordRootPath
    $target13 = Join-Path $modulesDir "AnimusForge_1_3_x"
    $targetFull = Get-FullPathSafe -Path $target13

    Write-Host "Deploying    : $targetFull (1.3.x)"
    Invoke-RobocopyModuleSync -SourceDir $SourceModuleDir -TargetDir $targetFull
    Copy-BuildOutputIntoModule -TargetModuleDir $targetFull -BuildDllPath $BuildDll13Path
    Set-SubModuleIdentity -TargetModuleDir $targetFull -ModuleId "AnimusForge_1_3_x" -DisplayName "AnimusForge 1.3.x"
    Assert-SameHash -SourceRoot $SourceModuleDir -TargetRoot $targetFull -RelativePath "ModuleData\RuleBehaviorPrompts.json"
    Assert-SameHash -SourceRoot $targetFull -TargetRoot $targetFull -RelativePath "bin\Win64_Shipping_Client\AnimusForge.dll"

    Write-Host "Deploy Mode  : 1.3.x version module output"
    Write-Host "Deploy Result: success"
}

function Assert-SameHash {
    param(
        [Parameter(Mandatory = $true)][string]$SourceRoot,
        [Parameter(Mandatory = $true)][string]$TargetRoot,
        [Parameter(Mandatory = $true)][string]$RelativePath
    )

    $sourcePath = Join-Path $SourceRoot $RelativePath
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        return
    }

    $targetPath = Join-Path $TargetRoot $RelativePath
    if (-not (Test-Path -LiteralPath $targetPath -PathType Leaf)) {
        throw "Missing deployed file: $targetPath"
    }

    $sourceHash = Get-Sha256Hash -Path $sourcePath
    $targetHash = Get-Sha256Hash -Path $targetPath
    if ($sourceHash -ne $targetHash) {
        throw "Hash mismatch after deploy: $RelativePath"
    }

    Write-Host "Verified    : $RelativePath"
}

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = Join-Path $PSScriptRoot ".."
}

$projectRootFull = Get-FullPathSafe -Path $ProjectRoot
$sourceModuleDir = Join-Path $projectRootFull "AnimusForge"
$sourceModuleDir = Get-FullPathSafe -Path $sourceModuleDir

Test-SourceModuleDir -Path $sourceModuleDir

if ($DualClientOutput) {
    if ([string]::IsNullOrWhiteSpace($BuildDll13) -or [string]::IsNullOrWhiteSpace($BuildDll14)) {
        throw "DualClientOutput requires both -BuildDll13 and -BuildDll14."
    }
    if ([string]::IsNullOrWhiteSpace($BannerlordRoot)) {
        throw "DualClientOutput requires -BannerlordRoot."
    }

    $targetModuleDirsForSync = @(Get-DualClientTargetModuleDirs -BannerlordRootPath $BannerlordRoot)
    Sync-PlayerExportsBackToSource -SourceModuleDir $sourceModuleDir -TargetModuleDirs $targetModuleDirsForSync
    Write-Host "Source Module: $sourceModuleDir"
    Deploy-DualClientModule -SourceModuleDir $sourceModuleDir -BannerlordRootPath $BannerlordRoot -BuildDll13Path $BuildDll13 -BuildDll14Path $BuildDll14
    exit 0
}

if ($Client13Output) {
    if ([string]::IsNullOrWhiteSpace($BuildDll13)) {
        throw "Client13Output requires -BuildDll13."
    }
    if ([string]::IsNullOrWhiteSpace($BannerlordRoot)) {
        throw "Client13Output requires -BannerlordRoot."
    }

    $targetModuleDirsForSync = @((Join-Path (Get-BannerlordModulesDir -BannerlordRootPath $BannerlordRoot) "AnimusForge_1_3_x"))
    Sync-PlayerExportsBackToSource -SourceModuleDir $sourceModuleDir -TargetModuleDirs $targetModuleDirsForSync
    Write-Host "Source Module: $sourceModuleDir"
    Deploy-Client13Module -SourceModuleDir $sourceModuleDir -BannerlordRootPath $BannerlordRoot -BuildDll13Path $BuildDll13
    exit 0
}

throw "Single unversioned deploy is disabled. Use -DualClientOutput so output goes only to AnimusForge_1_3_x and AnimusForge_1_4_5."

$targetModuleDirs = @(Get-TargetModuleDirs -BannerlordRootPath $BannerlordRoot)
Sync-SubModuleXmlBackToSource -SourceModuleDir $sourceModuleDir -TargetModuleDirs $targetModuleDirs
Sync-PlayerExportsBackToSource -SourceModuleDir $sourceModuleDir -TargetModuleDirs $targetModuleDirs
Sync-BuildOutputIntoSourceModule -SourceModuleDir $sourceModuleDir -BuildDllPath $BuildDll

Write-Host "Source Module: $sourceModuleDir"
Write-Host "Targets      : $($targetModuleDirs.Count)"

foreach ($targetModuleDir in $targetModuleDirs) {
    $targetModuleDirFull = Get-FullPathSafe -Path $targetModuleDir

    if ($targetModuleDirFull.Equals($sourceModuleDir, [System.StringComparison]::OrdinalIgnoreCase)) {
        Write-Host "Skipped      : $targetModuleDirFull (source and target are the same)"
        continue
    }

    Write-Host "Deploying    : $targetModuleDirFull"
    Invoke-RobocopySync -SourceDir $sourceModuleDir -TargetDir $targetModuleDirFull
    Assert-SameHash -SourceRoot $sourceModuleDir -TargetRoot $targetModuleDirFull -RelativePath "SubModule.xml"
    Assert-SameHash -SourceRoot $sourceModuleDir -TargetRoot $targetModuleDirFull -RelativePath "ModuleData\RuleBehaviorPrompts.json"
    Assert-SameHash -SourceRoot $sourceModuleDir -TargetRoot $targetModuleDirFull -RelativePath "bin\Win64_Shipping_Client\AnimusForge.dll"
}

Write-Host "Deploy Mode  : whole module copy (excluding Logs)"
Write-Host "Deploy Result: success"
