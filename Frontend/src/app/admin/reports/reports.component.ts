import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { ReportService } from '../../core/services/report.service';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: false
})
export class ReportsComponent {
  html = signal<SafeHtml | null>(null);
  loading = signal(false);

  constructor(
    private reportService: ReportService,
    private sanitizer: DomSanitizer
  ) {}

  loadReport(): void {
    this.loading.set(true);
    this.reportService.getClientesConContratosVigentes().subscribe({
      next: (html) => {
        this.html.set(this.sanitizer.bypassSecurityTrustHtml(html));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  print(): void {
    window.print();
  }
}
