import { Pipe, PipeTransform, Injectable, inject } from '@angular/core';
import { LocalizationService, TimezoneService } from '../services';

@Injectable()
@Pipe({
  name: 'abpUtcToLocal',
})
export class UtcToLocalPipe implements PipeTransform {
  protected readonly timezoneService = inject(TimezoneService);
  protected readonly localizationService = inject(LocalizationService);

  transform(
    value: string | Date | null | undefined,
    apply: boolean,
    options?: Intl.DateTimeFormatOptions,
  ): string | Date {
    if (!apply) return value;
    if (!value) return '';

    try {
      const dateInput = new Date(value);

      if (isNaN(dateInput.getTime())) {
        // Invalid date
        return '';
      }

      const formatOptions: Intl.DateTimeFormatOptions = options || {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      };
      console.log(this.timezoneService.getTimezone());
      const formatter = new Intl.DateTimeFormat('en-US', {
        ...formatOptions,
        timeZone: this.timezoneService.getTimezone(),
      });

      return formatter.format(dateInput);
    } catch (err) {
      return value;
    }
  }
}
