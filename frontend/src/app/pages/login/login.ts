import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private http = inject(HttpClient);
  private router = inject(Router);

  email = '';
  password = '';
  errorMessage = '';
  isLoading = false;

  login() {
    this.errorMessage = '';
    this.isLoading = true;

    const loginData = {
      email: this.email,
      password: this.password
    };

    this.http
      .post<any>('http://localhost:5040/api/users/login', loginData)
      .subscribe({
        next: (response) => {
          localStorage.setItem('token', response.token);

          this.isLoading = false;

          this.router.navigate(['/events']);
        },
        error: (error) => {
          this.isLoading = false;

          if (error.status === 401) {
            this.errorMessage = 'E-posta veya şifre hatalı.';
          } else {
            this.errorMessage = 'Giriş yapılırken bir hata oluştu.';
          }
        }
      });
  }
}