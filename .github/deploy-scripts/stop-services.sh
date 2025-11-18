#!/bin/bash

# ============================================
# Stop Services Script
# ============================================

set -e

echo "Stopping Rasel Inbox services..."

DEPLOY_DIR="/var/www/rasel-inbox/current"

if [ -d "$DEPLOY_DIR" ]; then
    cd "$DEPLOY_DIR"

    if [ -f docker-compose.yml ]; then
        docker-compose down
        echo "✓ Services stopped"
    else
        echo "⚠ docker-compose.yml not found"
    fi
else
    echo "⚠ Deployment directory not found"
fi

echo "Done."
