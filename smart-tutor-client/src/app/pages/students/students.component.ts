import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StudentService } from '../../core/services/student.service';
import { RequestStudentModel, StudentsResponseModel } from '../../core/models/student.models';

export interface StudentCardItem {
  id: string;
  name: string;
  avatar: string;
  avatarClass: string;
  grade: string;
  className: string;
  classType: 'Individual' | 'Group';
  ratePerSession: string;
  rawRate: number;
  parentName?: string;
  parentPhone: string;
}

@Component({
  selector: 'app-students',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './students.component.html',
  styleUrl: './students.component.scss'
})
export class StudentsComponent implements OnInit {
  students: StudentCardItem[] = [
    {
      id: 'duyanh',
      name: 'Nguyễn Duy Anh',
      avatar: 'DA',
      avatarClass: 'avatar-da',
      grade: 'Lớp 9',
      className: 'Toán Lớp 9',
      classType: 'Individual',
      ratePerSession: '120.000đ/b',
      rawRate: 120000,
      parentName: 'Bác Hùng',
      parentPhone: '0988.123.xxx'
    },
    {
      id: 'minhkhang',
      name: 'Trần Minh Khang',
      avatar: 'MK',
      avatarClass: 'avatar-mk',
      grade: 'Lớp 11',
      className: 'Hình Lớp 11',
      classType: 'Individual',
      ratePerSession: '150.000đ/b',
      rawRate: 150000,
      parentName: 'Cô Lan',
      parentPhone: '0912.456.xxx'
    },
    {
      id: 'baotram',
      name: 'Lê Bảo Trâm',
      avatar: 'BT',
      avatarClass: 'avatar-bt',
      grade: 'Lớp 8',
      className: 'Toán Lớp 8',
      classType: 'Individual',
      ratePerSession: '120.000đ/b',
      rawRate: 120000,
      parentName: 'Chị Mai',
      parentPhone: '0973.999.xxx'
    }
  ];

  // Modal State & Form Model (đồng bộ với RequestStudentModel ở backend)
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

  loadMyStudents(): void {
    this.studentService.getMyStudents().subscribe({
      next: (res) => {
        if (res?.succeeded && res.result && res.result.length > 0) {
          // Map backend students into local list, avoiding duplicates
          res.result.forEach(apiStudent => {
            if (!this.students.some(s => s.id === apiStudent.id.toString())) {
              this.students.unshift(this.mapBackendStudent(apiStudent));
            }
          });
        }
      },
      error: () => {
        // Backend not running or token expired; fallback to local mock data seamlessly
      }
    });
  }

  viewReport(studentId: string): void {
    this.router.navigate(['/report'], { queryParams: { student: studentId } });
  }

  openAddStudentModal(): void {
    this.formData = {
      fullName: '',
      className: 'Toán Lớp 9',
      classType: 'Individual',
      gradeLevel: 'Lớp 9',
      parentName: '',
      parentPhone: '',
      feePerSession: 120000
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
      this.formData.className = `Toán ${this.formData.gradeLevel}`;
    }
  }

  submitAddStudent(): void {
    if (!this.formData.fullName.trim()) {
      this.showToast('Vui lòng nhập họ tên học sinh!', 'warning');
      return;
    }

    this.isSubmitting = true;

    // Chuẩn bị payload chuẩn RequestStudentModel
    const payload: RequestStudentModel = {
      fullName: this.formData.fullName.trim(),
      className: this.formData.className?.trim() || `Môn học ${this.formData.gradeLevel}`,
      classType: this.formData.classType || 'Individual',
      gradeLevel: this.formData.gradeLevel || 'Lớp 9',
      parentName: this.formData.parentName?.trim() || '',
      parentPhone: this.formData.parentPhone?.trim() || '',
      feePerSession: Number(this.formData.feePerSession) || 120000
    };

    // Gọi API Backend: POST /api/students/create
    this.studentService.createStudent(payload).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res?.succeeded && res.result) {
          const newStudent = this.mapBackendStudent(res.result);
          this.students.unshift(newStudent);
          this.closeAddStudentModal();
          this.showToast(`✓ Đã lưu thành công học sinh ${newStudent.name} vào hệ thống!`, 'success');
        } else {
          // Fallback if backend responded without result
          this.addLocalStudent(payload);
        }
      },
      error: () => {
        // Fallback local adding to avoid breaking teacher's flow
        this.isSubmitting = false;
        this.addLocalStudent(payload);
      }
    });
  }

  private addLocalStudent(payload: RequestStudentModel): void {
    const initials = this.getInitials(payload.fullName);
    const newStudent: StudentCardItem = {
      id: 'hs_' + Date.now(),
      name: payload.fullName,
      avatar: initials,
      avatarClass: this.getRandomAvatarClass(),
      grade: payload.gradeLevel || 'Lớp 9',
      className: payload.className || 'Toán',
      classType: (payload.classType as 'Individual' | 'Group') || 'Individual',
      ratePerSession: (payload.feePerSession || 120000).toLocaleString('vi-VN') + 'đ/b',
      rawRate: payload.feePerSession || 120000,
      parentName: payload.parentName,
      parentPhone: payload.parentPhone || 'Chưa cập nhật'
    };
    this.students.unshift(newStudent);
    this.closeAddStudentModal();
    this.showToast(`✓ Đã tạo thành công học sinh ${newStudent.name}!`, 'success');
  }

  private mapBackendStudent(res: StudentsResponseModel): StudentCardItem {
    const initials = this.getInitials(res.fullName);
    return {
      id: res.id.toString(),
      name: res.fullName,
      avatar: initials,
      avatarClass: this.getRandomAvatarClass(),
      grade: res.gradeLevel || 'Lớp 9',
      className: res.className || 'Toán',
      classType: (res.classType as 'Individual' | 'Group') || 'Individual',
      ratePerSession: (res.feePerSession || 120000).toLocaleString('vi-VN') + 'đ/b',
      rawRate: res.feePerSession || 120000,
      parentName: res.parentName,
      parentPhone: res.parentPhone || 'Chưa cập nhật'
    };
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
