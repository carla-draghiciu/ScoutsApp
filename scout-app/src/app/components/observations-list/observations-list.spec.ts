import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ObservationsList } from './observations-list';

describe('ObservationsList', () => {
  let component: ObservationsList;
  let fixture: ComponentFixture<ObservationsList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ObservationsList],
    }).compileComponents();

    fixture = TestBed.createComponent(ObservationsList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
