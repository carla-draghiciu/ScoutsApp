import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { InactivityService } from './services/inactivity.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
  providers: [CookieService],
})
export class App implements OnInit {
  protected readonly title = signal('scout-app');

  constructor(private inactivityService: InactivityService) {}

  ngOnInit(): void {
    this.inactivityService.startTracking();
  }
}
