export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user?: UserInfo;
}

export interface GoogleAuthRequest {
  idToken: string;
  accessToken?: string;
}

export interface GoogleAuthResponse {
  token: string;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  email: string;
  name: string;
  picture?: string;
  provider?: 'google' | 'local';
} 