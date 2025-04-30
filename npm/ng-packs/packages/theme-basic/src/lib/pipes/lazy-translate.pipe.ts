import { LocalizationService, ConfigStateService } from '@abp/ng.core';
import { inject, Pipe, PipeTransform } from '@angular/core';
import { Observable, filter, take, switchMap, shareReplay } from 'rxjs';

@Pipe({
  name: 'abpLazyTranslate',
})
export class LazyTranslatePipe implements PipeTransform {
  private localizationService = inject(LocalizationService);
  private configStateService = inject(ConfigStateService);

  transform(key: string): Observable<string> {
    return this.configStateService.getAll$().pipe(
      filter(config => !!config.localization),
      take(1),
      switchMap(() => this.localizationService.get(key)),
      shareReplay({ bufferSize: 1, refCount: true }),
    );
  }
}
