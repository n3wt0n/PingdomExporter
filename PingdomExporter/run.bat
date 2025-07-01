@echo off
REM Pingdom Monitor Configuration Exporter
REM This script runs the Pingdom exporter application

echo Pingdom Monitor Configuration Exporter
echo =====================================
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET is not installed or not in PATH
    echo Please install .NET 8.0 or later from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo .NET version:
dotnet --version
echo.

REM Check if appsettings.json exists
if not exist "appsettings.json" (
    echo Warning: appsettings.json not found.
    echo Copying sample configuration...
    copy "appsettings.sample.json" "appsettings.json" >nul
    echo.
    echo Please edit appsettings.json and add your Pingdom API token before running this script again.
    echo.
    pause
    exit /b 1
)

REM Build the application
echo Building application...
dotnet build --configuration Release >nul
if %errorlevel% neq 0 (
    echo Error: Failed to build the application
    pause
    exit /b 1
)

echo Build successful!
echo.

REM Run the application
echo Starting export...
echo.
dotnet run --configuration Release

echo.
echo Export process completed.
pause
