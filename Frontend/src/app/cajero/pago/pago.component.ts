import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { PaymentService, PaymentResponse } from '../../core/services/payment.service';
import { ContractService, ContractResponse } from '../../core/services/contract.service';

@Component({
  selector: 'app-pago',
  templateUrl: './pago.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PagoComponent implements OnInit {
  private paymentService = inject(PaymentService);
  private contractService = inject(ContractService);
  private messageService = inject(MessageService);

  payments = signal<PaymentResponse[]>([]);
  contracts = signal<ContractResponse[]>([]);
  loading = signal(false);
  saving = signal(false);
  showDialog = signal(false);

  form = new FormGroup({
    contractId: new FormControl<number | null>(null, { validators: [Validators.required] }),
    amount: new FormControl<number | null>(null, { validators: [Validators.required, Validators.min(0.01)] }),
    attentionId: new FormControl<number>(0, { nonNullable: true })
  });

  ngOnInit(): void {
    this.loadPayments();
    this.contractService.getAll(undefined, 'VIG').subscribe({ next: (d) => this.contracts.set(d) });
  }

  loadPayments(): void {
    this.loading.set(true);
    this.paymentService.getAll().subscribe({
      next: (d) => { this.payments.set(d); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const v = this.form.getRawValue();
    this.paymentService.create({ contractId: v.contractId!, amount: v.amount!, attentionId: v.attentionId }).subscribe({
      next: () => {
        this.saving.set(false);
        this.showDialog.set(false);
        this.form.reset();
        this.messageService.add({ severity: 'success', summary: 'Pago registrado', detail: 'El pago fue registrado correctamente.' });
        this.loadPayments();
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error al registrar pago.' });
      }
    });
  }
}
