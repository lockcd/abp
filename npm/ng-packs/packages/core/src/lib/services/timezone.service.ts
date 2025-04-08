import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TimezoneService {
  private readonly cookieKey = '__timezone';

  getBrowserTimezone(): string {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
  }

  getTimezone(): string {
    const fromCookie = this.getCookie(this.cookieKey);
    return fromCookie || this.getBrowserTimezone();
  }

  setTimezone(timezone: string): void {
    document.cookie = `${this.cookieKey}=${timezone}; path=/`;
  }

  convertUtcToLocal(date: string | Date): Date {
    return new Date(date + 'Z');
  }

  convertLocalToUtc(date: Date): string {
    return date.toISOString();
  }

  private getCookie(name: string): string | null {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    return match ? decodeURIComponent(match[2]) : null;
  }
}
