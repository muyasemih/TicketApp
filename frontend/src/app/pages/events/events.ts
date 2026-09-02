import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

interface Event {
  id: number;
  name: string;
  date: string;
  venueId: number;
}

@Component({
  selector: 'app-events',
  imports: [DatePipe, RouterLink],
  templateUrl: './events.html',
  styleUrl: './events.css'
})
export class Events {
  private router = inject(Router);
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);

  events: Event[] = [];
  isLoading = true;
  errorMessage = '';

  ngOnInit() {
    console.log('Events component çalıştı');
    this.loadEvents();
  }

loadEvents() {
  console.log('API isteği gönderiliyor...');

  this.http
    .get<Event[]>('http://localhost:5040/api/events')
    .subscribe({
      next: (response) => {
        console.log('API cevabı:', response);

        this.events = response;
        this.isLoading = false;

        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('API HATASI:', error);

        this.errorMessage = 'Etkinlikler yüklenirken bir hata oluştu.';
        this.isLoading = false;
      }
    });
};
logout() {
  localStorage.removeItem('token');
  this.router.navigate(['/login']);
}
}