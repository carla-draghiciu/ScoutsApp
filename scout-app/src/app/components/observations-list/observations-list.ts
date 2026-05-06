import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChangeDetectorRef } from '@angular/core';
import { environment } from '../../../environments/environment';
import { PermissionService } from '../../services/permission';
import { RouterLink } from '@angular/router';

interface ObservationEntry {
  id: number;
  userId: number;
  user: { name: string; email: string };
  reason: string;
  flaggedAt: string;
  isResolved: boolean;
}

@Component({
  selector: 'app-observations-list',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './observations-list.html',
  styleUrl: './observations-list.css',
})
export class ObservationsList implements OnInit {
  env = environment.apiUrl;
  entries: ObservationEntry[] = [];
  userLogs: any[] = [];
  selectedUserId: number | null = null;
  isAdmin: boolean = false;

  private get headers() {
    return new HttpHeaders({ Authorization: `Bearer ${localStorage.getItem('token')}` });
  }

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef, private permissionService: PermissionService) {}

  ngOnInit() {
    this.loadEntries();
    this.isAdmin = this.permissionService.isAdmin();
  }

  loadEntries() {
    this.http.get<ObservationEntry[]>(`${this.env}/api/admin/observation-list`, { headers: this.headers })
      .subscribe(data =>{ this.entries = data; this.cdr.detectChanges(); });
  }

  viewLogs(userId: number) {
    if (this.selectedUserId === userId) {
      this.selectedUserId = null;
      return;
    }
    this.http.get<any[]>(`${this.env}/api/admin/logs/${userId}`, { headers: this.headers })
      .subscribe(logs => {
        this.userLogs = logs;
        this.selectedUserId = userId;
        this.cdr.detectChanges();
      });
  }

  resolve(entryId: number) {
    this.http.patch(`${this.env}/api/admin/observation-list/${entryId}/resolve`, {}, { headers: this.headers })
      .subscribe(() => this.loadEntries());
  }
}
