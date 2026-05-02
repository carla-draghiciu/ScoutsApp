import { Component } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EventModel, EventService } from '../../services/event';
import { CookieService } from '../../services/cookie.service';
import { OnInit } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';
import { AuthService, UserProfile } from '../../services/auth';
import { PermissionService } from '../../services/permission';

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

  canJoinEvent: boolean = false;

  constructor(
    private route: ActivatedRoute, 
    private service: EventService, 
    private cookie: CookieService,
    private cdr: ChangeDetectorRef,
    private userService: AuthService,
    private permissionService: PermissionService
  ) { }

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');
    const id = idParam ? Number(idParam) : NaN;

    this.canJoinEvent = this.permissionService.hasPermission('join_event');

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
    if (!this.canJoinEvent) {
      alert('You do not have permission to join events.');
      return;
    }

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