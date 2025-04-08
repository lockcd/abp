import { Pipe, PipeTransform, Injectable, inject } from '@angular/core';
import { TimezoneService } from '../services';

@Injectable()
@Pipe({
  name: 'utcToLocal',
  standalone: false,
})
export class UtcToLocalPipe implements PipeTransform {
  protected readonly timezoneService = inject(TimezoneService);
  private readonly timezone: string;

  constructor() {
    this.timezone = this.timezoneService.getTimezone();
  }

  transform(
    value: string | Date | null | undefined,
    locale: string = navigator.language,
    options?: Intl.DateTimeFormatOptions,
  ): string {
    if (!value) return '';

    try {
      const utcDate = new Date(value + 'Z'); // Ensure it's treated as UTC

      const formatOptions: Intl.DateTimeFormatOptions = options || {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      };

      const formatter = new Intl.DateTimeFormat(locale, {
        ...formatOptions,
        timeZone: this.timezone,
      });

      return formatter.format(utcDate);
    } catch (err) {
      return '';
    }
  }
}
