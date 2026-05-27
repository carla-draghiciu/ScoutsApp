import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaders } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { EventModel } from '../models/event.model';

@Injectable({
  providedIn: 'root',
})
export class EventService {
  env = environment.apiUrl;
  
  private apiUrl = `${this.env}/api/Events`;
  
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${localStorage.getItem('token')}` });
  }

  constructor(private http: HttpClient) { }

  getAll(status: string, location: string, price: string, pageNumber: number, pageSize: number): Observable<EventModel[]> {
    return this.http.get<EventModel[]>(`${this.apiUrl}?statusFilter=${status}&locationFilter=${location}&priceFilter=${price}&pageNumber=${pageNumber}&pageSize=${pageSize}`, { headers: this.getHeaders() });
  }

  getLocations(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/locations`, { headers: this.getHeaders() });
  }

  getById(id: number): Observable<EventModel | undefined> {
    return this.http.get<EventModel | undefined>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  getByCreatorId(creatorId: number): Observable<EventModel[]> {
    return this.http.get<EventModel[]>(`${this.apiUrl}/byUser/${creatorId}`, { headers: this.getHeaders() });
  }

  add(draft: Omit<EventModel, 'id' | 'creatorId' | 'attendees' | 'badge'>): Observable<EventModel> {
    return this.http.post<EventModel>(this.apiUrl, draft, { headers: this.getHeaders() });
  }

  update(id: number, draft: Partial<Omit<EventModel, 'id' | 'creatorId' | 'attendees'>>): Observable<EventModel | undefined> {
    return this.http.put<EventModel>(`${this.apiUrl}/${id}`, draft, { headers: this.getHeaders() });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  toggleAttendance(eventId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/attendance/${eventId}`, {}, { headers: this.getHeaders() });
  }

  search(query: string, pageNumber: number, pageSize: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/search?query=${query}&pageNumber=${pageNumber}&pageSize=${pageSize}`, { headers: this.getHeaders() });
  }
}
