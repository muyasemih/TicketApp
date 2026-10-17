import { Component, ChangeDetectorRef, inject, OnInit, OnDestroy } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

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
export class Events implements OnInit, OnDestroy {
  private router = inject(Router);
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);

  events: Event[] = [];
  isLoading = true;
  errorMessage = '';
  searchTerm = '';

  private searchSubject = new Subject<string>();
  private searchSubscription?: Subscription;

  ngOnInit() {
    this.searchSubscription = this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.loadEvents();
      });

    this.loadEvents();
  }

  ngOnDestroy() {
    this.searchSubscription?.unsubscribe();
  }

  loadEvents() {
    this.isLoading = true;
    this.errorMessage = '';

    let params = new HttpParams();
    if (this.searchTerm.trim()) {
      params = params.set('search', this.searchTerm.trim());
    }

    this.http
      .get<Event[]>(`${environment.apiUrl}/events`, { params })
      .subscribe({
        next: (response) => {
          this.events = response;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('API Hatası:', error);
          this.errorMessage = 'Etkinlikler yüklenirken bir hata oluştu.';
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  onSearch(event: EventTarget | null) {
    const input = event as HTMLInputElement;
    this.searchTerm = input?.value ?? '';
    this.searchSubject.next(this.searchTerm);
  }

  clearSearch() {
    this.searchTerm = '';
    this.searchSubject.next('');
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}

