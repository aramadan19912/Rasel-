import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root'
})
export class PermissionGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    // Check if user is authenticated
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: state.url }
      });
      return false;
    }

    // Get required permissions from route data
    const requiredPermissions = route.data['permissions'] as string[];
    const requireAll = route.data['requireAllPermissions'] as boolean || false;

    if (!requiredPermissions || requiredPermissions.length === 0) {
      // No specific permissions required
      return true;
    }

    // Check permissions
    const hasPermission = requireAll
      ? this.authService.hasAllPermissions(requiredPermissions)
      : this.authService.hasAnyPermission(requiredPermissions);

    if (!hasPermission) {
      this.snackBar.open('You do not have permission to access this page.', 'Close', {
        duration: 5000,
        panelClass: ['error-snackbar']
      });
      this.router.navigate(['/unauthorized']);
      return false;
    }

    return true;
  }
}
