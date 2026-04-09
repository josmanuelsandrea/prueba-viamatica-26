import { Component, OnInit, ChangeDetectionStrategy, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavbarComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  menuItems = signal<MenuItem[]>([]);

  readonly username = computed(() => this.authService.currentUser()?.username ?? '');
  readonly rolName = computed(() => this.authService.currentUser()?.rolName ?? '');

  ngOnInit(): void {
    const appMenu = this.authService.menu();
    const items: MenuItem[] = appMenu.map(item => ({
      label: item.label,
      icon: item.icon,
      command: () => this.router.navigateByUrl(item.route)
    }));
    this.menuItems.set(items);
  }

  logout(): void {
    this.authService.logout();
  }
}
