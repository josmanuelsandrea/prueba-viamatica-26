import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CatalogItem {
  id: number | string;
  description: string;
}

export interface AttentionTypeItem {
  id: string;
  description: string;
  prefix: string;
}

export interface RolItem {
  id: number;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private url = `${environment.apiUrl}/catalogs`;
  constructor(private http: HttpClient) {}

  getAttentionTypes(): Observable<AttentionTypeItem[]> {
    return this.http.get<AttentionTypeItem[]>(`${this.url}/attention-types`);
  }

  getMethodPayments(): Observable<CatalogItem[]> {
    return this.http.get<CatalogItem[]>(`${this.url}/method-payments`);
  }

  getRoles(): Observable<RolItem[]> {
    return this.http.get<RolItem[]>(`${this.url}/roles`);
  }

  getStatusContracts(): Observable<CatalogItem[]> {
    return this.http.get<CatalogItem[]>(`${this.url}/status-contracts`);
  }
}
