import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Badge {
  id: number;
  name: string;
}

export interface EventAttendee {
  attendeeId: number;
  attendeeName?: string;
}

export interface EventModel {
  id: number;
  name: string;
  location: string;
  startDate: string;
  endDate: string;
  price: number;
  registrationDeadline: string; // yyyy-mm-dd
  description: string;
  equipment: string;
  creatorId: number;
  attendees: EventAttendee[];
  badge: Badge;
}

@Injectable({
  providedIn: 'root',
})
export class EventService {
  private apiUrl = "https://localhost:7239/api/Events";

  constructor(private http: HttpClient) { }

  getAll(status: string, location: string, price: string, pageNumber: number, pageSize: number): Observable<EventModel[]> {
    const token = localStorage.getItem('token');

    return this.http.get<EventModel[]>(`${this.apiUrl}?statusFilter=${status}&locationFilter=${location}&priceFilter=${price}&pageNumber=${pageNumber}&pageSize=${pageSize}`, {
      headers: { 'Authorization': `Bearer ${token}` }
    });
  }

  getLocations(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/locations`);
  }

  getById(id: number): Observable<EventModel | undefined> {
    return this.http.get<EventModel | undefined>(`${this.apiUrl}/${id}`);
  }

  getByCreatorId(creatorId: number): Observable<EventModel[]> {
    return this.http.get<EventModel[]>(`${this.apiUrl}/byUser/${creatorId}`);
  }

  add(draft: Omit<EventModel, 'id' | 'creatorId' | 'attendees' | 'badge'>): Observable<EventModel> {
    const token = localStorage.getItem('token');

    return this.http.post<EventModel>(this.apiUrl, draft, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  update(id: number, draft: Partial<Omit<EventModel, 'id' | 'creatorId' | 'attendees'>>): Observable<EventModel | undefined> {
    return this.http.put<EventModel>(`${this.apiUrl}/${id}`, draft);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  toggleAttendance(eventId: number): Observable<void> {
    const token = localStorage.getItem('token');

    return this.http.put<void>(`${this.apiUrl}/attendance/${eventId}`, {}, {
      headers: {
        Authorization: `Bearer ${token}`
      }
    });
  }
}
