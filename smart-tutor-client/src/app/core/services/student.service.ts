import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResult } from '../models/auth.models';
import { RequestStudentModel, StudentsResponseModel } from '../models/student.models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private apiUrl = `${environment.apiUrl}/students`;

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

  createStudent(data: RequestStudentModel): Observable<ApiResult<StudentsResponseModel>> {
    return this.http.post<ApiResult<StudentsResponseModel>>(`${this.apiUrl}/create`, data, {
      headers: this.getAuthHeaders()
    });
  }

  getMyStudents(): Observable<ApiResult<StudentsResponseModel[]>> {
    return this.http.get<ApiResult<StudentsResponseModel[]>>(`${this.apiUrl}/my-students`, {
      headers: this.getAuthHeaders()
    });
  }

  editStudent(data: RequestStudentModel): Observable<ApiResult<StudentsResponseModel>> {
    return this.http.put<ApiResult<StudentsResponseModel>>(`${this.apiUrl}/edit`, data, {
      headers: this.getAuthHeaders()
    });
  }

  deleteStudent(studentId: number | string): Observable<ApiResult<string>> {
    return this.http.delete<ApiResult<string>>(`${this.apiUrl}/${studentId}`, {
      headers: this.getAuthHeaders()
    });
  }
}
