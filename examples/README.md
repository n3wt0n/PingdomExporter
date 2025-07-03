# Example Output Files

This directory contains sample output files to help you understand what the Pingdom Exporter generates.

## File Types

### Summary Files
These contain lists of all checks with basic information:
- `uptime-checks-summary-example.json` - List of uptime monitoring checks
- `transaction-checks-summary-example.json` - List of transaction monitoring checks

### Detailed Files (Full Mode Only)
These contain complete configuration for each check:
- `uptime-checks-detailed-example.json` - Full uptime check configurations
- `transaction-checks-detailed-example.json` - Full transaction check configurations

### Export Summary
- `export-summary-example.json` - Operation summary with statistics and errors

### UptimeRobot Import (UptimeRobot Mode Only)
- `uptimerobot-import-example.csv` - CSV file ready for UptimeRobot bulk import

## File Naming Convention

All files include timestamps in the format: `YYYYMMDD-HHMMSS`

Example: `uptime-checks-summary_20250703-143022.json`

## Understanding the Data

### Uptime Checks
- **Basic monitoring** for websites, servers, and services
- **HTTP/HTTPS, TCP, UDP, ICMP** protocols supported
- **Response time and availability** tracking

### Transaction Checks
- **Complex user workflows** like login flows, checkout processes
- **Multi-step browser automation** with detailed scripts
- **Performance monitoring** for critical user journeys

### Export Modes
- **Summary Mode**: Fast export with basic check information
- **Full Mode**: Complete export with all configuration details
- **UptimeRobot Mode**: Converts data for UptimeRobot import
