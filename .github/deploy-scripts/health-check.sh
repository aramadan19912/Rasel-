#!/bin/bash

# ============================================
# Health Check Script
# ============================================

set -e

echo "Running health checks..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check if services are running
check_service() {
    local service=$1
    local url=$2
    local max_attempts=30
    local attempt=0

    echo -n "Checking $service... "

    while [ $attempt -lt $max_attempts ]; do
        if curl -f -s "$url" > /dev/null 2>&1; then
            echo -e "${GREEN}✓ OK${NC}"
            return 0
        fi
        attempt=$((attempt + 1))
        sleep 2
    done

    echo -e "${RED}✗ FAILED${NC}"
    return 1
}

# Check Frontend
check_service "Frontend" "http://localhost:4000/health"

# Check Backend
check_service "Backend" "http://localhost:9090/health"

# Check Database (via backend connection)
echo -n "Checking Database... "
if docker-compose exec -T db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${DB_SA_PASSWORD}" -Q "SELECT 1" > /dev/null 2>&1; then
    echo -e "${GREEN}✓ OK${NC}"
else
    echo -e "${RED}✗ FAILED${NC}"
    exit 1
fi

# Check Docker containers status
echo ""
echo "Docker Container Status:"
docker-compose ps

echo ""
echo -e "${GREEN}All health checks passed!${NC}"
