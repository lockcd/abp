import { Routes } from '@angular/router';
import {
  authGuard,
  ReplaceableComponents,
  ReplaceableRouteContainerComponent,
  RouterOutletComponent,
} from '@abp/ng.core';
import { SettingManagementComponent } from './components/setting-management.component';
import { eSettingManagementComponents } from './enums/components';
import { provideSettingManagement } from './setting-management';

export const settingManagementRoutes: Routes = [
  {
    path: '',
    component: RouterOutletComponent,
    canActivate: [authGuard],
    providers: [...provideSettingManagement()],
    children: [
      {
        path: '',
        component: ReplaceableRouteContainerComponent,
        data: {
          requiredPolicy: 'AbpAccount.SettingManagement',
          replaceableComponent: {
            key: eSettingManagementComponents.SettingManagement,
            defaultComponent: SettingManagementComponent,
          } as ReplaceableComponents.RouteData,
        },
      },
    ],
    title: 'AbpSettingManagement::Settings',
  },
];
