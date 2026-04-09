import { Component, OnInit, ChangeDetectionStrategy, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../core/services/auth.service';
import { UserResponse } from '../core/services/user.service';
import { environment } from '../../environments/environment';

interface DailySummary {
  total: number;
  date: string;
}

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.component.html',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WelcomeComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);

  summary = signal<DailySummary | null>(null);
  loading = signal(true);
  pendingUsers = signal<UserResponse[]>([]);

  readonly username = computed(() => this.authService.currentUser()?.username ?? '');
  readonly rolName = computed(() => this.authService.currentUser()?.rolName ?? '');
  readonly rolId = computed(() => this.authService.rolId());

  ngOnInit(): void {
    this.http.get<DailySummary>(`${environment.apiUrl}/attentions/daily-summary`).subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    if (this.rolId() === 2) {
      this.http.get<UserResponse[]>(`${environment.apiUrl}/users`, { params: { statusId: 'PEN' } }).subscribe({
        next: (users) => this.pendingUsers.set(users),
        error: () => {}
      });
    }
  }
}
