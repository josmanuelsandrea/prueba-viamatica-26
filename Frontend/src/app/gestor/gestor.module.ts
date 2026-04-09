import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { GestorRoutingModule } from './gestor-routing.module';
import { TurnoComponent } from './turno/turno.component';
import { CajaComponent } from './caja/caja.component';
import { SharedModule } from '../shared/shared.module';

import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';

@NgModule({
  declarations: [TurnoComponent, CajaComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    GestorRoutingModule,
    SharedModule,
    TableModule,
    ButtonModule,
    SelectModule,
    CardModule,
    TagModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TooltipModule
  ],
  providers: [MessageService, ConfirmationService]
})
export class GestorModule {}
