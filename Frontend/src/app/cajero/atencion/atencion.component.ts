import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MessageService, ConfirmationService } from 'primeng/api';
import { AuthService } from '../../core/services/auth.service';
import { CashResponse } from '../../core/services/cash.service';
import { environment } from '../../../environments/environment';

export interface AttentionItem {
  attentionId: number;
  turnId: number;
  turnDescription: string;
  clientId: number;
  clientName: string | null;
  clientIdentification: string | null;
  attentionTypeId: string;
  attentionTypeDescription: string;
  statusId: number;
  statusDescription: string;
  createdAt: string;
}

@Component({
  selector: 'app-atencion',
  templateUrl: './atencion.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AtencionComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly messageService = inject(MessageService);
  private readonly confirmService = inject(ConfirmationService);
  private readonly authService = inject(AuthService);

  readonly attentions = signal<AttentionItem[]>([]);
  readonly activeCash = signal<CashResponse | null>(null);
  readonly loading = signal(false);

  get pendingCount(): number {
    return this.attentions().filter(a => a.statusId === 1).length;
  }

  ngOnInit(): void {
    this.loadActiveCashAndAttentions();
  }

  loadActiveCashAndAttentions(): void {
    this.loading.set(true);
    const userId = this.authService.userId();

    this.http.get<CashResponse[]>(`${environment.apiUrl}/cash`).subscribe({
      next: (cajas) => {
        const myCash = cajas.find(c =>
          c.assignedUsers?.some(u => u.userId === userId && u.isActive)
        ) ?? null;
        this.activeCash.set(myCash);

        if (myCash) {
          this.http
            .get<AttentionItem[]>(`${environment.apiUrl}/attentions`, {
              params: { cashId: myCash.cashId.toString() }
            })
            .subscribe({
              next: (data) => { this.attentions.set(data); this.loading.set(false); },
              error: () => this.loading.set(false)
            });
        } else {
          this.attentions.set([]);
          this.loading.set(false);
        }
      },
      error: () => this.loading.set(false)
    });
  }

  markAttended(item: AttentionItem): void {
    this.confirmService.confirm({
      message: `¿Marcar el turno ${item.turnDescription} como atendido?`,
      header: 'Confirmar',
      accept: () => this.updateStatus(item, 2, 'Atendido')
    });
  }

  markCancelled(item: AttentionItem): void {
    this.confirmService.confirm({
      message: `¿Cancelar el turno ${item.turnDescription}?`,
      header: 'Confirmar cancelación',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.updateStatus(item, 3, 'Cancelado')
    });
  }

  getStatusSeverity(statusId: number): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    if (statusId === 1) return 'warn';
    if (statusId === 2) return 'success';
    if (statusId === 3) return 'danger';
    return 'secondary';
  }

  private updateStatus(item: AttentionItem, statusId: number, label: string): void {
    this.http
      .put<AttentionItem>(`${environment.apiUrl}/attentions/${item.attentionId}/status`, { statusId })
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: label,
            detail: `Turno ${item.turnDescription} marcado como ${label.toLowerCase()}.`
          });
          this.loadActiveCashAndAttentions();
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: err?.error?.message ?? 'No se pudo actualizar.'
          });
        }
      });
  }
}
