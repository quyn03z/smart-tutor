import { Routes } from '@angular/router';
import { LoginComponent } from './modules/account/login/login.component';
import { RegisterComponent } from './modules/account/register/register.component';
import { ForgotPasswordComponent } from './modules/account/forgot-password/forgot-password.component';
import { MainLayoutComponent } from './shared/layouts/main-layout/main-layout.component';
import { HomeComponent } from './pages/home/home.component';

export const routes: Routes = [
  // 1. Landing Page & Các trang dùng Main Layout (Header + Content + Footer)
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'home', component: HomeComponent }
    ]
  },

  // 2. Trang Xác thực (Login / Register / Forgot Password)
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },

  // 3. Fallback route
  { path: '**', redirectTo: '' }
];
