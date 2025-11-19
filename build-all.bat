@echo off
REM Build All Projects Script for Windows
REM This script builds both Backend (.NET) and Frontend (Angular) projects

setlocal enabledelayedexpansion

echo =========================================
echo Building Rasel Inbox Management System
echo =========================================
echo.

set BACKEND_STATUS=0
set FRONTEND_STATUS=0

REM Build Backend (.NET)
echo [1/2] Building Backend (.NET 8.0)...
echo =========================================

where dotnet >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    cd Backend

    echo Restoring NuGet packages...
    dotnet restore OutlookInboxManagement.csproj

    echo.
    echo Building Backend project...
    dotnet build OutlookInboxManagement.csproj --configuration Release --no-restore
    if %ERRORLEVEL% EQU 0 (
        echo [92m[SUCCESS] Backend build succeeded[0m
        set BACKEND_STATUS=0
    ) else (
        echo [91m[FAILED] Backend build failed[0m
        set BACKEND_STATUS=1
    )

    cd ..
) else (
    echo [93m[WARNING] .NET SDK not found - skipping Backend build[0m
    echo Install .NET 8.0 SDK from: https://dotnet.microsoft.com/download
    set BACKEND_STATUS=2
)

echo.
echo.

REM Build Frontend (Angular)
echo [2/2] Building Frontend (Angular)...
echo =========================================

where npm >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    cd Frontend

    echo Installing npm packages...
    call npm install

    echo.
    echo Building Frontend project...
    call npm run build
    if %ERRORLEVEL% EQU 0 (
        echo [92m[SUCCESS] Frontend build succeeded[0m
        set FRONTEND_STATUS=0
    ) else (
        echo [91m[FAILED] Frontend build failed[0m
        set FRONTEND_STATUS=1
    )

    cd ..
) else (
    echo [93m[WARNING] npm not found - skipping Frontend build[0m
    echo Install Node.js from: https://nodejs.org/
    set FRONTEND_STATUS=2
)

echo.
echo.
echo =========================================
echo Build Summary
echo =========================================

if %BACKEND_STATUS% EQU 0 (
    echo Backend:  [92mSUCCESS[0m
) else if %BACKEND_STATUS% EQU 1 (
    echo Backend:  [91mFAILED[0m
) else (
    echo Backend:  [93mSKIPPED[0m
)

if %FRONTEND_STATUS% EQU 0 (
    echo Frontend: [92mSUCCESS[0m
) else if %FRONTEND_STATUS% EQU 1 (
    echo Frontend: [91mFAILED[0m
) else (
    echo Frontend: [93mSKIPPED[0m
)

echo.

REM Exit with error if any build failed
if %BACKEND_STATUS% EQU 1 (
    echo [91mBuild completed with errors[0m
    exit /b 1
) else if %FRONTEND_STATUS% EQU 1 (
    echo [91mBuild completed with errors[0m
    exit /b 1
) else if %BACKEND_STATUS% EQU 2 (
    if %FRONTEND_STATUS% EQU 2 (
        echo [93mBoth builds were skipped - install required SDKs[0m
        exit /b 2
    )
)

echo [92mAll builds completed successfully![0m
exit /b 0
