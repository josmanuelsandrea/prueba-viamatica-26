import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { TurnService, TurnResponse } from '../../core/services/turn.service';
import { CashService, CashResponse } from '../../core/services/cash.service';
import { CatalogService, AttentionTypeItem } from '../../core/services/catalog.service';

@Component({
  selector: 'app-turno',
  templateUrl: './turno.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TurnoComponent implements OnInit {
  private turnService = inject(TurnService);
  private cashService = inject(CashService);
  private catalogService = inject(CatalogService);
  private messageService = inject(MessageService);

  turns = signal<TurnResponse[]>([]);
  cashes = signal<CashResponse[]>([]);
  attentionTypes = signal<AttentionTypeItem[]>([]);
  loading = signal(false);
  creating = signal(false);

  form = new FormGroup({
    cashId: new FormControl<number | null>(null, { validators: [Validators.required] }),
    attentionTypeId: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] })
  });

  ngOnInit(): void {
    this.loadCatalogs();
    this.loadTurns();
  }

  loadCatalogs(): void {
    this.cashService.getAll().subscribe({ next: (d) => this.cashes.set(d) });
    this.catalogService.getAttentionTypes().subscribe({ next: (d) => this.attentionTypes.set(d) });
  }

  loadTurns(): void {
    this.loading.set(true);
    this.turnService.getAll().subscribe({
      next: (d) => { this.turns.set(d); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  create(): void {
    if (this.form.invalid) return;
    this.creating.set(true);
    const val = this.form.getRawValue();
    this.turnService.create({ cashId: val.cashId!, attentionTypeId: val.attentionTypeId }).subscribe({
      next: (turn) => {
        this.creating.set(false);
        this.messageService.add({ severity: 'success', summary: 'Turno creado', detail: `Turno ${turn.description} asignado en ${turn.cashDescription}.` });
        this.form.reset();
        this.loadTurns();
      },
      error: (err) => {
        this.creating.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'No se pudo crear el turno.' });
      }
    });
  }
}
