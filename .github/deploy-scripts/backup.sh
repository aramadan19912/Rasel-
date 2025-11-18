#!/bin/bash

# ============================================
# Backup Script
# ============================================

set -e

BACKUP_DIR="/var/backups/rasel-inbox"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_PATH="$BACKUP_DIR/backup_$TIMESTAMP"

mkdir -p "$BACKUP_PATH"

echo "Creating backup at $BACKUP_PATH..."

# Backup database
echo "Backing up database..."
docker-compose exec -T db /opt/mssql-tools/bin/sqlcmd \
    -S localhost -U sa -P "${DB_SA_PASSWORD}" \
    -Q "BACKUP DATABASE [RaselInbox] TO DISK = '/var/opt/mssql/backup/RaselInbox_$TIMESTAMP.bak'"

# Copy backup file from container
docker cp rasel-db:/var/opt/mssql/backup/RaselInbox_$TIMESTAMP.bak "$BACKUP_PATH/"

# Backup uploaded files
echo "Backing up uploaded files..."
docker cp rasel-backend:/app/uploads "$BACKUP_PATH/"

# Backup configuration
echo "Backing up configuration..."
cp .env "$BACKUP_PATH/.env"
cp docker-compose.yml "$BACKUP_PATH/"

# Create archive
echo "Creating archive..."
cd "$BACKUP_DIR"
tar -czf "backup_$TIMESTAMP.tar.gz" "backup_$TIMESTAMP"
rm -rf "backup_$TIMESTAMP"

# Keep only last 7 backups
ls -t | grep backup_ | tail -n +8 | xargs -r rm -f

echo "✓ Backup completed: $BACKUP_DIR/backup_$TIMESTAMP.tar.gz"
