# ASP.NET Core Web API - Google OAuth Backend

This is a complete ASP.NET Core Web API 9 implementation for Google OAuth authentication with JWT tokens and user management.

## Features

- ✅ Google OAuth 2.0 Authentication
- ✅ JWT Token Generation and Validation
- ✅ User Management with Entity Framework
- ✅ Session-based OAuth State Management
- ✅ CORS Configuration for Angular Frontend
- ✅ Swagger API Documentation
- ✅ Secure Password Hashing (placeholder)
- ✅ User Profile Management

## Prerequisites

1. **Google Cloud Console Setup** (Already completed)
   - OAuth 2.0 Client ID and Secret
   - Authorized redirect URIs configured

2. **Development Environment**
   - .NET 9.0 SDK
   - SQL Server (LocalDB, SQL Server Express, or Azure SQL)
   - Visual Studio 2022 or VS Code

## Setup Instructions

### 1. Configuration

Update `appsettings.json` with your actual values:

```json
{
  "Google": {
    "ClientId": "your-actual-google-client-id",
    "ClientSecret": "your-actual-google-client-secret",
    "RedirectUri": "http://localhost:7002/api/UserLogin/auth/google/callback"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-jwt-key-here-make-it-long-and-secure-at-least-32-characters",
    "Issuer": "your-api-issuer",
    "Audience": "your-api-audience",
    "ExpirationMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourApiDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 2. Database Setup

Create the database and apply migrations:

```bash
# Add initial migration
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update
```

### 3. Build and Run

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

The API will be available at: `https://localhost:7002`

## API Endpoints

### Authentication Endpoints

#### 1. Local Login
```
POST /api/UserLogin/login
Content-Type: application/json

{
  "username": "user@example.com",
  "password": "password123"
}
```

#### 2. Initiate Google OAuth
```
GET /api/UserLogin/auth/google
```
Redirects user to Google OAuth consent screen.

#### 3. Google OAuth Callback
```
POST /api/UserLogin/google-callback
Content-Type: application/json

{
  "code": "authorization_code_from_google",
  "state": "state_parameter_for_csrf_protection"
}
```

#### 4. Direct Google Login (Alternative)
```
POST /api/UserLogin/google-login
Content-Type: application/json

{
  "idToken": "google_id_token_from_frontend"
}
```

### Protected Endpoints

#### 5. Get User Profile
```
GET /api/UserLogin/profile
Authorization: Bearer your-jwt-token
```

## Response Format

All authentication endpoints return the same format:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "1",
    "email": "user@example.com",
    "name": "John Doe",
    "picture": "https://lh3.googleusercontent.com/...",
    "provider": "google"
  }
}
```

## Security Features

1. **CSRF Protection**: State parameter validation for OAuth flow
2. **JWT Security**: Signed tokens with expiration
3. **CORS Configuration**: Restricted to Angular frontend
4. **Session Management**: Secure OAuth state storage
5. **Input Validation**: Request validation and sanitization

## Frontend Integration

### Angular Service Update

Update your Angular auth service URL to match the backend:

```typescript
private apiUrl = 'http://localhost:7002/api/UserLogin';
```

### CORS Configuration

The backend is configured to allow requests from:
- `http://localhost:4200` (Angular development server)

For production, update the CORS policy in `Program.cs`.

## Database Schema

The `User` table includes:

- `Id` (Primary Key)
- `Email` (Unique, Required)
- `Name` (Required)
- `Picture` (Optional - Google profile picture)
- `Provider` (Required - "google" or "local")
- `PasswordHash` (Optional - for local users)
- `CreatedAt` (Timestamp)
- `LastLoginAt` (Timestamp)

## Environment Variables (Production)

For production deployment, use environment variables:

```bash
# Google OAuth
GOOGLE__CLIENTID=your-client-id
GOOGLE__CLIENTSECRET=your-client-secret
GOOGLE__REDIRECTURI=https://yourdomain.com/api/UserLogin/auth/google/callback

# JWT
JWT__SECRETKEY=your-super-secret-key
JWT__ISSUER=your-api-issuer
JWT__AUDIENCE=your-api-audience
JWT__EXPIRATIONMINUTES=60

# Database
CONNECTIONSTRINGS__DEFAULTCONNECTION=your-production-connection-string
```

## Testing

1. **Start the backend**: `dotnet run`
2. **Start your Angular frontend**
3. **Test Google OAuth flow**:
   - Click "Continue with Google"
   - Complete Google authentication
   - Verify user is logged in
4. **Test JWT token**: Use the token in the Authorization header for protected endpoints

## Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure Angular app URL is in CORS policy
2. **Google OAuth Errors**: Verify redirect URI matches exactly
3. **Database Connection**: Check connection string and SQL Server
4. **JWT Token Issues**: Verify secret key length and configuration

### Logs

Check application logs for detailed error information:
- Google OAuth flow errors
- Database connection issues
- JWT token validation problems

## Next Steps

1. **Implement proper password hashing** (BCrypt recommended)
2. **Add refresh token functionality**
3. **Implement user registration endpoint**
4. **Add email verification**
5. **Implement password reset functionality**
6. **Add role-based authorization**
7. **Implement rate limiting**
8. **Add comprehensive logging and monitoring**

## Security Recommendations

1. **Use HTTPS in production**
2. **Implement proper password hashing**
3. **Add rate limiting for authentication endpoints**
4. **Use secure session configuration**
5. **Implement proper error handling**
6. **Add request validation**
7. **Use environment variables for secrets**
8. **Regular security updates** 