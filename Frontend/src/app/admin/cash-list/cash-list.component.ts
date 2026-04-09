import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { CashService, CashResponse } from '../../core/services/cash.service';

@Component({
  selector: 'app-cash-list',
  templateUrl: './cash-list.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CashListComponent implements OnInit {
  private cashService = inject(CashService);
  private messageService = inject(MessageService);
  private confirmService = inject(ConfirmationService);
  private fb = inject(FormBuilder);

  cajas = signal<CashResponse[]>([]);
  loading = signal(false);
  saving = signal(false);
  showDialog = signal(false);
  editingId = signal<number | null>(null);

  form = this.fb.group({
    cashDescription: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]]
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.cashService.getAll().subscribe({
      next: (data) => { this.cajas.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.reset();
    this.showDialog.set(true);
  }

  openEdit(caja: CashResponse): void {
    this.editingId.set(caja.cashId);
    this.form.setValue({ cashDescription: caja.cashDescription });
    this.showDialog.set(true);
  }

  submit(): void {
    if (this.form.invalid) return;
    const dto = { cashDescription: this.form.value.cashDescription! };
    this.saving.set(true);

    const req$ = this.editingId() != null
      ? this.cashService.update(this.editingId()!, dto)
      : this.cashService.create(dto);

    req$.subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Guardado', detail: 'Caja guardada correctamente.' });
        this.showDialog.set(false);
        this.saving.set(false);
        this.load();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'No se pudo guardar.' });
        this.saving.set(false);
      }
    });
  }

  confirmDelete(caja: CashResponse): void {
    this.confirmService.confirm({
      message: `¿Eliminar la caja "${caja.cashDescription}"?`,
      header: 'Confirmar eliminación',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.cashService.delete(caja.cashId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Eliminado', detail: 'Caja eliminada.' });
            this.load();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'No se pudo eliminar.' })
        });
      }
    });
  }
}
