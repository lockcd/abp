import { AuthWrapperService } from '@abp/ng.account.core';
import { Component } from '@angular/core';
import { LocalizationPipe, ReplaceableTemplateDirective } from '@abp/ng.core';
import { TenantBoxComponent } from '../tenant-box/tenant-box.component';

@Component({
  selector: 'abp-auth-wrapper',
  templateUrl: './auth-wrapper.component.html',
  providers: [AuthWrapperService],
  imports: [TenantBoxComponent, ReplaceableTemplateDirective, LocalizationPipe],
})
export class AuthWrapperComponent {
  constructor(public service: AuthWrapperService) {}
}
