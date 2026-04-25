# AnimusForge Mod Build Script
# PowerShell Version

# ============ Configuration - Modify paths here ============
$BANNERLORD_ROOT = "C:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord"
$OUTPUT_DIR = "Z:\MyBannerlordMods\AFmod\AF-beta0.1.0"
# ===========================================================

Write-Host "========================================"
Write-Host "AnimusForge Mod Build Script"
Write-Host "========================================"
Write-Host ""

Write-Host "Configuration:"
Write-Host "- Bannerlord Root: $BANNERLORD_ROOT"
Write-Host "- Output Directory: $OUTPUT_DIR"
Write-Host ""

# Validate Bannerlord path
Write-Host "Validating configuration..."
if (-not (Test-Path $BANNERLORD_ROOT)) {
    Write-Host ""
    Write-Host "ERROR: Game root path does not exist!" -ForegroundColor Red
    Write-Host "Path: $BANNERLORD_ROOT" -ForegroundColor Yellow
    Write-Host "Please check the BANNERLORD_ROOT configuration in build.ps1" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press any key to exit"
    exit 1
}

if (-not (Test-Path "$BANNERLORD_ROOT\bin\Win64_Shipping_Client")) {
    Write-Host ""
    Write-Host "ERROR: Game installation appears incomplete!" -ForegroundColor Red
    Write-Host "Cannot find: $BANNERLORD_ROOT\bin\Win64_Shipping_Client" -ForegroundColor Yellow
    Write-Host "Please verify your Bannerlord installation" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press any key to exit"
    exit 1
}

Write-Host "Configuration validated successfully" -ForegroundColor Green
Write-Host ""

# Get project root directory
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = (Get-Item $SCRIPT_DIR).Parent.FullName
Set-Location $PROJECT_ROOT

Write-Host "Project Root: $PROJECT_ROOT"
Write-Host "Current Directory: $(Get-Location)"
Write-Host ""

# Verify project file exists
if (-not (Test-Path "AnimusForge.csproj")) {
    Write-Host ""
    Write-Host "ERROR: Project file not found!" -ForegroundColor Red
    Write-Host "Cannot find AnimusForge.csproj in: $PROJECT_ROOT" -ForegroundColor Yellow
    Write-Host "Please ensure the script is in the correct location" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press any key to exit"
    exit 1
}

Write-Host "Project file found" -ForegroundColor Green
Write-Host ""

# [1/5] Clean old build artifacts
Write-Host "[1/5] Cleaning old build artifacts..."
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
Write-Host "Cleaned"
Write-Host ""

# [2/5] Build project
Write-Host "[2/5] Building project..."
Write-Host "Running: dotnet build AnimusForge.csproj -c Release" -ForegroundColor Cyan
$buildResult = & dotnet build AnimusForge.csproj -c Release /p:BannerlordRoot="$BANNERLORD_ROOT" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    Write-Host "Build output:" -ForegroundColor Yellow
    Write-Host $buildResult
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "1. .NET SDK not installed or not in PATH" -ForegroundColor Yellow
    Write-Host "2. Missing dependencies or references" -ForegroundColor Yellow
    Write-Host "3. Incorrect Bannerlord path in configuration" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press any key to exit"
    exit 1
}
Write-Host "Build succeeded" -ForegroundColor Green
Write-Host ""

# [3/5] Prepare output directory
Write-Host "[3/5] Preparing output directory..."
if (-not (Test-Path $OUTPUT_DIR)) {
    New-Item -ItemType Directory -Path $OUTPUT_DIR | Out-Null
}
if (-not (Test-Path "$OUTPUT_DIR\bin\Win64_Shipping_Client")) {
    New-Item -ItemType Directory -Path "$OUTPUT_DIR\bin\Win64_Shipping_Client" | Out-Null
}
Write-Host "Output directory ready"
Write-Host ""

# [4/5] Copy files to output directory
Write-Host "[4/5] Copying files to output directory..."

# Verify build output exists
if (-not (Test-Path "bin\Release\net472\AnimusForge.dll")) {
    Write-Host ""
    Write-Host "ERROR: Build output not found!" -ForegroundColor Red
    Write-Host "Cannot find: bin\Release\net472\AnimusForge.dll" -ForegroundColor Yellow
    Write-Host "Build may have failed silently" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press any key to exit"
    exit 1
}

# Copy main DLL and dependencies
Write-Host "- Copying AnimusForge.dll..."
try {
    Copy-Item "bin\Release\net472\AnimusForge.dll" "$OUTPUT_DIR\bin\Win64_Shipping_Client\" -Force -ErrorAction Stop
    Copy-Item "bin\Release\net472\AnimusForge.pdb" "$OUTPUT_DIR\bin\Win64_Shipping_Client\" -Force -ErrorAction Stop
} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to copy AnimusForge.dll" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "Press any key to exit"
    exit 1
}

Write-Host "- Copying dependency DLLs..."
try {
    Copy-Item "bin\Release\net472\Microsoft.ML.OnnxRuntime.dll" "$OUTPUT_DIR\bin\Win64_Shipping_Client\" -Force -ErrorAction Stop
    Copy-Item "bin\Release\net472\System.Memory.dll" "$OUTPUT_DIR\bin\Win64_Shipping_Client\" -Force -ErrorAction Stop
    Copy-Item "bin\Release\net472\System.Buffers.dll" "$OUTPUT_DIR\bin\Win64_Shipping_Client\" -Force -ErrorAction Stop
    Copy-Item "bin\Release\net472\System.Runtime.CompilerServices.Unsafe.dll" "$OUTPUT_DIR\bin\Win64_Shipping_Client\" -Force -ErrorAction Stop
} catch {
    Write-Host ""
    Write-Host "WARNING: Some dependency DLLs may not have been copied" -ForegroundColor Yellow
    Write-Host "Error: $_" -ForegroundColor Yellow
}

# Copy resource files
Write-Host "- Copying resource files..."
try {
    Copy-Item "AnimusForge\SubModule.xml" "$OUTPUT_DIR\" -Force -ErrorAction Stop
    Copy-Item "AnimusForge\VoiceMapping.json" "$OUTPUT_DIR\" -Force -ErrorAction Stop
} catch {
    Write-Host ""
    Write-Host "WARNING: Some resource files may not have been copied" -ForegroundColor Yellow
    Write-Host "Error: $_" -ForegroundColor Yellow
}

Write-Host "- Copying resource folders..."
try {
    Copy-Item "AnimusForge\GUI" "$OUTPUT_DIR\GUI\" -Recurse -Force -ErrorAction Stop
    Copy-Item "AnimusForge\ModuleData" "$OUTPUT_DIR\ModuleData\" -Recurse -Force -ErrorAction Stop
    Copy-Item "AnimusForge\ONNX" "$OUTPUT_DIR\ONNX\" -Recurse -Force -ErrorAction Stop
    Copy-Item "AnimusForge\PlayerExports" "$OUTPUT_DIR\PlayerExports\" -Recurse -Force -ErrorAction Stop
} catch {
    Write-Host ""
    Write-Host "WARNING: Some resource folders may not have been copied" -ForegroundColor Yellow
    Write-Host "Error: $_" -ForegroundColor Yellow
}

Write-Host "Files copied successfully" -ForegroundColor Green
Write-Host ""

# [5/5] Clean build artifacts from source directory
Write-Host "[5/5] Cleaning build artifacts from source directory..."
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
Write-Host "Cleaned"
Write-Host ""

Write-Host "========================================"
Write-Host "Build completed successfully!"
Write-Host "Output location: $OUTPUT_DIR"
Write-Host "========================================"
Read-Host "Press any key to exit"
