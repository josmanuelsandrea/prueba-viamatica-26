import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private url = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  getClientesConContratosVigentes(): Observable<string> {
    return this.http.get(`${this.url}/clientes-contratos`, { responseType: 'text' });
  }
}
