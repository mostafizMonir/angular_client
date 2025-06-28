# Configuration Setup Guide

## Environment Setup

This application requires several configuration values to be set up properly. Follow these steps to configure your development environment:

### 1. Google OAuth Configuration

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API
4. Go to "Credentials" and create an OAuth 2.0 Client ID
5. Set the authorized redirect URI to: `http://localhost:7002/api/UserLogin/google-callback`
6. Copy the Client ID and Client Secret

### 2. JWT Configuration

Generate a secure JWT secret key. You can use an online generator or run this command:
```bash
openssl rand -base64 32
```

### 3. Database Configuration

Set up your PostgreSQL database and note the connection details.

### 4. Local Development Setup

For local development, create a `appsettings.Development.json` file in the `backend-aspnet-google-oauth` directory with the following structure:

```json
{
  "Google": {
    "ClientId": "YOUR_ACTUAL_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_ACTUAL_GOOGLE_CLIENT_SECRET",
    "RedirectUri": "http://localhost:7002/api/UserLogin/google-callback"
  },
  "Jwt": {
    "SecretKey": "YOUR_ACTUAL_JWT_SECRET_KEY",
    "Issuer": "your-api-issuer",
    "Audience": "your-api-audience",
    "ExpirationMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GoogleLoginDb;Username=postgres;Password=YOUR_DB_PASSWORD"
  }
}
```

### 5. Production Setup

For production, use environment variables or secure configuration management systems like:
- Azure Key Vault
- AWS Secrets Manager
- Environment variables
- Docker secrets

### Security Notes

- Never commit real credentials to version control
- Use different credentials for development, staging, and production
- Rotate secrets regularly
- Use strong, randomly generated JWT keys
- Consider using managed identity services in production 