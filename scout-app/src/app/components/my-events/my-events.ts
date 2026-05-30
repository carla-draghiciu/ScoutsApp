import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EventService } from '../../services/event';
import { OnInit } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';
import { PermissionService } from '../../services/permission';
import { EventModel } from '../../models/event.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-my-events',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './my-events.html',
  styleUrl: './my-events.css',
})
export class MyEvents implements OnInit {
  events: EventModel[] = [];
  page = 1;
  pageSize = 6;
  userId = Number(localStorage.getItem('userId'));
  isAdmin: boolean = false;
  isLoading = false;

  isAttending(event: EventModel): boolean {
    return event?.attendees?.some(a => a.attendeeId === this.userId) ?? false;
  }

  constructor(
    private service: EventService,
    private cdr: ChangeDetectorRef,
    private permissionService: PermissionService
  ) {}

  ngOnInit() {
    this.isAdmin = this.permissionService.isAdmin();
    this.isLoading = true;
    this.service.getByCreatorId(this.userId).subscribe(usersEvents => {
      this.isLoading = false;
      this.events = usersEvents;
      this.cdr.detectChanges();
    })
  }

  get paginatedEvents(): EventModel[] {
    const start = (this.page - 1) * this.pageSize;
    return this.events.slice(start, start + this.pageSize);
  }
}
