import { TenantBoxService } from '@abp/ng.account.core';
import { Component } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { ButtonComponent, ModalCloseDirective, ModalComponent } from '@abp/ng.theme.shared';
import { NgModel } from '@angular/forms';

@Component({
  selector: 'abp-tenant-box',
  templateUrl: './tenant-box.component.html',
  providers: [TenantBoxService],
  imports: [
    NgModel,
    ModalComponent,
    LocalizationPipe,
    ButtonComponent,
    ModalCloseDirective,
  ],
})
export class TenantBoxComponent {
  constructor(public service: TenantBoxService) {}
}
