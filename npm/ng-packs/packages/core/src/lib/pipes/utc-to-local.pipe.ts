import { Pipe, PipeTransform, Injectable, inject, LOCALE_ID } from '@angular/core';
import { ConfigStateService, LocalizationService, TimezoneService } from '../services';
import { getShortDateFormat, getShortDateShortTimeFormat, getShortTimeFormat } from '../utils';
import { formatDate } from '@angular/common';

@Injectable()
@Pipe({
  name: 'abpUtcToLocal',
})
export class UtcToLocalPipe implements PipeTransform {
  protected readonly timezoneService = inject(TimezoneService);
  protected readonly configState = inject(ConfigStateService);
  protected readonly localizationService = inject(LocalizationService);
  protected readonly locale = inject(LOCALE_ID);

  transform(
    value: string | Date | null | undefined,
    propType: 'date' | 'datetime' | 'time',
  ): string | Date {
    if (!value) return '';

    try {
      let format: string;
      switch (propType) {
        case 'date':
          format = getShortDateFormat(this.configState);
          break;
        case 'datetime':
          format = getShortDateShortTimeFormat(this.configState);
          break;
        case 'time':
          format = getShortTimeFormat(this.configState);
          break;
        default:
          format = getShortDateShortTimeFormat(this.configState);
      }

      if (this.timezoneService.isUtcClockEnabled) {
        const timeZone = this.timezoneService.timezone;
        const options: Intl.DateTimeFormatOptions = { timeZone };
        let localeStr: string;
        switch (propType) {
          case 'date':
            localeStr = new Date(value).toLocaleDateString(this.locale, options);
            break;
          case 'datetime':
            localeStr = new Date(value).toLocaleString(this.locale, options);
            break;
          case 'time':
            localeStr = new Date(value).toLocaleTimeString(this.locale, options);
            break;
          default:
            localeStr = new Date(value).toLocaleString(this.locale, options);
        }
        return formatDate(localeStr, format, this.locale);
      } else {
        return formatDate(value, format, this.locale, format);
      }
    } catch (err) {
      return value;
    }
  }
}
