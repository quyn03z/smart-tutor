import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-teacher-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './teacher-layout.component.html',
  styleUrl: './teacher-layout.component.scss'
})
export class TeacherLayoutComponent implements OnInit {
  teacherName = '';
  teacherBank = '';
  teacherInitials = '';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.getCurrentUser().subscribe({
    next: (res) => {
      if (res?.succeeded && res.result) {
        this.teacherName = res.result.fullName;
        this.teacherBank = `${res.result.bankCode}: ${res.result.bankAccountNumber}`;
        
        // Lấy 2 chữ cái đầu viết tắt cho Avatar (Ví dụ: Đình Quyền -> DQ)
        const parts = this.teacherName.trim().split(' ');
        this.teacherInitials = parts.length > 1 
          ? (parts[parts.length - 2][0] + parts[parts.length - 1][0]).toUpperCase()
          : this.teacherName.slice(0, 2).toUpperCase();
      }
    },
    error: (err) => {
      console.error('Không thể lấy thông tin người dùng hiện tại:', err);
    }
  });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
