import { Component, ViewEncapsulation } from '@angular/core';
import { PageAlertService } from '@abp/ng.theme.shared';
import { NgClass } from '@angular/common';
import { LocalizationPipe, SafeHtmlPipe } from '@abp/ng.core';

@Component({
  selector: 'abp-page-alert-container',
  templateUrl: './page-alert-container.component.html',
  encapsulation: ViewEncapsulation.None,
  imports: [NgClass, LocalizationPipe, SafeHtmlPipe],
})
export class PageAlertContainerComponent {
  constructor(public service: PageAlertService) {}
}
