# Backend Google OAuth Setup Guide

## Prerequisites
1. Google Cloud Console account
2. Backend server (Node.js/Express, .NET, Java, etc.)

## Google Cloud Console Setup

### 1. Create a Google Cloud Project
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable the Google+ API

### 2. Configure OAuth 2.0 Credentials
1. Go to "APIs & Services" > "Credentials"
2. Click "Create Credentials" > "OAuth 2.0 Client IDs"
3. Choose "Web application"
4. Add authorized redirect URIs:
   - `http://localhost:7002/api/UserLogin/auth/google/callback` (for development)
   - `https://yourdomain.com/api/UserLogin/auth/google/callback` (for production)
5. Note down your Client ID and Client Secret

## Backend Implementation

### Required Endpoints

Your backend should implement these endpoints:

#### 1. Initiate Google OAuth
```
GET /api/UserLogin/auth/google
```
- Redirects user to Google OAuth consent screen
- Should include state parameter for security

#### 2. Google OAuth Callback
```
POST /api/UserLogin/google-callback
``` 
- Receives authorization code from Google
- Exchanges code for access token
- Gets user info from Google
- Creates/updates user in your database
- Returns JWT token and user info

#### 3. Direct Google Login (Alternative)
```
POST /api/UserLogin/google-login
```
- Receives Google ID token from frontend
- Verifies token with Google
- Creates/updates user in your database
- Returns JWT token and user info

### Example Response Format

```json
{
  "token": "your-jwt-token-here",
  "user": {
    "id": "user-id",
    "email": "user@example.com",
    "name": "User Name",
    "picture": "https://lh3.googleusercontent.com/...",
    "provider": "google"
  }
}
```

## Security Considerations

1. **State Parameter**: Always use state parameter to prevent CSRF attacks
2. **Token Verification**: Verify Google ID tokens on the server side
3. **HTTPS**: Use HTTPS in production
4. **CORS**: Configure CORS properly for your frontend domain
5. **Environment Variables**: Store Google credentials in environment variables

## Testing

1. Start your backend server
2. Start your Angular frontend
3. Click "Continue with Google" button
4. Complete Google OAuth flow
5. Verify user is logged in and token is stored

## Troubleshooting

- Check browser console for errors
- Verify redirect URIs match exactly
- Ensure Google+ API is enabled
- Check backend logs for authentication errors
- Verify CORS configuration 