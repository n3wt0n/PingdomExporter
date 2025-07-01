# Changelog

All notable changes to this project will be documented in this file. See [Conventional Commits](https://conventionalcommits.org) for commit guidelines.

## [0.0.4](https://github.com/n3wt0n/PingdomExporter/compare/v0.0.3...v0.0.4) (2025-07-01)

### 🐛 Bug Fixes

* added missing dependency for semantic release ([d272e38](https://github.com/n3wt0n/PingdomExporter/commit/d272e38cc6ea4b4c68a36bd7a514523c09497685))
* updated semantic release version and config ([1315b1d](https://github.com/n3wt0n/PingdomExporter/commit/1315b1da83cd7f77947a30e114f8701a751b2e73))
* fixed configuration binding issue with appsettings.json
* resolved HTTP authentication header problems with Pingdom API
* implemented automatic gzip decompression for API responses
* fixed JSON deserialization issues with flexible CheckType handling
* centralized HTTP request handling with proper rate limiting

### ✨ Improvements

* Enhanced error handling and debugging output
* Improved configuration loading with environment variable support
* Added debug mode configuration file copying
* Centralized HTTP client configuration in dependency injection
* Flexible JSON parsing for varying API response structures

## [Unreleased]

### 🚀 Features

- ✅ **Core Functionality Working**
  - Export uptime monitoring checks from Pingdom API
  - Export transaction monitoring checks (TMS) 
  - Fetch detailed configuration for each monitor
  - Multiple output formats (JSON and CSV)
  - Rate limiting to respect API limits
  - Comprehensive error handling and retry logic
  - Cross-platform support (Windows, Linux, macOS)

- ✅ **Configuration & Security**
  - Configurable export options via appsettings.json
  - Environment variable overrides with PINGDOM_ prefix
  - Secure API token handling (not logged or stored in output)
  - Debug vs Release mode configuration handling

- ✅ **Authentication & HTTP**
  - Bearer token authentication with Pingdom API
  - Automatic gzip compression/decompression
  - Centralized HTTP client configuration
  - Proper relative URL handling for API endpoints
  - Automatic rate limiting between requests

- ✅ **Data Handling**
  - Flexible JSON deserialization for varying API response structures
  - Support for both simple string and complex object type fields
  - Robust error handling for malformed API responses
  - Export summary reporting with statistics and errors

### 📖 Documentation

- Complete README with usage instructions
- API documentation and examples  
- Platform-specific run scripts
- Configuration options documentation
- Troubleshooting guide
- CI/CD pipeline documentation

### 🏗️ Build System

- .NET 9.0 project structure
- NuGet package dependencies (Microsoft.Extensions.*, Newtonsoft.Json)
- Cross-platform build support
- GitHub Actions CI/CD pipeline with semantic versioning
- Automatic release creation with platform-specific binaries

### 🔧 Technical Implementation

- **HTTP Client**: Configured with automatic decompression and proper authentication
- **Configuration**: Uses Microsoft.Extensions.Configuration with JSON, environment variables, and command-line support
- **Dependency Injection**: Proper DI container setup for services and HTTP clients
- **JSON Parsing**: Custom converters for flexible API response handling
- **Rate Limiting**: Configurable delays between API requests
- **Error Handling**: Comprehensive exception handling with user-friendly error messages
