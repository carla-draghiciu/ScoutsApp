import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RegisteringUser {
  name: string;
  email: string;
  scoutId: string;
  password: string;
  dateOfBirth: string;
  scoutLevel: string;
}

export interface LoggingUser {
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = "https://localhost:7239/api/Authentificator";

  constructor(private http: HttpClient) {}

  register(user: RegisteringUser): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/register`, user);
  }

  login(user: LoggingUser): Observable<{ token: string, userId: number }> {
    return this.http.post<{ token: string, userId: number }>(`${this.apiUrl}/login`, user);
  }

  logout(): Observable<void> {
    const token = localStorage.getItem('token');
    return this.http.post<void>(`${this.apiUrl}/logout`, {}, 
      { 
        headers: { 'Authorization': `Bearer ${token}` } 
      })
  }
}
