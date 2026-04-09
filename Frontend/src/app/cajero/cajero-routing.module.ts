import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ClienteListComponent } from './cliente-list/cliente-list.component';
import { ContratoComponent } from './contrato/contrato.component';
import { PagoComponent } from './pago/pago.component';
import { AtencionComponent } from './atencion/atencion.component';

const routes: Routes = [
  { path: 'atenciones', component: AtencionComponent },
  { path: 'clientes', component: ClienteListComponent },
  { path: 'contratos', component: ContratoComponent },
  { path: 'pagos', component: PagoComponent },
  { path: '', redirectTo: 'atenciones', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CajeroRoutingModule {}
