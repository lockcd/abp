import { Pipe, PipeTransform, Injectable, inject } from '@angular/core';
import { ConfigStateService, LocalizationService, TimezoneService } from '../services';

@Injectable()
@Pipe({
  name: 'abpUtcToLocal',
})
export class UtcToLocalPipe implements PipeTransform {
  protected readonly timezoneService = inject(TimezoneService);
  protected readonly configState = inject(ConfigStateService);
  protected readonly localizationService = inject(LocalizationService);

  transform(value: string | Date | null | undefined): string | Date {
    if (!value) return '';

    try {
      const dateInput = new Date(value);

      if (isNaN(dateInput.getTime())) {
        return '';
      }

      const localization = this.configState.getOne('localization');
      const locale = localization?.currentCulture?.cultureName ?? 'en-US';
      const options: Intl.DateTimeFormatOptions = this.timezoneService.isUtcClockEnabled
        ? { timeZone: this.timezoneService.getTimezone() }
        : undefined;
      return dateInput.toLocaleString(locale, options);
    } catch (err) {
      return value;
    }
  }
}
