import { NgModule } from '@angular/core';
import { LocalizationPipe } from './pipes/localization.pipe';

@NgModule({
  exports: [LocalizationPipe],
  imports: [LocalizationPipe],
})
export class LocalizationModule {}
