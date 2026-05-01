import { TestBed } from '@angular/core/testing';
import { EventService } from './event';
import { takeLast } from 'rxjs';


describe('EventService', () => {
  let service: EventService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EventService);
  });


  it('should initialize with default events', () => {
    const events = service.getAll();
    expect(events.length).toBeGreaterThan(0);
  });


  it('should return events sorted by id', () => {
    const events = service.getAll();
    for (let i = 1; i < events.length; i++) {
      expect(events[i].id).toBeGreaterThan(events[i - 1].id);
    }
  });


  it('should return event by id', () => {
    const event = service.getById(1);
    expect(event).toBeDefined();
    expect(event?.id).toBe(1);
  });


  it('should return undefined for invalid id', () => {
    const event = service.getById(999);
    expect(event).toBeUndefined();
  });


  it('should add a new event', () => {
    const newEvent = service.add({
      name: 'Test Event',
      location: 'Test Location',
      startDate: '2026-01-01',
      endDate: '2026-01-02',
      price: 50,
      registrationDeadline: '2025-12-31',
      description: 'Test Description',
      equipment: 'Test Equipment'
    });

    expect(newEvent.id).toBeDefined();
    expect(service.getAll().length).toBeGreaterThan(7);
  });


  it('should default price to 0 if invalid', () => {
    const event = service.add({
      name: 'Invalid Price Event',
      location: 'Nowhere',
      startDate: '2026-01-01',
      endDate: '2026-01-02',
      price: NaN,
      registrationDeadline: '2025-12-31',
      description: '',
      equipment: ''
    });

    expect(event.price).toBe(0);
  });


  it('should update an existing event', () => {
    const updated = service.update(1, {
      name: 'Updated Name'
    });

    expect(updated).toBeDefined();
    expect(updated?.name).toBe('Updated Name');
  });


  it('should convert price to number on update', () => {
    const updated = service.update(1, {
      price: '100' as any
    });

    expect(updated?.price).toBe(100);
  });

  it('should keep existing price if draft.price is undefined', () => {
    const service = new EventService();

    const event = service.add({
      name: 'Test',
      location: 'Cluj',
      startDate: '2026-05-01',
      endDate: '2026-05-02',
      price: 15,
      registrationDeadline: '2026-04-28',
      description: '',
      equipment: ''
    });

    const updated = service.update(event.id, {
      name: 'Updated name'
    });

    expect(updated?.price).toBe(15);
  });


  it('should set price to 0 if draft.price is invalid', () => {
    const service = new EventService();

    const event = service.add({
      name: 'Test',
      location: 'Cluj',
      startDate: '2026-05-01',
      endDate: '2026-05-02',
      price: 10,
      registrationDeadline: '2026-04-28',
      description: '',
      equipment: ''
    });

    const updated = service.update(event.id, {
      price: NaN as any
    });

    expect(updated?.price).toBe(0);
  });


  it('should return undefined when updating non-existing event', () => {
    const updated = service.update(999, {
      name: 'No Event'
    });

    expect(updated).toBeUndefined();
  });


  it('should delete an event', () => {
    const initialLength = service.getAll().length;

    service.delete(1);

    const newLength = service.getAll().length;
    expect(newLength).toBe(initialLength - 1);
  });


  it('should not crash when deleting non-existing event', () => {
    const initialLength = service.getAll().length;

    service.delete(999);

    expect(service.getAll().length).toBe(initialLength);
  });


  it('should add user to attendees if not present', () => {
    const event = service.getById(6); // has empty attendees
    const userId = 999;

    service.toggleAttendance(6, userId);

    expect(event?.attendees.includes(userId)).toBe(true);
  });


  it('should remove user if already attending', () => {
    const event = service.getById(1);
    const userId = event!.attendees[0];

    service.toggleAttendance(1, userId);

    expect(event?.attendees.includes(userId)).toBe(false);
  });


  it('should not crash when toggling attendance for invalid event', () => {
    expect(() => service.toggleAttendance(999, 1)).not.toThrow();
  });
});