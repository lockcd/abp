import { NgModule } from '@angular/core';
import { LocalizationPipe } from './pipes/localization.pipe';

/**
 * @deprecated Use `LocalizationPipe` directly as a standalone pipe.
 * This module is no longer necessary for using the `LocalizationPipe`.
 */

@NgModule({
  exports: [LocalizationPipe],
  imports: [LocalizationPipe],
})
export class LocalizationModule {}
