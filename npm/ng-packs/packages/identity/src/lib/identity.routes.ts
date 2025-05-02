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
import { RolesComponent, UsersComponent } from './components';
import { identityExtensionsResolver } from './resolvers';
import { provideIdentity } from './identity';
import { eIdentityComponents } from './enums';

export const identityRoutes: Routes = [
  {
    path: '',
    component: RouterOutletStandaloneComponent,
    canActivate: [authGuard, permissionGuard],
    resolve: [identityExtensionsResolver],
    providers: [...provideIdentity()],
    children: [
      { path: '', redirectTo: 'roles', pathMatch: 'full' },
      {
        path: 'roles',
        component: ReplaceableRouteContainerStandaloneComponent,
        data: {
          requiredPolicy: 'AbpIdentity.Roles',
          replaceableComponent: {
            key: eIdentityComponents.Roles,
            defaultComponent: RolesComponent,
          } as ReplaceableComponents.RouteData<RolesComponent>,
        },
        title: 'AbpIdentity::Roles',
      },
      {
        path: 'users',
        component: ReplaceableRouteContainerStandaloneComponent,
        data: {
          requiredPolicy: 'AbpIdentity.Users',
          replaceableComponent: {
            key: eIdentityComponents.Users,
            defaultComponent: UsersComponent,
          } as ReplaceableComponents.RouteData<UsersComponent>,
        },
        title: 'AbpIdentity::Users',
      },
    ],
  },
];
