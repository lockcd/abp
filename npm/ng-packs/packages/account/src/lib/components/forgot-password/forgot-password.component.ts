import { AccountService } from '@abp/ng.account.core/proxy';
import { Component } from '@angular/core';
import {
  ReactiveFormsModule,
  UntypedFormBuilder,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { CoreModule } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'abp-forgot-password',
  templateUrl: 'forgot-password.component.html',
  imports: [CommonModule, ReactiveFormsModule, CoreModule, ThemeSharedModule],
})
export class ForgotPasswordComponent {
  form: UntypedFormGroup;

  inProgress?: boolean;

  isEmailSent = false;

  constructor(
    private fb: UntypedFormBuilder,
    private accountService: AccountService,
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit() {
    if (this.form.invalid) return;

    this.inProgress = true;

    this.accountService
      .sendPasswordResetCode({
        email: this.form.get('email')?.value,
        appName: 'Angular',
      })
      .pipe(finalize(() => (this.inProgress = false)))
      .subscribe(() => {
        this.isEmailSent = true;
      });
  }
}
