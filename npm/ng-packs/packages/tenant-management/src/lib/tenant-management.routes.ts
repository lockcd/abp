import { Routes } from '@angular/router';

import {
  authGuard,
  permissionGuard,
  ReplaceableComponents,
  ReplaceableRouteContainerComponent,
  ReplaceableRouteContainerStandaloneComponent,
  RouterOutletComponent,
  RouterOutletStandaloneComponent,
} from '@abp/ng.core';

import { TenantsComponent } from './components/tenants/tenants.component';
import { eTenantManagementComponents } from './enums/components';
import { tenantManagementExtensionsResolver } from './resolvers';
import { provideTenantManagement } from './tenant-management';

export const tenantManagementRoutes: Routes = [
  {
    path: '',
    component: RouterOutletStandaloneComponent,
    canActivate: [authGuard, permissionGuard],
    resolve: [tenantManagementExtensionsResolver],
    providers: [...provideTenantManagement()],
    children: [
      { path: '', redirectTo: 'tenants', pathMatch: 'full' },
      {
        path: 'tenants',
        component: ReplaceableRouteContainerStandaloneComponent,
        data: {
          requiredPolicy: 'AbpTenantManagement.Tenants',
          replaceableComponent: {
            key: eTenantManagementComponents.Tenants,
            defaultComponent: TenantsComponent,
          } as ReplaceableComponents.RouteData<TenantsComponent>,
        },
        title: 'AbpTenantManagement::Tenants',
      },
    ],
  },
];
