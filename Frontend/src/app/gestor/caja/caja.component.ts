import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { CashService, CashResponse } from '../../core/services/cash.service';
import { UserService, UserResponse } from '../../core/services/user.service';

@Component({
  selector: 'app-caja',
  templateUrl: './caja.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CajaComponent implements OnInit {
  private cashService = inject(CashService);
  private userService = inject(UserService);
  private messageService = inject(MessageService);
  private confirmService = inject(ConfirmationService);

  cashes = signal<CashResponse[]>([]);
  cajeros = signal<UserResponse[]>([]);
  loading = signal(false);
  saving = signal(false);
  selectedCash = signal<CashResponse | null>(null);
  showAssignDialog = signal(false);

  assignForm = new FormGroup({
    userId: new FormControl<number | null>(null, { validators: [Validators.required] })
  });

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.cashService.getAll().subscribe({
      next: (cashes) => { this.cashes.set(cashes); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.userService.getAll(3, 'ACT').subscribe({
      next: (users) => this.cajeros.set(users)
    });
  }

  openAssign(cash: CashResponse): void {
    this.selectedCash.set(cash);
    this.assignForm.reset();
    this.showAssignDialog.set(true);
  }

  assign(): void {
    if (this.assignForm.invalid) return;
    const cash = this.selectedCash();
    if (!cash) return;
    this.saving.set(true);
    this.cashService.assignUser(cash.cashId, this.assignForm.getRawValue().userId!).subscribe({
      next: (updated) => {
        this.saving.set(false);
        this.showAssignDialog.set(false);
        this.cashes.update(list => list.map(c => c.cashId === updated.cashId ? updated : c));
        this.messageService.add({ severity: 'success', summary: 'Asignado', detail: 'Cajero asignado a la caja.' });
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'No se pudo asignar.' });
      }
    });
  }

  removeUser(cash: CashResponse, userId: number, username: string): void {
    this.confirmService.confirm({
      message: `¿Quitar a ${username} de ${cash.cashDescription}?`,
      header: 'Confirmar',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.cashService.removeUser(cash.cashId, userId).subscribe({
          next: () => {
            this.loadData();
            this.messageService.add({ severity: 'success', summary: 'Removido', detail: `${username} removido de la caja.` });
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error.' })
        });
      }
    });
  }

  toggleActive(cash: CashResponse, userId: number, isActive: boolean): void {
    const action$ = isActive
      ? this.cashService.deactivateUser(cash.cashId, userId)
      : this.cashService.activateUser(cash.cashId, userId);

    action$.subscribe({
      next: (updated) => {
        this.cashes.update(list => list.map(c => c.cashId === updated.cashId ? updated : c));
        this.messageService.add({ severity: 'success', summary: 'Actualizado', detail: 'Estado del cajero actualizado.' });
      },
      error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error.' })
    });
  }

  getAvailableCajeros(cash: CashResponse | null): UserResponse[] {
    if (!cash) return [];
    const assignedIds = cash.assignedUsers.map(u => u.userId);
    return this.cajeros().filter(c => !assignedIds.includes(c.userId));
  }
}
