import { Routes } from '@angular/router';

export const appRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./home/home.component').then(m => m.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(m => m.createAccountRoutingConfiguration()),
  },
  {
    path: 'identity',
    loadChildren: () =>
      import('@abp/ng.identity').then(m => m.createIdentityRoutingConfiguration()),
  },
  {
    path: 'tenant-management',
    loadChildren: () =>
      import('@abp/ng.tenant-management').then(m => m.createTenantManagementRoutingConfiguration()),
  },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m =>
        m.createSettingManagementRoutingConfiguration(),
      ),
  },
];
