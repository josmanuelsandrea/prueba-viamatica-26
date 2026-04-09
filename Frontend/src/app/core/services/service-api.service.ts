import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DeviceItem {
  deviceId: number;
  deviceName: string;
  active: boolean;
}

export interface ServiceResponse {
  serviceId: number;
  serviceName: string;
  serviceDescription: string;
  speedMbps: number;
  price: number;
  active: boolean;
  devices: DeviceItem[];
}

@Injectable({ providedIn: 'root' })
export class ServiceApiService {
  private url = `${environment.apiUrl}/services`;
  constructor(private http: HttpClient) {}

  getAll(includeInactive = false): Observable<ServiceResponse[]> {
    return this.http.get<ServiceResponse[]>(this.url, {
      params: { includeInactive: includeInactive.toString() }
    });
  }
}
