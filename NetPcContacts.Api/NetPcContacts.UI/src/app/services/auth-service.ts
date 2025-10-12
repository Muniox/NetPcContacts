import {computed, effect, inject, Injectable, signal, untracked} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable, tap} from 'rxjs';

import {environment} from '../../environments/environment';
import {LoginRequest, LoginResponse, RegisterRequest} from '../models';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  // Signals for state management
  private readonly accessTokenSignal = signal<string | null>(this.getAccessTokenFromStorage());
  private readonly refreshTokenSignal = signal<string | null>(this.getRefreshTokenFromCookie());

  // Public computed signals
  readonly isLoggedIn = computed(() => !!this.accessTokenSignal());
  readonly accessToken = computed(() => this.accessTokenSignal());

  constructor() {
    // Sync accessToken changes to localStorage
    effect(() => {
      const token = this.accessTokenSignal();
      untracked(() => {
        if (token) {
          localStorage.setItem('accessToken', token);
        } else {
          localStorage.removeItem('accessToken');
        }
      });
    });

    // Sync refreshToken changes to cookie (7 days expiration)
    effect(() => {
      const token = this.refreshTokenSignal();
      untracked(() => {
        if (token) {
          this.setCookie('refreshToken', token, 7);
        } else {
          this.deleteCookie('refreshToken');
        }
      });
    });
  }

  register(credentials: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/api/identity/register`, credentials);
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/api/identity/login`, credentials)
      .pipe(
        tap(response => {
          this.accessTokenSignal.set(response.accessToken);
          this.refreshTokenSignal.set(response.refreshToken);
        })
      );
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.refreshTokenSignal();

    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/api/identity/refresh`, {refreshToken})
      .pipe(
        tap(response => {
          this.accessTokenSignal.set(response.accessToken);
          this.refreshTokenSignal.set(response.refreshToken);
        })
      );
  }

  logout(): void {
    this.accessTokenSignal.set(null);
    this.refreshTokenSignal.set(null);
  }

  private getAccessTokenFromStorage(): string | null {
    return localStorage.getItem('accessToken');
  }

  private getRefreshTokenFromCookie(): string | null {
    const cookieString = document.cookie;
    const cookieArray = cookieString.split('; ');

    for (const cookie of cookieArray) {
      const [name, value] = cookie.trim().split('=');

      if (name === 'refreshToken') {
        return value;
      }
    }

    return null;
  }

  private setCookie(name: string, value: string, days: number): void {
    const date = new Date();
    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
    const expires = `expires=${date.toUTCString()}`;
    const secure = window.location.protocol === 'https:' ? 'Secure;' : '';
    document.cookie = `${name}=${value};${expires};path=/;${secure}SameSite=Strict`;
  }

  private deleteCookie(name: string): void {
    const secure = window.location.protocol === 'https:' ? 'Secure;' : '';
    document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;${secure}SameSite=Strict`;
  }
}
