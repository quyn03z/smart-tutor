export interface ApiResult<T> {
  succeeded: boolean;
  result: T;
  errors: string[];
  message?: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  role: string;
  permissions?: string[];
}

export interface UserSession {
  token: string;
  refreshToken: string;
  role: string;
  email?: string;
}
