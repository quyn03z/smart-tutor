import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent implements OnInit {
  forgotPasswordForm!: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService) { }

  ngOnInit(): void {
    this.forgotPasswordForm = new FormGroup({
      email: new FormControl('', [Validators.required, Validators.email])
    });
  }

  onSubmit(){
    if (this.forgotPasswordForm.invalid) {
      this.forgotPasswordForm.markAllAsTouched();
      return;
    }
    
    this.isLoading = true;
    this.errorMessage = '';
    const { email } = this.forgotPasswordForm.value;
    
    this.authService.forgotPassword(email).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.succeeded) {
          this.router.navigate(['/verify-code']);
        } else {
          this.errorMessage = response.message || response.errors?.[0] || 'Có lỗi xảy ra.';
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage =
          err.error?.message ||
          (err.error?.errors && err.error.errors[0]) ||
          (err.status === 0 ? 'Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại backend!' : 'Email không tồn tại.');
      }
    });
    

  }
  
}
