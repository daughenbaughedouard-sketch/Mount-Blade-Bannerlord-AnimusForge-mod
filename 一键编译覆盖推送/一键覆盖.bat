@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "PROJECT_ROOT=%%~fI"
cd /d "%SCRIPT_DIR%"
set "BUILD_DEPLOY_SCRIPT=%SCRIPT_DIR%一键编译并覆盖.bat"

echo [AnimusForge] One-click deploy redirected to build + overwrite...
echo Script Dir : "%SCRIPT_DIR%"
echo Project Dir: "%PROJECT_ROOT%"

if not exist "%BUILD_DEPLOY_SCRIPT%" (
    echo [ERROR] Build deploy script not found:
    echo   "%BUILD_DEPLOY_SCRIPT%"
    pause
    exit /b 1
)

echo.
echo [INFO] Single unversioned deploy is disabled.
echo [INFO] Default output will only go to:
echo   AnimusForge_1_3_x
echo [INFO] Use 一键编译并覆盖.bat --dual when 1.4.5 output is also needed.
echo.

call "%BUILD_DEPLOY_SCRIPT%" --no-launch
if errorlevel 1 (
    echo [ERROR] Build + overwrite failed.
    pause
    exit /b 1
)

echo [SUCCESS] Deploy completed.
pause
exit /b 0
