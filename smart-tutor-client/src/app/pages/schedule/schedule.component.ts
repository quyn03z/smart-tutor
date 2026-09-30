import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SessionService, SessionRespond, SessionRequest, StudentAttendanceUpdateItem } from '../../core/services/session.service';
import { StudentService } from '../../core/services/student.service';
import { StudentsResponseModel } from '../../core/models/student.models';

export interface ScheduleSessionItem {
  id: number;
  classId: number;
  className: string;
  classType: 'Group' | 'Individual';
  studentName?: string;
  studentsCount: number;
  feePerSession: number;
  sessionDate: string; // YYYY-MM-DD
  startTime: string; // HH:mm
  endTime: string; // HH:mm
  timeRange: string;
  durationHours: number;
  lessonContent?: string;
  status: string;
  presentCount: number;
  avgHomeworkScore?: number | null;
  borderClass: string;
  statusTag: string;
}

export interface WeekDayItem {
  date: Date;
  dateStr: string; // YYYY-MM-DD
  dayName: string; // 'Thứ 2 (04/08)'
  shortDayName: string; // 'Thứ 2'
  formattedDate: string; // '04/08'
  fullTitle: string; // 'HÔM NAY – THỨ HAI (04/08/2026)'
  isToday: boolean;
  sessions: ScheduleSessionItem[];
}

export interface ClassOptionItem {
  classId: number;
  displayName: string;
  classType: 'Group' | 'Individual';
  feePerSession: number;
  defaultSchedule?: string;
}

@Component({
  selector: 'app-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './schedule.component.html',
  styleUrl: './schedule.component.scss'
})
export class ScheduleComponent implements OnInit {
  scheduleView: 'list' | 'grid' = 'list';
  isLoading = false;

  currentWeekOffset = 0;
  daysOfWeek: WeekDayItem[] = [];
  weekRangeTitle = '';
  todaySessionsCount = 0;

  // Options for adding sessions
  availableClasses: ClassOptionItem[] = [];

  // Add session modal
  isAddSessionModalOpen = false;
  selectedAddClassId: number | null = null;
  newSessionDate = ''; // YYYY-MM-DD
  newSessionStartTime = '18:00';
  newSessionEndTime = '20:00';
  newSessionTopic = '';
  isSubmittingSession = false;

  // Attendance & Edit Log Modal (for 1-1 or quick log)
  isAttendanceModalOpen = false;
  activeSession: ScheduleSessionItem | null = null;
  activeAttendanceStatus: 'present' | 'excused' | 'unexcused' = 'present';
  activeHw = '100%';
  activeAttitude = 'Tốt';
  activeNote = '';
  activeLessonTopic = '';
  isSavingLog = false;

  hwOptions: string[] = ['100%', '90%', '80%', '50%', '0%'];
  attitudeOptions: string[] = ['Tốt', 'Chăm chỉ', 'Cần tập trung', 'Lười học', 'Nói chuyện'];

  toastMessage: string | null = null;
  toastType: 'success' | 'info' | 'warning' = 'info';
  private toastTimer: any = null;

  constructor(
    private router: Router,
    private sessionService: SessionService,
    private studentService: StudentService
  ) {}

  ngOnInit(): void {
    this.loadClassesOptions();
    this.loadSchedule();
  }

  loadClassesOptions(): void {
    this.studentService.getMyStudents().subscribe({
      next: (res) => {
        if (res?.succeeded && res.result) {
          const list: ClassOptionItem[] = [];
          const map = new Map<string, StudentsResponseModel[]>();

          res.result.forEach(s => {
            if (s.classType === 'Group') {
              const key = (s.className || 'Lớp Nhóm').trim();
              if (!map.has(key)) map.set(key, []);
              map.get(key)!.push(s);
            } else {
              // 1-1 student
              if (s.classId) {
                list.push({
                  classId: s.classId,
                  displayName: `Gia sư 1-1: ${s.fullName} (${s.gradeLevel || '1-1'})`,
                  classType: 'Individual',
                  feePerSession: s.feePerSession || 120000
                });
              }
            }
          });

          map.forEach((students, className) => {
            const first = students[0];
            if (first.classId) {
              list.push({
                classId: first.classId,
                displayName: `Lớp Nhóm: ${className} (${students.length} HS)`,
                classType: 'Group',
                feePerSession: first.feePerSession || 80000
              });
            }
          });

          this.availableClasses = list;
          if (list.length > 0 && !this.selectedAddClassId) {
            this.selectedAddClassId = list[0].classId;
          }
        }
      }
    });
  }

  loadSchedule(): void {
    this.daysOfWeek = this.generateWeekDays();
    const fromDate = this.daysOfWeek[0].dateStr;
    const toDate = this.daysOfWeek[6].dateStr;

    // Set title
    const firstDate = this.daysOfWeek[0].date;
    const lastDate = this.daysOfWeek[6].date;
    const mm = String(firstDate.getMonth() + 1).padStart(2, '0');
    const yyyy = firstDate.getFullYear();
    const dd1 = String(firstDate.getDate()).padStart(2, '0');
    const mm1 = String(firstDate.getMonth() + 1).padStart(2, '0');
    const dd2 = String(lastDate.getDate()).padStart(2, '0');
    const mm2 = String(lastDate.getMonth() + 1).padStart(2, '0');
    this.weekRangeTitle = `Tháng ${mm}, ${yyyy} (Tuần ${dd1}/${mm1} - ${dd2}/${mm2})`;

    this.isLoading = true;
    this.sessionService.getTeachingSessions(fromDate, toDate).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res?.succeeded && res.result) {
          this.distributeSessions(res.result);
        } else {
          this.distributeSessions([]);
        }
      },
      error: () => {
        this.isLoading = false;
        this.distributeSessions([]);
      }
    });
  }

  private distributeSessions(sessions: SessionRespond[]): void {
    let todayCount = 0;
    const todayStr = this.formatDateIso(new Date());

    this.daysOfWeek.forEach(day => {
      const daySessions = sessions.filter(s => {
        const sDate = s.sessionDate ? s.sessionDate.split('T')[0] : '';
        return sDate === day.dateStr;
      });

      day.sessions = daySessions.map(s => this.mapSessionItem(s));

      if (day.dateStr === todayStr) {
        todayCount = day.sessions.length;
      }
    });

    this.todaySessionsCount = todayCount;
  }

  private mapSessionItem(s: SessionRespond): ScheduleSessionItem {
    const isCompleted = s.status === 'Completed';
    const classType = (s.classType === 'Group' || s.studentsCount > 1) ? 'Group' : 'Individual';
    const startTimeStr = this.formatTimeSpan(s.startTime);
    const endTimeStr = this.formatTimeSpan(s.endTime);

    return {
      id: s.id,
      classId: s.classId,
      className: s.className || (classType === 'Group' ? 'Lớp Nhóm' : 'Gia sư 1-1'),
      classType: classType,
      studentName: s.studentName || s.className || '',
      studentsCount: s.studentsCount || 1,
      feePerSession: s.feePerSession || (classType === 'Group' ? 80000 : 120000),
      sessionDate: s.sessionDate ? s.sessionDate.split('T')[0] : '',
      startTime: startTimeStr,
      endTime: endTimeStr,
      timeRange: `${startTimeStr} - ${endTimeStr}`,
      durationHours: s.durationHours || 2,
      lessonContent: s.lessonContent || 'Ôn tập và luyện tập chuyên đề.',
      status: s.status,
      presentCount: s.presentCount || 0,
      avgHomeworkScore: s.avgHomeworkScore,
      borderClass: isCompleted ? 'border-left-emerald' : (classType === 'Group' ? 'border-left-indigo' : 'border-left-emerald'),
      statusTag: isCompleted ? '● Đã hoàn thành' : '● Đã lên lịch'
    };
  }

  private formatTimeSpan(val: any): string {
    if (!val) return '18:00';
    if (typeof val === 'string') {
      const parts = val.split(':');
      if (parts.length >= 2) return `${parts[0].padStart(2, '0')}:${parts[1].padStart(2, '0')}`;
      return val;
    }
    return '18:00';
  }

  private generateWeekDays(): WeekDayItem[] {
    const monday = this.getMondayOfWeek(this.currentWeekOffset);
    const days: WeekDayItem[] = [];
    const dayNames = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'];
    const todayStr = this.formatDateIso(new Date());

    for (let i = 0; i < 7; i++) {
      const date = new Date(monday);
      date.setDate(monday.getDate() + i);
      const dateStr = this.formatDateIso(date);
      const dd = String(date.getDate()).padStart(2, '0');
      const mm = String(date.getMonth() + 1).padStart(2, '0');
      const yyyy = date.getFullYear();
      const isToday = dateStr === todayStr;

      const fullTitle = `${isToday ? 'HÔM NAY – ' : ''}${dayNames[i].toUpperCase()} (${dd}/${mm}/${yyyy})`;

      days.push({
        date: date,
        dateStr: dateStr,
        dayName: `${dayNames[i]} (${dd}/${mm})`,
        shortDayName: dayNames[i],
        formattedDate: `${dd}/${mm}`,
        fullTitle: fullTitle,
        isToday: isToday,
        sessions: []
      });
    }
    return days;
  }

  private getMondayOfWeek(offsetWeeks: number = 0): Date {
    const d = new Date();
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1) + (offsetWeeks * 7);
    const mon = new Date(d.setDate(diff));
    mon.setHours(0, 0, 0, 0);
    return mon;
  }

  private formatDateIso(d: Date): string {
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  toggleScheduleView(view: 'list' | 'grid'): void {
    this.scheduleView = view;
  }

  prevWeek(): void {
    this.currentWeekOffset--;
    this.loadSchedule();
  }

  currentWeek(): void {
    this.currentWeekOffset = 0;
    this.loadSchedule();
  }

  nextWeek(): void {
    this.currentWeekOffset++;
    this.loadSchedule();
  }

  openAddSessionModal(presetDate?: string): void {
    this.newSessionDate = presetDate || this.formatDateIso(new Date());
    this.newSessionStartTime = '18:00';
    this.newSessionEndTime = '20:00';
    this.newSessionTopic = '';
    if (this.availableClasses.length > 0 && !this.selectedAddClassId) {
      this.selectedAddClassId = this.availableClasses[0].classId;
    }
    this.isAddSessionModalOpen = true;
  }

  closeAddSessionModal(): void {
    this.isAddSessionModalOpen = false;
  }

  submitAddSession(): void {
    if (!this.selectedAddClassId) {
      this.showToast('Vui lòng chọn lớp học hoặc học sinh!', 'warning');
      return;
    }
    if (!this.newSessionDate) {
      this.showToast('Vui lòng chọn ngày học!', 'warning');
      return;
    }
    if (!this.newSessionTopic.trim()) {
      this.showToast('Vui lòng nhập nội dung bài dạy!', 'warning');
      return;
    }

    const start = this.newSessionStartTime.length === 5 ? this.newSessionStartTime + ':00' : this.newSessionStartTime;
    const end = this.newSessionEndTime.length === 5 ? this.newSessionEndTime + ':00' : this.newSessionEndTime;

    let duration = 2;
    if (this.newSessionStartTime && this.newSessionEndTime) {
      const [sh, sm] = this.newSessionStartTime.split(':').map(Number);
      const [eh, em] = this.newSessionEndTime.split(':').map(Number);
      const diffMinutes = (eh * 60 + em) - (sh * 60 + sm);
      if (diffMinutes > 0) {
        duration = diffMinutes / 60;
      }
    }

    const payload: SessionRequest = {
      classId: Number(this.selectedAddClassId),
      sessionDate: this.newSessionDate,
      startTime: start,
      endTime: end,
      durationHours: duration,
      lessonContent: this.newSessionTopic.trim(),
      status: 'Scheduled'
    };

    this.isSubmittingSession = true;
    this.sessionService.createSession(payload).subscribe({
      next: (res) => {
        this.isSubmittingSession = false;
        if (res?.succeeded) {
          this.closeAddSessionModal();
          this.showToast('✓ Đã thêm ca dạy mới vào lịch giảng dạy thành công!', 'success');
          this.loadSchedule();
        } else {
          this.showToast(res?.message || 'Có lỗi khi tạo ca dạy', 'warning');
        }
      },
      error: (err) => {
        this.isSubmittingSession = false;
        const msg = err?.error?.message || 'Không thể tạo ca dạy. Vui lòng thử lại!';
        this.showToast(msg, 'warning');
      }
    });
  }

  quickAddSession(dateStr: string): void {
    this.openAddSessionModal(dateStr);
  }

  goToClassAttendance(session: ScheduleSessionItem): void {
    this.router.navigate(['/classes'], { queryParams: { class: session.className } });
  }

  openAttendanceModal(session: ScheduleSessionItem): void {
    this.activeSession = session;
    this.activeLessonTopic = session.lessonContent || '';
    this.activeAttendanceStatus = 'present';
    this.activeHw = session.avgHomeworkScore !== null && session.avgHomeworkScore !== undefined ? `${session.avgHomeworkScore}%` : '100%';
    this.activeAttitude = 'Tốt';
    this.activeNote = '';
    this.isAttendanceModalOpen = true;

    // Load full details from DB
    this.sessionService.getSessionAttendance(session.id).subscribe({
      next: (res) => {
        if (res?.succeeded && res.result) {
          const detail = res.result;
          this.activeLessonTopic = detail.lessonContent || this.activeLessonTopic;
          if (detail.students && detail.students.length > 0) {
            const first = detail.students[0];
            const norm = (first.attendanceStatus || '').toLowerCase();
            if (norm.includes('present') || norm.includes('có mặt')) this.activeAttendanceStatus = 'present';
            else if (norm.includes('excused') || norm.includes('phép')) this.activeAttendanceStatus = 'excused';
            else this.activeAttendanceStatus = 'unexcused';

            if (first.homeworkScore !== undefined && first.homeworkScore !== null) {
              this.activeHw = `${first.homeworkScore}%`;
            }
            if (first.attitude) this.activeAttitude = first.attitude;
            if (first.individualNote) this.activeNote = first.individualNote;
          }
        }
      }
    });
  }

  closeAttendanceModal(): void {
    this.isAttendanceModalOpen = false;
    this.activeSession = null;
  }

  saveAttendanceLog(): void {
    if (!this.activeSession) return;

    this.isSavingLog = true;
    const session = this.activeSession;

    // Prepare student attendance payload
    let hw = 0;
    if (this.activeHw !== '--') {
      const num = parseInt(this.activeHw.replace('%', ''), 10);
      hw = isNaN(num) ? 0 : num;
    }

    let statusStr = 'Present';
    if (this.activeAttendanceStatus === 'excused') statusStr = 'Excused';
    else if (this.activeAttendanceStatus === 'unexcused') statusStr = 'Absent';

    // First fetch session students list if needed, or save via bulk attendance
    this.sessionService.getSessionAttendance(session.id).subscribe({
      next: (res) => {
        const studentId = (res?.succeeded && res.result?.students?.[0]?.studentId) ? res.result.students[0].studentId : 1;
        const attendances: StudentAttendanceUpdateItem[] = [{
          studentId: studentId,
          attendanceStatus: statusStr,
          homeworkScore: hw,
          attitude: this.activeAttitude,
          individualNote: this.activeNote
        }];

        this.sessionService.saveBulkAttendance(session.id, { attendances }).subscribe({
          next: () => {
            // Also update session lesson content if modified
            const editReq: SessionRequest = {
              id: session.id,
              classId: session.classId,
              sessionDate: session.sessionDate,
              startTime: session.startTime + (session.startTime.length === 5 ? ':00' : ''),
              endTime: session.endTime + (session.endTime.length === 5 ? ':00' : ''),
              lessonContent: this.activeLessonTopic,
              status: 'Completed'
            };
            this.sessionService.editSession(editReq).subscribe({
              next: () => {
                this.isSavingLog = false;
                this.closeAttendanceModal();
                this.showToast('✓ Đã lưu nhật ký ca dạy và cập nhật trạng thái hoàn thành!', 'success');
                this.loadSchedule();
              },
              error: () => {
                this.isSavingLog = false;
                this.closeAttendanceModal();
                this.showToast('✓ Đã lưu nhật ký ca dạy thành công!', 'success');
                this.loadSchedule();
              }
            });
          },
          error: (err) => {
            this.isSavingLog = false;
            const msg = err?.error?.message || 'Không thể lưu nhật ký. Vui lòng thử lại!';
            this.showToast(msg, 'warning');
          }
        });
      },
      error: () => {
        this.isSavingLog = false;
        this.showToast('Có lỗi khi lưu thông tin ca học.', 'warning');
      }
    });
  }

  deleteActiveSession(): void {
    if (!this.activeSession) return;
    if (!confirm('Bạn có chắc chắn muốn xóa ca dạy này khỏi lịch không?')) return;

    this.isSavingLog = true;
    this.sessionService.deleteSession(this.activeSession.id).subscribe({
      next: (res) => {
        this.isSavingLog = false;
        this.closeAttendanceModal();
        this.showToast('✓ Đã xóa ca dạy thành công!', 'success');
        this.loadSchedule();
      },
      error: (err) => {
        this.isSavingLog = false;
        const msg = err?.error?.message || 'Không thể xóa ca dạy.';
        this.showToast(msg, 'warning');
      }
    });
  }

  showToast(message: string, type: 'success' | 'info' | 'warning' = 'info'): void {
    this.toastMessage = message;
    this.toastType = type;
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toastTimer = setTimeout(() => {
      this.toastMessage = null;
    }, 4000);
  }

  closeToast(): void {
    this.toastMessage = null;
  }
}
