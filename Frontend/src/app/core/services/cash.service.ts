import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserCashItem {
  userId: number;
  username: string;
  isActive: boolean;
  assignedAt: string;
}

export interface CashResponse {
  cashId: number;
  cashDescription: string;
  active: boolean;
  assignedUsers: UserCashItem[];
}

@Injectable({ providedIn: 'root' })
export class CashService {
  private url = `${environment.apiUrl}/cash`;
  constructor(private http: HttpClient) {}

  getAll(): Observable<CashResponse[]> {
    return this.http.get<CashResponse[]>(this.url);
  }

  create(dto: { cashDescription: string }): Observable<CashResponse> {
    return this.http.post<CashResponse>(this.url, dto);
  }

  update(cashId: number, dto: { cashDescription: string }): Observable<CashResponse> {
    return this.http.put<CashResponse>(`${this.url}/${cashId}`, dto);
  }

  delete(cashId: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${cashId}`);
  }

  assignUser(cashId: number, userId: number): Observable<CashResponse> {
    return this.http.post<CashResponse>(`${this.url}/${cashId}/assign-user`, { userId });
  }

  removeUser(cashId: number, userId: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${cashId}/remove-user/${userId}`);
  }

  activateUser(cashId: number, userId: number): Observable<CashResponse> {
    return this.http.put<CashResponse>(`${this.url}/${cashId}/activate-user/${userId}`, {});
  }

  deactivateUser(cashId: number, userId: number): Observable<CashResponse> {
    return this.http.put<CashResponse>(`${this.url}/${cashId}/deactivate-user/${userId}`, {});
  }
}
