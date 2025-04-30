import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  standalone: false,
  selector: 'abp-router-outlet',
  template: ` <router-outlet></router-outlet> `,
})
export class RouterOutletComponent {}

@Component({
  selector: 'abp-router-outlet',
  template: ` <router-outlet></router-outlet> `,
  imports: [RouterModule],
})
export class RouterOutletStandaloneComponent {}
