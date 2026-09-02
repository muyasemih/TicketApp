import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private http = inject(HttpClient);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  name = '';
  email = '';
  password = '';
  confirmPassword = '';
  isStudent = false;

  errorMessage = '';
  successMessage = '';
  isLoading = false;

  register() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.email || !this.password || !this.confirmPassword) {
      this.errorMessage = 'Lütfen tüm alanları doldurun.';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Şifreler eşleşmiyor.';
      return;
    }

    this.isLoading = true;

  const registerData = {
    name: this.name,
    email: this.email,
    password: this.password,
    isStudent: this.isStudent
  };

  this.http
    .post('http://localhost:5040/api/users', registerData)
      .subscribe({
        next: () => {
          this.isLoading = false;
          this.successMessage = 'Kayıt başarılı! Giriş sayfasına yönlendiriliyorsunuz.';

          this.cdr.detectChanges();

          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 1500);
        },
        error: (error) => {
          this.isLoading = false;

          if (error.status === 400 || error.status === 409) {
            this.errorMessage =
              error.error?.error ||
              error.error?.message ||
              'Bu e-posta adresi zaten kayıtlı olabilir.';
          } else {
            this.errorMessage = 'Kayıt sırasında bir hata oluştu.';
          }

          this.cdr.detectChanges();
        }
      });
  }
}