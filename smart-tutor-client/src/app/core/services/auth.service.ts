import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResult, CurrentUserResponse, ForgotPasswordResponse, LoginRequest, LoginResponse, RegisterRequest, RegisterResponse, ResetPasswordRequest, UserSession } from '../models/auth.models';

const TOKEN_KEY = 'st_access_token';
const REFRESH_TOKEN_KEY = 'st_refresh_token';
const USER_KEY = 'st_user_session';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<UserSession | null>(this.getStoredUser());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(credentials: LoginRequest, rememberMe: boolean = true): Observable<ApiResult<LoginResponse>> {
    return this.http.post<ApiResult<LoginResponse>>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        if (response?.succeeded && response.result) {
          const userSession: UserSession = {
            token: response.result.token,
            refreshToken: response.result.refreshToken,
            role: response.result.role,
            email: credentials.email
          };

          this.saveSession(userSession, rememberMe);
          this.currentUserSubject.next(userSession);
        }
      })
    );
  }

  register(data: RegisterRequest): Observable<ApiResult<RegisterResponse>> {
    return this.http.post<ApiResult<RegisterResponse>>(`${this.apiUrl}/register`, data);
  }

  forgotPassword(email: string): Observable<ApiResult<ForgotPasswordResponse>> {
    return this.http.post<ApiResult<ForgotPasswordResponse>>(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(data: ResetPasswordRequest): Observable<ApiResult<string>> {
    return this.http.post<ApiResult<string>>(`${this.apiUrl}/reset-password`, data);
  }


  logout(): void {
    // Optionally notify server
    this.http.post(`${this.apiUrl}/logout`, {}).subscribe({
      error: () => {} // Ignore errors on logout
    });

    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(REFRESH_TOKEN_KEY);
    sessionStorage.removeItem(USER_KEY);

    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY) || sessionStorage.getItem(TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY) || sessionStorage.getItem(REFRESH_TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getRole(): string | null {
    const user = this.currentUserSubject.value;
    return user?.role || null;
  }

  private saveSession(session: UserSession, rememberMe: boolean): void {
    const storage = rememberMe ? localStorage : sessionStorage;
    
    // Clear other storage to avoid conflicts
    const otherStorage = rememberMe ? sessionStorage : localStorage;
    otherStorage.removeItem(TOKEN_KEY);
    otherStorage.removeItem(REFRESH_TOKEN_KEY);
    otherStorage.removeItem(USER_KEY);

    storage.setItem(TOKEN_KEY, session.token);
    storage.setItem(REFRESH_TOKEN_KEY, session.refreshToken);
    storage.setItem(USER_KEY, JSON.stringify(session));
  }

  private getStoredUser(): UserSession | null {
    const rawUser = localStorage.getItem(USER_KEY) || sessionStorage.getItem(USER_KEY);
    if (!rawUser) return null;
    try {
      return JSON.parse(rawUser) as UserSession;
    } catch {
      return null;
    }
  }

getCurrentUser(): Observable<ApiResult<CurrentUserResponse>> {
  const token = this.getToken();
  const headers = new HttpHeaders({
    'Authorization': `Bearer ${token}`
  });
  return this.http.get<ApiResult<CurrentUserResponse>>(`${this.apiUrl}/current-user`, { headers });
}



}
