@echo off
setlocal EnableExtensions

set "RESOLVED_DOTNET_EXE="
set "HAS_DOTNET_SDK="

if defined PROJECT_ROOT (
    for %%I in ("%PROJECT_ROOT%\..\.dotnet-sdk\dotnet.exe") do if exist "%%~fI" set "RESOLVED_DOTNET_EXE=%%~fI"
    if not defined RESOLVED_DOTNET_EXE for %%I in ("%PROJECT_ROOT%\.dotnet\dotnet.exe") do if exist "%%~fI" set "RESOLVED_DOTNET_EXE=%%~fI"
)

if not defined RESOLVED_DOTNET_EXE (
    for %%I in ("%~dp0..\..\.dotnet-sdk\dotnet.exe") do if exist "%%~fI" set "RESOLVED_DOTNET_EXE=%%~fI"
)

if not defined RESOLVED_DOTNET_EXE (
    for /f "delims=" %%D in ('where dotnet 2^>nul') do if not defined RESOLVED_DOTNET_EXE set "RESOLVED_DOTNET_EXE=%%D"
)

if not defined RESOLVED_DOTNET_EXE (
    echo [ERROR] dotnet SDK not found. Expected local SDK at "%~dp0..\..\.dotnet-sdk\dotnet.exe" or dotnet in PATH.
    exit /b 1
)

for /f "delims=" %%S in ('"%RESOLVED_DOTNET_EXE%" --list-sdks 2^>nul') do set "HAS_DOTNET_SDK=1"
if not defined HAS_DOTNET_SDK (
    echo [ERROR] dotnet executable has no SDK available:
    echo   "%RESOLVED_DOTNET_EXE%"
    echo Install an SDK or keep G:\AFMOD\.dotnet-sdk available.
    exit /b 1
)

endlocal & set "DOTNET_EXE=%RESOLVED_DOTNET_EXE%"
exit /b 0
