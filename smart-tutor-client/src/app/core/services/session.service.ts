import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResult } from '../models/auth.models';
import { AuthService } from './auth.service';

export interface StudentAttendanceUpdateItem {
  studentId: number;
  attendanceStatus: string;
  homeworkScore: number;
  attitude: string;
  individualNote?: string;
}

export interface ClassSessionAttendanceRequest {
  classId?: number;
  className?: string;
  sessionDate: string; // YYYY-MM-DD
  startTime?: string;
  endTime?: string;
  lessonContent?: string;
  attendances: StudentAttendanceUpdateItem[];
}

export interface ClassSessionAttendanceResponse {
  sessionId: number;
  classId: number;
  className: string;
  sessionDate: string;
  totalStudents: number;
  presentCount: number;
  excusedCount: number;
  absentCount: number;
  feePerSession: number;
  totalFeeCalculated: number;
  message: string;
}

export interface GenerateReportRequest {
  classId: number;
  reportMonth: string;
  studentId?: number;
  teacherComment?: string;
  roadmap?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SessionService {
  private apiUrl = `${environment.apiUrl}/sessions`;
  private reportsUrl = `${environment.apiUrl}/reports`;

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

  saveClassAttendance(data: ClassSessionAttendanceRequest): Observable<ApiResult<ClassSessionAttendanceResponse>> {
    return this.http.post<ApiResult<ClassSessionAttendanceResponse>>(
      `${this.apiUrl}/class-attendance`,
      data,
      { headers: this.getAuthHeaders() }
    );
  }

  generateMonthlyReports(data: GenerateReportRequest): Observable<ApiResult<any>> {
    return this.http.post<ApiResult<any>>(
      `${this.reportsUrl}/generate`,
      data,
      { headers: this.getAuthHeaders() }
    );
  }
}
