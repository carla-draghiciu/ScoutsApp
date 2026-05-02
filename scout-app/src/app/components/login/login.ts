import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService, LoggingUser } from '../../services/auth';


@Component({
  selector: 'app-login',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  email = '';
  password = '';

  constructor(private router: Router, private authService: AuthService) {}

  submit() {
    const loggingUser: LoggingUser = {
      email: this.email,
      password: this.password
    };

    this.authService.login(loggingUser).subscribe({
      next: (response: any) => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('userName', response.name);
        localStorage.setItem('userId', response.userId);
        localStorage.setItem('role', response.role);
        localStorage.setItem('permissions', JSON.stringify(response.permissions));
        this.router.navigate(['/events']);
      },
      error: err => {
        alert('Login failed: ' + (err.error || 'Unknown error')); 
      }
    });
  }
}
