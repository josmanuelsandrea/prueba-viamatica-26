import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { CajeroRoutingModule } from './cajero-routing.module';
import { ClienteListComponent } from './cliente-list/cliente-list.component';
import { ContratoComponent } from './contrato/contrato.component';
import { PagoComponent } from './pago/pago.component';
import { AtencionComponent } from './atencion/atencion.component';
import { SharedModule } from '../shared/shared.module';

import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DatePickerModule } from 'primeng/datepicker';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { AvatarModule } from 'primeng/avatar';
import { ConfirmationService, MessageService } from 'primeng/api';

@NgModule({
  declarations: [ClienteListComponent, ContratoComponent, PagoComponent, AtencionComponent],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CajeroRoutingModule,
    SharedModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    CardModule,
    TagModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    DatePickerModule,
    TooltipModule,
    DividerModule,
    AvatarModule
  ],
  providers: [MessageService, ConfirmationService]
})
export class CajeroModule {}
