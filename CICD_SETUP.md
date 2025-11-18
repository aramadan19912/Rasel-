# CI/CD Setup Guide for Hostinger VPS

## Quick Start

This guide will help you set up automated deployment to your Hostinger VPS.

---

## 🎯 Overview

**Deployment Configuration:**
- **Frontend**: Port 4000 (Angular Application)
- **Backend**: Port 9090 (.NET API)
- **Database**: Port 1433 (SQL Server)
- **Nginx Proxy**: Port 80/443 (Optional)

---

## 📋 Prerequisites

### 1. GitHub Repository
- Your code must be in a GitHub repository
- You need admin access to configure secrets

### 2. Hostinger VPS
- Ubuntu 20.04+ installed
- Root or sudo access
- Minimum 2 GB RAM
- Docker and Docker Compose installed

---

## 🚀 Setup Steps

### Step 1: Prepare Your VPS

SSH into your Hostinger VPS:

```bash
ssh root@your-vps-ip
```

Run the one-time setup script:

```bash
# Update system
apt update && apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh
systemctl start docker
systemctl enable docker

# Install Docker Compose
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

# Create deployment directory
mkdir -p /var/www/rasel-inbox/current

# Configure firewall
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 4000/tcp
ufw allow 9090/tcp
ufw enable
```

### Step 2: Generate SSH Key for GitHub Actions

On your **local machine** or VPS:

```bash
# Generate SSH key pair
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ~/.ssh/github_deploy

# Add public key to VPS
ssh-copy-id -i ~/.ssh/github_deploy.pub root@your-vps-ip

# Display private key (you'll need this for GitHub)
cat ~/.ssh/github_deploy
```

**Important**: Copy the entire private key output (including BEGIN and END lines).

### Step 3: Configure GitHub Secrets

1. Go to your GitHub repository
2. Navigate to: **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add the following secrets:

| Secret Name | Value | Example |
|------------|-------|---------|
| `VPS_HOST` | Your VPS IP or domain | `123.45.67.89` or `yourdomain.com` |
| `VPS_USERNAME` | VPS username (usually root) | `root` |
| `VPS_SSH_KEY` | Private SSH key from Step 2 | `-----BEGIN OPENSSH PRIVATE KEY-----...` |
| `VPS_PORT` | SSH port (default 22) | `22` |

### Step 4: Configure Environment Variables

Create `.env` file on your VPS:

```bash
# On VPS
cd /var/www/rasel-inbox/current
nano .env
```

Add configuration (use `.env.example` as template):

```env
# Database
DB_SA_PASSWORD=YourStrongPassword123!
DB_CONNECTION_STRING=Server=db,1433;Database=RaselInbox;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;

# JWT
JWT_SECRET=your-super-secret-jwt-key-minimum-32-characters
JWT_ISSUER=RaselInboxSystem
JWT_AUDIENCE=RaselInboxUsers
JWT_EXPIRY_MINUTES=60

# CORS (Important: Add your VPS IP)
CORS_ALLOWED_ORIGINS=http://YOUR_VPS_IP:4000,http://YOUR_VPS_IP

# Other settings
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
```

**Replace `YOUR_VPS_IP` with your actual VPS IP address!**

### Step 5: Update Frontend Configuration

Update the API URL in your frontend configuration:

```bash
# Edit environment file
nano Frontend/src/environments/environment.prod.ts
```

Change to:

```typescript
export const environment = {
  production: true,
  apiUrl: 'http://YOUR_VPS_IP:9090',  // ← Replace YOUR_VPS_IP
  signalRUrl: 'http://YOUR_VPS_IP:9090/hubs',  // ← Replace YOUR_VPS_IP
  enableDebugTools: false,
  logLevel: 'error',
  version: '1.0.0'
};
```

Commit this change:

```bash
git add Frontend/src/environments/environment.prod.ts
git commit -m "Update production API URL"
git push origin main
```

### Step 6: Test Deployment

Push to main branch or manually trigger workflow:

```bash
git add .
git commit -m "Test deployment"
git push origin main
```

Monitor deployment at: `https://github.com/YOUR_USERNAME/Rasel-/actions`

---

## 🔄 Deployment Workflow

### Automatic Deployment

Every push to `main` or `master` branch triggers:

1. ✅ **Build Frontend** (Angular)
2. ✅ **Build Backend** (.NET)
3. ✅ **Deploy to VPS** (Docker Compose)
4. ✅ **Health Checks** (Verify services)

### Manual Deployment

Trigger deployment manually:

1. Go to GitHub repository
2. Click **Actions** tab
3. Select **CI/CD Pipeline - Hostinger VPS**
4. Click **Run workflow**
5. Choose branch (main/master)
6. Click **Run workflow** button

---

## 📊 Monitoring Deployment

### Via GitHub Actions

1. Go to **Actions** tab in your repository
2. Click on the running workflow
3. View real-time logs for each step

### Via VPS Logs

```bash
# SSH into VPS
ssh root@your-vps-ip

# View deployment logs
cd /var/www/rasel-inbox/current
docker-compose logs -f

# Check specific service
docker-compose logs -f backend
docker-compose logs -f frontend
```

---

## 🧪 Verify Deployment

After deployment completes, verify services:

### 1. Check Docker Containers

```bash
docker-compose ps
```

Expected output:
```
NAME                COMMAND                  STATUS              PORTS
rasel-backend       "dotnet OutlookInbox…"   Up 2 minutes        0.0.0.0:9090->9090/tcp
rasel-frontend      "nginx -g 'daemon of…"   Up 2 minutes        0.0.0.0:4000->4000/tcp
rasel-db            "/opt/mssql/bin/perm…"   Up 2 minutes        0.0.0.0:1433->1433/tcp
```

### 2. Test Endpoints

```bash
# Frontend
curl http://your-vps-ip:4000

# Backend health
curl http://your-vps-ip:9090/health

# Backend API
curl http://your-vps-ip:9090/api
```

### 3. Access in Browser

- **Frontend**: `http://your-vps-ip:4000`
- **Backend API**: `http://your-vps-ip:9090/api`
- **Swagger UI**: `http://your-vps-ip:9090/swagger`

---

## 🛠️ Common Commands

### Restart Services

```bash
cd /var/www/rasel-inbox/current
docker-compose restart
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
```

### Stop Services

```bash
docker-compose down
```

### Start Services

```bash
docker-compose up -d
```

### Rebuild Containers

```bash
docker-compose down
docker-compose up -d --build
```

### Database Backup

```bash
./.github/deploy-scripts/backup.sh
```

### Rollback Deployment

```bash
./.github/deploy-scripts/rollback.sh
```

---

## ⚙️ Configuration Files

### Key Configuration Files

```
/var/www/rasel-inbox/current/
├── .env                          # Environment variables
├── docker-compose.yml            # Docker orchestration
├── Frontend/
│   ├── Dockerfile               # Frontend container config
│   ├── nginx.conf              # Frontend web server config
│   └── src/environments/
│       └── environment.prod.ts  # Frontend API URL
└── Backend/
    ├── Dockerfile               # Backend container config
    └── appsettings.Production.json
```

### Environment Variables (`.env`)

All sensitive configuration is in `.env`. Never commit this file!

**Important variables:**
- `DB_SA_PASSWORD`: Database password
- `JWT_SECRET`: Authentication secret (min 32 chars)
- `CORS_ALLOWED_ORIGINS`: Frontend URLs
- `SMTP_*`: Email configuration (optional)

---

## 🐛 Troubleshooting

### Deployment Fails

**1. Check GitHub Actions logs**
- Go to Actions tab
- Click failed workflow
- Review error messages

**2. SSH to VPS and check**
```bash
ssh root@your-vps-ip
cd /var/www/rasel-inbox/current
docker-compose logs
```

### Containers Not Starting

```bash
# Check container status
docker-compose ps -a

# View detailed logs
docker-compose logs backend

# Restart problematic service
docker-compose restart backend
```

### Database Connection Errors

```bash
# Check database is running
docker-compose ps db

# Test database connection
docker-compose exec backend dotnet ef database update

# Reset database container
docker-compose down
docker-compose up -d db
```

### Port Already in Use

```bash
# Find what's using the port
lsof -i :9090

# Kill the process
kill -9 [PID]

# Restart docker-compose
docker-compose up -d
```

### Frontend Can't Reach Backend

1. Check `environment.prod.ts` has correct VPS IP
2. Verify CORS in `.env` includes frontend URL
3. Check firewall: `ufw status`
4. Test backend: `curl http://localhost:9090/health`

---

## 🔒 Security Best Practices

1. **Change Default Passwords**
   - Database SA password
   - JWT secret
   - Admin user password

2. **Configure Firewall**
   ```bash
   ufw enable
   ufw allow 22,80,443,4000,9090/tcp
   ```

3. **Enable SSL/HTTPS**
   ```bash
   certbot --nginx -d yourdomain.com
   ```

4. **Regular Updates**
   ```bash
   apt update && apt upgrade -y
   docker-compose pull
   docker-compose up -d
   ```

5. **Monitor Logs**
   ```bash
   docker-compose logs -f
   ```

---

## 📈 Performance Tips

1. **Increase Docker Resources**
   - Upgrade VPS if needed (4GB RAM recommended)
   - Add swap: `fallocate -l 2G /swapfile`

2. **Enable Nginx Caching**
   - Edit `nginx/conf.d/rasel-inbox.conf`
   - Add caching directives

3. **Database Optimization**
   - Regular backups
   - Index optimization
   - Query performance monitoring

---

## 📞 Support

**GitHub Issues**: [Create Issue](https://github.com/YOUR_USERNAME/Rasel-/issues)

**Documentation**:
- [Deployment Guide](./DEPLOYMENT_GUIDE.md)
- [Docker Documentation](https://docs.docker.com/)
- [Nginx Documentation](https://nginx.org/en/docs/)

---

## ✅ Deployment Checklist

- [ ] VPS prepared with Docker installed
- [ ] SSH key generated and added
- [ ] GitHub secrets configured
- [ ] `.env` file created on VPS
- [ ] `environment.prod.ts` updated with VPS IP
- [ ] Firewall configured
- [ ] Initial deployment successful
- [ ] All services running (docker-compose ps)
- [ ] Frontend accessible at port 4000
- [ ] Backend API accessible at port 9090
- [ ] Database connected
- [ ] SSL configured (production)
- [ ] Monitoring set up

---

**Version**: 1.0.0
**Last Updated**: November 2025
