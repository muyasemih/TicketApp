import { Component, ChangeDetectorRef, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { environment } from '../../../environments/environment';

interface VenueItem {
  id: number;
  name: string;
  blocks?: VenueBlock[];
}

interface VenueBlock {
  id: number;
  name: string;
  type: number;
  capacity: number;
}

interface BlockPrice {
  venueBlockId: number;
  blockName?: string;
  price: number | null;
}

interface EventItem {
  id: number;
  name: string;
  eventDate: string;
  venueId: number;
  venue?: {
    name: string;
  };
  blockPrices?: {
    id: number;
    venueBlockId: number;
    price: number;
  }[];
}

@Component({
  selector: 'app-admin',
  imports: [FormsModule, RouterLink, DatePipe],
  templateUrl: './admin.html',
  styleUrl: './admin.css'
})
export class Admin {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef);

  events: EventItem[] = [];
  venues: VenueItem[] = [];
  venueBlocks: VenueBlock[] = [];
  blockPrices: BlockPrice[] = [];

  name = '';
  date = '';
  venueId: number | null = null;

  editingId: number | null = null;

  isLoading = true;
  isLoadingVenues = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit() {
    this.loadVenues();
    this.loadEvents();
  }

  getHeaders() {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  loadVenues() {
    this.isLoadingVenues = true;
    this.http
      .get<VenueItem[]>(`${environment.apiUrl}/venues`)
      .subscribe({
        next: (response) => {
          this.venues = response ?? [];
          this.isLoadingVenues = false;

          // Eğer seçili salon yoksa ve en az 1 salon varsa ilkini seç
          if (!this.venueId && this.venues.length > 0) {
            this.venueId = this.venues[0].id;
            this.loadVenueBlocks();
          }

          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Salon listesi yüklenemedi:', error);
          this.isLoadingVenues = false;
          this.cdr.detectChanges();
        }
      });
  }

  loadVenueBlocks() {
    if (!this.venueId) {
      this.venueBlocks = [];
      this.blockPrices = [];
      return;
    }

    this.http
      .get<any>(`${environment.apiUrl}/venues/${this.venueId}`)
      .subscribe({
        next: (response) => {
          this.venueBlocks = response.blocks ?? [];
          this.blockPrices = this.venueBlocks.map(block => ({
            venueBlockId: block.id,
            blockName: block.name,
            price: null
          }));
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Salon blokları yüklenemedi:', error);
          this.venueBlocks = [];
          this.blockPrices = [];
          this.cdr.detectChanges();
        }
      });
  }

  loadEvents() {
    this.http
      .get<EventItem[]>(`${environment.apiUrl}/events`)
      .subscribe({
        next: (response) => {
          this.events = response;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Etkinlikler yüklenemedi:', error);
          this.errorMessage = 'Etkinlikler yüklenemedi.';
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  saveEvent() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.name || !this.date || !this.venueId) {
      this.errorMessage = 'Lütfen etkinlik adı, tarih ve salon seçimini eksiksiz yapın.';
      return;
    }

    if (this.blockPrices.length === 0) {
      this.errorMessage = 'Seçilen salonda fiyatlandırılabilir blok bulunamadı.';
      return;
    }

    if (this.blockPrices.some(block => block.price === null || block.price < 0)) {
      this.errorMessage = 'Lütfen tüm bilet fiyatlarını geçerli bir tutar olarak girin.';
      return;
    }

    this.isSaving = true;

    const blocks = this.blockPrices.map(block => ({
      venueBlockId: block.venueBlockId,
      price: Number(block.price)
    }));

    const eventData = {
      name: this.name,
      eventDate: this.date,
      venueId: Number(this.venueId),
      blocks: blocks
    };

    if (this.editingId === null) {
      this.http
        .post(`${environment.apiUrl}/events`, eventData, { headers: this.getHeaders() })
        .subscribe({
          next: () => {
            this.successMessage = 'Etkinlik başarıyla oluşturuldu.';
            this.clearForm();
            this.loadEvents();
          },
          error: (error) => {
            console.error('Etkinlik oluşturma hatası:', error);
            this.errorMessage = error.error?.error || 'Etkinlik oluşturulamadı.';
            this.isSaving = false;
            this.cdr.detectChanges();
          }
        });
    } else {
      this.http
        .put(`${environment.apiUrl}/events/${this.editingId}`, eventData, { headers: this.getHeaders() })
        .subscribe({
          next: () => {
            this.successMessage = 'Etkinlik başarıyla güncellendi.';
            this.clearForm();
            this.loadEvents();
          },
          error: (error) => {
            console.error('Etkinlik güncelleme hatası:', error);
            this.errorMessage = error.error?.error || 'Etkinlik güncellenemedi.';
            this.isSaving = false;
            this.cdr.detectChanges();
          }
        });
    }
  }

  editEvent(event: EventItem) {
    this.editingId = event.id;
    this.name = event.name;
    this.date = event.eventDate;
    this.venueId = event.venueId;

    this.errorMessage = '';
    this.successMessage = '';

    this.http
      .get<any>(`${environment.apiUrl}/venues/${event.venueId}`)
      .subscribe({
        next: (response) => {
          this.venueBlocks = response.blocks ?? [];
          this.blockPrices = this.venueBlocks.map(block => ({
            venueBlockId: block.id,
            blockName: block.name,
            price: event.blockPrices?.find(p => p.venueBlockId === block.id)?.price ?? null
          }));
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Salon blokları yüklenemedi:', error);
          this.cdr.detectChanges();
        }
      });
  }

  deleteEvent(id: number) {
    if (!confirm('Bu etkinliği silmek istediğinize emin misiniz?')) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';

    this.http
      .delete(`${environment.apiUrl}/events/${id}`, { headers: this.getHeaders() })
      .subscribe({
        next: () => {
          this.successMessage = 'Etkinlik başarıyla silindi.';
          this.loadEvents();
        },
        error: (error) => {
          console.error('Etkinlik silme hatası:', error);
          this.errorMessage = error.error?.error || 'Etkinlik silinemedi.';
          this.cdr.detectChanges();
        }
      });
  }

  clearForm() {
    this.name = '';
    this.date = '';
    this.editingId = null;
    this.isSaving = false;

    if (this.venues.length > 0) {
      this.venueId = this.venues[0].id;
      this.loadVenueBlocks();
    } else {
      this.venueId = null;
      this.venueBlocks = [];
      this.blockPrices = [];
    }

    this.cdr.detectChanges();
  }
}
