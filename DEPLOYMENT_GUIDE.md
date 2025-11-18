# Rasel Inbox Management System - Deployment Guide

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [VPS Setup](#vps-setup)
3. [GitHub Secrets Configuration](#github-secrets-configuration)
4. [Manual Deployment](#manual-deployment)
5. [CI/CD Deployment](#cicd-deployment)
6. [Post-Deployment](#post-deployment)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software on VPS
- Ubuntu 20.04 LTS or later (recommended)
- Docker Engine 24.0+
- Docker Compose 2.0+
- Git
- Nginx (optional, for reverse proxy)

### Hostinger VPS Requirements
- Minimum 2 GB RAM
- 2 CPU cores
- 40 GB storage
- Open ports: 80, 443, 4000, 9090

---

## VPS Setup

### 1. Connect to Your Hostinger VPS

```bash
ssh root@your-vps-ip
```

### 2. Update System

```bash
apt update && apt upgrade -y
```

### 3. Install Docker

```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Start Docker
systemctl start docker
systemctl enable docker

# Verify installation
docker --version
```

### 4. Install Docker Compose

```bash
# Download Docker Compose
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose

# Make executable
chmod +x /usr/local/bin/docker-compose

# Verify installation
docker-compose --version
```

### 5. Configure Firewall

```bash
# Allow SSH
ufw allow 22/tcp

# Allow HTTP/HTTPS
ufw allow 80/tcp
ufw allow 443/tcp

# Allow application ports
ufw allow 4000/tcp  # Frontend
ufw allow 9090/tcp  # Backend

# Enable firewall
ufw enable
ufw status
```

### 6. Create Deployment Directory

```bash
mkdir -p /var/www/rasel-inbox/current
mkdir -p /var/backups/rasel-inbox
```

### 7. Create Swap File (if RAM < 4GB)

```bash
fallocate -l 2G /swapfile
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile
echo '/swapfile none swap sw 0 0' >> /etc/fstab
```

---

## GitHub Secrets Configuration

### Required Secrets

Add these secrets to your GitHub repository (Settings → Secrets and variables → Actions):

```
VPS_HOST          = your-vps-ip-or-domain
VPS_USERNAME      = root (or your VPS username)
VPS_SSH_KEY       = <Your SSH private key>
VPS_PORT          = 22 (default SSH port)
```

### Generate SSH Key for GitHub Actions

```bash
# On your local machine or VPS
ssh-keygen -t ed25519 -C "github-actions" -f ~/.ssh/github_actions

# Copy public key to VPS authorized_keys
ssh-copy-id -i ~/.ssh/github_actions.pub root@your-vps-ip

# Copy private key content for GitHub secret
cat ~/.ssh/github_actions
# Copy the entire output and add as VPS_SSH_KEY secret
```

---

## Manual Deployment

### 1. Clone Repository on VPS

```bash
cd /var/www/rasel-inbox/current
git clone https://github.com/your-username/Rasel-.git .
```

### 2. Configure Environment

```bash
# Copy environment template
cp .env.example .env

# Edit .env file
nano .env
```

**Important**: Update these values in `.env`:

```env
# Database
DB_SA_PASSWORD=YourStrongPassword123!
DB_CONNECTION_STRING=Server=db,1433;Database=RaselInbox;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;

# JWT
JWT_SECRET=your-super-secret-jwt-key-minimum-32-characters-change-this
JWT_ISSUER=RaselInboxSystem
JWT_AUDIENCE=RaselInboxUsers
JWT_EXPIRY_MINUTES=60

# CORS
CORS_ALLOWED_ORIGINS=http://your-vps-ip:4000,http://your-vps-ip

# Email (Optional)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
```

### 3. Update Frontend Environment

Edit `Frontend/src/environments/environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'http://your-vps-ip:9090',  // ← Change this
  signalRUrl: 'http://your-vps-ip:9090/hubs',  // ← Change this
  enableDebugTools: false,
  logLevel: 'error',
  version: '1.0.0'
};
```

### 4. Deploy Services

```bash
# Build and start all services
docker-compose up -d --build

# Check status
docker-compose ps

# View logs
docker-compose logs -f
```

### 5. Run Database Migrations

```bash
# Execute migrations
docker-compose exec backend dotnet ef database update

# Or manually create database
docker-compose exec db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrongPassword123!" -Q "CREATE DATABASE RaselInbox"
```

---

## CI/CD Deployment

### Automatic Deployment Flow

1. **Push to main/master branch** → Triggers CI/CD
2. **Build Frontend** → Compiles Angular app
3. **Build Backend** → Compiles .NET app
4. **Deploy to VPS** → Copies files and restarts services

### Trigger Deployment

```bash
# Commit and push changes
git add .
git commit -m "Deploy to production"
git push origin main

# Watch GitHub Actions progress at:
# https://github.com/your-username/Rasel-/actions
```

### Manual Workflow Trigger

You can also trigger deployment manually from GitHub:
1. Go to Actions tab
2. Select "CI/CD Pipeline - Hostinger VPS"
3. Click "Run workflow"

---

## Post-Deployment

### 1. Verify Services

```bash
# Check all containers
docker-compose ps

# Test frontend
curl http://localhost:4000

# Test backend
curl http://localhost:9090/health

# Test API
curl http://localhost:9090/api/health
```

### 2. Access Application

- **Frontend**: `http://your-vps-ip:4000`
- **Backend API**: `http://your-vps-ip:9090/api`
- **Swagger Docs**: `http://your-vps-ip:9090/swagger`
- **Nginx Proxy** (if configured): `http://your-vps-ip`

### 3. Create Initial Admin User

Option 1: Via API (Swagger):
1. Go to `http://your-vps-ip:9090/swagger`
2. Use `/api/Auth/register` endpoint
3. Create user with admin role

Option 2: Via Database:
```bash
docker-compose exec db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword"
# Execute SQL to create admin user
```

### 4. Configure SSL (Production)

```bash
# Install Certbot
apt install certbot python3-certbot-nginx -y

# Obtain certificate
certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Auto-renewal is configured automatically
# Test renewal
certbot renew --dry-run
```

### 5. Setup Monitoring

```bash
# Install monitoring tools
docker run -d \
  --name=portainer \
  -p 9000:9000 \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v portainer_data:/data \
  portainer/portainer-ce

# Access at: http://your-vps-ip:9000
```

---

## Maintenance Commands

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f db

# Last 100 lines
docker-compose logs --tail=100 backend
```

### Restart Services

```bash
# Restart all
docker-compose restart

# Restart specific service
docker-compose restart backend
docker-compose restart frontend
```

### Update Application

```bash
# Pull latest code
git pull origin main

# Rebuild and restart
docker-compose down
docker-compose up -d --build
```

### Backup Database

```bash
# Use backup script
/var/www/rasel-inbox/current/.github/deploy-scripts/backup.sh

# Manual backup
docker-compose exec db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword" \
  -Q "BACKUP DATABASE [RaselInbox] TO DISK = '/var/opt/mssql/backup/backup.bak'"
```

### Rollback Deployment

```bash
# Use rollback script
/var/www/rasel-inbox/current/.github/deploy-scripts/rollback.sh
```

---

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker-compose logs [service-name]

# Check container status
docker ps -a

# Inspect container
docker inspect [container-id]

# Remove and recreate
docker-compose down
docker-compose up -d --force-recreate
```

### Database Connection Issues

```bash
# Test database connection
docker-compose exec backend dotnet ef database update

# Check database container
docker-compose logs db

# Connect to database manually
docker-compose exec db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword"
```

### Port Already in Use

```bash
# Find process using port
lsof -i :9090
lsof -i :4000

# Kill process
kill -9 [PID]

# Or use different ports in docker-compose.yml
```

### Out of Memory

```bash
# Check memory usage
docker stats

# Increase swap
dd if=/dev/zero of=/swapfile bs=1M count=4096
mkswap /swapfile
swapon /swapfile

# Or upgrade VPS
```

### Frontend Can't Connect to Backend

1. Check CORS settings in `.env`
2. Verify `environment.prod.ts` has correct API URL
3. Check firewall: `ufw status`
4. Test backend: `curl http://localhost:9090/health`

### SSL Certificate Issues

```bash
# Renew certificate
certbot renew

# Check certificate
certbot certificates

# Reconfigure nginx
certbot --nginx -d yourdomain.com
```

---

## Performance Optimization

### 1. Enable Nginx Caching

Add to nginx configuration:

```nginx
proxy_cache_path /var/cache/nginx levels=1:2 keys_zone=my_cache:10m max_size=1g inactive=60m;
proxy_cache my_cache;
```

### 2. Database Optimization

```sql
-- Create indexes
CREATE INDEX idx_contact_email ON Contacts(Email);
CREATE INDEX idx_message_date ON Messages(DateReceived);

-- Update statistics
EXEC sp_updatestats;
```

### 3. Docker Resource Limits

Edit `docker-compose.yml`:

```yaml
services:
  backend:
    deploy:
      resources:
        limits:
          cpus: '1.5'
          memory: 1G
```

---

## Support

- GitHub Issues: https://github.com/your-username/Rasel-/issues
- Documentation: See README.md
- Logs Location: `/var/www/rasel-inbox/current/logs/`

---

## Security Checklist

- [ ] Changed default database password
- [ ] Generated strong JWT secret
- [ ] Configured firewall (ufw)
- [ ] Enabled SSL/HTTPS
- [ ] Set up regular backups
- [ ] Configured CORS properly
- [ ] Disabled debug mode in production
- [ ] Set up monitoring
- [ ] Regular security updates: `apt update && apt upgrade`

---

**Last Updated**: November 2025
**Version**: 1.0.0
