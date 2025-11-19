# Build Instructions - Rasel Inbox Management System

This document provides comprehensive instructions for building the Rasel Inbox Management System, which consists of a .NET 8.0 Backend and an Angular Frontend.

## Prerequisites

### Required Software

1. **Backend (.NET)**
   - .NET 8.0 SDK or later
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0

2. **Frontend (Angular)**
   - Node.js 18.x or later (20.x recommended)
   - npm 9.x or later (comes with Node.js)
   - Download from: https://nodejs.org/

3. **Database**
   - SQL Server 2019 or later
   - SQL Server Express (free) is sufficient
   - Download from: https://www.microsoft.com/sql-server/sql-server-downloads

4. **Optional Tools**
   - Visual Studio 2022 (for IDE support)
   - Visual Studio Code (lightweight alternative)
   - SQL Server Management Studio (SSMS)

## Quick Start

### Option 1: Build All Projects (Automated)

**Linux/macOS:**
```bash
./build-all.sh
```

**Windows:**
```cmd
build-all.bat
```

### Option 2: Build Using Visual Studio

1. Open `RaselInbox.sln` in Visual Studio 2022
2. Right-click solution in Solution Explorer
3. Select "Restore NuGet Packages"
4. Build → Build Solution (Ctrl+Shift+B)

### Option 3: Build Using .NET CLI

**Build Backend:**
```bash
cd Backend
dotnet restore
dotnet build --configuration Release
```

**Build Frontend:**
```bash
cd Frontend
npm install
npm run build
```

## Detailed Build Instructions

### Backend Build

#### 1. Restore Dependencies
```bash
cd Backend
dotnet restore OutlookInboxManagement.csproj
```

#### 2. Build Project
```bash
# Debug build
dotnet build OutlookInboxManagement.csproj --configuration Debug

# Release build (optimized)
dotnet build OutlookInboxManagement.csproj --configuration Release
```

#### 3. Run Backend (Development)
```bash
dotnet run --project OutlookInboxManagement.csproj
```

The backend will start on:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5001

#### 4. Verify Build
```bash
# Check for build errors
dotnet build --no-restore

# Expected output: "Build succeeded. 0 Warning(s). 0 Error(s)."
```

### Frontend Build

#### 1. Install Dependencies
```bash
cd Frontend
npm install
```

#### 2. Build Project
```bash
# Development build
npm run build

# Production build (optimized)
npm run build -- --configuration production
```

#### 3. Run Frontend (Development Server)
```bash
npm start
# or
ng serve
```

The frontend will start on: http://localhost:4200

#### 4. Verify Build
Check the `dist/outlook-inbox-frontend/` directory for compiled files.

## Configuration

### Backend Configuration

**Database Connection:**
Edit `Backend/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OutlookInboxManagement;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**JWT Settings:**
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration2024MinimumLength32Characters!",
    "Issuer": "RaselOutlookManagement",
    "Audience": "RaselOutlookUsers",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Frontend Configuration

**API Endpoint:**
Edit `Frontend/src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5001/api'
};
```

## Database Setup

### 1. Create Database
```sql
CREATE DATABASE OutlookInboxManagement;
```

### 2. Run Migrations
```bash
cd Backend
dotnet ef database update
```

### 3. Verify Database
```sql
USE OutlookInboxManagement;
SELECT * FROM INFORMATION_SCHEMA.TABLES;
```

## Build Verification

### Check for Zero Errors

**Backend:**
```bash
cd Backend
dotnet build --no-restore | grep -E "Error|Warning|succeeded"
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**Frontend:**
```bash
cd Frontend
npm run build 2>&1 | grep -E "error|Error|warning|Warning|✔|successfully"
```

Expected output:
```
✔ Browser application bundle generation complete.
```

## Troubleshooting

### Common Issues

#### Backend Issues

**Issue: "The type or namespace name could not be found"**
- Solution: Run `dotnet restore` to restore NuGet packages

**Issue: "Unable to connect to database"**
- Solution: Verify SQL Server is running and connection string is correct
- Check: `services.msc` → SQL Server (MSSQLSERVER) is running

**Issue: "Build failed with namespace conflicts"**
- Solution: This has been resolved in the latest commits
- The following files are excluded from build:
  - `NewProgram.cs`
  - `API/**/*.cs` (incomplete architecture)
  - `Services/Admin/DashboardService.cs`
  - `Services/Admin/ReportService.cs`

#### Frontend Issues

**Issue: "npm ERR! code ENOENT"**
- Solution: Run `npm install` first to install dependencies

**Issue: "Module not found" errors**
- Solution: Delete `node_modules` and `package-lock.json`, then run `npm install`

**Issue: "ng: command not found"**
- Solution: Install Angular CLI globally: `npm install -g @angular/cli`

### Clean Build

**Backend:**
```bash
cd Backend
dotnet clean
dotnet restore
dotnet build
```

**Frontend:**
```bash
cd Frontend
rm -rf node_modules dist
npm install
npm run build
```

## Solution Structure

```
RaselInbox.sln                  # Visual Studio solution file
├── Backend/
│   └── OutlookInboxManagement.csproj  # Backend .NET project
└── Frontend/                   # Angular project (not in .sln)
    ├── package.json
    └── angular.json
```

## Build Configuration

### Excluded Files (Backend)

The following files are excluded from compilation to prevent build conflicts:

```xml
<ItemGroup>
  <Compile Remove="NewProgram.cs" />
  <Compile Remove="API\**\*.cs" />
  <Compile Remove="Services\Admin\DashboardService.cs" />
  <Compile Remove="Services\Admin\ReportService.cs" />
</ItemGroup>
```

These files represent an incomplete architecture migration and are preserved for future reference but do not participate in the build.

## Production Build

### Backend (Release Configuration)
```bash
cd Backend
dotnet publish -c Release -o ./publish
```

Output: `Backend/publish/` directory

### Frontend (Production Configuration)
```bash
cd Frontend
npm run build -- --configuration production
```

Output: `Frontend/dist/outlook-inbox-frontend/browser/` directory

## Docker Build (Alternative)

### Build Backend Image
```bash
docker build -t rasel-inbox-backend -f Backend/Dockerfile .
```

### Build Frontend Image
```bash
docker build -t rasel-inbox-frontend -f Frontend/Dockerfile .
```

### Build All Services
```bash
docker-compose build
```

## Continuous Integration

The project includes GitHub Actions workflows for automated building:
- `.github/workflows/deploy.yml`

Builds are triggered on:
- Push to main/master branches
- Pull requests
- Manual workflow dispatch

## Support

For build issues:
1. Check this document for troubleshooting steps
2. Verify all prerequisites are installed
3. Run clean build commands
4. Check GitHub Issues for known problems

## Version Information

- .NET SDK: 8.0
- Angular: 19.x
- Node.js: 18.x+ (20.x recommended)
- TypeScript: 5.x
- SQL Server: 2019+

---

**Last Updated:** 2025-01-19

Build with 0 errors! 🎯
