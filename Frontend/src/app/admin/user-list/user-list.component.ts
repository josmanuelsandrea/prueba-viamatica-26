import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MessageService, ConfirmationService } from 'primeng/api';
import { UserService, UserResponse } from '../../core/services/user.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserListComponent implements OnInit {
  private userService = inject(UserService);
  private messageService = inject(MessageService);
  private confirmService = inject(ConfirmationService);

  users = signal<UserResponse[]>([]);
  loading = signal(false);
  showDialog = signal(false);
  selectedUser = signal<UserResponse | null>(null);
  dialogMode = signal<'create' | 'edit'>('create');

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading.set(true);
    this.userService.getAll().subscribe({
      next: (data) => { this.users.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.selectedUser.set(null);
    this.dialogMode.set('create');
    this.showDialog.set(true);
  }

  openEdit(user: UserResponse): void {
    this.selectedUser.set(user);
    this.dialogMode.set('edit');
    this.showDialog.set(true);
  }

  onSaved(): void {
    this.showDialog.set(false);
    this.loadUsers();
  }

  approve(user: UserResponse): void {
    this.confirmService.confirm({
      message: `¿Aprobar al usuario ${user.username}?`,
      header: 'Confirmar aprobación',
      icon: 'pi pi-check',
      accept: () => {
        this.userService.approve(user.userId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Aprobado', detail: `Usuario ${user.username} aprobado.` });
            this.loadUsers();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'No se pudo aprobar.' })
        });
      }
    });
  }

  toggleStatus(user: UserResponse): void {
    const newStatus = user.statusId === 'ACT' ? 'INA' : 'ACT';
    const action = newStatus === 'ACT' ? 'activar' : 'desactivar';
    this.confirmService.confirm({
      message: `¿Desea ${action} al usuario ${user.username}?`,
      header: 'Confirmar cambio de estado',
      accept: () => {
        this.userService.changeStatus(user.userId, newStatus).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Actualizado', detail: `Estado cambiado correctamente.` });
            this.loadUsers();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error al cambiar estado.' })
        });
      }
    });
  }

  deleteUser(user: UserResponse): void {
    this.confirmService.confirm({
      message: `¿Eliminar al usuario ${user.username}? Esta acción no se puede deshacer.`,
      header: 'Confirmar eliminación',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.userService.delete(user.userId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Eliminado', detail: `Usuario ${user.username} eliminado.` });
            this.loadUsers();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err?.error?.message ?? 'Error al eliminar.' })
        });
      }
    });
  }

  onFileUpload(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const wb = XLSX.read(e.target?.result, { type: 'binary' });
        const ws = wb.Sheets[wb.SheetNames[0]];
        const rows: any[] = XLSX.utils.sheet_to_json(ws);
        let success = 0, errors = 0;
        const promises = rows.map(row =>
          this.userService.create({
            username: row['username'] ?? row['Usuario'],
            password: row['password'] ?? row['Contraseña'],
            email: row['email'] ?? row['Correo'],
            rolId: Number(row['rolId'] ?? row['Rol'])
          }).toPromise().then(() => success++).catch(() => errors++)
        );
        Promise.all(promises).then(() => {
          this.messageService.add({ severity: 'info', summary: 'Carga masiva', detail: `${success} creados, ${errors} errores.` });
          this.loadUsers();
        });
      } catch {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Archivo inválido.' });
      }
    };
    reader.readAsBinaryString(file);
    input.value = '';
  }

  getStatusSeverity(statusId: string): 'success' | 'warn' | 'danger' | 'secondary' {
    switch (statusId) {
      case 'ACT': return 'success';
      case 'PEN': return 'warn';
      case 'INA': return 'danger';
      default: return 'secondary';
    }
  }
}
