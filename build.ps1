param(
    [string]$Configuration = "Debug",
    [string]$ProjectPath = "$PSScriptRoot\Voxforge.csproj",
    [string]$DepsDir = "$PSScriptRoot\..\_deps_261550_managed",
    [string]$BannerlordRoot = "F:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord",
    [string]$Mcmv5Path = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-Mcmv5Path {
    param([string]$Root)

    $candidates = @(
        (Join-Path $Root "Modules\MCMv5\bin\Win64_Shipping_Client\MCMv5.dll"),
        (Join-Path $Root "Modules\MCM\bin\Win64_Shipping_Client\MCMv5.dll")
    )

    foreach ($path in $candidates) {
        if (Test-Path $path) { return $path }
    }

    $steamApps = Split-Path (Split-Path $Root -Parent) -Parent
    $workshop261550 = Join-Path $steamApps "workshop\content\261550"
    if (Test-Path $workshop261550) {
        $hit = Get-ChildItem -Path $workshop261550 -Recurse -File -Filter "MCMv5.dll" -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName
        if ($hit) { return $hit }
    }

    return ""
}

if ([string]::IsNullOrWhiteSpace($Mcmv5Path)) {
    $Mcmv5Path = Resolve-Mcmv5Path -Root $BannerlordRoot
}

$voxforgeBinDir = Join-Path $BannerlordRoot "Modules\Voxforge\bin\Win64_Shipping_Client"

$required = @(
    (Join-Path $DepsDir "TaleWorlds.CampaignSystem.dll"),
    (Join-Path $DepsDir "TaleWorlds.MountAndBlade.dll"),
    (Join-Path $DepsDir "TaleWorlds.Core.dll"),
    (Join-Path $DepsDir "TaleWorlds.Library.dll"),
    (Join-Path $DepsDir "TaleWorlds.MountAndBlade.View.dll"),
    (Join-Path $DepsDir "TaleWorlds.Engine.GauntletUI.dll"),
    (Join-Path $DepsDir "TaleWorlds.Engine.dll"),
    (Join-Path $DepsDir "TaleWorlds.GauntletUI.dll"),
    (Join-Path $DepsDir "Newtonsoft.Json.dll"),
    (Join-Path $DepsDir "TaleWorlds.Localization.dll"),
    (Join-Path $DepsDir "TaleWorlds.CampaignSystem.ViewModelCollection.dll"),
    (Join-Path $DepsDir "TaleWorlds.InputSystem.dll"),
    (Join-Path $DepsDir "TaleWorlds.ObjectSystem.dll"),
    (Join-Path $DepsDir "TaleWorlds.ScreenSystem.dll"),
    (Join-Path $DepsDir "TaleWorlds.DotNet.dll"),
    (Join-Path $DepsDir "SandBox.View.dll"),
    (Join-Path $DepsDir "SandBox.ViewModelCollection.dll"),
    (Join-Path $DepsDir "TaleWorlds.MountAndBlade.ViewModelCollection.dll"),
    (Join-Path $voxforgeBinDir "0Harmony.dll"),
    (Join-Path $voxforgeBinDir "Microsoft.ML.OnnxRuntime.dll"),
    (Join-Path $voxforgeBinDir "System.Memory.dll")
)

if (-not [string]::IsNullOrWhiteSpace($Mcmv5Path)) {
    $required += $Mcmv5Path
}

$missing = @($required | Where-Object { -not (Test-Path $_) })

if ([string]::IsNullOrWhiteSpace($Mcmv5Path)) {
    Write-Host "MCMv5.dll not found automatically." -ForegroundColor Yellow
    Write-Host "Pass -Mcmv5Path <full path> after Workshop mod download." -ForegroundColor Yellow
    exit 1
}

if ($missing.Count -gt 0) {
    Write-Host "Missing dependencies:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    exit 1
}

Write-Host "Building: $ProjectPath" -ForegroundColor Cyan
Write-Host "DepsDir : $DepsDir" -ForegroundColor Cyan
Write-Host "MCMv5   : $Mcmv5Path" -ForegroundColor Cyan

$args = @(
    "build",
    $ProjectPath,
    "-c", $Configuration,
    "/p:DepsDir=$DepsDir",
    "/p:BannerlordRoot=$BannerlordRoot",
    "/p:VoxforgeBinDir=$voxforgeBinDir",
    "/p:Mcmv5Path=$Mcmv5Path"
)

& dotnet @args
exit $LASTEXITCODE
