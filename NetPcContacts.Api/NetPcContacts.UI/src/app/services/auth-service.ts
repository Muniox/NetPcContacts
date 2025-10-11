import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, Observable} from 'rxjs';

import {environment} from '../../environments/environment';
import {LoginRequest, LoginResponse} from '../models';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  http = inject(HttpClient);
  apiUrl = environment.apiUrl;

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/identity/login`, credentials)
      .pipe(map(response => {
        localStorage.setItem('accessToken', response.accessToken);
        document.cookie = `refreshToken=${response.refreshToken}; path=/identity/refresh;`;
        return response;
      }))
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshTokenFromCookie();

    return this.http.post<LoginResponse>(`${this.apiUrl}/identity/refresh`, {refreshToken})
      .pipe(map(response => {
        localStorage.setItem('accessToken', response.accessToken);
        document.cookie = `refreshToken=${response.refreshToken}; path=/identity/refresh;`;
        return response;
      }))
  }

  private getRefreshTokenFromCookie(): string | null {
    const cookieString = document.cookie
    const cookieArray = cookieString.split('; ');

    for (const cookie of cookieArray) {
      const [name, value] = cookie.trim().split('=');

      if (name === 'refreshToken') {
        return value;
      }
    }

    return null;
  }

  logout(): void {
    localStorage.removeItem('accessToken');
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('accessToken');
  }
}
