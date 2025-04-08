import { inject, Injectable } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { ConfigStateService } from './config-state.service';

@Injectable({
  providedIn: 'root',
})
export class TimezoneService {
  protected readonly configState = inject(ConfigStateService);
  protected readonly document = inject(DOCUMENT);
  private readonly cookieKey = '__timezone';
  private timeZoneNameFromSettings: string | null | undefined;

  constructor() {
    this.configState.getOne$('timing').subscribe(timezoneSettings => {
      this.timeZoneNameFromSettings = timezoneSettings?.timeZone?.iana?.timeZoneName;
    });
  }

  getBrowserTimezone(): string {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
  }

  getTimezone(): string {
    const fromCookie = this.getCookie(this.cookieKey);
    return this.timeZoneNameFromSettings || fromCookie || this.getBrowserTimezone();
  }

  setTimezone(timezone: string): void {
    this.document.cookie = `${this.cookieKey}=${timezone}; path=/`;
  }

  convertUtcToLocal(date: string | Date): Date {
    return new Date(date + 'Z');
  }

  convertLocalToUtc(date: Date): string {
    return date.toISOString();
  }

  private getCookie(name: string): string | null {
    const match = this.document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    return match ? decodeURIComponent(match[2]) : null;
  }
}
