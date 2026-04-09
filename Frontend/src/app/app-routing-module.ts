import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { roleGuard } from './core/guards/role.guard';

const routes: Routes = [
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: 'welcome',
    canActivate: [authGuard],
    loadChildren: () => import('./welcome/welcome.module').then(m => m.WelcomeModule)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard, roleGuard],
    data: { roles: [1] },
    loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule)
  },
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { roles: [1] },
    loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule)
  },
  {
    path: 'gestor',
    canActivate: [authGuard, roleGuard],
    data: { roles: [2] },
    loadChildren: () => import('./gestor/gestor.module').then(m => m.GestorModule)
  },
  {
    path: 'cajero',
    canActivate: [authGuard, roleGuard],
    data: { roles: [3] },
    loadChildren: () => import('./cajero/cajero.module').then(m => m.CajeroModule)
  },
  {
    path: 'kiosk',
    loadChildren: () => import('./kiosk/kiosk.module').then(m => m.KioskModule)
  },
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  { path: '**', redirectTo: 'auth/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
