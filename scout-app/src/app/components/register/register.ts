import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth';
import { RegisteringUser } from '../../models/user.model';
import { ChangeDetectorRef } from '@angular/core';

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
  isLoading = false;

  constructor(
    private router: Router, 
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  submit() {
    const registering: RegisteringUser = {
      name: this.name,
      email: this.email,
      scoutId: this.scoutId,
      dateOfBirth: this.dateOfBirth,
      scoutLevel: this.scoutLevel,
      password: this.password
    };

    this.isLoading = true;
    this.authService.register(registering).subscribe({
      next: () => {
        this.router.navigate(['/login']);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.isLoading = false;
        this.cdr.detectChanges();
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
