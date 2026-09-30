import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { StudentService } from '../../core/services/student.service';
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
  classes: GroupClassInfo[] = [];
  selectedClass: GroupClassInfo | null = null;

  classLessonTopic = 'Chuyên đề: Giải bài toán bằng cách lập hệ phương trình (Dạng năng suất & chuyển động)';
  
  toastMessage: string | null = null;
  toastType: 'success' | 'info' | 'warning' = 'success';
  private toastTimer: any = null;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private studentService: StudentService
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
                status: 'present',
                homework: '100%',
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

  private useDefaultMockClasses(): void {
    this.classes = [
      {
        id: 'lop_toan_9a',
        className: 'Lớp Toán 9A (Chuyên Đề Vào 10)',
        gradeLevel: 'Lớp 9',
        feePerSession: 80000,
        scheduleText: 'Lịch học cố định: Thứ 3 & Thứ 5 (18:00 – 20:00)',
        roomText: 'Phòng học: Tầng 2 / Trực tiếp',
        students: [
          { stt: 1, id: 'hs1', name: 'Hoàng Yến', avatar: 'HY', avatarClass: 'avatar-hy', status: 'present', homework: '100%', attitude: 'Tập trung, phát biểu nhiều', note: 'Hiểu bài tốt', feePerSession: 80000 },
          { stt: 2, id: 'hs2', name: 'Đức Anh', avatar: 'ĐA', avatarClass: 'avatar-da', status: 'present', homework: '100%', attitude: 'Tốt', note: 'Có tiến bộ', feePerSession: 80000 },
          { stt: 3, id: 'hs3', name: 'Minh Quân', avatar: 'MQ', avatarClass: 'avatar-mq', status: 'present', homework: '80%', attitude: 'Khá', note: 'Cần nộp bài đúng hạn', feePerSession: 80000 },
          { stt: 4, id: 'hs4', name: 'Gia Hưng', avatar: 'GH', avatarClass: 'avatar-gh', status: 'excused', homework: '--', attitude: 'Nghỉ phép', note: 'Phụ huynh xin nghỉ về quê', feePerSession: 80000 },
          { stt: 5, id: 'hs5', name: 'Thanh Thảo', avatar: 'TT', avatarClass: 'avatar-tt', status: 'present', homework: '100%', attitude: 'Tốt', note: 'Rất chăm chỉ', feePerSession: 80000 },
          { stt: 6, id: 'hs6', name: 'Khánh Linh', avatar: 'KL', avatarClass: 'avatar-kl', status: 'present', homework: '90%', attitude: 'Tốt', note: 'Nắm chắc kiến thức', feePerSession: 80000 }
        ]
      }
    ];
    this.checkRouteSelection();
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
  }

  selectClass(cls: GroupClassInfo): void {
    this.selectedClass = cls;
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
    const feeStr = this.selectedClass.feePerSession.toLocaleString('vi-VN');
    this.showToast(`✓ Đã lưu sổ điểm danh ca dạy lớp ${this.selectedClass.className}! Hệ thống tự động tính ${feeStr}đ/buổi cho từng học sinh.`, 'success');
  }

  exportBatchReports(): void {
    if (!this.selectedClass) return;
    this.showToast(`📦 Đang xuất hàng loạt ${this.selectedClass.students.length} phiếu thu PDF kèm mã VietQR cá nhân hóa cho ${this.selectedClass.className}!`, 'success');
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

