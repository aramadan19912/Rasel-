#!/bin/bash

# Build All Projects Script
# This script builds both Backend (.NET) and Frontend (Angular) projects

set -e  # Exit on error

echo "========================================="
echo "Building Rasel Inbox Management System"
echo "========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Track build status
BACKEND_STATUS=0
FRONTEND_STATUS=0

# Build Backend (.NET)
echo -e "${YELLOW}[1/2] Building Backend (.NET 8.0)...${NC}"
echo "========================================="

if command -v dotnet &> /dev/null; then
    cd Backend

    echo "Restoring NuGet packages..."
    dotnet restore OutlookInboxManagement.csproj

    echo ""
    echo "Building Backend project..."
    if dotnet build OutlookInboxManagement.csproj --configuration Release --no-restore; then
        echo -e "${GREEN}✓ Backend build succeeded${NC}"
        BACKEND_STATUS=0
    else
        echo -e "${RED}✗ Backend build failed${NC}"
        BACKEND_STATUS=1
    fi

    cd ..
else
    echo -e "${YELLOW}⚠ .NET SDK not found - skipping Backend build${NC}"
    echo "Install .NET 8.0 SDK from: https://dotnet.microsoft.com/download"
    BACKEND_STATUS=2
fi

echo ""
echo ""

# Build Frontend (Angular)
echo -e "${YELLOW}[2/2] Building Frontend (Angular)...${NC}"
echo "========================================="

if command -v npm &> /dev/null; then
    cd Frontend

    echo "Installing npm packages..."
    npm install

    echo ""
    echo "Building Frontend project..."
    if npm run build; then
        echo -e "${GREEN}✓ Frontend build succeeded${NC}"
        FRONTEND_STATUS=0
    else
        echo -e "${RED}✗ Frontend build failed${NC}"
        FRONTEND_STATUS=1
    fi

    cd ..
else
    echo -e "${YELLOW}⚠ npm not found - skipping Frontend build${NC}"
    echo "Install Node.js from: https://nodejs.org/"
    FRONTEND_STATUS=2
fi

echo ""
echo ""
echo "========================================="
echo "Build Summary"
echo "========================================="

# Print summary
if [ $BACKEND_STATUS -eq 0 ]; then
    echo -e "Backend:  ${GREEN}✓ SUCCESS${NC}"
elif [ $BACKEND_STATUS -eq 1 ]; then
    echo -e "Backend:  ${RED}✗ FAILED${NC}"
else
    echo -e "Backend:  ${YELLOW}⊘ SKIPPED${NC}"
fi

if [ $FRONTEND_STATUS -eq 0 ]; then
    echo -e "Frontend: ${GREEN}✓ SUCCESS${NC}"
elif [ $FRONTEND_STATUS -eq 1 ]; then
    echo -e "Frontend: ${RED}✗ FAILED${NC}"
else
    echo -e "Frontend: ${YELLOW}⊘ SKIPPED${NC}"
fi

echo ""

# Exit with error if any build failed
if [ $BACKEND_STATUS -eq 1 ] || [ $FRONTEND_STATUS -eq 1 ]; then
    echo -e "${RED}Build completed with errors${NC}"
    exit 1
elif [ $BACKEND_STATUS -eq 2 ] && [ $FRONTEND_STATUS -eq 2 ]; then
    echo -e "${YELLOW}Both builds were skipped - install required SDKs${NC}"
    exit 2
else
    echo -e "${GREEN}All builds completed successfully!${NC}"
    exit 0
fi
