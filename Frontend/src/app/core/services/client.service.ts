import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ClientResponse {
  clientId: number;
  name: string;
  lastname: string;
  identification: string;
  phone: string;
  address: string;
  referenceAddress: string;
  email: string;
  active: boolean;
  createdAt: string;
}

export interface CreateClientDto {
  name: string;
  lastname: string;
  identification: string;
  phone: string;
  address: string;
  referenceAddress: string;
  email: string;
}

export interface UpdateClientDto {
  name: string;
  lastname: string;
  phone: string;
  address: string;
  referenceAddress: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class ClientService {
  private url = `${environment.apiUrl}/clients`;
  constructor(private http: HttpClient) {}

  getAll(search?: string): Observable<ClientResponse[]> {
    let params: Record<string, string> = {};
    if (search) params['search'] = search;
    return this.http.get<ClientResponse[]>(this.url, { params });
  }

  getById(id: number): Observable<ClientResponse> {
    return this.http.get<ClientResponse>(`${this.url}/${id}`);
  }

  getByIdentification(identification: string): Observable<ClientResponse> {
    return this.http.get<ClientResponse>(`${this.url}/by-identification/${identification}`);
  }

  create(dto: CreateClientDto): Observable<ClientResponse> {
    return this.http.post<ClientResponse>(this.url, dto);
  }

  update(id: number, dto: UpdateClientDto): Observable<ClientResponse> {
    return this.http.put<ClientResponse>(`${this.url}/${id}`, dto);
  }

  changeStatus(id: number, active: boolean): Observable<ClientResponse> {
    return this.http.put<ClientResponse>(`${this.url}/${id}/status`, { active });
  }
}
