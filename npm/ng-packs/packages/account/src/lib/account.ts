import { importProvidersFrom } from '@angular/core';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { NgxValidateCoreModule } from '@ngx-validate/core';
import { AccountConfigOptions } from './models/config-options';
import { ACCOUNT_CONFIG_OPTIONS } from './tokens/config-options.token';
import { accountConfigOptionsFactory } from './utils/factory-utils';
import { AuthenticationFlowGuard } from './guards/authentication-flow.guard';
import { RE_LOGIN_CONFIRMATION_TOKEN } from './tokens';
import { ACCOUNT_EDIT_FORM_PROP_CONTRIBUTORS } from './tokens/extensions.token';
import { AccountExtensionsGuard } from './guards/extensions.guard';

export function provideAccount(options: AccountConfigOptions = {}) {
  return [
    importProvidersFrom(NgbDropdownModule, NgxValidateCoreModule),
    AuthenticationFlowGuard,
    { provide: ACCOUNT_CONFIG_OPTIONS, useValue: options },
    {
      provide: 'ACCOUNT_OPTIONS',
      useFactory: accountConfigOptionsFactory,
      deps: [ACCOUNT_CONFIG_OPTIONS],
    },
    {
      provide: RE_LOGIN_CONFIRMATION_TOKEN,
      useValue: options.isPersonalSettingsChangedConfirmationActive ?? true,
    },
    {
      provide: ACCOUNT_EDIT_FORM_PROP_CONTRIBUTORS,
      useValue: options.editFormPropContributors,
    },
    AccountExtensionsGuard,
  ];
}
