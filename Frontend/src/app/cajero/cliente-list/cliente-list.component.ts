import { Component, ChangeDetectionStrategy, signal, inject, computed } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ClientService, ClientResponse } from '../../core/services/client.service';
import { ContractService, ContractResponse } from '../../core/services/contract.service';
import * as XLSX from 'xlsx';

type View = 'lookup' | 'client' | 'new-client';

@Component({
  selector: 'app-cliente-list',
  templateUrl: './cliente-list.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClienteListComponent {
  private clientService = inject(ClientService);
  private contractService = inject(ContractService);
  private messageService = inject(MessageService);
  private confirmService = inject(ConfirmationService);

  view = signal<View>('lookup');
  searchInput = signal('');
  searching = signal(false);
  saving = signal(false);
  currentClient = signal<ClientResponse | null>(null);
  contracts = signal<ContractResponse[]>([]);
  showEditDialog = signal(false);

  readonly numpadKeys = ['1','2','3','4','5','6','7','8','9','C','0','←'];

  readonly clientInitials = computed(() => {
    const c = this.currentClient();
    return c ? (c.name.charAt(0) + c.lastname.charAt(0)).toUpperCase() : '';
  });

  readonly activeContract = computed(() =>
    this.contracts().find(c => c.statusCode === 'VIG') ?? null
  );

  newClientForm = new FormGroup({
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    lastname: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    phone: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/^09\d{8,}$/)] }),
    address: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.minLength(20), Validators.maxLength(100)] }),
    referenceAddress: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.minLength(20), Validators.maxLength(100)] }),
    email: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.email] })
  });

  editForm = new FormGroup({
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    lastname: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    phone: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/^09\d{8,}$/)] }),
    address: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.minLength(20), Validators.maxLength(100)] }),
    referenceAddress: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.minLength(20), Validators.maxLength(100)] }),
    email: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.email] })
  });

  pressKey(key: string): void {
    if (key === 'C') {
      this.searchInput.set('');
    } else if (key === '←') {
      this.searchInput.update(v => v.slice(0, -1));
    } else if (this.searchInput().length < 13) {
      this.searchInput.update(v => v + key);
    }
  }

  search(): void {
    const id = this.searchInput();
    if (id.length < 10) {
      this.messageService.add({ severity: 'warn', summary: 'Aviso', detail: 'Ingrese al menos 10 dígitos.' });
      return;
    }
    this.searching.set(true);
    this.clientService.getByIdentification(id).subscribe({
      next: (client) => {
        this.searching.set(false);
        this.currentClient.set(client);
        this.loadContracts(client.clientId);
        this.view.set('client');
      },
      error: (err) => {
        this.searching.set(false);
        if (err.status === 404) {
          this.messageService.add({ severity: 'info', summary: 'No encontrado', detail: 'No existe cliente con esa identificación. Puede registrarlo.' });
          this.newClientForm.reset();
          this.view.set('new-client');
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al buscar cliente.' });
        }
      }
    });
  }

  loadContracts(clientId: number): void {
    this.contractService.getAll(clientId).subscribe({ next: (d) => this.contracts.set(d) });
  }

  back(): void {
    this.searchInput.set('');
    this.currentClient.set(null);
    this.contracts.set([]);
    this.view.set('lookup');
  }

  saveNewClient(): void {
    if (this.newClientForm.invalid) return;
    this.saving.set(true);
    const val = this.newClientForm.getRawValue();
    this.clientService.create({ ...val, identification: this.searchInput() }).subscribe({
      next: (client) => {
        this.saving.set(false);
        this.messageService.add({ severity: 'success', summary: 'Registrado', detail: `${client.name} ${client.lastname} registrado correctamente.` });
        this.currentClient.set(client);
        this.contracts.set([]);
        this.view.set('client');
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error al registrar.' });
      }
    });
  }

  openEdit(): void {
    const c = this.currentClient();
    if (!c) return;
    this.editForm.patchValue({ name: c.name, lastname: c.lastname, phone: c.phone, address: c.address, referenceAddress: c.referenceAddress, email: c.email });
    this.showEditDialog.set(true);
  }

  saveEdit(): void {
    if (this.editForm.invalid) return;
    this.saving.set(true);
    this.clientService.update(this.currentClient()!.clientId, this.editForm.getRawValue()).subscribe({
      next: (updated) => {
        this.saving.set(false);
        this.showEditDialog.set(false);
        this.currentClient.set(updated);
        this.messageService.add({ severity: 'success', summary: 'Actualizado', detail: 'Datos actualizados correctamente.' });
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error al actualizar.' });
      }
    });
  }

  toggleStatus(): void {
    const c = this.currentClient();
    if (!c) return;
    const action = c.active ? 'desactivar' : 'activar';
    this.confirmService.confirm({
      message: `¿Desea ${action} a ${c.name} ${c.lastname}?`,
      header: 'Confirmar',
      accept: () => {
        this.clientService.changeStatus(c.clientId, !c.active).subscribe({
          next: (updated) => {
            this.currentClient.set(updated);
            this.messageService.add({ severity: 'success', summary: 'Actualizado', detail: 'Estado cambiado.' });
          }
        });
      }
    });
  }

  getStatusSeverity(code: string): 'success' | 'warn' | 'danger' | 'info' | 'secondary' {
    const map: Record<string, any> = { VIG: 'success', REN: 'info', SUS: 'warn', CAN: 'danger' };
    return map[code] ?? 'secondary';
  }

  onClientFileUpload(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    input.value = '';

    const reader = new FileReader();
    reader.onload = (e) => {
      const data = new Uint8Array(e.target!.result as ArrayBuffer);
      const workbook = XLSX.read(data, { type: 'array' });
      const sheet = workbook.Sheets[workbook.SheetNames[0]];
      const rows: Record<string, string>[] = XLSX.utils.sheet_to_json(sheet, { defval: '' });

      if (rows.length === 0) {
        this.messageService.add({ severity: 'warn', summary: 'Archivo vacío', detail: 'El archivo no contiene filas de datos.' });
        return;
      }

      let successCount = 0;
      let errorCount = 0;
      let pending = rows.length;

      const finish = () => {
        this.messageService.add({
          severity: successCount > 0 ? 'success' : 'error',
          summary: 'Carga masiva finalizada',
          detail: `${successCount} creados, ${errorCount} errores`,
          life: 6000
        });
      };

      for (const row of rows) {
        const payload = {
          name:             (row['nombre']        ?? row['name']            ?? '').toString().trim(),
          lastname:         (row['apellido']       ?? row['lastname']        ?? '').toString().trim(),
          identification:   (row['identificacion'] ?? row['identification']  ?? '').toString().trim(),
          phone:            (row['telefono']       ?? row['phone']           ?? '').toString().trim(),
          email:            (row['correo']         ?? row['email']           ?? '').toString().trim(),
          address:          (row['direccion']      ?? row['address']         ?? '').toString().trim(),
          referenceAddress: (row['referencia']     ?? row['referenceAddress'] ?? '').toString().trim()
        };

        this.clientService.create(payload).subscribe({
          next: () => {
            successCount++;
            pending--;
            if (pending === 0) finish();
          },
          error: () => {
            errorCount++;
            pending--;
            if (pending === 0) finish();
          }
        });
      }
    };

    reader.readAsArrayBuffer(file);
  }
}
