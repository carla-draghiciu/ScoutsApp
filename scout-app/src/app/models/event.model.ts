export interface Event {
  id: number;
  name: string;
  location: string;
  startDate: string;
  endDate: string;
  price: number;
  registrationDeadline: string;
  description: string;
  equipment: string;
  creatorId: number;
  attendees: number[];
}