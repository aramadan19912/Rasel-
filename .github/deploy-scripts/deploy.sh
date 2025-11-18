#!/bin/bash

# ============================================
# Rasel Inbox Management System
# Main Deployment Script
# ============================================

set -e  # Exit on error

echo "============================================"
echo "  RASEL INBOX DEPLOYMENT"
echo "============================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Deployment directory
DEPLOY_DIR="/var/www/rasel-inbox/current"

cd "$DEPLOY_DIR"

# Check if .env exists
if [ ! -f .env ]; then
    echo -e "${RED}Error: .env file not found!${NC}"
    echo "Please create .env file from .env.example"
    exit 1
fi

echo -e "${GREEN}✓ Environment file found${NC}"

# Pull latest images (if using pre-built images)
# echo "Pulling Docker images..."
# docker-compose pull

# Build and start services
echo -e "${YELLOW}Building and starting services...${NC}"
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# Wait for services to start
echo -e "${YELLOW}Waiting for services to start...${NC}"
sleep 15

# Check service health
echo -e "${YELLOW}Checking service health...${NC}"
./deploy-scripts/health-check.sh

# Run database migrations (if needed)
echo -e "${YELLOW}Running database migrations...${NC}"
docker-compose exec -T backend dotnet ef database update || echo -e "${YELLOW}⚠ No migrations to run or migrations failed${NC}"

# Cleanup old images
echo -e "${YELLOW}Cleaning up old Docker images...${NC}"
docker image prune -f

echo ""
echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}  DEPLOYMENT COMPLETED SUCCESSFULLY!${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""
echo "Services:"
echo "  Frontend: http://$(hostname -I | awk '{print $1}'):4000"
echo "  Backend API: http://$(hostname -I | awk '{print $1}'):9090/api"
echo "  Swagger: http://$(hostname -I | awk '{print $1}'):9090/swagger"
echo "  Nginx Proxy: http://$(hostname -I | awk '{print $1}')"
echo ""
echo "Logs:"
echo "  View all: docker-compose logs -f"
echo "  Frontend: docker-compose logs -f frontend"
echo "  Backend: docker-compose logs -f backend"
echo "  Database: docker-compose logs -f db"
echo ""
