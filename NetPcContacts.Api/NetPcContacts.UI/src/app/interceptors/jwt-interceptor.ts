import { HttpInterceptorFn } from '@angular/common/http';
import {inject} from '@angular/core';

import {AuthService} from '../services/auth-service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const isLoggedIn = authService.isLoggedIn();

  if (isLoggedIn) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${localStorage.getItem('accessToken')}`
      }
    })
  }

  return next(req);
};
