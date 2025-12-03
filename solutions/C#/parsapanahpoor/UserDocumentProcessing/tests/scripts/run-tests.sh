#!/bin/bash

# Script for running all tests with detailed output
# Usage: ./scripts/run-tests.sh

set -e  # Exit on error

echo "======================================"
echo "🧪 Running Test Suite"
echo "======================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK not found${NC}"
    exit 1
fi

echo -e "${YELLOW}📦 Restoring dependencies...${NC}"
dotnet restore

echo ""
echo -e "${YELLOW}🔨 Building solution...${NC}"
dotnet build --no-restore -c Release

echo ""
echo -e "${YELLOW}🧪 Running Unit Tests...${NC}"
dotnet test tests/UserDocumentAPI.Tests/ \
    --no-build \
    -c Release \
    --logger "console;verbosity=detailed" \
    --collect:"XPlat Code Coverage"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Unit tests passed${NC}"
else
    echo -e "${RED}❌ Unit tests failed${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}🧪 Running Integration Tests...${NC}"
dotnet test tests/UserDocumentAPI.IntegrationTests/ \
    --no-build \
    -c Release \
    --logger "console;verbosity=detailed"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Integration tests passed${NC}"
else
    echo -e "${RED}❌ Integration tests failed${NC}"
    exit 1
fi

echo ""
echo -e "${YELLOW}📊 Generating Code Coverage Report...${NC}"
dotnet test \
    --no-build \
    -c Release \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=opencover \
    /p:CoverletOutput=./coverage/

echo ""
echo "======================================"
echo -e "${GREEN}✅ All Tests Completed Successfully!${NC}"
echo "======================================"
