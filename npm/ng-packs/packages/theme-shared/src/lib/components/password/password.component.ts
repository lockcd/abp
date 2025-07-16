import { Component, forwardRef, Input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { NgClass } from '@angular/common';
import { AbstractNgModelComponent } from '@abp/ng.core';
import { NgxValidateCoreModule } from '@ngx-validate/core';

/**
 * @deprecated use ShowPasswordDirective directive
 * https://abp.io/docs/latest/framework/ui/angular/show-password-directive
 */
@Component({
  selector: 'abp-password',
  imports: [NgClass, FormsModule, NgxValidateCoreModule],
  templateUrl: `./password.component.html`,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PasswordComponent),
      multi: true,
    },
  ],
})
export class PasswordComponent extends AbstractNgModelComponent {
  @Input() inputId!: string;
  @Input() formControlName!: string;
  fieldTextType?: boolean;

  toggleFieldTextType() {
    this.fieldTextType = !this.fieldTextType;
  }
}
