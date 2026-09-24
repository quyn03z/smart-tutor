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

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface RegisterResponse {
  id: number;
}

export interface ForgotPasswordResponse {
  resetToken: string;
  expiredAt: string;
}

export interface ResetPasswordRequest {
  email: string;
  resetToken: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface UserSession {
  token: string;
  refreshToken: string;
  role: string;
  email?: string;
}

export interface CurrentUserResponse {
  fullName: string;
  bankCode: string;
  bankAccountNumber: string;
}
