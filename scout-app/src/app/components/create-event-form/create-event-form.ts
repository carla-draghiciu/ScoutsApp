import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { EventService } from '../../services/event';

@Component({
  selector: 'app-create-event-form',
  imports: [FormsModule, RouterLink],
  templateUrl: './create-event-form.html',
  styleUrl: './create-event-form.css',
})
export class CreateEventForm {
  event: {
    name: string;
    location: string;
    startDate: string;
    endDate: string;
    price: number | null;
    registrationDeadline: string;
    description: string;
    equipment: string;
  } = {
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

  constructor(private service: EventService, private router: Router) { }

  submit(): void {
    console.log('CreateEventForm submit: event=', this.event);
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

    // const created = this.service.add({
    //   name: this.event.name.trim(),
    //   location: this.event.location.trim(),
    //   startDate: this.event.startDate,
    //   endDate: this.event.endDate,
    //   price: Number(this.event.price) || 0,
    //   registrationDeadline: this.event.registrationDeadline,
    //   description: this.event.description.trim(),
    //   equipment: this.event.equipment.trim(),
    // });

    this.service.add({
      name: this.event.name.trim(),
      location: this.event.location.trim(),
      startDate: this.event.startDate,
      endDate: this.event.endDate,
      price: Number(this.event.price) || 0,
      registrationDeadline: this.event.registrationDeadline,
      description: this.event.description.trim(),
      equipment: this.event.equipment.trim(),
    }).subscribe(created => {
      alert(`Event "${created.name}" created successfully!`);
      this.router.navigate(['/events']);
    });

    // this.router.navigate(['/events']);
  }
}
