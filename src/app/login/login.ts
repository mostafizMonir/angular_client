import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../auth';
import { LoginRequest, UserInfo } from '../models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent implements OnInit {
  credentials: LoginRequest = {
    username: '',
    password: ''
  };
  
  successMessageShow = false;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  storedToken = '';
  currentUser: UserInfo | null = null;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.loadStoredToken();
    this.checkGoogleCallback();
  }

  onLogin(): void {
    console.log('Login attempt with:', this.credentials);
    this.isLoading = true;
    this.successMessageShow = false;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        console.log('Login successful:', response);
        this.successMessage = 'Login successful! Token stored.';
        this.successMessageShow = true;        
        this.loadStoredToken();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Login failed:', error);
        this.errorMessage = error.error || 'Login failed. Please try again.';
        this.isLoading = false;
      }
    });
  }

  onGoogleLogin(): void {
    console.log('Initiating Google login...');
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    // Use the auth service method to initiate Google OAuth
    this.authService.initiateGoogleLogin();
  }

  private checkGoogleCallback(): void {
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    const state = urlParams.get('state');
    
    if (code && state) {
      console.log('Google callback detected, processing...');
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';
      
      this.authService.handleGoogleCallback().subscribe({
        next: (response) => {
          console.log('Google login successful:', response);
          this.successMessage = 'Google login successful! Welcome, ' + response.user.name;
          this.successMessageShow = true;
          this.loadStoredToken();
          this.isLoading = false;
          
          // Clean up URL parameters
          window.history.replaceState({}, document.title, window.location.pathname);
        },
        error: (error) => {
          console.error('Google login failed:', error);
          this.errorMessage = error.error || 'Google login failed. Please try again.';
          this.isLoading = false;
          
          // Clean up URL parameters
          window.history.replaceState({}, document.title, window.location.pathname);
        }
      });
    }
  }

  private loadStoredToken(): void {
    this.storedToken = this.authService.getToken() || '';
    this.currentUser = this.authService.getUser();
    console.log('Loaded stored token:', this.storedToken ? 'Token exists' : 'No token');
    console.log('Current user:', this.currentUser);
  }

  logout(): void {
    this.authService.logout();
    this.storedToken = '';
    this.currentUser = null;
    this.successMessage = 'Logged out successfully.';
    this.successMessageShow = true;
  }
}
