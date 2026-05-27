import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EventService } from '../../services/event';
import { CookieService } from '../../services/cookie.service';
import { OnInit } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';
import { PermissionService } from '../../services/permission';
import { EventModel } from '../../models/event.model';
import { debounceTime, distinctUntilChanged, Subject, switchMap } from 'rxjs';

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

  searchQuery = '';
  isSearching = false;

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

  onSearchKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      console.log('Enter pressed. Search query:', this.searchQuery);
      this.pageNumber = 1; // reset to first page on new search
      this.performSearch();
    }
  }

  performSearch() {
    if (this.searchQuery.trim()) {
      this.isSearching = true;
      this.service.search(this.searchQuery, this.pageNumber, this.pageSize)
        .subscribe((result: any) => {
          console.log('Search results for query:', this.searchQuery, result);
          this.events = result.items;
          this.eventsCount = result.totalCount;
          this.cdr.detectChanges();
        });
    } else {
      console.log('Search query is empty. Reloading events.');
      this.isSearching = false;
      this.loadEvents(); // back to normal
    }
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
    this.pageNumber = newPage;
    this.service.getAll(this.filterStatus, this.filterLocation, this.filterPrice, this.pageNumber, this.pageSize).subscribe({
      next: (response: any) => {
        this.events = response.items;
        this.eventsCount = response.totalCount;
        this.cdr.detectChanges();
      },
    });
  }

  onPageChange(page: number) {
    this.pageNumber = page;
    if (this.isSearching) {
      this.performSearch();
    } else {
      this.loadEvents();
    }
  }
}