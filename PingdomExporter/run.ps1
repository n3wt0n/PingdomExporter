# Pingdom Monitor Configuration Exporter PowerShell Script
# This script runs the Pingdom exporter application with enhanced error handling

param(
    [switch]$Auto,
    [string]$ApiToken,
    [string]$OutputDir = "exports"
)

Write-Host "Pingdom Monitor Configuration Exporter" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host

# Check if dotnet is installed
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command not found"
    }
    Write-Host ".NET version: $dotnetVersion" -ForegroundColor Cyan
    Write-Host
} catch {
    Write-Host "Error: .NET is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install .NET 8.0 or later from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Check if appsettings.json exists
if (-not (Test-Path "appsettings.json")) {
    Write-Host "Warning: appsettings.json not found." -ForegroundColor Yellow
    
    if (Test-Path "appsettings.sample.json") {
        Write-Host "Copying sample configuration..." -ForegroundColor Yellow
        Copy-Item "appsettings.sample.json" "appsettings.json"
        Write-Host
    } else {
        Write-Host "Error: Sample configuration file not found." -ForegroundColor Red
        exit 1
    }
}

# Update API token if provided
if ($ApiToken) {
    Write-Host "Updating API token in configuration..." -ForegroundColor Cyan
    try {
        $config = Get-Content "appsettings.json" | ConvertFrom-Json
        $config.ApiToken = $ApiToken
        if ($OutputDir) {
            $config.OutputDirectory = $OutputDir
        }
        $config | ConvertTo-Json -Depth 10 | Set-Content "appsettings.json"
        Write-Host "Configuration updated successfully." -ForegroundColor Green
        Write-Host
    } catch {
        Write-Host "Error updating configuration: $_" -ForegroundColor Red
        exit 1
    }
}

# Validate API token
try {
    $config = Get-Content "appsettings.json" | ConvertFrom-Json
    if ($config.ApiToken -eq "YOUR_PINGDOM_API_TOKEN_HERE" -or $config.ApiToken -eq "test-token-replace-with-real-token") {
        Write-Host "Error: Please set a valid Pingdom API token in appsettings.json" -ForegroundColor Red
        Write-Host "You can also use: .\run.ps1 -ApiToken 'your-token-here'" -ForegroundColor Yellow
        Write-Host
        Read-Host "Press Enter to exit"
        exit 1
    }
} catch {
    Write-Host "Error reading configuration: $_" -ForegroundColor Red
    exit 1
}

# Build the application
Write-Host "Building application..." -ForegroundColor Cyan
try {
    $buildResult = dotnet build --configuration Release --verbosity quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build output:" -ForegroundColor Yellow
        Write-Host $buildResult -ForegroundColor Yellow
        throw "Build failed with exit code $LASTEXITCODE"
    }
    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host
} catch {
    Write-Host "Error: Failed to build the application" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Run the application
Write-Host "Starting export..." -ForegroundColor Cyan
Write-Host

try {
    $arguments = @("run", "--configuration", "Release")
    if ($Auto) {
        $arguments += "--", "--auto"
    }
    
    $process = Start-Process -FilePath "dotnet" -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    Write-Host
    if ($process.ExitCode -eq 0) {
        Write-Host "Export completed successfully! 🎉" -ForegroundColor Green
        
        # Show output directory
        if (Test-Path $config.OutputDirectory) {
            Write-Host "Exported files are available in: $($config.OutputDirectory)" -ForegroundColor Cyan
            $files = Get-ChildItem $config.OutputDirectory -Filter "*.json" | Sort-Object LastWriteTime -Descending | Select-Object -First 5
            if ($files) {
                Write-Host "Recent files:" -ForegroundColor Cyan
                foreach ($file in $files) {
                    Write-Host "  - $($file.Name) ($($file.LastWriteTime.ToString('yyyy-MM-dd HH:mm')))" -ForegroundColor Gray
                }
            }
        }
    } else {
        Write-Host "Export process completed with errors. Exit code: $($process.ExitCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Error running application: $_" -ForegroundColor Red
    exit 1
}

if (-not $Auto) {
    Write-Host
    Read-Host "Press Enter to exit"
}
