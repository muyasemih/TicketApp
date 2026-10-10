import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

interface UserProfile {
  id: number;
  name: string;
  email: string;
  role: string;
  isStudent: boolean;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);

  name = signal('');
  email = signal('');
  role = signal('');
  isStudent = signal(false);

  currentPassword = '';
  newPassword = '';
  confirmPassword = '';

  isLoading = signal(true);
  isSaving = signal(false);
  successMessage = signal('');
  errorMessage = signal('');

  ngOnInit() {
    this.loadProfile();
  }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  loadProfile() {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.http.get<UserProfile>('http://localhost:5040/api/users/me', {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: (data) => {
        this.name.set(data.name || '');
        this.email.set(data.email || '');
        this.role.set(data.role || '');
        this.isStudent.set(!!data.isStudent);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.status === 401) {
          localStorage.removeItem('token');
          this.router.navigate(['/login']);
        } else {
          this.errorMessage.set('Profil bilgileri yüklenemedi.');
        }
      }
    });
  }

  updateProfile() {
    this.errorMessage.set('');
    this.successMessage.set('');

    const currentName = this.name().trim();
    const currentEmail = this.email().trim();

    if (!currentName || !currentEmail) {
      this.errorMessage.set('Ad ve E-posta alanları zorunludur.');
      return;
    }

    if (this.newPassword || this.currentPassword) {
      if (!this.currentPassword) {
        this.errorMessage.set('Şifre değiştirmek için mevcut şifrenizi girmelisiniz.');
        return;
      }
      if (this.newPassword.length < 6) {
        this.errorMessage.set('Yeni şifre en az 6 karakter olmalıdır.');
        return;
      }
      if (this.newPassword !== this.confirmPassword) {
        this.errorMessage.set('Yeni şifreler birbiriyle eşleşmiyor.');
        return;
      }
    }

    this.isSaving.set(true);

    const payload: any = {
      name: currentName,
      email: currentEmail
    };

    if (this.newPassword) {
      payload.currentPassword = this.currentPassword;
      payload.newPassword = this.newPassword;
    }

    this.http.put<UserProfile>('http://localhost:5040/api/users/me', payload, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: (updated) => {
        this.isSaving.set(false);
        this.name.set(updated.name);
        this.email.set(updated.email);
        this.currentPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
        this.successMessage.set('Profil bilgileriniz başarıyla güncellendi.');
      },
      error: (err) => {
        this.isSaving.set(false);
        if (err.error?.error) {
          this.errorMessage.set(err.error.error);
        } else {
          this.errorMessage.set('Güncelleme sırasında bir hata oluştu.');
        }
      }
    });
  }
}
