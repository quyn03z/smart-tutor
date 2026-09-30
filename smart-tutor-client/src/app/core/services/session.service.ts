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

export interface ClassSessionAttendanceDetail {
  sessionId?: number;
  classId?: number;
  className?: string;
  sessionDate: string;
  lessonContent?: string;
  hasRecorded: boolean;
  attendances: StudentAttendanceUpdateItem[];
}

export interface SessionRespond {
  id: number;
  classId: number;
  className?: string;
  classType?: string;
  feePerSession: number;
  studentsCount: number;
  studentName?: string;
  sessionDate: string;
  startTime: string;
  endTime: string;
  durationHours: number;
  lessonContent?: string;
  status: string;
  presentCount?: number;
  avgHomeworkScore?: number;
}

export interface SessionRequest {
  id?: number;
  classId: number;
  sessionDate: string;
  startTime: string;
  endTime: string;
  durationHours?: number;
  lessonContent?: string;
  status?: string;
  isRecurring?: boolean;
  recurringWeeks?: number;
  recurrencePattern?: string; // NONE, DAILY, WEEKLY, WEEKDAYS, CUSTOM
  recurringCount?: number;
  removeFutureRecurring?: boolean;
}

export interface StudentAttendanceDetailItem {
  studentId: number;
  studentName: string;
  parentPhone?: string;
  attendanceLogId?: number;
  attendanceStatus: string;
  homeworkScore: number;
  attitude: string;
  individualNote?: string;
}

export interface SessionAttendanceDetailResponse {
  sessionId: number;
  classId: number;
  className: string;
  classType: string;
  sessionDate: string;
  startTime: string;
  endTime: string;
  durationHours: number;
  lessonContent?: string;
  status: string;
  totalStudents: number;
  presentCount: number;
  absentCount: number;
  students: StudentAttendanceDetailItem[];
}

export interface BulkAttendanceRequest {
  attendances: StudentAttendanceUpdateItem[];
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

  getClassAttendance(classId?: number, className?: string, sessionDate?: string): Observable<ApiResult<ClassSessionAttendanceDetail>> {
    let params: any = {};
    if (classId) params.classId = classId;
    if (className) params.className = className;
    if (sessionDate) params.sessionDate = sessionDate;

    return this.http.get<ApiResult<ClassSessionAttendanceDetail>>(
      `${this.apiUrl}/class-attendance`,
      {
        headers: this.getAuthHeaders(),
        params: params
      }
    );
  }

  getTeachingSessions(fromDate?: string, toDate?: string): Observable<ApiResult<SessionRespond[]>> {
    let params: any = {};
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;

    return this.http.get<ApiResult<SessionRespond[]>>(
      `${this.apiUrl}/teaching`,
      {
        headers: this.getAuthHeaders(),
        params: params
      }
    );
  }

  createSession(data: SessionRequest): Observable<ApiResult<SessionRespond>> {
    return this.http.post<ApiResult<SessionRespond>>(
      `${this.apiUrl}/create`,
      data,
      { headers: this.getAuthHeaders() }
    );
  }

  editSession(data: SessionRequest): Observable<ApiResult<SessionRespond>> {
    return this.http.put<ApiResult<SessionRespond>>(
      `${this.apiUrl}/edit`,
      data,
      { headers: this.getAuthHeaders() }
    );
  }

  deleteSession(sessionId: number): Observable<ApiResult<string>> {
    return this.http.delete<ApiResult<string>>(
      `${this.apiUrl}/sessionId`,
      {
        headers: this.getAuthHeaders(),
        params: { sessionId: sessionId.toString() }
      }
    );
  }

  getSessionAttendance(sessionId: number): Observable<ApiResult<SessionAttendanceDetailResponse>> {
    return this.http.get<ApiResult<SessionAttendanceDetailResponse>>(
      `${this.apiUrl}/${sessionId}/attendance`,
      { headers: this.getAuthHeaders() }
    );
  }

  saveBulkAttendance(sessionId: number, data: BulkAttendanceRequest): Observable<ApiResult<string>> {
    return this.http.post<ApiResult<string>>(
      `${this.apiUrl}/${sessionId}/attendance/bulk`,
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
