import { LazyModuleFactory } from '@abp/ng.core';
import { ModuleWithProviders, NgModule, NgModuleFactory } from '@angular/core';
import { SettingManagementRoutingModule } from './setting-management-routing.module';

@NgModule({
  declarations: [],
  exports: [],
  imports: [SettingManagementRoutingModule],
})
export class SettingManagementModule {
  static forChild(): ModuleWithProviders<SettingManagementModule> {
    return {
      ngModule: SettingManagementModule,
      providers: [],
    };
  }

  static forLazy(): NgModuleFactory<SettingManagementModule> {
    return new LazyModuleFactory(SettingManagementModule.forChild());
  }
}
