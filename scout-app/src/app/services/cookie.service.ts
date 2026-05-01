import { Injectable } from '@angular/core';
import { CookieService as NgxCookieService } from 'ngx-cookie-service';

@Injectable({ providedIn: 'root' })
export class CookieService {

  constructor(private cookie: NgxCookieService) {}

  set(name: string, value: string) {
    this.cookie.set(name, value);
  }

  get(name: string): string {
    return this.cookie.get(name);
  }

  delete(name: string) {
    this.cookie.delete(name);
  }
}