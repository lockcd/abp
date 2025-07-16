import { TenantBoxService } from '@abp/ng.account.core';
import { Component } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { ButtonComponent, ModalCloseDirective, ModalComponent } from '@abp/ng.theme.shared';
import { FormsModule } from '@angular/forms';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'abp-tenant-box',
  templateUrl: './tenant-box.component.html',
  providers: [TenantBoxService],
  imports: [
    FormsModule,
    AsyncPipe,
    ModalComponent,
    LocalizationPipe,
    ButtonComponent,
    ModalCloseDirective,
  ],
})
export class TenantBoxComponent {
  constructor(public service: TenantBoxService) {}
}
