export interface ReportSessionDetail {
  sessionId: number;
  sessionDate: string;
  startTime: string;
  endTime: string;
  durationHours: number;
  lessonContent?: string;
  attendanceStatus: string; // Present, Absent, Excused, Late
  homeworkScore: number;
  attitude: string; // Tốt, Chăm chỉ, Lười học, Nói chuyện, Cần tập trung...
  individualNote?: string;
}

export interface MonthlyReportResponse {
  id: number;
  studentId: number;
  studentName: string;
  classId: number;
  className: string;
  reportMonth: string;
  totalSessions: number;
  totalHours: number;
  grossAmount: number;
  creditDeducted: number;
  finalAmount: number;
  amountPaid: number;
  overpaidAmount: number;
  transferCode: string;
  magicToken: string;
  teacherComment?: string;
  roadmap?: string;
  paymentStatus: string;
  createdAt: string;
}

export interface MonthlyReportDetailResponse extends MonthlyReportResponse {
  parentName?: string;
  parentPhone?: string;
  gradeLevel?: string;
  subject?: string;
  classType?: string;
  ratePerSession: number;
  teacherName?: string;
  teacherPhone?: string;
  teacherBankCode?: string;
  teacherBankAccountNumber?: string;
  teacherBankAccountName?: string;
  parentAcknowledged: boolean;
  parentNote?: string;
  parentNoteTime?: string;
  sessions: ReportSessionDetail[];
}

export interface UpdateMonthlyReportRequest {
  teacherComment?: string;
  roadmap?: string;
  finalAmount?: number;
  paymentStatus?: string;
  parentAcknowledged?: boolean;
  parentNote?: string;
}

export interface GenerateReportRequest {
  classId: number;
  reportMonth: string;
  studentId?: number;
  teacherComment?: string;
  roadmap?: string;
}
