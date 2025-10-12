import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideNativeDateAdapter } from '@angular/material/core';

import { routes } from './app.routes';
import {provideHttpClient, withInterceptors} from '@angular/common/http';
import {jwtInterceptor} from './interceptors/jwt-interceptor';
import {errorInterceptor} from './interceptors/error-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideNativeDateAdapter(),
    provideHttpClient(
      withInterceptors([
        jwtInterceptor,
        errorInterceptor
      ])
    ),
  ]
};
