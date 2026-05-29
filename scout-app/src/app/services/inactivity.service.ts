import { Injectable, NgZone, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth';

const INACTIVITY_LIMIT_MS = 30 * 60 * 1000;

const ACTIVITY_EVENTS = [
  'mousemove',
  'mousedown',
  'keydown',
  'touchstart',
  'scroll',
  'click',
];

@Injectable({
  providedIn: 'root',
})
export class InactivityService implements OnDestroy {
  private timer: ReturnType<typeof setTimeout> | null = null;
  private boundReset = this.resetTimer.bind(this);

  constructor(
    private authService: AuthService,
    private router: Router,
    private ngZone: NgZone
  ) {}

  startTracking(): void {
    this.ngZone.runOutsideAngular(() => {
      ACTIVITY_EVENTS.forEach((event) =>
        window.addEventListener(event, this.boundReset, { passive: true })
      );
    });
    this.resetTimer();
  }

  private resetTimer(): void {
    if (this.timer) {
      clearTimeout(this.timer);
    }
    this.timer = setTimeout(() => this.handleInactivity(), INACTIVITY_LIMIT_MS);
  }

  private handleInactivity(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      return;
    }

    this.ngZone.run(() => {
      this.authService.logout().subscribe({
        complete: () => this.clearSessionAndRedirect(),
        error: () => this.clearSessionAndRedirect(),
      });
    });
  }

  private clearSessionAndRedirect(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    this.router.navigate(['/']);
  }

  ngOnDestroy(): void {
    if (this.timer) {
      clearTimeout(this.timer);
    }
    ACTIVITY_EVENTS.forEach((event) =>
      window.removeEventListener(event, this.boundReset)
    );
  }
}
