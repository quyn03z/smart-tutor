import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StudentService } from '../../core/services/student.service';
import { RequestStudentModel, StudentsResponseModel } from '../../core/models/student.models';

export interface StudentCardItem {
  id: string;
  fullName: string;
  avatar: string;
  avatarClass: string;
  gradeLevel: string;
  className: string;
  classType: string;
  feePerSession: number;
  parentName?: string;
  parentPhone: string;
}

export interface DisplayCardItem {
  id: string;
  isGroup: boolean;
  title: string;
  avatar: string;
  avatarClass: string;
  gradeLevel: string;
  className?: string;
  feePerSession: number;
  parentName?: string;
  parentPhone?: string;
  students?: StudentCardItem[];
}

@Component({
  selector: 'app-students',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './students.component.html',
  styleUrl: './students.component.scss'
})
export class StudentsComponent implements OnInit {
  students: StudentCardItem[] = [];
  isLoading = false;

  // Modal Chi Tiết Lớp Nhóm
  selectedGroupClass: DisplayCardItem | null = null;
  isViewClassModalOpen = false;

  // Modal Thêm Học Sinh & Form Model (đồng bộ với RequestStudentModel ở backend)
  isAddStudentModalOpen = false;
  isSubmitting = false;

  formData: RequestStudentModel = {
    fullName: '',
    className: 'Toán Lớp 9',
    classType: 'Individual',
    gradeLevel: 'Lớp 9',
    parentName: '',
    parentPhone: '',
    feePerSession: 120000
  };

  // Các mức học phí gợi ý nhanh
  quickFeeOptions = [80000, 100000, 120000, 150000, 200000];

  // Danh sách khối lớp
  gradeLevels = [
    'Lớp 1', 'Lớp 2', 'Lớp 3', 'Lớp 4', 'Lớp 5',
    'Lớp 6', 'Lớp 7', 'Lớp 8', 'Lớp 9',
    'Lớp 10', 'Lớp 11', 'Lớp 12', 'Luyện thi Đại học'
  ];

  toastMessage: string | null = null;
  toastType: 'success' | 'info' | 'warning' = 'info';
  private toastTimer: any = null;

  constructor(
    private router: Router,
    private studentService: StudentService
  ) {}

  ngOnInit(): void {
    this.loadMyStudents();
  }

  // Danh sách thẻ hiển thị thống nhất trên giao diện
  get displayCards(): DisplayCardItem[] {
    const cards: DisplayCardItem[] = [];
    const groupMap = new Map<string, StudentCardItem[]>();
    const individualList: StudentCardItem[] = [];

    this.students.forEach(s => {
      if (s.classType === 'Group') {
        const key = (s.className || 'Lớp Nhóm').trim();
        if (!groupMap.has(key)) {
          groupMap.set(key, []);
        }
        groupMap.get(key)!.push(s);
      } else {
        individualList.push(s);
      }
    });

    // Thẻ Lớp nhóm
    groupMap.forEach((groupStudents, className) => {
      const first = groupStudents[0];
      cards.push({
        id: 'group_' + className,
        isGroup: true,
        title: className,
        avatar: this.getClassInitials(className),
        avatarClass: 'avatar-mk',
        gradeLevel: first.gradeLevel || 'Lớp 9',
        feePerSession: first.feePerSession || 80000,
        students: groupStudents
      });
    });

    // Thẻ Học sinh 1-1
    individualList.forEach(s => {
      cards.push({
        id: s.id,
        isGroup: false,
        title: s.fullName,
        avatar: s.avatar,
        avatarClass: s.avatarClass,
        gradeLevel: s.gradeLevel,
        className: s.className,
        feePerSession: s.feePerSession,
        parentName: s.parentName,
        parentPhone: s.parentPhone
      });
    });

    return cards;
  }

  loadMyStudents(): void {
    this.isLoading = true;
    this.studentService.getMyStudents().subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res?.succeeded && res.result) {
          this.students = res.result.map(apiStudent => this.mapBackendStudent(apiStudent));
          if (this.selectedGroupClass) {
            const updated = this.displayCards.find(c => c.id === this.selectedGroupClass?.id);
            if (updated) {
              this.selectedGroupClass = updated;
            }
          }
        } else {
          this.students = [];
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.students = [];
        console.error('Lỗi khi tải danh sách học sinh:', err);
        this.showToast('Không thể kết nối đến máy chủ để tải danh sách học sinh', 'warning');
      }
    });
  }

  viewReport(studentId: string): void {
    this.closeGroupClassModal();
    this.router.navigate(['/report'], { queryParams: { student: studentId } });
  }

  openGroupClassDetails(item: DisplayCardItem): void {
    this.selectedGroupClass = item;
    this.isViewClassModalOpen = true;
  }

  closeGroupClassModal(): void {
    this.isViewClassModalOpen = false;
    this.selectedGroupClass = null;
  }

  openAddStudentModal(defaultType: 'Individual' | 'Group' = 'Individual'): void {
    this.formData = {
      fullName: '',
      className: defaultType === 'Group' ? 'Lớp Toán Nhóm' : 'Toán Lớp 9',
      classType: defaultType,
      gradeLevel: 'Lớp 9',
      parentName: '',
      parentPhone: '',
      feePerSession: defaultType === 'Group' ? 80000 : 120000
    };
    this.isSubmitting = false;
    this.isAddStudentModalOpen = true;
  }

  closeAddStudentModal(): void {
    this.isAddStudentModalOpen = false;
  }

  selectClassType(type: 'Individual' | 'Group'): void {
    this.formData.classType = type;
    if (type === 'Group' && this.formData.feePerSession === 120000) {
      this.formData.feePerSession = 80000;
    }
  }

  setQuickFee(fee: number): void {
    this.formData.feePerSession = fee;
  }

  onGradeChange(): void {
    if (this.formData.gradeLevel) {
      if (this.formData.classType === 'Group') {
        this.formData.className = `Lớp Toán Nhóm ${this.formData.gradeLevel}`;
      } else {
        this.formData.className = `Toán ${this.formData.gradeLevel}`;
      }
    }
  }

  submitAddStudent(): void {
    if (!this.formData.fullName.trim()) {
      this.showToast('Vui lòng nhập họ tên học sinh!', 'warning');
      return;
    }

    this.isSubmitting = true;

    const payload: RequestStudentModel = {
      fullName: this.formData.fullName.trim(),
      className: this.formData.className?.trim() || `Môn học ${this.formData.gradeLevel}`,
      classType: this.formData.classType || 'Individual',
      gradeLevel: this.formData.gradeLevel || 'Lớp 9',
      parentName: this.formData.parentName?.trim() || '',
      parentPhone: this.formData.parentPhone?.trim() || '',
      feePerSession: Number(this.formData.feePerSession) || (this.formData.classType === 'Group' ? 80000 : 120000)
    };

    this.studentService.createStudent(payload).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res?.succeeded && res.result) {
          const newStudent = this.mapBackendStudent(res.result);
          this.students.unshift(newStudent);
          this.closeAddStudentModal();
          this.showToast(`✓ Đã lưu thành công học sinh ${newStudent.fullName} vào hệ thống!`, 'success');
        } else {
          this.showToast(res?.message || 'Có lỗi xảy ra khi tạo học sinh', 'warning');
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        const msg = err?.error?.message || 'Không thể lưu học sinh vào hệ thống. Vui lòng thử lại!';
        this.showToast(msg, 'warning');
      }
    });
  }

  private mapBackendStudent(res: StudentsResponseModel): StudentCardItem {
    const initials = this.getInitials(res.fullName);
    return {
      id: res.id ? res.id.toString() : '',
      fullName: res.fullName || 'Học sinh',
      avatar: initials,
      avatarClass: this.getRandomAvatarClass(),
      gradeLevel: res.gradeLevel || 'Lớp 9',
      className: res.className || 'Toán',
      classType: res.classType || 'Individual',
      feePerSession: res.feePerSession ?? (res.classType === 'Group' ? 80000 : 120000),
      parentName: res.parentName,
      parentPhone: res.parentPhone || 'Chưa cập nhật'
    };
  }

  private getClassInitials(name: string): string {
    if (!name) return 'LN';
    const words = name.trim().split(' ').filter(w => !!w);
    if (words.length === 1) return words[0].slice(0, 2).toUpperCase();
    return (words[0][0] + words[words.length - 1][0]).toUpperCase();
  }

  private getInitials(name: string): string {
    if (!name) return 'HS';
    const words = name.trim().split(' ').filter(w => !!w);
    if (words.length === 1) return words[0].slice(0, 2).toUpperCase();
    return (words[words.length - 2][0] + words[words.length - 1][0]).toUpperCase();
  }

  private getRandomAvatarClass(): string {
    const classes = ['avatar-da', 'avatar-mk', 'avatar-bt'];
    return classes[Math.floor(Math.random() * classes.length)];
  }

  showToast(message: string, type: 'success' | 'info' | 'warning' = 'info'): void {
    this.toastMessage = message;
    this.toastType = type;
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toastTimer = setTimeout(() => {
      this.toastMessage = null;
    }, 4500);
  }

  closeToast(): void {
    this.toastMessage = null;
  }
}
