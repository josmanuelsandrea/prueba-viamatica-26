import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TurnoComponent } from './turno/turno.component';
import { CajaComponent } from './caja/caja.component';

const routes: Routes = [
  { path: 'turnos', component: TurnoComponent },
  { path: 'cajas', component: CajaComponent },
  { path: '', redirectTo: 'turnos', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GestorRoutingModule {}
