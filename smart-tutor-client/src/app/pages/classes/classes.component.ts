import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { StudentService } from '../../core/services/student.service';
import { SessionService, ClassSessionAttendanceRequest } from '../../core/services/session.service';
import { StudentsResponseModel } from '../../core/models/student.models';

export interface ClassStudentItem {
  stt: number;
  id: string;
  name: string;
  avatar: string;
  avatarClass?: string;
  status: 'present' | 'excused' | 'unexcused';
  homework: string;
  attitude: string;
  note: string;
  feePerSession: number;
}

export interface GroupClassInfo {
  id: string;
  classId?: number;
  className: string;
  gradeLevel: string;
  feePerSession: number;
  scheduleText: string;
  roomText: string;
  students: ClassStudentItem[];
}

@Component({
  selector: 'app-classes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './classes.component.html',
  styleUrl: './classes.component.scss'
})
export class ClassesComponent implements OnInit {
  isLoading = false;
  isSavingAttendance = false;
  isExportingReports = false;
  classes: GroupClassInfo[] = [];
  selectedClass: GroupClassInfo | null = null;

  openHwDropdownStudentId: string | null = null;
  hwOptions: string[] = ['100%', '90%', '80%', '50%', '0%', '--'];

  classLessonTopic = 'Chuyên đề: Giải bài toán bằng cách lập hệ phương trình (Dạng năng suất & chuyển động)';
  
  toastMessage: string | null = null;
  toastType: 'success' | 'info' | 'warning' = 'success';
  private toastTimer: any = null;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-hw-dropdown-container')) {
      this.openHwDropdownStudentId = null;
    }
  }

  toggleHwDropdown(studentId: string, event: MouseEvent): void {
    event.stopPropagation();
    if (this.openHwDropdownStudentId === studentId) {
      this.openHwDropdownStudentId = null;
    } else {
      this.openHwDropdownStudentId = studentId;
    }
  }

  selectHomework(student: ClassStudentItem, opt: string, event: MouseEvent): void {
    event.stopPropagation();
    student.homework = opt;
    this.openHwDropdownStudentId = null;
  }

  getHwClass(rate: string): string {
    switch (rate) {
      case '100%': return 'hw-100';
      case '90%': return 'hw-90';
      case '80%': return 'hw-80';
      case '50%': return 'hw-50';
      case '0%': return 'hw-0';
      default: return 'hw-none';
    }
  }

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private studentService: StudentService,
    private sessionService: SessionService
  ) {}

  ngOnInit(): void {
    this.loadClasses();
  }

  loadClasses(): void {
    this.isLoading = true;
    this.studentService.getMyStudents().subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res?.succeeded && res.result && res.result.length > 0) {
          // Chỉ lấy các học sinh thuộc hình thức Lớp nhóm (classType === 'Group')
          const groupStudents = res.result.filter(s => s.classType === 'Group');

          if (groupStudents.length === 0) {
            this.classes = [];
            this.selectedClass = null;
            return;
          }

          const map = new Map<string, StudentsResponseModel[]>();
          
          // Phân nhóm theo tên lớp
          groupStudents.forEach(s => {
            const classKey = (s.className || 'Lớp Nhóm').trim();
            if (!map.has(classKey)) {
              map.set(classKey, []);
            }
            map.get(classKey)!.push(s);
          });

          const classesList: GroupClassInfo[] = [];
          map.forEach((students, className) => {
            const first = students[0];
            classesList.push({
              id: className,
              classId: first.classId,
              className: className,
              gradeLevel: first.gradeLevel || 'Lớp 9',
              feePerSession: first.feePerSession || 80000,
              scheduleText: this.extractSchedule(className),
              roomText: 'Phòng học: Tầng 2 / Trực tiếp',
              students: students.map((st, idx) => ({
                stt: idx + 1,
                id: st.id,
                name: st.fullName,
                avatar: this.getInitials(st.fullName),
                avatarClass: this.getRandomAvatarClass(idx),
                status: 'unexcused',
                homework: '0%',
                attitude: 'Tốt',
                note: '',
                feePerSession: st.feePerSession || first.feePerSession || 80000
              }))
            });
          });

          this.classes = classesList;
          this.checkRouteSelection();
        } else {
          this.classes = [];
          this.selectedClass = null;
        }
      },
      error: () => {
        this.isLoading = false;
        this.classes = [];
        this.selectedClass = null;
      }
    });
  }

  private checkRouteSelection(): void {
    const classParam = this.route.snapshot.queryParamMap.get('class');
    if (classParam) {
      const match = this.classes.find(c => c.className.toLowerCase().includes(classParam.toLowerCase()) || c.id === classParam);
      if (match) {
        this.selectClass(match);
        return;
      }
    }

    if (this.selectedClass) {
      const current = this.classes.find(c => c.id === this.selectedClass!.id);
      if (current) {
        this.selectClass(current);
      }
    }
  }

  selectClass(cls: GroupClassInfo): void {
    this.selectedClass = cls;
    this.fetchClassAttendance(cls);
  }

  fetchClassAttendance(cls: GroupClassInfo): void {
    const todayStr = new Date().toISOString().split('T')[0];
    this.sessionService.getClassAttendance(cls.classId, cls.className, todayStr).subscribe({
      next: (res) => {
        if (res?.succeeded && res.result && res.result.hasRecorded && res.result.attendances?.length > 0) {
          const detail = res.result;
          if (detail.lessonContent) {
            this.classLessonTopic = detail.lessonContent;
          }

          const attMap = new Map<number, any>();
          detail.attendances.forEach(a => attMap.set(Number(a.studentId), a));

          cls.students.forEach(st => {
            const log = attMap.get(Number(st.id));
            if (log) {
              const normStatus = (log.attendanceStatus || '').toLowerCase();
              if (normStatus.includes('present') || normStatus.includes('có mặt')) {
                st.status = 'present';
              } else if (normStatus.includes('excused') || normStatus.includes('phép')) {
                st.status = 'excused';
              } else {
                st.status = 'unexcused';
              }

              const score = log.homeworkScore;
              if (score !== undefined && score !== null) {
                if (score === 100) st.homework = '100%';
                else if (score === 90) st.homework = '90%';
                else if (score === 80) st.homework = '80%';
                else if (score === 50) st.homework = '50%';
                else if (score === 0) st.homework = '0%';
                else st.homework = `${score}%`;
              }

              if (log.attitude) st.attitude = log.attitude;
              if (log.individualNote) st.note = log.individualNote;
            }
          });
        }
      },
      error: () => {
        // Giữ trạng thái mặc định (Vắng & 0%) nếu chưa điểm danh
      }
    });
  }

  backToClassList(): void {
    this.selectedClass = null;
  }

  get todayFormatted(): string {
    const now = new Date();
    const days = ['Chủ Nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];
    const dayName = days[now.getDay()];
    const dd = String(now.getDate()).padStart(2, '0');
    const mm = String(now.getMonth() + 1).padStart(2, '0');
    const yyyy = now.getFullYear();
    return `${dayName} (${dd}/${mm}/${yyyy})`;
  }

  get presentCount(): number {
    if (!this.selectedClass) return 0;
    return this.selectedClass.students.filter(s => s.status === 'present').length;
  }

  get excusedCount(): number {
    if (!this.selectedClass) return 0;
    return this.selectedClass.students.filter(s => s.status === 'excused').length;
  }

  getEstimatedRevenue(cls: GroupClassInfo): number {
    return cls.feePerSession * cls.students.length * 8;
  }

  setAllAttendance(status: 'present' | 'excused' | 'unexcused'): void {
    if (!this.selectedClass) return;
    this.selectedClass.students.forEach(st => st.status = status);
    this.showToast('✓ Đã đánh dấu tất cả có mặt!', 'success');
  }

  setAllHW(rate: string): void {
    if (!this.selectedClass) return;
    this.selectedClass.students.forEach(st => {
      if (st.status === 'present') st.homework = rate;
    });
    this.showToast('✓ Đã cập nhật BTVN 100% cho cả lớp!', 'success');
  }

  saveClassAttendance(): void {
    if (!this.selectedClass) return;

    this.isSavingAttendance = true;
    const todayStr = new Date().toISOString().split('T')[0];

    const payload: ClassSessionAttendanceRequest = {
      classId: this.selectedClass.classId,
      className: this.selectedClass.className,
      sessionDate: todayStr,
      startTime: '18:00:00',
      endTime: '20:00:00',
      lessonContent: this.classLessonTopic,
      attendances: this.selectedClass.students.map(s => {
        let status = 'Present';
        if (s.status === 'excused') status = 'Excused';
        else if (s.status === 'unexcused') status = 'Absent';

        let hw = 0;
        if (s.homework !== '--') {
          const num = parseInt(s.homework.replace('%', ''), 10);
          hw = isNaN(num) ? 0 : num;
        }

        return {
          studentId: Number(s.id),
          attendanceStatus: status,
          homeworkScore: hw,
          attitude: s.attitude || 'Tốt',
          individualNote: s.note || ''
        };
      })
    };

    this.sessionService.saveClassAttendance(payload).subscribe({
      next: (res) => {
        this.isSavingAttendance = false;
        if (res?.succeeded) {
          const totalFeeStr = (res.result?.totalFeeCalculated ?? (this.presentCount * this.selectedClass!.feePerSession)).toLocaleString('vi-VN');
          this.showToast(
            res.result?.message || `✓ Đã lưu sổ điểm danh ca dạy lớp ${this.selectedClass!.className}! Tự động tính ${totalFeeStr}đ học phí cho ${this.presentCount} học sinh có mặt.`,
            'success'
          );
        } else {
          this.showToast(res?.message || 'Không thể lưu điểm danh. Vui lòng thử lại!', 'warning');
        }
      },
      error: (err) => {
        this.isSavingAttendance = false;
        const msg = err?.error?.message || 'Có lỗi xảy ra khi lưu điểm danh ca dạy vào hệ thống.';
        this.showToast(msg, 'warning');
      }
    });
  }

  exportBatchReports(): void {
    if (!this.selectedClass) return;

    const classId = this.selectedClass.classId;
    if (!classId) {
      this.showToast(`Đang xuất phiếu thu PDF kèm mã VietQR cho cả lớp ${this.selectedClass.className}!`, 'success');
      return;
    }

    this.isExportingReports = true;
    const now = new Date();
    const currentMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}`;

    this.sessionService.generateMonthlyReports({
      classId: classId,
      reportMonth: currentMonth
    }).subscribe({
      next: (res) => {
        this.isExportingReports = false;
        if (res?.succeeded) {
          const count = res.result?.length || this.selectedClass!.students.length;
          this.showToast(`📦 Đã tạo thành công ${count} phiếu thu học phí tháng ${currentMonth} kèm mã VietQR cho ${this.selectedClass!.className}!`, 'success');
          setTimeout(() => {
            this.router.navigate(['/report'], { queryParams: { classId: classId, month: currentMonth } });
          }, 1200);
        } else {
          this.showToast(res?.message || 'Có lỗi khi xuất phiếu thu cả lớp', 'warning');
        }
      },
      error: (err) => {
        this.isExportingReports = false;
        const msg = err?.error?.message || 'Không thể xuất phiếu thu cả lớp. Vui lòng thử lại sau!';
        this.showToast(msg, 'warning');
      }
    });
  }

  viewStudentCard(studentId: string): void {
    this.router.navigate(['/report'], { queryParams: { student: studentId } });
  }

  private extractSchedule(name: string): string {
    const match = name.match(/\((.*?)\)/);
    if (match && match[1]) {
      return `Lịch học cố định: ${match[1].trim()}`;
    }
    return 'Lịch học cố định: Thứ 3 & Thứ 5 (18:00 – 20:00)';
  }

  private getInitials(name: string): string {
    if (!name) return 'HS';
    const clean = name.replace(/[^a-zA-Z0-9À-ỹà-ỹ\s]/g, ' ').trim();
    const words = clean.split(/\s+/).filter(w => !!w);
    if (words.length === 0) return 'HS';
    if (words.length === 1) return words[0].slice(0, 2).toUpperCase();
    return (words[words.length - 2][0] + words[words.length - 1][0]).toUpperCase();
  }

  private getRandomAvatarClass(index: number): string {
    const classes = ['avatar-hy', 'avatar-da', 'avatar-mq', 'avatar-gh', 'avatar-tt', 'avatar-kl'];
    return classes[index % classes.length];
  }

  showToast(message: string, type: 'success' | 'info' | 'warning' = 'success'): void {
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

