@echo off
setlocal EnableExtensions EnableDelayedExpansion
chcp 65001 >nul

REM Script directory and project root
set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "PROJECT_ROOT=%%~fI"
cd /d "%SCRIPT_DIR%"

set "CONFIG=Debug"
set "BUILD_TARGET=1.3"
set "PATH_SCRIPT=%SCRIPT_DIR%resolve_bannerlord_paths.ps1"
set "DEPLOY_SCRIPT=%SCRIPT_DIR%deploy_module.ps1"
set "BANNERLORD_ROOT="
set "WORKSHOP_CONTENT_DIR="

:parse_args
if "%~1"=="" goto args_done
if /I "%~1"=="--dual" (
    set "BUILD_TARGET=dual"
    shift
    goto parse_args
)
if /I "%~1"=="--all" (
    set "BUILD_TARGET=dual"
    shift
    goto parse_args
)
echo [ERROR] Unknown argument: %~1
echo Usage: %~nx0 [--dual]
pause
exit /b 1

:args_done

for /f "usebackq tokens=1,* delims==" %%A in (`powershell -NoProfile -ExecutionPolicy Bypass -File "%PATH_SCRIPT%"`) do (
    if /I "%%A"=="BANNERLORD_ROOT" set "BANNERLORD_ROOT=%%B"
    if /I "%%A"=="WORKSHOP_CONTENT_DIR" set "WORKSHOP_CONTENT_DIR=%%B"
)

set "BUILD_OUTPUT=%PROJECT_ROOT%\bin\%CONFIG%\net472"
set "ARTIFACT_DIR=%PROJECT_ROOT%\bin\%CONFIG%\dual_client_artifacts"
set "ARTIFACT_13=%ARTIFACT_DIR%\1.3.x\AnimusForge.dll"
set "ARTIFACT_14=%ARTIFACT_DIR%\1.4.5\AnimusForge.dll"

echo [AnimusForge] Build + module output started...
echo Script Dir : "%SCRIPT_DIR%"
echo Project Dir: "%PROJECT_ROOT%"
echo Bannerlord : "%BANNERLORD_ROOT%"
if defined WORKSHOP_CONTENT_DIR echo Workshop  : "%WORKSHOP_CONTENT_DIR%"
echo Config     : "%CONFIG%"
echo Target     : "%BUILD_TARGET%"
echo.

call "%SCRIPT_DIR%resolve_dotnet_sdk.bat"
if errorlevel 1 (
    pause
    exit /b 1
)
echo Dotnet    : "%DOTNET_EXE%"

if not exist "%BANNERLORD_ROOT%" (
    echo [ERROR] Bannerlord root not found:
    echo "%BANNERLORD_ROOT%"
    pause
    exit /b 1
)

if not exist "%DEPLOY_SCRIPT%" (
    echo [ERROR] Deploy script not found:
    echo "%DEPLOY_SCRIPT%"
    pause
    exit /b 1
)

if /I "%BUILD_TARGET%"=="dual" goto build_dual

echo [1/2] Building AnimusForge for Bannerlord 1.3.x...
call :BuildVersion 1.3 "%ARTIFACT_13%"
if errorlevel 1 exit /b %ERRORLEVEL%

echo.
echo [2/2] Writing 1.3.x module to Bannerlord Modules...
powershell -NoProfile -ExecutionPolicy Bypass -File "%DEPLOY_SCRIPT%" -ProjectRoot "%PROJECT_ROOT%" -BannerlordRoot "%BANNERLORD_ROOT%" -Client13Output -BuildDll13 "%ARTIFACT_13%"
if errorlevel 1 (
    echo [ERROR] 1.3.x module output failed.
    pause
    exit /b 1
)

echo.
echo [SUCCESS] 1.3.x module generated:
echo   "%BANNERLORD_ROOT%\Modules\AnimusForge_1_3_x"
echo Use --dual only when 1.4.5 dependencies are available and you need both modules.
pause
exit /b 0

:build_dual
echo [1/3] Building AnimusForge for Bannerlord 1.3.x...
call :BuildVersion 1.3 "%ARTIFACT_13%"
if errorlevel 1 exit /b %ERRORLEVEL%

echo.
echo [2/3] Building AnimusForge for Bannerlord 1.4.5...
call :BuildVersion 1.4 "%ARTIFACT_14%"
if errorlevel 1 exit /b %ERRORLEVEL%

echo.
echo [3/3] Writing dual modules to Bannerlord Modules...
powershell -NoProfile -ExecutionPolicy Bypass -File "%DEPLOY_SCRIPT%" -ProjectRoot "%PROJECT_ROOT%" -BannerlordRoot "%BANNERLORD_ROOT%" -DualClientOutput -BuildDll13 "%ARTIFACT_13%" -BuildDll14 "%ARTIFACT_14%"
if errorlevel 1 (
    echo [ERROR] Dual module output failed.
    pause
    exit /b 1
)

echo.
echo [SUCCESS] Dual-version modules generated:
echo   "%BANNERLORD_ROOT%\Modules\AnimusForge_1_3_x"
echo   "%BANNERLORD_ROOT%\Modules\AnimusForge_1_4_5"
echo.
echo Enable only the AnimusForge module matching the current Bannerlord version.
pause
exit /b 0

:BuildVersion
set "API_VERSION=%~1"
set "TARGET_DLL=%~2"
set "TARGET_DIR=%~dp2"

if defined WORKSHOP_CONTENT_DIR (
    "%DOTNET_EXE%" build "%PROJECT_ROOT%\AnimusForge.csproj" -c %CONFIG% /p:BannerlordApi=%API_VERSION% /p:BannerlordRoot="%BANNERLORD_ROOT%" /p:WorkshopContentDir="%WORKSHOP_CONTENT_DIR%"
) else (
    "%DOTNET_EXE%" build "%PROJECT_ROOT%\AnimusForge.csproj" -c %CONFIG% /p:BannerlordApi=%API_VERSION% /p:BannerlordRoot="%BANNERLORD_ROOT%"
)
set "ERR=%ERRORLEVEL%"
if not "%ERR%"=="0" (
    echo.
    echo [FAILED] Build failed for BannerlordApi=%API_VERSION%. ExitCode=%ERR%
    pause
    exit /b %ERR%
)

if not exist "%BUILD_OUTPUT%\AnimusForge.dll" (
    echo [ERROR] Built DLL not found:
    echo   "%BUILD_OUTPUT%\AnimusForge.dll"
    pause
    exit /b 1
)

if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"
copy /Y "%BUILD_OUTPUT%\AnimusForge.dll" "%TARGET_DLL%" >nul
if errorlevel 1 (
    echo [ERROR] Failed to copy build artifact:
    echo   "%TARGET_DLL%"
    pause
    exit /b 1
)

if exist "%BUILD_OUTPUT%\AnimusForge.pdb" (
    copy /Y "%BUILD_OUTPUT%\AnimusForge.pdb" "%TARGET_DIR%AnimusForge.pdb" >nul
)

echo [OK] Captured BannerlordApi=%API_VERSION% artifact:
echo      "%TARGET_DLL%"
exit /b 0