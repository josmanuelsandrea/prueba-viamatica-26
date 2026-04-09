import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-recover-password',
  templateUrl: './recover-password.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RecoverPasswordComponent {
  loading = signal(false);
  sent = signal(false);

  form = new FormGroup({
    email: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.email] })
  });

  constructor(private authService: AuthService, private messageService: MessageService) {}

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);

    this.authService.recoverPassword(this.form.getRawValue().email).subscribe({
      next: () => {
        this.loading.set(false);
        this.sent.set(true);
      },
      error: () => {
        this.loading.set(false);
        this.sent.set(true);
      }
    });
  }
}
