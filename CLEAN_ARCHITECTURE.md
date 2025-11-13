# Clean Architecture Implementation

## Overview

The backend has been refactored to follow **Clean Architecture** principles with comprehensive security, authentication, authorization, user management, and role-based permissions.

## Architecture Layers

```
Backend/
├── Domain/                 # Core business entities and enums
│   ├── Entities/
│   │   └── Identity/      # User, Role, Permission entities
│   └── Enums/             # System constants
├── Application/            # Business logic interfaces and DTOs
│   ├── DTOs/
│   │   ├── Auth/          # Authentication DTOs
│   │   ├── Users/         # User management DTOs
│   │   ├── Roles/         # Role management DTOs
│   │   └── Permissions/   # Permission DTOs
│   └── Interfaces/        # Service contracts
├── Infrastructure/         # External concerns implementation
│   ├── Data/              # Database context
│   ├── Services/          # Service implementations
│   ├── Configuration/     # Settings classes
│   └── Authorization/     # Custom authorization
└── API/                    # Presentation layer
    └── Controllers/       # REST API endpoints
```

## Security Features

### 1. JWT Authentication
- **Access Tokens**: Short-lived (60 minutes)
- **Refresh Tokens**: Long-lived (7 days)
- **Token Revocation**: Support for logout and token invalidation
- **IP Tracking**: Tracks token creation and revocation by IP

### 2. Role-Based Access Control (RBAC)
Five predefined system roles:
- **SuperAdmin**: Full system access
- **Admin**: Administrative access
- **Manager**: Limited administrative access
- **User**: Regular user access
- **Guest**: Read-only access

### 3. Permission-Based Authorization
Granular permissions for each module:
- **Users**: create, read, update, delete, manage
- **Roles**: create, read, update, delete, manage
- **Messages**: create, read, update, delete, manage
- **Calendar**: create, read, update, delete, manage
- **Contacts**: create, read, update, delete, manage
- **Conference**: create, read, update, delete, manage, host, record

### 4. User Management
- User registration with validation
- Email/password authentication
- Password change and reset
- User activation/deactivation
- Role assignment
- Permission override

## Database Schema

### Identity Tables
- **Users**: ApplicationUser with custom properties
- **Roles**: Extended IdentityRole with description
- **UserRoles**: Many-to-many with tracking
- **Permissions**: Granular permission definitions
- **RolePermissions**: Role-permission mapping
- **UserPermissions**: User-specific permission overrides
- **RefreshTokens**: Refresh token storage with expiration

## API Endpoints

### Authentication (`/api/auth`)
```
POST   /api/auth/register         - Register new user
POST   /api/auth/login            - Login with email/password
POST   /api/auth/refresh-token    - Refresh access token
POST   /api/auth/logout           - Logout and revoke tokens
POST   /api/auth/change-password  - Change password
POST   /api/auth/forgot-password  - Request password reset
```

### Users (`/api/users`)
```
GET    /api/users                 - Get all users (paginated)
GET    /api/users/{id}            - Get user by ID
PUT    /api/users/{id}            - Update user
DELETE /api/users/{id}            - Delete user
POST   /api/users/{id}/activate   - Activate user
POST   /api/users/{id}/deactivate - Deactivate user
POST   /api/users/{id}/roles/{role} - Assign role
DELETE /api/users/{id}/roles/{role} - Remove role
GET    /api/users/{id}/roles      - Get user roles
GET    /api/users/{id}/permissions - Get user permissions
```

### Roles (`/api/roles`)
```
GET    /api/roles                 - Get all roles
GET    /api/roles/{id}            - Get role by ID
POST   /api/roles                 - Create role
PUT    /api/roles/{id}            - Update role
DELETE /api/roles/{id}            - Delete role
POST   /api/roles/{id}/permissions/{permId} - Assign permission
DELETE /api/roles/{id}/permissions/{permId} - Remove permission
GET    /api/roles/{id}/permissions - Get role permissions
```

### Permissions (`/api/permissions`)
```
GET    /api/permissions           - Get all permissions
GET    /api/permissions/{id}      - Get permission by ID
GET    /api/permissions/module/{module} - Get by module
POST   /api/permissions/seed      - Seed system permissions
```

## Configuration

### appsettings.json
```json
{
  "JwtSettings": {
    "SecretKey": "Your-Secret-Key-Min-32-Chars",
    "Issuer": "RaselOutlookManagement",
    "Audience": "RaselOutlookUsers",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

## Usage

### 1. Initialize Database
On first run, the system automatically:
- Applies migrations
- Seeds permissions
- Creates default roles
- Creates SuperAdmin user
  - Email: `admin@rasel.com`
  - Password: `Admin@123456`

### 2. Register New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "Password@123",
  "confirmPassword": "Password@123"
}
```

### 3. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@rasel.com",
  "password": "Admin@123456"
}
```

Response:
```json
{
  "userId": "...",
  "email": "admin@rasel.com",
  "fullName": "System Administrator",
  "accessToken": "eyJhbGc...",
  "refreshToken": "dGVz...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "roles": ["SuperAdmin"],
  "permissions": ["users.create", "users.read", ...]
}
```

### 4. Use Protected Endpoints
```http
GET /api/users
Authorization: Bearer eyJhbGc...
```

### 5. Custom Permission Check
In controllers, use the `[Permission]` attribute:
```csharp
[Permission(SystemPermission.UsersCreate)]
public async Task<IActionResult> CreateUser(...)
{
    // Only users with users.create permission can access
}
```

## Authorization Flow

1. User logs in → receives JWT access token
2. Access token contains:
   - User ID
   - Email
   - Name
   - Roles (as claims)
   - Permissions (as claims)
3. Each request includes token in Authorization header
4. `PermissionAuthorizationHandler` validates permission claims
5. Access granted if user has required permission

## Security Best Practices

✅ **Passwords**: Hashed with Identity's PBKDF2
✅ **Tokens**: Signed with HMAC-SHA256
✅ **HTTPS**: Required in production
✅ **CORS**: Configured for Angular frontend
✅ **Lockout**: 5 failed attempts = 15 min lockout
✅ **Password Policy**: Min 6 chars, uppercase, lowercase, digit, special char
✅ **Token Expiration**: Short-lived access tokens
✅ **Refresh Tokens**: Stored securely, revocable
✅ **IP Tracking**: Token creation and revocation logged

## Migration from Old Structure

The old structure had:
- Simple Identity with basic roles
- No permission system
- No JWT authentication
- No refresh tokens

The new structure provides:
- Full Clean Architecture separation
- Granular permission system
- JWT with refresh tokens
- IP tracking and audit trail
- Comprehensive user management
- Role and permission management APIs

## Next Steps

1. **Frontend Integration**: Update Angular app to use new auth endpoints
2. **Email Service**: Implement email for password reset
3. **Audit Logging**: Add comprehensive audit trail
4. **Rate Limiting**: Add API rate limiting
5. **2FA**: Implement two-factor authentication
6. **OAuth**: Add Google/Microsoft OAuth providers
