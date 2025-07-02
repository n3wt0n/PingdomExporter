# Project Structure

```
PingdomExporter/                 # Main project directory
├── Models/
│   ├── PingdomCheck.cs          # Data models for uptime checks
│   ├── TmsCheck.cs              # Data models for transaction checks
│   └── Common.cs                # Common models and configuration
├── Services/
│   ├── PingdomApiService.cs     # API communication service
│   ├── ExportService.cs         # Export logic and file operations
│   └── CliHandler.cs            # Command-line interface handler
├── Program.cs                   # Main application entry point
├── appsettings.json             # Configuration file (with your API token)
├── appsettings.sample.json      # Sample configuration template
├── PROJECT_STRUCTURE.md         # This documentation file
├── .gitignore                   # Git ignore file (protects sensitive data)
└── PingdomExporter.csproj       # Project file
```

## Root Directory Structure

```
c:\Prj\pingdomexport/
├── .git/                        # Git repository metadata
├── .github/                     # GitHub workflows and templates
├── .gitignore                   # Root git ignore file
├── .releaserc.json              # Semantic release configuration
├── CHANGELOG.md                 # Project changelog
├── CONTRIBUTING.md              # Contribution guidelines
├── LICENSE                      # Project license
├── package.json                 # NPM package configuration (for CI/CD)
├── pingdomexport.sln            # Visual Studio solution file
├── PingdomExporter/             # Main project directory (see above)
├── README.md                    # Main project documentation
└── Tests/
    └── checks.http              # HTTP test requests for API testing
```

## Features Implemented

✅ **Complete Pingdom API Integration**
- Uptime checks export with full configuration details
- Transaction checks (TMS) export with steps and metadata
- Proper authentication using Bearer tokens
- Rate limiting to respect API limits
- Comprehensive error handling

✅ **Multiple Output Formats**
- JSON format (detailed, structured data)
- CSV format (for spreadsheet analysis)
- Configurable output options

✅ **Robust Configuration**
- JSON configuration file
- Environment variable support
- Command-line argument support
- Validation and error messages

✅ **Command-Line Interface**
- Comprehensive CLI argument support with System.CommandLine
- Help documentation and version information
- Configuration via CLI, environment variables, or config files
- Auto mode for unattended execution
- Verbose output mode for debugging

✅ **Export Features**
- Detailed and summary exports
- Progress reporting
- Export summaries with statistics
- Error and warning tracking
- Timestamped output files

✅ **Cross-Platform Support**
- .NET 9.0 for Windows, Linux, macOS
- Cross-platform file handling

✅ **Security Best Practices**
- API tokens not logged or stored in exports
- .gitignore protects sensitive configuration
- Environment variable support for CI/CD

✅ **Production Ready**
- Comprehensive error handling
- Retry logic for transient failures
- Rate limiting implementation
- Detailed logging and progress reporting
- Graceful handling of partial failures

## Quick Start

1. **Set up API token:**
   ```bash
   # Copy sample configuration
   cp appsettings.sample.json appsettings.json
   
   # Edit appsettings.json and add your Pingdom API token
   ```

2. **Run the application:**
   ```bash
   # Basic usage
   dotnet run
   
   # With CLI arguments (auto mode)
   dotnet run -- --api-token "your_token" --auto
   
   # Show help
   dotnet run -- --help
   
   # Verbose output with custom settings
   dotnet run -- --api-token "token" --output-dir "backup" --verbose --auto
   ```

3. **View exported data:**
   ```bash
   # Check the exports/ directory for JSON files
   ls exports/
   ```

## API Endpoints Used

- `GET /checks` - List all uptime checks
- `GET /checks/{id}` - Get detailed uptime check configuration
- `GET /tms/check` - List all transaction checks
- `GET /tms/check/{id}` - Get detailed transaction check configuration

## Data Exported

### Uptime Checks
- Basic check information (ID, name, status, etc.)
- Check type configuration (HTTP, TCP, DNS, etc.)
- Monitoring settings (resolution, thresholds, etc.)
- Alert configuration (contacts, teams, notifications)
- Tags and metadata

### Transaction Checks (TMS)
- Check details and status
- Step-by-step transaction scripts
- Metadata and browser configuration
- Alert and notification settings
- Performance and monitoring configuration

## Error Handling

The application handles various error scenarios:
- Invalid API tokens
- Network connectivity issues
- API rate limiting
- Individual check fetch failures
- File system errors
- Configuration validation

Individual check failures don't stop the entire export process, allowing you to get partial results even if some checks can't be retrieved.

## Configuration Options

All configuration can be set via:
1. `appsettings.json` file
2. Environment variables (prefix: `PINGDOM_`)
3. Command line arguments

Key settings:
- `ApiToken` - Your Pingdom API token (required)
- `OutputDirectory` - Where to save exported files
- `OutputFormat` - "json", "csv", or "both"
- `RequestDelayMs` - Delay between API calls for rate limiting
- `ExportUptimeChecks` - Enable/disable uptime check export
- `ExportTransactionChecks` - Enable/disable transaction check export

## Technical Dependencies

### NuGet Packages
- **Microsoft.Extensions.Configuration** (9.0.6) - Configuration framework
- **Microsoft.Extensions.Configuration.CommandLine** (9.0.6) - CLI configuration support
- **Microsoft.Extensions.Configuration.EnvironmentVariables** (9.0.6) - Environment variable support
- **Microsoft.Extensions.Configuration.Json** (9.0.6) - JSON configuration file support
- **Microsoft.Extensions.Http** (9.0.6) - HTTP client factory and services
- **Newtonsoft.Json** (13.0.3) - JSON serialization/deserialization
- **System.CommandLine** (2.0.0-beta4.22272.1) - Modern command-line interface

### Framework
- **.NET 9.0** - Target framework for cross-platform compatibility

## Next Steps

This application provides a solid foundation for Pingdom monitoring configuration management. You could extend it to:

1. **Import functionality** - Create monitors from exported configurations
2. **Monitoring comparison** - Compare configurations between environments
3. **Automated backups** - Schedule regular exports
4. **Configuration validation** - Validate exported configurations
5. **Migration tools** - Migrate between Pingdom accounts
6. **Reporting** - Generate reports from exported data
