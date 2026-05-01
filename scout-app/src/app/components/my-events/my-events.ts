import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EventModel, EventService } from '../../services/event';
import { OnInit } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';


@Component({
  selector: 'app-my-events',
  imports: [CommonModule, RouterLink],
  templateUrl: './my-events.html',
  styleUrl: './my-events.css',
})
export class MyEvents implements OnInit {
  events: EventModel[] = [];
  page = 1;
  pageSize = 6;
  userId = Number(localStorage.getItem('userId'));

  isAttending(event: EventModel): boolean {
    return event?.attendees?.some(a => a.attendeeId === this.userId) ?? false;
  }

  constructor(
    private service: EventService,
    private cdr: ChangeDetectorRef) {
  }

  ngOnInit() {
    this.service.getByCreatorId(this.userId).subscribe(usersEvents => {
      this.events = usersEvents;
      this.cdr.detectChanges();
      console.log('MyEvents ngOnInit: fetched events=', this.events);
    })
  }

  get paginatedEvents(): EventModel[] {
    const start = (this.page - 1) * this.pageSize;
    return this.events.slice(start, start + this.pageSize);
  }
}
