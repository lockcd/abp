import { Component, Input } from '@angular/core';
import { NgClass, NgTemplateOutlet } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ABP, LocalizationPipe } from '@abp/ng.core';

@Component({
  selector: 'abp-breadcrumb-items',
  templateUrl: './breadcrumb-items.component.html',
  imports: [NgClass, NgTemplateOutlet, RouterLink, LocalizationPipe],
})
export class BreadcrumbItemsComponent {
  @Input() items: Partial<ABP.Route>[] = [];
}
