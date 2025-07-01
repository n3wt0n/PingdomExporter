# Pingdom Monitor Configuration Exporter

A .NET console application that connects to the Pingdom API and downloads monitor configurations for backup, migration, or analysis purposes.

## Features

- ✅ Export all uptime monitoring checks
- ✅ Export all transaction monitoring checks (TMS)
- ✅ Fetch detailed configuration for each monitor
- ✅ Support for JSON and CSV output formats
- ✅ Rate limiting to respect API limits
- ✅ Comprehensive error handling and retry logic
- ✅ Configurable export options
- ✅ Export summary reporting

## Prerequisites

- .NET 8.0 or later
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

```bash
# Interactive mode (prompts for confirmation)
dotnet run

# Automatic mode (no prompts)
dotnet run -- --auto

# Alternative automatic mode
dotnet run -- -y
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

### Authentication Errors
- Verify your API token is correct and active
- Ensure the token has appropriate read permissions
- Check if your Pingdom account has API access enabled

### Rate Limit Errors
- Increase the `RequestDelayMs` setting
- Check your API usage in Pingdom dashboard
- Consider running exports during off-peak hours

### Network Errors
- Verify internet connectivity
- Check if corporate firewall blocks API access
- Ensure DNS resolution for `api.pingdom.com`

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
