@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo AnimusForge Mod Build Configuration
echo ========================================
echo.

set "CONFIG_FILE=%~dp0config.txt"

echo Please enter Bannerlord root path (e.g., C:\SteamLibrary\steamapps\common\Mount ^& Blade II Bannerlord)
echo Default: C:\SteamLibrary\steamapps\common\Mount ^& Blade II Bannerlord
set /p "BANNERLORD_ROOT=Path: "

if "!BANNERLORD_ROOT!"=="" (
    set "BANNERLORD_ROOT=C:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord"
)

echo.
echo Please enter output directory (e.g., Z:\MyBannerlordMods\AFmod\AF-beta0.1.0)
echo Default: Z:\MyBannerlordMods\AFmod\AF-beta0.1.0
set /p "OUTPUT_DIR=Path: "

if "!OUTPUT_DIR!"=="" (
    set "OUTPUT_DIR=Z:\MyBannerlordMods\AFmod\AF-beta0.1.0"
)

echo.
echo Configuration:
echo - Bannerlord Root: !BANNERLORD_ROOT!
echo - Output Directory: !OUTPUT_DIR!
echo.

REM Validate Bannerlord path
if not exist "!BANNERLORD_ROOT!\bin\Win64_Shipping_Client" (
    echo Warning: Bannerlord path seems incorrect, cannot find bin\Win64_Shipping_Client directory
    echo Continue saving config? (Y/N)
    set /p "CONTINUE="
    if /i "!CONTINUE!" neq "Y" (
        echo Configuration cancelled
        pause
        exit /b 0
    )
)

REM Save config
echo BANNERLORD_ROOT=!BANNERLORD_ROOT!> "!CONFIG_FILE!"
echo OUTPUT_DIR=!OUTPUT_DIR!>> "!CONFIG_FILE!"

echo.
echo Configuration saved to: !CONFIG_FILE!
echo You can now run build.bat to compile
echo.
pause
