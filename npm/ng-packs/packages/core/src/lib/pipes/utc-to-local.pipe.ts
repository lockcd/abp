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

  transform(value: string | Date | null | undefined, apply: boolean): string | Date {
    if (!apply) return value;
    if (!value) return '';

    try {
      const dateInput = new Date(value);

      if (isNaN(dateInput.getTime())) {
        // Invalid date
        return '';
      }
      const localization = this.configState.getOne('localization');
      return dateInput.toLocaleString(localization?.currentCulture?.cultureName ?? 'en-US', {
        timeZone: this.timezoneService.getTimezone(),
      });
    } catch (err) {
      return value;
    }
  }
}
