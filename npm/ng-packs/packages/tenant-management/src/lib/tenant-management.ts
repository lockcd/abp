import { TenantManagementConfigOptions } from './models/config-options';
import {
  TENANT_MANAGEMENT_CREATE_FORM_PROP_CONTRIBUTORS,
  TENANT_MANAGEMENT_EDIT_FORM_PROP_CONTRIBUTORS,
  TENANT_MANAGEMENT_ENTITY_ACTION_CONTRIBUTORS,
  TENANT_MANAGEMENT_ENTITY_PROP_CONTRIBUTORS,
  TENANT_MANAGEMENT_TOOLBAR_ACTION_CONTRIBUTORS,
} from './tokens/extensions.token';

export function provideTenantManagement(options: TenantManagementConfigOptions = {}) {
  return [
    {
      provide: TENANT_MANAGEMENT_ENTITY_ACTION_CONTRIBUTORS,
      useValue: options.entityActionContributors,
    },
    {
      provide: TENANT_MANAGEMENT_TOOLBAR_ACTION_CONTRIBUTORS,
      useValue: options.toolbarActionContributors,
    },
    {
      provide: TENANT_MANAGEMENT_ENTITY_PROP_CONTRIBUTORS,
      useValue: options.entityPropContributors,
    },
    {
      provide: TENANT_MANAGEMENT_CREATE_FORM_PROP_CONTRIBUTORS,
      useValue: options.createFormPropContributors,
    },
    {
      provide: TENANT_MANAGEMENT_EDIT_FORM_PROP_CONTRIBUTORS,
      useValue: options.editFormPropContributors,
    },
  ];
}
