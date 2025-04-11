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
    type: 'date' | 'datetime' | 'time',
  ): string | Date {
    if (!value) return '';

    const date = new Date(value);
    if (isNaN(date.getTime())) return '';

    const format = this.getFormat(type);

    try {
      if (this.timezoneService.isUtcClockEnabled) {
        const timeZone = this.timezoneService.timezone;
        const options: Intl.DateTimeFormatOptions = { timeZone };
        const localeStr = this.formatWithIntl(date, type, options);
        return formatDate(localeStr, format, this.locale);
      } else {
        return formatDate(value, format, this.locale, format);
      }
    } catch (err) {
      return value;
    }
  }

  private getFormat(propType: 'date' | 'datetime' | 'time'): string {
    switch (propType) {
      case 'date':
        return getShortDateFormat(this.configState);
      case 'time':
        return getShortTimeFormat(this.configState);
      case 'datetime':
      default:
        return getShortDateShortTimeFormat(this.configState);
    }
  }

  private formatWithIntl(
    date: Date,
    propType: 'date' | 'datetime' | 'time',
    options: Intl.DateTimeFormatOptions,
  ): string {
    switch (propType) {
      case 'date':
        return date.toLocaleDateString(this.locale, options);
      case 'time':
        return date.toLocaleTimeString(this.locale, options);
      case 'datetime':
      default:
        return date.toLocaleString(this.locale, options);
    }
  }
}
