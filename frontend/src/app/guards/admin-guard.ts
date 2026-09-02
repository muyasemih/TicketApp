import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const adminGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token = localStorage.getItem('token');

  if (!token) {
    return router.createUrlTree(['/login']);
  }

  try {
    const base64 = token
      .split('.')[1]
      .replace(/-/g, '+')
      .replace(/_/g, '/');

    const padded = base64.padEnd(
      base64.length + (4 - base64.length % 4) % 4,
      '='
    );

    const payload = JSON.parse(atob(padded));

    const role =
      payload[
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
      ] ?? payload.role;

    if (role === 'Admin') {
      return true;
    }

    return router.createUrlTree(['/events']);
  } catch {
    localStorage.removeItem('token');
    return router.createUrlTree(['/login']);
  }
};