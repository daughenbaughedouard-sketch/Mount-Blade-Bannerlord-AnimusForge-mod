@echo off
setlocal
cd /d "%~dp0"
set "APP_EXE=src\PreprocessTopicPromptLab.App\bin\Debug\net10.0-windows\AnimusForgePreprocessTopicPromptLab.exe"
set "APP_PROJECT=src\PreprocessTopicPromptLab.App\PreprocessTopicPromptLab.App.csproj"

if exist "%APP_EXE%" (
    start "" "%APP_EXE%"
) else (
    start "" dotnet run --project "%APP_PROJECT%"
)
