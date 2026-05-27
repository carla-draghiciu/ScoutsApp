export interface Badge {
  id: number;
  name: string;
}

export interface EventAttendee {
  attendeeId: number;
  attendeeName?: string;
}

export interface EventModel {
  id: number;
  name: string;
  location: string;
  startDate: string;
  endDate: string;
  price: number;
  registrationDeadline: string; // yyyy-mm-dd
  description: string;
  equipment: string;
  creatorId: number;
  attendees: EventAttendee[];
  badge: Badge;
}