import { Component, OnInit, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserService, UserResponse } from '../core/services/user.service';
import { CashService, CashResponse } from '../core/services/cash.service';
import { environment } from '../../environments/environment';

interface DailySummary { total: number; date: string; }

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent implements OnInit {
  private http = inject(HttpClient);
  private userService = inject(UserService);
  private cashService = inject(CashService);

  summary = signal<DailySummary | null>(null);
  users = signal<UserResponse[]>([]);
  cashes = signal<CashResponse[]>([]);
  pendingUsers = signal<UserResponse[]>([]);
  loading = signal(true);

  totalUsers = signal(0);
  totalCashes = signal(0);
  activeUsers = signal(0);

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);

    this.http.get<DailySummary>(`${environment.apiUrl}/attentions/daily-summary`).subscribe({
      next: (d) => this.summary.set(d)
    });

    this.userService.getAll().subscribe({
      next: (users) => {
        this.users.set(users);
        this.totalUsers.set(users.length);
        this.activeUsers.set(users.filter(u => u.statusId === 'ACT').length);
        this.pendingUsers.set(users.filter(u => u.statusId === 'PEN'));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    this.cashService.getAll().subscribe({
      next: (cashes) => {
        this.cashes.set(cashes);
        this.totalCashes.set(cashes.length);
      }
    });
  }
}
