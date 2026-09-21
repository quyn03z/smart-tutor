import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  showPassword = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      emailOrUsername: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [true]
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const { emailOrUsername, password, rememberMe } = this.loginForm.value;

    // TODO: Tích hợp gọi Auth API backend khi sẵn sàng
    setTimeout(() => {
      this.isLoading = false;
      // Giả lập đăng nhập thành công
      console.log('Login payload:', { emailOrUsername, password, rememberMe });
      this.router.navigate(['/']);
    }, 1000);
  }

  fillDemo(accountType: 'teacher' | 'admin'): void {
    if (accountType === 'teacher') {
      this.loginForm.patchValue({
        emailOrUsername: 'thaydinhquyen@smarttutor.vn',
        password: 'Password123@'
      });
    }
  }
}
