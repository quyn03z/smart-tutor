import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResult } from '../models/auth.models';
import { 
  GenerateReportRequest, 
  MonthlyReportDetailResponse, 
  MonthlyReportResponse, 
  UpdateMonthlyReportRequest 
} from '../models/report.models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private apiUrl = `${environment.apiUrl}/reports`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  getStudentReportPreview(studentId: number, reportMonth: string, classId?: number): Observable<ApiResult<MonthlyReportDetailResponse>> {
    let params = new HttpParams()
      .set('studentId', studentId.toString())
      .set('reportMonth', reportMonth);

    if (classId) {
      params = params.set('classId', classId.toString());
    }

    return this.http.get<ApiResult<MonthlyReportDetailResponse>>(`${this.apiUrl}/preview`, {
      headers: this.getAuthHeaders(),
      params
    });
  }

  getMonthlyReports(reportMonth?: string, paymentStatus?: string, classId?: number): Observable<ApiResult<MonthlyReportResponse[]>> {
    let params = new HttpParams();
    if (reportMonth) params = params.set('reportMonth', reportMonth);
    if (paymentStatus) params = params.set('paymentStatus', paymentStatus);
    if (classId) params = params.set('classId', classId.toString());

    return this.http.get<ApiResult<MonthlyReportResponse[]>>(`${this.apiUrl}`, {
      headers: this.getAuthHeaders(),
      params
    });
  }

  getMonthlyReportDetail(reportId: number): Observable<ApiResult<MonthlyReportDetailResponse>> {
    return this.http.get<ApiResult<MonthlyReportDetailResponse>>(`${this.apiUrl}/${reportId}`, {
      headers: this.getAuthHeaders()
    });
  }

  generateMonthlyReports(data: GenerateReportRequest): Observable<ApiResult<MonthlyReportResponse[]>> {
    return this.http.post<ApiResult<MonthlyReportResponse[]>>(`${this.apiUrl}/generate`, data, {
      headers: this.getAuthHeaders()
    });
  }

  updateMonthlyReport(reportId: number, data: UpdateMonthlyReportRequest): Observable<ApiResult<MonthlyReportResponse>> {
    return this.http.put<ApiResult<MonthlyReportResponse>>(`${this.apiUrl}/${reportId}`, data, {
      headers: this.getAuthHeaders()
    });
  }
}
