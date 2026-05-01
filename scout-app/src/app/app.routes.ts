import { Routes } from '@angular/router';
import { Home } from './components/home/home';
import { EventsList } from './components/events-list/events-list';
import { EventDetail } from './components/event-detail/event-detail';
import { CreateEventForm } from './components/create-event-form/create-event-form';
import { Register } from './components/register/register';
import { Login } from './components/login/login';
import { Profile } from './components/profile/profile';
import { Badges } from './components/badges/badges';
import { Statistics } from './components/statistics/statistics';
import { EditEvent } from './components/edit-event/edit-event';
import { MyEvents } from './components/my-events/my-events';
import { EditProfile } from './components/edit-profile/edit-profile';

// export const routes: Routes = [];
export const routes: Routes = [
  { path: '', component: Home },
  { path: 'register', component: Register },
  { path: 'login', component: Login },
  { path: 'profile', component: Profile },
  { path: 'badges', component: Badges },
  { path: 'statistics', component: Statistics },
  { path: 'events', component: EventsList },
  { path: 'events/:id', component: EventDetail },
  { path: 'myevents', component: MyEvents },
  { path: 'myevents/:id', component: EditEvent },
  { path: 'create', component: CreateEventForm },
  { path: 'edit/:id', component: CreateEventForm },
  { path: 'editprofile', component: EditProfile }
];
