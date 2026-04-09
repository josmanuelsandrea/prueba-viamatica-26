import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TurnResponse {
  turnId: number;
  description: string;
  date: string;
  cashId: number;
  cashDescription: string;
  userGestorId: number;
  userGestorName: string;
  createdAt: string;
}

export interface CreateTurnDto {
  cashId: number;
  attentionTypeId: string;
}

@Injectable({ providedIn: 'root' })
export class TurnService {
  private url = `${environment.apiUrl}/turns`;
  constructor(private http: HttpClient) {}

  getAll(cashId?: number, date?: string): Observable<TurnResponse[]> {
    let params: Record<string, string> = {};
    if (cashId) params['cashId'] = cashId.toString();
    if (date) params['date'] = date;
    return this.http.get<TurnResponse[]>(this.url, { params });
  }

  create(dto: CreateTurnDto): Observable<TurnResponse> {
    return this.http.post<TurnResponse>(this.url, dto);
  }
}
