import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

export interface ClassStudentItem {
  stt: number;
  id: string;
  name: string;
  avatar: string;
  status: 'present' | 'excused' | 'unexcused';
  homework: string;
  attitude: string;
  note: string;
}

@Component({
  selector: 'app-classes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './classes.component.html',
  styleUrl: './classes.component.scss'
})
export class ClassesComponent {
  classLessonTopic = 'Chuyên đề: Giải bài toán bằng cách lập hệ phương trình (Dạng năng suất & chuyển động)';
  
  toastMessage: string | null = null;
  toastType: 'success' | 'info' | 'warning' = 'success';
  private toastTimer: any = null;

  classStudents: ClassStudentItem[] = [
    { stt: 1, id: 'hs1', name: 'Hoàng Yến', avatar: 'HY', status: 'present', homework: '100%', attitude: 'Tập trung, phát biểu nhiều', note: 'Hiểu bài tốt' },
    { stt: 2, id: 'hs2', name: 'Đức Anh', avatar: 'ĐA', status: 'present', homework: '100%', attitude: 'Tốt', note: 'Có tiến bộ' },
    { stt: 3, id: 'hs3', name: 'Minh Quân', avatar: 'MQ', status: 'present', homework: '80%', attitude: 'Khá', note: 'Cần nộp bài đúng hạn' },
    { stt: 4, id: 'hs4', name: 'Gia Hưng', avatar: 'GH', status: 'excused', homework: '--', attitude: 'Nghỉ phép', note: 'Phụ huynh xin nghỉ về quê' },
    { stt: 5, id: 'hs5', name: 'Thanh Thảo', avatar: 'TT', status: 'present', homework: '100%', attitude: 'Tốt', note: 'Rất chăm chỉ' },
    { stt: 6, id: 'hs6', name: 'Khánh Linh', avatar: 'KL', status: 'present', homework: '90%', attitude: 'Tốt', note: 'Nắm chắc kiến thức' }
  ];

  constructor(private router: Router) {}

  setAllAttendance(status: 'present' | 'excused' | 'unexcused'): void {
    this.classStudents.forEach(st => st.status = status);
    this.showToast('✓ Đánh dấu tất cả có mặt!', 'success');
  }

  setAllHW(rate: string): void {
    this.classStudents.forEach(st => {
      if (st.status === 'present') st.homework = rate;
    });
    this.showToast('✓ Đã cập nhật BTVN 100% cho cả lớp!', 'success');
  }

  saveClassAttendance(): void {
    this.showToast('✓ Đã lưu sổ điểm danh ca dạy! Hệ thống tự động nhân tiền 80.000đ/buổi cho từng học sinh.', 'success');
  }

  exportBatchReports(): void {
    this.showToast('📦 Đang xuất hàng loạt 12 phiếu thu PDF kèm mã VietQR cá nhân hóa cho cả Lớp Toán 9A!', 'success');
  }

  viewStudentCard(studentId: string): void {
    const reportStudentId = studentId === 'hs1' ? 'lop9a_hs1' : 'lop9a_hs2';
    this.router.navigate(['/report'], { queryParams: { student: reportStudentId } });
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
