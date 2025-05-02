import { CoreModule } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { NgModule } from '@angular/core';
import { PermissionManagementComponent } from './components';

@NgModule({
  declarations: [],
  imports: [CoreModule, ThemeSharedModule, PermissionManagementComponent],
  exports: [PermissionManagementComponent],
})
export class PermissionManagementModule {}
