#!/bin/bash

# ============================================
# Rollback Script
# ============================================

set -e

RASEL_DIR="/var/www/rasel-inbox"

echo "Rolling back to previous deployment..."

cd "$RASEL_DIR"

# Find most recent backup
LATEST_BACKUP=$(ls -t | grep backup_ | head -1)

if [ -z "$LATEST_BACKUP" ]; then
    echo "Error: No backup found!"
    exit 1
fi

echo "Rolling back to: $LATEST_BACKUP"

# Stop current services
cd current
docker-compose down || true
cd ..

# Move current to failed
mv current "failed_$(date +%Y%m%d_%H%M%S)"

# Restore from backup
mv "$LATEST_BACKUP" current

# Start services
cd current
docker-compose up -d

echo "✓ Rollback completed"
echo "Please verify the application is working correctly"
