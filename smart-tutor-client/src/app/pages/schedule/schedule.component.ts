import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './schedule.component.html',
  styleUrl: './schedule.component.scss'
})
export class ScheduleComponent {
  scheduleView: 'list' | 'grid' = 'list';

  // Add session modal
  isAddSessionModalOpen = false;
  newSessionDate = '18/08';
  newSessionTopic = '';
  newSessionHw = '100%';
  newSessionAttitude = 'Tốt';

  toastMessage: string | null = null;
  toastType: 'success' | 'info' | 'warning' = 'info';
  private toastTimer: any = null;

  constructor(private router: Router) {}

  toggleScheduleView(view: 'list' | 'grid'): void {
    this.scheduleView = view;
  }

  prevWeek(): void {
    this.showToast('Hiển thị lịch tuần trước (28/07 - 03/08/2026)', 'info');
  }

  currentWeek(): void {
    this.showToast('Hiển thị lịch tuần hiện tại (04/08 - 10/08/2026)', 'info');
  }

  nextWeek(): void {
    this.showToast('Hiển thị lịch tuần kế tiếp (11/08 - 17/08/2026)', 'info');
  }

  openAddSessionModal(): void {
    this.newSessionTopic = '';
    this.isAddSessionModalOpen = true;
  }

  closeAddSessionModal(): void {
    this.isAddSessionModalOpen = false;
  }

  submitAddSession(): void {
    if (!this.newSessionTopic.trim()) {
      this.showToast('Vui lòng nhập nội dung ca dạy!', 'warning');
      return;
    }
    this.closeAddSessionModal();
    this.showToast('✓ Đã thêm ca dạy mới vào lịch giảng dạy!', 'success');
  }

  quickAddSession(date: string): void {
    this.newSessionDate = date.split('-').slice(1).reverse().join('/');
    this.openAddSessionModal();
  }

  openAttendanceModal(studentName: string, time: string): void {
    this.showToast(`Đang mở nhật ký ca dạy của ${studentName} (${time})`, 'info');
  }

  goToClasses(): void {
    this.router.navigate(['/classes']);
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
