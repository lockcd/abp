import { Component, ChangeDetectionStrategy, forwardRef, input } from '@angular/core';
import { NG_VALUE_ACCESSOR, ControlValueAccessor, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { ABP, LocalizationModule } from '@abp/ng.core';
import { BaseThemeSharedModule } from '@abp/ng.theme.shared';
import { FormProp } from '@abp/ng.components/extensible';

const EXTENSIBLE_FORM_MULTI_SELECT_CONTROL_VALUE_ACCESSOR = {
  provide: NG_VALUE_ACCESSOR,
  useExisting: forwardRef(() => ExtensibleFormMultiselectComponent),
  multi: true,
};

@Component({
  selector: 'abp-extensible-form-multi-select',
  template: `
    <div [id]="prop().id">
      @for (option of options(); track option.value) {
        <div class="d-flex align-items-center column-gap-2">
          <div class="form-check" validationTarget>
            <input
              type="checkbox"
              class="form-check-input"
              [disabled]="disabled"
              [checked]="isChecked(option.value)"
              (change)="onCheckboxChange(option.value, $event)"
            />
            @if (prop().isExtra) {
              {{ '::' + option.key | abpLocalization }}
            } @else {
              {{ option.key }}
            }
          </div>
        </div>
      }
    </div>
  `,
  providers: [EXTENSIBLE_FORM_MULTI_SELECT_CONTROL_VALUE_ACCESSOR],
  imports: [BaseThemeSharedModule, LocalizationModule, CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExtensibleFormMultiselectComponent implements ControlValueAccessor {
  prop = input.required<FormProp>();
  options = input.required<ABP.Option<any>[]>();

  selectedValues: any[] = [];
  disabled = false;

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  isChecked(value: any): boolean {
    return this.selectedValues.includes(value);
  }

  onCheckboxChange(value: any, event: any): void {
    const checked = event.target.checked;

    if (checked) {
      this.selectedValues.push(value);
    } else {
      this.selectedValues = this.selectedValues.filter(item => item !== value);
    }

    this.onChange(this.selectedValues);
    this.onTouched();
  }

  writeValue(value: any[]): void {
    if (Array.isArray(value)) {
      this.selectedValues = value;
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  private onChange(fn: any): void {}

  private onTouched(): any {}
}
