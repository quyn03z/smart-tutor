import { Routes } from '@angular/router';
import { LoginComponent } from './modules/account/login/login.component';
import { RegisterComponent } from './modules/account/register/register.component';
import { ForgotPasswordComponent } from './modules/account/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './modules/account/reset-password/reset-password.component';
import { MainLayoutComponent } from './shared/layouts/main-layout/main-layout.component';
import { HomeComponent } from './pages/home/home.component';

import { TeacherLayoutComponent } from './shared/layouts/teacher-layout/teacher-layout.component';
import { ReportComponent } from './pages/report/report.component';
import { ClassesComponent } from './pages/classes/classes.component';
import { ScheduleComponent } from './pages/schedule/schedule.component';
import { StudentsComponent } from './pages/students/students.component';

export const routes: Routes = [
  // 1. Phân hệ Giáo viên (Teacher Portal Layout with child routes)
  {
    path: '',
    component: TeacherLayoutComponent,
    children: [
      { path: 'report', component: ReportComponent },
      { path: 'classes', component: ClassesComponent },
      { path: 'schedule', component: ScheduleComponent },
      { path: 'students', component: StudentsComponent },
      { path: 'dashboard', redirectTo: 'report', pathMatch: 'full' }
    ]
  },

  // 2. Landing Page & Các trang dùng Main Layout (Header + Content + Footer)
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'home', component: HomeComponent }
    ]
  },

  // 3. Trang Xác thực (Login / Register / Forgot Password / Reset Password)
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },

  // 4. Fallback route
  { path: '**', redirectTo: '' }
];
