import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ContractResponse {
  contractId: number;
  clientId: number;
  clientName: string;
  serviceId: number;
  serviceName: string;
  methodPaymentId: number;
  methodPaymentName: string;
  statusCode: string;
  statusDescription: string;
  startDate: string;
  endDate: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateContractDto {
  clientId: number;
  serviceId: number;
  methodPaymentId: number;
  startDate: string;
  endDate: string;
}

@Injectable({ providedIn: 'root' })
export class ContractService {
  private url = `${environment.apiUrl}/contracts`;
  constructor(private http: HttpClient) {}

  getAll(clientId?: number, status?: string): Observable<ContractResponse[]> {
    let params: Record<string, string> = {};
    if (clientId) params['clientId'] = clientId.toString();
    if (status) params['status'] = status;
    return this.http.get<ContractResponse[]>(this.url, { params });
  }

  getById(id: number): Observable<ContractResponse> {
    return this.http.get<ContractResponse>(`${this.url}/${id}`);
  }

  create(dto: CreateContractDto): Observable<ContractResponse> {
    return this.http.post<ContractResponse>(this.url, dto);
  }

  changeService(id: number, newServiceId: number): Observable<ContractResponse> {
    return this.http.put<ContractResponse>(`${this.url}/${id}/change-service`, { newServiceId });
  }

  changePaymentMethod(id: number, methodPaymentId: number): Observable<ContractResponse> {
    return this.http.put<ContractResponse>(`${this.url}/${id}/change-payment-method`, { methodPaymentId });
  }

  cancel(id: number): Observable<ContractResponse> {
    return this.http.put<ContractResponse>(`${this.url}/${id}/cancel`, {});
  }
}
