import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService, RegisteringUser } from '../../services/auth';

@Component({
  selector: 'app-register',
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  name = '';
  email = '';
  scoutId = '';
  dateOfBirth = '';
  scoutLevel = 'Lupisor';
  password = '';
  confirmPassword = '';

  constructor(private router: Router, private authService: AuthService) {}

  submit() {
    const registering: RegisteringUser = {
      name: this.name,
      email: this.email,
      scoutId: this.scoutId,
      dateOfBirth: this.dateOfBirth,
      scoutLevel: this.scoutLevel,
      password: this.password
    };

    this.authService.register(registering).subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: err => {
      console.log(err);
      
      if (typeof err.error === 'string') {
        alert(err.error);
      } else if (err.error?.title) {
        alert(err.error.title);
      } else if (err.error?.message) {
        alert(err.error.message);
      } else {
        alert('Registration failed. Please try again.');
      }
    }
    });
  }
}
