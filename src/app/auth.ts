import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, GoogleAuthRequest, GoogleAuthResponse, UserInfo } from './models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:7002/api/UserLogin'; // Adjust port as needed
  private tokenKey = 'access_token';
  private userKey = 'user_info';

  constructor(private http: HttpClient) {}

  login(credentials: LoginRequest): Observable<LoginResponse> {
    console.log('Making login request to:', `${this.apiUrl}/login`);
    console.log('Request payload:', credentials);
    
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          console.log('Login response received:', response);
          this.storeToken(response.token);
          if (response.user) {
            this.storeUser(response.user);
          }
        })
      );
  }

  googleLogin(googleAuth: GoogleAuthRequest): Observable<GoogleAuthResponse> {
    console.log('Making Google login request to:', `${this.apiUrl}/google-login`);
    console.log('Google auth payload:', googleAuth);
    
    return this.http.post<GoogleAuthResponse>(`${this.apiUrl}/google-login`, googleAuth)
      .pipe(
        tap(response => {
          console.log('Google login response received:', response);
          this.storeToken(response.token);
          this.storeUser(response.user);
        })
      );
  }

  handleGoogleCallback(): Observable<GoogleAuthResponse> {
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const state = urlParams.get('state');
    
    console.log('Handling Google callback with code:', code);
    
    return this.http.post<GoogleAuthResponse>(`${this.apiUrl}/google-callback`, { code, state })
      .pipe(
        tap(response => {
          console.log('Google callback response received:', response);
          this.storeToken(response.token);
          this.storeUser(response.user);
        })
      );
  }

  private storeToken(token: string): void {
    console.log('Storing token:', token ? 'Token received' : 'No token');
    localStorage.setItem(this.tokenKey, token);
  }

  private storeUser(user: UserInfo): void {
    console.log('Storing user info:', user);
    localStorage.setItem(this.userKey, JSON.stringify(user));
  }

  getToken(): string | null {
    const token = localStorage.getItem(this.tokenKey);
    console.log('Getting token from storage:', token ? 'Token exists' : 'No token');
    return token;
  }

  getUser(): UserInfo | null {
    const userStr = localStorage.getItem(this.userKey);
    if (userStr) {
      try {
        return JSON.parse(userStr);
      } catch (e) {
        console.error('Error parsing user info:', e);
        return null;
      }
    }
    return null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    console.log('Logging out - clearing token and user info');
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }

  // Method to get headers with token for API calls
  getAuthHeaders(): { [key: string]: string } {
    const token = this.getToken();
    return token ? { 'Authorization': `Bearer ${token}` } : {};
  }

  // Method to initiate Google OAuth flow
  initiateGoogleLogin(): void {
    const googleAuthUrl = `${this.apiUrl}/auth/google`;
    window.location.href = googleAuthUrl;
  }
}
