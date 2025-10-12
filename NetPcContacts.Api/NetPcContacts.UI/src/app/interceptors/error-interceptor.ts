import {HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest} from '@angular/common/http';
import {catchError, Observable, switchMap, throwError} from 'rxjs';
import {inject} from '@angular/core';
import {AuthService} from '../services/auth-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  return next(req)
    .pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          // Don't try to refresh on login/register/refresh endpoints
          if (!req.url.includes('/login') && !req.url.includes('/register') && !req.url.includes('/refresh')) {
            return handle401Error(req, next, authService);
          }
          else {
            return throwError(() => error);
          }
        }
        else {
          return throwError(() => error);
        }
      })
    )
};

const addToken = (req: HttpRequest<unknown>) => {
  const accessToken = localStorage.getItem('accessToken');
  if (accessToken) {
    return req.clone({
      setHeaders: {
        Authorization: `Bearer ${accessToken}`
      }
    })
  }
  return req;
}

const handle401Error = (req: HttpRequest<unknown>, next: HttpHandlerFn, authService: AuthService): Observable<any> => {
  return authService.refreshToken()
    .pipe(
      switchMap(() => {
        return next(addToken(req))
      }),
      catchError(error => {
        console.error("Failed to refresh token - logging out user" , error);
        authService.logout();

        // Create a more user-friendly error
        const customError = new Error('Sesja wygasła. Zaloguj się ponownie.');
        return throwError(() => customError);
      })
    )
}


