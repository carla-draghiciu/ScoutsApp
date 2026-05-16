import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth';
import { PermissionService } from '../../services/permission';

@Component({
  selector: 'app-edit-profile',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './edit-profile.html',
  styleUrl: './edit-profile.css',
})
export class EditProfile {
  isAdmin: boolean = false;
  constructor(private router: Router, private authService: AuthService, private permissionService: PermissionService) {
    this.isAdmin = this.permissionService.isAdmin();
  }

  logout() {
    const token = localStorage.getItem('token');

    this.authService.logout().subscribe({
      next: () => {
        localStorage.removeItem('token');
        this.router.navigate(['/']);
      },
      error: err => {
        console.error('Logout failed:', err);
      }
    });
  }
}
