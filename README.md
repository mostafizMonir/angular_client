# Authentication Frontend

This is an Angular application that provides a login interface for the MilanAuth API.

## Features

- Beautiful and modern login form
- Username and password authentication
- Automatic token storage in localStorage
- Token display in the UI
- Automatic authorization header injection for API calls
- Responsive design
- Error handling and success messages

## Prerequisites

- Node.js (version 18 or higher)
- Angular CLI
- Your MilanAuth backend running on `https://localhost:7001`

## Installation

1. Navigate to the project directory:
   ```bash
   cd auth-frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

## Configuration

Before running the application, make sure to:

1. Update the API URL in `src/app/auth.ts` if your backend is running on a different port:
   ```typescript
   private apiUrl = 'https://localhost:7001/api/UserLogin'; // Adjust as needed
   ```

2. Ensure your MilanAuth backend is running and accessible

## Running the Application

1. Start the development server:
   ```bash
   ng serve
   ```

2. Open your browser and navigate to `http://localhost:4200`

## Usage

1. **Login**: Enter your username and password in the form fields
2. **Submit**: Click the "Login" button to authenticate
3. **Token Storage**: Upon successful login, the access token will be automatically stored and displayed
4. **API Calls**: The token will be automatically included in the Authorization header for subsequent API calls
5. **Logout**: Click the "Logout" button to clear the stored token

## API Integration

The application is configured to work with your `UserLoginController`:

- **Endpoint**: `POST /api/UserLogin/login`
- **Request Body**: 
  ```json
  {
    "username": "your_username",
    "password": "your_password"
  }
  ```
- **Response**: 
  ```json
  {
    "token": "your_access_token"
  }
  ```

## Features for Further API Calls

The `AuthService` provides methods for making authenticated API calls:

```typescript
// Get the current token
const token = this.authService.getToken();

// Check if user is logged in
const isLoggedIn = this.authService.isLoggedIn();

// Get authorization headers for API calls
const headers = this.authService.getAuthHeaders();

// Logout
this.authService.logout();
```

## HTTP Interceptor

The application includes an HTTP interceptor that automatically adds the Authorization header to all HTTP requests when a token is available:

```typescript
Authorization: Bearer your_access_token
```

## Project Structure

```
src/
├── app/
│   ├── login/                 # Login component
│   │   ├── login.ts          # Component logic
│   │   ├── login.html        # Template
│   │   └── login.css         # Styles
│   ├── models/
│   │   └── auth.models.ts    # TypeScript interfaces
│   ├── interceptors/
│   │   └── auth.interceptor.ts # HTTP interceptor
│   ├── auth.ts               # Authentication service
│   ├── app.ts                # Main app component
│   ├── app.config.ts         # App configuration
│   └── app.routes.ts         # Routing configuration
```

## Troubleshooting

1. **CORS Issues**: Make sure your backend allows requests from `http://localhost:4200`
2. **API Connection**: Verify your backend is running on the correct port
3. **Token Issues**: Check the browser's developer tools to see if the token is being stored correctly

## Development

To build the application for production:

```bash
ng build
```

The build artifacts will be stored in the `dist/` directory.
