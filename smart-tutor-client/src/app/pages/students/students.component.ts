import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

export interface StudentCardItem {
  id: string;
  name: string;
  avatar: string;
  avatarClass: string;
  grade: string;
  ratePerSession: string;
  parentPhone: string;
}

@Component({
  selector: 'app-students',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './students.component.html',
  styleUrl: './students.component.scss'
})
export class StudentsComponent {
  students: StudentCardItem[] = [
    {
      id: 'duyanh',
      name: 'Nguyễn Duy Anh',
      avatar: 'DA',
      avatarClass: 'avatar-da',
      grade: 'Toán Lớp 9',
      ratePerSession: '120.000đ/b',
      parentPhone: '0988.123.xxx'
    },
    {
      id: 'minhkhang',
      name: 'Trần Minh Khang',
      avatar: 'MK',
      avatarClass: 'avatar-mk',
      grade: 'Hình Lớp 11',
      ratePerSession: '150.000đ/b',
      parentPhone: '0912.456.xxx'
    },
    {
      id: 'baotram',
      name: 'Lê Bảo Trâm',
      avatar: 'BT',
      avatarClass: 'avatar-bt',
      grade: 'Toán Lớp 8',
      ratePerSession: '120.000đ/b',
      parentPhone: '0973.999.xxx'
    }
  ];

  isAddStudentModalOpen = false;
  newStudentName = '';
  newStudentGrade = 'Toán Lớp 9';
  newStudentRate = 120000;
  newStudentPhone = '';

  toastMessage: string | null = null;
  toastType: 'success' | 'info' | 'warning' = 'info';
  private toastTimer: any = null;

  constructor(private router: Router) {}

  viewReport(studentId: string): void {
    this.router.navigate(['/report'], { queryParams: { student: studentId } });
  }

  openAddStudentModal(): void {
    this.newStudentName = '';
    this.newStudentPhone = '';
    this.isAddStudentModalOpen = true;
  }

  closeAddStudentModal(): void {
    this.isAddStudentModalOpen = false;
  }

  submitAddStudent(): void {
    if (!this.newStudentName.trim()) {
      this.showToast('Vui lòng nhập họ tên học sinh!', 'warning');
      return;
    }

    const initials = this.newStudentName
      .trim()
      .split(' ')
      .slice(-2)
      .map(w => w[0]?.toUpperCase())
      .join('');

    const newId = 'hs_' + Date.now();
    this.students.push({
      id: newId,
      name: this.newStudentName.trim(),
      avatar: initials || 'HS',
      avatarClass: 'avatar-da',
      grade: this.newStudentGrade,
      ratePerSession: this.newStudentRate.toLocaleString('vi-VN') + 'đ/b',
      parentPhone: this.newStudentPhone.trim() || '09xx.xxx.xxx'
    });

    this.closeAddStudentModal();
    this.showToast(`✓ Đã thêm thành công học sinh ${this.newStudentName.trim()}!`, 'success');
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
