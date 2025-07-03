# Project Structure

## Main Project Directory

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
PingdomExporter/                 # Repository root
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
├── PingdomExporter.Tests/       # Test project directory (see below)
├── PingdomExporter.HttpTests/   # HTTP test requests for API testing
│   └── checks.http              # HTTP test file
└── README.md                    # Main project documentation
```

## Test Project Structure

```
PingdomExporter.Tests/           # Test project directory
├── Models/
│   ├── CommonTests.cs           # Tests for common models and configuration
│   ├── PingdomCheckTests.cs     # Tests for uptime check models
│   └── TmsCheckTests.cs         # Tests for transaction check models
├── Services/
│   └── CliHandlerTests.cs       # Tests for CLI argument parsing
├── ProgramTests.cs              # Integration and dependency injection tests
└── PingdomExporter.Tests.csproj # Test project file
```

## Test Organization

### Test Coverage by File
- **CliHandlerTests.cs** (42 tests) - CLI argument parsing, validation, help/version commands
- **CommonTests.cs** (47 tests) - Configuration models, error handling, export summaries, UptimeRobot conversion
- **PingdomCheckTests.cs** (67 tests) - Uptime check models, JSON serialization, API response handling
- **TmsCheckTests.cs** (60 tests) - Transaction check models, complex nested objects, step validation
- **ProgramTests.cs** (17 tests) - Dependency injection, HTTP client configuration, integration testing

### Test Categories
- **Unit Tests** - Individual class and method testing
- **Integration Tests** - Service interaction and dependency injection
- **Serialization Tests** - JSON conversion and API response handling
- **Configuration Tests** - Settings validation and CLI argument parsing
- **Model Tests** - Data model validation and edge cases

## Key Architectural Patterns

### Dependency Injection
- Services registered in `Program.cs`
- Configuration injected as singleton
- HTTP clients configured with authentication
- Export and API services as transient

### Configuration Management
- Multiple configuration sources (JSON, environment variables, CLI)
- Hierarchical configuration with precedence
- Validation and error handling

### API Integration
- Dedicated service layer for Pingdom API
- Rate limiting and retry logic
- Comprehensive error handling
- Bearer token authentication

### Export System
- Multiple output formats (JSON, CSV)
- Progress reporting and statistics
- Error tracking and warnings
- Timestamped output files
