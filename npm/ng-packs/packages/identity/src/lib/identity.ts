import {
  IDENTITY_CREATE_FORM_PROP_CONTRIBUTORS,
  IDENTITY_EDIT_FORM_PROP_CONTRIBUTORS,
  IDENTITY_ENTITY_ACTION_CONTRIBUTORS,
  IDENTITY_ENTITY_PROP_CONTRIBUTORS,
  IDENTITY_TOOLBAR_ACTION_CONTRIBUTORS,
} from './tokens';
import { IdentityConfigOptions } from './models';

export function provideIdentity(options: IdentityConfigOptions = {}) {
  return [
    {
      provide: IDENTITY_ENTITY_ACTION_CONTRIBUTORS,
      useValue: options.entityActionContributors,
    },
    {
      provide: IDENTITY_TOOLBAR_ACTION_CONTRIBUTORS,
      useValue: options.toolbarActionContributors,
    },
    {
      provide: IDENTITY_ENTITY_PROP_CONTRIBUTORS,
      useValue: options.entityPropContributors,
    },
    {
      provide: IDENTITY_CREATE_FORM_PROP_CONTRIBUTORS,
      useValue: options.createFormPropContributors,
    },
    {
      provide: IDENTITY_EDIT_FORM_PROP_CONTRIBUTORS,
      useValue: options.editFormPropContributors,
    },
  ];
}
