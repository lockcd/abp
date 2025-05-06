import { NgModule } from '@angular/core';
import { LocalizationPipe } from './pipes/localization.pipe';
import { LazyLocalizationPipe } from './pipes';

/**
 * @deprecated Use `LocalizationPipe` directly as a standalone pipe.
 * This module is no longer necessary for using the `LocalizationPipe`.
 */

@NgModule({
  exports: [LocalizationPipe, LazyLocalizationPipe],
  imports: [LocalizationPipe, LazyLocalizationPipe],
})
export class LocalizationModule {}
