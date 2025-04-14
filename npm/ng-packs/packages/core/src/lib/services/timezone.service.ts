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
  public isUtcClockEnabled: boolean | undefined;

  constructor() {
    this.configState.getOne$('setting').subscribe(settings => {
      this.timeZoneNameFromSettings = settings?.values?.['Abp.Timing.TimeZone'];
    });
    this.configState.getOne$('clock').subscribe(clock => {
      this.isUtcClockEnabled = clock?.kind === 'Utc';
    });
  }

  getBrowserTimezone(): string {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
  }

  get timezone(): string {
    if (!this.isUtcClockEnabled) {
      return this.getBrowserTimezone();
    }
    return this.timeZoneNameFromSettings || this.getBrowserTimezone();
  }

  setTimezone(timezone: string): void {
    if (this.isUtcClockEnabled) {
      this.document.cookie = `${this.cookieKey}=${timezone}; path=/`;
    }
  }
}
