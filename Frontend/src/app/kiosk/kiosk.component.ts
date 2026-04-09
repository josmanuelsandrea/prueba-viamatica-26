import { Component, ChangeDetectionStrategy, signal, inject, computed } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { KioskService, KioskClientResponse, KioskTurnResponse } from '../core/services/kiosk.service';
import { CatalogService, AttentionTypeItem } from '../core/services/catalog.service';

type Step = 'identify' | 'register' | 'found' | 'select-service' | 'turn-issued';

@Component({
  selector: 'app-kiosk',
  templateUrl: './kiosk.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KioskComponent {
  private kioskService = inject(KioskService);
  private catalogService = inject(CatalogService);
  private messageService = inject(MessageService);

  step = signal<Step>('identify');
  idInput = signal('');
  searching = signal(false);
  saving = signal(false);
  requesting = signal(false);

  currentClient = signal<KioskClientResponse | null>(null);
  attentionTypes = signal<AttentionTypeItem[]>([]);
  issuedTurn = signal<KioskTurnResponse | null>(null);

  readonly numpadKeys = ['1','2','3','4','5','6','7','8','9','C','0','←'];

  readonly clientInitials = computed(() => {
    const c = this.currentClient();
    return c ? (c.name.charAt(0) + c.lastname.charAt(0)).toUpperCase() : '';
  });

  registerForm = new FormGroup({
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    lastname: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    phone: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.pattern(/^09\d{8,}$/)] }),
    address: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.minLength(20), Validators.maxLength(100)] }),
    referenceAddress: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.minLength(20), Validators.maxLength(100)] }),
    email: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.email] })
  });

  pressKey(key: string): void {
    if (key === 'C') { this.idInput.set(''); }
    else if (key === '←') { this.idInput.update(v => v.slice(0, -1)); }
    else if (this.idInput().length < 13) { this.idInput.update(v => v + key); }
  }

  checkIdentification(): void {
    if (this.idInput().length < 10) {
      this.messageService.add({ severity: 'warn', summary: '', detail: 'Ingrese al menos 10 dígitos.' });
      return;
    }
    this.searching.set(true);
    this.kioskService.checkClient(this.idInput()).subscribe({
      next: (client) => {
        this.searching.set(false);
        this.currentClient.set(client);
        this.step.set('found');
      },
      error: (err) => {
        this.searching.set(false);
        if (err.status === 404) {
          this.registerForm.reset();
          this.step.set('register');
        } else {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al consultar. Intente de nuevo.' });
        }
      }
    });
  }

  register(): void {
    if (this.registerForm.invalid) return;
    this.saving.set(true);
    this.kioskService.register({ ...this.registerForm.getRawValue(), identification: this.idInput() }).subscribe({
      next: (client) => {
        this.saving.set(false);
        this.currentClient.set(client);
        this.step.set('found');
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error al registrarse.' });
      }
    });
  }

  goSelectService(): void {
    this.catalogService.getAttentionTypes().subscribe({
      next: (types) => { this.attentionTypes.set(types); this.step.set('select-service'); },
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al cargar servicios.' })
    });
  }

  selectService(typeId: string): void {
    this.requesting.set(true);
    this.kioskService.requestTurn(this.currentClient()!.clientId, typeId).subscribe({
      next: (turn) => {
        this.requesting.set(false);
        this.issuedTurn.set(turn);
        this.step.set('turn-issued');
      },
      error: (err) => {
        this.requesting.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error al solicitar turno.' });
      }
    });
  }

  restart(): void {
    this.idInput.set('');
    this.currentClient.set(null);
    this.issuedTurn.set(null);
    this.attentionTypes.set([]);
    this.step.set('identify');
  }
}
