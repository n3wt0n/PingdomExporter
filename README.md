# Pingdom Monitor Configuration Exporter

[![Build and Test](https://github.com/n3wt0n/PingdomExporter/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/n3wt0n/PingdomExporter/actions/workflows/ci-cd.yml)
[![Latest Release](https://img.shields.io/github/v/release/n3wt0n/PingdomExporter?include_prereleases&sort=semver)](https://github.com/n3wt0n/PingdomExporter/releases/latest)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)](#cross-platform-support)

A robust .NET console application that connects to the Pingdom API and downloads monitor configurations for backup, migration, or analysis purposes.

## 🎉 Status: **FULLY FUNCTIONAL**

This application is now **production-ready** with all core features working:
- ✅ Successful authentication with Pingdom API
- ✅ Export of uptime and transaction monitoring checks
- ✅ Automatic gzip decompression and JSON parsing
- ✅ Flexible handling of varying API response structures  
- ✅ Rate limiting and comprehensive error handling
- ✅ Cross-platform compatibility (Windows, Linux, macOS)

## Features

- ✅ **Export all uptime monitoring checks** - Complete list and detailed configurations
- ✅ **Export all transaction monitoring checks (TMS)** - Full transaction scripts and settings
- ✅ **Fetch detailed configuration for each monitor** - Individual check configurations with all parameters
- ✅ **Support for JSON and CSV output formats** - Choose your preferred export format
- ✅ **UptimeRobot bulk import format** - Export checks in a format ready for direct UptimeRobot import
- ✅ **Rate limiting to respect API limits** - Configurable delays between requests
- ✅ **Comprehensive error handling and retry logic** - Robust error recovery and reporting
- ✅ **Configurable export options** - Customizable via configuration files or environment variables
- ✅ **Export summary reporting** - Detailed statistics and error reporting
- ✅ **Secure authentication** - Bearer token authentication with automatic gzip compression
- ✅ **Cross-platform support** - Works on Windows, Linux, and macOS

## 🚀 Quick Start

1. **Get your Pingdom API token** from Settings → API in your Pingdom account
2. **Clone or download** this repository
3. **Configure**: Edit `appsettings.json` and replace `YOUR_PINGDOM_API_TOKEN_HERE` with your token
4. **Run**: Execute `dotnet run` and confirm with 'y' when prompted
5. **Results**: Find exported configurations in the `exports/` directory

That's it! The application will export all your Pingdom monitor configurations.

## Export Modes

The application supports three export modes to balance between detail, performance, and use case:

### Summary Mode (Default)
- **Faster execution** - Only makes summary API calls, avoiding detailed check configuration requests
- **Smaller output files** - Exports basic check information (ID, name, status, type, etc.)
- **Lower API usage** - Respects rate limits and reduces API quota consumption
- **Best for** - Regular backups, inventorying checks, or when full details aren't needed

### Full Mode
- **Complete details** - Makes additional API calls to fetch full configuration for each check
- **Larger output files** - Includes all check parameters, settings, and metadata
- **Higher API usage** - More comprehensive but uses more API quota
- **Best for** - Migration planning, detailed analysis, or complete configuration backup

### UptimeRobot Mode
- **Migration-focused** - Converts Pingdom uptime checks to UptimeRobot import format
- **CSV output only** - Generates a CSV file compatible with UptimeRobot bulk import
- **Uptime checks only** - Only processes uptime checks (transaction checks are not supported by UptimeRobot)
- **Direct import ready** - Output file can be uploaded directly to UptimeRobot for bulk import
- **Best for** - Migrating from Pingdom to UptimeRobot

You can switch between modes using the `--export-mode` parameter or by setting `ExportMode` in your configuration file.

## Prerequisites

- .NET 9.0 or later
- Valid Pingdom API token with appropriate permissions

## Getting Started

### 1. Obtain API Token

1. Log into your Pingdom account
2. Go to **Settings** → **API**
3. Generate a new API token with read permissions
4. Copy the token for configuration

### 2. Configure the Application

Edit the `appsettings.json` file and replace `YOUR_PINGDOM_API_TOKEN_HERE` with your actual API token:

```json
{
  "ApiToken": "your_actual_api_token_here",
  "BaseUrl": "https://api.pingdom.com/api/3.1",
  "OutputDirectory": "exports",
  "ExportUptimeChecks": true,
  "ExportTransactionChecks": true,
  "IncludeTags": true,
  "IncludeTeams": true,
  "OutputFormat": "json",
  "RequestDelayMs": 1000
}
```

### 3. Run the Application

The application supports multiple ways to run:

#### Basic Usage
```bash
# Interactive mode (prompts for confirmation)
dotnet run

# Automatic mode (no prompts)
dotnet run -- --auto
dotnet run -- -y
```

#### Advanced CLI Usage

The application supports comprehensive command-line arguments for automation and CI/CD scenarios:

```bash
# Basic export with API token via CLI
dotnet run -- --api-token "your_token_here" --auto

# Custom output directory and format
dotnet run -- --api-token "your_token" --output-dir "/path/to/exports" --format csv --auto

# Export only uptime checks, skip transaction checks
dotnet run -- --api-token "your_token" --no-transaction --auto

# Export only transaction checks with custom delay
dotnet run -- --api-token "your_token" --no-uptime --delay 2000 --auto

# Summary mode (faster, less data) - only exports basic check information (default)
dotnet run -- --api-token "your_token" --export-mode Summary --auto

# Full mode (detailed) - exports all check details
dotnet run -- --api-token "your_token" --export-mode Full --auto

# UptimeRobot mode - generates CSV file for UptimeRobot bulk import
dotnet run -- --api-token "your_token" --export-mode UptimeRobot --auto

# Verbose output with specific API endpoint
dotnet run -- --api-token "your_token" --base-url "https://api.pingdom.com/api/3.1" --verbose --auto

# Skip tags and teams information
dotnet run -- --api-token "your_token" --no-tags --no-teams --auto

# Include disabled/paused checks in export
dotnet run -- --api-token "your_token" --include-disabled --auto
```

#### All CLI Options

| Short | Long | Description | Example |
|-------|------|-------------|---------|
| `-t` | `--api-token` | Pingdom API token (Bearer token) | `--api-token "abc123"` |
| `-u` | `--base-url` | Pingdom API base URL | `--base-url "https://api.pingdom.com/api/3.1"` |
| `-o` | `--output-dir` | Output directory for exported files | `--output-dir "exports"` |
| `-f` | `--format` | Output format: json, csv, or both | `--format csv` |
| `-m` | `--export-mode` | Export mode: Full, Summary, or UptimeRobot | `--export-mode Summary` |
| | `--no-uptime` | Skip exporting uptime checks | `--no-uptime` |
| | `--no-transaction` | Skip exporting transaction checks | `--no-transaction` |
| | `--no-tags` | Skip tag information | `--no-tags` |
| | `--no-teams` | Skip team assignments | `--no-teams` |
| | `--include-disabled` | Include disabled/paused checks | `--include-disabled` |
| `-d` | `--delay` | Delay between API requests in milliseconds | `--delay 1500` |
| `-y` | `--auto` | Run automatically without prompts | `--auto` |
| `-v` | `--verbose` | Enable verbose output | `--verbose` |
| `-h` | `--help` | Show help and usage information | `--help` |
| | `--version` | Show version information | `--version` |

#### Configuration Priority

The application uses the following configuration priority (highest to lowest):

1. **Command-line arguments** (highest priority)
2. **Environment variables** with `PINGDOM_` prefix
3. **appsettings.json** file
4. **Default values** (lowest priority)

Example showing all three methods:
```bash
# appsettings.json sets defaults
# Environment variable overrides file
export PINGDOM_OutputDirectory="/tmp/pingdom-backup"
# CLI argument overrides everything
dotnet run -- --output-dir "/final/export/path" --auto
```

## Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `ApiToken` | Your Pingdom API token | Required |
| `BaseUrl` | Pingdom API base URL | `https://api.pingdom.com/api/3.1` |
| `OutputDirectory` | Directory for exported files | `exports` |
| `ExportUptimeChecks` | Export uptime monitoring checks | `true` |
| `ExportTransactionChecks` | Export transaction checks | `true` |
| `IncludeTags` | Include tag information | `true` |
| `IncludeTeams` | Include team assignments | `true` |
| `OutputFormat` | Output format: `json`, `csv`, or `both` | `json` |
| `ExportMode` | Export mode: `Full`, `Summary`, or `UptimeRobot` | `Summary` |
| `RequestDelayMs` | Delay between API requests (rate limiting) | `1000` |

## Environment Variables

You can override configuration using environment variables with the `PINGDOM_` prefix:

```bash
# Set API token via environment variable
export PINGDOM_ApiToken="your_token_here"

# Override output directory
export PINGDOM_OutputDirectory="/path/to/exports"

# Run the application
dotnet run
```

## Output Files

The application generates several files in the output directory:

### Uptime Checks
- `uptime-checks-summary_YYYYMMDD-HHMMSS.json` - List of all uptime checks
- `uptime-checks-detailed_YYYYMMDD-HHMMSS.json` - Detailed configuration for each check

### Transaction Checks
- `transaction-checks-summary_YYYYMMDD-HHMMSS.json` - List of all transaction checks
- `transaction-checks-detailed_YYYYMMDD-HHMMSS.json` - Detailed configuration for each check

### Summary
- `export-summary_YYYYMMDD-HHMMSS.json` - Export operation summary with statistics and any errors

### UptimeRobot Import (UptimeRobot mode only)
- `uptimerobot-import_YYYYMMDD-HHMMSS.csv` - CSV file ready for UptimeRobot bulk import

## Sample Output Structure

### Uptime Check
```json
{
  "id": 12345,
  "name": "Website Health Check",
  "hostname": "example.com",
  "status": "up",
  "resolution": 5,
  "type": {
    "http": {
      "url": "/health",
      "encryption": true,
      "port": 443,
      "verify_certificate": true
    }
  },
  "tags": [
    {
      "name": "production",
      "type": "u"
    }
  ]
}
```

### Transaction Check
```json
{
  "id": 67890,
  "name": "User Login Flow",
  "active": true,
  "region": "us-east",
  "interval": 10,
  "steps": [
    {
      "fn": "go_to",
      "args": {
        "url": "https://example.com/login"
      }
    },
    {
      "fn": "fill",
      "args": {
        "input": "username",
        "value": "testuser"
      }
    }
  ]
}
```

## Rate Limiting

The application implements rate limiting to respect Pingdom's API limits:

- Default 1-second delay between API requests
- Configurable via `RequestDelayMs` setting
- Monitors API response headers for rate limit information

## Error Handling

- Individual check failures don't stop the entire export
- Warnings are logged for non-critical errors
- Detailed error information in export summary
- Network timeouts and retries are handled gracefully

## Security Best Practices

- API tokens are not logged or stored in output files
- Use environment variables for sensitive configuration
- Limit API token permissions to read-only access
- Store exported files securely as they contain monitoring configuration

## Troubleshooting

### Common Issues (Now Resolved ✅)

#### Configuration Not Loading
- **Fixed**: Configuration binding now works correctly with `appsettings.json`
- **Fixed**: Environment variables with `PINGDOM_` prefix properly override settings
- **Fixed**: Debug vs Release mode configuration file handling

#### Authentication Errors
- **Fixed**: Bearer token authentication now works correctly
- **Fixed**: HTTP client properly configured with authentication headers
- **Fixed**: Relative URL handling for API endpoints
- Verify your API token is correct and active
- Ensure the token has appropriate read permissions
- Check if your Pingdom account has API access enabled

#### API Response Issues
- **Fixed**: Automatic gzip decompression implemented
- **Fixed**: Flexible JSON parsing handles varying API response structures
- **Fixed**: Custom converters handle both string and object type fields

#### Rate Limit Errors
- **Working**: Rate limiting properly implemented and configurable
- Increase the `RequestDelayMs` setting if needed
- Check your API usage in Pingdom dashboard
- Consider running exports during off-peak hours

#### Network Errors
- **Improved**: Better error handling and timeout management
- Verify internet connectivity
- Check if corporate firewall blocks API access
- Ensure DNS resolution for `api.pingdom.com`

### Recent Fixes Applied

1. **Configuration Binding**: Changed from `configuration.Bind()` to `configuration.Get<T>()` for reliable settings loading
2. **HTTP Authentication**: Moved authentication configuration to DI container with proper header setup
3. **Gzip Decompression**: Added `AutomaticDecompression` with GZip and Deflate support
4. **JSON Parsing**: Implemented `FlexibleTypeConverter` for handling varying API response structures
5. **Error Handling**: Centralized HTTP request handling with consistent error reporting

## Development

To build and run the application:

```bash
# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run

# Run tests (if available)
dotnet test
```

### CI/CD Pipeline

This project includes a streamlined GitHub Actions workflow with three main jobs:

#### 1. Build Job
- ✅ Builds and tests the application on Ubuntu
- ✅ Validates dependencies and code quality
- ✅ Tests application help command
- ✅ Uploads build artifacts for reuse
- ✅ Determines if a release should be triggered

#### 2. Cross-Platform Test Job (Pull Requests Only)
- ✅ Tests compatibility across Windows, Linux, and macOS
- ✅ Validates build success on each platform
- ✅ Runs in parallel after successful build

#### 3. Release Job (Main Branch Only)
- ✅ Downloads artifacts from build job (no rebuild needed)
- ✅ Creates platform-specific releases (Windows, Linux, macOS)
- ✅ Generates semantic versions automatically
- ✅ Creates GitHub releases with downloadable binaries
- ✅ Updates CHANGELOG.md automatically

This design eliminates duplication by having the release job reuse build artifacts instead of rebuilding everything from scratch.

#### Semantic Versioning

The project uses [Conventional Commits](https://conventionalcommits.org/) for automatic semantic versioning:

- `feat:` → Minor version bump (new features)
- `fix:` → Patch version bump (bug fixes)
- `BREAKING CHANGE:` → Major version bump (breaking changes)
- `docs:`, `style:`, `refactor:`, etc. → Patch version bump

Example commit messages:
```bash
git commit -m "feat: add support for custom API endpoints"
git commit -m "fix: handle network timeout errors gracefully"  
git commit -m "docs: update installation instructions"
```

#### Release Process

Releases are automatically created when code is pushed to the `main` branch:

1. **Automatic Version Calculation**: Based on conventional commit messages
2. **Cross-Platform Builds**: Self-contained executables for Windows, Linux, and macOS
3. **GitHub Release**: Tagged release with release notes and downloadable assets
4. **Changelog Update**: Automatically generated and committed back to the repository

To trigger a release:
```bash
git add .
git commit -m "feat: add new export feature"
git push origin main
```

The workflow will automatically:
- Determine the next version (e.g., 0.1.0 → 0.2.0 for a feature)
- Build platform-specific binaries
- Create a GitHub release with assets
- Update the CHANGELOG.md

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License. See LICENSE file for details.

## Support

For issues related to:
- **Pingdom API**: Consult [Pingdom API Documentation](https://docs.pingdom.com/api/)
- **Application bugs**: Create an issue in this repository
- **Feature requests**: Create an issue with enhancement label
