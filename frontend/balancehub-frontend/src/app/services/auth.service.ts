import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
  role: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: {
    id: string;
    email: string;
    role: string;
    firstName?: string;
    lastName?: string;
  };
}

export interface User {
  id: string;
  email: string;
  role: string;
  firstName?: string;
  lastName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'current_user';
  private readonly REMEMBER_ME_KEY = 'remember_me';

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.initializeAuthState();
  }

  private initializeAuthState(): void {
    const token = this.getToken();
    const user = this.getCurrentUser();

    if (token && user) {
      this.currentUserSubject.next(user);
      this.isAuthenticatedSubject.next(true);
    }
  }

  login(email: string, password: string, role: string): Observable<LoginResponse> {
    const loginRequest: LoginRequest = { email, password, role };

    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, loginRequest)
      .pipe(
        tap(response => {
          if (response.success && response.token && response.user) {
            this.setSession(response.token, response.user);
          }
        }),
        catchError(this.handleError.bind(this))
      );
  }

  logout(): void {
    this.clearSession();
    this.router.navigate(['/login']);
  }

  private setSession(token: string, user: User): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));

    // Set remember me preference if provided
    const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';

    this.currentUserSubject.next(user);
    this.isAuthenticatedSubject.next(true);
  }

  private clearSession(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.REMEMBER_ME_KEY);

    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem(this.USER_KEY);
    return userStr ? JSON.parse(userStr) : null;
  }

  isLoggedIn(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user ? user.role === role : false;
  }

  setRememberMe(remember: boolean): void {
    localStorage.setItem(this.REMEMBER_ME_KEY, remember.toString());
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unexpected error occurred';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      switch (error.status) {
        case 400:
          errorMessage = error.error?.message || 'Invalid login credentials';
          break;
        case 401:
          errorMessage = 'Invalid email or password';
          break;
        case 403:
          errorMessage = 'Access denied';
          break;
        case 404:
          errorMessage = 'Authentication service not found';
          break;
        case 500:
          errorMessage = 'Internal server error';
          break;
        default:
          errorMessage = error.error?.message || 'Login failed';
      }
    }

    return throwError(() => ({
      success: false,
      message: errorMessage
    }));
  }

  // Token refresh logic (for future implementation)
  refreshToken(): Observable<LoginResponse> {
    const currentUser = this.getCurrentUser();
    if (!currentUser) {
      return throwError(() => ({ success: false, message: 'No user logged in' }));
    }

    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/refresh`, {})
      .pipe(
        tap(response => {
          if (response.success && response.token) {
            this.setSession(response.token, currentUser);
          }
        }),
        catchError(this.handleError.bind(this))
      );
  }
}
