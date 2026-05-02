import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PermissionService {
  getRole(): string {
    return localStorage.getItem('role') || '';
  }

  getPermissions(): string[] {
    return JSON.parse(localStorage.getItem('permissions') || '[]');
  }

  hasPermission(permission: string): boolean {
    return this.getPermissions().includes(permission);
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }
}
