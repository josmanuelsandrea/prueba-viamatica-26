import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PaymentResponse {
  paymentId: number;
  contractId: number;
  clientId: number;
  clientName: string;
  serviceName: string;
  amount: number;
  attentionId: number;
  createdAt: string;
}

export interface CreatePaymentDto {
  contractId: number;
  amount: number;
  attentionId: number;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private url = `${environment.apiUrl}/payments`;
  constructor(private http: HttpClient) {}

  getAll(contractId?: number, date?: string): Observable<PaymentResponse[]> {
    let params: Record<string, string> = {};
    if (contractId) params['contractId'] = contractId.toString();
    if (date) params['date'] = date;
    return this.http.get<PaymentResponse[]>(this.url, { params });
  }

  create(dto: CreatePaymentDto): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(this.url, dto);
  }
}
