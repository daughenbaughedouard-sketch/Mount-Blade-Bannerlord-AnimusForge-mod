@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo AnimusForge Mod 一键编译脚本
echo ========================================
echo.

REM ============ 配置区域 - 请修改这里的路径 ============
set "BANNERLORD_ROOT=C:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord"
set "OUTPUT_DIR=Z:\MyBannerlordMods\AFmod\AF-beta0.1.0"
REM ====================================================

echo 配置信息:
echo - 骑砍本体路径: %BANNERLORD_ROOT%
echo - 输出目录: %OUTPUT_DIR%
echo.

REM 获取项目根目录（脚本在 AFbeta-src/build-scripts，项目在 AFbeta-src）
set "SCRIPT_DIR=%~dp0"
set "PROJECT_ROOT=%SCRIPT_DIR%.."
cd /d "%PROJECT_ROOT%"

echo 当前工作目录: %CD%
echo.

echo [1/5] 清理旧的编译产物...
if exist "bin" rd /s /q "bin"
if exist "obj" rd /s /q "obj"
echo 清理完成
echo.

echo [2/5] 编译项目...
dotnet build AnimusForge.csproj -c Release /p:BannerlordRoot="%BANNERLORD_ROOT%"
if %errorlevel% neq 0 (
    echo.
    echo 编译失败！
    pause
    exit /b 1
)
echo 编译成功
echo.

echo [3/5] 准备输出目录...
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"
if not exist "%OUTPUT_DIR%\bin\Win64_Shipping_Client" mkdir "%OUTPUT_DIR%\bin\Win64_Shipping_Client"
echo 输出目录已准备
echo.

echo [4/5] 复制文件到输出目录...

REM 复制主DLL和依赖
echo - 复制 AnimusForge.dll...
copy /y "bin\Release\net472\AnimusForge.dll" "%OUTPUT_DIR%\bin\Win64_Shipping_Client\" >nul
copy /y "bin\Release\net472\AnimusForge.pdb" "%OUTPUT_DIR%\bin\Win64_Shipping_Client\" >nul

echo - 复制依赖DLL...
copy /y "bin\Release\net472\Microsoft.ML.OnnxRuntime.dll" "%OUTPUT_DIR%\bin\Win64_Shipping_Client\" >nul
copy /y "bin\Release\net472\System.Memory.dll" "%OUTPUT_DIR%\bin\Win64_Shipping_Client\" >nul
copy /y "bin\Release\net472\System.Buffers.dll" "%OUTPUT_DIR%\bin\Win64_Shipping_Client\" >nul
copy /y "bin\Release\net472\System.Runtime.CompilerServices.Unsafe.dll" "%OUTPUT_DIR%\bin\Win64_Shipping_Client\" >nul

REM 复制资源文件
echo - 复制资源文件...
xcopy /y /i /q "AnimusForge\SubModule.xml" "%OUTPUT_DIR%\" >nul
xcopy /y /i /q "AnimusForge\VoiceMapping.json" "%OUTPUT_DIR%\" >nul

echo - 复制资源文件夹...
xcopy /y /i /e /q "AnimusForge\GUI" "%OUTPUT_DIR%\GUI\" >nul
xcopy /y /i /e /q "AnimusForge\ModuleData" "%OUTPUT_DIR%\ModuleData\" >nul
xcopy /y /i /e /q "AnimusForge\ONNX" "%OUTPUT_DIR%\ONNX\" >nul
xcopy /y /i /e /q "AnimusForge\PlayerExports" "%OUTPUT_DIR%\PlayerExports\" >nul

echo 文件复制完成
echo.

echo [5/5] 清理源码目录中的编译产物...
rd /s /q "bin"
rd /s /q "obj"
echo 清理完成
echo.

echo ========================================
echo 编译完成！
echo 输出位置: %OUTPUT_DIR%
echo ========================================
pause
