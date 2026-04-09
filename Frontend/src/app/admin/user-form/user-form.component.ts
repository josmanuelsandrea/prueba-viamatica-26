import { Component, Input, Output, EventEmitter, OnChanges, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { UserService, UserResponse } from '../../core/services/user.service';
import { CatalogService, RolItem } from '../../core/services/catalog.service';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserFormComponent implements OnChanges {
  @Input() visible = false;
  @Input() user: UserResponse | null = null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  private userService = inject(UserService);
  private catalogService = inject(CatalogService);
  private messageService = inject(MessageService);

  loading = signal(false);
  roles = signal<RolItem[]>([]);

  form = new FormGroup({
    username: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.minLength(8), Validators.maxLength(20), Validators.pattern(/^(?=.*[a-zA-Z])(?=.*\d)[a-zA-Z\d]{8,20}$/)] }),
    password: new FormControl<string>('', { nonNullable: true }),
    email: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.email] }),
    rolId: new FormControl<number | null>(null, { validators: [Validators.required] })
  });

  ngOnChanges(): void {
    if (this.visible) {
      this.loadRoles();
      if (this.mode === 'create') {
        this.form.reset();
        this.form.controls.password.setValidators([Validators.required, Validators.minLength(8), Validators.maxLength(30), Validators.pattern(/^(?=.*[A-Z])(?=.*\d).{8,30}$/)]);
      } else if (this.user) {
        this.form.controls.password.clearValidators();
        this.form.patchValue({ username: this.user.username, email: this.user.email, rolId: this.user.rolId });
      }
      this.form.controls.password.updateValueAndValidity();
    }
  }

  loadRoles(): void {
    this.catalogService.getRoles().subscribe({ next: (r) => this.roles.set(r) });
  }

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    const val = this.form.getRawValue();

    const obs = this.mode === 'create'
      ? this.userService.create({ username: val.username, password: val.password, email: val.email, rolId: val.rolId! })
      : this.userService.update(this.user!.userId, { username: val.username, email: val.email });

    obs.subscribe({
      next: () => {
        this.loading.set(false);
        this.messageService.add({ severity: 'success', summary: 'Guardado', detail: this.mode === 'create' ? 'Usuario creado.' : 'Usuario actualizado.' });
        this.saved.emit();
      },
      error: (err) => {
        this.loading.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error al guardar.' });
      }
    });
  }

  cancel(): void {
    this.cancelled.emit();
  }
}
