import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface KioskClientResponse {
  clientId: number;
  name: string;
  lastname: string;
  identification: string;
  isNew: boolean;
}

export interface KioskRegisterDto {
  name: string;
  lastname: string;
  identification: string;
  phone: string;
  address: string;
  referenceAddress: string;
  email: string;
}

export interface KioskTurnResponse {
  turnId: number;
  turnNumber: string;
  attentionTypeDescription: string;
  cashDescription: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class KioskService {
  private url = `${environment.apiUrl}/kiosk`;
  constructor(private http: HttpClient) {}

  checkClient(identification: string): Observable<KioskClientResponse> {
    return this.http.get<KioskClientResponse>(`${this.url}/check/${identification}`);
  }

  register(dto: KioskRegisterDto): Observable<KioskClientResponse> {
    return this.http.post<KioskClientResponse>(`${this.url}/register`, dto);
  }

  requestTurn(clientId: number, attentionTypeId: string): Observable<KioskTurnResponse> {
    return this.http.post<KioskTurnResponse>(`${this.url}/request-turn`, { clientId, attentionTypeId });
  }
}
