import { Component, ViewEncapsulation } from '@angular/core';
import { PageAlertService } from '@abp/ng.theme.shared';
import { CommonModule } from '@angular/common';
import { CoreModule } from '@abp/ng.core';

@Component({
  selector: 'abp-page-alert-container',
  templateUrl: './page-alert-container.component.html',
  encapsulation: ViewEncapsulation.None,
  imports: [CommonModule, CoreModule],
})
export class PageAlertContainerComponent {
  constructor(public service: PageAlertService) {}
}
