import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { CreateEventForm } from './create-event-form';

describe.skip('CreateEventForm', () => {
  let component: CreateEventForm;
  let fixture: ComponentFixture<CreateEventForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RouterTestingModule, CreateEventForm],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateEventForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
