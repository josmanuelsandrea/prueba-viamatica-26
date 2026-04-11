import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserListComponent } from './user-list/user-list.component';
import { CashListComponent } from './cash-list/cash-list.component';
import { ReportsComponent } from './reports/reports.component';

const routes: Routes = [
  { path: 'users', component: UserListComponent },
  { path: 'cajas', component: CashListComponent },
  { path: 'reportes', component: ReportsComponent },
  { path: '', redirectTo: 'users', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule {}
