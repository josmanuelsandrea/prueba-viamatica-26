import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const allowedRoles: number[] = route.data['roles'] ?? [];
  const userRolId = authService.rolId();

  if (allowedRoles.length === 0 || allowedRoles.includes(userRolId)) {
    return true;
  }

  return router.createUrlTree(['/welcome']);
};
