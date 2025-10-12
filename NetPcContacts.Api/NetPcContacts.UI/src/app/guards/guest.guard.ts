import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';

/**
 * Guard that prevents authenticated users from accessing guest-only routes (login, register).
 * If user is logged in, redirects to /contacts.
 */
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    router.navigate(['/contacts']);
    return false;
  }

  return true;
};
