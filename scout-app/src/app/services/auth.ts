import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { RegisteringUser, LoggingUser, UserProfile } from '../models/user.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  env = environment.apiUrl;
  private apiUrl = `${this.env}/api/Authentificator`;

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

  getUserById(id: number): Observable<UserProfile | undefined> {
    return this.http.get<UserProfile | undefined>(`${this.apiUrl}/${id}`);
  }
}
