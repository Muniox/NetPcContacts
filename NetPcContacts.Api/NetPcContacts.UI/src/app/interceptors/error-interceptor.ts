import {HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpInterceptorFn, HttpRequest} from '@angular/common/http';
import {catchError, Observable, switchMap, throwError} from 'rxjs';
import {inject} from '@angular/core';
import {AuthService} from '../services/auth-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req)
    .pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          if (!req.url.includes('/login')) {
            return handle401Error(req, next);
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

const handle401Error = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<any> => {
  const authService = inject(AuthService);

  return authService.refreshToken()
    .pipe(
      switchMap(() => {
        return next(addToken(req))
      }),
      catchError(error => {
        console.error("Failed to refresh token:" , error);
        authService.logout();
        return throwError(() => error);
      })
    )
}


