#!/bin/bash

# Pingdom Monitor Configuration Exporter
# This script runs the Pingdom exporter application

set -e

echo "Pingdom Monitor Configuration Exporter"
echo "====================================="
echo

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET is not installed or not in PATH${NC}"
    echo -e "${YELLOW}Please install .NET 9.0 or later from https://dotnet.microsoft.com/download${NC}"
    exit 1
fi

echo -e "${CYAN}.NET version:${NC}"
dotnet --version
echo

# Check if appsettings.json exists
if [ ! -f "appsettings.json" ]; then
    echo -e "${YELLOW}Warning: appsettings.json not found.${NC}"
    
    if [ -f "appsettings.sample.json" ]; then
        echo -e "${YELLOW}Copying sample configuration...${NC}"
        cp "appsettings.sample.json" "appsettings.json"
        echo
        echo -e "${YELLOW}Please edit appsettings.json and add your Pingdom API token before running this script again.${NC}"
        echo
        exit 1
    else
        echo -e "${RED}Error: Sample configuration file not found.${NC}"
        exit 1
    fi
fi

# Parse command line arguments
AUTO_MODE=false
API_TOKEN=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --auto|-y)
            AUTO_MODE=true
            shift
            ;;
        --token|-t)
            API_TOKEN="$2"
            shift 2
            ;;
        --help|-h)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  --auto, -y         Run in automatic mode (no prompts)"
            echo "  --token, -t TOKEN  Set API token"
            echo "  --help, -h         Show this help message"
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Update API token if provided
if [ -n "$API_TOKEN" ]; then
    echo -e "${CYAN}Updating API token in configuration...${NC}"
    
    # Use sed to replace the API token in the JSON file
    if command -v jq &> /dev/null; then
        # Use jq if available for proper JSON manipulation
        jq --arg token "$API_TOKEN" '.ApiToken = $token' appsettings.json > appsettings.json.tmp && mv appsettings.json.tmp appsettings.json
    else
        # Fallback to sed (less robust but works for simple cases)
        sed -i.bak "s/\"ApiToken\".*$/\"ApiToken\": \"$API_TOKEN\",/" appsettings.json
    fi
    
    echo -e "${GREEN}Configuration updated successfully.${NC}"
    echo
fi

# Check API token validity
if grep -q "YOUR_PINGDOM_API_TOKEN_HERE\|test-token-replace-with-real-token" appsettings.json; then
    echo -e "${RED}Error: Please set a valid Pingdom API token in appsettings.json${NC}"
    echo -e "${YELLOW}You can also use: $0 --token 'your-token-here'${NC}"
    echo
    exit 1
fi

# Build the application
echo -e "${CYAN}Building application...${NC}"
if ! dotnet build --configuration Release --verbosity quiet > /dev/null 2>&1; then
    echo -e "${RED}Error: Failed to build the application${NC}"
    exit 1
fi

echo -e "${GREEN}Build successful!${NC}"
echo

# Run the application
echo -e "${CYAN}Starting export...${NC}"
echo

if [ "$AUTO_MODE" = true ]; then
    dotnet run --configuration Release -- --auto
else
    dotnet run --configuration Release
fi

EXIT_CODE=$?

echo
if [ $EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}Export completed successfully! 🎉${NC}"
    
    # Show output directory contents
    OUTPUT_DIR=$(grep -o '"OutputDirectory"[^"]*"[^"]*"' appsettings.json | cut -d'"' -f4)
    if [ -z "$OUTPUT_DIR" ]; then
        OUTPUT_DIR="exports"
    fi
    
    if [ -d "$OUTPUT_DIR" ]; then
        echo -e "${CYAN}Exported files are available in: $OUTPUT_DIR${NC}"
        echo -e "${CYAN}Recent files:${NC}"
        ls -lt "$OUTPUT_DIR"/*.json 2>/dev/null | head -5 | while read -r line; do
            echo "  $line"
        done
    fi
else
    echo -e "${YELLOW}Export process completed with errors. Exit code: $EXIT_CODE${NC}"
fi

exit $EXIT_CODE
