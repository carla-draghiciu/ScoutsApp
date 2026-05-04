import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EventModel, EventService } from '../../services/event';
import { CookieService } from '../../services/cookie.service';
import type { Event } from '../../models/event.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { OnInit } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';
import { PermissionService } from '../../services/permission';

@Component({
  selector: 'app-events-list',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './events-list.html',
  styleUrl: './events-list.css',
})
export class EventsList implements OnInit {
  events: EventModel[] = [];
  pageNumber = 1;
  pageSize = 6;
  eventsCount = 0;
  userId = Number(localStorage.getItem('userId'));
  prefetchedPage: EventModel[] = [];
  isLoading = false;


  canViewEvents: boolean = false;
  isAdmin: boolean = false;


  allLocations: string[] = [];

  filterStatus: 'all' | 'attending' | 'notAttending' = 'all';
  filterLocation: string = '';
  filterPrice: 'all' | 'free' = 'all';

  isAttending(event: EventModel): boolean {
    return event.attendees?.some(a => a.attendeeId === this.userId) ?? false;
  }

  constructor(
    private cdr: ChangeDetectorRef, 
    private service: EventService, 
    private cookie: CookieService,
    private permissionService: PermissionService
  ) {}

  loadEvents() {
    console.log('user is', this.userId);
    this.service.getAll(this.filterStatus, this.filterLocation, this.filterPrice, this.pageNumber, this.pageSize).subscribe(
      {
        next: (response: any) => {
          this.events = response.items;
          this.eventsCount = response.totalCount;
          this.cdr.detectChanges();
        },
        error: err => console.error('Failed to load events:', err)
      }
    );
  }

  ngOnInit() {
    this.canViewEvents = this.permissionService.hasPermission('view_events');
    this.isAdmin = this.permissionService.isAdmin();
    if (!this.canViewEvents) {
      return;
    }
    this.loadEvents();

    this.service.getLocations().subscribe({
      next: (locations: string[]) => {
        this.allLocations = locations;
        this.cdr.detectChanges();
      },
      error: err => console.error('Failed to load locations:', err)
    });
  }

  get lastViewedEvent(): EventModel | undefined {
    const idStr = this.cookie.get('lastViewedEventId');
    if (idStr) {
      return this.events.find(e => e.id === Number(idStr));
    }
    return undefined;
  }

  onFilterChange() {
    this.pageNumber = 1;
    this.cookie.set('filterLocation', this.filterLocation);
    this.cookie.set('filterPrice', this.filterPrice);
    this.loadEvents();
  }

  get paginatedEvents(): EventModel[] {
    return this.events;
  }

  changePage(newPage: number): void {
    console.log('Changing to page', newPage);
    this.pageNumber = newPage;
    this.service.getAll(this.filterStatus, this.filterLocation, this.filterPrice, this.pageNumber, this.pageSize).subscribe({
      next: (response: any) => {
        console.log('Fetched page', newPage, 'response:', response);
        this.events = response.items;
        this.eventsCount = response.totalCount;
        this.cdr.detectChanges();
      },
    });
  }
}