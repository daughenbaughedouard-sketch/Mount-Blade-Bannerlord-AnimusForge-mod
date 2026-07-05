param(
    [switch]$FrameworkDependent,
    [switch]$SkipZip
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $root "src\ActionPostprocessPromptLab.App\ActionPostprocessPromptLab.App.csproj"

function Test-FileLocked {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $false
    }

    try {
        $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
        $stream.Close()
        return $false
    } catch {
        return $true
    }
}

if ($FrameworkDependent) {
    $output = Join-Path $root "dist\win-x64-framework-dependent"
    dotnet publish $project -c Release -r win-x64 --self-contained false -o $output /p:PublishSingleFile=true
} else {
    $output = Join-Path $root "dist\win-x64-self-contained"
    $targetExe = Join-Path $output "AnimusForgeActionPostprocessPromptLab.exe"
    if ((Test-Path -LiteralPath $targetExe -PathType Leaf) -or (Test-FileLocked -Path $targetExe)) {
        $output = Join-Path $root ("dist\win-x64-self-contained-" + (Get-Date -Format "yyyyMMdd-HHmmss"))
    }
    dotnet publish $project -c Release -r win-x64 --self-contained true -o $output /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true /p:PublishTrimmed=false
}

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$exe = Join-Path $output "AnimusForgeActionPostprocessPromptLab.exe"
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Publish completed but exe was not found: $exe"
}

$readmePath = Join-Path $output "README_User.txt"
$readmeLines = @(
    "AnimusForge 后处理提示词实验室",
    "",
    "使用方式：",
    "1. 建议把整个工具保留在 AnimusForge 仓库内，或直接从仓库里的 dist 发布目录运行。",
    "2. 双击 AnimusForgeActionPostprocessPromptLab.exe。",
    "3. 填写 OpenAI 兼容接口地址、API Key 和模型名。",
    "4. 顶部[思考]默认开启，[强度]默认 max；需要兼容不支持思考字段的接口时可以关闭。",
    "5. 从 tools\ActionPostprocessPromptLab\cases 打开或编辑案例。",
    "6. 在[标签提示词]页可以修改全局标签说明；标签本身只读，不能改格式。",
    "7. 在[完整提示词]页看最终发送内容，在[原始回复]页看接口完整返回。",
    "8. 可以运行当前案例，也可以批量运行全部案例。",
    "",
    "输出文件：",
    "- 完整提示词、请求体、模型回复和元数据文件会保存到 tools\ActionPostprocessPromptLab\runs\<timestamp>。",
    "- 请求体文件不会包含 API Key。",
    "",
    "说明：",
    "- 本工具不会启动 Bannerlord。",
    "- 本工具不会加载 TaleWorlds DLL 或 AnimusForge.dll。",
    "- 本工具不会部署或覆盖 Bannerlord Modules。"
)
Set-Content -LiteralPath $readmePath -Value $readmeLines -Encoding UTF8

$zipPath = ""
if (-not $SkipZip) {
    $packageRoot = Join-Path $root "dist\packages"
    New-Item -ItemType Directory -Force -Path $packageRoot | Out-Null
    $flavor = if ($FrameworkDependent) { "framework-dependent" } else { "self-contained" }
    $zipPath = Join-Path $packageRoot ("AnimusForgeActionPostprocessPromptLab-win-x64-" + $flavor + "-" + (Get-Date -Format "yyyyMMdd-HHmmss") + ".zip")
    Compress-Archive -Path (Join-Path $output "*") -DestinationPath $zipPath -Force
}

Write-Host "Published:"
Write-Host $exe
if (-not [string]::IsNullOrWhiteSpace($zipPath)) {
    Write-Host "Package:"
    Write-Host $zipPath
}
