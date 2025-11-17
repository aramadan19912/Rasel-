import { Directive, Input, TemplateRef, ViewContainerRef, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Directive({
  selector: '[appHasRole]',
  standalone: false
})
export class HasRoleDirective implements OnInit, OnDestroy {
  private roles: string[] = [];
  private destroy$ = new Subject<void>();

  @Input()
  set appHasRole(roles: string | string[]) {
    this.roles = Array.isArray(roles) ? roles : [roles];
    this.updateView();
  }

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Listen to auth state changes
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateView();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateView(): void {
    const hasRole = this.checkRole();

    if (hasRole) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }

  private checkRole(): boolean {
    if (!this.roles || this.roles.length === 0) {
      return true;
    }

    return this.roles.some(role => this.authService.hasRole(role));
  }
}
