import { Component } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EventModel, EventService } from '../../services/event';
import { CookieService } from '../../services/cookie.service';
import { OnInit } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';
import { AuthService, UserProfile } from '../../services/auth';

@Component({
  selector: 'app-event-detail',
  imports: [RouterLink],
  templateUrl: './event-detail.html',
  styleUrl: './event-detail.css',
})
export class EventDetail implements OnInit {
  event: EventModel | undefined;
  userId = Number(localStorage.getItem('userId'));
  creatorProfile: UserProfile | undefined;

  constructor(
    private route: ActivatedRoute, 
    private service: EventService, 
    private cookie: CookieService,
    private cdr: ChangeDetectorRef,
    private userService: AuthService
  ) { }

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : NaN;
    console.log('EventDetail ngOnInit: idParam=', idParam, 'id=', id);
    console.log(typeof id);
    

    this.service.getById(id).subscribe(event => {
      this.event = event;
      this.cdr.detectChanges();
      if (this.event) {
        this.cookie.set('lastViewedEventId', this.event.id.toString());
      }

      this.userService.getUserById(this.event?.creatorId || NaN).subscribe(profile => {
        this.creatorProfile = profile;
        this.cdr.detectChanges();
      });
    });
  }

  isAttending(): boolean {
    return this.event?.attendees?.some(a => a.attendeeId === this.userId) ?? false;
  }


  toggleAttendance(): void {
    if (this.event) {
      this.service.toggleAttendance(this.event.id).subscribe({
        
        next: () => {
          console.log('EventDetail toggleAttendance: toggled attendance for eventId=', this.event!.id);
          // refresh
          this.service.getById(this.event!.id).subscribe(updatedEvent => {
            this.event = updatedEvent;
            this.cdr.detectChanges();
          });
        },
        error: err => console.error('Failed to toggle attendance:', err)
      });
    }
  }
}