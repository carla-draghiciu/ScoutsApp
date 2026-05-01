import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EventService } from '../../services/event';
import type { Event } from '../../models/event.model';


@Component({
  selector: 'app-profile',
  imports: [RouterLink],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile {
  events: Event[] = [];
  pageSize = 3;
  userId = 201;

  constructor(private service: EventService) { }

  get myEvents(): Event[] {
    const myEvents = this.events.filter(e => e.creatorId === this.userId);
    const start = 0;
    return myEvents.slice(start, start + this.pageSize);
  }

}
