import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ContractService, ContractResponse } from '../../core/services/contract.service';
import { ClientService, ClientResponse } from '../../core/services/client.service';
import { ServiceApiService, ServiceResponse } from '../../core/services/service-api.service';
import { CatalogService, CatalogItem } from '../../core/services/catalog.service';

@Component({
  selector: 'app-contrato',
  templateUrl: './contrato.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContratoComponent implements OnInit {
  private contractService = inject(ContractService);
  private clientService = inject(ClientService);
  private serviceApi = inject(ServiceApiService);
  private catalogService = inject(CatalogService);
  private messageService = inject(MessageService);
  private confirmService = inject(ConfirmationService);

  contracts = signal<ContractResponse[]>([]);
  clients = signal<ClientResponse[]>([]);
  services = signal<ServiceResponse[]>([]);
  methodPayments = signal<CatalogItem[]>([]);
  loading = signal(false);
  showCreate = signal(false);
  showChangeService = signal(false);
  showChangePayment = signal(false);
  selectedContract = signal<ContractResponse | null>(null);
  saving = signal(false);

  createForm = new FormGroup({
    clientId: new FormControl<number | null>(null, { validators: [Validators.required] }),
    serviceId: new FormControl<number | null>(null, { validators: [Validators.required] }),
    methodPaymentId: new FormControl<number | null>(null, { validators: [Validators.required] }),
    startDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    endDate: new FormControl<Date | null>(null, { validators: [Validators.required] })
  });

  changeServiceForm = new FormGroup({
    newServiceId: new FormControl<number | null>(null, { validators: [Validators.required] })
  });

  changePaymentForm = new FormGroup({
    methodPaymentId: new FormControl<number | null>(null, { validators: [Validators.required] })
  });

  ngOnInit(): void {
    this.loadContracts();
    this.clientService.getAll().subscribe({ next: (d) => this.clients.set(d) });
    this.serviceApi.getAll().subscribe({ next: (d) => this.services.set(d) });
    this.catalogService.getMethodPayments().subscribe({ next: (d) => this.methodPayments.set(d) });
  }

  loadContracts(): void {
    this.loading.set(true);
    this.contractService.getAll().subscribe({
      next: (d) => { this.contracts.set(d); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  create(): void {
    if (this.createForm.invalid) return;
    this.saving.set(true);
    const v = this.createForm.getRawValue();
    this.contractService.create({
      clientId: v.clientId!,
      serviceId: v.serviceId!,
      methodPaymentId: v.methodPaymentId!,
      startDate: v.startDate!.toISOString(),
      endDate: v.endDate!.toISOString()
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.showCreate.set(false);
        this.createForm.reset();
        this.messageService.add({ severity: 'success', summary: 'Contrato creado', detail: 'Contrato registrado correctamente.' });
        this.loadContracts();
      },
      error: (err) => { this.saving.set(false); this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error al crear.' }); }
    });
  }

  openChangeService(c: ContractResponse): void {
    this.selectedContract.set(c);
    this.changeServiceForm.reset();
    this.showChangeService.set(true);
  }

  submitChangeService(): void {
    if (this.changeServiceForm.invalid) return;
    this.saving.set(true);
    this.contractService.changeService(this.selectedContract()!.contractId, this.changeServiceForm.getRawValue().newServiceId!).subscribe({
      next: () => {
        this.saving.set(false);
        this.showChangeService.set(false);
        this.messageService.add({ severity: 'success', summary: 'Servicio cambiado', detail: 'Contrato renovado.' });
        this.loadContracts();
      },
      error: (err) => { this.saving.set(false); this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error.' }); }
    });
  }

  openChangePayment(c: ContractResponse): void {
    this.selectedContract.set(c);
    this.changePaymentForm.reset();
    this.showChangePayment.set(true);
  }

  submitChangePayment(): void {
    if (this.changePaymentForm.invalid) return;
    this.saving.set(true);
    this.contractService.changePaymentMethod(this.selectedContract()!.contractId, this.changePaymentForm.getRawValue().methodPaymentId!).subscribe({
      next: () => {
        this.saving.set(false);
        this.showChangePayment.set(false);
        this.messageService.add({ severity: 'success', summary: 'Actualizado', detail: 'Método de pago cambiado.' });
        this.loadContracts();
      },
      error: (err) => { this.saving.set(false); this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error.' }); }
    });
  }

  cancelContract(c: ContractResponse): void {
    this.confirmService.confirm({
      message: `¿Cancelar el contrato #${c.contractId} de ${c.clientName}?`,
      header: 'Confirmar cancelación',
      icon: 'pi pi-times-circle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.contractService.cancel(c.contractId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Cancelado', detail: 'Contrato cancelado.' });
            this.loadContracts();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error.' })
        });
      }
    });
  }

  getStatusSeverity(code: string): 'success' | 'warn' | 'danger' | 'info' | 'secondary' {
    switch (code) {
      case 'VIG': return 'success';
      case 'REN': return 'info';
      case 'SUS': return 'warn';
      case 'CAN': return 'danger';
      default: return 'secondary';
    }
  }
}
