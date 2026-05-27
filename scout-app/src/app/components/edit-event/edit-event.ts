import { ChangeDetectorRef, Component } from '@angular/core';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EventService } from '../../services/event';
import { OnInit } from '@angular/core';
import { PermissionService } from '../../services/permission';

@Component({
  selector: 'app-edit-event',
  imports: [RouterLink, FormsModule],
  templateUrl: './edit-event.html',
  styleUrl: './edit-event.css',
})
export class EditEvent implements OnInit {
  eventId: number = 0;
  event: any = {
    name: '',
    location: '',
    startDate: '',
    endDate: '',
    price: null,
    registrationDeadline: '',
    description: '',
    equipment: '',
  };
  errorMessage = '';
  canEditEvent: boolean = false;
  canDeleteEvent: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private service: EventService,
    private cdr: ChangeDetectorRef,
    private permissionService: PermissionService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    this.canEditEvent = this.permissionService.hasPermission('update_event');
    this.canDeleteEvent = this.permissionService.hasPermission('delete_event');

    this.eventId = idParam ? Number(idParam) : NaN;
    this.service.getById(this.eventId).subscribe((existingEvent) => {
      if (existingEvent) {
        this.event = { ...existingEvent };
        this.cdr.detectChanges();
      } else {
        this.router.navigate(['/profile']);
      }
    });
  }

  submit(): void {
    if (!this.canEditEvent) {
      alert('You do not have permission to edit events.');
      return;
    }

    this.errorMessage = '';
    
    if (!this.event.name || !this.event.description || !this.event.startDate || !this.event.endDate || !this.event.registrationDeadline || !this.event.location || this.event.price === null || this.event.price === undefined) {
      this.errorMessage = 'All fields except equipment are required.';
      return;
    }

    const start = new Date(this.event.startDate);
    const end = new Date(this.event.endDate);
    const deadline = new Date(this.event.registrationDeadline);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (start <= today) {
      this.errorMessage = 'Start date must be after current date.';
      return;
    }

    if (deadline <= today) {
      this.errorMessage = 'Registration deadline must be after current date.';
      return;
    }

    if (start >= end) {
      this.errorMessage = 'Start date must be before end date.';
      return;
    }

    this.service.update(this.eventId, {
      name: this.event.name.trim(),
      location: this.event.location.trim(),
      startDate: this.event.startDate,
      endDate: this.event.endDate,
      price: Number(this.event.price) || 0,
      registrationDeadline: this.event.registrationDeadline,
      description: this.event.description.trim(),
      equipment: this.event.equipment.trim(),
    }).subscribe(() => {
      this.router.navigate(['/myevents']);
    });
  }

  deleteEvent(): void {
    if (!this.canDeleteEvent) {
      alert('You do not have permission to delete events.');
      return;
    }

    if (confirm('Are you sure you want to delete this event?')) {
      this.service.delete(this.eventId).subscribe(() => {
        this.router.navigate(['/myevents']);
      });
    }
  }
}
