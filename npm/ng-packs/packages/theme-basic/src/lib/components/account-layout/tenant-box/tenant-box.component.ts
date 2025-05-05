import { TenantBoxService } from '@abp/ng.account.core';
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreModule } from '@abp/ng.core';

@Component({
  selector: 'abp-tenant-box',
  templateUrl: './tenant-box.component.html',
  providers: [TenantBoxService],
  imports: [CommonModule, CoreModule],
})
export class TenantBoxComponent {
  constructor(public service: TenantBoxService) {}
}
