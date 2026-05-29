import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth';
import { LoggingUser } from '../../models/user.model';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  email = '';
  password = '';
  isLoading = false;

  constructor(
    private router: Router, 
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  submit() {
    const loggingUser: LoggingUser = {
      identifier: this.email,
      password: this.password
    };

    this.isLoading = true;
    this.authService.login(loggingUser).subscribe({
      next: (response: any) => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('userName', response.name);
        localStorage.setItem('userId', response.userId);
        localStorage.setItem('role', response.role);
        localStorage.setItem('permissions', JSON.stringify(response.permissions));
        this.router.navigate(['/events']);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.isLoading = false;
        this.cdr.detectChanges();
        alert('Login failed: ' + (err.error || 'Unknown error'));
      }
    });
  }
}
