import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

export const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('newPassword');
  const confirmPassword = control.get('confirmNewPassword');

  if (!password || !confirmPassword) {
    return null;
  }

  return password.value === confirmPassword.value ? null : { passwordMismatch: true };
};

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm!: FormGroup;
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;
  token = '';
  email = '';
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParams['token'] || '';
    this.email = this.route.snapshot.queryParams['email'] || '';

    if (!this.token || !this.email) {
      this.errorMessage = 'Đường dẫn đặt lại mật khẩu không hợp lệ hoặc thiếu thông tin xác thực.';
    }

    this.resetPasswordForm = this.fb.group({
      newPassword: [
        '',
        [
          Validators.required,
          Validators.minLength(6),
          Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$/)
        ]
      ],
      confirmNewPassword: ['', [Validators.required]]
    }, { validators: passwordMatchValidator });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit(): void {
    if (this.resetPasswordForm.invalid) {
      this.resetPasswordForm.markAllAsTouched();
      return;
    }

    if (!this.token || !this.email) {
      this.errorMessage = 'Liên kết không hợp lệ. Vui lòng thực hiện lại từ email của bạn.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const { newPassword, confirmNewPassword } = this.resetPasswordForm.value;

    this.authService.resetPassword({
      email: this.email,
      resetToken: this.token,
      newPassword,
      confirmNewPassword
    }).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.succeeded) {
          this.successMessage = 'Đặt lại mật khẩu thành công! Đang chuyển hướng về trang Đăng nhập...';
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        } else {
          this.errorMessage = response.message || response.errors?.[0] || 'Đặt lại mật khẩu không thành công.';
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage =
          err.error?.message ||
          (err.error?.errors && err.error.errors[0]) ||
          'Mã xác thực đã hết hạn hoặc không hợp lệ. Vui lòng yêu cầu cấp lại mã mới.';
      }
    });
  }
}
