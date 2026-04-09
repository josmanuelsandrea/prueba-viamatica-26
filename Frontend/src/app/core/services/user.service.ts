import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserResponse {
  userId: number;
  username: string;
  email: string;
  rolId: number;
  rolName: string;
  statusId: string;
  statusDescription: string;
  createdAt: string;
}

export interface CreateUserDto {
  username: string;
  password: string;
  email: string;
  rolId: number;
}

export interface UpdateUserDto {
  username: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private url = `${environment.apiUrl}/users`;
  constructor(private http: HttpClient) {}

  getAll(rolId?: number, statusId?: string): Observable<UserResponse[]> {
    let params: Record<string, string> = {};
    if (rolId) params['rolId'] = rolId.toString();
    if (statusId) params['statusId'] = statusId;
    return this.http.get<UserResponse[]>(this.url, { params });
  }

  getById(id: number): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.url}/${id}`);
  }

  create(dto: CreateUserDto): Observable<UserResponse> {
    return this.http.post<UserResponse>(this.url, dto);
  }

  update(id: number, dto: UpdateUserDto): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.url}/${id}`, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  approve(id: number): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.url}/${id}/approve`, {});
  }

  changeStatus(id: number, statusId: string): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.url}/${id}/status`, { statusId });
  }
}
